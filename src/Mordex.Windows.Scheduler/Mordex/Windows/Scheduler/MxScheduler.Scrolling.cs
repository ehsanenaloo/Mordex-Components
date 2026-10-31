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
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		private void ShowScrollDateLabel()
		{
			int dayOffset = (GetVerticalScrollBarControl().GetScrollValue() - _verticalScrollPosition) * 7;
			DateTime dateTime;
			try
			{
				dateTime = _date.AddDays(dayOffset);
			}
			catch
			{
				dateTime = _date;
			}
			if (_scrollDateLabel == null)
			{
				_scrollDateLabel = new SchedulerHiddenAppointmentButton(this, dateTime, -1, HiddenAppointmentMode.Manual);
				Point position = Cursor.Position;
				Graphics graphics = CreateGraphics();
				position = PointToClient(position);
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
				Size size = textLayoutRenderer.RenderContent(graphics, dateTime.ToShortDateString(), Font, 0).ToSize();
				textLayoutRenderer.ResetFormat();
				size.Width += 2;
				size.Height += 2;
				Rectangle bounds = new Rectangle(GetClientRight() - size.Width - 5, position.Y, size.Width, size.Height);
				_scrollDateLabel.Bounds = bounds;
				_scrollDateLabel.SetDateLabel(true);
				graphics.Dispose();
				graphics = null;
				base.Controls.Add(_scrollDateLabel);
			}
			else
			{
				_scrollDateLabel._date = dateTime;
				_scrollDateLabel.Invalidate();
			}
		}

		private void ApplyScrollBarPosition()
		{
			if (_view == SchedulerView.DayView)
			{
				ScrollDayViewRows();
				return;
			}
			int dayOffset = (GetVerticalScrollBarControl().GetScrollValue() - _verticalScrollPosition) * 7;
			int visibleDayCount = 7;
			if (_view == SchedulerView.MonthView)
			{
				visibleDayCount = _weeks * 7;
			}
			DateTime dateTime = _date.AddDays(dayOffset);
			if (dateTime.AddDays(visibleDayCount - 1) > GetWeekEnd(_maxDate))
			{
				GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
				return;
			}
			if (dateTime < GetWeekStart(_minDate))
			{
				GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
				return;
			}
			_verticalScrollPosition = GetVerticalScrollBarControl().GetScrollValue();
			_date = _date.AddDays(dayOffset);
			_isAppointmentCacheStale = true;
			RenderCurrentView(null);
			if (dayOffset != 0)
			{
				OnScroll(EventArgs.Empty);
			}
			Invalidate();
		}

		private void ScrollToPosition(int verticalScrollPosition)
		{
			if (verticalScrollPosition != _verticalScrollPosition)
			{
				int dayOffset = (verticalScrollPosition - _verticalScrollPosition) * 7;
				int visibleDayCount = 7;
				if (_view == SchedulerView.MonthView)
				{
					visibleDayCount = _weeks * 7;
				}
				DateTime dateTime = _date.AddDays(dayOffset);
				if (dateTime.AddDays(visibleDayCount - 1) > GetWeekEnd(_maxDate))
				{
					GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
					return;
				}
				if (dateTime < GetWeekStart(_minDate))
				{
					GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
					return;
				}
				_verticalScrollPosition = verticalScrollPosition;
				_date = dateTime;
				_isAppointmentCacheStale = true;
				RenderCurrentView(null);
				OnScroll(EventArgs.Empty);
				Invalidate();
			}
			if (_scrollDateLabel != null)
			{
				_scrollDateLabel.Dispose();
				_scrollDateLabel = null;
			}
		}

		protected internal override void OnScrolling(CancelEventArgs e)
		{
			if (!TryCommitEditor())
			{
				e.Cancel = true;
			}
		}

		protected internal override void OnVerticalScroll(ScrollEventType eventType)
		{
			if (_view != SchedulerView.DayView && TryCommitEditor())
			{
				switch (eventType)
				{
				case ScrollEventType.SmallDecrement:
					ScrollToPosition(_verticalScrollPosition - 1);
					break;
				case ScrollEventType.SmallIncrement:
					ScrollToPosition(_verticalScrollPosition + 1);
					break;
				case ScrollEventType.LargeDecrement:
					ScrollToPosition(_verticalScrollPosition - GetVerticalScrollBarControl().GetLargeChange());
					break;
				case ScrollEventType.LargeIncrement:
					ScrollToPosition(_verticalScrollPosition + GetVerticalScrollBarControl().GetLargeChange());
					break;
				case ScrollEventType.ThumbPosition:
					ScrollToPosition(GetVerticalScrollBarControl().GetScrollValue());
					break;
				case ScrollEventType.EndScroll:
					ScrollToPosition(GetVerticalScrollBarControl().GetScrollValue());
					break;
				}
				if (_verticalScrollPosition != GetVerticalScrollBarControl().GetScrollValue() && _continuousScroll)
				{
					_verticalScrollPosition = GetVerticalScrollBarControl().GetScrollValue();
				}
			}
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			if (!TryCommitEditor())
			{
				switch (scrollCode)
				{
				case 7:
				case 8:
				case 12:
					GetHorizontalScrollBarControl().SetScrollValueWithoutNotify(_firstVisibleColumnIndex);
					return;
				}
				if (_view != SchedulerView.DayView)
				{
					GetVerticalScrollBarControl().SetScrollValueWithoutNotify(_verticalScrollPosition);
					return;
				}
				int firstRowIndex = 0;
				if (_firstVisibleSlot != null)
				{
					firstRowIndex = _firstVisibleSlot.GetIndex();
				}
				GetVerticalScrollBarControl().SetScrollValueWithoutNotify(firstRowIndex);
				return;
			}
			switch (scrollCode)
			{
			case 7:
			case 8:
			case 12:
				_firstVisibleColumnIndex = GetHorizontalScrollBarControl().GetScrollValue();
				if (_firstVisibleColumnIndex < 0)
				{
					_firstVisibleColumnIndex = 0;
				}
				InvalidateAppointmentLayout();
				OnScroll(EventArgs.Empty);
				break;
			case 0:
				if (_view == SchedulerView.DayView)
				{
					ScrollDayViewRows();
				}
				else
				{
					GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
				}
				break;
			case 1:
				if (_view != SchedulerView.DayView && !_continuousScroll)
				{
					ShowScrollDateLabel();
				}
				if (_continuousScroll)
				{
					ApplyScrollBarPosition();
				}
				break;
			}
		}

		private void OnAutoScrollTimerTick(object sender, EventArgs eventArgs)
		{
			if (_view != SchedulerView.DayView)
			{
				ShowOwnerListPopup();
			}
			else if (_isEdgeScrollVertical)
			{
				if (_isEdgeScrollForward)
				{
					GetVerticalScrollBarControl().UpdateScrollValue(GetVerticalScrollBarControl().GetScrollValue() + 1);
				}
				else
				{
					GetVerticalScrollBarControl().UpdateScrollValue(GetVerticalScrollBarControl().GetScrollValue() - 1);
				}
			}
			else if (_isEdgeScrollForward)
			{
				GetHorizontalScrollBarControl().UpdateScrollValue(GetHorizontalScrollBarControl().GetScrollValue() + 1);
			}
			else
			{
				GetHorizontalScrollBarControl().UpdateScrollValue(GetHorizontalScrollBarControl().GetScrollValue() - 1);
			}
		}

		private void StopAutoScrollTimer()
		{
			if (_edgeScrollTimer != null)
			{
				_edgeScrollTimer.Enabled = false;
				_edgeScrollTimer.Tick -= OnAutoScrollTimerTick;
				_edgeScrollTimer.Dispose();
				_edgeScrollTimer = null;
			}
		}

		protected virtual void OnScroll(EventArgs e)
		{
			if (base.Events[_scrollEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}
	}
}
