using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class BackstageMenuAccessibleObject : AccessibleObject
	{
		private BackstageMenu _backstageMenu;

		public override string DefaultAction
		{
			get
			{
				if (GetOwnerRenderer() != null)
				{
					return GetOwnerRenderer().DefaultAction;
				}
				return base.DefaultAction;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (GetOwnerRenderer() != null)
				{
					if (GetOwnerRenderer()._layoutItemOwner != null)
					{
						return GetOwnerRenderer()._layoutItemOwner.AccessibilityObject;
					}
					if (GetOwnerRenderer()._commandBaseRendererOwner != null)
					{
						return GetOwnerRenderer()._commandBaseRendererOwner.AccessibilityObject;
					}
					if (GetOwnerRenderer()._parentControl != null)
					{
						return GetOwnerRenderer()._parentControl.AccessibilityObject;
					}
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				if (GetOwnerRenderer() != null)
				{
					Control parentControl = GetOwnerRenderer()._parentControl;
					if (parentControl != null)
					{
						Point location = parentControl.PointToScreen(GetOwnerRenderer()._bounds.Location);
						return new Rectangle(location, GetOwnerRenderer()._bounds.Size);
					}
					return GetOwnerRenderer()._bounds;
				}
				return base.Bounds;
			}
		}

		public override string Description
		{
			get
			{
				if (_backstageMenu != null && _backstageMenu.AccessibleDescription != null && _backstageMenu.AccessibleDescription.Length > 0)
				{
					return _backstageMenu.AccessibleDescription;
				}
				return base.Description;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (GetOwnerRenderer() != null)
				{
					return GetOwnerRenderer().AccessibleRole;
				}
				return _backstageMenu.AccessibleRole;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (GetOwnerRenderer() != null)
				{
					return GetOwnerRenderer().AccessibleState;
				}
				return AccessibleStates.Invisible | AccessibleStates.Focusable;
			}
		}

		public override string Name
		{
			get
			{
				if (_backstageMenu != null)
				{
					if (_backstageMenu.AccessibleName != null && _backstageMenu.AccessibleName.Length > 0)
					{
						return _backstageMenu.AccessibleName;
					}
					return _backstageMenu.Name;
				}
				return base.Name;
			}
		}

		internal BackstageMenuAccessibleObject(BackstageMenu backstageMenu)
		{
			_backstageMenu = backstageMenu;
		}

		internal CommandBaseRenderer GetOwnerRenderer()
		{
			return _backstageMenu._owner;
		}

		public override void DoDefaultAction()
		{
			if (GetOwnerRenderer() != null)
			{
				GetOwnerRenderer().DoDefaultAction();
			}
			else
			{
				_backstageMenu.DropDown();
			}
		}

		public override int GetChildCount()
		{
			return _backstageMenu.Commands.Count;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index >= 0 && index < _backstageMenu.Commands.Count)
			{
				return _backstageMenu.Commands[index].AccessibilityObject;
			}
			return null;
		}
	}
}
