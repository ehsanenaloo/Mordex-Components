using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Timeline;

namespace Mordex.Demo
{
	public sealed class ProjectTimelinePage : DemoPage
	{
		private static readonly Color DoneColor = Color.FromArgb(46, 139, 87);
		private static readonly Color ActiveColor = Color.FromArgb(0, 114, 188);
		private static readonly Color PlannedColor = Color.FromArgb(120, 130, 145);

		private readonly MxTimeline _timeline = new MxTimeline();

		public ProjectTimelinePage()
		{
			_timeline.Dock = DockStyle.Fill;
			_timeline.Fields.Add(new TimelineField("Project") { TypeCode = TypeCode.String });
			_timeline.Fields.Add(new TimelineField("Resource") { TypeCode = TypeCode.String });
			_timeline.Fields.Add(new TimelineField("Percent") { TypeCode = TypeCode.Int32 });
			_timeline.ItemRead += (sender, e) => StyleItem(e.Item);
			_timeline.Groups.Add(new TimelineGroup(_timeline.Fields["Project"]));
			_timeline.ItemLayoutMode = ItemLayoutMode.NewRowOnOverlap;
			_timeline.ItemSizeMode = ItemSizeMode.DurationBased;
			_timeline.RowHeight = 54;
			_timeline.ItemsDurationBarHeight = 12;
			_timeline.ShowItemsEstimatedDurationBar = true;
			_timeline.AllowEdit = true;
			_timeline.AllowItemDrag = AllowItemDrag.All;
			_timeline.HighlightCurrentTime = true;
			AddTasks();
			ApplyZoom("Week");
			_timeline.FirstDate = DemoData.WeekStart.AddDays(-14);

			OptionsPanel options = new OptionsPanel();
			options.AddChoice("Zoom", new[] { "Day", "Week", "Month" }, "Week", ApplyZoom);
			options.AddToggle("Mark today", true, value => _timeline.HighlightCurrentTime = value);
			options.AddToggle("Allow move and resize", true, value => _timeline.AllowItemDrag = value ? AllowItemDrag.All : AllowItemDrag.None);
			options.AddToggle("Show week numbers", false, value => _timeline.ShowWeekNumbers = value);
			options.AddChoice("Layout of tasks", new[] { "One row per overlap", "One row per task", "All on one row" }, "One row per overlap", text =>
			{
				_timeline.ItemLayoutMode = text.StartsWith("One row per o", StringComparison.Ordinal) ? ItemLayoutMode.NewRowOnOverlap : text.StartsWith("One row per t", StringComparison.Ordinal) ? ItemLayoutMode.RowXItem : ItemLayoutMode.AllItemsOnOneRow;
			});
			options.AddButton("Collapse all projects", _timeline.CollapseGroups);
			options.AddButton("Expand all projects", _timeline.ExpandGroups);
			options.AddButton("Go to today", () => _timeline.EnsureVisible(DateTime.Today));
			options.AddCaption("Task colors");
			options.AddCaption("Done").ForeColor = DoneColor;
			options.AddCaption("In progress").ForeColor = ActiveColor;
			options.AddCaption("Not started").ForeColor = PlannedColor;
			options.AddNote("Drag a task to move it, or drag its edge to change its length. The text shows how much of the task is done.");
			Compose(_timeline, options);
		}

		public override string Title => "Project timeline";

		public override string Group => "Scheduling";

		public override string Summary => "Four projects with their tasks on a timeline. Zoom between days, weeks and months, and drag a task to move or resize it.";

		public override int Order => 30;

		public override void OnShown()
		{
			_timeline.EnsureVisible(DemoData.WeekStart.AddDays(-14));
		}

		private void AddTasks()
		{
			foreach (DemoData.ProjectTask task in DemoData.ProjectTasks())
			{
				TimelineItem item = new TimelineItem(task.Start, task.End, task.Name + "  " + task.Percent + "%");
				if (task.Percent > 0)
				{
					// The estimated bar shows the finished part of the task.
					item.EstimatedStartTime = task.Start;
					item.EstimatedEndTime = task.Start.AddTicks((task.End - task.Start).Ticks * task.Percent / 100);
				}
				_timeline.Items.Add(item);
				item.SetValue("Project", task.Project);
				item.SetValue("Resource", task.Resource);
				item.SetValue("Percent", task.Percent);
				StyleItem(item);
			}
		}

		// Items are created again from their rows when the control gets its window, so the style is set from the row values.
		private static void StyleItem(TimelineItem item)
		{
			int percent = item.GetValue("Percent") is int value ? value : 0;
			Color color = percent >= 100 ? DoneColor : percent > 0 ? ActiveColor : PlannedColor;
			item.FormatStyle.BackColor = Tint(color);
			item.ItemBarFomatStyle.BackColor = Color.FromArgb(200, color);
			item.ItemEstimatedBarFormatStyle.BackColor = ControlPaint.Dark(color, 0.15f);
			item.ToolTipText = item.Text + "\n" + item.GetValue("Resource") + "\n" + item.StartTime.ToString("d MMM") + " to " + item.EndTime.ToString("d MMM");
		}

		private static Color Tint(Color color)
		{
			return Color.FromArgb(255, (color.R + 255 * 3) / 4, (color.G + 255 * 3) / 4, (color.B + 255 * 3) / 4);
		}

		private void ApplyZoom(string name)
		{
			_timeline.SuspendTimescaleValidation();
			switch (name)
			{
			case "Week":
				SetTiers(TimescaleTiers.ThreeTiers, TimelineInterval.Year, "yyyy", TimelineInterval.Month, "MMMM", TimelineInterval.Week, "d MMM", 150);
				break;
			case "Month":
				SetTiers(TimescaleTiers.TwoTiers, TimelineInterval.Year, "yyyy", TimelineInterval.Year, "yyyy", TimelineInterval.Month, "MMMM", 300);
				break;
			default:
				SetTiers(TimescaleTiers.ThreeTiers, TimelineInterval.Month, "MMMM yyyy", TimelineInterval.Week, "d MMM", TimelineInterval.Day, "ddd d", 100);
				break;
			}
			_timeline.ResumeTimescaleValidation();
		}

		private void SetTiers(TimescaleTiers tiers, TimelineInterval top, string topFormat, TimelineInterval middle, string middleFormat, TimelineInterval bottom, string bottomFormat, int size)
		{
			_timeline.TimescaleTiers = tiers;
			_timeline.TopTier.Interval = top;
			_timeline.TopTier.CustomFormat = topFormat;
			_timeline.MiddleTier.Interval = middle;
			_timeline.MiddleTier.CustomFormat = middleFormat;
			_timeline.BottomTier.Interval = bottom;
			_timeline.BottomTier.CustomFormat = bottomFormat;
			_timeline.IntervalSize = size;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timeline.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
