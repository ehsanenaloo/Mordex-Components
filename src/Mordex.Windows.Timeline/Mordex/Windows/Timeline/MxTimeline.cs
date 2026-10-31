using System;
using System.Collections;
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
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Data;
using Mordex.Windows.Timeline.Design;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	[DefaultProperty("DataSource")]
	[Designer("Mordex.Windows.Timeline.Design.TimelineDesigner, Mordex.Windows.Timeline.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	[ListBindable(true)]
	[ToolboxItem(true)]
	public partial class MxTimeline : ScrollableBase, ITimelineOwner, ITimelineWorkingHoursOwner, ITimelineDropDownOwner, ISupportInitialize, IMordexXmlLayoutsSupport, ITextBoxOwner, ISupportFilterBuilder, IPersistComponentSettings
	{

		private Timer _currentTimeTimer;

		private TimelineDataSourceManager _timelineDataSourceManager;

		private Timer _timelineTimer;

		private IContainer _iContainer;

		private TimelineSurface _dropDownCalendar;

		private bool _shouldRaiseSelectedItemsChanged = true;

		private int _rowsBottom;

		private bool _useDefaultMenu = true;

		private ApplicationSettings _mordexApplicationSettings;

		private bool _saveSettings;

		private string _settingsKey;

		private ContextMenuStrip _contextMenu;

		private static readonly object _dropDownEventKey = new object();

		private static readonly object _calendarCloseUpEventKey = new object();

		private bool _isDropDownOpen;

		private bool _hasPendingLastDateChange;

		private DateTime _firstDate;

		private int _intervalWidth = 84;

		private bool _allowEdit = true;

		private bool _forceDropOnIntervals = true;

		private AllowItemDrag _allowItemDrag = AllowItemDrag.All;

		private bool _dropDownNavigator = true;

		private object _dataSource;

		private bool _highlightCurrentTime;

		private TimelineLayout _currentLayout;

		private TimelineLayout _timelineLayout;

		private TimelineLayoutCollection _layouts;

		private bool _isFullSerialization = true;

		private bool _isFullDeserialization = true;

		private bool _isLoadingItems;

		private bool _overwriteFields;

		private ItemSizeMode _itemSizeMode = ItemSizeMode.TextBased;

		private bool _isRefreshEnabled = true;

		private BindingMemberInfo _bindingMemberInfo;

		private object _pendingDataSource;

		private string _pendingDataMember;

		private BindingContext _bindingContext;

		private CurrencyManager _currencyManager;

		private bool _allowExpandRowHeader = true;

		private int _rowHeaderWidth = 120;

		private GroupMode _rowHeaderGroupMode;

		private bool _showLastGroupAsRowHeader;

		private static readonly object _drawTimelineAreaEventKey = new object();

		private static readonly object _selectedItemsChangedEventKey = new object();

		private static readonly object _editingItemEventKey = new object();

		private static readonly object _formattingGroupRowEventKey = new object();

		private static readonly object _filterAppliedEventKey = new object();

		private static readonly object _itemReadEventKey = new object();

		private static readonly object _applyingFilterEventKey = new object();

		private static readonly object _validatingItemTextEventKey = new object();

		private static readonly object _itemDroppedEventKey = new object();

		private static readonly object _droppingItemEventKey = new object();

		private static readonly object _itemDragEventKey = new object();

		private static readonly object _draggingItemEventKey = new object();

		private static readonly object _getNewObjectEventKey = new object();

		private static readonly object _addingItemEventKey = new object();

		private static readonly object _itemChangingEventKey = new object();

		private static readonly object _rootTableChangedEventKey = new object();

		private static readonly object _itemChangedEventKey = new object();

		private static readonly object _itemAddedEventKey = new object();

		private static readonly object _itemRemovedEventKey = new object();

		private TimelineItem _timelineItem;

		private readonly ConditionalWeakTable<object, TimelineItem> _itemsBySourceRow = new ConditionalWeakTable<object, TimelineItem>();

		private static readonly object _removingItemEventKey = new object();

		private static readonly object _layoutLoadEventKey = new object();

		private static readonly object _currentLayoutChangedEventKey = new object();

		private static readonly object _selectedRangeChangedEventKey = new object();

		private static readonly object _lastDateChangedEventKey = new object();

		private static readonly object _verticalScrollPositionChangedEventKey = new object();

		private static readonly object _firstDateChangedEventKey = new object();

		private static readonly object _currentLayoutChangingEventKey = new object();

		private static readonly object _itemEditedEventKey = new object();

		private static readonly object _itemsLoadedEventKey = new object();

		private MxRowLayoutState _mordexRowLayoutState = default;

		private string _itemsPrefix = "";

		private string _itemsSuffix = string.Empty;

		private ItemToolTip _itemToolTip = ItemToolTip.UseToolTipText;

		private ArrayList _rowLayouts;

		private bool? _useCompatibleTextRendering;

		private bool _isUnbound;

		private DataTable _dataTable;

		private string _startTimeField = "";

		private string _estimatedStartTimeField = "";

		private string _endTimeField = "";

		private string _estimatedEndTimeField = "";

		private string _textField = "";

		private BindingMemberInfo _timelineBindingMemberInfo;

		private BindingMemberInfo _estimatedEndTimeBinding;

		private BindingMemberInfo _startTimeBinding;

		private BindingMemberInfo _endTimeBinding;

		private BindingMemberInfo _textBinding;

		private BindingMemberInfo _toolTipBinding;

		private TimelineItemCollection _items;

		private TimelineSelectedItemCollection _selectedItems;

		private string _defaultItemsImageKey = "";

		private int _defaultItemsImageIndex = -1;

		private TimelineField _timelineField;

		private AlphaMode _defaultAlphaMode = AlphaMode.Opaque;

		private AlphaMode _defaultBackColorAlphaMode;

		private AlphaMode _defaultForeColorAlphaMode;

		private int _defaultAlpha = 255;

		private float _defaultBlend;

		private Color _blendColor = Color.Empty;

		private Font _tooltipFont;

		private int _itemsDurationBarHeight = 7;

		private int _groupRowHeight;

		private int _rowHeight = 1;

		private int _timelineRowHeight = -1;

		private float _floatValue;

		private TimelineCollection _allRowLayouts;

		private DateTime _minDate = DateTime.MinValue.Date.AddMonths(1);

		private DateTime _maxDate = DateTime.MaxValue.Date.AddYears(-1);

		private ArrayList _visibleItems;

		private ArrayList _rowItems;

		private bool _showItemsDurationBar = true;

		private bool _showItemsEstimatedDurationBar;

		private bool _showLabel = true;

		private ItemLayoutMode _itemLayoutMode = ItemLayoutMode.NewRowOnOverlap;

		private int _maximumLabelLength = -1;

		private bool _multiSelect = true;

		private bool _showWeekNumbers;

		private string _weekText;

		private int _minItemWidth = 20;

		private double _doubleValue;

		private double _lastDateOffset;

		private DateTime _lastVisibleDate;

		private bool _isLayoutDirty = true;

		private bool _isTimescaleValidationEnabled = true;

		private TimescaleTiers _timescaleTiers = TimescaleTiers.TwoTiers;

		private bool _isTimescaleDirty = true;

		internal long _intervalSeconds = 86400;

		private int _bottomTierCharWidth;

		private int _upperTierCharWidth;

		internal TimelineThemeRenderer _themeRenderer;

		internal bool _areRowsDirty = true;

		private MxRowLayoutState _timelineMordexRowLayoutState;

		private TimelineWindowMessageHandler _timelineWindowMessageHandler;

		private ToolTip _toolTip;

		private TimelineItem _timelineTimelineItem;

		private TimeFormat _timeFormat;

		private OwnerDrawnArea _ownerDrawnArea;

		private WorkingHoursSchema _workingHourSchema;

		private int _toolTipDelay = 5000;

		private ArrayList _draggedItems;

		private ItemDragType _itemDragType;

		private double _dragGrabOffset;

		private TimelineItem _dragItem;

		private bool _isDraggingItems;

		private bool _isDropPending;

		private bool _isAutoScrollVertical;

		private bool _isAutoScrollForward;

		private bool _isAutoScrolling;

		private long _lastAutoScrollTicks;

		private TimelineItem _blinkingItem;

		private TimelineItem _editingItem;

		private TimelineFormatStyleControl _timelineFormatStyleControl;

		private Point _dragStartPoint = Point.Empty;

		private MxRowLayoutState _focusedRowLayout;

		private TimelineItem _currentItem;

		private bool _isSelectingRange;

		private DateRange _selectedRange;

		private DateTime _selectionAnchorDate = DateTime.MinValue;

		private GroupMode _groupMode = GroupMode.Expanded;

		private BackgroundImageDrawMode _backgroundImageDrawMode = BackgroundImageDrawMode.Stretch;

		private TimeSpan _workStartTime = new TimeSpan(8, 0, 0);

		private TimeSpan _workEndTime = new TimeSpan(17, 0, 0);

		private DayOfWeek _workWeek = DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday;

		private TimelineFormatStyle _selectedFormatStyle;

		private HideSelection _hideSelection = HideSelection.HighlightInactive;

		private TimelineFormatStyle _selectedInactiveFormatStyle;

		private TimelineFormatStyle _itemsFormatStyle;

		private TimelineFormatStyle _backgroundFormatStyle;

		private TimelineFormatStyle _workingTimeFormatStyle;

		private Timescale _topTier;

		private Timescale _middleTier;

		private Timescale _bottomTier;

		private TimescaleTiers _timelineTimescaleTiers = TimescaleTiers.TwoTiers;

		private TimelineFormatStyle _lowerScaleTierFormatStyle;

		private TimelineFormatStyle _upperScaleTiersFormatStyle;

		private TimelineFormatStyle _itemsBarFormatStyle;

		private TimelineFormatStyle _itemsEstimatedBarFormatStyle;

		private TimelineFormatStyle _rowHeadersFormatStyle;

		private TimelineFormatStyle _groupRowsFormatStyle;

		private int _groupIndent = 20;

		private Appearance _headerAppearance = Appearance.Light3D;

		private ItemsLineStyle _itemsLineStyle;

		private MxRowLayoutState _firstVisibleRowLayout;

		private TimelineFormatConditionCollection _formatConditions;

		private Image _expandedArrowImage;

		private Image _expandBoxImage;

		private Image _collapsedArrowImage;

		private Image _collapsedPlusImage;

		private ImageSource _imageSource = ImageSource.ExternalImageList;

		private ImageList _internalImageList;

		private ImageList _externalImageList;

		private bool _isInitializing;

		private CalendarWeekRule _weekRule;

		private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

		private GridLines _gridLines = GridLines.None;

		private GridLineStyle _gridLineStyle;

		private ExpandBoxStyle _groupExpandBoxStyle;

		private Image _expandGlyph;

		private Image _collapseGlyph;

		private Color _gridLineColor = Color.Empty;

		private string _yearText = "Year";

		private int _intervalSize = 100;

		internal bool _isPrinting;

		private TimelineWatermarkImage _watermarkImage;

		private FocusZone _focusZone = FocusZone.BottomTierScale;

		private bool _acceptsTab = true;

		private int _blinkTickCount;

		public MxTimeline()
		{
			UpdateIContainer();
			InitializeDefaults();
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			_dropDownCalendar = new TimelineSurface(this);
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
					Debug.WriteLine("Timeline settings were not saved: " + ex.Message);
				}
			}
			if (_toolTip != null)
			{
				_toolTip.Dispose();
				_toolTip = null;
			}
			if (_currentTimeTimer != null)
			{
				_currentTimeTimer.Enabled = false;
				_currentTimeTimer.Tick -= OnTimerTick;
				_currentTimeTimer.Dispose();
				_currentTimeTimer = null;
			}
			if (_timelineTimer != null)
			{
				_timelineTimer.Enabled = false;
				_timelineTimer.Tick -= OnItemTimerTick;
				_timelineTimer.Dispose();
				_timelineTimer = null;
			}
			if (_timelineWindowMessageHandler != null)
			{
				_timelineWindowMessageHandler.DisposeAll();
				_timelineWindowMessageHandler = null;
			}
			if (disposing && _iContainer != null)
			{
				_iContainer.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeDefaults()
		{
			_timelineDataSourceManager = new TimelineDataSourceManager(this);
			_timelineDataSourceManager.RootTable = new TimelineMordexTableBase(this);
			_timelineTimer = new Timer();
			_timelineTimer.Interval = SystemInformation.DoubleClickTime;
			_timelineTimer.Tick += OnItemTimerTick;
			_items = new TimelineItemCollection(this);
			_selectedItems = new TimelineSelectedItemCollection(this);
			_formatConditions = new TimelineFormatConditionCollection(this);
			_layouts = new TimelineLayoutCollection(this);
			_topTier = new Timescale(this, new Font("Tahoma", 16f), TimelineInterval.Month);
			_middleTier = new Timescale(this, new Font("Tahoma", 8f), TimelineInterval.Week);
			_bottomTier = new Timescale(this, new Font("Tahoma", 8f), TimelineInterval.Day);
			_bottomTier.SetHasFocus(true);
			_workingHourSchema = new WorkingHoursSchema(this);
			_showWeekNumbers = false;
			_weekText = "Week";
			DisposeResources();
			_firstDate = DateTime.Today;
			UpdateRowHeights();
			SetUnbound(true);
			_groupRowsFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.GroupRow);
			_groupRowsFormatStyle._text = "GroupRowsFormatStyle";
			_rowHeadersFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.RowHeader);
			_rowHeadersFormatStyle._text = "RowHeadersFormatStyle";
			_itemsFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.Item);
			_itemsFormatStyle._text = "ItemsFormatStyle";
			_itemsBarFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.ItemBar);
			_itemsBarFormatStyle._text = "ItemsBarFormatStyle";
			_itemsEstimatedBarFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.EstimatedItemBar);
			_itemsEstimatedBarFormatStyle._text = "ItemsEstimatedBarFormatStyle";
			_selectedFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.Selected);
			_selectedFormatStyle._text = "SelectedFormatStyle";
			_selectedInactiveFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.SelectedInactive);
			_selectedInactiveFormatStyle._text = "SelectedInactiveFormatStyle";
			_lowerScaleTierFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.LowerScale);
			_lowerScaleTierFormatStyle._text = "LowerScaleTierFormatStyle";
			_upperScaleTiersFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.UpperScale);
			_upperScaleTiersFormatStyle._text = "UpperScaleTierFormatStyle";
			_backgroundFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.None);
			_backgroundFormatStyle._text = "BackgroundFormatStyle";
			_workingTimeFormatStyle = new TimelineFormatStyle(this, PredefinedStyle.WorkingTime);
			_workingTimeFormatStyle._text = "WorkingTimeFormatStyle";
		}

		public void Refetch()
		{
			object dataSource = _dataSource;
			BindingMemberInfo bindingMemberInfo = _bindingMemberInfo;
			ApplyDataBinding(null, default(BindingMemberInfo));
			ApplyDataBinding(dataSource, bindingMemberInfo);
		}

		private void DisposeResources()
		{
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			StringFormat stringFormat = new StringFormat();
			stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
			string text = "";
			string tierFormat = GetBottomTierFormat();
			int maxNameWidth = 0;
			int widestDayOfWeek = 1;
			int month = 1;
			// A year of wide digits, kept inside the range that the culture calendar can format.
			int sampleYear = Math.Min(8888, currentInfo.Calendar.MaxSupportedDateTime.Year - 1);
			DateTime dateTime = new DateTime(sampleYear, 1, 1);
			for (int i = 0; i < 7; i++)
			{
				text = dateTime.ToString("dddd");
				int dayNameWidth = TextRenderer.MeasureText(text, BottomTier.Font, Size.Empty).Width + 1;
				if (dayNameWidth > maxNameWidth)
				{
					maxNameWidth = dayNameWidth;
					widestDayOfWeek = (int)dateTime.DayOfWeek;
				}
				dateTime = dateTime.AddDays(1.0);
			}
			maxNameWidth = 0;
			for (int j = 0; j < 12; j++)
			{
				text = dateTime.ToString("MMMM");
				int monthNameWidth = TextRenderer.MeasureText(text, BottomTier.Font, Size.Empty).Width + 1;
				if (monthNameWidth > maxNameWidth)
				{
					maxNameWidth = monthNameWidth;
					month = dateTime.Month;
				}
				dateTime = dateTime.AddMonths(1);
			}
			dateTime = new DateTime(sampleYear, month, 20, 10, 58, 58, 58);
			for (int k = 0; k < 7; k++)
			{
				if (dateTime.DayOfWeek == (System.DayOfWeek)widestDayOfWeek)
				{
					break;
				}
				dateTime = dateTime.AddDays(1.0);
			}
			string sampleText = "";
			if (tierFormat == "W")
			{
				sampleText = currentInfo.GetAbbreviatedDayName((System.DayOfWeek)widestDayOfWeek);
			}
			else
			{
				try
				{
					if (tierFormat.Length > 0)
					{
						sampleText = dateTime.ToString(tierFormat, Application.CurrentCulture);
						if (tierFormat.IndexOf("W", 0) >= 0)
						{
							string abbreviatedDayName = currentInfo.GetAbbreviatedDayName((System.DayOfWeek)widestDayOfWeek);
							sampleText = sampleText.Replace("W", abbreviatedDayName);
						}
					}
					else
					{
						sampleText = tierFormat;
					}
				}
				catch
				{
					sampleText = tierFormat;
				}
			}
			if (_showWeekNumbers && BottomTier.Interval == TimelineInterval.Week)
			{
				sampleText = _weekText + " 88: " + sampleText;
			}
			_intervalWidth = TextRenderer.MeasureText(sampleText, BottomTier.Font, Size.Empty).Width + 4;
			_intervalWidth += _intervalWidth / 10;
			_intervalWidth = (int)((double)_intervalWidth * (double)_intervalSize / 100.0);
			stringFormat.Dispose();
			stringFormat = null;
		}

		private void OnControlInitialized(object sender, EventArgs eventArgs)
		{
			ApplyDataBinding(_pendingDataSource, new BindingMemberInfo(_pendingDataMember));
			if (_pendingDataSource is ISupportInitializeNotification supportInitializeNotification)
			{
				supportInitializeNotification.Initialized -= OnControlInitialized;
			}
			_pendingDataSource = null;
			_pendingDataMember = null;
		}

		internal override void OnAreaStyleChanged(EventArgs eventArgs)
		{
			UpdateThemeListening();
			RefreshHorizontalPanelColor();
			UpdateFormatStyleColors();
			InvalidateRows();
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			UpdateFormatStyleColors();
			if (_dropDownCalendar != null)
			{
				_dropDownCalendar.ApplyThemeToFormatStyles();
			}
			Invalidate();
		}

		protected virtual void OnFilterApplied(EventArgs e)
		{
			if (base.Events[_filterAppliedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnApplyingFilter(CancelEventArgs e)
		{
			if (base.Events[_applyingFilterEventKey] is CancelEventHandler cancelEventHandler)
			{
				cancelEventHandler(this, e);
			}
		}

		protected virtual void OnGetNewObject(GetNewObjectEventArgs e)
		{
			if (base.Events[_getNewObjectEventKey] is GetNewObjectEventHandler getNewObjectEventHandler)
			{
				getNewObjectEventHandler(this, e);
			}
		}

		private void OnRootTableChanged(EventArgs eventArgs)
		{
			if (base.Events[_rootTableChangedEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, eventArgs);
			}
		}

		internal object RaiseGetNewObject(MordexNewDataRow mordexNewDataRow)
		{
			if (!_isUnbound)
			{
				GetNewObjectEventArgs e = new GetNewObjectEventArgs();
				OnGetNewObject(e);
				return e.NewObject;
			}
			return null;
		}

		public void Refetch(TimelineItem item)
		{
			item?._row.EndEdit();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			UpdateRowHeights();
			InvalidateRows();
		}

		private void RecalculateLayout()
		{
			LayoutTiers();
			int visibleRowCount = GetVisibleRowCount();
			if (_rowLayouts != null && _rowLayouts.Count > 0)
			{
				UpdateVerticalScrollBar(0, _rowLayouts.Count - 1, visibleRowCount, _firstVisibleRowLayout.GetRowIndex());
			}
			else
			{
				UpdateVerticalScrollBar(0, 0, (base.ClientRectangle.Height - GetHeaderHeight()) / (RowHeight - 1), 0);
			}
			if (visibleRowCount >= _rowLayouts.Count)
			{
				if (IsVerticalScrollRequired())
				{
					base.SetVerticalScrollRequired(false);
					RecalculateLayout();
					return;
				}
			}
			else if (!IsVerticalScrollRequired())
			{
				base.SetVerticalScrollRequired(true);
				RecalculateLayout();
				return;
			}
			_doubleValue = GetIntervalDistance(_minDate, _firstDate);
			int visibleIntervalCount = base.ClientRectangle.Width / _intervalWidth;
			_floatValue = base.ClientRectangle.Left;
			int totalIntervalCount = 0;
			if (BottomTier.Interval == TimelineInterval.Year)
			{
				totalIntervalCount = MaxDate.Year - MinDate.Year;
			}
			else if (BottomTier.Interval == TimelineInterval.Month)
			{
				int yearSpan = MaxDate.Year - MinDate.Year;
				totalIntervalCount = yearSpan * 12 + (MaxDate.Month - MinDate.Month);
			}
			else
			{
				long ticks = MaxDate.Subtract(MinDate).Ticks;
				long intervalCount = ticks / ((long)_intervalSeconds * 10000000L);
				if (ticks % ((long)_intervalSeconds * 10000000L) != 0)
				{
					intervalCount++;
				}
				totalIntervalCount = (int)Math.Min(intervalCount, int.MaxValue);
			}
			if (totalIntervalCount == 0)
			{
				totalIntervalCount++;
			}
			DateTime dateTime = AlignToInterval(_firstDate, BottomTier.Interval, BottomTier.Count);
			switch (BottomTier.Interval)
			{
			case TimelineInterval.Second:
				if (_steps == -1)
				{
					GetHorizontalScrollBarControl().SetValueScale(11);
					GetHorizontalScrollBarControl().UpdateSmallChange(11);
					if (totalIntervalCount > 365)
					{
						_steps = (120 + visibleIntervalCount / 2) * 11;
					}
					else
					{
						_steps = (int)FirstDate.Subtract(MinDate).TotalSeconds * 100 / totalIntervalCount * totalIntervalCount / 100 * 11;
					}
				}
				if (dateTime != _firstDate)
				{
					_floatValue -= (float)_firstDate.Subtract(dateTime).TotalSeconds / (float)_intervalSeconds * (float)_intervalWidth;
				}
				if (totalIntervalCount > 365)
				{
					if (visibleIntervalCount > 40)
					{
						visibleIntervalCount *= 3;
					}
					UpdateHorizontalScrollBar(0, Math.Min(totalIntervalCount - 1, 180 + visibleIntervalCount), base.ClientRectangle.Width / _intervalWidth, _steps);
				}
				else
				{
					ApplyHorizontalScrollBar(0, totalIntervalCount - 1, base.ClientRectangle.Width / _intervalWidth, _steps, false);
				}
				break;
			case TimelineInterval.Minute:
				if (_steps == -1)
				{
					GetHorizontalScrollBarControl().SetValueScale(11);
					GetHorizontalScrollBarControl().UpdateSmallChange(11);
					if (totalIntervalCount > 365)
					{
						_steps = (77 + visibleIntervalCount / 2) * 11;
					}
					else
					{
						_steps = (int)FirstDate.Subtract(MinDate).TotalMinutes * 100 / totalIntervalCount * totalIntervalCount / 100 * 11;
					}
				}
				if (dateTime != _firstDate)
				{
					_floatValue -= (float)_firstDate.Subtract(dateTime).TotalSeconds / (float)_intervalSeconds * (float)_intervalWidth;
				}
				else
				{
					_floatValue -= (float)_firstDate.TimeOfDay.Seconds / (float)_intervalSeconds * (float)_intervalWidth;
				}
				if (totalIntervalCount > 365)
				{
					if (visibleIntervalCount > 40)
					{
						visibleIntervalCount *= 3;
					}
					UpdateHorizontalScrollBar(0, Math.Min(totalIntervalCount - 1, 180 + visibleIntervalCount), base.ClientRectangle.Width / _intervalWidth, _steps);
				}
				else
				{
					ApplyHorizontalScrollBar(0, totalIntervalCount - 1, base.ClientRectangle.Width / _intervalWidth, _steps, false);
				}
				break;
			case TimelineInterval.Hour:
				if (_steps == -1)
				{
					GetHorizontalScrollBarControl().SetValueScale(11);
					GetHorizontalScrollBarControl().UpdateSmallChange(11);
					if (totalIntervalCount > 365)
					{
						_steps = (77 + visibleIntervalCount / 2) * 11;
					}
					else
					{
						_steps = (int)FirstDate.Subtract(MinDate).TotalHours * 100 / totalIntervalCount * totalIntervalCount / 100 * 11;
					}
				}
				if (dateTime != _firstDate)
				{
					_floatValue -= (float)_firstDate.Subtract(dateTime).TotalSeconds / (float)_intervalSeconds * (float)_intervalWidth;
				}
				else
				{
					_floatValue -= (float)(_firstDate.TimeOfDay.Minutes * 60 + _firstDate.TimeOfDay.Seconds) / (float)_intervalSeconds * (float)_intervalWidth;
				}
				if (totalIntervalCount > 365)
				{
					if (visibleIntervalCount > 40)
					{
						visibleIntervalCount *= 3;
					}
					UpdateHorizontalScrollBar(0, Math.Min(totalIntervalCount - 1, 154 + visibleIntervalCount), base.ClientRectangle.Width / _intervalWidth, _steps);
				}
				else
				{
					ApplyHorizontalScrollBar(0, totalIntervalCount - 1, base.ClientRectangle.Width / _intervalWidth, _steps, false);
				}
				break;
			case TimelineInterval.Month:
			{
				if (_steps == -1)
				{
					if (totalIntervalCount > 365)
					{
						_steps = 12 + visibleIntervalCount / 2;
					}
					else
					{
						int monthsFromMin = (FirstDate.Year - MinDate.Year) * 12 + (FirstDate.Month - MinDate.Month);
						_steps = monthsFromMin * 100 / totalIntervalCount * totalIntervalCount / 100 * 11;
					}
					GetHorizontalScrollBarControl().SetValueScale(11);
					GetHorizontalScrollBarControl().UpdateSmallChange(11);
				}
				_floatValue -= (float)(GetIntervalDistance(dateTime, _firstDate) * (double)_intervalWidth);
				if (totalIntervalCount > 365)
				{
					if (visibleIntervalCount > 40)
					{
						visibleIntervalCount *= 3;
					}
					UpdateHorizontalScrollBar(0, Math.Min(totalIntervalCount - 1, 24 + visibleIntervalCount), base.ClientRectangle.Width / _intervalWidth, _steps);
				}
				else
				{
					ApplyHorizontalScrollBar(0, totalIntervalCount - 1, base.ClientRectangle.Width / _intervalWidth, _steps, false);
				}
				break;
			}
			case TimelineInterval.Week:
				if (_steps == -1)
				{
					GetHorizontalScrollBarControl().SetValueScale(11);
					GetHorizontalScrollBarControl().UpdateSmallChange(11);
					if (totalIntervalCount > 365)
					{
						_steps = (23 + visibleIntervalCount / 2) * 11;
					}
					else
					{
						_steps = (int)(FirstDate.Subtract(MinDate).TotalDays / 7.0) * 100 / totalIntervalCount * totalIntervalCount / 100 * 11;
					}
				}
				_floatValue -= (float)_firstDate.Subtract(dateTime).TotalSeconds / (float)_intervalSeconds * (float)_intervalWidth;
				if (totalIntervalCount > 365)
				{
					if (visibleIntervalCount > 40)
					{
						visibleIntervalCount *= 3;
					}
					UpdateHorizontalScrollBar(0, Math.Min(totalIntervalCount - 1, 46 + visibleIntervalCount), base.ClientRectangle.Width / _intervalWidth, _steps);
				}
				else
				{
					ApplyHorizontalScrollBar(0, totalIntervalCount - 1, base.ClientRectangle.Width / _intervalWidth, _steps, false);
				}
				break;
			case TimelineInterval.Year:
				if (_steps == -1)
				{
					GetHorizontalScrollBarControl().SetValueScale(11);
					GetHorizontalScrollBarControl().UpdateSmallChange(11);
					if (totalIntervalCount <= 365)
					{
						_steps = (FirstDate.Year - MinDate.Year) * 100 / totalIntervalCount * totalIntervalCount / 100 * 11;
						if (_firstDate.Subtract(MinDate).TotalDays > 0.0)
						{
							_steps += 11;
						}
					}
					else
					{
						_steps = 12 + visibleIntervalCount / 2;
					}
				}
				_floatValue -= (float)(GetIntervalDistance(dateTime, _firstDate) * (double)_intervalWidth);
				if (totalIntervalCount > 365)
				{
					if (visibleIntervalCount > 40)
					{
						visibleIntervalCount *= 3;
					}
					UpdateHorizontalScrollBar(0, Math.Min(totalIntervalCount - 1, 24 + visibleIntervalCount), base.ClientRectangle.Width / 300, _steps);
				}
				else
				{
					ApplyHorizontalScrollBar(0, totalIntervalCount - 1, base.ClientRectangle.Width / _intervalWidth, _steps, false);
				}
				break;
			case TimelineInterval.Day:
				if (_steps == -1)
				{
					GetHorizontalScrollBarControl().SetValueScale(11);
					GetHorizontalScrollBarControl().UpdateSmallChange(11);
					if (totalIntervalCount <= 365)
					{
						_steps = (int)FirstDate.Subtract(MinDate).TotalDays * 100 / totalIntervalCount * totalIntervalCount / 100 * 11;
					}
					else
					{
						_steps = (41 + visibleIntervalCount / 2) * 11;
					}
				}
				if (dateTime != _firstDate)
				{
					_floatValue -= (float)_firstDate.Subtract(dateTime).TotalSeconds / (float)_intervalSeconds * (float)_intervalWidth;
				}
				else
				{
					_floatValue -= (float)_firstDate.TimeOfDay.TotalSeconds / (float)_intervalSeconds * (float)_intervalWidth;
				}
				if (totalIntervalCount <= 365)
				{
					ApplyHorizontalScrollBar(0, totalIntervalCount - 1, base.ClientRectangle.Width / _intervalWidth, _steps, false);
					break;
				}
				if (visibleIntervalCount > 40)
				{
					visibleIntervalCount *= 3;
				}
				UpdateHorizontalScrollBar(0, Math.Min(totalIntervalCount - 1, 82 + visibleIntervalCount), base.ClientRectangle.Width / _intervalWidth, _steps);
				break;
			}
			if (BottomTier.Interval == TimelineInterval.Year)
			{
				SetLastVisibleDate(_firstDate.AddYears((int)((float)base.ClientRectangle.Width / (float)_intervalWidth * (float)BottomTier.Count) + 1));
			}
			else if (BottomTier.Interval == TimelineInterval.Month)
			{
				SetLastVisibleDate(_firstDate.AddMonths((int)((float)base.ClientRectangle.Width / (float)_intervalWidth * (float)BottomTier.Count) + 1));
			}
			else
			{
				SetLastVisibleDate(_firstDate.AddSeconds((float)base.ClientRectangle.Width / (float)_intervalWidth * (float)_intervalSeconds + (float)_intervalSeconds));
			}
			if (FirstDate <= MinDate && _lastVisibleDate >= MaxDate)
			{
				base.SetHorizontalScrollRequired(false);
			}
			else
			{
				base.SetHorizontalScrollRequired(true);
			}
			_lastDateOffset = GetIntervalDistance(_minDate, _lastVisibleDate);
			LayoutVisibleItems();
			if (GetEditedItem() != null && _timelineFormatStyleControl != null)
			{
				Rectangle bounds = TimelineUtilities.RoundToRectangle(_editingItem._rectangleF);
				int itemsAreaLeft = base.ClientRectangle.Left + _groupIndent * _editingItem.GetGroupIndex() + 1;
				if (bounds.X < itemsAreaLeft)
				{
					int clippedLeft = itemsAreaLeft - bounds.X;
					_timelineFormatStyleControl._hasLeftBorder = false;
					bounds.Width -= clippedLeft;
					bounds.X = itemsAreaLeft;
				}
				else
				{
					_timelineFormatStyleControl._hasLeftBorder = true;
				}
				if (bounds.Right > base.ClientRectangle.Right)
				{
					int clippedRight = bounds.Right - base.ClientRectangle.Right;
					_timelineFormatStyleControl._hasRightBorder = false;
					bounds.Width -= clippedRight;
				}
				else
				{
					_timelineFormatStyleControl._hasRightBorder = true;
				}
				_timelineFormatStyleControl.Bounds = bounds;
			}
			_isLayoutDirty = false;
		}

		private void DisposeCachedResources()
		{
			if (_themeRenderer != null)
			{
				_themeRenderer = null;
			}
			if (_expandedArrowImage != null)
			{
				_expandedArrowImage.Dispose();
				_expandedArrowImage = null;
			}
			if (_collapsedArrowImage != null)
			{
				_collapsedArrowImage.Dispose();
				_collapsedArrowImage = null;
			}
			if (_expandBoxImage != null)
			{
				_expandBoxImage.Dispose();
				_expandBoxImage = null;
			}
			if (_collapsedPlusImage != null)
			{
				_collapsedPlusImage.Dispose();
				_collapsedPlusImage = null;
			}
			_scrollBarStyle.DisposeAllResources();
		}

		protected override void OnClientResize()
		{
			TryCommitItemEdit();
			if (_rowLayouts != null)
			{
				LayoutRows();
				foreach (MxRowLayoutState item in _rowLayouts)
				{
					Rectangle rectangle = item.GetRowBounds();
					if (!rectangle.IsEmpty)
					{
						rectangle.Width = base.ClientRectangle.Right - rectangle.X;
						item.SetRowBounds(rectangle);
					}
				}
			}
			InvalidateLayout();
		}

		protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
		{
			base.OnGiveFeedback(e);
			if (_itemDragType != ItemDragType.Move)
			{
				e.UseDefaultCursors = false;
			}
			else
			{
				e.UseDefaultCursors = true;
			}
		}

		private void UpdateCurrentTimeTimer()
		{
			if (_highlightCurrentTime)
			{
				if (_currentTimeTimer == null)
				{
					_currentTimeTimer = new Timer();
					_currentTimeTimer.Tick += OnTimerTick;
					_currentTimeTimer.Interval = 60000;
					_currentTimeTimer.Enabled = true;
				}
			}
			else if (_currentTimeTimer != null)
			{
				try
				{
					_currentTimeTimer.Enabled = false;
					_currentTimeTimer.Tick -= OnTimerTick;
					_currentTimeTimer.Dispose();
					_currentTimeTimer = null;
				}
				catch
				{
				}
			}
		}

		private int GetSelectionDurationSeconds()
		{
			if (_selectedRange.Start != DateTime.MinValue)
			{
				return (int)_selectedRange.End.Subtract(_selectedRange.Start).TotalSeconds;
			}
			return 0;
		}

		private void DrawCurrentTimeMarker(Graphics graphics)
		{
			DateTime now = DateTime.Now;
			if (FirstDate < now && LastDate > now)
			{
				double nowOffset = GetIntervalDistance(_minDate, now);
				float markerX = (float)base.ClientRectangle.Left + (float)((nowOffset - _doubleValue) * (double)_intervalWidth);
				Rectangle rect = new Rectangle((int)markerX - 2, base.ClientRectangle.Top, 5, _rowsBottom);
				ResolvedTheme theme = MxThemeManager.ResolveDefault(DeviceDpi);
				graphics.FillRectangle(theme.Brush(ThemeRole.AccentSubtle), rect);
				graphics.DrawLine(theme.Pen(ThemeRole.Accent), (int)markerX, rect.Top, (int)markerX, rect.Bottom);
			}
		}

		internal void DrawWorkingTimeBackground(Graphics graphics, Rectangle rectangleArgument, DateTime dateTimeArgument, int width, int startX)
		{
			DateTime dateTime = AlignToInterval(dateTimeArgument, BottomTier.Interval, BottomTier.Count);
			DateTime cellEnd = dateTime;
			int cellX = startX;
			long cellSeconds = _intervalSeconds;
			int bottom = rectangleArgument.Bottom;
			if (Groups.Count > 0 && _rowLayouts != null && _rowLayouts.Count > 0)
			{
				MxRowLayoutState mordexRowLayoutState = (MxRowLayoutState)_rowLayouts[_rowLayouts.Count - 1];
				if (mordexRowLayoutState.GetRowBounds().Bottom < bottom)
				{
					bottom = mordexRowLayoutState.GetRowBounds().Bottom;
				}
			}
			while (cellX < rectangleArgument.Right)
			{
				cellEnd = ((BottomTier.Interval == TimelineInterval.Year) ? dateTime.AddYears(BottomTier.Count) : ((BottomTier.Interval != TimelineInterval.Month) ? dateTime.AddSeconds(cellSeconds) : dateTime.AddMonths(BottomTier.Count)));
				DateRange dateRange = new DateRange(dateTime, cellEnd);
				TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(WorkingTimeFormatStyle);
				TimelineFormatStyle backgroundStyle = new TimelineFormatStyle(BackgroundFormatStyle);
				if (backgroundStyle.BackColor.IsEmpty)
				{
					backgroundStyle.BackColor = MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.SurfaceAlt);
				}
				TimelineFormatStyle cellStyle = GetFormatStyleForRange(dateRange, backgroundStyle, timelineFormatStyle);
				Rectangle rectangle = new Rectangle(cellX, GetHeaderHeight() + rectangleArgument.Top, width, bottom - GetHeaderHeight());
				Brush brush = cellStyle.GetBackBrush(rectangle);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
					cellStyle.DisposeResources();
				}
				if (cellStyle.BackgroundImage != null)
				{
					DrawBackgroundImage(graphics, Rectangle.Ceiling(rectangle), cellStyle.BackgroundImage, cellStyle.BackgroundImageDrawMode, rectangle);
				}
				cellX += width;
				dateTime = cellEnd;
			}
		}
	}
}
