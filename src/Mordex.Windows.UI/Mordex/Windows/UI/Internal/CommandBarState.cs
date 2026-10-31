using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Internal
{
	internal class CommandBarState
	{
		public MxCommandBar _commandBar;

		public int _rowIndex = -1;

		public CommandBarState(MxCommandBar commandBar, int rowIndex)
		{
			_commandBar = commandBar;
			_rowIndex = rowIndex;
		}
	}
}
