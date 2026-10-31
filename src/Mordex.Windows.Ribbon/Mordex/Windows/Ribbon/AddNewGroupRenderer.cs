using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class AddNewGroupRenderer : RibbonGroupBoxRenderer
	{
		internal AddNewGroupRenderer(MxRibbon ribbon, ICommandRenderHost owner)
			: base(null, ribbon, owner)
		{
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_isCollapsed = true;
			_childRenderers = new List<CommandBaseRenderer>();
			_fullItemWidth = 75;
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			RibbonChromePainter.DrawHotButtonBackground(graphics, _bounds, colorScheme, false);
			textRenderer.SetWrapEnabled(true);
			textRenderer.SetLineAlignment(StringAlignment.Center);
			textRenderer.SetStringAlignment(StringAlignment.Center);
			Brush brush = GdiCache.Brush(foreColor);
			textRenderer.DrawTextWithBrush(graphics, "Add New Group", _bounds, font, brush);
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			if (Pushed)
			{
				RibbonTab tab = _commandBaseRendererRibbon.SelectedTab;
				using (DesignEditScope edit = DesignEditScope.Begin(tab, "Groups", "Add Group"))
				{
					RibbonGroup ribbonGroup = new RibbonGroup();
					ribbonGroup.Text = "Group " + tab.Groups.Count;
					tab.Groups.Add(ribbonGroup);
					edit.End();
				}
			}
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			GetRenderHost().MouseDownItem = this;
			if (eventArgs.Button == MouseButtons.Left)
			{
				Pushed = true;
			}
		}
	}
}
