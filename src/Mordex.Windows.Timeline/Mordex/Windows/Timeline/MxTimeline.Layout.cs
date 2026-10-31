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
		internal ContextMenuStrip GetContextMenu()
		{
			if (_contextMenu == null)
			{
				_contextMenu = new ContextMenuStrip();
			}
			return _contextMenu;
		}

		protected virtual void OnLayoutLoad(EventArgs e)
		{
			if (base.Events[_layoutLoadEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		internal void InvalidateLayout()
		{
			_isLayoutDirty = true;
			Invalidate();
		}

		public void EnsureVisible(TimelineItem item)
		{
			TryCommitItemEdit();
			if (item == null)
			{
				return;
			}
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
			MordexRow mordexRow = FindRowForItem(_timelineDataSourceManager.RootRow.Rows, item);
			if (mordexRow != null)
			{
				ScrollRowIntoView(mordexRow);
			}
		}

		public void EnsureVisible(DateTime date)
		{
			TryCommitItemEdit();
			if (_isTimescaleDirty)
			{
				if (!_isTimescaleValidationEnabled)
				{
					ResumeTimescaleValidation();
				}
				UpdateTimescaleMetrics();
			}
			DateTime dateTime = date;
			if (BottomTier.Interval == TimelineInterval.Month)
			{
				double leftOffsetMonths = 0.0;
				if (Groups.Count > 0)
				{
					leftOffsetMonths = (float)(_groupIndent * (Groups.Count - 1)) / (float)_intervalWidth;
					if (_showLastGroupAsRowHeader)
					{
						leftOffsetMonths += (double)((float)_rowHeaderWidth / (float)_intervalWidth);
					}
				}
				leftOffsetMonths += (double)((float)base.ClientRectangle.Left / (float)_intervalWidth);
				dateTime = dateTime.AddMonths(-(int)leftOffsetMonths);
				int dayAdjustment = (int)((leftOffsetMonths - (double)(int)leftOffsetMonths) * (double)DateTime.DaysInMonth(dateTime.Year, dateTime.Month)) + 1;
				dateTime = dateTime.AddDays(-dayAdjustment);
			}
			else if (BottomTier.Interval == TimelineInterval.Year)
			{
				double leftOffsetYears = 0.0;
				if (Groups.Count > 0)
				{
					leftOffsetYears = (float)(_groupIndent * (Groups.Count - 1)) / (float)_intervalWidth;
					if (_showLastGroupAsRowHeader)
					{
						leftOffsetYears += (double)((float)_rowHeaderWidth / (float)_intervalWidth);
					}
				}
				leftOffsetYears += (double)((float)base.ClientRectangle.Left / (float)_intervalWidth);
				dateTime = dateTime.AddYears(-(int)leftOffsetYears);
				int daysInYear = (DateTime.IsLeapYear(dateTime.Year) ? 366 : 365);
				int yearDayAdjustment = (int)((leftOffsetYears - (double)(int)leftOffsetYears) * (double)daysInYear) + 1;
				dateTime = dateTime.AddDays(-yearDayAdjustment);
			}
			else
			{
				long leftOffsetSeconds = 0;
				if (Groups.Count > 0)
				{
					leftOffsetSeconds = (long)((double)(_groupIndent * (Groups.Count - 1)) / (double)_intervalWidth * (double)_intervalSeconds) + 1;
					if (_showLastGroupAsRowHeader)
					{
						leftOffsetSeconds += (long)((double)_rowHeaderWidth / (double)_intervalWidth * (double)_intervalSeconds) + 1;
					}
				}
				leftOffsetSeconds += (long)((double)base.ClientRectangle.Left / (double)_intervalWidth * (double)_intervalSeconds);
				dateTime = dateTime.AddSeconds(-leftOffsetSeconds);
			}
			if (dateTime > DateTime.MinValue)
			{
				FirstDate = dateTime;
			}
			else
			{
				FirstDate = _minDate;
			}
		}

		internal ImageList GetImageList()
		{
			if (_imageSource == ImageSource.ExternalImageList)
			{
				return _externalImageList;
			}
			return _internalImageList;
		}

		internal ExpandBoxStyle ResolveExpandBoxStyle(bool isExpanded, bool isThemed)
		{
			switch (GroupExpandBoxStyle)
			{
			case ExpandBoxStyle.Arrows:
				return ExpandBoxStyle.Arrows;
			case ExpandBoxStyle.Classic:
				return ExpandBoxStyle.Classic;
			case ExpandBoxStyle.UseCustomGlyph:
				if (isExpanded)
				{
					if (ExpandGlyph != null)
					{
						return ExpandBoxStyle.UseCustomGlyph;
					}
					return ExpandBoxStyle.Classic;
				}
				if (CollapseGlyph != null)
				{
					return ExpandBoxStyle.UseCustomGlyph;
				}
				return ExpandBoxStyle.Classic;
			case ExpandBoxStyle.UseDefault:
				if (isThemed)
				{
					return ExpandBoxStyle.Arrows;
				}
				return ExpandBoxStyle.Classic;
			default:
				return ExpandBoxStyle.Classic;
			}
		}

		private string GetTimescaleFormat(Timescale timescale)
		{
			if (timescale.CustomFormat.Length > 0)
			{
				return timescale.CustomFormat;
			}
			switch (timescale.Interval)
			{
			case TimelineInterval.Second:
				if (_timeFormat == TimeFormat.TwelveHours)
				{
					return "HH:mm:ss";
				}
				return "h:mm:ss tt";
			case TimelineInterval.Minute:
				if (_timeFormat == TimeFormat.TwelveHours)
				{
					return "HH:mm";
				}
				return "h:mm tt";
			case TimelineInterval.Hour:
				if (_timeFormat == TimeFormat.TwentyFourHours)
				{
					return "%H";
				}
				return "h tt";
			case TimelineInterval.Week:
				return "D";
			case TimelineInterval.Year:
				return "'" + YearText + "' yyy";
			case TimelineInterval.Month:
				return "MMMM yyy";
			case TimelineInterval.Day:
				return "ddd MMM dd";
			default:
				return "";
			}
		}
	}
}
