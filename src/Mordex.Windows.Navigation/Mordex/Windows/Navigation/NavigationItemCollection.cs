using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public class NavigationItemCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private NavigationGroup _group;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Item";

		public NavigationItem this[int index] => (NavigationItem)base.InnerList[index];

		public NavigationItem this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						NavigationItem navigationBarItem = (NavigationItem)enumerator.Current;
						if (string.Equals(navigationBarItem.Key, key, StringComparison.OrdinalIgnoreCase))
						{
							return navigationBarItem;
						}
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				throw new ArgumentOutOfRangeException();
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int itemIndex = 0;
			PropertyValue propertyValue = info["Item" + itemIndex];
			while (propertyValue != null)
			{
				NavigationItem navigationBarItem = new NavigationItem();
				base.InnerList.Add(navigationBarItem);
				navigationBarItem._owner = this;
				navigationBarItem._index = itemIndex;
				itemIndex++;
				propertyValue.DeserializeObject(navigationBarItem);
				propertyValue = info["Item" + itemIndex];
				if (navigationBarItem.Selected)
				{
					_group.NavigationBar._selectedItem = navigationBarItem;
					navigationBarItem._itemState = ItemState.Selected;
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int itemIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationItem item = (NavigationItem)enumerator.Current;
					info.AddValue("Item" + itemIndex, item);
					itemIndex++;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		internal NavigationItemCollection(NavigationGroup group)
		{
			if (group == null)
			{
				throw new ArgumentNullException();
			}
			_group = group;
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationItem navigationBarItem = (NavigationItem)enumerator.Current;
					navigationBarItem._owner = null;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		protected override void OnClearComplete()
		{
			RelayoutGroup();
		}

		public bool Contains(NavigationItem item)
		{
			return base.InnerList.Contains(item);
		}

		public void Insert(int index, NavigationItem item)
		{
			if (item._owner != null)
			{
				throw new InvalidOperationException();
			}
			for (int i = index; i < base.Count; i++)
			{
				this[i]._index++;
			}
			item._index = index;
			item._owner = this;
			base.InnerList.Insert(index, item);
			RelayoutGroup();
		}

		public new void RemoveAt(int index)
		{
			if (index < 0 || index >= base.Count)
			{
				throw new ArgumentOutOfRangeException();
			}
			NavigationItem item = this[index];
			Remove(item);
		}

		public void Remove(NavigationItem item)
		{
			if (base.InnerList.Contains(item))
			{
				if (_group != null && _group._owner != null && item == GetGroup().NavigationBar.SelectedItem)
				{
					_group.NavigationBar.ClearSelectionOfRemovedItem(item);
				}
				for (int i = item._index + 1; i < base.Count; i++)
				{
					this[i]._index--;
				}
				base.InnerList.Remove(item);
				if (GetGroup() != null && _group.NavigationBar != null && GetGroup().NavigationBar._firstVisibleItem == item)
				{
					_group.NavigationBar._firstVisibleItem = null;
				}
				item._owner = null;
				RelayoutGroup();
			}
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				NavigationItem item = (NavigationItem)base.List[index];
				Remove(item);
				index--;
			}
		}

		internal NavigationGroup GetGroup()
		{
			return _group;
		}

		public void Add(NavigationItem item)
		{
			if (item._owner != null)
			{
				throw new InvalidOperationException();
			}
			item._index = base.Count;
			base.InnerList.Add(item);
			item._owner = this;
			RelayoutGroup();
		}

		public void AddRange(NavigationItem[] items)
		{
			for (int i = 0; i < items.Length; i++)
			{
				Add(items[i]);
			}
		}

		public void Add(string text)
		{
			NavigationItem item = new NavigationItem(text);
			Add(item);
		}

		public void Add(string text, int imageIndex)
		{
			NavigationItem item = new NavigationItem(text, imageIndex);
			Add(item);
		}

		internal void RelayoutGroup()
		{
			if (_group != null)
			{
				_group.InvalidateLayout();
			}
		}

		internal void RepaintBar()
		{
			if (_group != null)
			{
				_group.RepaintBar();
			}
		}

		internal void ResetItemStates()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationItem navigationBarItem = (NavigationItem)enumerator.Current;
					navigationBarItem._itemState = ItemState.Normal;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
