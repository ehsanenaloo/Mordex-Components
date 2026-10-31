using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	public partial class MxTaskPane
	{
		internal void DrawGroupBackground(Graphics graphics, TaskPaneGroup taskPaneGroup, Rectangle backgroundBounds, Brush fillBrush, Rectangle clipRect)
		{
			Rectangle visibleRect = Rectangle.Intersect(clipRect, backgroundBounds);
			if (visibleRect.Width <= 0 || visibleRect.Height <= 0)
			{
				return;
			}
			if (fillBrush != null && !TryFillThemedBody(graphics, taskPaneGroup, backgroundBounds, visibleRect, fillBrush))
			{
				graphics.FillRectangle(fillBrush, visibleRect);
			}
			if (taskPaneGroup.BackgroundImage != null)
			{
				switch (taskPaneGroup.BackgroundImageDrawMode)
				{
				case ImageDrawMode.Centered:
					DrawCenteredImage(graphics, taskPaneGroup.BackgroundImage, backgroundBounds, visibleRect);
					break;
				case ImageDrawMode.Stretched:
					DrawStretchedImage(graphics, taskPaneGroup.BackgroundImage, backgroundBounds, visibleRect);
					break;
				case ImageDrawMode.Tiled:
					DrawTiledImage(graphics, taskPaneGroup.BackgroundImage, backgroundBounds, visibleRect);
					break;
				}
			}
		}

		private void PaintGroups(Graphics graphics, Rectangle clipBounds)
		{
			Color empty = Color.Empty;
			bool isHeaderOwnerDrawn = IsAreaOwnerDrawn(OwnerDrawnArea.GroupHeader);
			IsAreaOwnerDrawn(OwnerDrawnArea.Item);
			bool isHandled = false;
			foreach (TaskPaneGroup item in _groups)
			{
				if (!item.Visible)
				{
					continue;
				}
				Rectangle dirtyHeaderBounds = Rectangle.Intersect(clipBounds, item.GetHeaderBounds());
				TaskPaneFormatStyle taskPaneFormatStyle = ComputeGroupFormatStyle(item);
				isHandled = false;
				empty = taskPaneFormatStyle.BackColor;
				int backAlpha = taskPaneFormatStyle.GetEffectiveAlpha(taskPaneFormatStyle.Alpha, taskPaneFormatStyle.BackColorAlphaMode);
				if (backAlpha != 255)
				{
					empty = Color.FromArgb(backAlpha, empty);
				}
				if (!dirtyHeaderBounds.IsEmpty)
				{
					if (isHeaderOwnerDrawn)
					{
						DrawTaskPaneAreaEventArgs e = new DrawTaskPaneAreaEventArgs(graphics, this, TaskPaneArea.GroupHeader, item.GetHeaderBounds(), item);
						OnDrawTaskPaneArea(e);
						isHandled = e.Handled;
					}
					if (!isHandled)
					{
						DrawGroupHeader(graphics, item, item.GetHeaderBounds(), taskPaneFormatStyle, true);
					}
				}
				taskPaneFormatStyle.DisposeDrawingObjects();
				if (item.Expanded && item.Items.Count > 0 && !item._container && !Rectangle.Intersect(clipBounds, item.GetItemsBounds()).IsEmpty)
				{
					TaskPaneStateStyles taskPaneStateStyles = new TaskPaneStateStyles(_itemsStateStyles);
					taskPaneStateStyles = taskPaneStateStyles.MergeWith(item.ItemsStateStyles);
					if (isHeaderOwnerDrawn)
					{
						DrawTaskPaneAreaEventArgs itemsAreaArgs = new DrawTaskPaneAreaEventArgs(graphics, this, TaskPaneArea.Items, item.GetItemsBounds(), item);
						OnDrawTaskPaneArea(itemsAreaArgs);
						isHandled = itemsAreaArgs.Handled;
					}
					if (!isHandled)
					{
						DrawGroupBackground(graphics, item, item.GetItemsBounds(), taskPaneStateStyles.FormatStyle.GetBackBrush(item.GetItemsBounds()), clipBounds);
					}
					DrawGroupItems(graphics, item, clipBounds, taskPaneStateStyles, empty);
					item.WatermarkImage.DrawImage(graphics, item.GetItemsBounds());
					taskPaneStateStyles.DisposeDrawingObjects();
				}
				if (item.Expanded && item.GetItemsBounds() != Rectangle.Empty)
				{
					DrawGroupBorder(graphics, item, item.GetItemsBounds(), empty);
				}
			}
		}

		internal void DrawGroupBorder(Graphics graphics, TaskPaneGroup taskPaneGroup, Rectangle borderBounds, Color borderColor)
		{
			if (!ShouldShowGroupBorder(taskPaneGroup) || !taskPaneGroup._expanded)
			{
				return;
			}
			borderColor = GetTheme().Color(ThemeRole.Border);
			if (TryDrawThemedBodyBorder(graphics, taskPaneGroup, borderBounds, borderColor))
			{
				return;
			}
			Pen borderPen = GdiCache.Pen(borderColor);
			if (!taskPaneGroup.ShowGroupCaption || taskPaneGroup.HeaderHeight == 0)
			{
				if (taskPaneGroup.Container)
				{
					borderBounds.Y--;
					borderBounds.Height++;
				}
				TaskPanePaintHelper.DrawOutline(graphics, borderPen, borderBounds);
			}
			else
			{
				graphics.DrawLine(borderPen, borderBounds.Left, borderBounds.Top, borderBounds.Left, borderBounds.Top + borderBounds.Height - 1);
				graphics.DrawLine(borderPen, borderBounds.Right - 1, borderBounds.Top, borderBounds.Right - 1, borderBounds.Top + borderBounds.Height - 1);
				graphics.DrawLine(borderPen, borderBounds.Left, borderBounds.Top + borderBounds.Height - 1, borderBounds.Right - 1, borderBounds.Top + borderBounds.Height - 1);
			}
		}

		private int ResolveGroupImageIndexByKey(TaskPaneGroup taskPaneGroup)
		{
			if (taskPaneGroup.Image == null && taskPaneGroup.Icon == null)
			{
				try
				{
					if (ImageList != null)
					{
						if (!taskPaneGroup.Enabled || !base.Enabled)
						{
							if (taskPaneGroup.DisabledImageKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneGroup.DisabledImageKey))
							{
								return _imageList.Images.IndexOfKey(taskPaneGroup.DisabledImageKey);
							}
						}
						else if ((taskPaneGroup.GetItemState() == ItemState.Focus || taskPaneGroup.GetItemState() == ItemState.SelectedFocus) && taskPaneGroup.FocusImageKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneGroup.FocusImageKey))
						{
							return _imageList.Images.IndexOfKey(taskPaneGroup.FocusImageKey);
						}
						if (taskPaneGroup.ImageKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneGroup.ImageKey))
						{
							return _imageList.Images.IndexOfKey(taskPaneGroup.ImageKey);
						}
					}
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
					return -1;
				}
			}
			return -1;
		}

		private int ResolveGroupImageIndexByIndex(TaskPaneGroup taskPaneGroup)
		{
			if (taskPaneGroup.Image == null && taskPaneGroup.Icon == null)
			{
				try
				{
					if (_imageList != null)
					{
						if (!taskPaneGroup.Enabled || !base.Enabled)
						{
							if (taskPaneGroup.DisabledImageIndex >= 0 && taskPaneGroup.DisabledImageIndex < _imageList.Images.Count)
							{
								return taskPaneGroup.DisabledImageIndex;
							}
						}
						else if ((taskPaneGroup.GetItemState() == ItemState.Focus || taskPaneGroup.GetItemState() == ItemState.SelectedFocus) && taskPaneGroup.FocusImageIndex >= 0 && taskPaneGroup.FocusImageIndex < _imageList.Images.Count)
						{
							return taskPaneGroup.FocusImageIndex;
						}
						if (taskPaneGroup.ImageIndex >= 0 && taskPaneGroup.ImageIndex < _imageList.Images.Count)
						{
							return taskPaneGroup.ImageIndex;
						}
						if (_defaultGroupImageKey.Length > 0 && _imageList.Images.ContainsKey(_defaultGroupImageKey))
						{
							return _imageList.Images.IndexOfKey(_defaultGroupImageKey);
						}
						if (_defaultGroupImageIndex >= 0 && _defaultGroupImageIndex < _imageList.Images.Count)
						{
							return _defaultGroupImageIndex;
						}
					}
				}
				catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
				{
					ControlFailure.Log(failure);
					return -1;
				}
			}
			return -1;
		}

		private Image ResolveGroupImage(TaskPaneGroup taskPaneGroup)
		{
			if (taskPaneGroup.Image != null)
			{
				return taskPaneGroup.Image;
			}
			try
			{
				if (_imageList != null)
				{
					int imageIndex = ResolveGroupImageIndexByKey(taskPaneGroup);
					if (imageIndex == -1)
					{
						imageIndex = ResolveGroupImageIndexByIndex(taskPaneGroup);
					}
					if (imageIndex >= 0 && imageIndex < _imageList.Images.Count)
					{
						return _imageList.Images[imageIndex];
					}
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
				return null;
			}
			return null;
		}

		private void DrawRecoloredImage(Graphics graphics, Image image, Rectangle bounds, Color tintColor)
		{
			using ImageAttributes imageAttributes = new ImageAttributes();
			ColorMap colorMap = new ColorMap();
			colorMap.NewColor = tintColor;
			colorMap.OldColor = Color.Black;
			ColorMap[] remapTable = new ColorMap[1] { colorMap };
			imageAttributes.SetRemapTable(remapTable);
			Rectangle destRect = new Rectangle(bounds.Left + (bounds.Width - image.Width) / 2, bounds.Top + (bounds.Height - image.Height) / 2, image.Width, image.Height);
			graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
		}

		private void DrawGroupCloseButton(Graphics graphics, TaskPaneGroup taskPaneGroup, TaskPaneFormatStyle taskPaneFormatStyle)
		{
			bool isEnabled = taskPaneGroup.Enabled && base.Enabled;
			bool isHot = isEnabled && taskPaneGroup.IsCloseButtonHot();
			if (!taskPaneGroup.ShowCloseButton || !(taskPaneGroup.GetCloseButtonBounds() != Rectangle.Empty))
			{
				return;
			}
			DrawThemedButtonBackground(graphics, taskPaneGroup.GetCloseButtonBounds(), isHot, taskPaneGroup.IsCloseButtonPressed());
			// The resource manager returns a new bitmap on every call.
			using (Image glyphImage = TaskPanePaintHelper.MakeMagentaTransparent(TaskPaneState.GetCloseBitmap()))
			{
				Rectangle buttonBounds = taskPaneGroup.GetCloseButtonBounds();
				if (glyphImage != null)
				{
					Color glyphColor = isEnabled ? taskPaneFormatStyle.ForeColor : GetGlyphDisabledColor();
					DrawRecoloredImage(graphics, glyphImage, buttonBounds, glyphColor);
				}
			}
		}

		private void DrawExpandButton(Graphics dc, TaskPaneGroup taskPaneGroup, bool hot, TaskPaneFormatStyle taskPaneFormatStyle, bool isExpanded)
		{
			bool isEnabled = taskPaneGroup.Enabled && base.Enabled;
			if (!taskPaneGroup.Expandable || !(taskPaneGroup.GetExpandButtonBounds() != Rectangle.Empty))
			{
				return;
			}
			Image customImage = null;
			if (isExpanded)
			{
				if (_imageList != null)
				{
					if (taskPaneGroup.ExpandButtonImageKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneGroup.ExpandButtonImageKey))
					{
						customImage = _imageList.Images[taskPaneGroup.ExpandButtonImageKey];
					}
					else if (taskPaneGroup.ExpandButtonImageIndex >= 0 && taskPaneGroup.ExpandButtonImageIndex < _imageList.Images.Count)
					{
						customImage = _imageList.Images[taskPaneGroup.ExpandButtonImageIndex];
					}
				}
			}
			else if (_imageList != null)
			{
				if (taskPaneGroup.CollapsedButtonImageKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneGroup.CollapsedButtonImageKey))
				{
					customImage = _imageList.Images[taskPaneGroup.CollapsedButtonImageKey];
				}
				else if (taskPaneGroup.CollapsedButtonImageIndex >= 0 && taskPaneGroup.CollapsedButtonImageIndex < _imageList.Images.Count)
				{
					customImage = _imageList.Images[taskPaneGroup.CollapsedButtonImageIndex];
				}
			}
			if (customImage != null)
			{
				dc.DrawImage(customImage, taskPaneGroup.GetExpandButtonBounds(), 0, 0, taskPaneGroup.GetExpandButtonBounds().Width, taskPaneGroup.GetExpandButtonBounds().Height, GraphicsUnit.Pixel);
				return;
			}
			DrawThemedButtonBackground(dc, taskPaneGroup.GetExpandButtonBounds(), hot, false);
			DrawThemedChevron(dc, taskPaneGroup.GetExpandButtonBounds(), isExpanded, isEnabled ? taskPaneFormatStyle.ForeColor : GetGlyphDisabledColor());
		}

		private void DrawHeaderBackground(Graphics graphics, Rectangle headerBounds, TaskPaneFormatStyle taskPaneFormatStyle, bool isExpanded)
		{
			if (headerBounds.Width <= 0 || headerBounds.Height <= 0)
			{
				return;
			}
			DrawThemedHeaderBackground(graphics, headerBounds, taskPaneFormatStyle, isExpanded);
		}

		private Alignment ComputeAlignment(Alignment alignment)
		{
			if (RightToLeft == RightToLeft.Yes)
			{
				switch (alignment)
				{
				case Alignment.Far:
					alignment = Alignment.Near;
					break;
				case Alignment.Near:
					alignment = Alignment.Far;
					break;
				}
			}
			return alignment;
		}

		private void DrawGroupHeader(Graphics graphics, TaskPaneGroup taskPaneGroup, Rectangle headerBounds, TaskPaneFormatStyle taskPaneFormatStyle, bool shouldDrawText)
		{
			if (!taskPaneGroup.ShowGroupCaption)
			{
				return;
			}
			_ = Rectangle.Empty;
			Rectangle textBounds = headerBounds;
			Rectangle contentBounds = headerBounds;
			bool ownsGraphics = false;
			if (taskPaneGroup.Expandable && taskPaneGroup.GetHeaderBounds().Height > 0 && taskPaneGroup.GetExpandButtonBounds() != Rectangle.Empty)
			{
				contentBounds.Width -= taskPaneGroup.GetHeaderButtonSize();
				if (taskPaneGroup.ResolveExpandButtonAlignment() == ButtonAlignment.Left)
				{
					contentBounds.X += taskPaneGroup.GetHeaderButtonSize();
				}
			}
			if (taskPaneGroup.ShowCloseButton && taskPaneGroup.GetCloseButtonBounds() != Rectangle.Empty)
			{
				contentBounds.Width -= taskPaneGroup.GetCloseButtonBounds().Width;
				if (taskPaneGroup.ComputeButtonAlignment() == ButtonAlignment.Left)
				{
					contentBounds.X += taskPaneGroup.GetCloseButtonBounds().Width;
				}
			}
			textBounds = contentBounds;
			textBounds.Inflate(-2, -2);
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			bool isEnabled = base.Enabled && taskPaneGroup.Enabled;
			bool isHot = false;
			if (isEnabled && (taskPaneGroup.GetItemState() == ItemState.Focus || taskPaneGroup.GetItemState() == ItemState.SelectedFocus))
			{
				isHot = true;
			}
			DrawHeaderBackground(graphics, headerBounds, taskPaneFormatStyle, taskPaneGroup.Expanded);
			if (taskPaneGroup.Expandable)
			{
				DrawExpandButton(graphics, taskPaneGroup, isHot, taskPaneFormatStyle, taskPaneGroup.Expanded);
			}
			if (taskPaneGroup.ShowCloseButton)
			{
				DrawGroupCloseButton(graphics, taskPaneGroup, taskPaneFormatStyle);
			}
			Image groupImage = ResolveGroupImage(taskPaneGroup);
			Rectangle imageBounds = Rectangle.Empty;
			if (groupImage != null || taskPaneGroup.Icon != null)
			{
				Size imageSize = GetGroupImageSize(taskPaneGroup);
				int height = imageSize.Height;
				int width = imageSize.Width;
				int imageTop = 0;
				imageTop = ((headerBounds.Height <= height) ? (headerBounds.Bottom - 2 - height) : ((headerBounds.Height - height) / 2 + headerBounds.Top));
				int x = (contentBounds.Width - width) / 2;
				switch (ComputeAlignment(taskPaneGroup.ImageAlignment))
				{
				case Alignment.Near:
					imageBounds = new Rectangle(contentBounds.X + 5, imageTop, width, height);
					textBounds.X = imageBounds.Right + 6;
					textBounds.Width -= imageBounds.Width + 6;
					break;
				case Alignment.Far:
					imageBounds = new Rectangle(contentBounds.Right - width - 5, imageTop, width, height);
					textBounds.Width -= imageBounds.Width + 9;
					break;
				default:
					imageBounds = new Rectangle(x, imageTop, width, height);
					break;
				}
				if (taskPaneGroup.Icon != null)
				{
					if (isEnabled)
					{
						graphics.DrawIcon(taskPaneGroup.Icon, imageBounds);
					}
					else
					{
						Bitmap disabledIcon = new Bitmap(taskPaneGroup.Icon.ToBitmap(), imageBounds.Size);
						ControlPaint.DrawImageDisabled(graphics, disabledIcon, imageBounds.X, imageBounds.Y, taskPaneFormatStyle.BackColor);
						disabledIcon.Dispose();
						disabledIcon = null;
					}
				}
				else if (isEnabled)
				{
					int imageIndex = ResolveGroupImageIndexByKey(taskPaneGroup);
					if (imageIndex == -1)
					{
						imageIndex = ResolveGroupImageIndexByIndex(taskPaneGroup);
					}
					if (imageIndex >= 0 && _imageList != null && imageIndex < _imageList.Images.Count && _imageList.ImageSize == imageBounds.Size)
					{
						_imageList.Draw(graphics, imageBounds.X, imageBounds.Y, imageBounds.Width, imageBounds.Height, imageIndex);
					}
					else
					{
						graphics.DrawImage(groupImage, imageBounds);
					}
				}
				else
				{
					ControlPaint.DrawImageDisabled(graphics, groupImage, imageBounds.X, imageBounds.Y, taskPaneFormatStyle.BackColor);
				}
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.SetShowHotkeyPrefix(_useMnemonic);
			switch (ComputeAlignment(taskPaneGroup.TextAlignment))
			{
			case Alignment.Near:
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
				break;
			case Alignment.Far:
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
				break;
			default:
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
				break;
			}
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			Font headerFont = taskPaneFormatStyle.ResolveFont(Font);
			if (shouldDrawText && taskPaneFormatStyle.GetForeBrush() != null)
			{
				textLayoutRenderer.DrawTextWithBrush(graphics, taskPaneGroup.Text, textBounds, headerFont, taskPaneFormatStyle.GetForeBrush());
			}
			if (ShowFocusRectangle && base.ContainsFocus && taskPaneGroup == _activeGroup && _isKeyboardNavigating)
			{
				Pen focusPen = new Pen(GetTheme().Color(ThemeRole.BorderFocus));
				focusPen.DashStyle = DashStyle.Dot;
				Rectangle focusBounds = headerBounds;
				focusBounds.Inflate(-2, -2);
				TaskPanePaintHelper.DrawOutline(graphics, focusPen, focusBounds);
				focusPen.Dispose();
				focusPen = null;
			}
			textLayoutRenderer.InvalidateCache();
			taskPaneFormatStyle.DisposeDrawingObjects();
			if (ownsGraphics)
			{
				graphics.Dispose();
				graphics = null;
			}
		}

	}
}
