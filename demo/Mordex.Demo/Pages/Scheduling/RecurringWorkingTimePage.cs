using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Scheduler;

namespace Mordex.Demo
{
	public sealed class RecurringWorkingTimePage : DemoPage
	{
		private static readonly Color StandupColor = Color.FromArgb(0, 128, 128);
		private static readonly Color ReviewColor = Color.FromArgb(0, 114, 188);
		private static readonly Color ReportColor = Color.FromArgb(142, 68, 173);
		private static readonly Color MeetingColor = Color.FromArgb(110, 120, 135);
		private static readonly Color HolidayColor = Color.FromArgb(246, 222, 222);

		private readonly MxScheduler _scheduler = new MxScheduler();
		private readonly DateTime _monday = SchedulingSample.MondayOf(SchedulingSample.FirstDay());
		private readonly WorkingHourException _holiday = new WorkingHourException();
		private Label _result;
		private TimeSpan _duration = TimeSpan.FromHours(1);
		private bool _onlyWorkingTime = true;
		private bool _lunchBreak = true;
		private DateTime _from;
		private DateTime _found = DateTime.MinValue;

		public RecurringWorkingTimePage()
		{
			_from = SchedulingSample.FirstDay().AddHours(8);
			_scheduler.Dock = DockStyle.Fill;
			SchedulingSample.SetWorkingTime(_scheduler, _lunchBreak);
			_scheduler.FirstDayOfWeek = SchedulerDayOfWeek.Monday;
			_scheduler.View = SchedulerView.WorkWeek;
			_scheduler.Date = _monday;
			_scheduler.DayEndHour = 20;
			_scheduler.DayStartHour = 7;
			_scheduler.Interval = Interval.ThirtyMinutes;
			_scheduler.ShowTimeAsClocks = false;
			AddHoliday();
			AddRecurring();
			AddMeetings();

			OptionsPanel options = new OptionsPanel();
			options.AddChoice("View", new[] { "Day", "Work week", "Week", "Month" }, "Work week", ChangeView);
			options.AddToggle("Lunch break is not working time", _lunchBreak, value =>
			{
				_lunchBreak = value;
				SchedulingSample.SetWorkingTime(_scheduler, value);
				_scheduler.Invalidate();
			});
			options.AddToggle("Holiday on Wednesday", true, value =>
			{
				_scheduler.WorkingHourSchema.Exceptions.Clear();
				if (value)
				{
					_scheduler.WorkingHourSchema.Exceptions.Add(_holiday);
				}
				_scheduler.SetDateBackColor(_monday.AddDays(2), value ? HolidayColor : Color.Empty);
			});
			options.AddCaption("Free time search");
			options.AddChoice("Length", new[] { "30 minutes", "1 hour", "2 hours", "4 hours" }, "1 hour", text =>
			{
				_duration = text.StartsWith("30", StringComparison.Ordinal) ? TimeSpan.FromMinutes(30) : text.StartsWith("1", StringComparison.Ordinal) ? TimeSpan.FromHours(1) : text.StartsWith("2", StringComparison.Ordinal) ? TimeSpan.FromHours(2) : TimeSpan.FromHours(4);
			});
			options.AddToggle("Only in working time", true, value => _onlyWorkingTime = value);
			options.AddButton("Find a free slot", FindSlot);
			options.AddButton("Book this slot", BookSlot);
			_result = options.AddNote("Press the button to find the first slot with no appointment.");
			options.AddNote("Recurring: a daily stand-up, a weekly review on Friday and a monthly report. Drag the Friday review to see how a single occurrence is changed.");
			Compose(_scheduler, options);
		}

		public override string Title => "Recurring and working time";

		public override string Group => "Scheduling";

		public override string Summary => "Recurring appointments, a shaded lunch break and a holiday, and a search for the next free slot. The search can skip non-working time.";

		public override int Order => 20;

		public override void OnLookChanged()
		{
			SchedulingSample.SetWorkingTime(_scheduler, _lunchBreak);
		}

		public override void OnShown()
		{
			_scheduler.FirstVisibleTime = TimeSpan.FromHours(7);
		}

		private void AddHoliday()
		{
			_holiday.DateRange = new DateRange(_monday.AddDays(2), _monday.AddDays(3));
			_holiday.HourRange.StartTime = TimeSpan.Zero;
			_holiday.HourRange.EndTime = TimeSpan.FromDays(1);
			_holiday.HourRange.IsWorkingTime = false;
			_holiday.HourRange.FormatStyle.BackColor = HolidayColor;
			_holiday.HourRange.AlternatingFormatStyle.BackColor = HolidayColor;
			_scheduler.WorkingHourSchema.Exceptions.Add(_holiday);
			_scheduler.SetDateBackColor(_monday.AddDays(2), HolidayColor);
		}

		private void AddRecurring()
		{
			SchedulerAppointment standup = new SchedulerAppointment(_monday.AddHours(9), _monday.AddHours(9.25), "Daily stand-up");
			standup.Description = "Video call";
			RecurrencePattern daily = new RecurrencePattern();
			daily.RecurrenceType = RecurrenceType.Daily;
			daily.DailyRecurrenceMode = DailyRecurrenceMode.WeekDays;
			daily.PatternStartDate = _monday.AddDays(-60);
			daily.RecurrenceEndMode = RecurrenceEndMode.NoEndDate;
			standup.RecurrencePattern = daily;
			SchedulingSample.Style(standup, StandupColor);
			_scheduler.Appointments.Add(standup);

			SchedulerAppointment review = new SchedulerAppointment(_monday.AddDays(4).AddHours(15), _monday.AddDays(4).AddHours(16), "Weekly review");
			review.Description = "Room A";
			RecurrencePattern weekly = new RecurrencePattern();
			weekly.RecurrenceType = RecurrenceType.Weekly;
			weekly.DayOfWeek = SchedulerDayOfWeek.Friday;
			weekly.Interval = 1;
			weekly.PatternStartDate = _monday.AddDays(-60);
			weekly.RecurrenceEndMode = RecurrenceEndMode.NoEndDate;
			review.RecurrencePattern = weekly;
			SchedulingSample.Style(review, ReviewColor);
			_scheduler.Appointments.Add(review);

			DateTime reportDay = _monday.AddDays(1);
			SchedulerAppointment report = new SchedulerAppointment(reportDay.AddHours(14), reportDay.AddHours(16), "Monthly report");
			report.Description = "Finance";
			RecurrencePattern monthly = new RecurrencePattern();
			monthly.RecurrenceType = RecurrenceType.Monthly;
			monthly.DayOfMonth = reportDay.Day;
			monthly.Interval = 1;
			monthly.PatternStartDate = _monday.AddDays(-60);
			monthly.RecurrenceEndMode = RecurrenceEndMode.NoEndDate;
			report.RecurrencePattern = monthly;
			SchedulingSample.Style(report, ReportColor);
			_scheduler.Appointments.Add(report);
		}

		private void AddMeetings()
		{
			foreach (DemoData.CalendarEvent item in DemoData.CalendarEvents())
			{
				bool onHoliday = item.Start.Date == _monday.AddDays(2);
				bool overlapsStandup = item.Start.TimeOfDay < TimeSpan.FromHours(9.25) && item.End.TimeOfDay > TimeSpan.FromHours(9);
				if (item.Owner != DemoData.Staff[0] || onHoliday || overlapsStandup)
				{
					continue;
				}
				SchedulerAppointment appointment = new SchedulerAppointment(item.Start, item.End, item.Subject);
				appointment.Description = item.Location;
				SchedulingSample.Style(appointment, MeetingColor);
				_scheduler.Appointments.Add(appointment);
			}
		}

		private void ChangeView(string name)
		{
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
			_scheduler.Weeks = 4;
			_scheduler.Date = name == "Day" ? SchedulingSample.FirstDay() : _monday;
			_scheduler.FirstVisibleTime = TimeSpan.FromHours(7);
		}

		private void FindSlot()
		{
			DateTime slot = _scheduler.FindFreeTimeSlot(_from, _duration, _onlyWorkingTime);
			if (slot == DateTime.MaxValue)
			{
				_found = DateTime.MinValue;
				_result.Text = "There is no free slot of this length.";
				return;
			}
			_found = slot;
			_from = slot.AddMinutes(30);
			_scheduler.HideSelection = HideSelection.Highlight;
			_scheduler.SelectedDays = new DateRange(slot, slot.Add(_duration));
			_scheduler.Date = slot.Date;
			_scheduler.FirstVisibleTime = TimeSpan.FromHours(7);
			_result.Text = "Free: " + slot.ToString("ddd d MMM, HH:mm") + " to " + slot.Add(_duration).ToString("HH:mm") + ". Press again for the next one.";
		}

		private void BookSlot()
		{
			if (_found == DateTime.MinValue)
			{
				_result.Text = "Find a free slot first.";
				return;
			}
			SchedulerAppointment booked = new SchedulerAppointment(_found, _found.Add(_duration), "New meeting");
			SchedulingSample.Style(booked, MeetingColor);
			_scheduler.Appointments.Add(booked);
			_result.Text = "Booked " + _found.ToString("ddd d MMM, HH:mm") + ".";
			_from = _found;
			_found = DateTime.MinValue;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_scheduler.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
