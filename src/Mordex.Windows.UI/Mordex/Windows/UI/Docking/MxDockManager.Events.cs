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
		public event MdiTabGroupEventHandler MdiTabGroupDestroyed
		{
			add
			{
				base.Events.AddHandler(_mdiTabGroupDestroyedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabGroupDestroyedKey, value);
			}
		}

		public event MdiTabGroupCreatedEventHandler MdiTabGroupCreated
		{
			add
			{
				base.Events.AddHandler(_mdiTabGroupCreatedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabGroupCreatedKey, value);
			}
		}

		public event MdiTabEventHandler MdiTabCreated
		{
			add
			{
				base.Events.AddHandler(_mdiTabCreatedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabCreatedKey, value);
			}
		}

		[Description("Occurs when an Mdi tab is restored from a layout file.")]
		public event RestoreMdiChildFormEventHandler RestoreMdiChildForm
		{
			add
			{
				base.Events.AddHandler(_restoreMdiChildFormKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_restoreMdiChildFormKey, value);
			}
		}

		[Description("Occurs when a tab representing an Mdi child form in a tab group is being initialized. ")]
		public event MdiTabInitializeEventHandler MdiTabInitialize
		{
			add
			{
				base.Events.AddHandler(_mdiTabInitializeKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabInitializeKey, value);
			}
		}

		[Description("Occurs when the mouse pointer is over a tab representing an Mdi child form and a mouse button is pressed.")]
		public event MdiTabMouseEventHandler MdiTabMouseDown
		{
			add
			{
				base.Events.AddHandler(_mdiTabMouseDownKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabMouseDownKey, value);
			}
		}

		[Description("Occurs when an mdi tab is about to be closed by the user.")]
		public event MdiTabClosingEventHandler MdiTabClosing
		{
			add
			{
				base.Events.AddHandler(_mdiTabClosingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabClosingKey, value);
			}
		}

		[Description("Occurs when an mdi tab was closed by the user.")]
		public event MdiTabClosedEventHandler MdiTabClosed
		{
			add
			{
				base.Events.AddHandler(_mdiTabClosedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabClosedKey, value);
			}
		}

		[Description("Occurs when the mouse pointer is over a tab representing an Mdi child form and a mouse button is released.")]
		public event MdiTabMouseEventHandler MdiTabMouseUp
		{
			add
			{
				base.Events.AddHandler(_mdiTabMouseUpKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabMouseUpKey, value);
			}
		}

		[Description("Occurs when a tab representing an Mdi child form is clicked.")]
		public event MdiTabEventHandler MdiTabClick
		{
			add
			{
				base.Events.AddHandler(_mdiTabClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabClickKey, value);
			}
		}

		[Description("Occurs when a tab representing an Mdi child form is double-clicked.")]
		public event MdiTabEventHandler MdiTabDoubleClick
		{
			add
			{
				base.Events.AddHandler(_mdiTabDoubleClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mdiTabDoubleClickKey, value);
			}
		}

		public event PanelActionEventHandler PanelGroupCreated
		{
			add
			{
				base.Events.AddHandler(_panelGroupCreatedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelGroupCreatedKey, value);
			}
		}

		[Description("Occurs after a layout is loaded.")]
		public event EventHandler LayoutLoad
		{
			add
			{
				base.Events.AddHandler(_layoutLoadKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_layoutLoadKey, value);
			}
		}

		[Description("Occurs after the CurrentLayout property has changed.")]
		public event EventHandler CurrentLayoutChanged
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_currentLayoutChangedKey, value);
			}
		}

		[Description("Event fired when the ContainerControl property changes.")]
		public event EventHandler ContainerControlChanged
		{
			add
			{
				base.Events.AddHandler(_containerControlChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_containerControlChangedKey, value);
			}
		}

		[Description("Event fired when the system colors change.")]
		public event EventHandler SystemColorsChanged
		{
			add
			{
				base.Events.AddHandler(_systemColorsChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_systemColorsChangedKey, value);
			}
		}

		[Description("Event fired when the VisualStyle property changes.")]
		public event EventHandler VisualStyleChanged
		{
			add
			{
				base.Events.AddHandler(_visualStyleChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_visualStyleChangedKey, value);
			}
		}

		[Description("Occurs before the CurrentLayout property changes")]
		public event CancelEventHandler CurrentLayoutChanging
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_currentLayoutChangingKey, value);
			}
		}

		[Description("Occurs when the SelectedPanel property is about to change in a tab-like panel group.")]
		public event GroupSelectedPanelChangingEventHandler GroupSelectedPanelChanging
		{
			add
			{
				base.Events.AddHandler(_groupSelectedPanelChangingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_groupSelectedPanelChangingKey, value);
			}
		}

		[Description("Occurs when the SelectedPanel property changes in a tab-like panel group.")]
		public event GroupSelectedPanelChangedEventHandler GroupSelectedPanelChanged
		{
			add
			{
				base.Events.AddHandler(_groupSelectedPanelChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_groupSelectedPanelChangedKey, value);
			}
		}

		[Description("Occurs when a panel drag operation ends.")]
		public event PanelDragDropEventHandler PanelDrop
		{
			add
			{
				base.Events.AddHandler(_panelDropKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelDropKey, value);
			}
		}

		[Description("Occurs when the user is dragging a panel.")]
		public event PanelDragDropEventHandler DraggingPanel
		{
			add
			{
				base.Events.AddHandler(_draggingPanelKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_draggingPanelKey, value);
			}
		}

		[Description("Occurs when the user is about to drag a panel or a panel group.")]
		public event PanelActionCancelEventHandler PanelStartDrag
		{
			add
			{
				base.Events.AddHandler(_panelStartDragKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelStartDragKey, value);
			}
		}

		[Description("Occurs when the Closed property is about to change in a panel or a panel group.")]
		public event PanelActionCancelEventHandler PanelClosedChanging
		{
			add
			{
				base.Events.AddHandler(_panelClosedChangingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelClosedChangingKey, value);
			}
		}

		[Description("Occurs when the DockStyle property in a panel or a panel group changes.")]
		public event PanelActionEventHandler PanelDockStyleChanged
		{
			add
			{
				base.Events.AddHandler(_panelDockStyleChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelDockStyleChangedKey, value);
			}
		}

		[Description("Occurs when the DockState property in a panel or a panel group changes.")]
		public event PanelActionEventHandler PanelDockStateChanged
		{
			add
			{
				base.Events.AddHandler(_panelDockStateChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelDockStateChangedKey, value);
			}
		}

		[Description("Occurs when the Closed property in a panel or a panel group changes.")]
		public event PanelActionEventHandler PanelClosedChanged
		{
			add
			{
				base.Events.AddHandler(_panelClosedChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelClosedChangedKey, value);
			}
		}

		[Description("Occurs when a new panel becomes active.")]
		public event PanelActionEventHandler PanelActivated
		{
			add
			{
				base.Events.AddHandler(_panelActivatedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelActivatedKey, value);
			}
		}

		[Description("Occurs when the user has closed a panel or a panel group.")]
		public event PanelActionEventHandler PanelDeactivate
		{
			add
			{
				base.Events.AddHandler(_panelDeactivateKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelDeactivateKey, value);
			}
		}

		[Description("Occurs when the user is about to set a panel or a panel group as an autohide panel.")]
		public event PanelActionCancelEventHandler PanelAutoHideChanging
		{
			add
			{
				base.Events.AddHandler(_panelAutoHideChangingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelAutoHideChangingKey, value);
			}
		}

		[Description("Occurs when the user set a panel or a panel group as AutoHide.")]
		public event PanelActionEventHandler PanelAutoHideChanged
		{
			add
			{
				base.Events.AddHandler(_panelAutoHideChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelAutoHideChangedKey, value);
			}
		}

		[Description("Occurs when the user shows or hides an auto hide panel.")]
		public event PanelActionEventHandler PanelAutoHideActiveChanged
		{
			add
			{
				base.Events.AddHandler(_panelAutoHideActiveChangedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelAutoHideActiveChangedKey, value);
			}
		}

		[Description("Occurs when an auto hide panel is about to be displayed or hidden.")]
		public event PanelActionCancelEventHandler PanelAutoHideActiveChanging
		{
			add
			{
				base.Events.AddHandler(_panelAutoHideActiveChangingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_panelAutoHideActiveChangingKey, value);
			}
		}

		internal void RaiseMdiTabInitialize(MdiChildTab tab)
		{
			MdiTabInitializeEventArgs e = new MdiTabInitializeEventArgs(tab);
			try
			{
				OnMdiTabInitialize(e);
			}
			catch
			{
				// This runs while the tab is being built. A failing user handler must not leave the tab group half built.
			}
		}

		internal bool RaiseMdiTabClosing(MdiChildTab tab)
		{
			MdiTabClosingEventArgs e = new MdiTabClosingEventArgs(tab);
			OnMdiTabClosing(e);
			return !e.Cancel;
		}

		internal void RaiseMdiTabClosed(MdiChildTab tab)
		{
			MdiTabClosedEventArgs e = new MdiTabClosedEventArgs(tab);
			OnMdiTabClosed(e);
		}

		internal void RaiseMdiTabMouseDown(MdiChildTab tab, MouseEventArgs eventArgs)
		{
			MdiTabMouseEventArgs e = new MdiTabMouseEventArgs(eventArgs.Button, eventArgs.Clicks, eventArgs.X, eventArgs.Y, eventArgs.Delta, tab);
			OnMdiTabMouseDown(e);
		}

		internal void RaiseMdiTabMouseUp(MdiChildTab tab, MouseEventArgs eventArgs)
		{
			MdiTabMouseEventArgs e = new MdiTabMouseEventArgs(eventArgs.Button, eventArgs.Clicks, eventArgs.X, eventArgs.Y, eventArgs.Delta, tab);
			OnMdiTabMouseUp(e);
		}

		internal void RaiseMdiTabClick(MdiChildTab tab)
		{
			MdiTabEventArgs e = new MdiTabEventArgs(tab);
			OnMdiTabClick(e);
		}

		internal void RaiseMdiTabDoubleClick(MdiChildTab tab)
		{
			MdiTabEventArgs e = new MdiTabEventArgs(tab);
			OnMdiTabDoubleClick(e);
		}

		internal void RaisePanelGroupCreated(MxDockPanelGroup panel)
		{
			PanelActionEventArgs e = new PanelActionEventArgs(panel);
			OnPanelGroupCreated(e);
		}

		protected virtual void OnMdiTabGroupDestroyed(MdiTabGroupEventArgs e)
		{
			if (base.Events[_mdiTabGroupDestroyedKey] is MdiTabGroupEventHandler mdiTabGroupEventHandler)
			{
				mdiTabGroupEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabGroupCreated(MdiTabGroupCreatedEventArgs e)
		{
			if (base.Events[_mdiTabGroupCreatedKey] is MdiTabGroupCreatedEventHandler mdiTabGroupCreatedEventHandler)
			{
				mdiTabGroupCreatedEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabCreated(MdiTabEventArgs e)
		{
			if (base.Events[_mdiTabCreatedKey] is MdiTabEventHandler mdiTabEventHandler)
			{
				mdiTabEventHandler(this, e);
			}
		}

		protected virtual void OnRestoreMdiChildForm(RestoreMdiChildFormEventArgs e)
		{
			if (base.Events[_restoreMdiChildFormKey] is RestoreMdiChildFormEventHandler restoreMdiChildFormEventHandler)
			{
				restoreMdiChildFormEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabInitialize(MdiTabInitializeEventArgs e)
		{
			if (base.Events[_mdiTabInitializeKey] is MdiTabInitializeEventHandler mdiTabInitializeEventHandler)
			{
				mdiTabInitializeEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabMouseDown(MdiTabMouseEventArgs e)
		{
			if (base.Events[_mdiTabMouseDownKey] is MdiTabMouseEventHandler mdiTabMouseEventHandler)
			{
				mdiTabMouseEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabClosing(MdiTabClosingEventArgs e)
		{
			if (base.Events[_mdiTabClosingKey] is MdiTabClosingEventHandler mdiTabClosingEventHandler)
			{
				mdiTabClosingEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabClosed(MdiTabClosedEventArgs e)
		{
			if (base.Events[_mdiTabClosedKey] is MdiTabClosedEventHandler mdiTabClosedEventHandler)
			{
				mdiTabClosedEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabMouseUp(MdiTabMouseEventArgs e)
		{
			if (base.Events[_mdiTabMouseUpKey] is MdiTabMouseEventHandler mdiTabMouseEventHandler)
			{
				mdiTabMouseEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabClick(MdiTabEventArgs e)
		{
			if (base.Events[_mdiTabClickKey] is MdiTabEventHandler mdiTabEventHandler)
			{
				mdiTabEventHandler(this, e);
			}
		}

		protected virtual void OnMdiTabDoubleClick(MdiTabEventArgs e)
		{
			if (base.Events[_mdiTabDoubleClickKey] is MdiTabEventHandler mdiTabEventHandler)
			{
				mdiTabEventHandler(this, e);
			}
		}

		protected virtual void OnPanelGroupCreated(PanelActionEventArgs e)
		{
			if (base.Events[_panelGroupCreatedKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		protected virtual void OnLayoutLoad(EventArgs e)
		{
			if (base.Events[_layoutLoadKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanged(EventArgs e)
		{
			if (base.Events[_currentLayoutChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnContainerControlChanged(EventArgs e)
		{
			if (base.Events[_containerControlChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnSystemColorsChanged(EventArgs e)
		{
			if (base.Events[_systemColorsChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnVisualStyleChanged(EventArgs e)
		{
			if (base.Events[_visualStyleChangedKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[_currentLayoutChangingKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnGroupSelectedPanelChanging(GroupSelectedPanelChangingEventArgs e)
		{
			if (base.Events[_groupSelectedPanelChangingKey] is GroupSelectedPanelChangingEventHandler groupSelectedPanelChangingEventHandler)
			{
				groupSelectedPanelChangingEventHandler(this, e);
			}
		}

		internal void RaiseGroupSelectedPanelChanging(GroupSelectedPanelChangingEventArgs eventArgs)
		{
			if (_containerControl != null)
			{
				OnGroupSelectedPanelChanging(eventArgs);
			}
		}

		protected virtual void OnGroupSelectedPanelChanged(GroupSelectedPanelChangedEventArgs e)
		{
			if (base.Events[_groupSelectedPanelChangedKey] is GroupSelectedPanelChangedEventHandler groupSelectedPanelChangedEventHandler)
			{
				groupSelectedPanelChangedEventHandler(this, e);
			}
		}

		internal void RaiseGroupSelectedPanelChanged(GroupSelectedPanelChangedEventArgs eventArgs)
		{
			if (_containerControl != null)
			{
				LayoutAutoHideStrips();
				OnGroupSelectedPanelChanged(eventArgs);
			}
		}

		protected virtual void OnPanelDrop(PanelDragDropEventArgs e)
		{
			if (base.Events[_panelDropKey] is PanelDragDropEventHandler panelDragDropEventHandler)
			{
				panelDragDropEventHandler(this, e);
			}
		}

		protected virtual void OnDraggingPanel(PanelDragDropEventArgs e)
		{
			if (base.Events[_draggingPanelKey] is PanelDragDropEventHandler panelDragDropEventHandler)
			{
				panelDragDropEventHandler(this, e);
			}
		}

		protected virtual void OnPanelStartDrag(PanelActionCancelEventArgs e)
		{
			if (base.Events[_panelStartDragKey] is PanelActionCancelEventHandler panelActionCancelEventHandler)
			{
				panelActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnPanelClosedChanging(PanelActionCancelEventArgs e)
		{
			if (base.Events[_panelClosedChangingKey] is PanelActionCancelEventHandler panelActionCancelEventHandler)
			{
				panelActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnPanelDockStyleChanged(PanelActionEventArgs e)
		{
			if (base.Events[_panelDockStyleChangedKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		protected virtual void OnPanelDockStateChanged(PanelActionEventArgs e)
		{
			if (base.Events[_panelDockStateChangedKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		protected virtual void OnPanelClosedChanged(PanelActionEventArgs e)
		{
			if (base.Events[_panelClosedChangedKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		protected virtual void OnPanelActivated(PanelActionEventArgs e)
		{
			if (base.Events[_panelActivatedKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		protected virtual void OnPanelDeactivate(PanelActionEventArgs e)
		{
			if (base.Events[_panelDeactivateKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		protected virtual void OnPanelAutoHideChanging(PanelActionCancelEventArgs e)
		{
			if (base.Events[_panelAutoHideChangingKey] is PanelActionCancelEventHandler panelActionCancelEventHandler)
			{
				panelActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnPanelAutoHideChanged(PanelActionEventArgs e)
		{
			if (base.Events[_panelAutoHideChangedKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		protected virtual void OnPanelAutoHideActiveChanged(PanelActionEventArgs e)
		{
			if (base.Events[_panelAutoHideActiveChangedKey] is PanelActionEventHandler panelActionEventHandler)
			{
				panelActionEventHandler(this, e);
			}
		}

		protected virtual void OnPanelAutoHideActiveChanging(PanelActionCancelEventArgs e)
		{
			if (base.Events[_panelAutoHideActiveChangingKey] is PanelActionCancelEventHandler panelActionCancelEventHandler)
			{
				panelActionCancelEventHandler(this, e);
			}
		}

		internal void RaiseMdiTabGroupCreated(MxMdiTabGroup group, MdiChildTab tab)
		{
			MdiTabGroupCreatedEventArgs e = new MdiTabGroupCreatedEventArgs(group, tab);
			OnMdiTabGroupCreated(e);
		}

		internal void RaiseMdiTabGroupDestroyed(MxMdiTabGroup group)
		{
			MdiTabGroupEventArgs e = new MdiTabGroupEventArgs(group);
			OnMdiTabGroupDestroyed(e);
		}

		internal void RaiseMdiTabCreated(MdiChildTab tab)
		{
			MdiTabEventArgs e = new MdiTabEventArgs(tab);
			OnMdiTabCreated(e);
		}
	}
}
