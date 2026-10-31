using System;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class EditBaseTextBox : TextBox
	{
		internal class EditBaseTextBoxAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleObject Parent
			{
				get
				{
					if (base.Owner.Parent != null)
					{
						return base.Owner.Parent.AccessibilityObject;
					}
					return null;
				}
			}

			internal EditBaseTextBoxAccessibleObject(EditBaseTextBox owner)
				: base(owner)
			{
			}
		}

		private EditBase _owner;

		internal EditBaseTextBox(EditBase owner)
		{
			_owner = owner;
		}

		protected override void OnClick(EventArgs eventArgs)
		{
			base.OnClick(eventArgs);
			if (_owner != null)
			{
				_owner.RaiseClick(eventArgs);
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new EditBaseTextBoxAccessibleObject(this);
		}
	}
}
