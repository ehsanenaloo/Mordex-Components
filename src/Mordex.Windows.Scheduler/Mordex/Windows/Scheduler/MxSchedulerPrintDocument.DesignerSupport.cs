using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	public partial class MxSchedulerPrintDocument
	{
		public bool ShouldSerializeStartTime()
		{
			return _startTime != null;
		}

		public void ResetStartTime()
		{
			_startTime = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeEndTime()
		{
			return !_endTime.Equals(TimeSpan.Zero);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetEndTime()
		{
			_endTime = TimeSpan.Zero;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderFont()
		{
			return _headerFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderFont()
		{
			_headerFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFooterFont()
		{
			return _footerFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFooterFont()
		{
			_footerFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFooterRight()
		{
			string strA = DateTime.Today.ToShortDateString();
			return string.Compare(strA, _footerRight, ignoreCase: true) != 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFooterRight()
		{
			FooterRight = DateTime.Today.ToShortDateString();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDateHeadingsFont()
		{
			return !_dateHeadingsFont.Equals(new Font("Tahoma", 24f));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDateHeadingsFont()
		{
			_dateHeadingsFont = new Font("Tahoma", 24f);
		}

		private bool ShouldUseShortestDayNames()
		{
			string name = CultureInfo.CurrentCulture.Name;
			if (string.Compare(name, "zh-CHS", ignoreCase: true) == 0 || string.Compare(name, "zh-MO", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "zh-CN", ignoreCase: true) == 0 || string.Compare(name, "zh-SG", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "zh-TW", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "he-IL", ignoreCase: true) == 0)
			{
				return true;
			}
			return false;
		}

		private bool ShouldShowTimeHint(SchedulerAppointmentView appointmentRenderer)
		{
			if (_scheduler.ShowTimeHintOnAppointments == TimeHintOnAppointments.Never)
			{
				return false;
			}
			if (_scheduler.ShowTimeHintOnAppointments == TimeHintOnAppointments.Always)
			{
				return true;
			}
			if (appointmentRenderer.GetStartTimeOfDay() != appointmentRenderer.GetAppointment().StartTime.TimeOfDay)
			{
				return true;
			}
			if (appointmentRenderer.GetSchedulerRenderer4TimeSpan() > EndTime)
			{
				return true;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMonthFont()
		{
			return _monthFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMonthFont()
		{
			_monthFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWeekFont()
		{
			return _weekFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWeekFont()
		{
			_weekFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDayFont()
		{
			return _dayFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDayFont()
		{
			_dayFont = null;
		}
	}
}
