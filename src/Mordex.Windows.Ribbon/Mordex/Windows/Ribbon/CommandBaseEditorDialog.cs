using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog : Form, ICommandRenderHost, ICommandDesignTarget
	{

		protected Rectangle _contentBounds;

		internal ArrayList _layoutItems;

		protected CommandCollection _commands;

		protected Rectangle _adjustedAnchorBounds;

		protected Rectangle _anchorScreenBounds;

		protected bool _isLayoutDirty;

		protected bool _isMenuBoundsDirty;

		protected CommandBase _parentCommand;

		internal CommandBaseRenderer _sourceItem;

		internal bool _selectFirstItem;

		internal bool _opensVertically;

		internal bool _isFlipped;

		internal bool _isRightToLeft;

		protected int _maxPopupHeight;

		private bool _exitMessageLoop;

		private bool _ignoreNextMouseUp;

		protected CommandBaseRenderer _focusedItem;

		private CommandBaseRenderer _designHoverItem;

		private CommandBaseEditorDialog _childMenu;

		internal ICommandRenderHost _owner;

		private Timer _hoverTimer;

		private Timer _childCloseTimer;

		protected int _maxDropDownItems;

		private Rectangle _resizeGripBounds;

		private Rectangle _resizeBarBounds;

		private bool _isResizing;

		private Point _resizeStartPoint;

		private DropDownResizeMode _dropDownResizeMode;

		private Size _resizeStartSize;

		private RibbonControl _pressedScrollBar;

		private RibbonControl _scrollBar;

		private CommandBaseRenderer _selectedItem;

		internal int _scrollOffset;

		protected bool _hasVerticalScrollBar;

		private bool _isResizeClamped;

		private PopupMenuType _popupMenuType;

		protected int _totalItemHeight;

		private List<CommandBase> _mdiListCommands;

		internal bool _isMouseInside;

		public RibbonColorScheme _popupColorScheme;

		internal RibbonPopupShadowWindow _shadowWindow;

		internal bool _isDesignMode;

		private RibbonContextMenuRenderer _designContextMenuRenderer;

		internal PopupOwnerItemRenderer _ownerItemRenderer;

		private CommandBaseRenderer _designSelectedItem;

		internal CommandBase _inPlaceEditCommand;

		private CommandBase _addedCommand;

		private bool _allowInPlaceEdit;

		private Point _dragStartPoint;

		public Size _minPopupSize;

		public Point _locationOverride;

		internal bool _activateOnShow;

		private int _galleryColumnCount;

		protected int _minPopupWidth;

		private bool _animateHide = true;

		protected MxRibbon _ribbon;

		private IntPtr _hoveredControlHandle;

		protected Size _contentPadding;

		protected int _contentOffsetY;

		internal Size _anchorSize;

		internal CommandSizeStyle _commandSizeStyle;

		protected IList<PopupMenuSection> _sections;

		internal int _upwardOffset;

		internal MxSuperTip _superTip;

		private ButtonCommand _superTipCommand;

		internal bool _showToolTipOnFocus;

		private Point _lastMousePoint;

		private static readonly object s_dropDownEventKey = new object();

		private IContainer _components;

		private bool _ownsSuperTip;

		private bool _isDisposed;

		public CommandBaseEditorDialog(ICommandRenderHost owner, CommandCollection commands, CommandBaseRenderer item, DropDownCommand parentCommand)
		{
			InitializeComponent();
			if (item != null)
			{
				item.Active = true;
				if (item is RibbonButtonRenderer)
				{
					((RibbonButtonRenderer)item).DroppedDown = true;
				}
			}
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			_owner = owner;
			base.Padding = new Padding(2);
			_owner.ChildMenu = this;
			_commands = commands;
			_dropDownResizeMode = DropDownResizeMode.None;
			_parentCommand = parentCommand;
			if (GetRibbonContextMenu() != null)
			{
				_maxDropDownItems = GetRibbonContextMenu().MaxDropDownItems;
			}
			if (parentCommand != null)
			{
				if (parentCommand != null)
				{
					_maxDropDownItems = parentCommand.MaxDropDownItems;
					if (parentCommand.AllowDropDownResize)
					{
						_dropDownResizeMode = DropDownResizeMode.Height;
					}
				}
				if (parentCommand is Gallery)
				{
					if (_dropDownResizeMode == DropDownResizeMode.Height && ((Gallery)parentCommand).AllowResizeColumns)
					{
						_dropDownResizeMode = DropDownResizeMode.Both;
					}
				}
				else
				{
					_popupMenuType = PopupMenuType.CommandList;
				}
			}
			else if (item != null && item.GetCommand() is RibbonGroup)
			{
				_popupMenuType = PopupMenuType.RibbonGroupCommands;
			}
			Font = _owner.Font;
			if (GetRibbon() != null)
			{
				_superTip = GetRibbon()._mordexSuperTip;
			}
			if (_superTip == null)
			{
				_superTip = new MxSuperTip { UseDefaultTheme = true };
				_ownsSuperTip = true;
				_superTip.Font = Font;
			}
			_galleryColumnCount = -1;
			_isDesignMode = _owner.IsInDesignMode;
			_popupColorScheme = _owner.ColorScheme;
			_isRightToLeft = _owner.RightToLeft == RightToLeft.Yes;
			_sourceItem = item;
			_upwardOffset = 0;
			_opensVertically = true;
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
		}

		internal MxRibbon GetRibbon()
		{
			if (_ribbon != null)
			{
				return _ribbon;
			}
			if (_owner is CommandBaseEditorDialog)
			{
				return ((CommandBaseEditorDialog)_owner).GetRibbon();
			}
			return _owner as MxRibbon;
		}

		internal MxRibbonStatusBar GetRibbonStatusBar()
		{
			if (_owner is CommandBaseEditorDialog)
			{
				return ((CommandBaseEditorDialog)_owner).GetRibbonStatusBar();
			}
			return _owner as MxRibbonStatusBar;
		}

		internal MxRibbonContextMenu GetRibbonContextMenu()
		{
			if (_owner is CommandBaseEditorDialog)
			{
				return ((CommandBaseEditorDialog)_owner).GetRibbonContextMenu();
			}
			return _owner as MxRibbonContextMenu;
		}

		internal CommandBase GetParentMenuCommand()
		{
			return _parentCommand;
		}

		internal void SetPressedScrollBar(RibbonControl ribbonControl)
		{
			_pressedScrollBar = ribbonControl;
		}

		internal int GetScrollOffset()
		{
			return _scrollOffset;
		}

		internal void SetScrollOffset(int scrollOffset)
		{
			if (_scrollOffset != scrollOffset)
			{
				_scrollOffset = scrollOffset;
				MarkLayoutDirty();
			}
		}

		public int GetGalleryColumnCount()
		{
			return _galleryColumnCount;
		}

		public void SetGalleryColumnCount(int galleryColumnCount)
		{
			_galleryColumnCount = galleryColumnCount;
		}

		public CommandBaseRenderer GetSelectedItem()
		{
			return _selectedItem;
		}

		public void SetSelectedItem(CommandBaseRenderer commandBaseEditorDialogCommandBaseRenderer)
		{
			if (_selectedItem != commandBaseEditorDialogCommandBaseRenderer)
			{
				if (_selectedItem != null)
				{
					_selectedItem.Selected = false;
				}
				_selectedItem = commandBaseEditorDialogCommandBaseRenderer;
				if (_selectedItem != null)
				{
					StopHoverTimer();
					StopChildCloseTimer();
					_selectedItem.Selected = true;
				}
			}
		}

		internal CommandBaseEditorDialog GetChildMenu()
		{
			return _childMenu;
		}

		internal void SetChildMenu(CommandBaseEditorDialog commandBaseEditorDialog)
		{
			if (_childMenu != commandBaseEditorDialog)
			{
				StopChildCloseTimer();
				if (_childMenu != null)
				{
					NativeMethods.AnimateWindow(_childMenu.Handle, NativeMethods.PopupFadeMilliseconds, NativeMethods.AwHideWithFade);
					_childMenu.RemoveDropDownHandler(OnChildMenuDropDown);
					_childMenu.Dispose();
				}
				_childMenu = commandBaseEditorDialog;
				if (_childMenu != null)
				{
					_childMenu.AddDropDownHandler(OnChildMenuDropDown);
				}
			}
		}

		internal void UpdateScrollBar(Rectangle rectangle, int scrollMinimum, int scrollMaximum, int scrollPageSize, int scrollValue, int scrollLineSize)
		{
			try
			{
				if (_scrollBar == null)
				{
					_scrollBar = new RibbonControl(ScrollBarType.Vertical);
					_scrollBar.AddScrollHandler(OnScrollBarScroll);
					_scrollBar.MouseDown += OnStateMouseDown;
					_scrollBar.MouseUp += OnStateMouseUp;
				}
				if (GetRibbon() != null)
				{
				}
				_scrollBar.SetMinimum(scrollMinimum);
				_scrollBar.SetMaximum(scrollMaximum);
				if (scrollPageSize > 0)
				{
					_scrollBar.SetLargeChange(scrollPageSize);
				}
				else
				{
					_scrollBar.SetLargeChange(scrollLineSize);
				}
				_scrollBar.SetSmallChange(scrollLineSize);
				_scrollBar.SetScrollValue(scrollValue);
				if (scrollMaximum <= scrollPageSize)
				{
					_scrollBar.Enabled = false;
				}
				else
				{
					_scrollBar.Enabled = true;
				}
				int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
				if (RightToLeft == RightToLeft.Yes)
				{
					_scrollBar.Location = new Point(rectangle.X, rectangle.Top);
				}
				else
				{
					_scrollBar.Location = new Point(rectangle.Right - verticalScrollBarWidth, rectangle.Top);
				}
				_scrollBar.Size = new Size(verticalScrollBarWidth, rectangle.Height);
				if (!base.Controls.Contains(_scrollBar))
				{
					base.Controls.Add(_scrollBar);
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException || ex is ArgumentException)
			{
				// The dialog may be closing while it lays out; the scroll bar is not needed then.
			}
		}

		private CommandBaseEditorDialog FindAncestorMenuAt(NativeMethods.NativePoint screenPoint)
		{
			if (_owner is CommandBaseEditorDialog)
			{
				CommandBaseEditorDialog commandBaseEditorDialog = (CommandBaseEditorDialog)_owner;
				if (commandBaseEditorDialog.Bounds.Contains(screenPoint._x, screenPoint._y))
				{
					return commandBaseEditorDialog;
				}
				if (commandBaseEditorDialog._owner is CommandBaseEditorDialog)
				{
					return ((CommandBaseEditorDialog)commandBaseEditorDialog._owner).FindAncestorMenuAt(screenPoint);
				}
			}
			return null;
		}

		protected virtual ContainerControlCommand GetParentCommand(Control childControl, ref bool exitLoop)
		{
			return FindContainerControlCommand(childControl, ref exitLoop, _layoutItems);
		}

		internal ContainerControlCommand FindContainerControlCommand(Control childControl, ref bool exitLoop, ArrayList layoutItems)
		{
			if (layoutItems == null)
			{
				return null;
			}
			foreach (CommandBaseRenderer item in layoutItems)
			{
				if (item is CommandContainerRenderer)
				{
					ArrayList arrayList = new ArrayList();
					foreach (CommandBaseRenderer childRenderer in ((CommandContainerRenderer)item)._childRenderers)
					{
						arrayList.Add(childRenderer);
					}
					ContainerControlCommand containerControlCommand = FindContainerControlCommand(childControl, ref exitLoop, arrayList);
					if (containerControlCommand != null)
					{
						return containerControlCommand;
					}
				}
				if (item.CommandType != CommandType.ContainerControl)
				{
					continue;
				}
				ContainerControlCommand itemCommand = ((ContainerControlCommandRenderer)item).GetContainerCommand();
				if (itemCommand.Control == null)
				{
					continue;
				}
				if (itemCommand.Control == childControl)
				{
					return itemCommand;
				}
				if (NativeMethods.IsChild(itemCommand.Control.Handle, childControl.Handle))
				{
					return itemCommand;
				}
				for (Control control = RibbonDrawingUtils.GetParentControl(childControl); control != null; control = RibbonDrawingUtils.GetParentControl(control))
				{
					if (control == itemCommand.Control)
					{
						return itemCommand;
					}
				}
			}
			return null;
		}

		internal void HideSuperTip()
		{
			if (_superTip != null)
			{
				_superTip.HideActiveToolTip();
			}
			_superTipCommand = null;
		}

		internal void UpdateSuperTip(CommandBaseRenderer commandBaseRenderer)
		{
			if (_owner != null && !_owner.ShowToolTips)
			{
				return;
			}
			ButtonCommand buttonCommand = null;
			if (commandBaseRenderer != null)
			{
				buttonCommand = commandBaseRenderer.GetCommand() as ButtonCommand;
			}
			if (buttonCommand == null || buttonCommand.SuperTipSettings.IsEmpty)
			{
				HideSuperTip();
			}
			else if (!_showToolTipOnFocus)
			{
				HideSuperTip();
			}
			else
			{
				if (buttonCommand == _superTipCommand)
				{
					return;
				}
				HideSuperTip();
				_superTipCommand = buttonCommand;
				string text = "";
				if (ShouldShowShortcutInToolTip() && buttonCommand.Shortcut != Shortcut.None)
				{
					text = text + " (" + RibbonDrawingUtils.GetShortcutText(buttonCommand.Shortcut) + ")";
				}
				Point p = new Point(PointToClient(Cursor.Position).X, commandBaseRenderer._bounds.Bottom + 10);
				p = PointToScreen(p);
				SuperTipSettings superTipSettings = buttonCommand._superTipSettings.Clone();
				superTipSettings.HeaderText += text;
				if (GetRibbon() != null)
				{
					if (GetRibbon().SuperTipDelay <= 0)
					{
						_superTip.ShowAlways = true;
					}
					else
					{
						_superTip.ShowAlways = false;
						_superTip.AutoPopDelay = GetRibbon().SuperTipDelay;
					}
				}
				if (GetRibbon() != null)
				{
					_superTip.BodyWidth = GetRibbon().SuperTipWidth;
					_superTip.Show(superTipSettings, this, p, GetRibbon().SuperTipDelay);
				}
				else
				{
					_superTip.Show(superTipSettings, this, p, 9000);
				}
			}
		}

		private bool ShouldShowShortcutInToolTip()
		{
			if (_owner != null)
			{
				if (_owner is CommandBaseEditorDialog)
				{
					return ((CommandBaseEditorDialog)_owner).ShouldShowShortcutInToolTip();
				}
				if (_owner is MxRibbon)
				{
					return ((MxRibbon)_owner).ShowShortcutInToolTips;
				}
				if (_owner is MxRibbonContextMenu)
				{
					return ((MxRibbonContextMenu)_owner).ShowShortcutInToolTips;
				}
			}
			return false;
		}

		internal void RefreshForCommand(CommandBase command, bool forceRefresh)
		{
			if (forceRefresh || (_commands != null && _commands.Contains(command)))
			{
				MarkMenuBoundsDirty();
			}
			if (command != null && command.GetParentContainer() is Gallery)
			{
				MarkMenuBoundsDirty();
			}
			if (GetChildMenu() != null)
			{
				GetChildMenu().RefreshForCommand(command, forceRefresh);
			}
		}

		internal bool IsOutsideDragRectangle(int x, int y, Point point)
		{
			Size dragSize = SystemInformation.DragSize;
			if (!new Rectangle(point.X - dragSize.Width, point.Y - dragSize.Height, dragSize.Width * 2, dragSize.Height * 2).Contains(x, y))
			{
				return true;
			}
			return false;
		}

		private void EndResize(bool cancel)
		{
			if (cancel)
			{
				base.Size = _resizeStartSize;
				MarkMenuBoundsDirty();
			}
			else if (base.Capture)
			{
				RibbonDrawingUtils.SetCapture(this, false);
			}
			_isResizing = false;
			_resizeStartPoint = Point.Empty;
			_resizeStartSize = base.Size;
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new PopupMenuAccessibleObject(this);
		}

		public void AddDropDownHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(s_dropDownEventKey, commandEventHandler);
		}

		public void RemoveDropDownHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(s_dropDownEventKey, commandEventHandler);
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			if (_shadowWindow != null)
			{
				if (base.Region == null)
				{
					_shadowWindow.ResizeShadow(new Rectangle(base.Location, new Size(base.Bounds.Width + 4, base.Bounds.Height + 4)));
				}
				else
				{
					_shadowWindow.ResizeShadow(new Rectangle(base.Location, new Size(base.Bounds.Width + 4, base.Bounds.Height + 3)));
				}
			}
			base.OnResize(eventArgs);
		}

		protected override void OnFormClosing(FormClosingEventArgs eventArgs)
		{
			base.OnFormClosing(eventArgs);
		}

		protected override void OnVisibleChanged(EventArgs eventArgs)
		{
			base.OnVisibleChanged(eventArgs);
			if (!base.Visible)
			{
				if (_shadowWindow != null)
				{
					_shadowWindow.DisposeShadow();
					_shadowWindow = null;
				}
				if (_childMenu != null)
				{
					_childMenu._animateHide = false;
					_childMenu.Visible = false;
					_childMenu._animateHide = true;
				}
				if (_animateHide)
				{
					NativeMethods.AnimateWindow(base.Handle, NativeMethods.PopupFadeMilliseconds, NativeMethods.AwHideWithFade);
				}
			}
			else if (_shadowWindow == null && !_isDesignMode && ThemeAllowsShadow && RibbonDrawingUtils.IsHighColorDisplay())
			{
				_shadowWindow = new RibbonPopupShadowWindow(_popupColorScheme._popupShadowColor);
				if (base.Region != null)
				{
					_shadowWindow.ShowShadow(new Rectangle(base.Location, new Size(base.Bounds.Width + 4, base.Bounds.Height + 3)));
				}
				else
				{
					_shadowWindow.ShowShadow(new Rectangle(base.Location, new Size(base.Bounds.Width + 4, base.Bounds.Height + 4)));
				}
			}
		}

		internal void SetDesignHoverItem(CommandBaseRenderer commandBaseRenderer)
		{
			if (_designHoverItem == commandBaseRenderer)
			{
				return;
			}
			if (_designHoverItem != null)
			{
				if (_designHoverItem is RibbonContextMenuRenderer)
				{
					_designHoverItem.Hot = false;
				}
				else if (_designHoverItem.CommandType == CommandType.Gallery)
				{
					((GalleryMenuRenderer)_designHoverItem).SetDesignHighlightItem(null);
				}
			}
			if (commandBaseRenderer is RibbonContextMenuRenderer)
			{
				commandBaseRenderer.Hot = true;
			}
			_designHoverItem = commandBaseRenderer;
			Invalidate();
		}

		public void SetDesignContextMenuRenderer(RibbonContextMenuRenderer ribbonContextMenuRenderer)
		{
			if (_designContextMenuRenderer != ribbonContextMenuRenderer)
			{
				if (_designContextMenuRenderer != null)
				{
					_designContextMenuRenderer.Dispose();
				}
				_designContextMenuRenderer = ribbonContextMenuRenderer;
			}
		}

		private void SetInPlaceEditCommand(CommandBaseRenderer commandBaseRenderer)
		{
			if (commandBaseRenderer != null && CanEditInPlace())
			{
				_inPlaceEditCommand = commandBaseRenderer.GetCommand();
				MarkMenuBoundsDirty();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!disposing || _isDisposed)
			{
				base.Dispose(disposing);
				return;
			}
			_isDisposed = true;
			try
			{
				AccessibilityNotifyClients(AccessibleEvents.SystemMenuPopupEnd, 0);
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is Win32Exception)
			{
				// Accessibility clients are optional; closing must continue.
			}
			_exitMessageLoop = true;
			HideSuperTip();
			StopHoverTimer();
			StopChildCloseTimer();
			if (GetRibbon() != null)
			{
				GetRibbon().ClearSuperTipCommand();
				if (base.Controls.Count > 0)
				{
					GetRibbon().InvalidateTabContent();
				}
			}
			Form[] ownedForms = base.OwnedForms;
			foreach (Form form in ownedForms)
			{
				form.Owner = null;
			}
			foreach (Control control in base.Controls)
			{
				control.MouseEnter -= OnControlMouseEnter;
				control.MouseLeave -= OnControlMouseLeave;
			}
			if (_focusedItem != null)
			{
				_focusedItem.Hot = false;
			}
			SetDesignContextMenuRenderer(null);
			if (_sourceItem != null)
			{
				_sourceItem.Active = false;
				if (_sourceItem.GetCommand() is DropDownCommand)
				{
					((DropDownCommand)_sourceItem.GetCommand()).NotifyDropDownClosed();
					if (_sourceItem is RibbonDropDownButtonRenderer)
					{
						((RibbonDropDownButtonRenderer)_sourceItem).DroppedDown = false;
						_sourceItem.Hot = false;
					}
				}
			}
			if (_scrollBar != null)
			{
				_scrollBar.MouseUp -= OnStateMouseUp;
				_scrollBar.MouseDown -= OnStateMouseDown;
				_scrollBar.RemoveScrollHandler(OnScrollBarScroll);
			}
			if (_layoutItems != null)
			{
				foreach (CommandBaseRenderer item in _layoutItems)
				{
					if (item.CommandType == CommandType.Gallery)
					{
						((GalleryMenuRenderer)item).Dispose();
					}
				}
			}
			base.Controls.Clear();
			if (_scrollBar != null)
			{
				_scrollBar.Dispose();
				_scrollBar = null;
			}
			DisposeList();
			if (_shadowWindow != null)
			{
				_shadowWindow.DisposeShadow();
				_shadowWindow = null;
			}
			_owner.SelectedItem = null;
			if (_ownsSuperTip && _superTip != null)
			{
				_superTip.Dispose();
			}
			_superTip = null;
			if (GetChildMenu() != null)
			{
				SetChildMenu(null);
			}
			if (_components != null)
			{
				_components.Dispose();
			}
			_parentCommand = null;
			SetDesignContextMenuRenderer(null);
			_sourceItem = null;
			_commands = null;
			_popupColorScheme = null;
			_focusedItem = null;
			_selectedItem = null;
			_superTipCommand = null;
			if (_layoutItems != null)
			{
				foreach (object layoutItem in _layoutItems)
				{
					if (layoutItem is CommandBaseRenderer)
					{
						((CommandBaseRenderer)layoutItem).Dispose();
					}
				}
				_layoutItems.Clear();
				_layoutItems = null;
			}
			_sections = null;
			if (_owner != null)
			{
				_owner = null;
			}
			base.Events.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			_components = new Container();
			_contentPadding = new Size(4, 4);
			_contentOffsetY = 2;
			_commandSizeStyle = CommandSizeStyle.Small;
		}
	}
}
