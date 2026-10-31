using System;
using System.Collections;

namespace Mordex.Windows.UI.Docking
{
	public class DockPanelCollection : IList, ICollection, IEnumerable
	{
		private ArrayList _panels;

		private IPanelContainer _owner;

		private bool _assignsParent;

		public int Count => _panels.Count;

		public DockPanelBase this[int index] => (DockPanelBase)_panels[index];

		bool ICollection.IsSynchronized => _panels.IsSynchronized;

		object ICollection.SyncRoot => _panels.SyncRoot;

		bool IList.IsFixedSize => false;

		bool IList.IsReadOnly => false;

		object IList.this[int index]
		{
			get
			{
				return _panels[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		internal DockPanelCollection(IPanelContainer owner, bool setOwner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException();
			}
			_owner = owner;
			_panels = new ArrayList();
			_assignsParent = setOwner;
		}

		public int IndexOf(DockPanelBase panel)
		{
			return _panels.IndexOf(panel);
		}

		public bool Contains(DockPanelBase panel)
		{
			return IndexOf(panel) >= 0;
		}

		public void Add(DockPanelBase panel)
		{
			if (panel != null && !Contains(panel) && _owner.OnPanelAdd(panel, _panels.Count))
			{
				_panels.Add(panel);
				if (_assignsParent)
				{
					panel.SetParentContainer(_owner);
				}
				_owner.OnPanelAdded(panel);
			}
		}

		public void Insert(int index, DockPanelBase panel)
		{
			if (panel == null)
			{
				return;
			}
			if (index < 0 || index > _panels.Count)
			{
				index = _panels.Count;
			}
			if (!Contains(panel))
			{
				if (_owner.OnPanelAdd(panel, index))
				{
					_panels.Insert(index, panel);
					if (_assignsParent)
					{
						panel.SetParentContainer(_owner);
					}
					_owner.OnPanelAdded(panel);
				}
			}
			else
			{
				if (IndexOf(panel) < index)
				{
					index--;
				}
				SetPanelIndex(panel, index);
			}
		}

		internal void ReplacePanel(DockPanelBase panel, DockPanelBase newPanel)
		{
			int index = IndexOf(panel);
			if (index >= 0)
			{
				_panels[index] = newPanel;
				if (_assignsParent)
				{
					panel.SetParentContainer(null);
					newPanel.SetParentContainer(_owner);
				}
				_owner.InvalidateLayout();
			}
		}

		public void SetPanelIndex(DockPanelBase panel, int index)
		{
			int currentIndex = IndexOf(panel);
			if (currentIndex < 0)
			{
				throw new ArgumentException("'panel' is not part of the collection");
			}
			if (currentIndex != index && _owner.OnPanelMove(panel, index))
			{
				_panels.Remove(panel);
				_panels.Insert(index, panel);
				_owner.OnPanelMoved(panel);
			}
		}

		public void Remove(DockPanelBase panel)
		{
			if (Contains(panel) && _owner.OnPanelRemove(panel))
			{
				_panels.Remove(panel);
				_owner.OnPanelRemoved(panel);
				if (panel.GetParentContainer() == _owner)
				{
					panel.SetParentContainer(null);
				}
			}
		}

		public void RemoveAt(int index)
		{
			Remove((DockPanelBase)_panels[index]);
		}

		public void Clear()
		{
			ArrayList arrayList = new ArrayList();
			foreach (DockPanelBase item in _panels)
			{
				if (_owner.OnPanelRemove(item))
				{
					arrayList.Add(item);
				}
			}
			foreach (DockPanelBase removedPanel in arrayList)
			{
				_panels.Remove(removedPanel);
				_owner.OnPanelRemoved(removedPanel);
				if (removedPanel.GetParentContainer() == _owner)
				{
					removedPanel.SetParentContainer(null);
				}
			}
		}

		void ICollection.CopyTo(Array destination, int index)
		{
			_panels.CopyTo(destination, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _panels.GetEnumerator();
		}

		int IList.Add(object value)
		{
			Add((DockPanelBase)value);
			return _panels.IndexOf(value);
		}

		bool IList.Contains(object value)
		{
			return _panels.Contains(value);
		}

		int IList.IndexOf(object value)
		{
			return _panels.IndexOf(value);
		}

		void IList.Insert(int index, object value)
		{
			Insert(index, (DockPanelBase)value);
		}

		void IList.Remove(object value)
		{
			Remove((DockPanelBase)value);
		}
	}
}
