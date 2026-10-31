using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class CheckBase
	{
		private void InvalidateLayout()
		{
			_isLayoutDirty = true;
			_preferredSize = Size.Empty;
			Invalidate();
		}

		// The size follows the DPI of the control, not that of the Graphics: a picture made of the control (DrawToBitmap, print preview)
		// draws on a Graphics at the default DPI.
		private int ScaleCheckGlyphSize()
		{
			return (int)Math.Round(CheckGlyphSizeAtReferenceDpi * (DeviceDpi / ReferenceDpi));
		}

		private Rectangle ComputeGlyphBounds(int width, ref Rectangle rectangle)
		{
			if (_appearance == System.Windows.Forms.Appearance.Button)
			{
				return Rectangle.Empty;
			}
			ContentAlignment contentAlignment = CheckAlign;
			if (RightToLeft == RightToLeft.Yes)
			{
				switch (contentAlignment)
				{
				case ContentAlignment.BottomLeft:
					contentAlignment = ContentAlignment.BottomRight;
					break;
				case ContentAlignment.MiddleLeft:
					contentAlignment = ContentAlignment.MiddleRight;
					break;
				case ContentAlignment.TopLeft:
					contentAlignment = ContentAlignment.TopRight;
					break;
				case ContentAlignment.BottomRight:
					contentAlignment = ContentAlignment.BottomLeft;
					break;
				case ContentAlignment.MiddleRight:
					contentAlignment = ContentAlignment.MiddleLeft;
					break;
				case ContentAlignment.TopRight:
					contentAlignment = ContentAlignment.TopLeft;
					break;
				}
			}
			int glyphTop = 0;
			switch (contentAlignment)
			{
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomRight:
				glyphTop = rectangle.Bottom - width;
				break;
			case ContentAlignment.TopLeft:
			case ContentAlignment.TopCenter:
			case ContentAlignment.TopRight:
				glyphTop = rectangle.Top;
				break;
			default:
				glyphTop = rectangle.Y + (rectangle.Height - width) / 2;
				break;
			}
			if (glyphTop < 0)
			{
				glyphTop = 0;
			}
			Rectangle result = Rectangle.Empty;
			switch (contentAlignment)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.BottomLeft:
				result = new Rectangle(rectangle.X, glyphTop, width, width);
				rectangle.Width -= width + GlyphTextGap;
				rectangle.X += width + GlyphTextGap;
				break;
			case ContentAlignment.TopCenter:
			case ContentAlignment.MiddleCenter:
			case ContentAlignment.BottomCenter:
			{
				int glyphLeft = rectangle.X + (rectangle.Width - width) / 2;
				if (glyphLeft < 0)
				{
					glyphLeft = 0;
				}
				result = new Rectangle(glyphLeft, glyphTop, width, width);
				break;
			}
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				result = new Rectangle(rectangle.Right - width - 1, glyphTop, width, width);
				rectangle.Width -= width + GlyphTextGap;
				break;
			}
			return result;
		}

		private void UpdatePreferredSize(Graphics graphics, Font font, Size size, TextLayoutRenderer textLayoutRenderer)
		{
			int height = base.Height;
			int width = base.Width;
			if (Dock != DockStyle.Right && Dock != DockStyle.Left)
			{
				height = font.Height;
				if (size.Height > height)
				{
					height = size.Height;
				}
				height += 4;
				height += base.Padding.Top + base.Padding.Bottom;
			}
			if (Dock != DockStyle.Bottom && Dock != DockStyle.Top && Dock != DockStyle.Fill)
			{
				int glyphSize = ScaleCheckGlyphSize();
				int textWidth = (int)textLayoutRenderer.MeasureText(graphics, Text, font, 0).Width;
				width = glyphSize + textWidth + TextExtraWidth;
				if (!size.IsEmpty)
				{
					width += size.Width + ImageGap;
				}
				width += base.Padding.Left + base.Padding.Right;
			}
			_preferredSize = new Size(width, height);
			if (AutoSize)
			{
				if (MinimumSize.Width > _preferredSize.Width)
				{
					_preferredSize.Width = MinimumSize.Width;
				}
				if (MinimumSize.Height > _preferredSize.Height)
				{
					_preferredSize.Height = MinimumSize.Height;
				}
				if (MaximumSize.Width > 0 && MaximumSize.Width < _preferredSize.Width)
				{
					_preferredSize.Width = MaximumSize.Width;
				}
				if (MaximumSize.Height > 0 && MaximumSize.Height < _preferredSize.Height)
				{
					_preferredSize.Height = MaximumSize.Height;
				}
				base.Size = _preferredSize;
			}
		}

		internal void LayoutContent(Graphics graphics, FormatStyle uiFormatStyle, TextLayoutRenderer textLayoutRenderer)
		{
			Size size = MeasureImage();
			Font font = uiFormatStyle.ResolveFont(Font);
			UpdatePreferredSize(graphics, font, size, textLayoutRenderer);
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.X += base.Padding.Left;
			clientRectangle.Y += base.Padding.Top;
			clientRectangle.Width -= base.Padding.Left + base.Padding.Right;
			clientRectangle.Height -= base.Padding.Bottom + base.Padding.Top;
			int glyphSize = ScaleCheckGlyphSize();
			_glyphBounds = ComputeGlyphBounds(glyphSize, ref clientRectangle);
			_textBounds = clientRectangle;
			if (!size.IsEmpty)
			{
				int textAreaWidth = _textBounds.Width;
				if (_imageAlignment != ImageHorizontalAlignment.Center)
				{
					textAreaWidth -= size.Width;
				}
				int x = RenderingHelper.LayoutImageHorizontallyWithMaxWidth(graphics, size, ref _textBounds, textAreaWidth, _imageAlignment, Text, textLayoutRenderer, font);
				int imageTop = clientRectangle.Top;
				if (_imageVerticalAlignment == Mordex.Windows.UI.Controls.ImageAlignment.Center)
				{
					imageTop = clientRectangle.Y + (clientRectangle.Height - size.Height) / 2;
				}
				else if (_imageVerticalAlignment == Mordex.Windows.UI.Controls.ImageAlignment.Bottom)
				{
					imageTop = clientRectangle.Bottom - size.Height - 1;
				}
				if (imageTop < 0)
				{
					imageTop = 0;
				}
				_imageBounds = new Rectangle(x, imageTop, size.Width, size.Height);
			}
			else
			{
				_imageBounds = Rectangle.Empty;
				RenderingHelper.LayoutImageHorizontallyWithMaxWidth(graphics, size, ref _textBounds, _textBounds.Width, _imageAlignment, Text, textLayoutRenderer, font);
			}
			int textHeight = (int)textLayoutRenderer.MeasureText(graphics, Text, font, _textBounds.Width).Height;
			if (_textVerticalAlignment == TextAlignment.Near)
			{
				_textBounds.Y = clientRectangle.Top;
				_textBaseline = clientRectangle.Top + textHeight + 1;
			}
			else if (_textVerticalAlignment == TextAlignment.Far)
			{
				_textBounds.Y = clientRectangle.Bottom - _textBounds.Height - 1;
				_textBaseline = _textBounds.Y + textHeight + 1;
			}
			else
			{
				_textBounds.Height = clientRectangle.Height;
				int textTop = clientRectangle.Y + (_textBounds.Height - textHeight) / 2;
				_textBaseline = textTop + textHeight;
				_textBounds.Y = textTop + 1;
				_textBounds.Height = textHeight;
			}
			if (_textBounds.Y < 0)
			{
				_textBounds.Y = 0;
			}
			_isLayoutDirty = false;
		}

		private int GetImageListIndex()
		{
			if (ImageList != null)
			{
				if (_imageKey.Length > 0 && ImageList.Images.ContainsKey(_imageKey))
				{
					return ImageList.Images.IndexOfKey(_imageKey);
				}
				if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
				{
					return _imageIndex;
				}
			}
			return -1;
		}

		private Size MeasureImage()
		{
			Size result = Size.Empty;
			if (_icon != null)
			{
				result = _icon.Size;
			}
			else if (_image != null)
			{
				result = _image.Size;
			}
			else if (_imageList != null)
			{
				if (_imageKey.Length > 0 && ImageList.Images.ContainsKey(_imageKey))
				{
					result = ImageList.ImageSize;
				}
				else if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
				{
					result = _imageList.ImageSize;
				}
			}
			if (!result.IsEmpty && !_imageSize.IsEmpty)
			{
				return _imageSize;
			}
			return result;
		}

		internal TextLayoutRenderer CreateTextLayoutRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.SetRightToLeft(RightToLeft);
			textLayoutRenderer.SetNoPadding(true);
			textLayoutRenderer.SetShowHotkeyPrefix(true);
			textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			if (_textAlignment == TextAlignment.Center)
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			}
			else if (_textAlignment == TextAlignment.Far)
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
			}
			else
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			}
			if (_textVerticalAlignment == TextAlignment.Near)
			{
				textLayoutRenderer.SetLineAlignment(StringAlignment.Near);
			}
			else if (_textVerticalAlignment == TextAlignment.Far)
			{
				textLayoutRenderer.SetLineAlignment(StringAlignment.Far);
			}
			else
			{
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			}
			if (!AutoSize)
			{
				textLayoutRenderer.SetWordWrap(WordWrap);
			}
			return textLayoutRenderer;
		}
	}
}
