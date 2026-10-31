using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	[ToolboxItem(false)]
	public partial class DatePickerCalendar : Control, IDatePickerElement
	{
		private const int ScrollRepeatInterval = 200;

		private const int CaptureRetakeInterval = 20;

		private const int WmCaptureChanged = 0x215;

		private const int WmCancelMode = 0x1F;

		private IContainer _components;

		internal Timer _scrollTimer;

		internal CalendarControl _calendarControl;

		private bool _retakeCaptureOnLoss;

		private MonthLayout[,] _monthLayouts = new MonthLayout[0, 0];

		private IntPtr _previousParentHandle = IntPtr.Zero;

		private MxDatePicker _datePicker;

		private string _headerFormat = "MMMM yyyy";

		internal Font _headerFont;

		private Color _blendColor = Color.Empty;

		private float _defaultBlend;

		private Timer _captureTimer;

		private bool _followSelection = true;

		private MonthLayout _hotMonthHeader;

		private MonthLayout _pressedMonthHeader;

		private ThemeRenderer _themeRenderer;

		private readonly ControlStyle _borderHelperStyle = new ControlStyle(null);

		private Font _weekNumberFont;

		private CalendarWeekRule _weekRule;

		private bool _weekNumbers;

		private int _headerHeight;

		private int _dayCellWidth;

		private int _dayCellHeight;

		private int _weekColumnWidth;

		private Rectangle[] _scrollArrowBounds = new Rectangle[2];

		private BorderStyle _borderStyle = BorderStyle.Flat;

		private int _columns = 1;

		private int _rows = 1;

		private MonthLayout _monthLayoutTemplate;

		private string[] _dayHeaderTexts = new string[7];

		private CultureInfo _cultureStamp;

		private CalendarDayOfWeek _firstDayOfWeek = CalendarDayOfWeek.Default;

		private DayOfWeekAbbreviation _dayOfWeekAbbreviation;

		private Rectangle _contentBounds = Rectangle.Empty;

		private Size _borderSize = new Size(2, 2);

		private DateTime _firstMonth;

		private bool _isAdjustingFirstMonth;

		private int _scrollMonthsStep;

		private int _scrollChange = 1;

		private int _pickedMonthIndex;

		private Font _dayHeaderFont;

		private DateTime[] _boldedDates = new DateTime[0];

		private DateTime[] _annuallyBoldedDates = new DateTime[0];

		private DateTime _selectedDate = DateTime.Today;

		private CalendarFormatStyle _selectedFormatStyle;

		private CalendarFormatStyle _trailingFormatStyle;

		private CalendarFormatStyle _daysFormatStyle;

		private CalendarFormatStyle _headerFormatStyle;

		private CalendarFormatStyle _dayHeadersFormatStyle;

		private CalendarBackgroundStyle _calendarBackgroundStyle;

		private DateRange _visibleRange;

		private Color _todayRectColor = Color.Empty;

		private bool _todayRect = true;

		private bool _showTrailingDays = true;

		private DateTime _maxDate = DateTime.MaxValue.Date.AddYears(-1);

		private DateTime _minDate = DateTime.MinValue.AddMonths(1);

		private ControlStyle _controlStyle;

		private IDropListOwner _dropListOwner;

		private Rectangle _buttonRowBounds;

		private PickerButton _todayButton;

		private PickerButton _noneButton;

		private PickerButton _hotButton;

		private PickerButton _pressedButton;

		internal bool _isDroppedDown;

		private string _todayButtonText = "Today";

		private string _noneButtonText = "None";

		private bool _isNullDateSelected;

		private bool _showNoneButton;

		private bool _showTodayButton = true;

		DatePickerCalendar IDatePickerElement.Calendar => this;

		internal DatePickerCalendar()
			: this(null)
		{
		}

		internal DatePickerCalendar(MxDatePicker datePicker)
		{
			_datePicker = datePicker;
			InitializeState();
			InitializeCalendarDefaults();
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		}

		protected override void Dispose(bool disposing)
		{
			StopThemeListening();
			if (disposing)
			{
				DisposeCaptureTimer();
				_scrollTimer.Tick -= OnScrollTimerTick;
				_components.Dispose();
				_calendarControl.Dispose();
				_dayHeaderFont.Dispose();
				if (_weekNumberFont != null)
				{
					_weekNumberFont.Dispose();
					_weekNumberFont = null;
				}
				_selectedFormatStyle.DisposeResources();
				_trailingFormatStyle.DisposeResources();
				_daysFormatStyle.DisposeResources();
				_headerFormatStyle.DisposeResources();
				_dayHeadersFormatStyle.DisposeResources();
				_calendarBackgroundStyle.DisposeBrush();
				if (_controlStyle != null)
				{
					_controlStyle.DisposeResources();
				}
				_borderHelperStyle.DisposeResources();
				ReleaseThemeRenderer();
			}
			base.Dispose(disposing);
		}

		private void DisposeCaptureTimer()
		{
			if (_captureTimer != null)
			{
				_captureTimer.Tick -= OnCaptureTimerTick;
				_captureTimer.Dispose();
				_captureTimer = null;
			}
		}

		void IDatePickerElement.Invalidate()
		{
			Invalidate();
		}

		private void InitializeCalendarDefaults()
		{
			_components = new Container();
			_scrollTimer = new Timer(_components);
			_scrollTimer.Interval = ScrollRepeatInterval;
			_scrollTimer.Tick += OnScrollTimerTick;
			BackColor = SystemColors.Window;
			ForeColor = SystemColors.WindowText;
			Text = "Calendar";
		}

		private void OnScrollTimerTick(object sender, EventArgs eventArgs)
		{
			FirstMonth = DatePickerHelper.AddMonthsClamped(_firstMonth, _scrollMonthsStep);
			_scrollTimer.Interval = ScrollRepeatInterval;
		}

		private void InitializeState()
		{
			_todayButton = new PickerButton(this, 0);
			_noneButton = new PickerButton(this, 0);
			_monthLayoutTemplate = new MonthLayout(HeaderFormat);
			UpdateDayHeaderTexts();
			_firstMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			if (_dayHeaderFont == null)
			{
				_dayHeaderFont = new Font(base.Font, FontStyle.Bold);
			}
			_headerFormatStyle = new CalendarFormatStyle(this, PredefinedStyle.Header);
			_daysFormatStyle = new CalendarFormatStyle(this, PredefinedStyle.None);
			_selectedFormatStyle = new CalendarFormatStyle(this, PredefinedStyle.Selected);
			_calendarBackgroundStyle = new CalendarBackgroundStyle(this);
			_trailingFormatStyle = new CalendarFormatStyle(this, PredefinedStyle.Trailing);
			_dayHeadersFormatStyle = new CalendarFormatStyle(this, PredefinedStyle.None);
			RebuildLayout();
			_calendarControl = new CalendarControl(!UsesCompatibleTextRendering());
			_calendarControl.SetMonthFormat(HeaderFormat);
			_calendarControl._owner = this;
			_calendarControl.Visible = false;
			_calendarControl.Location = new Point(7, 34);
			_calendarControl.TabIndex = 0;
			_calendarControl.Size = new Size(0, 0);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			ReleaseThemeRenderer();
			Invalidate();
		}

		protected override void OnParentFontChanged(EventArgs e)
		{
			base.OnParentFontChanged(e);
			if (_headerFont == null)
			{
				MeasureCells();
				ComputeDroppedControlSize(_columns, _rows);
				UpdateMonthLayouts(true);
				UpdateMonthDates(true);
				Invalidate();
			}
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			_dayHeaderFont.Dispose();
			_dayHeaderFont = new Font(base.Font, FontStyle.Bold);
			if (_weekNumberFont != null)
			{
				_weekNumberFont.Dispose();
				_weekNumberFont = null;
			}
			ComputeDroppedControlSize(_columns, _rows);
			MeasureCells();
			UpdateMonthLayouts(true);
			UpdateMonthDates(true);
			Invalidate();
		}
	}
}
