using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxGroupBox
	{
		private int GetCaptionHeight()
		{
			FormatStyle uiFormatStyle = CreateBaseFormatStyle();
			uiFormatStyle = uiFormatStyle.MergeStyle(_formatStyle);
			int fontLineHeight = 0;
			if (Text.Length > 0)
			{
				fontLineHeight = uiFormatStyle.ResolveFont(Font).Height + 1;
			}
			Size size = GetContentImageSize();
			return (fontLineHeight >= size.Height) ? fontLineHeight : size.Height;
		}

		private Size GetContentImageSize()
		{
			Size result = Size.Empty;
			if (_icon != null)
			{
				result = _icon.Size;
			}
			else if (_displayImage != null)
			{
				result = _displayImage.Size;
			}
			else if (_imageList != null)
			{
				if (_imageKey.Length > 0 && ImageList.Images.ContainsKey(_imageKey))
				{
					result = _imageList.ImageSize;
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

		private void LayoutCaption(Graphics graphics, TextLayoutRenderer textLayoutRenderer, FormatStyle uiFormatStyle)
		{
			Font font = uiFormatStyle.ResolveFont(Font);
			int lineHeight = font.Height + 1;
			Rectangle clientRectangle = base.ClientRectangle;
			int textInset = _textOffset;
			if (_textOffset == DefaultTextOffset)
			{
				textInset = DefaultTextInset;
			}
			clientRectangle.Inflate(-textInset, 0);
			Size size = GetContentImageSize();
			_textBounds = clientRectangle;
			_textBounds.Height = lineHeight;
			int textWidth = 0;
			if (Text.Length > 0)
			{
				textWidth = (int)textLayoutRenderer.MeasureText(graphics, Text, font, 0).Width + 1;
			}
			if (textWidth > clientRectangle.Width)
			{
				textWidth = clientRectangle.Width;
			}
			if (!size.IsEmpty)
			{
				int x = RenderingHelper.LayoutImageHorizontally(graphics, size, ref _textBounds, _imageAlignment, Text, textLayoutRenderer, Font);
				_imageBounds = new Rectangle(x, clientRectangle.Top, _imageSize.Width, _imageSize.Height);
				if (lineHeight > _imageBounds.Height)
				{
					_imageBounds.Height = lineHeight;
				}
				else
				{
					_textBounds.Height = _imageBounds.Height;
				}
				if (_textBounds.Width != textWidth)
				{
					switch (_textAlignment)
					{
					case TextAlignment.Center:
						_textBounds.X += (_textBounds.Width - textWidth) / 2;
						break;
					case TextAlignment.Far:
						if (RightToLeft != RightToLeft.Yes)
						{
							_textBounds.X = _textBounds.Right - textWidth;
						}
						break;
					case TextAlignment.Near:
						if (RightToLeft == RightToLeft.Yes)
						{
							_textBounds.X = _textBounds.Right - textWidth;
						}
						break;
					}
				}
				_textBounds.Width = textWidth;
				if (Text.Length == 0)
				{
					_captionBounds = _imageBounds;
				}
				else if (_imageBounds.X <= _textBounds.X)
				{
					_captionBounds = new Rectangle(_imageBounds.X, _imageBounds.Y, _textBounds.Right - _imageBounds.X, _imageBounds.Height);
				}
				else
				{
					_captionBounds = new Rectangle(_textBounds.X, _imageBounds.Y, _imageBounds.Right - _textBounds.X, _imageBounds.Height);
				}
			}
			else
			{
				_imageBounds = Rectangle.Empty;
				switch (_textAlignment)
				{
				case TextAlignment.Center:
					_textBounds.X += (_textBounds.Width - textWidth) / 2;
					break;
				case TextAlignment.Far:
					if (RightToLeft != RightToLeft.Yes)
					{
						_textBounds.X = _textBounds.Right - textWidth;
					}
					break;
				case TextAlignment.Near:
					if (RightToLeft == RightToLeft.Yes)
					{
						_textBounds.X = _textBounds.Right - textWidth;
					}
					break;
				}
				_textBounds.Width = textWidth;
				_captionBounds = _textBounds;
			}
			_isLayoutDirty = false;
		}

		private void InvalidateLayout()
		{
			_isLayoutDirty = true;
			Invalidate(invalidateChildren: true);
		}
	}
}
