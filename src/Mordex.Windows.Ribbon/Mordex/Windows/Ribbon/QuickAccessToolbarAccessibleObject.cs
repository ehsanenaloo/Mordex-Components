using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class QuickAccessToolbarAccessibleObject : Control.ControlAccessibleObject
	{
		private QuickAccessToolbarRenderer _quickAccessBar;

		public override AccessibleObject Parent
		{
			get
			{
				if (_quickAccessBar != null && _quickAccessBar.GetRibbon() != null)
				{
					return _quickAccessBar.GetRibbon().AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				if (_quickAccessBar != null && _quickAccessBar.GetRibbon() != null)
				{
					return new Rectangle(_quickAccessBar.GetRibbon().PointToScreen(_quickAccessBar._rectangle.Location), _quickAccessBar._rectangle.Size);
				}
				return base.Bounds;
			}
		}

		public override string Name => "Quick Access Toolbar";

		public override AccessibleRole Role => AccessibleRole.ToolBar;

		public override AccessibleStates State => AccessibleStates.None;

		public QuickAccessToolbarAccessibleObject(QuickAccessToolbarRenderer quickAccessBar)
			: base(quickAccessBar.GetRibbon())
		{
			_quickAccessBar = quickAccessBar;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_quickAccessBar._itemRenderers != null)
			{
				childCount = _quickAccessBar._itemRenderers.Count;
			}
			if (_quickAccessBar._customizeButton != null)
			{
				childCount++;
			}
			if (_quickAccessBar._overflowButton != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_quickAccessBar._itemRenderers != null)
			{
				if (index >= 0 && index < _quickAccessBar._itemRenderers.Count)
				{
					return _quickAccessBar._itemRenderers[index].AccessibilityObject;
				}
				index -= _quickAccessBar._itemRenderers.Count;
			}
			if (index >= 0)
			{
				if (index == 0)
				{
					if (_quickAccessBar._customizeButton != null)
					{
						return _quickAccessBar._customizeButton.AccessibilityObject;
					}
					if (_quickAccessBar._overflowButton != null)
					{
						return _quickAccessBar._overflowButton.AccessibilityObject;
					}
				}
				if (index == 1 && _quickAccessBar._overflowButton != null)
				{
					return _quickAccessBar._overflowButton.AccessibilityObject;
				}
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_quickAccessBar.GetRibbon() != null)
			{
				Point pt = _quickAccessBar.GetRibbon().PointToClient(new Point(screenX, screenY));
				CommandBaseRenderer itemAt = _quickAccessBar.GetItemAt(pt);
				if (itemAt != null)
				{
					return itemAt.AccessibilityObject;
				}
			}
			return this;
		}
	}
}
