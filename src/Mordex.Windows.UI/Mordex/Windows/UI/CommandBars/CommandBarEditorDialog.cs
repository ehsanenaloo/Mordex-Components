using System;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	internal class CommandBarEditorDialog : Form, ILayoutItemOwner
	{
		private const int WsClipSiblings = 0x04000000;

		private const int WsClipChildren = 0x02000000;

		private const int WsExToolWindow = 0x80;

		private const int WsExNoActivate = 0x08000000;

		private const int WmActivate = 6;

		private const int WmActivateApp = 28;

		private const int WmSetCursor = 32;

		private const int WmMouseActivate = 33;

		private const int MouseActivateNoActivate = 3;

		private const int SwShowNoActivate = 4;

		private const int SwShowNormal = 1;

		private const uint SwpNoSizeNoActivate = 0x11u;

		private const uint SwpNoSizeNoMoveNoActivate = 0x13u;

		private const int ResizeGripWidth = 3;

		private const int FrameMargin = 6;

		private const float LightBlend = 0.85f;

		private Container _container = new Container();

		internal FormatStyleLayoutState _captionItem;

		private CommandLayoutState _customizeButtonItem;

		private FormatStyleLayoutState _closeButtonItem;

		private ColorSchemeWindowMessageHandler _colorSchemeMessageHandler;

		internal bool _isDisposed;

		private MxCommandBar _commandBar;

		private Form _ownerForm;

		internal ResizeMode _resizeMode;

		internal CommandBarMessageFilter _dragMessageFilter;

		internal Point _dragStart = Point.Empty;

		private bool _isCommandMenuOpen;

		private FormatStyleLayoutState _hotItem;

		private FormatStyleLayoutState _pressedItem;

		private bool _isLayoutDirty = true;

		private VisualStylePainter _themeRenderer;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Style = int.MinValue;
				createParams.Style |= WsClipSiblings;
				createParams.Style |= WsClipChildren;
				createParams.ExStyle += WsExToolWindow;
				if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				{
					createParams.ExStyle += WsExNoActivate;
				}
				else if (Environment.OSVersion.Version.Major > 4)
				{
					createParams.ExStyle += WsExNoActivate;
				}
				return createParams;
			}
		}

		public override Cursor Cursor
		{
			get
			{
				if (Control.MouseButtons == MouseButtons.None && GetCommandBar() != null && !_commandBar._isHostingPopupControl)
				{
					Point point = PointToClient(Cursor.Position);
					if (GetHotItem() != null)
					{
						return base.Cursor;
					}
					if (point.X <= base.ClientRectangle.X + ResizeGripWidth)
					{
						return Cursors.SizeWE;
					}
					if (point.X >= base.ClientRectangle.Right - ResizeGripWidth)
					{
						return Cursors.SizeWE;
					}
					if (point.Y <= base.ClientRectangle.Top + ResizeGripWidth)
					{
						return Cursors.SizeNS;
					}
					if (point.Y >= base.ClientRectangle.Bottom - ResizeGripWidth)
					{
						return Cursors.SizeNS;
					}
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		Control ILayoutItemOwner.ParentControl => this;

		public CommandBarEditorDialog(MxCommandBar commandBar)
		{
			base.ShowInTaskbar = false;
			base.Size = commandBar.FloatingSize;
			_ownerForm = RenderingHelper.FindForm(commandBar.CommandManager.ContainerControl);
			if (_ownerForm != null)
			{
				Font = _ownerForm.Font;
			}
			SetCommandBar(commandBar);
			base.Location = commandBar.FloatingLocation;
			commandBar.Location = Point.Empty;
			InitializeDialog();
			Text = commandBar.Text;
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			GetCommandBar().CommandManager.GetEditorDialogs().AddDialog(this);
			base.Owner = _ownerForm;
			if (GetCommandBar().CommandManager.ContainerControl != null)
			{
				GetCommandBar().CommandManager.ContainerControl.VisibleChanged += OnControlVisibleChanged;
			}
			if (_ownerForm != null)
			{
				_ownerForm.Activated += OnFormActivated;
				_ownerForm.Deactivate += OnFormDeactivate;
			}
		}

		private void OnControlVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (!base.IsDisposed && GetCommandManager() != null && GetCommandManager().ContainerControl != null)
			{
				base.Visible = GetCommandManager().ContainerControl.Visible;
			}
		}

		private void OnFormActivated(object sender, EventArgs eventArgs)
		{
			if (!base.IsDisposed)
			{
				base.Visible = true;
			}
		}

		private void OnFormDeactivate(object sender, EventArgs eventArgs)
		{
			if (!base.IsDisposed && Form.ActiveForm == null && !IsCustomizeMode())
			{
				base.Visible = false;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}
			if (_ownerForm != null)
			{
				_ownerForm.Activated -= OnFormActivated;
				_ownerForm.Deactivate -= OnFormDeactivate;
			}
			if (GetCommandManager() != null && GetCommandManager().ContainerControl != null)
			{
				GetCommandManager().ContainerControl.VisibleChanged -= OnControlVisibleChanged;
			}
			_isDisposed = true;
			if (_commandBar != null)
			{
				try
				{
					if (base.Controls.Contains(_commandBar))
					{
						base.Controls.Remove(_commandBar);
					}
					_commandBar.CommandManager?.GetEditorDialogs().RemoveDialog(this);
				}
				catch (ObjectDisposedException)
				{
					// The bar or its manager is already gone; nothing is left to detach.
				}
			}
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeDialog()
		{
			base.FormBorderStyle = FormBorderStyle.None;
		}

		public MxCommandManager GetCommandManager()
		{
			if (_commandBar != null)
			{
				return _commandBar.CommandManager;
			}
			return null;
		}

		public MxCommandBar GetCommandBar()
		{
			return _commandBar;
		}

		public void SetCommandBar(MxCommandBar uiCommandBar)
		{
			if (_commandBar == uiCommandBar)
			{
				return;
			}
			_commandBar = uiCommandBar;
			if (_commandBar != null)
			{
				if (!base.Controls.Contains(_commandBar))
				{
					base.Controls.Add(_commandBar);
				}
			}
			else
			{
				Dispose();
			}
		}

		internal void ShowAt(Point point)
		{
			MxCommandManager commandManager = GetCommandBar().CommandManager;
			NativeMethods.SetWindowPos(base.Handle, new IntPtr(0), point.X, point.Y, 0, 0, SwpNoSizeNoActivate);
			NativeMethods.ShowWindow(base.Handle, SwShowNoActivate);
			if (!base.Visible)
			{
				NativeMethods.ShowWindow(base.Handle, SwShowNormal);
			}
			if (!base.Visible)
			{
				Show();
			}
			foreach (MxCommandBar commandBar in commandManager.CommandBars)
			{
				if (commandBar.GetChildMenu() != null)
				{
					CommandControl uiCommandControl = commandBar.GetChildMenu();
					NativeMethods.SetWindowPos(uiCommandControl.Handle, new IntPtr(0), 0, 0, 0, 0, SwpNoSizeNoMoveNoActivate);
					while (uiCommandControl.ChildMenu != null)
					{
						uiCommandControl = uiCommandControl.ChildMenu;
						NativeMethods.SetWindowPos(uiCommandControl.Handle, new IntPtr(0), 0, 0, 0, 0, SwpNoSizeNoMoveNoActivate);
					}
					break;
				}
			}
			base.Bounds = base.Bounds;
		}

		protected override void WndProc(ref Message message)
		{
			switch (message.Msg)
			{
			case WmMouseActivate:
				message.Result = (IntPtr)MouseActivateNoActivate;
				break;
			case WmSetCursor:
				if (GetOwnerForm() != null && GetOwnerForm().Modal && base.Enabled && !base.CanFocus)
				{
					base.Enabled = false;
					base.Enabled = true;
				}
				base.WndProc(ref message);
				break;
			case WmActivateApp:
			{
				if (Form.ActiveForm == null && !IsCustomizeMode())
				{
					base.Visible = false;
				}
				else
				{
					base.Visible = true;
				}
				if (base.Visible)
				{
					NativeMethods.BringWindowToTop(base.Handle);
					ShowAt(base.Location);
				}
				base.WndProc(ref message);
				break;
			}
			default:
				base.WndProc(ref message);
				break;
			case WmActivate:
				break;
			}
		}

		public Form GetOwnerForm()
		{
			return _ownerForm;
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (_pressedItem != null && _pressedItem._bounds.Contains(eventArgs.X, eventArgs.Y) && _pressedItem == _closeButtonItem)
			{
				GetCommandBar().SetClosed(true);
			}
			SetPressedItem(null);
			_dragStart = Point.Empty;
			if (_dragMessageFilter != null)
			{
				_dragMessageFilter.EndDrag();
				_dragMessageFilter = null;
			}
		}

		private bool IsCustomizeMode()
		{
			if (GetCommandManager() != null)
			{
				return GetCommandManager()._isCustomizing;
			}
			return false;
		}

		protected override void OnDoubleClick(EventArgs eventArgs)
		{
			base.OnDoubleClick(eventArgs);
			if (_captionItem == null)
			{
				return;
			}
			Point pt = PointToClient(Cursor.Position);
			if (_captionItem._bounds.Contains(pt))
			{
				FormatStyleLayoutState uiFormatStyleLayoutState = FindItemAt(pt.X, pt.Y);
				if (uiFormatStyleLayoutState == null)
				{
					GetCommandBar().RestoreDockedPosition();
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (eventArgs.Button == MouseButtons.Left)
			{
				NativeMethods.SetWindowPos(base.Handle, new IntPtr(0), base.Location.X, base.Location.Y, 0, 0, SwpNoSizeNoActivate);
				FormatStyleLayoutState uiFormatStyleLayoutState = null;
				if (!IsCustomizeMode())
				{
					uiFormatStyleLayoutState = FindItemAt(eventArgs.X, eventArgs.Y);
				}
				if (uiFormatStyleLayoutState != null)
				{
					SetPressedItem(uiFormatStyleLayoutState);
					if (uiFormatStyleLayoutState == _customizeButtonItem)
					{
						Point point = PointToScreen(_customizeButtonItem._bounds.Location);
						_isCommandMenuOpen = true;
						GetCommandBar().ShowCustomizeMenu(_customizeButtonItem, point);
						_isCommandMenuOpen = false;
						Invalidate();
					}
					return;
				}
				GetCommandBar().CommandManager.SetActiveElement(null);
				if (_captionItem._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_dragStart = Cursor.Position;
					_resizeMode = ResizeMode.Floating;
				}
				if (eventArgs.X <= base.ClientRectangle.X + ResizeGripWidth)
				{
					_dragStart = Cursor.Position;
					_resizeMode = ResizeMode.Left;
				}
				else if (eventArgs.X >= base.ClientRectangle.Right - ResizeGripWidth)
				{
					_dragStart = Cursor.Position;
					_resizeMode = ResizeMode.Right;
				}
				else if (eventArgs.Y <= base.ClientRectangle.Top + ResizeGripWidth)
				{
					_dragStart = Cursor.Position;
					_resizeMode = ResizeMode.Top;
				}
				else if (eventArgs.Y >= base.ClientRectangle.Bottom - ResizeGripWidth)
				{
					_dragStart = Cursor.Position;
					_resizeMode = ResizeMode.Bottom;
				}
			}
			else if (eventArgs.Button == MouseButtons.Right && !IsCustomizeMode() && GetCommandBar().CommandManager.ShowQuickCustomizeMenu)
			{
				GetCommandBar().CommandManager.ShowToolbarsMenu(this);
			}
		}

		internal FormatStyleLayoutState GetHotItem()
		{
			return _hotItem;
		}

		internal void SetHotItem(FormatStyleLayoutState hotItem)
		{
			if (_hotItem != hotItem)
			{
				if (_hotItem != null)
				{
					_hotItem.SetHot(false);
				}
				_hotItem = hotItem;
				if (_hotItem != null)
				{
					_hotItem.SetHot(true);
				}
			}
		}

		internal void SetPressedItem(FormatStyleLayoutState pressedItem)
		{
			if (_pressedItem == pressedItem)
			{
				return;
			}
			if (_pressedItem != null)
			{
				_pressedItem.SetPressed(false);
				if (_colorSchemeMessageHandler != null)
				{
					_colorSchemeMessageHandler.DestroyHandle();
					_colorSchemeMessageHandler = null;
				}
			}
			_pressedItem = pressedItem;
			if (_pressedItem != null)
			{
				_pressedItem.SetPressed(true);
			}
		}

		private FormatStyleLayoutState FindItemAt(int x, int y)
		{
			if (_closeButtonItem != null && _closeButtonItem._bounds.Contains(x, y))
			{
				return _closeButtonItem;
			}
			if (_customizeButtonItem != null && _customizeButtonItem._bounds.Contains(x, y))
			{
				return _customizeButtonItem;
			}
			return null;
		}

		protected override void OnMouseLeave(EventArgs eventArgs)
		{
			base.OnMouseLeave(eventArgs);
			SetHotItem(null);
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (!_dragStart.IsEmpty)
			{
				if (_dragMessageFilter == null)
				{
					_dragMessageFilter = CommandBarMessageFilter.StartWindowDrag(_commandBar, _resizeMode, _dragStart, PointToClient(_dragStart));
				}
				_dragMessageFilter.UpdateDrag(Cursor.Position);
			}
			else if (!IsCustomizeMode())
			{
				FormatStyleLayoutState uiFormatStyleLayoutState = FindItemAt(eventArgs.X, eventArgs.Y);
				SetHotItem(uiFormatStyleLayoutState);
			}
		}

		protected override void OnResize(EventArgs eventArgs)
		{
			base.OnResize(eventArgs);
			if (_commandBar != null)
			{
				Size size = base.Size;
				size.Height -= 6;
				size.Width -= 6;
				_commandBar.FloatingSize = size;
			}
			InvalidateLayout();
		}

		internal void InvalidateLayout()
		{
			if (_commandBar != null)
			{
				_commandBar.InvalidateLayout();
			}
			_isLayoutDirty = true;
			Invalidate(invalidateChildren: true);
		}

		private void DrawCaption(Graphics graphics, FormatStyle uiFormatStyle, Color color)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!GetCommandBar().ShouldUseCompatibleTextRendering());
			textLayoutRenderer.SetWordWrap(false);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
			textLayoutRenderer.SetLineAlignment(StringAlignment.Center);
			Brush brush = GdiCache.Brush(color);
			graphics.FillRectangle(brush, _captionItem._bounds);
			brush = null;
			textLayoutRenderer.DrawText(graphics, GetCommandBar().Text, _captionItem._textBounds, uiFormatStyle.ResolveFont(GetCommandBar().Font), uiFormatStyle.GetEffectiveForeColor());
			textLayoutRenderer.ResetCache();
		}

		private void DrawCloseButtonBackground(Graphics graphics, StateStyles uiStateStyles)
		{
			bool isHighlighted = _closeButtonItem.IsHot() || _closeButtonItem.IsPressed();
			bool isPressedAndHot = _closeButtonItem.IsHot() && _closeButtonItem.IsPressed();
			FormatStyle uiFormatStyle = null;
			Image image = null;
			uiFormatStyle = _commandBar.BuildItemFormatStyle(uiStateStyles, isHighlighted, isPressedAndHot, true, false);
			image = RenderingHelper.MakeMagentaTransparent(WindowsResources.Close());
			RenderingHelper.DrawFormatStyleState(graphics, _closeButtonItem, image, uiFormatStyle, isHighlighted, _closeButtonItem.IsPressed());
			image?.Dispose();
			uiFormatStyle.DisposeResources();
		}

		private void DrawCustomizeButtonBackground(Graphics graphics, StateStyles uiStateStyles)
		{
			bool isHot = ((FormatStyleLayoutState)_customizeButtonItem).IsHot();
			bool isPressed = ((FormatStyleLayoutState)_customizeButtonItem).IsPressed() || _isCommandMenuOpen;
			if (isPressed)
			{
				isHot = false;
			}
			FormatStyle uiFormatStyle;
			uiFormatStyle = _commandBar.BuildItemFormatStyle(uiStateStyles, isHot, isPressed, true, false);
			if (isPressed)
			{
				uiFormatStyle.ForeColor = RenderingHelper.GetSchemeColor(ColorName.ControlText);
				Brush controlBrush = GdiCache.Brush(RenderingHelper.GetSchemeColor(ColorName.Control));
				graphics.FillRectangle(controlBrush, _customizeButtonItem._bounds);
			}
			RenderingHelper.DrawFormatStyleState(graphics, _customizeButtonItem, null, uiFormatStyle, isHot, isPressed);
			DrawDropDownArrow(graphics, _customizeButtonItem._bounds.Width, uiFormatStyle.ForeColor);
			uiFormatStyle.DisposeResources();
		}

		private void DrawDropDownArrow(Graphics graphics, int buttonWidth, Color color)
		{
			int arrowWidth = (buttonWidth + 1) / 2;
			if (arrowWidth % 2 == 0)
			{
				arrowWidth--;
			}
			int arrowHeight = arrowWidth / 2 + 1;
			Rectangle bounds = _customizeButtonItem._bounds;
			Pen pen = GdiCache.Pen(color);
			int arrowTop = bounds.Top + (bounds.Height - arrowHeight) / 2;
			int arrowLeft = bounds.Left + (bounds.Width - arrowWidth) / 2;
			while (arrowWidth >= 0)
			{
				if (arrowWidth == 1)
				{
					graphics.DrawLine(pen, arrowLeft, arrowTop - 1, arrowLeft, arrowTop);
				}
				else
				{
					graphics.DrawLine(pen, arrowLeft, arrowTop, arrowLeft + arrowWidth - 1, arrowTop);
				}
				arrowLeft++;
				arrowWidth -= 2;
				arrowTop++;
			}
			pen = null;
		}

		internal void LayoutCaption(Graphics graphics)
		{
			if (_commandBar._isLayoutDirty)
			{
				_commandBar.RecalculateLayout();
			}
			bool ownsGraphics = false;
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			Size size = new Size(FrameMargin, FrameMargin);
			int halfMarginX = size.Width / 2;
			int halfMarginY = size.Height / 2;
			int captionHeight = (int)GetCommandBar().Font.GetHeight() + 5;
			_commandBar.Location = new Point(halfMarginX, halfMarginY + captionHeight + 1);
			base.Size = new Size(_commandBar.Width + size.Width, _commandBar.Height + captionHeight + size.Height + 1);
			_captionItem = new FormatStyleLayoutState();
			_captionItem._bounds = new Rectangle(halfMarginX, halfMarginY, base.Width - size.Width, captionHeight);
			_captionItem._textBounds = _captionItem._bounds;
			int buttonRight = _captionItem._bounds.Right - captionHeight;
			if (GetCommandBar().CanClose())
			{
				_closeButtonItem = new FormatStyleLayoutState(this);
				_closeButtonItem._bounds = new Rectangle(buttonRight, halfMarginY, captionHeight, captionHeight);
				buttonRight -= captionHeight + 1;
			}
			if (_commandBar.ShouldShowCustomizeButton())
			{
				if (_customizeButtonItem == null)
				{
					_customizeButtonItem = new CommandLayoutState(null, "", this);
				}
				_customizeButtonItem._bounds = new Rectangle(buttonRight, halfMarginY, captionHeight, captionHeight);
				buttonRight -= captionHeight;
			}
			else
			{
				_customizeButtonItem = null;
			}
			_captionItem._textBounds.Width = buttonRight - _captionItem._bounds.Left;
			if (ownsGraphics)
			{
				graphics.Dispose();
				graphics = null;
			}
			_isLayoutDirty = false;
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			if (_isLayoutDirty)
			{
				LayoutCaption(eventArgs.Graphics);
			}
			StateStyles uiStateStyles = _commandBar.BuildCommandStateStyles();
			Color controlColor = RenderingHelper.GetSchemeColor(ColorName.Control);
			Color controlDarkColor = RenderingHelper.GetSchemeColor(ColorName.ControlDark);
			Color frameColor = controlDarkColor;
			Color captionColor = frameColor;
			uiStateStyles.FormatStyle.ForeColor = Color.White;
			uiStateStyles.FormatStyle.FontBold = TriState.True;
			if ((_commandBar.IsThemed()))
			{
				_themeRenderer = new VisualStylePainter("Rebar");
				_themeRenderer.DrawRebarBackground(eventArgs.Graphics, base.ClientRectangle, base.ClientRectangle, true, DockStyle.Top);
				frameColor = VisualStylePainter.GetFloatingBarCaptionColor();
				captionColor = frameColor;
				_themeRenderer.DisposeResources();
				_themeRenderer = null;
			}
			else
			{
				Color darkestControlColor = ControlPaint.DarkDark(controlColor);
				frameColor = RenderingHelper.BlendColors(controlDarkColor, darkestControlColor, LightBlend);
			}
			{
				Pen pen = new Pen(frameColor);
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle = RenderingHelper.DrawRectangleOutline(eventArgs.Graphics, pen, clientRectangle);
				clientRectangle = RenderingHelper.DrawRectangleOutline(eventArgs.Graphics, pen, clientRectangle);
				eventArgs.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Top, clientRectangle.Left + 1, clientRectangle.Top + 1);
				eventArgs.Graphics.DrawLine(pen, clientRectangle.Left, clientRectangle.Bottom - 1, clientRectangle.Left + 1, clientRectangle.Bottom);
				eventArgs.Graphics.DrawLine(pen, clientRectangle.Right - 1, clientRectangle.Top, clientRectangle.Right, clientRectangle.Top + 1);
				eventArgs.Graphics.DrawLine(pen, clientRectangle.Right - 1, clientRectangle.Bottom - 1, clientRectangle.Right, clientRectangle.Bottom);
				pen.Dispose();
			}
			DrawCaption(eventArgs.Graphics, uiStateStyles.FormatStyle, captionColor);
			if (_closeButtonItem != null)
			{
				DrawCloseButtonBackground(eventArgs.Graphics, uiStateStyles);
			}
			if (_customizeButtonItem != null)
			{
				DrawCustomizeButtonBackground(eventArgs.Graphics, uiStateStyles);
			}
			if (_themeRenderer != null)
			{
				_themeRenderer.DisposeResources();
				_themeRenderer = null;
			}
			uiStateStyles.FormatStyle.DisposeResources();
		}

		void ILayoutItemOwner.DoDefaultAction(FormatStyleLayoutState item)
		{
			if (item == _customizeButtonItem)
			{
				Point point = PointToScreen(_customizeButtonItem._bounds.Location);
				_isCommandMenuOpen = true;
				GetCommandBar().ShowCustomizeMenu(_customizeButtonItem, point);
				_isCommandMenuOpen = false;
				Invalidate();
			}
			else if (item == _closeButtonItem && GetCommandBar() != null)
			{
				GetCommandBar().SetClosed(true);
			}
		}

		void ILayoutItemOwner.Invalidate(Rectangle rect)
		{
			Invalidate(rect);
		}

		void ILayoutItemOwner.AccessibleStateChanged(FormatStyleLayoutState item)
		{
		}
	}
}
