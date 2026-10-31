using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Text;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridTable
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			InitializeCollections();
			AllowAddNew = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "AllowAddNew", typeof(InheritableBoolean), InheritableBoolean.Default);
			AllowChildTableGroups = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "AllowChildTableGroups", typeof(InheritableBoolean), InheritableBoolean.Default);
			DataMember = (string)LayoutReader.GetPropertyValue(info, "DataMember", typeof(string), string.Empty);
			DynamicFiltering = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "DynamicFiltering", typeof(InheritableBoolean), InheritableBoolean.Default);
			FilterParentRows = (bool)LayoutReader.GetPropertyValue(info, "FilterParentRows", typeof(bool), true);
			GroupTotals = (GroupTotals)LayoutReader.GetPropertyValue(info, "GroupTotals", typeof(GroupTotals), GroupTotals.Default);
			Key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			NewRowPosition = (NewRowPosition)LayoutReader.GetPropertyValue(info, "NewRowPosition", typeof(NewRowPosition), NewRowPosition.Default);
			Caption = (string)LayoutReader.GetPropertyValue(info, "Caption", typeof(string), string.Empty);
			_tag = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			_imageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey", typeof(string), string.Empty);
			_allowRemoveColumns = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "AllowRemoveColumns", typeof(InheritableBoolean), InheritableBoolean.Default);
			_allowDelete = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "AllowDelete", typeof(InheritableBoolean), InheritableBoolean.Default);
			_allowEdit = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "AllowEdit", typeof(InheritableBoolean), InheritableBoolean.Default);
			_allowGroup = (bool)LayoutReader.GetPropertyValue(info, "AllowGroup", typeof(bool), true);
			_cellLayoutMode = (CellLayoutMode)LayoutReader.GetPropertyValue(info, "CellLayoutMode", typeof(CellLayoutMode), CellLayoutMode.UseColumns);
			if (!Enum.IsDefined(typeof(CellLayoutMode), _cellLayoutMode))
			{
				_cellLayoutMode = CellLayoutMode.UseColumns;
			}
			_cellToolTip = (CellToolTip)LayoutReader.GetPropertyValue(info, "CellToolTip", typeof(CellToolTip), CellToolTip.Default);
			_cellToolTipContent = (string)LayoutReader.GetPropertyValue(info, "CellToolTipText", typeof(string), null);
			info.DeserializeObject("ChildTables", ChildTables);
			_columnHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "ColumnHeaders", typeof(InheritableBoolean), InheritableBoolean.Default);
			info.DeserializeObject("Columns", Columns);
			info.DeserializeObject("SortKeys", SortKeys);
			_columnSetHeaderLines = (int)LayoutReader.GetPropertyValue(info, "ColumnSetHeaderLines", typeof(int), 1);
			_columnSetHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "ColumnSetHeaders", typeof(InheritableBoolean), InheritableBoolean.Default);
			_columnSetRowCount = (int)LayoutReader.GetPropertyValue(info, "ColumnSetRowCount", typeof(int), 2);
			info.DeserializeObject("ColumnSets", _columnSets);
			info.DeserializeObject("FormatConditions", _formatConditions);
			_lineColor = (Color)LayoutReader.GetPropertyValue(info, "GridLineColor", typeof(Color), Color.Empty);
			_gridLines = (GridLines)LayoutReader.GetPropertyValue(info, "GridLines", typeof(GridLines), GridLines.Default);
			_gridLineStyle = (GridLineStyle)LayoutReader.GetPropertyValue(info, "GridLineStyle", typeof(GridLineStyle), GridLineStyle.Default);
			info.DeserializeObject("GroupCondition", _groupCondition);
			_groupConditionCountTitle = (string)LayoutReader.GetPropertyValue(info, "GroupConditionCountTitle", typeof(string), "");
			GroupMode = (GroupMode)LayoutReader.GetPropertyValue(info, "GroupMode", typeof(GroupMode), GroupMode.Default);
			info.DeserializeObject("CustomGroups", CustomGroups);
			foreach (GridColumn column in Columns)
			{
				column.ResolveCustomGroupFromIndex();
			}
			info.DeserializeObject("Groups", Groups);
			info.DeserializeObject("GroupHeaderTotals", GroupHeaderTotals);
			info.DeserializeObject("StoredFilters", StoredFilters);
			_headerLines = (int)LayoutReader.GetPropertyValue(info, "HeaderLines", typeof(int), 1);
			_newRowCaption = (string)LayoutReader.GetPropertyValue(info, "NewRowInfoText", typeof(string), "");
			_previewRow = (bool)LayoutReader.GetPropertyValue(info, "PreviewRow", typeof(bool), false);
			_previewRowLeftMargin = (int)LayoutReader.GetPropertyValue(info, "PreviewRowLeftMargin", typeof(int), 16);
			_previewRowLines = (int)LayoutReader.GetPropertyValue(info, "PreviewRowLines", typeof(int), 3);
			_previewRowMember = (string)LayoutReader.GetPropertyValue(info, "PreviewRowMember", typeof(string), string.Empty);
			_repeatHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "RepeatHeaders", typeof(InheritableBoolean), InheritableBoolean.Default);
			_rowHeaders = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "RowHeaders", typeof(InheritableBoolean), InheritableBoolean.Default);
			_rowHeaderWidth = (int)LayoutReader.GetPropertyValue(info, "RowHeaderWidth", typeof(int), -1);
			_rowHeight = (int)LayoutReader.GetPropertyValue(info, "RowHeight", typeof(int), -1);
			_showGroupConditionCount = (bool)LayoutReader.GetPropertyValue(info, "ShowGroupConditionCount", typeof(bool), false);
			_tableHeader = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "TableHeader", typeof(InheritableBoolean), InheritableBoolean.Default);
			_imageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex", typeof(int), -1);
			TotalRow = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "TotalRow", typeof(InheritableBoolean), InheritableBoolean.Default);
			_useGroupCondition = (bool)LayoutReader.GetPropertyValue(info, "UseGroupCondition", typeof(bool), false);
			HierarchicalMode = (HierarchicalMode)LayoutReader.GetPropertyValue(info, "HierarchicalMode", typeof(HierarchicalMode), HierarchicalMode.UseChildTables);
			_useGroupRowSelector = (InheritableBoolean)LayoutReader.GetPropertyValue(info, "UseGroupRowSelector", typeof(InheritableBoolean), InheritableBoolean.Default);
			_rowHeaderContent = (RowHeaderContent)LayoutReader.GetPropertyValue(info, "RowHeaderContent", typeof(RowHeaderContent), RowHeaderContent.Default);
			PropertyValue propertyValue = info["SelfReferencingSettings"];
			if (propertyValue != null)
			{
				GridSelfReferencingSettings gridSelfReferencingSettings = new GridSelfReferencingSettings(null);
				gridSelfReferencingSettings._owner = this;
				propertyValue.DeserializeObject(gridSelfReferencingSettings);
				gridSelfReferencingSettings._owner = null;
				base.BaseSelfReferencingSettings = gridSelfReferencingSettings;
			}
			else
			{
				base.BaseSelfReferencingSettings = new GridSelfReferencingSettings(this);
			}
			PropertyValue infoItem = null;
			infoItem = info["TableHeaderFormatStyle"];
			if (infoItem == null)
			{
				TableHeaderFormatStyle = DeserializeGridTableState(info, "TableHeaderFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(TableHeaderFormatStyle);
			}
			infoItem = info["AlternatingRowFormatStyle"];
			if (infoItem == null)
			{
				AlternatingRowFormatStyle = DeserializeGridTableState(info, "AlternatingRowFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(AlternatingRowFormatStyle);
			}
			infoItem = info["GroupByBoxInfoFormatStyle"];
			if (infoItem == null)
			{
				GroupByBoxInfoFormatStyle = DeserializeGridTableState(info, "GroupByBoxInfoFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(GroupByBoxInfoFormatStyle);
			}
			infoItem = info["RowFormatStyle"];
			if (infoItem == null)
			{
				RowFormatStyle = DeserializeGridTableState(info, "RowFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(RowFormatStyle);
			}
			infoItem = info["HeaderFormatStyle"];
			if (infoItem == null)
			{
				HeaderFormatStyle = DeserializeGridTableState(info, "HeaderFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(HeaderFormatStyle);
			}
			infoItem = info["RowHeaderFormatStyle"];
			if (infoItem == null)
			{
				RowHeaderFormatStyle = DeserializeGridTableState(info, "RowHeaderFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(RowHeaderFormatStyle);
			}
			infoItem = info["GroupRowFormatStyle"];
			if (infoItem == null)
			{
				GroupRowFormatStyle = DeserializeGridTableState(info, "GroupRowFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(GroupRowFormatStyle);
			}
			infoItem = info["TotalRowFormatStyle"];
			if (infoItem == null)
			{
				TotalRowFormatStyle = DeserializeGridTableState(info, "TotalRowformatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(TotalRowFormatStyle);
			}
			infoItem = info["GroupTotalRowFormatStyle"];
			if (infoItem == null)
			{
				GroupTotalRowFormatStyle = DeserializeGridTableState(info, "GroupTotalRowFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(GroupTotalRowFormatStyle);
			}
			infoItem = info["NewRowFormatStyle"];
			if (infoItem == null)
			{
				NewRowFormatStyle = DeserializeGridTableState(info, "NewRowFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(NewRowFormatStyle);
			}
			infoItem = info["PreviewRowFormatStyle"];
			if (infoItem == null)
			{
				PreviewRowFormatStyle = DeserializeGridTableState(info, "PreviewRowFormatStyleIndex");
			}
			else
			{
				infoItem.DeserializeObject(PreviewRowFormatStyle);
			}
			propertyValue = info["FilterCondition"];
			if (propertyValue != null)
			{
				GridFilterCondition gridFilterCondition = new GridFilterCondition();
				gridFilterCondition._owner = this;
				propertyValue.DeserializeObject(gridFilterCondition);
				gridFilterCondition._owner = null;
				FilterCondition = gridFilterCondition;
				return;
			}
			foreach (GridStoredFilter storedFilter in StoredFilters)
			{
				if (storedFilter.GetBaseIsCurrent())
				{
					FilterCondition = storedFilter;
					break;
				}
			}
		}

		private GridFormatStyle DeserializeGridTableState(LayoutReader reader, string propertyName)
		{
			MxDataGrid dataGrid = DataGrid;
			if (dataGrid != null)
			{
				int propertyValue = (int)LayoutReader.GetPropertyValue(reader, propertyName, typeof(int), -1);
				if (propertyValue >= 0 && propertyValue < dataGrid.FormatStyles.Count)
				{
					return dataGrid.FormatStyles[propertyValue];
				}
			}
			return null;
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (AllowAddNew != InheritableBoolean.Default)
			{
				info.AddValue("AllowAddNew", AllowAddNew);
			}
			if (AllowChildTableGroups != InheritableBoolean.Default)
			{
				info.AddValue("AllowChildTableGroups", AllowChildTableGroups);
			}
			if (DataMember != string.Empty)
			{
				info.AddValue("DataMember", DataMember);
			}
			if (DynamicFiltering != InheritableBoolean.Default)
			{
				info.AddValue("DynamicFiltering", DynamicFiltering);
			}
			if (!FilterParentRows)
			{
				info.AddValue("FilterParentRows", FilterParentRows);
			}
			if (GroupTotals != GroupTotals.Default)
			{
				info.AddValue("GroupTotals", GroupTotals);
			}
			if (Key != string.Empty)
			{
				info.AddValue("Key", Key);
			}
			if (NewRowPosition != NewRowPosition.Default)
			{
				info.AddValue("NewRowPosition", NewRowPosition);
			}
			if (Caption != string.Empty)
			{
				info.AddValue("Caption", Caption);
			}
			if (Columns != null && Columns.Count > 0)
			{
				info.AddValue("Columns", Columns);
			}
			if (SortKeys != null && SortKeys.Count > 0)
			{
				info.AddValue("SortKeys", SortKeys);
			}
			if (_allowDelete != InheritableBoolean.Default)
			{
				info.AddValue("AllowDelete", _allowDelete);
			}
			if (_allowEdit != InheritableBoolean.Default)
			{
				info.AddValue("AllowEdit", _allowEdit);
			}
			if (!_allowGroup)
			{
				info.AddValue("AllowGroup", _allowGroup);
			}
			if (_allowRemoveColumns != InheritableBoolean.Default)
			{
				info.AddValue("AllowRemoveColumns", _allowRemoveColumns);
			}
			if (_cellLayoutMode != CellLayoutMode.UseColumns)
			{
				info.AddValue("CellLayoutMode", _cellLayoutMode);
			}
			if (_cellToolTip != CellToolTip.Default)
			{
				info.AddValue("CellToolTip", _cellToolTip);
			}
			if (_cellToolTipContent != null)
			{
				info.AddValue("CellToolTipText", _cellToolTipContent);
			}
			if (ChildTables != null && ChildTables.Count > 0)
			{
				info.AddValue("ChildTables", ChildTables);
			}
			if (_columnHeaders != InheritableBoolean.Default)
			{
				info.AddValue("ColumnHeaders", _columnHeaders);
			}
			if (_columnSets != null && _columnSets.Count > 0)
			{
				info.AddValue("ColumnSets", _columnSets);
			}
			if (_columnSetHeaderLines != 1)
			{
				info.AddValue("ColumnSetHeaderLines", _columnSetHeaderLines);
			}
			if (_columnSetHeaders != InheritableBoolean.Default)
			{
				info.AddValue("ColumnSetHeaders", _columnSetHeaders);
			}
			if (_columnSetRowCount != 2)
			{
				info.AddValue("ColumnSetRowCount", _columnSetRowCount);
			}
			if (_formatConditions != null && _formatConditions.Count > 0)
			{
				info.AddValue("FormatConditions", _formatConditions);
			}
			if (!_lineColor.Equals(Color.Empty))
			{
				info.AddValue("GridLineColor", _lineColor);
			}
			if (_gridLines != GridLines.Default)
			{
				info.AddValue("GridLines", _gridLines);
			}
			if (_gridLineStyle != GridLineStyle.Default)
			{
				info.AddValue("GridLineStyle", _gridLineStyle);
			}
			if (_groupCondition != null)
			{
				info.AddValue("GroupCondition", _groupCondition);
			}
			if (_groupConditionCountTitle != "")
			{
				info.AddValue("GroupConditionCountTitle", _groupConditionCountTitle);
			}
			if (GroupMode != GroupMode.Default)
			{
				info.AddValue("GroupMode", GroupMode);
			}
			if (Groups != null && Groups.Count > 0)
			{
				info.AddValue("Groups", Groups);
			}
			if (GroupHeaderTotals != null && GroupHeaderTotals.Count > 0)
			{
				info.AddValue("GroupHeaderTotals", GroupHeaderTotals);
			}
			if (_headerLines != 1)
			{
				info.AddValue("HeaderLines", _headerLines);
			}
			if (_newRowCaption != "")
			{
				info.AddValue("NewRowInfoText", _newRowCaption);
			}
			if (_previewRow)
			{
				info.AddValue("PreviewRow", _previewRow);
			}
			if (_previewRowLeftMargin != 16)
			{
				info.AddValue("PreviewRowLeftMargin", _previewRowLeftMargin);
			}
			if (_previewRowLines != 3)
			{
				info.AddValue("PreviewRowLines", _previewRowLines);
			}
			if (_previewRowMember != string.Empty)
			{
				info.AddValue("PreviewRowMember", _previewRowMember);
			}
			if (_repeatHeaders != InheritableBoolean.Default)
			{
				info.AddValue("RepeatHeaders", _repeatHeaders);
			}
			if (_rowHeaders != InheritableBoolean.Default)
			{
				info.AddValue("RowHeaders", _rowHeaders);
			}
			if (_rowHeaderContent != RowHeaderContent.Default)
			{
				info.AddValue("RowHeaderContent", _rowHeaderContent);
			}
			if (ShouldSerializeRowHeaderWidth())
			{
				info.AddValue("RowHeaderWidth", _rowHeaderWidth);
			}
			if (_rowHeight != -1)
			{
				info.AddValue("RowHeight", _rowHeight);
			}
			if (_showGroupConditionCount)
			{
				info.AddValue("ShowGroupConditionCount", _showGroupConditionCount);
			}
			if (TotalRow != InheritableBoolean.Default)
			{
				info.AddValue("TotalRow", TotalRow);
			}
			if (_useGroupCondition)
			{
				info.AddValue("UseGroupCondition", _useGroupCondition);
			}
			if (_tableHeader != InheritableBoolean.Default)
			{
				info.AddValue("TableHeader", _tableHeader);
			}
			if (_imageIndex != -1)
			{
				info.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey.Length > 0)
			{
				info.AddValue("ImageKey", _imageKey);
			}
			if (HierarchicalMode != HierarchicalMode.UseChildTables)
			{
				info.AddValue("HierarchicalMode", HierarchicalMode);
			}
			if (_useGroupRowSelector != InheritableBoolean.Default)
			{
				info.AddValue("UseGroupRowSelector", _useGroupRowSelector);
			}
			if (StoredFilters.Count > 0)
			{
				info.AddValue("StoredFilters", StoredFilters);
			}
			if (CustomGroups.Count > 0)
			{
				info.AddValue("CustomGroups", CustomGroups);
			}
			if (FilterCondition is GridFilterCondition)
			{
				info.AddValue("FilterCondition", FilterCondition);
			}
			if (_tableHeaderFormatStyle != null)
			{
				if (_tableHeaderFormatStyle._owner == this)
				{
					if (!_tableHeaderFormatStyle.IsDefaultStyle())
					{
						info.AddValue("TableHeaderFormatStyle", _tableHeaderFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int value = DataGrid.FormatStyles.IndexOf(_tableHeaderFormatStyle);
					if (value != -1)
					{
						info.AddValue("TableHeaderFormatStyleIndex", value);
					}
				}
			}
			if (_alternatingRowFormatStyle != null)
			{
				if (_alternatingRowFormatStyle._owner == this)
				{
					if (!_alternatingRowFormatStyle.IsDefaultStyle())
					{
						info.AddValue("AlternatingRowFormatStyle", _alternatingRowFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int otherIndexOf = DataGrid.FormatStyles.IndexOf(_alternatingRowFormatStyle);
					info.AddValue("AlternatingRowFormatStyleIndex", otherIndexOf);
				}
			}
			if (_groupByBoxInfoFormatStyle != null)
			{
				if (_groupByBoxInfoFormatStyle._owner == this)
				{
					if (!_groupByBoxInfoFormatStyle.IsDefaultStyle())
					{
						info.AddValue("GroupByBoxInfoFormatStyle", _groupByBoxInfoFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int nextIndexOf = DataGrid.FormatStyles.IndexOf(_groupByBoxInfoFormatStyle);
					info.AddValue("GroupByBoxInfoFormatStyleIndex", nextIndexOf);
				}
			}
			if (_rowFormatStyle != null)
			{
				if (_rowFormatStyle._owner == this)
				{
					if (!_rowFormatStyle.IsDefaultStyle())
					{
						info.AddValue("RowFormatStyle", _rowFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int previousIndexOf = DataGrid.FormatStyles.IndexOf(_rowFormatStyle);
					info.AddValue("RowFormatStyleIndex", previousIndexOf);
				}
			}
			if (_headerFormatStyle != null)
			{
				if (_headerFormatStyle._owner == this)
				{
					if (!_headerFormatStyle.IsDefaultStyle())
					{
						info.AddValue("HeaderFormatStyle", _headerFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int firstIndexOf = DataGrid.FormatStyles.IndexOf(_headerFormatStyle);
					info.AddValue("HeaderFormatStyleIndex", firstIndexOf);
				}
			}
			if (_rowHeaderFormatStyle != null)
			{
				if (_rowHeaderFormatStyle._owner == this)
				{
					if (!_rowHeaderFormatStyle.IsDefaultStyle())
					{
						info.AddValue("RowHeaderFormatStyle", _rowHeaderFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int secondIndexOf = DataGrid.FormatStyles.IndexOf(_rowHeaderFormatStyle);
					info.AddValue("RowHeaderFormatStyleIndex", secondIndexOf);
				}
			}
			if (_groupRowFormatStyle != null)
			{
				if (_groupRowFormatStyle._owner == this)
				{
					if (!_groupRowFormatStyle.IsDefaultStyle())
					{
						info.AddValue("GroupRowFormatStyle", _groupRowFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int thirdIndexOf = DataGrid.FormatStyles.IndexOf(_groupRowFormatStyle);
					info.AddValue("GroupRowFormatStyleIndex", thirdIndexOf);
				}
			}
			if (_totalRowFormatStyle != null)
			{
				if (_totalRowFormatStyle._owner == this)
				{
					if (!_totalRowFormatStyle.IsDefaultStyle())
					{
						info.AddValue("TotalRowFormatStyle", _totalRowFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int fourthIndexOf = DataGrid.FormatStyles.IndexOf(_totalRowFormatStyle);
					info.AddValue("TotalRowFormatStyleIndex", fourthIndexOf);
				}
			}
			if (_groupTotalRowFormatStyle != null)
			{
				if (_groupTotalRowFormatStyle._owner == this)
				{
					if (!_groupTotalRowFormatStyle.IsDefaultStyle())
					{
						info.AddValue("GroupTotalRowFormatStyle", _groupTotalRowFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int fifthIndexOf = DataGrid.FormatStyles.IndexOf(_groupTotalRowFormatStyle);
					info.AddValue("GroupTotalRowFormatStyleIndex", fifthIndexOf);
				}
			}
			if (_newRowFormatStyle != null)
			{
				if (_newRowFormatStyle._owner == this)
				{
					if (!_newRowFormatStyle.IsDefaultStyle())
					{
						info.AddValue("NewRowFormatStyle", _newRowFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int sixthIndexOf = DataGrid.FormatStyles.IndexOf(_newRowFormatStyle);
					info.AddValue("NewRowFormatStyleIndex", sixthIndexOf);
				}
			}
			if (_previewRowFormatStyle != null)
			{
				if (_previewRowFormatStyle._owner == this)
				{
					if (!_previewRowFormatStyle.IsDefaultStyle())
					{
						info.AddValue("PreviewRowFormatStyle", _previewRowFormatStyle);
					}
				}
				else if (DataGrid != null)
				{
					int seventhIndexOf = DataGrid.FormatStyles.IndexOf(_previewRowFormatStyle);
					info.AddValue("PreviewRowFormatStyleIndex", seventhIndexOf);
				}
			}
			if (SelfReferencingSettings.HasNonDefaultSettings())
			{
				info.AddValue("SelfReferencingSettings", SelfReferencingSettings);
			}
			if (_tag is string)
			{
				info.AddValue("Tag", _tag);
			}
		}

		protected override MordexSelfReferencingSettingsBase CreateSelfReferencingSettings()
		{
			return new GridSelfReferencingSettings(this);
		}

		public bool ShouldSerializeRowHeaderWidth()
		{
			return _rowHeaderWidth != -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeGridLineColor()
		{
			return !_lineColor.Equals(Color.Empty);
		}

		protected override CollectionChangingAction OnSortSettingsChanging(CollectionChangeType change, MordexSortFieldBase sortField)
		{
			if (DataGrid != null && !DataGrid.UpdateData())
			{
				return CollectionChangingAction.CancelNoError;
			}
			return CollectionChangingAction.Continue;
		}
	}
}
