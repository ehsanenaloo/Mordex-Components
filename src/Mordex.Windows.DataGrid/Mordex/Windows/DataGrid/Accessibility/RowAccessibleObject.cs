using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal abstract class RowAccessibleObject : AccessibleObject
	{
		private MordexRow _row;

		private MxDataGrid _owner;

		public override Rectangle Bounds => _owner.GetAccessibleCellBounds(this, null, true);

		public override AccessibleRole Role => AccessibleRole.Row;

		public override AccessibleObject Parent => GetOwner().GetParentAccessibleObject(_row);

		protected RowAccessibleObject(MxDataGrid owner, MordexRow row)
		{
			_row = row;
			_owner = owner;
		}

		internal MxDataGrid GetOwner()
		{
			return _owner;
		}

		internal MordexRow GetRow()
		{
			return _row;
		}

		internal GridTable GetOwnerTable()
		{
			return (GridTable)_row.Table;
		}

		public override void Select(AccessibleSelection flags)
		{
			if (flags == AccessibleSelection.None)
			{
				MxDataGrid dataGrid = GetOwner();
				if (dataGrid != null && _row != null)
				{
					GridRow gridRow = dataGrid.GetGridRowForLogicalRow(_row);
					dataGrid.EnsureVisible(gridRow.Position);
					dataGrid.HorizontalScrollPosition = 0;
				}
			}
			base.Select(flags);
		}

		internal AccessibleObject ComputeAccessibleObject(GridColumn gridColumn)
		{
			int childCount = GetChildCount();
			for (int i = 0; i < childCount; i++)
			{
				if (GetChild(i) is GridColumnAccessibleObject gridColumnAccessibleObject && gridColumnAccessibleObject.GetCol() == gridColumn)
				{
					return gridColumnAccessibleObject;
				}
			}
			return null;
		}

		internal AccessibleObject FindRowHeader()
		{
			int childCount = GetChildCount();
			for (int i = 0; i < childCount; i++)
			{
				if (GetChild(i) is GridColumnAccessibleObject gridColumnAccessibleObject && gridColumnAccessibleObject.Role == AccessibleRole.RowHeader)
				{
					return gridColumnAccessibleObject;
				}
			}
			return null;
		}
	}
}
