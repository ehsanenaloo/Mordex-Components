using System;
using System.Drawing;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	public partial class DatePickerCalendar : IThemeListener
	{
		private bool _listensToThemes;

		void IThemeListener.OnThemeChanged()
		{
			ReleaseThemeRenderer();
			ApplyThemeToFormatStyles();
			Invalidate();
		}

		// The calendar repaints when the default theme changes. It listens while it has a handle.
		private void UpdateThemeListening()
		{
			if (IsHandleCreated && !_listensToThemes)
			{
				MxThemeManager.AddListener(this);
				_listensToThemes = true;
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

		internal void ApplyThemeToFormatStyles()
		{
			HeaderFormatStyle.ApplyTheme();
			SelectedFormatStyle.ApplyTheme();
			TrailingFormatStyle.ApplyTheme();
		}

		internal void OnTextRenderingChanged()
		{
			UpdateMonthLayouts(false);
			UpdateMonthDates(true);
			UpdateDayHeaderTexts();
			if (_calendarControl != null)
			{
				_calendarControl._useTextRenderer = !UsesCompatibleTextRendering();
			}
		}

		internal bool UsesCompatibleTextRendering()
		{
			if (_datePicker != null)
			{
				return _datePicker.UseCompatibleTextRendering;
			}
			return false;
		}

		private Color GetEffectiveBackColor()
		{
			return ShouldSerializeBackColor() ? BackColor : MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.InputBack);
		}

		internal Color GetEffectiveForeColor()
		{
			if (ForeColor == SystemColors.WindowText)
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.Text);
			}
			return base.ForeColor;
		}

		internal ControlStyle GetControlStyle()
		{
			if (_controlStyle == null)
			{
				_controlStyle = new ControlStyle(null);
			}
			return _controlStyle;
		}

		internal void SetControlStyle(ControlStyle controlStyle)
		{
			_controlStyle = controlStyle;
		}
	}
}
