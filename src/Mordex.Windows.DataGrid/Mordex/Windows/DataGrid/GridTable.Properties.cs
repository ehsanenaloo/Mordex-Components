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
		string IMordexXmlLayoutsSupport.Identifier
		{
			get
			{
				if (!IsRootTable())
				{
					return Key;
				}
				return null;
			}
		}

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("The text displayed in the table header.")]
		[DefaultValue("")]
		public string Caption
		{
			get
			{
				return base.BaseTableCaption;
			}
			set
			{
				if (base.BaseTableCaption != value)
				{
					base.BaseTableCaption = value;
					RefreshDataGrid();
				}
			}
		}

		[DefaultValue("")]
		[Description("The name of a list in the data source for which the table displays its data.")]
		[Editor("Mordex.Windows.DataGrid.Design.DataMemberEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public string DataMember
		{
			get
			{
				return base.BaseDataMember;
			}
			set
			{
				base.BaseDataMember = value;
			}
		}

		[Description("Determines whether FilterCondition is evaluated in rows only when it is set or every time a row is updated.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean DynamicFiltering
		{
			get
			{
				return DataState.ToGridInheritableBoolean(base.BaseDynamicFiltering);
			}
			set
			{
				base.BaseDynamicFiltering = DataState.ToDataInheritableBoolean(value);
			}
		}

		[Browsable(false)]
		public GridTable ParentTable => (GridTable)base.BaseParentTable;

		[Localizable(true)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridColumnCollection Columns => (GridColumnCollection)base.BaseFields;

		[Browsable(false)]
		public GridCustomGroupCollection CustomGroups => (GridCustomGroupCollection)base.BaseCustomGroups;

		[Browsable(false)]
		public MxDataGrid DataGrid
		{
			get
			{
				GridTable parentTable = ParentTable;
				if (parentTable != null)
				{
					return parentTable.DataGrid;
				}
				if (_dataGrid == null && ((IMordexTable)this).DataSourceManager is GridDataSourceManager gridDataSourceManager)
				{
					return gridDataSourceManager.GetOwner();
				}
				return _dataGrid;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Localizable(true)]
		[Browsable(false)]
		public GridChildTableCollection ChildTables => (GridChildTableCollection)base.BaseChildTables;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridSortKeyCollection SortKeys => (GridSortKeyCollection)base.BaseSortFields;

		[Browsable(false)]
		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridGroupCollection Groups => (GridGroupCollection)base.BaseGroups;

		[Localizable(true)]
		[Browsable(false)]
		public GridColumnSetCollection ColumnSets => _columnSets;

		[DefaultValue(CellLayoutMode.UseColumns)]
		[Description("Indicates how coluwns will be rendered.")]
		public CellLayoutMode CellLayoutMode
		{
			get
			{
				return _cellLayoutMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CellLayoutMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _cellLayoutMode)
				{
					_cellLayoutMode = value;
					if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
					{
						_columnSets.RecalculateColumnPositions();
						ColumnSets.DisposeResources();
					}
					_isAutoResizePending = true;
					RefreshDataGrid();
				}
			}
		}

		[Description("Indicates whether column set headers will be displayed.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean ColumnSetHeaders
		{
			get
			{
				return _columnSetHeaders;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _columnSetHeaders)
				{
					_columnSetHeaders = value;
					if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
					{
						RefreshDataGrid();
					}
				}
			}
		}

		[DefaultValue(2)]
		[Description("Specifies how many row levels will the table have in a column set layout.")]
		public int ColumnSetRowCount
		{
			get
			{
				return _columnSetRowCount;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value == _columnSetRowCount)
				{
					return;
				}
				_columnSetRowCount = value;
				foreach (GridColumnSet item in _columnSets)
				{
					item.RebuildForRowCountChange();
				}
				if (_cellLayoutMode == CellLayoutMode.UseColumnSets)
				{
					RefreshDataGrid();
				}
			}
		}

		[DefaultValue(16)]
		[Description("Indicates the distance to indent the text in the preview row.")]
		public int PreviewRowLeftMargin
		{
			get
			{
				return _previewRowLeftMargin;
			}
			set
			{
				if (_previewRowLeftMargin != value)
				{
					_previewRowLeftMargin = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Indicates whether preview rows should be displayed.")]
		[DefaultValue(false)]
		public bool PreviewRow
		{
			get
			{
				return _previewRow;
			}
			set
			{
				if (value != _previewRow)
				{
					_previewRow = value;
					RefreshDataGrid();
					NotifyPreviewRowChanged();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.DataMemberEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Specifies the property of the data source whose contents you want to display in the preview row.")]
		[DefaultValue("")]
		public string PreviewRowMember
		{
			get
			{
				return _previewRowMember;
			}
			set
			{
				if (value != _previewRowMember)
				{
					_previewRowMember = value;
					if (_previewRow)
					{
						RefreshDataGrid();
					}
					NotifyPreviewRowChanged();
				}
			}
		}

		[DefaultValue(3)]
		[Description("Specifies the number of lines to be displayed in preview rows.")]
		public int PreviewRowLines
		{
			get
			{
				return _previewRowLines;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _previewRowLines)
				{
					_previewRowLines = value;
					if (_previewRow)
					{
						RefreshDataGrid();
					}
				}
			}
		}

		[Description("Indicates how many lines tall will be the column headers.")]
		[DefaultValue(1)]
		public int HeaderLines
		{
			get
			{
				return _headerLines;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _headerLines)
				{
					_headerLines = value;
					RefreshDataGrid();
				}
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Indicates whether coluwn headers should be displayed.")]
		public InheritableBoolean ColumnHeaders
		{
			get
			{
				return _columnHeaders;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _columnHeaders)
				{
					_columnHeaders = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Specifies how many lines tall will be the column set headers.")]
		[DefaultValue(1)]
		public int ColumnSetHeaderLines
		{
			get
			{
				return _columnSetHeaderLines;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _columnSetHeaderLines)
				{
					_columnSetHeaderLines = value;
					if (CellLayoutMode == CellLayoutMode.UseColumnSets)
					{
						RefreshDataGrid();
					}
				}
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Indicates whether row headers should be shown in rows that belong to the table.")]
		public InheritableBoolean RowHeaders
		{
			get
			{
				return _rowHeaders;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _rowHeaders)
				{
					_rowHeaders = value;
					if (DataGrid != null && DataGrid.ColumnAutoResize)
					{
						_isAutoResizePending = true;
					}
					RefreshDataGrid();
				}
			}
		}

		[Description("Specifies the width of the row headers that belongs to the table.")]
		public int RowHeaderWidth
		{
			get
			{
				if (_rowHeaderWidth == -1)
				{
					if (ComputeRowHeaderContent() == RowHeaderContent.RowHeaderText)
					{
						return 18;
					}
					return 50;
				}
				return _rowHeaderWidth;
			}
			set
			{
				if (value < -1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _rowHeaderWidth)
				{
					_rowHeaderWidth = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Indicates the color used to draw grid lines in the cells that belongs to the table.")]
		public Color GridLineColor
		{
			get
			{
				return _lineColor;
			}
			set
			{
				if (value != _lineColor)
				{
					_lineColor = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Indicates how lines between cells will be drawn.")]
		[DefaultValue(GridLines.Default)]
		public GridLines GridLines
		{
			get
			{
				return _gridLines;
			}
			set
			{
				if (value != _gridLines)
				{
					_gridLines = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Specifies the style of the grid lines.")]
		[DefaultValue(GridLineStyle.Default)]
		public GridLineStyle GridLineStyle
		{
			get
			{
				return _gridLineStyle;
			}
			set
			{
				if (value != _gridLineStyle)
				{
					_gridLineStyle = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("The key used to identify an object in a collection.")]
		public string Key
		{
			get
			{
				return base.BaseKey;
			}
			set
			{
				base.BaseKey = value;
			}
		}

		[Description("Indicates whether the user can add rows.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowAddNew
		{
			get
			{
				return DataState.ToGridInheritableBoolean(base.BaseAllowAddNew);
			}
			set
			{
				base.BaseAllowAddNew = DataState.ToDataInheritableBoolean(value);
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Determines whether the user can group rows in the table using columns from its child tables.")]
		public InheritableBoolean AllowChildTableGroups
		{
			get
			{
				return DataState.ToGridInheritableBoolean(base.BaseAllowChildTableGroups);
			}
			set
			{
				base.BaseAllowChildTableGroups = DataState.ToDataInheritableBoolean(value);
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Indicates whether the user can edit rows.")]
		public InheritableBoolean AllowEdit
		{
			get
			{
				return _allowEdit;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_allowEdit == value)
				{
					return;
				}
				if (DataGrid != null)
				{
					bool condition = false;
					switch (value)
					{
					case InheritableBoolean.False:
						condition = true;
						break;
					case InheritableBoolean.Default:
						if (DataGrid.ShouldAllowEdit())
						{
							condition = true;
						}
						break;
					}
					if (condition && !DataGrid.UpdateData())
					{
						throw new InvalidOperationException();
					}
				}
				_allowEdit = value;
			}
		}

		[Description("Indicates whether the user can delete rows.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowDelete
		{
			get
			{
				return _allowDelete;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_allowDelete = value;
			}
		}

		[DefaultValue(NewRowPosition.Default)]
		[Description("Indicates whether the new row should appear as the last row or in top rows.")]
		public NewRowPosition NewRowPosition
		{
			get
			{
				return DataState.ComputeNewRowPosition(base.BaseNewRowPosition);
			}
			set
			{
				base.BaseNewRowPosition = DataState.ToDataNewRowPosition(value);
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Indicates whether column headers should be drawn again when the previous record is from another table.")]
		public InheritableBoolean RepeatHeaders
		{
			get
			{
				return _repeatHeaders;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _repeatHeaders)
				{
					_repeatHeaders = value;
					RefreshDataGrid();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public GridFormatConditionCollection FormatConditions => _formatConditions;

		[Description("Determines when group total rows should be shown.")]
		[DefaultValue(GroupTotals.Default)]
		public GroupTotals GroupTotals
		{
			get
			{
				return DataState.ComputeGroupTotals(base.BaseGroupTotalRows);
			}
			set
			{
				base.BaseGroupTotalRows = DataState.ToDataGroupTotals(value);
			}
		}

		[Browsable(false)]
		public int HierarchyLevel
		{
			get
			{
				if (IsRootTable() || ParentTable == null)
				{
					return 0;
				}
				return ParentTable.HierarchyLevel + 1;
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Indicates whether total rows should be displayed for the table.")]
		public InheritableBoolean TotalRow
		{
			get
			{
				return DataState.ToGridInheritableBoolean(base.BaseTotalRow);
			}
			set
			{
				base.BaseTotalRow = DataState.ToDataInheritableBoolean(value);
			}
		}

		[DefaultValue(-1)]
		[Description("Indicates the height of the rows that belong to the table.")]
		public int RowHeight
		{
			get
			{
				return _rowHeight;
			}
			set
			{
				if (value <= 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _rowHeight)
				{
					_rowHeight = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Represents the format condition that is applied in the group rows that belongs to the table.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridFormatCondition GroupCondition => _groupCondition;

		[DefaultValue(false)]
		[Description("Indicates whether the group condition should be used or not to format group rows.")]
		public bool UseGroupCondition
		{
			get
			{
				if (_useGroupCondition)
				{
					return _groupCondition.Enabled;
				}
				return false;
			}
			set
			{
				if (value != _useGroupCondition)
				{
					_useGroupCondition = value;
					RefreshDataGrid();
				}
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the count of rows that meet the criteria in the GroupCondition settings, will be displayed in group rows.")]
		public bool ShowGroupConditionCount
		{
			get
			{
				return _showGroupConditionCount;
			}
			set
			{
				if (value != _showGroupConditionCount)
				{
					_showGroupConditionCount = value;
					RefreshDataGrid();
				}
			}
		}

		[DefaultValue("")]
		[Localizable(true)]
		[Description("The text displayed in a group row when one or more rows in the group meet the criteria specified in the GroupCondition object's properties.")]
		public string GroupConditionCountTitle
		{
			get
			{
				return _groupConditionCountTitle;
			}
			set
			{
				if (value != _groupConditionCountTitle)
				{
					_groupConditionCountTitle = value;
					RefreshDataGrid();
				}
			}
		}

		[DefaultValue(null)]
		[Editor("Mordex.Windows.DataGrid.Design.TableFilterConditionEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.TableFilterConverter, Mordex.Windows.DataGrid.Design")]
		[Description("Represents the GridFilterCondition or GridStoredFilter applied to the rows that belongs to the table.")]
		public IFilterCondition FilterCondition
		{
			get
			{
				return (IFilterCondition)base.BaseFilterCondition;
			}
			set
			{
				Mordex.Data.IFilterCondition filterCondition = (Mordex.Data.IFilterCondition)value;
				if (filterCondition != base.BaseFilterCondition)
				{
					if (DataGrid != null)
					{
						DataGrid.FirstRow = 0;
					}
					base.BaseFilterCondition = filterCondition;
				}
			}
		}

		[Browsable(false)]
		public GridFilterCondition FilterApplied
		{
			get
			{
				if (FilterCondition != null)
				{
					return FilterCondition.FilterCondition;
				}
				return null;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether parent rows should be filtered when applying a filter condition to the table.")]
		public bool FilterParentRows
		{
			get
			{
				return base.BaseFilterParentRows;
			}
			set
			{
				base.BaseFilterParentRows = value;
			}
		}

		[DefaultValue(InheritableBoolean.Default)]
		[Description("Indicates whether columns that belongs to the table should be hidden when groupping by them.")]
		public InheritableBoolean HideColumnsWhenGrouped
		{
			get
			{
				return _hideColumnsWhenGrouped;
			}
			set
			{
				if (_hideColumnsWhenGrouped != value)
				{
					_hideColumnsWhenGrouped = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Indicates whether columns should be hidden when the user drops them out of the control.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowRemoveColumns
		{
			get
			{
				return _allowRemoveColumns;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_allowRemoveColumns = value;
			}
		}

		[DefaultValue(GroupMode.Default)]
		[Description("Indicates whether groups should be collapsed or expanded when groups are calculated.")]
		public GroupMode GroupMode
		{
			get
			{
				return DataState.ToGridGroupMode(base.BaseGroupMode);
			}
			set
			{
				base.BaseGroupMode = DataState.ComputeGroupMode(value);
			}
		}

		[Description("Indicates whether groups can be expanded and collapsed.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean ExpandableGroups
		{
			get
			{
				return _expandableGroups;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_expandableGroups != value)
				{
					_expandableGroups = value;
					RefreshDataGrid();
				}
			}
		}

		[DefaultValue("")]
		[Description("The text displayed in an empty new row.")]
		[Localizable(true)]
		public string NewRowInfoText
		{
			get
			{
				return _newRowCaption;
			}
			set
			{
				if (_newRowCaption != value)
				{
					_newRowCaption = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Indicates whether a table header should be displayed.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean TableHeader
		{
			get
			{
				return _tableHeader;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _tableHeader)
				{
					_tableHeader = value;
					RefreshDataGrid();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("The image list index value of the image displayed on the table header.")]
		[DefaultValue(-1)]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (value != _imageIndex)
				{
					if (_imageIndex != -1)
					{
						_imageKey = "";
					}
					_imageIndex = value;
					RefreshDataGrid();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue("")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[Description("The key of the image displayed on the table header.")]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageKey != value)
				{
					if (value.Length > 0)
					{
						_imageIndex = -1;
					}
					_imageKey = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Indicates whether the user can group records in the table.")]
		[DefaultValue(true)]
		public bool AllowGroup
		{
			get
			{
				return _allowGroup;
			}
			set
			{
				_allowGroup = value;
			}
		}

		[DefaultValue(null)]
		[Description("User defined data to associate with the column.")]
		[TypeConverter("System.ComponentModel.StringConverter")]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		[Browsable(false)]
		public GridGroupHeaderTotalCollection GroupHeaderTotals => (GridGroupHeaderTotalCollection)base.BaseGroupTotals;

		[Browsable(false)]
		public GridStoredFilterCollection StoredFilters => (GridStoredFilterCollection)base.BaseStoredFilters;

		[Description("The text to display in tool tips for cells that belong to the table.")]
		[DefaultValue(null)]
		public string CellToolTipText
		{
			get
			{
				return _cellToolTipContent;
			}
			set
			{
				_cellToolTipContent = value;
			}
		}

		[Description("Indicates which tool tip to use in cells that belong to this table.")]
		[DefaultValue(CellToolTip.Default)]
		public CellToolTip CellToolTip
		{
			get
			{
				return _cellToolTip;
			}
			set
			{
				_cellToolTip = value;
			}
		}

		[Browsable(false)]
		public GridSelfReferencingSettings SelfReferencingSettings => (GridSelfReferencingSettings)base.BaseSelfReferencingSettings;

		[Description("Determines the kind of hierarchy to use in child rows of the table.")]
		[DefaultValue(HierarchicalMode.UseChildTables)]
		public HierarchicalMode HierarchicalMode
		{
			get
			{
				return DataState.ComputeHierarchicalMode(base.BaseHierarchicalMode);
			}
			set
			{
				base.BaseHierarchicalMode = DataState.ToDataHierarchicalMode(value);
			}
		}

		[DefaultValue(RowHeaderContent.Default)]
		[Description("Determines what is rendered in row headers.")]
		public RowHeaderContent RowHeaderContent
		{
			get
			{
				return _rowHeaderContent;
			}
			set
			{
				if (!Enum.IsDefined(typeof(RowHeaderContent), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_rowHeaderContent != value)
				{
					_rowHeaderContent = value;
					RefreshDataGrid();
				}
			}
		}

		[Description("Determines whether check box selectors are shown in group rows too.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean UseGroupRowSelector
		{
			get
			{
				return _useGroupRowSelector;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_useGroupRowSelector != value)
				{
					_useGroupRowSelector = value;
					if (DataGrid != null)
					{
						DataGrid.RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Description("Format settings used to draw the caption of the table in the 'Group By Box'.")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Format Styles")]
		public GridFormatStyle GroupByBoxInfoFormatStyle
		{
			get
			{
				if (_groupByBoxInfoFormatStyle == null)
				{
					_groupByBoxInfoFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _groupByBoxInfoFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _groupByBoxInfoFormatStyle)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || DataGrid != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_groupByBoxInfoFormatStyle != null && _groupByBoxInfoFormatStyle._owner == this)
				{
					_groupByBoxInfoFormatStyle._owner = null;
				}
				_groupByBoxInfoFormatStyle = value;
				RefreshDataGrid();
			}
		}

		[Category("Format Styles")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Format settings used to draw cells in rows that belong to the table. ")]
		public GridFormatStyle RowFormatStyle
		{
			get
			{
				if (_rowFormatStyle == null)
				{
					_rowFormatStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _rowFormatStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _rowFormatStyle)
				{
					return;
				}
				if (value.IsInCollection())
				{
					if (DataGrid == null || DataGrid != value.DataGrid)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (value._owner != null)
					{
						throw new InvalidOperationException();
					}
					value._owner = this;
				}
				if (_rowFormatStyle != null && _rowFormatStyle._owner == this)
				{
					_rowFormatStyle._owner = null;
				}
				_rowFormatStyle = value;
				RefreshDataGrid();
			}
		}
	}
}
