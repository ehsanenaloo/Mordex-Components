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
using System.Runtime.CompilerServices;
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
		// A handler of the draw event may paint over a text that is still waiting.
		private void FlushBeforeOwnerDraw(TextLayoutRenderer textLayoutRenderer, Graphics graphics, OwnerDrawnArea area)
		{
			if (textLayoutRenderer.Batch != null && textLayoutRenderer.Batch.Count > 0 && (_ownerDrawnArea & area) != OwnerDrawnArea.None)
			{
				textLayoutRenderer.Batch.Flush(graphics);
			}
		}

		internal void DrawTierLabels(Graphics graphics, Timescale timescale, DateTime dateTimeArgument, Rectangle rectangle, TextLayoutRenderer textLayoutRenderer, TimelineFormatStyle timelineFormatStyle, bool showWeekNumber, bool booleanArgument, float floatValue, bool isTopTier)
		{
			float labelStartX = rectangle.Left;
			float labelEndX = 0f;
			DateTime dateTime = dateTimeArgument;
			DateTime intervalStart = AlignToInterval(dateTimeArgument, timescale.Interval, timescale.Count);
			Rectangle empty = Rectangle.Empty;
			DateTime intervalEnd = intervalStart;
			string text = GetTimescaleFormat(timescale);
			Brush brush = timelineFormatStyle.GetForeColorBrush(GetForegroundColor());
			long secondsPerCell = GetBottomTierStepSeconds(dateTimeArgument, false);
			float intervalWidth = 0f;
			do
			{
				switch (timescale.Interval)
				{
				case TimelineInterval.Second:
					intervalEnd = intervalStart.AddSeconds(timescale.Count);
					intervalWidth = (float)timescale.Count / (float)secondsPerCell * floatValue;
					showWeekNumber = false;
					break;
				case TimelineInterval.Minute:
					intervalEnd = intervalStart.AddMinutes(timescale.Count);
					intervalWidth = 60f * (float)timescale.Count / (float)secondsPerCell * floatValue;
					showWeekNumber = false;
					break;
				case TimelineInterval.Hour:
					intervalEnd = intervalStart.AddHours(timescale.Count);
					intervalWidth = 3600f * (float)timescale.Count / (float)secondsPerCell * floatValue;
					showWeekNumber = false;
					break;
				case TimelineInterval.Day:
					intervalEnd = intervalStart.AddDays(timescale.Count);
					intervalWidth = 86400f * (float)timescale.Count / (float)secondsPerCell * floatValue;
					break;
				case TimelineInterval.Week:
					intervalEnd = intervalStart.AddDays(7.0 * timescale.Count);
					intervalWidth = 604800f * (float)timescale.Count / (float)secondsPerCell * floatValue;
					break;
				case TimelineInterval.Month:
					intervalEnd = intervalStart.AddMonths(timescale.Count);
					intervalWidth = 2592000f * (float)timescale.Count / (float)secondsPerCell * floatValue;
					break;
				case TimelineInterval.Year:
					intervalEnd = intervalStart.AddYears(timescale.Count);
					intervalWidth = 31536000f * (float)timescale.Count / (float)secondsPerCell * floatValue;
					showWeekNumber = false;
					break;
				}
				float intervalSeconds = (float)intervalEnd.Subtract(dateTime).TotalSeconds;
				labelEndX = labelStartX + intervalSeconds / (float)secondsPerCell * floatValue;
				string labelText = "";
				if (showWeekNumber)
				{
					DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
					System.DayOfWeek firstDayOfWeek = TimelineUtilities.ToSystemDayOfWeek(FirstDayOfWeek);
					int weekOfYear = currentInfo.Calendar.GetWeekOfYear(dateTime, WeekRule, firstDayOfWeek);
					labelText = _weekText + " " + weekOfYear + ": ";
				}
				try
				{
					if (text.Length > 0)
					{
						labelText += intervalStart.ToString(text, Application.CurrentCulture);
					}
				}
				catch
				{
					labelText += text;
				}
				float textWidth = textLayoutRenderer.RenderContent(graphics, labelText, timescale.Font, 0).Width + 1f;
				RectangleF rectangleF;
				if (textWidth > intervalWidth)
				{
					rectangleF = new RectangleF(labelStartX, rectangle.Top, labelEndX - labelStartX, rectangle.Height);
					empty = Rectangle.Empty;
				}
				else
				{
					int rightPadding = _upperTierCharWidth;
					if (!booleanArgument)
					{
						rightPadding = 0;
					}
					if (labelStartX + textWidth >= labelEndX - (float)rightPadding)
					{
						labelStartX = labelEndX - textWidth - (float)rightPadding;
					}
					rectangleF = new RectangleF(labelStartX, rectangle.Top, textWidth, rectangle.Height);
					int arrowSize = rectangle.Height / 2 + 2;
					empty = new Rectangle((int)rectangleF.Right, rectangle.Bottom - arrowSize / 2 - 4, arrowSize, arrowSize);
				}
				DrawTimelineAreaEventArgs e = null;
				if (brush != null)
				{
					FlushBeforeOwnerDraw(textLayoutRenderer, graphics, isTopTier ? OwnerDrawnArea.TopTierInterval : OwnerDrawnArea.MiddleTierInterval);
					e = ((!isTopTier) ? new DrawTimelineAreaEventArgs(graphics, this, OwnerDrawnArea.MiddleTierInterval, TimelineUtilities.RoundToRectangle(rectangleF), null, brush, intervalStart) : new DrawTimelineAreaEventArgs(graphics, this, OwnerDrawnArea.TopTierInterval, TimelineUtilities.RoundToRectangle(rectangleF), null, brush, intervalStart));
					RaiseDrawTimelineArea(e);
					if (!e.Handled)
					{
						textLayoutRenderer.DrawTextWithBrush(graphics, labelText, TimelineUtilities.RoundToRectangle(rectangleF), timescale.Font, brush);
					}
				}
				labelStartX = labelEndX;
				dateTime = intervalEnd;
				intervalStart = intervalEnd;
				if (booleanArgument && _dropDownNavigator && (e == null || !e.Handled))
				{
					Color color = timelineFormatStyle.ResolveForeColor(GetForegroundColor());
					if (textLayoutRenderer.Batch != null && textLayoutRenderer.Batch.IntersectsPending(Rectangle.Inflate(empty, 2, 2)))
					{
						textLayoutRenderer.Batch.Flush(graphics);
					}
					_scrollBarStyle.DrawDownArrow(graphics, empty, color);
				}
			}
			while ((int)labelStartX < rectangle.Right);
		}

		internal void DrawUpperTier(Graphics graphics, Timescale timescale, DateTime dateTime, Rectangle rectArgument, Rectangle rectangle, bool showWeekNumber, bool booleanArgument, bool isTopTier)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateWrapsText(false);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_upperScaleTiersFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
			timelineFormatStyle = timelineFormatStyle.MergeWith(timescale.FormatStyle);
			if (_themeRenderer == null)
			{
				_themeRenderer = new TimelineThemeRenderer();
			}
			Brush brush = timelineFormatStyle.GetBackBrush(Rectangle.Ceiling(rectArgument));
			if (brush != null)
			{
				graphics.FillRectangle(brush, rectArgument);
			}
			if (timelineFormatStyle.BackgroundImage != null)
			{
				DrawBackgroundImage(graphics, Rectangle.Ceiling(rectArgument), timelineFormatStyle.BackgroundImage, timelineFormatStyle.BackgroundImageDrawMode, rectangle);
			}
			if (_selectedRange.Start != DateTime.MinValue && timescale.HasFocus())
			{
				TimelineFormatStyle selectedStyle = null;
				if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
				{
					if (_hideSelection == HideSelection.HighlightInactive)
					{
						selectedStyle = timelineFormatStyle.MergeWith(_selectedInactiveFormatStyle);
					}
				}
				else
				{
					selectedStyle = timelineFormatStyle.MergeWith(_selectedFormatStyle);
				}
				RectangleF rectangleF = GetSelectedRangeBounds(rectArgument);
				if (selectedStyle != null)
				{
					brush = selectedStyle.GetBackBrush(Rectangle.Ceiling(rectangleF));
				}
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangleF);
				}
				if (selectedStyle.BackgroundImage != null)
				{
					DrawBackgroundImage(graphics, Rectangle.Ceiling(rectangleF), selectedStyle.BackgroundImage, selectedStyle.BackgroundImageDrawMode, rectangle);
				}
				selectedStyle.DisposeResources();
			}
			// The labels are drawn together under the clip of the tier, before the border line below them.
			textLayoutRenderer.Batch = BatchLabels && BatchTiers && textLayoutRenderer._useTextRenderer ? new TextBatch() : null;
			DrawTierLabels(graphics, timescale, dateTime, rectArgument, textLayoutRenderer, timelineFormatStyle, showWeekNumber, booleanArgument, _intervalWidth, isTopTier);
			textLayoutRenderer.Batch?.Flush(graphics);
			textLayoutRenderer.Batch = null;
			Pen pen = _themeRenderer.GetBorderPen();
			graphics.DrawLine(pen, timescale.GetBounds().Left, timescale.GetBounds().Bottom, timescale.GetBounds().Right - 1, timescale.GetBounds().Bottom);
			textLayoutRenderer.ResetFormat();
			timelineFormatStyle.DisposeResources();
		}

		internal void DrawLowerTier(Graphics graphics, DateTime dateTime, Rectangle rect, float floatValue, int cellWidth, Rectangle rectangle, bool showSelection)
		{
			if (rect.Height == 0 || rect.Width == 0)
			{
				return;
			}
			Brush brush = null;
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(_lowerScaleTierFormatStyle, GetEffectiveAlpha(), GetBackColorAlphaMode(), GetForeColorAlphaMode());
			timelineFormatStyle = timelineFormatStyle.MergeWith(BottomTier.FormatStyle);
			TimelineFormatStyle selectedStyle = null;
			brush = timelineFormatStyle.GetBackBrush(rect);
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
			if (timelineFormatStyle.BackgroundImage != null)
			{
				DrawBackgroundImage(graphics, rect, timelineFormatStyle.BackgroundImage, timelineFormatStyle.BackgroundImageDrawMode, rectangle);
			}
			RectangleF rectangleF = RectangleF.Empty;
			if (_selectedRange.Start != DateTime.MinValue && showSelection)
			{
				if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
				{
					if (_hideSelection == HideSelection.HighlightInactive)
					{
						selectedStyle = timelineFormatStyle.MergeWith(_selectedInactiveFormatStyle);
					}
				}
				else
				{
					selectedStyle = timelineFormatStyle.MergeWith(_selectedFormatStyle);
				}
				rectangleF = GetSelectedRangeBounds(rect);
				if (selectedStyle != null)
				{
					brush = selectedStyle.GetBackBrush(Rectangle.Ceiling(rectangleF));
				}
				if (brush != null)
				{
					graphics.FillRectangle(brush, rectangleF);
				}
				if (selectedStyle != null)
				{
					if (selectedStyle.BackgroundImage != null)
					{
						DrawBackgroundImage(graphics, Rectangle.Ceiling(rectangleF), selectedStyle.BackgroundImage, selectedStyle.BackgroundImageDrawMode, rectangle);
					}
					selectedStyle.DisposeResources();
				}
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Far);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.Batch = BatchLabels && BatchTiers && textLayoutRenderer._useTextRenderer ? new TextBatch() : null;
			DrawLowerTierCells(graphics, textLayoutRenderer, dateTime, rect, floatValue, cellWidth, true, timelineFormatStyle, rectangleF);
			textLayoutRenderer.Batch?.Flush(graphics);
			textLayoutRenderer.Batch = null;
			textLayoutRenderer.ResetFormat();
			timelineFormatStyle.DisposeResources();
		}

		internal void DrawLowerTierCells(Graphics graphics, TextLayoutRenderer textLayoutRenderer, DateTime dateTimeArgument, Rectangle rectangle, float floatValue, int width, bool isThemed, TimelineFormatStyle baseStyle, RectangleF rectangleFArgument)
		{
			DateTimeFormatInfo currentInfo = DateTimeFormatInfo.CurrentInfo;
			RectangleF rectangleF = new RectangleF(floatValue, (float)rectangle.Top + 2f, width, (float)rectangle.Height - 10f);
			int cellX = (int)floatValue;
			Font font = BottomTier.Font;
			_ = BottomTier.Count;
			Pen pen = null;
			pen = ((!isThemed) ? GdiCache.Pen(baseStyle.ResolveForeColor(GetForegroundColor())) : _themeRenderer.GetBorderPen());
			float tickTop = rectangleF.Bottom - 1f;
			string text = "";
			string format = GetBottomTierFormat();
			DateTime dateTime = AlignToInterval(dateTimeArgument, BottomTier.Interval, BottomTier.Count);
			bool hasWeekNumbers = _showWeekNumbers && BottomTier.Interval == TimelineInterval.Week;
			while ((double)cellX < (double)rectangle.Right)
			{
				if (format == "W")
				{
					text = currentInfo.GetAbbreviatedDayName(dateTime.DayOfWeek);
				}
				else
				{
					try
					{
						text = dateTime.ToString(format, currentInfo);
					}
					catch
					{
						text = format;
					}
					if (format.IndexOf("W", 0) >= 0)
					{
						string abbreviatedDayName = currentInfo.GetAbbreviatedDayName(dateTime.DayOfWeek);
						text = text.Replace("W", abbreviatedDayName);
					}
				}
				if (hasWeekNumbers)
				{
					System.DayOfWeek dayOfWeek = TimelineUtilities.ToSystemDayOfWeek(FirstDayOfWeek);
					DateTime time = TimelineUtilities.GetWeekEndDate(dateTime, dayOfWeek);
					int weekOfYear = currentInfo.Calendar.GetWeekOfYear(time, WeekRule, dayOfWeek);
					text = _weekText + " " + weekOfYear + ": " + text;
				}
				TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(baseStyle);
				if (!rectangleFArgument.IsEmpty && rectangleFArgument.Contains(rectangleF))
				{
					if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
					{
						if (_hideSelection == HideSelection.HighlightInactive)
						{
							timelineFormatStyle = timelineFormatStyle.MergeWith(_selectedInactiveFormatStyle);
						}
					}
					else
					{
						timelineFormatStyle = timelineFormatStyle.MergeWith(_selectedFormatStyle);
					}
				}
				Brush brush = timelineFormatStyle.GetForeColorBrush(GetForegroundColor());
				FlushBeforeOwnerDraw(textLayoutRenderer, graphics, OwnerDrawnArea.BottomTierInterval);
				DrawTimelineAreaEventArgs e = new DrawTimelineAreaEventArgs(graphics, this, OwnerDrawnArea.BottomTierInterval, TimelineUtilities.RoundToRectangle(rectangleF), null, brush, dateTime);
				RaiseDrawTimelineArea(e);
				if (!e.Handled)
				{
					textLayoutRenderer.DrawTextWithBrush(graphics, text, TimelineUtilities.RoundToRectangle(rectangleF), font, brush);
				}
				if (textLayoutRenderer.Batch != null && textLayoutRenderer.Batch.IntersectsPending(new Rectangle(cellX - 1, (int)tickTop + 1, 3, 9)))
				{
					textLayoutRenderer.Batch.Flush(graphics);
				}
				graphics.DrawLine(pen, cellX, tickTop + 2f, cellX, tickTop + 8f);
				dateTime = ((BottomTier.Interval != TimelineInterval.Month) ? ((BottomTier.Interval != TimelineInterval.Year) ? dateTime.AddSeconds(_intervalSeconds) : dateTime.AddYears(BottomTier.Count)) : dateTime.AddMonths(BottomTier.Count));
				cellX += width;
				rectangleF.X = cellX;
				timelineFormatStyle.DisposeResources();
			}
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
		}
	}
}
