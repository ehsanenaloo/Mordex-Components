using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	// Draws and measures text with TextRenderer (GDI) or with Graphics (GDI+).
	internal sealed class TextLayoutRenderer : IDisposable
	{
		private readonly bool _useTextRenderer;

		private RightToLeft _rightToLeft = RightToLeft.No;

		private StringAlignment _horizontalAlignment = StringAlignment.Center;

		private StringAlignment _verticalAlignment = StringAlignment.Center;

		private bool _wordWrap;

		private StringFormat _stringFormat;

		internal TextLayoutRenderer(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
		}

		internal RightToLeft RightToLeft
		{
			get
			{
				return _rightToLeft;
			}
			set
			{
				if (_rightToLeft != value)
				{
					_rightToLeft = value;
					ResetStringFormat();
				}
			}
		}

		internal StringAlignment HorizontalAlignment
		{
			get
			{
				return _horizontalAlignment;
			}
			set
			{
				if (_horizontalAlignment != value)
				{
					_horizontalAlignment = value;
					ResetStringFormat();
				}
			}
		}

		internal StringAlignment VerticalAlignment
		{
			get
			{
				return _verticalAlignment;
			}
			set
			{
				if (_verticalAlignment != value)
				{
					_verticalAlignment = value;
					ResetStringFormat();
				}
			}
		}

		internal bool WordWrap
		{
			get
			{
				return _wordWrap;
			}
			set
			{
				if (_wordWrap != value)
				{
					_wordWrap = value;
					ResetStringFormat();
				}
			}
		}

		private StringFormat GetStringFormat()
		{
			if (_stringFormat == null)
			{
				_stringFormat = new StringFormat
				{
					FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip | StringFormatFlags.LineLimit,
					LineAlignment = _verticalAlignment,
					Alignment = _horizontalAlignment,
					Trimming = StringTrimming.None
				};
				if (!_wordWrap)
				{
					_stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
				}
				if (_rightToLeft == RightToLeft.Yes)
				{
					_stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				}
			}
			return _stringFormat;
		}

		private TextFormatFlags GetMeasureFlags()
		{
			TextFormatFlags flags = TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.NoPrefix;
			if (_rightToLeft == RightToLeft.Yes)
			{
				flags |= TextFormatFlags.RightToLeft;
			}
			if (_wordWrap)
			{
				flags |= TextFormatFlags.WordBreak | TextFormatFlags.NoFullWidthCharacterBreak;
			}
			else
			{
				flags |= TextFormatFlags.SingleLine;
			}
			return flags;
		}

		private TextFormatFlags GetDrawFlags()
		{
			TextFormatFlags flags = GetMeasureFlags();
			bool rightToLeft = _rightToLeft == RightToLeft.Yes;
			switch (_horizontalAlignment)
			{
			case StringAlignment.Center:
				flags |= TextFormatFlags.HorizontalCenter;
				break;
			case StringAlignment.Near:
				if (rightToLeft)
				{
					flags |= TextFormatFlags.Right;
				}
				break;
			case StringAlignment.Far:
				if (!rightToLeft)
				{
					flags |= TextFormatFlags.Right;
				}
				break;
			}
			switch (_verticalAlignment)
			{
			case StringAlignment.Center:
				flags |= TextFormatFlags.VerticalCenter;
				break;
			case StringAlignment.Far:
				flags |= TextFormatFlags.Bottom;
				break;
			}
			return flags;
		}

		private void ResetStringFormat()
		{
			_stringFormat?.Dispose();
			_stringFormat = null;
		}

		public void Dispose()
		{
			ResetStringFormat();
		}

		internal void DrawText(Graphics graphics, string text, Rectangle bounds, Font font, Color foreColor)
		{
			if (foreColor.IsEmpty)
			{
				return;
			}
			if (_useTextRenderer)
			{
				TextRenderer.DrawText(graphics, text, font, bounds, foreColor, GetDrawFlags());
				return;
			}
			Brush brush = GdiCache.Brush(foreColor);
			graphics.DrawString(text, font, brush, bounds, GetStringFormat());
		}

		internal SizeF Measure(Graphics graphics, string text, Font font, int width, int height)
		{
			if (_useTextRenderer)
			{
				return TextRenderer.MeasureText(text, font, new Size(width, height), GetMeasureFlags());
			}
			SizeF size = graphics.MeasureString(text, font, new SizeF(width, height), GetStringFormat());
			return new SizeF(size.Width + 1f, size.Height);
		}
	}
}
