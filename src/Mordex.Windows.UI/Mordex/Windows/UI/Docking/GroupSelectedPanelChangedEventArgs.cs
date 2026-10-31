using System;

namespace Mordex.Windows.UI.Docking
{
	public class GroupSelectedPanelChangedEventArgs : EventArgs
	{
		private readonly MxDockPanelGroup _group;

		private readonly DockPanelBase _selectedPanel;

		public MxDockPanelGroup PanelGroup => _group;

		public DockPanelBase SelectedPanel => _selectedPanel;

		internal GroupSelectedPanelChangedEventArgs(MxDockPanelGroup group, DockPanelBase selectedPanel)
		{
			_selectedPanel = selectedPanel;
			_group = group;
		}
	}
}
