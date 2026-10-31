using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Mordex.Demo
{
	public sealed class CatalogEntry
	{
		public string Name;
		public Func<Control> Create;
		public Func<Control, string> Verify;
	}

	public static class ControlCatalog
	{
		public static DataTable CreateSampleTable()
		{
			DataTable table = new DataTable("People");
			table.Columns.Add("Id", typeof(int));
			table.Columns.Add("Name", typeof(string));
			table.Columns.Add("Born", typeof(DateTime));
			table.Rows.Add(1, "Ada", new DateTime(1815, 12, 10));
			table.Rows.Add(2, "Alan", new DateTime(1912, 6, 23));
			table.Rows.Add(3, "Grace", new DateTime(1906, 12, 9));
			return table;
		}

		public static IList<CatalogEntry> All()
		{
			return new List<CatalogEntry>
			{
				new CatalogEntry
				{
					Name = "MxDataGrid",
					Create = () =>
					{
						Mordex.Windows.DataGrid.MxDataGrid grid = new Mordex.Windows.DataGrid.MxDataGrid();
						grid.DataSource = CreateSampleTable();
						return grid;
					},
					Verify = c => { Mordex.Windows.DataGrid.MxDataGrid g = (Mordex.Windows.DataGrid.MxDataGrid)c; g.RetrieveStructure(); return g.RowCount == 3 ? null : "DataGrid row count is " + g.RowCount + ", cols=" + g.RootTable.Columns.Count; }
				},
				new CatalogEntry { Name = "MxRibbon", Create = () => new Mordex.Windows.Ribbon.MxRibbon() },
				new CatalogEntry { Name = "MxNavigationBar", Create = () => new Mordex.Windows.Navigation.MxNavigationBar() },
				new CatalogEntry { Name = "MxTaskPane", Create = () => new Mordex.Windows.TaskPane.MxTaskPane() },
				new CatalogEntry { Name = "MxDatePicker", Create = () => new Mordex.Windows.DatePicker.MxDatePicker() },
				new CatalogEntry { Name = "MxScheduler", Create = () => new Mordex.Windows.Scheduler.MxScheduler() },
				new CatalogEntry { Name = "MxTimeline", Create = () => new Mordex.Windows.Timeline.MxTimeline() },
				new CatalogEntry { Name = "MxTabControl", Create = () => new Mordex.Windows.UI.Tabs.MxTabControl() },
				new CatalogEntry { Name = "MxButton", Create = () => new Mordex.Windows.UI.Controls.MxButton { Text = "Button" } },
				new CatalogEntry { Name = "MxComboBox", Create = () => new Mordex.Windows.UI.Controls.MxComboBox() },
				new CatalogEntry { Name = "MxTextBox", Create = () => new Mordex.Windows.DataGrid.Editors.MxTextBox { Text = "Edit" } },
				new CatalogEntry { Name = "MxNumericBox", Create = () => new Mordex.Windows.DataGrid.Editors.MxNumericBox() }
			};
		}
	}
}
