using System.Drawing;
using Mordex.Windows.Common;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class TextLayoutRenderer
	{
		internal bool _useTextRenderer;

		// Set around the paint of one item whose text may be held back by the grid; null draws at once.
		internal GridTextBatch Batch;

		// For tests and the benchmark: measures every text with the text renderer.
		internal static bool DisableFastMeasure { get; set; }

		private bool _isDisposed;

		private StringTrimming _trimming;

		public RightToLeft _rightToLeft;

		private StringAlignment _stringAlignment;

		private StringAlignment _lineAlignment;

		private bool _noClipping;

		private bool _wordWrap;

		private bool _noPadding;

		private bool _showHotkeyPrefix;

		private StringFormat _stringFormat;

		private TextFormatFlags _textFormatFlags;

		internal TextLayoutRenderer(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
			_isDisposed = true;
			_rightToLeft = RightToLeft.No;
			_wordWrap = false;
			_stringAlignment = StringAlignment.Near;
			_lineAlignment = StringAlignment.Center;
			_trimming = StringTrimming.None;
			_noPadding = false;
			_showHotkeyPrefix = false;
		}

		internal TextLayoutRenderer(bool useTextRenderer, StringTrimming trimming)
			: this(useTextRenderer)
		{
			_trimming = trimming;
		}

		// Identifies the settings that can change the size a text measures to.
		internal int GetMeasureSignature()
		{
			int signature = (int)_trimming | ((int)_stringAlignment << 4) | ((int)_lineAlignment << 8) | ((int)_rightToLeft << 12);
			if (_useTextRenderer)
			{
				signature |= 1 << 16;
			}
			if (_wordWrap)
			{
				signature |= 1 << 17;
			}
			if (_noPadding)
			{
				signature |= 1 << 18;
			}
			if (_noClipping)
			{
				signature |= 1 << 19;
			}
			if (_showHotkeyPrefix)
			{
				signature |= 1 << 20;
			}
			return signature;
		}

		public StringTrimming GetTrimming()
		{
			return _trimming;
		}

		public void UpdateTrimming(StringTrimming trimming)
		{
			if (_trimming != trimming)
			{
				_trimming = trimming;
				ReleaseStringFormat();
			}
		}

		public void UpdateRightToLeft(RightToLeft rightToLeft)
		{
			if (_rightToLeft != rightToLeft)
			{
				_rightToLeft = rightToLeft;
				ReleaseStringFormat();
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
				if (!_isDisposed)
				{
					ReleaseStringFormat();
				}
			}
			else if (_stringFormat != null)
			{
				_stringFormat.Alignment = _stringAlignment;
			}
		}

		public void UpdateTextLayoutRendererStringAlignment(StringAlignment textLayoutRendererStringAlignment)
		{
			if (_lineAlignment != textLayoutRendererStringAlignment)
			{
				_lineAlignment = textLayoutRendererStringAlignment;
				ReleaseStringFormat();
			}
		}

		public void UpdateTextLayoutRendererBoolean(bool textLayoutRendererBoolean)
		{
			if (_noClipping != textLayoutRendererBoolean)
			{
				_noClipping = textLayoutRendererBoolean;
				ReleaseStringFormat();
			}
		}

		public bool IsWordWrap()
		{
			return _wordWrap;
		}

		public void SetWordWrap(bool wordWrap)
		{
			if (_wordWrap != wordWrap)
			{
				_wordWrap = wordWrap;
				ReleaseStringFormat();
			}
		}

		public bool IsNoPadding()
		{
			return _noPadding;
		}

		public void SetNoPadding(bool noPadding)
		{
			if (_noPadding != noPadding)
			{
				_noPadding = noPadding;
				ReleaseStringFormat();
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

		public TextFormatFlags ComputeTextFormatFlags()
		{
			if (_isDisposed)
			{
				UpdateTextLayoutRenderer();
			}
			return _textFormatFlags;
		}

		private void BuildStringFormat()
		{
			StringFormatFlags stringFormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces;
			if (!IsWordWrap())
			{
				stringFormatFlags |= StringFormatFlags.NoWrap;
				stringFormatFlags |= StringFormatFlags.LineLimit;
			}
			else
			{
				stringFormatFlags |= StringFormatFlags.LineLimit;
			}
			if (_rightToLeft == RightToLeft.Yes)
			{
				stringFormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			_stringFormat = new StringFormat(stringFormatFlags);
			_stringFormat.LineAlignment = _lineAlignment;
			_stringFormat.Alignment = _stringAlignment;
			_stringFormat.Trimming = _trimming;
			if (_showHotkeyPrefix)
			{
				_stringFormat.HotkeyPrefix = HotkeyPrefix.Show;
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
			if (!IsWordWrap())
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
			if (GetTrimming() == StringTrimming.EllipsisCharacter)
			{
				_textFormatFlags |= TextFormatFlags.EndEllipsis;
			}
			else if (GetTrimming() == StringTrimming.EllipsisWord)
			{
				_textFormatFlags |= TextFormatFlags.WordEllipsis;
			}
			else if (GetTrimming() == StringTrimming.EllipsisPath)
			{
				_textFormatFlags |= TextFormatFlags.PathEllipsis;
			}
			if (_noClipping)
			{
				_textFormatFlags |= TextFormatFlags.NoClipping;
			}
			_textFormatFlags |= TextFormatFlags.ExpandTabs;
			_isDisposed = false;
		}

		public void ReleaseStringFormat()
		{
			if (_stringFormat != null)
			{
				_stringFormat.Dispose();
				_stringFormat = null;
			}
			_isDisposed = true;
		}

		internal void DrawTextWithBrush(Graphics dc, string text, Rectangle bounds, Font font, Brush brush)
		{
			SolidBrush solidBrush = brush as SolidBrush;
			if (_useTextRenderer && solidBrush != null)
			{
				TextFormatFlags flags = ComputeTextFormatFlags();
				if (Batch == null || !Batch.TryDraw(dc, text, bounds, font, solidBrush.Color, flags))
				{
					TextRenderer.DrawText(dc, text, font, bounds, solidBrush.Color, flags);
				}
				return;
			}
			StringFormat stringFormat = GetStringFormat();
			stringFormat.SetTabStops(0f, new float[1] { font.Size * 16f / 3f });
			dc.DrawString(text, font, brush, bounds, stringFormat);
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
			Brush brush = GdiCache.Brush(foreColor);
			StringFormat stringFormat = GetStringFormat();
			stringFormat.SetTabStops(0f, new float[1] { font.Size * 16f / 3f });
			dc.DrawString(text, font, brush, bounds, stringFormat);
		}

		internal SizeF MeasureText(Graphics dc, string text, Font font, int width)
		{
			if (_useTextRenderer)
			{
				TextFormatFlags flags = BuildBaseTextFormatFlags();
				// An unbounded single line: the same GDI call as the text renderer without its lookups.
				if (!DisableFastMeasure && width <= 0 && (!IsNoPadding() || FastText.UsesScreenResolution(dc)) && FastText.TryMeasure(text, font, flags, out Size fast))
				{
					return fast;
				}
				if (IsNoPadding())
				{
					return TextRenderer.MeasureText(dc, text, font, new Size(width, 0), flags);
				}
				return TextRenderer.MeasureText(text, font, new Size(width, 0), flags);
			}
			return dc.MeasureString(text, font, width, GetStringFormat());
		}

		internal SizeF RenderContent(Graphics graphics, string text, Font font, int width, bool condition)
		{
			if (condition)
			{
				string[] array = text.Split('\n');
				SizeF result = new SizeF(0f, 0f);
				bool wordWrap = _wordWrap;
				_wordWrap = false;
				if (array == null || array.Length == 1)
				{
					result = MeasureText(graphics, text, font, width);
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						SizeF sizeF = MeasureText(graphics, array[i], font, width);
						if (sizeF.Height > result.Height)
						{
							result.Height = sizeF.Height;
						}
						if (sizeF.Width > result.Width)
						{
							result.Width = sizeF.Width;
						}
					}
				}
				_wordWrap = wordWrap;
				return result;
			}
			return MeasureText(graphics, text, font, width);
		}
	}
}
