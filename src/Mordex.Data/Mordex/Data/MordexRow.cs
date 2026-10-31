using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace Mordex.Data
{
	public abstract partial class MordexRow : IComparable
	{
		private MordexRowCollection _parentCollection;

		private MordexRowCollection _rows;

		private bool _shouldRetrieveChildRows;

		private object _tag;

		private object _accessibilityTag;

		private bool _expanded;

		private bool _visible;

		private object _externalData;

		private int _indexInParent;

		private object _cachedRowKey;

		private MordexRow _parentRow;

		private int _rowManagerPosition = -1;

		private bool _suppressExpandedNotification;

		private bool _isCheckedCore;

		private int _cacheIndex = -1;

		private object[] _dataRowKeys;

		protected virtual bool DefaultExpandedValue => false;

		public virtual MordexRow ParentRow => _parentRow;

		public MordexRowCollection ParentCollection => _parentCollection;

		public virtual DataSourceManager DataSourceManager
		{
			get
			{
				if (ParentRow != null)
				{
					return ParentRow.DataSourceManager;
				}
				return null;
			}
		}

		public virtual MordexTableBase Table
		{
			get
			{
				if (ParentRow != null)
				{
					return ParentRow.Table;
				}
				return null;
			}
		}

		public virtual object DataRow => null;

		public MordexRowCollection Rows
		{
			get
			{
				LoadChildRows();
				return _rows;
			}
		}

		public virtual bool ChildrenLoaded => HasRowCollection();

		public virtual MordexGroupBase Group => null;

		public bool Expanded
		{
			get
			{
				return _expanded;
			}
			set
			{
				if (_expanded != value && CanExpand())
				{
					_expanded = value;
					OnExpandedChanged();
				}
			}
		}

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
					_visible = value;
					NoteStructureChanged();
					RaiseRowChange(this, RowChangeType.VisibleChanged);
				}
			}
		}

		public MordexRow NextSibling
		{
			get
			{
				if (ParentRow != null)
				{
					return ParentRow.GetNextChildAfter(this);
				}
				return null;
			}
		}

		public MordexRow LastChild
		{
			get
			{
				if (CanHaveChildren())
				{
					MordexRow[] postListRows = GetPostListRows();
					if (postListRows != null && postListRows.Length > 0)
					{
						return postListRows[postListRows.Length - 1];
					}
					if (Rows != null && Rows.Count > 0)
					{
						return Rows[Rows.Count - 1];
					}
					postListRows = GetPreListRows();
					if (postListRows != null && postListRows.Length > 0)
					{
						return postListRows[postListRows.Length - 1];
					}
				}
				return null;
			}
		}

		public MordexRow LastVisibleChild
		{
			get
			{
				if (CanHaveChildren())
				{
					MordexRow[] postListRows = GetPostListRows();
					if (postListRows != null && postListRows.Length > 0)
					{
						for (int i = postListRows.Length - 1; i >= 0; i--)
						{
							if (postListRows[i].Visible)
							{
								return postListRows[i];
							}
						}
					}
					if (Rows != null)
					{
						for (int j = Rows.Count - 1; j >= 0; j--)
						{
							if (Rows[j].Visible)
							{
								return Rows[j];
							}
						}
					}
					postListRows = GetPreListRows();
					if (postListRows != null && postListRows.Length > 0)
					{
						for (int k = postListRows.Length - 1; k >= 0; k--)
						{
							if (postListRows[k].Visible)
							{
								return postListRows[k];
							}
						}
					}
				}
				return null;
			}
		}

		public MordexRow LastChildLoaded
		{
			get
			{
				if (ChildrenLoaded)
				{
					return LastChild;
				}
				return null;
			}
		}

		public MordexRow FirstChild
		{
			get
			{
				if (CanHaveChildren())
				{
					MordexRow[] preListRows = GetPreListRows();
					if (preListRows != null && preListRows.Length > 0)
					{
						return preListRows[0];
					}
					if (Rows != null && Rows.Count > 0)
					{
						return Rows[0];
					}
					preListRows = GetPostListRows();
					if (preListRows != null && preListRows.Length > 0)
					{
						return preListRows[0];
					}
				}
				return null;
			}
		}

		public MordexRow FirstChildLoaded
		{
			get
			{
				if (ChildrenLoaded)
				{
					return FirstChild;
				}
				return null;
			}
		}

		public MordexRow PreviousRowLoaded
		{
			get
			{
				MordexRow mordexRow = PreviousSibling;
				if (mordexRow != null)
				{
					for (MordexRow lastChildLoaded = mordexRow.LastChildLoaded; lastChildLoaded != null; lastChildLoaded = mordexRow.LastChildLoaded)
					{
						mordexRow = lastChildLoaded;
					}
					return mordexRow;
				}
				if (!(ParentRow is MordexRootRow))
				{
					return ParentRow;
				}
				return null;
			}
		}

		public MordexRow PreviousRow
		{
			get
			{
				MordexRow mordexRow = PreviousSibling;
				if (mordexRow != null)
				{
					for (MordexRow lastChild = mordexRow.LastChild; lastChild != null; lastChild = mordexRow.LastChild)
					{
						mordexRow = lastChild;
					}
					return mordexRow;
				}
				if (!(ParentRow is MordexRootRow))
				{
					return ParentRow;
				}
				return null;
			}
		}

		public MordexRow NextRowLoaded
		{
			get
			{
				MordexRow mordexRow = FirstChildLoaded;
				if (mordexRow == null)
				{
					MordexRow current = this;
					while (current != null && mordexRow == null)
					{
						mordexRow = current.NextSibling;
						current = current.ParentRow;
					}
				}
				return mordexRow;
			}
		}

		public MordexRow NextRow
		{
			get
			{
				MordexRow mordexRow = FirstChild;
				if (mordexRow == null)
				{
					MordexRow current = this;
					while (current != null && mordexRow == null)
					{
						mordexRow = current.NextSibling;
						current = current.ParentRow;
					}
				}
				return mordexRow;
			}
		}

		public MordexRow PreviousSibling
		{
			get
			{
				if (ParentRow != null)
				{
					return ParentRow.GetPreviousChildBefore(this);
				}
				return null;
			}
		}

		public virtual int RowManagerPosition => _rowManagerPosition;

		public virtual MordexNewRowPosition NewRowPosition => MordexNewRowPosition.None;

		public MordexRow RootRow
		{
			get
			{
				if (ParentRow != null)
				{
					return ParentRow.RootRow;
				}
				return this;
			}
		}

		public object ExternalData
		{
			get
			{
				return _externalData;
			}
			set
			{
				_externalData = value;
			}
		}

		public virtual int RecordPosition => -1;

		public virtual bool IsCheckedCore
		{
			get
			{
				return _isCheckedCore;
			}
			set
			{
				_isCheckedCore = value;
			}
		}

		public bool IsChecked
		{
			get
			{
				if (CheckedDependsOnChild())
				{
					return CheckedState == CheckedState.Checked;
				}
				return IsCheckedCore;
			}
			set
			{
				IsCheckedCore = value;
				if (!CheckedDependsOnChild() || Rows == null)
				{
					return;
				}
				foreach (MordexRow item in (IEnumerable)Rows)
				{
					if (item.Visible)
					{
						item.IsChecked = value;
					}
				}
			}
		}

		protected virtual bool IsTreeRow => false;

		protected virtual bool LoadIsChecked => false;

		public CheckedState CheckedState
		{
			get
			{
				if (CheckedDependsOnChild())
				{
					return GetStateFromChildRows();
				}
				if (IsCheckedCore)
				{
					return CheckedState.Checked;
				}
				return CheckedState.Unchecked;
			}
			set
			{
				if (CheckedState == value)
				{
					return;
				}
				switch (value)
				{
				case CheckedState.Mixed:
					return;
				case CheckedState.Checked:
					IsCheckedCore = true;
					break;
				default:
					IsCheckedCore = false;
					break;
				}
				if (Rows == null)
				{
					return;
				}
				foreach (MordexRow item in (IEnumerable)Rows)
				{
					item.CheckedState = value;
				}
			}
		}

		public object[] DataRowKeys
		{
			get
			{
				if (_dataRowKeys == null)
				{
					LoadDataRowKeys();
				}
				return _dataRowKeys;
			}
			set
			{
				_dataRowKeys = value;
			}
		}

		public object AccessibilityTag
		{
			get
			{
				return _accessibilityTag;
			}
			set
			{
				_accessibilityTag = value;
			}
		}

		protected MordexRow()
		{
			InitializeVariables();
		}

		internal int GetIndexInParent()
		{
			if (_parentCollection == null)
			{
				return -1;
			}
			if (_indexInParent >= 0 && _indexInParent < _parentCollection.Count && _parentCollection[_indexInParent] == this)
			{
				return _indexInParent;
			}
			_indexInParent = _parentCollection.IndexOf(this);
			return _indexInParent;
		}

		internal void SetIndexInParent(int mordexRowInteger)
		{
			_indexInParent = mordexRowInteger;
		}

		internal void CollectDistinctValues(IMordexField field, IMordexValueItemCollection valueList)
		{
			GetDistinctValuesCore(field, valueList);
		}

		protected virtual void GetDistinctValuesCore(IMordexField field, IMordexValueItemCollection valueList)
		{
			if (!ChildrenLoaded)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)Rows)
			{
				item.GetDistinctValuesCore(field, valueList);
			}
		}

		internal object ReadFieldValue(MordexFieldBase field, string dataMember)
		{
			return GetValueOfFieldCore(field, dataMember);
		}

		protected virtual object GetValueOfFieldCore(MordexFieldBase field, string dataMember)
		{
			return null;
		}

		protected virtual void InitializeVariables()
		{
			_visible = true;
			_expanded = DefaultExpandedValue;
			_indexInParent = -1;
		}

		protected void DeleteRowFromCollection()
		{
			if (ParentCollection != null)
			{
				MordexRow parentRow = ParentRow;
				ParentCollection.UnindexRow(this);
				ParentCollection.RemoveChildRow(this);
				parentRow?.NotifyChildRowDisposed(this);
			}
			Dispose();
		}

		internal void NotifyChildRowDisposed(MordexRow row)
		{
			OnChildRowDisposed(row);
		}

		protected virtual void OnChildRowDisposed(MordexRow row)
		{
		}

		internal object GetCachedRowKey()
		{
			if (_cachedRowKey == null)
			{
				_cachedRowKey = GetRowKey();
			}
			return _cachedRowKey;
		}

		internal bool HasRowKeyChanged()
		{
			return IsRowKeyChangedCore();
		}

		protected virtual bool IsRowKeyChangedCore()
		{
			return false;
		}

		internal void RefreshCachedRowKey()
		{
			_cachedRowKey = GetRowKey();
		}

		protected virtual object GetRowKey()
		{
			return null;
		}

		internal void SetShouldRetrieveChildRows(bool mordexRowBoolean)
		{
			_shouldRetrieveChildRows = mordexRowBoolean;
		}

		internal IMordexTable GetTableInterface()
		{
			return Table;
		}

		public int PositionOfChild(MordexRow row)
		{
			return row.RowManagerPosition;
		}

		public void LoadChildRows()
		{
			if (_rows == null)
			{
				_rows = CreateChildRowsIfSupported();
				if (_rows != null && _shouldRetrieveChildRows)
				{
					RetrieveChildRows();
				}
			}
		}

		internal bool HasRowCollection()
		{
			return _rows != null;
		}

		public bool IsDescendentOf(MordexRow row)
		{
			for (MordexRow mordexRow = this; mordexRow != null; mordexRow = mordexRow.ParentRow)
			{
				if (mordexRow == row)
				{
					return true;
				}
			}
			return false;
		}

		public void EnsureVisible()
		{
			for (MordexRow parentRow = ParentRow; parentRow != null; parentRow = parentRow.ParentRow)
			{
				parentRow.Expanded = true;
			}
		}

		private MordexRowCollection CreateChildRowsIfSupported()
		{
			if (CanHaveChildren())
			{
				return CreateChildRows();
			}
			return null;
		}

		internal MordexRow FindRowForDataItem(object value, IMordexTable iMordexTable)
		{
			if (ChildrenLoaded && Rows != null)
			{
				if (_rows.GetCollectionType() == RowCollectionType.DataRows && GetTableInterface() == iMordexTable)
				{
					object key = value;
					if (value is DataRowView)
					{
						DataRow row = ((DataRowView)value).Row;
						if (row.RowState == DataRowState.Detached)
						{
							return null;
						}
						key = ((DataRowView)value).Row;
					}
					MordexRow mordexRow = _rows.FindRowByKey(key);
					if (mordexRow != null)
					{
						return mordexRow;
					}
					if (!Table.IsSelfReferencingTable())
					{
						return null;
					}
				}
				foreach (MordexRow item in (IEnumerable)_rows)
				{
					MordexRow foundRow = item.FindRowForDataItem(value, iMordexTable);
					if (foundRow != null)
					{
						return foundRow;
					}
				}
			}
			return null;
		}

		internal void SetParentCollection(MordexRowCollection mordexRowCollection)
		{
			_parentCollection = mordexRowCollection;
			if (mordexRowCollection != null)
			{
				_parentRow = mordexRowCollection.ParentRow;
			}
			else
			{
				_parentRow = null;
			}
		}

		public virtual void Dispose()
		{
			OnDisposing();
			OnDisposed();
		}

		protected void DisposeChildRows()
		{
			if (_rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)_rows)
			{
				item.Dispose();
				item._parentRow = null;
				item._parentCollection = null;
			}
		}

		protected virtual void OnDisposing()
		{
			DisposeChildRows();
		}

		protected virtual void OnDisposed()
		{
		}

		public virtual bool CanHaveChildren()
		{
			return false;
		}

		protected virtual MordexRowCollection CreateChildRows()
		{
			return null;
		}

		protected virtual void RetrieveChildRows()
		{
		}

		protected virtual int CompareTo(MordexRow row)
		{
			return 1;
		}

		protected virtual int CompareRowValue(object o)
		{
			return 1;
		}

		int IComparable.CompareTo(object value)
		{
			if (value is MordexRow row)
			{
				return CompareTo(row);
			}
			return CompareRowValue(value);
		}

		internal virtual void RefreshSort(MordexTableBase table)
		{
			if (_rows == null)
			{
				return;
			}
			if (table == null || table == Table)
			{
				if (_rows.IsSortingEnabled())
				{
					_rows.ResortRows();
				}
				else
				{
					_rows.SetSortingEnabled(true);
				}
			}
			foreach (MordexRow item in (IEnumerable)_rows)
			{
				item.RefreshSort(table);
			}
		}

		internal void RefreshGroupRows(MordexTableBase table)
		{
			RefreshGroupsCore(table);
		}

		protected virtual void RefreshGroupsCore(MordexTableBase table)
		{
			if (_rows == null)
			{
				return;
			}
			foreach (MordexRow item in (IEnumerable)_rows)
			{
				item.RefreshGroupsCore(table);
			}
		}

		protected void RefreshChildRows()
		{
			DisposeChildRows();
			_rows = null;
			NoteStructureChanged();
			LoadChildRows();
			RaiseRowChange(this, RowChangeType.ChildRowReset);
		}

		internal bool CanContainRow(MordexRow row)
		{
			return ShouldContainRow(row);
		}

		protected virtual bool ShouldContainRow(MordexRow row)
		{
			return true;
		}

		private bool CanExpand()
		{
			if (!CanHaveChildren())
			{
				return false;
			}
			return DataSourceManager?.CanChangeExpandedState(this) ?? true;
		}

		protected virtual void OnExpandedChanged()
		{
			if (!_suppressExpandedNotification)
			{
				RaiseRowChange(this, RowChangeType.ExpandedChanged);
			}
			DataSourceManager?.RaiseRowExpandedChanged(this);
		}

		public virtual bool IsVirtualRow()
		{
			return false;
		}

		public virtual bool IsRowVisible()
		{
			if (_visible)
			{
				MordexRow parentRow = ParentRow;
				while (parentRow != null && !(parentRow is MordexRootRow))
				{
					if (!parentRow.Visible || !parentRow.Expanded)
					{
						return false;
					}
					parentRow = parentRow.ParentRow;
				}
				return true;
			}
			return false;
		}

		protected virtual MordexRow[] GetPreListRows()
		{
			return null;
		}

		protected virtual MordexRow[] GetPostListRows()
		{
			return null;
		}
	}
}
