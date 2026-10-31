using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal partial class ControlStyleControl : Control
	{
		internal class CalendarDropDownAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleObject Parent
			{
				get
				{
					AccessibleObject accessibleObject = null;
					if (GetOwnerControl() != null && GetOwnerControl()._dropOwner != null)
					{
						accessibleObject = GetOwnerControl()._dropOwner.AccessibleObject;
					}
					if (accessibleObject != null)
					{
						return accessibleObject;
					}
					return base.Parent;
				}
			}

			public override string Name => "MordexCalendarDropDown";

			internal CalendarDropDownAccessibleObject(ControlStyleControl owner)
				: base(owner)
			{
			}

			private ControlStyleControl GetOwnerControl()
			{
				return (ControlStyleControl)base.Owner;
			}
		}

		private CalendarValueState _calendarValueState;

		internal ControlStyle _calendarStyle;

		private Timer _timer;

		private ScrollingCalendarControl _innerCalendarControl;

		private CalendarLayoutState[,] _monthBlocks = new CalendarLayoutState[0, 0];

		internal Font _calendarFont;

		internal Color _adjacentMonthColor;

		private bool _isRangeSelecting;

		protected bool _isDropDownShown;

		private MxColorSchemeAppearanceState _mordexColorSchemeAppearanceState;

		private int _titleHeight;

		private int _dayCellWidth;

		private int _dayCellHeight;

		private int _sideColumnWidth;

		private Rectangle[] _arrowRectangles = new Rectangle[2];

		private int _monthColumns;

		private int _monthRows;

		private CalendarLayoutState _calendarLayoutState;

		private string[] _weekdayNames = new string[7];

		private DaysOfWeekEnum _daysOfWeekEnum = DaysOfWeekEnum.Default;

		private DayOfWeekAbbreviationEnum _dayOfWeekAbbreviationEnum = default;

		private Rectangle _footerBounds;

		private GridLayoutState _gridLayoutState;

		private GridLayoutState _controlStyleControlGridLayoutState;

		private bool _isTodayPressed;

		private bool _isNonePressed;

		private DateTime _dateTime;

		private int _scrollMonthDelta;

		private int _scrollMonthStep = 1;

		private int _clickedMonthIndex;

		private Font _controlStyleControlFont;

		private DateTime _controlStyleControlDateTime = DateTime.Today;

		private bool _isValueEmpty = true;

		private Color _todayBorderColor = Color.DarkRed;

		private bool _showTodayBorder = true;

		private SelectionStyleEnum _selectionStyleEnum = default;

		private CalendarValueState _controlStyleControlCalendarValueState = new CalendarValueState(DateTime.Today, DateTime.Today);

		private object _rangeStartDate;

		private int _maxRangeDays = 7;

		private IGridDropListOwner _dropOwner;

		private string _todayButtonText = "Today";

		private string _noneButtonText = "None";

		private GridFormatStyle _gridFormatStyle;

		private Timer _controlStyleControlTimer;

		private IntPtr _originalParentHandle = IntPtr.Zero;

		internal ControlStyleControl()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, value: true);
			InitializeLayoutState();
			InitializeChildControls();
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
		}
	}
}
