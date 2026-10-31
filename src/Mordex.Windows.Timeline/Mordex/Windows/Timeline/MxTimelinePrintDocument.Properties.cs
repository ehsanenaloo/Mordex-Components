using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	public partial class MxTimelinePrintDocument
	{
		[Localizable(true)]
		[Description("Indicates the first date to print.")]
		public DateTime StartDate
		{
			get
			{
				if (_startDate == DateTime.MinValue && _timeline != null)
				{
					return _timeline.FirstDate;
				}
				return _startDate;
			}
			set
			{
				if (_startDate != value)
				{
					_startDate = value;
				}
			}
		}

		[Browsable(false)]
		public int TotalPages => _totalPages;

		[Browsable(false)]
		public int Page => _currentPageIndex;

		[DefaultValue("")]
		[Description("The text displayed in the footer section on the center of the page.")]
		public string PageFooterCenter
		{
			get
			{
				return _pageFooterCenter;
			}
			set
			{
				_pageFooterCenter = value;
			}
		}

		[DefaultValue("")]
		[Description("The text displayed in the footer section on the left of the page.")]
		public string PageFooterLeft
		{
			get
			{
				return _pageFooterLeft;
			}
			set
			{
				_pageFooterLeft = value;
			}
		}

		[Description("The text displayed in the footer section on the right of the page.")]
		[DefaultValue("")]
		public string PageFooterRight
		{
			get
			{
				return _pageFooterRight;
			}
			set
			{
				_pageFooterRight = value;
			}
		}

		[Description("The text displayed in the header section on the center of the page.")]
		[DefaultValue("")]
		public string PageHeaderCenter
		{
			get
			{
				return _pageHeaderCenter;
			}
			set
			{
				_pageHeaderCenter = value;
			}
		}

		[Description("The text displayed in the header section on the left of the page.")]
		[DefaultValue("")]
		public string PageHeaderLeft
		{
			get
			{
				return _pageHeaderLeft;
			}
			set
			{
				_pageHeaderLeft = value;
			}
		}

		[Description("The text displayed in the header section on the right of the page.")]
		[DefaultValue("")]
		public string PageHeaderRight
		{
			get
			{
				return _pageHeaderRight;
			}
			set
			{
				_pageHeaderRight = value;
			}
		}

		[Description("Gets or sets a value indicating the last date to print.")]
		public DateTime EndDate
		{
			get
			{
				if (_endDate == DateTime.MinValue && _timeline != null)
				{
					return _timeline.LastDate;
				}
				return _endDate;
			}
			set
			{
				_endDate = value;
			}
		}

		[Description("Determines whether collapsed rows should be printed.")]
		[DefaultValue(true)]
		public bool PrintCollapsedRows
		{
			get
			{
				return _printCollapsedRows;
			}
			set
			{
				_printCollapsedRows = value;
			}
		}

		[Description("Indicates whether background and foreground colors should be printed.")]
		[DefaultValue(true)]
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

		[Description("Specifies whether the expand box of the groups should be printed")]
		[DefaultValue(true)]
		public bool PrintExpandBox
		{
			get
			{
				return _printExpandBox;
			}
			set
			{
				_printExpandBox = value;
			}
		}

		[Description("The Timeline control to print.")]
		[DefaultValue(null)]
		public MxTimeline Timeline
		{
			get
			{
				return _timeline;
			}
			set
			{
				_timeline = value;
			}
		}
	}
}
