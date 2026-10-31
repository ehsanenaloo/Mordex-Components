using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	public class MordexGroupBase : MordexCollectionElement, IMordexGroup
	{
		private class GroupComparisonSnapshot
		{
			internal IComparer _comparer;

			internal CultureInfo _cultureInfo;

			internal MordexTableBase _groupTable;

			internal void ClearCache()
			{
				_comparer = null;
				_cultureInfo = null;
			}
		}

		protected const int ErrorCodeFieldNotInTable = 11;

		protected const int ErrorCodeFieldNull = 12;

		protected const int ErrorCodeGroupCustom = 20;

		private string _multipleFieldSeparator;

		private IMordexField _field;

		private SortOrder _sortOrder;

		private GroupInterval _groupInterval;

		private FormatMode _groupFormatMode;

		private string _groupFormatString;

		private MordexCustomGroupBase _customGroup;

		private GroupTotalVisibility _groupTotalVisibility;

		private OutlookDateRangeCalculator _outlookDateCalculator;

		private GroupComparisonSnapshot _comparisonSnapshot;

		private GroupKeyComparer _compositeGroupComparer;

		private DataSourceManagerComparer _dataSourceManagerComparer;

		protected GroupTotalVisibility BaseGroupTotalVisibility
		{
			get
			{
				return _groupTotalVisibility;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GroupTotalVisibility), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_groupTotalVisibility != value)
				{
					_groupTotalVisibility = value;
					if (base.CollectionOwner is MordexTableBase mordexTableBase && mordexTableBase.GetDataSourceManager() != null)
					{
						mordexTableBase.GetDataSourceManager().RefreshRowManagers();
					}
				}
			}
		}

		protected MordexFieldBase[] BaseFields
		{
			get
			{
				IMordexCustomGroup mordexCustomGroup = _customGroup;
				if (mordexCustomGroup != null)
				{
					if (mordexCustomGroup.CustomGroupType == CustomGroupType.CompositeColumns)
					{
						return _customGroup.GetCompositeFields();
					}
				}
				else if (GetEffectiveField() != null)
				{
					return new MordexFieldBase[1] { (MordexFieldBase)GetEffectiveField() };
				}
				return null;
			}
		}

		protected MordexFieldBase BaseField
		{
			get
			{
				return (MordexFieldBase)GetEffectiveField();
			}
			set
			{
				if (value == null && base.CollectionOwner != null)
				{
					throw new ArgumentNullException(GetErrorMessage(12));
				}
				if (GetEffectiveField() != value)
				{
					if (!IsValidField(value))
					{
						throw new ArgumentException(GetErrorMessage(11));
					}
					if (OnSettingsChanging())
					{
						_field = value;
						_customGroup = null;
						OnSettingsChanged();
					}
				}
			}
		}

		protected string BaseMultipleFieldSeparator
		{
			get
			{
				return _multipleFieldSeparator;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_multipleFieldSeparator != value)
				{
					_multipleFieldSeparator = value;
				}
			}
		}

		protected GroupInterval BaseGroupInterval
		{
			get
			{
				if (_groupInterval == GroupInterval.Default && GetEffectiveField() != null)
				{
					return GetEffectiveField().GroupInterval;
				}
				return _groupInterval;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GroupInterval), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_groupInterval != value && OnSettingsChanging())
				{
					_groupInterval = value;
					OnSettingsChanged();
				}
			}
		}

		protected SortOrder BaseSortOrder
		{
			get
			{
				return _sortOrder;
			}
			set
			{
				if (!Enum.IsDefined(typeof(SortOrder), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_sortOrder != value && OnSettingsChanging())
				{
					_sortOrder = value;
					OnSettingsChanged();
				}
			}
		}

		protected FormatMode BaseGroupFormatMode
		{
			get
			{
				if (_groupFormatMode == FormatMode.UseDefault && GetEffectiveField() != null)
				{
					return GetEffectiveField().GroupFormatMode;
				}
				return _groupFormatMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FormatMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_groupFormatMode != value && OnSettingsChanging())
				{
					_groupFormatMode = value;
					OnSettingsChanged();
				}
			}
		}

		protected string BaseGroupFormatString
		{
			get
			{
				if (_groupFormatString == null)
				{
					if (GetEffectiveField() != null)
					{
						return GetEffectiveField().GroupFormatString;
					}
					return "";
				}
				return _groupFormatString;
			}
			set
			{
				if (_groupFormatString != value)
				{
					_groupFormatString = value;
					OnSettingsChanged();
				}
			}
		}

		protected MordexTableBase BaseTable => (MordexTableBase)base.CollectionOwner;

		protected MordexCustomGroupBase BaseCustomGroup
		{
			get
			{
				return _customGroup;
			}
			set
			{
				if (value == null && base.CollectionOwner != null)
				{
					throw new ArgumentException(GetErrorMessage(12));
				}
				if (_customGroup != value && OnSettingsChanging())
				{
					if (value != null)
					{
						_field = null;
					}
					_customGroup = value;
					OnSettingsChanged();
				}
			}
		}

		IMordexField IMordexGroup.Field
		{
			get
			{
				return BaseField;
			}
			set
			{
				BaseField = (MordexFieldBase)value;
			}
		}

		IMordexField[] IMordexGroup.Fields => BaseFields;

		IMordexTable IMordexGroup.Table => BaseTable;

		SortOrder IMordexGroup.SortOrder
		{
			get
			{
				return BaseSortOrder;
			}
			set
			{
				BaseSortOrder = value;
			}
		}

		GroupInterval IMordexGroup.GroupInterval
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

		FormatMode IMordexGroup.GroupFormatMode
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

		string IMordexGroup.GroupFormatString
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

		IMordexCustomGroup IMordexGroup.CustomGroup
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

		string IMordexGroup.MultipleFieldSeparator
		{
			get
			{
				return BaseMultipleFieldSeparator;
			}
			set
			{
				BaseMultipleFieldSeparator = value;
			}
		}

		internal static MordexGroupBase CreateDefault()
		{
			return new MordexGroupBase();
		}

		protected MordexGroupBase()
			: this(null)
		{
		}

		protected MordexGroupBase(MordexFieldBase field)
			: this(field, SortOrder.Ascending)
		{
		}

		protected MordexGroupBase(MordexFieldBase field, SortOrder sortOrder)
		{
			_sortOrder = sortOrder;
			_field = field;
			_groupInterval = GroupInterval.Default;
			_groupFormatMode = FormatMode.UseDefault;
			_multipleFieldSeparator = "-";
			_groupTotalVisibility = GroupTotalVisibility.Default;
		}

		private IMordexField GetEffectiveField()
		{
			if (_customGroup != null)
			{
				IMordexCustomGroup mordexCustomGroup = _customGroup;
				if (mordexCustomGroup != null && mordexCustomGroup.CustomGroupType == CustomGroupType.CompositeColumns && mordexCustomGroup.CompositeFields != null && mordexCustomGroup.CompositeFields.Length == 1)
				{
					return mordexCustomGroup.CompositeFields[0];
				}
			}
			return _field;
		}

		internal GroupTotalVisibility GetEffectiveTotalVisibility()
		{
			if (_groupTotalVisibility == GroupTotalVisibility.Default)
			{
				if (base.CollectionOwner is MordexTableBase mordexTableBase)
				{
					switch (mordexTableBase.GetEffectiveGroupTotals())
					{
					case GroupTotals.Always:
						return GroupTotalVisibility.Visible;
					case GroupTotals.ExpandedRow:
						return GroupTotalVisibility.VisibleWhenExpanded;
					default:
						return GroupTotalVisibility.Hidden;
					}
				}
				return GroupTotalVisibility.Hidden;
			}
			return _groupTotalVisibility;
		}

		internal MordexTableBase GetGroupTable()
		{
			if (_comparisonSnapshot != null)
			{
				return _comparisonSnapshot._groupTable;
			}
			if (GetEffectiveField() != null)
			{
				if (GetEffectiveField().MultipleValues)
				{
					return (MordexTableBase)((MordexFieldBase)GetEffectiveField()).GetMultipleValuesTable();
				}
				return (MordexTableBase)GetEffectiveField().Table;
			}
			if (BaseFields != null && BaseFields.Length > 0)
			{
				return (MordexTableBase)((IMordexField)BaseFields[0]).Table;
			}
			return null;
		}

		internal MordexFieldBase GetValueField()
		{
			return ResolveValueField(GetEffectiveField());
		}

		internal MordexFieldBase ResolveValueField(IMordexField iMordexField)
		{
			if (iMordexField != null && iMordexField.MultipleValues)
			{
				IMordexTable mordexTable = ((MordexFieldBase)iMordexField).GetMultipleValuesTable();
				if (mordexTable.Fields.Count > 0)
				{
					return (MordexFieldBase)mordexTable.Fields[0];
				}
				return null;
			}
			return (MordexFieldBase)iMordexField;
		}

		protected bool ShouldSerializeBaseGroupInterval()
		{
			return _groupInterval != GroupInterval.Default;
		}

		protected virtual bool IsValidField(MordexFieldBase field)
		{
			if (BaseTable != null)
			{
				return BaseTable.CheckCanGroupByField(field);
			}
			return true;
		}

		protected bool BaseShouldSerializeGroupFormatMode()
		{
			return _groupFormatMode != FormatMode.UseDefault;
		}

		protected void BaseResetGroupFormatMode()
		{
			BaseGroupFormatMode = FormatMode.UseDefault;
		}

		protected bool BaseShouldSerializeGroupFormatString()
		{
			return _groupFormatString != null;
		}

		protected void BaseResetGroupFormatString()
		{
			BaseGroupFormatString = null;
		}

		protected override string GetErrorMessage(int errorCode)
		{
			switch (errorCode)
			{
			case 11:
				return "Can not group by this field.";
			case 12:
				return "Field property can not be null (Nothing) in a Group that is part of a collection.";
			default:
				return base.GetErrorMessage(errorCode);
			}
		}

		internal object GetGroupValue(DataHolderRow sourceRow)
		{
			MordexFieldBase[] compositeFields = null;
			if (_customGroup != null && ((IMordexCustomGroup)_customGroup).CustomGroupType == CustomGroupType.CompositeColumns)
			{
				compositeFields = _customGroup.GetCompositeFields();
			}
			if (compositeFields == null)
			{
				MordexFieldBase mordexFieldBase = GetValueField();
				DataHolderRow dataHolderRow = null;
				IMordexTable table = ((IMordexField)mordexFieldBase).Table;
				MordexTableBase mordexTableBase = (MordexTableBase)table;
				if (mordexTableBase.IsSelfReferencingTable() && table.SelfReferencingSettings.HierarchicalGroupMode == HierarchicalGroupMode.ParentRows)
				{
					dataHolderRow = sourceRow.FindHolderForTable(mordexTableBase);
					dataHolderRow = dataHolderRow.GetTopmostSameTableHolder();
				}
				else
				{
					dataHolderRow = sourceRow.FindHolderForTable(mordexTableBase);
				}
				return mordexFieldBase.GetRowValue(dataHolderRow);
			}
			object[] compositeValues = new object[compositeFields.Length];
			DataHolderRow fieldRow = null;
			for (int i = 0; i < compositeFields.Length; i++)
			{
				MordexFieldBase compositeField = ResolveValueField(compositeFields[i]);
				IMordexTable fieldTable = ((IMordexField)compositeField).Table;
				MordexTableBase fieldTableBase = (MordexTableBase)fieldTable;
				if (fieldTableBase.IsSelfReferencingTable() && fieldTable.SelfReferencingSettings.HierarchicalGroupMode == HierarchicalGroupMode.ParentRows)
				{
					fieldRow = sourceRow.FindHolderForTable(fieldTableBase);
					fieldRow = fieldRow.GetTopmostSameTableHolder();
				}
				else
				{
					fieldRow = sourceRow.FindHolderForTable(fieldTableBase);
				}
				compositeValues[i] = compositeField.GetRowValue(fieldRow);
			}
			return compositeValues;
		}

		private MordexFieldBase[] GetCompositeFields()
		{
			IMordexCustomGroup mordexCustomGroup = _customGroup;
			if (mordexCustomGroup != null && mordexCustomGroup.CustomGroupType == CustomGroupType.CompositeColumns)
			{
				return _customGroup.GetCompositeFields();
			}
			return null;
		}

		private object GetFieldGroupValue(MordexFieldBase mordexFieldBase, DataHolderRow dataHolderRow)
		{
			if (dataHolderRow == null)
			{
				return DBNull.Value;
			}
			mordexFieldBase = ResolveValueField(mordexFieldBase);
			GroupInterval groupInterval = BaseGroupInterval;
			if (GetCompositeFields() != null)
			{
				groupInterval = ((IMordexField)mordexFieldBase).GroupInterval;
			}
			switch (groupInterval)
			{
			case GroupInterval.Alphabetical:
			{
				string text = mordexFieldBase.GetRowText(dataHolderRow);
				if (text.Length > 0)
				{
					return text.Substring(0, 1).ToUpper();
				}
				return "";
			}
			case GroupInterval.Value:
				return mordexFieldBase.GetRowValue(dataHolderRow);
			case GroupInterval.Text:
			{
				object fieldValue = mordexFieldBase.GetRowValue(dataHolderRow);
				if (fieldValue == null || fieldValue == DBNull.Value || (fieldValue is string && ((string)fieldValue).Length == 0))
				{
					return ((IMordexField)mordexFieldBase).GroupEmptyString;
				}
				return mordexFieldBase.GetRowText(dataHolderRow);
			}
			case GroupInterval.Date:
				return GetDateOnly(mordexFieldBase, dataHolderRow);
			case GroupInterval.Year:
				return GetYearValue(mordexFieldBase, dataHolderRow);
			case GroupInterval.Month:
				return GetMonthValue(mordexFieldBase, dataHolderRow);
			case GroupInterval.Quarter:
				return GetQuarterValue(mordexFieldBase, dataHolderRow);
			case GroupInterval.Hour:
				return GetHourValue(mordexFieldBase, dataHolderRow);
			case GroupInterval.Minute:
				return GetMinuteValue(mordexFieldBase, dataHolderRow);
			case GroupInterval.Second:
				return GetSecondValue(mordexFieldBase, dataHolderRow);
			case GroupInterval.OutlookDates:
				return GetOutlookDateRange(mordexFieldBase, dataHolderRow);
			default:
				return DBNull.Value;
			}
		}

		internal object GetGroupValueFromBaseFields(DataHolderRow sourceRow)
		{
			MordexFieldBase[] baseFields = BaseFields;
			DataHolderRow dataHolderRow = null;
			MordexTableBase mordexTableBase = GetGroupTable();
			if (mordexTableBase.IsSelfReferencingTable() && ((IMordexTable)mordexTableBase).SelfReferencingSettings.HierarchicalGroupMode == HierarchicalGroupMode.ParentRows)
			{
				dataHolderRow = sourceRow.FindHolderForTable(mordexTableBase);
				dataHolderRow = dataHolderRow.GetTopmostSameTableHolder();
			}
			else
			{
				dataHolderRow = sourceRow.FindHolderForTable(mordexTableBase);
			}
			object[] fieldValues = new object[baseFields.Length];
			for (int i = 0; i < baseFields.Length; i++)
			{
				fieldValues[i] = GetFieldGroupValue(baseFields[i], dataHolderRow);
			}
			if (fieldValues.Length == 1)
			{
				return fieldValues[0];
			}
			return fieldValues;
		}

		private object GetDateValue(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			object rawValue = mordexFieldBase.GetRowValue(mordexRow);
			if (rawValue == null || rawValue == DBNull.Value)
			{
				return DBNull.Value;
			}
			if (rawValue is DateTime)
			{
				return rawValue;
			}
			try
			{
				DateTime dateTime = Convert.ToDateTime(rawValue);
				return dateTime;
			}
			catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException)
			{
				return DBNull.Value;
			}
		}

		private object GetDateOnly(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			if (GetDateValue(mordexFieldBase, mordexRow) is DateTime dateTime)
			{
				return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
			}
			return DBNull.Value;
		}

		private OutlookDateRangeCalculator GetOutlookDateCalculator()
		{
			if (_outlookDateCalculator == null || _outlookDateCalculator.Today != DateTime.Today)
			{
				_outlookDateCalculator = new OutlookDateRangeCalculator(GetEffectiveField().Table.DataSourceManager);
			}
			return _outlookDateCalculator;
		}

		private OutlookDateRange GetOutlookDateRange(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			if (GetDateValue(mordexFieldBase, mordexRow) is DateTime dateTime)
			{
				return GetOutlookDateCalculator().FindRange(dateTime);
			}
			return OutlookDateRange.NullDate;
		}

		internal string FormatGroupText(object value)
		{
			if (GetCompositeFields() != null && value is IList list && list.Count == GetCompositeFields().Length)
			{
				string text = "";
				for (int i = 0; i < GetCompositeFields().Length; i++)
				{
					IMordexField mordexField = GetCompositeFields()[i];
					if (i > 0)
					{
						text = text + " " + _multipleFieldSeparator + " ";
					}
					text += FormatGroupValue(mordexField, list[i], mordexField.GroupInterval, mordexField.GroupFormatMode, mordexField.GroupFormatString);
				}
				return text;
			}
			return FormatGroupValue(GetEffectiveField(), value, BaseGroupInterval, BaseGroupFormatMode, BaseGroupFormatString);
		}

		private string FormatGroupValue(IMordexField iMordexField, object value, GroupInterval groupInterval, FormatMode formatMode, string format)
		{
			if (value is OutlookDateRange)
			{
				return GetOutlookDateCalculator().GetName((OutlookDateRange)value);
			}
			switch (groupInterval)
			{
			case GroupInterval.Quarter:
			{
				int quarter = 0;
				int year = 0;
				if (value is DateTime date)
				{
					quarter = (date.Month - 1) / 3 + 1;
					year = date.Year;
				}
				if (year != 0 && quarter != 0)
				{
					if (formatMode == FormatMode.UseStringFormat)
					{
						return string.Format(GetCulture(), format, quarter, year);
					}
					return string.Format(GetCulture(), "{0} {1}", quarter, year);
				}
				break;
			}
			case GroupInterval.Alphabetical:
				if (value != DBNull.Value)
				{
					string text = ((MordexFieldBase)iMordexField).FormatWithGroupSettings(value, formatMode, format, iMordexField.GroupEmptyString);
					if (text.Length > 0)
					{
						return text.Substring(0, 1).ToUpper();
					}
				}
				break;
			}
			return ((MordexFieldBase)iMordexField).FormatWithGroupSettings(value, formatMode, format, iMordexField.GroupEmptyString);
		}

		private object GetHourValue(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			if (GetDateValue(mordexFieldBase, mordexRow) is DateTime dateTime)
			{
				return new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, dateTime.Hour, 0, 0);
			}
			return DBNull.Value;
		}

		private object GetMinuteValue(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			if (GetDateValue(mordexFieldBase, mordexRow) is DateTime dateTime)
			{
				return new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, dateTime.Hour, dateTime.Minute, 0);
			}
			return DBNull.Value;
		}

		private object GetSecondValue(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			if (GetDateValue(mordexFieldBase, mordexRow) is DateTime dateTime)
			{
				return new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
			}
			return DBNull.Value;
		}

		private object GetYearValue(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			if (GetDateValue(mordexFieldBase, mordexRow) is DateTime dateTime)
			{
				return new DateTime(dateTime.Year, 1, 1);
			}
			return DBNull.Value;
		}

		private object GetMonthValue(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			if (GetDateValue(mordexFieldBase, mordexRow) is DateTime dateTime)
			{
				return new DateTime(dateTime.Year, dateTime.Month, 1);
			}
			return DBNull.Value;
		}

		private object GetQuarterValue(MordexFieldBase mordexFieldBase, MordexRow mordexRow)
		{
			if (GetDateValue(mordexFieldBase, mordexRow) is DateTime dateTime)
			{
				int quarterIndex = (dateTime.Month - 1) / 3;
				return new DateTime(dateTime.Year, quarterIndex * 3 + 1, 1);
			}
			return DBNull.Value;
		}

		internal void CaptureComparisonSnapshot()
		{
			MordexTableBase baseTable = BaseTable;
			if (baseTable != null)
			{
				GroupComparisonSnapshot a = new GroupComparisonSnapshot();
				a._comparer = GetGroupComparer();
				a._cultureInfo = baseTable.GetDataSourceManager().CurrentCulture;
				a._groupTable = GetGroupTable();
				_comparisonSnapshot = a;
			}
		}

		internal void ReleaseComparisonSnapshot()
		{
			_comparisonSnapshot = null;
		}

		internal CultureInfo GetCulture()
		{
			if (_comparisonSnapshot != null)
			{
				return _comparisonSnapshot._cultureInfo;
			}
			MordexTableBase baseTable = BaseTable;
			if (baseTable != null)
			{
				DataSourceManager dataSourceManager = baseTable.GetDataSourceManager();
				if (dataSourceManager != null)
				{
					return dataSourceManager.CurrentCulture;
				}
			}
			return null;
		}

		internal IComparer GetGroupComparer()
		{
			if (_comparisonSnapshot != null)
			{
				return _comparisonSnapshot._comparer;
			}
			IComparer comparer = null;
			if (GetEffectiveField() != null)
			{
				comparer = GetEffectiveField().GroupComparer;
				if (comparer == null && GetEffectiveField().DataTypeCode == TypeCode.Object)
				{
					if (_dataSourceManagerComparer == null)
					{
						_dataSourceManagerComparer = new DataSourceManagerComparer(((IMordexTable)BaseTable).DataSourceManager);
					}
					comparer = _dataSourceManagerComparer;
				}
			}
			else if (GetCompositeFields() != null)
			{
				if (_compositeGroupComparer == null)
				{
					_compositeGroupComparer = new GroupKeyComparer(((IMordexTable)BaseTable).DataSourceManager, this);
				}
				comparer = _compositeGroupComparer;
			}
			return comparer;
		}
	}
}
