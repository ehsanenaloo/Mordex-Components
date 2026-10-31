using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid;
using Mordex.Windows.Scheduler;
using Mordex.Windows.Timeline;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Demo
{
	/// <summary>
	/// The three print documents with a live preview. A document prints on white paper in a light look, also when the
	/// window uses a dark theme.
	/// </summary>
	public sealed class PrintPreviewPage : DemoPage
	{
		private const string Grid = "Data grid";
		private const string Calendar = "Scheduler";
		private const string Projects = "Timeline";

		private readonly MxDataGrid _grid;
		private readonly MxScheduler _scheduler = new MxScheduler { Dock = DockStyle.Fill };
		private readonly MxTimeline _timeline = new MxTimeline { Dock = DockStyle.Fill };
		private readonly MxTabControl _sources = new MxTabControl { Dock = DockStyle.Fill };
		private readonly MxDataGridPrintDocument _gridDocument = new MxDataGridPrintDocument();
		private readonly MxSchedulerPrintDocument _schedulerDocument = new MxSchedulerPrintDocument();
		private readonly MxTimelinePrintDocument _timelineDocument = new MxTimelinePrintDocument();
		private readonly PrintPreviewControl _preview = new PrintPreviewControl { Dock = DockStyle.Fill, AutoZoom = true, UseAntiAlias = true };
		private readonly Label _note = new Label { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 6, 8, 0) };
		private readonly bool _hasPrinter;
		private PrintDocument _current;

		public PrintPreviewPage()
		{
			_hasPrinter = PrinterSettings.InstalledPrinters.Count > 0;
			_grid = GridDemo.CreateGrid(DemoData.Employees());
			GridDemo.SetupEmployeeColumns(_grid);
			_grid.AllowEdit = InheritableBoolean.False;
			_grid.Dock = DockStyle.Fill;
			_grid.RootTable.SortKeys.Add("Name");

			SchedulingSample.AddOwners(_scheduler);
			SchedulingSample.SetWorkingTime(_scheduler, true);
			SchedulingSample.AddCalendarEvents(_scheduler);
			_scheduler.Date = SchedulingSample.FirstDay();
			_scheduler.View = SchedulerView.WeekView;

			_timeline.Fields.Add(new TimelineField("Project") { TypeCode = TypeCode.String });
			foreach (DemoData.ProjectTask task in DemoData.ProjectTasks())
			{
				TimelineItem item = new TimelineItem(task.Start, task.End, task.Name);
				_timeline.Items.Add(item);
				item.SetValue("Project", task.Project);
			}
			_timeline.TimescaleTiers = TimescaleTiers.TwoTiers;
			_timeline.TopTier.Interval = TimelineInterval.Month;
			_timeline.MiddleTier.Interval = TimelineInterval.Month;
			_timeline.BottomTier.Interval = TimelineInterval.Week;
			_timeline.FirstDate = DemoData.WeekStart.AddDays(-14);

			_sources.TabPages.Add(Tab(Grid, _grid));
			_sources.TabPages.Add(Tab(Calendar, _scheduler));
			_sources.TabPages.Add(Tab(Projects, _timeline));

			_gridDocument.DataGrid = _grid;
			_gridDocument.PageHeaderCenter = "Employees";
			_gridDocument.PageFooterCenter = "Page &p of &P";
			_schedulerDocument.Scheduler = _scheduler;
			_timelineDocument.Timeline = _timeline;
			_current = _gridDocument;

			Panel previewHost = new Panel { Dock = DockStyle.Fill };
			previewHost.Controls.Add(_preview);
			previewHost.Controls.Add(_note);
			_preview.BringToFront();
			_note.Text = _hasPrinter
				? "The preview shows the printed page. It is white with dark text, whatever theme the window uses."
				: "No printer is installed, so the preview cannot be drawn on this computer.";
			_note.ForeColor = SystemColors.GrayText;

			TableLayoutPanel grid = PageKit.Grid(2, 100f);
			Mordex.Windows.UI.Controls.MxGroupBox source = PageKit.Group("The controls to print");
			source.Controls.Add(_sources);
			Mordex.Windows.UI.Controls.MxGroupBox preview = PageKit.Group("Print preview");
			preview.Controls.Add(previewHost);
			grid.Controls.Add(source, 0, 0);
			grid.Controls.Add(preview, 1, 0);

			OptionsPanel options = new OptionsPanel();
			options.AddChoice("Document", new[] { Grid, Calendar, Projects }, Grid, name =>
			{
				_current = name == Grid ? (PrintDocument)_gridDocument : name == Calendar ? _schedulerDocument : _timelineDocument;
				_sources.SelectedIndex = name == Grid ? 0 : name == Calendar ? 1 : 2;
				RefreshPreview();
			});
			options.AddToggle("Repeat grid headers on each page", true, on =>
			{
				_gridDocument.RepeatHeaders = on;
				RefreshPreview();
			});
			options.AddToggle("Landscape", false, on =>
			{
				_gridDocument.DefaultPageSettings.Landscape = on;
				_schedulerDocument.DefaultPageSettings.Landscape = on;
				_timelineDocument.DefaultPageSettings.Landscape = on;
				RefreshPreview();
			});
			options.AddButton("Open the preview dialog", ShowDialog);
			options.AddNote("Change the theme in the strip above the page and compare the paper with the window.");
			Compose(grid, options);
		}

		private static MxTabPage Tab(string text, Control content)
		{
			MxTabPage page = new MxTabPage { Text = text };
			content.Dock = DockStyle.Fill;
			page.Controls.Add(content);
			return page;
		}

		private void RefreshPreview()
		{
			if (!_hasPrinter || !IsHandleCreated)
			{
				return;
			}
			try
			{
				_preview.Document = _current;
				_preview.InvalidatePreview();
			}
			catch (InvalidPrinterException)
			{
				_note.Text = "The printer settings could not be read, so the preview cannot be drawn.";
			}
		}

		private void ShowDialog()
		{
			if (!_hasPrinter)
			{
				MessageBox.Show(FindForm(), "No printer is installed, so there is nothing to preview.", "Print preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			using (PrintPreviewDialog dialog = new PrintPreviewDialog { Document = _current, WindowState = FormWindowState.Maximized, UseAntiAlias = true })
			{
				ControlThemer.Attach(dialog);
				dialog.ShowDialog(FindForm());
			}
		}

		public override void OnShown()
		{
			RefreshPreview();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_preview.Document = null;
				_gridDocument.Dispose();
				_schedulerDocument.Dispose();
				_timelineDocument.Dispose();
				_grid.Dispose();
				_scheduler.Dispose();
				_timeline.Dispose();
			}
			base.Dispose(disposing);
		}

		public override string Title => "Print and preview";

		public override string Group => "Output and help";

		public override string Summary => "Print documents for the grid, the scheduler and the timeline, with a live preview. Under a dark theme the paper stays light.";

		public override int Order => 10;
	}
}
