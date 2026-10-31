using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	public class RibbonTabCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private ITabOwner _owner;

		internal IList<RibbonTab> _mergedTabs;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Tab";

		public RibbonTab this[int index] => (RibbonTab)base.List[index];

		public RibbonTab this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (RibbonTab item in base.List)
				{
					if (string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		internal RibbonTabCollection(ITabOwner owner)
		{
			_owner = owner;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			DesignEditScope edit = null;
			try
			{
				int tabIndex = 0;
				PropertyValue propertyValue = reader["Tab" + tabIndex];
				ArrayList arrayList = new ArrayList();
				while (propertyValue != null)
				{
					LayoutReader layoutReader = propertyValue.GetLayoutReader();
					string text = (string)LayoutReader.GetPropertyValue(layoutReader, "Name", typeof(string), "");
					RibbonTab ribbonTab = FindByName(text);
					if (ribbonTab == null)
					{
						ribbonTab = new RibbonTab();
					}
					if (ribbonTab.Container == null)
					{
						if (edit == null && !(_owner is MxRibbon))
						{
							edit = DesignEditScope.Begin(_owner, "Tabs", "Load Layout");
						}
						AddToDesignerContainer(ribbonTab, text);
						ribbonTab.SetTabOwner(_owner);
					}
					propertyValue.DeserializeObject(ribbonTab);
					arrayList.Add(ribbonTab);
					ribbonTab.Index = tabIndex;
					tabIndex++;
					propertyValue = reader["Tab" + tabIndex];
				}
				RibbonTab[] existingItems = new RibbonTab[base.Count];
				base.InnerList.CopyTo(existingItems);
				foreach (RibbonTab existingTab in existingItems)
				{
					if (!arrayList.Contains(existingTab))
					{
						existingTab.Dispose();
					}
				}
				DetachAll();
				foreach (RibbonTab item in arrayList)
				{
					AddWithoutEvents(item);
				}
				edit?.End();
			}
			finally
			{
				edit?.Dispose();
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
					RibbonTab value = (RibbonTab)enumerator.Current;
					info.AddValue("Tab" + tabIndex, value);
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

		private void AddToDesignerContainer(Component component, string name)
		{
			if (_owner == null)
			{
				return;
			}
			MxRibbon ribbon = _owner.Ribbon;
			if (ribbon == null || !ribbon.IsInDesignMode() || ribbon.Container == null)
			{
				return;
			}
			if (name != null && name.Length > 0 && ribbon.Site != null)
			{
				INameCreationService nameCreationService = (INameCreationService)ribbon.Site.GetService(typeof(INameCreationService));
				if (nameCreationService == null || nameCreationService.IsValidName(name))
				{
					ribbon.Container.Add(component, name);
				}
				else
				{
					ribbon.Container.Add(component);
				}
			}
			else
			{
				ribbon.Container.Add(component);
			}
		}

		public void Add(RibbonTab tab)
		{
			base.List.Add(tab);
		}

		public void AddRange(RibbonTab[] tabs)
		{
			foreach (RibbonTab tab in tabs)
			{
				Add(tab);
			}
		}

		public void Insert(int index, RibbonTab tab)
		{
			base.List.Insert(index, tab);
		}

		public void Remove(RibbonTab tab)
		{
			base.List.Remove(tab);
		}

		internal void DetachTab(RibbonTab tab)
		{
			if (Contains(tab))
			{
				base.InnerList.Remove(tab);
				tab.SetTabOwner(null);
				RenumberTabs();
			}
		}

		internal void AddWithoutEvents(RibbonTab ribbonTab)
		{
			base.InnerList.Add(ribbonTab);
			RenumberTabs();
			if (!ribbonTab.IsMerged())
			{
				ribbonTab.SetTabOwner(_owner);
			}
		}

		internal void DisposeAll()
		{
			RibbonTab[] snapshot = new RibbonTab[base.Count];
			base.InnerList.CopyTo(snapshot, 0);
			foreach (RibbonTab item in snapshot)
			{
				item.Dispose();
				if (base.InnerList.Contains(item))
				{
					Remove(item);
				}
			}
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				RibbonTab tab = (RibbonTab)base.List[index];
				Remove(tab);
				index--;
			}
		}

		internal void DetachAll()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				RibbonTab ribbonTab = (RibbonTab)base.List[index];
				DetachTab(ribbonTab);
				index--;
			}
		}

		public bool Contains(RibbonTab tab)
		{
			return base.List.Contains(tab);
		}

		public bool Contains(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (RibbonTab item in base.List)
			{
				if (string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public int IndexOf(RibbonTab tab)
		{
			return base.List.IndexOf(tab);
		}

		internal RibbonTab FindByName(string strA)
		{
			if (strA.Length == 0)
			{
				return null;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (RibbonTab item in base.List)
			{
				if (string.Compare(strA, item.Name, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		private void RenumberTabs()
		{
			for (int i = 0; i < base.Count; i++)
			{
				((RibbonTab)base.List[i]).Index = i;
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			RenumberTabs();
			RibbonTab ribbonTab = (RibbonTab)value;
			if (!ribbonTab.IsMerged())
			{
				ribbonTab.SetTabOwner(_owner);
			}
			_owner.TabAdded(ribbonTab);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			RenumberTabs();
			RibbonTab ribbonTab = (RibbonTab)value;
			if (!ribbonTab.IsMerged())
			{
				ribbonTab.SetTabOwner(null);
			}
			_owner.TabRemoved(ribbonTab);
		}

		internal RibbonTab FindByKey(string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (RibbonTab item in base.List)
			{
				if (item.Key != null && string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		private void SaveOriginalIndexes()
		{
			for (int i = 0; i < base.List.Count; i++)
			{
				RibbonTab ribbonTab = (RibbonTab)base.List[i];
				ribbonTab.SetIndexInOwner(i);
			}
		}

		internal void StashMergedTabs()
		{
			_mergedTabs = new List<RibbonTab>();
			for (int i = 0; i < base.List.Count; i++)
			{
				RibbonTab ribbonTab = (RibbonTab)base.List[i];
				if (ribbonTab.IsMerged())
				{
					_mergedTabs.Add(ribbonTab);
					base.InnerList.Remove(ribbonTab);
					i--;
				}
			}
		}

		internal void ReinsertRemovedTabs()
		{
			if (_mergedTabs == null)
			{
				return;
			}
			foreach (RibbonTab item in _mergedTabs)
			{
				if (item.GetIndexInOwner() < base.InnerList.Count)
				{
					base.InnerList.Insert(item.GetIndexInOwner(), item);
				}
				else
				{
					base.InnerList.Add(item);
				}
				if (_owner != null)
				{
					if (_owner.Ribbon != null)
					{
						item.UpdateMergedState(_owner.Ribbon._mergeHost, false);
					}
				}
				else
				{
					item.SetMerged(false);
				}
			}
			_mergedTabs = new List<RibbonTab>();
		}

		internal void MergeTabs(RibbonTabCollection ribbonTabCollection)
		{
			ribbonTabCollection.SaveOriginalIndexes();
			SaveOriginalIndexes();
			foreach (RibbonTab item in ribbonTabCollection)
			{
				if (!item.AllowMerge)
				{
					continue;
				}
				CommandMergeType mergeType = item.MergeType;
				RibbonTab existingTab = FindByKey(item.Key);
				if (_owner != null)
				{
					item.UpdateMergedState(_owner.Ribbon, true);
				}
				else
				{
					item.SetMerged(true);
				}
				item.SetRibbon(_owner?.Ribbon);
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
						existingTab.Groups.MergeGroups(item.Groups);
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
			RibbonTabMergeOrderComparer comparer = new RibbonTabMergeOrderComparer();
			base.InnerList.Sort(comparer);
			RenumberTabs();
		}

		internal void UndoMerge()
		{
			for (int i = 0; i < base.Count; i++)
			{
				RibbonTab ribbonTab = (RibbonTab)base.List[i];
				if (!ribbonTab.IsMerged())
				{
					ribbonTab.SetHiddenByMerge(false);
					ribbonTab.Groups.UndoMerge();
				}
				else if (ribbonTab._unusedTabReference != null)
				{
					if (ribbonTab.MergeType == CommandMergeType.Hide)
					{
						ribbonTab._unusedTabReference.SetHiddenByMerge(false);
					}
					else
					{
						ReplaceTab(ribbonTab, ribbonTab._unusedTabReference);
					}
					ribbonTab._unusedTabReference = null;
					if (_owner != null)
					{
						ribbonTab.UpdateMergedState(_owner.Ribbon, false);
					}
					else
					{
						ribbonTab.SetMerged(false);
					}
					ribbonTab.SetRibbon(null);
				}
				else
				{
					Remove(ribbonTab);
					i--;
					if (_owner != null)
					{
						ribbonTab.UpdateMergedState(_owner.Ribbon, false);
					}
					else
					{
						ribbonTab.SetMerged(false);
					}
					ribbonTab.SetRibbon(null);
				}
			}
			RibbonTabOriginalIndexComparer comparer = new RibbonTabOriginalIndexComparer();
			base.InnerList.Sort(comparer);
			RenumberTabs();
		}

		internal void ReplaceTab(RibbonTab ribbonTab, RibbonTab replacement)
		{
			int index = base.List.IndexOf(ribbonTab);
			base.List[index] = replacement;
			replacement.Index = index;
		}
	}
}
