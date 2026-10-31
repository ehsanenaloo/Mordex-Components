using System.Drawing;

namespace Mordex.Windows.Scheduler.EditControls
{
	internal interface IStyleAwareControl
	{
		Pen FlatBorderPen { get; }

		void OnControlStyleChanged();
	}
}
