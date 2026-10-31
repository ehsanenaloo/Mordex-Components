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
		[Browsable(false)]
		public Type Type
		{
			get
			{
				if (GetBasePropertyDescriptor() != null)
				{
					return GetBasePropertyDescriptor().PropertyType;
				}
				switch (DataTypeCode)
				{
				case TypeCode.Boolean:
					return typeof(bool);
				case TypeCode.Byte:
					return typeof(byte);
				case TypeCode.Char:
					return typeof(char);
				case TypeCode.DateTime:
					return typeof(DateTime);
				case TypeCode.DBNull:
					return typeof(DBNull);
				case TypeCode.Decimal:
					return typeof(decimal);
				case TypeCode.Double:
					return typeof(double);
				case TypeCode.Int16:
					return typeof(short);
				case TypeCode.Int32:
					return typeof(int);
				case TypeCode.Int64:
					return typeof(long);
				case TypeCode.SByte:
					return typeof(sbyte);
				case TypeCode.Single:
					return typeof(float);
				case TypeCode.String:
					return typeof(string);
				case TypeCode.UInt16:
					return typeof(ushort);
				case TypeCode.UInt32:
					return typeof(uint);
				case TypeCode.UInt64:
					return typeof(ulong);
				default:
					return typeof(object);
				}
			}
		}

		[DefaultValue(TypeCode.Object)]
		[Description("Determines the type of data in the column.")]
		public TypeCode DataTypeCode
		{
			get
			{
				return base.BaseDataTypeCode;
			}
			set
			{
				if (BoundMode == ColumnBoundMode.Bound)
				{
					throw new InvalidOperationException("DataTypeCode can only be set in unbound columns.");
				}
				base.BaseDataTypeCode = value;
			}
		}

		[Description("The text to display in tool tips for cells that belong to the column.")]
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

		[Description("The text to display in tool tips for buttons in cells that belong to the column.")]
		[DefaultValue(null)]
		public string ButtonToolTipText
		{
			get
			{
				return _buttonToolTipContent;
			}
			set
			{
				_buttonToolTipContent = value;
			}
		}

		[DefaultValue(CellToolTip.Default)]
		[Description("Indicates which tool tip to use in cells that belong to this column.")]
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

		[DefaultValue(true)]
		[Description("Indicates whether the casing should be ignored when comparing text.")]
		public bool CompareIgnoreCase
		{
			get
			{
				return base.BaseCompareIgnoreCase;
			}
			set
			{
				base.BaseCompareIgnoreCase = value;
			}
		}

		[DefaultValue('\0')]
		[Category("Display")]
		[Description("Represents the character used to mask characters of a password in a non WordWrap column.")]
		public char PasswordChar
		{
			get
			{
				return _passwordChar;
			}
			set
			{
				if (value != _passwordChar)
				{
					_passwordChar = value;
					if (DataGrid != null)
					{
						DataGrid.RebuildRows(true);
					}
				}
			}
		}

		[DefaultValue(CharacterCasing.Normal)]
		[Category("Edit")]
		[Description("Indicates whether the control modifies the case of characters as they are typed.")]
		public CharacterCasing CharacterCasing
		{
			get
			{
				return _characterCasing;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CharacterCasing), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _characterCasing)
				{
					_characterCasing = value;
					if (DataGrid != null)
					{
						DataGrid.RebuildRows(true);
					}
				}
			}
		}

		[Category("Edit")]
		[DefaultValue(InvalidValueAction.Default)]
		[Description("Determines the behavior of the control when an invalid value has been entered by the user in a cell that belongs to the column.")]
		public InvalidValueAction InvalidValueAction
		{
			get
			{
				return _invalidValueAction;
			}
			set
			{
				_invalidValueAction = value;
			}
		}

		[Category("Behavior")]
		[DefaultValue(SelectableCells.RowCells | SelectableCells.NewRowCells | SelectableCells.FilterRowCells)]
		[Description("Determines which cells can be selected in the column when the Selectable property of the column is True.")]
		public SelectableCells SelectableCells
		{
			get
			{
				return _gridColumnSelectableCells;
			}
			set
			{
				_gridColumnSelectableCells = value;
			}
		}

		MxDataGrid IFormatStyleOwner.DataGrid => DataGrid;

		[DefaultValue(RightToLeft.Inherit)]
		[Description("Indicates whether cells are aligned to support locales using right-to-left fonts.")]
		public RightToLeft RightToLeft
		{
			get
			{
				return _rightToLeft;
			}
			set
			{
				if (_rightToLeft != value)
				{
					_rightToLeft = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Description("Determines how the checkbox is aligned in the cells that belong to the column.")]
		[DefaultValue(CheckBoxAlignment.Default)]
		public CheckBoxAlignment CheckBoxAlignment
		{
			get
			{
				return _checkBoxAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(CheckBoxAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_checkBoxAlignment != value)
				{
					_checkBoxAlignment = value;
					NotifyCollectionLayoutChanged();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(ExportValueKind.Automatic)]
		[Description("Determines whether numeric values of the column are exported to Excel as numbers or as exact text.")]
		public ExportValueKind ExportValueKind
		{
			get
			{
				return _exportValueKind;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ExportValueKind), value))
				{
					throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(ExportValueKind));
				}
				_exportValueKind = value;
			}
		}

		[Description("Represents the property name from which to get the value of each element in the list bound to a cell.")]
		public string MultipleValueDataMember
		{
			get
			{
				return base.BaseMultipleValuesDataMember;
			}
			set
			{
				base.BaseMultipleValuesDataMember = value;
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the checkbox selector is shown inside the cells of the column.")]
		public bool ShowRowSelector
		{
			get
			{
				return _showRowSelector;
			}
			set
			{
				if (_showRowSelector != value)
				{
					_showRowSelector = value;
					if (DataGrid != null)
					{
						NotifyCollectionLayoutChanged();
					}
				}
			}
		}

		[DefaultValue(ColumnAutoSizeMode.Default)]
		[Description("Determines how the size of the column is calculated when AutoSize method is called or when the user double clicks the header border.")]
		public ColumnAutoSizeMode AutoSizeMode
		{
			get
			{
				return _autoSizeMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ColumnAutoSizeMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_autoSizeMode != value)
				{
					_autoSizeMode = value;
				}
			}
		}

		[Description("Determines whether AutoComplete functionality is used in a Combo or MultiColumnCombo column.")]
		[DefaultValue(true)]
		public bool AutoComplete
		{
			get
			{
				return _autoComplete;
			}
			set
			{
				_autoComplete = value;
			}
		}

		[DefaultValue(FilterConditionOperator.Default)]
		public FilterConditionOperator DefaultFilterRowComparison
		{
			get
			{
				return _defaultFilterRowComparison;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FilterConditionOperator), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_defaultFilterRowComparison != value)
				{
					_defaultFilterRowComparison = value;
					if (DataGrid != null)
					{
						DataGrid.RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		public ConditionOperator FilterRowComparison
		{
			get
			{
				if (_filterRowComparison.HasValue)
				{
					return _filterRowComparison.Value;
				}
				return ComputeConditionOperator();
			}
			set
			{
				if (!Enum.IsDefined(typeof(ConditionOperator), value))
				{
					throw new InvalidEnumArgumentException();
				}
				switch (value)
				{
				case ConditionOperator.Between:
				case ConditionOperator.NotBetween:
				case ConditionOperator.IsNull:
				case ConditionOperator.NotIsNull:
				case ConditionOperator.In:
				case ConditionOperator.NotIn:
					throw new ArgumentException(string.Format("Cannot use {0} as FilterRowComparison in a column.", value));
				}
				if (_filterRowComparison != value)
				{
					_filterRowComparison = value;
					if (DataGrid != null)
					{
						DataGrid.RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[DefaultValue(FilterRowButtonStyle.Default)]
		public FilterRowButtonStyle FilterRowButtonStyle
		{
			get
			{
				return _filterRowButtonStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(FilterRowButtonStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_filterRowButtonStyle != value)
				{
					_filterRowButtonStyle = value;
					if (DataGrid != null)
					{
						DataGrid.RefreshRowCountAndInvalidate();
					}
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Determines whether the None button is visible in the calendar dropdown.")]
		public TriState CalendarNoneButtonVisible
		{
			get
			{
				return _calendarNoneButtonVisible;
			}
			set
			{
				_calendarNoneButtonVisible = value;
			}
		}

		[Description("Determines whether the Today button is visible in the calendar dropdown.")]
		[DefaultValue(TriState.Empty)]
		public TriState CalendarTodayButtonVisible
		{
			get
			{
				return _calendarTodayButtonVisible;
			}
			set
			{
				_calendarTodayButtonVisible = value;
			}
		}
	}
}
