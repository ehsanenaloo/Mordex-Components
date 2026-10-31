using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonButtonRenderer : CommandBaseRenderer
	{
		private CommandSizeStyle _sizeStyle;

		private bool _showText;

		internal bool _isSingleWordText;

		private bool _droppedDown;

		public override string AccessibleName
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleName != null && GetCommand().AccessibleName.Length > 0)
				{
					return GetCommand().AccessibleName;
				}
				if (Text != null)
				{
					string text = Text.Replace("&", "");
					if (text.Length > 0)
					{
						return text;
					}
				}
				return base.AccessibleName;
			}
		}

		public override string AccessibleDescription
		{
			get
			{
				if (GetButtonCommand() != null)
				{
					if (GetButtonCommand().AccessibleDescription != null && GetButtonCommand().AccessibleDescription.Length > 0)
					{
						return GetCommand().AccessibleDescription;
					}
					if (GetButtonCommand().SuperTipSettings != null)
					{
						string text = GetButtonCommand().SuperTipSettings.Text;
						if (text != null && text.Length > 0)
						{
							return text;
						}
					}
				}
				return base.AccessibleDescription;
			}
		}

		public override AccessibleStates AccessibleState
		{
			get
			{
				AccessibleStates accessibleStates = base.AccessibleState;
				if (GetButtonCommand() != null && GetButtonCommand().Checked)
				{
					accessibleStates |= AccessibleStates.Checked;
				}
				return accessibleStates;
			}
		}

		public override string DefaultAction => "Press";

		public virtual bool UseBitmap => false;

		public virtual Size SmallItemSize => GetRenderHost().SmallImageSize;

		public virtual bool ShowText
		{
			get
			{
				return _showText;
			}
			set
			{
				_showText = value;
			}
		}

		public virtual bool DroppedDown
		{
			get
			{
				return _droppedDown;
			}
			set
			{
				if (_droppedDown != value)
				{
					_droppedDown = value;
					Invalidate();
				}
			}
		}

		public override CommandType CommandType
		{
			get
			{
				if (SizeStyle == CommandSizeStyle.Large)
				{
					return CommandType.LargeButton;
				}
				return CommandType.SmallButton;
			}
		}

		public virtual CommandSizeStyle SizeStyle
		{
			get
			{
				return _sizeStyle;
			}
			set
			{
				_sizeStyle = value;
			}
		}

		public virtual string Text => GetButtonCommand().Text;

		public virtual int ArrowWidth => 0;

		public RibbonButtonRenderer(ButtonCommand command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
			_sizeStyle = command.GetSizeStyle();
			_showText = command.GetEffectiveCommandStyle() != CommandStyle.Image;
			_isSingleWordText = command.Text.IndexOf(" ") <= 0;
		}

		public ButtonCommand GetButtonCommand()
		{
			return _commandBaseRendererCommand as ButtonCommand;
		}

		public override void DoDefaultAction()
		{
			GetCommand().InvokeOnClick();
		}

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			int tipX = 0;
			int tipY = 0;
			if (SizeStyle == CommandSizeStyle.Large)
			{
				if (keyTip.Length == 1)
				{
					keyTip = "W";
				}
				int tipTextWidth = (int)textRenderer.MeasureText(graphics, keyTip, font, 0, 0).Width;
				tipX = _bounds.X + (_bounds.Width - tipTextWidth) / 2;
				tipY = _bounds.Bottom - 6;
			}
			else
			{
				int rowInColumn = _rowInColumn;
				if (_rowInColumn > 3)
				{
					rowInColumn = _rowInColumn % 3;
				}
				tipX = _bounds.X + 11;
				switch (rowInColumn)
				{
				case 0:
					tipY = _bounds.Y - 8;
					break;
				case 1:
					tipY = _bounds.Y + (_bounds.Height - (font.Height + 1)) / 2;
					break;
				default:
					tipY = _bounds.Bottom - 6;
					break;
				}
			}
			return new Point(tipX, tipY);
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			if (GetRenderHost() == null)
			{
				return;
			}
			_hasImageContent = RibbonDrawingUtils.HasButtonImage(GetButtonCommand(), SizeStyle, GetRenderHost().GetImageList(GetCommand()), GetRenderHost().GetLargeImageList(GetCommand()));
			if (SizeStyle == CommandSizeStyle.Large)
			{
				int textWidth = 0;
				if (ShowText && GetButtonCommand().GetEffectiveCommandStyle() != CommandStyle.Image && Text.Length > 0)
				{
					if (!_isSingleWordText)
					{
						textWidth = RibbonDrawingUtils.MeasureTwoLineTextWidth(graphics, textRenderer, Text, GetButtonCommand().ForegroundStyle, font, 0, ArrowWidth) + 4;
						if (ArrowWidth > 0)
						{
							using (RibbonFontLease styledFont = GetButtonCommand().ForegroundStyle.LeaseFont(font))
							{
								if (RibbonDrawingUtils.MeasureLastWord(graphics, GetButtonCommand().Text, new Rectangle(0, 0, textWidth, font.Height * 2), styledFont.Font).Right > (float)(textWidth - 4))
								{
									textWidth += ArrowWidth;
								}
							}
						}
					}
					else
					{
						textWidth = (int)textRenderer.MeasureStyledText(graphics, GetButtonCommand().Text, GetButtonCommand().ForegroundStyle, font, 0, 0).Width;
						textWidth++;
					}
					_hasTextContent = true;
				}
				else
				{
					_hasTextContent = false;
				}
				_ = GetRenderHost().LargeImageSize.Width;
				int largeItemWidth = GetRenderHost().LargeImageSize.Width + 10;
				if (textWidth > largeItemWidth)
				{
					largeItemWidth = textWidth;
				}
				_fullItemWidth = largeItemWidth;
			}
			else
			{
				textRenderer.SetWrapEnabled(false);
				int smallTextWidth = 0;
				if (ShowText && Text.Length > 0)
				{
					smallTextWidth = (int)textRenderer.MeasureStyledText(graphics, Text, GetButtonCommand().ForegroundStyle, font, 0, 0).Width;
					_hasTextContent = true;
					if (!_hasImageContent && ArrowWidth == 0)
					{
						smallTextWidth += 4;
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
				_fullItemWidth = imageWidth + smallTextWidth + ArrowWidth;
				if (_fullItemWidth < SmallItemSize.Width + 6)
				{
					_fullItemWidth = SmallItemSize.Width + 6;
				}
			}
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			if (GetRenderHost() == null)
			{
				return;
			}
			if (SizeStyle == CommandSizeStyle.Large)
			{
				if (_hasImageContent)
				{
					int x = left + (width - GetRenderHost().LargeImageSize.Width) / 2;
					_imageBounds = new Rectangle(x, top + 3, GetRenderHost().LargeImageSize.Width, GetRenderHost().LargeImageSize.Height);
				}
				else
				{
					_imageBounds = Rectangle.Empty;
				}
				if (_hasTextContent)
				{
					int textTop = top + GetRenderHost().LargeImageSize.Height + 3;
					_ = GetRenderHost().Font.Height;
					_textBounds = new Rectangle(left, textTop, width, itemHeight + top - textTop);
					if (_textBounds.Bottom > top + itemHeight)
					{
						_textBounds = new Rectangle(left, top + GetRenderHost().LargeImageSize.Height, width, itemHeight - GetRenderHost().LargeImageSize.Height);
					}
				}
				else
				{
					_textBounds = Rectangle.Empty;
				}
			}
			else if (_hasImageContent)
			{
				int imageWidth = SmallItemSize.Width;
				int height = SmallItemSize.Height;
				if (!_hasTextContent)
				{
					_imageBounds = new Rectangle(left + 3, top + (itemHeight - height) / 2, imageWidth, height);
				}
				else
				{
					_imageBounds = new Rectangle(left + 2, top + (itemHeight - height) / 2, imageWidth, height);
				}
				_textBounds = new Rectangle(left + imageWidth + 4, top, width - imageWidth - 4 - ArrowWidth, itemHeight);
			}
			else
			{
				_textBounds = new Rectangle(left + 2, top, width - 4, itemHeight);
			}
			_bounds = new Rectangle(left, top, width, itemHeight);
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRect)
		{
			if (GetButtonCommand() == null || GetCommand() == null || GetRenderHost() == null)
			{
				return;
			}
			bool isHot = Hot || Active;
			bool isPushed = Pushed;
			bool isChecked = GetButtonCommand().Checked;
			if (DroppedDown)
			{
				isPushed = false;
				isChecked = true;
				isHot = false;
			}
			if ((GetCommand().IsEnabled || Active) && (isHot || isChecked || isPushed))
			{
				DrawButtonBackground(graphics, _bounds, colorScheme, SizeStyle, isHot, isChecked, isPushed);
			}
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
