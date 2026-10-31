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
	public partial class MxTimeline
	{
		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
			}
		}

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(false)]
		[Description("Indicates whether the control should automatically persist its application settings properties.")]
		public bool SaveSettings
		{
			get
			{
				return _saveSettings;
			}
			set
			{
				_saveSettings = value;
			}
		}

		MxTimeline ITimelineWorkingHoursOwner.Timeline => this;

		DayOfWeek ITimelineWorkingHoursOwner.FirstDayOfWeek => FirstDayOfWeek;

		DayOfWeek ITimelineWorkingHoursOwner.WorkWeek => _workWeek;

		MxTimeline ITimelineOwner.Timeline => this;

		[Browsable(false)]
		public DateTime LastDate
		{
			get
			{
				try
				{
					if ((_isLayoutDirty && base.Created) || _isTimescaleDirty)
					{
						if (_isTimescaleDirty)
						{
							if (!_isTimescaleValidationEnabled)
							{
								ResumeTimescaleValidation();
							}
							UpdateTimescaleMetrics();
						}
						if (_areRowsDirty)
						{
							RebuildRows();
						}
						if (_isLayoutDirty)
						{
							RecalculateLayout();
						}
					}
				}
				catch (Exception ex)
				{
					Mordex.Windows.Common.MxMessageBox.Show(ex.ToString());
				}
				return GetDateAtX(base.ClientRectangle.Right);
			}
		}

		[Description("Represents the first visible date in the control")]
		[Category("Dates")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public DateTime FirstDate
		{
			get
			{
				return _firstDate;
			}
			set
			{
				if (value < _minDate)
				{
					value = _minDate;
				}
				if (value > _maxDate)
				{
					value = _maxDate;
				}
				if (_rowLayouts != null && value.Add(LastDate.Subtract(FirstDate)) > MaxDate)
				{
					value = MaxDate.Subtract(LastDate.Subtract(FirstDate));
				}
				if (value.Millisecond != 0)
				{
					value = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
				}
				if (_firstDate != value)
				{
					if (BottomTier.Interval == TimelineInterval.Year || BottomTier.Interval == TimelineInterval.Month || BottomTier.Interval == TimelineInterval.Week)
					{
						value = value.Date;
					}
					_firstDate = value;
					OnFirstDateChanged(EventArgs.Empty);
					InvalidateLayout();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TimelineLayoutCollection Layouts => _layouts;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public TimelineLayout DesignTimeLayout
		{
			get
			{
				if (ShouldSerializeDesignTimeLayout())
				{
					TimelineLayout timelineLayout = new TimelineLayout();
					timelineLayout._owner = this;
					timelineLayout.SetPersistenceMode(PersistenceMode.Fields | PersistenceMode.StoredFilters | PersistenceMode.FilterCondition | PersistenceMode.GroupHeaderTotals | PersistenceMode.InternalImageList | PersistenceMode.FormatConditions | PersistenceMode.Groups | PersistenceMode.WorkingHourSchema);
					LayoutWriter mordexLayoutWriter = new LayoutWriter();
					SerializeLayout(mordexLayoutWriter, timelineLayout.GetPersistenceMode());
					LayoutWriter.StoreInLayout("TimelineLayoutData", mordexLayoutWriter, timelineLayout);
					timelineLayout.DataMember = DataMember;
					timelineLayout.DataSource = DataSource;
					return timelineLayout;
				}
				return null;
			}
			set
			{
				value?.SetPersistenceMode(PersistenceMode.Fields | PersistenceMode.StoredFilters | PersistenceMode.FilterCondition | PersistenceMode.GroupHeaderTotals | PersistenceMode.InternalImageList | PersistenceMode.FormatConditions | PersistenceMode.Groups | PersistenceMode.WorkingHourSchema);
				LoadLayout(value);
			}
		}

		[Localizable(true)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TimelineGroupCollection Groups => (TimelineGroupCollection)_timelineDataSourceManager.RootTable.Groups;

		[Description("Represents the string displayed before the text of the items.")]
		[Category("Timeline Items")]
		[DefaultValue("")]
		public string ItemsPrefix
		{
			get
			{
				return _itemsPrefix;
			}
			set
			{
				if (_itemsPrefix != value)
				{
					_itemsPrefix = value;
					RefreshRowLayouts();
				}
			}
		}

		[Description("Represents the string displayed after the text of the items.")]
		[Category("Timeline Items")]
		[DefaultValue("")]
		public string ItemsSuffix
		{
			get
			{
				return _itemsSuffix;
			}
			set
			{
				if (_itemsSuffix != value)
				{
					_itemsSuffix = value;
					RefreshRowLayouts();
				}
			}
		}

		[Category("Timeline Items")]
		[Description("Specifies what will be the text for the tooltip of the items")]
		[DefaultValue(ItemToolTip.UseToolTipText)]
		public ItemToolTip ItemToolTip
		{
			get
			{
				return _itemToolTip;
			}
			set
			{
				_itemToolTip = value;
			}
		}

		[Browsable(false)]
		public TimelineGroupHeaderTotalCollection GroupHeaderTotals => (TimelineGroupHeaderTotalCollection)GetTimelineDataSourceManager().RootTable.GroupTotals;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public TimelineStoredFilterCollection StoredFilters => (TimelineStoredFilterCollection)_timelineDataSourceManager.RootTable.StoredFilters;

		[TypeConverter(typeof(FilterConditionConverter))]
		[DefaultValue(null)]
		[Category("Groups and Filter")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Represents the criteria used to filter the items in the Timeline.")]
		[Editor(typeof(FilterConditionEditor), typeof(UITypeEditor))]
		public IFilterCondition FilterCondition
		{
			get
			{
				return (IFilterCondition)_timelineDataSourceManager.RootTable.FilterCondition;
			}
			set
			{
				if (value != _timelineDataSourceManager.RootTable.FilterCondition)
				{
					_timelineDataSourceManager.RootTable.FilterCondition = (Mordex.Data.IFilterCondition)value;
					OnFilterApplied(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public TimelineFilterCondition FilterConditionApplied
		{
			get
			{
				if (FilterCondition != null)
				{
					return FilterCondition.FilterCondition;
				}
				return null;
			}
		}

		[Description("Specifies the property of the data source to map to the EstimatedStartTime property of the TimelineItem objects.")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Category("Data")]
		public string EstimatedStartTimeMember
		{
			get
			{
				return _timelineBindingMemberInfo.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_timelineBindingMemberInfo))
				{
					return;
				}
				if (!IsUnbound())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_timelineBindingMemberInfo = bindingMemberInfo;
					_estimatedStartTimeField = _timelineBindingMemberInfo.BindingField;
					InvalidateRowsAndLayout();
				}
				else
				{
					_timelineBindingMemberInfo = bindingMemberInfo;
					_estimatedStartTimeField = _timelineBindingMemberInfo.BindingField;
				}
			}
		}

		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Description("Specifies the property of the data source to map to the EstimatedEndTime property of the TimelineItem objects.")]
		[Category("Data")]
		[DefaultValue("")]
		public string EstimatedEndTimeMember
		{
			get
			{
				return _estimatedEndTimeBinding.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_estimatedEndTimeBinding))
				{
					return;
				}
				if (!IsUnbound())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_estimatedEndTimeBinding = bindingMemberInfo;
					_estimatedEndTimeField = _estimatedEndTimeBinding.BindingField;
					InvalidateRowsAndLayout();
				}
				else
				{
					_estimatedEndTimeBinding = bindingMemberInfo;
					_estimatedEndTimeField = _estimatedEndTimeBinding.BindingField;
				}
			}
		}

		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Description("Specifies the property of the data source to map to the StartTime property of the TimelineItem objects.")]
		[DefaultValue("")]
		[Category("Data")]
		public string StartTimeMember
		{
			get
			{
				return _startTimeBinding.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_startTimeBinding))
				{
					return;
				}
				if (!IsUnbound())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_startTimeBinding = bindingMemberInfo;
					_startTimeField = _startTimeBinding.BindingField;
					if (_timelineDataSourceManager != null)
					{
						_timelineDataSourceManager.Refresh(refreshAll: true);
					}
					InvalidateRowsAndLayout();
				}
				else
				{
					_startTimeBinding = bindingMemberInfo;
					_startTimeField = _startTimeBinding.BindingField;
				}
			}
		}

		[Description("Specifies the property of the data source to map to the EndTime property of the TimelineItem objects.")]
		[DefaultValue("")]
		[Category("Data")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(DataSourceFieldsConverter))]
		public string EndTimeMember
		{
			get
			{
				return _endTimeBinding.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (bindingMemberInfo.Equals(_endTimeBinding))
				{
					return;
				}
				if (!IsUnbound())
				{
					if (DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_endTimeBinding = bindingMemberInfo;
					_endTimeField = _endTimeBinding.BindingField;
					if (_timelineDataSourceManager != null)
					{
						_timelineDataSourceManager.Refresh(refreshAll: true);
					}
					InvalidateRowsAndLayout();
				}
				else
				{
					_endTimeBinding = bindingMemberInfo;
					_endTimeField = _endTimeBinding.BindingField;
				}
			}
		}

		[TypeConverter(typeof(DataSourceFieldsConverter))]
		[Description("Specifies the property of the data source to use as tooltip for the item when the ItemToolTip property is set as UseToolTipMember.")]
		[Category("Data")]
		[Editor(typeof(DataSourceFieldsEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		public string ToolTipMember
		{
			get
			{
				return _toolTipBinding.BindingMember;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				BindingMemberInfo bindingMemberInfo = new BindingMemberInfo(value);
				if (!bindingMemberInfo.Equals(_toolTipBinding))
				{
					if (!IsUnbound() && DataMember == "")
					{
						DataMember = bindingMemberInfo.BindingPath;
					}
					_toolTipBinding = bindingMemberInfo;
				}
			}
		}

		[Browsable(false)]
		[Description("Represents the collection of the items contained in the Timeline control")]
		public TimelineItemCollection Items => _items;

		[Browsable(false)]
		public TimelineSelectedItemCollection SelectedItems => _selectedItems;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public TimelineFieldCollection Fields => (TimelineFieldCollection)_timelineDataSourceManager.RootTable.Fields;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int VerticalScrollbarPosition
		{
			get
			{
				if (GetVerticalScrollBarControl() != null)
				{
					return GetVerticalScrollBarControl().GetScrollValue();
				}
				return 0;
			}
			set
			{
				if (GetVerticalScrollBarControl() != null)
				{
					GetVerticalScrollBarControl().UpdateScrollValue(value);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public WorkingHoursSchema WorkingHourSchema => _workingHourSchema;

		[DefaultValue(5000)]
		[Description("Determines the length of time the tooltip remains visible if the pointer is stationary inside the tooltip region")]
		[Category("Behavior")]
		public int ToolTipDelay
		{
			get
			{
				return _toolTipDelay;
			}
			set
			{
				_toolTipDelay = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateRange SelectedRange
		{
			get
			{
				return _selectedRange;
			}
			set
			{
				if (_selectedRange.Start != value.Start || _selectedRange.End != value.End)
				{
					_selectedRange = value;
					OnSelectedRangeChanged(EventArgs.Empty);
					SetFocusZone(FocusZone.BottomTierScale);
					Invalidate(BottomTier.GetBounds());
				}
			}
		}

		[Description("Indicates the start time of the day for the working hours.")]
		[Category("Working Time")]
		[Localizable(true)]
		public TimeSpan WorkStartTime
		{
			get
			{
				return _workStartTime;
			}
			set
			{
				if (_workStartTime != value)
				{
					_workStartTime = value;
					Invalidate();
				}
			}
		}

		[Localizable(true)]
		[Category("Working Time")]
		[Description("Indicates the end time of the day for the working hours.")]
		public TimeSpan WorkEndTime
		{
			get
			{
				return _workEndTime;
			}
			set
			{
				if (_workEndTime != value)
				{
					_workEndTime = value;
					Invalidate();
				}
			}
		}

		[Localizable(true)]
		[Category("Working Time")]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		[DefaultValue(DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday)]
		[TypeConverter(typeof(FlagsEnumConverter))]
		[Description("Indicates the working days of the week")]
		public DayOfWeek WorkWeek
		{
			get
			{
				return _workWeek;
			}
			set
			{
				if (_workWeek != value)
				{
					if (value == (DayOfWeek)0)
					{
						throw new ArgumentException();
					}
					_workWeek = value;
					Invalidate();
				}
			}
		}

		[Description("The TimeScale object containing the properties for the top tier in the Timeline control.")]
		[Category("Timescale")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Timescale TopTier => _topTier;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Timescale")]
		[Description("The TimeScale object containing the properties for the middle tier in the Timeline control.")]
		public Timescale MiddleTier => _middleTier;

		[Category("Timescale")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The TimeScale object containing the properties for the bottom tier in the Timeline control.")]
		public Timescale BottomTier => _bottomTier;

		[Description("Determines how many timescale tiers will be displayed in the Timeline control.")]
		[DefaultValue(TimescaleTiers.TwoTiers)]
		[Category("Timescale")]
		public TimescaleTiers TimescaleTiers
		{
			get
			{
				return _timelineTimescaleTiers;
			}
			set
			{
				if (_timelineTimescaleTiers != value)
				{
					if (!AreTimescaleTiersValid(value))
					{
						throw new ArgumentException("An upper tier has less time per tick than one of the tiers below it.");
					}
					_timelineTimescaleTiers = value;
					InvalidateTimescale();
				}
			}
		}

		[DefaultValue(20)]
		[Category("Groups and Filter")]
		[Description("The distance, in pixels, to indent group levels.")]
		public int GroupIndent
		{
			get
			{
				return _groupIndent;
			}
			set
			{
				if (_groupIndent != value)
				{
					if (value < 14)
					{
						throw new ArgumentOutOfRangeException("The value must be greater than 14");
					}
					_groupIndent = value;
					Invalidate();
				}
			}
		}

		[Category("Appearance")]
		[DefaultValue(Appearance.Light3D)]
		[Description("Determines the appearance of the headers in the Timeline")]
		public Appearance HeaderAppearance
		{
			get
			{
				return _headerAppearance;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Appearance), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_headerAppearance != value)
				{
					_headerAppearance = value;
					Invalidate();
				}
			}
		}

		[Description("Specifies which rule is used to determine the first calendar week of the year.")]
		[Category("Timescale")]
		[DefaultValue(CalendarWeekRule.FirstDay)]
		public CalendarWeekRule WeekRule
		{
			get
			{
				return _weekRule;
			}
			set
			{
				if (_weekRule != value)
				{
					_weekRule = value;
					Invalidate();
				}
			}
		}

		[Category("Working Time")]
		[Description("The first day of the week displayed in the DropDownCalendar.")]
		[DefaultValue(DayOfWeek.Sunday)]
		public DayOfWeek FirstDayOfWeek
		{
			get
			{
				return _firstDayOfWeek;
			}
			set
			{
				if (_firstDayOfWeek != value)
				{
					_firstDayOfWeek = value;
				}
			}
		}
	}
}
