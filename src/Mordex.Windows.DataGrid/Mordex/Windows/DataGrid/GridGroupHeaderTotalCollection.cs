using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridGroupHeaderTotalCollection : MordexGroupTotalCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "GroupHeaderTotal";

		public GridTable Table => (GridTable)base.BaseTable;

		public GridGroupHeaderTotal this[int index] => (GridGroupHeaderTotal)BaseGet(index);

		public GridGroupHeaderTotal this[string key] => (GridGroupHeaderTotal)BaseGet(key);

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int value = 0;
			for (PropertyValue propertyValue = info["GroupHeaderTotal" + value]; propertyValue != null; propertyValue = info["GroupHeaderTotal" + value])
			{
				GridGroupHeaderTotal gridGroupHeaderTotal = new GridGroupHeaderTotal();
				Add(gridGroupHeaderTotal);
				propertyValue.DeserializeObject(gridGroupHeaderTotal);
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
					GridGroupHeaderTotal value = (GridGroupHeaderTotal)enumerator.Current;
					info.AddValue("GroupHeaderTotal" + otherValue, value);
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

		internal GridGroupHeaderTotalCollection(GridTable table)
			: base(table)
		{
		}

		public void Add(GridGroupHeaderTotal groupHeaderTotal)
		{
			BaseAdd(groupHeaderTotal);
		}

		public void AddRange(GridGroupHeaderTotal[] groupHeaderTotals)
		{
			foreach (GridGroupHeaderTotal gridGroupHeaderTotal in groupHeaderTotals)
			{
				if (gridGroupHeaderTotal == null)
				{
					throw new ArgumentNullException();
				}
				BaseAdd(gridGroupHeaderTotal);
			}
		}

		public void Remove(GridGroupHeaderTotal groupTotal)
		{
			BaseRemove(groupTotal);
		}

		public int IndexOf(GridGroupHeaderTotal groupHeaderTotal)
		{
			return ((IList)this).IndexOf((object)groupHeaderTotal);
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			RefetchGridIfGrouped();
		}

		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			RefetchGridIfGrouped();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			base.OnRemoveComplete(index, value);
			RefetchGridIfGrouped();
		}

		internal void RefetchGridIfGrouped()
		{
			if (Table != null && Table.Groups.Count > 0)
			{
				Table.DataGrid?.Refetch();
			}
		}
	}
}
