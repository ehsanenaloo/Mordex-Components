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
		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (_isDragScrolling)
			{
				UpdateItemDragHighlight(null, null);
				if (_isScrollingBack)
				{
					ScrollToPreviousItem();
				}
				else if (_isScrollingForward)
				{
					ScrollToNextItem();
				}
			}
			else
			{
				if (!_isGroupSwitchPending || !_dragTargetGroup.Enabled)
				{
					return;
				}
				_firstVisibleItem = null;
				SetHoverItem(null);
				if (!_hasContainerGroup && _itemsAreaBounds.Height > 0 && _itemsAreaBounds.Width > 0 && _unfoldEffect == UnfoldEffect.Slide)
				{
					if (!_isSlideAnimating)
					{
						AnimateGroupSwitch(_dragTargetGroup);
					}
					else
					{
						SelectedGroup = _dragTargetGroup;
					}
				}
				else
				{
					SelectedGroup = _dragTargetGroup;
				}
				Update();
				_timer.Stop();
				_isGroupSwitchPending = false;
				_dragTargetGroup = null;
				_dragGroupIndex = -1;
				_timer.Stop();
			}
		}

		public HitTest HitTest()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return HitTest(position);
		}

		public HitTest HitTest(int x, int y)
		{
			Point hitPoint = new Point(x, y);
			return HitTest(hitPoint);
		}

		public HitTest HitTest(Point point)
		{
			if (point.Y < _itemsAreaBounds.Top || point.Y > _itemsAreaBounds.Bottom)
			{
				if (FindGroupAt(point.X, point.Y) != null)
				{
					return Mordex.Windows.Navigation.HitTest.NavigationGroup;
				}
			}
			else if (FindItemAt(point.X, point.Y) != null)
			{
				return Mordex.Windows.Navigation.HitTest.NavigationItem;
			}
			return Mordex.Windows.Navigation.HitTest.None;
		}

		public NavigationItem GetItemFromPoint()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return FindItemAt(position.X, position.Y);
		}

		public NavigationItem GetItemFromPoint(int x, int y)
		{
			return FindItemAt(x, y);
		}

		public NavigationItem GetItemFromPoint(Point point)
		{
			return FindItemAt(point.X, point.Y);
		}

		public NavigationGroup GetGroupFromPoint()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return FindGroupAt(position.X, position.Y);
		}

		public NavigationGroup GetGroupFromPoint(int x, int y)
		{
			return FindGroupAt(x, y);
		}

		public NavigationGroup GetGroupFromPoint(Point point)
		{
			return FindGroupAt(point.X, point.Y);
		}

		private void OnScrollPreviousClick(object sender, EventArgs eventArgs)
		{
			ScrollToPreviousItem();
		}

		private void OnScrollNextClick(object sender, EventArgs eventArgs)
		{
			ScrollToNextItem();
		}

		internal bool ScrollToPreviousItem()
		{
			bool isHorizontal = false;
			if (_orientation == NavigationBarOrientation.Horizontal)
			{
				isHorizontal = true;
			}
			NavigationItem navigationBarItem = GetLastVisibleItem(_selectedGroup);
			if (_firstVisibleItem == null)
			{
				return false;
			}
			NavigationItem previousItem = GetPreviousVisibleItem(_firstVisibleItem._index);
			if (previousItem == null)
			{
				return false;
			}
			if (_selectedGroup.View == NavigationBarView.SmallIcons)
			{
				int height = previousItem.GetBounds().Height;
				int width = previousItem.GetBounds().Width;
				foreach (NavigationItem item in _selectedGroup.Items)
				{
					if (!isHorizontal)
					{
						item.UpdateBoundsY(item.GetBoundsY() + height);
					}
					else
					{
						item.UpdateBoundsX(item.GetBoundsX() + width);
					}
				}
				Invalidate(_itemsAreaBounds);
				DisposeRenderers();
				if (navigationBarItem == null)
				{
					_scrollNextButton.SetButtonEnabled(false);
				}
				else if (!isHorizontal)
				{
					if (navigationBarItem.GetBounds().Bottom > _itemsAreaBounds.Bottom && !_scrollNextButton.IsButtonEnabled())
					{
						_scrollNextButton.SetButtonEnabled(true);
					}
				}
				else if (navigationBarItem.GetBounds().Right > _itemsAreaBounds.Right && !_scrollNextButton.IsButtonEnabled())
				{
					_scrollNextButton.SetButtonEnabled(true);
				}
			}
			else
			{
				int verticalShift = previousItem.GetBounds().Height + previousItem.GetTextBounds().Height + 10;
				int horizontalShift = previousItem.GetBounds().Width + 25;
				bool isFullItemSelection = false;
				if (SelectionArea == SelectionArea.FullItem)
				{
					isFullItemSelection = true;
					verticalShift = previousItem.GetBounds().Height;
					horizontalShift -= 25;
				}
				foreach (NavigationItem shiftedItem in _selectedGroup.Items)
				{
					if (!isHorizontal)
					{
						shiftedItem.UpdateBoundsY(shiftedItem.GetBoundsY() + verticalShift);
						if (!isFullItemSelection)
						{
							shiftedItem.UpdateTextY(shiftedItem.GetBounds().Bottom + 2);
							continue;
						}
						shiftedItem.UpdateImageY(shiftedItem.GetBounds().Y + 5);
						shiftedItem.UpdateTextY(shiftedItem.GetImageBounds().Bottom + 2);
					}
					else
					{
						shiftedItem.UpdateBoundsX(shiftedItem.GetBoundsX() + horizontalShift);
						if (!isFullItemSelection)
						{
							shiftedItem.UpdateTextX(shiftedItem.GetTextX() + horizontalShift);
							continue;
						}
						shiftedItem.UpdateImageX(shiftedItem.GetImageX() + horizontalShift);
						shiftedItem.UpdateTextX(shiftedItem.GetBounds().Left);
					}
				}
				Invalidate(_itemsAreaBounds);
				if (!isHorizontal)
				{
					if (navigationBarItem.GetTextBounds().Bottom > _itemsAreaBounds.Bottom && !_scrollNextButton.IsButtonEnabled())
					{
						_scrollNextButton.SetButtonEnabled(true);
					}
				}
				else if (navigationBarItem.GetTextBounds().Right > _itemsAreaBounds.Right && !_scrollNextButton.IsButtonEnabled())
				{
					_scrollNextButton.SetButtonEnabled(true);
				}
			}
			if (!isHorizontal || _selectedGroup.View == NavigationBarView.LargeIcons)
			{
				_firstVisibleItem = previousItem;
			}
			else
			{
				_firstVisibleItem = FindItemAt(_itemsAreaBounds.Left + 1, _itemsAreaBounds.Top + 1);
			}
			if (_firstVisibleItem != null && _firstVisibleItem.Index == 0)
			{
				_scrollPreviousButton.SetButtonEnabled(false);
			}
			return true;
		}

		internal void ScrollToNextItem()
		{
			try
			{
				bool isHorizontal = false;
				if (_orientation == NavigationBarOrientation.Horizontal)
				{
					isHorizontal = true;
				}
				NavigationItem navigationBarItem = GetLastVisibleItem(_selectedGroup);
				if (_firstVisibleItem == null)
				{
					return;
				}
				NavigationItem nextItem = GetNextVisibleItem(_firstVisibleItem._index);
				if (_selectedGroup.View == NavigationBarView.SmallIcons)
				{
					foreach (NavigationItem item in _selectedGroup.Items)
					{
						if (!isHorizontal)
						{
							item.UpdateBoundsY(item.GetBoundsY() - _firstVisibleItem.GetBounds().Height);
						}
						else
						{
							item.UpdateBoundsX(item.GetBoundsX() - _firstVisibleItem.GetBounds().Width);
						}
					}
					Invalidate(_itemsAreaBounds);
					DisposeRenderers();
					if (_firstVisibleItem.Index < navigationBarItem._index)
					{
						if (!isHorizontal)
						{
							_firstVisibleItem = nextItem;
						}
						else
						{
							_firstVisibleItem = FindItemAt(_itemsAreaBounds.Left + 1, _itemsAreaBounds.Top + 1);
						}
					}
					if (_firstVisibleItem.Index > 0 && !_scrollPreviousButton.IsButtonEnabled())
					{
						_scrollPreviousButton.SetButtonEnabled(true);
					}
					if (!isHorizontal)
					{
						if (navigationBarItem == null || navigationBarItem.GetBounds().Bottom < _itemsAreaBounds.Bottom)
						{
							_scrollNextButton.SetButtonEnabled(false);
						}
					}
					else if (navigationBarItem == null || navigationBarItem.GetBounds().Right < _itemsAreaBounds.Right)
					{
						_scrollNextButton.SetButtonEnabled(false);
					}
				}
				else
				{
					int verticalShift = _firstVisibleItem.GetBounds().Height + _firstVisibleItem.GetTextBounds().Height + 10;
					int horizontalShift = _firstVisibleItem.GetBounds().Width + 25;
					bool isFullItemSelection = false;
					if (SelectionArea == SelectionArea.FullItem)
					{
						isFullItemSelection = true;
						verticalShift = _firstVisibleItem.GetBounds().Height;
						horizontalShift -= 25;
					}
					foreach (NavigationItem shiftedItem in _selectedGroup.Items)
					{
						if (!isHorizontal)
						{
							shiftedItem.UpdateBoundsY(shiftedItem.GetBoundsY() - verticalShift);
							if (!isFullItemSelection)
							{
								shiftedItem.UpdateTextY(shiftedItem.GetBounds().Bottom + 2);
								continue;
							}
							shiftedItem.UpdateImageY(shiftedItem.GetBounds().Y + 5);
							shiftedItem.UpdateTextY(shiftedItem.GetImageBounds().Bottom + 2);
						}
						else
						{
							shiftedItem.UpdateBoundsX(shiftedItem.GetBoundsX() - horizontalShift);
							if (!isFullItemSelection)
							{
								shiftedItem.UpdateTextX(shiftedItem.GetTextX() - horizontalShift);
								continue;
							}
							shiftedItem.UpdateTextX(shiftedItem.GetBounds().Left);
							shiftedItem.UpdateImageX(shiftedItem.GetImageX() - horizontalShift);
						}
					}
					Invalidate(_itemsAreaBounds);
					if (nextItem != null && _firstVisibleItem.Index < nextItem._index)
					{
						_firstVisibleItem = nextItem;
					}
					if (_firstVisibleItem != null && _firstVisibleItem.Index > 0 && !_scrollPreviousButton.IsButtonEnabled())
					{
						_scrollPreviousButton.SetButtonEnabled(true);
					}
					if (!isHorizontal)
					{
						if (navigationBarItem.GetTextBounds().Bottom < _itemsAreaBounds.Bottom)
						{
							_scrollNextButton.SetButtonEnabled(false);
						}
					}
					else if (navigationBarItem.GetTextBounds().Right < _itemsAreaBounds.Right)
					{
						_scrollNextButton.SetButtonEnabled(false);
					}
				}
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				ControlFailure.Log(failure);
			}
		}

		internal void OnGroupVisibilityChanged(NavigationGroup changedGroup, bool isVisible)
		{
			if (!isVisible && changedGroup == _selectedGroup)
			{
				NavigationGroup navigationBarGroup = GetNextVisibleGroup();
				if (_selectedGroup == navigationBarGroup)
				{
					navigationBarGroup = GetPreviousVisibleGroup();
				}
				SelectedGroup = navigationBarGroup;
			}
			else if (isVisible && _selectedGroup == null)
			{
				SelectedGroup = changedGroup;
			}
			InvalidateLayout(true);
		}

		private NavigationGroup GetNextVisibleGroup()
		{
			for (int i = _selectedGroup._index + 1; i < _groups.Count; i++)
			{
				if (_groups[i].Visible)
				{
					return _groups[i];
				}
			}
			if (_selectedGroup != null)
			{
				return _selectedGroup;
			}
			return null;
		}

		private NavigationItem FindItemAt(int x, int y)
		{
			if (_selectedGroup == null)
			{
				return null;
			}
			if (_itemsAreaBounds.Contains(x, y))
			{
				foreach (NavigationItem item in _selectedGroup.Items)
				{
					if (item.GetBounds().Contains(x, y))
					{
						return item;
					}
					if (_selectedGroup.View == NavigationBarView.LargeIcons)
					{
						Rectangle labelBounds = item.GetTextBounds();
						labelBounds.Inflate(0, 2);
						if (labelBounds.Contains(x, y))
						{
							return item;
						}
					}
				}
			}
			return null;
		}

		private NavigationGroup FindGroupAt(int x, int y)
		{
			foreach (NavigationGroup item in _groups)
			{
				if (item.Bounds.Contains(x, y))
				{
					return item;
				}
			}
			return null;
		}

		protected override void OnParentVisibleChanged(EventArgs e)
		{
			base.OnParentVisibleChanged(e);
			HideToolTip();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHoverItem(null);
			SetHoverGroup(null);
			HideToolTip();
		}

		private void ShowToolTip(string caption)
		{
			if (!_showToolTips)
			{
				return;
			}
			Form form = null;
			form = FindForm();
			if (form != null && !form.ContainsFocus)
			{
				return;
			}
			if (NavigationGraphicsHelper.IsFeatureAvailable())
			{
				if (_tipWindow == null)
				{
					_tipWindow = new NavigationBarTipWindow(Font, UseCompatibleTextRendering);
				}
				ApplyThemedTooltip(_tipWindow);
				_tipWindow.SetTipFont(Font);
				_tipWindow.SetHideDelay(0);
				_tipWindow.ShowTipAtCursor(caption, this);
			}
			else
			{
				if (_toolTip != null)
				{
					_toolTip.Dispose();
					_toolTip = null;
				}
				_toolTip = new Mordex.Windows.Common.ThemedToolTip();
				_toolTip.SetToolTip(this, caption);
			}
		}

		private void HideToolTip()
		{
			if (_tipWindow != null)
			{
				_tipWindow.DisposeTip();
				_tipWindow = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
			_tipItemIndex = -1;
			_tipGroupIndex = -1;
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			if (base.Enabled)
			{
				_scrollNextButton.SetButtonEnabled(_scrollNextButton.IsButtonEnabled());
				_scrollPreviousButton.SetButtonEnabled(_scrollPreviousButton.IsButtonEnabled());
			}
			else
			{
				_wasScrollForwardEnabled = _scrollNextButton.IsButtonEnabled();
				_wasScrollBackEnabled = _scrollPreviousButton.IsButtonEnabled();
				_scrollNextButton.SetButtonEnabled(false);
				_scrollPreviousButton.SetButtonEnabled(false);
			}
			UpdateScrollButtonStates();
		}

		private void ApplyCursor(Cursor cursor)
		{
			if (cursor == null)
			{
				if (_cursor != null)
				{
					base.Cursor = _cursor;
					_cursor = null;
				}
			}
			else if (cursor != Cursor)
			{
				if (_cursor == null)
				{
					_cursor = Cursor;
				}
				base.Cursor = cursor;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.Button == MouseButtons.Left && _dragStartPoint != Point.Empty && _dragMode != DragMode.Manual)
			{
				Point position = Cursor.Position;
				if (IsBeyondDragThreshold(position.X, position.Y))
				{
					if (_dragSourceItem != null)
					{
						bool allowDrop = AllowDrop;
						NavigationGraphicsHelper.SetAllowDrop(this, true);
						DoDragDrop(_dragSourceItem, DragDropEffects.All);
						ResetDragState();
						NavigationGraphicsHelper.SetAllowDrop(this, allowDrop);
						_dragStartPoint = Point.Empty;
						_cancelDrag = false;
					}
					else if (_pressedGroup != null && _allowDragGroups)
					{
						bool savedAllowDrop = AllowDrop;
						NavigationGraphicsHelper.SetAllowDrop(this, true);
						DoDragDrop(_pressedGroup, DragDropEffects.All);
						ResetDragState();
						NavigationGraphicsHelper.SetAllowDrop(this, savedAllowDrop);
						_dragStartPoint = Point.Empty;
						_cancelDrag = false;
					}
				}
				return;
			}
			if (_itemsAreaBounds.Contains(e.X, e.Y))
			{
				NavigationItem navigationBarItem = FindItemAt(e.X, e.Y);
				SetHoverGroup(null);
				if (navigationBarItem != null && (e.Button != MouseButtons.Left || navigationBarItem == _dragSourceItem) && navigationBarItem.Enabled && _selectedGroup.Enabled)
				{
					ApplyCursor(navigationBarItem.Cursor);
					if (navigationBarItem.ToolTipText != string.Empty && _tipItemIndex != navigationBarItem._index)
					{
						if (_tipItemIndex != -1 || _tipGroupIndex != -1)
						{
							HideToolTip();
						}
						_tipItemIndex = navigationBarItem._index;
						ShowToolTip(navigationBarItem.ToolTipText);
					}
					else if (_tipItemIndex != navigationBarItem._index)
					{
						HideToolTip();
					}
					SetHoverItem(navigationBarItem);
				}
				else
				{
					ApplyCursor(null);
					HideToolTip();
					SetHoverItem(null);
				}
				return;
			}
			NavigationGroup navigationBarGroup = FindGroupAt(e.X, e.Y);
			if (navigationBarGroup != null && (e.Button != MouseButtons.Left || navigationBarGroup == _pressedGroup) && navigationBarGroup.Enabled)
			{
				ApplyCursor(navigationBarGroup.Cursor);
				if (navigationBarGroup.ToolTipText != string.Empty && _tipGroupIndex != navigationBarGroup._index)
				{
					if (_tipItemIndex != -1 || _tipGroupIndex != -1)
					{
						HideToolTip();
					}
					_tipGroupIndex = navigationBarGroup._index;
					ShowToolTip(navigationBarGroup.ToolTipText);
				}
				else if (_tipGroupIndex != navigationBarGroup._index)
				{
					HideToolTip();
				}
				SetHoverGroup(navigationBarGroup);
			}
			else
			{
				ApplyCursor(null);
				HideToolTip();
				SetHoverGroup(null);
			}
			SetHoverItem(null);
		}

		public void RenameItem(NavigationItem item)
		{
			Update();
			if (base.Controls.Contains(_txtRename))
			{
				TryCommitRename();
			}
			if (item != null)
			{
				_txtRename.Tag = item;
				_txtRename.Text = item.Text;
				Rectangle bounds = item.GetTextBounds();
				if (_selectedGroup.View == NavigationBarView.SmallIcons)
				{
					bounds = item.GetBounds();
				}
				else if (_selectionArea == SelectionArea.FullItem)
				{
					bounds.Height += 6;
				}
				bounds.Inflate(-2, -2);
				_txtRename.Bounds = bounds;
				_txtRename.SelectionStart = item.Text.Length;
				_txtRename.ScrollToCaret();
				base.Controls.Add(_txtRename);
				NavigationGraphicsHelper.FocusControl(_txtRename);
			}
		}

		public void RenameGroup(NavigationGroup group)
		{
			Update();
			if (base.Controls.Contains(_txtRename))
			{
				TryCommitRename();
			}
			if (group != null)
			{
				_txtRename.Tag = group;
				_txtRename.Text = group.Text;
				Rectangle bounds = group.Bounds;
				bounds.Inflate(-2, -2);
				_txtRename.Bounds = bounds;
				_txtRename.SelectionStart = group.Text.Length;
				_txtRename.ScrollToCaret();
				base.Controls.Add(_txtRename);
				NavigationGraphicsHelper.FocusControl(_txtRename);
			}
		}

		protected override void OnLeave(EventArgs e)
		{
			TryCommitRename();
			base.OnLeave(e);
		}

		private bool TryCommitRename()
		{
			if (_txtRename.Tag != null)
			{
				if (_txtRename.Tag is NavigationItem)
				{
					NavigationItem navigationBarItem = (NavigationItem)_txtRename.Tag;
					if (_txtRename.Text != navigationBarItem.Text)
					{
						navigationBarItem.Text = _txtRename.Text;
						OnItemRenamed(new ItemEventArgs(navigationBarItem));
					}
					if (base.Controls.Contains(_txtRename))
					{
						base.Controls.Remove(_txtRename);
						return true;
					}
				}
				else if (_txtRename.Tag is NavigationGroup)
				{
					NavigationGroup navigationBarGroup = (NavigationGroup)_txtRename.Tag;
					if (_txtRename.Text != navigationBarGroup.Text)
					{
						navigationBarGroup.Text = _txtRename.Text;
						OnGroupRenamed(new GroupEventArgs(navigationBarGroup));
					}
					if (base.Controls.Contains(_txtRename))
					{
						base.Controls.Remove(_txtRename);
						return true;
					}
				}
				_txtRename.Tag = null;
			}
			return false;
		}

		protected override bool ProcessDialogKey(Keys key)
		{
			switch (key)
			{
			case Keys.Return:
				if (TryCommitRename())
				{
					return true;
				}
				break;
			case Keys.Escape:
				_txtRename.Tag = null;
				if (base.Controls.Contains(_txtRename))
				{
					base.Controls.Remove(_txtRename);
					return true;
				}
				break;
			}
			return base.ProcessDialogKey(key);
		}

		private void ScrollByWheelDelta(int delta)
		{
			if (!base.Created || base.ClientRectangle.Width <= 0 || base.ClientRectangle.Height <= 0)
			{
				return;
			}
			if (delta < 0)
			{
				if (_scrollNextButton != null && _scrollNextButton.Visible && _scrollNextButton.IsButtonEnabled())
				{
					ScrollToNextItem();
				}
			}
			else if (delta > 0 && _scrollPreviousButton != null && _scrollPreviousButton.Visible && _scrollPreviousButton.IsButtonEnabled())
			{
				ScrollToPreviousItem();
			}
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			ScrollByWheelDelta(e.Delta);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			NavigationGraphicsHelper.FocusControl(this);
			TryCommitRename();
			_dragStartPoint = Cursor.Position;
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			if (_itemsAreaBounds.Contains(e.X, e.Y))
			{
				NavigationItem navigationBarItem = FindItemAt(e.X, e.Y);
				if (navigationBarItem != null && navigationBarItem.Enabled && _selectedGroup.Enabled)
				{
					SetHoverItem(null);
					navigationBarItem._itemState = ItemState.SelectedFocus;
					_mouseDownItem = navigationBarItem;
					SetHoverItem(navigationBarItem);
					_dragSourceItem = navigationBarItem;
				}
			}
			else
			{
				NavigationGroup navigationBarGroup = FindGroupAt(e.X, e.Y);
				if (navigationBarGroup != null && navigationBarGroup.Enabled)
				{
					navigationBarGroup._itemState = ItemState.Selected;
					_hoverGroup = navigationBarGroup;
					Invalidate(navigationBarGroup.Bounds);
					_dragSourceItem = null;
					_pressedGroup = navigationBarGroup;
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			_dragStartPoint = Point.Empty;
			if (!_itemsAreaBounds.Contains(e.X, e.Y))
			{
				NavigationGroup navigationBarGroup = FindGroupAt(e.X, e.Y);
				if (navigationBarGroup != null && navigationBarGroup._itemState == ItemState.Selected)
				{
					GroupEventArgs clickArgs = new GroupEventArgs(navigationBarGroup);
					OnGroupClick(clickArgs);
					navigationBarGroup._itemState = ItemState.Normal;
					Invalidate(navigationBarGroup.Bounds);
					Update();
					if (navigationBarGroup != _selectedGroup)
					{
						SetHoverItem(null);
						if (!_hasContainerGroup && _itemsAreaBounds.Height > 0 && _itemsAreaBounds.Width > 0 && _unfoldEffect == UnfoldEffect.Slide)
						{
							AnimateGroupSwitch(navigationBarGroup);
						}
						else
						{
							SelectedGroup = navigationBarGroup;
						}
					}
				}
			}
			else
			{
				NavigationItem navigationBarItem = FindItemAt(e.X, e.Y);
				if (navigationBarItem != null)
				{
					if (navigationBarItem == _mouseDownItem)
					{
						new ItemEventArgs(navigationBarItem);
						if (_dragItemRenderer != null)
						{
							_dragItemRenderer.RestoreScreenArea();
							_dragItemRenderer.DisposeBitmaps();
							_dragItemRenderer = null;
						}
						SelectedItem = navigationBarItem;
						_mouseDownItem = null;
						navigationBarItem.InvokeOnClick();
						Invalidate();
					}
					else if (_mouseDownItem != null)
					{
						_mouseDownItem._itemState = ItemState.Normal;
						InvalidateItem(_mouseDownItem);
						_mouseDownItem = null;
					}
				}
				else if (_mouseDownItem != null)
				{
					_mouseDownItem._itemState = ItemState.Normal;
					InvalidateItem(_mouseDownItem);
					_mouseDownItem = null;
				}
			}
			_pressedGroup = null;
			_dragSourceItem = null;
			if (_selectedItem != null && _selectedItem._itemState != ItemState.Normal && !_keepSelection)
			{
				SetHoverItem(_selectedItem);
				SetHoverItem(null);
			}
		}

		private void SetHoverGroup(NavigationGroup navigationBarNavigationGroup)
		{
			if (_hoverGroup == navigationBarNavigationGroup)
			{
				return;
			}
			if (_hoverGroup != null)
			{
				_hoverGroup._itemState = ItemState.Normal;
				Invalidate(_hoverGroup.Bounds);
			}
			_hoverGroup = navigationBarNavigationGroup;
			if (_hoverGroup != null)
			{
				if (_hoverGroup._itemState != ItemState.Selected)
				{
					_hoverGroup._itemState = ItemState.Focus;
				}
				Invalidate(_hoverGroup.Bounds);
			}
		}

		private void SetHoverItem(NavigationItem hoverItem)
		{
			if (hoverItem == _hoverItem)
			{
				return;
			}
			if (_hoverItem != null)
			{
				if (_hoverItem._itemState == ItemState.SelectedFocus || (_hoverItem._itemState == ItemState.Selected && _keepSelection))
				{
					_hoverItem._itemState = ItemState.Selected;
				}
				else
				{
					_hoverItem._itemState = ItemState.Normal;
				}
				if (_dragItemRenderer != null)
				{
					_dragItemRenderer.RestoreScreenArea();
					_dragItemRenderer.DisposeBitmaps();
					_dragItemRenderer = null;
				}
				InvalidateItem(_hoverItem);
			}
			_hoverItem = hoverItem;
			if (_hoverItem == null || _hoverItem == _txtRename.Tag)
			{
				return;
			}
			if ((_hoverItem == _selectedItem && _keepSelection) || _hoverItem._itemState == ItemState.SelectedFocus)
			{
				if (_keepSelection)
				{
					_hoverItem._itemState = ItemState.SelectedFocus;
				}
				else
				{
					_hoverItem._itemState = ItemState.Selected;
				}
			}
			else
			{
				_hoverItem._itemState = ItemState.Focus;
			}
			if (_selectedGroup.View == NavigationBarView.SmallIcons)
			{
				_isItemLayoutValid = false;
				if (_hoverItem._fullWidth > _hoverItem.GetBounds().Width)
				{
					Graphics overlayGraphics = null;
					NavigationBarFormatStyle navigationBarFormatStyle = null;
					try
					{
						_dragItemRenderer = new NavigationItemRenderer(_hoverItem, _hoverItem._fullWidth, _hoverItem.GetBounds().Height);
						overlayGraphics = _dragItemRenderer.CreateDragImageGraphics();
						overlayGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
						overlayGraphics.TranslateTransform(-_hoverItem.GetBounds().X, -_hoverItem.GetBounds().Y);
						Rectangle overlayBounds = new Rectangle(_hoverItem.GetBounds().X, _hoverItem.GetBounds().Y, _hoverItem._fullWidth, _hoverItem.GetBounds().Height);
						NavigationBarStateStyles navigationBarStateStyles = ItemsStateStyles.MergeWith(_hoverItem.Group.ItemsStateStyles);
						navigationBarFormatStyle = GetItemFormatStyle(_hoverItem, navigationBarStateStyles);
						NavigationBarFormatStyle groupsStyle = CreateBaseFormatStyle();
						NavigationBarFormatStyle backgroundStyle = groupsStyle.MergeWith(_groupsBackgroundFormatStyle);
						navigationBarFormatStyle = backgroundStyle.MergeWith(navigationBarFormatStyle);
						if (navigationBarFormatStyle.BackColor.IsEmpty)
						{
							navigationBarFormatStyle.BackColor = GetEffectiveBackColor();
						}
						Rectangle backgroundBounds = new Rectangle(_itemsAreaBounds.X, _itemsAreaBounds.Y, overlayBounds.Width, _itemsAreaBounds.Height);
						DrawItemsAreaBackground(overlayGraphics, backgroundBounds, navigationBarFormatStyle.GetCachedBackBrush(backgroundBounds), overlayBounds);
						_useImageListIcons = false;
						try
						{
							DrawSmallIconItem(overlayGraphics, _hoverItem, overlayBounds, navigationBarFormatStyle, true);
						}
						finally
						{
							_useImageListIcons = true;
						}
						navigationBarFormatStyle.DisposeBrushesAndFont();
						navigationBarFormatStyle = null;
						overlayGraphics.Dispose();
						overlayGraphics = null;
						Point location = _hoverItem.GetBounds().Location;
						location = PointToScreen(location);
						_dragItemRenderer.CaptureAndDrawDragImage(location);
						return;
					}
					catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
					{
						overlayGraphics?.Dispose();
						overlayGraphics = null;
						_dragItemRenderer?.DisposeBitmaps();
						_dragItemRenderer = null;
						InvalidateItem(_hoverItem);
						return;
					}
					finally
					{
						navigationBarFormatStyle?.DisposeBrushesAndFont();
						overlayGraphics?.Dispose();
					}
				}
				InvalidateItem(_hoverItem);
			}
			else
			{
				InvalidateItem(_hoverItem);
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new MxNavigationBarAccessibleObject(this);
			}
			return _accessibleObject;
		}

		public void EnsureVisible(NavigationItem item)
		{
			if (!base.Visible || !item.Visible || item.NavigationBar != this || !item.Group.Visible)
			{
				return;
			}
			if (!_isGroupLayoutValid)
			{
				Graphics groupsGraphics = CreateGraphics();
				LayoutGroups(groupsGraphics);
				groupsGraphics.Dispose();
			}
			if (_firstVisibleItem == null)
			{
				Update();
				if (_firstVisibleItem == null)
				{
					return;
				}
			}
			if (_selectedGroup != item.Group)
			{
				SelectedGroup = item.Group;
				Update();
			}
			if (!_isItemLayoutValid)
			{
				Graphics itemsGraphics = CreateGraphics();
				LayoutVerticalItems(itemsGraphics, SelectedGroup);
				itemsGraphics.Dispose();
			}
			if (_orientation == NavigationBarOrientation.Vertical)
			{
				while (item.GetItemBottom() > _itemsAreaBounds.Bottom && item.GetBounds().Top > _itemsAreaBounds.Top)
				{
					int top = item.GetBounds().Top;
					ScrollToNextItem();
					if (item.GetBounds().Top == top)
					{
						break;
					}
				}
				while (item.GetBounds().Top < _itemsAreaBounds.Top && ScrollToPreviousItem())
				{
				}
			}
			else
			{
				while (item.GetBounds().Right > _itemsAreaBounds.Right && item.GetBounds().Left > _itemsAreaBounds.Left)
				{
					int left = item.GetBounds().Left;
					ScrollToNextItem();
					if (item.GetBounds().Left == left)
					{
						break;
					}
				}
				while (item.GetBounds().Left < _itemsAreaBounds.Left && ScrollToPreviousItem())
				{
				}
			}
			Invalidate();
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			if (base.Enabled && base.Visible)
			{
				foreach (NavigationGroup group in Groups)
				{
					if (!group.Visible || !group.Enabled)
					{
						continue;
					}
					if (HasMnemonic(group.Text))
					{
						char groupMnemonic = char.ToLower(GetMnemonicChar(group.Text));
						if (charCode == groupMnemonic)
						{
							GroupEventArgs e = new GroupEventArgs(group);
							SelectedGroup = group;
							OnGroupClick(e);
							return true;
						}
					}
					foreach (NavigationItem item in group.Items)
					{
						if (item.Enabled && item.Group.Enabled && item.Visible && HasMnemonic(item.Text))
						{
							char itemMnemonic = char.ToLower(GetMnemonicChar(item.Text));
							if (charCode == itemMnemonic)
							{
								item.InvokeOnClick();
								SelectedItem = item;
								return true;
							}
						}
					}
				}
			}
			return base.ProcessMnemonic(charCode);
		}

		private bool HasMnemonic(string text)
		{
			return GetMnemonicChar(text) != '\0';
		}

		/// <summary>Returns the character after the first single ampersand, or '\0' when the text has no mnemonic.</summary>
		private static char GetMnemonicChar(string text)
		{
			for (int i = 0; i < text.Length - 1; i++)
			{
				if (text[i] != '&')
				{
					continue;
				}
				if (text[i + 1] == '&')
				{
					i++;
					continue;
				}
				return text[i + 1];
			}
			return '\0';
		}
	}
}
