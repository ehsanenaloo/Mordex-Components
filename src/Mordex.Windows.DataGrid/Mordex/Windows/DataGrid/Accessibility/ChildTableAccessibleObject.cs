using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class ChildTableAccessibleObject : RowAccessibleObject
	{
		private HeaderRowAccessibleObject _headerRow;

		public override string Name => "Child Table " + GetOwnerTable().Caption;

		public override AccessibleRole Role => AccessibleRole.Table;

		internal ChildTableAccessibleObject(MxDataGrid owner, MordexRow row)
			: base(owner, row)
		{
		}

		public override int GetChildCount()
		{
			int childCount = GetRow().Rows.Count + 1;
			if (GetOwnerTable().IsAddNewAllowed())
			{
				childCount++;
			}
			if (GetOwnerTable().IsTotalRowVisible())
			{
				childCount++;
			}
			return childCount;
		}

		internal HeaderRowAccessibleObject GetHeaderRow()
		{
			if (_headerRow == null)
			{
				_headerRow = new HeaderRowAccessibleObject(GetOwner(), GetRow(), this);
			}
			return _headerRow;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index == 0)
			{
				return GetHeaderRow();
			}
			index--;
			if (GetOwnerTable().IsAddNewAllowed() && GetOwnerTable().ComputeNewRowPosition() == NewRowPosition.TopRow)
			{
				if (index == 0)
				{
					return GetOwner().GetOrCreateRowAccessibleObject(((MordexTableRow)GetRow()).NewRow);
				}
				index--;
			}
			if (index >= 0 && index < GetRow().Rows.Count)
			{
				return GetOwner().GetOrCreateRowAccessibleObject(GetRow().Rows[index]);
			}
			index -= GetRow().Rows.Count;
			if (GetOwnerTable().ComputeNewRowPosition() == NewRowPosition.BottomRow && GetOwnerTable().IsAddNewAllowed())
			{
				if (index == 0)
				{
					return GetOwner().GetOrCreateRowAccessibleObject(((MordexTableRow)GetRow()).NewRow);
				}
				index--;
			}
			if (GetOwnerTable().IsTotalRowVisible() && index == 0)
			{
				return GetOwner().GetOrCreateRowAccessibleObject(((MordexTableRow)GetRow()).TotalRow);
			}
			return null;
		}
	}
}
