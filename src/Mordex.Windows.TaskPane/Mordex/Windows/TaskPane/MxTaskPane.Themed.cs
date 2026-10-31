using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane : IThemeListener
	{
		private bool _listensToThemes;

		void IThemeListener.OnThemeChanged()
		{
			RefreshThemeResources();
			Invalidate(true);
		}

		internal ResolvedTheme GetTheme()
		{
			return MxThemeManager.ResolveDefault(DeviceDpi);
		}

		// The pane repaints when the default theme changes.
		private void UpdateThemeListening()
		{
			bool wanted = IsHandleCreated;
			if (wanted && !_listensToThemes)
			{
				MxThemeManager.AddListener(this);
				_listensToThemes = true;
			}
			else if (!wanted && _listensToThemes)
			{
				MxThemeManager.RemoveListener(this);
				_listensToThemes = false;
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			// The default theme can change while the pane has no handle and no listener.
			RefreshThemeResources();
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (_listensToThemes)
			{
				MxThemeManager.RemoveListener(this);
				_listensToThemes = false;
			}
			base.OnHandleDestroyed(e);
		}

		// Sets the default colors of a style from the theme. A color the developer set on the style still wins.
		private void ApplyThemedStyle(TaskPaneFormatStyle style)
		{
			ResolvedTheme theme = GetTheme();
			Color back = Color.Empty;
			Color fore = theme.Color(ThemeRole.Text);
			if (theme.Style.ExplorerBar && ApplyExplorerStyle(style, theme))
			{
				return;
			}
			if (style == _backgroundFormatStyle)
			{
				back = theme.Color(ThemeRole.Surface);
			}
			else if (style == _groupsStateStyles.FormatStyle)
			{
				back = theme.Color(ThemeRole.Header);
				fore = theme.Color(ThemeRole.HeaderText);
			}
			else if (style == _groupsStateStyles.HotFormatStyle)
			{
				back = theme.Color(ThemeRole.HeaderHot);
				fore = theme.Color(ThemeRole.HeaderText);
			}
			else if (style == _groupsStateStyles.PressedFormatStyle || style == _groupsStateStyles.SelectedFormatStyle)
			{
				back = theme.Color(ThemeRole.HeaderPressed);
				fore = theme.Color(ThemeRole.HeaderText);
			}
			else if (style == _specialGroupsStateStyles.FormatStyle)
			{
				back = theme.Color(ThemeRole.Accent);
				fore = theme.Color(ThemeRole.TextOnAccent);
			}
			else if (style == _specialGroupsStateStyles.HotFormatStyle)
			{
				back = theme.Color(ThemeRole.AccentHot);
				fore = theme.Color(ThemeRole.TextOnAccent);
			}
			else if (style == _specialGroupsStateStyles.PressedFormatStyle || style == _specialGroupsStateStyles.SelectedFormatStyle)
			{
				back = theme.Color(ThemeRole.AccentPressed);
				fore = theme.Color(ThemeRole.TextOnAccent);
			}
			else if (style == _itemsStateStyles.FormatStyle)
			{
				back = theme.Color(ThemeRole.SurfaceRaised);
				fore = theme.Color(ThemeRole.TextLink);
			}
			else if (style == _itemsStateStyles.HotFormatStyle)
			{
				back = theme.Color(ThemeRole.SurfaceHot);
				fore = theme.Color(ThemeRole.TextLink);
			}
			else if (style == _itemsStateStyles.SelectedFormatStyle || style == _itemsStateStyles.PressedFormatStyle)
			{
				back = theme.Color(ThemeRole.AccentSubtle);
				fore = theme.Color(ThemeRole.TextLink);
			}
			else if (style == _groupsStateStyles.DisabledFormatStyle || style == _specialGroupsStateStyles.DisabledFormatStyle || style == _itemsStateStyles.DisabledFormatStyle)
			{
				fore = theme.Color(ThemeRole.TextDisabled);
			}
			style.ApplyThemedColors(back, fore);
		}

		// The Windows XP explorer bar takes its colors from the caption color of the theme: a pale blue pane, white to pale
		// headers with dark text, and headers of special groups in the caption color with white text.
		private Color ExplorerPaneColor(ResolvedTheme theme)
		{
			return MxTheme.Blend(theme.Color(ThemeRole.Caption), Color.White, 0.5f);
		}

		private bool ApplyExplorerStyle(TaskPaneFormatStyle style, ResolvedTheme theme)
		{
			Color cap = theme.Color(ThemeRole.Caption);
			Color back;
			Color fore;
			if (style == _backgroundFormatStyle)
			{
				back = ExplorerPaneColor(theme);
				fore = theme.Color(ThemeRole.Text);
			}
			else if (style == _groupsStateStyles.FormatStyle || style == _groupsStateStyles.HotFormatStyle || style == _groupsStateStyles.PressedFormatStyle || style == _groupsStateStyles.SelectedFormatStyle)
			{
				back = MxTheme.Blend(cap, Color.White, style == _groupsStateStyles.FormatStyle ? 0.8f : 0.88f);
				fore = MxTheme.Darken(cap, 0.1f);
			}
			else if (style == _specialGroupsStateStyles.FormatStyle || style == _specialGroupsStateStyles.HotFormatStyle || style == _specialGroupsStateStyles.PressedFormatStyle || style == _specialGroupsStateStyles.SelectedFormatStyle)
			{
				back = style == _specialGroupsStateStyles.FormatStyle ? cap : MxTheme.Lighten(cap, 0.2f);
				fore = Color.White;
			}
			else if (style == _itemsStateStyles.FormatStyle || style == _itemsStateStyles.HotFormatStyle || style == _itemsStateStyles.SelectedFormatStyle || style == _itemsStateStyles.PressedFormatStyle)
			{
				back = MxTheme.Blend(cap, Color.White, style == _itemsStateStyles.FormatStyle ? 0.9f : 0.82f);
				fore = MxTheme.Darken(cap, 0.1f);
			}
			else
			{
				return false;
			}
			style.ApplyThemedColors(back, fore);
			return true;
		}

		private bool DrawExplorerHeader(Graphics graphics, Rectangle headerBounds, TaskPaneFormatStyle style, ResolvedTheme theme)
		{
			if (headerBounds.Width < 12 || headerBounds.Height < 8)
			{
				return false;
			}
			Color back = style.BackColor;
			bool special = style.ForeColor.ToArgb() == Color.White.ToArgb();
			Color left = special ? back : Color.White;
			Color right = special ? MxTheme.Lighten(back, 0.35f) : back;
			GdiCache.FillGradient(graphics, headerBounds, left, right, LinearGradientMode.Horizontal);
			Brush pane = GdiCache.Brush(ExplorerPaneColor(theme));
			int radius = Math.Max(3, theme.Scaled(5));
			for (int i = 0; i < radius; i++)
			{
				double reach = radius - Math.Sqrt(radius * radius - (radius - i - 0.5) * (radius - i - 0.5));
				int inset = (int)Math.Round(reach);
				if (inset > 0)
				{
					graphics.FillRectangle(pane, headerBounds.X, headerBounds.Y + i, inset, 1);
					graphics.FillRectangle(pane, headerBounds.Right - inset, headerBounds.Y + i, inset, 1);
				}
			}
			return true;
		}

		private void ApplyThemedStyles()
		{
			ApplyThemedStyle(_backgroundFormatStyle);
			TaskPaneStateStyles[] states = { _groupsStateStyles, _specialGroupsStateStyles, _itemsStateStyles };
			foreach (TaskPaneStateStyles state in states)
			{
				ApplyThemedStyle(state.FormatStyle);
				ApplyThemedStyle(state.HotFormatStyle);
				ApplyThemedStyle(state.SelectedFormatStyle);
				ApplyThemedStyle(state.PressedFormatStyle);
				ApplyThemedStyle(state.DisabledFormatStyle);
			}
		}

		// Fills a header with the style color. A color that is a role of the theme gets the rounded corners of the theme.
		private void DrawThemedHeaderBackground(Graphics graphics, Rectangle headerBounds, TaskPaneFormatStyle style, bool isExpanded)
		{
			Brush brush = style.GetBackBrush(headerBounds);
			if (brush == null)
			{
				return;
			}
			ResolvedTheme theme = GetTheme();
			if (theme.Style.ExplorerBar && DrawExplorerHeader(graphics, headerBounds, style, theme))
			{
				return;
			}
			int radius = theme.Radius(ThemeElement.Panel);
			ThemeRole role;
			if (theme.Style.FaceGradient != ThemeGradient.None && TryFindRole(theme, style.BackColor, out role))
			{
				// A gradient header: the shape reaches below the header and the clip cuts it, so only the top corners are round.
				GraphicsState state = graphics.Save();
				graphics.IntersectClip(headerBounds);
				Rectangle shape = headerBounds;
				if (isExpanded)
				{
					shape.Height += theme.Scaled(radius);
				}
				theme.FillFace(graphics, shape, role, radius, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
				graphics.Restore(state);
				return;
			}
			if (radius > 0 && TryFindRole(theme, style.BackColor, out role))
			{
				theme.FillRounded(graphics, headerBounds, role, radius);
				if (isExpanded)
				{
					int half = headerBounds.Height / 2;
					graphics.FillRectangle(brush, headerBounds.X, headerBounds.Y + half, headerBounds.Width, headerBounds.Height - half);
				}
				return;
			}
			graphics.FillRectangle(brush, headerBounds);
		}

		private static readonly ThemeRole[] HeaderRoles =
		{
			ThemeRole.Header, ThemeRole.HeaderHot, ThemeRole.HeaderPressed, ThemeRole.Accent, ThemeRole.AccentHot, ThemeRole.AccentPressed
		};

		private static bool TryFindRole(ResolvedTheme theme, Color color, out ThemeRole role)
		{
			foreach (ThemeRole candidate in HeaderRoles)
			{
				if (theme.Color(candidate) == color)
				{
					role = candidate;
					return true;
				}
			}
			role = ThemeRole.Header;
			return false;
		}

		// The body of a group has the rounded bottom corners of the theme. Under a header the top corners stay square: the
		// shape reaches above the body and the clip cuts the top corners away, so the body joins the header.
		private Rectangle ThemedBodyShape(TaskPaneGroup group, Rectangle bounds, ResolvedTheme theme, out bool joined)
		{
			joined = group.ShowGroupCaption && group.HeaderHeight != 0;
			if (!joined)
			{
				return bounds;
			}
			int lift = theme.Scaled(theme.Radius(ThemeElement.Panel));
			return new Rectangle(bounds.X, bounds.Y - lift, bounds.Width, bounds.Height + lift);
		}

		private bool TryFillThemedBody(Graphics graphics, TaskPaneGroup group, Rectangle bounds, Rectangle visible, Brush fillBrush)
		{
			if (!(fillBrush is SolidBrush solid))
			{
				return false;
			}
			ResolvedTheme theme = GetTheme();
			if (theme.Radius(ThemeElement.Panel) <= 0 || !TryFindAnyRole(theme, solid.Color, out ThemeRole role))
			{
				return false;
			}
			Rectangle shape = ThemedBodyShape(group, bounds, theme, out bool joined);
			GraphicsState state = graphics.Save();
			graphics.IntersectClip(visible);
			theme.FillRounded(graphics, shape, role, theme.Radius(ThemeElement.Panel));
			graphics.Restore(state);
			return true;
		}

		private bool TryDrawThemedBodyBorder(Graphics graphics, TaskPaneGroup group, Rectangle bounds, Color color)
		{
			ResolvedTheme theme = GetTheme();
			if (theme.Radius(ThemeElement.Panel) <= 0 || !TryFindAnyRole(theme, color, out ThemeRole role))
			{
				return false;
			}
			if (group.Container && !(group.ShowGroupCaption && group.HeaderHeight != 0))
			{
				bounds.Y--;
				bounds.Height++;
			}
			Rectangle shape = ThemedBodyShape(group, bounds, theme, out bool joined);
			GraphicsState state = graphics.Save();
			if (joined)
			{
				graphics.IntersectClip(bounds);
			}
			theme.DrawRounded(graphics, shape, role, theme.Radius(ThemeElement.Panel));
			graphics.Restore(state);
			return true;
		}

		private static bool TryFindAnyRole(ResolvedTheme theme, Color color, out ThemeRole role)
		{
			foreach (ThemeRole candidate in AllRoles)
			{
				if (theme.Color(candidate) == color)
				{
					role = candidate;
					return true;
				}
			}
			role = ThemeRole.Surface;
			return false;
		}

		private static readonly ThemeRole[] AllRoles = (ThemeRole[])Enum.GetValues(typeof(ThemeRole));

		private Color GetGlyphDisabledColor()
		{
			return GetTheme().Color(ThemeRole.TextDisabled);
		}

		private void DrawThemedChevron(Graphics graphics, Rectangle bounds, bool isExpanded, Color color)
		{
			float scale = DeviceDpi / 96f;
			if (GetTheme().Style.ExplorerBar)
			{
				color = MxTheme.Darken(GetTheme().Color(ThemeRole.Caption), 0.1f);
				// The round button of the explorer bar: a white disc with a thin edge in the chevron color.
				int size = Math.Min(Math.Min(bounds.Width, bounds.Height), (int)(17 * scale));
				Rectangle disc = new Rectangle(bounds.X + (bounds.Width - size) / 2, bounds.Y + (bounds.Height - size) / 2, size - 1, size - 1);
				System.Drawing.Drawing2D.SmoothingMode before = graphics.SmoothingMode;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				graphics.FillEllipse(GdiCache.Brush(Color.White), disc);
				graphics.DrawEllipse(GdiCache.Pen(color), disc);
				graphics.SmoothingMode = before;
			}
			float halfWidth = 4f * scale;
			float halfHeight = 2f * scale;
			float cx = bounds.X + bounds.Width / 2f;
			float cy = bounds.Y + bounds.Height / 2f;
			float tip = isExpanded ? -halfHeight : halfHeight;
			System.Drawing.Drawing2D.SmoothingMode smoothing = graphics.SmoothingMode;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			Pen pen = GdiCache.Pen(color, Math.Max(1.5f, 1.5f * scale));
			graphics.DrawLine(pen, cx - halfWidth, cy - tip, cx, cy + tip);
			graphics.DrawLine(pen, cx, cy + tip, cx + halfWidth, cy - tip);
			graphics.SmoothingMode = smoothing;
		}

		private void DrawThemedButtonBackground(Graphics graphics, Rectangle bounds, bool isHot, bool isPressed)
		{
			if (!isHot && !isPressed)
			{
				return;
			}
			ResolvedTheme theme = GetTheme();
			theme.FillRounded(graphics, bounds, isPressed ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, Math.Min(theme.Radius(ThemeElement.Panel), bounds.Height / 4));
		}
	}
}
