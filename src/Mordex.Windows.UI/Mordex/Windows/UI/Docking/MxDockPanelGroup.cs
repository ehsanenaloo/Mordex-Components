using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	[DefaultEvent("SelectedPanelChanged")]
	[ToolboxItem(false)]
	public class MxDockPanelGroup : DockPanelBase, IDropInfoProvider, IMordexXmlLayoutsSupport, IPanelContainer
	{
		private delegate void SizeChangedCallback(EventArgs eventArgs);

		private const int DefaultSplitterSize = 4;

		private bool _staticGroup;

		private DockPanelCollection _panels;

		internal DockPanelGroupState _groupState;

		private bool _isGroupLayoutSuspended;

		private bool _isAutoClosed;

		private PanelGroupStyle _groupStyle;

		private int _visibleOutlookButtons;

		private bool _showOutlookNavigatorConfigureMenu;

		private TabDisplayMode _tabDisplay;

		private TabAlignment _tabAlignment;

		private bool _allowOutlookButtonsResize;

		private int _splitterSize;

		private static readonly object _selectedPanelChangedKey = new object();

		private static readonly object _selectedPanelChangingKey = new object();

		[Description("Determines whether the 'Configure Buttons' menu is shown in an Outlook Navigator group.")]
		[Localizable(true)]
		[DefaultValue(true)]
		public bool ShowOutlookNavigatorConfigureMenu
		{
			get
			{
				return _showOutlookNavigatorConfigureMenu;
			}
			set
			{
				if (_showOutlookNavigatorConfigureMenu != value)
				{
					_showOutlookNavigatorConfigureMenu = value;
					if (_groupStyle == PanelGroupStyle.OutlookNavigator)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[Localizable(true)]
		[Description("Determines how many panel buttons are visible in an Outlook Navigator group.")]
		[DefaultValue(-1)]
		public int VisibleOutlookButtons
		{
			get
			{
				return _visibleOutlookButtons;
			}
			set
			{
				if (_visibleOutlookButtons != value)
				{
					_visibleOutlookButtons = value;
					if (_groupStyle == PanelGroupStyle.OutlookNavigator)
					{
						RefreshLayout();
					}
				}
			}
		}

		[Description("Determines whether the visible buttons in an Outlook Navigator group can be resized.")]
		[DefaultValue(true)]
		[Localizable(true)]
		public bool AllowOutlookButtonsResize
		{
			get
			{
				return _allowOutlookButtonsResize;
			}
			set
			{
				if (_allowOutlookButtonsResize != value)
				{
					_allowOutlookButtonsResize = value;
					if (_groupStyle == PanelGroupStyle.OutlookNavigator)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[Localizable(true)]
		[Description("determines whether the panel group should be static and not allow users to remove its panels or to add new panels to it.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public bool StaticGroup
		{
			get
			{
				if (_staticGroup)
				{
					return true;
				}
				if (base.ParentGroup != null)
				{
					return base.ParentGroup._staticGroup;
				}
				return _staticGroup;
			}
			set
			{
				if (_staticGroup != value)
				{
					_staticGroup = value;
					_dockedLayoutState.SetIsStatic(_staticGroup);
					_floatingLayoutState.SetIsStatic(_staticGroup);
					if (StaticGroup)
					{
						SetIsAutoClosed(false);
					}
					RecalculateLayout();
					if (!IsUpdateSuspended() && base.PanelManager != null)
					{
						base.PanelManager.RemoveRedundantGroups();
					}
				}
			}
		}

		protected override bool HasCaption
		{
			get
			{
				if (_staticGroup)
				{
					if (!IsTabGroup())
					{
						return base.HasCaption;
					}
					return false;
				}
				if (_groupState != null)
				{
					return _groupState.HasCaption;
				}
				return false;
			}
		}

		protected override bool HasBorderPanel
		{
			get
			{
				if (_groupState != null && _groupState.IsTabLayout && GetAutoHidePanel() != null)
				{
					return false;
				}
				if (base.BorderPanel == InheritableBoolean.True)
				{
					return true;
				}
				if (base.BorderPanel == InheritableBoolean.False)
				{
					return false;
				}
				return _groupState.HasBorderPanel;
			}
		}

		[Description("Determines whether the panel group is visible.")]
		public override bool Closed
		{
			get
			{
				if (StaticGroup || _panels.Count == 0)
				{
					return base.Closed;
				}
				if (_isAutoClosed)
				{
					return true;
				}
				return base.Closed;
			}
			set
			{
				if (StaticGroup)
				{
					base.Closed = value;
				}
				else
				{
					if (value == Closed)
					{
						return;
					}
					bool closed = Closed;
					if (!CanChangeClosed())
					{
						return;
					}
					SuspendGroupLayout();
					ArrayList arrayList = new ArrayList();
					foreach (DockPanelBase item in (IEnumerable)Panels)
					{
						arrayList.Add(item);
					}
					foreach (DockPanelBase childPanel in arrayList)
					{
						childPanel.Closed = value;
					}
					ResumeGroupLayout();
					if (closed != Closed)
					{
						OnClosedChanged();
					}
				}
			}
		}

		[Description("Determines whether the panel should automatically hide itself when it is not activated.")]
		[Localizable(true)]
		public override bool AutoHide
		{
			get
			{
				if (StaticGroup)
				{
					return base.AutoHide;
				}
				foreach (DockPanelBase item in (IEnumerable)_panels)
				{
					if (!item.AutoHide)
					{
						return false;
					}
				}
				return _panels.Count > 0;
			}
			set
			{
				if (StaticGroup || IsUpdateSuspended())
				{
					base.AutoHide = value;
				}
				else
				{
					if (value == AutoHide)
					{
						return;
					}
					bool autoHide = AutoHide;
					if (!CanChangeAutoHide())
					{
						return;
					}
					foreach (DockPanelBase item in (IEnumerable)Panels)
					{
						item.AutoHide = value;
						item.UpdateVisibility();
					}
					if (autoHide != AutoHide)
					{
						if (base.PanelManager != null)
						{
							base.PanelManager.RaisePanelAutoHideChanged(this);
						}
						OnAutoHideChanged();
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DockPanelCollection Panels => _panels;

		[Browsable(false)]
		[DefaultValue(null)]
		public DockPanelBase SelectedPanel
		{
			get
			{
				if (IsTabGroup())
				{
					return _groupState.GetSelectedPanel();
				}
				return null;
			}
			set
			{
				_groupState.SetSelectedPanel(value);
			}
		}

		[Description("Determines the style of the UIPanelGroup.")]
		[DefaultValue(PanelGroupStyle.HorizontalTiles)]
		public PanelGroupStyle GroupStyle
		{
			get
			{
				return _groupStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(PanelGroupStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_groupStyle != value)
				{
					if (_groupState != null)
					{
						_groupState.Dispose();
					}
					_groupStyle = value;
					_dockedLayoutState.SetGroupStyle(_groupStyle);
					_floatingLayoutState.SetGroupStyle(_groupStyle);
					_groupState = CreateGroupState(_groupStyle);
					RecalculateLayout();
				}
			}
		}

		[Localizable(false)]
		[Description("Determines how the tab, representing the panel in an AutoHideStrip, is displayed.")]
		[DefaultValue(TabDisplayMode.UseDefault)]
		public TabDisplayMode TabDisplay
		{
			get
			{
				return _tabDisplay;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_tabDisplay != value)
				{
					_tabDisplay = value;
					if (_groupStyle == PanelGroupStyle.Tab)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[Description("Indicates the area of the control where the tabs are aligned.")]
		[DefaultValue(TabAlignment.UseDefault)]
		[Localizable(false)]
		public TabAlignment TabAlignment
		{
			get
			{
				return _tabAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_tabAlignment != value)
				{
					_tabAlignment = value;
					if (_groupStyle == PanelGroupStyle.Tab)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[Description("Specifies the width of the splitter bar in a tiled panel group.")]
		[Localizable(true)]
		[DefaultValue(-1)]
		public int SplitterSize
		{
			get
			{
				return _splitterSize;
			}
			set
			{
				if (_splitterSize != value)
				{
					_splitterSize = value;
					if (!IsTabGroup())
					{
						RecalculateLayout();
					}
				}
			}
		}

		public override Cursor Cursor
		{
			get
			{
				if (!AutoHide || base.AutoHideActive)
				{
					Point pt = PointToClient(Cursor.Position);
					Cursor cursor = _groupState.GetCursor(pt);
					if (cursor != null)
					{
						return cursor;
					}
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => base.Id.ToString();

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		DockPanelCollection IPanelContainer.Panels => Panels;

		MxDockManager IPanelContainer.PanelManager => base.PanelManager;

		public event PanelActionCancelEventHandler SelectedPanelChanging
		{
			add
			{
				base.Events.AddHandler(_selectedPanelChangingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedPanelChangingKey, value);
			}
		}

		public event PanelActionEventHandler SelectedPanelChanged
		{
			add
			{
				base.Events.AddHandler(_selectedPanelChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_selectedPanelChangedKey, value);
			}
		}

		public MxDockPanelGroup()
		{
			InitializeGroup();
			InitializeControl(Guid.NewGuid());
			GetCurrentLayoutState().SetGroupStyle(_groupStyle);
		}

		public MxDockPanelGroup(PanelGroupStyle groupStyle)
		{
			InitializeGroup();
			InitializeControl(Guid.NewGuid());
			GroupStyle = groupStyle;
		}

		internal MxDockPanelGroup(Guid id)
		{
			InitializeGroup();
			InitializeControl(id);
			GetCurrentLayoutState().SetGroupStyle(_groupStyle);
		}

		private void InitializeGroup()
		{
			_visibleOutlookButtons = -1;
			_showOutlookNavigatorConfigureMenu = true;
			_allowOutlookButtonsResize = true;
			_splitterSize = -1;
			_staticGroup = false;
			_panels = new DockPanelCollection(this, setOwner: true);
			_groupStyle = PanelGroupStyle.HorizontalTiles;
			_groupState = CreateGroupState(_groupStyle);
			_tabDisplay = TabDisplayMode.UseDefault;
			_tabAlignment = TabAlignment.UseDefault;
			SetIsAutoClosed(true);
		}

		private DockPanelGroupState CreateGroupState(PanelGroupStyle panelGroupStyle)
		{
			switch (panelGroupStyle)
			{
			case PanelGroupStyle.VerticalTiles:
				return new DockTileSplitterFilter(this, DockTileSplitterFilter.TileOrientation.Vertical);
			case PanelGroupStyle.HorizontalTiles:
				return new DockTileSplitterFilter(this, DockTileSplitterFilter.TileOrientation.Horizontal);
			case PanelGroupStyle.Tab:
				return new TabbedGroupState(this);
			case PanelGroupStyle.OfficeTaskPane:
				return new OfficeTaskPaneGroupState(this);
			case PanelGroupStyle.OutlookNavigator:
				return new OutlookNavigatorGroupState(this);
			default:
				return null;
			}
		}

		internal Rectangle GetNewTabHeaderBounds()
		{
			return _groupState.GetNewTabBounds();
		}

		protected override void OnEndInitialize()
		{
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				item.EndInitializeIfReady();
			}
			base.OnEndInitialize();
			if (!IsTabGroup())
			{
				return;
			}
			if (_panels.Count > 0)
			{
				foreach (DockPanelBase childPanel in (IEnumerable)Panels)
				{
					if (!childPanel.Closed && childPanel != SelectedPanel && childPanel is MxDockPanel)
					{
						((MxDockPanel)childPanel).InnerContainer.SetSelectable(false);
					}
				}
			}
			if (SelectedPanel != null)
			{
				RaiseSelectedPanelChanged();
			}
		}

		internal int GetGroupCaptionHeight()
		{
			if (HasCaption && _captionRenderer != null)
			{
				return _captionRenderer.GetCaptionBounds().Height;
			}
			return 0;
		}

		internal bool ChildPanelTextActsAsButton()
		{
			return _groupState.TreatChildPanelTextAsButton();
		}

		internal BorderEdges GetChildBorderEdges(DockPanelBase childPanel)
		{
			return _groupState.GetChildPanelEdges(childPanel);
		}

		internal Point GetTabLocation(DockTabItem tabLayout)
		{
			Point location = tabLayout.GetBounds().Location;
			if (_groupState is TabbedGroupState tabState)
			{
				location.Offset(tabState._tabStrip.GetBounds().Location);
			}
			return location;
		}

		internal bool IsGroupLayoutSuspended()
		{
			if (_isGroupLayoutSuspended)
			{
				return true;
			}
			if (base.ParentGroup != null)
			{
				return base.ParentGroup.IsGroupLayoutSuspended();
			}
			return false;
		}

		protected override void RefreshLayout()
		{
			if (base.IsDisposed || base.PanelManager == null || IsUpdateSuspended())
			{
				return;
			}
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				item.RecalculateLayout();
			}
			_groupState.RefreshVisiblePanels();
			base.RefreshLayout();
			if (IsGroupLayoutSuspended())
			{
				return;
			}
			bool isEmpty = _groupState.GetVisiblePanels().Count == 0;
			if (IsAutoClosed() != isEmpty)
			{
				SetIsAutoClosed(isEmpty);
				if (Closed)
				{
					return;
				}
			}
			SuspendLayout();
			_groupState.RefreshVisiblePanels();
			_groupState.RefreshGroupLayout();
			InvalidateNonChildArea();
			ResumeLayout(performLayout: true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !base.IsDisposed && !base.DesignMode)
			{
				base.Controls.Clear();
			}
			base.Dispose(disposing);
		}

		private bool IsAutoClosed()
		{
			return _isAutoClosed;
		}

		private void SetIsAutoClosed(bool isAutoClosed)
		{
			if ((!isAutoClosed || !StaticGroup) && _isAutoClosed != isAutoClosed)
			{
				_isAutoClosed = isAutoClosed;
				OnClosedChanged();
			}
		}

		public void SuspendGroupLayout()
		{
			_isGroupLayoutSuspended = true;
		}

		public void ResumeGroupLayout()
		{
			_isGroupLayoutSuspended = false;
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				if (item is MxDockPanelGroup)
				{
					((MxDockPanelGroup)item).ResumeGroupLayout();
				}
			}
			RefreshLayout();
			ApplySizeLimits();
		}

		protected override bool GetVisibilityCore()
		{
			if (!Closed)
			{
				if (StaticGroup)
				{
					return true;
				}
				return _panels.Count > 0;
			}
			return false;
		}

		public void SelectPanel(DockPanelBase panel, bool giveFocus)
		{
			_groupState.SelectPanel(panel, giveFocus);
		}

		internal TabAlignment GetEffectiveTabAlignment()
		{
			if (_tabAlignment == TabAlignment.UseDefault)
			{
				if (base.PanelManager != null)
				{
					return base.PanelManager.DefaultPanelSettings.TabAlignment;
				}
				return TabAlignment.Bottom;
			}
			return _tabAlignment;
		}

		internal TabDisplayMode GetEffectiveTabDisplay()
		{
			if (_tabDisplay == TabDisplayMode.UseDefault)
			{
				if (base.PanelManager != null)
				{
					return base.PanelManager.DefaultPanelSettings.TabDisplay;
				}
				return TabDisplayMode.ImageAndText;
			}
			return _tabDisplay;
		}

		internal bool HasChildDragHandle(DockPanelBase uiPanelBase)
		{
			if (_groupStyle == PanelGroupStyle.OfficeTaskPane)
			{
				return CanDragPanel();
			}
			return false;
		}

		public bool ContainsPanel(DockPanelBase panel)
		{
			if (panel == this)
			{
				return true;
			}
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				if (panel == item)
				{
					return true;
				}
				if (item is MxDockPanelGroup && ((MxDockPanelGroup)item).ContainsPanel(panel))
				{
					return true;
				}
			}
			return false;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if (!GetIsChangingClosed() && base.IsHandleCreated && (_isLayoutSizing || (base.ParentGroup != null && !base.ParentGroup.GetIsChangingClosed() && base.ParentGroup.ParentGroup == null)) && base.Visible)
			{
				SizeChangedCallback method = HandleDeferredSizeChanged;
				BeginInvoke(method, e);
			}
			else
			{
				base.OnSizeChanged(e);
			}
		}

		private void HandleDeferredSizeChanged(EventArgs eventArgs)
		{
			base.OnSizeChanged(eventArgs);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (base.PanelManager != null)
			{
				_groupState.ProcessPaint(e);
			}
			base.OnPaint(e);
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			if (_groupState.ProcessDoubleClick())
			{
				base.OnDoubleClick(e);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (_groupState.ProcessMouseDown(e))
			{
				base.OnMouseDown(e);
			}
		}

		private void SetUnselectedTabsSelectable(bool isSelectable)
		{
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				if (item is MxDockPanel)
				{
					if (item != SelectedPanel)
					{
						((MxDockPanel)item).InnerContainer.SetSelectable(isSelectable);
					}
				}
				else if (item != SelectedPanel)
				{
					((MxDockPanelGroup)item).SetUnselectedTabsSelectable(isSelectable);
				}
				else
				{
					((MxDockPanelGroup)item).SetUnselectedTabsSelectable(true);
				}
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (IsTabGroup() && (keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift)))
			{
				SetUnselectedTabsSelectable(false);
				return base.ProcessDialogKey(keyData);
			}
			return base.ProcessDialogKey(keyData);
		}

		internal int GetEffectiveSplitterSize()
		{
			if (_splitterSize < 0)
			{
				if (base.PanelManager != null)
				{
					return base.PanelManager.SplitterSize;
				}
				return DefaultSplitterSize;
			}
			return _splitterSize;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_groupState.ProcessMouseMove(e))
			{
				base.OnMouseMove(e);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			_groupState.ProcessMouseMove(new MouseEventArgs(MouseButtons.None, 0, -1, -1, 0));
			base.OnMouseLeave(e);
		}

		public override void Activate()
		{
			base.Activate();
			if (SelectedPanel != null)
			{
				SelectedPanel.Activate();
			}
			else if (_groupState.GetVisiblePanels().Count > 0)
			{
				((DockPanelBase)_groupState.GetVisiblePanels()[0]).Activate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (_groupState.ProcessMouseUp(e))
			{
				base.OnMouseUp(e);
			}
		}

		internal bool IsTabGroup()
		{
			return _groupState.IsTabLayout;
		}

		public DockPanelBase GetPanelAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetPanelAt(point.X, point.Y);
		}

		public DockPanelBase GetPanelAt(Point pt)
		{
			return GetPanelAt(pt.X, pt.Y);
		}

		public DockPanelBase GetPanelAt(int x, int y)
		{
			if (_groupState != null)
			{
				if (_groupState is OutlookNavigatorGroupState)
				{
					OutlookPanelButton buttonLayout = ((OutlookNavigatorGroupState)_groupState).HitTestButton(x, y, true);
					if (buttonLayout != null)
					{
						return buttonLayout.GetPanel();
					}
				}
				else if (_groupState is TabbedGroupState)
				{
					DockTabItem tabLayout = ((TabbedGroupState)_groupState).HitTestTab(x, y);
					if (tabLayout != null)
					{
						return tabLayout.GetPanel();
					}
				}
			}
			return null;
		}

		protected override void OnPanelManagerChanged()
		{
			base.OnPanelManagerChanged();
		}

		internal bool HandleCloseButtonClick(DockPanelBase panel)
		{
			if (StaticGroup && IsTabGroup())
			{
				if (base.PanelManager != null)
				{
					base.PanelManager.HideAutoHidePanels();
				}
				Closed = true;
				return true;
			}
			return _groupState.ProcessCloseButtonClick(panel);
		}

		internal void HandleChildCaptionButtonDown(DockPanelBase panel, Rectangle buttonBounds)
		{
			_groupState.ProcessCaptionButtonDown(panel, buttonBounds);
		}

		internal bool HandleCaptionDoubleClick(DockPanelBase panel)
		{
			if (base.ParentGroup != null && base.ParentGroup.StaticGroup)
			{
				return base.ParentGroup.HandleCaptionDoubleClick(this);
			}
			if (GetEffectiveCaptionDoubleClickAction() == CaptionDoubleClickAction.ToggleDockState && !AutoHide && (base.ParentGroup == null || !base.ParentGroup.StaticGroup))
			{
				return _groupState.ProcessCaptionDoubleClick(panel);
			}
			return true;
		}

		internal bool HandleAutoHideButtonClick(DockPanelBase panel)
		{
			if (StaticGroup && IsTabGroup())
			{
				AutoHide = !AutoHide;
				return true;
			}
			return _groupState.ProcessAutoHideButtonClick(panel);
		}

		protected override void OnDockPanel()
		{
			if (StaticGroup)
			{
				ApplyDockState(PanelDockState.Docked);
				if (base.PanelManager != null)
				{
					base.PanelManager.MoveLayoutState(base.Id, PanelDockState.Floating, PanelDockState.Docked);
				}
				return;
			}
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				item.EnsureDocked();
			}
			_groupState.RefreshVisiblePanels();
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new DockPanelGroupAccessibleObject(this);
		}

		protected override void OnUndockPanel()
		{
			if (!StaticGroup && !IsTabGroup())
			{
				foreach (DockPanelBase item in (IEnumerable)_panels)
				{
					item.EnsureFloating();
				}
				return;
			}
			ApplyDockState(PanelDockState.Floating);
			if (base.PanelManager != null)
			{
				base.PanelManager.MoveLayoutState(base.Id, PanelDockState.Docked, PanelDockState.Floating);
			}
		}

		public override Size GetMinimumSize()
		{
			Size minimumSize = base.MinimumSize;
			Size stateMinimumSize = _groupState.MinimumSize;
			if (minimumSize.Width < stateMinimumSize.Width)
			{
				minimumSize.Width = stateMinimumSize.Width;
			}
			if (minimumSize.Height < stateMinimumSize.Height)
			{
				minimumSize.Height = stateMinimumSize.Height;
			}
			return minimumSize;
		}

		public override Size GetMaximumSize()
		{
			Size maximumSize = base.MaximumSize;
			Size stateMaximumSize = _groupState.MaximumSize;
			if (maximumSize.Width == -1 || (maximumSize.Width > stateMaximumSize.Width && stateMaximumSize.Width > 0))
			{
				maximumSize.Width = stateMaximumSize.Width;
			}
			if (maximumSize.Height == -1 || (maximumSize.Height > stateMaximumSize.Height && stateMaximumSize.Height > 0))
			{
				maximumSize.Height = stateMaximumSize.Height;
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

		internal bool ShouldDragWholeGroup()
		{
			return _groupState.ShouldDragGroup();
		}

		PanelDropInfo IDropInfoProvider.GetDropInfo(DockPanelBase panel, Point screenPoint)
		{
			if (StaticGroup)
			{
				return GetDropInfoAt(panel, screenPoint);
			}
			if (panel == this)
			{
				return null;
			}
			if (panel.Contains(this))
			{
				return null;
			}
			PanelDropInfo panelDropInfo = null;
			if (base.ParentGroup != null || base.DockStyle != PanelDockStyle.Fill || GroupStyle == PanelGroupStyle.Tab)
			{
				panelDropInfo = _groupState.GetDropInfo(panel, screenPoint);
				if (panelDropInfo != null)
				{
					return panelDropInfo;
				}
			}
			foreach (DockPanelBase item in _groupState.GetVisiblePanels())
			{
				panelDropInfo = ((IDropInfoProvider)item).GetDropInfo(panel, screenPoint);
				if (panelDropInfo != null)
				{
					break;
				}
			}
			if (panelDropInfo != null)
			{
				if (_panels.Contains(panelDropInfo._targetPanel))
				{
					_groupState.ProcessChildDropInfo(panelDropInfo);
				}
				return panelDropInfo;
			}
			panelDropInfo = GetDropInfoAt(panel, screenPoint);
			if (base.PanelManager.DockingStyleMode == DockingStyleMode.Standard && panelDropInfo != null)
			{
				if (panelDropInfo._targetPanel != null)
				{
					return panelDropInfo;
				}
				return null;
			}
			return panelDropInfo;
		}

		internal bool PaintChildCaptionBar(Graphics graphics, DockCaptionRenderer captionBar)
		{
			return _groupState.PaintChildCaptionBar(graphics, captionBar);
		}

		internal bool CanSelectPanel(DockPanelBase panel, bool giveFocus)
		{
			if (IsTabGroup() && !IsUpdateSuspended())
			{
				if (panel != null && base.Visible && giveFocus && base.CanFocus && panel.CanFocus)
				{
					panel.Focus();
					Form form = RenderingHelper.FindForm(this);
					if ((form == null || !form.IsMdiContainer || form.ActiveMdiChild == null) && !panel.IsAutoHideEffective() && !panel.ContainsFocus && panel.CanFocus && !base.DesignMode)
					{
						return false;
					}
				}
				PanelActionCancelEventArgs e = new PanelActionCancelEventArgs(panel, cancel: false);
				OnSelectedPanelChanging(e);
				if (!e.Cancel)
				{
					if (base.PanelManager != null)
					{
						GroupSelectedPanelChangingEventArgs managerArgs = new GroupSelectedPanelChangingEventArgs(this, panel);
						base.PanelManager.RaiseGroupSelectedPanelChanging(managerArgs);
						return !managerArgs.Cancel;
					}
					return true;
				}
				return !e.Cancel;
			}
			return true;
		}

		protected virtual void OnSelectedPanelChanging(PanelActionCancelEventArgs e)
		{
			if (base.Events[_selectedPanelChangingKey] is PanelActionCancelEventHandler panelActionCancelEventHandler)
			{
				panelActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnSelectedPanelChanged(PanelActionEventArgs e)
		{
			if (base.Events[_selectedPanelChangedKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		internal void RaiseSelectedPanelChanged()
		{
			if (IsTabGroup() && !IsUpdateSuspended())
			{
				OnSelectedPanelChanged(new PanelActionEventArgs(SelectedPanel));
				if (base.PanelManager != null)
				{
					GroupSelectedPanelChangedEventArgs e = new GroupSelectedPanelChangedEventArgs(this, SelectedPanel);
					base.PanelManager.RaiseGroupSelectedPanelChanged(e);
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			Deserialize(reader);
			_showOutlookNavigatorConfigureMenu = (bool)LayoutReader.GetPropertyValue(reader, "ShowOutlookNavigatorConfigureMenu", typeof(bool), true);
			_allowOutlookButtonsResize = (bool)LayoutReader.GetPropertyValue(reader, "AllowOutlookButtonsResize", typeof(bool), true);
			_staticGroup = (bool)LayoutReader.GetPropertyValue(reader, "IsEntityPanel", typeof(bool), false);
			_visibleOutlookButtons = (int)LayoutReader.GetPropertyValue(reader, "VisibleOutlookButtons", typeof(int), -1);
			_tabDisplay = (TabDisplayMode)LayoutReader.GetPropertyValue(reader, "TabDisplay", typeof(TabDisplayMode), TabDisplayMode.UseDefault);
			_tabAlignment = (TabAlignment)LayoutReader.GetPropertyValue(reader, "TabAlignment", typeof(TabAlignment), TabAlignment.UseDefault);
			GroupStyle = (PanelGroupStyle)LayoutReader.GetPropertyValue(reader, "GroupStyle", typeof(PanelGroupStyle), PanelGroupStyle.HorizontalTiles);
			_splitterSize = (int)LayoutReader.GetPropertyValue(reader, "SplitterSize", typeof(int), -1);
			int selectedIndex = (int)LayoutReader.GetPropertyValue(reader, "SelectedPanelIndex", typeof(int), -1);
			if (selectedIndex >= 0 && selectedIndex < _panels.Count)
			{
				SelectedPanel = _panels[selectedIndex];
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			Serialize(info);
			info.AddValue("GroupStyle", _groupStyle);
			if (!_allowOutlookButtonsResize)
			{
				info.AddValue("AllowOutlookButtonsResize", _allowOutlookButtonsResize);
			}
			if (_staticGroup)
			{
				info.AddValue("IsEntityPanel", _staticGroup);
			}
			if (_tabAlignment != TabAlignment.UseDefault)
			{
				info.AddValue("TabAlignment", _tabAlignment);
			}
			if (_tabDisplay != TabDisplayMode.UseDefault)
			{
				info.AddValue("TabDisplay", _tabDisplay);
			}
			if (!_showOutlookNavigatorConfigureMenu)
			{
				info.AddValue("ShowOutlookNavigatorConfigureMenu", _showOutlookNavigatorConfigureMenu);
			}
			if (_visibleOutlookButtons != -1)
			{
				info.AddValue("VisibleOutlookButtons", _visibleOutlookButtons);
			}
			if (_splitterSize != -1)
			{
				info.AddValue("SplitterSize", _splitterSize);
			}
			if (SelectedPanel != null)
			{
				info.AddValue("SelectedPanelIndex", Panels.IndexOf(SelectedPanel));
			}
		}

		DockLayoutState IPanelContainer.GetDockLayoutState(PanelDockState dockState)
		{
			if (dockState == GetDockState())
			{
				return GetCurrentLayoutState();
			}
			return null;
		}

		bool IPanelContainer.OnPanelAdd(DockPanelBase panel, int index)
		{
			if (panel == this)
			{
				throw new ArgumentException("Panel can not be parented by itself or by a child panel.");
			}
			for (MxDockPanelGroup parentGroup = base.ParentGroup; parentGroup != null; parentGroup = parentGroup.ParentGroup)
			{
				if (parentGroup == panel)
				{
					throw new ArgumentException("Panel can not be parented by itself or by a child panel.");
				}
			}
			return true;
		}

		void IPanelContainer.OnPanelAdded(DockPanelBase panel)
		{
			if (IsTabGroup() && !MxDockManager.IsSinglePanel(panel))
			{
				MxDockPanelGroup addedGroup = (MxDockPanelGroup)panel;
				ArrayList arrayList = new ArrayList();
				foreach (DockPanelBase item in (IEnumerable)addedGroup.Panels)
				{
					arrayList.Add(item);
				}
				foreach (DockPanelBase childPanel in arrayList)
				{
					_panels.Add(childPanel);
				}
				_panels.Remove(panel);
				return;
			}
			if (!StaticGroup)
			{
				bool? siblingsAutoHide = null;
				if (!_isGroupLayoutSuspended)
				{
					foreach (DockPanelBase sibling in (IEnumerable)_panels)
					{
						if (sibling != panel)
						{
							if (!sibling.AutoHide)
							{
								siblingsAutoHide = false;
								break;
							}
							siblingsAutoHide = true;
						}
					}
					if (siblingsAutoHide.HasValue)
					{
						panel.AutoHide = siblingsAutoHide.Value;
						panel.RaiseAutoHideChanged();
						panel.UpdateVisibility();
					}
				}
			}
			panel.SetDockStateRecursive(DockState);
			GetCurrentLayoutState().GetChildren().MoveToCurrentIndex(panel.Id, _panels.IndexOf(panel));
			RecalculateLayout();
			if (base.PanelManager != null && AutoHide)
			{
				base.PanelManager.LayoutAutoHideStrips();
			}
		}

		bool IPanelContainer.OnPanelMove(DockPanelBase panel, int index)
		{
			return true;
		}

		void IPanelContainer.OnPanelMoved(DockPanelBase panel)
		{
			GetCurrentLayoutState().GetChildren().MoveToCurrentIndex(panel.Id, _panels.IndexOf(panel));
			RecalculateLayout();
		}

		bool IPanelContainer.OnPanelRemove(DockPanelBase panel)
		{
			return true;
		}

		void IPanelContainer.OnPanelRemoved(DockPanelBase panel)
		{
			_groupState.RefreshVisiblePanels();
			if (panel == SelectedPanel)
			{
				SelectedPanel = null;
				_groupState.RefreshVisiblePanels();
			}
			RecalculateLayout();
		}

		void IPanelContainer.InvalidateLayout()
		{
			RecalculateLayout();
		}
	}
}
