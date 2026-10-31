using System;

namespace Mordex.Windows.UI.Docking
{
	public class PanelActionCancelEventArgs : EventArgs
	{
		private readonly DockPanelBase _panel;

		private bool _cancel;

		public DockPanelBase Panel => _panel;

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

		internal PanelActionCancelEventArgs(DockPanelBase panel, bool cancel)
		{
			_panel = panel;
			_cancel = cancel;
		}
	}
}
