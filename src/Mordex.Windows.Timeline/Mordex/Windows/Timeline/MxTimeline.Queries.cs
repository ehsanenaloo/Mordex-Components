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

		public TimelineLayout GetLayout()
		{
			return CaptureLayout(PersistenceMode.All);
		}

		internal string GetLayoutXml(PersistenceMode persistenceMode)
		{
			LayoutWriter mordexLayoutWriter = new LayoutWriter();
			SerializeLayout(mordexLayoutWriter, persistenceMode);
			PropertyValue root = new PropertyValue("TimelineLayoutData", new RawXML(mordexLayoutWriter.ToString()));
			LayoutWriter wrapperWriter = new LayoutWriter(root, isRoot: false);
			return wrapperWriter.ToString();
		}

		public TimelineLayout FindLayout(string key)
		{
			return FindLayoutInCollection(Layouts, key);
		}

		private TimelineLayout FindLayoutInCollection(TimelineLayoutCollection timelineLayoutCollection, string strB)
		{
			foreach (TimelineLayout item in timelineLayoutCollection)
			{
				if (string.Compare(item.Key, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					return item;
				}
				TimelineLayout childMatch = FindLayoutInCollection(item.ChildLayouts, strB);
				if (childMatch != null)
				{
					return childMatch;
				}
			}
			return null;
		}

		internal CurrencyManager GetCurrencyManager()
		{
			return _currencyManager;
		}

		internal bool IsUnbound()
		{
			return _isUnbound;
		}

		internal string GetToolTipMemberText(TimelineItem timelineItem)
		{
			if (ToolTipMember.Length > 0 && timelineItem._row != null)
			{
				return string.Concat(timelineItem._row.GetValue(ToolTipMember));
			}
			return "";
		}

		internal AlphaMode GetBackColorAlphaMode()
		{
			if (_defaultBackColorAlphaMode == AlphaMode.Empty)
			{
				return _defaultAlphaMode;
			}
			return _defaultBackColorAlphaMode;
		}

		internal AlphaMode GetForeColorAlphaMode()
		{
			if (_defaultForeColorAlphaMode == AlphaMode.Empty)
			{
				return _defaultAlphaMode;
			}
			return _defaultForeColorAlphaMode;
		}

		internal int GetEffectiveAlpha()
		{
			if (_defaultAlphaMode == AlphaMode.UseAlpha)
			{
				return _defaultAlpha;
			}
			if (_defaultAlphaMode == AlphaMode.Transparent)
			{
				return 0;
			}
			return 255;
		}

		private bool IsInVisibleRange(double doubleValue, double doubleValueArgument)
		{
			if (doubleValue >= _doubleValue && doubleValue <= _lastDateOffset)
			{
				return true;
			}
			if (doubleValueArgument > _doubleValue && doubleValue < _lastDateOffset)
			{
				return true;
			}
			return false;
		}

		internal Pen GetBorderPen()
		{
			if (_themeRenderer == null)
			{
				_themeRenderer = new TimelineThemeRenderer();
			}
			return _themeRenderer.CreateBorderPen();
		}

		internal bool IsOutsideDragThreshold(int x, int integerArgument)
		{
			Size dragSize = SystemInformation.DragSize;
			if (!new Rectangle(_dragStartPoint.X - dragSize.Width, _dragStartPoint.Y - dragSize.Height, dragSize.Width * 2, dragSize.Height * 2).Contains(x, integerArgument))
			{
				return true;
			}
			return false;
		}

		internal int GetHeaderAreaRight()
		{
			int right = base.ClientRectangle.Left;
			if (Groups.Count > 0)
			{
				right += (Groups.Count - 1) * _groupIndent;
				right = ((!_showLastGroupAsRowHeader) ? (right + _groupIndent) : (right + _rowHeaderWidth));
			}
			return right;
		}

		private long GetZoneStepSeconds(DateTime dateTime, bool usePreviousPeriod)
		{
			if (_focusZone == FocusZone.TopTierScale || _focusZone == FocusZone.MiddleTierScale)
			{
				Timescale tier = _focusZone == FocusZone.MiddleTierScale ? MiddleTier : TopTier;
				return GetTierStepSeconds(tier.Interval, tier.Count, dateTime, usePreviousPeriod);
			}
			return GetBottomTierStepSeconds(dateTime, usePreviousPeriod);
		}

		private static long GetTierStepSeconds(TimelineInterval interval, int count, DateTime dateTime, bool usePreviousPeriod)
		{
			if (interval == TimelineInterval.Year || interval == TimelineInterval.Month)
			{
				return GetCalendarStepSeconds(interval, count, dateTime, usePreviousPeriod);
			}
			return GetFixedIntervalSeconds(interval) * count;
		}

		// For year and month tiers this is the day unit that _intervalSeconds uses; their real cell length comes from GetCalendarStepSeconds.
		private static long GetFixedIntervalSeconds(TimelineInterval interval)
		{
			switch (interval)
			{
			case TimelineInterval.Minute:
				return 60L;
			case TimelineInterval.Hour:
				return 3600L;
			case TimelineInterval.Day:
			case TimelineInterval.Month:
			case TimelineInterval.Year:
				return 86400L;
			case TimelineInterval.Week:
				return 604800L;
			default:
				return 1L;
			}
		}

		internal Color GetForegroundColor()
		{
			if (ForeColor == SystemColors.ControlText)
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.Text);
			}
			return base.ForeColor;
		}

		internal Color GetBackgroundColor()
		{
			if (!ShouldSerializeBackColor())
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.InputBack);
			}
			return BackColor;
		}

		internal Image GetExpandedBoxImage(Color color, Color colorArgument, Color markColor, bool useChevron)
		{
			if (useChevron)
			{
				if (_expandedArrowImage == null)
				{
					_expandedArrowImage = TimelineUtilities.CreateChevronImage(color, colorArgument, markColor, true);
				}
				return _expandedArrowImage;
			}
			if (_expandBoxImage == null)
			{
				_expandBoxImage = TimelineUtilities.CreateExpandBoxImage(color, colorArgument, markColor, true);
			}
			return _expandBoxImage;
		}

		internal Image GetCollapsedBoxImage(Color color, Color colorArgument, Color markColor, bool useChevron)
		{
			if (useChevron)
			{
				if (_collapsedArrowImage == null)
				{
					_collapsedArrowImage = TimelineUtilities.CreateChevronImage(color, colorArgument, markColor, false);
				}
				return _collapsedArrowImage;
			}
			if (_collapsedPlusImage == null)
			{
				_collapsedPlusImage = TimelineUtilities.CreateExpandBoxImage(color, colorArgument, markColor, false);
			}
			return _collapsedPlusImage;
		}

		internal bool IsWorkingSlot(TimelineHourRangeCollection timelineHourRangeCollection, DateTime start, TimeSpan timeSpan)
		{
			DateRange dateRange = new DateRange(start, start.Add(timeSpan));
			if (timelineHourRangeCollection != null)
			{
				foreach (TimelineHourRange item in timelineHourRangeCollection)
				{
					if (TimelineUtilities.IsRangeInWorkingHours(dateRange, item.DayOfWeek, item))
					{
						return item.IsWorkingTime;
					}
					// A closing range closes a slot that it only partly covers too.
					if (!item.IsWorkingTime && item.StartTime < item.EndTime && (item.DayOfWeek & TimelineUtilities.ToDayOfWeekFlag(start.DayOfWeek)) != 0 && start.TimeOfDay < item.EndTime && start.TimeOfDay + timeSpan > item.StartTime)
					{
						return false;
					}
				}
			}
			// Times of day are compared so that a slot on the last day of the calendar cannot overflow.
			if (start.TimeOfDay >= _workStartTime && start.TimeOfDay + timeSpan <= _workEndTime && (TimelineUtilities.ToDayOfWeekFlag(start.DayOfWeek) & _workWeek) != 0)
			{
				return true;
			}
			return false;
		}

		internal bool IsWorkingSlotInRange(TimelineHourRange timelineHourRange, DateTime start, TimeSpan timeSpanArgument)
		{
			if (timelineHourRange != null)
			{
				return timelineHourRange.IsWorkingTime;
			}
			DateRange dateRange = new DateRange(start, start.Add(timeSpanArgument));
			DayOfWeek dayOfWeek = TimelineUtilities.ToDayOfWeekFlag(dateRange.Start.DayOfWeek);
			if ((_workWeek & dayOfWeek) != 0)
			{
				if ((int)dateRange.End.Subtract(dateRange.Start).TotalDays >= 1)
				{
					return true;
				}
				TimeSpan timeSpan = dateRange.End.TimeOfDay;
				if (timeSpan == TimeSpan.Zero && dateRange.End.Date > dateRange.Start.Date)
				{
					timeSpan = new TimeSpan(1, 0, 0, 0);
				}
				if (dateRange.Start.TimeOfDay >= _workStartTime && timeSpan <= _workEndTime)
				{
					return true;
				}
			}
			return false;
		}

		internal Brush GetAreaForeBrush(DrawTimelineAreaEventArgs eventArgs)
		{
			return GetAreaFormatStyle(eventArgs)?.GetForeColorBrush(GetForegroundColor());
		}

		internal Brush GetAreaBackgroundBrush(DrawTimelineAreaEventArgs eventArgs)
		{
			return GetAreaFormatStyle(eventArgs)?.GetBackBrush(eventArgs.Bounds);
		}

		internal TimelineFormatStyle GetAreaFormatStyle(DrawTimelineAreaEventArgs eventArgs)
		{
			TimelineFormatStyle timelineFormatStyle = null;
			switch (eventArgs.OwnerDrawnArea)
			{
			case OwnerDrawnArea.RowHeader:
			{
				TimelineGroupRow rowHeaderGroupRow = eventArgs.GroupRow;
				timelineFormatStyle = new TimelineFormatStyle(_rowHeadersFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
				if (rowHeaderGroupRow != null)
				{
					if (!rowHeaderGroupRow.Group.FormatStyle.HasOnlyDefaultValues())
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(rowHeaderGroupRow.Group.FormatStyle);
					}
					if (!rowHeaderGroupRow.FormatStyle.HasOnlyDefaultValues())
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(rowHeaderGroupRow.FormatStyle);
					}
				}
				break;
			}
			case OwnerDrawnArea.GroupRow:
			{
				TimelineGroupRow groupRow = eventArgs.GroupRow;
				timelineFormatStyle = new TimelineFormatStyle(_groupRowsFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
				if (groupRow != null)
				{
					if (!groupRow.Group.FormatStyle.HasOnlyDefaultValues())
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(groupRow.Group.FormatStyle);
					}
					if (!groupRow.FormatStyle.HasOnlyDefaultValues())
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(groupRow.FormatStyle);
					}
				}
				break;
			}
			case OwnerDrawnArea.ControlBackground:
				timelineFormatStyle = new TimelineFormatStyle(_backgroundFormatStyle);
				if (timelineFormatStyle.BackColor.IsEmpty)
				{
					timelineFormatStyle.BackColor = GetBackgroundColor();
				}
				break;
			case OwnerDrawnArea.Items:
			{
				TimelineItem item = eventArgs.Item;
				if (item == null)
				{
					break;
				}
				timelineFormatStyle = new TimelineFormatStyle(_itemsFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
				foreach (TimelineFormatCondition condition in _formatConditions)
				{
					if (condition.Enabled && condition.EvaluateItem(item) && condition.AllowMerge)
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(condition.FormatStyle);
					}
				}
				if (item.FormatStyle != null && !item.FormatStyle.HasOnlyDefaultValues())
				{
					timelineFormatStyle = timelineFormatStyle.MergeWith(item.FormatStyle);
				}
				break;
			}
			}
			return timelineFormatStyle;
		}
	}
}
