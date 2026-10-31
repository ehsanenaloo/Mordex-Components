using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class EditBase
	{
		protected override void OnPaint(PaintEventArgs e)
		{
			if (_isTextAlignmentDirty)
			{
				ApplyTextAlignment();
				_isTextAlignmentDirty = false;
			}
			if (_textBox != null && _textBox.BackColor != (base.Enabled ? BackColor : DisabledBackColor))
			{
				// The edit box may have been made before a theme change that this editor did not hear about.
				ApplyThemedTextBoxColors();
			}
			bool condition = false;
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _officeStyle.GetLayoutState().GetEditControlsStyle();
			Rectangle clientRectangle = base.ClientRectangle;
			if (!base.Enabled)
			{
				Brush brush = GdiCache.Brush(DisabledBackColor);
				e.Graphics.FillRectangle(brush, clientRectangle);
			}
			else
			{
				Brush solidBrush = GdiCache.Brush(BackColor);
				e.Graphics.FillRectangle(solidBrush, clientRectangle);
				solidBrush = null;
				if (_textBox == null && GetUseHighlightColor())
				{
					Rectangle otherClientRectangle = base.ClientRectangle;
					otherClientRectangle.Inflate(-2, -2);
					e.Graphics.FillRectangle(SystemBrushes.Highlight, otherClientRectangle);
					condition = true;
				}
			}
			Brush otherSolidBrush = (base.Enabled ? ((!condition) ? GdiCache.Brush(ForeColor) : GdiCache.Brush(SystemColors.HighlightText)) : GdiCache.Brush(DisabledForeColor));
			if (_textBox != null && _textBox.Visible)
			{
				_textArea = _imageArea;
			}
			PaintText(e.Graphics, otherSolidBrush, _textArea);
			otherSolidBrush = null;
			DrawBorder(e.Graphics);
			DrawButtons(e.Graphics);
			base.OnPaint(e);
		}

		protected virtual void PaintText(Graphics g, Brush foreBrush, Rectangle textRect)
		{
			Image image = _displayImage;
			image = _editBaseImage;
			int indexOfKey = -1;
			if (!string.IsNullOrEmpty(_imageKey) && _imageList != null)
			{
				indexOfKey = _imageList.Images.IndexOfKey(_imageKey);
			}
			if (indexOfKey == -1)
			{
				indexOfKey = _imageIndex;
			}
			if (!_hasImage)
			{
				image = null;
				indexOfKey = -1;
			}
			string text = "";
			if (HasText && (_textBox == null || !_textBox.Visible))
			{
				text = Text;
				if (_textBox != null && _textBox.PasswordChar != 0)
				{
					text = new string(_textBox.PasswordChar, text.Length);
				}
			}
			GridHelper.DrawImageAndText(g, foreBrush, text, image, textRect, _imageHorizontalAlignment, _imageVerticalAlignment, ComputeTextLayoutRenderer(), Font, RightToLeft == RightToLeft.Yes, indexOfKey, _imageList, false);
		}

		private void DrawBorder(Graphics graphics)
		{
			Color color = Color.Empty;
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _officeStyle.GetLayoutState().GetControlBorderStyle();
			if (!base.Enabled)
			{
				color = (!mordexColorSchemeAppearanceState._isThemed && !mordexColorSchemeAppearanceState.IsThemedLook) ? FlatBorderColor : mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.DisabledBorder);
			}
			else
			{
				if (_hoverMode == HoverMode.Highlight)
				{
					if (base.ContainsFocus || IsMouseOver())
					{
						color = mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.HotElementBorder);
					}
				}
				if (color.IsEmpty)
				{
					color = ((!mordexColorSchemeAppearanceState._isThemed && !mordexColorSchemeAppearanceState.IsThemedLook) ? FlatBorderColor : mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.ControlBorder));
				}
				if (mordexColorSchemeAppearanceState.IsThemedLook)
				{
					MxTheme theme = MxThemeManager.DefaultTheme;
					color = base.ContainsFocus ? theme.GetColor(ThemeRole.BorderFocus) : IsMouseOver() ? theme.GetColor(ThemeRole.BorderHot) : theme.GetColor(ThemeRole.Border);
				}
			}
			BorderStyle borderStyle = ComputeBorderStyle();
			if (borderStyle == BorderStyle.None)
			{
				return;
			}
			if (borderStyle == BorderStyle.Flat)
			{
				Pen pen = GdiCache.Pen(color);
				Rectangle clientRectangle = base.ClientRectangle;
				ResolvedTheme themed = base.Enabled && base.ContainsFocus ? MxThemeManager.ResolveDefault(DeviceDpi) : null;
				if (themed != null && (themed.Style.Modern.InputFocus != ThemeFocusKind.Border || themed.Style.Modern.InputFocusWidth > 1))
				{
					themed.DrawInputFrame(graphics, clientRectangle, ThemeRole.BorderFocus, 0, true);
				}
				else
				{
					GridHelper.DrawFlatBorder(graphics, pen, clientRectangle);
				}
				if (GetBorderWidth() > 1)
				{
					clientRectangle.Inflate(-1, -1);
					pen = GdiCache.Pen(SystemColors.Window);
					GridHelper.DrawFlatBorder(graphics, pen, clientRectangle);
				}
			}
			else
			{
				_officeStyle.DrawBorder(graphics, base.ClientRectangle, borderStyle, base.Enabled);
			}
		}

		private void RenderContent(Graphics graphics, GridGroupLayoutState item)
		{
			item._isRibbonStyle = _actAsRibbonEditControl;
			CellRendererBase renderer = _officeStyle.GetLayoutState().GetRenderer(item);
			GridFormatStyle style = new GridFormatStyle();
			if (_hoverMode == HoverMode.Highlight)
			{
				item._isHot = base.ContainsFocus || IsMouseOver();
			}
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _officeStyle.GetLayoutState().GetEditControlsStyle();
			Rectangle bounds = item._rect;
			renderer.PaintBackground(graphics, item, style, bounds, selected: false, focused: false);
			renderer.PaintForeground(graphics, item, style, bounds, selected: false, focused: false);
			if (item._type == LayoutItemType.CellColumnButton && item.GetButtonStyle() == Mordex.Windows.DataGrid.ButtonStyle.TextButton)
			{
				TextLayoutRenderer textLayoutRenderer = CreateButtonTextRenderer();
				textLayoutRenderer.DrawTextWithColor(graphics, _buttonCaption, bounds, ButtonFont, _officeStyle.ControlTextColor);
				textLayoutRenderer.ReleaseStringFormat();
			}
			Rectangle otherRectangle = _contentBounds;
			bool firstCondition = false;
			bool controlStyleRenderer2Boolean = false;
			BorderStyle borderStyle = ComputeBorderStyle();
			if (base.Enabled)
			{
				if (_actAsRibbonEditControl)
				{
					firstCondition = borderStyle == BorderStyle.Flat;
					controlStyleRenderer2Boolean = IsMouseOver() || base.ContainsFocus;
				}
				else if (mordexColorSchemeAppearanceState.IsThemedLook)
				{
					firstCondition = borderStyle == BorderStyle.Flat;
					controlStyleRenderer2Boolean = firstCondition || item._isHot || item._isPressed;
				}
				else if (!mordexColorSchemeAppearanceState._isThemed)
				{
					switch (_officeStyle.ButtonAppearance)
					{
					case ButtonAppearance.PopUp:
						switch (borderStyle)
						{
						case BorderStyle.Flat:
							firstCondition = !item._isHot && !item._isPressed;
							controlStyleRenderer2Boolean = true;
							break;
						case BorderStyle.None:
							controlStyleRenderer2Boolean = ((_hoverMode != HoverMode.Highlight && !_actAsRibbonEditControl) ? true : false);
							break;
						default:
							firstCondition = false;
							controlStyleRenderer2Boolean = true;
							break;
						}
						break;
					case ButtonAppearance.Flat:
						firstCondition = borderStyle == BorderStyle.Flat;
						controlStyleRenderer2Boolean = firstCondition || item._isHot || item._isPressed;
						break;
					case ButtonAppearance.FlatBorderless:
						firstCondition = borderStyle == BorderStyle.Flat;
						controlStyleRenderer2Boolean = item._isHot || item._isPressed;
						break;
					default:
						controlStyleRenderer2Boolean = true;
						break;
					}
				}
			}
			else if (mordexColorSchemeAppearanceState.IsThemedLook)
			{
				controlStyleRenderer2Boolean = false;
			}
			else if (!mordexColorSchemeAppearanceState._isThemed)
			{
				switch (_officeStyle.ButtonAppearance)
				{
				case ButtonAppearance.PopUp:
					switch (borderStyle)
					{
					case BorderStyle.Flat:
						firstCondition = !item._isHot && !item._isPressed;
						controlStyleRenderer2Boolean = true;
						break;
					case BorderStyle.None:
						controlStyleRenderer2Boolean = ((_hoverMode != HoverMode.Highlight && !_actAsRibbonEditControl) ? true : false);
						break;
					default:
						firstCondition = false;
						controlStyleRenderer2Boolean = true;
						break;
					}
					break;
				case ButtonAppearance.Flat:
					firstCondition = borderStyle == BorderStyle.Flat;
					controlStyleRenderer2Boolean = firstCondition || item._isHot || item._isPressed;
					break;
				case ButtonAppearance.FlatBorderless:
					firstCondition = borderStyle == BorderStyle.Flat;
					controlStyleRenderer2Boolean = item._isHot || item._isPressed;
					break;
				default:
					controlStyleRenderer2Boolean = true;
					break;
				}
			}
			if (firstCondition)
			{
				otherRectangle.Inflate(0, 1);
				if (RightToLeft == RightToLeft.Yes)
				{
					otherRectangle.X--;
				}
				otherRectangle.Width++;
			}
			if (item._type == LayoutItemType.CellDownButton)
			{
				otherRectangle = Rectangle.FromLTRB(otherRectangle.Left, item._rect.Top, otherRectangle.Right, otherRectangle.Bottom);
			}
			else if (item._type == LayoutItemType.CellUpButton)
			{
				otherRectangle = Rectangle.FromLTRB(otherRectangle.Left, otherRectangle.Top, otherRectangle.Right, item._rect.Bottom);
			}
			if (!controlStyleRenderer2Boolean)
			{
				return;
			}
			ButtonControlRenderer controlStyleRenderer = renderer as ButtonControlRenderer;
			if (controlStyleRenderer != null)
			{
				controlStyleRenderer._controlStyleRenderer2Boolean = controlStyleRenderer2Boolean;
			}
			renderer.PaintAddornments(graphics, item, style, otherRectangle);
			if (controlStyleRenderer != null)
			{
				controlStyleRenderer._controlStyleRenderer2Boolean = false;
				controlStyleRenderer._isLeftToRight = false;
				controlStyleRenderer._isRightToLeft = false;
			}
		}

		private void DrawButtons(Graphics graphics)
		{
			if (_buttonType == EditButtonType.SingleButton)
			{
				if (_buttonStyle == EditButtonStyle.NoButton)
				{
					return;
				}
				ImageListLookup imageListLookup = new ImageListLookup();
				Image image = null;
				GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CellColumnButton, null, null, _buttonStates[0]._bounds);
				gridGroupLayoutState.SetButtonStyle(ComputeButtonStyle(_buttonStyle));
				if (_buttonStyle == EditButtonStyle.Image)
				{
					imageListLookup._currentImage = _buttonImage;
					imageListLookup._imageList = _imageList;
					imageListLookup.SetIndex(_buttonImageIndex);
					imageListLookup.SetKey(_buttonImageKey);
					image = GetButtonImage();
					if (image != null && !_buttonImageSize.IsEmpty && image.Size != _buttonImageSize)
					{
						image = new Bitmap(image, _buttonImageSize);
						imageListLookup._currentImage = image;
					}
					gridGroupLayoutState._cellValue = imageListLookup;
				}
				gridGroupLayoutState._isHot = _buttonStates[0]._isHot;
				gridGroupLayoutState._isPressed = _buttonStates[0]._isPressed;
				if (!base.Enabled || !_buttonEnabled)
				{
					gridGroupLayoutState.SetDisabled(true);
				}
				RenderContent(graphics, gridGroupLayoutState);
			}
			else
			{
				GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.CellUpDownBackground, null, null, _contentBounds);
				groupLayoutState.SetDisabled(!base.Enabled);
				RenderContent(graphics, groupLayoutState);
				if (_hotButtonIndex == 0 || _pressedButtonIndex == 0)
				{
					DrawUpDownButton(graphics, 1);
					DrawUpDownButton(graphics, 0);
				}
				else
				{
					DrawUpDownButton(graphics, 0);
					DrawUpDownButton(graphics, 1);
				}
			}
		}

		private void DrawUpDownButton(Graphics graphics, int value)
		{
			if (Mordex.Windows.Common.MxThemeManager.ResolveDefault(DeviceDpi).DrawDropButton(graphics, _buttonStates[value]._bounds, base.Enabled, _buttonStates[value]._isHot, _buttonStates[value]._isPressed, value))
			{
				return;
			}
			GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CellUpButton, null, null, _buttonStates[value]._bounds);
			if (value == 1)
			{
				gridGroupLayoutState._type = LayoutItemType.CellDownButton;
			}
			gridGroupLayoutState._isHot = _buttonStates[value]._isHot;
			gridGroupLayoutState._isPressed = _buttonStates[value]._isPressed;
			gridGroupLayoutState.SetDisabled(!base.Enabled);
			RenderContent(graphics, gridGroupLayoutState);
		}

		internal TextLayoutRenderer ComputeTextLayoutRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			switch (TextAlignment)
			{
			case TextAlignment.Center:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				break;
			case TextAlignment.Far:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
				break;
			default:
				textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
				break;
			}
			if (EditControl != null && EditControl.Multiline)
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Near);
				textLayoutRenderer.SetWordWrap(true);
			}
			textLayoutRenderer.UpdateRightToLeft(RightToLeft);
			textLayoutRenderer.SetNoPadding(true);
			return textLayoutRenderer;
		}

		private TextLayoutRenderer CreateButtonTextRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateRightToLeft(RightToLeft);
			return textLayoutRenderer;
		}

		MxColorSchemeAppearanceState IGridStyleSource.GetDrawingStyle(GridStyleArea styleArea)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = new MxColorSchemeAppearanceState(!UseCompatibleTextRendering);
			mordexColorSchemeAppearanceState.SetThemedLook(true);
			return mordexColorSchemeAppearanceState;
		}

		public void ResetUseCompatibleTextRendering()
		{
			_useCompatibleTextRendering = null;
			RecalculateLayout();
		}
	}
}
