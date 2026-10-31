using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class PushButtonAccessibleObject : Control.ControlAccessibleObject
	{
		private MxButton _button;

		public override string Name
		{
			get
			{
				if (_button.AccessibleName != null && _button.AccessibleName.Length > 0)
				{
					return _button.AccessibleName;
				}
				if (_button.Text != null && _button.Text.Length > 0)
				{
					return _button.Text;
				}
				return _button.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override string Description => _button.AccessibleDescription;

		public override AccessibleRole Role
		{
			get
			{
				if (_button.AccessibleRole != AccessibleRole.Default)
				{
					return _button.AccessibleRole;
				}
				if (_button.ResolveButtonStyle() == ButtonStyle.DropDownButton)
				{
					return AccessibleRole.SplitButton;
				}
				if (_button.ResolveButtonStyle() == ButtonStyle.DropDown)
				{
					return AccessibleRole.ButtonDropDown;
				}
				return AccessibleRole.PushButton;
			}
		}

		public override AccessibleStates State => _button.GetAccessibleStates();

		public override string DefaultAction
		{
			get
			{
				if (_button.AccessibleDefaultActionDescription != null && _button.AccessibleDefaultActionDescription.Length > 0)
				{
					return _button.AccessibleDefaultActionDescription;
				}
				if (Role == AccessibleRole.ButtonDropDown)
				{
					return "Open";
				}
				return "Press";
			}
		}

		public PushButtonAccessibleObject(MxButton button)
			: base(button)
		{
			_button = button;
		}

		public override void DoDefaultAction()
		{
			if (Role == AccessibleRole.ButtonDropDown)
			{
				_button.DropDown();
			}
			else
			{
				_button.PerformClick();
			}
		}

		public override int GetChildCount()
		{
			if (Role == AccessibleRole.SplitButton)
			{
				return 1;
			}
			return 0;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index == 0 && Role == AccessibleRole.SplitButton)
			{
				return _button.GetOrCreateAccessibleObject();
			}
			return null;
		}
	}
}
