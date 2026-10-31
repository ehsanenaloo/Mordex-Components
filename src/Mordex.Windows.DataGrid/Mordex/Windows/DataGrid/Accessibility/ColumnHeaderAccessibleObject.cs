using System.Drawing;
using System.Windows.Forms;
using Mordex.Data;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class ColumnHeaderAccessibleObject : AccessibleObject, IAccessibilityElement
	{
		private GridColumn _col;

		private MordexRow _parentRow;

		public override string Name
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
				return "Column Header";
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
				return AccessibleRole.ColumnHeader;
			}
		}

		public override string Value
		{
			get
			{
				if (_col != null)
				{
					return _col.Caption;
				}
				return base.Value;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (_col != null && _col.DataGrid.AutomaticSort)
				{
					if (_col.SortOrder == SortOrder.Ascending)
					{
						return "Sort Descending";
					}
					return "Sort Ascending";
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
						return AccessibleStates.Selectable;
					}
					return AccessibleStates.Invisible;
				}
				MxDataGrid dataGrid = ((GridTable)_parentRow.Table).DataGrid;
				if (dataGrid == null)
				{
					return AccessibleStates.Invisible;
				}
				if (dataGrid.View == View.TableView)
				{
					if (((GridTable)_parentRow.Table).AreRowHeadersVisible())
					{
						return AccessibleStates.None;
					}
					return AccessibleStates.Invisible;
				}
				return AccessibleStates.None;
			}
		}

		public override AccessibleObject Parent => ((_col == null) ? ((GridTable)_parentRow.Table).DataGrid : _col.DataGrid)?.GetTableAccessibleObject(_parentRow);

		public override Rectangle Bounds
		{
			get
			{
				MxDataGrid dataGrid = ((_col == null) ? ((GridTable)_parentRow.Table).DataGrid : _col.DataGrid);
				if (dataGrid != null)
				{
					HeaderRowAccessibleObject parent = Parent as HeaderRowAccessibleObject;
					return dataGrid.GetAccessibleColumnHeaderBounds(parent, _col, false);
				}
				return base.Bounds;
			}
		}

		GridColumn IAccessibilityElement.Column => _col;

		internal ColumnHeaderAccessibleObject(MordexRow parentRow, GridColumn col)
		{
			_col = col;
			_parentRow = parentRow;
		}

		public override void DoDefaultAction()
		{
			if (_col == null || !_col.DataGrid.AutomaticSort)
			{
				return;
			}
			if (_col.IsGrouped)
			{
				if (_col.Group.SortOrder == SortOrder.Ascending)
				{
					_col.Group.SortOrder = SortOrder.Descending;
				}
				else
				{
					_col.Group.SortOrder = SortOrder.Ascending;
				}
			}
			else if (_col.IsSorted)
			{
				if (_col.SortKey.SortOrder == SortOrder.Ascending)
				{
					_col.SortKey.SortOrder = SortOrder.Descending;
				}
				else
				{
					_col.SortKey.SortOrder = SortOrder.Ascending;
				}
			}
			else
			{
				_col.Table.SortKeys.Clear();
				_col.Table.SortKeys.Add(_col);
			}
		}

		public override void Select(AccessibleSelection flags)
		{
			if (flags == AccessibleSelection.None && _col != null)
			{
				MxDataGrid dataGrid = _col.DataGrid;
				if (dataGrid != null)
				{
					if (_col.Table == dataGrid.RootTable)
					{
						dataGrid.ScrollColumnIntoView(_col);
						return;
					}
					if (Parent is HeaderRowAccessibleObject accessibilityState)
					{
						if (accessibilityState.Parent is ChildTableAccessibleObject otherAccessibilityState)
						{
							int childCount = otherAccessibilityState.GetChildCount();
							((childCount != 1) ? (otherAccessibilityState.GetChild(1) as RowAccessibleObject) : (otherAccessibilityState.Parent as RowAccessibleObject))?.Select(AccessibleSelection.None);
						}
						dataGrid.ScrollColumnIntoView(_col);
					}
				}
			}
			base.Select(flags);
		}
	}
}
