using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class LargeMenuButtonRenderer : SmallMenuButtonRenderer
	{
		public int _contentHeight;

		private Rectangle _descriptionBounds;

		public override CommandType CommandType => CommandType.LargeButton;

		public LargeMenuButtonRenderer(ButtonCommand command, ICommandRenderHost owner)
			: base(command, owner)
		{
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() == null)
			{
				return;
			}
			int rightPadding = 18;
			if (HasChildren)
			{
				rightPadding = 18;
			}
			_hasImageContent = RibbonDrawingUtils.HasButtonImage(GetButtonCommand(), CommandSizeStyle.Large, GetRenderHost().GetImageList(GetCommand()), GetRenderHost().GetLargeImageList(GetCommand()));
			int imageWidth = GetRenderHost().LargeImageSize.Width + 12;
			string text = GetButtonCommand().Text;
			if (ShouldShowShortcut())
			{
				text = text + "   " + RibbonDrawingUtils.GetShortcutText(GetButtonCommand().Shortcut);
			}
			int textWidth = (int)textRenderer.MeasureStyledText(graphics, text, GetButtonCommand().ForegroundStyle, font, 0, 0).Width + 3;
			int contentHeight = imageWidth;
			int textHeight = font.Height + 3;
			if (GetButtonCommand().Description.Length > 0)
			{
				int descriptionWidth = RibbonDrawingUtils.MeasureTwoLineTextWidth(graphics, textRenderer, GetButtonCommand().Description, GetButtonCommand().DescriptionForegroundStyle, font, textWidth, 0);
				textHeight += (font.Height + 3) * 2;
				if (textHeight > contentHeight)
				{
					contentHeight = textHeight;
				}
				if (descriptionWidth > textWidth)
				{
					textWidth = descriptionWidth;
				}
			}
			_fullItemWidth = imageWidth + textWidth + rightPadding;
			_contentHeight = contentHeight;
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			Size largeImageSize = GetRenderHost().LargeImageSize;
			int imageAreaWidth = largeImageSize.Width + 12;
			_imageBounds = new Rectangle(left + 4, top + (itemHeight - largeImageSize.Height) / 2, largeImageSize.Width, largeImageSize.Height);
			_bounds = new Rectangle(left, top, width, itemHeight);
			int rightPadding = 18;
			if (HasChildren)
			{
				rightPadding = 18;
			}
			if (GetButtonCommand().Description.Length > 0)
			{
				_textBounds = new Rectangle(left + imageAreaWidth, top, width - imageAreaWidth - rightPadding, itemHeight / 3);
				_descriptionBounds = new Rectangle(left + imageAreaWidth, _textBounds.Bottom, width - imageAreaWidth - rightPadding, itemHeight - _textBounds.Height);
			}
			else
			{
				_textBounds = new Rectangle(left + imageAreaWidth, top, width - imageAreaWidth - rightPadding, itemHeight);
			}
			if (GetDropDownCommand() != null && GetDropDownCommand().DropDownStyle == DropDownButtonStyle.SplitButton)
			{
				_arrowBounds = new Rectangle(_bounds.Right - rightPadding - 1, top, rightPadding + 1, itemHeight);
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			if (Hot || Selected)
			{
				if (GetCommand().IsEnabled)
				{
					DrawButtonBackground(graphics, _bounds, colorScheme, CommandSizeStyle.Small, hot: true, selected: false, pushed: false);
				}
				else
				{
					RibbonChromePainter.DrawHotMenuItemBackground(graphics, _bounds, colorScheme, true);
				}
			}
			if (HasChildren)
			{
				Pen pen = GdiCache.Pen(foreColor);
				Rectangle rectangle = new Rectangle(_bounds.Right - 19, _bounds.Y, 18, _bounds.Height);
				RibbonDrawingUtils.DrawHorizontalArrow(graphics, 17, rectangle, 1, pen);
			}
			textRenderer.SetWrapEnabled(true);
			textRenderer.SetLineAlignment(StringAlignment.Center);
			textRenderer.SetStringAlignment(StringAlignment.Near);
			textRenderer.RenderContent(graphics, GetButtonCommand().Text, _textBounds, GetButtonCommand().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
			if (ShouldShowShortcut())
			{
				textRenderer.SetStringAlignment(StringAlignment.Far);
				textRenderer.RenderContent(graphics, RibbonDrawingUtils.GetShortcutText(GetButtonCommand().Shortcut), _textBounds, GetButtonCommand().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
				textRenderer.SetStringAlignment(StringAlignment.Near);
			}
			if (GetButtonCommand().Description.Length > 0)
			{
				textRenderer.SetLineAlignment(StringAlignment.Near);
				textRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
				textRenderer.RenderContent(graphics, GetButtonCommand().Description, _descriptionBounds, GetButtonCommand().DescriptionForegroundStyle, font, colorScheme.Theme.Color(ThemeRole.TextMuted), colorScheme._disabledTextColor, GetCommand().IsEnabled);
			}
			if (GetButtonCommand().Checked)
			{
				Rectangle checkedBackgroundBounds = _imageBounds;
				checkedBackgroundBounds.Inflate(2, 2);
				DrawCheckedImageBackground(graphics, checkedBackgroundBounds, colorScheme, _hasImageContent, foreColor);
			}
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawButtonImage(graphics, GetButtonCommand(), GetRenderHost().GetLargeImageList(GetCommand()), GetRenderHost().GetImageList(GetCommand()), _imageBounds, CommandSizeStyle.Large, false, colorScheme);
			}
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			return new Point(_bounds.X + 22, _bounds.Bottom - 22);
		}
	}
}
