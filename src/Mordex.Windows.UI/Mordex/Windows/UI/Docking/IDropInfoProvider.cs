using System.Drawing;
using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal interface IDropInfoProvider
	{
		PanelDropInfo GetDropInfo(DockPanelBase panel, Point screenPoint);
	}
}
