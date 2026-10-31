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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("The drop-down Calendar control associated with this DatePicker control.")]
		[Editor(typeof(DropDownEditor), typeof(UITypeEditor))]
		[Category("Timescale")]
		public TimelineSurface DropDownCalendar => _dropDownCalendar;

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
				if (_timelineFormatStyleControl != null)
				{
					if (!_useDefaultMenu && _timelineFormatStyleControl._textBoxControl.ContextMenuStrip == GetContextMenu())
					{
						_timelineFormatStyleControl._textBoxControl.ContextMenuStrip = null;
					}
					else
					{
						_timelineFormatStyleControl._textBoxControl.ContextMenuStrip = GetContextMenu();
					}
				}
			}
		}

		[Description("Gets or sets the value of the application settings key for the current instance of the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string SettingsKey
		{
			get
			{
				if ((_settingsKey == null || _settingsKey.Length == 0) && base.Name != null && base.Name.Length > 0)
				{
					return base.Name;
				}
				if (_settingsKey == null)
				{
					return "";
				}
				return _settingsKey;
			}
			set
			{
				_settingsKey = value;
			}
		}

		DateTime ITimelineWorkingHoursOwner.MinDate => _minDate;

		[Description("Indicates whether edits are allowed")]
		[DefaultValue(true)]
		[Category("Behavior")]
		public bool AllowEdit
		{
			get
			{
				return _allowEdit;
			}
			set
			{
				if (_allowEdit != value)
				{
					_allowEdit = value;
					TryCommitItemEdit();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Specifyng whether the item is forced to be moved only on intervals while is being dragged.")]
		[Category("Behavior")]
		public bool ForceDropOnIntervals
		{
			get
			{
				return _forceDropOnIntervals;
			}
			set
			{
				_forceDropOnIntervals = value;
			}
		}

		[DefaultValue(AllowItemDrag.All)]
		[Category("Behavior")]
		[TypeConverter(typeof(FlagsEnumConverter))]
		[Description("Indicates which options the user has when dragging items.")]
		[Editor(typeof(FlagsEnumEditor), typeof(UITypeEditor))]
		public AllowItemDrag AllowItemDrag
		{
			get
			{
				return _allowItemDrag;
			}
			set
			{
				_allowItemDrag = value;
			}
		}

		[Description("Indicates whether the DropDrowCalendar should be displayed when the user press the timescale.")]
		[Category("Timescale")]
		[DefaultValue(true)]
		public bool DropDownNavigator
		{
			get
			{
				return _dropDownNavigator;
			}
			set
			{
				_dropDownNavigator = value;
			}
		}

		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
		[AttributeProvider(typeof(IListSource))]
		[DefaultValue(null)]
		[Category("Data")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[Description("Represent the data source that the Timeline is displaying data for")]
		public object DataSource
		{
			get
			{
				return _dataSource;
			}
			set
			{
				SetUnbound(value == null);
				if (_dataSource is IComponent && _dataSource != value)
				{
					((IComponent)_dataSource).Disposed -= OnDataSourceDisposed;
				}
				bool isDataSourceChanged = _dataSource != value;
				ApplyDataBinding(value, _bindingMemberInfo);
				_dataSource = value;
				if (_dataSource is IComponent && isDataSourceChanged)
				{
					((IComponent)_dataSource).Disposed += OnDataSourceDisposed;
				}
				if (value == null)
				{
					DataMember = "";
				}
			}
		}

		[Description("Specifies whether the current date-time will be highlighted")]
		[DefaultValue(false)]
		public bool HighlightCurrentTime
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
		[DefaultValue(null)]
		public TimelineItem CurrentItem
		{
			get
			{
				return _currentItem;
			}
			set
			{
				if (value != null && !_items.Contains(value))
				{
					throw new ArgumentException("The value must be in the Items collection");
				}
				SetFocusZone(FocusZone.Items);
				_focusedRowLayout = null;
				_currentItem = value;
				if (value != null)
				{
					EnsureVisible(value);
					SelectItemOnClick(value);
				}
				if (value != null)
				{
					_focusedRowLayout = GetRowLayoutAt((int)_currentItem._rectangleF.Location.X, (int)_currentItem._rectangleF.Location.Y);
				}
				else
				{
					Invalidate();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[DefaultValue(null)]
		public TimelineLayout CurrentLayout
		{
			get
			{
				return _currentLayout;
			}
			set
			{
				if (value != null && value.GetTimeline() != this)
				{
					throw new InvalidOperationException("The layout must belong to the Layouts collection of the control");
				}
				if (value != null && !value.CanBeLoaded())
				{
					throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
				}
				if (_currentLayout == value)
				{
					return;
				}
				CancelEventArgs e = new CancelEventArgs(cancel: false);
				OnCurrentLayoutChanging(e);
				if (!e.Cancel)
				{
					if (_currentLayout != null)
					{
						_currentLayout.SetTimeline(null);
						_currentLayout._isCurrentLayout = false;
					}
					_currentLayout = value;
					if (_currentLayout != null)
					{
						_currentLayout.SetTimeline(this);
					}
					if (!_isInitializing)
					{
						UpdateTimelineLayout();
						InvalidateRowsAndLayout();
					}
				}
			}
		}

		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public string LayoutData
		{
			get
			{
				if (_currentLayout != null)
				{
					_currentLayout.Update();
					return "";
				}
				return GetLayoutXml(PersistenceMode.Fields | PersistenceMode.StoredFilters | PersistenceMode.FilterCondition | PersistenceMode.GroupHeaderTotals | PersistenceMode.InternalImageList | PersistenceMode.FormatConditions | PersistenceMode.Groups | PersistenceMode.WorkingHourSchema);
			}
			set
			{
				if (value != null && value != "")
				{
					try
					{
						LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
						PropertyValue propertyValue = mordexLayoutReader["TimelineLayoutData"];
						DeserializeLayout(propertyValue.GetLayoutReader(), PersistenceMode.Fields | PersistenceMode.StoredFilters | PersistenceMode.FilterCondition | PersistenceMode.GroupHeaderTotals | PersistenceMode.InternalImageList | PersistenceMode.FormatConditions | PersistenceMode.Groups | PersistenceMode.WorkingHourSchema);
						return;
					}
					catch (Exception innerException)
					{
						throw new ArgumentException("LayoutData stream is invalid or corrupted.", "value", innerException);
					}
				}
				DeserializeLayout(new LayoutReader(), (PersistenceMode)0);
			}
		}

		[Category("Data")]
		[Description("The name of a list in a data source from which the control displays its data.")]
		[Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(UITypeEditor))]
		[DefaultValue("")]
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
					if (IsUnbound())
					{
						_bindingMemberInfo = new BindingMemberInfo(value);
					}
					ApplyDataBinding(_dataSource, new BindingMemberInfo(value));
				}
				catch (Exception)
				{
					_bindingMemberInfo = bindingMemberInfo;
					throw new InvalidOperationException();
				}
			}
		}

		[Category("Groups and Filter")]
		[Description("Specifies whether the last group in the Timeline can be expanded and collapsed.")]
		[DefaultValue(true)]
		public bool AllowExpandRowHeader
		{
			get
			{
				return _allowExpandRowHeader;
			}
			set
			{
				if (_allowExpandRowHeader != value)
				{
					_allowExpandRowHeader = value;
					InvalidateRows();
				}
			}
		}

		[DefaultValue(GroupMode.Default)]
		[Description("Determines whether row headers are collapsed or expanded.")]
		[Category("Groups and Filter")]
		public GroupMode RowHeaderGroupMode
		{
			get
			{
				return _rowHeaderGroupMode;
			}
			set
			{
				if (_rowHeaderGroupMode != value)
				{
					_rowHeaderGroupMode = value;
					InvalidateRowsAndLayout();
				}
			}
		}

		[Editor(typeof(DateTimeEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.Timeline.Design.DateTimeConverter))]
		[Description("The minimum allowable date.")]
		[Category("Dates")]
		public DateTime MinDate
		{
			get
			{
				return _minDate;
			}
			set
			{
				if (_minDate != value)
				{
					if (value > _maxDate)
					{
						throw new ArgumentOutOfRangeException("The MinDate must be lower or equal than the MaxDate");
					}
					if (value.Date < new DateTime(1, 2, 1))
					{
						throw new ArgumentOutOfRangeException(value.ToShortDateString() + " is not a valid value for 'MinDate'.  'MinDate' must be greater than or equal to 2/1/0001");
					}
					_minDate = value;
					if (_minDate > FirstDate)
					{
						FirstDate = _minDate;
					}
					_dropDownCalendar.UpdateMinDate(value);
					InvalidateRows();
					InvalidateLayout();
				}
			}
		}

		[Category("Dates")]
		[Description("The maximum allowable date")]
		public DateTime MaxDate
		{
			get
			{
				return _maxDate;
			}
			set
			{
				if (_maxDate != value)
				{
					if (value < _minDate)
					{
						throw new ArgumentOutOfRangeException("The MaxDate must be greater or equal than the MinDate");
					}
					if (value.Date > new DateTime(9998, 12, 31))
					{
						throw new ArgumentOutOfRangeException(value.ToShortDateString() + " is not a valid value for 'MaxDate'.  'MaxDate' must be lower than or equal to 12/31/9998");
					}
					_maxDate = value;
					if (_maxDate < _firstDate)
					{
						FirstDate = _maxDate;
					}
					if (value.Date != value)
					{
						_dropDownCalendar.UpdateMaxDate(value);
					}
					else
					{
						_dropDownCalendar.UpdateMaxDate(value.AddDays(-1.0));
					}
					InvalidateRows();
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(ItemLayoutMode.NewRowOnOverlap)]
		[Description("Indicates how the items will be positioned on the rows of the control.")]
		public ItemLayoutMode ItemLayoutMode
		{
			get
			{
				return _itemLayoutMode;
			}
			set
			{
				if (_itemLayoutMode != value)
				{
					_itemLayoutMode = value;
					InvalidateRows();
				}
			}
		}

		[DefaultValue(-1)]
		[Category("Timeline Items")]
		[Description("Indicates maximum number of characters displayed in an item.")]
		public int MaximumLabelLength
		{
			get
			{
				return _maximumLabelLength;
			}
			set
			{
				if (_maximumLabelLength != value)
				{
					_maximumLabelLength = value;
					InvalidateRows();
				}
			}
		}

		[Category("Behavior")]
		[Description("Indicates whether multiselect is allowed")]
		[DefaultValue(true)]
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

		[DefaultValue(GroupMode.Expanded)]
		[Category("Groups and Filter")]
		[Description("Determines whether groups are collapsed or expanded.")]
		public GroupMode GroupMode
		{
			get
			{
				return _groupMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(GroupMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == GroupMode.Default)
				{
					value = GroupMode.Expanded;
				}
				if (_groupMode != value)
				{
					_groupMode = value;
					if (value == GroupMode.Expanded)
					{
						GetTimelineDataSourceManager().GroupMode = Mordex.Data.GroupMode.Expanded;
					}
					else
					{
						GetTimelineDataSourceManager().GroupMode = Mordex.Data.GroupMode.Collapsed;
					}
					InvalidateRowsAndLayout();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		[Description("Indicates whether the Tab key is handled by the control to select items.")]
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

		IMordexTable ISupportFilterBuilder.RootTable => GetRootTable();
	}
}
