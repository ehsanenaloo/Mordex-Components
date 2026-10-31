using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;

namespace Mordex.Windows.Scheduler
{
	internal partial class SchedulerAppointmentView
	{
		private void DrawDayViewContent(Graphics graphics, int firstImageIndex, int integerArgument, int thirdImageIndex, int fourthImageIndex, Rectangle rect)
		{
			Region region = null;
			Region clip = graphics.Clip;
			if (HasAnyImage(firstImageIndex, integerArgument, thirdImageIndex, fourthImageIndex))
			{
				region = (graphics.Clip = new Region(rect));
				DrawImage(graphics, rect.Width, rect.Left, rect.Top, rect.Height, _appointment, firstImageIndex, integerArgument, thirdImageIndex, fourthImageIndex, true);
			}
			if (_appointmentState == AppointmentState.Normal || !_scheduler._isEditingAppointment)
			{
				Rectangle rectangle = new Rectangle(_textStartX, rect.Top, rect.Right - _textStartX, rect.Height);
				if (!_isStartInView)
				{
					_timeSpan = _scheduler._firstVisibleSlotTime;
				}
				string text = "";
				text = ((GetAppointment().Prefix != "") ? (GetAppointment().Prefix + " " + GetAppointment().Text) : ((!(_scheduler.AppointmentsPrefix != "")) ? GetAppointment().Text : (_scheduler.AppointmentsPrefix + " " + GetAppointment().Text)));
				DrawAppointmentText(graphics, text, rectangle, GetEffectiveFormatStyle().ResolveFont(_appointmentFont), GetEffectiveFormatStyle().ResolveForeColor(_scheduler.GetThemedForeColor()), ShouldShowTimeHint());
			}
			if (region != null)
			{
				region.Dispose();
				region = null;
				graphics.Clip = clip;
			}
		}

		internal Rectangle DrawTimeRange(Graphics graphics, Rectangle rectangle)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			string text;
			string timeRangeText;
			if (_scheduler.TimeFormat == TimeFormat.TwentyFourHours)
			{
				text = _appointment._startTime.ToString("%H:mm");
				timeRangeText = text + "-" + _appointment._endTime.ToString("%H:mm");
			}
			else
			{
				text = _appointment._startTime.ToString("t", Application.CurrentCulture);
				timeRangeText = text + " " + _appointment._endTime.ToString("t", Application.CurrentCulture);
			}
			Font font = GetEffectiveFormatStyle().ResolveFont(_appointmentFont);
			int timeTextWidth = (int)textLayoutRenderer.RenderContent(graphics, timeRangeText, font, 0).Width;
			if (_bounds.Width - timeTextWidth >= 50)
			{
				textLayoutRenderer.DrawText(graphics, timeRangeText, rectangle, font, GetEffectiveFormatStyle().ResolveForeColor(_scheduler.GetThemedForeColor()));
			}
			else
			{
				timeTextWidth = (int)textLayoutRenderer.RenderContent(graphics, text, font, 0).Width;
				if (_bounds.Width - timeTextWidth >= 50)
				{
					textLayoutRenderer.DrawText(graphics, text, rectangle, font, GetEffectiveFormatStyle().ResolveForeColor(_scheduler.GetThemedForeColor()));
				}
				else
				{
					timeTextWidth = 0;
				}
			}
			rectangle.Width -= timeTextWidth;
			rectangle.X += timeTextWidth;
			textLayoutRenderer.ResetFormat();
			return rectangle;
		}

		internal Rectangle DrawTimeLabels(Graphics graphics, Rectangle rectangle)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			string text = "";
			string endOrRangeText = "";
			int timeTextWidth = 0;
			if (_appointment.Duration.TotalDays < 1.0)
			{
				text = FormatTime(_appointment.StartTime);
				endOrRangeText = text + "-" + FormatTime(_appointment.EndTime);
				Font font = GetEffectiveFormatStyle().ResolveFont(_appointmentFont);
				timeTextWidth = (int)textLayoutRenderer.RenderContent(graphics, endOrRangeText, font, 0).Width;
				if (_bounds.Width - timeTextWidth >= 50)
				{
					textLayoutRenderer.DrawText(graphics, endOrRangeText, rectangle, font, GetEffectiveFormatStyle().ResolveForeColor(_scheduler.GetThemedForeColor()));
				}
				else
				{
					timeTextWidth = (int)textLayoutRenderer.RenderContent(graphics, text, font, 0).Width;
					if (_bounds.Width - timeTextWidth >= 50)
					{
						textLayoutRenderer.DrawText(graphics, text, rectangle, font, GetEffectiveFormatStyle().ResolveForeColor(_scheduler.GetThemedForeColor()));
					}
					else
					{
						timeTextWidth = 0;
					}
				}
				rectangle.Width -= timeTextWidth;
				rectangle.X += timeTextWidth;
			}
			else
			{
				if (_isFirstSegment)
				{
					if (_appointment.StartTime.TimeOfDay != TimeSpan.Zero)
					{
						text = FormatTime(_appointment.StartTime);
					}
				}
				else
				{
					text = _scheduler.FromText + " " + _appointment.StartTime.ToString("MMM dd");
				}
				if (_isLastSegment)
				{
					if (_appointment.EndTime.TimeOfDay != TimeSpan.Zero)
					{
						endOrRangeText = FormatTime(_appointment.EndTime);
					}
				}
				else
				{
					endOrRangeText = _scheduler.ToText + " " + _appointment.EndTime.ToString("MMM dd");
				}
				Font timeFont = GetEffectiveFormatStyle().ResolveFont(_appointmentFont);
				int startTextWidth = 0;
				int endTextWidth = 0;
				if (text.Length > 0)
				{
					startTextWidth = (int)textLayoutRenderer.RenderContent(graphics, text, timeFont, 0).Width;
				}
				if (endOrRangeText.Length > 0)
				{
					endTextWidth = (int)textLayoutRenderer.RenderContent(graphics, endOrRangeText, timeFont, 0).Width;
				}
				if (_bounds.Width - startTextWidth - endTextWidth >= 50)
				{
					textLayoutRenderer.DrawText(graphics, text, rectangle, timeFont, GetEffectiveFormatStyle().ResolveForeColor(_scheduler.GetThemedForeColor()));
					textLayoutRenderer.UpdateStringAlignment(StringAlignment.Far);
					textLayoutRenderer.DrawText(graphics, endOrRangeText, rectangle, timeFont, GetEffectiveFormatStyle().ResolveForeColor(_scheduler.GetThemedForeColor()));
					textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
					rectangle.X += startTextWidth;
					rectangle.Width -= endTextWidth + startTextWidth;
				}
			}
			textLayoutRenderer.ResetFormat();
			return rectangle;
		}
	}
}
