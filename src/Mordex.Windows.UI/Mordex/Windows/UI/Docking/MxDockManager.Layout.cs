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
using System.Runtime.CompilerServices;
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
		internal void RefreshFormIfNoComposition()
		{
			if (_containerControl == null)
			{
				return;
			}
			Form form = RenderingHelper.FindForm(_containerControl);
			if (form == null)
			{
				return;
			}
			bool isCompositionEnabled = false;
			if (Environment.OSVersion.Version.Major >= WindowsVistaMajorVersion)
			{
				NativeMethods.DwmIsCompositionEnabled(ref isCompositionEnabled);
			}
			if (!isCompositionEnabled)
			{
				if (form.MdiParent != null)
				{
					form.MdiParent.Refresh();
				}
				else
				{
					form.Refresh();
				}
			}
		}

		internal void RefreshFloatingWindows()
		{
			DockPanelBase[] floatingPanels = FloatingPanels;
			foreach (DockPanelBase uiPanelBase in floatingPanels)
			{
				if (uiPanelBase.FloatingWindow != null)
				{
					uiPanelBase.FloatingWindow.Refresh();
				}
			}
		}

		private void InvalidatePanels(DockPanelCollection panels)
		{
			ArrayList arrayList = new ArrayList();
			foreach (DockPanelBase item in (IEnumerable)panels)
			{
				arrayList.Add(item);
			}
			foreach (DockPanelBase panel in arrayList)
			{
				panel.RecalculateLayout();
				panel.Invalidate();
				if (panel.FloatingWindow != null)
				{
					panel._floatingWindow.ApplyLook();
				}
				if (panel is MxDockPanelGroup)
				{
					InvalidatePanels(((MxDockPanelGroup)panel).Panels);
				}
				else if (panel is MxDockPanel)
				{
					MxDockPanel uiPanel = (MxDockPanel)panel;
					if (uiPanel.InnerContainer != null)
					{
						uiPanel.InnerContainer.Invalidate();
					}
				}
				if (panel.ContainerCaptionControl != null)
				{
					panel.ContainerCaptionControl.Invalidate();
				}
			}
		}

		internal void SuspendContainerLayout()
		{
			if (_layoutSuspendCount == 0 && _containerControl != null)
			{
				_containerControl.SuspendLayout();
			}
			_layoutSuspendCount++;
		}

		internal void ResumeContainerLayout(bool performLayout)
		{
			_layoutSuspendCount--;
			if (_layoutSuspendCount == 0 && _containerControl != null)
			{
				_containerControl.ResumeLayout(performLayout);
			}
		}

		private void UpdateControlOrder(bool suspendLayout)
		{
			if (_isPanelLayoutSuspended)
			{
				return;
			}
			_isPanelLayoutSuspended = true;
			for (int index = _panels.Count - 1; index >= 0; index--)
			{
				DockPanelBase uiPanelBase = _panels[index];
				if (uiPanelBase.DockState != PanelDockState.Floating && uiPanelBase.DockStyle == PanelDockStyle.Fill)
				{
					_panels.SetPanelIndex(uiPanelBase, _panels.Count - 1);
				}
			}
			_isPanelLayoutSuspended = false;
			if (_containerControl == null)
			{
				return;
			}
			if (suspendLayout)
			{
				SuspendContainerLayout();
			}
			DockPanelBase[] dockedPanels = DockedPanels;
			Control[] orderedControls = new Control[_containerControl.Controls.Count];
			int slot = orderedControls.Length - 1;
			for (int controlIndex = _containerControl.Controls.Count - 1; controlIndex >= 0; controlIndex--)
			{
				Control control = _containerControl.Controls[controlIndex];
				if (control.Dock != DockStyle.Fill && control.Dock != DockStyle.None && !(control is DockPanelBase) && !(control is MxAutoHideStrip))
				{
					orderedControls[slot] = control;
					slot--;
				}
			}
			if (_containerControl.Contains(_leftStrip))
			{
				orderedControls[slot] = _leftStrip;
				slot--;
			}
			if (_containerControl.Contains(_topStrip))
			{
				orderedControls[slot] = _topStrip;
				slot--;
			}
			if (_containerControl.Contains(_rightStrip))
			{
				orderedControls[slot] = _rightStrip;
				slot--;
			}
			if (_containerControl.Contains(_bottomStrip))
			{
				orderedControls[slot] = _bottomStrip;
				slot--;
			}
			foreach (DockPanelBase dockedPanel in dockedPanels)
			{
				if (_containerControl.Controls.Contains(dockedPanel))
				{
					orderedControls[slot] = dockedPanel;
					slot--;
				}
			}
			for (int fillIndex = _containerControl.Controls.Count - 1; fillIndex >= 0; fillIndex--)
			{
				Control control = _containerControl.Controls[fillIndex];
				if (!(control is MxAutoHideStrip) && !(control is DockPanelBase) && (control.Dock == DockStyle.Fill || control.Dock == DockStyle.None))
				{
					orderedControls[slot] = control;
					slot--;
				}
			}
			try
			{
				for (int j = slot + 1; j < orderedControls.Length; j++)
				{
					_containerControl.Controls.SetChildIndex(orderedControls[j], j);
				}
			}
			catch (ArgumentException)
			{
				// A control left the container while the order was being built. The next layout pass fixes the order.
			}
			if (suspendLayout)
			{
				ResumeContainerLayout(true);
			}
		}

		internal void LayoutAutoHideStrips()
		{
			_bottomStrip.UpdateLayout();
			_leftStrip.UpdateLayout();
			_rightStrip.UpdateLayout();
			_topStrip.UpdateLayout();
		}

		internal Rectangle GetAutoHidePanelBounds(AutoHideStripDock autoHideStripDock, DockPanelBase uiPanelBase)
		{
			Rectangle result = _dockArea;
			switch (autoHideStripDock)
			{
			case AutoHideStripDock.Bottom:
				result.Y = result.Bottom - (uiPanelBase.Height + 1);
				result.Height = uiPanelBase.Height + 1;
				break;
			case AutoHideStripDock.Top:
				result.Height = uiPanelBase.Height + 1;
				break;
			case AutoHideStripDock.Left:
				result.Width = uiPanelBase.Width + 1;
				break;
			case AutoHideStripDock.Right:
				result.X = result.Right - (uiPanelBase.Width + 1);
				result.Width = uiPanelBase.Width + 1;
				break;
			}
			return result;
		}

		private void UpdateDockAreas()
		{
			if (_containerControl == null || _containerControl.ClientRectangle.IsEmpty)
			{
				_dockArea = Rectangle.Empty;
				_fillArea = Rectangle.Empty;
				return;
			}
			_dockArea = _containerControl.DisplayRectangle;
			_fillArea = _dockArea;
			for (int index = _containerControl.Controls.Count - 1; index >= 0; index--)
			{
				Control control = _containerControl.Controls[index];
				if (control.Visible)
				{
					bool isOtherControl = !(control is DockPanelBase);
					switch (control.Dock)
					{
					case DockStyle.Bottom:
						_fillArea.Height -= control.Height;
						if (isOtherControl)
						{
							_dockArea.Height -= control.Height;
						}
						break;
					case DockStyle.Right:
						_fillArea.Width -= control.Width;
						if (isOtherControl)
						{
							_dockArea.Width -= control.Width;
						}
						break;
					case DockStyle.Top:
						_fillArea.Y += control.Height;
						_fillArea.Height -= control.Height;
						if (isOtherControl)
						{
							_dockArea.Y += control.Height;
							_dockArea.Height -= control.Height;
						}
						break;
					case DockStyle.Left:
						_fillArea.X += control.Width;
						_fillArea.Width -= control.Width;
						if (isOtherControl)
						{
							_dockArea.X += control.Width;
							_dockArea.Width -= control.Width;
						}
						break;
					}
				}
			}
		}

		private void OnContainerControlVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (_containerControl == null)
			{
				return;
			}
			if (_containerControl.Visible)
			{
				RestoreDockLayout();
				foreach (DockPanelBase floatingPanel in FloatingPanels)
				{
					if (!floatingPanel.Closed && floatingPanel._floatingWindow != null)
					{
						floatingPanel._floatingWindow.Visible = true;
					}
				}
				return;
			}
			foreach (DockPanelBase floatingPanel in FloatingPanels)
			{
				if (floatingPanel._floatingWindow != null)
				{
					floatingPanel._floatingWindow.Visible = false;
				}
			}
		}

		private void OnContainerControlLayout(object sender, LayoutEventArgs eventArgs)
		{
			if (_isInitializing)
			{
				return;
			}
			UpdateControlOrder(true);
			UpdateDockAreas();
		}

		private void OnContainerControlResize(object sender, EventArgs eventArgs)
		{
			UpdateDockAreas();
		}

		internal void OnPanelActiveChanged(DockPanelBase panel)
		{
			if (panel.IsActive)
			{
				OnPanelActivated(new PanelActionEventArgs(panel));
			}
			else
			{
				OnPanelDeactivate(new PanelActionEventArgs(panel));
			}
			LayoutAutoHideStrips();
		}

		public void SuspendPanelLayout()
		{
			_isPanelLayoutSuspended = true;
			SuspendContainerLayout();
		}

		public void ResumePanelLayout()
		{
			if (!_isPanelLayoutSuspended)
			{
				return;
			}
			_isPanelLayoutSuspended = false;
			foreach (DockPanelBase item in (IEnumerable)_panels)
			{
				item.RecalculateLayout();
			}
			StoreDockedPanelOrder();
			UpdateControlOrder(false);
			ResumeContainerLayout(true);
			LayoutAutoHideStrips();
		}
	}
}
