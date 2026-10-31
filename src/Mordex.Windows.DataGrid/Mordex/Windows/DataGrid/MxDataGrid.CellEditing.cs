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
		private void CancelEditOnEscape()
		{
			if (_row == null || (_row.RowType != RowType.Record && _row.RowType != RowType.NewRecord))
			{
				return;
			}
			if (EditMode == EditMode.EditOn && _currentColumn != null && _row.Cells[_currentColumn].DataChanged)
			{
				bool condition = true;
				foreach (GridCell item in (IEnumerable)_row.Cells)
				{
					if (item.DataChanged && item.Column != _currentColumn)
					{
						condition = false;
						break;
					}
				}
				ColumnActionCancelEventArgs e = new ColumnActionCancelEventArgs(_currentColumn, cancel: false);
				OnCancelingCellEdit(e);
				if (e.Cancel)
				{
					return;
				}
				if (condition)
				{
					RowActionCancelEventArgs rowActionCancelEventArgs = new RowActionCancelEventArgs(_row);
					OnCancelingRowEdit(rowActionCancelEventArgs);
					if (rowActionCancelEventArgs.Cancel)
					{
						return;
					}
				}
				if (EndCellEdit(false))
				{
					if (_row != null && _currentColumn != null)
					{
						_row.Cells[_currentColumn].DataChanged = _cellDataChanged;
					}
					_row.RecalculateDataChanged();
					if (!_row.HasChangedCells())
					{
						int row = Row;
						int col = Col;
						_isDataChanging = true;
						_isEditCancelling = true;
						try
						{
							_row.SetDataChanged(false);
							_row.CancelEdit();
							_row = null;
							SetCurrentRowAndSelect(row);
							Col = col;
							RefreshRowCountAndInvalidate();
						}
						finally
						{
							_isDataChanging = false;
							_isEditCancelling = false;
						}
					}
					else
					{
						InvalidateGridRowLayout(_row);
					}
					InvalidateGridRowLayout(_row);
				}
				OnCellEditCanceled(new ColumnActionEventArgs(_currentColumn));
				if (condition)
				{
					OnRowEditCanceled(new RowActionEventArgs(_row));
				}
			}
			else if (DataChanged)
			{
				RowActionCancelEventArgs otherRowActionCancelEventArgs = new RowActionCancelEventArgs(_row);
				OnCancelingRowEdit(otherRowActionCancelEventArgs);
				if (otherRowActionCancelEventArgs.Cancel)
				{
					return;
				}
				if (_row._isEditing)
				{
					_isDataChanging = true;
					int otherRow = Row;
					try
					{
						_row.SetDataChanged(false);
						_row.CancelEdit();
						_row = null;
						SetCurrentRowAndSelect(otherRow);
						RefreshRowCountAndInvalidate();
					}
					finally
					{
						_isDataChanging = false;
					}
				}
				else if (_row.DataRow != null)
				{
					int nextRow = Row;
					_isDataChanging = true;
					try
					{
						_row.SetDataChanged(false);
						_row.CancelEdit();
						_row = null;
						SetCurrentRowAndSelect(nextRow);
						RefreshRowCountAndInvalidate();
					}
					finally
					{
						_isDataChanging = false;
					}
				}
				DataChanged = false;
				OnRowEditCanceled(new RowActionEventArgs(_row));
			}
			else if (EditMode == EditMode.EditOn && !AutoEdit)
			{
				EditMode = EditMode.EditOff;
			}
			InvalidateTotalAndGroupRows(_row);
		}

		private void HideCellEditorControl()
		{
			if (_gridColumnContainerControl != null)
			{
				_gridColumnContainerControl.NotifyButtonMouseUp();
				if (base.ContainsFocus)
				{
					_gridColumnContainerControl.Hide();
					GridHelper.SelectAndFocus(this);
				}
				else
				{
					_gridColumnContainerControl.Hide();
				}
			}
		}

		private void RepositionCellEditor()
		{
			PositionCellEditorControl(false);
		}

		private void PositionCellEditorControl(bool condition)
		{
			if (_editDelayTimer != null || !_isLayoutValid || _gridColumnContainerControl == null || _row == null || _currentColumn == null)
			{
				return;
			}
			GridGroupLayoutState gridGroupLayoutState = ((_row.RowType != RowType.FilterRow) ? FindCellLayoutItem(_row, _currentColumn) : FindFilterCellLayoutItem(_row, _currentColumn));
			if (gridGroupLayoutState == null)
			{
				return;
			}
			if (condition && gridGroupLayoutState._children != null)
			{
				UpdateCellButtonVisibility(gridGroupLayoutState);
				InvalidateLayoutItem(gridGroupLayoutState);
			}
			Rectangle rectangle = AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
			rectangle = gridGroupLayoutState.GetContentRect(rectangle, RightToLeft == RightToLeft.Yes);
			Rectangle clientRectangle = base.ClientRectangle;
			if (GetEffectiveFrozenColumns() != 0 && gridGroupLayoutState._isHorizontallyScrolled)
			{
				int rowLayoutRightEdge = 0;
				rowLayoutRightEdge = GetRowLayoutRightEdge((MxRowLayoutState)gridGroupLayoutState._gridGroupLayoutStateOwner);
				if (rowLayoutRightEdge != 0)
				{
					clientRectangle = new Rectangle(clientRectangle.Left + rowLayoutRightEdge - 1, clientRectangle.Top, clientRectangle.Width - rowLayoutRightEdge, clientRectangle.Height);
					if (RightToLeft == RightToLeft.Yes)
					{
						clientRectangle.X = base.ClientRectangle.Left;
						clientRectangle.Width += base.ClientRectangle.Left;
					}
				}
			}
			if (_view == View.TableView && Row >= 0 && Row < RowCount)
			{
				clientRectangle.Height = _bodyBottomY - clientRectangle.Top;
			}
			if (!_gridColumnContainerControl.Bounds.IsEmpty)
			{
				Invalidate(_gridColumnContainerControl.Bounds);
			}
			_gridColumnContainerControl.ClipEditBounds(rectangle, clientRectangle);
			_gridColumnContainerControl.ApplyComboVirtualWidth(gridGroupLayoutState.DeflateForGridlines(gridGroupLayoutState._rect, RightToLeft == RightToLeft.Yes).Width);
			if (!_gridColumnContainerControl.Visible)
			{
				_gridColumnContainerControl.Show();
				if (base.ContainsFocus)
				{
					try
					{
						if (_gridColumnContainerControl.GetActiveEditControl() == null)
						{
							base.ActiveControl = _gridColumnContainerControl;
							_gridColumnContainerControl.Focus();
						}
						else
						{
							base.ActiveControl = _gridColumnContainerControl.GetActiveEditControl();
							GridHelper.SelectAndFocus(_gridColumnContainerControl.GetActiveEditControl());
						}
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
			}
			_gridColumnContainerControl.Update();
		}

		private void BeginCurrentRowEdit()
		{
			if (_hasCurrencyManagerHandlers)
			{
				_currencyManager.PositionChanged -= OnCurrencyManagerPositionChanged;
				_currencyManager.ItemChanged -= OnCurrencyManagerItemChanged;
			}
			_row.BeginEdit();
			if (_row.RowType == RowType.NewRecord && _row.DataRow != null && _row.Table == RootTable && _currencyManager != null)
			{
				for (int position = _currencyManager.Count - 1; position >= 0; position--)
				{
					if (_currencyManager.List[position] == _row.DataRow)
					{
						if (_currencyManager.Position != position)
						{
							_currencyManager.Position = position;
						}
						break;
					}
				}
			}
			if (_hasCurrencyManagerHandlers)
			{
				_currencyManager.PositionChanged += OnCurrencyManagerPositionChanged;
				_currencyManager.ItemChanged += OnCurrencyManagerItemChanged;
			}
		}

		private bool CommitCurrentRow()
		{
			try
			{
				if (!AcceptCellEdit())
				{
					return false;
				}
				if (_row.RowType == RowType.FilterRow)
				{
					return ApplyFilterRow(false);
				}
				if (!_row.HasChangedCells())
				{
					_isDataChanging = true;
					_row.CancelEdit();
					_isDataChanging = false;
					return true;
				}
				if (_row.GetLogicalRow() is MordexDataRow || _row.GetLogicalRow() is MordexNewDataRow)
				{
					GridRow gridRow = _row;
					BeginCurrentRowEdit();
					_row = gridRow;
					if (!ConfirmRecordUpdate(gridRow))
					{
						InvalidateGridRowLayout(_row);
						return false;
					}
					if (!DataChanged)
					{
						return true;
					}
					_isRowEditSuppressed = true;
					object dataRow = _row.DataRow;
					if (dataRow is DataRowView)
					{
						dataRow = ((DataRowView)dataRow).Row;
					}
					_isDataChanging = true;
					if (UpdateMode == UpdateMode.RowUpdate)
					{
						foreach (GridCell item in (IEnumerable)_row.Cells)
						{
							if (item.Column.BoundMode != ColumnBoundMode.UnboundFetch)
							{
								if (item.DataChanged)
								{
									item.Column.SetRowValue(_row.GetLogicalRow(), item, item.Value);
								}
								else if (_row.RowType == RowType.NewRecord && item.Column.DefaultValue != null)
								{
									item.Column.SetRowValue(_row.GetLogicalRow(), item, item.Column.DefaultValue);
								}
							}
						}
					}
					else if (_row.RowType == RowType.NewRecord)
					{
						foreach (GridCell cell in (IEnumerable)_row.Cells)
						{
							if (cell.Column.BoundMode != ColumnBoundMode.UnboundFetch && cell.Column.DefaultValue != null && !cell.DataChanged)
							{
								cell.Column.SetRowValue(_row.GetLogicalRow(), cell, cell.Column.DefaultValue);
							}
						}
					}
					GridRow row = _row;
					_row.EndEdit();
					_isRowEditSuppressed = false;
					_isDataChanging = false;
					MordexRow mordexRow = _gridDataSourceManager.FindDataRow(dataRow);
					GridRow orCreateGridRow = ((mordexRow != null) ? GetOrCreateGridRow(mordexRow, true) : null);
					if (orCreateGridRow == null)
					{
						orCreateGridRow = _row;
					}
					_row = orCreateGridRow;
					if (_row != null && !_row.Selected)
					{
						SyncCellSelectionWithCurrent();
					}
					_cachedRow = orCreateGridRow;
					try
					{
						if (row.RowType == RowType.NewRecord)
						{
							OnRecordAdded(EventArgs.Empty);
						}
						else
						{
							OnRecordUpdated(EventArgs.Empty);
						}
					}
					finally
					{
						_cachedRow = null;
					}
				}
			}
			catch (Exception ex)
			{
				_isCurrentCellChanging = false;
				_isDataChanging = false;
				_isRowEditSuppressed = false;
				ProcessException(ex, true);
				return false;
			}
			return true;
		}

		internal bool GetAllowAddNew()
		{
			return _gridDataSourceManager.AllowAddNew;
		}

		internal bool ShouldAllowDelete()
		{
			InheritableBoolean inheritableBoolean = _allowDelete;
			if (inheritableBoolean == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool ShouldAllowEdit()
		{
			InheritableBoolean inheritableBoolean = _allowEdit;
			if (inheritableBoolean == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		protected virtual void OnParseEditValue(ParseEditValueEventArgs e)
		{
			if (base.Events[_parseEditValueEventKey] is ParseEditValueEventHandler parseEditValueEventHandler)
			{
				parseEditValueEventHandler(this, e);
			}
		}

		protected virtual void OnEditModeChanged(EventArgs e)
		{
			if (!_useDefaultMenu && EditTextBox != null)
			{
				EditTextBox.ContextMenuStrip = new ContextMenuStrip();
			}
			if (base.Events[_editModeChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnInputMaskError(InputMaskErrorEventArgs e)
		{
			if (base.Events[_inputMaskErrorEventKey] is InputMaskErrorEventHandler inputMaskErrorEventHandler)
			{
				inputMaskErrorEventHandler(this, e);
			}
		}

		protected virtual void OnCancelingCellEdit(ColumnActionCancelEventArgs e)
		{
			if (base.Events[_eventKeyCancelingCellEdit] is ColumnActionCancelEventHandler columnActionCancelEventHandler)
			{
				columnActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnCellEditCanceled(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyCellEditCanceled] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnCancelingRowEdit(RowActionCancelEventArgs e)
		{
			if (base.Events[_eventKeyCancelingRowEdit] is RowActionCancelEventHandler rowActionCancelEventHandler)
			{
				rowActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnRowEditCanceled(RowActionEventArgs e)
		{
			if (base.Events[_eventKeyRowEditCanceled] is RowActionEventHandler rowActionEventHandler)
			{
				rowActionEventHandler(this, e);
			}
		}

		protected virtual void OnCellEdited(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyCellEdited] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnEditingCell(EditingCellEventArgs e)
		{
			if (base.Events[_eventKeyEditingCell] is EditingCellEventHandler editingCellEventHandler)
			{
				editingCellEventHandler(this, e);
			}
		}

		protected virtual void OnRecordsDeleted(EventArgs e)
		{
			if (base.Events[_eventKeyRecordsDeleted] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnInitCustomEdit(InitCustomEditEventArgs e)
		{
			if (base.Events[_eventKeyInitCustomEdit] is InitCustomEditEventHandler initCustomEditEventHandler)
			{
				initCustomEditEventHandler(this, e);
			}
		}

		protected virtual void OnEndCustomEdit(EndCustomEditEventArgs e)
		{
			if (base.Events[_eventKeyEndCustomEdit] is EndCustomEditEventHandler endCustomEditEventHandler)
			{
				endCustomEditEventHandler(this, e);
			}
		}

		internal void SyncRowCheckToEditor(GridRow gridRow)
		{
			if (_row != null && gridRow.GetLogicalRow() == _row.GetLogicalRow() && _currentColumn != null && _currentColumn.ActAsSelector && _gridColumnContainerControl != null)
			{
				if (RowCheckStateBehavior == RowCheckStateBehavior.CheckStateDependsOnChild)
				{
					_gridColumnContainerControl.WriteEditorValue(gridRow.CheckState);
				}
				else
				{
					_gridColumnContainerControl.WriteEditorValue(gridRow.IsChecked);
				}
			}
			PrepareRowForDisplay(gridRow, true);
			Invalidate();
		}

		protected override bool IsInputChar(char charCode)
		{
			if (char.IsLetterOrDigit(charCode))
			{
				return true;
			}
			return base.IsInputChar(charCode);
		}

		public void CancelCurrentEdit()
		{
			DataChanged = false;
		}

		private static void CopyFontToFormatStyle(GridFormatStyle gridFormatStyle, Font font)
		{
			gridFormatStyle.Font = null;
			if (font.Bold)
			{
				gridFormatStyle.FontBold = TriState.True;
			}
			else
			{
				gridFormatStyle.FontBold = TriState.False;
			}
			if (font.Italic)
			{
				gridFormatStyle.FontItalic = TriState.True;
			}
			else
			{
				gridFormatStyle.FontItalic = TriState.False;
			}
			gridFormatStyle.FontName = font.Name;
			gridFormatStyle.FontSize = font.Size;
			if (font.Strikeout)
			{
				gridFormatStyle.FontStrikeout = TriState.True;
			}
			else
			{
				gridFormatStyle.FontStrikeout = TriState.False;
			}
			if (font.Underline)
			{
				gridFormatStyle.FontUnderline = TriState.True;
			}
			else
			{
				gridFormatStyle.FontUnderline = TriState.False;
			}
		}
	}
}
