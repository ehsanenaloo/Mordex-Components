namespace Mordex.Windows.UI.CommandBars
{
	internal interface ICommandBarOwner
	{
		bool IsInitializing { get; }

		void OnBarAdded(MxCommandBar toolBar);

		void OnBarRemoved(MxCommandBar toolBar);

		MxCommandManager GetCommandManagerOwner();
	}
}
