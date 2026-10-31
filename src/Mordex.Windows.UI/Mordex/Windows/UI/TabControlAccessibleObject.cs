using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI
{
	internal class TabControlAccessibleObject : Control.ControlAccessibleObject
	{
		private MxTabControl _tabControl;

		public override AccessibleRole Role
		{
			get
			{
				if (_tabControl.AccessibleRole != AccessibleRole.Default)
				{
					return _tabControl.AccessibleRole;
				}
				return AccessibleRole.PageTabList;
			}
		}

		public TabControlAccessibleObject(MxTabControl tabControl)
			: base(tabControl)
		{
			_tabControl = tabControl;
		}

		public override int GetChildCount()
		{
			List<AccessibleObject> caption = _tabControl.GetCaptionAccessibleObjects();
			if (caption != null)
			{
				return caption.Count + (_tabControl.SelectedTab != null ? 1 : 0);
			}
			int childCount = 0;
			foreach (TabPageRenderer item in _tabControl._pageRenderers)
			{
				_ = item;
				childCount++;
			}
			if (_tabControl._scrollBackButton != null && _tabControl._scrollBackButton.IsVisible())
			{
				childCount++;
			}
			if (_tabControl._scrollForwardButton != null && _tabControl._scrollForwardButton.IsVisible())
			{
				childCount++;
			}
			if (_tabControl._closeButton != null && _tabControl._closeButton.IsVisible())
			{
				childCount++;
			}
			if (_tabControl.SelectedTab != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			// The tabs in the title bar have objects of their own, since the strip of the control has no renderers then.
			List<AccessibleObject> caption = _tabControl.GetCaptionAccessibleObjects();
			if (caption != null)
			{
				if (index >= 0 && index < caption.Count)
				{
					return caption[index];
				}
				return index == caption.Count && _tabControl.SelectedTab != null ? _tabControl.SelectedTab.AccessibilityObject : null;
			}
			List<AccessibleObject> list = new List<AccessibleObject>();
			foreach (TabPageRenderer item in _tabControl._pageRenderers)
			{
				list.Add(item.AccessibleObject);
			}
			if (_tabControl._scrollBackButton != null && _tabControl._scrollBackButton.IsVisible())
			{
				list.Add(_tabControl._scrollBackButton.AccessibleObject);
			}
			if (_tabControl._scrollForwardButton != null && _tabControl._scrollForwardButton.IsVisible())
			{
				list.Add(_tabControl._scrollForwardButton.AccessibleObject);
			}
			if (_tabControl._closeButton != null && _tabControl._closeButton.IsVisible())
			{
				list.Add(_tabControl._closeButton.AccessibleObject);
			}
			if (_tabControl.SelectedTab != null)
			{
				list.Add(_tabControl.SelectedTab.AccessibilityObject);
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_tabControl != null && _tabControl.CaptionTabsActive)
			{
				AccessibleObject part = _tabControl.CaptionTabs.AccessibleHitTest(new Point(screenX, screenY));
				if (part != null)
				{
					return part;
				}
			}
			if (_tabControl != null)
			{
				Point point = _tabControl.PointToClient(new Point(screenX, screenY));
				FormatStyleLayoutState uiFormatStyleLayoutState = _tabControl.HitTestElement(point.X, point.Y);
				if (uiFormatStyleLayoutState != null)
				{
					return uiFormatStyleLayoutState.AccessibleObject;
				}
				if (_tabControl.SelectedTab != null)
				{
					if (_tabControl.SelectedTab.ClientRectangle.Contains(new Point(screenX, screenY)))
					{
						return _tabControl.SelectedTab.AccessibilityObject;
					}
				}
			}
			return this;
		}
	}
}
