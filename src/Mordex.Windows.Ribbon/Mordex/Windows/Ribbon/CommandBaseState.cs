
namespace Mordex.Windows.Ribbon
{
	internal class CommandBaseState
	{
		private CommandBaseRenderer _item;

		private CommandBase _command;

		private RibbonTab _tab;

		public CommandBaseState(CommandBaseRenderer item)
		{
			_item = item;
		}

		public CommandBaseState(CommandBase command)
		{
			_command = command;
		}

		public CommandBaseState(RibbonTab tab)
		{
			_tab = tab;
		}

		public CommandBaseRenderer GetRenderer()
		{
			return _item;
		}
	}
}
