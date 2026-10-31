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
		string IMordexXmlLayoutsSupport.Identifier => Key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				if (base.Site != null && base.Site.Name != null)
				{
					Name = base.Site.Name;
				}
			}
		}

		[DefaultValue("")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					if (Site != null && Site.Name != value && _name.Length > 0)
					{
						Site.Name = value;
					}
				}
			}
		}

		[Description("Gets or sets a value specifying whether the command is displayed in list of commands of the customize dialog")]
		[DefaultValue(true)]
		public bool ShowInCustomizeDialog
		{
			get
			{
				return _showInCustomizeDialog;
			}
			set
			{
				_showInCustomizeDialog = value;
			}
		}

		[Browsable(false)]
		public MxCommand BaseCommand => _baseCommand;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public bool IsMergedCommand
		{
			get
			{
				if (!_isMergedCommand && _baseCommand != null)
				{
					return _baseCommand.IsMergedCommand;
				}
				return _isMergedCommand;
			}
		}

		[Description("Gets or sets a String expression containing a descriptive string associated with this command.")]
		[Localizable(true)]
		public string Description
		{
			get
			{
				if (_description.Length <= 0 && _baseCommand != null)
				{
					return _baseCommand._description;
				}
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		[Description("Indicates whether the command aligns towards the beginning or end of the CommandBar. ")]
		public CommandAlignment Alignment
		{
			get
			{
				if (!IsDesignerEditing() && _alignment == CommandAlignment.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.Alignment;
					}
					return CommandAlignment.Near;
				}
				return _alignment;
			}
			set
			{
				if (_alignment != value)
				{
					_alignment = value;
					NotifyInvalidated();
				}
			}
		}

		[DefaultValue(AccessibleRole.Default)]
		[Description("The role that will be reported to accessibility clients.")]
		public AccessibleRole AccessibleRole
		{
			get
			{
				return _accessibleRole;
			}
			set
			{
				if (_accessibleRole != value)
				{
					_accessibleRole = value;
				}
			}
		}

		[Description("The name that will be reported to accessibility clients.")]
		[DefaultValue(null)]
		public string AccessibleName
		{
			get
			{
				return _accessibleName;
			}
			set
			{
				if (_accessibleName != value)
				{
					_accessibleName = value;
				}
			}
		}

		[Description("The description that will be reported to accessibility clients.")]
		[DefaultValue(null)]
		public string AccessibleDescription
		{
			get
			{
				return _accessibleDescription;
			}
			set
			{
				if (_accessibleDescription != value)
				{
					_accessibleDescription = value;
				}
			}
		}

		[Description("Determines the alignment of the text on the command.")]
		public ContentAlignment TextAlignment
		{
			get
			{
				if (!IsDesignerEditing() && _textAlignment == ContentAlignment.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.TextAlignment;
					}
					return ContentAlignment.MiddleCenter;
				}
				return _textAlignment;
			}
			set
			{
				if (_textAlignment != value)
				{
					_textAlignment = value;
					NotifyInvalidated();
				}
			}
		}

		[Description("Determines the alignment of the image on the command.")]
		public ContentAlignment ImageAlignment
		{
			get
			{
				if (!IsDesignerEditing() && _imageAlignment == ContentAlignment.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.ImageAlignment;
					}
					return ContentAlignment.MiddleCenter;
				}
				return _imageAlignment;
			}
			set
			{
				if (_imageAlignment != value)
				{
					_imageAlignment = value;
					NotifyInvalidated();
				}
			}
		}

		[Description("Determines the position of text and image relative to each other. ")]
		public TextImageRelation TextImageRelation
		{
			get
			{
				if (!IsDesignerEditing() && _textImageRelation == TextImageRelation.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.TextImageRelation;
					}
					return TextImageRelation.ImageBeforeText;
				}
				return _textImageRelation;
			}
			set
			{
				if (_textImageRelation != value)
				{
					_textImageRelation = value;
					NotifyInvalidated();
				}
			}
		}

		[Description("Represents the size of the command")]
		public Size Size
		{
			get
			{
				if (!IsDesignerEditing() && _commandSize.IsEmpty && _baseCommand != null)
				{
					return _baseCommand.Size;
				}
				return _commandSize;
			}
			set
			{
				if (_commandSize != value)
				{
					_commandSize = value;
					NotifyInvalidated();
				}
			}
		}

		[Description("Indicates whether the child commands can be customized.")]
		public InheritableBoolean AllowCustomizeCommands
		{
			get
			{
				if (!IsDesignerEditing() && _allowCustomizeCommands == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.AllowCustomizeCommands;
					}
					return InheritableBoolean.True;
				}
				return _allowCustomizeCommands;
			}
			set
			{
				if (_allowCustomizeCommands != value)
				{
					_allowCustomizeCommands = value;
				}
			}
		}

		[Description("Determines whether the child command for this object can be dragged from its parent to create floating toolbars.")]
		[Localizable(false)]
		public InheritableBoolean AllowFloatingChildren
		{
			get
			{
				if (!IsDesignerEditing() && _allowFloatingChildren == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.AllowFloatingChildren;
					}
					return InheritableBoolean.False;
				}
				return _allowFloatingChildren;
			}
			set
			{
				if (_allowFloatingChildren != value)
				{
					_allowFloatingChildren = value;
					NotifyInvalidated();
				}
			}
		}

		[Localizable(false)]
		[Description("Determines how the command is handled when merging CommandBars")]
		public CommandMergeType MergeType
		{
			get
			{
				if (!IsDesignerEditing() && _mergeType == CommandMergeType.Default)
				{
					if (_baseCommand != null)
					{
						return BaseCommand.MergeType;
					}
					return CommandMergeType.Replace;
				}
				return _mergeType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CommandMergeType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_mergeType != value)
				{
					_mergeType = value;
				}
			}
		}

		[Description("Determines the merge order of the command.")]
		[Localizable(true)]
		public int MergeOrder
		{
			get
			{
				if (!IsDesignerEditing() && _mergeOrder == -1)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.MergeOrder;
					}
					return 0;
				}
				return _mergeOrder;
			}
			set
			{
				if (_mergeOrder != value)
				{
					_mergeOrder = value;
				}
			}
		}

		[Browsable(false)]
		[Localizable(true)]
		public string CategoryName
		{
			get
			{
				if (_baseCommand != null)
				{
					return _baseCommand.CategoryName;
				}
				if (_category != null)
				{
					return _category.CategoryName;
				}
				return _categoryName;
			}
			set
			{
				if (_categoryName != value)
				{
					if (_baseCommand != null)
					{
						throw new InvalidOperationException();
					}
					_categoryName = value;
					NotifyInvalidated();
				}
			}
		}

		[Editor(typeof(CategoriesEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(CategoriesConverter))]
		[Description("Represents the category associated with this command")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CommandCategory Category
		{
			get
			{
				if (_category == null && _baseCommand != null)
				{
					return _baseCommand.Category;
				}
				if (_category == null && _categoryName.Length > 0 && CommandManager != null && CommandManager.Categories.ContainsName(_categoryName))
				{
					return CommandManager.Categories[_categoryName];
				}
				return _category;
			}
			set
			{
				if (_category != value)
				{
					if (_baseCommand != null)
					{
						throw new InvalidOperationException();
					}
					_category = value;
					if (value != null)
					{
						_categoryName = value.CategoryName;
					}
					else
					{
						_categoryName = "";
					}
					NotifyInvalidated();
				}
			}
		}

		[Description("Indicated whether the command is checked")]
		[Localizable(false)]
		public InheritableBoolean Checked
		{
			get
			{
				if (!IsDesignerEditing() && _checkedValue == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.Checked;
					}
					return InheritableBoolean.False;
				}
				return _checkedValue;
			}
			set
			{
				if (_checkedValue != value)
				{
					_checkedValue = value;
					NotifyInvalidated();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsChecked
		{
			get
			{
				if (_checkedValue == InheritableBoolean.Default)
				{
					if (_baseCommand != null)
					{
						return _baseCommand.IsChecked;
					}
					return false;
				}
				if (_checkedValue == InheritableBoolean.True)
				{
					return true;
				}
				return false;
			}
			set
			{
				if (value != IsChecked)
				{
					if (value)
					{
						Checked = InheritableBoolean.True;
					}
					else
					{
						Checked = InheritableBoolean.False;
					}
				}
			}
		}

		[Description("Specifies whether the assigned Control can recieve the input focus.")]
		public InheritableBoolean IsEditableControl
		{
			get
			{
				if (_isEditableControl == InheritableBoolean.Default && _baseCommand != null)
				{
					return _baseCommand.IsEditableControl;
				}
				return _isEditableControl;
			}
			set
			{
				_isEditableControl = value;
			}
		}

		[Description("Represents the System.Windows.Forms.Control associated with this command.")]
		[Editor(typeof(ControlEditor), typeof(UITypeEditor))]
		public Control Control
		{
			get
			{
				if (CommandManager == null)
				{
					return _control;
				}
				Control control = (_baseCommand == null) ? GetOrCreateControl(this) : _baseCommand.GetOrCreateControl(this);
				if (control != null)
				{
					return control;
				}
				if (_control == null && _baseCommand != null)
				{
					return _baseCommand.Control;
				}
				return _control;
			}
			set
			{
				if (_control != value)
				{
					_control = value;
					if (_control != null)
					{
						_control.Width = ControlWidth;
					}
					if (CommandType == CommandType.ControlContainer || CommandType == CommandType.ControlPopup)
					{
						NotifyInvalidated();
					}
				}
			}
		}

		[Description("Specifies how the command will be drawn on a UICommandBar.")]
		[Localizable(false)]
		public CommandStyle CommandStyle
		{
			get
			{
				if (!IsDesignerEditing() && _commandStyle == CommandStyle.Default && _baseCommand != null)
				{
					return _baseCommand.CommandStyle;
				}
				return _commandStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CommandStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_commandStyle != value)
				{
					_commandStyle = value;
					NotifyInvalidated();
				}
			}
		}

		[Localizable(false)]
		[Description("Represents the style and behavior of the DropDownButton.")]
		[DefaultValue(DropDownButtonStyle.Default)]
		public DropDownButtonStyle DropDownButtonStyle
		{
			get
			{
				if (!IsDesignerEditing() && _dropDownButtonStyle == DropDownButtonStyle.Default && _baseCommand != null)
				{
					return _baseCommand.DropDownButtonStyle;
				}
				return _dropDownButtonStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(DropDownButtonStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_dropDownButtonStyle != value)
				{
					_dropDownButtonStyle = value;
					NotifyInvalidated();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsParent
		{
			get
			{
				if (Commands.Count > 0)
				{
					return true;
				}
				if ((CommandType == CommandType.ColorPickerCommand || CommandType == CommandType.ControlPopup) && Control != null)
				{
					return true;
				}
				if (CommandType == CommandType.MdiList && CommandManager != null)
				{
					Form form = CommandManager.GetForm();
					if (form != null && form.MdiChildren.Length > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		[Description("Specifies the behaviour of this command.")]
		public CommandType CommandType
		{
			get
			{
				if (_baseCommand != null)
				{
					return _baseCommand.CommandType;
				}
				return _commandType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CommandType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (BaseCommand != null)
				{
					throw new InvalidOperationException("Is not possible to modify the type of a derived command");
				}
				if (_commandType != value)
				{
					DisposeHostedControls();
					_commandType = value;
					switch (CommandType)
					{
					case CommandType.MdiList:
						_control = null;
						break;
					case CommandType.Command:
					case CommandType.ToggleButton:
					case CommandType.Separator:
						_control = null;
						break;
					case CommandType.ColorPickerCommand:
						_isEditableControl = InheritableBoolean.False;
						break;
					case CommandType.ComboBoxCommand:
						_isEditableControl = InheritableBoolean.True;
						break;
					case CommandType.FontPickerCommand:
						_isEditableControl = InheritableBoolean.True;
						break;
					case CommandType.TextBoxCommand:
						_isEditableControl = InheritableBoolean.True;
						break;
					}
					if ((HasEmbeddedControl() || CommandType == CommandType.ControlPopup) && Control != null)
					{
						Control.Width = _controlWidth;
					}
					NotifyInvalidated();
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the text displayed for this command.")]
		public string Text
		{
			get
			{
				if (!IsDesignerEditing() && _text.Length <= 0 && _baseCommand != null)
				{
					return _baseCommand.Text;
				}
				return _text;
			}
			set
			{
				if (!(_text != value))
				{
					return;
				}
				try
				{
					_text = value ?? "";
					int ampersandIndex = _text.IndexOf('&');
					if (ampersandIndex != -1 && ampersandIndex < _text.Length - 1)
					{
						SetHasMnemonic(true);
						_mnemonic = _text[ampersandIndex + 1];
					}
					else
					{
						SetHasMnemonic(false);
						_mnemonic = NoMnemonic;
					}
					NotifyInvalidated();
				}
				catch (ObjectDisposedException)
				{
					// A bar that shows this command can be closing; the new text is kept.
				}
			}
		}

		[Description("The icon that is displayed on the UICommand.")]
		[Localizable(true)]
		public Icon Icon
		{
			get
			{
				if (!IsDesignerEditing() && _icon == null && _baseCommand != null)
				{
					return _baseCommand.Icon;
				}
				return _icon;
			}
			set
			{
				if (_icon != value)
				{
					_icon = value;
					NotifyInvalidated();
				}
			}
		}
	}
}
