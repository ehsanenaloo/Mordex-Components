using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Mordex.Data;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid.Export
{
	public class GridExporter : Component
	{
		private const int MaxSheetNameLength = 31;

		private MxDataGrid _dataGrid;

		private bool _includeCollapsedRows;

		private bool _includeChildTables;

		private bool _includeHeaders;

		private bool _includeFormatStyle;

		private ExportMode _exportMode;

		private string _sheetName;

		private int _lastReportedProgress;

		private ProgressChangedEventHandler _progressChanged;

		private int _totalRowCount;

		private bool _includeExcelProcessingInstruction = true;

		private IContainer _iContainer;

		[DefaultValue(null)]
		public MxDataGrid DataGrid
		{
			get
			{
				return _dataGrid;
			}
			set
			{
				_dataGrid = value;
			}
		}

		[DefaultValue("Sheet1")]
		public string SheetName
		{
			get
			{
				return _sheetName;
			}
			set
			{
				_sheetName = SanitizeSheetName(value);
			}
		}

		private static string SanitizeSheetName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return "Sheet1";
			}
			char[] invalid = { '[', ']', ':', '*', '?', '/', '\\' };
			string cleaned = string.Concat(name.Split(invalid));
			if (cleaned.Length > MaxSheetNameLength)
			{
				cleaned = cleaned.Substring(0, MaxSheetNameLength);
			}
			return cleaned.Length == 0 ? "Sheet1" : cleaned;
		}

		[DefaultValue(true)]
		public bool IncludeCollapsedRows
		{
			get
			{
				return _includeCollapsedRows;
			}
			set
			{
				_includeCollapsedRows = value;
			}
		}

		[DefaultValue(false)]
		public bool IncludeChildTables
		{
			get
			{
				return _includeChildTables;
			}
			set
			{
				_includeChildTables = value;
			}
		}

		[DefaultValue(true)]
		public bool IncludeHeaders
		{
			get
			{
				return _includeHeaders;
			}
			set
			{
				_includeHeaders = value;
			}
		}

		[DefaultValue(true)]
		public bool IncludeFormatStyle
		{
			get
			{
				return _includeFormatStyle;
			}
			set
			{
				_includeFormatStyle = value;
			}
		}

		[DefaultValue(ExportMode.AllRows)]
		[Description("Determines which rows are included in the document created by the GridExporter.")]
		public ExportMode ExportMode
		{
			get
			{
				return _exportMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ExportMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_exportMode = value;
			}
		}

		[DefaultValue(true)]
		public bool IncludeExcelProcessingInstruction
		{
			get
			{
				return _includeExcelProcessingInstruction;
			}
			set
			{
				_includeExcelProcessingInstruction = value;
			}
		}

		public event ProgressChangedEventHandler ProgressChanged
		{
			add
			{
				ProgressChangedEventHandler progressChangedEventHandler = _progressChanged;
				ProgressChangedEventHandler otherProgressChangedEventHandler;
				do
				{
					otherProgressChangedEventHandler = progressChangedEventHandler;
					ProgressChangedEventHandler otherValue = (ProgressChangedEventHandler)Delegate.Combine(otherProgressChangedEventHandler, value);
					progressChangedEventHandler = Interlocked.CompareExchange(ref _progressChanged, otherValue, otherProgressChangedEventHandler);
				}
				while ((object)progressChangedEventHandler != otherProgressChangedEventHandler);
			}
			remove
			{
				ProgressChangedEventHandler progressChangedEventHandler = _progressChanged;
				ProgressChangedEventHandler otherProgressChangedEventHandler;
				do
				{
					otherProgressChangedEventHandler = progressChangedEventHandler;
					ProgressChangedEventHandler otherValue = (ProgressChangedEventHandler)Delegate.Remove(otherProgressChangedEventHandler, value);
					progressChangedEventHandler = Interlocked.CompareExchange(ref _progressChanged, otherValue, otherProgressChangedEventHandler);
				}
				while ((object)progressChangedEventHandler != otherProgressChangedEventHandler);
			}
		}

		public GridExporter()
		{
			SetIContainer();
			SetDefaults();
		}

		public GridExporter(IContainer container)
		{
			container.Add(this);
			SetIContainer();
			SetDefaults();
		}

		private void SetDefaults()
		{
			_includeCollapsedRows = true;
			_includeChildTables = false;
			_includeHeaders = true;
			_includeFormatStyle = true;
			_exportMode = ExportMode.AllRows;
			_sheetName = "Sheet1";
		}

		/// <summary>Bytes of row XML kept in memory before the rows move to a temporary file.</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal int RowBufferMemoryLimit { get; set; } = ExportRowBuffer.MemoryLimit;

		public void Export(Stream stream)
		{
			Export(stream, CancellationToken.None);
		}

		/// <summary>
		/// Writes the grid to the stream as an Excel XML workbook. Call it on the thread that owns the grid. Rows are
		/// buffered (in memory, then in a temporary file for large exports) and nothing is written to the stream until
		/// every row is read. The stream is never closed.
		/// <para>
		/// If the token is cancelled or the export fails before writing starts, nothing is written to the stream.
		/// Once writing has started, the token is still checked before each block of rows. A cancel or a failure at that
		/// point stops the export and leaves an incomplete workbook in the stream. The exporter does not try to remove it,
		/// because a stream write cannot be undone in general. Discard the output when this method throws, or write to a
		/// temporary file and move it into place only after the method returns.
		/// </para>
		/// <para>
		/// The last <see cref="ProgressChanged"/> event (100 percent) is raised after the workbook is written and the
		/// stream is flushed. If that handler throws, the exception is passed to the caller, but the stream holds the
		/// whole workbook.
		/// </para>
		/// </summary>
		/// <exception cref="OperationCanceledException">The token was cancelled.</exception>
		public void Export(Stream stream, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(stream);
			if (!stream.CanWrite)
			{
				throw new ArgumentException("The stream must be writable.", nameof(stream));
			}
			cancellationToken.ThrowIfCancellationRequested();
			if (_dataGrid != null && _dataGrid.RootTable != null)
			{
				if (!_dataGrid.IsHandleCreated)
				{
					_dataGrid.CreateControl();
				}
				ProcessStream(stream, cancellationToken);
			}
		}

		private void ProcessStream(Stream stream, CancellationToken cancellationToken)
		{
			ExcelXmlBuilder excelXmlBuilder = new ExcelXmlBuilder();
			_dataGrid.UpdateData();
			_dataGrid.RebuildRows(true);
			_lastReportedProgress = 0;
			try
			{
				GridRow[] array = GetVisibleRootRows();
				_totalRowCount = array.Length;
				excelXmlBuilder.ProcessStream(stream, this, array, cancellationToken);
				ReportProgress(100);
			}
			finally
			{
				_lastReportedProgress = 0;
			}
		}

		internal void ReportProgress(int value)
		{
			if (_lastReportedProgress != value)
			{
				_lastReportedProgress = value;
				if (_progressChanged != null)
				{
					_progressChanged(this, new ProgressChangedEventArgs(_lastReportedProgress));
				}
			}
		}

		private GridRow[] GetVisibleRootRows()
		{
			GridDataSourceManager gridDataSourceManager = _dataGrid.GetRowSourceManager();
			if (gridDataSourceManager != null && gridDataSourceManager.RootRow != null)
			{
				MordexRowCollection rows = gridDataSourceManager.RootRow.Rows;
				List<GridRow> list = new List<GridRow>();
				double amount = rows.Count;
				for (int i = 0; i < rows.Count; i++)
				{
					if (rows[i].IsRowVisible())
					{
						list.Add(_dataGrid.GetGridRowForLogicalRow(rows[i]));
						double otherAmount = (double)i / amount * 0.2 * 100.0;
						ReportProgress((int)otherAmount);
					}
				}
				return list.ToArray();
			}
			return new GridRow[0];
		}

		internal void ReportRowProgress(int value)
		{
			int otherValue = (int)((double)value / (double)_totalRowCount * 0.8 * 100.0) + 20;
			ReportProgress(otherValue);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void SetIContainer()
		{
			_iContainer = new Container();
		}
	}
}
