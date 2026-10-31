using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class MxFieldChooser : Control, IRefreshListener, IGridStyleSource
	{
		private MxDataGrid _dataGrid;

		private GridTable _sourceTable;

		private HeaderOrder _headerOrder;

		private ScrollBarControl _scrollBar;

		private GridFormatStyle _gridFormatStyle;

		private GridGroupLayoutState _gridGroupLayoutState;

		private GridGroupLayoutState _gridFieldChooserControlGridGroupLayoutState;

		private Rectangle _dragThresholdBounds;

		private int _headerHeight;

		private MxRowLayoutState _mordexRowLayoutState;

		private ControlStyle _chooserControlStyle;

		private int _firstVisibleIndex;

		private BorderStyle _borderStyle;

		private bool _isScrollBarVisible;

		private int _borderWidth;

		private Rectangle _contentBounds;


		public new Color BackColor
		{
			get
			{
				if (base.BackColor == SystemColors.Window)
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.InputBack);
				}
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		[DefaultValue(HeaderOrder.Alphabetical)]
		public HeaderOrder HeaderOrder
		{
			get
			{
				return _headerOrder;
			}
			set
			{
				if (!Enum.IsDefined(typeof(HeaderOrder), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_headerOrder != value)
				{
					_headerOrder = value;
					RebuildLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridFormatStyle HeaderStyle => _gridFormatStyle;

		[DefaultValue(null)]
		public MxDataGrid DataGrid
		{
			get
			{
				return _dataGrid;
			}
			set
			{
				if (_dataGrid != value)
				{
					if (_dataGrid != null)
					{
						((IDragHost)_dataGrid).RemoveListener((IRefreshListener)this);
						_dataGrid.Disposed -= OnGridDisposed;
					}
					_dataGrid = value;
					if (_dataGrid != null)
					{
						((IDragHost)_dataGrid).AddListener((IRefreshListener)this);
						_dataGrid.Disposed += OnGridDisposed;
					}
					if (_sourceTable != null && _sourceTable.DataGrid != _dataGrid)
					{
						Table = null;
					}
					_scrollBar._style = ControlStyle;
					ResetScrollPosition();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridTable Table
		{
			get
			{
				if (_sourceTable == null)
				{
					if (_dataGrid == null)
					{
						return null;
					}
					return _dataGrid.RootTable;
				}
				return _sourceTable;
			}
			set
			{
				if (value != null && _dataGrid != null && value.DataGrid != _dataGrid)
				{
					throw new InvalidOperationException("Table must belong to the DataGrid control that field chooser is associated to.");
				}
				if (_sourceTable != value)
				{
					if (_dataGrid == null && value != null)
					{
						DataGrid = value.DataGrid;
					}
					_sourceTable = value;
					ResetScrollPosition();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlStyle ControlStyle => _chooserControlStyle;

		[DefaultValue(BorderStyle.SunkenLight3D)]
		public BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_borderStyle != value)
				{
					_borderStyle = value;
					UpdateContentBounds();
				}
			}
		}

		Font IGridStyleSource.ControlStyleFont => Font;

		bool IGridStyleSource.IsRightToLeft => RightToLeft == RightToLeft.Yes;

		public MxFieldChooser()
		{
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			InitializeChooser();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopListeningToThemes();
			}
			base.Dispose(disposing);
		}

		private void InitializeChooser()
		{
			_chooserControlStyle = new ControlStyle(this);
			_scrollBar = new ScrollBarControl(ScrollBarType.Vertical);
			_scrollBar.AddValueChangedHandler(OnScrollBarValueChanged);
			_scrollBar.AddScrollHandler(OnScrollBarScroll);
			_scrollBar._style = _chooserControlStyle;
			_scrollBar.Visible = _isScrollBarVisible;
			base.Controls.Add(_scrollBar);
			_headerOrder = HeaderOrder.Alphabetical;
			_gridFormatStyle = new GridFormatStyle();
			base.BackColor = SystemColors.Window;
			_borderStyle = BorderStyle.SunkenLight3D;
		}

		private void OnScrollBarScroll(object sender, ScrollEventArgs eventArgs)
		{
			if (eventArgs.Type == ScrollEventType.ThumbTrack)
			{
				_scrollBar.SetScrollValue(eventArgs.NewValue);
				OnScrollBarValueChanged(sender, EventArgs.Empty);
			}
		}

		private void OnScrollBarValueChanged(object sender, EventArgs eventArgs)
		{
			SetFirstVisibleIndex(_scrollBar.GetScrollValue());
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return base.BackColor != SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		private GridFormatStyle BuildChooserStyle()
		{
			if (Table != null)
			{
				GridFormatStyle gridFormatStyle = _dataGrid.GetHeaderStyle(Table);
				return gridFormatStyle.ApplyOverrides(_gridFormatStyle);
			}
			return new GridFormatStyle(_gridFormatStyle);
		}

		private GridGroupLayoutState GetPressedItem()
		{
			return _gridGroupLayoutState;
		}

		private void SetPressedItem(GridGroupLayoutState gridGroupLayoutState)
		{
			if (_gridGroupLayoutState != gridGroupLayoutState)
			{
				if (_gridGroupLayoutState != null)
				{
					_gridGroupLayoutState._isPressed = false;
					Invalidate();
				}
				_gridGroupLayoutState = gridGroupLayoutState;
				if (_gridGroupLayoutState != null)
				{
					_gridGroupLayoutState._isPressed = true;
					Invalidate();
				}
			}
		}

		private void SetHotItem(GridGroupLayoutState gridFieldChooserControlGridGroupLayoutState)
		{
			if (_gridFieldChooserControlGridGroupLayoutState != gridFieldChooserControlGridGroupLayoutState)
			{
				if (_gridFieldChooserControlGridGroupLayoutState != null)
				{
					_gridFieldChooserControlGridGroupLayoutState._isHot = false;
					Invalidate();
				}
				_gridFieldChooserControlGridGroupLayoutState = gridFieldChooserControlGridGroupLayoutState;
				if (_gridFieldChooserControlGridGroupLayoutState != null)
				{
					_gridFieldChooserControlGridGroupLayoutState._isHot = true;
					Invalidate();
				}
			}
		}

		private GridGroupLayoutState FindItemAt(int value, int otherValue)
		{
			if (_mordexRowLayoutState != null)
			{
				for (int i = 0; i < _mordexRowLayoutState._arrayList.Count; i++)
				{
					Rectangle rectangle = GetItemBoundsFromFirstVisible(i);
					if (!rectangle.IsEmpty && rectangle.Contains(value, otherValue))
					{
						return (GridGroupLayoutState)_mordexRowLayoutState._arrayList[i];
					}
				}
			}
			return null;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			UpdateContentBounds();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			SetHotItem(null);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.Button == MouseButtons.None)
			{
				SetHotItem(FindItemAt(e.X, e.Y));
			}
			else if (GetPressedItem() != null && _dataGrid != null && GetPressedItem() != null && !_dragThresholdBounds.Contains(e.X, e.Y))
			{
				Rectangle rectangle = GetItemBoundsFromFirstVisible(_mordexRowLayoutState._arrayList.IndexOf(_gridGroupLayoutState));
				((IDragHost)_dataGrid).StartDrag(_gridGroupLayoutState, e.X - rectangle.X, e.Y - rectangle.Y);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left)
			{
				SetPressedItem(FindItemAt(e.X, e.Y));
				_dragThresholdBounds = new Rectangle(e.X - SystemInformation.DragSize.Width / 2, e.Y - SystemInformation.DragSize.Height / 2, SystemInformation.DragSize.Width, SystemInformation.DragSize.Height);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			SetPressedItem(null);
			_dragThresholdBounds = Rectangle.Empty;
			base.OnMouseUp(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			GridTable table = Table;
			Brush brush = GdiCache.Brush(BackColor);
			e.Graphics.FillRectangle(brush, base.ClientRectangle);
			if (table != null && _dataGrid != null && !_dataGrid.IsDisposed && _mordexRowLayoutState != null)
			{
				GridFormatStyle gridFormatStyle = BuildChooserStyle();
				for (int i = _firstVisibleIndex; i < _mordexRowLayoutState._arrayList.Count; i++)
				{
					GridGroupLayoutState gridGroupLayoutState = (GridGroupLayoutState)_mordexRowLayoutState._arrayList[i];
					Rectangle rectangle = GetItemBoundsFromFirstVisible(i);
					if (!rectangle.IsEmpty)
					{
						_dataGrid.DrawLayoutItemInBounds(e.Graphics, gridGroupLayoutState, gridFormatStyle, rectangle);
					}
				}
			}
			ControlStyle.DrawBorder(e.Graphics, base.ClientRectangle, _borderStyle, base.Enabled);
		}

		private void OnGridDisposed(object sender, EventArgs eventArgs)
		{
			DataGrid = null;
		}

		private void ResetScrollPosition()
		{
			_firstVisibleIndex = 0;
			RebuildLayout();
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			Point position = Cursor.Position;
			position = PointToClient(position);
			GridGroupLayoutState gridGroupLayoutState = FindItemAt(position.X, position.Y);
			if (gridGroupLayoutState == null)
			{
				return;
			}
			if (gridGroupLayoutState._columnSet != null)
			{
				gridGroupLayoutState._columnSet.Visible = true;
			}
			else if (gridGroupLayoutState._column != null)
			{
				gridGroupLayoutState._column.Visible = true;
			}
			else if (gridGroupLayoutState._gridGroup != null && DataGrid != null)
			{
				GroupsChangingEventArgs groupsChangingEventArgs = new GroupsChangingEventArgs(gridGroupLayoutState._gridGroup, _sourceTable, GroupChangeAction.Add, _sourceTable.Groups.Count);
				_dataGrid.RaiseGroupsChanging(groupsChangingEventArgs);
				if (!groupsChangingEventArgs.Cancel)
				{
					Table.Groups.Insert(groupsChangingEventArgs.ProposedPosition, gridGroupLayoutState._gridGroup);
					_dataGrid.RaiseGroupsChanged(new GroupsChangedEventArgs(Table));
				}
			}
		}

		private bool IsCustomGroupAvailable(GridCustomGroup gridCustomGroup)
		{
			if (!gridCustomGroup.ShowInFieldChooser)
			{
				return false;
			}
			if (Table != null)
			{
				foreach (GridGroup group in Table.Groups)
				{
					if (group.CustomGroup == gridCustomGroup)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private ArrayList ComputeArrayList(GridTable gridTable)
		{
			ArrayList arrayList = new ArrayList();
			int objectType = 0;
			bool condition = true;
			if (gridTable != null && gridTable.CellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				condition = false;
			}
			if (condition)
			{
				objectType = 0;
				foreach (GridColumn column in gridTable.Columns)
				{
					if (column.ShowInFieldChooser && !column.IsVisibleInCurrentLayout())
					{
						arrayList.Add(column);
					}
				}
			}
			else
			{
				objectType = 1;
				foreach (GridColumnSet columnSet in gridTable.ColumnSets)
				{
					if (columnSet.ShowInFieldChooser && !columnSet.Visible)
					{
						arrayList.Add(columnSet);
					}
				}
			}
			arrayList.Sort(new FieldChooserItemComparer(objectType, _headerOrder));
			for (int value = gridTable.CustomGroups.Count - 1; value >= 0; value--)
			{
				GridCustomGroup gridCustomGroup = gridTable.CustomGroups[value];
				if (IsCustomGroupAvailable(gridCustomGroup))
				{
					arrayList.Add(gridCustomGroup);
				}
			}
			return arrayList;
		}

		private int GetVisibleItemCount()
		{
			if (_headerHeight > 0)
			{
				return base.ClientRectangle.Height / _headerHeight;
			}
			return 0;
		}

		private void RebuildLayout()
		{
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			GridTable table = Table;
			int value = 0;
			int otherValue = 0;
			if (table != null && _dataGrid != null && !_dataGrid.IsDisposed)
			{
				Graphics graphics = CreateGraphics();
				_mordexRowLayoutState = new MxRowLayoutState(LayoutRowType.ColumnHeader, table);
				GridFormatStyle gridFormatStyle = BuildChooserStyle();
				if (table.CellLayoutMode == CellLayoutMode.UseColumns)
				{
					ArrayList arrayList = ComputeArrayList(table);
					value = _contentBounds.Y - 1;
					_headerHeight = table.CalculateColumnHeaderHeight(graphics, gridFormatStyle, true);
					foreach (object item in arrayList)
					{
						otherValue++;
						Rectangle rectangle = new Rectangle(_contentBounds.X, value, _contentBounds.Width, _headerHeight);
						GridGroupLayoutState gridGroupLayoutState = null;
						if (item is GridColumn column)
						{
							gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnHeader, _mordexRowLayoutState, column);
						}
						else if (item is GridCustomGroup)
						{
							gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxHeader, _mordexRowLayoutState, null);
							gridGroupLayoutState._gridGroup = new GridGroup(item as GridCustomGroup);
						}
						gridGroupLayoutState._showSortIndicator = false;
						gridGroupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Bottom;
						if (_isScrollBarVisible)
						{
							if (RightToLeft == RightToLeft.No)
							{
								gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Right;
							}
							else
							{
								gridGroupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Left;
							}
						}
						gridGroupLayoutState._rect = rectangle;
						value += _headerHeight;
						_mordexRowLayoutState._arrayList.Add(gridGroupLayoutState);
					}
				}
				else
				{
					ArrayList otherArrayList = ComputeArrayList(table);
					value = _contentBounds.Y - 1;
					_headerHeight = table.CalculateColumnSetHeaderHeight(graphics, gridFormatStyle);
					foreach (object otherItem in otherArrayList)
					{
						otherValue++;
						Rectangle otherRectangle = new Rectangle(_contentBounds.X, value, _contentBounds.Width, _headerHeight);
						GridGroupLayoutState groupLayoutState = null;
						if (otherItem is GridColumnSet gridColumnSet)
						{
							groupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnSetHeader, _mordexRowLayoutState, null);
							groupLayoutState._columnSet = gridColumnSet;
						}
						else if (otherItem is GridCustomGroup)
						{
							groupLayoutState = new GridGroupLayoutState(LayoutItemType.GroupByBoxHeader, _mordexRowLayoutState, null);
							groupLayoutState._gridGroup = new GridGroup(otherItem as GridCustomGroup);
						}
						groupLayoutState._showSortIndicator = false;
						groupLayoutState._gridGroupLayoutStateBorderFlags = BorderFlags.Bottom;
						if (_isScrollBarVisible)
						{
							if (RightToLeft == RightToLeft.No)
							{
								groupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Right;
							}
							else
							{
								groupLayoutState._gridGroupLayoutStateBorderFlags |= BorderFlags.Left;
							}
						}
						groupLayoutState._rect = otherRectangle;
						value += _headerHeight;
						_mordexRowLayoutState._arrayList.Add(groupLayoutState);
					}
				}
				graphics.Dispose();
			}
			if (value <= base.ClientRectangle.Height)
			{
				SetScrollBarShown(false);
			}
			else
			{
				SetScrollBarShown(true);
				ConfigureScrollBar(0, otherValue - 1, GetVisibleItemCount(), _firstVisibleIndex);
			}
			Invalidate();
		}

		internal void ConfigureScrollBar(int controlStyleControl2Integer, int otherControlStyleControl2Integer, int nextControlStyleControl2Integer, int previousControlStyleControl2Integer)
		{
			try
			{
				_scrollBar.SetMinimum(controlStyleControl2Integer);
				_scrollBar.SetMaximum(otherControlStyleControl2Integer);
				if (nextControlStyleControl2Integer > 0)
				{
					_scrollBar.SetPageSize(nextControlStyleControl2Integer);
				}
				else
				{
					_scrollBar.SetPageSize(1);
				}
				_scrollBar.SetScrollValue(previousControlStyleControl2Integer);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private Rectangle GetItemBoundsFromFirstVisible(int value)
		{
			if (value < _firstVisibleIndex)
			{
				return Rectangle.Empty;
			}
			if (value >= _mordexRowLayoutState._arrayList.Count)
			{
				return Rectangle.Empty;
			}
			int y = ((GridGroupLayoutState)_mordexRowLayoutState._arrayList[_firstVisibleIndex])._rect.Y;
			Rectangle offsetCore = ((GridGroupLayoutState)_mordexRowLayoutState._arrayList[value])._rect;
			offsetCore.Offset(0, -y);
			return offsetCore;
		}

		private int GetItemCount()
		{
			if (_mordexRowLayoutState == null)
			{
				return 0;
			}
			return _mordexRowLayoutState._arrayList.Count;
		}

		private void SetFirstVisibleIndex(int gridFieldChooserControlInteger)
		{
			if (gridFieldChooserControlInteger < 0)
			{
				gridFieldChooserControlInteger = 0;
			}
			if (gridFieldChooserControlInteger >= GetItemCount())
			{
				gridFieldChooserControlInteger = GetItemCount() - 1;
			}
			if (gridFieldChooserControlInteger != _firstVisibleIndex)
			{
				_firstVisibleIndex = gridFieldChooserControlInteger;
				RebuildLayout();
			}
		}

		private bool IsScrollBarShown()
		{
			return _isScrollBarVisible;
		}

		private void SetScrollBarShown(bool visible)
		{
			if (_isScrollBarVisible != visible)
			{
				_isScrollBarVisible = visible;
				_scrollBar.Visible = visible;
				UpdateContentBounds();
			}
		}

		private void UpdateBorderWidth()
		{
			_borderWidth = 1;
			if (_borderStyle == BorderStyle.None)
			{
				_borderWidth = 0;
			}
		}

		private void UpdateContentBounds()
		{
			UpdateBorderWidth();
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-_borderWidth, -_borderWidth);
			if (IsScrollBarShown())
			{
				if (RightToLeft == RightToLeft.Yes)
				{
					clientRectangle.X += SystemInformation.VerticalScrollBarWidth;
				}
				clientRectangle.Width -= SystemInformation.VerticalScrollBarWidth;
			}
			SetContentBounds(clientRectangle);
			Invalidate(invalidateChildren: true);
		}

		private void SetContentBounds(Rectangle gridFieldChooserControlRectangle)
		{
			if (_contentBounds != gridFieldChooserControlRectangle)
			{
				_contentBounds = gridFieldChooserControlRectangle;
				PositionScrollBar();
			}
		}

		private void PositionScrollBar()
		{
			if (RightToLeft == RightToLeft.Yes)
			{
				_scrollBar.Bounds = new Rectangle(_contentBounds.Left - SystemInformation.VerticalScrollBarWidth, _contentBounds.Y, SystemInformation.VerticalScrollBarWidth, _contentBounds.Height);
			}
			else
			{
				_scrollBar.Bounds = new Rectangle(_contentBounds.Right, _contentBounds.Y, SystemInformation.VerticalScrollBarWidth, _contentBounds.Height);
			}
			RebuildLayout();
		}

		MxColorSchemeAppearanceState IGridStyleSource.GetDrawingStyle(GridStyleArea area)
		{
			if (_dataGrid != null)
			{
				_ = _dataGrid.UseCompatibleTextRendering;
			}
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = new MxColorSchemeAppearanceState(useTextRenderer: true);
			mordexColorSchemeAppearanceState._isThemed = VisualStyleDrawer.IsThemingAvailable();
			mordexColorSchemeAppearanceState.SetThemedLook(true);
			return mordexColorSchemeAppearanceState;
		}

		ImageList IGridStyleSource.GetImageList()
		{
			return null;
		}

		void IGridStyleSource.OnControlStyleChanged()
		{
			UpdateContentBounds();
		}

		void IRefreshListener.Refresh()
		{
			RebuildLayout();
		}
	}
}
