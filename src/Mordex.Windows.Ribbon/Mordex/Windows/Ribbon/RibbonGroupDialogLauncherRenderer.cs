using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonGroupDialogLauncherRenderer : CommandBaseRenderer
	{
		public override string AccessibleName
		{
			get
			{
				string text = "";
				if (_layoutItemOwner is RibbonGroupBoxRenderer ribbonRenderer && ribbonRenderer.GetRibbonGroup() != null)
				{
					text = ribbonRenderer.GetRibbonGroup().DialogButtonSuperTipSettings.HeaderText;
				}
				if (text != null && text.Length > 0)
				{
					return text;
				}
				return base.AccessibleName;
			}
		}

		public override bool Hot
		{
			get
			{
				return base.Hot;
			}
			set
			{
				if (value == base.Hot)
				{
					return;
				}
				base.Hot = value;
				if (_layoutItemOwner is RibbonGroupBoxRenderer groupRenderer && groupRenderer.GetRibbonGroup().Ribbon != null)
				{
					if (value)
					{
						groupRenderer.GetRibbonGroup().Ribbon.ShowSuperTip(null);
						groupRenderer.GetRibbonGroup().Ribbon.ShowSuperTip(this);
					}
					else
					{
						groupRenderer.GetRibbonGroup().Ribbon.ShowSuperTip(null);
					}
				}
			}
		}

		public RibbonGroupDialogLauncherRenderer(ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(null, owner, layoutItemOwner)
		{
		}

		public override void Invalidate()
		{
			if (GetRenderHost() != null && GetRenderHost().ParentControl != null)
			{
				GetRenderHost().ParentControl.Invalidate(_bounds);
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (Hot || Active)
			{
				if (Pushed)
				{
					RibbonChromePainter.DrawBackground(graphics, _bounds, colorScheme, true);
				}
				else
				{
					RibbonChromePainter.DrawHotButtonBackground(graphics, _bounds, colorScheme, true);
				}
			}
			ThemedRibbonPainter.DialogLauncher(graphics, colorScheme.Theme, _bounds);
			return;
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			int keyTipWidth = (int)textRenderer.MeasureText(graphics, keyTip, font, 0, 0).Width;
			int x = _bounds.X + (_bounds.Width - keyTipWidth) / 2;
			int y = _bounds.Bottom + 2;
			return new Point(x, y);
		}
	}
}
