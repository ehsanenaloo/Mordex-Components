using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	[ToolboxItem(false)]
	[Designer("Mordex.Windows.UI.Design.DockPanelBaseDesigner, Mordex.Windows.UI.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public abstract partial class DockPanelBase : Control, IDropInfoProvider, IFormatStyleOwner, ISupportInitialize, IDockElement
	{
		private const int DefaultPanelSize = 200;

		private const int DefaultMinimumPanelSize = 40;

		// Space kept free for the container when a docked panel is resized.
		private const int MinimumContainerSpace = 40;

		private const int DefaultSplitterSize = 4;

		private const int NewTabWidth = 40;

		private const int NewTabHeight = 20;

		private const int NewTabIndent = 4;

		// Distance from the panel edge where a drop splits the panel into tiles.
		private const int DropEdgeMargin = 12;

		private const int AutoHideImageIndex = 1;

		private const int DockedImageIndex = 2;

		private object _layoutLoadState;

		private bool _isSettingDock;

		private bool _isAutoHideTabSelected;

		private MxAutoHideStrip _autoHideStrip;

		private Rectangle _dragStartBounds;

		internal DockCaptionRenderer _captionRenderer;

		internal MxFloatingWindow _floatingWindow;

		private Rectangle _splitterBounds;

		private Rectangle _displayBounds;

		private bool _isActive;

		private TabDisplayMode _autoHideTabDisplay;

		private PanelCaptionDisplayMode _captionDisplayMode;

		private PanelCaptionStyle _captionStyle;

		private InheritableBoolean _captionVisible;

		private CaptionDoubleClickAction _captionDoubleClickAction;

		private StateStyles _tabStateStyles;

		private FormatStyle _formatStyleActiveCaption;

		private FormatStyle _formatStyleCaption;

		private FormatStyle _formatStyleInfoText;

		private Image _captionImage;

		private Icon _icon;

		private int _imageIndex;

		private string _imageKey;

		private Image _largeImage;

		private Icon _largeIcon;

		private int _largeImageIndex;

		private string _largeImageKey;

		private Guid _panelId;

		private Color _borderPanelColor;

		private bool _useContainerCaption;

		internal DockPanelCaptionContainer _containerCaptionControl;

		private InheritableBoolean _borderPanel;

		private Color _borderCaptionColor;

		private InheritableBoolean _borderCaption;

		private Size _minimumSize;

		private Size _maximumSize;

		private int _captionHeight;

		private InheritableBoolean _allowResize;

		private InheritableBoolean _allowPanelDrag;

		private InheritableBoolean _allowPanelDrop;

		private InheritableBoolean _closeButtonVisible;

		private InheritableBoolean _autoHideButtonVisible;

		private InheritableBoolean _useActiveCaptionStyle;

		private bool _autoHide;

		private bool _closed;

		private ActiveCaptionMode _activeCaptionMode;

		private PanelDockState _dockState;

		private string _text = "";

		internal bool _isLayoutSizing = true;

		private bool _isInitializing;

		private bool _isNotifyingDockState;

		internal MxDockManager _dockManager;

		private bool _isChangingClosed;

		private ResizeMode _resizeMode;

		private Rectangle _splitterDragArea;

		private Rectangle _splitterTrackBounds;

		private Point _splitterStartLocation;

		private DockMessageFilter _dockMessageFilter;

		private bool _isSplitterTrackErased;

		private Form _parentForm;

		private bool _isUpdateSuspended;

		private WindowMessageHandler _tipWindowHandler;

		private ToolTip _toolTip;

		private Rectangle _innerAreaBounds;

		private DockPanelBasePanel _autoHidePanel;

		private IPanelContainer _parentGroup;

		internal DockLayoutState _dockedLayoutState;

		internal DockLayoutState _floatingLayoutState;

		internal bool _shouldSkipLayoutSync;

		private PanelTextAlignment _textAlignment;

		private PanelImageAlignment _imageAlignment;

		private PanelTextAlignment _infoTextAlignment;

		private string _infoText;

		protected void InitializeControl(Guid id)
		{
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			_textAlignment = PanelTextAlignment.UseDefault;
			_imageAlignment = PanelImageAlignment.UseDefault;
			_infoText = "";
			_panelId = id;
			_dockedLayoutState = new DockLayoutState(this);
			_dockedLayoutState.SetPanelSize(Size);
			_dockedLayoutState.UpdateIsCurrent(true);
			_floatingLayoutState = new DockLayoutState(this);
			_activeCaptionMode = ActiveCaptionMode.UseDefault;
			_captionDoubleClickAction = CaptionDoubleClickAction.Default;
			_useActiveCaptionStyle = InheritableBoolean.Default;
			_captionStyle = PanelCaptionStyle.UseDefault;
			_borderPanelColor = Color.Empty;
			_borderPanel = InheritableBoolean.Default;
			_borderCaption = InheritableBoolean.Default;
			_borderCaptionColor = Color.Empty;
			_formatStyleCaption = new FormatStyle(this);
			_formatStyleActiveCaption = new FormatStyle(this);
			_formatStyleInfoText = new FormatStyle(this);
			_captionHeight = -1;
			_captionVisible = InheritableBoolean.Default;
			_useContainerCaption = false;
			_closeButtonVisible = InheritableBoolean.Default;
			_autoHideButtonVisible = InheritableBoolean.Default;
			_allowPanelDrag = InheritableBoolean.Default;
			_allowPanelDrop = InheritableBoolean.Default;
			base.Dock = System.Windows.Forms.DockStyle.Left;
			_closed = false;
			_minimumSize = new Size(-1, -1);
			_maximumSize = new Size(-1, -1);
			_dockState = PanelDockState.Docked;
			_imageIndex = -1;
			_imageKey = "";
			_captionImage = null;
			_icon = null;
			_largeImageIndex = -1;
			_largeImageKey = "";
			_largeImage = null;
			_largeIcon = null;
			_captionDisplayMode = PanelCaptionDisplayMode.UseDefault;
			_tabStateStyles = new StateStyles(this);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserMouse, value: true);
			SetStyle(ControlStyles.Selectable, value: false);
			SetStyle(ControlStyles.ContainerControl, value: true);
			_shouldSkipLayoutSync = false;
		}

		void IDockElement.CancelSizing()
		{
			EndSplitterDrag(0, 0, true);
		}

		void IDockElement.EraseSizingBar()
		{
			EraseSplitterTrack();
		}

		void IFormatStyleOwner.PropertyChange()
		{
			RefreshLayout();
		}

		void ISupportInitialize.BeginInit()
		{
			if (!_isInitializing)
			{
				_isInitializing = true;
			}
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_isInitializing = false;
				ValidateParent();
				OnEndInitialize();
			}
		}

		internal bool GetIsChangingClosed()
		{
			return _isChangingClosed;
		}

		internal void SetIsChangingClosed(bool isChangingClosed)
		{
			if (_isChangingClosed != isChangingClosed)
			{
				_isChangingClosed = isChangingClosed;
				if (ParentGroup != null)
				{
					ParentGroup.SetIsChangingClosed(isChangingClosed);
				}
			}
		}

		internal bool GetIsAutoHideTabSelected()
		{
			return _isAutoHideTabSelected;
		}

		internal void SetIsAutoHideTabSelected(bool isSelected)
		{
			_isAutoHideTabSelected = isSelected;
		}

		internal MxAutoHideStrip GetAutoHideStrip()
		{
			if (ParentGroup != null && ParentGroup.StaticGroup && ParentGroup.IsTabGroup())
			{
				return ParentGroup.GetAutoHideStrip();
			}
			return _autoHideStrip;
		}

		internal void SetAutoHideStrip(MxAutoHideStrip uiAutoHideStrip)
		{
			_autoHideStrip = uiAutoHideStrip;
		}

		internal PanelCaptionStyle GetEffectiveCaptionStyle()
		{
			if (_captionStyle == PanelCaptionStyle.UseDefault)
			{
				if (PanelManager != null)
				{
					return PanelManager.DefaultPanelSettings.CaptionStyle;
				}
				return PanelCaptionStyle.Standard;
			}
			return _captionStyle;
		}

		private Control GetInnermostActiveControl(Control control)
		{
			if (control is ContainerControl)
			{
				ContainerControl containerControl = (ContainerControl)control;
				if (containerControl.ActiveControl != null)
				{
					return GetInnermostActiveControl(containerControl.ActiveControl);
				}
				return containerControl;
			}
			return control;
		}

		protected override void Select(bool directed, bool forward)
		{
			if (directed)
			{
				SelectNextControl(null, forward, tabStopOnly: true, nested: true, wrap: false);
			}
			else if (!SelectNextControl(null, forward: true, tabStopOnly: true, nested: true, wrap: false))
			{
				Focus();
			}
		}

		internal CaptionDoubleClickAction GetEffectiveCaptionDoubleClickAction()
		{
			if (_captionDoubleClickAction == CaptionDoubleClickAction.Default)
			{
				if (PanelManager != null)
				{
					return PanelManager.DefaultPanelSettings.CaptionDoubleClickAction;
				}
				return CaptionDoubleClickAction.ToggleDockState;
			}
			return _captionDoubleClickAction;
		}

		internal TabDisplayMode GetEffectiveAutoHideTabDisplay()
		{
			if (_autoHideTabDisplay == TabDisplayMode.UseDefault)
			{
				if (PanelManager != null)
				{
					return PanelManager.DefaultPanelSettings.AutoHideTabDisplay;
				}
				return TabDisplayMode.ImageAndTextOnSelected;
			}
			return _autoHideTabDisplay;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !base.IsDisposed && !base.DesignMode && _floatingWindow != null)
			{
				_floatingWindow.Dispose();
				_floatingWindow = null;
			}
			_parentForm = null;
			if (_containerCaptionControl != null)
			{
				DetachContainerCaption();
				if (_containerCaptionControl != null)
				{
					_containerCaptionControl.Dispose();
					_containerCaptionControl = null;
				}
			}
			base.Dispose(disposing);
			DestroyHandle();
		}

		internal bool IsAutoHideEffective()
		{
			if (ParentGroup != null && ParentGroup.IsTabGroup() && ParentGroup.StaticGroup)
			{
				return ParentGroup.IsAutoHideEffective();
			}
			return AutoHide;
		}

		internal bool GetDesignMode()
		{
			return base.DesignMode;
		}

		internal bool HasDragHandle()
		{
			if (ParentGroup != null)
			{
				return ParentGroup.HasChildDragHandle(this);
			}
			return false;
		}

		internal bool CanAutoHideCollapse()
		{
			if (base.ContainsFocus)
			{
				return false;
			}
			if (_resizeMode == ResizeMode.NoResize)
			{
				return true;
			}
			return false;
		}

		internal bool IsRootPanel()
		{
			return ParentGroup == null;
		}

		internal int GetIndexInParent()
		{
			if (_parentGroup == null)
			{
				return -1;
			}
			return _parentGroup.Panels.IndexOf(this);
		}

		internal void SetFloatingWindow(MxFloatingWindow uiFloatingWindow)
		{
			_floatingWindow = uiFloatingWindow;
			RefreshLayout();
		}

		internal bool IsAutoHideButtonShown()
		{
			if (IsFloating() && !AutoHide)
			{
				return false;
			}
			if (DockStyle == PanelDockStyle.Fill)
			{
				return false;
			}
			if (ParentGroup != null && ParentGroup.StaticGroup)
			{
				if (!ParentGroup.IsTabGroup())
				{
					return false;
				}
				return ParentGroup.IsAutoHideButtonShown();
			}
			if (_autoHideButtonVisible == InheritableBoolean.Default)
			{
				if (PanelManager != null)
				{
					return PanelManager.DefaultPanelSettings.AutoHideButtonVisible;
				}
			}
			else if (_autoHideButtonVisible == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal bool IsTextShownAsButton()
		{
			if (ParentGroup != null)
			{
				if (ParentGroup.ChildPanelTextActsAsButton())
				{
					return !IsAutoHideEffective();
				}
				return false;
			}
			return false;
		}

		internal bool IsCloseButtonShown()
		{
			if (ParentGroup != null && ParentGroup.StaticGroup)
			{
				if (!ParentGroup.IsTabGroup())
				{
					return false;
				}
				return ParentGroup.IsCloseButtonShown();
			}
			if (_closeButtonVisible == InheritableBoolean.Default)
			{
				if (PanelManager != null)
				{
					return PanelManager.DefaultPanelSettings.CloseButtonVisible;
				}
			}
			else if (_closeButtonVisible == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		internal bool IsFloatingRoot()
		{
			if (ParentGroup != null)
			{
				return false;
			}
			return _dockState == PanelDockState.Floating;
		}

		internal bool IsFloating()
		{
			return DockState == PanelDockState.Floating;
		}

		protected override void OnResize(EventArgs e)
		{
			if (PanelManager == null || !PanelManager._isDisposing)
			{
				if (_parentForm == null)
				{
					_parentForm = RenderingHelper.FindForm(this);
				}
				if (_parentForm == null || _parentForm.WindowState != FormWindowState.Minimized)
				{
					RecalculateLayout();
				}
				base.OnResize(e);
				if (IsRootPanel() && (_parentForm == null || _parentForm.WindowState != FormWindowState.Minimized))
				{
					GetCurrentLayoutState().GetTopLevelState().SetPanelSize(Size);
				}
			}
			else
			{
				base.OnResize(e);
			}
		}

		protected override void OnLocationChanged(EventArgs e)
		{
			base.OnLocationChanged(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			RecalculateLayout();
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (HasCaption)
			{
				RefreshLayout();
			}
			if (ParentGroup != null)
			{
				ParentGroup.RecalculateLayout();
				if (PanelManager != null && ParentGroup.IsAutoHideEffective())
				{
					PanelManager.LayoutAutoHideStrips();
				}
			}
			if (PanelManager != null && AutoHide)
			{
				PanelManager.LayoutAutoHideStrips();
			}
			_text = Text;
			base.OnTextChanged(e);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			Invalidate(invalidateChildren: true);
		}

		protected virtual void OnPanelManagerChanging()
		{
		}

		protected virtual void OnPanelManagerChanged()
		{
			if (ParentGroup != null && ParentGroup.PanelManager != PanelManager)
			{
				ParentGroup = null;
			}
			if (_autoHide)
			{
				OnAutoHideChanged();
			}
			EndInitializeIfReady();
			UpdateParent();
		}

		protected virtual void OnClosedChanged()
		{
			if (!IsUpdateSuspended())
			{
				PanelManager.RaisePanelClosedChanged(this);
				UpdateVisibility();
			}
		}

		protected virtual bool GetVisibilityCore()
		{
			return !Closed;
		}

		internal void UpdateVisibility()
		{
			bool visibilityCore = GetVisibilityCore();
			if (IsFloatingRoot() && _floatingWindow != null)
			{
				_floatingWindow.Visible = visibilityCore;
				base.Visible = visibilityCore;
				RecalculateLayout();
			}
			else if (!visibilityCore || AutoHide)
			{
				base.Visible = false;
			}
			else
			{
				SyncControlDock();
				base.Visible = true;
			}
			if (_autoHidePanel != null)
			{
				_autoHidePanel.SlideClose(false);
			}
			if (_autoHideStrip != null)
			{
				_autoHideStrip.UpdateLayout();
			}
			if (ParentGroup != null)
			{
				if (ParentGroup.GetAutoHideStrip() != null)
				{
					ParentGroup.GetAutoHideStrip().UpdateLayout();
				}
				ParentGroup.RecalculateLayout();
				ParentGroup.ApplySizeLimits();
			}
		}

		internal void RaiseAutoHideChanged()
		{
			OnAutoHideChanged();
		}

		protected virtual void OnAutoHideChanged()
		{
			if (PanelManager != null)
			{
				PanelManager.PlaceInAutoHideStrip(this, AutoHide);
			}
			UpdateParent();
			UpdateVisibility();
			if (ParentGroup != null)
			{
				ParentGroup.RecalculateLayout();
				ParentGroup.ApplySizeLimits();
				ParentGroup.UpdateParent();
			}
			else
			{
				RecalculateLayout();
			}
			if (AutoHide && PanelManager != null)
			{
				PanelManager.LayoutAutoHideStrips();
			}
		}

		internal bool CanDragPanel()
		{
			if (AutoHide)
			{
				return false;
			}
			if (ParentGroup != null && ParentGroup.StaticGroup)
			{
				return false;
			}
			if (_allowPanelDrag == InheritableBoolean.Default)
			{
				if (PanelManager == null)
				{
					return false;
				}
				return PanelManager.AllowPanelDrag;
			}
			if (_allowPanelDrag == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool CanDropPanel()
		{
			if (Closed || AutoHide)
			{
				return false;
			}
			switch (_allowPanelDrop)
			{
			case InheritableBoolean.False:
				return false;
			case InheritableBoolean.True:
				return true;
			default:
				if (PanelManager == null)
				{
					return false;
				}
				return PanelManager.AllowPanelDrop;
			}
		}

		internal bool CanChangeClosed()
		{
			if (PanelManager != null)
			{
				return PanelManager.RaisePanelClosedChanging(this);
			}
			return true;
		}

		internal bool CanChangeAutoHide()
		{
			if (PanelManager != null)
			{
				return PanelManager.RaisePanelAutoHideChanging(this);
			}
			return true;
		}

		private void AttachContainerCaption()
		{
			if (_containerCaptionControl == null)
			{
				_containerCaptionControl = new DockPanelCaptionContainer(this);
				_containerCaptionControl.MouseDown += OnContainerCaptionControlMouseDown;
				_containerCaptionControl.MouseUp += OnContainerCaptionControlMouseUp;
				_containerCaptionControl.MouseMove += OnContainerCaptionControlMouseMove;
				_containerCaptionControl.DoubleClick += OnContainerCaptionControlDoubleClick;
				_containerCaptionControl.MouseLeave += OnContainerCaptionControlMouseLeave;
			}
			if (!Controls.Contains(_containerCaptionControl))
			{
				Controls.Add(_containerCaptionControl);
			}
			if (base.Container == null || _containerCaptionControl.Container != null)
			{
				return;
			}
			string name = base.Name + "CaptionContainer";
			try
			{
				base.Container.Add(ContainerCaptionControl, name);
			}
			catch
			{
				base.Container.Add(ContainerCaptionControl);
				if (ContainerCaptionControl.Site != null)
				{
					ContainerCaptionControl.Name = ContainerCaptionControl.Site.Name;
				}
			}
		}

		private void DetachContainerCaption()
		{
			if (_containerCaptionControl != null && Controls.Contains(_containerCaptionControl))
			{
				_containerCaptionControl.MouseDown -= OnContainerCaptionControlMouseDown;
				_containerCaptionControl.MouseUp -= OnContainerCaptionControlMouseUp;
				_containerCaptionControl.MouseMove -= OnContainerCaptionControlMouseMove;
				_containerCaptionControl.DoubleClick -= OnContainerCaptionControlDoubleClick;
				_containerCaptionControl.MouseLeave -= OnContainerCaptionControlMouseLeave;
				if (base.DesignMode)
				{
					_containerCaptionControl.Dispose();
					_containerCaptionControl = null;
				}
				else
				{
					Controls.Remove(_containerCaptionControl);
				}
			}
		}

		internal void SuspendPanelUpdate()
		{
			_isUpdateSuspended = true;
		}

		internal void ResumePanelUpdate()
		{
			_isUpdateSuspended = false;
			UpdateParent();
			RecalculateLayout();
		}

		internal void DisposeToolTips()
		{
			if (_tipWindowHandler != null)
			{
				_tipWindowHandler.HideAndDisposeTimers();
				_tipWindowHandler = null;
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
		}

		internal void ApplyToolTipText(string caption)
		{
			if (caption.Length <= 0 || base.DesignMode)
			{
				DisposeToolTips();
			}
			else if (VisualStyleSupport.IsToolTipHookSupported())
			{
				if (_tipWindowHandler == null)
				{
					bool useCompatibleTextRendering = false;
					if (PanelManager != null)
					{
						useCompatibleTextRendering = PanelManager.UseCompatibleTextRendering;
					}
					_tipWindowHandler = new WindowMessageHandler(Font, useCompatibleTextRendering);
					_tipWindowHandler.SetIsPersistent(false);
					_tipWindowHandler.RequestShow(caption, this);
				}
				else
				{
					_tipWindowHandler.UpdateTipText(caption);
				}
			}
			else
			{
				if (_toolTip == null)
				{
					_toolTip = new Mordex.Windows.Common.ThemedToolTip();
				}
				_toolTip.SetToolTip(this, caption);
			}
		}

		internal bool IsUpdateSuspended()
		{
			if (_isUpdateSuspended)
			{
				return true;
			}
			if (PanelManager == null)
			{
				return true;
			}
			return PanelManager.IsInitializing();
		}

		internal void EndInitializeIfReady()
		{
			if (!IsUpdateSuspended())
			{
				OnEndInitialize();
			}
		}

		protected virtual void OnEndInitialize()
		{
			RecalculateLayout();
			UpdateVisibility();
		}

		internal DockPanelBasePanel GetAutoHidePanel()
		{
			return _autoHidePanel;
		}

		internal void SetAutoHidePanel(DockPanelBasePanel uiPanelBasePanel)
		{
			_autoHidePanel = uiPanelBasePanel;
		}

		private void ValidateParent()
		{
			Control control = RenderingHelper.GetParentControl(this);
			if (control == _autoHidePanel)
			{
				return;
			}
			if (control != null && PanelManager == null && control != ParentGroup)
			{
				throw new InvalidOperationException("A '" + GetType().Name + "' can not act as a standalone control.\n\r\rThe control must be associated with a UIPanelManager before adding it to a control.");
			}
			if (PanelManager == null || control == null)
			{
				return;
			}
			string message = "The Parent control can only be the ContainerControl of its PanelManager or its ParentGroup.";
			if (ParentGroup != null)
			{
				if (control != ParentGroup)
				{
					throw new InvalidOperationException(message);
				}
			}
			else if (control != _floatingWindow && control != PanelManager.ContainerControl)
			{
				throw new InvalidOperationException(message);
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			if (!IsUpdateSuspended())
			{
				ValidateParent();
			}
			base.OnParentChanged(e);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			RefreshLayout();
		}

		internal bool CanResize()
		{
			switch (_allowResize)
			{
			case InheritableBoolean.False:
				return false;
			case InheritableBoolean.True:
				return true;
			default:
				if (PanelManager != null)
				{
					return PanelManager.AllowPanelResize;
				}
				return true;
			}
		}

		internal PanelCaptionDisplayMode GetEffectiveCaptionDisplayMode()
		{
			if (_captionDisplayMode == PanelCaptionDisplayMode.UseDefault)
			{
				if (PanelManager != null)
				{
					return PanelManager.DefaultPanelSettings.CaptionDisplayMode;
				}
				return PanelCaptionDisplayMode.ImageAndText;
			}
			return _captionDisplayMode;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			UpdateIsActive(true);
			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			UpdateIsActive(base.ContainsFocus);
			base.OnLostFocus(e);
		}

		internal void UpdateIsActive(bool isActive)
		{
			if (_isActive != isActive)
			{
				_isActive = isActive;
				if (PanelManager != null)
				{
					PanelManager.OnPanelActiveChanged(this);
				}
				Invalidate(invalidateChildren: true);
				for (DockPanelBase parentGroup = ParentGroup; parentGroup != null; parentGroup = parentGroup.ParentGroup)
				{
					parentGroup.Invalidate(invalidateChildren: true);
				}
			}
		}

		internal IPanelContainer GetParentContainer()
		{
			return _parentGroup;
		}

		internal void SetParentContainer(IPanelContainer parentGroup)
		{
			if (_parentGroup != parentGroup)
			{
				if (_parentGroup != null)
				{
					IPanelContainer previousGroup = _parentGroup;
					_parentGroup = parentGroup;
					previousGroup.Panels.Remove(this);
				}
				_parentGroup = parentGroup;
				if (_parentGroup is MxDockPanelGroup)
				{
					ParentGroup = (MxDockPanelGroup)_parentGroup;
					ApplyDockState(ParentGroup.DockState);
				}
				else
				{
					ParentGroup = null;
				}
				if (_parentGroup == null && _autoHideStrip != null)
				{
					_autoHideStrip.RemoveForeignPanels();
				}
				OnParentGroupChanged();
				UpdateParent();
				SyncLayoutStateParent();
			}
		}
	}
}
