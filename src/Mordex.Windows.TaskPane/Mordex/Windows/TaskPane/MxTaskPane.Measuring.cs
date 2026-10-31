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
		private const int ScrollLineHeight = 8;

		internal void InvalidateLayout()
		{
			_minTextWidth = 30;
			_isLayoutValid = false;
			UpdateHoverItem(null);
			Invalidate(invalidateChildren: true);
		}

		private int MeasureGroupHeaderHeight(Graphics graphics, TaskPaneGroup taskPaneGroup, TaskPaneFormatStyle baseFormatStyle)
		{
			TaskPaneFormatStyle taskPaneFormatStyle = baseFormatStyle.MergeWith(taskPaneGroup.StateStyles.FormatStyle);
			if ((taskPaneGroup.GetItemState() & ItemState.Focus) != ItemState.Normal)
			{
				taskPaneFormatStyle = taskPaneFormatStyle.MergeWith(_hoverGroupFormatStyle);
			}
			Font headerFont = taskPaneFormatStyle.ResolveFont(Font);
			int fontHeight = (int)headerFont.GetHeight(graphics);
			return fontHeight + 12;
		}

		private Size GetItemImageSize(TaskPaneItem taskPaneItem)
		{
			if (_imageSize != Size.Empty)
			{
				return _imageSize;
			}
			if (taskPaneItem.Icon != null)
			{
				return taskPaneItem.Icon.Size;
			}
			if (taskPaneItem.Image != null)
			{
				return taskPaneItem.Image.Size;
			}
			if (_imageList != null)
			{
				return _imageList.ImageSize;
			}
			return new Size(16, 16);
		}

		private Size GetGroupImageSize(TaskPaneGroup taskPaneGroup)
		{
			if (taskPaneGroup.Icon != null)
			{
				return taskPaneGroup.Icon.Size;
			}
			if (taskPaneGroup.Image != null)
			{
				return taskPaneGroup.Image.Size;
			}
			if (_imageList != null)
			{
				return _imageList.ImageSize;
			}
			return new Size(16, 16);
		}

		private int MeasureItemHeight(Graphics graphics, TaskPaneItem taskPaneItem, TaskPaneFormatStyle taskPaneFormatStyle, int availableWidth, bool hasGroupBorder)
		{
			int textHeight = 0;
			if (hasGroupBorder)
			{
				availableWidth -= 2;
			}
			if (taskPaneItem.Indent != 0)
			{
				availableWidth -= taskPaneItem.Indent;
			}
			Size imageSize = GetItemImageSize(taskPaneItem);
			int height = imageSize.Height;
			availableWidth -= 4;
			if (taskPaneItem.ImageAlignment != Alignment.Center && (GetItemImage(taskPaneItem) != null || taskPaneItem.Icon != null))
			{
				availableWidth -= imageSize.Width + 9;
			}
			if (availableWidth > 0)
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				if (_useMnemonic)
				{
					textLayoutRenderer.SetShowHotkeyPrefix(true);
				}
				textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
				Font itemFont = taskPaneFormatStyle.ResolveFont(Font);
				if (availableWidth < _minTextWidth || !taskPaneItem.WordWrap)
				{
					if (availableWidth < _minTextWidth)
					{
						_isTextTooNarrow = true;
					}
					taskPaneItem.SetWordWrapped(false);
					textLayoutRenderer.SetWordWrapped(false);
					textHeight = (int)textLayoutRenderer.MeasureText(graphics, taskPaneItem.Text, itemFont, 0).Height;
				}
				else
				{
					taskPaneItem.SetWordWrapped(true);
					textLayoutRenderer.SetWordWrapped(true);
					textHeight = (int)textLayoutRenderer.MeasureText(graphics, taskPaneItem.Text, itemFont, availableWidth).Height;
				}
			}
			if (height > textHeight)
			{
				textHeight = height;
			}
			return textHeight + 4;
		}

		private void UpdateContentBounds()
		{
			_contentBounds = base.ClientRectangle;
			if (ComputeBorderStyle() == BorderStyle.None)
			{
				_borderInset = 0;
				return;
			}
			if (ComputeBorderStyle() == BorderStyle.Sunken || ComputeBorderStyle() == BorderStyle.Raised)
			{
				_borderInset = -2;
			}
			else
			{
				_borderInset = -1;
			}
			_contentBounds.Inflate(_borderInset, _borderInset);
		}

		private int GetEffectiveGroupSeparation()
		{
			if (_groupSeparation != -1)
			{
				return _groupSeparation;
			}
			return 10;
		}

		private int GetEffectiveLeftMargin()
		{
			if (_leftMargin >= 0)
			{
				return _leftMargin;
			}
			return 6;
		}

		private int GetEffectiveRightMargin()
		{
			if (_rightMargin >= 0)
			{
				return _rightMargin;
			}
			return 6;
		}

		private int GetEffectiveTopMargin()
		{
			if (_topMargin >= 0)
			{
				return _topMargin;
			}
			return 6;
		}

		private void OnVerticalScrollbarMouseDown(object sender, MouseEventArgs eventArgs)
		{
			Point position = PointerPosition.Current;
			position = PointToClient(position);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, position.X, position.Y, eventArgs.Delta);
			OnMouseDown(e);
		}

		private void OnVerticalScrollbarMouseUp(object sender, MouseEventArgs eventArgs)
		{
			// A scroll bar release at design time must not click the group under the pointer.
			if (base.DesignMode)
			{
				return;
			}
			Point position = PointerPosition.Current;
			position = PointToClient(position);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, position.X, position.Y, eventArgs.Delta);
			OnMouseUp(e);
		}

		private void OnVerticalScrollbarMouseMove(object sender, MouseEventArgs eventArgs)
		{
			Point position = PointerPosition.Current;
			position = PointToClient(position);
			MouseEventArgs e = new MouseEventArgs(eventArgs.Button, eventArgs.Clicks, position.X, position.Y, eventArgs.Delta);
			OnMouseMove(e);
		}

		private void OnVerticalScrollbarVisibleChanged(object sender, EventArgs eventArgs)
		{
			int widthDelta = 0;
			int xDelta = 0;
			widthDelta = ((!_verticalScrollbar.Visible) ? (widthDelta + _verticalScrollbar.Width) : (widthDelta - _verticalScrollbar.Width));
			if (RightToLeft == RightToLeft.Yes)
			{
				xDelta = ((!_verticalScrollbar.Visible) ? (xDelta - _verticalScrollbar.Width) : (xDelta + _verticalScrollbar.Width));
			}
			foreach (TaskPaneGroup item in _groups)
			{
				if (!item.Visible)
				{
					continue;
				}
				Rectangle bounds = item.GetHeaderBounds();
				bounds.Width += widthDelta;
				bounds.X += xDelta;
				item.SetHeaderBounds(bounds);
				foreach (TaskPaneItem childItem in item.Items)
				{
					if (item.Expanded && childItem.Visible)
					{
						bounds = childItem.Bounds;
						bounds.Width += widthDelta;
						bounds.X += xDelta;
						childItem.SetBounds(bounds);
					}
				}
				bounds = item.GetItemsBounds();
				bounds.Width += widthDelta;
				bounds.X += xDelta;
				item.SetItemsBounds(bounds);
			}
			Invalidate(invalidateChildren: true);
		}

		private void OnVerticalScroll(object sender, ScrollEventArgs eventArgs)
		{
			UpdateVerticalScrollPosition(sender, EventArgs.Empty);
		}

		private void UpdateVerticalScrollPosition(object sender, EventArgs eventArgs)
		{
			int scrollDelta = (_verticalScrollPosition - _verticalScrollbar.GetValue()) * ScrollLineHeight;
			_verticalScrollPosition = _verticalScrollbar.GetValue();
			if (_isInitializing)
			{
				return;
			}
			foreach (TaskPaneGroup item in _groups)
			{
				if (!item.Visible)
				{
					continue;
				}
				Rectangle bounds = item.GetHeaderBounds();
				bounds.Y += scrollDelta;
				item.SetHeaderBounds(bounds);
				foreach (TaskPaneItem childItem in item.Items)
				{
					if (item.Expanded && childItem.Visible)
					{
						bounds = childItem.Bounds;
						bounds.Y += scrollDelta;
						childItem.SetBounds(bounds);
					}
				}
				bounds = item.GetItemsBounds();
				bounds.Y += scrollDelta;
				item.SetItemsBounds(bounds);
			}
			Invalidate(invalidateChildren: true);
		}

		internal void RemoveItemContainerControl(TaskPaneItem taskPaneItem)
		{
			if (taskPaneItem.ContainerControl != null && base.Controls.Contains(taskPaneItem.ContainerControl))
			{
				base.Controls.Remove(taskPaneItem.ContainerControl);
			}
		}

		internal void SyncItemContainer(TaskPaneItem item)
		{
			if (!item.Container)
			{
				if (item.ContainerControl != null)
				{
					item.ContainerControl.Dispose();
					item.ContainerControl = null;
				}
			}
			else if (item.ContainerControl == null)
			{
				TaskPaneContainerControl taskPaneContainerControl = (item.ContainerControl = new TaskPaneContainerControl(this, item));
				taskPaneContainerControl._updatesLayoutOnResize = false;
				taskPaneContainerControl.Bounds = Rectangle.Empty;
				taskPaneContainerControl._updatesLayoutOnResize = true;
				if (base.Container != null)
				{
					base.Container.Add(taskPaneContainerControl);
				}
				base.Controls.Add(taskPaneContainerControl);
			}
			else if (item.ContainerControl._item == null)
			{
				item.ContainerControl._item = item;
				item.ContainerControl._owner = this;
			}
		}

		internal void SyncGroupContainer(TaskPaneGroup group)
		{
			if (!group.Container)
			{
				if (group.ContainerControl != null)
				{
					group.ContainerControl.Dispose();
					group.ContainerControl = null;
				}
			}
			else if (group.ContainerControl == null)
			{
				TaskPaneContainerControl taskPaneContainerControl = (group.ContainerControl = new TaskPaneContainerControl(this, group));
				taskPaneContainerControl._updatesLayoutOnResize = false;
				taskPaneContainerControl.Bounds = Rectangle.Empty;
				taskPaneContainerControl._updatesLayoutOnResize = true;
				if (base.Container != null)
				{
					base.Container.Add(taskPaneContainerControl);
				}
				base.Controls.Add(taskPaneContainerControl);
			}
			else if (group.ContainerControl._group == null)
			{
				group.ContainerControl._group = group;
				group.ContainerControl._owner = this;
			}
			InvalidateLayout();
		}

		private void LayoutColumns(Graphics graphics, int availableWidth)
		{
			int columnCount = _columns;
			int visibleSpanTotal = 0;
			foreach (TaskPaneGroup item in _groups)
			{
				if (item.Visible)
				{
					visibleSpanTotal += item.ColumnSpan;
				}
			}
			if (visibleSpanTotal < columnCount)
			{
				columnCount = visibleSpanTotal;
			}
			if (columnCount == 0)
			{
				columnCount = 1;
			}
			int columnWidth = (availableWidth - (columnCount - 1) * _columnSeparation) / columnCount;
			if (columnWidth < 0)
			{
				columnWidth = 0;
			}
			TaskPaneFormatStyle taskPaneFormatStyle = new TaskPaneFormatStyle(_groupsFormatStyle);
			TaskPaneFormatStyle itemsFormatStyle = new TaskPaneFormatStyle(_itemsFormatStyle);
			_hasContainerGroups = false;
			int[] columnBottoms = new int[columnCount];
			int currentY = _contentBounds.Top + GetEffectiveTopMargin() - _verticalScrollPosition * ScrollLineHeight;
			for (int i = 0; i < columnCount; i++)
			{
				columnBottoms.SetValue(currentY, i);
			}
			int maxY = currentY;
			int startY = currentY;
			int columnIndex = 1;
			int currentX = _contentBounds.Left + GetEffectiveLeftMargin();
			int columnRemainder = availableWidth - (columnCount - 1) * _columnSeparation - columnWidth * columnCount;
			columnRemainder = 0;
			int groupColumnSpan = 1;
			int baseColumnWidth = columnWidth;
			bool hasContent = false;
			foreach (TaskPaneGroup group in _groups)
			{
				columnWidth = baseColumnWidth;
				if (group.Visible)
				{
					hasContent = false;
					int itemTopOffset = 0;
					int itemLeftOffset = 0;
					int itemWidthReduction = 0;
					int groupBottomPadding = 0;
					if (columnIndex > columnCount)
					{
						columnIndex = 1;
						columnWidth -= columnRemainder;
					}
					else if (columnIndex == columnCount)
					{
						columnWidth += columnRemainder;
					}
					if (columnIndex == 1)
					{
						currentX = _contentBounds.Left + GetEffectiveLeftMargin();
					}
					else
					{
						int lastColumnAdjust = 0;
						if (columnIndex == columnCount)
						{
							lastColumnAdjust += columnRemainder;
						}
						currentX += (columnWidth + _columnSeparation - lastColumnAdjust) * groupColumnSpan - groupColumnSpan;
					}
					TaskPaneFormatStyle groupFormatStyle = taskPaneFormatStyle.MergeWith(group.StateStyles.FormatStyle);
					groupColumnSpan = ((group.ColumnSpan == 0) ? (columnCount - (columnIndex - 1)) : ((group.ColumnSpan <= columnCount - (columnIndex - 1)) ? group.ColumnSpan : (columnCount - (columnIndex - 1))));
					if (groupColumnSpan > 1)
					{
						_hasColumnSpan = true;
					}
					maxY = startY;
					for (int j = 0; j < groupColumnSpan; j++)
					{
						currentY = columnBottoms[columnIndex + j - 1];
						if (currentY > maxY)
						{
							maxY = currentY;
						}
					}
					group._columnIndex = columnIndex;
					currentY = maxY;
					columnWidth = baseColumnWidth * groupColumnSpan;
					if (groupColumnSpan > 1)
					{
						columnWidth += (groupColumnSpan - 1) * _columnSeparation;
					}
					int headerHeight = group.HeaderHeight;
					if (!group.ShowGroupCaption)
					{
						headerHeight = 0;
					}
					if (headerHeight < 0)
					{
						headerHeight = MeasureGroupHeaderHeight(graphics, group, groupFormatStyle);
					}
					if (groupFormatStyle != taskPaneFormatStyle)
					{
						groupFormatStyle.DisposeDrawingObjects();
					}
					Rectangle headerBounds = new Rectangle(currentX, currentY, columnWidth, headerHeight);
					group.SetHeaderBounds(headerBounds);
					currentY += headerHeight;
					if (group.Container)
					{
						_hasContainerGroups = true;
						hasContent = true;
						if (group.ContainerControl == null)
						{
							SyncGroupContainer(group);
						}
						TaskPaneContainerControl containerControl = group.ContainerControl;
						if (containerControl._owner == null)
						{
							containerControl._owner = this;
							containerControl._group = group;
						}
						if (group.Expanded)
						{
							int containerHeight = group.ContainerHeight;
							containerControl.Visible = true;
							if (containerHeight < 0)
							{
								containerHeight = 100;
								group.ContainerHeight = 100;
							}
							Rectangle containerBounds = new Rectangle(currentX, currentY, columnWidth, containerHeight);
							group.SetItemsBounds(containerBounds);
							containerControl.Visible = true;
							containerControl.Enabled = group.Enabled;
							currentY += containerHeight;
						}
						else
						{
							containerControl.Visible = false;
							if (!base.DesignMode)
							{
								group.SetItemsBounds(Rectangle.Empty);
							}
						}
						foreach (TaskPaneItem clearedItem in group.Items)
						{
							if (clearedItem.Container)
							{
								clearedItem.SetBounds(Rectangle.Empty);
								if (clearedItem.ContainerControl != null)
								{
									clearedItem.ContainerControl.Visible = false;
								}
							}
						}
						currentY += GetEffectiveGroupSeparation();
						for (int k = 0; k < groupColumnSpan; k++)
						{
							columnBottoms.SetValue(currentY, columnIndex + k - 1);
						}
					}
					else
					{
						if (ShouldShowGroupBorder(group))
						{
							itemTopOffset = 0;
							itemLeftOffset = 1;
							itemWidthReduction = 2;
							groupBottomPadding = 1;
						}
						foreach (TaskPaneItem groupItem in group.Items)
						{
							if (group.Expanded && groupItem.Visible)
							{
								hasContent = true;
								if (groupItem.Container)
								{
									_hasContainerGroups = true;
									if (groupItem.ContainerControl == null)
									{
										SyncItemContainer(groupItem);
									}
									TaskPaneContainerControl itemContainerControl = groupItem.ContainerControl;
									if (itemContainerControl._owner == null)
									{
										itemContainerControl._owner = this;
										itemContainerControl._item = groupItem;
									}
									int itemContainerHeight = groupItem.ContainerHeight;
									itemContainerControl.Visible = true;
									if (itemContainerHeight < 0)
									{
										itemContainerHeight = 24;
										groupItem.ContainerHeight = 24;
									}
									Rectangle itemContainerBounds = new Rectangle(currentX + itemLeftOffset, currentY + itemTopOffset, columnWidth - itemWidthReduction, itemContainerHeight);
									groupItem.SetBounds(itemContainerBounds);
									if (!itemContainerControl.Visible)
									{
										itemContainerControl.Visible = true;
									}
									itemContainerControl.Enabled = groupItem.Enabled;
									currentY += itemContainerHeight;
								}
								else
								{
									TaskPaneFormatStyle groupItemsFormatStyle = itemsFormatStyle.MergeWith(group.StateStyles.FormatStyle);
									TaskPaneFormatStyle itemFormatStyle = groupItemsFormatStyle.MergeWith(groupItem.StateStyles.FormatStyle);
									int itemHeight = groupItem.Height;
									if (itemHeight < 0)
									{
										itemHeight = MeasureItemHeight(graphics, groupItem, itemFormatStyle, columnWidth, ShouldShowGroupBorder(group));
									}
									Rectangle itemBounds = new Rectangle(currentX + itemLeftOffset, currentY + itemTopOffset, columnWidth - itemWidthReduction, itemHeight);
									groupItem.SetBounds(itemBounds);
									currentY += itemHeight;
									if (itemFormatStyle != groupItemsFormatStyle)
									{
										itemFormatStyle.DisposeDrawingObjects();
									}
									groupItemsFormatStyle.DisposeDrawingObjects();
								}
							}
							else
							{
								groupItem.SetBounds(Rectangle.Empty);
								if (groupItem.ContainerControl != null)
								{
									groupItem.ContainerControl.Visible = false;
									_hasContainerGroups = true;
								}
							}
						}
						currentY += groupBottomPadding;
						group.SetItemsBounds(new Rectangle(currentX, group.GetHeaderBounds().Bottom, columnWidth, currentY - group.GetHeaderBounds().Bottom));
						if (group.GetItemsBounds().Height > 1 || group.GetHeaderBounds().Height != 0)
						{
							currentY += GetEffectiveGroupSeparation();
						}
						if (!hasContent)
						{
							group.SetItemsBounds(Rectangle.Empty);
						}
						for (int spanIndex = 0; spanIndex < groupColumnSpan; spanIndex++)
						{
							columnBottoms.SetValue(currentY, columnIndex + spanIndex - 1);
						}
					}
					columnIndex += groupColumnSpan;
					continue;
				}
				group.SetHeaderBounds(Rectangle.Empty);
				group.SetItemsBounds(Rectangle.Empty);
				if (group.ContainerControl != null)
				{
					group.ContainerControl.Visible = false;
				}
				foreach (TaskPaneItem hiddenItem in group.Items)
				{
					hiddenItem.SetBounds(Rectangle.Empty);
					if (hiddenItem.ContainerControl != null)
					{
						hiddenItem.ContainerControl.Visible = false;
					}
				}
			}
			maxY = 0;
			for (int scanColumn = 0; scanColumn < columnCount; scanColumn++)
			{
				currentY = columnBottoms[scanColumn];
				if (currentY > maxY)
				{
					maxY = currentY;
				}
			}
			_minTextWidth = 30;
			currentY = maxY;
			taskPaneFormatStyle.DisposeDrawingObjects();
			itemsFormatStyle.DisposeDrawingObjects();
			_isLayoutValid = true;
			currentY -= GetEffectiveGroupSeparation();
			currentY = (_innerHeight = currentY + _verticalScrollPosition * ScrollLineHeight);
			if (currentY < _contentBounds.Bottom)
			{
				currentY = _contentBounds.Bottom;
			}
			int visibleLines = _contentBounds.Height / ScrollLineHeight;
			int totalLines = currentY / ScrollLineHeight - 1;
			if (totalLines == visibleLines && currentY > _contentBounds.Bottom)
			{
				totalLines++;
			}
			ApplyScrollBarRange(0, totalLines, visibleLines, _verticalScrollPosition);
			if (visibleLines >= totalLines)
			{
				_verticalScrollbar.Enabled = false;
			}
			else
			{
				_verticalScrollbar.Enabled = true;
			}
			if (_verticalScrollBar == VerticalScrollBar.Never)
			{
				if (_verticalScrollbar.Visible)
				{
					_verticalScrollbar.Visible = false;
					InvalidateLayout();
				}
				else
				{
					_verticalScrollbar.Visible = false;
				}
			}
			else if (!_verticalScrollbar.Enabled && _verticalScrollBar == VerticalScrollBar.Automatic)
			{
				if (_verticalScrollbar.Visible)
				{
					_verticalScrollbar.Visible = false;
					if (!_isAnimatingGroup && !_verticalScrollbar.Visible)
					{
						if (_isTextTooNarrow)
						{
							_minTextWidth = 30 + _verticalScrollbar.Width + 1;
						}
						LayoutContent(graphics);
					}
				}
				else
				{
					_verticalScrollbar.Visible = false;
				}
			}
			else if (!_verticalScrollbar.Visible)
			{
				_verticalScrollbar.Visible = true;
				if (!_isAnimatingGroup && _verticalScrollbar.Visible)
				{
					if (_isTextTooNarrow)
					{
						_minTextWidth = 30;
					}
					LayoutContent(graphics);
				}
			}
			else
			{
				_verticalScrollbar.Visible = true;
			}
		}

		internal void LayoutContent(Graphics graphics)
		{
			if (_isLayoutRunning)
			{
				return;
			}
			_isLayoutRunning = true;
			try
			{
				LayoutContentCore(graphics);
			}
			finally
			{
				_isLayoutRunning = false;
			}
		}

		private void LayoutContentCore(Graphics graphics)
		{
			_isTextTooNarrow = false;
			_hasColumnSpan = false;
			_hasContainerGroups = false;
			UpdateContentBounds();
			if (_borderPanel == null)
			{
				_borderPanel = new TaskPaneControl(this);
			}
			if (!base.Controls.Contains(_borderPanel))
			{
				base.Controls.Add(_borderPanel);
			}
			_borderPanel._borderInset = _borderInset;
			_borderPanel.Bounds = base.ClientRectangle;
			_borderPanel.Visible = _borderInset != 0;
			int verticalScrollBarArrowHeight = SystemInformation.VerticalScrollBarArrowHeight;
			if (RightToLeft == RightToLeft.No)
			{
				_verticalScrollbar.SetBounds(_contentBounds.Right - verticalScrollBarArrowHeight, _contentBounds.Top, verticalScrollBarArrowHeight, _contentBounds.Height);
			}
			else
			{
				_verticalScrollbar.SetBounds(_contentBounds.Left, _contentBounds.Top, verticalScrollBarArrowHeight, _contentBounds.Height);
			}
			int currentY = _contentBounds.Top + GetEffectiveTopMargin() - _verticalScrollPosition * ScrollLineHeight;
			int currentX = _contentBounds.Left + GetEffectiveLeftMargin();
			int contentWidth = 0;
			if (_verticalScrollbar.Visible)
			{
				contentWidth = _contentBounds.Width - GetEffectiveLeftMargin() - GetEffectiveRightMargin() - verticalScrollBarArrowHeight;
				if (RightToLeft == RightToLeft.Yes)
				{
					currentX += verticalScrollBarArrowHeight;
				}
			}
			else
			{
				contentWidth = _contentBounds.Width - GetEffectiveLeftMargin() - GetEffectiveRightMargin();
			}
			if (_columns != 1 && _groups.Count > 0)
			{
				LayoutColumns(graphics, contentWidth);
				_isLayoutRunning = false;
				return;
			}
			TaskPaneFormatStyle taskPaneFormatStyle = new TaskPaneFormatStyle(_groupsStateStyles.FormatStyle);
			TaskPaneFormatStyle itemsFormatStyle = new TaskPaneFormatStyle(_itemsStateStyles.FormatStyle);
			bool hasContent = false;
			foreach (TaskPaneGroup item in _groups)
			{
				if (item.Visible)
				{
					hasContent = false;
					TaskPaneFormatStyle groupFormatStyle = taskPaneFormatStyle.MergeWith(item.StateStyles.FormatStyle);
					int headerHeight = item.HeaderHeight;
					if (!item.ShowGroupCaption)
					{
						headerHeight = 0;
					}
					if (headerHeight < 0)
					{
						headerHeight = MeasureGroupHeaderHeight(graphics, item, groupFormatStyle);
					}
					if (groupFormatStyle != taskPaneFormatStyle)
					{
						groupFormatStyle.DisposeDrawingObjects();
					}
					Rectangle headerBounds = new Rectangle(currentX, currentY, contentWidth, headerHeight);
					item.SetHeaderBounds(headerBounds);
					currentY += headerHeight;
					int itemTopOffset = 0;
					int itemLeftOffset = 0;
					int groupBottomPadding = 0;
					int itemWidthReduction = 0;
					if (item.Container)
					{
						_hasContainerGroups = true;
						hasContent = true;
						if (item.ContainerControl == null)
						{
							SyncGroupContainer(item);
						}
						TaskPaneContainerControl containerControl = item.ContainerControl;
						if (containerControl._owner == null)
						{
							containerControl._owner = this;
							containerControl._group = item;
						}
						if (item.Expanded)
						{
							int containerHeight = item.ContainerHeight;
							containerControl.Visible = true;
							if (containerHeight < 0)
							{
								containerHeight = 100;
								item.ContainerHeight = 100;
							}
							Rectangle containerBounds = new Rectangle(currentX, currentY, contentWidth, containerHeight);
							item.SetItemsBounds(containerBounds);
							if (!containerControl.Visible)
							{
								containerControl.Visible = true;
							}
							containerControl.Enabled = item.Enabled;
							currentY += containerHeight;
						}
						else
						{
							containerControl.Visible = false;
							if (!base.DesignMode)
							{
								item.SetItemsBounds(Rectangle.Empty);
							}
						}
						foreach (TaskPaneItem clearedItem in item.Items)
						{
							if (clearedItem.Container)
							{
								clearedItem.SetBounds(Rectangle.Empty);
								if (clearedItem.ContainerControl != null)
								{
									clearedItem.ContainerControl.Visible = false;
								}
							}
						}
						currentY += GetEffectiveGroupSeparation();
						continue;
					}
					foreach (TaskPaneItem groupItem in item.Items)
					{
						if (item.Expanded && groupItem.Visible)
						{
							hasContent = true;
							if (groupItem.Container)
							{
								_hasContainerGroups = true;
								if (groupItem.ContainerControl == null)
								{
									SyncItemContainer(groupItem);
								}
								TaskPaneContainerControl itemContainerControl = groupItem.ContainerControl;
								if (itemContainerControl._owner == null)
								{
									itemContainerControl._owner = this;
									itemContainerControl._item = groupItem;
								}
								int itemContainerHeight = groupItem.ContainerHeight;
								itemContainerControl.Visible = true;
								if (itemContainerHeight < 0)
								{
									itemContainerHeight = 24;
									groupItem.ContainerHeight = 24;
								}
								Rectangle itemContainerBounds = new Rectangle(currentX + itemLeftOffset, currentY + itemTopOffset, contentWidth - itemWidthReduction, itemContainerHeight);
								groupItem.SetBounds(itemContainerBounds);
								if (!itemContainerControl.Visible)
								{
									itemContainerControl.Visible = true;
								}
								itemContainerControl.Enabled = groupItem.Enabled;
								currentY += itemContainerHeight;
							}
							else
							{
								TaskPaneFormatStyle groupItemsFormatStyle = itemsFormatStyle.MergeWith(item.StateStyles.FormatStyle);
								TaskPaneFormatStyle itemFormatStyle = groupItemsFormatStyle.MergeWith(groupItem.StateStyles.FormatStyle);
								int itemHeight = groupItem.Height;
								if (itemHeight < 0)
								{
									itemHeight = MeasureItemHeight(graphics, groupItem, itemFormatStyle, contentWidth, ShouldShowGroupBorder(item));
								}
								Rectangle itemBounds = new Rectangle(currentX + itemLeftOffset, currentY + itemTopOffset, contentWidth - itemWidthReduction, itemHeight);
								groupItem.SetBounds(itemBounds);
								currentY += itemHeight;
								if (itemFormatStyle != groupItemsFormatStyle)
								{
									itemFormatStyle.DisposeDrawingObjects();
								}
								groupItemsFormatStyle.DisposeDrawingObjects();
							}
						}
						else
						{
							groupItem.SetBounds(Rectangle.Empty);
							if (groupItem.ContainerControl != null)
							{
								groupItem.ContainerControl.Visible = false;
								_hasContainerGroups = true;
							}
						}
					}
					currentY += groupBottomPadding;
					item.SetItemsBounds(new Rectangle(currentX, item.GetHeaderBounds().Bottom, contentWidth, currentY - item.GetHeaderBounds().Bottom));
					if (item.GetItemsBounds().Height > 1 || item.GetHeaderBounds().Height != 0)
					{
						currentY += GetEffectiveGroupSeparation();
					}
					if (!hasContent)
					{
						item.SetItemsBounds(Rectangle.Empty);
					}
					continue;
				}
				item.SetHeaderBounds(Rectangle.Empty);
				item.SetItemsBounds(Rectangle.Empty);
				foreach (TaskPaneItem hiddenItem in item.Items)
				{
					hiddenItem.SetBounds(Rectangle.Empty);
					if (hiddenItem.ContainerControl != null)
					{
						hiddenItem.ContainerControl.Visible = false;
					}
				}
				if (item.ContainerControl != null)
				{
					item.ContainerControl.Visible = false;
				}
			}
			_minTextWidth = 30;
			taskPaneFormatStyle.DisposeDrawingObjects();
			itemsFormatStyle.DisposeDrawingObjects();
			_isLayoutValid = true;
			currentY -= GetEffectiveGroupSeparation();
			currentY = (_innerHeight = currentY + _verticalScrollPosition * ScrollLineHeight);
			if (currentY < _contentBounds.Bottom)
			{
				currentY = _contentBounds.Bottom;
			}
			int visibleLines = _contentBounds.Height / ScrollLineHeight;
			int totalLines = currentY / ScrollLineHeight - 1;
			_ = currentY % ScrollLineHeight;
			_isLayoutRunning = false;
			if (totalLines <= visibleLines + _verticalScrollPosition && currentY > _contentBounds.Bottom)
			{
				totalLines++;
			}
			ApplyScrollBarRange(0, totalLines, visibleLines, _verticalScrollPosition);
			if (visibleLines >= totalLines)
			{
				_verticalScrollbar.Enabled = false;
			}
			else
			{
				_verticalScrollbar.Enabled = true;
			}
			if (_verticalScrollBar == VerticalScrollBar.Never)
			{
				if (_verticalScrollbar.Visible)
				{
					_verticalScrollbar.Visible = false;
					InvalidateLayout();
				}
				else
				{
					_verticalScrollbar.Visible = false;
				}
			}
			else if (!_verticalScrollbar.Enabled && _verticalScrollBar == VerticalScrollBar.Automatic)
			{
				_verticalScrollPosition = 0;
				if (_verticalScrollbar.Visible)
				{
					_verticalScrollbar.Visible = false;
					if (!_verticalScrollbar.Visible)
					{
						if (_isTextTooNarrow)
						{
							_minTextWidth = 30 + _verticalScrollbar.Width;
						}
						LayoutContent(graphics);
					}
				}
				else
				{
					_verticalScrollbar.Visible = false;
				}
			}
			else if (!_verticalScrollbar.Visible)
			{
				_verticalScrollbar.Visible = true;
				if (_verticalScrollbar.Visible)
				{
					if (_isTextTooNarrow)
					{
						_minTextWidth = 30;
					}
					LayoutContent(graphics);
				}
			}
			else
			{
				_verticalScrollbar.Visible = true;
			}
			_borderPanel.BringToFront();
		}

		internal void ApplyScrollBarRange(int minimum, int maximum, int pageSize, int scrollPosition)
		{
			try
			{
				_verticalScrollbar.UpdateMinimum(minimum);
				_verticalScrollbar.UpdateMaximum(maximum);
				if (pageSize > 0)
				{
					_verticalScrollbar.UpdateLargeChange(pageSize);
				}
				else
				{
					_verticalScrollbar.UpdateLargeChange(1);
				}
				_verticalScrollbar.UpdateValue(scrollPosition);
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			CompleteGroupAnimation();
			base.OnResize(e);
			InvalidateLayout();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			InvalidateLayout();
		}
	}
}
