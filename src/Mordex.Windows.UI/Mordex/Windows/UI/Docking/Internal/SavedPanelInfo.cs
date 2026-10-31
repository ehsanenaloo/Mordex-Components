using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Mordex.Windows.UI.Design;

namespace Mordex.Windows.UI.Docking.Internal
{
	// A copy of a dock layout with its groups folded the same way DockLayoutState.CollapseGroup folds them:
	// an empty group is left out and a group with one child is replaced by that child, which takes the group's
	// dock style, location, size and tile size. Static groups and everything under them are copied as they are.
	// Building it reads the layout and never changes it.
	internal sealed class SavedPanelInfo : IPanelInfo
	{
		private readonly List<SavedPanelInfo> _children = new List<SavedPanelInfo>();

		private readonly Guid _panelId;

		private readonly bool _isGroup;

		private readonly bool _isStatic;

		private readonly bool _isCurrent;

		private readonly PanelGroupStyle _groupStyle;

		private PanelDockStyle _dockStyle;

		private Point _location;

		private Size _size;

		private int _tileSize;

		private SavedPanelInfo _parent;

		private SavedPanelInfo(DockLayoutState state)
		{
			_panelId = state.GetId();
			_isGroup = state.GetIsGroup();
			_isStatic = state.GetIsStatic();
			_isCurrent = state.GetIsCurrent();
			_groupStyle = state.GetGroupStyle();
			_dockStyle = state.GetDockStyle();
			_location = state.GetLocation();
			_size = state.GetPanelSize();
			_tileSize = state.GetTileSize();
		}

		Guid IPanelInfo.PanelId => _panelId;

		int IPanelInfo.TileSize => _tileSize;

		Size IPanelInfo.Size => _size;

		IPanelInfo IPanelInfo.Parent => _parent;

		bool IPanelInfo.IsStaticGroup => _isStatic;

		bool IPanelInfo.IsGroup => _isGroup;

		IEnumerable IPanelInfo.Children => _children.AsReadOnly();

		PanelGroupStyle IPanelInfo.GroupStyle => _groupStyle;

		Point IPanelInfo.FloatingLocation => _location;

		PanelDockStyle IPanelInfo.DockStyle => _dockStyle;

		bool IPanelInfo.CurrentState => _isCurrent;

		// The root itself is never folded, as DockLayoutState.CollapseChildren only folds the children.
		internal static IEnumerable FoldChildren(DockLayoutState root)
		{
			SavedPanelInfo rootInfo = new SavedPanelInfo(root);
			foreach (DockLayoutState child in (IEnumerable)root.GetChildren())
			{
				rootInfo.Adopt(Fold(child));
			}
			return rootInfo._children.AsReadOnly();
		}

		private static SavedPanelInfo Fold(DockLayoutState state)
		{
			if (!state.GetIsGroup() || state.GetIsStatic())
			{
				return CopyTree(state);
			}
			SavedPanelInfo group = new SavedPanelInfo(state);
			foreach (DockLayoutState child in (IEnumerable)state.GetChildren())
			{
				group.Adopt(Fold(child));
			}
			if (group._children.Count == 0)
			{
				return null;
			}
			if (group._children.Count == 1)
			{
				SavedPanelInfo onlyChild = group._children[0];
				onlyChild._dockStyle = group._dockStyle;
				onlyChild._location = group._location;
				onlyChild._size = group._size;
				onlyChild._tileSize = group._tileSize;
				return onlyChild;
			}
			return group;
		}

		private static SavedPanelInfo CopyTree(DockLayoutState state)
		{
			SavedPanelInfo info = new SavedPanelInfo(state);
			foreach (DockLayoutState child in (IEnumerable)state.GetChildren())
			{
				info.Adopt(CopyTree(child));
			}
			return info;
		}

		private void Adopt(SavedPanelInfo child)
		{
			if (child != null)
			{
				child._parent = this;
				_children.Add(child);
			}
		}
	}
}
