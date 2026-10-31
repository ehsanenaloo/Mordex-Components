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
		protected override MordexChildTableCollectionBase CreateChildTableCollection()
		{
			return new GridChildTableCollection(this);
		}

		private void NotifyPreviewRowChanged()
		{
			if (DataGrid != null)
			{
				DataGrid.RebuildRows(true);
			}
		}

		internal bool HasPreviewRow()
		{
			if (_previewRow)
			{
				return _previewRowLines > 0;
			}
			return false;
		}

		internal bool HasAnyHeaderRow()
		{
			if (AreColumnHeadersVisible())
			{
				return true;
			}
			if (IsTableHeaderVisible())
			{
				return true;
			}
			if (AreColumnSetHeadersVisible() && _cellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return true;
			}
			return false;
		}

		internal bool UsesChildTables()
		{
			if (ChildTables == null || DataGrid == null)
			{
				return false;
			}
			if (base.BaseHierarchicalMode == Mordex.Data.HierarchicalMode.UseChildTables && ChildTables.Count > 0 && DataGrid.Hierarchical)
			{
				return true;
			}
			return false;
		}

		internal bool HasChildTables()
		{
			if (ChildTables == null)
			{
				return false;
			}
			return ChildTables.Count != 0;
		}

		internal void RefreshGridRows()
		{
			DataGrid?.RefreshAfterTableDataChange(this);
		}

		internal bool AreGroupsExpandable()
		{
			if (_expandableGroups == InheritableBoolean.Default)
			{
				if (ParentTable != null)
				{
					return ParentTable.AreGroupsExpandable();
				}
				if (DataGrid != null)
				{
					return DataGrid.ShouldAllowExpandableGroups();
				}
			}
			else if (_expandableGroups == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal void RetrieveBaseChildTables()
		{
			BaseRetrieveChildTables();
		}

		internal bool AnyChildGroupsDefined()
		{
			foreach (GridTable childTable in ChildTables)
			{
				if (childTable.AnyGroupsDefined() || childTable.AnyChildGroupsDefined())
				{
					return true;
				}
			}
			return false;
		}

		internal string GetEffectiveCellToolTipText()
		{
			if (_cellToolTipContent == null)
			{
				if (DataGrid != null)
				{
					return DataGrid.CellToolTipText;
				}
				return "";
			}
			return _cellToolTipContent;
		}

		internal CellToolTip ComputeCellToolTip()
		{
			if (_cellToolTip == CellToolTip.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.CellToolTip;
				}
				return CellToolTip.UseCellToolTipText;
			}
			return _cellToolTip;
		}

		internal GridFormatStyle GetGroupRowFormatStyle(bool condition)
		{
			if (condition)
			{
				GridFormatStyle gridFormatStyle = new GridFormatStyle(null, PredefinedStyle.GroupRowFormatStyle);
				if (_dataGrid != null)
				{
					gridFormatStyle = gridFormatStyle.CloneWithDefaults(_dataGrid.DefaultAlphaLevel, _dataGrid.GetBackColorAlphaMode(), _dataGrid.GetForeColorAlphaMode(), _dataGrid.BlendColor);
					gridFormatStyle = gridFormatStyle.MergeWith(_dataGrid.GroupRowFormatStyle, false, false);
				}
				if (_groupRowFormatStyle != null)
				{
					gridFormatStyle = gridFormatStyle.ApplyOverrides(_groupRowFormatStyle);
				}
				return gridFormatStyle;
			}
			if (_dataGrid != null)
			{
				return _dataGrid.GetGroupRowStyle(this);
			}
			return _groupRowFormatStyle;
		}

		internal GridColumn GetExpandColumn()
		{
			if (GetBaseIsSelfReferencingTable() && base.BaseActAsSelfReferencingTable)
			{
				if (SelfReferencingSettings.ExpandColumn != null)
				{
					return SelfReferencingSettings.ExpandColumn;
				}
				for (int i = 0; i < Columns.Count; i++)
				{
					GridColumn columnInPosition = Columns.GetColumnInPosition(i);
					if (columnInPosition.Visible)
					{
						return columnInPosition;
					}
				}
			}
			return null;
		}

		internal RowHeaderContent ComputeRowHeaderContent()
		{
			if (_rowHeaderContent == RowHeaderContent.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.RowHeaderContent;
				}
				return RowHeaderContent.RowHeaderText;
			}
			return _rowHeaderContent;
		}

		internal bool UsesGroupRowSelector()
		{
			if (_useGroupRowSelector == InheritableBoolean.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.UseGroupRowSelector;
				}
			}
			else if (_useGroupRowSelector == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}
	}
}
