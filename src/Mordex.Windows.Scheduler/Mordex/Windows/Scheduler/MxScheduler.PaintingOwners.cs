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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Scheduler.Design;

namespace Mordex.Windows.Scheduler
{
	public partial class MxScheduler
	{
		// For tests and the benchmark: false draws each label of the time navigator on its own.
		internal static bool BatchTimeNavigatorLabels = true;

		// For tests: labels of the time navigator that went to the batch, and labels drawn on their own, on this thread.
		[ThreadStatic]
		internal static long TimeNavigatorLabelsBatched;

		[ThreadStatic]
		internal static long TimeNavigatorLabelsDrawnAlone;

		internal void DrawThemedTimeNavigatorBackground(Graphics graphics, Rectangle rect)
		{
			UpdateThemedRendering();
			// A fill color that the developer set on the time navigator style wins over the theme.
			using (Brush developerFill = _timeNavigatorFormatStyle.CreateDeveloperBackBrush(rect))
			{
				graphics.FillRectangle(developerFill ?? GdiCache.Brush(_themeRenderer._normalEndColor), rect);
			}
		}

		private void DrawTimeNavigatorBackground(Graphics graphics)
		{
			DrawThemedTimeNavigatorBackground(graphics, _timeNavigatorBounds);
		}

		private void DrawTimeNavigator(Graphics graphics, int lastRowBottom)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateWrapsText(false);
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_timeNavigatorFormatStyle);
			int rowTop = _dayAreaTop + base.ClientRectangle.Top;
			int startHour = _firstVisibleSlotTime.Hours;
			if (_firstVisibleSlotTime.Minutes != 0)
			{
				int hiddenSlotCount = _firstVisibleSlotTime.Minutes / _minuteInterval;
				rowTop -= hiddenSlotCount * _actualRowHeight;
				startHour++;
			}
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			string text = currentCulture.DateTimeFormat.AMDesignator;
			if (text == "")
			{
				text = "00";
			}
			int amDesignatorWidth = (int)textLayoutRenderer.RenderContent(graphics, text, DayFont, 0).Width;
			if (UseCompatibleTextRendering)
			{
				amDesignatorWidth += 2;
			}
			int timeLabelWidth = ((_interval != Interval.SixtyMinutes) ? ((int)textLayoutRenderer.RenderContent(graphics, "12", _baseFont, 0).Width + amDesignatorWidth) : ((int)textLayoutRenderer.RenderContent(graphics, "12:", _baseFont, 0).Width + amDesignatorWidth));
			_timeNavigatorBounds = new Rectangle(base.ClientRectangle.Left, _dayAreaTop + base.ClientRectangle.Top, timeLabelWidth + 5, base.ClientRectangle.Height - _dayAreaTop);
			if ((_ownerDrawnArea & OwnerDrawnArea.TimeNavigator) != OwnerDrawnArea.None)
			{
				DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, this, OwnerDrawnArea.TimeNavigator, _timeNavigatorBounds, null);
				RaiseDrawSchedulerArea(e);
				if (e.Handled)
				{
					return;
				}
			}
			DrawTimeNavigatorBackground(graphics);
			int hourRowHeight = _actualRowHeight;
			if (_interval != Interval.SixtyMinutes)
			{
				hourRowHeight *= 2;
			}
			SetSchedulerFont(schedulerFormatStyle);
			_dayAreaLeft = timeLabelWidth + 5 + _columnGap + base.ClientRectangle.Left;
			int designatorLeft = timeLabelWidth - amDesignatorWidth + base.ClientRectangle.Left;
			string hourFormat = "%h";
			bool shouldHighlightCurrentTime = false;
			int lineInset = 5;
			if ((_highlightCurrentTime == TriState.True || _highlightCurrentTime == TriState.Empty) && _dates.Contains(DateTime.Today))
			{
				shouldHighlightCurrentTime = true;
			}
			string amDesignator;
			string pmDesignator;
			if (_timeFormat == TimeFormat.TwentyFourHours)
			{
				hourFormat = "%H";
				amDesignator = "00";
				pmDesignator = "00";
			}
			else
			{
				amDesignator = currentCulture.DateTimeFormat.AMDesignator;
				pmDesignator = currentCulture.DateTimeFormat.PMDesignator;
			}
			if (amDesignator == string.Empty)
			{
				amDesignator = "00";
			}
			if (pmDesignator == string.Empty)
			{
				pmDesignator = "00";
			}
			amDesignator = amDesignator.ToLower(currentCulture);
			pmDesignator = pmDesignator.ToLower(currentCulture);
			DateTime dateTime = ((_dates.Count <= 0) ? _date.Date.Add(_firstVisibleSlotTime) : _dates[0].Date.Add(_firstVisibleSlotTime));
			Color color = _themeRenderer._lineColor;
			// The labels are drawn together at the end with one device context. A line or a fill that would paint over a label
			// still waiting draws the waiting labels first, so the order of the pixels stays the same.
			TextBatch labels = BatchTimeNavigatorLabels ? new TextBatch() : null;
			List<Rectangle> waiting = labels != null ? new List<Rectangle>() : null;
			void Label(string labelText, Rectangle bounds, Font font, Brush brush)
			{
				Rectangle painted = textLayoutRenderer.QueueTextWithBrush(graphics, labels, labelText, bounds, font, brush);
				if (painted.IsEmpty)
				{
					TimeNavigatorLabelsDrawnAlone++;
				}
				else
				{
					TimeNavigatorLabelsBatched++;
					waiting.Add(painted);
				}
			}
			void BeforePaint(Rectangle area)
			{
				if (waiting == null || waiting.Count == 0)
				{
					return;
				}
				foreach (Rectangle painted in waiting)
				{
					if (painted.IntersectsWith(area))
					{
						labels.Flush(graphics);
						waiting.Clear();
						return;
					}
				}
			}
			void Line(Point from, Point to)
			{
				BeforePaint(Rectangle.FromLTRB(Math.Min(from.X, to.X), Math.Min(from.Y, to.Y), Math.Max(from.X, to.X) + 1, Math.Max(from.Y, to.Y) + 1));
				graphics.DrawLine(GetLightLightPen(color), from, to);
			}
			do
			{
				string designatorLabel = "00";
				if (dateTime.TimeOfDay.Hours == 0)
				{
					designatorLabel = amDesignator;
				}
				else if (dateTime.TimeOfDay.Hours == 12)
				{
					designatorLabel = pmDesignator;
				}
				else if (dateTime.TimeOfDay.Hours == startHour)
				{
					designatorLabel = ((dateTime.TimeOfDay.Hours >= 12) ? pmDesignator : amDesignator);
				}
				string hourText = dateTime.ToString(hourFormat, currentCulture);
				if (_interval == Interval.SixtyMinutes)
				{
					hourText = ((!(designatorLabel == "00")) ? (hourText + " ") : (hourText + ":"));
				}
				if (shouldHighlightCurrentTime && dateTime.Hour == DateTime.Now.Hour)
				{
					int hourHeight = _actualRowHeight * (int)_interval;
					float currentTimeY = (float)hourHeight / 60f * (float)DateTime.Now.Minute + (float)rowTop - 1f;
					if (currentTimeY < (float)rowTop)
					{
						currentTimeY = rowTop;
					}
					Rectangle currentTimeRect = new Rectangle(base.ClientRectangle.Left + 5, (int)currentTimeY, _dayAreaLeft - 10 - base.ClientRectangle.Left, 10);
					BeforePaint(currentTimeRect);
					graphics.FillRectangle(GdiCache.Brush(_currentTimeTopColor), currentTimeRect);
					graphics.DrawLine(GdiCache.Pen(_currentTimeLineColor), currentTimeRect.Left, currentTimeRect.Bottom - 1, currentTimeRect.Right - 1, currentTimeRect.Bottom - 1);
					shouldHighlightCurrentTime = false;
				}
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
				Rectangle rectangle = new Rectangle(base.ClientRectangle.Left + 3, rowTop, timeLabelWidth - amDesignatorWidth + 1, hourRowHeight);
				Brush labelBrush = schedulerFormatStyle.GetForeColorBrush(GetThemedForeColor());
				if (labelBrush != null)
				{
					Label(hourText, rectangle, _baseFont, labelBrush);
				}
				Line(new Point(base.ClientRectangle.Left + lineInset, rowTop - 1), new Point(timeLabelWidth + base.ClientRectangle.Left, rowTop - 1));
				if (lastRowBottom > 0)
				{
					Line(new Point(base.ClientRectangle.Left, lastRowBottom - 1), new Point(_dayAreaLeft - 5 - base.ClientRectangle.Left, lastRowBottom - 1));
				}
				Rectangle designatorRect = new Rectangle(designatorLeft + 1, rowTop, timeLabelWidth - designatorLeft + base.ClientRectangle.Left + 1, _actualRowHeight);
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
				if (labelBrush != null)
				{
					Label(designatorLabel, designatorRect, _timeDesignatorFont, labelBrush);
				}
				int minuteValue = 0;
				for (int i = 1; i < (int)_interval; i++)
				{
					if (_showMinutesInTimeNavigator)
					{
						if (minuteValue > 0)
						{
							designatorRect = new Rectangle(designatorLeft + 1, rowTop, timeLabelWidth - designatorLeft + base.ClientRectangle.Left + 1, _actualRowHeight);
							if (labelBrush != null)
							{
								Label(minuteValue.ToString(), designatorRect, _timeDesignatorFont, labelBrush);
							}
						}
						minuteValue += _minuteInterval;
					}
					rowTop += _actualRowHeight;
					Line(new Point(designatorLeft + 1, rowTop - 1), new Point(timeLabelWidth + base.ClientRectangle.Left, rowTop - 1));
				}
				if (_showMinutesInTimeNavigator)
				{
					if (minuteValue > 0)
					{
						designatorRect = new Rectangle(designatorLeft + 1, rowTop, timeLabelWidth - designatorLeft + base.ClientRectangle.Left + 1, _actualRowHeight);
						if (labelBrush != null)
						{
							Label(minuteValue.ToString(), designatorRect, _timeDesignatorFont, labelBrush);
						}
					}
					minuteValue += _minuteInterval;
				}
				rowTop += _actualRowHeight;
				dateTime = dateTime.AddHours(1.0);
			}
			while (rowTop <= GetClientBottom() - 1);
			labels?.Flush(graphics);
			textLayoutRenderer.ResetFormat();
			schedulerFormatStyle.DisposeResources();
		}

		private void DrawOwnerHeader(Graphics graphics)
		{
			Rectangle rectangle = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, base.ClientRectangle.Width, _actualRowHeight);
			if ((_ownerDrawnArea & OwnerDrawnArea.OwnerHeader) != OwnerDrawnArea.None)
			{
				DrawSchedulerAreaEventArgs e = new DrawSchedulerAreaEventArgs(graphics, this, OwnerDrawnArea.OwnerHeader, rectangle, null);
				RaiseDrawSchedulerArea(e);
				if (e.Handled)
				{
					return;
				}
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateWrapsText(false);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_headersFormatStyle);
			string text = "";
			Rectangle ownerListButtonRect = Rectangle.Empty;
			int imageIndex = -1;
			if (CurrentOwner != null)
			{
				text = _currentOwner.Text;
				imageIndex = _currentOwner.ImageIndex;
			}
			rectangle = DrawHeaderCellBackground(graphics, Date, rectangle, schedulerFormatStyle, false, false);
			Rectangle textRect = rectangle;
			Rectangle imageRect = rectangle;
			textRect.X += 5;
			imageRect.X += 5;
			int textWidth = (int)textLayoutRenderer.RenderContent(graphics, text + " ", GetViewFont(schedulerFormatStyle), 0).Width + 1;
			Image image = null;
			if (imageIndex >= 0)
			{
				try
				{
					ImageList imageList = GetImageList();
					if (imageList != null && imageIndex < imageList.Images.Count)
					{
						image = imageList.Images[imageIndex];
					}
				}
				catch
				{
				}
			}
			if (image != null)
			{
				imageRect.Width = image.Width;
				int imageOffsetY = (imageRect.Height - image.Height) / 2;
				if (imageOffsetY < 0)
				{
					imageOffsetY = 0;
				}
				imageRect.Y += imageOffsetY;
				graphics.DrawImageUnscaled(image, imageRect.Left, imageRect.Top);
				textRect.X = imageRect.Left + image.Width + 5;
				textRect.Width -= imageRect.Left + image.Width + 5;
			}
			if (text.Length > 0)
			{
				textLayoutRenderer.DrawText(graphics, text, textRect, GetViewFont(schedulerFormatStyle), schedulerFormatStyle.ResolveForeColor(GetThemedForeColor()));
			}
			textWidth += textRect.X;
			if (_showOwnerList)
			{
				ownerListButtonRect = new Rectangle(textWidth + 2, base.ClientRectangle.Top + _actualRowHeight / 2 + 2, _actualRowHeight - 4, _actualRowHeight - 4);
				_scrollBarStyle.DrawDownArrow(graphics, ownerListButtonRect);
			}
			_ownerHeaderBounds = new Rectangle(base.ClientRectangle.Left, base.ClientRectangle.Top, textWidth + _actualRowHeight + 7, _actualRowHeight);
			textLayoutRenderer.ResetFormat();
			schedulerFormatStyle.DisposeResources();
		}
	}
}
