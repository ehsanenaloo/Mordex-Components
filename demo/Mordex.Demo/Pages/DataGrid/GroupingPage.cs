using Mordex.Windows.DataGrid;

namespace Mordex.Demo
{
	public sealed class GroupingPage : DemoPage
	{
		private readonly MxDataGrid _grid;

		public GroupingPage()
		{
			_grid = GridDemo.CreateGrid(DemoData.Employees());
			GridDemo.SetupEmployeeColumns(_grid);
			_grid.AllowEdit = InheritableBoolean.False;
			_grid.GroupByBoxVisible = true;
			_grid.TotalRow = InheritableBoolean.True;
			_grid.GroupTotals = GroupTotals.ExpandedGroup;
			_grid.TotalRowPosition = TotalRowPosition.BottomFixed;
			_grid.HideColumnsWhenGrouped = InheritableBoolean.True;
			_grid.RootTable.AllowGroup = true;

			GridColumnCollection columns = _grid.RootTable.Columns;
			columns["Name"].AggregateFunction = AggregateFunction.Count;
			columns["Name"].TotalFormatString = "0 people";
			columns["Salary"].AggregateFunction = AggregateFunction.Sum;
			columns["Salary"].TotalFormatString = "C0";
			columns["Rating"].AggregateFunction = AggregateFunction.Average;
			columns["Rating"].TotalFormatString = "0.00 avg";
			columns["Id"].AllowGroup = false;

			GridGroupHeaderTotal salary = new GridGroupHeaderTotal(columns["Salary"], AggregateFunction.Sum)
			{
				TotalFormatString = "C0",
				TotalPrefix = "Payroll ",
				TotalSuffix = string.Empty
			};
			GridGroupHeaderTotal count = new GridGroupHeaderTotal(columns["Name"], AggregateFunction.Count)
			{
				TotalPrefix = "(",
				TotalSuffix = " people)"
			};
			_grid.RootTable.GroupHeaderTotals.Add(count);
			_grid.RootTable.GroupHeaderTotals.Add(salary);

			SetGrouping("Department");

			OptionsPanel options = new OptionsPanel();
			options.AddChoice("Group by", new[] { "Department", "Country", "Department, then country", "Job title", "None" }, "Department", SetGrouping);
			options.AddChoice("Group total rows", new[] { GroupTotals.ExpandedGroup, GroupTotals.Always, GroupTotals.Never }, GroupTotals.ExpandedGroup, value => _grid.GroupTotals = value);
			options.AddToggle("Grand total row", true, on => _grid.TotalRow = on ? InheritableBoolean.True : InheritableBoolean.False);
			options.AddToggle("Group by box", true, on => _grid.GroupByBoxVisible = on);
			options.AddToggle("Keep total row in view", true, on => _grid.TotalRowPosition = on ? TotalRowPosition.BottomFixed : TotalRowPosition.BottomScrollable);
			options.AddButton("Expand all groups", () => _grid.ExpandGroups());
			options.AddButton("Collapse all groups", () => _grid.CollapseGroups());
			options.AddNote("Totals: people count, payroll sum and average rating. Drag more headers to the group box to nest the groups.");
			Compose(_grid, options);
		}

		private void SetGrouping(string choice)
		{
			GridTable table = _grid.RootTable;
			table.Groups.Clear();
			GridColumnCollection columns = table.Columns;
			switch (choice)
			{
				case "Department":
					table.Groups.Add(columns["Department"]);
					break;
				case "Country":
					table.Groups.Add(columns["Country"]);
					break;
				case "Department, then country":
					table.Groups.Add(columns["Department"]);
					table.Groups.Add(columns["Country"]);
					break;
				case "Job title":
					table.Groups.Add(columns["Title"]);
					break;
			}
			_grid.GroupMode = GroupMode.Collapsed;
			ExpandFirstGroup();
		}

		public override void OnShown()
		{
			ExpandFirstGroup();
		}

		private void ExpandFirstGroup()
		{
			GridRow[] rows = _grid.GetRows();
			if (rows.Length > 0 && rows[0].RowType == RowType.GroupHeader)
			{
				rows[0].Expanded = true;
			}
			GridDemo.ScrollToTop(_grid);
		}

		public override string Title => "Grouping and totals";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "Group rows by one or more columns and see totals for each group and for the whole grid.";

		public override int Order => 20;
	}
}
