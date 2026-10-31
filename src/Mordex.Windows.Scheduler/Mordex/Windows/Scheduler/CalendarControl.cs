using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal class CalendarControl : Control
	{
		protected Timer _scrollTimer;

		protected int _rowHeight;

		internal bool _isDroppedDown;

		internal MxCalendarView _owner;

		internal Color _borderColor;

		public DateTime _maxDate = DateTime.MaxValue.AddMonths(-1);

		public DateTime _minDate = DateTime.MinValue.AddMonths(1);

		private int _scrollDirection;

		internal SchedulerFormatStyle _schedulerFormatStyle;

		private object _value;

		private IntPtr _intPtr;

		internal DateTime _firstVisibleMonth;

		public bool _useTextRenderer;

		private string _text = "MMMM yyyy";

		private MxCalendarView.DateSelectedCallback _valueSelectedCallback;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 128;
				return createParams;
			}
		}

		internal CalendarControl(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
			UpdateTimer();
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
		}

		public string GetMonthFormat()
		{
			return _text;
		}

		public void SetMonthFormat(string text)
		{
			_text = text;
		}

		private void UpdateTimer()
		{
			_scrollTimer = new Timer();
			_scrollTimer.Interval = 100;
			_scrollTimer.Tick += OnTimerTick;
			base.TabIndex = 0;
			base.Size = new Size(0, 0);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _scrollTimer != null)
			{
				_scrollTimer.Tick -= OnTimerTick;
				_scrollTimer.Dispose();
			}
			base.Dispose(disposing);
		}

		protected void OnTimerTick(object sender, EventArgs eventArgs)
		{
			UpdateFirstVisibleMonth(_firstVisibleMonth.AddMonths(_scrollDirection));
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			Rectangle clientRectangle = base.ClientRectangle;
			_scrollDirection = 0;
			if (clientRectangle.Contains(eventArgs.X, eventArgs.Y))
			{
				int offsetY = eventArgs.Y - clientRectangle.Top;
				int months = offsetY / _rowHeight;
				SetHighlightedMonth(_firstVisibleMonth.AddMonths(months));
			}
			else
			{
				SetHighlightedMonth(DateTime.MinValue);
				try
				{
					if (eventArgs.Y < clientRectangle.Top)
					{
						_scrollDirection = -1;
					}
					else if (eventArgs.Y > clientRectangle.Bottom)
					{
						_scrollDirection = 1;
					}
				}
				catch
				{
					_scrollDirection = 0;
					UpdateFirstVisibleMonth(_firstVisibleMonth);
				}
			}
			_scrollTimer.Enabled = _scrollDirection != 0;
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			_scrollTimer.Enabled = false;
			UnDrop(false);
		}

		internal void UnDrop(bool cancel)
		{
			base.Visible = false;
			NativeMethods.SetParent(base.Handle, _intPtr);
			_scrollTimer.Enabled = false;
			SchedulerRenderer.SetCapture(this, false);
			_isDroppedDown = false;
			if (_owner != null)
			{
				_owner.ApplyPickedMonth(GetHighlightedMonth());
			}
		}

		internal void Drop(Point point, Size size, DateTime dateTime)
		{
			_isDroppedDown = true;
			_rowHeight = size.Height;
			point.Y = point.Y - _rowHeight * 3 - 1;
			size.Height = size.Height * 7 + 2;
			base.Size = size;
			_firstVisibleMonth = dateTime.AddMonths(-3);
			_intPtr = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			NativeMethods.SetWindowPos(base.Handle, (IntPtr)(-1), 0, 0, 0, 0, 67);
			base.Location = point;
			SchedulerRenderer.SetCapture(this, true);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(_useTextRenderer);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			Brush brush = GdiCache.Brush(BackColor);
			Rectangle clientRectangle = base.ClientRectangle;
			int rowTop = clientRectangle.Top + 1;
			DateTime dateTime = _firstVisibleMonth;
			for (int i = 0; i < 7; i++)
			{
				Rectangle rectangle = new Rectangle(clientRectangle.Left, rowTop, clientRectangle.Width, _rowHeight);
				if (_value != null && dateTime.Month == ((DateTime)_value).Month && dateTime.Year == ((DateTime)_value).Year)
				{
					Brush highlightBrush = _schedulerFormatStyle.GetBackBrush(rectangle);
					if (highlightBrush != null)
					{
						eventArgs.Graphics.FillRectangle(highlightBrush, rectangle);
					}
					textLayoutRenderer.DrawText(eventArgs.Graphics, dateTime.ToString(GetMonthFormat(), Application.CurrentCulture), rectangle, Font, _schedulerFormatStyle.ResolveForeColor(SystemColors.HighlightText));
				}
				else
				{
					eventArgs.Graphics.FillRectangle(brush, rectangle);
					textLayoutRenderer.DrawText(eventArgs.Graphics, dateTime.ToString(GetMonthFormat(), Application.CurrentCulture), rectangle, Font, ForeColor);
				}
				rowTop += _rowHeight;
				dateTime = dateTime.AddMonths(1);
			}
			SchedulerRenderer.DrawBorder(eventArgs.Graphics, GdiCache.Pen(_borderColor), base.ClientRectangle);
		}

		internal object GetHighlightedMonth()
		{
			return _value;
		}

		internal void SetHighlightedMonth(object value)
		{
			if (value != null && !(value is DateTime))
			{
				throw new ArgumentException();
			}
			if (_value != value)
			{
				_value = value;
				Invalidate();
				Update();
			}
		}

		internal void UpdateFirstVisibleMonth(DateTime firstVisibleMonth)
		{
			if (_firstVisibleMonth != firstVisibleMonth && firstVisibleMonth.AddMonths(6) <= _maxDate && firstVisibleMonth.AddMonths(3) >= _minDate)
			{
				_firstVisibleMonth = firstVisibleMonth;
				Invalidate();
				Update();
			}
		}

		internal void SetValueSelectedCallback(MxCalendarView.DateSelectedCallback valueSelectedCallback)
		{
			_valueSelectedCallback = valueSelectedCallback;
		}
	}
}
