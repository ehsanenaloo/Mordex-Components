using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.StatusBar
{
	public partial class MxStatusBar
	{
		[Description("Occurs when the mouse pointer hovers over one of the panels.")]
		public event StatusBarEventHandler PanelHover
		{
			add
			{
				base.Events.AddHandler(_panelHoverKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelHoverKey, value);
			}
		}

		[Description("Occurs when the user clicks on one of the panels.")]
		public event StatusBarEventHandler PanelClick
		{
			add
			{
				base.Events.AddHandler(_panelClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelClickKey, value);
			}
		}

		[Description("Occurs when a panel is double-clicked.")]
		public event StatusBarEventHandler PanelDoubleClick
		{
			add
			{
				base.Events.AddHandler(_panelDoubleClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelDoubleClickKey, value);
			}
		}

		protected virtual void OnPanelHover(StatusBarEventArgs e)
		{
			if (base.Events[_panelHoverKey] is StatusBarEventHandler statusBarEventHandler)
			{
				statusBarEventHandler(this, e);
			}
		}

		protected virtual void OnPanelClick(StatusBarEventArgs e)
		{
			if (base.Events[_panelClickKey] is StatusBarEventHandler statusBarEventHandler)
			{
				statusBarEventHandler(this, e);
			}
		}

		protected virtual void OnPanelDoubleClick(StatusBarEventArgs e)
		{
			if (base.Events[_panelDoubleClickKey] is StatusBarEventHandler statusBarEventHandler)
			{
				statusBarEventHandler(this, e);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Escape && !_gripDragStart.IsEmpty)
			{
				_gripDragStart = Point.Empty;
				if (!_gripDragStartSize.IsEmpty)
				{
					RenderingHelper.GetParentControl(this).Size = _gripDragStartSize;
				}
				_gripDragStartSize = Size.Empty;
			}
			return base.ProcessDialogKey(keyData);
		}

		public MxStatusBarPanel GetPanelAt(int x, int y)
		{
			if (_panels != null)
			{
				foreach (MxStatusBarPanel item in _panels)
				{
					if (item.Visible && item._bounds.Contains(x, y))
					{
						return item;
					}
				}
			}
			return null;
		}

		public MxStatusBarPanel GetPanelAt(Point pt)
		{
			return GetPanelAt(pt.X, pt.Y);
		}

		public MxStatusBarPanel GetPanelAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetPanelAt(point.X, point.Y);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHotPanel(null);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (_gripDragStart != Point.Empty)
			{
				if (base.Capture)
				{
					Point position = Cursor.Position;
					Size size = new Size(_gripDragStart.X - position.X, _gripDragStart.Y - position.Y);
					Control control = RenderingHelper.GetParentControl(this);
					size = (control.Size = new Size(control.Size.Width - size.Width, control.Size.Height - size.Height));
					if (control.Size.Width != size.Width)
					{
						position.X += control.Size.Width - size.Width;
					}
					if (control.Size.Height != size.Height)
					{
						position.Y += control.Size.Height - size.Height;
					}
					_gripDragStart = position;
					control.Update();
				}
				else
				{
					_gripDragStart = Point.Empty;
				}
			}
			else
			{
				MxStatusBarPanel panelAt = GetPanelAt(e.X, e.Y);
				SetHotPanel(panelAt);
			}
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			MxStatusBarPanel panelAt = GetPanelAt();
			if (panelAt != null)
			{
				StatusBarEventArgs panelArgs = new StatusBarEventArgs(panelAt);
				OnPanelClick(panelArgs);
			}
		}

		private void UpdateToolTip(MxStatusBarPanel uiStatusBarPanel)
		{
			if (uiStatusBarPanel == null || (uiStatusBarPanel.ToolTipText != null && uiStatusBarPanel.ToolTipText.Length == 0))
			{
				DisposeResources();
			}
			else
			{
				if (!ShowToolTips)
				{
					return;
				}
				string toolTipText = uiStatusBarPanel.ToolTipText;
				if (VisualStyleSupport.IsToolTipHookSupported())
				{
					if (_toolTipHandler == null)
					{
						_toolTipHandler = new WindowMessageHandler(Font, UseCompatibleTextRendering);
						_toolTipHandler.RequestShow(toolTipText, this);
					}
					else
					{
						_toolTipHandler.UpdateTipText(toolTipText);
					}
					return;
				}
				if (_toolTip == null)
				{
					_toolTip = new Mordex.Windows.Common.ThemedToolTip();
					_toolTip.ShowAlways = true;
				}
				_toolTip.SetToolTip(this, toolTipText);
			}
		}

		internal void SetHotPanel(MxStatusBarPanel uiStatusBarPanel)
		{
			if (_hotPanel != uiStatusBarPanel)
			{
				if (_hotPanel != null)
				{
					_hotPanel.SetIsHot(false);
				}
				_hotPanel = uiStatusBarPanel;
				UpdateToolTip(_hotPanel);
				if (_hotPanel != null)
				{
					_hotPanel.SetIsHot(true);
					StatusBarEventArgs e = new StatusBarEventArgs(_hotPanel);
					OnPanelHover(e);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			_gripDragStart = Point.Empty;
			_gripDragStartSize = Size.Empty;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (_sizeGripState != null && _sizeGripState._bounds.Contains(e.X, e.Y))
			{
				_gripDragStart = Cursor.Position;
				_gripDragStartSize = RenderingHelper.GetParentControl(this).Size;
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WmNcHitTest)
			{
				if (CanShowSizeGrip())
				{
					ControlCollection controls = base.Parent.Controls;
					int count = controls.Count;
					for (int i = 0; i < count; i++)
					{
						Control control = controls[i];
						if (control != this && control.Dock == DockStyle.Bottom && control.Top > base.Top)
						{
							base.WndProc(ref m);
							return;
						}
					}
					Point point = PointToClient(Cursor.Position);
					if (_sizeGripState != null && _sizeGripState._bounds.Contains(point.X, point.Y))
					{
						m.Result = (IntPtr)HtBottomRight;
						return;
					}
				}
			}
			else if (m.Msg == WmActivateApp)
			{
				_needsKeyStateRefresh = true;
				Invalidate();
			}
			base.WndProc(ref m);
		}
	}
}
