using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Design;

namespace Mordex.Windows.Timeline
{
	public partial class TimelineSurface
	{
		internal void DrawBorder(Graphics graphics)
		{
			if (_borderStyle == BorderStyle.None)
			{
				return;
			}
			if (_themeRenderer == null)
			{
				_themeRenderer = new TimelineThemeRenderer();
			}
			TimelineUtilities.DrawFlatBorder(graphics, GdiCache.Pen(_themeRenderer._controlBorderColor), base.ClientRectangle);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				Graphics graphics = e.Graphics;
				UpdateTimelineLayoutStateArray(false);
				_backgroundBrush = GdiCache.Brush(GetEffectiveBackColor());
				graphics.FillRectangle(_backgroundBrush, base.ClientRectangle);
				TimelineFormatStyle timelineFormatStyle = TimelineFormatStyle.CloneForPainting(_monthBackgroundStyle);
				TimelineFormatStyle headerStyle = TimelineFormatStyle.CloneForPainting(_headerFormatStyle);
				Brush brush = timelineFormatStyle.GetBackBrush(base.ClientRectangle);
				if (brush != null)
				{
					graphics.FillRectangle(brush, e.ClipRectangle);
				}
				if (_monthColumnCount == 0 || _monthRowCount == 0)
				{
					return;
				}
				if (_themeRenderer == null)
				{
					_themeRenderer = new TimelineThemeRenderer();
				}
				for (int i = 0; i < _monthRowCount; i++)
				{
					for (int j = 0; j < _monthColumnCount; j++)
					{
						TimelineLayoutState timelineLayoutState = _timelineLayoutStateArray[i, j];
						Rectangle monthBodyRect = new Rectangle(timelineLayoutState.GetLeftMarginBounds().Left, timelineLayoutState.GetHeaderBounds().Bottom, timelineLayoutState.GetTotalSize().Width, timelineLayoutState.GetTotalSize().Height);
						brush = GetMonthBodyBrush(timelineLayoutState, monthBodyRect, timelineFormatStyle);
						if (brush != null)
						{
							graphics.FillRectangle(brush, monthBodyRect);
						}
						Rectangle rect = timelineLayoutState.GetHeaderBounds();
						if (_monthColumnCount > 1)
						{
							rect.Width--;
							if (j > 0)
							{
								rect.X++;
							}
						}
						graphics.FillRectangle(GdiCache.Brush(_themeRenderer._calendarHighlightColor), rect);
						if (i == 0)
						{
							DrawNavigationArrow(graphics, -1, _rectangleArray[0], headerStyle);
							DrawNavigationArrow(graphics, 1, _rectangleArray[1], headerStyle);
						}
						RenderContent(graphics, timelineLayoutState, timelineLayoutState.GetHeaderBounds(), headerStyle);
						DrawBackground(graphics, timelineLayoutState);
					}
				}
				if (!_todayButton._bounds.IsEmpty)
				{
					_themeRenderer.DrawButtonBackground(graphics, _todayButton._bounds, _todayButton.IsHovered(), _todayButton.IsPressed(), base.Enabled);
					TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!ShouldUseCompatibleTextRendering());
					textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
					textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
					textLayoutRenderer.UpdateRightToLeft(RightToLeft);
					textLayoutRenderer.DrawText(graphics, TodayText, _todayButton._bounds, HeaderFormatStyle.ResolveFont(Font), HeaderFormatStyle.ResolveForeColor(GetEffectiveForeColor()));
				}
				if (!_noneButton._bounds.IsEmpty)
				{
					ScrollBarStyle noneButtonStyle = (timeline == null) ? new ScrollBarStyle() : timeline.ScrollBarStyle;
					if (!TimelineVisualStyleRenderer.AreVisualStylesActive(true) || !TimelineVisualStyleRenderer.DrawText(graphics, _noneButton._bounds, HeaderFont, _text, _noneButton.IsHovered(), _noneButton.IsPressed()))
					{
						_noneButton.DrawText(graphics, _text, HeaderFont, noneButtonStyle, RightToLeft == RightToLeft.Yes);
					}
				}
				DrawBorder(e.Graphics);
				DisposeResources();
				HeaderFormatStyle.DisposeResources();
				if (timeline != null)
				{
					timeline.ScrollBarStyle.DisposeAllResources();
				}
				timelineFormatStyle.DisposeResources();
				headerStyle.DisposeResources();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Calendar paint failed: " + ex);
			}
		}

		private void DrawBackground(Graphics graphics, TimelineLayoutState timelineLayoutState)
		{
			System.DayOfWeek firstDayOfWeek = ToSystemDayOfWeek(GetDayOfWeek());
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!ShouldUseCompatibleTextRendering());
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateIsPaddingDisabled(true);
			textLayoutRenderer.Batch = textLayoutRenderer._useTextRenderer ? new TextBatch() : null;
			System.Collections.Generic.List<Font> boldFonts = new System.Collections.Generic.List<Font>();
			TimelineFormatStyle timelineFormatStyle = TimelineFormatStyle.CloneForPainting(_daysFormatStyle);
			TimelineFormatStyle selectedStyle = timelineFormatStyle.MergeWith(_selectedFormatStyle);
			TimelineFormatStyle trailingStyle = timelineFormatStyle.MergeWith(_trailingFormatStyle);
			TimelineFormatStyle dayHeadersStyle = timelineFormatStyle.MergeWith(_dayHeadersFormatStyle);
			_gridLinePen = GdiCache.Pen(_themeRenderer != null ? _themeRenderer._lineColor : SystemColors.ControlDark);
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			Color color = dayHeadersStyle.ResolveForeColor(GetEffectiveForeColor());
			Font font = dayHeadersStyle.ResolveFont(base.Font);
			int y = 0;
			int separatorTop = 0;
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			for (int i = 0; i < 7; i++)
			{
				TimelineCalendarDay dayHeader = timelineLayoutState.GetWeekdayHeaderCell(i);
				Rectangle rectangle = dayHeader.GetBounds();
				rectangle.Height--;
				graphics.DrawLine(_gridLinePen, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
				Brush brush = dayHeadersStyle.GetBackBrush(rectangle);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
				}
				rectangle.X += 2;
				textLayoutRenderer.DrawText(graphics, _textArray[i], rectangle, font, color);
			}
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
			bool isSelectedDay = false;
			for (int j = 0; j < 6; j++)
			{
				TimelineCalendarDay timelineLayoutState2Argument;
				for (int k = 0; k < 7; k++)
				{
					timelineLayoutState2Argument = timelineLayoutState.GetDayCell(j, k);
					isSelectedDay = ((timelineLayoutState2Argument.GetDate() == _currentDate && !_isNoneSelected) ? true : false);
					Brush backgroundBrush;
					Brush textBrush;
					if (isSelectedDay && _themeRenderer != null)
					{
						backgroundBrush = GdiCache.Brush(_themeRenderer._currentDateFillColor);
						textBrush = GetDayTextBrush(timelineLayoutState2Argument, timelineFormatStyle, selectedStyle, trailingStyle, false);
					}
					else
					{
						backgroundBrush = GetDayBackgroundBrush(timelineLayoutState2Argument, timelineFormatStyle, selectedStyle, trailingStyle, isSelectedDay);
						textBrush = GetDayTextBrush(timelineLayoutState2Argument, timelineFormatStyle, selectedStyle, trailingStyle, isSelectedDay);
					}
					Rectangle rectangle = timelineLayoutState2Argument.GetBounds();
					if (backgroundBrush != null)
					{
						graphics.FillRectangle(backgroundBrush, rectangle);
					}
					if (timelineLayoutState2Argument.IsVisible())
					{
						Font dayFont;
						if (timelineLayoutState2Argument.IsBold())
						{
							Font baseFont = GetDayFont(timelineLayoutState2Argument, timelineFormatStyle, selectedStyle, trailingStyle, isSelectedDay);
							dayFont = new Font(baseFont, baseFont.Style | FontStyle.Bold);
						}
						else
						{
							dayFont = GetDayFont(timelineLayoutState2Argument, timelineFormatStyle, selectedStyle, trailingStyle, isSelectedDay);
						}
						if (_todayRect && timelineLayoutState2Argument.GetDate() == DateTime.Today)
						{
							graphics.DrawRectangle(GdiCache.Pen(TodayRectColor), rectangle.Left, rectangle.Top, rectangle.Width - 1, rectangle.Height - 1);
						}
						if (!ShouldUseCompatibleTextRendering())
						{
							rectangle.Width -= 2;
						}
						if (textBrush != null)
						{
							textLayoutRenderer.DrawTextWithBrush(graphics, timelineLayoutState2Argument.GetDayText(), rectangle, dayFont, textBrush);
						}
						if (!ShouldUseCompatibleTextRendering())
						{
							rectangle.Width += 2;
						}
						if (timelineLayoutState2Argument.IsBold())
						{
							boldFonts.Add(dayFont);
							dayFont = null;
						}
					}
					if (j == 5 && _isTodayButtonVisible)
					{
						graphics.DrawLine(_gridLinePen, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
					}
				}
				if (!_weekNumbers)
				{
					continue;
				}
				timelineLayoutState2Argument = timelineLayoutState.GetDayCell(j, 6);
				if (!timelineLayoutState2Argument.IsVisible())
				{
					timelineLayoutState2Argument = timelineLayoutState.GetDayCell(j, 0);
				}
				if (timelineLayoutState2Argument.IsVisible())
				{
					int weekOfYear = currentInfo.Calendar.GetWeekOfYear(timelineLayoutState2Argument.GetDate(), GetWeekRule(), firstDayOfWeek);
					Rectangle weekNumberRect = timelineLayoutState2Argument.GetBounds();
					weekNumberRect.X = timelineLayoutState.GetLeftMarginBounds().X;
					if (!ShouldUseCompatibleTextRendering())
					{
						weekNumberRect.Width -= 2;
					}
					textLayoutRenderer.DrawText(graphics, weekOfYear.ToString(Application.CurrentCulture), weekNumberRect, GetWeekNumberFont(), color);
					y = timelineLayoutState2Argument.GetBounds().Bottom - 3;
				}
				else
				{
					y = timelineLayoutState2Argument.GetBounds().Top;
				}
			}
			if (_weekNumbers)
			{
				separatorTop = timelineLayoutState.GetWeekdayHeaderCell(0).GetBounds().Bottom;
				graphics.DrawLine(_gridLinePen, timelineLayoutState.GetLeftMarginBounds().Right, separatorTop, timelineLayoutState.GetLeftMarginBounds().Right, y);
			}
			textLayoutRenderer.Batch?.Flush(graphics);
			foreach (Font boldFont in boldFonts)
			{
				boldFont.Dispose();
			}
			selectedStyle.DisposeResources();
			timelineFormatStyle.DisposeResources();
			trailingStyle.DisposeResources();
			dayHeadersStyle.DisposeResources();
			_gridLinePen = null;
			textLayoutRenderer.ResetFormat();
		}

		private void DrawNavigationArrow(Graphics graphics, int direction, Rectangle rectangle, TimelineFormatStyle timelineFormatStyle)
		{
			Color color = timelineFormatStyle.ForeColor;
			if (_themeRenderer != null)
			{
				color = _themeRenderer._textColor;
			}
			else if (color == Color.Empty)
			{
				color = GetEffectiveForeColor();
			}
			Pen pen = GdiCache.Pen(color);
			Point point = ((direction != -1) ? new Point(rectangle.Right - 6, rectangle.Top + rectangle.Height / 2) : new Point(rectangle.Left + 6, rectangle.Top + rectangle.Height / 2));
			Point pt = point;
			graphics.DrawLine(pen, point.X, point.Y, point.X - direction, point.Y);
			for (int i = 0; i < 4; i++)
			{
				point.Offset(-direction, -1);
				pt.Offset(-direction, 1);
				graphics.DrawLine(pen, point, pt);
			}
		}

		private void RenderContent(Graphics graphics, TimelineLayoutState timelineLayoutState, Rectangle rectangle, TimelineFormatStyle timelineFormatStyle)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!ShouldUseCompatibleTextRendering());
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateRightToLeft(RightToLeft);
			textLayoutRenderer.DrawText(graphics, timelineLayoutState.GetHeaderText(), rectangle, timelineFormatStyle.ResolveFont(HeaderFont), timelineFormatStyle.ResolveForeColor(GetEffectiveForeColor()));
			textLayoutRenderer.ResetFormat();
		}
	}
}
