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
		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_isFullSerialization)
			{
				SerializeTimelineState(info);
				if (_layouts.Count > 0)
				{
					info.AddValue("Layouts", _layouts);
				}
			}
			else
			{
				info.AddValue("Fields", Fields);
				info.AddValue("Items", _items);
			}
		}

		internal void SerializeTimelineState(LayoutWriter mordexLayoutWriter)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.Fields) == 0)
				{
					SerializeFields(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.Groups) == 0)
				{
					SerializeGroups(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.WorkingHourSchema) != 0)
				{
					SerializeWorkingHourSchema(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.GroupHeaderTotals) == 0)
				{
					SerializeGroupHeaderTotals(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.StoredFilters) == 0)
				{
					SerializeStoredFilters(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.FilterCondition) == 0)
				{
					SerializeFilterCondition(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.InternalImageList) == 0)
				{
					SerializeInternalImageList(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.AppearanceProperties) == 0)
				{
					SerializeAppearanceProperties(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.OtherProperties) == 0)
				{
					SerializeOtherProperties(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.FormatConditions) == 0)
				{
					SerializeFormatConditions(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.DropDownCalendar) == 0)
				{
					SerializeDropDownCalendar(mordexLayoutWriter);
				}
			}
			else
			{
				SerializeFields(mordexLayoutWriter);
				SerializeGroups(mordexLayoutWriter);
				SerializeWorkingHourSchema(mordexLayoutWriter);
				SerializeGroupHeaderTotals(mordexLayoutWriter);
				SerializeStoredFilters(mordexLayoutWriter);
				SerializeFilterCondition(mordexLayoutWriter);
				SerializeInternalImageList(mordexLayoutWriter);
				SerializeAppearanceProperties(mordexLayoutWriter);
				SerializeOtherProperties(mordexLayoutWriter);
				SerializeFormatConditions(mordexLayoutWriter);
				SerializeDropDownCalendar(mordexLayoutWriter);
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			if (_isFullDeserialization)
			{
				DeserializeLayout(info, (PersistenceMode)0);
			}
			else
			{
				if (_overwriteFields)
				{
					Groups.Clear();
					FilterCondition = null;
					GroupHeaderTotals.Clear();
					StoredFilters.Clear();
					FormatConditions.Clear();
					info.DeserializeObject("Fields", Fields);
				}
				DisposeUnboundTable();
				ApplyDataBinding(null, new BindingMemberInfo(""));
				PropertyValue propertyValue = info["Items"];
				propertyValue.DeserializeObject(_items);
			}
			InvalidateRows();
		}

		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				if (_isInitializing)
				{
					MemoryStream memoryStream = new MemoryStream();
					SaveLayoutFile(memoryStream);
					byte[] defaultLayoutSettings = memoryStream.ToArray();
					GetApplicationSettings().DefaultLayoutSettings = defaultLayoutSettings;
					memoryStream.Close();
					memoryStream.Dispose();
					memoryStream = null;
				}
				byte[] layoutSettings = GetApplicationSettings().LayoutSettings;
				if (layoutSettings != null && layoutSettings.Length > 0)
				{
					MemoryStream layoutStream = new MemoryStream(layoutSettings);
					LoadLayoutFile(layoutStream);
					layoutStream.Close();
					layoutStream.Dispose();
					layoutStream = null;
				}
			}
		}

		public void SaveComponentSettings()
		{
			MemoryStream memoryStream = new MemoryStream();
			SaveLayoutFile(memoryStream);
			byte[] layoutSettings = memoryStream.ToArray();
			GetApplicationSettings().LayoutSettings = layoutSettings;
			GetApplicationSettings().Save();
			memoryStream.Close();
			memoryStream.Dispose();
			memoryStream = null;
		}

		internal void DeserializeLayout(LayoutReader mordexLayoutReader, PersistenceMode persistenceMode)
		{
			bool shouldLoadFields = true;
			bool shouldLoadInternalImageList = true;
			bool shouldLoadAppearanceProperties = true;
			bool shouldLoadOtherProperties = true;
			bool shouldLoadStoredFilters = true;
			bool shouldLoadFilterCondition = true;
			bool shouldLoadGroupHeaderTotals = true;
			bool shouldLoadDropDownCalendar = true;
			bool shouldLoadFormatConditions = true;
			bool shouldLoadGroups = true;
			bool shouldLoadWorkingHourSchema = true;
			if (persistenceMode != 0)
			{
				shouldLoadFields = (persistenceMode & PersistenceMode.Fields) != 0;
				shouldLoadInternalImageList = (persistenceMode & PersistenceMode.InternalImageList) != 0;
				shouldLoadAppearanceProperties = (persistenceMode & PersistenceMode.AppearanceProperties) != 0;
				shouldLoadOtherProperties = (persistenceMode & PersistenceMode.OtherProperties) != 0;
				shouldLoadStoredFilters = (persistenceMode & PersistenceMode.StoredFilters) != 0;
				shouldLoadFilterCondition = (persistenceMode & PersistenceMode.FilterCondition) != 0;
				shouldLoadGroupHeaderTotals = (persistenceMode & PersistenceMode.GroupHeaderTotals) != 0;
				shouldLoadFormatConditions = (persistenceMode & PersistenceMode.FormatConditions) != 0;
				shouldLoadDropDownCalendar = (persistenceMode & PersistenceMode.DropDownCalendar) != 0;
				shouldLoadGroups = (persistenceMode & PersistenceMode.Groups) != 0;
				shouldLoadWorkingHourSchema = (persistenceMode & PersistenceMode.WorkingHourSchema) != 0;
			}
			if (shouldLoadFields)
			{
				FilterCondition = null;
				Groups.Clear();
				GroupHeaderTotals.Clear();
				StoredFilters.Clear();
				FormatConditions.Clear();
				mordexLayoutReader.DeserializeObject("Fields", Fields);
			}
			if (shouldLoadGroups)
			{
				mordexLayoutReader.DeserializeObject("Groups", Groups);
			}
			if (shouldLoadWorkingHourSchema)
			{
				PropertyValue propertyValue = mordexLayoutReader["WorkingHourSchema"];
				_workingHourSchema = new WorkingHoursSchema(this);
				propertyValue?.DeserializeObject(_workingHourSchema);
			}
			if (shouldLoadGroupHeaderTotals)
			{
				PropertyValue groupHeaderTotalsValue = mordexLayoutReader["GroupHeaderTotals"];
				GroupHeaderTotals.Clear();
				groupHeaderTotalsValue?.DeserializeObject(GroupHeaderTotals);
			}
			if (shouldLoadStoredFilters)
			{
				PropertyValue storedFiltersValue = mordexLayoutReader["StoredFilters"];
				StoredFilters.Clear();
				if (storedFiltersValue != null)
				{
					mordexLayoutReader.DeserializeObject("StoredFilters", StoredFilters);
				}
			}
			if (shouldLoadFilterCondition)
			{
				PropertyValue filterConditionValue = mordexLayoutReader["FilterCondition"];
				if (filterConditionValue != null)
				{
					TimelineFilterCondition timelineFilterCondition = new TimelineFilterCondition();
					timelineFilterCondition.SetConditionOwner((IConditionOwner)_timelineDataSourceManager.RootTable);
					filterConditionValue.DeserializeObject(timelineFilterCondition);
					_timelineDataSourceManager.RootTable.FilterCondition = timelineFilterCondition;
				}
				else
				{
					foreach (TimelineStoredFilter storedFilter in StoredFilters)
					{
						if (storedFilter.GetBaseIsCurrent())
						{
							_timelineDataSourceManager.RootTable.FilterCondition = storedFilter;
							break;
						}
					}
				}
			}
			if (shouldLoadInternalImageList)
			{
				DeserializeInternalImageList(mordexLayoutReader);
			}
			if (shouldLoadAppearanceProperties)
			{
				DeserializeAppearanceProperties(mordexLayoutReader);
			}
			if (shouldLoadOtherProperties)
			{
				DeserializeOtherProperties(mordexLayoutReader);
			}
			if (shouldLoadFormatConditions)
			{
				PropertyValue formatConditionsValue = mordexLayoutReader["FormatConditions"];
				_formatConditions = new TimelineFormatConditionCollection(this);
				formatConditionsValue?.DeserializeObject(_formatConditions);
			}
			if (shouldLoadDropDownCalendar)
			{
				mordexLayoutReader["DropDownCalendar"]?.DeserializeObject(_dropDownCalendar);
			}
			if (shouldLoadFields)
			{
				OnRootTableChanged(EventArgs.Empty);
			}
		}

		private void SerializeGroups(LayoutWriter mordexLayoutWriter)
		{
			if (Groups.Count > 0)
			{
				mordexLayoutWriter.AddValue("Groups", Groups);
			}
		}

		private void SerializeWorkingHourSchema(LayoutWriter mordexLayoutWriter)
		{
			if (_workingHourSchema.Exceptions.Count > 0 || _workingHourSchema.WorkingHoursRange.Count > 0)
			{
				mordexLayoutWriter.AddValue("WorkingHourSchema", _workingHourSchema);
			}
		}

		private void SerializeFields(LayoutWriter mordexLayoutWriter)
		{
			if (Fields.Count > 0)
			{
				mordexLayoutWriter.AddValue("Fields", Fields);
			}
		}

		private void SerializeFormatConditions(LayoutWriter mordexLayoutWriter)
		{
			if (_formatConditions.Count > 0)
			{
				mordexLayoutWriter.AddValue("FormatConditions", _formatConditions);
			}
		}

		private void SerializeGroupHeaderTotals(LayoutWriter mordexLayoutWriter)
		{
			if (GroupHeaderTotals.Count > 0)
			{
				mordexLayoutWriter.AddValue("GroupHeaderTotals", GroupHeaderTotals);
			}
		}

		private void SerializeStoredFilters(LayoutWriter mordexLayoutWriter)
		{
			if (StoredFilters.Count > 0)
			{
				mordexLayoutWriter.AddValue("StoredFilters", StoredFilters);
			}
		}

		private void SerializeFilterCondition(LayoutWriter mordexLayoutWriter)
		{
			if (FilterCondition != null && FilterCondition is TimelineFilterCondition)
			{
				mordexLayoutWriter.AddValue("FilterCondition", FilterCondition);
			}
		}

		internal void SerializeLayout(LayoutWriter mordexLayoutWriter, PersistenceMode persistenceMode)
		{
			if ((persistenceMode & PersistenceMode.Fields) != 0)
			{
				SerializeFields(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.Groups) != 0)
			{
				SerializeGroups(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.WorkingHourSchema) != 0)
			{
				SerializeWorkingHourSchema(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.GroupHeaderTotals) != 0)
			{
				SerializeGroupHeaderTotals(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.StoredFilters) != 0)
			{
				SerializeStoredFilters(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.FilterCondition) != 0)
			{
				SerializeFilterCondition(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.FormatConditions) != 0)
			{
				SerializeFormatConditions(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.InternalImageList) != 0)
			{
				SerializeInternalImageList(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.AppearanceProperties) != 0)
			{
				SerializeAppearanceProperties(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.OtherProperties) != 0)
			{
				SerializeOtherProperties(mordexLayoutWriter);
			}
		}

		private void DeserializeInternalImageList(LayoutReader reader)
		{
			if (_internalImageList != null)
			{
				_internalImageList = null;
			}
			_internalImageList = new ImageList();
			_internalImageList.ColorDepth = (ColorDepth)LayoutReader.GetPropertyValue(reader, "ImageList.ColorDepth", typeof(ColorDepth), _internalImageList.ColorDepth);
			_internalImageList.ImageSize = (Size)LayoutReader.GetPropertyValue(reader, "ImageList.ImageSize", typeof(Size), _internalImageList.ImageSize);
			ImageStrip.FromStrip(_internalImageList, (Image)LayoutReader.GetPropertyValue(reader, "ImageList.ImageStrip", typeof(Image), null));
			_internalImageList.TransparentColor = (Color)LayoutReader.GetPropertyValue(reader, "ImageList.TransparentColor", typeof(Color), _internalImageList.TransparentColor);
		}

		private void SerializeInternalImageList(LayoutWriter mordexLayoutWriter)
		{
			if (_internalImageList != null && _internalImageList.Images.Count > 0)
			{
				mordexLayoutWriter.AddValue("ImageList.ColorDepth", _internalImageList.ColorDepth);
				mordexLayoutWriter.AddValue("ImageList.ImageSize", _internalImageList.ImageSize);
				mordexLayoutWriter.AddValue("ImageList.ImageStrip", ImageStrip.ToStrip(_internalImageList));
				mordexLayoutWriter.AddValue("ImageList.TransparentColor", _internalImageList.TransparentColor);
			}
		}

		private void DeserializeOtherProperties(LayoutReader reader)
		{
			_isRefreshEnabled = false;
			try
			{
				ReadOtherProperties(reader);
			}
			finally
			{
				_isRefreshEnabled = true;
			}
		}

		private void ReadOtherProperties(LayoutReader reader)
		{
			_acceptsTab = (bool)LayoutReader.GetPropertyValue(reader, "AcceptsTab", typeof(bool), true);
			_allowEdit = (bool)LayoutReader.GetPropertyValue(reader, "AllowEdit", typeof(bool), true);
			_allowItemDrag = (AllowItemDrag)LayoutReader.GetPropertyValue(reader, "AllowItemDrag", typeof(AllowItemDrag), AllowItemDrag.All);
			_dropDownNavigator = (bool)LayoutReader.GetPropertyValue(reader, "DropDownNavigator", typeof(bool), true);
			_firstDayOfWeek = (DayOfWeek)LayoutReader.GetPropertyValue(reader, "FirstDayOfWeek", typeof(DayOfWeek), DayOfWeek.Sunday);
			_hideSelection = (HideSelection)LayoutReader.GetPropertyValue(reader, "HideSelection", typeof(HideSelection), HideSelection.HighlightInactive);
			_imageSource = (ImageSource)LayoutReader.GetPropertyValue(reader, "ImageSource", typeof(ImageSource), ImageSource.ExternalImageList);
			_itemsPrefix = (string)LayoutReader.GetPropertyValue(reader, "ItemsPrefix", typeof(string), string.Empty);
			_itemsSuffix = (string)LayoutReader.GetPropertyValue(reader, "ItemsSuffix", typeof(string), string.Empty);
			_minDate = (DateTime)LayoutReader.GetPropertyValue(reader, "MinDate", typeof(DateTime), DateTime.MinValue.AddMonths(1));
			_maxDate = (DateTime)LayoutReader.GetPropertyValue(reader, "MaxDate", typeof(DateTime), DateTime.MaxValue.AddYears(-1));
			if (_dropDownCalendar != null)
			{
				// The calendar checks each bound against the other, so the bound that moves outward goes first.
				if (_minDate.Date > _dropDownCalendar.GetMaxDate())
				{
					_dropDownCalendar.UpdateMaxDate(_maxDate);
					_dropDownCalendar.UpdateMinDate(_minDate);
				}
				else
				{
					_dropDownCalendar.UpdateMinDate(_minDate);
					_dropDownCalendar.UpdateMaxDate(_maxDate);
				}
			}
			_forceDropOnIntervals = (bool)LayoutReader.GetPropertyValue(reader, "ForceDropOnIntervals", typeof(bool), true);
			base.MouseWheelBehavior = (MouseWheelBehavior)LayoutReader.GetPropertyValue(reader, "MouseWheelBehavior", typeof(MouseWheelBehavior), MouseWheelBehavior.Automatic);
			base.VerticalScrollBar = (VerticalScrollBar)LayoutReader.GetPropertyValue(reader, "VerticalScrollBar", typeof(VerticalScrollBar), VerticalScrollBar.Automatic);
			_weekRule = (CalendarWeekRule)LayoutReader.GetPropertyValue(reader, "WeekRule", typeof(CalendarWeekRule), CalendarWeekRule.FirstDay);
			object propertyValue = LayoutReader.GetPropertyValue(reader, "UseCompatibleTextRendering", typeof(bool), null);
			if (propertyValue != null)
			{
				_useCompatibleTextRendering = (bool)propertyValue;
			}
			_yearText = (string)LayoutReader.GetPropertyValue(reader, "YearText", typeof(string), "Year");
			_weekText = (string)LayoutReader.GetPropertyValue(reader, "WeekText", typeof(string), "Week");
			_showWeekNumbers = (bool)LayoutReader.GetPropertyValue(reader, "ShowWeekNumbers", typeof(bool), false);
			_itemLayoutMode = (ItemLayoutMode)LayoutReader.GetPropertyValue(reader, "ItemLayoutMode", typeof(ItemLayoutMode), ItemLayoutMode.NewRowOnOverlap);
			_itemsDurationBarHeight = (int)LayoutReader.GetPropertyValue(reader, "ItemsDurationBarHeight", typeof(int), 7);
			_highlightCurrentTime = (bool)LayoutReader.GetPropertyValue(reader, "HighlightCurrentTime", typeof(bool), false);
			_itemToolTip = (ItemToolTip)LayoutReader.GetPropertyValue(reader, "ItemToolTip", typeof(ItemToolTip), ItemToolTip.UseToolTipText);
			_toolTipDelay = (int)LayoutReader.GetPropertyValue(reader, "ToolTipDelay", typeof(int), 5000);
			_itemSizeMode = (ItemSizeMode)LayoutReader.GetPropertyValue(reader, "ItemSizeMode", typeof(ItemSizeMode), ItemSizeMode.TextBased);
			TimescaleTiers = (TimescaleTiers)LayoutReader.GetPropertyValue(reader, "TimescaleTiers", typeof(TimescaleTiers), TimescaleTiers.TwoTiers);
			_bottomTier = new Timescale(this, _bottomTier.GetDefaultFont(), TimelineInterval.Day);
			reader["BottomTier"]?.DeserializeObject(_bottomTier);
			_middleTier = new Timescale(this, _middleTier.GetDefaultFont(), TimelineInterval.Week);
			reader["MiddleTier"]?.DeserializeObject(_middleTier);
			_topTier = new Timescale(this, _topTier.GetDefaultFont(), TimelineInterval.Month);
			reader["TopTier"]?.DeserializeObject(_topTier);
			InvalidateTimescale();
			_workStartTime = (TimeSpan)LayoutReader.GetPropertyValue(reader, "WorkStartTime", typeof(TimeSpan), new TimeSpan(8, 0, 0));
			_workEndTime = (TimeSpan)LayoutReader.GetPropertyValue(reader, "WorkEndTime", typeof(TimeSpan), new TimeSpan(17, 0, 0));
			DayOfWeek dayOfWeek = DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday;
			_workWeek = (DayOfWeek)LayoutReader.GetPropertyValue(reader, "WorkWeek", typeof(DayOfWeek), dayOfWeek);
			_allowExpandRowHeader = (bool)LayoutReader.GetPropertyValue(reader, "AllowExpandRowHeader", typeof(bool), true);
			_rowHeaderGroupMode = (GroupMode)LayoutReader.GetPropertyValue(reader, "RowHeaderGroupMode", typeof(GroupMode), GroupMode.Default);
			_rowHeaderWidth = (int)LayoutReader.GetPropertyValue(reader, "RowHeaderWidth", typeof(int), 120);
			_showLastGroupAsRowHeader = (bool)LayoutReader.GetPropertyValue(reader, "ShowLastGroupAsRowHeader", typeof(bool), false);
			_multiSelect = (bool)LayoutReader.GetPropertyValue(reader, "MultiSelect", typeof(bool), true);
			_ownerDrawnArea = (OwnerDrawnArea)LayoutReader.GetPropertyValue(reader, "OwnerDrawnArea", typeof(OwnerDrawnArea), OwnerDrawnArea.None);
			_timelineRowHeight = (int)LayoutReader.GetPropertyValue(reader, "RowHeight", typeof(int), -1);
			_minItemWidth = (int)LayoutReader.GetPropertyValue(reader, "MinItemWidth", typeof(int), 20);
			_timeFormat = (TimeFormat)LayoutReader.GetPropertyValue(reader, "TimeFormat", typeof(TimeFormat), TimeFormat.TwelveHours);
		}

		private void SerializeOtherProperties(LayoutWriter mordexLayoutWriter)
		{
			if (!AcceptsTab)
			{
				mordexLayoutWriter.AddValue("AcceptsTab", _acceptsTab);
			}
			if (AllowItemDrag != AllowItemDrag.All)
			{
				mordexLayoutWriter.AddValue("AllowItemDrag", _allowItemDrag);
			}
			if (!DropDownNavigator)
			{
				mordexLayoutWriter.AddValue("DropDownNavigator", _dropDownNavigator);
			}
			if (!AllowEdit)
			{
				mordexLayoutWriter.AddValue("AllowEdit", _allowEdit);
			}
			if (_firstDayOfWeek != DayOfWeek.Sunday)
			{
				mordexLayoutWriter.AddValue("FirstDayOfWeek", _firstDayOfWeek);
			}
			if (_hideSelection != HideSelection.HighlightInactive)
			{
				mordexLayoutWriter.AddValue("HideSelection", _hideSelection);
			}
			if (_imageSource != ImageSource.ExternalImageList)
			{
				mordexLayoutWriter.AddValue("ImageSource", _imageSource);
			}
			if (_itemsPrefix != string.Empty)
			{
				mordexLayoutWriter.AddValue("ItemsPrefix", _itemsPrefix);
			}
			if (_itemsSuffix != string.Empty)
			{
				mordexLayoutWriter.AddValue("ItemsSuffix", _itemsSuffix);
			}
			if (ShouldSerializeMinDate())
			{
				mordexLayoutWriter.AddValue("MinDate", _minDate);
			}
			if (ShouldSerializeMaxDate())
			{
				mordexLayoutWriter.AddValue("MaxDate", _maxDate);
			}
			if (base.MouseWheelBehavior != MouseWheelBehavior.Automatic)
			{
				mordexLayoutWriter.AddValue("MouseWheelBehavior", base.MouseWheelBehavior);
			}
			if (base.VerticalScrollBar != VerticalScrollBar.Automatic)
			{
				mordexLayoutWriter.AddValue("VerticalScrollBar", base.VerticalScrollBar);
			}
			if (_weekRule != CalendarWeekRule.FirstDay)
			{
				mordexLayoutWriter.AddValue("WeekRule", _weekRule);
			}
			if (_yearText != "Year")
			{
				mordexLayoutWriter.AddValue("YearText", _yearText);
			}
			if (_weekText != "Week")
			{
				mordexLayoutWriter.AddValue("WeekText", _weekText);
			}
			if (_showWeekNumbers)
			{
				mordexLayoutWriter.AddValue("ShowWeekNumbers", _showWeekNumbers);
			}
			if (!_forceDropOnIntervals)
			{
				mordexLayoutWriter.AddValue("ForceDropOnIntervals", _forceDropOnIntervals);
			}
			if (_timelineTimescaleTiers != TimescaleTiers.TwoTiers)
			{
				mordexLayoutWriter.AddValue("TimescaleTiers", _timelineTimescaleTiers);
			}
			if (_bottomTier.HasNonDefaultSettings())
			{
				mordexLayoutWriter.AddValue("BottomTier", _bottomTier);
			}
			if (_middleTier.HasNonDefaultSettings())
			{
				mordexLayoutWriter.AddValue("MiddleTier", _middleTier);
			}
			if (_topTier.HasNonDefaultSettings())
			{
				mordexLayoutWriter.AddValue("TopTier", _topTier);
			}
			if (_toolTipDelay != 5000)
			{
				mordexLayoutWriter.AddValue("ToolTipDelay", _toolTipDelay);
			}
			if (_highlightCurrentTime)
			{
				mordexLayoutWriter.AddValue("HighlightCurrentTime", _highlightCurrentTime);
			}
			if (ItemToolTip != ItemToolTip.UseToolTipText)
			{
				mordexLayoutWriter.AddValue("ItemToolTip", _itemToolTip);
			}
			if (ShouldSerializeWorkStartTime())
			{
				mordexLayoutWriter.AddValue("WorkStartTime", _workStartTime);
			}
			if (ShouldSerializeWorkEndTime())
			{
				mordexLayoutWriter.AddValue("WorkEndTime", _workEndTime);
			}
			DayOfWeek dayOfWeek = DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday;
			if (_workWeek != dayOfWeek)
			{
				mordexLayoutWriter.AddValue("WorkWeek", _workWeek);
			}
			if (!_allowExpandRowHeader)
			{
				mordexLayoutWriter.AddValue("AllowExpandRowHeader", _allowExpandRowHeader);
			}
			if (_rowHeaderGroupMode != GroupMode.Default)
			{
				mordexLayoutWriter.AddValue("RowHeaderGroupMode", _rowHeaderGroupMode);
			}
			if (_rowHeaderWidth != 120)
			{
				mordexLayoutWriter.AddValue("RowHeaderWidth", _rowHeaderWidth);
			}
			if (_showLastGroupAsRowHeader)
			{
				mordexLayoutWriter.AddValue("ShowLastGroupAsRowHeader", _showLastGroupAsRowHeader);
			}
			if (!_multiSelect)
			{
				mordexLayoutWriter.AddValue("MultiSelect", _multiSelect);
			}
			if (_itemLayoutMode != ItemLayoutMode.NewRowOnOverlap)
			{
				mordexLayoutWriter.AddValue("ItemLayoutMode", ItemLayoutMode);
			}
			if (_itemsDurationBarHeight != 7)
			{
				mordexLayoutWriter.AddValue("ItemsDurationBarHeight", ItemsDurationBarHeight);
			}
			if (_ownerDrawnArea != OwnerDrawnArea.None)
			{
				mordexLayoutWriter.AddValue("OwnerDrawnArea", _ownerDrawnArea);
			}
			if (_itemSizeMode != ItemSizeMode.TextBased)
			{
				mordexLayoutWriter.AddValue("ItemSizeMode", _itemSizeMode);
			}
			if (_useCompatibleTextRendering.HasValue)
			{
				mordexLayoutWriter.AddValue("UseCompatibleTextRendering", _useCompatibleTextRendering);
			}
			if (_timelineRowHeight != -1)
			{
				mordexLayoutWriter.AddValue("RowHeight", _timelineRowHeight);
			}
			if (_minItemWidth != 20)
			{
				mordexLayoutWriter.AddValue("MinItemWidth", _minItemWidth);
			}
			if (_timeFormat != TimeFormat.TwelveHours)
			{
				mordexLayoutWriter.AddValue("TimeFormat", _timeFormat);
			}
		}

		private void SerializeDropDownCalendar(LayoutWriter mordexLayoutWriter)
		{
			mordexLayoutWriter.AddValue("DropDownCalendar", _dropDownCalendar);
		}

		private void SerializeAppearanceProperties(LayoutWriter mordexLayoutWriter)
		{
			if (ShouldSerializeBackColor())
			{
				mordexLayoutWriter.AddValue("BackColor", BackColor);
			}
			if (ShouldSerializeBlendColor())
			{
				mordexLayoutWriter.AddValue("BlendColor", _blendColor);
			}
			if (ShouldSerializeGridLineColor())
			{
				mordexLayoutWriter.AddValue("GridLineColor", _gridLineColor);
			}
			if (ShouldSerializeGroupRowsFormatStyle())
			{
				mordexLayoutWriter.AddValue("GroupRowsFormatStyle", _groupRowsFormatStyle);
			}
			if (ShouldSerializeItemsBarFormatStyle())
			{
				mordexLayoutWriter.AddValue("ItemsBarFormatStyle", _itemsBarFormatStyle);
			}
			if (ShouldSerializeItemsFormatStyle())
			{
				mordexLayoutWriter.AddValue("ItemsFormatStyle", _itemsFormatStyle);
			}
			if (ShouldSerializeItemsEstimatedBarFormatStyle())
			{
				mordexLayoutWriter.AddValue("ItemsEstimatedBarFormatStyle", _itemsEstimatedBarFormatStyle);
			}
			if (ShouldSerializeLowerScaleTierFormatStyle())
			{
				mordexLayoutWriter.AddValue("LowerScaleTierFormatStyle", _lowerScaleTierFormatStyle);
			}
			if (ShouldSerializeUpperScaleTiersFormatStyle())
			{
				mordexLayoutWriter.AddValue("UpperScaleTiersFormatStyle", _upperScaleTiersFormatStyle);
			}
			if (_workingTimeFormatStyle.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("WorkingTimeFormatStyle", _workingTimeFormatStyle);
			}
			if (_backgroundFormatStyle.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("BackgroundFormatStyle", _backgroundFormatStyle);
			}
			if (_rowHeadersFormatStyle.ShouldSerialize())
			{
				mordexLayoutWriter.AddValue("RowHeadersFormatStyle", _rowHeadersFormatStyle);
			}
			if (ShouldSerializeScrollBarStyle())
			{
				mordexLayoutWriter.AddValue("ScrollBarStyle", _scrollBarStyle);
			}
			if (ShouldSerializeSelectedFormatStyle())
			{
				mordexLayoutWriter.AddValue("SelectedFormatStyle", _selectedFormatStyle);
			}
			if (ShouldSerializeSelectedInactiveFormatStyle())
			{
				mordexLayoutWriter.AddValue("SelectedInactiveFormatStyle", _selectedInactiveFormatStyle);
			}
			if (BackgroundImage != null)
			{
				mordexLayoutWriter.AddValue("BackgroundImage", BackgroundImage);
			}
			if (_backgroundImageDrawMode != BackgroundImageDrawMode.Stretch)
			{
				mordexLayoutWriter.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (base.BorderStyle != BorderStyle.SunkenLight3D)
			{
				mordexLayoutWriter.AddValue("BorderStyle", base.BorderStyle);
			}
			if (_defaultAlpha != 255)
			{
				mordexLayoutWriter.AddValue("DefaultAlpha", _defaultAlpha);
			}
			if (_defaultAlphaMode != AlphaMode.Opaque)
			{
				mordexLayoutWriter.AddValue("DefaultAlphaMode", _defaultAlphaMode);
			}
			if (_defaultBackColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("DefaultBackColorAlphaMode", _defaultBackColorAlphaMode);
			}
			if (_defaultBlend != 0f)
			{
				mordexLayoutWriter.AddValue("DefaultBlend", _defaultBlend);
			}
			if (_defaultForeColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("DefaultForeColorAlphaMode", _defaultForeColorAlphaMode);
			}
			if (_defaultItemsImageIndex != -1)
			{
				mordexLayoutWriter.AddValue("DefaultItemsImageIndex", _defaultItemsImageIndex);
			}
			if (_defaultItemsImageKey.Length > 0)
			{
				mordexLayoutWriter.AddValue("DefaultItemsImageKey", _defaultItemsImageKey);
			}
			if (_groupIndent != 20)
			{
				mordexLayoutWriter.AddValue("GroupIndent", _groupIndent);
			}
			if (_groupExpandBoxStyle != ExpandBoxStyle.UseDefault)
			{
				mordexLayoutWriter.AddValue("GroupExpandBoxStyle", _groupExpandBoxStyle);
			}
			if (_expandGlyph != null)
			{
				mordexLayoutWriter.AddValue("ExpandGlyph", _expandGlyph);
			}
			if (_collapseGlyph != null)
			{
				mordexLayoutWriter.AddValue("CollapseGlyph", _collapseGlyph);
			}
			if (_headerAppearance != Appearance.Light3D)
			{
				mordexLayoutWriter.AddValue("HeaderAppearance", _headerAppearance);
			}
			if (_itemsLineStyle != ItemsLineStyle.None)
			{
				mordexLayoutWriter.AddValue("ItemsLineStyle", _itemsLineStyle);
			}
			if (_gridLineStyle != GridLineStyle.SmallDots)
			{
				mordexLayoutWriter.AddValue("GridLineStyle", _gridLineStyle);
			}
			if (_gridLines != GridLines.None)
			{
				mordexLayoutWriter.AddValue("GridLines", _gridLines);
			}
			if (!base.FlatBorderColor.IsEmpty)
			{
				mordexLayoutWriter.AddValue("FlatBorderColor", base.FlatBorderColor);
			}
			if (MaximumLabelLength != -1)
			{
				mordexLayoutWriter.AddValue("MaximumLabelWidth", _maximumLabelLength);
			}
			if (!ShowItemsDurationBar)
			{
				mordexLayoutWriter.AddValue("ShowItemsDurationBar", _showItemsDurationBar);
			}
			if (ShowItemsEstimatedDurationBar)
			{
				mordexLayoutWriter.AddValue("ShowItemsEstimatedDurationBar", _showItemsEstimatedDurationBar);
			}
			if (!_showLabel)
			{
				mordexLayoutWriter.AddValue("ShowLabel", _showLabel);
			}
			if (ShouldSerializeWatermarkImage())
			{
				mordexLayoutWriter.AddValue("WatermarkImage", _watermarkImage);
			}
			if (!Font.Equals(Control.DefaultFont))
			{
				mordexLayoutWriter.AddValue("Font", Font);
			}
		}

		private void DeserializeFormatStyle(LayoutReader mordexLayoutReader, string propertyName, TimelineFormatStyle targetInstance)
		{
			targetInstance.Reset();
			mordexLayoutReader.DeserializeObject(propertyName, targetInstance);
		}

		private void DeserializeAppearanceProperties(LayoutReader reader)
		{
			DeserializeFormatStyle(reader, "GroupRowsFormatStyle", _groupRowsFormatStyle);
			DeserializeFormatStyle(reader, "ItemsBarFormatStyle", _itemsBarFormatStyle);
			DeserializeFormatStyle(reader, "ItemsFormatStyle", _itemsFormatStyle);
			DeserializeFormatStyle(reader, "ItemsEstimatedBarFormatStyle", _itemsEstimatedBarFormatStyle);
			DeserializeFormatStyle(reader, "LowerScaleTierFormatStyle", _lowerScaleTierFormatStyle);
			DeserializeFormatStyle(reader, "UpperScaleTiersFormatStyle", _upperScaleTiersFormatStyle);
			DeserializeFormatStyle(reader, "SelectedFormatStyle", _selectedFormatStyle);
			DeserializeFormatStyle(reader, "SelectedInactiveFormatStyle", _selectedInactiveFormatStyle);
			DeserializeFormatStyle(reader, "WorkingTimeFormatStyle", _workingTimeFormatStyle);
			DeserializeFormatStyle(reader, "BackgroundFormatStyle", _backgroundFormatStyle);
			DeserializeFormatStyle(reader, "RowHeadersFormatStyle", _rowHeadersFormatStyle);
			BackgroundImage = (Image)LayoutReader.GetPropertyValue(reader, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (BackgroundImageDrawMode)LayoutReader.GetPropertyValue(reader, "BackgroundImageDrawMode", typeof(BackgroundImageDrawMode), BackgroundImageDrawMode.Stretch);
			base.BorderStyle = (BorderStyle)LayoutReader.GetPropertyValue(reader, "BorderStyle", typeof(BorderStyle), BorderStyle.SunkenLight3D);
			_defaultAlpha = (int)LayoutReader.GetPropertyValue(reader, "DefaultAlpha", typeof(int), 255);
			_defaultAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultAlphaMode", typeof(AlphaMode), AlphaMode.Opaque);
			_defaultBackColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultBackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_defaultBlend = (float)LayoutReader.GetPropertyValue(reader, "DefaultBlend", typeof(float), 0f);
			_defaultForeColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_defaultItemsImageIndex = (int)LayoutReader.GetPropertyValue(reader, "DefaultItemsImageIndex", typeof(int), -1);
			_defaultItemsImageKey = (string)LayoutReader.GetPropertyValue(reader, "DefaultItemsImageKey", typeof(string), "");
			_groupIndent = (int)LayoutReader.GetPropertyValue(reader, "GroupIndent", typeof(int), 20);
			_collapseGlyph = (Image)LayoutReader.GetPropertyValue(reader, "CollapseGlyph", typeof(Image), null);
			_expandGlyph = (Image)LayoutReader.GetPropertyValue(reader, "ExpandGlyph", typeof(Image), null);
			_groupExpandBoxStyle = (ExpandBoxStyle)LayoutReader.GetPropertyValue(reader, "GroupExpandBoxStyle", typeof(ExpandBoxStyle), ExpandBoxStyle.UseDefault);
			_headerAppearance = (Appearance)LayoutReader.GetPropertyValue(reader, "HeaderAppearance", typeof(Appearance), Appearance.Light3D);
			_itemsLineStyle = (ItemsLineStyle)LayoutReader.GetPropertyValue(reader, "ItemsLineStyle", typeof(ItemsLineStyle), ItemsLineStyle.None);
			_gridLineStyle = (GridLineStyle)LayoutReader.GetPropertyValue(reader, "GridLineStyle", typeof(GridLineStyle), GridLineStyle.SmallDots);
			_gridLines = (GridLines)LayoutReader.GetPropertyValue(reader, "GridLines", typeof(GridLines), GridLines.None);
			_maximumLabelLength = (int)LayoutReader.GetPropertyValue(reader, "MaximumLabelWidth", typeof(int), -1);
			BackColor = (Color)LayoutReader.GetPropertyValue(reader, "BackColor", typeof(Color), SystemColors.Window);
			_blendColor = (Color)LayoutReader.GetPropertyValue(reader, "BlendColor", typeof(Color), Color.Empty);
			_gridLineColor = (Color)LayoutReader.GetPropertyValue(reader, "GridLineColor", typeof(Color), Color.Empty);
			_showItemsDurationBar = (bool)LayoutReader.GetPropertyValue(reader, "ShowItemsDurationBar", typeof(bool), true);
			_showItemsEstimatedDurationBar = (bool)LayoutReader.GetPropertyValue(reader, "ShowItemsEstimatedDurationBar", typeof(bool), false);
			base.FlatBorderColor = (Color)LayoutReader.GetPropertyValue(reader, "FlatBorderColor", typeof(Color), Color.Empty);
			_showLabel = (bool)LayoutReader.GetPropertyValue(reader, "ShowLabel", typeof(bool), true);
			Font = (Font)LayoutReader.GetPropertyValue(reader, "Font", typeof(Font), Font);
			_scrollBarStyle.Reset();
			reader.DeserializeObject("ScrollBarStyle", _scrollBarStyle);
			ResetWatermarkImage();
			reader.DeserializeObject("WatermarkImage", _watermarkImage);
			UpdateFormatStyleColors();
		}

		public void LoadLayout(TimelineLayout layout)
		{
			try
			{
				if (layout == null || layout.LayoutString == "")
				{
					throw new ArgumentNullException();
				}
				if (!layout.CanBeLoaded())
				{
					throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
				}
				layout = layout.GetEffectiveLayout();
				LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
				PropertyValue propertyValue = mordexLayoutReader["TimelineLayoutData"];
				if (propertyValue != null)
				{
					DeserializeLayout(propertyValue.GetLayoutReader(), layout.GetPersistenceMode());
					ApplyDataBinding(_dataSource, _bindingMemberInfo);
				}
			}
			catch (Exception ex)
			{
				Mordex.Windows.Common.MxMessageBox.Show(ex.ToString());
			}
		}

		public void LoadLayoutFile(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["TimelineLayoutFile"];
			if (propertyValue != null)
			{
				CurrentLayout = null;
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				DeserializeLayout(layoutReader, (PersistenceMode)0);
				_layouts.Clear();
				layoutReader["Layouts"]?.DeserializeObject(_layouts);
				TimelineLayout timelineLayout = _layouts.FindCurrentLayout();
				if (timelineLayout != null)
				{
					CurrentLayout = timelineLayout;
					return;
				}
				CurrentLayout = null;
				OnLayoutLoad(EventArgs.Empty);
			}
		}

		public void SaveLayoutFile(Stream stream)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
			}
			PropertyValue propertyValue = new PropertyValue("TimelineLayoutFile", this);
			propertyValue.LayoutType = "Mordex.Windows.Timeline.MxTimeline";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void SaveItems(Stream stream)
		{
			_isFullSerialization = false;
			PropertyValue root = new PropertyValue("Timeline", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
			_isFullSerialization = true;
		}

		public void LoadItems(Stream stream)
		{
			LoadItems(stream, overwriteFields: true);
		}

		public void LoadItems(Stream stream, bool overwriteFields)
		{
			_isLoadingItems = true;
			_isFullDeserialization = false;
			_overwriteFields = overwriteFields;
			DataSource = null;
			if (_overwriteFields)
			{
				Fields.Clear();
			}
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			mordexLayoutReader["Timeline"]?.DeserializeObject(this);
			InvalidateRowsAndLayout();
			OnItemsLoaded(EventArgs.Empty);
			_isFullDeserialization = true;
			_isLoadingItems = false;
		}

		internal void SaveItemToRow(TimelineItem timelineItem)
		{
			if (GetCurrencyManager() != null)
			{
				_timelineItem = timelineItem;
				MordexRow row = timelineItem._row;
				timelineItem._row = null;
				row.SetValue(GetEstimatedEndTimeField(), timelineItem.EstimatedEndTime);
				row.SetValue(GetEstimatedStartTimeField(), timelineItem.EstimatedStartTime);
				row.SetValue(GetStartTimeField(), timelineItem.StartTime);
				row.SetValue(GetEndTimeField(), timelineItem.EndTime);
				row.SetValue(GetTextField(), timelineItem.Text);
				row.EndEdit();
				_timelineItem = null;
			}
		}

		internal void DeserializeItem(TimelineItem timelineItem, LayoutReader reader)
		{
			if (GetCurrencyManager() == null)
			{
				return;
			}
			_timelineItem = timelineItem;
			MordexRow row = timelineItem._row;
			row.BeginEdit();
			row.SetValue(GetStartTimeField(), timelineItem.StartTime);
			row.SetValue(GetEndTimeField(), timelineItem.EndTime);
			row.SetValue(GetTextField(), timelineItem.Text);
			row.SetValue(GetEstimatedStartTimeField(), timelineItem.EstimatedStartTime);
			row.SetValue(GetEstimatedEndTimeField(), timelineItem.EstimatedEndTime);
			int fieldIndex = 0;
			foreach (TimelineField field in Fields)
			{
				object propertyValue = LayoutReader.GetPropertyValue(reader, "Field" + fieldIndex, field.Type, null);
				if (propertyValue != null)
				{
					row.SetValue(field, propertyValue);
				}
				fieldIndex++;
			}
			row.EndEdit();
			_timelineItem = null;
		}
	}
}
