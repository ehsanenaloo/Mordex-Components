using System;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonStatusBar
	{
		private ThemeSubscription _themeSubscription;

		private ThemeSubscription ThemeSub => _themeSubscription ?? (_themeSubscription = new ThemeSubscription(OnDefaultThemeChanged));

		// A themed status bar takes new colors when the default theme changes.
		private void OnDefaultThemeChanged()
		{
			if (!IsDisposed)
			{
				RefreshColorScheme();
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			RefreshColorScheme();
			ThemeSub.Set(true);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			ThemeSub.Set(false);
			base.OnHandleDestroyed(e);
		}
	}
}
