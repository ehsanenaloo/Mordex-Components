using System;
using System.Drawing;
using Mordex.Windows.Common;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal partial class ControlStyleControl
	{
		protected override void OnPaint(PaintEventArgs pe)
		{
			DrawCalendar(pe.Graphics);
			DrawBorder(pe.Graphics);
		}

		private void DrawBorder(Graphics graphics)
		{
			MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _mordexColorSchemeAppearanceState;
			Rectangle clientRectangle = base.ClientRectangle;
			if (mordexColorSchemeAppearanceState != null && mordexColorSchemeAppearanceState.IsThemedLook)
			{
				Pen pen = GdiCache.Pen(mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.ControlBorder));
				clientRectangle = GridHelper.DrawFlatBorder(graphics, pen, clientRectangle);
			}
			else if (mordexColorSchemeAppearanceState != null && mordexColorSchemeAppearanceState._isThemed)
			{
				Pen otherPen = GdiCache.Pen(mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.ControlBorder));
				GridHelper.DrawFlatBorder(graphics, otherPen, clientRectangle);
			}
			else
			{
				GridHelper.DrawBorderForStyle(graphics, _calendarStyle, clientRectangle, BorderStyle.Flat);
			}
		}

		private void DrawCalendar(Graphics graphics)
		{
			RebuildMonthBlocks(false);
			Rectangle clientRectangle = base.ClientRectangle;
			graphics.FillRectangle(_calendarStyle.GetWindowBrush(), clientRectangle);
			if (_monthColumns > 0 && _monthRows > 0)
			{
				CalendarLayoutState calendarLayoutState = _monthBlocks[0, 0];
				DrawBackground(graphics, calendarLayoutState);
				DrawWeekdaysAndDays(graphics, calendarLayoutState);
				int value = calendarLayoutState.GetDayCell(5, 0).GetBounds().Bottom + 2;
				int x = calendarLayoutState.GetDayCell(5, 0).GetBounds().X;
				int right = calendarLayoutState.GetDayCell(5, 6).GetBounds().Right;
				if (_mordexColorSchemeAppearanceState._isThemed)
				{
					Pen pen = GdiCache.Pen(_mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.Gridline));
					graphics.DrawLine(pen, x, value, right, value);
				}
				else
				{
					graphics.DrawLine(_calendarStyle.GetWindowTextPen(), x, value, right, value);
				}
			}
			ControlStyleRenderer controlStyleRenderer = new ControlStyleRenderer(_mordexColorSchemeAppearanceState, _calendarStyle);
			if (_gridLayoutState._isVisible)
			{
				controlStyleRenderer.RenderContent(graphics, _gridLayoutState._bounds, Font, _todayButtonText, _gridLayoutState._isPressed, _gridLayoutState._isHot, RightToLeft == RightToLeft.Yes);
			}
			if (_controlStyleControlGridLayoutState._isVisible)
			{
				controlStyleRenderer.RenderContent(graphics, _controlStyleControlGridLayoutState._bounds, Font, _noneButtonText, _controlStyleControlGridLayoutState._isPressed, _controlStyleControlGridLayoutState._isHot, RightToLeft == RightToLeft.Yes);
			}
		}

		private void DrawBackground(Graphics graphics, CalendarLayoutState calendarLayoutState)
		{
			Rectangle otherRectangle = calendarLayoutState.GetTitleLayout()._bounds;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(_mordexColorSchemeAppearanceState.GetUseTextRenderer());
			textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			if (_mordexColorSchemeAppearanceState.IsThemedLook)
			{
				Brush brush = GdiCache.Brush(_mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderGradient));
				graphics.FillRectangle(brush, otherRectangle);
				textLayoutRenderer.DrawTextWithColor(graphics, calendarLayoutState.GetMonthTitle(), otherRectangle, Font, _mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderText));
			}
			else if (_mordexColorSchemeAppearanceState._isThemed)
			{
				RenderContent(graphics, calendarLayoutState, calendarLayoutState.GetTitleRectangle());
			}
			else
			{
				_calendarStyle.DrawBackground(graphics, otherRectangle, calendarLayoutState.GetTitleLayout()._isHot, calendarLayoutState.GetTitleLayout()._isPressed);
				textLayoutRenderer.DrawTextWithBrush(graphics, calendarLayoutState.GetMonthTitle(), otherRectangle, Font, _calendarStyle.GetControlTextBrush());
			}
			Rectangle rectangle = new Rectangle(otherRectangle.Left, otherRectangle.Top, otherRectangle.Height, otherRectangle.Height);
			_calendarStyle.DrawHorizontalArrow(graphics, rectangle, -1, calendarLayoutState.GetTitleLayout()._isHot, calendarLayoutState.GetTitleLayout()._isPressed, calendarLayoutState.GetTitleLayout()._isEnabled);
			rectangle = new Rectangle(otherRectangle.Right - otherRectangle.Height, otherRectangle.Top, otherRectangle.Height, otherRectangle.Height);
			_calendarStyle.DrawHorizontalArrow(graphics, rectangle, 1, calendarLayoutState.GetTitleLayout()._isHot, calendarLayoutState.GetTitleLayout()._isPressed, calendarLayoutState.GetTitleLayout()._isEnabled);
			textLayoutRenderer.ReleaseStringFormat();
		}

		private void DrawWeekdaysAndDays(Graphics graphics, CalendarLayoutState calendarLayoutState)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(_mordexColorSchemeAppearanceState.GetUseTextRenderer());
			textLayoutRenderer.SetStringAlignment(StringAlignment.Far);
			Pen pen = ((_mordexColorSchemeAppearanceState.IsThemedLook || _mordexColorSchemeAppearanceState._isThemed) ? GdiCache.Pen(_mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.Gridline)) : _calendarStyle.GetControlDarkPen());
			for (int i = 0; i < 7; i++)
			{
				CalendarDayCell weekdayCell = calendarLayoutState.GetWeekdayCell(i);
				Rectangle rectangle = weekdayCell.GetBounds();
				rectangle.Height--;
				graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
				textLayoutRenderer.DrawTextWithBrush(graphics, _weekdayNames[i], rectangle, base.Font, _calendarStyle.GetWindowTextBrush());
			}
			Brush brush;
			Brush solidBrush;
			if (_mordexColorSchemeAppearanceState.IsThemedLook)
			{
				brush = GdiCache.Brush(_mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.HighlightColor));
				solidBrush = GdiCache.Brush(_mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.HighlightTextColor));
			}
			else
			{
				brush = GdiCache.Brush(GetSelectedStyle().GetEffectiveBackColor());
				solidBrush = ((!GetSelectedStyle().ForeColor.IsEmpty) ? GdiCache.Brush(GetSelectedStyle().ForeColor) : GdiCache.Brush(_calendarStyle.WindowTextColor));
			}
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					CalendarDayCell calendarLayoutState2Argument = calendarLayoutState.GetDayCell(i, j);
					Rectangle rectangle = calendarLayoutState2Argument.GetBounds();
					bool isSelected = ((_selectionStyleEnum != SelectionStyleEnum.Single) ? calendarLayoutState2Argument.GetIsSelected() : ((calendarLayoutState2Argument.GetDate() == _controlStyleControlDateTime) ? true : false));
					Brush windowTextBrush;
					if (!isSelected || !calendarLayoutState2Argument.GetIsVisible())
					{
						windowTextBrush = ((!calendarLayoutState2Argument.GetIsAdjacentMonth()) ? _calendarStyle.GetWindowTextBrush() : _calendarStyle.GetControlDarkBrush());
					}
					else
					{
						graphics.FillRectangle(brush, rectangle);
						windowTextBrush = solidBrush;
					}
					if (windowTextBrush == null)
					{
						windowTextBrush = _calendarStyle.GetWindowTextBrush();
					}
					if (calendarLayoutState2Argument.GetIsVisible())
					{
						Font font = ((!calendarLayoutState2Argument.GetIsBold()) ? base.Font : _controlStyleControlFont);
						textLayoutRenderer.DrawTextWithBrush(graphics, calendarLayoutState2Argument.GetDayText(), rectangle, font, windowTextBrush);
						if (_showTodayBorder && calendarLayoutState2Argument.GetDate() == DateTime.Today)
						{
							Pen otherPen = GdiCache.Pen(_todayBorderColor);
							graphics.DrawRectangle(otherPen, rectangle);
							otherPen = null;
						}
					}
				}
			}
			textLayoutRenderer.ReleaseStringFormat();
			textLayoutRenderer = null;
		}

		private void RenderContent(Graphics graphics, CalendarLayoutState calendarLayoutState, Rectangle bounds)
		{
			GridColumn gridColumn = new GridColumn();
			gridColumn.Caption = calendarLayoutState.GetMonthTitle();
			GridGroupLayoutState gridGroupLayoutState = new GridGroupLayoutState(LayoutItemType.ColumnHeader, null, gridColumn, bounds);
			gridGroupLayoutState._isHot = calendarLayoutState.GetTitleLayout()._isHot;
			gridGroupLayoutState._isPressed = calendarLayoutState.GetTitleLayout()._isPressed;
			GridFormatStyle gridFormatStyle = new GridFormatStyle();
			gridFormatStyle.TextAlignment = TextAlignment.Center;
			gridFormatStyle.ForeColor = _mordexColorSchemeAppearanceState.GetResolvedAppearance().GetGridColor(GridColorKey.HeaderText);
			CellRendererBase renderer = _calendarStyle.GetLayoutState().GetRenderer(gridGroupLayoutState);
			renderer.PaintBackground(graphics, gridGroupLayoutState, gridFormatStyle, bounds, selected: false, focused: false);
			renderer.PaintAddornments(graphics, gridGroupLayoutState, gridFormatStyle, bounds);
			renderer.PaintForeground(graphics, gridGroupLayoutState, gridFormatStyle, bounds, selected: false, focused: false);
		}

		private void UpdateTodayButtonHover(bool condition)
		{
			if (condition != _gridLayoutState._isPressed)
			{
				_gridLayoutState._isPressed = condition;
				Invalidate(_gridLayoutState._bounds);
			}
		}

		private void FillMonthBlocks()
		{
			DateTime dateTime = _dateTime;
			for (int i = 0; i < _monthRows; i++)
			{
				for (int j = 0; j < _monthColumns; j++)
				{
					CalendarLayoutState calendarLayoutState = _monthBlocks[i, j];
					calendarLayoutState.SetMonth(dateTime.Month);
					calendarLayoutState.SetYear(dateTime.Year);
					bool condition = i == 0 && j == 0;
					bool otherCondition = (i == _monthRows - 1) & (j == _monthColumns - 1);
					calendarLayoutState.FillDays(condition, otherCondition, _daysOfWeekEnum);
					try
					{
						dateTime = dateTime.AddMonths(1);
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
					}
				}
			}
			if (_monthBlocks.Length > 0)
			{
				_calendarValueState = new CalendarValueState(_monthBlocks[0, 0].GetVisibleRange().GetStart(), _monthBlocks[_monthRows - 1, _monthColumns - 1].GetVisibleRange().GetEnd());
			}
			ResetRangeSelection();
		}

		private void InvalidateDate(DateTime dateTime)
		{
			CalendarDayCell dayCellByDateInBlocks = FindDayCellByDateInBlocks(dateTime);
			if (dayCellByDateInBlocks != null)
			{
				InvalidateDayRect(dayCellByDateInBlocks.GetBounds());
			}
		}

		private void InvalidateDayRect(Rectangle rc)
		{
			Invalidate(rc);
		}
	}
}
