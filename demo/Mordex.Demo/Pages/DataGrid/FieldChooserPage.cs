using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid;
using Mordex.Windows.FilterBuilder;

namespace Mordex.Demo
{
	/// <summary>A dialog that edits the filter of a grid with the filter builder. It takes the theme of the application.</summary>
	internal sealed class FilterDialog : Form
	{
		private readonly MxFilterBuilder _builder;

		public FilterDialog(MxDataGrid grid)
		{
			Text = "Filter builder";
			Size = new Size(720, 420);
			StartPosition = FormStartPosition.CenterParent;
			MinimizeBox = false;
			MaximizeBox = false;
			ShowInTaskbar = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			_builder = new MxFilterBuilder { Dock = DockStyle.Fill, SourceControl = grid, AutoApply = false };
			Button ok = new Button { Text = "Apply", DialogResult = DialogResult.OK, Width = 90, Height = 28 };
			Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90, Height = 28 };
			FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8, 8, 8, 0) };
			buttons.Controls.Add(cancel);
			buttons.Controls.Add(ok);
			Controls.Add(_builder);
			Controls.Add(buttons);
			AcceptButton = ok;
			CancelButton = cancel;
			ControlThemer.Attach(this);
		}

		public void ApplyFilter()
		{
			_builder.ApplyFilter();
		}
	}

	public sealed class FieldChooserPage : DemoPage
	{
		private readonly MxDataGrid _grid;
		private readonly MxFieldChooser _chooser = new MxFieldChooser { Dock = DockStyle.Fill };
		private readonly Label _status;
		private readonly Label _count = PageKit.Value();

		public FieldChooserPage()
		{
			_grid = GridDemo.CreateGrid(DemoData.Employees());
			GridDemo.SetupEmployeeColumns(_grid);
			_grid.AllowEdit = InheritableBoolean.False;
			_grid.GroupByBoxVisible = true;
			_grid.RootTable.SortKeys.Add("Name");
			_grid.RootTable.Columns["Id"].Visible = false;
			_grid.RootTable.Columns["Country"].Visible = false;
			_grid.RootTable.Columns["City"].Visible = false;
			_chooser.DataGrid = _grid;

			Mordex.Windows.UI.Controls.MxGroupBox chooserGroup = PageKit.Group("Field chooser");
			chooserGroup.Controls.Add(_chooser);
			chooserGroup.Dock = DockStyle.Right;
			chooserGroup.Width = 230;
			chooserGroup.Margin = new Padding(0);

			Panel host = new Panel { Dock = DockStyle.Fill };
			_grid.Dock = DockStyle.Fill;
			host.Controls.Add(_grid);
			host.Controls.Add(chooserGroup);
			_grid.BringToFront();

			OptionsPanel options = new OptionsPanel();
			options.AddCaption("Field chooser");
			options.AddToggle("Show the field chooser panel", true, on => chooserGroup.Visible = on);
			options.AddButton("Open the chooser in a window", () =>
			{
				FieldChooserForm window = _grid.ShowFieldChooser(FindForm(), "Fields");
				_status.Text = window == null ? "The chooser could not open." : "The chooser window is open. Drag a header into it to hide a column.";
			});
			options.AddCaption("Filter");
			options.AddButton("Edit the filter in a dialog", EditFilter);
			options.AddButton("Clear the filter", () =>
			{
				_grid.RemoveFilters();
				_grid.RootTable.RemoveFilter();
				UpdateCount();
			});
			_status = options.AddNote("Drag a column header onto the chooser to hide it, and drag a field from the chooser to the header to show it again.");
			Compose(host, options);
			_grid.FilterApplied += (s, e) => UpdateCount();
			UpdateCount();
		}

		private void EditFilter()
		{
			using (FilterDialog dialog = new FilterDialog(_grid))
			{
				if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
				{
					dialog.ApplyFilter();
					UpdateCount();
				}
			}
		}

		private void UpdateCount()
		{
			_count.Text = _grid.RecordCount + " employees shown";
			_status.Text = _count.Text + ".";
		}

		public override void OnShown()
		{
			GridDemo.ScrollToTop(_grid);
		}

		public override string Title => "Field chooser and filter dialog";

		public override string Group => GridDemo.GroupName;

		public override string Summary => "Hide and show columns with the field chooser, and set conditions for the grid in a filter builder dialog.";

		public override int Order => 80;
	}
}
