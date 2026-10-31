using System;
using System.Collections;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	internal partial class CommandBarHostLayoutState
	{
		void ICommandBarOwner.OnBarAdded(MxCommandBar commandBar)
		{
			commandBar._isRowLayoutInProgress = true;
			commandBar.SetHostRow(this);
			if (commandBar._isLayoutDirty)
			{
				commandBar.RecalculateLayout(true);
			}
			if (_isHorizontal)
			{
				int totalMinSize = SumMinSizes();
				if (_bounds.Width > 0 && _bounds.Width < totalMinSize)
				{
					int minOffsetX = ComputeMinOffsetX(commandBar);
					if (commandBar.Location.X == 0 && minOffsetX > 0)
					{
						ArrayList orderedBars = _bars.GetSortedBars(_isHorizontal, false);
						if (orderedBars.Count > 0)
						{
							MxCommandBar lastBar = (MxCommandBar)orderedBars[orderedBars.Count - 1];
							if (lastBar == commandBar && orderedBars.Count > 1)
							{
								lastBar = (MxCommandBar)orderedBars[orderedBars.Count - 2];
							}
							int newX = lastBar.GetPosition().X + 1;
							commandBar.MoveToPosition(new Point(newX, commandBar.GetPosition().Y));
							commandBar.MoveToLeft(newX);
						}
					}
					PushBarToNextRow();
					if (!GetBars().Contains(commandBar))
					{
						RecalculateContentSize();
						if (IsContentFull())
						{
							PackBars();
						}
						return;
					}
				}
				commandBar.Top = _bounds.Top;
				if (_bars.Count == 1 || _bounds.Height < commandBar.Height)
				{
					_host.LayoutRows();
				}
			}
			else
			{
				int totalMinSizeY = SumMinSizes();
				if (_bounds.Height > 0 && _bounds.Height < totalMinSizeY)
				{
					int minOffsetY = ComputeMinOffsetX(commandBar);
					if (commandBar.Location.Y == 0 && minOffsetY > 0)
					{
						ArrayList orderedBarsY = _bars.GetSortedBars(_isHorizontal, false);
						if (orderedBarsY.Count > 0)
						{
							MxCommandBar lastBarY = (MxCommandBar)orderedBarsY[orderedBarsY.Count - 1];
							if (lastBarY == commandBar && orderedBarsY.Count > 1)
							{
								lastBarY = (MxCommandBar)orderedBarsY[orderedBarsY.Count - 2];
							}
							int newY = lastBarY.GetPosition().Y + 1;
							commandBar.MoveToPosition(new Point(commandBar.GetPosition().X, newY));
							commandBar.MoveToTop(newY);
						}
					}
					PushBarToNextRow();
					if (!GetBars().Contains(commandBar))
					{
						RecalculateContentSize();
						if (IsContentFull())
						{
							PackBars();
						}
						return;
					}
				}
				commandBar.Left = _bounds.Left;
				if (_bars.Count == 1 || _bounds.Width < commandBar.Width)
				{
					_host.LayoutRows();
				}
			}
			commandBar.SetHostRow(this);
			commandBar.Resize += OnControlResize;
			commandBar.VisibleChanged += OnControlVisibleChanged;
			RecalculateContentSize();
			if (_bars.Count > 1)
			{
				if (_isHorizontal)
				{
					int currentMinOffsetX = ComputeMinOffsetX(commandBar);
					if (commandBar.Location.X == 0 && currentMinOffsetX > 0)
					{
						ArrayList orderedBarsAfterAdd = _bars.GetSortedBars(_isHorizontal, false);
						if (orderedBarsAfterAdd.Count > 0)
						{
							MxCommandBar lastBarAfterAdd = (MxCommandBar)orderedBarsAfterAdd[orderedBarsAfterAdd.Count - 1];
							if (lastBarAfterAdd == commandBar && orderedBarsAfterAdd.Count > 1)
							{
								lastBarAfterAdd = (MxCommandBar)orderedBarsAfterAdd[orderedBarsAfterAdd.Count - 2];
							}
							int adjustedX = lastBarAfterAdd.GetPosition().X + 1;
							commandBar.MoveToPosition(new Point(adjustedX, commandBar.GetPosition().Y));
							commandBar.MoveToLeft(adjustedX);
						}
					}
				}
				else
				{
					int currentMinOffsetY = ComputeMinOffsetY(commandBar);
					if (commandBar.Location.Y == 0 && currentMinOffsetY > 0)
					{
						ArrayList orderedBarsAfterAddY = _bars.GetSortedBars(_isHorizontal, false);
						if (orderedBarsAfterAddY.Count > 0)
						{
							MxCommandBar lastBarAfterAddY = (MxCommandBar)orderedBarsAfterAddY[orderedBarsAfterAddY.Count - 1];
							if (lastBarAfterAddY == commandBar && orderedBarsAfterAddY.Count > 1)
							{
								lastBarAfterAddY = (MxCommandBar)orderedBarsAfterAddY[orderedBarsAfterAddY.Count - 2];
							}
							int adjustedY = lastBarAfterAddY.GetPosition().Y + 1;
							commandBar.MoveToPosition(new Point(commandBar.GetPosition().X, adjustedY));
							commandBar.MoveToTop(adjustedY);
						}
					}
				}
			}
			if (_host != null && _host._commandManager != null && _host._commandManager.GetMergedManager() == null)
			{
				if (_isHorizontal)
				{
					if (commandBar.Location.X == 0)
					{
						Point freeLocationX = commandBar.Location;
						foreach (MxCommandBar item in GetBars())
						{
							if (item != commandBar && item.Location.X == freeLocationX.X && item.RowIndex == commandBar._rowIndex)
							{
								if (item._isLayoutDirty)
								{
									item.RecalculateLayout(true);
								}
								freeLocationX = new Point(item.Right, freeLocationX.Y);
							}
						}
						commandBar.Location = freeLocationX;
					}
				}
				else if (commandBar.Location.Y == 0)
				{
					Point freeLocationY = commandBar.Location;
					foreach (MxCommandBar rowBar in GetBars())
					{
						if (rowBar != commandBar && rowBar.Location.Y == freeLocationY.Y && rowBar.RowIndex == commandBar._rowIndex)
						{
							if (rowBar._isLayoutDirty)
							{
								rowBar.RecalculateLayout(true);
							}
							freeLocationY = new Point(freeLocationY.X, rowBar.Bottom);
						}
					}
					commandBar.Location = freeLocationY;
				}
				if (commandBar.GetPosition().IsEmpty)
				{
					commandBar.MoveToPosition(commandBar.Location);
				}
			}
			if (IsContentFull())
			{
				PackBars();
			}
			commandBar.RecalculateLayout();
			commandBar._isRowLayoutInProgress = false;
			RebalanceRows();
		}

		void ICommandBarOwner.OnBarRemoved(MxCommandBar commandBar)
		{
			if (_savedRowIndex == -1 && _bars.Count > 0)
			{
				SetSavedRowIndex(commandBar.RowIndex);
			}
			commandBar.SetHostRow(null);
			commandBar.Resize -= OnControlResize;
			commandBar.VisibleChanged -= OnControlVisibleChanged;
			RecalculateContentSize();
			if (_bars.Count == 0)
			{
				if (!HasRowOwner())
				{
					_host.GetRows().Remove(this);
				}
				else if (!_isMovingBar)
				{
					_rowOwner.RebalanceRows();
				}
			}
			else
			{
				NotifyLayoutChanged(true);
			}
			if (!_isMovingBar)
			{
				_host.LayoutRows();
				RebalanceRows();
			}
		}

		internal void NotifyLayoutChanged(bool isGrowing)
		{
			HasRowOwner();
		}

		MxCommandManager ICommandBarOwner.GetCommandManagerOwner()
		{
			return _host.CommandManager;
		}
	}
}
