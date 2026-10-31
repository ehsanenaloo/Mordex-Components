using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Internal
{
	internal partial class VisualStylePainter
	{

		private void DrawClassicSizeGrip(Graphics graphics, Rectangle bounds)
		{
			Brush darkBrush = null;
			Brush lightBrush = null;
			Brush midBrush = null;
			darkBrush = GdiCache.Brush(Color.FromArgb(40, 50, 71));
			lightBrush = GdiCache.Brush(Color.FromArgb(249, 249, 251));
			midBrush = GdiCache.Brush(Color.FromArgb(97, 116, 142));
			Rectangle gripRow = bounds;
			gripRow.Width -= 4;
			Rectangle dotBounds;
			for (int i = 0; i < 3; i++)
			{
				dotBounds = new Rectangle(gripRow.Right, gripRow.Bottom - 4, 2, 2);
				graphics.FillRectangle(darkBrush, dotBounds);
				dotBounds.Offset(1, 1);
				graphics.FillRectangle(lightBrush, dotBounds);
				dotBounds.Size = new Size(1, 1);
				graphics.FillRectangle(midBrush, dotBounds);
				gripRow.Width -= 4;
			}
			gripRow.Width = bounds.Width - 4;
			gripRow.Height -= 4;
			for (int j = 0; j < 2; j++)
			{
				dotBounds = new Rectangle(gripRow.Right, gripRow.Bottom - 4, 2, 2);
				graphics.FillRectangle(darkBrush, dotBounds);
				dotBounds.Offset(1, 1);
				graphics.FillRectangle(lightBrush, dotBounds);
				dotBounds.Size = new Size(1, 1);
				graphics.FillRectangle(midBrush, dotBounds);
				gripRow.Width -= 4;
			}
			gripRow.Width = bounds.Width - 4;
			dotBounds = new Rectangle(gripRow.Right, gripRow.Bottom - 8, 2, 2);
			graphics.FillRectangle(darkBrush, dotBounds);
			dotBounds.Offset(1, 1);
			graphics.FillRectangle(lightBrush, dotBounds);
			dotBounds.Size = new Size(1, 1);
			graphics.FillRectangle(midBrush, dotBounds);
			darkBrush = null;
			lightBrush = null;
			midBrush = null;
		}

		public void DrawStatusBarGripper(Graphics dc, Rectangle bounds)
		{
			if (!_isStandardStyle)
			{
				DrawClassicSizeGrip(dc, bounds);
				return;
			}
			VisualStyleElement gripperElement = VisualStyleElement.Status.Gripper.Normal;
			if (TrySelectElement(gripperElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}

		public void DrawStatusBarPane(Graphics dc, Rectangle bounds)
		{
			VisualStyleElement paneElement = VisualStyleElement.Status.Pane.Normal;
			if (TrySelectElement(paneElement))
			{
				_visualStyleRenderer.DrawBackground(dc, bounds);
			}
		}
	}
}
