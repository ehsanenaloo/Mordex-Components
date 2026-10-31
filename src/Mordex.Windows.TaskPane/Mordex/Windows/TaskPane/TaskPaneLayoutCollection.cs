using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.TaskPane
{
	public class TaskPaneLayoutCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private TaskPaneLayout _parentLayout;

		private MxTaskPane _taskPane;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Layout";

		public TaskPaneLayout this[int index] => (TaskPaneLayout)base.List[index];

		public TaskPaneLayout this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TaskPaneLayout taskPaneLayout = (TaskPaneLayout)enumerator.Current;
						if (string.Equals(taskPaneLayout.Key, key, StringComparison.OrdinalIgnoreCase))
						{
							return taskPaneLayout;
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
			int layoutIndex = 0;
			for (PropertyValue propertyValue = info["Layout" + layoutIndex]; propertyValue != null; propertyValue = info["Layout" + layoutIndex])
			{
				TaskPaneLayout taskPaneLayout = new TaskPaneLayout();
				propertyValue.DeserializeObject(taskPaneLayout);
				Add(taskPaneLayout);
				layoutIndex++;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskPaneLayout existingLayout = (TaskPaneLayout)enumerator.Current;
					if (existingLayout._isCurrentLayout)
					{
						GetOwnerTaskPane().CurrentLayout = existingLayout;
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

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int layoutIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskPaneLayout currentLayout = (TaskPaneLayout)enumerator.Current;
					info.AddValue("Layout" + layoutIndex, currentLayout);
					layoutIndex++;
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

		internal TaskPaneLayoutCollection(MxTaskPane expBar)
		{
			_taskPane = expBar;
		}

		internal TaskPaneLayoutCollection(TaskPaneLayout parentLayout)
		{
			_parentLayout = parentLayout;
		}

		internal TaskPaneLayout GetParentLayout()
		{
			return _parentLayout;
		}

		internal MxTaskPane ResolveTaskPane()
		{
			if (_parentLayout == null)
			{
				return _taskPane;
			}
			return _parentLayout.ResolveTaskPane();
		}

		internal MxTaskPane GetOwnerTaskPane()
		{
			return _taskPane;
		}

		public void Add(TaskPaneLayout layout)
		{
			base.List.Add(layout);
		}

		public void AddRange(TaskPaneLayout[] layouts)
		{
			foreach (TaskPaneLayout layout in layouts)
			{
				Add(layout);
			}
		}

		protected override void OnInsert(int index, object value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (value.GetType() != typeof(TaskPaneLayout))
			{
				throw new ArgumentException();
			}
			TaskPaneLayout taskPaneLayout = (TaskPaneLayout)value;
			if (taskPaneLayout.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException();
			}
			if (_parentLayout == null && !taskPaneLayout.StandAlone)
			{
				throw new InvalidOperationException("Only standalone layouts can be added to the root collection of layouts.");
			}
		}

		internal TaskPaneLayout FindCurrentLayout()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskPaneLayout taskPaneLayout = (TaskPaneLayout)enumerator.Current;
					TaskPaneLayout foundLayout = taskPaneLayout.FindCurrentLayout();
					if (foundLayout != null)
					{
						return foundLayout;
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
			return null;
		}

		protected override void OnInsertComplete(int index, object value)
		{
			((TaskPaneLayout)value).SetOwnerCollection(this);
			((TaskPaneLayout)value).EnsureIncrementalLayout();
		}

		public void Remove(TaskPaneLayout layout)
		{
			base.List.Remove(layout);
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			MxTaskPane taskPane = ResolveTaskPane();
			if (taskPane != null && taskPane.CurrentLayout == (TaskPaneLayout)value)
			{
				taskPane.CurrentLayout = null;
			}
			((TaskPaneLayout)value).SetOwnerCollection(null);
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskPaneLayout taskPaneLayout = (TaskPaneLayout)enumerator.Current;
					taskPaneLayout.SetOwnerCollection(null);
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

		public int IndexOf(TaskPaneLayout layout)
		{
			return base.List.IndexOf(layout);
		}

		public bool Contains(TaskPaneLayout layout)
		{
			return base.List.Contains(layout);
		}

		public bool Contains(string layoutKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TaskPaneLayout taskPaneLayout = (TaskPaneLayout)enumerator.Current;
					if (string.Equals(taskPaneLayout.Key, layoutKey, StringComparison.OrdinalIgnoreCase))
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
	}
}
