using System;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class EditBase : IThemeListener
	{
		private bool _listensToThemes;

		private bool _themeStale;

		void IThemeListener.OnThemeChanged()
		{
			if (IsDisposed || _officeStyle == null)
			{
				return;
			}
			// A hidden editor (for example on another tab) catches up when it is shown.
			if (!Visible)
			{
				_themeStale = true;
				return;
			}
			ApplyThemeChange();
		}

		private void ApplyThemeChange()
		{
			_themeStale = false;
			ApplyThemedTextBoxColors();
			_officeStyle.ApplyThemeChange();
			_officeStyle.GetLayoutState().ResetCachedStyles();
			RecalculateLayout();
			Invalidate();
		}

		private void ApplyStaleTheme()
		{
			if (_themeStale && Visible && !IsDisposed && _officeStyle != null)
			{
				ApplyThemeChange();
			}
		}

		// A themed editor listens to theme changes. Other looks do not.
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

		// The edit box inside the editor is a native control, so it does not read the theme by itself.
		private void ApplyThemedTextBoxColors()
		{
			if (_textBox != null)
			{
				_textBox.BackColor = base.Enabled ? BackColor : DisabledBackColor;
				_textBox.ForeColor = base.Enabled ? ForeColor : DisabledForeColor;
				ThemeWindow.ApplyNativeLook(_textBox, MxThemeManager.DefaultTheme);
			}
		}
	}
}
