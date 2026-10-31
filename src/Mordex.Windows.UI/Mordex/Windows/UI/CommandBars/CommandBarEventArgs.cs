using System;

namespace Mordex.Windows.UI.CommandBars
{
	public class CommandBarEventArgs : EventArgs
	{
		private MxCommandBar _commandBar;

		public MxCommandBar CommandBar => _commandBar;

		internal CommandBarEventArgs(MxCommandBar commandBar)
		{
			_commandBar = commandBar;
		}
	}
}
