using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class ContainerControlCommandRenderer : CommandBaseRenderer
	{
		private bool _showText;

		private int _textWidth;

		public override CommandType CommandType => CommandType.ContainerControl;

		public ContainerControlCommandRenderer(ContainerControlCommand command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
			_showText = command.GetEffectiveCommandStyle() != CommandStyle.Image;
		}

		public ContainerControlCommand GetContainerCommand()
		{
			return _commandBaseRendererCommand as ContainerControlCommand;
		}

		public Control GetHostedControl()
		{
			if (_commandBaseRendererCommand is ContainerControlCommand)
			{
				return ((ContainerControlCommand)_commandBaseRendererCommand).Control;
			}
			return null;
		}

		public bool ShouldShowText()
		{
			return _showText;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() != null)
			{
				_hasImageContent = RibbonDrawingUtils.HasButtonImage(_commandBaseRendererCommand as ButtonCommand, CommandSizeStyle.Small, GetRenderHost().GetImageList(GetCommand()), GetRenderHost().GetLargeImageList(GetCommand()));
				textRenderer.SetWrapEnabled(false);
				_textWidth = 0;
				if (ShouldShowText() && GetContainerCommand().Text.Length > 0)
				{
					_textWidth = (int)textRenderer.MeasureStyledText(graphics, GetContainerCommand().Text, GetContainerCommand().ForegroundStyle, font, 0, 0).Width + 1;
					_hasTextContent = true;
				}
				else
				{
					_hasTextContent = false;
				}
				int imageWidth = 0;
				if (_hasImageContent)
				{
					imageWidth = GetRenderHost().SmallImageSize.Width + 4;
				}
				int controlWidth = 0;
				if (GetHostedControl() != null || GetRenderHost().IsInDesignMode)
				{
					controlWidth = GetContainerCommand().ControlWidth;
					if (_hasImageContent && _hasTextContent)
					{
						controlWidth += 2;
					}
					else if (_hasTextContent)
					{
						controlWidth += 4;
					}
				}
				_fullItemWidth = imageWidth + _textWidth + controlWidth;
			}
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (GetRenderHost() == null)
			{
				return;
			}
			int currentX = left;
			if (_hasImageContent)
			{
				int imageWidth = GetRenderHost().SmallImageSize.Width;
				_imageBounds = new Rectangle(left + 2, top + 2, imageWidth, GetRenderHost().SmallImageSize.Height);
				_textBounds = new Rectangle(left + imageWidth + 4, top, _textWidth, itemHeight);
				currentX += imageWidth + 4 + _textWidth;
			}
			else if (_hasTextContent)
			{
				_textBounds = new Rectangle(left + 2, top, _textWidth, itemHeight);
				currentX += _textWidth + 4;
			}
			if (GetHostedControl() != null && !GetHostedControl().IsDisposed)
			{
				currentX = left + width - GetContainerCommand().ControlWidth;
				Rectangle bounds = new Rectangle(currentX, top + (itemHeight - GetHostedControl().Height) / 2, GetContainerCommand().ControlWidth, GetHostedControl().Height);
				if ((itemHeight - GetHostedControl().Height) % 2 != 0)
				{
					bounds.Y++;
				}
				if (!GetHostedControl().ContainsFocus && GetHostedControl().Visible)
				{
					_ = GetHostedControl().Visible;
					GetHostedControl().Visible = false;
				}
				GetHostedControl().Bounds = bounds;
				AddControlToParent(GetHostedControl());
				GetHostedControl().Visible = GetContainerCommand().Visible;
			}
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		private void AddControlToParent(Control control)
		{
			if (GetRenderHost() != null && GetRenderHost().ParentControl != null && !GetRenderHost().ParentControl.Controls.Contains(control) && (!(GetRenderHost().ParentControl is MxRibbon) || !((MxRibbon)GetRenderHost().ParentControl).Minimized))
			{
				GetRenderHost().ParentControl.Controls.Add(control);
			}
		}

		public override void MoveY(int newY)
		{
			int deltaY = _bounds.Y - newY;
			_bounds.Y = newY;
			if (!_textBounds.IsEmpty)
			{
				_textBounds.Y -= deltaY;
			}
			if (!_imageBounds.IsEmpty)
			{
				_imageBounds.Y -= deltaY;
			}
			if (GetHostedControl() != null)
			{
				Rectangle bounds = GetHostedControl().Bounds;
				bounds.Y -= deltaY;
				GetHostedControl().Bounds = bounds;
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			GetContainerCommand().InvokeBeforePaint();
			if (_hasTextContent)
			{
				textRenderer.SetLineAlignment(StringAlignment.Center);
				textRenderer.SetStringAlignment(StringAlignment.Near);
				textRenderer.RenderContent(graphics, GetContainerCommand().Text, _textBounds, GetContainerCommand().ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, GetCommand().IsEnabled);
			}
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawButtonImage(graphics, GetContainerCommand(), GetRenderHost().GetLargeImageList(GetCommand()), GetRenderHost().GetImageList(GetCommand()), _imageBounds, GetContainerCommand().SizeStyle, false, colorScheme);
			}
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			int tipX = 0;
			int tipY = 0;
			Rectangle rectangle = _imageBounds;
			if (rectangle.IsEmpty)
			{
				rectangle = ((GetHostedControl() == null) ? _bounds : GetHostedControl().Bounds);
			}
			int rowInColumn = _rowInColumn;
			if (_rowInColumn > 3)
			{
				rowInColumn = _rowInColumn % 3;
			}
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			int tipTextWidth = (int)textRenderer.MeasureText(graphics, keyTip, font, 0, 0).Width;
			tipX = rectangle.X + (rectangle.Width - tipTextWidth) / 2;
			switch (rowInColumn)
			{
			case 0:
				tipY = rectangle.Y - 8;
				break;
			case 1:
				tipY = rectangle.Y + (rectangle.Height - (font.Height + 1)) / 2;
				break;
			default:
				tipY = rectangle.Bottom - 6;
				break;
			}
			return new Point(tipX, tipY);
		}
	}
}
