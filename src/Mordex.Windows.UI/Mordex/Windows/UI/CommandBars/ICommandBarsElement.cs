using System.Windows.Forms;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	internal interface ICommandBarsElement
	{
		Control Control { get; }

		MxCommandCollection Commands { get; }

		MxCollection LayoutItems { get; }

		MxCommand SelectedCommand { get; set; }
	}
}
