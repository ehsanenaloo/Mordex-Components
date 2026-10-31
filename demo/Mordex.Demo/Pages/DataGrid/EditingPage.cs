using System.Data;
using System.Drawing;
using Mordex.Windows.DataGrid;

namespace Mordex.Demo
{
	public sealed class EditingPage : DemoPage
	{
		private readonly MxDataGrid _grid;
		private readonly DataTable _employees;
		private readonly GridFormatCondition[] _rules;

		public EditingPage()
		{
			_employees = DemoData.Employees();
			_grid = GridDemo.CreateGrid(_employees);
			GridDemo.SetupEmployeeColumns(_grid);
			_grid.AllowEdit = InheritableBoolean.True;
			_grid.AllowAddNew = InheritableBoolean.True;
			_grid.AllowDelete = InheritableBoolean.True;
			_grid.RowHeaders = InheritableBoolean.True;
			_grid.NewRowPosition = NewRowPosition.TopRow;
			_grid.RootTable.SortKeys.Add("Name");

			GridColumnCollection columns = _grid.RootTable.Columns;
			columns["Id"].EditType = EditType.NoEdit;
			columns["Department"].EditType = EditType.Combo;
			columns["Department"].LimitToList = true;
			columns["Department"].HasValueList = true;
			foreach (string department in DemoData.Departments)
			{
				columns["Department"].ValueList.Add(department, department);
			}
			columns["HireDate"].EditType = EditType.CalendarDropDown;
			columns["Salary"].EditType = EditType.TextBox;
			columns["Rating"].ValueList.Clear();
			columns["Rating"].HasValueList = false;
			columns["Rating"].EditType = EditType.IntegerUpDown;
			columns["Rating"].UpDownMinimum = 1;
			columns["Rating"].UpDownMaximum = 5;
			columns["Rating"].TextAlignment = TextAlignment.Center;
			columns["Rating"].HeaderAlignment = TextAlignment.Center;

			_rules = new[]
			{
				Rule(columns["Active"], ConditionOperator.Equal, false, null, Color.FromArgb(246, 246, 246), Color.Gray, TriState.Empty),
				Rule(columns["Rating"], ConditionOperator.LessThanOrEqualTo, 2, columns["Rating"], GridDemo.SoftRed, Color.FromArgb(160, 20, 20), TriState.True),
				Rule(columns["Salary"], ConditionOperator.GreaterThanOrEqualTo, 100000m, columns["Salary"], Color.Empty, Color.Empty, TriState.True)
			};

			OptionsPanel options = new OptionsPanel();
			options.AddToggle("Allow editing", true, on => _grid.AllowEdit = on ? InheritableBoolean.True : InheritableBoolean.False);
			options.AddToggle("Allow new rows", true, on => _grid.AllowAddNew = on ? InheritableBoolean.True : InheritableBoolean.False);
			options.AddToggle("Allow delete", true, on => _grid.AllowDelete = on ? InheritableBoolean.True : InheritableBoolean.False);
			options.AddToggle("Conditional formatting", true, on =>
			{
				foreach (GridFormatCondition rule in _rules)
				{
					rule.Enabled = on;
				}
				_grid.Refresh();
			});
			options.AddChoice("View", new[] { View.TableView, View.CardView }, View.TableView, value => _grid.View = value);
			options.AddChoice("New row", new[] { NewRowPosition.TopRow, NewRowPosition.BottomRow }, NewRowPosition.TopRow, value => _grid.NewRowPosition = value);
			options.AddNote("Click a cell to edit. Department is a combo box, Hired opens a calendar, Rating has a spin box and Active is a check box.");
			options.AddNote("Red: rating of 2 or less. Bold: salary of 100,000 or more. Grey: inactive staff.");
			Compose(_grid, options);
		}

		private GridFormatCondition Rule(GridColumn column, ConditionOperator op, object value, GridColumn target, Color back, Color fore, TriState bold)
		{
			GridFormatCondition rule = new GridFormatCondition(column, op, value);
			rule.TargetColumn = target;
			if (back != Color.Empty)
			{
				rule.FormatStyle.BackColor = back;
			}
			if (fore != Color.Empty)
			{
				rule.FormatStyle.ForeColor = fore;
			}
			if (bold != TriState.Empty)
			{
				rule.FormatStyle.FontBold = bold;
			}
			_grid.RootTable.FormatConditions.Add(rule);
			return rule;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_employees?.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnShown()
		{
			GridDemo.ScrollToTop(_grid);
		}

		public override string Title => "Editing and styles";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "Edit cells with a combo box, a calendar, a spin box and a check box. Rules color the cells and rows by their values.";

		public override int Order => 60;
	}
}
