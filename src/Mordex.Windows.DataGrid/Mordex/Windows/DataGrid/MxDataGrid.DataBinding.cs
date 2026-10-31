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
		internal CurrencyManager GetCurrencyManager()
		{
			return _currencyManager;
		}

		internal void ProcessCurrencyManager(CurrencyManager currencyManager)
		{
			bool condition = false;
			if (_currencyManager == currencyManager)
			{
				return;
			}
			if (_currencyManager != null && _hasCurrencyManagerHandlers)
			{
				_currencyManager.PositionChanged -= OnCurrencyManagerPositionChanged;
				_currencyManager.ItemChanged -= OnCurrencyManagerItemChanged;
				_hasCurrencyManagerHandlers = false;
			}
			_currencyManager = currencyManager;
			if (_currencyManager != null)
			{
				if (BoundMode == BoundMode.Bound)
				{
					_isBuildingBoundData = true;
					_gridDataSourceManager._isUpdatingFromManager = true;
					_gridDataSourceManager.BindableRootList = new DataBindableList(_currencyManager);
					_gridDataSourceManager._isUpdatingFromManager = false;
					_isBuildingBoundData = false;
					_currencyManager.PositionChanged += OnCurrencyManagerPositionChanged;
					_currencyManager.ItemChanged += OnCurrencyManagerItemChanged;
					_hasCurrencyManagerHandlers = true;
					condition = true;
				}
			}
			else
			{
				_gridDataSourceManager.BindableRootList = null;
			}
			ClearLayoutStates();
			if (_numericEditControl != null)
			{
				_numericEditControl.Enabled = _currencyManager != null;
			}
			if (condition)
			{
				OnCurrencyManagerBound();
			}
		}

		private void OnCurrencyManagerBound()
		{
		}

		private void OnCurrencyManagerItemChanged(object sender, ItemChangedEventArgs eventArgs)
		{
			try
			{
				if (eventArgs.Index == -1)
				{
					RunOnUiThread(RebindListIfChanged);
				}
				else
				{
					int changedIndex = eventArgs.Index;
					RunOnUiThread(() => CheckIndexSuspended(changedIndex));
				}
			}
			catch (InvalidOperationException)
			{
				// The bound list changed while the grid was updating; the next change notification refreshes it.
			}
		}

		private void RebindListIfChanged()
		{
			if (_gridDataSourceManager.RootList != GetCurrencyManager().List)
			{
				_gridDataSourceManager.BindableRootList = new DataBindableList(GetCurrencyManager());
				ClearLayoutStates();
				RefreshRowCountAndInvalidate();
			}
			else if (_row != null)
			{
				_ = _row.RowType;
				_ = 1;
			}
		}

		private void OnDataSourceDisposed(object sender, EventArgs eventArgs)
		{
			CancelCurrentEdit();
			SetDataSourceAndMember(null, "", true);
		}

		internal void SetDataSourceAndMember(object dataSource, string dataMember, bool condition)
		{
			if ((_isInitializing && !_isCreatingBindingContext) || _bindingContext == null)
			{
				_dataSource = dataSource;
				_dataMember = dataMember;
				return;
			}
			_gridDataSourceManager.UseSampleRows = base.DesignMode;
			if (_gridValueListItemCollectionScrollableBase != null)
			{
				((IGridDropListOwner)this).OnDropListHide(cancel: true);
			}
			EndCellEdit(true);
			EndCellEdit(false);
			if (!UpdateData())
			{
				CancelCurrentEdit();
			}
			if (BoundMode == BoundMode.Bound)
			{
				_dataSource = dataSource;
				_dataMember = dataMember;
				if (dataSource == null)
				{
					ProcessCurrencyManager(null);
				}
				else
				{
					CurrencyManager currencyManager = null;
					try
					{
						currencyManager = (CurrencyManager)_bindingContext[dataSource, dataMember];
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						if (dataMember == "")
						{
							SetDataBinding(null, "");
						}
						else
						{
							SetDataBinding(dataSource, "");
						}
						return;
					}
					ProcessCurrencyManager(currencyManager);
				}
			}
			else
			{
				ProcessCurrencyManager((CurrencyManager)_bindingContext[_gridDataSourceManager.RootList]);
			}
			if (base.DesignMode)
			{
				ExpandRecords();
			}
		}

		internal BindingContext GetBindingContext()
		{
			return _bindingContext;
		}

		internal void UpdateBindingContext(BindingContext bindingContext)
		{
			if (_bindingContext == bindingContext)
			{
				return;
			}
			_bindingContext = bindingContext;
			if (_bindingContext == null)
			{
				ProcessCurrencyManager(null);
			}
			else
			{
				SetDataSourceAndMember(_dataSource, _dataMember, true);
			}
			foreach (GridDropDown item in _dropDowns)
			{
				item.UpdateBindingContext(_bindingContext);
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);
			UpdateBindingContext(BindingContext);
		}

		public void SetDataBinding(object dataSource, string dataMember)
		{
			SetDataSourceAndMember(dataSource, dataMember, true);
			RefreshRowCountAndInvalidate();
		}

		internal void ReplaceRootTable(GridTable gridTable, bool condition)
		{
			if (_gridDataSourceManager.RootTable != gridTable)
			{
				if (RootTable != null)
				{
					RootTable.UpdateDataGrid(null);
				}
				_gridTableCollection = CreateTableCollection(gridTable);
				_gridDataSourceManager.RootTable = gridTable;
				gridTable?.UpdateDataGrid(this);
			}
			ResetAfterRootTableChange(condition);
		}

		internal void ResetAfterRootTableChange(bool condition)
		{
			_gridRow = null;
			ClearLayoutStates();
			RebuildTableCollection();
			MarkTablesForAutoResize();
			_horizontalScrollOffset = 0;
			_verticalScrollOffset = 0;
			if (_gridFieldChooser != null)
			{
				if (_view == View.TableView)
				{
					ShowFieldChooser();
				}
				else
				{
					HideFieldChooser();
				}
			}
			if (condition)
			{
				OnRootTableChanged(EventArgs.Empty);
			}
		}

		internal ArrayList ComputeArrayList(MxDataGridPrintDocument gridPrintDocument)
		{
			GetRowManager();
			bool condition = false;
			if (gridPrintDocument.PrinterSettings.PrintRange == PrintRange.Selection)
			{
				condition = true;
			}
			ArrayList arrayList = ((gridPrintDocument.PrintHierarchical && _view == View.TableView && Hierarchical) ? _gridDataSourceManager.CollectRootChildRows(gridPrintDocument.PrintCollapsedRows, condition) : ((_view != View.TableView) ? _gridDataSourceManager.ComputeArrayList(false, condition) : _gridDataSourceManager.ComputeArrayList(gridPrintDocument.PrintCollapsedRows, condition)));
			if (_view == View.TableView && _totalRowPosition == TotalRowPosition.BottomFixed && _gridDataSourceManager.GetRootTotalRow() != null)
			{
				arrayList.Add(_gridDataSourceManager.GetRootTotalRow());
			}
			return arrayList;
		}

		private GridGroupLayoutState BuildGroupByBoxHeaderItem(Graphics graphics, GridGroup gridGroup, GridFormatStyle formatStyle)
		{
			GridFormatStyle gridFormatStyle = ((gridGroup.Column == null) ? new GridFormatStyle(formatStyle) : formatStyle.ApplyOverrides(gridGroup.Column.HeaderStyle));
			GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxHeader, null, gridGroup.Column);
			gridGroupLayoutState._gridGroup = gridGroup;
			gridGroupLayoutState._isHorizontallyScrolled = false;
			Image image = gridGroupLayoutState.GetDisplayImage(ComputeImageList());
			if (image == null)
			{
				image = GetImageByKey(gridGroupLayoutState.GetImageKey());
			}
			if (image == null)
			{
				image = GetImageByIndex(gridGroupLayoutState.GetImageIndex());
			}
			int height = (int)gridFormatStyle.ResolveFont(Font).GetHeight(graphics);
			if (image != null && height < image.Height)
			{
				height = image.Height;
			}
			int width = 0;
			if (gridGroup.Table.HeaderLines > 1)
			{
				height = height * gridGroup.Table.HeaderLines + 2;
			}
			height += 5;
			if (gridGroup.HeaderCaption.Length > 0)
			{
				width = (int)graphics.MeasureString(gridGroup.HeaderCaption, gridFormatStyle.ResolveFont(Font), new SizeF(0f, height)).Width;
			}
			if (image != null)
			{
				width += image.Width + 4;
			}
			width += 32;
			gridGroupLayoutState._rect = new Rectangle(0, 0, width, height);
			gridGroupLayoutState._leftMargin = 1;
			gridGroupLayoutState._rightMargin = 1;
			gridGroupLayoutState._topMargin = 1;
			if (gridFormatStyle != formatStyle)
			{
				gridFormatStyle.DisposeResources();
			}
			return gridGroupLayoutState;
		}

		internal GridFormatStyle GetTableHeaderStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = GetHeaderStyle(gridTable);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(_tableHeaderFormatStyle);
			if (gridTable != null && !gridTable.TableHeaderFormatStyle.IsDefaultStyle())
			{
				GridFormatStyle otherApplyOverrides = gridFormatStyle.ApplyOverrides(gridTable.TableHeaderFormatStyle);
				if (otherApplyOverrides != applyOverrides)
				{
					applyOverrides.DisposeResources();
				}
				applyOverrides = otherApplyOverrides;
			}
			if (applyOverrides != gridFormatStyle)
			{
				gridFormatStyle.DisposeResources();
			}
			return applyOverrides;
		}

		private void ProcessIDataErrorInfo(IDataErrorInfo iDataErrorInfo, GridCell gridCell)
		{
			if (gridCell == null || iDataErrorInfo == null || gridCell.Column == null || gridCell.Column.BoundMode != ColumnBoundMode.Bound)
			{
				return;
			}
			string text = "";
			try
			{
				if (gridCell.Column.BoundMode == ColumnBoundMode.Bound && gridCell.Column.DataMember.Length > 0 && !gridCell.Column.MultipleValues && gridCell.Column.GetBasePropertyDescriptor() != null)
				{
					text = iDataErrorInfo[gridCell.Column.DataMember];
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
			if (text != null && text.Length > 0)
			{
				gridCell.SetErrorText(text);
				gridCell.Image = GetErrorImage();
				gridCell.SetHasErrorIcon(gridCell.Image != null);
				return;
			}
			gridCell.SetErrorText("");
			if (gridCell.HasErrorIcon())
			{
				gridCell.Image = null;
				gridCell.SetHasErrorIcon(false);
			}
		}

		internal void NotifyRefreshListeners()
		{
			if (_isInitializing || _refreshListeners == null)
			{
				return;
			}
			foreach (IRefreshListener item in _refreshListeners)
			{
				item.Refresh();
			}
		}

		private void UpdateDataGrid()
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
			if (gridValueStateInteger == _gridGridValueState._currentLinePosition)
			{
				return;
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
			int value = ((RightToLeft != RightToLeft.Yes) ? ((gridValueStateInteger - _cardSpacing / 2) / (_gridGridValueState._resizeCardIndex + 1) - CardSpacing) : ((base.ClientRectangle.Right - gridValueStateInteger - _cardSpacing / 2) / (_gridGridValueState._resizeCardIndex + 1) - CardSpacing));
			UpdateRectangleArray(value);
			_gridGridValueState._currentLinePosition = gridValueStateInteger;
			Rectangle[] cardBounds = _cardBounds;
			foreach (Rectangle otherRectangle in cardBounds)
			{
				Rectangle otherAdjustRectangleToViewport = AdjustRectangleToViewport(otherRectangle, false);
				Point otherPointToScreen = PointToScreen(otherAdjustRectangleToViewport.Location);
				Point nextPointToScreen = PointToScreen(new Point(otherAdjustRectangleToViewport.Left, otherAdjustRectangleToViewport.Bottom));
				GridHelper.DrawSplitLine(this, otherPointToScreen, nextPointToScreen, Color.Black);
			}
		}

		public GridColumn ColumnFromPoint()
		{
			Point point = PointToClient(PointerPosition.Current);
			return ColumnFromPoint(point.X, point.Y);
		}

		public GridColumn ColumnFromPoint(int x, int y)
		{
			GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(x, y);
			switch (gridGroupLayoutState._type)
			{
			case LayoutItemType.Cell:
			case LayoutItemType.ColumnHeader:
			case LayoutItemType.TotalRowCell:
			case LayoutItemType.GroupTotalCell:
			case LayoutItemType.CardHeader:
			case LayoutItemType.CardCell:
			case LayoutItemType.FilterRowCell:
			case LayoutItemType.CellColumnButton:
			case LayoutItemType.CellDropDownButton:
			case LayoutItemType.CellUpButton:
			case LayoutItemType.CellDownButton:
				return gridGroupLayoutState._column;
			default:
				return null;
			}
		}

		public GridColumnSet ColumnSetFromPoint()
		{
			Point point = PointToClient(PointerPosition.Current);
			return ColumnSetFromPoint(point.X, point.Y);
		}

		public GridColumnSet ColumnSetFromPoint(int x, int y)
		{
			GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(x, y);
			LayoutItemType type = gridGroupLayoutState._type;
			if (type == LayoutItemType.ColumnSetHeader)
			{
				return gridGroupLayoutState._columnSet;
			}
			return null;
		}

		private bool UsesColumnSets(GridTable gridTable)
		{
			if (gridTable == null)
			{
				return false;
			}
			if (gridTable.CellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return true;
			}
			if (Hierarchical && gridTable.HierarchicalMode == HierarchicalMode.UseChildTables)
			{
				foreach (GridTable childTable in gridTable.ChildTables)
				{
					if (UsesColumnSets(childTable))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected override ImageList GetControlImageList()
		{
			return ComputeImageList();
		}

		internal ImageList ComputeImageList()
		{
			if (this is GridDropDown)
			{
				GridDropDown gridDropDown = (GridDropDown)this;
				if (gridDropDown._ownerCollection != null)
				{
					return gridDropDown._ownerCollection.DataGrid.ComputeImageList();
				}
			}
			if (_imageSource == ImageSource.InternalImageList)
			{
				return GetOrCreateImageList();
			}
			return _externalImageList;
		}

		internal void RefreshAfterTableDataChange(GridTable gridTable)
		{
			if (!_isInitializing)
			{
				AcceptCellEdit();
				NotifyRefreshListeners();
				if (_filterMode != FilterMode.None)
				{
					_gridRow = null;
					RootTable.RemoveFilter();
				}
				if (_rowCache != null)
				{
					RebuildRows(true);
					RefreshRowCountAndInvalidate();
				}
			}
		}

		internal void RefreshDataSourceView()
		{
			_gridDataSourceManager.Refresh(refreshAll: false);
			if (RootTable != null && _numericEditControl != null)
			{
				_numericEditControl.UpdateNumericEditControlBoolean(RootTable.IsAddNewAllowed());
			}
			RefreshRowCountAndInvalidate();
		}

		internal static GridTableCollection CreateTableCollection(GridTable gridTable)
		{
			GridTableCollection gridTableCollection = new GridTableCollection();
			if (gridTable != null)
			{
				gridTableCollection.AddTableWithChildren(gridTable);
			}
			return gridTableCollection;
		}

		internal void RebuildTableCollection()
		{
			_gridTableCollection = CreateTableCollection(RootTable);
			RefreshRowCountAndInvalidate();
		}

		internal GridValueListItemCollection GetDistinctValuesForColumn(GridColumn field)
		{
			if (field.Table != RootTable)
			{
				return null;
			}
			return (GridValueListItemCollection)_gridDataSourceManager.GetNonDuplicatedList(field.Table, field, excludeNonVisible: true);
		}

		private bool HasFocusableColumnFrom(int value, int position)
		{
			GridColumn gridColumn = null;
			GridRow gridRow = ((value < -2 || (value == -1 && !IsNewRowAtTop())) ? null : ((value == GetTotalRowPosition()) ? GetGridRowForLogicalRow(_gridDataSourceManager.GetRootTotalRow()) : ((value <= RowCount - 1) ? GetGridRowForLogicalRow(_gridDataSourceManager.GetRowByIndex(value)) : null)));
			if (gridRow != null && position >= 0 && position < gridRow.Table.Columns.Count)
			{
				GridColumn lastEditableColumn = GetLastEditableColumn(gridRow);
				if (lastEditableColumn != null)
				{
					if (position < lastEditableColumn.Position)
					{
						GridColumn columnInPosition = gridRow.Table.Columns.GetColumnInPosition(position);
						bool visible = columnInPosition.Visible;
						if (columnInPosition.ColumnSet != null && !columnInPosition.ColumnSet.Visible)
						{
							visible = false;
						}
						if (!IsCellSelectable(gridRow, columnInPosition) || !visible)
						{
							return HasFocusableColumnFrom(value, position + 1);
						}
						gridColumn = columnInPosition;
					}
					else
					{
						gridColumn = lastEditableColumn;
					}
				}
			}
			else
			{
				gridColumn = null;
			}
			return CanFocusCell(value, gridColumn);
		}

		public bool UpdateData()
		{
			if (_row == null)
			{
				return true;
			}
			_ = Row;
			object dataRow = _row.DataRow;
			if (dataRow is DataRowView)
			{
				dataRow = ((DataRowView)dataRow).Row;
			}
			if (!AcceptCellEdit())
			{
				return false;
			}
			if (DataChanged)
			{
				bool autoEdit = _autoEdit;
				bool result = CommitCurrentRow();
				_autoEdit = autoEdit;
				return result;
			}
			if (_row != null && _row.RowType == RowType.FilterRow)
			{
				AcceptCellEdit();
				return ApplyFilterRow(false);
			}
			if (_row != null && _row._isEditing)
			{
				if (_row.RowType == RowType.NewRecord)
				{
					_row.CancelEdit();
				}
				else
				{
					_row.EndEdit();
				}
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual void OnResetDataSource(EventArgs e)
		{
			if (_resetDataSourceHandler != null)
			{
				_resetDataSourceHandler(this, e);
			}
		}

		public FieldChooserForm ShowFieldChooser()
		{
			if (_view != View.TableView)
			{
				throw new InvalidOperationException();
			}
			EnsureFieldChooserForm();
			_gridFieldChooser.ProcessDataGrid(this, RootTable, true);
			return _gridFieldChooser;
		}

		public FieldChooserForm ShowFieldChooser(Form owner)
		{
			if (_view != View.TableView)
			{
				throw new InvalidOperationException();
			}
			EnsureFieldChooserForm();
			_gridFieldChooser.Owner = owner;
			_gridFieldChooser.ProcessDataGrid(this, RootTable, true);
			return _gridFieldChooser;
		}

		public FieldChooserForm ShowFieldChooser(Form owner, string caption)
		{
			if (_view != View.TableView)
			{
				throw new InvalidOperationException();
			}
			EnsureFieldChooserForm();
			_gridFieldChooser.Owner = owner;
			_gridFieldChooser.Text = caption;
			_gridFieldChooser.ProcessDataGrid(this, RootTable, true);
			return _gridFieldChooser;
		}

		public FieldChooserForm ShowFieldChooser(Form owner, string caption, GridTable table, bool showTableList)
		{
			if (table != null && table.DataGrid != this)
			{
				throw new ArgumentException();
			}
			if (_view != View.TableView)
			{
				throw new InvalidOperationException();
			}
			EnsureFieldChooserForm();
			_gridFieldChooser.Owner = owner;
			_gridFieldChooser.Text = caption;
			_gridFieldChooser.ProcessDataGrid(this, table, showTableList);
			return _gridFieldChooser;
		}

		public void HideFieldChooser()
		{
			if (_gridFieldChooser != null)
			{
				_gridFieldChooser.Close();
			}
		}

		private void EnsureFieldChooserForm()
		{
			if (_gridFieldChooser == null)
			{
				_gridFieldChooser = new FieldChooserForm();
				_gridFieldChooser.ShowInTaskbar = false;
			}
		}

		internal void DisposeFieldChooser()
		{
			OnFieldChooserClosed(EventArgs.Empty);
			if (_gridFieldChooser != null)
			{
				_gridFieldChooser.Dispose();
			}
			_gridFieldChooser = null;
		}

		internal bool ShouldHideColumnsWhenGrouped()
		{
			InheritableBoolean inheritableBoolean = _hideColumnsWhenGrouped;
			if (inheritableBoolean == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool ShouldAllowRemoveColumns()
		{
			InheritableBoolean inheritableBoolean = _allowRemoveColumns;
			if (inheritableBoolean == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool ShouldShowColumnHeaders()
		{
			InheritableBoolean inheritableBoolean = _columnHeaders;
			if (inheritableBoolean == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal bool ShouldShowColumnSetHeaders()
		{
			InheritableBoolean inheritableBoolean = _columnSetHeaders;
			if (inheritableBoolean == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		private ImageList GetOrCreateImageList()
		{
			if (_imageList == null)
			{
				_imageList = new ImageList();
			}
			return _imageList;
		}

		public object GetValue(int columnIndex)
		{
			return GetValue(CurrentTable.Columns[columnIndex]);
		}

		public object GetValue(GridColumn column)
		{
			if (_updatingCellEventArgs != null && _updatingCellEventArgs.Column == column)
			{
				return _updatingCellEventArgs.Value;
			}
			if (_row != null)
			{
				if (_currentColumn == column && _gridColumnContainerControl != null && column != null)
				{
					GridCell gridCell = _row.Cells[column];
					EditType editType = column.EditType;
					if (_row.RowType == RowType.FilterRow)
					{
						if (column.FilterEditType != FilterEditType.SameAsEditType)
						{
							editType = ((column.FilterEditType != FilterEditType.NoEdit) ? EditType.TextBox : EditType.NoEdit);
						}
					}
					else
					{
						editType = gridCell.EditType;
					}
					if (editType != EditType.NoEdit)
					{
						return _gridColumnContainerControl.ReadEditorValue();
					}
				}
				return _row.Cells[column].Value;
			}
			return null;
		}

		public object GetValue(string columnKey)
		{
			return GetValue(CurrentTable.Columns[columnKey]);
		}

		public void SetValue(int columnIndex, object value)
		{
			SetValue(CurrentTable.Columns[columnIndex], value);
		}

		public void SetValue(GridColumn column, object value)
		{
			if (_row != null && (_row.RowType == RowType.Record || _row.RowType == RowType.NewRecord || _row.RowType == RowType.FilterRow))
			{
				if (_currentColumn == column && _gridColumnContainerControl != null && column.EditType != EditType.NoEdit)
				{
					_gridColumnContainerControl.WriteEditorValue(value);
					if (_gridColumnContainerControl != null)
					{
						_gridColumnContainerControl.SetModified(true);
					}
					return;
				}
				if (value is string && column.Type != typeof(string) && column.BoundMode != ColumnBoundMode.UnboundFetch)
				{
					try
					{
						value = PropertyValue.ConvertValue((string)value, column.Type);
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
				bool dataChanged = DataChanged;
				if (_updateMode == UpdateMode.CellUpdate && _row.RowType != RowType.FilterRow)
				{
					if (!_row._isEditing && (_row.RowType == RowType.Record || _row.RowType == RowType.NewRecord))
					{
						BeginCurrentRowEdit();
						_row.ResetNewRecord();
					}
					GridCell gridCell = _row.Cells[column];
					if (gridCell.CellDisplayType == ColumnType.BoundImage)
					{
						if (value == null)
						{
							gridCell.Value = null;
						}
						else if (value is Image)
						{
							Image source = (Image)value;
							Image filled = GridHelper.FillImageBackground(source, gridCell.Column.BoundImageMaskColor);
							byte[] array;
							using (MemoryStream memoryStream = new MemoryStream())
							{
								try
								{
									// When the background cannot be filled, the caller's own image is saved and stays the caller's.
									(filled ?? source).Save(memoryStream, ImageFormat.Bmp);
								}
								finally
								{
									filled?.Dispose();
								}
								array = memoryStream.ToArray();
							}
							gridCell.Value = array;
						}
						else
						{
							gridCell.Value = value;
						}
					}
					else
					{
						gridCell.Value = value;
					}
				}
				_row.Cells[column].AssignValue(value, true);
				if (!dataChanged)
				{
					if (_row.RowType == RowType.NewRecord && _row.Table.ComputeNewRowPosition() == NewRowPosition.BottomRow)
					{
						MordexNewDataRow mordexNewDataRow = (MordexNewDataRow)_row.GetLogicalRow();
						mordexNewDataRow.AddExtraNewRow();
					}
					if (_row.RowType != RowType.FilterRow)
					{
						OnFirstChange(new RowActionEventArgs(_row));
					}
				}
				if (_currentColumn == column && _gridColumnContainerControl != null)
				{
					GridCell cell = _row.Cells[_currentColumn];
					if (cell.EditType != EditType.NoEdit)
					{
						_gridColumnContainerControl.WriteEditorValue(_row.Cells[column].Text);
					}
				}
				Invalidate();
				return;
			}
			throw new InvalidOperationException("Unable to set value because there is no row selected");
		}

		public void SetValue(string columnKey, object value)
		{
			SetValue(CurrentTable.Columns[columnKey], value);
		}

		public override void Refresh()
		{
			if (CurrentRow != null)
			{
				CurrentRow.Format();
			}
			RebuildRows(!DataChanged);
		}

		public void Refetch()
		{
			DataChanged = false;
			object[] array = null;
			if (_gridRow != null && FilterMode != FilterMode.None && RootTable != null && RootTable.Columns.Count > 0)
			{
				array = new object[RootTable.Columns.Count];
				for (int i = 0; i < RootTable.Columns.Count; i++)
				{
					array[i] = _gridRow.Cells[i].Value;
				}
			}
			_gridDataSourceManager.Refresh(refreshAll: true);
			ClearLayoutStates();
			try
			{
				if (array != null && _filterMode == FilterMode.Automatic)
				{
					_gridRow = GetFilterGridRow();
					for (int j = 0; j < array.Length; j++)
					{
						GridCell gridCell = _gridRow.Cells[j];
						gridCell.AssignValue(array[j], true);
					}
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		internal bool ShouldShowTableHeaders()
		{
			if (_tableHeaders == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		private void ProcessArrayList(ArrayList arrayList, GridTable gridTable, MordexRow mordexRow, bool condition)
		{
			if (!mordexRow.IsVirtualRow() && mordexRow.Table == gridTable)
			{
				arrayList.Add(mordexRow);
				if (mordexRow is MordexGroupRow mordexGroupRow && mordexGroupRow.TotalRow != null && (condition || mordexGroupRow.TotalRow.IsRowVisible()))
				{
					arrayList.Add(mordexGroupRow.TotalRow);
				}
			}
			if (!mordexRow.ChildrenLoaded)
			{
				return;
			}
			if (mordexRow.Expanded || condition || mordexRow is MordexGroupRow)
			{
				foreach (MordexRow item in (IEnumerable)mordexRow.Rows)
				{
					ProcessArrayList(arrayList, gridTable, item, condition);
				}
			}
			if (mordexRow is MordexTableRow mordexTableRow && mordexTableRow.TotalRow != null && (condition || mordexTableRow.TotalRow.IsRowVisible()))
			{
				arrayList.Add(mordexTableRow.TotalRow);
			}
		}

		private void RefreshTablesAfterInit()
		{
			ClearUnusedRootTableStyles();
			ClearOrphanRootTableDropDowns();
			RemoveOrphanRootTableSortKeys();
		}

		internal void ClearUnusedRootTableStyles()
		{
			if (!_isInitializing && RootTable != null)
			{
				RootTable.DropDefaultFormatStyles();
			}
		}

		internal object RaiseCreateMultipleValueItem(GridColumn column, object itemValue)
		{
			CreateMultipleValueItemEventArgs e = new CreateMultipleValueItemEventArgs(column, itemValue);
			OnCreateMultipleValueItem(e);
			return e.Item;
		}

		protected virtual void OnFitColumns(EventArgs e)
		{
			if (base.Events[_fitColumnsEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnRootTableChanged(EventArgs e)
		{
			if (base.Events[_rootTableChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCreateMultipleValueItem(CreateMultipleValueItemEventArgs e)
		{
			if (base.Events[_createMultipleValueItemEventKey] is CreateMultipleValueItemEventHandler createMultipleValueItemEventHandler)
			{
				createMultipleValueItemEventHandler(this, e);
			}
		}

		protected virtual void OnFieldChooserClosed(EventArgs e)
		{
			if (base.Events[_fieldChooserClosedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal IList ComputeIList(MordexRow mordexRow, GridTable childTable)
		{
			GridRow parentRow = GetGridRowForLogicalRow(mordexRow);
			GetChildListEventArgs e = new GetChildListEventArgs(parentRow, childTable);
			OnGetChildList(e);
			if (e.ChildList is DataTable)
			{
				return new DataView((DataTable)e.ChildList);
			}
			return (IList)e.ChildList;
		}

		protected virtual void OnMovingColumn(MovingColumnEventArgs e)
		{
			if (base.Events[_eventKeyMovingColumn] is MovingColumnEventHandler movingColumnEventHandler)
			{
				movingColumnEventHandler(this, e);
			}
		}

		protected virtual void OnColumnMoved(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyColumnMoved] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnSizingColumn(SizingColumnEventArgs e)
		{
			if (base.Events[_eventKeySizingColumn] is SizingColumnEventHandler sizingColumnEventHandler)
			{
				sizingColumnEventHandler(this, e);
			}
		}

		protected virtual void OnSizingColumnSet(SizingColumnSetEventArgs e)
		{
			if (base.Events[_eventKeySizingColumnSet] is SizingColumnSetEventHandler sizingColumnSetEventHandler)
			{
				sizingColumnSetEventHandler(this, e);
			}
		}

		protected virtual void OnNotInList(NotInListEventArgs e)
		{
			if (base.Events[_eventKeyNotInList] is NotInListEventHandler notInListEventHandler)
			{
				notInListEventHandler(this, e);
			}
		}

		private int GetEffectiveFrozenColumns()
		{
			if (_view == View.TableView)
			{
				if (_hierarchical && _gridTableCollection.Count > 1)
				{
					return 0;
				}
				return _frozenColumns;
			}
			return 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBuiltInTextsData()
		{
			BuiltInTexts.Reset();
		}

		void IDragHost.AddListener(IRefreshListener listener)
		{
			if (listener == null)
			{
				throw new ArgumentNullException();
			}
			if (_refreshListeners == null)
			{
				_refreshListeners = new List<IRefreshListener>();
			}
			_refreshListeners.Add(listener);
		}

		void IDragHost.RemoveListener(IRefreshListener listener)
		{
			if (_refreshListeners != null)
			{
				_refreshListeners.Remove(listener);
			}
		}

		public void SuspendBinding()
		{
			_gridDataSourceManager.RowManager.SuspendIndex();
		}

		public void ResumeBinding()
		{
			_gridDataSourceManager.RowManager.ResumeIndex();
			if (!_gridDataSourceManager.RowManager.IsIndexSuspend)
			{
				RebuildRows(true);
			}
		}

		public void ClearItems()
		{
			if (BoundMode == BoundMode.Bound)
			{
				throw new InvalidOperationException("ClearItems can not be used when BoundMode is Bound. Clear the items directly in the datasource or change BoundMode to Unbound.");
			}
			CancelCurrentEdit();
			CurrencyManager currencyManager = GetCurrencyManager();
			currencyManager?.SuspendBinding();
			_gridDataSourceManager.ClearItems();
			currencyManager?.ResumeBinding();
		}

		public GridRow AddItem(params object[] rowValues)
		{
			if (BoundMode == BoundMode.Bound)
			{
				throw new InvalidOperationException("Items can not be added in a grid when BoundMode is Bound. Add the items directly in the datasource or change BoundMode to Unbound.");
			}
			MordexDataRow mordexDataRow = _gridDataSourceManager.AddItem();
			GridRow gridRow = GetGridRowForLogicalRow(mordexDataRow);
			if (rowValues != null)
			{
				gridRow.BeginEdit();
				for (int i = 0; i < rowValues.Length; i++)
				{
					_ = RootTable.Columns[i];
					gridRow.Cells[i].Value = rowValues[i];
				}
				gridRow.EndEdit();
			}
			return gridRow;
		}

		public GridRow AddItem()
		{
			if (BoundMode == BoundMode.Bound)
			{
				throw new InvalidOperationException("Items can not be added in a grid when BoundMode is Bound. Add the items directly in the datasource or change BoundMode to Unbound.");
			}
			return GetGridRowForLogicalRow(_gridDataSourceManager.AddItem());
		}

		public GridRow AddItem(int index)
		{
			if (BoundMode == BoundMode.Bound)
			{
				throw new InvalidOperationException("Items can not be added in a grid when BoundMode is Bound. Add the items directly in the datasource or change BoundMode to Unbound.");
			}
			return GetGridRowForLogicalRow(_gridDataSourceManager.AddItem(index));
		}

		public GridRow AddItem(GridRow parent)
		{
			return AddItem(parent, null);
		}

		public GridRow AddItem(GridRow parent, GridTable table)
		{
			if (BoundMode == BoundMode.Bound)
			{
				throw new InvalidOperationException("Items can not be added in a grid when BoundMode is Bound. Add the items directly in the datasource or change BoundMode to Unbound.");
			}
			if (parent == null)
			{
				if (table == null || table == RootTable)
				{
					return AddItem();
				}
				throw new ArgumentNullException("table can not be null.");
			}
			if (parent.DataGrid != this)
			{
				throw new InvalidOperationException("Parent row does not belong to the grid.");
			}
			if (parent.RowType != RowType.Record)
			{
				throw new InvalidOperationException("Rows can only be added to a GridRow with RowType = RowType.Record");
			}
			if (table == null)
			{
				if (parent.Table.GetBaseIsSelfReferencingTable())
				{
					table = parent.Table;
				}
				else
				{
					if (parent.Table.ChildTables.Count == 0)
					{
						throw new InvalidOperationException("Can not add child rows when table has no childtables defined.");
					}
					table = parent.Table.ChildTables[0];
				}
			}
			MordexTableRow mordexTableRow = null;
			foreach (MordexTableRow item in (IEnumerable)parent.GetLogicalRow().Rows)
			{
				if (item.Table == table)
				{
					mordexTableRow = item;
					break;
				}
			}
			if (mordexTableRow == null)
			{
				throw new InvalidEnumArgumentException("table is not a child of the table in parent row.");
			}
			MordexDataRow mordexDataRow = _gridDataSourceManager.AddItem((MordexDataRow)parent.GetLogicalRow(), table);
			return GetGridRowForLogicalRow(mordexDataRow);
		}
	}
}
