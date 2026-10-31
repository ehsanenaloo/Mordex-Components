using System;

namespace Mordex.Windows.Ribbon
{
	public class CommandEventArgs : EventArgs
	{
		private CommandBase _command;

		public CommandBase Command => _command;

		public CommandEventArgs(CommandBase command)
		{
			_command = command;
		}
	}
}
