using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class GridGroupAccessibleObject : AccessibleObject
	{
		private GridGroup _group;

		private GridAccessibleObject _parent;

		public override AccessibleObject Parent => _parent;

		public override Rectangle Bounds => _group.ComputeDataGrid()?.GetGroupByBoxHeaderBounds(_group) ?? base.Bounds;

		public override string DefaultAction
		{
			get
			{
				if (_group != null && _group.ComputeDataGrid() != null)
				{
					if (_group.SortOrder == SortOrder.Ascending)
					{
						return "Sort Descending";
					}
					return "Sort Ascending";
				}
				return base.DefaultAction;
			}
		}

		public override string Name
		{
			get
			{
				if (_group != null && _group.Column != null)
				{
					return "Group " + _group.Column.Caption;
				}
				return "Group " + _group.Index;
			}
		}

		public override AccessibleRole Role => AccessibleRole.PushButton;

		internal GridGroupAccessibleObject(GridAccessibleObject parent, GridGroup group)
		{
			_parent = parent;
			_group = group;
		}

		public override void DoDefaultAction()
		{
			if (_group != null && _group.ComputeDataGrid() != null)
			{
				if (_group.SortOrder == SortOrder.Ascending)
				{
					_group.SortOrder = SortOrder.Descending;
				}
				else
				{
					_group.SortOrder = SortOrder.Ascending;
				}
			}
			base.DoDefaultAction();
		}
	}
}
