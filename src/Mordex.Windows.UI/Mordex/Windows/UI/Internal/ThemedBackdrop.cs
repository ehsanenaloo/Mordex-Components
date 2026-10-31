using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Internal
{
	internal static class ThemedBackdrop
	{
		// Paints what lies behind a control with rounded corners. Without visual styles the parent cannot paint into the control, so the nearest opaque parent color is used.
		internal static void Paint(Graphics graphics, Rectangle bounds, Control control)
		{
			if (Application.RenderWithVisualStyles)
			{
				ButtonRenderer.DrawParentBackground(graphics, bounds, control);
				return;
			}
			Color color = SystemColors.Control;
			for (Control parent = control.Parent; parent != null; parent = parent.Parent)
			{
				if (parent.BackColor.A == byte.MaxValue)
				{
					color = parent.BackColor;
					break;
				}
			}
			graphics.FillRectangle(Mordex.Windows.Common.GdiCache.Brush(color), bounds);
		}
	}
}
