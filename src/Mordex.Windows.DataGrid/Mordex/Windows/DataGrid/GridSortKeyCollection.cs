using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridSortKeyCollection : MordexSortFieldCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "SortKey";

		public GridSortKey this[int index] => (GridSortKey)BaseGet(index);

		public GridSortKey this[GridColumn column]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridSortKey gridSortKey = (GridSortKey)enumerator.Current;
						if (gridSortKey.Column == column)
						{
							return gridSortKey;
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
		}

		public GridSortKey this[string columnKey]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridSortKey gridSortKey = (GridSortKey)enumerator.Current;
						if (gridSortKey.Column != null && string.Compare(gridSortKey.Column.Key, columnKey, ignoreCase: true) == 0)
						{
							return gridSortKey;
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
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int value = 0;
			for (PropertyValue propertyValue = info["SortKey" + value]; propertyValue != null; propertyValue = info["SortKey" + value])
			{
				GridSortKey gridSortKey = new GridSortKey();
				gridSortKey._ownerTable = GetOwnerTable();
				propertyValue.DeserializeObject(gridSortKey);
				gridSortKey._ownerTable = null;
				BaseAdd(gridSortKey);
				value++;
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
					GridSortKey value = (GridSortKey)enumerator.Current;
					info.AddValue("SortKey" + otherValue, value);
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

		internal GridSortKeyCollection(GridTable table)
			: base(table)
		{
		}

		internal GridTable GetOwnerTable()
		{
			return (GridTable)base.BaseTable;
		}

		internal MxDataGrid ComputeDataGrid()
		{
			if (GetOwnerTable() != null)
			{
				return GetOwnerTable().DataGrid;
			}
			return null;
		}

		public int Add(GridSortKey sortKey)
		{
			return BaseAdd(sortKey);
		}

		public GridSortKey Add(GridColumn column)
		{
			return Add(column, SortOrder.Ascending);
		}

		public GridSortKey Add(GridColumn column, SortOrder sortOrder)
		{
			GridSortKey gridSortKey = new GridSortKey(column, sortOrder);
			Add(gridSortKey);
			return gridSortKey;
		}

		public GridSortKey Add(string columnKey)
		{
			return Add(columnKey, SortOrder.Ascending);
		}

		public GridSortKey Add(string columnKey, SortOrder sortOrder)
		{
			GridColumn gridColumn = GetOwnerTable().Columns[columnKey];
			if (gridColumn == null)
			{
				throw new ArgumentOutOfRangeException("There is no column with Key '" + columnKey + "' in the table.");
			}
			return Add(gridColumn, sortOrder);
		}

		public void AddRange(GridSortKey[] sortKeys)
		{
			foreach (GridSortKey sortField in sortKeys)
			{
				BaseAdd(sortField);
			}
		}

		public void RemoveAt(int index)
		{
			Remove(this[index]);
		}

		public void Remove(GridSortKey sortKey)
		{
			BaseRemove(sortKey);
		}
	}
}
