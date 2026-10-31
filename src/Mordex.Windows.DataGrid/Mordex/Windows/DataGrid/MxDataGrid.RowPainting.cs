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
		internal void PaintLayoutItemForPrint(Graphics graphics, MxDataGridPrintDocument gridPrintDocument, GridGroupLayoutState item, GridFormatStyle gridFormatStyle, Rectangle bounds)
		{
			bounds = MirrorRectForPrintRtl(gridPrintDocument, graphics, bounds);
			GridFormatStyle style = gridFormatStyle.CreatePrintStyle(gridPrintDocument);
			CellRendererBase renderer = gridPrintDocument._printControlStyle.GetLayoutState().GetRenderer(item);
			if (renderer == null)
			{
				return;
			}
			switch (item._type)
			{
			case LayoutItemType.RowHeader:
			case LayoutItemType.NewRowSeparator:
			case LayoutItemType.TotalRowSeparator:
				style = gridPrintDocument.BuildHeaderStyle(item.GetOwnerTable()).CreatePrintStyle(gridPrintDocument);
				break;
			case LayoutItemType.GroupIndent:
				style = gridPrintDocument.BuildGroupHeaderCellStyle(item, false).CreatePrintStyle(gridPrintDocument);
				break;
			}
			bool condition = false;
			if (IsItemOwnerDrawn(item))
			{
				Font font = (Font)renderer.GetStyleFont(style).Clone();
				DrawGridAreaEventArgs e = new DrawGridAreaEventArgs(graphics, bounds, font, style, renderer.GetTextRenderer(item, style).GetStringFormat(), item, this);
				e._itemRenderer = renderer;
				OnDrawGridArea(e);
				font?.Dispose();
				if (e.Handled)
				{
					condition = true;
				}
			}
			if (!condition && gridPrintDocument.PrintCellBackground)
			{
				renderer.PaintBackground(graphics, item, style, bounds, selected: false, focused: false);
			}
			Rectangle foregroundRect = renderer.GetForegroundRect(bounds, item, style);
			if (!condition)
			{
				renderer.PaintForeground(graphics, item, style, foregroundRect, selected: false, focused: false);
			}
			renderer.PaintAddornments(graphics, item, style, bounds);
			renderer._gridlineRenderer = gridPrintDocument._printControlStyle.GetLayoutState().GetGridLinePenRenderer();
			renderer.PaintGridlines(graphics, item, bounds);
		}

		private bool IsItemOwnerDrawn(GridGroupLayoutState gridGroupLayoutState)
		{
			switch (gridGroupLayoutState._type)
			{
			case LayoutItemType.CardCaption:
				return (_ownerDrawnAreas & GridOwnerDrawnArea.CardCaption) != 0;
			case LayoutItemType.GroupByBoxBackground:
				return (_ownerDrawnAreas & GridOwnerDrawnArea.GroupByBoxBackground) != 0;
			case LayoutItemType.GroupHeaderRow:
				return (_ownerDrawnAreas & GridOwnerDrawnArea.GroupRow) != 0;
			case LayoutItemType.CardCell:
				if (gridGroupLayoutState._column != null && (gridGroupLayoutState._column.OwnerDrawnMode & ColumnOwnerDrawnMode.CardCell) == ColumnOwnerDrawnMode.CardCell)
				{
					return true;
				}
				break;
			case LayoutItemType.Cell:
				if (gridGroupLayoutState._column != null && (gridGroupLayoutState._column.OwnerDrawnMode & ColumnOwnerDrawnMode.Cells) == ColumnOwnerDrawnMode.Cells)
				{
					return true;
				}
				break;
			case LayoutItemType.CardHeader:
				if (gridGroupLayoutState._column != null && (gridGroupLayoutState._column.OwnerDrawnMode & ColumnOwnerDrawnMode.CardHeader) != ColumnOwnerDrawnMode.None)
				{
					return true;
				}
				break;
			case LayoutItemType.GroupByBoxHeader:
			case LayoutItemType.ColumnHeader:
				if (gridGroupLayoutState._column != null && (gridGroupLayoutState._column.OwnerDrawnMode & ColumnOwnerDrawnMode.ColumnHeader) != ColumnOwnerDrawnMode.None)
				{
					return true;
				}
				break;
			case LayoutItemType.GroupTotalCell:
				if (gridGroupLayoutState._column != null && (gridGroupLayoutState._column.OwnerDrawnMode & ColumnOwnerDrawnMode.GroupTotalCell) != ColumnOwnerDrawnMode.None)
				{
					return true;
				}
				break;
			case LayoutItemType.TotalRowCell:
				if (gridGroupLayoutState._column != null && (gridGroupLayoutState._column.OwnerDrawnMode & ColumnOwnerDrawnMode.TotalRowCell) != ColumnOwnerDrawnMode.None)
				{
					return true;
				}
				break;
			case LayoutItemType.FilterRowCell:
				if (gridGroupLayoutState._column != null && (gridGroupLayoutState._column.OwnerDrawnMode & ColumnOwnerDrawnMode.FilterRowCell) != ColumnOwnerDrawnMode.None)
				{
					return true;
				}
				break;
			case LayoutItemType.RowHeader:
				if ((_ownerDrawnAreas & GridOwnerDrawnArea.RowHeaders) != GridOwnerDrawnArea.None)
				{
					return true;
				}
				break;
			case LayoutItemType.GroupByBoxInfo:
				if ((_ownerDrawnAreas & GridOwnerDrawnArea.GroupByBoxInfoText) != GridOwnerDrawnArea.None)
				{
					return true;
				}
				break;
			case LayoutItemType.GroupByBoxTableHeader:
				if ((_ownerDrawnAreas & GridOwnerDrawnArea.GroupByBoxTableInfo) != GridOwnerDrawnArea.None)
				{
					return true;
				}
				break;
			case LayoutItemType.PreviewRow:
				if ((_ownerDrawnAreas & GridOwnerDrawnArea.PreviewRows) != GridOwnerDrawnArea.None)
				{
					return true;
				}
				break;
			case LayoutItemType.NewRowSeparator:
			case LayoutItemType.TotalRowSeparator:
				if ((_ownerDrawnAreas & GridOwnerDrawnArea.NewAndTotalRowSeparators) != GridOwnerDrawnArea.None)
				{
					return true;
				}
				break;
			case LayoutItemType.TableHeader:
				if ((_ownerDrawnAreas & GridOwnerDrawnArea.TableHeaders) != GridOwnerDrawnArea.None)
				{
					return true;
				}
				break;
			case LayoutItemType.ColumnSetHeader:
				if ((_ownerDrawnAreas & GridOwnerDrawnArea.ColumnSetHeaders) != GridOwnerDrawnArea.None)
				{
					return true;
				}
				break;
			}
			return false;
		}

		private void PaintRowFocusRectangle(Graphics graphics, MxRowLayoutState mordexRowLayoutState)
		{
			if (_row == null || !HasFocusOrDropDown())
			{
				return;
			}
			bool condition = false;
			if (graphics == null)
			{
				if (base.IsDisposed || base.Disposing)
				{
					return;
				}
				condition = true;
				graphics = CreateGraphics();
			}
			if (CellSelectionMode == CellSelectionMode.EntireRow || mordexRowLayoutState._rowType == LayoutRowType.GroupHeaderRow || (SelectionMode != SelectionMode.MultipleCellSelection && (_currentColumn == null || _gridSelectedItemCollection.Count > 1)))
			{
				Rectangle rectangle = AdjustRectangleToViewport(mordexRowLayoutState.GetContentBounds(), GetEffectiveFrozenColumns() == 0);
				if (rectangle.Width > 0 && rectangle.Height > 0)
				{
					if (rectangle.Left < base.ClientRectangle.Left)
					{
						rectangle.Width = rectangle.Right - base.ClientRectangle.Left;
						rectangle.X = base.ClientRectangle.Left;
					}
					if (rectangle.Right > base.ClientRectangle.Right)
					{
						rectangle.Width = base.ClientRectangle.Right - rectangle.X;
					}
					if (mordexRowLayoutState.LayoutTable.GetEffectiveGridLines() == GridLines.Both || mordexRowLayoutState.LayoutTable.GetEffectiveGridLines() == GridLines.Horizontal || mordexRowLayoutState.LayoutTable.GetEffectiveGridLines() == GridLines.RowOutline)
					{
						rectangle.Height--;
					}
					if (mordexRowLayoutState.LayoutTable.GetEffectiveGridLines() == GridLines.None || mordexRowLayoutState.LayoutTable.GetEffectiveGridLines() == GridLines.Horizontal || mordexRowLayoutState.LayoutTable.GetEffectiveGridLines() == GridLines.RowOutline)
					{
						rectangle.Width--;
					}
					DrawStyledFocusRectangle(graphics, rectangle, _focusStyle, false);
				}
			}
			if (_currentColumn != null && IsSingleCellSelection())
			{
				GridGroupLayoutState gridGroupLayoutState = mordexRowLayoutState.FindCellItemForColumn(_currentColumn);
				if (gridGroupLayoutState != null)
				{
					Rectangle rectangle = AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
					if (gridGroupLayoutState.HasRightBorder())
					{
						rectangle.Width--;
					}
					if (gridGroupLayoutState.HasBottomBorder())
					{
						rectangle.Height--;
					}
					FocusStyle focusStyle = _focusStyle;
					DrawStyledFocusRectangle(graphics, rectangle, focusStyle, true);
				}
			}
			if (condition)
			{
				graphics.Dispose();
				graphics = null;
			}
		}

		private void DrawStyledFocusRectangle(Graphics graphics, Rectangle rectangle, FocusStyle focusStyle, bool condition)
		{
			switch (focusStyle)
			{
			case FocusStyle.Dotted:
			{
				Color black = Color.Black;
				Color white = Color.White;
				Brush brush = new HatchBrush(HatchStyle.Percent50, black, white);
				Pen otherPen = new Pen(brush);
				GridHelper.DrawFlatBorder(graphics, otherPen, rectangle);
				otherPen.Dispose();
				brush.Dispose();
				break;
			}
			case FocusStyle.Solid:
			{
				Pen pen = ((!_gridControlStyle.GetLayoutState().GetGridlinesStyle().IsThemedLook) ? GdiCache.Pen(_selectedFormatStyle.BackColor) : GdiCache.Pen(_gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.HotElementBorder)));
				GridHelper.DrawFlatBorder(graphics, pen, rectangle);
				pen = null;
				break;
			}
			}
		}

		private void PaintCellFocusRectangle(Graphics graphics, GridGroupLayoutState gridGroupLayoutState)
		{
			if (!base.ContainsFocus)
			{
				return;
			}
			bool condition = false;
			if (graphics == null)
			{
				if (base.IsDisposed || base.Disposing)
				{
					return;
				}
				condition = true;
				graphics = CreateGraphics();
			}
			Rectangle rectangle = AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), true);
			if (rectangle.Left < base.ClientRectangle.Left)
			{
				rectangle.Width = rectangle.Right - base.ClientRectangle.Left;
				rectangle.X = base.ClientRectangle.Left;
			}
			if (rectangle.Right > base.ClientRectangle.Right)
			{
				rectangle.Width = base.ClientRectangle.Right - rectangle.X;
			}
			if (_cardViewGridlines == CardViewGridlines.Both || _cardViewGridlines == CardViewGridlines.Horizontal || _cardViewGridlines == CardViewGridlines.FieldsOnly)
			{
				rectangle.Height--;
				if (_cardInnerSpacing != 0)
				{
					rectangle.Width--;
				}
			}
			DrawStyledFocusRectangle(graphics, rectangle, _focusStyle, true);
			if (condition)
			{
				graphics.Dispose();
				graphics = null;
			}
		}

		private static HeaderDragFeedback CreateDragRenderer(Graphics graphics, GridGroupLayoutState item, int width, int height)
		{
			try
			{
				return new NativeHeaderDragFeedback(graphics, item, width, height);
			}
			finally
			{
			}
		}

		private void DrawColumnResizeLine()
		{
			Point position = PointerPosition.Current;
			int gridValueStateInteger = PointToClient(position).X;
			if (RightToLeft == RightToLeft.No)
			{
				if (gridValueStateInteger < _gridGridValueState._minPosition)
				{
					gridValueStateInteger = _gridGridValueState._minPosition;
				}
			}
			else if (gridValueStateInteger > _gridGridValueState._minPosition)
			{
				gridValueStateInteger = _gridGridValueState._minPosition;
			}
			if (gridValueStateInteger != _gridGridValueState._currentLinePosition)
			{
				Rectangle rectangle = AdjustRectangleToViewport(_activeButtonItem.GetAbsoluteItemRect(), _activeButtonItem._isHorizontallyScrolled);
				Point point;
				Point pointToScreen;
				if (_gridGridValueState._currentLinePosition != -1)
				{
					point = PointToScreen(new Point(_gridGridValueState._currentLinePosition, rectangle.Top));
					pointToScreen = PointToScreen(new Point(_gridGridValueState._currentLinePosition, base.ClientRectangle.Bottom));
					GridHelper.DrawSplitLine(this, point, pointToScreen, Color.Black);
				}
				_gridGridValueState._currentLinePosition = gridValueStateInteger;
				point = PointToScreen(new Point(_gridGridValueState._currentLinePosition, rectangle.Top));
				pointToScreen = PointToScreen(new Point(_gridGridValueState._currentLinePosition, base.ClientRectangle.Bottom));
				GridHelper.DrawSplitLine(this, point, pointToScreen, Color.Black);
			}
		}

		private void ResetPaintResources()
		{
			if (_cachedImageRenderer != null)
			{
				_cachedImageRenderer.DisposeResources();
				_cachedImageRenderer = null;
			}
			if (_rowBorderPen != null)
			{
				_rowBorderPen.Dispose();
				_rowBorderPen = null;
			}
			if (_gridlinePen != null)
			{
				_gridlinePen.Dispose();
				_gridlinePen = null;
			}
			if (RootTable != null)
			{
				RootTable.DisposeGridLinePen();
			}
			_controlStyleFont = null;
			_gridControlStyle.ReleaseLayoutState();
		}

		internal void InvalidateAfterTableChange(GridTable gridTable)
		{
			AcceptCellEdit();
			RefreshRowCountAndInvalidate();
		}

		private void InvalidateRowLayout(MordexRow mordexRow)
		{
			if (mordexRow == null)
			{
				return;
			}
			if (_view == View.TableView)
			{
				MxRowLayoutState mordexRowLayoutState = null;
				if (_bodyRowLayouts != null)
				{
					foreach (MxRowLayoutState item in _bodyRowLayouts)
					{
						if (item._gridRow != null && item._gridRow.GetLogicalRow() == mordexRow)
						{
							mordexRowLayoutState = item;
							break;
						}
					}
				}
				if (mordexRowLayoutState == null && _headerRowLayouts != null)
				{
					foreach (MxRowLayoutState headerRowLayout in _headerRowLayouts)
					{
						if (headerRowLayout._gridRow != null && headerRowLayout._gridRow.GetLogicalRow() == mordexRow)
						{
							mordexRowLayoutState = headerRowLayout;
							break;
						}
					}
				}
				if (mordexRowLayoutState == null && _footerRowLayouts != null)
				{
					foreach (MxRowLayoutState footerRowLayout in _footerRowLayouts)
					{
						if (footerRowLayout._gridRow != null && footerRowLayout._gridRow.GetLogicalRow() == mordexRow)
						{
							mordexRowLayoutState = footerRowLayout;
							break;
						}
					}
				}
				if (mordexRowLayoutState != null)
				{
					Rectangle rc = new Rectangle(base.ClientRectangle.Left, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
					rc.Inflate(1, 1);
					Invalidate(rc);
				}
			}
			else
			{
				Invalidate();
			}
		}

		private void InvalidateLayoutItem(GridGroupLayoutState gridGroupLayoutState)
		{
			if (gridGroupLayoutState != null)
			{
				Rectangle rc = AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
				rc.Inflate(1, 1);
				Invalidate(rc);
			}
		}

		internal void InvalidateGridRowLayout(GridRow gridRow)
		{
			if (gridRow == null)
			{
				return;
			}
			if (_view == View.TableView)
			{
				MxRowLayoutState mordexRowLayoutState = null;
				if (_bodyRowLayouts != null)
				{
					foreach (MxRowLayoutState item in _bodyRowLayouts)
					{
						if (AreSameLogicalRow(item._gridRow, gridRow))
						{
							mordexRowLayoutState = item;
							break;
						}
					}
				}
				if (mordexRowLayoutState == null && _headerRowLayouts != null)
				{
					foreach (MxRowLayoutState headerRowLayout in _headerRowLayouts)
					{
						if (AreSameLogicalRow(headerRowLayout._gridRow, gridRow))
						{
							mordexRowLayoutState = headerRowLayout;
							break;
						}
					}
				}
				if (mordexRowLayoutState == null && _footerRowLayouts != null)
				{
					foreach (MxRowLayoutState footerRowLayout in _footerRowLayouts)
					{
						if (AreSameLogicalRow(footerRowLayout._gridRow, gridRow))
						{
							mordexRowLayoutState = footerRowLayout;
							break;
						}
					}
				}
				if (mordexRowLayoutState != null)
				{
					Rectangle rc = new Rectangle(base.ClientRectangle.Left, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
					rc.Inflate(1, 1);
					Invalidate(rc);
				}
			}
			else
			{
				Invalidate();
			}
		}

		private void InvalidateSelectedRows()
		{
			if (!_isLayoutValid)
			{
				return;
			}
			if (View == View.TableView)
			{
				if (_bodyRowLayouts != null)
				{
					foreach (MxRowLayoutState item in _bodyRowLayouts)
					{
						if (item._gridRow != null && item._gridRow.Selected)
						{
							Rectangle rc = new Rectangle(base.ClientRectangle.Left, item.GetLocation().Y, base.ClientRectangle.Width, item._rowHeight);
							Invalidate(rc);
						}
					}
				}
				if (_headerRowLayouts == null)
				{
					return;
				}
				{
					foreach (MxRowLayoutState headerRowLayout in _headerRowLayouts)
					{
						if (headerRowLayout._gridRow != null && headerRowLayout._gridRow.Selected)
						{
							Rectangle rectangle = new Rectangle(base.ClientRectangle.Left, headerRowLayout.GetLocation().Y, base.ClientRectangle.Width, headerRowLayout._rowHeight);
							Invalidate(rectangle);
						}
					}
					return;
				}
			}
			Invalidate();
		}

		internal void InvalidateTableRows(GridTable gridTable)
		{
			foreach (GridRow item in _rowCache)
			{
				if (gridTable == null || item.Table == gridTable)
				{
					item.DisposeFormatStyles();
					item._isFormatting = false;
				}
			}
			RefreshRowCountAndInvalidate();
		}

		// A value of the row was edited: the totals above it change by this row only.
		internal void InvalidateTotalAndGroupRows(GridRow editedRow)
		{
			editedRow?.GetLogicalRow()?.NoteValueChanged();
			InvalidateTotalAndGroupRows();
		}

		internal void InvalidateTotalAndGroupRows()
		{
			bool condition = false;
			GridRow[] array = new GridRow[_rowCache.Count];
			int value = 0;
			foreach (GridRow item in _rowCache)
			{
				array[value] = item;
				value++;
			}
			GridRow[] otherArray = array;
			foreach (GridRow gridRow in otherArray)
			{
				if (gridRow.RowType == RowType.GroupFooter || gridRow.RowType == RowType.TotalRow)
				{
					gridRow.RecalculateTotals();
					condition = true;
				}
				if (gridRow.RowType == RowType.GroupHeader)
				{
					gridRow._isLayoutDirty = true;
					condition = true;
				}
			}
			_rowCache.ReleaseStaleRows();
			if (condition)
			{
				RefreshRowCountAndInvalidate();
			}
		}

		protected virtual void OnDrawGridArea(DrawGridAreaEventArgs e)
		{
			if (base.Events[_eventKeyDrawGridArea] is DrawGridAreaEventHandler drawGridAreaEventHandler)
			{
				drawGridAreaEventHandler(this, e);
			}
		}

		void IFormatStyleOwner.InvalidateFormatStyleLayout()
		{
			RefreshRowCountAndInvalidate();
		}

		// The gradient of a selected row in the theme (Office 2003 and 2007), or null.
		internal System.Drawing.Drawing2D.ColorBlend GetThemedSelectionBlend()
		{
			return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance().SelectionBlend;
		}

		internal Color GetPredefinedStyleGradientColor(PredefinedStyle predefinedStyle)
		{
			switch (predefinedStyle)
			{
			case PredefinedStyle.SelectedFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.HighlightColor);
			case PredefinedStyle.GroupByBoxFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGroupByBoxStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.GroupByBoxGradient);
			case PredefinedStyle.GroupByBoxInfoFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGroupByBoxStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.GroupByBoxInfoGradient);
			case PredefinedStyle.HeaderFormatStyle:
				return _gridControlStyle.GetLayoutState().GetHeadersStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.HeaderGradient);
			case PredefinedStyle.GroupRowFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGroupRowsStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.GroupRowGradient);
			case PredefinedStyle.CardCaptionFormatStyle:
				return _gridControlStyle.GetLayoutState().GetCardsStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.CardCaptionGradient);
			default:
				return Color.Empty;
			}
		}

		internal void DrawLayoutItemInBounds(Graphics graphics, GridGroupLayoutState gridGroupLayoutState, GridFormatStyle gridFormatStyle, Rectangle rectangle)
		{
			SetPaintClipRectangle(rectangle);
			DrawLayoutItem(graphics, gridGroupLayoutState, gridFormatStyle, rectangle, rectangle);
		}

		// The part of the item that its gridlines leave free; texts inside it can be held back until the row is done.
		private Rectangle GetTextFreeArea(GridGroupLayoutState item, GridFormatStyle style, Rectangle bounds)
		{
			if (((IGridEnvironment)_gridControlStyle).IsPrinting())
			{
				return Rectangle.Empty;
			}
			Rectangle free = item.DeflateForGridlines(bounds, item.IsRightToLeft(RightToLeft == RightToLeft.Yes));
			return free;
		}

		internal bool DrawLayoutItem(Graphics graphics, GridGroupLayoutState item, GridFormatStyle style, Rectangle bounds, Rectangle otherBounds)
		{
			if (!item._isVisible)
			{
				return true;
			}
			UpdateCellButtonVisibility(item);
			CellRendererBase renderer = _gridControlStyle.GetLayoutState().GetRenderer(item);
			bool wasTableView = _wasTableView;
			if (item._type == LayoutItemType.PreviewRow)
			{
				if (RightToLeft == RightToLeft.Yes)
				{
					if (bounds.X > base.ClientRectangle.X)
					{
						bounds.X = base.ClientRectangle.X;
					}
				}
				else if (bounds.X < base.ClientRectangle.X)
				{
					bounds.X = base.ClientRectangle.X;
				}
				otherBounds.X = base.ClientRectangle.X;
				otherBounds.Width = base.ClientRectangle.Width;
			}
			_gridControlStyle._textBatch.FlushIfOverlaps(Rectangle.Inflate(bounds, 2, 2));
			if (IsItemOwnerDrawn(item))
			{
				wasTableView = true;
			}
			else
			{
				Rectangle rectangle = ExpandRectForGridLines(item, bounds);
				Rectangle nextBounds = Rectangle.Intersect(otherBounds, rectangle);
				if (nextBounds != rectangle)
				{
					wasTableView = true;
				}
			}
			if (!wasTableView && (style.IsTranslucent() || item._type == LayoutItemType.GroupHeaderCell || item._type == LayoutItemType.GroupHeaderRow))
			{
				wasTableView = true;
			}
			GridFormatStyle itemStyle = null;
			if (renderer != null)
			{
				if (renderer._alignsText)
				{
					wasTableView = true;
				}
				Region region = null;
				Region clip = null;
				if (wasTableView)
				{
					Rectangle otherIntersect = Rectangle.Intersect(otherBounds, ExpandRectForGridLines(item, bounds));
					if (style.IsTranslucent() || item._type == LayoutItemType.GroupHeaderCell || item._type == LayoutItemType.GroupHeaderRow)
					{
						otherIntersect = Rectangle.Intersect(otherIntersect, GetPaintClipRectangle());
					}
					if (otherIntersect.IsEmpty)
					{
						return true;
					}
					region = new Region(otherIntersect);
					clip = graphics.Clip;
					graphics.Clip = region;
				}
				bool isLayoutItemSelected = IsLayoutItemSelected(item);
				bool isSelectionVisible = _isSelectionVisible;
				switch (item._type)
				{
				case LayoutItemType.RowHeader:
					itemStyle = GetRowHeaderStyle(item.GetOwnerTable());
					break;
				case LayoutItemType.NewRowSeparator:
				case LayoutItemType.TotalRowSeparator:
					itemStyle = GetHeaderStyle(item.GetOwnerTable());
					break;
				case LayoutItemType.GroupIndent:
					itemStyle = ResolveGroupRowFormatStyle(item, isLayoutItemSelected);
					break;
				}
				if (itemStyle != null)
				{
					style = itemStyle;
				}
				bool condition = false;
				TextLayoutRenderer textRenderer = renderer.GetTextRenderer(item, style);
				if (IsItemOwnerDrawn(item))
				{
					Font font = (Font)renderer.GetStyleFont(style).Clone();
					DrawGridAreaEventArgs e = new DrawGridAreaEventArgs(graphics, bounds, font, style, textRenderer.GetStringFormat(), item, this);
					e._itemRenderer = renderer;
					e._isFocused = isSelectionVisible;
					e._isBackgroundPainted = isLayoutItemSelected;
					_gridControlStyle._textBatch.Flush();
					OnDrawGridArea(e);
					font?.Dispose();
					if (e.Handled)
					{
						condition = true;
					}
				}
				if (!condition)
				{
					renderer.PaintBackground(graphics, item, style, bounds, isLayoutItemSelected, isSelectionVisible);
				}
				Rectangle foregroundRect = renderer.GetForegroundRect(bounds, item, style);
				if ((item._type == LayoutItemType.Cell || item._type == LayoutItemType.CardCell) && item._gridGroupLayoutStateOwner.GridRow != null && item._gridGroupLayoutStateOwner.GridRow.RowType == RowType.NewRecord && item._gridGroupLayoutStateOwner.GridRow != _row)
				{
					condition = true;
				}
				if (!condition)
				{
					if (!IsItemOwnerDrawn(item) && renderer.TextMayBeDeferred(style) && !(GridTextBatch.CellsOnly && (renderer is HeaderCellRendererBase || renderer is ListCellRenderer)))
					{
						_gridControlStyle._textBatch.SetFreeArea(renderer.GetTextFreeArea(item, bounds, GetTextFreeArea(item, style, bounds)), renderer is HeaderCellRendererBase);
					}
					renderer.PaintForeground(graphics, item, style, foregroundRect, isLayoutItemSelected, isSelectionVisible);
					_gridControlStyle._textBatch.SetFreeArea(Rectangle.Empty);
				}
				renderer.PaintAddornments(graphics, item, style, bounds);
				renderer._gridlineRenderer = _gridControlStyle.GetLayoutState().GetGridLinePenRenderer();
				renderer.PaintGridlines(graphics, item, bounds);
				if (!item.HasSize())
				{
					switch (item._type)
					{
					case LayoutItemType.PreviewRow:
						textRenderer.SetWordWrap(true);
						if (item.GetOwnerTable().ComputeCellToolTip() == CellToolTip.TruncatedText)
						{
							item.SetCachedSize(GridHelper.LayoutImageAndText(graphics, null, item.GetDisplayText(), null, foregroundRect, style.ImageHorizontalAlignment, style.ImageVerticalAlignment, textRenderer, style.ResolveFont(Font), RightToLeft == RightToLeft.Yes));
						}
						break;
					case LayoutItemType.Cell:
					case LayoutItemType.CardCell:
					{
						if (item._column.ComputeCellToolTip() != CellToolTip.TruncatedText)
						{
							break;
						}
						GridCell gridCell = item.GetCell();
						if (gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText || gridCell.CellDisplayType == ColumnType.Link)
						{
							Image otherImage = item.GetDisplayImage(null);
							if (otherImage == null)
							{
								otherImage = GetImageByKey(item.GetImageKey());
							}
							if (otherImage == null)
							{
								otherImage = GetImageByIndex(item.GetImageIndex());
							}
							item.SetCachedSize(GridHelper.LayoutImageAndText(graphics, item._column, item.GetDisplayText(), otherImage, foregroundRect, style.ImageHorizontalAlignment, style.ImageVerticalAlignment, textRenderer, style.ResolveFont(Font), RightToLeft == RightToLeft.Yes));
						}
						break;
					}
					case LayoutItemType.TotalRowCell:
					case LayoutItemType.GroupTotalCell:
					case LayoutItemType.FilterRowCell:
						if (item._column.ComputeCellToolTip() == CellToolTip.TruncatedText && (item._column.ColumnType == ColumnType.Text || item._column.ColumnType == ColumnType.ImageAndText))
						{
							if (item._type == LayoutItemType.GroupTotalCell || item._type == LayoutItemType.TotalRowCell)
							{
								textRenderer.SetWordWrap(false);
							}
							Image image = item.GetDisplayImage(null);
							if (image == null)
							{
								image = GetImageByKey(item.GetImageKey());
							}
							if (image == null)
							{
								image = GetImageByIndex(item.GetImageIndex());
							}
							item.SetCachedSize(GridHelper.LayoutImageAndText(graphics, item._column, item.GetDisplayText(), image, foregroundRect, style.ImageHorizontalAlignment, style.ImageVerticalAlignment, textRenderer, style.ResolveFont(Font), RightToLeft == RightToLeft.Yes));
						}
						break;
					default:
						item.SetCachedSize(new Size(0, 0));
						break;
					}
				}
				if (wasTableView)
				{
					graphics.Clip = clip;
					clip.Dispose();
					region.Dispose();
					region = null;
				}
			}
			if (item._children != null)
			{
				for (int value = item._children.Length - 1; value >= 0; value--)
				{
					PaintLayoutItemAtBounds(graphics, item._children[value], style, otherBounds);
				}
			}
			itemStyle?.DisposeResources();
			return true;
		}

		protected override void OnUseCompatibleTextRenderingChanged()
		{
			base.OnUseCompatibleTextRenderingChanged();
			RefreshRowCountAndInvalidate();
		}

		private void InvalidateDeferred()
		{
			if (_isBufferStale && GetPendingInvalidRect().IsEmpty)
			{
				Invalidate();
				SetPendingInvalidRect(Rectangle.Empty);
				_isBufferStale = true;
			}
			else
			{
				Invalidate();
			}
		}

		protected override void OnInvalidated(InvalidateEventArgs e)
		{
			_isBufferStale = false;
			Rectangle rectangle = Rectangle.Intersect(base.ClientRectangle, e.InvalidRect);
			if (GetPendingInvalidRect().IsEmpty)
			{
				SetPendingInvalidRect(rectangle);
			}
			else
			{
				SetPendingInvalidRect(Rectangle.Union(GetPendingInvalidRect(), rectangle));
			}
			if (_isRenderingPartial)
			{
				SetPaintClipRectangle(GetPendingInvalidRect());
			}
			base.OnInvalidated(e);
		}
	}
}
