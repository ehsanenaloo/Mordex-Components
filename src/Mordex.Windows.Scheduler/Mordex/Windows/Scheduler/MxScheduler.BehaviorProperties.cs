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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		bool ITextBoxOwner.UseDefaultMenu
		{
			get
			{
				return _useDefaultMenu;
			}
			set
			{
				if (value == _useDefaultMenu)
				{
					return;
				}
				_useDefaultMenu = value;
				if (_appointmentEditor != null)
				{
					if (!_useDefaultMenu && _appointmentEditor._textBoxControl.ContextMenuStrip == GetContextMenu())
					{
						_appointmentEditor._textBoxControl.ContextMenuStrip = null;
					}
					else
					{
						_appointmentEditor._textBoxControl.ContextMenuStrip = GetContextMenu();
					}
				}
			}
		}

		DateTime ISchedulerWorkingHoursOwner.MinDate => MinDate;

		[Description("Specifies whether the current date-time will be highlighted")]
		[DefaultValue(TriState.Empty)]
		[Category("Behavior")]
		public TriState HighlightCurrentTime
		{
			get
			{
				return _highlightCurrentTime;
			}
			set
			{
				if (_highlightCurrentTime != value)
				{
					_highlightCurrentTime = value;
					UpdateCurrentTimeTimer();
					Invalidate();
				}
			}
		}

		[Browsable(false)]
		public int MinuteInterval => _minuteInterval;

		[Description("Specifies whether the Schedule should change its View property after clicking on the scroll button of the days.")]
		[DefaultValue(TriState.Empty)]
		public TriState ViewNavigationMode
		{
			get
			{
				return _viewNavigationMode;
			}
			set
			{
				if (_viewNavigationMode != value)
				{
					_viewNavigationMode = value;
				}
			}
		}

		[Description("Indicates whether the Tab key is handled by the control to select items.")]
		[Category("Behavior")]
		[DefaultValue(true)]
		public bool AcceptsTab
		{
			get
			{
				return _acceptsTab;
			}
			set
			{
				_acceptsTab = value;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether deletes of appointments are allowed when the user press the delete key")]
		[Category("Behavior")]
		public bool AllowDelete
		{
			get
			{
				return _allowDelete;
			}
			set
			{
				_allowDelete = value;
			}
		}

		[Description("Indicates whether multiselect is allowed")]
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool MultiSelect
		{
			get
			{
				return _multiSelect;
			}
			set
			{
				_multiSelect = value;
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the user can drag the appoinments over non working times")]
		[Category("Behavior")]
		public bool AllowDragInNonWorkingTime
		{
			get
			{
				return _allowDragInNonWorkingTime;
			}
			set
			{
				_allowDragInNonWorkingTime = value;
			}
		}

		[Description("Indicates which options the user has when dragging appointments.")]
		[Category("Behavior")]
		[TypeConverter(typeof(FlagsEnumConverter))]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		[DefaultValue(AllowAppointmentDrag.All)]
		public AllowAppointmentDrag AllowAppointmentDrag
		{
			get
			{
				return _allowAppointmentDrag;
			}
			set
			{
				if (_allowAppointmentDrag == value)
				{
					return;
				}
				_allowAppointmentDrag = value;
				foreach (SchedulerAppointmentView item in _appointmentViews)
				{
					item.SetAllowAppointmentDrag(value);
				}
				if (_view != SchedulerView.DayView)
				{
					return;
				}
				foreach (SchedulerAppointmentView allDayView in _allDayAppointmentViews)
				{
					allDayView.SetAllowAppointmentDrag(value);
				}
			}
		}

		[DefaultValue(true)]
		[Category("Behavior")]
		[Description("Indicates whether edits are allowed")]
		public bool AllowEdit
		{
			get
			{
				return _allowEdit;
			}
			set
			{
				if (_allowEdit == value)
				{
					return;
				}
				_allowEdit = value;
				foreach (SchedulerAppointmentView item in _appointmentViews)
				{
					item.SetAllowEdit(value);
				}
				if (_view != SchedulerView.DayView)
				{
					return;
				}
				foreach (SchedulerAppointmentView allDayView in _allDayAppointmentViews)
				{
					allDayView.SetAllowEdit(value);
				}
			}
		}

		[Description("Determines whether the user can resize day columns when in day or work week view.")]
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool AllowDayResize
		{
			get
			{
				return _allowDayResize;
			}
			set
			{
				_allowDayResize = value;
			}
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[Category("Data")]
		[DefaultValue(null)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
		[Description("Represent the data source that the Schedule is displaying data for")]
		[AttributeProvider(typeof(IListSource))]
		public object DataSource
		{
			get
			{
				return _dataSource;
			}
			set
			{
				SetUnboundMode(value == null);
				if (_dataSource is IComponent && _dataSource != value)
				{
					((IComponent)_dataSource).Disposed -= OnDataSourceDisposed;
				}
				bool isDataSourceChanged = _dataSource != value;
				if (value == null)
				{
					_dataSource = null;
				}
				BindDataSource(value, _bindingMemberInfo, false);
				if (_dataSource is IComponent && isDataSourceChanged)
				{
					((IComponent)_dataSource).Disposed += OnDataSourceDisposed;
				}
				OnDataSourceChanged(EventArgs.Empty);
				if (value == null)
				{
					DataMember = "";
				}
			}
		}

		[Description("The name of a list in a data source from which the control displays its data.")]
		[Category("Data")]
		[DefaultValue("")]
		[Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(UITypeEditor))]
		public string DataMember
		{
			get
			{
				return _bindingMemberInfo.BindingMember;
			}
			set
			{
				BindingMemberInfo bindingMemberInfo = _bindingMemberInfo;
				try
				{
					if (IsUnboundMode())
					{
						_bindingMemberInfo = new BindingMemberInfo(value);
					}
					BindDataSource(_dataSource, new BindingMemberInfo(value), false);
				}
				catch
				{
					_bindingMemberInfo = bindingMemberInfo;
					throw;
				}
			}
		}

		[DefaultValue(null)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public SchedulerLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.GetScheduler() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the control");
				}
				if (value != null && !value.CanLoad())
				{
					throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
				}
				if (_currentLayout == value)
				{
					return;
				}
				_isAppointmentCacheStale = true;
				_isFullLayoutStale = true;
				_isAppointmentLayoutStale = true;
				CancelEventArgs e = new CancelEventArgs(cancel: false);
				OnCurrentLayoutChanging(e);
				if (!e.Cancel)
				{
					if (_currentLayout != null)
					{
						_currentLayout.SetSchedule(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.SetSchedule(this);
					}
					if (!_isInitializing)
					{
						ApplyCurrentLayout();
					}
				}
			}
		}

		[Browsable(false)]
		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string LayoutData
		{
			get
			{
				if (_currentLayout != null)
				{
					_currentLayout.Update();
					return "";
				}
				return GetLayoutString(PersistenceMode.Fields | PersistenceMode.FormatConditions | PersistenceMode.StoredFilters | PersistenceMode.FilterCondition | PersistenceMode.InternalImageList | PersistenceMode.Dates);
			}
			set
			{
				if (value != null && value != "")
				{
					try
					{
						LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
						PropertyValue propertyValue = mordexLayoutReader["SchedulerLayoutData"];
						DeserializeSchedulerState(propertyValue.GetLayoutReader(), PersistenceMode.Fields | PersistenceMode.FormatConditions | PersistenceMode.StoredFilters | PersistenceMode.FilterCondition | PersistenceMode.InternalImageList | PersistenceMode.Dates);
						return;
					}
					catch (Exception innerException)
					{
						throw new ArgumentException("LayoutData stream is invalid or corrupted.", "value", innerException);
					}
				}
				DeserializeSchedulerState(new LayoutReader(), (PersistenceMode)0);
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		public SchedulerAppointment CurrentAppointment
		{
			get
			{
				return _currentAppointment;
			}
			set
			{
				_currentAppointment = value;
				_schedulerSelectedAppointments.Clear();
				CommitEdit();
				if (!_publicAppointments.Contains(value))
				{
					return;
				}
				_currentAppointment.Selected = true;
				if (_currentAppointment.EndTime.TimeOfDay < FirstVisibleTime || _currentAppointment.StartTime.TimeOfDay > LastVisibleTime)
				{
					EnsureVisible(_currentAppointment);
				}
				RenderCurrentView(null);
				if (_allowEdit && _currentAppointment != null)
				{
					SchedulerAppointmentView appointmentView = FindAppointmentView(_currentAppointment);
					if (appointmentView != null)
					{
						ShowEditor(appointmentView, _currentAppointment.Text);
					}
				}
			}
		}

		[DefaultValue(HiddenAppointmentMode.Automatic)]
		[Description("The behavior of the label indicating more appointments")]
		[Category("Behavior")]
		public HiddenAppointmentMode HiddenAppointmentMode
		{
			get
			{
				return _hiddenAppointmentMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(HiddenAppointmentMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_hiddenAppointmentMode != value)
				{
					_hiddenAppointmentMode = value;
					_isAppointmentLayoutStale = true;
					Invalidate();
				}
			}
		}

		[Category("Dates/Times")]
		[Localizable(true)]
		[Description("The maximum allowable date")]
		public DateTime MaxDate
		{
			get
			{
				return _maxDate;
			}
			set
			{
				if (!(_maxDate != value))
				{
					return;
				}
				if (value.Date < _minDate)
				{
					throw new ArgumentOutOfRangeException("The MaxDate must be greater or equal than the MinDate");
				}
				if (value.Date > new DateTime(9998, 12, 31))
				{
					throw new ArgumentOutOfRangeException(value.ToShortDateString() + " is not a valid value for 'MaxDate'.  'MaxDate' must be lower than or equal to 12/31/9998");
				}
				_maxDate = value.Date;
				OnMaxDateChanged(EventArgs.Empty);
				int totalDays = (int)_maxDate.Subtract(_minDate).TotalDays;
				if (totalDays / 7 < _weeks)
				{
					int weekCount = totalDays / 7;
					if (totalDays % 7 != 0)
					{
						weekCount++;
					}
					Weeks = weekCount;
				}
				if (_maxDate < _date)
				{
					Date = _maxDate;
				}
				else
				{
					InvalidateAppointmentLayout();
				}
			}
		}

		[Editor(typeof(DateTimeEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[Description("The minimum allowable date")]
		[TypeConverter(typeof(System.ComponentModel.DateTimeConverter))]
		[Category("Dates/Times")]
		public DateTime MinDate
		{
			get
			{
				return _minDate;
			}
			set
			{
				if (!(_minDate != value))
				{
					return;
				}
				if (value.Date > _maxDate)
				{
					throw new ArgumentOutOfRangeException("The MinDate must be lower or equal than the MaxDate");
				}
				if (value.Date < new DateTime(1, 2, 1))
				{
					throw new ArgumentOutOfRangeException(value.ToShortDateString() + " is not a valid value for 'MinDate'.  'MinDate' must be greater than or equal to 2/1/0001");
				}
				_minDate = value.Date;
				OnMinDateChanged(EventArgs.Empty);
				int totalDays = (int)_maxDate.Subtract(_minDate).TotalDays;
				if (totalDays / 7 < _weeks)
				{
					int weekCount = totalDays / 7;
					if (totalDays % 7 != 0)
					{
						weekCount++;
					}
					Weeks = weekCount;
				}
				if (_minDate > _date)
				{
					Date = _minDate;
				}
				else
				{
					InvalidateAppointmentLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		// Null with MultiOwner off shows the appointments of all owners.
		public SchedulerAppointmentOwner CurrentOwner
		{
			get
			{
				return _currentOwner;
			}
			set
			{
				if (_currentOwner == value)
				{
					return;
				}
				if (IsMultiOwnerActive())
				{
					if (value == null)
					{
						throw new InvalidOperationException("The current owner can not be null in multi owner mode");
					}
					if (!_owners.Contains(value))
					{
						throw new InvalidOperationException("The current owner must belong to the control");
					}
				}
				else if (value != null && !value.Visible)
				{
					throw new InvalidOperationException("The current owner must be visible");
				}
				_currentOwner = value;
				if (!_isAppointmentDragActive && _shouldClearSelectionOnOwnerChange)
				{
					_schedulerSelectedAppointments.Clear();
				}
				RefreshOwners(false);
				OnCurrentOwnerChanged(EventArgs.Empty);
			}
		}

		[Description("Specifies the interval in which the hour is divided")]
		[DefaultValue(Interval.ThirtyMinutes)]
		[Category("Dates/Times")]
		[Localizable(true)]
		public Interval Interval
		{
			get
			{
				return _interval;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Interval), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_interval != value)
				{
					if (_view == SchedulerView.DayView)
					{
						_minuteInterval = 60 / (int)value;
						AlignSelectionToSlots();
						_interval = value;
						InvalidateAppointmentLayout();
						RenderCurrentView(null);
						FirstVisibleTime = ClampToVisibleHours(FirstVisibleTime);
					}
					else
					{
						_interval = value;
						_minuteInterval = 60 / (int)_interval;
					}
					OnIntervalChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(-1)]
		[Category("Behavior")]
		[Description("Represents the maximum number of rows allowed in the all-day area.")]
		public int MaxAllDayRows
		{
			get
			{
				return _maxAllDayRows;
			}
			set
			{
				if (_maxAllDayRows != value)
				{
					_maxAllDayRows = value;
					InvalidateAppointmentLayout();
				}
			}
		}

		[DefaultValue(AddNewMode.Automatic)]
		[Category("Behavior")]
		[Description("Specifies whether the Schedule control should add appointments in response to user interaction.")]
		public AddNewMode AddNewMode
		{
			get
			{
				return _addNewMode;
			}
			set
			{
				_addNewMode = value;
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether the control has multiple appointment owners.")]
		[DefaultValue(false)]
		public bool MultiOwner
		{
			get
			{
				return _multiOwner;
			}
			set
			{
				if (_multiOwner != value)
				{
					_multiOwner = value;
					RefreshOwners(false);
				}
			}
		}

		[Description("Gets or sets the value of the application settings key for the current instance of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string SettingsKey
		{
			get
			{
				if (_settingsKey.Length == 0 && base.Name != null && base.Name.Length > 0)
				{
					return base.Name;
				}
				return _settingsKey;
			}
			set
			{
				_settingsKey = value;
			}
		}
	}
}
