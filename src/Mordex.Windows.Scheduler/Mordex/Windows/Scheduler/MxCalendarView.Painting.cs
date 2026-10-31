using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxCalendarView
	{
		internal void DrawBorder(Graphics graphics)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (_borderStyle != BorderStyle.None)
			{
				clientRectangle = SchedulerRenderer.DrawBorder(graphics, GdiCache.Pen(GetThemeColors()._borderColor), clientRectangle);
			}
			else
			{
				ScrollBarStyle scrollBarStyle = new ScrollBarStyle(null);
				scrollBarStyle.ControlColor = GetBackgroundColor();
				SchedulerRenderer.RenderContent(graphics, scrollBarStyle, clientRectangle, GetEffectiveBorderStyle());
				scrollBarStyle.DisposeAllResources();
				scrollBarStyle = null;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				Graphics graphics = e.Graphics;
				UpdateWorkingHoursSchemaLayoutStateArray(false);
				if (_shouldRefreshBoldDates)
				{
					UpdateDateCollection(false);
					_shouldRefreshBoldDates = false;
				}
				_backgroundBrush = GdiCache.Brush(GetBackgroundColor());
				SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_monthBackgroundStyle);
				SchedulerFormatStyle alternatingMonthStyle = schedulerFormatStyle.MergeWith(_alternatingMonthBackgroundStyle);
				SchedulerFormatStyle headerStyle = new SchedulerFormatStyle(_headersFormatStyle);
				graphics.FillRectangle(_backgroundBrush, e.ClipRectangle);
				if (BackgroundImage != null)
				{
					switch (_backgroundImageDrawMode)
					{
					case ImageDrawMode.Centered:
						SchedulerRenderer.DrawImageCentered(graphics, BackgroundImage, _contentBounds, e.ClipRectangle);
						break;
					case ImageDrawMode.Stretched:
						SchedulerRenderer.DrawImageInRect(graphics, BackgroundImage, _contentBounds, e.ClipRectangle);
						break;
					case ImageDrawMode.Tiled:
						SchedulerRenderer.DrawImage(graphics, BackgroundImage, _contentBounds, e.ClipRectangle);
						break;
					}
				}
				if (WatermarkImage.Image != null)
				{
					_watermarkImage.DrawImage(graphics, _contentBounds);
				}
				if (_monthColumnCount == 0 || _monthRowCount == 0)
				{
					Rectangle rect = _contentBounds;
					graphics.FillRectangle(_backgroundBrush, rect);
				}
				else
				{
					TextBatch textBatch = new TextBatch();
					for (int i = 0; i < _monthRowCount; i++)
					{
						for (int j = 0; j < _monthColumnCount; j++)
						{
							WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState = _workingHoursSchemaLayoutStateArray[i, j];
							Rectangle rectangle = new Rectangle(workingHoursSchemaLayoutState.GetLeftMarginBounds().Left, workingHoursSchemaLayoutState.GetHeaderBounds().Bottom, workingHoursSchemaLayoutState.GetTotalSize().Width, workingHoursSchemaLayoutState.GetTotalSize().Height);
							Brush brush = GetMonthBackgroundBrush(workingHoursSchemaLayoutState, rectangle, schedulerFormatStyle, alternatingMonthStyle);
							if (brush != null)
							{
								graphics.FillRectangle(brush, rectangle);
							}
							Rectangle themedHeaderBounds = workingHoursSchemaLayoutState.GetHeaderBounds();
							if (_monthColumnCount > 1)
							{
								themedHeaderBounds.Width--;
								if (j > 0)
								{
									themedHeaderBounds.X++;
								}
							}
							// The themed look takes the header color from the header style, so a color set by the developer wins.
							Color headerColor = !headerStyle.BackColor.IsEmpty ? headerStyle.BackColor : GetThemeColors()._headerColor;
							graphics.FillRectangle(GdiCache.Brush(headerColor), themedHeaderBounds);
							if (i == 0)
							{
								DrawScrollArrow(graphics, -1, _rectangleArray[0], headerStyle);
								DrawScrollArrow(graphics, 1, _rectangleArray[1], headerStyle);
							}
							RenderContent(graphics, textBatch, workingHoursSchemaLayoutState, workingHoursSchemaLayoutState.GetHeaderBounds(), headerStyle);
							DrawBackground(graphics, textBatch, workingHoursSchemaLayoutState, graphics.ClipBounds);
						}
					}
				}
				DrawBorder(e.Graphics);
				DisposePaintResources();
				alternatingMonthStyle.DisposeResources();
				schedulerFormatStyle.DisposeResources();
				headerStyle.DisposeResources();
			}
			catch (Exception ex)
			{
				ReportPaintError(ex);
				DrawPaintError(e.Graphics, ex);
			}
		}

		// A paint must not open a dialog: a modal box paints the control again and blocks a run that nobody watches. Each
		// different error goes to Trace once; the control shows the error look on every paint that fails.
		internal void ReportPaintError(Exception exception)
		{
			string text = exception.GetType().FullName + ": " + exception.Message;
			if (_paintError != text)
			{
				_paintError = text;
				System.Diagnostics.Trace.WriteLine("MxCalendarView could not paint: " + exception);
			}
		}

		// The error look of WinForms: a red cross over the control and the error text.
		private void DrawPaintError(Graphics graphics, Exception exception)
		{
			try
			{
				Rectangle bounds = base.ClientRectangle;
				graphics.ResetClip();
				graphics.FillRectangle(Brushes.White, bounds);
				using (Pen pen = new Pen(Color.Red, 2f))
				{
					graphics.DrawRectangle(pen, bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2);
					graphics.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
					graphics.DrawLine(pen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Top);
				}
				TextRenderer.DrawText(graphics, exception.GetType().Name + ": " + exception.Message, SystemFonts.DefaultFont, Rectangle.Inflate(bounds, -6, -6), Color.Black, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
			}
			catch (Exception)
			{
				// Nothing more can be shown when even the error look cannot be drawn.
			}
		}

		private void DrawBackground(Graphics graphics, TextBatch textBatch, WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState, RectangleF rectangleF)
		{
			DayOfWeek firstDayOfWeek = ToSystemDayOfWeek(FirstDayOfWeek);
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateIsPaddingDisabled(true);
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_daysFormatStyle);
			if (workingHoursSchemaLayoutState.GetMonth() % 2 != 0)
			{
				schedulerFormatStyle = schedulerFormatStyle.MergeWith(_alternatingMonthDaysFormatStyle);
			}
			SchedulerFormatStyle dayHeaderStyle = schedulerFormatStyle.MergeWith(_daysHeadersFormatStyle);
			if (_themeColors != null)
			{
				_gridLinePen = GdiCache.Pen(_themeColors._lineColor);
			}
			else
			{
				_gridLinePen = GdiCache.Pen(SystemColors.ControlDark);
			}
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			Font font = dayHeaderStyle.ResolveFont(base.Font);
			Color color = dayHeaderStyle.ResolveForeColor(GetTextColor());
			int separatorEnd = 0;
			int separatorStart = 0;
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			for (int i = 0; i < 7; i++)
			{
				SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutState = workingHoursSchemaLayoutState.GetWeekdayHeaderCell(i);
				Rectangle rectangle = schedulerFormatStyleLayoutState.GetBounds();
				rectangle.Height--;
				graphics.DrawLine(_gridLinePen, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
				Brush brush = dayHeaderStyle.GetBackBrush(rectangle);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangle);
				}
				rectangle.X += 2;
				textLayoutRenderer.QueueText(graphics, textBatch, _textArray[i], rectangle, font, color);
			}
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
			bool isSelected = false;
			Dictionary<(SchedulerFormatStyle, bool, bool), SchedulerFormatStyle> cellStyles = new Dictionary<(SchedulerFormatStyle, bool, bool), SchedulerFormatStyle>();
			Font boldDayFont = null;
			List<(bool, int, int, int, Rectangle)> overlays = new List<(bool, int, int, int, Rectangle)>(6);
			Rectangle todayBounds = Rectangle.Empty;
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					SchedulerFormatStyleLayoutState schedulerFormatStyleLayoutStateArgument = workingHoursSchemaLayoutState.GetDayCell(i, j);
					if (RectangleF.Intersect(rectangleF, schedulerFormatStyleLayoutStateArgument.GetBounds()).IsEmpty)
					{
						continue;
					}
					isSelected = ((_selectionStyle != CalendarSelectionStyle.Single) ? schedulerFormatStyleLayoutStateArgument.IsSelected() : ((schedulerFormatStyleLayoutStateArgument.GetDate() == _currentDate && schedulerFormatStyleLayoutStateArgument.IsVisible()) ? true : false));
					SchedulerFormatStyle ownStyle = schedulerFormatStyleLayoutStateArgument.IsVisible() ? schedulerFormatStyleLayoutStateArgument.GetSchedulerFormatStyle() : null;
					bool isDimmed = schedulerFormatStyleLayoutStateArgument.IsOutsideMonth() || !schedulerFormatStyleLayoutStateArgument.IsWithinDateLimits();
					// The style of a cell depends only on these three values, so cells with the same values share one style.
					if (!cellStyles.TryGetValue((ownStyle, isSelected, isDimmed), out SchedulerFormatStyle dayStyle))
					{
						dayStyle = new SchedulerFormatStyle(schedulerFormatStyle);
						if (ownStyle != null)
						{
							dayStyle = dayStyle.MergeWith(ownStyle);
						}
						dayStyle = GetDayCellStyle(schedulerFormatStyleLayoutStateArgument, dayStyle, isSelected);
						cellStyles.Add((ownStyle, isSelected, isDimmed), dayStyle);
					}
					Brush dayBrush = dayStyle.GetBackBrush(schedulerFormatStyleLayoutStateArgument.GetBounds());
					Rectangle rectangle = schedulerFormatStyleLayoutStateArgument.GetBounds();
					if (dayBrush != null)
					{
						graphics.FillRectangle(dayBrush, rectangle);
					}
					if (schedulerFormatStyleLayoutStateArgument.IsVisible())
					{
						Font dayFont;
						bool ownsDayFont = false;
						bool isDeferred = false;
						if (schedulerFormatStyleLayoutStateArgument.IsBold())
						{
							Font baseDayFont = dayStyle.ResolveFont(Font);
							if (baseDayFont == Font)
							{
								boldDayFont ??= new Font(baseDayFont, baseDayFont.Style | FontStyle.Bold);
								dayFont = boldDayFont;
								isDeferred = true;
							}
							else
							{
								dayFont = new Font(baseDayFont, baseDayFont.Style | FontStyle.Bold);
								ownsDayFont = true;
							}
						}
						else
						{
							dayFont = dayStyle.ResolveFont(Font);
							isDeferred = dayFont == Font;
						}
						if (_todayRect && schedulerFormatStyleLayoutStateArgument.GetDate() == DateTime.Today)
						{
							todayBounds = rectangle;
						}
						if (!UseCompatibleTextRendering)
						{
							rectangle.Width -= 2;
						}
						if (isDeferred)
						{
							textLayoutRenderer.QueueText(graphics, textBatch, schedulerFormatStyleLayoutStateArgument.GetText(), rectangle, dayFont, dayStyle.ResolveForeColor(GetTextColor()));
						}
						else
						{
							textLayoutRenderer.DrawText(graphics, schedulerFormatStyleLayoutStateArgument.GetText(), rectangle, dayFont, dayStyle.ResolveForeColor(GetTextColor()));
						}
						if (ownsDayFont)
						{
							textBatch.ReleaseFont(graphics, dayFont);
						}
					}
				}
				bool hasSeparator = false;
				int separatorLeft = 0;
				if (_weekNumbers)
				{
					SchedulerFormatStyleLayoutState lastDayCell = workingHoursSchemaLayoutState.GetDayCell(i, 6);
					DateTime time = lastDayCell.GetDate();
					if (lastDayCell.IsVisible() || workingHoursSchemaLayoutState.GetDayCell(i, 0).IsVisible())
					{
						int weekOfYear = currentInfo.Calendar.GetWeekOfYear(time, _weekRule, firstDayOfWeek);
						Rectangle weekNumberBounds = lastDayCell.GetBounds();
						weekNumberBounds.X = workingHoursSchemaLayoutState.GetLeftMarginBounds().X;
						if (!UseCompatibleTextRendering)
						{
							weekNumberBounds.Width -= 2;
						}
						textLayoutRenderer.QueueText(graphics, textBatch, weekOfYear.ToString(Application.CurrentCulture), weekNumberBounds, GetWeekNumberFont(), color);
						separatorEnd = lastDayCell.GetBounds().Bottom - 3;
					}
					else
					{
						separatorEnd = lastDayCell.GetBounds().Top;
					}
					separatorStart = workingHoursSchemaLayoutState.GetWeekdayHeaderCell(0).GetBounds().Bottom;
					separatorLeft = workingHoursSchemaLayoutState.GetWeekdayHeaderCell(0).GetBounds().Left;
					hasSeparator = true;
				}
				overlays.Add((hasSeparator, separatorLeft, separatorStart, separatorEnd, todayBounds));
			}
			textBatch.Flush(graphics);
			// The separator lines and the today frame go over the text, as they did when the text was drawn row by row.
			foreach ((bool hasSeparator, int left, int start, int end, Rectangle today) in overlays)
			{
				if (hasSeparator)
				{
					graphics.DrawLine(_gridLinePen, left, start, left, end);
				}
				if (!today.IsEmpty)
				{
					Pen pen = (_themeColors == null) ? GdiCache.Pen(_todayRectColor) : GdiCache.Pen(_themeColors._todayBorderColor);
					graphics.DrawRectangle(pen, today.Left, today.Top, today.Width - 1, today.Height - 1);
				}
			}
			boldDayFont?.Dispose();
			foreach (SchedulerFormatStyle cellStyle in cellStyles.Values)
			{
				cellStyle.DisposeResources();
			}
			schedulerFormatStyle.DisposeResources();
			_gridLinePen = null;
			textLayoutRenderer.ResetFormat();
		}

		internal void DrawGradientScrollArrow(Graphics graphics, Rectangle rectangle, int direction, SchedulerFormatStyle schedulerFormatStyle)
		{
			Color color = schedulerFormatStyle.ForeColor;
			if (GetThemeColors() != null)
			{
				color = GetThemeColors()._glyphColor;
			}
			if (color == Color.Empty)
			{
				color = GetTextColor();
			}
			int arrowDepth = rectangle.Width / 4;
			int arrowHeight = arrowDepth * 2 - 1;
			int arrowX = ((direction <= 0) ? (rectangle.Left + (rectangle.Width - arrowDepth) / 2 + arrowDepth - 1) : (rectangle.Left + (rectangle.Width - arrowDepth) / 2));
			int arrowY = rectangle.Top + (rectangle.Height - arrowHeight) / 2 + 1;
			Pen pen = null;
			Rectangle rect = new Rectangle(arrowX - arrowDepth / 2, arrowY - arrowHeight / 2, arrowDepth * 2, arrowHeight * 2);
			Color gradientStartColor = Color.FromArgb(80, color);
			Color gradientEndColor = Color.FromArgb(245, color);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, gradientStartColor, gradientEndColor, LinearGradientMode.ForwardDiagonal);
			pen = new Pen(linearGradientBrush);
			linearGradientBrush.Dispose();
			while (arrowHeight >= 0)
			{
				if (arrowHeight == 1)
				{
					graphics.DrawLine(pen, arrowX, arrowY, arrowX, (float)arrowY + 0.5f);
				}
				else
				{
					graphics.DrawLine(pen, arrowX, arrowY, arrowX, arrowY + arrowHeight - 1);
				}
				arrowX += direction;
				arrowHeight -= 2;
				arrowY++;
			}
			pen.Dispose();
		}

		private void DrawScrollArrow(Graphics graphics, int direction, Rectangle rectangle, SchedulerFormatStyle schedulerFormatStyle)
		{
			Color color = schedulerFormatStyle.ForeColor;
			if (GetThemeColors() != null)
			{
				color = GetThemeColors()._glyphColor;
			}
			if (color == Color.Empty)
			{
				color = GetTextColor();
			}
			Pen pen = GdiCache.Pen(color);
			Point point = ((direction != -1) ? new Point(rectangle.Right - 6, rectangle.Top + (rectangle.Height - 9) / 2 + 4) : new Point(rectangle.Left + 6, rectangle.Top + (rectangle.Height - 9) / 2 + 4));
			Point pt = point;
			graphics.DrawLine(pen, point.X, point.Y, point.X - direction, point.Y);
			for (int i = 0; i < 4; i++)
			{
				point.Offset(-direction, -1);
				pt.Offset(-direction, 1);
				graphics.DrawLine(pen, point, pt);
			}
		}

		private void RenderContent(Graphics graphics, TextBatch textBatch, WorkingHoursSchemaLayoutState workingHoursSchemaLayoutState, Rectangle rectangle, SchedulerFormatStyle schedulerFormatStyle)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateRightToLeft(RightToLeft);
			textLayoutRenderer.QueueText(graphics, textBatch, workingHoursSchemaLayoutState.GetHeaderText(), rectangle, schedulerFormatStyle.ResolveFont(HeaderFont), schedulerFormatStyle.ResolveForeColor(GetTextColor()));
			textLayoutRenderer.ResetFormat();
		}
	}
}
