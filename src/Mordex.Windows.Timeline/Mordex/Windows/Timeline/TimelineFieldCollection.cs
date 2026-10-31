using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;

namespace Mordex.Windows.Timeline
{
	public class TimelineFieldCollection : MordexFieldCollectionBase, IMordexXmlLayoutsSupport
	{
		private TimelineMordexTableBase _table;

		private bool _shouldNotifyOwner = true;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Field";

		public MxTimeline Timeline
		{
			get
			{
				if (_table != null)
				{
					return _table.GetTimeline();
				}
				return null;
			}
		}

		public TimelineField this[int index] => (TimelineField)BaseGet(index);

		public TimelineField this[string key] => (TimelineField)BaseGet(key);

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int index = 0;
			for (PropertyValue propertyValue = info["Field" + index]; propertyValue != null; propertyValue = info["Field" + index])
			{
				TimelineField field = new TimelineField();
				BaseAdd(field);
				index++;
			}
			index = 0;
			for (PropertyValue propertyValue = info["Field" + index]; propertyValue != null; propertyValue = info["Field" + index])
			{
				propertyValue.DeserializeObject(this[index]);
				Timeline.SyncDataTableColumn(this[index], EditType.Add);
				index++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int index = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TimelineField value = (TimelineField)enumerator.Current;
					info.AddValue("Field" + index, value);
					index++;
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

		internal TimelineFieldCollection(TimelineMordexTableBase table)
			: base(table)
		{
			_table = table;
		}

		public void Add(TimelineField field)
		{
			BaseAdd(field);
		}

		public void AddRange(TimelineField[] fields)
		{
			_shouldNotifyOwner = false;
			for (int i = 0; i < fields.Length; i++)
			{
				Add(fields[i]);
			}
			_shouldNotifyOwner = true;
			SyncUnboundColumn(null, EditType.Clear);
		}

		public void Remove(TimelineField field)
		{
			BaseRemove(field);
		}

		public bool Contains(TimelineField field)
		{
			return base.List.Contains(field);
		}

		public bool Contains(string fieldKey)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TimelineField timelineField = (TimelineField)enumerator.Current;
					if (string.Compare(timelineField.Key, fieldKey, ignoreCase: true) == 0)
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

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			if (Timeline != null)
			{
				Timeline.UpdateTimelineField(null);
			}
			SyncUnboundColumn(null, EditType.Clear);
		}

		internal void ReplaceFields(TimelineField[] objects)
		{
			BaseReplaceElements(objects);
		}

		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			TimelineField timelineField = (TimelineField)value;
			if (timelineField.ItemsImage && Timeline != null)
			{
				Timeline.UpdateTimelineField(timelineField);
			}
			if (timelineField.DataMemberMapping == DataMemberMapping.Custom)
			{
				SyncUnboundColumn(timelineField, EditType.Add);
			}
			else
			{
				timelineField.ApplyDataMemberMapping();
			}
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			TimelineField timelineField = (TimelineField)value;
			if (timelineField.ItemsImage && Timeline != null)
			{
				Timeline.UpdateTimelineField(null);
			}
			base.OnRemoveComplete(index, value);
			if (timelineField.DataMemberMapping == DataMemberMapping.Custom)
			{
				SyncUnboundColumn(timelineField, EditType.Remove);
			}
		}

		internal void SyncUnboundColumn(TimelineField timelineField, EditType editType)
		{
			if (_shouldNotifyOwner && Timeline != null)
			{
				Timeline.SyncDataTableColumn(timelineField, editType);
			}
		}
	}
}
