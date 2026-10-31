using System.Data;
using Mordex.Windows.DataGrid;

namespace Mordex.Demo
{
	public sealed class TreePage : DemoPage
	{
		private readonly MxDataGrid _grid;
		private readonly DataTable _chart;

		public TreePage()
		{
			_chart = DemoData.OrgChart();
			_grid = GridDemo.CreateGrid(_chart);
			_grid.AllowEdit = InheritableBoolean.False;

			GridTable table = _grid.RootTable;
			GridColumnCollection columns = table.Columns;
			table.HierarchicalMode = HierarchicalMode.SelfReferencing;
			table.SelfReferencingSettings.ChildDataMember = "ParentId";
			table.SelfReferencingSettings.ParentDataMember = "Id";
			table.SelfReferencingSettings.ExpandColumn = columns["Unit"];

			GridDemo.Configure(columns["Id"], "ID", 50, TextAlignment.Far);
			columns["Id"].Visible = false;
			columns["ParentId"].Visible = false;
			GridDemo.Configure(columns["Unit"], "Unit", 260);
			GridDemo.Configure(columns["Lead"], "Lead", 170);
			GridDemo.Configure(columns["Headcount"], "Headcount", 100, TextAlignment.Far);
			GridDemo.Configure(columns["Budget"], "Budget", 120, TextAlignment.Far);
			columns["Budget"].FormatString = "C0";
			columns["Headcount"].AggregateFunction = AggregateFunction.Sum;
			columns["Budget"].AggregateFunction = AggregateFunction.Sum;
			columns["Budget"].TotalFormatString = "C0";
			columns["Unit"].AggregateFunction = AggregateFunction.Count;
			columns["Unit"].TotalFormatString = "0 units";
			table.TotalRow = InheritableBoolean.True;
			_grid.TreeLines = true;
			_grid.ColumnAutoResize = true;

			OptionsPanel options = new OptionsPanel();
			options.AddToggle("Tree lines", true, on => _grid.TreeLines = on);
			options.AddToggle("Alternating row colors", true, on => _grid.AlternatingColors = on);
			options.AddToggle("Total row", true, on => table.TotalRow = on ? InheritableBoolean.True : InheritableBoolean.False);
			options.AddChoice("Totals add up", new[] { SelfReferencingAggregationMode.AllRows, SelfReferencingAggregationMode.RootRows }, SelfReferencingAggregationMode.AllRows, value => table.SelfReferencingSettings.AggregationMode = value);
			options.AddChoice("Expand box", new[] { ExpandBoxStyle.Classic, ExpandBoxStyle.Arrows }, ExpandBoxStyle.Classic, value => _grid.HierarchicalExpandBoxStyle = value);
			options.AddButton("Expand all", () => _grid.ExpandRecords());
			options.AddButton("Collapse all", () => _grid.CollapseRecords());
			options.AddNote("Each row points to its parent with a parent ID, so the grid builds the tree itself. Totals add up headcount and budget.");
			Compose(_grid, options);
		}

		public override void OnShown()
		{
			_grid.ExpandRecords();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_chart?.Dispose();
			}
			base.Dispose(disposing);
		}

		public override string Title => "Tree";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "A company tree built from one table with an ID and a parent ID, with headcount and budget totals.";

		public override int Order => 50;
	}
}
