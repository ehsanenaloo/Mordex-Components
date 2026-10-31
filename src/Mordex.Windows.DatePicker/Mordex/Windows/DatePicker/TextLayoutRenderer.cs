using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	// Draws and measures single-line text with TextRenderer (GDI) or Graphics.DrawString (GDI+).
	internal sealed class TextLayoutRenderer : IDisposable
	{
		private readonly bool _useTextRenderer;

		private StringAlignment _alignment = StringAlignment.Center;

		private StringAlignment _lineAlignment = StringAlignment.Center;

		private RightToLeft _rightToLeft = RightToLeft.No;

		private bool _noPadding;

		private StringFormat _stringFormat;

		private TextFormatFlags? _textFormatFlags;

		// When set, texts drawn with the text renderer are collected and drawn together by Flush of the batch.
		internal TextBatch Batch { get; set; }

		internal TextLayoutRenderer(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
		}

		internal StringAlignment Alignment
		{
			get => _alignment;
			set
			{
				if (_alignment != value)
				{
					_alignment = value;
					ResetCachedFormat();
				}
			}
		}

		internal StringAlignment LineAlignment
		{
			get => _lineAlignment;
			set
			{
				if (_lineAlignment != value)
				{
					_lineAlignment = value;
					ResetCachedFormat();
				}
			}
		}

		internal RightToLeft RightToLeft
		{
			get => _rightToLeft;
			set
			{
				if (_rightToLeft != value)
				{
					_rightToLeft = value;
					ResetCachedFormat();
				}
			}
		}

		internal bool NoPadding
		{
			get => _noPadding;
			set
			{
				if (_noPadding != value)
				{
					_noPadding = value;
					ResetCachedFormat();
				}
			}
		}

		public void Dispose()
		{
			ResetCachedFormat();
		}

		private StringFormat GetStringFormat()
		{
			if (_stringFormat == null)
			{
				StringFormat format = new StringFormat();
				format.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.LineLimit | StringFormatFlags.NoWrap;
				format.LineAlignment = _lineAlignment;
				format.Alignment = _alignment;
				format.Trimming = StringTrimming.None;
				if (_rightToLeft == RightToLeft.Yes)
				{
					format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				}
				_stringFormat = format;
			}
			return _stringFormat;
		}

		private TextFormatFlags GetBaseTextFormatFlags()
		{
			TextFormatFlags flags = TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
			if (_rightToLeft == RightToLeft.Yes)
			{
				flags |= TextFormatFlags.RightToLeft;
			}
			if (_noPadding)
			{
				flags |= TextFormatFlags.NoPadding;
			}
			return flags;
		}

		private TextFormatFlags GetTextFormatFlags()
		{
			if (!_textFormatFlags.HasValue)
			{
				bool rightToLeft = _rightToLeft == RightToLeft.Yes;
				TextFormatFlags flags = GetBaseTextFormatFlags();
				if (_alignment == StringAlignment.Center)
				{
					flags |= TextFormatFlags.HorizontalCenter;
				}
				else if ((_alignment == StringAlignment.Near) == rightToLeft)
				{
					flags |= TextFormatFlags.Right;
				}
				if (_lineAlignment == StringAlignment.Center)
				{
					flags |= TextFormatFlags.VerticalCenter;
				}
				else if (_lineAlignment == StringAlignment.Far)
				{
					flags |= TextFormatFlags.Bottom;
				}
				_textFormatFlags = flags;
			}
			return _textFormatFlags.Value;
		}

		private void ResetCachedFormat()
		{
			if (_stringFormat != null)
			{
				_stringFormat.Dispose();
				_stringFormat = null;
			}
			_textFormatFlags = null;
		}

		private void DrawWithTextRenderer(Graphics graphics, string text, Font font, Rectangle bounds, Color color)
		{
			if (Batch != null)
			{
				Batch.Add(text, bounds, font, color, GetTextFormatFlags());
			}
			else
			{
				TextRenderer.DrawText(graphics, text, font, bounds, color, GetTextFormatFlags());
			}
		}

		internal void DrawText(Graphics graphics, string text, Rectangle bounds, Font font, Brush brush)
		{
			if (_useTextRenderer && brush is SolidBrush solidBrush)
			{
				DrawWithTextRenderer(graphics, text, font, bounds, solidBrush.Color);
			}
			else
			{
				graphics.DrawString(text, font, brush, bounds, GetStringFormat());
			}
		}

		internal void DrawText(Graphics graphics, string text, Rectangle bounds, Font font, Color foreColor)
		{
			if (foreColor.IsEmpty)
			{
				return;
			}
			if (_useTextRenderer)
			{
				DrawWithTextRenderer(graphics, text, font, bounds, foreColor);
				return;
			}
			graphics.DrawString(text, font, GdiCache.Brush(foreColor), bounds, GetStringFormat());
		}

		internal SizeF MeasureText(Graphics graphics, string text, Font font, int width)
		{
			if (_useTextRenderer)
			{
				TextFormatFlags flags = GetBaseTextFormatFlags();
				if (_noPadding)
				{
					return TextRenderer.MeasureText(graphics, text, font, new Size(width, 0), flags);
				}
				return TextRenderer.MeasureText(text, font, new Size(width, 0), flags);
			}
			return graphics.MeasureString(text, font, width, GetStringFormat());
		}
	}
}
