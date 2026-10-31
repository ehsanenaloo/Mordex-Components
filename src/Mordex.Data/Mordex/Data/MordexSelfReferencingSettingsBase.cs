using System;
using System.ComponentModel;
using Mordex.Data.Internal;

namespace Mordex.Data
{
	public class MordexSelfReferencingSettingsBase : IMordexSelfReferencingSettings
	{
		private ParentRootMode _parentRootMode;

		private MordexTableBase _table;

		private string _childMember;

		private string _parentMember;

		private string _childListMember;

		private object _parentRootValue;

		private HierarchicalGroupMode _hierarchicalGroupMode;

		private SelfReferencingAggregationMode _aggregationMode;

		private bool _treatOrphanRowsAsRoot;

		protected MordexTableBase BaseTable => _table;

		protected ParentRootMode BaseParentRootMode
		{
			get
			{
				return _parentRootMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ParentRootMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_parentRootMode != value)
				{
					_parentRootMode = value;
					NotifySettingChanged("ParentRootMode");
				}
			}
		}

		protected SelfReferencingAggregationMode BaseAggregationMode
		{
			get
			{
				return _aggregationMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(SelfReferencingAggregationMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_aggregationMode != value)
				{
					_aggregationMode = value;
					NotifySettingChanged("AggregationMode");
				}
			}
		}

		protected bool BaseTreatOrphanRowsAsRoot
		{
			get
			{
				return _treatOrphanRowsAsRoot;
			}
			set
			{
				if (_treatOrphanRowsAsRoot != value)
				{
					_treatOrphanRowsAsRoot = value;
					NotifySettingChanged("TreatOrphanRowsAsRoot");
				}
			}
		}

		protected HierarchicalGroupMode BaseHierarchicalGroupMode
		{
			get
			{
				return _hierarchicalGroupMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(HierarchicalGroupMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == HierarchicalGroupMode.ParentRows && _table != null && _table.IsSelfReferencingTable())
				{
					foreach (IMordexGroup group in ((IMordexTable)_table).Groups)
					{
						if (group.Field.MultipleValues)
						{
							throw new InvalidOperationException("HierarchicalGroupMode.ParentRows can not be used when the table is grouped by a MultipleValues field.");
						}
					}
				}
				if (_hierarchicalGroupMode != value)
				{
					_hierarchicalGroupMode = value;
					if (_table != null && _table.IsGrouped())
					{
						NotifySettingChanged("HierarchicalGroupMode");
					}
				}
			}
		}

		protected string BaseChildListMember
		{
			get
			{
				return _childListMember;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (_childListMember != value)
				{
					_childListMember = value;
					NotifySettingChanged("ChildListMember");
				}
			}
		}

		protected string BaseParentMember
		{
			get
			{
				return _parentMember;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (_parentMember != value)
				{
					_parentMember = value;
					NotifySettingChanged("ParentMember");
				}
			}
		}

		protected string BaseChildMember
		{
			get
			{
				return _childMember;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (_childMember != value)
				{
					_childMember = value;
					NotifySettingChanged("ChildMember");
				}
			}
		}

		protected object BaseParentRootValue
		{
			get
			{
				return _parentRootValue;
			}
			set
			{
				if (_parentRootValue != value)
				{
					_parentRootValue = value;
					NotifySettingChanged("ParentRootValue");
				}
			}
		}

		HierarchicalGroupMode IMordexSelfReferencingSettings.HierarchicalGroupMode
		{
			get
			{
				return BaseHierarchicalGroupMode;
			}
			set
			{
				BaseHierarchicalGroupMode = value;
			}
		}

		string IMordexSelfReferencingSettings.ChildListMember
		{
			get
			{
				return BaseChildListMember;
			}
			set
			{
				BaseChildListMember = value;
			}
		}

		string IMordexSelfReferencingSettings.ChildMember
		{
			get
			{
				return BaseChildMember;
			}
			set
			{
				BaseChildMember = value;
			}
		}

		string IMordexSelfReferencingSettings.ParentMember
		{
			get
			{
				return BaseParentMember;
			}
			set
			{
				BaseParentMember = value;
			}
		}

		ParentRootMode IMordexSelfReferencingSettings.ParentRootMode
		{
			get
			{
				return BaseParentRootMode;
			}
			set
			{
				BaseParentRootMode = value;
			}
		}

		SelfReferencingAggregationMode IMordexSelfReferencingSettings.AggregationMode
		{
			get
			{
				return BaseAggregationMode;
			}
			set
			{
				BaseAggregationMode = value;
			}
		}

		object IMordexSelfReferencingSettings.ParentRootValue
		{
			get
			{
				return BaseParentRootValue;
			}
			set
			{
				BaseParentRootValue = value;
			}
		}

		bool IMordexSelfReferencingSettings.TreatOrphanRowsAsRoot
		{
			get
			{
				return BaseTreatOrphanRowsAsRoot;
			}
			set
			{
				BaseTreatOrphanRowsAsRoot = value;
			}
		}

		internal static MordexSelfReferencingSettingsBase CreateForTable(MordexTableBase table)
		{
			return new MordexSelfReferencingSettingsBase(table);
		}

		protected MordexSelfReferencingSettingsBase(MordexTableBase table)
		{
			_table = table;
			ResetToDefaults();
		}

		private void ResetToDefaults()
		{
			_hierarchicalGroupMode = HierarchicalGroupMode.ParentRows;
			_childListMember = string.Empty;
			_parentRootMode = ParentRootMode.NullValue;
			_parentRootValue = null;
			_childMember = string.Empty;
			_parentMember = string.Empty;
			_aggregationMode = SelfReferencingAggregationMode.AllRows;
		}

		internal void ReassignTable(MordexTableBase table)
		{
			_table = table;
			NotifySettingChanged("");
		}

		private void NotifySettingChanged(string text)
		{
			if (_table != null)
			{
				_table.OnSelfReferencingSettingChanged(text);
			}
		}
	}
}
