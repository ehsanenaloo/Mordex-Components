using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class RadioButtonAccessibleObject : Control.ControlAccessibleObject
	{
		private MxRadioButton _radioButton;

		public override AccessibleRole Role
		{
			get
			{
				if (_radioButton.AccessibleRole != AccessibleRole.Default)
				{
					return _radioButton.AccessibleRole;
				}
				return AccessibleRole.RadioButton;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				AccessibleStates accessibleStates = AccessibleStates.Focusable;
				if (_radioButton.Focused)
				{
					accessibleStates |= AccessibleStates.Focused;
				}
				if (_radioButton.Checked)
				{
					accessibleStates |= AccessibleStates.Checked;
				}
				return accessibleStates;
			}
		}

		public RadioButtonAccessibleObject(MxRadioButton radioButton)
			: base(radioButton)
		{
			_radioButton = radioButton;
		}
	}
}
