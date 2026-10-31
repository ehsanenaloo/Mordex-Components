using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridChildTableCollection : MordexChildTableCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Table";

		public GridTable this[int index] => (GridTable)BaseGet(index);

		public GridTable this[string key] => (GridTable)BaseGet(key);

		public GridTable ParentTable => (GridTable)base.BaseTable;

		public MxDataGrid DataGrid
		{
			get
			{
				if (ParentTable == null)
				{
					return null;
				}
				return ParentTable.DataGrid;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			Clear();
			int value = 0;
			for (PropertyValue propertyValue = info["Table" + value]; propertyValue != null; propertyValue = info["Table" + value])
			{
				GridTable gridTable = new GridTable();
				BaseAdd(gridTable);
				value++;
				propertyValue.DeserializeObject(gridTable);
			}
			RefreshOwnerGrid();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			int otherValue = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridTable value = (GridTable)enumerator.Current;
					info.AddValue("Table" + otherValue, value);
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

		internal GridChildTableCollection(GridTable table)
			: base(table)
		{
		}

		private void RefreshOwnerGrid()
		{
			MxDataGrid dataGrid = DataGrid;
			if (dataGrid != null)
			{
				dataGrid.RebuildTableCollection();
				dataGrid.Refetch();
			}
		}

		public void Add(GridTable table)
		{
			BaseAdd(table);
			RefreshOwnerGrid();
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			RefreshOwnerGrid();
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
			RefreshOwnerGrid();
		}

		public void Remove(GridTable table)
		{
			if (table == null)
			{
				throw new ArgumentNullException();
			}
			BaseRemove(table);
			RefreshOwnerGrid();
		}

		public void Remove(string key)
		{
			BaseRemove(key);
			RefreshOwnerGrid();
		}

		public int IndexOf(GridTable table)
		{
			return BaseIndexOf(table);
		}

		public int IndexOf(string key)
		{
			return BaseIndexOf(key);
		}

		public bool Contains(GridTable table)
		{
			return BaseContains(table);
		}

		public bool Contains(string key)
		{
			return BaseContains(key);
		}
	}
}
