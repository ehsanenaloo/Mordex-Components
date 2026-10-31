using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxColorPicker
	{
		private ThemeHook _themeHook;

		private bool IsThemed => true;

		private void UpdateThemeListening()
		{
			if (_themeHook == null)
			{
				if (!IsThemed)
				{
					return;
				}
				_themeHook = new ThemeHook(this);
			}
			_themeHook.Update(IsThemed);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateThemeListening();
			// The layout also sets the size of the picker. A picker that is not painted yet (a window off screen, or one
			// drawn into a bitmap) would otherwise keep the default size and cut off its swatches.
			if (_isLayoutDirty)
			{
				RecalculateLayout();
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			_themeHook?.Stop();
			base.OnHandleDestroyed(e);
		}

		// True while the picker is the body of a pop-up menu, as in the drop-down of MxColorButton. It then paints like the
		// menus of the suite: the menu back color, and the selection look of the menu rows for the hot parts.
		internal bool IsInMenu => Parent is CommandControl;

		// Paints the palette like a flat tool bar, or like a menu when it is in one: the swatches and the two buttons show a
		// face only when hot or selected. A back color or a text color that the developer set wins over the theme.
		private void PaintThemed(Graphics graphics, TextLayoutRenderer textLayoutRenderer)
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
			bool menu = IsInMenu;
			Rectangle client = base.ClientRectangle;
			Color back = menu ? theme.MenuBackColor : theme.Color(ThemeRole.Surface);
			if (ShouldSerializeBackColor() && !(menu && BackColor.A == 0))
			{
				back = BackColor;
			}
			graphics.FillRectangle(GdiCache.Brush(back), client);
			int radius = Math.Min(theme.Shape.CornerRadius, 3);
			foreach (ColorEntry item in _colorEntries)
			{
				DrawThemedFace(graphics, theme, item._bounds, item._isHot, item._isPressed, item == _selectedEntry, radius, menu);
				Rectangle swatchBounds = item._bounds;
				swatchBounds.Inflate(-SwatchInset, -SwatchInset);
				graphics.FillRectangle(GdiCache.Brush(item.Color), swatchBounds);
				theme.DrawRounded(graphics, swatchBounds, ThemeRole.BorderStrong, 0);
			}
			Color textColor = !base.Enabled ? theme.Color(ThemeRole.TextDisabled) : ForeColor == SystemColors.ControlText ? theme.Color(ThemeRole.Text) : ForeColor;
			if (_automaticButtonState != null)
			{
				bool selection = DrawThemedFace(graphics, theme, _automaticButtonState._bounds, _automaticButtonState.IsHot(), _automaticButtonState.IsPressed(), _isAutomaticSelected, radius, menu);
				if (!_automaticButtonState._imageBounds.IsEmpty)
				{
					graphics.FillRectangle(GdiCache.Brush(_automaticColor), _automaticButtonState._imageBounds);
					theme.DrawRounded(graphics, _automaticButtonState._imageBounds, ThemeRole.BorderStrong, 0);
				}
				textLayoutRenderer.DrawText(graphics, _automaticButtonState._caption, _automaticButtonState._bounds, Font, SelectionText(theme, selection, textColor));
			}
			if (_moreColorsButtonState != null)
			{
				bool selection = DrawThemedFace(graphics, theme, _moreColorsButtonState._bounds, _moreColorsButtonState.IsHot(), _moreColorsButtonState.IsPressed(), false, radius, menu);
				textLayoutRenderer.DrawText(graphics, _moreColorsButtonState._caption, _moreColorsButtonState._bounds, Font, SelectionText(theme, selection, textColor));
			}
			if (BorderStyle != Mordex.Windows.UI.BorderStyle.None)
			{
				theme.DrawRounded(graphics, client, ThemeRole.Border, 0);
			}
		}

		// An older menu draws its selected row with its own text color; a text color that the developer set still wins.
		private Color SelectionText(ResolvedTheme theme, bool selection, Color textColor)
		{
			return selection && base.Enabled && !theme.MenuSelectionText.IsEmpty && ForeColor == SystemColors.ControlText ? theme.MenuSelectionText : textColor;
		}

		// Draws the face of a hot, pressed or selected part. In a menu a hot or pressed part takes the selection look of the menu
		// rows; the method returns true when that look was an older one with its own text color.
		private static bool DrawThemedFace(Graphics graphics, ResolvedTheme theme, Rectangle bounds, bool hot, bool pressed, bool selected, int radius, bool menu)
		{
			if (!hot && !pressed && !selected)
			{
				return false;
			}
			if (menu && (hot || pressed) && theme.DrawMenuSelection(graphics, bounds))
			{
				return true;
			}
			bool down = pressed || selected;
			theme.FillRounded(graphics, bounds, down ? ThemeRole.SurfacePressed : ThemeRole.SurfaceHot, radius);
			theme.DrawRounded(graphics, bounds, down ? ThemeRole.BorderPressed : ThemeRole.BorderHot, radius);
			return false;
		}
	}
}
