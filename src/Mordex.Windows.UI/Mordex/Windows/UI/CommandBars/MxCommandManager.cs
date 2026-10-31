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
	[ProvideProperty("ContextMenu", typeof(Component))]
	[ToolboxItem(true)]
	[DefaultEvent("CommandClick")]
	[Designer("Mordex.Windows.UI.Design.CommandManagerDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public partial class MxCommandManager : Component, IAccessibleCommandBarsElement, IFormatStyleOwner, ICommandBarOwner, ISupportInitialize, IMordexXmlLayoutsSupport, IMessageFilter, IExtenderProvider, IPersistComponentSettings, INotifyDesignChange
	{
		private const int DefaultShowFullMenuDelay = 5000;

		private const int WmActivateApp = 0x001C;

		private const int WmNcLButtonDown = 0x00A1;

		private const int WmNcRButtonDown = 0x00A4;

		private const int WmNcMButtonDown = 0x00A7;

		private const int WmKeyDown = 0x0100;

		private const int WmKeyUp = 0x0101;

		private const int WmChar = 0x0102;

		private const int WmSysKeyDown = 0x0104;

		private const int WmSysKeyUp = 0x0105;

		private const int WmSysChar = 0x0106;

		private const int WmLButtonDown = 0x0201;

		private const int WmRButtonDown = 0x0204;

		private const int WmRButtonUp = 0x0205;

		private const int WmMButtonDown = 0x0207;

		private const int WmXButtonDown = 0x020B;

		// Bit 22 of the WM_SYSCHAR lParam (inside the scan code byte).
		private const int SysCharSkipFlag = 0x400000;

		private static readonly Size DefaultSmallImageSize = new Size(16, 16);

		private static readonly Size DefaultLargeImageSize = new Size(24, 24);

		private ApplicationSettings _applicationSettings;

		private bool _saveSettings;

		private string _settingsKey;

		private ContextMenuStrip _contextMenu;

		private bool _isF10Handled;

		private Hashtable _contextMenusByComponent;

		private Container _components = new Container();

		private bool _isInitializing;

		private bool _isEditorDialogOpen;

		private CommandBarsColorSet _commandBarsAppearanceState;

		private EditContextMenu _editContextMenu;

		private Guid _id = Guid.Empty;

		private CommandBarsLayout _currentLayout;

		private CommandBarsLayout _appliedLayout;

		private CommandBarsLayoutCollection _layouts;

		private Stream _layoutStream;

		internal bool _isBatchUpdating;

		internal bool _isLoadingLayout;

		private MxCommandCollection _commands;

		private CommandCategoryCollection _categories;

		private CommandBarCollection _commandBars;

		private ContextMenuCollection _contextMenus;

		private bool? _useCompatibleTextRendering;

		private bool _useMnemonic = true;

		private BuiltInTextList _builtInTexts;

		private Control _containerControl;

		private ICommandBarsElement _activeElement;

		private MxCommand _selectedCommand;

		private bool _lockCommandBars;

		private bool _showQuickCustomizeMenu = true;

		private InheritableBoolean _allowCustomize;

		private InheritableBoolean _allowClose;

		private ShortcutBehavior _shortcutBehavior;

		private InheritableBoolean _showCustomizeButton;

		private bool _showShadowUnderMenus;

		private InheritableBoolean _showAddRemoveButton;

		private bool _alwaysShowFullMenus;

		private int _showFullMenuDelay = DefaultShowFullMenuDelay;

		private bool _showShortcutInToolTips;

		private bool _useFontInFontNames = true;

		private StateStyles _commandsStateStyles;

		private PopupSettings _popupSettings;

		private DropDownAnimation _menuAnimation = DropDownAnimation.System;

		private FormatStyle _commandBarsFormatStyle;

		internal bool _isCustomizing;

		private bool _isToolbarsMenuDirty = true;

		private MxContextMenu _toolbarsMenu;

		private MxCommand _separatorCommand;

		private Form _form;

		private MxCommandManager _pendingMergeManager;

		private bool _keepMergeSettings = true;

		private MxCommandManager _mergeHost;

		private MxCommandManager _mergedManager;

		private static readonly object _mergeChangingEvent = new object();

		private static readonly object _mergeChangedEvent = new object();

		private bool _isExplicitMerge;

		private bool _allowMerge = true;

		private EditorDialogCollection _editorDialogs;

		private bool _showToolTips = true;

		private bool _showToolTipOnMenus;

		private MxCommandBarHost _topRebar;

		private MxCommandBarHost _bottomRebar;

		private MxCommandBarHost _leftRebar;

		private MxCommandBarHost _rightRebar;

		private object _tag;

		private CommandStyle _commandsStyle;

		internal ImageList _mergedSmallImageList;

		private ImageList _smallImageList;

		internal ImageList _mergedLargeImageList;

		private ImageList _largeImageList;

		private TextAlign _textAlign = TextAlign.Right;

		private int _daysForInfrequency;

		private View _popupView = View.SmallIcons;

		private View _view = View.SmallIcons;

		private Control _activeCommandControl;

		private MxCommandBar _activeBar;

		private bool _canActivateMenuOnAltUp = true;

		private static readonly object _layoutLoadEvent = new object();

		private static readonly object _currentLayoutChangedEvent = new object();

		private static readonly object _currentLayoutChangingEvent = new object();

		private static readonly object _viewChangedEvent = new object();

		private static readonly object _addingCommandBarEvent = new object();

		private static readonly object _visualStyleChangedEvent = new object();

		private static readonly object _systemColorsChangedEvent = new object();

		private static readonly object _containerControlChangedEvent = new object();

		private static readonly object _designChangeEvent = new object();

		private Size _smallImageSize = DefaultSmallImageSize;

		private Size _largeImageSize = DefaultLargeImageSize;

		private static readonly object _removingCommandBarEvent = new object();

		private static readonly object _commandBarAddedEvent = new object();

		private static readonly object _commandBarRemovedEvent = new object();

		private static readonly object _commandAddedEvent = new object();

		private static readonly object _commandMouseHoverEvent = new object();

		private static readonly object _commandMouseLeaveEvent = new object();

		private static readonly object _commandClickEvent = new object();

		private static readonly object _commandControlValueChangedEvent = new object();

		private static readonly object _commandCloseUpEvent = new object();

		private static readonly object _commandClosingEvent = new object();

		private static readonly object _commandPopupEvent = new object();

		private static readonly object _selectedCommandChangedEvent = new object();

		private static readonly object _showingCustomizeDialogEvent = new object();

		private static readonly object _customizeDialogShownEvent = new object();

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (!_isInitializing)
			{
				return;
			}
			_isInitializing = false;
			if (_layoutStream != null)
			{
				LayoutStream = _layoutStream;
				_layoutStream = null;
			}
			if (_topRebar != null)
			{
				ShowRebar(_topRebar);
			}
			if (_leftRebar != null && _leftRebar.HasAddedBar())
			{
				ShowRebar(_leftRebar);
			}
			if (_rightRebar != null && _rightRebar.HasAddedBar())
			{
				ShowRebar(_rightRebar);
			}
			if (_bottomRebar != null && _bottomRebar.HasAddedBar())
			{
				ShowRebar(_bottomRebar);
			}
			foreach (MxCommand item in _commands)
			{
				if (item.BaseCommand != null)
				{
					throw new InvalidOperationException("A command with a BaseCommand can not be added to the UICommandManager");
				}
				if (!IsCommandKeyUnique(item, item.Key))
				{
					throw new ArgumentException(string.Format("A Command with key '{0}' already exist.", item.Key));
				}
			}
			foreach (MxCommand command in Commands)
			{
				BindChildBaseCommands(command);
			}
			foreach (MxCommandBar commandBar in _commandBars)
			{
				if (!IsCommandBarKeyUnique(commandBar, commandBar.Key))
				{
					throw new ArgumentException(string.Format("A CommandBar with key '{0}' already exist.", commandBar.Key));
				}
				commandBar.CompleteInitialization();
			}
			foreach (MxContextMenu contextMenu in _contextMenus)
			{
				if (!IsContextMenuKeyUnique(contextMenu, contextMenu.Key))
				{
					throw new ArgumentException(string.Format("A ContextMenu with key '{0}' already exist.", contextMenu.Key));
				}
				contextMenu.CompleteInitialization();
			}
			_isInitializing = true;
			TopRebar.ReapplyBarRowIndexes();
			if (_topRebar.Container == null)
			{
				AddToDesignContainer(_topRebar, "TopRebar");
			}
			BottomRebar.ReapplyBarRowIndexes();
			if (_bottomRebar.Container == null)
			{
				AddToDesignContainer(_bottomRebar, "BottomRebar");
			}
			LeftRebar.ReapplyBarRowIndexes();
			if (_leftRebar.Container == null)
			{
				AddToDesignContainer(_leftRebar, "LeftRebar");
			}
			RightRebar.ReapplyBarRowIndexes();
			if (_rightRebar.Container == null)
			{
				AddToDesignContainer(_rightRebar, "RightRebar");
			}
			_isInitializing = false;
			RefreshRebars();
			SaveDefaults();
		}

		MxCommandManager ICommandBarOwner.GetCommandManagerOwner()
		{
			return this;
		}

		MxCommandManager IAccessibleCommandBarsElement.GetCommandManager()
		{
			return this;
		}

		ImageList IAccessibleCommandBarsElement.GetImageList(bool smallIcons, MxCommand cmd)
		{
			if (smallIcons)
			{
				if (cmd != null && cmd.IsMergedCommand && _mergedSmallImageList != null)
				{
					return _mergedSmallImageList;
				}
				return _smallImageList;
			}
			if (cmd != null && cmd.IsMergedCommand && _mergedLargeImageList != null)
			{
				return _mergedLargeImageList;
			}
			return _largeImageList;
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

		protected override void Dispose(bool disposing)
		{
			if (_saveSettings && !base.DesignMode)
			{
				SaveComponentSettings();
			}
			if (disposing)
			{
				if (_components != null)
				{
					_components.Dispose();
				}
				if (_topRebar != null)
				{
					TopRebar.Dispose();
					_topRebar = null;
				}
				if (_leftRebar != null)
				{
					LeftRebar.Dispose();
					_leftRebar = null;
				}
				if (_rightRebar != null)
				{
					RightRebar.Dispose();
					_rightRebar = null;
				}
				if (_bottomRebar != null)
				{
					BottomRebar.Dispose();
					_bottomRebar = null;
				}
				_commands.DetachAll();
				for (int i = 0; i < _commands.Count; i++)
				{
					MxCommand uiCommand = _commands[i];
					uiCommand.ForceDispose();
				}
			}
			if (_containerControl != null)
			{
				_containerControl.Resize -= OnContainerControlResize;
				_containerControl.ParentChanged -= OnContainerControlVisibleChanged;
				_containerControl.Layout -= OnContainerControlLayout;
				_containerControl.SystemColorsChanged -= OnContainerControlSystemColorsChanged;
				_containerControl.HandleCreated -= OnContainerControlVisibleChanged;
				_containerControl = null;
				if (_form != null)
				{
					foreach (Control control in _form.Controls)
					{
						if (control is MdiClient)
						{
							MdiClientHook.RemoveMenuUser((MdiClient)control);
						}
					}
					_form.MdiChildActivate -= OnFormMdiChildActivate;
					_form.ControlAdded -= OnFormControlAdded;
					_form.ControlRemoved -= OnFormControlRemoved;
					_form.Activated -= OnFormActivated;
					_form = null;
					Application.RemoveMessageFilter(this);
				}
			}
			if (_contextMenusByComponent != null)
			{
				ArrayList arrayList = new ArrayList();
				foreach (DictionaryEntry item in _contextMenusByComponent)
				{
					arrayList.Add(item);
				}
				foreach (DictionaryEntry entry in arrayList)
				{
					SetContextMenu((Component)entry.Key, null);
				}
				_contextMenusByComponent = null;
			}
			int index;
			for (index = 0; index < ContextMenus.Count; index++)
			{
				MxContextMenu uiContextMenu = _contextMenus[index];
				uiContextMenu.Dispose();
				index--;
			}
			if (_builtInTexts != null)
			{
				_builtInTexts._commandManager = null;
			}
			if (_editContextMenu != null)
			{
				_editContextMenu._commandManager = null;
				_editContextMenu.CommandClick -= OnUIEditContextMenuCommandClick;
				_editContextMenu.Dispose();
				_editContextMenu = null;
			}
			base.Dispose(disposing);
			base.Events.Dispose();
		}

		private void BindChildBaseCommands(MxCommand uiCommand)
		{
			if (_isInitializing)
			{
				return;
			}
			if (uiCommand.CommandType == CommandType.ComboBoxCommand && _containerControl != null && _containerControl.IsHandleCreated)
			{
				uiCommand.GetUIComboBox().BindingContext = _containerControl.BindingContext;
			}
			foreach (MxCommand command in uiCommand.Commands)
			{
				if (command.BaseCommand != null)
				{
					if (command.BaseCommand.CommandManager == null)
					{
						Commands.Add(command.BaseCommand);
					}
				}
				else if (command.CommandType == CommandType.Separator)
				{
					command.SetBaseCommand(GetSeparatorCommand());
				}
				else if (command.Key.Length > 0)
				{
					MxCommand matchingCommand = Commands.FindByKey(command.Key);
					if (matchingCommand != null)
					{
						command.SetBaseCommand(matchingCommand);
					}
				}
				if (command.BaseCommand == uiCommand)
				{
					throw new InvalidOperationException("A command can not be child of its base command");
				}
				BindChildBaseCommands(command);
			}
		}

		internal bool IsCommandNameAvailable(MxCommand uiCommand, string text)
		{
			if (text.IndexOf(" ") != -1)
			{
				throw new ArgumentException("Identifier '" + text + "' is not valid");
			}
			if (text.IndexOf(".") != -1)
			{
				throw new ArgumentException("Identifier '" + text + "' is not valid");
			}
			if (text.IndexOf("-") != -1)
			{
				throw new ArgumentException("Identifier '" + text + "' is not valid");
			}
			if (!_commands.IsNameAvailable(uiCommand, text, true))
			{
				return false;
			}
			foreach (MxCommandBar item in _commandBars)
			{
				if (!item.Commands.IsNameAvailable(uiCommand, text, false))
				{
					return false;
				}
			}
			foreach (MxContextMenu contextMenu in _contextMenus)
			{
				if (!contextMenu.Commands.IsNameAvailable(uiCommand, text, false))
				{
					return false;
				}
			}
			return true;
		}

		bool IExtenderProvider.CanExtend(object target)
		{
			if (target is Control)
			{
				if (!(target is MxCommandBar) && !(target is MxCommandBarHost) && !(target is CommandBarEditorDialog))
				{
					return true;
				}
			}
			else if (target is NotifyIcon)
			{
				return true;
			}
			return false;
		}

		private ArrayList GetEditTargetControls()
		{
			if (EditContextMenu != null)
			{
				return EditContextMenu._editMenuControls;
			}
			return null;
		}

		public MxCommandManager(IContainer container)
		{
			InitializeMembers();
			SetContainer();
			container?.Add(this);
		}

		internal bool IsInitializing()
		{
			return _isInitializing;
		}

		internal bool IsEditorDialogOpen()
		{
			return _isEditorDialogOpen;
		}

		internal void SetEditorDialogOpen(bool isOpen)
		{
			_isEditorDialogOpen = isOpen;
		}

		public MxCommandManager()
		{
			InitializeMembers();
			SetContainer();
		}

		private void InitializeMembers()
		{
			if (_editContextMenu == null)
			{
				_editContextMenu = new EditContextMenu(this);
				_editContextMenu.CommandClick += OnUIEditContextMenuCommandClick;
			}
			_id = Guid.NewGuid();
			_layouts = new CommandBarsLayoutCollection(this);
			_commands = new MxCommandCollection(this);
			_commandBars = new CommandBarCollection(this);
			_contextMenus = new ContextMenuCollection(this);
			_categories = new CommandCategoryCollection(this);
			_commandBarsAppearanceState = new CommandBarsColorSet();
			_showShadowUnderMenus = true;
			RefreshAppearance();
		}

		private void OnControlComponentRename(object sender, ComponentRenameEventArgs eventArgs)
		{
			if (eventArgs.Component is MxCommand)
			{
				MxCommand uiCommand = (MxCommand)eventArgs.Component;
				if (!IsCommandNameAvailable(uiCommand, eventArgs.NewName))
				{
					throw new InvalidOperationException("There is already a component named '" + eventArgs.NewName + "'. Components must have unique names, and names must be case-insensitive.  A name also cannot conflict with the name of any component in an inherited class.");
				}
				if (uiCommand.CommandManager == this)
				{
					uiCommand.SyncNameFromSite();
				}
			}
		}

		public static string GetUIVersionString()
		{
			string localPath = typeof(MxCommandManager).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		private void SetContainer()
		{
			_components = new Container();
		}

		internal ICommandBarsElement GetActiveElement()
		{
			return _activeElement;
		}

		internal void SetActiveElement(ICommandBarsElement iCommandBarsElement)
		{
			if (_activeElement != iCommandBarsElement)
			{
				ICommandBarsElement previousElement = _activeElement;
				_activeElement = iCommandBarsElement;
				if (previousElement != null)
				{
					previousElement.SelectedCommand = null;
				}
			}
		}

		internal MxCommand GetSelectedCommand()
		{
			return _selectedCommand;
		}

		internal void SetSelectedCommand(MxCommand uiCommand)
		{
			if (_selectedCommand != uiCommand)
			{
				_selectedCommand = uiCommand;
				OnSelectedCommandChanged(EventArgs.Empty);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUsageData()
		{
			foreach (MxCommand command in Commands)
			{
				command.ResetUsageData();
			}
		}

		internal ImageList GetImageListForView(View view, MxCommand uiCommand)
		{
			if (view == View.SmallIcons)
			{
				if (uiCommand != null && uiCommand.IsMergedCommand && _mergedSmallImageList != null)
				{
					return _mergedSmallImageList;
				}
				return ImageList;
			}
			if (uiCommand != null && uiCommand.IsMergedCommand && _mergedLargeImageList != null)
			{
				return _mergedLargeImageList;
			}
			return LargeImageList;
		}

		internal bool IsCommandKeyUnique(MxCommand uiCommand, string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxCommand item in _commands)
			{
				if (item != uiCommand && string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0)
				{
					return false;
				}
			}
			return true;
		}

		internal bool IsContextMenuKeyUnique(MxContextMenu uiContextMenu, string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxContextMenu item in _contextMenus)
			{
				if (item != uiContextMenu && string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0)
				{
					return false;
				}
			}
			return true;
		}

		internal bool IsCommandBarKeyUnique(MxCommandBar uiCommandBar, string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxCommandBar item in _commandBars)
			{
				if (item != uiCommandBar && string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0)
				{
					return false;
				}
			}
			return true;
		}

		internal bool CanAddCommandBar(MxCommandBar commandBar)
		{
			if (_isInitializing)
			{
				return true;
			}
			CancelCommandBarEventArgs e = new CancelCommandBarEventArgs(commandBar);
			OnAddingCommandBar(e);
			return !e.Cancel;
		}

		internal bool CanRemoveCommandBar(MxCommandBar commandBar)
		{
			CancelCommandBarEventArgs e = new CancelCommandBarEventArgs(commandBar);
			OnRemovingCommandBar(e);
			return !e.Cancel;
		}
	}
}
