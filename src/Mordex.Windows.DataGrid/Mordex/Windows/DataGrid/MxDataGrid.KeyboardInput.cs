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
		private void MoveToCurrentPosition(bool condition)
		{
			if (_isDataChanging || !_isDataChangeTrackingEnabled || CurrencyManager == null)
			{
				return;
			}
			if (_currencyManager.Position == -1 || _currencyManager.List == null || _currencyManager.List.Count == 0)
			{
				if (condition)
				{
					Refetch();
				}
				if (_gridDataSourceManager.AllowAddNew)
				{
					MoveToNewRecord();
				}
				else
				{
					Row = -3;
				}
			}
			else if (Row != -2)
			{
				GridRow row = GetRow(_currencyManager.Current);
				if (row != null && (_row == null || _row.DataRow != row.DataRow))
				{
					row.GetLogicalRow().EnsureVisible();
					IsRowFocusableInCurrentColumn(row.Position);
					SyncCellSelectionWithCurrent();
				}
			}
		}

		private GridLayout FindLayoutByKey(GridLayoutCollection gridLayoutCollection, string strB)
		{
			foreach (GridLayout item in gridLayoutCollection)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return item;
				}
				GridLayout layoutByKey = FindLayoutByKey(item.ChildLayouts, strB);
				if (layoutByKey != null)
				{
					return layoutByKey;
				}
			}
			return null;
		}

		private Image GetImageByKey(string key)
		{
			if (key.Length > 0)
			{
				ImageList imageList = ComputeImageList();
				if (imageList != null && imageList.Images.ContainsKey(key))
				{
					return imageList.Images[key];
				}
			}
			return null;
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.End:
			case Keys.Home:
			case Keys.Left:
			case Keys.Up:
			case Keys.Right:
			case Keys.Down:
				return true;
			default:
				return base.IsInputKey(keyData);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (_isKeyEventHandled || e.Handled)
			{
				return;
			}
			Keys keys = e.KeyCode;
			if (RightToLeft == RightToLeft.Yes)
			{
				if (_view == View.TableView)
				{
					switch (keys)
					{
					case Keys.Left:
						keys = Keys.Right;
						break;
					case Keys.Right:
						keys = Keys.Left;
						break;
					}
				}
				else
				{
					switch (keys)
					{
					case Keys.Left:
						keys = Keys.Down;
						break;
					case Keys.Right:
						keys = Keys.Up;
						break;
					}
				}
			}
			if (e.KeyCode == Keys.Down && e.Alt)
			{
				BeginCellEdit(false, '\0', false);
				if (_gridColumnContainerControl != null)
				{
					_gridColumnContainerControl.DropEditorList();
				}
				return;
			}
			switch (keys)
			{
			case Keys.Down:
				if (e.Control && View == View.TableView)
				{
					MoveLast();
				}
				else
				{
					MoveCurrentDownOneLine();
				}
				break;
			case Keys.Up:
				if (e.Control && View == View.TableView)
				{
					MoveFirst();
				}
				else
				{
					MoveCurrentUpOneLine();
				}
				break;
			case Keys.Right:
				if (_view == View.TableView)
				{
					if (_row != null)
					{
						if (_row.RowType == RowType.GroupHeader)
						{
							if (!_row.Expanded)
							{
								SetHorizontalScrollOffset(0);
								ToggleRowExpanded(_row.GetLogicalRow());
							}
							else
							{
								Row++;
							}
						}
						else if (RowStateHelper.CanExpandRow(this, _row.GetLogicalRow()) && !_row.Expanded)
						{
							if (CanExpandRow(_row.GetLogicalRow()))
							{
								GridColumn lastEditableColumn = GetLastEditableColumn(_row);
								if (lastEditableColumn == null || _currentColumn == lastEditableColumn)
								{
									_row.Expanded = true;
								}
								else
								{
									MoveCurrentToNextCell();
								}
							}
							else
							{
								MoveCurrentToNextCell();
							}
						}
						else
						{
							MoveCurrentToNextCell();
						}
					}
					else if (HasFocusableColumnFrom(0, 0))
					{
						SyncCellSelectionWithCurrent();
					}
				}
				else
				{
					MoveCurrentToNextCell();
				}
				break;
			case Keys.Left:
				if (_view == View.TableView)
				{
					if (_row == null)
					{
						break;
					}
					if (_row.RowType == RowType.GroupHeader)
					{
						if (_row.Expanded)
						{
							SetHorizontalScrollOffset(0);
							ToggleRowExpanded(_row.GetLogicalRow());
						}
						else if (IsValidRowPosition(Row - 1))
						{
							Row--;
						}
					}
					else if (RowStateHelper.CanExpandRow(this, _row.GetLogicalRow()) && _row.Expanded)
					{
						GridColumn firstEditableColumn = GetFirstEditableColumn(_row);
						if (CurrentColumn == null || firstEditableColumn == null || _currentColumn == firstEditableColumn)
						{
							_row.Expanded = false;
						}
						else
						{
							MoveCurrentToPreviousCell();
						}
					}
					else
					{
						MoveCurrentToPreviousCell();
					}
				}
				else
				{
					MoveCurrentToPreviousCell();
				}
				break;
			case Keys.Home:
				SetHorizontalScrollOffset(0);
				Col = 0;
				break;
			case Keys.End:
				MoveCurrentToRowLastColumn(Row);
				break;
			case Keys.F2:
				BeginCellEdit(false, '\0', false);
				break;
			case Keys.F4:
				if (e.KeyCode == e.KeyData)
				{
					EnsureVisible();
					BeginCellEdit(false, '\0', false);
					Update();
					if (_gridColumnContainerControl != null)
					{
						_gridColumnContainerControl.DropEditorList();
					}
					break;
				}
				return;
			case Keys.Next:
				if (e.Shift)
				{
					if (SelectionMode == SelectionMode.MultipleSelection)
					{
						if (IsRowFocusableInCurrentColumn(GetPageDownTargetRow()))
						{
							SelectRowRange(Row, _selectionAnchorRow, false);
							Update();
						}
					}
					else if (SelectionMode == SelectionMode.MultipleSelectionSameTable)
					{
						MordexRow mordexRow = FindNextRowOfTable(GetPageDownTargetRow() - 1, CurrentTable);
						if (mordexRow != null && IsRowFocusableInCurrentColumn(mordexRow.RowManagerPosition))
						{
							SelectRowRange(Row, _selectionAnchorRow, true);
							Update();
						}
					}
				}
				else if (e.Control)
				{
					if (SelectionMode == SelectionMode.MultipleSelection)
					{
						if (IsRowFocusableInCurrentColumn(GetPageDownTargetRow()))
						{
							Update();
						}
					}
					else if (SelectionMode == SelectionMode.MultipleSelectionSameTable)
					{
						MordexRow nextRowOfTable = FindNextRowOfTable(GetPageDownTargetRow() - 1, CurrentTable);
						if (nextRowOfTable != null && IsRowFocusableInCurrentColumn(nextRowOfTable.RowManagerPosition))
						{
							Update();
						}
					}
				}
				else
				{
					_isCurrentCellChanging = true;
					if (IsRowFocusableInCurrentColumn(GetPageDownTargetRow()))
					{
						SyncCellSelectionWithCurrent();
						_isCurrentCellChanging = false;
						EnsureVisible();
						Update();
					}
					else
					{
						_isCurrentCellChanging = false;
					}
				}
				break;
			case Keys.Prior:
				if (e.Shift)
				{
					if (SelectionMode == SelectionMode.MultipleSelection)
					{
						if (IsRowFocusableInCurrentColumn(GetPageUpTargetRow()))
						{
							SelectRowRange(Row, _selectionAnchorRow, false);
							Update();
						}
					}
					else if (SelectionMode == SelectionMode.MultipleSelectionSameTable)
					{
						MordexRow previousRowOfTable = FindPreviousRowOfTable(GetPageUpTargetRow() + 1, CurrentTable);
						if (previousRowOfTable == null)
						{
							previousRowOfTable = FindNextRowOfTable(0, CurrentTable);
						}
						if (previousRowOfTable != null && IsRowFocusableInCurrentColumn(previousRowOfTable.RowManagerPosition))
						{
							SelectRowRange(Row, _selectionAnchorRow, true);
							Update();
						}
					}
				}
				else if (e.Control)
				{
					if (SelectionMode == SelectionMode.MultipleSelection)
					{
						if (IsRowFocusableInCurrentColumn(GetPageUpTargetRow()))
						{
							Update();
						}
					}
					else if (SelectionMode == SelectionMode.MultipleSelectionSameTable)
					{
						MordexRow otherPreviousRowOfTable = FindPreviousRowOfTable(GetPageUpTargetRow() + 1, CurrentTable);
						if (otherPreviousRowOfTable == null)
						{
							otherPreviousRowOfTable = FindNextRowOfTable(0, CurrentTable);
						}
						if (otherPreviousRowOfTable != null && IsRowFocusableInCurrentColumn(otherPreviousRowOfTable.RowManagerPosition))
						{
							Update();
						}
					}
				}
				else
				{
					_isCurrentCellChanging = true;
					if (IsRowFocusableInCurrentColumn(GetPageUpTargetRow()))
					{
						SyncCellSelectionWithCurrent();
						_isCurrentCellChanging = false;
						EnsureVisible();
						Update();
					}
					else
					{
						_isCurrentCellChanging = false;
					}
				}
				break;
			case Keys.Delete:
				if (CurrentRow != null && CurrentRow.RowType == RowType.FilterRow && CurrentColumn != null)
				{
					SetValue(CurrentColumn, null);
					UpdateData();
					e.Handled = true;
					return;
				}
				Delete();
				break;
			case Keys.Space:
				SyncCellSelectionWithCurrent();
				if (_currentColumn != null || _row == null || _row.Table == null || (_row.RowType != RowType.Record && _row.RowType != RowType.NewRecord))
				{
					break;
				}
				foreach (GridColumn column in _row.Table.Columns)
				{
					if (column.ActAsSelector && column.Visible)
					{
						ApplyRowCheckFromEditor(!_row.IsChecked);
						OnCellValueChanged(new ColumnActionEventArgs(column));
						break;
					}
					if (column.ShowRowSelector && column.Visible)
					{
						ApplyRowCheckFromEditor(!_row.IsChecked);
						break;
					}
				}
				break;
			default:
				return;
			}
			e.Handled = true;
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (_isKeyEventHandled || _gridColumnContainerControl != null || e.Handled)
			{
				return;
			}
			if (_row != null)
			{
				if (_row.RowType == RowType.GroupHeader && CurrentTable.AreGroupsExpandable())
				{
					if (e.KeyChar == '+')
					{
						_row.Expanded = true;
					}
					else if (e.KeyChar == '-')
					{
						_row.Expanded = false;
					}
				}
				else if (_view != View.TableView && _currentColumn == null && (e.KeyChar == '+' || e.KeyChar == '-'))
				{
					ToggleCardExpanded(_row.GetLogicalRow());
				}
			}
			if (TryIncrementalSearch(e.KeyChar) || char.IsControl(e.KeyChar))
			{
				return;
			}
			BeginCellEdit(false, e.KeyChar, false);
			if (_gridColumnContainerControl == null || char.IsControl(e.KeyChar))
			{
				return;
			}
			try
			{
				_gridColumnContainerControl.StartEditorWithChar(e.KeyChar);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (!HandleNavigationKey(keyData))
			{
				return base.ProcessDialogKey(keyData);
			}
			return true;
		}

		private int GetPageDownTargetRow()
		{
			int pageDownTargetRow = 0;
			if (View != View.TableView)
			{
				pageDownTargetRow = Row + 1;
			}
			else
			{
				if (!IsVerticalScrollRequired())
				{
					return RowCount - 1;
				}
				pageDownTargetRow = Row + GetVisibleRowsPerPage() - 1;
			}
			if (pageDownTargetRow >= RowCount)
			{
				pageDownTargetRow = RowCount - 1;
			}
			return pageDownTargetRow;
		}

		private int GetPageUpTargetRow()
		{
			int pageUpTargetRow = 0;
			if (View != View.TableView)
			{
				pageUpTargetRow = Row - 1;
				if (pageUpTargetRow < 0)
				{
					pageUpTargetRow = 0;
				}
				if (pageUpTargetRow > RowCount - 1)
				{
					pageUpTargetRow = -1;
				}
				return pageUpTargetRow;
			}
			if (IsVerticalScrollRequired())
			{
				pageUpTargetRow = Row - GetVisibleRowsPerPage() + 1;
				if (pageUpTargetRow < 0 && Row > 0)
				{
					return 0;
				}
				if (pageUpTargetRow <= -1)
				{
					if (IsNewRowAtTop())
					{
						return -1;
					}
					return 0;
				}
				return pageUpTargetRow;
			}
			if (Row <= 0 && IsNewRowAtTop())
			{
				return -1;
			}
			return 0;
		}

		private void OnStateKeyDown(object sender, KeyEventArgs eventArgs)
		{
			_isKeyEventHandled = true;
			try
			{
				OnKeyDown(eventArgs);
			}
			finally
			{
				_isKeyEventHandled = false;
			}
		}

		private void OnStateKeyUp(object sender, KeyEventArgs eventArgs)
		{
			_isKeyEventHandled = true;
			try
			{
				OnKeyUp(eventArgs);
			}
			finally
			{
				_isKeyEventHandled = false;
			}
		}

		private void OnStateKeyPress(object sender, KeyPressEventArgs eventArgs)
		{
			_isKeyEventHandled = true;
			try
			{
				OnKeyPress(eventArgs);
			}
			finally
			{
				_isKeyEventHandled = false;
			}
		}

		protected internal override void OnNavigatorMoveToRow(int row)
		{
			try
			{
				MoveTo(row);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				SyncRecordNavigatorPosition();
				ProcessException(new Exception(BuiltInTexts[GridBuiltInText.RecordNavigatorError]), true);
			}
			base.ActiveControl = null;
			FocusEditorOrGrid();
		}

		private void SyncRecordNavigatorPosition()
		{
			if (_numericEditControl == null)
			{
				return;
			}
			if (_row != null && _row.Table != null)
			{
				if (_row.Table == RootTable)
				{
					_numericEditControl.SetCurrentIndex(_row.GetLogicalRow().RecordPosition);
					_numericEditControl.SetIsNewRecord(_row.RowType == RowType.NewRecord);
				}
				else
				{
					_numericEditControl.SetIsNewRecord(false);
					_numericEditControl.SetCurrentIndex(RowStateHelper.FindRootDataRow(_row.GetLogicalRow()).RecordPosition);
				}
			}
			else
			{
				_numericEditControl.SetCurrentIndex(-1);
			}
		}

		public void MoveToRowIndex(int rowIndex)
		{
			if (_currencyManager.Position != rowIndex)
			{
				_currencyManager.Position = rowIndex;
			}
			else
			{
				OnCurrencyManagerPositionChanged(_currencyManager, EventArgs.Empty);
			}
		}

		public void MoveTo(int recordPosition)
		{
			if (recordPosition == 0)
			{
				MoveFirst();
			}
			else if (RootTable != null && _gridDataSourceManager.RootRow != null)
			{
				MordexRow[] dataRows = _gridDataSourceManager.RootRow.GetDataRows(RootTable);
				if (recordPosition < 0 || recordPosition >= dataRows.Length)
				{
					throw new ArgumentOutOfRangeException();
				}
				MordexRow mordexRow = dataRows[recordPosition];
				if (mordexRow != null)
				{
					MoveTo(GetGridRowForLogicalRow(mordexRow));
				}
			}
		}

		public void MoveTo(GridRow row)
		{
			if (row != null)
			{
				if (row.DataGrid == this)
				{
					if (row.GetLogicalRow() != null)
					{
						object dataRow = row.GetLogicalRow().DataRow;
						if (dataRow != null)
						{
							row = GetRow(row.GetLogicalRow().DataRow);
						}
						if (row == null)
						{
							row = GetRow(dataRow);
						}
						row.GetLogicalRow().EnsureVisible();
						IsRowFocusableInCurrentColumn(row.Position);
						SyncCellSelectionWithCurrent();
					}
				}
				else if (row.DataGrid != null)
				{
					throw new InvalidOperationException();
				}
			}
			else
			{
				Row = -3;
			}
		}

		public void MoveFirst()
		{
			if (RootTable == null || _gridDataSourceManager.RootRow == null)
			{
				return;
			}
			FirstRow = 0;
			MordexRow[] dataRows = _gridDataSourceManager.RootRow.GetDataRows(RootTable);
			if (dataRows.Length > 0)
			{
				MordexRow mordexRow = dataRows[0];
				if (mordexRow != null)
				{
					MoveTo(GetGridRowForLogicalRow(mordexRow));
				}
			}
		}

		public void MovePrevious()
		{
			if (RootTable == null || Row < 0 || _row == null)
			{
				return;
			}
			MordexRow mordexRow = _row.GetLogicalRow();
			while (mordexRow != null)
			{
				mordexRow = mordexRow.PreviousRowLoaded;
				if (mordexRow != null && mordexRow.Visible && (mordexRow is MordexDataRow || mordexRow is MordexNewDataRow) && mordexRow.Table == RootTable)
				{
					break;
				}
			}
			if (mordexRow != null)
			{
				MoveTo(GetGridRowForLogicalRow(mordexRow));
			}
		}

		public void MoveNext()
		{
			if (RootTable == null)
			{
				return;
			}
			if (Row >= 0)
			{
				if (_row == null)
				{
					return;
				}
				MordexRow mordexRow = _row.GetLogicalRow();
				while (mordexRow != null)
				{
					mordexRow = mordexRow.NextRowLoaded;
					if (mordexRow != null && mordexRow.Visible && (mordexRow is MordexDataRow || mordexRow is MordexNewDataRow) && mordexRow.Table == RootTable)
					{
						break;
					}
				}
				if (mordexRow != null)
				{
					MoveTo(GetGridRowForLogicalRow(mordexRow));
				}
			}
			else
			{
				MoveFirst();
			}
		}

		public void MoveLast()
		{
			if (RootTable == null || GetRowSourceManager() == null)
			{
				return;
			}
			MordexRow[] dataRows = _gridDataSourceManager.RootRow.GetDataRows(RootTable);
			if (dataRows.Length > 0)
			{
				MordexRow mordexRow = dataRows[dataRows.Length - 1];
				if (mordexRow != null)
				{
					MoveTo(GetGridRowForLogicalRow(mordexRow));
				}
			}
		}

		public void MoveToNewRecord()
		{
			if ((_row != null && _row.Table != null && _row.Table.IsRootTable() && _row.RowType == RowType.NewRecord) || !UpdateData() || RootTable == null || !RootTable.IsAddNewAllowed())
			{
				return;
			}
			int rowManagerPosition = -1;
			if (RootTable.ComputeNewRowPosition() == NewRowPosition.TopRow)
			{
				rowManagerPosition = -1;
			}
			else
			{
				if (_gridDataSourceManager.GetRootNewRow() != null)
				{
					rowManagerPosition = _gridDataSourceManager.GetRootNewRow().RowManagerPosition;
				}
				if (rowManagerPosition == -1)
				{
					return;
				}
			}
			if (CanFocusCell(rowManagerPosition, _currentColumn))
			{
				SyncCellSelectionWithCurrent();
			}
		}

		private string GetNavigatorCaptionAndText()
		{
			if (_numericEditControl != null)
			{
				return _numericEditControl.GetCaptionText() + "|" + _numericEditControl.GetNumericEditControlText();
			}
			return string.Empty;
		}

		private void ApplyNavigatorCaptionAndText(string otherText)
		{
			if (otherText == null || otherText.IndexOf("|") == -1)
			{
				otherText += "|";
			}
			int length = otherText.IndexOf("|");
			string text = otherText.Substring(0, length);
			string numericEditControlText = otherText.Substring(length + 1);
			if (_numericEditControl != null)
			{
				_numericEditControl.SetCaptionText(text);
				_numericEditControl.UpdateNumericEditControlText(numericEditControlText);
			}
		}

		internal void ApplyRecordNavigatorBuiltInText()
		{
			if (_builtInTexts != null)
			{
				ApplyNavigatorCaptionAndText(_builtInTexts[GridBuiltInText.RecordNavigator]);
			}
			RefreshRowCountAndInvalidate();
		}

		internal void RemoveOrphanRootTableSortKeys()
		{
			if (!_isInitializing && RootTable != null)
			{
				RootTable.RemoveInvalidSortKeysAndGroups();
			}
		}

		protected virtual void OnSortKeysChanged(EventArgs e)
		{
			if (base.Events[_sortKeysChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		bool IMordexContainer.PreProcessDialogKey(Keys keyData)
		{
			return HandleNavigationKey(keyData);
		}

		private void ProcessKeyInput(KeyEventArgs eventArgs)
		{
			int col = 0;
			new Point(Col, Row);
			MordexRow mordexRow = null;
			bool condition = false;
			GridRow gridRow = null;
			if (eventArgs.KeyCode == Keys.Down)
			{
				if (_row == null)
				{
					if (IsValidRowPosition(0))
					{
						MoveTo(0);
						Col = 0;
					}
				}
				else
				{
					if (Row < 0)
					{
						return;
					}
					if (_selectionAnchorRow < 0)
					{
						_selectionAnchorRow = Row;
					}
					gridRow = GetRow(_selectionAnchorRow);
					if (GetSelectionAnchorColumn() == null)
					{
						condition = true;
					}
					else if (gridRow.RowType == RowType.GroupFooter || gridRow.RowType == RowType.GroupHeader)
					{
						condition = true;
					}
					for (int i = Row + 1; i < GetRowManager().Count; i++)
					{
						MordexRow rowManagerItem = GetRowManager()[i];
						if (!condition)
						{
							if (rowManagerItem.Table == gridRow.Table)
							{
								if (rowManagerItem is MordexDataRow || rowManagerItem is MordexNewDataRow)
								{
									mordexRow = rowManagerItem;
									break;
								}
								if (rowManagerItem is MordexTotalRow && !(rowManagerItem.ParentRow is MordexGroupRow))
								{
									mordexRow = rowManagerItem;
								}
							}
						}
						else if (gridRow.Table == rowManagerItem.Table)
						{
							mordexRow = rowManagerItem;
							break;
						}
					}
					col = Col;
				}
			}
			else if (eventArgs.KeyCode == Keys.Up)
			{
				if (_row == null)
				{
					if (IsValidRowPosition(0))
					{
						MoveTo(0);
						Col = 0;
					}
				}
				else
				{
					if (Row < 0)
					{
						return;
					}
					if (_selectionAnchorRow < 0)
					{
						_selectionAnchorRow = Row;
					}
					gridRow = GetRow(_selectionAnchorRow);
					if (GetSelectionAnchorColumn() == null)
					{
						condition = true;
					}
					else if (gridRow.RowType == RowType.GroupFooter || gridRow.RowType == RowType.GroupHeader)
					{
						condition = true;
					}
					for (int value = Row - 1; value >= 0; value--)
					{
						MordexRow otherRowManagerItem = GetRowManager()[value];
						if (!condition)
						{
							if (otherRowManagerItem.Table == gridRow.Table)
							{
								if (otherRowManagerItem is MordexDataRow || otherRowManagerItem is MordexNewDataRow)
								{
									mordexRow = otherRowManagerItem;
									break;
								}
								if (otherRowManagerItem is MordexTotalRow && !(otherRowManagerItem.ParentRow is MordexGroupRow))
								{
									mordexRow = otherRowManagerItem;
								}
							}
						}
						else if (gridRow.Table == otherRowManagerItem.Table)
						{
							mordexRow = otherRowManagerItem;
							break;
						}
					}
					col = Col;
				}
			}
			else if (eventArgs.KeyCode == Keys.Right)
			{
				if (GetSelectionAnchorColumn() == null)
				{
					return;
				}
				if (_row != null)
				{
					mordexRow = _row.GetLogicalRow();
					if (Col < 0)
					{
						col = 0;
					}
					else
					{
						col = Col;
						for (int j = Col + 1; j < _row.Table.Columns.Count; j++)
						{
							GridColumn columnInPosition = _row.Table.Columns.GetColumnInPosition(j);
							if (columnInPosition.Visible)
							{
								col = j;
								break;
							}
						}
					}
				}
			}
			else if (eventArgs.KeyCode == Keys.Left)
			{
				if (GetSelectionAnchorColumn() == null)
				{
					return;
				}
				if (_row != null)
				{
					mordexRow = _row.GetLogicalRow();
					if (Col > 0)
					{
						col = Col;
						for (int position = Col - 1; position >= 0; position--)
						{
							GridColumn otherColumnInPosition = _row.Table.Columns.GetColumnInPosition(position);
							if (otherColumnInPosition.Visible)
							{
								col = position;
								break;
							}
						}
					}
				}
			}
			if (mordexRow == null)
			{
				return;
			}
			Rectangle rectangle = RectangleFromCorners(new Point((GetSelectionAnchorColumn() != null) ? GetSelectionAnchorColumn().Position : 0, _selectionAnchorRow), new Point(Col, Row));
			HasFocusableColumnFrom(mordexRow.RowManagerPosition, col);
			if (_row != null && _row.GetLogicalRow() == mordexRow)
			{
				if (condition)
				{
					SelectRowRangeWithAllCells(_selectionAnchorRow, mordexRow.RowManagerPosition);
					return;
				}
				Rectangle otherRectangle = RectangleFromCorners(new Point((GetSelectionAnchorColumn() != null) ? GetSelectionAnchorColumn().Position : 0, _selectionAnchorRow), new Point(col, Row));
				DeselectCellsOutsideRange(rectangle, otherRectangle);
				SelectCellsInRange(otherRectangle);
			}
		}

		private bool HandleNavigationKey(Keys keyData)
		{
			KeyEventArgs e = new KeyEventArgs(keyData);
			if ((keyData & Keys.Shift) != Keys.None)
			{
				if (keyData == (Keys.Tab | Keys.Shift) && _tabKeyBehavior == TabKeyBehavior.ColumnNavigation)
				{
					base.OnKeyDown(e);
					if (!e.Handled)
					{
						MoveCurrentToPreviousCell();
					}
					return true;
				}
				if (SelectionMode == SelectionMode.MultipleSelection)
				{
					switch (keyData)
					{
					case Keys.Down | Keys.Shift:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MoveCurrentToNextRow(false);
							SelectRowRange(Row, _selectionAnchorRow, false);
							Update();
						}
						return true;
					case Keys.Up | Keys.Shift:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MoveCurrentToPreviousRow(false);
							SelectRowRange(Row, _selectionAnchorRow, false);
							Update();
						}
						return true;
					}
				}
				else if (SelectionMode == SelectionMode.MultipleSelectionSameTable)
				{
					switch (keyData)
					{
					case Keys.Down | Keys.Shift:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MordexRow nextRowOfTable = FindNextRowOfTable(Row, CurrentTable);
							if (nextRowOfTable != null && IsRowFocusableInCurrentColumn(nextRowOfTable.RowManagerPosition))
							{
								SelectRowRange(Row, _selectionAnchorRow, true);
								Update();
							}
						}
						return true;
					case Keys.Up | Keys.Shift:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MordexRow mordexRow = FindPreviousRowOfTable(Row, CurrentTable);
							if (mordexRow != null && IsRowFocusableInCurrentColumn(mordexRow.RowManagerPosition))
							{
								SelectRowRange(Row, _selectionAnchorRow, true);
								Update();
							}
						}
						return true;
					}
				}
				else if (SelectionMode == SelectionMode.MultipleCellSelection)
				{
					switch (keyData)
					{
					case Keys.Up | Keys.Shift:
					case Keys.Down | Keys.Shift:
						base.OnKeyDown(e);
						if (!e.Handled && SelectionMode == SelectionMode.MultipleCellSelection)
						{
							ProcessKeyInput(e);
						}
						return true;
					case Keys.Left | Keys.Shift:
					case Keys.Right | Keys.Shift:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							if (_row != null && RowStateHelper.IsContainerRow(_row.GetLogicalRow()) && _row.Expanded)
							{
								ToggleRowExpanded(_row.GetLogicalRow());
							}
							else
							{
								ProcessKeyInput(e);
							}
						}
						return true;
					}
				}
				switch (keyData)
				{
				case Keys.Left | Keys.Shift:
					base.OnKeyDown(e);
					if (!e.Handled && _row != null && RowStateHelper.IsContainerRow(_row.GetLogicalRow()) && _row.Expanded)
					{
						ToggleRowExpanded(_row.GetLogicalRow());
					}
					return true;
				case Keys.Right | Keys.Shift:
					base.OnKeyDown(e);
					if (!e.Handled && _row != null && RowStateHelper.IsContainerRow(_row.GetLogicalRow()) && !_row.Expanded)
					{
						ToggleRowExpanded(_row.GetLogicalRow());
					}
					return true;
				}
			}
			else if ((keyData & Keys.Control) != Keys.None)
			{
				switch (keyData)
				{
				case Keys.Home | Keys.Control:
					base.OnKeyDown(e);
					if (!e.Handled)
					{
						MoveFirst();
					}
					return true;
				case Keys.End | Keys.Control:
					base.OnKeyDown(e);
					if (!e.Handled)
					{
						MoveLast();
					}
					return true;
				case Keys.Tab | Keys.Control:
					base.OnKeyDown(e);
					if (!e.Handled && base.TopLevelControl != null)
					{
						base.TopLevelControl.SelectNextControl(this, forward: true, tabStopOnly: true, nested: true, wrap: true);
					}
					return true;
				case Keys.Space | Keys.Control:
					if (SelectionMode == SelectionMode.SingleSelection || _row == null)
					{
						break;
					}
					base.OnKeyDown(e);
					if (!e.Handled)
					{
						if (_row.Selected)
						{
							RemoveSelectedItemAtPosition(Row);
						}
						else
						{
							AddRowToSelection(Row);
						}
					}
					return true;
				}
				if (SelectionMode == SelectionMode.MultipleSelection)
				{
					switch (keyData)
					{
					case Keys.Down | Keys.Control:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MoveCurrentToNextRow(false);
							_selectionAnchorRow = Row;
							Update();
						}
						return true;
					case Keys.Up | Keys.Control:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MoveCurrentToPreviousRow(false);
							_selectionAnchorRow = Row;
							Update();
						}
						return true;
					}
				}
				else if (SelectionMode == SelectionMode.MultipleSelectionSameTable)
				{
					switch (keyData)
					{
					case Keys.Down | Keys.Control:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MordexRow otherNextRowOfTable = FindNextRowOfTable(Row, CurrentTable);
							if (otherNextRowOfTable != null && IsRowFocusableInCurrentColumn(otherNextRowOfTable.RowManagerPosition))
							{
								Update();
							}
						}
						return true;
					case Keys.Up | Keys.Control:
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MordexRow previousRowOfTable = FindPreviousRowOfTable(Row, CurrentTable);
							if (previousRowOfTable != null && IsRowFocusableInCurrentColumn(previousRowOfTable.RowManagerPosition))
							{
								Update();
							}
						}
						return true;
					}
				}
			}
			else
			{
				switch (keyData)
				{
				case Keys.End:
				case Keys.Home:
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
					OnKeyDown(e);
					return true;
				case Keys.Tab:
				{
					if (_tabKeyBehavior == TabKeyBehavior.ColumnNavigation)
					{
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							MoveCurrentToNextCell();
						}
						return true;
					}
					EndCellEdit(true);
					Control control = FindForm();
					if (control == null)
					{
						control = base.TopLevelControl;
					}
					if (control != null)
					{
						control.SelectNextControl(this, forward: true, tabStopOnly: true, nested: true, wrap: true);
						return true;
					}
					break;
				}
				case Keys.Return:
					if (Row == -1)
					{
						switch (NewRowEnterKeyBehavior)
						{
						case NewRowEnterKeyBehavior.AddRowAndStayInCurrentCell:
						case NewRowEnterKeyBehavior.AddRowAndMoveToFirstCellInNewRow:
						case NewRowEnterKeyBehavior.AddRowAndMoveToAddedRow:
							base.OnKeyDown(e);
							if (!e.Handled)
							{
								CommitRowOnEnter();
							}
							return true;
						}
					}
					if (_enterKeyBehavior == EnterKeyBehavior.NextRow)
					{
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							CommitRowOnEnter();
						}
						return true;
					}
					if (_enterKeyBehavior != EnterKeyBehavior.NextCell)
					{
						break;
					}
					base.OnKeyDown(e);
					if (!e.Handled)
					{
						if (_row != null && _row.RowType == RowType.NewRecord)
						{
							if (CurrentColumn != null && CurrentColumn == GetLastEditableColumn(_row))
							{
								_ = Row;
								int col = Col;
								if (UpdateData())
								{
									MoveToNewRecord();
									if (Col == col)
									{
										Col = 0;
									}
									SyncCellSelectionWithCurrent();
								}
							}
							else
							{
								MoveCurrentToNextCell();
							}
						}
						else
						{
							MoveCurrentToNextCell();
						}
					}
					return true;
				case Keys.Escape:
					if (_isButtonPressActive)
					{
						EndColumnResize(true);
						return true;
					}
					if (_isCellMousePressed)
					{
						EndCardResize(true);
						return true;
					}
					if (_isHeaderPressed)
					{
						EndHeaderDrag(true);
						return true;
					}
					if (_acceptsEscape && (DataChanged || (!AutoEdit && EditMode == EditMode.EditOn)))
					{
						base.OnKeyDown(e);
						if (!e.Handled)
						{
							CancelEditOnEscape();
						}
						return true;
					}
					break;
				}
			}
			return false;
		}
	}
}
