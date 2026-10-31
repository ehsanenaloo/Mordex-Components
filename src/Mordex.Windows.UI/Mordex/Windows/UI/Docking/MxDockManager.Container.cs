using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxDockManager
	{
		public void NewMdiTabGroup()
		{
			if (_mdiTabGroups != null && _mdiTabGroups.GetActiveGroup() != null && _mdiTabGroups.GetActiveGroup().MdiTabs.Length > 1)
			{
				_mdiTabGroups.AppendGroup(_mdiTabGroups.GetForm().ActiveMdiChild);
			}
		}

		public void NewMdiTabGroup(MdiGroupOrientation groupOrientation)
		{
			_tabbedMdiSettings.GroupOrientation = groupOrientation;
			NewMdiTabGroup();
		}

		public void NewMdiTabGroup(MdiChildTab tab)
		{
			if (_mdiTabGroups != null)
			{
				_mdiTabGroups.AppendGroup(tab.Form);
			}
		}

		private void RemoveFromContainer(Control control)
		{
			if (_containerControl != null)
			{
				Form form = RenderingHelper.FindForm(_containerControl);
				if (form != null && form.ActiveControl != null && (form.ActiveControl == control || control.Contains(form.ActiveControl)))
				{
					RenderingHelper.TryFocusControl(form);
				}
				_containerControl.Controls.Remove(control);
			}
		}

		internal bool HasNonMdiContainer()
		{
			if (_containerControl is Form form)
			{
				return !form.IsMdiContainer;
			}
			return true;
		}

		private void DetachContainerControl()
		{
			if (_containerControl != null)
			{
				_containerControl.VisibleChanged -= OnContainerControlVisibleChanged;
				_containerControl.Layout -= OnContainerControlLayout;
				_containerControl.Resize -= OnContainerControlResize;
				_containerControl.SystemColorsChanged -= OnContainerControlSystemColorsChanged;
				_containerControl.Disposed -= OnContainerControlDisposed;
				RemoveFromContainer(_leftStrip);
				RemoveFromContainer(_topStrip);
				RemoveFromContainer(_rightStrip);
				RemoveFromContainer(_bottomStrip);
			}
		}

		private void AttachContainerControl()
		{
			if (_designer != null)
			{
				DesignHelper.PanelManagerDesigner_SelectContainer(_designer);
			}
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				item.UpdateParent();
			}
			if (_containerControl != null)
			{
				_leftStrip.Parent = _containerControl;
				_topStrip.Parent = _containerControl;
				_rightStrip.Parent = _containerControl;
				_bottomStrip.Parent = ContainerControl;
				LayoutAutoHideStrips();
				_containerControl.VisibleChanged += OnContainerControlVisibleChanged;
				_containerControl.Layout += OnContainerControlLayout;
				_containerControl.Resize += OnContainerControlResize;
				_containerControl.SystemColorsChanged += OnContainerControlSystemColorsChanged;
				_containerControl.Disposed += OnContainerControlDisposed;
				UpdateControlOrder(true);
				UpdateDockAreas();
			}
		}

		private void OnContainerControlDisposed(object sender, EventArgs eventArgs)
		{
			ContainerControl = null;
		}

		private void OnContainerControlSystemColorsChanged(object sender, EventArgs eventArgs)
		{
			if (_panelRenderer != null)
			{
				CreateRenderers();
			}
			OnSystemColorsChanged(eventArgs);
		}

		internal void RefreshMdiTabGroups()
		{
			if (_mdiTabGroups != null)
			{
				if (_mdiTabGroups.GetMdiClient() != null)
				{
					NativeMethods.SendMessage(_mdiTabGroups.GetMdiClient().Handle, WM_NCACTIVATE, IntPtr.Zero, IntPtr.Zero);
				}
				_mdiTabGroups.LayoutGroups();
			}
		}

		internal void RefreshPanelSettings(string propertyName)
		{
			if (propertyName == "MinimumSize" || propertyName == "MaximumSize")
			{
				foreach (DockPanelBase item in (IEnumerable)_panels)
				{
					item.ApplySizeLimits();
				}
			}
			foreach (DockPanelBase panelToInvalidate in (IEnumerable)_panels)
			{
				panelToInvalidate.Invalidate(invalidateChildren: true);
			}
			InvalidatePanels(_panels);
			LayoutAutoHideStrips();
		}

		internal Rectangle GetDockArea()
		{
			return _dockArea;
		}

		internal Rectangle GetFillArea()
		{
			return _fillArea;
		}

		internal bool TryRestoreMdiChildForm(MdiChildTab tab)
		{
			RestoreMdiChildFormEventArgs e = new RestoreMdiChildFormEventArgs(tab);
			OnRestoreMdiChildForm(e);
			if (e.Form != null && _mdiTabGroups.GetMdiClient().Contains(e.Form))
			{
				tab.AttachForm(e.Form);
				return true;
			}
			return false;
		}

		private void SetMdiTabGroups(MdiTabGroupRenderer mdiTabGroups)
		{
			if (_mdiTabGroups != mdiTabGroups)
			{
				if (_mdiTabGroups != null)
				{
					_mdiTabGroups.Detach();
				}
				_mdiTabGroups = mdiTabGroups;
			}
		}

		private void RecreateMdiTabGroups()
		{
			if (_tabbedMdi && _containerControl is Form && !base.DesignMode)
			{
				Form form = (Form)_containerControl;
				SetMdiTabGroups(null);
				SetMdiTabGroups(new MdiTabGroupRenderer(this, form));
			}
			else
			{
				SetMdiTabGroups(null);
			}
		}
	}
}
