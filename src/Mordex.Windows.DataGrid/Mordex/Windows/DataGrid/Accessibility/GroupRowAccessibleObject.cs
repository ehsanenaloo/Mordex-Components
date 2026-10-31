using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class GroupRowAccessibleObject : RowAccessibleObject
	{
		private ExpandButtonAccessibleObject _expandButton;

		public override string Value => GetOwner().GetGridRowForLogicalRow(GetRow()).GroupCaption;

		public override string Name => "Group Row " + GetRow().ParentCollection.IndexOf(GetRow());

		public override AccessibleStates State
		{
			get
			{
				AccessibleStates accessibleStates = AccessibleStates.ReadOnly | AccessibleStates.Selectable;
				if (GetRow().Expanded)
				{
					return accessibleStates | AccessibleStates.Expanded;
				}
				return accessibleStates | AccessibleStates.Collapsed;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (!GetRow().Expanded)
				{
					return "Expand";
				}
				return "Collapse";
			}
		}

		internal GroupRowAccessibleObject(MxDataGrid grid, MordexRow row)
			: base(grid, row)
		{
		}

		public override int GetChildCount()
		{
			int childCount = 1;
			if (GetRow().Expanded)
			{
				childCount += GetRow().Rows.Count;
				if (GetOwnerTable().ComputeGroupTotals() != GroupTotals.Never)
				{
					childCount++;
				}
			}
			else if (GetOwnerTable().ComputeGroupTotals() == GroupTotals.Always)
			{
				childCount++;
			}
			return childCount;
		}

		private ExpandButtonAccessibleObject GetExpandButton()
		{
			if (_expandButton == null)
			{
				_expandButton = new ExpandButtonAccessibleObject(GetRow());
			}
			return _expandButton;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index == 0)
			{
				return GetExpandButton();
			}
			index--;
			if (GetRow().Expanded && index >= 0 && index < GetRow().Rows.Count)
			{
				return GetOwner().GetOrCreateRowAccessibleObject(GetRow().Rows[index]);
			}
			if (index == GetChildCount() - 2)
			{
				return GetOwner().GetOrCreateRowAccessibleObject(((MordexGroupRow)GetRow()).TotalRow);
			}
			return null;
		}

		public override void DoDefaultAction()
		{
			GridRow gridRow = GetOwner().GetGridRowForLogicalRow(GetRow());
			gridRow.Expanded = !gridRow.Expanded;
		}
	}
}
