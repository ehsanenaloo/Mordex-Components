using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.TaskPane
{
	public class TaskPaneItemCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private TaskPaneGroup _group;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Item";

		public TaskPaneItem this[int index]
		{
			get
			{
				return (TaskPaneItem)base.InnerList[index];
			}
		}

		public TaskPaneItem this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TaskPaneItem taskPaneItem = (TaskPaneItem)enumerator.Current;
						if (string.Equals(taskPaneItem.Key, key, StringComparison.OrdinalIgnoreCase))
						{
							return taskPaneItem;
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
			int itemIndex = 0;
			for (PropertyValue propertyValue = info["Item" + itemIndex]; propertyValue != null; propertyValue = info["Item" + itemIndex])
			{
				TaskPaneItem taskPaneItem = new TaskPaneItem();
				base.InnerList.Add(taskPaneItem);
				taskPaneItem._owner = this;
				taskPaneItem._index = itemIndex;
				itemIndex++;
				propertyValue.DeserializeObject(taskPaneItem);
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
					TaskPaneItem currentItem = (TaskPaneItem)enumerator.Current;
					info.AddValue("Item" + itemIndex, currentItem);
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

		internal TaskPaneItemCollection(TaskPaneGroup group)
		{
			if (group == null)
			{
				throw new ArgumentNullException();
			}
			_group = group;
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskPaneItem taskPaneItem = (TaskPaneItem)enumerator.Current;
					taskPaneItem._owner = null;
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

		protected override void OnClearComplete()
		{
			InvalidateGroupLayout();
		}

		public bool Contains(TaskPaneItem item)
		{
			return base.InnerList.Contains(item);
		}

		public void Insert(int index, TaskPaneItem item)
		{
			if (item._owner != null)
			{
				throw new InvalidOperationException();
			}
			for (int i = index; i < base.Count; i++)
			{
				this[i]._index++;
			}
			item._index = index;
			item._owner = this;
			base.InnerList.Insert(index, item);
			InvalidateGroupLayout();
		}

		public void Remove(TaskPaneItem item)
		{
			if (base.InnerList.Contains(item))
			{
				for (int i = item._index + 1; i < base.Count; i++)
				{
					this[i]._index--;
				}
				base.InnerList.Remove(item);
				if (item.ContainerControl != null && item.TaskPane != null)
				{
					item.TaskPane.RemoveItemContainerControl(item);
				}
				item._owner = null;
				InvalidateGroupLayout();
			}
		}

		public new void RemoveAt(int index)
		{
			TaskPaneItem item = this[index];
			Remove(item);
		}

		internal TaskPaneGroup GetGroup()
		{
			return _group;
		}

		public new void Clear()
		{
			int itemIndex;
			for (itemIndex = 0; itemIndex < base.List.Count; itemIndex++)
			{
				TaskPaneItem item = (TaskPaneItem)base.List[itemIndex];
				Remove(item);
				itemIndex--;
			}
		}

		public void Add(TaskPaneItem item)
		{
			if (item._owner != null)
			{
				throw new InvalidOperationException();
			}
			item._index = base.Count;
			base.InnerList.Add(item);
			item._owner = this;
			InvalidateGroupLayout();
		}

		public void AddRange(TaskPaneItem[] items)
		{
			for (int i = 0; i < items.Length; i++)
			{
				Add(items[i]);
			}
		}

		public void Add(string text)
		{
			TaskPaneItem item = new TaskPaneItem(text);
			Add(item);
		}

		public void Add(string text, int imageIndex)
		{
			TaskPaneItem item = new TaskPaneItem(text, imageIndex);
			Add(item);
		}

		public void Add(string text, ItemType itemType)
		{
			TaskPaneItem item = new TaskPaneItem(text, itemType);
			Add(item);
		}

		internal void InvalidateGroupLayout()
		{
			if (_group != null)
			{
				_group.InvalidateOwnerLayout();
			}
		}

		internal void InvalidateTaskPane()
		{
			if (_group != null)
			{
				_group.InvalidateTaskPane();
			}
		}

		internal void ResetItemStates()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskPaneItem taskPaneItem = (TaskPaneItem)enumerator.Current;
					taskPaneItem._itemState = ItemState.Normal;
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
	}
}
