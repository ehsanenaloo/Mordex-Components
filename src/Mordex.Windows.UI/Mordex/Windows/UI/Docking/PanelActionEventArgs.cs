using System;

namespace Mordex.Windows.UI.Docking
{
	public class PanelActionEventArgs : EventArgs
	{
		private readonly DockPanelBase _panel;

		public DockPanelBase Panel => _panel;

		internal PanelActionEventArgs(DockPanelBase panel)
		{
			_panel = panel;
		}
	}
}
