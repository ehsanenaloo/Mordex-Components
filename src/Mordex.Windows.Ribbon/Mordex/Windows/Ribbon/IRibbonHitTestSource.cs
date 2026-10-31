using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal interface IRibbonHitTestSource
	{
		Control HostControl { get; }

		RibbonColorScheme ColorScheme { get; }

		Point PointToClient(Point screenPoint);

		CommandBase GetCommandAt(Point pt);

		Rectangle GetCommandBounds(CommandBase command);

		Rectangle GetBackstageBarBounds();
	}
}
