using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonLayoutItemAccessibleObject : AccessibleObject
	{
		private RibbonLayoutItem _item;

		public override string DefaultAction
		{
			get
			{
				if (_item != null)
				{
					return _item.DefaultAction;
				}
				return base.DefaultAction;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (_item is CommandBaseRenderer commandBaseRenderer)
				{
					if (commandBaseRenderer._layoutItemOwner != null)
					{
						return commandBaseRenderer._layoutItemOwner.AccessibilityObject;
					}
					if (commandBaseRenderer._commandBaseRendererOwner != null)
					{
						return commandBaseRenderer._commandBaseRendererOwner.AccessibilityObject;
					}
				}
				if (_item != null && _item._parentControl != null)
				{
					return _item._parentControl.AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				Control control = ((_item != null) ? _item._parentControl : null);
				if (_item != null)
				{
					if (control != null)
					{
						Point location = control.PointToScreen(_item._bounds.Location);
						return new Rectangle(location, _item._bounds.Size);
					}
					return _item._bounds;
				}
				return base.Bounds;
			}
		}

		public override string Description
		{
			get
			{
				if (_item != null)
				{
					return _item.AccessibleDescription;
				}
				return base.Description;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_item != null)
				{
					return _item.AccessibleRole;
				}
				return base.Role;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (_item != null)
				{
					return _item.AccessibleState;
				}
				return base.State;
			}
		}

		public override string Name
		{
			get
			{
				if (_item != null)
				{
					return _item.AccessibleName;
				}
				return base.Name;
			}
		}

		internal RibbonLayoutItemAccessibleObject(RibbonLayoutItem item)
		{
			_item = item;
		}

		public override void DoDefaultAction()
		{
			if (_item != null)
			{
				_item.DoDefaultAction();
			}
			else
			{
				base.DoDefaultAction();
			}
		}
	}
}
