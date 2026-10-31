using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Mordex.Windows.DataGrid;
using Mordex.Windows.DataGrid.Export;

namespace Mordex.Demo
{
	public sealed class ExportLayoutPage : DemoPage
	{
		private readonly MxDataGrid _grid;
		private readonly DataTable _employees;
		private readonly GridExporter _exporter;
		private readonly Label _status;
		private byte[] _defaultLayout;
		private byte[] _savedLayout;

		public ExportLayoutPage()
		{
			_employees = DemoData.Employees();
			_grid = GridDemo.CreateGrid(_employees);
			GridDemo.SetupEmployeeColumns(_grid);
			_grid.AllowEdit = InheritableBoolean.False;
			_grid.GroupByBoxVisible = true;
			_grid.SelectionMode = Mordex.Windows.DataGrid.SelectionMode.MultipleSelection;
			_grid.RootTable.SortKeys.Add("Name");
			_grid.RootTable.Columns["Salary"].AggregateFunction = AggregateFunction.Sum;
			_grid.RootTable.Columns["Salary"].TotalFormatString = "C0";
			_grid.TotalRow = InheritableBoolean.True;
			_grid.TotalRowPosition = TotalRowPosition.BottomFixed;
			_exporter = new GridExporter { DataGrid = _grid, SheetName = "Employees" };
			_defaultLayout = CaptureLayout();

			OptionsPanel options = new OptionsPanel();
			options.AddCaption("Excel export");
			options.AddChoice("Rows to export", new[] { ExportMode.AllRows, ExportMode.SelectedRows }, ExportMode.AllRows, value => _exporter.ExportMode = value);
			options.AddToggle("Include column headers", true, on => _exporter.IncludeHeaders = on);
			options.AddToggle("Keep colors and fonts", true, on => _exporter.IncludeFormatStyle = on);
			options.AddButton("Export to Excel", ExportToFile);
			options.AddCaption("Layout");
			options.AddButton("Save layout", () =>
			{
				_savedLayout = CaptureLayout();
				_status.Text = "Layout saved at " + DateTime.Now.ToLongTimeString() + ".";
			});
			options.AddButton("Restore saved layout", () => Restore(_savedLayout, "Saved layout restored.", "Nothing saved yet. Change the grid and press Save layout first."));
			options.AddButton("Back to the default layout", () => Restore(_defaultLayout, "Default layout restored.", null));
			_status = options.AddNote("Move, resize, sort or group columns, save the layout, change it again and then restore it.");
			Compose(_grid, options);
		}

		private byte[] CaptureLayout()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				_grid.SaveLayoutFile(stream);
				return stream.ToArray();
			}
		}

		private void Restore(byte[] layout, string done, string missing)
		{
			if (layout == null)
			{
				_status.Text = missing;
				return;
			}
			using (MemoryStream stream = new MemoryStream(layout))
			{
				_grid.LoadLayoutFile(stream);
			}
			_status.Text = done;
		}

		private void ExportToFile()
		{
			// A private folder of the user and a name that cannot be guessed, so no other process can place a file or a link there first.
			string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mordex", "Exports");
			string path = Path.Combine(folder, "employees-" + Guid.NewGuid().ToString("N") + ".xml");
			try
			{
				Directory.CreateDirectory(folder);
				using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
				{
					_exporter.Export(stream);
				}
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				MessageBox.Show(FindForm(), "The file could not be written: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			_status.Text = "Exported to " + path;
			if (MessageBox.Show(FindForm(), "The grid was exported to:\n" + path + "\n\nOpen the file now?", "Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				Process.Start(new ProcessStartInfo(path) { UseShellExecute = true })?.Dispose();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_exporter?.Dispose();
				_employees?.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnShown()
		{
			GridDemo.ScrollToTop(_grid);
		}

		public override string Title => "Export and layouts";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "Send the grid to Excel, and save and restore the column order, widths, sort and groups.";

		public override int Order => 70;
	}
}
