using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class CommandBarDesigner : ControlDesigner
	{
		public override SelectionRules SelectionRules => SelectionRules.None;

		protected override bool EnableDragRect => false;

		public CommandBarDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		protected override void Dispose(bool disposing)
		{
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			if (selectionService != null)
			{
				selectionService.SelectionChanged -= OnSelectionChanged;
			}
			base.Dispose(disposing);
		}

		private void OnSelectionChanged(object sender, EventArgs eventArgs)
		{
			if (Control != null)
			{
				MxCommandBar commandBar = (MxCommandBar)Control;
				DesignHelper.ForceDragDrop(commandBar);
			}
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			EnableDragDrop(value: false);
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			if (selectionService != null)
			{
				selectionService.SelectionChanged += OnSelectionChanged;
			}
		}

		protected override void WndProc(ref Message msg)
		{
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
			case NativeMethods.WM_MOUSEMOVE:
				DefWndProc(ref msg);
				return;
			case NativeMethods.WM_LBUTTONUP:
				DefWndProc(ref msg);
				return;
			case NativeMethods.WM_MOUSELEAVE:
			{
				MxCommandBar commandBar = (MxCommandBar)Control;
				DesignHelper.ForceMouseLeave(commandBar);
				break;
			}
			}
			base.WndProc(ref msg);
		}
	}
}
