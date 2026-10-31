using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class AccessibilityState : RowAccessibleObject
	{
		private ExpandButtonAccessibleObject _child;

		private AccessibleObject[] _columnAccessibleObjects;

		public override string Name
		{
			get
			{
				if (GetRow() == null)
				{
					return "";
				}
				if (GetRow() is MordexNewDataRow)
				{
					return "AddNewRow";
				}
				return "Row" + GetRow().ParentCollection.IndexOf(GetRow());
			}
		}

		public override AccessibleStates State => AccessibleStates.Selectable;

		internal AccessibilityState(MxDataGrid gridOwner, MordexRow dataRow)
			: base(gridOwner, dataRow)
		{
		}

		public override int GetChildCount()
		{
			int childCount = GetOwnerTable().Columns.Count + 1;
			if (GetOwner().Hierarchical && GetOwnerTable().HierarchicalMode == HierarchicalMode.UseChildTables && GetOwnerTable().ChildTables.Count > 0)
			{
				childCount++;
				if (GetRow().Expanded)
				{
					childCount += GetOwnerTable().ChildTables.Count;
				}
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (GetOwner().Hierarchical && GetOwnerTable().HierarchicalMode == HierarchicalMode.UseChildTables && GetOwnerTable().ChildTables.Count > 0)
			{
				if (index == 0)
				{
					if (_child == null)
					{
						_child = new ExpandButtonAccessibleObject(GetRow());
					}
					return _child;
				}
				index--;
			}
			if (_columnAccessibleObjects == null)
			{
				_columnAccessibleObjects = new AccessibleObject[GetOwnerTable().Columns.Count + 1];
			}
			if (index >= 0 && index < _columnAccessibleObjects.Length)
			{
				GridColumn gridColumn = null;
				if (index > 0)
				{
					gridColumn = GetOwnerTable().Columns.GetColumnInPosition(index - 1);
					index = gridColumn.Index + 1;
				}
				if (_columnAccessibleObjects[index] == null)
				{
					_columnAccessibleObjects[index] = new GridColumnAccessibleObject(GetRow(), gridColumn);
				}
				return _columnAccessibleObjects[index];
			}
			index -= _columnAccessibleObjects.Length;
			if (GetOwner().Hierarchical && GetRow().Expanded && GetOwnerTable().HierarchicalMode == HierarchicalMode.UseChildTables)
			{
				if (index >= 0 && index < GetOwnerTable().ChildTables.Count)
				{
					return GetOwner().GetOrCreateRowAccessibleObject(GetRow().Rows[index]);
				}
				index -= GetOwnerTable().ChildTables.Count;
			}
			return null;
		}
	}
}
