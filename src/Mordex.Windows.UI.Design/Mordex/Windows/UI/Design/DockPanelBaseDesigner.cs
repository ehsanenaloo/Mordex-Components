using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class DockPanelBaseDesigner : ControlDesigner
	{
		private object _designedPanel;

		public override SelectionRules SelectionRules
		{
			get
			{
				DockPanelBase uiPanelBase = (DockPanelBase)_designedPanel;
				if (uiPanelBase.ParentGroup == null)
				{
					SelectionRules selectionRules = SelectionRules.Visible;
					switch (uiPanelBase.DockStyle)
					{
					case PanelDockStyle.Bottom:
						return selectionRules | SelectionRules.TopSizeable;
					case PanelDockStyle.Top:
						return selectionRules | SelectionRules.BottomSizeable;
					case PanelDockStyle.Left:
						return selectionRules | SelectionRules.RightSizeable;
					case PanelDockStyle.Right:
						return selectionRules | SelectionRules.LeftSizeable;
					default:
						return selectionRules | SelectionRules.Locked;
					}
				}
				return SelectionRules.Locked | SelectionRules.Visible;
			}
		}

		internal MxDockManager PanelManager
		{
			get
			{
				if (base.Component is DockPanelBase)
				{
					return ((DockPanelBase)base.Component).PanelManager;
				}
				return null;
			}
		}

		protected override bool EnableDragRect => false;

		public DockPanelBaseDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			_designedPanel = component;
			EnableDragDrop(value: false);
		}

		protected override void WndProc(ref Message msg)
		{
			try
			{
				if (PanelManager != null)
				{
					IMessageFilter dragHandler = DesignHelper.GetDragHandler(PanelManager);
					if (dragHandler != null && dragHandler.PreFilterMessage(ref msg))
					{
						return;
					}
				}
				switch (msg.Msg)
				{
				case NativeMethods.WM_LBUTTONDOWN:
				{
					ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
					if (selectionService != null)
					{
						ArrayList arrayList = new ArrayList();
						arrayList.Add(Control);
						selectionService.SetSelectedComponents(arrayList);
					}
					DefWndProc(ref msg);
					return;
				}
				case NativeMethods.WM_SETCURSOR:
					DefWndProc(ref msg);
					return;
				case NativeMethods.WM_MOUSEMOVE:
					if (Control.Capture)
					{
						DefWndProc(ref msg);
						return;
					}
					break;
				case NativeMethods.WM_LBUTTONUP:
					DefWndProc(ref msg);
					return;
				}
				base.WndProc(ref msg);
			}
			catch
			{
				DesignHelper.ForceEndPanelResize((DockPanelBase)Control);
				base.WndProc(ref msg);
			}
		}
	}
}
