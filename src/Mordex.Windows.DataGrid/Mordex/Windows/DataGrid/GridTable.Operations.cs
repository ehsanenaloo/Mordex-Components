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
		private void InitializeCollections()
		{
			BaseResetCollections();
			if (_columnSets != null)
			{
				_columnSets._ownerTable = null;
				_columnSets.Clear();
			}
			_columnSets = new GridColumnSetCollection(this);
			if (_formatConditions != null)
			{
				_formatConditions._ownerTable = null;
				_formatConditions.Clear();
			}
			_formatConditions = new GridFormatConditionCollection(this);
			if (_groupCondition != null)
			{
				_groupCondition._ownerTable = null;
			}
			_groupCondition = new GridFormatCondition();
			_groupCondition._ownerTable = this;
			_groupCondition._requiresTableRefresh = true;
		}

		protected override MordexConditionBase CreateCondition()
		{
			return new GridFilterCondition();
		}

		protected override MordexGroupCollectionBase CreateGroupsCollection()
		{
			return new GridGroupCollection(this);
		}

		protected override MordexCustomGroupCollectionBase CreateCustomGroupCollection()
		{
			return new GridCustomGroupCollection(this);
		}

		internal void ClearOrphanDropDowns()
		{
			foreach (GridColumn column in Columns)
			{
				if (column.DropDown != null && column.DropDown._ownerCollection == null)
				{
					column.DropDown = null;
				}
			}
			foreach (GridTable childTable in ChildTables)
			{
				childTable.ClearOrphanDropDowns();
			}
		}

		internal void RemoveInvalidSortKeysAndGroups()
		{
			ArrayList arrayList = new ArrayList();
			foreach (GridSortKey sortKey in SortKeys)
			{
				if (sortKey.Column == null || sortKey.Column.GetOwnerColumnCollection() == null)
				{
					arrayList.Add(sortKey);
				}
			}
			foreach (GridSortKey item in arrayList)
			{
				SortKeys.Remove(item);
			}
			arrayList.Clear();
			foreach (GridGroup group in Groups)
			{
				if (group.CustomGroup == null && (group.Column == null || group.Column.GetOwnerColumnCollection() == null))
				{
					arrayList.Add(group);
				}
			}
			foreach (GridGroup arrayListItem in arrayList)
			{
				Groups.Remove(arrayListItem);
			}
			arrayList.Clear();
			foreach (GridFormatCondition formatCondition in _formatConditions)
			{
				if (formatCondition.TargetColumn != null && formatCondition.TargetColumn.GetOwnerColumnCollection() == null)
				{
					formatCondition.TargetColumn = null;
				}
				if ((formatCondition.Column == null || formatCondition.Column.GetOwnerColumnCollection() == null) && (formatCondition.FilterCondition == null || !formatCondition.IsValidForTable(this)))
				{
					arrayList.Add(formatCondition);
				}
			}
			foreach (GridFormatCondition otherArrayListItem in arrayList)
			{
				_formatConditions.Remove(otherArrayListItem);
			}
			if (FilterApplied != null)
			{
				FilterApplied.ClearInvalidColumnReferences();
			}
			foreach (GridStoredFilter storedFilter in StoredFilters)
			{
				if (storedFilter.FilterCondition != null)
				{
					storedFilter.FilterCondition.ClearInvalidColumnReferences();
				}
			}
			foreach (GridCustomGroup customGroup in CustomGroups)
			{
				customGroup.RemoveOrphanedFields();
			}
			foreach (GridTable childTable in ChildTables)
			{
				childTable.RemoveInvalidSortKeysAndGroups();
			}
		}

		internal bool IsHierarchical()
		{
			if (ChildTables == null || DataGrid == null)
			{
				return false;
			}
			if (HierarchicalMode == HierarchicalMode.SelfReferencing)
			{
				return true;
			}
			if (ChildTables.Count != 0)
			{
				return DataGrid.Hierarchical;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGridLineColor()
		{
			GridLineColor = Color.Empty;
		}

		internal GridLines GetEffectiveGridLines()
		{
			if (_gridLines == GridLines.Default)
			{
				return DataGrid?.ResolveGridLines() ?? GridLines.Both;
			}
			return _gridLines;
		}

		private Color GetEffectiveGridLineColor()
		{
			if (_lineColor.Equals(Color.Empty))
			{
				if (DataGrid != null)
				{
					return DataGrid.ResolveGridLineColor();
				}
				return SystemColors.ControlDark;
			}
			return _lineColor;
		}

		internal bool IsAddNewAllowed()
		{
			switch (AllowAddNew)
			{
			case InheritableBoolean.False:
				return false;
			case InheritableBoolean.True:
				return true;
			default:
				if (DataGrid != null)
				{
					return DataGrid.GetAllowAddNew();
				}
				return false;
			}
		}

		internal bool IsEditAllowed()
		{
			switch (_allowEdit)
			{
			case InheritableBoolean.False:
				return false;
			case InheritableBoolean.True:
				return true;
			default:
				if (DataGrid != null)
				{
					return DataGrid.ShouldAllowEdit();
				}
				return true;
			}
		}

		internal bool IsDeleteAllowed()
		{
			switch (_allowDelete)
			{
			case InheritableBoolean.False:
				return false;
			case InheritableBoolean.True:
				return true;
			default:
				if (DataGrid != null)
				{
					return DataGrid.ShouldAllowDelete();
				}
				return false;
			}
		}

		internal bool AreHeadersRepeated()
		{
			if (_repeatHeaders == InheritableBoolean.Default)
			{
				if (DataGrid != null)
				{
					return DataGrid.ShouldRepeatHeaders();
				}
			}
			else if (_repeatHeaders == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal int GetPrintSectionCount()
		{
			if (_printLayout != null)
			{
				return _printLayout._lineCount;
			}
			return 0;
		}

		internal PropertyDescriptor[] GetBaseProperties(bool includeFields, bool includeChildLists)
		{
			return BaseGetProperties(includeFields, includeChildLists);
		}

		internal bool AnyGroupsDefined()
		{
			if (Groups.Count > 0)
			{
				return true;
			}
			return false;
		}

		internal bool AppendClipHeaders(StringBuilder stringBuilder, char character, char otherCharacter)
		{
			if (_cellLayoutMode == CellLayoutMode.UseColumns)
			{
				return AppendColumnClipHeaders(stringBuilder, character, otherCharacter);
			}
			return AppendColumnSetClipHeaders(stringBuilder, character, otherCharacter);
		}

		internal void DropDefaultFormatStyles()
		{
			if (!GridHelper.IsStyleAttached(_alternatingRowFormatStyle))
			{
				_alternatingRowFormatStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_groupByBoxInfoFormatStyle))
			{
				_groupByBoxInfoFormatStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_rowFormatStyle))
			{
				_rowFormatStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_headerFormatStyle))
			{
				_headerFormatStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_rowHeaderFormatStyle))
			{
				_rowHeaderFormatStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_groupRowFormatStyle))
			{
				_groupRowFormatStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_totalRowFormatStyle))
			{
				_totalRowFormatStyle = null;
			}
			if (!GridHelper.IsStyleAttached(_groupTotalRowFormatStyle))
			{
				_groupTotalRowFormatStyle = null;
			}
			foreach (GridColumn column in Columns)
			{
				column.ClearDisposedStyles();
			}
			foreach (GridColumnSet item in _columnSets)
			{
				item.UpdateHeaderStyle();
			}
			foreach (GridFormatCondition formatCondition in _formatConditions)
			{
				formatCondition.ClearUnusedFormatStyle();
			}
			foreach (GridTable childTable in ChildTables)
			{
				childTable.DropDefaultFormatStyles();
			}
		}
	}
}
