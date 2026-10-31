using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Navigation.Design;
using Mordex.Windows.Navigation.Internal;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	[Designer("Mordex.Windows.Navigation.Design.NavigationBarDesigner, Mordex.Windows.Navigation.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[DefaultEvent("ItemClick")]
	[DefaultProperty("Groups")]
	public partial class MxNavigationBar : Control, INavigationBarOwner, ISupportInitialize, IMordexXmlLayoutsSupport, IPersistComponentSettings
	{

		public const int AlphaEmpty = -1;

		private IContainer _iContainer;

		private Timer _timer;

		internal NavigationBarControl _scrollPreviousButton;

		private ImageList _imageList;

		private TextBox _txtRename;

		internal NavigationBarControl _scrollNextButton;

		private bool _useImageListIcons = true;

		private NavigationItem _mouseDownItem;

		private ApplicationSettings _applicationSettings;

		private bool _saveSettings;

		private bool _isInitializing;

		private UnfoldEffect _unfoldEffect;

		private NavigationBarLayoutCollection _layouts;

		private NavigationBarLayout _currentLayout;

		private NavigationBarLayout _lastLoadedLayout;

		private bool _isDragScrolling;

		private bool _isGroupSwitchPending;

		private bool _isScrollingBack;

		private bool _isScrollingForward;

		private DragStyle _dragStyle;

		private bool _keepSelection = true;

		private NavigationGroup _dragTargetGroup;

		private ImageList _smallImageList;

		private ScrollLocation _scrollLocation;

		private ImageList _largeImageList;

		private Rectangle _innerBounds;

		internal Rectangle _itemsAreaBounds = Rectangle.Empty;

		private bool _headerGroupVisible = true;

		private bool _hasContainerGroup;

		private int _defaultGroupImageIndex = -1;

		private int _defaultItemSmallImageIndex = -1;

		private int _defaultItemLargeImageIndex = -1;

		private Color _dropLineColor = Color.Red;

		private int _dropLinePosition = -1;

		private int _dropLineStartX;

		private int _dropLineEndX;

		private int _dragItemIndex = -1;

		private int _dragGroupIndex = -1;

		private bool _cancelDrag;

		private bool _isDropAfterItem;

		private ToolTip _toolTip;

		private NavigationBarTipWindow _tipWindow;

		private bool _isDragLocked = default;

		private bool _wasScrollBackEnabled;

		private bool _wasScrollForwardEnabled;

		private bool _showToolTips = true;

		private bool? _useCompatibleTextRendering;

		private bool _useMnemonic = true;

		private int _tipItemIndex = -1;

		private int _tipGroupIndex = -1;

		private Cursor _cursor;

		private bool _allowDragGroups = true;

		private Point _dragStartPoint = Point.Empty;

		private OwnerDrawnArea _ownerDrawnArea;

		private Rectangle _slideBounds = Rectangle.Empty;

		private bool _isSlideAnimating;

		private NavigationGroupCollection _groups;

		private bool _isItemLayoutValid;

		internal bool _isGroupLayoutValid;

		private Color _flatBorderColor = Color.Empty;

		private NavigationItem _dragSourceItem;

		internal NavigationItem _firstVisibleItem;

		private NavigationItem _hoverItem;

		private NavigationItemRenderer _dragItemRenderer;

		private NavigationGroup _hoverGroup;

		private NavigationGroup _pressedGroup;

		internal NavigationItem _selectedItem;

		private DragMode _dragMode = DragMode.Automatic;

		private AccessibleObject _accessibleObject;

		private bool _groupSelectionAccepted;

		internal NavigationGroup _selectedGroup;

		private GroupAppearance _groupAppearance;

		private BorderStyle _borderStyle;

		private float _defaultBlend;

		private bool _isPainting;

		private NavigationBarOrientation _orientation;

		private Size _smallImageSize = Size.Empty;

		private Size _largeImageSize = Size.Empty;

		private ColorSchemeRenderer _mordexColorSchemeRenderer = new ColorSchemeRenderer();

		private bool _shadowOnHover;

		private bool _isSelectedGroupLast;

		private ImageDrawMode _backgroundImageDrawMode;

		private SelectionArea _selectionArea;

		private ItemAppearance _itemAppearance;

		private Color _blendColor = Color.Empty;

		private NavigationBarStateStyles _groupsStateStyles;

		private NavigationBarFormatStyle _groupsFormatStyle;

		private NavigationBarFormatStyle _groupsBackgroundFormatStyle;

		private NavigationBarStateStyles _itemsStateStyles;

		private NavigationBarFormatStyle _itemsFormatStyle;

		private NavigationBarFormatStyle _selectedItemFormatStyle;

		private NavigationBarFormatStyle _hoverItemFormatStyle;

		private NavigationBarFormatStyle _hoverGroupFormatStyle;

		private int _alpha = -1;

		private AlphaMode _alphaMode = AlphaMode.Opaque;

		private AlphaMode _backColorAlphaMode;

		private AlphaMode _foreColorAlphaMode;

		public MxNavigationBar()
		{
			InitializeComponents();
			InitializeState();
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_currentLayout = _layouts.FindCurrentLayout();
				if (_currentLayout != null)
				{
					_currentLayout.SetAttachedNavigationBar(this);
				}
				_isInitializing = false;
				OnLayoutLoad(EventArgs.Empty);
			}
		}

		private void InitializeState()
		{
			_imageList.Images.Clear();
			Image scrollDownImage = NavigationGraphicsHelper.MakeMagentaTransparent(NavigationBarState.GetDownBitmap());
			Image scrollUpImage = NavigationGraphicsHelper.MakeMagentaTransparent(NavigationBarState.GetUpBitmap());
			Image scrollLeftImage = NavigationGraphicsHelper.MakeMagentaTransparent(NavigationBarState.GetLeftBitmap());
			Image scrollRightImage = NavigationGraphicsHelper.MakeMagentaTransparent(NavigationBarState.GetRightBitmap());
			if (scrollDownImage != null)
			{
				_imageList.Images.Add(scrollDownImage);
			}
			if (scrollUpImage != null)
			{
				_imageList.Images.Add(scrollUpImage);
			}
			if (scrollLeftImage != null)
			{
				_imageList.Images.Add(scrollLeftImage);
			}
			if (scrollRightImage != null)
			{
				_imageList.Images.Add(scrollRightImage);
			}
			_groups = new NavigationGroupCollection(this);
			_borderStyle = BorderStyle.Light3D;
			_layouts = new NavigationBarLayoutCollection(this);
			InvalidateItemLayout();
			InvalidateLayout(true);
			_innerBounds = Rectangle.Empty;
			_scrollPreviousButton = new NavigationBarControl(this, -1, _orientation, GroupAppearance, _borderStyle, FlatBorderColor, _imageList);
			_scrollNextButton = new NavigationBarControl(this, 1, _orientation, GroupAppearance, _borderStyle, FlatBorderColor, _imageList);
			_scrollPreviousButton.AddScrollHandler(OnScrollPreviousClick);
			_scrollNextButton.Visible = true;
			_scrollPreviousButton.Visible = true;
			_scrollNextButton.Bounds = Rectangle.Empty;
			_scrollPreviousButton.Bounds = Rectangle.Empty;
			_scrollNextButton.AddScrollHandler(OnScrollNextClick);
			_groupsBackgroundFormatStyle = new NavigationBarFormatStyle(this);
			_groupsFormatStyle = new NavigationBarFormatStyle(this);
			_itemsFormatStyle = new NavigationBarFormatStyle(this);
			_hoverGroupFormatStyle = new NavigationBarFormatStyle(this);
			_hoverItemFormatStyle = new NavigationBarFormatStyle(this);
			_selectedItemFormatStyle = new NavigationBarFormatStyle(this);
			_itemsStateStyles = new NavigationBarStateStyles(this, _itemsFormatStyle, _hoverItemFormatStyle, _selectedItemFormatStyle);
			_groupsStateStyles = new NavigationBarStateStyles(this, _groupsFormatStyle, _hoverGroupFormatStyle, null);
			base.Controls.Add(_scrollPreviousButton);
			base.Controls.Add(_scrollNextButton);
			_scrollNextButton.BringToFront();
			ScrollLocation = ScrollLocation.ItemsLevel;
			RefreshFormatStyles();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CancelSlideAnimation();
				if (_saveSettings && !base.DesignMode)
				{
					try
					{
						SaveComponentSettings();
					}
					catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ConfigurationException || ex is InvalidOperationException)
					{
						// Settings are best effort; a failed save must not stop the control from being released.
						Debug.WriteLine("MxNavigationBar: could not save settings: " + ex.Message);
					}
				}
				_iContainer?.Dispose();
				_txtRename?.Dispose();
				_itemsStateStyles?.DisposeBrushesAndFont();
				_groupsStateStyles?.DisposeBrushesAndFont();
				_groupsBackgroundFormatStyle?.DisposeBrushesAndFont();
				if (_dragItemRenderer != null)
				{
					_dragItemRenderer.DisposeBitmaps();
					_dragItemRenderer = null;
				}
				DisposeRenderers();
				HideToolTip();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponents()
		{
			_iContainer = new Container();
			_timer = new Timer(_iContainer);
			_imageList = new ImageList(_iContainer);
			_txtRename = new TextBox();
			_timer.Tick += OnTimerTick;
			_imageList.ColorDepth = ColorDepth.Depth8Bit;
			_imageList.ImageSize = new Size(15, 15);
			_imageList.TransparentColor = Color.Magenta;
			_txtRename.AutoSize = false;
			_txtRename.BorderStyle = System.Windows.Forms.BorderStyle.None;
			_txtRename.Location = new Point(230, 17);
			_txtRename.Multiline = true;
			_txtRename.Name = "txtRename";
			_txtRename.Size = new Size(0, 0);
			_txtRename.TabIndex = 0;
			_txtRename.Text = "";
		}
	}
}
