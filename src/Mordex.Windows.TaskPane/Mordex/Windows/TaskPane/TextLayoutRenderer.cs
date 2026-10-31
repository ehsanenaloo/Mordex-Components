using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.TaskPane
{
	internal class TextLayoutRenderer
	{
		internal bool _useTextRenderer;

		private bool _isCacheDirty;

		private StringTrimming _stringTrimming;

		public RightToLeft _rightToLeft;

		private StringAlignment _stringAlignment;

		private StringAlignment _lineAlignment;

		private bool _isWordWrapped;

		private bool _noPadding;

		private bool _showHotkeyPrefix;

		private StringFormat _stringFormat;

		private TextFormatFlags _textFormatFlags;

		internal TextLayoutRenderer(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
			_isCacheDirty = true;
			_rightToLeft = RightToLeft.No;
			_isWordWrapped = false;
			_stringAlignment = StringAlignment.Center;
			_lineAlignment = StringAlignment.Center;
			_stringTrimming = StringTrimming.None;
			_noPadding = false;
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
				InvalidateCache();
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
				if (!_isCacheDirty)
				{
					InvalidateCache();
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
				InvalidateCache();
			}
		}

		public bool IsWordWrapped()
		{
			return _isWordWrapped;
		}

		public void SetWordWrapped(bool isWordWrapped)
		{
			if (_isWordWrapped != isWordWrapped)
			{
				_isWordWrapped = isWordWrapped;
				InvalidateCache();
			}
		}

		public bool IsNoPadding()
		{
			return _noPadding;
		}

		public void SetShowHotkeyPrefix(bool showHotkeyPrefix)
		{
			if (_showHotkeyPrefix != showHotkeyPrefix)
			{
				_showHotkeyPrefix = showHotkeyPrefix;
				InvalidateCache();
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
			if (_isCacheDirty)
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
			if (!IsWordWrapped())
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
			if (_rightToLeft == RightToLeft.Yes)
			{
				_stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
		}

		private TextFormatFlags BuildBaseTextFormatFlags()
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
			textFormatFlags = (IsWordWrapped() ? (textFormatFlags | TextFormatFlags.WordBreak) : (textFormatFlags | TextFormatFlags.SingleLine));
			if (!_showHotkeyPrefix)
			{
				textFormatFlags |= TextFormatFlags.NoPrefix;
			}
			return textFormatFlags;
		}

		private void RebuildTextFormatFlags()
		{
			_textFormatFlags = BuildBaseTextFormatFlags();
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
			_isCacheDirty = false;
		}

		public void InvalidateCache()
		{
			if (_stringFormat != null)
			{
				_stringFormat.Dispose();
				_stringFormat = null;
			}
			_isCacheDirty = true;
		}

		internal void DrawTextWithBrush(Graphics dc, string text, Rectangle bounds, Font font, Brush brush)
		{
			if (_useTextRenderer && brush is SolidBrush)
			{
				TextRenderer.DrawText(dc, text, font, bounds, ((SolidBrush)brush).Color, ComputeTextFormatFlags());
			}
			else
			{
				dc.DrawString(text, font, brush, bounds, ComputeStringFormat());
			}
		}

		internal void DrawTextWithColor(Graphics dc, string text, Rectangle bounds, Font font, Color foreColor)
		{
			if (foreColor.IsEmpty)
			{
				return;
			}
			if (_useTextRenderer)
			{
				TextRenderer.DrawText(dc, text, font, bounds, foreColor, ComputeTextFormatFlags());
				return;
			}
			Brush foreBrush = GdiCache.Brush(foreColor);
			dc.DrawString(text, font, foreBrush, bounds, ComputeStringFormat());
		}

		internal SizeF MeasureText(Graphics dc, string text, Font font, int width)
		{
			if (_useTextRenderer)
			{
				TextFormatFlags flags = BuildBaseTextFormatFlags();
				if (IsNoPadding())
				{
					return TextRenderer.MeasureText(dc, text, font, new Size(width, 0), flags);
				}
				return TextRenderer.MeasureText(text, font, new Size(width, 0), flags);
			}
			return dc.MeasureString(text, font, width, ComputeStringFormat());
		}
	}
}
