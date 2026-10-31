using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class AddNewTabRenderer : RibbonTabRenderer
	{
		internal AddNewTabRenderer(MxRibbon ribbon)
			: base(null, ribbon)
		{
			_commandBaseRendererRibbon = ribbon;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_fullItemWidth = (int)textRenderer.MeasureText(graphics, "Add New Tab", font, 0, 0).Width + 4;
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
			textRenderer.DrawTextWithBrush(graphics, "Add New Tab", _bounds, font, brush);
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			RibbonTab ribbonTab = new RibbonTab();
			ribbonTab.Text = "Tab " + (_commandBaseRendererRibbon.Tabs.Count + 1);
			_commandBaseRendererRibbon.Tabs.Add(ribbonTab);
		}
	}
}
