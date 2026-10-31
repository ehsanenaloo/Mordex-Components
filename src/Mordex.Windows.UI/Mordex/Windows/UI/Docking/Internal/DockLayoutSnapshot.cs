using System;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class DockLayoutSnapshot : IMordexXmlLayoutsSupport
	{
		private MxDockManager _manager;

		private DockLayoutState _dockedLayout;

		private DockLayoutState _floatingLayout;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		internal DockLayoutSnapshot(MxDockManager manager)
		{
			_manager = manager;
			_dockedLayout = CreateRootLayoutState(PanelDockState.Docked);
			_floatingLayout = CreateRootLayoutState(PanelDockState.Floating);
		}

		private static DockLayoutState CreateRootLayoutState(PanelDockState panelDockState)
		{
			DockLayoutState rootState = new DockLayoutState(Guid.Empty);
			rootState._panelDockState = panelDockState;
			return rootState;
		}

		internal DockLayoutState GetLayoutState(PanelDockState panelDockState)
		{
			if (panelDockState == PanelDockState.Docked)
			{
				return _dockedLayout;
			}
			return _floatingLayout;
		}

		internal DockLayoutState GetDockedLayout()
		{
			return _dockedLayout;
		}

		internal DockLayoutState GetFloatingLayout()
		{
			return _floatingLayout;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_dockedLayout = CreateRootLayoutState(PanelDockState.Docked);
			_floatingLayout = CreateRootLayoutState(PanelDockState.Floating);
			reader["DockedPanels"]?.DeserializeObject(_dockedLayout.GetChildren());
			reader["FloatingPanels"]?.DeserializeObject(_floatingLayout.GetChildren());
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_dockedLayout.GetChildren().GetCount() > 0)
			{
				info.AddValue("DockedPanels", _dockedLayout.GetChildren());
			}
			if (_floatingLayout.GetChildren().GetCount() > 0)
			{
				info.AddValue("FloatingPanels", _floatingLayout.GetChildren());
			}
		}
	}
}
