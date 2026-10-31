using System;
using System.Drawing;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerAppointmentOwnerLayoutState
	{
		public DateRange _dateRange;

		private int _imageIndex = -1;

		private int _columnIndex = -1;

		private DayOfWeek _dayOfWeek;

		private SchedulerAppointmentOwner _schedulerAppointmentOwner;

		private SchedulerHourRangeCollectionLayoutState _schedulerHourRangeCollectionLayoutState;

		private string _text = string.Empty;

		private bool _isHovered;

		private bool _isPressed;

		public Rectangle _bounds = Rectangle.Empty;

		private int _index = -1;

		private bool _isWorkingHour;

		private DateTime _date;

		private Color _color;

		internal SchedulerAppointmentOwnerLayoutState(DateTime date, Rectangle bounds)
		{
			_date = date;
			_bounds = bounds;
		}

		internal SchedulerAppointmentOwnerLayoutState(int index, Rectangle bounds)
		{
			_index = index;
			_bounds = bounds;
			_color = Color.Empty;
		}

		internal SchedulerAppointmentOwnerLayoutState(Rectangle rect, string text)
		{
			_bounds = rect;
			_text = text;
		}

		public int GetImageIndex()
		{
			return _imageIndex;
		}

		public void SetImageIndex(int imageIndex)
		{
			_imageIndex = imageIndex;
		}

		public int GetColumnIndex()
		{
			return _columnIndex;
		}

		public void SetColumnIndex(int schedulerAppointmentOwnerLayoutStateInteger)
		{
			_columnIndex = schedulerAppointmentOwnerLayoutStateInteger;
		}

		public DayOfWeek GetDayOfWeek()
		{
			return _dayOfWeek;
		}

		public void SetDayOfWeek(DayOfWeek dayOfWeek)
		{
			_dayOfWeek = dayOfWeek;
		}

		public SchedulerAppointmentOwner ResolveOwner()
		{
			if (_schedulerAppointmentOwner != null)
			{
				return _schedulerAppointmentOwner;
			}
			if (_schedulerHourRangeCollectionLayoutState != null)
			{
				return _schedulerHourRangeCollectionLayoutState.GetOwner();
			}
			return null;
		}

		public void SetSchedulerAppointmentOwner(SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			_schedulerAppointmentOwner = schedulerAppointmentOwner;
		}

		public SchedulerHourRangeCollectionLayoutState GetSchedulerHourRangeCollectionLayoutState()
		{
			return _schedulerHourRangeCollectionLayoutState;
		}

		public void SetSchedulerHourRangeCollectionLayoutState(SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			_schedulerHourRangeCollectionLayoutState = schedulerHourRangeCollectionLayoutState;
		}

		public string GetHeaderText()
		{
			return _text;
		}

		public void SetHeaderText(string text)
		{
			_text = text;
		}

		public bool IsHovered()
		{
			return _isHovered;
		}

		public void SetHovered(bool isHovered)
		{
			if (_isHovered != isHovered)
			{
				_isHovered = isHovered;
			}
		}

		public void SetPressed(bool schedulerAppointmentOwnerLayoutStateBoolean)
		{
			_isPressed = schedulerAppointmentOwnerLayoutStateBoolean;
		}

		public int GetIndex()
		{
			return _index;
		}

		public void SetIndex(int index)
		{
			_index = index;
		}

		public bool IsWorkingHour()
		{
			return _isWorkingHour;
		}

		public void SetWorkingHour(bool isWorkingHour)
		{
			_isWorkingHour = isWorkingHour;
		}

		public DateTime GetDate()
		{
			return _date;
		}

		public void UpdateDate(DateTime date)
		{
			if (_date != date)
			{
				_date = date;
			}
		}
	}
}
