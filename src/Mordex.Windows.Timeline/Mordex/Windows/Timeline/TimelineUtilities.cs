using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Timeline
{
	internal class TimelineUtilities
	{
		internal static Color BlendColorsByPercent(Color color, Color colorArgument, int percent)
		{
			if (percent > 100)
			{
				percent = 100;
			}
			int red = color.R * (100 - percent) / 100 + colorArgument.R * percent / 100;
			int green = color.G * (100 - percent) / 100 + colorArgument.G * percent / 100;
			int blue = color.B * (100 - percent) / 100 + colorArgument.B * percent / 100;
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

		internal static DateTime GetWeekEndDate(DateTime dateTime, System.DayOfWeek dayOfWeekArgument)
		{
			int dayOfWeek = (int)dateTime.DayOfWeek;
			int daysToAdd = 6;
			daysToAdd = (int)((dayOfWeek > (int)dayOfWeekArgument) ? (6 - (dayOfWeek - dayOfWeekArgument)) : ((dayOfWeek != (int)dayOfWeekArgument) ? (dayOfWeekArgument - dayOfWeek - 1) : System.DayOfWeek.Saturday));
			return dateTime.AddDays(daysToAdd);
		}

		internal static Graphics CreateGraphicsFromHdc(IntPtr hdc)
		{
			try
			{
				return Graphics.FromHdc(hdc);
			}
			finally
			{
			}
		}

		internal static void SetAllowDrop(Control control, bool allowDrop)
		{
			try
			{
				control.AllowDrop = allowDrop;
			}
			finally
			{
			}
		}

		internal static void SetControlCapture(Control control, bool hasCapture)
		{
			try
			{
				control.Capture = hasCapture;
			}
			finally
			{
			}
		}

		internal static void FocusControl(Control control)
		{
			try
			{
				control.Focus();
			}
			finally
			{
			}
		}

		public static string FormatDaysOfWeek(DayOfWeek dayOfWeek)
		{
			string text = "";
			if ((dayOfWeek & DayOfWeek.Monday) != 0)
			{
				text += "Monday, ";
			}
			if ((dayOfWeek & DayOfWeek.Tuesday) != 0)
			{
				text += "Tuesday, ";
			}
			if ((dayOfWeek & DayOfWeek.Wednesday) != 0)
			{
				text += "Wednesday, ";
			}
			if ((dayOfWeek & DayOfWeek.Thursday) != 0)
			{
				text += "Thursday, ";
			}
			if ((dayOfWeek & DayOfWeek.Friday) != 0)
			{
				text += "Friday, ";
			}
			if ((dayOfWeek & DayOfWeek.Saturday) != 0)
			{
				text += "Saturday, ";
			}
			if ((dayOfWeek & DayOfWeek.Sunday) != 0)
			{
				text += "Sunday, ";
			}
			return text.Remove(text.Length - 2, 2);
		}

		public static string GetRecurrenceDescription(WorkingHourRecurrencePattern workingHourRecurrencePattern)
		{
			string text = "Occurs every ";
			switch (workingHourRecurrencePattern.RecurrenceType)
			{
			case RecurrenceType.Daily:
				switch (workingHourRecurrencePattern.DailyRecurrenceMode)
				{
				case DailyRecurrenceMode.UseInterval:
					text = ((workingHourRecurrencePattern.Interval != 1) ? (text + workingHourRecurrencePattern.Interval + " days") : (text + "day"));
					break;
				case DailyRecurrenceMode.WeekDays:
					text += "weekday";
					break;
				case DailyRecurrenceMode.WeekendDays:
					text += "weekend day";
					break;
				}
				break;
			case RecurrenceType.Weekly:
				if (workingHourRecurrencePattern.Interval > 1)
				{
					text = text + workingHourRecurrencePattern.Interval + " weeks on ";
				}
				text += FormatDaysOfWeek(workingHourRecurrencePattern.DayOfWeek);
				break;
			case RecurrenceType.Monthly:
				if (workingHourRecurrencePattern.Interval > 1)
				{
					object monthlyPrefix = text;
					text = string.Concat(monthlyPrefix, "day ", workingHourRecurrencePattern.DayOfMonth, " of every month");
				}
				else
				{
					object monthlyIntervalPrefix = text;
					text = string.Concat(monthlyIntervalPrefix, "day ", workingHourRecurrencePattern.DayOfMonth, " of every ", workingHourRecurrencePattern.Interval, " months");
				}
				break;
			case RecurrenceType.MontNth:
			{
				object nthPrefix = text;
				text = string.Concat(nthPrefix, workingHourRecurrencePattern.OccurrenceInMonth, " ", FormatDaysOfWeek(workingHourRecurrencePattern.DayOfWeek));
				if (workingHourRecurrencePattern.Interval > 1)
				{
					text += " of every month";
					break;
				}
				object nthIntervalPrefix = text;
				text = string.Concat(nthIntervalPrefix, " of every ", workingHourRecurrencePattern.Interval, " months");
				break;
			}
			case RecurrenceType.Yearly:
			{
				object yearlyPrefix = text;
				text = string.Concat(yearlyPrefix, DateTimeFormatInfo.CurrentInfo.GetMonthName(workingHourRecurrencePattern.MonthOfYear), " ", workingHourRecurrencePattern.DayOfMonth);
				break;
			}
			case RecurrenceType.YearNth:
			{
				object yearNthPrefix = text;
				text = string.Concat(yearNthPrefix, workingHourRecurrencePattern.OccurrenceInMonth, " ", FormatDaysOfWeek(workingHourRecurrencePattern.DayOfWeek));
				text = text + " of " + DateTimeFormatInfo.CurrentInfo.GetMonthName(workingHourRecurrencePattern.MonthOfYear);
				break;
			}
			}
			text = text + " effective " + workingHourRecurrencePattern.PatternStartDate.ToShortDateString();
			DateTime dateTime = DateTime.Today.Add(workingHourRecurrencePattern.StartTime);
			DateTime endDateTime = DateTime.Today.Add(workingHourRecurrencePattern.EndTime);
			string description = text;
			return description + " from " + dateTime.ToShortTimeString() + " to " + endDateTime.ToShortTimeString();
		}

		internal static DateTime GetNthWeekdayInMonth(DateTime dateTimeArgument, DayOfWeek dayOfWeekArgument, OccurrenceInMonth occurrenceInMonth)
		{
			int matchCount = 0;
			DateTime dateTime = new DateTime(dateTimeArgument.Year, dateTimeArgument.Month, 1);
			int daysInMonth = DateTime.DaysInMonth(dateTimeArgument.Year, dateTimeArgument.Month);
			DateTime lastMatchDate = DateTime.MinValue;
			DayOfWeek dayOfWeek = DayOfWeek.Sunday | DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday | DayOfWeek.Saturday;
			if (occurrenceInMonth == OccurrenceInMonth.Last && dayOfWeekArgument == dayOfWeek)
			{
				return dateTime.AddDays(daysInMonth - 1);
			}
			for (int i = 0; i < daysInMonth; i++)
			{
				dateTimeArgument = dateTime.AddDays(i);
				if ((ToDayOfWeekFlag(dateTimeArgument.DayOfWeek) & dayOfWeekArgument) != 0)
				{
					matchCount++;
					if (occurrenceInMonth == OccurrenceInMonth.Last)
					{
						lastMatchDate = dateTimeArgument;
					}
					else if (matchCount == (int)occurrenceInMonth)
					{
						return dateTimeArgument;
					}
				}
			}
			if (occurrenceInMonth == OccurrenceInMonth.Last && lastMatchDate != DateTime.MinValue)
			{
				return lastMatchDate;
			}
			return dateTimeArgument;
		}

		internal static OccurrenceInMonth GetOccurrenceInMonth(DateTime dateTimeArgument, DayOfWeek dayOfWeek)
		{
			int occurrenceCount = 0;
			DateTime dateTime = new DateTime(dateTimeArgument.Year, dateTimeArgument.Month, 1);
			int daysInMonth = DateTime.DaysInMonth(dateTimeArgument.Year, dateTimeArgument.Month);
			for (int i = 0; i < daysInMonth; i++)
			{
				DateTime currentDay = dateTime.AddDays(i);
				if ((ToDayOfWeekFlag(currentDay.DayOfWeek) & dayOfWeek) != 0)
				{
					occurrenceCount++;
					if (currentDay == dateTimeArgument.Date)
					{
						break;
					}
				}
			}
			return (OccurrenceInMonth)Math.Min(occurrenceCount, (int)OccurrenceInMonth.Last);
		}

		internal static System.DayOfWeek ToSystemDayOfWeek(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
			case DayOfWeek.Sunday:
				return System.DayOfWeek.Sunday;
			case DayOfWeek.Monday:
				return System.DayOfWeek.Monday;
			case DayOfWeek.Tuesday:
				return System.DayOfWeek.Tuesday;
			case DayOfWeek.Wednesday:
				return System.DayOfWeek.Wednesday;
			case DayOfWeek.Thursday:
				return System.DayOfWeek.Thursday;
			case DayOfWeek.Friday:
				return System.DayOfWeek.Friday;
			case DayOfWeek.Saturday:
				return System.DayOfWeek.Saturday;
			default:
				return System.DayOfWeek.Sunday;
			}
		}

		internal static Image CreateExpandBoxImage(Color color, Color fillColor, Color colorArgument, bool isExpanded)
		{
			Bitmap bitmap = new Bitmap(14, 14);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.Clear(Color.Transparent);
			Rectangle rectangle = new Rectangle(1, 1, 11, 11);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rectangle, Color.White, fillColor, LinearGradientMode.ForwardDiagonal);
			Blend blend = new Blend(3);
			blend.Positions = new float[3] { 0f, 0.3f, 1f };
			blend.Factors = new float[3] { 0f, 0.1f, 1f };
			linearGradientBrush.Blend = blend;
			graphics.FillRectangle(linearGradientBrush, rectangle);
			linearGradientBrush.Dispose();
			Pen pen = new Pen(BlendColors(color, Color.White, 0.5f));
			DrawFlatBorder(graphics, pen, rectangle);
			pen.Dispose();
			pen = new Pen(BlendColors(colorArgument, Color.White, 0.75f));
			graphics.DrawLine(pen, rectangle.X, rectangle.Y + 1, rectangle.X, rectangle.Bottom - 2);
			graphics.DrawLine(pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
			pen.Dispose();
			pen = new Pen(BlendColors(colorArgument, Color.Black, 0.75f));
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Y + 1, rectangle.Right - 1, rectangle.Bottom - 2);
			graphics.DrawLine(pen, rectangle.X + 1, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
			rectangle.Inflate(-3, -3);
			rectangle.Width--;
			rectangle.Height--;
			graphics.DrawLine(pen, rectangle.X, rectangle.Y + rectangle.Height / 2, rectangle.Right, rectangle.Y + rectangle.Height / 2);
			if (!isExpanded)
			{
				graphics.DrawLine(pen, rectangle.X + rectangle.Width / 2, rectangle.Y, rectangle.X + rectangle.Width / 2, rectangle.Bottom);
			}
			pen.Dispose();
			graphics.Dispose();
			return bitmap;
		}

		internal static Image CreateChevronImage(Color color, Color fillColor, Color colorArgument, bool isPointingUp)
		{
			Bitmap bitmap = new Bitmap(14, 14);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.Clear(Color.Transparent);
			Rectangle rectangle = new Rectangle(1, 1, 11, 11);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rectangle, Color.White, fillColor, LinearGradientMode.ForwardDiagonal);
			Blend blend = new Blend(3);
			blend.Positions = new float[3] { 0f, 0.3f, 1f };
			blend.Factors = new float[3] { 0f, 0.1f, 1f };
			linearGradientBrush.Blend = blend;
			graphics.FillRectangle(linearGradientBrush, rectangle);
			linearGradientBrush.Dispose();
			Pen pen = new Pen(BlendColors(color, Color.White, 0.5f));
			DrawFlatBorder(graphics, pen, rectangle);
			pen.Dispose();
			pen = new Pen(BlendColors(colorArgument, Color.White, 0.75f));
			graphics.DrawLine(pen, rectangle.X, rectangle.Y + 1, rectangle.X, rectangle.Bottom - 2);
			graphics.DrawLine(pen, rectangle.X + 1, rectangle.Y, rectangle.Right - 2, rectangle.Y);
			pen.Dispose();
			pen = new Pen(BlendColors(colorArgument, Color.Black, 0.75f));
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Y + 1, rectangle.Right - 1, rectangle.Bottom - 2);
			graphics.DrawLine(pen, rectangle.X + 1, rectangle.Bottom - 1, rectangle.Right - 2, rectangle.Bottom - 1);
			rectangle.Inflate(-3, -3);
			rectangle.Width--;
			rectangle.Height--;
			if (isPointingUp)
			{
				graphics.DrawLine(pen, rectangle.X, rectangle.Y + rectangle.Height / 2, rectangle.X + rectangle.Width / 2, rectangle.Y);
				graphics.DrawLine(pen, rectangle.X + rectangle.Width / 2, rectangle.Y, rectangle.Right, rectangle.Y + rectangle.Height / 2);
				graphics.DrawLine(pen, rectangle.X, rectangle.Y + 1 + rectangle.Height / 2, rectangle.X + rectangle.Width / 2, rectangle.Y + 1);
				graphics.DrawLine(pen, rectangle.X + rectangle.Width / 2, rectangle.Y + 1, rectangle.Right, rectangle.Y + rectangle.Height / 2 + 1);
			}
			else
			{
				rectangle.Y++;
				graphics.DrawLine(pen, rectangle.X, rectangle.Y, rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
				graphics.DrawLine(pen, rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2, rectangle.Right, rectangle.Y);
				graphics.DrawLine(pen, rectangle.X, rectangle.Y + 1, rectangle.X + rectangle.Width / 2, rectangle.Y + 1 + rectangle.Height / 2);
				graphics.DrawLine(pen, rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2 + 1, rectangle.Right, rectangle.Y + 1);
			}
			pen.Dispose();
			graphics.Dispose();
			return bitmap;
		}

		internal static int GetImageIndex(ImageList imageList, int index, string key)
		{
			if (imageList != null)
			{
				if (key.Length > 0)
				{
					if (imageList.Images.ContainsKey(key))
					{
						return imageList.Images.IndexOfKey(key);
					}
				}
				else if (index >= 0 && index < imageList.Images.Count)
				{
					return index;
				}
			}
			return -1;
		}

		public static Bitmap MakeMagentaTransparent(Bitmap bitmap)
		{
			try
			{
				bitmap?.MakeTransparent(Color.Magenta);
				return bitmap;
			}
			catch
			{
				return null;
			}
		}

		public static bool IsRangeInWorkingHours(DateRange dateRange, DayOfWeek dayOfWeekArgument, TimelineHourRange timelineHourRange)
		{
			TimeSpan startTime = timelineHourRange.StartTime;
			TimeSpan endTime = timelineHourRange.EndTime;
			DayOfWeek dayOfWeek = ToDayOfWeekFlag(dateRange.Start.DayOfWeek);
			if ((dayOfWeekArgument & dayOfWeek) != 0)
			{
				if ((int)dateRange.End.Subtract(dateRange.Start).TotalDays >= 1)
				{
					return true;
				}
				TimeSpan timeSpan = dateRange.End.TimeOfDay;
				if (timeSpan == TimeSpan.Zero && dateRange.End.Date > dateRange.Start.Date)
				{
					timeSpan = new TimeSpan(1, 0, 0, 0);
				}
				if (dateRange.Start.TimeOfDay >= startTime && timeSpan <= endTime)
				{
					return true;
				}
			}
			return false;
		}

		public static DayOfWeek ToDayOfWeekFlag(System.DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
			case System.DayOfWeek.Monday:
				return DayOfWeek.Monday;
			case System.DayOfWeek.Tuesday:
				return DayOfWeek.Tuesday;
			case System.DayOfWeek.Wednesday:
				return DayOfWeek.Wednesday;
			case System.DayOfWeek.Thursday:
				return DayOfWeek.Thursday;
			case System.DayOfWeek.Friday:
				return DayOfWeek.Friday;
			case System.DayOfWeek.Saturday:
				return DayOfWeek.Saturday;
			default:
				return DayOfWeek.Sunday;
			}
		}

		public static System.DayOfWeek ConvertToSystemDayOfWeek(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
			case DayOfWeek.Monday:
				return System.DayOfWeek.Monday;
			case DayOfWeek.Tuesday:
				return System.DayOfWeek.Tuesday;
			case DayOfWeek.Wednesday:
				return System.DayOfWeek.Wednesday;
			case DayOfWeek.Thursday:
				return System.DayOfWeek.Thursday;
			case DayOfWeek.Friday:
				return System.DayOfWeek.Friday;
			case DayOfWeek.Saturday:
				return System.DayOfWeek.Saturday;
			default:
				return System.DayOfWeek.Sunday;
			}
		}

		public static Rectangle DrawFlatBorder(Graphics graphics, Pen pen, Rectangle rectangle)
		{
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			rectangle.Inflate(-1, -1);
			return rectangle;
		}

		public static Rectangle RenderContent(Graphics graphics, ScrollBarStyle scrollBarStyle, Rectangle rectangle, BorderStyle borderStyle)
		{
			if (scrollBarStyle == null || borderStyle == BorderStyle.None)
			{
				return rectangle;
			}
			Pen pen = null;
			switch (borderStyle)
			{
			case BorderStyle.Flat:
				return DrawFlatBorder(graphics, scrollBarStyle.GetFlatBorderPen(), rectangle);
			case BorderStyle.RaisedLight3D:
			case BorderStyle.Raised:
				pen = scrollBarStyle.GetLightLightPen();
				break;
			case BorderStyle.SunkenLight3D:
				pen = scrollBarStyle.GetDarkPen();
				break;
			case BorderStyle.Sunken:
				pen = scrollBarStyle.GetDarkDarkPen();
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			switch (borderStyle)
			{
			case BorderStyle.Raised:
				pen = scrollBarStyle.GetDarkDarkPen();
				break;
			case BorderStyle.RaisedLight3D:
				pen = scrollBarStyle.GetDarkPen();
				break;
			case BorderStyle.SunkenLight3D:
				pen = scrollBarStyle.GetLightLightPen();
				break;
			case BorderStyle.Sunken:
				pen = scrollBarStyle.GetLightLightPen();
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			if (borderStyle == BorderStyle.Raised || borderStyle == BorderStyle.Sunken)
			{
				rectangle.Inflate(-1, -1);
				pen = ((borderStyle != BorderStyle.Raised) ? scrollBarStyle.GetDarkPen() : scrollBarStyle.GetLightPen());
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
				pen = ((borderStyle != BorderStyle.Raised) ? scrollBarStyle.GetLightPen() : scrollBarStyle.GetDarkPen());
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
				graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			}
			if (borderStyle != BorderStyle.None)
			{
				rectangle.Inflate(-1, -1);
			}
			return rectangle;
		}

		public static bool IsNullOrDBNull(object value)
		{
			if (value == null)
			{
				return true;
			}
			if (Convert.IsDBNull(value))
			{
				return true;
			}
			return false;
		}

		public static Color BlendColors(Color color, Color colorArgument, float floatValue)
		{
			if (color == Color.Empty)
			{
				return colorArgument;
			}
			if (colorArgument == Color.Empty)
			{
				return color;
			}
			float redExact = (float)(int)color.R * floatValue + (float)(int)colorArgument.R * (1f - floatValue);
			int red = (int)Math.Round(redExact, 0);
			float greenExact = (float)(int)color.G * floatValue + (float)(int)colorArgument.G * (1f - floatValue);
			int green = (int)Math.Round(greenExact, 0);
			float blueExact = (float)(int)color.B * floatValue + (float)(int)colorArgument.B * (1f - floatValue);
			int blue = (int)Math.Round(blueExact, 0);
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

		public static int CompareValuesIgnoreCase(object value, object valueArgument, IComparer iComparer)
		{
			return CompareValues(value, valueArgument, iComparer, true);
		}

		public static int CompareValues(object value, object otherValue, IComparer iComparer, bool ignoreCase)
		{
			int result = 0;
			try
			{
				if (iComparer == null)
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
						result = string.Compare((string)value, (string)otherValue, ignoreCase, CultureInfo.CurrentCulture);
					}
					else if (value != null)
					{
						result = ((otherValue == null) ? 1 : ((value is IComparable) ? ((IComparable)value).CompareTo(otherValue) : ((value != otherValue) ? 1 : 0)));
					}
					else if (otherValue != null)
					{
						result = -1;
					}
				}
				else
				{
					result = iComparer.Compare(value, otherValue);
				}
			}
			catch
			{
				result = 1;
			}
			return result;
		}

		public static Pen CreateDottedPen(Color baseColor, Color baseColorArgument)
		{
			Bitmap bitmap = new Bitmap(8, 8);
			Graphics graphics = Graphics.FromImage(bitmap);
			Brush brush = new SolidBrush(Color.FromArgb(255, baseColorArgument));
			graphics.FillRectangle(brush, 0, 0, 8, 8);
			brush.Dispose();
			brush = null;
			Color color = Color.FromArgb(255, baseColor);
			for (int i = 0; i < 8; i++)
			{
				if (i % 2 == 0)
				{
					for (int j = 0; j < 8; j += 2)
					{
						bitmap.SetPixel(i, j, color);
					}
				}
				else
				{
					for (int k = 1; k < 8; k += 2)
					{
						bitmap.SetPixel(i, k, color);
					}
				}
			}
			if (baseColorArgument.Equals(Color.Empty))
			{
				bitmap.MakeTransparent(baseColorArgument);
			}
			brush = new TextureBrush(bitmap);
			Pen result = new Pen(brush, 1f);
			bitmap.Dispose();
			bitmap = null;
			brush.Dispose();
			brush = null;
			return result;
		}

		public static string GetObjectDisplayText(MxTimeline timelineArgument, object value)
		{
			string text = "";
			if (value is MxTimeline)
			{
				MxTimeline timeline = (MxTimeline)value;
				text = "Timeline Control";
				if (timeline.Site != null)
				{
					text = text + " (" + timeline.Site.Name + ")";
				}
			}
			else if (value is TimelineFieldCollection)
			{
				text = "Fields";
			}
			else if (value is TimelineStoredFilterCollection)
			{
				text = "StoredFilters";
			}
			else if (value is TimelineField)
			{
				TimelineField timelineField = (TimelineField)value;
				text = "[" + timelineField.Index + "]";
				if (timelineField.Key.Length > 0)
				{
					text = text + " - '" + timelineField.Key + "'";
				}
			}
			else if (value is TimelineGroupHeaderTotalCollection)
			{
				text = "GroupHeaderTotals";
			}
			else if (value is TimelineGroupHeaderTotal)
			{
				TimelineGroupHeaderTotal timelineGroupHeaderTotal = (TimelineGroupHeaderTotal)value;
				text = "[" + timelineGroupHeaderTotal.GetParentCollection().IndexOf(timelineGroupHeaderTotal) + "]";
				if (timelineGroupHeaderTotal.Key.Length > 0)
				{
					text = text + " - '" + timelineGroupHeaderTotal.Key + "'";
				}
			}
			else if (value is TimelineFormatConditionCollection)
			{
				text = "FormatConditions";
			}
			else if (value is TimelineFormatCondition)
			{
				TimelineFormatCondition timelineFormatCondition = (TimelineFormatCondition)value;
				text = "[" + timelineFormatCondition.Index + "]";
				if (timelineFormatCondition.Key.Length > 0)
				{
					text = text + " - '" + timelineFormatCondition.Key + "'";
				}
			}
			else if (value is TimelineGroupCollection)
			{
				text = "Groups";
			}
			else if (value is TimelineGroup)
			{
				TimelineGroup timelineGroup = (TimelineGroup)value;
				text = "[" + timelineGroup.Index + "]";
				if (timelineGroup.Field != null)
				{
					if (timelineGroup.Field.Key.Length > 0)
					{
						text = text + " - '" + timelineGroup.Field.Key + "'";
					}
					else
					{
						object textBeforeField = text;
						text = string.Concat(textBeforeField, " - Field[", timelineGroup.Field.Index, "]");
					}
				}
			}
			else if (value is TimelineFormatStyle)
			{
				TimelineFormatStyle timelineFormatStyle = (TimelineFormatStyle)value;
				if (timelineFormatStyle._owner == timelineArgument)
				{
					text = timelineFormatStyle._text;
				}
			}
			return text;
		}

		internal static Rectangle RoundToRectangle(RectangleF rectangleF)
		{
			int left = (int)Math.Round(rectangleF.Left, 0);
			int top = (int)Math.Round(rectangleF.Top, 0);
			int width = 0;
			int height = 0;
			if (rectangleF.Width + (float)left >= 2.1474836E+09f)
			{
				width = ((left <= 0) ? int.MaxValue : (int.MaxValue - left));
			}
			else
			{
				width = (int)Math.Round(rectangleF.Width, 0);
				width++;
			}
			height = ((!(rectangleF.Height + (float)top >= 2.1474836E+09f)) ? ((int)Math.Round(rectangleF.Height, 0)) : ((top <= 0) ? int.MaxValue : (int.MaxValue - top)));
			return new Rectangle(left, top, width, height);
		}
	}
}
