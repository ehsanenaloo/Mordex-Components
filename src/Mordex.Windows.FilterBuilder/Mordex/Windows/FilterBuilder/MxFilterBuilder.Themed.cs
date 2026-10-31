using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.FilterBuilder
{
	public partial class MxFilterBuilder : IThemeListener
	{
		private bool _listensToThemes;
		private MxTheme _lookTheme;

		internal Color ConditionBorderColor { get; private set; }

		void IThemeListener.OnThemeChanged()
		{
			if (!IsDisposed)
			{
				ApplyThemeColors();
			}
		}

		// The control listens to theme changes only while it is themed and has a handle.
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
					ApplyThemeColors();
				}
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

		private void ApplyThemeColors()
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			_lookTheme = theme;
			ConditionBorderColor = theme.GetColor(ThemeRole.Border);
			Invalidate(invalidateChildren: true);
		}
	}
}
