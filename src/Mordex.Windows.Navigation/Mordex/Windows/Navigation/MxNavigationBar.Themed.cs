using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Navigation
{
	public partial class MxNavigationBar : IThemeListener
	{
		private bool _listensToThemes;

		void IThemeListener.OnThemeChanged()
		{
			RefreshFormatStyles();
			Invalidate(true);
		}

		internal ResolvedTheme GetTheme()
		{
			return MxThemeManager.ResolveDefault(DeviceDpi);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (!_listensToThemes)
			{
				MxThemeManager.AddListener(this);
				_listensToThemes = true;
			}
			RefreshFormatStyles();
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

		// Sets the default colors of a state style from the theme. A color the developer set on the style still wins.
		private void ApplyThemedStyle(NavigationBarFormatStyle style)
		{
			ResolvedTheme theme = GetTheme();
			Color back = Color.Empty;
			Color fore = Color.Empty;
			if (style == _groupsBackgroundFormatStyle)
			{
				back = theme.Color(ThemeRole.Surface);
			}
			else if (style == GroupsStateStyles.FormatStyle)
			{
				back = theme.Color(ThemeRole.Header);
				fore = theme.Color(ThemeRole.HeaderText);
			}
			else if (style == GroupsStateStyles.HotFormatStyle)
			{
				back = theme.Color(ThemeRole.HeaderHot);
				fore = theme.Color(ThemeRole.HeaderText);
			}
			else if (style == GroupsStateStyles.SelectedFormatStyle || style == GroupsStateStyles.PressedFormatStyle)
			{
				back = theme.Color(ThemeRole.HeaderPressed);
				fore = theme.Color(ThemeRole.HeaderText);
			}
			else if (style == ItemsStateStyles.FormatStyle)
			{
				fore = theme.Color(ThemeRole.Text);
			}
			else if (style == ItemsStateStyles.HotFormatStyle)
			{
				back = theme.Color(ThemeRole.SurfaceHot);
				fore = theme.Color(ThemeRole.Text);
			}
			else if (style == ItemsStateStyles.SelectedFormatStyle || style == ItemsStateStyles.PressedFormatStyle)
			{
				back = theme.Color(ThemeRole.AccentSubtle);
				fore = theme.Color(ThemeRole.Text);
			}
			else if (style == GroupsStateStyles.DisabledFormatStyle || style == ItemsStateStyles.DisabledFormatStyle)
			{
				fore = theme.Color(ThemeRole.TextDisabled);
			}
			if (theme.Style.Detail.NavHeaderTint && !back.IsEmpty && (style == GroupsStateStyles.FormatStyle || style == GroupsStateStyles.HotFormatStyle || style == GroupsStateStyles.SelectedFormatStyle || style == GroupsStateStyles.PressedFormatStyle))
			{
				// Outlook 2003 style group headers: a tint of the caption color of the theme.
				float amount = style == GroupsStateStyles.FormatStyle ? 0.3f : style == GroupsStateStyles.HotFormatStyle ? 0.2f : 0.1f;
				Color tint = MxTheme.Blend(theme.Color(ThemeRole.SurfaceRaised), theme.Color(ThemeRole.Caption), amount + 0.1f);
				style.ApplyThemedColors(MxTheme.Blend(Color.White, tint, 0.25f), fore, tint);
				return;
			}
			style.ApplyThemedColors(back, fore);
		}

		internal void ApplyThemedTooltip(NavigationBarTipWindow tip)
		{
			ResolvedTheme theme = GetTheme();
			tip.SetBackColor(theme.Color(ThemeRole.TooltipBack));
			tip.SetTextColor(theme.Color(ThemeRole.TooltipText));
			tip.SetBorderColor(theme.Color(ThemeRole.Border), theme.Shape.HasShadow);
		}
	}
}
