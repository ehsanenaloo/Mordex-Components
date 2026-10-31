using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	internal class DatePickerHelper
	{
		internal const int DaysPerWeek = 7;

		private static readonly ConcurrentDictionary<string, CultureInfo> _gregorianCultures = new ConcurrentDictionary<string, CultureInfo>(StringComparer.Ordinal);

		// The control always shows Gregorian dates, so text is formatted and parsed with a Gregorian copy of the current culture.
		internal static CultureInfo GetDisplayCulture()
		{
			CultureInfo culture = CultureInfo.CurrentCulture;
			if (culture.Calendar is GregorianCalendar)
			{
				return culture;
			}
			return _gregorianCultures.GetOrAdd(culture.Name, (string _) => CreateGregorianCulture(culture));
		}

		// Cached culture data is stamped with the culture it was built for; a different current culture makes the stamp stale.
		internal static bool IsCultureStale(CultureInfo stamp)
		{
			return !ReferenceEquals(stamp, CultureInfo.CurrentCulture);
		}

		internal static DateTimeFormatInfo GetDisplayFormat()
		{
			return GetDisplayCulture().DateTimeFormat;
		}

		private static CultureInfo CreateGregorianCulture(CultureInfo culture)
		{
			Calendar gregorian = culture.OptionalCalendars.FirstOrDefault((Calendar calendar) => calendar is GregorianCalendar);
			if (gregorian == null)
			{
				return CultureInfo.InvariantCulture;
			}
			CultureInfo clone = (CultureInfo)culture.Clone();
			clone.DateTimeFormat.Calendar = gregorian;
			return clone;
		}

		internal static string FormatDate(DateTime date, string format)
		{
			return date.ToString(format, GetDisplayCulture());
		}

		internal static int GetWeekOfYear(DateTime date, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
		{
			return GetDisplayFormat().Calendar.GetWeekOfYear(date, rule, firstDayOfWeek);
		}

		internal static bool TryAddMonths(DateTime date, int months, out DateTime result)
		{
			long monthIndex = date.Year * 12L + (date.Month - 1) + months;
			long year = monthIndex >= 0 ? monthIndex / 12 : -((11 - monthIndex) / 12);
			if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
			{
				result = date;
				return false;
			}
			result = date.AddMonths(months);
			return true;
		}

		// Adds days but keeps the date unchanged when the result would leave the supported range.
		internal static DateTime AddDaysClamped(DateTime date, int days)
		{
			if (days > 0 ? date > DateTime.MaxValue.AddDays(-days) : date < DateTime.MinValue.AddDays(-days))
			{
				return date;
			}
			return date.AddDays(days);
		}

		// Adds months but keeps the date unchanged when the result would leave the supported range.
		internal static DateTime AddMonthsClamped(DateTime date, int months)
		{
			TryAddMonths(date, months, out DateTime result);
			return result;
		}

		internal static Color BlendColors(Color color, Color blendColor, int blendPercent)
		{
			if (blendPercent > 100)
			{
				blendPercent = 100;
			}
			int red = color.R * (100 - blendPercent) / 100 + blendColor.R * blendPercent / 100;
			int green = color.G * (100 - blendPercent) / 100 + blendColor.G * blendPercent / 100;
			int blue = color.B * (100 - blendPercent) / 100 + blendColor.B * blendPercent / 100;
			if (red > 255)
			{
				red = 255;
			}
			if (green > 255)
			{
				green = 255;
			}
			if (blue > 255)
			{
				blue = 255;
			}
			return Color.FromArgb(red, green, blue);
		}

		internal static void SetMouseCapture(Control control, bool capture)
		{
			control.Capture = capture;
		}

		public static Color MixColors(Color color, Color secondColor, float firstColorWeight)
		{
			if (color == Color.Empty)
			{
				return secondColor;
			}
			if (secondColor == Color.Empty)
			{
				return color;
			}
			float redMix = (float)(int)color.R * firstColorWeight + (float)(int)secondColor.R * (1f - firstColorWeight);
			int red = (int)Math.Round(redMix, 0);
			float greenMix = (float)(int)color.G * firstColorWeight + (float)(int)secondColor.G * (1f - firstColorWeight);
			int green = (int)Math.Round(greenMix, 0);
			float blueMix = (float)(int)color.B * firstColorWeight + (float)(int)secondColor.B * (1f - firstColorWeight);
			int blue = (int)Math.Round(blueMix, 0);
			if (red > 255)
			{
				red = 255;
			}
			if (green > 255)
			{
				green = 255;
			}
			if (blue > 255)
			{
				blue = 255;
			}
			return Color.FromArgb(red, green, blue);
		}

		public static Rectangle DrawRectangleOutline(Graphics graphics, Pen pen, Rectangle rectangle)
		{
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			rectangle.Inflate(-1, -1);
			return rectangle;
		}

		// Draws a rectangle outline with the corners cut by one pixel on the rounded sides.
		public static void DrawRoundedOutline(Graphics graphics, Pen pen, Rectangle rectangle, bool roundLeft, bool roundRight)
		{
			int leftCut = roundLeft ? 1 : 0;
			int rightCut = roundRight ? 1 : 0;
			using (GraphicsPath outline = new GraphicsPath())
			{
				outline.AddLine(rectangle.X + leftCut, rectangle.Top, rectangle.Right - 1 - rightCut, rectangle.Top);
				outline.AddLine(rectangle.Right - 1 - rightCut, rectangle.Top, rectangle.Right - 1 - rightCut, rectangle.Top + rightCut);
				outline.AddLine(rectangle.Right - 1, rectangle.Top + rightCut, rectangle.Right - 1, rectangle.Bottom - 1 - rightCut);
				outline.AddLine(rectangle.Right - 1, rectangle.Bottom - 1 - rightCut, rectangle.Right - 1 - rightCut, rectangle.Bottom - 1 - rightCut);
				outline.AddLine(rectangle.Right - 1 - rightCut, rectangle.Bottom - 1, rectangle.X + leftCut, rectangle.Bottom - 1);
				outline.AddLine(rectangle.X + leftCut, rectangle.Bottom - 1, rectangle.X + leftCut, rectangle.Bottom - 1 - leftCut);
				outline.AddLine(rectangle.X, rectangle.Bottom - 1 - leftCut, rectangle.X, rectangle.Y + leftCut);
				outline.AddLine(rectangle.X, rectangle.Top + leftCut, rectangle.X + leftCut, rectangle.Top + leftCut);
				graphics.DrawPath(pen, outline);
			}
		}

		public static Rectangle DrawControlBorder(Graphics graphics, ControlStyle controlStyle, Rectangle rectangle, BorderStyle borderStyle)
		{
			if (borderStyle == BorderStyle.None)
			{
				return rectangle;
			}
			Color controlColor = controlStyle.ControlColor;
			Color shadowColor;
			Color darkShadowColor;
			Color highlightColor;
			Color lightHighlightColor;
			if (controlColor.Equals(SystemColors.Control) || controlColor.Equals(SystemColors.Window))
			{
				shadowColor = SystemColors.ControlDark;
				darkShadowColor = SystemColors.ControlDarkDark;
				highlightColor = SystemColors.ControlLight;
				lightHighlightColor = SystemColors.ControlLightLight;
			}
			else
			{
				shadowColor = ControlPaint.Dark(controlColor, 0f);
				darkShadowColor = ControlPaint.Dark(controlColor);
				highlightColor = ControlPaint.Light(controlColor);
				lightHighlightColor = ControlPaint.LightLight(controlColor);
			}
			Pen pen = null;
			switch (borderStyle)
			{
			case BorderStyle.Flat:
				if (controlStyle._owner == null)
				{
					pen = GdiCache.Pen(!controlStyle.GetFlatBorderColor().IsEmpty ? controlStyle.GetFlatBorderColor() : shadowColor);
					rectangle = DrawRectangleOutline(graphics, pen, rectangle);
				}
				else
				{
					rectangle = DrawRectangleOutline(graphics, controlStyle._owner.FlatBorderPen, rectangle);
				}
				return rectangle;
			case BorderStyle.RaisedLight3D:
			case BorderStyle.Raised:
				pen = GdiCache.Pen(lightHighlightColor);
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = GdiCache.Pen(shadowColor);
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			switch (borderStyle)
			{
			case BorderStyle.Raised:
				pen = GdiCache.Pen(darkShadowColor);
				break;
			case BorderStyle.RaisedLight3D:
				pen = GdiCache.Pen(shadowColor);
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = GdiCache.Pen(lightHighlightColor);
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			if (borderStyle == BorderStyle.Raised || borderStyle == BorderStyle.Sunken)
			{
				rectangle.Inflate(-1, -1);
				pen = GdiCache.Pen(borderStyle != BorderStyle.Raised ? darkShadowColor : highlightColor);
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
				pen = GdiCache.Pen(borderStyle != BorderStyle.Raised ? highlightColor : shadowColor);
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
				graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			}
			if (borderStyle != BorderStyle.None)
			{
				rectangle.Inflate(-1, -1);
			}
			return rectangle;
		}

		public static void DrawTriangleArrow(Graphics graphics, int arrowWidth, Rectangle rectangle, int rowStep, Pen pen)
		{
			int rowWidth = arrowWidth / 2;
			if (rowWidth % 2 == 0)
			{
				rowWidth--;
			}
			int arrowHeight = rowWidth / 2 + 1;
			int rowY = ((rowStep <= 0) ? (rectangle.Top + (rectangle.Height - arrowHeight) / 2 + arrowHeight - 1) : (rectangle.Top + (rectangle.Height - arrowHeight) / 2));
			int rowX = rectangle.Left + (rectangle.Width - rowWidth) / 2;
			while (rowWidth >= 0)
			{
				if (rowWidth == 1)
				{
					graphics.DrawLine(pen, rowX, rowY - rowStep, rowX, rowY);
				}
				else
				{
					graphics.DrawLine(pen, rowX, rowY, rowX + rowWidth - 1, rowY);
				}
				rowX++;
				rowWidth -= 2;
				rowY += rowStep;
			}
		}

		public static DatePortion ToDatePortion(PortionType portionType)
		{
			switch (portionType)
			{
			case PortionType.Hour12:
			case PortionType.Hour24:
				return DatePortion.Hour;
			case PortionType.AMPM:
				return DatePortion.AMPM;
			case PortionType.Day:
			case PortionType.DayOfWeek:
				return DatePortion.Day;
			case PortionType.Minute:
				return DatePortion.Minute;
			case PortionType.Month:
			case PortionType.MonthAbbreviated:
			case PortionType.MonthName:
				return DatePortion.Month;
			case PortionType.Second:
				return DatePortion.Second;
			case PortionType.Year:
				return DatePortion.Year;
			default:
				return DatePortion.None;
			}
		}

		public static PortionType ToPortionType(DatePortion datePortion)
		{
			switch (datePortion)
			{
			case DatePortion.AMPM:
				return PortionType.AMPM;
			case DatePortion.Day:
				return PortionType.Day;
			case DatePortion.Hour:
				return PortionType.Hour12;
			case DatePortion.Minute:
				return PortionType.Minute;
			case DatePortion.Second:
				return PortionType.Second;
			case DatePortion.Year:
				return PortionType.Year;
			case DatePortion.Month:
				return PortionType.Month;
			default:
				return PortionType.Day;
			}
		}

		public static int CompareValues(object value, object otherValue, IComparer comparer, CultureInfo culture)
		{
			int result = 0;
			try
			{
				if (comparer == null)
				{
					if (value == DBNull.Value)
					{
						value = null;
					}
					if (otherValue == DBNull.Value)
					{
						otherValue = null;
					}
					if (value is string && otherValue is string)
					{
						result = culture.CompareInfo.Compare((string)value, (string)otherValue);
					}
					else if (value != null)
					{
						if (otherValue != null)
						{
							if (value is IComparable)
							{
								result = ((IComparable)value).CompareTo(otherValue);
							}
						}
						else
						{
							result = 1;
						}
					}
					else if (value == null)
					{
						if (otherValue != null)
						{
							result = -1;
						}
					}
					else if (otherValue != null)
					{
						result = 1;
					}
				}
				else
				{
					result = comparer.Compare(value, otherValue);
				}
			}
			catch (ArgumentException)
			{
				// Values of different types cannot be ordered, so they count as different.
				result = 1;
			}
			return result;
		}
	}
}
