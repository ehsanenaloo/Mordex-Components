using System;
using System.Collections;
using System.Globalization;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.TaskPane
{
	[Serializable]
	public class TaskPaneGroupCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private MxTaskPane _taskPane;

		private bool _releasesContainers = true;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Group";

		public TaskPaneGroup this[int index] => (TaskPaneGroup)base.InnerList[index];

		public TaskPaneGroup this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TaskPaneGroup taskPaneGroup = (TaskPaneGroup)enumerator.Current;
						if (string.Equals(taskPaneGroup.Key, key, StringComparison.OrdinalIgnoreCase))
						{
							return taskPaneGroup;
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
			int groupIndex = 0;
			for (PropertyValue propertyValue = info["Group" + groupIndex]; propertyValue != null; propertyValue = info["Group" + groupIndex])
			{
				TaskPaneGroup taskPaneGroup = new TaskPaneGroup();
				base.InnerList.Add(taskPaneGroup);
				taskPaneGroup._owner = this;
				taskPaneGroup._index = groupIndex;
				groupIndex++;
				propertyValue.DeserializeObject(taskPaneGroup);
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
					TaskPaneGroup currentGroup = (TaskPaneGroup)enumerator.Current;
					info.AddValue("Group" + groupIndex, currentGroup);
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

		internal TaskPaneGroupCollection(MxTaskPane TaskPane)
		{
			if (TaskPane == null)
			{
				throw new ArgumentNullException();
			}
			SetOwnerTaskPane(TaskPane);
		}

		public bool Contains(TaskPaneGroup group)
		{
			return base.InnerList.Contains(group);
		}

		internal void ReindexGroups()
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i]._index = i;
				this[i]._owner = this;
			}
		}

		internal MxTaskPane GetOwnerTaskPane()
		{
			return _taskPane;
		}

		internal void SetOwnerTaskPane(MxTaskPane taskPane)
		{
			_taskPane = taskPane;
			if (_taskPane != null)
			{
				InvalidateLayout();
			}
		}

		internal void SyncGroupContainer(TaskPaneGroup taskPaneGroup)
		{
			if (_taskPane != null)
			{
				_taskPane.SyncGroupContainer(taskPaneGroup);
			}
		}

		public void Insert(int index, TaskPaneGroup group)
		{
			if (group == null)
			{
				throw new ArgumentNullException(nameof(group));
			}
			if (index < 0 || index > base.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			if (group._owner != null)
			{
				throw new InvalidOperationException();
			}
			for (int i = index; i < base.Count; i++)
			{
				this[i]._index++;
			}
			group._index = index;
			base.InnerList.Insert(index, group);
			group._owner = this;
			InvalidateLayout();
		}

		internal void ToggleGroup(TaskPaneGroup taskPaneGroup)
		{
			if (GetOwnerTaskPane() != null)
			{
				GetOwnerTaskPane().ToggleGroup(taskPaneGroup);
			}
		}

		internal void InvalidateLayout()
		{
			if (GetOwnerTaskPane() != null)
			{
				GetOwnerTaskPane().InvalidateLayout();
			}
		}

		internal void InvalidateTaskPane()
		{
			if (GetOwnerTaskPane() != null)
			{
				GetOwnerTaskPane().Invalidate(invalidateChildren: true);
			}
		}

		public void AddRange(TaskPaneGroup[] groups)
		{
			foreach (TaskPaneGroup taskPaneGroup in groups)
			{
				Add(taskPaneGroup);
			}
			InvalidateLayout();
		}

		public void Add(TaskPaneGroup group)
		{
			if (group._owner != null)
			{
				throw new InvalidOperationException();
			}
			group._index = base.Count;
			group._owner = this;
			base.InnerList.Add(group);
			if (group.Container)
			{
				SyncGroupContainer(group);
			}
			InvalidateLayout();
		}

		internal void AddWithoutNotify(TaskPaneGroup taskPaneGroup)
		{
			base.InnerList.Add(taskPaneGroup);
		}

		public void Add(string text)
		{
			TaskPaneGroup taskPaneGroup = new TaskPaneGroup(text);
			Add(taskPaneGroup);
		}

		public void Remove(TaskPaneGroup group)
		{
			if (base.InnerList.Contains(group))
			{
				for (int i = group._index + 1; i < base.Count; i++)
				{
					this[i]._index--;
				}
				base.InnerList.Remove(group);
				if (_releasesContainers)
				{
					group.Container = false;
				}
				group._owner = null;
				InvalidateLayout();
			}
		}

		public new void RemoveAt(int index)
		{
			TaskPaneGroup taskPaneGroup = this[index];
			Remove(taskPaneGroup);
		}

		internal TaskPaneGroup FindByKey(string strB)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			foreach (TaskPaneGroup item in base.List)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, invariantCulture) == 0)
				{
					return item;
				}
			}
			return null;
		}

		public void MoveAt(int index, TaskPaneGroup group)
		{
			int currentIndex = base.InnerList.IndexOf(group);
			if (currentIndex < 0)
			{
				throw new InvalidOperationException();
			}
			if (index < 0 || index >= base.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			if (index == currentIndex)
			{
				return;
			}
			base.InnerList.RemoveAt(currentIndex);
			base.InnerList.Insert(index, group);
			int first = Math.Min(index, currentIndex);
			int last = Math.Max(index, currentIndex);
			for (int i = first; i <= last; i++)
			{
				this[i]._index = i;
			}
			InvalidateLayout();
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskPaneGroup taskPaneGroup = (TaskPaneGroup)enumerator.Current;
					if (_releasesContainers)
					{
						taskPaneGroup.Container = false;
					}
					taskPaneGroup._owner = null;
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

		internal void ClearKeepingContainers()
		{
			_releasesContainers = false;
			try
			{
				base.List.Clear();
			}
			finally
			{
				_releasesContainers = true;
			}
		}

		protected override void OnClearComplete()
		{
			InvalidateLayout();
		}
	}
}
