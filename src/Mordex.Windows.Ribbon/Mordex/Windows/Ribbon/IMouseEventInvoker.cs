using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal interface IMouseEventInvoker
	{
		void InvokeMouseEnter();

		void InvokeMouseLeave();

		void InvokeMouseMove(MouseEventArgs eventArgs);

		void InvokeMouseDown(MouseEventArgs eventArgs);

		void InvokeMouseUp(MouseEventArgs eventArgs);
	}
}
