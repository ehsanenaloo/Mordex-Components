using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	internal partial class RibbonGroupBoxRenderer
	{
		public override void DoDefaultAction()
		{
			if (_isCollapsed)
			{
				ShowCollapsedPopup();
			}
		}

		public bool TryFocusItemInColumn(int columnIndex, int rowIndex, bool moveForward)
		{
			if (columnIndex < _commandColumns.Count && columnIndex >= 0)
			{
				RibbonCommandColumn thisValue = _commandColumns[columnIndex];
				int lastRowIndex = GetRenderHost().MaxItemRows - 1;
				if (thisValue._items.Count - 2 > lastRowIndex && thisValue._items.Count % 3 != 0)
				{
					lastRowIndex = thisValue._items.Count % 3;
				}
				if (rowIndex > lastRowIndex)
				{
					rowIndex = lastRowIndex;
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
				if (moveForward)
				{
					commandBaseRenderer = thisValue._items[rowIndex];
				}
				else
				{
					int partialRowCount = thisValue._items.Count % 3;
					if (rowIndex > 0 && partialRowCount > 0 && GetRenderHost().MaxItemRows > partialRowCount)
					{
						rowIndex--;
					}
					commandBaseRenderer = thisValue._items[thisValue._items.Count - rowIndex - 1];
				}
				if (commandBaseRenderer.GetCommand() is SeparatorCommand)
				{
					if (moveForward)
					{
						return TryFocusItemInColumn(columnIndex + 1, rowIndex, moveForward);
					}
					return TryFocusItemInColumn(columnIndex - 1, rowIndex, moveForward);
				}
				if (commandBaseRenderer._layoutItemOwner is IKeyboardNavigable && commandBaseRenderer._layoutItemOwner != this)
				{
					if (moveForward)
					{
						return ((IKeyboardNavigable)commandBaseRenderer._layoutItemOwner).MoveRight(null);
					}
					return ((IKeyboardNavigable)commandBaseRenderer._layoutItemOwner).MoveLeft(null);
				}
				if (commandBaseRenderer is IKeyboardNavigable)
				{
					if (moveForward)
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

		public override Point GetKeyTipLocation(Graphics graphics, TextLayoutRenderer textRenderer, Font font, string keyTip)
		{
			if (keyTip.Length == 1)
			{
				keyTip = "W";
			}
			int keyTipWidth = (int)textRenderer.MeasureText(graphics, keyTip, font, 0, 0).Width;
			int x = _bounds.X + (_bounds.Width - keyTipWidth) / 2;
			int y = _bounds.Bottom - 1;
			return new Point(x, y);
		}

		public override void OnMouseDown(MouseEventArgs eventArgs)
		{
			if (!GetRibbonGroup().Enabled)
			{
				base.OnMouseDown(eventArgs);
				return;
			}
			if (_isCollapsed)
			{
				if (eventArgs.Button == MouseButtons.Left)
				{
					ShowCollapsedPopup();
				}
				return;
			}
			if (_dialogLauncher != null && eventArgs.Button == MouseButtons.Left)
			{
				bool isOverLauncher = _dialogLauncher._bounds.Contains(eventArgs.X, eventArgs.Y);
				if (isOverLauncher != _dialogLauncher.Pushed)
				{
					_dialogLauncher.Pushed = isOverLauncher;
				}
			}
			base.OnMouseDown(eventArgs);
		}

		public override void OnMouseMove(MouseEventArgs eventArgs)
		{
			if (_dialogLauncher != null && GetRibbonGroup().Enabled)
			{
				_dialogLauncher.Hot = _dialogLauncher._bounds.Contains(eventArgs.X, eventArgs.Y);
			}
			base.OnMouseMove(eventArgs);
		}

		public override void OnMouseUp(MouseEventArgs eventArgs)
		{
			if (_dialogLauncher != null && _dialogLauncher.Pushed && GetRibbonGroup().Enabled)
			{
				_dialogLauncher.Pushed = false;
				_dialogLauncher.Hot = false;
				if (_dialogLauncher._bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					_commandBaseRendererRibbon.TryCloseParentControl(CommandCloseReason.CommandClicked);
					GetRibbonGroup().RaiseDialogButtonClick();
				}
			}
			else
			{
				base.OnMouseUp(eventArgs);
			}
		}

		internal override CommandDropInfo GetDropInfo(Point pt, CommandBase command)
		{
			if (_isCollapsed)
			{
				return null;
			}
			if (_childRenderers.Count == 0)
			{
				CommandDropInfo ribbonLayoutState = new CommandDropInfo();
				ribbonLayoutState._dropBounds = _bounds;
				ribbonLayoutState._dropIndex = 0;
				return ribbonLayoutState;
			}
			Rectangle rectangle = _bounds;
			rectangle.Inflate(-2, -2);
			rectangle.Height -= _captionBarBounds.Height;
			if (rectangle.Contains(pt))
			{
				foreach (RibbonCommandColumn item in _commandColumns)
				{
					Rectangle columnBounds = item.GetBounds();
					if (columnBounds.X <= pt.X && columnBounds.Right >= pt.X)
					{
						if (item._items.Count == 1 && item._items[0].GetCommand() == command)
						{
							return null;
						}
						CommandDropInfo columnDropInfo;
						if (pt.Y < columnBounds.Y)
						{
							columnDropInfo = new CommandDropInfo();
							CommandBaseRenderer thisValue = item._items[0];
							columnDropInfo._dropBounds = new Rectangle(columnBounds.X, thisValue._bounds.Y, columnBounds.Width, thisValue._bounds.Height);
							columnDropInfo._dropIndex = _childRenderers.IndexOf(thisValue);
							return columnDropInfo;
						}
						CommandBaseRenderer lastItem = item._items[item._items.Count - 1];
						columnDropInfo = new CommandDropInfo();
						columnDropInfo._dropBounds = new Rectangle(columnBounds.X, lastItem._bounds.Y + lastItem._bounds.Height, columnBounds.Width, lastItem._bounds.Height);
						columnDropInfo._dropIndex = _childRenderers.IndexOf(lastItem);
						if (!item.ContainsCommand(command))
						{
							columnDropInfo._dropIndex++;
						}
						return columnDropInfo;
					}
				}
			}
			return null;
		}
	}
}
