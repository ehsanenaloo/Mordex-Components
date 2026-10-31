using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Accessibility;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class MxDataGrid
	{
		[DefaultValue(4)]
		[Category("Table View")]
		[Description("The distance, in pixels, between tables in hierarchical mode.")]
		public int TableSpacing
		{
			get
			{
				return _tableSpacing;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != TableSpacing)
				{
					_tableSpacing = value;
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(FilterMode.None)]
		[Description("Determines if a filter row should be displayed and whether it filters row automatically.")]
		public FilterMode FilterMode
		{
			get
			{
				return _filterMode;
			}
			set
			{
				if (value != _filterMode)
				{
					if (_row != null && _row.RowType == RowType.FilterRow)
					{
						AcceptCellEdit();
					}
					_filterMode = value;
					if (_filterMode == FilterMode.None)
					{
						RemoveFilters();
					}
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Category("Table View")]
		[DefaultValue(false)]
		[Description("Indicates whether columns are automatically sized to fit on the client width.")]
		public bool ColumnAutoResize
		{
			get
			{
				return _columnAutoResize;
			}
			set
			{
				if (_columnAutoResize != value)
				{
					_columnAutoResize = value;
					MarkTablesForAutoResize();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[Bindable(BindableSupport.No)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[DefaultValue(InheritableBoolean.False)]
		[Category("Behavior")]
		[Description("Indicates whether columns are hidden when grouping by them.")]
		public InheritableBoolean HideColumnsWhenGrouped
		{
			get
			{
				return _hideColumnsWhenGrouped;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == InheritableBoolean.Default)
				{
					value = InheritableBoolean.False;
				}
				_hideColumnsWhenGrouped = value;
				RefreshRowCountAndInvalidate();
			}
		}

		[Description("Indicates whether columns are hidden when the user drops them outside the control.")]
		[DefaultValue(InheritableBoolean.False)]
		[Category("Behavior")]
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
				if (value == InheritableBoolean.Default)
				{
					value = InheritableBoolean.False;
				}
				_allowRemoveColumns = value;
			}
		}

		[Category("Edit")]
		[DefaultValue(InheritableBoolean.False)]
		[Description("Indicates whether the user can add rows.")]
		public InheritableBoolean AllowAddNew
		{
			get
			{
				return DataState.ComputeInheritableBoolean(_gridDataSourceManager.AllowAddNew);
			}
			set
			{
				_gridDataSourceManager.AllowAddNew = DataState.ResolveInheritableBoolean(value, false);
			}
		}

		[DefaultValue(InheritableBoolean.False)]
		[Description("Determines whether row headers are shown.")]
		[Category("Table View")]
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
				if (_rowHeaders == value)
				{
					return;
				}
				_rowHeaders = value;
				if (_columnAutoResize && _gridTableCollection != null)
				{
					foreach (GridTable item in _gridTableCollection)
					{
						item._isAutoResizePending = true;
					}
				}
				RefreshRowCountAndInvalidate();
			}
		}

		[Category("Edit")]
		[DefaultValue(InheritableBoolean.False)]
		[Description("Indicates whether the user can delete rows.")]
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
				if (value == InheritableBoolean.Default)
				{
					value = InheritableBoolean.False;
				}
				if (_allowDelete != value)
				{
					_allowDelete = value;
				}
			}
		}

		[Category("Edit")]
		[Description("Indicates whether the user can edit rows.")]
		[DefaultValue(InheritableBoolean.True)]
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
				if (value == InheritableBoolean.Default)
				{
					value = InheritableBoolean.True;
				}
				if (_allowEdit != value)
				{
					if (value == InheritableBoolean.False && !UpdateData())
					{
						throw new InvalidOperationException();
					}
					_allowEdit = value;
					Invalidate();
				}
			}
		}

		[Description("Determines whether the user can group rows in a table using a column from one of its child tables.")]
		[DefaultValue(false)]
		public bool AllowChildTableGroups
		{
			get
			{
				return GetRowSourceManager().AllowChildTableGroups;
			}
			set
			{
				GetRowSourceManager().AllowChildTableGroups = value;
			}
		}

		[Category("Table View")]
		[DefaultValue(InheritableBoolean.True)]
		[Description("Indicates whether column headers are visible or hidden.")]
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
				if (value == InheritableBoolean.Default)
				{
					value = InheritableBoolean.True;
				}
				if (_columnHeaders != value)
				{
					_columnHeaders = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Indicates whether column set headers are visible or hiddden.")]
		[DefaultValue(InheritableBoolean.True)]
		[Category("Table View")]
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
				if (value == InheritableBoolean.Default)
				{
					value = InheritableBoolean.True;
				}
				if (_columnSetHeaders != value)
				{
					_columnSetHeaders = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Determines the navigation mode in tables using column sets.")]
		[DefaultValue(ColumnSetNavigation.Row)]
		[Category("Table View")]
		public ColumnSetNavigation ColumnSetNavigation
		{
			get
			{
				return _columnSetNavigation;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ColumnSetNavigation), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == _columnSetNavigation)
				{
					return;
				}
				_columnSetNavigation = value;
				foreach (GridTable item in _gridTableCollection)
				{
					item.ColumnSets.RecalculateColumnPositions();
				}
			}
		}

		[Category("Appearance")]
		[Description("The color used to draw grid lines.")]
		public Color GridLineColor
		{
			get
			{
				return _gridLineColor;
			}
			set
			{
				if (value.A != byte.MaxValue)
				{
					throw new ArgumentException();
				}
				if (!_gridLineColor.Equals(value))
				{
					_gridLineColor = value;
					Invalidate();
				}
			}
		}

		[Description("Indicates how lines lines between cells are drawn.")]
		[DefaultValue(GridLines.Both)]
		[Category("Appearance")]
		public GridLines GridLines
		{
			get
			{
				return _gridLines;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GridLines), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_gridLines != value)
				{
					_gridLines = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Determines how grid lines are drawn.")]
		[DefaultValue(GridLineStyle.SmallDots)]
		[Category("Appearance")]
		public GridLineStyle GridLineStyle
		{
			get
			{
				return _gridLineStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GridLineStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == GridLineStyle.Default)
				{
					value = GridLineStyle.SmallDots;
				}
				if (_gridLineStyle != value)
				{
					_gridLineStyle = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(GroupTotals.Never)]
		[Description("Determines when group total rows should be shown.")]
		[Category("Table View")]
		public GroupTotals GroupTotals
		{
			get
			{
				return DataState.ComputeGroupTotals(_gridDataSourceManager.GroupTotalRows);
			}
			set
			{
				_gridDataSourceManager.GroupTotalRows = DataState.ToDataGroupTotals(value);
			}
		}

		[Description("Indicates whether the new row should appear as the last row or in top of rows.")]
		[DefaultValue(NewRowPosition.TopRow)]
		[Category("Appearance")]
		public NewRowPosition NewRowPosition
		{
			get
			{
				return DataState.ComputeNewRowPosition(_gridDataSourceManager.NewRowPosition);
			}
			set
			{
				_gridDataSourceManager.NewRowPosition = DataState.ToDataNewRowPosition(value);
			}
		}

		[Description("Indicates whether column headers should be drawn again in a table when the previous record is from another table.")]
		[Category("Table View")]
		[DefaultValue(InheritableBoolean.True)]
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
				if (value == InheritableBoolean.Default)
				{
					value = InheritableBoolean.True;
				}
				if (_repeatHeaders != value)
				{
					_repeatHeaders = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Indicates whether a total row should be displayed at the end of the list.")]
		[DefaultValue(InheritableBoolean.False)]
		[Category("Table View")]
		public InheritableBoolean TotalRow
		{
			get
			{
				return DataState.ComputeInheritableBoolean(_gridDataSourceManager.TableTotalRows);
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_gridDataSourceManager.TableTotalRows = DataState.ResolveInheritableBoolean(value, false);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DataChanged
		{
			get
			{
				if (_row != null && _row.RowType != RowType.FilterRow)
				{
					return _row.HasChangedCells();
				}
				return false;
			}
			set
			{
				if (_row == null || value == _row.HasChangedCells())
				{
					return;
				}
				if (_gridColumnContainerControl != null)
				{
					_gridColumnContainerControl.SetModified(false);
				}
				if (!value && !EndCellEdit(value))
				{
					return;
				}
				_row.SetDataChanged(value);
				if (!value)
				{
					bool autoEdit = _autoEdit;
					_autoEdit = false;
					if (_row.RowType == RowType.NewRecord)
					{
						int row = Row;
						_isDataChanging = true;
						_row.SetDataChanged(false);
						_row.GetLogicalRow().CancelEdit();
						SetCurrentRowAndSelect(row);
						_isDataChanging = false;
					}
					_autoEdit = autoEdit;
				}
				InvalidateGridRowLayout(_row);
				InvalidateTotalAndGroupRows(_row);
			}
		}

		[Browsable(false)]
		[Obsolete("'CalendarNoneText' will be removed in future versions. Use DataGrid.BuiltInTexts(GridBuiltInText.CalendarNoneButton) instead.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CalendarNoneText
		{
			get
			{
				return BuiltInTexts[GridBuiltInText.CalendarNoneButton];
			}
			set
			{
				BuiltInTexts[GridBuiltInText.CalendarNoneButton] = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("'CalendarTodayText' will be removed in future versions. Use DataGrid.BuiltInTexts(GridBuiltInText.CalendarTodayButton) instead.")]
		[Browsable(false)]
		public string CalendarTodayText
		{
			get
			{
				return BuiltInTexts[GridBuiltInText.CalendarTodayButton];
			}
			set
			{
				BuiltInTexts[GridBuiltInText.CalendarTodayButton] = value;
			}
		}

		[Description("Determines whether groups are collapsed or expanded by default.")]
		[Category("Table View")]
		[DefaultValue(GroupMode.Expanded)]
		public GroupMode GroupMode
		{
			get
			{
				return DataState.ToGridGroupMode(_gridDataSourceManager.GroupMode);
			}
			set
			{
				_gridDataSourceManager.GroupMode = DataState.ComputeGroupMode(value);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int RecordCount => _gridDataSourceManager.GetRootRecordCount();

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public int RowCount
		{
			get
			{
				if (GetRowManager() == null)
				{
					return 0;
				}
				if (GetRowManager().IsIndexSuspend)
				{
					return _rowCount;
				}
				return GetRowManager().Count;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("'RecordNavigatorText' will be removed in future versions. Use DataGrid.BuiltInTexts(GridBuiltInText.RecordNavigator) instead.")]
		[Browsable(false)]
		public string RecordNavigatorText
		{
			get
			{
				return GetNavigatorCaptionAndText();
			}
			set
			{
				ApplyNavigatorCaptionAndText(value);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public EditMode EditMode
		{
			get
			{
				if (_gridColumnContainerControl != null || _previousColumnContainerControl != null)
				{
					return EditMode.EditOn;
				}
				return EditMode.EditOff;
			}
			set
			{
				if (!Enum.IsDefined(typeof(EditMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (EditMode != value)
				{
					if (value == EditMode.EditOn)
					{
						BeginCellEdit(false, '\0', false);
					}
					else
					{
						AcceptCellEdit();
					}
				}
			}
		}

		[DefaultValue(true)]
		[Description("Determines whether the control should scroll its contents while the user drags the scroll thumb along the vertical scroll bar.")]
		[Category("Behavior")]
		public bool ContinuousScroll
		{
			get
			{
				return _continuousScroll;
			}
			set
			{
				_continuousScroll = value;
			}
		}

		[DefaultValue(false)]
		[Category("Appearance")]
		[Description("Indicates whether the control displays empty rows below the last row.")]
		public bool EmptyRows
		{
			get
			{
				return _emptyRows;
			}
			set
			{
				if (_emptyRows != value)
				{
					_emptyRows = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Indicates whether the user can drag columns.")]
		public bool AllowColumnDrag
		{
			get
			{
				return _allowColumnDrag;
			}
			set
			{
				_allowColumnDrag = value;
			}
		}

		[Description("Indicates whether groups can be expanded and collapsed.")]
		[DefaultValue(InheritableBoolean.True)]
		[Category("Table View")]
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
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("The default percentage used in format styles with Blend empty. Valid values are from 0 to 1")]
		[Category("Predefined Styles")]
		[DefaultValue(0f)]
		public float DefaultBlend
		{
			get
			{
				return _defaultBlend;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (_defaultBlend != value)
				{
					_defaultBlend = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[Description("The color that blends with the BackColor of format styles with Blend greater than 0")]
		public Color BlendColor
		{
			get
			{
				return _blendColor;
			}
			set
			{
				if (value.IsEmpty)
				{
					throw new ArgumentException();
				}
				if (!_blendColor.Equals(value))
				{
					_blendColor = value;
					Invalidate();
				}
			}
		}

		[Description("Determines the style of the focus rectangle around the active row.")]
		[Category("Appearance")]
		[DefaultValue(FocusStyle.Dotted)]
		public FocusStyle FocusStyle
		{
			get
			{
				return _focusStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FocusStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _focusStyle)
				{
					_focusStyle = value;
					Invalidate();
				}
			}
		}

		[Description("Determines whether borders should be drawn in flat headers.")]
		[DefaultValue(true)]
		[Category("Appearance")]
		public bool FlatBorderInHeaders
		{
			get
			{
				return _flatBorderInHeaders;
			}
			set
			{
				if (value != _flatBorderInHeaders)
				{
					_flatBorderInHeaders = value;
					Invalidate();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridDropDownCollection DropDowns => _dropDowns;

		[DefaultValue(false)]
		[Description("Indicates whether the EditorsControlStyle should be used to draw buttons and scroll bars in cell editors.")]
		[Category("Appearance")]
		public bool HasEditorsControlStyle
		{
			get
			{
				return _hasEditorsControlStyle;
			}
			set
			{
				_hasEditorsControlStyle = value;
			}
		}

		[Category("Appearance")]
		[Description("The ControlStyle object used to draw buttons and drop down in cell editors.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlStyle EditorsControlStyle => _editorsControlStyle;

		[DefaultValue(TabKeyBehavior.ColumnNavigation)]
		[Category("Behavior")]
		[Description("Determines how DataGrid behaves when Tab key is pressed.")]
		public TabKeyBehavior TabKeyBehavior
		{
			get
			{
				return _tabKeyBehavior;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TabKeyBehavior), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_tabKeyBehavior = value;
			}
		}

		[Description("Determines how DataGrid acts when the enter key is pressed.")]
		[Category("Behavior")]
		[DefaultValue(EnterKeyBehavior.NextRow)]
		public EnterKeyBehavior EnterKeyBehavior
		{
			get
			{
				return _enterKeyBehavior;
			}
			set
			{
				_enterKeyBehavior = value;
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether escape key is handled by the control to cancel edition.")]
		[DefaultValue(true)]
		public bool AcceptsEscape
		{
			get
			{
				return _acceptsEscape;
			}
			set
			{
				_acceptsEscape = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DefaultValue(ImageSource.ExternalImageList)]
		[Category("Behavior")]
		[Description("Indicates whether an external or the internal image list should be used.")]
		[Obsolete("ImageListSource is no longer supported. ExternalImageList and InternalImageList are deprecated. use ImageList property instead.")]
		[Browsable(false)]
		public ImageSource ImageSource
		{
			get
			{
				return _imageSource;
			}
			set
			{
				_imageSource = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[Description("Represents the ImageList used when ImageSource is equal to InternalImageList.")]
		[Obsolete("InternalImageList is no longer supported. Use ImageList property instead.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ImageList InternalImageList => GetOrCreateImageList();

		[Description("Indicates whether cards should be centered in single card view.")]
		[DefaultValue(true)]
		[Category("Card View")]
		public bool CenterSingleCard
		{
			get
			{
				return _centerSingleCard;
			}
			set
			{
				if (value != _centerSingleCard)
				{
					_centerSingleCard = value;
					if (_view == View.SingleCard)
					{
						RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DefaultValue("")]
		[Obsolete("'FilterRowInfoText' will be removed in future versions. Use DataGrid.BuiltInTexts(GridBuiltInText.FilterRowInfo) instead.")]
		[Localizable(true)]
		[Description("The text displayed in an empty filter row.")]
		[Category("Localization")]
		[Browsable(false)]
		public string FilterRowInfoText
		{
			get
			{
				return GetFilterRowInfoText();
			}
			set
			{
				SetFilterRowInfoText(value);
			}
		}

		[DefaultValue(InheritableBoolean.False)]
		[Description("Indicates whether table headers should be displayed.")]
		[Category("Table View")]
		public InheritableBoolean TableHeaders
		{
			get
			{
				return _tableHeaders;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_tableHeaders != value)
				{
					_tableHeaders = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.FlagsEnumConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(GridOwnerDrawnArea.None)]
		[Editor("Mordex.Windows.DataGrid.Design.FlagsEnumEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Appearance")]
		[Description("Determines which areas of the control should be drawn by the developer.")]
		public GridOwnerDrawnArea OwnerDrawnAreas
		{
			get
			{
				return _ownerDrawnAreas;
			}
			set
			{
				if (_ownerDrawnAreas != value)
				{
					_ownerDrawnAreas = value;
					RefreshRowCountAndInvalidate();
				}
			}
		}

		[Description("Determines the default alpha level used in format styles with Alpha property empty.")]
		[Category("Predefined Styles")]
		[DefaultValue(-1)]
		public int DefaultAlphaLevel
		{
			get
			{
				return _defaultAlphaLevel;
			}
			set
			{
				if (value < -1 || value > 255)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _defaultAlphaLevel)
				{
					_defaultAlphaLevel = value;
					Invalidate();
				}
			}
		}

		[Category("Predefined Styles")]
		[Description("Determines the default AlphaMode used in format styles with AlphaMode empty.")]
		[DefaultValue(AlphaMode.Opaque)]
		public AlphaMode DefaultAlphaMode
		{
			get
			{
				return _defaultAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == AlphaMode.Empty)
				{
					throw new InvalidOperationException("DefaultAlphaMode property can not be empty");
				}
				if (value != _defaultAlphaMode)
				{
					_defaultAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Category("Predefined Styles")]
		[DefaultValue(AlphaMode.Empty)]
		[Description("Determines the default BackColorAlphaMode used in format styles with BackColorAlphaMode empty.")]
		public AlphaMode DefaultBackColorAlphaMode
		{
			get
			{
				return _defaultBackColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _defaultBackColorAlphaMode)
				{
					_defaultBackColorAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Description("Determines the default ForeColorAlphaMode used in format styles with ForeColorAlphaMode empty.")]
		[DefaultValue(AlphaMode.Empty)]
		[Category("Predefined Styles")]
		public AlphaMode DefaultForeColorAlphaMode
		{
			get
			{
				return _defaultForeColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _defaultForeColorAlphaMode)
				{
					_defaultForeColorAlphaMode = value;
					Invalidate();
				}
			}
		}

		[Description("Format settings used to draw rows with errors.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		public GridFormatStyle RowWithErrorsFormatStyle => _rowWithErrorsFormatStyle;

		[Category("Predefined Styles")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Format setting used to draw link columns.")]
		public GridFormatStyle LinkFormatStyle => _linkFormatStyle;

		[Description("Format settings used to draw the selected cell in a DataGrid control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		public GridFormatStyle FocusCellFormatStyle => _focusCellFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		[Description("Format settings used to draw cells in grid rows.")]
		public GridFormatStyle RowFormatStyle => _rowFormatStyle;

		[Description("Format settings used to draw column and row headers.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		public GridFormatStyle RowHeaderFormatStyle => _rowHeaderFormatStyle;

		[Category("Predefined Styles")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Format settings used to draw column and row headers.")]
		public GridFormatStyle HeaderFormatStyle => _headerFormatStyle;

		[Description("Format settings used to draw table headers.")]
		[Category("Predefined Styles")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridFormatStyle TableHeaderFormatStyle => _tableHeaderFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Format settings used to draw every other row when AlternatingRows property is true.")]
		[Category("Predefined Styles")]
		public GridFormatStyle AlternatingRowFormatStyle => _alternatingRowFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		[Description("Format settings used to draw every the 'Group By Box'.")]
		public GridFormatStyle GroupByBoxFormatStyle => _groupByBoxFormatStyle;

		[Description("Format settings used to draw the box with the info text in the 'Group By Box'.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		public GridFormatStyle GroupByBoxInfoFormatStyle => _groupByBoxInfoFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		[Description("Format setting used to draw group header rows.")]
		public GridFormatStyle GroupRowFormatStyle => _groupRowFormatStyle;

		[Category("Predefined Styles")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Format settings used to draw group total rows.")]
		public GridFormatStyle GroupTotalRowFormatStyle => _groupTotalRowFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Format settings used to draw cells in total rows.")]
		[Category("Predefined Styles")]
		public GridFormatStyle TotalRowFormatStyle => _totalRowFormatStyle;

		[Category("Predefined Styles")]
		[Description("Format settings used to draw cells in the new row at the top of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridFormatStyle NewRowFormatStyle => _gridGridFormatStyle;

		[Category("Predefined Styles")]
		[Description("format settings used to draw preview rows.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridFormatStyle PreviewRowFormatStyle => _previewRowFormatStyle;

		[Description("Format settings used to draw selected rows.")]
		[Category("Predefined Styles")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridFormatStyle SelectedFormatStyle => _selectedFormatStyle;

		[Description("Format settings used to draw selected rows when DataGrid control is not focused.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		public GridFormatStyle SelectedInactiveFormatStyle => _selectedInactiveFormatStyle;

		[Description("Format settings used to draw captions in cards.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		public GridFormatStyle CardCaptionFormatStyle => _cardCaptionFormatStyle;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Format settings used to draw column headers in cards.")]
		[Category("Predefined Styles")]
		public GridFormatStyle CardColumnHeaderFormatStyle => _cardColumnHeaderFormatStyle;

		[Description("Format settings used to draw cells in the filter row.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Predefined Styles")]
		public GridFormatStyle FilterRowFormatStyle => _filterRowFormatStyle;

		MxDataGrid IFormatStyleOwner.DataGrid => this;
	}
}
