using Mordex.Windows.UI.Docking.Internal;

namespace Mordex.Windows.UI.Docking
{
	internal interface IPanelContainer
	{
		MxDockManager PanelManager { get; }

		DockPanelCollection Panels { get; }

		bool OnPanelAdd(DockPanelBase panel, int index);

		void OnPanelAdded(DockPanelBase panel);

		bool OnPanelRemove(DockPanelBase panel);

		void OnPanelRemoved(DockPanelBase panel);

		bool OnPanelMove(DockPanelBase panel, int index);

		void OnPanelMoved(DockPanelBase panel);

		void InvalidateLayout();

		DockLayoutState GetDockLayoutState(PanelDockState dockState);
	}
}
