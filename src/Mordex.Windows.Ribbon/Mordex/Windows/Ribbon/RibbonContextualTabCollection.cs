using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	public class RibbonContextualTabCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private MxRibbon _ribbon;

		private IList<RibbonContextualTab> _removedTabs;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "ContextualTab";

		public RibbonContextualTab this[int index] => (RibbonContextualTab)base.List[index];

		public RibbonContextualTab this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (RibbonContextualTab item in base.List)
				{
					if (string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		internal RibbonContextualTabCollection(MxRibbon ribbon)
		{
			_ribbon = ribbon;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			int tabIndex = 0;
			PropertyValue propertyValue = reader["ContextualTab" + tabIndex];
			ArrayList arrayList = new ArrayList();
			while (propertyValue != null)
			{
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				string text = (string)LayoutReader.GetPropertyValue(layoutReader, "Name", typeof(string), "");
				RibbonContextualTab ribbonContextualTab = FindByName(text);
				if (ribbonContextualTab == null)
				{
					ribbonContextualTab = new RibbonContextualTab();
				}
				if (ribbonContextualTab.Container == null)
				{
					AddToDesignContainer(ribbonContextualTab, text);
					ribbonContextualTab._ribbon = _ribbon;
				}
				propertyValue.DeserializeObject(ribbonContextualTab);
				arrayList.Add(ribbonContextualTab);
				ribbonContextualTab.SetIndexInOwner(tabIndex);
				tabIndex++;
				propertyValue = reader["ContextualTab" + tabIndex];
			}
			RibbonContextualTab[] existingItems = new RibbonContextualTab[base.Count];
			base.InnerList.CopyTo(existingItems);
			foreach (RibbonContextualTab existingTab in existingItems)
			{
				if (!arrayList.Contains(existingTab))
				{
					existingTab.Dispose();
				}
			}
			RemoveAllWithoutEvents();
			foreach (RibbonContextualTab item in arrayList)
			{
				AttachTab(item);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int tabIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RibbonContextualTab value = (RibbonContextualTab)enumerator.Current;
					info.AddValue("ContextualTab" + tabIndex, value);
					tabIndex++;
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

		private void AddToDesignContainer(Component component, string name)
		{
			if (_ribbon == null || !_ribbon.IsInDesignMode() || _ribbon.Container == null)
			{
				return;
			}
			if (name != null && name.Length > 0 && _ribbon.Site != null)
			{
				INameCreationService nameCreationService = (INameCreationService)_ribbon.Site.GetService(typeof(INameCreationService));
				if (nameCreationService == null || nameCreationService.IsValidName(name))
				{
					_ribbon.Container.Add(component, name);
				}
				else
				{
					_ribbon.Container.Add(component);
				}
			}
			else
			{
				_ribbon.Container.Add(component);
			}
		}

		public void Add(RibbonContextualTab tab)
		{
			base.List.Add(tab);
		}

		public void Insert(int index, RibbonContextualTab tab)
		{
			base.List.Insert(index, tab);
		}

		public void AddRange(RibbonContextualTab[] tabs)
		{
			foreach (RibbonContextualTab tab in tabs)
			{
				Add(tab);
			}
		}

		public void Remove(RibbonContextualTab tab)
		{
			base.List.Remove(tab);
		}

		internal void AttachTab(RibbonContextualTab ribbonContextualTab)
		{
			base.InnerList.Add(ribbonContextualTab);
			RenumberTabs();
			if (!ribbonContextualTab.IsMerged())
			{
				ribbonContextualTab._ribbon = _ribbon;
			}
			_ribbon.HandleContextualTabAdded(ribbonContextualTab, false);
		}

		internal void RemoveWithoutEvents(RibbonContextualTab tab)
		{
			if (base.List.Contains(tab))
			{
				base.InnerList.Remove(tab);
				_ribbon.HandleContextualTabRemoved(tab, false);
				tab._ribbon = null;
				RenumberTabs();
			}
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				RibbonContextualTab tab = (RibbonContextualTab)base.List[index];
				Remove(tab);
				index--;
			}
		}

		internal void RemoveAllWithoutEvents()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)base.List[index];
				RemoveWithoutEvents(ribbonContextualTab);
				index--;
			}
		}

		internal RibbonContextualTab FindByName(string strA)
		{
			if (strA.Length == 0)
			{
				return null;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (RibbonContextualTab item in base.List)
			{
				if (string.Compare(strA, item.Name, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		public bool Contains(RibbonContextualTab tab)
		{
			return base.List.Contains(tab);
		}

		public bool Contains(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (RibbonContextualTab item in base.List)
			{
				if (string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public int IndexOf(RibbonContextualTab cTab)
		{
			return base.List.IndexOf(cTab);
		}

		private void RenumberTabs()
		{
			for (int i = 0; i < base.Count; i++)
			{
				((RibbonContextualTab)base.List[i]).SetIndexInOwner(i);
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			RenumberTabs();
			RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)value;
			if (!ribbonContextualTab.IsMerged())
			{
				ribbonContextualTab._ribbon = _ribbon;
			}
			_ribbon.HandleContextualTabAdded(ribbonContextualTab, true);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			RenumberTabs();
			RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)value;
			_ribbon.HandleContextualTabRemoved(ribbonContextualTab, true);
			if (!ribbonContextualTab.IsMerged())
			{
				ribbonContextualTab._ribbon = null;
			}
		}

		internal void RemoveMergedTabs()
		{
			_removedTabs = new List<RibbonContextualTab>();
			for (int i = 0; i < base.List.Count; i++)
			{
				RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)base.List[i];
				if (ribbonContextualTab.IsMerged())
				{
					_removedTabs.Add(ribbonContextualTab);
					Remove(ribbonContextualTab);
					i--;
				}
			}
		}

		internal void ReinsertRemovedTabs()
		{
			if (_removedTabs == null)
			{
				return;
			}
			foreach (RibbonContextualTab item in _removedTabs)
			{
				if (item.GetOriginalIndex() < base.Count)
				{
					Insert(item.GetOriginalIndex(), item);
				}
				else
				{
					Add(item);
				}
			}
		}

		internal RibbonContextualTab FindByKey(string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (RibbonContextualTab item in base.List)
			{
				if (item.Key != null && string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		private void DisposeResources()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)enumerator.Current;
					ribbonContextualTab.SetOriginalIndex(IndexOf(ribbonContextualTab));
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

		internal void MergeFrom(RibbonContextualTabCollection ribbonContextualTabCollection)
		{
			ribbonContextualTabCollection.DisposeResources();
			DisposeResources();
			foreach (RibbonContextualTab item in ribbonContextualTabCollection)
			{
				if (!item.AllowMerge)
				{
					continue;
				}
				CommandMergeType mergeType = item.MergeType;
				RibbonContextualTab existingTab = FindByKey(item.Key);
				if (_ribbon != null)
				{
					item.UpdateMergedState(_ribbon, true);
				}
				else
				{
					item.SetMerged(true);
				}
				item.SetStandaloneRibbon(_ribbon);
				switch (mergeType)
				{
				case CommandMergeType.Add:
					base.List.Add(item);
					continue;
				case CommandMergeType.Hide:
					if (existingTab != null && !Contains(item))
					{
						existingTab.SetHiddenByMerge(true);
						item._unusedTabReference = existingTab;
					}
					continue;
				case CommandMergeType.MergeItems:
					if (existingTab != null)
					{
						existingTab.Tabs.MergeTabs(item.Tabs);
					}
					else
					{
						base.List.Add(item);
					}
					continue;
				}
				if (existingTab != null)
				{
					if (!Contains(item))
					{
						item._unusedTabReference = existingTab;
						ReplaceTab(existingTab, item);
					}
				}
				else
				{
					base.List.Add(item);
				}
			}
			RibbonContextualTabMergeOrderComparer comparer = new RibbonContextualTabMergeOrderComparer();
			base.InnerList.Sort(comparer);
			RenumberTabs();
		}

		internal void UndoMerge()
		{
			for (int i = 0; i < base.Count; i++)
			{
				RibbonContextualTab ribbonContextualTab = (RibbonContextualTab)base.List[i];
				if (!ribbonContextualTab.IsMerged())
				{
					ribbonContextualTab.SetHiddenByMerge(false);
					ribbonContextualTab.Tabs.UndoMerge();
				}
				else if (ribbonContextualTab._unusedTabReference != null)
				{
					if (ribbonContextualTab.MergeType == CommandMergeType.Hide)
					{
						ribbonContextualTab._unusedTabReference.SetHiddenByMerge(false);
					}
					else
					{
						ReplaceTab(ribbonContextualTab, ribbonContextualTab._unusedTabReference);
					}
					ribbonContextualTab._unusedTabReference = null;
					if (_ribbon != null)
					{
						ribbonContextualTab.UpdateMergedState(_ribbon, false);
					}
					else
					{
						ribbonContextualTab.SetMerged(false);
					}
					ribbonContextualTab._standaloneRibbon = null;
				}
				else
				{
					Remove(ribbonContextualTab);
					i--;
					if (_ribbon != null)
					{
						ribbonContextualTab.UpdateMergedState(_ribbon, false);
					}
					else
					{
						ribbonContextualTab.SetMerged(false);
					}
					ribbonContextualTab._standaloneRibbon = null;
				}
			}
			RibbonContextualTabOriginalIndexComparer comparer = new RibbonContextualTabOriginalIndexComparer();
			base.InnerList.Sort(comparer);
			RenumberTabs();
		}

		internal void ReplaceTab(RibbonContextualTab ribbonContextualTab, RibbonContextualTab replacement)
		{
			int index = base.List.IndexOf(ribbonContextualTab);
			base.List[index] = replacement;
			replacement.SetIndexInOwner(index);
		}
	}
}
