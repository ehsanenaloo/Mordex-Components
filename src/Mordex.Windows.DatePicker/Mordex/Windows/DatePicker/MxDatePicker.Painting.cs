using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		private const int CheckBoxGlyphSize = 11;

		// State of the mouse and focus that decides how the buttons are drawn in one paint pass.
		private struct ButtonPaintState
		{
			internal bool IsHotOrFocused;

			internal bool IsDropDownHot;

			internal bool ShowButtonBorder;
		}

		private ThemeRenderer GetThemeRenderer()
		{
			if (_themeRenderer == null)
			{
				_themeRenderer = new ThemeRenderer();
			}
			return _themeRenderer;
		}

		internal void ReleaseThemeRenderer()
		{
			_themeRenderer = null;
		}

		private void DrawArrow(Graphics graphics, Rectangle rectangle, int direction)
		{
			Color arrowColor = base.Enabled ? GetThemeRenderer()._arrowColor : DisabledForeColor;
			DatePickerHelper.DrawTriangleArrow(graphics, rectangle.Width, rectangle, direction, GdiCache.Pen(arrowColor));
		}

		private bool HasBorderOrHighlight()
		{
			if (BorderStyle != BorderStyle.None)
			{
				return true;
			}
			if (HoverMode == HoverMode.Highlight && (GetIsMouseOver() || base.ContainsFocus || DroppedDown))
			{
				return true;
			}
			return false;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			EnsureCulture();
			if (_layoutPending)
			{
				LayoutElements(e.Graphics);
				_layoutPending = false;
			}
			Color textColor = PaintBackground(e.Graphics);
			PaintText(e.Graphics, textColor);
			ButtonPaintState state = new ButtonPaintState();
			if (HoverMode == HoverMode.Highlight)
			{
				state.IsHotOrFocused = GetIsMouseOver() || base.ContainsFocus;
				state.IsDropDownHot = DroppedDown;
			}
			PaintBorder(e.Graphics);
			PaintCheckBox(e.Graphics);
			state.ShowButtonBorder = state.IsHotOrFocused || state.IsDropDownHot || (_hoverMode == HoverMode.None && BorderStyle != BorderStyle.None);
			if (_showDropDown)
			{
				PaintDropDownButton(e.Graphics, state);
			}
			if (_showUpDown)
			{
				PaintUpButton(e.Graphics, state);
				PaintDownButton(e.Graphics, state);
			}
			_controlStyle.DisposeResources();
			base.OnPaint(e);
		}

		// Fills the control background and returns the color for the text.
		private Color PaintBackground(Graphics graphics)
		{
			Color textColor = GetEditForeColor();
			Rectangle fillBounds = base.ClientRectangle;
			if (HasBorderOrHighlight())
			{
				fillBounds.Inflate(-1, -1);
			}
			if (base.Enabled)
			{
				graphics.FillRectangle(GdiCache.Brush(GetEditBackColor()), fillBounds);
				if (_showCheckBox && !_checkedValue)
				{
					textColor = DisabledForeColor;
				}
			}
			else
			{
				graphics.FillRectangle(GdiCache.Brush(DisabledBackColor), fillBounds);
				textColor = DisabledForeColor;
			}
			return textColor;
		}

		private void PaintText(Graphics graphics, Color textColor)
		{
			using (TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering))
			{
				textLayoutRenderer.RightToLeft = RightToLeft;
				textLayoutRenderer.Alignment = StringAlignment.Near;
				textLayoutRenderer.NoPadding = true;
				if (_editStyle != EditStyle.Masked)
				{
					if (_textBox != null)
					{
						textLayoutRenderer.DrawText(graphics, Text, _textBox.Bounds, Font, textColor);
					}
					return;
				}
				foreach (DateFieldPortion portion in _portions)
				{
					Rectangle portionBounds = portion._bounds;
					string portionText = portion.FormatText(_value, _isNullDate);
					switch (portion.GetPortionType())
					{
					case PortionType.Month:
					case PortionType.Day:
					case PortionType.Year:
					case PortionType.Hour12:
					case PortionType.Hour24:
					case PortionType.Minute:
					case PortionType.Second:
						textLayoutRenderer.Alignment = portionText.Length == 1 ? StringAlignment.Far : StringAlignment.Center;
						break;
					default:
						textLayoutRenderer.Alignment = StringAlignment.Center;
						break;
					}
					if (portion.GetIsActive() && base.ContainsFocus && (!_showCheckBox || _checkedValue))
					{
						ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
						graphics.FillRectangle(theme.Brush(ThemeRole.Selection), portionBounds);
						textLayoutRenderer.DrawText(graphics, portionText, portionBounds, Font, theme.Color(ThemeRole.SelectionText));
					}
					else
					{
						textLayoutRenderer.DrawText(graphics, portionText, portionBounds, Font, textColor);
					}
				}
			}
		}

		private void PaintBorder(Graphics graphics)
		{
			if (HasBorderOrHighlight())
			{
				PaintThemedFrame(graphics);
			}
		}

		private void PaintCheckBox(Graphics graphics)
		{
			if (_showCheckBox)
			{
				DrawThemedCheckBox(graphics);
			}
		}

		private void PaintDropDownButton(Graphics graphics, ButtonPaintState state)
		{
			bool isHot = state.IsHotOrFocused || _dropDownButton.GetIsHot() || DroppedDown;
			bool isPressed = state.IsDropDownHot || _dropDownButton.GetIsPressed() || DroppedDown;
			if (!IsDropDownEnabled())
			{
				graphics.FillRectangle(GdiCache.Brush(DisabledBackColor), _dropDownButton._bounds);
			}
			if (MxThemeManager.ResolveDefault(DeviceDpi).DrawDropButton(graphics, _dropDownButton._bounds, IsDropDownEnabled(), isHot, isPressed, 1))
			{
				return;
			}
			GetThemeRenderer().DrawButtonBackground(graphics, _dropDownButton._bounds, isHot, isPressed, IsDropDownEnabled(), state.ShowButtonBorder, _borderWidth > 0);
			DrawArrow(graphics, _dropDownButton._bounds, 1);
		}

		private void PaintUpButton(Graphics graphics, ButtonPaintState state)
		{
			PaintSpinButton(graphics, state, _upButton, -1, _upButton._bounds);
		}

		private void PaintDownButton(Graphics graphics, ButtonPaintState state)
		{
			PaintSpinButton(graphics, state, _downButton, 1, _downButton._bounds);
		}

		private void PaintSpinButton(Graphics graphics, ButtonPaintState state, PickerButton button, int direction, Rectangle bounds)
		{
			bool isHot = state.IsHotOrFocused || button.GetIsHot();
			if (MxThemeManager.ResolveDefault(DeviceDpi).DrawDropButton(graphics, bounds, base.Enabled, isHot, button.GetIsPressed(), direction < 0 ? 0 : 1))
			{
				return;
			}
			GetThemeRenderer().DrawButtonBackground(graphics, bounds, isHot, button.GetIsPressed(), base.Enabled, state.ShowButtonBorder, _borderWidth > 0);
			DrawArrow(graphics, bounds, direction);
		}

		private void DrawCheckMark(Graphics graphics, Rectangle rectangle, Pen pen)
		{
			rectangle.Inflate(-1, -1);
			int markY = rectangle.Top + 2;
			int markX = rectangle.Left;
			for (int i = 0; i < rectangle.Width; i++)
			{
				graphics.DrawLine(pen, markX, markY, markX, markY + 2);
				markY = (i >= 2) ? (markY - 1) : (markY + 1);
				markX++;
			}
		}
	}
}
