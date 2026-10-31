using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	/// <summary>Prints the scheduler.</summary>
	/// <remarks>When the default theme is light, the printout uses the theme colors. When it is dark, the job prints with the Standard theme (system colors on white paper); the theme of the screen does not change. See <see cref="PrintLook"/>.</remarks>
	public partial class MxSchedulerPrintDocument : PrintDocument
	{
		private IContainer _iContainer;

		private MonthViewHeaderNaming _monthViewHeaderNaming;

		private bool _showDateHeader = true;

		private MxScheduler _scheduler;

		private object _startTime;

		private TimeSpan _endTime = TimeSpan.Zero;

		private Font _headerFont;

		private Font _footerFont;

		private string _headerLeft;

		private string _headerCenter;

		private string _headerRight;

		private string _footerLeft;

		private bool _shouldNumberPages = true;

		private string _footerCenter;

		private string _footerRight;

		private Font _dateHeadingsFont = new Font("Tahoma", 24f);

		private string[] _textArray = new string[7];

		private Font _miniCalendarFont;

		private Font _boldMiniCalendarFont;

		private WorkingHoursSchemaLayoutState _workingHoursSchemaLayoutState;

		private int _rowHeight;

		private SchedulerHourRangeLayoutCollection _dayLayouts;

		private SchedulerOwnerLayoutCollection _headerLayouts;

		private Rectangle _gridBounds = Rectangle.Empty;

		private bool _grayShading = true;

		private Interval _interval;

		private int _dayColumnsLeft;

		private Font _timeLabelFont;

		private int _timeGridTop;

		private int _currentRowTop;

		private SchedulerDayOfWeek _schedulerDayOfWeek = SchedulerDayOfWeek.Sunday;

		private ArrayList _appointmentRenderers;

		private int _timeGridBottom;

		private int _allDayAppointmentTop;

		private int _dayColumnWidth;

		private int _extraAllDayRowCount;

		private Pen _appointmentOutlinePen;

		private SchedulerAppointment[] _currentDayLane;

		internal ArrayList _appointmentLanes;

		private int _minutesPerSlot;

		private int _appointmentTextLeft;

		private Brush _backgroundBrush;

		private Brush _textBrush;

		private ArrayList _allDayRenderers;

		private Font _monthFont;

		private Font _weekFont;

		private Font _dayFont;

		// Smaller copy of DayFont used while printing when the rows are too short for DayFont.
		private Font _fittedDayFont;

		private Rectangle _marginBounds;

		private int _pageNumber = 1;

		private int _pageWidth;

		private int _pageRight;

		private bool _isWeekendCompressed;

		private bool _compressWeekEnds;

		private int _currentDayIndex = -1;

		private int _totalPageCount = 1;

		private DayViewPrintMode _dayViewPrintMode;

		private LayoutPages _layoutPages;

		[Description("Specifies whether the header displaying the date is printed.")]
		[DefaultValue(true)]
		public bool ShowDateHeader
		{
			get
			{
				return _showDateHeader;
			}
			set
			{
				_showDateHeader = value;
			}
		}

		[Description("The Font object to apply to the text displayed on the header section.")]
		[Localizable(true)]
		public Font HeaderFont
		{
			get
			{
				if (_headerFont == null)
				{
					return Control.DefaultFont;
				}
				return _headerFont;
			}
			set
			{
				_headerFont = value;
			}
		}

		[Localizable(true)]
		[Description("The Font object to apply to the text displayed on the footer section.")]
		public Font FooterFont
		{
			get
			{
				if (_footerFont == null)
				{
					return Control.DefaultFont;
				}
				return _footerFont;
			}
			set
			{
				_footerFont = value;
			}
		}

		[Description("The Font object to apply to the text displayed on the date header section.")]
		[Localizable(true)]
		public Font DateHeadingsFont
		{
			get
			{
				return _dateHeadingsFont;
			}
			set
			{
				if (_dateHeadingsFont != value)
				{
					if (value == null)
					{
						throw new ArgumentNullException();
					}
					_dateHeadingsFont = value;
				}
			}
		}

		[Description("The Font object used by this print document when the Schedule is on month view.")]
		[Localizable(true)]
		public Font MonthFont
		{
			get
			{
				if (_monthFont != null || _scheduler == null)
				{
					return _monthFont;
				}
				return Scheduler.MonthFont;
			}
			set
			{
				_monthFont = value;
			}
		}

		[Localizable(true)]
		[Description("The Font object used by this print document when the Schedule is on week view.")]
		public Font WeekFont
		{
			get
			{
				if (_weekFont != null || _scheduler == null)
				{
					return _weekFont;
				}
				return Scheduler.WeekFont;
			}
			set
			{
				_weekFont = value;
			}
		}

		[Description("The Font object used by this print document when the Schedule is on day or work-week view.")]
		[Localizable(true)]
		public Font DayFont
		{
			get
			{
				if (_dayFont != null || _scheduler == null)
				{
					return _dayFont;
				}
				return Scheduler.DayFont;
			}
			set
			{
				_dayFont = value;
			}
		}

		[DefaultValue(DayViewPrintMode.PagePerDay)]
		[Description("Specifies how the Schedule will be printed when the Schedule is on DayView.")]
		public DayViewPrintMode DayViewPrintMode
		{
			get
			{
				return _dayViewPrintMode;
			}
			set
			{
				_dayViewPrintMode = value;
			}
		}

		public MxSchedulerPrintDocument()
		{
			_footerRight = DateTime.Now.ToShortDateString();
			_footerCenter = string.Empty;
			_footerLeft = string.Empty;
			_headerCenter = string.Empty;
			_headerLeft = string.Empty;
			_headerRight = string.Empty;
			_miniCalendarFont = new Font("Arial", 6f);
			_boldMiniCalendarFont = new Font("Arial", 6f, FontStyle.Bold);
			UpdateIContainer();
		}

		private Font PrintDayFont => _fittedDayFont ?? DayFont;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_iContainer?.Dispose();
				_miniCalendarFont?.Dispose();
				_miniCalendarFont = null;
				_boldMiniCalendarFont?.Dispose();
				_boldMiniCalendarFont = null;
				_timeLabelFont?.Dispose();
				_timeLabelFont = null;
				_fittedDayFont?.Dispose();
				_fittedDayFont = null;
				_appointmentOutlinePen?.Dispose();
				_appointmentOutlinePen = null;
			}
			base.Dispose(disposing);
		}

		private string ReplaceHeaderFooterPlaceholders(string textArgument, int pageNumber, int integerArgument)
		{
			string text = textArgument;
			text = ((_scheduler.TimeFormat != TimeFormat.TwelveHours) ? text.Replace("[Time Printed]", DateTime.Now.ToString("%H:mm")) : text.Replace("[Time Printed]", DateTime.Now.ToShortTimeString()));
			text = text.Replace("[Page #]", pageNumber.ToString());
			text = text.Replace("[Total Pages]", integerArgument.ToString());
			text = text.Replace("[Date Printed]", DateTime.Today.ToShortDateString());
			return text.Replace("[User Name]", SystemInformation.UserName);
		}

		private void UpdateIContainer()
		{
			_iContainer = new Container();
			new ResourceManager(typeof(MxSchedulerPrintDocument));
		}

		private void UpdateSchedulerPrintDocument()
		{
			bool shouldUseShortestDayNames = ShouldUseShortestDayNames();
			int dayOfWeekIndex = (int)SchedulerRenderer.ConvertToDayOfWeek(_scheduler.FirstDayOfWeek);
			for (int i = 0; i < 7; i++)
			{
				if (dayOfWeekIndex == 7)
				{
					dayOfWeekIndex = 0;
				}
				string abbreviatedDayName = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName((DayOfWeek)dayOfWeekIndex);
				abbreviatedDayName = (shouldUseShortestDayNames ? DateTimeFormatInfo.CurrentInfo.GetShortestDayName((DayOfWeek)dayOfWeekIndex) : abbreviatedDayName.Substring(0, 1));
				_textArray[i] = abbreviatedDayName;
				dayOfWeekIndex++;
			}
		}

		private SchedulerAppointmentOwnerLayoutState FindOwnerHeaderLayout(SchedulerAllDayArea headerControl, DateTime dateTime)
		{
			int dayIndex = 0;
			foreach (SchedulerAppointmentOwnerLayoutState item in headerControl._ownerHeaders)
			{
				if (item.GetDate().Date == dateTime && dayIndex == _currentDayIndex)
				{
					return item;
				}
				dayIndex++;
			}
			return null;
		}

		private int ComputeSlotHeight(int slotCount, int integerArgument, Interval interval)
		{
			switch (interval)
			{
			case Interval.OneMinute:
				_interval = Interval.TwoMinutes;
				break;
			case Interval.TwoMinutes:
				_interval = Interval.ThreeMinutes;
				break;
			case Interval.ThreeMinutes:
				_interval = Interval.FourMinutes;
				break;
			case Interval.FourMinutes:
				_interval = Interval.FiveMinutes;
				break;
			case Interval.TenMinutes:
				_interval = Interval.FifteenMinutes;
				break;
			case Interval.FifteenMinutes:
				_interval = Interval.TwentyMinutes;
				break;
			case Interval.TwentyMinutes:
				_interval = Interval.ThirtyMinutes;
				break;
			case Interval.ThirtyMinutes:
				_interval = Interval.SixtyMinutes;
				break;
			case Interval.SixMinutes:
				_interval = Interval.TenMinutes;
				break;
			case Interval.FiveMinutes:
				_interval = Interval.SixMinutes;
				break;
			}
			double visibleHours = EndTime.TotalHours - StartTime.TotalHours;
			int rowCount = (int)(visibleHours * (double)_interval);
			if (rowCount == 0)
			{
				rowCount++;
			}
			if (integerArgument / rowCount < 13 && _interval != Interval.SixtyMinutes)
			{
				return ComputeSlotHeight(slotCount, integerArgument, _interval);
			}
			if (integerArgument / rowCount > 13)
			{
				return integerArgument / rowCount;
			}
			return 13;
		}

		private void FitSlotHeight(int slotCount, Graphics graphics, int integerArgument)
		{
			int slotHeight = integerArgument / slotCount;
			Font dayFont = DayFont;
			int fontHeight = (int)dayFont.GetHeight(graphics);
			if (slotHeight < 13)
			{
				slotHeight = ComputeSlotHeight(slotCount, integerArgument, _scheduler.Interval);
			}
			if (fontHeight > slotHeight - 2)
			{
				double scale = (double)(slotHeight - 2) / (double)fontHeight;
				float emSize = dayFont.SizeInPoints - dayFont.SizeInPoints * (float)(1.0 - scale);
				if (_fittedDayFont == null || _fittedDayFont.Size != emSize || !_fittedDayFont.FontFamily.Equals(dayFont.FontFamily) || _fittedDayFont.Style != dayFont.Style || _fittedDayFont.Unit != dayFont.Unit)
				{
					_fittedDayFont?.Dispose();
					_fittedDayFont = new Font(dayFont.FontFamily, emSize, dayFont.Style, dayFont.Unit);
				}
			}
			else if (_fittedDayFont != null)
			{
				_fittedDayFont.Dispose();
				_fittedDayFont = null;
			}
			_rowHeight = slotHeight;
		}

		private void BuildMonthAppointmentViews()
		{
			int rowHeight = _rowHeight;
			_appointmentRenderers = new ArrayList();
			ArrayList arrayList = null;
			DateRange dateRange = new DateRange(_dayLayouts.GetLayoutAt(0).GetDate(), _dayLayouts.GetLayoutAt(_dayLayouts.Count - 1).GetDate());
			arrayList = ((!_scheduler.IsMultiOwnerActive() && _scheduler.CurrentOwner == null) ? _scheduler.Appointments.GetVisibleAppointmentsInRange(dateRange, true) : _scheduler.Appointments.GetAppointmentsForOwner(dateRange, true, _scheduler.CurrentOwner));
			foreach (SchedulerAppointment item in arrayList)
			{
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = _dayLayouts.FindLayoutByDate(item.StartTime.Date);
				if (schedulerHourRangeCollectionLayoutState == null || item.StartTime.Date < _dayLayouts.GetLayoutAt(0).GetDate())
				{
					schedulerHourRangeCollectionLayoutState = _dayLayouts.GetLayoutAt(0);
				}
				int left = schedulerHourRangeCollectionLayoutState._bounds.Left;
				int appointmentTop = schedulerHourRangeCollectionLayoutState._bounds.Top + rowHeight + 4;
				SchedulerAppointmentView appointmentRenderer = CreateAppointmentRenderer(left, appointmentTop, schedulerHourRangeCollectionLayoutState, item, GetViewFont(), false);
				if (appointmentRenderer != null)
				{
					appointmentRenderer._bounds = new Rectangle(schedulerHourRangeCollectionLayoutState._bounds.Left + 4, appointmentRenderer._rowTop, 0, rowHeight);
					if (appointmentRenderer._date == item.StartTime.Date)
					{
						appointmentRenderer.UpdateFirstSegment(true);
					}
					else
					{
						appointmentRenderer.SetContinuedFromPrevious(true);
					}
					LayoutMonthAppointment(appointmentRenderer, schedulerHourRangeCollectionLayoutState);
				}
			}
		}

		private SchedulerAppointmentView CreateAppointmentRenderer(int left, int top, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState, SchedulerAppointment appointment, Font font, bool shouldMarkOnlyThisDay)
		{
			top = FindFreeRowTop(left + 5, top);
			SchedulerAppointmentView result = null;
			DateTime dateTime = schedulerHourRangeCollectionLayoutState.GetDate();
			if (top + _rowHeight > schedulerHourRangeCollectionLayoutState._bounds.Bottom - 1)
			{
				try
				{
					if (!shouldMarkOnlyThisDay)
					{
						schedulerHourRangeCollectionLayoutState.SetHasHiddenAppointments(true);
						int dayIndex = schedulerHourRangeCollectionLayoutState.GetDayIndex();
						dateTime = schedulerHourRangeCollectionLayoutState.GetDate().AddDays(1.0);
						while (dateTime < appointment.EndTime && dayIndex < _dayLayouts.Count - 1)
						{
							dayIndex++;
							dateTime = dateTime.AddDays(1.0);
							_dayLayouts.GetLayoutAt(dayIndex).SetHasHiddenAppointments(true);
						}
					}
					else
					{
						schedulerHourRangeCollectionLayoutState.SetHasHiddenAppointments(true);
					}
				}
				catch
				{
					return null;
				}
				return result;
			}
			result = new SchedulerAppointmentView(appointment, _rowHeight, (int)_interval, allDay: false, _scheduler.View, schedulerHourRangeCollectionLayoutState.GetDate(), _scheduler, useCompatibleTextRendering: true);
			result._appointmentFont = font;
			result._rowTop = top;
			result._showTimeAsClocks = _scheduler.ShowTimeAsClocks;
			if ((int)appointment.Duration.TotalDays >= 1)
			{
				result._isMultiDay = true;
			}
			else
			{
				result._isMultiDay = false;
			}
			_appointmentRenderers.Add(result);
			return result;
		}

		private int FindFreeRowTop(int x, int integerArgument)
		{
			foreach (SchedulerAppointmentView item in _appointmentRenderers)
			{
				if (item._bounds.Contains(x, integerArgument + 1))
				{
					integerArgument += _rowHeight + 4;
					return FindFreeRowTop(x, integerArgument);
				}
			}
			return integerArgument;
		}

		internal Pen CreateLightLightPen(Color baseColor)
		{
			if (baseColor.IsEmpty)
			{
				baseColor = ControlPaint.LightLight(baseColor);
			}
			return new Pen(baseColor);
		}

		private void UpdateTimeLabelFont()
		{
			FontStyle fontStyle = FontStyle.Regular;
			Font dayFont = PrintDayFont;
			string name = dayFont.FontFamily.Name;
			if (_scheduler.TimeNavigatorFormatStyle.FontUnderline == TriState.True)
			{
				fontStyle |= FontStyle.Underline;
			}
			if (_scheduler.TimeNavigatorFormatStyle.FontBold == TriState.True)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (_scheduler.TimeNavigatorFormatStyle.FontItalic == TriState.True)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (_scheduler.TimeNavigatorFormatStyle.FontStrikeout == TriState.True)
			{
				fontStyle |= FontStyle.Strikeout;
			}
			using (FontFamily fontFamily = new FontFamily(name))
			{
				if (!fontFamily.IsStyleAvailable(fontStyle))
				{
					if (!fontFamily.IsStyleAvailable(FontStyle.Bold))
					{
						if (fontFamily.IsStyleAvailable(FontStyle.Italic))
						{
							fontStyle = FontStyle.Italic;
						}
						else if (fontFamily.IsStyleAvailable(FontStyle.Strikeout))
						{
							fontStyle = FontStyle.Strikeout;
						}
						else
						{
							if (!fontFamily.IsStyleAvailable(FontStyle.Underline))
							{
								if (_timeLabelFont != null)
								{
									_timeLabelFont.Dispose();
									_timeLabelFont = null;
								}
								_timeLabelFont = (Font)dayFont.Clone();
								return;
							}
							fontStyle = FontStyle.Underline;
						}
					}
					else
					{
						fontStyle = FontStyle.Bold;
					}
				}
			}
			if (_timeLabelFont != null)
			{
				_timeLabelFont.Dispose();
				_timeLabelFont = null;
			}
			if (_interval != Interval.SixtyMinutes)
			{
				_timeLabelFont = new Font(name, (int)dayFont.Size * 2, fontStyle);
			}
			else
			{
				_timeLabelFont = new Font(name, dayFont.SizeInPoints, fontStyle);
			}
		}

		private void PositionAllDayAppointment(SchedulerAppointmentView appointmentRenderer, int width, bool isFirstDay, bool booleanArgument, int left)
		{
			width -= 10;
			int top = _rowHeight + _marginBounds.Top + 5;
			if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
			{
				top += _rowHeight;
			}
			_allDayAppointmentTop = FindFreeAllDayTop(left, top);
			appointmentRenderer._bounds = new Rectangle(left, _allDayAppointmentTop, width, _rowHeight);
			appointmentRenderer.UpdateFirstSegment(isFirstDay);
			appointmentRenderer.UpdateLastSegment(booleanArgument);
			if (!isFirstDay)
			{
				appointmentRenderer.SetContinuedFromPrevious(true);
			}
			if (!booleanArgument)
			{
				appointmentRenderer.SetContinuedToNext(true);
			}
			if (_allDayAppointmentTop > _timeGridTop - _rowHeight)
			{
				_timeGridTop = _allDayAppointmentTop + _rowHeight;
				_extraAllDayRowCount++;
			}
		}

		internal Pen GetAppointmentOutlinePen()
		{
			if (_appointmentOutlinePen == null)
			{
				if (_grayShading)
				{
					_appointmentOutlinePen = new Pen(_scheduler.AppointmentOutLineColor);
				}
				else
				{
					_appointmentOutlinePen = new Pen(Color.Black);
				}
			}
			return _appointmentOutlinePen;
		}

		internal bool AreSlotsFree(SchedulerAppointment[] schedulerAppointmentArray, SchedulerAppointment schedulerAppointment, DateTime dateTime)
		{
			int durationMinutes = (int)schedulerAppointment.GetDurationOnDate(dateTime).TotalMinutes;
			int startMinutes = (int)schedulerAppointment.GetStartTimeOnDate(dateTime).TotalMinutes;
			int slotCount = durationMinutes / _minutesPerSlot;
			if (durationMinutes % _minutesPerSlot != 0 || startMinutes % _minutesPerSlot != 0)
			{
				slotCount++;
			}
			else if (slotCount == 0)
			{
				slotCount++;
			}
			int startSlot = startMinutes / _minutesPerSlot;
			if (slotCount + startSlot > schedulerAppointmentArray.Length)
			{
				slotCount = schedulerAppointmentArray.Length - startSlot;
			}
			for (int i = startSlot; i < startSlot + slotCount; i++)
			{
				if (schedulerAppointmentArray[i] != null)
				{
					return false;
				}
			}
			return true;
		}

		internal void AddDayAppointmentView(SchedulerAppointment appointment, DateTime date, int startSlot, int integerArgument, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			if (schedulerHourRangeCollectionLayoutState != null)
			{
				SchedulerAppointmentView appointmentRenderer = new SchedulerAppointmentView(appointment, _rowHeight, (int)_interval, allDay: false, SchedulerView.DayView, date, _scheduler, useCompatibleTextRendering: true);
				appointmentRenderer._appointmentFont = PrintDayFont;
				appointmentRenderer.SetStartTimeOfDay(new TimeSpan(0, startSlot * _minutesPerSlot, 0));
				appointmentRenderer.SetSchedulerRenderer4TimeSpan(new TimeSpan(0, (startSlot + integerArgument) * _minutesPerSlot, 0));
				if (_scheduler.IsMultiOwnerActive())
				{
					appointmentRenderer.SetSchedulerAppointmentOwner(schedulerHourRangeCollectionLayoutState.GetOwner());
				}
				else
				{
					appointmentRenderer.SetSchedulerAppointmentOwner(null);
				}
				_appointmentRenderers.Add(appointmentRenderer);
			}
		}

		private void ClearAppointmentRenderers()
		{
			if (_appointmentRenderers == null)
			{
				return;
			}
			foreach (SchedulerAppointmentView item in _appointmentRenderers)
			{
				item.StopTimerAndHideToolTip();
			}
			_appointmentRenderers.Clear();
		}

		private Font GetFormatStyleFont(SchedulerFormatStyle schedulerFormatStyle)
		{
			if (_scheduler.View == SchedulerView.MonthView)
			{
				return schedulerFormatStyle.ResolveFont(MonthFont);
			}
			if (_scheduler.View == SchedulerView.WeekView)
			{
				return schedulerFormatStyle.ResolveFont(WeekFont);
			}
			return schedulerFormatStyle.ResolveFont(PrintDayFont);
		}

		private Font GetViewFont()
		{
			if (_scheduler.View == SchedulerView.MonthView)
			{
				return MonthFont;
			}
			if (_scheduler.View == SchedulerView.WeekView)
			{
				return WeekFont;
			}
			return PrintDayFont;
		}

		protected override void OnBeginPrint(PrintEventArgs e)
		{
			base.OnBeginPrint(e);
			if (_scheduler == null)
			{
				e.Cancel = true;
			}
			_currentDayIndex = -1;
			_printLook?.Dispose();
			_printLook = e.Cancel ? null : PrintLook.Begin();
		}

		// A dark theme does not print: the default theme is the Standard theme until the job ends (see PrintLook).
		private IDisposable _printLook;

		protected override void OnEndPrint(PrintEventArgs e)
		{
			base.OnEndPrint(e);
			_printLook?.Dispose();
			_printLook = null;
			if (_fittedDayFont != null)
			{
				_fittedDayFont.Dispose();
				_fittedDayFont = null;
			}
		}
	}
}
