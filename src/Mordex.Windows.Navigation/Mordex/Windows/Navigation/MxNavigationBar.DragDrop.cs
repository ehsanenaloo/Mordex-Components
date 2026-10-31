using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Navigation.Internal;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public partial class MxNavigationBar
	{
		private const int DropMarkerSize = 8;

		protected override void OnDragDrop(DragEventArgs e)
		{
			if (_isDragLocked || (_selectedGroup.Container && _dragSourceItem != null))
			{
				base.OnDragDrop(e);
				return;
			}
			if (_dragMode == DragMode.AutomaticNoMove)
			{
				base.OnDragDrop(e);
				return;
			}
			Point dropPoint = new Point(e.X, e.Y);
			dropPoint = PointToClient(dropPoint);
			bool isDropLineStyle = true;
			if (DragStyle == DragStyle.InvertedRect)
			{
				isDropLineStyle = false;
			}
			if (_orientation == NavigationBarOrientation.Horizontal && _selectedGroup.View == NavigationBarView.SmallIcons && _firstVisibleItem != null)
			{
				isDropLineStyle = false;
				if (_dragSourceItem._index < _firstVisibleItem._index)
				{
					_firstVisibleItem = GetNextVisibleItem(_firstVisibleItem._index);
				}
			}
			if (_dragSourceItem != null)
			{
				UpdateItemDragHighlight(null, null);
				NavigationItem navigationBarItem = GetLastVisibleItem(_selectedGroup);
				if (_itemsAreaBounds.Contains(dropPoint))
				{
					NavigationItem targetItem = FindItemAt(dropPoint.X, dropPoint.Y);
					if (targetItem == _dragSourceItem)
					{
						base.OnDragDrop(e);
						return;
					}
					if (targetItem != null)
					{
						int index = _dragSourceItem._index;
						int firstVisibleIndex = _firstVisibleItem._index;
						int targetIndex = targetItem._index;
						NavigationGroup navigationBarGroup = _dragSourceItem._owner.GetGroup();
						if (targetIndex > index && isDropLineStyle && targetItem._owner.GetGroup() == navigationBarGroup)
						{
							targetIndex--;
						}
						navigationBarGroup.Items.Remove(_dragSourceItem);
						if (navigationBarGroup == _selectedGroup)
						{
							if (targetItem == navigationBarItem && isDropLineStyle)
							{
								if (!_isDropAfterItem)
								{
									_selectedGroup.Items.Insert(targetIndex, _dragSourceItem);
								}
								else
								{
									_selectedGroup.Items.Insert(targetIndex + 1, _dragSourceItem);
								}
							}
							else
							{
								_selectedGroup.Items.Insert(targetIndex, _dragSourceItem);
							}
							if (targetItem == _firstVisibleItem)
							{
								_firstVisibleItem = _dragSourceItem;
								if (_orientation == NavigationBarOrientation.Vertical)
								{
									_firstVisibleItem.UpdateBoundsY(targetItem.GetBounds().Top);
								}
								else
								{
									_firstVisibleItem.UpdateBoundsX(targetItem.GetBounds().Left);
								}
							}
							else if (_dragSourceItem == _firstVisibleItem)
							{
								_firstVisibleItem = _selectedGroup.Items[firstVisibleIndex];
								if (_orientation == NavigationBarOrientation.Vertical)
								{
									_firstVisibleItem.UpdateBoundsY(_dragSourceItem.GetBounds().Top);
								}
								else
								{
									_firstVisibleItem.UpdateBoundsX(_dragSourceItem.GetBounds().Left);
								}
							}
						}
						else
						{
							if (targetItem._index == navigationBarItem._index && _isDropAfterItem)
							{
								_selectedGroup.Items.Insert(targetIndex + 1, _dragSourceItem);
								_isDropAfterItem = false;
							}
							else
							{
								_selectedGroup.Items.Insert(targetIndex, _dragSourceItem);
							}
							if (targetItem == _firstVisibleItem)
							{
								_firstVisibleItem = _selectedGroup.Items[firstVisibleIndex];
								if (_orientation == NavigationBarOrientation.Vertical)
								{
									_firstVisibleItem.UpdateBoundsY(targetItem.GetBounds().Top);
								}
								else
								{
									_firstVisibleItem.UpdateBoundsX(targetItem.GetBounds().Left);
								}
							}
						}
					}
					else
					{
						_firstVisibleItem = null;
						NavigationGroup sourceGroup = _dragSourceItem._owner.GetGroup();
						sourceGroup.Items.Remove(_dragSourceItem);
						_selectedGroup.Items.Add(_dragSourceItem);
					}
					if (_dragGroupIndex != -1)
					{
						UpdateGroupDragHighlight(null, null);
					}
				}
			}
			else if (_pressedGroup != null)
			{
				NavigationGroup targetGroup = FindGroupAt(dropPoint.X, dropPoint.Y);
				if (targetGroup != null)
				{
					bool wasSelected = false;
					int targetGroupIndex = targetGroup._index;
					if (_pressedGroup.Selected)
					{
						wasSelected = true;
					}
					_groups._resetGroupContainers = false;
					_groups.Remove(_pressedGroup);
					_groups.Insert(targetGroupIndex, _pressedGroup);
					_groups._resetGroupContainers = true;
					if (wasSelected)
					{
						_pressedGroup.Selected = true;
					}
					_firstVisibleItem = null;
				}
			}
			base.OnDragDrop(e);
			Invalidate();
		}

		private void UpdateItemDropLine(Graphics graphics, int dropItemIndex)
		{
			int linePosition = 0;
			int previousLineStart = _dropLineStartX;
			bool isHorizontalLarge = false;
			if (_orientation == NavigationBarOrientation.Horizontal && _selectedGroup.View == NavigationBarView.LargeIcons)
			{
				isHorizontalLarge = true;
			}
			if (dropItemIndex == _firstVisibleItem._index && !_isDropAfterItem)
			{
				linePosition = (isHorizontalLarge ? (_itemsAreaBounds.Left + DropMarkerSize) : (_itemsAreaBounds.Top + DropMarkerSize));
			}
			else if (dropItemIndex != -1)
			{
				if (isHorizontalLarge)
				{
					linePosition = ((!_isDropAfterItem) ? ((_selectedGroup.View != NavigationBarView.LargeIcons) ? (_selectedGroup.Items[dropItemIndex].GetBounds().Left - 3) : (_selectedGroup.Items[dropItemIndex].GetTextBounds().Left - 3)) : ((_selectedGroup.View != NavigationBarView.LargeIcons) ? _selectedGroup.Items[dropItemIndex].GetBounds().Right : _selectedGroup.Items[dropItemIndex].GetTextBounds().Right));
				}
				else if (!_isDropAfterItem)
				{
					Rectangle itemBounds = _selectedGroup.Items[dropItemIndex].GetBounds();
					linePosition = itemBounds.Top;
					if (_selectedGroup.View == NavigationBarView.SmallIcons)
					{
						_dropLineStartX = itemBounds.Left + 3;
						_dropLineEndX = itemBounds.Right - 3;
						if (_orientation == NavigationBarOrientation.Horizontal)
						{
							linePosition = itemBounds.Bottom - 3;
						}
					}
				}
				else if (_selectedGroup.View == NavigationBarView.LargeIcons)
				{
					linePosition = _selectedGroup.Items[dropItemIndex].GetTextBounds().Bottom;
				}
				else
				{
					Rectangle itemBoundsBelow = _selectedGroup.Items[dropItemIndex].GetBounds();
					linePosition = itemBoundsBelow.Bottom;
					_dropLineStartX = itemBoundsBelow.Left + 3;
					_dropLineEndX = itemBoundsBelow.Right - 3;
				}
			}
			else if (_dropLinePosition != -1)
			{
				if (!isHorizontalLarge)
				{
					if (_dropLinePosition - DropMarkerSize < _itemsAreaBounds.Top)
					{
						_dropLinePosition = _itemsAreaBounds.Top + DropMarkerSize;
					}
					Invalidate(new Rectangle(_itemsAreaBounds.Left, _dropLinePosition - DropMarkerSize, _itemsAreaBounds.Width, 20));
				}
				else
				{
					if (_dropLinePosition - DropMarkerSize < _itemsAreaBounds.Left)
					{
						_dropLinePosition = _itemsAreaBounds.Left + DropMarkerSize;
					}
					Invalidate(new Rectangle(_dropLinePosition - DropMarkerSize, _itemsAreaBounds.Top, 20, _itemsAreaBounds.Height));
				}
				_dropLinePosition = -1;
				return;
			}
			if (_dropLinePosition == linePosition && _dropLineStartX == previousLineStart)
			{
				return;
			}
			if (_dropLinePosition != -1)
			{
				if (!isHorizontalLarge)
				{
					if (_dropLinePosition - DropMarkerSize < _itemsAreaBounds.Top)
					{
						_dropLinePosition = _itemsAreaBounds.Top + DropMarkerSize;
					}
					Invalidate(new Rectangle(_itemsAreaBounds.Left, _dropLinePosition - DropMarkerSize, _itemsAreaBounds.Width, 20));
				}
				else
				{
					if (_dropLinePosition - DropMarkerSize < _itemsAreaBounds.Left)
					{
						_dropLinePosition = _itemsAreaBounds.Left + DropMarkerSize;
					}
					Invalidate(new Rectangle(_dropLinePosition - DropMarkerSize, _itemsAreaBounds.Top, 20, _itemsAreaBounds.Height));
				}
				_dropLinePosition = -1;
			}
			else if (linePosition != 0)
			{
				_dropLinePosition = linePosition;
				DrawHorizontalDropLine(graphics, _dropLinePosition, false);
			}
		}

		private void UpdateGroupDragHighlight(Graphics graphics, NavigationGroup navigationBarGroup)
		{
			bool ownsGraphics = false;
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			if (_dragGroupIndex != -1)
			{
				Rectangle bounds = _groups[_dragGroupIndex].Bounds;
				DrawDragHighlight(bounds);
			}
			if (navigationBarGroup == null)
			{
				_dragGroupIndex = -1;
			}
			else
			{
				_dragGroupIndex = navigationBarGroup._index;
				DrawDragHighlight(navigationBarGroup.Bounds);
			}
			if (ownsGraphics)
			{
				graphics.Dispose();
				graphics = null;
			}
		}

		private void UpdateItemDragHighlight(Graphics graphics, NavigationItem navigationBarItem)
		{
			Rectangle highlightBounds;
			if (_dragItemIndex != -1)
			{
				highlightBounds = _selectedGroup.Items[_dragItemIndex].GetBounds();
				if (highlightBounds.Right > _itemsAreaBounds.Right)
				{
					highlightBounds.Width = _itemsAreaBounds.Right - highlightBounds.Left;
				}
				DrawDragHighlight(highlightBounds);
			}
			if (navigationBarItem == null)
			{
				_dragItemIndex = -1;
				return;
			}
			_dragItemIndex = navigationBarItem._index;
			highlightBounds = navigationBarItem.GetBounds();
			if (highlightBounds.Right > _itemsAreaBounds.Right)
			{
				highlightBounds.Width = _itemsAreaBounds.Right - highlightBounds.Left;
			}
			DrawDragHighlight(highlightBounds);
		}

		private void DrawVerticalDropLine(Graphics graphics, int linePosition, bool isErasing)
		{
			bool ownsGraphics = false;
			bool ownsBrush = false;
			Point[] topMarker = new Point[3];
			Point[] bottomMarker = new Point[3];
			int markerY = 0;
			if (isErasing)
			{
				if (_dropLinePosition - DropMarkerSize < _itemsAreaBounds.Left)
				{
					_dropLinePosition = _itemsAreaBounds.Left + DropMarkerSize;
				}
				Invalidate(new Rectangle(_dropLinePosition - DropMarkerSize, _itemsAreaBounds.Top, 15, _itemsAreaBounds.Height));
				_dropLinePosition = -1;
				return;
			}
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			Region clip = graphics.Clip;
			Region clipRegion = (graphics.Clip = new Region(_itemsAreaBounds));
			Brush dropBrush = null;
			NavigationBarFormatStyle navigationBarFormatStyle = null;
			NavigationBarFormatStyle groupsStyle = null;
			NavigationBarFormatStyle backgroundStyle = null;
			dropBrush = new SolidBrush(_dropLineColor);
			_dropLinePosition = linePosition;
			ownsBrush = true;
			int lastItemRight = 0;
			NavigationItem navigationBarItem = GetLastVisibleItem(_selectedGroup);
			if (navigationBarItem != null)
			{
				lastItemRight = ((_selectedGroup.View != NavigationBarView.LargeIcons) ? navigationBarItem.GetBounds().Right : navigationBarItem.GetTextBounds().Right);
			}
			if (_firstVisibleItem == null || linePosition == _itemsAreaBounds.Left + DropMarkerSize)
			{
				linePosition = _itemsAreaBounds.Left + 5;
				if (isErasing)
				{
					_dropLinePosition = linePosition;
				}
				markerY = _innerBounds.Top + 5;
				ref Point reference = ref topMarker[0];
				reference = new Point(linePosition + DropMarkerSize, markerY);
				ref Point startTopCorner = ref topMarker[1];
				startTopCorner = new Point(linePosition, markerY);
				ref Point startTopTip = ref topMarker[2];
				startTopTip = new Point(linePosition, markerY + DropMarkerSize);
				markerY = _innerBounds.Bottom - 5;
				ref Point startBottomEdge = ref bottomMarker[0];
				startBottomEdge = new Point(linePosition + DropMarkerSize, markerY);
				ref Point startBottomCorner = ref bottomMarker[1];
				startBottomCorner = new Point(linePosition, markerY);
				ref Point startBottomTip = ref bottomMarker[2];
				startBottomTip = new Point(linePosition, markerY - DropMarkerSize);
			}
			else if (linePosition == lastItemRight && (_isDropAfterItem || isErasing))
			{
				markerY = _innerBounds.Top + 5;
				ref Point endTopEdge = ref topMarker[0];
				endTopEdge = new Point(linePosition - DropMarkerSize, markerY);
				ref Point endTopCorner = ref topMarker[1];
				endTopCorner = new Point(linePosition, markerY);
				ref Point endTopTip = ref topMarker[2];
				endTopTip = new Point(linePosition, markerY + DropMarkerSize);
				markerY = _innerBounds.Bottom - 5;
				ref Point endBottomEdge = ref bottomMarker[0];
				endBottomEdge = new Point(linePosition - DropMarkerSize, markerY);
				ref Point endBottomCorner = ref bottomMarker[1];
				endBottomCorner = new Point(linePosition, markerY);
				ref Point endBottomTip = ref bottomMarker[2];
				endBottomTip = new Point(linePosition, markerY - DropMarkerSize);
			}
			else
			{
				markerY = _innerBounds.Top + 5;
				ref Point midTopRight = ref topMarker[0];
				midTopRight = new Point(linePosition + 5, markerY);
				ref Point midTopLeft = ref topMarker[1];
				midTopLeft = new Point(linePosition - 5, markerY);
				ref Point midTopTip = ref topMarker[2];
				midTopTip = new Point(linePosition, markerY + DropMarkerSize);
				markerY = _innerBounds.Bottom - 5;
				ref Point midBottomRight = ref bottomMarker[0];
				midBottomRight = new Point(linePosition + 5, markerY);
				ref Point midBottomLeft = ref bottomMarker[1];
				midBottomLeft = new Point(linePosition - 5, markerY);
				ref Point midBottomTip = ref bottomMarker[2];
				midBottomTip = new Point(linePosition, markerY - DropMarkerSize);
			}
			if (dropBrush != null)
			{
				graphics.FillPolygon(dropBrush, topMarker);
				using (Pen dropPen = new Pen(dropBrush))
				{
					graphics.DrawLine(dropPen, topMarker[2], bottomMarker[2]);
				}
				graphics.FillPolygon(dropBrush, bottomMarker);
			}
			if (ownsBrush)
			{
				dropBrush.Dispose();
				dropBrush = null;
			}
			else
			{
				navigationBarFormatStyle.DisposeBrushesAndFont();
				groupsStyle.DisposeBrushesAndFont();
				backgroundStyle.DisposeBrushesAndFont();
			}
			graphics.Clip = clip;
			clip.Dispose();
			clipRegion.Dispose();
			clipRegion = null;
			if (ownsGraphics)
			{
				graphics.Dispose();
				graphics = null;
			}
		}

		private void DrawHorizontalDropLine(Graphics graphics, int linePosition, bool isErasing)
		{
			NavigationItem navigationBarItem = GetLastVisibleItem(_selectedGroup);
			if (_orientation == NavigationBarOrientation.Horizontal && (_selectedGroup.View == NavigationBarView.LargeIcons || navigationBarItem == null))
			{
				DrawVerticalDropLine(graphics, linePosition, isErasing);
				return;
			}
			bool ownsGraphics = false;
			bool ownsBrush = false;
			Point[] leftMarker = new Point[3];
			Point[] rightMarker = new Point[3];
			int leftX = 0;
			int rightX = 0;
			if (isErasing)
			{
				if (_dropLinePosition - DropMarkerSize < _itemsAreaBounds.Top)
				{
					_dropLinePosition = _itemsAreaBounds.Top + DropMarkerSize;
				}
				Invalidate(new Rectangle(_itemsAreaBounds.Left, _dropLinePosition - DropMarkerSize, _itemsAreaBounds.Width, 15));
				_dropLinePosition = -1;
				return;
			}
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			Region clip = graphics.Clip;
			Region clipRegion = (graphics.Clip = new Region(_itemsAreaBounds));
			Brush dropBrush = null;
			NavigationBarFormatStyle navigationBarFormatStyle = null;
			NavigationBarFormatStyle groupsStyle = null;
			NavigationBarFormatStyle backgroundStyle = null;
			dropBrush = new SolidBrush(_dropLineColor);
			_dropLinePosition = linePosition;
			ownsBrush = true;
			int lastItemBottom = 0;
			if (navigationBarItem != null)
			{
				lastItemBottom = ((_selectedGroup.View != NavigationBarView.LargeIcons) ? navigationBarItem.GetBounds().Bottom : navigationBarItem.GetTextBounds().Bottom);
				if (linePosition == 0)
				{
					linePosition = lastItemBottom;
					_dropLinePosition = linePosition;
					_isDropAfterItem = true;
				}
			}
			bool canSnapToTop = true;
			bool isHorizontalLayout = false;
			if (_orientation == NavigationBarOrientation.Horizontal)
			{
				leftX = _dropLineStartX;
				rightX = _dropLineEndX;
				if (_selectedGroup.View == NavigationBarView.LargeIcons)
				{
					canSnapToTop = false;
				}
				isHorizontalLayout = true;
			}
			else
			{
				leftX = _innerBounds.Left + 5;
				rightX = _innerBounds.Right - 5;
			}
			if (_firstVisibleItem == null || (linePosition == _itemsAreaBounds.Top + DropMarkerSize && canSnapToTop))
			{
				linePosition = (isHorizontalLayout ? (_itemsAreaBounds.Top + 1) : (_itemsAreaBounds.Top + 5));
				if (isErasing)
				{
					_dropLinePosition = linePosition;
				}
				ref Point reference = ref leftMarker[0];
				reference = new Point(leftX, linePosition + DropMarkerSize);
				ref Point topLeftCorner = ref leftMarker[1];
				topLeftCorner = new Point(leftX, linePosition);
				ref Point topLeftTip = ref leftMarker[2];
				topLeftTip = new Point(leftX + DropMarkerSize, linePosition);
				ref Point topRightEdge = ref rightMarker[0];
				topRightEdge = new Point(rightX, linePosition + DropMarkerSize);
				ref Point topRightCorner = ref rightMarker[1];
				topRightCorner = new Point(rightX, linePosition);
				ref Point topRightTip = ref rightMarker[2];
				topRightTip = new Point(rightX - DropMarkerSize, linePosition);
			}
			else if (linePosition == lastItemBottom && (_isDropAfterItem || isErasing))
			{
				ref Point bottomLeftEdge = ref leftMarker[0];
				bottomLeftEdge = new Point(leftX, linePosition - DropMarkerSize);
				ref Point bottomLeftCorner = ref leftMarker[1];
				bottomLeftCorner = new Point(leftX, linePosition);
				ref Point bottomLeftTip = ref leftMarker[2];
				bottomLeftTip = new Point(leftX + DropMarkerSize, linePosition);
				ref Point bottomRightEdge = ref rightMarker[0];
				bottomRightEdge = new Point(rightX, linePosition - DropMarkerSize);
				ref Point bottomRightCorner = ref rightMarker[1];
				bottomRightCorner = new Point(rightX, linePosition);
				ref Point bottomRightTip = ref rightMarker[2];
				bottomRightTip = new Point(rightX - DropMarkerSize, linePosition);
			}
			else
			{
				ref Point midLeftLower = ref leftMarker[0];
				midLeftLower = new Point(leftX, linePosition + 5);
				ref Point midLeftUpper = ref leftMarker[1];
				midLeftUpper = new Point(leftX, linePosition - 5);
				ref Point midLeftTip = ref leftMarker[2];
				midLeftTip = new Point(leftX + DropMarkerSize, linePosition);
				ref Point midRightLower = ref rightMarker[0];
				midRightLower = new Point(rightX, linePosition + 5);
				ref Point midRightUpper = ref rightMarker[1];
				midRightUpper = new Point(rightX, linePosition - 5);
				ref Point midRightTip = ref rightMarker[2];
				midRightTip = new Point(rightX - DropMarkerSize, linePosition);
			}
			if (dropBrush != null)
			{
				graphics.FillPolygon(dropBrush, leftMarker);
				using (Pen dropPen = new Pen(dropBrush))
				{
					graphics.DrawLine(dropPen, leftMarker[2], rightMarker[2]);
				}
				graphics.FillPolygon(dropBrush, rightMarker);
			}
			if (ownsBrush)
			{
				dropBrush.Dispose();
				dropBrush = null;
			}
			else
			{
				navigationBarFormatStyle.DisposeBrushesAndFont();
				groupsStyle.DisposeBrushesAndFont();
				backgroundStyle.DisposeBrushesAndFont();
			}
			graphics.Clip = clip;
			clip.Dispose();
			clipRegion.Dispose();
			clipRegion = null;
			if (ownsGraphics)
			{
				graphics.Dispose();
				graphics = null;
			}
		}

		private void DrawDragHighlight(Rectangle highlightBounds)
		{
			highlightBounds.Location = PointToScreen(highlightBounds.Location);
			ControlPaint.FillReversibleRectangle(highlightBounds, Color.Empty);
		}

		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);
			if (_dragMode == DragMode.AutomaticNoMove || _isDragLocked)
			{
				return;
			}
			if (_dropLinePosition != -1)
			{
				NavigationItem navigationBarItem = GetLastVisibleItem(_selectedGroup);
				if (_orientation == NavigationBarOrientation.Horizontal && (_selectedGroup.View == NavigationBarView.LargeIcons || navigationBarItem == null))
				{
					if (_dropLinePosition - DropMarkerSize < _itemsAreaBounds.Left)
					{
						_dropLinePosition = _itemsAreaBounds.Left + DropMarkerSize;
					}
					Invalidate(new Rectangle(_dropLinePosition - DropMarkerSize, _itemsAreaBounds.Top, 15, _itemsAreaBounds.Height));
				}
				else
				{
					if (_dropLinePosition - DropMarkerSize < _itemsAreaBounds.Top)
					{
						_dropLinePosition = _itemsAreaBounds.Top + DropMarkerSize;
					}
					Invalidate(new Rectangle(_itemsAreaBounds.Left, _dropLinePosition - DropMarkerSize, _itemsAreaBounds.Width, 15));
				}
				_dropLinePosition = -1;
			}
			if (_dragItemIndex != -1)
			{
				UpdateItemDragHighlight(null, null);
			}
			if (_dragGroupIndex != -1)
			{
				UpdateGroupDragHighlight(null, null);
			}
			if (_timer.Enabled)
			{
				_timer.Stop();
				_isDragScrolling = false;
				_isGroupSwitchPending = false;
				_isScrollingForward = false;
				_isScrollingBack = false;
				_dragTargetGroup = null;
			}
		}

		private int GetAutoScrollDirection(int x, int y)
		{
			if (_orientation == NavigationBarOrientation.Vertical)
			{
				if (y + 10 >= _itemsAreaBounds.Bottom && _scrollNextButton.IsButtonEnabled())
				{
					return 2;
				}
				if (y - 10 <= _itemsAreaBounds.Top && _scrollPreviousButton.IsButtonEnabled())
				{
					return 1;
				}
			}
			else
			{
				if (x + 10 >= _itemsAreaBounds.Right && _scrollNextButton.IsButtonEnabled())
				{
					return 2;
				}
				if (x - 10 <= _itemsAreaBounds.Left && _scrollPreviousButton.IsButtonEnabled())
				{
					return 1;
				}
			}
			return -1;
		}

		protected override void OnDragOver(DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
			base.OnDragOver(e);
		}

		private void UpdateDragFeedback(int x, int y)
		{
			if (_dragMode == DragMode.AutomaticNoMove || _isDragLocked)
			{
				return;
			}
			Point clientPoint = new Point(x, y);
			if (!_itemsAreaBounds.Contains(clientPoint))
			{
				if (_isDragScrolling && _timer.Enabled)
				{
					_timer.Stop();
					_isDragScrolling = false;
					_isGroupSwitchPending = false;
					_isScrollingForward = false;
					_isScrollingBack = false;
					_dragTargetGroup = null;
				}
				NavigationGroup navigationBarGroup = FindGroupAt(clientPoint.X, clientPoint.Y);
				Graphics groupGraphics = CreateGraphics();
				UpdateItemDragHighlight(groupGraphics, null);
				if (_dragSourceItem != null && _dragMode == DragMode.Automatic)
				{
					if (navigationBarGroup != _selectedGroup && navigationBarGroup != null)
					{
						if (_dragGroupIndex != navigationBarGroup._index)
						{
							UpdateGroupDragHighlight(groupGraphics, navigationBarGroup);
						}
						_timer.Interval = 400;
						_dragTargetGroup = navigationBarGroup;
						_isGroupSwitchPending = true;
						_timer.Start();
					}
				}
				else if (_pressedGroup != null && _pressedGroup != navigationBarGroup && navigationBarGroup != null)
				{
					if (navigationBarGroup._index != _dragGroupIndex)
					{
						UpdateGroupDragHighlight(groupGraphics, navigationBarGroup);
					}
				}
				else if (_pressedGroup == navigationBarGroup && _dragGroupIndex != -1)
				{
					UpdateGroupDragHighlight(groupGraphics, null);
				}
				groupGraphics.Dispose();
				groupGraphics = null;
			}
			else if (_dragSourceItem != null && !_selectedGroup.Container)
			{
				if (_timer.Enabled && _isGroupSwitchPending)
				{
					UpdateItemDragHighlight(null, null);
					_timer.Stop();
					_isDragScrolling = false;
					_isGroupSwitchPending = false;
					_isScrollingForward = false;
					_isScrollingBack = false;
					_dragTargetGroup = null;
				}
				switch (GetAutoScrollDirection(clientPoint.X, clientPoint.Y))
				{
				case 2:
					if (!_timer.Enabled)
					{
						UpdateItemDragHighlight(null, null);
						_timer.Interval = 100;
						_isScrollingForward = true;
						_isDragScrolling = true;
						_timer.Start();
					}
					break;
				case 1:
					if (!_timer.Enabled)
					{
						UpdateItemDragHighlight(null, null);
						_timer.Interval = 100;
						_isScrollingBack = true;
						_isDragScrolling = true;
						_timer.Start();
					}
					break;
				default:
					if (_timer.Enabled)
					{
						_isDragScrolling = false;
						_isGroupSwitchPending = false;
						_isScrollingForward = false;
						_isScrollingBack = false;
						_timer.Stop();
					}
					break;
				}
				NavigationItem navigationBarItem = FindItemAt(clientPoint.X, clientPoint.Y);
				NavigationItem lastItem = GetLastVisibleItem(_selectedGroup);
				if (navigationBarItem == null && lastItem != null)
				{
					navigationBarItem = lastItem;
					if (_orientation == NavigationBarOrientation.Vertical)
					{
						if (clientPoint.Y < navigationBarItem.GetTextBounds().Bottom)
						{
							navigationBarItem = null;
						}
					}
					else
					{
						int right = navigationBarItem.GetTextBounds().Right;
						if (_selectedGroup.View == NavigationBarView.SmallIcons)
						{
							right = navigationBarItem.GetBounds().Right;
						}
						if (clientPoint.X < right)
						{
							navigationBarItem = null;
						}
					}
				}
				Graphics itemGraphics = CreateGraphics();
				if (_dragGroupIndex != _selectedGroup._index)
				{
					UpdateGroupDragHighlight(itemGraphics, _selectedGroup);
				}
				if (navigationBarItem != null && navigationBarItem != _dragSourceItem)
				{
					int bottom = navigationBarItem.GetTextBounds().Bottom;
					if (_selectedGroup.View == NavigationBarView.SmallIcons || _selectionArea == SelectionArea.FullItem)
					{
						bottom = navigationBarItem.GetBounds().Bottom;
					}
					if (_dragStyle == DragStyle.InvertedRect)
					{
						if (navigationBarItem._index != _dragItemIndex)
						{
							UpdateItemDragHighlight(itemGraphics, navigationBarItem);
						}
					}
					else if (bottom <= _itemsAreaBounds.Bottom)
					{
						int index = navigationBarItem._index;
						if (index != _dragSourceItem._index + 1 || _selectedGroup != _dragSourceItem._owner.GetGroup())
						{
							if (navigationBarItem == lastItem)
							{
								if (_orientation == NavigationBarOrientation.Vertical)
								{
									int halfItemHeight = (bottom - navigationBarItem.GetBounds().Top) / 2;
									if (clientPoint.Y <= navigationBarItem.GetBounds().Top + halfItemHeight)
									{
										_isDropAfterItem = false;
										UpdateItemDropLine(itemGraphics, index);
									}
									else
									{
										_isDropAfterItem = true;
										UpdateItemDropLine(itemGraphics, index);
									}
								}
								else
								{
									int halfItemWidth = navigationBarItem.GetBounds().Width / 2;
									_ = navigationBarItem.GetTextBounds().Right;
									if (_selectedGroup.View == NavigationBarView.SmallIcons)
									{
										_ = navigationBarItem.GetBounds().Right;
									}
									if (clientPoint.X <= navigationBarItem.GetBounds().Left + halfItemWidth)
									{
										_isDropAfterItem = false;
										UpdateItemDropLine(itemGraphics, index);
									}
									else
									{
										_isDropAfterItem = true;
										UpdateItemDropLine(itemGraphics, index);
									}
								}
							}
							else
							{
								_isDropAfterItem = false;
								UpdateItemDropLine(itemGraphics, index);
							}
						}
						else if (navigationBarItem._index == lastItem._index)
						{
							_isDropAfterItem = true;
							UpdateItemDropLine(itemGraphics, index);
						}
						else if (_dropLinePosition != -1)
						{
							UpdateItemDropLine(itemGraphics, -1);
						}
					}
				}
				else
				{
					if (lastItem == null)
					{
						DrawHorizontalDropLine(itemGraphics, 0, false);
					}
					else if (DragStyle == DragStyle.DropLine)
					{
						UpdateItemDropLine(itemGraphics, -1);
					}
					if (DragStyle == DragStyle.InvertedRect)
					{
						UpdateItemDragHighlight(itemGraphics, null);
					}
				}
				itemGraphics.Dispose();
			}
			else if (_dragGroupIndex != -1)
			{
				UpdateGroupDragHighlight(null, null);
			}
		}

		private bool IsBeyondDragThreshold(int x, int y)
		{
			Size dragSize = SystemInformation.DragSize;
			if (!new Rectangle(_dragStartPoint.X - dragSize.Width, _dragStartPoint.Y - dragSize.Height, dragSize.Width * 2, dragSize.Height * 2).Contains(x, y))
			{
				return true;
			}
			return false;
		}

		protected override void OnDragEnter(DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
			base.OnDragEnter(e);
		}

		protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
		{
			if (e.Action != DragAction.Continue && _dragSourceItem != null)
			{
				if (_dragSourceItem != _selectedItem || !_keepSelection)
				{
					_dragSourceItem._itemState = ItemState.Normal;
				}
				_mouseDownItem = null;
				return;
			}
			base.OnQueryContinueDrag(e);
			Point position = Cursor.Position;
			position = PointToClient(position);
			UpdateDragFeedback(position.X, position.Y);
			if (_cancelDrag)
			{
				e.Action = DragAction.Cancel;
			}
			else if (e.EscapePressed)
			{
				if (_dropLinePosition != -1)
				{
					UpdateItemDropLine(null, -1);
				}
				if (_dragItemIndex != -1)
				{
					UpdateItemDragHighlight(null, null);
				}
				if (_dragGroupIndex != -1)
				{
					UpdateGroupDragHighlight(null, null);
				}
				e.Action = DragAction.Cancel;
			}
		}
	}
}
