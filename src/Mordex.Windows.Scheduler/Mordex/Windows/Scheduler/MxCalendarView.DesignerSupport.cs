using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxCalendarView
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !BackColor.Equals(SystemColors.Window);
		}

		public new void ResetBackColor()
		{
			BackColor = SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderFont()
		{
			return _headerFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderFont()
		{
			HeaderFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBlendColor()
		{
			BlendColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBlendColor()
		{
			return !_blendColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				UpdateWorkingHoursSchemaLayoutStateArray(false);
				RefreshMonthLayouts(true);
				UpdateWeekdayNames();
				if (_calendarControl != null)
				{
					_calendarControl._useTextRenderer = !UseCompatibleTextRendering;
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFirstMonth()
		{
			DateTime value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			return !_firstMonth.Equals(value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFirstMonth()
		{
			DateTime firstMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
			FirstMonth = firstMonth;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBoldedDates()
		{
			return _boldedDates.Length != 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBoldedDates()
		{
			BoldedDates = new DateTime[0];
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAnnuallyBoldedDates()
		{
			return _annuallyBoldedDates.Length != 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAnnuallyBoldedDates()
		{
			AnnuallyBoldedDates = new DateTime[0];
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedFormatStyle()
		{
			return _schedulerFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedFormatStyle()
		{
			_schedulerFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTrailingFormatStyle()
		{
			return _calendarSchedulerFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTrailingFormatStyle()
		{
			_calendarSchedulerFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWatermarkImage()
		{
			if (_watermarkImage == null)
			{
				return false;
			}
			return _watermarkImage.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWatermarkImage()
		{
			WatermarkImage.Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedInactiveFormatStyle()
		{
			return _selectedInactiveFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedInactiveFormatStyle()
		{
			_selectedInactiveFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDaysFormatStyle()
		{
			return _daysFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDaysFormatStyle()
		{
			_daysFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlternatingMonthDaysFormatStyle()
		{
			return _alternatingMonthDaysFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlternatingMonthDaysFormatStyle()
		{
			_alternatingMonthDaysFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeadersFormatStyle()
		{
			return _headersFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeadersFormatStyle()
		{
			_headersFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDaysHeadersFormatStyle()
		{
			return _daysHeadersFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDaysHeadersFormatStyle()
		{
			_daysHeadersFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMonthBackgroundStyle()
		{
			return _monthBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMonthBackgroundStyle()
		{
			_monthBackgroundStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlternatingMonthBackgroundStyle()
		{
			return _alternatingMonthBackgroundStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlternatingMonthBackgroundStyle()
		{
			_alternatingMonthBackgroundStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTodayRectColor()
		{
			return !_todayRectColor.Equals(Color.DarkRed);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTodayRectColor()
		{
			TodayRectColor = Color.DarkRed;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWorkingHourSchema()
		{
			return _workingHoursSchema.IsCustomized();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWorkingHourSchema()
		{
			_workingHoursSchema.UpdateWorkingHoursSchema();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaxDate()
		{
			MaxDate = DateTime.MaxValue.Date.AddYears(-1);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaxDate()
		{
			return !_maxDate.Equals(DateTime.MaxValue.Date.AddYears(-1));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMinDate()
		{
			return !_minDate.Equals(DateTime.MinValue.AddMonths(1));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMinDate()
		{
			MinDate = DateTime.MinValue.AddMonths(1);
		}

		private bool ShouldBoldForOwner(object value)
		{
			if (_boldDatesMode == BoldDatesMode.All)
			{
				return true;
			}
			if (_scheduler.CurrentOwner != null)
			{
				if (_scheduler.CurrentOwner.ValueEquals(value))
				{
					return true;
				}
				return false;
			}
			return true;
		}
	}
}
