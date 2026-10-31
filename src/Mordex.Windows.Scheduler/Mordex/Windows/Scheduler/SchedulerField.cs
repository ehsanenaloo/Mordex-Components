using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[TypeConverter(typeof(SchedulerFieldConverter))]
	public class SchedulerField : IMordexXmlLayoutsSupport
	{
		private string _dataMember = "";

		private bool _compareIgnoreCase = true;

		private DataMemberMapping _dataMemberMapping = DataMemberMapping.Custom;

		internal int _index = -1;

		internal SchedulerFieldCollection _owner;

		private TypeCode _typeCode = TypeCode.String;

		private Type _type;

		private string _key = "";

		private bool _includeInCopy;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Indicates the name of a property in the data source from which the field gets its data.")]
		[DefaultValue("")]
		[Editor(typeof(DataMemberEditor), typeof(UITypeEditor))]
		public string DataMember
		{
			get
			{
				if (_dataMemberMapping != DataMemberMapping.Custom && Scheduler != null)
				{
					switch (_dataMemberMapping)
					{
					case DataMemberMapping.EndTimeMember:
						return Scheduler.GetEndTimeMemberName();
					case DataMemberMapping.StartTimeMember:
						return Scheduler.GetStartTimeMemberName();
					case DataMemberMapping.OwnerMember:
						return Scheduler.GetOwnerMemberName();
					case DataMemberMapping.TextMember:
						return Scheduler.GetTextMemberName();
					case DataMemberMapping.DescriptionMember:
						return Scheduler.GetDescriptionMemberName();
					}
				}
				return _dataMember;
			}
			set
			{
				if (_dataMemberMapping != DataMemberMapping.Custom)
				{
					_dataMemberMapping = DataMemberMapping.Custom;
					_dataMember = value;
					if (Scheduler != null)
					{
						if (Scheduler.FilterConditionApplied != null && Scheduler.FilterConditionApplied.UsesField(this))
						{
							Scheduler.RebuildFilteredAppointments();
						}
						Scheduler.InvalidateAppointmentLayout();
					}
				}
				else
				{
					if (!(_dataMember != value))
					{
						return;
					}
					if (_key == _dataMember)
					{
						_key = "";
					}
					if (Scheduler != null)
					{
						if (Scheduler.FilterConditionApplied != null && Scheduler.FilterConditionApplied.UsesField(this))
						{
							Scheduler.RebuildFilteredAppointments();
						}
						Scheduler.InvalidateAppointmentLayout();
					}
					_dataMember = value;
					if (_key == "")
					{
						_key = _dataMember;
					}
				}
			}
		}

		[Description("Indicates whether the casing should be ignored when comparing text.")]
		[DefaultValue(true)]
		public bool CompareIgnoreCase
		{
			get
			{
				return _compareIgnoreCase;
			}
			set
			{
				if (_compareIgnoreCase == value)
				{
					return;
				}
				_compareIgnoreCase = value;
				if (Scheduler != null)
				{
					if (Scheduler.FilterConditionApplied != null && Scheduler.FilterConditionApplied.UsesField(this))
					{
						Scheduler.RebuildFilteredAppointments();
					}
					Scheduler.InvalidateAppointmentLayout();
				}
			}
		}

		[DefaultValue(DataMemberMapping.Custom)]
		[Description("Specifies to which property of a SchedulerAppointment the field is bound to.")]
		public DataMemberMapping DataMemberMapping
		{
			get
			{
				return _dataMemberMapping;
			}
			set
			{
				if (_dataMemberMapping == value)
				{
					return;
				}
				_dataMemberMapping = value;
				if (Scheduler != null)
				{
					if (Scheduler.FilterConditionApplied != null && Scheduler.FilterConditionApplied.Field.Equals(this))
					{
						Scheduler.RebuildFilteredAppointments();
					}
					Scheduler.InvalidateAppointmentLayout();
				}
			}
		}

		[Browsable(false)]
		public int Index => _index;

		[DefaultValue(TypeCode.String)]
		[Description("Represents the TypeCode of the values in the field.")]
		public TypeCode TypeCode
		{
			get
			{
				if (_owner != null && !_owner.Scheduler.IsUnboundMode())
				{
					_typeCode = _owner.Scheduler.GetFieldTypeCode(this);
				}
				return _typeCode;
			}
			set
			{
				if (_owner != null && !_owner.Scheduler.IsUnboundMode())
				{
					throw new InvalidOperationException("This property can only be set in unbound mode");
				}
				_typeCode = value;
			}
		}

		[Browsable(false)]
		public Type Type
		{
			get
			{
				if (TypeCode == TypeCode.Object)
				{
					if (_type == null)
					{
						return typeof(object);
					}
					return _type;
				}
				switch (_typeCode)
				{
				case TypeCode.Boolean:
					return typeof(bool);
				case TypeCode.Byte:
					return typeof(byte);
				case TypeCode.Char:
					return typeof(char);
				case TypeCode.DateTime:
					return typeof(DateTime);
				case TypeCode.DBNull:
					return typeof(DBNull);
				case TypeCode.Decimal:
					return typeof(decimal);
				case TypeCode.Double:
					return typeof(double);
				case TypeCode.Empty:
					return null;
				case TypeCode.Int16:
					return typeof(short);
				case TypeCode.Int32:
					return typeof(int);
				case TypeCode.Int64:
					return typeof(long);
				case TypeCode.SByte:
					return typeof(sbyte);
				case TypeCode.Single:
					return typeof(float);
				case TypeCode.String:
					return typeof(string);
				case TypeCode.UInt16:
					return typeof(ushort);
				case TypeCode.UInt32:
					return typeof(uint);
				case TypeCode.UInt64:
					return typeof(ulong);
				default:
					return typeof(string);
				}
			}
			set
			{
				if (_typeCode != TypeCode.Object)
				{
					throw new InvalidOperationException("The TypeCode property must be set to TypeCode.Object in order to set this property");
				}
				if (_type == null || _type != value)
				{
					_type = value;
				}
			}
		}

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
		[Description("Represents the key used to identify this object in a colletion.")]
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

		[Description("Indicates whether the value of the field is also copied when the user is copying an appointment.")]
		[DefaultValue(false)]
		public bool IncludeInCopy
		{
			get
			{
				return _includeInCopy;
			}
			set
			{
				_includeInCopy = value;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_dataMember = (string)LayoutReader.GetPropertyValue(info, "DataMember", typeof(string), string.Empty);
			_dataMemberMapping = (DataMemberMapping)LayoutReader.GetPropertyValue(info, "DataMemberMapping", typeof(DataMemberMapping), DataMemberMapping.Custom);
			_typeCode = (TypeCode)LayoutReader.GetPropertyValue(info, "TypeCode", typeof(TypeCode), TypeCode.String);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_includeInCopy = (bool)LayoutReader.GetPropertyValue(info, "IncludeInCopy", typeof(bool), false);
			string text = (string)LayoutReader.GetPropertyValue(info, "TypeName", typeof(string), null);
			if (text != null && TypeCode == TypeCode.Object)
			{
				try
				{
					Type = LayoutTypeResolver.Resolve(text);
					return;
				}
				catch
				{
					_type = null;
					return;
				}
			}
			_type = null;
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_dataMember != string.Empty)
			{
				info.AddValue("DataMember", _dataMember);
			}
			if (_dataMemberMapping != DataMemberMapping.Custom)
			{
				info.AddValue("DataMemberMapping", _dataMemberMapping);
			}
			if (_key != string.Empty)
			{
				info.AddValue("Key", _key);
			}
			if (TypeCode != TypeCode.String)
			{
				info.AddValue("TypeCode", _typeCode);
			}
			if (TypeCode == TypeCode.Object && Type != typeof(object))
			{
				info.AddValue("TypeName", Type.FullName);
			}
			if (_includeInCopy)
			{
				info.AddValue("IncludeInCopy", _includeInCopy);
			}
		}

		public SchedulerField()
		{
		}

		public SchedulerField(string dataMember)
		{
			_dataMember = dataMember;
			_key = dataMember;
		}

		internal PropertyDescriptor FindPropertyDescriptor()
		{
			try
			{
				if (Scheduler != null)
				{
					return Scheduler.GetCurrencyManager().GetItemProperties().Find(_dataMember, ignoreCase: true);
				}
			}
			catch
			{
				return null;
			}
			return null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetType()
		{
			if (_typeCode == TypeCode.Object)
			{
				_type = null;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeType()
		{
			if (_typeCode == TypeCode.Object)
			{
				return _type != null;
			}
			return false;
		}
	}
}
