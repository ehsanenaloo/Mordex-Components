using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	internal partial class SchedulerAppointmentView
	{

		private Timer _editDelayTimer;

		private bool _useCompatibleTextRendering;

		public bool _isShown;

		private bool _isPointerOverText;

		private Pen _borderPen;

		public Rectangle _bounds = Rectangle.Empty;

		private MxScheduler _scheduler;

		internal int _cellWidth;

		private bool _allDay;

		private SchedulerAppointmentOwner _schedulerAppointmentOwner;

		internal bool _isWithinDay = true;

		private int _slotsPerHour;

		internal int _rowHeight;

		internal DateTime _date;

		private AppointmentState _appointmentState = AppointmentState.Normal;

		public Font _appointmentFont;

		private SchedulerView _view;

		private AllowAppointmentDrag _allowAppointmentDrag;

		private bool _allowEdit = true;

		private bool _wasDoubleClicked;

		private bool _wasFocusedOnMouseDown;

		private SchedulerAppointment _appointment;

		private Rectangle _focusBounds = Rectangle.Empty;

		private Rectangle _schedulerRenderer4Rectangle = Rectangle.Empty;

		private Rectangle _bodyBounds = Rectangle.Empty;

		private bool _isFirstSegment = true;

		private bool _isContinuedToNext;

		private bool _isContinuedFromPrevious;

		private bool _isLastSegment = true;

		private SchedulerFormatStyle _schedulerFormatStyle;

		// What the cached effective style was built from. The style is built again when one of them changes or the scheduler
		// says that an input of the appointment styles changed (theme, style properties, conditions, data, size).
		private int _styleVersion;

		private SchedulerAppointment _styleAppointment;

		private AppointmentState _styleState;

		private SchedulerView _styleView;

		private bool _styleAllDay;

		private bool _styleFocused;

		private Color _styleForeColor;

		private Color _styleBackColor;

		// For tests: how many times an effective appointment style was built on this thread.
		[ThreadStatic]
		internal static long StylesBuilt;

		internal Color _backColor = Color.Empty;

		internal Color _schedulerRenderer4Color = Color.Empty;

		public bool _isMultiDay;

		public bool _showTimeAsClocks = true;

		private Rectangle _timeAreaBounds = Rectangle.Empty;

		internal int _rowTop;

		private TimeSpan _timeSpan;

		private TimeSpan _schedulerRenderer4TimeSpan;

		internal ImageList _imageList;

		private ToolTip _toolTip;

		private string _schedulerRenderer4Text = string.Empty;

		internal bool _hasHiddenText;

		internal int _imageX;

		internal int _textStartX;

		internal int _textAreaWidth;

		internal int _imageWidth;

		internal bool _isStartInView = true;

		internal bool _isEndInView = true;

		private Brush _borderBrush;

		private Brush _schedulerRenderer4Brush;

		private DragType _dragType;

		private SchedulerWindowMessageHandler _schedulerWindowMessageHandler;

		private Point _mouseDownPosition = Point.Empty;

		internal bool _isMouseDown;

		private Image GetRecurrenceImage()
		{
			if (_appointment.IsRecurring)
			{
				if (_appointment._recurrenceState == RecurrenceState.Exception)
				{
					int recurrenceExceptionImageIndex = _scheduler.RecurrenceExceptionImageIndex;
					if (recurrenceExceptionImageIndex == -2)
					{
						return SchedulerRenderer.MakeMagentaTransparent(SchedulerImageResources.GetRecurrenceExceptionBitmap());
					}
					if (recurrenceExceptionImageIndex >= 0 && _imageList != null && recurrenceExceptionImageIndex < _imageList.Images.Count)
					{
						return _imageList.Images[recurrenceExceptionImageIndex];
					}
				}
				else
				{
					int recurrenceImageIndex = _scheduler.RecurrenceImageIndex;
					if (recurrenceImageIndex == -2)
					{
						return SchedulerRenderer.MakeMagentaTransparent(SchedulerImageResources.GetRecurrenceBitmap());
					}
					if (recurrenceImageIndex >= 0 && _imageList != null && recurrenceImageIndex < _imageList.Images.Count)
					{
						return _imageList.Images[recurrenceImageIndex];
					}
				}
			}
			return null;
		}

		private Image GetReminderImage()
		{
			if (_appointment.Reminder.Enabled)
			{
				if (_appointment.RecurrenceState == RecurrenceState.Occurrence && _appointment.RecurrencePattern != null && _appointment.RecurrencePattern.GetEffectiveReminderDismissTime() >= _appointment.Reminder.GetReminderDateTime())
				{
					return null;
				}
				int reminderImageIndex = _scheduler.ReminderImageIndex;
				if (reminderImageIndex == -2)
				{
					return SchedulerRenderer.MakeMagentaTransparent(SchedulerImageResources.GetReminderBitmap());
				}
				if (reminderImageIndex >= 0 && _imageList != null && reminderImageIndex < _imageList.Images.Count)
				{
					return _imageList.Images[reminderImageIndex];
				}
			}
			return null;
		}

		internal SchedulerAppointmentView(SchedulerAppointment appointment, int rowHeight, int interval, bool allDay, SchedulerView view, DateTime date, MxScheduler scheduler, bool useCompatibleTextRendering)
		{
			_useCompatibleTextRendering = useCompatibleTextRendering;
			_slotsPerHour = 60 / interval;
			_appointment = appointment;
			_rowHeight = rowHeight;
			_allDay = allDay;
			_view = view;
			_date = date;
			_scheduler = scheduler;
			_imageList = scheduler.GetImageList();
			_allowEdit = scheduler.AllowEdit;
			_allowAppointmentDrag = scheduler.AllowAppointmentDrag;
			UpdateTimer();
		}

		public bool GetAllDay()
		{
			return _allDay;
		}

		public SchedulerAppointmentOwner GetSchedulerAppointmentOwner()
		{
			return _schedulerAppointmentOwner;
		}

		public void SetSchedulerAppointmentOwner(SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			_schedulerAppointmentOwner = schedulerAppointmentOwner;
		}

		public void UpdateSlotsPerHour(int slotsPerHour)
		{
			if (_slotsPerHour != slotsPerHour)
			{
				_slotsPerHour = slotsPerHour;
			}
		}

		public void StopTimerAndHideToolTip()
		{
			_editDelayTimer.Stop();
			HideToolTip();
		}

		internal bool IsOutsideDragRectangle(int screenX, int integerArgument)
		{
			Size dragSize = SystemInformation.DragSize;
			if (!new Rectangle(_mouseDownPosition.X - dragSize.Width, _mouseDownPosition.Y - dragSize.Height, dragSize.Width * 2, dragSize.Height * 2).Contains(screenX, integerArgument))
			{
				return true;
			}
			return false;
		}

		internal void UpdateLayout(AppointmentState appointmentState)
		{
			if (_appointmentState == appointmentState)
			{
				return;
			}
			_appointmentState = appointmentState;
			_editDelayTimer.Stop();
			if (_view == SchedulerView.DayView)
			{
				if (_appointmentState == AppointmentState.Focus)
				{
					_bounds.Y -= 5;
					_bounds.Height += 10;
					_focusBounds = _bounds;
					_schedulerRenderer4Rectangle = new Rectangle(_bounds.Left, _bounds.Top + 5, 6, _bounds.Height - 10);
					_bodyBounds = new Rectangle(_schedulerRenderer4Rectangle.Right - 1, _schedulerRenderer4Rectangle.Top, _bounds.Right - _schedulerRenderer4Rectangle.Right + 1, _schedulerRenderer4Rectangle.Height);
					if (_scheduler.GetEditor() != null)
					{
						_scheduler.GetEditor()._textBoxControl.Font = _appointmentFont;
					}
				}
				else
				{
					_bounds.Y += 5;
					_bounds.Height -= 10;
					_focusBounds = Rectangle.Empty;
					_schedulerRenderer4Rectangle = _bounds;
					_schedulerRenderer4Rectangle.Width = 6;
					_bodyBounds = new Rectangle(_schedulerRenderer4Rectangle.Right - 1, _schedulerRenderer4Rectangle.Top, _bounds.Right - _schedulerRenderer4Rectangle.Right + 1, _schedulerRenderer4Rectangle.Height);
					CloseEditorIfOpen();
				}
			}
			else if (_appointmentState == AppointmentState.Focus)
			{
				if (_scheduler.GetEditor() != null)
				{
					_scheduler.GetEditor()._textBoxControl.Font = _appointmentFont;
				}
			}
			else
			{
				CloseEditorIfOpen();
			}
			_scheduler.Invalidate(_bounds);
		}

		private void UpdateTimer()
		{
			_editDelayTimer = new Timer();
			_editDelayTimer.Interval = 506;
			_editDelayTimer.Tick += OnTimerTick;
		}

		public SchedulerView GetView()
		{
			return _view;
		}

		public void UpdateView(SchedulerView view)
		{
			if (_view != view)
			{
				_view = view;
			}
		}

		public void SetAllowAppointmentDrag(AllowAppointmentDrag allowAppointmentDrag)
		{
			_allowAppointmentDrag = allowAppointmentDrag;
		}

		public void SetAllowEdit(bool allowEdit)
		{
			_allowEdit = allowEdit;
		}

		internal void CloseEditorIfOpen()
		{
			if (_scheduler.GetEditor() != null && _scheduler.GetEditor()._app.Equals(_appointment))
			{
				_scheduler.CommitEdit();
			}
		}

		internal void StartEditTimerIfAllowed()
		{
			if (_allowEdit && _appointmentState == AppointmentState.Focus && CanBeginEdit())
			{
				_editDelayTimer.Start();
			}
		}

		internal void AddToViewAndBeginEdit()
		{
			_appointment._selected = true;
			_isShown = true;
			if (_scheduler._isAppointmentLayoutStale)
			{
				_scheduler.Update();
			}
			if (_scheduler.View == SchedulerView.DayView && _allDay)
			{
				_scheduler._allDayAppointmentViews.Add(this);
			}
			else
			{
				_scheduler._appointmentViews.Add(this);
			}
			_scheduler.ShowEditor(this, "");
		}

		private bool CanBeginEdit()
		{
			if ((_scheduler.GetEditor() == null || _scheduler.GetEditor()._app.Equals(_appointment)) && !_editDelayTimer.Enabled && _isPointerOverText)
			{
				if (_scheduler.SelectedAppointments != null && _scheduler.SelectedAppointments.Count > 1)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		internal void UpdateSubBoundsAndInvalidate()
		{
			if (_view == SchedulerView.DayView)
			{
				if (_appointmentState == AppointmentState.Focus)
				{
					_bounds.Height += 10;
					_bounds.Y -= 5;
					_focusBounds = _bounds;
					_schedulerRenderer4Rectangle = new Rectangle(_bounds.Left, _bounds.Top + 5, 6, _focusBounds.Height - 10);
					_bodyBounds = new Rectangle(_schedulerRenderer4Rectangle.Right - 1, _schedulerRenderer4Rectangle.Top, _bounds.Right - _schedulerRenderer4Rectangle.Right + 1, _schedulerRenderer4Rectangle.Height);
				}
				else
				{
					_focusBounds = Rectangle.Empty;
					_schedulerRenderer4Rectangle = new Rectangle(_bounds.Left, _bounds.Top, 6, _bounds.Height);
					_bodyBounds = new Rectangle(_schedulerRenderer4Rectangle.Right - 1, _schedulerRenderer4Rectangle.Top, _bounds.Right - _schedulerRenderer4Rectangle.Right + 1, _schedulerRenderer4Rectangle.Height);
				}
			}
			_scheduler.Invalidate(_bounds);
		}

		public SchedulerAppointment GetAppointment()
		{
			return _appointment;
		}

		public void UpdateAppointment(SchedulerAppointment appointment)
		{
			if (_appointment != appointment)
			{
				_appointment = appointment;
			}
		}

		public bool IsFirstSegment()
		{
			return _isFirstSegment;
		}

		public void UpdateFirstSegment(bool isFirstSegment)
		{
			if (_isFirstSegment != isFirstSegment)
			{
				_isFirstSegment = isFirstSegment;
			}
		}

		public bool IsContinuedToNext()
		{
			return _isContinuedToNext;
		}

		public void SetContinuedToNext(bool isContinuedToNext)
		{
			_isContinuedToNext = isContinuedToNext;
		}

		public void SetContinuedFromPrevious(bool isContinuedFromPrevious)
		{
			_isContinuedFromPrevious = isContinuedFromPrevious;
		}

		public bool IsLastSegment()
		{
			return _isLastSegment;
		}

		public void UpdateLastSegment(bool isLastSegment)
		{
			if (_isLastSegment != isLastSegment)
			{
				_isLastSegment = isLastSegment;
			}
		}

		public SchedulerFormatStyle GetEffectiveFormatStyle()
		{
			bool focused = _scheduler.ContainsFocus;
			if (_schedulerFormatStyle != null && (_styleVersion != _scheduler._appointmentStyleVersion || !ReferenceEquals(_styleAppointment, _appointment) || _styleState != _appointmentState || _styleView != _view || _styleAllDay != _allDay || _styleFocused != focused))
			{
				ReleaseEffectiveFormatStyle();
			}
			if (_schedulerFormatStyle != null)
			{
				_schedulerRenderer4Color = _styleForeColor;
				_backColor = _styleBackColor;
			}
			if (_schedulerFormatStyle == null)
			{
				StylesBuilt++;
				_styleVersion = _scheduler._appointmentStyleVersion;
				_styleAppointment = _appointment;
				_styleState = _appointmentState;
				_styleView = _view;
				_styleAllDay = _allDay;
				_styleFocused = focused;
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.AppointmentsFormatStyle);
				_schedulerFormatStyle = schedulerFormatStyle;
				SchedulerFormatStyle mergedStyle = new SchedulerFormatStyle(_schedulerFormatStyle);
				foreach (SchedulerFormatCondition formatCondition in _scheduler.FormatConditions)
				{
					if (formatCondition.Enabled && formatCondition.MatchesAppointment(_appointment))
					{
						mergedStyle = ((!formatCondition.AllowMerge) ? _schedulerFormatStyle.MergeWith(formatCondition.FormatStyle) : mergedStyle.MergeWith(formatCondition.FormatStyle));
					}
				}
				_schedulerFormatStyle = mergedStyle;
				if (!_appointment.FormatStyle.HasOnlyDefaultValues())
				{
					_schedulerFormatStyle = _schedulerFormatStyle.MergeWith(_appointment.FormatStyle);
				}
				_schedulerRenderer4Color = _schedulerFormatStyle.ForeColor;
				_backColor = _schedulerFormatStyle.BackColor;
				_styleForeColor = _schedulerRenderer4Color;
				_styleBackColor = _backColor;
				if (_appointmentState == AppointmentState.Focus && (_view != SchedulerView.DayView || _allDay))
				{
					if (focused)
					{
						_schedulerFormatStyle = _schedulerFormatStyle.MergeWith(_scheduler.SelectedFormatStyle);
					}
					else
					{
						_schedulerFormatStyle = _schedulerFormatStyle.MergeWith(_scheduler.SelectedInactiveFormatStyle);
					}
				}
			}
			return _schedulerFormatStyle;
		}

		public TimeSpan GetStartTimeOfDay()
		{
			return _timeSpan;
		}

		public void SetStartTimeOfDay(TimeSpan timeSpan)
		{
			_timeSpan = timeSpan;
		}

		public TimeSpan GetSchedulerRenderer4TimeSpan()
		{
			return _schedulerRenderer4TimeSpan;
		}

		public void SetSchedulerRenderer4TimeSpan(TimeSpan schedulerRenderer4TimeSpan)
		{
			_schedulerRenderer4TimeSpan = schedulerRenderer4TimeSpan;
		}

		internal bool HasAnyImage(int firstImageIndex, int integerArgument, int thirdImageIndex, int fourthImageIndex)
		{
			if (_scheduler._isEditingAppointment && _scheduler.GetEditor() != null && _scheduler.GetEditor()._app.Equals(_appointment))
			{
				return false;
			}
			if (_imageList != null)
			{
				if (firstImageIndex != -1)
				{
					return true;
				}
				if (integerArgument != -1)
				{
					return true;
				}
				if (thirdImageIndex != -1)
				{
					return true;
				}
				if (fourthImageIndex != -1)
				{
					return true;
				}
			}
			if (GetReminderImage() != null)
			{
				return true;
			}
			if (GetRecurrenceImage() != null)
			{
				return true;
			}
			return false;
		}

		private bool ShouldShowTimeHint()
		{
			if (_scheduler.ShowTimeHintOnAppointments == TimeHintOnAppointments.Never)
			{
				return false;
			}
			if (_scheduler.ShowTimeHintOnAppointments == TimeHintOnAppointments.Always)
			{
				return true;
			}
			if (_timeSpan != _appointment.StartTime.TimeOfDay)
			{
				return true;
			}
			TimeSpan field_ = _scheduler._firstVisibleSlotTime;
			field_ = field_.Add(new TimeSpan(0, _scheduler.GetVerticalScrollBarControl().GetLargeChange() * _scheduler.MinuteInterval, 0));
			if (_schedulerRenderer4TimeSpan > field_)
			{
				return true;
			}
			return false;
		}

		public Brush GetBorderBrush(Color color)
		{
			if (_borderBrush == null)
			{
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.SelectedFormatStyle);
				schedulerFormatStyle.BackColor = color;
				schedulerFormatStyle.BackgroundGradientMode = BackgroundGradientMode.Solid;
				_borderBrush = schedulerFormatStyle.GetBackBrush(Rectangle.Empty);
			}
			return _borderBrush;
		}

		public Brush GetBarBackgroundBrush()
		{
			if (_schedulerRenderer4Brush == null)
			{
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.SelectedFormatStyle);
				schedulerFormatStyle.BackColor = _scheduler.GetThemedBackColor();
				schedulerFormatStyle.BackgroundGradientMode = BackgroundGradientMode.Solid;
				_schedulerRenderer4Brush = schedulerFormatStyle.GetBackBrush(Rectangle.Empty);
			}
			return _schedulerRenderer4Brush;
		}

		private void DisposeResources()
		{
			if (_schedulerRenderer4Brush != null)
			{
				_schedulerRenderer4Brush.Dispose();
				_schedulerRenderer4Brush = null;
			}
			if (_borderBrush != null)
			{
				_borderBrush.Dispose();
				_borderBrush = null;
			}
			// The effective style is kept for the next draw; a print draw does not keep it.
			if (_scheduler == null || _scheduler._isPrinting)
			{
				ReleaseEffectiveFormatStyle();
			}
		}

		internal void ReleaseEffectiveFormatStyle()
		{
			if (_schedulerFormatStyle != null)
			{
				_schedulerFormatStyle.DisposeResources();
				_schedulerFormatStyle = null;
			}
			_styleAppointment = null;
		}

		public void HideToolTip()
		{
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
			if (_schedulerWindowMessageHandler != null)
			{
				_schedulerWindowMessageHandler.DisposeAll();
				_schedulerWindowMessageHandler = null;
			}
		}

		private void ShowToolTip()
		{
			if (!_scheduler.ShowToolTips)
			{
				return;
			}
			Form form = null;
			try
			{
				form = _scheduler.FindForm();
			}
			finally
			{
			}
			if (form != null && !form.ContainsFocus)
			{
				return;
			}
			if (_schedulerWindowMessageHandler == null)
			{
				_schedulerWindowMessageHandler = new SchedulerWindowMessageHandler();
				string text = "";
				text = ((GetAppointment().Prefix != "") ? (GetAppointment().Prefix + " " + GetAppointment().Text) : ((!(_scheduler.AppointmentsPrefix != "")) ? GetAppointment().Text : (_scheduler.AppointmentsPrefix + " " + GetAppointment().Text)));
				string hintText = "";
				hintText = ((_scheduler.ShowTimeHintOnAppointments == TimeHintOnAppointments.Never) ? text : ((_scheduler.TimeFormat != TimeFormat.TwelveHours) ? (_appointment.StartTime.ToString("%H:mm") + " " + _appointment.EndTime.ToString("%H:mm") + "  " + text) : (_appointment.StartTime.ToShortTimeString() + " " + _appointment.EndTime.ToShortTimeString() + "  " + text)));
				if (_appointment.Description != "")
				{
					hintText = hintText + "\n" + _appointment.Description;
				}
				Point location = _bounds.Location;
				location.Y += _bounds.Height;
				location = _scheduler.PointToScreen(location);
				if (_bounds.Width > 120)
				{
					_schedulerWindowMessageHandler.SetRequestedSize(new Size(_bounds.Width, 0));
				}
				_schedulerWindowMessageHandler.SetFont(_scheduler.GetViewFont(null));
				_schedulerWindowMessageHandler.SetAutoHideDelay(0);
				_schedulerWindowMessageHandler.ShowToolTip(hintText, location, _scheduler);
			}
		}

		private string FormatTime(DateTime dateTime)
		{
			if (_scheduler.TimeFormat == TimeFormat.TwentyFourHours)
			{
				return dateTime.ToString("%H:mm");
			}
			return dateTime.ToString("t", Application.CurrentCulture);
		}

		private Rectangle GetEditorBounds(Graphics graphics)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.UpdateWrapsText(false);
			Rectangle empty = Rectangle.Empty;
			string text = _appointment._startTime.ToString("t", Application.CurrentCulture);
			string timeRangeText = text + " " + _appointment._endTime.ToString("t", Application.CurrentCulture);
			int timeTextWidth = (int)textLayoutRenderer.RenderContent(graphics, timeRangeText, _appointmentFont, 0).Width;
			if (_bounds.Width - timeTextWidth < 50)
			{
				timeTextWidth = (int)textLayoutRenderer.RenderContent(graphics, text, _appointmentFont, 0).Width;
			}
			empty = ((_bounds.Width - timeTextWidth >= 50) ? new Rectangle(_bounds.Left + timeTextWidth + 2, _bounds.Top + 1, _bounds.Width - timeTextWidth - 3, _bounds.Height - 2) : new Rectangle(_bounds.Left + 1, _bounds.Top + 1, _bounds.Width - 2, _bounds.Height - 2));
			textLayoutRenderer.ResetFormat();
			return empty;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_editDelayTimer != null)
			{
				_editDelayTimer.Stop();
			}
			if (_scheduler != null && _appointment != null)
			{
				_scheduler.ShowEditor(this, _appointment._text);
			}
		}
	}
}
