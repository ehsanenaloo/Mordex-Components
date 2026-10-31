using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.CommandBars
{
	public partial class MxCommandBar
	{
		private void LayoutWrapped(Graphics graphics, TextLayoutRenderer textLayoutRenderer, StateStyles uiStateStyles)
		{
			_layoutItems.ClearItems();
			ArrayList arrayList = new ArrayList();
			_customizeItem = null;
			_maxItemLength = 0;
			CommandLayoutRow commandBarsState = new CommandLayoutRow();
			int bandLength = 0;
			int bandThickness = 0;
			int totalLength = 0;
			int customizeLength = 0;
			int nextX = base.ClientRectangle.Left;
			int nextY = base.ClientRectangle.Top;
			bool isHorizontal = true;
			if (_floatingWindow == null)
			{
				if (_hostRow == null)
				{
					return;
				}
				if (IsHorizontalLayout())
				{
					nextX++;
				}
				else
				{
					isHorizontal = false;
					nextY++;
				}
			}
			if (CanMove() && _floatingWindow == null)
			{
				if (_gripItem == null)
				{
					_gripItem = new FormatStyleLayoutState(this);
				}
				if (isHorizontal)
				{
					_gripItem._bounds = new Rectangle(nextX, nextY, GripSize, base.ClientRectangle.Height - 2);
					nextX += GripSize;
				}
				else
				{
					_gripItem._bounds = new Rectangle(nextX, nextY, base.ClientRectangle.Width - 2, GripSize);
					nextY += GripSize;
				}
			}
			else
			{
				_gripItem = null;
			}
			if (ShouldShowCustomizeButton() && _floatingWindow == null)
			{
				customizeLength = GetCustomizeButtonLength();
				if (_customizeItem == null)
				{
					_customizeItem = new CommandLayoutState(GetCustomizeCommand(), "", this);
				}
			}
			else
			{
				_customizeItem = null;
			}
			FormatStyle formatStyle = uiStateStyles.FormatStyle;
			if (IsMdiChildMaximized() && _activeMdiChild != null && _activeMdiChild.ControlBox)
			{
				int mdiButtonWidth = (MdiButtonWidth);
				totalLength += mdiButtonWidth * MdiButtonCount + MdiButtonCount;
				if (_mdiButtons == null)
				{
					_mdiButtons = new CommandBarWindowButtonsState(this);
				}
				if (isHorizontal)
				{
					_mdiButtons._bounds = new Rectangle(0, 0, totalLength, MdiButtonsThickness);
				}
				else
				{
					_mdiButtons._bounds = new Rectangle(0, 0, MdiButtonsThickness, totalLength);
				}
				MxCommand command = BuildMdiSystemMenu();
				CommandLayoutState uiCommandLayoutState = new CommandLayoutState(command, "", this);
				_layoutItems.AddItem(uiCommandLayoutState);
				View view = _view;
				_view = View.SmallIcons;
				Size size = ((!IsHorizontalLayout()) ? MeasureVerticalItem(graphics, uiCommandLayoutState, nextY, nextX, textLayoutRenderer, formatStyle) : MeasureHorizontalItem(graphics, uiCommandLayoutState, nextX, nextY, textLayoutRenderer, formatStyle));
				_view = view;
				CommandLayoutRow mdiButtonRow = new CommandLayoutRow();
				mdiButtonRow.GetItems().Add(uiCommandLayoutState);
				if (isHorizontal)
				{
					commandBarsState._thickness = size.Height;
					commandBarsState._length = size.Width;
				}
				else
				{
					commandBarsState._thickness = size.Width;
					commandBarsState._length = size.Height;
				}
				arrayList.Add(mdiButtonRow);
			}
			else
			{
				_mdiButtons = null;
			}
			if (_commands.Count > 0)
			{
				ArrayList commandList = _commands.GetCommandsSortedByAlignment();
				_isFullShown = true;
				FormatStyle uiFormatStyle = null;
				bool canAddSeparator = false;
				foreach (MxCommand item in commandList)
				{
					if (item.IsEffectivelyVisible())
					{
						if (item.IsSeparator())
						{
							if (!canAddSeparator)
							{
								continue;
							}
							canAddSeparator = false;
						}
						else
						{
							canAddSeparator = true;
						}
						CommandLayoutState layoutItem = new CommandLayoutState(item, item.GetEffectiveText(), this);
						((FormatStyleLayoutState)layoutItem).SetEnabled((item.IsEnabled && base.Enabled) || CommandManager._isCustomizing);
						uiFormatStyle = formatStyle;
						if (!item.GetEffectiveStateStyles().FormatStyle.IsDefaultStyle())
						{
							uiFormatStyle = uiFormatStyle.MergeStyle(item.GetEffectiveStateStyles().FormatStyle);
						}
						Size itemSize;
						if (isHorizontal)
						{
							itemSize = MeasureHorizontalItem(graphics, layoutItem, nextX, nextY, textLayoutRenderer, uiFormatStyle);
							if (_maxItemLength < itemSize.Width)
							{
								_maxItemLength = itemSize.Width;
							}
						}
						else
						{
							itemSize = MeasureVerticalItem(graphics, layoutItem, nextY, nextX, textLayoutRenderer, uiFormatStyle);
							if (_maxItemLength < itemSize.Height)
							{
								_maxItemLength = itemSize.Height;
							}
						}
						if (item.IsSeparator())
						{
							commandBarsState._thickness = bandThickness;
							commandBarsState._length = bandLength;
							arrayList.Add(commandBarsState);
							bandLength = 0;
							bandThickness = 0;
							commandBarsState = new CommandLayoutRow();
						}
						if (isHorizontal)
						{
							bandLength += itemSize.Width;
							totalLength += itemSize.Width;
							if (itemSize.Height > bandThickness)
							{
								bandThickness = itemSize.Height;
							}
						}
						else
						{
							bandLength += itemSize.Height;
							totalLength += itemSize.Height;
							if (itemSize.Width > bandThickness)
							{
								bandThickness = itemSize.Width;
							}
						}
						layoutItem.SetHasDropDown(item.IsParent);
						commandBarsState.GetItems().Add(layoutItem);
						_layoutItems.AddItem(layoutItem);
						uiFormatStyle.DisposeResources();
					}
					else if (item.HasEmbeddedControl())
					{
						RemoveHostedControl(item);
					}
					formatStyle.DisposeResources();
				}
				if (commandBarsState.GetItems().Count > 0)
				{
					for (int index = commandBarsState.GetItems().Count - 1; index >= 0; index--)
					{
						CommandLayoutState lastItem = (CommandLayoutState)commandBarsState.GetItems()[index];
						if (lastItem.GetCommand() == null || !lastItem.GetCommand().IsSeparator())
						{
							break;
						}
						totalLength = ((!isHorizontal) ? (totalLength - lastItem._bounds.Height) : (totalLength - lastItem._bounds.Width));
						commandBarsState.GetItems().Remove(lastItem);
					}
				}
				if (commandBarsState.GetItems().Count > 0)
				{
					commandBarsState._thickness = bandThickness;
					commandBarsState._length = bandLength;
					arrayList.Add(commandBarsState);
				}
			}
			else
			{
				totalLength = GetMinimumLength();
			}
			if (_mdiButtons != null)
			{
				CommandLayoutRow mdiBand = new CommandLayoutRow();
				mdiBand.GetItems().Add(_mdiButtons);
				mdiBand._length = MdiButtonsBandLength;
				mdiBand._thickness = MdiButtonsBandThickness;
				arrayList.Add(mdiBand);
			}
			int rowCount = 0;
			if (totalLength < MinBarLength)
			{
				totalLength = MinBarLength;
			}
			int rowLength = 0;
			if (_floatingWindow != null)
			{
				rowLength = FloatingSize.Width;
			}
			else
			{
				rowLength = ((!isHorizontal) ? GetHostRow()._bounds.Height : GetHostRow()._bounds.Width);
				rowLength = ((_gripItem == null) ? (rowLength - 1) : (rowLength - GripSize - 1));
				rowLength -= customizeLength + 1;
			}
			if (rowLength > 0)
			{
				if (rowLength < _maxItemLength)
				{
					rowLength = _maxItemLength;
				}
				rowCount = totalLength / rowLength;
				if (totalLength % rowLength != 0 && arrayList.Count > 0 && Commands.Count > 0)
				{
					rowCount++;
				}
			}
			else
			{
				rowLength = _maxItemLength;
				rowCount = 1;
			}
			Size barSize = LayoutRows(arrayList, rowCount, totalLength, rowLength, isHorizontal);
			if (_floatingWindow != null)
			{
				_floatingSize = barSize;
				_floatingRowCount = rowCount;
			}
			if (isHorizontal)
			{
				SetThickness(barSize.Height);
			}
			else
			{
				SetThickness(barSize.Width);
			}
			Size = barSize;
			if (_gripItem != null)
			{
				if (_isHorizontal)
				{
					_gripItem._bounds.Height = base.ClientRectangle.Height - 2;
				}
				else
				{
					_gripItem._bounds.Width = base.ClientRectangle.Width - 2;
				}
			}
			if (_customizeItem != null)
			{
				if (isHorizontal)
				{
					_customizeItem._bounds = new Rectangle(base.ClientRectangle.Right - customizeLength - 2, nextY + 2, customizeLength, GetThickness() - 4);
				}
				else {
					_customizeItem._bounds = new Rectangle(nextX + 2, base.ClientRectangle.Bottom - customizeLength - 2, GetThickness() - 4, customizeLength);
				}
			}
			if (_mdiButtons != null)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				if (_customizeItem != null)
				{
					if (isHorizontal)
					{
						clientRectangle.Width -= customizeLength + 1;
					}
					else
					{
						clientRectangle.Height -= customizeLength + 1;
					}
				}
				if (_gripItem != null)
				{
					_mdiButtons.LayoutButtons(clientRectangle, isHorizontal, MdiButtonsOffsetWithGrip);
				}
				else
				{
					_mdiButtons.LayoutButtons(clientRectangle, isHorizontal, 2);
				}
			}
			SetFullLength(totalLength);
		}

		private bool CanFitInRow(int rowSpace, int totalSpace, int bandLength, int itemCount, int remainingLength)
		{
			if (rowSpace >= bandLength)
			{
				return true;
			}
			float fillRatio = (float)rowSpace / (float)bandLength;
			totalSpace -= rowSpace + (remainingLength - bandLength);
			if (fillRatio >= (float)totalSpace / (float)bandLength && itemCount > 1)
			{
				return true;
			}
			return false;
		}

		private bool CanFitItem(int rowSpace, int totalSpace, int remainingLength, int itemLength)
		{
			if (rowSpace >= itemLength)
			{
				return true;
			}
			if (GetFloatingWindow() == null)
			{
				return false;
			}
			float fillRatio = (float)rowSpace / (float)itemLength;
			totalSpace -= remainingLength - itemLength;
			if (fillRatio >= (float)totalSpace / (float)itemLength)
			{
				return true;
			}
			return false;
		}

		private bool MoveSeparatorToOwnRow(CommandLayoutRow commandBarsState, ref CommandLayoutRow pendingRow, int index, ref int remainingLength)
		{
			if (CommandBarType == CommandBarType.Menu)
			{
				return false;
			}
			if (commandBarsState.GetItems().Count == 0)
			{
				CommandLayoutState uiCommandLayoutState = (CommandLayoutState)pendingRow.GetItems()[0];
				bool isSeparator = false;
				if (uiCommandLayoutState.GetCommand() != null && uiCommandLayoutState.GetCommand().IsSeparator())
				{
					isSeparator = true;
				}
				if (isSeparator)
				{
					remainingLength -= SeparatorThickness;
					pendingRow._length -= SeparatorThickness;
					pendingRow.GetItems().Remove(uiCommandLayoutState);
					CommandLayoutRow separatorRow = new CommandLayoutRow();
					separatorRow.GetItems().Add(uiCommandLayoutState);
					separatorRow._thickness = SeparatorThickness;
					separatorRow._isSingleCommandLine = true;
					_rows.Insert(index, separatorRow);
					if (_layoutItems.ContainsItem(uiCommandLayoutState))
					{
						_layoutItems.RemoveItem(uiCommandLayoutState);
					}
					return true;
				}
			}
			return false;
		}

		private int FillRow(CommandLayoutRow commandBarsState, ref CommandLayoutRow pendingRow, int rowSpace, int totalSpace, ref int remainingLength, bool skipFitCheck, bool isHorizontal)
		{
			int usedLength = 0;
			int index;
			for (index = 0; index < pendingRow.GetItems().Count; index++)
			{
				CommandLayoutState uiCommandLayoutState = (CommandLayoutState)pendingRow.GetItems()[index];
				bool isSeparator = false;
				if (uiCommandLayoutState.GetCommand() != null && uiCommandLayoutState.GetCommand().IsSeparator())
				{
					isSeparator = true;
				}
				int itemLength = 0;
				itemLength = ((!isHorizontal) ? uiCommandLayoutState._bounds.Height : uiCommandLayoutState._bounds.Width);
				if (index == 0 && commandBarsState.GetItems().Count == 0 && isSeparator)
				{
					remainingLength -= itemLength;
					usedLength += itemLength;
					pendingRow.GetItems().Remove(uiCommandLayoutState);
					index--;
				}
				else
				{
					if (!skipFitCheck && !CanFitItem(rowSpace, totalSpace - usedLength, remainingLength, itemLength))
					{
						break;
					}
					commandBarsState.GetItems().Add(uiCommandLayoutState);
					rowSpace -= itemLength;
					commandBarsState._length -= itemLength;
					remainingLength -= itemLength;
					usedLength += itemLength;
					pendingRow.GetItems().Remove(uiCommandLayoutState);
					index--;
				}
			}
			return usedLength;
		}

		internal bool CanMerge()
		{
			if (_allowMerge == InheritableBoolean.False)
			{
				return false;
			}
			if (_allowMerge == InheritableBoolean.True)
			{
				return true;
			}
			return _commandManager.AllowMerge;
		}

		internal bool GetIsMergedCommandBar()
		{
			return _isMergedCommandBar;
		}

		internal void SetIsMergedCommandBar(bool isMergedCommandBar)
		{
			_isMergedCommandBar = isMergedCommandBar;
		}

		internal int GetFloatingRowCount()
		{
			return _floatingRowCount;
		}

		private Size LayoutRows(ArrayList arrayList, int rowCount, int totalLength, int rowLength, bool isHorizontal)
		{
			int rowSize = rowLength;
			_rows = new ArrayList();
			for (int i = 0; i < rowCount; i++)
			{
				CommandLayoutRow value = new CommandLayoutRow();
				_rows.Add(value);
			}
			rowCount--;
			int rowIndex = 0;
			int rowSpace = rowSize;
			int totalSpace = rowSize * (rowCount + 1);
			int remainingLength = totalLength;
			CommandLayoutRow commandBarsState = null;
			for (int j = 0; j < arrayList.Count; j++)
			{
				CommandLayoutRow pendingRow = (CommandLayoutRow)arrayList[j];
				if (rowIndex == rowCount || rowSpace == rowSize || CanFitInRow(rowSpace, totalSpace, pendingRow._length, pendingRow.GetItems().Count, remainingLength))
				{
					commandBarsState = (CommandLayoutRow)_rows[rowIndex];
					if (MoveSeparatorToOwnRow(commandBarsState, ref pendingRow, rowIndex, ref remainingLength))
					{
						rowIndex++;
						rowCount++;
						totalSpace = rowSize * (rowCount + 1 - rowIndex);
						rowSpace = rowSize;
						j--;
						continue;
					}
					int usedLength = FillRow(commandBarsState, ref pendingRow, rowSpace, totalSpace, ref remainingLength, rowIndex == rowCount && _floatingWindow != null, isHorizontal);
					if (pendingRow.GetItems().Count > 0)
					{
						rowIndex++;
						if (rowIndex >= rowCount + 1)
						{
							rowCount++;
							CommandLayoutRow newRow = new CommandLayoutRow();
							_rows.Add(newRow);
						}
						totalSpace = rowSize * (rowCount + 1 - rowIndex);
						rowSpace = rowSize;
						j--;
					}
					else
					{
						rowSpace -= usedLength;
						totalSpace -= usedLength;
					}
				}
				else
				{
					rowIndex++;
					totalSpace = rowSize * (rowCount + 1 - rowIndex);
					rowSpace = rowSize;
					j--;
				}
			}
			if (isHorizontal)
			{
				int rowTop = 0;
				int maxWidth = 0;
				foreach (CommandLayoutRow item in _rows)
				{
					Size size;
					if (item._isSingleCommandLine)
					{
						CommandLayoutState uiCommandLayoutState = (CommandLayoutState)item.GetItems()[0];
						uiCommandLayoutState._bounds = new Rectangle(0, rowTop, 0, item._thickness);
						size = new Size(0, item._thickness);
					}
					else
					{
						size = ArrangeRow(item, rowTop);
						if (_floatingWindow == null && CommandBarType == CommandBarType.ToolBar)
						{
							size.Height += ToolBarRowPadding;
						}
					}
					item._thickness = size.Height;
					CenterItemsInRow(size.Height, item.GetItems(), GetEffectiveTextAlign());
					rowTop += size.Height;
					if (size.Width > maxWidth)
					{
						maxWidth = size.Width;
					}
				}
				if (rowTop == 0)
				{
					rowTop = EmptyRowThickness;
				}
				if (maxWidth <= MinBarLength)
				{
					maxWidth = MinBarLength;
				}
				return new Size(maxWidth, rowTop);
			}
			int rowLeft = 0;
			int maxHeight = 0;
			foreach (CommandLayoutRow row in _rows)
			{
				Size size;
				if (row._isSingleCommandLine)
				{
					CommandLayoutState separatorItem = (CommandLayoutState)row.GetItems()[0];
					separatorItem._bounds = new Rectangle(rowLeft, 0, row._thickness, 0);
					size = new Size(row._thickness, 0);
				}
				else
				{
					size = ArrangeColumn(row, rowLeft);
					if (CommandBarType == CommandBarType.ToolBar)
					{
						size.Width += ToolBarRowPadding;
					}
					else
					{
						size.Width += 2;
					}
				}
				CenterItemsInColumn(size.Width, row.GetItems(), GetEffectiveTextAlign());
				rowLeft += size.Width;
				row._thickness = size.Width;
				if (size.Height > maxHeight)
				{
					maxHeight = size.Height;
				}
			}
			if (rowLeft == 0)
			{
				rowLeft = EmptyRowThickness;
			}
			if (maxHeight <= MinBarLength)
			{
				maxHeight = MinBarLength;
			}
			return new Size(rowLeft, maxHeight);
		}

		private Size ArrangeRow(CommandLayoutRow commandBarsState, int rowTop)
		{
			int x = base.ClientRectangle.Left;
			int rowHeight = MinRowThickness;
			foreach (CommandLayoutState item in commandBarsState.GetItems())
			{
				Rectangle rectangle = (item._bounds = new Rectangle(item._bounds.X + x, rowTop, item._bounds.Width, item._bounds.Height));
				if (item.HasDropDown() && _commandBarType != CommandBarType.Menu && (item.GetCommand() == null || item.GetCommand().GetEffectiveDropDownButtonStyle() == DropDownButtonStyle.SplitButton))
				{
					item._dropDownBounds = new Rectangle(item._dropDownBounds.X + x, rowTop, item._dropDownBounds.Width, item._dropDownBounds.Height);
				}
				Rectangle textBounds = new Rectangle(item._textBounds.X + x, item._textBounds.Y + rowTop, item._textBounds.Width, item._textBounds.Height);
				item._textBounds = textBounds;
				Rectangle imageBounds = new Rectangle(item._imageBounds.X + x, item._imageBounds.Top + rowTop, item._imageBounds.Width, item._imageBounds.Height);
				item._imageBounds = imageBounds;
				if (rectangle.Height > rowHeight)
				{
					rowHeight = rectangle.Height;
				}
				x += rectangle.Width;
				if (item.GetCommand() != null && item._hostsControl)
				{
					_isLayoutDirty = false;
					PlaceHostedControl(item);
					_isLayoutDirty = false;
				}
			}
			return new Size(x, rowHeight);
		}

		private Size ArrangeColumn(CommandLayoutRow commandBarsState, int columnLeft)
		{
			int y = base.ClientRectangle.Top;
			int columnWidth = 0;
			foreach (CommandLayoutState item in commandBarsState.GetItems())
			{
				Rectangle rectangle = (item._bounds = new Rectangle(columnLeft, y + item._bounds.Y, item._bounds.Width, item._bounds.Height));
				if (item.HasDropDown() && _commandBarType != CommandBarType.Menu && (item.GetCommand() == null || item.GetCommand().GetEffectiveDropDownButtonStyle() == DropDownButtonStyle.SplitButton))
				{
					item._dropDownBounds = new Rectangle(columnLeft + item._dropDownBounds.X, item._dropDownBounds.Y + y, item._dropDownBounds.Width, item._dropDownBounds.Height);
				}
				Rectangle textBounds = new Rectangle(item._textBounds.X + columnLeft, item._textBounds.Y + y, item._textBounds.Width, item._textBounds.Height);
				item._textBounds = textBounds;
				Rectangle imageBounds = new Rectangle(item._imageBounds.X + columnLeft, item._imageBounds.Top + y, item._imageBounds.Width, item._imageBounds.Height);
				item._imageBounds = imageBounds;
				if (rectangle.Width > columnWidth)
				{
					columnWidth = rectangle.Width;
				}
				y += rectangle.Height;
			}
			return new Size(columnWidth, y);
		}
	}
}
