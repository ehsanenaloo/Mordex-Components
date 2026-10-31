using System.Drawing;
using Mordex.Windows.DataGrid;

namespace Mordex.Demo
{
	/// <summary>Shared set-up for the data grid pages.</summary>
	internal static class GridDemo
	{
		public const string GroupName = "Data grid";

		public static MxDataGrid CreateGrid(object source, string member = null)
		{
			MxDataGrid grid = new MxDataGrid();
			grid.DataSource = source;
			if (member != null)
			{
				grid.DataMember = member;
			}
			grid.RetrieveStructure(true);
			grid.GroupByBoxVisible = false;
			grid.ColumnAutoResize = true;
			return grid;
		}

		/// <summary>The grid scrolls to its current row after a sort. Go back to the first row.</summary>
		public static void ScrollToTop(MxDataGrid grid)
		{
			grid.Row = 0;
			grid.VerticalScrollPosition = 0;
		}

		/// <summary>Sets captions, widths and formats for the columns of <see cref="DemoData.Employees"/>.</summary>
		public static void SetupEmployeeColumns(MxDataGrid grid)
		{
			GridColumnCollection columns = grid.RootTable.Columns;
			Configure(columns["Id"], "ID", 50, TextAlignment.Far);
			Configure(columns["Name"], "Name", 150);
			Configure(columns["Department"], "Department", 105);
			Configure(columns["Title"], "Job title", 125);
			Configure(columns["Country"], "Country", 90);
			Configure(columns["City"], "City", 95);
			Configure(columns["HireDate"], "Hired", 95, TextAlignment.Far);
			columns["HireDate"].FormatString = "d";
			Configure(columns["Salary"], "Salary", 95, TextAlignment.Far);
			columns["Salary"].FormatString = "C0";
			Configure(columns["Rating"], "Rating", 90);
			Configure(columns["Active"], "Active", 60, TextAlignment.Center);
			columns["Active"].ColumnType = ColumnType.CheckBox;
			AddStars(columns["Rating"]);
		}

		public static void Configure(GridColumn column, string caption, int width, TextAlignment alignment = TextAlignment.Empty)
		{
			column.Caption = caption;
			column.Width = width;
			if (alignment != TextAlignment.Empty)
			{
				column.TextAlignment = alignment;
				column.HeaderAlignment = alignment;
			}
		}

		/// <summary>Shows a 1 to 5 number as stars.</summary>
		public static void AddStars(GridColumn column)
		{
			column.EditType = EditType.DropDownList;
			column.HasValueList = true;
			for (int i = 1; i <= 5; i++)
			{
				column.ValueList.Add(i, new string('★', i) + new string('☆', 5 - i));
			}
			column.TextAlignment = TextAlignment.Near;
		}

		public static Color SoftRed => Color.FromArgb(255, 226, 226);
	}
}
