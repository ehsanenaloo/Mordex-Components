namespace Mordex.Windows.Ribbon.Design
{
	// Values returned by DesignHelper.HitTest.
	internal static class DesignHitTest
	{
		internal const int None = 0;

		internal const int RibbonTab = 1;

		internal const int RibbonBackground = 2;

		internal const int Command = 3;

		internal const int BackstageMenuButton = 4;

		internal const int RibbonScrollButton = 5;

		internal const int TabScrollButton = 6;

		internal const int ControlBoxButton = 7;

		internal const int CustomizeButton = 8;

		internal const int StatusBarResizePanel = 9;

		internal const int MinimizeRibbonButton = 10;
	}

	// Window messages the designers react to.
	internal static class DesignMessages
	{
		internal const int MouseMove = 512;

		internal const int LeftButtonDown = 513;

		internal const int LeftButtonUp = 514;

		internal const int LeftButtonDoubleClick = 515;

		internal const int RightButtonDown = 516;

		internal const int RightButtonUp = 517;

		internal const int MouseLeave = 675;
	}
}
