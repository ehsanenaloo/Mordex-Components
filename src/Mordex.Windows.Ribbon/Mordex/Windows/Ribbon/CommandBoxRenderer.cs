using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	internal class CommandBoxRenderer : CommandContainerRenderer, IKeyboardNavigable
	{
		private IList<RibbonCommandColumn> _columns;

		public override AccessibleRole AccessibleRole
		{
			get
			{
				if (GetCommand() != null && GetCommand().AccessibleRole != AccessibleRole.Default)
				{
					return GetCommand().AccessibleRole;
				}
				return AccessibleRole.Grouping;
			}
		}

		public override bool Focusable => false;

		public override CommandType CommandType => CommandType.Box;

		public CommandBoxRenderer(CommandBox command, ICommandRenderHost owner, CommandBaseRenderer layoutItemOwner)
			: base(command, owner, layoutItemOwner)
		{
		}

		public CommandBox GetCommandBox()
		{
			return _commandBaseRendererCommand as CommandBox;
		}

		public int GetPreferredItemHeight(int availableHeight)
		{
			if (GetCommandBox().BoxStyle == BoxStyle.Horizontal)
			{
				int maxItemHeight = GetRenderHost().SmallItemHeight;
				{
					foreach (CommandBaseRenderer item in _childRenderers)
					{
						int itemHeight = GetItemHeight(item, availableHeight);
						if (itemHeight > maxItemHeight)
						{
							maxItemHeight = itemHeight;
						}
					}
					return maxItemHeight;
				}
			}
			return availableHeight;
		}

		internal override CommandDropInfo GetDropInfo(Point pt, CommandBase command)
		{
			if (_childRenderers.Count == 0)
			{
				CommandDropInfo ribbonLayoutState = new CommandDropInfo();
				ribbonLayoutState._dropBounds = _bounds;
				ribbonLayoutState._dropIndex = 0;
				return ribbonLayoutState;
			}
			bool isCtrlPressed = Control.ModifierKeys == Keys.Control;
			foreach (RibbonCommandColumn item in _columns)
			{
				Rectangle rectangle = item.GetBounds();
				if (GetCommandBox().BoxStyle == BoxStyle.Vertical)
				{
					if (rectangle.X <= pt.X && rectangle.Right >= pt.X)
					{
						if (!isCtrlPressed && item._items.Count == 1 && item._items[0].GetCommand() == command)
						{
							return null;
						}
						CommandDropInfo verticalDropInfo;
						if (pt.Y < rectangle.Y)
						{
							verticalDropInfo = new CommandDropInfo();
							CommandBaseRenderer thisValue = item._items[0];
							verticalDropInfo._dropBounds = new Rectangle(rectangle.X, thisValue._bounds.Y, rectangle.Width, thisValue._bounds.Height);
							verticalDropInfo._dropIndex = _childRenderers.IndexOf(thisValue);
							return verticalDropInfo;
						}
						CommandBaseRenderer lastVerticalItem = item._items[item._items.Count - 1];
						verticalDropInfo = new CommandDropInfo();
						verticalDropInfo._dropBounds = new Rectangle(rectangle.X, lastVerticalItem._bounds.Y + lastVerticalItem._bounds.Height, rectangle.Width, lastVerticalItem._bounds.Height);
						verticalDropInfo._dropIndex = _childRenderers.IndexOf(lastVerticalItem);
						if (isCtrlPressed || !item.ContainsCommand(command))
						{
							verticalDropInfo._dropIndex++;
						}
						return verticalDropInfo;
					}
				}
				else if (rectangle.Y <= pt.Y && rectangle.Bottom >= pt.Y)
				{
					if (!isCtrlPressed && item._items.Count == 1 && item._items[0].GetCommand() == command)
					{
						return null;
					}
					CommandDropInfo horizontalDropInfo;
					if (pt.X < rectangle.X)
					{
						horizontalDropInfo = new CommandDropInfo();
						CommandBaseRenderer firstHorizontalItem = item._items[0];
						horizontalDropInfo._dropBounds = new Rectangle(firstHorizontalItem._bounds.X, rectangle.Y, firstHorizontalItem._bounds.Width, rectangle.Height);
						horizontalDropInfo._dropIndex = _childRenderers.IndexOf(firstHorizontalItem);
						return horizontalDropInfo;
					}
					CommandBaseRenderer lastHorizontalItem = item.GetItems()[item._items.Count - 1];
					horizontalDropInfo = new CommandDropInfo();
					horizontalDropInfo._dropBounds = new Rectangle(lastHorizontalItem._bounds.X + lastHorizontalItem._bounds.Width - 10, rectangle.Y, lastHorizontalItem._bounds.Width, rectangle.Height);
					horizontalDropInfo._dropIndex = _childRenderers.IndexOf(lastHorizontalItem);
					if (isCtrlPressed || !item.ContainsCommand(command))
					{
						horizontalDropInfo._dropIndex++;
					}
					return horizontalDropInfo;
				}
			}
			return null;
		}

		public override void CalculateFullItemWidth(Graphics graphics, TextLayoutRenderer textRenderer, Font font)
		{
			_childRenderers = new List<CommandBaseRenderer>();
			int columnWidth = 0;
			int maxColumnHeight = GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows;
			int columnHeight = 0;
			int totalWidth = 0;
			int rowIndex = 0;
			int itemsSpace = GetCommandBox().ItemsSpace;
			_columns = new List<RibbonCommandColumn>();
			RibbonCommandColumn currentColumn = null;
			if (GetCommandBox().BoxStyle == BoxStyle.Vertical)
			{
				foreach (CommandBase command in GetCommandBox().Commands)
				{
					if (command.Visible)
					{
						CommandBaseRenderer commandBaseRenderer = CreateLayoutItem(command);
						commandBaseRenderer.CalculateFullItemWidth(graphics, textRenderer, font);
						int itemHeight = GetItemHeight(commandBaseRenderer, maxColumnHeight);
						if (columnHeight + itemHeight > maxColumnHeight)
						{
							rowIndex = 0;
							totalWidth += columnWidth + itemsSpace;
							columnHeight = itemHeight;
							columnWidth = commandBaseRenderer._fullItemWidth;
						}
						else if (commandBaseRenderer._fullItemWidth > columnWidth)
						{
							columnWidth = commandBaseRenderer._fullItemWidth;
							columnHeight += itemHeight;
						}
						else
						{
							columnHeight += itemHeight;
						}
						CommandType commandType = commandBaseRenderer.CommandType;
						if (commandType == CommandType.ButtonGroup || commandType == CommandType.ContainerControl)
						{
							commandType = CommandType.SmallButton;
						}
						if (currentColumn == null || commandType != currentColumn._commandsType)
						{
							rowIndex = 0;
							currentColumn = new RibbonCommandColumn(GetRenderHost(), commandType);
							_columns.Add(currentColumn);
						}
						commandBaseRenderer._rowInColumn = rowIndex;
						commandBaseRenderer._columnIndex = _columns.Count - 1;
						currentColumn._items.Add(commandBaseRenderer);
						_childRenderers.Add(commandBaseRenderer);
						rowIndex++;
						if (rowIndex >= GetRenderHost().MaxItemRows)
						{
							rowIndex = 0;
						}
					}
				}
				if (columnWidth > 0)
				{
					totalWidth += columnWidth;
				}
			}
			else
			{
				foreach (CommandBase horizontalCommand in GetCommandBox().Commands)
				{
					if (!horizontalCommand.Visible)
					{
						continue;
					}
					CommandBaseRenderer horizontalRenderer = CreateLayoutItem(horizontalCommand);
					horizontalRenderer.CalculateFullItemWidth(graphics, textRenderer, font);
					horizontalRenderer._rowInColumn = 0;
					horizontalRenderer._columnIndex = 0;
					if (currentColumn == null)
					{
						CommandType columnType = horizontalRenderer.CommandType;
						if (columnType == CommandType.ButtonGroup || columnType == CommandType.ContainerControl)
						{
							columnType = CommandType.SmallButton;
						}
						currentColumn = new RibbonCommandColumn(GetRenderHost(), columnType);
						_columns.Add(currentColumn);
					}
					currentColumn._items.Add(horizontalRenderer);
					_childRenderers.Add(horizontalRenderer);
					totalWidth += horizontalRenderer._fullItemWidth;
				}
				totalWidth += (_childRenderers.Count - 1) * itemsSpace;
			}
			if (totalWidth == 0 && GetRenderHost().IsInDesignMode)
			{
				totalWidth = maxColumnHeight;
			}
			_fullItemWidth = totalWidth;
			SetLayoutWidth(_fullItemWidth);
		}

		public override void ArrangeItem(Graphics graphics, TextLayoutRenderer textRenderer, int left, int top, int width, int height)
		{
			_bounds = new Rectangle(left, top, width, height);
			int currentX = _bounds.Left;
			int currentY = _bounds.Top;
			int columnWidth = 0;
			int maxColumnHeight = GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows;
			int itemsSpace = GetCommandBox().ItemsSpace;
			if (GetCommandBox().BoxStyle == BoxStyle.Vertical)
			{
				foreach (CommandBaseRenderer item in _childRenderers)
				{
					int verticalItemHeight = GetItemHeight(item, GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows);
					if (currentY + verticalItemHeight > _bounds.Top + maxColumnHeight + 2)
					{
						currentY = _bounds.Top;
						currentX += columnWidth;
						columnWidth = 0;
					}
					item._ownerBottom = _ownerBottom;
					item.ArrangeItem(graphics, textRenderer, currentX, currentY, item.GetLayoutWidth(), verticalItemHeight);
					currentY += verticalItemHeight + itemsSpace;
					if (item._bounds.Width > columnWidth)
					{
						columnWidth = item._bounds.Width;
					}
				}
			}
			else
			{
				foreach (CommandBaseRenderer horizontalItem in _childRenderers)
				{
					int itemHeight = GetItemHeight(horizontalItem, GetRenderHost().SmallItemHeight * GetRenderHost().MaxItemRows);
					horizontalItem._ownerBottom = _ownerBottom;
					horizontalItem.ArrangeItem(graphics, textRenderer, currentX, currentY, horizontalItem.GetLayoutWidth(), itemHeight);
					currentX += horizontalItem._bounds.Width + itemsSpace;
				}
			}
			if (GetCommandBox().CenterItems && GetCommandBox().BoxStyle == BoxStyle.Vertical)
			{
				CenterColumnsVertically();
			}
		}

		private void CenterColumnsVertically()
		{
			int height = _bounds.Height;
			foreach (RibbonCommandColumn item in _columns)
			{
				item.SpreadLastGroupVertically(height, GetCommandBox().ItemsSpace);
			}
		}

		public bool CanShrinkColumns()
		{
			if (((CommandBox)GetCommand()).AllowAutoSizeItems)
			{
				foreach (RibbonCommandColumn item in _columns)
				{
					if (item.CanShrink(true))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void MoveY(int deltaY)
		{
			base.MoveY(deltaY);
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				item.MoveY(deltaY);
			}
		}

		public bool TryFocusItemInColumn(int columnIndex, int rowIndex, bool isForward)
		{
			if (columnIndex < _columns.Count && columnIndex >= 0)
			{
				RibbonCommandColumn thisValue = _columns[columnIndex];
				int maxRowIndex = GetRenderHost().MaxItemRows - 1;
				if (thisValue._items.Count - 2 > maxRowIndex && thisValue._items.Count % 3 != 0)
				{
					maxRowIndex = thisValue._items.Count % 3;
				}
				if (rowIndex > maxRowIndex)
				{
					rowIndex = maxRowIndex;
				}
				if (rowIndex > thisValue._items.Count - 1)
				{
					rowIndex = thisValue._items.Count - 1;
				}
				if (thisValue._commandsType == CommandType.LargeButton)
				{
					rowIndex = 0;
				}
				CommandBaseRenderer commandBaseRenderer;
				if (isForward)
				{
					commandBaseRenderer = thisValue._items[rowIndex];
				}
				else
				{
					int leftoverCount = thisValue._items.Count % 3;
					if (rowIndex > 0 && leftoverCount > 0 && GetRenderHost().MaxItemRows > leftoverCount)
					{
						rowIndex--;
					}
					commandBaseRenderer = thisValue._items[thisValue._items.Count - rowIndex - 1];
				}
				if (commandBaseRenderer.GetCommand() is SeparatorCommand)
				{
					if (isForward)
					{
						return TryFocusItemInColumn(columnIndex + 1, rowIndex, isForward);
					}
					return TryFocusItemInColumn(columnIndex - 1, rowIndex, isForward);
				}
				if (commandBaseRenderer._layoutItemOwner is IKeyboardNavigable && commandBaseRenderer._layoutItemOwner != this)
				{
					if (isForward)
					{
						return ((IKeyboardNavigable)commandBaseRenderer._layoutItemOwner).MoveRight(null);
					}
					return ((IKeyboardNavigable)commandBaseRenderer._layoutItemOwner).MoveLeft(null);
				}
				if (commandBaseRenderer is IKeyboardNavigable)
				{
					if (isForward)
					{
						return ((IKeyboardNavigable)commandBaseRenderer).SelectFirstItem();
					}
					return ((IKeyboardNavigable)commandBaseRenderer).SelectLastItem();
				}
				GetRenderHost().ActiveInputItem = commandBaseRenderer;
				return true;
			}
			return false;
		}

		bool IKeyboardNavigable.MoveNext(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				if (((IKeyboardNavigable)this).SelectFirstItem())
				{
					return true;
				}
				return false;
			}
			RibbonCommandColumn thisValue = _columns[selectedItem._columnIndex];
			if (thisValue.TryMoveNext(selectedItem))
			{
				return true;
			}
			int rowIndex = 0;
			int columnIndex = selectedItem._columnIndex + 1;
			return TryFocusItemInColumn(columnIndex, rowIndex, true);
		}

		bool IKeyboardNavigable.MovePrevious(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				if (((IKeyboardNavigable)this).SelectLastItem())
				{
					return true;
				}
				return false;
			}
			RibbonCommandColumn thisValue = _columns[selectedItem._columnIndex];
			if (thisValue.TryMovePrevious(selectedItem))
			{
				return true;
			}
			int rowIndex = 0;
			int columnIndex = selectedItem._columnIndex - 1;
			return TryFocusItemInColumn(columnIndex, rowIndex, false);
		}

		bool IKeyboardNavigable.MoveRight(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectFirstItem();
			}
			RibbonCommandColumn thisValue = _columns[selectedItem._columnIndex];
			if (thisValue.TryMoveRight(selectedItem))
			{
				return true;
			}
			int rowIndex = selectedItem._rowInColumn;
			int columnIndex = selectedItem._columnIndex + 1;
			if (selectedItem.CommandType == CommandType.LargeButton)
			{
				rowIndex = GetRenderHost().MaxItemRows / 2;
			}
			if (TryFocusItemInColumn(columnIndex, rowIndex, true))
			{
				return true;
			}
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveRight(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveLeft(CommandBaseRenderer selectedItem)
		{
			if (selectedItem == null)
			{
				return ((IKeyboardNavigable)this).SelectLastItem();
			}
			RibbonCommandColumn thisValue = _columns[selectedItem._columnIndex];
			if (thisValue.TryMoveLeft(selectedItem))
			{
				return true;
			}
			int rowIndex = selectedItem._rowInColumn;
			int columnIndex = selectedItem._columnIndex - 1;
			if (selectedItem.CommandType == CommandType.LargeButton)
			{
				rowIndex = GetRenderHost().MaxItemRows / 2;
			}
			if (TryFocusItemInColumn(columnIndex, rowIndex, false))
			{
				return true;
			}
			if (_layoutItemOwner is IKeyboardNavigable)
			{
				return ((IKeyboardNavigable)_layoutItemOwner).MoveLeft(this);
			}
			return false;
		}

		bool IKeyboardNavigable.MoveDown(CommandBaseRenderer selectedItem)
		{
			if (GetCommandBox().BoxStyle == BoxStyle.Vertical)
			{
				int columnIndex = -1;
				if (selectedItem == null)
				{
					columnIndex = 0;
				}
				else
				{
					if (selectedItem._layoutItemOwner != this)
					{
						if (selectedItem._layoutItemOwner is IKeyboardNavigable)
						{
							return ((IKeyboardNavigable)selectedItem._layoutItemOwner).MoveDown(selectedItem);
						}
						selectedItem = selectedItem._layoutItemOwner;
						while (selectedItem != null && selectedItem._layoutItemOwner != this)
						{
							selectedItem = selectedItem._layoutItemOwner;
						}
					}
					if (selectedItem == null)
					{
						return false;
					}
					columnIndex = selectedItem._columnIndex;
				}
				RibbonCommandColumn column = null;
				if (columnIndex >= 0)
				{
					column = _columns[columnIndex];
				}
				if (column != null)
				{
					return column.TryMoveDown(selectedItem);
				}
			}
			return false;
		}

		bool IKeyboardNavigable.MoveUp(CommandBaseRenderer selectedItem)
		{
			if (GetCommandBox().BoxStyle == BoxStyle.Vertical)
			{
				int columnIndex = -1;
				if (selectedItem == null)
				{
					columnIndex = 0;
				}
				else
				{
					if (selectedItem._layoutItemOwner != this)
					{
						if (selectedItem._layoutItemOwner is IKeyboardNavigable)
						{
							return ((IKeyboardNavigable)selectedItem._layoutItemOwner).MoveUp(selectedItem);
						}
						selectedItem = selectedItem._layoutItemOwner;
						while (selectedItem != null && selectedItem._layoutItemOwner != this)
						{
							selectedItem = selectedItem._layoutItemOwner;
						}
					}
					if (selectedItem == null)
					{
						return false;
					}
					columnIndex = selectedItem._columnIndex;
				}
				RibbonCommandColumn column = null;
				if (columnIndex >= 0)
				{
					column = _columns[columnIndex];
				}
				if (column != null)
				{
					return column.TryMoveUp(selectedItem);
				}
			}
			return false;
		}

		bool IKeyboardNavigable.SelectLastItem()
		{
			for (int index = _childRenderers.Count - 1; index >= 0; index--)
			{
				CommandBaseRenderer thisValue = _childRenderers[index];
				if (!(thisValue.GetCommand() is SeparatorCommand))
				{
					if (thisValue is IKeyboardNavigable && ((IKeyboardNavigable)thisValue).SelectLastItem())
					{
						return true;
					}
					GetRenderHost().ActiveInputItem = thisValue;
					return true;
				}
			}
			return false;
		}

		bool IKeyboardNavigable.SelectFirstItem()
		{
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (!(item.GetCommand() is SeparatorCommand))
				{
					if (item is IKeyboardNavigable && ((IKeyboardNavigable)item).SelectFirstItem())
					{
						return true;
					}
					GetRenderHost().ActiveInputItem = item;
					return true;
				}
			}
			return false;
		}

		void IKeyboardNavigable.AddKeyTips(Graphics graphics, TextLayoutRenderer textRenderer, Font font, IList<RibbonKeyTipEntry> keyTips, bool showToolTip)
		{
			bool useCompatibleTextRendering = GetRenderHost().UseCompatibleTextRendering;
			foreach (CommandBaseRenderer itemRenderer in _childRenderers)
			{
				if (itemRenderer is IKeyboardNavigable)
				{
					((IKeyboardNavigable)itemRenderer).AddKeyTips(graphics, textRenderer, font, keyTips, showToolTip);
				}
				else
				{
					if (!(itemRenderer.GetCommand() is IKeyTipProvider))
					{
						continue;
					}
					IKeyTipProvider keyTipProvider = (IKeyTipProvider)itemRenderer.GetCommand();
					if (keyTipProvider.HasKeyTip)
					{
						SuperTipSettingsWindowMessageHandler superTipSettingsWindowMessageHandler = new SuperTipSettingsWindowMessageHandler(font, useCompatibleTextRendering, keyTipProvider.KeyTip, GetCommand().Ribbon._colorScheme);
						superTipSettingsWindowMessageHandler.SetCompactMode(true);
						superTipSettingsWindowMessageHandler.SetSticky(true);
						superTipSettingsWindowMessageHandler._isKeyTipEnabled = itemRenderer.GetCommand().IsEnabled;
						if (showToolTip)
						{
							superTipSettingsWindowMessageHandler.ShowAt(GetRenderHost().ParentControl, GetRenderHost().ParentControl.PointToScreen(itemRenderer.GetKeyTipLocation(graphics, textRenderer, font, keyTipProvider.KeyTip)));
						}
						RibbonKeyTipEntry item = new RibbonKeyTipEntry(keyTipProvider.KeyTip, itemRenderer.GetCommand(), itemRenderer, superTipSettingsWindowMessageHandler, superTipSettingsWindowMessageHandler._isKeyTipEnabled);
						keyTips.Add(item);
					}
				}
			}
		}

		CommandBase IKeyboardNavigable.GetCommandAt(int x, int y)
		{
			if (_bounds.Contains(x, y))
			{
				foreach (CommandBaseRenderer item in _childRenderers)
				{
					if (item._bounds.Contains(x, y))
					{
						if (item is IKeyboardNavigable)
						{
							return ((IKeyboardNavigable)item).GetCommandAt(x, y);
						}
						return item.GetCommand();
					}
				}
				return GetCommand();
			}
			return null;
		}

		CommandBaseRenderer IKeyboardNavigable.GetCommandLayoutItem(CommandBase command)
		{
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (item.GetCommand() == command)
				{
					return item;
				}
				if (item is IKeyboardNavigable)
				{
					CommandBaseRenderer commandLayoutItem = ((IKeyboardNavigable)item).GetCommandLayoutItem(command);
					if (commandLayoutItem != null)
					{
						return commandLayoutItem;
					}
				}
			}
			return null;
		}

		void IKeyboardNavigable.RemoveControls()
		{
			if (GetRenderHost() == null || GetRenderHost().ParentControl == null)
			{
				return;
			}
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				if (item is ContainerControlCommandRenderer)
				{
					Control control = ((ContainerControlCommandRenderer)item).GetHostedControl();
					if (GetRenderHost().ParentControl.Controls.Contains(control))
					{
						GetRenderHost().ParentControl.Controls.Remove(control);
					}
				}
				else if (item is IKeyboardNavigable)
				{
					((IKeyboardNavigable)item).RemoveControls();
				}
			}
		}

		public override void Paint(Graphics graphics, TextLayoutRenderer textRenderer, RibbonColorScheme colorScheme, Font font, Color foreColor, Rectangle clipRectangle)
		{
			if (GetRenderHost().IsInDesignMode)
			{
				Pen pen = new Pen(Color.Black);
				pen.DashStyle = DashStyle.Dot;
				RibbonDrawingUtils.DrawRectangleOutline(graphics, pen, _bounds);
				pen.Dispose();
			}
			foreach (CommandBaseRenderer item in _childRenderers)
			{
				Rectangle rectangle = Rectangle.Intersect(clipRectangle, item._bounds);
				if (rectangle.Width > 0 && rectangle.Height > 0)
				{
					item.Paint(graphics, textRenderer, colorScheme, font, foreColor, clipRectangle);
				}
			}
		}
	}
}
