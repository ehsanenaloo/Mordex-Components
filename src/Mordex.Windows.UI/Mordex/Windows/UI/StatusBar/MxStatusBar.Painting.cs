using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.StatusBar
{
	public partial class MxStatusBar
	{
		internal Color GetPanelForeColor()
		{
			return base.ForeColor;
		}

		internal Color GetPanelBackColor()
		{
			if (BackColor == SystemColors.Control)
			{
				return RenderingHelper.GetSchemeColor(ColorName.Control);
			}
			return BackColor;
		}

		private Color GetPanelBorderColor(MxStatusBarPanel uiStatusBarPanel)
		{
			if (uiStatusBarPanel.BorderColor.IsEmpty)
			{
				return PanelsBorderColor;
			}
			return uiStatusBarPanel.BorderColor;
		}

		private TextLayoutRenderer CreateTextLayoutRenderer()
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.SetWordWrap(false);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.SetStringTrimming(StringTrimming.EllipsisCharacter);
			return textLayoutRenderer;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_needsKeyStateRefresh)
			{
				RefreshToggleKeyStates();
			}
			TextLayoutRenderer textLayoutRenderer = CreateTextLayoutRenderer();
			FormatStyle uiFormatStyle = new FormatStyle(PanelsFormatStyle);
			Region region = null;
			Region clip = null;
			if (IsThemed)
			{
				SetIsThemedRendering(false);
				if (_isLayoutDirty)
				{
					LayoutPanels(e.Graphics, textLayoutRenderer, uiFormatStyle);
				}
				PaintThemed(e.Graphics, textLayoutRenderer);
				uiFormatStyle.DisposeResources();
				textLayoutRenderer.ResetCache();
				base.OnPaint(e);
				return;
			}
			if (VisualStyleSupport.IsThemingUsable(false))
			{
				SetIsThemedRendering(true);
			}
			else
			{
				SetIsThemedRendering(false);
			}
			if (_isLayoutDirty)
			{
				LayoutPanels(e.Graphics, textLayoutRenderer, uiFormatStyle);
			}
			if (GetIsThemedRendering())
			{
				if (_currentStylePainter == null)
				{
					_currentStylePainter = GetStatusBarRenderer();
				}
				_currentStylePainter.DrawClassicRebarBackground(e.Graphics, base.ClientRectangle);
			}
			else
			{
				Brush brush = new SolidBrush(GetPanelBackColor());
				if (brush != null)
				{
					e.Graphics.FillRectangle(brush, base.ClientRectangle);
					brush.Dispose();
					brush = null;
				}
			}
			if (uiFormatStyle.ForeColor.IsEmpty)
			{
				uiFormatStyle.ForeColor = GetPanelForeColor();
			}
			if (_sizeGripState != null)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Width = _sizeGripState._bounds.Left + 1;
				region = new Region(clientRectangle);
				clip = e.Graphics.Clip;
				e.Graphics.Clip = region;
			}
			foreach (MxStatusBarPanel item in _panels)
			{
				if (item.Visible)
				{
					Rectangle rectangle = Rectangle.Intersect(e.ClipRectangle, item._bounds);
					if (rectangle.Width > 0 && rectangle.Height > 0)
					{
						DrawPanel(e.Graphics, item, uiFormatStyle, textLayoutRenderer);
					}
				}
			}
			if (region != null)
			{
				region.Dispose();
				region = null;
				e.Graphics.Clip = clip;
				clip.Dispose();
			}
			if (_sizeGripState != null)
			{
				if (_currentStylePainter != null)
				{
					_currentStylePainter.DrawStatusBarGripper(e.Graphics, _sizeGripState._bounds);
				}
				else
				{
					DrawSizeGrip(e.Graphics, uiFormatStyle);
				}
			}
			_currentStylePainter = null;
			uiFormatStyle.DisposeResources();
			textLayoutRenderer.ResetCache();
			base.OnPaint(e);
		}

		private bool DrawPanelImage(Graphics graphics, MxStatusBarPanel uiStatusBarPanel, Color color)
		{
			Rectangle uiStatusBarPanelRectangle = uiStatusBarPanel._imageBounds;
			Size size = GetPanelImageSize(uiStatusBarPanel);
			uiStatusBarPanelRectangle.X += (uiStatusBarPanelRectangle.Width - size.Width) / 2;
			uiStatusBarPanelRectangle.Y += (uiStatusBarPanelRectangle.Height - size.Height) / 2;
			uiStatusBarPanelRectangle.Size = size;
			Image image = uiStatusBarPanel.Image;
			int imageIndex = -1;
			if (image == null && _imageList != null)
			{
				if (uiStatusBarPanel.ImageKey.Length > 0 && _imageList.Images.ContainsKey(uiStatusBarPanel.ImageKey))
				{
					image = _imageList.Images[uiStatusBarPanel.ImageKey];
					imageIndex = _imageList.Images.IndexOfKey(uiStatusBarPanel.ImageKey);
				}
				else if (uiStatusBarPanel.ImageIndex >= 0 && uiStatusBarPanel.ImageIndex < _imageList.Images.Count)
				{
					image = _imageList.Images[uiStatusBarPanel.ImageIndex];
					imageIndex = uiStatusBarPanel.ImageIndex;
				}
			}
			if (image != null || uiStatusBarPanel.Icon != null)
			{
				RenderingHelper.DrawStateImage(graphics, uiStatusBarPanel.Icon, image, imageIndex, ImageList, uiStatusBarPanelRectangle, base.Enabled, false, false, color, GetIsThemedRendering(), null);
				return true;
			}
			return false;
		}

		private void DrawSizeGrip(Graphics graphics, FormatStyle uiFormatStyle)
		{
			Rectangle bounds = _sizeGripState._bounds;
			bounds.Y += 3;
			bounds.X += 2;
			bounds.Height -= 3;
			RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, bounds, bounds);
			Pen darkPen = GdiCache.Pen(RenderingHelper.GetSchemeColor(ColorName.ControlDark));
			Pen lightPen = GdiCache.Pen(ControlPaint.LightLight(RenderingHelper.GetSchemeColor(ColorName.Control)));
			Rectangle gripBounds = _sizeGripState._bounds;
			gripBounds.Width--;
			gripBounds.Height--;
			graphics.DrawLine(darkPen, gripBounds.Right - 2, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 2);
			graphics.DrawLine(darkPen, gripBounds.Right - 3, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 3);
			graphics.DrawLine(lightPen, gripBounds.Right - 4, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 4);
			graphics.DrawLine(darkPen, gripBounds.Right - 6, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 6);
			graphics.DrawLine(darkPen, gripBounds.Right - 7, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 7);
			graphics.DrawLine(lightPen, gripBounds.Right - 8, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 8);
			graphics.DrawLine(darkPen, gripBounds.Right - 10, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 10);
			graphics.DrawLine(darkPen, gripBounds.Right - 11, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 11);
			graphics.DrawLine(lightPen, gripBounds.Right - 12, gripBounds.Bottom - 1, gripBounds.Right - 1, gripBounds.Bottom - 12);
		}

		private void DrawPanel(Graphics graphics, MxStatusBarPanel uiStatusBarPanel, FormatStyle baseStyle, TextLayoutRenderer textLayoutRenderer)
		{
			FormatStyle uiFormatStyle = baseStyle.MergeStyle(uiStatusBarPanel.FormatStyle);
			if (uiStatusBarPanel.GetIsHot())
			{
				uiFormatStyle = uiFormatStyle.MergeStyle(HotPanelsFormatStyle);
				uiFormatStyle = uiFormatStyle.MergeStyle(uiStatusBarPanel.HotFormatStyle);
			}
			if (_currentStylePainter == null)
			{
				RenderingHelper.DrawFormatStyleBackground(graphics, uiFormatStyle, uiStatusBarPanel._bounds, uiStatusBarPanel._bounds);
			}
			if (uiStatusBarPanel.PanelType == StatusBarPanelType.Item || uiStatusBarPanel.PanelType == StatusBarPanelType.ToggleKeyState)
			{
				string text = uiStatusBarPanel.GetDisplayText();
				bool hasImage = DrawPanelImage(graphics, uiStatusBarPanel, uiFormatStyle.BackColor);
				if (text.Length > 0)
				{
					if (uiStatusBarPanel.Alignment == HorizontalAlignment.Left)
					{
						textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					}
					else if (uiStatusBarPanel.Alignment == HorizontalAlignment.Right)
					{
						textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
					}
					else if (!hasImage)
					{
						textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
					}
					else
					{
						textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					}
					Font font = uiFormatStyle.ResolveFont(Font);
					textLayoutRenderer.DrawText(graphics, text, uiStatusBarPanel._contentBounds, font, uiFormatStyle.GetEffectiveForeColor());
				}
			}
			else if (uiStatusBarPanel.PanelType == StatusBarPanelType.ProgressBar)
			{
				int progressBarValue = (int)(100.0 * ProgressFraction.Of(uiStatusBarPanel.ProgressBarValue, uiStatusBarPanel.ProgressBarMinValue, uiStatusBarPanel.ProgressBarMaxValue));
				if (_currentStylePainter != null && VisualStyleSupport.AreVisualStylesEnabled())
				{
					if (progressBarValue > 0)
					{
						_currentStylePainter.DrawProgressBar(graphics, uiStatusBarPanel._bounds, progressBarValue);
					}
				}
				else if (progressBarValue > 0)
				{
					Rectangle progressBounds = uiStatusBarPanel._bounds;
					progressBounds.Inflate(-3, -3);
					progressBounds.Width = progressBounds.Width * progressBarValue / 100;
					Brush brush = GdiCache.Brush(RenderingHelper.GetSchemeColor(ColorName.Highlight));
					graphics.FillRectangle(brush, progressBounds);
				}
			}
			if (uiStatusBarPanel.DrawBorder)
			{
				if (!GetIsThemedRendering())
				{
					Color color = uiFormatStyle.BackColor;
					if (color.IsEmpty)
					{
						color = GetPanelBackColor();
					}
					{
						Pen pen = GdiCache.Pen(GetPanelBorderColor(uiStatusBarPanel));
						RenderingHelper.DrawRectangleOutline(graphics, pen, uiStatusBarPanel._bounds);
						pen = null;
					}
				}
				else
				{
					_currentStylePainter.DrawStatusBarPane(graphics, uiStatusBarPanel._bounds);
				}
			}
			uiFormatStyle.DisposeResources();
		}
	}
}
