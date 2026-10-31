using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Mordex.Windows.DataGrid;
using Mordex.Windows.DataGrid.Export;

namespace Mordex.Demo
{
	/// <summary>
	/// Every way the grid sends its rows out: the Excel workbook (SpreadsheetML) with its options, and the print document.
	/// The result is shown as text and as a table, so the effect of each option is visible without opening Excel.
	/// </summary>
	public sealed class ExportFormatsPage : DemoPage
	{
		private const string Staff = "Employees";
		private const string Sales = "Customers with orders";

		private readonly DataTable _employees;
		private readonly DataSet _sales;
		private readonly MxDataGrid _staffGrid;
		private readonly MxDataGrid _salesGrid;
		private readonly GridExporter _exporter;
		private readonly ListView _table = new ListView { Dock = DockStyle.Fill, View = System.Windows.Forms.View.Details, FullRowSelect = true, GridLines = true, BorderStyle = System.Windows.Forms.BorderStyle.None };
		private readonly TextBox _xml = PageParts.CodeBox("");
		private readonly Label _summary = PageKit.Value("Nothing exported yet");
		private readonly Panel _gridHost = new Panel { Dock = DockStyle.Fill };
		private byte[] _lastFile = Array.Empty<byte>();
		private bool _ready;

		public ExportFormatsPage()
		{
			_employees = DemoData.Employees();
			_staffGrid = GridDemo.CreateGrid(_employees);
			GridDemo.SetupEmployeeColumns(_staffGrid);
			_staffGrid.AllowEdit = InheritableBoolean.False;
			_staffGrid.SelectionMode = Mordex.Windows.DataGrid.SelectionMode.MultipleSelection;
			_staffGrid.RootTable.SortKeys.Add("Name");
			_staffGrid.RootTable.Columns["Salary"].AggregateFunction = AggregateFunction.Sum;
			_staffGrid.RootTable.Columns["Salary"].TotalFormatString = "C0";
			_staffGrid.TotalRow = InheritableBoolean.True;
			_staffGrid.TotalRowPosition = TotalRowPosition.BottomFixed;

			_sales = DemoData.Sales();
			_salesGrid = GridDemo.CreateGrid(_sales, "Customers");
			_salesGrid.AllowEdit = InheritableBoolean.False;
			_salesGrid.SelectionMode = Mordex.Windows.DataGrid.SelectionMode.MultipleSelection;
			_salesGrid.Visible = false;

			foreach (MxDataGrid grid in new[] { _staffGrid, _salesGrid })
			{
				grid.Dock = DockStyle.Fill;
				_gridHost.Controls.Add(grid);
			}
			_exporter = new GridExporter { DataGrid = _staffGrid, SheetName = "Employees" };

			TableLayoutPanel layout = PageKit.Grid(3, 52f, 48f);
			layout.Controls.Add(_gridHost, 0, 0);
			layout.SetColumnSpan(_gridHost, 3);
			Mordex.Windows.UI.Controls.MxGroupBox tableGroup = PageKit.Group("The file as a table");
			_summary.Dock = DockStyle.Bottom;
			tableGroup.Controls.Add(_table);
			tableGroup.Controls.Add(_summary);
			_table.BringToFront();
			Mordex.Windows.UI.Controls.MxGroupBox xmlGroup = PageKit.Group("The file as text");
			xmlGroup.Controls.Add(_xml);
			Mordex.Windows.UI.Controls.MxGroupBox codeGroup = PageParts.CodeGroup("Code", "var exporter = new GridExporter\n{\n    DataGrid = grid,\n    SheetName = \"Employees\",\n    ExportMode = ExportMode.SelectedRows,\n    IncludeHeaders = true,\n    IncludeFormatStyle = true,\n    IncludeChildTables = true\n};\nusing (var stream = File.Create(path))\n    exporter.Export(stream);");
			layout.Controls.Add(tableGroup, 0, 1);
			layout.Controls.Add(xmlGroup, 1, 1);
			layout.Controls.Add(codeGroup, 2, 1);

			OptionsPanel options = new OptionsPanel();
			options.AddCaption("Format");
			options.AddNote("Excel workbook (SpreadsheetML, .xml). The print document is on the Print and preview page.");
			options.AddChoice("Data", new[] { Staff, Sales }, Staff, name =>
			{
				_staffGrid.Visible = name == Staff;
				_salesGrid.Visible = name == Sales;
				_exporter.DataGrid = name == Staff ? _staffGrid : _salesGrid;
				_exporter.SheetName = name == Staff ? "Employees" : "Customers";
				Export();
			});
			options.AddChoice("Rows to export", new[] { ExportMode.AllRows, ExportMode.SelectedRows, ExportMode.CheckedRows }, ExportMode.AllRows, value =>
			{
				_exporter.ExportMode = value;
				Export();
			});
			options.AddToggle("Include column headers", true, on => { _exporter.IncludeHeaders = on; Export(); });
			options.AddToggle("Keep colors and fonts", true, on => { _exporter.IncludeFormatStyle = on; Export(); });
			options.AddToggle("Include collapsed rows", true, on => { _exporter.IncludeCollapsedRows = on; Export(); });
			options.AddToggle("Include child tables", true, on => { _exporter.IncludeChildTables = on; Export(); });
			options.AddToggle("Add the Excel processing instruction", true, on => { _exporter.IncludeExcelProcessingInstruction = on; Export(); });
			options.AddButton("Select all rows", () => { CurrentGrid.SelectAll(); Export(); });
			options.AddButton("Clear the selection", () => { CurrentGrid.SelectedItems.Clear(); Export(); });
			options.AddButton("Export again", Export);
			options.AddButton("Save the file...", Save);
			Compose(layout, options);
			_ready = true;
			Export();
		}

		private MxDataGrid CurrentGrid => _exporter.DataGrid;

		private void Export()
		{
			if (!_ready)
			{
				return;
			}
			Stopwatch watch = Stopwatch.StartNew();
			using (MemoryStream stream = new MemoryStream())
			{
				_exporter.Export(stream);
				_lastFile = stream.ToArray();
			}
			watch.Stop();
			string text = Encoding.UTF8.GetString(_lastFile);
			_xml.Text = text.Length > 12000 ? text.Substring(0, 12000) + "\r\n..." : text;
			int rows = FillTable();
			_summary.Text = rows + " rows in the file, " + (_lastFile.Length / 1024) + " KB, " + watch.ElapsedMilliseconds + " ms";
		}

		// Reads the first rows of the workbook back, the way a spreadsheet program would show them.
		private int FillTable()
		{
			_table.BeginUpdate();
			_table.Items.Clear();
			_table.Columns.Clear();
			int rowCount = 0;
			int shown = 0;
			using (MemoryStream stream = new MemoryStream(_lastFile))
			using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
			{
				ListViewItem current = null;
				int column = 0;
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Row")
					{
						rowCount++;
						column = 0;
						current = shown < 40 ? new ListViewItem() : null;
					}
					else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Data" && current != null)
					{
						string value = reader.ReadElementContentAsString();
						if (column == 0)
						{
							current.Text = value;
						}
						else
						{
							current.SubItems.Add(value);
						}
						column++;
					}
					else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "Row" && current != null)
					{
						while (_table.Columns.Count < column)
						{
							_table.Columns.Add("Column " + (_table.Columns.Count + 1), 90);
						}
						_table.Items.Add(current);
						current = null;
						shown++;
					}
				}
			}
			_table.EndUpdate();
			return rowCount;
		}

		private void Save()
		{
			using (SaveFileDialog dialog = new SaveFileDialog { Filter = "Excel XML workbook (*.xml)|*.xml", FileName = _exporter.SheetName + ".xml" })
			{
				if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
				{
					return;
				}
				try
				{
					File.WriteAllBytes(dialog.FileName, _lastFile);
					_summary.Text = "Saved " + dialog.FileName;
				}
				catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
				{
					MessageBox.Show(FindForm(), "The file could not be written: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_exporter.Dispose();
				_employees.Dispose();
				_sales.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnShown()
		{
			GridDemo.ScrollToTop(_staffGrid);
		}

		public override string Title => "Export formats and options";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "Export the grid as an Excel workbook, choose which rows and styles go in, and see the file as a table and as text.";

		public override int Order => 71;
	}
}
