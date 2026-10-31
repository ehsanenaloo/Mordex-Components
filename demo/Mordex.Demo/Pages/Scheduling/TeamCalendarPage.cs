using System;
using System.Windows.Forms;
using Mordex.Windows.Scheduler;

namespace Mordex.Demo
{
	public sealed class TeamCalendarPage : DemoPage
	{
		private readonly MxScheduler _scheduler = new MxScheduler();
		private readonly MxCalendarView _navigator = new MxCalendarView();
		private readonly Panel _content = new Panel();
		private ComboBox _person;
		private CheckBox _sideBySide;
		private string _viewName = "Day";

		public TeamCalendarPage()
		{
			SchedulingSample.AddOwners(_scheduler);
			SchedulingSample.SetWorkingTime(_scheduler, true);
			SchedulingSample.AddCalendarEvents(_scheduler);
			_scheduler.Dock = DockStyle.Fill;
			_scheduler.Date = SchedulingSample.FirstDay();
			_scheduler.View = SchedulerView.DayView;
			_scheduler.FirstDayOfWeek = SchedulerDayOfWeek.Monday;
			_scheduler.DayEndHour = 20;
			_scheduler.DayStartHour = 7;
			_scheduler.Interval = Interval.ThirtyMinutes;
			_scheduler.ShowTimeAsClocks = false;
			_scheduler.AllowEdit = true;
			_scheduler.AllowAppointmentDrag = AllowAppointmentDrag.All;

			_navigator.Dock = DockStyle.Left;
			_navigator.Width = 230;
			_navigator.FirstDayOfWeek = CalendarDayOfWeek.Monday;
			_navigator.WeekNumbers = true;
			_navigator.Scheduler = _scheduler;

			_content.Controls.Add(_scheduler);
			_content.Controls.Add(_navigator);
			_scheduler.BringToFront();

			OptionsPanel options = new OptionsPanel();
			options.AddChoice("View", new[] { "Day", "Work week", "Week", "Month" }, _viewName, ChangeView);
			_sideBySide = options.AddToggle("Show people side by side", true, value => UpdateOwners());
			_person = options.AddChoice("Show one person", DemoData.Staff, DemoData.Staff[0], name => UpdateOwners());
			options.AddToggle("Mark the current time", true, value => _scheduler.HighlightCurrentTime = value ? TriState.True : TriState.False);
			options.AddToggle("Show the date navigator", true, value => _navigator.Visible = value);
			options.AddToggle("Allow drag and edit", true, value =>
			{
				_scheduler.AllowEdit = value;
				_scheduler.AllowAppointmentDrag = value ? AllowAppointmentDrag.All : AllowAppointmentDrag.None;
			});
			options.AddChoice("Time slot", new[] { "15 minutes", "30 minutes", "60 minutes" }, "30 minutes", text =>
			{
				_scheduler.Interval = text.StartsWith("15", StringComparison.Ordinal) ? Interval.FifteenMinutes : text.StartsWith("30", StringComparison.Ordinal) ? Interval.ThirtyMinutes : Interval.SixtyMinutes;
				ScrollToWorkDay();
			});
			options.AddCaption("People");
			for (int i = 0; i < DemoData.Staff.Length; i++)
			{
				options.AddCaption(DemoData.Staff[i]).ForeColor = SchedulingSample.ColorOf(i);
			}
			options.AddNote("Work hours are 08:00 to 18:00 with a lunch break from 12:00 to 13:00. Drag an appointment to move it.");
			Compose(_content, options);
			UpdateOwners();
		}

		public override string Title => "Team calendar";

		public override string Group => "Scheduling";

		public override string Summary => "A shared calendar for five people with day, work week, week and month views. Each person has a color, and non-working time is shaded.";

		public override int Order => 10;

		public override void OnLookChanged()
		{
			SchedulingSample.SetWorkingTime(_scheduler, true);
		}

		public override void OnShown()
		{
			ScrollToWorkDay();
		}

		private void ChangeView(string name)
		{
			_viewName = name;
			switch (name)
			{
			case "Day":
				_scheduler.View = SchedulerView.DayView;
				break;
			case "Work week":
				_scheduler.View = SchedulerView.WorkWeek;
				break;
			case "Week":
				_scheduler.View = SchedulerView.WeekView;
				break;
			default:
				_scheduler.View = SchedulerView.MonthView;
				break;
			}
			_scheduler.Weeks = 3;
			_scheduler.Date = name == "Month" ? DemoData.WeekStart : SchedulingSample.FirstDay();
			UpdateOwners();
			ScrollToWorkDay();
		}

		// Only the day view has room for the people side by side. The other views show one person.
		private void UpdateOwners()
		{
			if (_sideBySide == null || _person == null)
			{
				return;
			}
			bool day = _viewName == "Day";
			_sideBySide.Enabled = day;
			bool sideBySide = day && _sideBySide.Checked;
			_person.Enabled = !sideBySide;
			_scheduler.MultiOwner = sideBySide;
			if (!sideBySide && _person.SelectedItem is string name)
			{
				_scheduler.CurrentOwner = _scheduler.Owners[Array.IndexOf(DemoData.Staff, name)];
			}
		}

		private void ScrollToWorkDay()
		{
			_scheduler.FirstVisibleTime = TimeSpan.FromHours(7);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_navigator.Scheduler = null;
				_navigator.Dispose();
				_scheduler.Dispose();
				_content.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
