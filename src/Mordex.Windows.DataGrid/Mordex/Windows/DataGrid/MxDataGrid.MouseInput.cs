using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Accessibility;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class MxDataGrid
	{
		private bool TryStartDragTracking(int value, int otherValue)
		{
			_isDragPending = true;
			Size dragSize = SystemInformation.DragSize;
			_mouseDownPoint = new Point(value, otherValue);
			Point position = PointerPosition.Current;
			_dragStartBounds = new Rectangle(position.X - dragSize.Width / 2, position.Y - dragSize.Height / 2, dragSize.Width, dragSize.Height);
			if (!_dragStartBounds.Contains(PointToScreen(_mouseDownPoint)))
			{
				_isDragPending = false;
				return false;
			}
			_dragStartBounds = new Rectangle(value - dragSize.Width / 2, otherValue - dragSize.Height / 2, dragSize.Width, dragSize.Height);
			return true;
		}

		private void ToggleDragSourceHighlight(Graphics graphics)
		{
			if (_pressedHeaderItem != null)
			{
				if (_pressedHeaderItem.UsesVisualStyle(_gridControlStyle.GetLayoutState()))
				{
					_pressedHeaderItem._isPressed = true;
					InvalidateLayoutItem(_pressedHeaderItem);
					Update();
				}
				else
				{
					Rectangle a = AdjustRectangleToViewport(_pressedHeaderItem.GetAbsoluteItemRect(), _pressedHeaderItem._isHorizontallyScrolled);
					a = Rectangle.Intersect(a, base.ClientRectangle);
					a.Location = PointToScreen(a.Location);
					ControlPaint.FillReversibleRectangle(a, Color.Black);
				}
			}
		}

		private void HandleColumnHeaderClickSort(GridColumn column)
		{
			if (column == null)
			{
				return;
			}
			OnColumnHeaderClick(new ColumnActionEventArgs(column));
			if (!_automaticSort)
			{
				return;
			}
			_isCurrentCellChanging = true;
			bool condition = (Control.ModifierKeys & Keys.Shift) != 0;
			bool otherCondition = (Control.ModifierKeys & Keys.Control) != 0;
			bool nextCondition = false;
			foreach (GridTable item in _gridTableCollection)
			{
				GridGroup gridGroup = item.FindGroupForColumn(column);
				if (gridGroup == null)
				{
					continue;
				}
				nextCondition = true;
				GroupsChangingEventArgs e = new GroupsChangingEventArgs(gridGroup, column.Table, GroupChangeAction.SortOrderChange, gridGroup.Index);
				OnGroupsChanging(e);
				if (!e.Cancel)
				{
					if (gridGroup.SortOrder == SortOrder.Ascending)
					{
						gridGroup.SortOrder = SortOrder.Descending;
					}
					else
					{
						gridGroup.SortOrder = SortOrder.Ascending;
					}
					OnGroupsChanged(new GroupsChangedEventArgs(gridGroup.Table));
				}
			}
			foreach (GridSortKey sortKey in column.Table.SortKeys)
			{
				if (sortKey.Column != column || !column.AllowSort)
				{
					continue;
				}
				nextCondition = true;
				if (otherCondition)
				{
					column.Table.SortKeys.Remove(sortKey);
				}
				else if (condition)
				{
					if (sortKey.SortOrder == SortOrder.Ascending)
					{
						sortKey.SortOrder = SortOrder.Descending;
					}
					else
					{
						sortKey.SortOrder = SortOrder.Ascending;
					}
				}
				else
				{
					column.Table.SuspendBaseSort();
					column.Table.SortKeys.Clear();
					if (sortKey.SortOrder == SortOrder.Ascending)
					{
						column.Table.SortKeys.Add(new GridSortKey(column, SortOrder.Descending));
					}
					else
					{
						column.Table.SortKeys.Add(new GridSortKey(column, SortOrder.Ascending));
					}
					column.Table.ResumeBaseSort();
				}
				OnSortKeysChanged(EventArgs.Empty);
				break;
			}
			if (!nextCondition && !otherCondition && column.AllowSort)
			{
				if (!condition)
				{
					column.Table.SortKeys.Clear();
				}
				column.Table.SortKeys.Add(new GridSortKey(column));
				OnSortKeysChanged(EventArgs.Empty);
			}
			_isCurrentCellChanging = false;
			EnsureVisible(Row);
		}

		private void HandleGroupHeaderClickSort(GridGroup group)
		{
			if (group == null)
			{
				return;
			}
			OnGroupByBoxHeaderClick(new GroupActionEventArgs(group));
			if (!_automaticSort)
			{
				return;
			}
			GroupsChangingEventArgs e = new GroupsChangingEventArgs(group, group.Table, GroupChangeAction.SortOrderChange, group.Index);
			OnGroupsChanging(e);
			if (!e.Cancel)
			{
				if (group.SortOrder == SortOrder.Ascending)
				{
					group.SortOrder = SortOrder.Descending;
				}
				else
				{
					group.SortOrder = SortOrder.Ascending;
				}
				OnGroupsChanged(new GroupsChangedEventArgs(group.Table));
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			bool condition = false;
			_dragThresholdBounds = Rectangle.Empty;
			if (GetPressedLayoutItem() != null)
			{
				condition = true;
				ReleasePressedLayoutItem(e);
			}
			if (CurrentCellDroppedDown)
			{
				Control control = _gridColumnContainerControl.GetDropControl();
				if (control != null && !control.Capture)
				{
					GridHelper.SetCapture(control, true);
				}
			}
			if (_isMouseDownSelectionPending)
			{
				_isMouseDownSelectionPending = false;
				_isCurrentCellChanging = false;
				EnsureVisible();
			}
			_isDragSelectionSuppressed = false;
			_dragSelectStartRow = -1;
			_selectionRectEnd = new Point(-3, -3);
			_isCellDragSelecting = false;
			StopAutoScrollTimer();
			if (_isButtonPressActive)
			{
				EndColumnResize(false);
			}
			if (_isCellMousePressed)
			{
				EndCardResize(false);
			}
			if (_isHeaderPressed)
			{
				EndHeaderDrag(false);
			}
			if (_isDragPending)
			{
				_isDragPending = false;
			}
			if (_pressedHeaderItem != null)
			{
				if (AdjustRectangleToViewport(_pressedHeaderItem.GetAbsoluteItemRect(), _pressedHeaderItem._isHorizontallyScrolled).Contains(e.X, e.Y))
				{
					if (_pressedHeaderItem._type == LayoutItemType.ColumnHeader)
					{
						if (_pressedHeaderItem._column.ActAsSelector && _pressedHeaderItem._column.UseHeaderSelector)
						{
							ToggleRowCheckState(_pressedHeaderItem.GetMordexRow());
							OnColumnHeaderClick(new ColumnActionEventArgs(_pressedHeaderItem._column));
						}
						else
						{
							HandleColumnHeaderClickSort(_pressedHeaderItem._column);
						}
					}
					else if (_pressedHeaderItem._type == LayoutItemType.GroupByBoxHeader)
					{
						HandleGroupHeaderClickSort(_pressedHeaderItem._gridGroup);
					}
				}
				_pressedHeaderItem._isPressed = false;
				_pressedHeaderItem = null;
				ToggleDragSourceHighlight(null);
				Invalidate();
			}
			if (_lastClickedLayoutItem != null)
			{
				GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(e.X, e.Y);
				if (_lastClickedLayoutItem._type == gridGroupLayoutState._type && _lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow == gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow && _lastClickedLayoutItem._column == gridGroupLayoutState._column)
				{
					ApplyClickSelection(gridGroupLayoutState, MouseButtons.Left, true);
				}
				_lastClickedLayoutItem = null;
			}
			if (_mouseClickCount == 2 && !condition)
			{
				_mouseClickCount = 0;
				OnDoubleClick(EventArgs.Empty);
				OnMouseDoubleClick(e);
			}
			else if (_mouseClickCount == 1)
			{
				_mouseClickCount = 0;
				OnClick(EventArgs.Empty);
				OnMouseClick(e);
			}
			base.OnMouseUp(e);
			if (_selectorPressedItem != null && (_selectorPressedItem._type == LayoutItemType.CardSelector || _selectorPressedItem._type == LayoutItemType.RowSelector))
			{
				_selectorPressedItem._isPressed = false;
				Rectangle rc = AdjustRectangleToViewport(_selectorPressedItem.GetAbsoluteItemRect(), _selectorPressedItem._isHorizontallyScrolled);
				if (rc.Contains(e.X, e.Y))
				{
					if (_selectorPressedItem._gridGroupLayoutStateOwner is MxRowLayoutState && ((MxRowLayoutState)_selectorPressedItem._gridGroupLayoutStateOwner)._rowType == LayoutRowType.ColumnHeader)
					{
						ToggleRowCheckState(_selectorPressedItem.GetMordexRow());
					}
					else
					{
						RowCheckState checkState = _selectorPressedItem._gridGroupLayoutStateOwner.GridRow.CheckState;
						RowCheckState rowCheckState = RowCheckState.Checked;
						if (checkState == RowCheckState.Checked)
						{
							rowCheckState = RowCheckState.Unchecked;
						}
						CheckStateChangeType checkStateChangeType = CheckStateChangeType.RowChange;
						if (_selectorPressedItem._gridGroupLayoutStateOwner.GridRow.RowType == RowType.GroupHeader)
						{
							checkStateChangeType = CheckStateChangeType.GroupRowChange;
						}
						if (RaiseRowCheckStateChanging(_selectorPressedItem._gridGroupLayoutStateOwner.GridRow, checkStateChangeType, checkState, rowCheckState))
						{
							_selectorPressedItem._gridGroupLayoutStateOwner.GridRow.IsChecked = !_selectorPressedItem._gridGroupLayoutStateOwner.GridRow.IsChecked;
							RefreshRowCountAndInvalidate();
							OnRowCheckStateChanged(new RowCheckStateChangeEventArgs(_selectorPressedItem._gridGroupLayoutStateOwner.GridRow, checkStateChangeType, checkState, rowCheckState));
						}
					}
				}
				_selectorPressedItem = null;
				Invalidate(rc);
			}
			if (_mouseDownLayoutItem != null)
			{
				GridGroupLayoutState linkCellAtPoint = HitTester.FindLinkCellAtPoint(e.X, e.Y);
				if (linkCellAtPoint == _mouseDownLayoutItem)
				{
					OnLinkClicked(new ColumnActionEventArgs(linkCellAtPoint._column));
				}
				_mouseDownLayoutItem = null;
			}
		}

		private void HandleMouseButtonDown(MouseEventArgs eventArgs)
		{
			_mouseClickCount = eventArgs.Clicks;
			if (eventArgs.Clicks == 2)
			{
				GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(eventArgs.X, eventArgs.Y);
				if (gridGroupLayoutState.IsButtonItem() && !gridGroupLayoutState.IsDisabled())
				{
					SetPressedLayoutItem(gridGroupLayoutState);
				}
				base.OnMouseDown(eventArgs);
				return;
			}
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			_mouseButtons = eventArgs.Button;
			_dragThresholdBounds = Rectangle.Empty;
			if (!base.ContainsFocus && !(this is GridDropDown))
			{
				FocusEditorOrGrid();
			}
			if (_gridColumnContainerControl != null)
			{
				Control childAtPoint = GetChildAtPoint(new Point(eventArgs.X, eventArgs.Y));
				if (_gridColumnContainerControl == childAtPoint && _gridColumnContainerControl.GetColumn() != null && _gridColumnContainerControl.GetCurrentEditType() == EditType.CheckBox)
				{
					_gridColumnContainerControl.ToggleEditorCheckBox();
				}
			}
			bool focused = base.ContainsFocus || base.DesignMode || this is GridDropDown;
			if (focused && !(_view == View.TableView ? TryBeginColumnResize(eventArgs) : TryBeginCardResize(eventArgs)))
			{
				HandleLayoutItemMouseDown(eventArgs);
			}
			base.OnMouseDown(eventArgs);
		}

		private void HandleLayoutItemMouseDown(MouseEventArgs eventArgs)
		{
			if (eventArgs.Button == MouseButtons.Left && CanStartRowSelectDrag(eventArgs.X, eventArgs.Y) && Control.ModifierKeys == Keys.None)
			{
				_dragSelectStartRow = RowPositionFromPoint(eventArgs.X, eventArgs.Y);
				if (_dragSelectStartRow < 0)
				{
					_dragSelectStartRow = -1;
				}
			}
			GridGroupLayoutState layoutItemAtPoint = HitTester.FindLayoutItemAtPoint(eventArgs.X, eventArgs.Y);
			if (layoutItemAtPoint.IsHitTestable(this))
			{
				_lastClickedLayoutItem = null;
				ApplyClickSelection(layoutItemAtPoint, eventArgs.Button, false);
				_dragThresholdBounds = Rectangle.Empty;
				if (SelectionMode != SelectionMode.MultipleCellSelection)
				{
					if (_lastClickedLayoutItem != null)
					{
						if (!_lastClickedLayoutItem.IsButtonItem())
						{
							_dragThresholdBounds = new Rectangle(eventArgs.X - SystemInformation.DragSize.Width, eventArgs.Y - SystemInformation.DragSize.Height, SystemInformation.DragSize.Width * 2, SystemInformation.DragSize.Height * 2);
						}
					}
					else if ((eventArgs.Button == MouseButtons.Right || EditMode == EditMode.EditOff) && layoutItemAtPoint != null && !layoutItemAtPoint.IsButtonItem())
					{
						_dragThresholdBounds = new Rectangle(eventArgs.X - SystemInformation.DragSize.Width, eventArgs.Y - SystemInformation.DragSize.Height, SystemInformation.DragSize.Width * 2, SystemInformation.DragSize.Height * 2);
					}
				}
			}
			if (layoutItemAtPoint._type == LayoutItemType.CardSelector || layoutItemAtPoint._type == LayoutItemType.RowSelector)
			{
				layoutItemAtPoint._isPressed = true;
				_selectorPressedItem = layoutItemAtPoint;
				Invalidate(AdjustRectangleToViewport(layoutItemAtPoint.GetAbsoluteItemRect(), layoutItemAtPoint._isHorizontallyScrolled));
			}
			if (layoutItemAtPoint.IsButtonItem() && !layoutItemAtPoint.IsDisabled())
			{
				layoutItemAtPoint = HitTester.FindLayoutItemAtPoint(eventArgs.X, eventArgs.Y);
				if (layoutItemAtPoint.IsButtonItem() && !layoutItemAtPoint.IsDisabled())
				{
					if (layoutItemAtPoint.GetCell() != null && AreSameLogicalRow(layoutItemAtPoint.GetCell().Row, CurrentRow) && layoutItemAtPoint.GetCell().Column == CurrentColumn)
					{
						SetPressedLayoutItem(layoutItemAtPoint);
					}
					else
					{
						Point point = PointToClient(PointerPosition.Current);
						if (layoutItemAtPoint._type == LayoutItemType.CellColumnButton && layoutItemAtPoint.GetCell() != null && HitTester.FindLayoutItemAtPoint(point.X, point.Y) == layoutItemAtPoint && layoutItemAtPoint.GetCell().Column != null && !IsCellSelectable(layoutItemAtPoint.GetCell().Row, layoutItemAtPoint.GetCell().Column))
						{
							SetPressedLayoutItem(layoutItemAtPoint);
						}
						else
						{
							_gridGridGroupLayoutState = null;
						}
					}
				}
				else
				{
					SetPressedLayoutItem(null);
				}
			}
			if (eventArgs.Button == MouseButtons.Left && !ToggleExpandBoxAtPoint(eventArgs, layoutItemAtPoint))
			{
				_mouseDownLayoutItem = HitTester.FindLinkCellAtPoint(eventArgs.X, eventArgs.Y);
			}
		}

		// Expands or collapses the row or card whose expand box is under the mouse. Returns false when there is none.
		private bool ToggleExpandBoxAtPoint(MouseEventArgs eventArgs, GridGroupLayoutState layoutItemAtPoint)
		{
			if (_view == View.TableView)
			{
				int expandBoxRowAtPoint = HitTester.FindExpandBoxRowAtPoint(eventArgs.X, eventArgs.Y);
				if (expandBoxRowAtPoint == -1)
				{
					return false;
				}
				MordexRow mordexRow = GetRowManager()[expandBoxRowAtPoint];
				if (_row != null && _row.GetLogicalRow() == mordexRow)
				{
					mordexRow.Expanded = !mordexRow.Expanded;
				}
				else if (!SelectOnExpand)
				{
					mordexRow.Expanded = !mordexRow.Expanded;
					if (_row != null && !_row.IsVisible)
					{
						Row = expandBoxRowAtPoint;
					}
				}
				if (layoutItemAtPoint._type == LayoutItemType.SelfReferencingExpandBox && layoutItemAtPoint._column != null && layoutItemAtPoint._column.Table.SelfReferencingSettings.AutoSizeColumnOnExpand)
				{
					AutoSizeColumnToContent(layoutItemAtPoint._column, true);
				}
			}
			else
			{
				int cardExpandBoxAtPoint = HitTester.FindCardExpandBoxAtPoint(eventArgs.X, eventArgs.Y);
				if (cardExpandBoxAtPoint == -1)
				{
					return false;
				}
				if (!DataChanged || UpdateData())
				{
					GridRow row = GetRow(cardExpandBoxAtPoint);
					row.Expanded = !row.Expanded;
					RefreshRowCountAndInvalidate();
				}
			}
			return true;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			HandleMouseButtonDown(e);
			ShowTooltipForLayoutItem(null);
		}

		private void ApplyClickSelection(GridGroupLayoutState lastClickedLayoutItem, MouseButtons mouseButtons, bool condition)
		{
			if (lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow != null && lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow.Selected && mouseButtons == MouseButtons.Right)
			{
				return;
			}
			bool otherCondition = false;
			if ((lastClickedLayoutItem._type == LayoutItemType.Cell || lastClickedLayoutItem._type == LayoutItemType.CardCell || lastClickedLayoutItem._type == LayoutItemType.FilterRowCell || lastClickedLayoutItem._type == LayoutItemType.GroupTotalCell || lastClickedLayoutItem._type == LayoutItemType.TotalRowCell) && !IsCellSelectable(lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow, lastClickedLayoutItem._column))
			{
				otherCondition = true;
			}
			if (lastClickedLayoutItem.IsButtonItem())
			{
				otherCondition = true;
			}
			if (otherCondition)
			{
				_isCurrentCellChanging = true;
			}
			switch (SelectionMode)
			{
			case SelectionMode.SingleSelection:
				if (!lastClickedLayoutItem.IsButtonItem() && !condition && lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow != null && lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow.Selected)
				{
					_lastClickedLayoutItem = lastClickedLayoutItem;
					_isCurrentCellChanging = false;
					return;
				}
				if (!CanFocusLayoutItem(lastClickedLayoutItem, true))
				{
					break;
				}
				SyncCellSelectionWithCurrent();
				if (lastClickedLayoutItem._column == _currentColumn && mouseButtons == MouseButtons.Left)
				{
					bool nextCondition = true;
					if (lastClickedLayoutItem._type == LayoutItemType.CardHeader)
					{
						nextCondition = false;
					}
					else if (lastClickedLayoutItem.IsSideButton())
					{
						nextCondition = false;
					}
					BeginCellEdit(nextCondition, '\0', _delayEditionOnClick && !lastClickedLayoutItem.IsSideButton());
				}
				break;
			case SelectionMode.MultipleSelectionSameTable:
				if (!lastClickedLayoutItem.IsButtonItem() && !condition && lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow != null && lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow.Selected)
				{
					_lastClickedLayoutItem = lastClickedLayoutItem;
					_isCurrentCellChanging = false;
					return;
				}
				if (Control.ModifierKeys == Keys.Shift)
				{
					if (lastClickedLayoutItem.GetOwnerTable() != CurrentTable)
					{
						_isCurrentCellChanging = false;
						return;
					}
					if (CanFocusLayoutItem(lastClickedLayoutItem, true))
					{
						if (_selectionAnchorRow < 0)
						{
							_selectionAnchorRow = Row;
						}
						SelectRowRange(_selectionAnchorRow, Row, true);
					}
				}
				else if (Control.ModifierKeys == Keys.Control)
				{
					if (lastClickedLayoutItem.GetOwnerTable() != CurrentTable)
					{
						_isCurrentCellChanging = false;
						return;
					}
					if (lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow != null && CanFocusLayoutItem(lastClickedLayoutItem, true))
					{
						_selectionAnchorRow = Row;
						if (lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow.Selected)
						{
							RemoveSelectedItemAtPosition(Row);
						}
						else
						{
							AddRowToSelection(Row);
						}
					}
				}
				else if (CanFocusLayoutItem(lastClickedLayoutItem, true))
				{
					_selectionAnchorRow = Row;
					SyncCellSelectionWithCurrent();
					if (lastClickedLayoutItem._column == _currentColumn && mouseButtons == MouseButtons.Left)
					{
						BeginCellEdit(true, '\0', _delayEditionOnClick && !lastClickedLayoutItem.IsSideButton());
					}
				}
				break;
			case SelectionMode.MultipleCellSelection:
			{
				Point point = new Point(-3, -3);
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
				{
					point = new Point(Col, Row);
				}
				if (lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow == null)
				{
					break;
				}
				bool previousCondition = false;
				if (lastClickedLayoutItem.GetCell() != null && lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow == CurrentRow && lastClickedLayoutItem._column == CurrentColumn && !lastClickedLayoutItem.IsButtonItem())
				{
					previousCondition = true;
				}
				_isCurrentCellChanging = true;
				_isMouseDownSelectionPending = true;
				if (!CanFocusLayoutItem(lastClickedLayoutItem, true))
				{
					break;
				}
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
				{
					using (BeginSelectionChangeBatch())
					{
						if (lastClickedLayoutItem._type == LayoutItemType.RowHeader)
						{
							if (GetSelectionAnchorColumn() == null && _selectionAnchorRow >= 0)
							{
								SelectRowRangeWithAllCells(_selectionAnchorRow, Row);
								break;
							}
							if (_selectionAnchorRow < 0 || GetSelectionAnchorColumn() == null)
							{
								_selectionAnchorRow = Row;
							}
							SelectRowRange(_selectionAnchorRow, Row, true);
							break;
						}
						GridCell gridCell = lastClickedLayoutItem.GetCell();
						if (gridCell == null)
						{
							break;
						}
						if (_selectionRectStart == new Point(-3, -3))
						{
							_selectionRectStart = new Point(gridCell.Column.Position, Row);
						}
						if (_selectionRectStart.Y < 0)
						{
							_selectionRectStart.Y = Row;
						}
						Point otherPoint = new Point(gridCell.Column.Position, Row);
						bool firstCondition = true;
						if (IsValidRowPosition(_selectionRectStart.Y) && _row != null)
						{
							GridRow row = GetRow(_selectionRectStart.Y);
							if (row.Table != _row.Table)
							{
								SyncCellSelectionWithCurrent();
								_selectionRectStart = otherPoint;
								firstCondition = false;
							}
						}
						if (firstCondition)
						{
							Rectangle rectangle = RectangleFromCorners(_selectionRectStart, point);
							if (_selectionRectStart.Y >= 0 && otherPoint.Y < 0)
							{
								otherPoint.Y = 0;
							}
							Rectangle otherRectangle = RectangleFromCorners(_selectionRectStart, otherPoint);
							DeselectCellsOutsideRange(rectangle, otherRectangle);
							SelectCellsInRange(otherRectangle);
						}
					}
					break;
				}
				if (Control.ModifierKeys == Keys.Control)
				{
					_selectionAnchorRow = Row;
					using (BeginSelectionChangeBatch())
					{
						if (lastClickedLayoutItem._type == LayoutItemType.RowHeader)
						{
							SetLastSelectedColumn(null);
							GridRow gridRow = lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow;
							if (RowStateHelper.ComputeCellSelectionStatus(gridRow.GetLogicalRow()) == CellSelectionStatus.AllCellsAreSelected)
							{
								RemoveSelectedItemAtPosition(gridRow.Position);
								RowStateHelper.DeselectAllRowCells(gridRow.GetLogicalRow());
							}
							else
							{
								AddRowToSelection(gridRow.Position);
								RowStateHelper.SelectAllRowCells(gridRow.GetLogicalRow());
								_isDragSelectionSuppressed = true;
							}
							break;
						}
						GridCell cell = lastClickedLayoutItem.GetCell();
						if (cell != null)
						{
							SetLastSelectedColumn(cell.Column);
							SetCellSelected(cell, !cell.Selected);
							if (cell.Selected)
							{
								_isCellDragSelecting = lastClickedLayoutItem._type != LayoutItemType.GroupHeaderCell && lastClickedLayoutItem._type != LayoutItemType.GroupTotalCell && !lastClickedLayoutItem.IsButtonItem();
								_selectionRectStart = new Point(cell.Column.Position, cell.Row.Position);
							}
						}
					}
					break;
				}
				using (BeginSelectionChangeBatch())
				{
					_selectionAnchorRow = Row;
					if (lastClickedLayoutItem._type == LayoutItemType.RowHeader)
					{
						SetLastSelectedColumn(null);
						GridRow otherGridRow = lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow;
						SyncRowSelectionWithCurrent();
						if (RowStateHelper.ComputeCellSelectionStatus(otherGridRow.GetLogicalRow()) != CellSelectionStatus.AllCellsAreSelected)
						{
							RowStateHelper.SelectAllRowCells(otherGridRow.GetLogicalRow());
							RaiseSelectionChangedOrDefer();
							InvalidateGridRowLayout(otherGridRow);
						}
						_isDragSelectionSuppressed = true;
					}
					else
					{
						GridCell otherCell = lastClickedLayoutItem.GetCell();
						if (otherCell != null)
						{
							SetLastSelectedColumn(otherCell.Column);
							_isCellDragSelecting = lastClickedLayoutItem._type != LayoutItemType.GroupHeaderCell && lastClickedLayoutItem._type != LayoutItemType.GroupTotalCell && !lastClickedLayoutItem.IsButtonItem();
							_selectionRectStart = new Point(otherCell.Column.Position, otherCell.Row.Position);
							SyncCellSelectionWithCurrent();
						}
						else
						{
							SetLastSelectedColumn(null);
							_isCellDragSelecting = false;
							SyncRowSelectionWithCurrent();
							if (lastClickedLayoutItem._type == LayoutItemType.ExpandBox && _row != null)
							{
								RowStateHelper.SelectAllRowCells(_row.GetLogicalRow());
							}
						}
					}
					if (previousCondition && mouseButtons == MouseButtons.Left)
					{
						_isCellDragSelecting = false;
						_isCurrentCellChanging = false;
						EnsureVisible();
						BeginCellEdit(true, '\0', _delayEditionOnClick && !lastClickedLayoutItem.IsSideButton());
					}
				}
				break;
			}
			case SelectionMode.MultipleSelection:
				if (!lastClickedLayoutItem.IsButtonItem() && !condition && lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow != null && lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow.Selected && !lastClickedLayoutItem.IsButtonItem())
				{
					_lastClickedLayoutItem = lastClickedLayoutItem;
					_isCurrentCellChanging = false;
					return;
				}
				if (lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow == null || !CanFocusLayoutItem(lastClickedLayoutItem, true))
				{
					break;
				}
				if (Control.ModifierKeys == Keys.Shift)
				{
					SelectRowRange(_selectionAnchorRow, Row, false);
					break;
				}
				if (Control.ModifierKeys == Keys.Control)
				{
					_selectionAnchorRow = Row;
					if (lastClickedLayoutItem._gridGroupLayoutStateOwner.GridRow.Selected)
					{
						RemoveSelectedItemAtPosition(Row);
					}
					else
					{
						AddRowToSelection(Row);
					}
					break;
				}
				_selectionAnchorRow = Row;
				SyncCellSelectionWithCurrent();
				if (lastClickedLayoutItem._column == _currentColumn && mouseButtons == MouseButtons.Left)
				{
					BeginCellEdit(true, '\0', _delayEditionOnClick && !lastClickedLayoutItem.IsSideButton());
				}
				break;
			}
			if (otherCondition)
			{
				_isCurrentCellChanging = false;
				EnsureVisible(Row);
			}
		}

		private void BeginHeaderDragCheck(MouseEventArgs eventArgs, GridGroupLayoutState pressedHeaderItem)
		{
			if (eventArgs.Button != MouseButtons.Left)
			{
				return;
			}
			if (_isHeaderPressed)
			{
				EndHeaderDrag(true);
				return;
			}
			bool allowDrag = false;
			bool otherCancel = false;
			if (_allowColumnDrag && pressedHeaderItem._type == LayoutItemType.ColumnHeader)
			{
				allowDrag = pressedHeaderItem._column.AllowDrag;
				if (allowDrag)
				{
					ColumnActionCancelEventArgs e = new ColumnActionCancelEventArgs(pressedHeaderItem._column, cancel: false);
					OnDraggingColumn(e);
					otherCancel = e.Cancel;
				}
			}
			else if (pressedHeaderItem._type == LayoutItemType.ColumnSetHeader)
			{
				allowDrag = pressedHeaderItem._columnSet.AllowDrag;
				if (_allowColumnDrag && allowDrag)
				{
					ColumnSetActionCancelEventArgs columnSetActionCancelEventArgs = new ColumnSetActionCancelEventArgs(pressedHeaderItem._columnSet, cancel: false);
					OnDraggingColumnSet(columnSetActionCancelEventArgs);
					otherCancel = columnSetActionCancelEventArgs.Cancel;
				}
			}
			else if (pressedHeaderItem._type == LayoutItemType.GroupByBoxHeader)
			{
				if (!pressedHeaderItem._gridGroup.AllowRemove)
				{
					otherCancel = true;
				}
				else
				{
					allowDrag = true;
					GroupActionCancelEventArgs groupActionCancelEventArgs = new GroupActionCancelEventArgs(pressedHeaderItem._gridGroup, cancel: false);
					OnDraggingGroup(groupActionCancelEventArgs);
					otherCancel = groupActionCancelEventArgs.Cancel;
				}
			}
			if (!otherCancel && AcceptCellEdit())
			{
				_pressedHeaderItem = pressedHeaderItem;
				ToggleDragSourceHighlight(null);
				if (allowDrag)
				{
					_headerDragState = 1;
					TryStartDragTracking(eventArgs.X, eventArgs.Y);
				}
			}
		}

		private void StartHeaderDragFromSource()
		{
			GridGroupLayoutState pressedHeaderItem = _pressedHeaderItem;
			Rectangle rectangle = AdjustRectangleToViewport(pressedHeaderItem.GetAbsoluteItemRect(), pressedHeaderItem._isHorizontallyScrolled);
			GridFormatStyle gridFormatStyle = GetHeaderStyle(pressedHeaderItem.GetOwnerTable());
			GridFormatStyle applyOverrides = ((pressedHeaderItem._type == LayoutItemType.GroupByBoxHeader) ? ((pressedHeaderItem._gridGroup.Column != null) ? gridFormatStyle.ApplyOverrides(pressedHeaderItem._gridGroup.Column.HeaderStyle) : gridFormatStyle.ApplyOverrides(null)) : ((pressedHeaderItem._type != LayoutItemType.ColumnHeader) ? gridFormatStyle.ApplyOverrides(pressedHeaderItem._columnSet.HeaderStyle) : gridFormatStyle.ApplyOverrides(pressedHeaderItem._column.HeaderStyle)));
			_isHeaderPressed = true;
			Graphics graphics = CreateGraphics();
			_pressFeedbackRenderer = CreateDragRenderer(graphics, pressedHeaderItem, rectangle.Width, rectangle.Height);
			graphics.Dispose();
			graphics = null;
			Update();
			_pressFeedbackRenderer.SetDropAllowed(true);
			graphics = _pressFeedbackRenderer.CreateFeedbackGraphics();
			Point point = _mouseDownPoint;
			_pressFeedbackRenderer._gridRenderer3Point = new Point(point.X - rectangle.X, point.Y - rectangle.Y);
			rectangle.Offset(-rectangle.X, -rectangle.Y);
			pressedHeaderItem._isPressed = false;
			pressedHeaderItem._isHot = false;
			SetPaintClipRectangle(rectangle);
			PrepareControlStyleLayout();
			DrawLayoutItem(graphics, pressedHeaderItem, applyOverrides, rectangle, rectangle);
			pressedHeaderItem._isPressed = true;
			applyOverrides.DisposeResources();
			gridFormatStyle.DisposeResources();
		}

		internal void StartLayoutItemDrag(GridGroupLayoutState gridGroupLayoutState, int value, int otherValue)
		{
			Rectangle rectangle = gridGroupLayoutState.GetAbsoluteItemRect();
			GridFormatStyle gridFormatStyle = GetHeaderStyle(gridGroupLayoutState.GetOwnerTable());
			_isHeaderPressed = true;
			Graphics graphics = CreateGraphics();
			_pressFeedbackRenderer = CreateDragRenderer(graphics, gridGroupLayoutState, rectangle.Width, rectangle.Height);
			graphics.Dispose();
			graphics = null;
			Update();
			_pressFeedbackRenderer.SetDropAllowed(true);
			graphics = _pressFeedbackRenderer.CreateFeedbackGraphics();
			_pressFeedbackRenderer._gridRenderer3Point = new Point(value, otherValue);
			rectangle.Offset(-rectangle.X, -rectangle.Y);
			PrepareControlStyleLayout();
			gridGroupLayoutState._isPressed = false;
			SetPaintClipRectangle(rectangle);
			DrawLayoutItem(graphics, gridGroupLayoutState, gridFormatStyle, rectangle, rectangle);
			gridGroupLayoutState._isPressed = true;
			gridFormatStyle.DisposeResources();
			GridHelper.SetCapture(this, true);
		}

		protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
		{
			base.OnQueryContinueDrag(e);
		}

		private void EndHeaderDrag(bool condition)
		{
			Point point = PointToClient(PointerPosition.Current);
			if (_pressFeedbackRenderer == null || _isUpdatingHeaderPress)
			{
				return;
			}
			_isUpdatingHeaderPress = true;
			try
			{
				if (_pressedHeaderItem != null)
				{
					_pressedHeaderItem._isPressed = false;
					_pressedHeaderItem = null;
				}
				_pressFeedbackRenderer.EraseArrows();
				_pressFeedbackRenderer.EraseHeader();
				ApplyHeaderDrop(condition, point);
			}
			finally
			{
				// Event handlers called by the drop can throw. The drag state is reset anyway, so the next drag works.
				if (_pressFeedbackRenderer != null)
				{
					_pressFeedbackRenderer.Dispose();
					_pressFeedbackRenderer = null;
				}
				_isHeaderPressed = false;
				_isUpdatingHeaderPress = false;
			}
			Invalidate();
			NotifyRefreshListeners();
		}

		// A header dropped on the column list of the field chooser hides its column or column set.
		private bool TryHideHeaderDroppedOnFieldChooser()
		{
			if (_gridFieldChooser == null || !_gridFieldChooser.Visible || !_gridFieldChooser.IsCursorOverColumnList())
			{
				return false;
			}
			GridGroupLayoutState header = _pressFeedbackRenderer._gridGroupLayoutState;
			if (header._type == LayoutItemType.ColumnHeader)
			{
				if (!header._column.IsRemoveAllowed() || header._column.Table.CellLayoutMode != CellLayoutMode.UseColumns)
				{
					return false;
				}
				header._column.Visible = false;
				return true;
			}
			if (header._type == LayoutItemType.ColumnSetHeader)
			{
				header._columnSet.Visible = false;
				return true;
			}
			return false;
		}

		private void ApplyHeaderDrop(bool condition, Point point)
		{
			if (condition || TryHideHeaderDroppedOnFieldChooser())
			{
				return;
			}
			if (_pressFeedbackRenderer.IsDropTargetValid())
			{
				if (_pressFeedbackRenderer._mordexRowLayoutState != null && _pressFeedbackRenderer._dropIndex != -1)
				{
					if (_pressFeedbackRenderer._mordexRowLayoutState._rowType == LayoutRowType.ColumnHeader)
					{
						if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.GroupByBoxHeader)
						{
							GroupsChangingEventArgs e = new GroupsChangingEventArgs(_pressFeedbackRenderer._gridGroupLayoutState._gridGroup, _pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable(), GroupChangeAction.Remove, -1);
							OnGroupsChanging(e);
							if (!e.Cancel)
							{
								e.Table.Groups.Remove(e.Group);
								OnGroupsChanged(new GroupsChangedEventArgs(e.Table));
							}
							if (_pressFeedbackRenderer._gridGroupLayoutState._gridGroup != null && _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Column != null)
							{
								GridColumn column = _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Column;
								if (column.Table.CellLayoutMode == CellLayoutMode.UseColumns && _pressFeedbackRenderer._dropIndex != column._position)
								{
									MovingColumnEventArgs movingColumnEventArgs = new MovingColumnEventArgs(column, _pressFeedbackRenderer._dropIndex);
									OnMovingColumn(movingColumnEventArgs);
									if (!movingColumnEventArgs.Cancel)
									{
										if (movingColumnEventArgs.ProposedPosition < column.Position)
										{
											column.Position = movingColumnEventArgs.ProposedPosition;
										}
										else if (movingColumnEventArgs.ProposedPosition > column._position)
										{
											column.Position = movingColumnEventArgs.ProposedPosition - 1;
										}
										column.Visible = true;
										OnColumnMoved(new ColumnActionEventArgs(column));
									}
								}
							}
						}
						else if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnSetHeader)
						{
							if (_pressFeedbackRenderer._dropIndex <= _pressFeedbackRenderer._gridGroupLayoutState._columnSet.Position)
							{
								_pressFeedbackRenderer._gridGroupLayoutState._columnSet.Position = _pressFeedbackRenderer._dropIndex;
							}
							else
							{
								_pressFeedbackRenderer._gridGroupLayoutState._columnSet.Position = _pressFeedbackRenderer._dropIndex - 1;
							}
							_pressFeedbackRenderer._gridGroupLayoutState._columnSet.Visible = true;
						}
						else if (_pressFeedbackRenderer._gridGroupLayoutState._column.Table.CellLayoutMode == CellLayoutMode.UseColumns && (_pressFeedbackRenderer._dropIndex != _pressFeedbackRenderer._gridGroupLayoutState._column._position || !_pressFeedbackRenderer._gridGroupLayoutState._column.Visible))
						{
							MovingColumnEventArgs otherMovingColumnEventArgs = new MovingColumnEventArgs(_pressFeedbackRenderer._gridGroupLayoutState._column, _pressFeedbackRenderer._dropIndex);
							OnMovingColumn(otherMovingColumnEventArgs);
							if (!otherMovingColumnEventArgs.Cancel)
							{
								if (otherMovingColumnEventArgs.ProposedPosition < _pressFeedbackRenderer._gridGroupLayoutState._column.Position)
								{
									_pressFeedbackRenderer._gridGroupLayoutState._column.Position = otherMovingColumnEventArgs.ProposedPosition;
								}
								else if (otherMovingColumnEventArgs.ProposedPosition > _pressFeedbackRenderer._gridGroupLayoutState._column._position)
								{
									_pressFeedbackRenderer._gridGroupLayoutState._column.Position = otherMovingColumnEventArgs.ProposedPosition - 1;
								}
								_pressFeedbackRenderer._gridGroupLayoutState._column.Visible = true;
								OnColumnMoved(new ColumnActionEventArgs(_pressFeedbackRenderer._gridGroupLayoutState._column));
							}
						}
					}
					else if (_pressFeedbackRenderer._mordexRowLayoutState._rowType == LayoutRowType.GroupByBox)
					{
						if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader && _pressFeedbackRenderer._gridGroupLayoutState._column.CustomGroup != null)
						{
							_pressFeedbackRenderer._gridGroupLayoutState._type = LayoutItemType.GroupByBoxHeader;
							_pressFeedbackRenderer._gridGroupLayoutState._gridGroup = new GridGroup(_pressFeedbackRenderer._gridGroupLayoutState._column.CustomGroup);
							_pressFeedbackRenderer._gridGroupLayoutState._column = null;
						}
						if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader)
						{
							GridGroup gridGroup = new GridGroup(_pressFeedbackRenderer._gridGroupLayoutState._column);
							if (_pressFeedbackRenderer._gridGroupLayoutState._column.SortOrder != SortOrder.Unsorted)
							{
								gridGroup.SortOrder = _pressFeedbackRenderer._gridGroupLayoutState._column.SortOrder;
							}
							GridTable table = _pressFeedbackRenderer._mordexRowLayoutState.LayoutTable;
							if (!table.CanGroupBy(_pressFeedbackRenderer._gridGroupLayoutState._column))
							{
								table = _pressFeedbackRenderer._gridGroupLayoutState._column.Table;
							}
							GroupsChangingEventArgs groupsChangingEventArgs = new GroupsChangingEventArgs(gridGroup, table, GroupChangeAction.Add, _pressFeedbackRenderer._dropIndex);
							OnGroupsChanging(groupsChangingEventArgs);
							if (!groupsChangingEventArgs.Cancel)
							{
								table.Groups.Insert(groupsChangingEventArgs.ProposedPosition, gridGroup);
								OnGroupsChanged(new GroupsChangedEventArgs(table));
							}
						}
						else if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.GroupByBoxHeader)
						{
							if (_pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Table == null && _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.CustomGroup != null)
							{
								GroupsChangingEventArgs otherGroupsChangingEventArgs = new GroupsChangingEventArgs(_pressFeedbackRenderer._gridGroupLayoutState._gridGroup, _pressFeedbackRenderer._mordexRowLayoutState._owner, GroupChangeAction.Add, _pressFeedbackRenderer._dropIndex);
								OnGroupsChanging(otherGroupsChangingEventArgs);
								if (!otherGroupsChangingEventArgs.Cancel)
								{
									_pressFeedbackRenderer._mordexRowLayoutState._owner.Groups.Insert(otherGroupsChangingEventArgs.ProposedPosition, _pressFeedbackRenderer._gridGroupLayoutState._gridGroup);
									OnGroupsChanged(new GroupsChangedEventArgs(otherGroupsChangingEventArgs.Table));
								}
							}
							else if (_pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Table == _pressFeedbackRenderer._mordexRowLayoutState._owner)
							{
								GroupsChangingEventArgs nextGroupsChangingEventArgs = new GroupsChangingEventArgs(_pressFeedbackRenderer._gridGroupLayoutState._gridGroup, _pressFeedbackRenderer._mordexRowLayoutState._owner, GroupChangeAction.Move, _pressFeedbackRenderer._dropIndex);
								OnGroupsChanging(nextGroupsChangingEventArgs);
								if (!nextGroupsChangingEventArgs.Cancel)
								{
									_pressFeedbackRenderer._mordexRowLayoutState._owner.Groups.MoveGroup(nextGroupsChangingEventArgs.ProposedPosition, _pressFeedbackRenderer._gridGroupLayoutState._gridGroup);
									OnGroupsChanged(new GroupsChangedEventArgs(nextGroupsChangingEventArgs.Table));
								}
							}
							else
							{
								GridGroup otherGroup = new GridGroup(_pressFeedbackRenderer._gridGroupLayoutState._column, _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.SortOrder);
								GroupsChangingEventArgs previousGroupsChangingEventArgs = new GroupsChangingEventArgs(_pressFeedbackRenderer._gridGroupLayoutState._gridGroup, _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.GetParentCollection().GetOwnerTable(), GroupChangeAction.Remove, -1);
								OnGroupsChanging(previousGroupsChangingEventArgs);
								if (!previousGroupsChangingEventArgs.Cancel)
								{
									_pressFeedbackRenderer._gridGroupLayoutState._gridGroup.GetParentCollection().Remove(_pressFeedbackRenderer._gridGroupLayoutState._gridGroup);
									OnGroupsChanged(new GroupsChangedEventArgs(previousGroupsChangingEventArgs.Table));
								}
								previousGroupsChangingEventArgs = new GroupsChangingEventArgs(otherGroup, _pressFeedbackRenderer._mordexRowLayoutState._owner, GroupChangeAction.Add, _pressFeedbackRenderer._dropIndex);
								OnGroupsChanging(previousGroupsChangingEventArgs);
								if (!previousGroupsChangingEventArgs.Cancel)
								{
									_pressFeedbackRenderer._mordexRowLayoutState._owner.Groups.Insert(previousGroupsChangingEventArgs.ProposedPosition, otherGroup);
									OnGroupsChanged(new GroupsChangedEventArgs(previousGroupsChangingEventArgs.Table));
								}
							}
						}
					}
				}
				else if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.GroupByBoxHeader)
				{
					GroupsChangingEventArgs firstGroupsChangingEventArgs = new GroupsChangingEventArgs(_pressFeedbackRenderer._gridGroupLayoutState._gridGroup, _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Table, GroupChangeAction.Remove, -1);
					OnGroupsChanging(firstGroupsChangingEventArgs);
					if (!firstGroupsChangingEventArgs.Cancel)
					{
						if (_pressFeedbackRenderer._gridGroupLayoutState._column != null)
						{
							_pressFeedbackRenderer._gridGroupLayoutState._column.Visible = true;
						}
						if (_pressFeedbackRenderer._gridGroupLayoutState._gridGroup != null && _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.GetParentCollection() != null)
						{
							_pressFeedbackRenderer._gridGroupLayoutState._gridGroup.GetParentCollection().Remove(_pressFeedbackRenderer._gridGroupLayoutState._gridGroup);
							OnGroupsChanged(new GroupsChangedEventArgs(firstGroupsChangingEventArgs.Table));
						}
					}
				}
				else if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader && _pressFeedbackRenderer._gridGroupLayoutState._column.IsRemoveAllowed() && _pressFeedbackRenderer._gridGroupLayoutState._column.Table.CellLayoutMode == CellLayoutMode.UseColumns && HitTest(point.X, point.Y) != GridArea.ColumnHeader)
				{
					_pressFeedbackRenderer._gridGroupLayoutState._column.Visible = false;
				}
			}
			else if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.GroupByBoxHeader && _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Table != null)
			{
				GroupsChangingEventArgs secondGroupsChangingEventArgs = new GroupsChangingEventArgs(_pressFeedbackRenderer._gridGroupLayoutState._gridGroup, _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Table, GroupChangeAction.Remove, -1);
				OnGroupsChanging(secondGroupsChangingEventArgs);
				if (!secondGroupsChangingEventArgs.Cancel)
				{
					if (secondGroupsChangingEventArgs.Group.Column != null && secondGroupsChangingEventArgs.Group.Column.IsRemoveAllowed())
					{
						secondGroupsChangingEventArgs.Group.Column.Visible = true;
					}
					secondGroupsChangingEventArgs.Table.Groups.Remove(secondGroupsChangingEventArgs.Group);
					OnGroupsChanged(new GroupsChangedEventArgs(secondGroupsChangingEventArgs.Table));
				}
			}
			else if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader && _pressFeedbackRenderer._gridGroupLayoutState._column.IsRemoveAllowed() && HitTest(point.X, point.Y) != GridArea.ColumnHeader)
			{
				_pressFeedbackRenderer._gridGroupLayoutState._column.Visible = false;
			}
		}
	}
}
