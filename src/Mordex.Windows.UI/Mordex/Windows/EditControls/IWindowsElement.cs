using System.Drawing;

namespace Mordex.Windows.UI.Controls
{
	internal interface IWindowsElement
	{
		Color FlatBorderColor { get; }

		void OnControlStyleChanged();
	}
}
