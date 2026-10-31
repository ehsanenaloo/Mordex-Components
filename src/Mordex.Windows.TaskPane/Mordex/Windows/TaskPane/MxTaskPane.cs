using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.TaskPane.Design;

namespace Mordex.Windows.TaskPane
{
	[Designer("Mordex.Windows.TaskPane.Design.TaskPaneDesigner, Mordex.Windows.TaskPane.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[DefaultProperty("Groups")]
	[DefaultEvent("ItemClick")]
	public partial class MxTaskPane : Control, ITaskPaneOwner, ISupportInitialize, IMordexXmlLayoutsSupport, IMessageFilter, IPersistComponentSettings
	{

		public const int AlphaEmpty = -1;

		private IContainer _iContainer;

		internal ControlStyleControl _verticalScrollbar;

		private TaskPaneControl _borderPanel;

		private int _borderInset;

		private TaskPaneLayoutCollection _layouts;

		private TaskPaneLayout _currentLayout;

		private TaskPaneLayout _lastLoadedLayout;

		private bool _isInitializing;

		private TaskPaneFormatStyle _backgroundFormatStyle;

		private TaskPaneFormatStyle _groupsFormatStyle;

		private TaskPaneFormatStyle _specialGroupsFormatStyle;

		private TaskPaneFormatStyle _itemsFormatStyle;

		private TaskPaneFormatStyle _selectedItemFormatStyle;

		private TaskPaneFormatStyle _hoverItemFormatStyle;

		private TaskPaneFormatStyle _hoverGroupFormatStyle;

		private TaskPaneFormatStyle _hoverSpecialGroupFormatStyle;

		private TaskPaneStateStyles _itemsStateStyles;

		private TaskPaneStateStyles _groupsStateStyles;

		private TaskPaneStateStyles _specialGroupsStateStyles;

		private ApplicationSettings _applicationSettings;

		private bool _saveSettings;

		private bool _isKeyboardNavigating;

		private bool _showFocusRectangle;

		private bool _selectable;

		private Rectangle _contentBounds = Rectangle.Empty;

		private BorderStyle _borderStyle;

		private TaskPaneGroupCollection _groups;

		private Color _blendColor = Color.Empty;

		private Color _flatBorderColor = Color.Empty;

		private float _defaultBlend;

		internal bool _isLayoutValid;

		private int _minTextWidth = 30;

		private bool _isTextTooNarrow;

		private ImageDrawMode _backgroundImageDrawMode;

		private TaskPaneWatermarkImage _watermarkImage;

		private Size _imageSize = Size.Empty;

		private ImageList _imageList;

		private TaskPaneItem _pressedItem;

		private TaskPaneGroup _pressedGroup;

		private TaskPaneItem _selectedItem;

		private bool _keepSelection;

		private int _columnSeparation = 10;

		private int _groupSeparation = -1;

		private int _leftMargin = -1;

		private int _rightMargin = -1;

		private int _topMargin = -1;

		private bool _useMnemonic = true;

		private bool? _useCompatibleTextRendering;

		private bool _hasContainerGroups;

		private AccessibleObject _accessibleObject;

		private Form _form;

		private int _innerHeight;

		private bool _isLayoutRunning;

		private int _columns = 1;

		private VerticalScrollBar _verticalScrollBar;

		private int _verticalScrollPosition;

		private bool _hasColumnSpan;

		private bool _isAnimatingGroup;

		private bool _showGroupBorder = true;

		private Effect _effect;

		private int _defaultItemImageIndex = -1;

		private int _defaultGroupImageIndex = -1;

		private OwnerDrawnArea _ownerDrawnArea;

		private bool _isPainting;

		private bool _isCloseButtonPressed;

		private bool _shouldActivateFirstGroup = true;

		private TaskPaneGroup _hoverGroup;

		private TaskPaneItem _activeItem;

		private TaskPaneGroup _activeGroup;

		private TaskPaneItem _hoverItem;

		private Cursor _cursor;

		private bool _showToolTips = true;

		private ToolTip _toolTip;

		private int _toolTipItemIndex = -1;

		private int _toolTipGroupIndex = -1;

		private int _alpha = -1;

		private AlphaMode _alphaMode = AlphaMode.Opaque;

		private AlphaMode _backColorAlphaMode;

		private AlphaMode _foreColorAlphaMode;

		public MxTaskPane()
		{
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
			InitializeControls();
			InitializeDefaults();
		}

		private void InitializeControls()
		{
			_iContainer = new Container();
			SuspendLayout();
			ResumeLayout(performLayout: false);
		}

		internal bool IsInitializing()
		{
			return _isInitializing;
		}

		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_isInitializing = false;
				if (_watermarkImage != null)
				{
					_watermarkImage.RebuildWashedBitmap();
				}
				_currentLayout = _layouts.FindCurrentLayout();
				if (_currentLayout != null)
				{
					_currentLayout.AttachTaskPane(this);
				}
				OnLayoutLoad(EventArgs.Empty);
			}
		}

		private void InitializeDefaults()
		{
			_isKeyboardNavigating = false;
			_groups = new TaskPaneGroupCollection(this);
			_borderStyle = BorderStyle.SunkenLight3D;
			BackColor = SystemColors.Window;
			_layouts = new TaskPaneLayoutCollection(this);
			Selectable = true;
			_backgroundFormatStyle = new TaskPaneFormatStyle(this, PredefinedStyle.ControlBackground);
			_groupsFormatStyle = new TaskPaneFormatStyle(this, PredefinedStyle.GroupHeader);
			_hoverGroupFormatStyle = new TaskPaneFormatStyle(this, PredefinedStyle.None);
			_hoverItemFormatStyle = new TaskPaneFormatStyle(this, PredefinedStyle.Hot);
			_selectedItemFormatStyle = new TaskPaneFormatStyle(this, PredefinedStyle.Selected);
			_hoverItemFormatStyle.FontUnderline = TriState.True;
			_itemsFormatStyle = new TaskPaneFormatStyle(this, PredefinedStyle.GroupBackground);
			_hoverSpecialGroupFormatStyle = new TaskPaneFormatStyle(this, PredefinedStyle.None);
			_specialGroupsFormatStyle = new TaskPaneFormatStyle(this, PredefinedStyle.SpecialGroupHeader);
			_itemsStateStyles = new TaskPaneStateStyles(this, _itemsFormatStyle, _hoverItemFormatStyle, _selectedItemFormatStyle);
			_groupsStateStyles = new TaskPaneStateStyles(this, _groupsFormatStyle, _hoverGroupFormatStyle, null);
			_specialGroupsStateStyles = new TaskPaneStateStyles(this, _specialGroupsFormatStyle, _hoverSpecialGroupFormatStyle, null);
			_itemsStateStyles.DisabledFormatStyle.SetPredefinedStyle(PredefinedStyle.Disabled);
			_itemsStateStyles.PressedFormatStyle.SetPredefinedStyle(PredefinedStyle.Pressed);
			_groupsStateStyles.DisabledFormatStyle.SetPredefinedStyle(PredefinedStyle.Disabled);
			_specialGroupsStateStyles.DisabledFormatStyle.SetPredefinedStyle(PredefinedStyle.Disabled);
			RefreshThemeResources();
			_verticalScrollbar = new ControlStyleControl(ScrollBarType.Vertical);
			_verticalScrollbar.Name = "VerticalScrollbar";
			_verticalScrollbar.Visible = false;
			_verticalScrollbar.AddValueChangedHandler(UpdateVerticalScrollPosition);
			_verticalScrollbar.AddScrollHandler(OnVerticalScroll);
			_verticalScrollbar.VisibleChanged += OnVerticalScrollbarVisibleChanged;
			_verticalScrollbar.MouseDown += OnVerticalScrollbarMouseDown;
			_verticalScrollbar.MouseMove += OnVerticalScrollbarMouseMove;
			_verticalScrollbar.MouseUp += OnVerticalScrollbarMouseUp;
			base.Controls.Add(_verticalScrollbar);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CancelGroupAnimation();
				if (_saveSettings && !base.DesignMode)
				{
					try
					{
						SaveComponentSettings();
					}
					catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ConfigurationException || ex is InvalidOperationException)
					{
						// Settings are best effort; a failed save must not stop the control from being released.
						Debug.WriteLine("MxTaskPane: could not save settings: " + ex.Message);
					}
				}
				_iContainer?.Dispose();
				UpdateForm(null);
				_toolTip?.Dispose();
				_toolTip = null;
				_watermarkImage?.DisposeWashedBitmap();
				_itemsStateStyles?.DisposeDrawingObjects();
				_groupsStateStyles?.DisposeDrawingObjects();
				_specialGroupsStateStyles?.DisposeDrawingObjects();
				_backgroundFormatStyle?.DisposeDrawingObjects();
			}
			base.Dispose(disposing);
		}

		private AlphaMode GetBackColorAlphaMode()
		{
			if (BackColorAlphaMode != AlphaMode.Empty)
			{
				return BackColorAlphaMode;
			}
			return _alphaMode;
		}

		private AlphaMode GetForeColorAlphaMode()
		{
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return _foreColorAlphaMode;
			}
			return _alphaMode;
		}
	}
}
