using System;
using System.Drawing;

namespace Mordex.Windows.Timeline
{
	internal class TimelineCalendarDay
	{
		private DateTime _date;

		private bool _isOutsideMonth;

		private bool _isInRange = true;

		private bool _isVisible;

		private Rectangle _bounds;

		private string _text;

		private bool _isBold;

		private bool _isSelected;

		internal TimelineCalendarDay()
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

		internal bool IsInRange()
		{
			return _isInRange;
		}

		internal void SetInRange(bool timelineLayoutState2Boolean)
		{
			_isInRange = timelineLayoutState2Boolean;
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

		internal string GetDayText()
		{
			return _text;
		}

		internal void SetDayText(string text)
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

		internal void SetSelected(bool isSelected)
		{
			_isSelected = isSelected;
		}
	}
}
