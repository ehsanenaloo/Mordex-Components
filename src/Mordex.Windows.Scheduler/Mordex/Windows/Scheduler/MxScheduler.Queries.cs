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
	public partial class MxScheduler
	{
		internal bool IsInitializing()
		{
			return _isInitializing;
		}

		public static string GetSchedulerVersionString()
		{
			string localPath = typeof(MxScheduler).Assembly.Location;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(localPath);
			return versionInfo.FileVersion;
		}

		private int GetClientRight()
		{
			return base.ClientRectangle.Width + base.ClientRectangle.Left;
		}

		private int GetClientBottom()
		{
			return base.ClientRectangle.Height + base.ClientRectangle.Top;
		}

		internal bool IsViewNavigationEnabled()
		{
			if (_dayLayouts != null && _dayLayouts.Count > 1)
			{
				if (_viewNavigationMode == TriState.True)
				{
					return true;
				}
			}
			return false;
		}


		internal bool CanPlaceInLane(SchedulerAppointment[] schedulerAppointmentArray, SchedulerAppointment schedulerAppointment, DateTime dateTime)
		{
			int visibleMinutes = (int)schedulerAppointment.GetDurationOnDate(dateTime).TotalMinutes;
			int startMinutes = (int)schedulerAppointment.GetStartTimeOnDate(dateTime).TotalMinutes;
			int slotCount = (startMinutes % _minuteInterval + visibleMinutes) / _minuteInterval;
			if ((startMinutes % _minuteInterval + visibleMinutes) % _minuteInterval != 0)
			{
				slotCount++;
			}
			if (slotCount == 0)
			{
				slotCount++;
			}
			int startSlot = startMinutes / _minuteInterval;
			if (slotCount + startSlot > schedulerAppointmentArray.Length)
			{
				slotCount = schedulerAppointmentArray.Length - startSlot;
			}
			for (int i = startSlot; i < startSlot + slotCount; i++)
			{
				if (schedulerAppointmentArray[i] != null)
				{
					return false;
				}
			}
			return true;
		}

		internal Color GetThemedForeColor()
		{
			if (ForeColor == SystemColors.ControlText)
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.Text);
			}
			return base.ForeColor;
		}

		internal Color GetThemedBackColor()
		{
			if (!ShouldSerializeBackColor())
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.Surface);
			}
			return BackColor;
		}

		public bool IsValidRange()
		{
			DateTime dateTime = _dates[0];
			for (int i = 1; i < _dates.Count; i++)
			{
				DateTime currentDate = _dates[i];
				if (currentDate != dateTime.AddDays(1.0))
				{
					return false;
				}
				dateTime = dateTime.AddDays(1.0);
			}
			return true;
		}

		internal bool TryCommitEditor()
		{
			if (_appointmentEditor != null)
			{
				SchedulerAppointment app = _appointmentEditor._app;
				app._isEditingText = false;
				string text = _appointmentEditor._textBoxControl.Text;
				ValidatingAppointmentTextEventArgs e = new ValidatingAppointmentTextEventArgs(app, text);
				OnValidatingAppointmentText(e);
				if (!e.Cancel)
				{
					if (app.RecurrenceState != RecurrenceState.Occurrence && !_appointments.Contains(app) && text.Length > 0)
					{
						app.Selected = false;
						app._text = text;
						Appointments.Add(app);
					}
					if (!_appointments.Contains(app))
					{
						SetEditor(null);
						_isAppointmentLayoutStale = true;
						Invalidate(invalidateChildren: true);
						return true;
					}
					SetEditor(null);
				}
				else
				{
					SchedulerRenderer.FocusControl(_appointmentEditor._textBoxControl);
				}
				return !e.Cancel;
			}
			return true;
		}

		internal SchedulerAppointmentEditControl GetEditor()
		{
			return _appointmentEditor;
		}


		internal Font GetViewFont(SchedulerFormatStyle schedulerFormatStyle)
		{
			if (_view == SchedulerView.DayView)
			{
				if (schedulerFormatStyle != null)
				{
					return schedulerFormatStyle.ResolveFont(DayFont);
				}
				return DayFont;
			}
			if (_view == SchedulerView.WeekView)
			{
				if (schedulerFormatStyle != null)
				{
					return schedulerFormatStyle.ResolveFont(WeekFont);
				}
				return WeekFont;
			}
			if (schedulerFormatStyle != null)
			{
				return schedulerFormatStyle.ResolveFont(MonthFont);
			}
			return MonthFont;
		}

		public SchedulerLayout FindLayout(string key)
		{
			return FindLayoutByKey(Layouts, key);
		}

		public SchedulerLayout GetLayout()
		{
			return CreateLayout(PersistenceMode.All);
		}

		internal string GetLayoutString(PersistenceMode persistenceMode)
		{
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			WriteLayoutSections(mordexLayoutWriter, persistenceMode);
			PropertyValue root = new PropertyValue("SchedulerLayoutData", new RawXML(mordexLayoutWriter.ToString()));
			LayoutWriter rootWriter = new LayoutWriter(root, isRoot: false);
			return rootWriter.ToString();
		}

		internal CurrencyManager GetCurrencyManager()
		{
			return _schedulerCurrencyManager;
		}

		internal object GetMemberValue(SchedulerAppointment schedulerAppointment, string name)
		{
			try
			{
				if (schedulerAppointment._recurrenceState == RecurrenceState.Occurrence && schedulerAppointment.MasterAppointment != null)
				{
					return GetMemberValue(schedulerAppointment.MasterAppointment, name);
				}
				if (_schedulerCurrencyManager != null)
				{
					PropertyDescriptorCollection itemProperties = _schedulerCurrencyManager.GetItemProperties();
					if (name.Length > 0)
					{
						PropertyDescriptor propertyDescriptor = itemProperties.Find(name, ignoreCase: true);
						if (propertyDescriptor != null)
						{
							object value = ((schedulerAppointment._dataRow == null) ? propertyDescriptor.GetValue(_schedulerCurrencyManager.List[schedulerAppointment._index]) : propertyDescriptor.GetValue(schedulerAppointment._dataRow));
							if (value == DBNull.Value)
							{
								return null;
							}
							return value;
						}
						return null;
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		internal bool IsUnboundMode()
		{
			return _usesInternalTable;
		}

		internal string GetTextMemberName()
		{
			if (_usesInternalTable)
			{
				return "___TextMember";
			}
			return _textField;
		}

		internal string GetDescriptionMemberName()
		{
			if (_usesInternalTable)
			{
				return "___DescriptionMember";
			}
			return _descriptionField;
		}

		private int GetNextViewIndex()
		{
			return _usedAppointmentViewCount;
		}

		internal Pen GetGridLinePen()
		{
			return GdiCache.Pen(GridLineColor);
		}

		internal Pen GetLightGridLinePen()
		{
			return GdiCache.Pen(GridLineColorLight);
		}

		internal Brush GetStripBackgroundBrush()
		{
			if (_stripBackgroundBrush == null)
			{
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle();
				schedulerFormatStyle.Alpha = _defaultAlpha;
				schedulerFormatStyle.BackColorAlphaMode = ((ISchedulerFormatStyleOwner)this).BackColorAlphaMode;
				schedulerFormatStyle.BackColor = GetThemedBackColor();
				_stripBackgroundBrush = schedulerFormatStyle.GetBackBrush(Rectangle.Empty);
			}
			return _stripBackgroundBrush;
		}

		private SchedulerAppointmentOwnerLayoutState GetHeaderLayoutAt(int x, int integerArgument)
		{
			foreach (SchedulerAppointmentOwnerLayoutState item in _headerLayouts)
			{
				if (item._bounds.Contains(x, integerArgument))
				{
					return item;
				}
			}
			return null;
		}


		internal override int GetHeaderControlWidth()
		{
			if (ShouldShowAllDayAreaScrollBar())
			{
				return base.GetHeaderControlWidth();
			}
			return base.ClientRectangle.Width;
		}

		private bool IsTextCharacter(char character)
		{
			if (char.IsLetterOrDigit(character))
			{
				return true;
			}
			if (char.IsWhiteSpace(character))
			{
				return true;
			}
			if (char.IsPunctuation(character))
			{
				return true;
			}
			return false;
		}

		internal Color GetHeaderBackColor()
		{
			if (!_headersFormatStyle.BackColorGradient.IsEmpty)
			{
				return _headersFormatStyle.BackColorGradient;
			}
			return _headersFormatStyle.BackColor;
		}

		internal Color GetHeaderForeColor()
		{
			Color color = _headersFormatStyle.ForeColor;
			if (color == Color.Empty)
			{
				color = GetThemedForeColor();
			}
			return color;
		}

		internal Brush GetAreaForeBrush(OwnerDrawnArea ownerDrawnArea, object value)
		{
			return GetAreaFormatStyle(ownerDrawnArea, value)?.GetForeColorBrush(GetThemedForeColor());
		}

		internal Brush GetAreaBackgroundBrush(OwnerDrawnArea ownerDrawnArea, object value, Rectangle rectangle)
		{
			return GetAreaFormatStyle(ownerDrawnArea, value)?.GetBackBrush(rectangle);
		}

		internal SchedulerFormatStyle GetAreaFormatStyle(OwnerDrawnArea ownerDrawnArea, object value)
		{
			switch (ownerDrawnArea)
			{
			case OwnerDrawnArea.DateHeader:
			case OwnerDrawnArea.OwnerHeader:
			case OwnerDrawnArea.ScrollDayButton:
				return new SchedulerFormatStyle(_headersFormatStyle);
			case OwnerDrawnArea.Appointments:
				if (value is SchedulerAppointment schedulerAppointment)
				{
					SchedulerAppointmentView appointmentRenderer = FindAppointmentView(schedulerAppointment);
					if (appointmentRenderer != null)
					{
						return new SchedulerFormatStyle(appointmentRenderer.GetEffectiveFormatStyle());
					}
				}
				break;
			case OwnerDrawnArea.ControlBackground:
			{
				SchedulerFormatStyle backgroundStyle = new SchedulerFormatStyle();
				backgroundStyle.BackColor = GetThemedBackColor();
				return backgroundStyle;
			}
			case OwnerDrawnArea.AllDayArea:
				return new SchedulerFormatStyle(_allDayAreaBackgroundStyle);
			case OwnerDrawnArea.TimeNavigator:
				return new SchedulerFormatStyle(_timeNavigatorFormatStyle);
			case OwnerDrawnArea.WeekViewDay:
			{
				SchedulerFormatStyle weekDayStyle = new SchedulerFormatStyle(_weekDaysBackgroundStyle);
				if (value is SchedulerHourRangeCollectionLayoutState)
				{
					SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = value as SchedulerHourRangeCollectionLayoutState;
					if (schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle() != null)
					{
						weekDayStyle = weekDayStyle.MergeWith(schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle());
					}
				}
				return weekDayStyle;
			}
			case OwnerDrawnArea.MonthViewDay:
			{
				SchedulerFormatStyle monthDayStyle = new SchedulerFormatStyle(_monthDaysFormatStyle);
				if (value is SchedulerHourRangeCollectionLayoutState)
				{
					SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutStateArgument = value as SchedulerHourRangeCollectionLayoutState;
					if (schedulerHourRangeCollectionLayoutStateArgument.GetDate().Month % 2 == 0)
					{
						monthDayStyle = monthDayStyle.MergeWith(_alternatingMonthDaysFormatStyle);
					}
					if (schedulerHourRangeCollectionLayoutStateArgument.GetSchedulerFormatStyle() != null)
					{
						monthDayStyle = monthDayStyle.MergeWith(schedulerHourRangeCollectionLayoutStateArgument.GetSchedulerFormatStyle());
					}
				}
				return monthDayStyle;
			}
			case OwnerDrawnArea.DayViewRow:
				if (value is SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState)
				{
					SchedulerFormatStyle hourStyle = new SchedulerFormatStyle(_hourBackgroundStyle);
					SchedulerFormatStyle alternatingHourStyle = hourStyle.MergeWith(_alternatingHourBackgroundStyle);
					SchedulerFormatStyle workingHourStyle = hourStyle.MergeWith(_workingHourBackgroundStyle);
					SchedulerFormatStyle alternatingWorkingHourStyle = workingHourStyle.MergeWith(_alternatingWorkingHourBackgroundStyle);
					return GetTimeSlotStyle(schedulerAppointmentOwnerLayoutState, hourStyle, alternatingHourStyle, workingHourStyle, alternatingWorkingHourStyle);
				}
				break;
			case OwnerDrawnArea.OwnerListItem:
			{
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle();
				schedulerFormatStyle.BackColor = SystemColors.Menu;
				schedulerFormatStyle.ForeColor = SystemColors.MenuText;
				return schedulerFormatStyle;
			}
			}
			return null;
		}

		internal ApplicationSettings GetApplicationSettings()
		{
			if (_mordexApplicationSettings == null)
			{
				_mordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			else if (string.Compare(_mordexApplicationSettings.SettingsKey, SettingsKey, ignoreCase: true) != 0)
			{
				_mordexApplicationSettings = new ApplicationSettings(this, SettingsKey);
			}
			return _mordexApplicationSettings;
		}
	}
}
