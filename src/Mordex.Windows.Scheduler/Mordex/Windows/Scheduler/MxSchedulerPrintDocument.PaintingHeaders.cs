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
		private void DrawHeadersAndFooters(Graphics graphics, int left, int integerArgument, int width, int height)
		{
			Size empty = Size.Empty;
			StringFormat stringFormat = new StringFormat();
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			Rectangle emptyBounds = Rectangle.Empty;
			int pageCount = _totalPageCount;
			int pageNumber = _pageNumber;
			if (_scheduler.View == SchedulerView.DayView || _scheduler.View == SchedulerView.WorkWeek)
			{
				pageCount = ((_dayViewPrintMode == DayViewPrintMode.PagePerView) ? 1 : _scheduler._dayLayouts.Count);
				pageNumber = _currentDayIndex + 1;
			}
			if (_headerLeft.Length > 0)
			{
				string text = ReplaceHeaderFooterPlaceholders(_headerLeft, pageNumber, pageCount);
				empty = graphics.MeasureString(text, HeaderFont, 0, stringFormat).ToSize();
				graphics.DrawString(layoutRectangle: new Rectangle(left, integerArgument - empty.Height, empty.Width + 1, empty.Height + 1), s: text, font: HeaderFont, brush: _textBrush, format: stringFormat);
			}
			if (_headerCenter.Length > 0)
			{
				string headerCenterText = ReplaceHeaderFooterPlaceholders(_headerCenter, pageNumber, pageCount);
				empty = graphics.MeasureString(headerCenterText, HeaderFont, 0, stringFormat).ToSize();
				graphics.DrawString(layoutRectangle: new Rectangle(left + (width - empty.Width) / 2, integerArgument - empty.Height, empty.Width + 1, empty.Height + 1), s: headerCenterText, font: HeaderFont, brush: _textBrush, format: stringFormat);
			}
			if (_footerLeft.Length > 0)
			{
				string footerLeftText = ReplaceHeaderFooterPlaceholders(_footerLeft, pageNumber, pageCount);
				empty = graphics.MeasureString(footerLeftText, FooterFont, 0, stringFormat).ToSize();
				graphics.DrawString(layoutRectangle: new Rectangle(left, integerArgument + height, empty.Width + 1, empty.Height + 1), s: footerLeftText, font: FooterFont, brush: _textBrush, format: stringFormat);
			}
			if (_footerCenter.Length > 0)
			{
				string footerCenterText = ReplaceHeaderFooterPlaceholders(_footerCenter, pageNumber, pageCount);
				empty = graphics.MeasureString(footerCenterText, FooterFont, 0, stringFormat).ToSize();
				graphics.DrawString(layoutRectangle: new Rectangle(left + (width - empty.Width) / 2, integerArgument + height, empty.Width + 1, empty.Height + 1), s: footerCenterText, font: FooterFont, brush: _textBrush, format: stringFormat);
			}
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			if (_headerRight.Length > 0)
			{
				string headerRightText = ReplaceHeaderFooterPlaceholders(_headerRight, pageNumber, pageCount);
				empty = graphics.MeasureString(headerRightText, HeaderFont, 0, stringFormat).ToSize();
				graphics.DrawString(layoutRectangle: new Rectangle(left + width - empty.Width, integerArgument - empty.Height, empty.Width + 1, empty.Height + 1), s: headerRightText, font: HeaderFont, brush: _textBrush, format: stringFormat);
			}
			if (_footerRight.Length > 0)
			{
				string footerRightText = ReplaceHeaderFooterPlaceholders(_footerRight, pageNumber, pageCount);
				empty = graphics.MeasureString(footerRightText, FooterFont, 0, stringFormat).ToSize();
				graphics.DrawString(layoutRectangle: new Rectangle(left + width - empty.Width, integerArgument + height, empty.Width + 1, empty.Height + 1), s: footerRightText, font: FooterFont, brush: _textBrush, format: stringFormat);
			}
			stringFormat.Dispose();
			stringFormat = null;
		}

		private void DrawMiniCalendars(Graphics graphics, Rectangle rectangle)
		{
			Point point = new Point(rectangle.Right - 110, rectangle.Top + 5);
			UpdateSchedulerPrintDocument();
			RenderContent(graphics, point);
			DateTime dateTime = DateTime.MinValue;
			DateTime minValue = DateTime.MinValue;
			if (_scheduler.IsValidRange())
			{
				dateTime = _scheduler.DateRange.Start;
				minValue = _scheduler.DateRange.End;
			}
			else
			{
				minValue = _scheduler.Dates[0];
			}
			_workingHoursSchemaLayoutState.SetYear(minValue.Year);
			_workingHoursSchemaLayoutState.SetMonth(minValue.Month);
			_workingHoursSchemaLayoutState.BuildDayCells(false, false, ToCalendarDayOfWeek(_scheduler.FirstDayOfWeek));
			DrawAlignedText(graphics, _workingHoursSchemaLayoutState.GetHeaderText(), _miniCalendarFont, _textBrush, _workingHoursSchemaLayoutState.GetHeaderBounds(), StringAlignment.Center, StringAlignment.Center, false);
			DrawText(graphics, _workingHoursSchemaLayoutState);
			if (dateTime != DateTime.MinValue && dateTime.Month != minValue.Month)
			{
				point.Offset(-110, 0);
				RenderContent(graphics, point);
				_workingHoursSchemaLayoutState.SetYear(dateTime.Year);
				_workingHoursSchemaLayoutState.SetMonth(dateTime.Month);
				_workingHoursSchemaLayoutState.BuildDayCells(false, false, ToCalendarDayOfWeek(_scheduler.FirstDayOfWeek));
				DrawAlignedText(graphics, _workingHoursSchemaLayoutState.GetHeaderText(), _miniCalendarFont, _textBrush, _workingHoursSchemaLayoutState.GetHeaderBounds(), StringAlignment.Center, StringAlignment.Center, false);
				DrawText(graphics, _workingHoursSchemaLayoutState);
			}
		}

		private void DrawMonthViewDays(Graphics graphics, int firstColumn, int integerArgument)
		{
			Pen pen = null;
			pen = (_grayShading ? _scheduler.GetGridLinePen() : Pens.Black);
			_ = _scheduler.MonthFont;
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Near;
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			SchedulerFormatStyle schedulerFormatStyle = null;
			_scheduler.DrawControlBackground(graphics, _marginBounds, _gridBounds, _scheduler.BackgroundImage, _gridBounds);
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				Rectangle bounds = item._bounds;
				if (item.GetColumnIndex() > integerArgument || item.GetColumnIndex() < firstColumn)
				{
					continue;
				}
				if (_pageNumber > 1)
				{
					bounds.X -= _pageWidth;
				}
				DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.MonthViewDay, bounds, item);
				_scheduler.RaiseDrawSchedulerArea(e);
				if (e.Handled)
				{
					continue;
				}
				schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.MonthDaysFormatStyle);
				if (item.GetDate().Month % 2 == 0)
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(_scheduler.AlternatingMonthDaysFormatStyle);
				}
				if (item.GetSchedulerFormatStyle() != null)
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(item.GetSchedulerFormatStyle());
				}
				if (_grayShading)
				{
					Brush brush = schedulerFormatStyle.GetBackBrush(bounds);
					if (brush != null)
					{
						graphics.FillRectangle(brush, bounds);
					}
				}
				graphics.DrawLine(pen, bounds.Right, bounds.Top, bounds.Right, bounds.Bottom - 1);
				graphics.DrawLine(pen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
				bounds.Inflate(-1, -1);
				bounds.Width--;
				if (_grayShading)
				{
					Brush captionBrush = schedulerFormatStyle.GetForeColorBrush(_scheduler.GetThemedForeColor());
					if (captionBrush != null)
					{
						graphics.DrawString(item.GetCaption(), schedulerFormatStyle.ResolveFont(MonthFont), captionBrush, bounds, stringFormat);
					}
				}
				else
				{
					graphics.DrawString(item.GetCaption(), schedulerFormatStyle.ResolveFont(MonthFont), _textBrush, bounds, stringFormat);
				}
				bounds = item._bounds;
				bounds.X++;
				bounds.Y++;
				schedulerFormatStyle.DisposeResources();
			}
			DrawHeaders(graphics, firstColumn, integerArgument);
			graphics.DrawRectangle(pen, _gridBounds.Left, _marginBounds.Top, _pageWidth, _marginBounds.Bottom);
			stringFormat.Dispose();
		}

		private void DrawWeekViewBackground(Graphics graphics)
		{
			Pen pen = null;
			pen = (_grayShading ? _scheduler.GetGridLinePen() : Pens.Black);
			SchedulerFormatStyle schedulerFormatStyle = null;
			_scheduler.DrawControlBackground(graphics, _marginBounds, _gridBounds, _scheduler.BackgroundImage, _gridBounds);
			foreach (SchedulerHourRangeCollectionLayoutState item in _dayLayouts)
			{
				Rectangle bounds = item._bounds;
				DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.WeekViewDay, bounds, item);
				_scheduler.RaiseDrawSchedulerArea(e);
				if (e.Handled || (item.GetColumnIndex() != _pageNumber && _layoutPages == LayoutPages.TwoPages))
				{
					continue;
				}
				if (bounds.Left >= _pageRight)
				{
					if (_pageNumber <= 1)
					{
						continue;
					}
					bounds.X -= _pageWidth;
				}
				bounds.Y += _rowHeight;
				bounds.Height -= _rowHeight;
				schedulerFormatStyle = new SchedulerFormatStyle(_scheduler.WeekDaysBackgroundStyle);
				if (item.GetSchedulerFormatStyle() != null)
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(item.GetSchedulerFormatStyle());
				}
				if (_grayShading)
				{
					Brush brush = schedulerFormatStyle.GetBackBrush(bounds);
					if (brush != null)
					{
						graphics.FillRectangle(brush, bounds);
					}
				}
				schedulerFormatStyle.DisposeResources();
			}
			if (_layoutPages == LayoutPages.TwoPages)
			{
				if (_pageNumber > 1)
				{
					DrawHeaders(graphics, 2, 2);
				}
				else
				{
					DrawHeaders(graphics, 1, 1);
				}
			}
			else
			{
				DrawHeaders(graphics, 1, 2);
			}
			graphics.DrawRectangle(pen, _gridBounds.Left, _gridBounds.Top, _pageWidth, _gridBounds.Height);
			foreach (SchedulerHourRangeCollectionLayoutState dayLayout in _dayLayouts)
			{
				if (dayLayout.GetColumnIndex() != _pageNumber)
				{
					continue;
				}
				Rectangle boundsArgument = dayLayout._bounds;
				if (boundsArgument.Left >= _pageRight)
				{
					if (_pageNumber <= 1)
					{
						continue;
					}
					boundsArgument.X -= _pageWidth;
				}
				graphics.DrawLine(pen, boundsArgument.Right - 1, boundsArgument.Top, boundsArgument.Right - 1, boundsArgument.Bottom);
				graphics.DrawLine(pen, boundsArgument.Left, boundsArgument.Top, boundsArgument.Left, boundsArgument.Bottom);
				graphics.DrawLine(pen, boundsArgument.Left, boundsArgument.Bottom - 1, boundsArgument.Right, boundsArgument.Bottom - 1);
			}
		}

		private void DrawHeaders(Graphics graphics, int firstColumn, int integerArgument)
		{
			if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
			{
				Rectangle rectangle = new Rectangle(_marginBounds.Left, _marginBounds.Top, _marginBounds.Width, _rowHeight);
				DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.OwnerHeader, rectangle, null);
				_scheduler.RaiseDrawSchedulerArea(e);
				if (!e.Handled)
				{
					DrawHeaderCell(graphics, rectangle, _scheduler.CurrentOwner.Text);
				}
			}
			foreach (SchedulerAppointmentOwnerLayoutState item in _headerLayouts)
			{
				if (item.GetColumnIndex() <= integerArgument && item.GetColumnIndex() >= firstColumn)
				{
					Rectangle bounds = item._bounds;
					if (_pageNumber > 1)
					{
						bounds.X -= _pageWidth;
					}
					item._bounds = bounds;
					DrawSchedulerAreaEventArgs dateHeaderEventArgs = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.DateHeader, item._bounds, item);
					_scheduler.RaiseDrawSchedulerArea(dateHeaderEventArgs);
					if (!dateHeaderEventArgs.Handled)
					{
						DrawHeaderCell(graphics, item._bounds, item.GetHeaderText());
					}
				}
			}
		}

		private void DrawHeaderCell(Graphics graphics, Rectangle layoutRectangle, string text)
		{
			Color empty = Color.Empty;
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(PredefinedStyle.Header);
			schedulerFormatStyle.ApplyTheme();
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.Trimming = StringTrimming.EllipsisCharacter;
			if (!_grayShading)
			{
				SchedulerRenderer.DrawBorder(graphics, Pens.Black, layoutRectangle);
			}
			else
			{
				Brush brush = schedulerFormatStyle.GetBackBrush(layoutRectangle);
				empty = schedulerFormatStyle.BackColor;
				_scheduler.ScrollBarStyle.DrawBackground(graphics, layoutRectangle, false, false, _scheduler.HeaderAppearance, empty, brush);
			}
			if (_grayShading)
			{
				Brush headerTextBrush = schedulerFormatStyle.GetForeColorBrush(_scheduler.GetThemedForeColor());
				if (headerTextBrush != null)
				{
					graphics.DrawString(text, GetFormatStyleFont(schedulerFormatStyle), headerTextBrush, layoutRectangle, stringFormat);
				}
			}
			else
			{
				graphics.DrawString(text, GetFormatStyleFont(schedulerFormatStyle), _textBrush, layoutRectangle, stringFormat);
			}
			stringFormat.Dispose();
			stringFormat = null;
			schedulerFormatStyle.DisposeResources();
			_scheduler.ScrollBarStyle.DisposeAllResources();
		}

		private void DrawWeekViewDateHeader(Graphics graphics)
		{
			Rectangle rectangle = _marginBounds;
			rectangle.Width = _pageWidth;
			rectangle.Height = 120;
			StringFormat stringFormat = new StringFormat();
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			stringFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
			if (_grayShading)
			{
				Color color = SystemColors.Control;
				Brush brush = new SolidBrush(color);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
					brush.Dispose();
				}
			}
			graphics.DrawRectangle(Pens.Black, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
			if (_pageNumber == 1)
			{
				rectangle.Width = _marginBounds.Width - 250;
				string text = _scheduler.Dates[0].ToString("MMMM dd", Application.CurrentCulture);
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				string secondLineText;
				if (_scheduler.Dates.Count > 1)
				{
					text += " -";
					secondLineText = _scheduler.Dates[_scheduler.Dates.Count - 1].ToString("MMMM dd", Application.CurrentCulture);
				}
				else
				{
					secondLineText = currentCulture.DateTimeFormat.GetDayName(_scheduler.Dates[0].DayOfWeek);
				}
				Rectangle textBounds = rectangle;
				int firstLineHeight = (int)graphics.MeasureString(text, _dateHeadingsFont, 0, stringFormat).Height;
				graphics.DrawString(text, _dateHeadingsFont, _textBrush, textBounds, stringFormat);
				textBounds.Y += firstLineHeight;
				textBounds.Height -= firstLineHeight;
				graphics.DrawString(secondLineText, _dateHeadingsFont, _textBrush, textBounds, stringFormat);
			}
			_marginBounds.Height -= 122;
			_marginBounds.Y += 122;
		}

		private void DrawMonthViewDateHeader(Graphics graphics)
		{
			Rectangle rectangle = _marginBounds;
			rectangle.Width = _pageWidth;
			rectangle.Height = 120;
			StringFormat stringFormat = new StringFormat();
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			stringFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
			if (_grayShading)
			{
				Color color = SystemColors.Control;
				Brush brush = new SolidBrush(color);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
					brush.Dispose();
				}
			}
			graphics.DrawRectangle(Pens.Black, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
			rectangle.Width = _marginBounds.Width - 250;
			if (_pageNumber == 1)
			{
				string text = GetMonthHeaderText();
				_ = CultureInfo.CurrentCulture;
				Rectangle textBounds = rectangle;
				_ = graphics.MeasureString(text, _dateHeadingsFont, 0, stringFormat).Height;
				graphics.DrawString(text, _dateHeadingsFont, _textBrush, textBounds, stringFormat);
			}
			_marginBounds.Height -= 122;
			_marginBounds.Y += 122;
		}

		private void DrawAllDayAreaBackground(Graphics graphics)
		{
			Rectangle rectangle = new Rectangle(_marginBounds.Left, _marginBounds.Top, _marginBounds.Width, _timeGridTop - _marginBounds.Top + 1);
			DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.AllDayArea, rectangle, null);
			_scheduler.RaiseDrawSchedulerArea(e);
			if (e.Handled)
			{
				return;
			}
			if (_grayShading)
			{
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(PredefinedStyle.TimeNavigator);
				schedulerFormatStyle.ApplyTheme();
				Rectangle timeColumnBounds = rectangle;
				timeColumnBounds.Width = _dayColumnsLeft - rectangle.Left - 5;
				timeColumnBounds.Height = _marginBounds.Height;
				Rectangle rect = timeColumnBounds;
				rect.Height = rectangle.Height;
				Brush brush = schedulerFormatStyle.GetBackBrush(timeColumnBounds);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rect);
				}
				schedulerFormatStyle.DisposeResources();
				Rectangle allDayBounds = new Rectangle(_dayColumnsLeft - 5, _marginBounds.Top + _rowHeight, _marginBounds.Right - _dayColumnsLeft + 5, rectangle.Height - _rowHeight);
				if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
				{
					allDayBounds.Y += _rowHeight;
					allDayBounds.Height -= _rowHeight;
				}
				SchedulerFormatStyle allDayStyle = new SchedulerFormatStyle(PredefinedStyle.AllDay);
				allDayStyle.ApplyTheme();
				brush = allDayStyle.GetBackBrush(allDayBounds);
				if (brush != null)
				{
					graphics.FillRectangle(brush, allDayBounds);
				}
				allDayStyle.DisposeResources();
			}
			if (_grayShading)
			{
				Color color = SystemColors.ControlDark;
				Pen pen = new Pen(color);
				SchedulerRenderer.DrawBorder(graphics, pen, rectangle);
				pen.Dispose();
			}
			else
			{
				SchedulerRenderer.DrawBorder(graphics, Pens.Black, rectangle);
			}
		}

		private void DrawDayViewHeaders(Graphics graphics, int y1)
		{
			int count = _scheduler.Dates.Count;
			int columnWidth = (_marginBounds.Right - _dayColumnsLeft + 5) / count;
			int widthRemainder = _marginBounds.Right - (columnWidth * count + _dayColumnsLeft - 5) + 1;
			int columnLeft = _dayColumnsLeft - 5;
			int columnNumber = 1;
			_ = _scheduler._allDayAreaControl;
			foreach (SchedulerAppointmentOwnerLayoutState item in _scheduler._allDayAreaControl._dateHeaders)
			{
				if (columnNumber == count)
				{
					columnWidth += widthRemainder;
				}
				Rectangle rectangle = new Rectangle(columnLeft, y1, columnWidth, _rowHeight);
				DrawHeaderCell(graphics, rectangle, item.GetHeaderText());
				columnLeft += columnWidth;
				if (!_scheduler.IsMultiOwnerActive() && _scheduler.HeaderAppearance != Appearance.Flat && columnNumber != count)
				{
					graphics.DrawLine(Pens.Black, rectangle.Right, rectangle.Bottom + 1, rectangle.Right, _timeGridTop - 2);
				}
				columnNumber++;
			}
			if (_scheduler.IsMultiOwnerActive())
			{
				columnNumber = 1;
				count = _dayLayouts.Count;
				columnWidth = (_marginBounds.Right - _dayColumnsLeft + 5) / count;
				widthRemainder = _marginBounds.Right - (columnWidth * count + _dayColumnsLeft - 5) + 1;
				columnLeft = _dayColumnsLeft - 5;
				y1 += _rowHeight;
				foreach (SchedulerAppointmentOwnerLayoutState ownerHeader in _scheduler._allDayAreaControl._ownerHeaders)
				{
					if (columnNumber == count)
					{
						columnWidth += widthRemainder;
					}
					Rectangle ownerHeaderBounds = new Rectangle(columnLeft, y1, columnWidth, _rowHeight);
					DrawHeaderCell(graphics, ownerHeaderBounds, ownerHeader.GetHeaderText());
					columnLeft += columnWidth;
					if (_scheduler.HeaderAppearance != Appearance.Flat && columnNumber != count)
					{
						graphics.DrawLine(Pens.Black, ownerHeaderBounds.Right, ownerHeaderBounds.Bottom + 1, ownerHeaderBounds.Right, _timeGridTop - 2);
					}
					columnNumber++;
				}
			}
			if (!_grayShading)
			{
				graphics.DrawLine(Pens.Black, _dayColumnsLeft - 5, y1, _dayColumnsLeft - 5, _timeGridTop);
			}
		}

		private void DrawDayHeader(Graphics graphics, int y1, DateTime dateTime)
		{
			int width = _marginBounds.Right - _dayColumnsLeft + 5;
			int x = _dayColumnsLeft - 5;
			SchedulerAllDayArea field_ = _scheduler._allDayAreaControl;
			SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState = FindDateHeaderLayout(field_, dateTime.Date);
			Rectangle rectangle = new Rectangle(x, y1, width, _rowHeight);
			DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.DateHeader, rectangle, schedulerAppointmentOwnerLayoutState);
			_scheduler.RaiseDrawSchedulerArea(e);
			if (!e.Handled)
			{
				DrawHeaderCell(graphics, rectangle, schedulerAppointmentOwnerLayoutState.GetDate().Date.ToLongDateString());
			}
			_ = schedulerAppointmentOwnerLayoutState.GetDate().Date;
			if (_scheduler.IsMultiOwnerActive() && _scheduler.ShowOwnersHeader)
			{
				width = _marginBounds.Right - _dayColumnsLeft + 5;
				x = _dayColumnsLeft - 5;
				y1 += _rowHeight;
				schedulerAppointmentOwnerLayoutState = FindOwnerHeaderLayout(field_, dateTime.Date);
				rectangle = new Rectangle(x, y1, width, _rowHeight);
				e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.OwnerHeader, rectangle, schedulerAppointmentOwnerLayoutState);
				_scheduler.RaiseDrawSchedulerArea(e);
				if (!e.Handled)
				{
					DrawHeaderCell(graphics, rectangle, schedulerAppointmentOwnerLayoutState.GetHeaderText());
				}
			}
			if (!_grayShading)
			{
				graphics.DrawLine(Pens.Black, _dayColumnsLeft - 5, y1, _dayColumnsLeft - 5, _timeGridTop);
			}
		}

		private void DrawDayViewDateHeader(Graphics graphics, DateTime dateTime)
		{
			Rectangle rectangle = _marginBounds;
			rectangle.Height = 120;
			StringFormat stringFormat = new StringFormat();
			stringFormat.Trimming = StringTrimming.EllipsisCharacter;
			stringFormat.FormatFlags |= StringFormatFlags.FitBlackBox;
			if (_grayShading)
			{
				Color color = SystemColors.Control;
				Brush brush = new SolidBrush(color);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
					brush.Dispose();
				}
			}
			graphics.DrawRectangle(Pens.Black, rectangle.Left, rectangle.Top, rectangle.Width - 1, rectangle.Height - 1);
			rectangle.Width = _marginBounds.Width - 150;
			string text = dateTime.ToLongDateString();
			_ = CultureInfo.CurrentCulture;
			Rectangle textBounds = rectangle;
			int lineHeight = (int)graphics.MeasureString(text, _dateHeadingsFont, 0, stringFormat).Height;
			graphics.DrawString(text, _dateHeadingsFont, _textBrush, textBounds, stringFormat);
			textBounds.Y += lineHeight;
			textBounds.Height -= lineHeight;
			_marginBounds.Height -= 122;
			_marginBounds.Y += 122;
		}

		private void DrawTimeColumn(Graphics graphics)
		{
			StringFormat stringFormat = new StringFormat();
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap;
			_currentRowTop = _timeGridTop;
			int firstHour = StartTime.Hours;
			UpdateTimeLabelFont();
			_minutesPerSlot = 60 / (int)_interval;
			if (StartTime.Minutes != 0)
			{
				int skippedRowCount = StartTime.Minutes / (60 / (int)_interval);
				_currentRowTop -= skippedRowCount * _rowHeight;
				firstHour++;
			}
			Pen pen = null;
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(PredefinedStyle.TimeNavigator);
			schedulerFormatStyle.ApplyTheme();
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			string text = currentCulture.DateTimeFormat.AMDesignator;
			if (text == "")
			{
				text = "00";
			}
			pen = ((!_grayShading || schedulerFormatStyle.BackColor.IsEmpty) ? new Pen(Color.Black) : new Pen(ControlPaint.Dark(schedulerFormatStyle.BackColor, 0f)));
			int amWidth = (int)graphics.MeasureString(text, PrintDayFont, 0).Width + 2;
			int timeColumnWidth = ((_interval != Interval.SixtyMinutes) ? ((int)graphics.MeasureString("12", _timeLabelFont, 0, stringFormat).Width + amWidth) : ((int)graphics.MeasureString("12:", _timeLabelFont, 0, stringFormat).Width + amWidth));
			Rectangle rectangle = new Rectangle(_marginBounds.Left, _timeGridTop - 1, timeColumnWidth + 5, _timeGridBottom - _timeGridTop + 1);
			int hourRowHeight = _rowHeight;
			if (_interval != Interval.SixtyMinutes)
			{
				hourRowHeight *= 2;
			}
			Font font = schedulerFormatStyle.ResolveFont(PrintDayFont);
			_dayColumnsLeft = _marginBounds.Left + timeColumnWidth + 10;
			int hourTextRight = (int)graphics.MeasureString("12", _timeLabelFont, rectangle.Width, stringFormat).Width + _marginBounds.Left;
			string hourFormat = "%h";
			string amText;
			string pmText;
			if (_scheduler.TimeFormat == TimeFormat.TwentyFourHours)
			{
				hourFormat = "%H";
				amText = "00";
				pmText = "00";
			}
			else
			{
				amText = text;
				pmText = currentCulture.DateTimeFormat.PMDesignator;
			}
			if (amText == string.Empty)
			{
				amText = "00";
			}
			if (pmText == string.Empty)
			{
				pmText = "00";
			}
			amText = amText.ToLower(currentCulture);
			pmText = pmText.ToLower(currentCulture);
			DateTime dateTime = _scheduler.Dates[0].Date.Add(StartTime);
			DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, _scheduler, OwnerDrawnArea.TimeNavigator, rectangle, null);
			_scheduler.RaiseDrawSchedulerArea(e);
			if (e.Handled)
			{
				return;
			}
			if (_grayShading)
			{
				Brush brush = schedulerFormatStyle.GetBackBrush(rectangle);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
				}
			}
			Region region = new Region(rectangle);
			Region clip = graphics.Clip;
			graphics.Clip = region;
			if (_grayShading)
			{
				Color color = SystemColors.ControlDark;
				Pen borderPen = new Pen(color);
				graphics.DrawRectangle(borderPen, rectangle);
				borderPen.Dispose();
			}
			else
			{
				graphics.DrawRectangle(Pens.Black, rectangle);
			}
			do
			{
				string suffixText = "00";
				if (dateTime.TimeOfDay.Hours == 0)
				{
					suffixText = amText;
				}
				else if (dateTime.TimeOfDay.Hours == 12)
				{
					suffixText = pmText;
				}
				else if (dateTime.TimeOfDay.Hours == firstHour)
				{
					suffixText = ((dateTime.TimeOfDay.Hours >= 12) ? pmText : amText);
				}
				string hourText = dateTime.ToString(hourFormat, currentCulture);
				if (_interval == Interval.SixtyMinutes)
				{
					hourText = ((!(suffixText == "00")) ? (hourText + " ") : (hourText + ":"));
				}
				Brush textBrush = schedulerFormatStyle.GetForeColorBrush(_scheduler.GetThemedForeColor());
				stringFormat.Alignment = StringAlignment.Far;
				Rectangle hourBounds = new Rectangle(_marginBounds.Left, _currentRowTop, timeColumnWidth - amWidth + 1, hourRowHeight);
				if (_grayShading)
				{
					if (textBrush != null)
					{
						graphics.DrawString(hourText, _timeLabelFont, textBrush, hourBounds, stringFormat);
					}
				}
				else
				{
					graphics.DrawString(hourText, _timeLabelFont, _textBrush, hourBounds, stringFormat);
				}
				Point pt = new Point(_marginBounds.Left + 5, _currentRowTop);
				Point lineEnd = new Point(_marginBounds.Left + timeColumnWidth, _currentRowTop);
				graphics.DrawLine(pen, pt, lineEnd);
				Rectangle suffixBounds = new Rectangle(hourTextRight + 1, _currentRowTop, _marginBounds.Left + timeColumnWidth - hourTextRight, _rowHeight);
				stringFormat.Alignment = StringAlignment.Center;
				if (_grayShading)
				{
					if (textBrush != null)
					{
						graphics.DrawString(suffixText, font, textBrush, suffixBounds, stringFormat);
					}
				}
				else
				{
					graphics.DrawString(suffixText, font, _textBrush, suffixBounds, stringFormat);
				}
				int minuteValue = 0;
				for (int i = 1; i < (int)_interval; i++)
				{
					if (_scheduler.ShowMinutesInTimeNavigator)
					{
						if (minuteValue > 0)
						{
							suffixBounds = new Rectangle(hourTextRight + 1, _currentRowTop, timeColumnWidth - hourTextRight + _marginBounds.Left + 1, _rowHeight);
							if (textBrush != null)
							{
								graphics.DrawString(minuteValue.ToString(), font, textBrush, suffixBounds, stringFormat);
							}
						}
						minuteValue += _minutesPerSlot;
					}
					_currentRowTop += _rowHeight;
					pt = new Point(hourTextRight, _currentRowTop);
					lineEnd = new Point(_marginBounds.Left + timeColumnWidth, _currentRowTop);
					graphics.DrawLine(pen, pt, lineEnd);
				}
				if (_scheduler.ShowMinutesInTimeNavigator)
				{
					if (minuteValue > 0)
					{
						suffixBounds = new Rectangle(hourTextRight + 1, _currentRowTop, timeColumnWidth - hourTextRight + _marginBounds.Left + 1, _rowHeight);
						if (textBrush != null)
						{
							graphics.DrawString(minuteValue.ToString(), font, textBrush, suffixBounds, stringFormat);
						}
					}
					minuteValue += _minutesPerSlot;
				}
				_currentRowTop += _rowHeight;
				dateTime = dateTime.AddHours(1.0);
			}
			while (_currentRowTop <= rectangle.Bottom);
			graphics.Clip = clip;
			region.Dispose();
			region = null;
			stringFormat.Dispose();
			stringFormat = null;
			schedulerFormatStyle.DisposeResources();
			if (pen != null)
			{
				pen.Dispose();
				pen = null;
			}
		}

		internal Rectangle DrawTimeClocks(Graphics graphics, SchedulerAppointment schedulerAppointment, Rectangle rectangleArgument)
		{
			Rectangle rectangle = Rectangle.Empty;
			if (rectangleArgument.Width - 36 >= 50)
			{
				rectangle = new Rectangle(rectangleArgument.Left + 3, rectangleArgument.Top, 33, rectangleArgument.Height);
				DrawImage(graphics, false, schedulerAppointment, rectangle);
				DrawImage(graphics, true, schedulerAppointment, rectangle);
			}
			else if (rectangleArgument.Width - 18 >= 50)
			{
				rectangle = new Rectangle(rectangleArgument.Left + 3, rectangleArgument.Top, 18, rectangleArgument.Height);
				DrawImage(graphics, false, schedulerAppointment, rectangle);
			}
			return rectangle;
		}

		private void DrawTimeSlotBackground(Graphics graphics, SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState, SchedulerFormatStyle schedulerFormatStyleArgument, SchedulerFormatStyle alternatingHourStyle, SchedulerFormatStyle workingHourStyle, SchedulerFormatStyle alternatingWorkingHourStyle, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState)
		{
			SchedulerFormatStyle schedulerFormatStyle = _scheduler.GetTimeSlotStyle(schedulerAppointmentOwnerLayoutState, schedulerFormatStyleArgument, alternatingHourStyle, workingHourStyle, alternatingWorkingHourStyle);
			Brush brush = schedulerFormatStyle.GetBackBrush(schedulerAppointmentOwnerLayoutState._bounds);
			if (brush != null)
			{
				graphics.FillRectangle(brush, schedulerAppointmentOwnerLayoutState._bounds);
			}
			schedulerFormatStyle.DisposeResources();
		}
	}
}
