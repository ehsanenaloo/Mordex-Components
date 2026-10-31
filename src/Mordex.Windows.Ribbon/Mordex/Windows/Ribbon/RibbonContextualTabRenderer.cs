using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonContextualTabRenderer : CommandBaseRenderer
	{
		internal Rectangle _contextualGroupBounds;

		private RibbonContextualTab _contextualTab;

		internal bool _isLeftEdgeVisible;

		public override AccessibleStates AccessibleState => AccessibleStates.None;

		public RibbonContextualTabRenderer(RibbonContextualTab tab, ICommandRenderHost owner)
			: base(null, owner, null)
		{
			_contextualTab = tab;
			_commandBaseRendererRibbon = owner.Ribbon;
		}

		public RibbonContextualTab GetContextualTab()
		{
			return _contextualTab;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRender, Font font)
		{
			Font tabFont = font;
			bool ownsFont = false;
			if (!tabFont.Bold)
			{
				tabFont = new Font(font, FontStyle.Bold);
				ownsFont = true;
			}
			_hasImageContent = RibbonDrawingUtils.HasImageSource(_contextualTab.Icon, _contextualTab.Image, GetRenderHost().GetImageList(GetCommand()), _contextualTab.ImageIndex, _contextualTab.ImageKey);
			_fullItemWidth = (int)textRender.MeasureStyledText(graphics, _contextualTab.Text, _contextualTab.ForegroundStyle, tabFont, 0, 0).Width + 1;
			if (_hasImageContent)
			{
				_fullItemWidth += GetRenderHost().SmallImageSize.Width + 4;
			}
			_fullItemWidth += 6;
			SetLayoutWidth(_fullItemWidth);
			if (ownsFont)
			{
				tabFont.Dispose();
			}
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
			int textOffset = 0;
			_hasImageContent = RibbonDrawingUtils.HasImageSource(GetContextualTab().Icon, GetContextualTab().Image, GetRenderHost().GetImageList(GetCommand()), GetContextualTab().ImageIndex, GetContextualTab().ImageKey);
			if (_hasImageContent)
			{
				_imageBounds = new Rectangle(left + 4, top + (itemHeight - 5 - GetRenderHost().SmallImageSize.Height) / 2 + 5, GetRenderHost().SmallImageSize.Width, GetRenderHost().SmallImageSize.Height);
				textOffset += GetRenderHost().SmallImageSize.Width + 4;
			}
			else
			{
				_imageBounds = Rectangle.Empty;
			}
			_textBounds = new Rectangle(left + textOffset, top + 5, _bounds.Width - textOffset, itemHeight - 5);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			using RibbonFontLease tabFont = font.Bold ? new RibbonFontLease(font) : ThemedRibbonFonts.Bold(font);
			textRenderer.SetStringAlignment(StringAlignment.Center);
			if (_hasImageContent && !_commandBaseRendererRibbon._isGlassAvailable)
			{
				RibbonDrawingUtils.DrawImageFromSources(graphics, _contextualTab.Icon, _contextualTab.Image, _contextualTab.ImageIndex, _contextualTab.ImageKey, GetRenderHost().GetImageList(GetCommand()), _imageBounds, _contextualTab.IsEffectivelyEnabled(), false, colorScheme);
			}
			MxRibbon ribbon = _contextualTab._ribbon;
			if (_contextualTab.IsMerged())
			{
				ribbon = _contextualTab.GetRootRibbon();
			}
			DrawOffice2010Background(graphics, _bounds, _contextualTab.BackColor, colorScheme);
			if (_commandBaseRendererRibbon.IsGlassCaptionActive())
			{
				using TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
				textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
				textLayoutRenderer.DrawTextWithColor(graphics, _contextualTab.Text, _textBounds, tabFont.Font, foreColor);
			}
			else
			{
				textRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
				textRenderer.DrawTextWithColor(graphics, _contextualTab.Text, _textBounds, tabFont.Font, foreColor);
			}
			if (_hasImageContent && _commandBaseRendererRibbon._isGlassAvailable)
			{
				Image image = _contextualTab.Image;
				if (_contextualTab.Icon != null)
				{
					image = _contextualTab.Icon.ToBitmap();
				}
				RibbonDrawingUtils.DrawImageFromSources(graphics, null, image, _contextualTab.ImageIndex, _contextualTab.ImageKey, GetRenderHost().GetImageList(GetCommand()), _imageBounds, _contextualTab.IsEffectivelyEnabled(), false, colorScheme);
			}
		}

		private void DrawOffice2010Background(Graphics graphics, Rectangle rectangle, Color tabColor, RibbonColorScheme colorScheme)
		{
			Rectangle band = new Rectangle(rectangle.X, rectangle.Y + 1, rectangle.Width, rectangle.Height - 1 + _commandBaseRendererRibbon._tabStripRowHeight - 5);
			ThemedRibbonPainter.ContextualBand(graphics, colorScheme.Theme, band, tabColor, true);
			return;
		}

	}
}
