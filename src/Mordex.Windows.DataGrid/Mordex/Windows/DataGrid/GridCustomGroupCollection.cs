using System;
using System.Collections;
using System.ComponentModel;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridCustomGroupCollection : MordexCustomGroupCollectionBase, IMordexXmlLayoutsSupport
	{
		[Browsable(false)]
		public GridTable Table => owner as GridTable;

		public GridCustomGroup this[int index] => (GridCustomGroup)BaseGet(index);

		public GridCustomGroup this[string key] => (GridCustomGroup)BaseGet(key);

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "CustomGroup";

		internal GridCustomGroupCollection(GridTable owner)
			: base(owner)
		{
		}

		public int Add(GridCustomGroup customGroup)
		{
			return BaseAdd(customGroup);
		}

		public void Remove(GridCustomGroup customGroup)
		{
			BaseRemove(customGroup);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Remove(string key)
		{
			BaseRemove(key);
		}

		public bool Contains(string key)
		{
			return BaseContains(key);
		}

		public bool Contains(GridCustomGroup customGroup)
		{
			return BaseContains(customGroup);
		}

		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			RefreshOwnerGrid();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			base.OnRemoveComplete(index, value);
			RefreshOwnerGrid();
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			RefreshOwnerGrid();
		}

		private void RefreshOwnerGrid()
		{
			if (owner is GridTable gridTable && gridTable.DataGrid != null)
			{
				gridTable.DataGrid.NotifyRefreshListeners();
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int value = 0;
			for (PropertyValue propertyValue = info["CustomGroup" + value]; propertyValue != null; propertyValue = info["CustomGroup" + value])
			{
				GridCustomGroup gridCustomGroup = new GridCustomGroup();
				gridCustomGroup._ownerTable = Table;
				propertyValue.DeserializeObject(gridCustomGroup);
				gridCustomGroup._ownerTable = null;
				BaseAdd(gridCustomGroup);
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
					GridCustomGroup value = (GridCustomGroup)enumerator.Current;
					info.AddValue("CustomGroup" + otherValue, value);
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
