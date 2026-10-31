using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxCalendarView
	{
		int ISchedulerFormatStyleOwner.DefaultAlpha => DefaultAlpha;

		AlphaMode ISchedulerFormatStyleOwner.BackColorAlphaMode
		{
			get
			{
				if (_calendarBackColorAlphaMode == AlphaMode.Empty)
				{
					return _backColorAlphaMode;
				}
				return _calendarBackColorAlphaMode;
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		[Browsable(false)]
		public Size MonthSize
		{
			get
			{
				if (_monthTemplate != null)
				{
					return _monthTemplate.GetTotalSize();
				}
				return Size.Empty;
			}
		}

		[Description("The font of the text displayed in the header by the Calendar.")]
		[Localizable(true)]
		public Font HeaderFont
		{
			get
			{
				if (_headerFont == null)
				{
					try
					{
						if (base.Parent != null)
						{
							return base.Parent.Font;
						}
						return Control.DefaultFont;
					}
					finally
					{
					}
				}
				return _headerFont;
			}
			set
			{
				if (_headerFont == null || !_headerFont.Equals(value))
				{
					_headerFont = value;
					UpdateMonthGrid();
					MeasureMonthLayout();
					UpdateWorkingHoursSchemaLayoutStateArray(true);
					RefreshMonthLayouts(true);
					Invalidate();
				}
			}
		}

		[Description("Indicates the default percentage of the BlendColor to use.")]
		[DefaultValue(0f)]
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
						throw new ArgumentOutOfRangeException("DefaultValue", "The value must be between 0 and 1");
					}
					_defaultBlend = value;
					Invalidate();
				}
			}
		}

		[Description("The color that blends with the BackColor and BackColorGradient properties of the SchedulerFormatStyle and SchedulerFormatStyle when its Blend property is not zero.")]
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
					Invalidate();
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
					UpdateWeekdayNames();
					MeasureMonthLayout();
					UpdateMonthGrid();
					UpdateWorkingHoursSchemaLayoutStateArray(true);
					MeasureMonthLayout();
					RefreshMonthLayouts(true);
					UpdateMonthGrid();
					UpdateWorkingHoursSchemaLayoutStateArray(true);
					MeasureMonthLayout();
					Invalidate();
					if (_calendarControl != null)
					{
						_calendarControl._useTextRenderer = !UseCompatibleTextRendering;
					}
				}
			}
		}

		[DefaultValue(AlphaMode.Opaque)]
		[Description("Determines the default AlphaMode used in format styles with AlphaMode empty.")]
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

		[DefaultValue(AlphaMode.Empty)]
		[Description("Represents the default BackColorAlphaMode used in format styles with BackColorAlphaMode empty.")]
		public AlphaMode DefaultBackColorAlphaMode
		{
			get
			{
				return _calendarBackColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _calendarBackColorAlphaMode)
				{
					_calendarBackColorAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the default ForeColorAlphaMode used in format styles with ForeColorAlphaMode empty.")]
		[DefaultValue(AlphaMode.Empty)]
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

		[DefaultValue(BorderStyle.SunkenLight3D)]
		[Description("The Calendar\u00b4s border style.")]
		public BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (_borderStyle != value)
				{
					_borderStyle = value;
					Invalidate();
				}
			}
		}

		[Description("Indicates whether the control is automatically resized.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Browsable(true)]
		[DefaultValue(true)]
		public new bool AutoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					if (!_autoSize)
					{
						base.Width++;
						base.Width--;
					}
					UpdateMonthGrid();
					MeasureMonthLayout();
					UpdateWorkingHoursSchemaLayoutStateArray(true);
				}
			}
		}

		[Description("The date format string for the header.")]
		[Localizable(true)]
		[DefaultValue("MMMM yyyy")]
		public string HeaderFormat
		{
			get
			{
				return _headerFormat;
			}
			set
			{
				if (_headerFormat != null)
				{
					_headerFormat = value;
					MeasureMonthLayout();
					UpdateWorkingHoursSchemaLayoutStateArray(true);
					RefreshMonthLayouts(true);
					Invalidate();
					if (_calendarControl != null)
					{
						_calendarControl.SetMonthFormat(HeaderFormat);
					}
				}
			}
		}

		[Description("The cursor that is displayed when the mouse pointer is over the command.")]
		[Browsable(false)]
		[Localizable(true)]
		public override Cursor Cursor
		{
			get
			{
				try
				{
					if (SelectionStyle == CalendarSelectionStyle.Free || _selectionStyle == CalendarSelectionStyle.Schedule)
					{
						Point point = PointToClient(Cursor.Position);
						if (FindMonthAt(point.X, point.Y) != null)
						{
							return _cursor;
						}
					}
				}
				catch
				{
					return base.Cursor;
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object used to draw the selected days in the Calendar.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SchedulerFormatStyle SelectedFormatStyle => _schedulerFormatStyle;

		[Description("The SchedulerFormatStyle used to draw the trailing days.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SchedulerFormatStyle TrailingFormatStyle => _calendarSchedulerFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Indicates how to draw a watermark image in the background.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
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

		[Description("Specifies how the background image of the Calendar is drawn")]
		[DefaultValue(ImageDrawMode.Stretched)]
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
					Invalidate(_contentBounds);
				}
			}
		}

		[DefaultValue(HideSelection.HighlightInactive)]
		[Description("Specifies how the selected items will be displayed when the Calendar control loses focus.")]
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object used to draw the selected days in the Calendar when the control does not have the focus.")]
		public SchedulerFormatStyle SelectedInactiveFormatStyle => _selectedInactiveFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object used to draw the days of the months.")]
		public SchedulerFormatStyle DaysFormatStyle => _daysFormatStyle;

		[Description("The SchedulerFormatStyle object used to draw the days of the odd months.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public SchedulerFormatStyle AlternatingMonthDaysFormatStyle => _alternatingMonthDaysFormatStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The SchedulerFormatStyle used to draw the headers in the Calendar control.")]
		public SchedulerFormatStyle HeadersFormatStyle => _headersFormatStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The SchedulerFormatStyle object used to draw the section that displays the day of the week.")]
		public SchedulerFormatStyle DaysHeadersFormatStyle => _daysHeadersFormatStyle;

		[Description("The SchedulerFormatStyle object used to draw the background of the months in the Calendar.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public SchedulerFormatStyle MonthBackgroundStyle => _monthBackgroundStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The SchedulerFormatStyle object used to draw the background of the odd months.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public SchedulerFormatStyle AlternatingMonthBackgroundStyle => _alternatingMonthBackgroundStyle;

		[Description("Indicates the color of the frame in today\u00b4s date.")]
		public Color TodayRectColor
		{
			get
			{
				return _todayRectColor;
			}
			set
			{
				if (_todayRectColor != value)
				{
					_todayRectColor = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the first days of the previous and next month will be displayed in the Calendar")]
		public bool ShowTrailingDays
		{
			get
			{
				return _showTrailingDays;
			}
			set
			{
				if (_showTrailingDays != value)
				{
					_showTrailingDays = value;
					MeasureMonthLayout();
					UpdateWorkingHoursSchemaLayoutStateArray(true);
					RefreshMonthLayouts(true);
					UpdateMonthGrid();
					Invalidate();
				}
			}
		}

		[Description("The date selection mode on the Calendar control that specifies whether the user can select a single day, a range, or multiples days without been contiguous.")]
		[DefaultValue(CalendarSelectionStyle.Single)]
		public CalendarSelectionStyle SelectionStyle
		{
			get
			{
				return _selectionStyle;
			}
			set
			{
				if (_selectionStyle == value)
				{
					return;
				}
				_selectionStyle = value;
				if (_selectionStyle == CalendarSelectionStyle.Range)
				{
					if (_selectionRange.IsIncomplete())
					{
						SelectionRange = new DateRange(_currentDate, _currentDate);
					}
					else
					{
						SelectionRange = _selectionRange;
					}
				}
				else
				{
					if (_selectionStyle != CalendarSelectionStyle.Schedule || _scheduler == null)
					{
						return;
					}
					if (ToCalendarDayOfWeek(_scheduler.FirstDayOfWeek) != _firstDayOfWeek)
					{
						_scheduler.FirstDayOfWeek = ToSchedulerDayOfWeek(_firstDayOfWeek);
					}
					_minDate = _scheduler.MinDate;
					_calendarControl._minDate = _minDate;
					MaxDate = _scheduler.MaxDate;
					if (_minDate > FirstMonth)
					{
						FirstMonth = _minDate;
					}
					if (_scheduler.Dates.Count <= 0)
					{
						return;
					}
					DateTime[] schedulerDates = new DateTime[_scheduler.Dates.Count];
					int dateIndex = 0;
					_isWeekSelection = false;
					foreach (DateTime date in _scheduler.Dates)
					{
						schedulerDates.SetValue(date, dateIndex);
						dateIndex++;
					}
					if (schedulerDates.Length > 0)
					{
						ScrollToShowDate(schedulerDates[0]);
					}
					SelectedDates = schedulerDates;
				}
			}
		}
	}
}
