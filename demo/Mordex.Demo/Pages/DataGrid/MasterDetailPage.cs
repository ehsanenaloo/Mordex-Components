using System.Data;
using Mordex.Windows.DataGrid;

namespace Mordex.Demo
{
	public sealed class MasterDetailPage : DemoPage
	{
		private readonly MxDataGrid _grid;
		private readonly DataSet _sales;

		public MasterDetailPage()
		{
			_sales = DemoData.Sales();
			_grid = GridDemo.CreateGrid(_sales, "Customers");
			_grid.AllowEdit = InheritableBoolean.False;

			GridTable customers = _grid.RootTable;
			customers.Caption = "Customers";
			_grid.ColumnAutoResize = false;
			GridDemo.Configure(customers.Columns["CustomerId"], "ID", 70, TextAlignment.Far);
			GridDemo.Configure(customers.Columns["Company"], "Company", 300);
			GridDemo.Configure(customers.Columns["Contact"], "Contact", 240);
			GridDemo.Configure(customers.Columns["Country"], "Country", 170);
			GridDemo.Configure(customers.Columns["City"], "City", 170);

			GridTable orders = customers.ChildTables[0];
			orders.Caption = "Orders";
			GridDemo.Configure(orders.Columns["OrderId"], "Order", 70, TextAlignment.Far);
			GridDemo.Configure(orders.Columns["CustomerId"], "Customer", 70, TextAlignment.Far);
			orders.Columns["CustomerId"].Visible = false;
			GridDemo.Configure(orders.Columns["OrderDate"], "Date", 100, TextAlignment.Far);
			orders.Columns["OrderDate"].FormatString = "d";
			GridDemo.Configure(orders.Columns["Status"], "Status", 90);
			GridDemo.Configure(orders.Columns["Freight"], "Freight", 90, TextAlignment.Far);
			orders.Columns["Freight"].FormatString = "C2";
			orders.Columns["Freight"].AggregateFunction = AggregateFunction.Sum;
			orders.Columns["Freight"].TotalFormatString = "C2";
			orders.TotalRow = InheritableBoolean.True;
			orders.RowHeaders = InheritableBoolean.True;

			GridTable lines = orders.ChildTables[0];
			lines.Caption = "Order lines";
			GridDemo.Configure(lines.Columns["LineId"], "Line", 60, TextAlignment.Far);
			GridDemo.Configure(lines.Columns["OrderId"], "Order", 60, TextAlignment.Far);
			lines.Columns["OrderId"].Visible = false;
			GridDemo.Configure(lines.Columns["ProductId"], "Product", 70, TextAlignment.Far);
			GridDemo.Configure(lines.Columns["Quantity"], "Quantity", 75, TextAlignment.Far);
			lines.Columns["Quantity"].AggregateFunction = AggregateFunction.Sum;
			GridDemo.Configure(lines.Columns["UnitPrice"], "Unit price", 85, TextAlignment.Far);
			lines.Columns["UnitPrice"].FormatString = "C2";
			GridDemo.Configure(lines.Columns["Discount"], "Discount", 75, TextAlignment.Far);
			lines.Columns["Discount"].FormatString = "P0";
			GridDemo.Configure(lines.Columns["Total"], "Line total", 95, TextAlignment.Far);
			lines.Columns["Total"].FormatString = "C2";
			lines.Columns["Total"].AggregateFunction = AggregateFunction.Sum;
			lines.Columns["Total"].TotalFormatString = "C2";
			lines.TotalRow = InheritableBoolean.True;

			OptionsPanel options = new OptionsPanel();
			options.AddToggle("Row headers", true, on => customers.RowHeaders = on ? InheritableBoolean.True : InheritableBoolean.False);
			options.AddToggle("Alternating row colors", true, on => _grid.AlternatingColors = on);
			options.AddToggle("Tree lines", true, on => _grid.TreeLines = on);
			options.AddChoice("Expand box", new[] { ExpandBoxStyle.Classic, ExpandBoxStyle.Arrows }, ExpandBoxStyle.Classic, value => _grid.HierarchicalExpandBoxStyle = value);
			options.AddButton("Expand first level", () => _grid.ExpandRecords(customers));
			options.AddButton("Expand all levels", () => _grid.ExpandRecords());
			options.AddButton("Collapse all", () => _grid.CollapseRecords());
			options.AddNote("Open a customer to see its orders, and an order to see its lines. Each level has its own columns and totals.");
			Compose(_grid, options);
		}

		public override void OnShown()
		{
			GridRow[] customerRows = _grid.GetRows();
			if (customerRows.Length > 0)
			{
				customerRows[0].Expanded = true;
				GridRow[] orderRows = customerRows[0].GetChildRecords();
				if (orderRows.Length > 0)
				{
					orderRows[0].Expanded = true;
				}
			}
			GridDemo.ScrollToTop(_grid);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_sales?.Dispose();
			}
			base.Dispose(disposing);
		}

		public override string Title => "Master detail";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "Customers, their orders and the lines of each order in one grid, with totals at every level.";

		public override int Order => 40;
	}
}
