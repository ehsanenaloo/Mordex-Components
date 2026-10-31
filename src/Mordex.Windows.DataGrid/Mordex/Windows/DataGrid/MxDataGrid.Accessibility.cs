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
		internal void NotifyAccessibilityReorder()
		{
			RefreshRowCountAndInvalidate();
			if (_tableAccessibleObject != null)
			{
				_tableAccessibleObject.NotifyClients(AccessibleEvents.Reorder);
			}
		}

		internal Rectangle GetAccessibleCellBounds(RowAccessibleObject mordexRowAccessibleObject, GridColumn gridColumn, bool condition)
		{
			return GetAccessibleItemBounds(mordexRowAccessibleObject, gridColumn, condition, LayoutItemType.EmptyItem);
		}

		internal Rectangle GetAccessibleItemBounds(RowAccessibleObject mordexRowAccessibleObject, GridColumn gridColumn, bool condition, LayoutItemType layoutItemType)
		{
			CalculateLayoutUntilComplete();
			if (mordexRowAccessibleObject.GetRow() is MordexTableRow)
			{
				return Rectangle.Empty;
			}
			if (_view == View.TableView)
			{
				MxRowLayoutState mordexRowLayoutState = null;
				if (_headerRowLayouts != null)
				{
					foreach (MxRowLayoutState item in _headerRowLayouts)
					{
						if (item.GridRow != null && item.GridRow.GetLogicalRow() == mordexRowAccessibleObject.GetRow())
						{
							mordexRowLayoutState = item;
							break;
						}
					}
				}
				if (mordexRowLayoutState == null && _bodyRowLayouts != null)
				{
					foreach (MxRowLayoutState bodyRowLayout in _bodyRowLayouts)
					{
						if (bodyRowLayout.GridRow != null && bodyRowLayout.GridRow.GetLogicalRow() == mordexRowAccessibleObject.GetRow())
						{
							mordexRowLayoutState = bodyRowLayout;
							break;
						}
					}
				}
				if (mordexRowLayoutState == null && _footerRowLayouts != null)
				{
					{
						IEnumerator enumerator = _footerRowLayouts.GetEnumerator();
						try
						{
							if (enumerator.MoveNext())
							{
								MxRowLayoutState otherMordexRowLayoutState = (MxRowLayoutState)enumerator.Current;
								mordexRowLayoutState = otherMordexRowLayoutState;
							}
						}
						finally
						{
							IDisposable disposable = enumerator as IDisposable;
							if (disposable != null)
							{
								disposable.Dispose();
							}
						}
					}
				}
				if (mordexRowLayoutState != null)
				{
					if (condition)
					{
						Rectangle rectangle = mordexRowLayoutState.GetCellsBounds();
						rectangle = AdjustRectangleToViewport(rectangle, GetEffectiveFrozenColumns() == 0);
						rectangle.Intersect(base.ClientRectangle);
						if (rectangle.Width == 0 && rectangle.Height == 0)
						{
							return Rectangle.Empty;
						}
						rectangle.Location = PointToScreen(rectangle.Location);
						return rectangle;
					}
					bool otherCondition = false;
					bool nextCondition = layoutItemType == LayoutItemType.GroupExpandBox && gridColumn == null;
					bool previousCondition = layoutItemType == LayoutItemType.ExpandBox && gridColumn == null;
					foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
					{
						if (nextCondition && arrayListItem._type == LayoutItemType.GroupExpandBox)
						{
							otherCondition = true;
						}
						else if (previousCondition && arrayListItem._type == LayoutItemType.ExpandBox)
						{
							otherCondition = true;
						}
						else if (gridColumn == null && arrayListItem._type == LayoutItemType.RowHeader)
						{
							otherCondition = true;
						}
						else if (gridColumn == arrayListItem._column && (arrayListItem._type == LayoutItemType.Cell || arrayListItem._type == LayoutItemType.TotalRowCell || arrayListItem._type == LayoutItemType.GroupTotalCell))
						{
							otherCondition = true;
						}
						GridGroupLayoutState gridGroupLayoutState = null;
						if (otherCondition)
						{
							gridGroupLayoutState = arrayListItem;
							if (otherCondition && !nextCondition && !previousCondition && layoutItemType != LayoutItemType.EmptyItem)
							{
								otherCondition = false;
								GridGroupLayoutState otherGridGroupLayoutState = null;
								if (arrayListItem._children != null)
								{
									GridGroupLayoutState[] gridGroupLayoutStateArray = arrayListItem._children;
									foreach (GridGroupLayoutState gridGroupLayoutStateArrayItem in gridGroupLayoutStateArray)
									{
										if (gridGroupLayoutStateArrayItem._type == layoutItemType)
										{
											otherGridGroupLayoutState = gridGroupLayoutStateArrayItem;
											otherCondition = true;
											break;
										}
									}
								}
								gridGroupLayoutState = otherGridGroupLayoutState;
							}
						}
						if (otherCondition)
						{
							Rectangle result = AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
							result.Intersect(base.ClientRectangle);
							if (result.Width > 0 && result.Height > 0)
							{
								result.Location = PointToScreen(result.Location);
								return result;
							}
							break;
						}
					}
				}
			}
			else if (_cardRowLayouts != null)
			{
				bool firstCondition = false;
				foreach (GridTableLayoutState cardRowLayout in _cardRowLayouts)
				{
					if (cardRowLayout.GridRow == null || cardRowLayout.GridRow.GetLogicalRow() != mordexRowAccessibleObject.GetRow())
					{
						continue;
					}
					if (condition)
					{
						Rectangle bounds = cardRowLayout.GetBounds();
						bounds = AdjustRectangleToViewport(bounds, true);
						bounds.Intersect(base.ClientRectangle);
						if (bounds.Width == 0 && bounds.Height == 0)
						{
							return Rectangle.Empty;
						}
						bounds.Location = PointToScreen(bounds.Location);
						return bounds;
					}
					foreach (GridGroupLayoutState otherArrayListItem in cardRowLayout._arrayList)
					{
						if (gridColumn == null && otherArrayListItem._type == LayoutItemType.CardCaption)
						{
							firstCondition = true;
						}
						else if (gridColumn == otherArrayListItem._column && otherArrayListItem._type == LayoutItemType.CardCell)
						{
							firstCondition = true;
						}
						if (firstCondition)
						{
							Rectangle adjustRectangleToViewport = AdjustRectangleToViewport(otherArrayListItem.GetAbsoluteItemRect(), otherArrayListItem._isHorizontallyScrolled);
							adjustRectangleToViewport.Intersect(base.ClientRectangle);
							if (adjustRectangleToViewport.Width > 0 && adjustRectangleToViewport.Height > 0)
							{
								adjustRectangleToViewport.Location = PointToScreen(adjustRectangleToViewport.Location);
								return adjustRectangleToViewport;
							}
							break;
						}
					}
				}
			}
			return Rectangle.Empty;
		}

		internal Rectangle GetAccessibleColumnHeaderBounds(HeaderRowAccessibleObject accessibilityState, GridColumn gridColumn, bool condition)
		{
			if (View != View.TableView)
			{
				return Rectangle.Empty;
			}
			CalculateLayoutUntilComplete();
			MxRowLayoutState mordexRowLayoutState = null;
			if (accessibilityState.GetRow() == GetRowSourceManager().RootRow)
			{
				if (_headerRowLayouts != null)
				{
					foreach (MxRowLayoutState item in _headerRowLayouts)
					{
						if (item._rowType == LayoutRowType.ColumnHeader && item.LayoutTable == accessibilityState.GetOwnerTable())
						{
							mordexRowLayoutState = item;
							break;
						}
					}
				}
			}
			else if (_bodyRowLayouts != null)
			{
				mordexRowLayoutState = _bodyRowLayouts.FindRow(LayoutRowType.ColumnHeader, accessibilityState.GetRow().FirstChild, false);
			}
			if (mordexRowLayoutState != null)
			{
				if (condition)
				{
					Rectangle result = mordexRowLayoutState.GetCellsBounds();
					result.Intersect(base.ClientRectangle);
					if (result.Width == 0 && result.Height == 0)
					{
						return Rectangle.Empty;
					}
					result.Location = PointToScreen(result.Location);
					return result;
				}
				bool otherCondition = false;
				foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
				{
					if (gridColumn == null && arrayListItem._type == LayoutItemType.RowHeader)
					{
						otherCondition = true;
					}
					else if (gridColumn == arrayListItem._column && arrayListItem._type == LayoutItemType.ColumnHeader)
					{
						otherCondition = true;
					}
					if (otherCondition)
					{
						Rectangle adjustRectangleToViewport = AdjustRectangleToViewport(arrayListItem.GetAbsoluteItemRect(), arrayListItem._isHorizontallyScrolled);
						adjustRectangleToViewport.Intersect(base.ClientRectangle);
						if (adjustRectangleToViewport.Width > 0 && adjustRectangleToViewport.Height > 0)
						{
							adjustRectangleToViewport.Location = PointToScreen(adjustRectangleToViewport.Location);
							return adjustRectangleToViewport;
						}
						break;
					}
				}
			}
			return Rectangle.Empty;
		}

		internal AccessibleObject GetTableAccessibleObject(MordexRow mordexRow)
		{
			if (mordexRow is MordexTableRow mordexTableRow)
			{
				if (mordexTableRow.Table == RootTable)
				{
					return GetOrCreateRowAccessibleObject(mordexTableRow);
				}
				if (GetOrCreateRowAccessibleObject(mordexRow) is ChildTableAccessibleObject accessibilityState)
				{
					return accessibilityState.GetHeaderRow();
				}
			}
			return null;
		}

		internal AccessibleObject ComputeAccessibleObject(MordexRow mordexRow, GridColumn gridColumn)
		{
			if (GetOrCreateRowAccessibleObject(mordexRow) is RowAccessibleObject mordexRowAccessibleObject && gridColumn != null)
			{
				return mordexRowAccessibleObject.ComputeAccessibleObject(gridColumn);
			}
			return null;
		}

		internal AccessibleObject GetOrCreateRowAccessibleObject(MordexRow dataRow)
		{
			AccessibleObject accessibleObject = dataRow.AccessibilityTag as AccessibleObject;
			if (accessibleObject != null)
			{
				return accessibleObject;
			}
			if (dataRow is MordexTableRow mordexTableRow)
			{
				accessibleObject = (AccessibleObject)(mordexTableRow.AccessibilityTag = ((mordexTableRow.Table != RootTable) ? ((RowAccessibleObject)new ChildTableAccessibleObject(this, mordexTableRow)) : ((RowAccessibleObject)new HeaderRowAccessibleObject(this, mordexTableRow, base.AccessibilityObject))));
			}
			else if (dataRow is MordexDataRow mordexDataRow)
			{
				accessibleObject = (AccessibleObject)(mordexDataRow.AccessibilityTag = new AccessibilityState(this, mordexDataRow));
			}
			else if (dataRow is MordexGroupRow mordexGroupRow)
			{
				accessibleObject = (AccessibleObject)(mordexGroupRow.AccessibilityTag = new GroupRowAccessibleObject(this, mordexGroupRow));
			}
			else if (dataRow is MordexNewDataRow mordexNewDataRow)
			{
				accessibleObject = (AccessibleObject)(mordexNewDataRow.AccessibilityTag = new AccessibilityState(this, dataRow));
			}
			else if (dataRow is MordexTotalRow mordexTotalRow)
			{
				accessibleObject = (AccessibleObject)(mordexTotalRow.AccessibilityTag = new TotalRowAccessibleObject(this, dataRow));
			}
			return accessibleObject;
		}

		internal AccessibleObject GetParentAccessibleObject(MordexRow mordexRow)
		{
			MordexRow rootRow = GetRowSourceManager().RootRow;
			if (mordexRow == rootRow || mordexRow.ParentRow == rootRow)
			{
				return base.AccessibilityObject;
			}
			if (mordexRow.ParentRow != null)
			{
				return GetOrCreateRowAccessibleObject(mordexRow.ParentRow);
			}
			return null;
		}

		internal AccessibleObject GetAccessibleObjectAtPoint(int value, int otherValue)
		{
			CalculateLayoutUntilComplete();
			Point pt = PointToClient(new Point(value, otherValue));
			if (!base.ClientRectangle.Contains(pt))
			{
				return null;
			}
			GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(pt.X, pt.Y);
			if (_view == View.TableView)
			{
				if ((gridGroupLayoutState == null || gridGroupLayoutState._type == LayoutItemType.EmptyItem) && GetGroupByBoxRectangle().Contains(pt.X, pt.Y))
				{
					return GetOrCreateGridAccessibleObject();
				}
				switch (gridGroupLayoutState._type)
				{
				case LayoutItemType.CellColumnButton:
				case LayoutItemType.CellDropDownButton:
				case LayoutItemType.CellUpButton:
				case LayoutItemType.CellDownButton:
					if (gridGroupLayoutState._gridGroupLayoutStateOwner != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow != null)
					{
						MordexRow logicalRow = gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow();
						RowAccessibleObject mordexRowAccessibleObject = null;
						if (logicalRow != null)
						{
							mordexRowAccessibleObject = GetOrCreateRowAccessibleObject(logicalRow) as RowAccessibleObject;
						}
						if (mordexRowAccessibleObject != null && mordexRowAccessibleObject.ComputeAccessibleObject(gridGroupLayoutState._column) is GridColumnAccessibleObject gridColumnAccessibleObject)
						{
							return gridColumnAccessibleObject.GetButtonAccessibleObject(gridGroupLayoutState._type);
						}
					}
					break;
				case LayoutItemType.GroupByBoxInfo:
					return GetOrCreateGridAccessibleObject().GetChild(0);
				case LayoutItemType.Cell:
				case LayoutItemType.TotalRowCell:
				case LayoutItemType.GroupTotalCell:
					if (gridGroupLayoutState._gridGroupLayoutStateOwner != null && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow != null)
					{
						MordexRow otherLogicalRow = gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow();
						RowAccessibleObject orCreateRowAccessibleObject = null;
						if (otherLogicalRow != null)
						{
							orCreateRowAccessibleObject = GetOrCreateRowAccessibleObject(otherLogicalRow) as RowAccessibleObject;
						}
						if (orCreateRowAccessibleObject != null)
						{
							return orCreateRowAccessibleObject.ComputeAccessibleObject(gridGroupLayoutState._column);
						}
					}
					break;
				case LayoutItemType.ColumnHeader:
					if (gridGroupLayoutState._gridGroupLayoutStateOwner is MxRowLayoutState mordexRowLayoutState && gridGroupLayoutState._column != null)
					{
						if (mordexRowLayoutState._mordexRow == null)
						{
							return GetTableAccessibleObject(GetRowSourceManager().RootRow).GetChild(gridGroupLayoutState._column.Position + 1);
						}
						MordexRow otherParentRow = mordexRowLayoutState._mordexRow.ParentRow;
						if (otherParentRow != null)
						{
							return GetTableAccessibleObject(otherParentRow).GetChild(gridGroupLayoutState._column._position + 1);
						}
					}
					break;
				case LayoutItemType.GroupExpandBox:
				{
					MordexRow nextLogicalRow = gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow();
					AccessibleObject accessibleObject = GetOrCreateRowAccessibleObject(nextLogicalRow);
					if (accessibleObject != null)
					{
						AccessibleObject child = accessibleObject.GetChild(0);
						if (child != null)
						{
							return child;
						}
					}
					return accessibleObject;
				}
				case LayoutItemType.ExpandBox:
				{
					MordexRow previousLogicalRow = gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow();
					AccessibleObject otherOrCreateRowAccessibleObject = GetOrCreateRowAccessibleObject(previousLogicalRow);
					if (otherOrCreateRowAccessibleObject != null)
					{
						AccessibleObject otherChild = otherOrCreateRowAccessibleObject.GetChild(0);
						if (otherChild != null)
						{
							return otherChild;
						}
					}
					return otherOrCreateRowAccessibleObject;
				}
				case LayoutItemType.GroupHeaderRow:
				{
					MordexRow firstLogicalRow = gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.GetLogicalRow();
					return GetOrCreateRowAccessibleObject(firstLogicalRow);
				}
				case LayoutItemType.RowHeader:
				{
					if (!(gridGroupLayoutState._gridGroupLayoutStateOwner is MxRowLayoutState rowLayoutState))
					{
						break;
					}
					if (rowLayoutState._rowType == LayoutRowType.ColumnHeader)
					{
						if (rowLayoutState._mordexRow == null)
						{
							return GetTableAccessibleObject(GetRowSourceManager().RootRow).GetChild(0);
						}
						MordexRow parentRow = rowLayoutState._mordexRow.ParentRow;
						if (parentRow != null && !rowLayoutState._showTreeLines)
						{
							return GetTableAccessibleObject(parentRow).GetChild(0);
						}
						break;
					}
					MordexRow mordexRow = rowLayoutState.GridRow.GetLogicalRow();
					RowAccessibleObject nextOrCreateRowAccessibleObject = null;
					if (mordexRow != null)
					{
						nextOrCreateRowAccessibleObject = GetOrCreateRowAccessibleObject(mordexRow) as RowAccessibleObject;
					}
					if (nextOrCreateRowAccessibleObject != null)
					{
						return nextOrCreateRowAccessibleObject.FindRowHeader();
					}
					break;
				}
				}
			}
			return null;
		}

		internal AccessibleObject GetOrCreateGridAccessibleObject()
		{
			if (_groupAccessibleObject == null)
			{
				_groupAccessibleObject = new GridAccessibleObject(this);
				RefreshRowCountAndInvalidate();
			}
			return _groupAccessibleObject;
		}

		private void EnsureCellAccessibleObject(GridCell gridCell)
		{
			GetOrCreateRowAccessibleObject(gridCell.Row.GetLogicalRow())?.GetChild(gridCell.Column.Position + 1);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_tableAccessibleObject == null)
			{
				_tableAccessibleObject = new MxDataGridAccessibleObject(this);
			}
			return _tableAccessibleObject;
		}

		internal AccessibleObject GetEditorAccessibleObject()
		{
			return _gridGridColumnContainerControl.AccessibilityObject;
		}

		internal AccessibleObject GetEditCellAccessibleObject()
		{
			if (EditMode == EditMode.EditOn && GetOrCreateRowAccessibleObject(CurrentRow.GetLogicalRow()) is RowAccessibleObject mordexRowAccessibleObject)
			{
				return mordexRowAccessibleObject.ComputeAccessibleObject(CurrentColumn);
			}
			return null;
		}
	}
}
