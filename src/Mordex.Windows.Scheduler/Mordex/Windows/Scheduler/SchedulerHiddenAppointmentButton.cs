using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerHiddenAppointmentButton : Control
	{
		private Container _container = new Container();

		private int _direction = -1;

		internal HiddenAppointmentMode _mode;

		internal DateTime _date;

		internal MxScheduler _scheduler;

		private bool _isDateLabel;

		internal SchedulerHiddenAppointmentButton(MxScheduler scheduler, DateTime date, int direction, HiddenAppointmentMode mode)
		{
			_scheduler = scheduler;
			_date = date;
			_direction = direction;
			_mode = mode;
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetContainer();
			BackColor = SystemColors.Info;
			ForeColor = SystemColors.ControlText;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		public void UpdateDirection(int direction)
		{
			if (_direction != direction)
			{
				_direction = direction;
				Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (IsDateLabel())
			{
				return;
			}
			if (_mode == HiddenAppointmentMode.Automatic)
			{
				if (base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
				{
					_scheduler.View = SchedulerView.DayView;
					_scheduler.Date = _date;
					_scheduler._isAppointmentCacheStale = true;
				}
			}
			else
			{
				_scheduler.RaiseHiddenAppointmentClick(_date);
			}
		}

		public bool IsDateLabel()
		{
			return _isDateLabel;
		}

		public void SetDateLabel(bool isDateLabel)
		{
			_isDateLabel = isDateLabel;
		}

		// The default colors follow the tip colors of the theme (the Standard theme gives the Windows info colors); a color set
		// on the button wins.
		internal Color GetFillColor()
		{
			return BackColor == SystemColors.Info ? MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.TooltipBack) : BackColor;
		}

		internal Color GetTextColor()
		{
			return ForeColor == SystemColors.ControlText ? MxThemeManager.ResolveDefault(DeviceDpi).Color(ThemeRole.TooltipText) : ForeColor;
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			Color textColor = GetTextColor();
			Brush brush = GdiCache.Brush(GetFillColor());
			if (!_isDateLabel)
			{
				Brush foregroundBrush = GdiCache.Brush(textColor);
				Point[] arrowPoints = new Point[3];
				int left = base.ClientRectangle.Left;
				int top = base.ClientRectangle.Top;
				Rectangle rect;
				if (_direction == 1)
				{
					ref Point reference = ref arrowPoints[0];
					reference = new Point(left + 1, top + 6);
					ref Point upBaseRight = ref arrowPoints[1];
					upBaseRight = new Point(left + 10, top + 6);
					ref Point upTip = ref arrowPoints[2];
					upTip = new Point(left + 5, top + 1);
					rect = new Rectangle(left + 10, top + 4, 2, 2);
				}
				else
				{
					ref Point downLeft = ref arrowPoints[0];
					downLeft = new Point(left + 2, top + 2);
					ref Point downRight = ref arrowPoints[1];
					downRight = new Point(left + 9, top + 2);
					ref Point downTip = ref arrowPoints[2];
					downTip = new Point(left + 5, top + 6);
					rect = new Rectangle(left + 10, top + 2, 2, 2);
				}
				eventArgs.Graphics.FillRectangle(brush, left, top, 19, 7);
				using (Pen pen = new Pen(foregroundBrush))
				{
					eventArgs.Graphics.DrawRectangle(pen, left, top, base.ClientRectangle.Right - 1, base.ClientRectangle.Height - 1);
				}
				eventArgs.Graphics.FillPolygon(foregroundBrush, arrowPoints);
				eventArgs.Graphics.FillRectangle(foregroundBrush, rect);
				rect.X += 3;
				eventArgs.Graphics.FillRectangle(foregroundBrush, rect);
				rect.X += 3;
				eventArgs.Graphics.FillRectangle(foregroundBrush, rect);
			}
			else
			{
				eventArgs.Graphics.FillRectangle(brush, base.ClientRectangle);
				eventArgs.Graphics.DrawRectangle(GdiCache.Pen(textColor), 0, 0, base.ClientRectangle.Width - 1, base.ClientRectangle.Height - 1);
				TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_scheduler.UseCompatibleTextRendering);
				textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
				textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
				textLayoutRenderer.UpdateIsPaddingDisabled(true);
				textLayoutRenderer.DrawText(eventArgs.Graphics, _date.ToShortDateString(), base.ClientRectangle, Font, textColor);
			}
		}

		private void SetContainer()
		{
			_container = new Container();
		}
	}
}
