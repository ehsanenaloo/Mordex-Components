using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonTabStripAccessibleObject : Control.ControlAccessibleObject
	{
		private RibbonTabStripRenderer _tabStripRenderer;

		public override Rectangle Bounds
		{
			get
			{
				if (_tabStripRenderer != null && _tabStripRenderer.GetRibbon() != null)
				{
					return new Rectangle(_tabStripRenderer.GetRibbon().PointToScreen(_tabStripRenderer._stripBounds.Location), _tabStripRenderer._stripBounds.Size);
				}
				return base.Bounds;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (_tabStripRenderer != null && _tabStripRenderer.GetRibbon() != null)
				{
					return _tabStripRenderer.GetRibbon().AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override string Name => "Ribbon Tabs";

		public override AccessibleRole Role => AccessibleRole.PageTabList;

		public override AccessibleStates State => AccessibleStates.None;

		public RibbonTabStripAccessibleObject(RibbonTabStripRenderer tabsLayoutHandler)
			: base(tabsLayoutHandler.GetRibbon())
		{
			_tabStripRenderer = tabsLayoutHandler;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_tabStripRenderer.GetTabRenderers() != null)
			{
				childCount += _tabStripRenderer.GetTabRenderers().Count;
			}
			if (_tabStripRenderer._leftScrollButton != null)
			{
				childCount++;
			}
			if (_tabStripRenderer._rightScrollButton != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_tabStripRenderer.GetTabRenderers() != null)
			{
				if (index >= 0 && index < _tabStripRenderer.GetTabRenderers().Count)
				{
					return _tabStripRenderer.GetTabRenderers()[index].AccessibilityObject;
				}
				index -= _tabStripRenderer.GetTabRenderers().Count;
			}
			if (index >= 0)
			{
				if (index == 0)
				{
					if (_tabStripRenderer._leftScrollButton != null)
					{
						return _tabStripRenderer._leftScrollButton.AccessibilityObject;
					}
					if (_tabStripRenderer._rightScrollButton != null)
					{
						return _tabStripRenderer._rightScrollButton.AccessibilityObject;
					}
				}
				if (index == 1 && _tabStripRenderer._rightScrollButton != null)
				{
					return _tabStripRenderer._rightScrollButton.AccessibilityObject;
				}
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_tabStripRenderer != null && _tabStripRenderer.GetRibbon() != null)
			{
				Point point = _tabStripRenderer.GetRibbon().PointToClient(new Point(screenX, screenY));
				RibbonTabRenderer ribbonTabRenderer = _tabStripRenderer.GetTabAt(point.X, point.Y);
				if (ribbonTabRenderer != null)
				{
					return ribbonTabRenderer.AccessibilityObject;
				}
			}
			return this;
		}
	}
}
