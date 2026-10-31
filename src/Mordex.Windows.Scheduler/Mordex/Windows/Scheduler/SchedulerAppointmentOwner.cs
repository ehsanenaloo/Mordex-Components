using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter(typeof(AppointmentOwnerConverter))]
	public class SchedulerAppointmentOwner : IMordexXmlLayoutsSupport, ISchedulerWorkingHoursOwner
	{
		internal SchedulerAppointmentOwnerCollection _owner;

		private string _text = "";

		private object _tag;

		private int _imageIndex = -1;

		private string _imageKey = "";

		internal int _index = -1;

		private string _key = "";

		private bool _visible = true;

		private SchedulerHourRangeCollection _dayViewHourRanges;

		private WorkingHoursSchema _workingHourSchema;

		private object _value;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		SchedulerDayOfWeek ISchedulerWorkingHoursOwner.FirstDayOfWeek
		{
			get
			{
				if (Scheduler != null)
				{
					return Scheduler.FirstDayOfWeek;
				}
				return SchedulerDayOfWeek.Sunday;
			}
		}

		SchedulerDayOfWeek ISchedulerWorkingHoursOwner.WorkWeek
		{
			get
			{
				if (Scheduler != null)
				{
					return Scheduler.WorkWeek;
				}
				return SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
			}
		}

		DateTime ISchedulerWorkingHoursOwner.MinDate
		{
			get
			{
				if (Scheduler != null)
				{
					return Scheduler.MinDate;
				}
				return DateTime.MinValue.AddMonths(1);
			}
		}

		MxScheduler ISchedulerWorkingHoursOwner.Scheduler => Scheduler;

		[Browsable(false)]
		public MxScheduler Scheduler
		{
			get
			{
				if (_owner != null)
				{
					return _owner.Scheduler;
				}
				return null;
			}
		}

		[DefaultValue("")]
		[Description("The text of the SchedulerAppointmentOwner.")]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (_text != value)
				{
					_text = value;
					UpdateSchedulerAppointmentOwner();
				}
			}
		}

		[DefaultValue(null)]
		[Browsable(false)]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				if (_tag != value)
				{
					_tag = value;
				}
			}
		}

		[Description("Represents the image list index of the image displayed on the SchedulerAppointmentOwner")]
		[TypeConverter(typeof(ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					_imageKey = "";
					UpdateSchedulerAppointmentOwner();
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(ImageKeyConverter))]
		[Description("The key accessor for the image in the ImageList displayed on the SchedulerAppointmentOwner")]
		[DefaultValue("")]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageKey != value)
				{
					_imageKey = value;
					_imageIndex = -1;
					UpdateSchedulerAppointmentOwner();
				}
			}
		}

		[Browsable(false)]
		public int Index => _index;

		[DefaultValue("")]
		[Description("Represents the key used to identify an object in a colletion.")]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		[Description("Indicates whether the SchedulerAppointmentOwner is visible or hidden.")]
		[DefaultValue(true)]
		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				if (_visible != value)
				{
					if (_owner != null && _owner._scheduler.CurrentOwner != null && _owner._scheduler.CurrentOwner.Equals(this) && !value && !Scheduler.TrySelectAdjacentOwner())
					{
						throw new InvalidOperationException("You can not hide the current owner");
					}
					_visible = value;
					if (_owner != null)
					{
						_owner._scheduler.RefreshOwners(true);
					}
				}
			}
		}

		[Description("Represents the collection of SchedulerHourRange object used to format the rows in the Schedule control.")]
		[Browsable(false)]
		[Obsolete("This property is obsolete. Use WorkingHourSchema.WorkingHoursRange property instead.")]
		public SchedulerHourRangeCollection DayViewHourRanges => _dayViewHourRanges;

		[Browsable(false)]
		public WorkingHoursSchema WorkingHourSchema => _workingHourSchema;

		[Description("Represents the object value to compare with the appointments.")]
		[DefaultValue(null)]
		[TypeConverter(typeof(StringConverter))]
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (_value != value)
				{
					if (_owner != null)
					{
						_owner.EnsureValueIsUnique(value);
					}
					_value = value;
					if (_owner != null)
					{
						_owner._scheduler.RefreshOwners(false);
					}
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), "");
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), string.Empty);
			_visible = (bool)LayoutReader.GetPropertyValue(info, "Visible", typeof(bool), true);
			string text = (string)LayoutReader.GetPropertyValue(info, "Value", typeof(string), null);
			if (text != null)
			{
				string valueTypeName = (string)LayoutReader.GetPropertyValue(info, "ValueTypeName", typeof(string), null);
				try
				{
					if (valueTypeName != null)
					{
						Type type = LayoutTypeResolver.Resolve(valueTypeName);
						_value = PropertyValue.ConvertValue(text, type);
					}
					else
					{
						_value = text;
					}
				}
				catch
				{
					_value = text;
				}
			}
			else
			{
				_value = null;
			}
			string tagText = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			if (tagText != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null);
				if (tagTypeName != null)
				{
					try
					{
						Type tagType = LayoutTypeResolver.Resolve(tagTypeName);
						_tag = PropertyValue.ConvertValue(tagText, tagType);
					}
					catch
					{
						_tag = tagText;
					}
				}
				else
				{
					_tag = tagText;
				}
			}
			else
			{
				_tag = null;
			}
			PropertyValue propertyValue = info["DayViewHourRanges"];
			_dayViewHourRanges = new SchedulerHourRangeCollection(this);
			propertyValue?.DeserializeObject(_dayViewHourRanges);
			_workingHourSchema = new WorkingHoursSchema(this, _dayViewHourRanges);
			info["WorkingHourSchema"]?.DeserializeObject(_workingHourSchema);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (_key != string.Empty)
			{
				info.AddValue("Key", _key);
			}
			if (_text.Length > 0)
			{
				info.AddValue("Text", _text);
			}
			if (!_visible)
			{
				info.AddValue("Visible", _visible);
			}
			if (_value != null)
			{
				if (_value.GetType() != typeof(string))
				{
					info.AddValue("ValueTypeName", _value.GetType().FullName);
				}
				info.AddValue("Value", _value);
			}
			if (_tag != null)
			{
				if (_tag.GetType() != typeof(string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				if (_tag.GetType().IsEnum)
				{
					_ = (int)_tag;
					info.AddValue("Tag", _tag);
				}
				else
				{
					if (_tag.GetType() != typeof(string))
					{
						info.AddValue("TagTypeName", _tag.GetType().FullName);
					}
					info.AddValue("Tag", _tag);
				}
			}
			if (_workingHourSchema.IsCustomized())
			{
				info.AddValue("WorkingHourSchema", _workingHourSchema);
			}
		}

		void ISchedulerWorkingHoursOwner.InvalidateHour()
		{
			if (Scheduler != null)
			{
				Scheduler._isHourRangeCacheStale = true;
				Scheduler.InvalidateAppointmentLayout();
			}
		}

		public SchedulerAppointmentOwner(object Value, string text)
		{
			_value = Value;
			_text = text;
			_dayViewHourRanges = new SchedulerHourRangeCollection(this);
			_workingHourSchema = new WorkingHoursSchema(this, _dayViewHourRanges);
		}

		public SchedulerAppointmentOwner()
		{
			_dayViewHourRanges = new SchedulerHourRangeCollection(this);
			_workingHourSchema = new WorkingHoursSchema(this, _dayViewHourRanges);
		}

		public SchedulerAppointmentOwner(object Value, string text, int imageIndex)
		{
			_value = Value;
			_text = text;
			_imageIndex = imageIndex;
			_dayViewHourRanges = new SchedulerHourRangeCollection(this);
			_workingHourSchema = new WorkingHoursSchema(this, _dayViewHourRanges);
		}

		private void UpdateSchedulerAppointmentOwner()
		{
			if (_owner != null)
			{
				_owner._scheduler.RefreshOwners(false);
			}
		}

		internal bool ValueEquals(object otherValue)
		{
			if (_value == otherValue)
			{
				return true;
			}
			if (_value != null && _value.Equals(otherValue))
			{
				return true;
			}
			return false;
		}
	}
}
