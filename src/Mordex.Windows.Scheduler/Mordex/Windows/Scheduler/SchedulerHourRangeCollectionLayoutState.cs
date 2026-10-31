using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerHourRangeCollectionLayoutState
	{
		public SchedulerAppointmentOwnerLayoutState _schedulerAppointmentOwnerLayoutState;

		public SchedulerAppointmentOwnerLayoutState _hiddenBelowButton;

		public SchedulerAppointmentOwnerLayoutState _hiddenAboveButton;

		private SchedulerHourRangeCollection _schedulerHourRangeCollection;

		private SchedulerAppointmentOwner _owner;

		private int _laneCount = -1;

		private SchedulerFormatStyle _schedulerFormatStyle;

		private int _columnIndex = -1;

		private string _caption = string.Empty;

		private int _dayIndex;

		private bool _hasHiddenAbove;

		private bool _hasHiddenBelow;

		private bool _hasAllDayRowsAbove;

		private bool _hasAllDayRowsBelow;

		private bool _hasHiddenAppointments;

		private DateTime _date;

		private bool _isDateInRange = true;

		public Rectangle _bounds = Rectangle.Empty;

		private SchedulerOwnerLayoutCollection _timeSlots = new SchedulerOwnerLayoutCollection();

		internal SchedulerHourRangeCollectionLayoutState(DateTime date)
		{
			_date = date;
		}

		internal SchedulerHourRangeCollectionLayoutState(DateTime date, SchedulerAppointmentOwner owner)
		{
			_date = date;
			_owner = owner;
		}

		internal SchedulerHourRangeCollectionLayoutState(DateTime date, Rectangle bounds, string caption)
		{
			_date = date;
			_bounds = bounds;
			_caption = caption;
		}

		internal SchedulerHourRangeCollectionLayoutState(DateTime date, Rectangle bounds)
		{
			_date = date;
			_bounds = bounds;
		}

		public SchedulerHourRangeCollection GetSchedulerHourRangeCollection()
		{
			return _schedulerHourRangeCollection;
		}

		public void SetSchedulerHourRangeCollection(SchedulerHourRangeCollection schedulerHourRangeCollection)
		{
			_schedulerHourRangeCollection = schedulerHourRangeCollection;
		}

		public SchedulerAppointmentOwner GetOwner()
		{
			return _owner;
		}

		public void SetOwner(SchedulerAppointmentOwner owner)
		{
			_owner = owner;
		}

		internal int GetLaneCount()
		{
			return _laneCount;
		}

		internal void SetLaneCount(int laneCount)
		{
			_laneCount = laneCount;
		}

		public SchedulerFormatStyle GetSchedulerFormatStyle()
		{
			return _schedulerFormatStyle;
		}

		public void SetSchedulerFormatStyle(SchedulerFormatStyle schedulerFormatStyle)
		{
			_schedulerFormatStyle = schedulerFormatStyle;
		}

		public int GetColumnIndex()
		{
			return _columnIndex;
		}

		public void SetColumnIndex(int schedulerHourRangeCollectionLayoutStateInteger)
		{
			_columnIndex = schedulerHourRangeCollectionLayoutStateInteger;
		}

		public string GetCaption()
		{
			return _caption;
		}

		public void UpdateCaption(string caption)
		{
			if (_caption != caption)
			{
				_caption = caption;
			}
		}

		public int GetDayIndex()
		{
			return _dayIndex;
		}

		public void UpdateDayIndex(int dayIndex)
		{
			if (_dayIndex != dayIndex)
			{
				_dayIndex = dayIndex;
			}
		}

		public bool GetHasHiddenAbove()
		{
			return _hasHiddenAbove;
		}

		public void SetHasHiddenAbove(bool hasHiddenAbove)
		{
			_hasHiddenAbove = hasHiddenAbove;
		}

		public bool GetHasHiddenBelow()
		{
			return _hasHiddenBelow;
		}

		public void SetHasHiddenBelow(bool schedulerHourRangeCollectionLayoutStateBoolean)
		{
			_hasHiddenBelow = schedulerHourRangeCollectionLayoutStateBoolean;
		}

		public bool GetHasAllDayRowsAbove()
		{
			return _hasAllDayRowsAbove;
		}

		public void SetHasAllDayRowsAbove(bool hasAllDayRowsAbove)
		{
			_hasAllDayRowsAbove = hasAllDayRowsAbove;
		}

		public bool GetHasAllDayRowsBelow()
		{
			return _hasAllDayRowsBelow;
		}

		public void SetHasAllDayRowsBelow(bool hasAllDayRowsBelow)
		{
			_hasAllDayRowsBelow = hasAllDayRowsBelow;
		}

		public bool GetHasHiddenAppointments()
		{
			return _hasHiddenAppointments;
		}

		public void SetHasHiddenAppointments(bool hasHiddenAppointments)
		{
			_hasHiddenAppointments = hasHiddenAppointments;
		}

		public DateTime GetDate()
		{
			return _date;
		}

		public bool GetIsDateInRange()
		{
			return _isDateInRange;
		}

		public void SetIsDateInRange(bool isDateInRange)
		{
			_isDateInRange = isDateInRange;
		}

		public SchedulerOwnerLayoutCollection GetTimeSlots()
		{
			return _timeSlots;
		}

		internal bool HandleMouseDown(MouseEventArgs eventArgs, MxScheduler scheduler)
		{
			if (_hiddenAboveButton != null && _hiddenAboveButton._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				scheduler.ExtendVisibleTimeToDayStart(this);
				return true;
			}
			if (_hiddenBelowButton != null && _hiddenBelowButton._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				scheduler.ExtendVisibleTimeToDayEnd(this);
				return true;
			}
			if (scheduler.IsViewNavigationEnabled() && _schedulerAppointmentOwnerLayoutState != null && _schedulerAppointmentOwnerLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				scheduler.SetPressedWeekHeader(_schedulerAppointmentOwnerLayoutState);
				return true;
			}
			return false;
		}

		internal bool HandleMouseMove(MouseEventArgs eventArgs, MxScheduler scheduler)
		{
			if (_hiddenAboveButton != null && _hiddenAboveButton._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				scheduler.UpdateSchedulerAppointmentOwnerLayoutState(_hiddenAboveButton);
				return true;
			}
			if (_hiddenBelowButton != null && _hiddenBelowButton._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				scheduler.UpdateSchedulerAppointmentOwnerLayoutState(_hiddenBelowButton);
				return true;
			}
			if (scheduler.IsViewNavigationEnabled() && _schedulerAppointmentOwnerLayoutState != null && _schedulerAppointmentOwnerLayoutState._bounds.Contains(eventArgs.X, eventArgs.Y))
			{
				scheduler.UpdateSchedulerAppointmentOwnerLayoutState(null);
				scheduler.UpdateSchedulerSchedulerAppointmentOwnerLayoutState(_schedulerAppointmentOwnerLayoutState);
				return true;
			}
			return false;
		}
	}
}
