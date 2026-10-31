using System;
using System.Collections;
using System.Reflection;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.Docking.Internal
{
	[DefaultMember("Item")]
	internal class DockLayoutStateCollection : IEnumerable, IMordexXmlLayoutsSupport
	{
		private DockLayoutState _parentState;

		private ArrayList _children;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Item";

		internal DockLayoutStateCollection(DockLayoutState owner)
		{
			_parentState = owner;
			_children = new ArrayList();
		}

		internal void ReplaceLayoutState(DockLayoutState oldState, DockLayoutState newState)
		{
			int index = _children.IndexOf(oldState);
			if (index >= 0)
			{
				RemoveLayoutState(oldState);
				if (!_children.Contains(newState))
				{
					_children.Insert(index, newState);
					_parentState.AdoptChild(newState);
				}
			}
		}

		internal void AddLayoutState(DockLayoutState item)
		{
			if (!_children.Contains(item))
			{
				if (!_parentState.PrepareChildForAdoption(item))
				{
					return;
				}
				_children.Add(item);
			}
			_parentState.AdoptChild(item);
		}

		internal void RemoveLayoutState(DockLayoutState item)
		{
			if (_parentState.IsParentOf(item))
			{
				_children.Remove(item);
				_parentState.DetachChild(item);
			}
		}

		internal int GetCount()
		{
			return _children.Count;
		}

		internal DockLayoutState FindLayoutState(Guid guid)
		{
			foreach (DockLayoutState item in _children)
			{
				DockLayoutState dockLayoutState = item.FindById(guid);
				if (dockLayoutState != null)
				{
					return dockLayoutState;
				}
			}
			return null;
		}

		internal DockLayoutState GetAt(int index)
		{
			return (DockLayoutState)_children[index];
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _children.GetEnumerator();
		}

		internal int GetCurrentIndex(Guid guid)
		{
			int currentIndex = 0;
			for (int i = 0; i < _children.Count; i++)
			{
				DockLayoutState dockLayoutState = GetAt(i);
				if (dockLayoutState.GetIsCurrent())
				{
					if (dockLayoutState.GetId() == guid)
					{
						return currentIndex;
					}
					currentIndex++;
				}
			}
			return -1;
		}

		internal void MoveToCurrentIndex(Guid guid, int targetIndex)
		{
			int currentIndex = GetCurrentIndex(guid);
			if (currentIndex == -1 || currentIndex == targetIndex)
			{
				return;
			}
			DockLayoutState dockLayoutState = FindLayoutState(guid);
			_children.Remove(dockLayoutState);
			if (targetIndex == 0)
			{
				_children.Insert(0, dockLayoutState);
				return;
			}
			int currentCount = 0;
			for (int i = 0; i < _children.Count; i++)
			{
				DockLayoutState childState = GetAt(i);
				if (childState.GetIsCurrent())
				{
					if (targetIndex == currentCount)
					{
						_children.Insert(i, dockLayoutState);
						return;
					}
					currentCount++;
				}
			}
			_children.Add(dockLayoutState);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			int itemIndex = 0;
			for (PropertyValue propertyValue = reader["Item" + itemIndex]; propertyValue != null; propertyValue = reader["Item" + itemIndex])
			{
				DockLayoutState dockLayoutState = new DockLayoutState(Guid.Empty);
				dockLayoutState._parentState = _parentState;
				_children.Add(dockLayoutState);
				itemIndex++;
				propertyValue.DeserializeObject(dockLayoutState);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			for (int i = 0; i < _children.Count; i++)
			{
				info.AddValue("Item" + i, _children[i]);
			}
		}
	}
}
