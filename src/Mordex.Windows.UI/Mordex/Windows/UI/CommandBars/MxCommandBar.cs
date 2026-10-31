using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	[DefaultEvent("CommandClick")]
	[Designer("Mordex.Windows.UI.Design.CommandBarDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[DefaultProperty("CommandBarType")]
	[ToolboxItem(false)]
	public partial class MxCommandBar : Control, IAccessibleCommandBarsElement, ILayoutItemOwner, ICommandBarsElement, IFormatStyleOwner, ICommandMenuOwner, ISupportInitialize, IMordexXmlLayoutsSupport
	{
		private const int FloatingFrameExtra = 6;

		private const int GripSize = 8;

		private const int SeparatorThickness = 6;

		private const int MinItemLength = 20;

		private const int MinItemThickness = 19;

		private const int SplitArrowWidth = 11;

		private const int ThemedSplitArrowWidth = 14;

		private const int TextPadding = 4;

		private const int ImagePadding = 4;

		private const int HorizontalImagePadding = 7;

		private const int MenuTextExtraHeight = 6;

		private const int MenuTextExtraWidth = 5;

		private const int CustomizeButtonLength = 13;

		private const int ThemedCustomizeButtonLength = 17;

		private const int CustomizeButtonImageMargin = 5;

		private const int DefaultBarThickness = 26;

		private const int MinFloatingLength = 24;

		private const int MinBarLength = 63;

		private const int MinRowThickness = 22;

		private const int EmptyRowThickness = 21;

		private const int ToolBarRowPadding = 4;

		private const int MdiButtonWidth = 16;

		private const int MdiButtonCount = 3;

		private const int MdiButtonsThickness = 20;

		private const int MdiButtonsBandLength = 144;

		private const int MdiButtonsBandThickness = 18;

		private const int MdiButtonsOffsetWithGrip = 11;

		private const int MdiIconSize = 16;

		private const int DropMarkerWidth = 6;

		private const int DropAreaLength = 15;

		private const int DropDownArrowSize = 10;

		private const int ChildMenuCloseDelay = 350;

		private static readonly Size DefaultSmallImageSize = new Size(16, 16);

		private static readonly Size DefaultLargeImageSize = new Size(24, 24);

		private bool _isLoadingLayout;

		private Container _container = new Container();

		internal bool _isLayoutDirty = true;

		internal MxCollection _layoutItems;

		internal bool _isMenuActive;

		internal Point _mousePoint = Point.Empty;

		internal bool _isKeyboardNavigation;

		private Timer _childMenuCloseTimer;

		private bool _isCreatingFloatingWindow;

		private bool _isInitializing;

		internal int _detachedIndex = -1;

		internal MxCommandManager _commandManager;

		private CommandBarType _commandBarType;

		private bool _isDragging;

		private InheritableBoolean _allowCustomize;

		private InheritableBoolean _allowClose;

		private InheritableBoolean _showCustomizeButton;

		private InheritableBoolean _showAddRemoveButton;

		private MxCommand _customizeCommand;

		private string _key = "";

		private bool _isThemed;

		private bool _fullRow;

		internal InheritableBoolean _wrappable;

		private InheritableBoolean _commandBarWrappable;


		private TextAlign _textAlign;

		private View _popupView;

		private View _view;

		private DropDownAnimation _animation;

		private bool _isHorizontal = true;

		private Point _floatingLocation = Point.Empty;

		private Size _floatingSize = Size.Empty;

		private MxCommand _sourceCommand;

		private CommandBarEditorDialog _floatingWindow;

		internal bool _forceDockStyleUpdate = true;

		private BarsDockStyle _dockStyle = BarsDockStyle.Top;

		private CommandBarHostLayoutState _hostRow;

		internal MxCommandBarHost _host;

		private InheritableBoolean _lockCommandBar;

		private bool _isClosed;

		private MxCommandCollection _commands;

		private Guid _parentID = Guid.Empty;

		private StateStyles _commandsStateStyles;

		private FormatStyle _formatStyle;

		private CommandStyle _commandsStyle;

		private PopupSettings _popupSettings;

		private CommandControl _childMenu;

		private InheritableBoolean _showToolTips;

		private bool _showInfrequentItems;

		private ColorSchemeWindowMessageHandler _selectedItemShadow;

		private CommandLayoutState _pressedItem;

		private CommandLayoutState _selectedItem;

		private WindowMessageHandler _toolTipWindow;

		private ToolTip _toolTip;

		private string _toolTipText = "";

		private CommandLayoutState _toolTipItem;

		private CommandLayoutState _hotItem;

		private ImageList _imageList;

		private ImageList _largeImageList;

		private int _mergeRowOrder = -1;

		private Point _mergedLocation = Point.Empty;

		private int _dockedRowIndex = -1;

		private BarsDockStyle _dockedDockStyle = BarsDockStyle.Top;

		private Point _dockedLocation = Point.Empty;

		internal bool _isRestoringDockedPosition;

		internal bool _forceRowUpdate;

		internal int _rowIndex = -1;

		internal Point _position;

		private int _maxItemLength;

		internal bool _canBeShifted = true;

		internal bool _isRowLayoutInProgress;

		private int _fullLength;

		internal int _thickness;

		private bool _allowRename;

		private bool _allowDelete;

		private InheritableBoolean _allowMerge;

		private bool _isMergedCommandBar;

		private int _floatingRowCount;

		private ArrayList _rows;

		private bool _isFullShown;

		internal InheritableBoolean _savedWrappable;

		internal int _savedRowIndex = -1;

		internal MxCollection _overflowItems;

		internal bool _isHostingPopupControl;

		internal Color _barBackColor;

		internal CommandLayoutState _customizeItem;

		internal FormatStyleLayoutState _gripItem;

		private CommandBarWindowButtonsState _mdiButtons;

		private MxCommand _mdiSystemMenu;

		internal bool _isInvokingShortcut;

		private Form _parentForm;

		private Form _activeMdiChild;

		private bool _isMdiChildMaximized;

		private Point _gripMouseDownPoint = Point.Empty;

		internal CommandBarMessageFilter _messageFilter;

		private AccessibleObject _accessibleObject;

		private bool _isDropAfterItem;

		private int _dropIndex = -1;

		private MxCommand _customizeSelectedCommand;

		private static readonly object _commandMouseHoverEvent = new object();

		private static readonly object _commandMouseLeaveEvent = new object();

		private static readonly object _changingDockStyleEvent = new object();

		private static readonly object _dockStyleChangedEvent = new object();

		private static readonly object _commandControlValueChangedEvent = new object();

		private static readonly object _commandClickEvent = new object();

		private static readonly object _commandCloseUpEvent = new object();

		private static readonly object _commandClosingEvent = new object();

		private static readonly object _commandPopupEvent = new object();

		public MxCommandBar()
			: this(null, "")
		{
		}

		public MxCommandBar(string key)
			: this(null, key)
		{
		}

		public MxCommandBar(MxCommandManager commandManager, string key)
		{
			InitializeLayout();
			Text = key;
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			BackColor = Color.Transparent;
			TabStop = false;
			CommandManager = commandManager;
			Key = key;
		}

		internal bool IsInitializing()
		{
			if (_commandManager != null && _commandManager.IsInitializing())
			{
				return true;
			}
			return _isInitializing;
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing && (_commandManager == null || !_commandManager.IsInitializing()))
			{
				_isInitializing = false;
				if (CommandManager != null)
				{
					ResolveBaseCommands();
				}
			}
			InvalidateLayout();
		}

		internal void CompleteInitialization()
		{
			_isInitializing = false;
			BindCommands();
			_forceDockStyleUpdate = true;
			DockStyle = _dockStyle;
			InvalidateLayout();
		}

		void IFormatStyleOwner.PropertyChange()
		{
			InvalidateLayout();
		}

		ImageList ICommandMenuOwner.GetImageList(MxCommand cmd)
		{
			ImageList imageList = ResolveImageList(GetEffectivePopupView(), cmd);
			if (imageList == null && GetEffectivePopupView() == View.LargeIcons)
			{
				return ResolveImageList(View.SmallIcons, cmd);
			}
			return imageList;
		}

		ImageList IAccessibleCommandBarsElement.GetImageList(bool smallIcons, MxCommand cmd)
		{
			if (smallIcons)
			{
				return ResolveImageList(View.SmallIcons, cmd);
			}
			return ResolveImageList(View.LargeIcons, cmd);
		}

		void IAccessibleCommandBarsElement.OnInvalidateCommand()
		{
			if (!IsInitializing() && CommandManager != null && !CommandManager._isBatchUpdating && !CommandManager._isLoadingLayout)
			{
				if (base.Visible)
				{
					RecalculateLayout(true);
					Invalidate();
				}
				else
				{
					InvalidateLayout();
				}
			}
			else
			{
				InvalidateLayout();
			}
		}

		MxCommandManager IAccessibleCommandBarsElement.GetCommandManager()
		{
			return CommandManager;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopChildMenuCloseTimer();
				_themeHook?.Stop();
				if (CommandManager != null && CommandManager.CommandBars.Contains(this))
				{
					CommandManager.CommandBars.Remove(this);
				}
				HideToolTip();
				_commands.DetachAll();
				for (int i = 0; i < _commands.Count; i++)
				{
					MxCommand uiCommand = _commands[i];
					uiCommand.ForceDispose();
				}
				_commands._originalCommands = null;
				if (_container != null)
				{
					_container.Dispose();
				}
				Controls.Clear();
				SetParentForm(null);
			}
			base.Dispose(disposing);
			if (disposing)
			{
				// Disposed handlers run inside base.Dispose, so the event list is cleared only after it.
				base.Events.Dispose();
			}
		}

		private void InitializeLayout()
		{
			_layoutItems = new MxCollection(this);
			_commands = new MxCommandCollection(this);
			SetThickness(GetDefaultThickness());
			Size = new Size(GetMinimumLength(), GetThickness());
		}

		internal bool IsDragging()
		{
			return _isDragging;
		}

		internal void SetDragging(bool isDragging)
		{
			if (_isDragging != isDragging)
			{
				_isDragging = isDragging;
				if (GetHostRow() != null)
				{
					GetHostRow()._isLayoutLocked = _isDragging;
				}
			}
		}
	}
}
