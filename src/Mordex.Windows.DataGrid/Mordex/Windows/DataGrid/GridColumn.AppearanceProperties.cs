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
		[Category("Groups")]
		[DefaultValue(GroupInterval.Value)]
		[Description("Determines how groups should be calculated.")]
		public GroupInterval DefaultGroupInterval
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

		[Description("Determines if the formatted text or the plain value should be used when editing a cell.")]
		[Category("Edit")]
		[DefaultValue(EditTarget.Value)]
		public EditTarget EditTarget
		{
			get
			{
				return _editTarget;
			}
			set
			{
				_editTarget = value;
			}
		}

		[Browsable(false)]
		public GridColumnSet ColumnSet => _owner;

		[Category("ColumnSet")]
		[Description("Specifies which row of a column set the column is placed.")]
		[DefaultValue(-1)]
		public int ColumnSetRow
		{
			get
			{
				return _columnSetRow;
			}
			set
			{
				if (_owner == null)
				{
					throw new InvalidOperationException("ColumnSetRow property can only be set when the column belongs to a ColumnSet");
				}
				if (value < 0 || value > Table.ColumnSetRowCount - 1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _columnSetRow)
				{
					int col = _columnSetColumn;
					int rowSpan = _rowSpan;
					int colSpan = _colSpan;
					if (value + _rowSpan > Table.ColumnSetRowCount)
					{
						rowSpan = Table.ColumnSetRowCount - value;
					}
					GridColumnSet gridColumnSet = _owner;
					gridColumnSet.Remove(this);
					gridColumnSet.Add(this, value, col, rowSpan, colSpan);
				}
			}
		}

		[Category("ColumnSet")]
		[Description("Specifies which column of a column set the object is placed.")]
		[DefaultValue(-1)]
		public int ColumnSetColumn
		{
			get
			{
				return _columnSetColumn;
			}
			set
			{
				if (_owner == null)
				{
					throw new InvalidOperationException("ColumnSetColumn property can only be set when the column belongs to a ColumnSet");
				}
				if (value < 0 || value > _owner.ColumnCount - 1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _columnSetColumn)
				{
					int row = _columnSetRow;
					int rowSpan = _rowSpan;
					int colSpan = _colSpan;
					if (value + _colSpan > _owner.ColumnCount)
					{
						colSpan = _owner.ColumnCount - value;
					}
					GridColumnSet gridColumnSet = _owner;
					gridColumnSet.Remove(this);
					gridColumnSet.Add(this, row, value, rowSpan, colSpan);
				}
			}
		}

		[Description("Specifies the number of columns in a column set, the object will span.")]
		[DefaultValue(-1)]
		[Category("ColumnSet")]
		public int ColSpan
		{
			get
			{
				return _colSpan;
			}
			set
			{
				if (_owner == null)
				{
					throw new InvalidOperationException("ColSpan property can only be set when the column belongs to a ColumnSet");
				}
				if (value < 1 || value + _columnSetColumn > _owner.ColumnCount)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _colSpan)
				{
					int row = _columnSetRow;
					int rowSpan = _rowSpan;
					int col = _columnSetColumn;
					GridColumnSet gridColumnSet = _owner;
					gridColumnSet.Remove(this);
					gridColumnSet.Add(this, row, col, rowSpan, value);
				}
			}
		}

		[DefaultValue(-1)]
		[Category("ColumnSet")]
		[Description("Specifies the number of rows in a column set, the object will span.")]
		public int RowSpan
		{
			get
			{
				return _rowSpan;
			}
			set
			{
				if (_owner == null)
				{
					throw new InvalidOperationException("RowSpan property can only be set when the column belongs to a ColumnSet");
				}
				if (value < 1 || value + _columnSetRow > Table.ColumnSetRowCount)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _rowSpan)
				{
					int row = _columnSetRow;
					int colSpan = _colSpan;
					int col = _columnSetColumn;
					GridColumnSet gridColumnSet = _owner;
					gridColumnSet.Remove(this);
					gridColumnSet.Add(this, row, col, value, colSpan);
				}
			}
		}

		[Description("Specifies the left margin of the cells that belong to the column.")]
		[Category("Layout")]
		[DefaultValue(1)]
		public int LeftMargin
		{
			get
			{
				return _leftMargin;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("");
				}
				if (value != _leftMargin)
				{
					_leftMargin = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("Specifies the top margin of the cells that belong to the column.")]
		[DefaultValue(1)]
		[Category("Layout")]
		public int TopMargin
		{
			get
			{
				return _topMargin;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _topMargin)
				{
					_topMargin = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Category("Layout")]
		[DefaultValue(1)]
		[Description("Specifies the right margin of the cells that belong to the column.")]
		public int RightMargin
		{
			get
			{
				return _rightMargin;
			}
			set
			{
				if (value != _rightMargin)
				{
					_rightMargin = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[DefaultValue(1)]
		[Category("Layout")]
		[Description("Specifies the bottom margin of the cells that belong to the column.")]
		public int BottomMargin
		{
			get
			{
				return _bottomMargin;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _bottomMargin)
				{
					_bottomMargin = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("Determines how the text is aligned in the cells that belong to the column.")]
		[DefaultValue(TextAlignment.Empty)]
		[Category("Display")]
		public TextAlignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _textAlignment)
				{
					_textAlignment = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[DefaultValue(TextAlignment.Empty)]
		[Category("Display")]
		[Description("Determines how the line(s) of text are aligned in the cells that belong to the column.")]
		public TextAlignment LineAlignment
		{
			get
			{
				return _lineAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _lineAlignment)
				{
					_lineAlignment = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("Determines how text is aligned in the column header.\n\rIf Empty, TextAlignment in HeaderFormatStyle is used.")]
		[Category("Display")]
		[DefaultValue(TextAlignment.Empty)]
		public TextAlignment HeaderAlignment
		{
			get
			{
				return _headerAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _headerAlignment)
				{
					_headerAlignment = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Category("Display")]
		[Description("Determines how the line(s) of text are aligned in the header.\n\rIf Empty, LineAlignment in HeaderFormatStyle is used.")]
		[DefaultValue(TextAlignment.Empty)]
		public TextAlignment HeaderLineAlignment
		{
			get
			{
				return _headerLineAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _headerLineAlignment)
				{
					_headerLineAlignment = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[DefaultValue(AggregateFunction.None)]
		[Description("The aggregate function whose result will be shown in total rows for a column.")]
		[Category("Column Totals")]
		public AggregateFunction AggregateFunction
		{
			get
			{
				return _aggregateFunction;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AggregateFunction), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _aggregateFunction)
				{
					_aggregateFunction = value;
					DataGrid?.RefreshAggregateRows();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Category("Data")]
		[Browsable(false)]
		[DefaultValue(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Indicates whether the column sholud get its values from the data source.")]
		[Obsolete("Bound property is obsolete. Use BoundMode property instead.")]
		public bool Bound
		{
			get
			{
				if (_actAsSelector)
				{
					return false;
				}
				return BoundMode == ColumnBoundMode.Bound;
			}
			set
			{
				if (value)
				{
					BoundMode = ColumnBoundMode.Bound;
				}
				else
				{
					BoundMode = ColumnBoundMode.UnboundFetch;
				}
			}
		}

		[DefaultValue(ColumnBoundMode.Bound)]
		[Description("Determines how cell values are retrieved")]
		[Category("Data")]
		public ColumnBoundMode BoundMode
		{
			get
			{
				return DataState.ToGridColumnBoundMode(base.BaseBoundMode);
			}
			set
			{
				base.BaseBoundMode = DataState.ComputeColumnBoundMode(value);
			}
		}

		[Category("Card View")]
		[Description("Indicates whether the column’s value will be displayed in the card caption bar.")]
		[DefaultValue(false)]
		public bool CardCaption
		{
			get
			{
				return _cardCaption;
			}
			set
			{
				if (_cardCaption == value)
				{
					return;
				}
				GridColumnCollection gridColumnCollection = GetOwnerColumnCollection();
				if (gridColumnCollection == null)
				{
					_cardCaption = value;
					return;
				}
				if (value)
				{
					foreach (GridColumn item in gridColumnCollection)
					{
						item.SetCardCaption();
					}
					_cardCaption = true;
					gridColumnCollection._owner = this;
				}
				else
				{
					gridColumnCollection._owner = null;
					_cardCaption = false;
				}
				NotifyCollectionLayoutChanged();
			}
		}

		[Category("Card View")]
		[Description("Indicates whether a column’s icon will be displayed in the card caption bar.")]
		[DefaultValue(false)]
		public bool CardIcon
		{
			get
			{
				return _cardIcon;
			}
			set
			{
				if (_cardIcon == value)
				{
					return;
				}
				GridColumnCollection gridColumnCollection = GetOwnerColumnCollection();
				if (gridColumnCollection == null)
				{
					_cardIcon = value;
				}
				else if (value)
				{
					foreach (GridColumn item in gridColumnCollection)
					{
						item.SetCardIcon();
					}
					_cardIcon = true;
					gridColumnCollection._gridColumnCollectionOwner = this;
				}
				else
				{
					gridColumnCollection._gridColumnCollectionOwner = null;
					_cardIcon = false;
				}
			}
		}

		[Description("Specifies the color to treat as transparent.")]
		[Category("BoundImage Settings")]
		public Color BoundImageMaskColor
		{
			get
			{
				return _imageMaskColor;
			}
			set
			{
				if (_imageMaskColor != value)
				{
					_imageMaskColor = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Category("BoundImage Settings")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The size in which bound images should be drawn.")]
		public Size BoundImageSize
		{
			get
			{
				return _boundImageSize;
			}
			set
			{
				if (value != _boundImageSize)
				{
					_boundImageSize = value;
					if (ColumnType == ColumnType.BoundImage)
					{
						NotifyCollectionLayoutChanged();
					}
				}
			}
		}

		[Category("Groups")]
		[Description("Format settings used to draw group rows that belong to the column.")]
		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public GridFormatStyle GroupRowStyle
		{
			get
			{
				if (_groupRowStyle == null)
				{
					_groupRowStyle = new GridFormatStyle(this, PredefinedStyle.None);
				}
				return _groupRowStyle;
			}
			set
			{
				if (value == null)
				{
					value = new GridFormatStyle();
				}
				if (value == _groupRowStyle)
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
				if (_groupRowStyle != null && _groupRowStyle._owner == this)
				{
					_groupRowStyle._owner = null;
				}
				_groupRowStyle = value;
				NotifyCollectionLayoutChanged();
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.InputMaskEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Represents the input mask used when editing a control.")]
		[DefaultValue("")]
		[Category("Edit")]
		public string InputMask
		{
			get
			{
				return base.BaseMask;
			}
			set
			{
				base.BaseMask = value;
			}
		}

		[Description("Represents the character used to prompt a user for input when a mask is used.")]
		[DefaultValue('_')]
		[Category("Edit")]
		public char MaskPrompt
		{
			get
			{
				return _maskPrompt;
			}
			set
			{
				if (value != _maskPrompt)
				{
					_maskPrompt = value;
				}
			}
		}

		[Category("Edit")]
		[Description("Indicates whether literals should be included when updating a masked value.")]
		[DefaultValue(true)]
		public bool IncludeLiterals
		{
			get
			{
				return _includeLiterals;
			}
			set
			{
				if (value != _includeLiterals)
				{
					_includeLiterals = value;
				}
			}
		}

		[Description("Indicates whether the column should be shown in the field chooser window.")]
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool ShowInFieldChooser
		{
			get
			{
				return _showInFieldChooser;
			}
			set
			{
				if (_showInFieldChooser != value)
				{
					_showInFieldChooser = value;
					if (DataGrid != null)
					{
						DataGrid.NotifyRefreshListeners();
					}
				}
			}
		}

		[Description("Determines when the column shulod be printed.")]
		[DefaultValue(ColumnPrintMode.PrintWhenVisible)]
		[Category("Behavior")]
		public ColumnPrintMode PrintMode
		{
			get
			{
				return _printMode;
			}
			set
			{
				_printMode = value;
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.DropDownConverter, Mordex.Windows.DataGrid.Design")]
		[Editor("Mordex.Windows.DataGrid.Design.DropDownEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Combo Settings")]
		[DefaultValue(null)]
		[Description("The GridDropdown object that acts as the drop down list for the column.")]
		public GridDropDown DropDown
		{
			get
			{
				return _dropDown;
			}
			set
			{
				if (value != null && value.DropDownCollection != null && value.DropDownCollection.DataGrid != DataGrid)
				{
					throw new InvalidOperationException("DropDown must be defined in the DropDowns collection of the DataGrid control.");
				}
				if (_dropDown != value)
				{
					_dropDown = value;
					if (DataGrid != null)
					{
						DataGrid.RebuildRows(true);
					}
				}
			}
		}

		[Description("Indicates whether null values shuld be allowed in a check box column.")]
		[Category("CheckBox Settings")]
		[DefaultValue(false)]
		public bool CheckBoxTriState
		{
			get
			{
				return _checkBoxTriState;
			}
			set
			{
				if (_checkBoxTriState != value)
				{
					_checkBoxTriState = value;
				}
			}
		}

		[Description("Represents the actual value used when setting the value of a check box column to null.")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ObjectTypeConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(null)]
		[Category("CheckBox Settings")]
		[Editor("Mordex.Windows.DataGrid.Design.ObjectTypeEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public object CheckBoxNullValue
		{
			get
			{
				return _checkBoxNullValue;
			}
			set
			{
				if (_checkBoxNullValue != value)
				{
					_checkBoxNullValue = value;
				}
			}
		}

		[DefaultValue(true)]
		[Description("Represents the actual value used when setting the value of the column to true.")]
		[Editor("Mordex.Windows.DataGrid.Design.ObjectTypeEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ObjectTypeConverter, Mordex.Windows.DataGrid.Design")]
		[Category("CheckBox Settings")]
		public object CheckBoxTrueValue
		{
			get
			{
				return _checkBoxTrueValue;
			}
			set
			{
				if (_checkBoxTrueValue != value)
				{
					_checkBoxTrueValue = value;
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.ObjectTypeEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(false)]
		[Description("Represents the actual value used when setting the value of a chek box column to false.")]
		[Category("CheckBox Settings")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ObjectTypeConverter, Mordex.Windows.DataGrid.Design")]
		public object CheckBoxFalseValue
		{
			get
			{
				return _checkBoxFalseValue;
			}
			set
			{
				if (_checkBoxFalseValue != value)
				{
					_checkBoxFalseValue = value;
				}
			}
		}

		[Description("Indicates whether sort, find and filter operations sholud be done against the value or the display text of a cell.")]
		[DefaultValue(ColumnCompareTarget.Value)]
		public ColumnCompareTarget CompareTarget
		{
			get
			{
				return DataState.ComputeColumnCompareTarget(base.BaseCompareTarget);
			}
			set
			{
				base.BaseCompareTarget = DataState.ComputeCompareTarget(value);
			}
		}

		[Description("The text that will be displayed when a cell value is null or System.DBNull.")]
		[DefaultValue("")]
		[Category("Display")]
		public string NullText
		{
			get
			{
				return base.BaseNullText;
			}
			set
			{
				base.BaseNullText = value;
			}
		}

		[DefaultValue(FilterEditType.SameAsEditType)]
		[Description("Determines how cells in the filter row will be edited.")]
		[Category("Edit")]
		public FilterEditType FilterEditType
		{
			get
			{
				return _filterEditType;
			}
			set
			{
				if (_filterEditType != value)
				{
					_filterEditType = value;
					switch (_filterEditType)
					{
					case FilterEditType.NoEdit:
						BaseFilterEditType = Mordex.Data.FilterEditType.NoEdit;
						break;
					case FilterEditType.DatePicker:
						BaseFilterEditType = Mordex.Data.FilterEditType.DatePicker;
						break;
					case FilterEditType.CalendarDropDown:
						BaseFilterEditType = Mordex.Data.FilterEditType.CalendarDropDown;
						break;
					case FilterEditType.DropDownList:
					case FilterEditType.CheckBox:
						BaseFilterEditType = Mordex.Data.FilterEditType.DropDownList;
						break;
					case FilterEditType.Combo:
						BaseFilterEditType = Mordex.Data.FilterEditType.ComboBox;
						break;
					case FilterEditType.Custom:
						BaseFilterEditType = Mordex.Data.FilterEditType.EditBox;
						break;
					case FilterEditType.SameAsEditType:
						BaseFilterEditType = Mordex.Data.FilterEditType.Default;
						break;
					case FilterEditType.TextBox:
						BaseFilterEditType = Mordex.Data.FilterEditType.EditBox;
						break;
					}
					NotifyCollectionLayoutChanged();
				}
			}
		}

		protected override Mordex.Data.FilterEditType BaseDefaultFilterEditType
		{
			get
			{
				if (ColumnType == ColumnType.CheckBox)
				{
					return Mordex.Data.FilterEditType.DropDownList;
				}
				if (ColumnType == ColumnType.BoundImage)
				{
					return Mordex.Data.FilterEditType.NoEdit;
				}
				if (DropDown != null)
				{
					return Mordex.Data.FilterEditType.ComboBox;
				}
				if (ColumnType == ColumnType.Image)
				{
					return Mordex.Data.FilterEditType.NoEdit;
				}
				return base.BaseDefaultFilterEditType;
			}
		}

		[Browsable(false)]
		public bool IsGrouped
		{
			get
			{
				if (Group == null)
				{
					return false;
				}
				return true;
			}
		}

		[Browsable(false)]
		public GridGroup Group
		{
			get
			{
				if (Table == null)
				{
					return null;
				}
				foreach (GridGroup group in Table.Groups)
				{
					if (group.Column == this)
					{
						return group;
					}
				}
				return null;
			}
		}

		[Browsable(false)]
		public GridSortKey SortKey
		{
			get
			{
				if (Table == null)
				{
					return null;
				}
				foreach (GridSortKey sortKey in Table.SortKeys)
				{
					if (sortKey.Column == this)
					{
						return sortKey;
					}
				}
				return null;
			}
		}

		[Browsable(false)]
		public bool IsSorted
		{
			get
			{
				if (SortKey == null)
				{
					return false;
				}
				return true;
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether the column should be hidden when groupping by it.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean HideWhenGrouped
		{
			get
			{
				return _hideWhenGrouped;
			}
			set
			{
				_hideWhenGrouped = value;
				NotifyCollectionLayoutChanged();
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether the column should be hidden when the user drops it outside the control.")]
		[DefaultValue(InheritableBoolean.Default)]
		public InheritableBoolean AllowRemove
		{
			get
			{
				return _allowRemove;
			}
			set
			{
				if (!Enum.IsDefined(typeof(InheritableBoolean), value))
				{
					throw new InvalidEnumArgumentException();
				}
				_allowRemove = value;
			}
		}

		[DefaultValue(Trimming.Ellipsis)]
		[Category("Display")]
		[Description("Determines how to trim a string so that it fits into the cell bounds.")]
		public Trimming Trimming
		{
			get
			{
				return _trimming;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Trimming), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_trimming != value)
				{
					_trimming = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.FlagsEnumConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(ColumnOwnerDrawnMode.None)]
		[Editor("Mordex.Windows.DataGrid.Design.FlagsEnumEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Behavior")]
		[Description("Specifies which areas of the column should be drawn by the developer.")]
		public ColumnOwnerDrawnMode OwnerDrawnMode
		{
			get
			{
				return _ownerDrawnMode;
			}
			set
			{
				if (_ownerDrawnMode != value)
				{
					_ownerDrawnMode = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("Represents the value stored in the data source when the value entered by the user is an empty string.")]
		[Browsable(false)]
		[Editor("Mordex.Windows.DataGrid.Design.ObjectTypeEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.DataGrid.Design.ObjectTypeConverter, Mordex.Windows.DataGrid.Design")]
		[Category("Edit")]
		public object EmptyStringValue
		{
			get
			{
				return _emptyTextValue;
			}
			set
			{
				_emptyTextValue = value;
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.ObjectTypeConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(null)]
		[Description("Represents the default value for the column when adding a new row.")]
		[Editor("Mordex.Windows.DataGrid.Design.ObjectTypeEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Category("Edit")]
		public object DefaultValue
		{
			get
			{
				return _defaultValue;
			}
			set
			{
				_defaultValue = value;
			}
		}

		[Category("Integer UpDown Settings")]
		[DefaultValue(0)]
		[Description("The minimum value an integer up down control can have.")]
		public int UpDownMinimum
		{
			get
			{
				return _upDownMinimum;
			}
			set
			{
				_upDownMinimum = value;
			}
		}

		[Category("Integer UpDown Settings")]
		[Description("The maximum value an integer up down control can have.")]
		[DefaultValue(100)]
		public int UpDownMaximum
		{
			get
			{
				return _upDownMaximum;
			}
			set
			{
				_upDownMaximum = value;
			}
		}

		[Category("Integer UpDown Settings")]
		[DefaultValue(1)]
		[Description("Indicates the increment or decrement in an integer up down control when the up or down buttons are clicked.")]
		public int UpDownIncrement
		{
			get
			{
				return _upDownIncrement;
			}
			set
			{
				_upDownIncrement = value;
			}
		}

		[Description("Specifies whether values not in a value list or a dropdown should be allowed.")]
		[Category("Combo Settings")]
		[DefaultValue(true)]
		public bool LimitToList
		{
			get
			{
				return _limitToList;
			}
			set
			{
				_limitToList = value;
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether the column should act as checkbox column in a checked list.")]
		[DefaultValue(false)]
		public bool ActAsSelector
		{
			get
			{
				return _actAsSelector;
			}
			set
			{
				if (value != _actAsSelector)
				{
					_actAsSelector = value;
					if (_actAsSelector)
					{
						BoundMode = ColumnBoundMode.UnboundFetch;
					}
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether an ActAsSelector column displays a check box in the header to allow the user to check all the rows at once.")]
		[Category("Behavior")]
		public bool UseHeaderSelector
		{
			get
			{
				return _useHeaderSelector;
			}
			set
			{
				if (value != _useHeaderSelector)
				{
					_useHeaderSelector = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}
	}
}
