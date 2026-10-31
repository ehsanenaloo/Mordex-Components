using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Common.Design
{
	// Designers read mouse positions from the message. Cursor.Position is the position on the machine,
	// which is not the point the user clicked when the designer runs in the Design Tools Server.
	public static class DesignMouse
	{
		private const int WM_MOUSEFIRST = 0x200;

		private const int WM_MOUSELAST = 0x20E;

		public static Point ClientPoint(Control control, Message m)
		{
			if (m.Msg >= WM_MOUSEFIRST && m.Msg <= WM_MOUSELAST && m.Msg != 0x20A && m.Msg != 0x20E)
			{
				long lParam = m.LParam.ToInt64();
				return new Point((short)(lParam & 0xFFFF), (short)((lParam >> 16) & 0xFFFF));
			}
			return control.PointToClient(Cursor.Position);
		}
	}
}
