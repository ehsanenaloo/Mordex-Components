using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[Designer("Mordex.Windows.Ribbon.Design.RibbonDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(true)]
	[DefaultEvent("CommandClick")]
	public partial class MxRibbon : Control, ICommandContainer, ITabOwner, IMessageFilter, ICommandRenderHost, IRibbonHitTestSource, IMordexXmlLayoutsSupport, ISupportInitialize, IPersistComponentSettings
	{

		internal object _designer;

		private CommandCollection _quickAccessCommands;

		private CommandCollection _quickAccessCustomizeList;

		private ControlBoxMenu _controlBoxMenu;

		private RibbonTabCollection _tabs;

		private RibbonContextualTabCollection _contextualTabs;

		internal CommandCollection _commands;

		private List<RibbonContextualTab> _contextualTabList;

		private Size _smallImageSize;

		private Size _largeImageSize;

		private int _itemRows;

		private bool? _useCompatibleTextRendering;

		private int _layoutHeight;

		private ImageList _smallImageList;

		private ImageList _largeImageList;

		internal RibbonTabStripRenderer _tabStripRenderer;

		internal RibbonTabContentRenderer _tabContentRenderer;

		internal QuickAccessToolbarRenderer _quickAccessToolbarRenderer;

		internal bool _isTabStripLayoutDirty;

		private bool _isTabContentDirty;

		private bool _showToolTips;

		private int _tabMinWidth;

		private RibbonTab _selectedTab;

		internal RibbonColorScheme _colorScheme;

		private int _smallItemHeight;

		internal int _tabStripRowHeight;

		private Rectangle _contentAreaBounds;

		private Form _form;

		internal RibbonWindowMessageHandler _ribbonWindowMessageHandler;

		private CommandBaseEditorDialog _popupMenu;

		private BackstageMenuScrollableBase _backstageMenuScrollableBase;

		private int _captionAreaWidth;

		private int _superTipWidth;

		private CommandBase _superTipCommand;

		private bool _showShortcutInToolTips;

		private bool _showShortcutInMenus;

		internal MxSuperTip _mordexSuperTip;

		private CommandBaseRenderer _hoveredRenderer;

		private CommandBaseRenderer _pressedRenderer;

		private IList<RibbonKeyTipEntry> _keyTips;

		internal int _groupLabelHeight;

		private bool _isKeyTipMode;

		private CommandBaseRenderer _keyboardFocusRenderer;

		private string _keyTipInput;

		private bool _isOpeningKeyTipDropDown;

		private DropDownCommand _dropDownCommand;

		private object _keyTipOwner;

		private System.Windows.Forms.Timer _contentSlideTimer;

		private int _contentAreaWidth;

		private bool _allowMerge;

		private MxRibbon _mergedRibbon;

		internal MxRibbon _mergeHost;

		private ImageList _ribbonImageList;

		private ImageList _ribbonLargeImageList;

		private bool _visibleBeforeMerge;

		private MxRibbon _mergeSourceRibbon;

		internal RibbonHeaderRenderer _headerRenderer;

		internal Rectangle _controlBoxButtonBounds;

		private bool _isMdiChildMaximized;

		private Form _activeMdiChild;

		private bool _showCustomizeButton;

		private BuiltInTextList _builtInTexts;

		private RibbonLayout _currentLayout;

		private RibbonLayout _appliedLayout;

		private RibbonLayoutCollection _layouts;

		private bool _isInitializing;

		private QuickAccessPosition _quickAccessPosition;

		private Stream _layoutStream;

		private bool _minimized;

		private bool _isMinimizedPopupOpen;

		internal ControlBoxButtonRenderer _controlBoxButtonRenderer;

		internal MxRibbonContextMenu _ribbonContextMenu;

		internal int _captionHeight;

		internal bool _isGlassAvailable;

		private bool _showCaptionBar;

		private bool _showControlBoxButton;

		private Size _frameBorderSize;

		private bool _isParentedToForm;

		private bool _showAddNewItems;

		internal bool _designerItemPressed;

		internal bool _isQuickAccessItemMouseUp;

		private bool _isAdjustingVisibility;

		private bool _requestedVisible;

		private Size _minimumFormSize;

		private Color _foreColorLight;

		private Color _foreColorDark;

		internal RibbonTabRenderer _activeTabRenderer;

		internal bool _wasSelectedTabClicked;

		private bool _handlesTitleBar;

		private string _documentName;

		private string _applicationName;

		private Font _titleBarFont;

		private System.Windows.Forms.Timer _keyTipTimer;

		private bool _keyTipsShownByAltHold;

		private bool _keyTipsShownByF10;

		private BufferedGraphics _backBuffer;

		private BufferedGraphicsContext _bufferedGraphicsContext;

		private bool _isBackgroundBuffered;

		private int _superTipDelay;

		private int _tabStripHeight;

		private bool _showQuickCustomizeMenu;

		private ControlBoxDoubleClickAction _controlBoxDoubleClickAction;

		private static bool s_isKeyTipSessionActive;

		private Icon _controlBoxIcon;

		private BackstageMenu _backstageMenu;

		private ControlBoxMode _controlBoxMode;

		private ButtonCommand _helpButton;

		private bool _allowMinimize;

		private ApplicationSettings _mordexApplicationSettings;

		private bool _saveSettings;

		private string _settingsKey;

		private bool _formAutoScroll;

		private bool _checkFirstKeyTipChar;

		private bool _minimizeOnDoubleClick = true;

		internal bool _isDesignerLoaded;

		private bool _enableGlassEffect = true;

		private int _firstTabOffset = 48;

		private bool _hasInternalCaptionRow;

		private bool _isPainting;

		private static readonly object s_commandDropDownClosingKey = new object();

		private static readonly object s_commandDropDownClosedKey = new object();

		private static readonly object s_commandDropDownOpeningKey = new object();

		private static readonly object s_commandDropDownOpenedKey = new object();

		private static readonly object s_commandClickKey = new object();

		private static readonly object s_quickAccessCommandAddedKey = new object();

		private static readonly object s_quickAccessCommandRemovedKey = new object();

		private static readonly object s_customizeCommandClickKey = new object();

		private static readonly object s_layoutLoadKey = new object();

		private static readonly object s_currentLayoutChangedKey = new object();

		private static readonly object s_currentLayoutChangingKey = new object();

		private static readonly object s_selectedTabChangedKey = new object();

		private static readonly object s_changingSelectedTabKey = new object();

		private static readonly object s_minimizedChangedKey = new object();

		private static readonly object s_controlBoxOpeningKey = new object();

		private static readonly object s_controlBoxOpenedKey = new object();

		private static readonly object s_controlBoxClosingKey = new object();

		private static readonly object s_controlBoxClosedKey = new object();

		private static readonly object s_backstageClosingKey = new object();

		private static readonly object s_backstageClosedKey = new object();

		private static readonly object s_backstageOpeningKey = new object();

		private static readonly object s_backstageOpenedKey = new object();

		private EventHandler _contentRenderedHandler;

		private EventHandler _mergeChangingHandler;

		private EventHandler _mergeChangedHandler;

		private EventHandler _ribbonMergedHandler;

		private EventHandler _ribbonUnmergedHandler;

		private bool _isAutoMerged = true;

		internal bool _isMergeInProgress;

		private bool _isTabStripHidden;

		private IContainer _components;

		internal bool _isDisposing;

		internal ImageList _designerImageList;

		public MxRibbon()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			InitializeDesignerImageList();
			InitializeDefaults();
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new MxRibbonAccessibleObject(this);
		}

		private void InitializeDefaults()
		{
			_isGlassAvailable = IsGlassAvailable();
			_allowMinimize = true;
			_mordexSuperTip = new MxSuperTip { UseDefaultTheme = true };
			_mordexSuperTip.InitialDelay = 900;
			_mordexSuperTip.Font = Font;
			_mordexSuperTip.ToolTipPopUp += OnMordexSuperTipToolTipPopUp;
			_applicationName = "";
			_documentName = "";
			_minimumFormSize = Size.Empty;
			_requestedVisible = true;
			_builtInTexts = new BuiltInTextList(this);
			_superTipDelay = 9000;
			_quickAccessCommands = new CommandCollection(this, isQuickAccessCollection: true);
			_quickAccessCustomizeList = new CommandCollection(this, isQuickAccessCollection: true);
			_controlBoxMenu = new ControlBoxMenu(this);
			_backstageMenu = new BackstageMenu(this);
			_commands = new CommandCollection(_backstageMenu, isQuickAccessCollection: false);
			_helpButton = new ButtonCommand("HelpButton", "");
			_helpButton.SetParentContainer(this);
			_tabs = new RibbonTabCollection(this);
			_contextualTabs = new RibbonContextualTabCollection(this);
			_layouts = new RibbonLayoutCollection(this);
			RebuildColorScheme();
			_helpButton.Image = RibbonDrawingUtils.MakeMagentaTransparent(RibbonImageResources.GetHelp2010Bitmap());
			_helpButton.Visible = false;
			_helpButton._defaultVisible = false;
			_itemRows = 3;
			_smallImageSize = new Size(16, 16);
			_largeImageSize = new Size(32, 32);
			_frameBorderSize = new Size(4, 4);
			_isTabStripLayoutDirty = true;
			_isTabContentDirty = true;
			_tabStripRenderer = new RibbonTabStripRenderer(this);
			_tabContentRenderer = new RibbonTabContentRenderer(this, this);
			_headerRenderer = new RibbonHeaderRenderer(this);
			_contextualTabList = new List<RibbonContextualTab>();
			_handlesTitleBar = true;
			_captionHeight = 0;
			_tabMinWidth = 34;
			_tabStripHeight = -1;
			_allowMerge = true;
			_superTipWidth = 210;
			_showToolTips = true;
			_showCustomizeButton = true;
			_showCaptionBar = true;
			_showControlBoxButton = true;
			_showQuickCustomizeMenu = true;
			_controlBoxDoubleClickAction = ControlBoxDoubleClickAction.CloseForm;
			RelayoutForFormState();
		}

		private void DisposeResources()
		{
			try
			{
				_keyTipOwner = this;
				_keyTips = new List<RibbonKeyTipEntry>();
				using Graphics graphics = CreateGraphics();
				using TextLayoutRenderer textRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				if (_tabStripRenderer != null && _tabStripRenderer.GetTabRenderers() != null)
				{
					foreach (RibbonTabRenderer tabRenderer in _tabStripRenderer.GetTabRenderers())
					{
						if (tabRenderer is BackstageMenuTabRenderer)
						{
							BackstageMenuTabRenderer backstageTabRenderer = (BackstageMenuTabRenderer)tabRenderer;
							if (backstageTabRenderer.GetBackstageMenu() != null && backstageTabRenderer.GetBackstageMenu().KeyTip != null && backstageTabRenderer.GetBackstageMenu().KeyTip.Length > 0)
							{
								SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(Font, UseCompatibleTextRendering, backstageTabRenderer.GetBackstageMenu().KeyTip, _colorScheme);
								superTipSettingsWindowMessageHandler.SetSticky(true);
								superTipSettingsWindowMessageHandler.SetCompactMode(true);
								superTipSettingsWindowMessageHandler.ShowAt(this, PointToScreen(tabRenderer.GetKeyTipLocation(graphics, textRenderer, Font, backstageTabRenderer.GetBackstageMenu().KeyTip)));
								RibbonKeyTipEntry item = new RibbonKeyTipEntry(backstageTabRenderer.GetBackstageMenu().KeyTip, backstageTabRenderer.GetBackstageMenu(), tabRenderer, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
								_keyTips.Add(item);
							}
						}
						else if (tabRenderer.GetTab() != null && tabRenderer.GetTab().KeyTip != null && tabRenderer.GetTab().KeyTip.Length > 0)
						{
							SuperTipSettingsWindowMessageHandler tabKeyTipWindow = new SuperTipSettingsWindowMessageHandler(Font, UseCompatibleTextRendering, tabRenderer.GetTab().KeyTip, _colorScheme);
							tabKeyTipWindow.SetSticky(true);
							tabKeyTipWindow.SetCompactMode(true);
							tabKeyTipWindow._isKeyTipEnabled = tabRenderer.GetTab().IsEffectivelyEnabled();
							tabKeyTipWindow.ShowAt(this, PointToScreen(tabRenderer.GetKeyTipLocation(graphics, textRenderer, Font, tabRenderer.GetTab().KeyTip)));
							RibbonKeyTipEntry tabKeyTipEntry = new RibbonKeyTipEntry(tabRenderer.GetTab().KeyTip, tabRenderer.GetTab(), tabRenderer, tabKeyTipWindow, tabKeyTipWindow._isKeyTipEnabled);
							_keyTips.Add(tabKeyTipEntry);
						}
					}
				}
				if (_controlBoxButtonRenderer != null && ControlBoxMenu.KeyTip != null && ControlBoxMenu.KeyTip.Length > 0)
				{
					SuperTipSettingsWindowMessageHandler controlBoxKeyTipWindow = new SuperTipSettingsWindowMessageHandler(Font, UseCompatibleTextRendering, ControlBoxMenu.KeyTip, _colorScheme);
					controlBoxKeyTipWindow.SetSticky(true);
					controlBoxKeyTipWindow.SetCompactMode(true);
					controlBoxKeyTipWindow.ShowAt(this, PointToScreen(_controlBoxButtonRenderer.GetKeyTipLocation(graphics, textRenderer, Font, ControlBoxMenu.KeyTip)));
					RibbonKeyTipEntry controlBoxKeyTipEntry = new RibbonKeyTipEntry(ControlBoxMenu.KeyTip, null, _controlBoxButtonRenderer, controlBoxKeyTipWindow, controlBoxKeyTipWindow._isKeyTipEnabled);
					_keyTips.Add(controlBoxKeyTipEntry);
				}
				if (_quickAccessToolbarRenderer != null)
				{
					((IKeyboardNavigable)_quickAccessToolbarRenderer).AddKeyTips(graphics, textRenderer, Font, _keyTips, showToolTip: true);
				}
			}
			catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.Runtime.InteropServices.ExternalException)
			{
				// Key tips are optional; drawing errors leave the ones collected so far.
			}
		}

		private void StartKeyTipTimer()
		{
			if (_keyTipTimer == null)
			{
				_keyTipTimer = new System.Windows.Forms.Timer();
				_keyTipTimer.Interval = 1000;
				_keyTipTimer.Tick += OnTimerTick;
				_keyTipTimer.Start();
			}
		}

		private void StopContentSlideTimer()
		{
			if (_contentSlideTimer != null)
			{
				_contentSlideTimer.Stop();
				_contentSlideTimer.Tick -= OnContentSlideTimerTick;
				_contentSlideTimer.Dispose();
				_contentSlideTimer = null;
			}
		}

		private void OnMergedRibbonChanging(object sender, EventArgs eventArgs)
		{
			_mergeSourceRibbon = _mergedRibbon;
			SetMergedRibbon(null);
			_mergeSourceRibbon.AddMergeChangedHandler(HandleSourceMergeChanged);
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				_isDisposing = true;
				base.Dispose(disposing);
				return;
			}
			SetKeyTipMode(false);
			// Merged content belongs to the other ribbon, so the merge is undone before anything is disposed.
			if (_mergedRibbon != null)
			{
				try
				{
					SetMergedRibbon(null);
				}
				catch (InvalidOperationException)
				{
					// The merged ribbon is already being disposed.
				}
			}
			if (_mergeHost != null && !_mergeHost._isDisposing)
			{
				try
				{
					_mergeHost.SetMergedRibbon(null);
				}
				catch (InvalidOperationException)
				{
					// The owner is already being disposed.
				}
			}
			_tabContentRenderer?.Dispose();
			_tabStripRenderer?.Dispose();
			try
			{
				if (_saveSettings && !base.DesignMode)
				{
					SaveComponentSettings();
				}
			}
			catch (Exception ex) when (ex is ConfigurationException || ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException || ex is InvalidOperationException)
			{
				// Settings that cannot be saved must not stop the control from closing.
			}
			_isDisposing = true;
			if (_components != null)
			{
				_components.Dispose();
			}
			if (_mordexSuperTip != null)
			{
				_mordexSuperTip.ToolTipPopUp -= OnMordexSuperTipToolTipPopUp;
				_mordexSuperTip.Dispose();
				_mordexSuperTip = null;
			}
			StopKeyTipTimer();
			StopContentSlideTimer();
			if (_ribbonContextMenu != null)
			{
				_ribbonContextMenu.CommandClick -= OnRibbonContextMenuCommandClick;
				_ribbonContextMenu.Dispose();
			}
			ClearSuperTipCommand();
			DisposeCommands(QuickAccessCommands);
			DisposeCommands(_quickAccessCustomizeList);
			List<RibbonTab> tabsToDispose = new List<RibbonTab>();
			foreach (RibbonTab tab in Tabs)
			{
				tabsToDispose.Add(tab);
			}
			foreach (RibbonTab tab in tabsToDispose)
			{
				tab.SetTabOwner(null);
				tab.Dispose();
			}
			List<RibbonContextualTab> contextualTabsToDispose = new List<RibbonContextualTab>();
			foreach (RibbonContextualTab contextualTab in ContextualTabs)
			{
				contextualTabsToDispose.Add(contextualTab);
			}
			foreach (RibbonContextualTab contextualTab in contextualTabsToDispose)
			{
				contextualTab._ribbon = null;
				contextualTab.Dispose();
			}
			if (_backstageMenu != null)
			{
				_backstageMenu.Dispose();
			}
			if (_helpButton != null)
			{
				_helpButton.Dispose();
			}
			if (GetHostForm() != null)
			{
				AttachToForm(null);
			}
			if (_activeMdiChild != null)
			{
				_activeMdiChild.SizeChanged -= OnFormSizeChanged;
				_activeMdiChild = null;
			}
			if (_popupMenu != null)
			{
				_popupMenu.RemoveDropDownHandler(OnPopupDropDownCommand);
				_popupMenu.Dispose();
				_popupMenu = null;
			}
			_tabContentRenderer = null;
			if (_backBuffer != null)
			{
				_backBuffer.Dispose();
				_backBuffer = null;
			}
			if (_bufferedGraphicsContext != null)
			{
				_bufferedGraphicsContext.Dispose();
				_bufferedGraphicsContext = null;
			}
			_headerRenderer?.ResetCaptionFont();
			base.Dispose(disposing);
		}

		private static void DisposeCommands(CommandCollection commands)
		{
			List<CommandBase> snapshot = new List<CommandBase>();
			foreach (CommandBase command in commands)
			{
				snapshot.Add(command);
			}
			foreach (CommandBase command in snapshot)
			{
				command.SetParentContainer(null);
				command.CommandsOwner = null;
				command.Dispose();
			}
		}
	}
}
