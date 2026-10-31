using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	[ToolboxItem(false)]
	public class DockPanelInnerContainer : ContainerControl
	{
		private MxDockPanel _ownerPanel;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		public DockPanelInnerContainer()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			HookFocusEvents(this);
		}

		protected override void Select(bool directed, bool forward)
		{
			if (!SelectNextControl(null, forward, tabStopOnly: true, nested: true, wrap: true))
			{
				base.Select(directed, forward);
			}
		}

		internal void SetSelectable(bool isSelectable)
		{
			SetStyle(ControlStyles.Selectable, isSelectable);
		}

		protected override void Dispose(bool disposing)
		{
			foreach (Control control in base.Controls)
			{
				UnhookFocusEvents(control);
			}
			base.Dispose(disposing);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (_ownerPanel != null)
			{
				_ownerPanel.Activate();
			}
			base.OnMouseDown(e);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			Invalidate();
		}

		private void HookFocusEvents(Control parentControl)
		{
			parentControl.GotFocus += OnControlGotFocus;
			parentControl.LostFocus += OnControlLostFocus;
			parentControl.ControlAdded += OnChildControlAdded;
			parentControl.ControlRemoved += OnChildControlRemoved;
			foreach (Control control in parentControl.Controls)
			{
				HookFocusEvents(control);
			}
		}

		private void UnhookFocusEvents(Control parentControl)
		{
			foreach (Control control in parentControl.Controls)
			{
				UnhookFocusEvents(control);
			}
			parentControl.GotFocus -= OnControlGotFocus;
			parentControl.LostFocus -= OnControlLostFocus;
			parentControl.ControlAdded -= OnChildControlAdded;
			parentControl.ControlRemoved -= OnChildControlRemoved;
		}

		private void OnChildControlAdded(object sender, ControlEventArgs eventArgs)
		{
			HookFocusEvents(eventArgs.Control);
		}

		private void OnChildControlRemoved(object sender, ControlEventArgs eventArgs)
		{
			UnhookFocusEvents(eventArgs.Control);
		}

		private void OnControlGotFocus(object sender, EventArgs eventArgs)
		{
			if (_ownerPanel != null)
			{
				_ownerPanel.UpdateIsActive(true);
			}
		}

		private void OnControlLostFocus(object sender, EventArgs eventArgs)
		{
			if (_ownerPanel != null)
			{
				_ownerPanel.UpdateIsActive(base.ContainsFocus);
			}
		}

		internal MxDockPanel GetOwnerPanel()
		{
			return _ownerPanel;
		}

		internal void SetOwnerPanel(MxDockPanel uiPanel)
		{
			_ownerPanel = uiPanel;
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);
			if (_ownerPanel != null)
			{
				switch (_ownerPanel.GetEffectiveInnerAreaStyle())
				{
				case PanelInnerAreaStyle.ContainerPanel:
				case PanelInnerAreaStyle.Window:
				case PanelInnerAreaStyle.AppWorkspace:
					PaintInnerArea(e.Graphics, _ownerPanel.GetEffectiveInnerAreaStyle());
					break;
				default:
				{
					FormatStyle uiFormatStyle = _ownerPanel.GetInnerContainerStyle();
					RenderingHelper.DrawFormatStyleBackground(e.Graphics, uiFormatStyle, base.ClientRectangle, base.ClientRectangle);
					uiFormatStyle.DisposeResources();
					break;
				}
				}
			}
			if (_ownerPanel != null && _ownerPanel.PanelManager == null && base.DesignMode)
			{
				e.Graphics.DrawString("Invalid Panel Use.\n\nA Panel must be associated to a UIPanelManager.", Font, SystemBrushes.ControlText, base.ClientRectangle);
			}
		}

		private void PaintInnerArea(Graphics graphics, PanelInnerAreaStyle style)
		{
			if (_ownerPanel.PanelManager != null)
			{
				DockManagerRendererBase renderer = _ownerPanel.PanelManager.GetPanelRenderer();
				renderer.DrawInnerAreaBackground(graphics, this, base.ClientRectangle, style);
			}
		}
	}
}
