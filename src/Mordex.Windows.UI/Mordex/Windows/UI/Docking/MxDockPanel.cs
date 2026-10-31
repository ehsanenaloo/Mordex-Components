using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	[Designer("Mordex.Windows.UI.Design.DockPanelDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(false)]
	public class MxDockPanel : DockPanelBase, IMordexXmlLayoutsSupport, IFormatStyleOwner
	{
		// Above this many panels in the parent group, size changes are deferred to keep drag resizing fast.
		private const int DeferredResizePanelCount = 10;

		private const int FallbackMinimumSize = 40;

		private delegate void SizeChangedCallback(EventArgs eventArgs);

		private FormatStyle _formatStyleInnerContainer;

		private PanelInnerAreaStyle _innerAreaStyle;

		private DockPanelInnerContainer _innerContainer;

		private bool _isSelectPending;

		private bool _sizeChangedPending;

		[Description("Settings used to draw the inner area when InnerAreaStyle is set to 'UseFormatStyle'.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("InnerContainer Format Settings")]
		public FormatStyle InnerContainerFormatStyle => _formatStyleInnerContainer;

		[Category("InnerContainer Format Settings")]
		[Description("Determines how the inner area is drawn.")]
		[Localizable(false)]
		[DefaultValue(PanelInnerAreaStyle.UseDefault)]
		public PanelInnerAreaStyle InnerAreaStyle
		{
			get
			{
				return _innerAreaStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelInnerAreaStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_innerAreaStyle != value)
				{
					_innerAreaStyle = value;
					_innerContainer.Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public DockPanelInnerContainer InnerContainer
		{
			get
			{
				return _innerContainer;
			}
			set
			{
				if (value == null)
				{
					value = new DockPanelInnerContainer();
				}
				if (_innerContainer == value)
				{
					return;
				}
				if (_innerContainer != null)
				{
					_innerContainer.SetOwnerPanel(null);
					Control control = _innerContainer;
					_innerContainer = null;
					RemoveInnerControl(control);
				}
				_innerContainer = value;
				if (_innerContainer != null)
				{
					if (_innerContainer.GetOwnerPanel() != null)
					{
						_innerContainer.GetOwnerPanel().InnerContainer = null;
					}
					_innerContainer.SetOwnerPanel(this);
					base.Controls.Add(_innerContainer);
				}
			}
		}

		Control IFormatStyleOwner.Owner => this;

		string IMordexXmlLayoutsSupport.Identifier => base.Id.ToString();

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		public MxDockPanel()
			: this("")
		{
		}

		public MxDockPanel(string text)
			: this(text, PanelDockStyle.Left)
		{
		}

		public MxDockPanel(string text, PanelDockStyle dockStyle)
		{
			InitializePanel();
			InitializeControl(Guid.NewGuid());
			Text = text;
			base.DockStyle = dockStyle;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !base.IsDisposed && !base.DesignMode && _innerContainer != null)
			{
				DockPanelInnerContainer innerContainer = _innerContainer;
				_innerContainer = null;
				innerContainer.SetOwnerPanel(null);
				innerContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		internal MxDockPanel(Guid id)
		{
			InitializePanel();
			InitializeControl(id);
		}

		private void InitializePanel()
		{
			InnerContainer = new DockPanelInnerContainer();
			_innerAreaStyle = PanelInnerAreaStyle.UseDefault;
			_formatStyleInnerContainer = new FormatStyle(this);
		}

		internal PanelInnerAreaStyle GetEffectiveInnerAreaStyle()
		{
			if (_innerAreaStyle == PanelInnerAreaStyle.UseDefault)
			{
				if (base.PanelManager != null)
				{
					return base.PanelManager.DefaultPanelSettings.InnerAreaStyle;
				}
				return PanelInnerAreaStyle.UseFormatStyle;
			}
			return _innerAreaStyle;
		}

		internal FormatStyle GetInnerContainerStyle()
		{
			if (base.PanelManager != null)
			{
				FormatStyle managerStyle = base.PanelManager.GetInnerContainerStyle();
				return managerStyle.MergeStyle(_formatStyleInnerContainer);
			}
			return _formatStyleInnerContainer;
		}

		public override void Activate()
		{
			if (!IsUpdateSuspended())
			{
				if (InnerContainer != null && !InnerContainer.ContainsFocus && !InnerContainer.SelectNextControl(null, forward: true, tabStopOnly: true, nested: true, wrap: true) && InnerContainer.Visible && !IsAutoHideEffective())
				{
					InnerContainer.Focus();
				}
			}
			else
			{
				_isSelectPending = true;
			}
			base.Activate();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if (base.IsHandleCreated && (_isLayoutSizing || (base.ParentGroup != null && base.ParentGroup.Panels.Count > DeferredResizePanelCount)) && base.Visible)
			{
				// During a drag resize many SizeChanged events arrive. Post one deferred call and let it handle them all.
				if (!_sizeChangedPending)
				{
					_sizeChangedPending = true;
					SizeChangedCallback method = HandleDeferredSizeChanged;
					BeginInvoke(method, e);
				}
			}
			else
			{
				base.OnSizeChanged(e);
			}
		}

		private void HandleDeferredSizeChanged(EventArgs eventArgs)
		{
			_sizeChangedPending = false;
			if (!base.IsDisposed)
			{
				base.OnSizeChanged(eventArgs);
			}
		}

		protected override void OnEndInitialize()
		{
			base.OnEndInitialize();
			if (_isSelectPending)
			{
				InnerContainer.SetSelectable(true);
				if (InnerContainer != null)
				{
					InnerContainer.Select();
				}
				_isSelectPending = false;
			}
		}

		private void RemoveInnerControl(Control control)
		{
			Form form = RenderingHelper.FindForm(this);
			if (form != null && form.ActiveControl != null && (form.ActiveControl == control || control.Contains(form.ActiveControl)))
			{
				RenderingHelper.TryFocusControl(this);
			}
			if (base.Controls.Contains(control))
			{
				base.Controls.Remove(control);
			}
		}

		protected override void OnParentGroupChanged()
		{
			if (InnerContainer == null)
			{
				return;
			}
			if (base.ParentGroup != null)
			{
				if (base.ParentGroup.SelectedPanel != this && base.ParentGroup.IsTabGroup())
				{
					InnerContainer.SetSelectable(false);
				}
				else
				{
					InnerContainer.SetSelectable(true);
				}
			}
			else
			{
				InnerContainer.SetSelectable(true);
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new DockPanelAccessibleObject(this);
		}

		protected override void OnControlAdded(ControlEventArgs e)
		{
			if (e.Control is DockPanelCaptionContainer)
			{
				return;
			}
			if (!(e.Control is DockPanelInnerContainer))
			{
				if (!base.DesignMode)
				{
					throw new InvalidOperationException("Can not add controls directly to a Panel, use the Controls collection of the InnerContainer instead.");
				}
				_innerContainer.Controls.Add(e.Control);
			}
			else if (e.Control != _innerContainer)
			{
				InnerContainer = (DockPanelInnerContainer)e.Control;
			}
		}

		public override Size GetMinimumSize()
		{
			Size minimumSize = base.MinimumSize;
			Size size = new Size(-1, -1);
			if (base.PanelManager != null)
			{
				size = base.PanelManager.DefaultPanelSettings.MinimumSize;
			}
			if (minimumSize.Width == -1)
			{
				minimumSize.Width = size.Width;
			}
			if (minimumSize.Height == -1)
			{
				minimumSize.Height = size.Height;
			}
			if (minimumSize.Width == -1)
			{
				minimumSize.Width = FallbackMinimumSize;
			}
			if (minimumSize.Height == -1)
			{
				minimumSize.Height = FallbackMinimumSize;
			}
			return minimumSize;
		}

		public override Size GetMaximumSize()
		{
			Size maximumSize = base.MaximumSize;
			Size size = new Size(-1, -1);
			if (base.PanelManager != null)
			{
				size = base.PanelManager.DefaultPanelSettings.MaximumSize;
			}
			if (maximumSize.Width == -1)
			{
				maximumSize.Width = size.Width;
			}
			if (maximumSize.Height == -1)
			{
				maximumSize.Height = size.Height;
			}
			if (maximumSize.Width == -1)
			{
				maximumSize.Width = 0;
			}
			if (maximumSize.Height == -1)
			{
				maximumSize.Height = 0;
			}
			return maximumSize;
		}

		protected override void RefreshLayout()
		{
			if (base.PanelManager == null && _innerContainer != null)
			{
				_innerContainer.Bounds = base.ClientRectangle;
			}
			if (IsUpdateSuspended() || !base.Visible)
			{
				return;
			}
			base.RefreshLayout();
			if (_innerContainer == null)
			{
				return;
			}
			Rectangle bounds = GetInnerAreaBounds();
			if (HasCaptionBorder() && HasCaption && !HasBorderPanel)
			{
				bounds.Y++;
				bounds.Height--;
			}
			if (HasBorderPanel && base.ParentGroup != null && !AutoHide)
			{
				BorderEdges borderEdges = base.ParentGroup.GetChildBorderEdges(this);
				if ((borderEdges & BorderEdges.Bottom) == 0)
				{
					bounds.Height++;
				}
				if ((borderEdges & BorderEdges.Left) == 0)
				{
					bounds.X--;
					bounds.Width++;
				}
				if ((borderEdges & BorderEdges.Right) == 0)
				{
					bounds.Width++;
				}
			}
			_innerContainer.Bounds = bounds;
		}

		void IFormatStyleOwner.PropertyChange()
		{
			RecalculateLayout();
			if (base.ParentGroup != null)
			{
				base.ParentGroup.Invalidate();
			}
			if (base.ContainerCaptionControl != null)
			{
				base.ContainerCaptionControl.Invalidate();
			}
			_innerContainer.Invalidate();
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			Deserialize(reader);
			PropertyValue propertyValue = reader["FormatStyleInnerContainer"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_formatStyleInnerContainer);
			}
			else
			{
				_formatStyleInnerContainer.Reset();
			}
			_innerAreaStyle = (PanelInnerAreaStyle)LayoutReader.GetPropertyValue(reader, "InnerAreaStyle", typeof(PanelInnerAreaStyle), PanelInnerAreaStyle.UseDefault);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			Serialize(info);
			if (!_formatStyleInnerContainer.IsDefaultStyle())
			{
				info.AddValue("FormatStyleInnerContainer", _formatStyleInnerContainer);
			}
			if (_innerAreaStyle != PanelInnerAreaStyle.UseDefault)
			{
				info.AddValue("InnerAreaStyle", _innerAreaStyle);
			}
		}
	}
}
