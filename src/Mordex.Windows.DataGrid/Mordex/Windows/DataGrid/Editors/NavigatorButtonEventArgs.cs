using System;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class NavigatorButtonEventArgs : EventArgs
	{
		private readonly NavigatorButton _navigatorButton;

		internal NavigatorButtonEventArgs(int buttonIndex)
		{
			_navigatorButton = (NavigatorButton)buttonIndex;
		}

		public NavigatorButton GetNavigatorButton()
		{
			return _navigatorButton;
		}
	}
}
