using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	public class CommandCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal ICommandContainer _parent;

		private bool _isQuickAccessCollection;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Command";

		public CommandBase this[int index] => (CommandBase)base.List[index];

		public CommandBase this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (CommandBase item in base.List)
				{
					if (item.Key != null && string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		internal CommandCollection(ICommandContainer parent, bool isQuickAccessCollection)
		{
			_parent = parent;
			_isQuickAccessCollection = isQuickAccessCollection;
		}

		// A command collection is saved with the component that owns it, so a design-time edit of the collection is reported on
		// that component: on its Commands property when that property holds this collection, otherwise on the whole component.
		internal static DesignEditScope BeginDesignEdit(CommandCollection commands, string description)
		{
			object owner = commands?._parent;
			if (!((owner as IComponent)?.Site?.DesignMode ?? false))
			{
				return DesignEditScope.Begin(null, null, description);
			}
			PropertyDescriptor property = TypeDescriptor.GetProperties(owner)["Commands"];
			string memberName = property != null && ReferenceEquals(property.GetValue(owner), commands) ? property.Name : null;
			return DesignEditScope.Begin(owner, memberName, description);
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			DesignEditScope edit = null;
			try
			{
				int commandIndex = 0;
				PropertyValue propertyValue = reader["Command" + commandIndex];
				ArrayList arrayList = new ArrayList();
				while (propertyValue != null)
				{
					LayoutReader layoutReader = propertyValue.GetLayoutReader();
					string text = (string)LayoutReader.GetPropertyValue(layoutReader, "Name", typeof(string), "");
					CommandBase commandBase = null;
					commandBase = ((!_isQuickAccessCollection) ? FindCommandByName(text) : FindCommandInRibbon(text));
					if (commandBase == null)
					{
						string typeName = (string)LayoutReader.GetPropertyValue(layoutReader, "CommandType", typeof(string), "");
						commandBase = LayoutTypeRegistry.CreateCommand(typeName);
					}
					if (commandBase.Container == null)
					{
						if (edit == null && !(_parent is MxRibbon))
						{
							edit = BeginDesignEdit(this, "Load Layout");
						}
						AddComponentToDesignContainer(commandBase, text);
						if (!_isQuickAccessCollection || commandBase.GetParentContainer() == null)
						{
							commandBase.SetParentContainer(_parent);
						}
					}
					propertyValue.DeserializeObject(commandBase);
					arrayList.Add(commandBase);
					commandBase.SetIndexInOwner(commandIndex);
					commandIndex++;
					propertyValue = reader["Command" + commandIndex];
				}
				for (int i = 0; i < base.Count; i++)
				{
					CommandBase existingCommand = (CommandBase)base.List[i];
					if (!arrayList.Contains(existingCommand))
					{
						if (existingCommand.CommandsOwner == this)
						{
							existingCommand.Dispose();
						}
						if (!Contains(existingCommand))
						{
							i--;
						}
					}
				}
				RemoveAllFromInnerList();
				foreach (CommandBase item in arrayList)
				{
					AddToInnerList(item);
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
			int commandIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CommandBase value = (CommandBase)enumerator.Current;
					info.AddValue("Command" + commandIndex, value);
					commandIndex++;
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

		private CommandBase FindCommandInRibbon(string text)
		{
			if (text == null || text.Length == 0)
			{
				return null;
			}
			MxRibbon ribbon = _parent.Ribbon;
			CommandBase commandBase = null;
			if (ribbon != null)
			{
				foreach (RibbonTab tab in ribbon.Tabs)
				{
					foreach (RibbonGroup group in tab.Groups)
					{
						if (group.Name == text)
						{
							return group;
						}
						commandBase = group.Commands.FindCommandByNameRecursive(text);
						if (commandBase != null)
						{
							return commandBase;
						}
					}
				}
				foreach (RibbonContextualTab contextualTab in ribbon.ContextualTabs)
				{
					foreach (RibbonTab contextualChildTab in contextualTab.Tabs)
					{
						foreach (RibbonGroup contextualChildGroup in contextualChildTab.Groups)
						{
							if (contextualChildGroup.Name == text)
							{
								return contextualChildGroup;
							}
							commandBase = contextualChildGroup.Commands.FindCommandByNameRecursive(text);
							if (commandBase != null)
							{
								return commandBase;
							}
						}
					}
				}
				commandBase = ribbon.ControlBoxMenu.LeftCommands.FindCommandByNameRecursive(text);
				if (commandBase != null)
				{
					return commandBase;
				}
				commandBase = ribbon.ControlBoxMenu.RightCommands.FindCommandByNameRecursive(text);
				if (commandBase != null)
				{
					return commandBase;
				}
				commandBase = ribbon.ControlBoxMenu.BottomCommands.FindCommandByNameRecursive(text);
				if (commandBase != null)
				{
					return commandBase;
				}
			}
			return null;
		}

		private CommandBase FindCommandByNameRecursive(string text)
		{
			CommandBase commandBase = null;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CommandBase currentCommand = (CommandBase)enumerator.Current;
					if (currentCommand.Name == text)
					{
						return currentCommand;
					}
					if (currentCommand is ICommandContainer)
					{
						commandBase = ((ICommandContainer)currentCommand).Commands.FindCommandByNameRecursive(text);
						if (commandBase != null)
						{
							return commandBase;
						}
					}
				}
				return commandBase;
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

		public void Add(CommandBase command)
		{
			if (CanContainCommand(command))
			{
				base.List.Add(command);
				return;
			}
			throw new InvalidOperationException("Commands of type " + command.CommandType + " cannot be added to this collection");
		}

		public void AddRange(CommandBase[] commands)
		{
			foreach (CommandBase commandBase in commands)
			{
				if (commandBase != null)
				{
					Add(commandBase);
				}
			}
		}

		public void Remove(CommandBase command)
		{
			base.List.Remove(command);
		}

		internal void AddToInnerList(CommandBase commandBase)
		{
			base.InnerList.Add(commandBase);
			if (_parent == null)
			{
				return;
			}
			RefreshCommandIndexes();
			if (!commandBase.IsMerged())
			{
				if (commandBase.GetParentContainer() == null || (commandBase.GetParentContainer() is MxRibbon && commandBase.CommandsOwner == null))
				{
					commandBase.SetParentContainer(_parent);
					commandBase.CommandsOwner = this;
				}
				else if (commandBase.GetParentContainer() is MxRibbonStatusBar)
				{
					commandBase.SetParentContainer(_parent);
					commandBase.CommandsOwner = this;
				}
				else if (commandBase.GetParentContainer() is MxRibbon && commandBase.CommandsOwner != null && commandBase.CommandsOwner._isQuickAccessCollection)
				{
					commandBase.SetParentContainer(_parent);
					commandBase.CommandsOwner = this;
				}
			}
		}

		internal void RemoveFromInnerList(CommandBase command)
		{
			if (Contains(command))
			{
				base.InnerList.Remove(command);
				if (_parent != null)
				{
					RefreshCommandIndexes();
				}
			}
		}

		internal void RemoveFromInnerListAndClearOwner(CommandBase command)
		{
			if (Contains(command))
			{
				base.InnerList.Remove(command);
				if (_parent != null)
				{
					command.SetParentContainer(null);
					command.CommandsOwner = null;
					RefreshCommandIndexes();
				}
			}
		}

		internal void RemoveAllAndClearOwners()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				CommandBase commandBase = (CommandBase)base.List[index];
				RemoveFromInnerListAndClearOwner(commandBase);
				index--;
			}
		}

		internal void RemoveAllFromInnerList()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				CommandBase commandBase = (CommandBase)base.List[index];
				RemoveFromInnerList(commandBase);
				index--;
			}
		}

		internal void DisposeAll()
		{
			CommandBase[] snapshot = new CommandBase[base.Count];
			base.InnerList.CopyTo(snapshot, 0);
			foreach (CommandBase command in snapshot)
			{
				command.Dispose();
				if (Contains(command))
				{
					Remove(command);
				}
			}
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				CommandBase command = (CommandBase)base.List[index];
				Remove(command);
				index--;
			}
		}

		public void Insert(int index, CommandBase command)
		{
			if (index < 0)
			{
				index = 0;
			}
			if (index > base.Count)
			{
				index = base.Count;
			}
			if (CanContainCommand(command))
			{
				base.List.Insert(index, command);
			}
		}

		internal void MoveCommandTo(int index, CommandBase command)
		{
			if (Contains(command))
			{
				base.InnerList.Remove(command);
				base.InnerList.Insert(Math.Max(0, Math.Min(index, base.InnerList.Count)), command);
				if (_parent != null)
				{
					RefreshCommandIndexes();
					_parent.CommandAdded(command);
				}
			}
		}

		public bool Contains(CommandBase command)
		{
			return base.List.Contains(command);
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

		public int IndexOf(CommandBase command)
		{
			return base.List.IndexOf(command);
		}

		internal CommandBase FindCommandByName(string strA)
		{
			if (strA.Length == 0)
			{
				return null;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (CommandBase item in base.List)
			{
				if (string.Compare(strA, item.Name, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		private void RefreshCommandIndexes()
		{
			for (int i = 0; i < base.Count; i++)
			{
				CommandBase commandBase = (CommandBase)base.List[i];
				if (commandBase.CommandsOwner == this)
				{
					commandBase.SetIndexInOwner(i);
				}
			}
		}

		protected override void OnInsertComplete(int index, object value)
		{
			CommandBase commandBase = (CommandBase)value;
			if (_parent == null)
			{
				return;
			}
			if (!commandBase.IsMerged())
			{
				if (commandBase.GetParentContainer() == null || (commandBase.GetParentContainer() is MxRibbon && commandBase.CommandsOwner == null))
				{
					commandBase.SetParentContainer(_parent);
					commandBase.CommandsOwner = this;
				}
				else if (commandBase.GetParentContainer() is MxRibbonStatusBar)
				{
					commandBase.SetParentContainer(_parent);
					commandBase.CommandsOwner = this;
				}
				else if (commandBase.GetParentContainer() is MxRibbon && commandBase.CommandsOwner != null && commandBase.CommandsOwner._isQuickAccessCollection)
				{
					commandBase.SetParentContainer(_parent);
					commandBase.CommandsOwner = this;
				}
			}
			RefreshCommandIndexes();
			_parent.CommandAdded(commandBase);
			if (_isQuickAccessCollection && _parent is MxRibbon)
			{
				((MxRibbon)_parent).NotifyQuickAccessCommandAdded(commandBase, this);
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			if (_parent != null)
			{
				RefreshCommandIndexes();
				CommandBase commandBase = (CommandBase)value;
				if (!commandBase.IsMerged() && commandBase.CommandsOwner == this)
				{
					commandBase.SetParentContainer(null);
					commandBase.CommandsOwner = null;
				}
				_parent.CommandRemoved(commandBase);
				if (_isQuickAccessCollection && _parent is MxRibbon)
				{
					((MxRibbon)_parent).NotifyQuickAccessCommandRemoved(commandBase, this);
				}
			}
		}

		internal bool CanContainCommand(CommandBase commandBase)
		{
			if (_parent != null)
			{
				if (_parent.CanBeParentTo(commandBase.CommandType))
				{
					return true;
				}
				return false;
			}
			return true;
		}

		internal CommandCollection CloneWithParent(ICommandContainer parent)
		{
			CommandCollection commandCollection = new CommandCollection(parent, _isQuickAccessCollection);
			foreach (CommandBase item in base.List)
			{
				CommandBase command = (CommandBase)item.Clone();
				commandCollection.Add(command);
			}
			return commandCollection;
		}

		internal CommandBase FindCommandByKey(string strA)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (CommandBase item in base.List)
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
					if (commandBase.CommandsOwner == this)
					{
						commandBase.SetOriginalIndex(IndexOf(commandBase));
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

		internal void MergeCommands(CommandCollection commandCollection)
		{
			DisposeResources();
			commandCollection.DisposeResources();
			foreach (CommandBase item in commandCollection)
			{
				CommandMergeType mergeType = item.MergeType;
				CommandBase existingCommand = FindCommandByKey(item.Key);
				if (_parent != null)
				{
					if (_parent is MxRibbon || _parent is BackstageMenu)
					{
						item.SetNullable(true);
					}
					else
					{
						item.ApplyMergeState(_parent.Ribbon, true);
					}
				}
				else
				{
					item.SetNullable(true);
				}
				item._ribbon = _parent?.Ribbon;
				switch (mergeType)
				{
				case CommandMergeType.Add:
					base.List.Add(item);
					continue;
				case CommandMergeType.Hide:
					if (existingCommand != null && !Contains(item))
					{
						existingCommand.SetHiddenByMerge(true);
						item._replacedCommand = existingCommand;
					}
					continue;
				case CommandMergeType.MergeItems:
					if (existingCommand != null)
					{
						if (existingCommand.CommandType == item.CommandType)
						{
							switch (existingCommand.CommandType)
							{
							case CommandType.Gallery:
								((Gallery)existingCommand).Commands.MergeCommands(((Gallery)item).Commands);
								((Gallery)existingCommand).GalleryItems.MergeItems(((Gallery)item).GalleryItems);
								break;
							case CommandType.DropDownButton:
								((DropDownCommand)existingCommand).Commands.MergeCommands(((DropDownCommand)item).Commands);
								break;
							case CommandType.Box:
								((CommandBox)existingCommand).Commands.MergeCommands(((CommandBox)item).Commands);
								break;
							case CommandType.ButtonGroup:
								((ButtonGroup)existingCommand).Commands.MergeCommands(((ButtonGroup)item).Commands);
								break;
							}
						}
					}
					else
					{
						base.List.Add(item);
					}
					continue;
				}
				if (existingCommand != null)
				{
					if (!Contains(item))
					{
						item._replacedCommand = existingCommand;
						ReplaceCommand(existingCommand, item);
					}
				}
				else
				{
					base.List.Add(item);
				}
			}
			CommandMergeOrderComparer comparer = new CommandMergeOrderComparer(this);
			base.InnerList.Sort(comparer);
			RefreshCommandIndexes();
		}

		internal void UnmergeCommands()
		{
			for (int i = 0; i < base.Count; i++)
			{
				CommandBase commandBase = (CommandBase)base.List[i];
				if (!commandBase.IsMerged())
				{
					commandBase.SetHiddenByMerge(false);
					switch (commandBase.CommandType)
					{
					case CommandType.ButtonGroup:
						((ButtonGroup)commandBase).Commands.UnmergeCommands();
						break;
					case CommandType.Box:
						((CommandBox)commandBase).Commands.UnmergeCommands();
						break;
					case CommandType.DropDownButton:
						((DropDownCommand)commandBase).Commands.UnmergeCommands();
						break;
					case CommandType.Gallery:
						((Gallery)commandBase).Commands.UnmergeCommands();
						((Gallery)commandBase).GalleryItems.UndoMerge();
						break;
					}
					continue;
				}
				if (commandBase._replacedCommand != null)
				{
					if (commandBase.MergeType == CommandMergeType.Hide)
					{
						commandBase._replacedCommand.SetHiddenByMerge(false);
					}
					else
					{
						ReplaceCommand(commandBase, commandBase._replacedCommand);
					}
					if (!_isQuickAccessCollection)
					{
						commandBase._replacedCommand = null;
						if (_parent != null)
						{
							commandBase.ApplyMergeState(_parent.Ribbon, false);
						}
						else
						{
							commandBase.SetNullable(false);
						}
						commandBase._ribbon = null;
					}
					continue;
				}
				Remove(commandBase);
				i--;
				if (!_isQuickAccessCollection)
				{
					if (_parent != null)
					{
						commandBase.ApplyMergeState(_parent.Ribbon, false);
					}
					else
					{
						commandBase.SetNullable(false);
					}
					commandBase._ribbon = null;
				}
			}
			CommandOriginalIndexComparer comparer = new CommandOriginalIndexComparer(this);
			base.InnerList.Sort(comparer);
			RefreshCommandIndexes();
		}

		internal void ReplaceCommand(CommandBase commandBase, CommandBase replacement)
		{
			int index = base.List.IndexOf(commandBase);
			base.List[index] = replacement;
			replacement.SetIndexInOwner(index);
		}
	}
}
