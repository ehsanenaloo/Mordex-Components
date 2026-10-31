using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommand
	{
		[Description("Occurs when the mouse pointer hovers over the command.")]
		public event CommandEventHandler MouseHover
		{
			add
			{
				base.Events.AddHandler(_mouseHoverEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mouseHoverEvent, value);
			}
		}

		[Description("Occurs when the mouse pointer leaves the command.")]
		public event CommandEventHandler MouseLeave
		{
			add
			{
				base.Events.AddHandler(_mouseLeaveEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_mouseLeaveEvent, value);
			}
		}

		[Description("Occurs when the ControlValue property has changed.")]
		public event CommandEventHandler ControlValueChanged
		{
			add
			{
				base.Events.AddHandler(_controlValueChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_controlValueChangedEvent, value);
			}
		}

		[Description("Occurs when the UICommand is clicked.")]
		public event CommandEventHandler Click
		{
			add
			{
				base.Events.AddHandler(_clickEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_clickEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of subitems is dismissed and disappears.")]
		public event CommandEventHandler CloseUp
		{
			add
			{
				base.Events.AddHandler(_closeUpEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_closeUpEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of subitems is about to be closed.")]
		public event CommandClosingEventHandler Closing
		{
			add
			{
				base.Events.AddHandler(_closingEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_closingEvent, value);
			}
		}

		[Description("Occurs before a list of subcommands is displayed.")]
		public event CommandEventHandler Popup
		{
			add
			{
				base.Events.AddHandler(_popupEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_popupEvent, value);
			}
		}

		internal void NotifyInvalidated()
		{
			RaiseInvalidated(EventArgs.Empty);
		}

		private void RelayInvalidated(object sender, EventArgs eventArgs)
		{
			RaiseInvalidated(EventArgs.Empty);
		}

		private void RelayCommandCollapsed(object sender, CommandEventArgs eventArgs)
		{
			RaiseCommandCollapsed(eventArgs);
		}

		private void RelayCommandExpanded(object sender, CommandEventArgs eventArgs)
		{
			RaiseCommandExpanded(eventArgs);
		}

		private void RelayCommandClicked(object sender, CommandEventArgs eventArgs)
		{
			RaiseCommandClicked(eventArgs);
		}

		private void OnBaseCommandControlValueChanged(object sender, CommandEventArgs eventArgs)
		{
			CommandEventArgs e = new CommandEventArgs(this);
			OnControlValueChanged(e);
		}

		private void RelayCommandControlValueChanged(object sender, CommandEventArgs eventArgs)
		{
			RaiseCommandControlValueChanged(eventArgs);
		}

		private void RelayCommandHover(object sender, CommandEventArgs eventArgs)
		{
			RaiseCommandHover(eventArgs);
		}

		private void RelayCommandMouseLeave(object sender, CommandEventArgs eventArgs)
		{
			RaiseCommandMouseLeave(eventArgs);
		}

		private void RelayCommandPopup(object sender, CommandEventArgs eventArgs)
		{
			RaiseCommandPopup(eventArgs);
		}

		private void RelayCommandCloseUp(object sender, CommandEventArgs eventArgs)
		{
			RaiseCommandCloseUp(eventArgs);
		}

		private void RelayCommandClosing(object sender, CommandClosingEventArgs eventArgs)
		{
			RaiseCommandClosing(eventArgs);
		}

		private void OnBaseCommandDisposed(object sender, CommandEventArgs eventArgs)
		{
			Dispose();
		}

		private void RelayCommandRemoved(object value, CommandActionEventArgs uiCommandEventArgs)
		{
			RaiseCommandRemoved(uiCommandEventArgs);
		}

		private void RelayCommandAdded(object value, CommandActionEventArgs uiCommandEventArgs)
		{
			RaiseCommandAdded(uiCommandEventArgs);
		}

		internal void NotifyPopup()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			if (_baseCommand != null)
			{
				CommandEventArgs baseEventArgs = new CommandEventArgs(BaseCommand);
				_baseCommand.OnPopup(baseEventArgs);
			}
			OnPopup(e);
		}

		internal void NotifyCloseUp()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			if (_baseCommand != null)
			{
				CommandEventArgs baseEventArgs = new CommandEventArgs(BaseCommand);
				_baseCommand.OnCloseUp(baseEventArgs);
			}
			OnCloseUp(e);
		}

		internal bool ConfirmClose(CommandCloseReason closeReason)
		{
			CommandClosingEventArgs e = new CommandClosingEventArgs(this, closeReason);
			if (_baseCommand != null)
			{
				CommandClosingEventArgs baseEventArgs = new CommandClosingEventArgs(BaseCommand, closeReason);
				_baseCommand.OnClosing(baseEventArgs);
				e.Cancel = baseEventArgs.Cancel;
			}
			OnClosing(e);
			return !e.Cancel;
		}

		internal void NotifyMouseHover()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			if (_baseCommand != null)
			{
				CommandEventArgs baseEventArgs = new CommandEventArgs(BaseCommand);
				_baseCommand.OnMouseHover(baseEventArgs);
			}
			OnMouseHover(e);
		}

		internal void NotifyMouseLeave()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			if (_baseCommand != null)
			{
				CommandEventArgs baseEventArgs = new CommandEventArgs(BaseCommand);
				_baseCommand.OnMouseLeave(baseEventArgs);
			}
			OnMouseLeave(e);
		}

		void IAccessibleCommandBarsElement.OnInvalidateCommand()
		{
			RaiseInvalidated(EventArgs.Empty);
		}

		void IAccessibleCommandBarsElement.OnCommandExpanded(CommandEventArgs eventArgs)
		{
			RaiseCommandExpanded(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandCollapsed(CommandEventArgs eventArgs)
		{
			RaiseCommandCollapsed(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandAdded(CommandActionEventArgs uiCommandEventArgs)
		{
			RaiseCommandAdded(uiCommandEventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandRemoved(CommandActionEventArgs uiCommandEventArgs)
		{
			RaiseCommandRemoved(uiCommandEventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandControlValueChanged(CommandEventArgs eventArgs)
		{
			RaiseCommandControlValueChanged(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandClicked(CommandEventArgs eventArgs)
		{
			SetLastClickedDate(eventArgs.Command.GetLastClickedDate());
			RaiseCommandClicked(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandHover(CommandEventArgs eventArgs)
		{
			RaiseCommandHover(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandMouseLeave(CommandEventArgs eventArgs)
		{
			RaiseCommandMouseLeave(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandPopUp(CommandEventArgs eventArgs)
		{
			RaiseCommandPopup(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandCloseUp(CommandEventArgs eventArgs)
		{
			RaiseCommandCloseUp(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandClosing(CommandClosingEventArgs eventArgs)
		{
			RaiseCommandClosing(eventArgs);
		}

		void IFormatStyleOwner.PropertyChange()
		{
			NotifyInvalidated();
		}

		protected virtual void OnMouseHover(CommandEventArgs e)
		{
			if (base.Events[_mouseHoverEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnMouseLeave(CommandEventArgs e)
		{
			if (base.Events[_mouseLeaveEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnControlValueChanged(CommandEventArgs e)
		{
			if (base.Events[_controlValueChangedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnClick(CommandEventArgs e)
		{
			if (base.Events[_clickEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnCloseUp(CommandEventArgs e)
		{
			if (base.Events[_closeUpEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnClosing(CommandClosingEventArgs e)
		{
			if (base.Events[_closingEvent] is CommandClosingEventHandler commandClosingEventHandler)
			{
				commandClosingEventHandler(this, e);
			}
		}

		protected virtual void OnPopup(CommandEventArgs e)
		{
			if (base.Events[_popupEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		internal void AddCommandHoverHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandHoverEvent, commandEventHandler);
		}

		internal void RemoveCommandHoverHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandHoverEvent, commandEventHandler);
		}

		internal void RaiseCommandHover(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandHoverEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandMouseLeaveHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandMouseLeaveEvent, commandEventHandler);
		}

		internal void RemoveCommandMouseLeaveHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandMouseLeaveEvent, commandEventHandler);
		}

		internal void RaiseCommandMouseLeave(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandMouseLeaveEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandControlValueChangedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandControlValueChangedEvent, commandEventHandler);
		}

		internal void RemoveCommandControlValueChangedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandControlValueChangedEvent, commandEventHandler);
		}

		internal void RaiseCommandControlValueChanged(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandControlValueChangedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandClickedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandClickedEvent, commandEventHandler);
		}

		internal void RemoveCommandClickedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandClickedEvent, commandEventHandler);
		}

		internal void RaiseCommandClicked(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandClickedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandExpandedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandExpandedEvent, commandEventHandler);
		}

		internal void RemoveCommandExpandedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandExpandedEvent, commandEventHandler);
		}

		internal void RaiseCommandExpanded(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandExpandedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandCollapsedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandCollapsedEvent, commandEventHandler);
		}

		internal void RemoveCommandCollapsedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandCollapsedEvent, commandEventHandler);
		}

		internal void RaiseCommandCollapsed(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandCollapsedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandCloseUpHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandCloseUpEvent, commandEventHandler);
		}

		internal void RemoveCommandCloseUpHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandCloseUpEvent, commandEventHandler);
		}

		internal void RaiseCommandCloseUp(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandCloseUpEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandClosingHandler(CommandClosingEventHandler commandClosingEventHandler)
		{
			base.Events.AddHandler(_commandClosingEvent, commandClosingEventHandler);
		}

		internal void RemoveCommandClosingHandler(CommandClosingEventHandler commandClosingEventHandler)
		{
			base.Events.RemoveHandler(_commandClosingEvent, commandClosingEventHandler);
		}

		internal void RaiseCommandClosing(CommandClosingEventArgs eventArgs)
		{
			if (base.Events[_commandClosingEvent] is CommandClosingEventHandler commandClosingEventHandler)
			{
				commandClosingEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandPopupHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandPopupEvent, commandEventHandler);
		}

		internal void RemoveCommandPopupHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandPopupEvent, commandEventHandler);
		}

		internal void RaiseCommandPopup(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandPopupEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCommandRemovedHandler(CommandActionHandler commandBarsCallback)
		{
			base.Events.AddHandler(_commandRemovedEvent, commandBarsCallback);
		}

		internal void RemoveCommandRemovedHandler(CommandActionHandler commandBarsCallback)
		{
			base.Events.RemoveHandler(_commandRemovedEvent, commandBarsCallback);
		}

		internal void RaiseCommandRemoved(CommandActionEventArgs uiCommandEventArgs)
		{
			if (base.Events[_commandRemovedEvent] is CommandActionHandler commandBarsCallback)
			{
				commandBarsCallback(this, uiCommandEventArgs);
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

		internal void AddCommandDisposedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_commandDisposedEvent, commandEventHandler);
		}

		internal void RemoveCommandDisposedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_commandDisposedEvent, commandEventHandler);
		}

		private void RaiseCommandDisposed(CommandEventArgs eventArgs)
		{
			if (base.Events[_commandDisposedEvent] is CommandEventHandler commandEventHandler && !IsMergedCommand)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddInvalidatedHandler(EventHandler eventHandler)
		{
			base.Events.AddHandler(_invalidatedEvent, eventHandler);
		}

		internal void RemoveInvalidatedHandler(EventHandler eventHandler)
		{
			base.Events.RemoveHandler(_invalidatedEvent, eventHandler);
		}

		internal void RaiseInvalidated(EventArgs eventArgs)
		{
			if (_floatingBar != null)
			{
				((IAccessibleCommandBarsElement)_floatingBar).OnInvalidateCommand();
			}
			if (base.Events[_invalidatedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		internal void AddExpandRequestedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_expandRequestedEvent, commandEventHandler);
		}

		internal void RemoveExpandRequestedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_expandRequestedEvent, commandEventHandler);
		}

		internal void RaiseExpandRequested(CommandEventArgs eventArgs)
		{
			if (base.Events[_expandRequestedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}

		internal void AddCollapseRequestedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.AddHandler(_collapseRequestedEvent, commandEventHandler);
		}

		internal void RemoveCollapseRequestedHandler(CommandEventHandler commandEventHandler)
		{
			base.Events.RemoveHandler(_collapseRequestedEvent, commandEventHandler);
		}

		internal void RaiseCollapseRequested(CommandEventArgs eventArgs)
		{
			if (base.Events[_collapseRequestedEvent] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, eventArgs);
			}
		}
	}
}
