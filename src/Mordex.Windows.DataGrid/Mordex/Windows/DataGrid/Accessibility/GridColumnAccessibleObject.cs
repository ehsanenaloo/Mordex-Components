using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class GridColumnAccessibleObject : AccessibleObject, IAccessibilityElement
	{
		private GridColumn _col;

		private MordexRow _parentRow;

		private CellButtonAccessibleObject _child;

		private CellButtonAccessibleObject _gridColumnAccessibleObjectChild;

		private CellButtonAccessibleObject _upButton;

		private CellButtonAccessibleObject _downButton;

		public override string Description
		{
			get
			{
				if (_col == null)
				{
					return "Row Header";
				}
				if (!string.IsNullOrEmpty(_col.Caption))
				{
					return _col.Caption;
				}
				if (!string.IsNullOrEmpty(_col.Key))
				{
					return _col.Key;
				}
				return "Cell_" + _col.Index;
			}
		}

		public override Rectangle Bounds => ((GridTable)_parentRow.Table).DataGrid?.GetAccessibleCellBounds(Parent as RowAccessibleObject, _col, false) ?? base.Bounds;

		public override string Name
		{
			get
			{
				string text = "";
				AccessibleObject parent = Parent;
				if (parent != null)
				{
					text = parent.Name + "_";
				}
				if (_col == null)
				{
					return text + "RowHeader";
				}
				if (!string.IsNullOrEmpty(_col.Key))
				{
					return text + _col.Key;
				}
				return text + "Cell_" + _col.Index;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_col == null)
				{
					return AccessibleRole.RowHeader;
				}
				return AccessibleRole.Cell;
			}
		}

		public override string Value
		{
			get
			{
				if (_col != null)
				{
					MxDataGrid dataGrid = _col.DataGrid;
					GridRow gridRow = dataGrid.GetGridRowForLogicalRow(_parentRow);
					if (gridRow == dataGrid.CurrentRow && _col == dataGrid.CurrentColumn)
					{
						object value = dataGrid.GetValue(_col);
						if ((_col.EditType == EditType.DatePicker || _col.EditType == EditType.CalendarDropDown) && value is DateTime dateTime)
						{
							return dateTime.ToString("MM/dd/yyyy");
						}
						return string.Concat(value);
					}
					return gridRow.Cells[_col].Text;
				}
				if (Role == AccessibleRole.RowHeader)
				{
					return "";
				}
				return base.Value;
			}
			set
			{
				if (_col != null)
				{
					MxDataGrid dataGrid = _col.DataGrid;
					GridRow gridRow = dataGrid.GetGridRowForLogicalRow(_parentRow);
					if (gridRow == dataGrid.CurrentRow && _col == dataGrid.CurrentColumn)
					{
						dataGrid.SetColumnEditText(_col, value);
						return;
					}
				}
				throw new InvalidOperationException("Value in cell can only be set for current cell.");
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (!(_parentRow is MordexTotalRow))
				{
					if (_col == null)
					{
						return "Select";
					}
					if (_col.Visible)
					{
						MxDataGrid dataGrid = _col.Table.DataGrid;
						GridRow gridRow = dataGrid.GetGridRowForLogicalRow(_parentRow);
						GridCell gridCell = gridRow.Cells[_col.Index];
						if (gridCell != null && gridCell.EditType != EditType.NoEdit && _col.Table.IsEditAllowed())
						{
							return "Edit";
						}
					}
				}
				return base.DefaultAction;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (_col != null)
				{
					if (_col.Visible)
					{
						AccessibleStates accessibleStates = AccessibleStates.None;
						if (_col.Selectable && !(_parentRow is MordexTotalRow))
						{
							if (IsRowHeader())
							{
								return accessibleStates | AccessibleStates.Selectable | AccessibleStates.Focused | AccessibleStates.ReadOnly;
							}
							return accessibleStates | AccessibleStates.Selectable | AccessibleStates.Focusable;
						}
						return accessibleStates | AccessibleStates.Selectable | AccessibleStates.ReadOnly;
					}
					return AccessibleStates.Invisible;
				}
				if (((GridTable)_parentRow.Table).AreRowHeadersVisible())
				{
					if (_parentRow is MordexTotalRow && _parentRow.ParentRow is MordexGroupRow)
					{
						return AccessibleStates.Invisible;
					}
					return AccessibleStates.Selectable;
				}
				return AccessibleStates.Invisible;
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

		GridColumn IAccessibilityElement.Column => _col;

		internal GridColumnAccessibleObject(MordexRow parentRow, GridColumn col)
		{
			_col = col;
			_parentRow = parentRow;
			if (_col != null)
			{
				GridCell gridCell = GetCell();
				if (gridCell.ButtonStyle != ButtonStyle.NoButton)
				{
					_child = new CellButtonAccessibleObject(parentRow, col, LayoutItemType.CellColumnButton);
				}
				switch (gridCell.EditType)
				{
				case EditType.IntegerUpDown:
				case EditType.IntegerUpDownCombo:
				case EditType.ValueListUpDown:
				case EditType.ValueListUpDownCombo:
					_downButton = new CellButtonAccessibleObject(parentRow, col, LayoutItemType.CellDownButton);
					_upButton = new CellButtonAccessibleObject(parentRow, col, LayoutItemType.CellUpButton);
					break;
				case EditType.CalendarDropDown:
				case EditType.DatePicker:
				case EditType.Combo:
				case EditType.DropDownList:
				case EditType.MultiColumnCombo:
				case EditType.MultiColumnDropDown:
					_gridColumnAccessibleObjectChild = new CellButtonAccessibleObject(parentRow, col, LayoutItemType.CellDropDownButton);
					break;
				case EditType.CheckBox:
					break;
				}
			}
		}

		internal GridColumn GetCol()
		{
			return _col;
		}

		public override void Select(AccessibleSelection flags)
		{
			if (flags == AccessibleSelection.None && _col != null)
			{
				MxDataGrid dataGrid = _col.DataGrid;
				if (dataGrid != null)
				{
					GridCell gridCell = GetCell();
					if (gridCell != null)
					{
						dataGrid.EnsureVisible(gridCell.Row.Position, _col);
						return;
					}
				}
			}
			base.Select(flags);
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

		private bool IsRowHeader()
		{
			if (_col != null)
			{
				return false;
			}
			return true;
		}

		private GridCell GetCell()
		{
			if (_col != null)
			{
				MxDataGrid dataGrid = _col.DataGrid;
				if (dataGrid != null && _parentRow != null)
				{
					GridRow gridRow = dataGrid.GetGridRowForLogicalRow(_parentRow);
					if (gridRow != null)
					{
						return gridRow.Cells[_col];
					}
				}
			}
			return null;
		}

		internal CellButtonAccessibleObject GetButtonAccessibleObject(LayoutItemType layoutItemType)
		{
			switch (layoutItemType)
			{
			case LayoutItemType.CellColumnButton:
			case LayoutItemType.FilterClearButton:
				return _child;
			case LayoutItemType.CellDropDownButton:
				return _gridColumnAccessibleObjectChild;
			case LayoutItemType.CellUpButton:
				return _upButton;
			case LayoutItemType.CellDownButton:
				return _downButton;
			default:
				return null;
			}
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_child != null)
			{
				childCount++;
			}
			if (_gridColumnAccessibleObjectChild != null)
			{
				childCount++;
			}
			if (_upButton != null)
			{
				childCount++;
			}
			if (_downButton != null)
			{
				childCount++;
			}
			if (IsCellBeingEdited())
			{
				childCount++;
			}
			return childCount;
		}

		private bool IsCellBeingEdited()
		{
			if (_col != null)
			{
				MxDataGrid dataGrid = _col.DataGrid;
				if (dataGrid != null && dataGrid.EditMode == EditMode.EditOn && dataGrid.CurrentColumn == _col && dataGrid.CurrentRow != null && dataGrid.CurrentRow.GetLogicalRow() == _parentRow)
				{
					return true;
				}
			}
			return false;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_child != null)
			{
				if (index == 0)
				{
					return _child;
				}
				index--;
			}
			if (_gridColumnAccessibleObjectChild != null)
			{
				if (index == 0)
				{
					return _gridColumnAccessibleObjectChild;
				}
				index--;
			}
			if (_upButton != null)
			{
				if (index == 0)
				{
					return _upButton;
				}
				index--;
			}
			if (_downButton != null)
			{
				if (index == 0)
				{
					return _downButton;
				}
				index--;
			}
			if (index == 0 && IsCellBeingEdited())
			{
				MxDataGrid dataGrid = _col.DataGrid;
				return dataGrid.GetEditorAccessibleObject();
			}
			return base.GetChild(index);
		}
	}
}
