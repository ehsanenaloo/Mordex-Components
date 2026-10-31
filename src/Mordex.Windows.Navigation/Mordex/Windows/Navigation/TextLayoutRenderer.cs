using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Navigation
{
	internal class TextLayoutRenderer
	{
		internal bool _useTextRenderer;

		private bool _isFormatDirty;

		private StringTrimming _stringTrimming;

		public RightToLeft _rightToLeft;

		private StringAlignment _stringAlignment;

		private StringAlignment _lineAlignment;

		private bool _isWrapEnabled;

		private bool _isPaddingDisabled;

		private bool _isMnemonicEnabled;

		private bool _isVertical;

		private StringFormat _stringFormat;

		private TextFormatFlags _textFormatFlags;

		internal TextLayoutRenderer(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
			_isFormatDirty = true;
			_rightToLeft = RightToLeft.No;
			_isWrapEnabled = false;
			_stringAlignment = StringAlignment.Center;
			_lineAlignment = StringAlignment.Center;
			_stringTrimming = StringTrimming.None;
			_isPaddingDisabled = false;
			_isMnemonicEnabled = false;
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
				InvalidateFormat();
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
				if (!_isFormatDirty)
				{
					InvalidateFormat();
				}
			}
			else if (_stringFormat != null)
			{
				_stringFormat.Alignment = _stringAlignment;
			}
		}

		public StringAlignment GetLineAlignment()
		{
			return _lineAlignment;
		}

		public void SetLineAlignment(StringAlignment textLayoutRendererStringAlignment)
		{
			if (_lineAlignment != textLayoutRendererStringAlignment)
			{
				_lineAlignment = textLayoutRendererStringAlignment;
				InvalidateFormat();
			}
		}

		public bool IsWrapEnabled()
		{
			return _isWrapEnabled;
		}

		public void UpdateWrapEnabled(bool isWrapEnabled)
		{
			if (_isWrapEnabled != isWrapEnabled)
			{
				_isWrapEnabled = isWrapEnabled;
				InvalidateFormat();
			}
		}

		public bool IsPaddingDisabled()
		{
			return _isPaddingDisabled;
		}

		public void UpdateMnemonicEnabled(bool isMnemonicEnabled)
		{
			if (_isMnemonicEnabled != isMnemonicEnabled)
			{
				_isMnemonicEnabled = isMnemonicEnabled;
				InvalidateFormat();
			}
		}

		public void UpdateVertical(bool isVertical)
		{
			if (_isVertical != isVertical)
			{
				_isVertical = isVertical;
				if (isVertical)
				{
					_useTextRenderer = false;
				}
			}
		}

		internal StringFormat ComputeStringFormat()
		{
			if (_stringFormat == null)
			{
				UpdateStringFormat();
			}
			return _stringFormat;
		}

		public TextFormatFlags ComputeTextFormatFlags()
		{
			if (_isFormatDirty)
			{
				RebuildTextFormatFlags();
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
			if (!IsWrapEnabled())
			{
				_stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
			}
			_stringFormat.LineAlignment = GetLineAlignment();
			_stringFormat.Alignment = GetStringAlignment();
			_stringFormat.Trimming = GetStringTrimming();
			if (_isMnemonicEnabled)
			{
				_stringFormat.HotkeyPrefix = HotkeyPrefix.Show;
			}
			if (_isVertical)
			{
				_stringFormat.FormatFlags |= StringFormatFlags.DirectionVertical;
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
			textFormatFlags |= TextFormatFlags.PreserveGraphicsTranslateTransform;
			textFormatFlags |= TextFormatFlags.Default;
			if (IsPaddingDisabled())
			{
				textFormatFlags |= TextFormatFlags.NoPadding;
			}
			if (!IsWrapEnabled())
			{
				textFormatFlags |= TextFormatFlags.SingleLine;
			}
			else
			{
				textFormatFlags |= TextFormatFlags.WordBreak;
				textFormatFlags |= TextFormatFlags.TextBoxControl;
			}
			if (!_isMnemonicEnabled)
			{
				textFormatFlags |= TextFormatFlags.NoPrefix;
			}
			return textFormatFlags;
		}

		private void RebuildTextFormatFlags()
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
			_isFormatDirty = false;
		}

		public void InvalidateFormat()
		{
			if (_stringFormat != null)
			{
				_stringFormat.Dispose();
				_stringFormat = null;
			}
			_isFormatDirty = true;
		}

		internal void DrawTextWithBrush(Graphics graphics, string text, Rectangle bounds, Font font, Brush brush)
		{
			if (_useTextRenderer && brush is SolidBrush)
			{
				TextRenderer.DrawText(graphics, text, font, bounds, ((SolidBrush)brush).Color, ComputeTextFormatFlags());
			}
			else
			{
				graphics.DrawString(text, font, brush, bounds, ComputeStringFormat());
			}
		}

		internal void DrawTextWithColor(Graphics graphics, string text, Rectangle bounds, Font font, Color foreColor)
		{
			if (foreColor.IsEmpty)
			{
				return;
			}
			if (_useTextRenderer)
			{
				TextRenderer.DrawText(graphics, text, font, bounds, foreColor, ComputeTextFormatFlags());
				return;
			}
			Brush textBrush = GdiCache.Brush(foreColor);
			graphics.DrawString(text, font, textBrush, bounds, ComputeStringFormat());
		}

		internal SizeF MeasureText(Graphics graphics, string text, Font font, int width)
		{
			if (_useTextRenderer)
			{
				TextFormatFlags flags = BuildTextFormatFlags();
				if (graphics != null)
				{
					return TextRenderer.MeasureText(graphics, text, font, new Size(width, 0), flags);
				}
				return TextRenderer.MeasureText(text, font, new Size(width, 0), flags);
			}
			return graphics?.MeasureString(text, font, width, ComputeStringFormat()) ?? new SizeF(width, 0f);
		}
	}
}
