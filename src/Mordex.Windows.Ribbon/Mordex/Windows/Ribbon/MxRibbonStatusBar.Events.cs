using System.ComponentModel;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbonStatusBar
	{
		[Description("Occurs when the drop-down window of one of the commands is about to be closed.")]
		public event DropDownClosingEventHandler CommandDropDownClosing
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownClosingKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownClosingKey, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is closed.")]
		public event CommandEventHandler CommandDropDownClosed
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownClosedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownClosedKey, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is opening.")]
		public event CommandCancelEventHandler CommandDropDownOpening
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownOpeningKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownOpeningKey, value);
			}
		}

		[Description("Occurs when the drop-down window of one of the commands is opened.")]
		public event CommandEventHandler CommandDropDownOpened
		{
			add
			{
				base.Events.AddHandler(s_commandDropDownOpenedKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandDropDownOpenedKey, value);
			}
		}

		[Description("Occurs when the user clicks on a Command.")]
		public event CommandEventHandler CommandClick
		{
			add
			{
				base.Events.AddHandler(s_commandClickKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_commandClickKey, value);
			}
		}
	}
}
