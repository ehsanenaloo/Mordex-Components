using System;

namespace Mordex.Windows.UI.CommandBars
{
	public class CancelCommandBarEventArgs : EventArgs
	{
		private MxCommandBar _commandBar;

		private bool _cancel;

		public MxCommandBar CommandBar => _commandBar;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		internal CancelCommandBarEventArgs(MxCommandBar commandBar)
		{
			_commandBar = commandBar;
		}
	}
}
