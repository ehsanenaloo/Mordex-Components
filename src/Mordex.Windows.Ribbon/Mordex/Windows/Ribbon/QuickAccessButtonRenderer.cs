using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal class QuickAccessButtonRenderer : RibbonButtonRenderer
	{
		private QuickAccessToolbarRenderer _toolbarRenderer;

		public override bool UseBitmap
		{
			get
			{
				if (_commandBaseRendererCommand.IsMerged() && _commandBaseRendererCommand.ResolveRootRibbon() != null)
				{
					return GetCommand().ResolveRootRibbon().IsGlassCaptionActive();
				}
				if (GetCommand().Ribbon != null)
				{
					return GetCommand().Ribbon.IsGlassCaptionActive();
				}
				return false;
			}
		}

		public override CommandSizeStyle SizeStyle
		{
			get
			{
				return CommandSizeStyle.Small;
			}
			set
			{
				base.SizeStyle = value;
			}
		}

		public override bool ShowText
		{
			get
			{
				return false;
			}
			set
			{
				base.ShowText = value;
			}
		}

		public override Size SmallItemSize => new Size(16, 16);

		public QuickAccessButtonRenderer(ButtonCommand command, ICommandRenderHost owner, QuickAccessToolbarRenderer qaBarOwner)
			: base(command, owner, null)
		{
			_toolbarRenderer = qaBarOwner;
		}

		public override void Invalidate()
		{
			if (_toolbarRenderer != null)
			{
				_toolbarRenderer.Invalidate(update: true);
			}
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
			if (_isVisible)
			{
				return _toolbarRenderer.PointToScreen(this, new Point(x, y));
			}
			return new Point(x, y);
		}
	}
}
