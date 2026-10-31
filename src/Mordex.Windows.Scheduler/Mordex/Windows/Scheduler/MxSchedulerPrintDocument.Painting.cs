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
		private void RenderContent(Graphics graphics, Point point)
		{
			try
			{
				_workingHoursSchemaLayoutState = new WorkingHoursSchemaLayoutState(null, "MMMM yyyy");
				SizeF sizeF = graphics.MeasureString("88", _miniCalendarFont);
				int height = new Size((int)sizeF.Width, (int)sizeF.Height).Height + 6;
				sizeF = graphics.MeasureString("88", _boldMiniCalendarFont);
				Size size = new Size((int)sizeF.Width, (int)sizeF.Height);
				int cellWidth = size.Width + 2;
				int cellHeight = size.Height + 3;
				for (int i = 0; i < 7; i++)
				{
					sizeF = graphics.MeasureString(_textArray[i], _miniCalendarFont);
					size = new Size((int)sizeF.Width, (int)sizeF.Height);
					if (size.Width + 4 > cellWidth)
					{
						cellWidth = size.Width + 2;
					}
				}
				int sideMargin = (int)((double)cellWidth * 0.66);
				Rectangle rectangle = new Rectangle(point.X, point.Y, sideMargin * 2 + cellWidth * 7, height);
				_workingHoursSchemaLayoutState.SetHeaderBounds(rectangle);
				_workingHoursSchemaLayoutState.LayoutGrid(cellWidth, cellHeight, sideMargin, new Point(0, 0));
			}
			catch
			{
			}
		}

		private void DrawAlignedText(Graphics graphics, string text, Font font, Brush brush, RectangleF layoutRectangle, StringAlignment stringAlignment, StringAlignment stringAlignmentArgument, bool isSingleLine)
		{
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = stringAlignment;
			stringFormat.LineAlignment = stringAlignmentArgument;
			if (isSingleLine)
			{
				stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			}
			if (brush != null)
			{
				graphics.DrawString(text, font, brush, layoutRectangle, stringFormat);
			}
			stringFormat.Dispose();
			stringFormat = null;
		}

		private void DrawText(Graphics graphics, WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState)
		{
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			Pen pen = null;
			if (_grayShading)
			{
				Color color = SystemColors.ControlDark;
				pen = new Pen(color);
			}
			else
			{
				pen = new Pen(Color.Black);
			}
			for (int i = 0; i < 7; i++)
			{
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = workingHoursSchemaLayoutState.GetWeekdayHeaderCell(i);
				Rectangle rectangle = schedulerFormatStyleLayoutState.GetBounds();
				rectangle.Height--;
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
				RectangleF layoutRectangle = rectangle;
				graphics.DrawString(_textArray[i], _miniCalendarFont, _textBrush, layoutRectangle, stringFormat);
			}
			Brush brush = null;
			brush = new SolidBrush(SystemColors.GrayText);
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutStateArgument = workingHoursSchemaLayoutState.GetDayCell(i, j);
					Brush cellBrush = ((!schedulerFormatStyleLayoutStateArgument.IsOutsideMonth()) ? _textBrush : brush);
					Rectangle rectangle = schedulerFormatStyleLayoutStateArgument.GetBounds();
					RectangleF layoutRectangle = rectangle;
					graphics.DrawString(font: (!schedulerFormatStyleLayoutStateArgument.IsBold()) ? _miniCalendarFont : _boldMiniCalendarFont, s: schedulerFormatStyleLayoutStateArgument.GetText(), brush: cellBrush, layoutRectangle: layoutRectangle, format: stringFormat);
				}
			}
			pen.Dispose();
			brush.Dispose();
			stringFormat.Dispose();
			stringFormat = null;
		}

		private void DrawClockHand(Graphics graphics, Pen pen, PointF pointF, int angle, float floatValue, float floatValueArgument)
		{
			float x = pointF.X + floatValueArgument * (float)Math.Cos((double)angle * Math.PI / 180.0);
			float y = pointF.Y + floatValueArgument * (float)Math.Sin((double)angle * Math.PI / 180.0);
			PointF pt = new PointF(x, y);
			x = pointF.X + floatValue * (float)Math.Cos((double)angle * Math.PI / 180.0);
			y = pointF.Y + floatValue * (float)Math.Sin((double)angle * Math.PI / 180.0);
			PointF innerPoint = new PointF(x, y);
			graphics.DrawLine(pen, pt, innerPoint);
		}

		private void DrawImage(Graphics graphics, bool isEndTime, SchedulerAppointment schedulerAppointment, Rectangle rectangle)
		{
			float x = rectangle.Left + 1;
			Pen pen = GetAppointmentOutlinePen();
			Brush brush = null;
			float clockSize = 14f;
			float y = (float)rectangle.Top + ((float)rectangle.Height - clockSize) / 2f;
			bool isPm = false;
			float hourValue;
			int minutes;
			if (isEndTime)
			{
				x = (float)rectangle.Right - clockSize - 2f;
				hourValue = (float)schedulerAppointment.EndTime.TimeOfDay.TotalHours;
				minutes = schedulerAppointment.EndTime.TimeOfDay.Minutes;
				if (schedulerAppointment.EndTime.TimeOfDay.TotalHours >= 12.0)
				{
					pen = Pens.White;
					brush = new SolidBrush(_appointmentOutlinePen.Color);
					isPm = true;
				}
			}
			else
			{
				hourValue = schedulerAppointment.StartTime.TimeOfDay.Hours;
				minutes = schedulerAppointment.StartTime.TimeOfDay.Minutes;
				if (schedulerAppointment.StartTime.TimeOfDay.TotalHours >= 12.0)
				{
					pen = Pens.White;
					brush = new SolidBrush(_appointmentOutlinePen.Color);
					isPm = true;
				}
			}
			if (hourValue > 12f)
			{
				hourValue -= 12f;
			}
			hourValue -= 3f;
			minutes -= 15;
			RectangleF rect = new RectangleF(x, y, clockSize, clockSize);
			float radius = rect.Width / 2f;
			PointF pointF = new PointF(rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f);
			if (_scheduler.ClockStyle == ClockStyle.Flat || !_grayShading)
			{
				graphics.DrawEllipse(_appointmentOutlinePen, rect);
				if (brush != null)
				{
					graphics.FillEllipse(brush, rect);
				}
				for (int i = 0; i <= 360; i += 30)
				{
					DrawClockHand(graphics, pen, pointF, i, radius - 1f, radius);
				}
			}
			else
			{
				pen = Pens.Black;
				Bitmap bitmap = null;
				bitmap = ((!isPm) ? SchedulerRenderer.MakeMagentaTransparent(SchedulerImageResources.GetDayClockBitmap()) : SchedulerRenderer.MakeMagentaTransparent(SchedulerImageResources.GetNightClockBitmap()));
				if ((float)rectangle.Right > rect.Right && (float)rectangle.Left <= rect.Left)
				{
					graphics.DrawImageUnscaled(bitmap, (int)rect.X, (int)rect.Y);
				}
			}
			float hourAngle = ((!(hourValue < 0f)) ? (hourValue * 30f) : (360f + hourValue * 30f));
			int minuteAngle = ((minutes >= 0) ? (minutes * 6) : (360 + minutes * 6));
			DrawClockHand(graphics, pen, pointF, (int)hourAngle, 0f, radius / 2f - 1f);
			DrawClockHand(graphics, pen, pointF, minuteAngle, 0f, radius - 2f);
			if (isPm)
			{
				brush.Dispose();
				brush = null;
			}
		}

		private void DrawBackground(Graphics graphics, SchedulerAppointmentView appointmentRenderer, Rectangle rectangleArgument, Rectangle rect)
		{
			Region region = null;
			Region clip = graphics.Clip;
			Rectangle rectangle = new Rectangle(rectangleArgument.Left + 5, rectangleArgument.Top, rectangleArgument.Width - 5, rectangleArgument.Height);
			graphics.DrawRectangle(GetAppointmentOutlinePen(), rectangle.Left, rectangle.Top, rectangle.Width - 1, rectangle.Height - 1);
			Rectangle innerBounds = rectangle;
			innerBounds.Inflate(-1, -1);
			SchedulerFormatStyle schedulerFormatStyle = null;
			SchedulerFormatStyle appointmentStyle = null;
			schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.AppointmentsFormatStyle);
			appointmentStyle = schedulerFormatStyle.MergeWith(appointmentRenderer.GetAppointment().FormatStyle);
			int firstImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex1, appointmentRenderer.GetAppointment().ImageKey1);
			int secondImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex2, appointmentRenderer.GetAppointment().ImageKey2);
			int thirdImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex3, appointmentRenderer.GetAppointment().ImageKey3);
			int fourthImageIndex = SchedulerRenderer.GetImageIndex(appointmentRenderer._imageList, appointmentRenderer.GetAppointment().ImageIndex4, appointmentRenderer.GetAppointment().ImageKey4);
			Color borderColor = appointmentRenderer.GetAppointment().BorderColor;
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
				if (!formatCondition.BorderColor.IsEmpty)
				{
					borderColor = formatCondition.BorderColor;
				}
			}
			appointmentRenderer.DrawBackground(graphics, appointmentRenderer._bounds, borderColor, _grayShading, GetAppointmentOutlinePen());
			if (_grayShading)
			{
				Brush brush = appointmentStyle.GetBackBrush(innerBounds);
				if (brush != null)
				{
					graphics.FillRectangle(brush, innerBounds);
				}
			}
			else
			{
				graphics.FillRectangle(_backgroundBrush, innerBounds);
			}
			rect.Intersect(innerBounds);
			region = (graphics.Clip = new Region(rect));
			if (appointmentRenderer.HasAnyImage(firstImageIndex, secondImageIndex, thirdImageIndex, fourthImageIndex))
			{
				appointmentRenderer.DrawImage(graphics, innerBounds.Width, innerBounds.Left, innerBounds.Top, innerBounds.Height, appointmentRenderer.GetAppointment(), firstImageIndex, secondImageIndex, thirdImageIndex, fourthImageIndex, false);
				_appointmentTextLeft = appointmentRenderer._textStartX;
			}
			else
			{
				_appointmentTextLeft = innerBounds.Left;
			}
			_ = appointmentRenderer.GetAppointment().Text;
			Rectangle textBounds = new Rectangle(_appointmentTextLeft, innerBounds.Top, innerBounds.Right - _appointmentTextLeft, rectangleArgument.Height);
			if (innerBounds.Top < _timeGridTop)
			{
				int clippedHeight = _timeGridTop - innerBounds.Top;
				textBounds.Y += clippedHeight;
				textBounds.Height -= clippedHeight;
				appointmentRenderer.SetStartTimeOfDay(StartTime);
			}
			string text = "";
			text = ((appointmentRenderer.GetAppointment().Prefix != "") ? (appointmentRenderer.GetAppointment().Prefix + " " + appointmentRenderer.GetAppointment().Text) : ((!(_scheduler.AppointmentsPrefix != "")) ? appointmentRenderer.GetAppointment().Text : (_scheduler.AppointmentsPrefix + " " + appointmentRenderer.GetAppointment().Text)));
			if (_grayShading)
			{
				appointmentRenderer.DrawAppointmentText(graphics, text, textBounds, GetFormatStyleFont(appointmentStyle), appointmentStyle.ResolveForeColor(_scheduler.GetThemedForeColor()), ShouldShowTimeHint(appointmentRenderer));
			}
			else
			{
				appointmentRenderer.DrawAppointmentText(graphics, text, textBounds, GetFormatStyleFont(appointmentStyle), Color.Black, ShouldShowTimeHint(appointmentRenderer));
			}
			appointmentStyle?.DisposeResources();
			schedulerFormatStyle?.DisposeResources();
			if (region != null)
			{
				region.Dispose();
				region = null;
				graphics.Clip = clip;
			}
		}

		protected override void OnPrintPage(PrintPageEventArgs e)
		{
			base.OnPrintPage(e);
			try
			{
				if (!_scheduler.Created)
				{
					_scheduler.CreateControl();
				}
				_scheduler.InvalidateWithChildren();
				_scheduler.RenderCurrentView(e.Graphics);
				if (_scheduler._allDayAreaControl != null)
				{
					_scheduler._allDayAreaControl.LayoutHeaders(e.Graphics);
				}
				_scheduler._isPrinting = true;
				_isWeekendCompressed = _compressWeekEnds || _layoutPages == LayoutPages.TwoPages;
				Region region = new Region(e.MarginBounds);
				Region clip = e.Graphics.Clip;
				e.Graphics.Clip = region;
				_marginBounds = e.MarginBounds;
				_pageWidth = e.MarginBounds.Width;
				_pageRight = e.MarginBounds.Right;
				int lastDayColumn = 7;
				_totalPageCount = 1;
				if (_layoutPages == LayoutPages.TwoPages && _scheduler.View != SchedulerView.DayView && _scheduler.View != SchedulerView.WorkWeek)
				{
					_marginBounds.Width *= 2;
					lastDayColumn = 3;
					_isWeekendCompressed = true;
					_totalPageCount = 2;
				}
				else if (_isWeekendCompressed && _scheduler.View == SchedulerView.MonthView)
				{
					lastDayColumn = 6;
				}
				if (_pageNumber == 1)
				{
					_backgroundBrush = Brushes.White;
					_textBrush = Brushes.Black;
					if (_scheduler.View == SchedulerView.WeekView)
					{
						if (_showDateHeader)
						{
							DrawWeekViewDateHeader(e.Graphics);
						}
						LayoutWeekView(e.Graphics);
						DrawWeekViewBackground(e.Graphics);
						DrawAppointments(e.Graphics, 1, 1, -1);
					}
					else if (_scheduler.View == SchedulerView.DayView || _scheduler.View == SchedulerView.WorkWeek)
					{
						_timeGridTop = 0;
						if (_currentDayIndex < 0)
						{
							if (_showDateHeader)
							{
								DrawDayViewDateHeader(e.Graphics, _scheduler._dayLayouts.GetLayoutAt(0).GetDate());
							}
							InitDayViewPrint(e.Graphics);
							_currentDayIndex = 0;
						}
						else
						{
							if (_showDateHeader)
							{
								DrawDayViewDateHeader(e.Graphics, _scheduler._dayLayouts.GetLayoutAt(_currentDayIndex).GetDate());
							}
							_extraAllDayRowCount = 0;
						}
						if (DayViewPrintMode == DayViewPrintMode.PagePerDay)
						{
							ClearAppointmentRenderers();
							SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState = _scheduler._dayLayouts.GetLayoutAt(_currentDayIndex);
							LayoutDayColumn(e.Graphics, schedulerHourRangeCollectionLayoutState);
							schedulerHourRangeCollectionLayoutState = _dayLayouts.GetLayoutAt(_currentDayIndex);
							LayoutDayAppointments(schedulerHourRangeCollectionLayoutState);
							DrawSchedulerAreaEventArgs backgroundEventArgs = new DrawSchedulerAreaEventArgs(e.Graphics, _scheduler, OwnerDrawnArea.ControlBackground, _marginBounds, null);
							_scheduler.RaiseDrawSchedulerArea(backgroundEventArgs);
							if (!backgroundEventArgs.Handled)
							{
								_scheduler.DrawControlBackground(e.Graphics, _marginBounds, _gridBounds, _scheduler.BackgroundImage, _gridBounds);
							}
							DrawTimeSlotRows(e.Graphics, schedulerHourRangeCollectionLayoutState);
							DrawTimeColumn(e.Graphics);
							DrawAllDayAreaBackground(e.Graphics);
							DrawDayHeader(e.Graphics, _marginBounds.Top + 1, schedulerHourRangeCollectionLayoutState.GetDate());
							DrawAppointmentBars(e.Graphics, schedulerHourRangeCollectionLayoutState);
							DrawAppointments(e.Graphics, 0, 0, schedulerHourRangeCollectionLayoutState._bounds.Width);
						}
						else
						{
							LayoutMultiDayColumns(e.Graphics);
							if (_showDateHeader)
							{
								DrawAllDayAreaBackground(e.Graphics);
								DrawDayViewHeaders(e.Graphics, _marginBounds.Top + 1);
							}
							else
							{
								DrawAllDayAreaBackground(e.Graphics);
								DrawDayViewHeaders(e.Graphics, _marginBounds.Top + 1);
							}
							DrawSchedulerAreaEventArgs pageBackgroundArgs = new DrawSchedulerAreaEventArgs(e.Graphics, _scheduler, OwnerDrawnArea.ControlBackground, _marginBounds, null);
							_scheduler.RaiseDrawSchedulerArea(pageBackgroundArgs);
							if (!pageBackgroundArgs.Handled)
							{
								_scheduler.DrawControlBackground(e.Graphics, _marginBounds, _gridBounds, _scheduler.BackgroundImage, _gridBounds);
							}
							DrawTimeColumn(e.Graphics);
							foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
							{
								ClearAppointmentRenderers();
								LayoutDayAppointments(item);
								DrawTimeSlotRows(e.Graphics, item);
								DrawAppointmentBars(e.Graphics, item);
								DrawAppointments(e.Graphics, 0, 0, -1);
							}
						}
					}
					else
					{
						if (_showDateHeader)
						{
							DrawMonthViewDateHeader(e.Graphics);
						}
						LayoutMonthView(e.Graphics);
						DrawMonthViewDays(e.Graphics, 1, lastDayColumn);
						DrawAppointments(e.Graphics, 1, lastDayColumn, -1);
					}
				}
				else if (_scheduler.View == SchedulerView.WeekView)
				{
					if (_showDateHeader)
					{
						DrawWeekViewDateHeader(e.Graphics);
					}
					DrawWeekViewBackground(e.Graphics);
					DrawAppointments(e.Graphics, 2, 2, -1);
				}
				else if (_scheduler.View == SchedulerView.MonthView)
				{
					if (_showDateHeader)
					{
						DrawMonthViewDateHeader(e.Graphics);
					}
					DrawMonthViewDays(e.Graphics, 4, 6);
					DrawAppointments(e.Graphics, 4, 6, -1);
				}
				if (_showDateHeader)
				{
					if (LayoutPages == LayoutPages.OnePage || _scheduler.View == SchedulerView.DayView || _scheduler.View == SchedulerView.WorkWeek)
					{
						DrawMiniCalendars(e.Graphics, e.MarginBounds);
					}
					else if (_pageNumber == 2)
					{
						DrawMiniCalendars(e.Graphics, e.MarginBounds);
					}
				}
				region.Dispose();
				region = null;
				e.Graphics.Clip = clip;
				if (_shouldNumberPages)
				{
					if (_scheduler.View == SchedulerView.DayView || _scheduler.View == SchedulerView.WorkWeek)
					{
						_footerCenter = (_currentDayIndex + 1).ToString();
					}
					else
					{
						_footerCenter = _pageNumber.ToString();
					}
				}
				DrawHeadersAndFooters(e.Graphics, e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Width, e.MarginBounds.Height);
				if (_scheduler.View != SchedulerView.DayView && _scheduler.View != SchedulerView.WorkWeek)
				{
					if (_layoutPages == LayoutPages.TwoPages && _pageNumber == 1)
					{
						e.HasMorePages = true;
						_pageNumber++;
					}
					else
					{
						e.HasMorePages = false;
						_pageNumber = 1;
					}
				}
				else if (DayViewPrintMode == DayViewPrintMode.PagePerDay && _scheduler._dayLayouts.Count - 1 > _currentDayIndex)
				{
					_currentDayIndex++;
					e.HasMorePages = true;
				}
				else
				{
					_currentDayIndex = -1;
				}
				_scheduler._isPrinting = false;
			}
			catch
			{
				e.HasMorePages = false;
				_scheduler._isPrinting = false;
			}
		}
	}
}
