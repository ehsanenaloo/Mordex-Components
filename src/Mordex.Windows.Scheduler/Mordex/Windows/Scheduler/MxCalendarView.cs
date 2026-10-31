using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[DefaultEvent("DatesChanged")]
	[DefaultProperty("CurrentDate")]
	[DefaultBindingProperty("CurrentDate")]
	public partial class MxCalendarView : Control, ISupportInitialize, ISchedulerWorkingHoursOwner, ISchedulerFormatStyleOwner
	{
		internal delegate void DateSelectedCallback(object selObject);

		private bool _isInitializing;

		// The last paint or layout error written to Trace.
		private string _paintError;

		private IContainer _iContainer;

		private DateRange _dateRange;

		private Timer _scrollTimer;

		private CalendarControl _calendarControl;

		private WorkingHoursSchemaLayoutState[,] _workingHoursSchemaLayoutStateArray = new WorkingHoursSchemaLayoutState[0, 0];

		private Cursor _cursor;

		private MxScheduler _scheduler;

		internal Font _headerFont;

		private DateRange _calendarDateRange = new DateRange(DateTime.MinValue, DateTime.MinValue);

		private bool _isOverWeekGutter;

		private bool _isSelecting;

		private bool _isWeekSelection;

		private Appearance _headerAppearance = Appearance.Light3D;

		private SchedulerFormatStyleLayoutState _schedulerFormatStyleLayoutState;

		private WorkingHoursSchemaLayoutState _workingHoursSchemaLayoutState;

		private WorkingHoursSchemaLayoutState _calendarWorkingHoursSchemaLayoutState;

		private float _defaultBlend;

		private Color _blendColor = Color.Empty;

		private Brush _backgroundBrush;

		private Pen _gridLinePen;

		private Font _weekNumberFont;

		private CalendarWeekRule _weekRule;

		private bool _weekNumbers;

		private int _headerHeight;

		private int _dayCellWidth;

		private int _dayCellHeight;

		private int _sideColumnWidth;

		private Rectangle[] _rectangleArray = new Rectangle[2];

		private bool _useMonthSelector = true;

		private bool? _useCompatibleTextRendering;

		private SchedulerThemeColors _themeColors;

		private AlphaMode _backColorAlphaMode = AlphaMode.Opaque;

		private AlphaMode _calendarBackColorAlphaMode;

		private AlphaMode _foreColorAlphaMode;

		private int _defaultAlpha = 255;

		private BorderStyle _borderStyle = BorderStyle.SunkenLight3D;

		private bool _autoSize;

		private bool _focusOnClick = true;

		private int _monthColumnCount;

		private int _monthRowCount;

		private WorkingHoursSchemaLayoutState _monthTemplate;

		private string[] _textArray = new string[7];

		private BoldDatesMode _boldDatesMode = BoldDatesMode.All;

		private DateCollection _appointmentBoldDates;

		private CalendarDayOfWeek _firstDayOfWeek = CalendarDayOfWeek.Default;

		private DayOfWeekAbbreviation _dayOfWeekAbbreviation;

		private Rectangle _contentBounds = Rectangle.Empty;

		private Size _borderSize = Size.Empty;

		private bool _scrollSelectedDates = true;

		private string _headerFormat = "MMMM yyyy";

		private bool _shouldShiftSelectionOnScroll = true;

		private DateTime _firstMonth;

		private int _scrollMonthStep;

		private int _scrollChange = 1;

		private int _clickedMonthIndex;

		private Font _calendarFont;

		private DateTime[] _boldedDates = new DateTime[0];

		private DateTime[] _annuallyBoldedDates = new DateTime[0];

		private DateTime _selectionAnchorDate;

		private DateTime _currentDate = DateTime.MinValue;

		private bool _shouldIgnoreSelectionLimit;

		private DateTime _selectionCursorDate = DateTime.MinValue;

		private SchedulerFormatStyle _schedulerFormatStyle;

		private SchedulerFormatStyle _calendarSchedulerFormatStyle;

		private SchedulerWatermarkImage _watermarkImage;

		private ImageDrawMode _backgroundImageDrawMode;

		private HideSelection _hideSelection = HideSelection.HighlightInactive;

		private SchedulerFormatStyle _selectedInactiveFormatStyle;

		private SchedulerFormatStyle _daysFormatStyle;

		private SchedulerFormatStyle _alternatingMonthDaysFormatStyle;

		private SchedulerFormatStyle _headersFormatStyle;

		private SchedulerFormatStyle _daysHeadersFormatStyle;

		private SchedulerFormatStyle _monthBackgroundStyle;

		private SchedulerFormatStyle _alternatingMonthBackgroundStyle;

		private Color _todayRectColor = Color.DarkRed;

		private bool _todayRect = true;

		private WorkingHoursSchema _workingHoursSchema;

		private bool _showTrailingDays = true;

		private DateTime _maxDate = DateTime.MaxValue.Date.AddYears(-1);

		private DateTime _minDate = DateTime.MinValue.AddMonths(1);

		private CalendarSelectionStyle _selectionStyle;

		private DateTime[] _selectedDates;

		private bool _isUpdatingScheduler;

		private bool _isSyncingFromScheduler;

		private DateTime[] _calendarSelectedDates = new DateTime[0];

		private ArrayList _selectedDateList = new ArrayList();

		private DateRange _selectionRange = new DateRange(DateTime.Today, DateTime.Today);

		private DateRange _activeSelectionRange = new DateRange(DateTime.MinValue, DateTime.MinValue);

		private DateRange _anchorWeekRange = new DateRange(DateTime.MinValue, DateTime.MinValue);

		private bool _isWeekRangeSelection;

		private int _maxSelectionCount = 7;

		private bool _shouldRefreshBoldDates;

		private static readonly object _datesChangedEventKey = new object();

		private static readonly object _currentDateChangedEventKey = new object();

		private static readonly object _selectionChangedKey = new object();

		[Description("Event fired when the range of dates displayed by the Calendar is changed.")]
		public event EventHandler DatesChanged
		{
			add
			{
				base.Events.AddHandler(_datesChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_datesChangedEventKey, value);
			}
		}

		[Description("Event fired when the CurrentDate property changes.")]
		public event EventHandler CurrentDateChanged
		{
			add
			{
				base.Events.AddHandler(_currentDateChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_currentDateChangedEventKey, value);
			}
		}

		[Description("Event fired when the current selection of days changes.")]
		public event EventHandler SelectionChanged
		{
			add
			{
				base.Events.AddHandler(_selectionChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectionChangedKey, value);
			}
		}

		void ISchedulerFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			Invalidate();
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_isInitializing = false;
			}
		}

		public MxCalendarView()
		{
			DisposeResources();
			UpdateCalendar();
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			CurrentDate = DateTime.Today;
			AutoSize = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			StopThemeListening();
			Scheduler = null;
			base.Dispose(disposing);
		}

		private void StopScrollTimer()
		{
			try
			{
				if (_scrollTimer != null)
				{
					_scrollTimer.Enabled = false;
					_scrollTimer.Tick -= OnTimerTick;
					_scrollTimer.Tick -= OnDragScrollTimerTick;
					_scrollTimer.Dispose();
					_scrollTimer = null;
				}
			}
			catch
			{
			}
		}

		private Color GetBackgroundColor()
		{
			Color result = BackColor;
			if (!ShouldSerializeBackColor())
			{
				result = MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.InputBack);
			}
			return result;
		}

		internal Color GetTextColor()
		{
			if (ForeColor == SystemColors.ControlText)
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.Text);
			}
			return base.ForeColor;
		}

		private void DisposeResources()
		{
			_workingHoursSchema = new WorkingHoursSchema(this);
			Stream stream = new MemoryStream(SchedulerImageResources.GetArrowCursorBytes());
			_cursor = new Cursor(stream);
			stream.Close();
			stream.Dispose();
			_monthTemplate = new WorkingHoursSchemaLayoutState(_workingHoursSchema, HeaderFormat);
			UpdateWeekdayNames();
			_firstMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			if (_calendarFont == null)
			{
				_calendarFont = new Font(base.Font, FontStyle.Bold);
			}
			_headersFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.Header);
			_daysFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
			_schedulerFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.Selected);
			_selectedInactiveFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.SelectedInactive);
			_alternatingMonthBackgroundStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
			_alternatingMonthDaysFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
			_monthBackgroundStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
			_calendarSchedulerFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.Trailing);
			_daysHeadersFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
			UpdateBorderSize();
			UpdateMonthGrid();
			MeasureMonthLayout();
			UpdateWorkingHoursSchemaLayoutStateArray(true);
			RefreshMonthLayouts(true);
			Invalidate();
		}

		private void UpdateBorderSize()
		{
			_borderSize = GetEffectiveBorderStyle() != BorderStyle.None ? new Size(2, 2) : Size.Empty;
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			_calendarFont = new Font(base.Font, FontStyle.Bold);
			MeasureMonthLayout();
			UpdateMonthGrid();
			UpdateWorkingHoursSchemaLayoutStateArray(true);
			RefreshMonthLayouts(true);
			Invalidate();
		}

		internal SchedulerFormatStyleLayoutState FindDayCellAt(int x, int integerArgument, bool shouldRequireMonthDay)
		{
			try
			{
				return FindMonthWithDayAt(x, integerArgument)?.GetCellAtPoint(x, integerArgument, shouldRequireMonthDay);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private SchedulerThemeColors GetThemeColors()
		{
			if (_themeColors == null)
			{
				_themeColors = new SchedulerThemeColors();
			}
			return _themeColors;
		}

		private void DisposePaintResources()
		{
			if (_themeColors != null)
			{
				_themeColors.DisposeResources();
				_themeColors = null;
			}
			_backgroundBrush = null;
		}

		private SchedulerFormatStyle GetDayCellStyle(SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState, SchedulerFormatStyle baseStyle, bool isSelected)
		{
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(baseStyle);
			if (isSelected)
			{
				if (base.ContainsFocus || _hideSelection == HideSelection.Highlight)
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(_schedulerFormatStyle);
				}
				else if (_hideSelection == HideSelection.HighlightInactive)
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(_selectedInactiveFormatStyle);
				}
			}
			if (schedulerFormatStyleLayoutState.IsOutsideMonth() || !schedulerFormatStyleLayoutState.IsWithinDateLimits())
			{
				schedulerFormatStyle = schedulerFormatStyle.MergeWith(_calendarSchedulerFormatStyle);
			}
			return schedulerFormatStyle;
		}

		protected override void OnResize(EventArgs e)
		{
			try
			{
				base.OnResize(e);
				if (!(base.Size == Size.Empty))
				{
					if (_monthTemplate == null || _monthTemplate.GetTotalSize().Width == 0 || _monthTemplate.GetTotalSize().Height == 0)
					{
						MeasureMonthLayout();
					}
					UpdateMonthGrid();
					_contentBounds = base.ClientRectangle;
					_contentBounds.Inflate(-_borderSize.Width / 2, -_borderSize.Height / 2);
				}
			}
			catch
			{
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (base.Visible)
			{
				UpdateWorkingHoursSchemaLayoutStateArray(true);
				UpdateMonthGrid();
				MeasureMonthLayout();
			}
		}

		private void ApplyThemeToFormatStyles()
		{
			HeadersFormatStyle.ApplyTheme();
			SelectedFormatStyle.ApplyTheme();
			SelectedInactiveFormatStyle.ApplyTheme();
			TrailingFormatStyle.ApplyTheme();
		}

		private BorderStyle GetEffectiveBorderStyle()
		{
			if (BorderStyle == BorderStyle.None)
			{
				return BorderStyle;
			}
			return BorderStyle.Flat;
		}

		private bool IsArabicCulture()
		{
			string name = CultureInfo.CurrentCulture.Name;
			if (name.StartsWith("ar", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}

		private void ClearAppointmentBoldDates()
		{
			if (_appointmentBoldDates == null)
			{
				return;
			}
			foreach (DateTime item in _appointmentBoldDates)
			{
				FindDayCellByDate(item)?.SetBold(false);
			}
			_appointmentBoldDates.Clear();
			_appointmentBoldDates = null;
		}

		private void ScrollToShowDate(DateTime dateTimeArgument)
		{
			if (!_scrollSelectedDates)
			{
				return;
			}
			_shouldShiftSelectionOnScroll = false;
			if (dateTimeArgument < FirstMonth && _calendarSelectedDates[_calendarSelectedDates.Length - 1] < FirstMonth && _calendarSelectedDates.Length < 7)
			{
				FirstMonth = new DateTime(dateTimeArgument.Year, dateTimeArgument.Month, 1);
			}
			else if (dateTimeArgument < DatesRange.Start && dateTimeArgument != GetWeekStart(DatesRange.Start) && _calendarSelectedDates.Length >= 7)
			{
				if (!(dateTimeArgument == GetWeekStart(DatesRange.Start)))
				{
					FirstMonth = new DateTime(dateTimeArgument.Year, dateTimeArgument.Month, 1);
				}
			}
			else
			{
				DateTime dateTime = ((_workingHoursSchemaLayoutStateArray.Length <= 0) ? _firstMonth.AddMonths(1) : _firstMonth.AddMonths(_workingHoursSchemaLayoutStateArray.Length - 1));
				dateTime = new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
				if (dateTimeArgument > dateTime)
				{
					dateTime = new DateTime(dateTimeArgument.Year, dateTimeArgument.Month, 1).AddMonths(-(_workingHoursSchemaLayoutStateArray.Length - 1));
					FirstMonth = dateTime;
				}
			}
			_shouldShiftSelectionOnScroll = true;
		}

		private bool HasSameSelectedDates(ArrayList arrayList)
		{
			if (_calendarSelectedDates == null)
			{
				if (arrayList.Count > 0)
				{
					return false;
				}
				return true;
			}
			if (arrayList.Count != _calendarSelectedDates.Length)
			{
				return false;
			}
			DateTime[] currentSelection = _calendarSelectedDates;
			foreach (DateTime dateTime in currentSelection)
			{
				if (!arrayList.Contains(dateTime))
				{
					return false;
				}
			}
			return true;
		}

		private void UpdateLayout(DateTime[] calendarSelectedDates)
		{
			if (_calendarSelectedDates == calendarSelectedDates)
			{
				return;
			}
			ArrayList arrayList = new ArrayList();
			int maxSelectedCount = 42;
			if (_selectionStyle == CalendarSelectionStyle.Single)
			{
				maxSelectedCount = 1;
			}
			else if (_selectionStyle != CalendarSelectionStyle.Schedule)
			{
				maxSelectedCount = _maxSelectionCount;
			}
			int addedCount = 0;
			if (calendarSelectedDates != null)
			{
				foreach (DateTime dateTime in calendarSelectedDates)
				{
					if (addedCount >= maxSelectedCount)
					{
						break;
					}
					arrayList.Add(dateTime);
					addedCount++;
				}
			}
			if (_calendarSelectedDates == null || !HasSameSelectedDates(arrayList))
			{
				_calendarSelectedDates = calendarSelectedDates;
				if (_calendarSelectedDates != null)
				{
					RebuildSelectionFromDates();
					if (_calendarSelectedDates.Length > 0)
					{
						_selectionCursorDate = _calendarSelectedDates[0];
						_selectionAnchorDate = _selectionCursorDate;
					}
					if (_calendarSelectedDates.Length > 14 || (_scheduler != null && _scheduler.View != SchedulerView.DayView && _scheduler.View != SchedulerView.WorkWeek))
					{
						_anchorWeekRange = new DateRange(GetWeekStart(_selectionCursorDate), GetWeekEnd(_selectionCursorDate));
						_isWeekSelection = true;
					}
				}
				else
				{
					ClearSelectedDays();
				}
				_selectedDates = calendarSelectedDates;
			}
			Invalidate();
		}

		private int GetInclusiveDayCount(DateTime dateTime, DateTime dateTimeArgument)
		{
			DateTime date = dateTime.Date;
			DateTime otherDate = dateTimeArgument.Date;
			int dayDifference = date.Subtract(otherDate).Days;
			if (dayDifference < 0)
			{
				dayDifference = -dayDifference;
			}
			return dayDifference + 1;
		}

		private void OnSchedulerAppointmentChanged(object sender, AppointmentChangeEventArgs eventArgs)
		{
			if (eventArgs.PropertyChanged == AppointmentChange.StartTime || eventArgs.PropertyChanged == AppointmentChange.EndTime || eventArgs.PropertyChanged == AppointmentChange.StartTimeEndTime)
			{
				RequestBoldDatesRefresh();
			}
		}

		private void OnSchedulerAppointmentAdded(object sender, AppointmentEventArgs eventArgs)
		{
			if (_scheduler != null && _boldDatesMode != BoldDatesMode.None && _appointmentBoldDates != null)
			{
				if (!ShouldBoldForOwner(eventArgs.Appointment._ownerValue) || eventArgs.Appointment.IsRecurring)
				{
					return;
				}
				if (_appointmentBoldDates == null)
				{
					UpdateDateCollection(true);
				}
				if (_dateRange.Start < eventArgs.Appointment.EndTime && eventArgs.Appointment.StartTime < _dateRange.End)
				{
					DateTime dateTime = eventArgs.Appointment.StartTime.Date;
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = null;
					while (eventArgs.Appointment.StartTime.Date <= dateTime && eventArgs.Appointment.EndTime > dateTime)
					{
						schedulerFormatStyleLayoutState = FindDayCellByDate(dateTime);
						if (schedulerFormatStyleLayoutState != null)
						{
							schedulerFormatStyleLayoutState.SetBold(true);
							if (!_appointmentBoldDates.Contains(dateTime))
							{
								_appointmentBoldDates.AddWithoutNotify(dateTime);
							}
						}
						dateTime = dateTime.AddDays(1.0);
					}
				}
			}
			Invalidate();
		}

		private void OnSchedulerFilterApplied(object sender, EventArgs eventArgs)
		{
			if (_scheduler != null && _boldDatesMode != BoldDatesMode.None)
			{
				try
				{
					RequestBoldDatesRefresh();
				}
				catch
				{
				}
				Invalidate();
			}
		}

		private void OnSchedulerCurrentOwnerChanged(object sender, EventArgs eventArgs)
		{
			if (_scheduler != null && _boldDatesMode != BoldDatesMode.None)
			{
				RequestBoldDatesRefresh();
				Invalidate();
			}
		}

		private void OnSchedulerAppointmentRemoved(object sender, AppointmentEventArgs eventArgs)
		{
			if (_scheduler != null && _scheduler._shouldHandleRemovals && _boldDatesMode != BoldDatesMode.None)
			{
				RequestBoldDatesRefresh();
			}
			Invalidate();
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			ApplyThemeToFormatStyles();
			Invalidate();
		}

	}
}
