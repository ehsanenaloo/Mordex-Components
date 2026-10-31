using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonTabStripRenderer
	{
		internal AccessibleObject GetAccessibleObject()
		{
			if (_cachedAccessibleObject == null)
			{
				_cachedAccessibleObject = new RibbonTabStripAccessibleObject(this);
			}
			return _cachedAccessibleObject;
		}

		internal AccessibleObject GetMinimizeButtonAccessibleObject()
		{
			if (_minimizeButton != null)
			{
				return _minimizeButton.AccessibilityObject;
			}
			return null;
		}

		internal AccessibleObject GetMdiRestoreButtonAccessibleObject()
		{
			if (_mdiControlBox != null)
			{
				return _mdiControlBox.GetRestoreAccessibleObject();
			}
			return null;
		}

		internal AccessibleObject GetMdiMinimizeButtonAccessibleObject()
		{
			if (_mdiControlBox != null)
			{
				return _mdiControlBox.GetMinimizeAccessibleObject();
			}
			return null;
		}

		internal AccessibleObject GetMdiCloseButtonAccessibleObject()
		{
			if (_mdiControlBox != null)
			{
				return _mdiControlBox.GetCloseAccessibleObject();
			}
			return null;
		}
	}
}
