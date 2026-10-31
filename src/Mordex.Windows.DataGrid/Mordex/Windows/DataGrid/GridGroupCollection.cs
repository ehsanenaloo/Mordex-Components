using System;
using System.Collections;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.DataGrid
{
	public class GridGroupCollection : MordexGroupCollectionBase, IMordexXmlLayoutsSupport
	{
		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => true;

		string IMordexXmlLayoutsSupport.CollectionElementName => "Group";

		public GridGroup this[int index] => (GridGroup)BaseGet(index);

		public GridGroup this[GridColumn column]
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						GridGroup gridGroup = (GridGroup)enumerator.Current;
						if (gridGroup.Column == column)
						{
							return gridGroup;
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

		public GridGroup this[string columnKey] => (GridGroup)BaseGet(columnKey);

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			int value = 0;
			for (PropertyValue propertyValue = info["Group" + value]; propertyValue != null; propertyValue = info["Group" + value])
			{
				GridGroup gridGroup = new GridGroup();
				gridGroup._ownerTable = GetOwnerTable();
				propertyValue.DeserializeObject(gridGroup);
				gridGroup._ownerTable = null;
				BaseAdd(gridGroup);
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
					GridGroup value = (GridGroup)enumerator.Current;
					info.AddValue("Group" + otherValue, value);
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

		internal GridGroupCollection(GridTable table)
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

		public int Add(GridGroup group)
		{
			return BaseAdd(group);
		}

		public GridGroup Add(GridColumn column)
		{
			return Add(column, SortOrder.Ascending);
		}

		public GridGroup Add(GridColumn column, SortOrder order)
		{
			GridGroup gridGroup = new GridGroup(column, order);
			BaseAdd(gridGroup);
			return gridGroup;
		}

		public GridGroup Add(string columnKey)
		{
			return Add(columnKey, SortOrder.Ascending);
		}

		public GridGroup Add(string columnKey, SortOrder sortOrder)
		{
			GridColumn gridColumn = GetOwnerTable().Columns[columnKey];
			if (gridColumn == null)
			{
				throw new ArgumentOutOfRangeException("There is no column with Key '" + columnKey + "' in the Table.");
			}
			return Add(gridColumn, sortOrder);
		}

		protected override bool OnInsert(int index, object value)
		{
			_ = (GridGroup)value;
			if (ComputeDataGrid() != null && ComputeDataGrid().View != View.TableView)
			{
				throw new InvalidOperationException("Groups can not be added in card view");
			}
			if (ComputeDataGrid() != null && !ComputeDataGrid().UpdateData())
			{
				return false;
			}
			return base.OnInsert(index, value);
		}

		public void AddRange(GridGroup[] groups)
		{
			if (groups.Length > 0 && ComputeDataGrid() != null && ComputeDataGrid().View != View.TableView)
			{
				throw new InvalidOperationException("Groups can not be added in card view");
			}
			if (ComputeDataGrid() != null && !ComputeDataGrid().UpdateData())
			{
				return;
			}
			foreach (GridGroup gridGroup in groups)
			{
				if (gridGroup == null)
				{
					throw new ArgumentNullException();
				}
				BaseAdd(gridGroup);
			}
		}

		public void Insert(int index, GridGroup group)
		{
			if (index > base.Count)
			{
				index = base.Count;
			}
			BaseInsert(index, group);
		}

		internal void MoveGroup(int index, GridGroup group)
		{
			if (index != group.Index && index != group.Index + 1)
			{
				if (index > group.Index)
				{
					index--;
				}
				BaseRemove(group);
				BaseInsert(index, group);
			}
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Remove(GridGroup group)
		{
			BaseRemove(group);
		}

		private void RefreshGrid()
		{
			if (ComputeDataGrid() != null)
			{
				ComputeDataGrid().RefreshRowCountAndInvalidate();
				ComputeDataGrid().NotifyRefreshListeners();
				ComputeDataGrid().ScrollToFirstRow();
			}
		}

		protected override void OnClearComplete()
		{
			base.OnClearComplete();
			RefreshGrid();
		}

		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			RefreshGrid();
		}

		protected override void OnRemoveComplete(int index, object value)
		{
			base.OnRemoveComplete(index, value);
			RefreshGrid();
		}
	}
}
