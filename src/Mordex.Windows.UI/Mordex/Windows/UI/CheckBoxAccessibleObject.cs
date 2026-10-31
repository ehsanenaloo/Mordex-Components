using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class CheckBoxAccessibleObject : Control.ControlAccessibleObject
	{
		private MxCheckBox _checkBox;

		public override AccessibleRole Role
		{
			get
			{
				if (_checkBox.AccessibleRole != AccessibleRole.Default)
				{
					return _checkBox.AccessibleRole;
				}
				return AccessibleRole.CheckButton;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				AccessibleStates accessibleStates = AccessibleStates.Focusable;
				if (_checkBox.Focused)
				{
					accessibleStates |= AccessibleStates.Focused;
				}
				if (_checkBox.CheckState == CheckState.Checked)
				{
					accessibleStates |= AccessibleStates.Checked;
				}
				else if (_checkBox.CheckState == CheckState.Indeterminate)
				{
					accessibleStates |= AccessibleStates.Mixed;
				}
				return accessibleStates;
			}
		}

		public CheckBoxAccessibleObject(MxCheckBox checkBox)
			: base(checkBox)
		{
			_checkBox = checkBox;
		}
	}
}
