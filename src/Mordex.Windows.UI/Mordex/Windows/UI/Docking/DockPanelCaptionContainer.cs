using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	[ToolboxItem(false)]
	[Designer("Mordex.Windows.UI.Design.CaptionContainerDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class DockPanelCaptionContainer : ContainerControl
	{
		internal bool _isSettingBounds;

		private DockPanelBase _panel;

		private IContainer _components;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public DockPanelBase Panel
		{
			get
			{
				return _panel;
			}
			set
			{
				_panel = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = value;
			}
		}

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

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
			}
		}

		public DockPanelCaptionContainer()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserMouse, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			CreateComponentContainer();
			HookFocusEvents(this);
		}

		internal DockPanelCaptionContainer(DockPanelBase panelOwner)
			: this()
		{
			_panel = panelOwner;
		}

		internal MxDockManager GetOwnerPanelManager()
		{
			if (_panel != null)
			{
				return _panel.PanelManager;
			}
			return null;
		}

		internal MxDockPanelGroup GetOwnerPanelGroup()
		{
			if (_panel != null)
			{
				return _panel.ParentGroup;
			}
			return null;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (_panel != null)
			{
				_panel.Activate();
			}
			base.OnMouseDown(e);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			Invalidate();
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new DockPanelCaptionContainerAccessibleObject(this);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			PaintCaption(e.Graphics);
			base.OnPaint(e);
		}

		private void PaintCaption(Graphics graphics)
		{
			if (_panel._captionRenderer != null && GetOwnerPanelManager() != null && (GetOwnerPanelGroup() == null || !GetOwnerPanelGroup().PaintChildCaptionBar(graphics, _panel._captionRenderer)))
			{
				_panel._captionRenderer.DrawCaption(graphics, false, -base.Bounds.X, -base.Bounds.Y);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (!_isSettingBounds && _panel != null && !_panel.IsUpdateSuspended())
			{
				_panel.CaptionHeight = base.Height;
			}
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

		private void OnControlGotFocus(object sender, EventArgs eventArgs)
		{
			if (_panel != null)
			{
				_panel.UpdateIsActive(true);
			}
		}

		private void OnControlLostFocus(object sender, EventArgs eventArgs)
		{
			if (_panel != null)
			{
				_panel.UpdateIsActive(base.ContainsFocus);
			}
		}

		private void OnChildControlAdded(object sender, ControlEventArgs eventArgs)
		{
			HookFocusEvents(eventArgs.Control);
		}

		private void OnChildControlRemoved(object sender, ControlEventArgs eventArgs)
		{
			UnhookFocusEvents(eventArgs.Control);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _components != null)
			{
				_components.Dispose();
			}
			foreach (Control control in base.Controls)
			{
				UnhookFocusEvents(control);
			}
			base.Dispose(disposing);
		}

		private void CreateComponentContainer()
		{
			_components = new Container();
		}
	}
}
