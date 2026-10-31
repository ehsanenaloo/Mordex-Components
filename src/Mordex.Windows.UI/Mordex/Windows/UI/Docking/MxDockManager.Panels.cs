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
		internal bool IsInitializing()
		{
			return _isInitializing;
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if ((_designer == null || DesignHelper.PanelManagerDesigner_IsHostLoaded(_designer)) && _isInitializing)
			{
				CompleteInitialization();
			}
		}

		DockLayoutState IPanelContainer.GetDockLayoutState(PanelDockState dockState)
		{
			return _dockInfo.GetLayoutState(dockState);
		}

		bool IPanelContainer.OnPanelAdd(DockPanelBase panel, int index)
		{
			return true;
		}

		void IPanelContainer.OnPanelAdded(DockPanelBase panel)
		{
			PlaceInAutoHideStrip(panel, panel.AutoHide);
			StoreDockedPanelOrder();
			UpdateControlOrder(true);
		}

		bool IPanelContainer.OnPanelMove(DockPanelBase panel, int index)
		{
			return true;
		}

		void IPanelContainer.OnPanelMoved(DockPanelBase panel)
		{
			StoreDockedPanelOrder();
			UpdateControlOrder(true);
		}

		bool IPanelContainer.OnPanelRemove(DockPanelBase panel)
		{
			return true;
		}

		void IPanelContainer.OnPanelRemoved(DockPanelBase panel)
		{
			_bottomStrip.RemoveForeignPanels();
			_leftStrip.RemoveForeignPanels();
			_rightStrip.RemoveForeignPanels();
			_topStrip.RemoveForeignPanels();
			UpdateControlOrder(true);
		}

		void IPanelContainer.InvalidateLayout()
		{
			UpdateControlOrder(true);
		}

		void IFormatStyleOwner.PropertyChange()
		{
			InvalidatePanels(_panels);
		}

		internal bool RaisePanelAutoHideChanging(DockPanelBase panel)
		{
			PanelActionCancelEventArgs e = new PanelActionCancelEventArgs(panel, cancel: false);
			OnPanelAutoHideChanging(e);
			return !e.Cancel;
		}

		internal bool RaisePanelClosedChanging(DockPanelBase panel)
		{
			PanelActionCancelEventArgs e = new PanelActionCancelEventArgs(panel, cancel: false);
			OnPanelClosedChanging(e);
			if (e.Cancel)
			{
				return false;
			}
			return true;
		}

		internal void RaisePanelClosedChanged(DockPanelBase panel)
		{
			OnPanelClosedChanged(new PanelActionEventArgs(panel));
		}

		internal void RaisePanelAutoHideChanged(DockPanelBase panel)
		{
			OnPanelAutoHideChanged(new PanelActionEventArgs(panel));
		}

		internal void RaisePanelAutoHideActiveChanged(DockPanelBase panel)
		{
			OnPanelAutoHideActiveChanged(new PanelActionEventArgs(panel));
		}

		internal void RaisePanelAutoHideActiveChanging(PanelActionCancelEventArgs eventArgs)
		{
			OnPanelAutoHideActiveChanging(eventArgs);
		}

		internal void RaisePanelDockStyleChanged(DockPanelBase panel)
		{
			OnPanelDockStyleChanged(new PanelActionEventArgs(panel));
		}

		internal void RaisePanelDockStateChanged(DockPanelBase panel)
		{
			OnPanelDockStateChanged(new PanelActionEventArgs(panel));
		}

		private void PrunePendingDockInfo()
		{
			ArrayList arrayList = GetAllPanels();
			PruneMissingPanels(arrayList, _pendingDockInfo.GetDockedLayout().GetChildren());
			PruneMissingPanels(arrayList, _pendingDockInfo.GetFloatingLayout().GetChildren());
			_pendingDockInfo.GetDockedLayout().CollapseChildren();
			_pendingDockInfo.GetFloatingLayout().CollapseChildren();
		}

		private void PruneMissingPanels(ArrayList panels, DockLayoutStateCollection layoutStates)
		{
			for (int index = layoutStates.GetCount() - 1; index >= 0; index--)
			{
				DockLayoutState dockLayoutState = layoutStates.GetAt(index);
				bool isKept = false;
				if (dockLayoutState.GetIsCurrent())
				{
					foreach (DockPanelBase item in panels)
					{
						if (item.Id == dockLayoutState.GetId())
						{
							isKept = true;
							break;
						}
					}
				}
				else
				{
					isKept = true;
				}
				if (!isKept)
				{
					layoutStates.RemoveLayoutState(dockLayoutState);
				}
				else
				{
					PruneMissingPanels(panels, dockLayoutState.GetChildren());
				}
			}
		}

		internal void EndInitPanels(DockPanelCollection panels)
		{
			foreach (DockPanelBase item in (IEnumerable)panels)
			{
				if (item is MxDockPanelGroup)
				{
					MxDockPanelGroup uiPanelGroup = (MxDockPanelGroup)item;
					EndInitPanels(uiPanelGroup.Panels);
				}
				((ISupportInitialize)item).EndInit();
			}
		}

		internal void CompleteInitialization()
		{
			if (!_isInitializing)
			{
				return;
			}
			_isInitializing = false;
			if (_pendingDockInfo != null)
			{
				PrunePendingDockInfo();
				EndPanelInfo();
			}
			EndInitPanels(_panels);
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				item.RecalculateLayout();
			}
			PlaceAllInAutoHideStrips();
			RestoreDockLayout();
			if (_tabbedMdi)
			{
				RecreateMdiTabGroups();
			}
			if (_layoutStream != null)
			{
				LoadLayoutFile(_layoutStream);
				_layoutStream = null;
			}
		}

		internal ArrayList GetAllPanels()
		{
			if (_panelSnapshot != null)
			{
				return _panelSnapshot;
			}
			ArrayList arrayList = new ArrayList();
			CollectPanels(arrayList, _panels);
			return arrayList;
		}

		private void CollectPanels(ArrayList result, DockPanelCollection panels)
		{
			foreach (DockPanelBase item in (IEnumerable)panels)
			{
				result.Add(item);
				if (item is MxDockPanelGroup)
				{
					CollectPanels(result, ((MxDockPanelGroup)item).Panels);
				}
			}
		}

		private void PlaceAllInAutoHideStrips()
		{
			ArrayList arrayList = GetAllPanels();
			foreach (DockPanelBase item in arrayList)
			{
				if (IsSinglePanel(item))
				{
					PlaceInAutoHideStrip(item, item.AutoHide);
				}
			}
			LayoutAutoHideStrips();
		}

		internal static bool IsSinglePanel(DockPanelBase uiPanelBase)
		{
			if (uiPanelBase is MxDockPanel)
			{
				return true;
			}
			return ((MxDockPanelGroup)uiPanelBase).StaticGroup;
		}

		public DockPanelBase FindPanel(string name)
		{
			ArrayList arrayList = GetAllPanels();
			foreach (DockPanelBase item in arrayList)
			{
				if (string.Compare(item.Name, name, ignoreCase: true) == 0)
				{
					return item;
				}
			}
			return null;
		}

		public MdiChildTab FindMdiTab(Form childForm)
		{
			MxMdiTabGroup[] mdiTabGroups = MdiTabGroups;
			foreach (MxMdiTabGroup uiMdiTabGroup in mdiTabGroups)
			{
				MdiChildTab[] mdiTabs = uiMdiTabGroup.MdiTabs;
				foreach (MdiChildTab uiMdiChildTab in mdiTabs)
				{
					if (uiMdiChildTab.Form == childForm)
					{
						return uiMdiChildTab;
					}
				}
			}
			return null;
		}

		public MxMdiTabGroup FindMdiTabGroup(string key)
		{
			if (key == null || key.Length == 0)
			{
				return null;
			}
			MxMdiTabGroup[] mdiTabGroups = MdiTabGroups;
			foreach (MxMdiTabGroup uiMdiTabGroup in mdiTabGroups)
			{
				if (string.Compare(uiMdiTabGroup.Key, key, ignoreCase: true) == 0)
				{
					return uiMdiTabGroup;
				}
			}
			return null;
		}

		internal Size GetPanelImageSize(CommandImageSource renderer)
		{
			if (_defaultPanelSettings.ImageSize.IsEmpty)
			{
				if (renderer.IsEmpty() || renderer.GetImageSize().IsEmpty)
				{
					return new Size(DefaultImageSize, DefaultImageSize);
				}
				return renderer.GetImageSize();
			}
			return _defaultPanelSettings.ImageSize;
		}

		internal Size GetSmallImageSize()
		{
			if (_imageList != null)
			{
				return _imageList.ImageSize;
			}
			return new Size(DefaultImageSize, DefaultImageSize);
		}

		internal Size GetLargeImageSizeFor(object value)
		{
			if (_defaultPanelSettings.LargeImageSize.IsEmpty)
			{
				if (value is Image)
				{
					return ((Image)value).Size;
				}
				if (value is Icon)
				{
					return ((Icon)value).Size;
				}
				return GetLargeImageSize();
			}
			return _defaultPanelSettings.LargeImageSize;
		}

		internal Size GetLargeImageSize()
		{
			if (_largeImageList != null)
			{
				return _largeImageList.ImageSize;
			}
			return new Size(DefaultLargeImageSize, DefaultLargeImageSize);
		}

		internal void ActivateParentForm()
		{
			if (_containerControl == null)
			{
				return;
			}
			Form form = RenderingHelper.FindForm(_containerControl);
			if (form != null)
			{
				form.Activate();
			}
		}

		internal object GetDesigner()
		{
			return _designer;
		}

		internal void SetDesigner(object designer)
		{
			_designer = designer;
		}

		internal void RaiseComponentChanging(string transactionName, DockPanelBase panel)
		{
			if (_designer != null)
			{
				try
				{
					DesignHelper.PanelManagerDesigner_RaiseComponentChanging(_designer, transactionName, panel);
				}
				catch (Exception ex)
				{
					Mordex.Windows.Common.MxMessageBox.Show(ex.Message);
				}
			}
		}

		internal void RaiseComponentChanged(string transactionName, DockPanelBase panel)
		{
			if (_designer != null)
			{
				try
				{
					DesignHelper.PanelManagerDesigner_RaiseComponentChanged(_designer, transactionName, panel);
				}
				catch (Exception ex)
				{
					Mordex.Windows.Common.MxMessageBox.Show(ex.Message);
				}
			}
		}

		private void StoreDockedPanelOrder()
		{
			if (!_isPanelLayoutSuspended)
			{
				DockPanelBase[] dockedPanels = DockedPanels;
				for (int i = 0; i < dockedPanels.Length; i++)
				{
					_dockInfo.GetDockedLayout().GetChildren().MoveToCurrentIndex(dockedPanels[i].Id, i);
				}
			}
		}

		private void OnControlComponentRemoving(object sender, ComponentEventArgs eventArgs)
		{
			if (eventArgs.Component is DockPanelBase && (_designer == null || DesignHelper.PanelManagerDesigner_IsHostLoaded(_designer)))
			{
				DockPanelBase uiPanelBase = (DockPanelBase)eventArgs.Component;
				if (uiPanelBase.PanelManager == this && ContainerControl != null && !ContainerControl.Disposing && !ContainerControl.IsDisposed && uiPanelBase.GetParentContainer() != null && _designer != null)
				{
					DesignHelper.PanelManagerDesigner_NotifyChange(_designer, changeComplete: false);
				}
			}
		}

		private void OnControlComponentRemoved(object sender, ComponentEventArgs eventArgs)
		{
			if (eventArgs.Component is DockPanelBase)
			{
				if (_designer == null || DesignHelper.PanelManagerDesigner_IsHostLoaded(_designer))
				{
					RemovePanelComponent((DockPanelBase)eventArgs.Component, false);
				}
			}
			else
			{
				if (eventArgs.Component != this)
				{
					return;
				}
				if (_designer != null && !DesignHelper.PanelManagerDesigner_IsHostLoaded(_designer))
				{
					IDesignerHost designerHost = (IDesignerHost)Site.GetService(typeof(IDesignerHost));
					if (designerHost != null && (!(designerHost.RootComponent is Control control) || control.Disposing))
					{
						return;
					}
				}
				while (_panels.Count > 0)
				{
					DockPanelBase uiPanelBase = _panels[0];
					RemovePanelComponent(uiPanelBase, true);
					uiPanelBase.Dispose();
				}
				ContainerControl = null;
			}
		}

		private void RemovePanelComponent(DockPanelBase panel, bool removeFromContainer)
		{
			if (removeFromContainer && base.Container != null)
			{
				base.Container.Remove(panel);
			}
			if (panel.ContainerCaptionControl != null && panel.ContainerCaptionControl.Container != null)
			{
				panel.ContainerCaptionControl.Container.Remove(panel.ContainerCaptionControl);
			}
			if (panel.PanelManager != this || panel.GetParentContainer() == null)
			{
				return;
			}
			if (panel is MxDockPanel)
			{
				base.Container.Remove(((MxDockPanel)panel).InnerContainer);
			}
			panel.GetParentContainer().Panels.Remove(panel);
			SuspendContainerLayout();
			RemoveRedundantGroups();
			ResumeContainerLayout(true);
			if (_designer != null)
			{
				DesignHelper.PanelManagerDesigner_NotifyChange(_designer, changeComplete: true);
			}
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				item.RecalculateLayout();
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int GetTileSize(DockPanelBase pnl)
		{
			if (pnl == null)
			{
				throw new ArgumentNullException(nameof(pnl));
			}
			return pnl.GetTileSize();
		}
	}
}
