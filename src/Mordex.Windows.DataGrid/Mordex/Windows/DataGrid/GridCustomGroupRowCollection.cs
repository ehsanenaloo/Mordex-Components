using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	[Editor("Mordex.Windows.DataGrid.Design.CustomGroupRowsEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
	public class GridCustomGroupRowCollection : MordexCustomGroupRowCollectionBase, IMordexXmlLayoutsSupport
	{
		public GridCustomGroupRow this[int index] => (GridCustomGroupRow)BaseGet(index);

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Row";

		internal GridCustomGroupRowCollection(GridCustomGroup owner)
			: base(owner)
		{
		}

		public int Add(GridCustomGroupRow customGroupRow)
		{
			return BaseAdd(customGroupRow);
		}

		public void Insert(int index, GridCustomGroupRow customGroupRow)
		{
			BaseInsert(index, customGroupRow);
		}

		public void Remove(GridCustomGroupRow customGroupRow)
		{
			BaseRemove(customGroupRow);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		internal GridTable GetOwnerTable()
		{
			if (owner is GridCustomGroup gridCustomGroup)
			{
				return gridCustomGroup.GetOwnerTable();
			}
			return null;
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader mordexReader)
		{
			int value = 0;
			for (PropertyValue propertyValue = mordexReader["Row" + value]; propertyValue != null; propertyValue = mordexReader["Row" + value])
			{
				GridCustomGroupRow gridCustomGroupRow = new GridCustomGroupRow();
				gridCustomGroupRow._ownerTable = GetOwnerTable();
				propertyValue.DeserializeObject(gridCustomGroupRow);
				gridCustomGroupRow._ownerTable = null;
				BaseAdd(gridCustomGroupRow);
				value++;
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter mordexWriter)
		{
			int otherValue = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					GridCustomGroupRow value = (GridCustomGroupRow)enumerator.Current;
					mordexWriter.AddValue("Row" + otherValue, value);
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
	}
}
