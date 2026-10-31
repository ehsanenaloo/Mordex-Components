using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandManager
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			ReadManagerState(reader);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			WriteManagerState(info);
			if (_layouts.Count > 0)
			{
				info.AddValue("Layouts", _layouts);
			}
		}

		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				if (_isInitializing)
				{
					((ISupportInitialize)this).EndInit();
					MemoryStream memoryStream = new MemoryStream();
					SaveLayoutFile(memoryStream);
					byte[] defaultLayoutSettings = memoryStream.ToArray();
					GetApplicationSettings().DefaultLayoutSettings = defaultLayoutSettings;
					memoryStream.Close();
					memoryStream.Dispose();
					memoryStream = null;
				}
				byte[] layoutSettings = GetApplicationSettings().LayoutSettings;
				if (layoutSettings != null && layoutSettings.Length > 0)
				{
					MemoryStream settingsStream = new MemoryStream(layoutSettings);
					LoadLayoutFile(settingsStream);
					settingsStream.Close();
					settingsStream.Dispose();
					settingsStream = null;
				}
			}
		}

		public void ResetComponentSettings()
		{
			if (!base.DesignMode)
			{
				GetApplicationSettings().Reset();
				LoadComponentSettings();
			}
		}

		public void SaveComponentSettings()
		{
			MemoryStream memoryStream = new MemoryStream();
			SaveLayoutFile(memoryStream);
			byte[] layoutSettings = memoryStream.ToArray();
			GetApplicationSettings().LayoutSettings = layoutSettings;
			GetApplicationSettings().Save();
			memoryStream.Close();
			memoryStream.Dispose();
			memoryStream = null;
		}

		private void RememberCurrentLayout()
		{
			_appliedLayout = _currentLayout;
			CommandBarsLayout uiCommandBarsLayout = null;
			if (_currentLayout != null)
			{
				uiCommandBarsLayout = _currentLayout.ResolveMergedLayout();
			}
			if (uiCommandBarsLayout == null || uiCommandBarsLayout.LayoutString == "")
			{
				ReadManagerState(new LayoutReader());
			}
			else
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(uiCommandBarsLayout);
				mordexLayoutReader["UICommandManager"]?.DeserializeObject(this);
				if (!_isInitializing)
				{
					OnLayoutLoad(EventArgs.Empty);
				}
			}
			OnCurrentLayoutChanged(EventArgs.Empty);
		}

		public void LoadLayout(CommandBarsLayout layout)
		{
			if (layout == null)
			{
				throw new ArgumentNullException();
			}
			if (!layout.IsResolvable())
			{
				throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
			}
			layout = layout.ResolveMergedLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
			mordexLayoutReader["UICommandManager"]?.DeserializeObject(this);
		}

		public void LoadLayoutFile(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["CommandManagerLayoutFile"];
			if (propertyValue != null)
			{
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				ReadManagerState(layoutReader);
				_layouts.Clear();
				layoutReader["Layouts"]?.DeserializeObject(_layouts);
				CommandBarsLayout uiCommandBarsLayout = _layouts.FindCurrentLayout();
				if (uiCommandBarsLayout != null)
				{
					CurrentLayout = uiCommandBarsLayout;
				}
				else
				{
					CurrentLayout = null;
					OnLayoutLoad(EventArgs.Empty);
				}
			}
		}

		public void SaveLayoutFile(Stream stream)
		{
			MxCommandManager uiCommandManager = _mergedManager;
			if (_mergedManager != null)
			{
				SetMergedManager(null);
			}
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			PropertyValue propertyValue = new PropertyValue("CommandManagerLayoutFile", this);
			propertyValue.LayoutType = "Mordex.Windows.UI.CommandBars.MxCommandManager";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
			if (uiCommandManager != null)
			{
				SetMergedManager(uiCommandManager);
			}
		}

		public CommandBarsLayout GetLayout()
		{
			CommandBarsLayout uiCommandBarsLayout = new CommandBarsLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			WriteManagerState(mordexLayoutWriter);
			LayoutWriter.StoreInLayout("UICommandManager", mordexLayoutWriter, uiCommandBarsLayout);
			return uiCommandBarsLayout;
		}

		internal void WriteManagerState(LayoutWriter mordexLayoutWriter)
		{
			MxCommandManager uiCommandManager = _mergedManager;
			if (_mergedManager != null)
			{
				SetMergedManager(null);
			}
			if (_allowClose != InheritableBoolean.Default)
			{
				mordexLayoutWriter.AddValue("AllowClose", _allowClose);
			}
			if (_allowCustomize != InheritableBoolean.Default)
			{
				mordexLayoutWriter.AddValue("AllowCustomize", _allowCustomize);
			}
			if (!_allowMerge)
			{
				mordexLayoutWriter.AddValue("AllowMerge", _allowMerge);
			}
			if (_alwaysShowFullMenus)
			{
				mordexLayoutWriter.AddValue("AlwaysShowFullMenu", _alwaysShowFullMenus);
			}
			if (_bottomRebar != null)
			{
				mordexLayoutWriter.AddValue("BottomRebar", _bottomRebar);
			}
			if (_categories != null && _categories.Count > 0)
			{
				mordexLayoutWriter.AddValue("Categories", _categories);
			}
			if (_commandBars != null && _commandBars.Count > 0)
			{
				mordexLayoutWriter.AddValue("CommandBars", _commandBars);
			}
			if (_commandBarsFormatStyle != null && !_commandBarsFormatStyle.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("CommandBarsFormatStyle", _commandBarsFormatStyle);
			}
			if (_commands != null && _commands.Count > 0)
			{
				mordexLayoutWriter.AddValue("Commands", _commands);
			}
			if (_commandsStateStyles != null && _commandsStateStyles.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("CommandsStateStyles", _commandsStateStyles);
			}
			if (_commandsStyle != CommandStyle.Default)
			{
				mordexLayoutWriter.AddValue("CommandsStyle", _commandsStyle);
			}
			if (_contextMenus != null && _contextMenus.Count > 0)
			{
				mordexLayoutWriter.AddValue("ContextMenus", _contextMenus);
			}
			if (_daysForInfrequency != 0)
			{
				mordexLayoutWriter.AddValue("DaysForInfrequency", _daysForInfrequency);
			}
			if (_leftRebar != null)
			{
				mordexLayoutWriter.AddValue("LeftRebar", _leftRebar);
			}
			if (ShouldSerializeBuiltInTextsData())
			{
				mordexLayoutWriter.AddValue("BuiltInTexts", _builtInTexts);
			}
			if (_lockCommandBars)
			{
				mordexLayoutWriter.AddValue("LockCommandBars", _lockCommandBars);
			}
			if (_menuAnimation != DropDownAnimation.System)
			{
				mordexLayoutWriter.AddValue("MenuAnimation", _menuAnimation);
			}
			if (_popupSettings != null && _popupSettings.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("PopupSettings", _popupSettings);
			}
			if (_popupView != View.SmallIcons)
			{
				mordexLayoutWriter.AddValue("PopupView", _popupView);
			}
			if (_rightRebar != null)
			{
				mordexLayoutWriter.AddValue("RightRebar", _rightRebar);
			}
			if (_shortcutBehavior != ShortcutBehavior.Always)
			{
				mordexLayoutWriter.AddValue("ShortcutBehavior", _shortcutBehavior);
			}
			if (_showCustomizeButton != InheritableBoolean.Default)
			{
				mordexLayoutWriter.AddValue("ShowCustomizeButton", _showCustomizeButton);
			}
			if (_showFullMenuDelay != DefaultShowFullMenuDelay)
			{
				mordexLayoutWriter.AddValue("ShowFullMenuDelay", _showFullMenuDelay);
			}
			if (!_showQuickCustomizeMenu)
			{
				mordexLayoutWriter.AddValue("ShowQuickCustomizeMenu", _showQuickCustomizeMenu);
			}
			if (!_showShadowUnderMenus)
			{
				mordexLayoutWriter.AddValue("ShowShadowUnderMenus", _showShadowUnderMenus);
			}
			if (_showShortcutInToolTips)
			{
				mordexLayoutWriter.AddValue("ShowShortcutInToolTips", _showShortcutInToolTips);
			}
			if (!_showToolTips)
			{
				mordexLayoutWriter.AddValue("ShowToolTips", _showToolTips);
			}
			if (_showToolTipOnMenus)
			{
				mordexLayoutWriter.AddValue("ShowToolTipOnMenus", ShowToolTipOnMenus);
			}
			if (TextAlign != TextAlign.Right)
			{
				mordexLayoutWriter.AddValue("TextAlign", _textAlign);
			}
			if (_topRebar != null)
			{
				mordexLayoutWriter.AddValue("TopRebar", _topRebar);
			}
			if (_useCompatibleTextRendering.HasValue)
			{
				mordexLayoutWriter.AddValue("UseCompatibleTextRendering", _useCompatibleTextRendering);
			}
			if (!_useFontInFontNames)
			{
				mordexLayoutWriter.AddValue("UseFontInFontNames", _useFontInFontNames);
			}
			if (!_useMnemonic)
			{
				mordexLayoutWriter.AddValue("UseMnemonic", _useMnemonic);
			}
			if (_view != View.SmallIcons)
			{
				mordexLayoutWriter.AddValue("View", _view);
			}
			if (uiCommandManager != null)
			{
				SetMergedManager(uiCommandManager);
			}
			RefreshRebars();
		}

		internal void ReadManagerState(LayoutReader reader)
		{
			_isBatchUpdating = true;
			_isLoadingLayout = true;
			try
			{
				object propertyValue = LayoutReader.GetPropertyValue(reader, "UseCompatibleTextRendering", typeof(bool), null);
				if (propertyValue != null)
				{
					_useCompatibleTextRendering = (bool)propertyValue;
				}
				_useMnemonic = (bool)LayoutReader.GetPropertyValue(reader, "UseMnemonic", typeof(bool), true);
				_view = (View)LayoutReader.GetPropertyValue(reader, "View", typeof(View), View.SmallIcons);
				_shortcutBehavior = (ShortcutBehavior)LayoutReader.GetPropertyValue(reader, "ShortcutBehavior", typeof(ShortcutBehavior), ShortcutBehavior.Always);
				PropertyValue categoriesValue = reader["Categories"];
				if (categoriesValue != null)
				{
					categoriesValue.DeserializeObject(_categories);
				}
				else
				{
					_categories.Clear();
				}
				_daysForInfrequency = (int)LayoutReader.GetPropertyValue(reader, "DaysForInfrequency", typeof(int), 0);
				PropertyValue commandsValue = reader["Commands"];
				MxCommandCollection uiCommandCollection = new MxCommandCollection(this);
				LayoutReader mordexLayoutReader = null;
				ArrayList arrayList = new ArrayList();
				foreach (MxCommand item in _commands)
				{
					arrayList.Add(item);
				}
				if (commandsValue != null)
				{
					mordexLayoutReader = commandsValue.GetLayoutReader();
					if (mordexLayoutReader != null)
					{
						int commandCount = mordexLayoutReader.Count;
						if (mordexLayoutReader["MergedCommands"] != null)
						{
							commandCount--;
						}
						for (int i = 0; i < commandCount; i++)
						{
							PropertyValue commandValue = mordexLayoutReader[i];
							MxCommand uiCommand = _commands.FindByKey(commandValue.Name);
							if (uiCommand == null)
							{
								uiCommand = new MxCommand(commandValue.Name);
								uiCommand._commandManager = this;
							}
							else
							{
								arrayList.Remove(uiCommand);
							}
							commandValue.DeserializeObject(uiCommand);
							uiCommandCollection.AddKeepingPositions(uiCommand);
						}
						for (int j = 0; j < _commands.Count; j++)
						{
							MxCommand command = _commands[j];
							if (!uiCommandCollection.Contains(command))
							{
								_commands.Remove(command);
								j--;
							}
						}
						_commands.DetachAllAndReset();
						_commands = uiCommandCollection;
						for (int k = 0; k < commandCount; k++)
						{
							PropertyValue loadedCommandValue = mordexLayoutReader[k];
							_commands.FindByKey(loadedCommandValue.Name)?.ApplyLayoutState(loadedCommandValue, this);
						}
					}
					_commands.DeserializeMergedCommands(commandsValue);
				}
				else
				{
					_commands.DetachAllAndReset();
					_commands = uiCommandCollection;
				}
				foreach (MxCommand staleCommand in arrayList)
				{
					staleCommand.Dispose();
				}
				PropertyValue commandBarsValue = reader["CommandBars"];
				CommandBarCollection uiCommandBarCollection = new CommandBarCollection(this);
				if (commandBarsValue != null)
				{
					LayoutReader layoutReader = commandBarsValue.GetLayoutReader();
					if (layoutReader != null)
					{
						int commandBarCount = layoutReader.Count;
						if (layoutReader["MergedBars"] != null)
						{
							commandBarCount--;
						}
						for (int barIndex = 0; barIndex < commandBarCount; barIndex++)
						{
							PropertyValue commandBarValue = layoutReader[barIndex];
							MxCommandBar uiCommandBar = _commandBars.FindBarByKey(commandBarValue.Name);
							if (uiCommandBar == null)
							{
								uiCommandBar = new MxCommandBar(commandBarValue.Name);
								uiCommandBar._commandManager = this;
							}
							commandBarValue.DeserializeObject(uiCommandBar);
							uiCommandBarCollection.AddWithoutEvents(uiCommandBar);
							uiCommandBar._commandManager = this;
							if (base.DesignMode && base.Container != null && uiCommandBar.Container == null)
							{
								base.Container.Add(uiCommandBar);
							}
						}
						PropertyValue mergedBarsValue = layoutReader["MergedBars"];
						if (mergedBarsValue != null)
						{
							LayoutReader mergedBarsReader = mergedBarsValue.GetLayoutReader();
							if (mergedBarsReader != null)
							{
								uiCommandBarCollection._mergedBars = new CommandBarCollection(this, validateBars: false);
								for (int m = 0; m < mergedBarsReader.Count; m++)
								{
									PropertyValue mergedBarValue = mergedBarsReader[m];
									MxCommandBar mergedBar = _commandBars.FindBarByKey(mergedBarValue.Name);
									if (mergedBar == null)
									{
										mergedBar = new MxCommandBar(mergedBarValue.Name);
									}
									mergedBar.SetIsMergedCommandBar(true);
									mergedBarValue.DeserializeObject(mergedBar);
									uiCommandBarCollection._mergedBars.Add(mergedBar);
								}
							}
						}
					}
				}
				for (int n = 0; n < _commandBars.Count; n++)
				{
					MxCommandBar existingBar = _commandBars[n];
					if (!uiCommandBarCollection.Contains(existingBar))
					{
						_commandBars.Remove(existingBar);
						n--;
					}
					else
					{
						((ICommandBarOwner)this).OnBarAdded(existingBar);
					}
				}
				_commandBars = uiCommandBarCollection;
				_commandBars.UpdateDetachedIndexes();
				PropertyValue contextMenusValue = reader["ContextMenus"];
				ContextMenuCollection uiContextMenuCollection = new ContextMenuCollection(this);
				if (contextMenusValue != null)
				{
					LayoutReader contextMenusReader = contextMenusValue.GetLayoutReader();
					if (contextMenusReader != null)
					{
						for (int menuIndex = 0; menuIndex < contextMenusReader.Count; menuIndex++)
						{
							PropertyValue contextMenuValue = contextMenusReader[menuIndex];
							MxContextMenu uiContextMenu = _contextMenus.FindByKey(contextMenuValue.Name);
							if (uiContextMenu == null)
							{
								uiContextMenu = new MxContextMenu(contextMenuValue.Name);
								uiContextMenu._commandManager = this;
							}
							contextMenuValue.DeserializeObject(uiContextMenu);
							uiContextMenuCollection.Add(uiContextMenu);
						}
					}
				}
				for (int existingMenuIndex = 0; existingMenuIndex < _contextMenus.Count; existingMenuIndex++)
				{
					MxContextMenu contextMenu = _contextMenus[existingMenuIndex];
					if (!uiContextMenuCollection.Contains(contextMenu))
					{
						_contextMenus.Remove(contextMenu);
						existingMenuIndex--;
					}
				}
				_contextMenus = uiContextMenuCollection;
				_contextMenus.RenumberIndexes();
				reader["BottomRebar"]?.DeserializeObject(BottomRebar);
				reader["RightRebar"]?.DeserializeObject(RightRebar);
				reader["TopRebar"]?.DeserializeObject(TopRebar);
				reader["LeftRebar"]?.DeserializeObject(LeftRebar);
				if (TopRebar.HasAddedBar())
				{
					ShowRebar(TopRebar);
				}
				if (LeftRebar.HasAddedBar())
				{
					ShowRebar(LeftRebar);
				}
				if (RightRebar.HasAddedBar())
				{
					ShowRebar(RightRebar);
				}
				if (BottomRebar.HasAddedBar())
				{
					ShowRebar(BottomRebar);
				}
				_allowClose = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowClose", typeof(InheritableBoolean), InheritableBoolean.Default);
				_allowCustomize = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "AllowCustomize", typeof(InheritableBoolean), InheritableBoolean.Default);
				_showCustomizeButton = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ShowCustomizeButton", typeof(InheritableBoolean), InheritableBoolean.Default);
				_alwaysShowFullMenus = (bool)LayoutReader.GetPropertyValue(reader, "AlwaysShowFullMenu", typeof(bool), false);
				_commandBarsFormatStyle = (FormatStyle)LayoutReader.GetPropertyValue(reader, "CommandBarsFormatStyle", typeof(FormatStyle), null);
				if (_commandBarsFormatStyle != null)
				{
					_commandBarsFormatStyle._owner = this;
				}
				_commandsStateStyles = null;
				reader["CommandsStateStyles"]?.DeserializeObject(CommandsStateStyles);
				_commandsStyle = (CommandStyle)LayoutReader.GetPropertyValue(reader, "CommandsStyle", typeof(CommandStyle), CommandStyle.Default);
				_lockCommandBars = (bool)LayoutReader.GetPropertyValue(reader, "LockCommandBars", typeof(bool), false);
				_menuAnimation = (DropDownAnimation)LayoutReader.GetPropertyValue(reader, "MenuAnimation", typeof(DropDownAnimation), DropDownAnimation.System);
				_popupSettings = (PopupSettings)LayoutReader.GetPropertyValue(reader, "PopupSettings", typeof(PopupSettings), null);
				if (_popupSettings != null)
				{
					_popupSettings._owner = this;
				}
				_popupView = (View)LayoutReader.GetPropertyValue(reader, "PopupView", typeof(View), View.SmallIcons);
				reader["BuiltInTexts"]?.DeserializeObject(_builtInTexts);
				_allowMerge = (bool)LayoutReader.GetPropertyValue(reader, "AllowMerge", typeof(bool), true);
				_showQuickCustomizeMenu = (bool)LayoutReader.GetPropertyValue(reader, "ShowQuickCustomizeMenu", typeof(bool), true);
				_showToolTips = (bool)LayoutReader.GetPropertyValue(reader, "ShowToolTips", typeof(bool), true);
				_showShadowUnderMenus = (bool)LayoutReader.GetPropertyValue(reader, "ShowShadowUnderMenus", typeof(bool), true);
				_showToolTipOnMenus = (bool)LayoutReader.GetPropertyValue(reader, "ShowToolTipOnMenus", typeof(bool), false);
				_showFullMenuDelay = (int)LayoutReader.GetPropertyValue(reader, "ShowFullMenuDelay", typeof(int), DefaultShowFullMenuDelay);
				_showShortcutInToolTips = (bool)LayoutReader.GetPropertyValue(reader, "ShowShortcutInToolTips", typeof(bool), false);
				_textAlign = (TextAlign)LayoutReader.GetPropertyValue(reader, "TextAlign", typeof(TextAlign), TextAlign.Right);
				_useFontInFontNames = (bool)LayoutReader.GetPropertyValue(reader, "UseFontInFontNames", typeof(bool), true);
				_showCustomizeButton = (InheritableBoolean)LayoutReader.GetPropertyValue(reader, "ShowCustomizeButton", typeof(InheritableBoolean), InheritableBoolean.Default);
				RefreshAppearance();
			}
			finally
			{
				_isLoadingLayout = false;
				_isBatchUpdating = false;
			}
			_isToolbarsMenuDirty = true;
			HideEmptyRebars();
			RefreshRebars();
			TopRebar.ReapplyBarOffsets();
			BottomRebar.ReapplyBarOffsets();
			RightRebar.ReapplyBarOffsets();
			LeftRebar.ReapplyBarOffsets();
			TopRebar.ApplyRowRectangles();
			LeftRebar.ApplyRowRectangles();
			RightRebar.ApplyRowRectangles();
			BottomRebar.ApplyRowRectangles();
		}
	}
}
