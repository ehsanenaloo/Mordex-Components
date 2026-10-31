using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI
{
	internal class CommandMenuAccessibleObject : Control.ControlAccessibleObject
	{
		private CommandControl _popupControl;

		public override AccessibleObject Parent => base.Parent;

		public override string Name
		{
			get
			{
				string text = _popupControl.AccessibleName;
				if (text == null)
				{
					text = "DropDown";
					if (_popupControl._parentItem != null && _popupControl._parentItem.GetCommand() != null && _popupControl._parentItem.GetCommand().GetAccessibleObject().Name != null)
					{
						text = _popupControl._parentItem.GetCommand().GetAccessibleObject().Name + text;
					}
				}
				return text;
			}
			set
			{
				_popupControl.AccessibleName = value;
			}
		}

		public override AccessibleRole Role => AccessibleRole.MenuPopup;

		public CommandMenuAccessibleObject(CommandControl menu)
			: base(menu)
		{
			_popupControl = menu;
		}

		public override int GetChildCount()
		{
			if (_popupControl.Commands != null)
			{
				return _popupControl.Commands.Count;
			}
			return 0;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index >= 0 && index < _popupControl.Commands.Count)
			{
				return _popupControl.Commands[index].GetAccessibleObject();
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_popupControl != null)
			{
				Point point = _popupControl.PointToClient(new Point(screenX, screenY));
				CommandLayoutState uiCommandLayoutState = _popupControl.HitTestItem(point.X, point.Y);
				if (uiCommandLayoutState != null && uiCommandLayoutState.GetCommand() != null)
				{
					return uiCommandLayoutState.GetCommand().GetAccessibleObject();
				}
				if (_popupControl.ChildMenu != null)
				{
					return _popupControl.ChildMenu.AccessibilityObject.HitTest(screenX, screenY);
				}
			}
			return base.HitTest(screenX, screenY);
		}
	}
}
