using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI
{
	internal class CommandAccessibleObject : AccessibleObject
	{
		private MxCommand _command;

		public override AccessibleObject Parent
		{
			get
			{
				if (_command.GetHostElement() != null)
				{
					if (_command._owner != null && _command.GetHostElement() is MxCommand && _command._owner._owner is CommandControl uiCommandControl && uiCommandControl._parentItem != null && uiCommandControl._parentItem.GetCommand() != null)
					{
						if (uiCommandControl._parentItem.GetCommand().GetHostElement() is MxCommand)
						{
							return uiCommandControl._parentItem.GetCommand().GetAccessibleObject();
						}
						return uiCommandControl.AccessibilityObject;
					}
					return _command.GetHostElement().AccessibilityObject;
				}
				if (_command._owner != null && _command._owner.GetParentControl() != null)
				{
					return _command._owner.GetParentControl().AccessibilityObject;
				}
				return base.Parent;
			}
		}

		public override Rectangle Bounds
		{
			get
			{
				if (_command._owner != null)
				{
					if (_command._owner.GetParentControl() != null)
					{
						return new Rectangle(_command._owner.GetParentControl().PointToScreen(_command._owner._bounds.Location), _command._owner._bounds.Size);
					}
					return _command._owner._bounds;
				}
				return base.Bounds;
			}
		}

		public override string Description
		{
			get
			{
				if (_command != null && _command.AccessibleDescription != null)
				{
					return _command.AccessibleDescription;
				}
				return base.Description;
			}
		}

		public override AccessibleRole Role
		{
			get
			{
				if (_command != null)
				{
					if (_command.AccessibleRole != AccessibleRole.Default)
					{
						return _command.AccessibleRole;
					}
					if (_command._owner != null)
					{
						return _command._owner.AccessibleRole;
					}
					bool isToolBar = false;
					if (_command.GetHostElement() != null && _command.GetHostElement() is MxCommandBar)
					{
						isToolBar = ((MxCommandBar)_command.GetHostElement()).CommandBarType == CommandBarType.ToolBar;
					}
					switch (_command.CommandType)
					{
					case CommandType.Label:
						return AccessibleRole.StaticText;
					case CommandType.Separator:
						return AccessibleRole.Separator;
					case CommandType.ComboBoxCommand:
					case CommandType.TextBoxCommand:
					case CommandType.FontPickerCommand:
					case CommandType.ControlContainer:
						return AccessibleRole.Pane;
					default:
						if (isToolBar)
						{
							if (_command.Commands.Count > 0 && _command.GetEffectiveDropDownButtonStyle() != DropDownButtonStyle.SplitButton)
							{
								return AccessibleRole.MenuItem;
							}
							return AccessibleRole.PushButton;
						}
						return AccessibleRole.MenuItem;
					}
				}
				return base.Role;
			}
		}

		public override AccessibleStates State
		{
			get
			{
				if (_command != null)
				{
					if (!_command.IsVisible)
					{
						return AccessibleStates.Invisible | AccessibleStates.Focusable;
					}
					if (!_command.IsEnabled)
					{
						return AccessibleStates.Unavailable | AccessibleStates.Focusable;
					}
					if (_command.IsChecked)
					{
						return AccessibleStates.Checked | AccessibleStates.Focusable;
					}
					return AccessibleStates.Focusable;
				}
				return AccessibleStates.Default;
			}
		}

		public override string Name
		{
			get
			{
				if (_command != null)
				{
					string text = null;
					if (Role == AccessibleRole.ToolTip)
					{
						text = _command.ToolTipText;
					}
					else
					{
						if (_command.AccessibleName != null && _command.AccessibleName.Length > 0)
						{
							return _command.AccessibleName;
						}
						if (_command.Text != null && _command.Text.Length > 0)
						{
							text = _command.Text.Replace("&", "");
						}
						if (text == null || text.Length == 0)
						{
							text = _command.Name;
						}
					}
					return text;
				}
				return base.Name;
			}
		}

		public override string DefaultAction => "Press";

		internal CommandAccessibleObject(MxCommand cmd)
		{
			_command = cmd;
		}

		public override void DoDefaultAction()
		{
			if (_command.IsParent)
			{
				_command.Expand();
			}
			else if (_command._owner != null)
			{
				_command._owner.InvokeDefaultAction();
			}
		}

		public override int GetChildCount()
		{
			if (_command.HasEmbeddedControl() && _command.Control != null)
			{
				return 1;
			}
			return _command.Commands.Count;
		}

		public override AccessibleObject GetChild(int index)
		{
			if (_command.HasEmbeddedControl() && _command.Control != null && index == 0)
			{
				return _command.Control.AccessibilityObject;
			}
			if (index >= 0 && index < _command.Commands.Count)
			{
				return _command.Commands[index].GetAccessibleObject();
			}
			return null;
		}

		private MxCommandBar GetHostCommandBar()
		{
			if (_command != null && _command.GetHostElement() != null && _command.GetHostElement() is MxCommandBar result)
			{
				return result;
			}
			return null;
		}

		public override AccessibleObject HitTest(int screenX, int screenY)
		{
			if (_command.Commands.Count > 0 && _command._owner != null && _command._owner.GetParentControl() != null)
			{
				if (_command._owner.GetParentControl() is CommandControl uiCommandControl)
				{
					return uiCommandControl.AccessibilityObject.HitTest(screenX, screenY);
				}
				if (GetHostCommandBar() != null && GetHostCommandBar().GetChildMenu() != null)
				{
					return GetHostCommandBar().GetChildMenu().AccessibilityObject.HitTest(screenX, screenY);
				}
			}
			return base.HitTest(screenX, screenY);
		}
	}
}
