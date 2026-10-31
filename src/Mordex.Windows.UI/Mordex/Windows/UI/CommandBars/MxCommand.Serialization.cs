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
		private void DeserializeCommands(LayoutReader reader)
		{
			PropertyValue propertyValue = reader["Commands"];
			LayoutReader mordexLayoutReader = null;
			IList<MxCommand> list = new List<MxCommand>();
			if (_commands != null)
			{
				foreach (MxCommand item in _commands)
				{
					list.Add(item);
				}
			}
			if (propertyValue != null)
			{
				mordexLayoutReader = propertyValue.GetLayoutReader();
				if (mordexLayoutReader != null)
				{
					int commandIndex = 0;
					MxCommandCollection uiCommandCollection = new MxCommandCollection(this);
					PropertyValue commandValue = mordexLayoutReader["Command" + commandIndex];
					while (commandValue != null)
					{
						PropertyValue itemValue = mordexLayoutReader[commandIndex];
						LayoutReader layoutReader = itemValue.GetLayoutReader();
						string text = (string)LayoutReader.GetPropertyValue(layoutReader, "Name", typeof(string), "");
						MxCommand uiCommand = Commands.FindByName(text);
						if (uiCommand == null)
						{
							uiCommand = new MxCommand(itemValue.Name);
						}
						else
						{
							list.Remove(uiCommand);
						}
						itemValue.DeserializeObject(uiCommand);
						commandIndex++;
						commandValue = mordexLayoutReader["Command" + commandIndex];
						uiCommandCollection.AddKeepingPositions(uiCommand);
					}
					Commands.DetachAllAndReset();
					_commands = uiCommandCollection;
				}
			}
			else if (_commands != null)
			{
				_commands.DetachAllAndReset();
				_commands._owner = this;
			}
			foreach (MxCommand staleCommand in list)
			{
				staleCommand.Dispose();
			}
		}

		internal void ApplyLayoutState(PropertyValue layoutValue, MxCommandManager uiCommandManager)
		{
			LayoutReader layoutReader = layoutValue.GetLayoutReader();
			string text = (string)LayoutReader.GetPropertyValue(layoutReader, "BaseCommand", typeof(string), "");
			if (text.Length > 0)
			{
				if (uiCommandManager != null)
				{
					MxCommand uiCommand = uiCommandManager.Commands.FindByKey(text);
					if (uiCommand != null)
					{
						SetBaseCommand(uiCommand);
					}
					else if (CommandType == CommandType.Separator || text == "Separator")
					{
						SetBaseCommand(uiCommandManager.GetSeparatorCommand());
					}
					else
					{
						_key = text;
					}
				}
				else
				{
					_key = text;
				}
			}
			PropertyValue propertyValue = layoutReader["Commands"];
			if (propertyValue == null)
			{
				return;
			}
			LayoutReader commandsReader = propertyValue.GetLayoutReader();
			if (commandsReader != null)
			{
				int commandCount = commandsReader.Count;
				if (commandsReader["MergedCommands"] != null)
				{
					commandCount--;
				}
				if (commandsReader["OriginalCommands"] != null)
				{
					commandCount--;
				}
				DateTime dateTime = DateTime.MinValue;
				for (int i = 0; i < commandCount; i++)
				{
					PropertyValue childValue = commandsReader[i];
					MxCommand childCommand = Commands[i];
					childCommand.ApplyLayoutState(childValue, uiCommandManager);
					if (childCommand.GetLastClickedDate() > dateTime)
					{
						dateTime = childCommand.GetLastClickedDate();
					}
				}
				foreach (MxCommand command in Commands)
				{
					command.SetReferenceDate(dateTime);
					if (command.GetLastClickedDate() == DateTime.MinValue && command.FrequencyMode == FrequencyMode.Frequent)
					{
						command.SetLastClickedDate(dateTime);
					}
				}
			}
			Commands.DeserializeOriginalCommands(propertyValue);
			Commands.DeserializeMergedCommands(propertyValue);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_name = (string)LayoutReader.GetPropertyValue(reader, "Name", typeof(string), "");
			DeserializeCommands(reader);
			_accessibleName = (string)LayoutReader.GetPropertyValue(reader, "AccessibleName", typeof(string), null);
			_accessibleDescription = (string)LayoutReader.GetPropertyValue(reader, "AccessibleDescription", typeof(string), null);
			_accessibleRole = (AccessibleRole)LayoutReader.GetPropertyValue(reader, "AccessibleRole", typeof(AccessibleRole), AccessibleRole.Default);
			_alignment = (CommandAlignment)LayoutReader.GetPropertyValue(reader, "Alignment", typeof(CommandAlignment), CommandAlignment.Default);
			_textAlignment = (ContentAlignment)LayoutReader.GetPropertyValue(reader, "TextAlignment", typeof(ContentAlignment), ContentAlignment.Default);
			_imageAlignment = (ContentAlignment)LayoutReader.GetPropertyValue(reader, "ImageAlignment", typeof(ContentAlignment), ContentAlignment.Default);
			_commandSize = (Size)LayoutReader.GetPropertyValue(reader, "Size", typeof(Size), Size.Empty);
			_textImageRelation = (TextImageRelation)LayoutReader.GetPropertyValue(reader, "TextImageRelation", typeof(TextImageRelation), TextImageRelation.Default);
			_imageReplaceableColor = (Color)LayoutReader.GetPropertyValue(reader, "ImageReplaceableColor", typeof(Color), Color.Empty);
			_allowFloatingChildren = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowFloatingChildren", typeof(InheritableBoolean), InheritableBoolean.Default);
			_allowCustomizeCommands = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowCustomizeCommands", typeof(InheritableBoolean), InheritableBoolean.Default);
			_checkedValue = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "Checked", typeof(InheritableBoolean), InheritableBoolean.Default);
			_lastClickedDate = (DateTime)LayoutReader.GetPropertyValue(reader, "LastClickedDate", typeof(DateTime), DateTime.MinValue);
			_category = (CommandCategory)LayoutReader.GetPropertyValue(reader, "Category", typeof(CommandCategory), null);
			_categoryName = (string)LayoutReader.GetPropertyValue(reader, "CategoryName", typeof(string), "");
			_commandStyle = (CommandStyle)LayoutReader.GetPropertyValue(reader, "CommandStyle", typeof(CommandStyle), CommandStyle.Default);
			_dropDownButtonStyle = (DropDownButtonStyle)LayoutReader.GetPropertyValue(reader, "DropDownButtonStyle", typeof(DropDownButtonStyle), DropDownButtonStyle.Default);
			_commandType = (CommandType)LayoutReader.GetPropertyValue(reader, "CommandType", typeof(CommandType), CommandType.Command);
			_controlWidth = (int)LayoutReader.GetPropertyValue(reader, "ControlWidth", typeof(int), -1);
			_defaultItem = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "DefaultItem", typeof(InheritableBoolean), InheritableBoolean.Default);
			PropertyValue propertyValue = reader["OriginalShortcut"];
			if (propertyValue != null)
			{
				_originalShortcut = (Shortcut)propertyValue.GetValue(typeof(Shortcut));
			}
			else
			{
				_originalShortcut = null;
			}
			_frequencyMode = (FrequencyMode)LayoutReader.GetPropertyValue(reader, "FrequencyMode", typeof(FrequencyMode), FrequencyMode.Frequent);
			_daysForInfrequency = (int)LayoutReader.GetPropertyValue(reader, "DaysForInfrequency", typeof(int), -1);
			_cursor = (Cursor)LayoutReader.GetPropertyValue(reader, "Cursor", typeof(Cursor), null);
			_description = (string)LayoutReader.GetPropertyValue(reader, "Description", typeof(string), "");
			_disabledImageIndex = (int)LayoutReader.GetPropertyValue(reader, "DisabledImageIndex", typeof(int), -1);
			_disabledImageKey = (string)LayoutReader.GetPropertyValue(reader, "DisabledImageKey", typeof(string), "");
			_largeDisabledImageIndex = (int)LayoutReader.GetPropertyValue(reader, "LargeDisabledImageIndex", typeof(int), -1);
			_largeDisabledImageKey = (string)LayoutReader.GetPropertyValue(reader, "LargeDisabledImageKey", typeof(string), "");
			_hotImageIndex = (int)LayoutReader.GetPropertyValue(reader, "HotImageIndex", typeof(int), -1);
			_hotImageKey = (string)LayoutReader.GetPropertyValue(reader, "HotImageKey", typeof(string), "");
			_largeHotImageIndex = (int)LayoutReader.GetPropertyValue(reader, "LargeHotImageIndex", typeof(int), -1);
			_largeHotImageKey = (string)LayoutReader.GetPropertyValue(reader, "LargeHotImageKey", typeof(string), "");
			_icon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_commandImage = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), "");
			_largeImageIndex = (int)LayoutReader.GetPropertyValue(reader, "LargeImageIndex", typeof(int), -1);
			_largeImageKey = (string)LayoutReader.GetPropertyValue(reader, "LargeImageKey", typeof(string), "");
			_isEditableControl = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "IsEditableControl", typeof(InheritableBoolean), InheritableBoolean.Default);
			_key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), "");
			_enabled = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "Enabled", typeof(InheritableBoolean), InheritableBoolean.Default);
			_showInCustomizeDialog = (bool)LayoutReader.GetPropertyValue(reader, "ShowInCustomizeDialog", typeof(bool), true);
			_mergeOrder = (int)LayoutReader.GetPropertyValue(reader, "MergeOrder", typeof(int), -1);
			_mergeType = (CommandMergeType)LayoutReader.GetPropertyValue(reader, "MergeType", typeof(CommandMergeType), CommandMergeType.Default);
			_position = (int)LayoutReader.GetPropertyValue(reader, "Position", typeof(int), -1);
			_popupSettings = (PopupSettings)LayoutReader.GetPropertyValue(reader, "PopupSettings", typeof(PopupSettings), null);
			if (_popupSettings != null)
			{
				_popupSettings._owner = this;
			}
			_selectedImageIndex = (int)LayoutReader.GetPropertyValue(reader, "SelectedImageIndex", typeof(int), -1);
			_selectedImageKey = (string)LayoutReader.GetPropertyValue(reader, "SelectedImageKey", typeof(string), "");
			_largeSelectedImageIndex = (int)LayoutReader.GetPropertyValue(reader, "LargeSelectedImageIndex", typeof(int), -1);
			_largeSelectedImageKey = (string)LayoutReader.GetPropertyValue(reader, "LargeSelectedImageKey", typeof(string), "");
			Shortcut = (Shortcut)LayoutReader.GetPropertyValue(reader, "Shortcut", typeof(Shortcut), Shortcut.None);
			_stateStyles = null;
			reader["StateStyles"]?.DeserializeObject(StateStyles);
			string text = (string)LayoutReader.GetPropertyValue(reader, "Tag", typeof(string), null);
			if (text != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(reader, "TagTypeName", typeof(string), null);
				_tag = tagTypeName != null ? LayoutTypePolicy.ConvertOrKeepText(text, tagTypeName) : text;
			}
			else
			{
				_tag = null;
			}
			_tagString = (string)LayoutReader.GetPropertyValue(reader, "TagString", typeof(string), null);
			Text = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
			_toolTipText = (string)LayoutReader.GetPropertyValue(reader, "ToolTipText", typeof(string), "");
			_visible = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "Visible", typeof(InheritableBoolean), InheritableBoolean.Default);
			_mdiList = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "MdiList", typeof(InheritableBoolean), InheritableBoolean.Default);
			_showTextInContainers = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ShowTextInContainers", typeof(InheritableBoolean), InheritableBoolean.Default);
			object selectedColor = LayoutReader.GetPropertyValue(reader, "SelectedColor", typeof(Color), null);
			if (selectedColor != null)
			{
				ControlValue = (Color)selectedColor;
				return;
			}
			string selectedFamily = (string)LayoutReader.GetPropertyValue(reader, "SelectedFamily", typeof(string), null);
			if (selectedFamily != null)
			{
				ControlValue = new FontFamily(selectedFamily);
				return;
			}
			string controlValueText = (string)LayoutReader.GetPropertyValue(reader, "ControlValue", typeof(string), null);
			if (controlValueText != null)
			{
				string controlValueTypeName = (string)LayoutReader.GetPropertyValue(reader, "ControlValueTypeName", typeof(string), null);
				ControlValue = controlValueTypeName != null ? LayoutTypePolicy.ConvertOrKeepText(controlValueText, controlValueTypeName) : controlValueText;
			}
			else
			{
				ControlValue = null;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_accessibleName != null)
			{
				info.AddValue("AccessibleName", _accessibleName);
			}
			if (_accessibleDescription != null)
			{
				info.AddValue("AccessibleDescription", _accessibleDescription);
			}
			if (_accessibleRole != AccessibleRole.Default)
			{
				info.AddValue("AccessibleRole", _accessibleRole);
			}
			if (_alignment != CommandAlignment.Default)
			{
				info.AddValue("Alignment", _alignment);
			}
			if (_allowFloatingChildren != InheritableBoolean.Default)
			{
				info.AddValue("AllowFloatingChildren", _allowFloatingChildren);
			}
			if (_allowCustomizeCommands != InheritableBoolean.Default)
			{
				info.AddValue("AllowCustomizeCommands", _allowCustomizeCommands);
			}
			if (_baseCommand != null)
			{
				info.AddValue("BaseCommand", BaseCommand.Key);
			}
			if (_category != null)
			{
				info.AddValue("Category", _category);
			}
			if (_categoryName.Length > 0)
			{
				info.AddValue("CategoryName", _categoryName);
			}
			if (_checkedValue != InheritableBoolean.Default)
			{
				info.AddValue("Checked", _checkedValue);
			}
			if (_commands != null && _commands.Count > 0 && _baseCommand == null)
			{
				info.AddValue("Commands", _commands);
			}
			if (_commandStyle != CommandStyle.Default)
			{
				info.AddValue("CommandStyle", _commandStyle);
			}
			if (_dropDownButtonStyle != DropDownButtonStyle.Default)
			{
				info.AddValue("DropDownButtonStyle", _dropDownButtonStyle);
			}
			if (_commandType != CommandType.Command)
			{
				info.AddValue("CommandType", _commandType);
			}
			if (_controlWidth != -1)
			{
				info.AddValue("ControlWidth", _controlWidth);
			}
			if (ControlValue != null)
			{
				if (ControlValue is Color)
				{
					Color color = (Color)ControlValue;
					info.AddValue("SelectedColor", color);
				}
				else if (ControlValue is FontFamily)
				{
					FontFamily fontFamily = (FontFamily)ControlValue;
					info.AddValue("SelectedFamily", fontFamily.Name);
				}
				else
				{
					if (ControlValue.GetType() != typeof(string))
					{
						info.AddValue("ControlValueTypeName", ControlValue.GetType().FullName);
					}
					info.AddValue("ControlValue", ControlValue);
				}
			}
			if (_cursor != null)
			{
				info.AddValue("Cursor", _cursor);
			}
			if (_baseCommand == null && _daysForInfrequency != -1)
			{
				info.AddValue("DaysForInfrequency", _daysForInfrequency);
			}
			if (_description.Length > 0)
			{
				info.AddValue("Description", _description);
			}
			if (_defaultItem != InheritableBoolean.Default)
			{
				info.AddValue("DefaultItem", _defaultItem);
			}
			if (_enabled != InheritableBoolean.Default)
			{
				info.AddValue("Enabled", _enabled);
			}
			if (_baseCommand == null && _frequencyMode != FrequencyMode.Frequent)
			{
				info.AddValue("FrequencyMode", _frequencyMode);
			}
			if (_icon != null)
			{
				info.AddValue("Icon", _icon);
			}
			if (_commandImage != null)
			{
				info.AddValue("Image", _commandImage);
			}
			if (_imageAlignment != ContentAlignment.Default)
			{
				info.AddValue("ImageAlignment", _imageAlignment);
			}
			if (!_imageReplaceableColor.IsEmpty)
			{
				info.AddValue("ImageReplaceableColor", _imageReplaceableColor);
			}
			if (_isEditableControl != InheritableBoolean.Default)
			{
				info.AddValue("IsEditableControl", _isEditableControl);
			}
			if (IsMergedCommand)
			{
				if (Key.Length > 0)
				{
					info.AddValue("Key", Key);
				}
			}
			else if (_key.Length > 0)
			{
				info.AddValue("Key", _key);
			}
			if (_largeDisabledImageIndex != -1)
			{
				info.AddValue("LargeDisabledImageIndex", _largeDisabledImageIndex);
			}
			if (_largeDisabledImageKey.Length > 0)
			{
				info.AddValue("LargeDisabledImageKey", _largeDisabledImageKey);
			}
			if (_largeHotImageIndex != -1)
			{
				info.AddValue("LargeHotImageIndex", _largeHotImageIndex);
			}
			if (_largeHotImageKey.Length > 0)
			{
				info.AddValue("LargeHotImageKey", _largeHotImageKey);
			}
			if (_largeImageIndex != -1)
			{
				info.AddValue("LargeImageIndex", _largeImageIndex);
			}
			if (_largeImageKey.Length > 0)
			{
				info.AddValue("LargeImageKey", _largeImageKey);
			}
			if (_largeSelectedImageIndex != -1)
			{
				info.AddValue("LargeSelectedImageIndex", _largeSelectedImageIndex);
			}
			if (_largeSelectedImageKey.Length > 0)
			{
				info.AddValue("LargeSelectedImageKey", _largeSelectedImageKey);
			}
			if (_baseCommand == null && _lastClickedDate != DateTime.MinValue)
			{
				info.AddValue("LastClickedDate", _lastClickedDate);
			}
			if (_mdiList != InheritableBoolean.Default)
			{
				info.AddValue("MdiList", _mdiList);
			}
			if (_mergeOrder != -1)
			{
				info.AddValue("MergeOrder", _mergeOrder);
			}
			if (_mergeType != CommandMergeType.Default)
			{
				info.AddValue("MergeType", _mergeType);
			}
			if (_name.Length > 0)
			{
				info.AddValue("Name", _name);
			}
			if (_originalShortcut != null)
			{
				info.AddValue("OriginalShortcut", _originalShortcut);
			}
			if (_popupSettings != null && _popupSettings.ShouldSerialize())
			{
				info.AddValue("PopupSettings", _popupSettings);
			}
			if (_position >= 0)
			{
				info.AddValue("Position", GetPosition());
			}
			if (_shortcut != Shortcut.None)
			{
				info.AddValue("Shortcut", _shortcut);
			}
			if (!_commandSize.IsEmpty)
			{
				info.AddValue("Size", _commandSize);
			}
			if (!_showInCustomizeDialog)
			{
				info.AddValue("ShowInCustomizeDialog", _showInCustomizeDialog);
			}
			if (_showTextInContainers != InheritableBoolean.Default)
			{
				info.AddValue("ShowTextInContainers", _showTextInContainers);
			}
			if (_disabledImageIndex != -1)
			{
				info.AddValue("DisabledImageIndex", _disabledImageIndex);
			}
			if (_disabledImageKey.Length > 0)
			{
				info.AddValue("DisabledImageKey", _disabledImageKey);
			}
			if (_hotImageIndex != -1)
			{
				info.AddValue("HotImageIndex", _hotImageIndex);
			}
			if (_hotImageKey.Length > 0)
			{
				info.AddValue("HotImageKey", _hotImageKey);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_selectedImageIndex != -1)
			{
				info.AddValue("SelectedImageIndex", _selectedImageIndex);
			}
			if (_selectedImageKey.Length > 0)
			{
				info.AddValue("SelectedImageKey", _selectedImageKey);
			}
			if (_stateStyles != null && _stateStyles.ShouldSerialize())
			{
				info.AddValue("StateStyles", _stateStyles);
			}
			if (_tag != null)
			{
				if (_tag.GetType() != typeof(string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				info.AddValue("Tag", _tag);
			}
			if (_tagString != null)
			{
				info.AddValue("TagString", _tagString);
			}
			if (_text.Length > 0)
			{
				info.AddValue("Text", _text);
			}
			if (_textAlignment != ContentAlignment.Default)
			{
				info.AddValue("TextAlignment", _textAlignment);
			}
			if (_textImageRelation != TextImageRelation.Default)
			{
				info.AddValue("TextImageRelation", _textImageRelation);
			}
			if (_toolTipText != null && _toolTipText.Length > 0)
			{
				info.AddValue("ToolTipText", _toolTipText);
			}
			if (_visible != InheritableBoolean.Default)
			{
				info.AddValue("Visible", _visible);
			}
		}
	}
}
