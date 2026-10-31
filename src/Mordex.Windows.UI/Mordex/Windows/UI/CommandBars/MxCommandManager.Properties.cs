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
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		AccessibleObject IAccessibleCommandBarsElement.AccessibilityObject => null;

		Control IFormatStyleOwner.Owner => null;

		bool ICommandBarOwner.IsInitializing => IsInitializing();

		bool IAccessibleCommandBarsElement.IsInitializing => IsInitializing();

		ISite IAccessibleCommandBarsElement.OwnerSite => base.Site;

		[DefaultValue(false)]
		[Description("Indicates whether the control should automatically persist its application settings properties.")]
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
					IExtenderProviderService extenderProviderService = (IExtenderProviderService)base.Site.GetService(typeof(IExtenderProviderService));
					if (extenderProviderService != null && EditContextMenu != null)
					{
						extenderProviderService.RemoveExtenderProvider(EditContextMenu);
					}
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
					IExtenderProviderService newExtenderProviderService = (IExtenderProviderService)value.GetService(typeof(IExtenderProviderService));
					if (newExtenderProviderService != null && EditContextMenu != null)
					{
						newExtenderProviderService.AddExtenderProvider(EditContextMenu);
					}
					IComponentChangeService newComponentChangeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
					if (newComponentChangeService != null)
					{
						newComponentChangeService.ComponentRename += OnControlComponentRename;
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public EditContextMenu EditContextMenu => _editContextMenu;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Guid Id
		{
			get
			{
				return _id;
			}
			set
			{
				if (_id != value)
				{
					_id = value;
				}
			}
		}

		[DefaultValue(null)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public CommandBarsLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.FindCommandManager() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the control");
				}
				if (value != null && !value.IsResolvable())
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
						_currentLayout.AttachCommandManager(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.AttachCommandManager(this);
					}
					if (!_isInitializing)
					{
						RememberCurrentLayout();
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public CommandBarsLayoutCollection Layouts => _layouts;

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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public MxCommandCollection Commands => _commands;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandCategoryCollection Categories => _categories;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandBarCollection CommandBars => _commandBars;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public ContextMenuCollection ContextMenus => _contextMenus;

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
					RefreshRebars();
				}
			}
		}

		[DefaultValue(true)]
		[Description("If True the first character preceded by an ampersand (&) will be used as the command's mnemonic key")]
		public bool UseMnemonic
		{
			get
			{
				return _useMnemonic;
			}
			set
			{
				if (_useMnemonic != value)
				{
					_useMnemonic = value;
					RefreshRebars();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
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

		[Localizable(true)]
		[Browsable(false)]
		public string BuiltInTextsData
		{
			get
			{
				return BuiltInTexts.GetLayoutXml();
			}
			set
			{
				BuiltInTexts.LoadLayoutXml(value);
			}
		}

		[Description("Represents the Control that will be the parent for the UIRebar controls containing the commandbars.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Control ContainerControl
		{
			get
			{
				return _containerControl;
			}
			set
			{
				if (_containerControl == value)
				{
					return;
				}
				bool hadRebars = false;
				if (_containerControl != null)
				{
					hadRebars = RemoveRebarsFromContainer();
					_containerControl.Resize -= OnContainerControlResize;
					_containerControl.ParentChanged -= OnContainerControlVisibleChanged;
					_containerControl.Layout -= OnContainerControlLayout;
					_containerControl.SystemColorsChanged -= OnContainerControlSystemColorsChanged;
					_containerControl.HandleCreated -= OnContainerControlVisibleChanged;
					_containerControl.VisibleChanged -= OnContainerControlVisibleChanged;
					UpdateForm(null);
				}
				_containerControl = value;
				if (_containerControl != null)
				{
					if (_containerControl is Form)
					{
						UpdateForm(_containerControl as Form);
					}
					UpdateForm(RenderingHelper.FindForm(_containerControl));
					if (hadRebars)
					{
						AddRebarsToContainer();
					}
					_containerControl.Resize += OnContainerControlResize;
					_containerControl.ParentChanged += OnContainerControlVisibleChanged;
					_containerControl.SystemColorsChanged += OnContainerControlSystemColorsChanged;
					_containerControl.Layout += OnContainerControlLayout;
					_containerControl.HandleCreated += OnContainerControlVisibleChanged;
					_containerControl.VisibleChanged += OnContainerControlVisibleChanged;
					if (!_isInitializing)
					{
						foreach (MxCommandBar commandBar in CommandBars)
						{
							commandBar._forceDockStyleUpdate = true;
							commandBar.DockStyle = commandBar.DockStyle;
						}
					}
				}
				OnContainerControlChanged(EventArgs.Empty);
			}
		}

		[Localizable(true)]
		[Description("Determines whether the commandbars are locked and can not be moved by the user.")]
		[DefaultValue(false)]
		public bool LockCommandBars
		{
			get
			{
				return _lockCommandBars;
			}
			set
			{
				if (_lockCommandBars != value)
				{
					_lockCommandBars = value;
					RefreshRebars();
				}
			}
		}

		[Description("Determines whether a popup menu will appear when the user right-click on the UIRebar or UICommandBar controls.")]
		[DefaultValue(true)]
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

		[Description("Specifies whether the user can customize the associated CommandBars.")]
		[Localizable(false)]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowCustomize
		{
			get
			{
				return _allowCustomize;
			}
			set
			{
				if (_allowCustomize != value)
				{
					_allowCustomize = value;
				}
			}
		}

		[Localizable(false)]
		[Description("Specifies whether the user can close the associated CommandBars.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowClose
		{
			get
			{
				return _allowClose;
			}
			set
			{
				if (_allowClose != value)
				{
					_allowClose = value;
				}
			}
		}

		[DefaultValue(ShortcutBehavior.Always)]
		[Description("Specifies which commands should fire its click event when its shortcut is pressed")]
		public ShortcutBehavior ShortcutBehavior
		{
			get
			{
				return _shortcutBehavior;
			}
			set
			{
				_shortcutBehavior = value;
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Localizable(false)]
		[Description("Inidcates whether the customize button will be shown in the toolbars.")]
		public InheritableBoolean ShowCustomizeButton
		{
			get
			{
				return _showCustomizeButton;
			}
			set
			{
				if (_showCustomizeButton != value)
				{
					_showCustomizeButton = value;
					RefreshRebars();
				}
			}
		}

		[Description("Specifies whether a shadow is painted under the drop down menus")]
		[DefaultValue(true)]
		public bool ShowShadowUnderMenus
		{
			get
			{
				return _showShadowUnderMenus;
			}
			set
			{
				if (_showShadowUnderMenus != value)
				{
					_showShadowUnderMenus = value;
				}
			}
		}

		[Description("Inidcates whether the whether the 'add or remove buttons' command will be displayed in the toolbars.")]
		[DefaultValue(InheritableBoolean.Default)]
		[Localizable(false)]
		public InheritableBoolean ShowAddRemoveButton
		{
			get
			{
				return _showAddRemoveButton;
			}
			set
			{
				if (_showAddRemoveButton != value)
				{
					_showAddRemoveButton = value;
				}
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the popup window will show all its commands regardless of their frequency.")]
		[Localizable(true)]
		public bool AlwaysShowFullMenus
		{
			get
			{
				return _alwaysShowFullMenus;
			}
			set
			{
				_alwaysShowFullMenus = value;
			}
		}

		[DefaultValue(DefaultShowFullMenuDelay)]
		[Description("Specifies the time, in milliseconds, that have to pass before the popup window displays all its commands.")]
		[Localizable(true)]
		public int ShowFullMenuDelay
		{
			get
			{
				return _showFullMenuDelay;
			}
			set
			{
				_showFullMenuDelay = value;
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
		[Localizable(true)]
		[Description("Specifying whether the font names are displayed using their corresponding Font in a UIFontPicker control.")]
		public bool UseFontInFontNames
		{
			get
			{
				return _useFontInFontNames;
			}
			set
			{
				if (_useFontInFontNames != value)
				{
					_useFontInFontNames = value;
					RefreshRebars();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the format styles used to draw the commands in their different states.")]
		public StateStyles CommandsStateStyles
		{
			get
			{
				if (_commandsStateStyles == null)
				{
					_commandsStateStyles = new StateStyles(this);
				}
				return _commandsStateStyles;
			}
		}

		[Description("Represents the format style properties used to draw the popup window that contains the child commands.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public PopupSettings PopupSettings
		{
			get
			{
				if (_popupSettings == null)
				{
					_popupSettings = new PopupSettings(this, empty: false);
				}
				return _popupSettings;
			}
		}

		[Description("Specifies the animation style used to show popup menus")]
		[DefaultValue(DropDownAnimation.System)]
		[Localizable(false)]
		public DropDownAnimation MenuAnimation
		{
			get
			{
				if (_menuAnimation == DropDownAnimation.Default)
				{
					return DropDownAnimation.System;
				}
				return _menuAnimation;
			}
			set
			{
				if (!Enum.IsDefined(typeof(DropDownAnimation), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_menuAnimation = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style properties used to draw the UICommandBar objects in the CommandBars collection.")]
		public FormatStyle CommandBarsFormatStyle
		{
			get
			{
				if (_commandBarsFormatStyle == null)
				{
					_commandBarsFormatStyle = new FormatStyle(this);
				}
				return _commandBarsFormatStyle;
			}
		}

		[Description("Specifies whether the UIPanelManager keeps the properties of merged toolbars once the merged toolbars are released.")]
		[DefaultValue(true)]
		public bool KeepMergeSettings
		{
			get
			{
				return _keepMergeSettings;
			}
			set
			{
				if (_keepMergeSettings != value)
				{
					_keepMergeSettings = value;
				}
			}
		}

		[Description("Specifies whether the UICommandManager will merge its commandbars with the commandbars form a UICommandManager in a MDI Parent or MDI Child form.")]
		[Localizable(true)]
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

		[Description("Indicates whether a ToolTip is shown when the mouse passes over a command.")]
		[DefaultValue(true)]
		[Localizable(true)]
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

		[Description("Indicates whether a ToolTip is shown when the mouse passes over a command in a menu window.")]
		[DefaultValue(false)]
		public bool ShowToolTipOnMenus
		{
			get
			{
				return _showToolTipOnMenus;
			}
			set
			{
				_showToolTipOnMenus = value;
			}
		}

		[Description("Represents the control that contains the UICommandBar controls at the top of the container control.")]
		[Browsable(false)]
		[DefaultValue(null)]
		public MxCommandBarHost TopRebar
		{
			get
			{
				if (_topRebar == null)
				{
					_topRebar = new MxCommandBarHost(this);
				}
				else if (_topRebar.IsDisposed)
				{
					_topRebar = null;
					_topRebar = new MxCommandBarHost(this);
				}
				return _topRebar;
			}
			set
			{
				if (!_isInitializing)
				{
					throw new InvalidOperationException();
				}
				if (_topRebar != value)
				{
					_topRebar = value;
				}
			}
		}

		[Description("Represents the control that contains the UICommandBar controls at the bottom of the container control.")]
		[DefaultValue(null)]
		[Browsable(false)]
		public MxCommandBarHost BottomRebar
		{
			get
			{
				if (_bottomRebar == null)
				{
					_bottomRebar = new MxCommandBarHost(this);
					_bottomRebar.Dock = DockStyle.Bottom;
				}
				else if (_bottomRebar.IsDisposed)
				{
					_bottomRebar = null;
					_bottomRebar = new MxCommandBarHost(this);
					_bottomRebar.Dock = DockStyle.Bottom;
				}
				return _bottomRebar;
			}
			set
			{
				if (!_isInitializing)
				{
					throw new InvalidOperationException();
				}
				if (_bottomRebar != value)
				{
					_bottomRebar = value;
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[Description("Represents the control that contains the UICommandBar controls on the left side of the container control.")]
		public MxCommandBarHost LeftRebar
		{
			get
			{
				if (_leftRebar == null)
				{
					_leftRebar = new MxCommandBarHost(this);
					_leftRebar.Dock = DockStyle.Left;
				}
				else if (_leftRebar.IsDisposed)
				{
					_leftRebar = null;
					_leftRebar = new MxCommandBarHost(this);
					_leftRebar.Dock = DockStyle.Left;
				}
				return _leftRebar;
			}
			set
			{
				if (!_isInitializing)
				{
					throw new InvalidOperationException();
				}
				if (_leftRebar != value)
				{
					_leftRebar = value;
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represents the control that contains the UICommandBar controls on the right side of the container control.")]
		[Browsable(false)]
		public MxCommandBarHost RightRebar
		{
			get
			{
				if (_rightRebar == null)
				{
					_rightRebar = new MxCommandBarHost(this);
					_rightRebar.Dock = DockStyle.Right;
				}
				else if (_rightRebar.IsDisposed)
				{
					_rightRebar = null;
					_rightRebar = new MxCommandBarHost(this);
					_rightRebar.Dock = DockStyle.Right;
				}
				return _rightRebar;
			}
			set
			{
				if (!_isInitializing)
				{
					throw new InvalidOperationException();
				}
				if (_rightRebar != value)
				{
					_rightRebar = value;
				}
			}
		}

		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		[Description("Specifies how the commands will be drawn on a UICommandBar.")]
		[DefaultValue(CommandStyle.Default)]
		public CommandStyle CommandsStyle
		{
			get
			{
				return _commandsStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CommandStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_commandsStyle != value)
				{
					_commandsStyle = value;
					RefreshRebars();
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
					RefreshRebars();
				}
			}
		}

		[Description("The ImageList to use when displaying commands as large icons.")]
		[Localizable(true)]
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
					RefreshRebars();
				}
			}
		}

		[Description("Indicates how the text of the commands are aligned, relative to their area, in the commanbars")]
		[DefaultValue(TextAlign.Right)]
		[Localizable(false)]
		public TextAlign TextAlign
		{
			get
			{
				if (_textAlign == TextAlign.Default)
				{
					return TextAlign.Right;
				}
				return _textAlign;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlign), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _textAlign)
				{
					_textAlign = value;
					RefreshRebars();
				}
			}
		}

		[Description("Represents the default number of days that a command could pass without being clicked before it became infrequent.")]
		[Localizable(true)]
		[DefaultValue(0)]
		public int DaysForInfrequency
		{
			get
			{
				return _daysForInfrequency;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				_daysForInfrequency = value;
			}
		}

		[Description("Specifies which ImageList will be used to get the images for the UICommand objects in a popup window.")]
		[Localizable(false)]
		[DefaultValue(View.SmallIcons)]
		public View PopupView
		{
			get
			{
				if (_popupView == View.Default)
				{
					return View;
				}
				return _popupView;
			}
			set
			{
				if (!Enum.IsDefined(typeof(View), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_popupView != value)
				{
					_popupView = value;
					RefreshRebars();
				}
			}
		}

		[DefaultValue(View.SmallIcons)]
		[Localizable(false)]
		[Description("Specifies which ImageList will be used to get the images for the UICommand objects.")]
		public View View
		{
			get
			{
				if (_view == View.Default)
				{
					return View.SmallIcons;
				}
				return _view;
			}
			set
			{
				if (!Enum.IsDefined(typeof(View), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_view != value)
				{
					_view = value;
					RefreshRebars();
					for (int index = _commandBars.Count - 1; index >= 0; index--)
					{
						MxCommandBar uiCommandBar = _commandBars[index];
						uiCommandBar.RecalculateLayout();
					}
					TopRebar.RefreshRows();
					LeftRebar.RefreshRows();
					RightRebar.RefreshRows();
					BottomRebar.RefreshRows();
					OnViewChanged(EventArgs.Empty);
				}
			}
		}

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
					RefreshRebars();
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
					RefreshRebars();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				RefreshRebars();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBuiltInTextsData()
		{
			return BuiltInTexts.HasCustomText();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBuiltInTextsData()
		{
			BuiltInTexts.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializePopupSettings()
		{
			if (_popupSettings != null)
			{
				return _popupSettings.ShouldSerialize();
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetPopupSettings()
		{
			if (ShouldSerializePopupSettings())
			{
				_popupSettings.Reset();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSmallImageSize()
		{
			return !_smallImageSize.Equals(DefaultSmallImageSize);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSmallImageSize()
		{
			SmallImageSize = DefaultSmallImageSize;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLargeImageSize()
		{
			return !_largeImageSize.Equals(DefaultLargeImageSize);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetLargeImageSize()
		{
			LargeImageSize = DefaultLargeImageSize;
		}
	}
}
