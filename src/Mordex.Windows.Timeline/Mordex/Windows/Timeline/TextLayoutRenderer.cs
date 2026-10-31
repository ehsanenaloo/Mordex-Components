using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Timeline
{
	internal class TextLayoutRenderer
	{
		internal bool _useTextRenderer;

		// When set, texts drawn with the text renderer are collected and drawn together by Flush of the batch.
		internal TextBatch Batch { get; set; }

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

		private void DrawWithTextRenderer(Graphics dc, string text, Font font, Rectangle bounds, Color color)
		{
			if (Batch != null)
			{
				Batch.Add(text, bounds, font, color, GetTextFormatFlags());
			}
			else
			{
				TextRenderer.DrawText(dc, text, font, bounds, color, GetTextFormatFlags());
			}
		}

		internal void DrawTextWithBrush(Graphics dc, string text, Rectangle bounds, Font font, Brush brush)
		{
			if (_useTextRenderer && brush is SolidBrush)
			{
				DrawWithTextRenderer(dc, text, font, bounds, ((SolidBrush)brush).Color);
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
				DrawWithTextRenderer(dc, text, font, bounds, foreColor);
				return;
			}
			using (Brush brush = new SolidBrush(foreColor))
			{
				dc.DrawString(text, font, brush, bounds, GetStringFormat());
			}
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
