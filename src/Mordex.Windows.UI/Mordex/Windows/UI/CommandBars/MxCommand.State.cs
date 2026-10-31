using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommand
	{
		[Description("Represents the object that holds the format styles used to draw the command in its different states.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public StateStyles StateStyles
		{
			get
			{
				if (_stateStyles == null)
				{
					_stateStyles = new StateStyles(this);
				}
				return _stateStyles;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the popup window that contains the child commands.")]
		public PopupSettings PopupSettings
		{
			get
			{
				if (_popupSettings == null)
				{
					_popupSettings = new PopupSettings(this);
				}
				return _popupSettings;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public MxCommandCollection Commands
		{
			get
			{
				if (_baseCommand != null)
				{
					return _baseCommand.Commands;
				}
				if (_commands == null)
				{
					_commands = new MxCommandCollection(this);
				}
				return _commands;
			}
		}

		[Localizable(true)]
		[Description("Represents the shortcut key associated with the command.")]
		public Shortcut Shortcut
		{
			get
			{
				if (!IsDesignerEditing() && !_isShortcutSet && _baseCommand != null)
				{
					return _baseCommand.Shortcut;
				}
				return _shortcut;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Shortcut), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_shortcut != value)
				{
					_isShortcutSet = true;
					_shortcut = value;
					NotifyInvalidated();
				}
			}
		}

		[Browsable(false)]
		public char Mnemonic
		{
			get
			{
				if (_mnemonic == NoMnemonic && _baseCommand != null)
				{
					return _baseCommand.Mnemonic;
				}
				return _mnemonic;
			}
		}

		[Description("Indicates whether the command is visible or hidded.")]
		[Localizable(false)]
		public InheritableBoolean Visible
		{
			get
			{
				if (!IsDesignerEditing() && _visible == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.Visible;
					}
					return InheritableBoolean.True;
				}
				return _visible;
			}
			set
			{
				if (_visible != value)
				{
					_visible = value;
					NotifyInvalidated();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public bool IsVisible
		{
			get
			{
				if (_visible == InheritableBoolean.Default && _baseCommand != null)
				{
					return _baseCommand.IsVisible;
				}
				if (_visible == InheritableBoolean.False)
				{
					return false;
				}
				return true;
			}
			set
			{
				if (IsVisible != value)
				{
					if (value)
					{
						Visible = InheritableBoolean.True;
					}
					else
					{
						Visible = InheritableBoolean.False;
					}
				}
			}
		}

		[Localizable(true)]
		[Browsable(false)]
		[DefaultValue(null)]
		public object ControlValue
		{
			get
			{
				switch (CommandType)
				{
				case CommandType.FontPickerCommand:
				{
					MxFontPicker uiFontPicker = GetUIFontPicker();
					if (uiFontPicker != null)
					{
						return uiFontPicker.SelectedValue;
					}
					break;
				}
				case CommandType.ComboBoxCommand:
				{
					MxComboBox uiComboBox = GetUIComboBox();
					if (uiComboBox != null)
					{
						return uiComboBox.SelectedValue;
					}
					break;
				}
				case CommandType.TextBoxCommand:
				{
					TextBox textBox = GetTextBox();
					if (textBox != null)
					{
						return textBox.Text;
					}
					break;
				}
				case CommandType.ColorPickerCommand:
				{
					MxColorPicker uiColorPicker = GetUIColorPicker();
					if (uiColorPicker != null)
					{
						return uiColorPicker.SelectedColor;
					}
					break;
				}
				}
				return null;
			}
			set
			{
				SetControlValue(Control, value);
			}
		}

		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
		public object Tag
		{
			get
			{
				if (_tag == null && _baseCommand != null)
				{
					return _baseCommand.Tag;
				}
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		[Description("A string that contains data to associate with the command.")]
		public string TagString
		{
			get
			{
				if (_tagString == null && _baseCommand != null)
				{
					return _baseCommand.TagString;
				}
				return _tagString;
			}
			set
			{
				_tagString = value;
			}
		}

		[Description("Represents the cursor that is displayed when the mouse pointer is over the command.")]
		[Localizable(true)]
		public Cursor Cursor
		{
			get
			{
				if (!IsDesignerEditing() && _cursor == null && _baseCommand != null)
				{
					return BaseCommand.Cursor;
				}
				return _cursor;
			}
			set
			{
				if (_cursor != value)
				{
					_cursor = value;
				}
			}
		}

		[Description("Determines whether the MDI Child Window List is appended to this item")]
		[Localizable(false)]
		public InheritableBoolean MdiList
		{
			get
			{
				if (!IsDesignerEditing() && _mdiList == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.MdiList;
					}
					return InheritableBoolean.False;
				}
				return _mdiList;
			}
			set
			{
				if (_mdiList != value)
				{
					_mdiList = value;
					NotifyInvalidated();
				}
			}
		}

		[Description("Represents the width of the control associated with this command.")]
		public int ControlWidth
		{
			get
			{
				if (_controlWidth < 0)
				{
					if (BaseCommand != null)
					{
						return _baseCommand.ControlWidth;
					}
					return DefaultControlWidth;
				}
				return _controlWidth;
			}
			set
			{
				if (_controlWidth != value)
				{
					_controlWidth = value;
					if ((HasEmbeddedControl() || CommandType == CommandType.ControlPopup) && Control != null)
					{
						Control.Width = _controlWidth;
					}
					NotifyInvalidated();
				}
			}
		}

		[Description("Indicates whether the command is the default command in a menu.")]
		[Localizable(false)]
		public InheritableBoolean DefaultItem
		{
			get
			{
				if (!IsDesignerEditing() && _defaultItem == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.DefaultItem;
					}
					return InheritableBoolean.False;
				}
				return _defaultItem;
			}
			set
			{
				if (_defaultItem != value)
				{
					_defaultItem = value;
					NotifyInvalidated();
				}
			}
		}

		[Localizable(false)]
		[Description("Specifies whether the text and the image of the command should be displayed on the toolbar when the command has a Control assigned")]
		public InheritableBoolean ShowTextInContainers
		{
			get
			{
				if (!IsDesignerEditing() && _showTextInContainers == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.ShowTextInContainers;
					}
					return InheritableBoolean.False;
				}
				return _showTextInContainers;
			}
			set
			{
				if (_showTextInContainers != value)
				{
					_showTextInContainers = value;
					NotifyInvalidated();
				}
			}
		}

		[Description("Indicates whether the UICommand is enabled.")]
		[Localizable(false)]
		public InheritableBoolean Enabled
		{
			get
			{
				if (!IsDesignerEditing() && _enabled == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.Enabled;
					}
					return InheritableBoolean.True;
				}
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					_enabled = value;
					NotifyInvalidated();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public bool IsEnabled
		{
			get
			{
				if (_enabled == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.IsEnabled;
					}
					return true;
				}
				if (_enabled == InheritableBoolean.False)
				{
					return false;
				}
				return true;
			}
			set
			{
				if (value != IsEnabled)
				{
					if (value)
					{
						Enabled = InheritableBoolean.True;
					}
					else
					{
						Enabled = InheritableBoolean.False;
					}
				}
			}
		}

		[Localizable(true)]
		[Description("The text that is shown when the mouse hovers over this command")]
		public string ToolTipText
		{
			get
			{
				if (!IsDesignerEditing() && _toolTipText != null && _toolTipText.Length <= 0 && _baseCommand != null)
				{
					return _baseCommand.ToolTipText;
				}
				return _toolTipText;
			}
			set
			{
				_toolTipText = value;
				if (CommandManager != null)
				{
					CommandManager.RefreshToolTips();
				}
			}
		}

		[Description("Specifies whether a command is displayed as frequent or infrequent and how its click frequency is treated.")]
		[Localizable(false)]
		[DefaultValue(FrequencyMode.Frequent)]
		public FrequencyMode FrequencyMode
		{
			get
			{
				if (_baseCommand != null)
				{
					return _baseCommand.FrequencyMode;
				}
				return _frequencyMode;
			}
			set
			{
				if (_baseCommand != null)
				{
					_baseCommand.FrequencyMode = value;
					return;
				}
				_frequencyMode = value;
				NotifyInvalidated();
			}
		}

		[Browsable(false)]
		public MxCommandManager CommandManager
		{
			get
			{
				if (_baseCommand != null)
				{
					return _baseCommand.CommandManager;
				}
				return _commandManager;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public MxCommandBar FloatingBar
		{
			get
			{
				return _floatingBar;
			}
			set
			{
				_floatingBar = value;
			}
		}

		bool IAccessibleCommandBarsElement.IsInitializing
		{
			get
			{
				if (CommandManager != null)
				{
					return ((IAccessibleCommandBarsElement)CommandManager).IsInitializing;
				}
				return false;
			}
		}

		ISite IAccessibleCommandBarsElement.OwnerSite
		{
			get
			{
				if (Site != null)
				{
					return Site;
				}
				if (CommandManager != null)
				{
					return CommandManager.Site;
				}
				return Site;
			}
		}

		AccessibleObject IAccessibleCommandBarsElement.AccessibilityObject => GetAccessibleObject();

		Control IFormatStyleOwner.Owner => null;
	}
}
