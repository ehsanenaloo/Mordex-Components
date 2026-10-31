using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public partial class TimelineSurface
	{
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 128;
				return createParams;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		MxTimeline ITimelineOwner.Timeline => timeline;

		[Description("Determines the appearance of the headers in the Calendar")]
		[DefaultValue(Appearance.Light3D)]
		public Appearance HeaderAppearance
		{
			get
			{
				return _headerAppearance;
			}
			set
			{
				if (_headerAppearance != value)
				{
					_headerAppearance = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the Calendar control displays week numbers (1-52) to the left of each row of days.")]
		public bool WeekNumbers
		{
			get
			{
				return _weekNumbers;
			}
			set
			{
				if (_weekNumbers != value)
				{
					_weekNumbers = value;
					Invalidate();
				}
			}
		}

		[DefaultValue(DayOfWeekAbbreviation.UseFirstLetter)]
		[Description("Indicates the format for the days of the week.")]
		public DayOfWeekAbbreviation DayOfWeekAbbreviation
		{
			get
			{
				return _dayOfWeekAbbreviation;
			}
			set
			{
				if (_dayOfWeekAbbreviation != value)
				{
					_dayOfWeekAbbreviation = value;
					UpdateDayNames();
					UpdateCellSizes();
					ComputeDroppedControlSize();
					UpdateTimelineLayoutStateArray(true);
					UpdateLayout(true);
					Invalidate();
				}
			}
		}

		[DefaultValue(1)]
		[Description("The scroll rate for Calendar control.")]
		public int ScrollChange
		{
			get
			{
				return _scrollChange;
			}
			set
			{
				if (value == 0)
				{
					value = 1;
				}
				_scrollChange = value;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates if the days with items should be displayed in bold.")]
		public bool BoldDatesWithItems
		{
			get
			{
				return _boldDatesWithItems;
			}
			set
			{
				if (_boldDatesWithItems != value)
				{
					_boldDatesWithItems = value;
					if (value && timeline != null)
					{
						MarkDatesWithItems();
					}
					else
					{
						ClearBoldedItemDates();
					}
					Invalidate();
				}
			}
		}

		[Description("The array of DateTime objects that determines which nonrecurring dates are displayed in bold.")]
		public DateTime[] BoldedDates
		{
			get
			{
				return _boldedDates;
			}
			set
			{
				_boldedDates = value;
				ApplyBoldedDates();
				Invalidate();
			}
		}

		[Description("The array of DateTime objects that determines which annual days are displayed in bold.")]
		public DateTime[] AnnuallyBoldedDates
		{
			get
			{
				return _annuallyBoldedDates;
			}
			set
			{
				_annuallyBoldedDates = value;
				ApplyBoldedDates();
				Invalidate();
			}
		}

		[DefaultValue(DockStyle.None)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new DockStyle Dock
		{
			get
			{
				return base.Dock;
			}
			set
			{
				base.Dock = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new Point Location
		{
			get
			{
				return base.Location;
			}
			set
			{
				base.Location = value;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether today's date its framed.")]
		public bool TodayRect
		{
			get
			{
				return _todayRect;
			}
			set
			{
				if (_todayRect != value)
				{
					_todayRect = value;
					Invalidate();
				}
			}
		}
	}
}
