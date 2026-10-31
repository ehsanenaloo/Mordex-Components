using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class ComboBoxItemAccessibleObject : AccessibleObject
	{
		private ComboBoxItem _item;

		public override AccessibleRole Role => AccessibleRole.ListItem;

		public override string Name => _item.Text;

		public override AccessibleStates State
		{
			get
			{
				AccessibleStates result = AccessibleStates.Focusable;
				if (_item.GetOwnerComboBase() != null && _item.GetOwnerComboBase()._formatStyleRenderer.GetHighlightedItem() == _item)
				{
					result = AccessibleStates.Selected;
				}
				return result;
			}
		}

		public override string DefaultAction => "Double Click";

		public ComboBoxItemAccessibleObject(ComboBoxItem item)
		{
			_item = item;
		}

		public override void DoDefaultAction()
		{
			if (_item != null && _item.GetOwnerComboBase() != null)
			{
				if (_item.GetOwnerComboBase() is MxComboBox)
				{
					((MxComboBox)_item.GetOwnerComboBase()).SelectedItem = _item;
				}
				else if (_item.GetOwnerComboBase() is MxFontPicker)
				{
					((MxComboBox)_item.GetOwnerComboBase()).SelectedItem = _item;
				}
			}
		}
	}
}
