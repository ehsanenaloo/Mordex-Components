using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	public static class DesignHelper
	{
		private static bool? _isDesignAssemblyLoaded = null;

		public static void ShowAboutBox(MxDockManager panelManager)
		{
			AboutDialog uiAboutDialog = new AboutDialog();
			uiAboutDialog.ShowForDockManager(panelManager);
			uiAboutDialog.Dispose();
		}

		public static void ShowAboutBox(MxColorPicker colorPicker)
		{
			AboutDialog uiAboutDialog = new AboutDialog();
			uiAboutDialog.ShowForColorPicker(colorPicker);
			uiAboutDialog.Dispose();
		}

		public static void ShowAboutBox(MxComboBase combo)
		{
			AboutDialog uiAboutDialog = new AboutDialog();
			uiAboutDialog.ShowForComboBase(combo);
			uiAboutDialog.Dispose();
		}

		public static void ShowAboutBox(MxStatusBar statusBar)
		{
			AboutDialog uiAboutDialog = new AboutDialog();
			uiAboutDialog.ShowForStatusBar(statusBar);
			uiAboutDialog.Dispose();
		}

		public static void ShowAboutBox(MxTabControl tabControl)
		{
			AboutDialog uiAboutDialog = new AboutDialog();
			uiAboutDialog.ShowForTabControl(tabControl);
			uiAboutDialog.Dispose();
		}

		public static void ShowAboutBox(MxCommandManager commandManager)
		{
			AboutDialog uiAboutDialog = new AboutDialog();
			uiAboutDialog.ShowForCommandManager(commandManager);
			uiAboutDialog.Dispose();
		}

		public static void ForceMouseLeave(MxCommandBar commandBar)
		{
			commandBar?.ClearHotTracking();
		}

		public static void EndPanelManagerInitialize(MxDockManager panelManager)
		{
			panelManager.CompleteInitialization();
		}

		public static object GetCurrentDesigner(MxDockManager panelManager)
		{
			return panelManager.GetDesigner();
		}

		public static void SetPanelManagerDesigner(MxDockManager panelManager, object designer)
		{
			panelManager.SetDesigner(designer);
		}

		public static IMessageFilter GetDragHandler(MxDockManager panelManager)
		{
			return panelManager?.GetDragFilter();
		}

		public static void PerformEndDrag(MxDockManager panelManager)
		{
			if (panelManager != null && panelManager.GetDragFilter() != null)
			{
				panelManager.GetDragFilter().EndDrag(cancel: true);
			}
		}

		public static bool IsPanelGroupTab(MxDockPanelGroup panelGroup)
		{
			return panelGroup.IsTabGroup();
		}

		public static Component GetPanelOwner(DockPanelBase panel)
		{
			return panel.GetParentContainer() as Component;
		}

		public static void SetFloatingPanelsVisibility(MxDockManager panelManager, bool visible)
		{
			foreach (DockPanelBase uiPanelBase in panelManager.FloatingPanels)
			{
				if (uiPanelBase._floatingWindow != null)
				{
					uiPanelBase._floatingWindow.Visible = visible;
				}
			}
		}

		public static bool TryTabScroll(MxTabControl tab)
		{
			Point position = Cursor.Position;
			position = tab.PointToClient(position);
			if (tab._scrollBackButton != null && tab._scrollBackButton.IsEnabled() && tab._scrollBackButton._bounds.Contains(position))
			{
				tab.ScrollBack();
				return true;
			}
			if (tab._scrollForwardButton != null && tab._scrollForwardButton.IsEnabled() && tab._scrollForwardButton._bounds.Contains(position))
			{
				tab.ScrollForward();
				return true;
			}
			return false;
		}

		public static void ForceEndPanelResize(DockPanelBase panel)
		{
			panel.EndSplitterDrag(0, 0, true);
		}

		public static bool TabPageFromPoint(MxTabControl tab, Point pt, ref MxTabPage tabPage)
		{
			TabPageRenderer uiTabPageRenderer = tab.HitTestTab(pt.X, pt.Y);
			if (uiTabPageRenderer != null && ((FormatStyleLayoutState)uiTabPageRenderer).IsVisible())
			{
				tabPage = uiTabPageRenderer.GetTabPage();
				return true;
			}
			return false;
		}

		public static IList GetControlsList(EditContextMenu contextMenu)
		{
			return contextMenu._editMenuControls;
		}

		public static MxCommandManager GetParentManager(EditContextMenu contextMenu)
		{
			return contextMenu.GetOwningManager();
		}

		public static DialogResult ShowCommandBarsDesignerDialog(MxCommandManager commandManager)
		{
			CommandManagerEditorDialog uiCommandManagerEditorDialog = new CommandManagerEditorDialog();
			commandManager.SetEditorDialogOpen(true);
			uiCommandManagerEditorDialog.ShowCommandManagerEditor(commandManager);
			commandManager.SetEditorDialogOpen(false);
			DialogResult dialogResult = uiCommandManagerEditorDialog.DialogResult;
			uiCommandManagerEditorDialog.Dispose();
			return dialogResult;
		}

		public static DialogResult ShowPanelManagerDesignerDialog(MxDockManager panelManager)
		{
			CommandManagerEditorDialog uiCommandManagerEditorDialog = new CommandManagerEditorDialog();
			uiCommandManagerEditorDialog.ShowPanelManagerEditor(panelManager);
			DialogResult dialogResult = uiCommandManagerEditorDialog.DialogResult;
			uiCommandManagerEditorDialog.Dispose();
			return dialogResult;
		}

		public static DialogResult ShowAutoHidePanelsDialog(MxDockManager panelManager)
		{
			PanelTreeDialog panelTreeDialog = new PanelTreeDialog();
			DialogResult result = panelTreeDialog.ShowAutoHidePanels(panelManager);
			panelTreeDialog.Dispose();
			return result;
		}

		public static DialogResult ShowClosedPanelsDialog(MxDockManager panelManager)
		{
			PanelTreeDialog panelTreeDialog = new PanelTreeDialog();
			DialogResult result = panelTreeDialog.ShowClosedPanels(panelManager);
			panelTreeDialog.Dispose();
			return result;
		}

		public static void CreatePanelGroup(IContainer container, MxDockManager panelManager, DockPanelBase panel, PanelGroupStyle groupStyle)
		{
			PanelDropInfo panelDropInfo = new PanelDropInfo();
			panelDropInfo._createsGroup = true;
			panelDropInfo._panelDockStyle = panel.DockStyle;
			panelDropInfo._groupStyle = groupStyle;
			panelDropInfo._targetPanel = panel;
			panelDropInfo._insertIndex = 1;
			MxDockPanel uiPanel = new MxDockPanel();
			panel.PanelManager.Panels.Add(uiPanel);
			if (container != null)
			{
				container.Add(uiPanel);
				container.Add(uiPanel.InnerContainer);
			}
			uiPanel.Text = uiPanel.Name;
			panelManager.EndPanelDrag(uiPanel, panelDropInfo);
		}

		// The panel layout as designer code saves it: empty groups are left out and a group with one panel is replaced by that panel.
		// The live layout is only read.
		public static void GetSavedPanelInfo(MxDockManager panelManager, out IEnumerable dockedPanels, out IEnumerable floatingPanels)
		{
			DockLayoutSnapshot layout = panelManager.GetLayoutSnapshot();
			dockedPanels = SavedPanelInfo.FoldChildren(layout.GetDockedLayout());
			floatingPanels = SavedPanelInfo.FoldChildren(layout.GetFloatingLayout());
		}

		public static void ForceDragDrop(MxCommandBar commandBar)
		{
			if (commandBar._messageFilter != null)
			{
				commandBar._messageFilter.EndDrag();
			}
		}

		public static void OnCommandManagerInitialize(MxCommandManager commandManager)
		{
			commandManager.AddRebarsToContainer();
		}

		public static void SetFloatingBarsVisibility(MxCommandManager commandManager, bool visible)
		{
			EditorDialogCollection uiCommandManagerCollection = commandManager.GetEditorDialogs();
			if (uiCommandManagerCollection == null)
			{
				return;
			}
			foreach (CommandBarEditorDialog item in uiCommandManagerCollection)
			{
				if (visible)
				{
					item.Show();
				}
				else
				{
					item.Hide();
				}
			}
		}

		public static bool PanelManagerDesigner_IsHostLoaded(object designer)
		{
			if (designer != null)
			{
				PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(designer).Find("PanelManagerDesigner_IsHostLoaded", ignoreCase: true);
				if (propertyDescriptor != null)
				{
					return (bool)propertyDescriptor.GetValue(designer);
				}
			}
			return false;
		}

		public static void PanelManagerDesigner_NotifyChange(object designer, bool changeComplete)
		{
			FindDesignerMethod(designer, "PanelManagerDesigner_NotifyChange")?.Invoke(designer, new object[1] { changeComplete });
		}

		public static DesignerTransaction PanelManagerDesigner_StartTransaction(object designer, string name)
		{
			if (designer != null)
			{
				MethodInfo method = FindDesignerMethod(designer, "PanelManagerDesigner_StartTransaction");
				if (method != null)
				{
					return (DesignerTransaction)method.Invoke(designer, new object[1] { name });
				}
			}
			return null;
		}

		public static void PanelManagerDesigner_SelectContainer(object designer)
		{
			FindDesignerMethod(designer, "PanelManagerDesigner_SelectContainer")?.Invoke(designer, null);
		}

		public static void PanelManagerDesigner_EndPanelDragTransaction(object designer, DockPanelBase panel, DockPanelBase groupCreated)
		{
			FindDesignerMethod(designer, "PanelManagerDesigner_EndPanelDragTransaction")?.Invoke(designer, new object[2] { panel, groupCreated });
		}

		public static void PanelManagerDesigner_RaiseComponentChanging(object designer, string transactionName, DockPanelBase panel)
		{
			FindDesignerMethod(designer, "PanelManagerDesigner_RaiseComponentChanging")?.Invoke(designer, new object[2] { transactionName, panel });
		}

		public static void PanelManagerDesigner_RaiseComponentChanged(object designer, string transactionName, DockPanelBase panel)
		{
			FindDesignerMethod(designer, "PanelManagerDesigner_RaiseComponentChanged")?.Invoke(designer, new object[2] { transactionName, panel });
		}

		// The designer assembly is unsigned and ships with this one, so it is loaded by simple name.
		internal const string DesignAssemblyName = "Mordex.Windows.UI.Design";

		private static readonly ConcurrentDictionary<(Type, string), MethodInfo> _designerMethods = new ConcurrentDictionary<(Type, string), MethodInfo>();

		internal static MethodInfo FindDesignerMethod(object designer, string name)
		{
			if (designer == null)
			{
				return null;
			}
			return _designerMethods.GetOrAdd((designer.GetType(), name), key => key.Item1.GetMethod(key.Item2));
		}

		internal static void EnsureDesignAssemblyLoaded()
		{
			if (!_isDesignAssemblyLoaded.HasValue)
			{
				try
				{
					Assembly.Load(DesignAssemblyName);
					_isDesignAssemblyLoaded = true;
				}
				catch (Exception ex) when (ex is FileNotFoundException || ex is FileLoadException || ex is BadImageFormatException)
				{
					_isDesignAssemblyLoaded = false;
				}
			}
			if (!_isDesignAssemblyLoaded.Value)
			{
				throw new Exception("This Form uses a Mordex WinForms Control that was not properly installed in this machine.  The Form Designer was prevented from being shown in order to avoid a possible corruption of the Form Settings. Please install the Mordex WinForms Controls using the Controls Setup in order to edit this Form.");
			}
		}
	}
}
