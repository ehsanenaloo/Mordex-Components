using System;
using System.Collections;
using System.Collections.Generic;

namespace Mordex.Data
{
	public class MordexGroupRow : MordexRow
	{
		internal bool _ignoreCase;

		private object _groupValue;

		private object _originalValue;

		private string _groupText;

		private MordexTotalRow _totalRow;

		internal bool _alwaysHidden;

		private bool _isRefreshingFilter;

		public string GroupText
		{
			get
			{
				if (_groupText == null)
				{
					if (_groupValue is IMordexCustomGroupRow)
					{
						return ((IMordexCustomGroupRow)_groupValue).GroupCaption;
					}
					return string.Concat(_groupValue);
				}
				return _groupText;
			}
			set
			{
				_groupText = value;
			}
		}

		public object OriginalGroupValue => _originalValue;

		public object GroupValue => _groupValue;

		public override MordexGroupBase Group
		{
			get
			{
				if (base.ParentCollection != null)
				{
					return base.ParentCollection.Group;
				}
				return null;
			}
		}

		public MordexTotalRow TotalRow
		{
			get
			{
				if (Group != null && Group.GetEffectiveTotalVisibility() != GroupTotalVisibility.Hidden)
				{
					if (_totalRow == null)
					{
						_totalRow = new MordexTotalRow(this);
					}
					return _totalRow;
				}
				return null;
			}
		}

		protected override bool IsTreeRow => true;

		protected override bool LoadIsChecked => true;

		internal MordexGroupRow(object originalValue, object groupValue)
		{
			_originalValue = originalValue;
			_groupValue = groupValue;
		}

		protected override bool CheckedDependsOnChild()
		{
			return true;
		}

		protected override object GetRowKey()
		{
			if (_groupValue == null)
			{
				return DBNull.Value;
			}
			if (_ignoreCase && _groupValue is string)
			{
				return ((string)_groupValue).ToUpper();
			}
			return _groupValue;
		}

		protected override int CompareRowValue(object o)
		{
			int result = DataValueHelper.CompareValues(DataSourceManager.CurrentCulture, _groupValue, o, null, true, false);
			if (((IMordexGroup)Group).SortOrder == SortOrder.Descending)
			{
				result = -result;
			}
			return result;
		}

		protected override void OnChildRowDisposed(MordexRow row)
		{
			if (base.Rows.Count == 0)
			{
				if (_groupValue is MordexCustomGroupRowBase)
				{
					UpdateVisibility();
				}
				else
				{
					DeleteRowFromCollection();
				}
			}
			else
			{
				UpdateVisibility();
			}
		}

		protected override int CompareTo(MordexRow row)
		{
			MordexGroupBase mordexGroupBase = Group;
			IMordexGroup mordexGroup = mordexGroupBase;
			if (mordexGroup != null)
			{
				if (row is MordexTotalRow)
				{
					return 0;
				}
				if (mordexGroup.CustomGroup != null && mordexGroup.CustomGroup.CustomGroupType == CustomGroupType.ConditionalGroupRows)
				{
					IComparable comparable = _groupValue as IComparable;
					int comparison = comparable.CompareTo(((MordexGroupRow)row)._groupValue);
					if (mordexGroup.SortOrder == SortOrder.Descending)
					{
						comparison = -comparison;
					}
					return comparison;
				}
				MordexGroupRow mordexGroupRow = (MordexGroupRow)row;
				IComparer comparer = mordexGroupBase.GetGroupComparer();
				int result = DataValueHelper.CompareValues(mordexGroupBase.GetCulture(), _groupValue, mordexGroupRow._groupValue, comparer, _ignoreCase, false);
				if (mordexGroup.SortOrder == SortOrder.Descending)
				{
					result = -result;
				}
				return result;
			}
			return 1;
		}

		public override bool CanHaveChildren()
		{
			return true;
		}

		protected override MordexRowCollection CreateChildRows()
		{
			int index = Group.Index;
			MordexRowCollection mordexRowCollection;
			if (index < GetTableInterface().Groups.Count - 1)
			{
				IMordexCustomGroup customGroup = GetTableInterface().Groups[index + 1].CustomGroup;
				mordexRowCollection = ((customGroup != null && customGroup.CustomGroupType != CustomGroupType.CompositeColumns) ? new MordexRowCollection(this, RowCollectionType.CustomGroupRows) : new MordexRowCollection(this, RowCollectionType.GroupRows));
				mordexRowCollection.Group = (MordexGroupBase)GetTableInterface().Groups[index + 1];
			}
			else
			{
				mordexRowCollection = new MordexRowCollection(this, RowCollectionType.DataRows);
			}
			return mordexRowCollection;
		}

		protected override bool ShouldContainRow(MordexRow row)
		{
			if (row is MordexDataRow mordexDataRow)
			{
				MordexGroupBase mordexGroupBase = Group;
				if (_groupValue is IMordexCustomGroupRow)
				{
					return false;
				}
				MordexTableBase mordexTableBase = mordexGroupBase.GetGroupTable();
				IList list = mordexDataRow.DataHolder.GetRowsForGroup(mordexTableBase, mordexGroupBase.Index == 0, false);
				foreach (DataHolderRow item in list)
				{
					object itemValue = mordexGroupBase.GetGroupValueFromBaseFields(item);
					if (DataValueHelper.CompareValues(mordexGroupBase.GetCulture(), itemValue, _groupValue, mordexGroupBase.GetGroupComparer(), _ignoreCase, false) == 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void ExpandGroups(MordexGroupBase group)
		{
			base.Expanded = true;
			if (group != Group)
			{
				base.ExpandGroups(group);
			}
		}

		protected override void OnEvaluateVisibility()
		{
			if (!_alwaysHidden)
			{
				if (_groupValue is IMordexCustomGroupRow mordexCustomGroupRow && mordexCustomGroupRow.ShowWhenEmpty)
				{
					base.Visible = true;
					return;
				}
				foreach (MordexRow item in (IEnumerable)base.Rows)
				{
					if (item.Visible)
					{
						base.Visible = true;
						return;
					}
				}
			}
			base.Visible = false;
		}

		protected override void OnRowChange(MordexRow row, RowChangeType change)
		{
			base.OnRowChange(row, change);
			if (row.ParentRow == this)
			{
				switch (change)
				{
				case RowChangeType.Delete:
				case RowChangeType.Add:
				case RowChangeType.ChildRowReset:
				case RowChangeType.VisibleChanged:
					if (!_isRefreshingFilter)
					{
						UpdateVisibility();
					}
					break;
				}
			}
			if (!_isRefreshingFilter && !IsLoading() && row != this && row != _totalRow)
			{
				NotifyTotalChanged();
			}
		}

		public override void RefreshFilter(MordexTableBase table)
		{
			bool visible = base.Visible;
			bool wasRefreshingFilter = _isRefreshingFilter;
			_isRefreshingFilter = true;
			try
			{
				base.RefreshFilter(table);
			}
			finally
			{
				_isRefreshingFilter = wasRefreshingFilter;
			}
			if (visible != base.Visible)
			{
				OnRowChange(this, RowChangeType.VisibleChanged);
			}
			NotifyTotalChanged();
		}

		private void NotifyTotalChanged()
		{
			if (_totalRow != null)
			{
				_totalRow.RaiseRowChange(_totalRow, RowChangeType.Change);
			}
			RaiseRowChange(this, RowChangeType.TotalChanged);
		}

		protected override MordexRow[] GetPostListRows()
		{
			if (TotalRow != null)
			{
				return new MordexRow[1] { TotalRow };
			}
			return null;
		}

		public override void FillList(List<MordexRow> list, bool childRowsOnly)
		{
			base.FillList(list, childRowsOnly);
			if (base.Visible && !base.Expanded)
			{
				GroupTotalVisibility groupTotalVisibility = Group.GetEffectiveTotalVisibility();
				if (groupTotalVisibility == GroupTotalVisibility.Visible)
				{
					TotalRow.FillList(list, childRowsOnly: false);
				}
			}
		}

		protected override void CollapseGroupRowsCore(MordexGroupBase group)
		{
			if (group == null || group == Group)
			{
				base.CollapseGroupRowsCore(null);
				base.Expanded = false;
			}
			else
			{
				base.CollapseGroupRowsCore(group);
			}
		}

		protected override void ExpandGroupRowsCore(MordexGroupBase group)
		{
			if (group == Group)
			{
				base.Expanded = true;
				return;
			}
			if (group == null)
			{
				base.Expanded = true;
			}
			base.ExpandGroupRowsCore(group);
		}

		protected override void LoadTreeCacheRowsCore(DataSourceManager manager, MordexRowCache cacheRow, object[] dataRowReferences)
		{
			base.LoadTreeCacheRowsCore(manager, cacheRow, dataRowReferences);
			IMordexGroup mordexGroup = Group;
			if (mordexGroup != null && mordexGroup.CustomGroup != null && mordexGroup.CustomGroup.CustomGroupType == CustomGroupType.ConditionalGroupRows)
			{
				IMordexCustomGroupRow mordexCustomGroupRow = null;
				if (cacheRow.GetFieldValue(0) is int)
				{
					int groupIndex = (int)cacheRow.GetFieldValue(0);
					if (groupIndex >= 0 && groupIndex < mordexGroup.CustomGroup.GroupRows.Count)
					{
						_groupText = ((IMordexCustomGroupRow)(_originalValue = (_groupValue = mordexGroup.CustomGroup.GroupRows[groupIndex]))).GroupCaption;
					}
					else if (groupIndex == -1)
					{
						mordexCustomGroupRow = MordexCustomGroupRowBase.CreateDefault();
						mordexCustomGroupRow.ShowRemainingRows = true;
						mordexCustomGroupRow.ShowWhenEmpty = false;
						_groupValue = mordexCustomGroupRow;
						_originalValue = mordexCustomGroupRow;
						_groupText = "";
					}
				}
			}
			else
			{
				_groupValue = cacheRow.GetFieldValue(0);
				_originalValue = cacheRow.GetFieldValue(1);
				_groupText = (string)cacheRow.GetFieldValue(2);
			}
		}
	}
}
