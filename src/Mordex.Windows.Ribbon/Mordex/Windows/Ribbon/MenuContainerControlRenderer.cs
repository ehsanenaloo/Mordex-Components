using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class MenuContainerControlRenderer : ContainerControlCommandRenderer
	{
		private int _textWidth;

		private bool _drawImageMenuBackground;

		public override CommandType CommandType => CommandType.ContainerControl;

		public MenuContainerControlRenderer(ContainerControlCommand command, ICommandRenderHost owner, bool drawImageMenuBackground)
			: base(command, owner, null)
		{
			_drawImageMenuBackground = drawImageMenuBackground;
		}

		public ButtonCommand GetButtonCommand()
		{
			return _commandBaseRendererCommand as ButtonCommand;
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			return new Point(_bounds.X + 12, _bounds.Bottom - 12);
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() != null)
			{
				int imageAreaWidth = GetRenderHost().SmallImageSize.Width + 8;
				_hasImageContent = RibbonDrawingUtils.HasButtonImage(_commandBaseRendererCommand as ButtonCommand, CommandSizeStyle.Small, GetRenderHost().GetImageList(GetCommand()), GetRenderHost().GetLargeImageList(GetCommand()));
				textRenderer.SetWrapEnabled(false);
				_textWidth = 0;
				if (ShouldShowText() && GetContainerCommand().Text.Length > 0)
				{
					_textWidth = (int)textRenderer.MeasureStyledText(graphics, GetContainerCommand().Text, GetContainerCommand().ForegroundStyle, font, 0, 0).Width + 2;
					_hasTextContent = true;
				}
				else
				{
					_hasTextContent = false;
				}
				int controlAreaWidth = 0;
				if (GetHostedControl() != null)
				{
					controlAreaWidth = GetContainerCommand().ControlWidth;
					if (_hasTextContent)
					{
						controlAreaWidth += 6;
					}
				}
				_fullItemWidth = imageAreaWidth + _textWidth + controlAreaWidth;
			}
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (GetRenderHost() != null)
			{
				Size smallImageSize = GetRenderHost().SmallImageSize;
				int imageAreaWidth = smallImageSize.Width + 8;
				_imageBounds = new Rectangle(left + 4, top + 4, smallImageSize.Width, smallImageSize.Height);
				if (_hasTextContent)
				{
					_textBounds = new Rectangle(left + imageAreaWidth + 7, top, _textWidth, itemHeight);
				}
				if (GetHostedControl() != null)
				{
					Rectangle bounds = new Rectangle(left + width - GetContainerCommand().ControlWidth - 2, top + (itemHeight - GetHostedControl().Height) / 2, GetContainerCommand().ControlWidth, GetHostedControl().Height);
					GetHostedControl().Bounds = bounds;
					AddControlToParent(GetHostedControl());
				}
				_bounds = new Rectangle(left, top, width, itemHeight);
			}
		}

		internal void RemoveControlFromParent()
		{
			if (GetRenderHost() != null && GetRenderHost().ParentControl != null && GetRenderHost().ParentControl.Controls.Contains(GetHostedControl()))
			{
				GetRenderHost().ParentControl.Controls.Remove(GetHostedControl());
			}
		}

		internal void AddControlToParent(Control control)
		{
			if (GetRenderHost() != null && GetRenderHost().ParentControl != null && !GetRenderHost().ParentControl.Controls.Contains(control))
			{
				GetRenderHost().ParentControl.Controls.Add(control);
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			if (GetContainerCommand() != null)
			{
				GetContainerCommand().InvokeBeforePaint();
			}
			if (_drawImageMenuBackground)
			{
				Brush brush = GdiCache.Brush(colorScheme._menuBackground);
				graphics.FillRectangle(brush, _imageBounds);
			}
			Pen pen = GdiCache.Pen(colorScheme._menuSeparator);
			if (GetCommand().IsEnabled && Hot)
			{
				DrawButtonBackground(graphics, _bounds, colorScheme, CommandSizeStyle.Small, Hot, Selected, Pushed);
			}
			textRenderer.SetLineAlignment(StringAlignment.Center);
			textRenderer.SetStringAlignment(StringAlignment.Near);
			textRenderer.RenderContent(graphics, GetContainerCommand().Text, _textBounds, GetContainerCommand().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, _commandBaseRendererCommand.IsEnabled);
			if (GetButtonCommand() != null && GetButtonCommand().Checked)
			{
				Rectangle rectangle = _imageBounds;
				rectangle.Inflate(2, 2);
				DrawCheckedImageBackground(graphics, rectangle, colorScheme, _hasImageContent, foreColor);
			}
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawButtonImage(graphics, GetButtonCommand(), GetRenderHost().GetLargeImageList(GetCommand()), GetRenderHost().GetImageList(GetCommand()), _imageBounds, CommandSizeStyle.Small, false, colorScheme);
			}
		}

		internal void DrawCheckedImageBackground(Graphics graphics, Rectangle imageBounds, RibbonColorScheme colorScheme, bool hasImage, Color color)
		{
			ThemedRibbonPainter.CheckedBackground(graphics, colorScheme.Theme, imageBounds);
			if (!hasImage)
			{
				ThemedRibbonPainter.Check(graphics, colorScheme.Theme, _imageBounds, ThemeRole.Text);
			}
			return;
		}
	}
}
