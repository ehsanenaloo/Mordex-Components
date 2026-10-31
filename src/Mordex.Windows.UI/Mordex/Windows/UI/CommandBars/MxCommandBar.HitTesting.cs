using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		public MxCommand GetCommandAt(int x, int y)
		{
			return HitTestItem(x, y)?.GetCommand();
		}

		public MxCommand GetCommandAt(Point pt)
		{
			return GetCommandAt(pt.X, pt.Y);
		}

		public MxCommand GetCommandAt()
		{
			Point point = PointToClient(Cursor.Position);
			return GetCommandAt(point.X, point.Y);
		}

		internal CommandLayoutState HitTestItem(int x, int y)
		{
			if (_layoutItems == null)
			{
				return null;
			}
			foreach (CommandLayoutState item in _layoutItems)
			{
				if (item._bounds.Contains(x, y) && ((FormatStyleLayoutState)item).IsVisible())
				{
					return item;
				}
			}
			if (_childMenu != null)
			{
				CommandControl childMenu = _childMenu;
				while (childMenu != null && !childMenu.IsDisposed)
				{
					Point p = PointToScreen(new Point(x, y));
					Point point = childMenu.PointToClient(p);
					CommandLayoutState uiCommandLayoutState = childMenu.HitTestItem(point.X, point.Y);
					if (uiCommandLayoutState != null)
					{
						return uiCommandLayoutState;
					}
					childMenu = childMenu.ChildMenu;
				}
			}
			return null;
		}

		public Rectangle GetCommandBounds(MxCommand command)
		{
			if (_isLayoutDirty)
			{
				RecalculateLayout();
			}
			if (_layoutItems != null)
			{
				foreach (CommandLayoutState item in _layoutItems)
				{
					if (item.GetCommand() == command)
					{
						return item._bounds;
					}
				}
			}
			if (_childMenu != null)
			{
				return _childMenu.GetCommandScreenBounds(command);
			}
			return Rectangle.Empty;
		}

		internal bool IsOutsideDragThreshold(int x, int y)
		{
			Size dragSize = SystemInformation.DragSize;
			if (!new Rectangle(_gripMouseDownPoint.X - dragSize.Width, _gripMouseDownPoint.Y - dragSize.Height, dragSize.Width * 2, dragSize.Height * 2).Contains(x, y))
			{
				return true;
			}
			return false;
		}
	}
}
