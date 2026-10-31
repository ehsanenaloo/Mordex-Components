using System.Drawing;

namespace Mordex.Windows.UI.Docking.Internal
{
	public class PanelDropInfo
	{
		internal PanelDockState _panelDockState;

		internal DockPanelBase _targetPanel;

		internal Rectangle _dropBounds;

		internal PanelDockStyle _panelDockStyle;

		internal PanelGroupStyle _groupStyle;

		internal int _insertIndex;

		internal bool _createsGroup;

		internal Rectangle _tabHeaderBounds;

		internal bool _isAllowed;

		internal PanelDropInfo()
		{
			_tabHeaderBounds = Rectangle.Empty;
			_panelDockState = PanelDockState.Docked;
			_isAllowed = true;
		}
	}
}
