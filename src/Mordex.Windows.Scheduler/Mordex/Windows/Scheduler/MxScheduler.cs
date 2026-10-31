using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	[Designer("Mordex.Windows.Scheduler.Design.SchedulerDesigner, Mordex.Windows.Scheduler.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ToolboxItem(true)]
	[ListBindable(true)]
	[DefaultProperty("Date")]
	[DefaultEvent("AppointmentChanged")]
	public partial class MxScheduler : ScrollableBase, ISchedulerFormatStyleOwner, IDropListOwner, ISchedulerFilterOwner, IMordexXmlLayoutsSupport, ISupportInitialize, ISchedulerWorkingHoursOwner, ITextBoxOwner, IPersistComponentSettings
	{

		private IContainer _iContainer;

		private System.Windows.Forms.Timer _schedulerTimer;

		internal bool _hasCopiesInCollection;

		private DateTime _dragOriginalStart;

		private DateTime _dragOriginalEnd;

		private TimeSpan _timeSpan;

		internal DateTime _dragAnchorTime;

		internal ArrayList _draggedAppointmentCopies;

		internal bool _isCopyDrag;

		internal bool _isAppointmentDragActive;

		private TimeSpan _schedulerTimeSpan;

		private DateTime _dragInitialAnchorTime;

		internal bool _hasDragChangedDates;

		private SchedulerAppointmentOwner _schedulerSchedulerAppointmentOwner;

		internal bool _shouldRaiseChangeEvents = true;

		internal bool _isDropAccepted;

		internal int _columnGap;

		private Rectangle _timeNavigatorBounds;

		private TriState _showAllDayAreaScrollBar;

		private int _verticalScrollPosition = 52;

		internal int _allDayRowCount;

		internal int _allDayScrollOffset;

		private SchedulerOwnerLayoutCollection _headerLayouts = new SchedulerOwnerLayoutCollection();

		internal int _allDayRowGap;

		private SchedulerAppointmentOwnerLayoutState _schedulerAppointmentOwnerLayoutState;

		internal SchedulerThemeColors _themeRenderer;


		private SchedulerOwnerLayoutCollection _weekHeaderLayouts;

		private SchedulerAppointmentOwnerLayoutState _schedulerSchedulerAppointmentOwnerLayoutState;

		private SchedulerAppointmentOwnerLayoutState _pressedWeekHeader;

		private TriState _showWeekHeaders;

		private TriState _viewNavigationMode;



		internal bool _isPrinting;

		internal int _appointmentStyleVersion;

		private bool _useDefaultMenu = true;

		private ContextMenuStrip _contextMenu;

		private bool _isInitializing;

		private TriState _highlightCurrentTime;

		private StringAlignment _allDayEventsAlignment = StringAlignment.Center;

		private bool _showMinutesInTimeNavigator;


		private float _defaultBlend;

		private Color _blendColor = Color.Empty;

		private string _appointmentsPrefix = "";

		private SchedulerFormatStyle _hourBackgroundStyle;

		private SchedulerFormatStyle _weekDaysBackgroundStyle;

		private SchedulerFormatStyle _monthDaysFormatStyle;

		private SchedulerFormatStyle _alternatingMonthDaysFormatStyle;

		private SchedulerFormatStyle _alternatingHourBackgroundStyle;

		private SchedulerFormatStyle _selectedFormatStyle;

		private SchedulerFormatStyle _selectedInactiveFormatStyle;

		private SchedulerFormatStyle _timeNavigatorFormatStyle;

		private SchedulerFormatStyle _headersFormatStyle;

		private SchedulerFormatStyle _allDayAreaBackgroundStyle;

		private SchedulerFormatStyle _appointmentsFormatStyle;

		private SchedulerFormatStyle _workingHourBackgroundStyle;

		private SchedulerFormatStyle _alternatingWorkingHourBackgroundStyle;

		private System.Windows.Forms.Timer _reminderTimer;

		private System.Windows.Forms.Timer _edgeScrollTimer;

		private SchedulerHiddenAppointmentButton _scrollDateLabel;

		private int _minuteInterval;

		internal ArrayList _visibleOwners = new ArrayList();

		private SchedulerAppointmentOwnerCollection _owners;

		private SchedulerAppointmentCollection _publicAppointments;

		private SchedulerTimeSlots _timeSlots;

		internal SchedulerAppointmentCollection _appointments;

		private RecurrenceAppointmentCollection _recurrenceAppointmentCollection;

		internal DateTime _date;

		private DateCollection _dates;

		private SchedulerHourRangeCollection _dayViewHourRanges;

		private WorkingHoursSchema _workingHourSchema;

		internal bool _isWorkWeekView;

		private SchedulerView _view;

		private SchedulerView _schedulerView;

		private SchedulerAppointment[] _currentDayLane;

		internal ArrayList _appointmentLanes;

		internal bool _isAppointmentCacheStale = true;

		// Counts start or end time writes of appointments that belong to a collection.
		internal int _appointmentTimesVersion;

		internal int _dayAreaTop;

		private int _leftoverHeight;

		private HourEvenOdd _hourEvenOdd;

		private int _lastAllDayRowTop;

		internal ArrayList _visibleAppointments;

		internal ArrayList _allDayAppointments;

		// Appointments used to paint the day background when the all-day area is hidden. Rebuilt by layout, never by paint.
		private ArrayList _backgroundAppointments;

		private string _nextAppointmentText = "Next Appointment";

		private string _previousAppointmentText = "Previous Appointment";

		private string _newAppointmentText = "Click to add appointment";

		private string _fromText = "From";

		private string _toText = "To";

		private string _customDateFormat = "";

		private AlphaMode _backColorAlphaMode = AlphaMode.Opaque;

		private AlphaMode _schedulerBackColorAlphaMode;

		private AlphaMode _foreColorAlphaMode;

		private int _defaultAlpha = 255;

		private bool _acceptsTab = true;

		private SchedulerWatermarkImage _watermarkImage;

		private ImageDrawMode _backgroundImageDrawMode;

		private Rectangle _gridBounds = Rectangle.Empty;

		internal bool _hasOwners;

		private Font _baseFont;

		private Font _monthFont;

		private SchedulerDayOfWeek _workWeek = SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;

		private SchedulerDayOfWeek _firstDayOfWeek = SchedulerDayOfWeek.Sunday;

		private SchedulerDayOfWeek _schedulerFirstDayOfWeek;

		private Font _weekFont;

		private bool? _useCompatibleTextRendering;

		private Font _dayFont;

		private bool _dayNavigationButtons;

		private bool _showAllDayArea = true;

		private int _dayStartHour;

		private int _dayEndHour = 24;

		private TimeSpan _workStartTime;

		private TimeSpan _workEndTime;

		private int _rowHeight = -1;

		internal int _actualRowHeight;

		private int _defaultRowHeight;

		private bool _isReadingAppointment;

		private Color _gridLineColor = Color.Empty;

		private Color _gridLineColorLight = Color.Empty;

		private TimeHintOnAppointments _showTimeHintOnAppointments;

		internal bool _hasFocus;

		private bool _isExtendingSelection;

		private DateTime _selectionCursor = DateTime.MinValue;

		internal DragType _dragType = DragType.ResizeLeft;

		internal SchedulerAppointment _schedulerSchedulerAppointment;

		private bool _allowDelete = true;

		private bool _canChangeEditor = true;

		private SchedulerAppointmentEditControl _appointmentEditor;

		internal SchedulerAppointment _currentAppointment;

		private DateRange _selectedDays;

		private TimeSpan _firstVisibleTime = new TimeSpan(8, 0, 0);

		private Cursor _cursor;

		private DateTime _hoveredAppointmentDate = DateTime.MinValue;

		private SchedulerAppointment _hoveredAppointment;

		internal SchedulerAppointmentView _schedulerOwner;

		private DateTime _selectionAnchor = DateTime.MinValue;

		private Point _dragStartScreenPoint = Point.Empty;

		private Point _mouseDownPoint = Point.Empty;

		private bool _shouldIgnoreMouseSelection;

		private bool _multiSelect = true;

		internal SchedulerAllDayArea _allDayAreaControl;

		internal TimeSpan _firstVisibleSlotTime;

		internal int _dayAreaLeft;

		private SchedulerAppointmentOwnerLayoutState _firstVisibleSlot;

		internal SchedulerHourRangeLayoutCollection _dayLayouts;

		private int _weeks = 5;

		private SchedulerHourRangeLayoutCollection _dateStyleOverrides;

		private int _firstVisibleSlotIndex = -1;

		internal bool _isHourRangeCacheStale = true;

		private int _firstVisibleColumnIndex;

		private bool _shouldHandleItemDeleted = true;

		private bool _allowDragInNonWorkingTime = true;

		private AllowAppointmentDrag _allowAppointmentDrag = AllowAppointmentDrag.All;

		private bool _allowEdit = true;

		private bool _isColumnSplitterActive;

		private bool _allowDayResize = true;

		private bool _shouldHandleItemChanged = true;

		internal bool _shouldHandleItemAdded = true;

		internal static bool _isUpdatingTimeRange;

		private IList _iList;

		private object _dataSource;

		private BindingMemberInfo _bindingMemberInfo;

		private CurrencyManager _currencyManager;

		private SchedulerFieldCollection _fields;

		internal bool _isLoadingLayout;

		private SchedulerLayout _currentLayout;

		private SchedulerLayout _schedulerLayout;

		private bool _shouldLoadFullLayout = true;

		private SchedulerLayoutCollection _layouts;

		private bool _shouldSaveFullLayout = true;

		private CurrencyManager _schedulerCurrencyManager;

		private BindingMemberInfo _schedulerBindingMemberInfo;

		private BindingMemberInfo _recurrenceInfoBindingMember;

		private BindingMemberInfo _appointmentSettingsBindingMember;

		private BindingMemberInfo _startTimeBindingMember;

		private BindingMemberInfo _endTimeBindingMember;

		private BindingMemberInfo _textBindingMember;

		private BindingMemberInfo _descriptionBindingMember;

		private PropertyDescriptor _propertyDescriptor;

		private PropertyDescriptor _schedulerPropertyDescriptor;

		private PropertyDescriptor _endTimeDescriptor;

		private PropertyDescriptor _textDescriptor;

		private PropertyDescriptor _descriptionDescriptor;

		private PropertyDescriptor _recurrenceInfoDescriptor;

		private PropertyDescriptor _appointmentSettingsDescriptor;

		private bool _usesInternalTable;

		private DataTable _dataTable;

		private string _startTimeField = "";

		private string _endTimeField = "";

		private string _textField = "";

		private string _descriptionField = "";

		private string _ownerField = "";

		private string _recurrenceInfoField = "";

		private string _appointmentSettingsField = "";

		private SchedulerFormatConditionCollection _formatConditions;

		private IFilterCondition _filterCondition;

		private Font _timeDesignatorFont;

		internal SchedulerAppointmentOwnerStateCollection _appointmentsByOwnerDate = new SchedulerAppointmentOwnerStateCollection();

		private int _usedAppointmentViewCount;

		internal ArrayList _appointmentViews = new ArrayList();

		internal ArrayList _allDayAppointmentViews = new ArrayList();

		private int _usedMoreButtonCount;

		private ArrayList _moreButtons = new ArrayList();

		private HiddenAppointmentMode _hiddenAppointmentMode;

		private int _usedAllDayViewCount;

		private DateTime _maxDate = DateTime.MaxValue.Date.AddYears(-1);

		private DateTime _minDate = DateTime.MinValue.AddMonths(1);




		private Color _appointmentOutLineColor = Color.Black;

		// True when the developer set the outline color. Black is the default, so the color alone cannot tell.
		private bool _isAppointmentOutLineColorSet;

		private Pen _lightLightPen;

		private SchedulerSelectedAppointments _schedulerSelectedAppointments;

		private TimeFormat _timeFormat;

		private Color _currentTimeTopColor = Color.FromArgb(239, 226, 188);


		private Color _currentTimeLineColor = Color.FromArgb(187, 85, 3);

		private Brush _borderBrush;

		private Brush _stripBackgroundBrush;


		private SchedulerAppointmentOwnerLayoutState _hotHeader;

		private SchedulerAppointmentOwner _currentOwner;

		private ClockStyle _clockStyle = ClockStyle.Enhanced;

		private Appearance _headerAppearance = Appearance.Light3D;

		private bool _compressWeekEnds;

		private ImageSource _imageSource = ImageSource.ExternalImageList;

		private ImageList _internalImageList;

		private int _reminderImageIndex = -2;

		private int _recurrenceExceptionImageIndex = -2;

		private int _recurrenceImageIndex = -2;

		private ImageList _imageList;

		private OwnerDrawnArea _ownerDrawnArea;

		private Rectangle _ownerHeaderBounds = Rectangle.Empty;

		internal bool _isFullLayoutStale = true;

		internal bool _isAppointmentLayoutStale;

		internal bool _shouldHandleRemovals = true;

		private Interval _interval;

		private HideSelection _hideSelection = HideSelection.HideSelection;

		private bool _continuousScroll = true;

		private bool _showToolTips = true;

		private int _maxAllDayRows = -1;

		private AddNewMode _addNewMode;

		private bool _isSelectingWithMouse;

		private bool _isLastButtonLeft = true;

		internal bool _isEditingAppointment;

		internal SchedulerAppointment _newAppointmentBeingEdited;

		private bool _hasRemovedCopies;

		private bool _isEdgeScrollVertical;

		private bool _isEdgeScrollForward;

		private int _dayResizeColumnWidth = -1;

		private bool _shouldClearSelectionOnOwnerChange = true;

		private AddAction _addAction = AddAction.Code;

		private bool _showTimeAsClocks = true;

		private bool _showOwnersHeader = true;

		private const double ReminderCheckIntervalMilliseconds = 30000.0;

		private DateTime _nextReminderTime = DateTime.MaxValue;

		private SchedulerAppointment _nextReminderAppointment;

		private SchedulerStoredFilterCollection _storedFilters;

		private bool _multiOwner;

		internal bool _isOwnerListVisible;

		private SchedulerFormatStyleScrollableBase _schedulerFormatStyleScrollableBase;

		private int _dayColumns = -1;

		private bool _isOwnerListActive;

		private bool _isUpdatingDates;

		private bool _showOwnerList = true;

		private static readonly object _value = new object();

		private static readonly object _reminderEventKey = new object();

		private static readonly object _appointmentReadEventKey = new object();

		private static readonly object _appointmentsResetEventKey = new object();

		private static readonly object _appointmentAddedEventKey = new object();

		private static readonly object _appointmentRemovedEventKey = new object();

		private static readonly object _recurrentAppointmentRemovedEventKey = new object();

		private static readonly object _appointmentChangingEventKey = new object();

		private static readonly object _appointmentChangedEventKey = new object();

		private static readonly object _appointmentsLoadedEventKey = new object();

		private static readonly object _addingAppointmentEventKey = new object();

		private static readonly object _queryHourRangeEventKey = new object();

		private static readonly object _appointmentsChangedEventKey = new object();

		private static readonly object _getNewObjectEventKey = new object();

		private static readonly object _copyingAppointmentEventKey = new object();

		private static readonly object _appointmentCopiedEventKey = new object();

		private static readonly object _appointmentDoubleClickEventKey = new object();

		private static readonly object _hiddenAppointmentClickEventKey = new object();

		private static readonly object _validatingAppointmentTextEventKey = new object();

		private static readonly object _editingAppointmentEventKey = new object();

		private static readonly object _droppingAppointmentEventKey = new object();

		private static readonly object _appointmentDroppedEventKey = new object();

		private static readonly object _appointmentDragEventKey = new object();

		private static readonly object _appointmentEditedEventKey = new object();

		private static readonly object _removingAppointmentEventKey = new object();

		private static readonly object _removingRecurrentAppointmentEventKey = new object();

		private static readonly object _creatingRecurrenceExceptionEventKey = new object();

		private static readonly object _recurrenceExceptionCreatedEventKey = new object();

		private static readonly object _viewChangedEventKey = new object();

		private static readonly object _minDateChangedEventKey = new object();

		private static readonly object _maxDateChangedEventKey = new object();

		private static readonly object _selectedAppointmentsChangedEventKey = new object();

		private static readonly object _currentOwnerChangedEventKey = new object();

		private static readonly object _selectedDaysChangedEventKey = new object();

		private static readonly object _ownersChangedEventKey = new object();

		private static readonly object _filterAppliedEventKey = new object();

		private static readonly object _applyingFilterEventKey = new object();

		private static readonly object _layoutLoadEventKey = new object();

		private static readonly object _currentLayoutChangedEventKey = new object();

		private static readonly object _scrollEventKey = new object();

		private static readonly object _currentLayoutChangingEventKey = new object();

		private static readonly object _datesChangedEventKey = new object();

		private static readonly object _dataSourceChangedEventKey = new object();

		private static readonly object _intervalChangedEventKey = new object();

		private bool _overwriteFields;

		private ApplicationSettings _mordexApplicationSettings;

		private bool _saveSettings;

		private string _settingsKey;

		public MxScheduler()
		{
			InitializeDefaults();
			InitializeComponent();
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
		}

		private void InitializeDefaults()
		{
			_baseFont = (Font)Mordex.Windows.Common.ThemeFonts.Current.Clone();
			_interval = Interval.ThirtyMinutes;
			_minuteInterval = 60 / (int)_interval;
			_recurrenceAppointmentCollection = new RecurrenceAppointmentCollection(this);
			_publicAppointments = new SchedulerAppointmentCollection(this, _minuteInterval, isPublic: true);
			_appointments = new SchedulerAppointmentCollection(this, _minuteInterval, isPublic: false);
			_formatConditions = new SchedulerFormatConditionCollection(this);
			_fields = new SchedulerFieldCollection(this);
			_storedFilters = new SchedulerStoredFilterCollection(this);
			_owners = new SchedulerAppointmentOwnerCollection(this);
			_schedulerSelectedAppointments = new SchedulerSelectedAppointments(this);
			_layouts = new SchedulerLayoutCollection(this);
			_dates = new DateCollection(this);
			_view = SchedulerView.DayView;
			_schedulerView = SchedulerView.DayView;
			UpdateRowHeight();
			_dayAreaTop = _actualRowHeight * 2 + 10 + base.ClientRectangle.Top;
			_firstVisibleSlotTime = new TimeSpan(0, 0, 0);
			_schedulerFirstDayOfWeek = SchedulerDayOfWeek.Sunday;
			_workWeek = SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
			_dayLayouts = new SchedulerHourRangeLayoutCollection();
			SetUnboundMode(true);
			_dateStyleOverrides = new SchedulerHourRangeLayoutCollection();
			_selectedDays = new DateRange(DateTime.MinValue, DateTime.MinValue);
			_workStartTime = new TimeSpan(8, 0, 0);
			_workEndTime = new TimeSpan(17, 0, 0);
			_reminderTimer = new System.Windows.Forms.Timer();
			_reminderTimer.Tick += OnReminderTimerTick;
			_settingsKey = "";
			_allDayRowGap = 4;
			_hourBackgroundStyle = new SchedulerFormatStyle(this, PredefinedStyle.Hour);
			_hourBackgroundStyle._text = "HourBackgroundStyle";
			_alternatingHourBackgroundStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
			_alternatingHourBackgroundStyle._text = "AlternatingHourBackgroundStyle";
			_alternatingWorkingHourBackgroundStyle = new SchedulerFormatStyle(this, PredefinedStyle.None);
			_alternatingWorkingHourBackgroundStyle._text = "AlternatingWorkingHourBackgroundStyle";
			_workingHourBackgroundStyle = new SchedulerFormatStyle(this, PredefinedStyle.WorkHour);
			_workingHourBackgroundStyle._text = "WorkingHourBackgroundStyle";
			_selectedFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.Selected);
			_selectedFormatStyle._text = "SelectedFormatStyle";
			_selectedInactiveFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.SelectedInactive);
			_selectedInactiveFormatStyle._text = "SelectedInactiveFormatStyle";
			_allDayAreaBackgroundStyle = new SchedulerFormatStyle(this, PredefinedStyle.AllDay);
			_allDayAreaBackgroundStyle._text = "AllDayAreaBackgroundStyle";
			_appointmentsFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.Appointment);
			_appointmentsFormatStyle._text = "AppointmentsFormatStyle";
			_timeNavigatorFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.TimeNavigator);
			_timeNavigatorFormatStyle._text = "TimeNavigatorFormatStyle";
			_headersFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.Header);
			HeadersFormatStyle._text = "HeadersFormatStyle";
			_weekDaysBackgroundStyle = new SchedulerFormatStyle(this, PredefinedStyle.Day);
			_weekDaysBackgroundStyle._text = "WeekDaysBackgroundStyle";
			_monthDaysFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.Day);
			_monthDaysFormatStyle._text = "MonthDaysFormatStyle";
			_alternatingMonthDaysFormatStyle = new SchedulerFormatStyle(this, PredefinedStyle.AlternatingDay);
			_alternatingMonthDaysFormatStyle._text = "AlternatingMonthDaysFormatStyle";
			_selectionAnchor = _date.Add(_workStartTime);
			Date = DateTime.Today;
			Dates.AddWithoutNotify(_date.Date);
			_dayViewHourRanges = new SchedulerHourRangeCollection(this);
			_workingHourSchema = new WorkingHoursSchema(this, _dayViewHourRanges);
			_ownerDrawnArea = OwnerDrawnArea.None;
		}

		protected override void Dispose(bool disposing)
		{
			StopThemeListening();
			if (disposing && _saveSettings && !base.DesignMode && SettingsKey.Length > 0)
			{
				try
				{
					SaveComponentSettings();
				}
				catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Configuration.ConfigurationErrorsException)
				{
					Debug.WriteLine("Scheduler settings were not saved: " + ex.Message);
				}
			}
			if (_appointmentEditor != null)
			{
				_appointmentEditor.Dispose();
				_appointmentEditor = null;
			}
			if (_reminderTimer != null)
			{
				_reminderTimer.Stop();
				_reminderTimer.Tick -= OnReminderTimerTick;
				_reminderTimer.Dispose();
				_reminderTimer = null;
			}
			StopAutoScrollTimer();
			if (_allDayAreaControl != null && !_allDayAreaControl.IsDisposed)
			{
				ToggleControlEventHandlers(_allDayAreaControl, true);
			}
			if (_schedulerTimer != null)
			{
				_schedulerTimer.Enabled = false;
				_schedulerTimer.Tick -= OnRepaintTimerTick;
				_schedulerTimer.Dispose();
				_schedulerTimer = null;
			}
			if (disposing)
			{
				_publicAppointments?.Detach();
				_appointments?.Detach();
			}
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		internal bool ValidateAppointmentChange(AppointmentChange appointmentChange)
		{
			if (!IsUnboundMode())
			{
				switch (appointmentChange)
				{
				case AppointmentChange.EndTime:
					if (_endTimeDescriptor != null && _isReadingAppointment)
					{
						throw new InvalidOperationException("You can not change any values that affect the datasource while the control is reading an appointment");
					}
					return true;
				case AppointmentChange.StartTime:
					if (_schedulerPropertyDescriptor != null && _isReadingAppointment)
					{
						throw new InvalidOperationException("You can not change any values that affect the datasource while the control is reading an appointment");
					}
					return true;
				case AppointmentChange.Owner:
					if (_propertyDescriptor != null && _isReadingAppointment)
					{
						throw new InvalidOperationException("You can not change any values that affect the datasource while the control is reading an appointment");
					}
					return true;
				case AppointmentChange.Text:
					if (_textDescriptor != null && _isReadingAppointment)
					{
						throw new InvalidOperationException("You can not change any values that affect the datasource while the control is reading an appointment");
					}
					return true;
				case AppointmentChange.RecurrencePattern:
					if (_recurrenceInfoDescriptor != null && _isReadingAppointment)
					{
						throw new InvalidOperationException("You can not change any values that affect the datasource while the control is reading an appointment");
					}
					return true;
				case AppointmentChange.Description:
					if (_descriptionDescriptor != null && _isReadingAppointment)
					{
						throw new InvalidOperationException("You can not change any values that affect the datasource while the control is reading an appointment");
					}
					return true;
				default:
					return true;
				}
			}
			return true;
		}

		internal SchedulerAppointmentOwner FindOwnerByValue(object ownerValue)
		{
			foreach (SchedulerAppointmentOwner item in _owners)
			{
				if (item.Value == ownerValue || (item.Value != null && item.Value.Equals(ownerValue)))
				{
					return item;
				}
			}
			return null;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 28)
			{
				ResetReminders();
			}
			base.WndProc(ref m);
		}

		internal void CommitEditOfOtherView(SchedulerAppointmentView appointmentView)
		{
			if (_schedulerOwner != null && _schedulerOwner != appointmentView)
			{
				CommitEdit();
			}
		}



		internal bool SupportsListChangeNotification()
		{
			if (GetCurrencyManager().List == null)
			{
				return false;
			}
			if (!typeof(IBindingList).IsAssignableFrom(GetCurrencyManager().List.GetType()))
			{
				return false;
			}
			return ((IBindingList)GetCurrencyManager().List).SupportsChangeNotification;
		}

		private void DetachCurrencyManagerEvents()
		{
			if (_schedulerCurrencyManager != null)
			{
				if (SupportsListChangeNotification())
				{
					((IBindingList)GetCurrencyManager().List).ListChanged -= OnControlListChanged;
				}
				GetCurrencyManager().ItemChanged -= OnControlItemChanged;
			}
		}

		private void AttachCurrencyManagerEvents()
		{
			if (_schedulerCurrencyManager != null)
			{
				if (SupportsListChangeNotification())
				{
					((IBindingList)GetCurrencyManager().List).ListChanged += OnControlListChanged;
				}
				GetCurrencyManager().ItemChanged += OnControlItemChanged;
			}
		}

		internal bool MatchesFilter(SchedulerAppointment schedulerAppointment)
		{
			if (FilterConditionApplied != null && !FilterConditionApplied.IsAppointmentAccepted(schedulerAppointment))
			{
				return false;
			}
			return true;
		}

		public void Refetch()
		{
			if (!IsUnboundMode())
			{
				ReloadAppointmentsFromDataSource();
			}
		}

		private void ToggleControlEventHandlers(Control control, bool isDetaching)
		{
			if (!isDetaching)
			{
				control.DoubleClick += OnControlDoubleClick;
				control.MouseDown += OnControlMouseDown;
				control.MouseMove += OnControlMouseMove;
				control.MouseUp += OnChildControlMouseUp;
				control.Click += OnControlClick;
				control.MouseLeave += OnControlMouseLeave;
				control.DragDrop += OnControlDragDrop;
				control.DragOver += OnControlDragOver;
				control.DragLeave += OnControlDragLeave;
				control.KeyPress += OnControlKeyPress;
				control.KeyDown += OnControlKeyDown;
				control.KeyUp += OnControlKeyUp;
				control.MouseClick += OnControlMouseClick;
				control.MouseDoubleClick += OnControlMouseDoubleClick;
			}
			else
			{
				control.DoubleClick -= OnControlDoubleClick;
				control.MouseDown -= OnControlMouseDown;
				control.MouseMove -= OnControlMouseMove;
				control.MouseUp -= OnChildControlMouseUp;
				control.Click -= OnControlClick;
				control.MouseLeave -= OnControlMouseLeave;
				control.DragDrop -= OnControlDragDrop;
				control.DragOver -= OnControlDragOver;
				control.DragLeave -= OnControlDragLeave;
				control.KeyPress -= OnControlKeyPress;
				control.KeyDown -= OnControlKeyDown;
				control.KeyUp -= OnControlKeyUp;
				control.MouseClick -= OnControlMouseClick;
				control.MouseDoubleClick -= OnControlMouseDoubleClick;
			}
		}

		internal Pen GetLightLightPen(Color baseColor)
		{
			if (_lightLightPen == null)
			{
				if (baseColor.IsEmpty)
				{
					baseColor = ControlPaint.LightLight(baseColor);
				}
				_lightLightPen = new Pen(baseColor);
			}
			return _lightLightPen;
		}

		internal void DisposeDrawingResources()
		{
			if (_lightLightPen != null)
			{
				_lightLightPen.Dispose();
				_lightLightPen = null;
			}
			if (_stripBackgroundBrush != null)
			{
				_stripBackgroundBrush.Dispose();
				_stripBackgroundBrush = null;
			}
			if (_borderBrush != null)
			{
				_borderBrush.Dispose();
				_borderBrush = null;
			}
			if (_themeRenderer != null)
			{
				_themeRenderer.DisposeResources();
				_themeRenderer = null;
			}
			_scrollBarStyle.DisposeAllResources();
		}

		private void UpdateNextReminder(SchedulerAppointment appointment)
		{
			if ((appointment.MasterAppointment != null && appointment.MasterAppointment.Reminder != null && !appointment.MasterAppointment.Reminder.GetActive()) || !appointment.Reminder.Enabled || !appointment.Reminder.GetActive())
			{
				return;
			}
			DateTime dateTime = appointment.GetReminderTimeOrMax();
			if (!(dateTime < _nextReminderTime))
			{
				return;
			}
			_nextReminderTime = dateTime;
			_nextReminderAppointment = appointment;
			if (dateTime <= DateTime.Now)
			{
				RaiseDueReminder();
				return;
			}
			_reminderTimer.Stop();
			ArmReminderTimer();
		}

		// The wait is cut into short steps and checked against the clock on every step,
		// so a clock change or a daylight saving shift cannot make a reminder late or early.
		private void ArmReminderTimer()
		{
			double remainingMilliseconds = (_nextReminderTime - DateTime.Now).TotalMilliseconds;
			_reminderTimer.Interval = (int)Math.Max(1.0, Math.Min(remainingMilliseconds, ReminderCheckIntervalMilliseconds));
			_reminderTimer.Start();
		}

		private void OnReminderTimerTick(object sender, EventArgs eventArgs)
		{
			_reminderTimer.Stop();
			if (_nextReminderAppointment == null)
			{
				ResetReminders();
			}
			else if (_nextReminderTime <= DateTime.Now)
			{
				RaiseDueReminder();
			}
			else
			{
				ArmReminderTimer();
			}
		}

		internal void RecalculateReminder(SchedulerAppointment schedulerAppointmentArgument)
		{
			if (_reminderTimer == null)
			{
				return;
			}
			schedulerAppointmentArgument.Reminder.UpdateActive(true);
			if (schedulerAppointmentArgument == _nextReminderAppointment)
			{
				_nextReminderAppointment = null;
				ResetReminders();
			}
			else if ((schedulerAppointmentArgument.MasterAppointment != null || schedulerAppointmentArgument._recurrenceState == RecurrenceState.Master) && schedulerAppointmentArgument.RecurrencePattern != null)
			{
				SchedulerAppointment schedulerAppointment = null;
				schedulerAppointment = ((schedulerAppointmentArgument._recurrenceState != RecurrenceState.Master) ? schedulerAppointmentArgument.MasterAppointment.GetNextReminderAppointment() : schedulerAppointmentArgument.GetNextReminderAppointment());
				if (schedulerAppointment != null)
				{
					UpdateNextReminder(schedulerAppointment);
				}
			}
			else
			{
				UpdateNextReminder(schedulerAppointmentArgument);
			}
		}

		private void InitializeComponent()
		{
			_iContainer = new Container();
			new ResourceManager(typeof(MxScheduler));
			base.Size = new Size(200, 200);
		}

		protected virtual void OnReminder(AppointmentEventArgs e)
		{
			if (base.Events[_reminderEventKey] is AppointmentEventHandler appointmentEventHandler)
			{
				appointmentEventHandler(this, e);
			}
		}

		internal void RaiseAppointmentsReset(EventArgs eventArgs)
		{
			if (base.Events[_appointmentsResetEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}
	}
}
