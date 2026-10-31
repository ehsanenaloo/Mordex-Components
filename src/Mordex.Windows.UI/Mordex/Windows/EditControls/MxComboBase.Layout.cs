using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBase
	{
		protected override void OnResize(EventArgs re)
		{
			base.OnResize(re);
			InvalidateLayout();
		}

		private int GetBorderThickness()
		{
			if (GetEffectiveBorderStyle() == Mordex.Windows.UI.BorderStyle.None)
			{
				return 0;
			}
			return 2;
		}

		private int GetDisplayInset()
		{
			if (GetEffectiveBorderStyle() != Mordex.Windows.UI.BorderStyle.None)
			{
				return 1;
			}
			return GetBorderThickness();
		}

		private Image GetCurrentImage()
		{
			if (_hasImage)
			{
				if (_itemImage != null)
				{
					return _itemImage;
				}
				if (_imageList != null)
				{
					if (GetImageKey().Length > 0 && _imageList.Images.ContainsKey(GetImageKey()))
					{
						return _imageList.Images[_imageKey];
					}
					if (GetImageIndex() >= 0 && GetImageIndex() < _imageList.Images.Count)
					{
						return _imageList.Images[_imageIndex];
					}
				}
			}
			return null;
		}

		internal void ApplyAutoHeight()
		{
			int lineHeight = (int)Font.GetHeight() + FontHeightPadding;
			if (!_autoSize || Dock == DockStyle.Left || Dock == DockStyle.Right || Dock == DockStyle.Fill)
			{
				return;
			}
			int requiredHeight = lineHeight;
			if (HasImage)
			{
				if (_itemImage != null)
				{
					int imageHeight = _itemImage.Size.Height;
					if (_imageVerticalAlignment == ImageVerticalAlignment.BottomOfText || _imageVerticalAlignment == ImageVerticalAlignment.TopOfText)
					{
						imageHeight += lineHeight + 1;
					}
					if (imageHeight > requiredHeight)
					{
						requiredHeight = imageHeight;
					}
				}
				else if (_imageList != null)
				{
					int imageListHeight = _imageList.ImageSize.Height;
					if (_imageVerticalAlignment == ImageVerticalAlignment.BottomOfText || _imageVerticalAlignment == ImageVerticalAlignment.TopOfText)
					{
						imageListHeight += lineHeight + 1;
					}
					if (imageListHeight > requiredHeight)
					{
						requiredHeight = imageListHeight;
					}
				}
			}
			requiredHeight += GetBorderThickness() * 2 + 2;
			if (GetBorderThickness() == 0)
			{
				requiredHeight += 2;
			}
			if (base.Height != requiredHeight)
			{
				base.Height = requiredHeight;
			}
		}

		internal void LayoutContent(Graphics targetGraphics)
		{
			if ((!base.IsHandleCreated && base.Disposing) || base.IsDisposed)
			{
				return;
			}
			Graphics graphics = null;
			if (targetGraphics == null)
			{
				if (!base.Created)
				{
					return;
				}
				graphics = CreateGraphics();
				targetGraphics = graphics;
			}
			int lineHeight = (int)Font.GetHeight() + FontHeightPadding;
			Size size = new Size(0, 0);
			Image image = GetCurrentImage();
			if (image != null)
			{
				size = image.Size;
			}
			Rectangle displayRectangle = DisplayRectangle;
			int buttonWidth = SystemInformation.VerticalScrollBarWidth;
			if (!VisualStyleSupport.IsThemingUsable(false))
			{
				buttonWidth = ClassicButtonWidth;
			}
			int leftInset = ((!UsesNativeTheme()) ? 1 : 2);
			if (RightToLeft == RightToLeft.Yes)
			{
				_windowsLayoutState._buttonBounds = new Rectangle(displayRectangle.Left, displayRectangle.Top, buttonWidth, displayRectangle.Height);
				_textBounds = new Rectangle(displayRectangle.Left + 3 + buttonWidth + leftInset, displayRectangle.Top, displayRectangle.Width - buttonWidth - 5, displayRectangle.Height);
			}
			else
			{
				_windowsLayoutState._buttonBounds = new Rectangle(displayRectangle.Right - buttonWidth, displayRectangle.Top, buttonWidth, displayRectangle.Height);
				_textBounds = new Rectangle(displayRectangle.Left + leftInset, displayRectangle.Top, displayRectangle.Width - buttonWidth - 4, displayRectangle.Height);
			}
			_windowsLayoutState._isButtonEnabled = base.Enabled;
			_windowsLayoutState._isButtonVisible = true;
			if (image != null)
			{
				Rectangle rectangle = _textBounds;
				bool ownsGraphics = false;
				if (targetGraphics == null)
				{
					targetGraphics = CreateGraphics();
					ownsGraphics = true;
				}
				int textAreaWidth = rectangle.Width;
				if (_imageHorizontalAlignment != ImageHorizontalAlignment.Center)
				{
					textAreaWidth -= size.Width;
				}
				int x = RenderingHelper.LayoutImageHorizontallyWithMaxWidth(targetGraphics, size, ref rectangle, textAreaWidth, _imageHorizontalAlignment, Text, GetOrCreateTextLayoutRenderer(), Font);
				int y = RenderingHelper.LayoutImageVertically(targetGraphics, size, ref rectangle, _imageVerticalAlignment, Text, Font, GetOrCreateTextLayoutRenderer());
				if (ownsGraphics)
				{
					targetGraphics.Dispose();
					targetGraphics = null;
				}
				if (rectangle.Width > textAreaWidth)
				{
					rectangle.Width = textAreaWidth;
				}
				else if (rectangle.Width != textAreaWidth && TextAlignment == TextAlignment.Near && _imageHorizontalAlignment == ImageHorizontalAlignment.BeforeText)
				{
					if (RightToLeft == RightToLeft.Yes)
					{
						rectangle.X -= textAreaWidth - rectangle.Width;
					}
					rectangle.Width = textAreaWidth;
				}
				_imageBounds = new Rectangle(x, y, size.Width, size.Height);
				_textBounds = new Rectangle(rectangle.Left, rectangle.Top + (rectangle.Height - lineHeight) / 2, rectangle.Width, lineHeight);
			}
			else
			{
				_imageBounds = Rectangle.Empty;
				_textBounds = new Rectangle(_textBounds.Left, _textBounds.Top + (_textBounds.Height - lineHeight) / 2, _textBounds.Width, lineHeight);
			}
			int textHeight = (int)GetOrCreateTextLayoutRenderer().MeasureText(targetGraphics, Text, Font, _textBounds.Width).Height;
			if (textHeight > 0)
			{
				_baselineOffset = (DisplayRectangle.Height - textHeight) / 2 + textHeight + 1;
			}
			else
			{
				textHeight = (int)GetOrCreateTextLayoutRenderer().MeasureText(targetGraphics, "A", Font, _textBounds.Width).Height;
				_baselineOffset = (DisplayRectangle.Height - textHeight) / 2 + textHeight + 1;
				textHeight = 0;
			}
			if (_textBox != null)
			{
				_textBox.AutoSize = false;
				if (RightToLeft != RightToLeft.Yes)
				{
					_textBounds.X++;
				}
				_textBounds.Width--;
				_textBox.Bounds = _textBounds;
				switch (_textAlignment)
				{
				case TextAlignment.Center:
					_textBox.TextAlign = HorizontalAlignment.Center;
					break;
				case TextAlignment.Far:
					_textBox.TextAlign = HorizontalAlignment.Right;
					break;
				default:
					_textBox.TextAlign = HorizontalAlignment.Left;
					break;
				}
				if (!UsesNativeTheme())
				{
					_textBox.Visible = base.Enabled && _hasText;
				}
			}
			_isLayoutDirty = false;
			graphics?.Dispose();
		}

		internal void InvalidateLayout()
		{
			_isLayoutDirty = true;
			if (_formatStyleRenderer != null && _formatStyleRenderer.GetIsDropDownOpen())
			{
				_formatStyleRenderer.FitToItems(base.ClientRectangle.Width);
			}
			if (!_isDisposing)
			{
				ApplyAutoHeight();
			}
			Invalidate();
		}
	}
}
