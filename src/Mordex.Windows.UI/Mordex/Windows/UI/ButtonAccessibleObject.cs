using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class ButtonAccessibleObject : AccessibleObject
	{
		private MxButton _button;

		public override AccessibleObject Parent => _button.AccessibilityObject;

		public override AccessibleRole Role => AccessibleRole.ButtonDropDown;

		public override string Description => _button.AccessibleDescription;

		public override Rectangle Bounds
		{
			get
			{
				if (_button != null)
				{
					return new Rectangle(_button.PointToScreen(_button.GetDropDownBounds().Location), _button.GetDropDownBounds().Size);
				}
				return base.Bounds;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (_button.AccessibleDefaultActionDescription != null)
				{
					return _button.AccessibleDefaultActionDescription;
				}
				return "Open";
			}
		}

		public ButtonAccessibleObject(MxButton button)
		{
			_button = button;
		}

		public override void DoDefaultAction()
		{
			_button.DropDown();
		}
	}
}
