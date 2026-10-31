using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal interface ICommandDesignTarget
	{
		ImageList DesignerImageList { get; }

		Control OwnerControl { get; }

		void AddNewCommand(CommandBase command, bool editItem);

		bool CanAddCommand(CommandType commandType);

		Point PointToScreen(Point clientPoint);
	}
}
