using System;
using System.Collections;
using System.Drawing;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class DockLayoutState : IMordexXmlLayoutsSupport, IPanelInfo
	{
		internal PanelDockState _panelDockState;

		private bool _isCurrent;

		private Guid _id;

		private bool _isGroup;

		private bool _isStatic;

		private PanelDockStyle _dockStyle;

		private Point _location;

		private Size _panelSize;

		private int _tileSize;

		internal DockLayoutState _parentState;

		private DockLayoutStateCollection _children;

		private PanelGroupStyle _groupStyle;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		Guid IPanelInfo.PanelId => GetId();

		int IPanelInfo.TileSize => GetTileSize();

		Size IPanelInfo.Size => GetPanelSize();

		bool IPanelInfo.IsStaticGroup => GetIsStatic();

		IPanelInfo IPanelInfo.Parent => GetParentState();

		bool IPanelInfo.IsGroup => GetIsGroup();

		IEnumerable IPanelInfo.Children => GetChildren();

		Point IPanelInfo.FloatingLocation => GetLocation();

		PanelGroupStyle IPanelInfo.GroupStyle => GetGroupStyle();

		PanelDockStyle IPanelInfo.DockStyle => GetDockStyle();

		bool IPanelInfo.CurrentState => GetIsCurrent();

		private PanelDockState GetPanelDockState()
		{
			if (_parentState != null)
			{
				return _parentState.GetPanelDockState();
			}
			return _panelDockState;
		}

		internal void CopyLayoutTo(DockLayoutState dockLayoutState)
		{
			dockLayoutState._dockStyle = _dockStyle;
			dockLayoutState._location = _location;
			dockLayoutState._groupStyle = _groupStyle;
			dockLayoutState._isGroup = _isGroup;
			dockLayoutState._panelSize = _panelSize;
			dockLayoutState._tileSize = _tileSize;
		}

		internal DockLayoutState(Guid id)
		{
			ResetDefaults();
			_id = id;
		}

		internal DockLayoutState(DockPanelBase panel)
		{
			ResetDefaults();
			if (panel != null)
			{
				_id = panel.Id;
				if (panel is MxDockPanelGroup)
				{
					_isGroup = true;
					_isStatic = ((MxDockPanelGroup)panel).StaticGroup;
				}
			}
		}

		private void ResetDefaults()
		{
			_id = Guid.Empty;
			_isGroup = false;
			_dockStyle = PanelDockStyle.Left;
			_location = new Point(-1, -1);
			_panelSize = new Size(-1, -1);
			_tileSize = -1;
			_children = new DockLayoutStateCollection(this);
			_groupStyle = PanelGroupStyle.HorizontalTiles;
		}

		internal int GetTileSize()
		{
			return _tileSize;
		}

		internal void SetTileSize(int tileSize)
		{
			_tileSize = tileSize;
		}

		internal bool GetIsStatic()
		{
			return _isStatic;
		}

		internal void SetIsStatic(bool isStatic)
		{
			_isStatic = isStatic;
		}

		internal Guid GetId()
		{
			return _id;
		}

		internal void SetId(Guid id)
		{
			_id = id;
		}

		internal Size GetPanelSize()
		{
			return _panelSize;
		}

		internal void SetPanelSize(Size size)
		{
			_panelSize = size;
		}

		internal DockLayoutState GetTopLevelState()
		{
			DockLayoutState dockLayoutState = this;
			while (dockLayoutState.GetParentState() != null && dockLayoutState.GetParentState()._id != Guid.Empty)
			{
				dockLayoutState = dockLayoutState.GetParentState();
			}
			return dockLayoutState;
		}

		internal DockLayoutState GetAncestorUnder(DockLayoutState dockLayoutState)
		{
			DockLayoutState ancestor = this;
			while (ancestor.GetParentState() != null && ancestor.GetParentState() != dockLayoutState)
			{
				ancestor = ancestor.GetParentState();
			}
			return ancestor;
		}

		internal void CollapseChildren()
		{
			for (int index = _children.GetCount() - 1; index >= 0; index--)
			{
				_children.GetAt(index).CollapseGroup();
			}
		}

		internal bool GetIsCurrent()
		{
			return _isCurrent;
		}

		internal void UpdateIsCurrent(bool isCurrent)
		{
			if (_isCurrent != isCurrent)
			{
				_isCurrent = isCurrent;
				if (_parentState != null && _parentState._id != Guid.Empty)
				{
					_parentState.RefreshIsCurrent();
				}
			}
		}

		internal void RefreshIsCurrent()
		{
			if (_children.GetCount() <= 0)
			{
				return;
			}
			foreach (DockLayoutState item in (IEnumerable)_children)
			{
				if (item.GetIsCurrent())
				{
					UpdateIsCurrent(true);
					return;
				}
			}
			UpdateIsCurrent(false);
		}

		internal void CollapseGroup()
		{
			if (_isGroup && !_isStatic)
			{
				for (int index = _children.GetCount() - 1; index >= 0; index--)
				{
					_children.GetAt(index).CollapseGroup();
				}
				if (_children.GetCount() == 0)
				{
					Reparent(null);
				}
				else if (_children.GetCount() == 1)
				{
					_children.GetAt(0)._dockStyle = _dockStyle;
					_children.GetAt(0)._location = _location;
					_children.GetAt(0)._panelSize = _panelSize;
					_children.GetAt(0)._tileSize = _tileSize;
					GetParentState()._children.ReplaceLayoutState(this, _children.GetAt(0));
				}
			}
		}

		internal bool GetIsGroup()
		{
			return _isGroup;
		}

		internal void SetIsGroup(bool isGroup)
		{
			_isGroup = isGroup;
		}

		internal PanelDockStyle GetDockStyle()
		{
			return _dockStyle;
		}

		internal void SetDockStyle(PanelDockStyle dockStyle)
		{
			_dockStyle = dockStyle;
		}

		internal Point GetLocation()
		{
			return _location;
		}

		internal void SetLocation(Point location)
		{
			_location = location;
		}

		internal PanelGroupStyle GetGroupStyle()
		{
			return _groupStyle;
		}

		internal void SetGroupStyle(PanelGroupStyle groupStyle)
		{
			_groupStyle = groupStyle;
		}

		internal DockLayoutState GetParentState()
		{
			return _parentState;
		}

		internal void Reparent(DockLayoutState dockLayoutState)
		{
			if (dockLayoutState != _parentState)
			{
				if (_parentState != null)
				{
					_parentState._children.RemoveLayoutState(this);
				}
				dockLayoutState?._children.AddLayoutState(this);
			}
		}

		internal DockLayoutStateCollection GetChildren()
		{
			return _children;
		}

		internal bool IsParentOf(DockLayoutState dockLayoutState)
		{
			if (dockLayoutState._parentState == this)
			{
				return true;
			}
			return false;
		}

		internal bool PrepareChildForAdoption(DockLayoutState dockLayoutState)
		{
			if (dockLayoutState._parentState == this)
			{
				return false;
			}
			if (dockLayoutState.GetParentState() != null)
			{
				dockLayoutState.Reparent(null);
			}
			return dockLayoutState.GetParentState() == null;
		}

		internal void AdoptChild(DockLayoutState dockLayoutState)
		{
			dockLayoutState._parentState = this;
		}

		internal void DetachChild(DockLayoutState dockLayoutState)
		{
			dockLayoutState._parentState = null;
			dockLayoutState._tileSize = -1;
		}

		internal DockLayoutState FindById(Guid guid)
		{
			if (guid == _id)
			{
				return this;
			}
			return _children.FindLayoutState(guid);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			_id = (Guid)LayoutReader.GetPropertyValue(reader, "Id", typeof(Guid), Guid.Empty);
			_dockStyle = (PanelDockStyle)LayoutReader.GetPropertyValue(reader, "DockStyle", typeof(PanelDockStyle), PanelDockStyle.Left);
			_location = (Point)LayoutReader.GetPropertyValue(reader, "Location", typeof(Point), new Point(-1, -1));
			_groupStyle = (PanelGroupStyle)LayoutReader.GetPropertyValue(reader, "GroupStyle", typeof(PanelGroupStyle), PanelGroupStyle.HorizontalTiles);
			_isGroup = (bool)LayoutReader.GetPropertyValue(reader, "IsGroup", typeof(bool), false);
			_panelSize = (Size)LayoutReader.GetPropertyValue(reader, "Size", typeof(Size), new Size(-1, -1));
			_tileSize = (int)LayoutReader.GetPropertyValue(reader, "TileSize", typeof(int), -1);
			_children = new DockLayoutStateCollection(this);
			_isCurrent = (bool)LayoutReader.GetPropertyValue(reader, "IsCurrent", typeof(bool), true);
			_isStatic = (bool)LayoutReader.GetPropertyValue(reader, "IsStatic", typeof(bool), false);
			reader["Panels"]?.DeserializeObject(_children);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			info.AddValue("Id", _id);
			if (_isGroup)
			{
				info.AddValue("IsGroup", _isGroup);
				if (_groupStyle != PanelGroupStyle.HorizontalTiles)
				{
					info.AddValue("GroupStyle", _groupStyle);
				}
			}
			if (_isStatic)
			{
				info.AddValue("IsStatic", _isStatic);
			}
			if (GetTopLevelState() == this)
			{
				if (GetPanelDockState() == PanelDockState.Docked)
				{
					if (_dockStyle != PanelDockStyle.Left)
					{
						info.AddValue("DockStyle", _dockStyle);
					}
				}
				else if (_location != new Point(-1, -1))
				{
					info.AddValue("Location", _location);
				}
				info.AddValue("Size", _panelSize);
			}
			else if (_tileSize != -1)
			{
				info.AddValue("TileSize", _tileSize);
			}
			if (_children.GetCount() > 0)
			{
				info.AddValue("Panels", _children);
			}
			if (!_isCurrent)
			{
				info.AddValue("IsCurrent", _isCurrent);
			}
		}
	}
}
