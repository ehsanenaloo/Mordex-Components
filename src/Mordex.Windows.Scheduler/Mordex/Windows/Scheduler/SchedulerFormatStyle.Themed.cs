using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerFormatStyle
	{
		// The fill for a part that the theme paints flat: a brush made from the back color the developer set, or null when no
		// back color is set and the theme decides. A gradient is used only when the developer set the gradient mode too.
		// The caller disposes the brush.
		internal Brush CreateDeveloperBackBrush(Rectangle bounds)
		{
			if (_backColor.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0)
			{
				return null;
			}
			SchedulerFormatStyle fill = new SchedulerFormatStyle(this);
			if (_backgroundGradientMode == BackgroundGradientMode.Empty)
			{
				fill._backgroundGradientMode = BackgroundGradientMode.Solid;
			}
			Brush brush = fill.GetBackBrush(bounds);
			fill._backBrush = null;
			fill.DisposeResources();
			return brush;
		}

		// The theme sets the defaults of the predefined styles. A color that the developer sets still wins.
		private void ApplyThemedColors()
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault();
			switch (_predefinedStyle)
			{
			case PredefinedStyle.Appointment:
				_themeBackColor = theme.Color(ThemeRole.SurfaceRaised);
				_themeForeColor = theme.Color(ThemeRole.Text);
				break;
			case PredefinedStyle.Trailing:
				_themeForeColor = theme.Color(ThemeRole.TextDisabled);
				break;
			case PredefinedStyle.WorkHour:
				_themeBackColor = theme.Color(ThemeRole.Surface);
				break;
			case PredefinedStyle.Hour:
			case PredefinedStyle.AlternatingDay:
				_themeBackColor = theme.Color(ThemeRole.SurfaceAlt);
				break;
			case PredefinedStyle.AllDay:
			case PredefinedStyle.TimeNavigator:
			case PredefinedStyle.Header:
				_themeBackColor = theme.Color(ThemeRole.Header);
				_themeForeColor = theme.Color(ThemeRole.HeaderText);
				_themeGradientMode = BackgroundGradientMode.Solid;
				if (_predefinedStyle == PredefinedStyle.Header && theme.Style.Detail.ChoiceLook != ThemeChoiceLook.Default && theme.Style.FaceGradient == ThemeGradient.Vertical)
				{
					// The older looks give the headers a face gradient, light at the top.
					_themeBackColor = theme.Style.LightEnd(_themeBackColor);
					_themeBackColorGradient = theme.Style.DarkEnd(theme.Color(ThemeRole.Header));
					_themeGradientMode = BackgroundGradientMode.Vertical;
				}
				break;
			case PredefinedStyle.Selected:
				_themeBackColor = theme.Color(ThemeRole.Selection);
				_themeForeColor = theme.Color(ThemeRole.SelectionText);
				_themeGradientMode = BackgroundGradientMode.Solid;
				break;
			case PredefinedStyle.SelectedInactive:
				_themeBackColor = theme.Color(ThemeRole.SelectionInactive);
				_themeForeColor = theme.Color(ThemeRole.SelectionInactiveText);
				_themeGradientMode = BackgroundGradientMode.Solid;
				break;
			}
		}
	}
}
