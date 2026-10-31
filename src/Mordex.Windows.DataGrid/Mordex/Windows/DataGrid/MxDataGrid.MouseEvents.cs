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
		private void UpdateHeaderDragTarget()
		{
			Point mousePosition = Control.MousePosition;
			_pressFeedbackRenderer.Move();
			mousePosition = PointToClient(mousePosition);
			if (_refreshListeners != null)
			{
				foreach (IRefreshListener item in _refreshListeners)
				{
					if (item is MxFieldChooser gridFieldChooserControl && IsPointOverControl(gridFieldChooserControl, Control.MousePosition))
					{
						_pressFeedbackRenderer.SetDropAllowed(true);
						return;
					}
				}
			}
			MxRowLayoutState mordexRowLayoutState = HitTester.FindRowLayoutAtPoint(mousePosition);
			_pressFeedbackRenderer._mordexRowLayoutState = mordexRowLayoutState;
			if (mordexRowLayoutState != null)
			{
				switch (mordexRowLayoutState._rowType)
				{
				case LayoutRowType.ColumnHeader:
				{
					if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.GroupByBoxHeader)
					{
						_pressFeedbackRenderer.SetDropAllowed(_pressFeedbackRenderer._gridGroupLayoutState._column != null && mordexRowLayoutState._owner == _pressFeedbackRenderer._gridGroupLayoutState._column.Table);
					}
					else if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader)
					{
						if (_pressFeedbackRenderer._gridGroupLayoutState._column.Position < GetEffectiveFrozenColumns())
						{
							_pressFeedbackRenderer.SetDropAllowed(!_pressFeedbackRenderer._gridGroupLayoutState._column.Visible);
						}
						else
						{
							_pressFeedbackRenderer.SetDropAllowed(mordexRowLayoutState._owner == _pressFeedbackRenderer._gridGroupLayoutState._column.Table);
						}
					}
					else
					{
						_pressFeedbackRenderer.SetDropAllowed(mordexRowLayoutState._owner == _pressFeedbackRenderer._gridGroupLayoutState._columnSet.Table);
					}
					if (!_pressFeedbackRenderer.IsDropTargetValid())
					{
						break;
					}
					int value = 0;
					if (GetEffectiveFrozenColumns() != 0)
					{
						value = mordexRowLayoutState.GetRightEdgeBeforeColumn(GetEffectiveFrozenColumns()) - 8;
					}
					bool condition = false;
					if (RightToLeft != RightToLeft.Yes)
					{
						condition = mousePosition.X < value + 8;
					}
					else
					{
						value = base.ClientRectangle.Right - value;
						condition = mousePosition.X > value - 8;
					}
					if (condition)
					{
						if (mousePosition.X > 0 && GetHorizontalScrollOffset() > 0)
						{
							_pressFeedbackRenderer.EraseArrows();
							_pressFeedbackRenderer.EraseHeader();
							SetHorizontalScrollOffset(_horizontalScrollOffset - 80);
							Update();
							_pressFeedbackRenderer.Move();
						}
					}
					else
					{
						if ((RightToLeft == RightToLeft.Yes) ? (mousePosition.X < base.ClientRectangle.Left + 8) : (mousePosition.X > base.ClientRectangle.Right - 8))
						{
							int fullWidth = mordexRowLayoutState._owner.CalculateFullWidth();
							if (fullWidth - _horizontalScrollOffset > base.ClientRectangle.Width)
							{
								_pressFeedbackRenderer.EraseArrows();
								_pressFeedbackRenderer.EraseHeader();
								int otherValue = _horizontalScrollOffset + 80;
								if (fullWidth - otherValue < base.ClientRectangle.Width)
								{
									otherValue = fullWidth - base.ClientRectangle.Width;
								}
								SetHorizontalScrollOffset(otherValue);
								Update();
								_pressFeedbackRenderer.Move();
							}
						}
					}
					Rectangle adjustRectangleToViewport = Rectangle.Empty;
					_pressFeedbackRenderer._dropIndex = -1;
					foreach (GridGroupLayoutState arrayListItem in mordexRowLayoutState._arrayList)
					{
						adjustRectangleToViewport = AdjustRectangleToViewport(arrayListItem.GetAbsoluteItemRect(), arrayListItem._isHorizontallyScrolled);
						if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader || _pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.GroupByBoxHeader)
						{
							if (arrayListItem._type != LayoutItemType.ColumnHeader || !adjustRectangleToViewport.Contains(mousePosition))
							{
								continue;
							}
							if (arrayListItem == _pressFeedbackRenderer._gridGroupLayoutState)
							{
								break;
							}
							if (RightToLeft != RightToLeft.Yes)
							{
								if (mousePosition.X - adjustRectangleToViewport.X > adjustRectangleToViewport.Right - mousePosition.X)
								{
									adjustRectangleToViewport = new Rectangle(adjustRectangleToViewport.Right, adjustRectangleToViewport.Top, 0, adjustRectangleToViewport.Height);
									if (_pressFeedbackRenderer._gridGroupLayoutState._column.Table.CellLayoutMode == CellLayoutMode.UseColumns)
									{
										_pressFeedbackRenderer._dropIndex = arrayListItem._column._position + 1;
										if (_pressFeedbackRenderer._dropIndex < GetEffectiveFrozenColumns())
										{
											_pressFeedbackRenderer._dropIndex = -1;
											_pressFeedbackRenderer.SetDropAllowed(false);
										}
									}
									else
									{
										_pressFeedbackRenderer._dropIndex = -1;
										_pressFeedbackRenderer.SetDropAllowed(false);
									}
									break;
								}
								adjustRectangleToViewport = new Rectangle(adjustRectangleToViewport.Left, adjustRectangleToViewport.Top, 0, adjustRectangleToViewport.Height);
								if (_pressFeedbackRenderer._gridGroupLayoutState._column.Table.CellLayoutMode == CellLayoutMode.UseColumns)
								{
									_pressFeedbackRenderer._dropIndex = arrayListItem._column._position;
									if (_pressFeedbackRenderer._dropIndex < GetEffectiveFrozenColumns())
									{
										_pressFeedbackRenderer._dropIndex = -1;
										_pressFeedbackRenderer.SetDropAllowed(false);
									}
									else if (GetEffectiveFrozenColumns() != 0)
									{
										Rectangle empty = Rectangle.Empty;
										empty.X = mordexRowLayoutState.GetRightEdgeBeforeColumn(GetEffectiveFrozenColumns());
										if (AdjustRectangleToViewport(empty, false).X > adjustRectangleToViewport.X)
										{
											_pressFeedbackRenderer._dropIndex = -1;
											_pressFeedbackRenderer.SetDropAllowed(false);
										}
									}
								}
								else
								{
									_pressFeedbackRenderer._dropIndex = -1;
									_pressFeedbackRenderer.SetDropAllowed(false);
								}
								break;
							}
							if (adjustRectangleToViewport.Right - mousePosition.X > mousePosition.X - adjustRectangleToViewport.X)
							{
								adjustRectangleToViewport = new Rectangle(adjustRectangleToViewport.X, adjustRectangleToViewport.Top, 0, adjustRectangleToViewport.Height);
								if (_pressFeedbackRenderer._gridGroupLayoutState._column.Table.CellLayoutMode == CellLayoutMode.UseColumns)
								{
									_pressFeedbackRenderer._dropIndex = arrayListItem._column._position + 1;
									if (_pressFeedbackRenderer._dropIndex < GetEffectiveFrozenColumns())
									{
										_pressFeedbackRenderer._dropIndex = -1;
										_pressFeedbackRenderer.SetDropAllowed(false);
									}
								}
								else
								{
									_pressFeedbackRenderer._dropIndex = -1;
									_pressFeedbackRenderer.SetDropAllowed(false);
								}
								break;
							}
							adjustRectangleToViewport = new Rectangle(adjustRectangleToViewport.Right, adjustRectangleToViewport.Top, 0, adjustRectangleToViewport.Height);
							if (_pressFeedbackRenderer._gridGroupLayoutState._column.Table.CellLayoutMode == CellLayoutMode.UseColumns)
							{
								_pressFeedbackRenderer._dropIndex = arrayListItem._column._position;
								if (_pressFeedbackRenderer._dropIndex < GetEffectiveFrozenColumns())
								{
									_pressFeedbackRenderer._dropIndex = -1;
									_pressFeedbackRenderer.SetDropAllowed(false);
								}
								else if (GetEffectiveFrozenColumns() != 0)
								{
									Rectangle otherRectangle = Rectangle.Empty;
									otherRectangle.X = mordexRowLayoutState.GetRightEdgeBeforeColumn(GetEffectiveFrozenColumns());
									if (AdjustRectangleToViewport(otherRectangle, false).X > adjustRectangleToViewport.X)
									{
										_pressFeedbackRenderer._dropIndex = -1;
										_pressFeedbackRenderer.SetDropAllowed(false);
									}
								}
							}
							else
							{
								_pressFeedbackRenderer._dropIndex = -1;
								_pressFeedbackRenderer.SetDropAllowed(false);
							}
							break;
						}
						if (_pressFeedbackRenderer._gridGroupLayoutState._type != LayoutItemType.ColumnSetHeader || arrayListItem._type != LayoutItemType.ColumnSetHeader || !adjustRectangleToViewport.Contains(mousePosition))
						{
							continue;
						}
						if (arrayListItem != _pressFeedbackRenderer._gridGroupLayoutState)
						{
							if (RightToLeft != RightToLeft.Yes)
							{
								if (mousePosition.X - adjustRectangleToViewport.X > adjustRectangleToViewport.Right - mousePosition.X)
								{
									adjustRectangleToViewport = new Rectangle(adjustRectangleToViewport.Right, adjustRectangleToViewport.Top, 0, adjustRectangleToViewport.Height);
									_pressFeedbackRenderer._dropIndex = arrayListItem._columnSet.Position + 1;
								}
								else
								{
									adjustRectangleToViewport = new Rectangle(adjustRectangleToViewport.Left, adjustRectangleToViewport.Top, 0, adjustRectangleToViewport.Height);
									_pressFeedbackRenderer._dropIndex = arrayListItem._columnSet.Position;
								}
							}
							else if (adjustRectangleToViewport.Right - mousePosition.X > mousePosition.X - adjustRectangleToViewport.X)
							{
								adjustRectangleToViewport = new Rectangle(adjustRectangleToViewport.Left, adjustRectangleToViewport.Top, 0, adjustRectangleToViewport.Height);
								_pressFeedbackRenderer._dropIndex = arrayListItem._columnSet.Position + 1;
							}
							else
							{
								adjustRectangleToViewport = new Rectangle(adjustRectangleToViewport.Right, adjustRectangleToViewport.Top, 0, adjustRectangleToViewport.Height);
								_pressFeedbackRenderer._dropIndex = arrayListItem._columnSet.Position;
							}
						}
						else
						{
							_pressFeedbackRenderer._dropIndex = -1;
						}
						break;
					}
					if (_pressFeedbackRenderer._dropIndex == -1)
					{
						if (_pressFeedbackRenderer._mordexRowLayoutState._owner.CellLayoutMode == CellLayoutMode.UseColumns && _pressFeedbackRenderer._mordexRowLayoutState.LayoutTable.Columns.GetLastVisibleColumn() == null)
						{
							_pressFeedbackRenderer._dropIndex = 0;
							adjustRectangleToViewport = _pressFeedbackRenderer._mordexRowLayoutState.GetContentBounds();
							adjustRectangleToViewport.Height = _pressFeedbackRenderer._gridGroupLayoutState._rect.Height;
						}
						else if (_pressFeedbackRenderer._mordexRowLayoutState._owner.CellLayoutMode == CellLayoutMode.UseColumnSets)
						{
							int nextValue = 0;
							foreach (GridColumnSet columnSet in _pressFeedbackRenderer._mordexRowLayoutState._owner.ColumnSets)
							{
								if (columnSet.Visible)
								{
									nextValue++;
								}
							}
							if (nextValue == 0)
							{
								_pressFeedbackRenderer._dropIndex = 0;
								adjustRectangleToViewport = _pressFeedbackRenderer._mordexRowLayoutState.GetContentBounds();
								adjustRectangleToViewport.Height = _pressFeedbackRenderer._gridGroupLayoutState._rect.Height;
							}
						}
					}
					if (_pressFeedbackRenderer._dropIndex != -1)
					{
						Rectangle otherAdjustRectangleToViewport = AdjustRectangleToViewport(_pressFeedbackRenderer._gridGroupLayoutState.GetAbsoluteItemRect(), _pressFeedbackRenderer._gridGroupLayoutState._isHorizontallyScrolled);
						if ((adjustRectangleToViewport.X != otherAdjustRectangleToViewport.X && adjustRectangleToViewport.X != otherAdjustRectangleToViewport.Right) || adjustRectangleToViewport.Y != otherAdjustRectangleToViewport.Y)
						{
							if (adjustRectangleToViewport.X < 0)
							{
								adjustRectangleToViewport.X = 0;
							}
							if (adjustRectangleToViewport.X > base.ClientRectangle.Right)
							{
								adjustRectangleToViewport.X = base.ClientRectangle.Right;
							}
							adjustRectangleToViewport.Location = PointToScreen(adjustRectangleToViewport.Location);
							_pressFeedbackRenderer.DrawArrows(adjustRectangleToViewport);
						}
						else
						{
							_pressFeedbackRenderer.EraseArrows();
						}
					}
					else
					{
						_pressFeedbackRenderer.EraseArrows();
					}
					break;
				}
				case LayoutRowType.GroupByBox:
				{
					if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader || _pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.GroupByBoxHeader)
					{
						_pressFeedbackRenderer.SetDropAllowed(_pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable().AllowGroup);
						if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader && !_pressFeedbackRenderer._gridGroupLayoutState._column.AllowGroup)
						{
							_pressFeedbackRenderer.SetDropAllowed(false);
						}
						if (_pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable().AnyGroupsDefined() || _pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable().AnyChildGroupsDefined())
						{
							if (mordexRowLayoutState.LayoutTable != _pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable())
							{
								_pressFeedbackRenderer.SetDropAllowed(false);
							}
						}
						else
						{
							GridTable gridTable = _pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable().FindGroupedTable();
							if (gridTable == null)
							{
								gridTable = RootTable;
							}
							if (gridTable != mordexRowLayoutState.LayoutTable)
							{
								_pressFeedbackRenderer.SetDropAllowed(false);
							}
						}
					}
					else
					{
						_pressFeedbackRenderer.SetDropAllowed(false);
					}
					if (mordexRowLayoutState != null && mordexRowLayoutState.LayoutTable != null && _pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader && !mordexRowLayoutState.LayoutTable.CanGroupBy(_pressFeedbackRenderer._gridGroupLayoutState._column))
					{
						GridTable groupedTable = _pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable().FindGroupedTable();
						if (groupedTable == null || groupedTable == mordexRowLayoutState.LayoutTable)
						{
							if (groupedTable == null)
							{
								groupedTable = mordexRowLayoutState._owner;
							}
							if (groupedTable.AllowChildTableGroups == InheritableBoolean.False || (groupedTable.AllowChildTableGroups == InheritableBoolean.Default && !AllowChildTableGroups))
							{
								if (groupedTable != _pressFeedbackRenderer._gridGroupLayoutState._column.Table)
								{
									_pressFeedbackRenderer.SetDropAllowed(_pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable().AllowGroup && _pressFeedbackRenderer._gridGroupLayoutState._column.AllowGroup);
								}
								else
								{
									_pressFeedbackRenderer.SetDropAllowed(false);
								}
							}
							else
							{
								_pressFeedbackRenderer.SetDropAllowed(false);
							}
						}
						else
						{
							_pressFeedbackRenderer.SetDropAllowed(false);
						}
					}
					if (!_pressFeedbackRenderer.IsDropTargetValid())
					{
						break;
					}
					Rectangle nextAdjustRectangleToViewport = Rectangle.Empty;
					_pressFeedbackRenderer._dropIndex = -1;
					foreach (GridGroupLayoutState otherArrayListItem in mordexRowLayoutState._arrayList)
					{
						if (RightToLeft != RightToLeft.Yes)
						{
							nextAdjustRectangleToViewport = AdjustRectangleToViewport(otherArrayListItem.GetAbsoluteItemRect(), otherArrayListItem._isHorizontallyScrolled);
							if (nextAdjustRectangleToViewport.Right <= mousePosition.X || otherArrayListItem._type != LayoutItemType.GroupByBoxHeader)
							{
								continue;
							}
							if (mousePosition.X - nextAdjustRectangleToViewport.X < nextAdjustRectangleToViewport.Width / 2)
							{
								nextAdjustRectangleToViewport = new Rectangle(nextAdjustRectangleToViewport.Left, nextAdjustRectangleToViewport.Top, 0, nextAdjustRectangleToViewport.Height);
								if (otherArrayListItem._type == LayoutItemType.GroupByBoxHeader)
								{
									_pressFeedbackRenderer._dropIndex = otherArrayListItem._gridGroup.Index;
								}
							}
							else
							{
								nextAdjustRectangleToViewport = new Rectangle(nextAdjustRectangleToViewport.Right, nextAdjustRectangleToViewport.Top, 0, nextAdjustRectangleToViewport.Height);
								_pressFeedbackRenderer._dropIndex = otherArrayListItem._gridGroup.Index + 1;
							}
							break;
						}
						nextAdjustRectangleToViewport = AdjustRectangleToViewport(otherArrayListItem.GetAbsoluteItemRect(), otherArrayListItem._isHorizontallyScrolled);
						if (nextAdjustRectangleToViewport.X >= mousePosition.X || otherArrayListItem._type != LayoutItemType.GroupByBoxHeader)
						{
							continue;
						}
						if (mousePosition.X - nextAdjustRectangleToViewport.X < nextAdjustRectangleToViewport.Width / 2)
						{
							nextAdjustRectangleToViewport = new Rectangle(nextAdjustRectangleToViewport.Left, nextAdjustRectangleToViewport.Top, 0, nextAdjustRectangleToViewport.Height);
							if (otherArrayListItem._type == LayoutItemType.GroupByBoxHeader)
							{
								_pressFeedbackRenderer._dropIndex = otherArrayListItem._gridGroup.Index + 1;
							}
						}
						else
						{
							nextAdjustRectangleToViewport = new Rectangle(nextAdjustRectangleToViewport.Right, nextAdjustRectangleToViewport.Top, 0, nextAdjustRectangleToViewport.Height);
							_pressFeedbackRenderer._dropIndex = otherArrayListItem._gridGroup.Index;
						}
						break;
					}
					if (_pressFeedbackRenderer._dropIndex == -1)
					{
						_pressFeedbackRenderer._dropIndex = _pressFeedbackRenderer._mordexRowLayoutState._owner.Groups.Count;
						GridGroup gridGroup = null;
						LayoutItemType layoutItemType = LayoutItemType.GroupByBoxInfo;
						if (_pressFeedbackRenderer._mordexRowLayoutState._owner.Groups.Count > 0)
						{
							gridGroup = _pressFeedbackRenderer._mordexRowLayoutState._owner.Groups[_pressFeedbackRenderer._mordexRowLayoutState._owner.Groups.Count - 1];
							layoutItemType = LayoutItemType.GroupByBoxHeader;
							_pressFeedbackRenderer._dropIndex = gridGroup.Index + 1;
						}
						else
						{
							_pressFeedbackRenderer._dropIndex = 0;
						}
						foreach (GridGroupLayoutState nextArrayListItem in _pressFeedbackRenderer._mordexRowLayoutState._arrayList)
						{
							if (nextArrayListItem._type == layoutItemType && nextArrayListItem._gridGroup == gridGroup)
							{
								nextAdjustRectangleToViewport = AdjustRectangleToViewport(nextArrayListItem.GetAbsoluteItemRect(), nextArrayListItem._isHorizontallyScrolled);
								nextAdjustRectangleToViewport = ((gridGroup != null) ? ((RightToLeft == RightToLeft.Yes) ? new Rectangle(nextAdjustRectangleToViewport.Left, nextAdjustRectangleToViewport.Top, 0, nextAdjustRectangleToViewport.Height) : new Rectangle(nextAdjustRectangleToViewport.Right, nextAdjustRectangleToViewport.Top, 0, nextAdjustRectangleToViewport.Height)) : new Rectangle(nextAdjustRectangleToViewport.Left, nextAdjustRectangleToViewport.Top, 0, nextAdjustRectangleToViewport.Height));
							}
						}
					}
					if (_pressFeedbackRenderer._dropIndex == -1)
					{
						break;
					}
					if (_pressFeedbackRenderer._gridGroupLayoutState._type == LayoutItemType.ColumnHeader)
					{
						if (nextAdjustRectangleToViewport.X < 0)
						{
							nextAdjustRectangleToViewport.X = 0;
						}
						if (nextAdjustRectangleToViewport.X > base.ClientRectangle.Right)
						{
							nextAdjustRectangleToViewport.X = base.ClientRectangle.Right;
						}
						nextAdjustRectangleToViewport.Location = PointToScreen(nextAdjustRectangleToViewport.Location);
						_pressFeedbackRenderer.DrawArrows(nextAdjustRectangleToViewport);
					}
					else
					{
						if (_pressFeedbackRenderer._gridGroupLayoutState._type != LayoutItemType.GroupByBoxHeader)
						{
							break;
						}
						if (_pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable() == _pressFeedbackRenderer._mordexRowLayoutState._owner && _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Index != _pressFeedbackRenderer._dropIndex && (_pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Index + 1 != _pressFeedbackRenderer._dropIndex || _pressFeedbackRenderer._gridGroupLayoutState._gridGroup.Index == -1))
						{
							Rectangle rectangle = AdjustRectangleToViewport(_pressFeedbackRenderer._gridGroupLayoutState.GetAbsoluteItemRect(), _pressFeedbackRenderer._gridGroupLayoutState._isHorizontallyScrolled);
							if (nextAdjustRectangleToViewport.X != rectangle.X && nextAdjustRectangleToViewport.X != rectangle.Right)
							{
								if (nextAdjustRectangleToViewport.X < 0)
								{
									nextAdjustRectangleToViewport.X = 0;
								}
								if (nextAdjustRectangleToViewport.X > base.ClientRectangle.Right)
								{
									nextAdjustRectangleToViewport.X = base.ClientRectangle.Right;
								}
								nextAdjustRectangleToViewport.Location = PointToScreen(nextAdjustRectangleToViewport.Location);
								_pressFeedbackRenderer.DrawArrows(nextAdjustRectangleToViewport);
							}
							else
							{
								_pressFeedbackRenderer.EraseArrows();
							}
						}
						else if (_pressFeedbackRenderer._gridGroupLayoutState.GetOwnerTable() != _pressFeedbackRenderer._mordexRowLayoutState._owner)
						{
							if (nextAdjustRectangleToViewport.X < 0)
							{
								nextAdjustRectangleToViewport.X = 0;
							}
							if (nextAdjustRectangleToViewport.X > base.ClientRectangle.Right)
							{
								nextAdjustRectangleToViewport.X = base.ClientRectangle.Right;
							}
							nextAdjustRectangleToViewport.Location = PointToScreen(nextAdjustRectangleToViewport.Location);
							_pressFeedbackRenderer.DrawArrows(nextAdjustRectangleToViewport);
						}
						else
						{
							_pressFeedbackRenderer.EraseArrows();
						}
					}
					break;
				}
				default:
					_pressFeedbackRenderer.SetDropAllowed(false);
					break;
				}
			}
			else
			{
				_pressFeedbackRenderer.SetDropAllowed(false);
			}
		}

		public GridArea HitTest()
		{
			Point point = PointToClient(Control.MousePosition);
			return HitTest(point.X, point.Y);
		}

		public GridArea HitTest(int x, int y)
		{
			if (!base.ClientRectangle.Contains(x, y))
			{
				return GridArea.None;
			}
			while (!_isLayoutValid)
			{
				CalculateLayoutIfReady();
			}
			GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(x, y);
			if (gridGroupLayoutState == null || gridGroupLayoutState._type == LayoutItemType.EmptyItem)
			{
				if (GetGroupByBoxRectangle().Contains(x, y))
				{
					return GridArea.GroupByBox;
				}
				return GridArea.BackGround;
			}
			if (_view == View.TableView)
			{
				switch (gridGroupLayoutState._type)
				{
				case LayoutItemType.Cell:
				case LayoutItemType.EmptyCell:
				case LayoutItemType.CellDropDownButton:
				case LayoutItemType.CellUpButton:
				case LayoutItemType.CellDownButton:
					try
					{
						if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.NewRecord)
						{
							return GridArea.NewRowCell;
						}
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
					return GridArea.Cell;
				case LayoutItemType.ColumnHeader:
				case LayoutItemType.EmptyHeader:
					return GridArea.ColumnHeader;
				case LayoutItemType.ColumnSetHeader:
					return GridArea.ColumnSetHeader;
				case LayoutItemType.ExpandBox:
				case LayoutItemType.GroupExpandBox:
				case LayoutItemType.CardExpandBox:
				case LayoutItemType.SelfReferencingExpandBox:
					return GridArea.ExpandBox;
				case LayoutItemType.FilterRowCell:
					return GridArea.FilterRow;
				case LayoutItemType.GroupByBoxConnector:
					return GridArea.GroupByBox;
				case LayoutItemType.GroupByBoxInfo:
					return GridArea.GroupByBoxInfoText;
				case LayoutItemType.GroupByBoxHeader:
					return GridArea.GroupByBoxHeader;
				case LayoutItemType.GroupHeaderRow:
					return GridArea.GroupRow;
				case LayoutItemType.GroupIndent:
					return GridArea.GroupIndent;
				case LayoutItemType.GroupTotalCell:
					return GridArea.GroupTotalRow;
				case LayoutItemType.PreviewRow:
					return GridArea.PreviewRow;
				case LayoutItemType.RowHeader:
					return GridArea.RowHeader;
				case LayoutItemType.TotalRowCell:
					return GridArea.TotalRow;
				case LayoutItemType.CellColumnButton:
					return GridArea.CellButton;
				case LayoutItemType.EmptyItem:
					return GridArea.BackGround;
				case LayoutItemType.TableHeader:
					return GridArea.TableHeader;
				case LayoutItemType.RowSelector:
					if (((MxRowLayoutState)gridGroupLayoutState._gridGroupLayoutStateOwner)._rowType == LayoutRowType.GroupHeaderRow)
					{
						return GridArea.GroupRowCheckBox;
					}
					return GridArea.Cell;
				default:
					if (((MxRowLayoutState)gridGroupLayoutState._gridGroupLayoutStateOwner)._rowType == LayoutRowType.GroupByBox)
					{
						return GridArea.GroupByBox;
					}
					return GridArea.BackGround;
				}
			}
			switch (gridGroupLayoutState._type)
			{
			case LayoutItemType.CardCaption:
				return GridArea.CardCaption;
			case LayoutItemType.CardHeader:
				return GridArea.CardColumnHeader;
			case LayoutItemType.CardCell:
			case LayoutItemType.CellDropDownButton:
			case LayoutItemType.CellUpButton:
			case LayoutItemType.CellDownButton:
				try
				{
					if (gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow.RowType == RowType.NewRecord)
					{
						return GridArea.NewRowCell;
					}
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
				return GridArea.Cell;
			case LayoutItemType.CardExpandBox:
				return GridArea.ExpandBox;
			case LayoutItemType.CellColumnButton:
				return GridArea.CellButton;
			default:
				return GridArea.BackGround;
			}
		}

		private void SetHoverLayoutItem(GridGroupLayoutState hoveredLayoutItem)
		{
			try
			{
				if (hoveredLayoutItem == _hoveredLayoutItem)
				{
					return;
				}
				if (_hoveredLayoutItem != null)
				{
					_hoveredLayoutItem._isHot = false;
					InvalidateLayoutItem(_hoveredLayoutItem);
				}
				if (hoveredLayoutItem != null)
				{
					ShowTooltipForLayoutItem(hoveredLayoutItem);
					if (hoveredLayoutItem.UsesVisualStyle(_gridControlStyle.GetLayoutState()))
					{
						_hoveredLayoutItem = hoveredLayoutItem;
						_hoveredLayoutItem._isHot = true;
						InvalidateLayoutItem(_hoveredLayoutItem);
					}
					else
					{
						_hoveredLayoutItem = null;
					}
				}
				else
				{
					ShowTooltipForLayoutItem(null);
					_hoveredLayoutItem = null;
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		internal bool TryHandleMouseMove(MouseEventArgs eventArgs)
		{
			if (!_isLayoutValid)
			{
				base.OnMouseMove(eventArgs);
				return true;
			}
			if (eventArgs.Button != MouseButtons.None)
			{
				if (GetPressedLayoutItem() != null)
				{
					UpdatePressedButtonHover(eventArgs);
				}
				if (_dragSelectStartRow != -1)
				{
					int value = eventArgs.X;
					if (value < base.ClientRectangle.X || value > base.ClientRectangle.Right)
					{
						value = base.ClientRectangle.X + 1;
					}
					AutoScrollSelectRows(value, eventArgs.Y);
				}
				else if (_isCellDragSelecting && !_isDragSelectionSuppressed)
				{
					if (!_dragThresholdBounds.IsEmpty && !_dragThresholdBounds.Contains(eventArgs.X, eventArgs.Y))
					{
						OnRowDrag(new RowDragEventArgs(_mouseButtons));
						_dragThresholdBounds = Rectangle.Empty;
					}
					AutoScrollSelectCells(eventArgs.X, eventArgs.Y);
				}
				else if (!_dragThresholdBounds.IsEmpty && !_dragThresholdBounds.Contains(eventArgs.X, eventArgs.Y))
				{
					OnRowDrag(new RowDragEventArgs(_mouseButtons));
					_dragThresholdBounds = Rectangle.Empty;
				}
			}
			else
			{
				SetPressedLayoutItem(null);
			}
			if (_isButtonPressActive)
			{
				DrawColumnResizeLine();
				return true;
			}
			if (_isCellMousePressed)
			{
				UpdateDataGrid();
				return true;
			}
			if (_isHeaderPressed)
			{
				UpdateHeaderDragTarget();
				return true;
			}
			if (_isDragPending && !_dragStartBounds.Contains(eventArgs.X, eventArgs.Y))
			{
				_isDragPending = false;
				int headerDragState = _headerDragState;
				if (headerDragState == 1)
				{
					StartHeaderDragFromSource();
				}
				_dragStartBounds = Rectangle.Empty;
				_headerDragState = 0;
				return true;
			}
			if (eventArgs.Button == MouseButtons.None)
			{
				GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(eventArgs.X, eventArgs.Y);
				SetHoverLayoutItem(gridGroupLayoutState);
			}
			base.OnMouseMove(eventArgs);
			return false;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			TryHandleMouseMove(e);
		}

		private void UpdatePressedButtonHover(MouseEventArgs eventArgs)
		{
			if (!GetPressedLayoutItem().IsButtonItem() || GetPressedLayoutItem().IsDisabled())
			{
				return;
			}
			if (AdjustRectangleToViewport(GetPressedLayoutItem().GetAbsoluteItemRect(), GetPressedLayoutItem()._isHorizontallyScrolled).Contains(eventArgs.X, eventArgs.Y))
			{
				if (!_gridGridGroupLayoutState._isPressed)
				{
					GetPressedLayoutItem()._isPressed = true;
					if ((_gridGridGroupLayoutState._type == LayoutItemType.CellDownButton || _gridGridGroupLayoutState._type == LayoutItemType.CellUpButton) && _gridColumnContainerControl != null)
					{
						_gridColumnContainerControl.ResumeEditorUpDownRepeater();
					}
					InvalidateLayoutItem(_gridGridGroupLayoutState);
				}
			}
			else if (_gridGridGroupLayoutState._isPressed)
			{
				GetPressedLayoutItem()._isPressed = false;
				if ((_gridGridGroupLayoutState._type == LayoutItemType.CellDownButton || _gridGridGroupLayoutState._type == LayoutItemType.CellUpButton) && _gridColumnContainerControl != null)
				{
					_gridColumnContainerControl.SuspendEditorUpDownRepeater();
				}
				InvalidateLayoutItem(_gridGridGroupLayoutState);
			}
		}

		private bool CanStartRowSelectDrag(int value, int otherValue)
		{
			GridGroupLayoutState gridGroupLayoutState = HitTester.FindLayoutItemAtPoint(value, otherValue);
			switch (gridGroupLayoutState._type)
			{
			case LayoutItemType.RowHeader:
			case LayoutItemType.GroupIndent:
			case LayoutItemType.Connector:
				return true;
			case LayoutItemType.Cell:
				if (!_isEditorDialogOpen && 7 - (value + _horizontalScrollOffset) > 0 && gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow != null)
				{
					return true;
				}
				return false;
			default:
				return false;
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHoverLayoutItem(null);
		}

		private void StartEditClickTimer()
		{
			if (_editDelayTimer != null)
			{
				_editDelayTimer.Tick -= OnEditClickTimerTick;
				_editDelayTimer.Dispose();
				_editDelayTimer = null;
			}
			_editDelayTimer = new System.Windows.Forms.Timer();
			_editDelayTimer.Interval = SystemInformation.DoubleClickTime;
			_editDelayTimer.Tick += OnEditClickTimerTick;
			_editDelayTimer.Enabled = true;
		}

		private void OnEditClickTimerTick(object sender, EventArgs eventArgs)
		{
			_editDelayTimer.Stop();
			_editDelayTimer.Tick -= OnEditClickTimerTick;
			_editDelayTimer.Dispose();
			_editDelayTimer = null;
			PositionCellEditorControl(true);
		}

		private void OnCellEditorDropDownHidden(object value, GridEventArgs gridEventArgs)
		{
			if (value != _gridColumnContainerControl)
			{
				return;
			}
			SetPressedLayoutItem(null);
			if (_row.RowType != RowType.FilterRow)
			{
				OnDropDownHide(new DropDownHideEventArgs(_gridColumnContainerControl.GetColumn(), gridEventArgs.GetValueSelected()));
				return;
			}
			OnDropDownHide(new DropDownHideEventArgs(_gridColumnContainerControl.GetColumn(), gridEventArgs.GetValueSelected()));
			switch (_filterRowUpdateMode)
			{
			case FilterRowUpdateMode.Default:
				if (gridEventArgs.GetValueSelected())
				{
					AcceptCellEdit();
					ApplyFilterRow(false);
					BeginCellEdit(false, '\0', false);
				}
				break;
			case FilterRowUpdateMode.WhenValueChanges:
				ReapplyFilterRowKeepingCaret();
				break;
			case FilterRowUpdateMode.WhenReturnKeyIsPressed:
				break;
			}
		}

		private void OnStateMouseDown(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseDown(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnStateMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (base.IsHandleCreated)
			{
				base.OnMouseUp(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
			}
		}

		private void OnStateMouseMove(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseMove(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnStateMouseClick(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseClick(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnStateMouseDoubleClick(object sender, MouseEventArgs eventArgs)
		{
			base.OnMouseDoubleClick(GridHelper.ProcessMouseInput((Control)sender, this, eventArgs));
		}

		private void OnStateClick(object sender, EventArgs eventArgs)
		{
			base.OnClick(eventArgs);
		}

		private void OnStateDoubleClick(object sender, EventArgs eventArgs)
		{
			Point point = PointToClient(Control.MousePosition);
			GridRow gridRow = null;
			switch (HitTest(point.X, point.Y))
			{
			case GridArea.RowHeader:
			case GridArea.CardCaption:
			case GridArea.CardColumnHeader:
			case GridArea.Cell:
			case GridArea.GroupRow:
			case GridArea.GroupTotalRow:
			case GridArea.TotalRow:
			case GridArea.NewRowCell:
			case GridArea.FilterRow:
			case GridArea.GroupIndent:
			case GridArea.PreviewRow:
			case GridArea.CellButton:
			{
				int rowPositionFromPoint = RowPositionFromPoint(point.X, point.Y);
				if (rowPositionFromPoint != -3)
				{
					gridRow = GetRow(rowPositionFromPoint);
				}
				break;
			}
			}
			base.OnDoubleClick(eventArgs);
			if (gridRow != null)
			{
				OnRowDoubleClick(new RowActionEventArgs(gridRow));
			}
		}

		private void HandleLayoutButtonClick(GridGroupLayoutState gridGroupLayoutState)
		{
			if (gridGroupLayoutState._type == LayoutItemType.CellColumnButton)
			{
				GridRow gridRow = gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow;
				if (gridRow != null)
				{
					OnColumnButtonClick(new ColumnActionEventArgs(gridGroupLayoutState._column));
				}
			}
			else if (gridGroupLayoutState._type == LayoutItemType.FilterClearButton)
			{
				ClearFilterRowColumn(gridGroupLayoutState._column, gridGroupLayoutState._gridGroupLayoutStateOwner.GridRow);
			}
			else if (gridGroupLayoutState._type == LayoutItemType.FilterRowDropDown)
			{
				GridValueListItemCollection gridValueListItemCollection = BuildFilterOperatorList(gridGroupLayoutState._column);
				_gridValueListItemCollectionScrollableBase = new GridValueListItemCollectionScrollableBase();
				_gridValueListItemCollectionScrollableBase.AssignItems(gridValueListItemCollection);
				_gridValueListItemCollectionScrollableBase._maxVisibleRows = gridValueListItemCollection.Count;
				ImageList imageList = new ImageList();
				imageList.ImageSize = new Size(16, 16);
				_gridValueListItemCollectionScrollableBase.SetItemImageList(imageList);
				_gridValueListItemCollectionScrollableBase.SetSelectedItem(gridValueListItemCollection[gridGroupLayoutState._column.FilterRowComparison]);
				Rectangle rectangle = AdjustRectangleToViewport(gridGroupLayoutState.GetAbsoluteItemRect(), gridGroupLayoutState._isHorizontallyScrolled);
				rectangle.Location = PointToScreen(rectangle.Location);
				_gridValueListItemCollectionScrollableBase.ShowDropDownAt(rectangle, this);
				GridHelper.SetCapture(_gridValueListItemCollectionScrollableBase, true);
			}
		}

		void IGridDropListOwner.OnDropListHide(bool cancel)
		{
			if (_gridValueListItemCollectionScrollableBase == null)
			{
				return;
			}
			_gridValueListItemCollectionScrollableBase.HideDropDown(cancel);
			if (!cancel)
			{
				GridValueListItem gridValueListItem = _gridValueListItemCollectionScrollableBase.GetSelectedItem();
				if (gridValueListItem.Value == DBNull.Value)
				{
					ClearFilterRowColumn(_currentColumn, _gridRow);
				}
				else
				{
					EditType editType = GridColumnContainerControl.GetFilterEditType(_currentColumn);
					if (editType == EditType.CheckBox && _gridRow.Cells[_currentColumn].Value == null && _gridColumnContainerControl != null)
					{
						_gridColumnContainerControl.WriteEditorValue(false);
					}
					_currentColumn.FilterRowComparison = (ConditionOperator)gridValueListItem.Value;
					_gridRow.Cells[_currentColumn].DataChanged = true;
					ReapplyFilterRowKeepingCaret();
					if (Row != -2)
					{
						EndCellEdit(false);
						if (_autoEdit)
						{
							BeginCellEdit(false, '\0', false);
						}
					}
				}
			}
			Cursor = Cursors.Default;
			if (_gridValueListItemCollectionScrollableBase != null)
			{
				_gridValueListItemCollectionScrollableBase.Dispose();
				_gridValueListItemCollectionScrollableBase = null;
			}
		}

		internal void SetDropDownActive(bool isDataChangeTrackingEnabled)
		{
			_isDataChangeTrackingEnabled = isDataChangeTrackingEnabled;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected internal override void OnNavigatorButtonClick(int button)
		{
			switch (button)
			{
			case 0:
				MoveFirst();
				break;
			case 1:
				MovePrevious();
				break;
			case 2:
				MoveNext();
				break;
			case 3:
				MoveLast();
				break;
			case 4:
				MoveToNewRecord();
				break;
			}
			FocusEditorOrGrid();
		}

		private void OnCellEditorDropDownShown(object sender, EventArgs eventArgs)
		{
			if (sender == _gridColumnContainerControl)
			{
				EnsureVisible();
				OnDropDown(new ColumnActionEventArgs(_gridColumnContainerControl.GetColumn()));
			}
		}

		private void OnStateMouseWheel(object sender, MouseEventArgs eventArgs)
		{
			OnMouseWheel(eventArgs);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (_gridColumnContainerControl != null && (_gridColumnContainerControl.EditorHandlesMouseWheel() || _gridColumnContainerControl.ForwardMouseWheel(e.Delta)))
			{
				return;
			}
			if (_view == View.CardView)
			{
				if (IsHorizontalScrollRequired())
				{
					if (e.Delta < 0)
					{
						ScrollBarControl horizontalScrollBar = GetHorizontalScrollBar();
						horizontalScrollBar.SetScrollValue(horizontalScrollBar.GetScrollValue() + GetHorizontalScrollBar().GetScrollStep());
					}
					else if (e.Delta > 0)
					{
						ScrollBarControl controlStyleControl2Argument = GetHorizontalScrollBar();
						controlStyleControl2Argument.SetScrollValue(controlStyleControl2Argument.GetScrollValue() - GetHorizontalScrollBar().GetScrollStep());
					}
				}
				base.OnMouseWheel(e);
			}
			else if (_view == View.SingleCard)
			{
				if (IsVerticalScrollRequired())
				{
					double amount = Math.Abs((double)e.Delta / SystemInformation.MouseWheelScrollDelta);
					if (amount < 1.0)
					{
						amount = 1.0;
					}
					int controlStyleControl2Integer = GetVScroll().GetScrollStep();
					controlStyleControl2Integer = ((SystemInformation.MouseWheelScrollLines != -1) ? (controlStyleControl2Integer * SystemInformation.MouseWheelScrollLines) : GetVScroll().GetPageSize());
					controlStyleControl2Integer = (int)((double)controlStyleControl2Integer * amount);
					if (e.Delta < 0)
					{
						ScrollBarControl controlStyleControl2Argument2 = GetVScroll();
						controlStyleControl2Argument2.SetScrollValue(controlStyleControl2Argument2.GetScrollValue() + controlStyleControl2Integer);
					}
					else
					{
						ScrollBarControl controlStyleControl2Argument3 = GetVScroll();
						controlStyleControl2Argument3.SetScrollValue(controlStyleControl2Argument3.GetScrollValue() - controlStyleControl2Integer);
					}
					Update();
				}
				base.OnMouseWheel(e);
			}
			else
			{
				base.OnMouseWheel(e);
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			Point point = PointToClient(Control.MousePosition);
			_isDragPending = false;
			if (_mouseButtons == MouseButtons.Left)
			{
				Point pointToClient = PointToClient(Control.MousePosition);
				GridGroupLayoutState gridGroupLayoutState = HitTester.FindResizableHeaderAtPoint(pointToClient.X, pointToClient.Y);
				if (gridGroupLayoutState != null && gridGroupLayoutState._column != null)
				{
					AutoSizeColumnToContent(gridGroupLayoutState._column, true);
					return;
				}
				if (_row != null)
				{
					switch (HitTest(pointToClient.X, pointToClient.Y))
					{
					case GridArea.GroupRow:
						ToggleRowExpanded(_row.GetLogicalRow());
						break;
					case GridArea.CardCaption:
						if (_expandableCards)
						{
							ToggleCardExpanded(_row.GetLogicalRow());
						}
						break;
					}
				}
			}
			GridRow gridRow = null;
			switch (HitTest(point.X, point.Y))
			{
			case GridArea.RowHeader:
			case GridArea.CardCaption:
			case GridArea.CardColumnHeader:
			case GridArea.Cell:
			case GridArea.GroupRow:
			case GridArea.GroupTotalRow:
			case GridArea.TotalRow:
			case GridArea.NewRowCell:
			case GridArea.FilterRow:
			case GridArea.GroupIndent:
			case GridArea.PreviewRow:
			case GridArea.CellButton:
			{
				int rowPositionFromPoint = RowPositionFromPoint(point.X, point.Y);
				if (rowPositionFromPoint != -3)
				{
					gridRow = GetRow(rowPositionFromPoint);
				}
				break;
			}
			}
			base.OnDoubleClick(e);
			if (gridRow != null)
			{
				OnRowDoubleClick(new RowActionEventArgs(gridRow));
			}
		}

		internal void ClearOrphanRootTableDropDowns()
		{
			if (!_isInitializing && RootTable != null)
			{
				RootTable.ClearOrphanDropDowns();
			}
		}

		protected virtual void OnRowDoubleClick(RowActionEventArgs e)
		{
			if (base.Events[_rowDoubleClickEventKey] is RowActionEventHandler rowActionEventHandler)
			{
				rowActionEventHandler(this, e);
			}
		}

		protected virtual void OnRowDrag(RowDragEventArgs e)
		{
			if (base.Events[_rowDragEventKey] is RowDragEventHandler rowDragEventHandler)
			{
				rowDragEventHandler(this, e);
			}
		}

		protected virtual void OnDraggingColumn(ColumnActionCancelEventArgs e)
		{
			if (base.Events[_eventKeyDraggingColumn] is ColumnActionCancelEventHandler columnActionCancelEventHandler)
			{
				columnActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnDraggingGroup(GroupActionCancelEventArgs e)
		{
			if (base.Events[_eventKeyDraggingGroup] is GroupActionCancelEventHandler groupActionCancelEventHandler)
			{
				groupActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnDraggingColumnSet(ColumnSetActionCancelEventArgs e)
		{
			if (base.Events[_eventKeyDraggingColumnSet] is ColumnSetActionCancelEventHandler columnSetActionCancelEventHandler)
			{
				columnSetActionCancelEventHandler(this, e);
			}
		}

		protected virtual void OnColumnButtonClick(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyColumnButtonClick] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnClearFilterButtonClick(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyClearFilterButtonClick] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnColumnHeaderClick(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyColumnHeaderClick] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnGroupByBoxHeaderClick(GroupActionEventArgs e)
		{
			if (base.Events[_eventKeyGroupByBoxHeaderClick] is GroupActionEventHandler groupActionEventHandler)
			{
				groupActionEventHandler(this, e);
			}
		}

		protected virtual void OnDropDown(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyDropDown] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		protected virtual void OnDropDownHide(DropDownHideEventArgs e)
		{
			if (base.Events[_eventKeyDropDownHide] is DropDownHideEventHandler dropDownHideEventHandler)
			{
				dropDownHideEventHandler(this, e);
			}
		}

		protected virtual void OnLinkClicked(ColumnActionEventArgs e)
		{
			if (base.Events[_eventKeyLinkClicked] is ColumnActionEventHandler columnActionEventHandler)
			{
				columnActionEventHandler(this, e);
			}
		}

		internal Cursor GetCursorAtPointer()
		{
			Cursor result = null;
			Point point = PointToClient(PointerPosition.Current);
			if (_view == View.TableView)
			{
				if (CanStartRowSelectDrag(point.X, point.Y))
				{
					result = GridHelper.ComputeCursor();
				}
				GridGroupLayoutState gridGroupLayoutState = HitTester.FindResizableHeaderAtPoint(point.X, point.Y);
				if (gridGroupLayoutState != null)
				{
					result = Cursors.VSplit;
				}
				else
				{
					GridGroupLayoutState linkCellAtPoint = HitTester.FindLinkCellAtPoint(point.X, point.Y);
					if (linkCellAtPoint != null && (linkCellAtPoint.GetDisplayText().Length > 0 || linkCellAtPoint.ResolveImage(ComputeImageList()) != null))
					{
						result = Cursors.Hand;
					}
				}
			}
			else
			{
				int cardResizeIndexAtPoint = HitTester.FindCardResizeIndexAtPoint(point.X, point.Y);
				if (cardResizeIndexAtPoint != -1)
				{
					result = Cursors.SizeWE;
				}
				else
				{
					GridGroupLayoutState otherLinkCellAtPoint = HitTester.FindLinkCellAtPoint(point.X, point.Y);
					if (otherLinkCellAtPoint != null && (otherLinkCellAtPoint.GetDisplayText().Length > 0 || otherLinkCellAtPoint.GetDisplayImage(ComputeImageList()) != null))
					{
						result = Cursors.Hand;
					}
				}
			}
			return result;
		}

		void IDragHost.StartDrag(GridGroupLayoutState item, int offsetX, int offsetY)
		{
			StartLayoutItemDrag(item, offsetX, offsetY);
		}
	}
}
