using System;
using System.Drawing;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class DockRenderer
	{
		public static void ToggleDragFrame(Rectangle rectangle, int borderWidth)
		{
			IntPtr region = RenderingHelper.CreateFrameRegion(rectangle, borderWidth);
			try
			{
				RenderingHelper.InvertWithDragHatch(region);
			}
			finally
			{
				NativeMethods.DeleteObject(region);
			}
		}
	}
}
