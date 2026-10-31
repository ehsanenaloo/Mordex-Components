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
		private int ResolveItemImageIndexByIndex(TaskPaneItem taskPaneItem)
		{
			if (taskPaneItem.ItemType != ItemType.LinkButton)
			{
				return -1;
			}
			if (taskPaneItem.Image == null && taskPaneItem.Icon == null)
			{
				try
				{
					if (_imageList != null)
					{
						if (!taskPaneItem.Enabled || !base.Enabled || !taskPaneItem.Group.Enabled)
						{
							if (taskPaneItem.ImageDisabledIndex >= 0 && taskPaneItem.ImageDisabledIndex < _imageList.Images.Count)
							{
								return taskPaneItem.ImageDisabledIndex;
							}
						}
						else if (taskPaneItem == _selectedItem)
						{
							if (taskPaneItem.ImageSelectedIndex >= 0 && taskPaneItem.ImageSelectedIndex < _imageList.Images.Count)
							{
								return taskPaneItem.ImageSelectedIndex;
							}
						}
						else if ((taskPaneItem._itemState == ItemState.Focus || taskPaneItem._itemState == ItemState.SelectedFocus) && taskPaneItem.ImageFocusIndex >= 0 && taskPaneItem.ImageFocusIndex < _imageList.Images.Count)
						{
							return taskPaneItem.ImageFocusIndex;
						}
						if (taskPaneItem.ImageIndex >= 0 && taskPaneItem.ImageIndex < _imageList.Images.Count)
						{
							return taskPaneItem.ImageIndex;
						}
						if (_defaultItemImageKey.Length > 0 && _imageList.Images.ContainsKey(_defaultItemImageKey))
						{
							return _imageList.Images.IndexOfKey(_defaultItemImageKey);
						}
						if (_defaultItemImageIndex >= 0 && _defaultItemImageIndex < _imageList.Images.Count)
						{
							return _defaultItemImageIndex;
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

		private int ResolveItemImageIndexByKey(TaskPaneItem taskPaneItem)
		{
			if (taskPaneItem.ItemType != ItemType.LinkButton)
			{
				return -1;
			}
			if (taskPaneItem.Image == null && taskPaneItem.Icon == null)
			{
				try
				{
					if (_imageList != null)
					{
						if (!taskPaneItem.Enabled || !base.Enabled || !taskPaneItem.Group.Enabled)
						{
							if (taskPaneItem.ImageDisabledKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneItem.ImageDisabledKey))
							{
								return _imageList.Images.IndexOfKey(taskPaneItem.ImageDisabledKey);
							}
						}
						else if (taskPaneItem == _selectedItem)
						{
							if (taskPaneItem.ImageSelectedKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneItem.ImageSelectedKey))
							{
								return _imageList.Images.IndexOfKey(taskPaneItem.ImageSelectedKey);
							}
						}
						else if ((taskPaneItem._itemState == ItemState.Focus || taskPaneItem._itemState == ItemState.SelectedFocus) && taskPaneItem.ImageFocusKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneItem.ImageFocusKey))
						{
							return _imageList.Images.IndexOfKey(taskPaneItem.ImageFocusKey);
						}
						if (taskPaneItem.ImageKey.Length > 0 && _imageList.Images.ContainsKey(taskPaneItem.ImageKey))
						{
							return _imageList.Images.IndexOfKey(taskPaneItem.ImageKey);
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

		private Image GetItemImage(TaskPaneItem taskPaneItem)
		{
			if (taskPaneItem.ItemType != ItemType.LinkButton)
			{
				return null;
			}
			if (taskPaneItem.Image != null)
			{
				return taskPaneItem.Image;
			}
			try
			{
				if (_imageList != null)
				{
					int imageIndex = ResolveItemImageIndexByKey(taskPaneItem);
					if (imageIndex == -1)
					{
						imageIndex = ResolveItemImageIndexByIndex(taskPaneItem);
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

		private void DrawItem(Graphics graphics, TaskPaneItem taskPaneItem, Rectangle itemBounds, TaskPaneFormatStyle taskPaneFormatStyle, bool shouldDrawText)
		{
			_ = Rectangle.Empty;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.SetShowHotkeyPrefix(_useMnemonic);
			itemBounds.Inflate(-1, 0);
			Rectangle textBounds = itemBounds;
			Rectangle imageBounds = itemBounds;
			textBounds.X += 2;
			textBounds.Width -= 2;
			if (taskPaneItem.Indent != 0)
			{
				textBounds.X += taskPaneItem.Indent;
				textBounds.Width -= taskPaneItem.Indent;
			}
			int imageHeight = 0;
			int imageWidth = 0;
			Size empty = Size.Empty;
			Image itemImage = GetItemImage(taskPaneItem);
			bool isEnabled = taskPaneItem.Enabled && base.Enabled && taskPaneItem.Group.Enabled;
			int contentRight = 0;
			int x = itemBounds.X;
			Alignment alignment = ComputeAlignment(taskPaneItem.TextAlignment);
			Alignment imageAlignment = ComputeAlignment(taskPaneItem.ImageAlignment);
			if (taskPaneItem.ItemType == ItemType.LinkButton)
			{
				if (itemImage != null || taskPaneItem.Icon != null)
				{
					empty = GetItemImageSize(taskPaneItem);
					imageHeight = empty.Height;
					imageWidth = empty.Width;
					int imageTop = 0;
					imageTop = ((itemBounds.Height <= imageHeight) ? (itemBounds.Bottom - 2 - imageHeight) : ((itemBounds.Height - imageHeight) / 2 + itemBounds.Top));
					switch (imageAlignment)
					{
					case Alignment.Near:
						imageBounds = new Rectangle(itemBounds.Left + 5 + taskPaneItem.Indent, imageTop, imageWidth, imageHeight);
						if (alignment == Alignment.Near)
						{
							textBounds.X = imageBounds.Right + 4;
							textBounds.Width -= imageBounds.Width + 9;
						}
						break;
					case Alignment.Far:
						imageBounds = new Rectangle(itemBounds.Right - imageWidth - 5, imageTop, imageWidth, imageHeight);
						if (alignment == Alignment.Far)
						{
							textBounds.Width -= imageBounds.Width + 9;
						}
						break;
					default:
					{
						int centeredImageX = itemBounds.Left + taskPaneItem.Indent + (itemBounds.Width - imageWidth) / 2;
						imageBounds = new Rectangle(centeredImageX, imageTop, imageWidth, imageHeight);
						break;
					}
					}
					x = imageBounds.Left;
					contentRight = imageBounds.Right;
				}
				if (isEnabled && taskPaneItem._itemState == ItemState.Focus && taskPaneFormatStyle._fontUnderline == TriState.Empty)
				{
					taskPaneFormatStyle.SetFontUnderline(TriState.True);
				}
			}
			else if (taskPaneItem.ItemType == ItemType.Description && taskPaneFormatStyle.ForeColor == _itemsStateStyles.FormatStyle._defaultForeColor)
			{
				taskPaneFormatStyle.ForeColor = GetTheme().Color(ThemeRole.Text);
			}
			Brush backBrush = taskPaneFormatStyle.GetBackBrush(itemBounds);
			if (backBrush != null)
			{
				graphics.FillRectangle(backBrush, itemBounds);
			}
			if (taskPaneItem.Icon != null && taskPaneItem.ItemType == ItemType.LinkButton)
			{
				TaskPanePaintHelper.DrawIcon(graphics, taskPaneItem.Icon, imageBounds, isEnabled, taskPaneFormatStyle.BackColor);
			}
			else if (itemImage != null)
			{
				if (isEnabled)
				{
					int imageListIndex = ResolveItemImageIndexByKey(taskPaneItem);
					if (imageListIndex == -1)
					{
						imageListIndex = ResolveItemImageIndexByIndex(taskPaneItem);
					}
					if (imageListIndex >= 0 && _imageList != null && imageListIndex < _imageList.Images.Count && _imageList.ImageSize == imageBounds.Size)
					{
						_imageList.Draw(graphics, imageBounds.X, imageBounds.Y, imageBounds.Width, imageBounds.Height, imageListIndex);
					}
					else
					{
						graphics.DrawImage(itemImage, imageBounds);
					}
				}
				else
				{
					Bitmap disabledBitmap = new Bitmap(itemImage, imageBounds.Size);
					ControlPaint.DrawImageDisabled(graphics, disabledBitmap, imageBounds.X, imageBounds.Y, taskPaneFormatStyle.BackColor);
					disabledBitmap.Dispose();
					disabledBitmap = null;
				}
			}
			switch (alignment)
			{
			case Alignment.Near:
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
				break;
			case Alignment.Far:
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
				contentRight = itemBounds.Right;
				break;
			default:
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
				contentRight = itemBounds.Right;
				break;
			}
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			Font textFont = taskPaneFormatStyle.ResolveFont(Font);
			textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			textLayoutRenderer.SetWordWrapped(taskPaneItem.IsWordWrapped());
			if (taskPaneItem.ItemType != ItemType.Separator && shouldDrawText)
			{
				if (isEnabled)
				{
					Brush textBrush = taskPaneFormatStyle.GetForeBrush();
					if (textBrush != null)
					{
						textLayoutRenderer.DrawTextWithBrush(graphics, taskPaneItem.Text, textBounds, textFont, textBrush);
					}
					if (ShowFocusRectangle && base.ContainsFocus && taskPaneItem == _activeItem && _isKeyboardNavigating)
					{
						Pen focusPen = new Pen(SystemColors.ControlText);
						int textWidth = (int)textLayoutRenderer.MeasureText(graphics, taskPaneItem.Text, textFont, taskPaneItem.Bounds.Width).Width;
						Rectangle bounds = taskPaneItem.Bounds;
						bounds.Width = textWidth;
						bounds.X = x;
						switch (alignment)
						{
						case Alignment.Near:
							bounds.X = itemBounds.X + 2;
							break;
						case Alignment.Far:
						{
							int rightAlignedX = textBounds.Right - textWidth;
							if (rightAlignedX < bounds.X)
							{
								bounds.X = rightAlignedX;
							}
							break;
						}
						default:
							bounds = taskPaneItem.Bounds;
							break;
						}
						if (contentRight > bounds.Right)
						{
							bounds.Width = contentRight - bounds.Left;
						}
						bounds.Inflate(-1, -1);
						focusPen.DashStyle = DashStyle.Dot;
						TaskPanePaintHelper.DrawOutline(graphics, focusPen, bounds);
						focusPen.Dispose();
						focusPen = null;
					}
				}
				else
				{
					Brush disabledTextBrush = taskPaneFormatStyle.GetForeBrush();
					if (disabledTextBrush != null)
					{
						textLayoutRenderer.DrawTextWithBrush(graphics, taskPaneItem.Text, textBounds, textFont, disabledTextBrush);
					}
				}
			}
			taskPaneFormatStyle.DisposeDrawingObjects();
			textLayoutRenderer.InvalidateCache();
		}

		internal void DrawDefaultArea(Graphics graphics, ExplorerArea explorerArea, object areaTarget)
		{
			if (explorerArea == ExplorerArea.Background)
			{
				DrawClientBackground(graphics, base.ClientRectangle, base.ClientRectangle);
				return;
			}
			TaskPaneGroup taskPaneGroup = null;
			TaskPaneItem taskPaneItem = null;
			if (explorerArea == ExplorerArea.Items)
			{
				taskPaneItem = (TaskPaneItem)areaTarget;
				taskPaneGroup = taskPaneItem.Group;
			}
			else
			{
				taskPaneGroup = (TaskPaneGroup)areaTarget;
			}
			if (explorerArea == ExplorerArea.GroupHeader)
			{
				TaskPaneFormatStyle taskPaneFormatStyle = null;
				taskPaneFormatStyle = ((!taskPaneGroup.SpecialGroup) ? new TaskPaneFormatStyle(_groupsFormatStyle) : new TaskPaneFormatStyle(_specialGroupsFormatStyle));
				TaskPaneFormatStyle headerStyle = ComputeGroupFormatStyle(taskPaneGroup);
				DrawGroupHeader(graphics, taskPaneGroup, taskPaneGroup.GetHeaderBounds(), headerStyle, false);
				headerStyle.DisposeDrawingObjects();
				taskPaneFormatStyle.DisposeDrawingObjects();
				return;
			}
			TaskPaneFormatStyle itemsStyle = new TaskPaneFormatStyle(_itemsFormatStyle);
			itemsStyle = itemsStyle.MergeWith(taskPaneGroup.ItemsStateStyles.FormatStyle);
			if (explorerArea == ExplorerArea.Items)
			{
				DrawGroupBackground(graphics, taskPaneGroup, taskPaneGroup.GetItemsBounds(), itemsStyle.GetBackBrush(taskPaneGroup.GetItemsBounds()), taskPaneGroup.GetItemsBounds());
				taskPaneGroup.WatermarkImage.DrawImage(graphics, taskPaneGroup.GetItemsBounds());
			}
			else
			{
				TaskPaneStateStyles taskPaneStateStyles = new TaskPaneStateStyles(_itemsStateStyles);
				_itemsStateStyles = taskPaneStateStyles.MergeWith(taskPaneItem.Group.ItemsStateStyles);
				TaskPaneFormatStyle itemStyle = ComputeItemFormatStyle(taskPaneItem, taskPaneStateStyles);
				DrawItem(graphics, taskPaneItem, taskPaneItem.Bounds, itemStyle, false);
				itemStyle.DisposeDrawingObjects();
			}
			itemsStyle.DisposeDrawingObjects();
		}

		private void DrawGroupItems(Graphics graphics, TaskPaneGroup taskPaneGroup, Rectangle clipRect, TaskPaneStateStyles taskPaneStateStyles, Color backColor)
		{
			if (Groups.Count <= 0)
			{
				return;
			}
			bool isHandled = false;
			Rectangle visibleRect = Rectangle.Intersect(clipRect, taskPaneGroup.GetItemsBounds());
			_ = taskPaneGroup.GetItemsBounds().Height;
			bool isOwnerDrawn = IsAreaOwnerDrawn(OwnerDrawnArea.Item);
			Region clip = graphics.Clip;
			Region itemsRegion = (graphics.Clip = new Region(taskPaneGroup.GetItemsBounds()));
			foreach (TaskPaneItem item in taskPaneGroup.Items)
			{
				if (item.Visible && !item.Container && !Rectangle.Intersect(visibleRect, item.Bounds).IsEmpty)
				{
					TaskPaneFormatStyle taskPaneFormatStyle = ComputeItemFormatStyle(item, taskPaneStateStyles);
					isHandled = false;
					if (isOwnerDrawn)
					{
						DrawTaskPaneAreaEventArgs e = new DrawTaskPaneAreaEventArgs(graphics, this, TaskPaneArea.Items, item.Bounds, item);
						OnDrawTaskPaneArea(e);
						isHandled = e.Handled;
					}
					if (!isHandled)
					{
						DrawItem(graphics, item, item.Bounds, taskPaneFormatStyle, true);
					}
					taskPaneFormatStyle.DisposeDrawingObjects();
				}
			}
			itemsRegion.Dispose();
			itemsRegion = null;
			graphics.Clip = clip;
			clip.Dispose();
		}
	}
}
