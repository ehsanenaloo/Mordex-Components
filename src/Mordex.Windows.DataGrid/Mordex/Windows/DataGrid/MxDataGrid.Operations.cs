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
		void ISupportInitialize.BeginInit()
		{
			_isInitializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			if (_isInitializing)
			{
				_isInitializing = false;
				_currentLayout = _layouts.FindCurrentLayout();
				if (_currentLayout != null && _currentLayout != _gridLayout)
				{
					_currentLayout.ProcessDataGrid(this);
					ApplyCurrentLayout();
				}
				RefreshTablesAfterInit();
				OnLayoutLoad(EventArgs.Empty);
				ClearLayoutStates();
				SetDataSourceAndMember(_dataSource, _dataMember, true);
				NotifyRefreshListeners();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					if (SaveSettings && !base.DesignMode)
					{
						SaveComponentSettings();
					}
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
				AcceptCellEdit();
				ReleaseCustomEditStyle();
				DisposeTimers();
				if (_bufferedGraphics != null)
				{
					_bufferedGraphics.Dispose();
					_bufferedGraphics = null;
				}
				foreach (GridDropDown item in _dropDowns)
				{
					item.Dispose();
				}
				if (_gridFieldChooser != null)
				{
					_gridFieldChooser.Close();
				}
				if (_watermarkImage != null)
				{
					_watermarkImage.DetachFromGrid();
				}
				if (_gridGridColumnContainerControl != null)
				{
					_gridGridColumnContainerControl.Dispose();
					_gridGridColumnContainerControl = null;
				}
				SetHoverLayoutItem(null);
				ProcessCurrencyManager(null);
				DataSource = null;
				_rowCache.DisposeAllRows();
				base.Controls.Clear();
			}
			base.Dispose(disposing);
		}

		private void DisposeTimers()
		{
			DisposeTimer(ref _gridTimer);
			DisposeTimer(ref _editDelayTimer);
			DisposeTimer(ref _filterRowDelayTimer);
			DisposeTimer(ref _incrementalSearchTimer);
		}

		private static void DisposeTimer(ref System.Windows.Forms.Timer timer)
		{
			if (timer != null)
			{
				timer.Stop();
				timer.Dispose();
				timer = null;
			}
		}

		private void RunOnUiThread(MethodInvoker action)
		{
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			if (!base.InvokeRequired)
			{
				action();
				return;
			}
			if (!base.IsHandleCreated)
			{
				return;
			}
			try
			{
				BeginInvoke(action);
			}
			catch (InvalidOperationException)
			{
				// The handle was destroyed while the call was queued; there is nothing left to update.
			}
		}

		private void CheckIndexSuspended(int value)
		{
			if (!DataChanged)
			{
				_ = _gridDataSourceManager.RowManager.IsIndexSuspend;
			}
		}

		public void ClearStructure()
		{
			ReplaceRootTable(null, true);
		}

		public void RetrieveStructure()
		{
			RetrieveStructure(retrieveChildTables: false);
		}

		public void RetrieveStructure(bool retrieveChildTables)
		{
			if (BoundMode != BoundMode.Unbound)
			{
				_isCreatingBindingContext = true;
				if (_bindingContext == null)
				{
					UpdateBindingContext(new BindingContext());
				}
				_isCreatingBindingContext = false;
				ReplaceRootTable(null, false);
				_isBuildingBoundData = true;
				_gridDataSourceManager.CreateTableStructure(retrieveChildTables);
				_isBuildingBoundData = false;
				if (RootTable != null)
				{
					RootTable.UpdateDataGrid(this);
					ResetAfterRootTableChange(true);
				}
				else
				{
					OnRootTableChanged(EventArgs.Empty);
				}
				if (!_hierarchical)
				{
					_hierarchical = _gridTableCollection.Count > 1;
				}
			}
		}

		private void InitializeCollections()
		{
			_gridDataSourceManager = new GridDataSourceManager(this);
			_builtInTexts = new GridBuiltInTextList(this);
			_arrayList = new ArrayList();
			_editorsControlStyle = new ControlStyle(this);
			_gridSelectedItemCollection = new GridSelectedItemCollection(this);
			_rowCache = new GridRowCache(this);
			RebuildTableCollection();
			_formatStyleCollection = new GridFormatStyleCollection(this);
			_dropDowns = new GridDropDownCollection(this);
			_layouts = new GridLayoutCollection(this);
			_watermarkImage = new GridWatermarkImage(this);
			CreateDefaultFormatStyles();
			ApplyRecordNavigatorBuiltInText();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			LayoutReader mordexLayoutReader = new LayoutReader();
			ReadGridLayoutByMode(mordexLayoutReader, (PersistenceMode)0);
			SetDataBinding(null, "");
			RefreshRowCountAndInvalidate();
		}

		internal GridFormatStyle GetHeaderStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = null;
			gridFormatStyle = _headerFormatStyle.CloneWithDefaults(_defaultAlphaLevel, GetBackColorAlphaMode(), GetForeColorAlphaMode(), _blendColor);
			if (gridTable != null && !gridTable.HeaderFormatStyle.IsDefaultStyle())
			{
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridTable.HeaderFormatStyle);
				gridFormatStyle.DisposeResources();
				gridFormatStyle = applyOverrides;
			}
			return gridFormatStyle;
		}

		private GridFormatStyle GetGroupByBoxInfoStyle(GridTable gridTable)
		{
			GridFormatStyle gridFormatStyle = null;
			gridFormatStyle = _groupByBoxInfoFormatStyle.CloneWithDefaults(_defaultAlphaLevel, GetBackColorAlphaMode(), GetForeColorAlphaMode(), _blendColor);
			if (gridTable != null && gridTable.GroupByBoxInfoFormatStyle != null)
			{
				GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridTable.GroupByBoxInfoFormatStyle);
				if (gridFormatStyle != applyOverrides)
				{
					gridFormatStyle.DisposeResources();
					gridFormatStyle = applyOverrides;
				}
			}
			return gridFormatStyle;
		}

		private GridFormatStyle GetGroupByBoxStyle()
		{
			return _groupByBoxFormatStyle.CloneWithDefaults(_defaultAlphaLevel, GetBackColorAlphaMode(), GetForeColorAlphaMode(), _blendColor);
		}

		internal void RaiseSelectionChangedOrDefer()
		{
			if (_selectionChangeBatch != null && _selectionChangeBatch.IsActive())
			{
				_selectionChangeBatch.SetRefreshPending(true);
			}
			else
			{
				OnSelectionChanged(EventArgs.Empty);
			}
		}

		private UpdateScope BeginSelectionChangeBatch()
		{
			if (_selectionChangeBatch == null)
			{
				_selectionChangeBatch = new UpdateScope(this);
			}
			_selectionChangeBatch.Enter();
			return _selectionChangeBatch;
		}

		internal void ScrollToFirstRow()
		{
			SetFirstVisibleRow(0);
			EnsureVisible();
		}

		private bool HasFocusOrDropDown()
		{
			if (!base.ContainsFocus)
			{
				return CurrentCellDroppedDown;
			}
			return true;
		}

		private GridFormatStyle GetSelectionFormatStyle()
		{
			if (HasFocusOrDropDown())
			{
				return _selectedFormatStyle;
			}
			switch (_hideSelection)
			{
			case HideSelection.Highlight:
				return _selectedFormatStyle;
			case HideSelection.HighlightInactive:
				return _selectedInactiveFormatStyle;
			default:
				return null;
			}
		}

		private Bitmap CreateWatermarkShadowBitmap()
		{
			Bitmap bitmap = new Bitmap(5, 5);
			int value = 90;
			for (int i = 0; i < 5; i++)
			{
				int alpha = value;
				for (int j = 0; j < 5; j++)
				{
					bitmap.SetPixel(j, i, Color.FromArgb(alpha, Color.Black));
					alpha -= value / 5;
				}
				value -= 16;
			}
			return bitmap;
		}

		private CornerShadowRenderer GetCornerShadowRenderer()
		{
			if (_cachedImageRenderer == null)
			{
				_cachedImageRenderer = new CornerShadowRenderer();
			}
			return _cachedImageRenderer;
		}

		private void ApplyGotFocus()
		{
			if (_isFocusHandled)
			{
				return;
			}
			_isFocusHandled = true;
			base.OnGotFocus(EventArgs.Empty);
			InvalidateSelectedRows();
			if (_gridColumnContainerControl != null && _gridColumnContainerControl.Visible)
			{
				if (_gridColumnContainerControl.GetActiveEditControl() != null)
				{
					_gridColumnContainerControl.GetActiveEditControl().Select();
				}
				else
				{
					_gridColumnContainerControl.Select();
				}
			}
		}

		private void ApplyLostFocus()
		{
			if (_isFocusHandled && !base.ContainsFocus)
			{
				if (_isHeaderPressed)
				{
					EndHeaderDrag(true);
				}
				if (_isButtonPressActive)
				{
					EndColumnResize(true);
				}
				_isFocusHandled = false;
				base.OnLostFocus(EventArgs.Empty);
				InvalidateSelectedRows();
				ShowTooltipForLayoutItem(null);
			}
		}

		private Rectangle GetGroupByBoxRectangle()
		{
			if (!_groupByBoxVisible || _groupByBoxHeight < 1)
			{
				return Rectangle.Empty;
			}
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Height = _groupByBoxHeight;
			return clientRectangle;
		}

		internal Rectangle MirrorRectForPrintRtl(MxDataGridPrintDocument gridPrintDocument, Graphics graphics, Rectangle rectangle)
		{
			if (RightToLeft == RightToLeft.Yes)
			{
				Rectangle otherRectangle = gridPrintDocument._pageBounds;
				int value = (int)((double)otherRectangle.Right / (double)graphics.PageScale);
				int otherValue = rectangle.X - (int)((double)otherRectangle.X / (double)graphics.PageScale);
				return new Rectangle(value - otherValue - rectangle.Width, rectangle.Top, rectangle.Width, rectangle.Height);
			}
			return rectangle;
		}

		private bool ShouldTranslateSystemColors()
		{
			if (_owner != null && _owner.TranslateSystemColors)
			{
				return true;
			}
			return false;
		}

		private GridGroupLayoutState GetPressedLayoutItem()
		{
			return _gridGridGroupLayoutState;
		}

		private int FindNextColumnScrollOffset()
		{
			int value = -1;
			Graphics graphics = CreateGraphics();
			GridGroupLayoutState gridGroupLayoutState = null;
			if (RootTable != null)
			{
				GridFormatStyle gridFormatStyle = GetHeaderStyle(RootTable);
				MxRowLayoutState mordexRowLayoutState = RootTable.BuildColumnHeaderLayout(graphics, null, null, gridFormatStyle, GetEffectiveFrozenColumns(), true);
				gridFormatStyle.DisposeResources();
				int frozenAreaRight = GetFrozenAreaRight(mordexRowLayoutState);
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					if ((item._type == LayoutItemType.ColumnHeader || item._type == LayoutItemType.EmptyHeader) && item._isHorizontallyScrolled)
					{
						Rectangle rectangle = item.GetAbsoluteItemRect();
						if (rectangle.X > _horizontalScrollOffset + frozenAreaRight && (gridGroupLayoutState == null || gridGroupLayoutState.GetAbsoluteItemRect().X > rectangle.X))
						{
							gridGroupLayoutState = item;
							value = rectangle.X - frozenAreaRight;
						}
					}
				}
			}
			graphics.Dispose();
			if (value == -1)
			{
				return _horizontalScrollOffset + 20;
			}
			return value;
		}

		private int FindPreviousColumnScrollOffset()
		{
			int value = -1;
			Graphics graphics = CreateGraphics();
			GridGroupLayoutState gridGroupLayoutState = null;
			GridGroupLayoutState groupLayoutState = null;
			if (RootTable != null)
			{
				GridFormatStyle gridFormatStyle = GetHeaderStyle(RootTable);
				MxRowLayoutState mordexRowLayoutState = RootTable.BuildColumnHeaderLayout(graphics, null, null, gridFormatStyle, GetEffectiveFrozenColumns(), true);
				gridFormatStyle.DisposeResources();
				int frozenAreaRight = GetFrozenAreaRight(mordexRowLayoutState);
				foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
				{
					if ((item._type == LayoutItemType.ColumnHeader || item._type == LayoutItemType.EmptyHeader) && item._isHorizontallyScrolled)
					{
						Rectangle rectangle = item.GetAbsoluteItemRect();
						if (rectangle.Right > _horizontalScrollOffset)
						{
							groupLayoutState = item;
						}
						if (rectangle.X < _horizontalScrollOffset + frozenAreaRight && (gridGroupLayoutState == null || gridGroupLayoutState.GetAbsoluteItemRect().X < rectangle.X))
						{
							gridGroupLayoutState = item;
							value = rectangle.X - frozenAreaRight;
						}
					}
				}
				if (gridGroupLayoutState == null)
				{
					value = 0;
				}
				else if (groupLayoutState == null)
				{
					value = -1;
				}
			}
			graphics.Dispose();
			if (value == -1)
			{
				return _horizontalScrollOffset - 20;
			}
			return value;
		}

		private bool CanScrollByBlit()
		{
			if (_isRenderingPartial)
			{
				return false;
			}
			if (_bufferedGraphics == null)
			{
				return false;
			}
			if (_watermarkImage.Image != null)
			{
				return false;
			}
			if (BackgroundImage != null)
			{
				return false;
			}
			if ((OwnerDrawnAreas & GridOwnerDrawnArea.Background) != GridOwnerDrawnArea.None)
			{
				return false;
			}
			if (RowCount == 0 && _builtInTexts[GridBuiltInText.EmptyGridInfo].Length > 0)
			{
				return false;
			}
			if (_bodyRowLayouts != null)
			{
				foreach (MxRowLayoutState item in _bodyRowLayouts)
				{
					if (item._rowType == LayoutRowType.GroupHeaderRow || item._rowType == LayoutRowType.GroupTotalRow)
					{
						return false;
					}
				}
			}
			return true;
		}

		private void FlushPendingPaint()
		{
			if (!GetPendingInvalidRect().IsEmpty && _bufferedGraphics != null && _bufferedGraphics != null)
			{
				SetPaintClipRectangle(GetPendingInvalidRect());
				_isRenderingPartial = true;
				PaintControl(_bufferedGraphics.Graphics);
				_isRenderingPartial = false;
				SetPendingInvalidRect(Rectangle.Empty);
			}
		}

		private void ToggleRenderingOriginY()
		{
			if (_bufferedGraphics != null)
			{
				Point renderingOrigin = _bufferedGraphics.Graphics.RenderingOrigin;
				if (renderingOrigin.Y == 0)
				{
					renderingOrigin.Y = 1;
				}
				else
				{
					renderingOrigin.Y = 0;
				}
				_bufferedGraphics.Graphics.RenderingOrigin = renderingOrigin;
			}
		}

		private void ToggleRenderingOriginX()
		{
			if (_bufferedGraphics != null)
			{
				Point renderingOrigin = _bufferedGraphics.Graphics.RenderingOrigin;
				if (renderingOrigin.X == 0)
				{
					renderingOrigin.X = 1;
				}
				else
				{
					renderingOrigin.X = 0;
				}
				_bufferedGraphics.Graphics.RenderingOrigin = renderingOrigin;
			}
		}

		private void ValidateClientArea()
		{
			try
			{
				NativeMethods.NativeRectStruct e = default(NativeMethods.NativeRectStruct);
				Rectangle bounds = base.Bounds;
				e._left = 0;
				e._nativeMethodsData4Integer = 0;
				e._bottom = bounds.Bottom;
				e._right = bounds.Right;
				NativeMethods.ValidateRect(base.Handle, ref e);
				SetPendingInvalidRect(Rectangle.Empty);
				_isBufferStale = true;
			}
			finally
			{
			}
		}

		private GridColumn GetSelectionAnchorColumn()
		{
			return _gridColumn;
		}

		private void FocusEditorOrGrid()
		{
			try
			{
				if (_gridColumnContainerControl != null && _editDelayTimer == null)
				{
					if (_gridColumnContainerControl.Visible)
					{
						_gridColumnContainerControl.Select();
					}
					else
					{
						Select();
					}
				}
				else
				{
					Select();
				}
			}
			finally
			{
			}
		}

		private void StartAutoScrollTimer()
		{
			if (_gridTimer == null)
			{
				_gridTimer = new System.Windows.Forms.Timer();
				_gridTimer.Interval = Timing.AutoRepeatInterval;
				_gridTimer.Tick += OnAutoScrollTimerTick;
				_gridTimer.Start();
			}
		}

		private void StopAutoScrollTimer()
		{
			if (_gridTimer != null)
			{
				_gridTimer.Stop();
				_gridTimer.Dispose();
				_gridTimer = null;
			}
		}

		private Rectangle RectangleFromCorners(Point point, Point location)
		{
			int x = point.X;
			int x2 = location.X;
			int y = point.Y;
			int y2 = location.Y;
			if (x > x2)
			{
				x = location.X;
				x2 = point.X;
			}
			if (y > y2)
			{
				y = location.Y;
				y2 = point.Y;
			}
			return Rectangle.FromLTRB(x, y, x2 + 1, y2 + 1);
		}

		private bool IsPointOverControl(Control control, Point point)
		{
			if (!control.Visible)
			{
				return false;
			}
			if (control.TopLevelControl == null || !(control.TopLevelControl is Form))
			{
				return false;
			}
			return control.ClientRectangle.Contains(control.PointToClient(point));
		}

		public GridGroup GroupByBoxHeaderFromPoint(int x, int y)
		{
			GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(x, y);
			if (gridGroupLayoutState._type == LayoutItemType.GroupByBoxHeader)
			{
				return gridGroupLayoutState._gridGroup;
			}
			return null;
		}

		private void ClearTooltipText()
		{
			if (_gridWindowMessageHandler != null)
			{
				_gridWindowMessageHandler.SetTooltipText("");
			}
		}

		private void EnsureTooltipWindow()
		{
			if (_gridWindowMessageHandler == null)
			{
				_gridWindowMessageHandler = new GridWindowMessageHandler(this, base.UseCompatibleTextRendering);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTreeLineColor()
		{
			TreeLineColor = SystemColors.ControlDark;
		}

		private Image GetImageByIndex(int value)
		{
			if (value != -1)
			{
				ImageList imageList = ComputeImageList();
				if (imageList != null && value >= 0 && value < imageList.Images.Count)
				{
					return imageList.Images[value];
				}
			}
			return null;
		}

		internal void ResetTotalsCache()
		{
			_isClearing = false;
			if (_dictionary != null)
			{
				_dictionary.Clear();
				_dictionary = null;
			}
		}

		private void MoveCurrentDownOneLine()
		{
			if (_view == View.TableView)
			{
				if (CurrentTable != null)
				{
					if (CurrentTable.CellLayoutMode == CellLayoutMode.UseColumns)
					{
						MoveCurrentToNextRow(true);
						Update();
						return;
					}
					if (_currentColumn == null)
					{
						MoveCurrentToNextRow(true);
						Update();
						return;
					}
					GridColumn gridColumn = null;
					for (int i = _currentColumn._columnSetRow + 1; i < CurrentTable.ColumnSetRowCount; i++)
					{
						gridColumn = _currentColumn._owner[i, _currentColumn._columnSetColumn];
						if (gridColumn != null && gridColumn != _currentColumn)
						{
							break;
						}
						gridColumn = null;
					}
					if (gridColumn != null)
					{
						IsColumnFocusableInCurrentRow(gridColumn);
						SyncCellSelectionWithCurrent();
						Update();
						return;
					}
					for (int j = 0; j < CurrentTable.ColumnSetRowCount; j++)
					{
						gridColumn = _currentColumn._owner[j, _currentColumn._columnSetColumn];
						if (gridColumn != null)
						{
							if (Row < RowCount - 1)
							{
								CanFocusCell(Row + 1, gridColumn);
								SyncCellSelectionWithCurrent();
								Update();
							}
							break;
						}
					}
				}
				else
				{
					MoveCurrentToPreviousRow(true);
					Update();
				}
			}
			else if (_row == null)
			{
				HasFocusableColumnFrom(0, 0);
				SyncCellSelectionWithCurrent();
				Update();
			}
			else if (_currentColumn == null)
			{
				if (RowStateHelper.GetRowTag(_row.GetLogicalRow()).GetIsExpanded())
				{
					HasFocusableColumnFrom(Row, 0);
					return;
				}
				CanFocusCell(Row + 1, null);
				SyncCellSelectionWithCurrent();
			}
			else
			{
				MoveCurrentToNextCell();
			}
		}

		private void MoveCurrentUpOneLine()
		{
			if (_view == View.TableView)
			{
				if (CurrentTable != null)
				{
					if (CurrentTable.CellLayoutMode == CellLayoutMode.UseColumns)
					{
						MoveCurrentToPreviousRow(true);
						Update();
						return;
					}
					if (_currentColumn == null)
					{
						MoveCurrentToPreviousRow(true);
						Update();
						return;
					}
					GridColumn gridColumn = null;
					for (int value = _currentColumn._columnSetRow - 1; value >= 0; value--)
					{
						gridColumn = _currentColumn._owner[value, _currentColumn._columnSetColumn];
						if (gridColumn != null && gridColumn != _currentColumn)
						{
							break;
						}
						gridColumn = null;
					}
					if (gridColumn != null)
					{
						IsColumnFocusableInCurrentRow(gridColumn);
						SyncCellSelectionWithCurrent();
						Update();
						return;
					}
					for (int i = CurrentTable.ColumnSetRowCount - 1; i >= 0; i--)
					{
						gridColumn = _currentColumn._owner[i, _currentColumn._columnSetColumn];
						if (gridColumn != null)
						{
							if (Row == 0)
							{
								if (IsNewRowAtTop())
								{
									CanFocusCell(-1, gridColumn);
									SyncCellSelectionWithCurrent();
									Update();
								}
							}
							else if (Row > 0)
							{
								CanFocusCell(Row - 1, gridColumn);
								SyncCellSelectionWithCurrent();
								Update();
							}
							break;
						}
					}
				}
				else
				{
					MoveCurrentToPreviousRow(true);
					Update();
				}
			}
			else if (_row == null)
			{
				HasFocusableColumnFrom(0, 0);
				SyncCellSelectionWithCurrent();
				Update();
			}
			else
			{
				MoveCurrentToPreviousCell();
			}
		}

		private void RemoveSelectedItemAtPosition(int value)
		{
			foreach (GridSelectedItem item in (IEnumerable)_gridSelectedItemCollection)
			{
				if (item.Position == value)
				{
					_gridSelectedItemCollection.Remove(item);
					break;
				}
			}
		}

		private void ProcessException(Exception exception, bool displayErrorMessage)
		{
			if (_eventErrorHandling)
			{
				ErrorEventArgs e = new ErrorEventArgs(exception);
				e.DisplayErrorMessage = displayErrorMessage;
				OnError(e);
				if (e.DisplayErrorMessage)
				{
					bool containsFocus = base.ContainsFocus;
					Mordex.Windows.Common.MxMessageBox.Show(e.ErrorMessage);
					if (containsFocus)
					{
						GridHelper.SelectAndFocus(this);
					}
				}
			}
			else if (displayErrorMessage)
			{
				throw exception;
			}
		}

		internal Color ResolveGridLineColor()
		{
			// A themed grid takes the grid line color of the theme until the developer sets another color.
			if (_gridControlStyle.GetLayoutState().GetGridlinesStyle()._isThemed
				|| (_gridControlStyle.GetLayoutState().GetGridlinesStyle().IsThemedLook && _gridLineColor == SystemColors.ControlDark))
			{
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.Gridline);
			}
			return _gridLineColor;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetGridLineColor()
		{
			GridLineColor = SystemColors.ControlDark;
		}

		internal GridLines ResolveGridLines()
		{
			if (_gridLines == GridLines.Default)
			{
				return GridLines.Both;
			}
			return _gridLines;
		}

		internal bool ShouldRepeatHeaders()
		{
			if (_repeatHeaders == InheritableBoolean.False)
			{
				return false;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBlendColor()
		{
			BlendColor = SystemColors.Window;
		}

		public string GetClipString()
		{
			return GetClipString(includeParentRows: true, includeHeaders: true, '\t', '\r');
		}

		public string GetClipString(bool includeParentRows, bool includeHeaders, char columnDelimeter, char rowDelimeter)
		{
			ArrayList arrayList = new ArrayList();
			MordexRowManager mordexRowManager = GetRowManager();
			if (_gridSelectedItemCollection.Count == 0)
			{
				return "";
			}
			_gridSelectedItemCollection.Sort();
			foreach (GridSelectedItem item in (IEnumerable)SelectedItems)
			{
				MordexRow mordexRow = mordexRowManager[item.Position];
				if (mordexRow is VirtualDataRow || mordexRow is MordexNewDataRow)
				{
					continue;
				}
				if (includeParentRows)
				{
					MordexRow parentRow = mordexRow.ParentRow;
					while (!(parentRow is MordexRootRow))
					{
						if (!parentRow.IsVirtualRow() && !RowStateHelper.GetRowTag(parentRow).GetDataState3Boolean() && !arrayList.Contains(parentRow))
						{
							arrayList.Add(parentRow);
						}
						parentRow = parentRow.ParentRow;
					}
				}
				arrayList.Add(mordexRow);
			}
			StringBuilder stringBuilder = new StringBuilder();
			GridTable gridTable = null;
			bool condition = true;
			foreach (MordexRow arrayListItem in arrayList)
			{
				if (!condition)
				{
					stringBuilder.Append(rowDelimeter);
				}
				else
				{
					condition = false;
				}
				if (includeHeaders && arrayListItem.Table != gridTable)
				{
					gridTable = (GridTable)arrayListItem.Table;
					if (gridTable.AppendClipHeaders(stringBuilder, columnDelimeter, rowDelimeter))
					{
						stringBuilder.Append(rowDelimeter);
					}
				}
				GridRow gridRow = GetGridRowForLogicalRow(arrayListItem);
				PrepareRowForDisplay(gridRow, false);
				gridRow.AppendClipText(stringBuilder, columnDelimeter, rowDelimeter);
			}
			return stringBuilder.ToString();
		}

		private void CreateDefaultFormatStyles()
		{
			_rowFormatStyle = new GridFormatStyle(this, PredefinedStyle.RowFormatStyle);
			_headerFormatStyle = new GridFormatStyle(this, PredefinedStyle.HeaderFormatStyle);
			_rowHeaderFormatStyle = new GridFormatStyle(this, PredefinedStyle.RowHeaderFormatStyle);
			_alternatingRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.AlternatingRowFormatStyle);
			_groupByBoxFormatStyle = new GridFormatStyle(this, PredefinedStyle.GroupByBoxFormatStyle);
			_groupByBoxInfoFormatStyle = new GridFormatStyle(this, PredefinedStyle.GroupByBoxInfoFormatStyle);
			_groupRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.GroupRowFormatStyle);
			_groupTotalRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.GroupTotalRowFormatStyle);
			_totalRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.TotalRowFormatStyle);
			_gridGridFormatStyle = new GridFormatStyle(this, PredefinedStyle.NewRowFormatStyle);
			_previewRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.PreviewRowFormatStyle);
			_selectedFormatStyle = new GridFormatStyle(this, PredefinedStyle.SelectedFormatStyle);
			_selectedInactiveFormatStyle = new GridFormatStyle(this, PredefinedStyle.SelectedInactiveFormatStyle);
			_cardCaptionFormatStyle = new GridFormatStyle(this, PredefinedStyle.CardCaptionFormatStyle);
			_cardColumnHeaderFormatStyle = new GridFormatStyle(this, PredefinedStyle.CardColumnHeaderFormatStyle);
			_filterRowFormatStyle = new GridFormatStyle(this, PredefinedStyle.FilterRowFormatStyle);
			_linkFormatStyle = new GridFormatStyle(this, PredefinedStyle.LinkFormatStyle);
			_rowWithErrorsFormatStyle = new GridFormatStyle(this, PredefinedStyle.RowWithErrorsFormatStyle);
			_tableHeaderFormatStyle = new GridFormatStyle(this, PredefinedStyle.TableHeaderFormatStyle);
			_focusCellFormatStyle = new GridFormatStyle(this, PredefinedStyle.FocusCellFormatStyle);
		}

		internal AlphaMode GetBackColorAlphaMode()
		{
			if (_defaultBackColorAlphaMode == AlphaMode.Empty)
			{
				return _defaultAlphaMode;
			}
			return _defaultBackColorAlphaMode;
		}

		internal AlphaMode GetForeColorAlphaMode()
		{
			if (_defaultForeColorAlphaMode == AlphaMode.Empty)
			{
				return _defaultAlphaMode;
			}
			return _defaultForeColorAlphaMode;
		}

		public static string GetGridVersionString()
		{
			string localPath = typeof(MxDataGrid).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		internal Image GetErrorImage()
		{
			if (_errorImageKey.Length > 0)
			{
				return GetImageByKey(_errorImageKey);
			}
			if (_errorImageIndex == -2)
			{
				Bitmap bitmap = ResourceImages.GetErrorImage();
				bitmap.MakeTransparent(Color.Magenta);
				return bitmap;
			}
			return GetImageByIndex(_errorImageIndex);
		}

		private int GetFrozenAreaRight(MxRowLayoutState mordexRowLayoutState)
		{
			if (_view == View.TableView && GetEffectiveFrozenColumns() != 0)
			{
				if (mordexRowLayoutState._owner.CellLayoutMode == CellLayoutMode.UseColumns)
				{
					int right = 0;
					{
						foreach (GridGroupLayoutState item in mordexRowLayoutState._arrayList)
						{
							if (!item._isHorizontallyScrolled)
							{
								Rectangle rectangle = item.GetAbsoluteItemRect();
								if (rectangle.Right > right)
								{
									right = rectangle.Right;
								}
							}
						}
						return right;
					}
				}
				return 0;
			}
			return 0;
		}

		internal Color GetPredefinedStyleBackColor(PredefinedStyle predefinedStyle)
		{
			switch (predefinedStyle)
			{
			case PredefinedStyle.GroupByBoxFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGroupByBoxStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.GroupByBox);
			case PredefinedStyle.GroupByBoxInfoFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGroupByBoxStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.GroupByBoxInfo);
			case PredefinedStyle.HeaderFormatStyle:
				return _gridControlStyle.GetLayoutState().GetHeadersStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.Header);
			case PredefinedStyle.GroupRowFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGroupRowsStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.GroupRow);
			case PredefinedStyle.AlternatingRowFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.AlternatingRow);
			case PredefinedStyle.CardCaptionFormatStyle:
				return _gridControlStyle.GetLayoutState().GetCardsStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.CardCaption);
			case PredefinedStyle.SelectedInactiveFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.SelectedInactive);
			case PredefinedStyle.RowFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.WindowColor);
			case PredefinedStyle.SelectedFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.HighlightColor);
			case PredefinedStyle.NewRowFormatStyle:
			case PredefinedStyle.GroupTotalRowFormatStyle:
			case PredefinedStyle.TotalRowFormatStyle:
			case PredefinedStyle.PreviewRowFormatStyle:
			case PredefinedStyle.FilterRowFormatStyle:
			case PredefinedStyle.CardColumnHeaderFormatStyle:
			case PredefinedStyle.LinkFormatStyle:
			case PredefinedStyle.RowWithErrorsFormatStyle:
			case PredefinedStyle.TableHeaderFormatStyle:
			case PredefinedStyle.FocusCellFormatStyle:
				return Color.Empty;
			default:
				return Color.Empty;
			}
		}

		internal Color GetPredefinedStyleForeColor(PredefinedStyle predefinedStyle)
		{
			switch (predefinedStyle)
			{
			case PredefinedStyle.RowFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.WindowTextColor);
			case PredefinedStyle.GroupByBoxInfoFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGroupByBoxStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.GroupByBoxInfoText);
			case PredefinedStyle.HeaderFormatStyle:
				return _gridControlStyle.GetLayoutState().GetHeadersStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.HeaderText);
			case PredefinedStyle.GroupRowFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGroupRowsStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.GroupRowText);
			case PredefinedStyle.CardCaptionFormatStyle:
				return _gridControlStyle.GetLayoutState().GetCardsStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.CardCaptionText);
			case PredefinedStyle.SelectedFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.HighlightTextColor);
			case PredefinedStyle.SelectedInactiveFormatStyle:
				return _gridControlStyle.GetLayoutState().GetGridlinesStyle().GetResolvedAppearance()
					.GetGridColor(GridColorKey.SelectedInactiveTextColor);
			default:
				return Color.Empty;
			}
		}

		IMordexCondition ISupportFilterBuilder.CreateCondition()
		{
			return new GridFilterCondition();
		}

		private void AllocateBufferedGraphics()
		{
			DoubleBuffered = false;
			if (base.IsHandleCreated)
			{
				if (_bufferedGraphics != null)
				{
					_bufferedGraphics.Dispose();
					_bufferedGraphics = null;
				}
				Rectangle bounds = base.Bounds;
				if (bounds.Width > 0 && bounds.Height > 0)
				{
					BufferedGraphicsManager.Current.MaximumBuffer = new Size(base.Bounds.Width + 1, base.Bounds.Height + 1);
					bounds.Location = new Point(0, 0);
					using (Graphics targetGraphics = CreateGraphics())
					{
						_bufferedGraphics = BufferedGraphicsManager.Current.Allocate(targetGraphics, bounds);
					}
					_isBufferStale = false;
					SetPendingInvalidRect(new Rectangle(0, 0, base.Width, base.Height));
				}
			}
		}

		private Rectangle GetPendingInvalidRect()
		{
			return _bufferedClipRectangle;
		}

		private void SetPendingInvalidRect(Rectangle bufferedClipRectangle)
		{
			_bufferedClipRectangle = bufferedClipRectangle;
		}
	}
}
