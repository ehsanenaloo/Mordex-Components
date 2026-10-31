using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public partial class TimelineSurface
	{
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

		internal bool ShouldUseCompatibleTextRendering()
		{
			if (timeline != null)
			{
				return timeline.UseCompatibleTextRendering;
			}
			return true;
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBoldedDates()
		{
			BoldedDates = new DateTime[0];
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBoldedDates()
		{
			return _boldedDates.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAnnuallyBoldedDates()
		{
			return _annuallyBoldedDates.Length > 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAnnuallyBoldedDates()
		{
			AnnuallyBoldedDates = new DateTime[0];
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedFormatStyle()
		{
			return _selectedFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedFormatStyle()
		{
			_selectedFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeTrailingFormatStyle()
		{
			return _trailingFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTrailingFormatStyle()
		{
			_trailingFormatStyle.Reset();
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
		public bool ShouldSerializeHeaderFormatStyle()
		{
			return _headerFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderFormatStyle()
		{
			_headerFormatStyle.Reset();
			Invalidate();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDayHeadersFormatStyle()
		{
			return _dayHeadersFormatStyle.ShouldSerialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDayHeadersFormatStyle()
		{
			_dayHeadersFormatStyle.Reset();
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
		public bool ShouldSerializeTodayRectColor()
		{
			return !_todayRectColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetTodayRectColor()
		{
			TodayRectColor = Color.Empty;
		}

		internal bool ShouldSerializeMaxDate()
		{
			return !_maxDate.Equals(DateTime.MaxValue.Date.AddYears(-1));
		}

		internal bool ShouldSerializeMinDate()
		{
			return !_minDate.Equals(DateTime.MinValue.AddMonths(1));
		}

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
		public bool ShouldSerializeForeColor()
		{
			return !ForeColor.Equals(SystemColors.ControlText);
		}

		public new void ResetForeColor()
		{
			base.ForeColor = SystemColors.ControlText;
		}
	}
}
