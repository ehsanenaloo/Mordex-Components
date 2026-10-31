using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal partial class SchedulerAppointmentView
	{
		private void DrawHorizontalAppointment(Graphics graphics, bool shouldDrawContent, Rectangle rect)
		{
			Brush brush = GetEffectiveFormatStyle().GetBackBrush(rect);
			ResolvedTheme older = _scheduler != null && !_scheduler._isPrinting ? MxThemeManager.ResolveDefault(_scheduler.DeviceDpi) : null;
			if (older != null && older.Style.Detail.ChoiceLook != ThemeChoiceLook.Default && older.Style.AppointmentRadius > 0 && brush is SolidBrush solid && rect.Width > 6 && rect.Height > 6)
			{
				// The month view and the all day row of the older looks: a rounded frame, with a gradient where the look has one.
				if (older.Style.Detail.AppointmentGradient)
				{
					older.FillColorFace(graphics, rect, solid.Color, 0.28f, 0.12f, older.Style.AppointmentRadius, false);
				}
				else
				{
					older.FillRoundedWith(graphics, rect, solid, older.Style.AppointmentRadius);
				}
				older.DrawRoundedWith(graphics, rect, _scheduler.GetAppointmentOutlinePen(), older.Style.AppointmentRadius);
			}
			else if (older != null && older.Style.ButtonLook == ThemeButtonLook.Classic && brush != null)
			{
				graphics.FillRectangle(brush, rect);
				graphics.DrawRectangle(GdiCache.Pen(Color.Black), rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
			}
			else if (brush != null)
			{
				graphics.FillRectangle(brush, rect);
			}
			if (shouldDrawContent)
			{
				DrawHorizontalContent(graphics, rect);
			}
			if (_isMultiDay || _appointmentState == AppointmentState.Focus)
			{
				DrawEdgeLines(graphics, rect);
			}
		}

		internal void DrawAppointmentText(Graphics graphics, string textArgument, Rectangle rectangle, Font font, Color color, bool shouldShowTime)
		{
			if (rectangle.Width <= 0 || rectangle.Height <= 0 || color.IsEmpty)
			{
				_hasHiddenText = true;
				return;
			}
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateWrapsText(true);
			string text = textArgument;
			if (shouldShowTime)
			{
				text = ((_scheduler.TimeFormat != TimeFormat.TwelveHours) ? (_appointment.StartTime.ToString("%H:mm") + "-" + _appointment.EndTime.ToString("%H:mm")) : (_appointment.StartTime.ToShortTimeString() + "-" + _appointment.EndTime.ToShortTimeString()));
				text = text.Trim();
				if (textLayoutRenderer.RenderContent(graphics, text, font, 0).Width > (float)rectangle.Width)
				{
					textLayoutRenderer.UpdateWrapsText(true);
					int height = rectangle.Height;
					rectangle.Height = _rowHeight;
					textLayoutRenderer.DrawText(graphics, text, rectangle, font, color);
					rectangle.Y += _rowHeight;
					rectangle.Height = height - _rowHeight;
					text = textArgument;
				}
				else
				{
					text = text + " " + textArgument;
				}
			}
			if (_appointment.Description != "")
			{
				text = text + "\n" + _appointment.Description;
			}
			else
			{
				textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
			}
			_schedulerRenderer4Text = text;
			textLayoutRenderer.DrawText(graphics, text, rectangle, font, color);
			if (_appointment.Description != "")
			{
				_hasHiddenText = true;
			}
			else
			{
				Size size = textLayoutRenderer.RenderContent(graphics, text, font, rectangle.Width + 1).ToSize();
				if (size.Width > rectangle.Width || size.Height > rectangle.Height)
				{
					_hasHiddenText = true;
				}
				else
				{
					_hasHiddenText = false;
				}
			}
			textLayoutRenderer.ResetFormat();
		}

		private void DrawDayViewAppointment(Graphics graphics, bool shouldDrawContent, Rectangle rectangleArgument)
		{
			int firstImageIndex = SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex1, _appointment.ImageKey1);
			int secondImageIndex = SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex2, _appointment.ImageKey2);
			int thirdImageIndex = SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex3, _appointment.ImageKey3);
			int fourthImageIndex = SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex4, _appointment.ImageKey4);
			Color borderColor = _appointment.BorderColor;
			foreach (SchedulerFormatCondition formatCondition in _scheduler.FormatConditions)
			{
				if (!formatCondition.Enabled || !formatCondition.MatchesAppointment(_appointment))
				{
					continue;
				}
				int conditionFirstImageIndex = SchedulerRenderer.GetImageIndex(_imageList, formatCondition.ImageIndex1, formatCondition.ImageKey1);
				int conditionSecondImageIndex = SchedulerRenderer.GetImageIndex(_imageList, formatCondition.ImageIndex2, formatCondition.ImageKey2);
				int conditionThirdImageIndex = SchedulerRenderer.GetImageIndex(_imageList, formatCondition.ImageIndex3, formatCondition.ImageKey3);
				int conditionFourthImageIndex = SchedulerRenderer.GetImageIndex(_imageList, formatCondition.ImageIndex4, formatCondition.ImageKey4);
				if (!formatCondition.AllowMerge)
				{
					firstImageIndex = ((conditionFirstImageIndex == -1) ? SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex1, _appointment.ImageKey1) : conditionFirstImageIndex);
					secondImageIndex = ((conditionSecondImageIndex == -1) ? SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex2, _appointment.ImageKey2) : conditionSecondImageIndex);
					thirdImageIndex = ((conditionThirdImageIndex == -1) ? SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex3, _appointment.ImageKey3) : conditionThirdImageIndex);
					fourthImageIndex = ((conditionFourthImageIndex == -1) ? SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex4, _appointment.ImageKey4) : conditionFourthImageIndex);
				}
				else
				{
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
				if (!formatCondition.BorderColor.IsEmpty)
				{
					borderColor = formatCondition.BorderColor;
				}
			}
			_textStartX = rectangleArgument.Left + 6;
			Rectangle rect;
			Brush brush;
			if (_focusBounds != Rectangle.Empty)
			{
				graphics.DrawRectangle(_borderPen, _focusBounds.Left, _focusBounds.Top, _focusBounds.Width - 1, _focusBounds.Height - 1);
				rect = _focusBounds;
				rect.Inflate(-1, -1);
				brush = GetBorderBrush(borderColor);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rect);
				}
			}
			else
			{
			}
			Rectangle bodyBounds = _bodyBounds;
			if (bodyBounds == Rectangle.Empty || (_scheduler != null && _scheduler._isPrinting))
			{
				bodyBounds = new Rectangle(rectangleArgument.Left + 5, rectangleArgument.Top, rectangleArgument.Width - 5, rectangleArgument.Height);
			}
			ResolvedTheme look = _scheduler != null && !_scheduler._isPrinting ? MxThemeManager.ResolveDefault(_scheduler.DeviceDpi) : null;
			int frameRadius = look != null ? look.Style.AppointmentRadius : 0;
			rect = bodyBounds;
			rect.Inflate(-1, -1);
			brush = GetEffectiveFormatStyle().GetBackBrush(rect);
			if (frameRadius > 0 && brush is SolidBrush glossBody && look.Style.Detail.AppointmentGradient)
			{
				look.FillColorFace(graphics, bodyBounds, glossBody.Color, 0.28f, 0.12f, frameRadius, false);
				look.DrawRoundedWith(graphics, bodyBounds, _borderPen, frameRadius);
			}
			else if (frameRadius > 0 && brush is SolidBrush solidBody)
			{
				if (look.Style.Modern.FaceKind == ThemeFaceKind.Glass)
				{
					look.FillColorFace(graphics, bodyBounds, solidBody.Color, 0.28f, 0.12f, frameRadius, false);
				}
				else
				{
					look.FillRoundedWith(graphics, bodyBounds, solidBody, frameRadius);
				}
				look.DrawRoundedWith(graphics, bodyBounds, _borderPen, frameRadius);
			}
			else
			{
				graphics.DrawRectangle(_borderPen, bodyBounds.Left, bodyBounds.Top, bodyBounds.Width - 1, bodyBounds.Height - 1);
				if (brush != null)
				{
					graphics.FillRectangle(brush, rect);
				}
			}
			DrawBackground(graphics, rectangleArgument, borderColor, !_appointment._isBeingMoved, _borderPen);
			if (look != null && look.Style.ButtonLook == ThemeButtonLook.Classic)
			{
				graphics.DrawRectangle(GdiCache.Pen(Color.Black), bodyBounds.Left, bodyBounds.Top, bodyBounds.Width - 1, bodyBounds.Height - 1);
			}
			if (shouldDrawContent)
			{
				DrawDayViewContent(graphics, firstImageIndex, secondImageIndex, thirdImageIndex, fourthImageIndex, rect);
			}
		}

		public void DrawAppointment(Graphics graphics)
		{
			DrawAppointmentInBounds(graphics, true, _bounds);
		}

		public void DrawAppointmentInBounds(Graphics graphics, bool shouldDrawContent, Rectangle rectangle)
		{
			_borderPen = _scheduler.GetAppointmentOutlinePen();
			if (_view != SchedulerView.DayView || _allDay)
			{
				DrawHorizontalAppointment(graphics, shouldDrawContent, rectangle);
			}
			else
			{
				DrawDayViewAppointment(graphics, shouldDrawContent, rectangle);
			}
			DisposeResources();
		}
	}
}
