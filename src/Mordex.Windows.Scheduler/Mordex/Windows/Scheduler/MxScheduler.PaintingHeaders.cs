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
		private void DrawDayCellHeader(Graphics graphics, Rectangle rect, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState, string text, TextLayoutRenderer textLayoutRenderer)
		{
			Brush brush = null;
			SchedulerFormatStyle schedulerFormatStyle = null;
			Font font = null;
			if (_view == SchedulerView.MonthView)
			{
				schedulerFormatStyle = new SchedulerFormatStyle(_monthDaysFormatStyle);
				if (schedulerHourRangeCollectionLayoutState.GetDate().Month % 2 == 0)
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(_alternatingMonthDaysFormatStyle);
				}
				if (schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle() != null)
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle());
				}
				if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
				{
					if (_hideSelection == HideSelection.HighlightInactive)
					{
						schedulerFormatStyle = schedulerFormatStyle.MergeWith(_selectedInactiveFormatStyle);
					}
				}
				else
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(_selectedFormatStyle);
				}
			}
			else
			{
				schedulerFormatStyle = new SchedulerFormatStyle(_headersFormatStyle);
				if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
				{
					if (_hideSelection == HideSelection.HighlightInactive)
					{
						schedulerFormatStyle = schedulerFormatStyle.MergeWith(_selectedInactiveFormatStyle);
					}
				}
				else
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(_selectedFormatStyle);
				}
			}
			brush = ((_view != SchedulerView.WeekView) ? schedulerFormatStyle.GetBackBrush(rect) : schedulerFormatStyle.GetBackBrush(rect));
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
			font = GetViewFont(schedulerFormatStyle);
			textLayoutRenderer.DrawText(graphics, text, rect, font, schedulerFormatStyle.ResolveForeColor(GetThemedForeColor()));
			schedulerFormatStyle.DisposeResources();
		}

		internal void DrawAllDayAreaBackground(Graphics graphics, Rectangle rect)
		{
			UpdateThemedRendering();
			// A fill set by the developer on AllDayAreaBackgroundStyle wins over the theme.
			using (Brush developerFill = _allDayAreaBackgroundStyle.CreateDeveloperBackBrush(rect))
			{
				graphics.FillRectangle(developerFill ?? GdiCache.Brush(_themeRenderer._allDayAreaColor), rect);
			}
		}

		private Rectangle DrawHeaderCellBackground(Graphics graphics, DateTime dateTime, Rectangle rectArgument, SchedulerFormatStyle schedulerFormatStyle, bool hot, bool canHighlightToday)
		{
			if (rectArgument.Right > base.ClientRectangle.Right && !_isPrinting)
			{
				rectArgument.Width -= rectArgument.Right - base.ClientRectangle.Right;
			}
			if (rectArgument.Width <= 0)
			{
				return rectArgument;
			}
			bool isTodayHighlighted = false;
			if (canHighlightToday && dateTime == DateTime.Now.Date && (_highlightCurrentTime == TriState.True || _highlightCurrentTime == TriState.Empty))
			{
				isTodayHighlighted = true;
			}
			UpdateThemedRendering();
			BorderFlags borderFlags = GetHeaderBorderFlags(rectArgument);
			// A fill color that the developer set on the header style wins over the theme. The style passed in is a copy that
			// holds the theme color as its own, so the header style itself tells whether the developer set one.
			using (Brush developerFill = _headersFormatStyle.CreateDeveloperBackBrush(rectArgument))
			{
				if (canHighlightToday && _view == SchedulerView.WeekView && IsDateSelected(dateTime))
				{
					_themeRenderer.DrawBackground(graphics, rectArgument, hot, true, borderFlags, isTodayHighlighted, developerFill);
				}
				else
				{
					rectArgument = _themeRenderer.DrawBackground(graphics, rectArgument, hot, false, borderFlags, isTodayHighlighted, developerFill);
				}
			}
			return rectArgument;
		}

		internal void DrawVerticalHeader(Graphics graphicsArgument, SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState, bool shouldDrawText)
		{
			bool ownsGraphics = false;
			if (graphicsArgument == null)
			{
				graphicsArgument = CreateGraphics();
				ownsGraphics = true;
			}
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_headersFormatStyle);
			Rectangle rectangle = new Rectangle(0, 0, schedulerAppointmentOwnerLayoutState._bounds.Height, schedulerAppointmentOwnerLayoutState._bounds.Width);
			if (rectangle.Width <= 0 || rectangle.Height <= 0)
			{
				return;
			}
			Bitmap bitmap = new Bitmap(rectangle.Width, rectangle.Height);
			Graphics graphics = Graphics.FromImage(bitmap);
			BorderFlags borderFlags = (BorderFlags)13;
			if (GetEffectiveBorderStyle() == BorderStyle.None)
			{
				borderFlags |= BorderFlags.Top;
			}
			DrawHeaderCellBackground(graphics, schedulerAppointmentOwnerLayoutState.GetDate(), rectangle, schedulerFormatStyle, schedulerAppointmentOwnerLayoutState.IsHovered(), false);
			if (shouldDrawText)
			{
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(useTextRenderer: false);
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
				if (schedulerAppointmentOwnerLayoutState.GetHeaderText() != null && schedulerAppointmentOwnerLayoutState.GetHeaderText().Length > 0)
				{
					textLayoutRenderer.DrawText(graphics, schedulerAppointmentOwnerLayoutState.GetHeaderText(), rectangle, GetViewFont(schedulerFormatStyle), schedulerFormatStyle.ResolveForeColor(GetThemedForeColor()));
				}
			}
			bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
			graphicsArgument.DrawImageUnscaled(bitmap, schedulerAppointmentOwnerLayoutState._bounds.X, schedulerAppointmentOwnerLayoutState._bounds.Y);
			bitmap.Dispose();
			graphics.Dispose();
			schedulerFormatStyle.DisposeResources();
			if (ownsGraphics)
			{
				graphicsArgument.Dispose();
				graphicsArgument = null;
			}
		}

		internal void DrawHeaderText(Graphics graphics, SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState, Rectangle rectangle)
		{
			bool ownsGraphics = false;
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_headersFormatStyle);
			if (schedulerFormatStyle.ForeColor.IsEmpty)
			{
				schedulerFormatStyle.ForeColor = GetThemedForeColor();
			}
			DrawHeaderContent(graphics, schedulerAppointmentOwnerLayoutState, schedulerAppointmentOwnerLayoutState.GetHeaderText(), rectangle, GetViewFont(schedulerFormatStyle), schedulerFormatStyle);
			schedulerFormatStyle.DisposeResources();
			if (ownsGraphics)
			{
				graphics.Dispose();
				graphics = null;
			}
		}

		internal void DrawDateHeader(Graphics graphics, SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState, bool isHot, Rectangle rectangle, bool booleanArgument)
		{
			bool ownsGraphics = false;
			if (graphics == null)
			{
				graphics = CreateGraphics();
				ownsGraphics = true;
			}
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_headersFormatStyle);
			rectangle = DrawHeaderCellBackground(graphics, schedulerAppointmentOwnerLayoutState.GetDate(), rectangle, schedulerFormatStyle, isHot, booleanArgument);
			if (rectangle.Width > 0 && rectangle.Height > 0)
			{
				DrawHeaderContent(graphics, schedulerAppointmentOwnerLayoutState, schedulerAppointmentOwnerLayoutState.GetHeaderText(), rectangle, GetViewFont(schedulerFormatStyle), schedulerFormatStyle);
				schedulerFormatStyle.DisposeResources();
				if (ownsGraphics)
				{
					graphics.Dispose();
					graphics = null;
				}
			}
		}

		private void DrawHeaderContent(Graphics graphics, SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState, string text, Rectangle rectangle, Font font, SchedulerFormatStyle schedulerFormatStyle)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!UseCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			if (_view == SchedulerView.WeekView && IsDateSelected(schedulerAppointmentOwnerLayoutState.GetDate()))
			{
				DrawDayCellHeader(graphics, rectangle, null, text, textLayoutRenderer);
			}
			else
			{
				Image image = null;
				if (schedulerAppointmentOwnerLayoutState.GetImageIndex() >= 0)
				{
					try
					{
						ImageList imageList = GetImageList();
						if (imageList != null && schedulerAppointmentOwnerLayoutState.GetImageIndex() < imageList.Images.Count)
						{
							image = imageList.Images[schedulerAppointmentOwnerLayoutState.GetImageIndex()];
						}
					}
					catch
					{
					}
				}
				if (image != null)
				{
					int textWidth = (int)textLayoutRenderer.RenderContent(graphics, text, font, 0).Width + 2;
					Rectangle rect = rectangle;
					rect.Width = image.Width;
					int imageLeft = rectangle.Left + (rectangle.Width - textWidth) / 2 - 6;
					if (imageLeft < rectangle.Left + 4)
					{
						rect.X += 4;
					}
					else
					{
						rect.X = imageLeft;
					}
					rect.Y += (rectangle.Height - image.Height) / 2 + 1;
					graphics.DrawImageUnscaled(image, rect);
					rectangle.Width = rectangle.Right - rect.Right;
					rectangle.X = rect.Right + 2;
					textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
				}
				if (text != null && text.Length > 0)
				{
					textLayoutRenderer.DrawText(graphics, text, rectangle, font, schedulerFormatStyle.ResolveForeColor(GetThemedForeColor()));
				}
			}
			textLayoutRenderer.ResetFormat();
		}

		private void DrawDateHeaders(Graphics graphics)
		{
			foreach (SchedulerAppointmentOwnerLayoutState item in _headerLayouts)
			{
				if (!IsAreaDrawnByHandler(graphics, OwnerDrawnArea.DateHeader, item._bounds, item))
				{
					DrawDateHeader(graphics, item, false, item._bounds, true);
				}
			}
		}

		private void DrawHourCell(Graphics graphics, SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState, Pen pen, Pen penArgument, SchedulerFormatStyle schedulerFormatStyleArgument, SchedulerFormatStyle alternatingHourStyle, SchedulerFormatStyle workingHourStyle, SchedulerFormatStyle alternatingWorkingHourStyle)
		{
			SchedulerAppointmentOwner schedulerAppointmentOwner = schedulerAppointmentOwnerLayoutState.ResolveOwner();
			SchedulerFormatStyle schedulerFormatStyle = GetTimeSlotStyle(schedulerAppointmentOwnerLayoutState, schedulerFormatStyleArgument, alternatingHourStyle, workingHourStyle, alternatingWorkingHourStyle);
			if (_schedulerSelectedAppointments.Count == 0 && (!IsMultiOwnerActive() || schedulerAppointmentOwner == CurrentOwner) && IsTimeSlotSelected(schedulerAppointmentOwnerLayoutState.GetDate()))
			{
				if (!base.ContainsFocus && _hideSelection != HideSelection.Highlight)
				{
					if (_hideSelection == HideSelection.HighlightInactive)
					{
						schedulerFormatStyle = schedulerFormatStyle.MergeWith(_selectedInactiveFormatStyle);
					}
				}
				else
				{
					schedulerFormatStyle = schedulerFormatStyle.MergeWith(_selectedFormatStyle);
				}
			}
			Brush brush = schedulerFormatStyle.GetBackBrush(schedulerAppointmentOwnerLayoutState._bounds);
			if (brush != null)
			{
				graphics.FillRectangle(brush, schedulerAppointmentOwnerLayoutState._bounds);
			}
			schedulerFormatStyle.DisposeResources();
			Rectangle bounds = schedulerAppointmentOwnerLayoutState._bounds;
			TimeSpan timeSpan = new TimeSpan(schedulerAppointmentOwnerLayoutState.GetDate().Hour, schedulerAppointmentOwnerLayoutState.GetDate().Minute + _minuteInterval, 0);
			if ((double)(int)timeSpan.TotalHours == timeSpan.TotalHours)
			{
				graphics.DrawLine(pen, bounds.Left + 1, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
			}
			else
			{
				graphics.DrawLine(penArgument, bounds.Left + 1, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
			}
		}

		private void DrawWeekDayBackground(Graphics graphics, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState, Rectangle rect)
		{
			Pen pen = null;
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_weekDaysBackgroundStyle);
			pen = GetGridLinePen();
			if (schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle() != null)
			{
				schedulerFormatStyle = schedulerFormatStyle.MergeWith(schedulerHourRangeCollectionLayoutState.GetSchedulerFormatStyle());
			}
			Brush brush = schedulerFormatStyle.GetBackBrush(rect);
			if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
			if (rect.Right - 1 == base.ClientRectangle.Right && GetVerticalScrollBarControl() != null && GetVerticalScrollBarControl().Visible)
			{
				graphics.DrawLine(pen, rect.Right - 2, rect.Top, rect.Right - 2, rect.Bottom - 1);
			}
			else
			{
				graphics.DrawLine(pen, rect.Right - 1, rect.Top, rect.Right - 1, rect.Bottom - 1);
			}
			graphics.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
			schedulerFormatStyle.DisposeResources();
		}

	}
}
