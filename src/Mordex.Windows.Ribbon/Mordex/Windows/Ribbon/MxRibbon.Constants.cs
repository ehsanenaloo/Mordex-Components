
namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		// SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_FRAMECHANGED
		private const uint SwpFrameChanged = 51u;

		// RDW_INVALIDATE | RDW_FRAME
		private const int RdwInvalidateFrame = 1025;

		private const int VkMenu = 18;

		private const int VirtualKeyF10 = 121;

		private const int KeysAltModifier = 262144;

		private const int MinCaptionHeight = 26;

		private const int QuickAccessBarHeight = 26;

		private const int SysCharContextCodeBit = 0x400000;

		private const int WmNcHitTest = 132;

		private const long HtTransparent = -1L;

		private const int WmSetRedraw = 11;

		private const int WmSysCommand = 274;

		private const long ScRestore = 61728L;

		private const long ScMaximize = 61488L;

		private const int WheelDelta = 120;

		private const int WmActivate = 6;

		private const int WmActivateApp = 28;

		private const int WmNcLButtonDown = 161;

		private const int WmNcRButtonDown = 164;

		private const int WmNcMButtonDown = 167;

		private const int WmLButtonDown = 513;

		private const int WmRButtonDown = 516;

		private const int WmMButtonDown = 519;

		private const int WmXButtonDown = 523;

		private const int WmMouseWheel = 522;

		private const int WmKeyDown = 256;

		private const int WmChar = 258;

		private const int WmSysKeyDown = 260;

		private const int WmSysKeyUp = 261;

		private const int WmSysChar = 262;
	}
}
