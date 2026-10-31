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
using System.Security;
using System.Text;
using System.Threading;
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
		private bool TryScrollRowsToFirstRow(int rowPosition, int otherRowPosition)
		{
			if (!CanScrollByBlit())
			{
				return false;
			}
			int lastVisibleRow = LastVisibleRow(includePartialRows: false);
			int otherValue = lastVisibleRow - rowPosition;
			int value = otherRowPosition - rowPosition;
			if (Math.Abs(value) >= otherValue || _bodyRowLayouts == null || otherRowPosition < 0 || otherRowPosition >= RowCount)
			{
				return false;
			}
			_ = DateTime.Now.Ticks;
			Rectangle clientRectangle = base.ClientRectangle;
			if (otherRowPosition > rowPosition)
			{
				if (!IsRowInVisibleTableRange(otherRowPosition))
				{
					return false;
				}
				_ = DateTime.Now.Ticks;
				FlushPendingPaint();
				if (_bufferedGraphics == null)
				{
					return false;
				}
				GridRow row = GetRow(otherRowPosition);
				MxRowLayoutState mordexRowLayoutState = null;
				switch (row.RowType)
				{
				case RowType.GroupFooter:
					mordexRowLayoutState = _bodyRowLayouts.FindRow(LayoutRowType.GroupTotalRow, row.GetLogicalRow(), false);
					break;
				case RowType.GroupHeader:
					mordexRowLayoutState = _bodyRowLayouts.FindRow(LayoutRowType.GroupHeaderRow, row.GetLogicalRow(), false);
					break;
				case RowType.NewRecord:
					mordexRowLayoutState = _bodyRowLayouts.FindRow(LayoutRowType.NewRow, row.GetLogicalRow(), false);
					break;
				case RowType.Record:
					mordexRowLayoutState = _bodyRowLayouts.FindRow(LayoutRowType.DataRow, row.GetLogicalRow(), false);
					break;
				case RowType.TotalRow:
					mordexRowLayoutState = _bodyRowLayouts.FindRow(LayoutRowType.TotalRow, row.GetLogicalRow(), false);
					break;
				}
				if (mordexRowLayoutState == null)
				{
					return false;
				}
				MxRowLayoutState rowAt = null;
				int nextValue = -1;
				if (mordexRowLayoutState.LayoutTable != RootTable)
				{
					int indexOfColumnHeader = _bodyRowLayouts.IndexOfColumnHeader(mordexRowLayoutState.LayoutTable);
					if (indexOfColumnHeader != -1)
					{
						rowAt = _bodyRowLayouts.GetRowAt(indexOfColumnHeader);
						_ = rowAt.GetLocation().Y;
						nextValue = rowAt.GetLocation().Y + rowAt._rowHeight;
					}
				}
				int y = mordexRowLayoutState.GetLocation().Y;
				if (nextValue != -1 && nextValue == y)
				{
					rowAt = null;
				}
				int previousValue = -1;
				_previousBodyRowLayouts = _bodyRowLayouts.CopyList();
				SetFirstVisibleRow(otherRowPosition);
				_isLayoutValid = false;
				_ = DateTime.Now.Ticks;
				ValidateClientArea();
				CalculateLayoutStates(_bufferedGraphics.Graphics);
				if (_previousBodyRowLayouts != null)
				{
					_previousBodyRowLayouts.ClearAll();
					_previousBodyRowLayouts = null;
				}
				previousValue = mordexRowLayoutState.GetLocation().Y;
				int firstValue = y - previousValue;
				int bodyTopY = _bodyTopY;
				if (firstValue <= 0)
				{
					return false;
				}
				ValidateClientArea();
				Rectangle otherClientRectangle = base.ClientRectangle;
				otherClientRectangle.Y = bodyTopY;
				otherClientRectangle.Height = _bodyBottomY - bodyTopY;
				ScrollClientAreaBlit(0, -firstValue, otherClientRectangle, true);
				if (previousValue != _bodyTopY)
				{
					Rectangle nextClientRectangle = base.ClientRectangle;
					nextClientRectangle.Y = _bodyTopY;
					nextClientRectangle.Height = previousValue - _bodyTopY;
					Invalidate(nextClientRectangle);
					FlushPendingPaint();
				}
				if (_footerRowLayouts != null && _footerRowLayouts.GetCount() > 0)
				{
					Rectangle previousClientRectangle = base.ClientRectangle;
					previousClientRectangle.Y = _bodyBottomY;
					previousClientRectangle.Height = base.Height - _bodyBottomY;
					Invalidate(previousClientRectangle);
					FlushPendingPaint();
				}
				InvalidateDeferred();
			}
			else if (otherRowPosition < rowPosition)
			{
				if (!IsRowInVisibleTableRange(rowPosition))
				{
					return false;
				}
				FlushPendingPaint();
				GridRow otherRow = GetRow(rowPosition);
				MxRowLayoutState nextRow = null;
				switch (otherRow.RowType)
				{
				case RowType.GroupFooter:
					nextRow = _bodyRowLayouts.FindRow(LayoutRowType.GroupTotalRow, otherRow.GetLogicalRow(), false);
					break;
				case RowType.GroupHeader:
					nextRow = _bodyRowLayouts.FindRow(LayoutRowType.GroupHeaderRow, otherRow.GetLogicalRow(), false);
					break;
				case RowType.NewRecord:
					nextRow = _bodyRowLayouts.FindRow(LayoutRowType.NewRow, otherRow.GetLogicalRow(), false);
					break;
				case RowType.Record:
					nextRow = _bodyRowLayouts.FindRow(LayoutRowType.DataRow, otherRow.GetLogicalRow(), false);
					break;
				case RowType.TotalRow:
					nextRow = _bodyRowLayouts.FindRow(LayoutRowType.TotalRow, otherRow.GetLogicalRow(), false);
					break;
				}
				if (nextRow == null)
				{
					return false;
				}
				int y2 = nextRow.GetLocation().Y;
				int secondValue = -1;
				_previousBodyRowLayouts = _bodyRowLayouts.CopyList();
				SetFirstVisibleRow(otherRowPosition);
				_isLayoutValid = false;
				CalculateLayoutIfReady();
				if (_previousBodyRowLayouts != null)
				{
					_previousBodyRowLayouts.ClearAll();
					_previousBodyRowLayouts = null;
				}
				secondValue = nextRow.GetLocation().Y;
				int thirdValue = secondValue - y2;
				int otherBodyTopY = _bodyTopY;
				if (y2 != otherBodyTopY)
				{
					otherBodyTopY = y2;
				}
				if (thirdValue <= 0)
				{
					return false;
				}
				ValidateClientArea();
				Rectangle firstClientRectangle = base.ClientRectangle;
				firstClientRectangle.Y = otherBodyTopY;
				firstClientRectangle.Height = _bodyBottomY - otherBodyTopY;
				ScrollClientAreaBlit(0, thirdValue, firstClientRectangle, true);
				if (y2 != _bodyTopY)
				{
					Rectangle secondClientRectangle = base.ClientRectangle;
					secondClientRectangle.Y = _bodyTopY;
					secondClientRectangle.Height = y2 - _bodyTopY;
					Invalidate(secondClientRectangle);
					FlushPendingPaint();
				}
				if (_footerRowLayouts != null && _footerRowLayouts.GetCount() > 0)
				{
					Rectangle thirdClientRectangle = base.ClientRectangle;
					thirdClientRectangle.Y = _bodyBottomY;
					thirdClientRectangle.Height = base.Height - _bodyBottomY;
					Invalidate(thirdClientRectangle);
					FlushPendingPaint();
				}
				InvalidateDeferred();
			}
			if (clientRectangle != base.ClientRectangle)
			{
				Invalidate();
			}
			return true;
		}

		private Rectangle ScrollClientAreaBlit(int width, int height, Rectangle rc, bool condition)
		{
			if (height == 0 && width == 0)
			{
				return Rectangle.Empty;
			}
			try
			{
				NativeMethods.NativeRectStruct e = new NativeMethods.NativeRectStruct
				{
					_left = rc.Left,
					_nativeMethodsData4Integer = rc.Top,
					_right = rc.Right,
					_bottom = rc.Bottom
				};
				NativeMethods.NativeRectStruct nativeMethodsData4Argument = e;
				Graphics graphics = _bufferedGraphics.Graphics;
				IntPtr hdc = graphics.GetHdc();
				NativeMethods.ScrollDC(hdc, width, height, ref e, ref nativeMethodsData4Argument, IntPtr.Zero, ref e);
				graphics.ReleaseHdc(hdc);
				if (height % 2 != 0)
				{
					ToggleRenderingOriginY();
				}
				if (width % 2 != 0)
				{
					ToggleRenderingOriginX();
				}
				if (height != 0)
				{
					if (height < 0)
					{
						rc.Y = rc.Bottom + height;
						rc.Height = -height;
					}
					else
					{
						rc.Height = height;
					}
				}
				else if (width != 0)
				{
					if (width < 0)
					{
						width -= 2;
						rc.X = rc.Right + width;
						rc.Width = -width;
					}
					else
					{
						width += 2;
						rc.Width = width;
					}
				}
				if (condition)
				{
					Invalidate(rc);
					FlushPendingPaint();
				}
			}
			finally
			{
			}
			return rc;
		}

		private void ReleasePressedLayoutItem(MouseEventArgs eventArgs)
		{
			Point point = PointToClient(PointerPosition.Current);
			if (AdjustRectangleToViewport(GetPressedLayoutItem().GetAbsoluteItemRect(), GetPressedLayoutItem()._isHorizontallyScrolled).Contains(point.X, point.Y) && GetPressedLayoutItem().IsButtonItem() && !GetPressedLayoutItem().IsDisabled())
			{
				GetPressedLayoutItem()._isPressed = false;
				_gridGridGroupLayoutState._isHot = false;
				if ((_gridGridGroupLayoutState._type == LayoutItemType.CellDownButton || _gridGridGroupLayoutState._type == LayoutItemType.CellUpButton) && _gridColumnContainerControl != null)
				{
					_gridColumnContainerControl.NotifyButtonMouseUp();
				}
				GridGroupLayoutState gridGroupLayoutState = GetPressedLayoutItem();
				SetPressedLayoutItem(null);
				GridHelper.SetCapture(this, false);
				HandleLayoutButtonClick(gridGroupLayoutState);
			}
			SetPressedLayoutItem(null);
		}

		private void OnAutoScrollTimerTick(object sender, EventArgs eventArgs)
		{
			System.Windows.Forms.Timer timer = (System.Windows.Forms.Timer)sender;
			timer.Stop();
			Point position = PointerPosition.Current;
			position = PointToClient(position);
			if (_isCellDragSelecting)
			{
				AutoScrollSelectCells(position.X, position.Y);
			}
			else
			{
				AutoScrollSelectRows(position.X, position.Y);
			}
			if (_gridTimer != null)
			{
				timer.Interval = Timing.ScrollStepInterval;
				timer.Start();
			}
		}

		private void AutoScrollSelectCells(int value, int otherValue)
		{
			int otherRow = -3;
			bool condition = false;
			if (Row == -1 || Row == -2)
			{
				otherRow = Row;
			}
			else if (otherValue < _bodyTopY)
			{
				FirstRow = _firstRow - 1;
				otherRow = _firstRow;
				condition = true;
			}
			else if (otherValue > base.ClientRectangle.Bottom)
			{
				if (!IsRowInVisibleTableRange(RowCount - 1))
				{
					FirstRow = _firstRow + 1;
				}
				otherRow = RowPositionFromPoint(base.ClientRectangle.X + 1, base.ClientRectangle.Bottom - 1);
				if (otherRow == -1)
				{
					otherRow = RowCount - 1;
				}
				condition = true;
			}
			else
			{
				otherRow = RowPositionFromPoint(base.ClientRectangle.X + 1, otherValue);
			}
			int horizontalScrollPosition = HorizontalScrollPosition;
			if (value < base.ClientRectangle.X)
			{
				HorizontalScrollPosition -= TableViewHorizontalScrollIncrement;
				value = base.ClientRectangle.X + 1;
				if (horizontalScrollPosition != HorizontalScrollPosition)
				{
					condition = true;
				}
			}
			else if (value >= base.ClientRectangle.Right)
			{
				HorizontalScrollPosition += TableViewHorizontalScrollIncrement;
				value = base.ClientRectangle.Right - 1;
				if (horizontalScrollPosition != HorizontalScrollPosition)
				{
					condition = true;
				}
			}
			GridColumn gridColumn = HitTester.FindColumnAtX(CurrentTable, value);
			int position = _selectionRectEnd.X;
			if (gridColumn != null)
			{
				position = gridColumn.Position;
			}
			if (otherRow == -3)
			{
				otherRow = _selectionRectEnd.Y;
			}
			if (!IsValidRowPosition(otherRow) || position < 0)
			{
				return;
			}
			if (otherRow != _selectionRectEnd.Y || position != _selectionRectEnd.X)
			{
				using (BeginSelectionChangeBatch())
				{
					if (_selectionRectStart == new Point(-3, -3))
					{
						_selectionRectStart = new Point(position, otherRow);
					}
					Rectangle rectangle = RectangleFromCorners(_selectionRectStart, new Point(position, otherRow));
					if (_selectionRectEnd != new Point(-3, -3))
					{
						Rectangle otherRectangle = RectangleFromCorners(_selectionRectStart, _selectionRectEnd);
						DeselectCellsOutsideRange(otherRectangle, rectangle);
					}
					SelectCellsInRange(rectangle);
					_selectionRectEnd = new Point(position, otherRow);
					GridRow row = GetRow(otherRow);
					if (row.RowType != RowType.GroupHeader && row.Table == CurrentTable)
					{
						GridColumn columnInPosition = CurrentTable.Columns.GetColumnInPosition(position);
						if (columnInPosition != null)
						{
							CanFocusCell(otherRow, columnInPosition);
						}
					}
				}
			}
			if (condition)
			{
				StartAutoScrollTimer();
			}
			else
			{
				StopAutoScrollTimer();
			}
		}

		private void AutoScrollSelectRows(int value, int otherValue)
		{
			if (_isCellDragSelecting && !_isDragSelectionSuppressed)
			{
				return;
			}
			int firstRow = -1;
			bool condition = false;
			if (otherValue < _bodyTopY)
			{
				FirstRow = _firstRow - 1;
				firstRow = _firstRow;
				condition = true;
			}
			else if (otherValue > base.ClientRectangle.Bottom)
			{
				if (!IsRowInVisibleTableRange(RowCount - 1))
				{
					FirstRow = _firstRow + 1;
				}
				firstRow = RowPositionFromPoint(value, base.ClientRectangle.Bottom - 1);
				if (firstRow == -1)
				{
					firstRow = RowCount - 1;
				}
				condition = true;
			}
			else
			{
				firstRow = RowPositionFromPoint(value, otherValue);
			}
			if (firstRow < 0)
			{
				return;
			}
			if (_dragSelectStartRow == -1)
			{
				_dragSelectStartRow = firstRow;
				_dragSelectCurrentRow = firstRow;
				return;
			}
			if (SelectionMode != SelectionMode.SingleSelection && _dragSelectCurrentRow != firstRow)
			{
				_dragSelectCurrentRow = firstRow;
				SelectRowRange(_dragSelectStartRow, _dragSelectCurrentRow, SelectionMode == SelectionMode.MultipleSelectionSameTable || SelectionMode == SelectionMode.MultipleCellSelection);
				if (GetRow(firstRow).Selected)
				{
					IsRowFocusableInCurrentColumn(firstRow);
				}
				Update();
				if (condition)
				{
					StartAutoScrollTimer();
				}
			}
			if (!condition)
			{
				StopAutoScrollTimer();
			}
		}

		private bool TryBeginCardResize(MouseEventArgs eventArgs)
		{
			_gridGridValueState._resizeCardIndex = HitTester.FindCardResizeIndexAtPoint(eventArgs.X, eventArgs.Y);
			if (_gridGridValueState._resizeCardIndex != -1)
			{
				BeginCardResize();
				return true;
			}
			return false;
		}

		private bool TryBeginColumnResize(MouseEventArgs eventArgs)
		{
			GridGroupLayoutState gridGroupLayoutState = HitTester.FindResizableHeaderAtPoint(eventArgs.X, eventArgs.Y);
			if (gridGroupLayoutState != null && eventArgs.Button == MouseButtons.Left)
			{
				Rectangle rectangle = AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
				_activeButtonItem = gridGroupLayoutState;
				_gridGridValueState._currentLinePosition = -1;
				_isButtonPressActive = true;
				if (gridGroupLayoutState._type == LayoutItemType.ColumnHeader)
				{
					if (RightToLeft == RightToLeft.No)
					{
						_gridGridValueState._minPosition = rectangle.Right - gridGroupLayoutState._column.Width + 8;
					}
					else
					{
						_gridGridValueState._minPosition = rectangle.Left + gridGroupLayoutState._column.Width - 8;
					}
				}
				else if (RightToLeft == RightToLeft.No)
				{
					_gridGridValueState._minPosition = rectangle.Right - gridGroupLayoutState._columnSet.Width + 8;
				}
				else
				{
					_gridGridValueState._minPosition = rectangle.Left + gridGroupLayoutState._columnSet.Width - 8;
				}
				return true;
			}
			gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(eventArgs.X, eventArgs.Y);
			if (gridGroupLayoutState.IsHeaderItem())
			{
				BeginHeaderDragCheck(eventArgs, gridGroupLayoutState);
				return true;
			}
			return false;
		}

		internal int MeasureRootTableHeaderHeight(Graphics graphics)
		{
			if (RootTable == null)
			{
				return 0;
			}
			GridFormatStyle gridFormatStyle = GetHeaderStyle(RootTable);
			int columnHeaderHeight = RootTable.CalculateColumnHeaderHeight(graphics, gridFormatStyle, false);
			gridFormatStyle.DisposeResources();
			if (RootTable.IsTableHeaderVisible())
			{
				columnHeaderHeight += RootTable.CalculateTableHeaderHeight(graphics);
			}
			return columnHeaderHeight;
		}

		internal int MeasureAutoRowHeight(Graphics graphics)
		{
			if (RootTable == null)
			{
				return (int)Font.GetHeight() + 4;
			}
			if (RootTable.RowHeight == -1)
			{
				GridFormatStyle gridFormatStyle = GetRowStyle(RootTable);
				GridRow gridRow = ((RecordCount != 0) ? GetRow(0) : GetGridRowForLogicalRow(_gridDataSourceManager.GetDataMordexRow()));
				int measureAutoRowHeight = 0;
				if (gridRow == null)
				{
					measureAutoRowHeight = (int)Font.GetHeight() + 4;
				}
				else
				{
					MxRowLayoutState mordexRowLayoutState = RootTable.BuildDataRowLayout(graphics, gridRow, null, gridFormatStyle, base.ClientRectangle.Width, GetEffectiveFrozenColumns());
					measureAutoRowHeight = mordexRowLayoutState._rowHeight;
					mordexRowLayoutState.ReleaseItems();
				}
				gridFormatStyle.DisposeResources();
				return measureAutoRowHeight;
			}
			return RootTable.RowHeight;
		}

		private void BeginCardResize()
		{
			HideCellEditorControl();
			_isCellMousePressed = true;
			if (RightToLeft == RightToLeft.No)
			{
				_gridGridValueState._minPosition = (24 + CardSpacing) * (_gridGridValueState._resizeCardIndex + 1) + CardSpacing / 2;
				_gridGridValueState._maxPosition = base.ClientRectangle.Right;
			}
			else
			{
				_gridGridValueState._minPosition = base.ClientRectangle.Left;
				Rectangle rectangle = new Rectangle((24 + CardSpacing) * (_gridGridValueState._resizeCardIndex + 1) + CardSpacing / 2, 0, 0, 0);
				_gridGridValueState._maxPosition = AdjustRectangleToViewport(rectangle, false).Right;
			}
			_gridGridValueState._currentLinePosition = -1;
		}

		private void EndColumnResize(bool condition)
		{
			_isButtonPressActive = false;
			Point position = PointerPosition.Current;
			int gridValueStateInteger = PointToClient(position).X;
			if (RightToLeft == RightToLeft.No)
			{
				if (gridValueStateInteger < _gridGridValueState._minPosition)
				{
					gridValueStateInteger = _gridGridValueState._minPosition;
				}
			}
			else if (gridValueStateInteger > _gridGridValueState._minPosition)
			{
				gridValueStateInteger = _gridGridValueState._minPosition;
			}
			Rectangle rectangle = AdjustRectangleToViewport(_activeButtonItem.GetAbsoluteItemRect(), _activeButtonItem._isHorizontallyScrolled);
			if (_gridGridValueState._currentLinePosition != -1)
			{
				Point point = PointToScreen(new Point(_gridGridValueState._currentLinePosition, rectangle.Top));
				Point pointToScreen = PointToScreen(new Point(_gridGridValueState._currentLinePosition, base.ClientRectangle.Bottom));
				GridHelper.DrawSplitLine(this, point, pointToScreen, Color.Black);
			}
			if (condition)
			{
				return;
			}
			if (_activeButtonItem._type == LayoutItemType.ColumnHeader)
			{
				SizingColumnEventArgs e = new SizingColumnEventArgs(proposedWidth: (RightToLeft != RightToLeft.No) ? (_activeButtonItem._column._width + (rectangle.Left - gridValueStateInteger + 1)) : (_activeButtonItem._column.Width + (gridValueStateInteger - rectangle.Right + 1)), column: _activeButtonItem._column, cancel: false);
				OnSizingColumn(e);
				if (!e.Cancel)
				{
					if (_columnAutoResize)
					{
						if (_activeButtonItem._column.Table.CellLayoutMode == CellLayoutMode.UseColumns)
						{
							GridTable table = _activeButtonItem._column.Table;
							table.AdjustColumnWidths(_activeButtonItem._column, e.ProposedWidth);
							table._isAutoResizePending = true;
							RefreshRowCountAndInvalidate();
						}
						else
						{
							_activeButtonItem._column.Width = e.ProposedWidth;
						}
					}
					else
					{
						_activeButtonItem._column.Width = e.ProposedWidth;
					}
				}
			}
			else if (_activeButtonItem._type == LayoutItemType.ColumnSetHeader)
			{
				SizingColumnSetEventArgs sizingColumnSetEventArgs = new SizingColumnSetEventArgs(proposedWidth: (RightToLeft != RightToLeft.No) ? (_activeButtonItem._columnSet.Width + (rectangle.Left - gridValueStateInteger + 1)) : (_activeButtonItem._columnSet.Width + (gridValueStateInteger - rectangle.Right + 1)), columnSet: _activeButtonItem._columnSet, cancel: false);
				OnSizingColumnSet(sizingColumnSetEventArgs);
				if (!sizingColumnSetEventArgs.Cancel)
				{
					_activeButtonItem._columnSet.Width = sizingColumnSetEventArgs.ProposedWidth;
				}
			}
			PositionCellEditorControl(true);
		}

		private void EndCardResize(bool condition)
		{
			Point position = PointerPosition.Current;
			int gridValueStateInteger = PointToClient(position).X;
			if (gridValueStateInteger < _gridGridValueState._minPosition)
			{
				gridValueStateInteger = _gridGridValueState._minPosition;
			}
			if (gridValueStateInteger > _gridGridValueState._maxPosition)
			{
				gridValueStateInteger = _gridGridValueState._maxPosition;
			}
			if (_gridGridValueState._currentLinePosition != -1)
			{
				Rectangle[] array = _cardBounds;
				foreach (Rectangle rectangle in array)
				{
					Rectangle adjustRectangleToViewport = AdjustRectangleToViewport(rectangle, false);
					Point point = PointToScreen(adjustRectangleToViewport.Location);
					Point pointToScreen = PointToScreen(new Point(adjustRectangleToViewport.Left, adjustRectangleToViewport.Bottom));
					GridHelper.DrawSplitLine(this, point, pointToScreen, Color.Black);
				}
			}
			if (!condition)
			{
				int proposedWidth;
				if (RightToLeft == RightToLeft.No)
				{
					proposedWidth = (gridValueStateInteger - _cardSpacing / 2) / (_gridGridValueState._resizeCardIndex + 1) - CardSpacing;
				}
				else
				{
					gridValueStateInteger = base.ClientRectangle.Right - gridValueStateInteger;
					proposedWidth = (gridValueStateInteger - _cardSpacing / 2) / (_gridGridValueState._resizeCardIndex + 1) - CardSpacing;
				}
				SizingCardsEventArgs e = new SizingCardsEventArgs(proposedWidth, cancel: false);
				OnSizingCards(e);
				if (!e.Cancel)
				{
					CardWidth = e.ProposedWidth;
				}
			}
			else
			{
				UpdateRectangleArray(_cardWidth);
			}
			_gridGridValueState._resizeCardIndex = -1;
			_isCellMousePressed = false;
			Cursor = Cursors.Default;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			ResetOnVisibleChange();
		}

		protected override void OnParentVisibleChanged(EventArgs e)
		{
			base.OnParentVisibleChanged(e);
			ResetOnVisibleChange();
		}

		private void ResetOnVisibleChange()
		{
			if (_gridFieldChooser != null)
			{
				_gridFieldChooser.Visible = base.Visible;
			}
			ShowTooltipForLayoutItem(null);
			Invalidate();
			CurrentCellDroppedDown = false;
		}

		public int RowPositionFromPoint()
		{
			Point point = PointToClient(PointerPosition.Current);
			return RowPositionFromPoint(point.X, point.Y);
		}

		public int RowPositionFromPoint(int x, int y)
		{
			if (_view == View.TableView)
			{
				MxRowLayoutState mordexRowLayoutState = HitTester.FindRowLayoutAtPoint(new Point(x, y));
				if (mordexRowLayoutState != null)
				{
					switch (mordexRowLayoutState._rowType)
					{
					case LayoutRowType.DataRow:
					case LayoutRowType.NewRow:
					case LayoutRowType.GroupHeaderRow:
					case LayoutRowType.GroupTotalRow:
					case LayoutRowType.TotalRow:
						return mordexRowLayoutState._rowPosition;
					case LayoutRowType.ColumnHeader:
					{
						if (mordexRowLayoutState._owner == RootTable || mordexRowLayoutState._mordexRow == null)
						{
							return -3;
						}
						MordexRow parentRow = mordexRowLayoutState._mordexRow.ParentRow;
						if (parentRow is MordexTableRow)
						{
							parentRow = parentRow.ParentRow;
						}
						return parentRow?.RecordPosition ?? (-3);
					}
					default:
						return -3;
					}
				}
			}
			else
			{
				GridTableLayoutState gridTableLayoutState = HitTester.FindTableLayoutAtPoint(new Point(x, y));
				if (gridTableLayoutState != null)
				{
					return gridTableLayoutState.GetRowPosition();
				}
			}
			return -3;
		}

		private void ShowTooltipForLayoutItem(GridGroupLayoutState toolTipLayoutItem)
		{
			if (_toolTipLayoutItem == toolTipLayoutItem)
			{
				return;
			}
			_toolTipLayoutItem = toolTipLayoutItem;
			if (_toolTipLayoutItem == null || (_toolTipLayoutItem == FindCellLayoutItem(_row, _currentColumn) && EditMode == EditMode.EditOn))
			{
				ClearTooltipText();
				return;
			}
			EnsureTooltipWindow();
			string text = "";
			Point empty = Point.Empty;
			_gridWindowMessageHandler.SetMultiline(false);
			_gridWindowMessageHandler.SetTooltipSize(new Size(0, 0));
			_gridWindowMessageHandler._owner = null;
			switch (toolTipLayoutItem._type)
			{
			case LayoutItemType.CellColumnButton:
				if (toolTipLayoutItem._column != null)
				{
					text = toolTipLayoutItem._column.ButtonToolTipText;
				}
				break;
			case LayoutItemType.Cell:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.CardCell:
				if (toolTipLayoutItem._gridGroupLayoutStateOwner.GridRow.Cells[toolTipLayoutItem._column].GetErrorText().Length > 0)
				{
					text = toolTipLayoutItem._gridGroupLayoutStateOwner.GridRow.Cells[toolTipLayoutItem._column].GetErrorText();
				}
				else
				{
					if (toolTipLayoutItem._column == null)
					{
						break;
					}
					switch (toolTipLayoutItem._column.ComputeCellToolTip())
					{
					case CellToolTip.FetchToolTipText:
						text = "<FETCH>";
						_gridWindowMessageHandler._owner = this;
						break;
					case CellToolTip.UseCellToolTipText:
						text = toolTipLayoutItem._gridGroupLayoutStateOwner.GridRow.Cells[toolTipLayoutItem._column].GetEffectiveToolTipText();
						break;
					case CellToolTip.UseHeaderToolTip:
						text = toolTipLayoutItem._column.HeaderToolTip;
						break;
					case CellToolTip.TruncatedText:
					{
						Size size = toolTipLayoutItem.GetAbsoluteItemRect().Size;
						if (toolTipLayoutItem.GetCachedSize().Width > size.Width || toolTipLayoutItem.GetCachedSize().Height > size.Height)
						{
							empty = AdjustRectangleToViewport(toolTipLayoutItem.GetAbsoluteItemRect(), toolTipLayoutItem._isHorizontallyScrolled).Location;
							empty = PointToScreen(empty);
							text = toolTipLayoutItem.GetDisplayText();
							if (toolTipLayoutItem._column.WordWrap)
							{
								_gridWindowMessageHandler.SetMultiline(true);
								_gridWindowMessageHandler.SetTooltipSize(new Size(size.Width, 0));
							}
						}
						break;
					}
					}
				}
				break;
			case LayoutItemType.ColumnHeader:
			case LayoutItemType.CardHeader:
				text = toolTipLayoutItem._column.HeaderToolTip;
				break;
			case LayoutItemType.ColumnSetHeader:
				text = toolTipLayoutItem._columnSet.HeaderToolTip;
				break;
			case LayoutItemType.RowHeader:
			case LayoutItemType.CardCaption:
				if (toolTipLayoutItem._gridGroupLayoutStateOwner.GridRow != null)
				{
					text = toolTipLayoutItem._gridGroupLayoutStateOwner.GridRow.GetTextOrEmpty();
				}
				break;
			}
			_gridWindowMessageHandler._fixedPosition = empty;
			_gridWindowMessageHandler.SetTooltipText("");
			Rectangle rectangle = AdjustRectangleToViewport(_toolTipLayoutItem.GetAbsoluteItemRect(), _toolTipLayoutItem._isHorizontallyScrolled);
			rectangle.Location = PointToScreen(rectangle.Location);
			_gridWindowMessageHandler._anchorBounds = rectangle;
			_gridWindowMessageHandler.SetTooltipText(text);
		}

		internal Size GetImageSize()
		{
			if (_imageSource == ImageSource.InternalImageList)
			{
				return GetOrCreateImageList().ImageSize;
			}
			if (_externalImageList != null)
			{
				return _externalImageList.ImageSize;
			}
			return new Size(0, 0);
		}

		private GridRow GetGridRowByPosition(int value)
		{
			if (value < 0)
			{
				if (value == -2 && _filterMode != FilterMode.None)
				{
					return GetFilterGridRow();
				}
				if (value == -1 && IsNewRowAtTop())
				{
					return GetGridRowForLogicalRow(_gridDataSourceManager.GetRootNewRow());
				}
				return null;
			}
			if (GetRowManager().IsIndexSuspend)
			{
				return null;
			}
			if (value == GetTotalRowPosition())
			{
				return GetGridRowForLogicalRow(_gridDataSourceManager.GetRootTotalRow());
			}
			if (value <= RowCount - 1)
			{
				return GetGridRowForLogicalRow(GetRowManager()[value]);
			}
			return null;
		}

		private int GetVisibleRowsPerPage()
		{
			if (GetVScroll().GetPageSize() <= 0)
			{
				return 1;
			}
			return GetVScroll().GetPageSize();
		}

		public void EnsureVisible()
		{
			EnsureVisible(Row, _currentColumn);
		}

		public void EnsureVisible(int rowPosition)
		{
			EnsureVisible(rowPosition, null);
		}

		public void EnsureVisible(int rowPosition, GridColumn column)
		{
			if (!_isCurrentCellChanging && base.IsHandleCreated)
			{
				if (_view == View.TableView)
				{
					EnsureTableCellVisible(rowPosition, column);
				}
				else if (_view == View.CardView || _view == View.SingleCard)
				{
					EnsureCardCellVisible(rowPosition, column);
				}
				else
				{
					FirstRow = rowPosition;
				}
			}
		}

		private int GetRowLayoutRightEdge(MxRowLayoutState mordexRowLayoutState)
		{
			int right = 0;
			if (GetEffectiveFrozenColumns() != 0)
			{
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					if (IsScrollableLayoutItem(item) && item._type != LayoutItemType.GroupHeaderRow)
					{
						Rectangle rectangle = ScrollOffsetRectangle(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
						if (rectangle.Right > right)
						{
							right = rectangle.Right;
						}
					}
				}
			}
			return right;
		}

		internal void ScrollColumnIntoView(GridColumn gridColumn)
		{
			if (base.IsDisposed || base.Disposing || View != View.TableView)
			{
				return;
			}
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			if (gridColumn == null || !gridColumn.Visible || gridColumn.Table == null || gridColumn.Table.DataGrid != this)
			{
				return;
			}
			GridFormatStyle gridFormatStyle = GetHeaderStyle(gridColumn.Table);
			Graphics graphics = CreateGraphics();
			MxRowLayoutState mordexRowLayoutState = gridColumn.Table.BuildColumnHeaderLayout(graphics, null, null, gridFormatStyle, GetEffectiveFrozenColumns(), true);
			int value = mordexRowLayoutState.GetRightEdgeBeforeColumn(GetEffectiveFrozenColumns()) - base.ClientRectangle.Left + _borderWidth;
			graphics.Dispose();
			graphics = null;
			if (!gridColumn.Table.IsRootTable())
			{
				Point point = mordexRowLayoutState.GetLocation();
				point.X = gridColumn.Table.CalculateIndentWidth(true);
				mordexRowLayoutState.SetLocation(point);
			}
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._column != gridColumn || IsScrollableLayoutItem(item))
				{
					continue;
				}
				Rectangle rectangle = ScrollOffsetRectangle(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				if (rectangle.Left <= value)
				{
					if (gridColumn.Position == 0)
					{
						SetHorizontalScrollOffset(0);
					}
					else
					{
						SetHorizontalScrollOffset(item.GetAbsoluteItemRect().Left - value);
					}
				}
				else if (rectangle.Right > base.ClientRectangle.Right - base.ClientRectangle.Left)
				{
					rectangle = item.GetAbsoluteItemRect();
					int otherValue = rectangle.Right - base.ClientRectangle.Width;
					if (otherValue <= 0)
					{
						otherValue = 0;
					}
					if (rectangle.Left - otherValue < value)
					{
						otherValue = rectangle.Left - value;
					}
					SetHorizontalScrollOffset(otherValue);
				}
				break;
			}
			gridFormatStyle.DisposeResources();
		}

		private void EnsureTableCellVisible(int firstRow, GridColumn gridColumn)
		{
			if (base.IsDisposed || base.Disposing || ((firstRow < -1 || firstRow >= RowCount) && (firstRow != -2 || _filterMode == FilterMode.None)))
			{
				return;
			}
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			if (firstRow != -1 && firstRow != -2 && !IsRowInVisibleTableRange(firstRow))
			{
				if (firstRow <= _firstRow)
				{
					FirstRow = firstRow;
				}
				else
				{
					ScrollTableRowIntoView(firstRow);
				}
			}
			if (gridColumn == null || !gridColumn.Visible || gridColumn.Table == null || gridColumn.Table.DataGrid != this)
			{
				return;
			}
			GridFormatStyle gridFormatStyle = GetHeaderStyle(gridColumn.Table);
			Graphics graphics = CreateGraphics();
			MxRowLayoutState mordexRowLayoutState = gridColumn.Table.BuildColumnHeaderLayout(graphics, null, null, gridFormatStyle, GetEffectiveFrozenColumns(), true);
			int value = mordexRowLayoutState.GetRightEdgeBeforeColumn(GetEffectiveFrozenColumns()) - base.ClientRectangle.Left + _borderWidth;
			graphics.Dispose();
			graphics = null;
			if (!gridColumn.Table.IsRootTable())
			{
				Point point = mordexRowLayoutState.GetLocation();
				point.X = gridColumn.Table.CalculateIndentWidth(true);
				mordexRowLayoutState.SetLocation(point);
			}
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._column != gridColumn || IsScrollableLayoutItem(item))
				{
					continue;
				}
				Rectangle rectangle = ScrollOffsetRectangle(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				if (rectangle.Left <= value)
				{
					if (gridColumn.Position == 0)
					{
						SetHorizontalScrollOffset(0);
					}
					else
					{
						SetHorizontalScrollOffset(item.GetAbsoluteItemRect().Left - value);
					}
				}
				else if (rectangle.Right > base.ClientRectangle.Right - base.ClientRectangle.Left)
				{
					rectangle = item.GetAbsoluteItemRect();
					int otherValue = rectangle.Right - base.ClientRectangle.Width;
					if (otherValue <= 0)
					{
						otherValue = 0;
					}
					if (rectangle.Left - otherValue < value)
					{
						otherValue = rectangle.Left - value;
					}
					SetHorizontalScrollOffset(otherValue);
				}
				break;
			}
			gridFormatStyle.DisposeResources();
		}

		private void EnsureCardCellVisible(int firstRow, GridColumn gridColumn)
		{
			if (firstRow < 0 || firstRow >= RowCount)
			{
				return;
			}
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			if (!IsRowInVisibleCardRange(firstRow))
			{
				if (firstRow < _firstRow)
				{
					FirstRow = firstRow;
				}
				else
				{
					ScrollCardRowIntoView(firstRow);
				}
			}
			if (gridColumn == null || !gridColumn.Visible)
			{
				return;
			}
			if (gridColumn == GetFirstEditableColumn(_row) && GetVerticalScrollOffset() != 0)
			{
				SetVerticalScrollOffset(0);
			}
			if (gridColumn.Table.DataGrid != this)
			{
				return;
			}
			int value = _cardRowLayouts.GetCount();
			for (int i = 0; i < value; i++)
			{
				if (i >= _cardRowLayouts.GetCount())
				{
					continue;
				}
				GridTableLayoutState gridTableLayoutState = _cardRowLayouts.GetTableLayout(i);
				if (gridTableLayoutState.GetRowPosition() != firstRow)
				{
					continue;
				}
				foreach (GridGroupLayoutState item in gridTableLayoutState._arrayList)
				{
					if (item._type != LayoutItemType.CardCell || item._column != gridColumn)
					{
						continue;
					}
					Rectangle rectangle = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
					if (Rectangle.Intersect(base.ClientRectangle, rectangle) != rectangle)
					{
						rectangle = item.GetAbsoluteItemRect();
						if (rectangle.Top - GetVerticalScrollOffset() < 0)
						{
							SetVerticalScrollOffset(rectangle.Top - 4);
						}
						else if (rectangle.Bottom > base.ClientRectangle.Bottom)
						{
							SetVerticalScrollOffset(rectangle.Bottom + 4 - base.ClientRectangle.Bottom);
						}
					}
					break;
				}
			}
		}

		private void ScrollTableRowIntoView(int value)
		{
			if (!base.IsDisposed && !base.Disposing)
			{
				Graphics graphics = CreateGraphics();
				int firstRow = CalculateFirstRowForTableView(graphics, value);
				graphics.Dispose();
				graphics = null;
				FirstRow = firstRow;
			}
		}

		private void ScrollCardRowIntoView(int otherFirstRow)
		{
			if (!base.IsDisposed && !base.Disposing)
			{
				if (_view == View.CardView)
				{
					Graphics graphics = CreateGraphics();
					int firstRow = CalculateFirstRowForCardView(graphics, otherFirstRow);
					graphics.Dispose();
					graphics = null;
					FirstRow = firstRow;
				}
				else
				{
					FirstRow = otherFirstRow;
				}
			}
		}
	}
}
