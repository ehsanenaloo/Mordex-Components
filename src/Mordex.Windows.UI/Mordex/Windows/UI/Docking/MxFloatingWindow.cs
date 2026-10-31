using System;
using System.Drawing;
using System.Security;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxFloatingWindow : Form, IMessageFilter
	{
		private const int BorderWidth = 3;

		private const int TotalBorderSize = BorderWidth * 2;

		private const int SwShowNormal = 1;

		private const int SwShowNoActivate = 4;

		private const int WmSetCursor = 0x20;

		private const int WmMouseActivate = 0x21;

		private const int WmKeyDown = 0x100;

		private const int WmNcLButtonUp = 0xA2;

		private const int WmNcRButtonUp = 0xA5;

		private const int WmNcMButtonUp = 0xA8;

		private const int WmNcXButtonUp = 0xAC;

		private const int WmRButtonUp = 0x205;

		private const int WmMButtonUp = 0x208;

		private const int WmXButtonUp = 0x20C;

		private const int VkEscape = 0x1B;

		private const int MaNoActivateAndEat = 3;

		private Size _initialSize;

		private Form _closeWatchedForm;

		private MxDockManager _manager;

		private DockPanelBase _panel;

		private bool _isUpdatingLayout;

		private Rectangle _resizeStartBounds;

		private Point _resizeStartCursor;

		private ContentAlignment _resizeEdge;

		private bool _isResizing;

		public override Font Font
		{
			get
			{
				if (base.Owner != null)
				{
					return base.Owner.Font;
				}
				return base.Font;
			}
			set
			{
				base.Font = value;
			}
		}

		public override Cursor Cursor
		{
			get
			{
				if (!_panel.CanResize())
				{
					return base.Cursor;
				}
				Point point = PointToClient(Cursor.Position);
				switch (HitTestResizeEdge(point))
				{
				case ContentAlignment.TopLeft:
				case ContentAlignment.BottomRight:
					return Cursors.SizeNWSE;
				case ContentAlignment.TopRight:
				case ContentAlignment.BottomLeft:
					return Cursors.SizeNESW;
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleRight:
					return Cursors.SizeWE;
				case ContentAlignment.TopCenter:
				case ContentAlignment.BottomCenter:
					return Cursors.SizeNS;
				default:
					return base.Cursor;
				}
			}
		}

		public DockPanelBase Panel => _panel;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopResizeFilter();
			}
			if (_closeWatchedForm != null)
			{
				_closeWatchedForm.FormClosed -= OnControlClosed;
				_closeWatchedForm = null;
			}
			if (_manager != null)
			{
				if (_manager.ContainerControl != null)
				{
					_manager.ContainerControl.Disposed -= OnControlDisposed;
				}
				_manager = null;
				_panel = null;
			}
			base.Dispose(disposing);
		}

		internal MxFloatingWindow(MxDockManager manager, DockPanelBase panel)
		{
			_manager = manager;
			base.FormBorderStyle = FormBorderStyle.None;
			if (_manager.ContainerControl != null)
			{
				Form form = RenderingHelper.FindRootForm(_manager);
				if (form != null && form.MdiParent != null)
				{
					form.FormClosed += OnControlClosed;
					_closeWatchedForm = form;
					form = form.MdiParent;
				}
				_manager.ContainerControl.Disposed += OnControlDisposed;
				base.Owner = form;
			}
			_panel = panel;
			DisableAutoScale();
			if (_panel.FloatingLocation != new Point(-1, -1))
			{
				base.StartPosition = FormStartPosition.Manual;
				base.Location = _panel.FloatingLocation;
			}
			else
			{
				base.StartPosition = FormStartPosition.WindowsDefaultLocation;
			}
			panel.SetFloatingWindow(this);
			Size size = base.Size;
			if (_panel != null)
			{
				size = panel.FloatingSize;
				if (size.Width != -1 && size.Height != -1)
				{
					size.Width += TotalBorderSize;
					size.Height += TotalBorderSize;
				}
			}
			_initialSize = size;
			base.Size = size;
			panel.MoveToParent(this);
			panel.SetControlDock(DockStyle.None);
			base.DockPadding.All = BorderWidth;
			base.ShowInTaskbar = false;
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			_panel.Resize += OnPanelResize;
			Text = _panel.Text;
			ApplyLook();
		}

		internal void ShowAtInitialSize()
		{
			if (!_panel.Closed && !_panel.IsDisposed)
			{
				NativeMethods.ShowWindow(base.Handle, SwShowNoActivate);
				if (!base.Visible)
				{
					NativeMethods.ShowWindow(base.Handle, SwShowNormal);
				}
				if (!base.Visible)
				{
					Show();
				}
			}
			base.Size = _initialSize;
		}

		private void OnPanelResize(object sender, EventArgs eventArgs)
		{
			if (!_isUpdatingLayout && sender == _panel && _panel.GetDockState() == PanelDockState.Floating)
			{
				Rectangle bounds = _panel.Bounds;
				bounds.Inflate(BorderWidth, BorderWidth);
				if (base.Bounds.Size != bounds.Size)
				{
					base.Size = bounds.Size;
				}
			}
		}

		protected override void OnMove(EventArgs e)
		{
			try
			{
				if (_panel != null)
				{
					_panel.FloatingLocation = base.Location;
				}
			}
			catch
			{
				// Move events can still arrive while the panel and its layout state are being torn down.
			}
			base.OnMove(e);
		}

		internal void ReplaceHostedPanel(DockPanelBase panel)
		{
			SuspendLayout();
			_panel.Resize -= OnPanelResize;
			_panel.MoveToParent(null);
			_panel.SetFloatingWindow(null);
			_panel = panel;
			if (_panel != null)
			{
				_panel.SetControlDock(DockStyle.None);
				_panel.Bounds = DisplayRectangle;
				_panel.Resize += OnPanelResize;
				_manager.Panels.Add(_panel);
				_panel.FloatingLocation = base.Location;
				_panel.SetFloatingWindow(this);
				_panel.SetDockState(PanelDockState.Floating);
				_panel.MoveToParent(this);
				_panel.ApplySizeLimits();
			}
			ResumeLayout(performLayout: true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				_manager.GetPanelRenderer().DrawFloatingBorder(e.Graphics, _panel, base.ClientRectangle);
			}
			catch
			{
				// The manager is cleared on dispose, and a paint can still be queued. A paint error must not
				// reach the message loop, or it repeats on every repaint.
			}
			base.OnPaint(e);
		}

		internal void FitPanelToWindow()
		{
			_isUpdatingLayout = true;
			if (_panel != null && RenderingHelper.GetParentControl(_panel) == this && _panel.GetDockState() == PanelDockState.Floating)
			{
				_ = base.Size;
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Size = base.Size;
				clientRectangle.Inflate(-BorderWidth, -BorderWidth);
				_panel.Bounds = clientRectangle;
			}
			Invalidate();
			Update();
			_isUpdatingLayout = false;
			_panel.ApplySizeLimits();
		}

		protected override void OnResize(EventArgs e)
		{
			try
			{
				FitPanelToWindow();
				base.OnResize(e);
			}
			catch
			{
				// Resize arrives while the window is created and torn down, when the panel may be missing.
				// The next resize lays it out again.
			}
		}

		private ContentAlignment HitTestResizeEdge(Point pt)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Contains(pt) && !DisplayRectangle.Contains(pt))
			{
				Rectangle rectangle = new Rectangle(clientRectangle.Left, clientRectangle.Top, SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
				if (rectangle.Contains(pt))
				{
					return ContentAlignment.TopLeft;
				}
				rectangle.X = clientRectangle.Right - rectangle.Width;
				if (rectangle.Contains(pt))
				{
					return ContentAlignment.TopRight;
				}
				rectangle.Y = clientRectangle.Bottom - rectangle.Height;
				if (rectangle.Contains(pt))
				{
					return ContentAlignment.BottomRight;
				}
				rectangle.X = clientRectangle.X;
				if (rectangle.Contains(pt))
				{
					return ContentAlignment.BottomLeft;
				}
				if (pt.X < DisplayRectangle.X)
				{
					return ContentAlignment.MiddleLeft;
				}
				if (pt.Y < DisplayRectangle.Y)
				{
					return ContentAlignment.TopCenter;
				}
				if (pt.X > DisplayRectangle.Right)
				{
					return ContentAlignment.MiddleRight;
				}
				if (pt.Y > DisplayRectangle.Bottom)
				{
					return ContentAlignment.BottomCenter;
				}
			}
			return ContentAlignment.MiddleCenter;
		}

		private void DisableAutoScale()
		{
			base.AutoScaleMode = AutoScaleMode.None;
		}

		protected override void OnActivated(EventArgs e)
		{
			SetWindowActive(true);
			if (_panel != null)
			{
				_panel.Invalidate();
			}
			base.OnActivated(e);
		}

		protected override void OnDeactivate(EventArgs e)
		{
			SetWindowActive(false);
			try
			{
				if (_panel != null)
				{
					_panel.Invalidate();
				}
			}
			catch (ObjectDisposedException)
			{
				// The panel can be disposed while its window loses focus.
			}
			base.OnDeactivate(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (!BeginResize(new Point(e.X, e.Y)))
			{
				base.OnMouseDown(e);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_isResizing)
			{
				if (Control.MouseButtons != MouseButtons.None)
				{
					ResizeToCursor(PointToScreen(new Point(e.X, e.Y)));
				}
			}
			else
			{
				UpdateHotEdge(new Point(e.X, e.Y));
				base.OnMouseMove(e);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			UpdateHotEdge(new Point(-1, -1));
			base.OnMouseLeave(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (_isResizing)
			{
				EndResize(false, PointToScreen(new Point(e.X, e.Y)));
			}
			else
			{
				base.OnMouseUp(e);
			}
		}

		private bool BeginResize(Point point)
		{
			if (!_panel.CanResize())
			{
				return false;
			}
			_resizeEdge = HitTestResizeEdge(point);
			if (_resizeEdge == ContentAlignment.MiddleCenter)
			{
				return false;
			}
			_resizeStartBounds = base.Bounds;
			_resizeStartCursor = Cursor.Position;
			if (!_isResizing)
			{
				_isResizing = true;
				Application.AddMessageFilter(this);
			}
			return true;
		}

		private void EndResize(bool isCancelled, Point point)
		{
			if (isCancelled)
			{
				base.Bounds = _resizeStartBounds;
			}
			else
			{
				ResizeToCursor(point);
			}
			_isResizing = false;
			RenderingHelper.SetMouseCapture(this, false);
			Application.RemoveMessageFilter(this);
		}

		private void StopResizeFilter()
		{
			if (_isResizing)
			{
				_isResizing = false;
				Application.RemoveMessageFilter(this);
			}
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			// Closing the window in the middle of a resize drag must not leave the filter eating key presses.
			StopResizeFilter();
			base.OnHandleDestroyed(e);
		}

		private void ResizeToCursor(Point cursorPoint)
		{
			Size size = new Size(SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight);
			Size maxSize = new Size(0, 0);
			if (_panel != null)
			{
				size = _panel.GetMinimumSize();
				maxSize = _panel.GetMaximumSize();
			}
			size.Width += TotalBorderSize;
			size.Height += TotalBorderSize;
			if (maxSize.Width != 0)
			{
				maxSize.Width += TotalBorderSize;
			}
			if (maxSize.Height != 0)
			{
				maxSize.Height += TotalBorderSize;
			}
			Rectangle rectangle = _resizeStartBounds;
			Point point = cursorPoint;
			int newWidth = rectangle.Width;
			int newHeight = rectangle.Height;
			switch (_resizeEdge)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.BottomLeft:
				newWidth += _resizeStartCursor.X - point.X;
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				newWidth += point.X - _resizeStartCursor.X;
				break;
			}
			switch (_resizeEdge)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.TopCenter:
			case ContentAlignment.TopRight:
				newHeight += _resizeStartCursor.Y - point.Y;
				break;
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomRight:
				newHeight += point.Y - _resizeStartCursor.Y;
				break;
			}
			if (maxSize.Width != 0 && newWidth > maxSize.Width)
			{
				newWidth = maxSize.Width;
			}
			if (maxSize.Height != 0 && newHeight > maxSize.Height)
			{
				newHeight = maxSize.Height;
			}
			if (newWidth < size.Width)
			{
				newWidth = size.Width;
			}
			if (newHeight < size.Height)
			{
				newHeight = size.Height;
			}
			switch (_resizeEdge)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.MiddleLeft:
			case ContentAlignment.BottomLeft:
				rectangle.X = rectangle.Right - newWidth;
				rectangle.Width = newWidth;
				break;
			case ContentAlignment.TopRight:
			case ContentAlignment.MiddleRight:
			case ContentAlignment.BottomRight:
				rectangle.Width = newWidth;
				break;
			}
			switch (_resizeEdge)
			{
			case ContentAlignment.TopLeft:
			case ContentAlignment.TopCenter:
			case ContentAlignment.TopRight:
				rectangle.Y = rectangle.Bottom - newHeight;
				rectangle.Height = newHeight;
				break;
			case ContentAlignment.BottomLeft:
			case ContentAlignment.BottomCenter:
			case ContentAlignment.BottomRight:
				rectangle.Height = newHeight;
				break;
			}
			if (base.Bounds != rectangle)
			{
				SuspendLayout();
				base.Bounds = rectangle;
				Invalidate();
				Update();
				_panel.Update();
				_manager.ContainerControl.Update();
			}
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case WmKeyDown:
				if ((int)message.WParam == VkEscape)
				{
					EndResize(true, Point.Empty);
				}
				return true;
			case WmSetCursor:
			case WmNcLButtonUp:
			case WmNcRButtonUp:
			case WmNcMButtonUp:
			case WmNcXButtonUp:
			case WmRButtonUp:
			case WmMButtonUp:
			case WmXButtonUp:
				return true;
			default:
				return false;
			}
		}

		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if (msg == WmMouseActivate)
			{
				if (_panel != null && _panel.GetDesignMode())
				{
					m.Result = (IntPtr)MaNoActivateAndEat;
				}
				else
				{
					base.WndProc(ref m);
				}
			}
			else
			{
				base.WndProc(ref m);
			}
		}

		private void OnControlDisposed(object sender, EventArgs eventArgs)
		{
			Close();
		}

		private void OnControlClosed(object sender, EventArgs eventArgs)
		{
			if (_panel != null)
			{
				_panel.MoveToParent(null);
			}
			Close();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == (Keys.F4 | Keys.Alt))
			{
				if (_panel != null)
				{
					_panel.Closed = true;
				}
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}
	}
}
