using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Timeline
{
	internal class CalendarControl : Control
	{
		protected Timer _scrollTimer;

		private IntPtr _intPtr;

		internal TimelineSurface _owner;

		public bool _useCompatibleTextRendering;

		public DateTime _maxDate = DateTime.MaxValue.AddMonths(-1);

		public DateTime _minDate = DateTime.MinValue.AddMonths(1);

		private int _scrollDirection;

		protected int _rowHeight;

		internal bool _isDroppedDown;

		internal Color _borderColor;

		internal TimelineFormatStyle _timelineFormatStyle;

		private object _value;

		private DateTime _firstVisibleMonth;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 128;
				return createParams;
			}
		}

		internal CalendarControl(bool useCompatibleTextRendering)
		{
			_useCompatibleTextRendering = useCompatibleTextRendering;
			UpdateTimer();
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		}

		private void UpdateTimer()
		{
			_scrollTimer = new Timer();
			_scrollTimer.Interval = 150;
			_scrollTimer.Tick += OnTimerTick;
			base.TabIndex = 0;
			base.Size = new Size(0, 0);
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
						UpdateFirstVisibleMonth(_firstVisibleMonth.AddMonths(-1));
					}
					else if (eventArgs.Y > clientRectangle.Bottom)
					{
						_scrollDirection = 1;
						UpdateFirstVisibleMonth(_firstVisibleMonth.AddMonths(1));
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
			UnDrop(cancel: false);
		}

		internal virtual void UnDrop(bool cancel)
		{
			NativeMethods.SetParent(base.Handle, _intPtr);
			_scrollTimer.Enabled = false;
			TimelineUtilities.SetControlCapture(this, false);
			_isDroppedDown = false;
			if (_owner != null)
			{
				_owner.ApplyPickedMonth(GetHighlightedMonth());
			}
		}

		internal virtual void Drop(Point pt, Size szHeader, DateTime month)
		{
			_isDroppedDown = true;
			_rowHeight = szHeader.Height;
			pt.Y = pt.Y - _rowHeight * 3 - 1;
			szHeader.Height = szHeader.Height * 7 + 2;
			base.Size = szHeader;
			UpdateFirstVisibleMonth(month.AddMonths(-3));
			_intPtr = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			base.Location = pt;
			base.Visible = true;
			TimelineUtilities.SetControlCapture(this, true);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_useCompatibleTextRendering);
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
					Brush highlightBrush = _timelineFormatStyle.GetBackBrush(rectangle);
					if (highlightBrush != null)
					{
						eventArgs.Graphics.FillRectangle(highlightBrush, rectangle);
					}
					textLayoutRenderer.DrawText(eventArgs.Graphics, dateTime.ToString("MMMM yyyy", Application.CurrentCulture), rectangle, Font, _timelineFormatStyle.ResolveForeColor(SystemColors.HighlightText));
				}
				else
				{
					eventArgs.Graphics.FillRectangle(brush, rectangle);
					textLayoutRenderer.DrawText(eventArgs.Graphics, dateTime.ToString("MMMM yyyy", Application.CurrentCulture), rectangle, Font, ForeColor);
				}
				rowTop += _rowHeight;
				dateTime = dateTime.AddMonths(1);
			}
			textLayoutRenderer.ResetFormat();
			TimelineUtilities.DrawFlatBorder(eventArgs.Graphics, GdiCache.Pen(_borderColor), base.ClientRectangle);
			_timelineFormatStyle.DisposeResources();
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
	}
}
