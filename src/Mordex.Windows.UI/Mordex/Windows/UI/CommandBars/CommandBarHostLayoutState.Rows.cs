using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	internal partial class CommandBarHostLayoutState
	{
		public bool HasDraggedBar()
		{
			foreach (MxCommandBar item in _bars)
			{
				if (item._messageFilter != null)
				{
					return true;
				}
			}
			return false;
		}

		public int GetThicknessWithFollowingRows()
		{
			if (_isHorizontal)
			{
				if (_nextRow != null)
				{
					return _bounds.Height + _nextRow.GetThicknessWithFollowingRows();
				}
				return _bounds.Height;
			}
			if (_nextRow != null)
			{
				return _bounds.Width + _nextRow.GetThicknessWithFollowingRows();
			}
			return _bounds.Width;
		}

		public int ComputeMaxBarThickness()
		{
			int maxThickness = 0;
			foreach (MxCommandBar item in GetBars())
			{
				if (item.Visible && item.GetFloatingWindow() == null && item._thickness > maxThickness)
				{
					maxThickness = item._thickness;
				}
			}
			return maxThickness;
		}

		private int SumMinSizes()
		{
			int totalSize = 0;
			foreach (MxCommandBar item in _bars)
			{
				if (item.Visible)
				{
					totalSize += item.GetMinimumLength();
				}
			}
			return totalSize;
		}

		private int SumMinSizesWithNextRowBar()
		{
			int totalSize = 0;
			foreach (MxCommandBar item in _bars)
			{
				if (item.Visible)
				{
					totalSize += item.GetMinimumLength();
				}
			}
			if (_nextRow != null && _nextRow.GetBars().Count > 0)
			{
				totalSize += _nextRow.GetBars()[_nextRow.GetBars().Count - 1].GetMinimumLength();
			}
			return totalSize;
		}

		private void PullBarFromNextRow()
		{
			if (_nextRow == null)
			{
				return;
			}
			if (_nextRow.GetBars().Count > 0)
			{
				MxCommandBar uiCommandBar = _nextRow.GetBars()[_nextRow.GetBars().Count - 1];
				_nextRow._isMovingBar = true;
				_nextRow.GetBars().Remove(uiCommandBar);
				_nextRow._isMovingBar = false;
				uiCommandBar.Location = uiCommandBar.GetPosition();
				GetBars().Add(uiCommandBar);
			}
			else
			{
				if (_nextRow.HasRowOwner())
				{
					_nextRow = _nextRow._nextRow;
				}
				else
				{
					_nextRow = null;
				}
				_host.LayoutRows();
			}
		}

		private void PushBarToNextRow()
		{
			_isMovingBar = true;
			ArrayList arrayList = _bars.GetSortedBars(_isHorizontal, false);
			MxCommandBar uiCommandBar = (MxCommandBar)arrayList[arrayList.Count - 1];
			_bars.Remove(uiCommandBar);
			if (_nextRow == null)
			{
				_nextRow = new CommandBarHostLayoutState(_host, _rowIndex, this);
				if (_isHorizontal)
				{
					_nextRow._bounds = new Rectangle(_bounds.X, _bounds.Bottom, _bounds.Width, _bounds.Height);
				}
				else
				{
					_nextRow._bounds = new Rectangle(_bounds.Right, _bounds.Y, _bounds.Width, _bounds.Height);
				}
				_nextRow.GetBars().Add(uiCommandBar);
			}
			else
			{
				_nextRow.GetBars().Add(uiCommandBar);
			}
			_host.LayoutRows();
			_isMovingBar = false;
			if (_isHorizontal)
			{
				uiCommandBar.MoveToLeft(0);
			}
			else
			{
				uiCommandBar.MoveToTop(0);
			}
			RebalanceRows();
			if (_nextRow != null)
			{
				_nextRow.RebalanceRows();
			}
		}

		public void RebalanceRows()
		{
			if (_bounds.Width > 0 && _bounds.Height > 0 && _host.CommandManager != null && !_host.CommandManager.IsInitializing())
			{
				int minSizeWithPull = SumMinSizesWithNextRowBar();
				int minSizeTotal = SumMinSizes();
				if (_isHorizontal)
				{
					if (_bounds.Width > minSizeWithPull)
					{
						if (_nextRow != null)
						{
							PullBarFromNextRow();
							RebalanceRows();
						}
					}
					else if (_bounds.Width < minSizeTotal)
					{
						PushBarToNextRow();
					}
				}
				else if (_bounds.Height > minSizeWithPull)
				{
					if (_nextRow != null)
					{
						PullBarFromNextRow();
						RebalanceRows();
					}
				}
				else if (_bounds.Height < minSizeTotal)
				{
					PushBarToNextRow();
				}
			}
			foreach (MxCommandBar item in GetBars())
			{
				item.InvalidateLayout();
			}
		}

		private void ResizeBarsWidth(int delta)
		{
			_isLayoutEnabled = false;
			int newWidth = 0;
			ArrayList arrayList = _bars.GetSortedBars(_isHorizontal, false);
			if (delta < 0)
			{
				foreach (MxCommandBar item in arrayList)
				{
					if (item.Visible && item.GetHostRow() == this)
					{
						newWidth = item.Width + delta;
						if (newWidth >= item.GetMinimumLength())
						{
							item.Width = newWidth;
							break;
						}
						delta += item.Width - item.GetMinimumLength();
						item.Width = item.GetMinimumLength();
					}
				}
			}
			else
			{
				for (int barIndex = arrayList.Count - 1; barIndex >= 0; barIndex--)
				{
					MxCommandBar candidateBar = (MxCommandBar)arrayList[barIndex];
					if (candidateBar.Visible && candidateBar.GetHostRow() == this && !candidateBar.IsFullShown)
					{
						newWidth = candidateBar.Width + delta;
						if (newWidth <= candidateBar.GetFullLength())
						{
							candidateBar.Width = newWidth;
							break;
						}
						delta -= candidateBar.GetFullLength() - candidateBar.Width;
						candidateBar.Width = candidateBar.GetFullLength();
					}
				}
			}
			if (IsContentFull())
			{
				PackBars();
			}
			_isLayoutEnabled = true;
		}

		private void ResizeBarsHeight(int delta)
		{
			_isLayoutEnabled = false;
			int newHeight = 0;
			ArrayList arrayList = _bars.GetSortedBars(_isHorizontal, false);
			if (delta < 0)
			{
				foreach (MxCommandBar item in arrayList)
				{
					if (item.Visible && item.GetHostRow() == this)
					{
						newHeight = item.Height + delta;
						if (newHeight >= item.GetMinimumLength())
						{
							item.Height = newHeight;
							break;
						}
						delta += item.Height - item.GetMinimumLength();
						item.Height = item.GetMinimumLength();
					}
				}
			}
			else
			{
				for (int barIndex = arrayList.Count - 1; barIndex >= 0; barIndex--)
				{
					MxCommandBar candidateBar = (MxCommandBar)arrayList[barIndex];
					if (candidateBar.Visible && candidateBar.GetHostRow() == this && !candidateBar.IsFullShown)
					{
						newHeight = candidateBar.Height + delta;
						if (newHeight <= candidateBar.GetFullLength())
						{
							candidateBar.Height = newHeight;
							break;
						}
						delta -= candidateBar.GetFullLength() - candidateBar.Height;
						candidateBar.Height = candidateBar.GetFullLength();
					}
				}
			}
			if (IsContentFull())
			{
				PackBars();
			}
			_isLayoutEnabled = true;
		}

		public void PackBars()
		{
			if (!IsRowFilled())
			{
				return;
			}
			ArrayList arrayList = _bars.GetSortedBars(_isHorizontal, false);
			int nextPosition = 0;
			if (_bounds.Width == 0 || _bounds.Height == 0 || _isPacking)
			{
				return;
			}
			_isPacking = true;
			foreach (MxCommandBar item in arrayList)
			{
				if (!item.Visible)
				{
					continue;
				}
				if (item._isLayoutDirty)
				{
					item.RecalculateLayout(true);
				}
				if (_isHorizontal)
				{
					if (item.Left != nextPosition)
					{
						item.MoveToLeft(nextPosition);
					}
					nextPosition = item.Right;
				}
				else
				{
					if (item.Top != nextPosition)
					{
						item.MoveToTop(nextPosition);
					}
					nextPosition = item.Bottom;
				}
			}
			_isPacking = false;
		}
	}
}
