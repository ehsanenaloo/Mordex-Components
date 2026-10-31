using System;
using System.Drawing;

namespace Mordex.Windows.DatePicker
{
	internal class CalendarCell
	{
		private DateTime _date;

		private bool _isAdjacentMonth;

		private bool _isSelectable = true;

		private bool _isVisible;

		private Rectangle _bounds;

		private string _text;

		private bool _isBold;

		internal CalendarCell()
		{
		}

		internal DateTime GetDate()
		{
			return _date;
		}

		internal void SetDate(DateTime date)
		{
			_date = date;
		}

		internal bool GetIsAdjacentMonth()
		{
			return _isAdjacentMonth;
		}

		internal void SetIsAdjacentMonth(bool isAdjacentMonth)
		{
			_isAdjacentMonth = isAdjacentMonth;
		}

		internal bool GetIsSelectable()
		{
			return _isSelectable;
		}

		internal void SetIsSelectable(bool isSelectable)
		{
			_isSelectable = isSelectable;
		}

		internal bool GetIsVisible()
		{
			return _isVisible;
		}

		internal void SetIsVisible(bool isVisible)
		{
			_isVisible = isVisible;
		}

		internal Rectangle GetBounds()
		{
			return _bounds;
		}

		internal void SetBounds(Rectangle bounds)
		{
			_bounds = bounds;
		}

		internal string GetDayText()
		{
			return _text;
		}

		internal void SetDayText(string text)
		{
			_text = text;
		}

		internal bool GetIsBold()
		{
			return _isBold;
		}

		internal void SetIsBold(bool isBold)
		{
			_isBold = isBold;
		}
	}
}
