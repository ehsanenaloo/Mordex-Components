using System;
using System.ComponentModel;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonContextMenu
	{
		[Description("Occurs when the drop-down window of one of the commands is about to be closed.")]
		public event DropDownClosingEventHandler CommandDropDownClosing
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownClosingEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownClosingEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is closed.")]
		public event CommandEventHandler CommandDropDownClosed
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownClosedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownClosedEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is opening.")]
		public event CommandCancelEventHandler CommandDropDownOpening
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownOpeningEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownOpeningEvent, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is opened.")]
		public event CommandEventHandler CommandDropDownOpened
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownOpenedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownOpenedEvent, value);
			}
		}

		[Description("Occurs when the user clicks on a Command.")]
		public event CommandEventHandler CommandClick
		{
			add
			{
				base.Events.AddHandler(s_commandClickEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandClickEvent, value);
			}
		}

		[Description("Occurs before the context menu is closed.")]
		public event EventHandler Closed
		{
			add
			{
				base.Events.AddHandler(s_closedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_closedEvent, value);
			}
		}

		[Description("Occurs when the context menu is about to be closed.")]
		public event DropDownClosingEventHandler Closing
		{
			add
			{
				base.Events.AddHandler(s_closingEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_closingEvent, value);
			}
		}

		[Description("Occurs before the context menu is displayed.")]
		public event EventHandler Opened
		{
			add
			{
				base.Events.AddHandler(s_openedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_openedEvent, value);
			}
		}

		[Description("Occurs when the context menu is about to be displayed.")]
		public event CancelEventHandler Opening
		{
			add
			{
				base.Events.AddHandler(s_openingEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_openingEvent, value);
			}
		}
	}
}
