using System.ComponentModel;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBase
	{
		private ThemeHook _themeHook;

		private bool IsThemed => true;

		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				if (!IsThemed)
				{
					return;
				}
				_themeHook = new ThemeHook(this, ApplyThemedTextBoxColors);
			}
			_themeHook.Update(IsThemed);
		}

		private void ApplyThemedTextBoxColors()
		{
			if (_textBox != null)
			{
				_textBox.BackColor = GetEffectiveBackColor();
				_textBox.ForeColor = GetEffectiveForeColor();
				ThemeWindow.ApplyNativeLook(_textBox, MxThemeManager.DefaultTheme);
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
			if (IsThemed)
			{
				ApplyThemedTextBoxColors();
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}

		// Paints the edit frame, the drop-down button and the content from the default theme. The layout has been made by the caller.
		private void PaintThemed(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool enabled = base.Enabled;
			bool dropped = DroppedDown;
			bool buttonHot = enabled && IsButtonHot();
			bool buttonPressed = enabled && (dropped || IsButtonPressed());
			bool hot = enabled && (buttonHot || IsMouseOver());
			bool focused = enabled && (base.ContainsFocus || dropped);
			Rectangle bounds = base.ClientRectangle;
			int radius = theme.Radius(ThemeElement.Input);
			ThemedBackdrop.Paint(graphics, bounds, this);
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (!enabled)
			{
				theme.FillRounded(graphics, bounds, ThemeRole.InputBackDisabled, radius);
			}
			else if (BackColor == SystemColors.Window)
			{
				theme.FillRounded(graphics, bounds, ThemeRole.InputBack, radius);
			}
			else
			{
				graphics.FillRectangle(GdiCache.Brush(BackColor), bounds);
			}
			Rectangle button = _windowsLayoutState._buttonBounds;
			if (!button.IsEmpty)
			{
				Rectangle face = Rectangle.Inflate(button, -1, -2);
				bool buttonDrawn = theme.DrawDropButton(graphics, button, enabled, buttonHot, buttonPressed);
				if (buttonDrawn)
				{
				}
				else if (theme.Style.ScrollBar.Custom && theme.Style.ScrollBar.ButtonFaces && enabled)
				{
					// An older look gives the drop-down button a face of its own.
					face = Rectangle.Inflate(button, -2, -2);
					theme.FillFace(graphics, face, buttonPressed ? ThemeRole.SurfacePressed : buttonHot ? ThemeRole.SurfaceHot : theme.Style.ButtonLook == ThemeButtonLook.Luna ? ThemeRole.AccentSubtle : ThemeRole.SurfaceRaised, Math.Min(theme.Radius(ThemeElement.Button), 3), LinearGradientMode.Vertical);
					theme.DrawBorder(graphics, face, theme.Style.ScrollBar.Edge == ThemeBorder.Raised ? (buttonPressed ? ThemeBorder.Sunken : ThemeBorder.Raised) : ThemeBorder.Flat, buttonHot || buttonPressed ? ThemeRole.BorderHot : ThemeRole.Border, Math.Min(theme.Radius(ThemeElement.Button), 3));
				}
				else if (buttonPressed || buttonHot)
				{
					theme.FillRounded(graphics, face, buttonPressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, Math.Max(0, radius - 1));
				}
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				if (buttonDrawn)
				{
				}
				else if (theme.Style.Arrow != ThemeArrowGlyph.Default)
				{
					theme.DrawArrow(graphics, button, 1, enabled ? ThemeRole.Text : ThemeRole.TextDisabled, ThemeArrowGlyph.Chevron);
				}
				else
				{
					Pen arrow = theme.Pen(enabled ? ThemeRole.Text : ThemeRole.TextDisabled);
					float cx = button.X + button.Width / 2f;
					float cy = button.Y + button.Height / 2f;
					float s = Math.Max(2f, theme.Scaled(3));
					graphics.DrawLine(arrow, cx - s, cy - s / 2f, cx, cy + s / 2f);
					graphics.DrawLine(arrow, cx, cy + s / 2f, cx + s, cy - s / 2f);
				}
			}
			if (BorderStyle != Mordex.Windows.UI.BorderStyle.None || hot || focused)
			{
				ThemeRole border = !enabled ? ThemeRole.BorderDisabled : focused ? ThemeRole.BorderFocus : hot ? ThemeRole.BorderHot : ThemeRole.Border;
				if (border == ThemeRole.Border)
				{
					theme.DrawBorder(graphics, bounds, theme.Style.InputBorder, border, radius);
				}
				else
				{
					theme.DrawInputFrame(graphics, bounds, border, radius, focused);
				}
			}
			graphics.SmoothingMode = smoothing;
			PaintThemedContent(graphics, theme, enabled);
		}

		private void PaintThemedContent(Graphics graphics, ResolvedTheme theme, bool enabled)
		{
			if (_hasImage && !_imageBounds.IsEmpty)
			{
				Image image = GetImage();
				int imageIndex = -1;
				if (image == null && _imageList != null)
				{
					if (_imageKey.Length > 0 && _imageList.Images.ContainsKey(_imageKey))
					{
						image = _imageList.Images[_imageKey];
						imageIndex = _imageList.Images.IndexOfKey(_imageKey);
					}
					else if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
					{
						image = _imageList.Images[_imageIndex];
						imageIndex = _imageIndex;
					}
				}
				if (image != null)
				{
					RenderingHelper.DrawStateImage(graphics, null, image, imageIndex, _imageList, _imageBounds, enabled, false, false, GetEffectiveBackColor(), false, null);
				}
			}
			if (!HasText || _textBounds.IsEmpty || (_textBox != null && _textBox.Visible))
			{
				return;
			}
			string text = Text;
			if (text.Length == 0)
			{
				return;
			}
			TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
			flags |= RightToLeft == RightToLeft.Yes ? TextFormatFlags.Right | TextFormatFlags.RightToLeft : TextFormatFlags.Left;
			Color color = enabled ? GetEffectiveForeColor() : theme.Color(ThemeRole.TextDisabled);
			TextRenderer.DrawText(graphics, text, Font, _textBounds, color, flags);
		}
	}
}
