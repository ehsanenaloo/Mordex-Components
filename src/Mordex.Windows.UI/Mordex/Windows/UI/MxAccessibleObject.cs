using System.Windows.Forms;
using Mordex.Windows.UI.Controls;

namespace Mordex.Windows.UI
{
	internal class MxAccessibleObject : Control.ControlAccessibleObject
	{
		private ComboDropDownList _dropDown;

		public override AccessibleObject Parent
		{
			get
			{
				if (_dropDown.Parent != null)
				{
					return _dropDown.Parent.AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override AccessibleRole Role => AccessibleRole.DropList;

		public override AccessibleStates State
		{
			get
			{
				AccessibleStates accessibleStates = AccessibleStates.Floating | AccessibleStates.Focusable;
				if (!_dropDown.GetIsDropDownOpen())
				{
					accessibleStates |= AccessibleStates.Invisible;
				}
				if (_dropDown.Focused)
				{
					accessibleStates |= AccessibleStates.Focused;
				}
				return accessibleStates;
			}
		}

		public MxAccessibleObject(ComboDropDownList dropDownList)
			: base(dropDownList)
		{
			_dropDown = dropDownList;
		}

		public override int GetChildCount()
		{
			return _dropDown.GetDropDownItems().Count;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index >= 0 && index < _dropDown.GetDropDownItems().Count)
			{
				return _dropDown.GetDropDownItems()[index].GetAccessibleObject();
			}
			return null;
		}
	}
}
