using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerRenderer
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

		internal static void SetCapture(Control control, bool capture)
		{
			try
			{
				control.Capture = capture;
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

		internal static Control GetParentControl(Control control)
		{
			try
			{
				return control.Parent;
			}
			finally
			{
			}
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

		public static DateTime GetWeekStart(DateTime dateTimeArgument, SchedulerDayOfWeek schedulerDayOfWeek)
		{
			DayOfWeek dayOfWeek = GetFirstSelectedDayOfWeek(schedulerDayOfWeek, (int)dateTimeArgument.DayOfWeek);
			DateTime dateTime = dateTimeArgument;
			for (int i = 0; i < 8; i++)
			{
				dateTime = dateTimeArgument.AddDays(-i);
				if (dateTime.DayOfWeek == dayOfWeek)
				{
					return dateTime;
				}
			}
			return dateTimeArgument;
		}

		internal static DayOfWeek ConvertToDayOfWeek(SchedulerDayOfWeek schedulerDayOfWeek)
		{
			switch (schedulerDayOfWeek)
			{
			case SchedulerDayOfWeek.Sunday:
				return DayOfWeek.Sunday;
			case SchedulerDayOfWeek.Monday:
				return DayOfWeek.Monday;
			case SchedulerDayOfWeek.Tuesday:
				return DayOfWeek.Tuesday;
			case SchedulerDayOfWeek.Wednesday:
				return DayOfWeek.Wednesday;
			case SchedulerDayOfWeek.Thursday:
				return DayOfWeek.Thursday;
			case SchedulerDayOfWeek.Friday:
				return DayOfWeek.Friday;
			case SchedulerDayOfWeek.Saturday:
				return DayOfWeek.Saturday;
			default:
				return DayOfWeek.Sunday;
			}
		}

		internal static DayOfWeek GetFirstSelectedDayOfWeek(SchedulerDayOfWeek schedulerDayOfWeekArgument, int startDayIndex)
		{
			for (int i = startDayIndex; i < 7; i++)
			{
				DayOfWeek dayOfWeek = (DayOfWeek)i;
				SchedulerDayOfWeek schedulerDayOfWeek = ToSchedulerDayOfWeek(dayOfWeek);
				if ((schedulerDayOfWeekArgument & schedulerDayOfWeek) != 0)
				{
					return dayOfWeek;
				}
			}
			for (int dayIndex = 0; dayIndex > startDayIndex; dayIndex--)
			{
				DayOfWeek earlierDayOfWeek = (DayOfWeek)dayIndex;
				SchedulerDayOfWeek earlierSchedulerDayOfWeek = ToSchedulerDayOfWeek(earlierDayOfWeek);
				if ((schedulerDayOfWeekArgument & earlierSchedulerDayOfWeek) != 0)
				{
					return earlierDayOfWeek;
				}
			}
			return DayOfWeek.Sunday;
		}





		internal static DateTime GetOccurrenceDate(DateTime dateTimeArgument, SchedulerDayOfWeek schedulerDayOfWeekArgument, OccurrenceInMonth occurrenceInMonth)
		{
			int occurrenceCount = 0;
			DateTime dateTime = new DateTime(dateTimeArgument.Year, dateTimeArgument.Month, 1);
			int daysInMonth = DateTime.DaysInMonth(dateTimeArgument.Year, dateTimeArgument.Month);
			if (occurrenceInMonth == OccurrenceInMonth.Last)
			{
				// The last matching day of the month, also when the mask holds several days (weekday, weekend day).
				for (int i = daysInMonth - 1; i >= 0; i--)
				{
					dateTimeArgument = dateTime.AddDays(i);
					if ((ToSchedulerDayOfWeek(dateTimeArgument.DayOfWeek) & schedulerDayOfWeekArgument) != 0)
					{
						return dateTimeArgument;
					}
				}
				return dateTimeArgument;
			}
			for (int i = 0; i < daysInMonth; i++)
			{
				dateTimeArgument = dateTime.AddDays(i);
				if ((ToSchedulerDayOfWeek(dateTimeArgument.DayOfWeek) & schedulerDayOfWeekArgument) == 0)
				{
					continue;
				}
				occurrenceCount++;
				if (occurrenceCount == (int)occurrenceInMonth)
				{
					return dateTimeArgument;
				}
			}
			return dateTimeArgument;
		}

		internal static OccurrenceInMonth GetOccurrenceInMonth(DateTime dateTimeArgument, SchedulerDayOfWeek schedulerDayOfWeek)
		{
			int occurrenceCount = 0;
			DateTime dateTime = new DateTime(dateTimeArgument.Year, dateTimeArgument.Month, 1);
			int daysInMonth = DateTime.DaysInMonth(dateTimeArgument.Year, dateTimeArgument.Month);
			for (int i = 0; i <= daysInMonth; i++)
			{
				DateTime day = dateTime.AddDays(i);
				if ((ToSchedulerDayOfWeek(day.DayOfWeek) & schedulerDayOfWeek) != 0)
				{
					occurrenceCount++;
					if (day == dateTimeArgument.Date)
					{
						break;
					}
				}
			}
			return (OccurrenceInMonth)occurrenceCount;
		}

		internal static SchedulerDayOfWeek ToSchedulerDayOfWeek(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
			case DayOfWeek.Monday:
				return SchedulerDayOfWeek.Monday;
			case DayOfWeek.Tuesday:
				return SchedulerDayOfWeek.Tuesday;
			case DayOfWeek.Wednesday:
				return SchedulerDayOfWeek.Wednesday;
			case DayOfWeek.Thursday:
				return SchedulerDayOfWeek.Thursday;
			case DayOfWeek.Friday:
				return SchedulerDayOfWeek.Friday;
			case DayOfWeek.Saturday:
				return SchedulerDayOfWeek.Saturday;
			default:
				return SchedulerDayOfWeek.Sunday;
			}
		}

		public static bool IsWithinWorkingHours(DateRange dateRange, SchedulerDayOfWeek schedulerDayOfWeekArgument, SchedulerHourRange schedulerHourRange)
		{
			TimeSpan startTime = schedulerHourRange.StartTime;
			TimeSpan endTime = schedulerHourRange.EndTime;
			SchedulerDayOfWeek schedulerDayOfWeek = ToSchedulerDayOfWeek(dateRange.Start.DayOfWeek);
			if ((schedulerDayOfWeekArgument & schedulerDayOfWeek) != 0)
			{
				double totalDays = dateRange.End.Subtract(dateRange.Start).TotalDays;
				if (totalDays < 1.0 && dateRange.End.Date > dateRange.Start.Date && dateRange.End.TimeOfDay >= dateRange.Start.TimeOfDay)
				{
					return true;
				}
				if (!(totalDays < 1.0))
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

		public static string GetDayNamesText(SchedulerDayOfWeek schedulerDayOfWeek)
		{
			string text = "";
			if ((schedulerDayOfWeek & SchedulerDayOfWeek.Monday) != 0)
			{
				text += "Monday, ";
			}
			if ((schedulerDayOfWeek & SchedulerDayOfWeek.Tuesday) != 0)
			{
				text += "Tuesday, ";
			}
			if ((schedulerDayOfWeek & SchedulerDayOfWeek.Wednesday) != 0)
			{
				text += "Wednesday, ";
			}
			if ((schedulerDayOfWeek & SchedulerDayOfWeek.Thursday) != 0)
			{
				text += "Thursday, ";
			}
			if ((schedulerDayOfWeek & SchedulerDayOfWeek.Friday) != 0)
			{
				text += "Friday, ";
			}
			if ((schedulerDayOfWeek & SchedulerDayOfWeek.Saturday) != 0)
			{
				text += "Saturday, ";
			}
			if ((schedulerDayOfWeek & SchedulerDayOfWeek.Sunday) != 0)
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
				text += GetDayNamesText(workingHourRecurrencePattern.DayOfWeek);
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
				object nthWeekdayPrefix = text;
				text = string.Concat(nthWeekdayPrefix, workingHourRecurrencePattern.OccurrenceInMonth, " ", GetDayNamesText(workingHourRecurrencePattern.DayOfWeek));
				if (workingHourRecurrencePattern.Interval > 1)
				{
					text += " of every month";
					break;
				}
				object nthWeekdayIntervalPrefix = text;
				text = string.Concat(nthWeekdayIntervalPrefix, " of every ", workingHourRecurrencePattern.Interval, " months");
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
				object yearlyNthPrefix = text;
				text = string.Concat(yearlyNthPrefix, workingHourRecurrencePattern.OccurrenceInMonth, " ", GetDayNamesText(workingHourRecurrencePattern.DayOfWeek));
				text = text + " of " + DateTimeFormatInfo.CurrentInfo.GetMonthName(workingHourRecurrencePattern.MonthOfYear);
				break;
			}
			}
			text = text + " effective " + workingHourRecurrencePattern.PatternStartDate.ToShortDateString();
			DateTime dateTime = DateTime.Today.Add(workingHourRecurrencePattern.StartTime);
			DateTime endDateTime = DateTime.Today.Add(workingHourRecurrencePattern.EndTime);
			string summary = text;
			return summary + " from " + dateTime.ToShortTimeString() + " to " + endDateTime.ToShortTimeString();
		}

		public static void DrawFadingLines(Graphics graphics, Point location, int baseAlpha, int integerArgument, Color baseColor, int alphaFalloff)
		{
			Size size = new Size(6, 6);
			if (size.Width <= 0 || size.Height <= 0)
			{
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				int startAlpha = baseAlpha + integerArgument * i;
				int endAlpha = startAlpha - alphaFalloff;
				if (startAlpha < 0)
				{
					startAlpha = 0;
				}
				if (endAlpha < 0)
				{
					endAlpha = 0;
				}
				Color color = Color.FromArgb(startAlpha, baseColor);
				Color endColor = Color.FromArgb(endAlpha, baseColor);
				Rectangle rect = new Rectangle(location, size);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, endColor, 0f);
				Pen pen = new Pen(linearGradientBrush);
				Point pt = location;
				pt.X += 4;
				graphics.DrawLine(pen, location, pt);
				pen.Dispose();
				pen = null;
				linearGradientBrush.Dispose();
				linearGradientBrush = null;
				location.Offset(0, 1);
			}
		}

		public static Rectangle DrawBorder(Graphics graphics, Pen pen, Rectangle rectangle)
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
				return DrawBorder(graphics, scrollBarStyle.GetFlatBorderPen(), rectangle);
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

		public static void DrawImage(Graphics graphics, Image image, Rectangle rectangle, Rectangle rectangleArgument)
		{
			int i = rectangle.X;
			for (int j = rectangle.Y; j < rectangle.Bottom; j += image.Height)
			{
				for (; i < rectangle.Right; i += image.Width)
				{
					Rectangle destRect = new Rectangle(i, j, image.Width, image.Height);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
				}
				i = rectangle.X;
			}
		}

		public static void DrawImageCentered(Graphics graphics, Image image, Rectangle rectangle, Rectangle rectangleArgument)
		{
			int x = rectangle.X + (rectangle.Width - image.Width) / 2;
			int y = rectangle.Y + (rectangle.Height - image.Height) / 2;
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
		}

		public static void DrawImageInRect(Graphics graphics, Image image, Rectangle destRect, Rectangle rectangle)
		{
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
		}

		public static string GetDisplayText(MxScheduler schedulerArgument, object value)
		{
			string text = "";
			if (value is MxScheduler)
			{
				MxScheduler scheduler = (MxScheduler)value;
				text = "Schedule Control";
				if (scheduler.Site != null)
				{
					text = text + " (" + scheduler.Site.Name + ")";
				}
			}
			else if (value is SchedulerFieldCollection)
			{
				text = "Fields";
			}
			else if (value is SchedulerAppointmentOwnerCollection)
			{
				text = "Owners";
			}
			else if (value is SchedulerStoredFilterCollection)
			{
				text = "StoredFilters";
			}
			else if (value is SchedulerField)
			{
				SchedulerField schedulerField = (SchedulerField)value;
				text = "[" + schedulerField.Index + "]";
				if (schedulerField.Key.Length > 0)
				{
					text = text + " - '" + schedulerField.Key + "'";
				}
			}
			else if (value is SchedulerAppointmentOwner)
			{
				SchedulerAppointmentOwner schedulerAppointmentOwner = (SchedulerAppointmentOwner)value;
				text = "[" + schedulerAppointmentOwner.Index + "]";
				if (schedulerAppointmentOwner.Key.Length > 0)
				{
					text = text + " - '" + schedulerAppointmentOwner.Key + "'";
				}
			}
			else if (value is SchedulerFormatConditionCollection)
			{
				text = "FormatConditions";
			}
			else if (value is SchedulerFormatCondition)
			{
				SchedulerFormatCondition schedulerFormatCondition = (SchedulerFormatCondition)value;
				text = "[" + schedulerFormatCondition.Index + "]";
				if (schedulerFormatCondition.Key.Length > 0)
				{
					text = text + " - '" + schedulerFormatCondition.Key + "'";
				}
			}
			else if (value is SchedulerFormatStyle)
			{
				SchedulerFormatStyle schedulerFormatStyle = (SchedulerFormatStyle)value;
				if (schedulerFormatStyle._owner == schedulerArgument)
				{
					text = schedulerFormatStyle._text;
				}
			}
			return text;
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

		public static bool IsEqualToAny(object value, object valueArgument, IComparer iComparer, bool ignoreCase)
		{
			if (valueArgument is IEnumerable enumerable)
			{
				foreach (object item in enumerable)
				{
					if (CompareValues(value, item, iComparer, ignoreCase) == 0)
					{
						return true;
					}
				}
				return false;
			}
			return CompareValues(value, valueArgument, iComparer, ignoreCase) == 0;
		}

		public static int CompareIgnoringCase(object value, object valueArgument, IComparer iComparer)
		{
			return CompareValues(value, valueArgument, iComparer, true);
		}

		public static int CompareValues(object value, object otherValue, IComparer iComparer, bool ignoreCase)
		{
			bool allowConversion = true;
			int result = 0;
			try
			{
				if (iComparer == null)
				{
					if (value == null)
					{
						value = DBNull.Value;
					}
					if (otherValue == null)
					{
						otherValue = DBNull.Value;
					}
					string text = value as string;
					string otherText = otherValue as string;
					if (text != null && otherText != null)
					{
						result = string.Compare(text, otherText, ignoreCase);
					}
					else if (value != DBNull.Value)
					{
						if (otherValue != DBNull.Value)
						{
							if (value is IComparable comparable)
							{
								if (otherValue.GetType() == value.GetType())
								{
									result = comparable.CompareTo(otherValue);
								}
								else if (allowConversion)
								{
									if (value is bool && otherValue is string)
									{
										otherText = ((string)otherValue).ToUpper();
										if (otherText == "0")
										{
											otherValue = false;
										}
										else if (otherText == bool.FalseString.ToUpper())
										{
											otherValue = false;
										}
										else if (otherText == bool.TrueString.ToUpper())
										{
											otherValue = true;
										}
										else
										{
											if (!(otherText == "1") && !(otherText == "-1"))
											{
												return 1;
											}
											otherValue = true;
										}
										return ((IComparable)value).CompareTo(otherValue);
									}
									if (value is IConvertible && otherValue is IConvertible)
									{
										try
										{
											if (CanConvertValue(otherValue, value.GetType()))
											{
												if (value.GetType().IsEnum && otherValue.GetType() == Enum.GetUnderlyingType(value.GetType()))
												{
													value = Convert.ChangeType(value, otherValue.GetType());
													result = ((IComparable)value).CompareTo(otherValue);
												}
												else
												{
													otherValue = Convert.ChangeType(otherValue, value.GetType());
													result = comparable.CompareTo(otherValue);
												}
											}
											else
											{
												result = 1;
											}
										}
										catch
										{
											result = 1;
										}
									}
									else
									{
										result = 1;
									}
								}
								else
								{
									result = 1;
								}
							}
							else
							{
								result = ((!value.Equals(otherValue)) ? 1 : 0);
							}
						}
						else
						{
							result = 1;
						}
					}
					else if (otherValue != DBNull.Value)
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

		private static bool CanConvertValue(object value, Type type)
		{
			if (value == null)
			{
				return false;
			}
			if (value == DBNull.Value)
			{
				return false;
			}
			if (value is string)
			{
				string text = value as string;
				switch (Type.GetTypeCode(type))
				{
				case TypeCode.Boolean:
				{
					bool parsedBoolean = false;
					return bool.TryParse(text, out parsedBoolean);
				}
				case TypeCode.Byte:
				{
					byte parsedByte = 0;
					return byte.TryParse(text, out parsedByte);
				}
				case TypeCode.Char:
				{
					char parsedChar = ' ';
					return char.TryParse(text, out parsedChar);
				}
				case TypeCode.DateTime:
				{
					DateTime parsedDateTime = DateTime.MinValue;
					return DateTime.TryParse(text, out parsedDateTime);
				}
				case TypeCode.Decimal:
				{
					decimal parsedDecimal = 0m;
					return decimal.TryParse(text, out parsedDecimal);
				}
				case TypeCode.Double:
				{
					double parsedDouble = 0.0;
					return double.TryParse(text, out parsedDouble);
				}
				case TypeCode.Int16:
				{
					short parsedShort = 0;
					return short.TryParse(text, out parsedShort);
				}
				case TypeCode.Int32:
				{
					int parsedInt = 0;
					return int.TryParse(text, out parsedInt);
				}
				case TypeCode.Int64:
				{
					long parsedLong = 0L;
					return long.TryParse(text, out parsedLong);
				}
				case TypeCode.SByte:
				{
					sbyte parsedSByte = 0;
					return sbyte.TryParse(text, out parsedSByte);
				}
				case TypeCode.Single:
				{
					float parsedSingle = 0f;
					return float.TryParse(text, out parsedSingle);
				}
				case TypeCode.String:
					return true;
				case TypeCode.UInt16:
				{
					ushort parsedUShort = 0;
					return ushort.TryParse(text, out parsedUShort);
				}
				case TypeCode.UInt32:
				{
					uint parsedUInt = 0u;
					return uint.TryParse(text, out parsedUInt);
				}
				case TypeCode.UInt64:
				{
					ulong result = 0uL;
					return ulong.TryParse(text, out result);
				}
				default:
					return false;
				}
			}
			if (IsPrimitiveOrDecimalType(value.GetType()) && IsPrimitiveOrDecimalType(type))
			{
				return true;
			}
			if (type == typeof(string))
			{
				return true;
			}
			return false;
		}

		private static bool IsPrimitiveOrDecimalType(Type type)
		{
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Empty:
			case TypeCode.Object:
			case TypeCode.DBNull:
			case TypeCode.DateTime:
			case TypeCode.String:
				return false;
			default:
				return true;
			}
		}

		public static bool ContainsIgnoreCase(object value, object valueArgument)
		{
			if (!(value is string) || !(valueArgument is string))
			{
				return false;
			}
			value = ((string)value).ToUpper();
			valueArgument = ((string)valueArgument).ToUpper();
			int index = ((string)value).IndexOf((string)valueArgument);
			if (index > -1)
			{
				return true;
			}
			return false;
		}

		public static bool EndsWithIgnoreCase(object value, object valueArgument)
		{
			if (!(value is string) || !(valueArgument is string))
			{
				return false;
			}
			value = ((string)value).ToUpper();
			valueArgument = ((string)valueArgument).ToUpper();
			return ((string)value).EndsWith((string)valueArgument);
		}

		public static bool StartsWithIgnoreCase(object value, object valueArgument)
		{
			if (!(value is string) || !(valueArgument is string))
			{
				return false;
			}
			value = ((string)value).ToUpper();
			valueArgument = ((string)valueArgument).ToUpper();
			return ((string)value).StartsWith((string)valueArgument);
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
			float redBlend = (float)(int)color.R * floatValue + (float)(int)colorArgument.R * (1f - floatValue);
			int red = (int)Math.Round(redBlend, 0);
			float greenBlend = (float)(int)color.G * floatValue + (float)(int)colorArgument.G * (1f - floatValue);
			int green = (int)Math.Round(greenBlend, 0);
			float blueBlend = (float)(int)color.B * floatValue + (float)(int)colorArgument.B * (1f - floatValue);
			int blue = (int)Math.Round(blueBlend, 0);
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

	}
}
