using System.Drawing;
using System.ComponentModel;
using System;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxScrollPanel
	{
		private ThemeHook _themeHook;

		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				_themeHook = new ThemeHook(this, OnDefaultThemeChanged);
			}
			_themeHook.Update(true);
		}

		private void OnDefaultThemeChanged()
		{
			_mordexColorSchemeAppearanceState?.RebuildColors();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}
	}
}
