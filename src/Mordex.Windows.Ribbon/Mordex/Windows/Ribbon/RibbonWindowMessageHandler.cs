using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonWindowMessageHandler : NativeWindow
	{
		private const int RedrawFrameInvalidate = 0x0401;

		private const int RedrawInvalidateNoChildren = 0x0041;

		private const int RedrawFrameNoChildrenInvalidate = 0x0441;

		private const uint SwpFrameChangedOnly = 0x0033u;

		internal bool _wasMaximized;

		protected Form _form;

		private int _borderWidth;

		protected MxRibbon _ribbon;

		protected MxRibbonFormAdorner _formAdorner;

		protected int _titleBarHeight;

		private NativeMethods.TrackMouseEventData _trackMouseEventData;

		private bool _isTrackingMouseLeave;

		private bool _isActive;

		private RibbonLayoutItem _hotCaptionButton;

		private RibbonLayoutItem _pushedCaptionButton;

		internal BufferedGraphics _frameBuffer;

		internal BufferedGraphics _leftBorderBuffer;

		internal BufferedGraphics _rightBorderBuffer;

		internal BufferedGraphics _bottomBorderBuffer;

		internal Font _font;

		internal bool _hasBuffers;

		internal RibbonHeaderRenderer _headerRenderer;

		internal RibbonLayoutItem _closeButton;

		internal RibbonLayoutItem _maximizeButton;

		internal RibbonLayoutItem _minimizeButton;

		internal RibbonLayoutItem _helpButton;

		internal MxRibbonStatusBar _ribbonStatusBar;

		internal bool _suppressMdiChildActivation;

		internal bool _isAdjustingFormSize;

		private AccessibleObject _cachedAccessibleObject;

		private bool _hasStatusBar;

		internal bool _isFrameRefreshed = true;

		internal static int s_cornerRadius = 7;

		public virtual string DocumentText
		{
			get
			{
				string text = "";
				if (_ribbon != null)
				{
					text = _ribbon.DocumentName;
					if (text.Length > 0 && _ribbon.ApplicationName.Length > 0)
					{
						text += " - ";
					}
				}
				return text;
			}
		}

		public virtual string ApplicationText
		{
			get
			{
				string result = "";
				if (_ribbon != null)
				{
					result = _ribbon.ApplicationName;
				}
				return result;
			}
		}

		public virtual RibbonColorScheme ColorScheme => null;

		internal virtual bool HasStatusBar
		{
			get
			{
				return false;
			}
			set
			{
				if (_hasStatusBar != value)
				{
					_hasStatusBar = value;
					OnFormResize(null, EventArgs.Empty);
					RefreshCaptionLayout();
				}
			}
		}

		public RibbonWindowMessageHandler(Form form, MxRibbon ribbon)
		{
			_form = form;
			_ribbon = ribbon;
			_wasMaximized = form.WindowState == FormWindowState.Maximized;
			_form.ControlAdded += OnFormControlAdded;
			_form.ControlRemoved += OnFormControlRemoved;
			_form.LocationChanged += OnFormLocationChanged;
			if (_ribbon != null)
			{
				_headerRenderer = _ribbon._headerRenderer;
			}
			SetBorderWidth(RibbonDrawingUtils.GetFrameBorderSize().Width);
			if (GetBorderWidth() < 4)
			{
				SetBorderWidth(4);
			}
			_titleBarHeight = SystemInformation.CaptionHeight;
			if (form.FormBorderStyle == FormBorderStyle.FixedToolWindow || form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				_titleBarHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			if (_titleBarHeight < 26)
			{
				_titleBarHeight = 26;
			}
			AssignHandle(form.Handle);
		}

		internal bool IsTopLevelMinimized()
		{
			if (_form != null)
			{
				if (_form.WindowState == FormWindowState.Minimized)
				{
					return !_form.IsMdiChild;
				}
				return false;
			}
			return false;
		}

		protected internal int GetBorderWidth()
		{
			if (_form != null && _form.IsMdiChild && _form.WindowState == FormWindowState.Minimized)
			{
				return 4;
			}
			return _borderWidth;
		}

		protected internal void SetBorderWidth(int borderWidth)
		{
			_borderWidth = borderWidth;
		}

		internal void OnFormLocationChanged(object sender, EventArgs eventArgs)
		{
			if ((_ribbon != null && _ribbon.IsGlassCaptionActive()) || _form.FormBorderStyle == FormBorderStyle.None)
			{
				return;
			}
			if (_form.WindowState != FormWindowState.Maximized && !_wasMaximized)
			{
				if (!IsTopLevelMinimized())
				{
					Rectangle bounds = _form.Bounds;
					bounds.X = 0;
					bounds.Y = 0;
					GraphicsPath graphicsPath = CreateRoundedPath(bounds);
					_form.Region = new Region(graphicsPath);
					graphicsPath.Dispose();
				}
			}
			else
			{
				_form.Region = null;
			}
		}

		internal void OnFormResize(object sender, EventArgs eventArgs)
		{
			if (_maximizeButton != null)
			{
				_maximizeButton.AccessibleName = ((_wasMaximized || _form.WindowState == FormWindowState.Maximized) ? "Restore" : "Maximize");
			}
			if ((_ribbon != null && _ribbon.IsGlassCaptionActive()) || _form.FormBorderStyle == FormBorderStyle.None)
			{
				return;
			}
			if (_form.WindowState != FormWindowState.Maximized && !_wasMaximized)
			{
				if (!IsTopLevelMinimized())
				{
					Rectangle bounds = _form.Bounds;
					bounds.X = 0;
					bounds.Y = 0;
					GraphicsPath graphicsPath = CreateRoundedPath(bounds);
					_form.Region = new Region(graphicsPath);
					graphicsPath.Dispose();
				}
				else if (_form.IsMdiChild)
				{
					_form.Region = null;
				}
			}
			else
			{
				_form.Region = null;
			}
		}

		public MxRibbon GetRibbon()
		{
			return _ribbon;
		}

		public Form GetForm()
		{
			return _form;
		}

		internal virtual void AddOffsetHeight()
		{
			int captionHeight = SystemInformation.CaptionHeight;
			if (_form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.SizableToolWindow)
			{
				captionHeight = SystemInformation.ToolWindowCaptionHeight;
			}
			Size size = RibbonDrawingUtils.GetFrameBorderSize();
			if (_form.FormBorderStyle == FormBorderStyle.FixedDialog || _form.FormBorderStyle == FormBorderStyle.FixedToolWindow || _form.FormBorderStyle == FormBorderStyle.FixedSingle || _form.FormBorderStyle == FormBorderStyle.Fixed3D)
			{
				size = SystemInformation.FixedFrameBorderSize;
			}
			int systemCaptionHeight = captionHeight;
			int topBorderHeight = GetTopBorderHeight();
			if (captionHeight < 26)
			{
				topBorderHeight -= 26 - captionHeight;
				captionHeight = 26;
				topBorderHeight = Math.Max(0, topBorderHeight);
			}
			int systemClientHeight = _form.Height - systemCaptionHeight - size.Height * 2;
			int customClientHeight = _form.Height - GetBottomBorderHeight() - topBorderHeight;
			int systemClientWidth = _form.Width - size.Width * 2;
			int customClientWidth = _form.Width - GetBorderWidth() * 2;
			int newHeight = _form.Height;
			int newWidth = _form.Width;
			if (systemClientHeight != customClientHeight)
			{
				int heightDelta = systemClientHeight - customClientHeight;
				newHeight += heightDelta;
			}
			if (systemClientWidth != customClientWidth)
			{
				int widthDelta = systemClientWidth - customClientWidth;
				newWidth += widthDelta;
			}
			_isAdjustingFormSize = true;
			_form.Size = new Size(newWidth, newHeight);
			_isAdjustingFormSize = false;
		}

		private void OnFormControlRemoved(object sender, ControlEventArgs eventArgs)
		{
			RebindStatusBar();
		}

		private void OnFormControlAdded(object sender, ControlEventArgs eventArgs)
		{
			RebindStatusBar();
		}

		internal void RebindStatusBar()
		{
			if (_ribbonStatusBar != null)
			{
				_ribbonStatusBar.VisibleChanged -= OnRibbonStatusBarVisibleChanged;
				_ribbonStatusBar.RemoveLayoutChangedHandler(OnStatusBarChanged);
				_ribbonStatusBar.SetWindowMessageHandler(null);
			}
			_ribbonStatusBar = null;
			if (_form != null)
			{
				foreach (Control control in _form.Controls)
				{
					if (control.Dock == DockStyle.Bottom && control is MxRibbonStatusBar)
					{
						_ribbonStatusBar = (MxRibbonStatusBar)control;
						break;
					}
				}
			}
			if (_ribbonStatusBar != null)
			{
				_ribbonStatusBar.VisibleChanged += OnRibbonStatusBarVisibleChanged;
				_ribbonStatusBar.AddLayoutChangedHandler(OnStatusBarChanged);
				_ribbonStatusBar.SetWindowMessageHandler(this);
			}
			HasStatusBar = _ribbonStatusBar != null && _ribbonStatusBar.Visible;
		}

		private void OnRibbonStatusBarVisibleChanged(object sender, EventArgs eventArgs)
		{
			RebindStatusBar();
		}

		private void OnStatusBarChanged(object sender, EventArgs eventArgs)
		{
		}

		internal bool IsActive()
		{
			return _isActive;
		}

		internal void SetActive(bool isActive)
		{
			if (_isActive != isActive)
			{
				_isActive = isActive;
				if ((_ribbon != null && !_ribbon.IsInDesignMode()) || (_formAdorner != null && !_formAdorner.IsInDesignMode()))
				{
					Refresh(update: true);
				}
				if (_ribbon != null)
				{
					_ribbon.Invalidate();
				}
			}
		}

		internal AccessibleObject GetAccessibleObject()
		{
			if (_cachedAccessibleObject == null)
			{
				_cachedAccessibleObject = new RibbonTitleBarAccessibleObject(this);
			}
			return _cachedAccessibleObject;
		}

		internal int GetBottomBorderHeight()
		{
			if (_form != null && _form.IsMdiChild && _form.WindowState == FormWindowState.Minimized)
			{
				return 1;
			}
			if (HasStatusBar && _form.FormBorderStyle != FormBorderStyle.None)
			{
				return 2;
			}
			return GetBorderWidth();
		}

		internal int GetTopBorderHeight()
		{
			if (GetBorderWidth() > 4 && (_form.WindowState != FormWindowState.Maximized || _form.IsMdiChild))
			{
				return 4;
			}
			return GetBorderWidth();
		}

		internal bool ShouldShowHelpButton()
		{
			if (_form != null)
			{
				if (!_form.HelpButton)
				{
					return false;
				}
				if (_form.MinimizeBox || _form.MaximizeBox)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		internal bool ShouldShowMinMaxButtons()
		{
			if (_form != null)
			{
				if (!_form.ControlBox)
				{
					return false;
				}
				if (!_form.MinimizeBox && !_form.MaximizeBox)
				{
					return false;
				}
				switch (_form.FormBorderStyle)
				{
				case FormBorderStyle.None:
				case FormBorderStyle.FixedToolWindow:
				case FormBorderStyle.SizableToolWindow:
					return false;
				default:
					return true;
				}
			}
			return true;
		}

		internal bool GetControlBox()
		{
			return _form.ControlBox;
		}

		internal virtual void InitializeDefaults()
		{
		}

		internal void SetHotCaptionButton(RibbonLayoutItem button)
		{
			if (_hotCaptionButton != button)
			{
				if (_hotCaptionButton != null)
				{
					_hotCaptionButton.Hot = false;
				}
				if (_hotCaptionButton != null)
				{
					PaintCaptionButtons(_hotCaptionButton);
				}
				_hotCaptionButton = button;
				if (_hotCaptionButton != null)
				{
					_hotCaptionButton.Hot = true;
				}
				if (_hotCaptionButton != null)
				{
					PaintCaptionButtons(_hotCaptionButton);
				}
			}
		}

		internal void SetPushedCaptionButton(RibbonLayoutItem button)
		{
			if (_pushedCaptionButton != button)
			{
				if (_pushedCaptionButton != null)
				{
					_pushedCaptionButton.Pushed = false;
				}
				if (_pushedCaptionButton != null)
				{
					PaintCaptionButtons(_pushedCaptionButton);
				}
				_pushedCaptionButton = button;
				if (_pushedCaptionButton != null)
				{
					SetHotCaptionButton(null);
					_pushedCaptionButton.Pushed = true;
				}
				if (_pushedCaptionButton != null)
				{
					PaintCaptionButtons(_pushedCaptionButton);
				}
			}
		}

		protected RibbonHeaderRenderer GetHeaderRenderer()
		{
			if (_ribbon != null && _ribbon._headerRenderer.GetFormHandle() != IntPtr.Zero)
			{
				return _ribbon._headerRenderer;
			}
			return null;
		}

		internal virtual void Dispose()
		{
			if (!_form.IsDisposed && !_form.Disposing && !_form.InvokeRequired)
			{
				_form.Region = null;
			}
			_form.LocationChanged -= OnFormLocationChanged;
			_form.ControlAdded -= OnFormControlAdded;
			_form.ControlRemoved -= OnFormControlRemoved;
			ReleaseHandle();
			DisposeResources();
			if (_ribbonStatusBar != null)
			{
				_ribbonStatusBar.VisibleChanged -= OnRibbonStatusBarVisibleChanged;
				_ribbonStatusBar.RemoveLayoutChangedHandler(OnStatusBarChanged);
				_ribbonStatusBar.SetWindowMessageHandler(null);
			}
		}

		internal virtual void SetCaptionBounds(Rectangle clientRect)
		{
		}

		internal void ForwardToBaseWndProc(ref Message message)
		{
			base.WndProc(ref message);
			RefreshCaptionLayout();
			if (GetHeaderRenderer() != null)
			{
				GetHeaderRenderer().RequestLayoutAndRepaint();
				_ribbon.Invalidate();
			}
			Refresh(update: false);
		}

		internal void RefreshCaptionLayout()
		{
			SetCaptionBounds(_form.ClientRectangle);
			DisposeResources();
			InvalidateNonClientArea();
		}

		internal virtual void InvalidateNonClientArea()
		{
			if (_form != null && !_form.IsDisposed && !_form.Disposing)
			{
				try
				{
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, RedrawFrameInvalidate);
				}
				catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
				{
					// The form handle can vanish while the form closes; nothing to redraw then.
				}
			}
		}

		internal void RedrawFrame()
		{
			if (_form == null || _form.IsDisposed || _form.Disposing)
			{
				return;
			}
			try
			{
				NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, RedrawInvalidateNoChildren);
				bool isDesignMode = _ribbon != null && _ribbon.IsInDesignMode();
				if (!isDesignMode)
				{
					isDesignMode = _formAdorner != null && _formAdorner.IsInDesignMode();
				}
				if (!isDesignMode)
				{
					NativeMethods.SetWindowPos(_form.Handle, IntPtr.Zero, 0, 0, 0, 0, SwpFrameChangedOnly);
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
			{
				// The form handle can vanish while the form closes; nothing to redraw then.
			}
		}

		internal virtual void Refresh(bool update)
		{
			if (_form != null && !_form.IsDisposed && !_form.Disposing)
			{
				try
				{
					NativeMethods.RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, RedrawFrameNoChildrenInvalidate);
				}
				catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
				{
					// The form handle can vanish while the form closes; nothing to redraw then.
				}
			}
		}

		public virtual Point PointToClient(Point screenPt)
		{
			int x = _form.Location.X;
			int y = _form.Location.Y;
			if (_form.Parent != null)
			{
				Point point = _form.Parent.PointToScreen(_form.Location);
				x = point.X;
				y = point.Y;
			}
			return new Point(screenPt.X - x, screenPt.Y - y);
		}

		internal void StartMouseLeaveTracking()
		{
			if (!_isTrackingMouseLeave)
			{
				_trackMouseEventData = default(NativeMethods.TrackMouseEventData);
				_trackMouseEventData._flags = 19;
				_trackMouseEventData._size = 12 + IntPtr.Size;
				_trackMouseEventData._windowHandle = _form.Handle;
				_isTrackingMouseLeave = NativeMethods.TrackMouseEvent(ref _trackMouseEventData);
			}
		}

		internal void StopTrackingMouseLeave()
		{
			_isTrackingMouseLeave = false;
		}

		internal virtual void PaintCaptionButtons(RibbonLayoutItem button)
		{
			if (_frameBuffer != null)
			{
				DisposeResources();
				PaintNonClientArea();
			}
			else
			{
				PaintNonClientArea();
			}
		}

		internal void RenderContent(Graphics graphics)
		{
			if (_closeButton != null && _closeButton._isVisible)
			{
				PaintCloseButton(graphics);
			}
			if (_maximizeButton != null && _maximizeButton._isVisible)
			{
				PaintMaximizeButton(graphics);
			}
			if (_minimizeButton != null && _minimizeButton._isVisible)
			{
				PaintMinimizeButton(graphics);
			}
			if (_helpButton != null && _helpButton._isVisible)
			{
				PaintHelpButton(graphics);
			}
		}

		private void PaintHelpButton(Graphics graphics)
		{
			if (_helpButton._bounds.Width > 0 && _helpButton._bounds.Height > 0)
			{
				RibbonChromePainter.DrawCaptionButtonBackgroundFlat(graphics, _helpButton._bounds, ColorScheme, _helpButton.Hot, _helpButton.Pushed);
				RibbonChromePainter.DrawHelpGlyphFlat(graphics, _helpButton._bounds, ColorScheme, IsActive(), _helpButton.Hot, _helpButton.Pushed);
			}
		}

		private void PaintMaximizeButton(Graphics graphics)
		{
			if (_maximizeButton._bounds.Width <= 0 || _maximizeButton._bounds.Height <= 0)
			{
				return;
			}
			RibbonChromePainter.DrawCaptionButtonBackgroundFlat(graphics, _maximizeButton._bounds, ColorScheme, _maximizeButton.Hot, _maximizeButton.Pushed);
			if (_form.WindowState == FormWindowState.Maximized)
			{
				RibbonChromePainter.DrawRestoreGlyphFlat(graphics, _maximizeButton._bounds, ColorScheme, IsActive(), _maximizeButton.Hot, _maximizeButton.Pushed);
			}
			else
			{
				RibbonChromePainter.DrawMaximizeGlyphFlat(graphics, _maximizeButton._bounds, ColorScheme, IsActive(), _maximizeButton.Hot, _maximizeButton.Pushed, _maximizeButton._isEnabled);
			}
		}

		private void PaintMinimizeButton(Graphics graphics)
		{
			if (_minimizeButton._bounds.Width <= 0 || _minimizeButton._bounds.Height <= 0)
			{
				return;
			}
			RibbonChromePainter.DrawCaptionButtonBackgroundFlat(graphics, _minimizeButton._bounds, ColorScheme, _minimizeButton.Hot, _minimizeButton.Pushed);
			if (_form.WindowState == FormWindowState.Minimized)
			{
				RibbonChromePainter.DrawRestoreGlyphFlat(graphics, _minimizeButton._bounds, ColorScheme, IsActive(), _minimizeButton.Hot, _minimizeButton.Pushed);
			}
			else
			{
				RibbonChromePainter.DrawMinimizeGlyphFlat(graphics, _minimizeButton._bounds, ColorScheme, IsActive(), _minimizeButton.Hot, _minimizeButton.Pushed, _minimizeButton._isEnabled);
			}
		}

		private void PaintCloseButton(Graphics graphics)
		{
			if (_closeButton._bounds.Width > 0 && _closeButton._bounds.Height > 0)
			{
				RibbonChromePainter.DrawCloseButtonFlat(graphics, _closeButton._bounds, ColorScheme, IsActive(), _closeButton.Hot, _closeButton.Pushed);
			}
		}

		internal virtual void DrawHeader(Graphics graphics)
		{
		}

		internal void DisposeResources()
		{
			if (_frameBuffer != null)
			{
				_frameBuffer.Dispose();
				_frameBuffer = null;
			}
			if (_leftBorderBuffer != null)
			{
				_leftBorderBuffer.Dispose();
				_leftBorderBuffer = null;
			}
			if (_rightBorderBuffer != null)
			{
				_rightBorderBuffer.Dispose();
				_rightBorderBuffer = null;
			}
			if (_bottomBorderBuffer != null)
			{
				_bottomBorderBuffer.Dispose();
				_bottomBorderBuffer = null;
			}
			_hasBuffers = false;
		}

		internal virtual void CreateBuffers(IntPtr dc)
		{
			if (!(dc != IntPtr.Zero) || _hasBuffers || IsTopLevelMinimized())
			{
				return;
			}
			try
			{
				if (_ribbon != null)
				{
					SetActive(((ICommandRenderHost)_ribbon).IsInDesignMode || _form == Form.ActiveForm);
				}
				else if (_formAdorner != null && _formAdorner.IsInDesignMode())
				{
					SetActive(true);
				}
				else
				{
					SetActive(_form == Form.ActiveForm);
				}
				if (!IsActive() && !_suppressMdiChildActivation && Form.ActiveForm != null && Form.ActiveForm.ActiveMdiChild == _form && _form.MdiParent == Form.ActiveForm)
				{
					SetActive(_form.ContainsFocus);
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
			{
				// Active form state can be unavailable while windows close; treat the window as active.
				SetActive(true);
			}
			NativeMethods.NativeRect i = default(NativeMethods.NativeRect);
			NativeMethods.GetWindowRect(base.Handle, ref i);
			Rectangle rectangle = Rectangle.FromLTRB(i._left, i._top, i._right, i._bottom);
			rectangle.X = 0;
			rectangle.Y = 0;
			if (rectangle.Width == 0 || rectangle.Height == 0)
			{
				return;
			}
			Rectangle targetRectangle = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, _titleBarHeight + GetTopBorderHeight());
			if (targetRectangle.Width > 0 && targetRectangle.Height > 0)
			{
				_frameBuffer = BufferedGraphicsManager.Current.Allocate(dc, targetRectangle);
				PaintFormBorder(_frameBuffer.Graphics, drawCaption: true, rectangle);
				DrawHeader(_frameBuffer.Graphics);
			}
			Rectangle bottomBorderRect = new Rectangle(rectangle.X, rectangle.Bottom - GetBottomBorderHeight(), rectangle.Width, GetBottomBorderHeight());
			if (bottomBorderRect.Width > 0 && bottomBorderRect.Height > 0)
			{
				_bottomBorderBuffer = BufferedGraphicsManager.Current.Allocate(dc, bottomBorderRect);
				if (HasStatusBar)
				{
					PaintStatusBarBorder(_bottomBorderBuffer.Graphics, rectangle);
				}
				else
				{
					PaintFormBorder(_bottomBorderBuffer.Graphics, drawCaption: false, rectangle);
				}
				PaintBottomCorners(_bottomBorderBuffer.Graphics, rectangle);
			}
			Rectangle leftBorderRect = new Rectangle(rectangle.X, targetRectangle.Bottom, GetBorderWidth(), rectangle.Height - targetRectangle.Height - bottomBorderRect.Height);
			if (leftBorderRect.Width > 0 && leftBorderRect.Height > 0)
			{
				_leftBorderBuffer = BufferedGraphicsManager.Current.Allocate(dc, leftBorderRect);
				PaintFormBorder(_leftBorderBuffer.Graphics, drawCaption: false, rectangle);
			}
			Rectangle rightBorderRect = new Rectangle(rectangle.Right - GetBorderWidth(), targetRectangle.Bottom, GetBorderWidth(), leftBorderRect.Height);
			if (rightBorderRect.Width > 0 && rightBorderRect.Height > 0)
			{
				_rightBorderBuffer = BufferedGraphicsManager.Current.Allocate(dc, rightBorderRect);
				PaintFormBorder(_rightBorderBuffer.Graphics, drawCaption: false, rectangle);
			}
			if (rectangle.Width > 0 && rectangle.Height > 0)
			{
				if (_leftBorderBuffer != null)
				{
					PaintBottomCorners(_leftBorderBuffer.Graphics, rectangle);
				}
				if (_rightBorderBuffer != null)
				{
					PaintBottomCorners(_rightBorderBuffer.Graphics, rectangle);
				}
			}
			_hasBuffers = true;
		}

		internal void PaintNonClientArea()
		{
			if ((_form != null && _form.FormBorderStyle == FormBorderStyle.None) || !(base.Handle != IntPtr.Zero))
			{
				return;
			}
			IntPtr windowDc = NativeMethods.GetWindowDC(base.Handle);
			try
			{
				CreateBuffers(windowDc);
				if (_hasBuffers)
				{
					if (_frameBuffer != null)
					{
						_frameBuffer.Render(windowDc);
					}
					if (_leftBorderBuffer != null)
					{
						_leftBorderBuffer.Render(windowDc);
					}
					if (_rightBorderBuffer != null)
					{
						_rightBorderBuffer.Render(windowDc);
					}
					if (_bottomBorderBuffer != null)
					{
						_bottomBorderBuffer.Render(windowDc);
					}
				}
			}
			finally
			{
				NativeMethods.ReleaseDC(base.Handle, windowDc);
			}
		}

		internal void DrawBackground(Graphics graphics, Rectangle rectangle)
		{
			Rectangle rect = new Rectangle(rectangle.X, rectangle.Y + 1, rectangle.Width, rectangle.Height - 1);
			GdiCache.FillGradient(graphics, rect, ColorScheme._activeCaptionGradientFirst, ColorScheme._activeCaptionGradientSecond, LinearGradientMode.Vertical);
		}

		internal void DrawActiveCaptionBackground(Graphics graphics, Rectangle captionRect)
		{
			DrawBackground(graphics, captionRect);
			return;
		}

		internal void DrawInactiveCaptionGradient(Graphics graphics, Rectangle rectangle)
		{
			Rectangle rect = new Rectangle(rectangle.X, rectangle.Y + 1, rectangle.Width, rectangle.Height - 1);
			GdiCache.FillGradient(graphics, rect, ColorScheme._inactiveCaptionGradientFirst, ColorScheme._inactiveCaptionGradientSecond, LinearGradientMode.Vertical);
		}

		internal void DrawInactiveCaptionBackground(Graphics graphics, Rectangle captionRect)
		{
			DrawInactiveCaptionGradient(graphics, captionRect);
			return;
		}

		internal void PaintStatusBarBorder(Graphics graphics, Rectangle rectangle)
		{
			if (_ribbonStatusBar != null && _ribbonStatusBar.Visible)
			{
				_ribbonStatusBar.DrawPanelBackgrounds(graphics, rectangle, GetBorderWidth());
			}
			try
			{
				if (_ribbon != null)
				{
					_isActive = ((ICommandRenderHost)_ribbon).IsInDesignMode || _form == Form.ActiveForm;
				}
				else
				{
					_isActive = _form == Form.ActiveForm;
				}
				if (!_isActive && Form.ActiveForm != null && Form.ActiveForm.ActiveMdiChild == _form && _form.MdiParent == Form.ActiveForm)
				{
					_isActive = true;
				}
			}
			catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
			{
				// Active form state can be unavailable while windows close; treat the window as active.
				_isActive = true;
			}
			Color color = ((!IsActive()) ? ColorScheme._inactiveWindowBorder : ColorScheme._activeWindowBorder);
			Color innerLineColor = Color.FromArgb(100, ColorScheme._activeWindowInnerBorder);
			Pen pen = GdiCache.Pen(color);
			graphics.DrawLine(pen, rectangle.X, rectangle.Y, rectangle.Right, rectangle.Y);
			graphics.DrawLine(pen, rectangle.X, rectangle.Y, rectangle.X, rectangle.Bottom);
			graphics.DrawLine(pen, rectangle.X + 1, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Bottom, rectangle.Right - 1, rectangle.Y);
			Pen innerLinePen = GdiCache.Pen(innerLineColor);
			graphics.DrawLine(innerLinePen, rectangle.X + 1, rectangle.Bottom - 2, rectangle.Right - 2, rectangle.Bottom - 2);
		}

		internal virtual void PaintOffice2010FormBorder(Graphics graphics, bool drawCaption, Rectangle rect)
		{
			Rectangle rectangle = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
			Rectangle captionRect = new Rectangle(rect.X, rect.Y, rect.Width, _titleBarHeight + GetTopBorderHeight());
			Pen pen = null;
			Color color;
			Color innerBorderColor;
			Color frameColor;
			if (IsActive())
			{
				color = ColorScheme._activeWindowBorder;
				innerBorderColor = ColorScheme._activeWindowInnerBorder;
				frameColor = ColorScheme._activeWindowFrame;
				if (drawCaption)
				{
					DrawBackground(graphics, captionRect);
				}
			}
			else
			{
				color = ColorScheme._inactiveWindowBorder;
				innerBorderColor = ColorScheme._inactiveWindowInnerBorder;
				frameColor = ColorScheme._inactiveWindowFrame;
				if (drawCaption)
				{
					DrawInactiveCaptionGradient(graphics, captionRect);
				}
				Rectangle gradientRect = new Rectangle(rectangle.X, rectangle.Y + 1, rectangle.Width, rectangle.Height - 1);
				LinearGradientBrush linearGradientBrush = new LinearGradientBrush(gradientRect, ColorScheme._inactiveCaptionGradientFirst, ColorScheme._inactiveCaptionGradientSecond, LinearGradientMode.Vertical);
				pen = new Pen(linearGradientBrush);
				linearGradientBrush.Dispose();
			}
			Pen borderPen = GdiCache.Pen(color);
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			Pen cornerPen = GdiCache.Pen(color, 3f);
			graphics.DrawArc(cornerPen, rect.X - 1, rect.Y, s_cornerRadius, s_cornerRadius, 180, 90);
			graphics.DrawArc(cornerPen, rect.Right - s_cornerRadius - 1, rect.Y, s_cornerRadius, s_cornerRadius, 270, 90);
			graphics.DrawLine(borderPen, rect.X, rect.Y, rect.Right, rect.Y);
			graphics.DrawLine(borderPen, rect.X, rect.Y, rect.X, rect.Bottom);
			graphics.DrawLine(borderPen, rect.X + 1, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
			graphics.DrawLine(borderPen, rect.Right - 1, rect.Bottom, rect.Right - 1, rect.Y);
			borderPen = GdiCache.Pen(innerBorderColor);
			rect.Inflate(-1, -1);
			cornerPen = GdiCache.Pen(innerBorderColor, 1f);
			graphics.DrawArc(cornerPen, rect.X + 1, rect.Y + 1, 6, 6, 180, 90);
			graphics.DrawArc(cornerPen, rect.Right - 6 - 2, rect.Y + 1, 6, 6, 270, 90);
			graphics.DrawLine(borderPen, rect.X + 3, rect.Y, rect.Right - 4, rect.Y);
			graphics.DrawLine(borderPen, rect.X, rect.Y + 3, rect.X, rect.Bottom);
			graphics.DrawLine(borderPen, rect.X + 1, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
			graphics.DrawLine(borderPen, rect.Right - 1, rect.Bottom, rect.Right - 1, rect.Y + 3);
			graphics.SmoothingMode = smoothingMode;
			rect.Inflate(1, 1);
			borderPen = GdiCache.Pen(frameColor);
			graphics.DrawLine(borderPen, rect.X + 2, rect.Y + 3, rect.X + 2, rect.Bottom - 3);
			graphics.DrawLine(borderPen, rect.Right - 3, rect.Bottom - 3, rect.Right - 3, rect.Y + 3);
			graphics.DrawLine(borderPen, rect.X + 1, rect.Bottom - 3, rect.Right - 2, rect.Bottom - 3);
			if (GetBorderWidth() >= 4)
			{
				for (int i = 0; i <= GetBorderWidth() - 4; i++)
				{
					graphics.DrawLine(borderPen, rect.X + 3 + i, rect.Y + 3, rect.X + 3 + i, rect.Bottom - 4 - i);
					graphics.DrawLine(borderPen, rect.X + 2 + i, rect.Bottom - 4 - i, rect.Right - 3 - i, rect.Bottom - 4 - i);
					graphics.DrawLine(borderPen, rect.Right - 4 - i, rect.Bottom - 4 - i, rect.Right - 4 - i, rect.Y + 3);
				}
			}
			if (pen != null)
			{
				for (int j = 1; j < GetBorderWidth(); j++)
				{
					graphics.DrawLine(pen, rectangle.X + j, rectangle.Y + GetBorderWidth(), rectangle.X + j, rectangle.Y + _titleBarHeight + GetBorderWidth() - 1);
					graphics.DrawLine(pen, rectangle.Right - j, rectangle.Y + GetBorderWidth(), rectangle.Right - j, rectangle.Y + _titleBarHeight + GetBorderWidth() - 1);
				}
				pen.Dispose();
			}
			RenderContent(graphics);
		}

		internal virtual void PaintFormBorder(Graphics graphics, bool drawCaption, Rectangle rect)
		{
			PaintOffice2010FormBorder(graphics, drawCaption, rect);
			return;
		}

		internal void PaintBottomCorners(Graphics graphics, Rectangle rectangle)
		{
			Pen pen = ((!IsActive()) ? new Pen(ColorScheme._inactiveWindowBorder, 3f) : new Pen(ColorScheme._activeWindowBorder, 3f));
			SmoothingMode smoothingMode = graphics.SmoothingMode;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.DrawArc(pen, rectangle.X, rectangle.Bottom - s_cornerRadius - 1, s_cornerRadius, s_cornerRadius, 90, 90);
			graphics.DrawArc(pen, rectangle.Right - s_cornerRadius - 1, rectangle.Bottom - s_cornerRadius - 1, s_cornerRadius, s_cornerRadius, 0, 90);
			graphics.SmoothingMode = smoothingMode;
			pen.Dispose();
		}

		internal GraphicsPath CreateRoundedPath(Rectangle rectangle)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			int halfCorner = s_cornerRadius / 2;
			graphicsPath.AddArc(rectangle.X, rectangle.Y, s_cornerRadius, s_cornerRadius, 180f, 90f);
			graphicsPath.AddLine(rectangle.X + halfCorner, rectangle.Y, rectangle.Right - halfCorner, rectangle.Y);
			graphicsPath.AddArc(rectangle.Right - s_cornerRadius - 1, rectangle.Y, s_cornerRadius, s_cornerRadius, 270f, 90f);
			graphicsPath.AddLine(rectangle.Right, rectangle.Y + halfCorner + 1, rectangle.Right, rectangle.Bottom - halfCorner);
			graphicsPath.AddArc(rectangle.Right - s_cornerRadius - 1, rectangle.Bottom - s_cornerRadius - 1, s_cornerRadius, s_cornerRadius, 0f, 90f);
			graphicsPath.AddLine(rectangle.Right - halfCorner, rectangle.Bottom, rectangle.X + halfCorner, rectangle.Bottom);
			graphicsPath.AddArc(rectangle.X, rectangle.Bottom - s_cornerRadius - 1, s_cornerRadius, s_cornerRadius, 90f, 90f);
			graphicsPath.AddLine(rectangle.X, rectangle.Bottom - halfCorner, rectangle.X, rectangle.Top + halfCorner);
			return graphicsPath;
		}
	}
}
