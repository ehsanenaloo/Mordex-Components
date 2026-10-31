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
		internal void RefreshRowCountAndInvalidate()
		{
			AdjustRowAfterCountChange();
			if (!_isPaintInProgress)
			{
				_isLayoutValid = false;
				Invalidate();
			}
		}

		internal void RefreshRowCountAndForceInvalidate()
		{
			AdjustRowAfterCountChange();
			_isLayoutValid = false;
			Invalidate();
		}

		private Rectangle GetPaintClipRectangle()
		{
			return _partialPaintBounds;
		}

		private void SetPaintClipRectangle(Rectangle gridRectangle)
		{
			_partialPaintBounds = gridRectangle;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			_ = DateTime.Now.Ticks;
			if (e.ClipRectangle.IsEmpty || _isPaintInProgress)
			{
				base.OnPaint(e);
				return;
			}
			_isPaintInProgress = true;
			try
			{
				if (!_isBufferStale || _bufferedGraphics == null)
				{
					if (_bufferedGraphics == null || _bufferNeedsRecreate)
					{
						AllocateBufferedGraphics();
						_bufferNeedsRecreate = false;
					}
					if (RootTable == null)
					{
						base.SetVerticalScrollRequired(false);
						base.SetHorizontalScrollRequired(false);
					}
					FlushPendingPaint();
				}
				if (_isBufferStale)
				{
					_bufferedGraphics.Render(e.Graphics);
				}
				try
				{
					if (IsCellVisible(Row, CurrentColumn) && _gridColumnContainerControl != null)
					{
						PositionCellEditorControl(false);
					}
				}
				catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is InvalidOperationException || ex is NullReferenceException)
				{
					MoveFirst();
				}
				base.OnPaint(e);
			}
			finally
			{
				_isPaintInProgress = false;
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		internal void DrawControlBackground(Graphics graphics, Rectangle bounds, Rectangle clipBounds, bool skipOwnerDraw, bool fillBackColor)
		{
			Region clip = graphics.Clip;
			Region region = new Region(clipBounds);
			graphics.Clip = region;
			try
			{
				if (!skipOwnerDraw && (_ownerDrawnAreas & GridOwnerDrawnArea.Background) != GridOwnerDrawnArea.None)
				{
					DrawGridAreaEventArgs e = new DrawGridAreaEventArgs(graphics, bounds, null, this, OwnerDrawnArea.ControlBackground);
					OnDrawGridArea(e);
					if (e.Handled)
					{
						return;
					}
				}
				if (fillBackColor)
				{
					Brush brush = GdiCache.Brush(base.BackColor);
					graphics.FillRectangle(brush, clipBounds);
				}
				GridHelper.DrawBackgroundImage(graphics, BackgroundImage, _backgroundImageDrawMode, bounds, clipBounds, 255, AlphaMode.Opaque);
				if (_watermarkImage.Image != null)
				{
					_watermarkImage.DrawWatermark(graphics, bounds);
				}
			}
			finally
			{
				graphics.Clip = clip;
				region.Dispose();
				clip.Dispose();
			}
		}

		private void UpdateWatermarkImage()
		{
			if (_watermarkImage != null)
			{
				_watermarkImage.DetachFromGrid();
			}
			_watermarkImage = new GridWatermarkImage(this);
		}

		private GridPassStyleCache _passStyles;

		internal bool UsesAlternatingColors => _alternatingColors;

		private void PaintControl(Graphics graphics)
		{
			bool ownsCache = _passStyles == null;
			if (ownsCache)
			{
				GridFontCache.BeginDeferral();
				_passStyles = new GridPassStyleCache();
			}
			try
			{
				PaintControlCore(graphics);
			}
			finally
			{
				if (ownsCache)
				{
					_passStyles.Release();
					_passStyles = null;
					GridFontCache.EndDeferral();
				}
			}
		}

		private void PaintControlCore(Graphics graphics)
		{
			while (!_isLayoutValid)
			{
				CalculateLayoutStates(graphics);
			}
			PrepareControlStyleLayout();
			DrawControlBackground(graphics, base.ClientRectangle, GetPaintClipRectangle(), false, true);
			_gridControlStyle._textBatch.Begin(graphics);
			try
			{
				if (_view == View.TableView)
				{
					DrawTableViewContent(graphics);
				}
				else
				{
					DrawCardViewCards(graphics);
				}
			}
			finally
			{
				_gridControlStyle._textBatch.End();
			}
			if (_pressedHeaderItem != null && !_pressedHeaderItem.UsesVisualStyle(_gridControlStyle.GetLayoutState()))
			{
				ToggleDragSourceHighlight(graphics);
			}
			base.DrawControlBorder(graphics);
			if (RootTable != null)
			{
				RootTable.DisposeGridLinePen();
			}
			ResetPaintResources();
			_isBufferStale = true;
		}

		private void DrawCardViewCards(Graphics graphics)
		{
			GridFormatStyle gridFormatStyle = GetCardCaptionStyle();
			if (_cardRowLayouts != null)
			{
				foreach (GridTableLayoutState item in _cardRowLayouts)
				{
					Rectangle a = AdjustRectangleToViewport(item.GetBounds(), true);
					if (Rectangle.Intersect(a, GetPaintClipRectangle()).IsEmpty)
					{
						continue;
					}
					DrawCard(graphics, item, gridFormatStyle);
					_gridControlStyle._textBatch.Flush();
					if (_currentColumn == null || item.GetRowPosition() != Row)
					{
						continue;
					}
					foreach (GridGroupLayoutState arrayListItem in item._arrayList)
					{
						if (arrayListItem._type == LayoutItemType.CardCell && arrayListItem._column == _currentColumn)
						{
							PaintCellFocusRectangle(graphics, arrayListItem);
							break;
						}
					}
				}
			}
			if (_view != View.SingleCard)
			{
				Brush brush = GdiCache.Brush(_gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.Gridline));
				Rectangle[] array = _cardBounds;
				foreach (Rectangle rectangle in array)
				{
					graphics.FillRectangle(brush, AdjustRectangleToViewport(rectangle, false));
				}
			}
			gridFormatStyle.DisposeResources();
			gridFormatStyle = null;
			if (RowCount == 0)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				if (_view == View.CardView)
				{
					clientRectangle.Width = CardWidth;
				}
				DrawEmptyGridInfo(graphics, clientRectangle);
			}
		}

		private void DrawTableViewContent(Graphics graphics)
		{
			DrawGroupByBoxBackground(graphics, false);
			if (_headerRowLayouts != null)
			{
				foreach (MxRowLayoutState item in _headerRowLayouts)
				{
					DrawRowLayout(graphics, item, base.ClientRectangle);
				}
			}
			if (_bodyRowLayouts != null)
			{
				Rectangle rectangle = GetRowAreaRectangle();
				for (int i = 0; i < _bodyRowLayouts.GetCount(); i++)
				{
					MxRowLayoutState mordexRowLayoutState = _bodyRowLayouts.GetRowAt(i);
					if (mordexRowLayoutState._rowType == LayoutRowType.Space)
					{
						if (i < _bodyRowLayouts.GetCount() - 1)
						{
							DrawSpaceRow(graphics, mordexRowLayoutState, _bodyRowLayouts.GetRowAt(i + 1), rectangle);
						}
						else
						{
							DrawSpaceRow(graphics, mordexRowLayoutState, null, rectangle);
						}
					}
					else
					{
						DrawRowLayout(graphics, mordexRowLayoutState, rectangle);
					}
				}
			}
			if (_footerRowLayouts != null)
			{
				Rectangle rectangle = Rectangle.FromLTRB(base.ClientRectangle.Left, _bodyTopY, base.ClientRectangle.Right, base.ClientRectangle.Bottom);
				foreach (MxRowLayoutState footerRowLayout in _footerRowLayouts)
				{
					DrawRowLayout(graphics, footerRowLayout, rectangle);
				}
			}
			if (RowCount == 0)
			{
				DrawEmptyGridInfo(graphics, GetRowAreaRectangle());
			}
		}

		private void DrawEmptyGridInfo(Graphics graphics, Rectangle rectangle)
		{
			if (BuiltInTexts[GridBuiltInText.EmptyGridInfo].Length > 0)
			{
				rectangle.Inflate(-2, -2);
				Region region = new Region(GetPaintClipRectangle());
				Region clip = graphics.Clip;
				graphics.Clip = region;
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!base.UseCompatibleTextRendering);
				switch (EmptyGridInfoAlignment)
				{
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight:
					textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Far);
					break;
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight:
					textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
					break;
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopRight:
					textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Near);
					break;
				}
				switch (EmptyGridInfoAlignment)
				{
				case ContentAlignment.TopCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.BottomCenter:
					textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
					break;
				case ContentAlignment.TopLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.BottomLeft:
					textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					break;
				case ContentAlignment.TopRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.BottomRight:
					textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
					break;
				}
				GridFormatStyle gridFormatStyle = GetRowStyle(RootTable);
				textLayoutRenderer.DrawTextWithColor(graphics, BuiltInTexts[GridBuiltInText.EmptyGridInfo], rectangle, gridFormatStyle.ResolveFont(Font), gridFormatStyle.ForeColor);
				textLayoutRenderer.ReleaseStringFormat();
				gridFormatStyle.DisposeResources();
				graphics.Clip = clip;
				clip.Dispose();
				region.Dispose();
			}
		}

		private void DrawNewRowInfoText(Graphics graphics, MxRowLayoutState mordexRowLayoutState)
		{
			_gridControlStyle._textBatch.Flush();
			if (mordexRowLayoutState.GridRow == _row || mordexRowLayoutState.LayoutTable.NewRowInfoText.Length <= 0)
			{
				return;
			}
			GridGroupLayoutState gridGroupLayoutState = null;
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._type == LayoutItemType.Cell)
				{
					GridCell gridCell = item.GetCell();
					if (gridCell != null && gridCell.EditType != EditType.NoEdit && (gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText))
					{
						gridGroupLayoutState = item;
						break;
					}
				}
			}
			if (gridGroupLayoutState != null)
			{
				Rectangle rectangle = gridGroupLayoutState.GetAbsoluteItemRect();
				rectangle.Width = mordexRowLayoutState.GetContentBounds().Right - rectangle.Left - 1;
				rectangle = AdjustRectangleToViewport(rectangle, gridGroupLayoutState._isHorizontallyScrolled);
				Rectangle bounds = Rectangle.Intersect(rectangle, GetPaintClipRectangle());
				if (!bounds.IsEmpty)
				{
					Region region = new Region(bounds);
					Region clip = graphics.Clip;
					graphics.Clip = region;
					TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!base.UseCompatibleTextRendering);
					textLayoutRenderer.UpdateRightToLeft(RightToLeft);
					textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					textLayoutRenderer.UpdateTrimming(StringTrimming.EllipsisCharacter);
					textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
					textLayoutRenderer.DrawTextWithColor(graphics, mordexRowLayoutState.LayoutTable.NewRowInfoText, rectangle, Font, _gridControlStyle.ResolveControlDarkColor());
					textLayoutRenderer.ReleaseStringFormat();
					graphics.Clip = clip;
					clip.Dispose();
					region.Dispose();
				}
			}
		}

		private void DrawFilterRowInfoText(Graphics graphics, MxRowLayoutState mordexRowLayoutState)
		{
			_gridControlStyle._textBatch.Flush();
			if (mordexRowLayoutState.GridRow == null || mordexRowLayoutState.GridRow == _row || !_showRowPreview || GetFilterRowInfoText().Length <= 0)
			{
				return;
			}
			GridGroupLayoutState gridGroupLayoutState = null;
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._type == LayoutItemType.FilterRowCell && GridColumnContainerControl.GetFilterEditType(item._column) != EditType.NoEdit && (item._column.ColumnType == ColumnType.Text || item._column.ColumnType == ColumnType.ImageAndText))
				{
					gridGroupLayoutState = item;
					break;
				}
			}
			if (gridGroupLayoutState != null)
			{
				Rectangle rectangle = gridGroupLayoutState.GetAbsoluteItemRect();
				rectangle.Width = mordexRowLayoutState.GetContentBounds().Right - rectangle.Left - 1;
				rectangle = AdjustRectangleToViewport(rectangle, gridGroupLayoutState._isHorizontallyScrolled);
				Rectangle bounds = Rectangle.Intersect(rectangle, GetPaintClipRectangle());
				if (!bounds.IsEmpty)
				{
					Region region = new Region(bounds);
					Region clip = graphics.Clip;
					graphics.Clip = region;
					TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!base.UseCompatibleTextRendering);
					textLayoutRenderer.SetStringAlignment(StringAlignment.Near);
					textLayoutRenderer.UpdateTrimming(StringTrimming.EllipsisCharacter);
					textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
					textLayoutRenderer.DrawTextWithColor(graphics, GetFilterRowInfoText(), rectangle, Font, _gridControlStyle.ResolveControlDarkColor());
					textLayoutRenderer.ReleaseStringFormat();
					graphics.Clip = clip;
					clip.Dispose();
					region.Dispose();
				}
			}
		}

		private void DrawRowLayout(Graphics graphics, MxRowLayoutState mordexRowLayoutState, Rectangle rectangle)
		{
			if (mordexRowLayoutState._gridRow != null && mordexRowLayoutState._gridRow.RowType == RowType.NewRecord && _showErrors)
			{
				if (mordexRowLayoutState._gridRow.DataRow != null)
				{
					mordexRowLayoutState._gridRow._hasTextOverride = false;
					HasRowError(mordexRowLayoutState._gridRow);
					if (mordexRowLayoutState._gridRow.DataRow is IDataErrorInfo || mordexRowLayoutState._gridRow.DataRow is DataRow)
					{
						foreach (GridCell item in (IEnumerable)mordexRowLayoutState._gridRow.Cells)
						{
							UpdateCellError(item);
						}
					}
				}
				else
				{
					foreach (GridCell cell in (IEnumerable)mordexRowLayoutState._gridRow.Cells)
					{
						cell.SetErrorText("");
						if (cell.HasErrorIcon())
						{
							cell.Image = null;
							cell.SetHasErrorIcon(false);
						}
					}
				}
			}
			switch (mordexRowLayoutState._rowType)
			{
			case LayoutRowType.GroupByBox:
				DrawGroupByBoxRow(graphics, mordexRowLayoutState);
				break;
			case LayoutRowType.ColumnHeader:
				DrawHeaderRow(graphics, mordexRowLayoutState, rectangle);
				break;
			case LayoutRowType.GroupTotalRow:
				DrawGroupTotalRow(graphics, mordexRowLayoutState, rectangle);
				break;
			case LayoutRowType.NewRow:
				DrawDataRow(graphics, mordexRowLayoutState, rectangle);
				break;
			case LayoutRowType.DataRow:
			case LayoutRowType.EmptyRow:
				DrawDataRow(graphics, mordexRowLayoutState, rectangle);
				break;
			case LayoutRowType.FilterRow:
				DrawFilterRow(graphics, mordexRowLayoutState, rectangle);
				break;
			case LayoutRowType.GroupHeaderRow:
				DrawGroupRow(graphics, mordexRowLayoutState, rectangle);
				break;
			case LayoutRowType.TotalRow:
				DrawTotalRow(graphics, mordexRowLayoutState, rectangle);
				break;
			}
			_gridControlStyle._textBatch.Flush();
			if (mordexRowLayoutState._gridRow != null && _row != null && mordexRowLayoutState._gridRow.GetLogicalRow() == _row.GetLogicalRow())
			{
				PaintRowFocusRectangle(graphics, mordexRowLayoutState);
			}
		}

		private void DrawSpaceRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, MxRowLayoutState rowLayoutState, Rectangle rectangle)
		{
			Rectangle b = new Rectangle(base.ClientRectangle.X, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
			if (Rectangle.Intersect(GetPaintClipRectangle(), b).IsEmpty)
			{
				return;
			}
			GridFormatStyle gridFormatStyle = new GridFormatStyle();
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				PaintLayoutItemAtBounds(graphics, item, gridFormatStyle, rectangle);
			}
			_gridControlStyle._textBatch.Flush();
		}

		private void DrawTotalRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, Rectangle paintClipRectangle)
		{
			Rectangle b = new Rectangle(base.ClientRectangle.X, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
			if (Rectangle.Intersect(GetPaintClipRectangle(), b).IsEmpty)
			{
				return;
			}
			GridFormatStyle gridFormatStyle = GetTotalRowStyle(mordexRowLayoutState.LayoutTable);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState._gridRow.RowStyle);
			GridFormatStyle otherGridFormatStyle = null;
			if (mordexRowLayoutState._gridRow.Selected)
			{
				if (!IsSingleCellSelection())
				{
					otherGridFormatStyle = GetSelectionFormatStyle();
				}
				else if (SelectionMode != SelectionMode.MultipleCellSelection && (_gridSelectedItemCollection.Count > 1 || _currentColumn == null))
				{
					otherGridFormatStyle = GetSelectionFormatStyle();
				}
			}
			else
			{
				otherGridFormatStyle = null;
			}
			Rectangle rectangle = Rectangle.FromLTRB(GetRowLayoutRightEdge(mordexRowLayoutState), paintClipRectangle.Top, paintClipRectangle.Right, paintClipRectangle.Bottom);
			Rectangle adjustRectangleToViewport = Rectangle.FromLTRB(paintClipRectangle.Left, paintClipRectangle.Top, rectangle.Left, paintClipRectangle.Bottom);
			rectangle = AdjustRectangleToViewport(rectangle, false);
			adjustRectangleToViewport = AdjustRectangleToViewport(adjustRectangleToViewport, false);
			rectangle = Rectangle.Intersect(rectangle, GetPaintClipRectangle());
			adjustRectangleToViewport = Rectangle.Intersect(adjustRectangleToViewport, GetPaintClipRectangle());
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				paintClipRectangle = ((item._type == LayoutItemType.TotalRowSeparator) ? GetPaintClipRectangle() : (IsScrollableLayoutItem(item) ? adjustRectangleToViewport : rectangle));
				Rectangle b2 = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				if (Rectangle.Intersect(paintClipRectangle, b2).IsEmpty)
				{
					continue;
				}
				GridFormatStyle otherApplyOverrides = applyOverrides;
				GridFormatStyle nextApplyOverrides;
				if (item._column != null)
				{
					if (item._column.CellStyle != null)
					{
						otherApplyOverrides = applyOverrides.ApplyOverrides(item._column.CellStyle);
					}
					nextApplyOverrides = otherApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle);
				}
				else
				{
					nextApplyOverrides = otherApplyOverrides;
				}
				GridFormatStyle previousApplyOverrides;
				if (mordexRowLayoutState._gridRow.Selected && (item._type == LayoutItemType.EmptyCell || item._type == LayoutItemType.TotalRowCell))
				{
					previousApplyOverrides = ((mordexRowLayoutState._gridRow != _row) ? nextApplyOverrides.ApplyOverrides(otherGridFormatStyle) : ((_currentColumn != null && item._column == _currentColumn) ? nextApplyOverrides : nextApplyOverrides.ApplyOverrides(otherGridFormatStyle)));
					if (SelectionMode == SelectionMode.MultipleCellSelection && item.GetCell() != null && item.GetCell().Selected && (EditMode == EditMode.EditOff || item._gridGroupLayoutStateOwner.GridRow != CurrentRow || item._column != CurrentColumn))
					{
						previousApplyOverrides = previousApplyOverrides.ApplyOverrides(GetSelectionFormatStyle());
					}
				}
				else
				{
					previousApplyOverrides = nextApplyOverrides;
				}
				PaintLayoutItemAtBounds(graphics, item, previousApplyOverrides, paintClipRectangle);
				if (item._column != null && item._type != LayoutItemType.PreviewRow)
				{
					if (previousApplyOverrides != nextApplyOverrides)
					{
						previousApplyOverrides.DisposeResources();
					}
					if (nextApplyOverrides != otherApplyOverrides)
					{
						nextApplyOverrides.DisposeResources();
					}
					if (otherApplyOverrides != applyOverrides)
					{
						otherApplyOverrides.DisposeResources();
					}
				}
			}
		}

		private void DrawGroupRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, Rectangle bounds)
		{
			if (mordexRowLayoutState.GridRow == null)
			{
				return;
			}
			GridFormatStyle groupRowStyle = GetGroupRowStyle(mordexRowLayoutState.LayoutTable);
			GridFormatStyle gridFormatStyle = groupRowStyle.ApplyOverrides(mordexRowLayoutState.GridRow.Group.GetGroupRowStyle());
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState._gridRow.RowStyle);
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.GetCachedFormatStyle());
			Rectangle b = new Rectangle(base.ClientRectangle.X, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
			if (Rectangle.Intersect(GetPaintClipRectangle(), b).IsEmpty)
			{
				return;
			}
			GridFormatStyle nextApplyOverrides;
			if (mordexRowLayoutState._gridRow.Selected)
			{
				GridFormatStyle otherGridFormatStyle = GetSelectionFormatStyle();
				nextApplyOverrides = otherApplyOverrides.ApplyOverrides(otherGridFormatStyle);
			}
			else
			{
				nextApplyOverrides = otherApplyOverrides;
			}
			Rectangle rectangle = Rectangle.FromLTRB(GetRowLayoutRightEdge(mordexRowLayoutState), bounds.Top, bounds.Right, bounds.Bottom);
			Rectangle adjustRectangleToViewport = Rectangle.FromLTRB(bounds.Left, bounds.Top, rectangle.Left, bounds.Bottom);
			rectangle = AdjustRectangleToViewport(rectangle, false);
			adjustRectangleToViewport = AdjustRectangleToViewport(adjustRectangleToViewport, false);
			rectangle = Rectangle.Intersect(rectangle, GetPaintClipRectangle());
			adjustRectangleToViewport = Rectangle.Intersect(adjustRectangleToViewport, GetPaintClipRectangle());
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._type == LayoutItemType.GroupHeaderCell)
				{
					if (item.GetDisplayText().Length <= 0)
					{
						continue;
					}
					Rectangle b2 = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
					if (!Rectangle.Intersect(GetPaintClipRectangle(), b2).IsEmpty)
					{
						GridFormatStyle previousApplyOverrides = null;
						GridFormatStyle firstApplyOverrides = null;
						GridFormatStyle nextGridFormatStyle = null;
						if (mordexRowLayoutState._gridRow.Cells[item._column.Index]._groupHeaderTotal is GridGroupHeaderTotal gridGroupHeaderTotal)
						{
							previousApplyOverrides = otherApplyOverrides.ApplyOverrides(gridGroupHeaderTotal.FormatStyle);
							firstApplyOverrides = previousApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle);
							nextGridFormatStyle = ((!mordexRowLayoutState._gridRow.Selected) ? firstApplyOverrides : firstApplyOverrides.ApplyOverrides(GetSelectionFormatStyle()));
						}
						else
						{
							firstApplyOverrides = otherApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle);
							nextGridFormatStyle = ((!mordexRowLayoutState._gridRow.Selected) ? firstApplyOverrides : firstApplyOverrides.ApplyOverrides(GetSelectionFormatStyle()));
						}
						if (!IsScrollableLayoutItem(item))
						{
							PaintLayoutItemAtBounds(graphics, item, nextGridFormatStyle, rectangle);
						}
						else
						{
							PaintLayoutItemAtBounds(graphics, item, nextGridFormatStyle, adjustRectangleToViewport);
						}
					}
				}
				else
				{
					Rectangle b3 = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
					if (!Rectangle.Intersect(GetPaintClipRectangle(), b3).IsEmpty)
					{
						PaintLayoutItemAtBounds(graphics, item, nextApplyOverrides, bounds);
					}
				}
			}
		}

		private void DrawDataRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, Rectangle paintClipRectangle)
		{
			GridGroupLayoutState gridGroupLayoutState = null;
			Rectangle b = new Rectangle(base.ClientRectangle.X, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
			if (!Rectangle.Intersect(GetPaintClipRectangle(), b).IsEmpty)
			{
				GridFormatStyle gridFormatStyle = null;
				GridFormatStyle applyOverrides = null;
				GridFormatStyle otherApplyOverrides = null;
				GridFormatStyle otherGridFormatStyle = null;
				if (mordexRowLayoutState._rowType != LayoutRowType.NewRow)
				{
					gridFormatStyle = _passStyles != null ? _passStyles.GetRowStyle(this, mordexRowLayoutState.LayoutTable, mordexRowLayoutState.GetRowPosition()) : GetAlternatingRowStyle(mordexRowLayoutState.LayoutTable, mordexRowLayoutState.GetRowPosition());
				}
				else if (mordexRowLayoutState.LayoutTable.ComputeNewRowPosition() == NewRowPosition.TopRow)
				{
					gridFormatStyle = GetNewRowStyle(mordexRowLayoutState.LayoutTable);
				}
				else
				{
					gridFormatStyle = GetAlternatingRowStyle(mordexRowLayoutState.LayoutTable, mordexRowLayoutState.GetRowPosition());
					applyOverrides = gridFormatStyle.ApplyOverrides(_gridGridFormatStyle);
					gridFormatStyle = applyOverrides;
					if (mordexRowLayoutState.LayoutTable != null && mordexRowLayoutState.LayoutTable.NewRowFormatStyle != null)
					{
						otherApplyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState.LayoutTable.NewRowFormatStyle);
						gridFormatStyle = otherApplyOverrides;
					}
				}
				if (mordexRowLayoutState._gridRow.Selected)
				{
					if (!IsSingleCellSelection())
					{
						otherGridFormatStyle = GetSelectionFormatStyle();
					}
					else if (SelectionMode != SelectionMode.MultipleCellSelection && (_gridSelectedItemCollection.Count > 1 || _currentColumn == null))
					{
						otherGridFormatStyle = GetSelectionFormatStyle();
					}
				}
				GridFormatStyle nextApplyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState._gridRow.RowStyle);
				GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.GetCachedFormatStyle());
				GridFormatStyle firstApplyOverrides = null;
				GridFormatStyle secondApplyOverrides = null;
				GridFormatStyle thirdApplyOverrides = null;
				GridFormatStyle fourthApplyOverrides = null;
				GridFormatStyle formatStyle = nextApplyOverrides;
				Rectangle rectangle = Rectangle.FromLTRB(GetRowLayoutRightEdge(mordexRowLayoutState), paintClipRectangle.Top, paintClipRectangle.Right, paintClipRectangle.Bottom);
				Rectangle adjustRectangleToViewport = Rectangle.FromLTRB(paintClipRectangle.Left, paintClipRectangle.Top, rectangle.Left, paintClipRectangle.Bottom);
				rectangle = AdjustRectangleToViewport(rectangle, false);
				adjustRectangleToViewport = AdjustRectangleToViewport(adjustRectangleToViewport, false);
				rectangle = Rectangle.Intersect(rectangle, GetPaintClipRectangle());
				adjustRectangleToViewport = Rectangle.Intersect(adjustRectangleToViewport, GetPaintClipRectangle());
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					if (item._type == LayoutItemType.PreviewRow)
					{
						gridGroupLayoutState = item;
					}
					paintClipRectangle = ((item._type == LayoutItemType.NewRowSeparator) ? GetPaintClipRectangle() : (IsScrollableLayoutItem(item) ? adjustRectangleToViewport : rectangle));
					if (item == gridGroupLayoutState)
					{
						continue;
					}
					Rectangle b2 = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
					if (Rectangle.Intersect(GetPaintClipRectangle(), b2).Height <= 0)
					{
						continue;
					}
					if (item._column != null && item._type != LayoutItemType.PreviewRow)
					{
						formatStyle = ((!item.IsCellItem() || item.GetCell() == null || item.GetCell().CellDisplayType != ColumnType.Link) ? previousApplyOverrides : previousApplyOverrides.ApplyOverrides(_linkFormatStyle));
						firstApplyOverrides = (_passStyles != null && formatStyle == previousApplyOverrides && formatStyle._isPassShared) ? _passStyles.GetCellStyle(formatStyle, item._column) : formatStyle.ApplyOverrides(item._column.CellStyle);
						secondApplyOverrides = firstApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].GetEffectiveFormatStyle());
						thirdApplyOverrides = secondApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle);
					}
					else
					{
						thirdApplyOverrides = previousApplyOverrides;
					}
					GridFormatStyle otherFormatStyle = ((mordexRowLayoutState._rowType != LayoutRowType.DataRow || !_showErrors) ? thirdApplyOverrides : ((!HasRowError(mordexRowLayoutState._gridRow)) ? thirdApplyOverrides : thirdApplyOverrides.ApplyOverrides(_rowWithErrorsFormatStyle)));
					if (mordexRowLayoutState._gridRow.Selected && (item._type == LayoutItemType.Cell || item._type == LayoutItemType.EmptyCell))
					{
						fourthApplyOverrides = ((!AreSameLogicalRow(mordexRowLayoutState._gridRow, _row)) ? otherFormatStyle.ApplyOverrides(otherGridFormatStyle) : ((_currentColumn != null && item._column == _currentColumn && base.ContainsFocus) ? ((!base.ContainsFocus) ? otherFormatStyle : ((FocusCellDisplayMode != FocusCellDisplayMode.UseFocusCellFormatStyle) ? otherFormatStyle.ApplyOverrides(_selectedFormatStyle) : otherFormatStyle.ApplyOverrides(_focusCellFormatStyle))) : otherFormatStyle.ApplyOverrides(otherGridFormatStyle)));
						if (SelectionMode == SelectionMode.MultipleCellSelection && item.GetCell() != null && item.GetCell().Selected && (EditMode == EditMode.EditOff || item._gridGroupLayoutStateOwner.GridRow != CurrentRow || item._column != CurrentColumn))
						{
							fourthApplyOverrides = fourthApplyOverrides.ApplyOverrides(GetSelectionFormatStyle());
						}
					}
					else
					{
						fourthApplyOverrides = otherFormatStyle;
					}
					PaintLayoutItemAtBounds(graphics, item, fourthApplyOverrides, paintClipRectangle);
					if (fourthApplyOverrides != otherFormatStyle)
					{
						fourthApplyOverrides.DisposeResources();
					}
					if (otherFormatStyle != null && otherFormatStyle != thirdApplyOverrides)
					{
						otherFormatStyle.DisposeResources();
					}
					if (thirdApplyOverrides != null && thirdApplyOverrides != previousApplyOverrides)
					{
						thirdApplyOverrides.DisposeResources();
					}
					if (secondApplyOverrides != null && secondApplyOverrides != firstApplyOverrides)
					{
						secondApplyOverrides.DisposeResources();
					}
					if (firstApplyOverrides != null && firstApplyOverrides != formatStyle)
					{
						firstApplyOverrides.DisposeResources();
					}
					if (formatStyle != null && formatStyle != previousApplyOverrides)
					{
						formatStyle.DisposeResources();
					}
				}
				previousApplyOverrides?.DisposeResources();
				nextApplyOverrides?.DisposeResources();
				gridFormatStyle?.DisposeResources();
				if (mordexRowLayoutState._rowType == LayoutRowType.NewRow)
				{
					DrawNewRowInfoText(graphics, mordexRowLayoutState);
				}
			}
			if (gridGroupLayoutState != null)
			{
				GridFormatStyle fifthApplyOverrides = null;
				GridFormatStyle alternatingRowStyle = GetAlternatingRowStyle(mordexRowLayoutState.LayoutTable, mordexRowLayoutState.GetRowPosition());
				GridFormatStyle sixthApplyOverrides = alternatingRowStyle.ApplyOverrides(_previewRowFormatStyle);
				if (gridGroupLayoutState.GetOwnerTable() != null && gridGroupLayoutState.GetOwnerTable().PreviewRowFormatStyle != null)
				{
					fifthApplyOverrides = sixthApplyOverrides.ApplyOverrides(gridGroupLayoutState.GetOwnerTable().PreviewRowFormatStyle);
					sixthApplyOverrides = fifthApplyOverrides;
				}
				GridFormatStyle seventhApplyOverrides = sixthApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.GetCachedRowStyle());
				GridFormatStyle nextGridFormatStyle = ((!mordexRowLayoutState.GridRow.Selected) ? seventhApplyOverrides : seventhApplyOverrides.ApplyOverrides(GetSelectionFormatStyle()));
				PaintLayoutItemAtBounds(graphics, gridGroupLayoutState, nextGridFormatStyle, paintClipRectangle);
				nextGridFormatStyle?.DisposeResources();
				seventhApplyOverrides?.DisposeResources();
				fifthApplyOverrides?.DisposeResources();
				sixthApplyOverrides?.DisposeResources();
				alternatingRowStyle?.DisposeResources();
			}
		}

		private void DrawFilterRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, Rectangle paintClipRectangle)
		{
			GridFormatStyle gridFormatStyle = GetFilterRowStyle(mordexRowLayoutState.LayoutTable);
			Rectangle b = new Rectangle(base.ClientRectangle.X, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
			if (Rectangle.Intersect(GetPaintClipRectangle(), b).IsEmpty)
			{
				return;
			}
			GridFormatStyle otherGridFormatStyle = null;
			if (mordexRowLayoutState.GridRow.Selected)
			{
				if (!IsSingleCellSelection())
				{
					otherGridFormatStyle = GetSelectionFormatStyle();
				}
				else if (SelectionMode != SelectionMode.MultipleCellSelection && (_gridSelectedItemCollection.Count > 1 || _currentColumn == null))
				{
					otherGridFormatStyle = GetSelectionFormatStyle();
				}
			}
			GridFormatStyle nextGridFormatStyle = null;
			GridFormatStyle applyOverrides = null;
			Rectangle rectangle = Rectangle.FromLTRB(GetRowLayoutRightEdge(mordexRowLayoutState), paintClipRectangle.Top, paintClipRectangle.Right, paintClipRectangle.Bottom);
			Rectangle adjustRectangleToViewport = Rectangle.FromLTRB(paintClipRectangle.Left, paintClipRectangle.Top, rectangle.Left, paintClipRectangle.Bottom);
			rectangle = AdjustRectangleToViewport(rectangle, false);
			adjustRectangleToViewport = AdjustRectangleToViewport(adjustRectangleToViewport, false);
			rectangle = Rectangle.Intersect(rectangle, GetPaintClipRectangle());
			adjustRectangleToViewport = Rectangle.Intersect(adjustRectangleToViewport, GetPaintClipRectangle());
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				paintClipRectangle = ((item._type == LayoutItemType.NewRowSeparator) ? GetPaintClipRectangle() : (IsScrollableLayoutItem(item) ? adjustRectangleToViewport : rectangle));
				Rectangle b2 = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				if (Rectangle.Intersect(GetPaintClipRectangle(), b2).IsEmpty)
				{
					continue;
				}
				nextGridFormatStyle = ((item._column == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(item._column.CellStyle));
				if (mordexRowLayoutState._gridRow.Selected && (item._type == LayoutItemType.FilterRowCell || item._type == LayoutItemType.EmptyCell))
				{
					applyOverrides = ((_row != mordexRowLayoutState.GridRow) ? nextGridFormatStyle.ApplyOverrides(otherGridFormatStyle) : ((_currentColumn != null && item._column == _currentColumn) ? nextGridFormatStyle : nextGridFormatStyle.ApplyOverrides(otherGridFormatStyle)));
					if (SelectionMode == SelectionMode.MultipleCellSelection && item.GetCell() != null && item.GetCell().Selected && (EditMode == EditMode.EditOff || item._gridGroupLayoutStateOwner.GridRow != CurrentRow || item._column != CurrentColumn))
					{
						applyOverrides = applyOverrides.ApplyOverrides(GetSelectionFormatStyle());
					}
				}
				else
				{
					applyOverrides = nextGridFormatStyle;
				}
				PaintLayoutItemAtBounds(graphics, item, applyOverrides, paintClipRectangle);
			}
			DrawFilterRowInfoText(graphics, mordexRowLayoutState);
		}

		private void DrawGroupTotalRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, Rectangle bounds)
		{
			Rectangle b = new Rectangle(base.ClientRectangle.X, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
			if (Rectangle.Intersect(GetPaintClipRectangle(), b).IsEmpty)
			{
				return;
			}
			GridFormatStyle gridFormatStyle = GetGroupTotalRowStyle(mordexRowLayoutState.LayoutTable);
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(mordexRowLayoutState._gridRow.Group.GetGroupRowStyle());
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.RowStyle);
			GridFormatStyle otherGridFormatStyle = null;
			GridFormatStyle formatStyle = null;
			GridFormatStyle nextApplyOverrides = null;
			if (mordexRowLayoutState._gridRow.Selected)
			{
				if (!IsSingleCellSelection())
				{
					otherGridFormatStyle = GetSelectionFormatStyle();
				}
				else if (SelectionMode != SelectionMode.MultipleCellSelection && (_gridSelectedItemCollection.Count > 1 || _currentColumn == null))
				{
					otherGridFormatStyle = GetSelectionFormatStyle();
				}
			}
			else
			{
				otherGridFormatStyle = null;
			}
			Rectangle rectangle = Rectangle.FromLTRB(GetRowLayoutRightEdge(mordexRowLayoutState), bounds.Top, bounds.Right, bounds.Bottom);
			Rectangle adjustRectangleToViewport = Rectangle.FromLTRB(bounds.Left, bounds.Top, rectangle.Left, bounds.Bottom);
			rectangle = AdjustRectangleToViewport(rectangle, false);
			adjustRectangleToViewport = AdjustRectangleToViewport(adjustRectangleToViewport, false);
			rectangle = Rectangle.Intersect(rectangle, GetPaintClipRectangle());
			adjustRectangleToViewport = Rectangle.Intersect(adjustRectangleToViewport, GetPaintClipRectangle());
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				bounds = (IsScrollableLayoutItem(item) ? adjustRectangleToViewport : rectangle);
				Rectangle b2 = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				if (Rectangle.Intersect(GetPaintClipRectangle(), b2).IsEmpty)
				{
					continue;
				}
				formatStyle = ((item._column == null) ? otherApplyOverrides : otherApplyOverrides.ApplyOverrides(mordexRowLayoutState._gridRow.Cells[item._column.Index].FormatStyle));
				if (mordexRowLayoutState._gridRow.Selected && (item._type == LayoutItemType.EmptyCell || item._type == LayoutItemType.GroupTotalCell))
				{
					nextApplyOverrides = formatStyle.ApplyOverrides(otherGridFormatStyle);
					if (SelectionMode == SelectionMode.MultipleCellSelection && item.GetCell() != null && item.GetCell().Selected && (EditMode == EditMode.EditOff || item._gridGroupLayoutStateOwner.GridRow != CurrentRow || item._column != CurrentColumn))
					{
						nextApplyOverrides = nextApplyOverrides.ApplyOverrides(GetSelectionFormatStyle());
					}
				}
				else
				{
					nextApplyOverrides = formatStyle;
				}
				PaintLayoutItemAtBounds(graphics, item, nextApplyOverrides, bounds);
			}
		}

		private void DrawHeaderRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState, Rectangle otherRectangle)
		{
			Rectangle rectangle = otherRectangle;
			Rectangle b = new Rectangle(mordexRowLayoutState.GetLocation().X, mordexRowLayoutState.GetLocation().Y, base.ClientRectangle.Width, mordexRowLayoutState._rowHeight);
			if (Rectangle.Intersect(GetPaintClipRectangle(), b).IsEmpty)
			{
				return;
			}
			GridFormatStyle gridFormatStyle = GetHeaderStyle(mordexRowLayoutState.LayoutTable);
			GridFormatStyle tableHeaderStyle = null;
			Rectangle adjustRectangleToViewport = Rectangle.FromLTRB(GetRowLayoutRightEdge(mordexRowLayoutState), otherRectangle.Top, otherRectangle.Right, otherRectangle.Bottom);
			Rectangle otherAdjustRectangleToViewport = Rectangle.FromLTRB(otherRectangle.Left, otherRectangle.Top, adjustRectangleToViewport.Left, otherRectangle.Bottom);
			adjustRectangleToViewport = AdjustRectangleToViewport(adjustRectangleToViewport, false);
			otherAdjustRectangleToViewport = AdjustRectangleToViewport(otherAdjustRectangleToViewport, false);
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				otherRectangle = ((item._type == LayoutItemType.TableHeader) ? rectangle : (IsScrollableLayoutItem(item) ? otherAdjustRectangleToViewport : adjustRectangleToViewport));
				Rectangle b2 = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				if (!Rectangle.Intersect(GetPaintClipRectangle(), b2).IsEmpty)
				{
					tableHeaderStyle = ((item._type == LayoutItemType.TableHeader) ? GetTableHeaderStyle(item.GetOwnerTable()) : ((item._column != null) ? gridFormatStyle.ApplyOverrides(item._column.HeaderStyle) : ((item._columnSet == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(item._columnSet.HeaderStyle))));
					PaintLayoutItemAtBounds(graphics, item, tableHeaderStyle, otherRectangle);
					if (tableHeaderStyle != gridFormatStyle)
					{
						tableHeaderStyle.DisposeResources();
					}
				}
			}
			gridFormatStyle?.DisposeResources();
		}

		private void DrawCard(Graphics graphics, GridTableLayoutState gridTableLayoutState, GridFormatStyle formatStyle)
		{
			GridFormatStyle gridFormatStyle = ((!gridTableLayoutState.GridRow.Selected) ? formatStyle : formatStyle.ApplyOverrides(GetSelectionFormatStyle()));
			GridFormatStyle alternatingRowStyle = GetAlternatingRowStyle(RootTable, gridTableLayoutState.GetLogicalRowIndex());
			GridFormatStyle cardColumnHeaderStyle = GetCardColumnHeaderStyle(gridTableLayoutState.GetLogicalRowIndex());
			GridFormatStyle applyOverrides = alternatingRowStyle.ApplyOverrides(gridTableLayoutState.GridRow.RowStyle);
			GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(gridTableLayoutState.GridRow.GetCachedFormatStyle());
			Rectangle rectangle = AdjustRectangleToViewport(gridTableLayoutState.GetBoundsBelowHeader(), true);
			Brush brush = otherApplyOverrides.GetOrCreateBackBrush(rectangle);
			if (brush != null)
			{
				Rectangle bounds = Rectangle.Intersect(GetPaintClipRectangle(), rectangle);
				if (!bounds.IsEmpty)
				{
					graphics.FillRectangle(brush, bounds);
				}
			}
			AdjustRectangleToViewport(gridTableLayoutState.GetBounds(), true).Inflate(1, 1);
			foreach (GridGroupLayoutState item in gridTableLayoutState._arrayList)
			{
				Rectangle a = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
				a = Rectangle.Intersect(a, GetPaintClipRectangle());
				switch (item._type)
				{
				case LayoutItemType.CardHeader:
					if (!a.IsEmpty)
					{
						GridFormatStyle nextApplyOverrides = cardColumnHeaderStyle.ApplyOverrides(item._column.HeaderStyle);
						PaintLayoutItemAtBounds(graphics, item, nextApplyOverrides, GetPaintClipRectangle());
						if (nextApplyOverrides != cardColumnHeaderStyle)
						{
							nextApplyOverrides.DisposeResources();
						}
					}
					break;
				case LayoutItemType.CardCell:
				{
					if (a.IsEmpty)
					{
						break;
					}
					GridCell gridCell = item.GetCell();
					if (gridCell != null)
					{
						GridFormatStyle otherFormatStyle = ((gridCell.CellDisplayType != ColumnType.Link) ? otherApplyOverrides : otherApplyOverrides.ApplyOverrides(_linkFormatStyle));
						GridFormatStyle previousApplyOverrides = otherFormatStyle.ApplyOverrides(item._column.CellStyle);
						GridFormatStyle firstApplyOverrides = previousApplyOverrides.ApplyOverrides(gridCell.GetEffectiveFormatStyle());
						GridFormatStyle secondApplyOverrides = firstApplyOverrides.ApplyOverrides(gridCell.FormatStyle);
						GridFormatStyle otherGridFormatStyle = ((!HasRowError(gridTableLayoutState._gridRow)) ? secondApplyOverrides : secondApplyOverrides.ApplyOverrides(_rowWithErrorsFormatStyle));
						PaintLayoutItemAtBounds(graphics, item, otherGridFormatStyle, GetPaintClipRectangle());
						if (otherGridFormatStyle != secondApplyOverrides)
						{
							otherGridFormatStyle.DisposeResources();
						}
						if (secondApplyOverrides != firstApplyOverrides)
						{
							secondApplyOverrides.DisposeResources();
						}
						if (firstApplyOverrides != previousApplyOverrides)
						{
							firstApplyOverrides.DisposeResources();
						}
						if (previousApplyOverrides != otherApplyOverrides)
						{
							previousApplyOverrides.DisposeResources();
						}
					}
					else
					{
						PaintLayoutItemAtBounds(graphics, item, otherApplyOverrides, GetPaintClipRectangle());
					}
					break;
				}
				case LayoutItemType.CardSelector:
				{
					GridFormatStyle nextFormatStyle = new GridFormatStyle();
					PaintLayoutItemAtBounds(graphics, item, nextFormatStyle, GetPaintClipRectangle());
					break;
				}
				case LayoutItemType.CardCaption:
				{
					GridFormatStyle thirdApplyOverrides = gridFormatStyle.ApplyOverrides(gridTableLayoutState.GridRow.GetCachedFormatStyle());
					PaintLayoutItemAtBounds(graphics, item, thirdApplyOverrides, GetPaintClipRectangle());
					break;
				}
				case LayoutItemType.CardExpandBox:
					PaintLayoutItemAtBounds(graphics, item, gridFormatStyle, GetPaintClipRectangle());
					break;
				default:
					PaintLayoutItemAtBounds(graphics, item, alternatingRowStyle, GetPaintClipRectangle());
					break;
				}
			}
			if (otherApplyOverrides != applyOverrides)
			{
				otherApplyOverrides.DisposeResources();
				otherApplyOverrides = null;
			}
			if (applyOverrides != alternatingRowStyle)
			{
				applyOverrides.DisposeResources();
				applyOverrides = null;
			}
			if (_cardBorders)
			{
				if (_gridControlStyle.GetLayoutState().GetCardsStyle()._isThemed)
				{
					Color color = _gridControlStyle.GetLayoutState().GetCardsStyle().GetResolvedAppearance()
						.GetGridColor(GridColorKey.Gridline);
					Pen otherPen = GdiCache.Pen(color);
					Rectangle otherAdjustRectangleToViewport = AdjustRectangleToViewport(gridTableLayoutState.GetBoundsBelowHeader(), true);
					if (otherAdjustRectangleToViewport.Height > 0)
					{
						otherAdjustRectangleToViewport.Width++;
						otherAdjustRectangleToViewport.Height++;
						graphics.DrawLine(otherPen, otherAdjustRectangleToViewport.Left, otherAdjustRectangleToViewport.Bottom - 1, otherAdjustRectangleToViewport.Left, otherAdjustRectangleToViewport.Top);
						graphics.DrawLine(otherPen, otherAdjustRectangleToViewport.Left, otherAdjustRectangleToViewport.Bottom - 1, otherAdjustRectangleToViewport.Right - 1, otherAdjustRectangleToViewport.Bottom - 1);
						graphics.DrawLine(otherPen, otherAdjustRectangleToViewport.Right - 1, otherAdjustRectangleToViewport.Top, otherAdjustRectangleToViewport.Right - 1, otherAdjustRectangleToViewport.Bottom - 1);
					}
				}
				else
				{
					Pen nextPen = GdiCache.Pen(gridFormatStyle.BackColor);
					Rectangle nextAdjustRectangleToViewport = AdjustRectangleToViewport(gridTableLayoutState.GetBounds(), true);
					nextAdjustRectangleToViewport.Inflate(1, 1);
					GridHelper.DrawFlatBorder(graphics, nextPen, nextAdjustRectangleToViewport);
					nextPen = null;
				}
			}
			if (gridFormatStyle != formatStyle)
			{
				gridFormatStyle.DisposeResources();
			}
		}

		private void DrawWatermarkImage(Graphics graphics, Rectangle rectangle)
		{
			_cachedImageRenderer = GetCornerShadowRenderer();
			_cachedImageRenderer.DrawShadow(graphics, rectangle);
		}

		internal void DrawGroupByBoxBackground(Graphics graphics, bool condition)
		{
			Rectangle rectangle = GetGroupByBoxRectangle();
			if (!rectangle.IsEmpty && !Rectangle.Intersect(GetPaintClipRectangle(), rectangle).IsEmpty)
			{
				GridFormatStyle gridFormatStyle = GetGroupByBoxStyle();
				DrawLayoutItem(graphics, new GridGroupLayoutState(LayoutItemType.GroupByBoxBackground, null, null), gridFormatStyle, rectangle, GetPaintClipRectangle());
			}
		}

		private void DrawGroupByBoxRow(Graphics graphics, MxRowLayoutState mordexRowLayoutState)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Height = mordexRowLayoutState._rowHeight;
			clientRectangle.Y = mordexRowLayoutState.GetLocation().Y;
			if (Rectangle.Intersect(GetPaintClipRectangle(), clientRectangle).IsEmpty)
			{
				return;
			}
			foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
			{
				if (item._type == LayoutItemType.GroupByBoxInfo)
				{
					GridFormatStyle gridFormatStyle = GetGroupByBoxInfoStyle(item.GetOwnerTable());
					PaintLayoutItemAtBounds(graphics, item, gridFormatStyle, GetPaintClipRectangle());
					gridFormatStyle.DisposeResources();
				}
				else if (item._type == LayoutItemType.GroupByBoxHeader)
				{
					GridFormatStyle gridFormatStyle = GetHeaderStyle(item.GetOwnerTable());
					GridFormatStyle formatStyle = ((item._gridGroup.Column == null) ? new GridFormatStyle(gridFormatStyle) : gridFormatStyle.ApplyOverrides(item._gridGroup.Column.HeaderStyle));
					PaintLayoutItemAtBounds(graphics, item, formatStyle, GetPaintClipRectangle());
					formatStyle.DisposeResources();
					gridFormatStyle.DisposeResources();
				}
				else if (item._type == LayoutItemType.GroupByBoxConnector)
				{
					Rectangle rectangle = AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
					DrawGroupByBoxConnectors(graphics, item, rectangle, Rectangle.Empty);
				}
				else if (item._type == LayoutItemType.GroupByBoxTableHeader)
				{
					GridFormatStyle gridFormatStyle = GetGroupByBoxInfoStyle(item.GetOwnerTable());
					AdjustRectangleToViewport(item.GetAbsoluteItemRect(), item._isHorizontallyScrolled);
					PaintLayoutItemAtBounds(graphics, item, gridFormatStyle, GetPaintClipRectangle());
					gridFormatStyle.DisposeResources();
				}
			}
		}

		private void DrawGroupByBoxConnectors(Graphics graphics, GridGroupLayoutState gridGroupLayoutState, Rectangle rectangle, Rectangle otherRectangle)
		{
			Pen pen = GdiCache.Pen(SystemColors.WindowFrame);
			if (otherRectangle.IsEmpty)
			{
				otherRectangle = new Rectangle(0, 0, 8, 8);
				otherRectangle.X = rectangle.X + (rectangle.Width - otherRectangle.Width) / 2;
				otherRectangle.Y = rectangle.Y + (rectangle.Height - otherRectangle.Height) / 2;
				otherRectangle.X += otherRectangle.Width / 2;
				otherRectangle.Y += otherRectangle.Height / 2;
				otherRectangle.Width = 0;
				otherRectangle.Height = 0;
			}
			if ((gridGroupLayoutState._borderFlags & BorderFlags.ConnectorLeft) != BorderFlags.None)
			{
				if (RightToLeft == RightToLeft.No)
				{
					graphics.DrawLine(pen, rectangle.Left, otherRectangle.Top + otherRectangle.Height / 2, otherRectangle.Left - 1, otherRectangle.Top + otherRectangle.Height / 2);
				}
				else
				{
					graphics.DrawLine(pen, otherRectangle.Right, otherRectangle.Top + otherRectangle.Height / 2, rectangle.Right, otherRectangle.Top + otherRectangle.Height / 2);
				}
			}
			if ((gridGroupLayoutState._borderFlags & BorderFlags.ConnectorTop) != BorderFlags.None)
			{
				graphics.DrawLine(pen, otherRectangle.Left + otherRectangle.Width / 2, rectangle.Top + gridGroupLayoutState._topMargin, otherRectangle.Left + otherRectangle.Width / 2, otherRectangle.Top - 1);
			}
			if ((gridGroupLayoutState._borderFlags & BorderFlags.ConnectorRight) != BorderFlags.None)
			{
				if (RightToLeft == RightToLeft.No)
				{
					graphics.DrawLine(pen, otherRectangle.Right, otherRectangle.Top + otherRectangle.Height / 2, rectangle.Right, otherRectangle.Top + otherRectangle.Height / 2);
				}
				else
				{
					graphics.DrawLine(pen, rectangle.Left, otherRectangle.Top + otherRectangle.Height / 2, otherRectangle.Left - 1, otherRectangle.Top + otherRectangle.Height / 2);
				}
			}
			if ((gridGroupLayoutState._borderFlags & BorderFlags.ConnectorBottom) != BorderFlags.None)
			{
				graphics.DrawLine(pen, otherRectangle.Left + otherRectangle.Width / 2, otherRectangle.Bottom, otherRectangle.Left + otherRectangle.Width / 2, rectangle.Bottom);
			}
		}

		private void PaintLayoutItemAtBounds(Graphics graphics, GridGroupLayoutState gridGroupLayoutState, GridFormatStyle gridFormatStyle, Rectangle bounds)
		{
			Rectangle rectangle = AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
			DrawLayoutItem(graphics, gridGroupLayoutState, gridFormatStyle, rectangle, bounds);
		}
	}
}
