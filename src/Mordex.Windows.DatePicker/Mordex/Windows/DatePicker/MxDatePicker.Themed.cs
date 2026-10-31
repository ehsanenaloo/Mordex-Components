using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker : IThemeListener
	{
		private bool _listensToThemes;

		void IThemeListener.OnThemeChanged()
		{
			ReleaseThemeRenderer();
			DropDownCalendar.ReleaseThemeRenderer();
			DropDownCalendar.ApplyThemeToFormatStyles();
			_controlStyle.ApplyThemeToFormats();
			ApplyEditColors();
			Invalidate();
		}

		// The picker repaints when the default theme changes. It listens while it has a handle.
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

		private Color GetEditBackColor()
		{
			return ShouldSerializeBackColor() ? BackColor : MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.InputBack);
		}

		internal Color GetEditForeColor()
		{
			// The default fore color of a control is the control text color, so both system colors count as not set.
			if (ForeColor == SystemColors.WindowText || ForeColor == SystemColors.ControlText)
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.Text);
			}
			return base.ForeColor;
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			ReleaseThemeRenderer();
			DropDownCalendar.ReleaseThemeRenderer();
			DropDownCalendar.ApplyThemeToFormatStyles();
			_controlStyle.ApplyThemeToFormats();
		}

		// The edit text box is a real control, so it keeps its own colors.
		private void ApplyEditColors()
		{
			if (_textBox != null)
			{
				_textBox.BackColor = GetEditBackColor();
				_textBox.ForeColor = GetEditForeColor();
				ThemeWindow.ApplyNativeLook(_textBox, MxThemeManager.DefaultTheme);
			}
		}

		private void PaintThemedFrame(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool focused = base.Enabled && (base.ContainsFocus || DroppedDown);
			if (focused && (theme.Style.Modern.InputFocus != ThemeFocusKind.Border || theme.Style.Modern.InputFocusWidth > 1))
			{
				theme.DrawInputFrame(graphics, base.ClientRectangle, ThemeRole.BorderFocus, 0, true);
				return;
			}
			DatePickerHelper.DrawRectangleOutline(graphics, GdiCache.Pen(GetThemedBorderColor()), base.ClientRectangle);
		}

		private Color GetThemedBorderColor()
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			if (!base.Enabled)
			{
				return theme.Color(ThemeRole.BorderDisabled);
			}
			if (base.ContainsFocus || DroppedDown)
			{
				return theme.Color(ThemeRole.BorderFocus);
			}
			return theme.Color(GetIsMouseOver() ? ThemeRole.BorderHot : ThemeRole.Border);
		}

		private void DrawThemedCheckBox(Graphics graphics)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			Rectangle checkBoxBounds = _checkBoxButton._bounds;
			Rectangle boxBounds = new Rectangle(checkBoxBounds.Left + (checkBoxBounds.Width - CheckBoxGlyphSize) / 2 + 1, checkBoxBounds.Top + (checkBoxBounds.Height - CheckBoxGlyphSize) / 2, CheckBoxGlyphSize, CheckBoxGlyphSize);
			bool isHot = base.Enabled && _checkBoxButton.GetIsHot();
			ThemeRole fill = !base.Enabled ? ThemeRole.InputBackDisabled : isHot ? ThemeRole.SurfaceHot : ThemeRole.InputBack;
			ThemeRole outline = !base.Enabled ? ThemeRole.BorderDisabled : isHot ? ThemeRole.BorderHot : ThemeRole.BorderStrong;
			graphics.FillRectangle(theme.Brush(fill), boxBounds);
			DatePickerHelper.DrawRectangleOutline(graphics, theme.Pen(outline), boxBounds);
			if (_checkedValue)
			{
				boxBounds.Inflate(-1, -1);
				DrawCheckMark(graphics, boxBounds, theme.Pen(base.Enabled ? ThemeRole.Text : ThemeRole.TextDisabled));
			}
		}
	}
}
