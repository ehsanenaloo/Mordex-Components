using System;
using Microsoft.DotNet.DesignTools.Designers;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class DockManagerDesigner : ComponentDesigner
	{
		private MxDockManager _dockManager;

		private DesignerTransaction _dragTransaction;

		private DesignerTransaction _autoHideTransaction;

		private DesignerTransaction _changeTransaction;

		private DockPanelBase _autoHidePanel;

		private bool _isHostLoaded;

		private bool IsHostLoaded
		{
			get
			{
				if (!_isHostLoaded)
				{
					return false;
				}
				IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
				if (designerHost != null)
				{
					if (designerHost.RootComponent is Control)
					{
						return !((Control)designerHost.RootComponent).Disposing;
					}
					return true;
				}
				return false;
			}
		}

		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerbCollection designerVerbCollection = new DesignerVerbCollection();
				designerVerbCollection.Add(new DesignerVerb("Panel Manager Designer...", OnCustomize));
				designerVerbCollection.Add(new DesignerVerb("Add Panel Wizard...", OnPanelAdded));
				designerVerbCollection.Add(new DesignerVerb("AutoHide Panels...", OnAutoHideClick));
				designerVerbCollection.Add(new DesignerVerb("Closed Panels...", OnClosedClick));
				designerVerbCollection.Add(new DesignerVerb("About", OnAbout));
				return designerVerbCollection;
			}
		}

		public bool PanelManagerDesigner_IsHostLoaded
		{
			get
			{
				return IsHostLoaded;
			}
			set
			{
				_isHostLoaded = value;
			}
		}

		private IDesignerHost _designerHost;

		private ISelectionService _selectionService;

		public DockManagerDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CancelTransaction(ref _changeTransaction);
				CancelTransaction(ref _dragTransaction);
				CancelTransaction(ref _autoHideTransaction);
				_autoHidePanel = null;
				if (_dockManager != null)
				{
					DesignHelper.SetPanelManagerDesigner(_dockManager, null);
					_dockManager.PanelStartDrag -= OnPanelStartDrag;
					_dockManager.PanelDrop -= OnPanelDrop;
					_dockManager.PanelAutoHideChanging -= OnPanelAutoHideChanging;
					_dockManager.PanelAutoHideChanged -= OnPanelAutoHideChanged;
				}
				if (_designerHost != null)
				{
					_designerHost.LoadComplete -= OnHostLoadComplete;
					_designerHost.Deactivated -= OnHostDeactivated;
					_designerHost.Activated -= OnHostActivated;
					_designerHost = null;
				}
				if (_selectionService != null)
				{
					_selectionService.SelectionChanging -= OnSelectionChanged;
					_selectionService = null;
				}
			}
			base.Dispose(disposing);
		}

		private static void CancelTransaction(ref DesignerTransaction transaction)
		{
			if (transaction != null)
			{
				if (!transaction.Committed && !transaction.Canceled)
				{
					transaction.Cancel();
				}
				transaction = null;
			}
		}

		private static void CommitTransaction(ref DesignerTransaction transaction)
		{
			if (transaction != null)
			{
				if (!transaction.Committed && !transaction.Canceled)
				{
					transaction.Commit();
				}
				transaction = null;
			}
		}

		// The drag transaction stays open until the control reports the end of the drag (PanelManagerDesigner_EndPanelDragTransaction).
		private void OnPanelDrop(object sender, PanelDragDropEventArgs eventArgs)
		{
			bool opened = false;
			if (_dragTransaction == null)
			{
				_dragTransaction = StartTransaction("Drag Panel " + eventArgs.Panel.Name);
				opened = true;
			}
			try
			{
				IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
				if (componentChangeService != null)
				{
					RaiseComponentChange(componentChangeService, changeComplete: false);
				}
			}
			catch
			{
				if (opened)
				{
					CancelTransaction(ref _dragTransaction);
				}
				throw;
			}
		}

		private void OnPanelStartDrag(object sender, PanelActionCancelEventArgs eventArgs)
		{
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			if (selectionService != null)
			{
				ArrayList arrayList = new ArrayList();
				arrayList.Add(base.Component);
				selectionService.SetSelectedComponents(arrayList);
			}
		}

		private void RaiseComponentChange(IComponentChangeService ics, bool changeComplete)
		{
			if (!changeComplete)
			{
				ics.OnComponentChanging(_dockManager, null);
			}
			else
			{
				ics.OnComponentChanged(_dockManager, null, null, null);
			}
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			_dockManager = (MxDockManager)base.Component;
			DesignHelper.SetPanelManagerDesigner(_dockManager, this);
			_dockManager.PanelStartDrag += OnPanelStartDrag;
			_dockManager.PanelDrop += OnPanelDrop;
			_dockManager.PanelAutoHideChanging += OnPanelAutoHideChanging;
			_dockManager.PanelAutoHideChanged += OnPanelAutoHideChanged;
			IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
			if (designerHost == null)
			{
				return;
			}
			_designerHost = designerHost;
			designerHost.LoadComplete += OnHostLoadComplete;
			if (!designerHost.Loading)
			{
				_isHostLoaded = true;
			}
			foreach (IComponent otherComponent in base.Component.Site.Container.Components)
			{
				if (otherComponent != _dockManager && otherComponent is MxDockManager)
				{
					throw new InvalidOperationException("Only one UIPanelManager can be used in a control.");
				}
			}
			if (_dockManager.ContainerControl == null && designerHost.RootComponent is Control)
			{
				_dockManager.ContainerControl = (Control)designerHost.RootComponent;
			}
			designerHost.Deactivated += OnHostDeactivated;
			designerHost.Activated += OnHostActivated;
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			if (selectionService != null)
			{
				_selectionService = selectionService;
				selectionService.SelectionChanging += OnSelectionChanged;
			}
		}

		private void OnPanelAutoHideChanging(object sender, PanelActionCancelEventArgs eventArgs)
		{
			SelectContainer();
			if (_autoHideTransaction == null)
			{
				_autoHidePanel = eventArgs.Panel;
				_autoHideTransaction = StartTransaction("Panel AutoHide");
			}
			try
			{
				IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
				PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(eventArgs.Panel).Find("AutoHide", ignoreCase: true);
				if (componentChangeService != null && propertyDescriptor != null)
				{
					componentChangeService.OnComponentChanging(eventArgs.Panel, propertyDescriptor);
				}
			}
			catch
			{
				if (eventArgs.Panel == _autoHidePanel)
				{
					CancelTransaction(ref _autoHideTransaction);
					_autoHidePanel = null;
				}
				throw;
			}
		}

		private void OnPanelAutoHideChanged(object sender, PanelActionEventArgs eventArgs)
		{
			IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
			if (componentChangeService != null)
			{
				PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(eventArgs.Panel).Find("AutoHide", ignoreCase: true);
				if (propertyDescriptor != null)
				{
					componentChangeService.OnComponentChanged(eventArgs.Panel, propertyDescriptor, !eventArgs.Panel.AutoHide, eventArgs.Panel.AutoHide);
				}
			}
			if (_autoHideTransaction != null && eventArgs.Panel == _autoHidePanel)
			{
				CommitTransaction(ref _autoHideTransaction);
				_autoHidePanel = null;
			}
		}

		private void OnSelectionChanged(object sender, EventArgs eventArgs)
		{
			DesignHelper.PerformEndDrag(_dockManager);
		}

		private void OnHostLoadComplete(object sender, EventArgs eventArgs)
		{
			_isHostLoaded = true;
			DesignHelper.EndPanelManagerInitialize(_dockManager);
		}

		private void OnHostDeactivated(object sender, EventArgs eventArgs)
		{
			DesignHelper.SetFloatingPanelsVisibility(_dockManager, visible: false);
		}

		private void OnHostActivated(object sender, EventArgs eventArgs)
		{
			DesignHelper.SetFloatingPanelsVisibility(_dockManager, visible: true);
		}

		private void OnAbout(object sender, EventArgs eventArgs)
		{
			DesignHelper.ShowAboutBox((MxDockManager)base.Component);
		}

		private void OnAutoHideClick(object sender, EventArgs eventArgs)
		{
			if (base.Component is MxDockManager dockManager)
			{
				DesignerChange.Run(dockManager, null, "AutoHide Panels", () => DesignHelper.ShowAutoHidePanelsDialog(dockManager) == DialogResult.OK);
			}
		}

		private void OnClosedClick(object sender, EventArgs eventArgs)
		{
			if (base.Component is MxDockManager dockManager)
			{
				DesignerChange.Run(dockManager, null, "Closed Panels", () => DesignHelper.ShowClosedPanelsDialog(dockManager) == DialogResult.OK);
			}
		}

		private void OnCustomize(object sender, EventArgs eventArgs)
		{
			try
			{
				if (base.Component is MxDockManager uiPanelManager)
				{
					DesignerChange.Run(uiPanelManager, null, "Panel Manager Designer", () => DesignHelper.ShowPanelManagerDesignerDialog(uiPanelManager) == DialogResult.OK);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "OnCustomize");
			}
		}

		private void OnPanelAdded(object sender, EventArgs eventArgs)
		{
			if (_dockManager == null)
			{
				return;
			}
			try
			{
				DesignerChange.Run(_dockManager, null, "Add Panel", () =>
				{
					DockPanelBase uiPanelBase;
					using (AddPanelWizard addPanelWizard = new AddPanelWizard())
					{
						uiPanelBase = addPanelWizard.CreatePanel(_dockManager);
					}
					if (uiPanelBase == null)
					{
						return false;
					}
					if (uiPanelBase.Container == null)
					{
						_dockManager.Container.Add(uiPanelBase, uiPanelBase.Name);
						if (uiPanelBase is MxDockPanel)
						{
							_dockManager.Container.Add(((MxDockPanel)uiPanelBase).InnerContainer, uiPanelBase.Name + "Container");
						}
					}
					if (uiPanelBase.ContainerCaptionControl != null && uiPanelBase.ContainerCaptionControl.Container == null)
					{
						_dockManager.Container.Add(uiPanelBase.ContainerCaptionControl, uiPanelBase.Name + "CaptionContainer");
					}
					return true;
				});
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void NotifyChange(bool changeComplete)
		{
			try
			{
				IComponentChangeService componentChangeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
				if (componentChangeService != null)
				{
					RaiseComponentChange(componentChangeService, changeComplete);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Error in NotifyChange");
			}
		}

		public void PanelManagerDesigner_NotifyChange(bool changeComplete)
		{
			NotifyChange(changeComplete);
		}

		private DesignerTransaction StartTransaction(string name)
		{
			return ((IDesignerHost)GetService(typeof(IDesignerHost)))?.CreateTransaction(name);
		}

		public DesignerTransaction PanelManagerDesigner_StartTransaction(string name)
		{
			return StartTransaction(name);
		}

		private void SelectContainer()
		{
			ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
			if (selectionService != null)
			{
				ArrayList arrayList = new ArrayList();
				arrayList.Add(_dockManager);
				selectionService.SetSelectedComponents(arrayList);
			}
		}

		public void PanelManagerDesigner_SelectContainer()
		{
			SelectContainer();
		}

		public void PanelManagerDesigner_EndPanelDragTransaction(DockPanelBase panel, DockPanelBase groupCreated)
		{
			if (_dragTransaction != null)
			{
				try
				{
					NotifyChange(changeComplete: true);
				}
				finally
				{
					CommitTransaction(ref _dragTransaction);
				}
			}
		}

		// A named change opens its own transaction, which the matching Changed call closes whatever name it passes.
		public void PanelManagerDesigner_RaiseComponentChanging(string transactionName, DockPanelBase panel)
		{
			bool opened = false;
			if (!string.IsNullOrEmpty(transactionName) && _changeTransaction == null)
			{
				_changeTransaction = StartTransaction(transactionName);
				opened = true;
			}
			try
			{
				((IComponentChangeService)GetService(typeof(IComponentChangeService)))?.OnComponentChanging(panel, null);
			}
			catch
			{
				if (opened)
				{
					CancelTransaction(ref _changeTransaction);
				}
				throw;
			}
		}

		public void PanelManagerDesigner_RaiseComponentChanged(string transactionName, DockPanelBase panel)
		{
			try
			{
				((IComponentChangeService)GetService(typeof(IComponentChangeService)))?.OnComponentChanged(panel, null, null, null);
			}
			finally
			{
				if (!string.IsNullOrEmpty(transactionName))
				{
					CommitTransaction(ref _changeTransaction);
				}
			}
		}
	}
}
