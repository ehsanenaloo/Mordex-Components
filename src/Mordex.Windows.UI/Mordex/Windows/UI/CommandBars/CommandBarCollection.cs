using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.CommandBars
{
	public class CommandBarCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private bool _validateBars = true;

		internal ICommandBarOwner _owner;

		internal CommandBarCollection _mergedBars;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "CommandBar";

		public MxCommandBar this[int index] => (MxCommandBar)base.List[index];

		public MxCommandBar this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (MxCommandBar item in base.List)
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
			int barIndex = 0;
			for (PropertyValue propertyValue = reader["CommandBar" + barIndex]; propertyValue != null; propertyValue = reader["CommandBar" + barIndex])
			{
				MxCommandBar uiCommandBar = new MxCommandBar();
				AddWithoutEvents(uiCommandBar);
				barIndex++;
				propertyValue.DeserializeObject(uiCommandBar);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxCommandBar uiCommandBar = (MxCommandBar)enumerator.Current;
					info.AddValue(uiCommandBar.Key, uiCommandBar);
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
			if (_mergedBars != null)
			{
				info.AddValue("MergedBars", _mergedBars);
			}
		}

		internal CommandBarCollection(ICommandBarOwner owner, bool validateBars)
		{
			_validateBars = validateBars;
			_owner = owner;
		}

		internal CommandBarCollection(ICommandBarOwner owner)
		{
			_owner = owner;
		}

		internal MxCommandManager GetOwnerCommandManager()
		{
			return _owner.GetCommandManagerOwner();
		}

		public bool Contains(MxCommandBar commandBar)
		{
			return base.List.Contains(commandBar);
		}

		public bool Contains(string key)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxCommandBar uiCommandBar = (MxCommandBar)enumerator.Current;
					if (string.Compare(uiCommandBar.Key, key, ignoreCase: true, invariantCulture) == 0)
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

		public int IndexOf(MxCommandBar commandBar)
		{
			return base.List.IndexOf(commandBar);
		}

		public void AddRange(MxCommandBar[] commandBars)
		{
			foreach (MxCommandBar commandBar in commandBars)
			{
				Add(commandBar);
			}
		}

		public void Add(MxCommandBar commandBar)
		{
			if (base.List.Contains(commandBar))
			{
				if (_owner.GetCommandManagerOwner() != null && !_owner.IsInitializing && !_owner.GetCommandManagerOwner().IsInitializing())
				{
					throw new InvalidOperationException("The CommandBar is already in the collection");
				}
			}
			else if (!(_owner is MxCommandManager) || GetOwnerCommandManager().CanAddCommandBar(commandBar))
			{
				base.List.Add(commandBar);
			}
		}

		internal void AddWithoutEvents(MxCommandBar uiCommandBar)
		{
			base.InnerList.Add(uiCommandBar);
		}

		public void Remove(MxCommandBar commandBar)
		{
			if (!(_owner is MxCommandManager) || GetOwnerCommandManager().CanRemoveCommandBar(commandBar))
			{
				base.List.Remove(commandBar);
			}
		}

		public new void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		internal MxCommandBar FindBarByKey(string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxCommandBar item in base.List)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		internal MxCommandBar FindBarByKeyAndParentId(string strB, Guid guid)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxCommandBar item in base.List)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0 && guid == item.GetParentID())
				{
					return item;
				}
			}
			return null;
		}

		internal MxCommandBar FindBarByBaseCommand(MxCommand uiCommand)
		{
			foreach (MxCommandBar item in base.List)
			{
				if (item.GetSourceCommand() != null && item.GetSourceCommand().BaseCommand == uiCommand)
				{
					return item;
				}
			}
			return null;
		}

		protected override void OnInsertComplete(int index, object value)
		{
			MxCommandBar toolBar = (MxCommandBar)value;
			UpdateDetachedIndexes();
			_owner.OnBarAdded(toolBar);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxCommandBar uiCommandBar = (MxCommandBar)value;
			if (_owner is MxCommandManager)
			{
				uiCommandBar._detachedIndex = -1;
			}
			if (_owner != null)
			{
				_owner.OnBarRemoved(uiCommandBar);
			}
			UpdateDetachedIndexes();
		}

		protected override void OnInsert(int index, object value)
		{
			ValidateBarOwnership((MxCommandBar)value);
			base.OnInsert(index, value);
		}

		public new void Clear()
		{
			int mergedIndex;
			for (mergedIndex = 0; mergedIndex < base.List.Count; mergedIndex++)
			{
				MxCommandBar commandBar = (MxCommandBar)base.List[mergedIndex];
				Remove(commandBar);
				mergedIndex--;
			}
		}

		internal void ClearWithoutEvents()
		{
			base.InnerList.Clear();
		}

		internal void ValidateBarOwnership(MxCommandBar uiCommandBar)
		{
			if (_validateBars && uiCommandBar.CommandManager != null && GetOwnerCommandManager() != null && uiCommandBar.CommandManager != GetOwnerCommandManager())
			{
				throw new InvalidOperationException("To add the CommandBar you must first remove it from its current CommandManager");
			}
		}

		internal void UnmergeBars(MxCommandManager commandManager, bool isExplicitMerge)
		{
			for (int i = 0; i < base.List.Count; i++)
			{
				MxCommandBar uiCommandBar = (MxCommandBar)base.List[i];
				uiCommandBar.MergeRowOrder = uiCommandBar.RowIndex;
				uiCommandBar.SetMergedLocation(uiCommandBar.Location);
				if (uiCommandBar.GetIsMergedCommandBar())
				{
					uiCommandBar.RemoveDockedPlaceholder();
					int index = 0;
					if (_mergedBars != null)
					{
						foreach (MxCommandBar item in _mergedBars)
						{
							if (string.Compare(item.Key, uiCommandBar.Key) == 0 && item.GetParentID() == commandManager.Id)
							{
								_mergedBars.List[index] = uiCommandBar;
								break;
							}
							index++;
						}
					}
					if (!commandManager.CommandBars.Contains(uiCommandBar))
					{
						continue;
					}
					Remove(uiCommandBar);
					if (isExplicitMerge)
					{
						uiCommandBar._commandManager = commandManager;
						if (!commandManager._isBatchUpdating)
						{
							uiCommandBar._forceDockStyleUpdate = true;
							uiCommandBar.DockStyle = uiCommandBar.DockStyle;
							commandManager.RefreshRebars();
						}
					}
					i--;
					continue;
				}
				uiCommandBar.RestoreUnmergedCommands();
				if (isExplicitMerge)
				{
					MxCommandBar existingBar = commandManager.CommandBars.FindBarByKey(uiCommandBar.Key);
					if (existingBar != null)
					{
						existingBar.Visible = true;
					}
				}
			}
			if (GetOwnerCommandManager() != null && !GetOwnerCommandManager().KeepMergeSettings && _mergedBars != null)
			{
				_mergedBars.ClearWithoutEvents();
				_mergedBars = null;
			}
			ArrayList arrayList = GetSortedBars(true, true, true);
			foreach (MxCommandBar sortedBar in arrayList)
			{
				if (!sortedBar.GetMergedLocation().IsEmpty)
				{
					sortedBar.RecalculateLayout(true);
				}
			}
		}

		internal void MergeBars(MxCommandManager uiCommandManager)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxCommandBar uiCommandBar = (MxCommandBar)enumerator.Current;
					if (!uiCommandBar.CanMerge())
					{
						continue;
					}
					MxCommandBar existingBar = uiCommandManager.CommandBars.FindBarByKey(uiCommandBar.Key);
					if (existingBar != null)
					{
						existingBar.Visible = false;
						uiCommandBar.RemoveHostedControls();
						uiCommandBar.Commands.MergeCommands(existingBar.Commands, uiCommandManager);
						continue;
					}
					uiCommandBar.Commands.MergeCommands(null, uiCommandManager);
					if (uiCommandBar.MergeRowOrder == -1)
					{
						uiCommandBar.MergeRowOrder = uiCommandBar.RowIndex;
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
			foreach (MxCommandBar commandBar in uiCommandManager.CommandBars)
			{
				commandBar.SetParentID(uiCommandManager.Id);
				if (!commandBar.CanMerge())
				{
					continue;
				}
				MxCommandBar matchingBar = FindBarByKey(commandBar.Key);
				if (matchingBar != null)
				{
					continue;
				}
				if (_mergedBars != null)
				{
					matchingBar = _mergedBars.FindBarByKeyAndParentId(commandBar.Key, uiCommandManager.Id);
				}
				else
				{
					_mergedBars = new CommandBarCollection(_owner, validateBars: false);
				}
				if (matchingBar == null)
				{
					_mergedBars.Add(commandBar);
					if (commandBar.MergeRowOrder < 0)
					{
						commandBar.MergeRowOrder = base.Count;
					}
				}
				else
				{
					commandBar.MergeRowOrder = matchingBar.MergeRowOrder;
					commandBar.SetMergedLocation(matchingBar.GetMergedLocation());
				}
				commandBar._commandManager = null;
				commandBar.SetIsMergedCommandBar(true);
				Add(commandBar);
				if (matchingBar != null)
				{
					commandBar.CopyFromBaseBar(matchingBar, uiCommandManager);
					continue;
				}
				commandBar._forceDockStyleUpdate = true;
				commandBar.DockStyle = commandBar.DockStyle;
			}
			ArrayList arrayList = GetSortedBars(true, true, true);
			foreach (MxCommandBar item in arrayList)
			{
				if (item.MergeRowOrder >= 0)
				{
					if (item.RowIndex != item.MergeRowOrder)
					{
						item.UpdateRowIndex(item.MergeRowOrder);
					}
					else
					{
						item.RecalculateLayout(true);
					}
				}
				else
				{
					item.RecalculateLayout(true);
				}
			}
			foreach (MxCommandBar sortedBar in arrayList)
			{
				if (!sortedBar.GetMergedLocation().IsEmpty && sortedBar.GetHostRow() != null)
				{
					if (sortedBar.GetHostRow().GetIsHorizontal())
					{
						sortedBar.MoveToLeft(sortedBar.GetMergedLocation().X);
					}
					else
					{
						sortedBar.MoveToTop(sortedBar.GetMergedLocation().Y);
					}
					sortedBar.Location = sortedBar.GetMergedLocation();
				}
			}
		}

		internal void UpdateDetachedIndexes()
		{
			if (_owner is MxCommandManager)
			{
				for (int i = 0; i < base.Count; i++)
				{
					this[i]._detachedIndex = i;
				}
			}
		}

		internal ArrayList GetSortedBars(bool isHorizontal, bool compareByRow)
		{
			return GetSortedBars(isHorizontal, compareByRow, false);
		}

		internal ArrayList GetSortedBars(bool isHorizontal, bool compareByRow, bool compareMergedValues)
		{
			CommandBarOrderComparer uiCommandManagerComparer = new CommandBarOrderComparer(compareByRow, compareMergedValues, GetOwnerCommandManager());
			uiCommandManagerComparer._isHorizontal = isHorizontal;
			ArrayList arrayList = CopyBars();
			arrayList.Sort(uiCommandManagerComparer);
			return arrayList;
		}

		internal ArrayList GetBarsSortedByPosition(bool horizontal)
		{
			CommandBarPositionComparer comparer = new CommandBarPositionComparer(horizontal);
			ArrayList arrayList = CopyBars();
			arrayList.Sort(comparer);
			return arrayList;
		}

		private ArrayList CopyBars()
		{
			ArrayList arrayList = new ArrayList();
			foreach (MxCommandBar item in base.List)
			{
				arrayList.Add(item);
			}
			return arrayList;
		}
	}
}
