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
	public abstract partial class DockPanelBase
	{
		protected virtual void OnParentGroupChanged()
		{
		}

		public void DockPanel()
		{
			if (DockState == PanelDockState.Floating && ParentGroup != null && ParentGroup.StaticGroup)
			{
				throw new InvalidOperationException("Can not Dock a panel that is contained in an ActAsSinglePanel group.");
			}
			MxDockManager panelManager = PanelManager;
			EnsureDocked();
			if (panelManager != null)
			{
				panelManager.RestoreDockLayout();
				panelManager.ActivateParentForm();
			}
			if (RenderingHelper.GetParentControl(this) != null)
			{
				Activate();
			}
		}

		public void UndockPanel()
		{
			if (DockState == PanelDockState.Docked && ParentGroup != null && ParentGroup.StaticGroup)
			{
				throw new InvalidOperationException("Can not Undock a panel that is contained in an ActAsSinglePanel group.");
			}
			EnsureFloating();
			PanelManager.RestoreDockLayout();
			Activate();
		}

		public MxDockPanelGroup CreateGroup(PanelGroupStyle groupStyle, DockPanelBase panelToGroupWith)
		{
			if (panelToGroupWith == null)
			{
				throw new ArgumentNullException("panelToGroupWith");
			}
			return WrapInNewGroup(groupStyle, panelToGroupWith, 1);
		}

		public virtual void Activate()
		{
			if (Closed)
			{
				return;
			}
			if (ParentGroup != null)
			{
				ParentGroup.SelectedPanel = this;
			}
			if (_containerCaptionControl != null)
			{
				_containerCaptionControl.Invalidate();
			}
			if (GetRootFloatingWindow() != null)
			{
				GetRootFloatingWindow().Activate();
			}
			if (GetAutoHideStrip() != null && IsAutoHideEffective())
			{
				GetAutoHideStrip().ShowPopup(this, true);
			}
		}

		private MxFloatingWindow GetRootFloatingWindow()
		{
			if (ParentGroup != null)
			{
				return ParentGroup.GetRootFloatingWindow();
			}
			return _floatingWindow;
		}

		protected override void OnDockChanged(EventArgs e)
		{
			if (!_isSettingDock)
			{
				SyncControlDock();
			}
			else
			{
				base.OnDockChanged(e);
			}
		}

		internal void SetControlDock(DockStyle dockStyle)
		{
			if (base.Dock != dockStyle)
			{
				_isSettingDock = true;
				base.Dock = dockStyle;
				_isSettingDock = false;
			}
		}

		internal PanelDockState GetDockState()
		{
			return _dockState;
		}

		internal void SetDockState(PanelDockState dockState)
		{
			_dockState = dockState;
		}

		internal void CloseFloatingWindow()
		{
			if (_floatingWindow != null)
			{
				MxFloatingWindow uiFloatingWindow = _floatingWindow;
				uiFloatingWindow.ReplaceHostedPanel(null);
				uiFloatingWindow.Close();
				uiFloatingWindow.Dispose();
				_floatingWindow = null;
			}
		}

		private void CreateFloatingWindow()
		{
			if (PanelManager != null && _floatingWindow == null)
			{
				_floatingWindow = new MxFloatingWindow(PanelManager, this);
				_floatingWindow.ShowAtInitialSize();
			}
		}

		private void PropagateDockState()
		{
			if (this is IPanelContainer)
			{
				IPanelContainer container = (IPanelContainer)this;
				foreach (DockPanelBase item in (IEnumerable)container.Panels)
				{
					item.SetDockStateRecursive(_dockState);
				}
			}
			UpdateParent();
		}

		internal void SetDockStateRecursive(PanelDockState dockState)
		{
			if (DockState != dockState)
			{
				_dockState = dockState;
				PropagateDockState();
			}
			else
			{
				_dockState = dockState;
			}
		}

		internal void ApplyDockState(PanelDockState dockState)
		{
			_dockState = dockState;
			if (PanelManager != null && GetCurrentLayoutState().GetParentState() == null)
			{
				GetCurrentLayoutState().Reparent(((IPanelContainer)PanelManager).GetDockLayoutState(_dockState));
			}
			_dockedLayoutState.UpdateIsCurrent(dockState == PanelDockState.Docked);
			_floatingLayoutState.UpdateIsCurrent(dockState == PanelDockState.Floating);
			if (this is IPanelContainer)
			{
				DockPanelCollection panels = ((IPanelContainer)this).Panels;
				foreach (DockPanelBase item in (IEnumerable)panels)
				{
					item.ApplyDockState(dockState);
				}
			}
			if (PanelManager != null)
			{
				_isNotifyingDockState = true;
				PanelManager.RaisePanelDockStateChanged(this);
				_isNotifyingDockState = false;
			}
		}

		internal void MoveToParent(Control control)
		{
			try
			{
				if (control == base.Parent)
				{
					return;
				}
				Form form = FindForm();
				if (form != null && (form.ActiveControl == this || Contains(GetInnermostActiveControl(form.ActiveControl))))
				{
					if (PanelManager != null && PanelManager.ContainerControl != null)
					{
						if (PanelManager.ContainerControl.FindForm() == form)
						{
							if (form.IsMdiChild && form.ActiveMdiChild != null)
							{
								form.ActiveMdiChild.Focus();
							}
							else
							{
								PanelManager.FocusDefaultControl();
							}
						}
					}
					else if (form is MxFloatingWindow)
					{
						form.Focus();
					}
					else if (base.Parent != null)
					{
						base.Parent.Focus();
					}
				}
				base.Parent = control;
			}
			catch
			{
			}
		}

		internal void AttachToGroupOrDetach(MxDockPanelGroup uiPanelGroup, PanelDockState dockState, PanelDockStyle panelDockStyle)
		{
			if (uiPanelGroup == null)
			{
				SuspendPanelUpdate();
				if (_dockState != dockState)
				{
					PanelDockState panelDockState = _dockState;
					_dockState = dockState;
					ParentGroup = null;
					if (dockState == PanelDockState.Docked)
					{
						DockStyle = panelDockStyle;
					}
					_dockState = panelDockState;
					DockState = dockState;
				}
				else
				{
					ParentGroup = null;
					if (dockState == PanelDockState.Docked)
					{
						SetBoundsCore(base.Left, base.Top, Width, Height, BoundsSpecified.All);
						DockStyle = panelDockStyle;
					}
					else if (_floatingWindow != null && FloatingLocation != new Point(-1, -1))
					{
						_floatingWindow.Location = FloatingLocation;
					}
				}
				ResumePanelUpdate();
			}
			else
			{
				ParentGroup = uiPanelGroup;
			}
		}

		protected virtual void OnDockPanel()
		{
			ApplyDockState(PanelDockState.Docked);
		}

		internal void EnsureDocked()
		{
			if (_dockState != PanelDockState.Docked)
			{
				OnDockPanel();
			}
		}

		internal MxDockPanelGroup WrapInNewGroup(PanelGroupStyle panelGroupStyle, DockPanelBase panel, int insertIndex)
		{
			GetTileSize();
			if (PanelManager != null)
			{
				PanelManager.SuspendPanelLayout();
			}
			MxDockPanelGroup uiPanelGroup = new MxDockPanelGroup();
			uiPanelGroup.Bounds = Bounds;
			if (_floatingWindow != null)
			{
				_floatingWindow.ReplaceHostedPanel(uiPanelGroup);
			}
			uiPanelGroup.GroupStyle = panelGroupStyle;
			uiPanelGroup.Size = Size;
			uiPanelGroup.DockStyle = DockStyle;
			uiPanelGroup.DockState = DockState;
			DockLayoutState dockLayoutState = uiPanelGroup.GetCurrentLayoutState();
			GetCurrentLayoutState().CopyLayoutTo(dockLayoutState);
			dockLayoutState.SetIsGroup(true);
			GetCurrentLayoutState()._parentState.GetChildren().ReplaceLayoutState(GetCurrentLayoutState(), dockLayoutState);
			dockLayoutState.SetGroupStyle(panelGroupStyle);
			uiPanelGroup.SuspendPanelUpdate();
			uiPanelGroup._shouldSkipLayoutSync = true;
			int index = _parentGroup.Panels.IndexOf(this);
			_parentGroup.Panels.Insert(index, uiPanelGroup);
			uiPanelGroup._shouldSkipLayoutSync = false;
			panel.ApplyDockState(uiPanelGroup.DockState);
			if (insertIndex == 0)
			{
				uiPanelGroup.Panels.Add(panel);
				uiPanelGroup.Panels.Add(this);
			}
			else
			{
				uiPanelGroup.Panels.Add(this);
				uiPanelGroup.Panels.Add(panel);
			}
			if (uiPanelGroup.ParentGroup != null)
			{
				uiPanelGroup.ParentGroup.ResumePanelUpdate();
			}
			if (base.Container != null)
			{
				base.Container.Add(uiPanelGroup);
			}
			if (PanelManager != null)
			{
				PanelManager.ResumePanelLayout();
			}
			uiPanelGroup.ResumePanelUpdate();
			if (PanelManager != null)
			{
				PanelManager.RaisePanelGroupCreated(uiPanelGroup);
			}
			return uiPanelGroup;
		}

		protected virtual void OnUndockPanel()
		{
			ApplyDockState(PanelDockState.Floating);
		}

		internal void EnsureFloating()
		{
			if (DockState != PanelDockState.Floating)
			{
				OnUndockPanel();
			}
		}

		internal void UpdateParent()
		{
			if (_isUpdateSuspended)
			{
				return;
			}
			if (AutoHide)
			{
				CloseFloatingWindow();
				MoveToParent(_autoHidePanel);
			}
			else
			{
				if (!IsRootPanel() || !IsFloating() || PanelManager == null || AutoHide)
				{
					CloseFloatingWindow();
				}
				if (ParentGroup != null)
				{
					MoveToParent(ParentGroup);
				}
				else if (PanelManager == null)
				{
					MoveToParent(null);
				}
				else if (IsRootPanel() && _dockState == PanelDockState.Floating)
				{
					if (this is MxDockPanelGroup && ((MxDockPanelGroup)this).Panels.Count == 0 && !((MxDockPanelGroup)this).StaticGroup)
					{
						return;
					}
					if (!Closed)
					{
						CreateFloatingWindow();
					}
				}
				else
				{
					MoveToParent(PanelManager.ContainerControl);
				}
			}
			SyncControlDock();
			RecalculateLayout();
		}

		private void SyncControlDock()
		{
			if (ParentGroup == null && DockState == PanelDockState.Docked && !AutoHide)
			{
				switch (DockStyle)
				{
				case PanelDockStyle.Top:
					SetControlDock(System.Windows.Forms.DockStyle.Top);
					break;
				case PanelDockStyle.Bottom:
					SetControlDock(System.Windows.Forms.DockStyle.Bottom);
					break;
				case PanelDockStyle.Fill:
					SetControlDock(System.Windows.Forms.DockStyle.Fill);
					break;
				case PanelDockStyle.Left:
					SetControlDock(System.Windows.Forms.DockStyle.Left);
					break;
				case PanelDockStyle.Right:
					SetControlDock(System.Windows.Forms.DockStyle.Right);
					break;
				}
			}
			else
			{
				SetControlDock(System.Windows.Forms.DockStyle.None);
			}
		}

		internal bool IsSideDocked()
		{
			if (DockStyle != PanelDockStyle.Left)
			{
				return DockStyle == PanelDockStyle.Right;
			}
			return true;
		}

		protected virtual void OnDockStyleChanged()
		{
			SyncControlDock();
			RecalculateLayout();
		}
	}
}
