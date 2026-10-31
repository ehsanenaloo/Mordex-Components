using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class MxRibbonAccessibleObject : Control.ControlAccessibleObject
	{
		private MxRibbon _ribbon;

		public override string Name
		{
			get
			{
				string accessibleName = _ribbon.AccessibleName;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				accessibleName = _ribbon.Text;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				return _ribbon.Name;
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
				if (_ribbon.AccessibleRole != AccessibleRole.Default)
				{
					return _ribbon.AccessibleRole;
				}
				return AccessibleRole.PropertyPage;
			}
		}

		public override AccessibleStates State => AccessibleStates.None;

		public MxRibbonAccessibleObject(MxRibbon ribbon)
			: base(ribbon)
		{
			_ribbon = ribbon;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_ribbon._quickAccessToolbarRenderer != null)
			{
				childCount++;
			}
			if (_ribbon._ribbonWindowMessageHandler != null)
			{
				childCount++;
				if (_ribbon._ribbonWindowMessageHandler._minimizeButton != null)
				{
					childCount++;
				}
				if (_ribbon._ribbonWindowMessageHandler._maximizeButton != null)
				{
					childCount++;
				}
				if (_ribbon._ribbonWindowMessageHandler._closeButton != null)
				{
					childCount++;
				}
				if (_ribbon._ribbonWindowMessageHandler._helpButton != null)
				{
					childCount++;
				}
			}
			if (_ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxButton && _ribbon._controlBoxButtonRenderer != null)
			{
				childCount++;
			}
			if (_ribbon._tabStripRenderer != null)
			{
				childCount++;
				if (_ribbon._tabStripRenderer.GetMinimizeButtonAccessibleObject() != null)
				{
					childCount++;
				}
				if (_ribbon._tabStripRenderer.GetMdiRestoreButtonAccessibleObject() != null)
				{
					childCount++;
				}
				if (_ribbon._tabStripRenderer.GetMdiMinimizeButtonAccessibleObject() != null)
				{
					childCount++;
				}
				if (_ribbon._tabStripRenderer.GetMdiCloseButtonAccessibleObject() != null)
				{
					childCount++;
				}
			}
			if (_ribbon._tabContentRenderer != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			List<AccessibleObject> list = new List<AccessibleObject>();
			if (_ribbon._quickAccessToolbarRenderer != null)
			{
				list.Add(_ribbon._quickAccessToolbarRenderer.GetAccessibleObject());
			}
			if (_ribbon._ribbonWindowMessageHandler != null)
			{
				list.Add(_ribbon._ribbonWindowMessageHandler.GetAccessibleObject());
				if (_ribbon._ribbonWindowMessageHandler._minimizeButton != null)
				{
					list.Add(_ribbon._ribbonWindowMessageHandler._minimizeButton.AccessibilityObject);
				}
				if (_ribbon._ribbonWindowMessageHandler._maximizeButton != null)
				{
					list.Add(_ribbon._ribbonWindowMessageHandler._maximizeButton.AccessibilityObject);
				}
				if (_ribbon._ribbonWindowMessageHandler._closeButton != null)
				{
					list.Add(_ribbon._ribbonWindowMessageHandler._closeButton.AccessibilityObject);
				}
				if (_ribbon._ribbonWindowMessageHandler._helpButton != null)
				{
					list.Add(_ribbon._ribbonWindowMessageHandler._helpButton.AccessibilityObject);
				}
			}
			if (_ribbon.GetEffectiveControlBoxMode() == ControlBoxMode.ControlBoxButton && _ribbon._controlBoxButtonRenderer != null)
			{
				list.Add(_ribbon._controlBoxButtonRenderer.AccessibilityObject);
			}
			if (_ribbon._tabStripRenderer != null)
			{
				list.Add(_ribbon._tabStripRenderer.GetAccessibleObject());
				if (_ribbon._tabStripRenderer.GetMinimizeButtonAccessibleObject() != null)
				{
					list.Add(_ribbon._tabStripRenderer.GetMinimizeButtonAccessibleObject());
				}
				// Same order as on screen: minimize, restore, close.
				if (_ribbon._tabStripRenderer.GetMdiMinimizeButtonAccessibleObject() != null)
				{
					list.Add(_ribbon._tabStripRenderer.GetMdiMinimizeButtonAccessibleObject());
				}
				if (_ribbon._tabStripRenderer.GetMdiRestoreButtonAccessibleObject() != null)
				{
					list.Add(_ribbon._tabStripRenderer.GetMdiRestoreButtonAccessibleObject());
				}
				if (_ribbon._tabStripRenderer.GetMdiCloseButtonAccessibleObject() != null)
				{
					list.Add(_ribbon._tabStripRenderer.GetMdiCloseButtonAccessibleObject());
				}
			}
			if (_ribbon._tabContentRenderer != null)
			{
				list.Add(_ribbon._tabContentRenderer.GetAccessibleObject());
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			Point pt = _ribbon.PointToClient(new Point(screenX, screenY));
			if (_ribbon._quickAccessToolbarRenderer != null && _ribbon._quickAccessToolbarRenderer._rectangle.Contains(pt))
			{
				return _ribbon._quickAccessToolbarRenderer.GetAccessibleObject();
			}
			if (_ribbon._ribbonWindowMessageHandler != null)
			{
				if (_ribbon._ribbonWindowMessageHandler._minimizeButton != null && _ribbon._ribbonWindowMessageHandler._minimizeButton._bounds.Contains(pt))
				{
					return _ribbon._ribbonWindowMessageHandler._minimizeButton.AccessibilityObject;
				}
				if (_ribbon._ribbonWindowMessageHandler._maximizeButton != null && _ribbon._ribbonWindowMessageHandler._maximizeButton._bounds.Contains(pt))
				{
					return _ribbon._ribbonWindowMessageHandler._maximizeButton.AccessibilityObject;
				}
				if (_ribbon._ribbonWindowMessageHandler._closeButton != null && _ribbon._ribbonWindowMessageHandler._closeButton._bounds.Contains(pt))
				{
					return _ribbon._ribbonWindowMessageHandler._closeButton.AccessibilityObject;
				}
				if (_ribbon._ribbonWindowMessageHandler._helpButton != null && _ribbon._ribbonWindowMessageHandler._helpButton._bounds.Contains(pt))
				{
					return _ribbon._ribbonWindowMessageHandler._helpButton.AccessibilityObject;
				}
				if (_ribbon._headerRenderer != null && _ribbon._headerRenderer.GetArea().Contains(pt))
				{
					return _ribbon._ribbonWindowMessageHandler.GetAccessibleObject();
				}
			}
			if (_ribbon._controlBoxButtonRenderer != null && _ribbon._controlBoxButtonRenderer._bounds.Contains(pt))
			{
				return _ribbon._controlBoxButtonRenderer.AccessibilityObject;
			}
			if (_ribbon._tabStripRenderer != null && _ribbon._tabStripRenderer._stripBounds.Contains(pt))
			{
				return _ribbon._tabStripRenderer.GetAccessibleObject();
			}
			if (_ribbon._tabContentRenderer != null && _ribbon._tabContentRenderer._bounds.Contains(pt))
			{
				return _ribbon._tabContentRenderer.GetAccessibleObject();
			}
			return this;
		}
	}
}
