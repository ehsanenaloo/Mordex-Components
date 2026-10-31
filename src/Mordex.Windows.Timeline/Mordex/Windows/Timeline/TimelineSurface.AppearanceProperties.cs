using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public partial class TimelineSurface
	{
		[Description("The font of the text displayed in the header by the Calendar.")]
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
				if (_headerFont != value)
				{
					_headerFont = value;
					UpdateCellSizes();
					ComputeDroppedControlSize();
					UpdateTimelineLayoutStateArray(true);
					UpdateLayout(true);
					Invalidate();
				}
			}
		}

		[Description("The color that blends with the BackColor and BackColorGradient properties of the TimelineFormatStyle and CalendarBackgroundStyle when its Blend property is not zero.")]
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
					Size size = _borderSize;
					UpdateBorderSize();
					base.ClientRectangle.Inflate(new Size(_borderSize.Width - size.Width, _borderSize.Height - size.Height));
					ComputeDroppedControlSize();
					UpdateCellSizes();
					UpdateTimelineLayoutStateArray(true);
					UpdateLayout(true);
					Invalidate();
				}
			}
		}

		[Description("The TimelineFormatStyle object used to draw the selected days in the Calendar.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TimelineFormatStyle SelectedFormatStyle => _selectedFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TimelineFormatStyle used to draw the trailing days.")]
		public TimelineFormatStyle TrailingFormatStyle => _trailingFormatStyle;

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

		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TimelineFormatStyle object used to draw the days of the months.")]
		public TimelineFormatStyle DaysFormatStyle => _daysFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The TimelineFormatStyle used to draw the header in the Calendar control.")]
		public TimelineFormatStyle HeaderFormatStyle => _headerFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The TimelineFormatStyle object used to draw the section that displays the day of the week.")]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public TimelineFormatStyle DayHeadersFormatStyle => _dayHeadersFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		[Description("The CalendarBackgroundStyle object used to draw the background of the month in the Calendar.")]
		public TimelineFormatStyle MonthBackgroundStyle => _monthBackgroundStyle;

		[Description("Indicates the color of the frame in today\u00b4s date.")]
		public Color TodayRectColor
		{
			get
			{
				if (_todayRectColor.IsEmpty)
				{
					return Color.DarkRed;
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
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

		[DefaultValue("Today")]
		[Description("Represents the text for the Today button")]
		[Localizable(true)]
		public string TodayText
		{
			get
			{
				return _todayText;
			}
			set
			{
				_todayText = value;
			}
		}
	}
}
