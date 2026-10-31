using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Scheduler;

namespace Mordex.Demo
{
	/// <summary>Shared helpers for the scheduling pages: one color for each person and the sample appointments.</summary>
	internal static class SchedulingSample
	{
		private static readonly Color[] OwnerColors =
		{
			Color.FromArgb(0, 114, 188),
			Color.FromArgb(0, 150, 110),
			Color.FromArgb(214, 109, 0),
			Color.FromArgb(142, 68, 173),
			Color.FromArgb(200, 40, 70)
		};

		public static Color ColorOf(int ownerIndex)
		{
			return OwnerColors[ownerIndex % OwnerColors.Length];
		}

		public static void AddOwners(MxScheduler scheduler)
		{
			foreach (string name in DemoData.Staff)
			{
				scheduler.Owners.Add(name, name);
			}
		}

		public static void Style(SchedulerAppointment appointment, Color color)
		{
			appointment.FormatStyle.BackColor = color;
			appointment.FormatStyle.ForeColor = Color.White;
			appointment.BorderColor = ControlPaint.Dark(color, 0.1f);
		}

		public static void AddCalendarEvents(MxScheduler scheduler)
		{
			foreach (DemoData.CalendarEvent item in DemoData.CalendarEvents())
			{
				SchedulerAppointment appointment = new SchedulerAppointment(item.Start, item.End, item.Subject);
				appointment.Owner = item.Owner;
				appointment.Description = item.Location;
				Style(appointment, ColorOf(Array.IndexOf(DemoData.Staff, item.Owner)));
				scheduler.Appointments.Add(appointment);
			}
		}

		/// <summary>Today when it is a work day, otherwise the next Monday, so the first view always has appointments.</summary>
		public static DateTime FirstDay()
		{
			DateTime today = DateTime.Today;
			if (today.DayOfWeek == DayOfWeek.Saturday)
			{
				return today.AddDays(2);
			}
			return today.DayOfWeek == DayOfWeek.Sunday ? today.AddDays(1) : today;
		}

		public static DateTime MondayOf(DateTime date)
		{
			return date.Date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
		}

		public static void SetWorkingTime(MxScheduler scheduler, bool lunchBreak)
		{
			scheduler.WorkWeek = SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
			scheduler.WorkStartTime = TimeSpan.FromHours(8);
			scheduler.WorkEndTime = TimeSpan.FromHours(18);
			scheduler.WorkingHourSchema.WorkingHoursRange.Clear();
			if (lunchBreak)
			{
				SchedulerHourRange lunch = new SchedulerHourRange(scheduler.WorkWeek, TimeSpan.FromHours(12), TimeSpan.FromHours(13), false);
				scheduler.WorkingHourSchema.WorkingHoursRange.Add(lunch);
			}
		}
	}
}
