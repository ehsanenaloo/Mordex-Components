using System;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		private ThemeSubscription _themeSubscription;

		private ThemeSubscription ThemeSub => _themeSubscription ?? (_themeSubscription = new ThemeSubscription(OnDefaultThemeChanged));

		// A themed ribbon takes new colors when the default theme changes.
		private void OnDefaultThemeChanged()
		{
			if (!IsDisposed)
			{
				RebuildColorScheme();
				ReconcileCaptionExtension(false);
				RequestTabStripLayout(true);
				InvalidateFullLayout();
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			ThemeSub.Set(false);
			StopCaptionWatch();
			base.OnHandleDestroyed(e);
		}
	}
}
