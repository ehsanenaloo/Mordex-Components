using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.Tabs
{
	public class TabPageCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxTabControl _tabControl;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "TabPage";

		public MxTabPage this[int index] => (MxTabPage)base.List[index];

		public MxTabPage this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (MxTabPage item in base.List)
				{
					if (string.Compare(item.Key, key, ignoreCase: true, invariantCulture) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			int index = 0;
			for (PropertyValue propertyValue = reader["TabPage" + index]; propertyValue != null; propertyValue = reader["TabPage" + index])
			{
				MxTabPage uiTabPage = new MxTabPage();
				base.InnerList.Add(uiTabPage);
				uiTabPage._index = index;
				index++;
				propertyValue.DeserializeObject(uiTabPage);
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
					MxTabPage value = (MxTabPage)enumerator.Current;
					info.AddValue("TabPage" + index, value);
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

		internal TabPageCollection(MxTabControl tabControl)
		{
			_tabControl = tabControl;
		}

		public void AddRange(MxTabPage[] tabPages)
		{
			if (tabPages != null)
			{
				foreach (MxTabPage value in tabPages)
				{
					base.List.Add(value);
				}
			}
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.Count; index++)
			{
				MxTabPage tabPage = (MxTabPage)base.List[index];
				Remove(tabPage);
				index--;
			}
		}

		internal void ClearWithoutEvents()
		{
			base.InnerList.Clear();
		}

		public void Add(MxTabPage tabPage)
		{
			base.List.Add(tabPage);
		}

		public void Remove(MxTabPage tabPage)
		{
			if (base.List.Contains(tabPage))
			{
				base.List.Remove(tabPage);
			}
		}

		public bool Contains(MxTabPage tabPage)
		{
			return base.List.Contains(tabPage);
		}

		internal MxTabPage FindByName(string name)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxTabPage item in base.List)
			{
				if (string.Compare(item.Name, name, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		public bool Contains(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxTabPage uiTabPage = (MxTabPage)enumerator.Current;
					if (string.Compare(uiTabPage.Key, key, ignoreCase: true, invariantCulture) == 0)
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

		public void Insert(int index, MxTabPage tabPage)
		{
			if (Contains(tabPage))
			{
				Remove(tabPage);
			}
			if (index >= base.List.Count)
			{
				Add(tabPage);
			}
			else
			{
				base.List.Insert(index, tabPage);
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			MxTabPage uiTabPage = (MxTabPage)value;
			uiTabPage.SetOwnerCollection(this);
			ReindexPages();
			_tabControl.AddTabPageControl(uiTabPage);
			InvalidateTabLayout();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxTabPage uiTabPage = (MxTabPage)value;
			ReindexPages();
			uiTabPage.SetOwnerCollection(null);
			_tabControl.RemoveTabPageControl(uiTabPage);
			uiTabPage._index = -1;
			InvalidateTabLayout();
		}

		// Moves a page to the place of another page without the events of a removal and an insertion.
		internal void MovePage(MxTabPage page, MxTabPage target)
		{
			int from = base.InnerList.IndexOf(page);
			int to = base.InnerList.IndexOf(target);
			if (from < 0 || to < 0 || from == to)
			{
				return;
			}
			base.InnerList.RemoveAt(from);
			base.InnerList.Insert(to, page);
			ReindexPages();
			InvalidateTabLayout();
		}

		internal void ReindexPages()
		{
			for (int i = 0; i < base.List.Count; i++)
			{
				MxTabPage uiTabPage = (MxTabPage)base.List[i];
				uiTabPage._index = i;
			}
		}

		private void InvalidateTabLayout()
		{
			_tabControl.InvalidateLayout();
		}
	}
}
