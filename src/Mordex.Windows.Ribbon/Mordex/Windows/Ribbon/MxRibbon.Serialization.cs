using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				if (_isInitializing)
				{
					((ISupportInitialize)this).EndInit();
					using MemoryStream memoryStream = new MemoryStream();
					SaveLayoutFile(memoryStream);
					byte[] defaultLayoutSettings = memoryStream.ToArray();
					GetApplicationSettings().DefaultLayoutSettings = defaultLayoutSettings;
				}
				byte[] layoutSettings = GetApplicationSettings().LayoutSettings;
				if (layoutSettings != null && layoutSettings.Length > 0)
				{
					using MemoryStream settingsStream = new MemoryStream(layoutSettings);
					LoadLayoutFile(settingsStream);
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
			using MemoryStream memoryStream = new MemoryStream();
			SaveLayoutFile(memoryStream);
			memoryStream.Flush();
			memoryStream.Position = 0L;
			byte[] layoutSettings = memoryStream.ToArray();
			ApplicationSettings settings = GetApplicationSettings();
			settings.LayoutSettings = layoutSettings;
			settings.Save();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBuiltInTextsData()
		{
			return BuiltInTexts.HasCustomTexts();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBuiltInTextsData()
		{
			BuiltInTexts.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeImageSize()
		{
			if (_largeImageSize.Width != 32 || _largeImageSize.Height != 32)
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeImageSize()
		{
			LargeImageSize = new Size(32, 32);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMinimumFormSize()
		{
			return !_minimumFormSize.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMinimumFormSize()
		{
			MinimumFormSize = Size.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				RelayoutForFormState();
				InvalidateTabContent();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSmallImageSize()
		{
			if (_smallImageSize.Width != 16 || _smallImageSize.Height != 16)
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSmallImageSize()
		{
			SmallImageSize = new Size(16, 16);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColorLight()
		{
			return !ForeColorLight.Equals(_colorScheme._foreColorLight);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetForeColorLight()
		{
			ForeColorLight = _colorScheme._foreColorLight;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColorDark()
		{
			return !ForeColorDark.Equals(_colorScheme._foreColorDark);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetForeColorDark()
		{
			ForeColorDark = _colorScheme._foreColorDark;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDocumentName()
		{
			return _documentName.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDocumentName()
		{
			DocumentName = "";
		}

		public bool ShouldSerializeBackstageMenuData()
		{
			return _backstageMenu.IsModifiedFromDefaults();
		}

		public void ResetBackstageMenuData()
		{
			_backstageMenu.ResetToDefaults();
		}

		private void OnControlComponentRename(object sender, ComponentRenameEventArgs eventArgs)
		{
			if (eventArgs.Component is CommandBase)
			{
				CommandBase commandBase = (CommandBase)eventArgs.Component;
				if (commandBase.Ribbon == this)
				{
					commandBase.SyncNameFromSite();
				}
			}
			else if (eventArgs.Component is RibbonTab)
			{
				RibbonTab ribbonTab = (RibbonTab)eventArgs.Component;
				if (ribbonTab.Ribbon == this)
				{
					ribbonTab.SyncNameFromSite();
				}
			}
			else if (eventArgs.Component is RibbonContextualTab)
			{
				RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)eventArgs.Component;
				if (ribbonContextualTab.Ribbon == this)
				{
					ribbonContextualTab.SyncNameFromSite();
				}
			}
		}

		internal void DesignerSelectCommand(CommandBase component)
		{
			if (_designer != null)
			{
				DesignHelper.Designer_SelectComponent(_designer, component);
			}
		}

		internal void DesignerSelectComponent(Component component)
		{
			if (_designer != null)
			{
				DesignHelper.Designer_SelectComponent(_designer, component);
			}
		}

		private void ApplyCurrentLayout()
		{
			_appliedLayout = _currentLayout;
			RibbonLayout ribbonLayout = null;
			if (_currentLayout != null)
			{
				ribbonLayout = _currentLayout.GetMergedLayout();
			}
			// Without a current layout the ribbon keeps its present content.
			if (ribbonLayout != null && ribbonLayout.LayoutString == "")
			{
				DeserializeRibbonState(new LayoutReader());
			}
			else if (ribbonLayout != null)
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(ribbonLayout);
				mordexLayoutReader["RibbonLayoutData"]?.DeserializeObject(this);
				if (!_isInitializing)
				{
					OnLayoutLoad(EventArgs.Empty);
				}
			}
			OnCurrentLayoutChanged(EventArgs.Empty);
		}

		public void LoadLayout(RibbonLayout layout)
		{
			if (layout == null)
			{
				throw new ArgumentNullException();
			}
			if (!layout.HasStandAloneBase())
			{
				throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
			}
			layout = layout.GetMergedLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
			mordexLayoutReader["RibbonLayoutData"]?.DeserializeObject(this);
		}

		public void LoadLayoutFile(Stream stream)
		{
			MxRibbon ribbon = _mergedRibbon;
			if (GetMergedRibbon() != null)
			{
				SetMergedRibbon(null);
			}
			try
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
				PropertyValue propertyValue = mordexLayoutReader["RibbonLayoutFile"];
				if (propertyValue != null)
				{
					LayoutReader layoutReader = propertyValue.GetLayoutReader();
					DeserializeRibbonState(layoutReader);
					_layouts.Clear();
					layoutReader["Layouts"]?.DeserializeObject(_layouts);
					RibbonLayout ribbonLayout = _layouts.FindCurrentLayout();
					if (ribbonLayout != null)
					{
						CurrentLayout = ribbonLayout;
					}
					else
					{
						CurrentLayout = null;
						OnLayoutLoad(EventArgs.Empty);
					}
				}
			}
			finally
			{
				if (ribbon != null)
				{
					SetMergedRibbon(ribbon);
				}
			}
		}

		public void SaveLayoutFile(Stream stream)
		{
			MxRibbon ribbon = _mergedRibbon;
			MxRibbon ownerRibbon = _mergeHost;
			try
			{
				if (ribbon != null)
				{
					ribbon._isMergeInProgress = true;
				}
				if (GetMergedRibbon() != null)
				{
					SetMergedRibbon(null);
				}
				else if (_mergeHost != null)
				{
					_mergeHost.SetMergedRibbon(null);
				}
				if (_currentLayout != null)
				{
					_currentLayout.Update();
				}
				PropertyValue propertyValue = new PropertyValue("RibbonLayoutFile", this);
				propertyValue.LayoutType = "Mordex.Windows.Ribbon.MxRibbon";
				LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
				mordexLayoutWriter.FillStream(stream);
			}
			finally
			{
				if (ribbon != null)
				{
					try
					{
						SetMergedRibbon(ribbon);
					}
					finally
					{
						ribbon._isMergeInProgress = false;
					}
				}
				ownerRibbon?.SetMergedRibbon(this);
			}
		}

		public RibbonLayout GetLayout()
		{
			RibbonLayout ribbonLayout = new RibbonLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			SerializeRibbonState(mordexLayoutWriter);
			LayoutWriter.StoreInLayout("RibbonLayoutData", mordexLayoutWriter, ribbonLayout);
			return ribbonLayout;
		}

		internal void SerializeRibbonState(LayoutWriter mordexLayoutWriter)
		{
			if (!_allowMerge)
			{
				mordexLayoutWriter.AddValue("AllowMerge", _allowMerge);
			}
			if (_applicationName.Length > 0)
			{
				mordexLayoutWriter.AddValue("ApplicationName", _applicationName);
			}
			if (ShouldSerializeBuiltInTextsData())
			{
				mordexLayoutWriter.AddValue("BuiltInTexts", _builtInTexts);
			}
			if (_contextualTabs.Count > 0)
			{
				mordexLayoutWriter.AddValue("ContextualTabs", _contextualTabs);
			}
			mordexLayoutWriter.AddValue("ControlBoxMenu", _controlBoxMenu);
			mordexLayoutWriter.AddValue("BackstageMenu", BackstageMenu);
			mordexLayoutWriter.AddValue("HelpButton", _helpButton);
			if (_documentName.Length > 0)
			{
				mordexLayoutWriter.AddValue("DocumentName", _documentName);
			}
			if (!_minimizeOnDoubleClick)
			{
				mordexLayoutWriter.AddValue("MinimizeOnDoubleClick", _minimizeOnDoubleClick);
			}
			if (ShouldSerializeForeColorDark())
			{
				mordexLayoutWriter.AddValue("ForeColorDark", _foreColorDark);
			}
			if (ShouldSerializeForeColorLight())
			{
				mordexLayoutWriter.AddValue("ForeColorLight", _foreColorLight);
			}
			if (!HandlesTitleBar)
			{
				mordexLayoutWriter.AddValue("HandlesTitleBar", HandlesTitleBar);
			}
			if (_itemRows != 3)
			{
				mordexLayoutWriter.AddValue("ItemRows", _itemRows);
			}
			if (ShouldSerializeLargeImageSize())
			{
				mordexLayoutWriter.AddValue("LargeImageSize", _largeImageSize);
			}
			if (Minimized)
			{
				mordexLayoutWriter.AddValue("Minimized", _minimized);
			}
			if (!MinimumFormSize.IsEmpty)
			{
				mordexLayoutWriter.AddValue("MinimumFormSize", _minimumFormSize);
			}
			if (_quickAccessPosition != QuickAccessPosition.AboveRibbon)
			{
				mordexLayoutWriter.AddValue("QuickAccessPosition", _quickAccessPosition);
			}
			if (_selectedTab != null && _tabs.Contains(_selectedTab))
			{
				mordexLayoutWriter.AddValue("SelectedTabIndex", _tabs.IndexOf(_selectedTab));
			}
			if (!ShowCaptionBar)
			{
				mordexLayoutWriter.AddValue("ShowCaptionBar", _showCaptionBar);
			}
			if (!_showControlBoxButton)
			{
				mordexLayoutWriter.AddValue("ShowControlBoxButton", _showControlBoxButton);
			}
			if (_controlBoxMode != ControlBoxMode.Default)
			{
				mordexLayoutWriter.AddValue("ControlBoxMode", _controlBoxMode);
			}
			if (!_showCustomizeButton)
			{
				mordexLayoutWriter.AddValue("ShowCustomizeButton", _showCustomizeButton);
			}
			if (!_showQuickCustomizeMenu)
			{
				mordexLayoutWriter.AddValue("ShowQuickCustomizeMenu", _showQuickCustomizeMenu);
			}
			if (_showShortcutInToolTips)
			{
				mordexLayoutWriter.AddValue("ShowShortcutInToolTips", _showShortcutInToolTips);
			}
			if (_showShortcutInMenus)
			{
				mordexLayoutWriter.AddValue("ShowShortcutInMenus", _showShortcutInMenus);
			}
			if (!_showToolTips)
			{
				mordexLayoutWriter.AddValue("ShowToolTips", _showToolTips);
			}
			if (ShouldSerializeSmallImageSize())
			{
				mordexLayoutWriter.AddValue("SmallImageSize", _smallImageSize);
			}
			if (_superTipDelay != 9000)
			{
				mordexLayoutWriter.AddValue("SuperTipDelay", _superTipDelay);
			}
			if (_superTipWidth != 210)
			{
				mordexLayoutWriter.AddValue("SuperTipWidth", _superTipWidth);
			}
			if (_tabMinWidth != 34)
			{
				mordexLayoutWriter.AddValue("TabMinWidth", _tabMinWidth);
			}
			if (_tabStripHeight != -1)
			{
				mordexLayoutWriter.AddValue("TabStripHeight", _tabStripHeight);
			}
			if (_firstTabOffset != 48)
			{
				mordexLayoutWriter.AddValue("FirstTabOffset", _firstTabOffset);
			}
			if (_tabs.Count > 0)
			{
				mordexLayoutWriter.AddValue("Tabs", _tabs);
			}
			if (_titleBarFont != null)
			{
				mordexLayoutWriter.AddValue("TitleBarFont", _titleBarFont);
			}
			if (_quickAccessCommands.Count > 0)
			{
				mordexLayoutWriter.AddValue("QuickAccessCommands", _quickAccessCommands);
			}
			if (_quickAccessCustomizeList.Count > 0)
			{
				mordexLayoutWriter.AddValue("QuickAccessCustomizeList", _quickAccessCustomizeList);
			}
			if (_useCompatibleTextRendering.HasValue)
			{
				mordexLayoutWriter.AddValue("UseCompatibleTextRendering", _useCompatibleTextRendering);
			}
		}

		internal void DeserializeRibbonState(LayoutReader reader)
		{
			_allowMerge = (bool)LayoutReader.GetPropertyValue(reader, "AllowMerge", typeof(bool), true);
			reader["BuiltInTexts"]?.DeserializeObject(_builtInTexts);
			_superTipDelay = (int)LayoutReader.GetPropertyValue(reader, "SuperTipDelay", typeof(int), 9000);
			_largeImageSize = (Size)LayoutReader.GetPropertyValue(reader, "LargeImageSize", typeof(Size), new Size(32, 32));
			_itemRows = Math.Max(1, (int)LayoutReader.GetPropertyValue(reader, "ItemRows", typeof(int), 3));
			_quickAccessPosition = (QuickAccessPosition)LayoutReader.GetPropertyValue(reader, "QuickAccessPosition", typeof(QuickAccessPosition), QuickAccessPosition.AboveRibbon);
			_showCustomizeButton = (bool)LayoutReader.GetPropertyValue(reader, "ShowCustomizeButton", typeof(bool), true);
			_showShortcutInToolTips = (bool)LayoutReader.GetPropertyValue(reader, "ShowShortcutInToolTips", typeof(bool), false);
			_showShortcutInMenus = (bool)LayoutReader.GetPropertyValue(reader, "ShowShortcutInMenus", typeof(bool), false);
			_showToolTips = (bool)LayoutReader.GetPropertyValue(reader, "ShowToolTips", typeof(bool), true);
			_smallImageSize = (Size)LayoutReader.GetPropertyValue(reader, "SmallImageSize", typeof(Size), new Size(16, 16));
			_superTipWidth = (int)LayoutReader.GetPropertyValue(reader, "SuperTipWidth", typeof(int), 210);
			_tabMinWidth = (int)LayoutReader.GetPropertyValue(reader, "TabMinWidth", typeof(int), 34);
			_applicationName = (string)LayoutReader.GetPropertyValue(reader, "ApplicationName", typeof(string), "");
			_documentName = (string)LayoutReader.GetPropertyValue(reader, "DocumentName", typeof(string), "");
			_minimizeOnDoubleClick = (bool)LayoutReader.GetPropertyValue(reader, "MinimizeOnDoubleClick", typeof(bool), true);
			_foreColorDark = (Color)LayoutReader.GetPropertyValue(reader, "ForeColorDark", typeof(Color), _colorScheme._foreColorDark);
			_foreColorLight = (Color)LayoutReader.GetPropertyValue(reader, "ForeColorLight", typeof(Color), _colorScheme._foreColorLight);
			_handlesTitleBar = (bool)LayoutReader.GetPropertyValue(reader, "HandlesTitleBar", typeof(bool), true);
			_minimized = (bool)LayoutReader.GetPropertyValue(reader, "Minimized", typeof(bool), false);
			_showCaptionBar = (bool)LayoutReader.GetPropertyValue(reader, "ShowCaptionBar", typeof(bool), true);
#pragma warning disable CS0618 // Obsolete layout properties are still read for compatibility.
			ShowControlBoxButton = (bool)LayoutReader.GetPropertyValue(reader, "ShowControlBoxButton", typeof(bool), true);
#pragma warning restore CS0618
			_controlBoxMode = (ControlBoxMode)LayoutReader.GetPropertyValue(reader, "ControlBoxMode", typeof(ControlBoxMode), ControlBoxMode.Default);
			_titleBarFont = (Font)LayoutReader.GetPropertyValue(reader, "TitleBarFont", typeof(Font), null);
			_headerRenderer?.ResetCaptionFont();
			_tabStripHeight = (int)LayoutReader.GetPropertyValue(reader, "TabStripHeight", typeof(int), -1);
			_firstTabOffset = (int)LayoutReader.GetPropertyValue(reader, "FirstTabOffset", typeof(int), 48);
			_showQuickCustomizeMenu = (bool)LayoutReader.GetPropertyValue(reader, "ShowQuickCustomizeMenu", typeof(bool), true);
			object propertyValue = LayoutReader.GetPropertyValue(reader, "UseCompatibleTextRendering", typeof(bool), null);
			if (propertyValue != null)
			{
				_useCompatibleTextRendering = (bool)propertyValue;
			}
			else
			{
				_useCompatibleTextRendering = null;
			}
			DeserializeContextualTabs(reader);
			DeserializeTabs(reader);
			reader["ControlBoxMenu"]?.DeserializeObject(_controlBoxMenu);
			reader["BackstageMenu"]?.DeserializeObject(_backstageMenu);
			reader["HelpButton"]?.DeserializeObject(_helpButton);
			DeserializeQuickAccessCommands(reader);
			DeserializeQuickAccessCustomizeList(reader);
			if (_selectedTab != null)
			{
				_selectedTab.SetMarkedSelected(false);
			}
			_selectedTab = null;
			int selectedTabIndex = (int)LayoutReader.GetPropertyValue(reader, "SelectedTabIndex", typeof(int), -1);
			if (selectedTabIndex >= 0 && selectedTabIndex < _tabs.Count && _tabs[selectedTabIndex].Visible)
			{
				SelectedTab = _tabs[selectedTabIndex];
			}
			else
			{
				SelectFirstVisibleTab();
			}
			RelayoutForFormState();
			InvalidateFullLayout();
			ReconcileCaptionExtension(true);
		}

		private void DeserializeTabs(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["Tabs"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_tabs);
				return;
			}
			RibbonTab[] tabs = new RibbonTab[_tabs.Count];
			((System.Collections.ICollection)_tabs).CopyTo(tabs, 0);
			foreach (RibbonTab tab in tabs)
			{
				tab.Dispose();
			}
		}

		private void DeserializeQuickAccessCustomizeList(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["QuickAccessCustomizeList"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_quickAccessCustomizeList);
			}
			else
			{
				_quickAccessCustomizeList.Clear();
			}
		}

		private void DeserializeQuickAccessCommands(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["QuickAccessCommands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_quickAccessCommands);
			}
			else
			{
				_quickAccessCommands.Clear();
			}
		}

		private void DeserializeContextualTabs(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["ContextualTabs"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_contextualTabs);
				return;
			}
			RibbonContextualTab[] contextualTabs = new RibbonContextualTab[_contextualTabs.Count];
			((System.Collections.ICollection)_contextualTabs).CopyTo(contextualTabs, 0);
			foreach (RibbonContextualTab contextualTab in contextualTabs)
			{
				contextualTab.Dispose();
			}
		}

		private void InitializeDesignerImageList()
		{
			_components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MxRibbon));
			_designerImageList = new ImageList(_components);
			SuspendLayout();
			ImageStrip.Load(_designerImageList, componentResourceManager, "imlDesign.ImageStrip");
			_designerImageList.TransparentColor = Color.Transparent;
			_designerImageList.Images.SetKeyName(0, "Separator.ico");
			_designerImageList.Images.SetKeyName(1, "ButtonGroup.ico");
			_designerImageList.Images.SetKeyName(2, "CheckBox.ico");
			_designerImageList.Images.SetKeyName(3, "ComboBox.ico");
			_designerImageList.Images.SetKeyName(4, "Command.ico");
			_designerImageList.Images.SetKeyName(5, "CommandBox.ico");
			_designerImageList.Images.SetKeyName(6, "CommandDropDown.ico");
			_designerImageList.Images.SetKeyName(7, "ContainerControl.ico");
			_designerImageList.Images.SetKeyName(8, "Delete2.ico");
			_designerImageList.Images.SetKeyName(9, "EditBox.ico");
			_designerImageList.Images.SetKeyName(10, "Gallery.ico");
			_designerImageList.Images.SetKeyName(11, "Label.ico");
			_designerImageList.Images.SetKeyName(12, "MoveDown.ico");
			_designerImageList.Images.SetKeyName(13, "MoveUp.ico");
			Dock = DockStyle.Top;
			ResumeLayout(performLayout: false);
		}
	}
}
