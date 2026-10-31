using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxContextMenu
	{
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

		[Description("Occurs when the mouse pointer leaves one of the associated commands.")]
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

		[Description("Occurs before the context menu is displayed.")]
		public event EventHandler Popup
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

		[Description("Occurs before the context menu is closed.")]
		public event EventHandler Closed
		{
			add
			{
				base.Events.AddHandler(_closedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(_closedEvent, value);
			}
		}

		[Description("Occurs when the context menu is about to be closed.")]
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

		[Description("Occurs before one of the commands in the UIContextMenu displays its list of subcommands.")]
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

		void IAccessibleCommandBarsElement.OnInvalidateCommand()
		{
			InvalidateMenu();
		}

		void IAccessibleCommandBarsElement.OnCommandPopUp(CommandEventArgs eventArgs)
		{
			if (!eventArgs.Command.IsMergedCommand)
			{
				OnCommandPopup(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandCloseUp(CommandEventArgs eventArgs)
		{
			if (!eventArgs.Command.IsMergedCommand)
			{
				OnCommandCloseUp(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandClosing(CommandClosingEventArgs eventArgs)
		{
			if (!eventArgs.Command.IsMergedCommand)
			{
				OnCommandClosing(eventArgs);
			}
		}

		MxCommandManager IAccessibleCommandBarsElement.GetCommandManager()
		{
			return CommandManager;
		}

		void IAccessibleCommandBarsElement.OnCommandExpanded(CommandEventArgs eventArgs)
		{
			ShowPopup(_sourceControl, Cursor.Position, 0);
		}

		void IAccessibleCommandBarsElement.OnCommandAdded(CommandActionEventArgs uiCommandEventArgs)
		{
			InvalidateMenu();
		}

		void IAccessibleCommandBarsElement.OnCommandRemoved(CommandActionEventArgs uiCommandEventArgs)
		{
			InvalidateMenu();
		}

		void IAccessibleCommandBarsElement.OnCommandHover(CommandEventArgs eventArgs)
		{
			if (!eventArgs.Command.IsMergedCommand)
			{
				OnCommandMouseHover(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandMouseLeave(CommandEventArgs eventArgs)
		{
			if (!eventArgs.Command.IsMergedCommand)
			{
				OnCommandMouseLeave(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandControlValueChanged(CommandEventArgs eventArgs)
		{
			if (!eventArgs.Command.IsMergedCommand)
			{
				OnCommandControlValueChanged(eventArgs);
			}
		}

		void IAccessibleCommandBarsElement.OnCommandClicked(CommandEventArgs eventArgs)
		{
			if (!_isProcessingShortcut && GetPopupControl() == null && !eventArgs.Command._isClickInvokedInCode)
			{
				return;
			}
			if (!eventArgs.Command.IsParent)
			{
				if (eventArgs.Command.Control != null && eventArgs.Command.IsControlEditable())
				{
					OnCommandClick(eventArgs);
					return;
				}
				TryClose(CommandCloseReason.CommandClicked);
			}
			OnCommandClick(eventArgs);
		}

		void IAccessibleCommandBarsElement.OnCommandCollapsed(CommandEventArgs eventArgs)
		{
			if (GetPopupControl() != null)
			{
				GetPopupControl().CloseMenuFor(eventArgs.Command);
			}
		}

		void IFormatStyleOwner.PropertyChange()
		{
			InvalidateMenu();
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

		protected virtual void OnPopup(EventArgs e)
		{
			if (base.Events[_popupEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnClosed(EventArgs e)
		{
			if (base.Events[_closedEvent] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnClosing(CommandClosingEventArgs e)
		{
			if (base.Events[_closingEvent] is CommandClosingEventHandler commandClosingEventHandler)
			{
				commandClosingEventHandler(this, e);
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
	}
}
