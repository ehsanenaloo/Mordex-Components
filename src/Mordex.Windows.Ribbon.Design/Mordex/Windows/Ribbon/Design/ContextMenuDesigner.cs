using System;
using Microsoft.DotNet.DesignTools.Designers;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon.Design
{
	internal class ContextMenuDesigner : ComponentDesigner
	{
		private ISelectionService _selectionService;

		private IDesignerHost _designerHost;

		private ContextMenuDesignBar _designBar;

		internal MxRibbonContextMenu ContextMenu => base.Component as MxRibbonContextMenu;

		internal Control ParentControl
		{
			get
			{
				if (_designerHost != null && _designerHost.RootComponent is Control)
				{
					return _designerHost.RootComponent as Control;
				}
				return null;
			}
		}

		public ContextMenuDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (ContextMenu != null)
				{
					DesignHelper.HideChildMenu(ContextMenu);
					ContextMenu.Closed -= OnContextMenuClosed;
					DesignHelper.SetDesigner(ContextMenu, null);
				}
				RemoveDesignBar();
				if (_selectionService != null)
				{
					_selectionService.SelectionChanged -= OnSelectionChanged;
				}
				if (_designerHost != null)
				{
					_designerHost.Activated -= OnDesignerHostActivated;
					_designerHost.Deactivated -= OnDesignerHostDeactivated;
				}
			}
			base.Dispose(disposing);
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			_selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
			_designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
			if (_selectionService != null)
			{
				_selectionService.SelectionChanged += OnSelectionChanged;
			}
			if (_designerHost != null)
			{
				_designerHost.Activated += OnDesignerHostActivated;
				_designerHost.Deactivated += OnDesignerHostDeactivated;
			}
			if (ContextMenu != null)
			{
				ContextMenu.Closed += OnContextMenuClosed;
				DesignHelper.SetDesigner(ContextMenu, this);
			}
		}

		private void OnContextMenuClosed(object sender, EventArgs eventArgs)
		{
			RemoveDesignBar();
		}

		private void OnDesignerHostDeactivated(object sender, EventArgs eventArgs)
		{
			if (ContextMenu != null)
			{
				DesignHelper.HideChildMenu(ContextMenu);
			}
		}

		private void OnDesignerHostActivated(object sender, EventArgs eventArgs)
		{
			if (ContextMenu != null)
			{
				DesignHelper.ShowChildMenu(ContextMenu);
			}
		}

		private void OnSelectionChanged(object sender, EventArgs eventArgs)
		{
			if (ContextMenu != null)
			{
				if (_selectionService.PrimarySelection == base.Component && _selectionService.SelectionCount == 1)
				{
					AddDesignBar();
					return;
				}
				if (DesignHelper.ProcessChildMenuComponentSelected(ContextMenu, _selectionService))
				{
					return;
				}
			}
			RemoveDesignBar();
		}

		private void AddDesignBar()
		{
			if (_designBar == null)
			{
				_designBar = new ContextMenuDesignBar(ContextMenu.Site?.Name ?? string.Empty);
				_designBar.Height = DesignHelper.GetSmallItemHeight(ContextMenu);
				if (ParentControl != null)
				{
					ParentControl.Controls.Add(_designBar);
					ParentControl.Controls.SetChildIndex(_designBar, 0);
					_designBar.Dock = DockStyle.Top;
					ContextMenu.Show(_designBar, new Point(5, _designBar.Height));
				}
			}
		}

		private void RemoveDesignBar()
		{
			if (_designBar != null)
			{
				_designBar.Dispose();
				_designBar = null;
			}
		}
	}
}
