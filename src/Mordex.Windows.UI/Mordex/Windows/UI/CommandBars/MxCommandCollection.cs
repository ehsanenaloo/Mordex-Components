using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public class MxCommandCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private bool _isMergedCollection;

		internal bool _isMerging;

		internal MxCommandCollection _mergedCommands;

		private bool _shouldUpdatePositions = true;

		internal MxCommandCollection _originalCommands;

		internal IAccessibleCommandBarsElement _owner;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Command";

		public MxCommand this[int index] => (MxCommand)base.List[index];

		public MxCommand this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (MxCommand item in base.List)
				{
					if (string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' key in the collection.");
			}
		}

		internal void DeserializeMergedCommands(PropertyValue parentValue)
		{
			LayoutReader parentReader = parentValue.GetLayoutReader();
			PropertyValue mergedValue = parentReader["MergedCommands"];
			if (_mergedCommands != null)
			{
				_mergedCommands.InnerList.Clear();
				_mergedCommands._owner = _owner;
			}
			if (mergedValue == null)
			{
				return;
			}
			if (_mergedCommands == null)
			{
				_mergedCommands = new MxCommandCollection(_owner, mergedCollection: true);
			}
			LayoutReader mergedReader = mergedValue.GetLayoutReader();
			int commandIndex = 0;
			for (PropertyValue commandValue = mergedReader["Command" + commandIndex]; commandValue != null; commandValue = mergedReader["Command" + commandIndex])
			{
				MxCommand command = new MxCommand();
				commandValue.DeserializeObject(command);
				if (_owner.GetCommandManager() != null)
				{
					command.UpdateMergeState(_owner.GetCommandManager().Id, true);
				}
				else
				{
					command.SetIsMergedCommand(true);
				}
				_mergedCommands.InnerList.Add(command);
				commandIndex++;
				command.ApplyLayoutState(commandValue, _owner.GetCommandManager());
			}
		}

		internal void DeserializeOriginalCommands(PropertyValue parentValue)
		{
			LayoutReader parentReader = parentValue.GetLayoutReader();
			PropertyValue originalValue = parentReader["OriginalCommands"];
			if (_originalCommands != null)
			{
				_originalCommands.InnerList.Clear();
				_originalCommands._owner = _owner;
			}
			if (originalValue != null)
			{
				if (_originalCommands == null)
				{
					_originalCommands = new MxCommandCollection(_owner);
				}
				LayoutReader originalReader = originalValue.GetLayoutReader();
				int commandIndex = 0;
				for (PropertyValue commandValue = originalReader["Command" + commandIndex]; commandValue != null; commandValue = originalReader["Command" + commandIndex])
				{
					MxCommand command = new MxCommand();
					commandValue.DeserializeObject(command);
					_originalCommands.InnerList.Add(command);
					command.SetLayoutIndex(commandIndex);
					commandIndex++;
					command.ApplyLayoutState(commandValue, _owner.GetCommandManager());
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader reader)
		{
			int commandIndex = 0;
			PropertyValue commandValue = reader["Command" + commandIndex];
			ArrayList loadedCommands = new ArrayList();
			while (commandValue != null)
			{
				LayoutReader commandReader = commandValue.GetLayoutReader();
				string commandName = (string)LayoutReader.GetPropertyValue(commandReader, "Name", typeof(string), "");
				MxCommand command = FindByName(commandName);
				if (command == null)
				{
					command = new MxCommand();
				}
				commandValue.DeserializeObject(command);
				loadedCommands.Add(command);
				command.SetLayoutIndex(commandIndex);
				commandIndex++;
				commandValue = reader["Command" + commandIndex];
			}
			IAccessibleCommandBarsElement owner = _owner;
			DetachAllAndReset();
			_owner = owner;
			foreach (MxCommand item in loadedCommands)
			{
				AddKeepingPositions(item);
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
					MxCommand command = (MxCommand)enumerator.Current;
					if (!_isMergedCollection && _owner is MxCommandManager)
					{
						info.AddValue(command.Key, command);
					}
					else
					{
						info.AddValue("Command" + commandIndex, command);
					}
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
			if (_originalCommands != null)
			{
				info.AddValue("OriginalCommands", _originalCommands);
			}
			if (_mergedCommands != null)
			{
				info.AddValue("MergedCommands", _mergedCommands);
			}
		}

		internal MxCommandCollection(IAccessibleCommandBarsElement owner, bool mergedCollection)
		{
			_owner = owner;
			_isMergedCollection = mergedCollection;
		}

		internal MxCommandCollection(IAccessibleCommandBarsElement owner)
		{
			_owner = owner;
		}

		private void AssignCommandIndexes()
		{
			for (int i = 0; i < base.List.Count; i++)
			{
				MxCommand uiCommand = (MxCommand)base.List[i];
				uiCommand.SetListIndex(i);
			}
		}

		internal void UpdateMergeStates(Guid guid)
		{
			foreach (MxCommand item in base.List)
			{
				if (item.BaseCommand == null)
				{
					item.Commands.UpdateMergeStates(guid);
				}
				if (item.IsMergedCommand)
				{
					item.UpdateMergeState(guid, false);
				}
			}
		}

		internal void MergeCommands(MxCommandCollection incomingCommands, MxCommandManager manager)
		{
			if (_isMerging)
			{
				return;
			}
			_isMerging = true;
			MxCommandCollection mergedResult = new MxCommandCollection(_owner);
			if (_mergedCommands != null)
			{
				foreach (MxCommand item in _mergedCommands)
				{
					MxCommand managerCommand = manager.Commands.FindByKey(item.Key);
					if (managerCommand != null && !item.IsSeparator())
					{
						item.SetBaseCommand(managerCommand);
						item.UpdateMergeState(_owner.GetCommandManager().Id, true);
						mergedResult.InnerList.Add(item);
					}
					else if (item.IsSeparator() && incomingCommands == null)
					{
						item.UpdateMergeState(_owner.GetCommandManager().Id, true);
						mergedResult.InnerList.Add(item);
					}
				}
			}
			else
			{
				if (incomingCommands == null || incomingCommands.Count <= 0)
				{
					_isMerging = false;
					return;
				}
				foreach (MxCommand incoming in incomingCommands)
				{
					UpdateMergedCommands(incoming);
				}
			}
			if (incomingCommands != null)
			{
				foreach (MxCommand incomingCommand in incomingCommands)
				{
					MxCommand matchingCommand = mergedResult.FindByKey(incomingCommand.Key);
					if (matchingCommand == null || incomingCommand.IsSeparator())
					{
						if (incomingCommand.IsSeparator() && incomingCommand._isMergedCommand && Contains(incomingCommand))
						{
							_shouldUpdatePositions = false;
							Remove(incomingCommand);
							_shouldUpdatePositions = true;
						}
						incomingCommand.UpdateMergeState(_owner.GetCommandManager().Id, true);
						int incomingIndex = incomingCommands.IndexOf(incomingCommand);
						if (incomingIndex < 0 || incomingIndex >= mergedResult.Count)
						{
							mergedResult.InnerList.Add(incomingCommand);
						}
						else
						{
							mergedResult.InnerList.Insert(incomingCommands.IndexOf(incomingCommand), incomingCommand);
						}
					}
					else
					{
						incomingCommand.UpdateMergeState(_owner.GetCommandManager().Id, true);
						mergedResult.ReplaceCommand(matchingCommand, incomingCommand);
					}
				}
			}
			AssignCommandIndexes();
			_shouldUpdatePositions = false;
			foreach (MxCommand resultCommand in mergedResult)
			{
				CommandMergeType commandMergeType = resultCommand.MergeType;
				MxCommand ownCommand = FindByKey(resultCommand.Key);
				if (ownCommand != null)
				{
					if (commandMergeType == CommandMergeType.Default)
					{
						commandMergeType = ownCommand.MergeType;
					}
				}
				else
				{
					resultCommand.UpdateMergeState(_owner.GetCommandManager().Id, true);
				}
				if (resultCommand.IsSeparator())
				{
					commandMergeType = CommandMergeType.Add;
					resultCommand.UpdateMergeState(_owner.GetCommandManager().Id, true);
				}
				switch (commandMergeType)
				{
				case CommandMergeType.Add:
					base.List.Add(resultCommand);
					continue;
				case CommandMergeType.Hide:
					if (ownCommand != null && !Contains(resultCommand))
					{
						ownCommand.SetHiddenByMerge(true);
						resultCommand.SetMergeTarget(ownCommand);
					}
					continue;
				case CommandMergeType.MergeItems:
					if (ownCommand != null)
					{
						ownCommand.Commands.MergeCommands(resultCommand.Commands, manager);
					}
					else
					{
						base.List.Add(resultCommand);
					}
					continue;
				}
				if (ownCommand != null)
				{
					if (!Contains(resultCommand))
					{
						resultCommand.SetMergeTarget(ownCommand);
						ReplaceCommand(ownCommand, resultCommand);
					}
				}
				else
				{
					base.List.Add(resultCommand);
				}
			}
			_shouldUpdatePositions = true;
			CommandMergeOrderComparer firstPassComparer = new CommandMergeOrderComparer(this);
			base.InnerList.Sort(firstPassComparer);
			AssignCommandOrder();
			CommandPositionMergeOrderComparer secondPassComparer = new CommandPositionMergeOrderComparer(this);
			base.InnerList.Sort(secondPassComparer);
			AssignCommandOrder();
			CommandInsertionComparer thirdPassComparer = new CommandInsertionComparer(this);
			base.InnerList.Sort(thirdPassComparer);
		}

		internal void SortCommands()
		{
			CommandPositionMergeOrderComparer comparer = new CommandPositionMergeOrderComparer(this);
			base.InnerList.Sort(comparer);
			AssignCommandOrder();
		}

		internal ArrayList GetCommandsSortedByAlignment()
		{
			CommandAlignmentComparer comparer = new CommandAlignmentComparer();
			ArrayList arrayList = new ArrayList();
			foreach (MxCommand item in base.List)
			{
				arrayList.Add(item);
			}
			arrayList.Sort(comparer);
			return arrayList;
		}

		public int IndexOf(MxCommand cmd)
		{
			return base.List.IndexOf(cmd);
		}

		public MxCommand GetDefaultCommand()
		{
			foreach (MxCommand item in base.List)
			{
				if (item.IsDefaultItem())
				{
					return item;
				}
			}
			return null;
		}

		internal MxCommandCollection GetUnmergedCommands()
		{
			_isMerging = false;
			MxCommandCollection uiCommandCollection = new MxCommandCollection(_owner);
			uiCommandCollection._shouldUpdatePositions = false;
			for (int i = 0; i < base.Count; i++)
			{
				MxCommand uiCommand = (MxCommand)base.List[i];
				if (!uiCommand.IsMergedCommand)
				{
					uiCommand.SetHiddenByMerge(false);
					uiCommandCollection.Add(uiCommand);
				}
				else if (uiCommand.GetMergeTarget() != null)
				{
					if (uiCommand.GetMergeTarget().CommandManager != null && uiCommand.GetMergeTarget().CommandManager.Id == _owner.GetCommandManager().Id)
					{
						if (uiCommand.MergeType == CommandMergeType.Hide)
						{
							uiCommand.GetMergeTarget().SetHiddenByMerge(false);
						}
						else
						{
							uiCommandCollection.Add(uiCommand.GetMergeTarget());
						}
						uiCommand.SetMergeTarget(null);
					}
					uiCommand.UpdateMergeState(_owner.GetCommandManager().Id, false);
				}
				else
				{
					uiCommand.UpdateMergeState(_owner.GetCommandManager().Id, false);
				}
			}
			uiCommandCollection._shouldUpdatePositions = false;
			uiCommandCollection._mergedCommands = _mergedCommands;
			CommandPositionComparer comparer = new CommandPositionComparer(this);
			uiCommandCollection.InnerList.Sort(comparer);
			uiCommandCollection.AssignCommandOrder();
			if (_mergedCommands != null && _owner != null && _owner.GetCommandManager() != null && !_owner.GetCommandManager().KeepMergeSettings)
			{
				_mergedCommands.DetachAllAndClear();
				_mergedCommands = null;
			}
			return uiCommandCollection;
		}

		internal void UpdateOwner()
		{
			if (_originalCommands == null)
			{
				return;
			}
			if (_owner is MxCommandBar)
			{
				((MxCommandBar)_owner).Controls.Clear();
			}
			DetachAllAndReset();
			_owner = _originalCommands._owner;
			if (_owner.GetCommandManager() != null)
			{
				_owner.GetCommandManager()._isBatchUpdating = true;
			}
			foreach (MxCommand item in _originalCommands)
			{
				AddKeepingPositions(item);
			}
			if (_owner.GetCommandManager() != null)
			{
				_owner.GetCommandManager()._isBatchUpdating = false;
				_owner.GetCommandManager().RefreshRebars();
			}
		}

		internal void SetOriginalCommands()
		{
			_originalCommands = CloneCommands();
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				MxCommand command = (MxCommand)base.List[index];
				Remove(command);
				index--;
			}
		}

		internal void DetachAllAndClear()
		{
			foreach (MxCommand item in base.List)
			{
				DetachCommandHandlers(item);
			}
			base.InnerList.Clear();
		}

		internal MxCommandCollection CloneCommands()
		{
			MxCommandCollection uiCommandCollection = new MxCommandCollection(_owner);
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxCommand uiCommand = (MxCommand)enumerator.Current;
					MxCommand value = uiCommand.Clone();
					uiCommandCollection.InnerList.Add(value);
				}
				return uiCommandCollection;
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

		public void AddRange(MxCommand[] commands)
		{
			foreach (MxCommand command in commands)
			{
				Add(command);
			}
		}

		internal void UpdateMergedCommands(MxCommand command)
		{
			if (_mergedCommands == null)
			{
				_mergedCommands = new MxCommandCollection(_owner, mergedCollection: true);
			}
			if (!_mergedCommands.Contains(command))
			{
				_mergedCommands.InnerList.Add(command);
			}
		}

		public MxCommand AddSeparator()
		{
			MxCommand uiCommand = new MxCommand();
			uiCommand.CommandType = CommandType.Separator;
			Add(uiCommand);
			return uiCommand;
		}

		public void Add(MxCommand command)
		{
			if (Contains(command))
			{
				throw new InvalidOperationException("The item is already in the collection");
			}
			if (_owner is MxCommand)
			{
				MxCommand ownerCommand = (MxCommand)_owner;
				if (ownerCommand == command.BaseCommand || ownerCommand == command)
				{
					throw new InvalidOperationException("A command can not be child of its base command");
				}
				if (ownerCommand != null)
				{
					foreach (MxCommand childCommand in command.Commands)
					{
						if (childCommand.BaseCommand == ownerCommand)
						{
							throw new InvalidOperationException("The command cannot be added because command '" + ownerCommand.Key + "' is already a child of command '" + command.Key + "'.");
						}
					}
				}
			}
			if (command.IsMergedCommand && !(_owner is MxContextMenu))
			{
				UpdateMergedCommands(command);
			}
			else if (!_owner.IsInitializing)
			{
				if (_owner is MxCommandManager)
				{
					if (!((MxCommandManager)_owner).IsCommandKeyUnique(command, command.Key))
					{
						throw new ArgumentException(string.Format("A Command with key '{0}' already exist.", command.Key));
					}
					if (command.BaseCommand != null)
					{
						throw new InvalidOperationException("A command with a BaseCommand can not be added to the UICommandManager");
					}
					foreach (MxCommand managerChild in command.Commands)
					{
						if (managerChild.BaseCommand == null)
						{
							if (managerChild.CommandType == CommandType.Separator)
							{
								managerChild.SetBaseCommand(_owner.GetCommandManager().GetSeparatorCommand());
							}
						}
						else if (managerChild.BaseCommand.CommandManager == null)
						{
							Add(managerChild.BaseCommand);
						}
					}
				}
				else if (command.BaseCommand != null)
				{
					if (command.BaseCommand.CommandManager == null && _owner.GetCommandManager() != null)
					{
						_owner.GetCommandManager().Commands.Add(command.BaseCommand);
					}
				}
				else if (_owner.GetCommandManager() != null)
				{
					if (command.CommandType == CommandType.Separator)
					{
						command.SetBaseCommand(_owner.GetCommandManager().GetSeparatorCommand());
					}
					else
					{
						if (command.Key.Length <= 0)
						{
							MxCommand copyWithoutKey = new MxCommand(command);
							Add(copyWithoutKey);
							return;
						}
						MxCommand existingCommand = _owner.GetCommandManager().Commands.FindByKey(command.Key);
						if (existingCommand == null || existingCommand == command)
						{
							MxCommand copyOfDuplicate = new MxCommand(command);
							Add(copyOfDuplicate);
							return;
						}
						command.SetBaseCommand(existingCommand);
					}
				}
			}
			base.List.Add(command);
		}

		internal void AddKeepingPositions(MxCommand uiCommand)
		{
			_shouldUpdatePositions = false;
			base.List.Add(uiCommand);
			_shouldUpdatePositions = true;
		}

		internal void AddWithoutEvents(MxCommand uiCommand)
		{
			base.InnerList.Add(uiCommand);
		}

		internal void DetachAll()
		{
			foreach (MxCommand item in base.List)
			{
				DetachCommandHandlers(item);
			}
		}

		internal void DetachAllAndReset()
		{
			foreach (MxCommand item in base.List)
			{
				DetachCommandHandlers(item);
			}
			_owner = null;
			_mergedCommands = null;
			base.InnerList.Clear();
		}

		public void Insert(int index, MxCommand command)
		{
			if (Contains(command))
			{
				throw new InvalidOperationException("The item is already in the collection");
			}
			if (_owner is MxCommand)
			{
				MxCommand ownerCommand = (MxCommand)_owner;
				if (ownerCommand == command.BaseCommand || ownerCommand == command)
				{
					throw new InvalidOperationException("A command can not be child of its base command");
				}
				if (ownerCommand != null)
				{
					foreach (MxCommand childCommand in command.Commands)
					{
						if (childCommand.BaseCommand == ownerCommand)
						{
							throw new InvalidOperationException("The command cannot be added because command '" + ownerCommand.Key + "' is already a child of command '" + command.Key + "'.");
						}
					}
				}
			}
			if (command.IsMergedCommand && !(_owner is MxContextMenu))
			{
				UpdateMergedCommands(command);
			}
			else if (!_owner.IsInitializing)
			{
				if (_owner is MxCommandManager)
				{
					if (!((MxCommandManager)_owner).IsCommandKeyUnique(command, command.Key))
					{
						throw new ArgumentException(string.Format("A Command with key '{0}' already exist.", command.Key));
					}
					if (command.BaseCommand != null)
					{
						throw new InvalidOperationException("A command with a BaseCommand can not be added to the UICommandManager");
					}
					foreach (MxCommand managerChild in command.Commands)
					{
						if (managerChild.BaseCommand == null)
						{
							if (managerChild.CommandType == CommandType.Separator)
							{
								managerChild.SetBaseCommand(_owner.GetCommandManager().GetSeparatorCommand());
							}
						}
						else if (managerChild.BaseCommand.CommandManager == null)
						{
							Add(managerChild.BaseCommand);
						}
					}
				}
				else if (command.BaseCommand != null)
				{
					if (command.BaseCommand.CommandManager == null && _owner.GetCommandManager() != null)
					{
						_owner.GetCommandManager().Commands.Add(command.BaseCommand);
					}
				}
				else if (_owner.GetCommandManager() != null)
				{
					if (command.CommandType == CommandType.Separator)
					{
						command.SetBaseCommand(_owner.GetCommandManager().GetSeparatorCommand());
					}
					else
					{
						if (command.Key.Length <= 0)
						{
							MxCommand copyWithoutKey = new MxCommand(command);
							Insert(index, copyWithoutKey);
							return;
						}
						MxCommand existingCommand = _owner.GetCommandManager().Commands.FindByKey(command.Key);
						if (existingCommand == null || existingCommand == command)
						{
							MxCommand copyOfDuplicate = new MxCommand(command);
							Insert(index, copyOfDuplicate);
							return;
						}
						command.SetBaseCommand(existingCommand);
					}
				}
			}
			if (index > base.List.Count)
			{
				index = base.List.Count;
			}
			base.List.Insert(index, command);
		}

		internal void ReplaceCommand(MxCommand oldCommand, MxCommand newCommand)
		{
			DetachCommandHandlers(oldCommand);
			int index = base.List.IndexOf(oldCommand);
			base.List[index] = newCommand;
			AttachCommandHandlers(newCommand);
		}

		public void Remove(MxCommand command)
		{
			base.List.Remove(command);
		}

		public new void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		public bool Contains(MxCommand command)
		{
			return base.List.Contains(command);
		}

		public bool Contains(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxCommand uiCommand = (MxCommand)enumerator.Current;
					if (string.Compare(uiCommand.Key, key, ignoreCase: true, invariantCulture) == 0)
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

		internal MxCommand FindByName(string name)
		{
			if (name.Length == 0)
			{
				return null;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxCommand item in base.List)
			{
				if (string.Compare(name, item.Name, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		internal MxCommand FindByKey(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxCommand item in base.List)
			{
				if (string.Compare(key, item.Key, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		protected override void OnInsertComplete(int index, object value)
		{
			MxCommand uiCommand = (MxCommand)value;
			uiCommand.SetHostElement(_owner);
			if (_owner is MxCommandManager)
			{
				uiCommand._commandManager = (MxCommandManager)_owner;
			}
			AttachCommandHandlers(uiCommand);
			MxCommand parentCommand = null;
			if (_owner is MxCommand)
			{
				parentCommand = _owner as MxCommand;
			}
			AssignCommandOrder();
			CommandActionEventArgs e = new CommandActionEventArgs(uiCommand, parentCommand);
			_owner.OnCommandAdded(e);
			if (uiCommand.Container == null && _owner.OwnerSite != null && _owner.OwnerSite.Container != null)
			{
				string text = CreateSiteName(uiCommand);
				MxCommandManager commandManager = _owner.GetCommandManager();
				if (commandManager == null || commandManager.IsInDesignMode())
				{
					if (text.Length > 0)
					{
						_owner.OwnerSite.Container.Add(uiCommand, text);
					}
					else
					{
						_owner.OwnerSite.Container.Add(uiCommand);
					}
				}
			}
			if (uiCommand.Site != null && uiCommand.Site.Name != null && uiCommand.Site.Name.Length > 0)
			{
				uiCommand.Name = uiCommand.Site.Name;
			}
			ShiftPositions(index, uiCommand, false);
		}

		private string CreateSiteName(MxCommand command)
		{
			string baseName = command.Key;
			if (baseName.Length == 0)
			{
				baseName = (command.IsSeparator() ? "Separator" : ((command.Name.Length <= 0) ? "Command" : command.Name));
			}
			string candidateName = baseName;
			INameCreationService nameCreationService = (INameCreationService)_owner.OwnerSite.GetService(typeof(INameCreationService));
			if (nameCreationService != null)
			{
				if (char.IsNumber(baseName, 0))
				{
					return nameCreationService.CreateName(_owner.OwnerSite.Container, typeof(MxCommand));
				}
				for (int i = 0; i < baseName.Length; i++)
				{
					if (!char.IsLetterOrDigit(baseName, i))
					{
						string character = baseName.Substring(i, 1);
						if (character != "_")
						{
							return nameCreationService.CreateName(_owner.OwnerSite.Container, typeof(MxCommand));
						}
					}
				}
				if (baseName.IndexOf(" ", 0, baseName.Length) != -1)
				{
					return nameCreationService.CreateName(_owner.OwnerSite.Container, typeof(MxCommand));
				}
				if (baseName.IndexOf("&", 0, baseName.Length) != -1)
				{
					return nameCreationService.CreateName(_owner.OwnerSite.Container, typeof(MxCommand));
				}
				if (baseName.IndexOf(".", 0, baseName.Length) != -1)
				{
					return nameCreationService.CreateName(_owner.OwnerSite.Container, typeof(MxCommand));
				}
				int suffix = 1;
				if (!(_owner is MxCommandManager))
				{
					candidateName += "1";
				}
				try
				{
					while (!nameCreationService.IsValidName(candidateName))
					{
						suffix++;
						candidateName = baseName + suffix;
					}
				}
				catch (Exception ex) when (!(ex is OutOfMemoryException))
				{
					// The designer name service is outside our control. The manager check below still makes the name unique.
				}
				while (!_owner.GetCommandManager().IsCommandNameAvailable(command, candidateName))
				{
					suffix++;
					candidateName = baseName + suffix;
				}
				return candidateName;
			}
			return "";
		}

		internal bool IsNameAvailable(MxCommand excludedCommand, string name, bool includeChildren)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxCommand other = (MxCommand)enumerator.Current;
					if (excludedCommand != other)
					{
						if (string.Compare(other.Name, name, ignoreCase: true) == 0)
						{
							return false;
						}
						if (includeChildren && !other.Commands.IsNameAvailable(excludedCommand, name, false))
						{
							return false;
						}
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
			return true;
		}

		internal void ReleaseCommandsFromContainer()
		{
			foreach (MxCommand item in base.List)
			{
				item.RemoveCommandDisposedHandler(OnCommandDisposed);
				if (item.Container != null)
				{
					item.Container.Remove(item);
				}
			}
		}

		internal void AttachDisposeHandlers()
		{
			foreach (MxCommand item in base.List)
			{
				item.AddCommandDisposedHandler(OnCommandDisposed);
			}
		}

		private void AttachCommandHandlers(MxCommand uiCommand)
		{
			uiCommand.Click += OnControlClick;
			uiCommand.ControlValueChanged += OnControlValueChanged;
			uiCommand.MouseHover += OnControlMouseHover;
			uiCommand.MouseLeave += OnControlMouseLeave;
			uiCommand.Popup += OnControlPopup;
			uiCommand.CloseUp += OnControlCloseUp;
			uiCommand.Closing += OnControlClosing;
			uiCommand.AddInvalidatedHandler(HandleInvalidateCommand);
			if (!uiCommand.IsMergedCommand)
			{
				uiCommand.AddCommandDisposedHandler(OnCommandDisposed);
			}
			if (!(_owner is MxCommandManager))
			{
				uiCommand.AddCommandClickedHandler(OnControlClick);
				uiCommand.AddCommandHoverHandler(OnControlMouseHover);
				uiCommand.AddCommandMouseLeaveHandler(OnControlMouseLeave);
				uiCommand.AddCommandPopupHandler(OnControlPopup);
				uiCommand.AddCommandCloseUpHandler(OnControlCloseUp);
				uiCommand.RemoveCommandClosingHandler(OnControlClosing);
				uiCommand.AddCommandControlValueChangedHandler(OnControlValueChanged);
				uiCommand.AddExpandRequestedHandler(HandleCommandExpanded);
				uiCommand.AddCollapseRequestedHandler(HandleCommandCollapsed);
				uiCommand.AddCommandExpandedHandler(HandleCommandExpanded);
				uiCommand.AddCommandCollapsedHandler(HandleCommandCollapsed);
			}
			uiCommand.AddCommandAddedHandler(HandleCommandAdded);
			uiCommand.AddCommandRemovedHandler(HandleCommandRemoved);
		}

		private void DetachCommandHandlers(MxCommand command)
		{
			command.Click -= OnControlClick;
			command.ControlValueChanged -= OnControlValueChanged;
			command.MouseHover -= OnControlMouseHover;
			command.MouseLeave -= OnControlMouseLeave;
			command.Popup -= OnControlPopup;
			command.CloseUp -= OnControlCloseUp;
			command.Closing -= OnControlClosing;
			command.RemoveCommandClickedHandler(OnControlClick);
			command.RemoveCommandExpandedHandler(HandleCommandExpanded);
			command.RemoveCommandCollapsedHandler(HandleCommandCollapsed);
			command.RemoveCommandControlValueChangedHandler(OnControlValueChanged);
			command.RemoveCommandHoverHandler(OnControlMouseHover);
			command.RemoveCommandMouseLeaveHandler(OnControlMouseLeave);
			command.RemoveCommandPopupHandler(OnControlPopup);
			command.RemoveCommandCloseUpHandler(OnControlCloseUp);
			command.RemoveCommandClosingHandler(OnControlClosing);
			command.RemoveInvalidatedHandler(HandleInvalidateCommand);
			command.RemoveCommandAddedHandler(HandleCommandAdded);
			command.RemoveCommandRemovedHandler(HandleCommandRemoved);
			command.RemoveCommandDisposedHandler(OnCommandDisposed);
			command.RemoveExpandRequestedHandler(HandleCommandExpanded);
			command.RemoveCollapseRequestedHandler(HandleCommandCollapsed);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxCommand uiCommand = (MxCommand)value;
			uiCommand.SetHostElement(null);
			if (uiCommand.BaseCommand == null && uiCommand._commandManager != null)
			{
				uiCommand.Dispose();
				return;
			}
			if (_owner is MxCommandManager)
			{
				uiCommand._commandManager = null;
			}
			DetachCommandHandlers(uiCommand);
			MxCommand parentCommand = null;
			if (_owner is MxCommand)
			{
				parentCommand = _owner as MxCommand;
			}
			CommandActionEventArgs e = new CommandActionEventArgs(uiCommand, parentCommand);
			_owner.OnCommandRemoved(e);
			if (uiCommand.Container != null)
			{
				uiCommand.Container.Remove(uiCommand);
			}
			AssignCommandOrder();
			ShiftPositions(index, uiCommand, true);
		}

		private void ShiftPositions(int index, MxCommand command, bool isRemoved)
		{
			if (!_shouldUpdatePositions)
			{
				return;
			}
			foreach (MxCommand item in base.List)
			{
				if (item.GetPosition() == -1)
				{
					continue;
				}
				if (isRemoved)
				{
					if (item.GetPosition() >= index && command != item)
					{
						item.UpdatePosition(item.GetPosition() - 1);
					}
				}
				else if (item.GetPosition() >= index && command != item)
				{
					item.UpdatePosition(item.GetPosition() + 1);
				}
			}
			if (_isMerging)
			{
				command.UpdatePosition(index);
			}
		}

		internal void AssignCommandOrder()
		{
			for (int i = 0; i < base.Count; i++)
			{
				((MxCommand)base.List[i]).SetLayoutIndex(i);
			}
		}

		private void OnControlMouseHover(object sender, CommandEventArgs eventArgs)
		{
			_owner.OnCommandHover(eventArgs);
		}

		private void OnControlMouseLeave(object sender, CommandEventArgs eventArgs)
		{
			_owner.OnCommandMouseLeave(eventArgs);
		}

		private void OnControlClick(object sender, CommandEventArgs eventArgs)
		{
			if (_owner != null)
			{
				_owner.OnCommandClicked(eventArgs);
			}
		}

		private void OnControlPopup(object sender, CommandEventArgs eventArgs)
		{
			_owner.OnCommandPopUp(eventArgs);
		}

		private void OnControlCloseUp(object sender, CommandEventArgs eventArgs)
		{
			_owner.OnCommandCloseUp(eventArgs);
		}

		private void OnControlClosing(object sender, CommandClosingEventArgs eventArgs)
		{
			_owner.OnCommandClosing(eventArgs);
		}

		private void HandleInvalidateCommand(object sender, EventArgs eventArgs)
		{
			_owner.OnInvalidateCommand();
		}

		private void HandleCommandCollapsed(object sender, CommandEventArgs eventArgs)
		{
			_owner.OnCommandCollapsed(eventArgs);
		}

		private void HandleCommandExpanded(object sender, CommandEventArgs eventArgs)
		{
			_owner.OnCommandExpanded(eventArgs);
		}

		private void OnCommandDisposed(object sender, CommandEventArgs eventArgs)
		{
			if (Contains(eventArgs.Command))
			{
				_shouldUpdatePositions = false;
				base.List.Remove(eventArgs.Command);
				_shouldUpdatePositions = true;
			}
		}

		private void OnControlValueChanged(object sender, CommandEventArgs eventArgs)
		{
			_owner.OnCommandControlValueChanged(eventArgs);
		}

		private void HandleCommandAdded(object value, CommandActionEventArgs uiCommandEventArgs)
		{
			if (_owner != null)
			{
				_owner.OnCommandAdded(uiCommandEventArgs);
			}
		}

		private void HandleCommandRemoved(object value, CommandActionEventArgs uiCommandEventArgs)
		{
			if (_owner != null)
			{
				_owner.OnCommandRemoved(uiCommandEventArgs);
			}
		}

		public MxCommand[] GetCommandsByCategory(CommandCategory category)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			ArrayList arrayList = new ArrayList();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxCommand uiCommand = (MxCommand)enumerator.Current;
					if (category != null)
					{
						if (string.Compare(uiCommand.CategoryName, category.CategoryName, ignoreCase: true, invariantCulture) == 0)
						{
							arrayList.Add(uiCommand);
						}
					}
					else
					{
						arrayList.Add(uiCommand);
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
			MxCommand[] commandsArray = new MxCommand[arrayList.Count];
			arrayList.CopyTo(commandsArray, 0);
			return commandsArray;
		}

		internal void MoveCommandUp(int commandIndex)
		{
			if (commandIndex > 0)
			{
				MxCommand value = (MxCommand)base.List[commandIndex];
				int index = commandIndex - 1;
				base.List[commandIndex] = base.List[index];
				base.List[index] = value;
			}
		}

		internal void MoveCommandDown(int commandIndex)
		{
			if (commandIndex >= 0 && commandIndex < base.List.Count - 1)
			{
				MxCommand value = (MxCommand)base.List[commandIndex];
				int index = commandIndex + 1;
				base.List[commandIndex] = base.List[index];
				base.List[index] = value;
			}
		}
	}
}
