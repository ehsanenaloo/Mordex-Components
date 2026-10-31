using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.Timeline
{
	public partial class TimelineFormatStyle
	{
		// The theme sets the defaults of the predefined styles. A color that the developer sets still wins.
		private void ApplyThemedColors()
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault();
			_defaultBackColor = Color.Empty;
			switch (_predefinedStyle)
			{
			case PredefinedStyle.Trailing:
				_defaultForeColor = theme.Color(ThemeRole.TextDisabled);
				break;
			case PredefinedStyle.LowerScale:
				_defaultBackColor = theme.Color(ThemeRole.Surface);
				_defaultForeColor = theme.Color(ThemeRole.Text);
				break;
			case PredefinedStyle.GroupRow:
			case PredefinedStyle.RowHeader:
			case PredefinedStyle.Header:
			case PredefinedStyle.UpperScale:
				_defaultBackColor = theme.Color(ThemeRole.Header);
				_defaultForeColor = theme.Color(ThemeRole.HeaderText);
				break;
			case PredefinedStyle.Selected:
				_defaultBackColor = theme.Color(ThemeRole.Selection);
				_defaultForeColor = theme.Color(ThemeRole.SelectionText);
				_defaultBackgroundGradientMode = BackgroundGradientMode.Solid;
				break;
			case PredefinedStyle.SelectedInactive:
				_defaultBackColor = theme.Color(ThemeRole.SelectionInactive);
				_defaultForeColor = theme.Color(ThemeRole.SelectionInactiveText);
				_defaultBackgroundGradientMode = BackgroundGradientMode.Solid;
				break;
			case PredefinedStyle.Item:
				_defaultBackColor = theme.Color(ThemeRole.SurfaceRaised);
				_defaultForeColor = theme.Color(ThemeRole.Text);
				break;
			case PredefinedStyle.ItemBar:
				_defaultBackColor = theme.Color(ThemeRole.Accent);
				break;
			case PredefinedStyle.EstimatedItemBar:
				_defaultBackColor = theme.Color(ThemeRole.Warning);
				break;
			case PredefinedStyle.WorkingTime:
				_defaultBackColor = theme.Color(ThemeRole.InputBack);
				break;
			}
			ThemeStyle look = theme.Style;
			_defaultBackColorGradient = Color.Empty;
			bool headerLike = _predefinedStyle == PredefinedStyle.GroupRow || _predefinedStyle == PredefinedStyle.RowHeader || _predefinedStyle == PredefinedStyle.Header || _predefinedStyle == PredefinedStyle.UpperScale;
			if (headerLike && look.FaceGradient == ThemeGradient.None)
			{
				_defaultBackgroundGradientMode = BackgroundGradientMode.Empty;
			}
			if (headerLike && look.FaceGradient != ThemeGradient.None)
			{
				Color face = _defaultBackColor;
				_defaultBackColor = look.LightEnd(face);
				_defaultBackColorGradient = look.DarkEnd(face);
				_defaultBackgroundGradientMode = look.FaceGradient == ThemeGradient.Horizontal ? BackgroundGradientMode.Horizontal : BackgroundGradientMode.Vertical;
			}
		}
	}
}
