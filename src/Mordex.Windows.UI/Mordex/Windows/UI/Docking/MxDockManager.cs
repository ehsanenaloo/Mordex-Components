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
	[Designer("Mordex.Windows.UI.Design.DockManagerDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(true)]
	[DesignerSerializer("Mordex.Windows.UI.Design.PanelManagerSerializer, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public partial class MxDockManager : Component, ISupportInitialize, IFormatStyleOwner, IMordexXmlLayoutsSupport, IPanelContainer, IPersistComponentSettings
	{
		private const int WM_NCACTIVATE = 0x86;

		private const int DefaultSplitterSize = 4;

		private const int DefaultPanelPadding = 3;

		// Vertical offset of the grab point when a drag starts without a mouse position.
		private const int DragStartOffsetY = 4;

		// Width of the band along each edge of the container that docks a dragged panel to that edge.
		private const int DockEdgeHitWidth = 12;

		private const int DefaultFloatingSize = 200;

		private const int WindowsVistaMajorVersion = 6;

		private const int ButtonImageSize = 16;

		private const int DefaultImageSize = 16;

		private const int DefaultLargeImageSize = 24;

		private DockLayoutSnapshot _dockInfo;

		private bool _allowPanelDrop;

		private bool _allowPanelDrag;

		private bool _allowPanelResize;

		private DockPanelCollection _panels;

		private FormatStyle _formatStyleTabStrip;

		private PanelPadding _panelPadding;

		private MxAutoHideStrip _leftStrip;

		private MxAutoHideStrip _rightStrip;

		private MxAutoHideStrip _topStrip;

		private MxAutoHideStrip _bottomStrip;


		private DockManagerRendererBase _panelRenderer;

		private TabGroupStyle _defaultTabGroupStyle;

		private ImageList _imageList;

		private ImageList _largeImageList;

		private TabbedMdiSettings _tabbedMdiSettings;

		private DockingStyleMode _dockingStyleMode;

		private bool _isInitializing;

		private DockPanelLayout _currentLayout;

		private PersistMode _persistMode = PersistMode.PanelLayout;

		private DockPanelLayoutCollection _layouts;

		private Stream _layoutStream;

		private ApplicationSettings _applicationSettings;

		private bool _saveSettings;

		private string _settingsKey;

		private DockManagerMessageFilter _dragFilter;

		private Rectangle _dockArea;

		private Rectangle _fillArea;

		private int _splitterSize;

		private Control _containerControl;

		private DefaultPanelSettings _defaultPanelSettings;

		private Color _backColorAutoHideStrip;

		private Color _backColorGradientAutoHideStrip;

		private Color _backColorSplitter;

		private Color _backColorGradientSplitter;

		private Color _backColorResizeBar;

		private Color _backColorGradientResizeBar;

		private DockManagerRendererBase _mdiRenderer;

		private bool _allowAutoHideAnimation = true;

		private object _tag;

		private bool? _useCompatibleTextRendering;

		private bool _showAutoHidePanelsOnClick;

		private int _layoutSuspendCount;

		private static readonly object _panelDockStateChangedKey = new object();

		private static readonly object _panelDockStyleChangedKey = new object();

		private static readonly object _panelStartDragKey = new object();

		private static readonly object _panelClosedChangingKey = new object();

		private static readonly object _panelClosedChangedKey = new object();

		private static readonly object _draggingPanelKey = new object();

		private static readonly object _panelDropKey = new object();

		private static readonly object _panelActivatedKey = new object();

		private static readonly object _panelDeactivateKey = new object();

		private static readonly object _panelAutoHideChangingKey = new object();

		private static readonly object _panelAutoHideChangedKey = new object();

		private static readonly object _panelAutoHideActiveChangedKey = new object();

		private static readonly object _panelAutoHideActiveChangingKey = new object();

		private static readonly object _groupSelectedPanelChangingKey = new object();

		private static readonly object _groupSelectedPanelChangedKey = new object();

		private static readonly object _layoutLoadKey = new object();

		private static readonly object _currentLayoutChangingKey = new object();

		private static readonly object _currentLayoutChangedKey = new object();

		private static readonly object _mdiTabInitializeKey = new object();

		private static readonly object _restoreMdiChildFormKey = new object();

		private static readonly object _mdiTabCreatedKey = new object();

		private static readonly object _mdiTabGroupCreatedKey = new object();

		private static readonly object _mdiTabGroupDestroyedKey = new object();

		private readonly object _mdiTabMouseDownKey = new object();

		private readonly object _mdiTabClosingKey = new object();

		private readonly object _mdiTabClosedKey = new object();

		private readonly object _mdiTabMouseUpKey = new object();

		private readonly object _mdiTabClickKey = new object();

		private readonly object _mdiTabDoubleClickKey = new object();

		private readonly object _panelGroupCreatedKey = new object();

		private static readonly object _containerControlChangedKey = new object();

		private static readonly object _systemColorsChangedKey = new object();

		private static readonly object _visualStyleChangedKey = new object();

		internal bool _isPanelLayoutSuspended;

		private ArrayList _panelSnapshot;

		private object _designer;

		private bool _tabbedMdi;

		private MdiTabGroupRenderer _mdiTabGroups;

		private BuiltInTextList _builtInTexts;

		internal bool _isDisposing;

		private bool _isAddingPanelInfo;

		private DockLayoutSnapshot _pendingDockInfo;

		private DesignerTransaction _designerTransaction;

		public MxDockManager(IContainer container)
		{
			InitializeDefaults();
			container?.Add(this);
		}

		public MxDockManager()
		{
			InitializeDefaults();
		}

		protected override void Dispose(bool disposing)
		{
			_isDisposing = true;
			StopThemeListening();
			if (disposing)
			{
				if (_saveSettings && !base.DesignMode)
				{
					TrySaveComponentSettings();
				}
				if (_bottomStrip != null)
				{
					_bottomStrip.Dispose();
				}
				if (_leftStrip != null)
				{
					_leftStrip.Dispose();
				}
				if (_rightStrip != null)
				{
					_rightStrip.Dispose();
				}
				if (_topStrip != null)
				{
					_topStrip.Dispose();
				}
			}
			DetachContainerControl();
			_containerControl = null;
			_isDisposing = false;
			// The base call raises the Disposed event, so the handler list is cleared only after it.
			base.Dispose(disposing);
			base.Events.Dispose();
		}
	}
}
