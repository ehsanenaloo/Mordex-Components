using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	[ToolboxItem(false)]
	public partial class TimelineSurface : Control, ITimelineOwner, IMordexXmlLayoutsSupport
	{
		internal delegate void DateSelectedCallback(DateTime SelDate);

		private IContainer _iContainer;

		internal Timer _scrollTimer;

		internal CalendarControl _calendarControl;

		private TimelineLayoutState[,] _timelineLayoutStateArray = new TimelineLayoutState[0, 0];

		private IntPtr _intPtr = IntPtr.Zero;

		private bool _isFirstCaptureChange = true;

		protected MxTimeline timeline;

		internal Font _headerFont;

		private Color _blendColor = Color.Empty;

		private float _defaultBlend;

		private Timer _calendarTimer;

		private Appearance _headerAppearance = Appearance.Light3D;

		private TimelineLayoutState _timelineLayoutState;

		private TimelineLayoutState _calendarTimelineLayoutState;

		private Brush _backgroundBrush;

		private Pen _gridLinePen;

		private TimelineThemeRenderer _themeRenderer;

		private Font _weekNumberFont;

		private bool _weekNumbers;

		private int _headerHeight;

		private int _dayCellWidth;

		private int _dayCellHeight;

		private int _weekNumberColumnWidth;

		private Rectangle[] _rectangleArray = new Rectangle[2];

		private BorderStyle _borderStyle = BorderStyle.Flat;

		private int _monthColumnCount;

		private int _monthRowCount;

		private TimelineLayoutState _monthLayoutTemplate;

		private string[] _textArray = new string[7];

		private DayOfWeek _dayOfWeek;

		private DayOfWeekAbbreviation _dayOfWeekAbbreviation;

		private Rectangle _contentBounds = Rectangle.Empty;

		private Size _borderSize = new Size(2, 2);

		private DateTime _firstVisibleMonth;

		private int _monthScrollStep;

		private int _scrollChange = 1;

		private bool _boldDatesWithItems = true;

		private ArrayList _boldedItemDates;

		private int _clickedMonthIndex;

		private Font _calendarFont;

		private DateTime[] _boldedDates = new DateTime[0];

		private DateTime[] _annuallyBoldedDates = new DateTime[0];

		private DateTime _currentDate = DateTime.Today;

		private TimelineFormatStyle _selectedFormatStyle;

		private TimelineFormatStyle _trailingFormatStyle;

		private TimelineFormatStyle _daysFormatStyle;

		private TimelineFormatStyle _headerFormatStyle;

		private TimelineFormatStyle _dayHeadersFormatStyle;

		private TimelineFormatStyle _monthBackgroundStyle;

		private DateRange _dateRange;

		private Color _todayRectColor;

		private bool _todayRect = true;

		private DateTime _maxDate = DateTime.MaxValue.Date.AddYears(-1);

		private DateTime _minDate = DateTime.MinValue.AddMonths(1);

		private ITimelineDropDownOwner _dropDownOwner;

		private Rectangle _footerBounds;

		private TimelineCalendarButton _todayButton;

		private TimelineCalendarButton _noneButton;

		private TimelineCalendarButton _hoveredButton;

		private TimelineCalendarButton _pressedButton;

		internal bool _isDroppedDown;

		private string _todayText = "Today";

		private string _text = "None";

		private bool _isNoneSelected;

		private bool _isTodayButtonVisible = true;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateTime CurrentDate
		{
			get
			{
				return _currentDate;
			}
			set
			{
				_isNoneSelected = false;
				InvalidateDate(_currentDate);
				_currentDate = value.Date;
				InvalidateDate(_currentDate);
				SetFirstVisibleMonth(_currentDate);
			}
		}

		[DefaultValue(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop
		{
			get
			{
				return base.TabStop;
			}
			set
			{
				base.TabStop = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new int TabIndex
		{
			get
			{
				return base.TabIndex;
			}
			set
			{
				base.TabIndex = value;
			}
		}

		internal TimelineSurface()
		{
			try
			{
				UpdateIContainer();
				UpdateCalendar();
				SetStyle(ControlStyles.Selectable, value: true);
				SetStyle(ControlStyles.Opaque, value: true);
				SetStyle(ControlStyles.ResizeRedraw, value: true);
				SetStyle(ControlStyles.UserPaint, value: true);
				SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
				SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			}
			catch
			{
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			AnnuallyBoldedDates = (DateTime[])LayoutReader.GetPropertyValue(info, "AnnuallyBoldedDates", typeof(DateTime[]), new DateTime[0]);
			BackColor = (Color)LayoutReader.GetPropertyValue(info, "BackColor", typeof(Color), SystemColors.Window);
			BackgroundImage = (Image)LayoutReader.GetPropertyValue(info, "BackgroundImage", typeof(Image), null);
			BlendColor = (Color)LayoutReader.GetPropertyValue(info, "BlendColor", typeof(Color), Color.Empty);
			BoldedDates = (DateTime[])LayoutReader.GetPropertyValue(info, "BoldedDates", typeof(DateTime[]), new DateTime[0]);
			info.DeserializeObject("DayHeadersFormatStyle", _dayHeadersFormatStyle);
			info.DeserializeObject("DaysFormatStyle", _daysFormatStyle);
			ForeColor = (Color)LayoutReader.GetPropertyValue(info, "ForeColor", typeof(Color), SystemColors.ControlText);
			HeaderFont = (Font)LayoutReader.GetPropertyValue(info, "HeaderFont", typeof(Font), null);
			info.DeserializeObject("HeaderFormatStyle", _headerFormatStyle);
			UpdateMaxDate((DateTime)LayoutReader.GetPropertyValue(info, "MaxDate", typeof(DateTime), DateTime.MaxValue.Date.AddYears(-1)));
			UpdateMinDate((DateTime)LayoutReader.GetPropertyValue(info, "MinDate", typeof(DateTime), DateTime.MinValue.AddMonths(1)));
			info.DeserializeObject("MonthBackgroundStyle", _monthBackgroundStyle);
			info.DeserializeObject("SelectedFormatStyle", _selectedFormatStyle);
			_todayRectColor = (Color)LayoutReader.GetPropertyValue(info, "TodayRectColot", typeof(Color), Color.Empty);
			info.DeserializeObject("TrailingFormatStyle", _trailingFormatStyle);
			_boldDatesWithItems = (bool)LayoutReader.GetPropertyValue(info, "BoldDatesWithItems", typeof(bool), true);
			_borderStyle = (BorderStyle)LayoutReader.GetPropertyValue(info, "BorderStyle", typeof(BorderStyle), BorderStyle.Flat);
			_dayOfWeekAbbreviation = (DayOfWeekAbbreviation)LayoutReader.GetPropertyValue(info, "DayOfWeekAbbreviation", typeof(DayOfWeekAbbreviation), DayOfWeekAbbreviation.UseFirstLetter);
			_defaultBlend = (float)LayoutReader.GetPropertyValue(info, "DefaultBlend", typeof(float), 0f);
			Font = (Font)LayoutReader.GetPropertyValue(info, "Font", typeof(Font), Font);
			_scrollChange = (int)LayoutReader.GetPropertyValue(info, "ScrollChange", typeof(int), 1);
			_todayRect = (bool)LayoutReader.GetPropertyValue(info, "TodayRect", typeof(bool), true);
			_weekNumbers = (bool)LayoutReader.GetPropertyValue(info, "WeekNumbers", typeof(bool), false);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (ShouldSerializeAnnuallyBoldedDates())
			{
				info.AddValue("AnnuallyBoldedDates", AnnuallyBoldedDates);
			}
			if (ShouldSerializeBackColor())
			{
				info.AddValue("BackColor", BackColor);
			}
			if (ShouldSerializeBlendColor())
			{
				info.AddValue("BlendColor", _blendColor);
			}
			if (ShouldSerializeBoldedDates())
			{
				info.AddValue("BoldedDates", BoldedDates);
			}
			if (ShouldSerializeDayHeadersFormatStyle())
			{
				info.AddValue("DayHeadersFormatStyle", _dayHeadersFormatStyle);
			}
			if (ShouldSerializeDaysFormatStyle())
			{
				info.AddValue("DaysFormatStyle", _daysFormatStyle);
			}
			if (ShouldSerializeForeColor())
			{
				info.AddValue("ForeColor", ForeColor);
			}
			if (ShouldSerializeHeaderFont())
			{
				info.AddValue("HeaderFont", _headerFont);
			}
			if (ShouldSerializeHeaderFormatStyle())
			{
				info.AddValue("HeaderFormatStyle", _headerFormatStyle);
			}
			if (ShouldSerializeMaxDate())
			{
				info.AddValue("MaxDate", _maxDate);
			}
			if (ShouldSerializeMinDate())
			{
				info.AddValue("MinDate", _minDate);
			}
			if (ShouldSerializeMonthBackgroundStyle())
			{
				info.AddValue("MonthBackgroundStyle", _monthBackgroundStyle);
			}
			if (ShouldSerializeSelectedFormatStyle())
			{
				info.AddValue("SelectedFormatStyle", _selectedFormatStyle);
			}
			if (ShouldSerializeTodayRectColor())
			{
				info.AddValue("TodayRectColor", _todayRectColor);
			}
			if (ShouldSerializeTrailingFormatStyle())
			{
				info.AddValue("TrailingFormatStyle", _trailingFormatStyle);
			}
			if (BackgroundImage != null)
			{
				info.AddValue("BackgrounndImage", BackgroundImage);
			}
			if (!BoldDatesWithItems)
			{
				info.AddValue("BoldDatesWithItems", _boldDatesWithItems);
			}
			if (_borderStyle != BorderStyle.Flat)
			{
				info.AddValue("BorderStyle", _borderStyle);
			}
			if (DayOfWeekAbbreviation != DayOfWeekAbbreviation.UseFirstLetter)
			{
				info.AddValue("DayOfWeekAbbreviation", DayOfWeekAbbreviation);
			}
			if (_defaultBlend != 0f)
			{
				info.AddValue("DefaultBlend", _defaultBlend);
			}
			if (Font != Control.DefaultFont)
			{
				info.AddValue("Font", Font);
			}
			if (ScrollChange != 1)
			{
				info.AddValue("ScrollChange", ScrollChange);
			}
			if (!TodayRect)
			{
				info.AddValue("TodayRect", TodayRect);
			}
			if (WeekNumbers)
			{
				info.AddValue("WeekNumber", WeekNumbers);
			}
		}

		internal TimelineSurface(MxTimeline timeline)
		{
			try
			{
				this.timeline = timeline;
				UpdateIContainer();
				UpdateCalendar();
				SetStyle(ControlStyles.Selectable, value: true);
				SetStyle(ControlStyles.Opaque, value: true);
				SetStyle(ControlStyles.ResizeRedraw, value: true);
				SetStyle(ControlStyles.UserPaint, value: true);
				SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
				SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			}
			catch (Exception)
			{
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			if (disposing)
			{
				if (_scrollTimer != null)
				{
					_scrollTimer.Enabled = false;
					_scrollTimer.Tick -= OnTimerTick;
					_scrollTimer.Dispose();
				}
				if (_calendarTimer != null)
				{
					_calendarTimer.Dispose();
					_calendarTimer = null;
				}
				_calendarFont?.Dispose();
				_calendarFont = null;
				_weekNumberFont?.Dispose();
				_weekNumberFont = null;
			}
			base.Dispose(disposing);
		}

		protected override void OnParentFontChanged(EventArgs e)
		{
			base.OnParentFontChanged(e);
			if (_headerFont == null)
			{
				UpdateCellSizes();
				ComputeDroppedControlSize();
				UpdateTimelineLayoutStateArray(true);
				UpdateLayout(true);
				Invalidate();
			}
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == 533)
			{
				base.WndProc(ref msg);
				if (_isFirstCaptureChange && msg.LParam == IntPtr.Zero && Visible)
				{
					TimelineUtilities.SetControlCapture(this, true);
					_isFirstCaptureChange = false;
				}
			}
			else if (msg.Msg == 31)
			{
				UnDrop(cancel: true);
				base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
			}
		}

		private TimelineCalendarButton GetButtonAt(int x, int integerArgument)
		{
			if (_todayButton.ContainsPoint(x, integerArgument))
			{
				return _todayButton;
			}
			if (_noneButton.ContainsPoint(x, integerArgument))
			{
				return _noneButton;
			}
			return null;
		}

		private void DisposeResources()
		{
			if (_themeRenderer != null)
			{
				_themeRenderer = null;
			}
			_backgroundBrush = null;
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			_calendarFont?.Dispose();
			_calendarFont = new Font(base.Font, FontStyle.Bold);
			_weekNumberFont?.Dispose();
			_weekNumberFont = null;
			UpdateCellSizes();
			ComputeDroppedControlSize();
			UpdateTimelineLayoutStateArray(true);
			UpdateLayout(true);
			Invalidate();
		}

		private DateTime GetScrollLimitDate()
		{
			int monthSpan = _maxDate.Month - _minDate.Month + (_maxDate.Year - _minDate.Year) * 12;
			if (monthSpan < _timelineLayoutStateArray.Length - 1)
			{
				return CreateDateTime(_minDate).AddMonths(_timelineLayoutStateArray.Length);
			}
			return _maxDate.AddMonths(1);
		}

		private void UpdateLayout(bool shouldRefreshBoldDates)
		{
			DateTime dateTime = _firstVisibleMonth;
			for (int i = 0; i < _monthRowCount; i++)
			{
				for (int j = 0; j < _monthColumnCount; j++)
				{
					TimelineLayoutState timelineLayoutState = _timelineLayoutStateArray[i, j];
					timelineLayoutState.SetMonth(dateTime.Month);
					timelineLayoutState.SetYear(dateTime.Year);
					timelineLayoutState.SetMinDate(_minDate);
					timelineLayoutState.SetMaxDate(_maxDate);
					bool isFirstMonth = i == 0 && j == 0;
					bool isLastMonth = (i == _monthRowCount - 1) & (j == _monthColumnCount - 1);
					timelineLayoutState.PopulateDayCells(isFirstMonth, isLastMonth, GetDayOfWeek());
					dateTime = dateTime.AddMonths(1);
				}
			}
			if (_timelineLayoutStateArray.Length > 0)
			{
				UpdateDateRange(new DateRange(_timelineLayoutStateArray[0, 0].GetDateRange().Start, _timelineLayoutStateArray[_monthRowCount - 1, _monthColumnCount - 1].GetDateRange().End));
			}
			if (shouldRefreshBoldDates)
			{
				ApplyBoldedDates();
			}
			Invalidate();
		}

		private void ClearBoldedItemDates()
		{
			if (_boldedItemDates == null)
			{
				return;
			}
			foreach (DateTime item in _boldedItemDates)
			{
				GetDayCellForDate(item)?.SetBold(false);
			}
			_boldedItemDates.Clear();
			_boldedItemDates = null;
		}

		private void MarkDatesWithItems()
		{
			ClearBoldedItemDates();
			_boldedItemDates = new ArrayList();
			foreach (TimelineItem item in timeline.Items)
			{
				DateTime date = item.StartTime.Date;
				if (date >= _dateRange.Start && date <= _dateRange.End)
				{
					TimelineCalendarDay dayCell = GetDayCellForDate(date);
					if (dayCell != null)
					{
						dayCell.SetBold(true);
						_boldedItemDates.Add(date);
					}
				}
			}
			Invalidate();
		}

		private Color GetEffectiveBackColor()
		{
			return BackColor;
		}

		internal Color GetEffectiveForeColor()
		{
			return base.ForeColor;
		}

		protected internal virtual void Drop(Rectangle rect, object owner)
		{
			_isDroppedDown = true;
			_dropDownOwner = (ITimelineDropDownOwner)owner;
			UpdateCellSizes();
			ComputeDroppedControlSize();
			_intPtr = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			Point location = new Point(rect.X - 1, rect.Top);
			if (location.X + base.Width > SystemInformation.VirtualScreen.Width)
			{
				location.X = SystemInformation.VirtualScreen.Width - base.Width;
			}
			if (location.X < SystemInformation.VirtualScreen.Left)
			{
				location.X = SystemInformation.VirtualScreen.Left;
			}
			if (location.Y + base.Height > SystemInformation.VirtualScreen.Height)
			{
				location.Y = rect.Y - base.Height;
			}
			Location = location;
			UpdateTimelineLayoutStateArray(true);
			Visible = true;
			Update();
		}

		protected internal virtual void UnDrop(bool cancel)
		{
			if (_isDroppedDown)
			{
				HideDropDown(cancel);
				_isDroppedDown = false;
				if (_dropDownOwner != null)
				{
					_dropDownOwner.OnDropListHide(cancel);
					_dropDownOwner = null;
				}
			}
		}

		internal void HideDropDown(bool cancel)
		{
			_isDroppedDown = false;
			Hide();
			NativeMethods.SetParent(base.Handle, _intPtr);
			TimelineUtilities.SetControlCapture(this, false);
			_intPtr = IntPtr.Zero;
		}

		private void SetHoveredButton(TimelineCalendarButton hoveredButton)
		{
			if (_hoveredButton != hoveredButton)
			{
				if (_hoveredButton != null)
				{
					_hoveredButton.SetHovered(false);
				}
				_hoveredButton = hoveredButton;
				if (_hoveredButton != null)
				{
					_hoveredButton.SetHovered(true);
				}
			}
		}

		private void SetPressedButton(TimelineCalendarButton pressedButton)
		{
			if (_pressedButton != pressedButton)
			{
				if (_pressedButton != null)
				{
					_pressedButton.SetPressed(false);
				}
				_pressedButton = pressedButton;
				if (_pressedButton != null)
				{
					_pressedButton.SetPressed(true);
				}
			}
		}

		private void SelectToday()
		{
			CurrentDate = DateTime.Today;
			UnDrop(cancel: false);
		}

		private void SelectNone()
		{
			_isNoneSelected = true;
			UnDrop(cancel: false);
		}

		protected internal void ComputeDroppedControlSize()
		{
			_monthRowCount = 1;
			_monthColumnCount = 1;
			Size size = _monthLayoutTemplate.GetTotalSize();
			int totalHeight = size.Height + _borderSize.Height / 2;
			int contentWidth = size.Width + _borderSize.Width / 2;
			int footerTop = size.Height + _borderSize.Height / 2;
			if (_isTodayButtonVisible)
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!ShouldUseCompatibleTextRendering());
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
				using (Graphics graphics = CreateGraphics())
				{
					int fontHeight = (int)HeaderFont.GetHeight(graphics);
					totalHeight += fontHeight + 15;
					_footerBounds = new Rectangle(0, size.Height, size.Width, fontHeight + 15);
					int minButtonWidth = size.Width / 3;
					int todayButtonWidth = (int)textLayoutRenderer.RenderContent(graphics, _todayText, HeaderFont, 0).Width + 8;
					if (todayButtonWidth < minButtonWidth)
					{
						todayButtonWidth = minButtonWidth;
					}
					int availableWidth = size.Width - 3;
					if (todayButtonWidth * 2 > availableWidth)
					{
						if (_text.Length > 0 && _todayText.Length > 0)
						{
							todayButtonWidth = availableWidth / 2;
						}
						else if (_text.Length == 0)
						{
							todayButtonWidth = availableWidth;
						}
					}
					int buttonLeft = (contentWidth - todayButtonWidth) / 2;
					_todayButton._bounds = new Rectangle(buttonLeft, footerTop + 7, todayButtonWidth, fontHeight + 8);
					_noneButton._bounds = Rectangle.Empty;
				}
			}
			else
			{
				_footerBounds = Rectangle.Empty;
				_todayButton._bounds = Rectangle.Empty;
				_noneButton._bounds = Rectangle.Empty;
			}
			Size = new Size(size.Width + _borderSize.Width, totalHeight + _borderSize.Height);
		}
	}
}
