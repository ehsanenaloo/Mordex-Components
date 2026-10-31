using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimeline
	{
		protected virtual void OnVerticalScrollPositionChanged(EventArgs e)
		{
			if (base.Events[_verticalScrollPositionChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		private void ScrollByWheel(int wheelDelta)
		{
			if (!base.Created || _editingItem != null || _isDropDownOpen)
			{
				return;
			}
			base.ScrollByWheelDelta(wheelDelta);
			if (_isHorizontalWheelScroll)
			{
				int direction = 0;
				if (wheelDelta < 0)
				{
					direction = 1;
				}
				else if (wheelDelta > 0)
				{
					direction = -1;
				}
				DateTime firstDate = AddIntervalUnits(_firstDate, direction);
				if (firstDate < MinDate)
				{
					firstDate = MinDate;
				}
				if (firstDate.Add(LastDate.Subtract(FirstDate)) > MaxDate)
				{
					firstDate = MaxDate.Subtract(LastDate.Subtract(FirstDate));
				}
				FirstDate = firstDate;
				GetHorizontalScrollBarControl().ClampScrollValue();
				_steps = GetHorizontalScrollBarControl().GetScrollValue();
			}
		}

		public void ScrollUp(int steps)
		{
			if (GetVerticalScrollBarControl().Visible)
			{
				StyledScrollBar verticalScrollBar = GetVerticalScrollBarControl();
				verticalScrollBar.UpdateScrollValue(verticalScrollBar.GetScrollValue() - steps);
			}
		}

		public void ScrollDown(int steps)
		{
			if (GetVerticalScrollBarControl().Visible)
			{
				StyledScrollBar verticalScrollBar = GetVerticalScrollBarControl();
				verticalScrollBar.UpdateScrollValue(verticalScrollBar.GetScrollValue() + steps);
			}
		}

		public void ScrollRight(int steps)
		{
			if (GetHorizontalScrollBarControl().Visible)
			{
				StyledScrollBar horizontalScrollBar = GetHorizontalScrollBarControl();
				horizontalScrollBar.UpdateScrollValue(horizontalScrollBar.GetScrollValue() + steps);
				_steps -= steps;
				OnHorizontalScroll(ScrollEventType.ThumbTrack);
			}
		}

		public void ScrollLeft(int steps)
		{
			if (GetHorizontalScrollBarControl().Visible)
			{
				StyledScrollBar horizontalScrollBar = GetHorizontalScrollBarControl();
				horizontalScrollBar.UpdateScrollValue(horizontalScrollBar.GetScrollValue() - steps);
				_steps += steps;
				OnHorizontalScroll(ScrollEventType.ThumbTrack);
			}
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			if (!TryCommitItemEdit())
			{
				switch (scrollCode)
				{
				case 7:
				case 8:
				case 12:
					GetHorizontalScrollBarControl().SetScrollValueWithoutNotify(_steps);
					return;
				}
				int firstVisibleRowIndex = 0;
				if (_firstVisibleRowLayout != null)
				{
					firstVisibleRowIndex = _firstVisibleRowLayout.GetRowIndex();
				}
				GetVerticalScrollBarControl().SetScrollValueWithoutNotify(firstVisibleRowIndex);
				OnVerticalScrollPositionChanged(EventArgs.Empty);
			}
			else if (scrollCode > 6)
			{
				switch (scrollCode)
				{
				case 7:
					if (_shouldClampScrollValue)
					{
						if (FirstDate == MinDate)
						{
							GetHorizontalScrollBarControl()._scrollValue = GetHorizontalScrollBarControl().GetMinimum();
							GetHorizontalScrollBarControl().RefreshThumbLayout();
						}
						else if (GetHorizontalScrollBarControl().GetScrollValue() == GetHorizontalScrollBarControl().GetMinimum())
						{
							GetHorizontalScrollBarControl()._scrollValue = GetHorizontalScrollBarControl().GetMinimum() + GetHorizontalScrollBarControl().GetLargeChange();
							if (GetHorizontalScrollBarControl()._scrollValue > GetHorizontalScrollBarControl().GetMaxScrollValue())
							{
								GetHorizontalScrollBarControl()._scrollValue = GetHorizontalScrollBarControl().GetMaxScrollValue();
							}
							GetHorizontalScrollBarControl().RefreshThumbLayout();
						}
					}
					_steps = GetHorizontalScrollBarControl().GetScrollValue();
					if (_shouldClampScrollValue)
					{
						if (LastDate >= MaxDate.AddMinutes(-1.0))
						{
							GetHorizontalScrollBarControl()._scrollValue = GetHorizontalScrollBarControl().GetMaxScrollValue();
							GetHorizontalScrollBarControl().RefreshThumbLayout();
						}
						else if (GetHorizontalScrollBarControl().GetScrollValue() == GetHorizontalScrollBarControl().GetMaxScrollValue())
						{
							GetHorizontalScrollBarControl()._scrollValue = GetHorizontalScrollBarControl().GetMaxScrollValue() - GetHorizontalScrollBarControl().GetLargeChange();
							if (GetHorizontalScrollBarControl()._scrollValue < GetHorizontalScrollBarControl().GetMinimum())
							{
								GetHorizontalScrollBarControl()._scrollValue = GetHorizontalScrollBarControl().GetMinimum();
							}
							GetHorizontalScrollBarControl().RefreshThumbLayout();
						}
					}
					_steps = GetHorizontalScrollBarControl().GetScrollValue();
					break;
				case 8:
					break;
				}
			}
			else if (_firstVisibleRowLayout != null)
			{
				_firstVisibleRowLayout.GetRowIndex();
				GetVerticalScrollBarControl().GetScrollValue();
				_ = RowHeight;
				if (GetVerticalScrollBarControl().GetScrollValue() < _rowLayouts.Count)
				{
					_firstVisibleRowLayout = (MxRowLayoutState)_rowLayouts[GetVerticalScrollBarControl().GetScrollValue()];
				}
				else
				{
					_firstVisibleRowLayout = null;
				}
				OnVerticalScrollPositionChanged(EventArgs.Empty);
				LayoutRows();
				Invalidate();
			}
		}

		protected internal override void OnScrollFindNext(EventArgs e)
		{
		}

		protected internal override void OnScrollFindPrevious(EventArgs e)
		{
		}

		protected internal override void OnHorizontalScroll(ScrollEventType eventType)
		{
			int scrollSteps = 0;
			int stepWidth = 0;
			DateTime firstDate = FirstDate;
			switch (eventType)
			{
			case ScrollEventType.SmallDecrement:
				scrollSteps = -1;
				stepWidth = _intervalWidth;
				break;
			case ScrollEventType.SmallIncrement:
				scrollSteps = 1;
				stepWidth = _intervalWidth;
				break;
			case ScrollEventType.LargeDecrement:
				scrollSteps = -GetHorizontalScrollBarControl().GetLargeChange() / GetHorizontalScrollBarControl().GetValueScale();
				stepWidth = _intervalWidth;
				break;
			case ScrollEventType.LargeIncrement:
				scrollSteps = GetHorizontalScrollBarControl().GetLargeChange() / GetHorizontalScrollBarControl().GetValueScale();
				stepWidth = _intervalWidth;
				break;
			case ScrollEventType.ThumbPosition:
				stepWidth = _intervalWidth;
				break;
			case ScrollEventType.EndScroll:
				stepWidth = _intervalWidth;
				break;
			case ScrollEventType.ThumbTrack:
				scrollSteps = GetHorizontalScrollBarControl().GetScrollValue() - _steps;
				stepWidth = 8;
				break;
			}
			if (BottomTier.Interval == TimelineInterval.Year)
			{
				if (stepWidth % _intervalWidth == 0)
				{
					int yearsToShift = scrollSteps * stepWidth / _intervalWidth;
					firstDate = _firstDate.AddYears(yearsToShift * BottomTier.Count);
				}
				else
				{
					float yearDaysToShift = (float)scrollSteps * ((float)stepWidth / (float)_intervalWidth);
					yearDaysToShift *= (float)GetDaysInYear(_firstDate.Year);
					_steps = GetHorizontalScrollBarControl().GetScrollValue();
					firstDate = _firstDate.AddDays(yearDaysToShift * (float)BottomTier.Count);
				}
			}
			else if (BottomTier.Interval == TimelineInterval.Month)
			{
				if (stepWidth % _intervalWidth == 0)
				{
					int monthsToShift = scrollSteps * stepWidth / _intervalWidth;
					firstDate = _firstDate.AddMonths(monthsToShift * BottomTier.Count);
				}
				else
				{
					float monthDaysToShift = (float)scrollSteps * ((float)stepWidth / (float)_intervalWidth);
					monthDaysToShift *= (float)DateTime.DaysInMonth(_firstDate.Year, _firstDate.Month);
					_steps = GetHorizontalScrollBarControl().GetScrollValue();
					firstDate = _firstDate.AddDays(monthDaysToShift * (float)BottomTier.Count);
				}
			}
			else
			{
				float secondsToShift = (float)scrollSteps * ((float)stepWidth / (float)_intervalWidth * (float)_intervalSeconds);
				firstDate = _firstDate.AddSeconds((int)Math.Ceiling(secondsToShift));
				if (firstDate != FirstDate)
				{
					_steps = GetHorizontalScrollBarControl().GetScrollValue();
				}
			}
			if (firstDate < MinDate)
			{
				firstDate = MinDate;
			}
			if (firstDate.Add(LastDate.Subtract(FirstDate)) > MaxDate)
			{
				firstDate = MaxDate.Subtract(LastDate.Subtract(FirstDate));
			}
			FirstDate = firstDate;
		}

		private void AutoScrollNearEdge(Point pt)
		{
			bool isNearEdge = false;
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Width = base.Bounds.Width;
			clientRectangle.Height = base.Bounds.Height;
			if (clientRectangle.Contains(pt))
			{
				if (pt.X >= base.ClientRectangle.Right - 20)
				{
					_isAutoScrollForward = true;
					isNearEdge = true;
					_isAutoScrollVertical = false;
				}
				else if (pt.X <= GetHeaderAreaRight() || (GetHeaderAreaRight() == base.ClientRectangle.Left && pt.X <= GetHeaderAreaRight() + 20))
				{
					_isAutoScrollForward = false;
					isNearEdge = true;
					_isAutoScrollVertical = false;
				}
				else if (pt.Y + 20 >= base.ClientRectangle.Bottom && _itemDragType == ItemDragType.Move)
				{
					_isAutoScrollForward = true;
					isNearEdge = true;
					_isAutoScrollVertical = true;
				}
				else if (pt.Y <= GetHeaderHeight() + base.ClientRectangle.Top + 1 && _itemDragType == ItemDragType.Move)
				{
					_isAutoScrollForward = false;
					isNearEdge = true;
					_isAutoScrollVertical = true;
				}
			}
			if (isNearEdge)
			{
				if (!_isAutoScrolling)
				{
					_isAutoScrolling = true;
					_lastAutoScrollTicks = DateTime.Now.Ticks;
					return;
				}
				long elapsedTicks = DateTime.Now.Ticks - _lastAutoScrollTicks;
				if (elapsedTicks / 10000 >= 50)
				{
					UpdateTimeline();
					_lastAutoScrollTicks = DateTime.Now.Ticks;
				}
			}
			else
			{
				_isAutoScrolling = false;
			}
		}
	}
}
