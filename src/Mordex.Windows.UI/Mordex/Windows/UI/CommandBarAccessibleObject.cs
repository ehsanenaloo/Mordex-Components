using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI
{
	internal class CommandBarAccessibleObject : Control.ControlAccessibleObject
	{
		private MxCommandBar _commandBar;

		public override string Name
		{
			get
			{
				string accessibleName = _commandBar.AccessibleName;
				if (accessibleName != null)
				{
					return accessibleName;
				}
				accessibleName = _commandBar.Text;
				if (accessibleName != null && accessibleName.Length > 0)
				{
					return accessibleName;
				}
				return _commandBar.Name;
			}
			set
			{
				base.Name = value;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_commandBar.AccessibleRole != AccessibleRole.Default)
				{
					return _commandBar.AccessibleRole;
				}
				if (_commandBar.CommandBarType == CommandBarType.Menu)
				{
					return AccessibleRole.MenuBar;
				}
				return AccessibleRole.ToolBar;
			}
		}

		public override AccessibleStates State => AccessibleStates.Focusable;

		public CommandBarAccessibleObject(MxCommandBar commandBar)
			: base(commandBar)
		{
			_commandBar = commandBar;
		}

		public override int GetChildCount()
		{
			int childCount = _commandBar.Commands.Count;
			if (_commandBar.GetCustomizeCommand() != null && _commandBar._customizeItem != null)
			{
				childCount++;
			}
			return childCount;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (index >= 0 && index < _commandBar.Commands.Count)
			{
				return _commandBar.Commands[index].GetAccessibleObject();
			}
			index -= _commandBar.Commands.Count;
			if (_commandBar.GetCustomizeCommand() != null && _commandBar._customizeItem != null)
			{
				if (index == 0)
				{
					return _commandBar.GetCustomizeCommand().GetAccessibleObject();
				}
				index--;
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_commandBar != null)
			{
				Point point = _commandBar.PointToClient(new Point(screenX, screenY));
				CommandLayoutState uiCommandLayoutState = _commandBar.HitTestItem(point.X, point.Y);
				if (uiCommandLayoutState != null)
				{
					return uiCommandLayoutState.GetCommand().GetAccessibleObject();
				}
				if (_commandBar.GetChildMenu() != null)
				{
					return _commandBar.GetChildMenu().AccessibilityObject.HitTest(screenX, screenY);
				}
			}
			return base.HitTest(screenX, screenY);
		}
	}
}
