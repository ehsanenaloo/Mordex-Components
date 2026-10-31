using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class HeaderRowAccessibleObject : RowAccessibleObject
	{
		private AccessibleObject _parent;

		private AccessibleObject[] _accessibilityState3AccessibleObjectArray;

		public override Rectangle Bounds => GetOwner().GetAccessibleColumnHeaderBounds(this, null, true);

		public override string Name => "Header Row " + GetOwnerTable().Caption;

		public override AccessibleObject Parent => _parent;

		public override AccessibleStates State
		{
			get
			{
				GridTable gridTable = GetOwnerTable();
				bool condition = false;
				if (gridTable != null)
				{
					if (gridTable.IsTableHeaderVisible())
					{
						condition = true;
					}
					if (!condition && gridTable.AreColumnHeadersVisible())
					{
						condition = true;
					}
					if (!condition && gridTable.CellLayoutMode == CellLayoutMode.UseColumnSets && gridTable.AreColumnSetHeadersVisible())
					{
						condition = true;
					}
				}
				if (!condition)
				{
					return AccessibleStates.Invisible;
				}
				return base.State;
			}
		}

		internal HeaderRowAccessibleObject(MxDataGrid gridOwner, MordexRow tableRow, AccessibleObject parent)
			: base(gridOwner, tableRow)
		{
			_parent = parent;
		}

		public override int GetChildCount()
		{
			return GetOwnerTable().Columns.Count + 1;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_accessibilityState3AccessibleObjectArray == null)
			{
				_accessibilityState3AccessibleObjectArray = new AccessibleObject[GetOwnerTable().Columns.Count + 1];
			}
			if (index >= 0 && index < _accessibilityState3AccessibleObjectArray.Length)
			{
				if (_accessibilityState3AccessibleObjectArray[index] == null)
				{
					GridColumn col = null;
					if (index > 0)
					{
						col = GetOwnerTable().Columns[index - 1];
					}
					_accessibilityState3AccessibleObjectArray[index] = new ColumnHeaderAccessibleObject(GetRow(), col);
				}
				return _accessibilityState3AccessibleObjectArray[index];
			}
			return null;
		}
	}
}
