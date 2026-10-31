using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public class RecurrenceForm : Form
	{
		private DateTimePicker _dtpStartTime;

		private Label _label;

		private Label _lblEndTime;

		private DateTimePicker _dtpEndTime;

		private RadioButton _rdbDaily;

		private RadioButton _rdbDailyInterval;

		private Button _btnCancel;

		private Button _btnOK;

		private DateTimePicker _dtpRecurrenceStart;

		private Label _lblRangeStart;

		private RadioButton _rdbNoEndDate;

		private RadioButton _rdbOccurrences;

		private DateTimePicker _dtpRecurrenceEndDate;

		private TextBox _txtOccurrences;

		private RadioButton _rdbPatternEndDate;

		private RadioButton _rdbWeekly;

		private GroupBox _grbDailyPattern;

		private CheckBox _chkMonday;

		private CheckBox _chkTuesday;

		private CheckBox _chkWednesday;

		private CheckBox _chkSunday;

		private CheckBox _chkThursday;

		private CheckBox _chkFriday;

		private GroupBox _grbWeeklyPattern;

		private GroupBox _grbPattern;

		private TextBox _txtDailyInterval;

		private GroupBox _grbRecurrenceRange;

		private CheckBox _chkSaturday;

		private RadioButton _rdbWeekDay;

		private Label _lblWeeklyRecurEvery;

		private TextBox _txtWeeklyInterval;

		private Label _lblWeeklyWeeksOn;

		private RadioButton _rdbMonthly;

		private GroupBox _grbMonthlyPattern;

		private Label _lblMonthNthOfEvery;

		private ComboBox _cboMonthlyDayWeek;

		private TextBox _txtMonthlyInterval;

		private Label _lblDayOfMonthOfEvery;

		private TextBox _txtDayOfMonth;

		private RadioButton _rdbDayOfMonth;

		private RadioButton _rdbMonthNth;

		private Label _lblMonthNthMonths;

		private Label _lblDayOfMonthMonths;

		private TextBox _txtMonthNthInterval;

		private GroupBox _grbAppointmentTime;

		private ComboBox _cboMonthlyOccurrence;

		private RadioButton _rdbYearly;

		private Label _lblOccurrences;

		private ComboBox _cboYearlyOccurrence;

		private TextBox _txtYearlyDayOfMonth;

		private Label _lblYearlyOf;

		private RadioButton _rdbMonthOfYear;

		private ComboBox _cboYearlyDayOfWeek;

		private ComboBox _cboMonthOfYear;

		private ComboBox _cboMonthOfYearNth;

		private Label _lblDailyDays;

		private GroupBox _grbYearlyPattern;

		private RadioButton _rdbYearNth;

		private Button _btnRemoveRecurrence;

		private RadioButton _rdbWeekendDay;

		private Container _container = new Container();

		private WorkingHourRecurrencePattern _workingHourRecurrencePattern;

		private bool _isLoadingPattern;

		public RecurrenceForm()
		{
			InitializeControls();
			Mordex.Windows.Common.ControlThemer.AttachDialog(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeControls()
		{
			_grbAppointmentTime = new GroupBox();
			_dtpStartTime = new DateTimePicker();
			_label = new Label();
			_lblEndTime = new Label();
			_dtpEndTime = new DateTimePicker();
			_dtpRecurrenceEndDate = new DateTimePicker();
			_dtpRecurrenceStart = new DateTimePicker();
			_grbPattern = new GroupBox();
			_grbWeeklyPattern = new GroupBox();
			_chkMonday = new CheckBox();
			_txtWeeklyInterval = new TextBox();
			_chkTuesday = new CheckBox();
			_chkWednesday = new CheckBox();
			_chkSunday = new CheckBox();
			_chkThursday = new CheckBox();
			_chkSaturday = new CheckBox();
			_chkFriday = new CheckBox();
			_lblWeeklyRecurEvery = new Label();
			_lblWeeklyWeeksOn = new Label();
			_rdbWeekly = new RadioButton();
			_rdbDaily = new RadioButton();
			_rdbMonthly = new RadioButton();
			_rdbYearly = new RadioButton();
			_grbMonthlyPattern = new GroupBox();
			_txtMonthNthInterval = new TextBox();
			_lblMonthNthOfEvery = new Label();
			_cboMonthlyDayWeek = new ComboBox();
			_cboMonthlyOccurrence = new ComboBox();
			_txtMonthlyInterval = new TextBox();
			_lblDayOfMonthOfEvery = new Label();
			_txtDayOfMonth = new TextBox();
			_rdbDayOfMonth = new RadioButton();
			_rdbMonthNth = new RadioButton();
			_lblMonthNthMonths = new Label();
			_lblDayOfMonthMonths = new Label();
			_grbYearlyPattern = new GroupBox();
			_cboYearlyOccurrence = new ComboBox();
			_txtYearlyDayOfMonth = new TextBox();
			_lblYearlyOf = new Label();
			_rdbMonthOfYear = new RadioButton();
			_cboYearlyDayOfWeek = new ComboBox();
			_rdbYearNth = new RadioButton();
			_cboMonthOfYear = new ComboBox();
			_cboMonthOfYearNth = new ComboBox();
			_grbDailyPattern = new GroupBox();
			_txtDailyInterval = new TextBox();
			_rdbDailyInterval = new RadioButton();
			_rdbWeekDay = new RadioButton();
			_lblDailyDays = new Label();
			_rdbWeekendDay = new RadioButton();
			_btnOK = new Button();
			_btnCancel = new Button();
			_grbRecurrenceRange = new GroupBox();
			_txtOccurrences = new TextBox();
			_rdbNoEndDate = new RadioButton();
			_lblRangeStart = new Label();
			_rdbOccurrences = new RadioButton();
			_rdbPatternEndDate = new RadioButton();
			_lblOccurrences = new Label();
			_btnRemoveRecurrence = new Button();
			_grbAppointmentTime.SuspendLayout();
			_grbPattern.SuspendLayout();
			_grbWeeklyPattern.SuspendLayout();
			_grbMonthlyPattern.SuspendLayout();
			_grbYearlyPattern.SuspendLayout();
			_grbDailyPattern.SuspendLayout();
			_grbRecurrenceRange.SuspendLayout();
			SuspendLayout();
			AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			_grbAppointmentTime.Controls.Add(_dtpStartTime);
			_grbAppointmentTime.Controls.Add(_label);
			_grbAppointmentTime.Controls.Add(_lblEndTime);
			_grbAppointmentTime.Controls.Add(_dtpEndTime);
			_grbAppointmentTime.FlatStyle = FlatStyle.System;
			_grbAppointmentTime.Location = new Point(8, 8);
			_grbAppointmentTime.Name = "grbAppointmentTime";
			_grbAppointmentTime.Size = new Size(472, 48);
			_grbAppointmentTime.TabIndex = 0;
			_grbAppointmentTime.TabStop = false;
			_grbAppointmentTime.Text = "Appointment Time:";
			_dtpStartTime.CustomFormat = "hh:mm tt";
			_dtpStartTime.Format = DateTimePickerFormat.Custom;
			_dtpStartTime.Location = new Point(45, 20);
			_dtpStartTime.Name = "dtpStartTime";
			_dtpStartTime.ShowUpDown = true;
			_dtpStartTime.Size = new Size(100, 21);
			_dtpStartTime.TabIndex = 3;
			_dtpStartTime.Value = new DateTime(2004, 2, 15, 0, 0, 0, 0);
			_label.AutoSize = true;
			_label.Location = new Point(8, 24);
			_label.Name = "label1";
			_label.Size = new Size(31, 13);
			_label.TabIndex = 1;
			_label.Text = "Start:";
			_lblEndTime.AutoSize = true;
			_lblEndTime.Location = new Point(166, 24);
			_lblEndTime.Name = "label2";
			_lblEndTime.Size = new Size(25, 13);
			_lblEndTime.TabIndex = 1;
			_lblEndTime.Text = "End:";
			_dtpEndTime.CustomFormat = "hh:mm tt";
			_dtpEndTime.Format = DateTimePickerFormat.Custom;
			_dtpEndTime.Location = new Point(198, 20);
			_dtpEndTime.Name = "dtpEndTime";
			_dtpEndTime.ShowUpDown = true;
			_dtpEndTime.Size = new Size(100, 21);
			_dtpEndTime.TabIndex = 1;
			_dtpEndTime.Value = new DateTime(2004, 2, 15, 0, 0, 0, 0);
			_dtpRecurrenceEndDate.Enabled = false;
			_dtpRecurrenceEndDate.Format = DateTimePickerFormat.Short;
			_dtpRecurrenceEndDate.Location = new Point(292, 78);
			_dtpRecurrenceEndDate.Name = "dtpRecurrenceEndDate";
			_dtpRecurrenceEndDate.Size = new Size(172, 21);
			_dtpRecurrenceEndDate.TabIndex = 0;
			_dtpRecurrenceEndDate.Value = new DateTime(2004, 2, 15, 0, 0, 0, 0);
			_dtpRecurrenceStart.Format = DateTimePickerFormat.Short;
			_dtpRecurrenceStart.Location = new Point(44, 20);
			_dtpRecurrenceStart.Name = "dtpRecurrenceStart";
			_dtpRecurrenceStart.Size = new Size(156, 21);
			_dtpRecurrenceStart.TabIndex = 3;
			_dtpRecurrenceStart.Value = new DateTime(2004, 2, 15, 0, 0, 0, 0);
			_dtpRecurrenceStart.ValueChanged += OnDtpRecurrenceStartValueChanged;
			_grbPattern.Controls.Add(_grbWeeklyPattern);
			_grbPattern.Controls.Add(_rdbWeekly);
			_grbPattern.Controls.Add(_rdbDaily);
			_grbPattern.Controls.Add(_rdbMonthly);
			_grbPattern.Controls.Add(_rdbYearly);
			_grbPattern.Controls.Add(_grbMonthlyPattern);
			_grbPattern.Controls.Add(_grbYearlyPattern);
			_grbPattern.Controls.Add(_grbDailyPattern);
			_grbPattern.FlatStyle = FlatStyle.System;
			_grbPattern.Location = new Point(8, 64);
			_grbPattern.Name = "grbPattern";
			_grbPattern.Size = new Size(472, 120);
			_grbPattern.TabIndex = 1;
			_grbPattern.TabStop = false;
			_grbPattern.Text = "Recurrence Pattern";
			_grbWeeklyPattern.Controls.Add(_chkMonday);
			_grbWeeklyPattern.Controls.Add(_txtWeeklyInterval);
			_grbWeeklyPattern.Controls.Add(_chkTuesday);
			_grbWeeklyPattern.Controls.Add(_chkWednesday);
			_grbWeeklyPattern.Controls.Add(_chkSunday);
			_grbWeeklyPattern.Controls.Add(_chkThursday);
			_grbWeeklyPattern.Controls.Add(_chkSaturday);
			_grbWeeklyPattern.Controls.Add(_chkFriday);
			_grbWeeklyPattern.Controls.Add(_lblWeeklyRecurEvery);
			_grbWeeklyPattern.Controls.Add(_lblWeeklyWeeksOn);
			_grbWeeklyPattern.FlatStyle = FlatStyle.System;
			_grbWeeklyPattern.Location = new Point(100, 12);
			_grbWeeklyPattern.Name = "grbWeeklyPattern";
			_grbWeeklyPattern.Size = new Size(364, 100);
			_grbWeeklyPattern.TabIndex = 5;
			_grbWeeklyPattern.TabStop = false;
			_chkMonday.FlatStyle = FlatStyle.System;
			_chkMonday.Location = new Point(96, 48);
			_chkMonday.Name = "chkMonday";
			_chkMonday.Size = new Size(64, 16);
			_chkMonday.TabIndex = 3;
			_chkMonday.Text = "Monday";
			_txtWeeklyInterval.Location = new Point(84, 16);
			_txtWeeklyInterval.Name = "txtWeeklyInterval";
			_txtWeeklyInterval.Size = new Size(32, 21);
			_txtWeeklyInterval.TabIndex = 2;
			_txtWeeklyInterval.Text = "1";
			_txtWeeklyInterval.TextAlign = HorizontalAlignment.Right;
			_chkTuesday.FlatStyle = FlatStyle.System;
			_chkTuesday.Location = new Point(184, 48);
			_chkTuesday.Name = "chkTuesday";
			_chkTuesday.Size = new Size(64, 16);
			_chkTuesday.TabIndex = 3;
			_chkTuesday.Text = "Tuesday";
			_chkWednesday.FlatStyle = FlatStyle.System;
			_chkWednesday.Location = new Point(272, 48);
			_chkWednesday.Name = "chkWednesday";
			_chkWednesday.Size = new Size(80, 16);
			_chkWednesday.TabIndex = 3;
			_chkWednesday.Text = "Wednesday";
			_chkSunday.FlatStyle = FlatStyle.System;
			_chkSunday.Location = new Point(8, 48);
			_chkSunday.Name = "chkSunday";
			_chkSunday.Size = new Size(64, 16);
			_chkSunday.TabIndex = 3;
			_chkSunday.Text = "Sunday";
			_chkThursday.FlatStyle = FlatStyle.System;
			_chkThursday.Location = new Point(8, 72);
			_chkThursday.Name = "chkThursday";
			_chkThursday.Size = new Size(72, 16);
			_chkThursday.TabIndex = 3;
			_chkThursday.Text = "Thursday";
			_chkSaturday.FlatStyle = FlatStyle.System;
			_chkSaturday.Location = new Point(184, 72);
			_chkSaturday.Name = "chkSaturday";
			_chkSaturday.Size = new Size(68, 16);
			_chkSaturday.TabIndex = 3;
			_chkSaturday.Text = "Saturday";
			_chkFriday.FlatStyle = FlatStyle.System;
			_chkFriday.Location = new Point(96, 72);
			_chkFriday.Name = "chkFriday";
			_chkFriday.Size = new Size(56, 16);
			_chkFriday.TabIndex = 3;
			_chkFriday.Text = "Friday";
			_lblWeeklyRecurEvery.AutoSize = true;
			_lblWeeklyRecurEvery.Location = new Point(8, 20);
			_lblWeeklyRecurEvery.Name = "label4";
			_lblWeeklyRecurEvery.Size = new Size(66, 13);
			_lblWeeklyRecurEvery.TabIndex = 4;
			_lblWeeklyRecurEvery.Text = "Recurr every";
			_lblWeeklyWeeksOn.AutoSize = true;
			_lblWeeklyWeeksOn.Location = new Point(124, 20);
			_lblWeeklyWeeksOn.Name = "label9";
			_lblWeeklyWeeksOn.Size = new Size(60, 13);
			_lblWeeklyWeeksOn.TabIndex = 4;
			_lblWeeklyWeeksOn.Text = "week(s) on:";
			_rdbWeekly.Checked = true;
			_rdbWeekly.FlatStyle = FlatStyle.System;
			_rdbWeekly.Location = new Point(8, 43);
			_rdbWeekly.Name = "rdbWeekly";
			_rdbWeekly.Size = new Size(64, 16);
			_rdbWeekly.TabIndex = 3;
			_rdbWeekly.Text = "Weekly";
			_rdbWeekly.CheckedChanged += OnRdbWeeklyCheckedChanged;
			_rdbDaily.FlatStyle = FlatStyle.System;
			_rdbDaily.Location = new Point(8, 20);
			_rdbDaily.Name = "rdbDaily";
			_rdbDaily.Size = new Size(64, 16);
			_rdbDaily.TabIndex = 0;
			_rdbDaily.Text = "Daily";
			_rdbDaily.CheckedChanged += OnRdbDailyCheckedChanged;
			_rdbMonthly.FlatStyle = FlatStyle.System;
			_rdbMonthly.Location = new Point(8, 66);
			_rdbMonthly.Name = "rdbMonthly";
			_rdbMonthly.Size = new Size(64, 16);
			_rdbMonthly.TabIndex = 5;
			_rdbMonthly.Text = "Monthly";
			_rdbMonthly.CheckedChanged += OnRdbMonthlyCheckedChanged;
			_rdbYearly.FlatStyle = FlatStyle.System;
			_rdbYearly.Location = new Point(8, 89);
			_rdbYearly.Name = "rdbYearly";
			_rdbYearly.Size = new Size(64, 16);
			_rdbYearly.TabIndex = 5;
			_rdbYearly.Text = "Yearly";
			_rdbYearly.CheckedChanged += OnRdbYearlyCheckedChanged;
			_grbMonthlyPattern.Controls.Add(_txtMonthNthInterval);
			_grbMonthlyPattern.Controls.Add(_lblMonthNthOfEvery);
			_grbMonthlyPattern.Controls.Add(_cboMonthlyDayWeek);
			_grbMonthlyPattern.Controls.Add(_cboMonthlyOccurrence);
			_grbMonthlyPattern.Controls.Add(_txtMonthlyInterval);
			_grbMonthlyPattern.Controls.Add(_lblDayOfMonthOfEvery);
			_grbMonthlyPattern.Controls.Add(_txtDayOfMonth);
			_grbMonthlyPattern.Controls.Add(_rdbDayOfMonth);
			_grbMonthlyPattern.Controls.Add(_rdbMonthNth);
			_grbMonthlyPattern.Controls.Add(_lblMonthNthMonths);
			_grbMonthlyPattern.Controls.Add(_lblDayOfMonthMonths);
			_grbMonthlyPattern.FlatStyle = FlatStyle.System;
			_grbMonthlyPattern.Location = new Point(100, 12);
			_grbMonthlyPattern.Name = "grbMonthlyPattern";
			_grbMonthlyPattern.Size = new Size(364, 100);
			_grbMonthlyPattern.TabIndex = 6;
			_grbMonthlyPattern.TabStop = false;
			_grbMonthlyPattern.Visible = false;
			_txtMonthNthInterval.Location = new Point(268, 50);
			_txtMonthNthInterval.Name = "txtMonthNthInterval";
			_txtMonthNthInterval.Size = new Size(32, 21);
			_txtMonthNthInterval.TabIndex = 2;
			_txtMonthNthInterval.Text = "1";
			_txtMonthNthInterval.TextAlign = HorizontalAlignment.Right;
			_lblMonthNthOfEvery.AutoSize = true;
			_lblMonthNthOfEvery.FlatStyle = FlatStyle.System;
			_lblMonthNthOfEvery.Location = new Point(224, 54);
			_lblMonthNthOfEvery.Name = "label6";
			_lblMonthNthOfEvery.Size = new Size(44, 13);
			_lblMonthNthOfEvery.TabIndex = 6;
			_lblMonthNthOfEvery.Text = "of every";
			_cboMonthlyDayWeek.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboMonthlyDayWeek.FormattingEnabled = true;
			_cboMonthlyDayWeek.Location = new Point(124, 50);
			_cboMonthlyDayWeek.Name = "cboMonthlyDayWeek";
			_cboMonthlyDayWeek.Size = new Size(96, 21);
			_cboMonthlyDayWeek.TabIndex = 5;
			_cboMonthlyOccurrence.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboMonthlyOccurrence.FormattingEnabled = true;
			_cboMonthlyOccurrence.Location = new Point(48, 50);
			_cboMonthlyOccurrence.Name = "cboMonthlyOccurrence";
			_cboMonthlyOccurrence.Size = new Size(72, 21);
			_cboMonthlyOccurrence.TabIndex = 5;
			_txtMonthlyInterval.Location = new Point(144, 18);
			_txtMonthlyInterval.Name = "txtMonthlyInterval";
			_txtMonthlyInterval.Size = new Size(32, 21);
			_txtMonthlyInterval.TabIndex = 2;
			_txtMonthlyInterval.Text = "1";
			_txtMonthlyInterval.TextAlign = HorizontalAlignment.Right;
			_lblDayOfMonthOfEvery.AutoSize = true;
			_lblDayOfMonthOfEvery.FlatStyle = FlatStyle.System;
			_lblDayOfMonthOfEvery.Location = new Point(96, 22);
			_lblDayOfMonthOfEvery.Name = "label5";
			_lblDayOfMonthOfEvery.Size = new Size(44, 13);
			_lblDayOfMonthOfEvery.TabIndex = 4;
			_lblDayOfMonthOfEvery.Text = "of every";
			_txtDayOfMonth.Location = new Point(56, 18);
			_txtDayOfMonth.Name = "txtDayOfMonth";
			_txtDayOfMonth.Size = new Size(32, 21);
			_txtDayOfMonth.TabIndex = 2;
			_txtDayOfMonth.Text = "1";
			_txtDayOfMonth.TextAlign = HorizontalAlignment.Right;
			_rdbDayOfMonth.Checked = true;
			_rdbDayOfMonth.FlatStyle = FlatStyle.System;
			_rdbDayOfMonth.Location = new Point(8, 20);
			_rdbDayOfMonth.Name = "rdbDayOfMonth";
			_rdbDayOfMonth.Size = new Size(48, 16);
			_rdbDayOfMonth.TabIndex = 1;
			_rdbDayOfMonth.Text = "Day";
			_rdbMonthNth.FlatStyle = FlatStyle.System;
			_rdbMonthNth.Location = new Point(8, 52);
			_rdbMonthNth.Name = "rdbMonthNth";
			_rdbMonthNth.Size = new Size(44, 16);
			_rdbMonthNth.TabIndex = 1;
			_rdbMonthNth.Text = "The";
			_lblMonthNthMonths.AutoSize = true;
			_lblMonthNthMonths.FlatStyle = FlatStyle.System;
			_lblMonthNthMonths.Location = new Point(304, 54);
			_lblMonthNthMonths.Name = "label7";
			_lblMonthNthMonths.Size = new Size(46, 13);
			_lblMonthNthMonths.TabIndex = 6;
			_lblMonthNthMonths.Text = "month(s)";
			_lblDayOfMonthMonths.AutoSize = true;
			_lblDayOfMonthMonths.FlatStyle = FlatStyle.System;
			_lblDayOfMonthMonths.Location = new Point(180, 22);
			_lblDayOfMonthMonths.Name = "label8";
			_lblDayOfMonthMonths.Size = new Size(46, 13);
			_lblDayOfMonthMonths.TabIndex = 6;
			_lblDayOfMonthMonths.Text = "month(s)";
			_grbYearlyPattern.Controls.Add(_cboYearlyOccurrence);
			_grbYearlyPattern.Controls.Add(_txtYearlyDayOfMonth);
			_grbYearlyPattern.Controls.Add(_lblYearlyOf);
			_grbYearlyPattern.Controls.Add(_rdbMonthOfYear);
			_grbYearlyPattern.Controls.Add(_cboYearlyDayOfWeek);
			_grbYearlyPattern.Controls.Add(_rdbYearNth);
			_grbYearlyPattern.Controls.Add(_cboMonthOfYear);
			_grbYearlyPattern.Controls.Add(_cboMonthOfYearNth);
			_grbYearlyPattern.FlatStyle = FlatStyle.System;
			_grbYearlyPattern.Location = new Point(100, 12);
			_grbYearlyPattern.Name = "grbYearlyPattern";
			_grbYearlyPattern.Size = new Size(364, 100);
			_grbYearlyPattern.TabIndex = 5;
			_grbYearlyPattern.TabStop = false;
			_grbYearlyPattern.Visible = false;
			_cboYearlyOccurrence.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboYearlyOccurrence.FormattingEnabled = true;
			_cboYearlyOccurrence.Location = new Point(54, 50);
			_cboYearlyOccurrence.Name = "cboYearlyOccurrence";
			_cboYearlyOccurrence.Size = new Size(76, 21);
			_cboYearlyOccurrence.TabIndex = 5;
			_txtYearlyDayOfMonth.Location = new Point(176, 18);
			_txtYearlyDayOfMonth.Name = "txtYearlyDayOfMonth";
			_txtYearlyDayOfMonth.Size = new Size(32, 21);
			_txtYearlyDayOfMonth.TabIndex = 2;
			_txtYearlyDayOfMonth.Text = "1";
			_txtYearlyDayOfMonth.TextAlign = HorizontalAlignment.Right;
			_lblYearlyOf.AutoSize = true;
			_lblYearlyOf.FlatStyle = FlatStyle.System;
			_lblYearlyOf.Location = new Point(234, 54);
			_lblYearlyOf.Name = "label14";
			_lblYearlyOf.Size = new Size(13, 13);
			_lblYearlyOf.TabIndex = 6;
			_lblYearlyOf.Text = "of";
			_rdbMonthOfYear.Checked = true;
			_rdbMonthOfYear.FlatStyle = FlatStyle.System;
			_rdbMonthOfYear.Location = new Point(8, 20);
			_rdbMonthOfYear.Name = "rdbMonthOfYear";
			_rdbMonthOfYear.Size = new Size(48, 16);
			_rdbMonthOfYear.TabIndex = 1;
			_rdbMonthOfYear.Text = "Every";
			_cboYearlyDayOfWeek.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboYearlyDayOfWeek.FormattingEnabled = true;
			_cboYearlyDayOfWeek.Location = new Point(132, 50);
			_cboYearlyDayOfWeek.Name = "cboYearlyDayOfWeek";
			_cboYearlyDayOfWeek.Size = new Size(100, 21);
			_cboYearlyDayOfWeek.TabIndex = 5;
			_rdbYearNth.FlatStyle = FlatStyle.System;
			_rdbYearNth.Location = new Point(8, 52);
			_rdbYearNth.Name = "rdbYearNth";
			_rdbYearNth.Size = new Size(44, 16);
			_rdbYearNth.TabIndex = 1;
			_rdbYearNth.Text = "The";
			_cboMonthOfYear.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboMonthOfYear.FormattingEnabled = true;
			_cboMonthOfYear.Location = new Point(64, 18);
			_cboMonthOfYear.Name = "cboMonthOfYear";
			_cboMonthOfYear.Size = new Size(104, 21);
			_cboMonthOfYear.TabIndex = 5;
			_cboMonthOfYearNth.DropDownStyle = ComboBoxStyle.DropDownList;
			_cboMonthOfYearNth.FormattingEnabled = true;
			_cboMonthOfYearNth.Location = new Point(250, 50);
			_cboMonthOfYearNth.Name = "cboMonthOfYearNth";
			_cboMonthOfYearNth.Size = new Size(104, 21);
			_cboMonthOfYearNth.TabIndex = 5;
			_grbDailyPattern.Controls.Add(_txtDailyInterval);
			_grbDailyPattern.Controls.Add(_rdbDailyInterval);
			_grbDailyPattern.Controls.Add(_rdbWeekDay);
			_grbDailyPattern.Controls.Add(_lblDailyDays);
			_grbDailyPattern.Controls.Add(_rdbWeekendDay);
			_grbDailyPattern.FlatStyle = FlatStyle.System;
			_grbDailyPattern.Location = new Point(100, 12);
			_grbDailyPattern.Name = "grbDailyPattern";
			_grbDailyPattern.Size = new Size(364, 100);
			_grbDailyPattern.TabIndex = 4;
			_grbDailyPattern.TabStop = false;
			_grbDailyPattern.Visible = false;
			_txtDailyInterval.Location = new Point(64, 18);
			_txtDailyInterval.Name = "txtDailyInterval";
			_txtDailyInterval.Size = new Size(32, 21);
			_txtDailyInterval.TabIndex = 2;
			_txtDailyInterval.Text = "1";
			_txtDailyInterval.TextAlign = HorizontalAlignment.Right;
			_rdbDailyInterval.Checked = true;
			_rdbDailyInterval.FlatStyle = FlatStyle.System;
			_rdbDailyInterval.Location = new Point(8, 20);
			_rdbDailyInterval.Name = "rdbDailyInterval";
			_rdbDailyInterval.Size = new Size(48, 16);
			_rdbDailyInterval.TabIndex = 1;
			_rdbDailyInterval.Text = "Every";
			_rdbWeekDay.FlatStyle = FlatStyle.System;
			_rdbWeekDay.Location = new Point(8, 46);
			_rdbWeekDay.Name = "rdbWeekDay";
			_rdbWeekDay.Size = new Size(100, 16);
			_rdbWeekDay.TabIndex = 1;
			_rdbWeekDay.Text = "Every weekday";
			_lblDailyDays.AutoSize = true;
			_lblDailyDays.Location = new Point(104, 22);
			_lblDailyDays.Name = "label11";
			_lblDailyDays.Size = new Size(34, 13);
			_lblDailyDays.TabIndex = 4;
			_lblDailyDays.Text = "day(s)";
			_rdbWeekendDay.FlatStyle = FlatStyle.System;
			_rdbWeekendDay.Location = new Point(8, 72);
			_rdbWeekendDay.Name = "rdbWeekendDay";
			_rdbWeekendDay.Size = new Size(124, 16);
			_rdbWeekendDay.TabIndex = 1;
			_rdbWeekendDay.Text = "Every weekend day";
			_btnOK.DialogResult = DialogResult.OK;
			_btnOK.FlatStyle = FlatStyle.System;
			_btnOK.Location = new Point(93, 308);
			_btnOK.Name = "btnOK";
			_btnOK.Size = new Size(86, 24);
			_btnOK.TabIndex = 2;
			_btnOK.Text = "OK";
			_btnOK.Click += OnBtnOKClick;
			_btnCancel.DialogResult = DialogResult.Cancel;
			_btnCancel.FlatStyle = FlatStyle.System;
			_btnCancel.Location = new Point(186, 308);
			_btnCancel.Name = "btnCancel";
			_btnCancel.Size = new Size(86, 24);
			_btnCancel.TabIndex = 3;
			_btnCancel.Text = "Cancel";
			_btnCancel.Click += OnBtnCancelClick;
			_grbRecurrenceRange.Controls.Add(_txtOccurrences);
			_grbRecurrenceRange.Controls.Add(_rdbNoEndDate);
			_grbRecurrenceRange.Controls.Add(_dtpRecurrenceStart);
			_grbRecurrenceRange.Controls.Add(_lblRangeStart);
			_grbRecurrenceRange.Controls.Add(_rdbOccurrences);
			_grbRecurrenceRange.Controls.Add(_rdbPatternEndDate);
			_grbRecurrenceRange.Controls.Add(_dtpRecurrenceEndDate);
			_grbRecurrenceRange.Controls.Add(_lblOccurrences);
			_grbRecurrenceRange.FlatStyle = FlatStyle.System;
			_grbRecurrenceRange.Location = new Point(8, 192);
			_grbRecurrenceRange.Name = "grbRecurrenceRange";
			_grbRecurrenceRange.Size = new Size(472, 108);
			_grbRecurrenceRange.TabIndex = 4;
			_grbRecurrenceRange.TabStop = false;
			_grbRecurrenceRange.Text = "Range of Recurrence";
			_txtOccurrences.Location = new Point(292, 49);
			_txtOccurrences.Name = "txtOccurrences";
			_txtOccurrences.Size = new Size(32, 21);
			_txtOccurrences.TabIndex = 2;
			_txtOccurrences.Text = "10";
			_txtOccurrences.TextAlign = HorizontalAlignment.Right;
			_rdbNoEndDate.Checked = true;
			_rdbNoEndDate.FlatStyle = FlatStyle.System;
			_rdbNoEndDate.Location = new Point(208, 22);
			_rdbNoEndDate.Name = "rdbNoEndDate";
			_rdbNoEndDate.Size = new Size(88, 16);
			_rdbNoEndDate.TabIndex = 2;
			_rdbNoEndDate.Text = "No end date";
			_lblRangeStart.AutoSize = true;
			_lblRangeStart.FlatStyle = FlatStyle.System;
			_lblRangeStart.Location = new Point(8, 24);
			_lblRangeStart.Name = "label3";
			_lblRangeStart.Size = new Size(31, 13);
			_lblRangeStart.TabIndex = 1;
			_lblRangeStart.Text = "Start:";
			_rdbOccurrences.FlatStyle = FlatStyle.System;
			_rdbOccurrences.Location = new Point(208, 51);
			_rdbOccurrences.Name = "rdbOccurrences";
			_rdbOccurrences.Size = new Size(72, 16);
			_rdbOccurrences.TabIndex = 2;
			_rdbOccurrences.Text = "End after:";
			_rdbPatternEndDate.FlatStyle = FlatStyle.System;
			_rdbPatternEndDate.Location = new Point(208, 80);
			_rdbPatternEndDate.Name = "rdbPatternEndDate";
			_rdbPatternEndDate.Size = new Size(72, 16);
			_rdbPatternEndDate.TabIndex = 2;
			_rdbPatternEndDate.Text = "End date:";
			_rdbPatternEndDate.CheckedChanged += OnRdbPatternEndDateCheckedChanged;
			_lblOccurrences.AutoSize = true;
			_lblOccurrences.Location = new Point(332, 53);
			_lblOccurrences.Name = "label10";
			_lblOccurrences.Size = new Size(61, 13);
			_lblOccurrences.TabIndex = 4;
			_lblOccurrences.Text = "occurrences";
			_btnRemoveRecurrence.DialogResult = DialogResult.OK;
			_btnRemoveRecurrence.FlatStyle = FlatStyle.System;
			_btnRemoveRecurrence.Location = new Point(279, 308);
			_btnRemoveRecurrence.Name = "btnRemoveRecurrence";
			_btnRemoveRecurrence.Size = new Size(116, 24);
			_btnRemoveRecurrence.TabIndex = 5;
			_btnRemoveRecurrence.Text = "Remove Recurrence";
			_btnRemoveRecurrence.Click += OnBtnRemoveRecurrenceClick;
			base.ClientSize = new Size(488, 342);
			base.Controls.Add(_btnRemoveRecurrence);
			base.Controls.Add(_grbRecurrenceRange);
			base.Controls.Add(_btnCancel);
			base.Controls.Add(_btnOK);
			base.Controls.Add(_grbPattern);
			base.Controls.Add(_grbAppointmentTime);
			Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "RecurrenceForm";
			base.StartPosition = FormStartPosition.CenterParent;
			Text = "Appointment Recurrence";
			_grbAppointmentTime.ResumeLayout(performLayout: false);
			_grbAppointmentTime.PerformLayout();
			_grbPattern.ResumeLayout(performLayout: false);
			_grbWeeklyPattern.ResumeLayout(performLayout: false);
			_grbWeeklyPattern.PerformLayout();
			_grbMonthlyPattern.ResumeLayout(performLayout: false);
			_grbMonthlyPattern.PerformLayout();
			_grbYearlyPattern.ResumeLayout(performLayout: false);
			_grbYearlyPattern.PerformLayout();
			_grbDailyPattern.ResumeLayout(performLayout: false);
			_grbDailyPattern.PerformLayout();
			_grbRecurrenceRange.ResumeLayout(performLayout: false);
			_grbRecurrenceRange.PerformLayout();
			ResumeLayout(performLayout: false);
		}

		internal WorkingHourRecurrencePattern GetWorkingHourRecurrencePattern()
		{
			return _workingHourRecurrencePattern;
		}

		internal void SetWorkingHourRecurrencePattern(WorkingHourRecurrencePattern workingHourRecurrencePattern)
		{
			_workingHourRecurrencePattern = workingHourRecurrencePattern;
		}

		internal void ShowPatternDialog(WorkingHourRecurrencePattern workingHourRecurrencePattern)
		{
			FillDayAndMonthLists();
			if (workingHourRecurrencePattern != null)
			{
				_workingHourRecurrencePattern = workingHourRecurrencePattern;
			}
			else
			{
				_workingHourRecurrencePattern = new WorkingHourRecurrencePattern();
			}
			LoadPatternIntoControls(workingHourRecurrencePattern);
			ShowDialog();
		}

		private SchedulerDayOfWeek GetCheckedWeekDays()
		{
			SchedulerDayOfWeek schedulerDayOfWeek = (SchedulerDayOfWeek)0;
			if (_chkMonday.Checked)
			{
				schedulerDayOfWeek |= SchedulerDayOfWeek.Monday;
			}
			if (_chkTuesday.Checked)
			{
				schedulerDayOfWeek |= SchedulerDayOfWeek.Tuesday;
			}
			if (_chkWednesday.Checked)
			{
				schedulerDayOfWeek |= SchedulerDayOfWeek.Wednesday;
			}
			if (_chkThursday.Checked)
			{
				schedulerDayOfWeek |= SchedulerDayOfWeek.Thursday;
			}
			if (_chkFriday.Checked)
			{
				schedulerDayOfWeek |= SchedulerDayOfWeek.Friday;
			}
			if (_chkSaturday.Checked)
			{
				schedulerDayOfWeek |= SchedulerDayOfWeek.Saturday;
			}
			if (_chkSunday.Checked)
			{
				schedulerDayOfWeek |= SchedulerDayOfWeek.Sunday;
			}
			return schedulerDayOfWeek;
		}

		private void UpdateRecurrenceForm(WorkingHourRecurrencePattern pattern)
		{
			if ((pattern.DayOfWeek & SchedulerDayOfWeek.Monday) != 0)
			{
				_chkMonday.Checked = true;
			}
			else
			{
				_chkMonday.Checked = false;
			}
			if ((pattern.DayOfWeek & SchedulerDayOfWeek.Tuesday) != 0)
			{
				_chkTuesday.Checked = true;
			}
			else
			{
				_chkTuesday.Checked = false;
			}
			if ((pattern.DayOfWeek & SchedulerDayOfWeek.Wednesday) != 0)
			{
				_chkWednesday.Checked = true;
			}
			else
			{
				_chkWednesday.Checked = false;
			}
			if ((pattern.DayOfWeek & SchedulerDayOfWeek.Thursday) != 0)
			{
				_chkThursday.Checked = true;
			}
			else
			{
				_chkThursday.Checked = false;
			}
			if ((pattern.DayOfWeek & SchedulerDayOfWeek.Friday) != 0)
			{
				_chkFriday.Checked = true;
			}
			else
			{
				_chkFriday.Checked = false;
			}
			if ((pattern.DayOfWeek & SchedulerDayOfWeek.Saturday) != 0)
			{
				_chkSaturday.Checked = true;
			}
			else
			{
				_chkSaturday.Checked = false;
			}
			if ((pattern.DayOfWeek & SchedulerDayOfWeek.Sunday) != 0)
			{
				_chkSunday.Checked = true;
			}
			else
			{
				_chkSunday.Checked = false;
			}
		}

		private void LoadPatternIntoControls(WorkingHourRecurrencePattern workingHourRecurrencePattern)
		{
			// The change handlers below would otherwise replace the loaded values with defaults.
			_isLoadingPattern = true;
			try
			{
				FillControlsFromPattern(workingHourRecurrencePattern);
			}
			finally
			{
				_isLoadingPattern = false;
			}
		}

		private void FillControlsFromPattern(WorkingHourRecurrencePattern workingHourRecurrencePattern)
		{
			if (workingHourRecurrencePattern != null)
			{
				_btnRemoveRecurrence.Enabled = true;
				if (workingHourRecurrencePattern.PatternEndDate != DateTime.MaxValue)
				{
					_dtpRecurrenceEndDate.Value = workingHourRecurrencePattern.PatternEndDate;
				}
				_dtpRecurrenceStart.Value = workingHourRecurrencePattern.PatternStartDate;
				_dtpStartTime.Value = _dtpRecurrenceStart.Value.Date.Add(workingHourRecurrencePattern.StartTime);
				_dtpEndTime.Value = _dtpRecurrenceStart.Value.Date.Add(workingHourRecurrencePattern.EndTime);
				_txtOccurrences.Text = workingHourRecurrencePattern.Occurrences.ToString();
				switch (workingHourRecurrencePattern.RecurrenceType)
				{
				case RecurrenceType.Daily:
					_rdbDaily.Checked = true;
					break;
				case RecurrenceType.Weekly:
					_rdbWeekly.Checked = true;
					UpdateRecurrenceForm(workingHourRecurrencePattern);
					_txtWeeklyInterval.Text = workingHourRecurrencePattern.Interval.ToString();
					break;
				case RecurrenceType.Monthly:
					_rdbMonthly.Checked = true;
					break;
				case RecurrenceType.MontNth:
					_rdbMonthly.Checked = true;
					_rdbMonthNth.Checked = true;
					break;
				case RecurrenceType.Yearly:
					_rdbYearly.Checked = true;
					break;
				case RecurrenceType.YearNth:
					_rdbYearly.Checked = true;
					_rdbYearNth.Checked = true;
					break;
				}
				if (workingHourRecurrencePattern.RecurrenceEndMode == RecurrenceEndMode.EndDate)
				{
					_rdbPatternEndDate.Checked = true;
				}
				else if (workingHourRecurrencePattern.RecurrenceEndMode == RecurrenceEndMode.Occurrences)
				{
					_rdbOccurrences.Checked = true;
				}
			}
			else
			{
				_btnRemoveRecurrence.Enabled = false;
				_dtpStartTime.Value = DateTime.Today;
				_dtpEndTime.Value = DateTime.Today;
				_dtpRecurrenceStart.Value = DateTime.Today;
				_dtpRecurrenceEndDate.Value = _dtpRecurrenceStart.Value.AddDays(63.0);
				_workingHourRecurrencePattern.SetDefaultValuesForDate(DateTime.Today);
				UpdateRecurrenceForm(_workingHourRecurrencePattern);
				_rdbWeekly.Checked = true;
			}
		}

		private void FillDayAndMonthLists()
		{
			SchedulerDayOfWeek schedulerDayOfWeek = SchedulerDayOfWeek.Sunday | SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday | SchedulerDayOfWeek.Saturday;
			SchedulerDayOfWeek weekDays = SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
			SchedulerDayOfWeek weekendDays = SchedulerDayOfWeek.Sunday | SchedulerDayOfWeek.Saturday;
			ON item = new ON(schedulerDayOfWeek, "Day");
			ON weekDayItem = new ON(weekDays, "Week day");
			ON weekendDayItem = new ON(weekendDays, "Weekend day");
			ON sundayItem = new ON(SchedulerDayOfWeek.Sunday, "Sunday");
			ON mondayItem = new ON(SchedulerDayOfWeek.Monday, "Monday");
			ON tuesdayItem = new ON(SchedulerDayOfWeek.Tuesday, "Tuesday");
			ON wednesdayItem = new ON(SchedulerDayOfWeek.Wednesday, "Wednesday");
			ON thursdayItem = new ON(SchedulerDayOfWeek.Thursday, "Thursday");
			ON fridayItem = new ON(SchedulerDayOfWeek.Friday, "Friday");
			ON saturdayItem = new ON(SchedulerDayOfWeek.Saturday, "Saturday");
			_cboMonthlyDayWeek.Items.Add(item);
			_cboMonthlyDayWeek.Items.Add(weekDayItem);
			_cboMonthlyDayWeek.Items.Add(weekendDayItem);
			_cboMonthlyDayWeek.Items.Add(sundayItem);
			_cboMonthlyDayWeek.Items.Add(mondayItem);
			_cboMonthlyDayWeek.Items.Add(tuesdayItem);
			_cboMonthlyDayWeek.Items.Add(wednesdayItem);
			_cboMonthlyDayWeek.Items.Add(thursdayItem);
			_cboMonthlyDayWeek.Items.Add(fridayItem);
			_cboMonthlyDayWeek.Items.Add(saturdayItem);
			_cboMonthlyDayWeek.DisplayMember = "Name";
			_cboMonthlyDayWeek.ValueMember = "Object";
			_cboYearlyDayOfWeek.Items.Add(item);
			_cboYearlyDayOfWeek.Items.Add(weekDayItem);
			_cboYearlyDayOfWeek.Items.Add(weekendDayItem);
			_cboYearlyDayOfWeek.Items.Add(sundayItem);
			_cboYearlyDayOfWeek.Items.Add(mondayItem);
			_cboYearlyDayOfWeek.Items.Add(tuesdayItem);
			_cboYearlyDayOfWeek.Items.Add(wednesdayItem);
			_cboYearlyDayOfWeek.Items.Add(thursdayItem);
			_cboYearlyDayOfWeek.Items.Add(fridayItem);
			_cboYearlyDayOfWeek.Items.Add(saturdayItem);
			_cboYearlyDayOfWeek.DisplayMember = "Name";
			_cboYearlyDayOfWeek.ValueMember = "Object";
			_cboMonthlyOccurrence.Items.Add(OccurrenceInMonth.First);
			_cboMonthlyOccurrence.Items.Add(OccurrenceInMonth.Second);
			_cboMonthlyOccurrence.Items.Add(OccurrenceInMonth.Third);
			_cboMonthlyOccurrence.Items.Add(OccurrenceInMonth.Fourth);
			_cboMonthlyOccurrence.Items.Add(OccurrenceInMonth.Last);
			_cboMonthlyOccurrence.SelectedIndex = 0;
			_cboMonthlyOccurrence.ValueMember = "Value";
			_cboYearlyOccurrence.Items.Add(OccurrenceInMonth.First);
			_cboYearlyOccurrence.Items.Add(OccurrenceInMonth.Second);
			_cboYearlyOccurrence.Items.Add(OccurrenceInMonth.Third);
			_cboYearlyOccurrence.Items.Add(OccurrenceInMonth.Fourth);
			_cboYearlyOccurrence.Items.Add(OccurrenceInMonth.Last);
			_cboYearlyOccurrence.SelectedIndex = 0;
			_cboYearlyOccurrence.ValueMember = "Value";
			_cboMonthOfYear.Items.Add(new ON(1, "January"));
			_cboMonthOfYear.Items.Add(new ON(2, "February"));
			_cboMonthOfYear.Items.Add(new ON(3, "March"));
			_cboMonthOfYear.Items.Add(new ON(4, "April"));
			_cboMonthOfYear.Items.Add(new ON(5, "May"));
			_cboMonthOfYear.Items.Add(new ON(6, "June"));
			_cboMonthOfYear.Items.Add(new ON(7, "July"));
			_cboMonthOfYear.Items.Add(new ON(8, "August"));
			_cboMonthOfYear.Items.Add(new ON(9, "September"));
			_cboMonthOfYear.Items.Add(new ON(10, "October"));
			_cboMonthOfYear.Items.Add(new ON(11, "November"));
			_cboMonthOfYear.Items.Add(new ON(12, "December"));
			_cboMonthOfYear.DisplayMember = "Name";
			_cboMonthOfYear.ValueMember = "Object";
			_cboMonthOfYearNth.Items.Add(new ON(1, "January"));
			_cboMonthOfYearNth.Items.Add(new ON(2, "February"));
			_cboMonthOfYearNth.Items.Add(new ON(3, "March"));
			_cboMonthOfYearNth.Items.Add(new ON(4, "April"));
			_cboMonthOfYearNth.Items.Add(new ON(5, "May"));
			_cboMonthOfYearNth.Items.Add(new ON(6, "June"));
			_cboMonthOfYearNth.Items.Add(new ON(7, "July"));
			_cboMonthOfYearNth.Items.Add(new ON(8, "August"));
			_cboMonthOfYearNth.Items.Add(new ON(9, "September"));
			_cboMonthOfYearNth.Items.Add(new ON(10, "October"));
			_cboMonthOfYearNth.Items.Add(new ON(11, "November"));
			_cboMonthOfYearNth.Items.Add(new ON(12, "December"));
			_cboMonthOfYearNth.DisplayMember = "Name";
			_cboMonthOfYearNth.ValueMember = "Object";
		}

		private bool TryApplyControlsToPattern()
		{
			try
			{
				ApplyControlsToPattern();
			}
			catch
			{
				Mordex.Windows.Common.MxMessageBox.Show(this, "The recurrence pattern is not valid", "Schedule", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return false;
			}
			return true;
		}

		/// <summary>Copies the control values to the pattern. If a value is not valid, the pattern keeps all its previous values.</summary>
		internal void ApplyControlsToPattern()
		{
			string savedPattern = _workingHourRecurrencePattern.ToString();
			_workingHourRecurrencePattern.BeginEdit();
			try
			{
				CopyControlsToPattern();
			}
			catch
			{
				LayoutReader.FromString(savedPattern)["RecurrencePattern"]?.DeserializeObject(_workingHourRecurrencePattern);
				throw;
			}
			finally
			{
				_workingHourRecurrencePattern.EndEdit();
			}
		}

		private void CopyControlsToPattern()
		{
			_workingHourRecurrencePattern.StartTime = _dtpStartTime.Value.TimeOfDay;
			TimeSpan timeOfDay = _dtpEndTime.Value.TimeOfDay;
			_workingHourRecurrencePattern.EndTime = timeOfDay;
			ApplyRangeDates();
			if (_rdbDaily.Checked)
			{
				_workingHourRecurrencePattern.RecurrenceType = RecurrenceType.Daily;
				_workingHourRecurrencePattern.Interval = int.Parse(_txtDailyInterval.Text);
				if (_rdbWeekDay.Checked)
				{
					_workingHourRecurrencePattern.DailyRecurrenceMode = DailyRecurrenceMode.WeekDays;
				}
				else if (_rdbWeekendDay.Checked)
				{
					_workingHourRecurrencePattern.DailyRecurrenceMode = DailyRecurrenceMode.WeekendDays;
				}
			}
			else if (_rdbWeekly.Checked)
			{
				_workingHourRecurrencePattern.RecurrenceType = RecurrenceType.Weekly;
				_workingHourRecurrencePattern.Interval = int.Parse(_txtWeeklyInterval.Text);
				_workingHourRecurrencePattern.DayOfWeek = GetCheckedWeekDays();
			}
			else if (_rdbMonthly.Checked)
			{
				if (_rdbMonthNth.Checked)
				{
					_workingHourRecurrencePattern.RecurrenceType = RecurrenceType.MontNth;
					_workingHourRecurrencePattern.OccurrenceInMonth = (OccurrenceInMonth)_cboMonthlyOccurrence.SelectedItem;
					_workingHourRecurrencePattern.DayOfWeek = (SchedulerDayOfWeek)((ON)_cboMonthlyDayWeek.SelectedItem).Object;
					_workingHourRecurrencePattern.Interval = int.Parse(_txtMonthNthInterval.Text);
				}
				else
				{
					_workingHourRecurrencePattern.RecurrenceType = RecurrenceType.Monthly;
					_workingHourRecurrencePattern.DayOfMonth = int.Parse(_txtDayOfMonth.Text);
					_workingHourRecurrencePattern.Interval = int.Parse(_txtMonthlyInterval.Text);
				}
			}
			else if (_rdbYearly.Checked)
			{
				if (_rdbYearNth.Checked)
				{
					_workingHourRecurrencePattern.RecurrenceType = RecurrenceType.YearNth;
					_workingHourRecurrencePattern.MonthOfYear = (int)((ON)_cboMonthOfYearNth.SelectedItem).Object;
					_workingHourRecurrencePattern.OccurrenceInMonth = (OccurrenceInMonth)_cboYearlyOccurrence.SelectedItem;
					_workingHourRecurrencePattern.DayOfWeek = (SchedulerDayOfWeek)((ON)_cboYearlyDayOfWeek.SelectedItem).Object;
				}
				else
				{
					_workingHourRecurrencePattern.RecurrenceType = RecurrenceType.Yearly;
					_workingHourRecurrencePattern.MonthOfYear = (int)((ON)_cboMonthOfYear.SelectedItem).Object;
					_workingHourRecurrencePattern.DayOfMonth = int.Parse(_txtYearlyDayOfMonth.Text);
				}
			}
			if (_rdbNoEndDate.Checked)
			{
				_workingHourRecurrencePattern.RecurrenceEndMode = RecurrenceEndMode.NoEndDate;
			}
			else if (_rdbOccurrences.Checked)
			{
				_workingHourRecurrencePattern.RecurrenceEndMode = RecurrenceEndMode.Occurrences;
				_workingHourRecurrencePattern.Occurrences = int.Parse(_txtOccurrences.Text);
			}
			else
			{
				_workingHourRecurrencePattern.RecurrenceEndMode = RecurrenceEndMode.EndDate;
			}
		}

		private void ApplyRangeDates()
		{
			DateTime rangeStart = _dtpRecurrenceStart.Value.Date;
			DateTime rangeEnd = _workingHourRecurrencePattern.PatternEndDate;
			if (!_rdbNoEndDate.Checked && !_rdbOccurrences.Checked)
			{
				rangeEnd = _dtpRecurrenceEndDate.Value.Date;
			}
			else if (rangeEnd < rangeStart)
			{
				// The end date is not used in this end mode; it only has to stay after the start.
				rangeEnd = rangeStart;
			}
			// When the range moves forward, the end goes first so the start never passes the old end.
			if (rangeStart > _workingHourRecurrencePattern.PatternEndDate)
			{
				_workingHourRecurrencePattern.PatternEndDate = rangeEnd;
				_workingHourRecurrencePattern.PatternStartDate = rangeStart;
			}
			else
			{
				_workingHourRecurrencePattern.PatternStartDate = rangeStart;
				_workingHourRecurrencePattern.PatternEndDate = rangeEnd;
			}
		}

		private void OnBtnOKClick(object sender, EventArgs eventArgs)
		{
			try
			{
				if (TryApplyControlsToPattern())
				{
					Close();
				}
			}
			catch
			{
				Mordex.Windows.Common.MxMessageBox.Show("The recurrence pattern is not valid", "Schedule");
			}
		}

		private void OnBtnCancelClick(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void SelectComboItemByValue(ComboBox comboBox, object value)
		{
			foreach (ON item in comboBox.Items)
			{
				if (item.Object.Equals(value))
				{
					comboBox.SelectedItem = item;
					break;
				}
			}
		}

		/// <summary>Returns the values to show for a newly chosen pattern type. A user change works on a copy, so Cancel leaves the pattern as it was.</summary>
		private WorkingHourRecurrencePattern PatternForTypeControls()
		{
			if (_isLoadingPattern)
			{
				return _workingHourRecurrencePattern;
			}
			WorkingHourRecurrencePattern copy = WorkingHourRecurrencePattern.FromXMLString(_workingHourRecurrencePattern.ToString());
			copy.SetDefaultValuesForDate(_dtpRecurrenceStart.Value);
			return copy;
		}

		private void OnRdbDailyCheckedChanged(object sender, EventArgs eventArgs)
		{
			_grbDailyPattern.Visible = _rdbDaily.Checked;
			if (_rdbDaily.Checked)
			{
				_txtDailyInterval.Text = _workingHourRecurrencePattern.Interval.ToString();
				switch (_workingHourRecurrencePattern.DailyRecurrenceMode)
				{
				case DailyRecurrenceMode.UseInterval:
					_rdbDailyInterval.Checked = true;
					break;
				case DailyRecurrenceMode.WeekDays:
					_rdbWeekDay.Checked = true;
					break;
				case DailyRecurrenceMode.WeekendDays:
					_rdbWeekendDay.Checked = true;
					break;
				}
			}
		}

		private void OnRdbWeeklyCheckedChanged(object sender, EventArgs eventArgs)
		{
			_grbWeeklyPattern.Visible = _rdbWeekly.Checked;
			if (_workingHourRecurrencePattern != null && _rdbWeekly.Checked)
			{
				WorkingHourRecurrencePattern pattern = PatternForTypeControls();
				_txtWeeklyInterval.Text = pattern.Interval.ToString();
				UpdateRecurrenceForm(pattern);
			}
		}

		private void OnRdbMonthlyCheckedChanged(object sender, EventArgs eventArgs)
		{
			_grbMonthlyPattern.Visible = _rdbMonthly.Checked;
			if (_rdbMonthly.Checked)
			{
				WorkingHourRecurrencePattern pattern = PatternForTypeControls();
				_txtDayOfMonth.Text = pattern.DayOfMonth.ToString();
				SelectComboItemByValue(_cboMonthlyDayWeek, pattern.DayOfWeek);
				_cboMonthlyOccurrence.SelectedItem = pattern.OccurrenceInMonth;
				_txtMonthlyInterval.Text = pattern.Interval.ToString();
				_txtMonthNthInterval.Text = pattern.Interval.ToString();
			}
		}

		private void OnRdbYearlyCheckedChanged(object sender, EventArgs eventArgs)
		{
			_grbYearlyPattern.Visible = _rdbYearly.Checked;
			if (_rdbYearly.Checked)
			{
				WorkingHourRecurrencePattern pattern = PatternForTypeControls();
				_txtYearlyDayOfMonth.Text = pattern.DayOfMonth.ToString();
				SelectComboItemByValue(_cboYearlyDayOfWeek, pattern.DayOfWeek);
				SelectComboItemByValue(_cboMonthOfYear, pattern.MonthOfYear);
				SelectComboItemByValue(_cboMonthOfYearNth, pattern.MonthOfYear);
				_cboYearlyOccurrence.SelectedItem = pattern.OccurrenceInMonth;
			}
		}

		private void OnBtnRemoveRecurrenceClick(object sender, EventArgs eventArgs)
		{
			SetWorkingHourRecurrencePattern(null);
			Close();
		}

		private void OnRdbPatternEndDateCheckedChanged(object sender, EventArgs eventArgs)
		{
			if (_rdbPatternEndDate.Checked)
			{
				_dtpRecurrenceEndDate.Enabled = true;
			}
			else
			{
				_dtpRecurrenceEndDate.Enabled = false;
			}
		}

		private void OnDtpRecurrenceStartValueChanged(object sender, EventArgs eventArgs)
		{
			if (_isLoadingPattern)
			{
				return;
			}
			int occurrenceCount = 10;
			try
			{
				occurrenceCount = int.Parse(_txtOccurrences.Text);
			}
			catch
			{
				occurrenceCount = 10;
			}
			_dtpRecurrenceEndDate.Value = _dtpRecurrenceStart.Value.AddDays(7 * occurrenceCount);
		}
	}
}
