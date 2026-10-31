using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid;
using Mordex.Windows.FilterBuilder;

namespace Mordex.Demo
{
	public sealed class FilteringPage : DemoPage
	{
		private readonly MxDataGrid _grid;
		private readonly MxFilterBuilder _builder;
		private readonly Label _count;
		private readonly Timer _timer;
		private readonly Font _boldFont;

		public FilteringPage()
		{
			_grid = GridDemo.CreateGrid(DemoData.Employees());
			GridDemo.SetupEmployeeColumns(_grid);
			_grid.AllowEdit = InheritableBoolean.False;
			_grid.FilterMode = FilterMode.Automatic;
			_grid.RootTable.SortKeys.Add("Name");
			GridColumnCollection columns = _grid.RootTable.Columns;
			columns["Id"].Visible = false;
			GridFilterCondition start = new GridFilterCondition(columns["Active"], ConditionOperator.Equal, true);
			start.AddCondition(LogicalOperator.And, new GridFilterCondition(columns["Salary"], ConditionOperator.GreaterThanOrEqualTo, 60000m));
			_grid.RootTable.FilterCondition = start;

			_builder = new MxFilterBuilder { Dock = DockStyle.Fill, SourceControl = _grid, AutoApply = true };
			_boldFont = new Font(Font, FontStyle.Bold);
			_count = new Label { Dock = DockStyle.Top, Height = 28, Padding = new Padding(8, 7, 0, 0), Font = _boldFont };
			Panel side = new Panel { Dock = DockStyle.Bottom, Height = 160 };
			Panel line = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = SystemColors.ControlDark };
			side.Controls.Add(_builder);
			side.Controls.Add(_count);
			side.Controls.Add(line);

			Panel host = new Panel { Dock = DockStyle.Fill };
			host.Controls.Add(_grid);
			host.Controls.Add(side);
			_grid.Dock = DockStyle.Fill;
			_grid.BringToFront();

			OptionsPanel options = new OptionsPanel();
			options.AddToggle("Filter row", true, on => _grid.FilterMode = on ? FilterMode.Automatic : FilterMode.None);
			options.AddToggle("Apply builder changes at once", true, on => _builder.AutoApply = on);
			options.AddChoice("Sample filters", new[] { "Choose one", "Well paid engineers", "Low rated, inactive", "Hired since 2022" }, "Choose one", ApplySample);
			options.AddButton("Apply builder filter", () => _builder.ApplyFilter());
			options.AddButton("Clear all filters", ClearFilters);
			options.AddNote("Type in the filter row, or build AND and OR conditions in the builder below the grid.");
			Compose(host, options);

			_timer = new Timer { Interval = 300 };
			_timer.Tick += (sender, e) => UpdateCount();
			_timer.Start();
		}

		private void ApplySample(string name)
		{
			GridColumnCollection columns = _grid.RootTable.Columns;
			GridFilterCondition condition = null;
			switch (name)
			{
				case "Well paid engineers":
					condition = new GridFilterCondition(columns["Department"], ConditionOperator.Equal, "Engineering");
					condition.AddCondition(LogicalOperator.And, new GridFilterCondition(columns["Salary"], ConditionOperator.GreaterThanOrEqualTo, 90000m));
					break;
				case "Low rated, inactive":
					condition = new GridFilterCondition(columns["Rating"], ConditionOperator.LessThanOrEqualTo, 2);
					condition.AddCondition(LogicalOperator.Or, new GridFilterCondition(columns["Active"], ConditionOperator.Equal, false));
					break;
				case "Hired since 2022":
					condition = new GridFilterCondition(columns["HireDate"], ConditionOperator.GreaterThanOrEqualTo, new System.DateTime(2022, 1, 1));
					break;
			}
			if (condition != null)
			{
				_grid.RootTable.FilterCondition = condition;
				_builder.Reload();
			}
		}

		private void ClearFilters()
		{
			_grid.RemoveFilters();
			_grid.RootTable.RemoveFilter();
			_builder.Reload();
		}

		private void UpdateCount()
		{
			_count.Text = _grid.RecordCount + " of 240 employees shown";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timer?.Dispose();
				_boldFont?.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnShown()
		{
			GridDemo.ScrollToTop(_grid);
		}

		public override string Title => "Filtering";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "Filter with the row under the headers or with the filter builder, which sets conditions for the grid.";

		public override int Order => 30;
	}
}
