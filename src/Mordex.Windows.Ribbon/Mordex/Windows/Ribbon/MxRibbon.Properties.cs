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
		[Description("Indicates whether the control should automatically persist its application settings properties.")]
		[DefaultValue(false)]
		public bool SaveSettings
		{
			get
			{
				return _saveSettings;
			}
			set
			{
				_saveSettings = value;
			}
		}

		[Description("Gets or sets the value of the application settings key for the current instance of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string SettingsKey
		{
			get
			{
				if ((_settingsKey == null || _settingsKey.Length == 0) && Site != null && Site.Name != null && Site.Name.Length > 0)
				{
					return Site.Name;
				}
				if (_settingsKey == null)
				{
					return "";
				}
				return _settingsKey;
			}
			set
			{
				_settingsKey = value;
			}
		}

		[Localizable(true)]
		[Description("Specifies whether the Ribbon will merge its commands with the commands form a Ribbon in a MDI Parent or MDI Child form.")]
		[DefaultValue(true)]
		public bool AllowMerge
		{
			get
			{
				return _allowMerge;
			}
			set
			{
				_allowMerge = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public BuiltInTextList BuiltInTexts
		{
			get
			{
				if (_builtInTexts == null)
				{
					_builtInTexts = new BuiltInTextList(this);
				}
				return _builtInTexts;
			}
		}

		[Browsable(false)]
		[Localizable(true)]
		public string BuiltInTextsData
		{
			get
			{
				return BuiltInTexts.GetCustomTextsXml();
			}
			set
			{
				BuiltInTexts.LoadCustomTextsXml(value);
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[Description("Represents the Color used to paint the foreground of the dark parts in the Ribbon")]
		public Color ForeColorLight
		{
			get
			{
				return _foreColorLight;
			}
			set
			{
				if (!_foreColorLight.Equals(value))
				{
					_foreColorLight = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the Color used to paint the foreground of the light parts in the Ribbon")]
		public Color ForeColorDark
		{
			get
			{
				return _foreColorDark;
			}
			set
			{
				if (!_foreColorDark.Equals(value))
				{
					_foreColorDark = value;
					Invalidate();
				}
			}
		}

		[Description("Inidcates whether the customize button will be displayed in the quick access bar.")]
		[DefaultValue(true)]
		public bool ShowCustomizeButton
		{
			get
			{
				return _showCustomizeButton;
			}
			set
			{
				if (_showCustomizeButton == value)
				{
					return;
				}
				_showCustomizeButton = value;
				if (_quickAccessPosition == QuickAccessPosition.AboveRibbon)
				{
					if (_headerRenderer != null)
					{
						_headerRenderer.RequestLayoutAndRepaint();
						_headerRenderer.RefreshHeader(true);
					}
				}
				else
				{
					InvalidateTabContent();
				}
				if (_quickAccessToolbarRenderer != null)
				{
					_quickAccessToolbarRenderer.MarkItemsDirty();
				}
			}
		}

		[DefaultValue(ControlBoxDoubleClickAction.CloseForm)]
		[Description("Specifies the default action after double clicking the control box button.")]
		public ControlBoxDoubleClickAction ControlBoxDoubleClickAction
		{
			get
			{
				return _controlBoxDoubleClickAction;
			}
			set
			{
				_controlBoxDoubleClickAction = value;
			}
		}

		[DefaultValue(9000)]
		[Description("Represents the period of time the ToolTip remains visible if the pointer is stationary on a control with specified ToolTip text.")]
		public int SuperTipDelay
		{
			get
			{
				return _superTipDelay;
			}
			set
			{
				_superTipDelay = value;
			}
		}

		[DefaultValue("File")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(DockStyle.Top)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = value;
			}
		}

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				if (base.Site == value)
				{
					return;
				}
				if (base.Site != null)
				{
					IComponentChangeService componentChangeService = (IComponentChangeService)base.Site.GetService(typeof(IComponentChangeService));
					if (componentChangeService != null)
					{
						componentChangeService.ComponentRename -= OnControlComponentRename;
					}
				}
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
				if (value != null)
				{
					IComponentChangeService newChangeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
					if (newChangeService != null)
					{
						newChangeService.ComponentRename += OnControlComponentRename;
					}
				}
			}
		}

		[Description("Indicates whether a ToolTip is shown when the mouse passes over a command.")]
		[DefaultValue(true)]
		public bool ShowToolTips
		{
			get
			{
				return _showToolTips;
			}
			set
			{
				_showToolTips = value;
			}
		}

		[Description("Indicates whether the Ribbon is minimized after double clicking on a tab")]
		[DefaultValue(true)]
		public bool MinimizeOnDoubleClick
		{
			get
			{
				return _minimizeOnDoubleClick;
			}
			set
			{
				_minimizeOnDoubleClick = value;
			}
		}

		[Description("Represents the Text of the Form.")]
		[Localizable(true)]
		public string DocumentName
		{
			get
			{
				if (_documentName.Length == 0 && _form != null)
				{
					return _form.Text;
				}
				return _documentName;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (!(_documentName != value))
				{
					return;
				}
				_documentName = value;
				if (_ribbonWindowMessageHandler != null)
				{
					if (_headerRenderer != null)
					{
						_headerRenderer.RequestLayoutAndRepaint();
					}
					_ribbonWindowMessageHandler.RefreshCaptionLayout();
					_ribbonWindowMessageHandler.Refresh(update: false);
				}
			}
		}

		[DefaultValue("")]
		[Description("Represents the name of the application that will be displayed in the title bar of Form.")]
		[Localizable(true)]
		public string ApplicationName
		{
			get
			{
				return _applicationName;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (!(_applicationName != value))
				{
					return;
				}
				_applicationName = value;
				if (_ribbonWindowMessageHandler != null)
				{
					if (_headerRenderer != null)
					{
						_headerRenderer.RequestLayoutAndRepaint();
					}
					_ribbonWindowMessageHandler.RefreshCaptionLayout();
					_ribbonWindowMessageHandler.Refresh(update: false);
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represents the Font used to draw the text in the title bar of the Form.")]
		public Font TitleBarFont
		{
			get
			{
				return _titleBarFont;
			}
			set
			{
				if (_titleBarFont != value)
				{
					_titleBarFont = value;
					_headerRenderer?.ResetCaptionFont();
					if (_ribbonWindowMessageHandler != null)
					{
						_ribbonWindowMessageHandler.RefreshCaptionLayout();
						Invalidate();
					}
				}
			}
		}

		[Description("Specifies whether the Ribbon can paint its contents in the Title bar of the Form.")]
		[DefaultValue(true)]
		public bool HandlesTitleBar
		{
			get
			{
				return _handlesTitleBar;
			}
			set
			{
				if (_handlesTitleBar == value)
				{
					return;
				}
				_handlesTitleBar = value;
				if (!value)
				{
					if (_ribbonWindowMessageHandler != null)
					{
						if (_ribbonWindowMessageHandler is RibbonGlassCaptionMessageHandler)
						{
							((RibbonGlassCaptionMessageHandler)_ribbonWindowMessageHandler).ResetGlassMargins();
						}
						if (_form != null && _form.IsHandleCreated)
						{
							NativeMethods.RemovePropW(GetHostForm().Handle, "Mordex_Ribbon");
						}
						_ribbonWindowMessageHandler.Dispose();
						_ribbonWindowMessageHandler = null;
					}
					RelayoutForFormState();
					SyncHeaderFormHandle();
					Rectangle rectangle = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width, _captionHeight);
					_headerRenderer.SetArea(rectangle);
					if (_form != null)
					{
						_form.Width--;
						_form.Width++;
						if (!_form.IsDisposed && !_form.Disposing)
						{
							NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged);
							NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, RdwInvalidateFrame);
							Invalidate();
						}
					}
				}
				else
				{
					CreateCaptionHandler(true, false);
					RelayoutForFormState();
					if (_form != null && _form.IsHandleCreated)
					{
						NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChanged);
					}
				}
				InvalidateFullLayout();
				RefreshWindowFrame(true);
			}
		}

		[DefaultValue(true)]
		[Description("Specifies whether a bar at the top of the Ribbon is added to display the ContextualTabs")]
		public bool ShowCaptionBar
		{
			get
			{
				return _showCaptionBar;
			}
			set
			{
				if (_showCaptionBar != value)
				{
					_showCaptionBar = value;
					RefreshInternalCaptionRow();
					RelayoutForFormState();
					ReconcileCaptionExtension(true);
					InvalidateFullLayout();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Specifies whether the Ribbon can be minimized")]
		public bool AllowMinimize
		{
			get
			{
				return _allowMinimize;
			}
			set
			{
				if (_allowMinimize != value)
				{
					_allowMinimize = value;
					RequestTabStripLayout(false);
				}
			}
		}

		[DefaultValue(true)]
		[Description("Specifies whether the ControlBox's button should be displayed.")]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("'ShowControlBoxButton' is obsolete: This property has been deprecated. Use ControlBoxMode property instead.")]
		public bool ShowControlBoxButton
		{
			get
			{
				return _showControlBoxButton;
			}
			set
			{
				if (_showControlBoxButton != value)
				{
					_showControlBoxButton = value;
					if (_showControlBoxButton)
					{
						ControlBoxMode = ControlBoxMode.ControlBoxButton;
					}
					else
					{
						ControlBoxMode = ControlBoxMode.None;
					}
				}
			}
		}

		[Description("Specifies what button should be displayed to show the main menu.")]
		[DefaultValue(ControlBoxMode.Default)]
		public ControlBoxMode ControlBoxMode
		{
			get
			{
				return _controlBoxMode;
			}
			set
			{
				if (_controlBoxMode != value)
				{
					_controlBoxMode = value;
					if (_headerRenderer != null)
					{
						_headerRenderer.RequestLayoutAndRepaint();
					}
					RequestTabStripLayout(true);
					RefreshWindowFrame(true);
					Invalidate();
				}
			}
		}

		[DefaultValue(3)]
		[Description("Specifies the number of small commands that can be placed vertically in the ribbon.")]
		public int ItemRows
		{
			get
			{
				return _itemRows;
			}
			set
			{
				if (_itemRows != value)
				{
					if (value < 1)
					{
						throw new ArgumentException("MaxItemRows must be equal or bigger than 1");
					}
					_itemRows = value;
					RelayoutForFormState();
				}
			}
		}

		[Description("Represents the width of the supertip for the commands")]
		[DefaultValue(210)]
		public int SuperTipWidth
		{
			get
			{
				return _superTipWidth;
			}
			set
			{
				_superTipWidth = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandCollection QuickAccessCommands => _quickAccessCommands;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandCollection QuickAccessCustomizeList => _quickAccessCustomizeList;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlBoxMenu ControlBoxMenu => _controlBoxMenu;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CommandCollection BackstageCommands => _commands;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Localizable(false)]
		public BackstageMenu BackstageMenu => _backstageMenu;

		[Browsable(false)]
		[Localizable(true)]
		public string BackstageMenuData
		{
			get
			{
				return _backstageMenu.ToString();
			}
			set
			{
				try
				{
					LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
					PropertyValue propertyValue = mordexLayoutReader["BackstageMenu"];
					if (propertyValue != null)
					{
						_backstageMenu._deserializeCommands = false;
						propertyValue.DeserializeObject(_backstageMenu);
					}
				}
				catch (Exception ex) when (ex is System.Xml.XmlException || ex is FormatException || ex is InvalidCastException || ex is ArgumentException || ex is InvalidOperationException)
				{
					// A bad layout string leaves the menu as it is.
				}
				finally
				{
					_backstageMenu._deserializeCommands = true;
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ButtonCommand HelpButton => _helpButton;

		[DefaultValue(null)]
		[Description("Represents the Icon displayed in the ControlBox button.")]
		public Icon ControlBoxIcon
		{
			get
			{
				return _controlBoxIcon;
			}
			set
			{
				if (_controlBoxIcon != value)
				{
					_controlBoxIcon = value;
					if (_headerRenderer != null)
					{
						_headerRenderer.RequestLayoutAndRepaint();
					}
					RefreshWindowFrame(true);
					Invalidate();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RibbonContextualTabCollection ContextualTabs => _contextualTabs;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public RibbonTabCollection Tabs => _tabs;

		[Localizable(true)]
		[Description("Determines the size that images will have when drawing a command in small icons view.")]
		public Size SmallImageSize
		{
			get
			{
				return _smallImageSize;
			}
			set
			{
				if (_smallImageSize != value)
				{
					_smallImageSize = value;
					RelayoutForFormState();
					InvalidateTabContent();
				}
			}
		}

		[Description("Determines the size that images will have when drawing a command in large icons view.")]
		[Localizable(true)]
		public Size LargeImageSize
		{
			get
			{
				return _largeImageSize;
			}
			set
			{
				if (_largeImageSize != value)
				{
					_largeImageSize = value;
					InvalidateTabContent();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The ImageList to use when displaying commands as small icons.")]
		public ImageList ImageList
		{
			get
			{
				return _smallImageList;
			}
			set
			{
				if (_smallImageList != value)
				{
					_smallImageList = value;
					if (_mordexSuperTip != null && _mordexSuperTip.ImageList == null)
					{
						_mordexSuperTip.ImageList = _smallImageList;
					}
					InvalidateTabContent();
				}
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the Ribbon should display its full contents or only the tab strip.")]
		public bool Minimized
		{
			get
			{
				return _minimized;
			}
			set
			{
				if (_minimized != value)
				{
					_minimized = value;
					RelayoutForFormState();
					InvalidateFullLayout();
					OnMinimizedChanged(EventArgs.Empty);
				}
			}
		}

		[Description("Specifies the location of the quick access bar in the Ribbon.")]
		[DefaultValue(QuickAccessPosition.AboveRibbon)]
		public QuickAccessPosition QuickAccessPosition
		{
			get
			{
				return _quickAccessPosition;
			}
			set
			{
				if (_quickAccessPosition != value)
				{
					_quickAccessPosition = value;
					_quickAccessToolbarRenderer = null;
					RelayoutForFormState();
					if (_headerRenderer != null)
					{
						_headerRenderer.RequestLayoutAndRepaint();
						_headerRenderer.RefreshHeader(false);
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CommandBase FocusedCommand
		{
			get
			{
				if (_popupMenu != null && _popupMenu.FocusItem != null)
				{
					return _popupMenu.FocusItem.GetCommand();
				}
				if (_tabContentRenderer != null && _tabContentRenderer.GetHotGroup() != null && _tabContentRenderer.GetHotGroup().GetHotRenderer() != null)
				{
					CommandBaseRenderer commandBaseRenderer = _tabContentRenderer.GetHotGroup().GetHotRenderer();
					while (commandBaseRenderer is CommandContainerRenderer)
					{
						CommandContainerRenderer ribbonState = commandBaseRenderer as CommandContainerRenderer;
						if (ribbonState.GetHotRenderer() != null)
						{
							commandBaseRenderer = ribbonState.GetHotRenderer();
						}
					}
					return commandBaseRenderer?.GetCommand();
				}
				if (_quickAccessToolbarRenderer != null && _quickAccessToolbarRenderer.GetHoveredItem() != null)
				{
					return _quickAccessToolbarRenderer.GetHoveredItem().GetCommand();
				}
				if (_hoveredRenderer != null)
				{
					return _hoveredRenderer.GetCommand();
				}
				return null;
			}
		}

		[Description("Specifies whether the glass effect for the Form's caption is enabled when the application is running under Windows Vista.")]
		[DefaultValue(true)]
		public bool EnableGlassEffect
		{
			get
			{
				return _enableGlassEffect;
			}
			set
			{
				if (_enableGlassEffect == value)
				{
					return;
				}
				_enableGlassEffect = value;
				if (_ribbonWindowMessageHandler != null && !base.DesignMode)
				{
					throw new InvalidOperationException("This property can only be set before the Ribbon is added to the Form.");
				}
				bool addCaptionOffset = true;
				if (_ribbonWindowMessageHandler != null)
				{
					if (_form != null && _form.IsHandleCreated)
					{
						NativeMethods.RemovePropW(GetHostForm().Handle, "Mordex_Ribbon");
					}
					_ribbonWindowMessageHandler.Dispose();
					_ribbonWindowMessageHandler = null;
					addCaptionOffset = false;
				}
				if (_form != null)
				{
					CreateCaptionHandler(addCaptionOffset, false);
				}
				if (_headerRenderer != null)
				{
					_headerRenderer.RequestLayoutAndRepaint();
				}
				RelayoutForFormState();
				InvalidateFullLayout();
				InvalidateWindowFrame();
			}
		}

		[Description("The ImageList to use when displaying commands as large icons.")]
		[DefaultValue(null)]
		public ImageList LargeImageList
		{
			get
			{
				return _largeImageList;
			}
			set
			{
				if (_largeImageList != value)
				{
					_largeImageList = value;
				}
			}
		}

		[Description("Specifies whether the text rendering should be compatible with previous release of Windows Forms.")]
		public bool UseCompatibleTextRendering
		{
			get
			{
				if (!_useCompatibleTextRendering.HasValue)
				{
					return TextRenderingOptions.DefaultUseCompatibleTextRendering;
				}
				return _useCompatibleTextRendering.Value;
			}
			set
			{
				if (_useCompatibleTextRendering != value)
				{
					_useCompatibleTextRendering = value;
					RelayoutForFormState();
					InvalidateFullLayout();
				}
			}
		}

		[DefaultValue(34)]
		[Description("Specifies the minimum width of the tab pages.")]
		public int TabMinWidth
		{
			get
			{
				return _tabMinWidth;
			}
			set
			{
				if (_tabMinWidth != value)
				{
					_tabMinWidth = value;
				}
			}
		}

		[Description("Represents the distance in pixels from the left border to the first tab.")]
		[DefaultValue(48)]
		public int FirstTabOffset
		{
			get
			{
				return _firstTabOffset;
			}
			set
			{
				if (_firstTabOffset != value)
				{
					_firstTabOffset = value;
					InvalidateFullLayout();
					InvalidateTabContent();
					RequestTabStripLayout(false);
					RefreshWindowFrame(true);
				}
			}
		}

		[Description("Specifies the height of the tab strip")]
		[DefaultValue(-1)]
		public int TabStripHeight
		{
			get
			{
				return _tabStripHeight;
			}
			set
			{
				if (_tabStripHeight != value)
				{
					_tabStripHeight = value;
					RelayoutForFormState();
					InvalidateFullLayout();
					InvalidateTabContent();
					RequestTabStripLayout(false);
					RefreshWindowFrame(true);
					if (_ribbonWindowMessageHandler != null)
					{
						_ribbonWindowMessageHandler.RedrawFrame();
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		[Browsable(false)]
		public RibbonTab SelectedTab
		{
			get
			{
				return _selectedTab;
			}
			set
			{
				if (_selectedTab == value)
				{
					return;
				}
				if (value is BackstageMenu)
				{
					ShowBackstageMenu();
					return;
				}
				if (value == null && HasVisibleTab())
				{
					throw new ArgumentNullException();
				}
				TabCancelEventArgs e = new TabCancelEventArgs(_selectedTab, value);
				OnChangingSelectedTab(e);
				if (e.Cancel && _selectedTab != null && value != null)
				{
					return;
				}
				RibbonTab previousTab = _selectedTab;
				if (_selectedTab == null || value == null)
				{
					if (_selectedTab != null)
					{
						_selectedTab.SetMarkedSelected(false);
					}
					_selectedTab = value;
					if (_tabs.Count > 0 || (!_isTabStripHidden && _tabs.Count == 0 && _selectedTab != null))
					{
						RelayoutForFormState();
					}
				}
				if (_selectedTab != null)
				{
					_selectedTab.SetMarkedSelected(false);
				}
				TabEventArgs e2 = new TabEventArgs(previousTab, value);
				_selectedTab = value;
				RequestTabStripLayout(false);
				if (_tabContentRenderer != null)
				{
					if (_selectedTab != null)
					{
						if (_selectedTab.GetTabOwner() is RibbonContextualTab)
						{
							((RibbonContextualTab)_selectedTab.GetTabOwner()).Active = true;
						}
						_selectedTab.SetMarkedSelected(true);
						_tabContentRenderer.SetScrollOffset(_selectedTab._scrollOffset);
					}
					else
					{
						_tabContentRenderer.SetScrollOffset(0);
					}
					_tabContentRenderer.MarkGroupsStale();
				}
				InvalidateTabContent();
				RefreshWindowFrame(true);
				OnSelectedTabChanged(e2);
			}
		}

		[Description("Indicates whether the shortcut of the command is displayed in the menu window")]
		[Localizable(true)]
		[DefaultValue(false)]
		public bool ShowShortcutInMenus
		{
			get
			{
				return _showShortcutInMenus;
			}
			set
			{
				_showShortcutInMenus = value;
			}
		}

		[Localizable(true)]
		[Description("Indicates whether the shortcut of the command is displayed on its tooltip")]
		[DefaultValue(false)]
		public bool ShowShortcutInToolTips
		{
			get
			{
				return _showShortcutInToolTips;
			}
			set
			{
				_showShortcutInToolTips = value;
			}
		}

		[DefaultValue(true)]
		[Description("Determines whether a popup menu will appear when the user right-click on the Ribbon control.")]
		[Localizable(true)]
		public bool ShowQuickCustomizeMenu
		{
			get
			{
				return _showQuickCustomizeMenu;
			}
			set
			{
				_showQuickCustomizeMenu = value;
			}
		}

		[Description("Represents the MordexSuperTip component to show the supertip settings of the commands")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public MxSuperTip SuperTipComponent => _mordexSuperTip;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		public RibbonLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.FindRibbon() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the control");
				}
				if (value != null && !value.HasStandAloneBase())
				{
					throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
				}
				if (_currentLayout == value)
				{
					return;
				}
				CancelEventArgs e = new CancelEventArgs(cancel: false);
				OnCurrentLayoutChanging(e);
				if (!e.Cancel)
				{
					if (_currentLayout != null)
					{
						_currentLayout.AttachRibbon(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.AttachRibbon(this);
					}
					if (!_isInitializing)
					{
						ApplyCurrentLayout();
					}
				}
			}
		}

		[Description("Represents the minimum size of the Form required to display the Ribbon.")]
		public Size MinimumFormSize
		{
			get
			{
				return _minimumFormSize;
			}
			set
			{
				if (_minimumFormSize != value)
				{
					_minimumFormSize = value;
					ApplyMinimumFormSize();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public RibbonLayoutCollection Layouts => _layouts;

		[DefaultValue(null)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[Localizable(true)]
		public Stream LayoutStream
		{
			get
			{
				if (_layouts != null && _layouts.Count == 0)
				{
					return null;
				}
				MemoryStream memoryStream = new MemoryStream();
				SaveLayoutFile(memoryStream);
				memoryStream.Flush();
				memoryStream.Position = 0L;
				return memoryStream;
			}
			set
			{
				if (value != null)
				{
					if (!_isInitializing)
					{
						LoadLayoutFile(value);
					}
					else
					{
						_layoutStream = value;
					}
				}
			}
		}
	}
}
