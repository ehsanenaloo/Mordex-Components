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
		internal void DrawControlBackground(Graphics graphics, Rectangle rectangle, Rectangle rectangleArgument, Image image, Rectangle clipRect)
		{
			Rectangle rect = Rectangle.Intersect(clipRect, rectangle);
			if (rect.Width == 0 || rect.Height == 0)
			{
				return;
			}
			Brush brush = GdiCache.Brush(GetThemedBackColor());
			graphics.FillRectangle(brush, rect);
			rect = Rectangle.Intersect(clipRect, rectangleArgument);
			if (rect.Width == 0 || rect.Height == 0)
			{
				return;
			}
			if (image != null)
			{
				switch (_backgroundImageDrawMode)
				{
				case ImageDrawMode.Centered:
					SchedulerRenderer.DrawImageCentered(graphics, image, rectangleArgument, rect);
					break;
				case ImageDrawMode.Stretched:
					SchedulerRenderer.DrawImageInRect(graphics, image, rectangleArgument, rect);
					break;
				case ImageDrawMode.Tiled:
					SchedulerRenderer.DrawImage(graphics, image, rectangleArgument, rect);
					break;
				}
			}
			if (WatermarkImage.Image != null)
			{
				_watermarkImage.DrawImage(graphics, rectangleArgument);
			}
		}

		private void RenderContent(Graphics graphics, TextLayoutRenderer textLayoutRenderer, Font font, bool useShortFormat)
		{
			if (_weekHeaderLayouts == null)
			{
				return;
			}
			foreach (SchedulerAppointmentOwnerLayoutState item in _weekHeaderLayouts)
			{
				DateTime start = item._dateRange.Start;
				DateTime end = item._dateRange.End;
				if (!useShortFormat)
				{
					string text = start.ToString("MMM dd");
					string rangeText = string.Concat(str2: (end.Month != start.Month) ? end.ToString("MMM dd") : end.ToString("dd"), str0: text, str1: "-");
					int textWidth = (int)textLayoutRenderer.RenderContent(graphics, rangeText, MonthFont, 0).Width + 1;
					if (textWidth > item._bounds.Height)
					{
						RenderContent(graphics, textLayoutRenderer, font, true);
						break;
					}
					item.SetHeaderText(rangeText);
				}
				else
				{
					string shortStartText = start.ToString("M dd");
					item.SetHeaderText(string.Concat(str2: (end.Month != start.Month) ? end.ToString("M dd") : end.ToString("dd"), str0: shortStartText, str1: "-"));
				}
			}
		}

		internal void RenderCurrentView(Graphics graphics)
		{
			if (base.Created)
			{
				bool ownsGraphics = false;
				if (graphics == null)
				{
					graphics = CreateGraphics();
					ownsGraphics = true;
				}
				if (_view == SchedulerView.DayView)
				{
					RenderDayView(graphics);
				}
				else if (_view == SchedulerView.MonthView)
				{
					RenderMonthView(graphics);
				}
				else
				{
					RenderWeekView(graphics);
				}
				if (ownsGraphics)
				{
					graphics.Dispose();
					graphics = null;
				}
			}
		}

		internal void DrawArrowGlyph(Graphics graphics, Rectangle rectangle, int direction)
		{
			Color baseColor = GetHeaderForeColor();
			if (_themeRenderer != null)
			{
				baseColor = _themeRenderer._glyphColor;
			}
			int arrowSize = rectangle.Width / 4;
			int arrowHeight = arrowSize * 2 - 1;
			int arrowX = ((direction <= 0) ? (rectangle.Left + (rectangle.Width - arrowSize) / 2 + arrowSize - 1) : (rectangle.Left + (rectangle.Width - arrowSize) / 2));
			int arrowY = rectangle.Top + (rectangle.Height - arrowHeight) / 2;
			Pen pen = null;
			Rectangle rect = new Rectangle(arrowX - arrowSize / 2, arrowY - arrowHeight / 2, arrowSize * 2, arrowHeight * 2);
			Color color = Color.FromArgb(80, baseColor);
			Color endColor = Color.FromArgb(245, baseColor);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, endColor, LinearGradientMode.ForwardDiagonal);
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

		internal void DrawNavigationArrow(Graphics graphics, Rectangle rect, bool isLeftArrow, bool hot, bool highLight)
		{
			UpdateThemedRendering();
			_themeRenderer.DrawBackground(graphics, rect, highLight, hot, BorderFlags.Bottom, false);
			Bitmap arrowBitmap = isLeftArrow ? SchedulerImageResources.GetTransparentLeftArrowBitmap() : SchedulerImageResources.GetTransparentRightArrowBitmap();
			if (arrowBitmap != null)
			{
				_themeRenderer.DrawImage(graphics, arrowBitmap, rect, GetHeaderForeColor());
			}
		}

		private void DrawBackground(Graphics graphics, SchedulerHourRangeCollectionLayoutState schedulerHourRangeCollectionLayoutState, Brush brushArgument, Rectangle rectArgument)
		{
			Pen pen = GetGridLinePen();
			if (brushArgument != null)
			{
				graphics.FillRectangle(brushArgument, rectArgument);
			}
			if (rectArgument.Right - 1 == base.ClientRectangle.Right && GetVerticalScrollBarControl() != null && GetVerticalScrollBarControl().Visible)
			{
				graphics.DrawLine(pen, rectArgument.Right - 2, rectArgument.Top, rectArgument.Right - 2, rectArgument.Bottom - 1);
			}
			else
			{
				graphics.DrawLine(pen, rectArgument.Right - 1, rectArgument.Top, rectArgument.Right - 1, rectArgument.Bottom - 1);
			}
			graphics.DrawLine(pen, rectArgument.Left, rectArgument.Bottom - 1, rectArgument.Right - 1, rectArgument.Bottom - 1);
			if (schedulerHourRangeCollectionLayoutState.GetDate() == DateTime.Today && (_highlightCurrentTime == TriState.True || HighlightCurrentTime == TriState.Empty))
			{
				Rectangle rect = rectArgument;
				rect.Height = _actualRowHeight;
				rect.Inflate(-1, -1);
				rect.Height++;
				rect.Y--;
				rect.Width--;
				graphics.FillRectangle(GdiCache.Brush(_currentTimeTopColor), rect);
				graphics.DrawLine(GdiCache.Pen(_currentTimeLineColor), rect.Left, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (!base.Created)
			{
				base.OnPaint(e);
				return;
			}
			RefreshLayoutIfDirty(e.Graphics);
			UpdateThemedRendering();
			Region region = null;
			Region clip = null;
			DrawSchedulerAreaEventArgs backgroundArgs = new DrawSchedulerAreaEventArgs(e.Graphics, this, OwnerDrawnArea.ControlBackground, base.ClientRectangle, null);
			RaiseDrawSchedulerArea(backgroundArgs);
			if (!backgroundArgs.Handled)
			{
				DrawControlBackground(e.Graphics, base.ClientRectangle, _gridBounds, BackgroundImage, e.ClipRectangle);
			}
			if (_view == SchedulerView.MonthView)
			{
				DrawMonthView(e.Graphics, e.ClipRectangle);
				DrawDateHeaders(e.Graphics);
			}
			else if (_view == SchedulerView.WeekView)
			{
				DrawWeekView(e.Graphics, e.ClipRectangle);
			}
			else
			{
				int lastRowBottom = 0;
				if (!_showAllDayArea && _dayLayouts.Count > 0)
				{
					int lastRowIndex = DayEndHour * (int)_interval - 1;
					lastRowBottom = _dayLayouts.GetLayoutAt(0).GetTimeSlots().GetLayoutAt(lastRowIndex)
						._bounds.Bottom;
					if (base.ClientRectangle.Bottom > lastRowBottom)
					{
						Rectangle clientRectangle = base.ClientRectangle;
						clientRectangle.Height = lastRowBottom - base.ClientRectangle.Top;
						region = new Region(clientRectangle);
						clip = e.Graphics.Clip;
						e.Graphics.Clip = region;
					}
					else
					{
						lastRowBottom = 0;
					}
				}
				DrawDayViewCells(e.Graphics, e.ClipRectangle);
				DrawTimeNavigator(e.Graphics, lastRowBottom);
				DrawAppointmentStrips(e.Graphics, e.ClipRectangle);
			}
			DrawAppointmentViews(e);
			if (region != null)
			{
				region.Dispose();
				region = null;
				e.Graphics.Clip = clip;
			}
			DrawBorder(e.Graphics, _themeRenderer);
			DisposeDrawingResources();
			base.OnPaint(e);
		}

		internal void DrawAreaDefault(Graphics graphics, OwnerDrawnArea ownerDrawnArea, object value, Rectangle rect)
		{
			switch (ownerDrawnArea)
			{
			case OwnerDrawnArea.WeekHeader:
				if (value is SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutState)
				{
					DrawVerticalHeader(graphics, schedulerAppointmentOwnerLayoutState, false);
				}
				break;
			case OwnerDrawnArea.Appointments:
				if (value is SchedulerAppointment schedulerAppointment)
				{
					FindAppointmentView(schedulerAppointment)?.DrawAppointmentInBounds(graphics, false, rect);
				}
				break;
			case OwnerDrawnArea.DayViewRow:
				if (value is SchedulerAppointmentOwnerLayoutState schedulerAppointmentOwnerLayoutStateArgument)
				{
					SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(_hourBackgroundStyle);
					SchedulerFormatStyle alternatingHourStyle = schedulerFormatStyle.MergeWith(_alternatingHourBackgroundStyle);
					SchedulerFormatStyle workingHourStyle = schedulerFormatStyle.MergeWith(_workingHourBackgroundStyle);
					SchedulerFormatStyle alternatingWorkingHourStyle = workingHourStyle.MergeWith(_alternatingWorkingHourBackgroundStyle);
					DrawHourCell(graphics, schedulerAppointmentOwnerLayoutStateArgument, GetGridLinePen(), GetLightGridLinePen(), schedulerFormatStyle, alternatingHourStyle, workingHourStyle, alternatingWorkingHourStyle);
				}
				break;
			case OwnerDrawnArea.TimeNavigator:
				DrawTimeNavigatorBackground(graphics);
				break;
			case OwnerDrawnArea.ControlBackground:
				DrawControlBackground(graphics, base.ClientRectangle, _gridBounds, BackgroundImage, base.ClientRectangle);
				break;
			case OwnerDrawnArea.AllDayArea:
				if (_allDayAreaControl != null)
				{
					_allDayAreaControl.DrawAreaBackground(graphics, _allDayAreaControl.ClientRectangle);
				}
				break;
			case OwnerDrawnArea.MonthViewDay:
				if (value is SchedulerHourRangeCollectionLayoutState)
				{
					Brush brush = GetAreaBackgroundBrush(ownerDrawnArea, value, rect);
					DrawBackground(graphics, value as SchedulerHourRangeCollectionLayoutState, brush, rect);
					if (brush != null)
					{
						brush.Dispose();
						brush = null;
					}
				}
				break;
			case OwnerDrawnArea.WeekViewDay:
				if (value is SchedulerHourRangeCollectionLayoutState)
				{
					DrawWeekDayBackground(graphics, value as SchedulerHourRangeCollectionLayoutState, rect);
				}
				break;
			case OwnerDrawnArea.DateHeader:
				if (value is SchedulerAppointmentOwnerLayoutState dateHeaderLayout)
				{
					DrawHeaderCellBackground(graphics, dateHeaderLayout.GetDate(), rect, GetAreaFormatStyle(ownerDrawnArea, value), dateHeaderLayout.IsHovered(), true);
				}
				break;
			case OwnerDrawnArea.OwnerHeader:
			case OwnerDrawnArea.ScrollDayButton:
				DrawHeaderCellBackground(graphics, Date, rect, GetAreaFormatStyle(ownerDrawnArea, value), false, false);
				break;
			case OwnerDrawnArea.OwnerListItem:
				graphics.FillRectangle(SystemBrushes.Menu, rect);
				break;
			}
		}
	}
}
