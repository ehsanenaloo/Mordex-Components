using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonContextMenu
	{
		private ThemeSubscription _themeSubscription;

		private ThemeSubscription ThemeSub => _themeSubscription ?? (_themeSubscription = new ThemeSubscription(OnDefaultThemeChanged));

		// A themed menu takes new colors when the default theme changes.
		private void OnDefaultThemeChanged()
		{
			RebuildColorScheme();
		}
	}
}
