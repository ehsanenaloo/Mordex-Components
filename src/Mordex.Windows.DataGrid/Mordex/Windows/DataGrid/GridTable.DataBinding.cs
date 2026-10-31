using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridTable
	{
		protected override void SetDefaultFieldValues()
		{
			base.SetDefaultFieldValues();
			InitializeCollections();
		}

		protected override MordexFieldCollectionBase CreateFieldCollection()
		{
			return new GridColumnCollection(this);
		}

		internal void UpdateDataGrid(MxDataGrid dataGrid)
		{
			if (dataGrid != _dataGrid)
			{
				_dataGrid = dataGrid;
			}
		}

		internal bool IsRootTable()
		{
			if (_dataGrid != null)
			{
				return _dataGrid.RootTable == this;
			}
			return ParentTable == null;
		}

		internal void RefreshDataGrid()
		{
			MxDataGrid dataGrid = DataGrid;
			if (dataGrid != null)
			{
				dataGrid.RefreshRowCountAndInvalidate();
				dataGrid.NotifyRefreshListeners();
			}
		}

		protected override void OnFieldRemoved(MordexFieldBase field)
		{
			GridColumn gridColumn = field as GridColumn;
			if (gridColumn != null && gridColumn.ColumnSet != null)
			{
				gridColumn.ColumnSet.Remove(gridColumn);
			}
			for (int index = _formatConditions.Count - 1; index >= 0; index--)
			{
				GridFormatCondition gridFormatCondition = _formatConditions[index];
				if (gridFormatCondition.Column == gridColumn)
				{
					_formatConditions.RemoveAt(index);
				}
				else if (gridFormatCondition.TargetColumn == gridColumn)
				{
					gridFormatCondition.TargetColumn = null;
				}
			}
			for (int value = GroupHeaderTotals.Count - 1; value >= 0; value--)
			{
				GridGroupHeaderTotal gridGroupHeaderTotal = GroupHeaderTotals[value];
				if (gridGroupHeaderTotal.ColumnAlign == gridColumn)
				{
					gridGroupHeaderTotal.ColumnAlign = null;
				}
			}
			base.OnFieldRemoved(field);
		}

		internal bool ShouldHideColumnsWhenGrouped()
		{
			if (_hideColumnsWhenGrouped == InheritableBoolean.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.ShouldHideColumnsWhenGrouped();
				}
			}
			else if (_hideColumnsWhenGrouped == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool CanRemoveColumns()
		{
			switch (_allowRemoveColumns)
			{
			case InheritableBoolean.True:
				return true;
			case InheritableBoolean.False:
				return false;
			default:
				if (DataGrid != null)
				{
					return DataGrid.ShouldAllowRemoveColumns();
				}
				return false;
			}
		}

		internal void RetrieveBaseFields()
		{
			BaseRetrieveFields();
		}

		internal bool HasTableImage()
		{
			if (_imageIndex != -1)
			{
				return true;
			}
			if (_imageKey.Length > 0)
			{
				return true;
			}
			return false;
		}

		private bool AppendColumnClipHeaders(StringBuilder stringBuilder, char character, char value)
		{
			if (!AreColumnHeadersVisible() && !IsTableHeaderVisible())
			{
				return false;
			}
			if (IsTableHeaderVisible())
			{
				stringBuilder.Append(Caption);
				stringBuilder.Append(value);
			}
			if (AreColumnHeadersVisible())
			{
				GridColumn[] array = new GridColumn[Columns.Count];
				foreach (GridColumn column in Columns)
				{
					array[column.Position] = column;
				}
				bool condition = true;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Visible)
					{
						if (!condition)
						{
							stringBuilder.Append(character);
						}
						else
						{
							condition = false;
						}
						stringBuilder.Append('"');
						stringBuilder.Append(array[i].Caption);
						stringBuilder.Append('"');
					}
				}
			}
			return true;
		}

		private bool AppendColumnSetClipHeaders(StringBuilder stringBuilder, char character, char value)
		{
			if (!IsTableHeaderVisible() && !AreColumnHeadersVisible() && !AreColumnSetHeadersVisible())
			{
				return false;
			}
			bool condition = true;
			if (IsTableHeaderVisible())
			{
				stringBuilder.Append(Caption);
				condition = false;
			}
			GridColumnSet[] array = new GridColumnSet[_columnSets.Count];
			foreach (GridColumnSet item in _columnSets)
			{
				array[item._position] = item;
			}
			if (AreColumnSetHeadersVisible())
			{
				if (!condition)
				{
					stringBuilder.Append(value);
				}
				condition = false;
				int columnCount = 0;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Visible)
					{
						for (int j = 0; j < columnCount; j++)
						{
							stringBuilder.Append(character);
						}
						stringBuilder.Append(array[i].Caption);
						columnCount = array[i].ColumnCount;
					}
				}
			}
			if (AreColumnHeadersVisible())
			{
				for (int k = 0; k < ColumnSetRowCount; k++)
				{
					if (!condition)
					{
						stringBuilder.Append(value);
					}
					else
					{
						condition = false;
					}
					bool otherCondition = true;
					for (int n = 0; n < array.Length; n++)
					{
						if (!array[n].Visible)
						{
							continue;
						}
						for (int m = 0; m < array[n].ColumnCount; m++)
						{
							if (!otherCondition)
							{
								stringBuilder.Append(character);
							}
							else
							{
								otherCondition = false;
							}
							GridColumn gridColumn = array[n][k, m];
							if (gridColumn != null && gridColumn.Visible && gridColumn.ColumnSetRow == k && gridColumn.ColumnSetColumn == m)
							{
								stringBuilder.Append(gridColumn.Caption);
							}
						}
					}
				}
			}
			return true;
		}

		internal bool GetBaseIsSelfReferencingTable()
		{
			return base.BaseIsSelfReferencingTable;
		}
	}
}
