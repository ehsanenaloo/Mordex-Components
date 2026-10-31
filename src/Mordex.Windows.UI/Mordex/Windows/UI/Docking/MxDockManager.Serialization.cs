using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxDockManager
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			DeserializeLayout(reader, PersistMode.PanelLayout, true);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			if (_persistMode != PersistMode.TabbedMdiLayout)
			{
				SerializePanelLayout(info);
			}
			if (_persistMode != PersistMode.PanelLayout)
			{
				SerializeMdiLayout(info);
			}
			if (_persistMode != PersistMode.TabbedMdiLayout && _layouts.Count > 0)
			{
				info.AddValue("Layouts", _layouts);
			}
		}

		private void ApplyCurrentLayout()
		{
			DockPanelLayout resolvedLayout = null;
			if (_currentLayout != null)
			{
				resolvedLayout = _currentLayout.GetMergedLayout();
			}
			if (resolvedLayout == null || resolvedLayout.LayoutString == "")
			{
				DeserializeLayout(new LayoutReader(), PersistMode.PanelLayout, true);
			}
			else
			{
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(resolvedLayout);
				mordexLayoutReader["UIPanelManager"]?.DeserializeObject(this);
				if (!_isInitializing)
				{
					OnLayoutLoad(EventArgs.Empty);
				}
			}
			OnCurrentLayoutChanged(EventArgs.Empty);
		}

		public void LoadLayout(DockPanelLayout layout)
		{
			if (layout == null)
			{
				throw new ArgumentNullException();
			}
			if (!layout.HasStandAloneRoot())
			{
				throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
			}
			layout = layout.GetMergedLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
			mordexLayoutReader["UIPanelManager"]?.DeserializeObject(this);
		}

		public void LoadLayoutFile(Stream stream)
		{
			LoadLayoutStream(stream, PersistMode.All);
		}

		private void LoadLayoutStream(Stream stream, PersistMode persistMode)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["PanelManagerLayoutFile"];
			if (propertyValue != null)
			{
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				_layouts.Clear();
				layoutReader["Layouts"]?.DeserializeObject(_layouts);
				DockPanelLayout uiPanelLayout = _layouts.FindCurrentLayout();
				DeserializeLayout(layoutReader, persistMode, uiPanelLayout == null);
				if (uiPanelLayout != null)
				{
					CurrentLayout = uiPanelLayout;
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
			SaveLayoutFile(stream, PersistMode.All);
		}

		public void SaveLayoutFile(Stream stream, PersistMode persistMode)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			_persistMode = persistMode;
			try
			{
				PropertyValue propertyValue = new PropertyValue("PanelManagerLayoutFile", this);
				propertyValue.LayoutType = "Mordex.Windows.UI.Docking.MxDockManager";
				LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
				mordexLayoutWriter.FillStream(stream);
			}
			finally
			{
				_persistMode = PersistMode.PanelLayout;
			}
		}

		public DockPanelLayout GetLayout()
		{
			DockPanelLayout uiPanelLayout = new DockPanelLayout();
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			SerializePanelLayout(mordexLayoutWriter);
			SerializeMdiLayout(mordexLayoutWriter);
			LayoutWriter.StoreInLayout("UIPanelManager", mordexLayoutWriter, uiPanelLayout);
			return uiPanelLayout;
		}

		internal void SerializePanelLayout(LayoutWriter mordexLayoutWriter)
		{
			if (!_allowPanelDrag)
			{
				mordexLayoutWriter.AddValue("AllowPanelDrag", _allowPanelDrag);
			}
			if (!_allowPanelDrop)
			{
				mordexLayoutWriter.AddValue("AllowPanelDrop", _allowPanelDrop);
			}
			if (!_allowPanelResize)
			{
				mordexLayoutWriter.AddValue("AllowPanelResize", _allowPanelResize);
			}
			if (_backColorAutoHideStrip != Color.Empty)
			{
				mordexLayoutWriter.AddValue("BackColorAutoHideStrip", _backColorAutoHideStrip);
			}
			if (_backColorGradientAutoHideStrip != Color.Empty)
			{
				mordexLayoutWriter.AddValue("BackColorGradientAutoHideStrip", _backColorGradientAutoHideStrip);
			}
			if (_backColorGradientSplitter != Color.Empty)
			{
				mordexLayoutWriter.AddValue("BackColorGradientSplitter", _backColorGradientSplitter);
			}
			if (_backColorSplitter != Color.Empty)
			{
				mordexLayoutWriter.AddValue("BackColorSplitter", _backColorSplitter);
			}
			if (_backColorResizeBar != Color.Empty)
			{
				mordexLayoutWriter.AddValue("BackColorResizeBar", _backColorResizeBar);
			}
			if (_backColorGradientResizeBar != Color.Empty)
			{
				mordexLayoutWriter.AddValue("BackColorGradientResizeBar", _backColorGradientResizeBar);
			}
			if (_defaultPanelSettings.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("DefaultPanelSettings", _defaultPanelSettings);
			}
			if (_defaultTabGroupStyle != TabGroupStyle.Tab)
			{
				mordexLayoutWriter.AddValue("DefaultTabGroupStyle", _defaultTabGroupStyle);
			}
			if (_dockingStyleMode != DockingStyleMode.VS2005)
			{
				mordexLayoutWriter.AddValue("DockingStyleMode", _dockingStyleMode);
			}
			if (_panelPadding.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("PanelPadding", _panelPadding);
			}
			if (_splitterSize != DefaultSplitterSize)
			{
				mordexLayoutWriter.AddValue("SplitterSize", _splitterSize);
			}
			if (_showAutoHidePanelsOnClick)
			{
				mordexLayoutWriter.AddValue("ShowAutoHidePanelsOnClick", _showAutoHidePanelsOnClick);
			}
			if (!_formatStyleTabStrip.IsDefaultStyle())
			{
				mordexLayoutWriter.AddValue("FormatStyleTabStrip", _formatStyleTabStrip);
			}
			if (_useCompatibleTextRendering.HasValue)
			{
				mordexLayoutWriter.AddValue("UseCompatibleTextRendering", _useCompatibleTextRendering);
			}
			if (BuiltInTexts.HasCustomTexts())
			{
				mordexLayoutWriter.AddValue("BuiltIntTexts", _builtInTexts);
			}
			int panelIndex = 0;
			ArrayList arrayList = GetAllPanels();
			foreach (DockPanelBase item in arrayList)
			{
				mordexLayoutWriter.AddValue("Panel" + panelIndex, item);
				panelIndex++;
			}
			if (arrayList.Count > 0)
			{
				mordexLayoutWriter.AddValue("DockInfo", _dockInfo);
			}
		}

		internal void DeserializeLayout(LayoutReader reader, PersistMode persistMode, bool loadPanels)
		{
			if (persistMode != PersistMode.TabbedMdiLayout)
			{
				persistMode = (PersistMode)LayoutReader.GetPropertyValue(reader, "PersistMode", typeof(PersistMode), PersistMode.All);
			}
			if (persistMode != PersistMode.TabbedMdiLayout)
			{
				SuspendPanelLayout();
				try
				{
					object propertyValue = LayoutReader.GetPropertyValue(reader, "UseCompatibleTextRendering", typeof(bool), null);
					if (propertyValue != null)
					{
						_useCompatibleTextRendering = (bool)propertyValue;
					}
					reader["BuiltIntTexts"]?.DeserializeObject(BuiltInTexts);
					_allowPanelDrag = (bool)LayoutReader.GetPropertyValue(reader, "AllowPanelDrag", typeof(bool), true);
					_allowPanelDrop = (bool)LayoutReader.GetPropertyValue(reader, "AllowPanelDrop", typeof(bool), true);
					_allowPanelResize = (bool)LayoutReader.GetPropertyValue(reader, "AllowPanelResize", typeof(bool), true);
					_backColorAutoHideStrip = (Color)LayoutReader.GetPropertyValue(reader, "BackColorAutoHideStrip", typeof(Color), Color.Empty);
					_backColorGradientAutoHideStrip = (Color)LayoutReader.GetPropertyValue(reader, "BackColorGradientAutoHideStrip", typeof(Color), Color.Empty);
					_backColorSplitter = (Color)LayoutReader.GetPropertyValue(reader, "BackColorSplitter", typeof(Color), Color.Empty);
					_backColorGradientSplitter = (Color)LayoutReader.GetPropertyValue(reader, "BackColorGradientSplitter", typeof(Color), Color.Empty);
					_backColorResizeBar = (Color)LayoutReader.GetPropertyValue(reader, "BackColorResizeBar", typeof(Color), Color.Empty);
					BackColorGradientResizeBar = (Color)LayoutReader.GetPropertyValue(reader, "BackColorGradientResizeBar", typeof(Color), Color.Empty);
					_dockingStyleMode = (DockingStyleMode)LayoutReader.GetPropertyValue(reader, "DockingStyleMode", typeof(DockingStyleMode), DockingStyleMode.VS2005);
					PropertyValue settingValue = reader["DefaultPanelSettings"];
					if (settingValue != null)
					{
						settingValue.DeserializeObject(_defaultPanelSettings);
					}
					else
					{
						_defaultPanelSettings.Reset();
					}
					_defaultTabGroupStyle = (TabGroupStyle)LayoutReader.GetPropertyValue(reader, "DefaultTabGroupStyle", typeof(TabGroupStyle), TabGroupStyle.Tab);
					settingValue = reader["PanelPadding"];
					if (settingValue != null)
					{
						settingValue.DeserializeObject(_panelPadding);
					}
					else
					{
						_panelPadding.Reset();
					}
					_splitterSize = (int)LayoutReader.GetPropertyValue(reader, "SplitterSize", typeof(int), DefaultSplitterSize);
					_showAutoHidePanelsOnClick = (bool)LayoutReader.GetPropertyValue(reader, "ShowAutoHidePanelsOnClick", typeof(bool), false);
					settingValue = reader["FormatStyleTabStrip"];
					if (settingValue != null)
					{
						settingValue.DeserializeObject(_formatStyleTabStrip);
					}
					else
					{
						_formatStyleTabStrip.Reset();
					}
					if (loadPanels)
					{
						DeserializePanels(reader);
					}
				}
				finally
				{
					ResumePanelLayout();
				}
				PlaceAllInAutoHideStrips();
			}
			if (persistMode != PersistMode.PanelLayout && loadPanels)
			{
				DeserializeMdiLayout(reader);
			}
		}

		private void ClearAutoHideStrip(MxAutoHideStrip strip)
		{
			DockPanelBase[] panels = strip.Panels;
			for (int index = panels.Length - 1; index >= 0; index--)
			{
				strip.RemovePanel(panels[index]);
			}
		}

		private void DeserializePanels(LayoutReader mordexLayoutReader)
		{
			ClearAutoHideStrip(_bottomStrip);
			ClearAutoHideStrip(_leftStrip);
			ClearAutoHideStrip(_topStrip);
			ClearAutoHideStrip(_rightStrip);
			if (_containerControl != null && _containerControl.ContainsFocus)
			{
				RenderingHelper.TryFocusControl(_containerControl);
			}
			ArrayList arrayList = GetAllPanels();
			foreach (DockPanelBase item in arrayList)
			{
				item.SetLayoutLoadState("Remove");
				item._dockManager = this;
			}
			PropertyValue propertyValue = mordexLayoutReader["DockInfo"];
			_dockInfo = new DockLayoutSnapshot(this);
			propertyValue?.DeserializeObject(_dockInfo);
			ArrayList loadedPanels = new ArrayList();
			int panelIndex = 0;
			for (PropertyValue panelValue = mordexLayoutReader["Panel" + panelIndex]; panelValue != null; panelValue = mordexLayoutReader["Panel" + panelIndex])
			{
				LayoutReader layoutReader = panelValue.GetLayoutReader();
				Guid guid = (Guid)layoutReader["Id"].GetValue(typeof(Guid));
				DockPanelBase loadedPanel = FindPanelInList(guid, arrayList);
				if (loadedPanel != null)
				{
					loadedPanel.SetLayoutLoadState("Existing");
				}
				else
				{
					propertyValue = layoutReader["GroupStyle"];
					loadedPanel = ((propertyValue != null) ? ((DockPanelBase)new MxDockPanelGroup(guid)) : ((DockPanelBase)new MxDockPanel(guid)));
					loadedPanel.SetLayoutLoadState("Created");
				}
				panelValue.DeserializeObject(loadedPanel);
				loadedPanels.Add(loadedPanel);
				DockLayoutState dockLayoutState = _dockInfo.GetDockedLayout().FindById(loadedPanel.Id);
				if (dockLayoutState != null)
				{
					if (loadedPanel is MxDockPanelGroup)
					{
						dockLayoutState.SetIsStatic(((MxDockPanelGroup)loadedPanel).StaticGroup);
					}
					loadedPanel._dockedLayoutState = dockLayoutState;
				}
				dockLayoutState = _dockInfo.GetFloatingLayout().FindById(loadedPanel.Id);
				if (dockLayoutState != null)
				{
					if (loadedPanel is MxDockPanelGroup)
					{
						dockLayoutState.SetIsStatic(((MxDockPanelGroup)loadedPanel).StaticGroup);
					}
					loadedPanel._floatingLayoutState = dockLayoutState;
				}
				loadedPanel.Visible = !loadedPanel.Closed;
				panelIndex++;
			}
			ArrayList removedPanels = new ArrayList();
			foreach (DockPanelBase existingPanel in arrayList)
			{
				if (!((string)existingPanel.GetLayoutLoadState() == "Remove"))
				{
					continue;
				}
				existingPanel._dockManager = null;
				existingPanel.SetParentContainer(null);
				existingPanel.AutoHide = false;
				removedPanels.Add(existingPanel);
				try
				{
					if (base.Container != null)
					{
						base.Container.Remove(existingPanel);
						if (existingPanel is MxDockPanel)
						{
							base.Container.Remove(((MxDockPanel)existingPanel).InnerContainer);
						}
						if (existingPanel.ContainerCaptionControl != null)
						{
							base.Container.Remove(existingPanel.ContainerCaptionControl);
						}
					}
				}
				catch
				{
					// A designer host may refuse to remove the component, with its own exception type.
					// The panel is disposed below anyway.
				}
			}
			arrayList = loadedPanels;
			foreach (DockPanelBase createdPanel in arrayList)
			{
				if (!((string)createdPanel.GetLayoutLoadState() == "Created"))
				{
					continue;
				}
				string innerContainerName = "";
				string captionContainerName = "";
				if (base.Container == null)
				{
					continue;
				}
				if (createdPanel.Name.Length > 0)
				{
					try
					{
						base.Container.Add(createdPanel, createdPanel.Name);
						innerContainerName = createdPanel.Name + "Container";
						captionContainerName = createdPanel.Name + "CaptionContainer";
					}
					catch
					{
						// The name is taken or not valid in this container. Each name service throws its own
						// exception type, so any failure here falls back to a name made by the container.
						base.Container.Add(createdPanel);
						if (createdPanel.Site != null && (createdPanel.Name == null || createdPanel.Name.Length == 0))
						{
							createdPanel.Name = createdPanel.Site.Name;
						}
					}
				}
				else
				{
					base.Container.Add(createdPanel);
				}
				if (createdPanel is MxDockPanel)
				{
					try
					{
						base.Container.Add(((MxDockPanel)createdPanel).InnerContainer, innerContainerName);
					}
					catch
					{
						// Name clash: fall back to a name made by the container.
						base.Container.Add(((MxDockPanel)createdPanel).InnerContainer);
						if (((MxDockPanel)createdPanel).InnerContainer.Site != null)
						{
							((MxDockPanel)createdPanel).InnerContainer.Name = ((MxDockPanel)createdPanel).InnerContainer.Site.Name;
						}
					}
				}
				if (createdPanel.ContainerCaptionControl == null)
				{
					continue;
				}
				try
				{
					base.Container.Add(createdPanel.ContainerCaptionControl, captionContainerName);
				}
				catch
				{
					// Name clash: fall back to a name made by the container.
					base.Container.Add(createdPanel.ContainerCaptionControl);
					if (createdPanel.ContainerCaptionControl.Site != null)
					{
						createdPanel.ContainerCaptionControl.Name = createdPanel.ContainerCaptionControl.Site.Name;
					}
				}
			}
			_panelSnapshot = arrayList;
			AddPanelsFromLayout(_dockInfo.GetDockedLayout(), PanelDockState.Docked);
			AddPanelsFromLayout(_dockInfo.GetFloatingLayout(), PanelDockState.Floating);
			foreach (DockPanelBase restoredPanel in _panelSnapshot)
			{
				restoredPanel.UpdateVisibility();
			}
			_panelSnapshot = null;
			foreach (DockPanelBase managedPanel in GetAllPanels())
			{
				managedPanel._dockManager = null;
			}
			foreach (DockPanelBase removedPanel in removedPanels)
			{
				removedPanel.Dispose();
			}
			RemoveRedundantGroups();
			PlaceAllInAutoHideStrips();
		}

		private DockPanelBase FindPanelInList(Guid guid, ArrayList arrayList)
		{
			foreach (DockPanelBase item in arrayList)
			{
				if (item.Id == guid)
				{
					return item;
				}
			}
			return null;
		}

		internal ApplicationSettings GetApplicationSettings()
		{
			if (_applicationSettings == null)
			{
				_applicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			else if (string.Compare(_applicationSettings.SettingsKey, SettingsKey, ignoreCase: true) != 0)
			{
				_applicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			return _applicationSettings;
		}

		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				if (_isInitializing)
				{
					CompleteInitialization();
					using (MemoryStream defaultLayoutStream = new MemoryStream())
					{
						SaveLayoutFile(defaultLayoutStream);
						GetApplicationSettings().DefaultLayoutSettings = defaultLayoutStream.ToArray();
					}
				}
				byte[] layoutSettings = null;
				try
				{
					layoutSettings = GetApplicationSettings().LayoutSettings;
				}
				catch
				{
					// A broken user settings file can fail in many ways. It must not stop the form from loading.
					layoutSettings = null;
				}
				if (layoutSettings != null && layoutSettings.Length > 0)
				{
					using (MemoryStream savedLayoutStream = new MemoryStream(layoutSettings))
					{
						LoadLayoutFile(savedLayoutStream);
					}
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
			using (MemoryStream layoutStream = new MemoryStream())
			{
				SaveLayoutFile(layoutStream);
				GetApplicationSettings().LayoutSettings = layoutStream.ToArray();
				GetApplicationSettings().Save();
			}
		}

		private void TrySaveComponentSettings()
		{
			try
			{
				SaveComponentSettings();
			}
			catch (Exception ex)
			{
				// Called from Dispose, which must not throw. A failed save keeps the settings from the last run.
				Debug.WriteLine(ex);
			}
		}

		internal void RestoreDockLayout()
		{
			if (!_isInitializing && !_isPanelLayoutSuspended)
			{
				if (_designer != null)
				{
					DesignHelper.PanelManagerDesigner_SelectContainer(_designer);
				}
				SuspendPanelLayout();
				AddPanelsFromLayout(_dockInfo.GetFloatingLayout(), PanelDockState.Floating);
				AddPanelsFromLayout(_dockInfo.GetDockedLayout(), PanelDockState.Docked);
				RemoveRedundantGroups();
				PlaceAllInAutoHideStrips();
				UpdateControlOrder(false);
				ResumePanelLayout();
			}
		}

		private void AddPanelsFromLayout(DockLayoutState dockLayoutState, PanelDockState panelDockState)
		{
			int insertIndex = 0;
			foreach (DockLayoutState item in (IEnumerable)dockLayoutState.GetChildren())
			{
				DockPanelBase uiPanelBase = BuildPanelFromLayout(item, panelDockState);
				if (uiPanelBase != null)
				{
					uiPanelBase._shouldSkipLayoutSync = true;
					uiPanelBase.AttachToGroupOrDetach(null, panelDockState, item.GetDockStyle());
					uiPanelBase.Size = uiPanelBase.GetCurrentLayoutState().GetTopLevelState().GetPanelSize();
					uiPanelBase.Visible = !uiPanelBase.Closed;
					_panels.Insert(insertIndex, uiPanelBase);
					uiPanelBase._dockManager = null;
					uiPanelBase._shouldSkipLayoutSync = false;
					insertIndex++;
				}
			}
		}

		public void RemoveRedundantGroups()
		{
			Control control = null;
			if (_containerControl != null && _containerControl.TopLevelControl is Form)
			{
				control = ((Form)_containerControl.TopLevelControl).ActiveControl;
			}
			if (_containerControl != null && _containerControl.ContainsFocus)
			{
				RenderingHelper.TryFocusControl(_containerControl);
			}
			_dockInfo.GetDockedLayout().CollapseChildren();
			_dockInfo.GetFloatingLayout().CollapseChildren();
			GetAllPanels();
			for (int index = _panels.Count - 1; index >= 0; index--)
			{
				DockPanelBase uiPanelBase = _panels[index];
				if (uiPanelBase is MxDockPanelGroup)
				{
					RemoveRedundantGroup((MxDockPanelGroup)uiPanelBase);
				}
			}
			if (control != null && control.TopLevelControl == _containerControl.TopLevelControl && control.IsHandleCreated)
			{
				control.Focus();
			}
			else
			{
				FocusDefaultControl();
			}
		}

		private void RemoveRedundantGroup(MxDockPanelGroup panel)
		{
			for (int index = panel.Panels.Count - 1; index >= 0; index--)
			{
				DockPanelBase uiPanelBase = panel.Panels[index];
				if (uiPanelBase is MxDockPanelGroup)
				{
					RemoveRedundantGroup((MxDockPanelGroup)uiPanelBase);
				}
			}
			if (panel.StaticGroup)
			{
				return;
			}
			if (panel.Panels.Count == 0)
			{
				panel._shouldSkipLayoutSync = true;
				panel.GetParentContainer().Panels.Remove(panel);
				panel._shouldSkipLayoutSync = false;
				if (panel.Container != null)
				{
					panel.Container.Remove(panel);
				}
			}
			else if (panel.Panels.Count == 1)
			{
				DockPanelBase onlyChild = panel.Panels[0];
				onlyChild._shouldSkipLayoutSync = true;
				panel._shouldSkipLayoutSync = true;
				onlyChild.SetParentContainer(this);
				panel.GetParentContainer().Panels.ReplacePanel(panel, onlyChild);
				onlyChild.Size = panel.Size;
				if (panel.Container != null)
				{
					panel.Container.Remove(panel);
				}
				panel._shouldSkipLayoutSync = false;
				onlyChild._shouldSkipLayoutSync = false;
			}
		}

		internal void FocusDefaultControl()
		{
			if (_containerControl == null)
			{
				return;
			}
			Control target = null;
			foreach (Control child in _containerControl.Controls)
			{
				if (child.Dock == DockStyle.Fill)
				{
					target = child;
					break;
				}
				if (!(child is DockPanelBase) && !(child is MxAutoHideStrip) && !(child is DockPanelBasePanel) && child.CanSelect && child.TabStop && (target == null || child.TabIndex < target.TabIndex))
				{
					target = child;
				}
			}
			if (target == null)
			{
				RenderingHelper.TryFocusControl(_leftStrip);
			}
			else
			{
				RenderingHelper.TryFocusControl(target);
			}
		}

		private DockPanelBase BuildPanelFromLayout(DockLayoutState dockLayoutState, PanelDockState panelDockState)
		{
			DockPanelBase uiPanelBase = FindPanelInState(dockLayoutState.GetId(), panelDockState);
			if (dockLayoutState.GetIsGroup())
			{
				ArrayList arrayList = new ArrayList();
				foreach (DockLayoutState item in (IEnumerable)dockLayoutState.GetChildren())
				{
					DockPanelBase childPanel = BuildPanelFromLayout(item, panelDockState);
					if (childPanel != null)
					{
						arrayList.Add(childPanel);
					}
				}
				if (arrayList.Count > 1 || (dockLayoutState.GetIsStatic() && dockLayoutState.GetIsCurrent()))
				{
					MxDockPanelGroup uiPanelGroup;
					if (uiPanelBase == null)
					{
						uiPanelGroup = new MxDockPanelGroup(dockLayoutState.GetId());
						uiPanelGroup._dockManager = this;
						if (panelDockState == PanelDockState.Docked)
						{
							uiPanelGroup._dockedLayoutState = dockLayoutState;
							DockLayoutState floatingState = _dockInfo.GetFloatingLayout().GetChildren().FindLayoutState(dockLayoutState.GetId());
							if (floatingState != null)
							{
								uiPanelGroup._floatingLayoutState = floatingState;
							}
						}
						else
						{
							uiPanelGroup._floatingLayoutState = dockLayoutState;
							DockLayoutState dockedState = _dockInfo.GetDockedLayout().GetChildren().FindLayoutState(dockLayoutState.GetId());
							if (dockedState != null)
							{
								uiPanelGroup._dockedLayoutState = dockedState;
							}
						}
						uiPanelGroup.SetDockStateRecursive(panelDockState);
					}
					else
					{
						uiPanelGroup = (MxDockPanelGroup)uiPanelBase;
					}
					uiPanelGroup.SuspendGroupLayout();
					uiPanelGroup.GroupStyle = dockLayoutState.GetGroupStyle();
					for (int i = 0; i < arrayList.Count; i++)
					{
						DockPanelBase child = (DockPanelBase)arrayList[i];
						child._shouldSkipLayoutSync = true;
						uiPanelGroup.Panels.Insert(i, child);
						child._shouldSkipLayoutSync = false;
					}
					uiPanelGroup.ResumeGroupLayout();
					return uiPanelGroup;
				}
				if (dockLayoutState.GetIsStatic())
				{
					return uiPanelBase;
				}
				if (arrayList.Count == 1)
				{
					return (DockPanelBase)arrayList[0];
				}
				return null;
			}
			return uiPanelBase;
		}

		private DockPanelBase FindPanelInState(Guid guid, PanelDockState panelDockState)
		{
			ArrayList arrayList = GetAllPanels();
			foreach (DockPanelBase item in arrayList)
			{
				if (item.Id == guid && item.GetDockState() == panelDockState)
				{
					return item;
				}
			}
			return null;
		}

		public static string GetUIVersionString()
		{
			string localPath = typeof(MxDockManager).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		private void SerializeMdiLayout(LayoutWriter mordexLayoutWriter)
		{
			if (_tabbedMdi)
			{
				mordexLayoutWriter.AddValue("TabbedMdi", _tabbedMdi);
			}
			if (_tabbedMdiSettings.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("TabbedMdiSettings", _tabbedMdiSettings);
			}
			if (_mdiTabGroups != null && _mdiTabGroups.GetGroups().Count > 0)
			{
				mordexLayoutWriter.AddValue("MdiTabGroups", _mdiTabGroups);
			}
			mordexLayoutWriter.AddValue("PersistMode", _persistMode);
		}

		private void DeserializeMdiLayout(LayoutReader reader)
		{
			TabbedMdi = (bool)LayoutReader.GetPropertyValue(reader, "TabbedMdi", typeof(bool), false);
			PropertyValue propertyValue = reader["TabbedMdiSettings"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_tabbedMdiSettings);
			}
			else
			{
				_tabbedMdiSettings.Reset();
			}
			if (_mdiTabGroups != null)
			{
				reader["MdiTabGroups"]?.DeserializeObject(_mdiTabGroups);
			}
		}

		internal void MoveLayoutState(Guid id, PanelDockState fromState, PanelDockState toState)
		{
			DockLayoutState dockLayoutState = _dockInfo.GetLayoutState(fromState).FindById(id);
			DockLayoutState targetRoot = _dockInfo.GetLayoutState(toState);
			if (dockLayoutState != null)
			{
				CopyLayoutState(dockLayoutState, targetRoot, toState, false);
			}
		}

		internal DockLayoutState CopyLayoutState(DockLayoutState dockLayoutState, DockLayoutState targetRoot, PanelDockState panelDockState, bool moveCurrent)
		{
			DockLayoutState copy = targetRoot.FindById(dockLayoutState.GetId());
			if (copy == null)
			{
				copy = new DockLayoutState(dockLayoutState.GetId());
			}
			if (moveCurrent)
			{
				dockLayoutState.CopyLayoutTo(copy);
				dockLayoutState.UpdateIsCurrent(false);
				copy.UpdateIsCurrent(true);
			}
			foreach (DockLayoutState item in (IEnumerable)dockLayoutState.GetChildren())
			{
				DockLayoutState childCopy = CopyLayoutState(item, targetRoot, panelDockState, true);
				childCopy.Reparent(copy);
			}
			return copy;
		}

		public void BeginPanelInfo()
		{
			_isAddingPanelInfo = true;
			_pendingDockInfo = new DockLayoutSnapshot(this);
		}

		public void AddDockPanelInfo(Guid id, PanelDockStyle dockStyle, Size size, bool isCurrent)
		{
			if (!_isAddingPanelInfo)
			{
				throw new InvalidOperationException();
			}
			DockLayoutState dockLayoutState = new DockLayoutState(id);
			dockLayoutState.SetDockStyle(dockStyle);
			dockLayoutState.SetPanelSize(size);
			dockLayoutState.UpdateIsCurrent(isCurrent);
			dockLayoutState.Reparent(_pendingDockInfo.GetDockedLayout());
		}

		public void AddDockPanelInfo(Guid id, Guid parentId, int tileSize, bool isCurrent)
		{
			if (!_isAddingPanelInfo)
			{
				throw new InvalidOperationException();
			}
			DockLayoutState dockLayoutState = _pendingDockInfo.GetDockedLayout().FindById(parentId);
			if (dockLayoutState == null)
			{
				dockLayoutState = _pendingDockInfo.GetDockedLayout();
			}
			DockLayoutState childState = new DockLayoutState(id);
			childState.UpdateIsCurrent(isCurrent);
			childState.SetTileSize(tileSize);
			childState.Reparent(dockLayoutState);
		}

		public void AddDockPanelInfo(Guid id, PanelGroupStyle groupStyle, PanelDockStyle dockStyle, bool isStaticGroup, Size size, bool isCurrent)
		{
			if (!_isAddingPanelInfo)
			{
				throw new InvalidOperationException();
			}
			DockLayoutState dockLayoutState = new DockLayoutState(id);
			dockLayoutState.SetDockStyle(dockStyle);
			dockLayoutState.SetPanelSize(size);
			dockLayoutState.SetIsGroup(true);
			dockLayoutState.SetIsStatic(isStaticGroup);
			dockLayoutState.SetGroupStyle(groupStyle);
			dockLayoutState.UpdateIsCurrent(isCurrent);
			dockLayoutState.Reparent(_pendingDockInfo.GetDockedLayout());
		}

		public void AddDockPanelInfo(Guid id, Guid parentId, PanelGroupStyle groupStyle, bool isStaticGroup, int tileSize, bool isCurrent)
		{
			if (!_isAddingPanelInfo)
			{
				throw new InvalidOperationException();
			}
			DockLayoutState dockLayoutState = _pendingDockInfo.GetDockedLayout().FindById(parentId);
			if (dockLayoutState == null)
			{
				dockLayoutState = _pendingDockInfo.GetDockedLayout();
			}
			DockLayoutState childState = new DockLayoutState(id);
			childState.SetIsGroup(true);
			childState.UpdateIsCurrent(isCurrent);
			childState.SetGroupStyle(groupStyle);
			childState.SetTileSize(tileSize);
			childState.SetIsStatic(isStaticGroup);
			childState.Reparent(dockLayoutState);
		}

		public void AddFloatingPanelInfo(Guid id, Point location, Size size, bool isCurrent)
		{
			if (!_isAddingPanelInfo)
			{
				throw new InvalidOperationException();
			}
			DockLayoutState dockLayoutState = new DockLayoutState(id);
			dockLayoutState.SetPanelSize(size);
			dockLayoutState.SetLocation(location);
			dockLayoutState.UpdateIsCurrent(isCurrent);
			dockLayoutState.Reparent(_pendingDockInfo.GetFloatingLayout());
		}

		public void AddFloatingPanelInfo(Guid id, Guid parentId, int tileSize, bool isCurrent)
		{
			if (!_isAddingPanelInfo)
			{
				throw new InvalidOperationException();
			}
			DockLayoutState dockLayoutState = _pendingDockInfo.GetFloatingLayout().FindById(parentId);
			if (dockLayoutState == null)
			{
				dockLayoutState = _pendingDockInfo.GetFloatingLayout();
			}
			DockLayoutState childState = new DockLayoutState(id);
			childState.SetTileSize(tileSize);
			childState.UpdateIsCurrent(isCurrent);
			childState.Reparent(dockLayoutState);
		}

		public void AddFloatingPanelInfo(Guid id, PanelGroupStyle groupStyle, bool isStaticGroup, Point location, Size size, bool isCurrent)
		{
			if (!_isAddingPanelInfo)
			{
				throw new InvalidOperationException();
			}
			DockLayoutState dockLayoutState = new DockLayoutState(id);
			dockLayoutState.SetPanelSize(size);
			dockLayoutState.SetLocation(location);
			dockLayoutState.SetIsGroup(true);
			dockLayoutState.SetIsStatic(isStaticGroup);
			dockLayoutState.SetGroupStyle(groupStyle);
			dockLayoutState.UpdateIsCurrent(isCurrent);
			dockLayoutState.Reparent(_pendingDockInfo.GetFloatingLayout());
		}

		public void AddFloatingPanelInfo(Guid id, Guid parentId, PanelGroupStyle groupStyle, bool isStaticGroup, int tileSize, bool isCurrent)
		{
			if (!_isAddingPanelInfo)
			{
				throw new InvalidOperationException();
			}
			DockLayoutState dockLayoutState = _pendingDockInfo.GetFloatingLayout().FindById(parentId);
			if (dockLayoutState == null)
			{
				dockLayoutState = _pendingDockInfo.GetFloatingLayout();
			}
			DockLayoutState childState = new DockLayoutState(id);
			childState.SetIsGroup(true);
			childState.SetIsStatic(isStaticGroup);
			childState.SetGroupStyle(groupStyle);
			childState.SetTileSize(tileSize);
			childState.UpdateIsCurrent(isCurrent);
			childState.Reparent(dockLayoutState);
		}

		public void EndPanelInfo()
		{
			if (_isInitializing)
			{
				return;
			}
			_isAddingPanelInfo = false;
			_panelSnapshot = GetAllPanels();
			_dockInfo = _pendingDockInfo;
			foreach (DockPanelBase item in _panelSnapshot)
			{
				DockLayoutState dockLayoutState = _dockInfo.GetDockedLayout().FindById(item.Id);
				if (dockLayoutState != null)
				{
					item._dockedLayoutState = dockLayoutState;
				}
				else
				{
					item._dockedLayoutState.Reparent(_dockInfo.GetDockedLayout());
					item._dockedLayoutState.UpdateIsCurrent(false);
				}
				dockLayoutState = _dockInfo.GetFloatingLayout().FindById(item.Id);
				if (dockLayoutState != null)
				{
					item._floatingLayoutState = dockLayoutState;
				}
				else
				{
					item._floatingLayoutState.Reparent(_dockInfo.GetFloatingLayout());
				}
				if (!item._dockedLayoutState.GetIsCurrent() && !item._floatingLayoutState.GetIsCurrent())
				{
					item._dockedLayoutState.UpdateIsCurrent(true);
				}
				if (item._dockedLayoutState.GetIsCurrent())
				{
					item.SetDockStateRecursive(PanelDockState.Docked);
					if (item is MxDockPanelGroup)
					{
						((MxDockPanelGroup)item).StaticGroup = item._dockedLayoutState.GetIsStatic();
					}
				}
				else
				{
					item.SetDockStateRecursive(PanelDockState.Floating);
					if (item is MxDockPanelGroup)
					{
						((MxDockPanelGroup)item).StaticGroup = item._floatingLayoutState.GetIsStatic();
					}
				}
			}
			if (_designer != null)
			{
				DesignHelper.PanelManagerDesigner_SelectContainer(_designer);
			}
			SuspendPanelLayout();
			_panelSnapshot = GetAllPanels();
			AddPanelsFromLayout(_dockInfo.GetFloatingLayout(), PanelDockState.Floating);
			AddPanelsFromLayout(_dockInfo.GetDockedLayout(), PanelDockState.Docked);
			RemoveRedundantGroups();
			_panelSnapshot = null;
			PlaceAllInAutoHideStrips();
			UpdateControlOrder(false);
			ResumePanelLayout();
			_pendingDockInfo = null;
		}

		internal DockLayoutSnapshot GetLayoutSnapshot()
		{
			return _dockInfo;
		}

		internal void BeginSizeTransaction()
		{
			if (_designer != null)
			{
				_designerTransaction = DesignHelper.PanelManagerDesigner_StartTransaction(_designer, "Size Panel");
				DesignHelper.PanelManagerDesigner_NotifyChange(_designer, changeComplete: false);
			}
		}

		internal void EndSizeTransaction()
		{
			if (_designer != null && _designerTransaction != null)
			{
				DesignHelper.PanelManagerDesigner_NotifyChange(_designer, changeComplete: true);
				_designerTransaction.Commit();
				_designerTransaction = null;
			}
		}
	}
}
