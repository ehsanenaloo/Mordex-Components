using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class BackstageMenuButtonRenderer : RibbonButtonRenderer
	{
		private BackstageMenu _backstageMenu;

		public BackstageMenuButtonRenderer(ButtonCommand command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
			SizeStyle = CommandSizeStyle.Small;
			if (owner is BackstageMenuScrollableBase backstageMenuScrollableBase)
			{
				_backstageMenu = backstageMenuScrollableBase.GetBackstageMenu();
			}
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() != null)
			{
				_hasImageContent = RibbonDrawingUtils.HasButtonImage(GetButtonCommand(), SizeStyle, GetRenderHost().GetImageList(GetCommand()), GetRenderHost().GetLargeImageList(GetCommand()));
				textRenderer.SetWrapEnabled(false);
				int textWidth = 0;
				if (Text.Length > 0)
				{
					textWidth = (int)textRenderer.MeasureStyledText(graphics, Text, GetButtonCommand().ForegroundStyle, font, 0, 0).Width;
					_hasTextContent = true;
					if (!_hasImageContent)
					{
						textWidth += 4;
					}
				}
				else
				{
					_hasTextContent = false;
				}
				int imageWidth = 0;
				if (_hasImageContent)
				{
					imageWidth = SmallItemSize.Width + 6;
				}
				if (_fullItemWidth < SmallItemSize.Width + 6)
				{
					_fullItemWidth = SmallItemSize.Width + 6;
				}
				_fullItemWidth = imageWidth + textWidth + 24;
			}
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (GetRenderHost() == null)
			{
				return;
			}
			left += 6;
			width -= 12;
			if (_hasImageContent)
			{
				int imageWidth = SmallItemSize.Width;
				int height = SmallItemSize.Height;
				if (!_hasTextContent)
				{
					_imageBounds = new Rectangle(left + 15, top + (itemHeight - height) / 2, imageWidth, height);
				}
				else
				{
					_imageBounds = new Rectangle(left + 14, top + (itemHeight - height) / 2, imageWidth, height);
				}
				_textBounds = new Rectangle(left + imageWidth + 16, top, width - imageWidth - 4 - ArrowWidth, itemHeight);
			}
			else
			{
				_textBounds = new Rectangle(left + 14, top, width - 4, itemHeight);
			}
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			SizeF sizeF = textRenderer.MeasureText(graphics, keyTip, font, 0, 0);
			int x = _textBounds.X - (int)sizeF.Width + 4;
			int y = _textBounds.Y + _textBounds.Height / 2 - (int)sizeF.Height - 4;
			return new Point(x, y);
		}

		internal override void DrawButtonBackground(Graphics graphics, Rectangle rect, RibbonColorScheme colorScheme, CommandSizeStyle sizeStyle, bool hot, bool selected, bool pushed)
		{
			if (!hot && !pushed && !selected)
			{
				return;
			}
			ThemedRibbonPainter.BackstageRow(graphics, colorScheme.Theme, rect, hot, selected, pushed);
			return;
		}
	}
}
