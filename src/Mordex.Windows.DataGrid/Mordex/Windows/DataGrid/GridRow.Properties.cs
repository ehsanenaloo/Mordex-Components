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
		public bool Selected
		{
			get
			{
				if (_logicalRow != null)
				{
					return RowStateHelper.GetRowTag(_logicalRow).GetDataState3Boolean();
				}
				return false;
			}
		}

		public bool ChildrenLoaded => _logicalRow.ChildrenLoaded;

		public GridRow Parent
		{
			get
			{
				if (_logicalRow != null)
				{
					MordexRow parentRow = _logicalRow.ParentRow;
					if (parentRow == null || parentRow is MordexRootRow)
					{
						return null;
					}
					while (parentRow != null && parentRow.IsVirtualRow())
					{
						parentRow = parentRow.ParentRow;
					}
					return DataGrid.GetGridRowForLogicalRow(parentRow);
				}
				return null;
			}
		}

		public bool IsVisible
		{
			get
			{
				if (_logicalRow == null)
				{
					return false;
				}
				return _logicalRow.IsRowVisible();
			}
		}

		public int Children
		{
			get
			{
				if (_logicalRow == null || _logicalRow.Rows == null)
				{
					return 0;
				}
				return _logicalRow.Rows.VisibleCount;
			}
		}

		public object DataRow
		{
			get
			{
				if (_logicalRow != null)
				{
					if (_rowType == RowType.Record)
					{
						return _logicalRow.DataRow;
					}
					if (_rowType == RowType.NewRecord)
					{
						return _logicalRow.DataRow;
					}
				}
				return null;
			}
		}

		public string PreviewRowText
		{
			get
			{
				return _gridFormatStyleState._previewRowText;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_gridFormatStyleState._previewRowText != value)
				{
					_gridFormatStyleState._previewRowText = value;
					RequestLayout(true);
				}
			}
		}

		public bool PreviewRowVisible
		{
			get
			{
				if (!string.IsNullOrEmpty(PreviewRowText) && _gridFormatStyleState._previewRowVisible.GetValueOrDefault(_tablePreviewRowVisible))
				{
					return true;
				}
				return false;
			}
			set
			{
				if (_rowType != RowType.Record)
				{
					throw new InvalidOperationException();
				}
				if (PreviewRowVisible != value)
				{
					_gridFormatStyleState._previewRowVisible = value;
					RequestLayout(true);
				}
				else
				{
					_gridFormatStyleState._previewRowVisible = value;
				}
			}
		}

		public GridTable Table
		{
			get
			{
				if (_rowType == RowType.FilterRow)
				{
					return _grid.RootTable;
				}
				if (_logicalRow != null)
				{
					return (GridTable)_logicalRow.Table;
				}
				return null;
			}
		}

		public int RowIndex
		{
			get
			{
				if (_logicalRow is MordexDataRow mordexDataRow)
				{
					return mordexDataRow.DataRowIndex;
				}
				if (_logicalRow is DataHolderRow dataHolderRow)
				{
					return dataHolderRow.DataRowIndex;
				}
				return -1;
			}
		}

		public int AbsolutePosition => _logicalRow.RecordPosition + 1;

		public int Position
		{
			get
			{
				if (_logicalRow != null)
				{
					if (IsVisible)
					{
						if (RowType == RowType.TotalRow && Table.IsRootTable() && DataGrid.TotalRowPosition == TotalRowPosition.BottomFixed)
						{
							return DataGrid.RowCount;
						}
						return _logicalRow.RowManagerPosition;
					}
					if (_logicalRow is DataMordexRow)
					{
						return -3;
					}
					return -1;
				}
				return -3;
			}
		}

		public int RowHeaderImageIndex
		{
			get
			{
				return _gridFormatStyleState._rowHeaderImageIndex;
			}
			set
			{
				if (_gridFormatStyleState._rowHeaderImageIndex != value)
				{
					_gridFormatStyleState._rowHeaderImageIndex = value;
					if (value != -1)
					{
						_gridFormatStyleState._rowHeaderImageKey = "";
					}
					RequestLayout(true);
				}
			}
		}

		public string RowHeaderImageKey
		{
			get
			{
				return _gridFormatStyleState._rowHeaderImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (RowHeaderImageKey != value)
				{
					_gridFormatStyleState._rowHeaderImageKey = value;
					if (value.Length > 0)
					{
						_gridFormatStyleState._rowHeaderImageIndex = -1;
					}
					RequestLayout(true);
				}
			}
		}

		public string RowHeaderText
		{
			get
			{
				if (_gridFormatStyleState._rowHeaderText == null)
				{
					if (RowType == RowType.FilterRow || _logicalRow is DataMordexRow)
					{
						return "";
					}
					if (Table != null)
					{
						switch (Table.ComputeRowHeaderContent())
						{
						case RowHeaderContent.RowIndex:
							if (RowType == RowType.NewRecord || RowType == RowType.TotalRow || _logicalRow is DataMordexRow)
							{
								return "";
							}
							return string.Concat(_logicalRow.RecordPosition + 1);
						case RowHeaderContent.RowPosition:
							if ((RowType == RowType.NewRecord || RowType == RowType.TotalRow) && Table.IsRootTable())
							{
								return "";
							}
							return string.Concat(Position + 1);
						}
					}
					return "";
				}
				return _gridFormatStyleState._rowHeaderText;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_gridFormatStyleState._rowHeaderText != value)
				{
					_gridFormatStyleState._rowHeaderText = value;
					RequestLayout(true);
				}
			}
		}

		public GridCellCollection Cells => _gridCellCollection;

		public RowType RowType => _rowType;

		public bool Expanded
		{
			get
			{
				if (DataGrid != null && DataGrid.View != View.TableView)
				{
					return RowStateHelper.GetRowTag(_logicalRow).GetIsExpanded();
				}
				return _logicalRow.Expanded;
			}
			set
			{
				DataGrid.SetRowExpandedState(_logicalRow, value);
			}
		}

		public MxDataGrid DataGrid => _grid;

		public string CardCaptionText
		{
			get
			{
				if (_gridFormatStyleState._cardCaptionText == null)
				{
					GridColumn owner = Table.Columns._owner;
					if (owner != null)
					{
						if (DataGrid.CardCaptionPrefix.Length > 0)
						{
							return DataGrid.CardCaptionPrefix + " " + _gridCellCollection[owner.Index].Text;
						}
						return _gridCellCollection[owner.Index].Text;
					}
					if (DataGrid.CardCaptionPrefix.Length == 0)
					{
						return "";
					}
					return DataGrid.CardCaptionPrefix;
				}
				return _gridFormatStyleState._cardCaptionText;
			}
			set
			{
				if (value != CardCaptionText)
				{
					_gridFormatStyleState._cardCaptionText = value;
					RequestLayout(false);
				}
			}
		}

		public GridFormatStyle RowStyle
		{
			get
			{
				return _gridFormatStyleState._rowFormatStyle;
			}
			set
			{
				if (_gridFormatStyleState._rowFormatStyle != value)
				{
					_gridFormatStyleState._rowFormatStyle = value;
					RequestLayout(true);
				}
			}
		}

		public int GroupConditionCount => _groupConditionCount;

		public GridGroup Group
		{
			get
			{
				if (_logicalRow != null)
				{
					return (GridGroup)_logicalRow.Group;
				}
				return null;
			}
		}

		public string GroupCaption
		{
			get
			{
				if (_gridFormatStyleState._groupCaption == null)
				{
					return _groupCaption;
				}
				return _gridFormatStyleState._groupCaption;
			}
			set
			{
				if (value != GroupCaption || _gridFormatStyleState._groupCaption == null)
				{
					_gridFormatStyleState._groupCaption = value;
					RequestLayout(false);
				}
			}
		}

		public object GroupValue
		{
			get
			{
				if (RowType == RowType.GroupHeader && _logicalRow != null)
				{
					return ((MordexGroupRow)_logicalRow).GroupValue;
				}
				return null;
			}
		}

		public int GroupImageIndex
		{
			get
			{
				if (_gridFormatStyleState.HasGroupImageIndex())
				{
					return _gridFormatStyleState.GetGroupImageIndex();
				}
				return _groupImageIndex;
			}
			set
			{
				if (_gridFormatStyleState.GetGroupImageIndex() != value)
				{
					_gridFormatStyleState.SetGroupImageIndex(value);
					_gridFormatStyleState._groupImageKey = "";
					RequestLayout(false);
				}
				else
				{
					_gridFormatStyleState.SetGroupImageIndex(value);
				}
			}
		}

		public string GroupImageKey
		{
			get
			{
				if (_gridFormatStyleState._groupImageKey == null)
				{
					return _groupImageKey;
				}
				return _gridFormatStyleState._groupImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_gridFormatStyleState._groupImageKey != value)
				{
					_gridFormatStyleState._groupImageKey = value;
					if (value.Length > 0)
					{
						_gridFormatStyleState.SetGroupImageIndex(-1);
					}
					RequestLayout(false);
				}
				else
				{
					_gridFormatStyleState._groupImageKey = value;
				}
			}
		}

		public Image GroupImage
		{
			get
			{
				if (_gridFormatStyleState.HasGroupImage())
				{
					return _gridFormatStyleState.GetGroupImage();
				}
				return _groupImage;
			}
			set
			{
				if (_gridFormatStyleState.GetGroupImage() != value)
				{
					_gridFormatStyleState.SetGroupImage(value);
					RequestLayout(true);
				}
				else
				{
					_gridFormatStyleState.SetGroupImage(value);
				}
			}
		}

		public bool IsChecked
		{
			get
			{
				if (_logicalRow != null)
				{
					return _logicalRow.IsChecked;
				}
				return false;
			}
			set
			{
				if (_logicalRow != null && (_rowType == RowType.Record || RowType == RowType.NewRecord || _rowType == RowType.GroupHeader))
				{
					_logicalRow.IsChecked = value;
					if (DataGrid != null)
					{
						DataGrid.SyncRowCheckToEditor(this);
						return;
					}
				}
				throw new InvalidOperationException();
			}
		}

		public RowCheckState CheckState
		{
			get
			{
				if (_logicalRow != null)
				{
					return DataState.ComputeRowCheckState(_logicalRow.CheckedState);
				}
				return RowCheckState.Unchecked;
			}
			set
			{
				if (_logicalRow != null)
				{
					_logicalRow.CheckedState = DataState.ComputeCheckedState(value);
					return;
				}
				throw new InvalidOperationException();
			}
		}
	}
}
