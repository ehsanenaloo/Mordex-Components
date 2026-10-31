using System;

namespace Mordex.Windows.Ribbon
{
	public class DropDownClosingEventArgs : EventArgs
	{
		private DropDownCommand _command;

		private bool _cancel;

		private CommandCloseReason _closeReason;

		public DropDownCommand Command => _command;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				if (value && CloseReason == CommandCloseReason.AppFocusChange)
				{
					value = false;
				}
				_cancel = value;
			}
		}

		public CommandCloseReason CloseReason => _closeReason;

		public DropDownClosingEventArgs(DropDownCommand command, CommandCloseReason closeReason)
		{
			_command = command;
			_closeReason = closeReason;
		}
	}
}
