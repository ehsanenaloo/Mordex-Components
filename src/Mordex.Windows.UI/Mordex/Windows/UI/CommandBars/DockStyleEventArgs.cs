using System;

namespace Mordex.Windows.UI.CommandBars
{
	public class DockStyleEventArgs : EventArgs
	{
		private BarsDockStyle _previousDockStyle;

		private BarsDockStyle _dockStyle;

		public BarsDockStyle PreviousDockStyle => _previousDockStyle;

		public BarsDockStyle DockStyle => _dockStyle;

		internal DockStyleEventArgs(BarsDockStyle prevDockStyle, BarsDockStyle dockStyle)
		{
			_previousDockStyle = prevDockStyle;
			_dockStyle = dockStyle;
		}
	}
}
