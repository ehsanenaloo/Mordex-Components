using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	internal class ControlBoxPanelButtonRenderer : SmallMenuButtonRenderer
	{
		public ControlBoxPanelButtonRenderer(ButtonCommand command, ICommandRenderHost owner)
			: base(command, owner)
		{
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() != null)
			{
				int rightPadding = 18;
				_hasImageContent = RibbonDrawingUtils.HasButtonImage(GetButtonCommand(), CommandSizeStyle.Small, GetRenderHost().GetImageList(GetCommand()), GetRenderHost().GetLargeImageList(GetCommand()));
				int imageAreaWidth = 0;
				if (_hasImageContent)
				{
					imageAreaWidth = GetRenderHost().SmallImageSize.Width + 8;
				}
				int textAreaWidth = (int)textRenderer.MeasureStyledText(graphics, GetButtonCommand().Text, GetButtonCommand().ForegroundStyle, GetRenderHost().Font, 0, 0).Width + 1 + 4;
				_fullItemWidth = imageAreaWidth + textAreaWidth + rightPadding;
			}
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			int rightPadding = 18;
			if (GetRenderHost() != null)
			{
				Size smallImageSize = GetRenderHost().SmallImageSize;
				int imageAreaWidth = 0;
				if (_hasImageContent)
				{
					imageAreaWidth = smallImageSize.Width + 8;
					_imageBounds = new Rectangle(left + 4, top + 4, smallImageSize.Width, smallImageSize.Height);
				}
				else
				{
					_imageBounds = Rectangle.Empty;
				}
				_textBounds = new Rectangle(left + imageAreaWidth + 7, top, width - imageAreaWidth - rightPadding - 2, itemHeight);
				_bounds = new Rectangle(left, top, width, itemHeight);
			}
		}
	}
}
