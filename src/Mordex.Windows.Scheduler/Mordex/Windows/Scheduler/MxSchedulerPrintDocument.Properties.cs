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
		[Description("Specifies the rule for printing the date header when the Schedule control is on MonthView.")]
		[DefaultValue(MonthViewHeaderNaming.Automatic)]
		public MonthViewHeaderNaming MonthViewHeaderNaming
		{
			get
			{
				return _monthViewHeaderNaming;
			}
			set
			{
				_monthViewHeaderNaming = value;
			}
		}

		[DefaultValue(null)]
		[Description("The Schedule control to print.")]
		public MxScheduler Scheduler
		{
			get
			{
				return _scheduler;
			}
			set
			{
				if (value != _scheduler && value != null)
				{
					_scheduler = value;
					_compressWeekEnds = Scheduler.CompressWeekEnds;
					_interval = _scheduler.Interval;
				}
			}
		}

		[Description("Indicates the start time to print for the day view.")]
		[Localizable(true)]
		public TimeSpan StartTime
		{
			get
			{
				if (_startTime == null)
				{
					if (_scheduler != null)
					{
						return _scheduler.WorkStartTime;
					}
					return TimeSpan.Zero;
				}
				return (TimeSpan)_startTime;
			}
			set
			{
				_startTime = value;
			}
		}

		[Description("Gets or sets a value indicating the end time to print for the day view.")]
		[Localizable(true)]
		public TimeSpan EndTime
		{
			get
			{
				if (!_endTime.Equals(TimeSpan.Zero) || _scheduler == null)
				{
					return _endTime;
				}
				if (_scheduler.WorkEndTime != TimeSpan.Zero)
				{
					return _scheduler.WorkEndTime;
				}
				return new TimeSpan(24, 0, 0);
			}
			set
			{
				if (_endTime != value)
				{
					_endTime = value;
				}
			}
		}

		[Description("The text displayed in the header section on the left corner of the page.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string HeaderLeft
		{
			get
			{
				return _headerLeft;
			}
			set
			{
				_headerLeft = value;
			}
		}

		[Localizable(true)]
		[Description("The text displayed in the header section on the center of the page.")]
		[DefaultValue("")]
		public string HeaderCenter
		{
			get
			{
				return _headerCenter;
			}
			set
			{
				_headerCenter = value;
			}
		}

		[Localizable(true)]
		[Description("The text displayed in the header section on the right corner of the page.")]
		[DefaultValue("")]
		public string HeaderRight
		{
			get
			{
				return _headerRight;
			}
			set
			{
				_headerRight = value;
			}
		}

		[Description("The text displayed in the footer section on the left corner of the page.")]
		[Localizable(true)]
		[DefaultValue("")]
		public string FooterLeft
		{
			get
			{
				return _footerLeft;
			}
			set
			{
				_footerLeft = value;
			}
		}

		[Localizable(true)]
		[Description("The text displayed in the footer section on the center of the page.")]
		[DefaultValue("")]
		public string FooterCenter
		{
			get
			{
				return _footerCenter;
			}
			set
			{
				_footerCenter = value;
				_shouldNumberPages = false;
			}
		}

		[Description("The text displayed in the footer section on the right corner of the page.")]
		[Localizable(true)]
		public string FooterRight
		{
			get
			{
				return _footerRight;
			}
			set
			{
				_footerRight = value;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether background and foreground colors should be printed.")]
		public bool GrayShading
		{
			get
			{
				return _grayShading;
			}
			set
			{
				if (_grayShading != value)
				{
					_grayShading = value;
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(false)]
		[Description("Indicates whether the weekends should be compressed in one day when printing the MonthView of the Schedule.")]
		public bool CompressWeekEnds
		{
			get
			{
				return _compressWeekEnds;
			}
			set
			{
				_compressWeekEnds = value;
			}
		}

		[Description("Indicates the number of pages printed by the document.")]
		[DefaultValue(LayoutPages.OnePage)]
		public LayoutPages LayoutPages
		{
			get
			{
				return _layoutPages;
			}
			set
			{
				_layoutPages = value;
			}
		}
	}
}
