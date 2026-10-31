using System;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Design
{
	[ToolboxItem(false)]
	public class ColumnsetDesigner : ScrollableBase
	{
		private MxDataGrid _dataGrid;

		private GridTable _targetTable;

		private MxRowLayoutState _mordexRowLayoutState;

		private int _scrollOffset;

		private int _contentWidth;

		private object _selectedObject;

		private Rectangle _dragFeedbackBounds;

		private EventHandler _selectedObjectChangedHandler;

		private GridValueState _gridValueState;

		private GridGroupLayoutState _gridGroupLayoutState;

		private bool _isResizing;

		private HeaderDragFeedback _dragRenderer;

		private bool _isDraggingHeader;

		private bool _isDetectingDrag;

		private bool _hasDragStarted;

		private Rectangle _dragThresholdRect;

		private GridGroupLayoutState _columnsetDesignerGridGroupLayoutState;

		private EventHandler _headerDragged;

		private Cursor _cursor;

		public event EventHandler SelectedObjectChange
		{
			add
			{
				EventHandler eventHandler = _selectedObjectChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedObjectChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _selectedObjectChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _selectedObjectChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		public ColumnsetDesigner()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			AllowDrop = true;
		}

		public void InitializeControl(MxDataGrid dataGrid, GridTable table)
		{
			_dataGrid = dataGrid;
			_targetTable = table;
			RightToLeft = _dataGrid.RightToLeft;
			RebuildLayout();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			RebuildLayout();
		}

		internal void RebuildLayout()
		{
			if (_dataGrid == null)
			{
				return;
			}
			Graphics graphics = CreateGraphics();
			_mordexRowLayoutState = _targetTable.BuildForcedColumnSetHeaderLayout(graphics, _dataGrid.GetHeaderStyle(_targetTable));
			_mordexRowLayoutState.SetLocation(base.ClientRectangle.Location);
			_contentWidth = 0;
			foreach (GridGroupLayoutState item in _mordexRowLayoutState._arrayList)
			{
				if (item._rect.Right > _contentWidth)
				{
					_contentWidth = item._rect.Right;
				}
			}
			UpdateColumnsetDesigner();
			if (_contentWidth > base.ClientRectangle.Width)
			{
				GetHorizontalScrollBar().SetScrollStep(6);
				ConfigureHorizontalScrollBar(0, _contentWidth, base.ClientRectangle.Width, _scrollOffset);
				SetHorizontalScrollRequired(true);
			}
			else
			{
				SetHorizontalScrollRequired(false);
				_scrollOffset = 0;
			}
			graphics.Dispose();
			Invalidate();
		}

		private int GetScrollOffset()
		{
			return _scrollOffset;
		}

		private void SetScrollOffset(int value)
		{
			if (value + base.ClientRectangle.Width > _contentWidth)
			{
				value = _contentWidth - base.ClientRectangle.Width;
			}
			if (value < 0)
			{
				value = 0;
			}
			if (_scrollOffset != value)
			{
				_scrollOffset = value;
				GetHorizontalScrollBar().SetScrollValue(_scrollOffset);
				Invalidate();
			}
		}

		internal object GetSelectedObject()
		{
			return _selectedObject;
		}

		internal void SetSelectedObject(object value)
		{
			if (_selectedObject != value)
			{
				_selectedObject = value;
				UpdateColumnsetDesigner();
				Invalidate();
				OnSelectedObjectChange(EventArgs.Empty);
			}
		}

		protected virtual void OnSelectedObjectChange(EventArgs e)
		{
			if (_selectedObjectChangedHandler != null)
			{
				_selectedObjectChangedHandler(this, e);
			}
		}

		private void UpdateColumnsetDesigner()
		{
			_dragFeedbackBounds = Rectangle.Empty;
			if (_mordexRowLayoutState == null)
			{
				return;
			}
			if (_selectedObject is GridColumnSet)
			{
				foreach (GridGroupLayoutState item in _mordexRowLayoutState._arrayList)
				{
					if (item._columnSet == _selectedObject || (item._column != null && item._column.ColumnSet == _selectedObject))
					{
						if (_dragFeedbackBounds.IsEmpty)
						{
							_dragFeedbackBounds = item.GetAbsoluteItemRect();
						}
						else
						{
							_dragFeedbackBounds = Rectangle.Union(_dragFeedbackBounds, item.GetAbsoluteItemRect());
						}
					}
				}
				return;
			}
			if (_selectedObject is GridColumn)
			{
				foreach (GridGroupLayoutState arrayListItem in _mordexRowLayoutState._arrayList)
				{
					if (arrayListItem._column == _selectedObject)
					{
						_dragFeedbackBounds = arrayListItem.GetAbsoluteItemRect();
						break;
					}
				}
				return;
			}
			_dragFeedbackBounds = Rectangle.Empty;
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			switch (scrollCode)
			{
			case 2:
				SetScrollOffset(GetHorizontalScrollBar().GetScrollValue());
				break;
			case 3:
				SetScrollOffset(GetHorizontalScrollBar().GetScrollValue());
				break;
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (_mordexRowLayoutState == null)
			{
				return;
			}
			object column = null;
			GridGroupLayoutState gridGroupLayoutState = FindResizeHandleAt(e.X, e.Y);
			if (gridGroupLayoutState != null)
			{
				Rectangle rectangle = ToClientBounds(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
				_gridGroupLayoutState = gridGroupLayoutState;
				_gridValueState._currentLinePosition = -1;
				_isResizing = true;
				if (gridGroupLayoutState._type == LayoutItemType.ColumnHeader)
				{
					_gridValueState._minPosition = rectangle.Right - gridGroupLayoutState._column.Width + 8;
				}
				else
				{
					_gridValueState._minPosition = rectangle.Right - gridGroupLayoutState._columnSet.Width + 8;
				}
				DoDragDrop(this, DragDropEffects.None);
				return;
			}
			gridGroupLayoutState = null;
			foreach (GridGroupLayoutState item in _mordexRowLayoutState._arrayList)
			{
				if (ToClientBounds(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled).Contains(e.X, e.Y))
				{
					if (item._type == LayoutItemType.ColumnHeader)
					{
						column = item._column;
					}
					else if (item._type == LayoutItemType.ColumnSetHeader)
					{
						column = item._columnSet;
					}
					gridGroupLayoutState = item;
					break;
				}
			}
			SetSelectedObject(column);
			if (gridGroupLayoutState != null && column != null)
			{
				ProcessMouseInput(e, gridGroupLayoutState);
			}
		}

		protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
		{
			base.OnGiveFeedback(e);
			if (_isResizing)
			{
				e.UseDefaultCursors = false;
				ProcessCursor(Cursors.VSplit, true);
			}
			if (_isDraggingHeader || _isDetectingDrag)
			{
				e.UseDefaultCursors = false;
			}
		}

		internal int GetLayoutHeight()
		{
			if (_mordexRowLayoutState != null)
			{
				return _mordexRowLayoutState._rowHeight;
			}
			return 0;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (_dataGrid == null)
			{
				base.OnPaint(e);
				return;
			}
			GridFormatStyle gridFormatStyle = _dataGrid.GetHeaderStyle(_targetTable);
			_dataGrid.PrepareControlStyleLayout();
			foreach (GridGroupLayoutState item in _mordexRowLayoutState._arrayList)
			{
				Rectangle rectangle = ToClientBounds(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				if (item._type == LayoutItemType.EmptyHeader)
				{
					DrawBackground(e.Graphics, rectangle);
				}
				else
				{
					_dataGrid.DrawLayoutItem(e.Graphics, item, gridFormatStyle, rectangle, rectangle);
				}
			}
			if (!_dragFeedbackBounds.IsEmpty)
			{
				Brush brush = new SolidBrush(Color.FromArgb(125, SystemColors.Highlight));
				e.Graphics.FillRectangle(brush, ToClientBounds(_dragFeedbackBounds, true));
				brush.Dispose();
			}
			if (_targetTable.ColumnSets.Count == 0)
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!base.UseCompatibleTextRendering);
				textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
				textLayoutRenderer.DrawTextWithColor(e.Graphics, "There are no columnsets in this table", base.ClientRectangle, Font, SystemColors.ControlDark);
				textLayoutRenderer.ReleaseStringFormat();
			}
			DrawControlBorder(e.Graphics);
		}

		private MxRowLayoutState FindRowLayoutAt(Point point)
		{
			if (_mordexRowLayoutState != null && point.Y >= _mordexRowLayoutState.GetLocation().Y && point.Y < _mordexRowLayoutState.GetLocation().Y + _mordexRowLayoutState._rowHeight)
			{
				return _mordexRowLayoutState;
			}
			return null;
		}

		private bool DetectDrag(int value, int otherValue)
		{
			bool allowDrop = AllowDrop;
			AllowDrop = true;
			Point p = new Point(value, otherValue);
			p = PointToScreen(p);
			_isDetectingDrag = true;
			_hasDragStarted = false;
			Size dragSize = SystemInformation.DragSize;
			_dragThresholdRect = new Rectangle(p.X - dragSize.Width / 2, p.Y - dragSize.Height / 2, dragSize.Width, dragSize.Height);
			DoDragDrop(_dragThresholdRect, DragDropEffects.None);
			_isDetectingDrag = false;
			AllowDrop = allowDrop;
			return _hasDragStarted;
		}

		private void MoveResizeLine()
		{
			Point position = Cursor.Position;
			int gridValueStateInteger = PointToClient(position).X;
			if (gridValueStateInteger < _gridValueState._minPosition)
			{
				gridValueStateInteger = _gridValueState._minPosition;
			}
			if (gridValueStateInteger != _gridValueState._currentLinePosition)
			{
				Rectangle rectangle = ToClientBounds(_gridGroupLayoutState.GetAbsoluteItemRect(), _gridGroupLayoutState._isHorizontallyScrolled);
				Point point;
				Point pointToScreen;
				if (_gridValueState._currentLinePosition != -1)
				{
					point = PointToScreen(new Point(_gridValueState._currentLinePosition, rectangle.Top));
					pointToScreen = PointToScreen(new Point(_gridValueState._currentLinePosition, base.ClientRectangle.Bottom));
					GridHelper.DrawSplitLine(this, point, pointToScreen, Color.Black);
				}
				_gridValueState._currentLinePosition = gridValueStateInteger;
				point = PointToScreen(new Point(_gridValueState._currentLinePosition, rectangle.Top));
				pointToScreen = PointToScreen(new Point(_gridValueState._currentLinePosition, base.ClientRectangle.Bottom));
				GridHelper.DrawSplitLine(this, point, pointToScreen, Color.Black);
			}
		}

		private void FinishResize(bool condition)
		{
			_isResizing = false;
			Point position = Cursor.Position;
			int gridValueStateInteger = PointToClient(position).X;
			if (gridValueStateInteger < _gridValueState._minPosition)
			{
				gridValueStateInteger = _gridValueState._minPosition;
			}
			Rectangle rectangle = ToClientBounds(_gridGroupLayoutState.GetAbsoluteItemRect(), _gridGroupLayoutState._isHorizontallyScrolled);
			if (_gridValueState._currentLinePosition != -1)
			{
				Point point = PointToScreen(new Point(_gridValueState._currentLinePosition, rectangle.Top));
				Point pointToScreen = PointToScreen(new Point(_gridValueState._currentLinePosition, base.ClientRectangle.Bottom));
				GridHelper.DrawSplitLine(this, point, pointToScreen, Color.Black);
			}
			if (!condition)
			{
				if (_gridGroupLayoutState._type == LayoutItemType.ColumnHeader)
				{
					_gridGroupLayoutState._column.Width = _gridGroupLayoutState._column.Width + (gridValueStateInteger - rectangle.Right + 1);
				}
				else if (_gridGroupLayoutState._type == LayoutItemType.ColumnSetHeader)
				{
					_gridGroupLayoutState._columnSet.Width = _gridGroupLayoutState._columnSet.Width + (gridValueStateInteger - rectangle.Right + 1);
				}
				RebuildLayout();
			}
		}

		protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
		{
			if (_isResizing)
			{
				if (e.Action == DragAction.Continue)
				{
					MoveResizeLine();
				}
				else
				{
					FinishResize(e.Action == DragAction.Cancel);
				}
			}
			if (_isDetectingDrag)
			{
				Point position = Cursor.Position;
				if (!_dragThresholdRect.Contains(position))
				{
					e.Action = DragAction.Cancel;
					_hasDragStarted = true;
				}
			}
			else if (_isDraggingHeader)
			{
				FinishOrContinueDrag(e);
			}
			else
			{
				base.OnQueryContinueDrag(e);
			}
		}

		private void ProcessMouseInput(MouseEventArgs eventArgs, GridGroupLayoutState data)
		{
			if (eventArgs.Button == MouseButtons.Left && DetectDrag(eventArgs.X, eventArgs.Y))
			{
				Rectangle rectangle = ToClientBounds(data.GetAbsoluteItemRect(), data._isHorizontallyScrolled);
				if (data._type == LayoutItemType.ColumnHeader)
				{
					_ = data._column;
					data._columnSet.Remove(data._column);
					RebuildLayout();
					Update();
					BeginHeaderDrag(data, eventArgs.X - rectangle.X, eventArgs.Y - rectangle.Y);
					return;
				}
				GridFormatStyle gridFormatStyle = _dataGrid.GetHeaderStyle(data.GetOwnerTable());
				AllowDrop = true;
				_isDraggingHeader = true;
				Graphics graphics = CreateGraphics();
				_dragRenderer = RenderContent(graphics, data, rectangle.Width, rectangle.Height);
				graphics.Dispose();
				graphics = null;
				Update();
				_dragRenderer.SetDropAllowed(true);
				graphics = _dragRenderer.CreateFeedbackGraphics();
				_dragRenderer._gridRenderer3Point = new Point(eventArgs.X - rectangle.X, eventArgs.Y - rectangle.Y);
				rectangle.Offset(-rectangle.X, -rectangle.Y);
				data._isPressed = false;
				data._isHot = false;
				_dataGrid.PrepareControlStyleLayout();
				_dataGrid.DrawLayoutItem(graphics, data, gridFormatStyle, rectangle, rectangle);
				data._isPressed = true;
				gridFormatStyle.DisposeResources();
				DataObject dataObject = new DataObject();
				dataObject.SetData("GridHeaderDrag", autoConvert: false, data);
				DoDragDrop(dataObject, DragDropEffects.Move);
			}
		}

		private HeaderDragFeedback RenderContent(Graphics graphics, GridGroupLayoutState item, int width, int height)
		{
			try
			{
				return CreateDragRenderer(graphics, item, width, height);
			}
			catch (SecurityException)
			{
				return new HeaderDragFeedback(graphics, item, width, height);
			}
		}

		private HeaderDragFeedback CreateDragRenderer(Graphics graphics, GridGroupLayoutState item, int width, int height)
		{
			return new NativeHeaderDragFeedback(graphics, item, width, height);
		}

		internal void BeginHeaderDrag(GridGroupLayoutState data, int value, int otherValue)
		{
			Rectangle rectangle = data.GetAbsoluteItemRect();
			GridFormatStyle gridFormatStyle = _dataGrid.GetHeaderStyle(data.GetOwnerTable());
			bool allowDrop = AllowDrop;
			AllowDrop = true;
			_isDraggingHeader = true;
			Graphics graphics = CreateGraphics();
			_dragRenderer = RenderContent(graphics, data, rectangle.Width, rectangle.Height);
			graphics.Dispose();
			graphics = null;
			Update();
			_dragRenderer.SetDropAllowed(true);
			graphics = _dragRenderer.CreateFeedbackGraphics();
			_dragRenderer._gridRenderer3Point = new Point(value, otherValue);
			rectangle.Offset(-rectangle.X, -rectangle.Y);
			_dataGrid.PrepareControlStyleLayout();
			data._isPressed = false;
			_dataGrid.DrawLayoutItem(graphics, data, gridFormatStyle, rectangle, rectangle);
			data._isPressed = true;
			gridFormatStyle.DisposeResources();
			DoDragDrop(data, DragDropEffects.Move);
			AllowDrop = allowDrop;
		}

		private void UpdateDragFeedback(QueryContinueDragEventArgs eventArgs)
		{
			Point mousePosition = Control.MousePosition;
			_dragRenderer.Move();
			mousePosition = PointToClient(mousePosition);
			MxRowLayoutState mordexRowLayoutState = FindRowLayoutAt(mousePosition);
			_dragRenderer._mordexRowLayoutState = mordexRowLayoutState;
			if (mordexRowLayoutState != null)
			{
				LayoutRowType rowType = mordexRowLayoutState._rowType;
				if (rowType == LayoutRowType.ColumnHeader)
				{
					if (_dragRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader)
					{
						_dragRenderer.SetDropAllowed(mordexRowLayoutState._owner == _dragRenderer._gridGroupLayoutState._column.Table);
					}
					else
					{
						_dragRenderer.SetDropAllowed(mordexRowLayoutState._owner == _dragRenderer._gridGroupLayoutState._columnSet.Table);
					}
					if (!_dragRenderer.IsDropTargetValid())
					{
						return;
					}
					if (mousePosition.X > 0 && mousePosition.X < 8)
					{
						if (GetScrollOffset() > 0)
						{
							_dragRenderer.EraseArrows();
							_dragRenderer.EraseHeader();
							SetScrollOffset(_scrollOffset - 80);
							Update();
							_dragRenderer.Move();
						}
					}
					else if (mousePosition.X > base.ClientRectangle.Right - 8)
					{
						int fullWidth = mordexRowLayoutState._owner.CalculateFullWidth();
						if (fullWidth - _scrollOffset > base.ClientRectangle.Right)
						{
							_dragRenderer.EraseArrows();
							_dragRenderer.EraseHeader();
							int value = _scrollOffset + 80;
							if (fullWidth - value < base.ClientRectangle.Right)
							{
								value = fullWidth - base.ClientRectangle.Right;
							}
							SetScrollOffset(value);
							Update();
							_dragRenderer.Move();
						}
					}
					Rectangle offsetCore = Rectangle.Empty;
					_dragRenderer._dropIndex = -1;
					bool condition = false;
					foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
					{
						offsetCore = ToClientBounds(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
						if (_dragRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader)
						{
							if (item._type != LayoutItemType.EmptyHeader || !offsetCore.Contains(mousePosition))
							{
								continue;
							}
							if (item != _dragRenderer._gridGroupLayoutState)
							{
								if (item != _columnsetDesignerGridGroupLayoutState)
								{
									_columnsetDesignerGridGroupLayoutState = item;
									_dragFeedbackBounds = item.GetAbsoluteItemRect();
									_dragRenderer.EraseHeader();
									Invalidate();
									Update();
									_dragRenderer.Move();
								}
								condition = true;
							}
							break;
						}
						if (_dragRenderer._gridGroupLayoutState._type != LayoutItemType.ColumnSetHeader || item._type != LayoutItemType.ColumnSetHeader || !offsetCore.Contains(mousePosition))
						{
							continue;
						}
						if (item != _dragRenderer._gridGroupLayoutState)
						{
							if (mousePosition.X - offsetCore.X > offsetCore.Right - mousePosition.X)
							{
								offsetCore = new Rectangle(offsetCore.Right, offsetCore.Top, 0, offsetCore.Height);
								_dragRenderer._dropIndex = item._columnSet.Position + 1;
							}
							else
							{
								offsetCore = new Rectangle(offsetCore.Left, offsetCore.Top, 0, offsetCore.Height);
								_dragRenderer._dropIndex = item._columnSet.Position;
							}
						}
						break;
					}
					if (_dragRenderer._dropIndex != -1)
					{
						Rectangle rectangle = ToClientBounds(_dragRenderer._gridGroupLayoutState.GetAbsoluteItemRect(), _dragRenderer._gridGroupLayoutState._isHorizontallyScrolled);
						if ((offsetCore.X != rectangle.X && offsetCore.X != rectangle.Right) || offsetCore.Y != rectangle.Y)
						{
							if (offsetCore.X < 0)
							{
								offsetCore.X = 0;
							}
							if (offsetCore.X > base.ClientRectangle.Right)
							{
								offsetCore.X = base.ClientRectangle.Right;
							}
							offsetCore.Location = PointToScreen(offsetCore.Location);
							_dragRenderer.DrawArrows(offsetCore);
						}
						else
						{
							_dragRenderer.EraseArrows();
						}
					}
					else
					{
						_dragRenderer.EraseArrows();
					}
					if (!condition && _columnsetDesignerGridGroupLayoutState != null)
					{
						_columnsetDesignerGridGroupLayoutState = null;
						_dragFeedbackBounds = Rectangle.Empty;
						_dragRenderer.EraseHeader();
						Invalidate();
						Update();
						_dragRenderer.Move();
					}
				}
				else
				{
					_dragRenderer.SetDropAllowed(false);
				}
			}
			else
			{
				_dragRenderer.SetDropAllowed(false);
			}
		}

		private void FinishOrContinueDrag(QueryContinueDragEventArgs eventArgs)
		{
			if (eventArgs.Action == DragAction.Continue)
			{
				if (_dragRenderer != null)
				{
					UpdateDragFeedback(eventArgs);
				}
				return;
			}
			PointToClient(Cursor.Position);
			if (_dragRenderer == null)
			{
				return;
			}
			_dragRenderer.EraseArrows();
			_dragRenderer.EraseHeader();
			if (eventArgs.Action == DragAction.Drop && _dragRenderer.IsDropTargetValid() && _dragRenderer._mordexRowLayoutState != null && (_dragRenderer._dropIndex != -1 || _columnsetDesignerGridGroupLayoutState != null) && _dragRenderer._mordexRowLayoutState._rowType == LayoutRowType.ColumnHeader)
			{
				if (_dragRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnSetHeader)
				{
					if (_dragRenderer._dropIndex <= _dragRenderer._gridGroupLayoutState._columnSet.Position)
					{
						_dragRenderer._gridGroupLayoutState._columnSet.Position = _dragRenderer._dropIndex;
					}
					else
					{
						_dragRenderer._gridGroupLayoutState._columnSet.Position = _dragRenderer._dropIndex - 1;
					}
					_dragRenderer._gridGroupLayoutState._columnSet.Visible = true;
				}
				else if (_columnsetDesignerGridGroupLayoutState != null)
				{
					_columnsetDesignerGridGroupLayoutState._columnSet.Add(_dragRenderer._gridGroupLayoutState._column, _columnsetDesignerGridGroupLayoutState._topMargin, _columnsetDesignerGridGroupLayoutState._leftMargin);
					SetSelectedObject(_dragRenderer._gridGroupLayoutState._column);
				}
			}
			_dragRenderer.Dispose();
			_dragRenderer = null;
			RebuildLayout();
			OnHeaderDragged(EventArgs.Empty);
		}

		internal void AddHeaderDraggedHandler(EventHandler otherEventHandler)
		{
			EventHandler eventHandler = _headerDragged;
			EventHandler nextEventHandler;
			do
			{
				nextEventHandler = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(nextEventHandler, otherEventHandler);
				eventHandler = Interlocked.CompareExchange(ref _headerDragged, value, nextEventHandler);
			}
			while ((object)eventHandler != nextEventHandler);
		}

		protected virtual void OnHeaderDragged(EventArgs e)
		{
			if (_headerDragged != null)
			{
				_headerDragged(this, e);
			}
		}

		private Rectangle ToClientBounds(Rectangle rectangle, bool condition)
		{
			if (RightToLeft == RightToLeft.No)
			{
				if (condition)
				{
					rectangle.Offset(-_scrollOffset, 0);
				}
				return rectangle;
			}
			int contentWidth = ((_contentWidth >= base.ClientRectangle.Width) ? _contentWidth : base.ClientRectangle.Width);
			contentWidth += base.ClientRectangle.Left;
			int value = contentWidth - rectangle.Right;
			value += _borderWidth;
			if (condition)
			{
				value -= _scrollOffset;
			}
			rectangle.X = value;
			return rectangle;
		}

		private void DrawBackground(Graphics graphics, Rectangle bounds)
		{
			graphics.DrawLine(SystemPens.WindowFrame, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
			graphics.DrawLine(SystemPens.WindowFrame, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom - 1);
			bounds.Width--;
			bounds.Height--;
			Brush brush = new SolidBrush(GridHelper.BlendColors(Color.White, SystemColors.Control, 0.75f));
			graphics.FillRectangle(brush, bounds);
			brush.Dispose();
			brush = null;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			GridGroupLayoutState gridGroupLayoutState = FindResizeHandleAt(e.X, e.Y);
			if (gridGroupLayoutState != null)
			{
				ProcessCursor(Cursors.VSplit, true);
			}
			else
			{
				ProcessCursor(null, false);
			}
		}

		private GridGroupLayoutState FindResizeHandleAt(int value, int otherValue)
		{
			Point pt = new Point(value, otherValue);
			foreach (GridGroupLayoutState item in _mordexRowLayoutState._arrayList)
			{
				Rectangle rectangle = ToClientBounds(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				if ((item._type == LayoutItemType.ColumnHeader || item._type == LayoutItemType.ColumnSetHeader) && new Rectangle(rectangle.Right - 3, rectangle.Top, 5, rectangle.Height).Contains(pt) && (item._type == LayoutItemType.ColumnHeader || item._type == LayoutItemType.ColumnSetHeader))
				{
					return item;
				}
			}
			return null;
		}

		private void ProcessCursor(Cursor cursor, bool condition)
		{
			if (!(Cursor != cursor))
			{
				return;
			}
			if (cursor == null)
			{
				if (_cursor == null)
				{
					Cursor = Cursors.Default;
					return;
				}
				Cursor = _cursor;
				_cursor = null;
			}
			else
			{
				if (condition)
				{
					_cursor = Cursor;
				}
				Cursor = cursor;
			}
		}
	}
}
