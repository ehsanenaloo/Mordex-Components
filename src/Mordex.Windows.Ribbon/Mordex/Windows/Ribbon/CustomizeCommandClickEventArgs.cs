using System;

namespace Mordex.Windows.Ribbon
{
	public class CustomizeCommandClickEventArgs : EventArgs
	{
		private CustomizeCommandButton _button;

		private bool _doDefaultAction = true;

		public CustomizeCommandButton CustomizeCommandButton => _button;

		public bool DoDefaultAction
		{
			get
			{
				return _doDefaultAction;
			}
			set
			{
				_doDefaultAction = value;
			}
		}

		public CustomizeCommandClickEventArgs(CustomizeCommandButton button)
		{
			_button = button;
		}
	}
}
