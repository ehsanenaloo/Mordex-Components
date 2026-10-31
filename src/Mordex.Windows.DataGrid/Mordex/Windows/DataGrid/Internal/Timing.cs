namespace Mordex.Windows.DataGrid.Internal
{
	/// <summary>Timer intervals in milliseconds and size limits shared by the grid and its editors.</summary>
	internal static class Timing
	{
		internal const int AutoRepeatInterval = 150;

		internal const int InitialAutoRepeatDelay = 300;

		internal const int ScrollBarInitialDelay = 400;

		internal const int ScrollStepInterval = 50;

		internal const int DeferredActionInterval = 20;

		internal const int PointerPollInterval = 10;

		internal const int FadeStepInterval = 70;

		internal const int IncrementalSearchTimeout = 5000;

		internal const int MaxDragImageSize = 10000;
	}
}
