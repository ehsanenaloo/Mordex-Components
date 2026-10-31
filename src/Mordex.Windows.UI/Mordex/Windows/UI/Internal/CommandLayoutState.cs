using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandLayoutState : FormatStyleLayoutState
	{
		private const int InvalidateMargin = 2;

		private MxCommand _command;

		public MxCommandBar _ownerCommandBar;

		public bool _hasGroupLine;

		private string _shortcutText = "";

		private bool _hasDropDown;

		private bool _isDropDownHot;

		public bool _isOverflowItem;

		public bool _hostsControl;

		public Rectangle _dropDownBounds = Rectangle.Empty;

		public bool _isDropDownPressed;

		public Rectangle _checkBounds = Rectangle.Empty;

		internal AccessibleRole _accessibleRole = AccessibleRole.Default;

		public override bool MouseDown
		{
			get
			{
				if (GetCommand() != null)
				{
					return GetCommand()._mouseDown;
				}
				return _mouseDown;
			}
			set
			{
				if (MouseDown != value)
				{
					_mouseDown = value;
					if (GetCommand() != null)
					{
						GetCommand()._mouseDown = value;
					}
					if (_owner != null)
					{
						_owner.AccessibleStateChanged(this);
					}
				}
			}
		}

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (_accessibleRole != AccessibleRole.Default)
				{
					return _accessibleRole;
				}
				bool isToolBar = false;
				if (_owner != null && _owner is MxCommandBar)
				{
					isToolBar = ((MxCommandBar)_owner).CommandBarType == CommandBarType.ToolBar;
				}
				if (GetCommand() != null)
				{
					switch (GetCommand().CommandType)
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
							if (GetCommand().Commands.Count > 0 && GetCommand().GetEffectiveDropDownButtonStyle() != DropDownButtonStyle.SplitButton)
							{
								return AccessibleRole.MenuItem;
							}
							return AccessibleRole.PushButton;
						}
						return AccessibleRole.MenuItem;
					}
				}
				return base.AccessibleRole;
			}
		}

		public CommandLayoutState()
		{
		}

		public CommandLayoutState(MxCommand command, string text)
			: this(command, text, null)
		{
		}

		public CommandLayoutState(MxCommand command, string text, ILayoutItemOwner owner)
		{
			_caption = text;
			SetCommand(command);
			_owner = owner;
		}

		public MxCommand GetCommand()
		{
			return _command;
		}

		public void SetCommand(MxCommand uiCommand)
		{
			if (_command != uiCommand)
			{
				if (_command != null)
				{
					_command._owner = null;
				}
				_command = uiCommand;
				if (_command != null)
				{
					_command._owner = this;
				}
			}
		}

		public string GetShortcutText()
		{
			return _shortcutText;
		}

		public void SetShortcutText(string shortcutText)
		{
			_shortcutText = shortcutText;
		}

		public bool HasDropDown()
		{
			return _hasDropDown;
		}

		public void SetHasDropDown(bool hasDropDown)
		{
			_hasDropDown = hasDropDown;
		}

		public bool IsSplitButton()
		{
			if (GetCommand() != null && GetCommand().GetEffectiveDropDownButtonStyle() == DropDownButtonStyle.SplitButton && GetCommand().Commands.Count > 0)
			{
				return true;
			}
			return false;
		}

		public bool IsDropDownHot()
		{
			return _isDropDownHot;
		}

		public void SetDropDownHot(bool isHot)
		{
			if (_isDropDownHot != isHot)
			{
				_isDropDownHot = isHot;
				if (_owner != null)
				{
					_owner.Invalidate(Rectangle.Inflate(_bounds, InvalidateMargin, InvalidateMargin));
				}
				else if (_parentControl != null)
				{
					_parentControl.Invalidate(Rectangle.Inflate(_bounds, InvalidateMargin, InvalidateMargin));
				}
			}
		}
	}
}
