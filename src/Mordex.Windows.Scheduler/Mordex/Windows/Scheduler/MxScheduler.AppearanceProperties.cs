using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		TextBox ITextBoxOwner.TextBoxControl
		{
			get
			{
				if (_appointmentEditor != null)
				{
					return _appointmentEditor._textBoxControl;
				}
				return null;
			}
		}

		int ISchedulerFormatStyleOwner.DefaultAlpha => DefaultAlpha;

		AlphaMode ISchedulerFormatStyleOwner.BackColorAlphaMode
		{
			get
			{
				if (_schedulerBackColorAlphaMode == AlphaMode.Empty)
				{
					return _backColorAlphaMode;
				}
				return _schedulerBackColorAlphaMode;
			}
		}

		float ISchedulerFormatStyleOwner.DefaultBlend => DefaultBlend;

		Color ISchedulerFormatStyleOwner.BlendColor => BlendColor;

		AlphaMode ISchedulerFormatStyleOwner.ForeColorAlphaMode
		{
			get
			{
				if (_foreColorAlphaMode == AlphaMode.Empty)
				{
					return _backColorAlphaMode;
				}
				return _foreColorAlphaMode;
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the minutes should be displayed in the time navigator bar.")]
		[Category("Behavior")]
		public bool ShowMinutesInTimeNavigator
		{
			get
			{
				return _showMinutesInTimeNavigator;
			}
			set
			{
				if (_showMinutesInTimeNavigator != value)
				{
					_showMinutesInTimeNavigator = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether a vertical scroll bar should be displayed in the all day area of the Schedule.")]
		public TriState ShowAllDayAreaScrollBar
		{
			get
			{
				return _showAllDayAreaScrollBar;
			}
			set
			{
				if (_showAllDayAreaScrollBar != value)
				{
					_showAllDayAreaScrollBar = value;
					if (_view == SchedulerView.DayView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[DefaultValue(0f)]
		[Description("The default percentage of the BlendColor to be used.")]
		[Category("Appearance")]
		public float DefaultBlend
		{
			get
			{
				return _defaultBlend;
			}
			set
			{
				if (_defaultBlend != value)
				{
					if (value < 0f || value > 1f)
					{
						throw new ArgumentOutOfRangeException("DefaultBlend", "The value must be between 0 and 1");
					}
					_defaultBlend = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Description("Gets or sets the color that blend with the BackColor and BackColorGradient properties of the SchedulerFormatStyle and SchedulerFormatStyle objects.")]
		[Category("Appearance")]
		public Color BlendColor
		{
			get
			{
				if (_blendColor.IsEmpty)
				{
					return SystemColors.Window;
				}
				return _blendColor;
			}
			set
			{
				if (!_blendColor.Equals(value))
				{
					_blendColor = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		[Description("The SchedulerFormatStyle object used to draw the background of the rows in day or work week views.")]
		public SchedulerFormatStyle HourBackgroundStyle => _hourBackgroundStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		[Description("The SchedulerFormatStyle object used to draw the days when the Schedule is in week view.")]
		public SchedulerFormatStyle WeekDaysBackgroundStyle => _weekDaysBackgroundStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		[Description("The SchedulerFormatStyle object used to draw the days when the Schedule is in month view.")]
		public SchedulerFormatStyle MonthDaysFormatStyle => _monthDaysFormatStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		[Description("The SchedulerFormatStyle object used to draw the days of the odd months when the Schedule is in month view.")]
		public SchedulerFormatStyle AlternatingMonthDaysFormatStyle => _alternatingMonthDaysFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object used to draw the background of alternating rows in day or work week views.")]
		public SchedulerFormatStyle AlternatingHourBackgroundStyle => _alternatingHourBackgroundStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Appearance")]
		[Description("The SchedulerFormatStyle object used to draw the selected items in the Schedule.")]
		public SchedulerFormatStyle SelectedFormatStyle => _selectedFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		[Description("The SchedulerFormatStyle object used to draw the selected items in the Schedule when the control does not have the focus.")]
		public SchedulerFormatStyle SelectedInactiveFormatStyle => _selectedInactiveFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		[Description("Returns the SchedulerFormatStyle object used to draw the time navigator of the Schedule.")]
		public SchedulerFormatStyle TimeNavigatorFormatStyle => _timeNavigatorFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		[Description("The SchedulerFormatStyle used to draw the headers in the Schedule control.")]
		public SchedulerFormatStyle HeadersFormatStyle => _headersFormatStyle;

		[Description("The SchedulerFormatStyle to apply to the all-day area on day and work-week view.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		public SchedulerFormatStyle AllDayAreaBackgroundStyle => _allDayAreaBackgroundStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object used to draw the SchedulerAppointment objects in the Schedule control.")]
		[Category("Appointments")]
		public SchedulerFormatStyle AppointmentsFormatStyle => _appointmentsFormatStyle;

		[Description("The SchedulerFormatStyle object used to draw the background of the rows, in the working hour range,  when the Schedule is in day or work week views.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Category("Appearance")]
		public SchedulerFormatStyle WorkingHourBackgroundStyle => _workingHourBackgroundStyle;

		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object used to draw the background of alternating rows, in the working hour range, when the Schedule is in day or work week views.")]
		public SchedulerFormatStyle AlternatingWorkingHourBackgroundStyle => _alternatingWorkingHourBackgroundStyle;

		[Description("Indicates whether a header for the weeks on month view should be displayed.")]
		[DefaultValue(TriState.Empty)]
		public TriState ShowWeekHeaders
		{
			get
			{
				return _showWeekHeaders;
			}
			set
			{
				if (_showWeekHeaders != value)
				{
					_showWeekHeaders = value;
					if (_view == SchedulerView.MonthView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[DefaultValue("Next Appointment")]
		[Description("Represents the text on the find next appointmnet button")]
		public string NextAppointmentText
		{
			get
			{
				return _nextAppointmentText;
			}
			set
			{
				if (_nextAppointmentText != value)
				{
					_nextAppointmentText = value;
					InvalidateWithChildren();
				}
			}
		}

		[Description("Represents the text on the find previous appointmnet button")]
		[DefaultValue("Previous Appointment")]
		public string PreviousAppointmentText
		{
			get
			{
				return _previousAppointmentText;
			}
			set
			{
				if (_previousAppointmentText != value)
				{
					_previousAppointmentText = value;
					InvalidateWithChildren();
				}
			}
		}

		[DefaultValue("Click to add appointment")]
		[Description("Represents the text on the new appointment button")]
		public string NewAppointmentText
		{
			get
			{
				return _newAppointmentText;
			}
			set
			{
				if (_newAppointmentText != value)
				{
					_newAppointmentText = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the text on the all-day appointments used to specify the start time of the Appointment")]
		[DefaultValue("From")]
		public string FromText
		{
			get
			{
				return _fromText;
			}
			set
			{
				if (_fromText != value)
				{
					_fromText = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the text on the all-day appointments used to specify the end time of the Appointment")]
		[DefaultValue("To")]
		public string ToText
		{
			get
			{
				return _toText;
			}
			set
			{
				if (_toText != value)
				{
					_toText = value;
					Invalidate();
				}
			}
		}

		[Description("Indicates the custom date-time format string for the header in the Schedule")]
		[Category("Dates/Times")]
		[DefaultValue("")]
		public string CustomDateFormat
		{
			get
			{
				return _customDateFormat;
			}
			set
			{
				if (_customDateFormat != value)
				{
					_customDateFormat = value;
					InvalidateAppointmentLayout();
				}
			}
		}

		[Description("Determines the default AlphaMode used in format styles with AlphaMode empty.")]
		[DefaultValue(AlphaMode.Opaque)]
		[Category("Appearance")]
		public AlphaMode DefaultAlphaMode
		{
			get
			{
				return _backColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == AlphaMode.Empty)
				{
					throw new InvalidOperationException("DefaultAlphaMode property can not be empty");
				}
				if (_backColorAlphaMode != value)
				{
					_backColorAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the default BackColorAlphaMode used in format styles with BackColorAlphaMode empty.")]
		[DefaultValue(AlphaMode.Empty)]
		[Category("Appearance")]
		public AlphaMode DefaultBackColorAlphaMode
		{
			get
			{
				return _schedulerBackColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _schedulerBackColorAlphaMode)
				{
					_schedulerBackColorAlphaMode = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(AlphaMode.Empty)]
		[Category("Appearance")]
		[Description("Represents the default ForeColorAlphaMode used in format styles with ForeColorAlphaMode empty.")]
		public AlphaMode DefaultForeColorAlphaMode
		{
			get
			{
				return _foreColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _foreColorAlphaMode)
				{
					_foreColorAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Description("Determines the default alpha level used in format styles with Alpha property empty.")]
		[DefaultValue(255)]
		[Category("Appearance")]
		public int DefaultAlpha
		{
			get
			{
				return _defaultAlpha;
			}
			set
			{
				if (value < 0 || value > 255)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (_defaultAlpha != value)
				{
					_defaultAlpha = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("Indicates how to draw a watermark image in the background.")]
		public SchedulerWatermarkImage WatermarkImage
		{
			get
			{
				if (_watermarkImage == null)
				{
					_watermarkImage = new SchedulerWatermarkImage(this);
				}
				return _watermarkImage;
			}
		}

		[DefaultValue(ImageDrawMode.Stretched)]
		[Category("Appearance")]
		[Description("Specifies how the background image of the Schedule is drawn")]
		public ImageDrawMode BackgroundImageDrawMode
		{
			get
			{
				return _backgroundImageDrawMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ImageDrawMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backgroundImageDrawMode)
				{
					_backgroundImageDrawMode = value;
					Invalidate(_gridBounds);
				}
			}
		}

		public new Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[Description("The font of the text displayed by the Schedule and the appointments on month view")]
		[Localizable(true)]
		[Category("Fonts")]
		public Font MonthFont
		{
			get
			{
				if (_monthFont == null)
				{
					Control control = SchedulerRenderer.GetParentControl(this);
					if (control == null)
					{
						return Control.DefaultFont;
					}
					return control.Font;
				}
				return _monthFont;
			}
			set
			{
				if (_monthFont != value)
				{
					_monthFont = value;
					UpdateRowHeight();
					if (_view == SchedulerView.MonthView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[Localizable(true)]
		[Description("The font of the text displayed by the Schedule and the appointments on week view.")]
		[Category("Fonts")]
		public Font WeekFont
		{
			get
			{
				if (_weekFont == null)
				{
					Control control = SchedulerRenderer.GetParentControl(this);
					if (control == null)
					{
						return Control.DefaultFont;
					}
					return control.Font;
				}
				return _weekFont;
			}
			set
			{
				if (_weekFont != value)
				{
					_weekFont = value;
					UpdateRowHeight();
					if (_view == SchedulerView.WeekView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[Description("Specifies whether the text rendering should be compatible with previous release of Windows Forms.")]
		public bool UseCompatibleTextRendering
		{
			get
			{
				if (!_useCompatibleTextRendering.HasValue)
				{
					return TextRenderingOptions.DefaultUseCompatibleTextRendering;
				}
				return _useCompatibleTextRendering.Value;
			}
			set
			{
				if (_useCompatibleTextRendering != value)
				{
					_useCompatibleTextRendering = value;
					InvalidateAppointmentLayout();
				}
			}
		}

		[Category("Fonts")]
		[Localizable(true)]
		[Description("The font of the text displayed by the Schedule and the appointments on day view")]
		public Font DayFont
		{
			get
			{
				if (_dayFont == null)
				{
					Control control = SchedulerRenderer.GetParentControl(this);
					if (control == null)
					{
						return Control.DefaultFont;
					}
					return control.Font;
				}
				return _dayFont;
			}
			set
			{
				if (_dayFont != value)
				{
					_dayFont = value;
					UpdateRowHeight();
					if (_view == SchedulerView.DayView)
					{
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[Description("Specifies whether the all-day area is displayed")]
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool ShowAllDayArea
		{
			get
			{
				return _showAllDayArea;
			}
			set
			{
				if (_showAllDayArea != value)
				{
					_showAllDayArea = value;
					_dayAreaTop = 0;
					if (_view == SchedulerView.DayView)
					{
						_isAppointmentCacheStale = true;
						InvalidateAppointmentLayout();
					}
				}
			}
		}

		[DefaultValue(-1)]
		[Description("Specifies the height of the rows in the control ")]
		public int RowHeight
		{
			get
			{
				return _rowHeight;
			}
			set
			{
				if (_rowHeight != value)
				{
					_rowHeight = value;
					UpdateRowHeight();
					InvalidateAppointmentLayout();
				}
			}
		}

		[Category("Appearance")]
		[Description("The color of the grid lines")]
		public Color GridLineColor
		{
			get
			{
				if (_gridLineColor.IsEmpty)
				{
					return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.GridLine);
				}
				return _gridLineColor;
			}
			set
			{
				if (_gridLineColor != value)
				{
					_gridLineColor = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[Description("Represents the Color used to darw the lines of the rows between the hours.")]
		public Color GridLineColorLight
		{
			get
			{
				if (_gridLineColorLight.IsEmpty)
				{
					return GridLineColor;
				}
				return _gridLineColorLight;
			}
			set
			{
				if (_gridLineColorLight != value)
				{
					_gridLineColorLight = value;
					Invalidate();
				}
			}
		}

		[Category("Appointments")]
		[Description("Indicating whether the appointment's start and end time will be shown when the appointment is not fully displayed by the control.")]
		[DefaultValue(TimeHintOnAppointments.Automatic)]
		public TimeHintOnAppointments ShowTimeHintOnAppointments
		{
			get
			{
				return _showTimeHintOnAppointments;
			}
			set
			{
				if (_showTimeHintOnAppointments != value)
				{
					_showTimeHintOnAppointments = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the time for the first visible row in the Schedule control.")]
		[Category("Dates/Times")]
		public TimeSpan FirstVisibleTime
		{
			get
			{
				if (_view == SchedulerView.DayView)
				{
					if (_firstVisibleSlot != null)
					{
						return _firstVisibleSlot.GetDate().TimeOfDay;
					}
					return _firstVisibleTime;
				}
				return TimeSpan.Zero;
			}
			set
			{
				if (_firstVisibleTime != value)
				{
					value = ClampToVisibleHours(value);
					_firstVisibleTime = value;
					_firstVisibleSlot = null;
					_firstVisibleSlotIndex = -1;
					InvalidateAppointmentLayout();
				}
				else
				{
					_firstVisibleTime = value;
					_firstVisibleSlot = null;
					_firstVisibleSlotIndex = -1;
					InvalidateAppointmentLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public TimeSpan LastVisibleTime
		{
			get
			{
				if (_view == SchedulerView.DayView)
				{
					TimeSpan timeAt = GetTimeAt(new Point(_dayAreaLeft, base.ClientRectangle.Bottom - _actualRowHeight + 2));
					return new TimeSpan(timeAt.Hours, timeAt.Minutes + MinuteInterval, 0);
				}
				return new TimeSpan(24, 0, 0);
			}
		}

		[Browsable(false)]
		[Localizable(true)]
		[Description("The cursor that is displayed when the mouse pointer is over the command.")]
		public override Cursor Cursor
		{
			get
			{
				if (_cursor != null)
				{
					return _cursor;
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		[Browsable(false)]
		[DefaultValue("")]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[Localizable(true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
			}
		}

		[Description("Specifies the property of the data source to map to the Text property of the SchedulerAppointment objects.")]
		[DefaultValue("")]
		[Category("Data")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		public string TextMember
		{
			get
			{
				return _textBindingMember.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_textBindingMember))
				{
					return;
				}
				if (!IsUnboundMode())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_textBindingMember = bindingMemberInfo;
					_textField = _textBindingMember.BindingField;
					ReloadAppointmentsFromDataSource();
				}
				else
				{
					_textBindingMember = bindingMemberInfo;
					_textField = _textBindingMember.BindingField;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public SchedulerFormatConditionCollection FormatConditions => _formatConditions;

		[Category("Appointments")]
		[Description("Specifies the color of the line borders for the SchedulerAppointment objects")]
		public Color AppointmentOutLineColor
		{
			get
			{
				return _appointmentOutLineColor;
			}
			set
			{
				if (_appointmentOutLineColor != value || !_isAppointmentOutLineColorSet)
				{
					_appointmentOutLineColor = value;
					_isAppointmentOutLineColorSet = true;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[DefaultValue(TimeFormat.TwelveHours)]
		[Localizable(true)]
		[Description("Indicates the hour time format for the Schedule")]
		[Category("Dates/Times")]
		public TimeFormat TimeFormat
		{
			get
			{
				return _timeFormat;
			}
			set
			{
				if (_timeFormat != value)
				{
					_timeFormat = value;
					InvalidateWithChildren();
				}
			}
		}

		[Description("Indicates the style applied to the clocks in the SchedulerAppointment objects.")]
		[DefaultValue(ClockStyle.Enhanced)]
		[Category("Appointments")]
		public ClockStyle ClockStyle
		{
			get
			{
				return _clockStyle;
			}
			set
			{
				if (_clockStyle != value)
				{
					_clockStyle = value;
					Invalidate(invalidateChildren: true);
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(ImageSource.ExternalImageList)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("ImageSource property will be removed in future versions of the control. Use ImageList property to assign the ImageList component with the images used in the control")]
		[Description("Indicates whether an external or the internal image list should be used.")]
		[Category("Appearance")]
		public ImageSource ImageSource
		{
			get
			{
				return _imageSource;
			}
			set
			{
				_imageSource = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("The ImageList used when the ImageSource property is equal to InternalImageList.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("InternalImageList property will be removed in future versions of the control. Use ImageList property to assign the ImageList component with the images used in the control")]
		[Browsable(false)]
		[Category("Appearance")]
		public ImageList InternalImageList
		{
			get
			{
				if (_internalImageList == null)
				{
					_internalImageList = new ImageList();
				}
				return _internalImageList;
			}
		}

		[DefaultValue(-2)]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.Scheduler.Design.ImageIndexConverter))]
		[Category("Appointments")]
		[Description("Represents the image list index value of the image displayed in an appointment with reminder.")]
		public int ReminderImageIndex
		{
			get
			{
				return _reminderImageIndex;
			}
			set
			{
				if (_reminderImageIndex != value)
				{
					_reminderImageIndex = value;
					Invalidate();
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed in an exception of a recurrence pattern.")]
		[DefaultValue(-2)]
		[Category("Appointments")]
		[TypeConverter(typeof(Mordex.Windows.Scheduler.Design.ImageIndexConverter))]
		public int RecurrenceExceptionImageIndex
		{
			get
			{
				return _recurrenceExceptionImageIndex;
			}
			set
			{
				if (_recurrenceExceptionImageIndex != value)
				{
					_recurrenceExceptionImageIndex = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the image list index value of the image displayed in an occurrence of a recurrence pattern.")]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-2)]
		[Category("Appointments")]
		[TypeConverter(typeof(Mordex.Windows.Scheduler.Design.ImageIndexConverter))]
		public int RecurrenceImageIndex
		{
			get
			{
				return _recurrenceImageIndex;
			}
			set
			{
				if (_recurrenceImageIndex != value)
				{
					_recurrenceImageIndex = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The collection of images available to the SchedulerAppointment objects")]
		[Category("Appearance")]
		public ImageList ImageList
		{
			get
			{
				return _imageList;
			}
			set
			{
				if (_imageList != value)
				{
					_imageList = value;
					if (value != null)
					{
						_imageSource = ImageSource.ExternalImageList;
					}
					_isAppointmentCacheStale = true;
					InvalidateAppointmentLayout();
					Invalidate();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(OwnerDrawnArea.None)]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		[Description("Specifies which area of a Schedule control needs to be drawn manually.")]
		[TypeConverter(typeof(FlagsEnumConverter))]
		public OwnerDrawnArea OwnerDrawnArea
		{
			get
			{
				return _ownerDrawnArea;
			}
			set
			{
				if (_ownerDrawnArea != value)
				{
					_ownerDrawnArea = value;
					Invalidate();
				}
			}
		}

		[Category("Behavior")]
		[Description("Indicates the way the selected item in the control is highlighted when the control loses focus")]
		[DefaultValue(HideSelection.HideSelection)]
		public HideSelection HideSelection
		{
			get
			{
				return _hideSelection;
			}
			set
			{
				if (_hideSelection != value)
				{
					_hideSelection = value;
					Invalidate();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Indicates whether the Schedule displays a ToolTip for the appointments when its contents are not fully displayed.")]
		public bool ShowToolTips
		{
			get
			{
				return _showToolTips;
			}
			set
			{
				_showToolTips = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public int AllDayAreaHeight
		{
			get
			{
				if (_allDayAreaControl != null)
				{
					return _allDayAreaControl.Height;
				}
				return 0;
			}
		}

		[DefaultValue(true)]
		[Category("Appointments")]
		[Localizable(true)]
		[Description("Specifies whether the SchedulerAppointment\u00b4s StartTime and EndTime properties should be represented as clocks when the Schedule is in week or month views.")]
		public bool ShowTimeAsClocks
		{
			get
			{
				return _showTimeAsClocks;
			}
			set
			{
				if (_showTimeAsClocks != value)
				{
					_showTimeAsClocks = value;
					_isAppointmentLayoutStale = true;
					Invalidate();
				}
			}
		}

		[DefaultValue(true)]
		[Category("Behavior")]
		[Description("Indicates whether the owners' header should be displayed.")]
		public bool ShowOwnersHeader
		{
			get
			{
				return _showOwnersHeader;
			}
			set
			{
				if (_showOwnersHeader != value)
				{
					_showOwnersHeader = value;
					InvalidateAppointmentLayout();
				}
			}
		}

		[Browsable(false)]
		public int TimeNavigatorWidth
		{
			get
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				string text = currentCulture.DateTimeFormat.AMDesignator;
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
				textLayoutRenderer.UpdateWrapsText(false);
				if (text == "")
				{
					text = "00";
				}
				int navigatorWidth = 0;
				if (_isFullLayoutStale)
				{
					CreateTimeNavigatorFont();
				}
				using (Graphics graphics = CreateGraphics())
				{
					int meridiemWidth = (int)textLayoutRenderer.RenderContent(graphics, text, DayFont, 0).Width;
					if (UseCompatibleTextRendering)
					{
						meridiemWidth += 2;
					}
					navigatorWidth = ((_interval != Interval.SixtyMinutes) ? ((int)textLayoutRenderer.RenderContent(graphics, "12", _baseFont, 0).Width + meridiemWidth) : ((int)textLayoutRenderer.RenderContent(graphics, "12:", _baseFont, 0).Width + meridiemWidth));
				}
				return navigatorWidth + 5;
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Indicates whether the owners' list should be displayed in month and week view..")]
		public bool ShowOwnerList
		{
			get
			{
				return _showOwnerList;
			}
			set
			{
				if (_showOwnerList != value)
				{
					_showOwnerList = value;
					Invalidate();
				}
			}
		}
	}
}
