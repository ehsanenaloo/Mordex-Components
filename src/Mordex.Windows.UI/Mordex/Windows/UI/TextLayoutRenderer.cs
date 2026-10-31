using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Mordex.Windows.UI
{
	internal class TextLayoutRenderer
	{
		internal bool _useTextRenderer;

		private bool _areFlagsDirty;

		private StringTrimming _stringTrimming;

		public RightToLeft _rightToLeft;

		private StringAlignment _stringAlignment;

		private StringAlignment _lineAlignment;

		private bool _isWordWrap;

		private bool _isNoPadding;

		private bool _showHotkeyPrefix;

		private bool _isVertical;

		private StringFormat _stringFormat;

		private TextFormatFlags _textFormatFlags;

		internal TextLayoutRenderer(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
			_areFlagsDirty = true;
			_rightToLeft = RightToLeft.No;
			_isWordWrap = false;
			_stringAlignment = StringAlignment.Center;
			_lineAlignment = StringAlignment.Center;
			_stringTrimming = StringTrimming.None;
			_isNoPadding = false;
			_showHotkeyPrefix = false;
		}

		public StringTrimming GetStringTrimming()
		{
			return _stringTrimming;
		}

		public void SetStringTrimming(StringTrimming stringTrimming)
		{
			if (_stringTrimming != stringTrimming)
			{
				_stringTrimming = stringTrimming;
				ResetCache();
			}
		}

		public RightToLeft GetRightToLeft()
		{
			return _rightToLeft;
		}

		public void SetRightToLeft(RightToLeft rightToLeft)
		{
			if (_rightToLeft != rightToLeft)
			{
				_rightToLeft = rightToLeft;
				ResetCache();
			}
		}

		public StringAlignment GetStringAlignment()
		{
			return _stringAlignment;
		}

		public void SetStringAlignment(StringAlignment stringAlignment)
		{
			if (_stringAlignment == stringAlignment)
			{
				return;
			}
			_stringAlignment = stringAlignment;
			if (_useTextRenderer)
			{
				if (!_areFlagsDirty)
				{
					ResetCache();
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

		public void SetLineAlignment(StringAlignment lineAlignment)
		{
			if (_lineAlignment != lineAlignment)
			{
				_lineAlignment = lineAlignment;
				ResetCache();
			}
		}

		public bool IsWordWrap()
		{
			return _isWordWrap;
		}

		public void SetWordWrap(bool isWordWrap)
		{
			if (_isWordWrap != isWordWrap)
			{
				_isWordWrap = isWordWrap;
				ResetCache();
			}
		}

		public bool IsNoPadding()
		{
			return _isNoPadding;
		}

		public void SetNoPadding(bool isNoPadding)
		{
			if (_isNoPadding != isNoPadding)
			{
				_isNoPadding = isNoPadding;
				ResetCache();
			}
		}

		public void SetShowHotkeyPrefix(bool showHotkeyPrefix)
		{
			if (_showHotkeyPrefix != showHotkeyPrefix)
			{
				_showHotkeyPrefix = showHotkeyPrefix;
				ResetCache();
			}
		}

		public void SetVertical(bool isVertical)
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

		internal StringFormat GetStringFormat()
		{
			if (_stringFormat == null)
			{
				CreateStringFormat();
			}
			return _stringFormat;
		}

		public TextFormatFlags GetTextFormatFlags()
		{
			if (_areFlagsDirty)
			{
				BuildTextFormatFlags();
			}
			return _textFormatFlags;
		}

		private void CreateStringFormat()
		{
			_stringFormat = new StringFormat();
			_stringFormat.LineAlignment = StringAlignment.Center;
			_stringFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
			_stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
			_stringFormat.FormatFlags |= StringFormatFlags.LineLimit;
			if (!IsWordWrap())
			{
				_stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
			}
			_stringFormat.LineAlignment = GetLineAlignment();
			_stringFormat.Alignment = GetStringAlignment();
			_stringFormat.Trimming = GetStringTrimming();
			if (_showHotkeyPrefix)
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

		private TextFormatFlags CreateBaseTextFormatFlags()
		{
			TextFormatFlags textFormatFlags = TextFormatFlags.PreserveGraphicsClipping;
			if (_rightToLeft == RightToLeft.Yes)
			{
				textFormatFlags |= TextFormatFlags.RightToLeft;
			}
			textFormatFlags |= TextFormatFlags.Default;
			if (IsNoPadding())
			{
				textFormatFlags |= TextFormatFlags.NoPadding;
			}
			textFormatFlags = (IsWordWrap() ? (textFormatFlags | TextFormatFlags.WordBreak) : (textFormatFlags | TextFormatFlags.SingleLine));
			if (!_showHotkeyPrefix)
			{
				textFormatFlags |= TextFormatFlags.NoPrefix;
			}
			return textFormatFlags | TextFormatFlags.ExpandTabs;
		}

		private void BuildTextFormatFlags()
		{
			_textFormatFlags = CreateBaseTextFormatFlags();
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
			_textFormatFlags |= GetEllipsisFlags();
			_areFlagsDirty = false;
		}

		private TextFormatFlags GetEllipsisFlags()
		{
			switch (_stringTrimming)
			{
			case StringTrimming.EllipsisCharacter:
				return TextFormatFlags.EndEllipsis;
			case StringTrimming.EllipsisWord:
				return TextFormatFlags.WordEllipsis;
			case StringTrimming.EllipsisPath:
				return TextFormatFlags.PathEllipsis;
			default:
				return TextFormatFlags.Default;
			}
		}

		public void ResetCache()
		{
			if (_stringFormat != null)
			{
				_stringFormat.Dispose();
				_stringFormat = null;
			}
			_areFlagsDirty = true;
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
			using (Brush brush = new SolidBrush(foreColor))
			{
				dc.DrawString(text, font, brush, bounds, GetStringFormat());
			}
		}

		internal SizeF MeasureText(Graphics dc, string text, Font font, int width)
		{
			if (_useTextRenderer)
			{
				TextFormatFlags textFormatFlags = CreateBaseTextFormatFlags();
				if (width > 0 && _stringTrimming != StringTrimming.None)
				{
					textFormatFlags |= GetEllipsisFlags();
				}
				if (IsNoPadding() && dc != null)
				{
					return TextRenderer.MeasureText(dc, text, font, new Size(width, 0), textFormatFlags);
				}
				return TextRenderer.MeasureText(text, font, new Size(width, 0), textFormatFlags);
			}
			return dc.MeasureString(text, font, width, GetStringFormat());
		}
	}
}
