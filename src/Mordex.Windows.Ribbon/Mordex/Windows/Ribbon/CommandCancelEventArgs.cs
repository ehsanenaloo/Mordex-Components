using System;

namespace Mordex.Windows.Ribbon
{
	public class CommandCancelEventArgs : EventArgs
	{
		private CommandBase _command;

		private bool _cancel;

		public CommandBase Command => _command;

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

		public CommandCancelEventArgs(CommandBase command)
		{
			_command = command;
		}
	}
}
