using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Data;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridColumn
	{
		string IMordexXmlLayoutsSupport.Identifier => Key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Description("Determines which sort indicator appears in the header of the column.")]
		[DefaultValue(SortIndicator.Automatic)]
		public SortIndicator SortIndicator
		{
			get
			{
				return _sortIndicator;
			}
			set
			{
				if (!Enum.IsDefined(typeof(SortIndicator), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_sortIndicator != value)
				{
					_sortIndicator = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.CustomGroupConverter, Mordex.Windows.DataGrid.Design")]
		[Editor("Mordex.Windows.DataGrid.Design.CustomGroupEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("The CustomGroup used to create the group when the column header is dropped in the Group By Box area.")]
		public GridCustomGroup CustomGroup
		{
			get
			{
				return _customGroup;
			}
			set
			{
				if (value != null && Table != null && Table != value.GetOwnerTable())
				{
					throw new ArgumentException("CustomGroup must belong to the table the column belongs.");
				}
				_customGroup = value;
			}
		}

		[DefaultValue("")]
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

		[Description("The name of a property in the data source for which the column displays its data.")]
		[DefaultValue("")]
		[Category("Data")]
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

		[Description("Indicates whether the column can be grouped by the user.")]
		[Category("Groups")]
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

		[Category("Behavior")]
		[Description("Indicates whether the column can be sized by the user.")]
		[DefaultValue(true)]
		public bool AllowSize
		{
			get
			{
				return _allowSize;
			}
			set
			{
				_allowSize = value;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the column can be dragged by the user.")]
		[Category("Behavior")]
		public bool AllowDrag
		{
			get
			{
				if (_actAsSelector && _useHeaderSelector)
				{
					return false;
				}
				return _allowDrag;
			}
			set
			{
				if (value != _allowDrag)
				{
					_allowDrag = value;
				}
			}
		}

		[DefaultValue(ButtonStyle.NoButton)]
		[Category("Button Style")]
		[Description("Determines which button style will be used in cells that belong to the column.")]
		public ButtonStyle ButtonStyle
		{
			get
			{
				return _buttonStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ButtonStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_buttonStyle != value)
				{
					_buttonStyle = value;
					if (DataGrid != null)
					{
						DataGrid.RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Description("Determines how buttons are displayed in the cells of this column.")]
		[DefaultValue(CellButtonDisplayMode.CurrentCell)]
		[Category("Button Style")]
		public CellButtonDisplayMode ButtonDisplayMode
		{
			get
			{
				return _buttonDisplayMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CellButtonDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_buttonDisplayMode != value)
				{
					_buttonDisplayMode = value;
					if (DataGrid != null)
					{
						DataGrid.RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Description("Determines how buttons from the edit control are displayed in the cells of this column.")]
		[DefaultValue(CellButtonDisplayMode.EditingCell)]
		[Category("Display")]
		public CellButtonDisplayMode EditButtonDisplayMode
		{
			get
			{
				return _editButtonDisplayMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CellButtonDisplayMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_editButtonDisplayMode != value)
				{
					_editButtonDisplayMode = value;
					if (DataGrid != null)
					{
						DataGrid.RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[Category("Button Style")]
		[Description("The text used in text buttons on cells that belong to the column.")]
		[DefaultValue("")]
		public string ButtonText
		{
			get
			{
				return _buttonCaption;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (value != _buttonCaption)
				{
					_buttonCaption = value;
					if (_buttonStyle == ButtonStyle.TextButton)
					{
						NotifyCollectionLayoutChanged();
					}
				}
			}
		}

		[Category("Button Style")]
		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("The image used in image buttons on cells that belong to the column.")]
		public Image ButtonImage
		{
			get
			{
				return _buttonImage;
			}
			set
			{
				if (value != _buttonImage)
				{
					_buttonImage = value;
					if (_buttonStyle == ButtonStyle.Image)
					{
						NotifyCollectionLayoutChanged();
					}
				}
			}
		}

		[Category("Display")]
		[Description("The text displayed in the column's heading area.")]
		[DefaultValue("")]
		public string Caption
		{
			get
			{
				return base.BaseDisplayName;
			}
			set
			{
				base.BaseDisplayName = value;
				if (DataGrid != null)
				{
					DataGrid.NotifyRefreshListeners();
				}
			}
		}

		[DefaultValue("")]
		[Category("Display")]
		public string FilterBuilderDisplayName
		{
			get
			{
				return base.BaseFilterBuilderDisplayName;
			}
			set
			{
				base.BaseFilterBuilderDisplayName = value;
			}
		}

		[Category("Appearance")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Format settings used to draw cells that belong to the column.")]
		public GridFormatStyle CellStyle
		{
			get
			{
				if (_cellStyle == null)
				{
					_cellStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _cellStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (_cellStyle == value)
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
				if (_cellStyle != null && _cellStyle._owner == this)
				{
					_cellStyle._owner = null;
				}
				_cellStyle = value;
				NotifyCollectionLayoutChanged();
			}
		}

		[Category("Layout")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Position
		{
			get
			{
				if (Table != null && Table.CellLayoutMode == CellLayoutMode.UseColumnSets)
				{
					return _gridColumnPosition;
				}
				return _position;
			}
			set
			{
				if (GetOwnerColumnCollection() == null)
				{
					throw new InvalidOperationException();
				}
				if (value < 0 || value > GetOwnerColumnCollection().Count - 1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _position)
				{
					if (GetOwnerColumnCollection() != null)
					{
						GetOwnerColumnCollection().ShiftPositionsAfterRemove(_position);
						GetOwnerColumnCollection().ShiftPositionsForInsert(value);
					}
					_position = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("Determines how column contents are displayed.")]
		[Category("Display")]
		[DefaultValue(ColumnType.Text)]
		public ColumnType ColumnType
		{
			get
			{
				if (_actAsSelector)
				{
					return ColumnType.CheckBox;
				}
				return _columnType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ColumnType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (MultipleValues)
				{
					switch (value)
					{
					case ColumnType.CheckBox:
					case ColumnType.BoundImage:
					case ColumnType.Link:
						throw new ArgumentException("ColumnType '" + value.ToString() + "' can not be used in a MultipleValues column.");
					}
				}
				if (value != _columnType)
				{
					_columnType = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[DefaultValue(false)]
		[Description("Determines whether the column is bound to a list of values or a single value.")]
		public bool MultipleValues
		{
			get
			{
				return base.BaseMultipleValues;
			}
			set
			{
				base.BaseMultipleValues = value;
			}
		}

		[Description("Specifies how the list of values in a MultipleValues cell is sorted.")]
		[DefaultValue(ColumnCompareTarget.Value)]
		public ColumnCompareTarget MultipleValuesSort
		{
			get
			{
				return DataState.ComputeColumnCompareTarget(base.BaseMultipleValuesSort);
			}
			set
			{
				base.BaseMultipleValuesSort = DataState.ComputeCompareTarget(value);
			}
		}

		[Category("Display")]
		[DefaultValue(-1)]
		[Description("The image list index value of the image displayed by default on the cells that belong to the column.")]
		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
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
						ImageKey = "";
					}
					_imageIndex = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("The key of the image displayed by default on the cells that belong to the column.")]
		[Category("Display")]
		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue("")]
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
					if (_imageKey.Length > 0)
					{
						_imageIndex = -1;
					}
					_imageKey = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Display")]
		[DefaultValue(null)]
		[Description("The Image displayed by default on the cells that belong to the column.")]
		public Image Image
		{
			get
			{
				return _columnImage;
			}
			set
			{
				if (value != _columnImage)
				{
					_columnImage = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[DefaultValue(EditType.TextBox)]
		[Description("Determines how a column is edited by the user.")]
		[Category("Edit")]
		public EditType EditType
		{
			get
			{
				if (_actAsSelector)
				{
					return EditType.CheckBox;
				}
				return _editType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(EditType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == _editType)
				{
					return;
				}
				_editType = value;
				if (DataGrid != null)
				{
					if (DataGrid.CurrentColumn == this && DataGrid.EditMode == EditMode.EditOn)
					{
						DataGrid.EditMode = EditMode.EditOff;
						DataGrid.EditMode = EditMode.EditOn;
					}
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("The format to use when rendering values.")]
		[DefaultValue("")]
		[Category("Display")]
		public string FormatString
		{
			get
			{
				return base.BaseFormatString;
			}
			set
			{
				if (value != FormatString)
				{
					base.BaseFormatString = value;
					RefreshHostGrid();
				}
			}
		}

		[DefaultValue(FormatMode.UseIFormattable)]
		[Description("Determines how values will be formatted.")]
		[Category("Display")]
		public FormatMode FormatMode
		{
			get
			{
				return DataState.ToGridFormatMode(base.BaseFormatMode);
			}
			set
			{
				if (FormatMode != value)
				{
					base.BaseFormatMode = DataState.ToDataFormatMode(value);
					RefreshHostGrid();
				}
			}
		}

		[Category("Column Totals")]
		[Description("Indicates the format to use when rendering values in total row cells.")]
		[DefaultValue("")]
		public string TotalFormatString
		{
			get
			{
				return _summaryFormat;
			}
			set
			{
				if (value != _summaryFormat)
				{
					_summaryFormat = value;
					if (DataGrid != null)
					{
						DataGrid.RebuildRows(false);
					}
				}
			}
		}

		[DefaultValue(FormatMode.UseIFormattable)]
		[Category("Column Totals")]
		[Description("Determines how values will be formatted in total rows.")]
		public FormatMode TotalFormatMode
		{
			get
			{
				return _totalFormatMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FormatMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == FormatMode.UseDefault)
				{
					value = FormatMode.UseIFormattable;
				}
				if (value != _totalFormatMode)
				{
					_totalFormatMode = value;
					if (DataGrid != null)
					{
						DataGrid.RebuildRows(false);
					}
				}
			}
		}

		[Category("Groups")]
		[DefaultValue("(none)")]
		[Description("The text displayed in a group row when the value grouped is an empty string or null.")]
		public string GroupEmptyString
		{
			get
			{
				return base.BaseGroupEmptyString;
			}
			set
			{
				base.BaseGroupEmptyString = value;
			}
		}

		[Category("Groups")]
		[Description("Indicates the format to use when rendering group row values.")]
		public string DefaultGroupFormatString
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

		[Description("Determines how values will be formatted in total rows.")]
		[Category("Groups")]
		public FormatMode DefaultGroupFormatMode
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

		[Category("Groups")]
		[Description("The text displayed in a group header row before the group row value.")]
		[DefaultValue(null)]
		public string DefaultGroupPrefix
		{
			get
			{
				if (_defaultGroupPrefix == null)
				{
					return Caption + ":";
				}
				return _defaultGroupPrefix;
			}
			set
			{
				if (value != _defaultGroupPrefix)
				{
					_defaultGroupPrefix = value;
					NotifyIfColumnIsGrouped();
				}
			}
		}

		[DefaultValue("")]
		[Category("Display")]
		[Description("The key of the image displayed on the header.")]
		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		public string HeaderImageKey
		{
			get
			{
				return _headerImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_headerImageKey != value)
				{
					_headerImageKey = value;
					if (_headerImageKey.Length > 0)
					{
						_headerImageIndex = -1;
					}
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("The Image displayed on the header.")]
		[DefaultValue(null)]
		[Category("Display")]
		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public Image HeaderImage
		{
			get
			{
				return _headerImage;
			}
			set
			{
				if (value != _headerImage)
				{
					_headerImage = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("The image list index value of the image displayed on the header.")]
		[Category("Display")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(-1)]
		public int HeaderImageIndex
		{
			get
			{
				return _headerImageIndex;
			}
			set
			{
				if (value != _headerImageIndex)
				{
					_headerImageIndex = value;
					if (_headerImageIndex != -1)
					{
						_headerImageKey = "";
					}
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Category("Appearance")]
		[Description("Format settings used to draw the header of the column.")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public GridFormatStyle HeaderStyle
		{
			get
			{
				if (_headerStyle == null)
				{
					_headerStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _headerStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _headerStyle)
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
				if (_headerStyle != null && _headerStyle._owner == this)
				{
					_headerStyle._owner = null;
				}
				_headerStyle = value;
				NotifyCollectionLayoutChanged();
			}
		}

		[DefaultValue("")]
		[Description("The text displayed in a tool tip when users moves the mouse pointer over a column header.")]
		public string HeaderToolTip
		{
			get
			{
				return _headerToolTip;
			}
			set
			{
				if (value != _headerToolTip)
				{
					_headerToolTip = value;
				}
			}
		}

		[Description("Specifies the maximum number of characters allowed in the cells that belong to the column.")]
		[DefaultValue(0)]
		[Category("Edit")]
		public int MaxLength
		{
			get
			{
				return _maxLength;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _maxLength)
				{
					_maxLength = value;
				}
			}
		}

		[Description("Specifies the minimum number of lines that should be displayed in the cells that belong to the column.")]
		[DefaultValue(1)]
		[Category("Display")]
		public int MinLines
		{
			get
			{
				return _minLines;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _minLines)
				{
					_minLines = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[DefaultValue(1)]
		[Category("Display")]
		[Description("Specifies the maximum number of lines that should be displayed in the cells that belong to the column.")]
		public int MaxLines
		{
			get
			{
				return _maxLines;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _maxLines)
				{
					_maxLines = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Indicates whether the user can select cells that belong to the column.")]
		public bool Selectable
		{
			get
			{
				return _selectable;
			}
			set
			{
				if (value != _selectable)
				{
					_selectable = value;
				}
			}
		}

		[Category("Card View")]
		[Description("Indicates whether the caption of the column will be displayed in the card body.")]
		[DefaultValue(true)]
		public bool CardViewCaptionVisible
		{
			get
			{
				return _cardViewCaptionVisible;
			}
			set
			{
				if (value != _cardViewCaptionVisible)
				{
					_cardViewCaptionVisible = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Browsable(false)]
		public SortOrder SortOrder
		{
			get
			{
				if (Table != null)
				{
					foreach (GridGroup group in Table.Groups)
					{
						if (group.Column == this)
						{
							return group.SortOrder;
						}
					}
					foreach (GridSortKey sortKey in Table.SortKeys)
					{
						if (sortKey.Column == this)
						{
							return sortKey.SortOrder;
						}
					}
				}
				return SortOrder.Unsorted;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IComparer SortComparer
		{
			get
			{
				return base.BaseSortComparer;
			}
			set
			{
				base.BaseSortComparer = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IComparer GroupComparer
		{
			get
			{
				return base.BaseGroupComparer;
			}
			set
			{
				base.BaseGroupComparer = value;
			}
		}

		[TypeConverter("System.ComponentModel.StringConverter")]
		[Description("User defined data to associate with the column.")]
		[DefaultValue(null)]
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

		[DefaultValue(false)]
		[Description("Indicates whether the column shold have a ValueList.")]
		[Category("Combo Settings")]
		public bool HasValueList
		{
			get
			{
				return base.BaseHasValueList;
			}
			set
			{
				base.BaseHasValueList = value;
			}
		}

		[Category("Combo Settings")]
		[Browsable(false)]
		[DefaultValue(null)]
		public GridValueListItemCollection ValueList => (GridValueListItemCollection)base.BaseValueList;

		[Browsable(false)]
		public GridValueListItemCollection EditValueList
		{
			get
			{
				return _editValueList;
			}
			set
			{
				_editValueList = value;
			}
		}

		[Category("Layout")]
		[Description("Indicates whether the column is visible or hidden.")]
		[DefaultValue(true)]
		public bool Visible
		{
			get
			{
				if (_owner != null && !_owner.Visible && Table.CellLayoutMode == CellLayoutMode.UseColumnSets)
				{
					return false;
				}
				if (ShouldHideWhenGrouped() && Group != null)
				{
					return false;
				}
				return _visible;
			}
			set
			{
				if (value != _visible && (DataGrid == null || DataGrid.RaiseColumnVisibleChanging(this)))
				{
					if (value && Table != null && DataGrid != null && DataGrid.ColumnAutoResize && Table.CellLayoutMode == CellLayoutMode.UseColumns)
					{
						Table._isAutoResizePending = true;
						int otherValue = DataGrid.ClientRectangle.Width - Width;
						Table.FitToWidth(otherValue);
					}
					_visible = value;
					if (_owner != null)
					{
						_owner.Width = -1;
					}
					RequestAutoResize();
					NotifyCollectionLayoutChanged();
					if (GetOwnerColumnCollection() != null)
					{
						GetOwnerColumnCollection().NotifyColumnVisibilityChanged(this);
					}
					if (DataGrid != null)
					{
						DataGrid.RaiseColumnVisibleChanged(this);
					}
				}
			}
		}

		[Description("Determines whether text in cells should be wrapped to the begining of the next line when necessary.")]
		[Category("Display")]
		[DefaultValue(false)]
		public bool WordWrap
		{
			get
			{
				return _wordWrap;
			}
			set
			{
				if (value != _wordWrap)
				{
					_wordWrap = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("Indicates whether a column should be sorted when the user clicks on its header.")]
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool AllowSort
		{
			get
			{
				return _allowSort;
			}
			set
			{
				if (value != _allowSort)
				{
					_allowSort = value;
				}
			}
		}

		[Browsable(false)]
		public MxDataGrid DataGrid => GetOwnerColumnCollection()?.DataGrid;

		[Category("Layout")]
		[DefaultValue(100)]
		[Description("Specifies the width of the column in pixels.")]
		public int Width
		{
			get
			{
				return _width;
			}
			set
			{
				if (value < 8)
				{
					value = 8;
				}
				if (value == _width)
				{
					return;
				}
				int width = value - _width;
				if (_owner != null && Table.CellLayoutMode == CellLayoutMode.UseColumnSets)
				{
					int column = _columnSetColumn + _colSpan - 1;
					if (_owner.GetColumnWidth(column) + width > 8)
					{
						_owner.SetColumnWidth(column, _owner.GetColumnWidth(column) + width);
					}
					else
					{
						width = _owner.GetColumnWidth(column) + width - 8;
						_owner.SetColumnWidth(column, 8);
						_owner.Width += width;
					}
				}
				else
				{
					_width = value;
				}
				RequestAutoResize();
				NotifyCollectionLayoutChanged();
			}
		}

		[Browsable(false)]
		public GridTable Table
		{
			get
			{
				if (GetOwnerColumnCollection() == null)
				{
					return null;
				}
				return GetOwnerColumnCollection().GetOwnerTable();
			}
		}
	}
}
