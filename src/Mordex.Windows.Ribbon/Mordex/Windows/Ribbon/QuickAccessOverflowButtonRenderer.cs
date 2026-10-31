using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class QuickAccessOverflowButtonRenderer : CommandBaseRenderer, IDropDownItem
	{
		private QuickAccessToolbarRenderer _qaBarOwner;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				return AccessibleRole.ButtonDropDown;
			}
		}

		public QuickAccessOverflowButtonRenderer(MxRibbon ribbon, QuickAccessToolbarRenderer qaBarOwner)
			: base(null, ribbon, null)
		{
			_commandBaseRendererRibbon = ribbon;
			_qaBarOwner = qaBarOwner;
			_accessibleName = "More items";
		}

		public override void Invalidate()
		{
			if (_qaBarOwner != null)
			{
				_qaBarOwner.Invalidate(update: true);
			}
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
			ThemedRibbonPainter.DoubleChevron(graphics, colorScheme.Theme, _bounds);
			return;
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left)
			{
				ShowOverflowPopup();
			}
		}

		public void ShowOverflowPopup()
		{
			ArrayList layoutItems = _qaBarOwner.GetOverflowItems();
			QuickAccessOverflowPopup popup = new QuickAccessOverflowPopup(_commandBaseRendererRibbon, GetRenderHost(), layoutItems, this);
			Active = true;
			popup.ShowAt(_qaBarOwner.PointToScreen(this, new Point(_bounds.X, _bounds.Top)));
			Active = false;
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			int keyTipWidth = (int)textRenderer.MeasureText(graphics, keyTip, font, 0, 0).Width;
			int x = _bounds.X + (_bounds.Width - keyTipWidth) / 2;
			int y = _bounds.Bottom - 10;
			return _qaBarOwner.PointToScreen(this, new Point(x, y));
		}

		void IDropDownItem.DropDown(bool selectFirstItem)
		{
			ShowOverflowPopup();
		}
	}
}
