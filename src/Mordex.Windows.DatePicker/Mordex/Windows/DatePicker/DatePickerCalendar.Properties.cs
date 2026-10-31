using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DatePicker.Design;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	public partial class DatePickerCalendar
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Represents the name used in code to identify this object")]
		public new string Name
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		[DefaultValue("MMMM yyyy")]
		[Localizable(true)]
		[Description("The date format string for the header.")]
		public string HeaderFormat
		{
			get
			{
				return _headerFormat;
			}
			set
			{
				if (value != null)
				{
					_headerFormat = value;
					MeasureCells();
					UpdateMonthLayouts(true);
					UpdateMonthDates(true);
					Invalidate();
					if (_calendarControl != null)
					{
						_calendarControl.SetMonthFormat(HeaderFormat);
					}
				}
			}
		}

		[Description("The font of the text displayed in the header by the Calendar.")]
		[Localizable(true)]
		public Font HeaderFont
		{
			get
			{
				return _headerFont ?? Font;
			}
			set
			{
				if (_headerFont != value)
				{
					_headerFont = value;
					MeasureCells();
					ComputeDroppedControlSize(_columns, _rows);
					UpdateMonthLayouts(true);
					UpdateMonthDates(true);
					Invalidate();
				}
			}
		}

		[Description("The color that blends with the BackColor and BackColorGradient properties of the CalendarFormatStyle and CalendarBackgroundStyle when its Blend property is not zero.")]
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

		[DefaultValue(0f)]
		[Description("Indicates the default percentage of the BlendColor to use.")]
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

		[DefaultValue(CalendarWeekRule.FirstDay)]
		[Description("Specifies which rule is used to determine the first calendar week of the year.")]
		[Localizable(true)]
		public CalendarWeekRule WeekRule
		{
			get
			{
				return _weekRule;
			}
			set
			{
				if (_weekRule != value)
				{
					_weekRule = value;
					Invalidate();
				}
			}
		}

		[Description("Indicates whether the Calendar control displays week numbers (1-52) to the left of each row of days.")]
		[DefaultValue(false)]
		[Localizable(true)]
		public bool WeekNumbers
		{
			get
			{
				return _weekNumbers;
			}
			set
			{
				if (_weekNumbers != value)
				{
					_weekNumbers = value;
					Invalidate();
				}
			}
		}

		[Description("The Calendar\u00b4s border style.")]
		[DefaultValue(BorderStyle.Flat)]
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
					RebuildLayout();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(CalendarDayOfWeek.Default)]
		[Description("The first day of the week as displayed in the Calendar.")]
		public CalendarDayOfWeek FirstDayOfWeek
		{
			get
			{
				return _firstDayOfWeek;
			}
			set
			{
				if (_firstDayOfWeek != value)
				{
					_firstDayOfWeek = value;
					UpdateMonthLayouts(false);
					UpdateMonthDates(true);
					UpdateDayHeaderTexts();
					Invalidate();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(DayOfWeekAbbreviation.UseFirstLetter)]
		[Description("Indicates the format for the days of the week.")]
		public DayOfWeekAbbreviation DayOfWeekAbbreviation
		{
			get
			{
				return _dayOfWeekAbbreviation;
			}
			set
			{
				if (_dayOfWeekAbbreviation != value)
				{
					_dayOfWeekAbbreviation = value;
					UpdateDayHeaderTexts();
					MeasureCells();
					ComputeDroppedControlSize(_columns, _rows);
					UpdateMonthLayouts(true);
					UpdateMonthDates(true);
					Invalidate();
				}
			}
		}

		[Description("Indicates the first month displayed by the Calendar control.")]
		public DateTime FirstMonth
		{
			get
			{
				return _firstMonth;
			}
			set
			{
				if (!(_firstMonth != value))
				{
					return;
				}
				if (value.Day != 1)
				{
					value = new DateTime(value.Year, value.Month, 1);
				}
				if (GetMonthStart(_minDate) > value)
				{
					FirstMonth = _minDate;
					return;
				}
				DateTime lastVisibleMonth = value;
				if (_monthLayouts.Length > 1)
				{
					lastVisibleMonth = DatePickerHelper.AddMonthsClamped(value, _monthLayouts.Length - 1);
				}
				DateTime lastVisibleDay = new DateTime(lastVisibleMonth.Year, lastVisibleMonth.Month, DateTime.DaysInMonth(lastVisibleMonth.Year, lastVisibleMonth.Month));
				if (GetRangeEndExclusive() <= lastVisibleDay)
				{
					if (_monthLayouts.Length <= 0)
					{
						FirstMonth = GetRangeEndExclusive().AddMonths(-1);
					}
					else
					{
						FirstMonth = GetRangeEndExclusive().AddMonths(-_monthLayouts.Length);
					}
					return;
				}
				_firstMonth = value;
				UpdateMonthLayouts(false);
				UpdateMonthDates(true);
				if (!_isAdjustingFirstMonth && new DateTime(GetSelectedDate().Year, GetSelectedDate().Month, 1) != FirstMonth)
				{
					SetSelectedDate(FirstMonth);
				}
			}
		}

		[DefaultValue(1)]
		[Description("The scroll rate for Calendar control.")]
		public int ScrollChange
		{
			get
			{
				return _scrollChange;
			}
			set
			{
				if (value == 0)
				{
					value = 1;
				}
				_scrollChange = value;
			}
		}

		[Localizable(true)]
		[Description("The array of DateTime objects that determines which nonrecurring dates are displayed in bold.")]
		public DateTime[] BoldedDates
		{
			get
			{
				return _boldedDates;
			}
			set
			{
				_boldedDates = value ?? new DateTime[0];
				UpdateMonthDates(true);
				Invalidate();
			}
		}

		[Localizable(true)]
		[Description("The array of DateTime objects that determines which annual days are displayed in bold.")]
		public DateTime[] AnnuallyBoldedDates
		{
			get
			{
				return _annuallyBoldedDates;
			}
			set
			{
				_annuallyBoldedDates = value ?? new DateTime[0];
				UpdateMonthDates(true);
				Invalidate();
			}
		}

		[Description("The CalendarFormatStyle object used to draw the selected days in the Calendar.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CalendarFormatStyle SelectedFormatStyle => _selectedFormatStyle;

		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The CalendarFormatStyle used to draw the trailing days.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CalendarFormatStyle TrailingFormatStyle => _trailingFormatStyle;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
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

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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
		public new Size Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				base.Size = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Point Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				base.Location = value;
			}
		}

		[DefaultValue("")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new string Text
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

		[Browsable(false)]
		[DefaultValue(DockStyle.None)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = value;
			}
		}

		[Description("The CalendarFormatStyle object used to draw the days of the months.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public CalendarFormatStyle DaysFormatStyle => _daysFormatStyle;

		[Description("The CalendarFormatStyle used to draw the header in the Calendar control.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CalendarFormatStyle HeaderFormatStyle => _headerFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The CalendarFormatStyle object used to draw the section that displays the day of the week.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public CalendarFormatStyle DayHeadersFormatStyle => _dayHeadersFormatStyle;

		[Description("The CalendarBackgroundStyle object used to draw the background of the month in the Calendar.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CalendarBackgroundStyle MonthBackgroundStyle => _calendarBackgroundStyle;

		[Description("Indicates the color of the frame in today\u00b4s date.")]
		public Color TodayRectColor
		{
			get
			{
				if (_todayRectColor.IsEmpty)
				{
					return GetThemeRenderer()._todayColor;
				}
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

		[Description("Indicates whether today's date its framed.")]
		[DefaultValue(true)]
		public bool TodayRect
		{
			get
			{
				return _todayRect;
			}
			set
			{
				if (_todayRect != value)
				{
					_todayRect = value;
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
					MeasureCells();
					UpdateMonthLayouts(true);
					UpdateMonthDates(true);
					Invalidate();
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

		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= NativeMethods.WsExToolWindow;
				return createParams;
			}
		}

		[DefaultValue(true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public new bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				base.Visible = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderFont()
		{
			return _headerFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderFont()
		{
			HeaderFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBlendColor()
		{
			BlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBlendColor()
		{
			return !_blendColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBoldedDates()
		{
			BoldedDates = new DateTime[0];
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBoldedDates()
		{
			return _boldedDates.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAnnuallyBoldedDates()
		{
			return _annuallyBoldedDates.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAnnuallyBoldedDates()
		{
			AnnuallyBoldedDates = new DateTime[0];
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedFormatStyle()
		{
			return _selectedFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedFormatStyle()
		{
			_selectedFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTrailingFormatStyle()
		{
			return _trailingFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTrailingFormatStyle()
		{
			_trailingFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDaysFormatStyle()
		{
			return _daysFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDaysFormatStyle()
		{
			_daysFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderFormatStyle()
		{
			return _headerFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderFormatStyle()
		{
			_headerFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDayHeadersFormatStyle()
		{
			return _dayHeadersFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDayHeadersFormatStyle()
		{
			_dayHeadersFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMonthBackgroundStyle()
		{
			return _calendarBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMonthBackgroundStyle()
		{
			_calendarBackgroundStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTodayRectColor()
		{
			return !_todayRectColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTodayRectColor()
		{
			TodayRectColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.Equals(SystemColors.Window);
		}

		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFirstMonth()
		{
			DateTime value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			return !_firstMonth.Equals(value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFirstMonth()
		{
			DateTime firstMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			FirstMonth = firstMonth;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColor()
		{
			return !ForeColor.Equals(SystemColors.WindowText);
		}

		public new void ResetForeColor()
		{
			base.ForeColor = SystemColors.WindowText;
		}
	}
}
