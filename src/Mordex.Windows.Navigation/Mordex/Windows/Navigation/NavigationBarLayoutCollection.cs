using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	public class NavigationBarLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private NavigationBarLayout _parentLayout;

		private MxNavigationBar _navigationBar;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public NavigationBarLayout this[int index] => (NavigationBarLayout)base.List[index];

		public NavigationBarLayout this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						NavigationBarLayout navigationBarLayout = (NavigationBarLayout)enumerator.Current;
						if (string.Equals(navigationBarLayout.Key, key, StringComparison.OrdinalIgnoreCase))
						{
							return navigationBarLayout;
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
			int layoutIndex = 0;
			for (PropertyValue propertyValue = info["Layout" + layoutIndex]; propertyValue != null; propertyValue = info["Layout" + layoutIndex])
			{
				NavigationBarLayout navigationBarLayout = new NavigationBarLayout();
				propertyValue.DeserializeObject(navigationBarLayout);
				Add(navigationBarLayout);
				layoutIndex++;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationBarLayout layout = (NavigationBarLayout)enumerator.Current;
					if (layout._isCurrentLayout)
					{
						GetOwnerNavigationBar().CurrentLayout = layout;
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
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int layoutIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationBarLayout layoutEntry = (NavigationBarLayout)enumerator.Current;
					info.AddValue("Layout" + layoutIndex, layoutEntry);
					layoutIndex++;
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

		internal NavigationBarLayoutCollection(MxNavigationBar btnBar)
		{
			_navigationBar = btnBar;
		}

		internal NavigationBarLayoutCollection(NavigationBarLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal NavigationBarLayout GetParentLayout()
		{
			return _parentLayout;
		}

		internal MxNavigationBar ResolveNavigationBar()
		{
			if (_parentLayout == null)
			{
				return _navigationBar;
			}
			return _parentLayout.ResolveNavigationBar();
		}

		internal MxNavigationBar GetOwnerNavigationBar()
		{
			return _navigationBar;
		}

		public void Add(NavigationBarLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(NavigationBarLayout[] layouts)
		{
			foreach (NavigationBarLayout layout in layouts)
			{
				Add(layout);
			}
		}

		protected override void OnInsert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (value.GetType() != typeof(NavigationBarLayout))
			{
				throw new ArgumentException();
			}
			NavigationBarLayout navigationBarLayout = (NavigationBarLayout)value;
			if (navigationBarLayout.GetParentCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !navigationBarLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			((NavigationBarLayout)value).SetParentCollection(this);
			((NavigationBarLayout)value).UpdateIncrementalLayout();
		}

		public void Remove(NavigationBarLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxNavigationBar navigationBar = ResolveNavigationBar();
			if (navigationBar != null && navigationBar.CurrentLayout == (NavigationBarLayout)value)
			{
				navigationBar.CurrentLayout = null;
			}
			((NavigationBarLayout)value).SetParentCollection(null);
		}

		internal NavigationBarLayout FindCurrentLayout()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationBarLayout navigationBarLayout = (NavigationBarLayout)enumerator.Current;
					NavigationBarLayout foundLayout = navigationBarLayout.FindCurrentLayout();
					if (foundLayout != null)
					{
						return foundLayout;
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
			return null;
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationBarLayout navigationBarLayout = (NavigationBarLayout)enumerator.Current;
					navigationBarLayout.SetParentCollection(null);
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

		public int IndexOf(NavigationBarLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(NavigationBarLayout layout)
		{
			return base.List.Contains(layout);
		}

		public bool Contains(string layoutKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NavigationBarLayout navigationBarLayout = (NavigationBarLayout)enumerator.Current;
					if (string.Equals(navigationBarLayout.Key, layoutKey, StringComparison.OrdinalIgnoreCase))
					{
						return true;
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
			return false;
		}
	}
}
