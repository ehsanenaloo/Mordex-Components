using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public class NavigationGroupCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private MxNavigationBar _navigationBar;

		internal bool _resetGroupContainers = true;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Group";

		public NavigationGroup this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						NavigationGroup navigationBarGroup = (NavigationGroup)enumerator.Current;
						if (string.Equals(navigationBarGroup.Key, key, StringComparison.OrdinalIgnoreCase))
						{
							return navigationBarGroup;
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

		public NavigationGroup this[int index] => (NavigationGroup)base.InnerList[index];

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int groupIndex = 0;
			PropertyValue propertyValue = info["Group" + groupIndex];
			while (propertyValue != null)
			{
				NavigationGroup navigationBarGroup = new NavigationGroup();
				base.InnerList.Add(navigationBarGroup);
				navigationBarGroup._owner = this;
				navigationBarGroup._index = groupIndex;
				groupIndex++;
				propertyValue.DeserializeObject(navigationBarGroup);
				propertyValue = info["Group" + groupIndex];
				if (navigationBarGroup.Selected)
				{
					_navigationBar._selectedGroup = navigationBarGroup;
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int groupIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationGroup group = (NavigationGroup)enumerator.Current;
					info.AddValue("Group" + groupIndex, group);
					groupIndex++;
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

		internal NavigationGroupCollection(MxNavigationBar navigationBar)
		{
			if (navigationBar == null)
			{
				throw new ArgumentNullException();
			}
			_navigationBar = navigationBar;
		}

		public bool Contains(NavigationGroup group)
		{
			return base.InnerList.Contains(group);
		}

		internal MxNavigationBar GetNavigationBar()
		{
			return _navigationBar;
		}

		public void Insert(int index, NavigationGroup group)
		{
			if (group._owner != null)
			{
				throw new InvalidOperationException();
			}
			for (int i = index; i < base.Count; i++)
			{
				this[i]._index++;
			}
			group._index = index;
			base.InnerList.Insert(index, group);
			group._owner = this;
			RelayoutBar();
		}

		internal void UpdateGroupContainer(NavigationGroup navigationBarGroup)
		{
			if (_navigationBar != null)
			{
				_navigationBar.UpdateGroupContainerControl(navigationBarGroup);
			}
		}

		internal void RelayoutBar()
		{
			if (_navigationBar != null)
			{
				_navigationBar.InvalidateLayout(true);
			}
		}

		internal void InvalidateItemLayout()
		{
			if (_navigationBar != null)
			{
				_navigationBar.InvalidateItemLayout();
			}
		}

		internal void RequestRepaint()
		{
			if (_navigationBar != null)
			{
				_navigationBar.RequestRepaint();
			}
		}

		public void AddRange(NavigationGroup[] groups)
		{
			foreach (NavigationGroup navigationBarGroup in groups)
			{
				navigationBarGroup._index = base.Count;
				navigationBarGroup._owner = this;
				base.InnerList.Add(navigationBarGroup);
			}
			if (_navigationBar.SelectedGroup == null && base.Count > 0)
			{
				_navigationBar.SelectedGroup = this[0];
			}
			RelayoutBar();
		}

		public void Add(NavigationGroup group)
		{
			if (group._owner != null)
			{
				throw new InvalidOperationException();
			}
			group._index = base.Count;
			group._owner = this;
			base.InnerList.Add(group);
			if (_navigationBar.SelectedGroup == null)
			{
				_navigationBar.SelectedGroup = group;
			}
			RelayoutBar();
		}

		public void Add(string text)
		{
			NavigationGroup navigationBarGroup = new NavigationGroup(text);
			Add(navigationBarGroup);
		}

		public void Remove(NavigationGroup group)
		{
			if (!base.InnerList.Contains(group))
			{
				return;
			}
			for (int i = group._index + 1; i < base.Count; i++)
			{
				this[i]._index--;
			}
			base.InnerList.Remove(group);
			if (_resetGroupContainers)
			{
				group.Container = false;
			}
			group._owner = null;
			if (group.Selected)
			{
				if (base.Count == 0)
				{
					_navigationBar.SelectedGroup = null;
				}
				else
				{
					_navigationBar.SelectedGroup = (NavigationGroup)base.InnerList[0];
				}
			}
			RelayoutBar();
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationGroup navigationBarGroup = (NavigationGroup)enumerator.Current;
					if (_resetGroupContainers)
					{
						navigationBarGroup.Container = false;
					}
					navigationBarGroup._owner = null;
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

		internal void ClearKeepingContainers()
		{
			_resetGroupContainers = false;
			base.List.Clear();
			_resetGroupContainers = true;
		}

		protected override void OnClearComplete()
		{
			_navigationBar.SelectedGroup = null;
			RelayoutBar();
		}
	}
}
