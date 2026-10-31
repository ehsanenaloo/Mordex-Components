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
using System.Runtime.CompilerServices;
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
		MxDockManager IPanelContainer.PanelManager => this;

		DockPanelCollection IPanelContainer.Panels => Panels;

		Control IFormatStyleOwner.Owner => _containerControl;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(null)]
		[Browsable(false)]
		[Localizable(false)]
		public DockPanelLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.GetPanelManager() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the component.");
				}
				if (value != null && !value.HasStandAloneRoot())
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
						_currentLayout.SetPanelManager(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.SetPanelManager(this);
					}
					if (!_isInitializing)
					{
						ApplyCurrentLayout();
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DockPanelLayoutCollection Layouts => _layouts;

		[DefaultValue(null)]
		[Localizable(true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
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
				_layoutStream = value;
			}
		}

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

		[Description("The background color of the splitter bars.")]
		public Color BackColorSplitter
		{
			get
			{
				if (_backColorSplitter.IsEmpty)
				{
					return GetPanelAppearance().GetColor(PanelColors.Splitter);
				}
				return _backColorSplitter;
			}
			set
			{
				if (_backColorSplitter != value)
				{
					_backColorSplitter = value;
					InvalidatePanels(_panels);
					LayoutAutoHideStrips();
				}
			}
		}

		[Description("The gradient background color of the splitter bars.")]
		public Color BackColorGradientSplitter
		{
			get
			{
				if (_backColorGradientSplitter.IsEmpty)
				{
					return GetPanelAppearance().GetColor(PanelColors.SplitterGradient);
				}
				return _backColorGradientSplitter;
			}
			set
			{
				if (_backColorGradientSplitter != value)
				{
					_backColorGradientSplitter = value;
					InvalidatePanels(_panels);
					LayoutAutoHideStrips();
				}
			}
		}

		[Description("Represents the settings used to draw the tab strip of Tab panel group.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle TabStripFormatStyle => _formatStyleTabStrip;

		[Description("The background color of the AutoHide strips.")]
		public Color BackColorAutoHideStrip
		{
			get
			{
				if (_backColorAutoHideStrip.IsEmpty)
				{
					return GetPanelAppearance().GetColor(PanelColors.AutoHideStrip);
				}
				return _backColorAutoHideStrip;
			}
			set
			{
				if (_backColorAutoHideStrip != value)
				{
					_backColorAutoHideStrip = value;
					LayoutAutoHideStrips();
				}
			}
		}

		[Description("Represents the gradient background color of the AutoHide strips.")]
		public Color BackColorGradientAutoHideStrip
		{
			get
			{
				if (_backColorGradientAutoHideStrip.IsEmpty)
				{
					return GetPanelAppearance().GetColor(PanelColors.AutoHideStripGradient);
				}
				return _backColorGradientAutoHideStrip;
			}
			set
			{
				if (_backColorGradientAutoHideStrip != value)
				{
					_backColorGradientAutoHideStrip = value;
					LayoutAutoHideStrips();
				}
			}
		}

		[Description("The background color of the resize bar in Outlook Navigator groups.")]
		public Color BackColorResizeBar
		{
			get
			{
				if (_backColorResizeBar.IsEmpty)
				{
					return GetPanelAppearance().GetColor(PanelColors.OutlookResizeBar);
				}
				return _backColorResizeBar;
			}
			set
			{
				if (_backColorResizeBar != value)
				{
					_backColorResizeBar = value;
					InvalidatePanels(_panels);
				}
			}
		}

		[Description("Represents the gradient background color of the resize bar in Outlook Navigator groups.")]
		public Color BackColorGradientResizeBar
		{
			get
			{
				if (_backColorGradientResizeBar.IsEmpty)
				{
					return GetPanelAppearance().GetColor(PanelColors.OutlookResizeBarGradient);
				}
				return _backColorGradientResizeBar;
			}
			set
			{
				if (_backColorGradientResizeBar != value)
				{
					_backColorGradientResizeBar = value;
					InvalidatePanels(_panels);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DockPanelBase[] DockedPanels
		{
			get
			{
				ArrayList arrayList = new ArrayList();
				for (int i = 0; i < _panels.Count; i++)
				{
					if (_panels[i].DockState == PanelDockState.Docked)
					{
						arrayList.Add(_panels[i]);
					}
				}
				DockPanelBase[] dockedPanels = new DockPanelBase[arrayList.Count];
				arrayList.CopyTo(dockedPanels);
				return dockedPanels;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DockPanelBase[] FloatingPanels
		{
			get
			{
				ArrayList arrayList = new ArrayList();
				for (int i = 0; i < _panels.Count; i++)
				{
					if (_panels[i].DockState == PanelDockState.Floating)
					{
						arrayList.Add(_panels[i]);
					}
				}
				DockPanelBase[] floatingPanels = new DockPanelBase[arrayList.Count];
				arrayList.CopyTo(floatingPanels);
				return floatingPanels;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Dock padding settings for all edges of the container control.")]
		public PanelPadding PanelPadding => _panelPadding;

		[DefaultValue(null)]
		[Description("Represents the Control that will be the parent for panel controls managed by the component.")]
		public Control ContainerControl
		{
			get
			{
				return _containerControl;
			}
			set
			{
				if (value is DockPanelBase && ((DockPanelBase)value).PanelManager == this)
				{
					throw new InvalidOperationException();
				}
				if (value is DockPanelInnerContainer)
				{
					DockPanelInnerContainer uiPanelInnerContainer = (DockPanelInnerContainer)value;
					if (uiPanelInnerContainer.GetOwnerPanel() != null && uiPanelInnerContainer.GetOwnerPanel().PanelManager == this)
					{
						throw new InvalidOperationException();
					}
				}
				if (_containerControl != value)
				{
					DetachContainerControl();
					_containerControl = value;
					AttachContainerControl();
					OnContainerControlChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether an animation occurs when showing an autohide panel.")]
		public bool AllowAutoHideAnimation
		{
			get
			{
				return _allowAutoHideAnimation;
			}
			set
			{
				_allowAutoHideAnimation = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MxAutoHideStrip AutoHideLeftStrip => _leftStrip;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MxAutoHideStrip AutoHideTopStrip => _topStrip;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MxAutoHideStrip AutoHideRightStrip => _rightStrip;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public MxAutoHideStrip AutoHideBottomStrip => _bottomStrip;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DockPanelCollection Panels => _panels;

		[Description("Contains default settings for panels managed by the component.")]
		[Localizable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public DefaultPanelSettings DefaultPanelSettings => _defaultPanelSettings;

		[Description("The settings that determines how TabbedMdi forms are managed.")]
		[Localizable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TabbedMdiSettings TabbedMdiSettings => _tabbedMdiSettings;

		[DefaultValue(true)]
		[Description("Determines whether panels can be dragged.")]
		public bool AllowPanelDrag
		{
			get
			{
				return _allowPanelDrag;
			}
			set
			{
				if (_allowPanelDrag != value)
				{
					_allowPanelDrag = value;
					InvalidatePanels(_panels);
				}
			}
		}

		[DefaultValue(null)]
		[Editor(typeof(ObjectTypeEditor), typeof(UITypeEditor))]
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
					CreateRenderers();
					InvalidatePanels(_panels);
					LayoutAutoHideStrips();
					RefreshMdiTabGroups();
				}
			}
		}

		[Description("Indicates the style used to drag/drop the panels")]
		[DefaultValue(DockingStyleMode.VS2005)]
		public DockingStyleMode DockingStyleMode
		{
			get
			{
				return _dockingStyleMode;
			}
			set
			{
				_dockingStyleMode = value;
			}
		}

		[DefaultValue(true)]
		[Description("Determines whether users can drop panels over other panels to create groups.")]
		public bool AllowPanelDrop
		{
			get
			{
				return _allowPanelDrop;
			}
			set
			{
				if (_allowPanelDrop != value)
				{
					_allowPanelDrop = value;
				}
			}
		}

		[DefaultValue(true)]
		[Description("Determines whether panels can be resized.")]
		public bool AllowPanelResize
		{
			get
			{
				return _allowPanelResize;
			}
			set
			{
				_allowPanelResize = value;
			}
		}

		[Description("Specifies the width in pixels of splitter bars.")]
		[DefaultValue(DefaultSplitterSize)]
		public int SplitterSize
		{
			get
			{
				return _splitterSize;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("'SplitterSize' must be bigger than or equal than 0");
				}
				if (_splitterSize != value)
				{
					_splitterSize = value;
					InvalidatePanels(_panels);
				}
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the auto hide panels should be displayed only when its tab is clicked")]
		public bool ShowAutoHidePanelsOnClick
		{
			get
			{
				return _showAutoHidePanelsOnClick;
			}
			set
			{
				_showAutoHidePanelsOnClick = value;
			}
		}

		[DefaultValue(TabGroupStyle.Tab)]
		[Localizable(false)]
		[Description("Determines which GroupStyle is used by default when a tab-like panel group is created.")]
		public TabGroupStyle DefaultTabGroupStyle
		{
			get
			{
				return _defaultTabGroupStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabGroupStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_defaultTabGroupStyle = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DockPanelBase ActivePanel
		{
			get
			{
				ArrayList arrayList = GetAllPanels();
				foreach (DockPanelBase item in arrayList)
				{
					if (item.IsActive)
					{
						return item;
					}
				}
				return null;
			}
		}

		[Description("Represents the ImageList that contains the images used in the 'OutlookNavigator' buttons.")]
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
					InvalidatePanels(_panels);
				}
			}
		}

		[DefaultValue(null)]
		[Localizable(true)]
		[Description("Represents the ImageList that contains the images used in the panels managed by the component.")]
		public ImageList ImageList
		{
			get
			{
				return _imageList;
			}
			set
			{
				if (_imageList != value)
				{
					_imageList = value;
					InvalidatePanels(_panels);
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(false)]
		[Description("Determines whether Mdi child form should be presented as tabs in one or more groups.")]
		public bool TabbedMdi
		{
			get
			{
				return _tabbedMdi;
			}
			set
			{
				if (value && _containerControl != null && !(_containerControl is Form))
				{
					throw new InvalidOperationException("TabbedMdi property can be true only when ContainerControl is an Mdi Container");
				}
				if (_tabbedMdi != value)
				{
					_tabbedMdi = value;
					if (!_isInitializing)
					{
						RecreateMdiTabGroups();
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MxMdiTabGroup[] MdiTabGroups
		{
			get
			{
				int groupCount = 0;
				if (_mdiTabGroups != null)
				{
					groupCount = _mdiTabGroups.GetGroups().Count;
				}
				MxMdiTabGroup[] groups = new MxMdiTabGroup[groupCount];
				if (_mdiTabGroups != null)
				{
					_mdiTabGroups.GetGroups().CopyTo(groups, 0);
				}
				return groups;
			}
		}

		[Localizable(true)]
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
				BuiltInTexts.SetLayoutXml(value);
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
				base.Site = value;
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
				if (value != null)
				{
					IComponentChangeService componentChangeService = (IComponentChangeService)value.GetService(typeof(IComponentChangeService));
					if (componentChangeService != null)
					{
						componentChangeService.ComponentRemoving += OnControlComponentRemoving;
						componentChangeService.ComponentRemoved += OnControlComponentRemoved;
					}
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorSplitter()
		{
			return !_backColorSplitter.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorSplitter()
		{
			BackColorSplitter = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorGradientSplitter()
		{
			return !_backColorGradientSplitter.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorGradientSplitter()
		{
			BackColorGradientSplitter = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorAutoHideStrip()
		{
			return !_backColorAutoHideStrip.Equals(Color.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorAutoHideStrip()
		{
			BackColorAutoHideStrip = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorGradientAutoHideStrip()
		{
			return !_backColorGradientAutoHideStrip.Equals(Color.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorGradientAutoHideStrip()
		{
			BackColorGradientAutoHideStrip = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorResizeBar()
		{
			return !_backColorResizeBar.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorResizeBar()
		{
			BackColorResizeBar = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorGradientResizeBar()
		{
			return !_backColorGradientResizeBar.Equals(Color.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorGradientResizeBar()
		{
			BackColorGradientResizeBar = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				CreateRenderers();
				InvalidatePanels(_panels);
				LayoutAutoHideStrips();
				RefreshMdiTabGroups();
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
			return BuiltInTexts.HasCustomTexts();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBuiltInTextsData()
		{
			BuiltInTexts.Reset();
		}
	}
}
