using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	public class RibbonGroupCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private RibbonTab _ribbonTab;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "RibbonGroup";

		public RibbonGroup this[int index] => (RibbonGroup)base.List[index];

		public RibbonGroup this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (RibbonGroup item in base.List)
				{
					if (item.Key != null && string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		internal RibbonGroupCollection(RibbonTab ribbonTab)
		{
			_ribbonTab = ribbonTab;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			DesignEditScope edit = null;
			try
			{
				int groupIndex = 0;
				PropertyValue propertyValue = reader["RibbonGroup" + groupIndex];
				ArrayList arrayList = new ArrayList();
				while (propertyValue != null)
				{
					LayoutReader layoutReader = propertyValue.GetLayoutReader();
					string text = (string)LayoutReader.GetPropertyValue(layoutReader, "Name", typeof(string), "");
					RibbonGroup ribbonGroup = FindByName(text);
					if (ribbonGroup == null)
					{
						ribbonGroup = new RibbonGroup();
					}
					if (ribbonGroup.Container == null)
					{
						if (edit == null)
						{
							edit = DesignEditScope.Begin(_ribbonTab, "Groups", "Load Layout");
						}
						AddToDesignerContainer(ribbonGroup, text);
						ribbonGroup.SetParentContainer(_ribbonTab);
					}
					propertyValue.DeserializeObject(ribbonGroup);
					arrayList.Add(ribbonGroup);
					ribbonGroup.SetIndexInOwner(groupIndex);
					groupIndex++;
					propertyValue = reader["RibbonGroup" + groupIndex];
				}
				RibbonGroup[] existingItems = new RibbonGroup[base.Count];
				base.InnerList.CopyTo(existingItems);
				foreach (RibbonGroup existingGroup in existingItems)
				{
					if (!arrayList.Contains(existingGroup))
					{
						existingGroup.Dispose();
					}
				}
				DetachAll();
				foreach (RibbonGroup item in arrayList)
				{
					AttachGroup(item);
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
			int groupIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					RibbonGroup value = (RibbonGroup)enumerator.Current;
					info.AddValue("RibbonGroup" + groupIndex, value);
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

		private void AddToDesignerContainer(Component component, string name)
		{
			if (_ribbonTab == null)
			{
				return;
			}
			MxRibbon ribbon = _ribbonTab.Ribbon;
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

		public void Add(RibbonGroup group)
		{
			base.List.Add(group);
		}

		public void AddRange(RibbonGroup[] groups)
		{
			foreach (RibbonGroup ribbonGroup in groups)
			{
				Add(ribbonGroup);
			}
		}

		public void Insert(int index, RibbonGroup group)
		{
			base.List.Insert(index, group);
		}

		public void Remove(RibbonGroup group)
		{
			base.List.Remove(group);
		}

		internal void DetachGroup(CommandBase command)
		{
			if (base.List.Contains(command))
			{
				base.InnerList.Remove(command);
				command.SetParentContainer(null);
				command.CommandsOwner = null;
				RenumberGroups();
			}
		}

		internal void AttachGroup(RibbonGroup ribbonGroup)
		{
			base.InnerList.Add(ribbonGroup);
			RenumberGroups();
			if (!ribbonGroup.IsMerged())
			{
				ribbonGroup.SetParentContainer(_ribbonTab);
			}
		}

		internal void DisposeAll()
		{
			RibbonGroup[] snapshot = new RibbonGroup[base.Count];
			base.InnerList.CopyTo(snapshot, 0);
			foreach (RibbonGroup item in snapshot)
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
				RibbonGroup ribbonGroup = (RibbonGroup)base.List[index];
				Remove(ribbonGroup);
				index--;
			}
		}

		internal void DetachAll()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				RibbonGroup ribbonGroup = (RibbonGroup)base.List[index];
				DetachGroup(ribbonGroup);
				index--;
			}
		}

		internal RibbonGroup FindByName(string strA)
		{
			if (strA.Length == 0)
			{
				return null;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (RibbonGroup item in base.List)
			{
				if (string.Compare(strA, item.Name, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		public bool Contains(RibbonGroup group)
		{
			return base.List.Contains(group);
		}

		public bool Contains(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (CommandBase item in base.List)
			{
				if (item.Key != null && string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public int IndexOf(RibbonGroup group)
		{
			return base.List.IndexOf(group);
		}

		protected override void OnInsertComplete(int index, object value)
		{
			RenumberGroups();
			RibbonGroup ribbonGroup = (RibbonGroup)value;
			if (!ribbonGroup.IsMerged())
			{
				ribbonGroup.SetParentContainer(_ribbonTab);
			}
			((ICommandContainer)_ribbonTab).CommandAdded((CommandBase)ribbonGroup);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			RenumberGroups();
			RibbonGroup ribbonGroup = (RibbonGroup)value;
			if (!ribbonGroup.IsMerged())
			{
				ribbonGroup.SetParentContainer(null);
			}
			((ICommandContainer)_ribbonTab).CommandRemoved((CommandBase)ribbonGroup);
		}

		private void RenumberGroups()
		{
			for (int i = 0; i < base.Count; i++)
			{
				((RibbonGroup)base.List[i]).SetIndexInOwner(i);
			}
		}

		internal RibbonGroupCollection CloneForTab(RibbonTab ribbonTab)
		{
			RibbonGroupCollection ribbonGroupCollection = new RibbonGroupCollection(ribbonTab);
			foreach (RibbonGroup item in base.List)
			{
				RibbonGroup clonedGroup = (RibbonGroup)item.Clone();
				ribbonGroupCollection.Add(clonedGroup);
			}
			return ribbonGroupCollection;
		}

		internal RibbonGroup FindByKey(string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (RibbonGroup item in base.List)
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
					RibbonGroup ribbonGroup = (RibbonGroup)enumerator.Current;
					ribbonGroup.SetOriginalIndex(IndexOf(ribbonGroup));
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

		internal void MergeGroups(RibbonGroupCollection ribbonGroupCollection)
		{
			DisposeResources();
			ribbonGroupCollection.DisposeResources();
			foreach (RibbonGroup item in ribbonGroupCollection)
			{
				CommandMergeType mergeType = item.MergeType;
				RibbonGroup existingGroup = FindByKey(item.Key);
				if (_ribbonTab != null)
				{
					item.ApplyMergeState(_ribbonTab.Ribbon, true);
				}
				else
				{
					item.SetNullable(true);
				}
				item._ribbon = _ribbonTab.Ribbon;
				switch (mergeType)
				{
				case CommandMergeType.Add:
					base.List.Add(item);
					continue;
				case CommandMergeType.Hide:
					if (existingGroup != null && !Contains(item))
					{
						existingGroup.SetHiddenByMerge(true);
						item._replacedCommand = existingGroup;
					}
					continue;
				case CommandMergeType.MergeItems:
					if (existingGroup != null)
					{
						existingGroup.Commands.MergeCommands(item.Commands);
					}
					else
					{
						base.List.Add(item);
					}
					continue;
				}
				if (existingGroup != null)
				{
					if (!Contains(item))
					{
						item._replacedCommand = existingGroup;
						ReplaceGroup(existingGroup, item);
					}
				}
				else
				{
					base.List.Add(item);
				}
			}
			CommandMergeOrderComparer comparer = new CommandMergeOrderComparer(this);
			base.InnerList.Sort(comparer);
			RenumberGroups();
		}

		internal void UndoMerge()
		{
			for (int i = 0; i < base.Count; i++)
			{
				RibbonGroup ribbonGroup = (RibbonGroup)base.List[i];
				if (!ribbonGroup.IsMerged())
				{
					ribbonGroup.SetHiddenByMerge(false);
					ribbonGroup.Commands.UnmergeCommands();
				}
				else if (ribbonGroup._replacedCommand != null)
				{
					if (ribbonGroup.MergeType == CommandMergeType.Hide)
					{
						ribbonGroup._replacedCommand.SetHiddenByMerge(false);
					}
					else
					{
						ReplaceGroup(ribbonGroup, ribbonGroup._replacedCommand as RibbonGroup);
					}
					ribbonGroup._replacedCommand = null;
					if (_ribbonTab != null)
					{
						ribbonGroup.ApplyMergeState(_ribbonTab.Ribbon, false);
					}
					else
					{
						ribbonGroup.SetNullable(false);
					}
					ribbonGroup._ribbon = null;
				}
				else
				{
					Remove(ribbonGroup);
					i--;
					if (_ribbonTab != null)
					{
						ribbonGroup.ApplyMergeState(_ribbonTab.Ribbon, false);
					}
					else
					{
						ribbonGroup.SetNullable(false);
					}
					ribbonGroup._ribbon = null;
				}
			}
			CommandOriginalIndexComparer comparer = new CommandOriginalIndexComparer(this);
			base.InnerList.Sort(comparer);
			RenumberGroups();
		}

		internal void ReplaceGroup(RibbonGroup ribbonGroup, RibbonGroup replacement)
		{
			int index = base.List.IndexOf(ribbonGroup);
			base.List[index] = replacement;
			replacement.SetIndexInOwner(index);
		}
	}
}
