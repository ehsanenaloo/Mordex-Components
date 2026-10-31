using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxButton
	{
		private void UpdateContentBounds()
		{
			_contentBounds = base.ClientRectangle;
			if ((VisualStyleSupport.IsThemingUsable(false)))
			{
				_contentBounds.Inflate(-2, -2);
				return;
			}
			switch (_appearance)
			{
			case Mordex.Windows.UI.Appearance.Empty:
			case Mordex.Windows.UI.Appearance.Flat:
			case Mordex.Windows.UI.Appearance.Light3D:
				_contentBounds.Inflate(-1, -1);
				break;
			case Mordex.Windows.UI.Appearance.Normal:
				_contentBounds.Inflate(-2, -2);
				break;
			case Mordex.Windows.UI.Appearance.FlatBorderless:
				break;
			}
		}

		internal ButtonStyle ResolveButtonStyle()
		{
			if (_buttonStyle == ButtonStyle.Default)
			{
				if (_dropDownContextMenu != null)
				{
					return ButtonStyle.DropDownButton;
				}
				return ButtonStyle.Button;
			}
			return _buttonStyle;
		}

		protected virtual Size GetImageSize()
		{
			Size result = Size.Empty;
			Icon icon = _icon;
			Image image = GetCurrentImage(false, false);
			if (icon == null && GetAssociatedCommand() != null)
			{
				icon = GetAssociatedCommand().Icon;
			}
			if (icon != null)
			{
				result = icon.Size;
			}
			else if (image != null)
			{
				result = image.Size;
			}
			else if (_imageList != null)
			{
				int imageIndex = GetImageIndex(false, false);
				if (imageIndex >= 0)
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

		private void UpdatePreferredSize(Graphics graphics, Font font, Size size, TextLayoutRenderer textLayoutRenderer)
		{
			int preferredHeight = base.Height;
			int width = base.Width;
			if (Dock != DockStyle.Right && Dock != DockStyle.Left)
			{
				preferredHeight = font.Height;
				if (size.Height > preferredHeight)
				{
					preferredHeight = size.Height;
				}
				preferredHeight += PreferredHeightPadding;
				preferredHeight += base.Padding.Top + base.Padding.Bottom;
				if (!size.IsEmpty && (ImageVerticalAlignment == ImageVerticalAlignment.BottomOfText || ImageVerticalAlignment == ImageVerticalAlignment.TopOfText))
				{
					preferredHeight += size.Height + ImageTextSpacing;
				}
			}
			if (Dock != DockStyle.Bottom && Dock != DockStyle.Top && Dock != DockStyle.Fill)
			{
				int textWidth = (int)textLayoutRenderer.MeasureText(graphics, Text, font, 0).Width;
				width = textWidth + PreferredWidthPadding;
				if (!size.IsEmpty && ImageHorizontalAlignment != ImageHorizontalAlignment.Center)
				{
					width += size.Width + ImageTextSpacing;
				}
				width += base.Padding.Left + base.Padding.Right;
			}
			_preferredSize = new Size(width, preferredHeight);
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

		private void LayoutContent(Graphics graphics, TextLayoutRenderer textLayoutRenderer, Font font)
		{
			_layoutFontKey = FontKey(font);
			Size imageSize = GetImageSize();
			UpdatePreferredSize(graphics, font, imageSize, textLayoutRenderer);
			UpdateContentBounds();
			_textBounds = _contentBounds;
			int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
			_faceBounds = base.ClientRectangle;
			if (ResolveButtonStyle() == ButtonStyle.DropDown || ResolveButtonStyle() == ButtonStyle.DropDownButton)
			{
				if (RightToLeft == RightToLeft.Yes)
				{
					_dropDownBounds = new Rectangle(_faceBounds.Left, _faceBounds.Top, verticalScrollBarWidth + 1, _faceBounds.Height);
					_textBounds = new Rectangle(_textBounds.Left + verticalScrollBarWidth, _textBounds.Top, _textBounds.Width - verticalScrollBarWidth, _textBounds.Height);
					_faceBounds.X += verticalScrollBarWidth;
					_faceBounds.Width -= verticalScrollBarWidth;
				}
				else
				{
					_dropDownBounds = new Rectangle(_faceBounds.Right - verticalScrollBarWidth - 1, _faceBounds.Top, verticalScrollBarWidth + 1, _faceBounds.Height);
					if (_dropDownBounds.X > 0)
					{
						_textBounds = new Rectangle(_textBounds.Left, _textBounds.Top, _textBounds.Width - verticalScrollBarWidth, _textBounds.Height);
					}
					else
					{
						_textBounds = Rectangle.Empty;
					}
					_faceBounds.Width -= verticalScrollBarWidth;
				}
			}
			else
			{
				_dropDownBounds = Rectangle.Empty;
			}
			if (!imageSize.IsEmpty)
			{
				int textAreaWidth = _textBounds.Width;
				if (_imageHorizontalAlignment != ImageHorizontalAlignment.Center)
				{
					textAreaWidth -= imageSize.Width;
				}
				int x = RenderingHelper.LayoutImageHorizontallyWithMaxWidth(graphics, imageSize, ref _textBounds, textAreaWidth, _imageHorizontalAlignment, ButtonText, textLayoutRenderer, font);
				int y = RenderingHelper.LayoutImageVertically(graphics, imageSize, ref _textBounds, _imageVerticalAlignment, ButtonText, font, textLayoutRenderer);
				_imageBounds = new Rectangle(x, y, imageSize.Width, imageSize.Height);
			}
			else
			{
				_imageBounds = Rectangle.Empty;
			}
			if (ButtonStyle == ButtonStyle.Ellipsis)
			{
				_textBounds.X += (_textBounds.Width - EllipsisTextWidth) / 2;
				if (_textVerticalAlignment == TextAlignment.Far)
				{
					_textBounds.Y = _textBounds.Bottom - BaselineOffsetFromBottom;
				}
				else if (_textVerticalAlignment == TextAlignment.Near)
				{
					_textBounds.Y += EllipsisNearOffset;
				}
				else
				{
					_textBounds.Y += (_textBounds.Height - EllipsisDotSize) / 2;
				}
			}
			_isLayoutDirty = false;
		}

		private static (string Name, float Size, FontStyle Style, GraphicsUnit Unit) FontKey(Font font)
		{
			return (font.Name, font.Size, font.Style, font.Unit);
		}

		private TextLayoutRenderer CreateTextLayoutRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			switch (TextHorizontalAlignment)
			{
			case TextAlignment.Empty:
			case TextAlignment.Center:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				break;
			case TextAlignment.Near:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				break;
			case TextAlignment.Far:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
				break;
			}
			switch (TextVerticalAlignment)
			{
			case TextAlignment.Empty:
			case TextAlignment.Center:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				break;
			case TextAlignment.Near:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Near);
				break;
			case TextAlignment.Far:
				textLayoutRenderer.SetLineAlignment(StringAlignment.Far);
				break;
			}
			textLayoutRenderer.SetRightToLeft(RightToLeft);
			if (!AutoSize)
			{
				textLayoutRenderer.SetWordWrap(WordWrap);
			}
			if (UseMnemonic)
			{
				textLayoutRenderer.SetShowHotkeyPrefix(true);
			}
			if (!_wordWrap || AutoSize)
			{
				textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			}
			return textLayoutRenderer;
		}

		private void UpdateToolTip(string caption)
		{
			if (caption == null || caption.Length == 0)
			{
				DisposeResources();
				return;
			}
			if (VisualStyleSupport.IsToolTipHookSupported())
			{
				if (_customToolTip == null)
				{
					_customToolTip = new WindowMessageHandler(Font, UseCompatibleTextRendering);
					_customToolTip.RequestShow(caption, this);
				}
				else
				{
					_customToolTip.UpdateTipText(caption);
				}
				return;
			}
			if (_toolTip == null)
			{
				_toolTip = new Mordex.Windows.Common.ThemedToolTip();
				_toolTip.ShowAlways = true;
			}
			_toolTip.SetToolTip(this, caption);
		}
	}
}
