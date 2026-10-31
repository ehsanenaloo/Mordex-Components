using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter(typeof(SchedulerStoredFilterConverter))]
	public class SchedulerStoredFilter : ISchedulerFilterOwner, IFilterCondition, IMordexXmlLayoutsSupport
	{
		private string _key = "";

		internal int _index = -1;

		private string _description = "";

		private object _tag;

		private SchedulerFilterCondition _filterCondition;

		private SchedulerStoredFilterCollection _ownerCollection;

		private bool _isCurrent;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Represents the key used to identify an object in a colletion.")]
		[DefaultValue("")]
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

		[Browsable(false)]
		public int Index => _index;

		[DefaultValue("")]
		[Description("Specifies the description of the stored filter")]
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		[Description("Represents an object that contains data to associate with the SchedulerStoredFilter class.")]
		[DefaultValue(null)]
		[TypeConverter("System.ComponentModel.StringConverter")]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		[DefaultValue(null)]
		[TypeConverter(typeof(FilterConditionConverter))]
		[Editor(typeof(FilterConditionEditor), typeof(UITypeEditor))]
		[Description("The SchedulerFilterCondition associated to this instance.")]
		public SchedulerFilterCondition FilterCondition
		{
			get
			{
				return _filterCondition;
			}
			set
			{
				if (value != null && value._filterOwner != null)
				{
					throw new InvalidOperationException("FilterCondition belongs to another object.\nUse a clone of the SchedulerFilterCondition to be able to stored it in this object.");
				}
				if (value != _filterCondition)
				{
					_filterCondition = value;
					if (_filterCondition != null)
					{
						_filterCondition._filterOwner = this;
					}
				}
			}
		}

		[Browsable(false)]
		public MxScheduler Scheduler
		{
			get
			{
				if (_ownerCollection == null)
				{
					return null;
				}
				return _ownerCollection.Scheduler;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_description = (string)LayoutReader.GetPropertyValue(info, "Description", typeof(string), "");
			_isCurrent = (bool)LayoutReader.GetPropertyValue(info, "IsCurrent", typeof(bool), false);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			string text = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			if (text != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null);
				if (tagTypeName != null)
				{
					try
					{
						Type type = LayoutTypeResolver.Resolve(tagTypeName);
						_tag = PropertyValue.ConvertValue(text, type);
					}
					catch
					{
						_tag = text;
					}
				}
				else
				{
					_tag = text;
				}
			}
			PropertyValue propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				_filterCondition = new SchedulerFilterCondition();
				_filterCondition._filterOwner = this;
				propertyValue.DeserializeObject(_filterCondition);
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_description != "")
			{
				info.AddValue("Description", _description);
			}
			if (_filterCondition != null)
			{
				info.AddValue("FilterCondition", _filterCondition);
			}
			if (_isCurrent)
			{
				info.AddValue("IsCurrent", _isCurrent);
			}
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
			if (_tag != null)
			{
				if (!(_tag is string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				info.AddValue("Tag", _tag);
			}
		}

		internal SchedulerStoredFilterCollection GetSchedulerStoredFilterCollection()
		{
			return _ownerCollection;
		}

		internal void SetSchedulerStoredFilterCollection(SchedulerStoredFilterCollection schedulerStoredFilterCollection)
		{
			_ownerCollection = schedulerStoredFilterCollection;
		}

		internal bool GetIsCurrent()
		{
			return _isCurrent;
		}

		internal void SetIsCurrent(bool isCurrent)
		{
			_isCurrent = isCurrent;
		}
	}
}
