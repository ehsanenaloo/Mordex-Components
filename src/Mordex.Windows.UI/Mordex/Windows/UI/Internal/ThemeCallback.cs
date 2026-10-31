using System;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Internal
{
	// A theme listener for a component that has no handle. The owner keeps it; the manager holds it weakly.
	internal sealed class ThemeCallback : IThemeListener
	{
		private readonly Action _changed;

		private bool _listening;

		internal ThemeCallback(Action changed)
		{
			_changed = changed;
		}

		void IThemeListener.OnThemeChanged()
		{
			_changed();
		}

		internal void Update(bool themed)
		{
			if (themed && !_listening)
			{
				MxThemeManager.AddListener(this);
				_listening = true;
			}
			else if (!themed && _listening)
			{
				MxThemeManager.RemoveListener(this);
				_listening = false;
			}
		}
	}
}
