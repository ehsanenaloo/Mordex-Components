using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class CellButtonAccessibleObject : AccessibleObject
	{
		private LayoutItemType _buttonType;

		private GridColumn _col;

		private MordexRow _parentRow;

		public override string Description => _col.Caption + " Button";

		public override Rectangle Bounds
		{
			get
			{
				MxDataGrid dataGrid = ((GridTable)_parentRow.Table).DataGrid;
				return dataGrid?.GetAccessibleItemBounds(dataGrid.GetOrCreateRowAccessibleObject(_parentRow) as RowAccessibleObject, _col, false, _buttonType) ?? base.Bounds;
			}
		}

		public override string Name => "Cell Button";

		public override AccessibleRole Role
		{
			get
			{
				if (_buttonType == LayoutItemType.CellDropDownButton)
				{
					return AccessibleRole.ButtonDropDown;
				}
				if (_buttonType == LayoutItemType.CellUpButton || _buttonType == LayoutItemType.CellDownButton)
				{
					return AccessibleRole.SpinButton;
				}
				return AccessibleRole.PushButton;
			}
		}

		public override string DefaultAction => "Press";

		public override AccessibleObject Parent
		{
			get
			{
				try
				{
					return ((GridTable)_parentRow.Table).DataGrid.ComputeAccessibleObject(_parentRow, _col);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					return null;
				}
			}
		}

		internal CellButtonAccessibleObject(MordexRow parentRow, GridColumn col, LayoutItemType buttonType)
		{
			_col = col;
			_parentRow = parentRow;
			_buttonType = buttonType;
		}

		public override void DoDefaultAction()
		{
			if (!(_parentRow is MordexTotalRow))
			{
				if (_col != null)
				{
					MxDataGrid dataGrid = _col.DataGrid;
					GridRow row = dataGrid.GetGridRowForLogicalRow(_parentRow);
					dataGrid.MoveTo(row);
					dataGrid.CurrentColumn = _col;
					dataGrid.Focus();
					_col.DataGrid.EditMode = EditMode.EditOn;
				}
				else if (State != AccessibleStates.Invisible)
				{
					MxDataGrid otherDataGrid = ((GridTable)_parentRow.Table).DataGrid;
					otherDataGrid.MoveTo(otherDataGrid.GetGridRowForLogicalRow(_parentRow));
				}
			}
		}
	}
}
