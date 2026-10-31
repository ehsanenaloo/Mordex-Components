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
		private void SetDefaultControlSize()
		{
			base.Size = new Size(400, 376);
		}

		private void SyncRowWithCurrencyPosition()
		{
			MoveToCurrentPosition(true);
		}

		private void OnCurrencyManagerPositionChanged(object sender, EventArgs eventArgs)
		{
			RunOnUiThread(SyncRowWithCurrencyPosition);
		}

		private void ApplyCurrentLayout()
		{
			_row = null;
			_gridLayout = _currentLayout;
			GridLayout gridLayout = null;
			if (_currentLayout != null)
			{
				gridLayout = _currentLayout.GetMergedLayout();
			}
			if (gridLayout == null || string.IsNullOrEmpty(gridLayout.LayoutString))
			{
				if (gridLayout == null)
				{
					SetDataSourceAndMember(null, "", true);
				}
				else
				{
					SetDataSourceAndMember(gridLayout.DataSource, gridLayout.DataMember, true);
				}
				ReadGridLayoutByMode(new LayoutReader(), PersistenceMode.All);
			}
			else
			{
				SetDataSourceAndMember(_currentLayout.DataSource, _currentLayout.DataMember, true);
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(gridLayout);
				mordexLayoutReader["GridLayoutData"]?.DeserializeObject(this);
			}
			if (!_isInitializing)
			{
				OnCurrentLayoutChanged(EventArgs.Empty);
			}
			RebuildRows(false);
		}

		internal void ClearLayoutStates()
		{
			if (_isInitializing)
			{
				return;
			}
			if (_rowCache != null)
			{
				_rowCache.ReleaseAllRows();
			}
			if (_headerRowLayouts != null)
			{
				_headerRowLayouts.ClearAll();
			}
			if (_bodyRowLayouts != null)
			{
				_bodyRowLayouts.ClearAll();
			}
			if (_cardRowLayouts != null)
			{
				_cardRowLayouts.ClearAndDispose();
			}
			_isSelectionUpdating = true;
			if (_gridSelectedItemCollection != null)
			{
				_gridSelectedItemCollection.Clear();
			}
			_currentColumn = null;
			_ = Row;
			bool condition = _row == null;
			SetCurrentGridRow(null, true);
			_isSelectionUpdating = false;
			if (_gridRow != null && FilterMode == FilterMode.Manual)
			{
				_gridRow.SetLogicalRow(_gridDataSourceManager.GetVirtualDataRow());
			}
			_firstRow = 0;
			if (_currencyManager != null && _currencyManager.Count > 0)
			{
				if (_currencyManager.Position == -1)
				{
					_currencyManager.Refresh();
				}
				GridRow row = GetRow(_currencyManager.Current);
				if (row != null && row.IsVisible && !_gridDataSourceManager.RowManager.IsIndexSuspend)
				{
					Row = row.Position;
				}
				else
				{
					SetCurrentRowAndSelect(0);
					if (Row == -3)
					{
						if (RootTable != null && RootTable.IsAddNewAllowed())
						{
							MoveToNewRecord();
						}
						else if (IsValidRowPosition(-2))
						{
							Row = -2;
						}
						else if (!condition)
						{
							RaiseSelectionChangedOrDefer();
						}
					}
				}
			}
			else if (RootTable != null && RootTable.IsAddNewAllowed())
			{
				MoveToNewRecord();
			}
			else
			{
				Row = -3;
				if (!condition && _currencyManager != null)
				{
					RaiseSelectionChangedOrDefer();
				}
			}
			RefreshRowCountAndInvalidate();
		}

		internal void PrepareControlStyleLayout()
		{
			UpdateSelectionVisibility();
			_gridControlStyle.GetLayoutState()._defaultBounds = base.ClientRectangle;
			_gridControlStyle.GetLayoutState()._treelineColor = _treeLineColor;
			_gridControlStyle.GetLayoutState()._showTreelines = _treeLines;
			if (_view == View.TableView)
			{
				_gridControlStyle.GetLayoutState()._textAlignment = TextAlignment.Center;
			}
			else
			{
				_gridControlStyle.GetLayoutState()._textAlignment = TextAlignment.Near;
			}
			if (_hasEditorsControlStyle)
			{
				_gridControlStyle.GetLayoutState()._appliedControlStyle = _editorsControlStyle;
			}
			else
			{
				_gridControlStyle.GetLayoutState()._appliedControlStyle = null;
			}
			_controlStyleFont = Font;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (_bufferedGraphics != null)
			{
				_bufferNeedsRecreate = true;
			}
		}

		protected override void OnClientResize()
		{
			MarkTablesForAutoResize();
			RefreshRowCountAndForceInvalidate();
			base.OnClientResize();
		}

		private int GetHorizontalScrollOffset()
		{
			return _horizontalScrollOffset;
		}

		private void SetHorizontalScrollOffset(int horizontalScrollOffset)
		{
			if (horizontalScrollOffset < 0)
			{
				horizontalScrollOffset = 0;
			}
			if (horizontalScrollOffset == _horizontalScrollOffset)
			{
				return;
			}
			if (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			if (horizontalScrollOffset + base.ClientRectangle.Width > _contentWidth)
			{
				horizontalScrollOffset = _contentWidth - base.ClientRectangle.Width;
			}
			if (horizontalScrollOffset < 0)
			{
				horizontalScrollOffset = 0;
			}
			_ = DateTime.Now.Ticks;
			if (!_isRenderingPartial)
			{
				FlushPendingPaint();
			}
			int otherHorizontalScrollOffset = _horizontalScrollOffset;
			_horizontalScrollOffset = horizontalScrollOffset;
			GetHorizontalScrollBar().SetScrollValue(_horizontalScrollOffset);
			_horizontalScrollOffset = GetHorizontalScrollBar().GetScrollValue();
			if (!_isRenderingPartial && GetHorizontalScrollBar().GetScrollValue() != otherHorizontalScrollOffset)
			{
				if (_isRenderingContent)
				{
					Invalidate();
				}
				else if (!TryScrollRowsByPixels(otherHorizontalScrollOffset, horizontalScrollOffset))
				{
					Invalidate();
					Update();
				}
			}
			if (!_isRenderingPartial && !_isRenderingContent)
			{
				Update();
			}
			PositionCellEditorControl(true);
			OnScroll(EventArgs.Empty);
		}

		private void SetFirstVisibleRow(int firstRow)
		{
			if (firstRow > RowCount - 1)
			{
				firstRow = RowCount - 1;
			}
			if (firstRow < 0)
			{
				firstRow = 0;
			}
			if (firstRow != _firstRow)
			{
				HideCellEditorControl();
				_firstRow = firstRow;
				if (_view == View.SingleCard)
				{
					SetCurrentRowAndSelect(_firstRow);
					_firstRow = Row;
				}
			}
		}

		internal Rectangle AdjustRectangleToViewport(Rectangle rectangle, bool condition)
		{
			if (!GetScrollableBaseBoolean())
			{
				if (_view == View.TableView)
				{
					if (condition)
					{
						rectangle.Offset(-_horizontalScrollOffset, 0);
					}
				}
				else if (condition)
				{
					rectangle.Offset(base.ClientRectangle.Left, -_verticalScrollOffset);
				}
				return rectangle;
			}
			if (_view == View.TableView)
			{
				rectangle.X -= base.ClientRectangle.Left;
				if (condition)
				{
					rectangle.Offset(-_horizontalScrollOffset, 0);
				}
			}
			else if (condition)
			{
				rectangle.Offset(_borderWidth, -_verticalScrollOffset);
			}
			rectangle.X = base.ClientRectangle.Right - rectangle.Left - rectangle.Width;
			return rectangle;
		}

		private Rectangle ScrollOffsetRectangle(Rectangle rectangle, bool condition)
		{
			if (_view == View.TableView)
			{
				if (condition)
				{
					rectangle.Offset(-_horizontalScrollOffset, 0);
				}
			}
			else
			{
				rectangle.Offset(base.ClientRectangle.Left, -_verticalScrollOffset);
			}
			return rectangle;
		}

		internal void CalculateLayoutIfReady()
		{
			if (!_isPaintingEnabled)
			{
				_isLayoutValid = true;
				return;
			}
			if (base.IsDisposed || base.Disposing || !base.IsHandleCreated)
			{
				_isLayoutValid = true;
				return;
			}
			Graphics graphics = CreateGraphics();
			CalculateLayoutStates(graphics);
			graphics.Dispose();
			graphics = null;
		}

		private void CalculateLayoutStates(Graphics graphics)
		{
			if (_gridDataSourceManager == null)
			{
				return;
			}
			_isRenderingContent = true;
			SyncRecordNavigatorPosition();
			if (_view == View.TableView)
			{
				if (GetHorizontalScrollBar() != null)
				{
					GetHorizontalScrollBar().SetScrollStep(_tableViewHorizontalScrollIncrement);
				}
				LayoutTableView(graphics);
			}
			else if (_view == View.CardView)
			{
				if (GetHorizontalScrollBar() != null)
				{
					GetHorizontalScrollBar().SetScrollStep(1);
				}
				LayoutCardView(graphics);
			}
			else
			{
				if (GetHorizontalScrollBar() != null)
				{
					GetHorizontalScrollBar().SetScrollStep(1);
				}
				LayoutSingleCardView(graphics);
			}
			if (_gridGridGroupLayoutState != null)
			{
				ReleasePressedLayoutItem(null);
				_gridGridGroupLayoutState = null;
			}
			LayoutRowStateItems(_headerRowLayouts, graphics);
			LayoutRowStateItems(_footerRowLayouts, graphics);
			LayoutCardStateItems(_cardRowLayouts, graphics);
			LayoutRowStateItems(_bodyRowLayouts, graphics);
			if (_gridColumnContainerControl != null && !IsCellVisible(Row, Col))
			{
				HideCellEditorControl();
			}
			_numericEditControl.SetTotalCount(_gridDataSourceManager.GetRootRecordCount());
			if (RootTable != null && RootTable.IsAddNewAllowed())
			{
				_numericEditControl.UpdateNumericEditControlBoolean(true);
			}
			_isRenderingContent = false;
		}

		internal void LayoutRowStateItems(RowLayoutList state, Graphics graphics)
		{
			if (state != null)
			{
				LayoutItemsInList(state.GetArrayList(), graphics);
			}
		}

		internal void LayoutCardStateItems(TableLayoutCollection state, Graphics graphics)
		{
			if (state != null)
			{
				LayoutItemsInList(state.GetArrayList(), graphics);
			}
		}

		private void LayoutItemsInList(ArrayList arrayList, Graphics graphics)
		{
			if (arrayList == null)
			{
				return;
			}
			for (int i = 0; i < arrayList.Count; i++)
			{
				ILayoutElement arrayListItem = arrayList[i] as ILayoutElement;
				for (int j = 0; j < arrayListItem.Items.Count; j++)
				{
					GridGroupLayoutState gridGroupLayoutState = (GridGroupLayoutState)arrayListItem.Items[j];
					gridGroupLayoutState.RenderContent(_gridControlStyle.GetLayoutState().GetEditControlsStyle(), RightToLeft == RightToLeft.Yes, graphics, Font, !base.UseCompatibleTextRendering);
				}
			}
		}

		private void LayoutCardView(Graphics graphics)
		{
			_isLayoutValid = true;
			if (_cardRowLayouts == null)
			{
				_cardRowLayouts = new TableLayoutCollection();
			}
			_cardRowLayouts.ClearAndDispose();
			int value = 0;
			UpdateRectangleArray(CardWidth + value);
			if (RootTable == null)
			{
				return;
			}
			GridColumnCollection columns = RootTable.Columns;
			_ = columns._owner;
			Rectangle clientRectangle = base.ClientRectangle;
			_rowCache.MarkOtherRowsStale(_row);
			int otherValue = 0;
			int cardColumnIndex = 0;
			int controlStyleControl2Integer = 0;
			GridFormatStyle gridFormatStyle = GetCardCaptionStyle();
			MordexRowManager mordexRowManager = GetRowManager();
			Point point = new Point(_cardSpacing, _cardSpacing);
			int firstRow = _firstRow;
			MordexRow mordexRow = null;
			if (firstRow >= 0 && firstRow < mordexRowManager.Count)
			{
				mordexRow = mordexRowManager[firstRow];
			}
			while (mordexRow != null)
			{
				GridRow gridRow = GetGridRowForLogicalRow(mordexRow);
				PrepareRowForDisplay(gridRow, false);
				gridRow._isLayoutDirty = false;
				GridFormatStyle cardColumnHeaderStyle = GetCardColumnHeaderStyle(gridRow.GetPosition());
				GridFormatStyle alternatingRowStyle = GetAlternatingRowStyle(RootTable, gridRow.GetPosition());
				GridTableLayoutState gridTableLayoutState = BuildCardLayout(graphics, false, gridRow, gridFormatStyle, cardColumnHeaderStyle, alternatingRowStyle);
				alternatingRowStyle.DisposeResources();
				cardColumnHeaderStyle.DisposeResources();
				if (point.Y == _cardSpacing)
				{
					gridTableLayoutState._location = point;
				}
				else
				{
					if (point.Y - value + gridTableLayoutState.GetHeight() > clientRectangle.Bottom)
					{
						point.X += CardWidth + _cardSpacing + value;
						if (point.X > clientRectangle.Right)
						{
							break;
						}
						cardColumnIndex++;
						point.Y = _cardSpacing;
					}
					gridTableLayoutState._location = point;
				}
				if (point.X == CardSpacing || point.X + gridTableLayoutState.GetWidth() < clientRectangle.Right)
				{
					otherValue++;
				}
				gridTableLayoutState._cardColumnIndex = cardColumnIndex;
				if (cardColumnIndex == 0)
				{
					controlStyleControl2Integer++;
				}
				_cardRowLayouts.AddTableLayout(gridTableLayoutState);
				point.Y += gridTableLayoutState.GetHeight() + _cardSpacing + value;
				firstRow++;
				mordexRow = ((firstRow < 0 || firstRow >= mordexRowManager.Count) ? null : mordexRowManager[firstRow]);
			}
			_cardRowLayouts.SetCurrentIndex(otherValue);
			_rowCache.ReleaseStaleRows();
			_maxCardHeight = 0;
			foreach (GridTableLayoutState item in _cardRowLayouts)
			{
				Rectangle rectangle = item.GetBounds();
				int right = base.ClientRectangle.Right;
				if (!IsVerticalScrollRequired())
				{
					right -= SystemInformation.VerticalScrollBarWidth;
				}
				if (rectangle.Left < right && item.GetHeight() > _maxCardHeight)
				{
					_maxCardHeight = item.GetHeight();
				}
			}
			if (_maxCardHeight + _cardSpacing > base.ClientRectangle.Height)
			{
				base.SetVerticalScrollRequired(true);
				ConfigureVerticalScrollBar(0, _maxCardHeight + _cardSpacing * 2, base.ClientRectangle.Height, _verticalScrollOffset);
				GetVScroll().SetScrollStep(4);
			}
			else
			{
				_verticalScrollOffset = 0;
				base.SetVerticalScrollRequired(false);
			}
			if (otherValue < RowCount)
			{
				ConfigureHorizontalScrollBar(0, RowCount - 1, otherValue, _firstRow);
				GetHorizontalScrollBar().SetScrollStep(controlStyleControl2Integer);
				base.SetHorizontalScrollRequired(true);
			}
			else
			{
				base.SetHorizontalScrollRequired(false);
			}
		}

		private void LayoutSingleCardView(Graphics graphics)
		{
			_isLayoutValid = true;
			if (RootTable == null)
			{
				return;
			}
			if (_cardRowLayouts == null)
			{
				_cardRowLayouts = new TableLayoutCollection();
			}
			int value = 0;
			_cardRowLayouts.ClearAndDispose();
			GridColumnCollection columns = RootTable.Columns;
			_ = columns._owner;
			Rectangle clientRectangle = base.ClientRectangle;
			_rowCache.MarkOtherRowsStale(_row);
			GridTableLayoutState gridTableLayoutState = null;
			int otherValue = 0;
			int cardColumnIndex = 0;
			int nextValue = 0;
			GridFormatStyle gridFormatStyle = GetCardCaptionStyle();
			MordexRowManager mordexRowManager = GetRowManager();
			Point point = new Point(_cardSpacing, _cardSpacing);
			if (FirstRow != Row)
			{
				SetCurrentRowAndSelect(FirstRow);
			}
			int firstRow = _firstRow;
			MordexRow mordexRow = null;
			if (firstRow >= 0 && firstRow < mordexRowManager.Count)
			{
				mordexRow = mordexRowManager[firstRow];
			}
			if (mordexRow != null)
			{
				GridRow gridRow = GetGridRowForLogicalRow(mordexRow);
				PrepareRowForDisplay(gridRow, false);
				gridRow._isLayoutDirty = false;
				GridFormatStyle cardColumnHeaderStyle = GetCardColumnHeaderStyle(gridRow.GetPosition());
				GridFormatStyle alternatingRowStyle = GetAlternatingRowStyle(RootTable, gridRow.GetPosition());
				gridTableLayoutState = BuildCardLayoutWithWidth(graphics, false, gridRow, gridFormatStyle, cardColumnHeaderStyle, alternatingRowStyle, base.ClientRectangle.Width - _cardSpacing * 2 - value);
				alternatingRowStyle.DisposeResources();
				cardColumnHeaderStyle.DisposeResources();
				if (point.Y == _cardSpacing)
				{
					gridTableLayoutState._location = point;
				}
				else
				{
					if (point.Y + gridTableLayoutState.GetHeight() > clientRectangle.Bottom)
					{
						point.X += CardWidth + _cardSpacing;
						cardColumnIndex++;
						point.Y = _cardSpacing;
					}
					gridTableLayoutState._location = point;
				}
				if (point.X == CardSpacing || point.X + gridTableLayoutState.GetWidth() < clientRectangle.Right)
				{
					otherValue++;
				}
				gridTableLayoutState._cardColumnIndex = cardColumnIndex;
				if (cardColumnIndex == 0)
				{
					nextValue++;
				}
				_cardRowLayouts.AddTableLayout(gridTableLayoutState);
				point.Y += gridTableLayoutState.GetHeight() + _cardSpacing;
			}
			_cardRowLayouts.SetCurrentIndex(1);
			_rowCache.ReleaseStaleRows();
			_maxCardHeight = 0;
			foreach (GridTableLayoutState item in _cardRowLayouts)
			{
				if (item.GetHeight() > _maxCardHeight)
				{
					_maxCardHeight = item.GetHeight();
				}
			}
			if (_maxCardHeight + _cardSpacing + value > base.ClientRectangle.Height)
			{
				base.SetVerticalScrollRequired(true);
				ConfigureVerticalScrollBar(0, _maxCardHeight + _cardSpacing * 2 + value, base.ClientRectangle.Height, _verticalScrollOffset);
				GetVScroll().SetScrollStep(4);
			}
			else
			{
				if (gridTableLayoutState != null && _maxCardHeight + CardSpacing * 2 < base.ClientRectangle.Height && _centerSingleCard)
				{
					gridTableLayoutState._location = new Point(_cardSpacing, (base.ClientRectangle.Height - gridTableLayoutState.GetHeight()) / 2);
				}
				_verticalScrollOffset = 0;
				base.SetVerticalScrollRequired(false);
			}
			base.SetHorizontalScrollRequired(false);
		}

		private int GetVerticalScrollOffset()
		{
			return _verticalScrollOffset;
		}

		private void SetVerticalScrollOffset(int verticalScrollOffset)
		{
			if (_view != View.TableView && !_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			int value = _maxCardHeight + _cardSpacing * 2;
			if (verticalScrollOffset + base.ClientRectangle.Height > value)
			{
				verticalScrollOffset = value - base.ClientRectangle.Height;
			}
			if (verticalScrollOffset < 0)
			{
				verticalScrollOffset = 0;
			}
			if (verticalScrollOffset != _verticalScrollOffset)
			{
				HideCellEditorControl();
				_verticalScrollOffset = verticalScrollOffset;
				GetVScroll().SetScrollValue(_verticalScrollOffset);
				Invalidate();
				PositionCellEditorControl(true);
				OnScroll(EventArgs.Empty);
			}
		}

		private void UpdateRectangleArray(int value)
		{
			int otherValue = base.ClientRectangle.Width / value;
			_cardBounds = new Rectangle[otherValue];
			if (!_allowCardSizing || otherValue <= 0)
			{
				return;
			}
			int nextValue = 0;
			int previousValue = CardSpacing / 2;
			while (true)
			{
				previousValue += value + CardSpacing;
				if (previousValue < base.ClientRectangle.Right)
				{
					ref Rectangle reference = ref _cardBounds[nextValue];
					reference = new Rectangle(previousValue, CardSpacing, 2, base.ClientRectangle.Height - CardSpacing);
					nextValue++;
					continue;
				}
				break;
			}
		}

		internal GridTableLayoutState BuildPrintCardLayout(Graphics graphics, MxDataGridPrintDocument gridPrintDocument, GridRow gridRow, Rectangle rectangle, GridFormatStyle gridFormatStyle, GridFormatStyle formatStyle, GridFormatStyle otherFormatStyle)
		{
			int width = rectangle.Width / gridPrintDocument.CardColumnsPerPage - _cardSpacing;
			return BuildCardLayoutWithWidth(graphics, true, gridRow, gridFormatStyle, formatStyle, otherFormatStyle, width);
		}

		private GridTableLayoutState BuildCardLayout(Graphics graphics, bool condition, GridRow gridRow, GridFormatStyle gridFormatStyle, GridFormatStyle formatStyle, GridFormatStyle otherFormatStyle)
		{
			return BuildCardLayoutWithWidth(graphics, condition, gridRow, gridFormatStyle, formatStyle, otherFormatStyle, _cardWidth);
		}

		private int CalculateColumnHeaderHeight(Graphics graphics, GridFormatStyle formatStyle, GridColumn gridColumn)
		{
			GridFormatStyle gridFormatStyle = formatStyle.ApplyOverrides(gridColumn.HeaderStyle);
			Image image = null;
			if (gridColumn.HeaderImage != null)
			{
				image = gridColumn.HeaderImage;
			}
			else
			{
				ImageList imageList = ComputeImageList();
				if (imageList != null)
				{
					if (gridColumn.HeaderImageKey.Length > 0)
					{
						if (imageList.Images.ContainsKey(gridColumn.HeaderImageKey))
						{
							image = imageList.Images[gridColumn.HeaderImageKey];
						}
					}
					else if (gridColumn.HeaderImageIndex >= 0 && gridColumn.HeaderImageIndex < imageList.Images.Count)
					{
						image = imageList.Images[gridColumn.HeaderImageIndex];
					}
				}
			}
			TextLayoutRenderer textLayoutRenderer = gridColumn.CreateHeaderLayoutRenderer(gridFormatStyle, !base.UseCompatibleTextRendering);
			int result = MeasureTextCellWidth(graphics, gridColumn.Caption, gridFormatStyle, textLayoutRenderer, image, null) - 3;
			textLayoutRenderer.ReleaseStringFormat();
			if (formatStyle != gridFormatStyle)
			{
				gridFormatStyle.DisposeResources();
			}
			return result;
		}

		private GridTableLayoutState BuildCardLayoutWithWidth(Graphics graphics, bool condition, GridRow gridRow, GridFormatStyle otherGridFormatStyle, GridFormatStyle formatStyle, GridFormatStyle otherFormatStyle, int width)
		{
			if (RootTable.CellLayoutMode == CellLayoutMode.UseColumnSets)
			{
				return BuildCardLayoutWithColumnSets(graphics, condition, gridRow, otherGridFormatStyle, formatStyle, otherFormatStyle, width);
			}
			int height = 0;
			height = GetImageSize().Height;
			GridTableLayoutState gridTableLayoutState = new GridTableLayoutState(LayoutRowType.DataRow, RootTable);
			gridTableLayoutState._gridRow = gridRow;
			int otherHeight = 0;
			if (_cardHeaders)
			{
				if (gridRow.CardCaptionText.Length > 0 && gridRow.Table.Columns._owner != null)
				{
					TextLayoutRenderer textLayoutRenderer = GridHelper.ComputeTextLayoutRenderer(otherGridFormatStyle, !base.UseCompatibleTextRendering);
					otherHeight = GridHelper.CalculateTextHeight(graphics, otherGridFormatStyle.ResolveFont(Font), gridRow.CardCaptionText, textLayoutRenderer, width - 4, RootTable.Columns._owner.MinLines, RootTable.Columns._owner.MaxLines) + 8;
					textLayoutRenderer.ReleaseStringFormat();
				}
				else
				{
					otherHeight = (int)otherGridFormatStyle.ResolveFont(Font).GetHeight(graphics) + 8;
				}
				gridTableLayoutState._headerHeight = otherHeight;
				GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CardCaption, gridTableLayoutState, null, new Rectangle(0, 0, width + 1, otherHeight));
				GridGroupLayoutState otherGridGroupLayoutState = gridGroupLayoutState;
				gridGroupLayoutState._leftMargin = 2;
				if (RootTable.Columns._gridColumnCollectionOwner != null)
				{
					gridGroupLayoutState._ownImage = gridRow.Cells[RootTable.Columns._gridColumnCollectionOwner.Index].ResolveImage(ComputeImageList());
				}
				if (_showErrors && HasRowError(gridRow))
				{
					gridGroupLayoutState._ownImage = GetErrorImage();
				}
				gridTableLayoutState._arrayList.Add(gridGroupLayoutState);
				if (_expandableCards)
				{
					gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CardExpandBox, gridTableLayoutState, null, new Rectangle(width - otherHeight - 2, 0, otherHeight, otherHeight));
					otherGridGroupLayoutState._rightMargin = otherHeight + 4;
					gridTableLayoutState._arrayList.Add(gridGroupLayoutState);
				}
				bool visible = false;
				GridColumn column = null;
				foreach (GridColumn otherColumn in RootTable.Columns)
				{
					if (otherColumn.ActAsSelector)
					{
						visible = otherColumn.Visible;
						column = otherColumn;
						break;
					}
				}
				if (visible)
				{
					otherGridGroupLayoutState._leftMargin = otherGridGroupLayoutState._rect.Height + 4;
					Rectangle bounds = otherGridGroupLayoutState._rect;
					bounds.Width = bounds.Height;
					bounds.X = 2;
					gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CardSelector, gridTableLayoutState, column, bounds);
					gridTableLayoutState._arrayList.Add(gridGroupLayoutState);
					gridTableLayoutState._gridGroupLayoutState = gridGroupLayoutState;
				}
			}
			int nextHeight = otherHeight;
			if (condition || RowStateHelper.GetRowTag(gridRow.GetLogicalRow()).GetIsExpanded() || !_expandableCards)
			{
				nextHeight += _cardInnerSpacing;
				int value = 0;
				GridColumn[] array = new GridColumn[RootTable.Columns.Count];
				foreach (GridColumn arrayItem in RootTable.Columns)
				{
					array[arrayItem._position] = arrayItem;
					int columnHeaderHeight = 0;
					if (arrayItem.Visible && arrayItem.Caption.Length > 0 && arrayItem.CardViewCaptionVisible && !arrayItem.ActAsSelector)
					{
						columnHeaderHeight = CalculateColumnHeaderHeight(graphics, formatStyle, arrayItem);
					}
					if (columnHeaderHeight > value)
					{
						value = columnHeaderHeight;
					}
				}
				GridFormatStyle gridFormatStyle = otherFormatStyle.ApplyOverrides(gridRow.RowStyle);
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.GetCachedFormatStyle());
				Font font = applyOverrides.ResolveFont(Font);
				int otherValue = GridHelper.RoundToInt(font.GetHeight(graphics)) + 2;
				GridColumn[] otherArray = array;
				foreach (GridColumn nextColumn in otherArray)
				{
					if (!nextColumn.Visible || nextColumn.ActAsSelector)
					{
						continue;
					}
					int previousHeight = otherValue;
					GridCell gridCell = gridRow.Cells[nextColumn.Index];
					if (gridCell.CellDisplayType == ColumnType.BoundImage)
					{
						if (gridCell.ImageSize.IsEmpty)
						{
							if (nextColumn.BoundImageSize != Size.Empty)
							{
								previousHeight = nextColumn.BoundImageSize.Height + nextColumn.TopMargin + nextColumn.BottomMargin;
							}
							else if (gridCell.Value is Image image)
							{
								previousHeight = image.Size.Height + nextColumn.TopMargin + nextColumn.BottomMargin;
							}
						}
						else
						{
							previousHeight = gridCell.ImageSize.Height + nextColumn.TopMargin + nextColumn.BottomMargin;
						}
					}
					else if (gridCell.CellDisplayType == ColumnType.Text || gridCell.CellDisplayType == ColumnType.ImageAndText)
					{
						GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(nextColumn.CellStyle);
						GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridRow.Cells[nextColumn.Index].GetEffectiveFormatStyle());
						GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridRow.Cells[nextColumn.Index].FormatStyle);
						Font otherFont = previousApplyOverrides.ResolveFont(Font);
						if (otherFont != font || nextColumn.WordWrap)
						{
							TextLayoutRenderer otherTextLayoutRenderer = nextColumn.ComputeTextLayoutRenderer(previousApplyOverrides, !base.UseCompatibleTextRendering);
							previousHeight = GridHelper.CalculateTextHeight(graphics, otherFont, gridRow.Cells[nextColumn.Index].Text, otherTextLayoutRenderer, width - value - nextColumn.LeftMargin - nextColumn.RightMargin - 2, nextColumn.MinLines, nextColumn.MaxLines) - 4;
							otherTextLayoutRenderer.ReleaseStringFormat();
						}
						if (gridCell.CellDisplayType == ColumnType.ImageAndText && previousHeight < height)
						{
							previousHeight = height;
						}
						previousHeight += nextColumn.TopMargin + nextColumn.BottomMargin;
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
					else if (gridCell.CellDisplayType == ColumnType.Image)
					{
						int firstHeight = height;
						Image otherImage = gridRow.Cells[nextColumn.Index].Image;
						if (otherImage != null)
						{
							firstHeight = otherImage.Height;
						}
						previousHeight = firstHeight + nextColumn.TopMargin + nextColumn.BottomMargin;
					}
					else if (gridCell.CellDisplayType == ColumnType.CheckBox)
					{
						previousHeight = 14 + nextColumn.TopMargin + nextColumn.BottomMargin;
					}
					previousHeight++;
					if (_cardViewGridlines == CardViewGridlines.Both || _cardViewGridlines == CardViewGridlines.Horizontal || CardViewGridlines == CardViewGridlines.FieldsOnly)
					{
						previousHeight++;
					}
					if (!_showEmptyFields && gridRow.Cells[nextColumn.Index].Text.Length <= 0)
					{
						continue;
					}
					int cardInnerSpacing = _cardInnerSpacing;
					int otherWidth = 0;
					if (nextColumn.CardViewCaptionVisible)
					{
						otherWidth = value;
						GridGroupLayoutState groupLayoutState = new GridGroupLayoutState(LayoutItemType.CardHeader, gridTableLayoutState, nextColumn);
						groupLayoutState._rect = new Rectangle(cardInnerSpacing, nextHeight, otherWidth, previousHeight);
						if (_cardInnerSpacing > 0 && _cardViewGridlines == CardViewGridlines.FieldsOnly)
						{
							groupLayoutState._rect = new Rectangle(cardInnerSpacing, nextHeight - 1, otherWidth, previousHeight + 1);
						}
						switch (_cardViewGridlines)
						{
						case CardViewGridlines.FieldsOnly:
							if (_cardInnerSpacing == 0)
							{
								groupLayoutState._borderFlags = BorderFlags.Right;
							}
							break;
						case CardViewGridlines.Horizontal:
							groupLayoutState._borderFlags = BorderFlags.Bottom;
							if (_cardInnerSpacing > 0)
							{
								groupLayoutState._borderFlags |= BorderFlags.Top;
							}
							break;
						case CardViewGridlines.Vertical:
							groupLayoutState._borderFlags = BorderFlags.Right;
							if (_cardInnerSpacing > 0)
							{
								groupLayoutState._borderFlags |= BorderFlags.Left;
							}
							break;
						case CardViewGridlines.Both:
							groupLayoutState._borderFlags = BorderFlags.Right | BorderFlags.Bottom;
							if (_cardInnerSpacing > 0)
							{
								groupLayoutState._borderFlags |= BorderFlags.Left | BorderFlags.Top;
							}
							break;
						}
						gridTableLayoutState._arrayList.Add(groupLayoutState);
						cardInnerSpacing += value + _cardInnerSpacing;
					}
					if (width > otherWidth)
					{
						GridGroupLayoutState otherGroupLayoutState = new GridGroupLayoutState(LayoutItemType.CardCell, gridTableLayoutState, nextColumn);
						otherGroupLayoutState.CopyColumnMargins(nextColumn);
						switch (_cardViewGridlines)
						{
						case CardViewGridlines.Both:
						case CardViewGridlines.FieldsOnly:
							otherGroupLayoutState._borderFlags = BorderFlags.Bottom;
							if (_cardInnerSpacing > 0)
							{
								otherGroupLayoutState._borderFlags |= BorderFlags.Left | BorderFlags.Top | BorderFlags.Right;
							}
							break;
						case CardViewGridlines.Horizontal:
							otherGroupLayoutState._borderFlags = BorderFlags.Bottom;
							if (_cardInnerSpacing > 0)
							{
								otherGroupLayoutState._borderFlags |= BorderFlags.Top;
							}
							break;
						case CardViewGridlines.Vertical:
							if (_cardSpacing > 0)
							{
								otherGroupLayoutState._borderFlags = BorderFlags.Left | BorderFlags.Right;
							}
							break;
						}
						if (otherWidth > 0)
						{
							otherGroupLayoutState._rect = new Rectangle(cardInnerSpacing, nextHeight, width - otherWidth - _cardInnerSpacing * 3, previousHeight);
						}
						else
						{
							otherGroupLayoutState._rect = new Rectangle(cardInnerSpacing, nextHeight, width - _cardInnerSpacing * 2, previousHeight);
						}
						gridTableLayoutState._arrayList.Add(otherGroupLayoutState);
					}
					nextHeight += previousHeight + _cardInnerSpacing;
				}
				if (applyOverrides != gridFormatStyle)
				{
					applyOverrides.DisposeResources();
				}
				if (gridFormatStyle != otherFormatStyle)
				{
					gridFormatStyle.DisposeResources();
				}
			}
			if ((_cardViewGridlines == CardViewGridlines.Both || _cardViewGridlines == CardViewGridlines.Horizontal || CardViewGridlines == CardViewGridlines.FieldsOnly) && (condition || RowStateHelper.GetRowTag(gridRow.GetLogicalRow()).GetIsExpanded() || !_expandableCards))
			{
				nextHeight--;
			}
			gridTableLayoutState._contentSize = new Size(width, nextHeight);
			return gridTableLayoutState;
		}
	}
}
