using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class TextLayoutRenderer : IDisposable
	{
		internal bool _useTextRenderer;

		private bool _isFormatDirty;

		private StringTrimming _stringTrimming;

		public RightToLeft _rightToLeft;

		private StringAlignment _stringAlignment;

		private StringAlignment _lineAlignment;

		private bool _wrapText;

		private bool _noPadding;

		private bool _showHotkeyPrefix;

		private StringFormat _stringFormat;

		private TextFormatFlags _textFormatFlags;

		internal bool _clipText = true;

		internal TextLayoutRenderer(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
			_isFormatDirty = true;
			_rightToLeft = RightToLeft.No;
			_wrapText = false;
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

		public void SetStringTrimming(StringTrimming stringTrimming)
		{
			if (_stringTrimming != stringTrimming)
			{
				_stringTrimming = stringTrimming;
				InvalidateFormats();
			}
		}

		public RightToLeft GetRightToLeft()
		{
			return _rightToLeft;
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
				if (!_isFormatDirty)
				{
					InvalidateFormats();
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
				InvalidateFormats();
			}
		}

		public bool IsWrapEnabled()
		{
			return _wrapText;
		}

		public void SetWrapEnabled(bool textLayoutRendererBoolean)
		{
			if (_wrapText != textLayoutRendererBoolean)
			{
				_wrapText = textLayoutRendererBoolean;
				InvalidateFormats();
			}
		}

		public bool HasNoPadding()
		{
			return _noPadding;
		}

		public void SetNoPadding(bool noPadding)
		{
			if (_noPadding != noPadding)
			{
				_noPadding = noPadding;
				InvalidateFormats();
			}
		}

		public void SetShowHotkeyPrefix(bool showHotkeyPrefix)
		{
			if (_showHotkeyPrefix != showHotkeyPrefix)
			{
				_showHotkeyPrefix = showHotkeyPrefix;
				InvalidateFormats();
			}
		}

		internal StringFormat GetStringFormat()
		{
			if (_stringFormat == null)
			{
				BuildStringFormat();
			}
			return _stringFormat;
		}

		public TextFormatFlags GetTextFormatFlags()
		{
			if (_isFormatDirty)
			{
				BuildTextFormatFlags();
			}
			return _textFormatFlags;
		}

		private void BuildStringFormat()
		{
			_stringFormat = new StringFormat();
			_stringFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
			_stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
			_stringFormat.FormatFlags |= StringFormatFlags.NoClip;
			_stringFormat.FormatFlags |= StringFormatFlags.LineLimit;
			_stringFormat.LineAlignment = GetLineAlignment();
			_stringFormat.Alignment = GetStringAlignment();
			_stringFormat.Trimming = GetStringTrimming();
			if (!IsWrapEnabled())
			{
				_stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
			}
			if (_showHotkeyPrefix)
			{
				_stringFormat.HotkeyPrefix = HotkeyPrefix.Show;
			}
			if (_rightToLeft == RightToLeft.Yes)
			{
				_stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
		}

		private TextFormatFlags BuildBaseFormatFlags()
		{
			TextFormatFlags textFormatFlags = TextFormatFlags.PreserveGraphicsClipping;
			if (!_clipText)
			{
				textFormatFlags = TextFormatFlags.NoClipping;
			}
			if (_rightToLeft == RightToLeft.Yes)
			{
				textFormatFlags |= TextFormatFlags.RightToLeft;
			}
			textFormatFlags |= TextFormatFlags.Default;
			if (HasNoPadding())
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
				textFormatFlags |= TextFormatFlags.NoFullWidthCharacterBreak;
			}
			if (!_showHotkeyPrefix)
			{
				textFormatFlags |= TextFormatFlags.NoPrefix;
			}
			return textFormatFlags;
		}

		private void BuildTextFormatFlags()
		{
			_textFormatFlags = BuildBaseFormatFlags();
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

		public void Dispose()
		{
			InvalidateFormats();
		}

		private void InvalidateFormats()
		{
			if (_stringFormat != null)
			{
				_stringFormat.Dispose();
				_stringFormat = null;
			}
			_isFormatDirty = true;
		}

		internal void RenderContent(Graphics graphics, string text, Rectangle rectangle, RibbonForegroundStyle ribbonForegroundStyle, Font defaultFont, Color color, Color disabledColor, bool isEnabled)
		{
			using (RibbonFontLease font = ribbonForegroundStyle.LeaseFont(defaultFont))
			{
				DrawTextWithColor(graphics, text, rectangle, font.Font, ribbonForegroundStyle.ResolveForeColor(color, disabledColor, isEnabled));
			}
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

		internal void DrawTextWithColor(Graphics dc, string text, Rectangle bounds, Font font, Color foreColor)
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
			Brush brush = GdiCache.Brush(foreColor);
			dc.DrawString(text, font, brush, bounds, GetStringFormat());
		}

		internal SizeF MeasureStyledText(Graphics graphics, string text, RibbonForegroundStyle ribbonForegroundStyle, Font defaultFont, int width, int height)
		{
			if (ribbonForegroundStyle == null)
			{
				return MeasureText(graphics, text, defaultFont, width, height);
			}
			using (RibbonFontLease font = ribbonForegroundStyle.LeaseFont(defaultFont))
			{
				return MeasureText(graphics, text, font.Font, width, height);
			}
		}

		internal SizeF MeasureText(Graphics dc, string text, Font font, int width, int height)
		{
			if (_useTextRenderer)
			{
				TextFormatFlags textFormatFlags = BuildBaseFormatFlags();
				if (width > 0 && _stringTrimming != StringTrimming.None)
				{
					if (GetStringTrimming() == StringTrimming.EllipsisCharacter)
					{
						textFormatFlags |= TextFormatFlags.EndEllipsis;
					}
					else if (GetStringTrimming() == StringTrimming.EllipsisWord)
					{
						textFormatFlags |= TextFormatFlags.WordEllipsis;
					}
					else if (GetStringTrimming() == StringTrimming.EllipsisPath)
					{
						textFormatFlags |= TextFormatFlags.PathEllipsis;
					}
				}
				if (HasNoPadding() && dc != null)
				{
					return TextRenderer.MeasureText(dc, text, font, new Size(width, height), textFormatFlags);
				}
				return TextRenderer.MeasureText(text, font, new Size(width, height), textFormatFlags);
			}
			SizeF sizeF = dc.MeasureString(text, font, new SizeF(width, height), GetStringFormat());
			return new SizeF(sizeF.Width + 1f, sizeF.Height);
		}
	}
}
