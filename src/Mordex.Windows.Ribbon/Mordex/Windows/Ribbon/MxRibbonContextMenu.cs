using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	[ProvideProperty("ContextMenu", typeof(Component))]
	[Designer("Mordex.Windows.Ribbon.Design.ContextMenuDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public partial class MxRibbonContextMenu : Component, ICommandContainer, ICommandRenderHost, IExtenderProvider, IMessageFilter
	{
		private CommandCollection _commandCollection;

		private CommandBaseEditorDialog _childMenu;

		private string _key;

		private ImageList _largeImageList;

		private ImageList _smallImageList;

		internal Control _control;

		private Size _largeImageSize;

		private Size _smallImageSize;

		private bool _showToolTips;

		internal RibbonColorScheme _colorScheme;

		private bool? _useCompatibleTextRendering;

		private ContextMenuStrip _contextMenu;

		private ArrayList _menuTargets;

		private bool _isMessageFilterInstalled;

		internal Size _anchorSize;

		internal int _upwardOffset;

		private bool _showShortcutInToolTips;

		private bool _showShortcutInMenus;

		private string _name;

		private int _maxDropDownItems;

		private object _tag;

		internal object _designer;

		private static readonly object s_commandDropDownClosingEvent = new object();

		private static readonly object s_commandDropDownClosedEvent = new object();

		private static readonly object s_commandDropDownOpeningEvent = new object();

		private static readonly object s_commandDropDownOpenedEvent = new object();

		private static readonly object s_commandClickEvent = new object();

		private static readonly object s_closedEvent = new object();

		private static readonly object s_closingEvent = new object();

		private static readonly object s_openedEvent = new object();

		private static readonly object s_openingEvent = new object();

		private IContainer _components;

		public MxRibbonContextMenu()
		{
			CreateComponentContainer();
			InitializeDefaults();
		}

		public MxRibbonContextMenu(IContainer container)
		{
			container.Add(this);
			CreateComponentContainer();
			InitializeDefaults();
		}

		public MxRibbonContextMenu(string key)
		{
			CreateComponentContainer();
			InitializeDefaults();
			_key = key;
		}

		public static string GetRibbonVersionString()
		{
			string localPath = typeof(MxRibbon).Assembly.Location;
			if (string.IsNullOrEmpty(localPath))
			{
				// Single-file apps have no assembly path.
				return typeof(MxRibbon).Assembly.GetName().Version?.ToString() ?? "";
			}
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		[DefaultValue(false)]
		public bool GetContextMenu(Component component)
		{
			if (_menuTargets != null && _menuTargets.Contains(component))
			{
				return true;
			}
			return false;
		}

		private AccessibleObject GetAccessibleObject()
		{
			return null;
		}

		internal CommandBaseEditorDialog GetChildMenu()
		{
			return _childMenu;
		}

		internal void SetChildMenu(CommandBaseEditorDialog childMenu)
		{
			if (_childMenu != childMenu)
			{
				if (_childMenu != null)
				{
					_childMenu.Dispose();
					OnClosed(EventArgs.Empty);
				}
				_childMenu = childMenu;
			}
		}

		internal ContextMenuStrip GetContextMenuStrip()
		{
			if (_contextMenu == null)
			{
				_contextMenu = new ContextMenuStrip();
			}
			return _contextMenu;
		}

		private void InitializeDefaults()
		{
			_commandCollection = new CommandCollection(this, isQuickAccessCollection: false);
			_smallImageSize = Size.Empty;
			_largeImageSize = Size.Empty;
			_showToolTips = true;
			RebuildColorScheme();
			_key = "";
			_upwardOffset = 0;
			_maxDropDownItems = 25;
		}

		internal void RefreshChildMenuLayout()
		{
			if (GetChildMenu() != null)
			{
				_childMenu.RefreshForCommand(null, true);
			}
		}

		private void InstallMessageFilter()
		{
			if (!_isMessageFilterInstalled)
			{
				Application.AddMessageFilter(this);
				_isMessageFilterInstalled = true;
			}
		}

		private void UninstallMessageFilter()
		{
			if (!_isMessageFilterInstalled)
			{
				return;
			}
			Application.RemoveMessageFilter(this);
			_isMessageFilterInstalled = false;
		}

		public void SetContextMenu(Component component, bool value)
		{
			if (component == null)
			{
				return;
			}
			if (_menuTargets == null)
			{
				_menuTargets = new ArrayList();
			}
			if (_menuTargets.Contains(component) && !value)
			{
				if (component is Control && ((Control)component).ContextMenuStrip == GetContextMenuStrip())
				{
					((Control)component).ContextMenuStrip = null;
				}
				_menuTargets.Remove(component);
				if (component is Control)
				{
					((Control)component).MouseUp -= OnControlMouseUp;
					((Control)component).KeyUp -= OnControlKeyUp;
				}
				else if (component is NotifyIcon)
				{
					((NotifyIcon)component).MouseUp -= OnControlMouseUp;
				}
				return;
			}
			if (!value || _menuTargets.Contains(component))
			{
				return;
			}
			_menuTargets.Add(component);
			InstallMessageFilter();
			if (component is Control)
			{
				if (component is TextBox)
				{
					((Control)component).ContextMenuStrip = GetContextMenuStrip();
				}
				((Control)component).MouseUp += OnControlMouseUp;
				((Control)component).KeyUp += OnControlKeyUp;
			}
			else if (component is NotifyIcon)
			{
				((NotifyIcon)component).MouseUp += OnControlMouseUp;
			}
		}

		private bool TryHandleShortcut(Shortcut shortcut, Control inputControl, Message msg)
		{
			foreach (CommandBase command in Commands)
			{
				if (command.SearchShortcut(shortcut, inputControl, msg))
				{
					return true;
				}
			}
			return false;
		}

		internal void ShowDropDownFor(DropDownCommand cmd)
		{
			if (!cmd.Enabled || !cmd.Visible)
			{
				return;
			}
			ICommandContainer iAccessibleRibbonElement = cmd.GetParentContainer();
			if (cmd.GetParentContainer() is DropDownCommand)
			{
				if (GetChildMenu() != null && GetChildMenu().GetParentMenuCommand() == cmd.GetParentContainer())
				{
					GetChildMenu().OpenDropDown(cmd);
				}
				else
				{
					ShowDropDownFor(cmd.GetParentContainer() as DropDownCommand);
				}
			}
			else if (iAccessibleRibbonElement == this && Commands.Contains(cmd) && GetChildMenu() != null)
			{
				GetChildMenu().OpenDropDown(cmd);
			}
		}

		private int GetShortcutKeyCode(IntPtr wParam)
		{
			int keyData = NativeMethods.ToInt32Unchecked(wParam);
			if ((Control.ModifierKeys & Keys.Alt) != Keys.Alt)
			{
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
				{
					keyData += 65536;
				}
				if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
				{
					keyData += 131072;
				}
			}
			return keyData;
		}

		private bool IsWithinEnabledTarget(Control item)
		{
			while (item != null)
			{
				if (_menuTargets.Contains(item) && item.Enabled)
				{
					return true;
				}
				item = item.Parent;
			}
			return false;
		}

		private void RebuildColorScheme()
		{
			_colorScheme = ThemedRibbon.CreateScheme(MxThemeManager.ResolveDefault(96));
			ThemeSub.Set(true);
		}

		protected virtual void OnCommandDropDownClosing(DropDownClosingEventArgs e)
		{
			if (base.Events[s_commandDropDownClosingEvent] is DropDownClosingEventHandler dropDownClosingEventHandler)
			{
				dropDownClosingEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownClosed(CommandEventArgs e)
		{
			if (base.Events[s_commandDropDownClosedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownOpening(CommandCancelEventArgs e)
		{
			if (base.Events[s_commandDropDownOpeningEvent] is CommandCancelEventHandler commandCancelEventHandler)
			{
				commandCancelEventHandler(this, e);
			}
		}

		protected virtual void OnCommandDropDownOpened(CommandEventArgs e)
		{
			if (base.Events[s_commandDropDownOpenedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandClick(CommandEventArgs e)
		{
			if (base.Events[s_commandClickEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected override void Dispose(bool disposing)
		{
			UninstallMessageFilter();
			_themeSubscription?.Set(false);
			if (disposing)
			{
				if (_menuTargets != null)
				{
					foreach (Component item in (ArrayList)_menuTargets.Clone())
					{
						SetContextMenu(item, value: false);
					}
					_menuTargets = null;
				}
				if (_childMenu != null)
				{
					CommandBaseEditorDialog openMenu = _childMenu;
					_childMenu = null;
					openMenu.Dispose();
				}
				if (_contextMenu != null)
				{
					_contextMenu.Dispose();
					_contextMenu = null;
				}
				if (_components != null)
				{
					_components.Dispose();
				}
				for (int i = 0; i < Commands.Count; i++)
				{
					Commands[i].SetParentContainer(null);
					Commands[i].CommandsOwner = null;
					Commands[i].Dispose();
				}
				base.Events.Dispose();
			}
			base.Dispose(disposing);
		}

		private void CreateComponentContainer()
		{
			_components = new Container();
		}
	}
}
