using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using Mordex.Data;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridRow
	{
		private GridFormatStyleState _gridFormatStyleState;

		private MxDataGrid _grid;

		private bool _hasBeenReset;

		private object _dataRow;

		private bool _tablePreviewRowVisible;

		private MordexRow _logicalRow;

		private GridCellCollection _gridCellCollection;

		private RowType _rowType;

		internal bool _isLayoutDirty;

		private bool _hasChangedCells;

		internal bool _isFormatting;

		internal bool _isLoading;

		internal bool _isLayoutSuspended;

		private GridFormatStyle _gridFormatStyle;

		private GridFormatStyle _gridRowGridFormatStyle;

		internal int _groupConditionCount;

		private string _groupCaption;

		private int _groupImageIndex = -1;

		private string _groupImageKey = "";

		private Image _groupImage;

		internal bool _isEditing;

		private string _textOverride;

		internal bool _hasTextOverride;

		internal GridRow(MordexRow logicalRow, MxDataGrid grid)
		{
			if (grid.KeepRowSettings)
			{
				RowTag rowTag = ((!(logicalRow is MordexDataRow)) ? RowStateHelper.GetRowTag(logicalRow) : RowStateHelper.GetRowTag(((MordexDataRow)logicalRow).DataHolder));
				if (rowTag.GetFormatStyleState() != null)
				{
					_gridFormatStyleState = rowTag.GetFormatStyleState();
				}
				else
				{
					_gridFormatStyleState = new GridFormatStyleState();
					rowTag.SetFormatStyleState(_gridFormatStyleState);
				}
			}
			else
			{
				_gridFormatStyleState = new GridFormatStyleState();
			}
			_grid = grid;
			_logicalRow = logicalRow;
			_dataRow = logicalRow.DataRow;
			_gridCellCollection = new GridCellCollection(this);
			_rowType = RowStateHelper.ComputeRowType(logicalRow);
			if (Table.HasPreviewRow() && _rowType == RowType.Record && logicalRow is MordexDataRow)
			{
				object value = null;
				try
				{
					value = logicalRow.GetValue(Table.PreviewRowMember);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
				if (value == null)
				{
					_gridFormatStyleState._previewRowText = string.Empty;
				}
				else
				{
					_gridFormatStyleState._previewRowText = GridHelper.ToSafeString(value);
				}
			}
			if (RowType == RowType.Record)
			{
				_tablePreviewRowVisible = Table.HasPreviewRow();
			}
			if (_rowType == RowType.GroupHeader)
			{
				GridColumn column = RowStateHelper.GetRowGroup(logicalRow).Column;
				object groupValue = ((MordexGroupRow)logicalRow).GroupValue;
				GridGroup gridGroup = RowStateHelper.GetRowGroup(logicalRow);
				if (column != null && gridGroup.GroupInterval == GroupInterval.Value)
				{
					column.FormatCellValue(groupValue);
				}
				_groupCaption = ((MordexGroupRow)logicalRow).GroupText;
				if (_groupCaption.Length == 0 && gridGroup.Column != null)
				{
					_groupCaption = gridGroup.Column.GroupEmptyString;
				}
				foreach (GridGroupHeaderTotal groupHeaderTotal in Table.GroupHeaderTotals)
				{
					if (groupHeaderTotal.ColumnAlign != null)
					{
						_gridCellCollection[groupHeaderTotal.ColumnAlign].AssignValue(GetSubTotal(groupHeaderTotal.Column, groupHeaderTotal.AggregateFunction, groupHeaderTotal.FilterCondition), false);
						_gridCellCollection[groupHeaderTotal.ColumnAlign].Text = groupHeaderTotal.GetTotalText(this);
						_gridCellCollection[groupHeaderTotal.ColumnAlign]._groupHeaderTotal = groupHeaderTotal;
					}
				}
				_groupCaption += BuildGroupTotalsText();
				if (groupValue is GridCustomGroupRow)
				{
					GridCustomGroupRow gridCustomGroupRow = groupValue as GridCustomGroupRow;
					if (gridCustomGroupRow.GroupImage != null)
					{
						_groupImage = gridCustomGroupRow.GroupImage;
						return;
					}
					_groupImageKey = gridCustomGroupRow.GroupImageKey;
					_groupImageIndex = gridCustomGroupRow.GroupImageIndex;
				}
				else
				{
					if (column == null || (column.ColumnType != ColumnType.Image && column.ColumnType != ColumnType.ImageAndText))
					{
						return;
					}
					if (column.Image != null)
					{
						_groupImage = column.Image;
					}
					else
					{
						_groupImageKey = column.ImageKey;
						_groupImageIndex = column.ImageIndex;
					}
					if (column.ValueList != null && column.ValueList.ReplaceValues)
					{
						GridValueListItem gridValueListItem = column.ValueList[((MordexGroupRow)logicalRow).OriginalGroupValue];
						if (gridValueListItem == null)
						{
							return;
						}
						if (gridValueListItem.Image == null)
						{
							_groupImageIndex = gridValueListItem.ImageIndex;
							_groupImageKey = gridValueListItem.ImageKey;
							if (_groupImageIndex != -1 || _groupImageKey.Length > 0)
							{
								_groupImage = null;
							}
						}
						else
						{
							_groupImage = gridValueListItem.Image;
						}
					}
					else if (column.DropDown != null && column.DropDown.ReplaceValues)
					{
						_groupImage = column.DropDown.GetImageForValue(((MordexGroupRow)logicalRow).OriginalGroupValue);
					}
				}
			}
			else if (logicalRow is MordexTotalRow)
			{
				RecalculateTotals();
			}
		}
	}
}
