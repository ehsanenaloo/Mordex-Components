using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking.Internal;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.StatusBar;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandManager
	{
		[Description("Occurs after a layout is loaded.")]
		public event EventHandler LayoutLoad
		{
			add
			{
				base.Events.AddHandler(_layoutLoadEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_layoutLoadEvent, value);
			}
		}

		[Description("Occurs after the CurrentLayout property has changed.")]
		public event EventHandler CurrentLayoutChanged
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_currentLayoutChangedEvent, value);
			}
		}

		[Description("Occurs before the CurrentLayout property has changed")]
		public event CancelEventHandler CurrentLayoutChanging
		{
			add
			{
				base.Events.AddHandler(_currentLayoutChangingEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_currentLayoutChangingEvent, value);
			}
		}

		[Description("Occurs when the View property has changed.")]
		public event EventHandler ViewChanged
		{
			add
			{
				base.Events.AddHandler(_viewChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_viewChangedEvent, value);
			}
		}

		[Description("Event fired when a UICommandBar is about to be added to the CommandBars collection.")]
		public event CancelCommandBarEventHandler AddingCommandBar
		{
			add
			{
				base.Events.AddHandler(_addingCommandBarEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_addingCommandBarEvent, value);
			}
		}

		[Description("Event fired when a UICommandBar is about to be removed from the CommandBars collection.")]
		public event CancelCommandBarEventHandler RemovingCommandBar
		{
			add
			{
				base.Events.AddHandler(_removingCommandBarEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_removingCommandBarEvent, value);
			}
		}

		[Description("Event fired when the VisualStyle property changes.")]
		public event EventHandler VisualStyleChanged
		{
			add
			{
				base.Events.AddHandler(_visualStyleChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_visualStyleChangedEvent, value);
			}
		}

		[Description("Event fired when the system colors change.")]
		public event EventHandler SystemColorsChanged
		{
			add
			{
				base.Events.AddHandler(_systemColorsChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_systemColorsChangedEvent, value);
			}
		}

		[Description("Event fired when the ContainerControl property changes.")]
		public event EventHandler ContainerControlChanged
		{
			add
			{
				base.Events.AddHandler(_containerControlChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_containerControlChangedEvent, value);
			}
		}

		event EventHandler INotifyDesignChange.DesignChange
		{
			add
			{
				base.Events.AddHandler(_designChangeEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_designChangeEvent, value);
			}
		}

		[Description("Event fired when a UICommandBar was added to the CommandBars collection")]
		public event CommandBarEventHandler CommandBarAdded
		{
			add
			{
				base.Events.AddHandler(_commandBarAddedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandBarAddedEvent, value);
			}
		}

		[Description("Event fired when a UICommandBar was removed from the CommandBars collection")]
		public event CommandBarEventHandler CommandBarRemoved
		{
			add
			{
				base.Events.AddHandler(_commandBarRemovedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandBarRemovedEvent, value);
			}
		}

		[Description("Occurs when the mouse pointer hovers over one of the command.")]
		public event CommandEventHandler CommandMouseHover
		{
			add
			{
				base.Events.AddHandler(_commandMouseHoverEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandMouseHoverEvent, value);
			}
		}

		[Description("Occurs when the mouse pointer leaves one of the command.")]
		public event CommandEventHandler CommandMouseLeave
		{
			add
			{
				base.Events.AddHandler(_commandMouseLeaveEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandMouseLeaveEvent, value);
			}
		}

		[Description("Occurs when the user clicks on a UICommand.")]
		public event CommandEventHandler CommandClick
		{
			add
			{
				base.Events.AddHandler(_commandClickEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandClickEvent, value);
			}
		}

		[Description("Event fired when the ControlValue property of one of the commands has changed.")]
		public event CommandEventHandler CommandControlValueChanged
		{
			add
			{
				base.Events.AddHandler(_commandControlValueChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandControlValueChangedEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is dismissed and disappears.")]
		public event CommandEventHandler CommandCloseUp
		{
			add
			{
				base.Events.AddHandler(_commandCloseUpEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandCloseUpEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is about to be closed.")]
		public event CommandClosingEventHandler CommandClosing
		{
			add
			{
				base.Events.AddHandler(_commandClosingEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandClosingEvent, value);
			}
		}

		[Description("Occurs before one of the commands in the UICommandManager displays its list of subcommands.")]
		public event CommandEventHandler CommandPopup
		{
			add
			{
				base.Events.AddHandler(_commandPopupEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_commandPopupEvent, value);
			}
		}

		[Description("Occurs when one of the dialogs used for customization is about to be shown.")]
		public event CancelDialogEventHandler ShowingCustomizeDialog
		{
			add
			{
				base.Events.AddHandler(_showingCustomizeDialogEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_showingCustomizeDialogEvent, value);
			}
		}

		[Description("Occurs when one of the dialogs used for customization is shown.")]
		public event DialogEventHandler CustomizeDialogShown
		{
			add
			{
				base.Events.AddHandler(_customizeDialogShownEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_customizeDialogShownEvent, value);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandPopUp(CommandEventArgs eventArgs)
		{
			OnCommandPopup(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandCloseUp(CommandEventArgs eventArgs)
		{
			OnCommandCloseUp(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandClosing(CommandClosingEventArgs eventArgs)
		{
			OnCommandClosing(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandAdded(CommandActionEventArgs uiCommandEventArgs)
		{
			if (_editorDialogs != null)
			{
				foreach (CommandBarEditorDialog item in _editorDialogs)
				{
					item.InvalidateLayout();
				}
			}
			if (!_isBatchUpdating)
			{
				RaiseCommandAdded(uiCommandEventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandExpanded(CommandEventArgs eventArgs)
		{
		}

		void IAccessibleCommandBarsElement.OnCommandControlValueChanged(CommandEventArgs eventArgs)
		{
			if (!_isBatchUpdating)
			{
				OnCommandControlValueChanged(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandRemoved(CommandActionEventArgs uiCommandEventArgs)
		{
			if (_editorDialogs == null)
			{
				return;
			}
			foreach (CommandBarEditorDialog item in _editorDialogs)
			{
				item.InvalidateLayout();
			}
		}

		void IAccessibleCommandBarsElement.OnCommandHover(CommandEventArgs eventArgs)
		{
			OnCommandMouseHover(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandMouseLeave(CommandEventArgs eventArgs)
		{
			OnCommandMouseLeave(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandClicked(CommandEventArgs eventArgs)
		{
			if (_activeBar != null && _activeBar.GetChildMenu() == null)
			{
				SetActiveBar(null);
			}
			OnCommandClick(eventArgs);
		}

		void IFormatStyleOwner.PropertyChange()
		{
			RefreshRebars();
		}

		void IAccessibleCommandBarsElement.OnCommandCollapsed(CommandEventArgs eventArgs)
		{
		}

		void IAccessibleCommandBarsElement.OnInvalidateCommand()
		{
		}

		void ICommandBarOwner.OnBarAdded(MxCommandBar commandBar)
		{
			commandBar._commandManager = this;
			if (base.DesignMode && base.Container != null && commandBar.Container == null)
			{
				base.Container.Add(commandBar);
			}
			if (!_isBatchUpdating)
			{
				commandBar._forceDockStyleUpdate = true;
				commandBar.DockStyle = commandBar.DockStyle;
				commandBar.BindCommands();
				CommandBarEventArgs e = new CommandBarEventArgs(commandBar);
				_isToolbarsMenuDirty = true;
				OnCommandBarAdded(e);
				RefreshRebars();
			}
		}

		void ICommandBarOwner.OnBarRemoved(MxCommandBar commandBar)
		{
			commandBar.SetFloatingWindow(null);
			commandBar.SetHost(null);
			if (base.DesignMode && base.Container != null && commandBar.Container != null)
			{
				commandBar.Commands.DetachAll();
				for (int i = 0; i < commandBar.Commands.Count; i++)
				{
					MxCommand uiCommand = commandBar.Commands[i];
					uiCommand.ForceDispose();
				}
			}
			if (!_isBatchUpdating)
			{
				CommandBarEventArgs e = new CommandBarEventArgs(commandBar);
				_isToolbarsMenuDirty = true;
				OnCommandBarRemoved(e);
				RefreshRebars();
				commandBar._commandManager = null;
			}
		}

		protected virtual void OnLayoutLoad(EventArgs e)
		{
			if (base.Events[_layoutLoadEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanged(EventArgs e)
		{
			if (base.Events[_currentLayoutChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnCurrentLayoutChanging(CancelEventArgs e)
		{
			if (base.Events[_currentLayoutChangingEvent] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnViewChanged(EventArgs e)
		{
			if (base.Events[_viewChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnAddingCommandBar(CancelCommandBarEventArgs e)
		{
			if (base.Events[_addingCommandBarEvent] is CancelCommandBarEventHandler cancelCommandBarEventHandler)
			{
				cancelCommandBarEventHandler(this, e);
			}
		}

		protected virtual void OnVisualStyleChanged(EventArgs e)
		{
			if (base.Events[_visualStyleChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnSystemColorsChanged(EventArgs e)
		{
			if (base.Events[_systemColorsChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnContainerControlChanged(EventArgs e)
		{
			if (base.Events[_containerControlChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal void OnDesignChange(EventArgs eventArgs)
		{
			if (base.Events[_designChangeEvent] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		internal void OnRemovingCommandBar(CancelCommandBarEventArgs eventArgs)
		{
			if (base.Events[_removingCommandBarEvent] is CancelCommandBarEventHandler cancelCommandBarEventHandler)
			{
				cancelCommandBarEventHandler(this, eventArgs);
			}
		}

		protected virtual void OnCommandBarAdded(CommandBarEventArgs e)
		{
			if (base.Events[_commandBarAddedEvent] is CommandBarEventHandler commandBarEventHandler)
			{
				commandBarEventHandler(this, e);
			}
		}

		protected virtual void OnCommandBarRemoved(CommandBarEventArgs e)
		{
			if (base.Events[_commandBarRemovedEvent] is CommandBarEventHandler commandBarEventHandler)
			{
				commandBarEventHandler(this, e);
			}
		}

		internal void AddCommandAddedHandler(CommandActionHandler commandBarsCallback)
		{
			base.Events.AddHandler(_commandAddedEvent, commandBarsCallback);
		}

		internal void RemoveCommandAddedHandler(CommandActionHandler commandBarsCallback)
		{
			base.Events.RemoveHandler(_commandAddedEvent, commandBarsCallback);
		}

		internal void RaiseCommandAdded(CommandActionEventArgs uiCommandEventArgs)
		{
			if (base.Events[_commandAddedEvent] is CommandActionHandler commandBarsCallback)
			{
				commandBarsCallback(this, uiCommandEventArgs);
			}
		}

		protected virtual void OnCommandMouseHover(CommandEventArgs e)
		{
			if (base.Events[_commandMouseHoverEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandMouseLeave(CommandEventArgs e)
		{
			if (base.Events[_commandMouseLeaveEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandClick(CommandEventArgs e)
		{
			if (base.Events[_commandClickEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandControlValueChanged(CommandEventArgs e)
		{
			if (base.Events[_commandControlValueChangedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandCloseUp(CommandEventArgs e)
		{
			if (base.Events[_commandCloseUpEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCommandClosing(CommandClosingEventArgs e)
		{
			if (base.Events[_commandClosingEvent] is CommandClosingEventHandler commandClosingEventHandler)
			{
				commandClosingEventHandler(this, e);
			}
		}

		protected virtual void OnCommandPopup(CommandEventArgs e)
		{
			if (base.Events[_commandPopupEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		internal void AddSelectedCommandChangedHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_selectedCommandChangedEvent, eventHandler);
		}

		internal void RemoveSelectedCommandChangedHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(_selectedCommandChangedEvent, eventHandler);
		}

		internal void OnSelectedCommandChanged(EventArgs eventArgs)
		{
			if (base.Events[_selectedCommandChangedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		protected virtual void OnShowingCustomizeDialog(CancelDialogEventArgs e)
		{
			if (base.Events[_showingCustomizeDialogEvent] is CancelDialogEventHandler cancelDialogEventHandler)
			{
				cancelDialogEventHandler(this, e);
			}
		}

		protected virtual void OnCustomizeDialogShown(DialogEventArgs e)
		{
			if (base.Events[_customizeDialogShownEvent] is DialogEventHandler dialogEventHandler)
			{
				dialogEventHandler(this, e);
			}
		}
	}
}
