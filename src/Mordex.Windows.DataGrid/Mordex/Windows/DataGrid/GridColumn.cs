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
	public partial class GridColumn : MordexFieldBase, IMordexXmlLayoutsSupport, IFormatStyleOwner
	{
		private int _customGroupIndex = -1;

		private SortIndicator _sortIndicator;

		private GridCustomGroup _customGroup;

		private bool _allowGroup = true;

		private bool _allowSize = true;

		private bool _allowDrag = true;

		private ButtonStyle _buttonStyle;

		private CellButtonDisplayMode _buttonDisplayMode = CellButtonDisplayMode.CurrentCell;

		private CellButtonDisplayMode _editButtonDisplayMode;

		private string _buttonCaption = "";

		private Image _buttonImage;

		private GridFormatStyle _cellStyle;

		internal int _position = -1;

		internal int _gridColumnPosition = -1;

		private ColumnType _columnType;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private Image _columnImage;

		private EditType _editType = EditType.TextBox;

		private string _summaryFormat = "";

		private FormatMode _totalFormatMode = FormatMode.UseIFormattable;

		private string _defaultGroupPrefix;

		private string _headerImageKey = "";

		private int _headerImageIndex = -1;

		private Image _headerImage;

		private GridFormatStyle _headerStyle;

		private string _headerToolTip = "";

		private int _maxLength;

		private int _minLines = 1;

		private int _maxLines = 1;

		private bool _selectable = true;

		private bool _cardViewCaptionVisible = true;

		private object _tag;

		private GridValueListItemCollection _editValueList;

		private bool _visible = true;

		private bool _wordWrap;

		private bool _allowSort = true;

		internal int _width;

		private EditTarget _editTarget;

		internal GridColumnSet _owner;

		internal int _columnSetRow;

		internal int _columnSetColumn;

		internal int _colSpan;

		internal int _rowSpan;

		private int _leftMargin = 1;

		private int _topMargin = 1;

		private int _rightMargin = 1;

		private int _bottomMargin = 1;

		private TextAlignment _textAlignment;

		private TextAlignment _lineAlignment;

		private TextAlignment _headerAlignment;

		private TextAlignment _headerLineAlignment;

		private AggregateFunction _aggregateFunction;

		private bool _cardCaption;

		private bool _cardIcon;

		private Color _imageMaskColor = Color.Empty;

		private Size _boundImageSize = Size.Empty;

		internal int _layoutWidth;

		internal int _layoutHeight;

		private GridFormatStyle _groupRowStyle;

		private char _maskPrompt;

		private bool _includeLiterals = true;

		private bool _showInFieldChooser = true;

		private ColumnPrintMode _printMode;

		private GridDropDown _dropDown;

		private bool _checkBoxTriState;

		private object _checkBoxNullValue;

		private object _checkBoxTrueValue = true;

		private object _checkBoxFalseValue = false;

		private FilterEditType _filterEditType;

		private InheritableBoolean _hideWhenGrouped;

		private InheritableBoolean _allowRemove;

		private Trimming _trimming = Trimming.Ellipsis;

		private ColumnOwnerDrawnMode _ownerDrawnMode;

		private object _emptyTextValue = DBNull.Value;

		private object _defaultValue;

		private int _upDownMinimum;

		private int _upDownMaximum = 100;

		private int _upDownIncrement = 1;

		private bool _limitToList = true;

		private bool _actAsSelector;

		private bool _useHeaderSelector = true;

		private string _cellToolTipContent;

		private string _buttonToolTipContent;

		private CellToolTip _cellToolTip;

		private char _passwordChar;

		private CharacterCasing _characterCasing;

		private InvalidValueAction _invalidValueAction;

		private SelectableCells _gridColumnSelectableCells = SelectableCells.RowCells | SelectableCells.NewRowCells | SelectableCells.FilterRowCells;

		private RightToLeft _rightToLeft = RightToLeft.Inherit;

		private CheckBoxAlignment _checkBoxAlignment;

		private ExportValueKind _exportValueKind;

		private bool _showRowSelector;

		private ColumnAutoSizeMode _autoSizeMode = ColumnAutoSizeMode.Default;

		private bool _autoComplete = true;

		private FilterConditionOperator _defaultFilterRowComparison;

		private ConditionOperator? _filterRowComparison;

		private FilterRowButtonStyle _filterRowButtonStyle;

		private TriState _calendarNoneButtonVisible;

		private TriState _calendarTodayButtonVisible;

		public GridColumn()
		{
		}

		public GridColumn(string key)
		{
			base.BaseKey = key;
			base.BaseDisplayName = key;
			base.BaseDataMember = key;
		}

		public GridColumn(string dataMember, ColumnType columnType)
		{
			base.BaseKey = dataMember;
			base.BaseDisplayName = dataMember;
			base.BaseDataMember = dataMember;
			_columnType = columnType;
			switch (columnType)
			{
			case ColumnType.Image:
			case ColumnType.BoundImage:
			case ColumnType.Link:
				_editType = EditType.NoEdit;
				break;
			case ColumnType.CheckBox:
				_editType = EditType.CheckBox;
				break;
			default:
				_editType = EditType.TextBox;
				break;
			}
		}

		public GridColumn(string dataMember, ColumnType columnType, EditType editType)
		{
			base.BaseKey = dataMember;
			base.BaseDisplayName = dataMember;
			base.BaseDataMember = dataMember;
			_columnType = columnType;
			_editType = editType;
		}
	}
}
