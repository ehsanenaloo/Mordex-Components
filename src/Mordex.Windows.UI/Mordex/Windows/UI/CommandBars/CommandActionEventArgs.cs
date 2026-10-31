using System;

namespace Mordex.Windows.UI.CommandBars
{
	internal class CommandActionEventArgs : EventArgs
	{
		private MxCommand _command;

		private MxCommand _parentCommand;

		internal CommandActionEventArgs(MxCommand command, MxCommand parentCommand)
		{
			_command = command;
			_parentCommand = parentCommand;
		}

		public MxCommand GetCommand()
		{
			return _command;
		}

		public MxCommand GetParentCommand()
		{
			return _parentCommand;
		}
	}
}
