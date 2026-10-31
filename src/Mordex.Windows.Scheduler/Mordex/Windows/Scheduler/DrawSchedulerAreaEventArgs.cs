using System;
using System.Drawing;

namespace Mordex.Windows.Scheduler
{
	public class DrawSchedulerAreaEventArgs : EventArgs
	{
		private MxScheduler _scheduler;

		private object _itemDrawn;

		private Graphics _graphics;

		private Rectangle _bounds = Rectangle.Empty;

		private Brush _backBrush;

		private Brush _foreBrush;

		private SchedulerFormatStyle _formatStyle;

		private OwnerDrawnArea _area;

		private bool _handled;

		public Graphics Graphics => _graphics;

		public Rectangle Bounds => _bounds;

		public SchedulerAppointment Appointment
		{
			get
			{
				if (_itemDrawn is SchedulerAppointment)
				{
					return _itemDrawn as SchedulerAppointment;
				}
				return null;
			}
		}

		public SchedulerAppointmentOwner Owner
		{
			get
			{
				switch (_area)
				{
				case OwnerDrawnArea.DayViewRow:
					if (_itemDrawn is SchedulerAppointmentOwnerLayoutState)
					{
						return ((SchedulerAppointmentOwnerLayoutState)_itemDrawn).ResolveOwner();
					}
					break;
				case OwnerDrawnArea.Appointments:
					if (Appointment != null)
					{
						return _scheduler.Owners.FindOwnerByValue(Appointment.Owner);
					}
					break;
				case OwnerDrawnArea.OwnerListItem:
					if (_itemDrawn is SchedulerAppointmentOwner)
					{
						return _itemDrawn as SchedulerAppointmentOwner;
					}
					return null;
				case OwnerDrawnArea.OwnerHeader:
					if (_itemDrawn == null)
					{
						return _scheduler.CurrentOwner;
					}
					if (_itemDrawn is SchedulerAppointmentOwnerLayoutState)
					{
						return ((SchedulerAppointmentOwnerLayoutState)_itemDrawn).ResolveOwner();
					}
					break;
				}
				return null;
			}
		}

		public DateTime DateTime
		{
			get
			{
				switch (_area)
				{
				case OwnerDrawnArea.DayViewRow:
					if (_itemDrawn is SchedulerAppointmentOwnerLayoutState)
					{
						return ((SchedulerAppointmentOwnerLayoutState)_itemDrawn).GetDate();
					}
					break;
				case OwnerDrawnArea.Appointments:
					if (Appointment != null)
					{
						return Appointment.StartTime;
					}
					break;
				case OwnerDrawnArea.MonthViewDay:
				case OwnerDrawnArea.WeekViewDay:
					if (_itemDrawn is SchedulerHourRangeCollectionLayoutState)
					{
						return ((SchedulerHourRangeCollectionLayoutState)_itemDrawn).GetDate();
					}
					break;
				case OwnerDrawnArea.DateHeader:
				case OwnerDrawnArea.OwnerHeader:
					if (_itemDrawn is SchedulerAppointmentOwnerLayoutState)
					{
						return ((SchedulerAppointmentOwnerLayoutState)_itemDrawn).GetDate();
					}
					break;
				case OwnerDrawnArea.WeekHeader:
					if (_itemDrawn is SchedulerAppointmentOwnerLayoutState)
					{
						return ((SchedulerAppointmentOwnerLayoutState)_itemDrawn)._dateRange.Start;
					}
					break;
				}
				return _scheduler.Date;
			}
		}

		public Brush BackBrush
		{
			get
			{
				if (_backBrush == null)
				{
					_backBrush = _scheduler.GetAreaBackgroundBrush(_area, _itemDrawn, _bounds);
				}
				return _backBrush;
			}
		}

		public Brush ForeBrush
		{
			get
			{
				if (_foreBrush == null)
				{
					_foreBrush = _scheduler.GetAreaForeBrush(_area, _itemDrawn);
				}
				return _foreBrush;
			}
		}

		public SchedulerFormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = _scheduler.GetAreaFormatStyle(_area, _itemDrawn);
				}
				return _formatStyle;
			}
		}

		public OwnerDrawnArea OwnerDrawnArea => _area;

		public bool Handled
		{
			get
			{
				return _handled;
			}
			set
			{
				_handled = value;
			}
		}

		internal DrawSchedulerAreaEventArgs(Graphics graphics, MxScheduler scheduler, OwnerDrawnArea area, Rectangle rect, object itemDrawn)
		{
			_graphics = graphics;
			_area = area;
			_bounds = rect;
			_itemDrawn = itemDrawn;
			_scheduler = scheduler;
		}

		public void PaintBackground()
		{
			_scheduler.DrawAreaDefault(_graphics, _area, _itemDrawn, _bounds);
		}
	}
}
