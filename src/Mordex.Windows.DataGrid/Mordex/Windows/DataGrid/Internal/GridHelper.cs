using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GridHelper
	{
		private static Cursor _cursor;

		private static readonly string[] StandardNumberFormatNames = { "CURRENCY", "PERCENT", "GENERAL", "NUMBER" };

		internal static bool IsStandardNumberFormat(string format)
		{
			string upper = format.ToUpperInvariant();
			foreach (string name in StandardNumberFormatNames)
			{
				if (upper.StartsWith(name, StringComparison.Ordinal))
				{
					return upper.Length == name.Length || int.TryParse(upper.Substring(name.Length), out _);
				}
			}
			return false;
		}

		internal static CultureInfo GetCurrentCulture()
		{
			return Application.CurrentCulture;
		}

		private GridHelper()
		{
		}

		internal static Image FillImageBackground(Image image, Color color)
		{
			if (image == null)
			{
				return null;
			}
			Bitmap bitmap = null;
			try
			{
				Rectangle rectangle = new Rectangle(0, 0, image.Width, image.Height);
				bitmap = new Bitmap(image.Width, image.Height);
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
					// The cached brush is shared with other paint code, so it is borrowed here and never disposed.
					graphics.FillRectangle(GdiCache.Brush(color), rectangle);
					graphics.DrawImage(image, rectangle, rectangle, GraphicsUnit.Pixel);
				}
				Bitmap result = bitmap;
				bitmap = null;
				return result;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return null;
			}
			finally
			{
				bitmap?.Dispose();
			}
		}

		internal static string ToSafeString(object value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			return value.ToString();
		}

		internal static object ConvertUsingTypeConverter(object value, Type otherType)
		{
			object[] customAttributes = otherType.GetCustomAttributes(typeof(TypeConverterAttribute), inherit: false);
			if (value != null && customAttributes.Length > 0)
			{
				_ = (TypeConverterAttribute)customAttributes[0];
				Type type = Type.GetType(((TypeConverterAttribute)customAttributes[0]).ConverterTypeName);
				TypeConverter typeConverter = (TypeConverter)Activator.CreateInstance(type, nonPublic: false);
				if (typeConverter.CanConvertFrom(value.GetType()))
				{
					return typeConverter.ConvertFrom(null, Application.CurrentCulture, value);
				}
			}
			return null;
		}

		internal static object ParseValue(string strA, Type enumType, CultureInfo culture)
		{
			if (enumType.IsEnum)
			{
				try
				{
					return Enum.Parse(enumType, strA, ignoreCase: true);
				}
				catch (Exception ex) when (ex is ArgumentException || ex is OverflowException)
				{
					// Not an enum member name; fall through to the generic conversions.
				}
			}
			if (enumType.IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				Type[] genericArguments = enumType.GetGenericArguments();
				if (genericArguments.Length == 1)
				{
					Type type = genericArguments[0];
					return ParseValue(strA, type, culture);
				}
				throw new InvalidCastException();
			}
			switch (Type.GetTypeCode(enumType))
			{
			case TypeCode.Boolean:
				if (string.Compare(strA, bool.TrueString, ignoreCase: true, culture) == 0)
				{
					return true;
				}
				if (string.Compare(strA, bool.FalseString, ignoreCase: true, culture) == 0)
				{
					return false;
				}
				throw new InvalidCastException();
			case TypeCode.DateTime:
				return DateTime.Parse(strA, culture.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
			case TypeCode.Decimal:
				return decimal.Parse(strA, NumberStyles.Currency, culture.NumberFormat);
			case TypeCode.Double:
				return double.Parse(strA, NumberStyles.Float, culture.NumberFormat);
			case TypeCode.Single:
				return float.Parse(strA, NumberStyles.Float, culture.NumberFormat);
			case TypeCode.Int16:
				return short.Parse(strA, NumberStyles.Integer, culture.NumberFormat);
			case TypeCode.Int32:
				return int.Parse(strA, NumberStyles.Integer, culture.NumberFormat);
			case TypeCode.Int64:
				return long.Parse(strA, NumberStyles.Integer, culture.NumberFormat);
			case TypeCode.UInt16:
				return ushort.Parse(strA, NumberStyles.Integer, culture.NumberFormat);
			case TypeCode.UInt32:
				return uint.Parse(strA, NumberStyles.Integer, culture.NumberFormat);
			case TypeCode.UInt64:
				return ulong.Parse(strA, NumberStyles.Integer, culture.NumberFormat);
			default:
				return Convert.ChangeType(strA, enumType);
			case TypeCode.Object:
			{
				if (enumType == typeof(TimeSpan))
				{
					return TimeSpan.Parse(strA);
				}
				if (enumType == typeof(Guid))
				{
					return new Guid(strA);
				}
				object parseUsingTypeConverter = ParseUsingTypeConverter(strA, enumType);
				if (parseUsingTypeConverter != null)
				{
					return parseUsingTypeConverter;
				}
				return strA;
			}
			}
		}

		private static object ParseUsingTypeConverter(string text, Type otherType)
		{
			object[] customAttributes = otherType.GetCustomAttributes(typeof(TypeConverterAttribute), inherit: false);
			if (customAttributes.Length > 0)
			{
				_ = (TypeConverterAttribute)customAttributes[0];
				Type type = Type.GetType(((TypeConverterAttribute)customAttributes[0]).ConverterTypeName);
				TypeConverter typeConverter = (TypeConverter)Activator.CreateInstance(type, nonPublic: false);
				if (typeConverter.CanConvertFrom(typeof(string)))
				{
					return typeConverter.ConvertFrom(null, Application.CurrentCulture, text);
				}
			}
			return null;
		}

		public static Bitmap MakeBitmapTransparent(Bitmap bitmap, Color transparentColor)
		{
			bitmap.MakeTransparent(transparentColor);
			return bitmap;
		}

		public static Cursor LoadCursorResource(string name)
		{
			Stream manifestResourceStream = typeof(MxDataGrid).Assembly.GetManifestResourceStream(typeof(MxDataGrid), name);
			Cursor result = new Cursor(manifestResourceStream);
			manifestResourceStream.Close();
			manifestResourceStream = null;
			return result;
		}

		public static Cursor ComputeCursor()
		{
			if (_cursor == null)
			{
				_cursor = LoadCursorResource("Arrow.cur");
			}
			return _cursor;
		}

		public static Color BlendColors(Color color, Color otherColor, float floatValue)
		{
			int red = (int)((float)(int)color.R * floatValue + (float)(int)otherColor.R * (1f - floatValue));
			int green = (int)((float)(int)color.G * floatValue + (float)(int)otherColor.G * (1f - floatValue));
			int blue = (int)((float)(int)color.B * floatValue + (float)(int)otherColor.B * (1f - floatValue));
			return Color.FromArgb(red, green, blue);
		}

		public static MouseEventArgs ProcessMouseInput(Control control, Control otherControl, MouseEventArgs eventArgs)
		{
			if (control.IsDisposed)
			{
				return eventArgs;
			}
			Point p = new Point(eventArgs.X, eventArgs.Y);
			p = control.PointToScreen(p);
			p = otherControl.PointToClient(p);
			return new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, p.X, p.Y, eventArgs.Delta);
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

		public static Brush CreateAlphaBrush(Color baseColor, int alpha, AlphaMode alphaMode)
		{
			if (alphaMode == AlphaMode.Empty)
			{
				alphaMode = AlphaMode.Opaque;
			}
			Color color = baseColor;
			switch (alphaMode)
			{
			case AlphaMode.Transparent:
				color = Color.Empty;
				break;
			case AlphaMode.UseAlpha:
				if (alpha == -1)
				{
					alpha = 255;
				}
				color = Color.FromArgb(alpha, baseColor);
				break;
			}
			if (!color.IsEmpty)
			{
				return new SolidBrush(color);
			}
			return null;
		}

		public static Brush CreateGradientBrush(Color baseColor, Color otherBaseColor, Rectangle bounds, BackgroundGradientMode backgroundGradientMode, int alpha, AlphaMode alphaMode)
		{
			if (alphaMode == AlphaMode.Empty)
			{
				alphaMode = AlphaMode.Opaque;
			}
			switch (alphaMode)
			{
			case AlphaMode.Transparent:
				return null;
			case AlphaMode.UseAlpha:
				if (alpha == -1)
				{
					alpha = 255;
				}
				baseColor = Color.FromArgb(alpha, baseColor);
				otherBaseColor = Color.FromArgb(alpha, otherBaseColor);
				break;
			}
			switch (backgroundGradientMode)
			{
			case BackgroundGradientMode.Horizontal:
				return new LinearGradientBrush(bounds, baseColor, otherBaseColor, 0f);
			case BackgroundGradientMode.Vertical:
				return new LinearGradientBrush(bounds, baseColor, otherBaseColor, 90f);
			case BackgroundGradientMode.Diagonal:
				return new LinearGradientBrush(bounds, baseColor, otherBaseColor, 45f);
			case BackgroundGradientMode.DiagonalBackwards:
				return new LinearGradientBrush(bounds, baseColor, otherBaseColor, 225f);
			default:
				return new SolidBrush(baseColor);
			}
		}

		public static void DrawBackgroundImage(Graphics graphics, Image image, BackgroundImageDrawMode backgroundImageDrawMode, Rectangle destRect, Rectangle rectangle, int value, AlphaMode alphaMode)
		{
			if (image != null)
			{
				switch (backgroundImageDrawMode)
				{
				case BackgroundImageDrawMode.Stretch:
					graphics.DrawImage(image, destRect, new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
					break;
				case BackgroundImageDrawMode.Empty:
				case BackgroundImageDrawMode.Tile:
				{
					Brush brush = new TextureBrush(image, WrapMode.Tile);
					graphics.FillRectangle(brush, destRect);
					brush.Dispose();
					break;
				}
				case BackgroundImageDrawMode.Center:
				{
					int x = destRect.X + (destRect.Width - image.Width) / 2;
					int y = destRect.Y + (destRect.Height - image.Height) / 2;
					graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
					break;
				}
				case BackgroundImageDrawMode.None:
					break;
				}
			}
		}

		public static int CalculateImageLeft(Graphics graphics, Size size, ref Rectangle rectangle, ImageHorizontalAlignment imageHorizontalAlignment, string text, TextLayoutRenderer textLayoutRenderer, Font font, bool condition)
		{
			switch (imageHorizontalAlignment)
			{
			case ImageHorizontalAlignment.Near:
				if (condition)
				{
					return rectangle.Right - size.Width;
				}
				return rectangle.Left;
			case ImageHorizontalAlignment.Center:
				return rectangle.Left + (rectangle.Width - size.Width) / 2;
			case ImageHorizontalAlignment.Far:
				if (condition)
				{
					return rectangle.Left;
				}
				return rectangle.Right - size.Width;
			case ImageHorizontalAlignment.AfterText:
				if (!textLayoutRenderer.IsWordWrap())
				{
					if (!IsTextLengthValid(text))
					{
						text = TruncateText(text);
					}
					int width = (int)textLayoutRenderer.MeasureText(graphics, text, font, 0).Width + 2;
					if (condition)
					{
						rectangle.X = rectangle.Right - width;
						rectangle.Width = width;
						return rectangle.Right - size.Width;
					}
					if (textLayoutRenderer.GetStringAlignment() == StringAlignment.Far)
					{
						rectangle.X = rectangle.Right - width - size.Width;
						rectangle.Width = width;
						return rectangle.Right;
					}
					if (width < rectangle.Width)
					{
						rectangle.Width = width;
					}
					return rectangle.Right;
				}
				if (condition)
				{
					rectangle.X += size.Width + 2;
					rectangle.Width -= size.Width + 2;
					return rectangle.X;
				}
				rectangle.Width -= size.Width + 2;
				return rectangle.Right + 2;
			default:
			{
				if (condition)
				{
					rectangle.Width -= size.Width + 2;
					return rectangle.Right + 2;
				}
				int x = rectangle.X;
				rectangle.X += size.Width + 2;
				rectangle.Width -= size.Width + 2;
				return x;
			}
			}
		}

		public static int RenderContent(Graphics graphics, Size size, ref Rectangle rectangle, ImageVerticalAlignment imageVerticalAlignment, string text, TextLayoutRenderer textLayoutRenderer, Font font, bool condition)
		{
			switch (imageVerticalAlignment)
			{
			case ImageVerticalAlignment.BottomOfText:
			{
				if (!IsTextLengthValid(text))
				{
					text = TruncateText(text);
				}
				int height = (int)textLayoutRenderer.MeasureText(graphics, text, font, rectangle.Width).Height;
				rectangle.Height = height;
				return rectangle.Bottom;
			}
			case ImageVerticalAlignment.Near:
				return rectangle.Top;
			case ImageVerticalAlignment.Far:
				return rectangle.Bottom - size.Height;
			case ImageVerticalAlignment.TopOfText:
			{
				int top = rectangle.Top;
				rectangle.Y += size.Height + 2;
				rectangle.Height -= size.Height + 2;
				return top;
			}
			default:
				return rectangle.Top + (rectangle.Height - size.Height) / 2;
			}
		}

		public static void DrawImageAndText(Graphics graphics, Brush brush, string text, Image image, Rectangle rectangle, ImageHorizontalAlignment imageHorizontalAlignment, ImageVerticalAlignment imageVerticalAlignment, TextLayoutRenderer textLayoutRenderer, Font font, bool condition, int value, ImageList imageList, bool otherCondition)
		{
			if (rectangle.IsEmpty)
			{
				return;
			}
			Region region = new Region(rectangle);
			Region clip = graphics.Clip;
			if (clip != null)
			{
				region.Intersect(clip);
			}
			graphics.Clip = region;
			try
			{
				if (otherCondition && image == null && imageList != null && value >= 0 && value < imageList.Images.Count)
				{
					image = imageList.Images[value];
				}
				if (!IsTextLengthValid(text))
				{
					text = TruncateText(text);
				}
				if (image != null || (imageList != null && value >= 0 && value < imageList.Images.Count))
				{
					Point location = rectangle.Location;
					Size size = Size.Empty;
					if (image != null)
					{
						size = image.Size;
					}
					else if (imageList != null)
					{
						size = imageList.ImageSize;
					}
					location.X = CalculateImageLeft(graphics, size, ref rectangle, imageHorizontalAlignment, text, textLayoutRenderer, font, condition);
					location.Y = RenderContent(graphics, size, ref rectangle, imageVerticalAlignment, text, textLayoutRenderer, font, condition);
					if (image != null)
					{
						graphics.DrawImage(image, location.X, location.Y, image.Width, image.Height);
					}
					else if (imageList != null && value >= 0 && value < imageList.Images.Count)
					{
						image = imageList.Images[value];
						graphics.DrawImage(image, location.X, location.Y, image.Width, image.Height);
					}
					if (text != "" && brush != null)
					{
						textLayoutRenderer.DrawTextWithBrush(graphics, text, rectangle, font, brush);
					}
				}
				else if (brush != null && text != "")
				{
					textLayoutRenderer.DrawTextWithBrush(graphics, text, rectangle, font, brush);
				}
			}
			finally
			{
				if (clip != null)
				{
					graphics.Clip = clip;
					clip.Dispose();
				}
				else
				{
					graphics.ResetClip();
				}
				region.Dispose();
			}
		}

		public static Size LayoutImageAndText(Graphics graphics, GridColumn gridColumn, string text, Image image, Rectangle rectangle, ImageHorizontalAlignment imageHorizontalAlignment, ImageVerticalAlignment imageVerticalAlignment, TextLayoutRenderer textLayoutRenderer, Font font, bool condition)
		{
			if (!IsTextLengthValid(text))
			{
				text = TruncateText(text);
			}
			Size result = Size.Empty;
			if (!rectangle.IsEmpty)
			{
				if (image != null)
				{
					Point location = rectangle.Location;
					location.X = CalculateImageLeft(graphics, image.Size, ref rectangle, imageHorizontalAlignment, text, textLayoutRenderer, font, condition);
					location.Y = RenderContent(graphics, image.Size, ref rectangle, imageVerticalAlignment, text, textLayoutRenderer, font, condition);
					if (text != "")
					{
						result = ((gridColumn != null && !gridColumn.WordWrap) ? textLayoutRenderer.MeasureText(graphics, text, font, 0).ToSize() : textLayoutRenderer.MeasureText(graphics, text, font, rectangle.Width).ToSize());
					}
					if (imageHorizontalAlignment == ImageHorizontalAlignment.Empty || imageHorizontalAlignment == ImageHorizontalAlignment.BeforeText || imageHorizontalAlignment == ImageHorizontalAlignment.AfterText)
					{
						result.Width += image.Width + 2;
					}
				}
				else if (text != "")
				{
					result = ((gridColumn != null && !gridColumn.WordWrap) ? textLayoutRenderer.MeasureText(graphics, text, font, 0).ToSize() : textLayoutRenderer.MeasureText(graphics, text, font, rectangle.Width).ToSize());
				}
			}
			result.Width += 3;
			result.Height += 2;
			return result;
		}

		public static Pen CreateHatchPen(Color foreColor, Color backColor)
		{
			Brush brush = new HatchBrush(HatchStyle.Percent50, foreColor, backColor);
			Pen result = new Pen(brush, 1f);
			brush.Dispose();
			brush = null;
			return result;
		}

		internal static int CompareValues(object value, object second, IComparer iComparer, bool ignoreCase)
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
					if (second == DBNull.Value)
					{
						second = null;
					}
					if (value is string && second is string)
					{
						result = string.Compare((string)value, (string)second, ignoreCase, GetCurrentCulture());
					}
					else if (value != null)
					{
						result = ((second == null) ? 1 : ((value is IComparable && second.GetType() == value.GetType()) ? ((IComparable)value).CompareTo(second) : CompareUnrelated(value, second)));
					}
					else if (second != null)
					{
						result = -1;
					}
				}
				else
				{
					result = iComparer.Compare(value, second);
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is InvalidCastException)
			{
				result = CompareUnrelated(value, second);
			}
			return result;
		}

		// Orders values that cannot be compared directly: by type name first, then by text, so the order is stable and antisymmetric.
		private static int CompareUnrelated(object first, object second)
		{
			if (first == null || second == null)
			{
				return first == null ? (second == null ? 0 : -1) : 1;
			}
			int byType = string.CompareOrdinal(first.GetType().FullName, second.GetType().FullName);
			if (byType != 0)
			{
				return byType;
			}
			return string.CompareOrdinal(first.ToString(), second.ToString());
		}

		internal static int CompareValuesWithFallback(object value, object second, IComparer iComparer, bool ignoreCase)
		{
			int result = 0;
			if (value != null && value != DBNull.Value)
			{
				Type.GetTypeCode(value.GetType());
			}
			try
			{
				if (iComparer == null)
				{
					if (value == DBNull.Value)
					{
						value = null;
					}
					if (second == DBNull.Value)
					{
						second = null;
					}
					if (value is string && second is string)
					{
						result = string.Compare((string)value, (string)second, ignoreCase, GetCurrentCulture());
					}
					else if (value != null)
					{
						if (second != null)
						{
							if (!(value is IComparable))
							{
								result = (value.Equals(second) ? 0 : CompareUnrelated(value, second));
							}
							else if (second.GetType() == value.GetType())
							{
								result = ((IComparable)value).CompareTo(second);
							}
							else if (!typeof(IConvertible).IsAssignableFrom(second.GetType()))
							{
								result = ((!(value is string)) ? CompareUnrelated(value, second) : string.Compare((string)value, second.ToString(), ignoreCase, GetCurrentCulture()));
							}
							else
							{
								try
								{
									object changeType = Convert.ChangeType(second, value.GetType());
									result = ((IComparable)value).CompareTo(changeType);
								}
								catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
								{
									result = CompareUnrelated(value, second);
								}
							}
						}
						else
						{
							result = 1;
						}
					}
					else if (second != null)
					{
						result = -1;
					}
				}
				else
				{
					result = iComparer.Compare(value, second);
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException || ex is InvalidCastException)
			{
				result = CompareUnrelated(value, second);
			}
			return result;
		}

		public static StringAlignment ToStringAlignmentDefaultNear(TextAlignment textAlignment)
		{
			switch (textAlignment)
			{
			case TextAlignment.Center:
				return StringAlignment.Center;
			case TextAlignment.Far:
				return StringAlignment.Far;
			default:
				return StringAlignment.Near;
			}
		}

		public static StringAlignment ToStringAlignmentDefaultCenter(TextAlignment textAlignment)
		{
			switch (textAlignment)
			{
			case TextAlignment.Near:
				return StringAlignment.Near;
			case TextAlignment.Far:
				return StringAlignment.Far;
			default:
				return StringAlignment.Center;
			}
		}

		public static TextLayoutRenderer ComputeTextLayoutRenderer(GridFormatStyle gridFormatStyle, bool useTextRenderer)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer);
			if (gridFormatStyle != null)
			{
				textLayoutRenderer.SetStringAlignment(ToStringAlignmentDefaultNear(gridFormatStyle.TextAlignment));
			}
			else
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			}
			textLayoutRenderer.UpdateTrimming(StringTrimming.EllipsisCharacter);
			if (gridFormatStyle != null)
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(ToStringAlignmentDefaultCenter(gridFormatStyle.LineAlignment));
			}
			else
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			}
			return textLayoutRenderer;
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

		public static Rectangle DrawBorderForStyle(Graphics graphics, ControlStyle controlStyle, Rectangle rectangle, BorderStyle borderStyle)
		{
			if (controlStyle == null || borderStyle == BorderStyle.None)
			{
				return rectangle;
			}
			Pen pen = null;
			switch (borderStyle)
			{
			case BorderStyle.Flat:
				return DrawFlatBorder(graphics, controlStyle.GetFlatBorderPen(), rectangle);
			case BorderStyle.RaisedLight3D:
			case BorderStyle.Raised:
				pen = controlStyle.GetControlLightLightPen();
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = controlStyle.GetControlDarkPen();
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			switch (borderStyle)
			{
			case BorderStyle.Raised:
				pen = controlStyle.GetControlDarkDarkPen();
				break;
			case BorderStyle.RaisedLight3D:
				pen = controlStyle.GetControlDarkPen();
				break;
			case BorderStyle.SunkenLight3D:
			case BorderStyle.Sunken:
				pen = controlStyle.GetControlLightLightPen();
				break;
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			if (borderStyle == BorderStyle.Raised || borderStyle == BorderStyle.Sunken)
			{
				rectangle.Inflate(-1, -1);
				pen = ((borderStyle != BorderStyle.Raised) ? controlStyle.GetControlDarkDarkPen() : controlStyle.GetControlLightPen());
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Left, rectangle.Top);
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
				pen = ((borderStyle != BorderStyle.Raised) ? controlStyle.GetControlLightPen() : controlStyle.GetControlDarkPen());
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
				graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			}
			if (borderStyle != BorderStyle.None)
			{
				rectangle.Inflate(-1, -1);
			}
			return rectangle;
		}

		public static int RoundToInt(float floatValue)
		{
			return (int)Math.Round(floatValue, 0);
		}

		public static int CalculateTextHeight(Graphics graphics, Font font, string text, TextLayoutRenderer textLayoutRenderer, int width, int value, int otherValue)
		{
			if (!IsTextLengthValid(text))
			{
				text = TruncateText(text);
			}
			if (font == null)
			{
				return 0;
			}
			int nextValue = 0;
			int previousValue = RoundToInt(font.GetHeight(graphics)) + 1;
			nextValue = ((text.Length <= 0) ? previousValue : RoundToInt(textLayoutRenderer.MeasureText(graphics, text, font, width).Height));
			if (value > 0 && nextValue < value * previousValue)
			{
				nextValue = value * previousValue;
			}
			if (otherValue > 0 && nextValue > (long)otherValue * (long)previousValue)
			{
				nextValue = otherValue * previousValue;
			}
			return nextValue + 4;
		}

		public static int CompareDates(object value, object item)
		{
			if (value == null)
			{
				if (item == null)
				{
					return 0;
				}
				return -1;
			}
			if (item == null)
			{
				return 1;
			}
			if (value is DateTime && item is DateTime)
			{
				return DateTime.Compare((DateTime)value, (DateTime)item);
			}
			return 0;
		}

		public static bool IsStyleAttached(GridFormatStyle gridFormatStyle)
		{
			if (gridFormatStyle != null)
			{
				return gridFormatStyle._owner != null;
			}
			return false;
		}

		public static string GetNodeDisplayText(MxDataGrid otherDataGrid, object value)
		{
			string text = "";
			if (value is GridDropDown)
			{
				GridDropDown gridDropDown = (GridDropDown)value;
				if (gridDropDown.DropDownCollection == null)
				{
					text = "DropDownList";
				}
				else
				{
					text = "[" + gridDropDown.DropDownCollection.IndexOf(gridDropDown) + "]";
					if (gridDropDown.Key.Length > 0)
					{
						text = text + " - '" + gridDropDown.Key + "'";
					}
				}
			}
			else if (value is MxDataGrid)
			{
				MxDataGrid dataGrid = (MxDataGrid)value;
				text = "DataGrid Control";
				if (dataGrid.Site != null)
				{
					text = text + " (" + dataGrid.Site.Name + ")";
				}
			}
			else if (value is GridTable)
			{
				GridTable gridTable = (GridTable)value;
				if (gridTable.DataGrid != null && gridTable.DataGrid.RootTable == gridTable)
				{
					text = "RootTable";
					if (gridTable.Key != null && gridTable.Key.Length > 0)
					{
						text = text + " - " + gridTable.Key;
					}
				}
				else
				{
					text = "'" + gridTable.Key + "'";
				}
			}
			else if (value is GridColumnCollection)
			{
				text = "Columns";
			}
			else if (value is GridStoredFilterCollection)
			{
				text = "StoredFilters";
			}
			else if (value is GridColumn)
			{
				GridColumn gridColumn = (GridColumn)value;
				text = "[" + gridColumn.Index + "]";
				if (gridColumn.Key.Length > 0)
				{
					text = text + " - '" + gridColumn.Key + "'";
				}
			}
			else if (value is GridGroupHeaderTotalCollection)
			{
				text = "GroupHeaderTotals";
			}
			else if (value is GridGroupHeaderTotal)
			{
				GridGroupHeaderTotal gridGroupHeaderTotal = (GridGroupHeaderTotal)value;
				text = "[" + gridGroupHeaderTotal.GetParentCollection().IndexOf(gridGroupHeaderTotal) + "]";
				if (gridGroupHeaderTotal.Key.Length > 0)
				{
					text = text + " - '" + gridGroupHeaderTotal.Key + "'";
				}
			}
			else if (value is GridColumnSetCollection)
			{
				text = "ColumnSets";
			}
			else if (value is GridColumnSet)
			{
				GridColumnSet gridColumnSet = (GridColumnSet)value;
				text = "[" + gridColumnSet.Index + "]";
				if (gridColumnSet.Key.Length > 0)
				{
					text = text + " - '" + gridColumnSet.Key + "'";
				}
			}
			else if (value is GridFormatConditionCollection)
			{
				text = "FormatConditions";
			}
			else if (value is GridFormatCondition)
			{
				GridFormatCondition gridFormatCondition = (GridFormatCondition)value;
				text = "[" + gridFormatCondition.Index + "]";
				if (gridFormatCondition.Key.Length > 0)
				{
					text = text + " - '" + gridFormatCondition.Key + "'";
				}
			}
			else if (value is GridSortKeyCollection)
			{
				text = "SortKeys";
			}
			else if (value is GridSortKey)
			{
				GridSortKey gridSortKey = (GridSortKey)value;
				text = "[" + gridSortKey.Index + "]";
				if (gridSortKey.Column != null)
				{
					if (gridSortKey.Column.Key.Length > 0)
					{
						text = text + " - '" + gridSortKey.Column.Key + "'";
					}
					else
					{
						object item = text;
						text = string.Concat(item, " - Column[", gridSortKey.Column.Index, "]");
					}
				}
			}
			else if (value is GridGroupCollection)
			{
				text = "Groups";
			}
			else if (value is GridGroup)
			{
				GridGroup gridGroup = (GridGroup)value;
				text = "[" + gridGroup.Index + "]";
				if (gridGroup.Column != null)
				{
					if (gridGroup.Column.Key.Length > 0)
					{
						text = text + " - '" + gridGroup.Column.Key + "'";
					}
					else
					{
						object otherItem = text;
						text = string.Concat(otherItem, " - Column[", gridGroup.Column.Index, "]");
					}
				}
			}
			else if (value is GridChildTableCollection)
			{
				text = "ChildTables";
			}
			else if (value is GridFormatStyleCollection)
			{
				text = "FormatStyles";
			}
			else if (value is GridFormatStyle)
			{
				GridFormatStyle gridFormatStyle = (GridFormatStyle)value;
				if (gridFormatStyle.GetOwner() == otherDataGrid)
				{
					text = gridFormatStyle.PredefinedStyle.ToString();
				}
				else if (gridFormatStyle.GetOwner() is GridFormatStyleCollection)
				{
					text = "[" + ((GridFormatStyleCollection)gridFormatStyle.GetOwner()).IndexOf(gridFormatStyle) + "]";
					text = ((gridFormatStyle.Key.Length <= 0) ? (text + " {GridFormatStyle}") : (text + " - '" + gridFormatStyle.Key + "'"));
				}
			}
			else if (value is GridDropDownCollection)
			{
				text = "DropDowns";
			}
			else if (value is GridCustomGroupCollection)
			{
				text = "CustomGroups";
			}
			return text;
		}

		internal static bool IsTextLengthValid(string text)
		{
			if (text != null)
			{
				return text.Length < 65536;
			}
			return false;
		}

		internal static string TruncateText(string text)
		{
			return text?.Substring(0, 65535);
		}

		internal static Control GetParentSafe(Control control)
		{
			if (control == null)
			{
				return null;
			}
			try
			{
				return control.Parent;
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

		internal static void SelectAndFocus(Control control)
		{
			try
			{
				control.Select();
				control.Focus();
			}
			finally
			{
			}
		}

		internal static IMordexContainer ComputeIMordexContainer(Control control)
		{
			control = GetParentSafe(control);
			while (control != null && !(control is IMordexContainer))
			{
				control = GetParentSafe(control);
			}
			return (IMordexContainer)control;
		}

		public static void DrawSplitLine(Control control, Point point, Point location, Color color)
		{
			Rectangle rectangle = Rectangle.Empty;
			if (point.X == location.X)
			{
				rectangle = Rectangle.FromLTRB(point.X, point.Y, point.X + 1, location.Y);
			}
			else if (point.Y == location.Y)
			{
				rectangle = Rectangle.FromLTRB(point.X, point.Y, point.X + 1, location.Y);
			}
			if (!rectangle.IsEmpty)
			{
				InvertRectangle(control, rectangle, 1, color);
			}
		}

		public static void InvertRectangle(Control control, Rectangle rectangle, int value, Color color)
		{
			rectangle.Location = control.PointToClient(rectangle.Location);
			IntPtr intPtr = NativeMethods.GetDC(control.Handle);
			IntPtr whiteBrush = CreateWhiteBrush(color);
			IntPtr selectObject = NativeMethods.SelectObject(intPtr, whiteBrush);
			NativeMethods.PatBlt(intPtr, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, 5898313u);
			NativeMethods.SelectObject(intPtr, selectObject);
			NativeMethods.DeleteObject(whiteBrush);
			NativeMethods.ReleaseDC(control.Handle, intPtr);
		}

		internal static IntPtr CreateWhiteBrush(Color color)
		{
			NativeMethods.NativeLogBrush g = new NativeMethods.NativeLogBrush
			{
				_brushStyle = 0u,
				_nativeMethodsData6UnsignedInteger = (uint)ColorTranslator.ToWin32(Color.White)
			};
			return NativeMethods.CreateBrushIndirect(ref g);
		}

		internal static bool IsWindows7RendererEnabled()
		{
			if (Environment.OSVersion.Version.Build >= 7600 && Environment.OSVersion.Version.Major >= 6)
			{
				return Environment.OSVersion.Version.Minor >= 1;
			}
			return false;
		}
	}
}
