using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Mordex.Data;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Export
{
	internal class ExcelXmlBuilder
	{
		private float _dpi;

		private Hashtable _tableLevels;

		public ExcelXmlBuilder()
		{
		}

		private float GetScreenDpi(Control control)
		{
			Graphics graphics = control.CreateGraphics();
			float dpiX = graphics.DpiX;
			graphics.Dispose();
			return dpiX;
		}

		private float PixelsToPoints(int value)
		{
			return (float)value / _dpi * 72f;
		}

		private ExportRow[] BuildHeaderRows(ExportWorksheet worksheet, MxDataGrid dataGrid, GridTable[] gridTableArray)
		{
			int value = 1;
			int otherValue = 0;
			int nextValue = 0;
			bool condition = false;
			bool otherCondition = false;
			foreach (GridTable gridTable in gridTableArray)
			{
				nextValue = 0;
				int previousValue = 0;
				condition |= gridTable.IsTableHeaderVisible();
				if (condition)
				{
					previousValue = 1;
					otherValue = 1;
					nextValue = 1;
				}
				if (gridTable.CellLayoutMode == CellLayoutMode.UseColumnSets)
				{
					otherCondition |= gridTable.AreColumnSetHeadersVisible();
					if (otherCondition)
					{
						previousValue++;
						nextValue++;
					}
					if (gridTable.AreColumnHeadersVisible())
					{
						previousValue += gridTable.ColumnSetRowCount;
					}
				}
				else if (gridTable.AreColumnHeadersVisible())
				{
					previousValue++;
				}
				if (previousValue > value)
				{
					value = previousValue;
				}
			}
			ExportRow[] array = new ExportRow[value];
			for (int j = 0; j < value; j++)
			{
				array[j] = new ExportRow();
			}
			int firstValue = 1;
			foreach (GridTable gridTableArrayItem in gridTableArray)
			{
				_tableLevels.Add(gridTableArrayItem, firstValue);
				GridFormatStyle gridFormatStyle = dataGrid.GetHeaderStyle(gridTableArrayItem);
				ExportCell padCell;
				for (int i = 0; i < gridTableArrayItem.Groups.Count; i++)
				{
					worksheet.GetColumns().Add(new ExportColumn(PixelsToPoints(dataGrid.GroupIndent)));
					for (int m = 0; m < array.Length; m++)
					{
						padCell = CreateCell(null, "");
						padCell.GetAppearance().ApplyFormatStyle(gridFormatStyle, dataGrid);
						padCell.GetAppearance().ProcessTextAlignment(gridFormatStyle.TextAlignment);
						array[m].GetCells().Add(padCell);
					}
					firstValue++;
				}
				if (gridTableArrayItem.CellLayoutMode == CellLayoutMode.UseColumns)
				{
					for (int n = 0; n < gridTableArrayItem.Columns.Count; n++)
					{
						GridColumn columnInPosition = gridTableArrayItem.Columns.GetColumnInPosition(n);
						if (columnInPosition != null && columnInPosition.Visible)
						{
							worksheet.GetColumns().Add(new ExportColumn(PixelsToPoints(columnInPosition.Width)));
							GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(columnInPosition.HeaderStyle);
							padCell = CreateCell(columnInPosition.Caption, columnInPosition.Caption);
							padCell.GetAppearance().ApplyFormatStyle(applyOverrides, dataGrid);
							padCell.GetAppearance().ProcessTextAlignment(applyOverrides.TextAlignment);
							array[array.Length - 1].GetCells().Add(padCell);
							firstValue++;
						}
					}
				}
				else
				{
					for (int position = 0; position < gridTableArrayItem.ColumnSets.Count; position++)
					{
						GridColumnSet columnSetInPosition = gridTableArrayItem.ColumnSets.GetColumnSetInPosition(position);
						if (!columnSetInPosition.Visible)
						{
							continue;
						}
						int secondValue = firstValue;
						for (int column = 0; column < columnSetInPosition.ColumnCount; column++)
						{
							worksheet.GetColumns().Add(new ExportColumn(PixelsToPoints(columnSetInPosition.GetColumnWidth(column))));
							for (int k = 0; k < gridTableArrayItem.ColumnSetRowCount; k++)
							{
								if (gridTableArrayItem.AreColumnHeadersVisible())
								{
									GridColumn gridColumn = columnSetInPosition[k, column];
									if (gridColumn != null && gridColumn.Visible && gridColumn.ColumnSetRow == k && gridColumn.ColumnSetColumn == column)
									{
										GridFormatStyle otherApplyOverrides = gridFormatStyle.ApplyOverrides(gridColumn.HeaderStyle);
										padCell = CreateCell(gridColumn.Caption, gridColumn.Caption);
										padCell.GetAppearance().ApplyFormatStyle(otherApplyOverrides, dataGrid);
										padCell.GetAppearance().ProcessTextAlignment(otherApplyOverrides.TextAlignment);
										padCell.SetMergeAcross(gridColumn.ColSpan - 1);
										array[nextValue + k].GetCells().Add(padCell);
									}
									else if (gridColumn == null || !gridColumn.Visible || gridColumn.ColumnSetRow != k)
									{
										padCell = CreateCell("", "");
										padCell.GetAppearance().ApplyFormatStyle(gridFormatStyle, dataGrid);
										array[nextValue + k].GetCells().Add(padCell);
									}
								}
							}
							firstValue++;
						}
						if (gridTableArrayItem.AreColumnSetHeadersVisible())
						{
							GridFormatStyle nextApplyOverrides = gridFormatStyle.ApplyOverrides(columnSetInPosition.HeaderStyle);
							padCell = CreateCell(columnSetInPosition.Caption, columnSetInPosition.Caption);
							padCell.GetAppearance().ApplyFormatStyle(nextApplyOverrides, dataGrid);
							padCell.GetAppearance().ProcessTextAlignment(nextApplyOverrides.TextAlignment);
							padCell.SetMergeAcross(firstValue - secondValue - 1);
							array[otherValue].GetCells().Add(padCell);
						}
					}
				}
				if (!gridTableArrayItem.IsTableHeaderVisible())
				{
					continue;
				}
				int countVisibleColumns = gridTableArrayItem.Columns.CountVisibleColumns();
				if (gridTableArrayItem.CellLayoutMode == CellLayoutMode.UseColumnSets)
				{
					countVisibleColumns = 0;
					foreach (GridColumnSet columnSet in gridTableArrayItem.ColumnSets)
					{
						if (columnSet.Visible)
						{
							countVisibleColumns += columnSet.ColumnCount;
						}
					}
				}
				padCell = CreateCell(gridTableArrayItem.Caption, gridTableArrayItem.Caption);
				padCell.GetAppearance().ApplyFormatStyle(gridFormatStyle, dataGrid);
				padCell.SetMergeAcross(countVisibleColumns - 1);
				array[0].GetCells().Add(padCell);
			}
			return array;
		}

		private void BuildRows(ExportWorksheet worksheet, GridExporter gridExporter, GridRow[] gridRowArray, GridTable[] gridTableArray)
		{
			MxDataGrid dataGrid = gridExporter.DataGrid;
			ExportRow[] array = BuildHeaderRows(worksheet, dataGrid, gridTableArray);
			if (gridExporter.IncludeHeaders)
			{
				ExportRow[] otherArray = array;
				foreach (ExportRow item in otherArray)
				{
					worksheet.AddRow(item);
				}
			}
			for (int j = 0; j < gridRowArray.Length; j++)
			{
				GridRow gridRow = gridRowArray[j];
				AddRowTree(worksheet, gridExporter, gridRow);
				gridExporter.ReportRowProgress(j);
			}
			if ((gridExporter.DataGrid.RootTable.TotalRow != InheritableBoolean.False || gridExporter.DataGrid.TotalRow == InheritableBoolean.True) && gridExporter.ExportMode == ExportMode.AllRows)
			{
				AddRowTree(worksheet, gridExporter, gridExporter.DataGrid.GetTotalRow());
			}
		}

		private void AddRowTree(ExportWorksheet worksheet, GridExporter gridExporter, GridRow otherGridRow)
		{
			if (otherGridRow == null)
			{
				return;
			}
			bool selected = true;
			switch (gridExporter.ExportMode)
			{
			case ExportMode.CheckedRows:
				selected = otherGridRow.CheckState != RowCheckState.Unchecked;
				break;
			case ExportMode.SelectedRows:
				selected = otherGridRow.Selected;
				break;
			}
			if (selected)
			{
				switch (otherGridRow.RowType)
				{
				case RowType.Record:
					AddRecordRow(worksheet, gridExporter.DataGrid, otherGridRow);
					break;
				case RowType.GroupHeader:
					AddGroupHeaderRow(worksheet, gridExporter.DataGrid, otherGridRow);
					break;
				case RowType.GroupFooter:
					AddGroupFooterRow(worksheet, gridExporter.DataGrid, otherGridRow);
					break;
				case RowType.TotalRow:
					AddTotalRow(worksheet, gridExporter.DataGrid, otherGridRow);
					break;
				}
			}
			if (otherGridRow.RowType == RowType.GroupHeader)
			{
				if (otherGridRow.Expanded || gridExporter.IncludeCollapsedRows)
				{
					GridRow[] childRows = otherGridRow.GetChildRows();
					foreach (GridRow gridRow in childRows)
					{
						AddRowTree(worksheet, gridExporter, gridRow);
					}
				}
				else if (!otherGridRow.Expanded && otherGridRow.Table.ComputeGroupTotals() == GroupTotals.Always)
				{
					GridRow gridRowForLogicalRow = gridExporter.DataGrid.GetGridRowForLogicalRow(((MordexGroupRow)otherGridRow.GetLogicalRow()).TotalRow);
					AddRowTree(worksheet, gridExporter, gridRowForLogicalRow);
				}
			}
			else
			{
				if (otherGridRow.RowType != RowType.Record)
				{
					return;
				}
				bool baseIsSelfReferencingTable = otherGridRow.Table.ChildTables.Count > 0 && gridExporter.IncludeChildTables;
				if (!baseIsSelfReferencingTable)
				{
					baseIsSelfReferencingTable = otherGridRow.Table.GetBaseIsSelfReferencingTable();
				}
				if (baseIsSelfReferencingTable && (otherGridRow.Expanded || gridExporter.IncludeCollapsedRows))
				{
					GridRow[] otherChildRows = otherGridRow.GetChildRows();
					foreach (GridRow nextGridRow in otherChildRows)
					{
						AddRowTree(worksheet, gridExporter, nextGridRow);
					}
				}
			}
		}

		private ExportCell CreateCell(object value, string text)
		{
			ExportCell cell = new ExportCell();
			cell.SetCellValue(value);
			cell.SetCellText(text);
			return cell;
		}

		private void AddGroupHeaderRow(ExportWorksheet worksheet, MxDataGrid dataGrid, GridRow gridRow)
		{
			ExportRow exportRow = new ExportRow();
			exportRow._rowKind = (int)_tableLevels[gridRow.Table];
			GridFormatStyle gridFormatStyle = dataGrid.GetGroupRowStyle(gridRow.Table);
			GridFormatStyle otherGridFormatStyle;
			for (int i = 0; i < gridRow.Group.Index; i++)
			{
				otherGridFormatStyle = ((gridRow.Table.Groups[i].Column == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(gridRow.Table.Groups[i].Column.GroupRowStyle));
				ExportCell padCell = CreateCell(null, "");
				padCell.GetAppearance().ApplyFormatStyle(otherGridFormatStyle, dataGrid);
				exportRow.GetCells().Add(padCell);
			}
			ExportCell cell = CreateCell(null, gridRow.Group.GroupPrefix + " " + gridRow.GroupCaption);
			cell.SetIsTextOnly(true);
			otherGridFormatStyle = ((gridRow.Group.Column == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(gridRow.Group.Column.GroupRowStyle));
			cell.GetAppearance().ApplyFormatStyle(otherGridFormatStyle, dataGrid);
			bool condition = false;
			foreach (GridCell item in (IEnumerable)gridRow.Cells)
			{
				if (item.Text.Length > 0)
				{
					condition = true;
					break;
				}
			}
			if (condition)
			{
				cell.SetMergeAcross(gridRow.Table.Groups.Count - gridRow.Group.Index - 1);
				exportRow.GetCells().Add(cell);
				bool otherCondition = false;
				for (int j = 0; j < gridRow.Table.Columns.Count; j++)
				{
					GridColumn columnInPosition = gridRow.Table.Columns.GetColumnInPosition(j);
					if (columnInPosition == null || !columnInPosition.Visible)
					{
						continue;
					}
					GridCell columnInPositionCell = gridRow.Cells[columnInPosition];
					if (columnInPositionCell.Text.Length > 0 || otherCondition)
					{
						otherCondition = true;
						ExportCell otherCell = CreateCell(columnInPositionCell.Value, columnInPositionCell.Text);
						otherCell.GetAppearance().ApplyFormatStyle(otherGridFormatStyle, dataGrid);
						if (columnInPositionCell._groupHeaderTotal is GridGroupHeaderTotal gridGroupHeaderTotal && gridGroupHeaderTotal.TotalFormatMode == FormatMode.UseIFormattable)
						{
							otherCell.SetIsTextOnly(false);
							otherCell.SetNumberFormat(gridGroupHeaderTotal.TotalFormatString);
							otherCell.SetTypeCode(TypeCode.Double);
						}
						else
						{
							otherCell.SetIsTextOnly(true);
						}
						exportRow.GetCells().Add(otherCell);
					}
					else if (!otherCondition)
					{
						cell.SetMergeAcross(cell.GetMergeAcross() + 1);
					}
				}
			}
			else
			{
				int countVisibleColumns = gridRow.Table.Columns.CountVisibleColumns();
				if (gridRow.Table.CellLayoutMode == CellLayoutMode.UseColumnSets)
				{
					countVisibleColumns = 0;
					foreach (GridColumnSet columnSet in gridRow.Table.ColumnSets)
					{
						if (columnSet.Visible)
						{
							countVisibleColumns += columnSet.ColumnCount;
						}
					}
				}
				cell.SetMergeAcross(countVisibleColumns + (gridRow.Table.Groups.Count - gridRow.Group.Index) - 1);
				exportRow.GetCells().Add(cell);
			}
			worksheet.AddRow(exportRow);
		}

		private void AddGroupFooterRow(ExportWorksheet worksheet, MxDataGrid dataGrid, GridRow gridRow)
		{
			int value = 1;
			value = ((gridRow.Table.CellLayoutMode == CellLayoutMode.UseColumns) ? 1 : gridRow.Table.ColumnSetRowCount);
			ExportRow[] array = new ExportRow[value];
			for (int i = 0; i < value; i++)
			{
				array[i] = new ExportRow();
				array[i]._rowKind = (int)_tableLevels[gridRow.Table];
			}
			GridFormatStyle gridFormatStyle = dataGrid.GetGroupRowStyle(gridRow.Table);
			GridFormatStyle otherGridFormatStyle;
			for (int j = 0; j < gridRow.Table.Groups.Count; j++)
			{
				GridColumn gridColumn = ((j <= gridRow.Group.Index) ? gridRow.Table.Groups[j].Column : gridRow.Group.Column);
				otherGridFormatStyle = ((gridColumn == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(gridColumn.GroupRowStyle));
				for (int k = 0; k < value; k++)
				{
					ExportCell padCell = CreateCell(null, "");
					padCell.GetAppearance().ApplyFormatStyle(otherGridFormatStyle, dataGrid);
					array[k].GetCells().Add(padCell);
				}
			}
			otherGridFormatStyle = ((gridRow.Group.Column == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(gridRow.Group.Column.GroupRowStyle));
			if (gridRow.Table.CellLayoutMode == CellLayoutMode.UseColumns)
			{
				for (int position = 0; position < gridRow.Table.Columns.Count; position++)
				{
					GridColumn columnInPosition = gridRow.Table.Columns.GetColumnInPosition(position);
					if (columnInPosition == null || !columnInPosition.Visible)
					{
						continue;
					}
					GridCell columnInPositionCell = gridRow.Cells[columnInPosition];
					GridFormatStyle applyOverrides = otherGridFormatStyle.ApplyOverrides(columnInPositionCell.FormatStyle);
					ExportCell cell = CreateCell(columnInPositionCell.Value, columnInPositionCell.Text);
					switch (columnInPosition.AggregateFunction)
					{
					case AggregateFunction.Count:
					case AggregateFunction.Sum:
					case AggregateFunction.Average:
					case AggregateFunction.StdDeviation:
					case AggregateFunction.ValueCount:
						cell.SetTypeCode(TypeCode.Double);
						break;
					default:
						cell.SetTypeCode(columnInPosition.DataTypeCode);
						cell.SetExportValueKind(columnInPosition.ExportValueKind);
						break;
					}
					cell.GetAppearance().ApplyFormatStyle(applyOverrides, dataGrid);
					if (columnInPosition.TotalFormatString.Length > 0)
					{
						if (columnInPosition.TotalFormatMode == FormatMode.UseIFormattable)
						{
							string text = columnInPosition.GetDisplayText(columnInPositionCell.Value, true);
							if (text == columnInPositionCell.Text)
							{
								cell.SetNumberFormat(columnInPosition.TotalFormatString);
							}
							else
							{
								cell.SetIsTextOnly(true);
							}
						}
						else
						{
							cell.SetIsTextOnly(true);
						}
					}
					if (cell.GetCellValue() == null || cell.GetCellValue() == DBNull.Value)
					{
						cell.SetIsTextOnly(true);
					}
					array[0].GetCells().Add(cell);
				}
			}
			else
			{
				for (int m = 0; m < gridRow.Table.ColumnSets.Count; m++)
				{
					GridColumnSet columnSetInPosition = gridRow.Table.ColumnSets.GetColumnSetInPosition(m);
					if (!columnSetInPosition.Visible)
					{
						continue;
					}
					for (int n = 0; n < columnSetInPosition.ColumnCount; n++)
					{
						for (int p = 0; p < gridRow.Table.ColumnSetRowCount; p++)
						{
							GridColumn columnSetInPositionItem = columnSetInPosition[p, n];
							if (columnSetInPositionItem != null && columnSetInPositionItem.Visible && columnSetInPositionItem._columnSetRow == p && columnSetInPositionItem.ColumnSetColumn == n)
							{
								GridFormatStyle otherApplyOverrides = gridFormatStyle.ApplyOverrides(columnSetInPositionItem.CellStyle);
								GridCell gridColumn2Cell = gridRow.Cells[columnSetInPositionItem];
								GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(gridColumn2Cell.FormatStyle);
								ExportCell otherCell = CreateCell(gridColumn2Cell.Value, gridColumn2Cell.Text);
								switch (columnSetInPositionItem.AggregateFunction)
								{
								case AggregateFunction.Count:
								case AggregateFunction.Sum:
								case AggregateFunction.Average:
								case AggregateFunction.StdDeviation:
								case AggregateFunction.ValueCount:
									otherCell.SetTypeCode(TypeCode.Double);
									break;
								default:
									otherCell.SetTypeCode(columnSetInPositionItem.DataTypeCode);
									otherCell.SetExportValueKind(columnSetInPositionItem.ExportValueKind);
									break;
								}
								otherCell.GetAppearance().ApplyFormatStyle(nextApplyOverrides, dataGrid);
								if (columnSetInPositionItem.TotalFormatString.Length > 0)
								{
									if (columnSetInPositionItem.TotalFormatMode == FormatMode.UseIFormattable)
									{
										string displayText = columnSetInPositionItem.GetDisplayText(gridColumn2Cell.Value, true);
										if (displayText == gridColumn2Cell.Text)
										{
											otherCell.SetNumberFormat(columnSetInPositionItem.TotalFormatString);
										}
										else
										{
											otherCell.SetIsTextOnly(true);
										}
									}
									else
									{
										otherCell.SetIsTextOnly(true);
									}
								}
								if (otherCell.GetCellValue() == null || otherCell.GetCellValue() == DBNull.Value)
								{
									otherCell.SetIsTextOnly(true);
								}
								otherCell.SetMergeAcross(columnSetInPositionItem.ColSpan - 1);
								array[p].GetCells().Add(otherCell);
							}
							else if (columnSetInPositionItem == null || columnSetInPositionItem.ColumnSetRow != p)
							{
								ExportCell nextCell = CreateCell("", "");
								nextCell.GetAppearance().ApplyFormatStyle(gridFormatStyle, dataGrid);
								array[p].GetCells().Add(nextCell);
							}
						}
					}
				}
			}
			for (int q = 0; q < array.Length; q++)
			{
				worksheet.AddRow(array[q]);
			}
		}

		private void AddRecordRow(ExportWorksheet worksheet, MxDataGrid dataGrid, GridRow gridRow)
		{
			int value = 1;
			value = ((gridRow.Table.CellLayoutMode == CellLayoutMode.UseColumns) ? 1 : gridRow.Table.ColumnSetRowCount);
			ExportRow[] array = new ExportRow[value];
			for (int i = 0; i < value; i++)
			{
				array[i] = new ExportRow();
				array[i]._rowKind = (int)_tableLevels[gridRow.Table];
			}
			GridFormatStyle gridFormatStyle;
			if (gridRow.Table.Groups.Count > 0)
			{
				gridFormatStyle = dataGrid.GetGroupRowStyle(gridRow.Table);
				for (int j = 0; j < gridRow.Table.Groups.Count; j++)
				{
					GridFormatStyle otherGridFormatStyle = ((gridRow.Table.Groups[j].Column == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(gridRow.Table.Groups[j].Column.GroupRowStyle));
					for (int k = 0; k < value; k++)
					{
						ExportCell padCell = CreateCell(null, "");
						padCell.GetAppearance().ApplyFormatStyle(otherGridFormatStyle, dataGrid);
						array[k].GetCells().Add(padCell);
					}
				}
			}
			gridFormatStyle = dataGrid.GetAlternatingRowStyle(gridRow.Table, gridRow.GetPosition());
			GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(gridRow.RowStyle);
			applyOverrides = applyOverrides.ApplyOverrides(gridRow.GetCachedFormatStyle());
			if (gridRow.Table.CellLayoutMode == CellLayoutMode.UseColumns)
			{
				for (int position = 0; position < gridRow.Table.Columns.Count; position++)
				{
					GridColumn columnInPosition = gridRow.Table.Columns.GetColumnInPosition(position);
					if (columnInPosition == null || !columnInPosition.Visible)
					{
						continue;
					}
					GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(columnInPosition.CellStyle);
					GridCell columnInPositionCell = gridRow.Cells[columnInPosition];
					GridFormatStyle nextApplyOverrides = otherApplyOverrides.ApplyOverrides(columnInPositionCell.GetEffectiveFormatStyle());
					GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(columnInPositionCell.FormatStyle);
					ExportCell cell = CreateCell(columnInPositionCell.Value, columnInPositionCell.Text);
					if (columnInPosition.Type.IsEnum)
					{
						cell.SetIsTextOnly(true);
					}
					cell.SetTypeCode(columnInPosition.DataTypeCode);
					cell.SetExportValueKind(columnInPosition.ExportValueKind);
					cell.GetAppearance().ApplyFormatStyle(previousApplyOverrides, dataGrid);
					if (columnInPosition.FormatString.Length > 0)
					{
						if (columnInPosition.FormatMode == FormatMode.UseIFormattable)
						{
							string text = columnInPosition.GetDisplayText(columnInPositionCell.Value, true);
							if (text == columnInPositionCell.Text)
							{
								cell.SetNumberFormat(columnInPosition.FormatString);
							}
							else
							{
								cell.SetIsTextOnly(true);
							}
						}
						else
						{
							cell.SetIsTextOnly(true);
						}
					}
					else
					{
						string displayText = columnInPosition.GetDisplayText(columnInPositionCell.Value, true);
						if (displayText != columnInPositionCell.Text)
						{
							cell.SetIsTextOnly(true);
						}
					}
					if (columnInPosition.HasValueList && columnInPosition.ValueList.ReplaceValues)
					{
						cell.SetIsTextOnly(true);
					}
					else if (columnInPosition.DropDown != null && columnInPosition.DropDown.ReplaceValues)
					{
						cell.SetIsTextOnly(true);
					}
					array[0].GetCells().Add(cell);
				}
			}
			else
			{
				for (int m = 0; m < gridRow.Table.ColumnSets.Count; m++)
				{
					GridColumnSet columnSetInPosition = gridRow.Table.ColumnSets.GetColumnSetInPosition(m);
					if (!columnSetInPosition.Visible)
					{
						continue;
					}
					for (int n = 0; n < gridRow.Table.ColumnSetRowCount; n++)
					{
						for (int p = 0; p < columnSetInPosition.ColumnCount; p++)
						{
							GridColumn gridColumn = columnSetInPosition[n, p];
							if (gridColumn != null && gridColumn.Visible && gridColumn._columnSetRow == n && gridColumn.ColumnSetColumn == p)
							{
								GridFormatStyle firstApplyOverrides = applyOverrides.ApplyOverrides(gridColumn.CellStyle);
								GridCell gridColumnCell = gridRow.Cells[gridColumn];
								GridFormatStyle secondApplyOverrides = firstApplyOverrides.ApplyOverrides(gridColumnCell.GetEffectiveFormatStyle());
								GridFormatStyle thirdApplyOverrides = secondApplyOverrides.ApplyOverrides(gridColumnCell.FormatStyle);
								ExportCell otherCell = CreateCell(gridColumnCell.Value, gridColumnCell.Text);
								otherCell.SetTypeCode(gridColumn.DataTypeCode);
								otherCell.SetExportValueKind(gridColumn.ExportValueKind);
								otherCell.GetAppearance().ApplyFormatStyle(thirdApplyOverrides, dataGrid);
								if (gridColumn.FormatString.Length > 0)
								{
									if (gridColumn.FormatMode == FormatMode.UseIFormattable)
									{
										string otherDisplayText = gridColumn.GetDisplayText(gridColumnCell.Value, true);
										if (otherDisplayText == gridColumnCell.Text)
										{
											otherCell.SetNumberFormat(gridColumn.FormatString);
										}
										else
										{
											otherCell.SetIsTextOnly(true);
										}
									}
									else
									{
										otherCell.SetIsTextOnly(true);
									}
								}
								else
								{
									string nextDisplayText = gridColumn.GetDisplayText(gridColumnCell.Value, true);
									if (nextDisplayText != gridColumnCell.Text)
									{
										otherCell.SetIsTextOnly(true);
									}
								}
								if (gridColumn.HasValueList && gridColumn.ValueList.ReplaceValues)
								{
									otherCell.SetIsTextOnly(true);
								}
								else if (gridColumn.DropDown != null && gridColumn.DropDown.ReplaceValues)
								{
									otherCell.SetIsTextOnly(true);
								}
								otherCell.SetMergeAcross(gridColumn.ColSpan - 1);
								array[n].GetCells().Add(otherCell);
							}
							else if (gridColumn == null || !gridColumn.Visible || gridColumn.ColumnSetRow != n)
							{
								ExportCell nextCell = CreateCell("", "");
								nextCell.GetAppearance().ApplyFormatStyle(gridFormatStyle, dataGrid);
								array[n].GetCells().Add(nextCell);
							}
						}
					}
				}
			}
			for (int q = 0; q < array.Length; q++)
			{
				worksheet.AddRow(array[q]);
			}
		}

		private void AddTotalRow(ExportWorksheet worksheet, MxDataGrid dataGrid, GridRow gridRow)
		{
			int value = 1;
			value = ((gridRow.Table.CellLayoutMode == CellLayoutMode.UseColumns) ? 1 : gridRow.Table.ColumnSetRowCount);
			ExportRow[] array = new ExportRow[value];
			for (int i = 0; i < value; i++)
			{
				array[i] = new ExportRow();
				array[i]._rowKind = (int)_tableLevels[gridRow.Table];
			}
			GridFormatStyle gridFormatStyle;
			if (gridRow.Table.Groups.Count > 0)
			{
				gridFormatStyle = dataGrid.GetGroupRowStyle(gridRow.Table);
				for (int j = 0; j < gridRow.Table.Groups.Count; j++)
				{
					GridFormatStyle otherGridFormatStyle = ((gridRow.Table.Groups[j].Column == null) ? gridFormatStyle : gridFormatStyle.ApplyOverrides(gridRow.Table.Groups[j].Column.GroupRowStyle));
					for (int k = 0; k < value; k++)
					{
						ExportCell padCell = CreateCell(null, "");
						padCell.GetAppearance().ApplyFormatStyle(otherGridFormatStyle, dataGrid);
						array[k].GetCells().Add(padCell);
					}
				}
			}
			gridFormatStyle = dataGrid.GetTotalRowStyle(gridRow.Table);
			if (gridRow.Table.CellLayoutMode == CellLayoutMode.UseColumns)
			{
				for (int position = 0; position < gridRow.Table.Columns.Count; position++)
				{
					GridColumn columnInPosition = gridRow.Table.Columns.GetColumnInPosition(position);
					if (columnInPosition == null || !columnInPosition.Visible)
					{
						continue;
					}
					GridFormatStyle applyOverrides = gridFormatStyle.ApplyOverrides(columnInPosition.CellStyle);
					GridCell columnInPositionCell = gridRow.Cells[columnInPosition];
					GridFormatStyle otherApplyOverrides = applyOverrides.ApplyOverrides(columnInPositionCell.FormatStyle);
					ExportCell cell = CreateCell(columnInPositionCell.Value, columnInPositionCell.Text);
					switch (columnInPosition.AggregateFunction)
					{
					case AggregateFunction.Count:
					case AggregateFunction.Sum:
					case AggregateFunction.Average:
					case AggregateFunction.StdDeviation:
					case AggregateFunction.ValueCount:
						cell.SetTypeCode(TypeCode.Double);
						break;
					default:
						cell.SetTypeCode(columnInPosition.DataTypeCode);
						cell.SetExportValueKind(columnInPosition.ExportValueKind);
						break;
					}
					cell.GetAppearance().ApplyFormatStyle(otherApplyOverrides, dataGrid);
					if (columnInPosition.TotalFormatString.Length > 0)
					{
						if (columnInPosition.TotalFormatMode == FormatMode.UseIFormattable)
						{
							string text = columnInPosition.GetDisplayText(columnInPositionCell.Value, true);
							if (text == columnInPositionCell.Text)
							{
								cell.SetNumberFormat(columnInPosition.TotalFormatString);
							}
							else
							{
								cell.SetIsTextOnly(true);
							}
						}
						else
						{
							cell.SetIsTextOnly(true);
						}
					}
					if (cell.GetCellValue() == null || cell.GetCellValue() == DBNull.Value)
					{
						cell.SetIsTextOnly(true);
					}
					array[0].GetCells().Add(cell);
				}
			}
			else
			{
				for (int m = 0; m < gridRow.Table.ColumnSets.Count; m++)
				{
					GridColumnSet columnSetInPosition = gridRow.Table.ColumnSets.GetColumnSetInPosition(m);
					if (!columnSetInPosition.Visible)
					{
						continue;
					}
					for (int n = 0; n < columnSetInPosition.ColumnCount; n++)
					{
						for (int p = 0; p < gridRow.Table.ColumnSetRowCount; p++)
						{
							GridColumn gridColumn = columnSetInPosition[p, n];
							if (gridColumn != null && gridColumn.Visible && gridColumn._columnSetRow == p && gridColumn.ColumnSetColumn == n)
							{
								GridFormatStyle nextApplyOverrides = gridFormatStyle.ApplyOverrides(gridColumn.CellStyle);
								GridCell gridColumnCell = gridRow.Cells[gridColumn];
								GridFormatStyle previousApplyOverrides = nextApplyOverrides.ApplyOverrides(gridColumnCell.FormatStyle);
								ExportCell otherCell = CreateCell(gridColumnCell.Value, gridColumnCell.Text);
								switch (gridColumn.AggregateFunction)
								{
								case AggregateFunction.Count:
								case AggregateFunction.Sum:
								case AggregateFunction.Average:
								case AggregateFunction.StdDeviation:
								case AggregateFunction.ValueCount:
									otherCell.SetTypeCode(TypeCode.Double);
									break;
								default:
									otherCell.SetTypeCode(gridColumn.DataTypeCode);
									otherCell.SetExportValueKind(gridColumn.ExportValueKind);
									break;
								}
								otherCell.GetAppearance().ApplyFormatStyle(previousApplyOverrides, dataGrid);
								if (gridColumn.TotalFormatString.Length > 0)
								{
									if (gridColumn.TotalFormatMode == FormatMode.UseIFormattable)
									{
										string displayText = gridColumn.GetDisplayText(gridColumnCell.Value, true);
										if (displayText == gridColumnCell.Text)
										{
											otherCell.SetNumberFormat(gridColumn.TotalFormatString);
										}
										else
										{
											otherCell.SetIsTextOnly(true);
										}
									}
									else
									{
										otherCell.SetIsTextOnly(true);
									}
								}
								if (otherCell.GetCellValue() == null || otherCell.GetCellValue() == DBNull.Value)
								{
									otherCell.SetIsTextOnly(true);
								}
								otherCell.SetMergeAcross(gridColumn.ColSpan - 1);
								array[p].GetCells().Add(otherCell);
							}
							else if (gridColumn == null || gridColumn.ColumnSetRow != p)
							{
								ExportCell nextCell = CreateCell("", "");
								nextCell.GetAppearance().ApplyFormatStyle(gridFormatStyle, dataGrid);
								array[p].GetCells().Add(nextCell);
							}
						}
					}
				}
			}
			for (int q = 0; q < array.Length; q++)
			{
				worksheet.AddRow(array[q]);
			}
		}

		public void ProcessStream(Stream stream, GridExporter gridExporter, GridRow[] gridRowArray, CancellationToken cancellationToken)
		{
			_tableLevels = new Hashtable();
			_dpi = GetScreenDpi(gridExporter.DataGrid);
			GridTable[] array;
			if (gridExporter.IncludeChildTables)
			{
				array = new GridTable[gridExporter.DataGrid.Tables.Count];
				for (int i = 0; i < gridExporter.DataGrid.Tables.Count; i++)
				{
					array[i] = gridExporter.DataGrid.Tables[i];
				}
			}
			else
			{
				array = new GridTable[1] { gridExporter.DataGrid.RootTable };
			}
			using (ExportWorksheet worksheet = new ExportWorksheet(gridExporter.IncludeFormatStyle, cancellationToken, new ExportRowBuffer(gridExporter.RowBufferMemoryLimit)))
			{
				BuildRows(worksheet, gridExporter, gridRowArray, array);
				worksheet.WriteDocument(stream, gridExporter);
			}
			stream.Flush();
		}
	}
}
