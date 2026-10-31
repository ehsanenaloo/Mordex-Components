using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class GridMessageFilter : IMessageFilter
	{
		private GridWindowMessageHandler _tooltipHandler;

		internal GridMessageFilter(GridWindowMessageHandler tp)
		{
			_tooltipHandler = tp;
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case WindowMessages.NonClientLeftButtonDown:
			case WindowMessages.NonClientRightButtonDown:
			case WindowMessages.NonClientMiddleButtonDown:
			case WindowMessages.LeftButtonDown:
			case WindowMessages.RightButtonDown:
			case WindowMessages.MiddleButtonDown:
				_tooltipHandler.DisposeResources();
				break;
			}
			return false;
		}
	}
}
