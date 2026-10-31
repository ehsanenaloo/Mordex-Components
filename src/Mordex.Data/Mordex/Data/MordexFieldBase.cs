using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	public partial class MordexFieldBase : MordexCollectionElement, IMordexField, IValueConverter
	{
		internal bool _isListItemValue;

		private FilterEditType _filterEditType;

		private FormatMode _groupFormatMode;

		private string _groupFormatString;

		private string _groupEmptyString;

		private MaskFormatter _maskState;

		private FormatMode _formatMode;

		private string _formatString;

		private string _nullText;

		private string _mask;

		private IComparer _sortComparer;

		// Changes with every setting that changes how two values compare; totals kept by the aggregate cache depend on it.
		private int _compareVersion;

		private ColumnBoundMode _boundMode;

		private GroupInterval _groupInterval;

		private string _dataMember;

		private string _key;

		private CompareTarget _compareTarget;

		private CompareTarget _multipleValuesSort;

		private bool _compareIgnoreCase;

		private bool _multipleValues;

		private PropertyDescriptor _propertyDescriptor;

		private IMordexTable _multipleValuesTable;

		private string _multipleValuesDataMember;

		private string _displayName;

		private bool _hasValueList;

		private MordexValueItemCollectionBase _valueList;

		private IComparer _groupComparer;

		private MordexCustomGroupBase _customGroup;

		private TypeCode _dataTypeCode;

		private string _filterBuilderDisplayName;

		private bool _isCommittingListValue;

		protected TypeCode BaseDataTypeCode
		{
			get
			{
				if (_multipleValues)
				{
					IMordexTable mordexTable = GetMultipleValuesTable();
					if (mordexTable != null && mordexTable.Fields.Count > 0)
					{
						return mordexTable.Fields[0].DataTypeCode;
					}
				}
				PropertyDescriptor basePropertyDescriptor = BasePropertyDescriptor;
				if (basePropertyDescriptor != null)
				{
					return GetDataTypeCode(_propertyDescriptor.PropertyType);
				}
				return _dataTypeCode;
			}
			set
			{
				_dataTypeCode = value;
			}
		}

		protected virtual FilterEditType BaseDefaultFilterEditType
		{
			get
			{
				if (BaseDataTypeCode == TypeCode.DateTime && _compareTarget == CompareTarget.Value)
				{
					return FilterEditType.DatePicker;
				}
				if (BaseDataTypeCode == TypeCode.String || _compareTarget == CompareTarget.Text)
				{
					return FilterEditType.ComboBox;
				}
				if (BaseHasValueList)
				{
					return FilterEditType.ComboBox;
				}
				return FilterEditType.EditBox;
			}
		}

		protected virtual FilterEditType BaseFilterEditType
		{
			get
			{
				return _filterEditType;
			}
			set
			{
				_filterEditType = value;
			}
		}

		protected IComparer BaseGroupComparer
		{
			get
			{
				return _groupComparer;
			}
			set
			{
				_groupComparer = value;
			}
		}

		protected ColumnBoundMode BaseBoundMode
		{
			get
			{
				IMordexTable baseTable = BaseTable;
				if (baseTable != null && baseTable.DataSourceManager != null && baseTable.DataSourceManager.BoundMode == BoundMode.Unbound && _boundMode != ColumnBoundMode.UnboundFetch)
				{
					return ColumnBoundMode.UnboundKeepValues;
				}
				return _boundMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ColumnBoundMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_boundMode = value;
			}
		}

		protected FormatMode BaseGroupFormatMode
		{
			get
			{
				if (_groupFormatMode == FormatMode.UseDefault)
				{
					return _formatMode;
				}
				return _groupFormatMode;
			}
			set
			{
				_groupFormatMode = value;
			}
		}

		protected string BaseGroupFormatString
		{
			get
			{
				if (_groupFormatString == null)
				{
					return _formatString;
				}
				return _groupFormatString;
			}
			set
			{
				_groupFormatString = value;
			}
		}

		protected FormatMode BaseFormatMode
		{
			get
			{
				return _formatMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FormatMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == FormatMode.UseDefault)
				{
					value = FormatMode.UseIFormattable;
				}
				_formatMode = value;
			}
		}

		protected string BaseGroupEmptyString
		{
			get
			{
				return _groupEmptyString;
			}
			set
			{
				_groupEmptyString = value;
			}
		}

		protected string BaseMask
		{
			get
			{
				return _mask;
			}
			set
			{
				if (_mask == null)
				{
					_mask = "";
				}
				if (_mask != value)
				{
					_mask = value;
					ResetMaskState();
				}
			}
		}

		protected string BaseFormatString
		{
			get
			{
				return _formatString;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				_formatString = value;
			}
		}

		protected string BaseNullText
		{
			get
			{
				return _nullText;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				_nullText = value;
			}
		}

		internal int CompareVersion => _compareVersion;

		internal System.Globalization.CultureInfo CompareCulture => DataValueHelper.GetCulture(GetDataSourceManager());

		protected IComparer BaseSortComparer
		{
			get
			{
				return _sortComparer;
			}
			set
			{
				_sortComparer = value;
				_compareVersion++;
			}
		}

		protected bool BaseHasValueList
		{
			get
			{
				return _hasValueList;
			}
			set
			{
				if (_hasValueList != value)
				{
					if (_hasValueList)
					{
						_valueList = null;
					}
					_hasValueList = value;
					if (_hasValueList)
					{
						_valueList = CreateValueList();
					}
				}
			}
		}

		protected GroupInterval BaseGroupInterval
		{
			get
			{
				return _groupInterval;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GroupInterval), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == GroupInterval.Default)
				{
					value = GroupInterval.Value;
				}
				_groupInterval = value;
			}
		}

		protected MordexValueItemCollectionBase BaseValueList => _valueList;

		protected string BaseDataMember
		{
			get
			{
				return _dataMember;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (_dataMember != value)
				{
					_dataMember = value;
				}
			}
		}

		protected string BaseDisplayName
		{
			get
			{
				return _displayName;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (_displayName != value)
				{
					_displayName = value;
					OnDisplayNameChanged();
				}
			}
		}

		protected string BaseFilterBuilderDisplayName
		{
			get
			{
				return _filterBuilderDisplayName;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (_filterBuilderDisplayName != value)
				{
					_filterBuilderDisplayName = value;
				}
			}
		}

		protected string BaseKey
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

		protected PropertyDescriptor BasePropertyDescriptor
		{
			get
			{
				if (BaseBoundMode == ColumnBoundMode.Bound && _propertyDescriptor == null && BaseTable != null)
				{
					_propertyDescriptor = BaseTable.FindPropertyDescriptor(_dataMember);
				}
				return _propertyDescriptor;
			}
		}

		protected MordexTableBase BaseTable
		{
			get
			{
				if (base.CollectionOwner == null)
				{
					return null;
				}
				return (MordexTableBase)base.CollectionOwner;
			}
		}

		protected bool BaseCompareIgnoreCase
		{
			get
			{
				return _compareIgnoreCase;
			}
			set
			{
				if (_compareIgnoreCase != value)
				{
					_compareIgnoreCase = value;
					_compareVersion++;
				}
			}
		}

		protected CompareTarget BaseCompareTarget
		{
			get
			{
				return _compareTarget;
			}
			set
			{
				if (_compareTarget != value)
				{
					_compareTarget = value;
					_compareVersion++;
				}
			}
		}

		protected bool BaseMultipleValues
		{
			get
			{
				return _multipleValues;
			}
			set
			{
				if (_multipleValues != value)
				{
					_multipleValues = value;
					if (_multipleValuesTable != null)
					{
						_multipleValuesTable = null;
					}
				}
			}
		}

		protected CompareTarget BaseMultipleValuesSort
		{
			get
			{
				return _multipleValuesSort;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CompareTarget), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_multipleValuesSort != value)
				{
					_multipleValuesSort = value;
				}
			}
		}

		protected string BaseMultipleValuesDataMember
		{
			get
			{
				return _multipleValuesDataMember;
			}
			set
			{
				if (_multipleValuesDataMember != value)
				{
					_multipleValuesDataMember = value;
					if (_multipleValuesTable != null)
					{
						_multipleValuesTable = null;
					}
				}
			}
		}

		protected MordexCustomGroupBase BaseCustomGroup
		{
			get
			{
				return _customGroup;
			}
			set
			{
				if (value != null)
				{
					if (value.GetOwningCollection().GetOwner() != base.CollectionOwner)
					{
						throw new InvalidOperationException("Only CustomGroups that belong to the same table can be used.");
					}
					_customGroup = value;
				}
			}
		}

		bool IMordexField.CompareIgnoreCase
		{
			get
			{
				return BaseCompareIgnoreCase;
			}
			set
			{
				BaseCompareIgnoreCase = value;
			}
		}

		string IMordexField.DataMember
		{
			get
			{
				return BaseDataMember;
			}
			set
			{
				BaseDataMember = value;
			}
		}

		string IMordexField.DisplayName
		{
			get
			{
				return BaseDisplayName;
			}
			set
			{
				BaseDisplayName = value;
			}
		}

		string IMordexField.FilterBuilderDisplayName
		{
			get
			{
				return BaseFilterBuilderDisplayName;
			}
			set
			{
				BaseFilterBuilderDisplayName = value;
			}
		}

		CompareTarget IMordexField.CompareTarget
		{
			get
			{
				return BaseCompareTarget;
			}
			set
			{
				BaseCompareTarget = value;
			}
		}

		bool IMordexField.MultipleValues
		{
			get
			{
				return BaseMultipleValues;
			}
			set
			{
				BaseMultipleValues = value;
			}
		}

		string IMordexField.Key
		{
			get
			{
				return BaseKey;
			}
			set
			{
				BaseKey = value;
			}
		}

		TypeCode IMordexField.DataTypeCode
		{
			get
			{
				return BaseDataTypeCode;
			}
			set
			{
				BaseDataTypeCode = value;
			}
		}

		IMordexTable IMordexField.Table => BaseTable;

		string IMordexField.MultipleValuesDataMember
		{
			get
			{
				return BaseMultipleValuesDataMember;
			}
			set
			{
				BaseMultipleValuesDataMember = value;
			}
		}

		FormatMode IMordexField.FormatMode
		{
			get
			{
				return BaseFormatMode;
			}
			set
			{
				BaseFormatMode = value;
			}
		}

		string IMordexField.FormatString
		{
			get
			{
				return BaseFormatString;
			}
			set
			{
				BaseFormatString = value;
			}
		}

		string IMordexField.NullText
		{
			get
			{
				return BaseNullText;
			}
			set
			{
				BaseNullText = value;
			}
		}

		string IMordexField.Mask
		{
			get
			{
				return BaseMask;
			}
			set
			{
				BaseMask = value;
			}
		}

		IComparer IMordexField.GroupComparer
		{
			get
			{
				return BaseGroupComparer;
			}
			set
			{
				BaseGroupComparer = value;
			}
		}

		IMordexCustomGroup IMordexField.CustomGroup
		{
			get
			{
				return BaseCustomGroup;
			}
			set
			{
				BaseCustomGroup = (MordexCustomGroupBase)value;
			}
		}

		ColumnBoundMode IMordexField.BoundMode
		{
			get
			{
				return BaseBoundMode;
			}
			set
			{
				BaseBoundMode = value;
			}
		}

		string IMordexField.GroupEmptyString
		{
			get
			{
				return BaseGroupEmptyString;
			}
			set
			{
				BaseGroupEmptyString = value;
			}
		}

		string IMordexField.GroupFormatString
		{
			get
			{
				return BaseGroupFormatString;
			}
			set
			{
				BaseGroupFormatString = value;
			}
		}

		FormatMode IMordexField.GroupFormatMode
		{
			get
			{
				return BaseGroupFormatMode;
			}
			set
			{
				BaseGroupFormatMode = value;
			}
		}

		GroupInterval IMordexField.GroupInterval
		{
			get
			{
				return BaseGroupInterval;
			}
			set
			{
				BaseGroupInterval = value;
			}
		}

		IComparer IMordexField.SortComparer
		{
			get
			{
				return BaseSortComparer;
			}
			set
			{
				BaseSortComparer = value;
			}
		}

		FilterEditType IMordexField.FilterEditType
		{
			get
			{
				return BaseFilterEditType;
			}
			set
			{
				BaseFilterEditType = value;
			}
		}

		FilterEditType IMordexField.DefaultFilterEditType => BaseDefaultFilterEditType;

		bool IMordexField.HasValueList
		{
			get
			{
				return BaseHasValueList;
			}
			set
			{
				BaseHasValueList = value;
			}
		}

		IMordexValueItemCollection IMordexField.ValueList => BaseValueList;

		public MordexFieldBase()
		{
			SetDefaultFieldValues();
		}

		public MordexFieldBase(string dataMember)
		{
			SetDefaultFieldValues();
			Initialize(dataMember);
		}

		protected virtual void SetDefaultFieldValues()
		{
			_isListItemValue = false;
			_key = string.Empty;
			_dataMember = string.Empty;
			_displayName = string.Empty;
			_multipleValuesDataMember = string.Empty;
			_compareTarget = CompareTarget.Value;
			_compareIgnoreCase = true;
			_multipleValues = false;
			_groupInterval = GroupInterval.Value;
			_formatMode = FormatMode.UseIFormattable;
			_formatString = "";
			_nullText = "";
			_mask = "";
			_groupEmptyString = "(none)";
			_groupFormatMode = FormatMode.UseDefault;
			_groupFormatString = null;
			_dataTypeCode = TypeCode.Object;
			_filterEditType = FilterEditType.Default;
			_boundMode = ColumnBoundMode.Bound;
			_filterBuilderDisplayName = string.Empty;
		}

		protected virtual MordexValueItemCollectionBase CreateValueList()
		{
			return MordexValueItemCollectionBase.CreateForOwner(this);
		}

		protected virtual void Initialize(string dataMember)
		{
			_key = dataMember;
			_dataMember = dataMember;
			_displayName = Regex.Replace(dataMember, "(\\p{Ll})(\\p{Lu})|_+", "$1 $2");
		}

		protected virtual void Initialize(PropertyDescriptor propertyDescriptor)
		{
			if (propertyDescriptor != null)
			{
				_key = propertyDescriptor.Name;
				TypeCode dataTypeCode = GetDataTypeCode(propertyDescriptor.PropertyType);
				switch (dataTypeCode)
				{
				case TypeCode.DateTime:
					_groupInterval = GroupInterval.Date;
					_formatString = "d";
					break;
				case TypeCode.Decimal:
					_formatString = "c";
					break;
				}
				_dataTypeCode = dataTypeCode;
				_displayName = Regex.Replace(propertyDescriptor.DisplayName, "(\\p{Ll})(\\p{Lu})|_+", "$1 $2");
			}
			else
			{
				_key = "";
			}
			_dataMember = _key;
		}

		protected TypeCode GetDataTypeCode(Type type)
		{
			if (type == null)
			{
				return TypeCode.Object;
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				Type[] genericArguments = type.GetGenericArguments();
				if (genericArguments.Length == 1)
				{
					Type underlyingType = genericArguments[0];
					if (underlyingType == null)
					{
						return TypeCode.Object;
					}
					return Type.GetTypeCode(underlyingType);
				}
				return TypeCode.Object;
			}
			return Type.GetTypeCode(type);
		}

		private DataSourceManager GetDataSourceManager()
		{
			return ((IMordexTable)BaseTable)?.DataSourceManager;
		}

		private MaskFormatter GetMaskState()
		{
			if (_maskState == null)
			{
				_maskState = new MaskFormatter(GetDataSourceManager());
				_maskState.SetMask(_mask);
			}
			return _maskState;
		}

		private void ResetMaskState()
		{
			_maskState = null;
		}

		protected bool BaseShouldSerializeGroupFormatMode()
		{
			return _groupFormatMode != FormatMode.UseDefault;
		}

		protected void BaseResetGroupFormatMode()
		{
			_groupFormatMode = FormatMode.UseDefault;
		}

		protected bool BaseShouldSerializeGroupFormatString()
		{
			return _groupFormatString != null;
		}

		protected void BaseResetGroupFormatString()
		{
			_groupFormatString = null;
		}

		protected virtual void OnDisplayNameChanged()
		{
		}

		protected virtual object GetUnboundValue(MordexRow row)
		{
			return null;
		}

		protected Image BaseGetImageValue(MordexRow row)
		{
			object rawValue = BaseGetValue(row);
			byte[] imageBytes = rawValue as byte[];
			Image image = null;
			if (imageBytes != null)
			{
				try
				{
					using MemoryStream memoryStream = new MemoryStream(imageBytes);
					using Image decoded = Image.FromStream(memoryStream);
					// A decoded image keeps reading its stream, so keep a copy.
					image = new Bitmap(decoded);
				}
				catch (ArgumentException)
				{
					image = null;
				}
			}
			if (image == null)
			{
				image = rawValue as Image;
			}
			return image;
		}

		private CultureInfo GetCulture()
		{
			DataSourceManager dataSourceManager = null;
			IMordexTable baseTable = BaseTable;
			if (baseTable != null)
			{
				dataSourceManager = baseTable.DataSourceManager;
			}
			return DataValueHelper.GetCulture(dataSourceManager);
		}

		protected object BaseGetMultipleValuesList(MordexRow row)
		{
			IList list = row.GetValue(this) as IList;
			IMordexTable mordexTable = GetMultipleValuesTable();
			if (list != null && mordexTable != null)
			{
				ArrayList arrayList = new ArrayList(list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					arrayList.Add(((MordexFieldBase)mordexTable.Fields[0]).GetItemValue(list[i]));
				}
				object[] sortedValues = new object[arrayList.Count];
				if (_multipleValuesSort == CompareTarget.Text)
				{
					if (arrayList.Count > 0)
					{
						ArrayList valueItems = new ArrayList(arrayList.Count);
						for (int j = 0; j < arrayList.Count; j++)
						{
							valueItems.Add(new MordexValueItemBase(arrayList[j], FormatValue(arrayList[j]), -1));
						}
						try
						{
							valueItems.Sort(new ValueItemComparer(GetCulture(), useValue: false));
						}
						catch (InvalidOperationException)
						{
							// Values that cannot be compared keep their original order.
						}
						for (int k = 0; k < valueItems.Count; k++)
						{
							sortedValues[k] = MordexValueItemBase.GetValueOrNull((MordexValueItemBase)valueItems[k]);
						}
					}
				}
				else
				{
					try
					{
						arrayList.Sort();
					}
					catch (InvalidOperationException)
					{
						// Values that cannot be compared keep their original order.
					}
					for (int l = 0; l < arrayList.Count; l++)
					{
						sortedValues[l] = arrayList[l];
					}
				}
				return sortedValues;
			}
			return new object[0];
		}
	}
}
