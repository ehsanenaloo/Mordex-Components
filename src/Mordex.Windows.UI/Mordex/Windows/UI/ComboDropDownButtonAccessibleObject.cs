using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class ComboDropDownButtonAccessibleObject : AccessibleObject
	{
		private MxComboBase _comboBox;

		public override AccessibleObject Parent => _comboBox.AccessibilityObject;

		public override AccessibleRole Role => AccessibleRole.ButtonDropDown;

		public override string Description => _comboBox.AccessibleDescription;

		public override Rectangle Bounds
		{
			get
			{
				if (_comboBox != null)
				{
					Rectangle rectangle = _comboBox.GetButtonBounds();
					return new Rectangle(_comboBox.PointToScreen(rectangle.Location), rectangle.Size);
				}
				return base.Bounds;
			}
		}

		public override string DefaultAction
		{
			get
			{
				if (_comboBox.AccessibleDefaultActionDescription != null)
				{
					return _comboBox.AccessibleDefaultActionDescription;
				}
				return "Open";
			}
		}

		public ComboDropDownButtonAccessibleObject(MxComboBase comboBox)
		{
			_comboBox = comboBox;
		}

		public override void DoDefaultAction()
		{
			_comboBox.DropListControl(setCapture: false);
		}
	}
}
