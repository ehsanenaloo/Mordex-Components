using System;

namespace Mordex.Windows.UI.Docking
{
	public class GroupSelectedPanelChangingEventArgs : EventArgs
	{
		private readonly MxDockPanelGroup _group;

		private readonly DockPanelBase _selectedPanel;

		private bool _cancel;

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

		public MxDockPanelGroup PanelGroup => _group;

		public DockPanelBase SelectedPanel => _selectedPanel;

		internal GroupSelectedPanelChangingEventArgs(MxDockPanelGroup group, DockPanelBase selectedPanel)
		{
			_selectedPanel = selectedPanel;
			_group = group;
			_cancel = false;
		}
	}
}
