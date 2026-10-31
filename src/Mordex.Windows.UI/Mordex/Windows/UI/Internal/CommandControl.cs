using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal partial class CommandControl : Control, ICommandMenuOwner, ICommandBarsElement, ILayoutItemOwner
	{
		private const int WsPopup = int.MinValue;

		private const int WsClipSiblings = 0x04000000;

		private const int WsClipChildren = 0x02000000;

		private const int WsExTopmost = 0x8;

		private const int WsExToolWindow = 0x80;

		private const int WsExNoActivate = 0x08000000;

		private const int WmActivateApp = 0x1C;

		private const int WmMouseActivate = 0x21;

		private const int WmNcLButtonDown = 0xA1;

		private const int WmNcLButtonUp = 0xA2;

		private const int WmNcLButtonDblClk = 0xA3;

		private const int WmNcRButtonDown = 0xA4;

		private const int WmNcRButtonUp = 0xA5;

		private const int WmNcRButtonDblClk = 0xA6;

		private const int WmNcMButtonDown = 0xA7;

		private const int WmNcMButtonUp = 0xA8;

		private const int WmNcMButtonDblClk = 0xA9;

		private const int WmNcXButtonDown = 0xAB;

		private const int WmNcXButtonUp = 0xAC;

		private const int WmKeyDown = 0x100;

		private const int WmKeyUp = 0x101;

		private const int WmChar = 0x102;

		private const int WmSysKeyDown = 0x104;

		private const int WmSysKeyUp = 0x105;

		private const int WmSysChar = 0x106;

		private const int WmMouseMove = 0x200;

		private const int WmLButtonDown = 0x201;

		private const int WmLButtonUp = 0x202;

		private const int WmLButtonDblClk = 0x203;

		private const int WmRButtonDown = 0x204;

		private const int WmRButtonUp = 0x205;

		private const int WmRButtonDblClk = 0x206;

		private const int WmMButtonDown = 0x207;

		private const int WmMButtonUp = 0x208;

		private const int WmMButtonDblClk = 0x209;

		private const int WmXButtonDown = 0x20B;

		private const int WmXButtonUp = 0x20C;

		private const int MaNoActivate = 3;

		private const int SwShowNormal = 1;

		private const int SwShowNoActivate = 4;

		private const uint IdcArrow = 32512u;

		private const uint AnimationDuration = 200u;

		private const uint AwBlend = 0x80000u;

		// AnimateWindow flags for a menu that opens downward; the slide value is added on top.
		private const uint AnimateDownFlags = 0x40005u;

		private const uint AnimateDownSlideFlags = 0x40006u;

		// AnimateWindow flags for a menu that opens upward; the slide value is added on top.
		private const uint AnimateUpFlags = 0x40009u;

		private const uint AnimateUpSlideFlags = 0x4000Au;

		private const int BorderSizeThemed = 3;

		private const int ShadowSize = 4;

		private const int MenuOverlap = 3;

		private const int SubmenuArrowWidth = 17;

		private const int SubmenuArrowRightMargin = 19;

		private const int SubmenuArrowGlyphSize = 17;

		private const int OverflowArrowGlyphSize = 10;

		private const int SeparatorHeightThemed = 8;

		private const int SeparatorHeight = 3;

		private const int ItemPadding = 4;

		private const int ItemHeightPadding = 7;

		private const int ImageGap = 8;

		private const int MinItemHeight = 20;

		private const int MinItemHeightModern = 24;

		private const int TextGap = 6;

		private const int TextGapThemed = 4;

		private const int CheckMarginWidth = 15;

		private const int CheckMarginWidthClassic = 24;

		private const int CheckGlyphSize = 16;

		private const int LargeCheckInset = 8;

		private const int TearOffBarHeight = 9;

		private const int TearOffHitMargin = 10;

		private const int ScrollButtonPadding = 9;

		private const int ScrollButtonPaddingThemed = 3;

		private const int ExpandHitExtraHeight = 13;

		private const int SideAreaTopInPopup = 2;

		private const int SideAreaTopWithTearOff = 10;

		private const int SideAreaTopInBarWithTearOff = 9;

		private const int OverflowBottomPadding = 5;

		private const int InsertMarkHeight = 5;

		private const int InsertMarkOffset = 6;

		private const int SubmenuDelay = 400;

		private const int OverflowSubmenuDelay = 600;

		private const int ScrollHoverDelay = 100;

		private const int ExpandBarHoverDelay = 750;

		private const int ExpandTimerStartInterval = 25;

		private const int ExpandTimerInterval = 5;

		private const int ExpandStartStep = 4;

		private const int ExpandStepFactor = 3;

		private static readonly Size DefaultImageSize = new Size(16, 16);

		internal bool _activateOnShow;

		internal bool _selectFirstItem;

		private bool _isLayoutDirty = true;

		private Size _menuSize = Size.Empty;

		private Point _menuLocation = Point.Empty;

		internal CommandLayoutState _parentItem;

		internal bool _dropsDown = true;

		private Timer _submenuTimer;

		private Timer _expandTimer;

		private Timer _hoverTimer;

		private Timer _childCloseTimer;

		internal MxCollection _layoutItems;

		private FormatStyleLayoutState _scrollDownButton;

		private FormatStyleLayoutState _scrollUpButton;

		private FormatStyleLayoutState _expandBar;

		private FormatStyleLayoutState _tearOffBar;

		private int _expandHeight = -1;

		private int _firstVisibleIndex;

		private int _maxHeight;

		internal bool _isFlipped;

		private Rectangle _anchorBounds = Rectangle.Empty;

		private bool _isExpanding;

		private bool _canScrollDown;

		internal PopupSettings _baseSettings;

		private Rectangle _imageAreaBounds = Rectangle.Empty;

		private Rectangle _sidebarBounds = Rectangle.Empty;

		private bool _isItemMouseDown = true;

		private Rectangle _menuClientBounds = Rectangle.Empty;

		private ColorSchemeWindowMessageHandler _shadowWindow;

		private Size _borderSize = Size.Empty;

		private int _expandStep = -1;

		private int _pendingSelectionIndex = -1;

		private MxCommand _pendingSelectedCommand;

		private bool _alignItemsToTop = true;

		private int _visibleItemCount;

		private bool _canAnimate = true;

		private bool _expandAfterDelay;

		private int _fullMenuDelay;

		private bool _expandOnHover = true;

		internal bool _isOverflowMenu;

		private bool _holdMessages = true;

		private Point _tearOffDragStart = Point.Empty;

		private bool _hasImages;

		private Rectangle _shadowAnchorBounds = Rectangle.Empty;

		private Rectangle _requestedAnchorBounds = Rectangle.Empty;

		private bool _isMouseOnly = true;

		private bool _useSmallImages;

		internal int _upwardOffset = -1;

		private bool _showShadow = true;

		private bool _canUseThemes;

		private bool _isMouseInside;

		internal bool _alignRight;

		internal Size _anchorSize = Size.Empty;

		private bool _useMnemonic = true;


		internal int _anchorHeight;

		private bool _isClosing;

		private MxCommand[] _windowCommands;

		private bool _ignoreNextMouseUp;

		private bool _insertMarkAfter;

		private int _insertMarkIndex = -1;

		private MxCommand _customizeCommand;

		internal bool _hostsPopupControl;

		internal MxCollection _overflowItems;

		private MxCommandCollection _commands;

		private bool _isTearingOff;

		private ICommandMenuOwner _owner;

		private CommandControl _childMenu;

		private bool _hasCheckMargin;

		private VisualStylePainter _renderer;

		private DropDownAnimation _dropDownAnimation;


		private CommandLayoutState _openItem;

		private CommandLayoutState _selectedItem;

		private WindowMessageHandler _toolTipWindow;

		private ToolTip _toolTip;

		private FormatStyleLayoutState _hotButton;

		internal CommandControl(MxCommandCollection commands, CommandLayoutState parentItem, ICommandMenuOwner owner, PopupSettings baseSettings, bool holdMessages)
		{
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			RightToLeft = owner.RightToLeft;
			_holdMessages = holdMessages;
			_commands = commands;
			_parentItem = parentItem;
			if (parentItem != null)
			{
				SetHasCheckMargin(parentItem.GetCommand().IsAddRemoveButtonsItem());
			}
			_layoutItems = new MxCollection(this);
			_owner = owner;
			if (parentItem != null)
			{
				_owner.SelectedCommand = parentItem.GetCommand();
			}
			if (_owner is MxCommandBar)
			{
				_selectFirstItem = ((MxCommandBar)_owner)._isKeyboardNavigation;
				if (holdMessages && GetCommandManagerOwner().ShowFullMenuDelay > 0)
				{
					_fullMenuDelay = GetCommandManagerOwner().ShowFullMenuDelay;
					_expandAfterDelay = true;
				}
				_showShadow = GetCommandManagerOwner().ShowShadowUnderMenus;
				_useMnemonic = GetCommandManagerOwner().UseMnemonic;
			}
			else if (owner is CommandControl)
			{
				_isFlipped = ((CommandControl)_owner)._isFlipped;
				_showShadow = ((CommandControl)_owner)._showShadow;
				_useMnemonic = ((CommandControl)_owner)._useMnemonic;
			}
			else if (_owner is MxContextMenu)
			{
				_showShadow = ((MxContextMenu)_owner).ShouldShowShadow();
				if (holdMessages && GetCommandManagerOwner() != null && GetCommandManagerOwner().ShowFullMenuDelay > 0)
				{
					_fullMenuDelay = GetCommandManagerOwner().ShowFullMenuDelay;
					_expandAfterDelay = true;
				}
				_useMnemonic = ((MxContextMenu)_owner).UseMnemonic;
			}
			if (_fullMenuDelay <= 0)
			{
				_expandOnHover = false;
			}
			_baseSettings = baseSettings;
			if (baseSettings.StateStyles.FormatStyle.ForeColor.IsEmpty)
			{
				baseSettings.StateStyles.FormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.ControlText);
			}
			if (holdMessages)
			{
				_dropDownAnimation = _owner.EffectiveAnimation;
			}
			else
			{
				_dropDownAnimation = DropDownAnimation.None;
			}
			_useSmallImages = _owner.UseSmallImages;
			Font = _baseSettings.StateStyles.FormatStyle.ResolveFont(_owner.Font);
			_canUseThemes = _owner.CanUseThemes;
			if (GetCommandManagerOwner() != null)
			{
			}
			if (!_canUseThemes)
			{
				_borderSize = new Size(1, 1);
			}
			else
			{
				_borderSize = new Size(BorderSizeThemed, BorderSizeThemed);
			}
			AttachCommandHandlers();
		}

		private void AttachCommandHandlers()
		{
			foreach (MxCommand item in _commands)
			{
				item.AddInvalidatedHandler(OnCommandChanged);
			}
		}

		private void DetachCommandHandlers()
		{
			foreach (MxCommand item in _commands)
			{
				item.RemoveInvalidatedHandler(OnCommandChanged);
			}
		}

		private void NotifyAccessibilityClients(AccessibleEvents accessibleEvent, int childIndex)
		{
			try
			{
				AccessibilityNotifyClients(accessibleEvent, childIndex);
			}
			catch (Exception ex) when (ex is ExternalException || ex is InvalidOperationException)
			{
				// Screen reader notifications are optional. A failed one must not break the menu.
			}
		}

		protected override void Dispose(bool disposing)
		{
			NotifyAccessibilityClients(AccessibleEvents.SystemMenuPopupEnd, 0);
			StopSubmenuTimer();
			_isClosing = true;
			HideToolTip();
			if (_layoutItems != null)
			{
				_layoutItems.ClearItems();
			}
			if (_parentItem != null && _parentItem.GetCommand() != null && GetCommandManagerOwner() != null && !GetCommandManagerOwner()._isCustomizing)
			{
				_parentItem.GetCommand().NotifyCloseUp();
			}
			if (base.ContainsFocus && GetCommandManagerOwner() != null && GetCommandManagerOwner().GetForm() != null)
			{
				RenderingHelper.TryFocusControl(GetCommandManagerOwner().GetForm());
			}
			foreach (Control control in base.Controls)
			{
				control.Resize -= OnControlResize;
				control.MouseMove -= OnControlMouseMove;
				control.LostFocus -= OnControlLostFocus;
			}
			DestroyHandle();
			base.Controls.Clear();
			if (_shadowWindow != null)
			{
				_shadowWindow.DestroyHandle();
				_shadowWindow = null;
			}
			if (_owner is CommandControl)
			{
				((CommandControl)_owner).SetOpenItem(null);
			}
			else if (_parentItem != null && _owner.SelectedCommand == _parentItem.GetCommand())
			{
				_owner.SelectedCommand = null;
			}
			else if (_owner is MxCommandBar && FindOwnerCommandBar().GetFloatingWindow() != null)
			{
				FindOwnerCommandBar().GetFloatingWindow().SetPressedItem(null);
			}
			DetachCommandHandlers();
			StopSubmenuTimer();
			StopHoverTimer();
			StopChildCloseTimer();
			if (ChildMenu != null)
			{
				ChildMenu = null;
			}
			if (_owner != null && _owner.FindForm != null && Environment.OSVersion.Version.Major < 6)
			{
				_owner.FindForm.Invalidate();
				_owner.FindForm.Update();
			}
			_layoutItems.ClearItems();
			_parentItem = null;
			base.Dispose(disposing);
		}


		internal void SetTearingOff(bool isTearingOff)
		{
			_isTearingOff = isTearingOff;
		}



		public MxContextMenu FindOwnerContextMenu()
		{
			if (_owner is MxContextMenu)
			{
				return (MxContextMenu)_owner;
			}
			if (_owner is CommandControl)
			{
				return ((CommandControl)_owner).FindOwnerContextMenu();
			}
			return null;
		}

		public MxCommandBar FindOwnerCommandBar()
		{
			if (_owner is MxCommandBar)
			{
				return (MxCommandBar)_owner;
			}
			if (_owner is CommandControl)
			{
				return ((CommandControl)_owner).FindOwnerCommandBar();
			}
			return null;
		}

		public bool ShouldUseCompatibleTextRendering()
		{
			if (GetCommandManagerOwner() != null)
			{
				return GetCommandManagerOwner().UseCompatibleTextRendering;
			}
			return false;
		}

		public MxCommandManager GetCommandManagerOwner()
		{
			return _owner.CommandManagerOwner;
		}

		internal Mordex.Windows.UI.CommandBars.View ResolvePopupView()
		{
			if (FindOwnerCommandBar() != null)
			{
				return FindOwnerCommandBar().GetEffectivePopupView();
			}
			if (_owner is MxContextMenu)
			{
				return ((MxContextMenu)_owner).GetEffectiveView();
			}
			if (_owner is CommandControl)
			{
				return ((CommandControl)_owner).ResolvePopupView();
			}
			return Mordex.Windows.UI.CommandBars.View.SmallIcons;
		}
	}
}
