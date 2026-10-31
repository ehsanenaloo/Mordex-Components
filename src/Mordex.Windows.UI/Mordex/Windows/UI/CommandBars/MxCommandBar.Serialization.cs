using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_isLoadingLayout = true;
			try
			{
				RemoveHostedControls();
				_isMergedCommandBar = (bool)LayoutReader.GetPropertyValue(reader, "IsMergedCommandBar", typeof(bool), false);
				_textAlign = (TextAlign)LayoutReader.GetPropertyValue(reader, "TextAlign", typeof(TextAlign), TextAlign.Default);
				_view = (View)LayoutReader.GetPropertyValue(reader, "View", typeof(View), View.Default);
				_allowClose = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowClose", typeof(InheritableBoolean), InheritableBoolean.Default);
				_allowCustomize = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowCustomize", typeof(InheritableBoolean), InheritableBoolean.Default);
				_showCustomizeButton = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ShowCustomizeButton", typeof(InheritableBoolean), InheritableBoolean.Default);
				_animation = (DropDownAnimation)LayoutReader.GetPropertyValue(reader, "Animation", typeof(DropDownAnimation), DropDownAnimation.Default);
				_allowDelete = (bool)LayoutReader.GetPropertyValue(reader, "AllowDelete", typeof(bool), false);
				_allowRename = (bool)LayoutReader.GetPropertyValue(reader, "AllowRename", typeof(bool), false);
				_allowMerge = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowMerge", typeof(InheritableBoolean), InheritableBoolean.Default);
				_commandBarType = (CommandBarType)LayoutReader.GetPropertyValue(reader, "CommandBarType", typeof(CommandBarType), CommandBarType.ToolBar);
				PropertyValue propertyValue = reader["Commands"];
				MxCommandCollection uiCommandCollection = new MxCommandCollection(this);
				LayoutReader mordexLayoutReader = null;
				IList<MxCommand> list = new List<MxCommand>();
				foreach (MxCommand item in _commands)
				{
					list.Add(item);
				}
				if (propertyValue != null)
				{
					mordexLayoutReader = propertyValue.GetLayoutReader();
					if (mordexLayoutReader != null)
					{
						int commandIndex = 0;
						int valueIndex = 0;
						PropertyValue commandValue = mordexLayoutReader["Command" + commandIndex];
						while (commandValue != null)
						{
							PropertyValue itemValue = mordexLayoutReader[valueIndex];
							valueIndex++;
							if (!(itemValue.Name == "OriginalCommands") && !(itemValue.Name == "MergedCommands"))
							{
								LayoutReader layoutReader = itemValue.GetLayoutReader();
								string text = (string)LayoutReader.GetPropertyValue(layoutReader, "Name", typeof(string), "");
								MxCommand uiCommand = _commands.FindByName(text);
								if (uiCommand == null)
								{
									uiCommand = new MxCommand(itemValue.Name);
								}
								else
								{
									list.Remove(uiCommand);
								}
								itemValue.DeserializeObject(uiCommand);
								if (CommandManager != null)
								{
									uiCommand.ApplyLayoutState(commandValue, CommandManager);
								}
								commandIndex++;
								commandValue = mordexLayoutReader["Command" + commandIndex];
								uiCommandCollection.AddKeepingPositions(uiCommand);
							}
						}
						_commands.DetachAllAndReset();
						_commands = uiCommandCollection;
						_commands.DeserializeOriginalCommands(propertyValue);
						_commands.DeserializeMergedCommands(propertyValue);
					}
					else
					{
						_commands.DeserializeMergedCommands(propertyValue);
					}
				}
				else
				{
					_commands.DetachAllAndReset();
					_commands = uiCommandCollection;
				}
				foreach (MxCommand unusedCommand in list)
				{
					unusedCommand.Dispose();
				}
				_commandsStateStyles = null;
				reader["CommandsStateStyles"]?.DeserializeObject(CommandsStateStyles);
				if (_commandsStateStyles != null)
				{
					_commandsStateStyles._owner = this;
				}
				_commandsStyle = (CommandStyle)LayoutReader.GetPropertyValue(reader, "CommandsStyle", typeof(CommandStyle), CommandStyle.Default);
				_lockCommandBar = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "LockCommandBar", typeof(InheritableBoolean), InheritableBoolean.Default);
				_forceDockStyleUpdate = true;
				SetClosed((bool)LayoutReader.GetPropertyValue(reader, "Closed", typeof(bool), false));
				FloatingLocation = (Point)LayoutReader.GetPropertyValue(reader, "FloatingLocation", typeof(Point), Point.Empty);
				FloatingSize = (Size)LayoutReader.GetPropertyValue(reader, "FloatingSize", typeof(Size), Size.Empty);
				base.Bounds = Rectangle.Empty;
				DockStyle = (BarsDockStyle)LayoutReader.GetPropertyValue(reader, "DockStyle", typeof(BarsDockStyle), BarsDockStyle.Top);
				Point location = (Point)LayoutReader.GetPropertyValue(reader, "Location", typeof(Point), Point.Empty);
				if (!location.IsEmpty)
				{
					Location = location;
				}
				_formatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "FormatStyle", typeof(FormatStyle), null);
				if (_formatStyle != null)
				{
					_formatStyle._owner = this;
				}
				if (CommandManager != null)
				{
					RemoveDockedPlaceholder();
				}
				_dockedDockStyle = (BarsDockStyle)LayoutReader.GetPropertyValue(reader, "DockedDockStyle", typeof(BarsDockStyle), BarsDockStyle.Top);
				_dockedRowIndex = (int)LayoutReader.GetPropertyValue(reader, "DockedRowIndex", typeof(int), -1);
				_dockedLocation = (Point)LayoutReader.GetPropertyValue(reader, "DockedLocation", typeof(Point), Point.Empty);
				ReserveDockedRow();
				Key = (string)LayoutReader.GetPropertyValue(reader, "Key", typeof(string), "");
				_commandBarWrappable = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "Wrappable", typeof(InheritableBoolean), InheritableBoolean.Default);
				_fullRow = (bool)LayoutReader.GetPropertyValue(reader, "FullRow", typeof(bool), false);
				_position = (Point)LayoutReader.GetPropertyValue(reader, "Position", typeof(Point), Point.Empty);
				_popupSettings = (PopupSettings)LayoutReader.GetPropertyValue(reader, "PopupSettings", typeof(PopupSettings), null);
				if (_popupSettings != null)
				{
					_popupSettings._owner = this;
				}
				_popupView = (View)LayoutReader.GetPropertyValue(reader, "PopupView", typeof(View), View.Default);
				_showToolTips = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ShowToolTips", typeof(InheritableBoolean), InheritableBoolean.Default);
				_parentID = (Guid)LayoutReader.GetPropertyValue(reader, "ParentID", typeof(Guid), Guid.Empty);
				if (_isMergedCommandBar)
				{
					Location = (Point)LayoutReader.GetPropertyValue(reader, "Location", typeof(Point), Point.Empty);
					_rowIndex = (int)LayoutReader.GetPropertyValue(reader, "RowIndex", typeof(int), -1);
				}
			}
			finally
			{
				_isLoadingLayout = false;
			}
			_mergedLocation = (Point)LayoutReader.GetPropertyValue(reader, "MergedLocation", typeof(Point), Point.Empty);
			_mergeRowOrder = (int)LayoutReader.GetPropertyValue(reader, "MergeRowOrder", typeof(int), -1);
			SetClosed((bool)LayoutReader.GetPropertyValue(reader, "Closed", typeof(bool), false));
			if (CommandManager != null && !GetIsMergedCommandBar())
			{
				BindCommands();
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_allowCustomize != InheritableBoolean.Default)
			{
				info.AddValue("AllowCustomize", _allowCustomize);
			}
			if (_allowClose != InheritableBoolean.Default)
			{
				info.AddValue("AllowClose", _allowClose);
			}
			if (_animation != DropDownAnimation.Default)
			{
				info.AddValue("Animation", _animation);
			}
			if (_allowDelete)
			{
				info.AddValue("AllowDelete", _allowDelete);
			}
			if (_allowMerge != InheritableBoolean.Default)
			{
				info.AddValue("AllowMerge", _allowMerge);
			}
			if (_allowRename)
			{
				info.AddValue("AllowRename", _allowRename);
			}
			if (_commandBarType != CommandBarType.ToolBar)
			{
				info.AddValue("CommandBarType", _commandBarType);
			}
			if (_commands != null && (_commands.Count > 0 || _commands._mergedCommands != null))
			{
				info.AddValue("Commands", _commands);
			}
			if (_commandsStateStyles != null && _commandsStateStyles.ShouldSerialize())
			{
				info.AddValue("CommandsStateStyles", _commandsStateStyles);
			}
			if (_commandsStyle != CommandStyle.Default)
			{
				info.AddValue("CommandsStyle", _commandsStyle);
			}
			if (_dockStyle != BarsDockStyle.Top)
			{
				info.AddValue("DockStyle", _dockStyle);
			}
			if (_dockedDockStyle != BarsDockStyle.Top)
			{
				info.AddValue("DockedDockStyle", _dockedDockStyle);
			}
			if (_dockedRowIndex != -1)
			{
				info.AddValue("DockedRowIndex", _dockedRowIndex);
			}
			if (!_dockedLocation.IsEmpty)
			{
				info.AddValue("DockedLocation", _dockedLocation);
			}
			if (!Location.IsEmpty)
			{
				info.AddValue("Location", Location);
			}
			if (!_floatingLocation.IsEmpty)
			{
				info.AddValue("FloatingLocation", _floatingLocation);
			}
			if (!_floatingSize.IsEmpty)
			{
				info.AddValue("FloatingSize", _floatingSize);
			}
			if (_formatStyle != null && !_formatStyle.AreAllPropertiesDefault())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (_isMergedCommandBar)
			{
				info.AddValue("IsMergedCommandBar", _isMergedCommandBar);
			}
			if (_key.Length > 0)
			{
				info.AddValue("Key", _key);
			}
			if (_lockCommandBar != InheritableBoolean.Default)
			{
				info.AddValue("LockCommandBar", _lockCommandBar);
			}
			if (_isMergedCommandBar && _parentID != Guid.Empty)
			{
				info.AddValue("ParentID", _parentID);
			}
			if (!GetMergedLocation().IsEmpty)
			{
				info.AddValue("MergedLocation", GetMergedLocation());
			}
			if (MergeRowOrder != -1)
			{
				info.AddValue("MergeRowOrder", MergeRowOrder);
			}
			if (!_position.IsEmpty)
			{
				info.AddValue("Position", _position);
			}
			if (_popupSettings != null && _popupSettings.ShouldSerialize())
			{
				info.AddValue("PopupSettings", _popupSettings);
			}
			if (_popupView != View.Default)
			{
				info.AddValue("PopupView", _popupView);
			}
			if (RowIndex != -1)
			{
				info.AddValue("RowIndex", RowIndex);
			}
			if (_showCustomizeButton != InheritableBoolean.Default)
			{
				info.AddValue("ShowCustomizeButton", _showCustomizeButton);
			}
			if (_showToolTips != InheritableBoolean.Default)
			{
				info.AddValue("ShowToolTips", _showToolTips);
			}
			if (_textAlign != TextAlign.Default)
			{
				info.AddValue("TextAlign", _textAlign);
			}
			if (_view != View.Default)
			{
				info.AddValue("View", _view);
			}
			if (_isClosed)
			{
				info.AddValue("Closed", IsClosed());
			}
			if (_fullRow)
			{
				info.AddValue("FullRow", _fullRow);
			}
			if (_commandBarWrappable != InheritableBoolean.Default)
			{
				info.AddValue("Wrappable", _commandBarWrappable);
			}
		}
	}
}
