using System;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid
{
	public partial class MxFieldChooser : IThemeListener
	{
		private bool _listensToThemes;

		void IThemeListener.OnThemeChanged()
		{
			if (IsDisposed || _chooserControlStyle == null)
			{
				return;
			}
			// The cached styles hold the colors of the old theme.
			_chooserControlStyle.ApplyThemeChange();
			_chooserControlStyle.GetLayoutState().ResetCachedStyles();
			_chooserControlStyle.ReleaseLayoutState();
			Invalidate(true);
		}

		// The chooser listens to theme changes only while it is themed and has a handle.
		private void UpdateThemeListening()
		{
			bool wanted = IsHandleCreated;
			if (wanted && !_listensToThemes)
			{
				MxThemeManager.AddListener(this);
				_listensToThemes = true;
			}
			else if (!wanted)
			{
				StopListeningToThemes();
			}
		}

		private void StopListeningToThemes()
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
			StopListeningToThemes();
			base.OnHandleDestroyed(e);
		}
	}
}
