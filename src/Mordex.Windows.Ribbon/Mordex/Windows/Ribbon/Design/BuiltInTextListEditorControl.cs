using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	internal class BuiltInTextListEditorControl : UserControl, IRibbonItemEditor
	{
		private const double IdColumnRatio = 0.2;

		private const double DefaultValueColumnRatio = 0.4;

		private BuiltInTextList _builtInTexts;

		private DataTable _dataTable;

		private DataGridView _dataGrid;

		private int _lastGridWidth;

		private object _editedObject;

		private static object s_selectedObjectChangedKey = new object();

		private static object s_refreshTreeKey = new object();

		private EventHandler _dataChangedHandler;

		private EventHandler _disableCancelHandler;

		object IRibbonItemEditor.SelectedObject
		{
			get
			{
				return _editedObject;
			}
			set
			{
				_editedObject = value;
			}
		}

		Image IRibbonItemEditor.ItemImage
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public event EventHandler DataChanged
		{
			add
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _dataChangedHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler remainingHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _dataChangedHandler, remainingHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		event EventHandler IRibbonItemEditor.SelectedObjectChanged
		{
			add
			{
				base.Events.AddHandler(s_selectedObjectChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_selectedObjectChangedKey, value);
			}
		}

		event EventHandler IRibbonItemEditor.RefreshTree
		{
			add
			{
				base.Events.AddHandler(s_refreshTreeKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_refreshTreeKey, value);
			}
		}

		public event EventHandler DisableCancel
		{
			add
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler combinedHandler = (EventHandler)Delegate.Combine(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, combinedHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
			remove
			{
				EventHandler eventHandler = _disableCancelHandler;
				EventHandler previousHandler;
				do
				{
					previousHandler = eventHandler;
					EventHandler remainingHandler = (EventHandler)Delegate.Remove(previousHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _disableCancelHandler, remainingHandler, previousHandler);
				}
				while ((object)eventHandler != previousHandler);
			}
		}

		public BuiltInTextListEditorControl(BuiltInTextList textList)
		{
			InitializeControls();
			_builtInTexts = textList;
			PopulateDataTable();
		}

		private void PopulateDataTable()
		{
			_dataTable = new DataTable("BuiltInTexts");
			DataColumn column = new DataColumn("IDValue", typeof(int));
			_dataTable.Columns.Add(column);
			DataColumn dataColumn = new DataColumn("ID", typeof(string));
			dataColumn.ReadOnly = true;
			_dataTable.Columns.Add(dataColumn);
			DataColumn defaultValueColumn = new DataColumn("DefaultValue", typeof(string));
			defaultValueColumn.ReadOnly = true;
			_dataTable.Columns.Add(defaultValueColumn);
			DataColumn proposedValueColumn = new DataColumn("ProposedValue", typeof(string));
			_dataTable.Columns.Add(proposedValueColumn);
			foreach (RibbonBuiltInText ribbonBuiltInText in Enum.GetValues(typeof(RibbonBuiltInText)))
			{
				DataRow dataRow = _dataTable.NewRow();
				dataRow[column] = (int)ribbonBuiltInText;
				dataRow[dataColumn] = ribbonBuiltInText.ToString();
				dataRow[defaultValueColumn] = _builtInTexts.GetDefaultText(ribbonBuiltInText);
				dataRow[proposedValueColumn] = _builtInTexts.GetCustomText(ribbonBuiltInText);
				_dataTable.Rows.Add(dataRow);
			}
			_dataTable.DefaultView.AllowNew = false;
			_dataTable.DefaultView.Sort = "ID";
			_dataGrid.DataSource = _dataTable;
			_dataGrid.Columns["IDValue"].Visible = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_dataTable?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_dataGrid = new DataGridView();
			((ISupportInitialize)_dataGrid).BeginInit();
			SuspendLayout();
			_dataGrid.BorderStyle = BorderStyle.None;
			_dataGrid.Dock = DockStyle.Fill;
			_dataGrid.Location = new Point(0, 0);
			_dataGrid.Name = "dataGrid1";
			_dataGrid.RowHeadersVisible = false;
			_dataGrid.Size = new Size(432, 360);
			_dataGrid.TabIndex = 0;
			_dataGrid.Resize += OnDataGridResize;
			base.Controls.Add(_dataGrid);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.Name = "BuiltInTextsPage";
			base.Size = new Size(432, 360);
			((ISupportInitialize)_dataGrid).EndInit();
			ResumeLayout(performLayout: false);
		}

		private void OnDataGridResize(object sender, EventArgs eventArgs)
		{
			DataGridViewColumn dataGridViewColumn = _dataGrid.Columns["ID"];
			DataGridViewColumn defaultValueColumn = _dataGrid.Columns["DefaultValue"];
			DataGridViewColumn proposedValueColumn = _dataGrid.Columns["ProposedValue"];
			if (dataGridViewColumn == null || defaultValueColumn == null || proposedValueColumn == null)
			{
				return;
			}
			int idColumnWidth;
			if (_lastGridWidth == 0)
			{
				idColumnWidth = (int)((double)_dataGrid.Width * IdColumnRatio);
			}
			else
			{
				float idWidthFraction = (float)dataGridViewColumn.Width / (float)_lastGridWidth;
				idColumnWidth = (int)((float)_dataGrid.Width * idWidthFraction);
			}
			dataGridViewColumn.Width = idColumnWidth;
			int defaultColumnWidth = (int)((double)_dataGrid.Width * DefaultValueColumnRatio);
			defaultValueColumn.Width = defaultColumnWidth;
			proposedValueColumn.Width = _dataGrid.Width - idColumnWidth - defaultColumnWidth - SystemInformation.VerticalScrollBarWidth;
			_lastGridWidth = _dataGrid.Width;
		}

		protected virtual void OnDataChanged(EventArgs eventArgs)
		{
			if (_dataChangedHandler != null)
			{
				_dataChangedHandler(this, eventArgs);
			}
		}

		bool IRibbonItemEditor.CommitChanges()
		{
			foreach (DataRow row in _dataTable.Rows)
			{
				RibbonBuiltInText textID = (RibbonBuiltInText)row["IDValue"];
				object proposedValue = row["ProposedValue"];
				if (proposedValue == null || proposedValue == DBNull.Value)
				{
					_builtInTexts[textID] = null;
				}
				else
				{
					_builtInTexts[textID] = (string)proposedValue;
				}
			}
			return true;
		}

		void IRibbonItemEditor.SetSite(ISite site)
		{
		}

		protected virtual void OnDisableCancel(EventArgs eventArgs)
		{
			if (_disableCancelHandler != null)
			{
				_disableCancelHandler(this, eventArgs);
			}
		}

		bool IRibbonItemEditor.IsOptionSupported(ToolbarOptions option)
		{
			return false;
		}
	}
}
