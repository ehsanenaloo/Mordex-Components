using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class CommandBaseAccessibleObject : AccessibleObject
	{
		protected CommandBase _command;

		public override string DefaultAction
		{
			get
			{
				if (GetOwner() != null)
				{
					return GetOwner().DefaultAction;
				}
				return base.DefaultAction;
			}
		}

		public override AccessibleObject Parent
		{
			get
			{
				if (_command.CommandsOwner != null)
				{
					return _command.CommandsOwner._parent.AccessibilityObject;
				}
				if (GetOwner() != null)
				{
					if (GetOwner()._layoutItemOwner != null)
					{
						return GetOwner()._layoutItemOwner.AccessibilityObject;
					}
					if (GetOwner()._commandBaseRendererOwner != null)
					{
						return GetOwner()._commandBaseRendererOwner.AccessibilityObject;
					}
					if (GetOwner()._parentControl != null)
					{
						return GetOwner()._parentControl.AccessibilityObject;
					}
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				if (GetOwner() != null)
				{
					Control parentControl = GetOwner()._parentControl;
					if (parentControl != null)
					{
						Point location = parentControl.PointToScreen(GetOwner()._bounds.Location);
						return new Rectangle(location, GetOwner()._bounds.Size);
					}
					return GetOwner()._bounds;
				}
				return base.Bounds;
			}
		}

		public override string Description
		{
			get
			{
				if (_command != null && _command.AccessibleDescription != null && _command.AccessibleDescription.Length > 0)
				{
					return _command.AccessibleDescription;
				}
				return base.Description;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (GetOwner() != null)
				{
					return GetOwner().AccessibleRole;
				}
				return _command.Role;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (GetOwner() != null)
				{
					return GetOwner().AccessibleState;
				}
				return AccessibleStates.Invisible | AccessibleStates.Focusable;
			}
		}

		public override string Name
		{
			get
			{
				if (_command != null)
				{
					if (_command.AccessibleName != null && _command.AccessibleName.Length > 0)
					{
						return _command.AccessibleName;
					}
					return _command.Name;
				}
				return base.Name;
			}
		}

		internal CommandBaseAccessibleObject(CommandBase command)
		{
			_command = command;
		}

		internal CommandBaseRenderer GetOwner()
		{
			return _command._renderer;
		}

		public override void DoDefaultAction()
		{
			if (GetOwner() != null)
			{
				GetOwner().DoDefaultAction();
			}
			else
			{
				_command.InvokeOnClick();
			}
		}
	}
}
