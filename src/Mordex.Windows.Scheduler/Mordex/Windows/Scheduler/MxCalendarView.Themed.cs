using System;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	public partial class MxCalendarView : IThemeListener
	{
		private bool _listensToThemes;

		void IThemeListener.OnThemeChanged()
		{
			ApplyThemeToFormatStyles();
			Invalidate();
		}

		// A themed calendar repaints when the default theme changes. Other looks do not listen.
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
