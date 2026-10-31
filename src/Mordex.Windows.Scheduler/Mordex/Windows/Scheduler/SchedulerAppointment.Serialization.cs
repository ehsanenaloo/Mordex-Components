using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Layouts;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerAppointment
	{
		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_formatStyle = (SchedulerFormatStyle)LayoutReader.GetPropertyValue(info, "FormatStyle", typeof(SchedulerFormatStyle), null);
			if (_formatStyle != null)
			{
				_formatStyle._owner = this;
			}
			_borderColor = (Color)LayoutReader.GetPropertyValue(info, "BorderColor", typeof(Color), Color.Blue);
			_firstImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex1", typeof(int), -1);
			_secondImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex2", typeof(int), -1);
			_thirdImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex3", typeof(int), -1);
			_fourthImageIndex = (int)LayoutReader.GetPropertyValue(info, "ImageIndex4", typeof(int), -1);
			_firstImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey1", typeof(string), "");
			_secondImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey2", typeof(string), "");
			_thirdImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey3", typeof(string), "");
			_fourthImageKey = (string)LayoutReader.GetPropertyValue(info, "ImageKey4", typeof(string), "");
			_prefix = (string)LayoutReader.GetPropertyValue(info, "Prefix", typeof(string), string.Empty);
			info["Reminder"]?.DeserializeObject(_reminder);
			_allDayEvent = (bool)LayoutReader.GetPropertyValue(info, "AllDayEvent", typeof(bool), false);
			_visible = (bool)LayoutReader.GetPropertyValue(info, "Visible", typeof(bool), true);
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), string.Empty);
			if (!_includesCoreFields)
			{
				return;
			}
			_description = (string)LayoutReader.GetPropertyValue(info, "Description", typeof(string), string.Empty);
			_endTime = (DateTime)LayoutReader.GetPropertyValue(info, "EndTime", typeof(DateTime), DateTime.MinValue);
			_startTime = (DateTime)LayoutReader.GetPropertyValue(info, "StartTime", typeof(DateTime), DateTime.MinValue);
			_text = (string)LayoutReader.GetPropertyValue(info, "Text", typeof(string), string.Empty);
			PropertyValue propertyValue = info["RecurrencePattern"];
			if (propertyValue != null)
			{
				RecurrencePattern recurrencePattern = new RecurrencePattern();
				propertyValue.DeserializeObject(recurrencePattern);
				RecurrencePattern = recurrencePattern;
			}
			else if (_recurrenceState != RecurrenceState.Exception)
			{
				RecurrencePattern = null;
			}
			string text = (string)LayoutReader.GetPropertyValue(info, "Tag", typeof(string), null);
			if (text != null)
			{
				string tagTypeName = (string)LayoutReader.GetPropertyValue(info, "TagTypeName", typeof(string), null);
				if (tagTypeName != null)
				{
					try
					{
						Type type = LayoutTypeResolver.Resolve(tagTypeName);
						_tag = PropertyValue.ConvertValue(text, type);
					}
					catch
					{
						_tag = text;
					}
				}
				else
				{
					_tag = text;
				}
			}
			else
			{
				_tag = null;
			}
			string ownerText = (string)LayoutReader.GetPropertyValue(info, "Owner", typeof(string), null);
			if (ownerText != null)
			{
				string ownerTypeName = (string)LayoutReader.GetPropertyValue(info, "OwnerTypeName", typeof(string), null);
				try
				{
					if (ownerTypeName != null)
					{
						Type ownerType = LayoutTypeResolver.Resolve(ownerTypeName);
						_ownerValue = PropertyValue.ConvertValue(ownerText, ownerType);
					}
					else
					{
						_ownerValue = ownerText;
					}
				}
				catch
				{
					_ownerValue = ownerText;
				}
			}
			else
			{
				_ownerValue = null;
			}
			_duration = _endTime.Subtract(_startTime);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_formatStyle != null && !_formatStyle.HasOnlyDefaultValues())
			{
				info.AddValue("FormatStyle", _formatStyle);
			}
			if (_borderColor != Color.Blue)
			{
				info.AddValue("BorderColor", _borderColor);
			}
			if (_firstImageIndex != -1)
			{
				info.AddValue("ImageIndex1", _firstImageIndex);
			}
			if (_secondImageIndex != -1)
			{
				info.AddValue("ImageIndex2", _secondImageIndex);
			}
			if (_thirdImageIndex != -1)
			{
				info.AddValue("ImageIndex3", _thirdImageIndex);
			}
			if (_fourthImageIndex != -1)
			{
				info.AddValue("ImageIndex4", _fourthImageIndex);
			}
			if (_firstImageKey.Length > 0)
			{
				info.AddValue("ImageKey1", _firstImageKey);
			}
			if (_secondImageKey.Length > 0)
			{
				info.AddValue("ImageKey2", _secondImageKey);
			}
			if (_thirdImageKey.Length > 0)
			{
				info.AddValue("ImageKey3", _thirdImageKey);
			}
			if (_fourthImageKey.Length > 0)
			{
				info.AddValue("ImageKey4", _fourthImageKey);
			}
			if (_prefix.Length > 0)
			{
				info.AddValue("Prefix", _prefix);
			}
			if (_reminder.HasNonDefaultSettings())
			{
				info.AddValue("Reminder", _reminder);
			}
			if (AllDayEvent)
			{
				info.AddValue("AllDayEvent", _allDayEvent);
			}
			if (!_visible)
			{
				info.AddValue("Visible", _visible);
			}
			if (_key != null && _key.Length > 0)
			{
				info.AddValue("Key", _key);
			}
			if (!_includesCoreFields)
			{
				return;
			}
			if (_description.Length > 0)
			{
				info.AddValue("Description", _description);
			}
			if (_endTime != DateTime.MinValue)
			{
				info.AddValue("EndTime", _endTime);
			}
			if (_text.Length > 0)
			{
				info.AddValue("Text", _text);
			}
			if (!_startTime.Equals(DateTime.MinValue))
			{
				info.AddValue("StartTime", _startTime);
			}
			if (_recurrencePattern != null)
			{
				info.AddValue("RecurrencePattern", RecurrencePattern);
			}
			if (_tag != null)
			{
				if (_tag.GetType() != typeof(string))
				{
					info.AddValue("TagTypeName", _tag.GetType().FullName);
				}
				if (_tag.GetType().IsEnum)
				{
					_ = (int)_tag;
					info.AddValue("Tag", _tag);
				}
				else
				{
					if (_tag.GetType() != typeof(string))
					{
						info.AddValue("TagTypeName", _tag.GetType().FullName);
					}
					info.AddValue("Tag", _tag);
				}
			}
			if (_ownerValue != null)
			{
				if (_ownerValue.GetType() != typeof(string))
				{
					info.AddValue("OwnerTypeName", _ownerValue.GetType().FullName);
				}
				info.AddValue("Owner", _ownerValue);
			}
			if (Scheduler != null)
			{
				Scheduler.SerializeSchedulerState(info, this);
			}
		}

		internal void WriteChangesToDataSource(bool ensureException, bool booleanArgument)
		{
			if (_recurrenceState == RecurrenceState.Occurrence && ensureException)
			{
				EnsureRecurrenceException();
			}
			if (_recurrenceState == RecurrenceState.Master && _recurrencePattern.GetEffectiveReminderDismissTime() == DateTime.MinValue && _reminder != null && _reminder.Enabled)
			{
				_recurrencePattern.UpdateReminderDismissTime(DateTime.Now.Subtract(_reminder.ReminderInterval));
			}
			if (Scheduler != null)
			{
				if (booleanArgument && _reminder != null && _reminder.Enabled)
				{
					_scheduler.RecalculateReminder(this);
				}
				if (_recurrenceState == RecurrenceState.Occurrence)
				{
					_scheduler.WriteSettingsToDataSource(MasterAppointment);
				}
				else
				{
					_scheduler.WriteSettingsToDataSource(this);
				}
			}
		}
	}
}
