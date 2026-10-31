namespace Mordex.Windows.UI.Docking.Internal
{
	internal interface IDockElement
	{
		bool AllowCancelDrag { get; }

		void EraseSizingBar();

		void CancelSizing();
	}
}
