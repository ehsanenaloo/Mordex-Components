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
		private void DrawRadialLine(Graphics graphics, Pen pen, PointF pointF, int angleDegrees, float floatValue, float floatValueArgument)
		{
			float x = pointF.X + floatValueArgument * (float)Math.Cos((double)angleDegrees * Math.PI / 180.0);
			float y = pointF.Y + floatValueArgument * (float)Math.Sin((double)angleDegrees * Math.PI / 180.0);
			PointF pt = new PointF(x, y);
			x = pointF.X + floatValue * (float)Math.Cos((double)angleDegrees * Math.PI / 180.0);
			y = pointF.Y + floatValue * (float)Math.Sin((double)angleDegrees * Math.PI / 180.0);
			PointF innerPoint = new PointF(x, y);
			graphics.DrawLine(pen, pt, innerPoint);
		}

		private void DrawClock(Graphics graphics, bool isEndTime, Rectangle rectangle, bool booleanArgument)
		{
			float x = rectangle.Left;
			Pen pen = _borderPen;
			Brush brush = null;
			float clockSize = 14f;
			float y = ((!booleanArgument) ? ((float)rectangle.Top) : ((float)rectangle.Top + (float)((int)((float)rectangle.Height - clockSize) / 2)));
			bool isAfternoon = false;
			float hourValue;
			int minutes;
			if (isEndTime)
			{
				x = (float)rectangle.Right - clockSize - 1f;
				hourValue = (float)_appointment.EndTime.TimeOfDay.TotalHours;
				minutes = _appointment.EndTime.TimeOfDay.Minutes;
				if (_appointment.EndTime.TimeOfDay.TotalHours >= 12.0)
				{
					pen = SystemPens.ControlLightLight;
					brush = GdiCache.Brush(_borderPen.Color);
					isAfternoon = true;
				}
			}
			else
			{
				hourValue = _appointment.StartTime.TimeOfDay.Hours;
				minutes = _appointment.StartTime.TimeOfDay.Minutes;
				if (_appointment.StartTime.TimeOfDay.TotalHours >= 12.0)
				{
					pen = SystemPens.ControlLightLight;
					brush = GdiCache.Brush(_borderPen.Color);
					isAfternoon = true;
				}
			}
			if (hourValue > 12f)
			{
				hourValue -= 12f;
			}
			hourValue -= 3f;
			minutes -= 15;
			RectangleF rect = new RectangleF(x, y, clockSize, clockSize);
			float radius = rect.Width / 2f;
			PointF pointF = new PointF(rect.Left + rect.Width / 2f, rect.Top + rect.Height / 2f);
			if (_scheduler.ClockStyle == ClockStyle.Flat)
			{
				graphics.DrawEllipse(_borderPen, rect);
				if (brush != null)
				{
					graphics.FillEllipse(brush, rect);
				}
				for (int i = 0; i <= 360; i += 30)
				{
					DrawRadialLine(graphics, pen, pointF, i, radius - 1f, radius);
				}
			}
			else
			{
				pen = SystemPens.ControlDarkDark;
				Bitmap bitmap = null;
				bitmap = ((!isAfternoon) ? SchedulerRenderer.MakeMagentaTransparent(SchedulerImageResources.GetDayClockBitmap()) : SchedulerRenderer.MakeMagentaTransparent(SchedulerImageResources.GetNightClockBitmap()));
				graphics.DrawImageUnscaled(bitmap, (int)rect.X, (int)rect.Y);
			}
			float hourAngle = ((!(hourValue < 0f)) ? (hourValue * 30f) : (360f + hourValue * 30f));
			int minuteAngle = ((minutes >= 0) ? (minutes * 6) : (360 + minutes * 6));
			DrawRadialLine(graphics, pen, pointF, (int)hourAngle, 0f, radius / 2f - 1f);
			DrawRadialLine(graphics, pen, pointF, minuteAngle, 0f, radius - 2f);
		}

		private void DrawHorizontalContent(Graphics graphics, Rectangle rect)
		{
			Region region = null;
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateWrapsText(false);
			int firstImageIndex = SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex1, _appointment.ImageKey1);
			int secondImageIndex = SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex2, _appointment.ImageKey2);
			int thirdImageIndex = SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex3, _appointment.ImageKey3);
			int fourthImageIndex = SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex4, _appointment.ImageKey4);
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
				if (formatCondition.AllowMerge)
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
				else
				{
					firstImageIndex = ((conditionFirstImageIndex == -1) ? SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex1, _appointment.ImageKey1) : conditionFirstImageIndex);
					secondImageIndex = ((conditionSecondImageIndex == -1) ? SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex2, _appointment.ImageKey2) : conditionSecondImageIndex);
					thirdImageIndex = ((conditionThirdImageIndex == -1) ? SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex3, _appointment.ImageKey3) : conditionThirdImageIndex);
					fourthImageIndex = ((conditionFourthImageIndex == -1) ? SchedulerRenderer.GetImageIndex(_imageList, _appointment.ImageIndex4, _appointment.ImageKey4) : conditionFourthImageIndex);
				}
			}
			if (_showTimeAsClocks || _isMultiDay)
			{
				if (_scheduler.ClockStyle != ClockStyle.None)
				{
					_timeAreaBounds = Rectangle.Empty;
					if (_isMultiDay)
					{
						if (_isFirstSegment)
						{
							DrawClock(graphics, false, rect, true);
							rect.X += 15;
							rect.Width -= 15;
						}
						if (_isLastSegment)
						{
							DrawClock(graphics, true, rect, true);
							rect.Width -= 15;
						}
					}
					else
					{
						_timeAreaBounds = DrawStartEndClocks(graphics, _bounds, true);
						if (_timeAreaBounds != Rectangle.Empty)
						{
							int right = rect.Right;
							rect.X = _timeAreaBounds.Right + 2;
							rect.Width = right - rect.X;
						}
						textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
					}
				}
			}
			else
			{
				rect = DrawTimeRange(graphics, rect);
				_timeAreaBounds = _bounds;
				_timeAreaBounds.Width = rect.Left;
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			}
			Region clip = graphics.Clip;
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			if (_appointment.Duration.TotalDays >= 1.0)
			{
				textLayoutRenderer.UpdateStringAlignment(_scheduler.AllDayEventsAlignment);
			}
			if (HasAnyImage(firstImageIndex, secondImageIndex, thirdImageIndex, fourthImageIndex))
			{
				_textStartX = rect.Left;
				int left = rect.Left;
				region = (graphics.Clip = new Region(rect));
				DrawImage(graphics, rect.Width, rect.Left, rect.Top, rect.Height, _appointment, firstImageIndex, secondImageIndex, thirdImageIndex, fourthImageIndex, true);
				rect.X = _textStartX;
				rect.Width -= rect.X - left;
				if (_scheduler.AllDayEventsAlignment == StringAlignment.Far)
				{
					rect.Width = _textAreaWidth;
				}
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			}
			string text = "";
			text = ((GetAppointment().Prefix != "") ? (GetAppointment().Prefix + " " + GetAppointment().Text) : ((!(_scheduler.AppointmentsPrefix != "")) ? GetAppointment().Text : (_scheduler.AppointmentsPrefix + " " + GetAppointment().Text)));
			if (_appointment.Description != "")
			{
				_hasHiddenText = true;
			}
			else
			{
				int width = textLayoutRenderer.RenderContent(graphics, text, GetEffectiveFormatStyle().ResolveFont(_appointmentFont), 0).ToSize().Width;
				if (width > rect.Width)
				{
					_hasHiddenText = true;
				}
				else
				{
					_hasHiddenText = false;
				}
			}
			if (text.Length > 0 && rect.Width > 0)
			{
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
				textLayoutRenderer.UpdateStringTrimming(StringTrimming.EllipsisCharacter);
				textLayoutRenderer.DrawText(graphics, text, rect, GetEffectiveFormatStyle().ResolveFont(_appointmentFont), GetEffectiveFormatStyle().ResolveForeColor(_scheduler.GetThemedForeColor()));
			}
			textLayoutRenderer.ResetFormat();
			if (region != null)
			{
				region.Dispose();
				region = null;
				graphics.Clip = clip;
			}
		}

		internal void DrawBackground(Graphics graphics, Rectangle schedulerRenderer4Rectangle, Color color, bool shouldFillDuration, Pen pen)
		{
			RectangleF rectangleF = Rectangle.Empty;
			try
			{
				_schedulerRenderer4Rectangle = schedulerRenderer4Rectangle;
				_schedulerRenderer4Rectangle.Width = 6;
				if (_appointmentState == AppointmentState.Focus)
				{
					_schedulerRenderer4Rectangle.Y += 5;
					_schedulerRenderer4Rectangle.Height -= 10;
				}
				int startMinutes = (int)_appointment.GetStartTimeOnDate(_date).TotalMinutes;
				int durationMinutes = (int)_appointment.GetDurationOnDate(_date).TotalMinutes;
				graphics.FillRectangle(GetBarBackgroundBrush(), _schedulerRenderer4Rectangle);
				int visibleMinutes = (int)(_schedulerRenderer4TimeSpan.TotalMinutes - _timeSpan.TotalMinutes);
				int startOffsetMinutes = (int)((double)startMinutes - _timeSpan.TotalMinutes);
				float pixelsPerMinute = (float)_schedulerRenderer4Rectangle.Height / (float)visibleMinutes;
				float fillTop = (float)startOffsetMinutes * pixelsPerMinute + (float)_schedulerRenderer4Rectangle.Top;
				float fillHeight = pixelsPerMinute * (float)durationMinutes;
				if (fillHeight + fillTop > (float)_schedulerRenderer4Rectangle.Bottom)
				{
					fillHeight = (float)_schedulerRenderer4Rectangle.Bottom - fillTop;
				}
				rectangleF = new RectangleF(_schedulerRenderer4Rectangle.Left, fillTop, _schedulerRenderer4Rectangle.Width, fillHeight);
				if (shouldFillDuration)
				{
					Brush brush = GetBorderBrush(color);
					if (brush != null)
					{
						graphics.FillRectangle(brush, rectangleF);
					}
				}
				if (_isEndInView)
				{
					graphics.DrawRectangle(pen, _schedulerRenderer4Rectangle.Left, _schedulerRenderer4Rectangle.Top, _schedulerRenderer4Rectangle.Width - 1, _schedulerRenderer4Rectangle.Height - 1);
				}
				else
				{
					graphics.DrawRectangle(pen, _schedulerRenderer4Rectangle.Left, _schedulerRenderer4Rectangle.Top, _schedulerRenderer4Rectangle.Width - 1, schedulerRenderer4Rectangle.Height - 1);
				}
			}
			catch
			{
			}
		}

		internal void DrawImage(Graphics graphics, int availableWidth, int left, int integerArgument, int availableHeight, SchedulerAppointment schedulerAppointment, int index, int indexArgument, int thirdImageIndex, int fourthImageIndex, bool shouldUseImageListDraw)
		{
			try
			{
				int imageHeight = 0;
				if (_imageList != null)
				{
					imageHeight = _imageList.ImageSize.Height;
					_imageWidth = _imageList.ImageSize.Width;
				}
				else
				{
					index = -1;
					indexArgument = -1;
					thirdImageIndex = -1;
					fourthImageIndex = -1;
					imageHeight = 16;
					_imageWidth = 16;
				}
				int imageY = integerArgument;
				int currentRow = 1;
				_imageX = left + 3;
				int imageCount = 0;
				if (_view == SchedulerView.DayView)
				{
					int maxRows = availableHeight / (imageHeight + 3);
					_imageX = left + 3;
					Image image = GetReminderImage();
					if (image != null)
					{
						graphics.DrawImageUnscaled(image, _imageX, imageY);
						imageCount++;
						if (maxRows > currentRow)
						{
							imageY += imageHeight + 3;
							currentRow++;
						}
						else
						{
							imageY = integerArgument;
							_imageX += _imageWidth + 3;
							currentRow = 1;
							imageCount = 0;
						}
					}
					image = GetRecurrenceImage();
					if (image != null)
					{
						graphics.DrawImageUnscaled(image, _imageX, imageY);
						imageCount++;
						if (maxRows > currentRow)
						{
							imageY += imageHeight + 3;
							currentRow++;
						}
						else
						{
							imageY = integerArgument;
							_imageX += _imageWidth + 3;
							currentRow = 1;
							imageCount = 0;
						}
					}
					if (index >= 0 && index < _imageList.Images.Count)
					{
						if (shouldUseImageListDraw)
						{
							_imageList.Draw(graphics, _imageX, imageY, index);
						}
						else
						{
							graphics.DrawImageUnscaled(_imageList.Images[index], _imageX, imageY);
						}
						imageCount++;
						if (maxRows > currentRow)
						{
							imageY += imageHeight + 3;
							currentRow++;
						}
						else
						{
							imageY = integerArgument;
							_imageX += _imageWidth + 3;
							currentRow = 1;
							imageCount = 0;
						}
					}
					if (indexArgument >= 0 && indexArgument < _imageList.Images.Count)
					{
						if (shouldUseImageListDraw)
						{
							_imageList.Draw(graphics, _imageX, imageY, indexArgument);
						}
						else
						{
							graphics.DrawImageUnscaled(_imageList.Images[indexArgument], _imageX, imageY);
						}
						imageCount++;
						if (maxRows > currentRow)
						{
							imageY += imageHeight + 3;
							currentRow++;
						}
						else
						{
							imageY = integerArgument;
							_imageX += _imageWidth + 3;
							currentRow = 1;
							imageCount = 0;
						}
					}
					if (thirdImageIndex >= 0 && thirdImageIndex < _imageList.Images.Count)
					{
						if (shouldUseImageListDraw)
						{
							_imageList.Draw(graphics, _imageX, imageY, thirdImageIndex);
						}
						else
						{
							graphics.DrawImageUnscaled(_imageList.Images[thirdImageIndex], _imageX, imageY);
						}
						imageCount++;
						if (maxRows > currentRow)
						{
							imageY += imageHeight + 3;
							currentRow++;
						}
						else
						{
							imageY = integerArgument;
							_imageX += _imageWidth + 3;
							currentRow = 1;
							imageCount = 0;
						}
					}
					if (fourthImageIndex >= 0 && fourthImageIndex < _imageList.Images.Count)
					{
						imageCount++;
						if (shouldUseImageListDraw)
						{
							_imageList.Draw(graphics, _imageX, imageY, fourthImageIndex);
						}
						else
						{
							graphics.DrawImageUnscaled(_imageList.Images[fourthImageIndex], _imageX, imageY);
						}
						if (maxRows > currentRow)
						{
							imageY += imageHeight + 3;
							currentRow++;
						}
						else
						{
							imageY = integerArgument;
							_imageX += _imageWidth + 3;
							imageCount = 0;
						}
					}
				}
				else
				{
					TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
					int textWidth = (int)textLayoutRenderer.RenderContent(graphics, schedulerAppointment.Text, _appointmentFont, 0).Width;
					textLayoutRenderer.ResetFormat();
					if (availableHeight > imageHeight)
					{
						imageY += (availableHeight - imageHeight) / 2;
					}
					Image reminderImage = GetReminderImage();
					Image recurrenceImage = GetRecurrenceImage();
					if (reminderImage != null)
					{
						imageCount++;
					}
					if (recurrenceImage != null)
					{
						imageCount++;
					}
					if (index != -1)
					{
						imageCount++;
					}
					if (indexArgument != -1)
					{
						imageCount++;
					}
					if (thirdImageIndex != -1)
					{
						imageCount++;
					}
					if (fourthImageIndex != -1)
					{
						imageCount++;
					}
					int imagesWidth = imageCount * _imageWidth + imageCount * 3;
					switch (_scheduler.AllDayEventsAlignment)
					{
					case StringAlignment.Center:
						if (_isMultiDay)
						{
							_imageX = (availableWidth - imagesWidth - textWidth) / 2;
						}
						else
						{
							_imageX = 3;
						}
						if (_imageX < 0)
						{
							_imageX = left + 3;
						}
						else
						{
							_imageX += left;
						}
						break;
					case StringAlignment.Near:
						_imageX = left + 3;
						break;
					case StringAlignment.Far:
						_imageX = left + availableWidth - imagesWidth;
						if (_imageX < 0)
						{
							_imageX = left + 3;
						}
						break;
					}
					if (reminderImage != null)
					{
						graphics.DrawImageUnscaled(reminderImage, _imageX, imageY);
						_imageX += _imageWidth + 3;
					}
					if (recurrenceImage != null)
					{
						graphics.DrawImageUnscaled(recurrenceImage, _imageX, imageY);
						_imageX += _imageWidth + 3;
					}
					if (index >= 0 && index < _imageList.Images.Count)
					{
						if (shouldUseImageListDraw)
						{
							RectangleF clipBounds = graphics.ClipBounds;
							if (clipBounds.X <= (float)_imageX && clipBounds.Right >= (float)(_imageX + _imageList.ImageSize.Width))
							{
								_imageList.Draw(graphics, _imageX, imageY, index);
							}
							else
							{
								graphics.DrawImageUnscaled(_imageList.Images[index], _imageX, imageY);
							}
						}
						else
						{
							graphics.DrawImageUnscaled(_imageList.Images[index], _imageX, imageY);
						}
						_imageX += _imageWidth + 3;
					}
					if (indexArgument >= 0 && indexArgument < _imageList.Images.Count)
					{
						if (shouldUseImageListDraw)
						{
							RectangleF secondClipBounds = graphics.ClipBounds;
							if (secondClipBounds.X <= (float)_imageX && secondClipBounds.Right >= (float)(_imageX + _imageList.ImageSize.Width))
							{
								_imageList.Draw(graphics, _imageX, imageY, indexArgument);
							}
							else
							{
								graphics.DrawImageUnscaled(_imageList.Images[indexArgument], _imageX, imageY);
							}
						}
						else
						{
							graphics.DrawImageUnscaled(_imageList.Images[indexArgument], _imageX, imageY);
						}
						_imageX += _imageWidth + 3;
					}
					if (thirdImageIndex >= 0 && thirdImageIndex < _imageList.Images.Count)
					{
						if (shouldUseImageListDraw)
						{
							RectangleF thirdClipBounds = graphics.ClipBounds;
							if (thirdClipBounds.X <= (float)_imageX && thirdClipBounds.Right >= (float)(_imageX + _imageList.ImageSize.Width))
							{
								_imageList.Draw(graphics, _imageX, imageY, thirdImageIndex);
							}
							else
							{
								graphics.DrawImageUnscaled(_imageList.Images[thirdImageIndex], _imageX, imageY);
							}
						}
						else
						{
							graphics.DrawImageUnscaled(_imageList.Images[thirdImageIndex], _imageX, imageY);
						}
						_imageX += _imageWidth + 3;
					}
					if (fourthImageIndex >= 0 && fourthImageIndex < _imageList.Images.Count)
					{
						if (shouldUseImageListDraw)
						{
							RectangleF fourthClipBounds = graphics.ClipBounds;
							if (fourthClipBounds.X <= (float)_imageX && fourthClipBounds.Right >= (float)(_imageX + _imageList.ImageSize.Width))
							{
								_imageList.Draw(graphics, _imageX, imageY, fourthImageIndex);
							}
							else
							{
								graphics.DrawImageUnscaled(_imageList.Images[fourthImageIndex], _imageX, imageY);
							}
						}
						else
						{
							graphics.DrawImageUnscaled(_imageList.Images[fourthImageIndex], _imageX, imageY);
						}
						_imageX += _imageWidth + 3;
					}
				}
				if (_view == SchedulerView.DayView)
				{
					if (imageCount > 0 && imageCount < currentRow)
					{
						_textStartX = _imageX + _imageWidth + 3;
					}
					else
					{
						_textStartX = _imageX;
					}
				}
				else if (_scheduler.AllDayEventsAlignment != StringAlignment.Far)
				{
					_textStartX = _imageX;
				}
				else
				{
					_textAreaWidth = availableWidth - _imageWidth - 3;
				}
			}
			catch
			{
			}
		}


		private void DrawEdgeLines(Graphics graphics, Rectangle rectangle)
		{
			Pen pen = _borderPen;
			graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top);
			graphics.DrawLine(pen, rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1);
			if (IsFirstSegment())
			{
				graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom - 1);
				rectangle.X++;
				rectangle.Width--;
			}
			if (_isLastSegment)
			{
				graphics.DrawLine(pen, rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1);
			}
		}

		internal Rectangle DrawStartEndClocks(Graphics graphics, Rectangle rectangleArgument, bool shouldCenterVertically)
		{
			Rectangle rectangle = Rectangle.Empty;
			if (rectangleArgument.Width - 36 >= 50)
			{
				rectangle = new Rectangle(rectangleArgument.Left + 1, rectangleArgument.Top, 33, rectangleArgument.Height);
				DrawClock(graphics, false, rectangle, shouldCenterVertically);
				DrawClock(graphics, true, rectangle, shouldCenterVertically);
			}
			else if (rectangleArgument.Width - 18 >= 50)
			{
				rectangle = new Rectangle(rectangleArgument.Left + 1, rectangleArgument.Top, 18, rectangleArgument.Height);
				DrawClock(graphics, false, rectangle, shouldCenterVertically);
			}
			return rectangle;
		}

		internal void RenderContent(Graphics graphics, SchedulerAppointmentEditControl editor)
		{
			if (_view == SchedulerView.DayView)
			{
				editor.Bounds = new Rectangle(_bounds.Left + 6, _bodyBounds.Top, _bounds.Width - 7, _bodyBounds.Height);
			}
			else if (_isMultiDay || _showTimeAsClocks)
			{
				if (_scheduler.ClockStyle == ClockStyle.None)
				{
					Rectangle noClockBounds = new Rectangle(_bounds.Left + 1, _bounds.Top + 1, _bounds.Width - 2, _bounds.Height - 2);
					if (!_isFirstSegment)
					{
						noClockBounds.X--;
						noClockBounds.Width++;
					}
					if (!_isLastSegment)
					{
						noClockBounds.Width++;
					}
					editor.Bounds = noClockBounds;
				}
				else if (_isMultiDay)
				{
					Rectangle inlineClockBounds = new Rectangle(_bounds.Left + 1, _bounds.Top + 1, _bounds.Width - 2, _bounds.Height - 2);
					if (!_isFirstSegment)
					{
						inlineClockBounds.X--;
						inlineClockBounds.Width++;
					}
					if (!_isLastSegment)
					{
						inlineClockBounds.Width++;
					}
					editor.Bounds = inlineClockBounds;
				}
				else
				{
					Rectangle leadingClockBounds = new Rectangle(_bounds.Left + 1, _bounds.Top + 1, _bounds.Width - 2, _bounds.Height - 2);
					if (leadingClockBounds.Width - 36 >= 48)
					{
						leadingClockBounds.X += 38;
						leadingClockBounds.Width -= 38;
					}
					else if (leadingClockBounds.Width - 18 >= 48)
					{
						leadingClockBounds.X += 20;
						leadingClockBounds.Width -= 20;
					}
					editor.Bounds = leadingClockBounds;
				}
			}
			else
			{
				editor.Bounds = GetEditorBounds(graphics);
			}
		}
	}
}
