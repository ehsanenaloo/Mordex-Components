using System.Collections.Generic;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class ComboBaseAccessibleObject : Control.ControlAccessibleObject
	{
		private MxComboBase _comboBox;

		public override AccessibleRole Role
		{
			get
			{
				if (_comboBox.AccessibleRole != AccessibleRole.Default)
				{
					return _comboBox.AccessibleRole;
				}
				return AccessibleRole.ComboBox;
			}
		}

		public override string Name
		{
			get
			{
				if (_comboBox.AccessibleName != null && _comboBox.AccessibleName.Length > 0)
				{
					return _comboBox.AccessibleName;
				}
				Label label = FindPrecedingLabel();
				if (label != null)
				{
					string name = label.AccessibilityObject.Name;
					return name + _comboBox.Text;
				}
				return base.Name;
			}
		}

		public ComboBaseAccessibleObject(MxComboBase comboBox)
			: base(comboBox)
		{
			_comboBox = comboBox;
		}

		private Label FindPrecedingLabel()
		{
			Control control = RenderingHelper.GetParentControl(_comboBox);
			if (control != null)
			{
				Control nextControl = control.GetNextControl(_comboBox, forward: false);
				if (nextControl != null && nextControl is Label)
				{
					return nextControl as Label;
				}
			}
			return null;
		}

		public override int GetChildCount()
		{
			int childCount = 0;
			if (_comboBox.GetOrCreateAccessibleObject() != null)
			{
				childCount++;
			}
			if (_comboBox.GetDropDownButtonAccessibleObject() != null)
			{
				childCount++;
			}
			if (_comboBox._formatStyleRenderer != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			List<AccessibleObject> list = new List<AccessibleObject>();
			if (_comboBox.GetOrCreateAccessibleObject() != null)
			{
				list.Add(_comboBox.GetOrCreateAccessibleObject());
			}
			if (_comboBox.GetDropDownButtonAccessibleObject() != null)
			{
				list.Add(_comboBox.GetDropDownButtonAccessibleObject());
			}
			if (_comboBox._formatStyleRenderer != null)
			{
				list.Add(_comboBox._formatStyleRenderer.AccessibilityObject);
			}
			if (index >= 0 && index < list.Count)
			{
				return list[index];
			}
			return null;
		}
	}
}
