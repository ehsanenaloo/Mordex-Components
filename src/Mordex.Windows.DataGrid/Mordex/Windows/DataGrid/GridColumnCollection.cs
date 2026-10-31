using System;
using System.Collections;
using System.Collections.Generic;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridColumnCollection : MordexFieldCollectionBase, IMordexXmlLayoutsSupport
	{
		private bool _isDeserializing;

		internal GridColumn _owner;

		internal GridColumn _gridColumnCollectionOwner;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Column";

		public GridColumn this[int index] => (GridColumn)BaseGet(index);

		public GridColumn this[string key] => (GridColumn)BaseGet(key);

		public MxDataGrid DataGrid => GetOwnerTable()?.DataGrid;

		internal int CountVisibleColumns()
		{
			int countVisibleColumns = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumn gridColumn = (GridColumn)enumerator.Current;
					if (gridColumn.Visible)
					{
						countVisibleColumns++;
					}
				}
				return countVisibleColumns;
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

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_isDeserializing = true;
			Clear();
			int position = 0;
			for (PropertyValue propertyValue = info["Column" + position]; propertyValue != null; propertyValue = info["Column" + position])
			{
				GridColumn gridColumn = new GridColumn();
				BaseAdd(gridColumn);
				gridColumn._position = position;
				position++;
			}
			position = 0;
			for (PropertyValue propertyValue = info["Column" + position]; propertyValue != null; propertyValue = info["Column" + position])
			{
				propertyValue.DeserializeObject(this[position]);
				position++;
			}
			_isDeserializing = false;
			ApplyColumnPositions();
			DisposeResources();
			RequestAutoResize();
			NotifyTableColumnsChanged();
		}

		private void ApplyColumnPositions()
		{
			List<GridColumn> list = new List<GridColumn>();
			GridColumn[] array = new GridColumn[base.Count];
			for (int i = 0; i < base.Count; i++)
			{
				GridColumn gridColumn = this[i];
				if (gridColumn._position >= 0 && gridColumn._position < array.Length)
				{
					if (array[gridColumn._position] == null)
					{
						array[gridColumn._position] = gridColumn;
					}
					else
					{
						list.Add(gridColumn);
					}
				}
				else
				{
					list.Add(gridColumn);
				}
			}
			if (list.Count <= 0)
			{
				return;
			}
			foreach (GridColumn item in list)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] == null)
					{
						array[j] = item;
						item._position = j;
						break;
					}
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int otherValue = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumn value = (GridColumn)enumerator.Current;
					info.AddValue("Column" + otherValue, value);
					otherValue++;
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

		internal GridColumnCollection(GridTable table)
			: base(table)
		{
		}

		public void AddRange(GridColumn[] columns)
		{
			foreach (GridColumn gridColumn in columns)
			{
				BaseAdd(gridColumn);
				gridColumn._position = gridColumn.Index;
			}
			DisposeResources();
			NotifyTableColumnsChanged();
			RequestAutoResize();
			NotifyTableLayoutChanged();
		}

		private void DisposeResources()
		{
			_owner = null;
			_gridColumnCollectionOwner = null;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumn gridColumn = (GridColumn)enumerator.Current;
					if (gridColumn.CardCaption)
					{
						if (_owner == null)
						{
							_owner = gridColumn;
						}
						else
						{
							gridColumn.SetCardCaption();
						}
					}
					if (gridColumn.CardIcon)
					{
						if (_gridColumnCollectionOwner == null)
						{
							_gridColumnCollectionOwner = gridColumn;
						}
						else
						{
							gridColumn.SetCardIcon();
						}
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
		}

		private void NotifyTableColumnsChanged()
		{
			if (!_isDeserializing && GetOwnerTable() != null)
			{
				GetOwnerTable().RefreshGridRows();
			}
		}

		public void Insert(int index, GridColumn column)
		{
			BaseInsert(index, column);
		}

		protected override bool OnInsert(int index, object value)
		{
			if (!(value is GridColumn))
			{
				return false;
			}
			if (base.OnInsert(index, value))
			{
				if (index >= 0 && index < base.Count)
				{
					ShiftPositionsForInsert(index);
				}
				return true;
			}
			return false;
		}

		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			if (!_isDeserializing)
			{
				GridColumn gridColumn = (GridColumn)value;
				gridColumn._position = gridColumn.Index;
				DisposeResources();
				RequestAutoResize();
				if (DataGrid != null)
				{
					DataGrid.RebuildRows(true);
				}
			}
		}

		public GridColumn Add()
		{
			return Add(string.Empty);
		}

		public GridColumn Add(string key)
		{
			GridColumn gridColumn = new GridColumn(key);
			BaseAdd(gridColumn);
			return gridColumn;
		}

		public GridColumn Add(string dataMember, ColumnType columnType)
		{
			GridColumn gridColumn = new GridColumn(dataMember, columnType);
			BaseAdd(gridColumn);
			return gridColumn;
		}

		public GridColumn Add(string dataMember, ColumnType columnType, EditType editType)
		{
			GridColumn gridColumn = new GridColumn(dataMember, columnType, editType);
			BaseAdd(gridColumn);
			return gridColumn;
		}

		public void Add(GridColumn column)
		{
			if (column == null)
			{
				throw new ArgumentNullException();
			}
			if (column.GetOwnerColumnCollection() != null)
			{
				throw new InvalidOperationException();
			}
			BaseAdd(column);
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			if (!_isDeserializing)
			{
				_owner = null;
				_gridColumnCollectionOwner = null;
				NotifyTableColumnsChanged();
				RequestAutoResize();
				if (DataGrid != null)
				{
					DataGrid.RemoveOrphanRootTableSortKeys();
				}
			}
		}

		internal void ReplaceColumns(GridColumn[] objects)
		{
			BaseReplaceElements(objects);
			for (int i = 0; i < base.Count; i++)
			{
				GridColumn gridColumn = this[i];
				gridColumn._position = i;
			}
			NotifyTableLayoutChanged();
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Remove(string key)
		{
			BaseRemove(key);
		}

		public void Remove(GridColumn column)
		{
			BaseRemove(column);
		}

		protected override bool OnRemove(int index, object value)
		{
			if (base.OnRemove(index, value))
			{
				GridColumn gridColumn = (GridColumn)value;
				ShiftPositionsAfterRemove(gridColumn.Position);
				return true;
			}
			return false;
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			base.OnRemoveComplete(index, value);
			GridColumn gridColumn = (GridColumn)value;
			if (_owner == gridColumn)
			{
				_owner = null;
			}
			if (_gridColumnCollectionOwner == gridColumn)
			{
				_gridColumnCollectionOwner = null;
			}
			NotifyTableColumnsChanged();
			RequestAutoResize();
			DataGrid.RemoveOrphanRootTableSortKeys();
		}

		internal void ShiftPositionsAfterRemove(int value)
		{
			if (value < 0)
			{
				return;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumn gridColumn = (GridColumn)enumerator.Current;
					if (gridColumn._position > value)
					{
						gridColumn._position--;
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
		}

		internal void ShiftPositionsForInsert(int value)
		{
			if (value < 0)
			{
				return;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumn gridColumn = (GridColumn)enumerator.Current;
					if (gridColumn._position >= value)
					{
						gridColumn._position++;
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
		}

		internal GridTable GetOwnerTable()
		{
			return (GridTable)base.BaseTable;
		}

		internal void NotifyTableLayoutChanged()
		{
			if (GetOwnerTable() != null)
			{
				GetOwnerTable().RefreshDataGrid();
			}
		}

		public GridColumn GetColumnInPosition(int position)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumn gridColumn = (GridColumn)enumerator.Current;
					if (gridColumn.Position == position)
					{
						return gridColumn;
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
			return null;
		}

		internal GridColumn GetLastVisibleColumn()
		{
			for (int position = base.Count - 1; position >= 0; position--)
			{
				GridColumn columnInPosition = GetColumnInPosition(position);
				if (columnInPosition != null && columnInPosition.Visible)
				{
					return columnInPosition;
				}
			}
			return null;
		}

		internal void NotifyColumnVisibilityChanged(GridColumn gridColumn)
		{
			if (GetOwnerTable() != null)
			{
				GetOwnerTable().InvalidateGridLayout();
			}
		}

		private void RequestAutoResize()
		{
			if (DataGrid != null && DataGrid.ColumnAutoResize)
			{
				GetOwnerTable()._isAutoResizePending = true;
			}
		}

		public bool Contains(string key)
		{
			return BaseContains(key);
		}

		public bool Contains(GridColumn column)
		{
			return BaseContains(column);
		}
	}
}
