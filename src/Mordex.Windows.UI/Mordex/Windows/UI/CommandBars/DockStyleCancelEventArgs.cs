using System;

namespace Mordex.Windows.UI.CommandBars
{
	public class DockStyleCancelEventArgs : EventArgs
	{
		private BarsDockStyle _previousDockStyle;

		private BarsDockStyle _dockStyle;

		private bool _cancel;

		public BarsDockStyle PreviousDockStyle => _previousDockStyle;

		public BarsDockStyle DockStyle => _dockStyle;

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

		internal DockStyleCancelEventArgs(BarsDockStyle prevDockStyle, BarsDockStyle dockStyle)
		{
			_previousDockStyle = prevDockStyle;
			_dockStyle = dockStyle;
		}
	}
}
