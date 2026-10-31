namespace Mordex.Windows.DataGrid.Internal
{
	/// <summary>Win32 message and raster operation codes used by the grid.</summary>
	internal static class WindowMessages
	{
		internal const int CancelMode = 0x001F;

		internal const int NonClientHitTest = 0x0084;

		internal const int NonClientLeftButtonDown = 0x00A1;

		internal const int NonClientRightButtonDown = 0x00A4;

		internal const int NonClientMiddleButtonDown = 0x00A7;

		internal const int EditSetSelection = 0x00B1;

		internal const int LeftButtonDown = 0x0201;

		internal const int RightButtonDown = 0x0204;

		internal const int MiddleButtonDown = 0x0207;

		internal const int MouseWheel = 0x020A;

		internal const int Cut = 0x0300;

		internal const int Copy = 0x0301;

		internal const int Paste = 0x0302;

		internal const int SourceCopy = 0x00CC0020;
	}
}
