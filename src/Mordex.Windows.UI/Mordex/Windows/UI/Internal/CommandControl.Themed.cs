using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal partial class CommandControl
	{
		private const int ThemedRowInsetX = 3;

		private const int ThemedRowInsetY = 1;

		private const int ThemedSeparatorGap = 6;

		private const int ThemedDescriptionInset = 3;

		private const int ThemedGripDots = 5;

		// Paints a pop-up menu from the default theme: raised surface, border, rows with hot and pressed fills, check
		// marks, shortcut text in the muted color. Colors that a state style sets still win over the theme.
		private void PaintThemed(PaintEventArgs eventArgs, TextLayoutRenderer textLayoutRenderer)
		{
			Graphics graphics = eventArgs.Graphics;
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			FormatStyle baseStyle = new FormatStyle(_baseSettings.StateStyles.FormatStyle);
			Color back = baseStyle.BackColor.IsEmpty ? theme.MenuBackColor : baseStyle.BackColor;
			if (baseStyle.ForeColor.IsEmpty)
			{
				baseStyle.ForeColor = theme.Color(ThemeRole.Text);
			}
			graphics.FillRectangle(GdiCache.Brush(back), _menuClientBounds);
			if (theme.Style.MenuGutter && theme.Style.Detail.MenuLook != ThemeMenuLook.Classic)
			{
				DrawThemedGutter(graphics, theme);
			}
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			foreach (CommandLayoutState item in _layoutItems)
			{
				if (item._isOverflowItem || !((FormatStyleLayoutState)item).IsVisible() || (_isExpanding && item.GetCommand().IsInfrequent()) || Rectangle.Intersect(eventArgs.ClipRectangle, item._bounds).IsEmpty)
				{
					continue;
				}
				if (item.GetCommand().IsSeparator())
				{
					DrawThemedSeparator(graphics, theme, item._bounds);
				}
				else
				{
					DrawThemedRow(graphics, theme, item, baseStyle, textLayoutRenderer, back);
				}
			}
			DrawThemedTearOff(graphics, theme, eventArgs.ClipRectangle);
			DrawThemedButton(graphics, theme, _expandBar, baseStyle, 1, eventArgs.ClipRectangle);
			DrawThemedButton(graphics, theme, _scrollUpButton, baseStyle, -1, eventArgs.ClipRectangle);
			DrawThemedButton(graphics, theme, _scrollDownButton, baseStyle, 1, eventArgs.ClipRectangle);
			graphics.SmoothingMode = smoothing;
			if (!theme.DrawMenuFrame(graphics, _menuClientBounds))
			{
				graphics.DrawRectangle(theme.Pen(ThemeRole.Border), _menuClientBounds.X, _menuClientBounds.Y, _menuClientBounds.Width - 1, _menuClientBounds.Height - 1);
			}
			if (_isOverflowMenu && _overflowItems != null && _overflowItems.Count > 0)
			{
				foreach (CommandLayoutState item in _overflowItems)
				{
					item._ownerCommandBar.DrawItem(graphics, null, item, item._ownerCommandBar.BuildCommandStateStyles(), textLayoutRenderer, eventArgs.ClipRectangle);
				}
			}
			if (GetCustomizeItem() != null)
			{
				DrawCustomizeFrame(graphics, GetCustomizeItem()._bounds);
			}
			baseStyle.DisposeResources();
			_baseSettings.DisposeStyleResources();
			textLayoutRenderer.ResetCache();
		}

		// The band behind the images and check marks, as far right as the widest image column of the visible rows.
		private void DrawThemedGutter(Graphics graphics, ResolvedTheme theme)
		{
			int right = 0;
			foreach (CommandLayoutState item in _layoutItems)
			{
				if (item._isOverflowItem || item.GetCommand().IsSeparator() || !((FormatStyleLayoutState)item).IsVisible())
				{
					continue;
				}
				right = Math.Max(right, Math.Max(item._imageBounds.IsEmpty ? 0 : item._imageBounds.Right, item._checkBounds.IsEmpty ? 0 : item._checkBounds.Right));
			}
			if (right <= 0)
			{
				return;
			}
			Rectangle gutter = new Rectangle(_menuClientBounds.X + 1, _menuClientBounds.Y + 1, Math.Min(right + theme.Scaled(ThemedRowInsetX + 3), _menuClientBounds.Width - 2) - 1, _menuClientBounds.Height - 2);
			if (theme.DrawMenuGutter(graphics, gutter))
			{
				return;
			}
			theme.FillBand(graphics, gutter, ThemeRole.SurfaceAlt, LinearGradientMode.Horizontal);
			graphics.DrawLine(theme.Pen(ThemeRole.GridLine), gutter.Right, gutter.Top, gutter.Right, gutter.Bottom - 1);
		}

		// The bar at the top of a pop-up that can be torn off: a flat strip with a row of grip dots, lit when hot or pressed.
		private void DrawThemedTearOff(Graphics graphics, ResolvedTheme theme, Rectangle clip)
		{
			if (_tearOffBar == null || Rectangle.Intersect(clip, _tearOffBar._bounds).IsEmpty)
			{
				return;
			}
			Rectangle bar = _tearOffBar._bounds;
			bar.Inflate(-theme.Scaled(ThemedRowInsetX), -theme.Scaled(ThemedRowInsetY));
			bool lit = _tearOffBar.IsPressed() || _tearOffBar.IsHot();
			graphics.FillRectangle(theme.Brush(_tearOffBar.IsPressed() ? ThemeRole.SurfacePressed : lit ? ThemeRole.SurfaceHot : ThemeRole.SurfaceAlt), bar);
			int dot = Math.Max(2, theme.Scaled(2));
			int pitch = dot * 2;
			int left = bar.X + (bar.Width - (ThemedGripDots * pitch - dot)) / 2;
			int top = bar.Y + (bar.Height - dot) / 2;
			Brush dots = theme.Brush(lit ? ThemeRole.Text : ThemeRole.TextMuted);
			for (int i = 0; i < ThemedGripDots; i++)
			{
				graphics.FillRectangle(dots, left + i * pitch, top, dot, dot);
			}
		}

		private void DrawThemedSeparator(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			int y = bounds.Y + bounds.Height / 2;
			int gap = theme.Scaled(ThemedSeparatorGap);
			graphics.DrawLine(theme.Pen(ThemeRole.Border), bounds.Left + gap, y, bounds.Right - gap - 1, y);
		}

		private static void FillThemedState(Graphics graphics, ResolvedTheme theme, Color custom, ThemeRole role, Rectangle bounds)
		{
			if (custom.IsEmpty || custom.ToArgb() == theme.Color(role).ToArgb())
			{
				theme.FillRounded(graphics, bounds, role, theme.Radius(ThemeElement.Menu));
			}
			else
			{
				graphics.FillRectangle(GdiCache.Brush(custom), bounds);
			}
		}

		private void DrawThemedButton(Graphics graphics, ResolvedTheme theme, FormatStyleLayoutState button, FormatStyle baseStyle, int direction, Rectangle clip)
		{
			if (button == null || Rectangle.Intersect(clip, button._bounds).IsEmpty)
			{
				return;
			}
			Rectangle bounds = button._bounds;
			bounds.Inflate(-theme.Scaled(ThemedRowInsetX), -theme.Scaled(ThemedRowInsetY));
			if (button.IsPressed() || button.IsHot())
			{
				theme.FillRounded(graphics, bounds, button.IsPressed() ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, theme.Radius(ThemeElement.Menu));
			}
			RenderingHelper.DrawUpDownTriangle(graphics, OverflowArrowGlyphSize, button._bounds, direction, GdiCache.Pen(baseStyle.ForeColor));
		}

		private void DrawThemedRow(Graphics graphics, ResolvedTheme theme, CommandLayoutState item, FormatStyle baseStyle, TextLayoutRenderer textLayoutRenderer, Color back)
		{
			MxCommand command = item.GetCommand();
			bool enabled = ((FormatStyleLayoutState)item).IsEnabled();
			bool pressed = ((FormatStyleLayoutState)item).IsPressed();
			bool hot = ((FormatStyleLayoutState)item).IsHot() || item == _openItem;
			bool isChecked = command.IsChecked;
			if (_isOverflowMenu)
			{
				DrawOverflowItem(graphics, item, textLayoutRenderer, baseStyle, baseStyle);
				return;
			}
			FormatStyle style = GetItemFormatStyle(item, baseStyle, pressed, hot, false, isChecked, false);
			Rectangle row = item._bounds;
			row.Inflate(-theme.Scaled(ThemedRowInsetX), -theme.Scaled(ThemedRowInsetY));
			bool selected = enabled && (hot || pressed);
			bool olderSelection = selected && theme.DrawMenuSelection(graphics, row);
			if (selected && !olderSelection)
			{
				FillThemedState(graphics, theme, style.BackColor, pressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, row);
			}
			Color fore = style.ForeColor.IsEmpty ? theme.Color(ThemeRole.Text) : style.ForeColor;
			if (olderSelection && !theme.MenuSelectionText.IsEmpty)
			{
				fore = theme.MenuSelectionText;
			}
			Color muted = enabled ? theme.Color(ThemeRole.TextMuted) : theme.Color(ThemeRole.TextDisabled);
			if (olderSelection && !theme.MenuSelectionText.IsEmpty)
			{
				muted = fore;
			}
			if (!enabled)
			{
				fore = theme.Color(ThemeRole.TextDisabled);
			}
			Rectangle imageBounds = item._imageBounds;
			bool hasImage = DrawThemedImage(graphics, theme, item, ref imageBounds, enabled, hot || pressed, back, isChecked && !_hasCheckMargin);
			Rectangle text = item._textBounds;
			Rectangle description = Rectangle.Empty;
			bool hasDescription = ResolvePopupView() == Mordex.Windows.UI.CommandBars.View.LargeIcons && command.Description.Length > 0;
			if (hasDescription)
			{
				text = new Rectangle(text.X, text.Y + ThemedDescriptionInset, text.Width, text.Height / 3);
				description = new Rectangle(text.X, text.Bottom, text.Width, item._bounds.Height - text.Height - ThemedDescriptionInset);
			}
			if (command.IsDefaultItem() && enabled)
			{
				style.FontBold = TriState.True;
			}
			Font font = style.ResolveFont(Font);
			if (RightToLeft == RightToLeft.Yes)
			{
				textLayoutRenderer.SetRightToLeft(RightToLeft.Yes);
			}
			ApplyTextAlignment(ref textLayoutRenderer, command);
			textLayoutRenderer.DrawTextWithBrush(graphics, item._caption, text, font, GdiCache.Brush(fore));
			string shortcut = item.GetShortcutText();
			if (shortcut != "")
			{
				textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
				textLayoutRenderer.DrawTextWithBrush(graphics, shortcut, text, font, GdiCache.Brush(muted));
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			}
			if (hasDescription)
			{
				textLayoutRenderer.SetWordWrap(true);
				textLayoutRenderer.SetLineAlignment(StringAlignment.Near);
				textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
				textLayoutRenderer.DrawTextWithBrush(graphics, command.Description, description, font, GdiCache.Brush(muted));
				textLayoutRenderer.SetWordWrap(false);
				textLayoutRenderer.SetStringTrimming(StringTrimming.None);
			}
			if (command.IsParent)
			{
				Rectangle arrow = new Rectangle(item._bounds.Right - SubmenuArrowRightMargin, item._bounds.Y, SubmenuArrowWidth, item._bounds.Height);
				RenderingHelper.DrawSideTriangle(graphics, SubmenuArrowGlyphSize, arrow, 1, GdiCache.Pen(muted));
			}
			if (isChecked && _hasCheckMargin)
			{
				DrawThemedCheck(graphics, theme, item._checkBounds, enabled ? fore : muted);
			}
			else if (isChecked && !hasImage)
			{
				DrawThemedCheck(graphics, theme, imageBounds, enabled ? fore : muted);
			}
			style.DisposeResources();
		}

		private static void DrawThemedCheck(Graphics graphics, ResolvedTheme theme, Rectangle bounds, Color color)
		{
			int size = Math.Min(theme.Scaled(10), Math.Min(bounds.Width, bounds.Height));
			int x = bounds.X + (bounds.Width - size) / 2;
			int y = bounds.Y + (bounds.Height - size) / 2;
			if (theme.Style.Check != ThemeCheckGlyph.Default)
			{
				theme.DrawCheck(graphics, new Rectangle(x, y, size, size), color, theme.Style.Check);
				return;
			}
			Pen pen = GdiCache.Pen(color, Math.Max(1.5f, theme.Scale * 1.5f));
			graphics.DrawLines(pen, new[]
			{
				new PointF(x + size * 0.1f, y + size * 0.55f),
				new PointF(x + size * 0.4f, y + size * 0.85f),
				new PointF(x + size * 0.92f, y + size * 0.15f)
			});
		}

		// Draws the image of an item and, when the item is checked and has an image, a frame around it. Returns whether an image was drawn.
		private bool DrawThemedImage(Graphics graphics, ResolvedTheme theme, CommandLayoutState item, ref Rectangle imageBounds, bool enabled, bool highlighted, Color back, bool frame)
		{
			MxCommand command = item.GetCommand();
			if (item._hasGroupLine)
			{
				imageBounds.Y++;
				imageBounds.Height--;
			}
			Mordex.Windows.UI.CommandBars.View view = ResolvePopupView();
			if (view == Mordex.Windows.UI.CommandBars.View.LargeIcons && _useSmallImages)
			{
				view = Mordex.Windows.UI.CommandBars.View.SmallIcons;
			}
			Icon icon = command.GetEffectiveIcon();
			ImageList imageList = null;
			Image image = null;
			int imageIndex = -1;
			if (icon == null)
			{
				imageList = GetImageList(command);
				image = RenderingHelper.GetCommandImage(imageList, item, view);
				imageIndex = RenderingHelper.FindCommandImageIndex(imageList, item, view);
				if (image == null)
				{
					return false;
				}
			}
			Size size = GetImageSize(ResolvePopupView(), command);
			imageBounds.X += (imageBounds.Width - size.Width) / 2;
			imageBounds.Y += (imageBounds.Height - size.Height) / 2;
			imageBounds.Size = size;
			if (frame)
			{
				Rectangle box = imageBounds;
				box.Inflate(theme.Scaled(3), theme.Scaled(3));
				theme.FillRounded(graphics, box, ThemeRole.AccentSubtle, theme.Radius(ThemeElement.Menu));
				theme.DrawRounded(graphics, box, ThemeRole.Accent, theme.Radius(ThemeElement.Menu));
			}
			ImageAttributes attributes = null;
			if (!command.GetEffectiveImageReplaceableColor().IsEmpty && command.CommandType == CommandType.ColorPickerCommand)
			{
				attributes = new ImageAttributes();
				ColorMap map = new ColorMap { NewColor = (Color)command.ControlValue, OldColor = command.GetEffectiveImageReplaceableColor() };
				attributes.SetRemapTable(new[] { map }, ColorAdjustType.Bitmap);
			}
			RenderingHelper.DrawStateImage(graphics, icon, image, imageIndex, imageList, imageBounds, enabled, highlighted, false, back, false, attributes);
			attributes?.Dispose();
			return true;
		}
	}
}
