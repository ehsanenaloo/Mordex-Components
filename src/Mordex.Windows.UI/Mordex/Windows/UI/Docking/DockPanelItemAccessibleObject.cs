using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal class DockPanelItemAccessibleObject : AccessibleObject
	{
		private DockTabItem _tabItem;

		public override string Name
		{
			get
			{
				if (_tabItem != null)
				{
					return _tabItem.GetAccessibleName();
				}
				return base.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override AccessibleRole Role => AccessibleRole.PageTab;

		public override AccessibleStates State
		{
			get
			{
				if (_tabItem != null)
				{
					return _tabItem.GetAccessibleStates();
				}
				return base.State;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				Control control = ((_tabItem != null) ? _tabItem.GetParentControl() : null);
				if (_tabItem != null)
				{
					if (control != null)
					{
						Point p = _tabItem.GetBounds().Location;
						if (_tabItem.GetParentControl() is MxDockPanelGroup uiPanelGroup)
						{
							p = uiPanelGroup.GetTabLocation(_tabItem);
						}
						p = _tabItem.GetParentControl().PointToScreen(p);
						return new Rectangle(p, _tabItem.GetBounds().Size);
					}
					return _tabItem.GetBounds();
				}
				return base.Bounds;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (_tabItem.GetDefaultActionName() != null)
				{
					return _tabItem.GetDefaultActionName();
				}
				return "Press";
			}
		}

		public DockPanelItemAccessibleObject(DockTabItem panelItem)
		{
			_tabItem = panelItem;
		}

		public override void DoDefaultAction()
		{
			if (_tabItem != null)
			{
				_tabItem.SelectPanel();
			}
			else
			{
				base.DoDefaultAction();
			}
		}
	}
}
