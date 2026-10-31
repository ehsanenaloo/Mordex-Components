using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	public class DesignHelper
	{
		private const string DesignAssemblyName = "Mordex.Windows.Ribbon.Design";

		private static bool? s_isDesignAssemblyAvailable = null;

		public static void ShowAboutBox(MxRibbon ribbon)
		{
			using RibbonAboutDialog ribbonAboutDialog = new RibbonAboutDialog();
			ribbonAboutDialog.ShowRibbonAbout(ribbon);
		}

		public static void ShowAboutBox(MxRibbonStatusBar statusBar)
		{
			using RibbonAboutDialog ribbonAboutDialog = new RibbonAboutDialog();
			ribbonAboutDialog.ShowStatusBarAbout(statusBar);
		}

		public static DialogResult ShowRibbonDesigner(MxRibbon ribbon)
		{
			using StatusBarDesignerDialog statusBarDesignerDialog = new StatusBarDesignerDialog();
			statusBarDesignerDialog.ShowRibbonDesignerDialog(ribbon);
			return statusBarDesignerDialog.DialogResult;
		}

		public static DialogResult ShowRibbonDesigner(MxRibbonStatusBar statusBar)
		{
			using StatusBarDesignerDialog statusBarDesignerDialog = new StatusBarDesignerDialog();
			statusBarDesignerDialog.ShowStatusBarDesignerDialog(statusBar);
			return statusBarDesignerDialog.DialogResult;
		}

		public static void RemoveFromOwner(RibbonTab tab)
		{
			tab.GetTabOwner().RemoveTab(tab);
		}

		public static void RemoveFromOwner(CommandBase command)
		{
			if (command.GetParentContainer() != null)
			{
				command.GetParentContainer().DetachCommand(command);
			}
		}

		public static Component GetOwner(CommandBase command)
		{
			return command.GetParentContainer() as Component;
		}

		public static CommandBase GetCommandAt(object glyphOwner, Point point)
		{
			if (glyphOwner is IRibbonHitTestSource hitTestSource)
			{
				return hitTestSource.GetCommandAt(point);
			}
			return null;
		}

		public static Rectangle GetCommandBounds(object glyphOwner, CommandBase command)
		{
			if (glyphOwner is IRibbonHitTestSource hitTestSource)
			{
				return hitTestSource.GetCommandBounds(command);
			}
			return Rectangle.Empty;
		}

		public static Rectangle GetBackstageBarBounds(object glyphOwner)
		{
			if (glyphOwner is IRibbonHitTestSource hitTestSource)
			{
				return hitTestSource.GetBackstageBarBounds();
			}
			return Rectangle.Empty;
		}

		public static bool ProcessBackstageMouseDown(MxRibbon ribbon, MouseButtons button, int x, int y)
		{
			return ribbon.ForwardBackstageMouseDown(button, x, y);
		}

		public static bool ProcessBackstageMouseUp(MxRibbon ribbon, MouseButtons button, int x, int y)
		{
			return ribbon.ForwardBackstageMouseUp(button, x, y);
		}

		public static bool ProcessBackstageMouseMove(MxRibbon ribbon, MouseButtons button, int x, int y)
		{
			return ribbon.ForwardBackstageMouseMove(button, x, y);
		}

		public static ISupportsDesignerMouse GetRibbonGroup(MxRibbon ribbon, Point pt)
		{
			return ribbon._tabContentRenderer.FindGroupRendererAt(pt.X, pt.Y);
		}

		public static ISupportsDesignerMouse GetPanelHandler(MxRibbonStatusBar statusBar, Point pt)
		{
			return statusBar.GetPanelRendererAt(pt.X, pt.Y);
		}

		public static void SetDesignerMouse(MxRibbon ribbon, bool value)
		{
			ribbon.SetShowAddNewItems(value);
		}

		public static void SetDesignerMouse(MxRibbonStatusBar statusBar, bool value)
		{
			statusBar.SetDesignerMouseActive(value);
		}

		public static void SetDesigner(MxRibbonStatusBar statusBar, object designer)
		{
			statusBar._designer = designer;
		}

		public static void SetDesigner(MxRibbon ribbon, object designer)
		{
			ribbon._designer = designer;
			if (designer != null && ribbon.Site != null)
			{
				IDesignerHost designerHost = ribbon.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
				if (designerHost != null)
				{
					ribbon._isDesignerLoaded = !designerHost.Loading;
				}
			}
		}

		public static void SetDesigner(MxRibbonContextMenu contextMenu, object designer)
		{
			contextMenu._designer = designer;
		}

		public static void HideChildMenu(MxRibbonContextMenu contextMenu)
		{
			if (contextMenu.GetChildMenu() != null)
			{
				contextMenu.GetChildMenu().HideWithChildMenus();
			}
		}

		public static void ShowChildMenu(MxRibbonContextMenu contextMenu)
		{
			if (contextMenu.GetChildMenu() != null)
			{
				contextMenu.GetChildMenu().ShowWithChildMenus();
			}
		}

		public static bool ProcessChildMenuComponentSelected(MxRibbonContextMenu contextMenu, ISelectionService selectionService)
		{
			if (contextMenu.GetChildMenu() != null)
			{
				return contextMenu.GetChildMenu().OnComponentSelected(selectionService);
			}
			return false;
		}

		public static void PerformFormSubclassVisibleChanged(MxRibbonFormAdorner formAdorner)
		{
			if (formAdorner != null && formAdorner._ribbonWindowMessageHandler != null)
			{
				((FormAdornerMessageHandler)formAdorner._ribbonWindowMessageHandler).InitializeControls(null, EventArgs.Empty);
			}
		}

		public static void AddLayoutCompleteHandler(MxRibbonStatusBar statusBar, EventHandler handler)
		{
			statusBar.AddLayoutCalculatedHandler(handler);
		}

		public static void RemoveLayoutCompleteHandler(MxRibbonStatusBar statusBar, EventHandler handler)
		{
			statusBar.RemoveLayoutCalculatedHandler(handler);
		}

		public static void AddLayoutCompleteHandler(MxRibbon ribbon, EventHandler handler)
		{
			ribbon.AddContentRenderedHandler(handler);
		}

		public static void RemoveLayoutCompleteHandler(MxRibbon ribbon, EventHandler handler)
		{
			ribbon.RemoveContentRenderedHandler(handler);
		}

		public static int HitTest(MxRibbonStatusBar statusBar, Point pt)
		{
			return (int)statusBar.GetHitTest(pt.X, pt.Y);
		}

		public static int HitTest(MxRibbon ribbon, Point pt)
		{
			return (int)ribbon.GetHitTest(pt.X, pt.Y);
		}

		public static int BackstageHitTest(MxRibbon ribbon, Point screenPt)
		{
			return ribbon.GetBackstageHitArea(screenPt);
		}

		public static bool GetDesignerItemPressed(MxRibbonStatusBar statusBar)
		{
			return statusBar._designerItemPressed;
		}

		public static void SetDesignerItemPresed(MxRibbonStatusBar statusBar, bool value)
		{
			statusBar._designerItemPressed = value;
		}

		public static bool GetDesignerItemPressed(MxRibbon ribbon)
		{
			return ribbon._designerItemPressed;
		}

		public static void SetDesignerItemPresed(MxRibbon ribbon, bool value)
		{
			ribbon._designerItemPressed = value;
		}

		public static bool IsRibbonGroup(CommandBase command)
		{
			return command.CommandType == CommandType.RibbonGroup;
		}

		public static int GetCommandIndex(CommandBase command)
		{
			return command.GetIndexInOwner();
		}

		public static CommandType GetCommandType(CommandBase command)
		{
			return command.CommandType;
		}

		public static CommandType GetCommandType(Component component)
		{
			if (component is ICommandContainer iAccessibleRibbonElement)
			{
				return iAccessibleRibbonElement.CommandType;
			}
			return CommandType.CommandBase;
		}

		public static bool IsCommandOwner(CommandBase command)
		{
			return command is ICommandContainer;
		}

		public static bool IsHorizontalLayout(Component component)
		{
			if (component is ICommandContainer iAccessibleRibbonElement)
			{
				return iAccessibleRibbonElement.LayoutDirection == BoxStyle.Horizontal;
			}
			return false;
		}

		public static void SetChildMenuVisibility(MxRibbonStatusBar statusBar, bool visible)
		{
			if (statusBar.GetChildMenu() != null)
			{
				if (visible)
				{
					statusBar.GetChildMenu().ShowWithChildMenus();
				}
				else
				{
					statusBar.GetChildMenu().HideWithChildMenus();
				}
			}
		}

		public static void SetChildMenuVisibility(MxRibbon ribbon, bool visible)
		{
			if (ribbon.GetPopupMenu() != null)
			{
				if (visible)
				{
					ribbon.GetPopupMenu().ShowWithChildMenus();
				}
				else
				{
					ribbon.GetPopupMenu().HideWithChildMenus();
				}
			}
		}

		public static void PerformDesignModeSelectionChanged(MxRibbonStatusBar statusBar, ISelectionService selectionService)
		{
			if (statusBar.GetChildMenu() != null)
			{
				statusBar.GetChildMenu().OnComponentSelected(selectionService);
			}
		}

		public static void PerformDesignModeSelectionChanged(MxRibbon ribbon, ISelectionService selectionService)
		{
			if (ribbon.GetPopupMenu() != null)
			{
				ribbon.GetPopupMenu().OnComponentSelected(selectionService);
			}
		}

		public static void ShowFileMenu(MxRibbon ribbon)
		{
			ribbon.ShowControlBoxMenu();
		}

		public static bool OwnerCanParentCommandType(object owner, CommandType commandType)
		{
			if (owner is ICommandContainer iAccessibleRibbonElement)
			{
				return iAccessibleRibbonElement.CanBeParentTo(commandType);
			}
			return false;
		}

		public static int GetVisibleGroupsCount(MxRibbon ribbon)
		{
			return ribbon.GetVisibleGroupCount();
		}

		public static Rectangle GetRibbonBounds(MxRibbon ribbon)
		{
			return ribbon.GetContentAreaBounds();
		}

		public static object GetItemsBox(MxRibbon ribbon, CommandBase command)
		{
			return ribbon.FindCommandRenderer(command) as CommandContainerRenderer;
		}

		public static object GetItemsBox(MxRibbonStatusBar statusBar, CommandBase command)
		{
			return statusBar.FindCommandRenderer(command) as CommandContainerRenderer;
		}

		public static object GetDropInfo(object itemsBox, Point pt, CommandBase command)
		{
			if (itemsBox is CommandContainerRenderer ribbonState)
			{
				return ribbonState.GetDropInfo(pt, command);
			}
			return null;
		}

		public static Rectangle GetDropRect(object dropInfo)
		{
			if (dropInfo is CommandDropInfo ribbonLayoutState)
			{
				return ribbonLayoutState._dropBounds;
			}
			throw new ArgumentException();
		}

		public static int GetDropIndex(object dropInfo)
		{
			if (dropInfo is CommandDropInfo ribbonLayoutState)
			{
				return ribbonLayoutState._dropIndex;
			}
			throw new ArgumentException();
		}

		public static Rectangle GetTabBounds(MxRibbon ribbon, RibbonTab tab)
		{
			return ribbon.GetTabBounds(tab);
		}

		public static void DrawGlyphHot(Control ctl, Graphics graphics, Rectangle bounds)
		{
			if (ctl is IRibbonHitTestSource hitTestSource)
			{
				RibbonColorScheme colorScheme = hitTestSource.ColorScheme;
				RibbonChromePainter.DrawTranslucentHotButtonBackground(graphics, bounds, colorScheme, true);
			}
		}

		public static void InsertCommand(CommandBase owner, int index, CommandBase command)
		{
			if (owner is ICommandContainer iAccessibleRibbonElement)
			{
				iAccessibleRibbonElement.InsertCommand(index, command);
			}
		}

		public static void InsertCommandInOwner(CommandBase command, int index, CommandBase commandToInsert)
		{
			if (command.GetParentContainer() != null)
			{
				command.GetParentContainer().InsertCommand(index, commandToInsert);
			}
		}

		public static int GetSmallItemHeight(MxRibbonContextMenu contextMenu)
		{
			return ((ICommandRenderHost)contextMenu).SmallItemHeight;
		}

		public static void ShowBackStageMenu(MxRibbon ribbon)
		{
			ribbon.ShowBackstageMenu();
		}

		public static void PerformDesignHostComplete(MxRibbon ribbon)
		{
			RibbonCustomCaptionMessageHandler captionHandler = ribbon._ribbonWindowMessageHandler as RibbonCustomCaptionMessageHandler;
			ribbon._isDesignerLoaded = true;
			ribbon.Invalidate();
			captionHandler?.OnFormVisibleChanged(null, EventArgs.Empty);
		}

		// The designer lives in the design-time assembly, so it is called by name.
		private static T InvokeDesigner<T>(object designer, string methodName, T fallback, params object[] args)
		{
			MethodInfo method = designer?.GetType().GetMethod(methodName);
			if (method == null)
			{
				return fallback;
			}
			try
			{
				return method.Invoke(designer, args) is T result ? result : fallback;
			}
			catch (TargetInvocationException)
			{
				// A failing designer must not break the control that reports to it.
				return fallback;
			}
		}

		public static bool Designer_SelectComponent(object designer, Component component)
		{
			return InvokeDesigner(designer, "RibbonDesigner_SelectComponent", false, component);
		}

		public static bool Designer_Locked(object designer)
		{
			return InvokeDesigner(designer, "RibbonDesigner_IsLocked", false);
		}

		internal static void EnsureDesignAssemblyLoaded()
		{
			if (!s_isDesignAssemblyAvailable.HasValue)
			{
				try
				{
					Assembly.Load(new AssemblyName(DesignAssemblyName));
					s_isDesignAssemblyAvailable = true;
				}
				catch (Exception ex) when (ex is FileNotFoundException || ex is FileLoadException || ex is BadImageFormatException)
				{
					s_isDesignAssemblyAvailable = false;
				}
			}
			if (!s_isDesignAssemblyAvailable.Value)
			{
				throw new InvalidOperationException("This Form uses a Mordex WinForms Control that was not properly installed in this machine.  The Form Designer was prevented from being shown in order to avoid a possible corruption of the Form Settings. Please install the Mordex WinForms Controls using the Controls Setup in order to edit this Form.");
			}
		}
	}
}
