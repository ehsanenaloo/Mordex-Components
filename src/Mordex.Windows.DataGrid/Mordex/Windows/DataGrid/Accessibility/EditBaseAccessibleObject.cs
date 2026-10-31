using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Editors;

namespace Mordex.Windows.DataGrid.Accessibility
{
	internal class EditBaseAccessibleObject : AccessibleObject
	{
		private EditBase _owner;

		private int _buttonIndex;

		public override Rectangle Bounds => _owner.GetButtonScreenBounds(_buttonIndex);

		public override string Name
		{
			get
			{
				if (Role == AccessibleRole.SpinButton)
				{
					if (_buttonIndex == 0)
					{
						return "SpinUp";
					}
					return "SpinDown";
				}
				return "EditButton";
			}
		}

		public override AccessibleRole Role => _owner.GetButtonAccessibleRole();

		public override string DefaultAction => "Press";

		public override AccessibleObject Parent => _owner.GetAccessibilityObjectOrBase();

		internal EditBaseAccessibleObject(EditBase owner)
		{
			_owner = owner;
			_buttonIndex = 0;
		}

		internal EditBaseAccessibleObject(EditBase owner, int buttonIndex)
		{
			_owner = owner;
			_buttonIndex = buttonIndex;
		}

		public override void DoDefaultAction()
		{
			_owner.DoButtonClick(_buttonIndex);
		}
	}
}
