using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class GroupByBoxInfoAccessibleObject : AccessibleObject
	{
		private MxDataGrid _dataGrid;

		private AccessibleObject _parent;

		public override AccessibleObject Parent => _parent;

		public override AccessibleRole Role => AccessibleRole.StaticText;

		public override AccessibleStates State => AccessibleStates.ReadOnly;

		public override string Name => "GroupByBoxInfo";

		public override string Value => _dataGrid.BuiltInTexts[GridBuiltInText.GroupByBoxInfo];

		public override Rectangle Bounds => _dataGrid.GetGroupByBoxInfoBounds(true);

		internal GroupByBoxInfoAccessibleObject(MxDataGrid dataGrid, AccessibleObject parent)
		{
			_dataGrid = dataGrid;
			_parent = parent;
		}
	}
}
