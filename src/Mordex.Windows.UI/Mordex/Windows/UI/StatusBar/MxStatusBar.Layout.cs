using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.StatusBar
{
	public partial class MxStatusBar
	{
		internal void InvalidateLayout()
		{
			_isLayoutDirty = true;
			Invalidate();
		}

		private int MeasureContentWidth(Graphics graphics, MxStatusBarPanel uiStatusBarPanel, TextLayoutRenderer textLayoutRenderer, FormatStyle baseStyle)
		{
			int width = GetPanelImageSize(uiStatusBarPanel).Width;
			string text = uiStatusBarPanel.GetDisplayText();
			if (text.Length > 0)
			{
				FormatStyle uiFormatStyle = baseStyle.MergeStyle(uiStatusBarPanel.FormatStyle);
				Font font = uiFormatStyle.ResolveFont(Font);
				width += (int)textLayoutRenderer.MeasureText(graphics, text, font, 0).Width + ContentPadding;
				uiFormatStyle.DisposeResources();
			}
			if (width + ContentPadding < uiStatusBarPanel.MinWidth)
			{
				return uiStatusBarPanel.MinWidth;
			}
			return width + ContentPadding;
		}

		private int GetPanelWidth(Graphics graphics, MxStatusBarPanel uiStatusBarPanel, TextLayoutRenderer textLayoutRenderer, FormatStyle uiFormatStyle)
		{
			if (uiStatusBarPanel.AutoSize == StatusBarPanelAutoSize.Contents)
			{
				return MeasureContentWidth(graphics, uiStatusBarPanel, textLayoutRenderer, uiFormatStyle);
			}
			if (uiStatusBarPanel.AutoSize == StatusBarPanelAutoSize.Spring)
			{
				return uiStatusBarPanel.MinWidth;
			}
			if (uiStatusBarPanel.MinWidth > uiStatusBarPanel._width)
			{
				return uiStatusBarPanel.MinWidth;
			}
			return uiStatusBarPanel.Width;
		}

		private Size GetPanelImageSize(MxStatusBarPanel uiStatusBarPanel)
		{
			Size result = Size.Empty;
			if (uiStatusBarPanel.Icon != null)
			{
				result = uiStatusBarPanel.Icon.Size;
			}
			else if (uiStatusBarPanel.Image != null)
			{
				result = uiStatusBarPanel.Image.Size;
			}
			else if (_imageList != null)
			{
				if (uiStatusBarPanel.ImageKey.Length > 0 && _imageList.Images.ContainsKey(uiStatusBarPanel.ImageKey))
				{
					result = _imageList.ImageSize;
				}
				else if (uiStatusBarPanel.ImageIndex >= 0 && uiStatusBarPanel.ImageIndex < _imageList.Images.Count)
				{
					result = _imageList.ImageSize;
				}
			}
			if (!result.IsEmpty)
			{
				if (!_imageSize.IsEmpty)
				{
					return _imageSize;
				}
				result.Height += 4;
				result.Width += 6;
			}
			return result;
		}

		private void OnControlResize(object sender, EventArgs eventArgs)
		{
			InvalidateLayout();
		}

		internal void RemovePanelControl(MxStatusBarPanel uiStatusBarPanel)
		{
			if (base.Controls.Contains(uiStatusBarPanel.Control))
			{
				uiStatusBarPanel.Control.Resize -= OnControlResize;
				base.Controls.Remove(uiStatusBarPanel.Control);
			}
		}

		private void LayoutPanel(Graphics graphics, MxStatusBarPanel uiStatusBarPanel, FormatStyle baseStyle, TextLayoutRenderer textLayoutRenderer, int panelLeft)
		{
			int y = 2;
			int panelHeight = base.ClientRectangle.Height - 4;
			uiStatusBarPanel._bounds = new Rectangle(panelLeft, y, uiStatusBarPanel._width, panelHeight);
			if (uiStatusBarPanel.PanelType == StatusBarPanelType.Item || uiStatusBarPanel.PanelType == StatusBarPanelType.ToggleKeyState)
			{
				string text = uiStatusBarPanel.GetDisplayText();
				Size size = GetPanelImageSize(uiStatusBarPanel);
				Rectangle contentBounds = uiStatusBarPanel._bounds;
				if (!size.IsEmpty)
				{
					switch (uiStatusBarPanel.Alignment)
					{
					case HorizontalAlignment.Left:
						uiStatusBarPanel._imageBounds = new Rectangle(panelLeft + 1, y, size.Width, panelHeight);
						contentBounds.Width -= size.Width;
						contentBounds.X += size.Width;
						break;
					case HorizontalAlignment.Center:
						if (text.Length > 0)
						{
							FormatStyle uiFormatStyle = baseStyle.MergeStyle(uiStatusBarPanel.FormatStyle);
							Font font = uiFormatStyle.ResolveFont(Font);
							int textWidth = (int)textLayoutRenderer.MeasureText(graphics, text, font, 0).Width + 2;
							uiFormatStyle.DisposeResources();
							int imageX = panelLeft;
							if (uiStatusBarPanel._bounds.Width > textWidth + size.Width)
							{
								imageX += (uiStatusBarPanel._bounds.Width - textWidth - size.Width) / 2;
							}
							uiStatusBarPanel._imageBounds = new Rectangle(imageX, y, size.Width, panelHeight);
							contentBounds.X += imageX - panelLeft + size.Width;
							contentBounds.Width -= imageX - panelLeft + size.Width;
						}
						else
						{
							int centeredImageX = panelLeft + (uiStatusBarPanel._bounds.Width - size.Width) / 2;
							uiStatusBarPanel._imageBounds = new Rectangle(centeredImageX, y, size.Width, panelHeight);
						}
						break;
					case HorizontalAlignment.Right:
					{
						int rightImageX = uiStatusBarPanel._bounds.Right - size.Width - 1;
						uiStatusBarPanel._imageBounds = new Rectangle(rightImageX, y, size.Width, panelHeight);
						contentBounds.Width -= size.Width;
						break;
					}
					}
				}
				uiStatusBarPanel._contentBounds = contentBounds;
				return;
			}
			uiStatusBarPanel._imageBounds = Rectangle.Empty;
			uiStatusBarPanel._contentBounds = Rectangle.Empty;
			uiStatusBarPanel._bounds = new Rectangle(panelLeft, y, uiStatusBarPanel._width, panelHeight);
			if (uiStatusBarPanel.PanelType == StatusBarPanelType.ControlContainer && uiStatusBarPanel.Control != null)
			{
				uiStatusBarPanel.Control.Resize -= OnControlResize;
				uiStatusBarPanel.Control.Bounds = uiStatusBarPanel._bounds;
				if (!base.Controls.Contains(uiStatusBarPanel.Control))
				{
					base.Controls.Add(uiStatusBarPanel.Control);
				}
				uiStatusBarPanel.Control.Resize += OnControlResize;
			}
		}

		private void LayoutPanels(Graphics graphics, TextLayoutRenderer textLayoutRenderer, FormatStyle uiFormatStyle)
		{
			int availableWidth = base.Width - 3;
			if (CanShowSizeGrip())
			{
				_sizeGripState = new FormatStyleLayoutState();
				availableWidth -= SizeGripWidth - 1;
			}
			else
			{
				_sizeGripState = null;
				availableWidth -= 3;
			}
			int visibleCount = 0;
			if (_panels.Count > 0)
			{
				ArrayList springPanels = new ArrayList();
				foreach (MxStatusBarPanel item in _panels)
				{
					if (item.Visible)
					{
						visibleCount++;
						item._width = GetPanelWidth(graphics, item, textLayoutRenderer, uiFormatStyle);
						if (item.AutoSize == StatusBarPanelAutoSize.Spring)
						{
							springPanels.Add(item);
						}
						availableWidth -= item.Width;
					}
				}
				if (availableWidth > 0 && springPanels.Count > 0)
				{
					if (!GetIsThemedRendering())
					{
						availableWidth -= PanelGap * (visibleCount - 1);
					}
					if (availableWidth > 0)
					{
						int springShare = availableWidth / springPanels.Count;
						int springRemainder = availableWidth - springShare * springPanels.Count;
						foreach (MxStatusBarPanel springPanel in springPanels)
						{
							springPanel._width += springShare;
							if (springRemainder > 0)
							{
								springPanel._width += springRemainder;
								springRemainder = 0;
							}
						}
					}
				}
				int panelLeft = 3;
				foreach (MxStatusBarPanel positionedPanel in _panels)
				{
					if (positionedPanel.Visible)
					{
						LayoutPanel(graphics, positionedPanel, uiFormatStyle, textLayoutRenderer, panelLeft);
						panelLeft = (((GetIsThemedRendering() ? (panelLeft + positionedPanel._width) : (panelLeft + (positionedPanel._width + PanelGap)))));
					}
				}
			}
			if (_sizeGripState != null)
			{
				_sizeGripState._bounds = new Rectangle(base.ClientRectangle.Right - SizeGripWidth, base.ClientRectangle.Top, SizeGripWidth, base.ClientRectangle.Height);
			}
			_isLayoutDirty = false;
		}
	}
}
