using System;
using System.Collections;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridColumnSetCollection : CollectionBase, IMordexXmlLayoutsSupport
	{
		internal GridTable _ownerTable;

		private int _countedColumns;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "ColumnSet";

		public GridColumnSet this[int index] => (GridColumnSet)base.InnerList[index];

		public GridColumnSet this[string key]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridColumnSet gridColumnSet = (GridColumnSet)enumerator.Current;
						if (string.Compare(gridColumnSet.Key, key, ignoreCase: true) == 0)
						{
							return gridColumnSet;
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
				throw new ArgumentOutOfRangeException();
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
					GridColumnSet value = (GridColumnSet)enumerator.Current;
					info.AddValue("ColumnSet" + otherValue, value);
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

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int index = 0;
			for (PropertyValue propertyValue = info["ColumnSet" + index]; propertyValue != null; propertyValue = info["ColumnSet" + index])
			{
				GridColumnSet gridColumnSet = new GridColumnSet();
				base.InnerList.Add(gridColumnSet);
				gridColumnSet.SetOwnerCollection(this);
				gridColumnSet._index = index;
				gridColumnSet._position = index;
				index++;
			}
			index = 0;
			for (PropertyValue propertyValue = info["ColumnSet" + index]; propertyValue != null; propertyValue = info["ColumnSet" + index])
			{
				propertyValue.DeserializeObject(this[index]);
				index++;
			}
			RecalculateColumnPositions();
			RequestAutoResize();
			NotifyTableLayoutChanged();
		}

		internal GridColumnSetCollection(GridTable table)
		{
			_ownerTable = table;
		}

		internal MxDataGrid ComputeDataGrid()
		{
			if (_ownerTable == null)
			{
				return null;
			}
			return _ownerTable.DataGrid;
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
					GridColumnSet gridColumnSet = (GridColumnSet)enumerator.Current;
					if (gridColumnSet._position > value)
					{
						gridColumnSet._position--;
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
					GridColumnSet gridColumnSet = (GridColumnSet)enumerator.Current;
					if (gridColumnSet._position >= value)
					{
						gridColumnSet._position++;
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

		internal void NotifyTableLayoutChanged()
		{
			if (_ownerTable != null && _ownerTable.CellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				_ownerTable.RefreshDataGrid();
			}
		}

		public GridColumnSet Add()
		{
			GridColumnSet gridColumnSet = new GridColumnSet();
			Add(gridColumnSet);
			return gridColumnSet;
		}

		public void Add(GridColumnSet columnSet)
		{
			if (columnSet == null)
			{
				throw new ArgumentNullException();
			}
			if (columnSet.GetOwnerCollection() != null)
			{
				throw new InvalidOperationException();
			}
			base.InnerList.Add(columnSet);
			columnSet.SetOwnerCollection(this);
			columnSet._index = base.Count - 1;
			columnSet._position = columnSet._index;
			RecalculateColumnPositions();
			RequestAutoResize();
			NotifyTableLayoutChanged();
		}

		public void AddRange(GridColumnSet[] columnSets)
		{
			for (int i = 0; i < columnSets.Length; i++)
			{
				if (columnSets[i].GetOwnerCollection() == null)
				{
					base.InnerList.Add(columnSets[i]);
					columnSets[i].SetOwnerCollection(this);
					columnSets[i]._index = base.Count - 1;
					columnSets[i]._position = base.Count - 1;
				}
			}
			RecalculateColumnPositions();
			RequestAutoResize();
			NotifyTableLayoutChanged();
		}

		public GridColumnSet GetColumnSetInPosition(int position)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumnSet gridColumnSet = (GridColumnSet)enumerator.Current;
					if (gridColumnSet._position == position)
					{
						return gridColumnSet;
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
			throw new ArgumentOutOfRangeException();
		}

		internal void ProcessArrayList(ArrayList arrayList)
		{
			GridColumnSet[] array = new GridColumnSet[base.Count];
			base.InnerList.CopyTo(array);
			GridColumnSet[] otherArray = array;
			foreach (GridColumnSet gridColumnSet in otherArray)
			{
				gridColumnSet._isRemoved = true;
			}
			base.InnerList.Clear();
			foreach (GridColumnSet item in arrayList)
			{
				base.InnerList.Add(item);
				item._isRemoved = false;
				item.SetOwnerCollection(this);
				item._index = base.Count - 1;
				item._position = base.Count - 1;
			}
			GridColumnSet[] nextArray = array;
			foreach (GridColumnSet columnSet in nextArray)
			{
				if (columnSet._isRemoved)
				{
					columnSet.Clear();
					columnSet.SetOwnerCollection(null);
				}
			}
			RecalculateColumnPositions();
			RequestAutoResize();
			NotifyTableLayoutChanged();
		}

		protected override void OnClear()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumnSet gridColumnSet = (GridColumnSet)enumerator.Current;
					gridColumnSet.Clear();
					gridColumnSet.SetOwnerCollection(null);
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

		protected override void OnClearComplete()
		{
			RecalculateColumnPositions();
			RequestAutoResize();
			NotifyTableLayoutChanged();
		}

		public new void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		public void Remove(GridColumnSet columnSet)
		{
			ShiftPositionsAfterRemove(columnSet._position);
			columnSet.Clear();
			columnSet.SetOwnerCollection(null);
			base.InnerList.Remove(columnSet);
			RenumberIndexes();
			RecalculateColumnPositions();
			RequestAutoResize();
			NotifyTableLayoutChanged();
		}

		internal void DisposeResources()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridColumnSet gridColumnSet = (GridColumnSet)enumerator.Current;
					gridColumnSet.ApplyWidthsToColumns();
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

		private void RenumberIndexes()
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i]._index = i;
			}
		}

		internal void RecalculateColumnPositions()
		{
			if (_ownerTable == null)
			{
				return;
			}
			GridColumnSet[] array = new GridColumnSet[base.Count];
			for (int i = 0; i < base.Count; i++)
			{
				GridColumnSet gridColumnSet = this[i];
				array[gridColumnSet._position] = gridColumnSet;
			}
			GridColumnCollection columns = _ownerTable.Columns;
			foreach (GridColumn item in columns)
			{
				item._gridColumnPosition = -1;
			}
			int gridColumnPosition = 0;
			_countedColumns = 0;
			if (_ownerTable == null || ComputeDataGrid() == null)
			{
				return;
			}
			if (ComputeDataGrid().ColumnSetNavigation == ColumnSetNavigation.Row && ComputeDataGrid().View == View.TableView)
			{
				for (int j = 0; j < _ownerTable.ColumnSetRowCount; j++)
				{
					for (int k = 0; k < base.Count; k++)
					{
						GridColumnSet arrayItem = array[k];
						if (arrayItem == null)
						{
							continue;
						}
						for (int n = 0; n < arrayItem.ColumnCount; n++)
						{
							GridColumn column = arrayItem[j, n];
							if (column != null && column._gridColumnPosition == -1)
							{
								column._gridColumnPosition = gridColumnPosition;
								_countedColumns++;
								gridColumnPosition++;
							}
						}
					}
				}
				return;
			}
			for (int m = 0; m < base.Count; m++)
			{
				GridColumnSet otherArrayItem = array[m];
				for (int n = 0; n < _ownerTable.ColumnSetRowCount; n++)
				{
					for (int value = 0; value < otherArrayItem.ColumnCount; value++)
					{
						GridColumn otherColumn = otherArrayItem[n, value];
						if (otherColumn != null && otherColumn._gridColumnPosition == -1)
						{
							otherColumn._gridColumnPosition = gridColumnPosition;
							_countedColumns++;
							gridColumnPosition++;
						}
					}
				}
			}
		}

		internal int GetColumnSetColumnCount()
		{
			return _countedColumns;
		}

		internal void NotifyVisibilityChanged(GridColumnSet gridColumnSet)
		{
			if (_ownerTable != null)
			{
				_ownerTable.InvalidateGridLayout();
			}
		}

		private void RequestAutoResize()
		{
			if (ComputeDataGrid() != null && ComputeDataGrid().ColumnAutoResize)
			{
				_ownerTable._isAutoResizePending = true;
			}
		}
	}
}
