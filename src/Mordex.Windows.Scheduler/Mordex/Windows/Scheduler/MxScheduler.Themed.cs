using System;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler : IThemeListener
	{
		private bool _listensToThemes;
		private MxTheme _lookTheme;

		void IThemeListener.OnThemeChanged()
		{
			_lookTheme = MxThemeManager.DefaultTheme;
			// The format styles and the current time colors hold theme colors, so they are applied again.
			OnAreaStyleChanged(EventArgs.Empty);
			Invalidate(invalidateChildren: true);
		}

		// A themed scheduler repaints when the default theme changes. Other looks do not listen.
		private void UpdateThemeListening()
		{
			bool wanted = IsHandleCreated;
			if (wanted && !_listensToThemes)
			{
				MxThemeManager.AddListener(this);
				_listensToThemes = true;
				// The default theme can change while the control has no handle and does not listen.
				if (_lookTheme != MxThemeManager.DefaultTheme)
				{
					((IThemeListener)this).OnThemeChanged();
				}
			}
			else if (!wanted && _listensToThemes)
			{
				StopThemeListening();
			}
		}

		private void StopThemeListening()
		{
			if (_listensToThemes)
			{
				MxThemeManager.RemoveListener(this);
				_listensToThemes = false;
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			StopThemeListening();
			base.OnHandleDestroyed(e);
		}
	}
}
