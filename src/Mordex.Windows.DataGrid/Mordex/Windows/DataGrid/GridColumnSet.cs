using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public class GridColumnSet : IFormatStyleOwner, IMordexXmlLayoutsSupport
	{
		private int[] _columnWidths;

		private GridColumnSetCollection _gridColumnSetCollection;

		private string _key = "";

		private static readonly int _noIndex = -1;

		internal int _index;

		private string _caption = "";

		private static readonly int _unsetPosition = -1;

		internal int _position;

		private bool _visible = true;

		private GridFormatStyle _headerStyle;

		private int _headerImageIndex = -1;

		private string _headerImageKey = "";

		private int _columnCount;

		private GridColumn[,] _columnGrid;

		private bool _isWidthCalculated;

		private int _width;

		private int _gridColumnSetWidth = -1;

		private TextAlignment _headerAlignment;

		private TextAlignment _headerLineAlignment;

		private bool _allowSize = true;

		private bool _allowDrag = true;

		private bool _showInFieldChooser = true;

		private string _headerToolTip = "";

		internal bool _isRemoved;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue("")]
		[Description("The key used to identify an object in a colletion.")]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		[Browsable(false)]
		public GridTable Table
		{
			get
			{
				if (_gridColumnSetCollection == null)
				{
					return null;
				}
				return _gridColumnSetCollection._ownerTable;
			}
		}

		[Browsable(false)]
		public int Index => _index;

		[DefaultValue("")]
		[Description("The text displayed in the column set header.")]
		public string Caption
		{
			get
			{
				return _caption;
			}
			set
			{
				if (value != _caption)
				{
					_caption = value;
				}
			}
		}

		[Browsable(false)]
		public int Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (_gridColumnSetCollection == null)
				{
					throw new InvalidOperationException();
				}
				if (value < 0 || value >= _gridColumnSetCollection.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _position)
				{
					_gridColumnSetCollection.ShiftPositionsAfterRemove(_position);
					_gridColumnSetCollection.ShiftPositionsForInsert(value);
					_position = value;
					_gridColumnSetCollection.RecalculateColumnPositions();
					NotifyCollectionChanged();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the column set is visible or hidden.")]
		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				if (value != _visible)
				{
					_visible = value;
					if (GetOwnerCollection() != null)
					{
						GetOwnerCollection().NotifyVisibilityChanged(this);
					}
					NotifyCollectionChanged();
					RequestAutoResize();
				}
			}
		}

		[Editor("Mordex.Windows.DataGrid.Design.FormatStylePropertyEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("Format settings used to draw the header of the column.")]
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
					if (ComputeDataGrid() == null || ComputeDataGrid() != value.DataGrid)
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
				NotifyCollectionChanged();
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.ImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[Description("The image list index value of the image displayed on the header.")]
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
					NotifyCollectionChanged();
				}
			}
		}

		[Description("The key of the image displayed on the header.")]
		[Editor("Mordex.Windows.DataGrid.Design.ImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue("")]
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
					NotifyCollectionChanged();
				}
			}
		}

		[DefaultValue(2)]
		[Description("The number of layout columns in the column set.")]
		public int ColumnCount
		{
			get
			{
				return _columnCount;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _columnCount)
				{
					int[] array = new int[value];
					for (int i = _columnCount; i < value; i++)
					{
						array[i] = 100;
					}
					for (int j = 0; j < _columnCount && j < value; j++)
					{
						array[j] = _columnWidths[j];
					}
					_columnWidths = array;
					_columnCount = value;
					RebuildColumnGrid();
					_gridColumnSetWidth = -1;
					RecalculateWidth();
					NotifyCollectionChanged();
				}
			}
		}

		[Description("The width of the column set in pixels.")]
		public int Width
		{
			get
			{
				if (!_isWidthCalculated)
				{
					RecalculateWidth();
				}
				return _width;
			}
			set
			{
				if (value < 0 && value != -1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _gridColumnSetWidth || value == -1)
				{
					_gridColumnSetWidth = value;
					_isWidthCalculated = false;
					RecalculateWidth();
					NotifyCollectionChanged();
					RequestAutoResize();
				}
			}
		}

		public GridColumn this[int row, int column]
		{
			get
			{
				if (_columnGrid == null)
				{
					throw new ArgumentOutOfRangeException();
				}
				return _columnGrid[row, column];
			}
		}

		[DefaultValue(TextAlignment.Empty)]
		[Description("Determines how text is aligned in the column set header.")]
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
					NotifyCollectionChanged();
				}
			}
		}

		[DefaultValue(TextAlignment.Empty)]
		[Description("Determines how the line(s) of text are aligned in the header.")]
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
					NotifyCollectionChanged();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the columnset can be sized by the user.")]
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

		[Description("Indicates whether the column set can be dragged by the user.")]
		[DefaultValue(true)]
		public bool AllowDrag
		{
			get
			{
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

		[DefaultValue(true)]
		[Description("Indicates whether the column set should be shown in the field chooser window.")]
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
					if (GetOwnerCollection() != null)
					{
						GetOwnerCollection().NotifyVisibilityChanged(this);
					}
				}
			}
		}

		[Description("The text to display in the tool tip for the header.")]
		[DefaultValue("")]
		public string HeaderToolTip
		{
			get
			{
				return _headerToolTip;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_headerToolTip = value;
			}
		}

		MxDataGrid IFormatStyleOwner.DataGrid => ComputeDataGrid();

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			_allowSize = (bool)LayoutReader.GetPropertyValue(info, "AllowSize", typeof(bool), true);
			_allowDrag = (bool)LayoutReader.GetPropertyValue(info, "AllowDrag", typeof(bool), true);
			_caption = (string)LayoutReader.GetPropertyValue(info, "Caption", typeof(string), string.Empty);
			_headerToolTip = (string)LayoutReader.GetPropertyValue(info, "HeaderToolTip", typeof(string), string.Empty);
			_headerAlignment = (TextAlignment)LayoutReader.GetPropertyValue(info, "HeaderAlignment", typeof(TextAlignment), TextAlignment.Empty);
			_headerImageIndex = (int)LayoutReader.GetPropertyValue(info, "HeaderImageIndex", typeof(int), -1);
			_headerImageKey = (string)LayoutReader.GetPropertyValue(info, "HeaderImageKey", typeof(string), string.Empty);
			_headerLineAlignment = (TextAlignment)LayoutReader.GetPropertyValue(info, "HeaderLineAlignment", typeof(TextAlignment), TextAlignment.Empty);
			_position = (int)LayoutReader.GetPropertyValue(info, "Position", typeof(int), -1);
			_showInFieldChooser = (bool)LayoutReader.GetPropertyValue(info, "ShowInFieldChooser", typeof(bool), true);
			_visible = (bool)LayoutReader.GetPropertyValue(info, "Visible", typeof(bool), true);
			_columnCount = (int)LayoutReader.GetPropertyValue(info, "ColumnCount", typeof(int), 2);
			_columnWidths = new int[_columnCount];
			HeaderStyle = null;
			PropertyValue propertyValue = info["HeaderStyle"];
			if (propertyValue == null)
			{
				if (ComputeDataGrid() != null)
				{
					int otherPropertyValue = (int)LayoutReader.GetPropertyValue(info, "HeaderStyleIndex", typeof(int), -1);
					if (otherPropertyValue >= 0 && otherPropertyValue < ComputeDataGrid().FormatStyles.Count)
					{
						HeaderStyle = ComputeDataGrid().FormatStyles[otherPropertyValue];
					}
				}
			}
			else
			{
				propertyValue.DeserializeObject(_headerStyle);
			}
			for (int i = 0; i < _columnWidths.Length; i++)
			{
				_columnWidths[i] = (int)LayoutReader.GetPropertyValue(info, "ColumnWidth" + i, typeof(int), 100);
			}
			RebuildColumnGrid();
			if (Table == null)
			{
				return;
			}
			int nextPropertyValue = (int)LayoutReader.GetPropertyValue(info, "EntriesCount", typeof(int), 0);
			for (int j = 0; j < nextPropertyValue; j++)
			{
				int previousPropertyValue = (int)LayoutReader.GetPropertyValue(info, "ColIndex" + j, typeof(int), -1);
				if (previousPropertyValue >= 0 && previousPropertyValue < Table.Columns.Count)
				{
					int row = (int)LayoutReader.GetPropertyValue(info, "Row" + j, typeof(int), -1);
					int col = (int)LayoutReader.GetPropertyValue(info, "Col" + j, typeof(int), -1);
					int rowSpan = (int)LayoutReader.GetPropertyValue(info, "RowSpan" + j, typeof(int), -1);
					int colSpan = (int)LayoutReader.GetPropertyValue(info, "ColSpan" + j, typeof(int), -1);
					try
					{
						Add(Table.Columns[previousPropertyValue], row, col, rowSpan, colSpan);
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
			}
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_key != string.Empty)
			{
				info.AddValue("Key", _key);
			}
			if (!_allowSize)
			{
				info.AddValue("AllowSize", _allowSize);
			}
			if (!_allowDrag)
			{
				info.AddValue("AllowDrag", _allowDrag);
			}
			if (_caption != string.Empty)
			{
				info.AddValue("Caption", _caption);
			}
			if (_columnCount != 2)
			{
				info.AddValue("ColumnCount", _columnCount);
			}
			if (_headerAlignment != TextAlignment.Empty)
			{
				info.AddValue("HeaderAlignment", _headerAlignment);
			}
			if (_headerImageIndex != -1)
			{
				info.AddValue("HeaderImageIndex", _headerImageIndex);
			}
			if (_headerImageKey.Length > 0)
			{
				info.AddValue("HeaderImageKey", _headerImageKey);
			}
			if (_headerLineAlignment != TextAlignment.Empty)
			{
				info.AddValue("HeaderLineAlignment", _headerLineAlignment);
			}
			if (_position != -1)
			{
				info.AddValue("Position", _position);
			}
			if (!_showInFieldChooser)
			{
				info.AddValue("ShowInFieldChooser", _showInFieldChooser);
			}
			if (!_visible)
			{
				info.AddValue("Visible", _visible);
			}
			if (_headerToolTip != "")
			{
				info.AddValue("HeaderToolTip", _headerToolTip);
			}
			if (_headerStyle != null)
			{
				if (_headerStyle._owner == this)
				{
					if (!_headerStyle.IsDefaultStyle())
					{
						info.AddValue("HeaderStyle", _headerStyle);
					}
				}
				else if (ComputeDataGrid() != null)
				{
					int nextValue = ComputeDataGrid().FormatStyles.IndexOf(HeaderStyle);
					info.AddValue("HeaderStyleIndex", nextValue);
				}
			}
			for (int i = 0; i < _columnCount; i++)
			{
				info.AddValue("ColumnWidth" + i, _columnWidths[i]);
			}
			GridTable table = Table;
			if (table == null)
			{
				return;
			}
			int value = 0;
			foreach (GridColumn column in Table.Columns)
			{
				if (column.ColumnSet == this)
				{
					value++;
				}
			}
			if (value <= 0)
			{
				return;
			}
			info.AddValue("EntriesCount", value);
			int otherValue = 0;
			foreach (GridColumn otherColumn in Table.Columns)
			{
				if (otherColumn.ColumnSet == this)
				{
					info.AddValue("ColIndex" + otherValue, otherColumn.Index);
					info.AddValue("Row" + otherValue, otherColumn._columnSetRow);
					info.AddValue("Col" + otherValue, otherColumn._columnSetColumn);
					info.AddValue("RowSpan" + otherValue, otherColumn._rowSpan);
					info.AddValue("ColSpan" + otherValue, otherColumn._colSpan);
					otherValue++;
				}
			}
		}

		public GridColumnSet()
		{
			_index = _noIndex;
			_position = _unsetPosition;
			_columnCount = 2;
			_columnWidths = new int[2] { 100, 100 };
		}

		internal GridColumnSetCollection GetOwnerCollection()
		{
			return _gridColumnSetCollection;
		}

		internal void SetOwnerCollection(GridColumnSetCollection gridColumnSetCollection)
		{
			if (gridColumnSetCollection != _gridColumnSetCollection)
			{
				_gridColumnSetCollection = gridColumnSetCollection;
				RebuildOrClearColumnGrid();
				if (_gridColumnSetCollection == null)
				{
					_index = _noIndex;
				}
			}
		}

		internal MxDataGrid ComputeDataGrid()
		{
			if (_gridColumnSetCollection == null)
			{
				return null;
			}
			return _gridColumnSetCollection.ComputeDataGrid();
		}

		internal void NotifyCollectionChanged()
		{
			if (_gridColumnSetCollection != null)
			{
				_gridColumnSetCollection.NotifyTableLayoutChanged();
			}
		}

		private void RequestAutoResize()
		{
			if (ComputeDataGrid() != null && ComputeDataGrid().ColumnAutoResize)
			{
				Table._isAutoResizePending = true;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderStyle()
		{
			HeaderStyle = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderStyle()
		{
			if (_headerStyle == null)
			{
				return false;
			}
			if (_headerStyle._owner == this && _headerStyle.IsDefaultStyle())
			{
				return false;
			}
			return true;
		}

		private void RebuildColumnGrid()
		{
			try
			{
				if (Table == null)
				{
					Clear();
					_columnGrid = null;
					return;
				}
				int columnSetRowCount = Table.ColumnSetRowCount;
				GridColumn[,] array = new GridColumn[columnSetRowCount, _columnCount];
				if (_columnGrid != null)
				{
					ArrayList arrayList = new ArrayList();
					for (int i = 0; i < _columnGrid.GetLength(0); i++)
					{
						for (int j = 0; j < _columnGrid.GetLength(1); j++)
						{
							if (_columnGrid[i, j] != null)
							{
								arrayList.Add(_columnGrid[i, j]);
								_columnGrid[i, j]._owner = null;
							}
						}
					}
					int length = _columnGrid.GetLength(1);
					int otherColumnSetRowCount = _columnGrid.GetLength(0);
					if (length > _columnCount)
					{
						length = _columnCount;
					}
					if (otherColumnSetRowCount > columnSetRowCount)
					{
						otherColumnSetRowCount = columnSetRowCount;
					}
					for (int k = 0; k < length; k++)
					{
						for (int value = 0; value < otherColumnSetRowCount; value++)
						{
							array[value, k] = _columnGrid[value, k];
						}
					}
					foreach (GridColumn item in arrayList)
					{
						if (item._columnSetRow < otherColumnSetRowCount && item._columnSetColumn < length)
						{
							if (item._columnSetRow + item._rowSpan > Table.ColumnSetRowCount)
							{
								item._rowSpan = Table.ColumnSetRowCount - item._columnSetRow;
							}
							if (item._columnSetColumn + item._colSpan > _columnCount)
							{
								item._colSpan = _columnCount - item._columnSetColumn;
							}
							item._owner = this;
						}
					}
					foreach (GridColumn arrayListItem in arrayList)
					{
						if (arrayListItem._owner == null)
						{
							arrayListItem.ClearColumnSetOwner();
						}
						else
						{
							arrayListItem._width = SumColumnWidths(arrayListItem._columnSetColumn, arrayListItem._colSpan);
						}
					}
					arrayList.Clear();
					arrayList = null;
				}
				_columnGrid = array;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private int SumColumnWidths(int value, int otherValue)
		{
			int sumColumnWidths = 0;
			if (_columnWidths != null)
			{
				for (int i = value; i < value + otherValue; i++)
				{
					sumColumnWidths += _columnWidths[i];
				}
			}
			return sumColumnWidths;
		}

		public int GetColumnWidth(int column)
		{
			return _columnWidths[column];
		}

		public void SetColumnWidth(int column, int width)
		{
			ApplyColumnWidth(column, width);
			_gridColumnSetWidth = -1;
			RecalculateWidth();
			RequestAutoResize();
		}

		private void ApplyColumnWidth(int value, int columnWidth)
		{
			if (Table == null)
			{
				return;
			}
			int width = columnWidth - _columnWidths[value];
			for (int i = 0; i < Table.ColumnSetRowCount; i++)
			{
				if (_columnGrid[i, value] != null && _columnGrid[i, value]._columnSetRow == i)
				{
					_columnGrid[i, value]._width += width;
				}
			}
			_columnWidths[value] = columnWidth;
			RequestAutoResize();
		}

		private void PlaceColumn(GridColumn gridColumn, int columnSetRow, int columnSetColumn, int rowSpan, int colSpan)
		{
			for (int i = columnSetRow; i < columnSetRow + rowSpan; i++)
			{
				for (int j = columnSetColumn; j < columnSetColumn + colSpan; j++)
				{
					_columnGrid[i, j] = gridColumn;
				}
			}
			gridColumn._owner = this;
			if (gridColumn.Table != null && gridColumn.Table.CellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				gridColumn._width = SumColumnWidths(columnSetColumn, colSpan);
			}
			gridColumn._columnSetRow = columnSetRow;
			gridColumn._columnSetColumn = columnSetColumn;
			gridColumn._rowSpan = rowSpan;
			gridColumn._colSpan = colSpan;
			if (_gridColumnSetCollection != null)
			{
				GetOwnerCollection().RecalculateColumnPositions();
			}
		}

		private void RebuildOrClearColumnGrid()
		{
			GridTable table = Table;
			if (table == null)
			{
				Clear();
			}
			else
			{
				RebuildColumnGrid();
			}
		}

		internal void RebuildForRowCountChange()
		{
			RebuildColumnGrid();
		}

		public void Add(GridColumn column, int row, int col)
		{
			Add(column, row, col, 1, 1);
		}

		public void Add(GridColumn column, int row, int col, int rowSpan, int colSpan)
		{
			if (column == null)
			{
				throw new ArgumentNullException();
			}
			if (Table == null || column.Table != Table)
			{
				throw new InvalidOperationException();
			}
			if (column._owner != null)
			{
				column._owner.Remove(column);
			}
			if (row > Table.ColumnSetRowCount - 1 || rowSpan < 1)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (row + rowSpan > Table.ColumnSetRowCount)
			{
				rowSpan = Table.ColumnSetRowCount - row;
			}
			if (col > _columnCount - 1 || colSpan < 1)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (col + colSpan > _columnCount)
			{
				colSpan = _columnCount - col;
			}
			PlaceColumn(column, row, col, rowSpan, colSpan);
			_isWidthCalculated = false;
			NotifyCollectionChanged();
		}

		public void Remove(GridColumn column)
		{
			if (column == null)
			{
				throw new ArgumentNullException();
			}
			if (_columnGrid == null || column._owner != this)
			{
				throw new InvalidOperationException();
			}
			for (int i = column._columnSetRow; i < column._columnSetRow + column._rowSpan; i++)
			{
				for (int j = column._columnSetColumn; j < column._columnSetColumn + column._colSpan; j++)
				{
					_columnGrid[i, j] = null;
				}
			}
			column.ClearColumnSetOwner();
			GetOwnerCollection().RecalculateColumnPositions();
			NotifyCollectionChanged();
		}

		public void Clear()
		{
			if (_columnGrid != null)
			{
				for (int i = 0; i < _columnGrid.GetLength(0); i++)
				{
					for (int j = 0; j < _columnGrid.GetLength(1); j++)
					{
						if (_columnGrid[i, j] != null)
						{
							_columnGrid[i, j].ClearColumnSetOwner();
							_columnGrid[i, j] = null;
						}
					}
				}
			}
			if (_gridColumnSetCollection != null)
			{
				GetOwnerCollection().RecalculateColumnPositions();
			}
		}

		internal void ApplyWidthsToColumns()
		{
			if (_columnGrid == null)
			{
				return;
			}
			for (int i = 0; i < _columnGrid.GetLength(0); i++)
			{
				for (int j = 0; j < _columnGrid.GetLength(1); j++)
				{
					if (_columnGrid[i, j] != null)
					{
						_columnGrid[i, j]._width = SumColumnWidths(_columnGrid[i, j]._columnSetColumn, _columnGrid[i, j]._colSpan);
					}
				}
			}
		}

		internal void RecalculateWidth()
		{
			_isWidthCalculated = true;
			int sumColumnWidths = 0;
			if (_columnGrid != null)
			{
				if (_gridColumnSetWidth == -1)
				{
					sumColumnWidths = SumColumnWidths(0, _columnCount);
				}
				else
				{
					if (_gridColumnSetWidth < _columnCount * 8)
					{
						_gridColumnSetWidth = _columnCount * 8;
					}
					int[] array = new int[_columnCount];
					for (int i = 0; i < _columnCount; i++)
					{
						array[i] = _columnWidths[i];
					}
					int otherSumColumnWidths = SumColumnWidths(0, _columnCount);
					int value = 0;
					int otherValue = _gridColumnSetWidth - otherSumColumnWidths;
					for (int j = 0; j < _columnCount; j++)
					{
						int arrayItem = (int)((float)otherValue * (float)array[j] / (float)otherSumColumnWidths);
						if (array[j] + arrayItem < 8)
						{
							arrayItem = 8 - array[j];
						}
						array[j] += arrayItem;
						value += arrayItem;
					}
					array[_columnCount - 1] += otherValue - value;
					for (int k = 0; k < _columnCount; k++)
					{
						ApplyColumnWidth(k, array[k]);
					}
					sumColumnWidths = _gridColumnSetWidth;
				}
			}
			_width = sumColumnWidths;
		}

		internal TextLayoutRenderer ComputeTextLayoutRenderer(GridFormatStyle gridFormatStyle, bool useTextRenderer)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer);
			if (_headerAlignment == TextAlignment.Empty)
			{
				textLayoutRenderer.SetStringAlignment(GridHelper.ToStringAlignmentDefaultNear(gridFormatStyle.TextAlignment));
			}
			else
			{
				textLayoutRenderer.SetStringAlignment(GridHelper.ToStringAlignmentDefaultNear(_headerAlignment));
			}
			textLayoutRenderer.UpdateTrimming(StringTrimming.EllipsisCharacter);
			if (_headerLineAlignment == TextAlignment.Empty)
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(GridHelper.ToStringAlignmentDefaultCenter(gridFormatStyle.LineAlignment));
			}
			else
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(GridHelper.ToStringAlignmentDefaultCenter(_headerLineAlignment));
			}
			if (Table != null && Table.ColumnSetHeaderLines > 1)
			{
				textLayoutRenderer.SetWordWrap(true);
			}
			return textLayoutRenderer;
		}

		internal void UpdateHeaderStyle()
		{
			if (!GridHelper.IsStyleAttached(_headerStyle))
			{
				_headerStyle = null;
			}
		}

		void IFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			NotifyCollectionChanged();
		}
	}
}
