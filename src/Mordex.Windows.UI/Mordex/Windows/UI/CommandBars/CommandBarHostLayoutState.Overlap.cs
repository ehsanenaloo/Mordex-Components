using System;
using System.Collections;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	internal partial class CommandBarHostLayoutState
	{
		internal int GetMaxAvailableExtent(MxCommandBar uiCommandBar)
		{
			if (_isHorizontal)
			{
				int maxWidth = _bounds.Width;
				if (!IsContentFull())
				{
					return maxWidth;
				}
				int otherWidths = 0;
				foreach (MxCommandBar item in _bars)
				{
					if (item.Visible && uiCommandBar != item && item.GetPosition().X >= uiCommandBar.GetPosition().X)
					{
						if (item.Left < maxWidth && uiCommandBar.Left < item.Left)
						{
							maxWidth = item.Left;
						}
						otherWidths += item.Width;
					}
				}
				if (_bounds.Width - otherWidths > maxWidth)
				{
					return _bounds.Width - otherWidths;
				}
				return maxWidth;
			}
			int maxHeight = _bounds.Height;
			if (!IsContentFull())
			{
				return maxHeight;
			}
			int otherHeights = 0;
			foreach (MxCommandBar otherBar in _bars)
			{
				if (otherBar.Visible && uiCommandBar != otherBar && otherBar.GetPosition().Y >= uiCommandBar.GetPosition().Y)
				{
					if (otherBar.Top < maxHeight && uiCommandBar.Top < otherBar.Top)
					{
						maxHeight = otherBar.Top;
					}
					otherHeights += otherBar.Height;
				}
			}
			if (_bounds.Height - otherHeights > maxHeight)
			{
				return _bounds.Height - otherHeights;
			}
			return maxHeight;
		}

		internal int GetDesiredBarExtent(MxCommandBar uiCommandBar)
		{
			int minExtent = uiCommandBar.GetMinimumLength();
			if (_isHorizontal)
			{
				int availableWidth = _bounds.Width;
				bool allOthersFullWidth = true;
				int othersPreferredWidth = 0;
				foreach (MxCommandBar item in _bars)
				{
					if (item.Visible && uiCommandBar != item)
					{
						if (!item.IsFullShown)
						{
							allOthersFullWidth = false;
						}
						else
						{
							availableWidth -= item.Width;
						}
						othersPreferredWidth += item.GetFullLength();
					}
				}
				if (_contentSize >= _bounds.Width)
				{
					if (allOthersFullWidth && _contentSize == _bounds.Width)
					{
						if (availableWidth > minExtent)
						{
							return availableWidth;
						}
					}
					else if (_bounds.Width - othersPreferredWidth > minExtent)
					{
						return _bounds.Width - othersPreferredWidth;
					}
				}
			}
			else
			{
				int availableHeight = _bounds.Height;
				bool allOthersFullHeight = true;
				int othersPreferredHeight = 0;
				foreach (MxCommandBar otherBar in _bars)
				{
					if (otherBar.Visible && uiCommandBar != otherBar)
					{
						if (!otherBar.IsFullShown)
						{
							allOthersFullHeight = false;
						}
						else
						{
							availableHeight -= otherBar.Height;
						}
						othersPreferredHeight += otherBar.GetFullLength();
					}
				}
				if (_contentSize >= _bounds.Height)
				{
					if (allOthersFullHeight && _contentSize == _bounds.Height)
					{
						if (availableHeight > minExtent)
						{
							return availableHeight;
						}
					}
					else if (_bounds.Height - othersPreferredHeight > minExtent)
					{
						return _bounds.Height - othersPreferredHeight;
					}
				}
			}
			return minExtent;
		}

		internal bool AllowsSharedRow(MxCommandBar uiCommandBar)
		{
			if (uiCommandBar.IsFullRowLayout())
			{
				return false;
			}
			if (_bars.Count > 0 && _bars[0].IsFullRowLayout())
			{
				return false;
			}
			return true;
		}

		internal void ResetBarPositions()
		{
			if (IsContentFull())
			{
				_canResolveOverlap = false;
				ArrayList barsInOrder = _bars.GetSortedBars(_isHorizontal, false);
				int nextPosition = 0;
				foreach (MxCommandBar item in barsInOrder)
				{
					if (item.Visible)
					{
						item.RecalculateLayout();
					}
				}
				foreach (MxCommandBar positionedBar in barsInOrder)
				{
					if (_isHorizontal)
					{
						positionedBar.MoveToLeft(nextPosition);
						nextPosition += positionedBar.Width;
					}
					else
					{
						positionedBar.MoveToTop(nextPosition);
						nextPosition += positionedBar.Height;
					}
				}
				_canResolveOverlap = true;
				return;
			}
			ArrayList bars = _bars.GetSortedBars(_isHorizontal, false);
			foreach (MxCommandBar visibleBar in bars)
			{
				if (visibleBar.Visible)
				{
					visibleBar.RecalculateLayout(true);
				}
			}
			if (_isHorizontal)
			{
				foreach (MxCommandBar horizontalBar in bars)
				{
					horizontalBar.MoveToLeft(horizontalBar.GetPosition().X);
				}
				return;
			}
			foreach (MxCommandBar verticalBar in bars)
			{
				verticalBar.MoveToTop(verticalBar.GetPosition().Y);
			}
		}

		internal void ResolveOverlapHorizontal(MxCommandBar uiCommandBar, int shift)
		{
			if (!_canResolveOverlap)
			{
				return;
			}
			uiCommandBar._canBeShifted = false;
			ArrayList arrayList = _bars.GetSortedBars(_isHorizontal, false);
			foreach (MxCommandBar item in arrayList)
			{
				if (!item.Visible || item.GetHostRow() != this || !item._canBeShifted || item == uiCommandBar)
				{
					continue;
				}
				if (shift < 0)
				{
					Rectangle overlapOnShrink = Rectangle.Intersect(item.Bounds, uiCommandBar.Bounds);
					if (overlapOnShrink.Width > 0 && overlapOnShrink.Height > 0)
					{
						_isLayoutEnabled = false;
						if (item.GetPosition().X <= uiCommandBar.GetPosition().X && uiCommandBar.Left > 0)
						{
							item.MoveToLeft(uiCommandBar.Left - item.Width);
						}
						else
						{
							item.MoveToLeft(uiCommandBar.Right);
						}
						_isLayoutEnabled = true;
						if (IsContentFull())
						{
							PackBars();
						}
						uiCommandBar._canBeShifted = true;
						return;
					}
					if (IsRowFilled())
					{
						continue;
					}
					if (item.GetPosition().X < item.Location.X)
					{
						int shrinkPosition = item.Location.X + shift;
						if (shrinkPosition < item.GetPosition().X)
						{
							shrinkPosition = item.GetPosition().X;
						}
						if (shrinkPosition < uiCommandBar.Right && uiCommandBar.GetPosition().X < item.GetPosition().X)
						{
							shrinkPosition = uiCommandBar.Right;
						}
						_isLayoutEnabled = false;
						item.MoveToLeft(shrinkPosition);
						_isLayoutEnabled = true;
					}
					else if (!item.IsFullShown)
					{
						int shrinkWidth = item.Width - shift;
						if (shrinkWidth > item.GetFullLength())
						{
							shrinkWidth = item.GetFullLength();
						}
						if (item.Width < shrinkWidth)
						{
							int shrinkWidthDelta = shrinkWidth - item.Width;
							item.Width = shrinkWidth;
							ResolveOverlapHorizontal(item, shrinkWidthDelta);
							shift += shrinkWidthDelta;
						}
					}
					continue;
				}
				Rectangle overlapOnGrow = Rectangle.Intersect(item.Bounds, uiCommandBar.Bounds);
				if (overlapOnGrow.Width > 0 && overlapOnGrow.Height > 0)
				{
					_isLayoutEnabled = false;
					if (item.GetPosition().X <= uiCommandBar.GetPosition().X && uiCommandBar.Left > 0)
					{
						item.MoveToLeft(uiCommandBar.Left - item.Width);
					}
					else
					{
						item.MoveToLeft(uiCommandBar.Right);
					}
					_isLayoutEnabled = true;
					if (IsContentFull())
					{
						PackBars();
					}
					uiCommandBar._canBeShifted = true;
					return;
				}
				if (IsRowFilled() || shift <= 0)
				{
					continue;
				}
				if (item.GetPosition().X > item.Location.X)
				{
					if (item.IsFullShown)
					{
						int growPosition = item.Location.X + shift;
						if (growPosition > item.GetPosition().X)
						{
							growPosition = item.GetPosition().X;
						}
						_isLayoutEnabled = false;
						item.MoveToLeft(growPosition);
						_isLayoutEnabled = true;
						continue;
					}
					int growWidth = item.Width + shift;
					if (growWidth > item.GetFullLength())
					{
						growWidth = item.GetFullLength();
					}
					if (item.Width < growWidth)
					{
						int growWidthDelta = growWidth - item.Width;
						item.Width = growWidth;
						ResolveOverlapHorizontal(item, growWidthDelta);
						shift -= growWidthDelta;
					}
				}
				else if (!item.IsFullShown)
				{
					int growWidthAlt = item.Width + shift;
					if (growWidthAlt > item.GetFullLength())
					{
						growWidthAlt = item.GetFullLength();
					}
					if (item.Width < growWidthAlt)
					{
						int growWidthDeltaAlt = growWidthAlt - item.Width;
						item.Width = growWidthAlt;
						ResolveOverlapHorizontal(item, growWidthDeltaAlt);
						shift -= growWidthDeltaAlt;
					}
				}
			}
			uiCommandBar._canBeShifted = true;
		}

		internal void ResolveOverlapVertical(MxCommandBar uiCommandBar, int shift)
		{
			if (!_canResolveOverlap)
			{
				return;
			}
			uiCommandBar._canBeShifted = false;
			ArrayList arrayList = _bars.GetSortedBars(_isHorizontal, false);
			foreach (MxCommandBar item in arrayList)
			{
				if (!item.Visible || item.GetHostRow() != this || !item._canBeShifted || item == uiCommandBar)
				{
					continue;
				}
				if (shift < 0)
				{
					Rectangle overlapOnShrink = Rectangle.Intersect(item.Bounds, uiCommandBar.Bounds);
					if (overlapOnShrink.Width > 0 && overlapOnShrink.Height > 0)
					{
						_isLayoutEnabled = false;
						if (item.GetPosition().Y <= uiCommandBar.GetPosition().Y && uiCommandBar.Top > 0)
						{
							item.MoveToTop(uiCommandBar.Top - item.Height);
						}
						else
						{
							item.MoveToTop(uiCommandBar.Bottom);
						}
						_isLayoutEnabled = true;
						if (IsContentFull())
						{
							PackBars();
						}
						uiCommandBar._canBeShifted = true;
						return;
					}
					if (IsRowFilled())
					{
						continue;
					}
					if (item.GetPosition().Y < item.Location.Y)
					{
						int shrinkPosition = item.Location.Y + shift;
						if (shrinkPosition < item.GetPosition().Y)
						{
							shrinkPosition = item.GetPosition().Y;
						}
						if (shrinkPosition < uiCommandBar.Bottom && uiCommandBar.GetPosition().Y < item.GetPosition().Y)
						{
							shrinkPosition = uiCommandBar.Bottom;
						}
						_isLayoutEnabled = false;
						item.MoveToTop(shrinkPosition);
						_isLayoutEnabled = true;
					}
					else if (!item.IsFullShown)
					{
						int shrinkHeight = item.Height - shift;
						if (shrinkHeight > item.GetFullLength())
						{
							shrinkHeight = item.GetFullLength();
						}
						if (item.Height < shrinkHeight)
						{
							int shrinkHeightDelta = shrinkHeight - item.Height;
							item.Height = shrinkHeight;
							ResolveOverlapVertical(item, shrinkHeightDelta);
							shift += shrinkHeightDelta;
						}
					}
					continue;
				}
				Rectangle overlapOnGrow = Rectangle.Intersect(item.Bounds, uiCommandBar.Bounds);
				if (overlapOnGrow.Width > 0 && overlapOnGrow.Height > 0)
				{
					_isLayoutEnabled = false;
					if (item.GetPosition().Y <= uiCommandBar.GetPosition().Y && uiCommandBar.Top > 0)
					{
						item.MoveToTop(uiCommandBar.Top - item.Height);
					}
					else
					{
						item.MoveToTop(uiCommandBar.Bottom);
					}
					_isLayoutEnabled = true;
					if (IsContentFull())
					{
						PackBars();
					}
					uiCommandBar._canBeShifted = true;
					return;
				}
				if (IsRowFilled() || shift <= 0)
				{
					continue;
				}
				if (item.GetPosition().Y > item.Location.Y)
				{
					if (item.IsFullShown)
					{
						int growPosition = item.Location.Y + shift;
						if (growPosition > item.GetPosition().Y)
						{
							growPosition = item.GetPosition().Y;
						}
						_isLayoutEnabled = false;
						item.MoveToTop(growPosition);
						_isLayoutEnabled = true;
						continue;
					}
					int growHeight = item.Height + shift;
					if (growHeight > item.GetFullLength())
					{
						growHeight = item.GetFullLength();
					}
					if (item.Height < growHeight)
					{
						int growHeightDelta = growHeight - item.Height;
						item.Height = growHeight;
						ResolveOverlapVertical(item, growHeightDelta);
						shift -= growHeightDelta;
					}
				}
				else if (!item.IsFullShown)
				{
					int growHeightAlt = item.Height + shift;
					if (growHeightAlt > item.GetFullLength())
					{
						growHeightAlt = item.GetFullLength();
					}
					if (item.Height < growHeightAlt)
					{
						int growHeightDeltaAlt = growHeightAlt - item.Height;
						item.Height = growHeightAlt;
						ResolveOverlapVertical(item, growHeightDeltaAlt);
						shift -= growHeightDeltaAlt;
					}
				}
			}
			uiCommandBar._canBeShifted = true;
		}
	}
}
