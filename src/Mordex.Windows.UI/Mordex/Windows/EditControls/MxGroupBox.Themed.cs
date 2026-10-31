using System.ComponentModel;
using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxGroupBox
	{
		private ThemeHook _themeHook;

		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				_themeHook = new ThemeHook(this);
			}
			_themeHook.Update(true);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}

		private Color ThemedBackColor(ResolvedTheme theme)
		{
			return BackColor == SystemColors.Control ? theme.Color(ThemeRole.Surface) : BackColor;
		}

		private void PaintThemedBackground(Graphics graphics)
		{
			if (base.ClientRectangle.Width == 0 || base.ClientRectangle.Height == 0)
			{
				return;
			}
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			graphics.FillRectangle(GdiCache.Brush(ThemedBackColor(theme)), base.ClientRectangle);
		}

		// Paints the frame, the caption image and the caption text from the default theme.
		private void PaintThemed(Graphics graphics)
		{
			if (base.ClientRectangle.Width == 0 || base.ClientRectangle.Height == 0)
			{
				return;
			}
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool enabled = base.Enabled;
			if (_isLayoutDirty)
			{
				TextLayoutRenderer layoutRenderer = CreateTextLayoutRenderer();
				FormatStyle style = CreateBaseFormatStyle();
				style = style.MergeStyle(_formatStyle);
				LayoutCaption(graphics, layoutRenderer, style);
				layoutRenderer.ResetCache();
				style.DisposeResources();
			}
			TextAlignment alignment = _textAlignment;
			if (RightToLeft == RightToLeft.Yes)
			{
				alignment = alignment == TextAlignment.Far ? TextAlignment.Near : alignment == TextAlignment.Near ? TextAlignment.Far : alignment;
			}
			Color back = ThemedBackColor(theme);
			if (_frameStyle != FrameStyle.None)
			{
				Rectangle frame = new Rectangle(0, FrameTopGap, base.ClientRectangle.Width, base.ClientRectangle.Height - FrameTopGap);
				Pen pen = !_borderColor.IsEmpty ? GdiCache.Pen(_borderColor) : theme.Pen(enabled ? ThemeRole.Border : ThemeRole.BorderDisabled);
				bool hasCaption = !_captionBounds.IsEmpty && (Text.Length > 0 || !_imageBounds.IsEmpty);
				if (_frameStyle == FrameStyle.Top)
				{
					int y = frame.Y;
					int left = hasCaption && alignment != TextAlignment.Near ? _captionBounds.X - theme.Scaled(3) : frame.Right;
					int right = hasCaption && alignment != TextAlignment.Far ? _captionBounds.Right + theme.Scaled(3) : frame.X;
					if (alignment != TextAlignment.Near || !hasCaption)
					{
						graphics.DrawLine(pen, frame.X, y, Math.Min(left, frame.Right - 1), y);
					}
					if (alignment != TextAlignment.Far || !hasCaption)
					{
						graphics.DrawLine(pen, Math.Max(right, frame.X), y, frame.Right - 1, y);
					}
				}
				else
				{
					int radius = theme.Radius(ThemeElement.Panel);
					Color groupFrame = theme.Style.Detail.GroupFrame;
					if (_borderColor.IsEmpty && enabled && !groupFrame.IsEmpty && theme.Style.PanelBorder == ThemeBorder.Flat)
					{
						theme.DrawRounded(graphics, frame, groupFrame, radius);
					}
					else if (_borderColor.IsEmpty)
					{
						theme.DrawBorder(graphics, frame, theme.Style.PanelBorder, enabled ? ThemeRole.Border : ThemeRole.BorderDisabled, radius);
					}
					else
					{
						graphics.DrawRectangle(pen, frame.X, frame.Y, frame.Width - 1, frame.Height - 1);
					}
					if (hasCaption)
					{
						int pad = theme.Scaled(3);
						Rectangle gap = new Rectangle(_captionBounds.X - pad, frame.Y - 1, _captionBounds.Width + pad * 2, 3);
						graphics.FillRectangle(GdiCache.Brush(back), gap);
					}
				}
			}
			if (!_imageBounds.IsEmpty)
			{
				PaintThemedImage(graphics, back);
			}
			if (Text.Length > 0 && !_textBounds.IsEmpty)
			{
				Color fore = !enabled ? theme.Color(ThemeRole.TextDisabled) : ForeColor == SystemColors.ControlText || ForeColor == theme.Color(ThemeRole.Text) ? theme.GroupTitleColor : ForeColor;
				TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
				flags |= RightToLeft == RightToLeft.Yes ? TextFormatFlags.RightToLeft | TextFormatFlags.Right : TextFormatFlags.Left;
				// The layout measures with GDI+; GDI gets a little more room so the text is not cut with a false ellipsis.
				Rectangle area = Rectangle.Inflate(_textBounds, theme.Scaled(4), 0);
				TextRenderer.DrawText(graphics, Text, Font, area, fore, flags);
			}
		}

		private void PaintThemedImage(Graphics graphics, Color back)
		{
			Size size = GetContentImageSize();
			Image image = null;
			int imageIndex = -1;
			if (_displayImage != null)
			{
				image = _displayImage;
			}
			else if (ImageList != null)
			{
				if (_imageKey.Length > 0 && _imageList.Images.ContainsKey(_imageKey))
				{
					image = ImageList.Images[_imageKey];
					imageIndex = ImageList.Images.IndexOfKey(_imageKey);
				}
				else if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
				{
					image = ImageList.Images[_imageIndex];
					imageIndex = ImageIndex;
				}
			}
			Rectangle bounds = _imageBounds;
			bounds.X += (bounds.Width - size.Width) / 2;
			bounds.Y += (bounds.Height - size.Height) / 2;
			bounds.Size = size;
			RenderingHelper.DrawStateImage(graphics, Icon, image, imageIndex, ImageList, bounds, base.Enabled, false, false, back, true, null);
		}
	}
}
