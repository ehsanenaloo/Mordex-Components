using Mordex.Windows.DataGrid;

namespace Mordex.Demo
{
	public sealed class OverviewPage : DemoPage
	{
		private readonly MxDataGrid _grid;

		public OverviewPage()
		{
			_grid = GridDemo.CreateGrid(DemoData.Employees());
			GridDemo.SetupEmployeeColumns(_grid);
			_grid.AllowEdit = InheritableBoolean.False;
			_grid.RowHeaders = InheritableBoolean.True;
			_grid.SelectionMode = SelectionMode.MultipleSelection;
			_grid.RootTable.SortKeys.Add("Name");

			OptionsPanel options = new OptionsPanel();
			options.AddToggle("Alternating row colors", true, on => _grid.AlternatingColors = on);
			options.AddToggle("Row headers", true, on => _grid.RowHeaders = on ? InheritableBoolean.True : InheritableBoolean.False);
			options.AddToggle("Row numbers in headers", false, on => _grid.RowHeaderContent = on ? RowHeaderContent.RowPosition : RowHeaderContent.Default);
			options.AddToggle("Filter row", false, on => _grid.FilterMode = on ? FilterMode.Automatic : FilterMode.None);
			options.AddToggle("Group by box", false, on => _grid.GroupByBoxVisible = on);
			options.AddToggle("Total row", false, on => _grid.TotalRow = on ? InheritableBoolean.True : InheritableBoolean.False);
			options.AddToggle("Select several rows", true, on => _grid.SelectionMode = on ? SelectionMode.MultipleSelection : SelectionMode.SingleSelection);
			options.AddToggle("Fit columns to window", true, on => _grid.ColumnAutoResize = on);
			options.AddToggle("Allow column drag", true, on => _grid.AllowColumnDrag = on);
			options.AddChoice("Grid lines", new[] { GridLines.Both, GridLines.Horizontal, GridLines.Vertical, GridLines.None }, GridLines.Both, value => _grid.GridLines = value);
			options.AddButton("Show column chooser", () => _grid.ShowFieldChooser(FindForm()));
			options.AddNote("Click a header to sort. Drag a header edge to resize it. Drag a header to move the column.");
			Compose(_grid, options);
		}

		public override void OnShown()
		{
			GridDemo.ScrollToTop(_grid);
		}

		public override string Title => "Overview";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "A sortable, resizable grid of 240 employees with formatted dates and money, star ratings and check boxes.";

		public override int Order => 10;
	}
}
