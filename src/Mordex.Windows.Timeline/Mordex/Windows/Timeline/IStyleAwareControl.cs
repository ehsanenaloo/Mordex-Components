using System.Drawing;

namespace Mordex.Windows.Timeline
{
	internal interface IStyleAwareControl
	{
		Pen FlatBorderPen { get; }

		void OnControlStyleChanged();
	}
}
