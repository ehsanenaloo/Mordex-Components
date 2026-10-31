using System;
using System.ComponentModel;
using System.Drawing.Design;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;

namespace Mordex.Windows.DataGrid
{
	public class GridGroup : MordexGroupBase, IMordexXmlLayoutsSupport
	{
		private bool _allowRemove = true;

		private string _groupPrefix;

		private string _headerCaption;

		internal GridTable _ownerTable;

		private static readonly SortOrder _sortOrder = SortOrder.Ascending;

		string IMordexXmlLayoutsSupport.Identifier => "Group" + Index;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("The text displayed in a group header row before the group row value.")]
		public string GroupPrefix
		{
			get
			{
				if (_groupPrefix == null)
				{
					if (Column != null)
					{
						return Column.DefaultGroupPrefix;
					}
					return "";
				}
				return _groupPrefix;
			}
			set
			{
				if (value != _groupPrefix)
				{
					_groupPrefix = value;
					if (ComputeDataGrid() != null)
					{
						ComputeDataGrid().RebuildRows(true);
					}
				}
			}
		}

		[DefaultValue(true)]
		public bool AllowRemove
		{
			get
			{
				return _allowRemove;
			}
			set
			{
				_allowRemove = value;
			}
		}

		[Description("The text displayed in the group by box header.")]
		public string HeaderCaption
		{
			get
			{
				if (_headerCaption == null)
				{
					if (CustomGroup != null)
					{
						return CustomGroup.HeaderCaption;
					}
					if (Column != null)
					{
						return Column.Caption;
					}
					return "";
				}
				return _headerCaption;
			}
			set
			{
				if (_headerCaption != value)
				{
					_headerCaption = value;
					if (ComputeDataGrid() != null)
					{
						ComputeDataGrid().RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Description("Represents the format to use when rendering group row values.")]
		public string GroupFormatString
		{
			get
			{
				return base.BaseGroupFormatString;
			}
			set
			{
				base.BaseGroupFormatString = value;
			}
		}

		[Description("Determines how values will be formatted in group header rows.")]
		public FormatMode GroupFormatMode
		{
			get
			{
				return DataState.ToGridFormatMode(base.BaseGroupFormatMode);
			}
			set
			{
				base.BaseGroupFormatMode = DataState.ToDataFormatMode(value);
			}
		}

		[Description("Determines how groups should be calculated.")]
		public GroupInterval GroupInterval
		{
			get
			{
				return DataState.ToGridGroupInterval(base.BaseGroupInterval);
			}
			set
			{
				base.BaseGroupInterval = DataState.ComputeGroupInterval(value);
			}
		}

		[Description("The CustomGroup used to create the group rows.")]
		[Editor("Mordex.Windows.DataGrid.Design.CustomGroupEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.CustomGroupConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(null)]
		public GridCustomGroup CustomGroup
		{
			get
			{
				return (GridCustomGroup)base.BaseCustomGroup;
			}
			set
			{
				base.BaseCustomGroup = value;
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.ColumnConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(null)]
		[Description("Represents the GridColumn in which the grouping occurs.")]
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

		[Description("Represents the order in which groups will be sorted.")]
		[DefaultValue(SortOrder.Ascending)]
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

		public GroupTotalVisibility GroupTotalVisibility
		{
			get
			{
				return DataState.ToGridGroupTotalVisibility(base.BaseGroupTotalVisibility);
			}
			set
			{
				base.BaseGroupTotalVisibility = DataState.ComputeGroupTotalVisibility(value);
			}
		}

		[Browsable(false)]
		public GridTable Table => (GridTable)base.CollectionOwner;

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			GridTable gridTable = GetEffectiveTable();
			if (gridTable != null)
			{
				int otherPropertyValue = (int)LayoutReader.GetPropertyValue(info, "ColIndex", typeof(int), -1);
				int nextPropertyValue = (int)LayoutReader.GetPropertyValue(info, "TableColumnIndex", typeof(int), -1);
				GridTable otherGridTable = gridTable;
				while (otherGridTable.ParentTable != null)
				{
					otherGridTable = otherGridTable.ParentTable;
				}
				GridTable table;
				if (nextPropertyValue != -1)
				{
					GridTableCollection gridTableCollection = MxDataGrid.CreateTableCollection(otherGridTable);
					table = ((nextPropertyValue < 0 || nextPropertyValue >= gridTableCollection.Count) ? Table : gridTableCollection[nextPropertyValue]);
				}
				else
				{
					table = gridTable;
				}
				int previousPropertyValue = (int)LayoutReader.GetPropertyValue(info, "ColumnCount", typeof(int), 1);
				GridColumn gridColumn = null;
				if (otherPropertyValue >= 0 && otherPropertyValue < table.Columns.Count)
				{
					gridColumn = table.Columns[otherPropertyValue];
				}
				if (previousPropertyValue > 1)
				{
					GridColumn[] array = new GridColumn[previousPropertyValue];
					array[0] = gridColumn;
					for (int i = 1; i < previousPropertyValue; i++)
					{
						if (otherPropertyValue >= 0 && otherPropertyValue < table.Columns.Count)
						{
							array[i] = table.Columns[otherPropertyValue];
						}
					}
				}
				else
				{
					Column = gridColumn;
				}
				int firstPropertyValue = (int)LayoutReader.GetPropertyValue(info, "CustomGroupIndex", typeof(int), -1);
				if (firstPropertyValue >= 0 && firstPropertyValue < gridTable.CustomGroups.Count)
				{
					CustomGroup = gridTable.CustomGroups[firstPropertyValue];
				}
				else
				{
					PropertyValue propertyValue = info["CustomGroup"];
					if (propertyValue != null)
					{
						GridCustomGroup gridCustomGroup = new GridCustomGroup();
						gridCustomGroup._ownerTable = gridTable;
						propertyValue.DeserializeObject(gridCustomGroup);
						gridCustomGroup._ownerTable = null;
						CustomGroup = gridCustomGroup;
					}
				}
			}
			GroupFormatString = (string)LayoutReader.GetPropertyValue(info, "GroupFormatString", typeof(string), null);
			_groupPrefix = (string)LayoutReader.GetPropertyValue(info, "GroupPrefix", typeof(string), null);
			_headerCaption = (string)LayoutReader.GetPropertyValue(info, "HeaderCaption", typeof(string), null);
			GroupInterval = (GroupInterval)LayoutReader.GetPropertyValue(info, "GroupInterval", typeof(GroupInterval), GroupInterval.Default);
			SortOrder = (SortOrder)LayoutReader.GetPropertyValue(info, "SortOrder", typeof(SortOrder), SortOrder.Ascending);
			GroupFormatMode = (FormatMode)LayoutReader.GetPropertyValue(info, "GroupFormatMode", typeof(FormatMode), FormatMode.UseDefault);
			_allowRemove = (bool)LayoutReader.GetPropertyValue(info, "AllowRemove", typeof(bool), true);
			GroupTotalVisibility = (GroupTotalVisibility)LayoutReader.GetPropertyValue(info, "GroupTotalVisibility", typeof(GroupTotalVisibility), GroupTotalVisibility.Default);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (Column != null && Column.Index != -1)
			{
				info.AddValue("ColIndex", Column.Index);
				if (Column.Table != Table && ComputeDataGrid() != null)
				{
					int indexOfTable = ComputeDataGrid().Tables.IndexOfTable(Column.Table);
					if (indexOfTable != -1)
					{
						info.AddValue("TableColumnIndex", indexOfTable);
					}
				}
			}
			MordexFieldBase[] baseFields = base.BaseFields;
			if (baseFields != null && baseFields.Length > 1)
			{
				info.AddValue("ColumnCount", baseFields.Length);
				for (int i = 1; i < baseFields.Length; i++)
				{
					GridColumn gridColumn = (GridColumn)baseFields[i];
					info.AddValue("ColIndex" + i, gridColumn.Index);
				}
			}
			if (ShouldSerializeGroupFormatString())
			{
				info.AddValue("GroupFormatString", GroupFormatString);
			}
			if (_groupPrefix != null)
			{
				info.AddValue("GroupPrefix", _groupPrefix);
			}
			if (_headerCaption != null)
			{
				info.AddValue("HeaderCaption", _headerCaption);
			}
			if (ShouldSerializeBaseGroupInterval())
			{
				info.AddValue("GroupInterval", GroupInterval);
			}
			if (SortOrder != SortOrder.Ascending)
			{
				info.AddValue("SortOrder", SortOrder);
			}
			if (ShouldSerializeGroupFormatMode())
			{
				info.AddValue("GroupFormatMode", GroupFormatMode);
			}
			if (GroupTotalVisibility != GroupTotalVisibility.Default)
			{
				info.AddValue("GroupTotalVisibility", GroupTotalVisibility);
			}
			if (CustomGroup != null)
			{
				if (CustomGroup.Index == -1)
				{
					info.AddValue("CustomGroup", CustomGroup);
				}
				else
				{
					info.AddValue("CustomGroupIndex", CustomGroup.Index);
				}
			}
			if (!_allowRemove)
			{
				info.AddValue("AllowRemove", _allowRemove);
			}
		}

		public GridGroup()
			: this(null, _sortOrder)
		{
		}

		public GridGroup(GridColumn column)
			: this(column, _sortOrder)
		{
		}

		public GridGroup(GridColumn column, SortOrder sortOrder)
			: base(column, DataState.ToDataSortOrder(sortOrder))
		{
		}

		public GridGroup(GridColumn column, SortOrder sortOrder, GroupInterval interval)
			: base(column, DataState.ToDataSortOrder(sortOrder))
		{
			GroupInterval = interval;
		}

		public GridGroup(GridCustomGroup customGroup)
		{
			CustomGroup = customGroup;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGroupPrefix()
		{
			return _groupPrefix != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGroupPrefix()
		{
			_groupPrefix = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderCaption()
		{
			return _headerCaption != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderCaption()
		{
			_headerCaption = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGroupFormatString()
		{
			return BaseShouldSerializeGroupFormatString();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGroupFormatString()
		{
			BaseResetGroupFormatString();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGroupFormatMode()
		{
			return BaseShouldSerializeGroupFormatMode();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGroupFormatMode()
		{
			BaseResetGroupFormatMode();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGroupInterval()
		{
			return ShouldSerializeBaseGroupInterval();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGroupInterval()
		{
			GroupInterval = GroupInterval.Default;
		}

		internal GridGroupCollection GetParentCollection()
		{
			return (GridGroupCollection)base.BaseParentCollection;
		}

		internal GridFormatStyle GetGroupRowStyle()
		{
			if (Column != null)
			{
				return Column.GroupRowStyle;
			}
			return null;
		}

		internal GridTable GetEffectiveTable()
		{
			if (_ownerTable != null)
			{
				return _ownerTable;
			}
			return Table;
		}

		internal MxDataGrid ComputeDataGrid()
		{
			return GetEffectiveTable()?.DataGrid;
		}

		internal bool CanGroupByTable(GridTable gridTable)
		{
			if (gridTable == null)
			{
				return true;
			}
			if (CustomGroup != null)
			{
				return true;
			}
			if (Column == null)
			{
				return false;
			}
			if (gridTable == Column.Table)
			{
				return true;
			}
			return gridTable.CanGroupBy(Column);
		}

		public void Expand()
		{
			if (ComputeDataGrid() != null)
			{
				ComputeDataGrid().ExpandGroupInSource(this);
				return;
			}
			throw new InvalidOperationException();
		}

		public void Collapse()
		{
			if (ComputeDataGrid() != null)
			{
				ComputeDataGrid().CollapseGroupInSource(this);
				return;
			}
			throw new InvalidOperationException();
		}

		public GridGroup Clone()
		{
			GridGroup gridGroup = new GridGroup();
			gridGroup.AllowRemove = AllowRemove;
			gridGroup.Column = Column;
			gridGroup.CustomGroup = CustomGroup;
			gridGroup.GroupFormatMode = GroupFormatMode;
			gridGroup.GroupFormatString = GroupFormatString;
			gridGroup.GroupInterval = GroupInterval;
			gridGroup.GroupPrefix = GroupPrefix;
			gridGroup.HeaderCaption = HeaderCaption;
			gridGroup.SortOrder = SortOrder;
			gridGroup.GroupTotalVisibility = GroupTotalVisibility;
			return gridGroup;
		}
	}
}
