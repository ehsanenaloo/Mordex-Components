using System;
using System.Drawing;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerFormatStyleLayoutState
	{
		private DateTime _date;

		private bool _isOutsideMonth;

		private bool _isWithinDateLimits = true;

		private bool _isVisible;

		private Rectangle _bounds;

		private string _text;

		private bool _isBold;

		private bool _isSelected;

		private SchedulerFormatStyle _schedulerFormatStyle;

		internal SchedulerFormatStyleLayoutState()
		{
		}

		internal DateTime GetDate()
		{
			return _date;
		}

		internal void SetDate(DateTime dateTime)
		{
			_date = dateTime;
		}

		internal bool IsOutsideMonth()
		{
			return _isOutsideMonth;
		}

		internal void SetOutsideMonth(bool isOutsideMonth)
		{
			_isOutsideMonth = isOutsideMonth;
		}

		internal bool IsWithinDateLimits()
		{
			return _isWithinDateLimits;
		}

		internal void SetWithinDateLimits(bool schedulerFormatStyleLayoutStateBoolean)
		{
			_isWithinDateLimits = schedulerFormatStyleLayoutStateBoolean;
		}

		internal bool IsVisible()
		{
			return _isVisible;
		}

		internal void SetVisible(bool isVisible)
		{
			_isVisible = isVisible;
		}

		internal Rectangle GetBounds()
		{
			return _bounds;
		}

		internal void SetBounds(Rectangle rectangle)
		{
			_bounds = rectangle;
		}

		internal string GetText()
		{
			return _text;
		}

		internal void SetText(string text)
		{
			_text = text;
		}

		internal bool IsBold()
		{
			return _isBold;
		}

		internal void SetBold(bool isBold)
		{
			_isBold = isBold;
		}

		internal bool IsSelected()
		{
			return _isSelected;
		}

		internal void SetSelected(bool isSelected)
		{
			_isSelected = isSelected;
		}

		public SchedulerFormatStyle GetSchedulerFormatStyle()
		{
			return _schedulerFormatStyle;
		}

		public void SetSchedulerFormatStyle(SchedulerFormatStyle schedulerFormatStyle)
		{
			_schedulerFormatStyle = schedulerFormatStyle;
		}
	}
}
