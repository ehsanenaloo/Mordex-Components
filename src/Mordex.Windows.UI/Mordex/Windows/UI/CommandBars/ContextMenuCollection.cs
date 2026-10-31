using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.UI.CommandBars
{
	public class ContextMenuCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal MxCommandManager _commandManager;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "ContextMenu";

		public MxContextMenu this[int index] => (MxContextMenu)base.List[index];

		public MxContextMenu this[string key]
		{
			get
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				foreach (MxContextMenu item in base.List)
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
			int menuIndex = 0;
			for (PropertyValue propertyValue = reader["ContextMenu" + menuIndex]; propertyValue != null; propertyValue = reader["ContextMenu" + menuIndex])
			{
				MxContextMenu uiContextMenu = new MxContextMenu();
				base.InnerList.Add(uiContextMenu);
				uiContextMenu._index = menuIndex;
				menuIndex++;
				propertyValue.DeserializeObject(uiContextMenu);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					MxContextMenu uiContextMenu = (MxContextMenu)enumerator.Current;
					info.AddValue(uiContextMenu.Key, uiContextMenu);
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

		internal ContextMenuCollection(MxCommandManager commandManager)
		{
			_commandManager = commandManager;
		}

		public bool Contains(MxContextMenu contextMenu)
		{
			return base.List.Contains(contextMenu);
		}

		public int IndexOf(MxContextMenu contextMenu)
		{
			return base.List.IndexOf(contextMenu);
		}

		public void AddRange(MxContextMenu[] contextMenus)
		{
			foreach (MxContextMenu contextMenu in contextMenus)
			{
				Add(contextMenu);
			}
		}

		public void Add(MxContextMenu contextMenu)
		{
			if (base.List.Contains(contextMenu))
			{
				if (_commandManager != null && !_commandManager.IsInitializing())
				{
					throw new InvalidOperationException("The UIContextMenu is already in the collection");
				}
				return;
			}
			if (contextMenu.CommandManager != null && contextMenu.CommandManager != _commandManager)
			{
				throw new InvalidOperationException("The UIContextMenu can not belog to two different CommandManager");
			}
			base.List.Add(contextMenu);
		}

		public void Remove(MxContextMenu contextMenu)
		{
			base.List.Remove(contextMenu);
		}

		public new void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		internal MxContextMenu FindByKey(string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (MxContextMenu item in base.List)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		protected override void OnInsertComplete(int index, object value)
		{
			MxContextMenu uiContextMenu = (MxContextMenu)value;
			RenumberIndexes();
			_commandManager.AttachContextMenu(uiContextMenu);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxContextMenu uiContextMenu = (MxContextMenu)value;
			uiContextMenu._index = -1;
			_commandManager.DetachContextMenu(uiContextMenu);
			RenumberIndexes();
		}

		protected override void OnInsert(int index, object value)
		{
			EnsureNotOwnedByOtherManager((MxContextMenu)value);
			base.OnInsert(index, value);
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				MxContextMenu contextMenu = (MxContextMenu)base.List[index];
				Remove(contextMenu);
				index--;
			}
		}

		internal void ClearItems()
		{
			base.InnerList.Clear();
		}

		internal void EnsureNotOwnedByOtherManager(MxContextMenu uiContextMenu)
		{
			if (uiContextMenu.CommandManager != null && _commandManager != null && uiContextMenu.CommandManager != _commandManager)
			{
				throw new InvalidOperationException("To add the UIContextMenu you must first remove it from it current CommandManager");
			}
		}

		internal void RenumberIndexes()
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i]._index = i;
			}
		}
	}
}
