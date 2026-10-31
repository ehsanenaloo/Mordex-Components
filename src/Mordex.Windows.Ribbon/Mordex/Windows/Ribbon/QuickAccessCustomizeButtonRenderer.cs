using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class QuickAccessCustomizeButtonRenderer : CommandBaseRenderer, IDropDownItem
	{
		private QuickAccessToolbarRenderer _toolbarRenderer;

		public override string DefaultAction => "Open";

		public QuickAccessCustomizeButtonRenderer(MxRibbon ribbon, QuickAccessToolbarRenderer qaBarOwner)
			: base(null, ribbon, null)
		{
			_commandBaseRendererRibbon = ribbon;
			_toolbarRenderer = qaBarOwner;
			_accessibleName = "Customize Toolbar";
		}

		public override void Invalidate()
		{
			if (_toolbarRenderer != null)
			{
				_toolbarRenderer.Invalidate(update: true);
			}
		}

		public override void DoDefaultAction()
		{
			ShowCustomizeMenu();
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_fullItemWidth = 13;
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (_isEnabled)
			{
				if (Hot || Active)
				{
					if (Selected || Pushed)
					{
						RibbonChromePainter.DrawSelectedHotButtonBackground(graphics, _bounds, colorScheme, true);
					}
					else
					{
						RibbonChromePainter.DrawHotButtonBackground(graphics, _bounds, colorScheme, true);
					}
				}
				else if (Selected)
				{
					RibbonChromePainter.DrawSelectedButtonBackground(graphics, _bounds, colorScheme, true);
				}
			}
			ThemedRibbonPainter.CustomizeGlyph(graphics, colorScheme.Theme, _bounds, _isEnabled);
			return;
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (_isEnabled && eventArgs.Button == MouseButtons.Left)
			{
				ShowCustomizeMenu();
			}
		}

		public void ShowCustomizeMenu()
		{
			if (_commandBaseRendererRibbon != null && !Active)
			{
				Active = true;
				_commandBaseRendererRibbon.ShowQuickAccessCustomizeMenuAt(_toolbarRenderer.PointToScreen(this, new Point(_bounds.X, _bounds.Bottom)), _bounds.Size);
				Hot = false;
				Active = false;
			}
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			ShowCustomizeMenu();
		}
	}
}
