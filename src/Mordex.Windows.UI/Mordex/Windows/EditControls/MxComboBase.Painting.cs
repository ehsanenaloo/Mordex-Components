using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBase
	{
		private void PaintDisabled(Graphics graphics, FormatStyle uiFormatStyle)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (BorderStyle != Mordex.Windows.UI.BorderStyle.None)
			{
				clientRectangle.Inflate(-1, -1);
			}
			RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, clientRectangle, clientRectangle);
			Image image = null;
			int imageIndex = -1;
			if (_hasImage && !_imageBounds.IsEmpty)
			{
				image = GetImage();
				if (image == null && _imageList != null)
				{
					if (_imageKey.Length > 0 && _imageList.Images.ContainsKey(_imageKey))
					{
						image = _imageList.Images[_imageKey];
						imageIndex = _imageList.Images.IndexOfKey(_imageKey);
					}
					else if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
					{
						image = _imageList.Images[_imageIndex];
						imageIndex = _imageIndex;
					}
				}
			}
			if (_hasText && _textBox == null)
			{
				_ = Text;
			}
			Brush brush = uiFormatStyle.GetForeBrush();
			PaintContent(graphics, image, imageIndex, _imageList, brush, uiFormatStyle.ResolveFont(Font));
			if (VisualStyleSupport.IsThemingUsable(false))
			{
				VisualStylePainter.DrawComboDropDownTheme(graphics, _windowsLayoutState._buttonBounds, _windowsLayoutState._isHot, _windowsLayoutState._isPressed, base.Enabled);
				if (BorderStyle != Mordex.Windows.UI.BorderStyle.None)
				{
					VisualStyleSupport.DrawComboBoxBorder(graphics, base.ClientRectangle, base.Enabled, false);
				}
				return;
			}
			if (BorderStyle != Mordex.Windows.UI.BorderStyle.None)
			{
				{
					Pen pen = GdiCache.Pen(FlatBorderColor);
					RenderingHelper.DrawRectangleOutline(graphics, pen, base.ClientRectangle);
				}
			}
			Pen arrowPen = GdiCache.Pen(RenderingHelper.GetSchemeColor(ColorName.GrayText));
			RenderingHelper.DrawUpDownTriangle(graphics, DisabledArrowSize, _windowsLayoutState._buttonBounds, 1, arrowPen);
		}

		internal StateStyles BuildStateStyles()
		{
			StateStyles uiStateStyles = new StateStyles();
			uiStateStyles.FormatStyle.ForeColor = GetEffectiveForeColor();
			uiStateStyles.FormatStyle.BackColor = _windowsAppearanceState._backColor;
			uiStateStyles.FormatStyle.BackColorGradient = _windowsAppearanceState._backGradientColor;
			uiStateStyles.HotFormatStyle.BackColor = _windowsAppearanceState._hotBackColor;
			uiStateStyles.PressedFormatStyle.BackColor = _windowsAppearanceState._pressedBackColor;
			uiStateStyles.SelectedFormatStyle.BackColor = _windowsAppearanceState._selectedBackColor;
			uiStateStyles.DisabledFormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.GrayText);
			uiStateStyles.DisabledFormatStyle.BackColor = RenderingHelper.GetSchemeColor(ColorName.DisabledColor);
			return uiStateStyles.CreateMergedStateStyles(StateStyles);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_isLayoutDirty)
			{
				LayoutContent(e.Graphics);
			}
			if (IsThemed)
			{
				PaintThemed(e.Graphics);
				base.OnPaint(e);
				return;
			}
			bool needsParentBackground = UsesNativeTheme();
			if (BorderStyle != Mordex.Windows.UI.BorderStyle.None && needsParentBackground)
			{
				Rectangle bounds = GetButtonBounds();
				bounds.Inflate(1, 1);
				ButtonRenderer.DrawParentBackground(e.Graphics, bounds, this);
			}
			StateStyles uiStateStyles = BuildStateStyles();
			FormatStyle uiFormatStyle = new FormatStyle(uiStateStyles.FormatStyle);
			bool isTextHighlighted = false;
			if (!base.Enabled && !UsesNativeTheme())
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.DisabledFormatStyle);
				PaintDisabled(e.Graphics, uiFormatStyle);
				base.OnPaint(e);
				return;
			}
			bool isHot = IsButtonHot() || DroppedDown;
			bool isPressed = DroppedDown || IsButtonPressed();
			if (!isHot && HoverMode == HoverMode.Highlight)
			{
				isHot = IsMouseOver() || base.ContainsFocus;
			}
			Brush brush = GdiCache.Brush(GetEffectiveBackColor());
			Rectangle clientRectangle = base.ClientRectangle;
			if (BorderStyle != Mordex.Windows.UI.BorderStyle.None)
			{
				clientRectangle.Inflate(-1, -1);
			}
			e.Graphics.FillRectangle(brush, clientRectangle);
			uiFormatStyle = ApplyStateStyle(uiStateStyles, uiFormatStyle, isHot, isPressed);
			if ((GetEditStyle() == EditStyle.DropDownList || !base.ContainsFocus) && !_droppedDown)
			{
				Rectangle highlightBounds = base.ClientRectangle;
				highlightBounds.Inflate(-3, -3);
				if (RightToLeft == RightToLeft.Yes)
				{
					highlightBounds.X += _windowsLayoutState._buttonBounds.Width + 1;
				}
				highlightBounds.Width -= _windowsLayoutState._buttonBounds.Width + 1;
				if (VisualStyleSupport.IsThemingUsable(false))
				{
					if (RightToLeft == RightToLeft.Yes)
					{
						highlightBounds.X -= 2;
					}
					highlightBounds.Width += 2;
				}
				if (base.ContainsFocus && !UsesNativeTheme())
				{
					e.Graphics.FillRectangle(SystemBrushes.Highlight, highlightBounds);
					isTextHighlighted = true;
				}
			}
			bool shouldDrawButton = true;
			Image image = null;
			int imageIndex = -1;
			if (_hasImage && !_imageBounds.IsEmpty)
			{
				image = GetImage();
				if (image == null && _imageList != null)
				{
					if (_imageKey.Length > 0 && _imageList.Images.ContainsKey(_imageKey))
					{
						image = _imageList.Images[_imageKey];
						imageIndex = _imageList.Images.IndexOfKey(_imageKey);
					}
					else if (_imageIndex >= 0 && _imageIndex < _imageList.Images.Count)
					{
						image = _imageList.Images[_imageIndex];
						imageIndex = _imageIndex;
					}
				}
			}
			if (_hasText && (_textBox == null || !_textBox.Visible))
			{
				_ = Text;
			}
			Brush textBrush = ((!isTextHighlighted) ? new SolidBrush(GetEffectiveForeColor()) : new SolidBrush(SystemColors.HighlightText));
			bool hasBorder = true;
			if (BorderStyle == Mordex.Windows.UI.BorderStyle.None)
			{
				hasBorder = HoverMode != HoverMode.None && (isHot || isPressed);
			}
			PaintContent(e.Graphics, image, imageIndex, ImageList, textBrush, uiFormatStyle.ResolveFont(Font));
			if ((VisualStyleSupport.IsThemingUsable(false)))
			{
				shouldDrawButton = UsesNativeTheme() || !VisualStylePainter.DrawComboDropDownTheme(e.Graphics, _windowsLayoutState._buttonBounds, _windowsLayoutState._isHot, _windowsLayoutState._isPressed, base.Enabled);
			}
			if (hasBorder)
			{
				if (VisualStyleSupport.IsThemingUsable(false))
				{
					if (hasBorder)
					{
						if (UsesNativeTheme())
						{
							GetOrCreateComboThemeRenderer().DrawDropDownListButton(e.Graphics, base.ClientRectangle, ComboStyle, base.Enabled, _windowsLayoutState._isHot || IsMouseOver(), DroppedDown);
							GetOrCreateComboThemeRenderer().DrawDropDownBorder(e.Graphics, base.ClientRectangle, ComboStyle, base.Enabled, _windowsLayoutState._isHot || IsMouseOver());
							if (ComboStyle == ComboStyle.DropDownList)
							{
								GetOrCreateComboThemeRenderer().DrawDropDownArrow(e.Graphics, _windowsLayoutState._buttonBounds, base.Enabled);
								shouldDrawButton = false;
								if (base.ContainsFocus)
								{
									using (Pen pen = new Pen(SystemColors.WindowFrame))
									{
										pen.DashStyle = DashStyle.Dot;
										RenderingHelper.DrawRectangleOutline(e.Graphics, pen, _textBounds);
									}
								}
							}
							PaintContent(e.Graphics, image, imageIndex, ImageList, textBrush, uiFormatStyle.ResolveFont(Font));
						}
						else
						{
							VisualStyleSupport.DrawComboBoxButtonBorder(e.Graphics, base.ClientRectangle, base.Enabled, _windowsLayoutState._isHot || (ComboStyle == ComboStyle.DropDownList && IsMouseOver()));
						}
					}
				}
				else if (HoverMode == HoverMode.Highlight && (isHot || isPressed))
				{
					Pen hoverBorderPen = GdiCache.Pen(uiFormatStyle.GetStateBorderColor(isHot, isPressed));
					RenderingHelper.DrawRectangleOutline(e.Graphics, hoverBorderPen, base.ClientRectangle);
				}
				else
				{
					Pen defaultBorderPen = GdiCache.Pen(FlatBorderColor);
					RenderingHelper.DrawRectangleOutline(e.Graphics, defaultBorderPen, base.ClientRectangle);
				}
			}
			if (shouldDrawButton)
			{
				if (UsesNativeTheme())
				{
					shouldDrawButton = !GetOrCreateComboThemeRenderer().DrawDropDownButton(e.Graphics, _windowsLayoutState._buttonBounds, RightToLeft == RightToLeft.Yes, base.Enabled, _windowsLayoutState._isPressed || DroppedDown, _windowsLayoutState._isHot);
				}
				else
				{
					DrawDropDownButton(e.Graphics, uiFormatStyle, isHot, isPressed, hasBorder, e.ClipRectangle);
				}
			}
			if (_textLayoutRenderer != null)
			{
				_textLayoutRenderer.ResetCache();
				_textLayoutRenderer = null;
			}
			if (_themeRenderer != null)
			{
				_themeRenderer.DisposeResources();
				_themeRenderer = null;
			}
			if (textBrush != null)
			{
				textBrush.Dispose();
				textBrush = null;
			}
			uiFormatStyle.DisposeResources();
			base.OnPaint(e);
		}

		private FormatStyle ApplyStateStyle(StateStyles uiStateStyles, FormatStyle uiFormatStyle, bool isHot, bool isPressed)
		{
			if (isHot)
			{
				uiFormatStyle = ((!isPressed) ? uiFormatStyle.MergeStyle(uiStateStyles.HotFormatStyle) : uiFormatStyle.MergeStyle(uiStateStyles.PressedFormatStyle));
			}
			else if (isPressed)
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStateStyles.SelectedFormatStyle);
			}
			return uiFormatStyle;
		}

		private void PaintContent(Graphics graphics, Image image, int imageIndex, ImageList imageList, Brush brush, Font font)
		{
			if (image != null)
			{
				RenderingHelper.DrawStateImage(graphics, null, image, imageIndex, imageList, _imageBounds, base.Enabled, false, false, GetEffectiveBackColor(), false, null);
			}
			if (HasText && Text.Length > 0 && brush != null && !_textBounds.IsEmpty)
			{
				Rectangle rectangle = _textBounds;
				if (RightToLeft != RightToLeft.Yes)
				{
					rectangle.Offset(-1, 0);
				}
				GetOrCreateTextLayoutRenderer().DrawTextWithBrush(graphics, Text, rectangle, font, brush);
			}
		}

		private void DrawDropDownButton(Graphics graphics, FormatStyle uiFormatStyle, bool hot, bool pressed, bool hasBorder, Rectangle clipRectangle)
		{
			Rectangle buttonBounds = _windowsLayoutState._buttonBounds;
			if (buttonBounds.Height <= 0 || buttonBounds.Height <= 0)
			{
				return;
			}
			if (pressed || hot)
			{
				RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, clipRectangle, buttonBounds);
				Pen hotBorderPen = GdiCache.Pen(uiFormatStyle.GetStateBorderColor(hot, pressed));
				if (hasBorder)
				{
					buttonBounds.Inflate(1, 1);
				}
				else if (RightToLeft == RightToLeft.Yes)
				{
					buttonBounds.X++;
					buttonBounds.Width++;
				}
				else
				{
					buttonBounds.X--;
					buttonBounds.Width++;
				}
				RenderingHelper.DrawRectangleOutline(graphics, hotBorderPen, buttonBounds);
				hotBorderPen = null;
			}
			else if (HoverMode == HoverMode.None)
			{
				buttonBounds.Inflate(1, 1);
				RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, clipRectangle, buttonBounds);
				Pen inactiveBorderPen = GdiCache.Pen(FlatBorderColor);
				RenderingHelper.DrawRectangleOutline(graphics, inactiveBorderPen, buttonBounds);
				inactiveBorderPen = null;
			}
			else
			{
				RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, clipRectangle, buttonBounds);
			}
			Color color = uiFormatStyle.ForeColor;
			Pen arrowPen = GdiCache.Pen(color);
			int arrowWidth = buttonBounds.Width - 2;
			RenderingHelper.DrawUpDownTriangle(graphics, arrowWidth, buttonBounds, 1, arrowPen);
		}
	}
}
