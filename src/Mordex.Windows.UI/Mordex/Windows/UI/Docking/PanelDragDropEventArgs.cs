using System;
using System.Drawing;

namespace Mordex.Windows.UI.Docking
{
	public class PanelDragDropEventArgs : EventArgs
	{
		private bool _canDrop;

		private PanelDockState _dockState;

		private PanelDockStyle _dockStyle;

		private DockPanelBase _parent;

		private DockPanelBase _panel;

		private PanelGroupStyle _groupStyle;

		private PanelDropAction _dropAction;

		private Size _panelSize;

		public bool CanDrop
		{
			get
			{
				return _canDrop;
			}
			set
			{
				_canDrop = value;
			}
		}

		public PanelGroupStyle GroupStyle
		{
			get
			{
				return _groupStyle;
			}
			set
			{
				if (_dropAction != PanelDropAction.CreateGroup)
				{
					throw new InvalidOperationException("GroupStyle can be changed only when DropAction is CreateGroup.");
				}
				_groupStyle = value;
			}
		}

		public PanelDockState DockState => _dockState;

		public PanelDockStyle DockStyle => _dockStyle;

		public DockPanelBase ParentPanel => _parent;

		public Size Size
		{
			get
			{
				return _panelSize;
			}
			set
			{
				_panelSize = value;
			}
		}

		public DockPanelBase Panel => _panel;

		public PanelDropAction DropAction => _dropAction;

		internal PanelDragDropEventArgs(DockPanelBase panel, PanelDockState dockState, PanelDockStyle dockStyle, DockPanelBase parent, PanelGroupStyle groupStyle, PanelDropAction dropAction, Size size)
		{
			_canDrop = true;
			_dockState = dockState;
			_dockStyle = dockStyle;
			_parent = parent;
			_panel = panel;
			_groupStyle = groupStyle;
			_dropAction = dropAction;
			_panelSize = size;
		}
	}
}
