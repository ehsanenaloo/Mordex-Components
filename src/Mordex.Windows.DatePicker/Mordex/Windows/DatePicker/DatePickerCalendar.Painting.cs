using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	public partial class DatePickerCalendar
	{
		// Gap between the cell text and the cell border when text is drawn with GDI+.
		private const int CellTextInset = 2;

		// Size of one arrow bar in the month scroll arrows.
		private const int ArrowMargin = 6;

		private const int ArrowHeight = 4;

		private const int OpaqueAlpha = 255;

		// Brushes, fonts and pens that are shared by all cells of one month page. They live for one paint pass.
		private sealed class CellPaintCache : IDisposable
		{
			private readonly System.Collections.Generic.Dictionary<Font, Font> _boldFonts = new System.Collections.Generic.Dictionary<Font, Font>();

			internal Font GetBoldFont(Font font)
			{
				if (!_boldFonts.TryGetValue(font, out Font boldFont))
				{
					boldFont = new Font(font, font.Style | FontStyle.Bold);
					_boldFonts.Add(font, boldFont);
				}
				return boldFont;
			}

			public void Dispose()
			{
				foreach (Font boldFont in _boldFonts.Values)
				{
					boldFont.Dispose();
				}
				_boldFonts.Clear();
			}
		}

		private ThemeRenderer GetThemeRenderer()
		{
			if (_themeRenderer == null)
			{
				_themeRenderer = new ThemeRenderer();
			}
			return _themeRenderer;
		}

		internal void ReleaseThemeRenderer()
		{
			_themeRenderer = null;
		}

		internal void DrawBorder(Graphics graphics)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (_borderStyle != BorderStyle.None)
			{
				DatePickerHelper.DrawRectangleOutline(graphics, GdiCache.Pen(GetThemeRenderer()._borderColor), clientRectangle);
			}
			else
			{
				_borderHelperStyle.ControlColor = GetEffectiveBackColor();
				DatePickerHelper.DrawControlBorder(graphics, _borderHelperStyle, clientRectangle, GetEffectiveBorderStyle());
			}
		}

		private void DrawButtonText(Graphics graphics, string text, Rectangle rectangle)
		{
			if (text.Length > 0)
			{
				using (TextLayoutRenderer textRenderer = new TextLayoutRenderer(!UsesCompatibleTextRendering()))
				{
					textRenderer.Alignment = StringAlignment.Center;
					textRenderer.RightToLeft = RightToLeft;
					textRenderer.DrawText(graphics, text, rectangle, HeaderFont, GetControlStyle().ControlTextColor);
				}
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			EnsureCulture();
			UpdateMonthLayouts(false);
			CalendarBackgroundStyle backgroundStyle = CalendarBackgroundStyle.CreateResolvedCopy(_calendarBackgroundStyle);
			CalendarFormatStyle headerStyle = CalendarFormatStyle.CreateResolvedCopy(_headerFormatStyle);
			try
			{
				graphics.FillRectangle(GdiCache.Brush(GetEffectiveBackColor()), base.ClientRectangle);
				graphics.FillRectangle(backgroundStyle.GetBackgroundBrush(base.ClientRectangle, OpaqueAlpha), e.ClipRectangle);
				if (_columns != 0 && _rows != 0)
				{
					DrawMonths(graphics, backgroundStyle, headerStyle);
					DrawActionButton(graphics, _todayButton, GetTodayButtonText());
					DrawActionButton(graphics, _noneButton, GetNoneButtonText());
					DrawBorder(graphics);
				}
			}
			finally
			{
				backgroundStyle.DisposeBrush();
				headerStyle.DisposeResources();
			}
			base.OnPaint(e);
		}

		private void DrawMonths(Graphics graphics, CalendarBackgroundStyle backgroundStyle, CalendarFormatStyle headerStyle)
		{
			for (int row = 0; row < _rows; row++)
			{
				for (int column = 0; column < _columns; column++)
				{
					MonthLayout monthLayout = _monthLayouts[row, column];
					Rectangle monthBodyBounds = new Rectangle(monthLayout.GetLeftMarginBounds().Left, monthLayout.GetHeaderBounds().Bottom, monthLayout.GetMonthSize().Width, monthLayout.GetMonthSize().Height);
					graphics.FillRectangle(GetMonthBackgroundBrush(monthBodyBounds, backgroundStyle), monthBodyBounds);
					DrawMonthHeaderBackground(graphics, monthLayout, column);
					if (row == 0)
					{
						DrawScrollArrows(graphics, headerStyle);
					}
					DrawMonthTitle(graphics, monthLayout, monthLayout.GetHeaderBounds(), headerStyle);
					DrawDays(graphics, monthLayout);
				}
			}
		}

		private void DrawMonthHeaderBackground(Graphics graphics, MonthLayout monthLayout, int column)
		{
			Rectangle fillBounds = monthLayout.GetHeaderBounds();
			if (_columns > 1)
			{
				fillBounds.Width--;
				if (column > 0)
				{
					fillBounds.X++;
				}
			}
			ThemeRenderer renderer = GetThemeRenderer();
			if (renderer._titleGradient && fillBounds.Width > 0 && fillBounds.Height > 0)
			{
				using (System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(fillBounds, renderer._titleLightColor, renderer._titleDarkColor, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
				{
					graphics.FillRectangle(brush, fillBounds);
				}
				return;
			}
			graphics.FillRectangle(GdiCache.Brush(renderer._titleBackColor), fillBounds);
		}

		private void DrawActionButton(Graphics graphics, PickerButton button, string text)
		{
			if (button._bounds.IsEmpty)
			{
				return;
			}
			GetThemeRenderer().DrawButtonBackground(graphics, button._bounds, button.GetIsHot(), button.GetIsPressed(), base.Enabled, true, false);
			DrawButtonText(graphics, text, button._bounds);
		}

		private Brush GetMonthBackgroundBrush(Rectangle bounds, CalendarBackgroundStyle backgroundStyle)
		{
			bounds.Inflate(1, 0);
			return backgroundStyle.GetBackgroundBrush(bounds, OpaqueAlpha);
		}

		private Font GetWeekNumberFont()
		{
			if (_weekNumberFont == null)
			{
				_weekNumberFont = new Font(Font.FontFamily, Math.Max(1f, Font.SizeInPoints - 2f));
			}
			return _weekNumberFont;
		}

		private CalendarFormatStyle GetCellStyle(CalendarCell cell, CalendarFormatStyle daysStyle, CalendarFormatStyle selectedStyle, CalendarFormatStyle trailingStyle, bool isSelected)
		{
			if (isSelected)
			{
				return selectedStyle;
			}
			return cell.GetIsAdjacentMonth() ? trailingStyle : daysStyle;
		}

		private Brush GetCellTextBrush(CalendarCell cell, CalendarFormatStyle cellStyle)
		{
			if (!cell.GetIsSelectable())
			{
				return MxThemeManager.ResolveDefault(DeviceDpi).Brush(ThemeRole.TextDisabled);
			}
			return cellStyle.GetForeBrush(GetEffectiveForeColor());
		}

		private void DrawDays(Graphics graphics, MonthLayout monthLayout)
		{
			DayOfWeek firstDayOfWeek = ToDayOfWeek(FirstDayOfWeek);
			bool usesTextRenderer = !UsesCompatibleTextRendering();
			CalendarFormatStyle daysStyle = CalendarFormatStyle.CreateResolvedCopy(_daysFormatStyle);
			CalendarFormatStyle selectedStyle = daysStyle.MergeWith(_selectedFormatStyle);
			CalendarFormatStyle trailingStyle = daysStyle.MergeWith(_trailingFormatStyle);
			CalendarFormatStyle dayHeadersStyle = daysStyle.MergeWith(_dayHeadersFormatStyle);
			Pen gridLinePen = GdiCache.Pen(GetThemeRenderer()._gridLineColor);
			using (TextLayoutRenderer textRenderer = new TextLayoutRenderer(usesTextRenderer))
			using (CellPaintCache paintCache = new CellPaintCache())
			{
				try
				{
					textRenderer.LineAlignment = StringAlignment.Center;
					textRenderer.NoPadding = true;
					textRenderer.Batch = usesTextRenderer ? new TextBatch() : null;
					Color dayHeaderColor = dayHeadersStyle.ForeColor;
					if (dayHeaderColor.IsEmpty)
					{
						dayHeaderColor = GetEffectiveForeColor();
					}
					DrawWeekdayHeaders(graphics, monthLayout, textRenderer, gridLinePen, dayHeadersStyle, dayHeaderColor);
					textRenderer.Alignment = StringAlignment.Far;
					int weekLineBottom = 0;
					for (int row = 0; row < MonthLayout.GridRows; row++)
					{
						for (int column = 0; column < MonthLayout.GridColumns; column++)
						{
							CalendarCell cell = monthLayout.GetDayCell(row, column);
							DrawDayCell(graphics, cell, textRenderer, paintCache, daysStyle, selectedStyle, trailingStyle);
							if (row == MonthLayout.GridRows - 1 && (_showNoneButton || _showTodayButton))
							{
								Rectangle cellBounds = cell.GetBounds();
								graphics.DrawLine(gridLinePen, cellBounds.Left, cellBounds.Bottom, cellBounds.Right, cellBounds.Bottom);
							}
						}
						if (_weekNumbers)
						{
							weekLineBottom = DrawWeekNumber(graphics, monthLayout, row, textRenderer, dayHeaderColor, firstDayOfWeek, usesTextRenderer);
						}
					}
					if (_weekNumbers)
					{
						int weekLineTop = monthLayout.GetWeekdayCell(0).GetBounds().Bottom;
						int separatorX = monthLayout.GetLeftMarginBounds().Right;
						graphics.DrawLine(gridLinePen, separatorX, weekLineTop, separatorX, weekLineBottom);
					}
				}
				finally
				{
					textRenderer.Batch?.Flush(graphics);
					daysStyle.DisposeResources();
					selectedStyle.DisposeResources();
					trailingStyle.DisposeResources();
					dayHeadersStyle.DisposeResources();
				}
			}
		}

		private void DrawWeekdayHeaders(Graphics graphics, MonthLayout monthLayout, TextLayoutRenderer textRenderer, Pen gridLinePen, CalendarFormatStyle dayHeadersStyle, Color dayHeaderColor)
		{
			Font dayHeaderFont = dayHeadersStyle.GetEffectiveFont(base.Font);
			textRenderer.Alignment = StringAlignment.Center;
			for (int column = 0; column < MonthLayout.GridColumns; column++)
			{
				Rectangle headerCellBounds = monthLayout.GetWeekdayCell(column).GetBounds();
				headerCellBounds.Height--;
				graphics.DrawLine(gridLinePen, headerCellBounds.Left, headerCellBounds.Bottom, headerCellBounds.Right, headerCellBounds.Bottom);
				graphics.FillRectangle(dayHeadersStyle.GetBackBrush(headerCellBounds), headerCellBounds);
				headerCellBounds.X += 2;
				textRenderer.DrawText(graphics, _dayHeaderTexts[column], headerCellBounds, dayHeaderFont, dayHeaderColor);
			}
		}

		private void DrawDayCell(Graphics graphics, CalendarCell cell, TextLayoutRenderer textRenderer, CellPaintCache paintCache, CalendarFormatStyle daysStyle, CalendarFormatStyle selectedStyle, CalendarFormatStyle trailingStyle)
		{
			bool isSelected = cell.GetDate() == _selectedDate && !_isNullDateSelected && cell.GetIsVisible();
			CalendarFormatStyle cellStyle = GetCellStyle(cell, daysStyle, selectedStyle, trailingStyle, isSelected);
			Brush backgroundBrush = cellStyle.GetBackBrush(cell.GetBounds());
			Brush textBrush = GetCellTextBrush(cell, cellStyle);
			Rectangle cellBounds = cell.GetBounds();
			graphics.FillRectangle(backgroundBrush, cellBounds);
			if (!cell.GetIsVisible())
			{
				return;
			}
			Font cellFont = cellStyle.GetEffectiveFont(base.Font);
			if (cell.GetIsBold())
			{
				cellFont = paintCache.GetBoldFont(cellFont);
			}
			Rectangle textBounds = cellBounds;
			if (!UsesCompatibleTextRendering())
			{
				textBounds.Width -= CellTextInset;
			}
			textRenderer.DrawText(graphics, cell.GetDayText(), textBounds, cellFont, textBrush);
			if (_todayRect && cell.GetDate() == DateTime.Today)
			{
				graphics.DrawRectangle(GdiCache.Pen(GetThemeRenderer()._todayColor), cellBounds.Left, cellBounds.Top, cellBounds.Width - 1, cellBounds.Height - 1);
			}
		}

		// Draws the week number of one row and returns where the vertical separator line must end.
		private int DrawWeekNumber(Graphics graphics, MonthLayout monthLayout, int row, TextLayoutRenderer textRenderer, Color color, DayOfWeek firstDayOfWeek, bool usesTextRenderer)
		{
			CalendarCell cell = monthLayout.GetDayCell(row, MonthLayout.GridColumns - 1);
			DateTime weekDate = cell.GetDate();
			if (!cell.GetIsVisible())
			{
				cell = monthLayout.GetDayCell(row, 0);
			}
			if (!cell.GetIsVisible())
			{
				return cell.GetBounds().Top;
			}
			int weekOfYear = DatePickerHelper.GetWeekOfYear(weekDate, _weekRule, firstDayOfWeek);
			Rectangle weekNumberBounds = cell.GetBounds();
			weekNumberBounds.X = monthLayout.GetLeftMarginBounds().X;
			if (usesTextRenderer)
			{
				weekNumberBounds.Width -= CellTextInset;
			}
			textRenderer.DrawText(graphics, weekOfYear.ToString(CultureInfo.CurrentCulture), weekNumberBounds, GetWeekNumberFont(), color);
			return cell.GetBounds().Bottom - 3;
		}

		private void DrawScrollArrows(Graphics graphics, CalendarFormatStyle headerStyle)
		{
			DrawScrollArrow(graphics, -1, _scrollArrowBounds[0]);
			DrawScrollArrow(graphics, 1, _scrollArrowBounds[1]);
		}

		private void DrawScrollArrow(Graphics graphics, int direction, Rectangle rectangle)
		{
			Pen pen = GdiCache.Pen(GetThemeRenderer()._arrowColor);
			Point tip = direction != -1 ? new Point(rectangle.Right - ArrowMargin, rectangle.Top + rectangle.Height / 2) : new Point(rectangle.Left + ArrowMargin, rectangle.Top + rectangle.Height / 2);
			Point lowerEnd = tip;
			graphics.DrawLine(pen, tip.X, tip.Y, tip.X - direction, tip.Y);
			for (int step = 0; step < ArrowHeight; step++)
			{
				tip.Offset(-direction, -1);
				lowerEnd.Offset(-direction, 1);
				graphics.DrawLine(pen, tip, lowerEnd);
			}
		}

		private void DrawMonthTitle(Graphics graphics, MonthLayout monthLayout, Rectangle bounds, CalendarFormatStyle headerStyle)
		{
			using (TextLayoutRenderer textRenderer = new TextLayoutRenderer(!UsesCompatibleTextRendering()))
			{
				textRenderer.Alignment = StringAlignment.Center;
				textRenderer.RightToLeft = RightToLeft;
				textRenderer.DrawText(graphics, monthLayout.GetHeaderText(), bounds, headerStyle.GetEffectiveFont(HeaderFont), headerStyle.GetForeBrush(GetEffectiveForeColor()));
			}
		}
	}
}
