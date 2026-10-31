using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class ExpandButtonAccessibleObject : AccessibleObject
	{
		private MordexRow _parentRow;

		public override AccessibleStates State
		{
			get
			{
				AccessibleStates result = base.State;
				GridTable gridTable = (GridTable)_parentRow.Table;
				if (_parentRow is MordexGroupRow)
				{
					if (!gridTable.AreGroupsExpandable())
					{
						result = AccessibleStates.Unavailable;
					}
				}
				else if (_parentRow is MordexDataRow && !gridTable.UsesChildTables())
				{
					result = AccessibleStates.Unavailable;
				}
				return result;
			}
		}

		public override string Description => "Expand Button";

		public override Rectangle Bounds
		{
			get
			{
				MxDataGrid dataGrid = ((GridTable)_parentRow.Table).DataGrid;
				if (dataGrid != null)
				{
					if (_parentRow is MordexGroupRow)
					{
						return dataGrid.GetAccessibleItemBounds(dataGrid.GetOrCreateRowAccessibleObject(_parentRow) as RowAccessibleObject, null, false, LayoutItemType.GroupExpandBox);
					}
					return dataGrid.GetAccessibleItemBounds(dataGrid.GetOrCreateRowAccessibleObject(_parentRow) as RowAccessibleObject, null, false, LayoutItemType.ExpandBox);
				}
				return base.Bounds;
			}
		}

		public override string Name => "ExpandButton";

		public override AccessibleRole Role => AccessibleRole.PushButton;

		public override string DefaultAction
		{
			get
			{
				if (_parentRow.Expanded)
				{
					return "Collapse";
				}
				return "Expand";
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				try
				{
					return ((GridTable)_parentRow.Table).DataGrid.GetOrCreateRowAccessibleObject(_parentRow);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					return null;
				}
			}
		}

		internal ExpandButtonAccessibleObject(MordexRow parentRow)
		{
			_parentRow = parentRow;
		}

		private MxDataGrid ComputeDataGrid()
		{
			if (_parentRow != null && _parentRow.Table != null)
			{
				return ((GridTable)_parentRow.Table).DataGrid;
			}
			return null;
		}

		public override void DoDefaultAction()
		{
			ComputeDataGrid()?.ToggleRowExpanded(_parentRow);
		}
	}
}
