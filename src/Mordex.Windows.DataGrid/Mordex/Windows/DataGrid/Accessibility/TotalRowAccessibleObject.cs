using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class TotalRowAccessibleObject : RowAccessibleObject
	{
		private AccessibleObject[] _accessibilityState5AccessibleObjectArray;

		public override string Name => "TotalRow";

		public override AccessibleStates State => AccessibleStates.Selectable;

		internal TotalRowAccessibleObject(MxDataGrid gridOwner, MordexRow dataRow)
			: base(gridOwner, dataRow)
		{
		}

		public override int GetChildCount()
		{
			return GetOwnerTable().Columns.Count + 1;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_accessibilityState5AccessibleObjectArray == null)
			{
				_accessibilityState5AccessibleObjectArray = new AccessibleObject[GetOwnerTable().Columns.Count + 1];
			}
			if (index >= 0 && index < _accessibilityState5AccessibleObjectArray.Length)
			{
				GridColumn gridColumn = null;
				if (index > 0)
				{
					gridColumn = GetOwnerTable().Columns.GetColumnInPosition(index - 1);
					index = gridColumn.Index + 1;
				}
				if (_accessibilityState5AccessibleObjectArray[index] == null)
				{
					_accessibilityState5AccessibleObjectArray[index] = new GridColumnAccessibleObject(GetRow(), gridColumn);
				}
				return _accessibilityState5AccessibleObjectArray[index];
			}
			return null;
		}
	}
}
