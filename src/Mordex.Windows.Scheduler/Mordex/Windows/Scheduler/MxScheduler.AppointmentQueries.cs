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
		private int FindFreeAllDayRowTop(int x, int integerArgument)
		{
			foreach (SchedulerAppointmentView item in _allDayAppointmentViews)
			{
				if (item._bounds.Contains(x, integerArgument))
				{
					integerArgument += _actualRowHeight + _allDayRowGap;
					return FindFreeAllDayRowTop(x, integerArgument);
				}
			}
			return integerArgument;
		}

		// Views placed in the current month or week pass, by the top of their bounds. A view keeps its top and
		// height after it is placed (only its left and width change), so a lookup tests the few views whose
		// rows cover the point instead of every view.
		private readonly Dictionary<int, List<SchedulerAppointmentView>> _placedViewsByTop = new Dictionary<int, List<SchedulerAppointmentView>>();

		// Placed views whose bounds were still empty when they were indexed; they are tested one by one.
		private readonly List<SchedulerAppointmentView> _unplacedViews = new List<SchedulerAppointmentView>();

		private int _placedViewsIndexed;

		private int _placedViewMaxHeight;

		private void ResetPlacedViewIndex()
		{
			_placedViewsByTop.Clear();
			_unplacedViews.Clear();
			_placedViewsIndexed = 0;
			_placedViewMaxHeight = 0;
		}

		private void IndexPlacedViews()
		{
			int used = GetNextViewIndex();
			if (used < _placedViewsIndexed)
			{
				ResetPlacedViewIndex();
			}
			for (int i = _unplacedViews.Count - 1; i >= 0; i--)
			{
				if (!_unplacedViews[i]._bounds.IsEmpty)
				{
					AddPlacedView(_unplacedViews[i]);
					_unplacedViews.RemoveAt(i);
				}
			}
			for (; _placedViewsIndexed < used && _placedViewsIndexed < _appointmentViews.Count; _placedViewsIndexed++)
			{
				SchedulerAppointmentView view = (SchedulerAppointmentView)_appointmentViews[_placedViewsIndexed];
				if (view._bounds.IsEmpty)
				{
					_unplacedViews.Add(view);
				}
				else
				{
					AddPlacedView(view);
				}
			}
		}

		private void AddPlacedView(SchedulerAppointmentView view)
		{
			int top = view._bounds.Top;
			if (!_placedViewsByTop.TryGetValue(top, out List<SchedulerAppointmentView> row))
			{
				row = new List<SchedulerAppointmentView>();
				_placedViewsByTop.Add(top, row);
			}
			row.Add(view);
			_placedViewMaxHeight = Math.Max(_placedViewMaxHeight, view._bounds.Height);
		}

		private bool IsPlacedViewAt(int x, int y)
		{
			foreach (SchedulerAppointmentView view in _unplacedViews)
			{
				if (view._bounds.Contains(x, y))
				{
					return true;
				}
			}
			for (int top = y; top > y - _placedViewMaxHeight; top--)
			{
				if (_placedViewsByTop.TryGetValue(top, out List<SchedulerAppointmentView> row))
				{
					foreach (SchedulerAppointmentView view in row)
					{
						if (view._bounds.Contains(x, y))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private int FindFreeAppointmentRowTop(int x, int integerArgument)
		{
			IndexPlacedViews();
			while (IsPlacedViewAt(x, integerArgument + 1))
			{
				integerArgument += _actualRowHeight + _allDayRowGap;
			}
			return integerArgument;
		}

		public SchedulerAppointment[] GetAppointmentsFromDate(DateTime date)
		{
			ArrayList arrayList = _publicAppointments.GetAppointmentsOnDate(date);
			SchedulerAppointment[] result = new SchedulerAppointment[arrayList.Count];
			int index = 0;
			foreach (SchedulerAppointment item in arrayList)
			{
				result.SetValue(item, index);
				index++;
			}
			return result;
		}

		public SchedulerAppointment[] GetAppointmentsFromRange(DateRange range)
		{
			ArrayList arrayList = _publicAppointments.GetAppointmentsOverlappingRange(range);
			SchedulerAppointment[] result = new SchedulerAppointment[arrayList.Count];
			int index = 0;
			foreach (SchedulerAppointment item in arrayList)
			{
				result.SetValue(item, index);
				index++;
			}
			return result;
		}

		public SchedulerAppointment[] GetAppointmentsFromRange(DateRange range, SchedulerAppointmentOwner owner)
		{
			if (IsMultiOwnerActive())
			{
				ArrayList arrayList = _publicAppointments.GetAppointmentsForOwnerValue(range, owner.Value);
				SchedulerAppointment[] result = new SchedulerAppointment[arrayList.Count];
				int index = 0;
				{
					foreach (SchedulerAppointment item in arrayList)
					{
						result.SetValue(item, index);
						index++;
					}
					return result;
				}
			}
			return GetAppointmentsFromRange(range);
		}

		private DateTime GetDateAcrossWeekRow(bool isBackward, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			DateTime dateAt;
			if (isBackward)
			{
				dateAt = GetDateAt(base.ClientRectangle.Left + 1, schedulerHourRangeCollectionLayoutState._bounds.Top);
				if (dateAt != _selectionCursor.Date)
				{
					return dateAt;
				}
				ScrollToPosition(_verticalScrollPosition - 1);
				GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
				RenderWeekView(null);
				dateAt = GetDateAt(GetClientRight() - 1, schedulerHourRangeCollectionLayoutState._bounds.Top);
				if (dateAt > _selectionCursor.Date)
				{
					return dateAt;
				}
				return _selectionCursor;
			}
			dateAt = GetDateAt(GetClientRight() - 1, schedulerHourRangeCollectionLayoutState._bounds.Top);
			if (dateAt != _selectionCursor.Date)
			{
				return dateAt;
			}
			ScrollToPosition(_verticalScrollPosition + 1);
			GetVerticalScrollBarControl().UpdateScrollValue(_verticalScrollPosition);
			RenderWeekView(null);
			dateAt = GetDateAt(base.ClientRectangle.Left + 1, schedulerHourRangeCollectionLayoutState._bounds.Top);
			if (dateAt > _selectionCursor.Date)
			{
				return dateAt;
			}
			return _selectionCursor;
		}

		internal bool TrySelectNextAppointment(Keys keys)
		{
			bool forward = false;
			if (keys == Keys.Tab)
			{
				forward = true;
			}
			if (_view == SchedulerView.DayView)
			{
				if (SelectAdjacentAppointment(forward))
				{
					return true;
				}
			}
			else if (_visibleAppointments != null && _visibleAppointments.Count > 0)
			{
				SchedulerAppointment schedulerAppointment = null;
				if (_schedulerOwner != null)
				{
					schedulerAppointment = _schedulerOwner.GetAppointment();
					if (!TryCommitEditor())
					{
						return true;
					}
				}
				SchedulerAppointmentView appointmentView = FindAppointmentView(GetAdjacentAppointment(schedulerAppointment, forward, DateTime.MinValue));
				if (appointmentView != null)
				{
					_schedulerSelectedAppointments.ClearWithoutLayoutUpdate();
					_currentAppointment = appointmentView.GetAppointment();
					appointmentView.GetAppointment().Selected = true;
					if (_allowEdit)
					{
						ShowEditor(appointmentView, appointmentView.GetAppointment().Text);
					}
					EnsureVisible(appointmentView.GetAppointment().StartTime);
					return true;
				}
				_schedulerSelectedAppointments.Clear();
				_currentAppointment = null;
				_schedulerOwner = null;
			}
			return false;
		}

		internal SchedulerAppointmentView FindAppointmentView(SchedulerAppointment schedulerAppointment)
		{
			foreach (SchedulerAppointmentView item in _appointmentViews)
			{
				if (item.GetAppointment() == schedulerAppointment)
				{
					return item;
				}
				if (schedulerAppointment != null && schedulerAppointment.RecurrenceState == RecurrenceState.Occurrence && schedulerAppointment.MasterAppointment == item.GetAppointment().MasterAppointment && schedulerAppointment.StartTime == item.GetAppointment().StartTime)
				{
					return item;
				}
			}
			if (_view == SchedulerView.DayView)
			{
				foreach (SchedulerAppointmentView allDayView in _allDayAppointmentViews)
				{
					if (allDayView.GetAppointment() == schedulerAppointment)
					{
						return allDayView;
					}
					if (schedulerAppointment != null && schedulerAppointment.RecurrenceState == RecurrenceState.Occurrence && schedulerAppointment.MasterAppointment == allDayView.GetAppointment().MasterAppointment && schedulerAppointment.StartTime == allDayView.GetAppointment().StartTime)
					{
						return allDayView;
					}
				}
			}
			return null;
		}

		internal ArrayList GetOwnerAppointmentsBetween(SchedulerAppointment schedulerAppointmentArgument, SchedulerAppointment lastAppointment, object owner)
		{
			SchedulerAppointment[] appointmentsFromRange = GetAppointmentsFromRange(new DateRange(DateRange.Start.Date, DateRange.End.Date.AddDays(1.0)));
			ArrayList arrayList = new ArrayList();
			int matchedCount = 0;
			if (schedulerAppointmentArgument == lastAppointment)
			{
				arrayList.Add(schedulerAppointmentArgument);
				return arrayList;
			}
			SchedulerAppointment[] visibleAppointments = appointmentsFromRange;
			foreach (SchedulerAppointment schedulerAppointment in visibleAppointments)
			{
				if (matchedCount >= 2)
				{
					break;
				}
				if (schedulerAppointmentArgument == schedulerAppointment || lastAppointment == schedulerAppointment)
				{
					matchedCount++;
				}
				else if (schedulerAppointmentArgument.RecurrenceState == RecurrenceState.Occurrence || lastAppointment.RecurrenceState == RecurrenceState.Occurrence)
				{
					if (schedulerAppointmentArgument.MasterAppointment == schedulerAppointment.MasterAppointment && schedulerAppointmentArgument.StartTime == schedulerAppointment.StartTime)
					{
						matchedCount++;
					}
					else if (lastAppointment.MasterAppointment == schedulerAppointment.MasterAppointment && lastAppointment.StartTime == schedulerAppointment.StartTime)
					{
						matchedCount++;
					}
				}
				if (matchedCount > 0 && (schedulerAppointment.Owner == owner || (schedulerAppointment.Owner != null && schedulerAppointment.Owner.Equals(owner))))
				{
					arrayList.Add(schedulerAppointment);
				}
			}
			return arrayList;
		}

		internal ArrayList GetAppointmentsBetween(SchedulerAppointment schedulerAppointmentArgument, SchedulerAppointment lastAppointment)
		{
			SchedulerAppointment[] appointmentsFromRange = GetAppointmentsFromRange(new DateRange(DateRange.Start.Date, DateRange.End.Date.AddDays(1.0)));
			ArrayList arrayList = new ArrayList();
			if (schedulerAppointmentArgument == lastAppointment)
			{
				arrayList.Add(schedulerAppointmentArgument);
				return arrayList;
			}
			int matchedCount = 0;
			SchedulerAppointment[] visibleAppointments = appointmentsFromRange;
			foreach (SchedulerAppointment schedulerAppointment in visibleAppointments)
			{
				if (matchedCount >= 2)
				{
					break;
				}
				if (schedulerAppointmentArgument == schedulerAppointment || lastAppointment == schedulerAppointment)
				{
					matchedCount++;
				}
				else if (schedulerAppointmentArgument.RecurrenceState == RecurrenceState.Occurrence || lastAppointment.RecurrenceState == RecurrenceState.Occurrence)
				{
					if (schedulerAppointmentArgument.MasterAppointment == schedulerAppointment.MasterAppointment && schedulerAppointmentArgument.StartTime == schedulerAppointment.StartTime)
					{
						matchedCount++;
					}
					else if (lastAppointment.MasterAppointment == schedulerAppointment.MasterAppointment && lastAppointment.StartTime == schedulerAppointment.StartTime)
					{
						matchedCount++;
					}
				}
				if (matchedCount > 0)
				{
					arrayList.Add(schedulerAppointment);
				}
			}
			return arrayList;
		}

		internal SchedulerAppointmentView GetHoveredAppointmentView()
		{
			if (_hoveredAppointment == null)
			{
				return null;
			}
			SchedulerAppointmentView result = null;
			foreach (SchedulerAppointmentView item in _appointmentViews)
			{
				if (item.GetAppointment().Equals(_hoveredAppointment))
				{
					result = item;
					if (item._date.Date == _hoveredAppointmentDate)
					{
						return item;
					}
				}
			}
			if (_view == SchedulerView.DayView)
			{
				foreach (SchedulerAppointmentView allDayView in _allDayAppointmentViews)
				{
					if (allDayView.GetAppointment().Equals(_hoveredAppointment))
					{
						result = allDayView;
						if (allDayView._date.Date == _hoveredAppointmentDate)
						{
							return allDayView;
						}
					}
				}
			}
			return result;
		}

		// First appointment of _appointments for each data index. It is rebuilt when an index is written or the
		// membership changes, so a run of data source changes does not scan the list for each one.
		private Dictionary<int, SchedulerAppointment> _appointmentsByDataIndex;

		private SchedulerAppointmentCollection _dataIndexMapCollection;

		private int _dataIndexMapIndexVersion;

		private int _dataIndexMapMembershipVersion;

		private SchedulerAppointment FindAppointmentByDataIndex(int dataIndex)
		{
			if (_appointmentsByDataIndex == null || !ReferenceEquals(_dataIndexMapCollection, _appointments) || _dataIndexMapIndexVersion != SchedulerAppointment.s_indexVersion || _dataIndexMapMembershipVersion != _appointments.MembershipVersion)
			{
				Dictionary<int, SchedulerAppointment> map = new Dictionary<int, SchedulerAppointment>(_appointments.Count);
				foreach (SchedulerAppointment item in _appointments)
				{
					map.TryAdd(item._index, item);
				}
				_appointmentsByDataIndex = map;
				_dataIndexMapCollection = _appointments;
				_dataIndexMapIndexVersion = SchedulerAppointment.s_indexVersion;
				_dataIndexMapMembershipVersion = _appointments.MembershipVersion;
			}
			_appointmentsByDataIndex.TryGetValue(dataIndex, out SchedulerAppointment found);
			return found;
		}

		internal string GetAppointmentSettingsMemberName()
		{
			if (_usesInternalTable)
			{
				return "___AppointmentLayoutInfoMember";
			}
			return _appointmentSettingsField;
		}

		public SchedulerAppointment GetAppointment(object dataRow)
		{
			if (dataRow == null)
			{
				return null;
			}
			foreach (SchedulerAppointment item in _appointments)
			{
				object appointmentDataRow = item.DataRow;
				if (appointmentDataRow is DataRowView)
				{
					if (appointmentDataRow.Equals(dataRow))
					{
						return item;
					}
					if (((DataRowView)appointmentDataRow).Row.Equals(dataRow))
					{
						return item;
					}
				}
				else if (dataRow.Equals(appointmentDataRow))
				{
					return item;
				}
			}
			return null;
		}

		private SchedulerAppointmentView GetMonthAppointmentView(int left, int top, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState, SchedulerAppointment appointment, Font font, bool isContinuation)
		{
			bool isNewView = false;
			if (GetNextViewIndex() >= _appointmentViews.Count)
			{
				isNewView = true;
			}
			top = FindFreeAppointmentRowTop(left + 5, top);
			SchedulerAppointmentView result = null;
			DateTime field_ = schedulerHourRangeCollectionLayoutState.GetDate();
			int cellBottom = schedulerHourRangeCollectionLayoutState._bounds.Bottom - 1;
			if (top + _actualRowHeight > cellBottom)
			{
				try
				{
					if (!isContinuation)
					{
						schedulerHourRangeCollectionLayoutState.SetHasHiddenAppointments(true);
						int dayIndex = schedulerHourRangeCollectionLayoutState.GetDayIndex();
						field_ = schedulerHourRangeCollectionLayoutState.GetDate().AddDays(1.0);
						while (field_ < appointment.EndTime && dayIndex < _dayLayouts.Count - 1)
						{
							dayIndex++;
							field_ = field_.AddDays(1.0);
							_dayLayouts.GetLayoutAt(dayIndex).SetHasHiddenAppointments(true);
						}
					}
					else
					{
						schedulerHourRangeCollectionLayoutState.SetHasHiddenAppointments(true);
					}
				}
				catch
				{
					return null;
				}
				return result;
			}
			if (isNewView)
			{
				result = new SchedulerAppointmentView(appointment, _actualRowHeight, (int)Interval, allDay: false, _view, schedulerHourRangeCollectionLayoutState.GetDate(), this, UseCompatibleTextRendering);
				result._appointmentFont = font;
				result._rowTop = top;
				_appointmentViews.Add(result);
				if (appointment.Selected)
				{
					result.UpdateLayout(AppointmentState.Focus);
				}
				else
				{
					AppointmentState appointmentState = AppointmentState.Normal;
					if (appointment.IsRecurring)
					{
						foreach (SchedulerAppointment selectedAppointment in SelectedAppointments)
						{
							if (selectedAppointment.MasterAppointment == appointment.MasterAppointment && selectedAppointment.StartTime == appointment.StartTime)
							{
								appointmentState = AppointmentState.Focus;
							}
						}
					}
					result.UpdateLayout(appointmentState);
				}
				result._showTimeAsClocks = _showTimeAsClocks;
				if ((int)appointment.Duration.TotalDays >= 1)
				{
					result._isMultiDay = true;
				}
				else
				{
					result._isMultiDay = false;
				}
			}
			else
			{
				result = (SchedulerAppointmentView)_appointmentViews[GetNextViewIndex()];
				if (appointment.Selected)
				{
					result.UpdateLayout(AppointmentState.Focus);
				}
				else
				{
					AppointmentState reusedViewState = AppointmentState.Normal;
					if (appointment.IsRecurring)
					{
						foreach (SchedulerAppointment selectedForReuse in SelectedAppointments)
						{
							if (selectedForReuse.MasterAppointment == appointment.MasterAppointment && selectedForReuse.StartTime == appointment.StartTime)
							{
								reusedViewState = AppointmentState.Focus;
							}
						}
					}
					result.UpdateLayout(reusedViewState);
				}
				result._appointmentFont = font;
				result.UpdateAppointment(appointment);
				result._bounds = Rectangle.Empty;
				result.UpdateSlotsPerHour(60 / (int)_interval);
				result._date = field_;
				result._cellWidth = schedulerHourRangeCollectionLayoutState._bounds.Width;
				result._rowHeight = _actualRowHeight;
				result._rowTop = top;
				result.UpdateFirstSegment(true);
				result.UpdateLastSegment(true);
				result.UpdateView(SchedulerView.MonthView);
				result.SetContinuedFromPrevious(false);
				result.SetContinuedToNext(false);
				result._showTimeAsClocks = _showTimeAsClocks;
				result._backColor = Color.Empty;
				result._schedulerRenderer4Color = Color.Empty;
				if ((int)appointment.Duration.TotalDays >= 1)
				{
					result._isMultiDay = true;
				}
				else
				{
					result._isMultiDay = false;
				}
				result._isShown = true;
			}
			SetNextViewIndex(GetNextViewIndex() + 1);
			return result;
		}

		internal SchedulerAppointmentView GetAllDayAppointmentView(SchedulerAppointment appointment)
		{
			SchedulerAppointmentView appointmentView = null;
			bool isNewView = false;
			if (_usedAllDayViewCount >= _allDayAppointmentViews.Count)
			{
				isNewView = true;
			}
			if (isNewView)
			{
				appointmentView = new SchedulerAppointmentView(appointment, _actualRowHeight, (int)_interval, allDay: true, SchedulerView.MonthView, appointment.StartTime, this, UseCompatibleTextRendering);
				appointmentView._appointmentFont = DayFont;
				if (appointment.Selected)
				{
					appointmentView.UpdateLayout(AppointmentState.Focus);
				}
				else
				{
					AppointmentState appointmentState = AppointmentState.Normal;
					if (appointment.IsRecurring)
					{
						foreach (SchedulerAppointment selectedAppointment in SelectedAppointments)
						{
							if (selectedAppointment.MasterAppointment == appointment.MasterAppointment && selectedAppointment.StartTime == appointment.StartTime)
							{
								appointmentState = AppointmentState.Focus;
							}
						}
					}
					appointmentView.UpdateLayout(appointmentState);
				}
				appointmentView._isShown = false;
				appointmentView._bounds = Rectangle.Empty;
				appointmentView._isMultiDay = true;
				appointmentView._showTimeAsClocks = true;
				_allDayAppointmentViews.Add(appointmentView);
			}
			else
			{
				appointmentView = (SchedulerAppointmentView)_allDayAppointmentViews[_usedAllDayViewCount];
				appointmentView.UpdateLayout(AppointmentState.Normal);
				appointmentView._appointmentFont = DayFont;
				if (appointment.Selected)
				{
					appointmentView.UpdateLayout(AppointmentState.Focus);
				}
				else
				{
					AppointmentState reusedViewState = AppointmentState.Normal;
					if (appointment.IsRecurring)
					{
						foreach (SchedulerAppointment selectedForReuse in SelectedAppointments)
						{
							if (selectedForReuse.MasterAppointment == appointment.MasterAppointment && selectedForReuse.StartTime == appointment.StartTime)
							{
								reusedViewState = AppointmentState.Focus;
							}
						}
					}
					appointmentView.UpdateLayout(reusedViewState);
				}
				appointmentView._isShown = false;
				appointmentView._bounds = Rectangle.Empty;
				appointmentView.UpdateAppointment(appointment);
				appointmentView.UpdateSlotsPerHour(60 / (int)_interval);
				appointmentView._date = appointment.StartTime;
				appointmentView._rowHeight = _actualRowHeight;
				appointmentView.UpdateFirstSegment(true);
				appointmentView.UpdateLastSegment(true);
				appointmentView.SetContinuedFromPrevious(false);
				appointmentView._isMultiDay = true;
				appointmentView._showTimeAsClocks = true;
				appointmentView.SetContinuedToNext(false);
				appointmentView._backColor = Color.Empty;
				appointmentView._schedulerRenderer4Color = Color.Empty;
			}
			_usedAllDayViewCount++;
			return appointmentView;
		}

		internal Pen GetAppointmentOutlinePen()
		{
			// Without a color set by the developer the outline follows the theme.
			if (!_isAppointmentOutLineColorSet)
			{
				return GdiCache.Pen(MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.BorderStrong));
			}
			return GdiCache.Pen(_appointmentOutLineColor);
		}

		internal Brush GetAppointmentBorderBrush(SchedulerAppointment schedulerAppointment)
		{
			Color borderColor = schedulerAppointment.BorderColor;
			foreach (SchedulerFormatCondition formatCondition in FormatConditions)
			{
				if (formatCondition.Enabled && !formatCondition.BorderColor.IsEmpty && formatCondition.MatchesAppointment(schedulerAppointment))
				{
					borderColor = formatCondition.BorderColor;
				}
			}
			if (_borderBrush != null && ((SolidBrush)_borderBrush).Color != borderColor)
			{
				_borderBrush.Dispose();
				_borderBrush = null;
			}
			if (_borderBrush == null)
			{
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle();
				schedulerFormatStyle.Alpha = _defaultAlpha;
				schedulerFormatStyle.BackColorAlphaMode = ((ISchedulerFormatStyleOwner)this).BackColorAlphaMode;
				schedulerFormatStyle.BackColor = borderColor;
				_borderBrush = schedulerFormatStyle.GetBackBrush(Rectangle.Empty);
			}
			return _borderBrush;
		}

		public SchedulerAppointment GetAppointmentAt()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return GetAppointmentAt(position.X, position.Y);
		}

		public SchedulerAppointment GetAppointmentAt(Point pt)
		{
			return GetAppointmentAt(pt.X, pt.Y);
		}

		public SchedulerAppointment GetAppointmentAt(int x, int y)
		{
			foreach (SchedulerAppointmentView item in _appointmentViews)
			{
				if (item._bounds.Contains(x, y))
				{
					return item.GetAppointment();
				}
			}
			if (_view == SchedulerView.DayView)
			{
				foreach (SchedulerAppointmentView allDayRenderer in _allDayAppointmentViews)
				{
					if (allDayRenderer._bounds.Contains(x, y))
					{
						return allDayRenderer.GetAppointment();
					}
				}
			}
			return null;
		}

		internal bool TryShowNextAppointmentDay(bool shouldNavigate)
		{
			DateTime dateTime = _dates[_dates.Count - 1].Date.AddDays(1.0);
			SchedulerAppointment schedulerAppointment = _publicAppointments.FindNextAppointment(dateTime);
			if (schedulerAppointment != null)
			{
				DateTime startTime = schedulerAppointment.StartTime;
				if (startTime.Date != dateTime)
				{
					foreach (SchedulerAppointment item in _recurrenceAppointmentCollection)
					{
						SchedulerAppointment nextOccurrence = item.RecurrencePattern.FindNextOccurrence(dateTime);
						if (nextOccurrence != null)
						{
							if (nextOccurrence.StartTime < startTime)
							{
								startTime = nextOccurrence.StartTime;
							}
							if (startTime.Date == dateTime)
							{
								break;
							}
						}
					}
				}
				if (shouldNavigate)
				{
					SetDayViewDates(startTime.Date, false);
					EnsureVisible(startTime);
				}
				return true;
			}
			bool foundOccurrence = false;
			DateTime earliestStart = MaxDate;
			foreach (SchedulerAppointment recurringAppointment in _recurrenceAppointmentCollection)
			{
				SchedulerAppointment candidateOccurrence = recurringAppointment.RecurrencePattern.FindNextOccurrence(dateTime);
				if (candidateOccurrence != null)
				{
					if (candidateOccurrence.StartTime < earliestStart)
					{
						earliestStart = candidateOccurrence.StartTime;
						foundOccurrence = true;
					}
					if (earliestStart.Date == dateTime)
					{
						break;
					}
				}
			}
			if (shouldNavigate && foundOccurrence)
			{
				SetDayViewDates(earliestStart.Date, false);
				EnsureVisible(earliestStart);
			}
			return foundOccurrence;
		}

		internal bool TryShowPreviousAppointmentDay(bool shouldNavigate)
		{
			DateTime date = _dates[0].Date;
			SchedulerAppointment schedulerAppointment = _publicAppointments.FindPreviousAppointment(date);
			if (schedulerAppointment != null)
			{
				DateTime startTime = schedulerAppointment.StartTime;
				if (startTime.Date != date)
				{
					foreach (SchedulerAppointment item in _recurrenceAppointmentCollection)
					{
						SchedulerAppointment previousOccurrence = item.RecurrencePattern.FindPreviousOccurrence(date);
						if (previousOccurrence != null)
						{
							if (previousOccurrence.StartTime > startTime && previousOccurrence.StartTime <= date)
							{
								startTime = previousOccurrence.StartTime;
							}
							if (startTime == date)
							{
								break;
							}
						}
					}
				}
				if (shouldNavigate)
				{
					SetDayViewDates(startTime.Date, true);
					EnsureVisible(startTime);
				}
				return true;
			}
			bool foundOccurrence = false;
			DateTime dateTime = MaxDate;
			foreach (SchedulerAppointment recurringAppointment in _recurrenceAppointmentCollection)
			{
				SchedulerAppointment candidateOccurrence = recurringAppointment.RecurrencePattern.FindPreviousOccurrence(date);
				if (candidateOccurrence != null)
				{
					if (candidateOccurrence.StartTime > dateTime && candidateOccurrence.StartTime <= date)
					{
						dateTime = candidateOccurrence.StartTime;
						foundOccurrence = true;
					}
					if (dateTime == date)
					{
						break;
					}
				}
			}
			if (foundOccurrence && shouldNavigate)
			{
				SetDayViewDates(dateTime.Date, true);
				EnsureVisible(dateTime);
			}
			return foundOccurrence;
		}

		internal bool CanAddAppointment(SchedulerAppointment appointment)
		{
			if (!_appointments.Contains(appointment))
			{
				AddToDataSource(appointment);
			}
			if (_isAppointmentDragActive || !_shouldRaiseChangeEvents)
			{
				return true;
			}
			AddAppointmentEventArgs e = new AddAppointmentEventArgs(appointment, _addAction);
			_addAction = AddAction.Code;
			OnAddingAppointment(e);
			return !e.Cancel;
		}

		private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, bool> _overridesRemovalHooks = new System.Collections.Concurrent.ConcurrentDictionary<Type, bool>();

		private static bool OverridesRemovalHooks(Type type)
		{
			return _overridesRemovalHooks.GetOrAdd(type, t =>
			{
				System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
				return t.GetMethod("OnRemovingAppointment", flags).DeclaringType != typeof(MxScheduler)
					|| t.GetMethod("OnAppointmentRemoved", flags).DeclaringType != typeof(MxScheduler);
			});
		}

		// True when removing every appointment one by one would raise no event and touch no editor, selection,
		// recurrence, reminder or filter state, so the collections can be emptied in one step.
		internal bool CanDropAllAppointments(SchedulerAppointmentCollection collection, out bool needsFullLayout)
		{
			needsFullLayout = false;
			if (_isAppointmentDragActive || _appointmentEditor != null || _nextReminderAppointment != null || FilterConditionApplied != null
				|| _recurrenceAppointmentCollection.Count > 0 || (_schedulerSelectedAppointments != null && _schedulerSelectedAppointments.Count > 0)
				|| base.Events[_removingAppointmentEventKey] != null || base.Events[_appointmentRemovedEventKey] != null
				|| OverridesRemovalHooks(GetType()) || _appointments.Count != collection.Count)
			{
				return false;
			}
			foreach (SchedulerAppointment item in collection)
			{
				if (item._recurrenceState != RecurrenceState.NotRecurring || item._selected || item._isRemovalSilent || !_appointments.Contains(item))
				{
					return false;
				}
				if (_view == SchedulerView.DayView && item._duration.TotalDays >= 1.0)
				{
					needsFullLayout = true;
				}
			}
			return true;
		}

		internal bool CanRemoveAppointment(SchedulerAppointment app)
		{
			if (_isAppointmentDragActive)
			{
				return true;
			}
			if (!app._isRemovalSilent)
			{
				AppointmentCancelEventArgs e = new AppointmentCancelEventArgs(app);
				OnRemovingAppointment(e);
				return !e.Cancel;
			}
			return true;
		}
	}
}
