using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace Mordex.Data
{
	public partial class MordexTableBase : MordexCollectionElement, IMordexTable, ICollectionOwner, IConditionOwner
	{
		private string _key;

		private string _caption;

		private GroupMode _groupMode;

		private IMordexFieldCollection _fieldCollection;

		private IMordexSortFieldCollection _sortFieldCollection;

		private IMordexGroupCollection _groupCollection;

		private IMordexChildTableCollection _childTableCollection;

		private IMordexGroupTotalCollection _groupTotalCollection;

		private string _dataMember;

		private InheritableBoolean _allowChildTableGroups;

		private IFilterCondition _filterCondition;

		private InheritableBoolean _dynamicFiltering;

		private bool _filterParentRows;

		private MordexSelfReferencingSettingsBase _selfReferencingSettings;

		private IMordexField _ownerField;

		private GroupTotals _groupTotalRows;

		private NewRowPosition _newRowPosition;

		private InheritableBoolean _allowAddNew;

		private IMordexStoredFilterCollection _storedFilterCollection;

		private InheritableBoolean _totalRow;

		private HierarchicalMode _hierarchicalMode;

		private MordexCustomGroupCollectionBase _customGroupCollection;

		private bool _isApplyingFilter;

		private bool _isSortSuspended;

		internal bool _suppressCollectionEvents;

		private DataSourceManager _dataSourceManager;

		private PropertyDescriptor _propertyDescriptor;

		private string[] _dataKeyMembers;

		private EventHandler _columnsChanged;

		private EventHandler _filterConditionChanged;

		protected InheritableBoolean BaseAllowAddNew
		{
			get
			{
				return _allowAddNew;
			}
			set
			{
				if (_allowAddNew != value)
				{
					_allowAddNew = value;
					if (GetDataSourceManager() != null)
					{
						GetDataSourceManager().RefreshRowManagers();
					}
				}
			}
		}

		protected MordexFieldBase BaseOwnerField
		{
			get
			{
				return (MordexFieldBase)_ownerField;
			}
			set
			{
				_ownerField = value;
			}
		}

		protected InheritableBoolean BaseDynamicFiltering
		{
			get
			{
				return _dynamicFiltering;
			}
			set
			{
				_dynamicFiltering = value;
			}
		}

		protected GroupMode BaseGroupMode
		{
			get
			{
				return _groupMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GroupMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_groupMode = value;
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

		protected string BaseTableCaption
		{
			get
			{
				return _caption;
			}
			set
			{
				_caption = value;
			}
		}

		protected NewRowPosition BaseNewRowPosition
		{
			get
			{
				return _newRowPosition;
			}
			set
			{
				if (!Enum.IsDefined(typeof(NewRowPosition), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_newRowPosition != value)
				{
					_newRowPosition = value;
					if (GetDataSourceManager() != null)
					{
						GetDataSourceManager().RefreshRowManagers();
					}
				}
			}
		}

		protected IFilterCondition BaseFilterCondition
		{
			get
			{
				return _filterCondition;
			}
			set
			{
				if (value != null)
				{
					if (value.FilterCondition != null)
					{
						((MordexConditionBase)value.FilterCondition).ValidateTable(this);
					}
					if (value is IMordexStoredFilter mordexStoredFilter && mordexStoredFilter.Table != this)
					{
						throw new InvalidOperationException("The stored filter must belong to the table.");
					}
				}
				if (_filterCondition == value)
				{
					return;
				}
				if (_filterCondition != null)
				{
					if (_filterCondition is MordexConditionBase)
					{
						((MordexConditionBase)_filterCondition).SetConditionOwner(null);
					}
					else if (_filterCondition is MordexStoredFilterBase)
					{
						((MordexStoredFilterBase)_filterCondition).SetIsCurrent(false);
					}
				}
				_filterCondition = value;
				if (_filterCondition != null)
				{
					if (_filterCondition is MordexConditionBase)
					{
						((MordexConditionBase)_filterCondition).SetConditionOwner(this);
					}
					else if (_filterCondition is MordexStoredFilterBase)
					{
						((MordexStoredFilterBase)_filterCondition).SetIsCurrent(true);
					}
				}
				OnFilterConditionChanged();
				if (_dataSourceManager != null)
				{
					bool wasApplyingFilter = _isApplyingFilter;
					_isApplyingFilter = true;
					try
					{
						_dataSourceManager.RefreshTableFilter(this);
					}
					finally
					{
						_isApplyingFilter = wasApplyingFilter;
					}
				}
			}
		}

		protected string BaseDataMember
		{
			get
			{
				return _dataMember;
			}
			set
			{
				_dataMember = value;
			}
		}

		protected MordexFieldCollectionBase BaseFields => (MordexFieldCollectionBase)_fieldCollection;

		protected MordexCustomGroupCollectionBase BaseCustomGroups => _customGroupCollection;

		protected MordexChildTableCollectionBase BaseChildTables => (MordexChildTableCollectionBase)_childTableCollection;

		protected MordexSortFieldCollectionBase BaseSortFields => (MordexSortFieldCollectionBase)_sortFieldCollection;

		protected MordexStoredFilterCollectionBase BaseStoredFilters => (MordexStoredFilterCollectionBase)_storedFilterCollection;

		protected MordexGroupCollectionBase BaseGroups => (MordexGroupCollectionBase)_groupCollection;

		protected MordexGroupTotalCollectionBase BaseGroupTotals => (MordexGroupTotalCollectionBase)_groupTotalCollection;

		protected MordexTableBase BaseParentTable
		{
			get
			{
				if (_ownerField != null)
				{
					return (MordexTableBase)_ownerField.Table;
				}
				return (MordexTableBase)base.CollectionOwner;
			}
		}

		protected InheritableBoolean BaseTotalRow
		{
			get
			{
				return _totalRow;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new NotImplementedException();
				}
				if (_totalRow != value)
				{
					_totalRow = value;
					if (GetDataSourceManager() != null)
					{
						GetDataSourceManager().RefreshRowManagers();
					}
				}
			}
		}

		protected InheritableBoolean BaseAllowChildTableGroups
		{
			get
			{
				return _allowChildTableGroups;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_allowChildTableGroups = value;
			}
		}

		protected PropertyDescriptor PropertyDescriptor
		{
			get
			{
				if (_propertyDescriptor == null && BaseParentTable != null && _dataMember != null && _dataMember.Length > 0)
				{
					_propertyDescriptor = BaseParentTable.FindPropertyDescriptor(_dataMember);
				}
				return _propertyDescriptor;
			}
		}

		IMordexTable IConditionOwner.Table => this;

		protected bool BaseFilterParentRows
		{
			get
			{
				return _filterParentRows;
			}
			set
			{
				if (_filterParentRows != value)
				{
					_filterParentRows = value;
					if (_filterCondition != null && GetDataSourceManager() != null)
					{
						GetDataSourceManager().RefreshTableFilter(this);
					}
				}
			}
		}

		protected MordexSelfReferencingSettingsBase BaseSelfReferencingSettings
		{
			get
			{
				return _selfReferencingSettings;
			}
			set
			{
				if (value != _selfReferencingSettings)
				{
					if (_selfReferencingSettings != value)
					{
						_selfReferencingSettings.ReassignTable(null);
					}
					_selfReferencingSettings = value;
					_selfReferencingSettings.ReassignTable(this);
				}
			}
		}

		protected bool BaseIsSelfReferencingTable => _hierarchicalMode != HierarchicalMode.UseChildTables;

		protected GroupTotals BaseGroupTotalRows
		{
			get
			{
				return _groupTotalRows;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GroupTotals), value))
				{
					throw new NotImplementedException();
				}
				if (_groupTotalRows != value)
				{
					_groupTotalRows = value;
					if (GetDataSourceManager() != null)
					{
						GetDataSourceManager().RefreshRowManagers();
					}
				}
			}
		}

		protected bool BaseActAsSelfReferencingTable
		{
			get
			{
				if (IsSelfReferencingTable() && _groupCollection.Count > 0 && ((IMordexSelfReferencingSettings)_selfReferencingSettings).HierarchicalGroupMode == HierarchicalGroupMode.AllRows)
				{
					return false;
				}
				return true;
			}
		}

		protected HierarchicalMode BaseHierarchicalMode
		{
			get
			{
				return _hierarchicalMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(HierarchicalMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_hierarchicalMode != value)
				{
					_hierarchicalMode = value;
					if (GetDataSourceManager() != null)
					{
						GetDataSourceManager().Refresh(refreshAll: true);
					}
				}
			}
		}

		IMordexChildTableCollection IMordexTable.ChildTables => BaseChildTables;

		GroupMode IMordexTable.GroupMode
		{
			get
			{
				return BaseGroupMode;
			}
			set
			{
				BaseGroupMode = value;
			}
		}

		IMordexSortFieldCollection IMordexTable.SortFields => BaseSortFields;

		IMordexGroupCollection IMordexTable.Groups => BaseGroups;

		IMordexFieldCollection IMordexTable.Fields => BaseFields;

		IMordexStoredFilterCollection IMordexTable.StoredFilters => BaseStoredFilters;

		IMordexGroupTotalCollection IMordexTable.GroupTotals => BaseGroupTotals;

		bool IMordexTable.FilterParentRows
		{
			get
			{
				return BaseFilterParentRows;
			}
			set
			{
				BaseFilterParentRows = value;
			}
		}

		string IMordexTable.Key
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

		string IMordexTable.DataMember
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

		string IMordexTable.TableCaption
		{
			get
			{
				return BaseTableCaption;
			}
			set
			{
				BaseTableCaption = value;
			}
		}

		InheritableBoolean IMordexTable.TotalRow
		{
			get
			{
				return BaseTotalRow;
			}
			set
			{
				BaseTotalRow = value;
			}
		}

		IMordexTable IMordexTable.ParentTable => BaseParentTable;

		IMordexSelfReferencingSettings IMordexTable.SelfReferencingSettings => BaseSelfReferencingSettings;

		bool IMordexTable.IsAuxiliaryTable => IsMultipleValuesTable();

		bool IMordexTable.IsFiltered => HasFilterInTree();

		bool IMordexTable.HasChildTables => HasChildLevels();

		HierarchicalMode IMordexTable.HierarchicalMode
		{
			get
			{
				return BaseHierarchicalMode;
			}
			set
			{
				BaseHierarchicalMode = value;
			}
		}

		DataSourceManager IMordexTable.DataSourceManager
		{
			get
			{
				return GetDataSourceManager();
			}
			set
			{
				SetDataSourceManager(value);
			}
		}

		InheritableBoolean IMordexTable.DynamicFiltering
		{
			get
			{
				return BaseDynamicFiltering;
			}
			set
			{
				BaseDynamicFiltering = value;
			}
		}

		InheritableBoolean IMordexTable.AllowChildTableGroups
		{
			get
			{
				return BaseAllowChildTableGroups;
			}
			set
			{
				BaseAllowChildTableGroups = value;
			}
		}

		IFilterCondition IMordexTable.FilterCondition
		{
			get
			{
				return BaseFilterCondition;
			}
			set
			{
				BaseFilterCondition = value;
			}
		}

		GroupTotals IMordexTable.GroupTotalRows
		{
			get
			{
				return BaseGroupTotalRows;
			}
			set
			{
				BaseGroupTotalRows = value;
			}
		}

		NewRowPosition IMordexTable.NewRowPosition
		{
			get
			{
				return BaseNewRowPosition;
			}
			set
			{
				BaseNewRowPosition = value;
			}
		}

		InheritableBoolean IMordexTable.AllowAddNew
		{
			get
			{
				return BaseAllowAddNew;
			}
			set
			{
				BaseAllowAddNew = value;
			}
		}

		string[] IMordexTable.DataKeyMembers
		{
			get
			{
				return BaseDataKeyMembers;
			}
			set
			{
				BaseDataKeyMembers = value;
			}
		}

		protected string[] BaseDataKeyMembers
		{
			get
			{
				return _dataKeyMembers;
			}
			set
			{
				_dataKeyMembers = value;
			}
		}

		event EventHandler IMordexTable.ColumnsChanged
		{
			add
			{
				AddColumnsChangedHandler(value);
			}
			remove
			{
				RemoveColumnsChangedHandler(value);
			}
		}

		event EventHandler IMordexTable.FilterConditionChanged
		{
			add
			{
				AddFilterConditionChangedHandler(value);
			}
			remove
			{
				RemoveFilterConditionChangedHandler(value);
			}
		}

		public MordexTableBase()
		{
			SetDefaultFieldValues();
		}

		internal MordexTableBase(MordexFieldBase owner)
		{
			SetDefaultFieldValues();
			Initialize("");
			_ownerField = owner;
			_dataMember = _ownerField.DataMember;
			if (_ownerField.MultipleValuesDataMember != null && _ownerField.MultipleValuesDataMember.Length > 0)
			{
				MordexFieldBase field = new MordexFieldBase(_ownerField.MultipleValuesDataMember);
				((IMordexFieldCollection)BaseFields).Add((IMordexField)field);
			}
			else
			{
				MordexFieldBase itemField = new MordexFieldBase
				{
					_isListItemValue = true
				};
				((IMordexFieldCollection)BaseFields).Add((IMordexField)itemField);
			}
		}

		internal MordexFieldBase GetOwnerField()
		{
			return (MordexFieldBase)_ownerField;
		}

		internal bool HasOwnerField()
		{
			return _ownerField != null;
		}

		protected PropertyDescriptor[] BaseGetProperties(bool includeFields, bool includeChildLists)
		{
			if (GetDataSourceManager() != null)
			{
				return GetDataSourceManager().GetTablePropertyDescriptors(this, includeFields, includeChildLists);
			}
			return null;
		}

		protected virtual MordexConditionBase CreateCondition()
		{
			return new MordexConditionBase();
		}

		protected void BaseRetrieveFields()
		{
			DataSourceManager dataSourceManager = GetDataSourceManager();
			if (dataSourceManager != null)
			{
				BaseFields.Clear();
				BoundListInspector rootListState = null;
				if (dataSourceManager.RootList != null)
				{
					rootListState = new BoundListInspector(dataSourceManager.RootList, dataSourceManager);
				}
				dataSourceManager.CreateFieldsFromProperties(rootListState, this, BaseGetProperties(includeFields: true, includeChildLists: false));
			}
		}

		protected void BaseRetrieveChildTables()
		{
			DataSourceManager dataSourceManager = GetDataSourceManager();
			if (dataSourceManager != null)
			{
				BaseChildTables.Clear();
				dataSourceManager.CreateChildTables(this);
			}
		}

		protected virtual void SetDefaultFieldValues()
		{
			_fieldCollection = CreateFieldCollection();
			if (_fieldCollection == null)
			{
				throw new Exception("CreateFieldCollection can not return null.");
			}
			_customGroupCollection = CreateCustomGroupCollection();
			_storedFilterCollection = CreateStoredFiltersCollection();
			_sortFieldCollection = CreateSortFieldCollection();
			_groupCollection = CreateGroupsCollection();
			_childTableCollection = CreateChildTableCollection();
			_groupTotalCollection = CreateGroupTotalCollection();
			_caption = "";
			_allowChildTableGroups = InheritableBoolean.Default;
			_dynamicFiltering = InheritableBoolean.Default;
			_filterParentRows = true;
			_groupTotalRows = GroupTotals.Default;
			_newRowPosition = NewRowPosition.Default;
			_selfReferencingSettings = CreateSelfReferencingSettings();
			_allowAddNew = InheritableBoolean.Default;
			_totalRow = InheritableBoolean.Default;
			_groupMode = GroupMode.Default;
			_hierarchicalMode = HierarchicalMode.UseChildTables;
			_key = "";
		}

		protected void Initialize(string dataMember)
		{
			_key = dataMember;
			_dataMember = dataMember;
			OnInitialize();
		}

		protected virtual void OnInitialize()
		{
		}

		internal bool ShowsTotalRow()
		{
			if (_totalRow == InheritableBoolean.Default)
			{
				if (GetDataSourceManager() != null)
				{
					return GetDataSourceManager().TableTotalRows;
				}
			}
			else if (_totalRow == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool AllowsAddNew()
		{
			if (_allowAddNew == InheritableBoolean.Default)
			{
				if (GetDataSourceManager() != null)
				{
					return GetDataSourceManager().AllowAddNew;
				}
			}
			else if (_allowAddNew == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool UsesDynamicFiltering()
		{
			if (_dynamicFiltering == InheritableBoolean.Default)
			{
				if (GetDataSourceManager() != null)
				{
					return GetDataSourceManager().DynamicFiltering;
				}
			}
			else if (_dynamicFiltering == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		internal bool AllowsChildTableGroups()
		{
			if (_allowChildTableGroups == InheritableBoolean.Default)
			{
				if (GetDataSourceManager() != null)
				{
					return GetDataSourceManager().AllowChildTableGroups;
				}
				return true;
			}
			if (_allowChildTableGroups == InheritableBoolean.True)
			{
				return true;
			}
			return false;
		}

		protected virtual MordexStoredFilterCollectionBase CreateStoredFiltersCollection()
		{
			return MordexStoredFilterCollectionBase.CreateForOwner(this);
		}

		protected virtual MordexSortFieldCollectionBase CreateSortFieldCollection()
		{
			return MordexSortFieldCollectionBase.CreateForOwner(this);
		}

		protected virtual MordexCustomGroupCollectionBase CreateCustomGroupCollection()
		{
			return MordexCustomGroupCollectionBase.CreateForOwner(this);
		}

		protected virtual MordexChildTableCollectionBase CreateChildTableCollection()
		{
			return MordexChildTableCollectionBase.CreateForOwner(this);
		}

		protected virtual MordexGroupTotalCollectionBase CreateGroupTotalCollection()
		{
			return MordexGroupTotalCollectionBase.CreateForOwner(this);
		}
	}
}
