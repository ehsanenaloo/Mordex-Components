using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class GalleryItemRenderer : CommandBaseRenderer
	{
		private GalleryItem _item;

		private Gallery _gallery;

		private Rectangle _galleryItemRendererRectangle;

		private bool _showDescription;

		public int _rowIndex;

		public int _columnInGalleryRow;

		public bool _displayDescription;

		public Size _size;

		public GalleryItemRenderer(GalleryItem item, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner, bool displayDescription)
			: base(item, owner, layoutItemOwner)
		{
			_item = item;
			if (item != null)
			{
				_gallery = item.GetParentContainer() as Gallery;
			}
			_displayDescription = displayDescription && item.GetEffectiveStyle() != CommandStyle.Image;
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int itemHeight)
		{
			_bounds = new Rectangle(left, top, width, itemHeight);
			if (_item.GetEffectiveStyle() != CommandStyle.Text)
			{
				_hasImageContent = RibbonDrawingUtils.HasImageSource(_item.Icon, _item.Image, _gallery.ImageList, _item.ImageIndex, _item.ImageKey);
			}
			else
			{
				_hasImageContent = false;
			}
			_hasTextContent = _item.GetEffectiveStyle() != CommandStyle.Image && _item.Text.Length > 0;
			if (_gallery == null)
			{
				return;
			}
			_showDescription = _displayDescription && _item.Description.Length > 0;
			if (_showDescription)
			{
				_showDescription = !_hasImageContent || (_gallery.ItemsTextImageRelation != TextImageRelation.TextAboveImage && _gallery.ItemsTextImageRelation != TextImageRelation.ImageAboveText);
			}
			Size size = Size.Empty;
			if (_hasImageContent)
			{
				if (!_gallery.ImageSize.IsEmpty)
				{
					_size = _gallery.ImageSize;
				}
				else if (_item.Icon != null)
				{
					_size = _item.Icon.Size;
				}
				else if (_item.Image != null)
				{
					_size = _item.Image.Size;
				}
				else if (_gallery.ImageList != null)
				{
					_size = _gallery.ImageList.ImageSize;
				}
				size = new Size(_size.Width + 4, _size.Height + 4);
			}
			if (_hasTextContent)
			{
				Font font = GetRenderHost().Font;
				int textWidth = (int)textRenderer.MeasureStyledText(graphics, _item.Text, _item.ForegroundStyle, font, width, 0).Width + 1;
				if (_showDescription)
				{
					int descriptionWidth = RibbonDrawingUtils.MeasureTwoLineTextWidth(graphics, textRenderer, _item.Description, _item.DescriptionForegroundStyle, font, width - 4 - size.Width, 0);
					if (descriptionWidth > textWidth)
					{
						textWidth = descriptionWidth;
					}
				}
				Rectangle rectangle = _bounds;
				rectangle.Inflate(-3, -3);
				_textBounds = rectangle;
				if (_hasImageContent)
				{
					int x = ArrangeImageHorizontally(graphics, size, ref _textBounds, textWidth);
					int imageY = ArrangeImageVertically(graphics, size, ref _textBounds, font);
					_imageBounds = new Rectangle(x, imageY + 2, _size.Width, _size.Height);
				}
				else
				{
					_imageBounds = Rectangle.Empty;
				}
				if (_showDescription)
				{
					_galleryItemRendererRectangle = _textBounds;
					_galleryItemRendererRectangle.Inflate(-2, 0);
					_textBounds.Height = font.Height + 1;
					_galleryItemRendererRectangle.Y += font.Height + 3;
					_galleryItemRendererRectangle.Height -= font.Height + 3;
				}
			}
			else
			{
				_textBounds = Rectangle.Empty;
				if (_hasImageContent)
				{
					_imageBounds = new Rectangle(_bounds.X + (_bounds.Width - _size.Width) / 2, _bounds.Y + (_bounds.Height - _size.Height) / 2, _size.Width, _size.Height);
				}
				else
				{
					_imageBounds = Rectangle.Empty;
				}
			}
		}

		public int ArrangeImageHorizontally(Graphics graphics, Size size, ref Rectangle rectangle, int textWidth)
		{
			bool isRightToLeft = GetRenderHost().RightToLeft == RightToLeft.Yes;
			TextImageRelation textImageRelation = _gallery.ItemsTextImageRelation;
			StringAlignment stringAlignment;
			switch (_gallery.ItemsTextAlignment)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.BottomLeft:
				stringAlignment = (isRightToLeft ? StringAlignment.Far : StringAlignment.Near);
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				stringAlignment = ((!isRightToLeft) ? StringAlignment.Far : StringAlignment.Near);
				break;
			default:
				stringAlignment = StringAlignment.Center;
				break;
			}
			if (isRightToLeft)
			{
				switch (textImageRelation)
				{
				case TextImageRelation.TextBeforeImage:
					textImageRelation = TextImageRelation.ImageBeforeText;
					break;
				case TextImageRelation.ImageBeforeText:
					textImageRelation = TextImageRelation.TextBeforeImage;
					break;
				}
			}
			switch (textImageRelation)
			{
			case TextImageRelation.TextBeforeImage:
				switch (stringAlignment)
				{
				case StringAlignment.Center:
					rectangle.X += (rectangle.Width - textWidth - size.Width) / 2;
					rectangle.Width = textWidth;
					return rectangle.Right;
				case StringAlignment.Far:
					rectangle.X = rectangle.Right - textWidth - size.Width;
					rectangle.Width = textWidth;
					return rectangle.Right;
				default:
					rectangle.Width = textWidth;
					return rectangle.Right - 2;
				}
			case TextImageRelation.ImageBeforeText:
				switch (stringAlignment)
				{
				case StringAlignment.Center:
					rectangle.X += (rectangle.Width - size.Width - textWidth) / 2 + size.Width + 2;
					rectangle.Width = textWidth;
					return rectangle.X - size.Width - 2;
				case StringAlignment.Far:
					rectangle.X = rectangle.Right - textWidth;
					rectangle.Width = textWidth;
					return rectangle.X - size.Width - 2;
				default:
				{
					int x = rectangle.X;
					rectangle.X += size.Width + 2;
					rectangle.Width = textWidth;
					return x + 2;
				}
				}
			default:
			{
				StringAlignment imageAlignment;
				switch (_gallery.ItemsImageAlignment)
				{
				case ContentAlignment.TopLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.BottomLeft:
					imageAlignment = (isRightToLeft ? StringAlignment.Far : StringAlignment.Near);
					break;
				case ContentAlignment.TopRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.BottomRight:
					imageAlignment = ((!isRightToLeft) ? StringAlignment.Far : StringAlignment.Near);
					break;
				default:
					imageAlignment = StringAlignment.Center;
					break;
				}
				switch (imageAlignment)
				{
				case StringAlignment.Near:
					return rectangle.Left + 2;
				case StringAlignment.Center:
					return rectangle.Left + (rectangle.Width - size.Width) / 2;
				case StringAlignment.Far:
					return rectangle.Right - size.Width - 2;
				default:
					return 0;
				}
			}
			}
		}

		public int ArrangeImageVertically(Graphics graphics, Size size, ref Rectangle rectangle, Font font)
		{
			int textHeight = 0;
			int top = rectangle.Top;
			switch (_gallery.ItemsTextImageRelation)
			{
			case TextImageRelation.TextAboveImage:
			{
				textHeight = font.Height + 1;
				int topOffset = (rectangle.Height - textHeight - size.Height - 2) / 2;
				if (topOffset > 0)
				{
					rectangle.Y += topOffset;
				}
				rectangle.Height = textHeight;
				return rectangle.Y + textHeight + 2;
			}
			case TextImageRelation.ImageAboveText:
				textHeight = font.Height + 1;
				top += (rectangle.Height - textHeight - size.Height) / 2;
				if (top < rectangle.Top)
				{
					top = rectangle.Top;
				}
				rectangle.Y = top + size.Height + 1;
				rectangle.Height = textHeight;
				return top;
			default:
				switch (_gallery.ItemsImageAlignment)
				{
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight:
					return rectangle.Bottom - size.Height;
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopRight:
					return rectangle.Top;
				default:
					return rectangle.Top + (rectangle.Height - size.Height) / 2;
				}
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			bool isHighlighted = Hot || Active;
			bool isSelected = Selected || _item.Checked;
			if (_item.IsEnabled && (isHighlighted || isSelected))
			{
				DrawButtonBackground(graphics, _bounds, colorScheme, CommandSizeStyle.Small, isHighlighted, isSelected, Pushed);
			}
			Color color = _item.BackColor;
			if (color.IsEmpty)
			{
				color = _gallery.ItemsBackColor;
			}
			if (!color.IsEmpty)
			{
				Brush brush = GdiCache.Brush(color);
				Rectangle rect = Rectangle.Inflate(_bounds, -4, -4);
				graphics.FillRectangle(brush, rect);
			}
			textRenderer.SetWrapEnabled(false);
			textRenderer.RenderContent(graphics, _item.Text, _textBounds, _item.ForegroundStyle, font, foreColor, colorScheme._disabledTextColor, _item.IsEnabled);
			if (_showDescription)
			{
				textRenderer.SetWrapEnabled(true);
				textRenderer.SetLineAlignment(StringAlignment.Near);
				textRenderer.RenderContent(graphics, _item.Description, _galleryItemRendererRectangle, _item.DescriptionForegroundStyle, font, foreColor, colorScheme._disabledTextColor, _item.IsEnabled);
				textRenderer.SetWrapEnabled(false);
			}
			if (_hasImageContent)
			{
				RibbonDrawingUtils.DrawImageFromSources(graphics, _item.Icon, _item.Image, _item.ImageIndex, _item.ImageKey, _gallery.ImageList, _imageBounds, _item.IsEnabled, false, colorScheme);
			}
		}
	}
}
