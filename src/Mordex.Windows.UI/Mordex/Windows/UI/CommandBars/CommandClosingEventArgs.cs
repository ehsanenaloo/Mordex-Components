using System;

namespace Mordex.Windows.UI.CommandBars
{
	public class CommandClosingEventArgs : EventArgs
	{
		private MxCommand _command;

		private bool _cancel;

		private CommandCloseReason _closeReason;

		public MxCommand Command => _command;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		public CommandCloseReason CloseReason => _closeReason;

		public CommandClosingEventArgs(MxCommand command, CommandCloseReason closeReason)
		{
			_command = command;
			_closeReason = closeReason;
		}
	}
}
