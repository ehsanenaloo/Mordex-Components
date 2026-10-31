using System.Collections.Generic;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI
{
	internal class MdiTabGroupAccessibleObject : Control.ControlAccessibleObject
	{
		private MxMdiTabGroup _tabGroup;

		public override string Name
		{
			get
			{
				string accessibleName = _tabGroup.AccessibleName;
				if (accessibleName != null)
				{
					return accessibleName;
				}
				accessibleName = _tabGroup.Text;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				return _tabGroup.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_tabGroup.AccessibleRole != AccessibleRole.Default)
				{
					return _tabGroup.AccessibleRole;
				}
				return AccessibleRole.PageTabList;
			}
		}

		public MdiTabGroupAccessibleObject(MxMdiTabGroup tabGroup)
			: base(tabGroup)
		{
			_tabGroup = tabGroup;
		}

		public override int GetChildCount()
		{
			return GetVisibleTabs().Count;
		}

		public override AccessibleObject GetChild(int index)
		{
			List<MdiTabItem> tabs = GetVisibleTabs();
			if (index >= 0 && index < tabs.Count)
			{
				return tabs[index].GetAccessibleObject();
			}
			return null;
		}

		private List<MdiTabItem> GetVisibleTabs()
		{
			List<MdiTabItem> tabs = new List<MdiTabItem>();
			MdiTabStripLayout layoutState = _tabGroup.GetGroupLayout();
			if (layoutState == null)
			{
				return tabs;
			}
			foreach (MdiTabItem item in layoutState._tabs)
			{
				tabs.Add(item);
			}
			AddIfVisible(tabs, layoutState.GetScrollLeftButton());
			AddIfVisible(tabs, layoutState.GetScrollRightButton());
			if (!layoutState._showCloseButtonOnSelectedTab)
			{
				AddIfVisible(tabs, layoutState.GetCloseButton());
			}
			AddIfVisible(tabs, layoutState.GetActiveFilesButton());
			return tabs;
		}

		private static void AddIfVisible(List<MdiTabItem> tabs, MdiTabItem tab)
		{
			if (tab != null && tab.GetIsVisible())
			{
				tabs.Add(tab);
			}
		}
	}
}
