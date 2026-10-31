using Mordex.Windows.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Design;

namespace Mordex.Windows.DataGrid.Internal
{
	[ToolboxItem(false)]
	public class ColumnList : ScrollableBase
	{
		private MxDataGrid _columnListDataGrid;

		private ColumnsetDesigner _columnsetDesigner;

		private int _rows = 8;

		private bool _isColumnMode;

		private GridTable _activeTable;

		private int _defaultItemHeight = 16;

		private int _columnCount;

		private MxRowLayoutState _columnListMordexRowLayoutState;

		private int _firstColumn;

		private GridGroupLayoutState _columnListGridGroupLayoutState;

		private GridGroupLayoutState _hotItem;

		private bool _canStartDrag = true;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int Rows
		{
			get
			{
				return _rows;
			}
			set
			{
				if (_rows != value)
				{
					_rows = value;
					RebuildLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public GridTable CurrentTable
		{
			get
			{
				return _activeTable;
			}
			set
			{
				if (_activeTable != value)
				{
					_activeTable = value;
					_firstColumn = 0;
					RebuildLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int FirstColumn
		{
			get
			{
				return _firstColumn;
			}
			set
			{
				if (value > _columnCount - _rows)
				{
					value = _columnCount - _rows;
				}
				if (value < 0)
				{
					value = 0;
				}
				if (value != _firstColumn)
				{
					_firstColumn = value;
					RebuildLayout();
					GetVScroll().SetScrollValue(_firstColumn);
					Invalidate();
				}
			}
		}

		public ColumnList()
		{
			AllowDrop = true;
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		internal void UpdateColumnListDataGrid(MxDataGrid columnListDataGrid)
		{
			if (columnListDataGrid == _columnListDataGrid)
			{
				return;
			}
			_columnListDataGrid = columnListDataGrid;
			if (_columnListDataGrid != null)
			{
				if (_columnListDataGrid is GridDropDown gridDropDown)
				{
					if (gridDropDown._ownerCollection != null && gridDropDown._ownerCollection.DataGrid != null)
					{
						AssignControlStyle(gridDropDown._ownerCollection.DataGrid._gridControlStyle);
					}
					else
					{
						AssignControlStyle(new ControlStyle(this));
					}
				}
				else
				{
					AssignControlStyle(_columnListDataGrid.ControlStyle);
				}
				RebuildLayout();
			}
			else
			{
				AssignControlStyle(null);
				RebuildLayout();
			}
		}

		internal void SetColumnsetDesigner(ColumnsetDesigner columnsetDesigner)
		{
			_columnsetDesigner = columnsetDesigner;
		}

		internal void UpdateColumnListBoolean(bool columnListBoolean)
		{
			if (_isColumnMode != columnListBoolean)
			{
				_isColumnMode = columnListBoolean;
				RebuildLayout();
			}
		}

		internal void RebuildLayout()
		{
			_columnCount = 0;
			if (_columnListDataGrid != null)
			{
				if (_isColumnMode)
				{
					if (_activeTable != null)
					{
						foreach (GridColumn otherColumn in _activeTable.Columns)
						{
							if (otherColumn.ColumnSet == null)
							{
								_columnCount++;
							}
						}
						GridColumn[] array = new GridColumn[_columnCount];
						int value = 0;
						foreach (GridColumn nextColumn in _activeTable.Columns)
						{
							if (nextColumn.ColumnSet == null)
							{
								array[value] = nextColumn;
								value++;
							}
						}
						GridFormatStyle gridFormatStyle = _columnListDataGrid.GetHeaderStyle(_activeTable);
						switch (gridFormatStyle.Appearance)
						{
						case Appearance.Sunken:
						case Appearance.Raised:
							base.BorderStyle = BorderStyle.Sunken;
							break;
						case Appearance.SunkenLight:
						case Appearance.RaisedLight:
							base.BorderStyle = BorderStyle.SunkenLight3D;
							break;
						default:
							base.BorderStyle = BorderStyle.Flat;
							break;
						}
						_defaultItemHeight = CalculateListHeight();
						_columnListMordexRowLayoutState = new MxRowLayoutState(LayoutRowType.ColumnHeader, _activeTable);
						int top = base.ClientRectangle.Top;
						int width = base.ClientRectangle.Width;
						for (int i = _firstColumn; i < array.Length; i++)
						{
							GridColumn column = array[i];
							GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnHeader, _columnListMordexRowLayoutState, column);
							gridGroupLayoutState._rect = new Rectangle(0, top, width, _defaultItemHeight);
							top += _defaultItemHeight;
							_columnListMordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
						}
					}
				}
				else
				{
					if (_activeTable == null)
					{
						_activeTable = _columnListDataGrid.RootTable;
					}
					if (_activeTable != null)
					{
						if (_activeTable.CellLayoutMode == CellLayoutMode.UseColumns)
						{
							foreach (GridColumn previousColumn in _activeTable.Columns)
							{
								if (!previousColumn.Visible && previousColumn.ShowInFieldChooser && (!previousColumn.ShouldHideWhenGrouped() || previousColumn.Group == null))
								{
									_columnCount++;
								}
							}
							GridColumn[] columns = new GridColumn[_columnCount];
							int otherValue = 0;
							foreach (GridColumn firstColumn in _activeTable.Columns)
							{
								if (!firstColumn.Visible && firstColumn.ShowInFieldChooser && (!firstColumn.ShouldHideWhenGrouped() || firstColumn.Group == null))
								{
									columns[otherValue] = firstColumn;
									otherValue++;
								}
							}
							GridFormatStyle headerStyle = _columnListDataGrid.GetHeaderStyle(_activeTable);
							switch (headerStyle.Appearance)
							{
							case Appearance.Sunken:
							case Appearance.Raised:
								base.BorderStyle = BorderStyle.Sunken;
								break;
							case Appearance.SunkenLight:
							case Appearance.RaisedLight:
								base.BorderStyle = BorderStyle.SunkenLight3D;
								break;
							default:
								base.BorderStyle = BorderStyle.Flat;
								break;
							}
							_defaultItemHeight = CalculateListHeight();
							_columnListMordexRowLayoutState = new MxRowLayoutState(LayoutRowType.ColumnHeader, _activeTable);
							int otherTop = base.ClientRectangle.Top;
							int otherWidth = base.ClientRectangle.Width;
							for (int j = _firstColumn; j < columns.Length; j++)
							{
								GridColumn secondColumn = columns[j];
								GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnHeader, _columnListMordexRowLayoutState, secondColumn);
								groupLayoutState._rect = new Rectangle(0, otherTop, otherWidth, _defaultItemHeight);
								otherTop += _defaultItemHeight;
								_columnListMordexRowLayoutState._arrayList.Add(groupLayoutState);
							}
						}
						else
						{
							foreach (GridColumnSet columnSet in _activeTable.ColumnSets)
							{
								if (!columnSet.Visible && columnSet.ShowInFieldChooser)
								{
									_columnCount++;
								}
							}
							GridColumnSet[] columnSets = new GridColumnSet[_columnCount];
							int nextValue = 0;
							foreach (GridColumnSet otherColumnSet in _activeTable.ColumnSets)
							{
								if (!otherColumnSet.Visible && otherColumnSet.ShowInFieldChooser)
								{
									columnSets[nextValue] = otherColumnSet;
									nextValue++;
								}
							}
							GridFormatStyle otherHeaderStyle = _columnListDataGrid.GetHeaderStyle(_activeTable);
							switch (otherHeaderStyle.Appearance)
							{
							case Appearance.Sunken:
							case Appearance.Raised:
								base.BorderStyle = BorderStyle.Sunken;
								break;
							case Appearance.SunkenLight:
							case Appearance.RaisedLight:
								base.BorderStyle = BorderStyle.SunkenLight3D;
								break;
							default:
								base.BorderStyle = BorderStyle.Flat;
								break;
							}
							_defaultItemHeight = CalculateListHeight();
							_columnListMordexRowLayoutState = new MxRowLayoutState(LayoutRowType.ColumnHeader, _activeTable);
							int nextTop = base.ClientRectangle.Top;
							int nextWidth = base.ClientRectangle.Width;
							for (int k = _firstColumn; k < columnSets.Length; k++)
							{
								GridColumnSet gridColumnSet = columnSets[k];
								GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnSetHeader, _columnListMordexRowLayoutState, null);
								otherGroupLayoutState._columnSet = gridColumnSet;
								otherGroupLayoutState._rect = new Rectangle(0, nextTop, nextWidth, _defaultItemHeight);
								nextTop += _defaultItemHeight;
								_columnListMordexRowLayoutState._arrayList.Add(otherGroupLayoutState);
							}
						}
					}
				}
				if (_columnListMordexRowLayoutState != null && _rows >= 0)
				{
					_columnListMordexRowLayoutState.SetLocation(new Point(_borderWidth, _borderWidth));
					int previousValue = _borderWidth * 2;
					base.Height = previousValue + _defaultItemHeight * _rows;
					ConfigureVerticalScrollBar(0, _columnCount - 1, _rows - 1, _firstColumn);
					base.SetVerticalScrollRequired(_columnCount > _rows);
				}
				Invalidate();
			}
			else
			{
				_columnListMordexRowLayoutState = null;
			}
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			switch (scrollCode)
			{
			case 0:
			case 1:
				FirstColumn = GetVScroll().GetScrollValue();
				break;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.FillRectangle(_gridControlStyle.GetWindowBrush(), base.ClientRectangle);
			if (_columnListDataGrid == null || _activeTable == null)
			{
				return;
			}
			_columnListDataGrid.PrepareControlStyleLayout();
			GridFormatStyle gridFormatStyle = _columnListDataGrid.GetHeaderStyle(_activeTable);
			foreach (GridGroupLayoutState item in _columnListMordexRowLayoutState._arrayList)
			{
				_columnListDataGrid.DrawLayoutItem(e.Graphics, item, gridFormatStyle, item._rect, item._rect);
			}
			gridFormatStyle.DisposeResources();
			DrawControlBorder(e.Graphics);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			RebuildLayout();
		}

		protected override void OnResize(EventArgs e)
		{
			RebuildLayout();
			base.OnResize(e);
		}

		private int CalculateListHeight()
		{
			if (base.IsDisposed || base.Disposing)
			{
				return 0;
			}
			GridFormatStyle gridFormatStyle = _columnListDataGrid.GetHeaderStyle(_activeTable);
			Graphics graphics = CreateGraphics();
			int listHeight = GridHelper.RoundToInt(gridFormatStyle.ResolveFont(_columnListDataGrid.Font).GetHeight(graphics)) * _activeTable.HeaderLines + 2;
			if (listHeight < _columnListDataGrid.GetImageSize().Height + 2)
			{
				listHeight = _columnListDataGrid.GetImageSize().Height + 2;
			}
			listHeight += 2;
			graphics.Dispose();
			graphics = null;
			return listHeight;
		}

		private void UpdateColumnListGridGroupLayoutState(GridGroupLayoutState columnListGridGroupLayoutState)
		{
			if (columnListGridGroupLayoutState != _columnListGridGroupLayoutState)
			{
				if (_columnListGridGroupLayoutState != null)
				{
					_columnListGridGroupLayoutState._isPressed = false;
				}
				_columnListGridGroupLayoutState = columnListGridGroupLayoutState;
				if (_columnListGridGroupLayoutState != null)
				{
					SetHotItem(null);
					_columnListGridGroupLayoutState._isPressed = true;
				}
				Invalidate();
			}
		}

		private void SetHotItem(GridGroupLayoutState hotItem)
		{
			if (hotItem != _hotItem)
			{
				if (_hotItem != null)
				{
					_hotItem._isHot = false;
				}
				_hotItem = hotItem;
				if (_hotItem != null)
				{
					_hotItem._isHot = true;
				}
				Invalidate();
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			Point position = PointerPosition.Current;
			position = PointToClient(position);
			if (_columnListMordexRowLayoutState == null || _columnListDataGrid == null)
			{
				return;
			}
			foreach (GridGroupLayoutState item in _columnListMordexRowLayoutState._arrayList)
			{
				if (!item.GetAbsoluteItemRect().Contains(position.X, position.Y))
				{
					continue;
				}
				if (!_isColumnMode)
				{
					if (item._column != null)
					{
						item._column.Visible = true;
						_columnListDataGrid.EnsureVisible(_columnListDataGrid.Row, item._column);
					}
					else if (item._columnSet != null)
					{
						item._columnSet.Visible = true;
					}
					RebuildLayout();
					Update();
				}
				break;
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Clicks == 2)
			{
				_canStartDrag = true;
			}
			else
			{
				if (_columnListMordexRowLayoutState == null || _columnListDataGrid == null)
				{
					return;
				}
				foreach (GridGroupLayoutState item in _columnListMordexRowLayoutState._arrayList)
				{
					Rectangle rectangle = item.GetAbsoluteItemRect();
					if (rectangle.Contains(e.X, e.Y))
					{
						UpdateColumnListGridGroupLayoutState(item);
						Update();
						if (_isColumnMode)
						{
							_columnsetDesigner.BeginHeaderDrag(item, e.X - rectangle.X, e.Y - rectangle.Y);
						}
						else
						{
							_columnListDataGrid.StartLayoutItemDrag(item, e.X - rectangle.X, e.Y - rectangle.Y);
						}
						break;
					}
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			GridGroupLayoutState gridGroupLayoutState = null;
			if (_columnListMordexRowLayoutState != null && _columnListDataGrid != null)
			{
				foreach (GridGroupLayoutState item in _columnListMordexRowLayoutState._arrayList)
				{
					if (item.GetAbsoluteItemRect().Contains(e.X, e.Y))
					{
						gridGroupLayoutState = item;
						break;
					}
				}
			}
			SetHotItem(gridGroupLayoutState);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			UpdateColumnListGridGroupLayoutState(null);
			if (_canStartDrag)
			{
				_canStartDrag = false;
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHotItem(null);
		}

		internal void UpdateColumnList()
		{
			UpdateColumnListGridGroupLayoutState(null);
		}
	}
}
