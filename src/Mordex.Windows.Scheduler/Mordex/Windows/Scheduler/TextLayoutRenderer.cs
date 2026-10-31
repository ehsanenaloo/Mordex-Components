using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal class TextLayoutRenderer
	{
		internal bool _useTextRenderer;

		private bool _isFormatStale;

		private StringTrimming _stringTrimming;

		public RightToLeft _rightToLeft;

		private StringAlignment _stringAlignment;

		private StringAlignment _lineAlignment;

		private bool _wrapsText;

		private bool _isPaddingDisabled;

		private bool _showHotkeyPrefix;

		private StringFormat _stringFormat;

		private TextFormatFlags _textFormatFlags;

		internal TextLayoutRenderer(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
			_isFormatStale = true;
			_rightToLeft = RightToLeft.No;
			_wrapsText = false;
			_stringAlignment = StringAlignment.Center;
			_lineAlignment = StringAlignment.Center;
			_stringTrimming = StringTrimming.None;
			_isPaddingDisabled = false;
			_showHotkeyPrefix = false;
		}

		public StringTrimming GetStringTrimming()
		{
			return _stringTrimming;
		}

		public void UpdateStringTrimming(StringTrimming stringTrimming)
		{
			if (_stringTrimming != stringTrimming)
			{
				_stringTrimming = stringTrimming;
				ResetFormat();
			}
		}

		public void UpdateRightToLeft(RightToLeft rightToLeft)
		{
			if (_rightToLeft != rightToLeft)
			{
				_rightToLeft = rightToLeft;
				ResetFormat();
			}
		}

		public StringAlignment GetStringAlignment()
		{
			return _stringAlignment;
		}

		public void UpdateStringAlignment(StringAlignment stringAlignment)
		{
			if (_stringAlignment == stringAlignment)
			{
				return;
			}
			_stringAlignment = stringAlignment;
			if (_useTextRenderer)
			{
				if (!_isFormatStale)
				{
					ResetFormat();
				}
			}
			else if (_stringFormat != null)
			{
				_stringFormat.Alignment = _stringAlignment;
			}
		}

		public StringAlignment GetTextLayoutRendererStringAlignment()
		{
			return _lineAlignment;
		}

		public void UpdateTextLayoutRendererStringAlignment(StringAlignment textLayoutRendererStringAlignment)
		{
			if (_lineAlignment != textLayoutRendererStringAlignment)
			{
				_lineAlignment = textLayoutRendererStringAlignment;
				ResetFormat();
			}
		}

		public bool GetWrapsText()
		{
			return _wrapsText;
		}

		public void UpdateWrapsText(bool textLayoutRendererBoolean)
		{
			if (_wrapsText != textLayoutRendererBoolean)
			{
				_wrapsText = textLayoutRendererBoolean;
				ResetFormat();
			}
		}

		public bool GetIsPaddingDisabled()
		{
			return _isPaddingDisabled;
		}

		public void UpdateIsPaddingDisabled(bool isPaddingDisabled)
		{
			if (_isPaddingDisabled != isPaddingDisabled)
			{
				_isPaddingDisabled = isPaddingDisabled;
				ResetFormat();
			}
		}

		internal StringFormat GetStringFormat()
		{
			if (_stringFormat == null)
			{
				UpdateStringFormat();
			}
			return _stringFormat;
		}

		public TextFormatFlags GetTextFormatFlags()
		{
			if (_isFormatStale)
			{
				UpdateTextLayoutRenderer();
			}
			return _textFormatFlags;
		}

		private void UpdateStringFormat()
		{
			_stringFormat = new StringFormat();
			_stringFormat.LineAlignment = StringAlignment.Center;
			_stringFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
			_stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
			_stringFormat.FormatFlags |= StringFormatFlags.LineLimit;
			if (!GetWrapsText())
			{
				_stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
			}
			_stringFormat.LineAlignment = GetTextLayoutRendererStringAlignment();
			_stringFormat.Alignment = GetStringAlignment();
			_stringFormat.Trimming = GetStringTrimming();
			if (_showHotkeyPrefix)
			{
				_stringFormat.HotkeyPrefix = HotkeyPrefix.Show;
			}
			if (_rightToLeft == RightToLeft.Yes)
			{
				_stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
		}

		private TextFormatFlags BuildTextFormatFlags()
		{
			TextFormatFlags textFormatFlags = TextFormatFlags.PreserveGraphicsClipping;
			if (_rightToLeft == RightToLeft.Yes)
			{
				textFormatFlags |= TextFormatFlags.RightToLeft;
			}
			textFormatFlags |= TextFormatFlags.Default;
			if (GetIsPaddingDisabled())
			{
				textFormatFlags |= TextFormatFlags.NoPadding;
			}
			if (!GetWrapsText())
			{
				textFormatFlags |= TextFormatFlags.SingleLine;
			}
			else
			{
				textFormatFlags |= TextFormatFlags.WordBreak;
				textFormatFlags |= TextFormatFlags.TextBoxControl;
			}
			if (!_showHotkeyPrefix)
			{
				textFormatFlags |= TextFormatFlags.NoPrefix;
			}
			return textFormatFlags;
		}

		private void UpdateTextLayoutRenderer()
		{
			_textFormatFlags = BuildTextFormatFlags();
			if (_rightToLeft == RightToLeft.Yes)
			{
				_textFormatFlags |= TextFormatFlags.RightToLeft;
			}
			if (_stringAlignment == StringAlignment.Center)
			{
				_textFormatFlags |= TextFormatFlags.HorizontalCenter;
			}
			else if (_stringAlignment == StringAlignment.Near)
			{
				if (_rightToLeft == RightToLeft.Yes)
				{
					_textFormatFlags |= TextFormatFlags.Right;
				}
				else
				{
					_textFormatFlags |= TextFormatFlags.Default;
				}
			}
			else if (_stringAlignment == StringAlignment.Far)
			{
				if (_rightToLeft == RightToLeft.Yes)
				{
					_textFormatFlags |= TextFormatFlags.Default;
				}
				else
				{
					_textFormatFlags |= TextFormatFlags.Right;
				}
			}
			if (_lineAlignment == StringAlignment.Center)
			{
				_textFormatFlags |= TextFormatFlags.VerticalCenter;
			}
			else if (_lineAlignment == StringAlignment.Far)
			{
				_textFormatFlags |= TextFormatFlags.Bottom;
			}
			else
			{
				_textFormatFlags |= TextFormatFlags.Default;
			}
			if (GetStringTrimming() == StringTrimming.EllipsisCharacter)
			{
				_textFormatFlags |= TextFormatFlags.EndEllipsis;
			}
			else if (GetStringTrimming() == StringTrimming.EllipsisWord)
			{
				_textFormatFlags |= TextFormatFlags.WordEllipsis;
			}
			else if (GetStringTrimming() == StringTrimming.EllipsisPath)
			{
				_textFormatFlags |= TextFormatFlags.PathEllipsis;
			}
			_isFormatStale = false;
		}

		public void ResetFormat()
		{
			if (_stringFormat != null)
			{
				_stringFormat.Dispose();
				_stringFormat = null;
			}
			_isFormatStale = true;
		}

		internal void DrawTextWithBrush(Graphics dc, string text, Rectangle bounds, Font font, Brush brush)
		{
			if (_useTextRenderer && brush is SolidBrush)
			{
				TextRenderer.DrawText(dc, text, font, bounds, ((SolidBrush)brush).Color, GetTextFormatFlags());
			}
			else
			{
				dc.DrawString(text, font, brush, bounds, GetStringFormat());
			}
		}

		internal void DrawText(Graphics dc, string text, Rectangle bounds, Font font, Color foreColor)
		{
			if (foreColor.IsEmpty)
			{
				return;
			}
			if (_useTextRenderer)
			{
				TextRenderer.DrawText(dc, text, font, bounds, foreColor, GetTextFormatFlags());
				return;
			}
			dc.DrawString(text, font, GdiCache.Brush(foreColor), bounds, GetStringFormat());
		}

		// Draws now when the text renderer is not used, otherwise adds the text to the batch.
		internal void QueueText(Graphics dc, TextBatch batch, string text, Rectangle bounds, Font font, Color foreColor)
		{
			if (foreColor.IsEmpty)
			{
				return;
			}
			if (_useTextRenderer)
			{
				batch.Add(text, bounds, font, foreColor, GetTextFormatFlags());
				return;
			}
			DrawText(dc, text, bounds, font, foreColor);
		}

		// Adds the text to the batch when the text renderer is used and the brush is solid, otherwise draws it now. Returns the
		// part of the bounds that the text can paint, or an empty rectangle when the text was drawn now.
		internal Rectangle QueueTextWithBrush(Graphics dc, TextBatch batch, string text, Rectangle bounds, Font font, Brush brush)
		{
			if (batch != null && _useTextRenderer && brush is SolidBrush solid)
			{
				TextFormatFlags flags = GetTextFormatFlags();
				batch.Add(text, bounds, font, solid.Color, flags);
				return GetPaintedArea(text, bounds, font, flags);
			}
			DrawTextWithBrush(dc, text, bounds, font, brush);
			return Rectangle.Empty;
		}

		// A single line text that is cut to its bounds paints only the rows of its line, placed as the vertical alignment says.
		// Two rows more on each side cover rounding. Any other text can paint anywhere in its bounds.
		internal static Rectangle GetPaintedArea(string text, Rectangle bounds, Font font, TextFormatFlags flags)
		{
			const TextFormatFlags measureFlags = TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
			if ((flags & TextFormatFlags.SingleLine) == 0 || (flags & TextFormatFlags.NoClipping) != 0 || !FastText.TryMeasure(text, font, measureFlags, out Size size))
			{
				return bounds;
			}
			int top = bounds.Top;
			if ((flags & TextFormatFlags.VerticalCenter) != 0)
			{
				top += (bounds.Height - size.Height) / 2;
			}
			else if ((flags & TextFormatFlags.Bottom) != 0)
			{
				top = bounds.Bottom - size.Height;
			}
			return Rectangle.Intersect(bounds, Rectangle.FromLTRB(bounds.Left, top - 2, bounds.Right, top + size.Height + 2));
		}

		internal SizeF RenderContent(Graphics dc, string text, Font font, int width)
		{
			if (_useTextRenderer)
			{
				TextFormatFlags flags = BuildTextFormatFlags();
				if (GetIsPaddingDisabled())
				{
					return TextRenderer.MeasureText(dc, text, font, new Size(width, 0), flags);
				}
				return TextRenderer.MeasureText(text, font, new Size(width, 0), flags);
			}
			return dc.MeasureString(text, font, width, GetStringFormat());
		}
	}
}
