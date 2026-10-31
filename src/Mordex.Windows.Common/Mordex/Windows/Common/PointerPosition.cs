using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	// The pointer position used by controls that track a mouse gesture themselves.
	// A designer host that replays input for a control sets Override so the control sees the replayed point.
	public static class PointerPosition
	{
		[System.ThreadStatic]
		private static Point? overridePosition;

		public static Point Current => overridePosition ?? Cursor.Position;

		public static void SetOverride(Point screenPoint)
		{
			overridePosition = screenPoint;
		}

		public static void ClearOverride()
		{
			overridePosition = null;
		}
	}
}
