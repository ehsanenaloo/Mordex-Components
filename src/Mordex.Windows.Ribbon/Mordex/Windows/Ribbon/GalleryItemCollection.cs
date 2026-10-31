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
	public class GalleryItemCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal ICommandContainer _parent;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "GalleryItem";

		public GalleryItem this[int index] => (GalleryItem)base.List[index];

		public GalleryItem this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (GalleryItem item in base.List)
				{
					if (item.Key != null && string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		internal GalleryItemCollection(ICommandContainer parent)
		{
			_parent = parent;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			DesignEditScope edit = null;
			try
			{
				int itemIndex = 0;
				PropertyValue propertyValue = reader["GalleryItem" + itemIndex];
				ArrayList arrayList = new ArrayList();
				while (propertyValue != null)
				{
					LayoutReader layoutReader = propertyValue.GetLayoutReader();
					string text = (string)LayoutReader.GetPropertyValue(layoutReader, "Name", typeof(string), "");
					GalleryItem galleryItem = FindByName(text);
					if (galleryItem == null)
					{
						galleryItem = new GalleryItem();
					}
					if (galleryItem.Container == null)
					{
						if (edit == null && !(_parent is MxRibbon))
						{
							edit = DesignEditScope.Begin(_parent, "GalleryItems", "Load Layout");
						}
						AddComponentToDesignContainer(galleryItem, text);
						galleryItem.SetParentContainer(_parent);
					}
					propertyValue.DeserializeObject(galleryItem);
					arrayList.Add(galleryItem);
					galleryItem.SetIndexInOwner(itemIndex);
					itemIndex++;
					propertyValue = reader["GalleryItem" + itemIndex];
				}
				List<GalleryItem> droppedItems = new List<GalleryItem>();
				foreach (GalleryItem existingItem in base.InnerList)
				{
					if (!arrayList.Contains(existingItem))
					{
						droppedItems.Add(existingItem);
					}
				}
				ICommandContainer parent = _parent;
				DetachFromParent();
				_parent = parent;
				foreach (GalleryItem item in arrayList)
				{
					AddWithoutEvents(item);
				}
				foreach (GalleryItem droppedItem in droppedItems)
				{
					droppedItem.SetParentContainer(null);
					droppedItem.Dispose();
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
			int itemIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GalleryItem value = (GalleryItem)enumerator.Current;
					info.AddValue("GalleryItem" + itemIndex, value);
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

		private void AddComponentToDesignContainer(Component component, string name)
		{
			if (_parent == null)
			{
				return;
			}
			MxRibbon ribbon = _parent.Ribbon;
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

		public void Add(GalleryItem item)
		{
			base.List.Add(item);
		}

		public void AddRange(GalleryItem[] items)
		{
			foreach (GalleryItem item in items)
			{
				Add(item);
			}
		}

		public void Remove(GalleryItem item)
		{
			base.List.Remove(item);
		}

		internal void RemoveWithoutEvents(GalleryItem item)
		{
			if (Contains(item))
			{
				base.InnerList.Remove(item);
				item.SetParentContainer(null);
				RenumberItems();
			}
		}

		public void Insert(int index, GalleryItem item)
		{
			base.List.Insert(index, item);
		}

		internal void AddWithoutEvents(GalleryItem galleryItem)
		{
			base.InnerList.Add(galleryItem);
			RenumberItems();
			if (!galleryItem.IsMerged())
			{
				galleryItem.SetParentContainer(_parent);
			}
		}

		internal void DisposeAll()
		{
			GalleryItem[] snapshot = new GalleryItem[base.Count];
			base.InnerList.CopyTo(snapshot, 0);
			foreach (GalleryItem item in snapshot)
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
				GalleryItem item = (GalleryItem)base.List[index];
				Remove(item);
				index--;
			}
		}

		internal void RemoveAllWithoutEvents()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				GalleryItem galleryItem = (GalleryItem)base.List[index];
				RemoveWithoutEvents(galleryItem);
				index--;
			}
		}

		public bool Contains(GalleryItem item)
		{
			return base.List.Contains(item);
		}

		public int IndexOf(GalleryItem item)
		{
			return base.List.IndexOf(item);
		}

		internal void DetachFromParent()
		{
			_parent = null;
			base.InnerList.Clear();
		}

		internal GalleryItem FindByName(string strA)
		{
			if (strA.Length == 0)
			{
				return null;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (GalleryItem item in base.List)
			{
				if (string.Compare(strA, item.Name, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		public bool Contains(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (GalleryItem item in base.List)
			{
				if (item.Key != null && string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}

		private void RenumberItems()
		{
			for (int i = 0; i < base.Count; i++)
			{
				((GalleryItem)base.List[i]).SetIndexInOwner(i);
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			RenumberItems();
			GalleryItem galleryItem = (GalleryItem)value;
			if (!galleryItem.IsMerged())
			{
				galleryItem.SetParentContainer(_parent);
			}
			_parent.CommandAdded(galleryItem);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			RenumberItems();
			GalleryItem galleryItem = (GalleryItem)value;
			if (!galleryItem.IsMerged())
			{
				galleryItem.SetParentContainer(null);
			}
			_parent.CommandRemoved(galleryItem);
		}

		internal GalleryItemCollection CloneWithParent(ICommandContainer parent)
		{
			GalleryItemCollection galleryItemCollection = new GalleryItemCollection(parent);
			foreach (GalleryItem sourceItem in base.List)
			{
				GalleryItem item = (GalleryItem)sourceItem.Clone();
				galleryItemCollection.Add(item);
			}
			return galleryItemCollection;
		}

		internal GalleryItem FindByKey(string strA)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (GalleryItem item in base.List)
			{
				if (item.Key != null && string.Compare(strA, item.Key, ignoreCase: true, invariantCulture) == 0)
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
					CommandBase commandBase = (CommandBase)enumerator.Current;
					commandBase.SetOriginalIndex(IndexOf(commandBase as GalleryItem));
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

		internal void MergeItems(GalleryItemCollection galleryItemCollection)
		{
			DisposeResources();
			galleryItemCollection.DisposeResources();
			foreach (GalleryItem item in galleryItemCollection)
			{
				CommandMergeType mergeType = item.MergeType;
				GalleryItem existingItem = FindByKey(item.Key);
				if (_parent != null)
				{
					item.ApplyMergeState(_parent.Ribbon, true);
				}
				else
				{
					item.SetNullable(true);
				}
				item._ribbon = _parent.Ribbon;
				switch (mergeType)
				{
				case CommandMergeType.Add:
					base.List.Add(item);
					continue;
				case CommandMergeType.Hide:
					if (existingItem != null && !Contains(item))
					{
						existingItem.SetHiddenByMerge(true);
						item._replacedCommand = existingItem;
					}
					continue;
				case CommandMergeType.MergeItems:
					base.List.Add(item);
					continue;
				}
				if (existingItem != null)
				{
					if (!Contains(item))
					{
						item._replacedCommand = existingItem;
						ReplaceCommand(existingItem, item);
					}
				}
				else
				{
					base.List.Add(item);
				}
			}
			CommandMergeOrderComparer comparer = new CommandMergeOrderComparer(this);
			base.InnerList.Sort(comparer);
			RenumberItems();
		}

		internal void UndoMerge()
		{
			for (int i = 0; i < base.Count; i++)
			{
				GalleryItem galleryItem = (GalleryItem)base.List[i];
				if (!galleryItem.IsMerged())
				{
					galleryItem.SetHiddenByMerge(false);
				}
				else if (galleryItem._replacedCommand != null)
				{
					if (galleryItem.MergeType == CommandMergeType.Hide)
					{
						galleryItem._replacedCommand.SetHiddenByMerge(false);
					}
					else
					{
						ReplaceCommand(galleryItem, galleryItem._replacedCommand);
					}
					galleryItem._replacedCommand = null;
					if (_parent != null)
					{
						galleryItem.ApplyMergeState(_parent.Ribbon, false);
					}
					else
					{
						galleryItem.SetNullable(false);
					}
					galleryItem._ribbon = null;
				}
				else
				{
					Remove(galleryItem);
					i--;
					if (_parent != null)
					{
						galleryItem.ApplyMergeState(_parent.Ribbon, false);
					}
					else
					{
						galleryItem.SetNullable(false);
					}
					galleryItem._ribbon = null;
				}
			}
			CommandOriginalIndexComparer comparer = new CommandOriginalIndexComparer(this);
			base.InnerList.Sort(comparer);
			RenumberItems();
		}

		internal void ReplaceCommand(CommandBase commandBase, CommandBase replacement)
		{
			int index = base.List.IndexOf(commandBase);
			base.List[index] = replacement;
			replacement.SetIndexInOwner(index);
		}
	}
}
