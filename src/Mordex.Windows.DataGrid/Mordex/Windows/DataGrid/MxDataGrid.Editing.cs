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
		internal GridColumn GetLastEditableColumn(GridRow gridRow)
		{
			GridTable gridTable = CurrentTable;
			if (gridRow != null)
			{
				gridTable = gridRow.Table;
			}
			int layoutColumnCount = ((gridTable != null && gridTable.CellLayoutMode != CellLayoutMode.UseColumns) ? gridTable.ColumnSets.GetColumnSetColumnCount() : gridTable.Columns.Count);
			for (int position = layoutColumnCount - 1; position >= 0; position--)
			{
				GridColumn columnInPosition = gridTable.Columns.GetColumnInPosition(position);
				if (IsCellSelectable(gridRow, columnInPosition))
				{
					return columnInPosition;
				}
			}
			return null;
		}

		internal static GridColumn GetFirstEditableColumn(GridRow gridRow)
		{
			if (gridRow == null || gridRow.Table == null)
			{
				return null;
			}
			GridColumn[] array = ((gridRow.Table.CellLayoutMode != CellLayoutMode.UseColumns) ? new GridColumn[gridRow.Table.ColumnSets.GetColumnSetColumnCount()] : new GridColumn[gridRow.Table.Columns.Count]);
			foreach (GridColumn column in gridRow.Table.Columns)
			{
				if (column.Position != -1)
				{
					array[column.Position] = column;
				}
			}
			GridColumn[] otherArray = array;
			foreach (GridColumn gridColumn in otherArray)
			{
				if (IsCellSelectable(gridRow, gridColumn))
				{
					return gridColumn;
				}
			}
			return null;
		}

		internal GridSelectedItem CreateSelectedItem(int value)
		{
			if (SelectionMode == SelectionMode.SingleSelection && _gridSelectedItemCollection.Count > 0)
			{
				throw new InvalidOperationException();
			}
			if (!IsValidRowPosition(value))
			{
				throw new ArgumentNullException();
			}
			MordexRow mordexRow = _gridDataSourceManager.GetRowByIndex(value);
			if (SelectionMode == SelectionMode.MultipleSelectionSameTable && !_isSelectingAll && _gridSelectedItemCollection.Count > 0)
			{
				GridSelectedItem gridSelectedItem = _gridSelectedItemCollection[0];
				if (gridSelectedItem.Table != mordexRow.Table)
				{
					throw new InvalidOperationException();
				}
			}
			GridSelectedItem selectedItem = null;
			if (!RowStateHelper.GetRowTag(mordexRow).GetDataState3Boolean())
			{
				selectedItem = new GridSelectedItem(this, mordexRow);
				RowStateHelper.GetRowTag(mordexRow).SetDataState3Boolean(true);
				InvalidateRowLayout(mordexRow);
				_gridSelectedItemCollection.AddItem(selectedItem);
				if (!_isSelectionUpdating)
				{
					RaiseSelectionChangedOrDefer();
				}
			}
			return selectedItem;
		}

		internal void DeselectItem(GridSelectedItem gridSelectedItem)
		{
			RowStateHelper.GetRowTag(gridSelectedItem._row).SetDataState3Boolean(false);
			RowStateHelper.DeselectAllRowCells(gridSelectedItem._row);
			gridSelectedItem._isValid = false;
			InvalidateRowLayout(gridSelectedItem._row);
			if (!_isSelectionUpdating)
			{
				RaiseSelectionChangedOrDefer();
			}
		}

		internal void DeselectAllItems()
		{
			if (_gridSelectedItemCollection.Count == 0)
			{
				return;
			}
			foreach (GridSelectedItem item in (IEnumerable)_gridSelectedItemCollection)
			{
				RowStateHelper.GetRowTag(item._row).SetDataState3Boolean(false);
				RowStateHelper.DeselectAllRowCells(item._row);
				InvalidateRowLayout(item._row);
				item._isValid = false;
			}
			_gridSelectedItemCollection.ClearItems();
			if (!_isSelectionUpdating)
			{
				RaiseSelectionChangedOrDefer();
			}
		}

		private static bool CanEditLayoutItem(GridGroupLayoutState gridGroupLayoutState)
		{
			if (gridGroupLayoutState != null)
			{
				GridCell gridCell = gridGroupLayoutState.GetCell();
				if (gridGroupLayoutState._gridGroupLayoutStateOwner == null || gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow == null)
				{
					return false;
				}
				if (gridCell != null)
				{
					if (!gridCell.Enabled)
					{
						return false;
					}
					if (gridCell.ButtonStyle == ButtonStyle.ButtonCell)
					{
						return false;
					}
				}
				if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.FilterRow)
				{
					if (gridGroupLayoutState._column.EditType == EditType.NoEdit && gridGroupLayoutState._column.FilterEditType == FilterEditType.SameAsEditType)
					{
						return false;
					}
					if (gridGroupLayoutState._column.FilterEditType == FilterEditType.NoEdit)
					{
						return false;
					}
					return true;
				}
				if (gridGroupLayoutState._column.ActAsSelector)
				{
					return true;
				}
				if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.NewRecord)
				{
					if (gridCell != null)
					{
						if (gridCell.EditType != EditType.NoEdit)
						{
							return gridGroupLayoutState.GetOwnerTable().IsAddNewAllowed();
						}
						return false;
					}
					return false;
				}
				if (gridCell != null)
				{
					if (gridCell.EditType != EditType.NoEdit)
					{
						return gridGroupLayoutState.GetOwnerTable().IsEditAllowed();
					}
					return false;
				}
				return false;
			}
			return false;
		}

		private void BeginCellEdit(bool capture, char character, bool condition)
		{
			if (_gridColumnContainerControl != null || _isStartingEdit)
			{
				return;
			}
			_isStartingEdit = true;
			try
			{
				StartCellEdit(capture, character, condition);
			}
			finally
			{
				_isStartingEdit = false;
			}
		}

		private void StartCellEdit(bool capture, char character, bool condition)
		{
			EnsureVisible();
			if (!_isLayoutValid)
			{
				try
				{
					CalculateLayoutIfReady();
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
			GridGroupLayoutState gridGroupLayoutState = FindCellLayoutItem(_row, _currentColumn);
			if (CanEditLayoutItem(gridGroupLayoutState))
			{
				GridCell gridCell = gridGroupLayoutState.GetCell();
				_cellDataChanged = gridCell.DataChanged;
				CommitRowOnCellUpdate();
				bool cancel = false;
				object cellValue = gridGroupLayoutState.GetCellValue();
				if (_currentColumn.MultipleValues && _currentColumn.IsGrouped && !gridCell.DataChanged)
				{
					cellValue = _currentColumn.GetBaseMultipleValuesList(_row.GetLogicalRow());
				}
				if (gridGroupLayoutState._column.EditTarget == EditTarget.Text && (!gridGroupLayoutState._column.HasValueList || !gridGroupLayoutState._column.ValueList.ReplaceValues) && (gridGroupLayoutState._column.DropDown == null || !gridGroupLayoutState._column.DropDown.ReplaceValues) && gridCell.EditType != EditType.CheckBox)
				{
					cellValue = gridGroupLayoutState.GetDisplayText();
				}
				if (_currentColumn != null)
				{
					EditingCellEventArgs e = new EditingCellEventArgs(_currentColumn, cellValue);
					OnEditingCell(e);
					cancel = e.Cancel;
					cellValue = e.Value;
				}
				gridGroupLayoutState = FindCellLayoutItem(_row, _currentColumn);
				if (!CanEditLayoutItem(gridGroupLayoutState))
				{
					return;
				}
				if (!cancel)
				{
					ClearTooltipText();
					_editorHadFocus = base.ContainsFocus;
					_gridGridColumnContainerControl.DestroyEditorState();
					_editorHadFocus = false;
					_gridColumnContainerControl = _gridGridColumnContainerControl;
					_gridColumnContainerControl.RightToLeft = gridGroupLayoutState._column.RightToLeft;
					if (_hasEditorsControlStyle)
					{
						_gridColumnContainerControl.SetEditorStyle(EditorsControlStyle);
					}
					else
					{
						_gridColumnContainerControl.SetEditorStyle(_gridControlStyle);
					}
					if (!base.Controls.Contains(_gridColumnContainerControl))
					{
						base.Controls.Add(_gridColumnContainerControl);
					}
					if (gridGroupLayoutState._column.EditType == EditType.Custom)
					{
						InitCustomEditControl(gridGroupLayoutState, cellValue, character);
					}
					else
					{
						GridFormatStyle gridFormatStyle = BuildCellFormatStyle(gridGroupLayoutState);
						Image image = gridGroupLayoutState.GetDisplayImage(ComputeImageList());
						if (image == null)
						{
							image = GetImageByKey(gridGroupLayoutState.GetImageKey());
						}
						if (image == null)
						{
							image = GetImageByIndex(gridGroupLayoutState.GetImageIndex());
						}
						_gridColumnContainerControl.ConfigureForCell(this, gridGroupLayoutState._column, gridGroupLayoutState.GetCell(), gridFormatStyle, cellValue, image, capture, null);
						if (EditTextBox != null)
						{
							EditTextBox.SelectAll();
						}
						_gridColumnContainerControl.ApplyComboVirtualWidth(gridGroupLayoutState.DeflateForGridlines(gridGroupLayoutState._rect, RightToLeft == RightToLeft.Yes).Width);
					}
					HookCellEditorEvents();
					if (capture && condition)
					{
						StartEditClickTimer();
					}
					else
					{
						PositionCellEditorControl(true);
						if (_gridColumnContainerControl != null && _isFocusHandled)
						{
							if (_gridColumnContainerControl.GetActiveEditControl() != null)
							{
								_gridColumnContainerControl.GetActiveEditControl().Select();
							}
							else
							{
								_gridColumnContainerControl.Select();
							}
						}
					}
				}
			}
			else
			{
				gridGroupLayoutState = FindFilterCellLayoutItem(_row, _currentColumn);
				if (gridGroupLayoutState != null && gridGroupLayoutState._column.FilterEditType != FilterEditType.NoEdit)
				{
					if (gridGroupLayoutState._children != null)
					{
						UpdateCellButtonVisibility(gridGroupLayoutState);
						InvalidateLayoutItem(gridGroupLayoutState);
					}
					if (gridGroupLayoutState._column.FilterEditType != FilterEditType.SameAsEditType || gridGroupLayoutState._column.EditType != EditType.NoEdit)
					{
						_gridGridColumnContainerControl.DestroyEditorState();
						ActivateCellEditorControl(gridGroupLayoutState, character, capture, condition);
						if (_gridColumnContainerControl != null)
						{
							_gridColumnContainerControl.ApplyComboVirtualWidth(gridGroupLayoutState.DeflateForGridlines(gridGroupLayoutState._rect, RightToLeft == RightToLeft.Yes).Width);
						}
					}
				}
			}
			if (capture && _gridColumnContainerControl != null)
			{
				_gridColumnContainerControl.ToggleEditorCheckBox();
			}
			if (_gridColumnContainerControl != null)
			{
				InvalidateGridRowLayout(_row);
				OnEditModeChanged(EventArgs.Empty);
			}
		}

		internal void SetColumnEditText(GridColumn gridColumn, string text)
		{
			EditMode = EditMode.EditOn;
			if (_gridColumnContainerControl != null)
			{
				_gridColumnContainerControl.SetEditorText(text);
			}
			EditMode = EditMode.EditOff;
		}

		private void CommitRowOnCellUpdate()
		{
			if (UpdateMode == UpdateMode.CellUpdate && _row != null && (_row.RowType == RowType.Record || _row.RowType == RowType.NewRecord) && !_row._isEditing)
			{
				BeginCurrentRowEdit();
				_row.ResetNewRecord();
				InvalidateGridRowLayout(_row);
			}
		}

		private void SetCustomEditStyle(GridFormatStyle style)
		{
			ReleaseCustomEditStyle();
			_customEditStyle = style;
		}

		private void ReleaseCustomEditStyle()
		{
			_customEditStyle?.DisposeResources();
			_customEditStyle = null;
		}

		private void InitCustomEditControl(GridGroupLayoutState gridGroupLayoutState, object value, char editChar)
		{
			GridFormatStyle gridFormatStyle = BuildCellFormatStyle(gridGroupLayoutState);
			gridFormatStyle.PinResolvedFont(Font);
			SetCustomEditStyle(gridFormatStyle);
			InitCustomEditEventArgs e = new InitCustomEditEventArgs(_row, gridGroupLayoutState._column, gridFormatStyle, value, editChar);
			OnInitCustomEdit(e);
			_gridColumnContainerControl.ConfigureForCell(this, gridGroupLayoutState._column, gridGroupLayoutState.GetCell(), gridFormatStyle, value, gridGroupLayoutState.GetDisplayImage(ComputeImageList()), false, e.EditControl);
		}

		private void ActivateCellEditorControl(GridGroupLayoutState gridGroupLayoutState, char editChar, bool capture, bool condition)
		{
			if (gridGroupLayoutState._column.ActAsSelector)
			{
				return;
			}
			_gridColumnContainerControl = _gridGridColumnContainerControl;
			if (HasEditorsControlStyle)
			{
				_gridColumnContainerControl.SetEditorStyle(EditorsControlStyle);
			}
			else
			{
				_gridColumnContainerControl.SetEditorStyle(_gridControlStyle);
			}
			base.Controls.Add(_gridColumnContainerControl);
			GridFormatStyle gridFormatStyle = GetFilterRowStyle(null);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridGroupLayoutState._column.CellStyle);
			object cellValue = gridGroupLayoutState.GetCellValue();
			if ((gridGroupLayoutState._column.FilterEditType == FilterEditType.SameAsEditType && gridGroupLayoutState._column.EditType == EditType.Custom) || gridGroupLayoutState._column.FilterEditType == FilterEditType.Custom)
			{
				GridFormatStyle cellFormatStyle = BuildCellFormatStyle(gridGroupLayoutState);
				cellFormatStyle.PinResolvedFont(Font);
				SetCustomEditStyle(cellFormatStyle);
				InitCustomEditEventArgs e = new InitCustomEditEventArgs(_row, gridGroupLayoutState._column, cellFormatStyle, cellValue, editChar);
				OnInitCustomEdit(e);
				_gridColumnContainerControl.ProcessDataGrid(this, gridGroupLayoutState._column, cellFormatStyle, cellValue, _filterMode, false, e.EditControl);
			}
			else
			{
				_gridColumnContainerControl.ProcessDataGrid(this, gridGroupLayoutState._column, applyOverrides, cellValue, _filterMode, capture, null);
			}
			HookCellEditorEvents();
			if (capture && condition)
			{
				StartEditClickTimer();
			}
			else
			{
				PositionCellEditorControl(true);
			}
		}

		internal void ApplyRowCheckFromEditor(bool isChecked)
		{
			if (_isDropDownToggling)
			{
				return;
			}
			RowCheckState checkState = _row.CheckState;
			if (RowCheckStateBehavior == RowCheckStateBehavior.CheckStateDependsOnChild)
			{
				RowCheckState rowCheckState = RowCheckState.Checked;
				if (!isChecked)
				{
					rowCheckState = RowCheckState.Unchecked;
				}
				if (!RaiseRowCheckStateChanging(_row, CheckStateChangeType.RowChange, checkState, rowCheckState))
				{
					_isDropDownToggling = true;
					SyncRowCheckToEditor(_row);
					_isDropDownToggling = false;
					return;
				}
				_row.CheckState = rowCheckState;
				SyncRowCheckToEditor(_row);
			}
			else
			{
				RowCheckState otherCheckState = RowCheckState.Unchecked;
				if (isChecked)
				{
					otherCheckState = RowCheckState.Checked;
				}
				if (!RaiseRowCheckStateChanging(_row, CheckStateChangeType.RowChange, checkState, otherCheckState))
				{
					_isDropDownToggling = true;
					SyncRowCheckToEditor(_row);
					_isDropDownToggling = false;
					return;
				}
				_row.IsChecked = isChecked;
			}
			OnRowCheckStateChanged(new RowCheckStateChangeEventArgs(_row, CheckStateChangeType.RowChange, checkState, _row.CheckState));
		}

		private void OnCellEditorValueChanged(object sender, EventArgs eventArgs)
		{
			if (_currentColumn == null || _row == null || _gridColumnContainerControl == null)
			{
				return;
			}
			if (_currentColumn.ActAsSelector)
			{
				object editorValue = _gridColumnContainerControl.ReadEditorValue();
				bool condition = false;
				if (editorValue is RowCheckState)
				{
					if ((RowCheckState)editorValue == RowCheckState.Checked)
					{
						condition = true;
					}
				}
				else
				{
					condition = (bool)editorValue;
				}
				ApplyRowCheckFromEditor(condition);
			}
			else
			{
				EnsureVisible();
				if (!_row.HasChangedCells())
				{
					if (_row.RowType == RowType.NewRecord && _row.Table.ComputeNewRowPosition() == NewRowPosition.BottomRow)
					{
						((MordexNewDataRow)_row.GetLogicalRow()).AddExtraNewRow();
						Invalidate();
						Update();
						PositionCellEditorControl(true);
					}
					if (_row.RowType != RowType.FilterRow)
					{
						_row.Cells[_currentColumn].DataChanged = true;
						OnFirstChange(new RowActionEventArgs(_row));
					}
					InvalidateGridRowLayout(_row);
				}
				_row.Cells[_currentColumn].DataChanged = true;
			}
			if (_row == null)
			{
				return;
			}
			if (_row.RowType != RowType.FilterRow)
			{
				OnCellValueChanged(new ColumnActionEventArgs(_currentColumn));
			}
			else if (_filterRowUpdateMode == FilterRowUpdateMode.WhenValueChanges)
			{
				if (_filterRowUpdateDelay > 0)
				{
					StopFilterRowTimer();
					StartFilterRowTimer();
				}
				else
				{
					ReapplyFilterRowKeepingCaret();
				}
			}
		}

		public void Delete()
		{
			if (_gridSelectedItemCollection.Count <= 0)
			{
				return;
			}
			long ticks = DateTime.Now.Ticks;
			int row = Row;
			_gridSelectedItemCollection.Sort();
			ArrayList arrayList = new ArrayList(_gridSelectedItemCollection.Count);
			foreach (GridSelectedItem item in (IEnumerable)_gridSelectedItemCollection)
			{
				if (item._row is MordexDataRow && item.Table.IsDeleteAllowed())
				{
					arrayList.Add(item._row);
					if (item.Position >= 0 && item.Position < row)
					{
						row = item.Position;
					}
				}
			}
			if (arrayList.Count == 0)
			{
				return;
			}
			CancelEventArgs e = new CancelEventArgs(cancel: false);
			OnDeletingRecords(e);
			if (e.Cancel)
			{
				return;
			}
			EndCellEdit(false);
			CancelCurrentEdit();
			int value = 0;
			_isSelectionUpdating = true;
			_isSelectionNotifyEnabled = false;
			try
			{
				arrayList.Reverse();
				if (_hasCurrencyManagerHandlers)
				{
					_currencyManager.PositionChanged -= OnCurrencyManagerPositionChanged;
				}
				for (int i = 0; i < arrayList.Count; i++)
				{
					MordexDataRow mordexDataRow = (MordexDataRow)arrayList[i];
					if (mordexDataRow.CanDelete())
					{
						GridRow gridRow = GetGridRowForLogicalRow(mordexDataRow);
						gridRow.SetDataChanged(false);
						RowActionCancelEventArgs rowActionCancelEventArgs = new RowActionCancelEventArgs(gridRow);
						OnDeletingRecord(rowActionCancelEventArgs);
						if (!rowActionCancelEventArgs.Cancel)
						{
							mordexDataRow.Delete();
							value++;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ProcessException(ex, true);
			}
			finally
			{
				ticks = DateTime.Now.Ticks - ticks;
				if (row < RecordCount)
				{
					_lastRecordPosition = row;
				}
				else
				{
					_lastRecordPosition = row - 1;
				}
				if (_hasCurrencyManagerHandlers)
				{
					_currencyManager.PositionChanged += OnCurrencyManagerPositionChanged;
				}
				_isSelectionUpdating = false;
				if (row < RowCount)
				{
					SetCurrentRowAndSelect(row);
				}
				else
				{
					SetCurrentRowAndSelect(row - 1);
				}
			}
			_isSelectionNotifyEnabled = true;
			if (value > 0)
			{
				OnRecordsDeleted(EventArgs.Empty);
				EnsureVisible();
				RaiseSelectionChangedOrDefer();
			}
			Refresh();
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			EditMode editMode = EditMode;
			if (!AcceptCellEdit())
			{
				e.Cancel = true;
			}
			if (!e.Cancel && _updateOnLeave)
			{
				if (DataChanged)
				{
					if (!UpdateData())
					{
						EditMode = editMode;
						e.Cancel = true;
					}
				}
				else if (_row != null && _row.RowType == RowType.FilterRow)
				{
					UpdateData();
				}
			}
			base.OnValidating(e);
			if (!e.Cancel)
			{
				EndCellEdit(false);
			}
		}

		private bool AcceptCellEdit()
		{
			return EndCellEdit(true);
		}

		private bool EndCellEdit(bool condition)
		{
			if (_gridColumnContainerControl != null && _gridGridColumn == null)
			{
				GridColumnContainerControl gridColumnContainerControl = (_previousColumnContainerControl = _gridColumnContainerControl);
				_gridColumnContainerControl = null;
				if (_row != null && _row.GetLogicalRow() != null && _currentColumn != null)
				{
					gridColumnContainerControl.SetDroppedDown(false);
					GridColumn gridColumn = _currentColumn;
					object editorValue = null;
					bool dataChanged = false;
					bool otherCondition = false;
					Exception ex = null;
					if (CurrentColumn.ActAsSelector)
					{
						condition = false;
					}
					if (gridColumnContainerControl.GetCurrentEditType() == EditType.Custom)
					{
						EndCustomEditEventArgs e = new EndCustomEditEventArgs(_row, gridColumnContainerControl.GetColumn(), gridColumnContainerControl.GetActiveEditControl(), gridColumnContainerControl.GetOriginalValue());
						e.CancelUpdate = !condition;
						OnEndCustomEdit(e);
						if (e.DataChanged)
						{
							_gridColumnContainerControl = gridColumnContainerControl;
							OnCellEditorValueChanged(gridColumnContainerControl, EventArgs.Empty);
							_gridColumnContainerControl = null;
						}
						if (e.CancelUpdate && condition)
						{
							_gridColumnContainerControl = gridColumnContainerControl;
							_previousColumnContainerControl = null;
							GridHelper.SelectAndFocus(_gridColumnContainerControl);
							return false;
						}
						dataChanged = e.DataChanged;
						editorValue = e.Value;
					}
					else if (gridColumnContainerControl.IsModified() && condition)
					{
						bool nextCondition = false;
						if (condition)
						{
							MaskEdit maskEdit = gridColumnContainerControl.GetEditorMaskEdit();
							if (maskEdit != null && !maskEdit.IsTextValid())
							{
								InputMaskErrorEventArgs inputMaskErrorEventArgs = new InputMaskErrorEventArgs(gridColumnContainerControl.GetColumn(), maskEdit.Text, InputMaskErrorAction.CancelUpdate);
								OnInputMaskError(inputMaskErrorEventArgs);
								switch (inputMaskErrorEventArgs.Action)
								{
								case InputMaskErrorAction.CancelUpdate:
									_gridColumnContainerControl = gridColumnContainerControl;
									GridHelper.SelectAndFocus(_gridColumnContainerControl);
									return false;
								case InputMaskErrorAction.DiscardChanges:
									condition = false;
									break;
								case InputMaskErrorAction.ContinueUpdate:
									editorValue = inputMaskErrorEventArgs.Text;
									nextCondition = true;
									break;
								}
							}
						}
						if (condition && gridColumnContainerControl.GetColumn().LimitToList && gridColumnContainerControl.IsNotInList() && _row.RowType != RowType.FilterRow)
						{
							if (gridColumnContainerControl.GetColumn().MultipleValues)
							{
								if (gridColumnContainerControl.ReadEditorValue() is IList)
								{
									IList list = (IList)gridColumnContainerControl.ReadEditorValue();
									ILookUpTable lookUpTable = null;
									lookUpTable = ((!gridColumnContainerControl.GetColumn().HasValueList) ? ((ILookUpTable)gridColumnContainerControl.GetColumn().DropDown) : ((ILookUpTable)((gridColumnContainerControl.GetColumn().EditValueList == null) ? gridColumnContainerControl.GetColumn().ValueList : gridColumnContainerControl.GetColumn().EditValueList)));
									foreach (object item in list)
									{
										string text = string.Concat(item);
										if (lookUpTable != null && lookUpTable.GetItemFromValue(item) == null)
										{
											NotInListEventArgs notInListEventArgs = new NotInListEventArgs(gridColumnContainerControl.GetColumn(), text, cancel: true);
											OnNotInList(notInListEventArgs);
											if (notInListEventArgs.Cancel)
											{
												_gridColumnContainerControl = gridColumnContainerControl;
												_previousColumnContainerControl = null;
												GridHelper.SelectAndFocus(_gridColumnContainerControl);
												return false;
											}
											gridColumnContainerControl.ReevaluateEditorValue();
										}
									}
								}
							}
							else
							{
								string toStringCore = "";
								if (gridColumnContainerControl.ReadEditorValue() != null)
								{
									toStringCore = GridHelper.ToSafeString(gridColumnContainerControl.ReadEditorValue());
								}
								NotInListEventArgs otherNotInListEventArgs = new NotInListEventArgs(gridColumnContainerControl.GetColumn(), toStringCore, cancel: true);
								OnNotInList(otherNotInListEventArgs);
								if (otherNotInListEventArgs.Cancel)
								{
									_gridColumnContainerControl = gridColumnContainerControl;
									_previousColumnContainerControl = null;
									_gridColumnContainerControl.Select();
									return false;
								}
								gridColumnContainerControl.ReevaluateEditorValue();
							}
						}
						if (!nextCondition)
						{
							editorValue = gridColumnContainerControl.ReadEditorValue();
							if (gridColumnContainerControl.ComputeTextBox() is NumericEdit numericEdit && numericEdit.IsTextInvalid())
							{
								editorValue = numericEdit.Text;
							}
							if (_row.RowType == RowType.FilterRow && gridColumnContainerControl.GetColumn().CompareTarget == ColumnCompareTarget.Text && !gridColumnContainerControl.GetColumn().MultipleValues && gridColumnContainerControl.GetActiveEditControl() != null)
							{
								editorValue = gridColumnContainerControl.GetActiveEditControl().Text;
							}
						}
						object parseExact = null;
						try
						{
							Type type = gridColumnContainerControl.GetColumn().Type;
							if (_row.RowType == RowType.FilterRow && gridColumnContainerControl.GetColumn().CompareTarget == ColumnCompareTarget.Text)
							{
								type = typeof(string);
							}
							if (!gridColumnContainerControl.GetColumn().MultipleValues)
							{
								if (editorValue == null || editorValue == DBNull.Value || editorValue.GetType() == type)
								{
									parseExact = ((editorValue != null || _row.RowType == RowType.FilterRow) ? editorValue : gridColumnContainerControl.GetColumn().GetEmptyValue(_row.DataRow));
								}
								else if (editorValue is string strA)
								{
									if (strA.Length == 0)
									{
										parseExact = ((_row.RowType == RowType.FilterRow) ? null : gridColumnContainerControl.GetColumn().GetEmptyValue(_row.DataRow));
									}
									else if (type == typeof(DateTime))
									{
										try
										{
											parseExact = DateTime.ParseExact((string)editorValue, gridColumnContainerControl.GetColumn().FormatString, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
										}
										catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
										{
											GridExceptions.Report(caught);
											parseExact = DateTime.Parse((string)editorValue, null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
										}
									}
									else
									{
										parseExact = GridHelper.ParseValue(strA, type, GridHelper.GetCurrentCulture());
									}
								}
								else if (type.IsAssignableFrom(editorValue.GetType()))
								{
									parseExact = editorValue;
								}
								else
								{
									parseExact = GridHelper.ConvertUsingTypeConverter(editorValue, type);
									if (parseExact == null && editorValue != null)
									{
										parseExact = Convert.ChangeType(editorValue, type);
									}
								}
							}
							else
							{
								parseExact = editorValue;
							}
						}
						catch (Exception exception)
						{
							ex = exception;
						}
						if (parseExact == null && _row.RowType != RowType.FilterRow)
						{
							parseExact = CurrentColumn.GetEmptyValue(_row.DataRow);
						}
						ParseEditValueEventArgs parseEditValueEventArgs = new ParseEditValueEventArgs(_row, gridColumnContainerControl.GetColumn(), parseExact, editorValue);
						parseEditValueEventArgs.ParseException = ex;
						OnParseEditValue(parseEditValueEventArgs);
						editorValue = parseEditValueEventArgs.ConvertedValue;
						ex = parseEditValueEventArgs.ParseException;
						dataChanged = true;
					}
					if (dataChanged && condition)
					{
						if (editorValue is string otherText && otherText.Length == 0)
						{
							editorValue = gridColumn.GetEmptyValue(_row.DataRow);
						}
						UpdatingCellEventArgs updatingCellEventArgs = new UpdatingCellEventArgs(gridColumnContainerControl.GetColumn(), gridColumnContainerControl.GetOriginalValue(), editorValue);
						if (_row.RowType != RowType.FilterRow)
						{
							_updatingCellEventArgs = updatingCellEventArgs;
							OnUpdatingCell(updatingCellEventArgs);
							_updatingCellEventArgs = null;
							if (updatingCellEventArgs.Value != null && !updatingCellEventArgs.Value.Equals(editorValue))
							{
								ex = null;
							}
						}
						if (!updatingCellEventArgs.Cancel)
						{
							if (_updateMode == UpdateMode.CellUpdate && _row.RowType != RowType.FilterRow)
							{
								if (ex == null)
								{
									try
									{
										if (!_row._isEditing && (_row.RowType == RowType.Record || _row.RowType == RowType.NewRecord))
										{
											BeginCurrentRowEdit();
											_row.ResetNewRecord();
										}
										if (gridColumn.BoundMode != ColumnBoundMode.UnboundFetch)
										{
											_row.Cells[gridColumn].Value = updatingCellEventArgs.Value;
											_row.Cells[gridColumn.Index].AssignValue(_row.Cells[gridColumn].Value, true);
											_row.CommitCells();
											_row.Cells[gridColumn.Index].ClearValueAndDisplayCache();
										}
										else
										{
											_row.Cells[gridColumn.Index].AssignValue(updatingCellEventArgs.Value, true);
										}
									}
									catch (Exception otherException)
									{
										ex = otherException;
									}
								}
								if (ex != null && gridColumn != null)
								{
									switch (gridColumn.ComputeInvalidValueAction())
									{
									case InvalidValueAction.CancelUpdate:
										ProcessException(ex, false);
										_gridColumnContainerControl = gridColumnContainerControl;
										_previousColumnContainerControl = null;
										return false;
									case InvalidValueAction.CancelUpdateAndShowErrorMessage:
										ProcessException(ex, true);
										_gridColumnContainerControl = gridColumnContainerControl;
										_previousColumnContainerControl = null;
										return false;
									case InvalidValueAction.DiscardChangesAndShowErrorMessage:
										ProcessException(ex, true);
										break;
									case InvalidValueAction.DiscardChanges:
										ProcessException(ex, false);
										break;
									}
								}
							}
							else
							{
								if (ex != null && _updateMode == UpdateMode.RowUpdate)
								{
									bool previousCondition = false;
									switch (gridColumn.ComputeInvalidValueAction())
									{
									case InvalidValueAction.CancelUpdate:
										ProcessException(ex, false);
										previousCondition = true;
										break;
									case InvalidValueAction.CancelUpdateAndShowErrorMessage:
										ProcessException(ex, true);
										previousCondition = true;
										break;
									case InvalidValueAction.DiscardChangesAndShowErrorMessage:
									case InvalidValueAction.ContinueAndShowErrorMessage:
										ProcessException(ex, true);
										break;
									case InvalidValueAction.DiscardChanges:
									case InvalidValueAction.Continue:
										ProcessException(ex, false);
										break;
									}
									if (previousCondition && gridColumnContainerControl != null)
									{
										try
										{
											gridColumnContainerControl.WriteEditorValue(updatingCellEventArgs.Value);
										}
										catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
										{
											GridExceptions.Report(caught);
										}
										_gridColumnContainerControl = gridColumnContainerControl;
										_previousColumnContainerControl = null;
										GridHelper.SelectAndFocus(_gridColumnContainerControl);
										return false;
									}
									otherCondition = gridColumnContainerControl.GetColumn().ComputeInvalidValueAction() == InvalidValueAction.DiscardChanges || gridColumnContainerControl.GetColumn().ComputeInvalidValueAction() == InvalidValueAction.DiscardChangesAndShowErrorMessage;
								}
								if (!otherCondition || (_row.RowType == RowType.FilterRow && _filterMode == FilterMode.Manual))
								{
									_row.Cells[gridColumn.Index].AssignValue(updatingCellEventArgs.Value, true);
								}
							}
							EnsureCellAccessibleObject(_row.Cells[gridColumn.Index]);
							if (_row.RowType != RowType.FilterRow)
							{
								OnCellUpdated(new ColumnActionEventArgs(gridColumnContainerControl.GetColumn()));
							}
						}
						else if (gridColumnContainerControl != null)
						{
							try
							{
								gridColumnContainerControl.WriteEditorValue(updatingCellEventArgs.Value);
							}
							catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
							{
								GridExceptions.Report(caught);
							}
							_gridColumnContainerControl = gridColumnContainerControl;
							_previousColumnContainerControl = null;
							InvalidateGridRowLayout(_row);
							return false;
						}
					}
				}
				if (gridColumnContainerControl == null)
				{
					return true;
				}
				_gridColumnContainerControl = null;
				if (gridColumnContainerControl.GetCurrentEditType() == EditType.Custom)
				{
					gridColumnContainerControl.EndEditorCustomEdit();
					ReleaseCustomEditStyle();
				}
				if (_row != null && _row.RowType != RowType.FilterRow)
				{
					OnCellEdited(new ColumnActionEventArgs(gridColumnContainerControl.GetColumn()));
				}
				UnhookCellEditorEvents(gridColumnContainerControl);
				if (base.ContainsFocus)
				{
					gridColumnContainerControl.Hide();
					GridHelper.SelectAndFocus(this);
				}
				else
				{
					gridColumnContainerControl.Hide();
				}
				gridColumnContainerControl = null;
				_previousColumnContainerControl = null;
				if (_gridColumnContainerControl == null)
				{
					OnEditModeChanged(EventArgs.Empty);
				}
			}
			return true;
		}

		private void UnhookCellEditorEvents(GridColumnContainerControl gridColumnContainerControl)
		{
			if (gridColumnContainerControl != null)
			{
				gridColumnContainerControl.GotFocus -= OnStateGotFocus;
				gridColumnContainerControl.LostFocus -= OnStateLostFocus;
				gridColumnContainerControl.MouseWheel -= OnStateMouseWheel;
				gridColumnContainerControl.RemoveEditValueListHandler(OnCellEditorDropDownShown);
				gridColumnContainerControl.RemoveEditorCallback(OnCellEditorDropDownHidden);
				gridColumnContainerControl.RemoveEditorEventHandler(OnCellEditorValueChanged);
				gridColumnContainerControl.KeyPress -= OnStateKeyPress;
				gridColumnContainerControl.KeyDown -= OnStateKeyDown;
				gridColumnContainerControl.KeyUp -= OnStateKeyUp;
				gridColumnContainerControl.MouseDown -= OnStateMouseDown;
				gridColumnContainerControl.MouseUp -= OnStateMouseUp;
				gridColumnContainerControl.MouseMove -= OnStateMouseMove;
				gridColumnContainerControl.MouseClick -= OnStateMouseClick;
				gridColumnContainerControl.MouseDoubleClick -= OnStateMouseDoubleClick;
				gridColumnContainerControl.Click -= OnStateClick;
				gridColumnContainerControl.DoubleClick -= OnStateDoubleClick;
			}
		}

		private void HookCellEditorEvents()
		{
			if (_gridColumnContainerControl != null)
			{
				_gridColumnContainerControl.GotFocus += OnStateGotFocus;
				_gridColumnContainerControl.LostFocus += OnStateLostFocus;
				_gridColumnContainerControl.MouseWheel += OnStateMouseWheel;
				_gridColumnContainerControl.AddEditValueListHandler(OnCellEditorDropDownShown);
				_gridColumnContainerControl.AddEditorCallback(OnCellEditorDropDownHidden);
				_gridColumnContainerControl.AddEditorEventHandler(OnCellEditorValueChanged);
				_gridColumnContainerControl.KeyPress += OnStateKeyPress;
				_gridColumnContainerControl.KeyDown += OnStateKeyDown;
				_gridColumnContainerControl.KeyUp += OnStateKeyUp;
				_gridColumnContainerControl.MouseDown += OnStateMouseDown;
				_gridColumnContainerControl.MouseUp += OnStateMouseUp;
				_gridColumnContainerControl.MouseClick += OnStateMouseClick;
				_gridColumnContainerControl.MouseDoubleClick += OnStateMouseDoubleClick;
				_gridColumnContainerControl.MouseMove += OnStateMouseMove;
				_gridColumnContainerControl.Click += OnStateClick;
				_gridColumnContainerControl.DoubleClick += OnStateDoubleClick;
			}
		}

		private void CommitRowOnEnter()
		{
			if (_row == null)
			{
				return;
			}
			switch (_row.RowType)
			{
			case RowType.FilterRow:
				AcceptCellEdit();
				ApplyFilterRow(true);
				break;
			case RowType.Record:
			{
				int col = Col;
				GridRow gridRow = null;
				if (Row < RowCount - 1)
				{
					gridRow = GetRow(Row + 1);
				}
				if (!UpdateData())
				{
					break;
				}
				if (_view == View.TableView)
				{
					if (gridRow != null && gridRow.GetLogicalRow() != null && gridRow.GetLogicalRow().DataSourceManager != null)
					{
						MoveTo(gridRow);
					}
					else
					{
						MoveCurrentToNextRow(true);
					}
					Col = col;
					Update();
				}
				else if (col == -1 && ExpandableCards)
				{
					_row.Expanded = !_row.Expanded;
				}
				else
				{
					MoveCurrentToNextRow(true);
					Col = col;
					Update();
				}
				break;
			}
			case RowType.NewRecord:
			{
				if (!AcceptCellEdit() || !DataChanged)
				{
					break;
				}
				if (Row != -1)
				{
					_ = Col;
					if (UpdateData() && _row != null && _row.Table.ComputeNewRowPosition() == NewRowPosition.BottomRow)
					{
						int otherRow = Row + 1;
						if (IsValidRowPosition(otherRow))
						{
							Row = otherRow;
						}
						else
						{
							MoveLast();
						}
					}
					break;
				}
				if (_row != null)
				{
					BeginCurrentRowEdit();
				}
				object dataRow = _row.DataRow;
				if (dataRow is DataRowView)
				{
					dataRow = ((DataRowView)dataRow).Row;
				}
				if (UpdateData())
				{
					switch (_newRowEnterKeyBehavior)
					{
					case NewRowEnterKeyBehavior.Default:
					case NewRowEnterKeyBehavior.AddRowAndStayInCurrentCell:
					case NewRowEnterKeyBehavior.AddRowAndMoveToFirstCellInNewRow:
					{
						GridRow row = GetRow(dataRow);
						if (row != null)
						{
							MordexRow mordexRow = row.GetLogicalRow();
							bool dataState3Boolean = RowStateHelper.GetRowTag(mordexRow).GetDataState3Boolean();
							if (!dataState3Boolean)
							{
								mordexRow.EnsureVisible();
								EnsureVisible(mordexRow.RowManagerPosition);
								RowStateHelper.GetRowTag(mordexRow).SetDataState3Boolean(true);
								Invalidate();
								Update();
								RowStateHelper.GetRowTag(mordexRow).SetDataState3Boolean(dataState3Boolean);
								Invalidate();
								Update();
							}
						}
						MoveToNewRecord();
						if (_newRowEnterKeyBehavior == NewRowEnterKeyBehavior.AddRowAndMoveToFirstCellInNewRow)
						{
							Col = 0;
						}
						break;
					}
					case NewRowEnterKeyBehavior.AddRowAndMoveToAddedRow:
					{
						GridRow row = GetRow(dataRow);
						if (row != null)
						{
							MoveTo(row);
						}
						break;
					}
					}
				}
				if (_autoEdit)
				{
					BeginCellEdit(false, '\0', false);
				}
				break;
			}
			case RowType.GroupHeader:
				ToggleRowExpanded(_row.GetLogicalRow());
				break;
			case RowType.GroupFooter:
			case RowType.TotalRow:
				break;
			}
		}
	}
}
