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
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			if (_shouldLoadFullLayout)
			{
				DeserializeSchedulerState(info, (PersistenceMode)0);
				return;
			}
			if (_overwriteFields)
			{
				PropertyValue propertyValue = info["Fields"];
				_fields = new SchedulerFieldCollection(this);
				propertyValue?.DeserializeObject(_fields);
			}
			PropertyValue ownersValue = info["Owners"];
			_owners = new SchedulerAppointmentOwnerCollection(this);
			ownersValue?.DeserializeObject(_owners);
			DisposeDataTable();
			BindDataSource(null, new BindingMemberInfo(""), false);
			PropertyValue appointmentsValue = info["Appointments"];
			_publicAppointments.Detach();
			_appointments.Detach();
			_publicAppointments = new SchedulerAppointmentCollection(this, _minuteInterval, isPublic: true);
			_appointments = new SchedulerAppointmentCollection(this, 0, isPublic: false);
			_backgroundAppointments = null;
			_recurrenceAppointmentCollection.Clear();
			appointmentsValue.DeserializeObject(_appointments);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_shouldSaveFullLayout)
			{
				WriteLayout(info);
				if (_layouts.Count > 0)
				{
					info.AddValue("Layouts", _layouts);
				}
				if (StartTimeMember != "")
				{
					info.AddValue("StartTimeMember", StartTimeMember);
				}
				if (EndTimeMember != "")
				{
					info.AddValue("EndTimeMember", EndTimeMember);
				}
				if (TextMember != "")
				{
					info.AddValue("TextMember", TextMember);
				}
				if (OwnerMember != "")
				{
					info.AddValue("OwnerMember", OwnerMember);
				}
				if (DescriptionMember != "")
				{
					info.AddValue("DescriptionMember", DescriptionMember);
				}
				if (AppointmentSettingsMember != "")
				{
					info.AddValue("AppointmentSettingsMember", AppointmentSettingsMember);
				}
				if (RecurrenceInfoMember != "")
				{
					info.AddValue("RecurrenceInfoMember", RecurrenceInfoMember);
				}
				if (_multiOwner)
				{
					info.AddValue("MultiOwner", _multiOwner);
				}
			}
			else
			{
				info.AddValue("Fields", _fields);
				info.AddValue("Owners", _owners);
				info.AddValue("Appointments", _appointments);
			}
		}

		internal void WriteLayout(LayoutWriter mordexLayoutWriter)
		{
			if (_currentLayout != null)
			{
				_currentLayout.Update();
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.Fields) == 0)
				{
					WriteFields(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.FormatConditions) == 0)
				{
					WriteFormatConditions(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.StoredFilters) == 0)
				{
					WriteStoredFilters(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.FilterCondition) == 0)
				{
					WriteFilterCondition(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.InternalImageList) == 0)
				{
					WriteImageList(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.AppearanceProperties) == 0)
				{
					WriteAppearanceProperties(mordexLayoutWriter);
				}
				if ((_currentLayout.GetPersistenceMode() & PersistenceMode.OtherProperties) == 0)
				{
					WriteOtherProperties(mordexLayoutWriter);
				}
				if (View == SchedulerView.DayView && _dates.Count > 1)
				{
					WriteDates(mordexLayoutWriter);
				}
				WriteOwners(mordexLayoutWriter);
				if (_workingHourSchema.IsCustomized())
				{
					mordexLayoutWriter.AddValue("WorkingHourSchema", _workingHourSchema);
				}
			}
			else
			{
				WriteFields(mordexLayoutWriter);
				WriteOwners(mordexLayoutWriter);
				WriteFormatConditions(mordexLayoutWriter);
				WriteStoredFilters(mordexLayoutWriter);
				WriteFilterCondition(mordexLayoutWriter);
				WriteImageList(mordexLayoutWriter);
				WriteAppearanceProperties(mordexLayoutWriter);
				WriteOtherProperties(mordexLayoutWriter);
				if (_workingHourSchema.IsCustomized())
				{
					mordexLayoutWriter.AddValue("WorkingHourSchema", _workingHourSchema);
				}
				if (View == SchedulerView.DayView && _dates.Count > 1)
				{
					WriteDates(mordexLayoutWriter);
				}
			}
		}

		internal void WriteRecurrenceToDataSource(SchedulerAppointment appointment)
		{
			if (_schedulerCurrencyManager != null)
			{
				_shouldHandleItemChanged = false;
				object dataItem = ((appointment._dataRow == null) ? _schedulerCurrencyManager.List[appointment._index] : appointment._dataRow);
				if (!appointment.IsDataRowEditStarted() && dataItem is IEditableObject)
				{
					((IEditableObject)dataItem).BeginEdit();
				}
				if (_recurrenceInfoDescriptor != null && !_recurrenceInfoDescriptor.IsReadOnly)
				{
					_recurrenceInfoDescriptor.SetValue(dataItem, appointment.RecurrencePatternInfo);
				}
				if (!appointment.IsDataRowEditStarted() && dataItem is IEditableObject)
				{
					((IEditableObject)dataItem).EndEdit();
				}
				if (FilterConditionApplied != null)
				{
					if (appointment.Scheduler == null)
					{
						appointment._scheduler = this;
					}
					if (!FilterConditionApplied.IsAppointmentAccepted(appointment) && _publicAppointments.Contains(appointment))
					{
						appointment._isRemovalSilent = true;
						_publicAppointments.RemoveWithoutNotify(appointment);
						appointment._scheduler = this;
						appointment._isRemovalSilent = false;
					}
					else
					{
						appointment._scheduler = null;
					}
				}
				if (!appointment.IsDataRowEditStarted())
				{
					_schedulerCurrencyManager.EndCurrentEdit();
				}
			}
			_shouldHandleItemChanged = true;
		}

		internal void WriteSettingsToDataSource(SchedulerAppointment appointment)
		{
			if (_schedulerCurrencyManager != null)
			{
				_shouldHandleItemChanged = false;
				object dataItem;
				if (appointment._dataRow != null)
				{
					dataItem = appointment._dataRow;
				}
				else
				{
					if (appointment._index < 0)
					{
						_shouldHandleItemChanged = true;
						return;
					}
					dataItem = _schedulerCurrencyManager.List[appointment._index];
				}
				if (!appointment.IsDataRowEditStarted() && dataItem is IEditableObject)
				{
					((IEditableObject)dataItem).BeginEdit();
				}
				if (_appointmentSettingsDescriptor != null && !_appointmentSettingsDescriptor.IsReadOnly)
				{
					_appointmentSettingsDescriptor.SetValue(dataItem, appointment.SettingsInfo);
				}
				if (!appointment.IsDataRowEditStarted() && dataItem is IEditableObject)
				{
					((IEditableObject)dataItem).EndEdit();
				}
				if (FilterConditionApplied != null && !FilterConditionApplied.IsAppointmentAccepted(appointment) && _publicAppointments.Contains(appointment))
				{
					appointment._isRemovalSilent = true;
					_publicAppointments.RemoveWithoutNotify(appointment);
					appointment._scheduler = this;
					appointment._isRemovalSilent = false;
				}
				if (!appointment.IsDataRowEditStarted())
				{
					_schedulerCurrencyManager.EndCurrentEdit();
				}
			}
			_shouldHandleItemChanged = true;
			if (appointment.RecurrenceState == RecurrenceState.Exception && appointment.MasterAppointment != null)
			{
				WriteRecurrenceToDataSource(appointment.MasterAppointment);
			}
		}

		private void WriteAppointmentToDataSource(SchedulerAppointment appointment)
		{
			if (_isAppointmentDragActive)
			{
				return;
			}
			if (_schedulerCurrencyManager != null)
			{
				if (appointment.RecurrenceState == RecurrenceState.Occurrence)
				{
					appointment.EnsureRecurrenceException();
					return;
				}
				_shouldHandleItemChanged = false;
				object dataItem = ((appointment._dataRow == null) ? _schedulerCurrencyManager.List[appointment._index] : appointment._dataRow);
				if (!appointment.IsDataRowEditStarted() && dataItem is IEditableObject)
				{
					((IEditableObject)dataItem).BeginEdit();
				}
				_isUpdatingTimeRange = true;
				if (_schedulerPropertyDescriptor != null)
				{
					_schedulerPropertyDescriptor.SetValue(dataItem, appointment.StartTime);
				}
				if (_endTimeDescriptor != null)
				{
					_endTimeDescriptor.SetValue(dataItem, appointment.EndTime);
				}
				_isUpdatingTimeRange = false;
				if (_textDescriptor != null && !_textDescriptor.IsReadOnly)
				{
					_textDescriptor.SetValue(dataItem, appointment.Text);
				}
				if (_descriptionDescriptor != null && !_descriptionDescriptor.IsReadOnly)
				{
					_descriptionDescriptor.SetValue(dataItem, appointment.Description);
				}
				if (_propertyDescriptor != null && !_propertyDescriptor.IsReadOnly)
				{
					object owner = appointment.Owner;
					if (owner != null)
					{
						_propertyDescriptor.SetValue(dataItem, owner);
					}
				}
				if (!appointment.IsDataRowEditStarted() && dataItem is IEditableObject)
				{
					((IEditableObject)dataItem).EndEdit();
				}
				if (FilterConditionApplied != null)
				{
					if (!FilterConditionApplied.IsAppointmentAccepted(appointment))
					{
						if (_publicAppointments.Contains(appointment))
						{
							appointment._isRemovalSilent = true;
							_publicAppointments.RemoveWithoutNotify(appointment);
							appointment._scheduler = this;
							appointment._isRemovalSilent = false;
						}
					}
					else if (!_publicAppointments.Contains(appointment))
					{
						_publicAppointments.AddUnchecked(appointment);
					}
				}
				if (!appointment.IsDataRowEditStarted())
				{
					_schedulerCurrencyManager.EndCurrentEdit();
				}
			}
			_shouldHandleItemChanged = true;
		}

		private void WriteAppearanceProperties(LayoutWriter mordexLayoutWriter)
		{
			if (ShouldSerializeAllDayAreaBackgroundStyle())
			{
				mordexLayoutWriter.AddValue("AllDayAreaBackgroundStyle", _allDayAreaBackgroundStyle);
			}
			if (ShouldSerializeAlternatingHourBackgroundStyle())
			{
				mordexLayoutWriter.AddValue("AlternatingHourBackgroundStyle", _alternatingHourBackgroundStyle);
			}
			if (ShouldSerializeAlternatingMonthDaysFormatStyle())
			{
				mordexLayoutWriter.AddValue("AlternatingMonthDaysFormatStyle", _alternatingMonthDaysFormatStyle);
			}
			if (ShouldSerializeAlternatingWorkingHourBackgroundStyle())
			{
				mordexLayoutWriter.AddValue("AlternatingWorkingHourBackgroundStyle", _alternatingWorkingHourBackgroundStyle);
			}
			if (ShouldSerializeAppointmentOutLineColor())
			{
				mordexLayoutWriter.AddValue("AppointmentOutLineColor", _appointmentOutLineColor);
			}
			if (ShouldSerializeAppointmentsFormatStyle())
			{
				mordexLayoutWriter.AddValue("AppointmentsFormatStyle", _appointmentsFormatStyle);
			}
			if (ShouldSerializeBackColor())
			{
				mordexLayoutWriter.AddValue("BackColor", BackColor);
			}
			if (ShouldSerializeBlendColor())
			{
				mordexLayoutWriter.AddValue("BlendColor", _blendColor);
			}
			if (ShouldSerializeDayFont())
			{
				mordexLayoutWriter.AddValue("DayFont", _dayFont);
			}
			if (ShouldSerializeFlatBorderColor())
			{
				mordexLayoutWriter.AddValue("FlatBorderColor", base.FlatBorderColor);
			}
			if (ShouldSerializeGridLineColor())
			{
				mordexLayoutWriter.AddValue("GridLineColor", _gridLineColor);
			}
			if (ShouldSerializeGridLineColorLight())
			{
				mordexLayoutWriter.AddValue("GridLineColorLight", _gridLineColorLight);
			}
			if (ShouldSerializeHeadersFormatStyle())
			{
				mordexLayoutWriter.AddValue("HeadersFormatStyle", _headersFormatStyle);
			}
			if (ShouldSerializeHourBackgroundStyle())
			{
				mordexLayoutWriter.AddValue("HourBackgroundStyle", _hourBackgroundStyle);
			}
			if (ShouldSerializeMonthDaysFormatStyle())
			{
				mordexLayoutWriter.AddValue("MonthDaysFormatStyle", _monthDaysFormatStyle);
			}
			if (ShouldSerializeMonthFont())
			{
				mordexLayoutWriter.AddValue("MonthFont", _monthFont);
			}
			if (ShouldSerializeScrollBarStyle())
			{
				mordexLayoutWriter.AddValue("ScrollBarStyle", base.ScrollBarStyle);
			}
			if (ShouldSerializeSelectedFormatStyle())
			{
				mordexLayoutWriter.AddValue("SelectedFormatStyle", _selectedFormatStyle);
			}
			if (ShouldSerializeSelectedInactiveFormatStyle())
			{
				mordexLayoutWriter.AddValue("SelectedInactiveFormatStyle", _selectedInactiveFormatStyle);
			}
			if (ShouldSerializeTimeNavigatorFormatStyle())
			{
				mordexLayoutWriter.AddValue("TimeNavigatorFormatStyle", _timeNavigatorFormatStyle);
			}
			if (ShouldSerializeWeekDaysBackgroundStyle())
			{
				mordexLayoutWriter.AddValue("WeekDaysBackgroundStyle", _weekDaysBackgroundStyle);
			}
			if (ShouldSerializeWeekFont())
			{
				mordexLayoutWriter.AddValue("WeekFont", _weekFont);
			}
			if (ShouldSerializeWorkingHourBackgroundStyle())
			{
				mordexLayoutWriter.AddValue("WorkingHourBackgroundStyle", _workingHourBackgroundStyle);
			}
			if (_defaultAlpha != 255)
			{
				mordexLayoutWriter.AddValue("DefaultAlpha", _defaultAlpha);
			}
			if (_backColorAlphaMode != AlphaMode.Opaque)
			{
				mordexLayoutWriter.AddValue("DefaultAlphaMode", _backColorAlphaMode);
			}
			if (_schedulerBackColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("DefaultBackColorAlphaMode", _schedulerBackColorAlphaMode);
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				mordexLayoutWriter.AddValue("DefaultForeColorAlphaMode", _foreColorAlphaMode);
			}
			if (BackgroundImage != null)
			{
				mordexLayoutWriter.AddValue("BackgroundImage", BackgroundImage);
			}
			if (_backgroundImageDrawMode != ImageDrawMode.Stretched)
			{
				mordexLayoutWriter.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (base.BorderStyle != BorderStyle.SunkenLight3D)
			{
				mordexLayoutWriter.AddValue("BorderStyle", base.BorderStyle);
			}
			if (_clockStyle != ClockStyle.Enhanced)
			{
				mordexLayoutWriter.AddValue("ClockStyle", _clockStyle);
			}
			if (_defaultBlend != 0f)
			{
				mordexLayoutWriter.AddValue("DefaultBlend", _defaultBlend);
			}
			if (_headerAppearance != Appearance.Light3D)
			{
				mordexLayoutWriter.AddValue("HeaderAppearance", _headerAppearance);
			}
			if (!_showTimeAsClocks)
			{
				mordexLayoutWriter.AddValue("ShowTimeAsClocks", _showTimeAsClocks);
			}
			if (!_showAllDayArea)
			{
				mordexLayoutWriter.AddValue("ShowAllDayArea", _showAllDayArea);
			}
			if (_showWeekHeaders != TriState.Empty)
			{
				mordexLayoutWriter.AddValue("ShowWeekHeaders", _showWeekHeaders);
			}
			if (!_showToolTips)
			{
				mordexLayoutWriter.AddValue("ShowTooltips", _showToolTips);
			}
			if (View != SchedulerView.DayView)
			{
				mordexLayoutWriter.AddValue("View", View);
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

		private void ReadStyle(LayoutReader mordexLayoutReader, string propertyName, SchedulerFormatStyle targetInstance)
		{
			targetInstance.Reset();
			mordexLayoutReader.DeserializeObject(propertyName, targetInstance);
		}

		private void ReadAppearanceProperties(LayoutReader reader)
		{
			ReadStyle(reader, "AllDayAreaBackgroundStyle", _allDayAreaBackgroundStyle);
			ReadStyle(reader, "AlternatingHourBackgroundStyle", _alternatingHourBackgroundStyle);
			ReadStyle(reader, "AlternatingMonthDaysFormatStyle", _alternatingMonthDaysFormatStyle);
			ReadStyle(reader, "AlternatingWorkingHourBackgroundStyle", _alternatingWorkingHourBackgroundStyle);
			_appointmentOutLineColor = (Color)LayoutReader.GetPropertyValue(reader, "AppointmentOutLineColor", typeof(Color), Color.Black);
			_isAppointmentOutLineColorSet = reader["AppointmentOutLineColor"] != null;
			ReadStyle(reader, "AppointmentsFormatStyle", _appointmentsFormatStyle);
			BackColor = (Color)LayoutReader.GetPropertyValue(reader, "BackColor", typeof(Color), SystemColors.Window);
			_blendColor = (Color)LayoutReader.GetPropertyValue(reader, "BlendColor", typeof(Color), Color.Empty);
			_dayFont = (Font)LayoutReader.GetPropertyValue(reader, "DayFont", typeof(Font), null);
			base.FlatBorderColor = (Color)LayoutReader.GetPropertyValue(reader, "FlatBorderColor", typeof(Color), Color.Empty);
			_gridLineColorLight = (Color)LayoutReader.GetPropertyValue(reader, "GridLineColorLight", typeof(Color), Color.Empty);
			_gridLineColor = (Color)LayoutReader.GetPropertyValue(reader, "GridLineColor", typeof(Color), Color.Empty);
			ReadStyle(reader, "HeadersFormatStyle", _headersFormatStyle);
			ReadStyle(reader, "HourBackgroundStyle", _hourBackgroundStyle);
			ReadStyle(reader, "MonthDaysFormatStyle", _monthDaysFormatStyle);
			_monthFont = (Font)LayoutReader.GetPropertyValue(reader, "MonthFont", typeof(Font), null);
			reader.DeserializeObject("ScrollBarStyle", _scrollBarStyle);
			ReadStyle(reader, "SelectedFormatStyle", _selectedFormatStyle);
			ReadStyle(reader, "SelectedInactiveFormatStyle", _selectedInactiveFormatStyle);
			ReadStyle(reader, "TimeNavigatorFormatStyle", _timeNavigatorFormatStyle);
			ReadStyle(reader, "WeekDaysBackgroundStyle", _weekDaysBackgroundStyle);
			_weekFont = (Font)LayoutReader.GetPropertyValue(reader, "WeekFont", typeof(Font), null);
			ReadStyle(reader, "WorkingHourBackgroundStyle", _workingHourBackgroundStyle);
			_defaultAlpha = (int)LayoutReader.GetPropertyValue(reader, "DefaultAlpha", typeof(int), 255);
			_backColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultAlphaMode", typeof(AlphaMode), AlphaMode.Opaque);
			_schedulerBackColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultBackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(reader, "DefaultForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			BackgroundImage = (Image)LayoutReader.GetPropertyValue(reader, "BackgroundImage", typeof(Image), null);
			// Older layout files wrote this value under a misspelled key.
			object oldBackgroundImageDrawMode = LayoutReader.GetPropertyValue(reader, "BackgrounndImageDrawMode", typeof(ImageDrawMode), ImageDrawMode.Stretched);
			_backgroundImageDrawMode = (ImageDrawMode)LayoutReader.GetPropertyValue(reader, "BackgroundImageDrawMode", typeof(ImageDrawMode), oldBackgroundImageDrawMode);
			base.BorderStyle = (BorderStyle)LayoutReader.GetPropertyValue(reader, "BorderStyle", typeof(BorderStyle), BorderStyle.SunkenLight3D);
			_clockStyle = (ClockStyle)LayoutReader.GetPropertyValue(reader, "ClockStyle", typeof(ClockStyle), ClockStyle.Enhanced);
			_defaultBlend = (float)LayoutReader.GetPropertyValue(reader, "DefaultBlend", typeof(float), 0f);
			_headerAppearance = (Appearance)LayoutReader.GetPropertyValue(reader, "HeaderAppearance", typeof(Appearance), Appearance.Light3D);
			_showTimeAsClocks = (bool)LayoutReader.GetPropertyValue(reader, "ShowTimeAsClocks", typeof(bool), true);
			_showAllDayArea = (bool)LayoutReader.GetPropertyValue(reader, "ShowAllDayArea", typeof(bool), true);
			_showWeekHeaders = (TriState)LayoutReader.GetPropertyValue(reader, "ShowWeekHeaders", typeof(TriState), TriState.Empty);
			_showToolTips = (bool)LayoutReader.GetPropertyValue(reader, "ShowTooltips", typeof(bool), true);
			View = (SchedulerView)LayoutReader.GetPropertyValue(reader, "View", typeof(SchedulerView), SchedulerView.DayView);
			object propertyValue = LayoutReader.GetPropertyValue(reader, "Font", typeof(Font), null);
			if (propertyValue != null)
			{
				Font = (Font)propertyValue;
			}
			ResetWatermarkImage();
			reader.DeserializeObject("WatermarkImage", _watermarkImage);
			OnAreaStyleChanged(EventArgs.Empty);
		}

		internal void DeserializeSchedulerState(LayoutReader reader, PersistenceMode persistenceMode)
		{
			_isLoadingLayout = true;
			StartTimeMember = (string)LayoutReader.GetPropertyValue(reader, "StartTimeMember", typeof(string), "");
			EndTimeMember = (string)LayoutReader.GetPropertyValue(reader, "EndTimeMember", typeof(string), "");
			TextMember = (string)LayoutReader.GetPropertyValue(reader, "TextMember", typeof(string), "");
			OwnerMember = (string)LayoutReader.GetPropertyValue(reader, "OwnerMember", typeof(string), "");
			string text = (string)LayoutReader.GetPropertyValue(reader, "DescriptionMember", typeof(string), "");
			if (text.Length > 0)
			{
				DescriptionMember = text;
			}
			string recurrenceInfoMember = (string)LayoutReader.GetPropertyValue(reader, "RecurrenceInfoMember", typeof(string), "");
			if (recurrenceInfoMember.Length > 0)
			{
				RecurrenceInfoMember = recurrenceInfoMember;
			}
			string appointmentSettingsMember = (string)LayoutReader.GetPropertyValue(reader, "AppointmentSettingsMember", typeof(string), "");
			if (appointmentSettingsMember.Length > 0)
			{
				AppointmentSettingsMember = appointmentSettingsMember;
			}
			MultiOwner = (bool)LayoutReader.GetPropertyValue(reader, "MultiOwner", typeof(bool), false);
			bool shouldLoadFields = true;
			bool shouldLoadImageList = true;
			bool shouldLoadAppearanceProperties = true;
			bool shouldLoadOtherProperties = true;
			bool shouldLoadStoredFilters = true;
			bool shouldLoadDates = true;
			bool shouldLoadFormatConditions = true;
			bool shouldLoadOwners = true;
			bool shouldLoadFilterCondition = true;
			if (persistenceMode != 0)
			{
				shouldLoadFields = (persistenceMode & PersistenceMode.Fields) != 0;
				shouldLoadImageList = (persistenceMode & PersistenceMode.InternalImageList) != 0;
				shouldLoadFormatConditions = (persistenceMode & PersistenceMode.FormatConditions) != 0;
				shouldLoadAppearanceProperties = (persistenceMode & PersistenceMode.AppearanceProperties) != 0;
				shouldLoadOtherProperties = (persistenceMode & PersistenceMode.OtherProperties) != 0;
				shouldLoadStoredFilters = (persistenceMode & PersistenceMode.StoredFilters) != 0;
				shouldLoadDates = (persistenceMode & PersistenceMode.Dates) != 0;
				shouldLoadFilterCondition = (persistenceMode & PersistenceMode.FilterCondition) != 0;
			}
			if (shouldLoadFields)
			{
				PropertyValue propertyValue = reader["Fields"];
				_fields = new SchedulerFieldCollection(this);
				propertyValue?.DeserializeObject(_fields);
			}
			if (shouldLoadOwners)
			{
				PropertyValue ownersValue = reader["Owners"];
				_owners = new SchedulerAppointmentOwnerCollection(this);
				ownersValue?.DeserializeObject(_owners);
				RefreshOwners(true);
			}
			if (shouldLoadStoredFilters)
			{
				PropertyValue storedFiltersValue = reader["StoredFilters"];
				_storedFilters = new SchedulerStoredFilterCollection(this);
				if (storedFiltersValue != null)
				{
					reader.DeserializeObject("StoredFilters", _storedFilters);
				}
			}
			if (shouldLoadFilterCondition)
			{
				PropertyValue filterConditionValue = reader["FilterCondition"];
				if (filterConditionValue != null)
				{
					_filterCondition = new SchedulerFilterCondition();
					_filterCondition.FilterCondition._filterOwner = this;
					filterConditionValue.DeserializeObject((SchedulerFilterCondition)_filterCondition);
				}
				else
				{
					foreach (SchedulerStoredFilter item in _storedFilters)
					{
						if (item.GetIsCurrent())
						{
							_filterCondition = item;
							break;
						}
					}
				}
			}
			if (shouldLoadFormatConditions)
			{
				PropertyValue formatConditionsValue = reader["FormatConditions"];
				_formatConditions = new SchedulerFormatConditionCollection(this);
				formatConditionsValue?.DeserializeObject(_formatConditions);
			}
			if (shouldLoadImageList)
			{
				ReadImageList(reader);
			}
			if (shouldLoadAppearanceProperties)
			{
				ReadAppearanceProperties(reader);
			}
			if (shouldLoadOtherProperties)
			{
				ReadOtherProperties(reader);
			}
			if (shouldLoadDates)
			{
				PropertyValue datesValue = reader["Dates"];
				if (datesValue != null)
				{
					_dates = new DateCollection(this);
					reader.DeserializeObject("Dates", _dates);
				}
			}
			PropertyValue hourRangesValue = reader["DayViewHourRanges"];
			_dayViewHourRanges = new SchedulerHourRangeCollection(this);
			hourRangesValue?.DeserializeObject(_dayViewHourRanges);
			_workingHourSchema = new WorkingHoursSchema(this, _dayViewHourRanges);
			reader["WorkingHourSchema"]?.DeserializeObject(_workingHourSchema);
			_isLoadingLayout = false;
		}

		private void WriteFields(LayoutWriter mordexLayoutWriter)
		{
			if (_fields.Count > 0)
			{
				mordexLayoutWriter.AddValue("Fields", _fields);
			}
		}

		private void WriteOwners(LayoutWriter mordexLayoutWriter)
		{
			if (_owners.Count > 0)
			{
				mordexLayoutWriter.AddValue("Owners", _owners);
			}
		}

		private void WriteDates(LayoutWriter mordexLayoutWriter)
		{
			if (_dates.Count > 0)
			{
				mordexLayoutWriter.AddValue("Dates", _dates);
			}
		}

		private void WriteStoredFilters(LayoutWriter mordexLayoutWriter)
		{
			if (_storedFilters.Count > 0)
			{
				mordexLayoutWriter.AddValue("StoredFilters", _storedFilters);
			}
		}

		private void WriteFilterCondition(LayoutWriter mordexLayoutWriter)
		{
			if (_filterCondition != null && _filterCondition is SchedulerFilterCondition)
			{
				mordexLayoutWriter.AddValue("FilterCondition", _filterCondition);
			}
		}

		private void WriteFormatConditions(LayoutWriter mordexLayoutWriter)
		{
			if (_formatConditions.Count > 0)
			{
				mordexLayoutWriter.AddValue("FormatConditions", _formatConditions);
			}
		}

		internal void WriteLayoutSections(LayoutWriter mordexLayoutWriter, PersistenceMode persistenceMode)
		{
			if ((persistenceMode & PersistenceMode.Fields) != 0)
			{
				WriteFields(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.Owners) != 0)
			{
				WriteOwners(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.FormatConditions) != 0)
			{
				WriteFormatConditions(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.StoredFilters) != 0)
			{
				WriteStoredFilters(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.FilterCondition) != 0)
			{
				WriteFilterCondition(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.InternalImageList) != 0)
			{
				WriteImageList(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.Dates) != 0 && View == SchedulerView.DayView && _dates.Count > 1)
			{
				WriteDates(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.AppearanceProperties) != 0)
			{
				WriteAppearanceProperties(mordexLayoutWriter);
			}
			if ((persistenceMode & PersistenceMode.OtherProperties) != 0)
			{
				WriteOtherProperties(mordexLayoutWriter);
			}
			if (StartTimeMember != "")
			{
				mordexLayoutWriter.AddValue("StartTimeMember", StartTimeMember);
			}
			if (EndTimeMember != "")
			{
				mordexLayoutWriter.AddValue("EndTimeMember", EndTimeMember);
			}
			if (TextMember != "")
			{
				mordexLayoutWriter.AddValue("TextMember", TextMember);
			}
			if (OwnerMember != "")
			{
				mordexLayoutWriter.AddValue("OwnerMember", OwnerMember);
			}
			if (AppointmentSettingsMember != "")
			{
				mordexLayoutWriter.AddValue("AppointmentSettingsMember", AppointmentSettingsMember);
			}
			if (DescriptionMember != "")
			{
				mordexLayoutWriter.AddValue("DescriptionMember", DescriptionMember);
			}
			if (RecurrenceInfoMember != "")
			{
				mordexLayoutWriter.AddValue("RecurrenceInfoMember", RecurrenceInfoMember);
			}
			if (_multiOwner)
			{
				mordexLayoutWriter.AddValue("MultiOwner", _multiOwner);
			}
			if (_workingHourSchema.IsCustomized())
			{
				mordexLayoutWriter.AddValue("WorkingHourSchema", _workingHourSchema);
			}
		}

		private void ReadImageList(LayoutReader reader)
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

		private void WriteImageList(LayoutWriter mordexLayoutWriter)
		{
			if (_internalImageList != null && _internalImageList.Images.Count > 0)
			{
				mordexLayoutWriter.AddValue("ImageList.ColorDepth", _internalImageList.ColorDepth);
				mordexLayoutWriter.AddValue("ImageList.ImageSize", _internalImageList.ImageSize);
				mordexLayoutWriter.AddValue("ImageList.ImageStrip", ImageStrip.ToStrip(_internalImageList));
				mordexLayoutWriter.AddValue("ImageList.TransparentColor", _internalImageList.TransparentColor);
			}
		}

		private void ReadOtherProperties(LayoutReader reader)
		{
			_allDayEventsAlignment = (StringAlignment)LayoutReader.GetPropertyValue(reader, "AllDayEventsAlignment", typeof(StringAlignment), StringAlignment.Center);
			_acceptsTab = (bool)LayoutReader.GetPropertyValue(reader, "AcceptsTab", typeof(bool), true);
			_addNewMode = (AddNewMode)LayoutReader.GetPropertyValue(reader, "AddNewMode", typeof(AddNewMode), AddNewMode.Automatic);
			_allowDayResize = (bool)LayoutReader.GetPropertyValue(reader, "AllowDayResize", typeof(bool), true);
			_allowDragInNonWorkingTime = (bool)LayoutReader.GetPropertyValue(reader, "AllowDragInNonWorkingTime", typeof(bool), true);
			_allowDelete = (bool)LayoutReader.GetPropertyValue(reader, "AllowDelete", typeof(bool), true);
			_allowAppointmentDrag = (AllowAppointmentDrag)LayoutReader.GetPropertyValue(reader, "AllowAppointmentDrag", typeof(AllowAppointmentDrag), AllowAppointmentDrag.All);
			_allowEdit = (bool)LayoutReader.GetPropertyValue(reader, "AllowEdit", typeof(bool), true);
			_compressWeekEnds = (bool)LayoutReader.GetPropertyValue(reader, "CompressWeekEnds", typeof(bool), false);
			_continuousScroll = (bool)LayoutReader.GetPropertyValue(reader, "ContinuousScroll", typeof(bool), true);
			_dayColumns = (int)LayoutReader.GetPropertyValue(reader, "DayColumns", typeof(int), -1);
			_dayNavigationButtons = (bool)LayoutReader.GetPropertyValue(reader, "DayNavigationButtons", typeof(bool), false);
			_schedulerFirstDayOfWeek = (SchedulerDayOfWeek)LayoutReader.GetPropertyValue(reader, "FirstDayOfWeek", typeof(SchedulerDayOfWeek), SchedulerDayOfWeek.Sunday);
			_hiddenAppointmentMode = (HiddenAppointmentMode)LayoutReader.GetPropertyValue(reader, "HiddenAppointmentMode", typeof(HiddenAppointmentMode), HiddenAppointmentMode.Automatic);
			_hideSelection = (HideSelection)LayoutReader.GetPropertyValue(reader, "HideSelection", typeof(HideSelection), HideSelection.HideSelection);
			_hourEvenOdd = (HourEvenOdd)LayoutReader.GetPropertyValue(reader, "HourEvenOdd", typeof(HourEvenOdd), HourEvenOdd.None);
			_imageSource = (ImageSource)LayoutReader.GetPropertyValue(reader, "ImageSource", typeof(ImageSource), ImageSource.ExternalImageList);
			Interval = (Interval)LayoutReader.GetPropertyValue(reader, "Interval", typeof(Interval), Interval.ThirtyMinutes);
			_maxDate = (DateTime)LayoutReader.GetPropertyValue(reader, "MaxDate", typeof(DateTime), DateTime.MaxValue.Date.AddYears(-1));
			_minDate = (DateTime)LayoutReader.GetPropertyValue(reader, "MinDate", typeof(DateTime), DateTime.MinValue.AddMonths(1));
			_multiSelect = (bool)LayoutReader.GetPropertyValue(reader, "MultiSelect", typeof(bool), true);
			_timeFormat = (TimeFormat)LayoutReader.GetPropertyValue(reader, "TimeFormat", typeof(TimeFormat), TimeFormat.TwelveHours);
			base.VerticalScrollBar = (VerticalScrollBar)LayoutReader.GetPropertyValue(reader, "VerticalScrollBar", typeof(VerticalScrollBar), VerticalScrollBar.Automatic);
			Weeks = (int)LayoutReader.GetPropertyValue(reader, "Weeks", typeof(int), 5);
			_workEndTime = (TimeSpan)LayoutReader.GetPropertyValue(reader, "WorkEndTime", typeof(TimeSpan), new TimeSpan(17, 0, 0));
			_workStartTime = (TimeSpan)LayoutReader.GetPropertyValue(reader, "WorkStartTime", typeof(TimeSpan), new TimeSpan(8, 0, 0));
			HighlightCurrentTime = (TriState)LayoutReader.GetPropertyValue(reader, "HighlightCurrentTime", typeof(TriState), TriState.Empty);
			_showMinutesInTimeNavigator = (bool)LayoutReader.GetPropertyValue(reader, "ShowMinutesInTimeNavigator", typeof(bool), false);
			_showAllDayAreaScrollBar = (TriState)LayoutReader.GetPropertyValue(reader, "ShowAllDayAreaScrollBar", typeof(TriState), TriState.Empty);
			_appointmentsPrefix = (string)LayoutReader.GetPropertyValue(reader, "AppointmentsPrefix", typeof(string), "");
			object propertyValue = LayoutReader.GetPropertyValue(reader, "UseCompatibleTextRendering", typeof(bool), null);
			if (propertyValue != null)
			{
				_useCompatibleTextRendering = (bool)propertyValue;
			}
			_maxAllDayRows = (int)LayoutReader.GetPropertyValue(reader, "MaxAllDayRows", typeof(int), -1);
			SchedulerDayOfWeek schedulerDayOfWeek = SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
			_workWeek = (SchedulerDayOfWeek)LayoutReader.GetPropertyValue(reader, "WorkWeek", typeof(SchedulerDayOfWeek), schedulerDayOfWeek);
			MultiOwner = (bool)LayoutReader.GetPropertyValue(reader, "MultiOwner", typeof(bool), false);
			_showTimeHintOnAppointments = (TimeHintOnAppointments)LayoutReader.GetPropertyValue(reader, "ShowTimeHintOnAppointments", typeof(TimeHintOnAppointments), TimeHintOnAppointments.Automatic);
			FirstVisibleTime = (TimeSpan)LayoutReader.GetPropertyValue(reader, "FirstVisibleTime", typeof(TimeSpan), new TimeSpan(8, 0, 0));
			_dayEndHour = (int)LayoutReader.GetPropertyValue(reader, "DayEndHour", typeof(int), 24);
			_dayStartHour = (int)LayoutReader.GetPropertyValue(reader, "DayStartHour", typeof(int), 0);
			Date = (DateTime)LayoutReader.GetPropertyValue(reader, "Date", typeof(DateTime), DateTime.Today);
			_newAppointmentText = (string)LayoutReader.GetPropertyValue(reader, "NewAppointmentText", typeof(string), "Click to add appointment");
			_nextAppointmentText = (string)LayoutReader.GetPropertyValue(reader, "NextAppointmentText", typeof(string), "Next Appointment");
			_previousAppointmentText = (string)LayoutReader.GetPropertyValue(reader, "PreviousAppointmentText", typeof(string), "Previous Appointment");
			_toText = (string)LayoutReader.GetPropertyValue(reader, "ToText", typeof(string), "To");
			_fromText = (string)LayoutReader.GetPropertyValue(reader, "FromText", typeof(string), "From");
			_ownerDrawnArea = (OwnerDrawnArea)LayoutReader.GetPropertyValue(reader, "OwnerDrawnArea", typeof(OwnerDrawnArea), OwnerDrawnArea.None);
			RowHeight = (int)LayoutReader.GetPropertyValue(reader, "RowHeight", typeof(int), -1);
		}

		private void WriteOtherProperties(LayoutWriter mordexLayoutWriter)
		{
			if (!_acceptsTab)
			{
				mordexLayoutWriter.AddValue("AcceptsTab", _acceptsTab);
			}
			if (_addNewMode != AddNewMode.Automatic)
			{
				mordexLayoutWriter.AddValue("AddNewMode", AddNewMode);
			}
			if (_allDayEventsAlignment != StringAlignment.Center)
			{
				mordexLayoutWriter.AddValue("AllDayEventsAlignment", StringAlignment.Center);
			}
			if (!_allowDayResize)
			{
				mordexLayoutWriter.AddValue("AllowDayResize", _allowDayResize);
			}
			if (!_allowDelete)
			{
				mordexLayoutWriter.AddValue("AllowDelete", _allowDelete);
			}
			if (!_allowDragInNonWorkingTime)
			{
				mordexLayoutWriter.AddValue("AllowDragInNonWorkingTime", _allowDragInNonWorkingTime);
			}
			if (_allowAppointmentDrag != AllowAppointmentDrag.All)
			{
				mordexLayoutWriter.AddValue("AllowAppointmentDrag", _allowAppointmentDrag);
			}
			if (!_allowEdit)
			{
				mordexLayoutWriter.AddValue("AllowEdit", _allowEdit);
			}
			if (_appointmentsPrefix != "")
			{
				mordexLayoutWriter.AddValue("AppointmentsPrefix", _appointmentsPrefix);
			}
			if (_compressWeekEnds)
			{
				mordexLayoutWriter.AddValue("CompressWeekEnds", _compressWeekEnds);
			}
			if (!_continuousScroll)
			{
				mordexLayoutWriter.AddValue("ContinuousScroll", _continuousScroll);
			}
			mordexLayoutWriter.AddValue("Date", _date);
			if (_dayColumns != -1)
			{
				mordexLayoutWriter.AddValue("DayColumns", _dayColumns);
			}
			if (_dayNavigationButtons)
			{
				mordexLayoutWriter.AddValue("DayNavigationButtons", _dayNavigationButtons);
			}
			if (_schedulerFirstDayOfWeek != SchedulerDayOfWeek.Sunday)
			{
				mordexLayoutWriter.AddValue("FirstDayOfWeek", _schedulerFirstDayOfWeek);
			}
			if (_hiddenAppointmentMode != HiddenAppointmentMode.Automatic)
			{
				mordexLayoutWriter.AddValue("HiddenAppointmentMode", _hiddenAppointmentMode);
			}
			if (_hideSelection != HideSelection.HideSelection)
			{
				mordexLayoutWriter.AddValue("HideSelection", _hideSelection);
			}
			if (_hourEvenOdd != HourEvenOdd.None)
			{
				mordexLayoutWriter.AddValue("HourEvenOdd", _hourEvenOdd);
			}
			if (_imageSource != ImageSource.ExternalImageList)
			{
				mordexLayoutWriter.AddValue("ImageSource", _imageSource);
			}
			if (_interval != Interval.ThirtyMinutes)
			{
				mordexLayoutWriter.AddValue("Interval", _interval);
			}
			if (ShouldSerializeMaxDate())
			{
				mordexLayoutWriter.AddValue("MaxDate", _maxDate);
			}
			if (ShouldSerializeMinDate())
			{
				mordexLayoutWriter.AddValue("MinDate", _minDate);
			}
			if (!_multiSelect)
			{
				mordexLayoutWriter.AddValue("MultiSelect", _multiSelect);
			}
			if (_timeFormat != TimeFormat.TwelveHours)
			{
				mordexLayoutWriter.AddValue("TimeFormat", _timeFormat);
			}
			if (base.VerticalScrollBar != VerticalScrollBar.Automatic)
			{
				mordexLayoutWriter.AddValue("VerticalScrollBar", base.VerticalScrollBar);
			}
			if (_weeks != 5)
			{
				mordexLayoutWriter.AddValue("Weeks", _weeks);
			}
			if (ShouldSerializeWorkEndTime())
			{
				mordexLayoutWriter.AddValue("WorkEndTime", _workEndTime);
			}
			if (ShouldSerializeWorkStartTime())
			{
				mordexLayoutWriter.AddValue("WorkStartTime", _workStartTime);
			}
			if (_showMinutesInTimeNavigator)
			{
				mordexLayoutWriter.AddValue("ShowMinutesInTimeNavigator", _showMinutesInTimeNavigator);
			}
			if (_showAllDayAreaScrollBar != TriState.Empty)
			{
				mordexLayoutWriter.AddValue("ShowAllDayAreaScrollBar", _showAllDayAreaScrollBar);
			}
			if (_highlightCurrentTime != TriState.Empty)
			{
				mordexLayoutWriter.AddValue("HighlightCurrentTime", _highlightCurrentTime);
			}
			SchedulerDayOfWeek schedulerDayOfWeek = SchedulerDayOfWeek.Monday | SchedulerDayOfWeek.Tuesday | SchedulerDayOfWeek.Wednesday | SchedulerDayOfWeek.Thursday | SchedulerDayOfWeek.Friday;
			if (_workWeek != schedulerDayOfWeek)
			{
				mordexLayoutWriter.AddValue("WorkWeek", _workWeek);
			}
			if (_showTimeHintOnAppointments != TimeHintOnAppointments.Automatic)
			{
				mordexLayoutWriter.AddValue("ShowTimeHintOnAppointments", _showTimeHintOnAppointments);
			}
			if (ShouldSerializeFirstVisibleTime())
			{
				mordexLayoutWriter.AddValue("FirstVisibleTime", _firstVisibleTime);
			}
			if (_dayStartHour != 0)
			{
				mordexLayoutWriter.AddValue("DayStartHour", _dayStartHour);
			}
			if (_dayEndHour != 24)
			{
				mordexLayoutWriter.AddValue("DayEndHour", _dayEndHour);
			}
			if (_maxAllDayRows != -1)
			{
				mordexLayoutWriter.AddValue("MaxAllDayRows", _maxAllDayRows);
			}
			if (_ownerDrawnArea != OwnerDrawnArea.None)
			{
				mordexLayoutWriter.AddValue("OwnerDrawnArea", _ownerDrawnArea);
			}
			if (_useCompatibleTextRendering.HasValue)
			{
				mordexLayoutWriter.AddValue("UseCompatibleTextRendering", _useCompatibleTextRendering);
			}
			if (_rowHeight != -1)
			{
				mordexLayoutWriter.AddValue("RowHeight", _rowHeight);
			}
			if (_newAppointmentText != "Click to add appointment")
			{
				mordexLayoutWriter.AddValue("NewAppointmentText", _newAppointmentText);
			}
			if (_nextAppointmentText != "Next Appointment")
			{
				mordexLayoutWriter.AddValue("NextAppointmentText", _nextAppointmentText);
			}
			if (_previousAppointmentText != "Previous Appointment")
			{
				mordexLayoutWriter.AddValue("PreviousAppointmentText", _previousAppointmentText);
			}
			if (_toText != "To")
			{
				mordexLayoutWriter.AddValue("ToText", _toText);
			}
			if (_fromText != "From")
			{
				mordexLayoutWriter.AddValue("FromText", _fromText);
			}
		}

		public void LoadLayout(SchedulerLayout layout)
		{
			if (layout == null || layout.LayoutString == "")
			{
				throw new ArgumentNullException();
			}
			if (!layout.CanLoad())
			{
				throw new InvalidOperationException("Incremental layouts can be loaded only when they are contained in the layout collection of a standalone layout.");
			}
			layout = layout.GetMergedLayout();
			LayoutReader mordexLayoutReader = LayoutReader.FromLayout(layout);
			PropertyValue propertyValue = mordexLayoutReader["SchedulerLayoutData"];
			if (propertyValue != null)
			{
				DeserializeSchedulerState(propertyValue.GetLayoutReader(), layout.GetPersistenceMode());
				BindDataSource(_dataSource, _bindingMemberInfo, true);
			}
		}

		public void LoadLayoutFile(Stream stream)
		{
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			PropertyValue propertyValue = mordexLayoutReader["SchedulerLayoutFile"];
			if (propertyValue != null)
			{
				CurrentLayout = null;
				LayoutReader layoutReader = propertyValue.GetLayoutReader();
				DeserializeSchedulerState(layoutReader, (PersistenceMode)0);
				_layouts.Clear();
				layoutReader["Layouts"]?.DeserializeObject(_layouts);
				SchedulerLayout schedulerLayout = _layouts.FindCurrentLayout();
				if (schedulerLayout != null)
				{
					CurrentLayout = schedulerLayout;
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
			PropertyValue propertyValue = new PropertyValue("SchedulerLayoutFile", this);
			propertyValue.LayoutType = "Mordex.Windows.Scheduler.MxScheduler";
			LayoutWriter mordexLayoutWriter = new LayoutWriter(propertyValue, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
		}

		public void SaveAppointments(Stream stream)
		{
			_shouldSaveFullLayout = false;
			PropertyValue root = new PropertyValue("Schedule", this);
			LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
			mordexLayoutWriter.FillStream(stream);
			_shouldSaveFullLayout = true;
		}

		internal void SerializeSchedulerState(LayoutWriter mordexLayoutWriter, SchedulerAppointment schedulerAppointment)
		{
			SchedulerField schedulerField = null;
			for (int i = 0; i < _fields.Count; i++)
			{
				schedulerField = _fields[i];
				if (schedulerAppointment.DataRow != null)
				{
					object fieldValue = GetFieldValue(schedulerAppointment, schedulerField);
					if (fieldValue != null)
					{
						mordexLayoutWriter.AddValue("Field" + i, fieldValue);
					}
				}
			}
		}

		internal void ReadAppointmentFields(SchedulerAppointment schedulerAppointment, LayoutReader reader)
		{
			object dataItem = _schedulerCurrencyManager.List[schedulerAppointment._index];
			if (dataItem is IEditableObject)
			{
				((IEditableObject)dataItem).BeginEdit();
			}
			SetItemValue(dataItem, GetStartTimeMemberName(), schedulerAppointment._startTime);
			SetItemValue(dataItem, GetEndTimeMemberName(), schedulerAppointment.EndTime);
			SetItemValue(dataItem, GetTextMemberName(), schedulerAppointment.Text);
			SetItemValue(dataItem, GetOwnerMemberName(), schedulerAppointment.Owner);
			SetItemValue(dataItem, GetDescriptionMemberName(), schedulerAppointment.Description);
			int fieldIndex = 0;
			foreach (SchedulerField item in _fields)
			{
				object propertyValue = LayoutReader.GetPropertyValue(reader, "Field" + fieldIndex, item.Type, null);
				if (propertyValue != null)
				{
					SetItemValue(dataItem, item.DataMember, propertyValue);
				}
				fieldIndex++;
			}
			if (dataItem is IEditableObject)
			{
				((IEditableObject)dataItem).EndEdit();
			}
			if (FilterConditionApplied != null)
			{
				if (FilterConditionApplied.IsAppointmentAccepted(schedulerAppointment))
				{
					_publicAppointments.AddValidated(schedulerAppointment);
				}
			}
			else
			{
				_publicAppointments.AddValidated(schedulerAppointment);
			}
		}

		public void LoadAppointments(Stream stream)
		{
			LoadAppointments(stream, overwriteFields: true);
		}

		public void LoadAppointments(Stream stream, bool overwriteFields)
		{
			_shouldLoadFullLayout = false;
			_overwriteFields = overwriteFields;
			DataSource = null;
			if (_overwriteFields)
			{
				_fields.Clear();
			}
			LayoutReader mordexLayoutReader = LayoutReader.FromStream(stream);
			mordexLayoutReader["Schedule"]?.DeserializeObject(this);
			_isAppointmentLayoutStale = true;
			_isAppointmentCacheStale = true;
			Invalidate();
			OnAppointmentsLoaded(EventArgs.Empty);
			_shouldLoadFullLayout = true;
			RefreshOwners(true);
		}

		public void LoadComponentSettings()
		{
			if (!base.DesignMode)
			{
				if (IsInitializing())
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
					MemoryStream savedLayoutStream = new MemoryStream(layoutSettings);
					LoadLayoutFile(savedLayoutStream);
					savedLayoutStream.Close();
					savedLayoutStream.Dispose();
					savedLayoutStream = null;
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
	}
}
