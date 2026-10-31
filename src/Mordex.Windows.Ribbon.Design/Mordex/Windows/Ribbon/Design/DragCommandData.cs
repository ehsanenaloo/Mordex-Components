namespace Mordex.Windows.Ribbon.Design
{
	internal class DragCommandData
	{
		private CommandBase _command;

		private RibbonTab _tab;

		public CommandBase Command => _command;

		internal DragCommandData(CommandBase command)
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
			this._command = command;
		}

		internal DragCommandData(RibbonTab tab)
		{
			_tab = tab;
		}
	}
}
