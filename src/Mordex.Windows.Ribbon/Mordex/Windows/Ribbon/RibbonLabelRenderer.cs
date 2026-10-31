using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonLabelRenderer : RibbonButtonRenderer
	{
		public override bool Focusable => false;

		public override CommandType CommandType => CommandType.Label;

		public RibbonLabelRenderer(LabelCommand command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			if (_hasTextContent)
			{
				Rectangle rectangle = _textBounds;
				if (SizeStyle == CommandSizeStyle.Large)
				{
					textRenderer.SetWrapEnabled(true);
					textRenderer.SetLineAlignment(StringAlignment.Near);
					textRenderer.SetStringAlignment(StringAlignment.Center);
					rectangle.Y += 2;
				}
				else
				{
					textRenderer.SetLineAlignment(StringAlignment.Center);
					textRenderer.SetStringAlignment(StringAlignment.Near);
				}
				textRenderer.RenderContent(graphics, Text, rectangle, GetButtonCommand().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
				textRenderer.SetWrapEnabled(false);
			}
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawButtonImage(graphics, GetButtonCommand(), GetRenderHost().GetLargeImageList(GetCommand()), GetRenderHost().GetImageList(GetCommand()), _imageBounds, SizeStyle, UseBitmap, colorScheme);
			}
		}
	}
}
