using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridStoredFilterCollection : MordexStoredFilterCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "StoredFilter";

		public GridStoredFilter this[int index] => (GridStoredFilter)BaseGet(index);

		public GridStoredFilter this[string key] => (GridStoredFilter)BaseGet(key);

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int value = 0;
			for (PropertyValue propertyValue = info["StoredFilter" + value]; propertyValue != null; propertyValue = info["StoredFilter" + value])
			{
				GridStoredFilter gridStoredFilter = new GridStoredFilter();
				base.List.Add(gridStoredFilter);
				value++;
				propertyValue.DeserializeObject(gridStoredFilter);
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
					GridStoredFilter value = (GridStoredFilter)enumerator.Current;
					info.AddValue("StoredFilter" + otherValue, value);
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

		internal GridStoredFilterCollection(GridTable table)
			: base(table)
		{
		}

		internal GridTable GetOwnerTable()
		{
			return (GridTable)base.BaseTable;
		}

		public int IndexOf(GridStoredFilter storedFilter)
		{
			return BaseIndexOf(storedFilter);
		}

		public int IndexOf(string key)
		{
			return BaseIndexOf(key);
		}

		public void Remove(string key)
		{
			BaseRemove(key);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Add(GridStoredFilter storedFilter)
		{
			BaseAdd(storedFilter);
		}

		public void AddRange(GridStoredFilter[] storedFilters)
		{
			foreach (GridStoredFilter storedFilter in storedFilters)
			{
				BaseAdd(storedFilter);
			}
		}

		public void Remove(GridStoredFilter storedFilter)
		{
			BaseRemove(storedFilter);
		}
	}
}
