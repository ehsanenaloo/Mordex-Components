using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonTabRenderer : CommandBaseRenderer
	{
		private bool _isContextualTab;

		public int _extraWidth;

		private int _contextualTabMinWidth;

		private RibbonTab _tab;

		public override string AccessibleName
		{
			get
			{
				if (_tab != null && _tab.AccessibleName != null && _tab.AccessibleName.Length > 0)
				{
					return _tab.AccessibleName;
				}
				if (_tab != null)
				{
					string text = _tab.Text;
					if (text != null && text.Length > 0)
					{
						return text;
					}
					return _tab.Name;
				}
				return base.AccessibleName;
			}
		}

		public override AccessibleRole AccessibleRole => AccessibleRole.PageTab;

		public override string DefaultAction => "Switch";

		public override bool Selected
		{
			get
			{
				if (GetTab() != null && GetTab().Ribbon != null)
				{
					if (GetTab().Ribbon.IsBackstageOpen())
					{
						return false;
					}
					if (!GetTab().Ribbon.Minimized || GetTab().Ribbon.IsMinimizedPopupOpen())
					{
						return GetTab().Selected;
					}
				}
				return false;
			}
		}

		public RibbonTabRenderer(RibbonTab tab, MxRibbon ribbon, bool contextualTab)
			: base(null, ribbon, null)
		{
			_commandBaseRendererRibbon = ribbon;
			_tab = tab;
			_isContextualTab = contextualTab;
		}

		public RibbonTabRenderer(RibbonTab tab, MxRibbon ribbon)
			: base(null, ribbon, null)
		{
			_commandBaseRendererRibbon = ribbon;
			_tab = tab;
		}

		public RibbonTab GetTab()
		{
			return _tab;
		}

		public override void DoDefaultAction()
		{
			if (_tab != null)
			{
				_tab.Selected = true;
			}
		}

		public bool IsOnGlassCaption()
		{
			if (_commandBaseRendererRibbon != null && _commandBaseRendererRibbon.IsCaptionBarShown())
			{
				return _commandBaseRendererRibbon.IsGlassCaptionActive();
			}
			return false;
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			int textWidth = (int)textRenderer.MeasureText(graphics, keyTip, font, 0, 0).Width;
			int x = _bounds.X + (_bounds.Width - textWidth) / 2;
			int y = _bounds.Y + (_bounds.Height - font.Height) / 2 + font.Height;
			return new Point(x, y);
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRender, Font font)
		{
			_hasImageContent = RibbonDrawingUtils.HasImageSource(GetTab().Icon, GetTab().Image, GetRenderHost().GetImageList(GetCommand()), GetTab().ImageIndex, GetTab().ImageKey);
			_fullItemWidth = (int)textRender.MeasureStyledText(graphics, GetTab().Text, GetTab().ForegroundStyle, font, 0, 0).Width + 1;
			if (_hasImageContent)
			{
				_fullItemWidth += GetRenderHost().SmallImageSize.Width + 4;
			}
			_fullItemWidth += 13;
			if (_tab.GetTabOwner() is RibbonContextualTab)
			{
				RibbonContextualTab ribbonContextualTab = _tab.GetTabOwner() as RibbonContextualTab;
				if (ribbonContextualTab.CountVisibleTabs() == 1)
				{
					RibbonContextualTabRenderer ribbonContextualTabRenderer = new RibbonContextualTabRenderer(_tab.GetTabOwner() as RibbonContextualTab, GetRenderHost());
					ribbonContextualTabRenderer.CalculateFullItemWidth(graphics, textRender, font);
					int contextualWidth = ribbonContextualTabRenderer._fullItemWidth;
					if (contextualWidth > _fullItemWidth)
					{
						_contextualTabMinWidth = contextualWidth;
						_fullItemWidth = contextualWidth;
					}
					else
					{
						_contextualTabMinWidth = contextualWidth;
					}
				}
			}
			_extraWidth = 0;
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			int imageOffset = 0;
			if (_hasImageContent)
			{
				_imageBounds = new Rectangle(left, top + (itemHeight - GetRenderHost().SmallImageSize.Height) / 2, GetRenderHost().SmallImageSize.Width, GetRenderHost().SmallImageSize.Height);
				imageOffset += GetRenderHost().SmallImageSize.Width + 4;
			}
			else
			{
				_imageBounds = Rectangle.Empty;
			}
			int maxTextWidth = width + _extraWidth + 8;
			int textWidth = (int)textRenderer.MeasureStyledText(graphics, GetTab().Text, GetTab().ForegroundStyle, GetRenderHost().Font, 0, 0).Width + 1;
			if (textWidth > maxTextWidth)
			{
				_textBounds = new Rectangle(left + imageOffset, top, maxTextWidth, itemHeight);
			}
			else
			{
				_textBounds = new Rectangle(left + imageOffset, top, textWidth, itemHeight);
			}
			int tabWidth = _textBounds.Right + 8 + _extraWidth - left;
			if (width > 0 && tabWidth > width + _extraWidth)
			{
				tabWidth = width;
			}
			if (tabWidth < _contextualTabMinWidth)
			{
				tabWidth = _contextualTabMinWidth;
			}
			_bounds = new Rectangle(left, top, tabWidth, itemHeight);
			AlignContentInBounds();
		}

		internal void AlignContentInBounds()
		{
			int width = _bounds.Width;
			int textWidth = _textBounds.Width;
			if (_hasImageContent)
			{
				int iconTextLeft = (width - _imageBounds.Width - textWidth) / 2 + 2;
				if (iconTextLeft < 2)
				{
					iconTextLeft = 2;
				}
				iconTextLeft += _bounds.X;
				_imageBounds.X = iconTextLeft;
				_textBounds.X = _imageBounds.Right + 2;
				if (textWidth + _imageBounds.Width + 4 > width)
				{
					_textBounds.Width = width - _imageBounds.Width - 4;
				}
			}
			else
			{
				int textLeft = (width - textWidth) / 2 + 2;
				if (textLeft < 2)
				{
					textLeft = 2;
				}
				textLeft += _bounds.X;
				_textBounds.X = textLeft;
				if (textWidth > width - 4)
				{
					_textBounds.Width = width - 4;
				}
			}
		}

		public override void Invalidate()
		{
			if (IsOnGlassCaption())
			{
				_commandBaseRendererRibbon.RefreshWindowFrame(true);
			}
			else
			{
				base.Invalidate();
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			Color contextualColor = _isContextualTab && _tab.GetTabOwner() is RibbonContextualTab contextualTab ? contextualTab.BackColor : Color.Empty;
			ThemedRibbonPainter.Tab(graphics, colorScheme.Theme, _bounds, Selected, Hot, contextualColor);
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawImageFromSources(graphics, _tab.Icon, _tab.Image, _tab.ImageIndex, _tab.ImageKey, GetRenderHost().GetImageList(GetCommand()), _imageBounds, _tab.IsEffectivelyEnabled(), false, colorScheme);
			}
			if (IsOnGlassCaption())
			{
				using RibbonFontLease tabFont = GetTab().ForegroundStyle.LeaseFont(font);
				if (RibbonDrawingUtils.IsClearTypeFont(tabFont.Font))
				{
					using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
					TextRenderingHint textRenderingHint = graphics.TextRenderingHint;
					SmoothingMode smoothingMode = graphics.SmoothingMode;
					graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					// The style is applied once: the leased font is already styled.
					textLayoutRenderer.DrawTextWithColor(graphics, GetTab().Text, _textBounds, tabFont.Font, GetTab().ForegroundStyle.ResolveForeColor(foreColor, colorScheme._disabledTextColor, GetTab().IsEffectivelyEnabled()));
					graphics.SmoothingMode = smoothingMode;
					graphics.TextRenderingHint = textRenderingHint;
				}
				else
				{
					RibbonDrawingUtils.DrawBackground(graphics, GetTab().Text, _textBounds, tabFont.Font, foreColor, colorScheme._disabledTextColor, GetTab().IsEffectivelyEnabled(), GetTab().ForegroundStyle);
				}
			}
			else
			{
				textRenderer.RenderContent(graphics, GetTab().Text, _textBounds, GetTab().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetTab().IsEffectivelyEnabled());
			}
		}

		internal virtual GraphicsPath GetTabPath(Rectangle rect)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(rect.X - 6, rect.Bottom - 6, 6, 6, 90f, -90f);
			graphicsPath.AddLine(rect.X, rect.Bottom - 2, rect.X, rect.Y + 2);
			graphicsPath.AddArc(rect.X, rect.Y, 6, 6, 180f, 90f);
			graphicsPath.AddLine(rect.X + 2, rect.Y, rect.Right - 3, rect.Y);
			graphicsPath.AddArc(rect.Right - 7, rect.Y, 6, 6, -90f, 90f);
			graphicsPath.AddLine(rect.Right - 1, rect.Y + 2, rect.Right - 1, rect.Bottom - 2);
			graphicsPath.AddArc(rect.Right - 1, rect.Bottom - 6, 6, 6, 180f, -90f);
			return graphicsPath;
		}

	}
}
