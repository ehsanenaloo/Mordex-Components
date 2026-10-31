using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Navigation
{
	internal class ScrollButtonAccessibleObject : Control.ControlAccessibleObject
	{
		private NavigationBarControl _button;

		public override string DefaultAction => "Press";

		public override AccessibleObject Parent
		{
			get
			{
				if (_button != null && _button.GetOwner() != null)
				{
					return _button.GetOwner().AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				Control control = ((_button != null) ? _button.GetOwner() : null);
				if (_button != null)
				{
					if (control != null)
					{
						Point location = control.PointToScreen(_button.Bounds.Location);
						return new Rectangle(location, _button.Bounds.Size);
					}
					return _button.Bounds;
				}
				return base.Bounds;
			}
		}

		public override string Description
		{
			get
			{
				if (_button != null)
				{
					return _button.AccessibleDescription;
				}
				return base.Description;
			}
		}

		public override AccessibleRole Role => AccessibleRole.PushButton;

		public override AccessibleStates State
		{
			get
			{
				AccessibleStates accessibleStates = AccessibleStates.Selectable;
				if (!_button.Visible)
				{
					accessibleStates = AccessibleStates.Invisible;
				}
				else if (!_button.IsButtonEnabled())
				{
					accessibleStates = AccessibleStates.Unavailable;
				}
				else
				{
					accessibleStates = AccessibleStates.Focusable | AccessibleStates.Selectable;
					if (_button.IsHot())
					{
						accessibleStates |= AccessibleStates.HotTracked;
					}
					if (_button.IsPressed())
					{
						accessibleStates |= AccessibleStates.Pressed;
					}
				}
				return accessibleStates;
			}
		}

		public override string Name
		{
			get
			{
				if (_button._direction < 0)
				{
					return "Scroll up";
				}
				return "Scroll down";
			}
		}

		internal ScrollButtonAccessibleObject(NavigationBarControl button)
			: base(button)
		{
			_button = button;
		}

		public override void DoDefaultAction()
		{
			if (_button != null && _button.GetOwner() != null)
			{
				if (_button._direction < 0)
				{
					_button.GetOwner().ScrollToPreviousItem();
				}
				else
				{
					_button.GetOwner().ScrollToNextItem();
				}
			}
		}
	}
}
