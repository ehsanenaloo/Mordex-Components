using System;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid
{
	public partial class ScrollableBase : IThemeListener
	{
		private bool _listensToThemes;
		private MxTheme _lookTheme;

		// The control style does not exist yet while the base constructors run.
		internal bool IsThemedLook => _gridControlStyle != null;

		void IThemeListener.OnThemeChanged()
		{
			if (!IsDisposed && _gridControlStyle != null)
			{
				RefreshThemedLook();
			}
		}

		// A themed control listens to theme changes. Other looks do not.
		internal void UpdateThemeListening()
		{
			bool wanted = IsThemedLook && IsHandleCreated;
			if (wanted && !_listensToThemes)
			{
				MxThemeManager.AddListener(this);
				_listensToThemes = true;
				// The default theme can change while the control has no handle and does not listen.
				if (_lookTheme != MxThemeManager.DefaultTheme)
				{
					RefreshThemedLook();
				}
			}
			else if (!wanted && _listensToThemes)
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
			if (_listensToThemes)
			{
				MxThemeManager.RemoveListener(this);
				_listensToThemes = false;
			}
			base.OnHandleDestroyed(e);
		}

		// The cached styles hold the colors of the old theme, so they are dropped before the repaint.
		private void RefreshThemedLook()
		{
			_lookTheme = MxThemeManager.DefaultTheme;
			_gridControlStyle.ApplyThemeChange();
			OnVisualStyleAreaChanged();
			_gridControlStyle.GetLayoutState().ResetCachedStyles();
			_gridControlStyle.ReleaseLayoutState();
			_numericEditControl.UpdateCStyle(_gridControlStyle);
			RelayoutScrollControls();
			_numericEditControl.Invalidate();
		}
	}
}
