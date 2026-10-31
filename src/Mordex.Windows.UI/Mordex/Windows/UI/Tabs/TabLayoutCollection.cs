using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.Tabs
{
	public class TabLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private TabLayout _parentLayout;

		private MxTabControl _tabControl;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public MxTabControl TabControl => _tabControl;

		public TabLayout this[int index] => (TabLayout)base.List[index];

		public TabLayout this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TabLayout uiTabLayout = (TabLayout)enumerator.Current;
						if (string.Compare(uiTabLayout.Key, key, ignoreCase: true, invariantCulture) == 0)
						{
							return uiTabLayout;
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
			int index = 0;
			for (PropertyValue propertyValue = info["Layout" + index]; propertyValue != null; propertyValue = info["Layout" + index])
			{
				TabLayout uiTabLayout = new TabLayout();
				propertyValue.DeserializeObject(uiTabLayout);
				Add(uiTabLayout);
				index++;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TabLayout loadedLayout = (TabLayout)enumerator.Current;
					if (loadedLayout._isCurrentLayout && TabControl != null)
					{
						TabControl.CurrentLayout = loadedLayout;
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
			int index = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TabLayout value = (TabLayout)enumerator.Current;
					info.AddValue("Layout" + index, value);
					index++;
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

		internal TabLayoutCollection(MxTabControl tabControl)
		{
			_tabControl = tabControl;
		}

		internal TabLayoutCollection(TabLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal TabLayout GetParentLayout()
		{
			return _parentLayout;
		}

		internal MxTabControl GetTabControl()
		{
			if (_parentLayout == null)
			{
				return _tabControl;
			}
			return _parentLayout.GetTabControl();
		}

		public void Add(TabLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(TabLayout[] layouts)
		{
			foreach (TabLayout layout in layouts)
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
			if (value.GetType() != typeof(TabLayout))
			{
				throw new InvalidCastException();
			}
			TabLayout uiTabLayout = (TabLayout)value;
			if (uiTabLayout.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !uiTabLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			TabLayout uiTabLayout = (TabLayout)value;
			uiTabLayout.SetOwnerCollection(this);
			uiTabLayout.MakeIncremental();
		}

		public void Remove(TabLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxTabControl uiTab = GetTabControl();
			if (uiTab != null && uiTab.CurrentLayout == (TabLayout)value)
			{
				uiTab.CurrentLayout = null;
			}
			((TabLayout)value).SetOwnerCollection(null);
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TabLayout uiTabLayout = (TabLayout)enumerator.Current;
					uiTabLayout.SetOwnerCollection(null);
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

		public int IndexOf(TabLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(TabLayout layout)
		{
			return base.List.Contains(layout);
		}

		public bool Contains(string layoutKey)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TabLayout uiTabLayout = (TabLayout)enumerator.Current;
					if (string.Compare(uiTabLayout.Key, layoutKey, ignoreCase: true, invariantCulture) == 0)
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

		internal TabLayout FindCurrentLayout()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TabLayout uiTabLayout = (TabLayout)enumerator.Current;
					TabLayout currentLayout = uiTabLayout.FindCurrentLayout();
					if (currentLayout != null)
					{
						return currentLayout;
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
	}
}
