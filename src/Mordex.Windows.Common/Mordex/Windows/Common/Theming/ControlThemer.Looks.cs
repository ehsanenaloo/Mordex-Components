using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	// The ordinary buttons, check boxes, radio buttons and edit controls in the older looks. Windows Classic hands the
	// controls to the classic drawing of Windows; the other looks draw the face or the glyph over the standard control.
	public static partial class ControlThemer
	{
		private sealed class LookState
		{
			public bool Hooked;
			public bool Hot;
			public bool Pressed;
			public bool ClassicWanted;
			public bool ClassicSet;
			public bool HandleHooked;
			public bool PageBackSaved;
			public Color PageBack;
			public bool BorderSaved;
			public NativeEditPainter Painter;
			public BorderStyle Border;
			public FlatStyle ComboFlat;
		}

		private static readonly ConditionalWeakTable<Control, LookState> LookStates = new ConditionalWeakTable<Control, LookState>();

		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr window, string subAppName, string subIdList);

		// The body of a Windows XP tab is a light page, not the color of the window.
		private static bool InsideTabPage(Control control)
		{
			for (Control parent = control.Parent; parent != null; parent = parent.Parent)
			{
				if (parent.GetType().Name == "MxTabControl")
				{
					return true;
				}
			}
			return false;
		}

		// Panels, pages and labels inside a tab page take the page color of the theme, and give their own color back later.
		private static void ApplyTabPageBack(Control control, MxTheme theme, bool themed)
		{
			if (!(control is Panel || control is Label || control is UserControl || control is FlowLayoutPanel || control is TableLayoutPanel))
			{
				return;
			}
			LookState state = LookStates.GetValue(control, key => new LookState());
			bool wanted = themed && theme != null && !theme.Style.Detail.TabPageBack.IsEmpty && InsideTabPage(control);
			if (wanted)
			{
				if (!state.PageBackSaved)
				{
					state.PageBack = control.BackColor;
					state.PageBackSaved = true;
				}
				control.BackColor = theme.Style.Detail.TabPageBack;
			}
			else if (state.PageBackSaved)
			{
				control.BackColor = state.PageBack;
				state.PageBackSaved = false;
			}
		}

		// Returns true when the theme paints the face of the button itself; the caller then gives the button a flat face.
		private static bool ApplyControlLook(Control control, MxTheme theme, bool themed)
		{
			bool classicCapable = control is ButtonBase || control is ComboBox || control is TextBoxBase || control is ListBox || control is ListView || control is TreeView || control is TabControl || control is ScrollableControl scroll && scroll.AutoScroll;
			if (!classicCapable)
			{
				return false;
			}
			ThemeChoiceLook look = themed && theme != null ? theme.Style.Detail.ChoiceLook : ThemeChoiceLook.Default;
			LookState state = LookStates.GetValue(control, key => new LookState());
			SetClassic(control, state, look == ThemeChoiceLook.Classic && !(control is ButtonBase) && !(control is TabControl), themed ? theme : null);
			// An edit box that a suite control hosts inside itself draws its frame through the host.
			bool hosted = control.Parent != null && control.Parent.GetType().Namespace != null && control.Parent.GetType().Namespace.StartsWith("Mordex.Windows", StringComparison.Ordinal);
			bool flatEdit = look != ThemeChoiceLook.Default && look != ThemeChoiceLook.Classic && !hosted;
			if (control is TextBox)
			{
				TextBox edit = (TextBox)control;
				if (flatEdit && !state.BorderSaved)
				{
					state.Border = edit.BorderStyle;
					state.BorderSaved = true;
					edit.BorderStyle = BorderStyle.FixedSingle;
					state.Painter = new NativeEditPainter(edit);
				}
				else if (!flatEdit && state.BorderSaved)
				{
					state.Painter?.Detach();
					state.Painter = null;
					edit.BorderStyle = state.Border;
					state.BorderSaved = false;
				}
			}
			else if (control is ComboBox combo)
			{
				if (flatEdit && !state.BorderSaved)
				{
					state.ComboFlat = combo.FlatStyle;
					state.BorderSaved = true;
					combo.FlatStyle = FlatStyle.Flat;
					state.Painter = new NativeEditPainter(combo);
				}
				else if (!flatEdit && state.BorderSaved)
				{
					state.Painter?.Detach();
					state.Painter = null;
					combo.FlatStyle = state.ComboFlat;
					state.BorderSaved = false;
				}
			}
			ButtonBase button = control as ButtonBase;
			bool painted = button != null && look != ThemeChoiceLook.Default && (control is Button || control is CheckBox || control is RadioButton);
			if (painted && !state.Hooked)
			{
				button.Paint += PaintThemedButton;
				button.MouseEnter += LookMouseEnter;
				button.MouseLeave += LookMouseLeave;
				button.MouseDown += LookMouseDown;
				button.MouseUp += LookMouseUp;
				state.Hooked = true;
			}
			else if (!painted && state.Hooked)
			{
				button.Paint -= PaintThemedButton;
				button.MouseEnter -= LookMouseEnter;
				button.MouseLeave -= LookMouseLeave;
				button.MouseDown -= LookMouseDown;
				button.MouseUp -= LookMouseUp;
				state.Hooked = false;
			}
			control.Invalidate();
			return painted;
		}

		private static void SetClassic(Control control, LookState state, bool classic, MxTheme theme)
		{
			state.ClassicWanted = classic;
			if (!state.HandleHooked)
			{
				state.HandleHooked = true;
				// A new handle (also one made again, as a change of the draw mode of a list does) starts with the look of Windows.
				control.HandleCreated += (sender, e) =>
				{
					state.ClassicSet = false;
					PushClassic(control, state, null);
				};
			}
			if (control.IsHandleCreated)
			{
				PushClassic(control, state, theme);
			}
		}

		private static void PushClassic(Control control, LookState state, MxTheme theme)
		{
			if (state.ClassicWanted == state.ClassicSet && !(state.ClassicWanted && !control.IsHandleCreated))
			{
				return;
			}
			if (state.ClassicWanted)
			{
				SetWindowTheme(control.Handle, "", "");
			}
			else
			{
				SetWindowTheme(control.Handle, null, null);
				ThemeWindow.ApplyNativeLook(control, NativeLookFor(control, theme));
			}
			state.ClassicSet = state.ClassicWanted;
		}

		private static void LookMouseEnter(object sender, EventArgs e)
		{
			SetLookState((Control)sender, true, null);
		}

		private static void LookMouseLeave(object sender, EventArgs e)
		{
			SetLookState((Control)sender, false, false);
		}

		private static void LookMouseDown(object sender, MouseEventArgs e)
		{
			SetLookState((Control)sender, null, true);
		}

		private static void LookMouseUp(object sender, MouseEventArgs e)
		{
			SetLookState((Control)sender, null, false);
		}

		private static void SetLookState(Control control, bool? hot, bool? pressed)
		{
			if (LookStates.TryGetValue(control, out LookState state))
			{
				state.Hot = hot ?? state.Hot;
				state.Pressed = pressed ?? state.Pressed;
				control.Invalidate();
			}
		}

		private static void PaintThemedButton(object sender, PaintEventArgs e)
		{
			ButtonBase button = (ButtonBase)sender;
			if (!LookStates.TryGetValue(button, out LookState state))
			{
				return;
			}
			ResolvedTheme theme = MxThemeManager.ResolveDefault(button.DeviceDpi);
			Graphics graphics = e.Graphics;
			bool enabled = button.Enabled;
			bool hot = enabled && state.Hot;
			bool pressed = enabled && hot && state.Pressed;
			Rectangle bounds = button.ClientRectangle;
			if (button is CheckBox || button is RadioButton)
			{
				PaintThemedChoice(graphics, theme, button, bounds, enabled, hot, pressed);
				return;
			}
			ButtonRenderer.DrawParentBackground(graphics, bounds, button);
			bool isDefault = enabled && (button.Focused || (button.FindForm()?.AcceptButton == button));
			ThemeRole fill = !enabled ? ThemeRole.SurfaceDisabled : pressed ? ThemeRole.SurfacePressed : hot ? ThemeRole.SurfaceHot : ThemeRole.SurfaceRaised;
			ThemeRole border = !enabled ? ThemeRole.BorderDisabled : pressed ? ThemeRole.BorderPressed : hot ? ThemeRole.BorderHot : ThemeRole.Border;
			int radius = theme.Radius(ThemeElement.Button);
			if (!theme.DrawButtonLook(graphics, bounds, fill, border, radius, enabled, hot, pressed, isDefault))
			{
				theme.FillRounded(graphics, bounds, fill, radius);
				theme.DrawRounded(graphics, bounds, border, radius);
			}
			Rectangle text = bounds;
			text.Inflate(-theme.Scaled(3), -theme.Scaled(2));
			TextRenderer.DrawText(graphics, button.Text, button.Font, text, theme.Color(enabled ? ThemeRole.Text : ThemeRole.TextDisabled), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			if (button.Focused)
			{
				Rectangle ring = bounds;
				ring.Inflate(-theme.Scaled(4), -theme.Scaled(4));
				ControlPaint.DrawFocusRectangle(graphics, ring);
			}
		}

		// The base paint drew a flat glyph; the glyph column is covered and the glyph of the theme goes on top.
		private static void PaintThemedChoice(Graphics graphics, ResolvedTheme theme, ButtonBase button, Rectangle bounds, bool enabled, bool hot, bool pressed)
		{
			if (!(button is CheckBox || button is RadioButton) || (button is CheckBox box ? box.CheckAlign : ((RadioButton)button).CheckAlign) != ContentAlignment.MiddleLeft)
			{
				return;
			}
			int size = theme.Scaled(13);
			Rectangle column = new Rectangle(0, 0, size + theme.Scaled(3), bounds.Height);
			Rectangle glyph = new Rectangle(theme.Scaled(1), (bounds.Height - size) / 2, size, size);
			graphics.FillRectangle(GdiCache.Brush(button.BackColor), column);
			bool radio = button is RadioButton;
			bool isChecked = radio ? ((RadioButton)button).Checked : ((CheckBox)button).CheckState != CheckState.Unchecked;
			bool indeterminate = !radio && ((CheckBox)button).CheckState == CheckState.Indeterminate;
			theme.DrawChoiceGlyph(graphics, glyph, radio, isChecked, indeterminate, enabled, hot, pressed);
		}
	}
}
