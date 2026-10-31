using System;

namespace Mordex.Windows.UI.CommandBars
{
	public class CommandEventArgs : EventArgs
	{
		private MxCommand _command;

		public MxCommand Command => _command;

		public CommandEventArgs(MxCommand command)
		{
			_command = command;
		}
	}
}
