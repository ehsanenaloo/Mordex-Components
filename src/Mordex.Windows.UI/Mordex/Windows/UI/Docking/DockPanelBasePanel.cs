using System;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	internal class DockPanelBasePanel : Panel
	{
		private const int SlideInInterval = 25;

		private const int SlideOutInterval = 22;

		private const int InitialSlideStep = 4;

		private const int SlideStepIncrease = 8;

		private const int AutoHideCheckInterval = 1000;

		private const int DefaultWidth = 272;

		private const int DefaultHeight = 248;

		private Container _components = new Container();

		private Timer _slideTimer;

		private Timer _autoHideTimer;

		private DockPanelBase _panel;

		private MxDockManager _dockManager;

		private AutoHideStripDock _autoHideStripDock;

		private Rectangle _openBounds;

		private MxAutoHideStrip _parentStrip;

		private Control _previousParent;

		private DockStyle _previousDock;

		private int _slideStep;

		private bool _wasAutoScroll;

		private bool _isSlidingOut;

		internal DockPanelBasePanel(MxDockManager panelManager, DockPanelBase panel, MxAutoHideStrip parentStrip)
		{
			InitializeDefaults();
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, value: true);
			_parentStrip = parentStrip;
			_panel = panel;
			panel.SetAutoHidePanel(this);
			_dockManager = panelManager;
			_autoHideStripDock = parentStrip.AutoHideDock;
			_openBounds = _dockManager.GetAutoHidePanelBounds(_autoHideStripDock, panel);
		}

		internal void SlideOpen(bool isShowRequested, bool activate)
		{
			_panel.RecalculateLayout();
			_previousParent = RenderingHelper.GetParentControl(_panel);
			_wasAutoScroll = false;
			if (_dockManager.ContainerControl is ScrollableControl)
			{
				_wasAutoScroll = ((ScrollableControl)_dockManager.ContainerControl).AutoScroll;
				((ScrollableControl)_dockManager.ContainerControl).AutoScroll = false;
			}
			_previousDock = _panel.Dock;
			Rectangle bounds = _openBounds;
			switch (_autoHideStripDock)
			{
			case AutoHideStripDock.Bottom:
				base.DockPadding.Top = 1;
				_panel.SetControlDock(DockStyle.Top);
				bounds.Y = bounds.Bottom;
				bounds.Height = 0;
				break;
			case AutoHideStripDock.Left:
				base.DockPadding.Right = 1;
				_panel.SetControlDock(DockStyle.Right);
				bounds.Width = 0;
				break;
			case AutoHideStripDock.Right:
				base.DockPadding.Left = 1;
				_panel.SetControlDock(DockStyle.Left);
				bounds.X = bounds.Right;
				bounds.Width = 0;
				break;
			case AutoHideStripDock.Top:
				base.DockPadding.Bottom = 1;
				_panel.SetControlDock(DockStyle.Bottom);
				bounds.Height = 0;
				break;
			}
			base.Bounds = bounds;
			_dockManager.ContainerControl.Controls.Add(this);
			BringToFront();
			base.Visible = true;
			_panel.Parent = this;
			_panel.Visible = true;
			_panel.Resize += OnPanelResize;
			base.Parent.Resize += OnControlResize;
			if (_dockManager == null || _dockManager.AllowAutoHideAnimation)
			{
				StopSlideTimer();
				_slideTimer = new Timer();
				_slideTimer.Interval = SlideInInterval;
				_slideStep = InitialSlideStep;
				_slideTimer.Tick += OnSlideInTick;
				if (_dockManager != null)
				{
					_dockManager.RaisePanelAutoHideActiveChanged(_panel);
				}
				if (_slideTimer != null)
				{
					_slideTimer.Start();
				}
			}
			else
			{
				if (_dockManager != null)
				{
					_dockManager.RaisePanelAutoHideActiveChanged(_panel);
				}
				_parentStrip.UpdateLayout();
				if (!base.ContainsFocus)
				{
					StartAutoHideTimer();
				}
			}
			if (activate)
			{
				_panel.Activate();
			}
		}

		protected override void OnParentVisibleChanged(EventArgs eventArgs)
		{
			base.OnParentVisibleChanged(eventArgs);
			CloseNow();
		}

		private void OnControlResize(object sender, EventArgs eventArgs)
		{
			CloseNow();
		}

		private void OnSlideInTick(object sender, EventArgs eventArgs)
		{
			_slideStep += SlideStepIncrease;
			Rectangle bounds = base.Bounds;
			bool isFullyOpen = false;
			switch (_autoHideStripDock)
			{
			case AutoHideStripDock.Left:
				bounds.Width += _slideStep;
				break;
			case AutoHideStripDock.Top:
				bounds.Height += _slideStep;
				break;
			case AutoHideStripDock.Right:
				bounds.X -= _slideStep;
				bounds.Width += _slideStep;
				break;
			case AutoHideStripDock.Bottom:
				bounds.Y -= _slideStep;
				bounds.Height += _slideStep;
				break;
			}
			switch (_autoHideStripDock)
			{
			case AutoHideStripDock.Left:
			case AutoHideStripDock.Right:
				if (bounds.Width > _openBounds.Width)
				{
					isFullyOpen = true;
					bounds = _openBounds;
				}
				break;
			case AutoHideStripDock.Top:
			case AutoHideStripDock.Bottom:
				if (bounds.Height > _openBounds.Height)
				{
					isFullyOpen = true;
					bounds = _openBounds;
				}
				break;
			}
			base.Bounds = bounds;
			Update();
			if (isFullyOpen)
			{
				StopSlideTimer();
				if (!base.ContainsFocus)
				{
					StartAutoHideTimer();
				}
			}
		}

		private void StopSlideTimer()
		{
			if (_slideTimer != null)
			{
				Timer timer = _slideTimer;
				_slideTimer = null;
				timer.Stop();
				// The same field holds the slide-in timer and the slide-out timer.
				timer.Tick -= OnSlideInTick;
				timer.Tick -= OnSlideOutTick;
				timer.Dispose();
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			_dockManager.GetPanelRenderer().DrawPanelBorder(eventArgs.Graphics, _panel, base.ClientRectangle);
			base.OnPaint(eventArgs);
		}

		private void OnAutoHideCheckTick(object sender, EventArgs eventArgs)
		{
			try
			{
				if (base.IsDisposed)
				{
					StopAutoHideTimer();
					SlideClose(false);
					return;
				}
				bool shouldHide = true;
				Point pt = PointToClient(Cursor.Position);
				if (!_panel.CanAutoHideCollapse())
				{
					shouldHide = false;
				}
				else if (_panel.Capture || base.ClientRectangle.Contains(pt))
				{
					shouldHide = false;
				}
				else if (_parentStrip != null)
				{
					pt = _parentStrip.PointToClient(Cursor.Position);
					if (_parentStrip.GetPopupTarget(_parentStrip.PanelFromPoint(pt.X, pt.Y)) == _panel)
					{
						shouldHide = false;
					}
				}
				if (shouldHide)
				{
					StopAutoHideTimer();
					SlideClose(true);
				}
			}
			catch
			{
				// The panel or the strip can go away between ticks. Any error stops the check and hides the panel.
				StopAutoHideTimer();
				SlideClose(false);
			}
		}

		private void StopAutoHideTimer()
		{
			if (_autoHideTimer != null)
			{
				Timer timer = _autoHideTimer;
				_autoHideTimer = null;
				timer.Stop();
				timer.Tick -= OnAutoHideCheckTick;
				timer.Dispose();
			}
		}

		private void StartAutoHideTimer()
		{
			if (_autoHideTimer != null)
			{
				StopAutoHideTimer();
			}
			_slideStep = InitialSlideStep;
			_autoHideTimer = new Timer();
			_autoHideTimer.Interval = AutoHideCheckInterval;
			_autoHideTimer.Tick += OnAutoHideCheckTick;
			_autoHideTimer.Start();
		}

		protected override void OnEnter(EventArgs eventArgs)
		{
			StopAutoHideTimer();
			base.OnEnter(eventArgs);
		}

		protected override void OnLeave(EventArgs eventArgs)
		{
			StartAutoHideTimer();
			base.OnLeave(eventArgs);
		}

		protected override void OnSystemColorsChanged(EventArgs eventArgs)
		{
			base.OnSystemColorsChanged(eventArgs);
			Invalidate();
		}

		internal void SlideClose(bool isAnimated)
		{
			if (_isSlidingOut || (GetPanel() != null && !GetPanel().IsAutoHideEffective()))
			{
				return;
			}
			if (_dockManager != null && !base.IsDisposed)
			{
				PanelActionCancelEventArgs e = new PanelActionCancelEventArgs(_panel, cancel: false);
				_dockManager.RaisePanelAutoHideActiveChanging(e);
				if (e.Cancel)
				{
					return;
				}
			}
			if (isAnimated && _dockManager != null && _dockManager.AllowAutoHideAnimation)
			{
				StopSlideTimer();
				_isSlidingOut = true;
				_slideTimer = new Timer();
				_slideTimer.Interval = SlideOutInterval;
				_slideTimer.Tick += OnSlideOutTick;
				_slideTimer.Start();
			}
			else
			{
				CloseNow();
			}
		}

		private void OnSlideOutTick(object sender, EventArgs eventArgs)
		{
			_slideStep += SlideStepIncrease;
			Rectangle bounds = base.Bounds;
			bool isFullyClosed = false;
			switch (_autoHideStripDock)
			{
			case AutoHideStripDock.Left:
				bounds.Width -= _slideStep;
				break;
			case AutoHideStripDock.Top:
				bounds.Height -= _slideStep;
				break;
			case AutoHideStripDock.Right:
				bounds.X += _slideStep;
				bounds.Width -= _slideStep;
				break;
			case AutoHideStripDock.Bottom:
				bounds.Y += _slideStep;
				bounds.Height -= _slideStep;
				break;
			}
			switch (_autoHideStripDock)
			{
			case AutoHideStripDock.Left:
			case AutoHideStripDock.Right:
				if (bounds.Width <= 0)
				{
					isFullyClosed = true;
				}
				break;
			case AutoHideStripDock.Top:
			case AutoHideStripDock.Bottom:
				if (bounds.Height <= 0)
				{
					isFullyClosed = true;
				}
				break;
			}
			if (isFullyClosed)
			{
				CloseNow();
				return;
			}
			base.Bounds = bounds;
			Update();
			Control control = RenderingHelper.GetParentControl(this);
			if (control != null && !control.IsDisposed)
			{
				control.Update();
			}
		}

		private void MoveControlToParent(Control child, Control newParent)
		{
			Control parent = child.Parent;
			if (newParent != parent)
			{
				Form form = null;
				if (child.TopLevelControl is Form)
				{
					form = (Form)child.TopLevelControl;
				}
				if (form != null && (form.ActiveControl == child || child.Contains(form.ActiveControl)))
				{
					child.Hide();
					form.SelectNextControl(child, forward: true, tabStopOnly: true, nested: true, wrap: true);
					child.Show();
				}
				child.Parent = newParent;
			}
		}

		internal void CloseNow()
		{
			try
			{
				base.Visible = false;
				StopAutoHideTimer();
				StopSlideTimer();
				_panel.Resize -= OnPanelResize;
				Control control = RenderingHelper.GetParentControl(this);
				if (control != null)
				{
					control.Resize -= OnControlResize;
				}
				_panel.SetAutoHidePanel(null);
				_isSlidingOut = false;
				_panel.SetControlDock(_previousDock);
				if (_previousParent != null)
				{
					_panel.MoveToParent(_previousParent);
				}
				else
				{
					_panel.MoveToParent(null);
				}
				MoveControlToParent(this, null);
				try
				{
					if (_dockManager.ContainerControl != null && !_dockManager.ContainerControl.IsDisposed && _dockManager.ContainerControl is ScrollableControl && ((ScrollableControl)_dockManager.ContainerControl).AutoScroll != _wasAutoScroll)
					{
						((ScrollableControl)_dockManager.ContainerControl).AutoScroll = _wasAutoScroll;
						_dockManager.ContainerControl.Invalidate(invalidateChildren: true);
					}
				}
				catch
				{
					// The container can be closing. Its scroll state no longer matters, and the panel must still be disposed below.
				}
				Dispose();
				StopAutoHideTimer();
				if (_parentStrip != null)
				{
					_parentStrip.ClearPopupPanel();
				}
				if (_dockManager != null)
				{
					_dockManager.RaisePanelAutoHideActiveChanged(_panel);
				}
			}
			catch
			{
				// Runs from timer ticks and focus changes while the form may be closing.
				// An error here must not reach the message loop.
			}
		}

		internal DockPanelBase GetPanel()
		{
			return _panel;
		}

		private void OnPanelResize(object sender, EventArgs eventArgs)
		{
			switch (_autoHideStripDock)
			{
			case AutoHideStripDock.Left:
				base.Width = _panel.Width + 1;
				break;
			case AutoHideStripDock.Right:
			{
				Rectangle bounds = base.Bounds;
				bounds.X = bounds.Right - (_panel.Width + 1);
				bounds.Width = _panel.Width + 1;
				base.Bounds = bounds;
				break;
			}
			case AutoHideStripDock.Top:
				base.Height = _panel.Height + 1;
				break;
			case AutoHideStripDock.Bottom:
			{
				Rectangle bounds = base.Bounds;
				bounds.Y = bounds.Bottom - (_panel.Height + 1);
				bounds.Height = _panel.Height + 1;
				base.Bounds = bounds;
				break;
			}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopSlideTimer();
				StopAutoHideTimer();
				if (_components != null)
				{
					_components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		private void InitializeDefaults()
		{
			base.ClientSize = new Size(DefaultWidth, DefaultHeight);
			base.Name = "AutoHideContainer";
		}
	}
}
