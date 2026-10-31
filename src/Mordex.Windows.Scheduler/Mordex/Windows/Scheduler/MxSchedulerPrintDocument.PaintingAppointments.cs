using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	public partial class MxSchedulerPrintDocument
	{
		private void DrawOwnerAppointmentBars(Graphics graphics, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			int slotMinutes = 60 / (int)_interval;
			Rectangle rect = _marginBounds;
			rect.Y = _timeGridTop + 1;
			rect.Height = _timeGridBottom - rect.Y;
			Region region = new Region(rect);
			Region clip = graphics.Clip;
			graphics.Clip = region;
			Brush brush = _scheduler.GetStripBackgroundBrush();
			if (schedulerHourRangeCollectionLayoutState != null)
			{
				Rectangle barBounds = new Rectangle(schedulerHourRangeCollectionLayoutState._bounds.Left - 5, schedulerHourRangeCollectionLayoutState._bounds.Top, 5, schedulerHourRangeCollectionLayoutState._bounds.Height);
				float barTop = schedulerHourRangeCollectionLayoutState._bounds.Top;
				if (_grayShading)
				{
					if (brush != null)
					{
						graphics.FillRectangle(brush, barBounds.Left, barBounds.Top, barBounds.Width, barBounds.Height);
					}
				}
				else
				{
					graphics.FillRectangle(_backgroundBrush, barBounds);
				}
				DateTime dateTime = schedulerHourRangeCollectionLayoutState.GetDate().Add(StartTime);
				foreach (SchedulerAppointment item in _scheduler._allDayAppointments)
				{
					if (!schedulerHourRangeCollectionLayoutState.GetOwner().ValueEquals(item.Owner) || item.StartTime.Date > schedulerHourRangeCollectionLayoutState.GetDate() || dateTime.Date > schedulerHourRangeCollectionLayoutState.GetDate())
					{
						break;
					}
					if (!(item.EndTime > dateTime))
					{
						continue;
					}
					float allDaySlots = (float)item.Duration.TotalMinutes / (float)slotMinutes;
					if (item.StartTime < dateTime)
					{
						allDaySlots -= (float)dateTime.Subtract(item.StartTime).TotalMinutes / (float)slotMinutes;
					}
					float height = allDaySlots * (float)_rowHeight;
					if (item.StartTime > dateTime)
					{
						barTop += (float)item.StartTime.Subtract(dateTime).TotalMinutes / (float)slotMinutes * (float)_rowHeight;
					}
					RectangleF allDayBarRect = new RectangleF(barBounds.Left, barTop, barBounds.Width, height);
					if (_grayShading)
					{
						Brush barBrush = _scheduler.GetAppointmentBorderBrush(item);
						if (barBrush != null)
						{
							graphics.FillRectangle(barBrush, allDayBarRect);
						}
					}
					else
					{
						graphics.FillRectangle(_backgroundBrush, allDayBarRect);
					}
					barTop = (float)item.EndTime.TimeOfDay.Subtract(StartTime).TotalMinutes / (float)slotMinutes * (float)_rowHeight + (float)_timeGridTop;
					dateTime = item.EndTime;
				}
				SchedulerAppointmentOwnerState schedulerAppointmentOwnerState = _scheduler._appointmentsByOwnerDate.FindOwnerStateByDateAndOwner(schedulerHourRangeCollectionLayoutState.GetDate(), schedulerHourRangeCollectionLayoutState.GetOwner());
				ArrayList arrayList = ((schedulerAppointmentOwnerState == null) ? new ArrayList() : schedulerAppointmentOwnerState.GetAppointments());
				foreach (SchedulerAppointment dayAppointment in arrayList)
				{
					if (!schedulerHourRangeCollectionLayoutState.GetOwner().ValueEquals(dayAppointment.Owner) || dayAppointment.StartTime.Date > schedulerHourRangeCollectionLayoutState.GetDate() || dateTime.Date > schedulerHourRangeCollectionLayoutState.GetDate())
					{
						break;
					}
					if (!(dayAppointment.EndTime > dateTime))
					{
						continue;
					}
					float appointmentSlots = (float)dayAppointment.Duration.TotalMinutes / (float)slotMinutes;
					if (dayAppointment.StartTime < dateTime)
					{
						appointmentSlots -= (float)dateTime.Subtract(dayAppointment.StartTime).TotalMinutes / (float)slotMinutes;
					}
					float appointmentBarHeight = appointmentSlots * (float)_rowHeight;
					if (dayAppointment.StartTime > dateTime)
					{
						barTop += (float)dayAppointment.StartTime.Subtract(dateTime).TotalMinutes / (float)slotMinutes * (float)_rowHeight;
					}
					RectangleF appointmentBarRect = new RectangleF(barBounds.Left, barTop, barBounds.Width, appointmentBarHeight);
					if (appointmentBarRect.Bottom > (float)_marginBounds.Bottom)
					{
						appointmentBarRect.Height = (float)_marginBounds.Bottom - appointmentBarRect.Top;
					}
					if (_grayShading)
					{
						Brush appointmentBorderBrush = _scheduler.GetAppointmentBorderBrush(dayAppointment);
						if (appointmentBorderBrush != null)
						{
							graphics.FillRectangle(appointmentBorderBrush, appointmentBarRect);
						}
					}
					else
					{
						graphics.FillRectangle(_backgroundBrush, appointmentBarRect);
					}
					barTop = (float)dayAppointment.EndTime.TimeOfDay.Subtract(StartTime).TotalMinutes / (float)slotMinutes * (float)_rowHeight + (float)_timeGridTop;
					dateTime = dayAppointment.EndTime;
				}
			}
			if (_grayShading)
			{
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(PredefinedStyle.TimeNavigator);
				schedulerFormatStyle.ApplyTheme();
				Color backColor = schedulerFormatStyle.BackColor;
				backColor = ControlPaint.Dark(backColor, 0f);
				Pen pen = CreateLightLightPen(backColor);
				graphics.DrawLine(pen, _dayColumnsLeft - 5, _timeGridTop - 1, _dayColumnsLeft - 5, _timeGridBottom);
				pen.Dispose();
			}
			else
			{
				graphics.DrawLine(Pens.Black, _dayColumnsLeft - 5, _timeGridTop - 1, _dayColumnsLeft - 5, _timeGridBottom);
			}
			region.Dispose();
			region = null;
			graphics.Clip = clip;
		}

		private void DrawAppointmentBars(Graphics graphics, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
			{
				DrawOwnerAppointmentBars(graphics, schedulerHourRangeCollectionLayoutState);
				return;
			}
			int slotMinutes = 60 / (int)_interval;
			Rectangle rect = _marginBounds;
			rect.Y = _timeGridTop + 1;
			rect.Height = _timeGridBottom - rect.Y;
			Region region = new Region(rect);
			Region clip = graphics.Clip;
			graphics.Clip = region;
			Brush brush = _scheduler.GetStripBackgroundBrush();
			Rectangle barBounds = new Rectangle(schedulerHourRangeCollectionLayoutState._bounds.Left - 5, schedulerHourRangeCollectionLayoutState._bounds.Top, 5, schedulerHourRangeCollectionLayoutState._bounds.Height);
			float barTop = schedulerHourRangeCollectionLayoutState._bounds.Top;
			if (_grayShading)
			{
				if (brush != null)
				{
					graphics.FillRectangle(brush, barBounds.Left, barBounds.Top, barBounds.Width, barBounds.Height);
				}
			}
			else
			{
				graphics.FillRectangle(_backgroundBrush, barBounds);
			}
			DateTime dateTime = schedulerHourRangeCollectionLayoutState.GetDate().Add(StartTime);
			foreach (SchedulerAppointment item in _scheduler._allDayAppointments)
			{
				if (item.StartTime.Date > schedulerHourRangeCollectionLayoutState.GetDate() || dateTime.Date > schedulerHourRangeCollectionLayoutState.GetDate())
				{
					break;
				}
				if (!(item.EndTime > dateTime))
				{
					continue;
				}
				float allDaySlots = (float)item.Duration.TotalMinutes / (float)slotMinutes;
				if (item.StartTime < dateTime)
				{
					allDaySlots -= (float)dateTime.Subtract(item.StartTime).TotalMinutes / (float)slotMinutes;
				}
				float height = allDaySlots * (float)_rowHeight;
				if (item.StartTime > dateTime)
				{
					barTop += (float)item.StartTime.Subtract(dateTime).TotalMinutes / (float)slotMinutes * (float)_rowHeight;
				}
				RectangleF allDayBarRect = new RectangleF(barBounds.Left, barTop, barBounds.Width, height);
				if (allDayBarRect.Bottom > (float)_marginBounds.Bottom)
				{
					allDayBarRect.Height = (float)_marginBounds.Bottom - allDayBarRect.Top;
				}
				if (_grayShading)
				{
					Brush barBrush = _scheduler.GetAppointmentBorderBrush(item);
					if (barBrush != null)
					{
						graphics.FillRectangle(barBrush, allDayBarRect);
					}
				}
				else
				{
					graphics.FillRectangle(_backgroundBrush, allDayBarRect);
				}
				barTop = (float)item.EndTime.TimeOfDay.Subtract(StartTime).TotalMinutes / (float)slotMinutes * (float)_rowHeight + (float)_timeGridTop;
				dateTime = item.EndTime;
			}
			foreach (SchedulerAppointment dayAppointment in _scheduler._visibleAppointments)
			{
				if (dayAppointment.StartTime.Date > schedulerHourRangeCollectionLayoutState.GetDate() || dateTime.Date > schedulerHourRangeCollectionLayoutState.GetDate())
				{
					break;
				}
				if (!(dayAppointment.EndTime > dateTime))
				{
					continue;
				}
				float appointmentSlots = (float)dayAppointment.Duration.TotalMinutes / (float)slotMinutes;
				if (dayAppointment.StartTime < dateTime)
				{
					appointmentSlots -= (float)dateTime.Subtract(dayAppointment.StartTime).TotalMinutes / (float)slotMinutes;
				}
				float appointmentBarHeight = appointmentSlots * (float)_rowHeight;
				if (dayAppointment.StartTime > dateTime)
				{
					barTop += (float)dayAppointment.StartTime.Subtract(dateTime).TotalMinutes / (float)slotMinutes * (float)_rowHeight;
				}
				RectangleF appointmentBarRect = new RectangleF(barBounds.Left, barTop, barBounds.Width, appointmentBarHeight);
				if (_grayShading)
				{
					Brush appointmentBorderBrush = _scheduler.GetAppointmentBorderBrush(dayAppointment);
					if (appointmentBorderBrush != null)
					{
						graphics.FillRectangle(appointmentBorderBrush, appointmentBarRect);
					}
				}
				else
				{
					graphics.FillRectangle(_backgroundBrush, appointmentBarRect);
				}
				barTop = (float)dayAppointment.EndTime.TimeOfDay.Subtract(StartTime).TotalMinutes / (float)slotMinutes * (float)_rowHeight + (float)_timeGridTop;
				dateTime = dayAppointment.EndTime;
			}
			if (_grayShading)
			{
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(PredefinedStyle.TimeNavigator);
				schedulerFormatStyle.ApplyTheme();
				Color backColor = schedulerFormatStyle.BackColor;
				backColor = ControlPaint.Dark(backColor, 0f);
				Pen pen = CreateLightLightPen(backColor);
				graphics.DrawLine(pen, _dayColumnsLeft - 5, _timeGridTop - 1, _dayColumnsLeft - 5, _timeGridBottom);
				pen.Dispose();
			}
			else
			{
				graphics.DrawLine(Pens.Black, _dayColumnsLeft - 5, _timeGridTop - 1, _dayColumnsLeft - 5, _timeGridBottom);
			}
			region.Dispose();
			region = null;
			graphics.Clip = clip;
		}

		private void DrawSingleLineAppointment(Graphics graphics, SchedulerAppointmentView appointmentRenderer, Rectangle rectangleArgument)
		{
			Rectangle rectangle = rectangleArgument;
			if (appointmentRenderer._isMultiDay)
			{
				rectangle = DrawAppointmentBorder(graphics, appointmentRenderer.IsFirstSegment(), appointmentRenderer.IsLastSegment(), rectangleArgument);
			}
			Region region = null;
			SchedulerFormatStyle schedulerFormatStyle = null;
			SchedulerFormatStyle appointmentStyle = null;
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Near;
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			stringFormat.Trimming = StringTrimming.EllipsisCharacter;
			schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.AppointmentsFormatStyle);
			appointmentStyle = schedulerFormatStyle.MergeWith(appointmentRenderer.GetAppointment().FormatStyle);
			int firstImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex1, appointmentRenderer.GetAppointment().ImageKey1);
			int secondImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex2, appointmentRenderer.GetAppointment().ImageKey2);
			int thirdImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex3, appointmentRenderer.GetAppointment().ImageKey3);
			int fourthImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex4, appointmentRenderer.GetAppointment().ImageKey4);
			foreach (SchedulerFormatCondition formatCondition in _scheduler.FormatConditions)
			{
				if (!formatCondition.Enabled || !formatCondition.MatchesAppointment(appointmentRenderer.GetAppointment()))
				{
					continue;
				}
				int conditionFirstImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, formatCondition.ImageIndex1, formatCondition.ImageKey1);
				int conditionSecondImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, formatCondition.ImageIndex2, formatCondition.ImageKey2);
				int conditionThirdImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, formatCondition.ImageIndex3, formatCondition.ImageKey3);
				int conditionFourthImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, formatCondition.ImageIndex4, formatCondition.ImageKey4);
				if (formatCondition.AllowMerge)
				{
					appointmentStyle = appointmentStyle.MergeWith(formatCondition.FormatStyle);
					if (conditionFirstImageIndex != -1)
					{
						firstImageIndex = conditionFirstImageIndex;
					}
					if (conditionSecondImageIndex != -1)
					{
						secondImageIndex = conditionSecondImageIndex;
					}
					if (conditionThirdImageIndex != -1)
					{
						thirdImageIndex = conditionThirdImageIndex;
					}
					if (conditionFourthImageIndex != -1)
					{
						fourthImageIndex = conditionFourthImageIndex;
					}
				}
				else
				{
					appointmentStyle = schedulerFormatStyle.MergeWith(appointmentRenderer.GetAppointment().FormatStyle).MergeWith(formatCondition.FormatStyle);
					firstImageIndex = ((conditionFirstImageIndex == -1) ? SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex1, appointmentRenderer.GetAppointment().ImageKey1) : conditionFirstImageIndex);
					secondImageIndex = ((conditionSecondImageIndex == -1) ? SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex2, appointmentRenderer.GetAppointment().ImageKey2) : conditionSecondImageIndex);
					thirdImageIndex = ((conditionThirdImageIndex == -1) ? SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex3, appointmentRenderer.GetAppointment().ImageKey3) : conditionThirdImageIndex);
					fourthImageIndex = ((conditionFourthImageIndex == -1) ? SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex4, appointmentRenderer.GetAppointment().ImageKey4) : conditionFourthImageIndex);
				}
			}
			if (_grayShading)
			{
				Brush brush = appointmentStyle.GetBackBrush(rectangle);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
				}
			}
			else
			{
				graphics.FillRectangle(_backgroundBrush, rectangle);
			}
			if (appointmentRenderer._isMultiDay)
			{
				if (_scheduler.ClockStyle != ClockStyle.None)
				{
					if (appointmentRenderer.IsFirstSegment())
					{
						DrawImage(graphics, false, appointmentRenderer.GetAppointment(), rectangleArgument);
						rectangle.X += 15;
						rectangle.Width -= 15;
					}
					if (appointmentRenderer.IsLastSegment())
					{
						DrawImage(graphics, true, appointmentRenderer.GetAppointment(), rectangleArgument);
						rectangle.Width -= 15;
					}
				}
			}
			else if (appointmentRenderer._showTimeAsClocks)
			{
				if (_scheduler.ClockStyle != ClockStyle.None)
				{
					Rectangle clockBounds = DrawTimeClocks(graphics, appointmentRenderer.GetAppointment(), rectangleArgument);
					rectangle.X = clockBounds.Right + 2;
					rectangle.Width -= clockBounds.Width + 5;
					stringFormat.Alignment = StringAlignment.Near;
				}
			}
			else
			{
				rectangle = appointmentRenderer.DrawTimeRange(graphics, rectangle);
				stringFormat.Alignment = StringAlignment.Near;
			}
			Region clip = graphics.Clip;
			if (appointmentRenderer.HasAnyImage(firstImageIndex, secondImageIndex, thirdImageIndex, fourthImageIndex))
			{
				int left = rectangle.Left;
				region = (graphics.Clip = new Region(rectangle));
				appointmentRenderer.DrawImage(graphics, rectangle.Width, rectangle.Left, rectangle.Top, rectangle.Height, appointmentRenderer.GetAppointment(), firstImageIndex, secondImageIndex, thirdImageIndex, fourthImageIndex, false);
				rectangle.X = appointmentRenderer._textStartX;
				rectangle.Width -= rectangle.X - left;
				stringFormat.Alignment = StringAlignment.Near;
				if (region != null)
				{
					region.Dispose();
					region = null;
					graphics.Clip = clip;
				}
			}
			string text = "";
			text = ((appointmentRenderer.GetAppointment().Prefix != "") ? (appointmentRenderer.GetAppointment().Prefix + " " + appointmentRenderer.GetAppointment().Text) : ((!(_scheduler.AppointmentsPrefix != "")) ? appointmentRenderer.GetAppointment().Text : (_scheduler.AppointmentsPrefix + " " + appointmentRenderer.GetAppointment().Text)));
			if (text.Length > 0)
			{
				if (appointmentRenderer.GetAppointment().Duration.TotalDays >= 1.0)
				{
					stringFormat.Alignment = _scheduler.AllDayEventsAlignment;
				}
				if (_grayShading)
				{
					Color color = appointmentStyle.ResolveForeColor(_scheduler.GetThemedForeColor());
					Brush textBrush = new SolidBrush(color);
					graphics.DrawString(text, appointmentStyle.ResolveFont(GetViewFont()), textBrush, rectangle, stringFormat);
					textBrush.Dispose();
				}
				else
				{
					graphics.DrawString(text, appointmentStyle.ResolveFont(GetViewFont()), _textBrush, rectangle, stringFormat);
				}
			}
			appointmentStyle?.DisposeResources();
			schedulerFormatStyle?.DisposeResources();
			stringFormat.Dispose();
			stringFormat = null;
		}

		private Rectangle DrawAppointmentBorder(Graphics graphics, bool shouldDrawLeftEdge, bool booleanArgument, Rectangle rectangle)
		{
			Pen pen = GetAppointmentOutlinePen();
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right, rectangle.Bottom - 1);
			if (shouldDrawLeftEdge)
			{
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom);
				rectangle.X++;
				rectangle.Width--;
			}
			if (booleanArgument)
			{
				graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom);
				rectangle.Width--;
			}
			rectangle.Y++;
			rectangle.Height -= 2;
			return rectangle;
		}

		private void DrawAppointments(Graphics graphics, int firstColumn, int integerArgument, int maxWidth)
		{
			if (_scheduler.View == SchedulerView.DayView || _scheduler.View == SchedulerView.WorkWeek)
			{
				foreach (SchedulerAppointmentView item in _allDayRenderers)
				{
					DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.Appointments, item._bounds, item.GetAppointment());
					_scheduler.RaiseDrawSchedulerArea(e);
					if (!e.Handled)
					{
						Rectangle field_ = item._bounds;
						if (maxWidth > 0 && field_.Width > maxWidth)
						{
							field_.Width = maxWidth - 10;
						}
						DrawSingleLineAppointment(graphics, item, field_);
					}
				}
				Region clip = graphics.Clip;
				Rectangle rectangle = _marginBounds;
				rectangle.Y = _timeGridTop;
				rectangle.Height = _timeGridBottom - rectangle.Y;
				Region region = (graphics.Clip = new Region(rectangle));
				foreach (SchedulerAppointmentView timedRenderer in _appointmentRenderers)
				{
					DrawSchedulerAreaEventArgs timedEventArgs = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.Appointments, timedRenderer._bounds, timedRenderer.GetAppointment());
					_scheduler.RaiseDrawSchedulerArea(timedEventArgs);
					if (!timedEventArgs.Handled)
					{
						DrawBackground(graphics, timedRenderer, timedRenderer._bounds, rectangle);
					}
				}
				region.Dispose();
				region = null;
				graphics.Clip = clip;
				return;
			}
			foreach (SchedulerAppointmentView monthRenderer in _appointmentRenderers)
			{
				Rectangle bounds = monthRenderer._bounds;
				SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = _dayLayouts.FindLayoutByDate(monthRenderer._date);
				if (_layoutPages == LayoutPages.TwoPages)
				{
					if (_pageNumber == 1)
					{
						if (schedulerHourRangeCollectionLayoutState.GetColumnIndex() > integerArgument)
						{
							continue;
						}
					}
					else if (_scheduler.View == SchedulerView.MonthView)
					{
						bounds.X -= _pageWidth;
					}
					else if (_pageNumber == 2)
					{
						if (schedulerHourRangeCollectionLayoutState.GetColumnIndex() < firstColumn)
						{
							continue;
						}
						bounds.X -= _pageWidth;
					}
				}
				DrawSchedulerAreaEventArgs monthEventArgs = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.Appointments, bounds, monthRenderer.GetAppointment());
				_scheduler.RaiseDrawSchedulerArea(monthEventArgs);
				if (!monthEventArgs.Handled)
				{
					DrawSingleLineAppointment(graphics, monthRenderer, bounds);
				}
			}
		}

		private void DrawTimeSlotRows(Graphics graphics, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			Pen pen = null;
			pen = (_grayShading ? _scheduler.GetGridLinePen() : Pens.Black);
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.HourBackgroundStyle);
			SchedulerFormatStyle alternatingHourStyle = schedulerFormatStyle.MergeWith(_scheduler.AlternatingHourBackgroundStyle);
			SchedulerFormatStyle workingHourStyle = schedulerFormatStyle.MergeWith(_scheduler.WorkingHourBackgroundStyle);
			SchedulerFormatStyle alternatingWorkingHourStyle = workingHourStyle.MergeWith(_scheduler.AlternatingWorkingHourBackgroundStyle);
			for (int i = 0; i < schedulerHourRangeCollectionLayoutState.GetTimeSlots().Count; i++)
			{
				SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = schedulerHourRangeCollectionLayoutState.GetTimeSlots().GetLayoutAt(i);
				Rectangle bounds = schedulerAppointmentOwnerLayoutState._bounds;
				DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.DayViewRow, schedulerAppointmentOwnerLayoutState._bounds, schedulerAppointmentOwnerLayoutState);
				_scheduler.RaiseDrawSchedulerArea(e);
				if (!e.Handled)
				{
					if (_grayShading)
					{
						DrawTimeSlotBackground(graphics, schedulerAppointmentOwnerLayoutState, schedulerFormatStyle, alternatingHourStyle, workingHourStyle, alternatingWorkingHourStyle, schedulerHourRangeCollectionLayoutState);
					}
					graphics.DrawLine(pen, bounds.Left + 1, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
				}
			}
			graphics.DrawLine(pen, schedulerHourRangeCollectionLayoutState._bounds.Left, schedulerHourRangeCollectionLayoutState._bounds.Top, schedulerHourRangeCollectionLayoutState._bounds.Left, schedulerHourRangeCollectionLayoutState._bounds.Bottom);
			graphics.DrawLine(pen, schedulerHourRangeCollectionLayoutState._bounds.Right, schedulerHourRangeCollectionLayoutState._bounds.Top, schedulerHourRangeCollectionLayoutState._bounds.Right, schedulerHourRangeCollectionLayoutState._bounds.Bottom);
			alternatingWorkingHourStyle.DisposeResources();
			workingHourStyle.DisposeResources();
			alternatingHourStyle.DisposeResources();
			schedulerFormatStyle.DisposeResources();
		}
	}
}
