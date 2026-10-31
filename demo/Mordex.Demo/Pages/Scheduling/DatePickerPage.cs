using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Mordex.Windows.DatePicker;
using Mordex.Windows.Scheduler;

namespace Mordex.Demo
{
	public sealed class DatePickerPage : DemoPage
	{
		private static readonly Dictionary<string, string> Cultures = new Dictionary<string, string>
		{
			["English (United States)"] = "en-US",
			["English (United Kingdom)"] = "en-GB",
			["German"] = "de-DE",
			["French"] = "fr-FR",
			["Spanish"] = "es-ES",
			["Japanese"] = "ja-JP"
		};

		// The culture of the thread before this page took it; null while the page does not hold the thread culture.
		private CultureInfo _originalCulture;
		private readonly MxDatePicker _single = new MxDatePicker();
		private readonly MxDatePicker _limited = new MxDatePicker();
		private readonly MxDatePicker _threeMonths = new MxDatePicker();
		private readonly MxCalendarView _range = new MxCalendarView();
		private readonly MxCalendarView _months = new MxCalendarView();
		private readonly Label _rangeText = new Label { Height = 26, TextAlign = ContentAlignment.MiddleLeft };
		private readonly Label _busyText = new Label { Height = 26, TextAlign = ContentAlignment.MiddleLeft, ForeColor = SystemColors.GrayText };
		private readonly Panel _content = new Panel { BackColor = Color.White };
		private CultureInfo _culture = CultureInfo.CurrentCulture;
		private DateTime[] _busyDays;
		private string _firstDayChoice = "Culture default";

		public DatePickerPage()
		{
			DateTime today = DateTime.Today;
			_busyDays = DemoData.CalendarEvents().Select(item => item.Start.Date).Distinct().ToArray();

			_single.Value = today;
			_single.DateFormat = DateFormat.Long;
			_single.ShowTodayButton = true;

			_limited.Value = today;
			_limited.DateFormat = DateFormat.Short;
			_limited.MinDate = today.AddDays(-30);
			_limited.MaxDate = today.AddDays(60);
			_limited.DropDownCalendar.BoldedDates = _busyDays;
			_limited.DropDownCalendar.WeekNumbers = true;

			_threeMonths.Value = today;
			_threeMonths.DateFormat = DateFormat.Long;
			_threeMonths.DropDownColumns = 3;
			_threeMonths.DropDownRows = 1;
			_threeMonths.DropDownCalendar.BoldedDates = _busyDays;

			_range.SelectionStyle = CalendarSelectionStyle.Range;
			_range.HideSelection = HideSelection.Highlight;
			_range.MaxSelectionCount = 14;
			_range.SelectedDates = Enumerable.Range(0, 5).Select(day => DemoData.WeekStart.AddDays(day)).ToArray();
			_range.SelectionChanged += (sender, e) => ShowRange();
			_range.BoldedDates = _busyDays;

			_months.SelectionStyle = CalendarSelectionStyle.Free;
			_months.BoldedDates = _busyDays;
			_months.WeekNumbers = true;

			BuildLayout();
			ShowRange();

			OptionsPanel options = new OptionsPanel();
			options.AddChoice("Culture", Cultures.Keys.ToArray(), CultureNameOf(_culture), name => SetCulture(Cultures[name]));
			options.AddChoice("First day of week", new[] { "Culture default", "Monday", "Sunday", "Saturday" }, "Culture default", SetFirstDay);
			options.AddToggle("Show week numbers", true, value =>
			{
				_limited.DropDownCalendar.WeekNumbers = value;
				_months.WeekNumbers = value;
				_range.WeekNumbers = value;
			});
			options.AddToggle("Show days of other months", true, value =>
			{
				_single.DropDownCalendar.ShowTrailingDays = value;
				_limited.DropDownCalendar.ShowTrailingDays = value;
				_threeMonths.DropDownCalendar.ShowTrailingDays = value;
				_range.ShowTrailingDays = value;
				_months.ShowTrailingDays = value;
			});
			options.AddToggle("Show the Today button", true, value =>
			{
				_single.ShowTodayButton = value;
				_limited.ShowTodayButton = value;
				_threeMonths.ShowTodayButton = value;
			});
			options.AddChoice("Selection in the calendar", new[] { "Range", "Single day", "Free choice" }, "Range", text =>
			{
				_range.SelectionStyle = text == "Range" ? CalendarSelectionStyle.Range : text == "Single day" ? CalendarSelectionStyle.Single : CalendarSelectionStyle.Free;
				ShowRange();
			});
			options.AddNote("Days in bold have appointments in the team calendar. The second picker only accepts dates from 30 days ago to 60 days ahead.");
			Compose(_content, options);
			VisibleChanged += (sender, e) => HoldCulture(Visible && Parent != null);
			ParentChanged += (sender, e) => HoldCulture(Visible && Parent != null);
		}

		public override string Title => "Date picker and calendar";

		public override string Group => "Scheduling";

		public override string Summary => "Date pickers with a single date, limits and bold days, and calendars with range selection and several months.";

		public override int Order => 40;

		private static string CultureNameOf(CultureInfo culture)
		{
			foreach (KeyValuePair<string, string> pair in Cultures)
			{
				if (string.Equals(pair.Value, culture.Name, StringComparison.OrdinalIgnoreCase))
				{
					return pair.Key;
				}
			}
			return "English (United States)";
		}

		private void BuildLayout()
		{
			FlowLayoutPanel top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 135, Padding = new Padding(12, 12, 0, 0), WrapContents = false, AutoScroll = true };
			top.Controls.Add(Card("Single date", "Type a date or open the calendar.", _single, 290, 120));
			top.Controls.Add(Card("Limits and bold days", "Dates outside the limits cannot be chosen.", _limited, 290, 120));
			top.Controls.Add(Card("Three months", "The drop-down shows three months at once.", _threeMonths, 330, 120));

			TableLayoutPanel bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(12, 0, 0, 12) };
			bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
			bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			Panel rangeCard = Card("Date range", "Click a day, then Shift and click another.", _range, 0, 0);
			rangeCard.Dock = DockStyle.Top;
			rangeCard.Height = 350;
			Panel summary = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 8, 0) };
			_rangeText.Dock = DockStyle.Top;
			_busyText.Dock = DockStyle.Top;
			summary.Controls.Add(_busyText);
			summary.Controls.Add(_rangeText);
			Panel left = new Panel { Dock = DockStyle.Fill };
			left.Controls.Add(summary);
			left.Controls.Add(rangeCard);
			Panel monthsCard = Card("Several months", "Scroll to move through the year.", _months, 0, 0);
			monthsCard.Dock = DockStyle.Fill;
			bottom.Controls.Add(left, 0, 0);
			bottom.Controls.Add(monthsCard, 1, 0);

			_content.Controls.Add(bottom);
			_content.Controls.Add(top);
		}

		private Panel Card(string title, string text, Control control, int width, int height)
		{
			Panel card = new Panel { Width = width, Height = height, Margin = new Padding(0, 0, 16, 0), Padding = new Padding(0, 0, 8, 0) };
			Label heading = new Label { Text = title, Dock = DockStyle.Top, Height = 24, Font = new Font(Font, FontStyle.Bold) };
			Label note = new Label { Text = text, Dock = DockStyle.Top, Height = 22, ForeColor = SystemColors.GrayText };
			if (width > 0)
			{
				control.Width = width - 16;
				control.Location = new Point(0, 62);
				control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			}
			else
			{
				control.Dock = DockStyle.Fill;
			}
			card.Controls.Add(control);
			card.Controls.Add(note);
			card.Controls.Add(heading);
			return card;
		}

		private void ShowRange()
		{
			DateTime[] days = _range.SelectedDates;
			if (days == null || days.Length == 0)
			{
				_rangeText.Text = "Nothing is selected.";
				_busyText.Text = "";
				return;
			}
			DateTime first = days.Min();
			DateTime last = days.Max();
			_rangeText.Text = days.Length == 1
				? "Selected: " + first.ToString("D", CultureInfo.CurrentCulture)
				: "Selected: " + first.ToString("d MMM", CultureInfo.CurrentCulture) + " to " + last.ToString("d MMM", CultureInfo.CurrentCulture) + ", " + days.Length + " days";
			int busy = days.Count(day => _busyDays.Contains(day.Date));
			_busyText.Text = busy + (busy == 1 ? " day has" : " days have") + " appointments.";
		}

		private void SetCulture(string name)
		{
			_culture = CultureInfo.GetCultureInfo(name);
			if (_originalCulture != null)
			{
				ApplyCulture(_culture);
			}
		}

		// The pickers read the culture of the thread, so it is set only while this page is on screen and put back when it is not.
		// Other pages made or shown later must not see the culture of this page.
		private void HoldCulture(bool hold)
		{
			if (hold && _originalCulture == null)
			{
				_originalCulture = CultureInfo.CurrentCulture;
				ApplyCulture(_culture);
			}
			else if (!hold && _originalCulture != null)
			{
				CultureInfo original = _originalCulture;
				_originalCulture = null;
				ApplyCulture(original);
			}
		}

		private void ApplyCulture(CultureInfo culture)
		{
			CultureInfo.CurrentCulture = culture;
			// The pickers build their text when the format changes, so the format is set again to use the new culture.
			RefreshFormat(_single, DateFormat.Long);
			RefreshFormat(_limited, DateFormat.Short);
			RefreshFormat(_threeMonths, DateFormat.Long);
			SetFirstDay(_firstDayChoice);
			foreach (Control view in new Control[] { _range, _months })
			{
				view.Invalidate(true);
			}
			ShowRange();
			PerformLayout();
		}

		private static void RefreshFormat(MxDatePicker picker, DateFormat format)
		{
			picker.DateFormat = format == DateFormat.Long ? DateFormat.Short : DateFormat.Long;
			picker.DateFormat = format;
		}

		private void SetFirstDay(string name)
		{
			_firstDayChoice = name;
			DayOfWeek day = name == "Monday" ? DayOfWeek.Monday : name == "Sunday" ? DayOfWeek.Sunday : name == "Saturday" ? DayOfWeek.Saturday : _culture.DateTimeFormat.FirstDayOfWeek;
			_single.DropDownCalendar.FirstDayOfWeek = (Mordex.Windows.DatePicker.CalendarDayOfWeek)(int)day;
			_limited.DropDownCalendar.FirstDayOfWeek = (Mordex.Windows.DatePicker.CalendarDayOfWeek)(int)day;
			_threeMonths.DropDownCalendar.FirstDayOfWeek = (Mordex.Windows.DatePicker.CalendarDayOfWeek)(int)day;
			_range.FirstDayOfWeek = (Mordex.Windows.Scheduler.CalendarDayOfWeek)(int)day;
			_months.FirstDayOfWeek = (Mordex.Windows.Scheduler.CalendarDayOfWeek)(int)day;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_originalCulture != null)
				{
					CultureInfo.CurrentCulture = _originalCulture;
					_originalCulture = null;
				}
				_single.Dispose();
				_limited.Dispose();
				_threeMonths.Dispose();
				_range.Dispose();
				_months.Dispose();
				_rangeText.Dispose();
				_busyText.Dispose();
				_content.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
