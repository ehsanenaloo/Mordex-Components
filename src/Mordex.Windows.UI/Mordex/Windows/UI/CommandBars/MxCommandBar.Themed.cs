using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		private const int ThemedSeparatorInset = 4;

		private ThemeHook _themeHook;

		// Called from paint, so the bar listens to the default theme exactly while it paints with it.
		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				_themeHook = new ThemeHook(this);
			}
			_themeHook.Update(true);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}

		private Color ResolveThemedBarBack(ResolvedTheme theme)
		{
			Color color = FormatStyle.BackColor;
			if (color.IsEmpty && GetHost() != null)
			{
				color = GetHost().CommandBarsFormatStyle.BackColor;
			}
			if (color.IsEmpty)
			{
				color = CommandManager.CommandBarsFormatStyle.BackColor;
			}
			return color.IsEmpty ? theme.Color(ThemeRole.ToolbarBack) : color;
		}

		private void PaintThemed(PaintEventArgs e, TextLayoutRenderer textLayoutRenderer, StateStyles baseStyles)
		{
			Graphics graphics = e.Graphics;
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			Rectangle client = base.ClientRectangle;
			_barBackColor = ResolveThemedBarBack(theme);
			if (_barBackColor.ToArgb() == theme.Color(ThemeRole.ToolbarBack).ToArgb())
			{
				theme.FillBand(graphics, client, ThemeRole.ToolbarBack, _isHorizontal ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal);
			}
			else
			{
				graphics.FillRectangle(GdiCache.Brush(_barBackColor), client);
			}
			if (GetFloatingWindow() != null)
			{
				graphics.DrawRectangle(theme.Pen(ThemeRole.BorderStrong), client.X, client.Y, client.Width - 1, client.Height - 1);
			}
			else if (theme.Style.Bevel == ThemeBevel.Raised)
			{
				theme.DrawThinRaised(graphics, client);
			}
			if (_mdiButtons != null)
			{
				Rectangle mdiArea = Rectangle.Intersect(e.ClipRectangle, _mdiButtons._bounds);
				if (mdiArea.Width > 0 && mdiArea.Height > 0)
				{
					_mdiButtons.RenderContent(graphics, null, baseStyles, ForeColor);
				}
			}
			GraphicsState state = graphics.Save();
			if (IsFullRowLayout() && _floatingWindow == null)
			{
				Rectangle items = client;
				items.Inflate(-1, -1);
				if (_customizeItem != null)
				{
					if (_isHorizontal)
					{
						items.Width = _customizeItem._bounds.Left;
					}
					else
					{
						items.Height = _customizeItem._bounds.Top;
					}
				}
				graphics.SetClip(items, CombineMode.Intersect);
			}
			SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			foreach (CommandLayoutState item in _layoutItems)
			{
				if (((FormatStyleLayoutState)item).IsVisible() && !Rectangle.Intersect(e.ClipRectangle, item._bounds).IsEmpty)
				{
					DrawThemedItem(graphics, theme, item, baseStyles, textLayoutRenderer);
				}
			}
			graphics.SmoothingMode = smoothing;
			graphics.Restore(state);
			DrawThemedRowSeparators(graphics, theme, client);
			if (_gripItem != null && !Rectangle.Intersect(e.ClipRectangle, _gripItem._bounds).IsEmpty)
			{
				DrawThemedGrip(graphics, theme, _gripItem._bounds);
			}
			if (_customizeItem != null && !Rectangle.Intersect(e.ClipRectangle, _customizeItem._bounds).IsEmpty)
			{
				DrawThemedCustomizeButton(graphics, theme, _customizeItem);
			}
			if (GetCustomizeSelectedCommand() != null)
			{
				DrawCustomizeSelection(graphics);
			}
		}

		private void DrawThemedItem(Graphics graphics, ResolvedTheme theme, CommandLayoutState item, StateStyles baseStyles, TextLayoutRenderer textLayoutRenderer)
		{
			MxCommand command = item.GetCommand();
			if (command.IsSeparator())
			{
				DrawThemedSeparator(graphics, theme, item._bounds);
				return;
			}
			bool enabled = ((FormatStyleLayoutState)item).IsEnabled() && base.Enabled;
			bool pressed = ((FormatStyleLayoutState)item).IsPressed();
			bool hot = ((FormatStyleLayoutState)item).IsHot();
			bool isChecked = command.IsChecked;
			bool opened = item == _selectedItem;
			if (hot && command.IsParent && GetChildMenu() != null && GetChildMenu()._parentItem == item)
			{
				hot = false;
			}
			bool dropDownHot = item.IsSplitButton() && item.IsDropDownHot();
			if (hot && dropDownHot)
			{
				hot = false;
			}
			bool selected = pressed || isChecked || opened;
			StateStyles styles = baseStyles.CreateMergedStateStyles(command.GetEffectiveStateStyles());
			FormatStyle style = BuildItemFormatStyle(styles, hot, selected && (!command.IsParent || isChecked), enabled, !item._isDropDownPressed);
			ApplyTextAlignment(ref textLayoutRenderer, command);
			Rectangle bounds = item._bounds;
			if (enabled && (hot || selected || opened))
			{
				ThemeRole role = pressed || opened ? ThemeRole.SurfacePressed : hot ? ThemeRole.SurfaceHot : ThemeRole.AccentSubtle;
				FillThemed(graphics, theme, style.BackColor, role, bounds);
				if (isChecked && !opened)
				{
					theme.DrawRounded(graphics, bounds, ThemeRole.Accent, theme.Radius(ThemeElement.Button));
				}
			}
			else if (!style.BackColor.IsEmpty)
			{
				graphics.FillRectangle(GdiCache.Brush(style.BackColor), bounds);
			}
			if (!item._dropDownBounds.IsEmpty)
			{
				Rectangle part = item._dropDownBounds;
				if (enabled && (dropDownHot || item._isDropDownPressed))
				{
					FillThemed(graphics, theme, Color.Empty, item._isDropDownPressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, part);
				}
				if (enabled && (hot || dropDownHot || pressed))
				{
					graphics.DrawLine(theme.Pen(ThemeRole.Border), part.Left, part.Top + 3, part.Left, part.Bottom - 4);
				}
				Color arrow = style.ForeColor.IsEmpty ? theme.Color(enabled ? ThemeRole.ToolbarText : ThemeRole.TextDisabled) : style.ForeColor;
				RenderingHelper.DrawUpDownTriangle(graphics, DropDownArrowSize, part, 1, GdiCache.Pen(arrow));
			}
			DrawText(graphics, item, style, textLayoutRenderer);
			style.DisposeResources();
		}

		// A color the developer set for the state wins over the role of the theme.
		private static void FillThemed(Graphics graphics, ResolvedTheme theme, Color custom, ThemeRole role, Rectangle bounds)
		{
			if (custom.IsEmpty || custom.ToArgb() == theme.Color(role).ToArgb())
			{
				theme.FillRounded(graphics, bounds, role, theme.Radius(ThemeElement.Button));
			}
			else
			{
				graphics.FillRectangle(GdiCache.Brush(custom), bounds);
			}
		}

		// A separator that breaks a bar into rows is a line over the whole row, not a short mark between two items.
		private void DrawThemedRowSeparators(Graphics graphics, ResolvedTheme theme, Rectangle client)
		{
			if (_rows == null || _rows.Count <= 1)
			{
				return;
			}
			int grip = _gripItem == null ? 0 : (_isHorizontal ? _gripItem._bounds.Width : _gripItem._bounds.Height);
			int inset = theme.Scaled(ThemedSeparatorInset);
			Pen pen = theme.Pen(ThemeRole.Border);
			foreach (CommandLayoutRow row in _rows)
			{
				if (!row._isSingleCommandLine)
				{
					continue;
				}
				Rectangle bounds = ((CommandLayoutState)row.GetItems()[0])._bounds;
				if (_isHorizontal)
				{
					int y = bounds.Top + bounds.Height / 2;
					graphics.DrawLine(pen, client.Left + grip, y, client.Right - inset - 1, y);
				}
				else
				{
					int x = bounds.Left + bounds.Width / 2;
					graphics.DrawLine(pen, x, client.Top + grip, x, client.Bottom - inset - 1);
				}
			}
		}

		private void DrawThemedSeparator(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			Pen pen = theme.Pen(ThemeRole.Border);
			int inset = theme.Scaled(ThemedSeparatorInset);
			if (_isHorizontal)
			{
				int x = bounds.X + bounds.Width / 2;
				graphics.DrawLine(pen, x, bounds.Top + inset, x, bounds.Bottom - inset - 1);
			}
			else
			{
				int y = bounds.Y + bounds.Height / 2;
				graphics.DrawLine(pen, bounds.Left + inset, y, bounds.Right - inset - 1, y);
			}
		}

		private void DrawThemedGrip(Graphics graphics, ResolvedTheme theme, Rectangle bounds)
		{
			if (theme.Style.Grip)
			{
				Rectangle area = _isHorizontal ? new Rectangle(bounds.X, bounds.Y + 2, bounds.Width, bounds.Height - 4) : new Rectangle(bounds.X + 2, bounds.Y, bounds.Width - 4, bounds.Height);
				theme.DrawGrip(graphics, area, !_isHorizontal);
				return;
			}
			Brush dots = theme.Brush(ThemeRole.BorderStrong);
			int size = Math.Max(2, theme.Scaled(2));
			int step = Math.Max(4, theme.Scaled(4));
			if (_isHorizontal)
			{
				int x = bounds.Left + (bounds.Width - size) / 2;
				for (int y = bounds.Top + step; y + size < bounds.Bottom - step / 2; y += step)
				{
					graphics.FillRectangle(dots, x, y, size, size);
				}
			}
			else
			{
				int y = bounds.Top + (bounds.Height - size) / 2;
				for (int x = bounds.Left + step; x + size < bounds.Right - step / 2; x += step)
				{
					graphics.FillRectangle(dots, x, y, size, size);
				}
			}
		}

		private void DrawThemedCustomizeButton(Graphics graphics, ResolvedTheme theme, CommandLayoutState item)
		{
			Rectangle bounds = item._bounds;
			bool opened = item == _selectedItem;
			if (opened || ((FormatStyleLayoutState)item).IsHot())
			{
				SmoothingMode smoothing = graphics.SmoothingMode;
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				theme.FillRounded(graphics, bounds, opened ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, theme.Radius(ThemeElement.Button));
				graphics.SmoothingMode = smoothing;
			}
			RenderingHelper.DrawUpDownTriangle(graphics, DropDownArrowSize, bounds, 1, theme.Pen(ThemeRole.ToolbarText));
		}
	}
}
