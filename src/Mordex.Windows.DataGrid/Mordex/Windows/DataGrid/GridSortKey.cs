using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid
{
	public class GridSortKey : MordexSortFieldBase, IMordexXmlLayoutsSupport
	{
		internal GridTable _ownerTable;

		string IMordexXmlLayoutsSupport.Identifier => "SortKey" + Index;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(SortOrder.Ascending)]
		[Description("Represents the order in which values will be sorted.")]
		public SortOrder SortOrder
		{
			get
			{
				return DataState.ComputeSortOrder(base.BaseSortOrder);
			}
			set
			{
				if (value == SortOrder.Unsorted)
				{
					throw new InvalidOperationException();
				}
				base.BaseSortOrder = DataState.ToDataSortOrder(value);
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		[Description("Represents the GridColumn in which the sorting occurs.")]
		[DefaultValue(null)]
		[Editor("Mordex.Windows.DataGrid.Design.ColumnEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public GridColumn Column
		{
			get
			{
				return (GridColumn)base.BaseField;
			}
			set
			{
				base.BaseField = value;
			}
		}

		[Browsable(false)]
		public GridTable Table
		{
			get
			{
				if (_ownerTable != null)
				{
					return _ownerTable;
				}
				return (GridTable)base.CollectionOwner;
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			if (Table != null)
			{
				int propertyValue = (int)LayoutReader.GetPropertyValue(info, "ColIndex", typeof(int), -1);
				if (propertyValue >= 0 && propertyValue < Table.Columns.Count)
				{
					Column = Table.Columns[propertyValue];
				}
			}
			SortOrder = (SortOrder)LayoutReader.GetPropertyValue(info, "SortOrder", typeof(SortOrder), SortOrder.Ascending);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Column != null)
			{
				info.AddValue("ColIndex", Column.Index);
			}
			if (SortOrder != SortOrder.Ascending)
			{
				info.AddValue("SortOrder", SortOrder);
			}
		}

		public GridSortKey()
			: this(null)
		{
		}

		public GridSortKey(GridColumn column)
			: this(column, SortOrder.Ascending)
		{
		}

		public GridSortKey(GridColumn column, SortOrder sortOrder)
			: base(column, DataState.ToDataSortOrder(sortOrder))
		{
		}

		internal GridSortKeyCollection GetParentSortKeys()
		{
			return (GridSortKeyCollection)base.BaseParentCollection;
		}

		internal bool BelongsToTable(GridTable gridTable)
		{
			if (Column == null || Column.Table != gridTable)
			{
				return false;
			}
			return true;
		}

		internal void SetBaseParentCollection(GridSortKeyCollection parent)
		{
			BaseSetParentCollection(parent);
		}
	}
}
