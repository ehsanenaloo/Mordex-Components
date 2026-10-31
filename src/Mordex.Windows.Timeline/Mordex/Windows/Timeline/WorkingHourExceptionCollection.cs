using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Timeline
{
	public class WorkingHourExceptionCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		private MxTimeline _timeline;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Exception";

		public MxTimeline Timeline => _timeline;

		public WorkingHourException this[int index] => (WorkingHourException)base.List[index];

		public WorkingHourException this[string key]
		{
			get
			{
				foreach (WorkingHourException item in base.List)
				{
					if (item.Key == key)
					{
						return item;
					}
				}
				throw new ArgumentOutOfRangeException("There is no object with '" + key + "' in this collection.");
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int exceptionIndex = 0;
			for (PropertyValue propertyValue = info["Exception" + exceptionIndex]; propertyValue != null; propertyValue = info["Exception" + exceptionIndex])
			{
				WorkingHourException ex = new WorkingHourException();
				base.InnerList.Add(ex);
				ex._owner = this;
				exceptionIndex++;
			}
			exceptionIndex = 0;
			for (PropertyValue propertyValue = info["Exception" + exceptionIndex]; propertyValue != null; propertyValue = info["Exception" + exceptionIndex])
			{
				propertyValue.DeserializeObject(this[exceptionIndex]);
				exceptionIndex++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int exceptionIndex = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					WorkingHourException value = (WorkingHourException)enumerator.Current;
					info.AddValue("Exception" + exceptionIndex, value);
					exceptionIndex++;
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

		internal WorkingHourExceptionCollection(MxTimeline timeline)
		{
			_timeline = timeline;
		}

		public void AddRange(WorkingHourException[] exceptions)
		{
			for (int i = 0; i < exceptions.Length; i++)
			{
				Add(exceptions[i]);
			}
		}

		public void Add(WorkingHourException exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException();
			}
			if (exception._owner != null)
			{
				if (exception.Timeline != _timeline)
				{
					throw new InvalidOperationException("The exception day can only belog to one Timeline");
				}
				throw new InvalidOperationException("The exception day can only be added once");
			}
			base.List.Add(exception);
		}

		public bool Contains(WorkingHourException exception)
		{
			return base.List.Contains(exception);
		}

		public void Remove(WorkingHourException exception)
		{
			base.List.Remove(exception);
		}

		public new void Clear()
		{
			int index;
			for (index = 0; index < base.List.Count; index++)
			{
				WorkingHourException exception = this[index];
				Remove(exception);
				index--;
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			WorkingHourException ex = (WorkingHourException)value;
			ex._owner = null;
			InvalidateOwnerLayout();
		}

		protected override void OnInsertComplete(int index, object value)
		{
			WorkingHourException ex = (WorkingHourException)value;
			ex._owner = this;
			InvalidateOwnerLayout();
		}

		internal void InvalidateOwnerLayout()
		{
			if (_timeline != null)
			{
				Timeline.InvalidateLayout();
			}
		}
	}
}
