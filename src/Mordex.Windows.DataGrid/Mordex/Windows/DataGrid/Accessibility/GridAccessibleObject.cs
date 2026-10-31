using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class GridAccessibleObject : AccessibleObject
	{
		private MxDataGrid _dataGrid;

		private GridGroup[] _groups;

		private GridGroupAccessibleObject[] _groupAccessibleObjects;

		private GroupByBoxInfoAccessibleObject _groupByBoxInfo;

		public override string Name => "GroupByBox";

		public override Rectangle Bounds => _dataGrid.GetGroupByBoxInfoBounds(false);

		public override AccessibleStates State
		{
			get
			{
				if (!_dataGrid.GroupByBoxVisible)
				{
					return AccessibleStates.Invisible;
				}
				return base.State;
			}
		}

		public override AccessibleRole Role => AccessibleRole.Pane;

		public override AccessibleObject Parent => _dataGrid._tableAccessibleObject;

		internal GridAccessibleObject(MxDataGrid dataGrid)
		{
			_dataGrid = dataGrid;
		}

		internal void SetGroups(GridGroup[] gridGroupArray)
		{
			if (gridGroupArray != null && gridGroupArray.Length == 0)
			{
				gridGroupArray = null;
			}
			if (!AreGroupsEqual(_groups, gridGroupArray))
			{
				_groups = gridGroupArray;
				RebuildGroupAccessibleObjects();
				if (_dataGrid != null)
				{
					((Control.ControlAccessibleObject)_dataGrid.AccessibilityObject).NotifyClients(AccessibleEvents.Reorder);
				}
			}
		}

		private void RebuildGroupAccessibleObjects()
		{
			if (_groups == null)
			{
				_groupAccessibleObjects = null;
				return;
			}
			_groupAccessibleObjects = new GridGroupAccessibleObject[_groups.Length];
			for (int i = 0; i < _groups.Length; i++)
			{
				_groupAccessibleObjects[i] = new GridGroupAccessibleObject(this, _groups[i]);
			}
		}

		private bool AreGroupsEqual(GridGroup[] gridGroupArray, GridGroup[] groupArray)
		{
			if (gridGroupArray == null && groupArray == null)
			{
				return true;
			}
			if (gridGroupArray != null || groupArray != null)
			{
				return false;
			}
			if (gridGroupArray.Length != groupArray.Length)
			{
				return false;
			}
			for (int i = 0; i < gridGroupArray.Length; i++)
			{
				if (gridGroupArray[i] != groupArray[1])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetChildCount()
		{
			if (_groups == null)
			{
				return 1;
			}
			return _groups.Length;
		}

		private GroupByBoxInfoAccessibleObject GetGroupByBoxInfo()
		{
			if (_groupByBoxInfo == null)
			{
				_groupByBoxInfo = new GroupByBoxInfoAccessibleObject(_dataGrid, this);
			}
			return _groupByBoxInfo;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_groupAccessibleObjects == null)
			{
				if (index == 0)
				{
					return GetGroupByBoxInfo();
				}
			}
			else if (index >= 0 && index < _groupAccessibleObjects.Length)
			{
				return _groupAccessibleObjects[index];
			}
			return null;
		}
	}
}
