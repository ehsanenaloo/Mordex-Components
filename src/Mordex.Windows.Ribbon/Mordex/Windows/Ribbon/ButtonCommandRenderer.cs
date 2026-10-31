using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class ButtonCommandRenderer : CommandBaseRenderer
	{
		private ButtonCommand _command;

		private Size _smallImageSize;

		public ButtonCommandRenderer(ButtonCommand command, ICommandRenderHost owner, Size smallImageSize)
			: base(command, owner, null)
		{
			_command = command;
			_smallImageSize = smallImageSize;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_hasImageContent = RibbonDrawingUtils.HasButtonImage(_command, CommandSizeStyle.Small, GetRenderHost().GetImageList(GetCommand()), GetRenderHost().GetLargeImageList(GetCommand()));
			textRenderer.SetWrapEnabled(false);
			int textWidth = 0;
			if (_command.Text.Length > 0)
			{
				textWidth = (int)textRenderer.MeasureStyledText(graphics, _command.Text, _command.ForegroundStyle, font, 0, 0).Width + 1;
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
				imageWidth = _smallImageSize.Width + 6;
			}
			_fullItemWidth = imageWidth + textWidth;
			if (_fullItemWidth < _smallImageSize.Width + 6)
			{
				_fullItemWidth = _smallImageSize.Width + 6;
			}
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (_hasImageContent)
			{
				int imageWidth = _smallImageSize.Width;
				int height = _smallImageSize.Height;
				_imageBounds = new Rectangle(left + 2, top + (itemHeight - height) / 2, imageWidth, height);
				_textBounds = new Rectangle(left + imageWidth + 4, top, width - imageWidth - 4, itemHeight);
			}
			else
			{
				_textBounds = new Rectangle(left + 2, top, width - 4, itemHeight);
			}
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			bool isHotOrActive = Hot || Active;
			bool pushed = Pushed;
			bool isChecked = _command.Checked;
			if ((GetCommand().IsEnabled || Active) && (isHotOrActive || isChecked || pushed))
			{
				DrawButtonBackground(graphics, _bounds, colorScheme, CommandSizeStyle.Small, isHotOrActive, isChecked, pushed);
			}
			else
			{
				DrawIdleBackground(graphics, colorScheme);
			}
			if (_hasTextContent)
			{
				Rectangle rectangle = _textBounds;
				textRenderer.SetLineAlignment(StringAlignment.Center);
				textRenderer.SetStringAlignment(StringAlignment.Near);
				textRenderer.RenderContent(graphics, _command.Text, rectangle, _command.ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
				textRenderer.SetWrapEnabled(false);
			}
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawButtonImage(graphics, _command, GetRenderHost().GetLargeImageList(GetCommand()), GetRenderHost().GetImageList(GetCommand()), _imageBounds, CommandSizeStyle.Small, false, colorScheme);
			}
		}

		private void DrawIdleBackground(Graphics graphics, RibbonColorScheme colorScheme)
		{
			ThemedRibbonPainter.Face(graphics, colorScheme.Theme, _bounds, ThemeRole.SurfaceRaised, ThemeRole.Border);
			return;
		}
	}
}
