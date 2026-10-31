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

		public Brush CreateItemBrush(bool isEnabled, bool isChecked, bool isHot)
		{
			if (!isEnabled && (isHot || isChecked))
			{
				return new SolidBrush(_menuColor);
			}
			Brush result = null;
			if (isChecked)
			{
				result = ((!isHot) ? new SolidBrush(_itemCheckedColor) : new SolidBrush(_itemPressedColor));
			}
			else if (isHot)
			{
				result = new SolidBrush(_itemHotColor);
			}
			return result;
		}

		public void DrawMenuItemFrame(Graphics graphics, Rectangle bounds, CommandLayoutState itemState)
		{
			bounds.Inflate(1, 1);
			Pen pen = null;
			if (((FormatStyleLayoutState)itemState).IsEnabled())
			{
				Brush brush = CreateItemBrush(((FormatStyleLayoutState)itemState).IsEnabled(), true, ((FormatStyleLayoutState)itemState).IsHot());
				if (brush != null)
				{
					graphics.FillRectangle(brush, bounds);
					brush.Dispose();
					brush = null;
				}
				pen = GdiCache.Pen(_menuItemBorderColor);
			}
			else
			{
				pen = GdiCache.Pen(RenderingHelper.BlendColors(SystemColors.ControlDark, SystemColors.Window, 0.9f));
			}
			RenderingHelper.DrawRectangleOutline(graphics, pen, bounds);
			pen = null;
		}
	}
}
