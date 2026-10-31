using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class GalleryState : GalleryItemRenderer
	{
		private Gallery _gallery;

		internal GalleryState(MxRibbon ribbon, Gallery gallery)
			: base(null, null, null, displayDescription: false)
		{
			_commandBaseRendererRibbon = ribbon;
			_gallery = gallery;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_fullItemWidth = (int)textRenderer.MeasureText(graphics, "New Gallery Item", font, 0, 0).Width + 4;
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			RibbonChromePainter.DrawHotButtonBackground(graphics, _bounds, colorScheme, true);
			Brush brush = GdiCache.Brush(foreColor);
			textRenderer.DrawTextWithBrush(graphics, "New Gallery Item", _bounds, font, brush);
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			using (DesignEditScope edit = DesignEditScope.Begin(_gallery, "GalleryItems", "Add Gallery Item"))
			{
				GalleryItem galleryItem = new GalleryItem();
				_gallery.GalleryItems.Add(galleryItem);
				if (galleryItem.Site != null)
				{
					galleryItem.Text = galleryItem.Site.Name;
				}
				edit.End();
			}
		}
	}
}
