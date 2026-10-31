using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class DropDownCommandState : CommandBaseAccessibleObject
	{
		private DropDownCommand _dropDownCommand;

		internal DropDownCommandState(DropDownCommand command)
			: base(command)
		{
			_dropDownCommand = command;
		}

		public override int GetChildCount()
		{
			return _dropDownCommand.Commands.Count;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index >= 0 && index < _dropDownCommand.Commands.Count)
			{
				return _dropDownCommand.Commands[index].AccessibilityObject;
			}
			return null;
		}
	}
}
