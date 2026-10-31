using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation
{
	internal class MxNavigationBarAccessibleObject : Control.ControlAccessibleObject
	{
		private MxNavigationBar _navigationBar;

		public override string Name
		{
			get
			{
				string accessibleName = _navigationBar.AccessibleName;
				if (accessibleName != null)
				{
					return accessibleName;
				}
				accessibleName = _navigationBar.Text;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				return _navigationBar.Name;
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
				if (_navigationBar.AccessibleRole != AccessibleRole.Default)
				{
					return _navigationBar.AccessibleRole;
				}
				return AccessibleRole.Pane;
			}
		}

		public override AccessibleStates State => AccessibleStates.Focusable;

		public MxNavigationBarAccessibleObject(MxNavigationBar navigationBar)
			: base(navigationBar)
		{
			_navigationBar = navigationBar;
		}

		public override int GetChildCount()
		{
			int childCount = _navigationBar.Groups.Count;
			if (_navigationBar._scrollNextButton != null)
			{
				childCount++;
			}
			if (_navigationBar._scrollPreviousButton != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index >= 0 && index < _navigationBar.Groups.Count)
			{
				return _navigationBar.Groups[index].GetAccessibleObject();
			}
			index -= _navigationBar.Groups.Count;
			if (_navigationBar._scrollPreviousButton != null)
			{
				if (index == 0)
				{
					return _navigationBar._scrollPreviousButton.AccessibilityObject;
				}
				index--;
			}
			if (_navigationBar._scrollNextButton != null && index == 0)
			{
				return _navigationBar._scrollNextButton.AccessibilityObject;
			}
			return null;
		}

		public override AccessibleObject HitTest(int integer, int integerArgument)
		{
			if (_navigationBar != null)
			{
				Point clientPoint = _navigationBar.PointToClient(new Point(integer, integerArgument));
				if (_navigationBar._scrollNextButton != null && _navigationBar._scrollNextButton.Bounds.Contains(clientPoint))
				{
					return _navigationBar._scrollNextButton.AccessibilityObject;
				}
				if (_navigationBar._scrollPreviousButton != null && _navigationBar._scrollPreviousButton.Bounds.Contains(clientPoint))
				{
					return _navigationBar._scrollPreviousButton.AccessibilityObject;
				}
				foreach (NavigationGroup group in _navigationBar.Groups)
				{
					if (group.Bounds.Contains(clientPoint))
					{
						return group.GetAccessibleObject();
					}
					if (group.Selected)
					{
						if (group.Container && group.ContainerControl != null && group.ContainerControl.Visible && group.ContainerControl.Bounds.Contains(clientPoint))
						{
							return group.ContainerControl.AccessibilityObject;
						}
						Rectangle navigationBarBounds = _navigationBar._itemsAreaBounds;
						if (navigationBarBounds.Contains(clientPoint))
						{
							return group.GetAccessibleObject();
						}
					}
				}
			}
			return base.HitTest(integer, integerArgument);
		}
	}
}
