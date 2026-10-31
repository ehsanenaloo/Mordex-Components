using System;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	// Small pop-up list of months that opens when the user clicks a month header.
	internal sealed class CalendarControl : Control
	{
		private const int ScrollTimerInterval = 100;

		private const int VisibleMonthCount = 7;

		private const int MonthsAboveHovered = 3;

		private Timer _scrollTimer;

		private int _rowHeight;

		internal bool _isShown;

		private IntPtr _originalParent;

		internal DatePickerCalendar _owner;

		internal DateTime _maxMonth = DateTime.MaxValue.AddMonths(-1);

		internal DateTime _minMonth = DateTime.MinValue.AddMonths(1);

		private int _scrollDirection;

		internal Color _borderColor;

		internal CalendarFormatStyle _calendarFormatStyle;

		private DateTime? _hoveredMonth;

		internal DateTime _firstVisibleMonth;

		internal bool _useTextRenderer;

		private string _monthFormat = "MMMM yyyy";

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= NativeMethods.WsExToolWindow;
				return createParams;
			}
		}

		internal CalendarControl(bool useTextRenderer)
		{
			_useTextRenderer = useTextRenderer;
			_scrollTimer = new Timer();
			_scrollTimer.Interval = ScrollTimerInterval;
			_scrollTimer.Tick += OnTimerTick;
			base.TabIndex = 0;
			base.Size = new Size(0, 0);
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _scrollTimer != null)
			{
				_scrollTimer.Tick -= OnTimerTick;
				_scrollTimer.Dispose();
				_scrollTimer = null;
			}
			base.Dispose(disposing);
		}

		internal void SetMonthFormat(string format)
		{
			_monthFormat = format;
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			SetFirstVisibleMonth(DatePickerHelper.AddMonthsClamped(_firstVisibleMonth, _scrollDirection));
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			if (_rowHeight <= 0)
			{
				return;
			}
			Rectangle clientRectangle = base.ClientRectangle;
			_scrollDirection = 0;
			if (clientRectangle.Contains(eventArgs.X, eventArgs.Y))
			{
				int months = (eventArgs.Y - clientRectangle.Top) / _rowHeight;
				SetHoveredMonth(DatePickerHelper.AddMonthsClamped(_firstVisibleMonth, months));
			}
			else
			{
				SetHoveredMonth(null);
				if (eventArgs.Y < clientRectangle.Top)
				{
					_scrollDirection = -1;
				}
				else if (eventArgs.Y > clientRectangle.Bottom)
				{
					_scrollDirection = 1;
				}
			}
			_scrollTimer.Enabled = _scrollDirection != 0;
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			Dismiss();
		}

		internal void Dismiss()
		{
			_isShown = false;
			NativeMethods.SetParent(base.Handle, _originalParent);
			_scrollTimer.Enabled = false;
			DatePickerHelper.SetMouseCapture(this, false);
			if (_owner != null)
			{
				_owner.OnMonthPicked(_hoveredMonth);
			}
		}

		internal void ShowAt(Point point, Size itemSize, DateTime centerMonth)
		{
			_isShown = true;
			_rowHeight = itemSize.Height;
			point.Y = point.Y - _rowHeight * MonthsAboveHovered - 1;
			itemSize.Height = itemSize.Height * VisibleMonthCount + 2;
			base.Size = itemSize;
			_firstVisibleMonth = DatePickerHelper.AddMonthsClamped(centerMonth, -MonthsAboveHovered);
			_originalParent = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			NativeMethods.ShowTopMost(base.Handle);
			base.Location = point;
			base.Visible = true;
			DatePickerHelper.SetMouseCapture(this, true);
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			Color hoveredTextColor = _calendarFormatStyle.ForeColor;
			if (hoveredTextColor.IsEmpty)
			{
				hoveredTextColor = SystemColors.HighlightText;
			}
			Rectangle clientRectangle = base.ClientRectangle;
			int rowTop = clientRectangle.Top + 1;
			DateTime month = _firstVisibleMonth;
			using (TextLayoutRenderer textRenderer = new TextLayoutRenderer(_useTextRenderer))
			{
				Brush backBrush = GdiCache.Brush(BackColor);
				textRenderer.Alignment = StringAlignment.Center;
				for (int row = 0; row < VisibleMonthCount; row++)
				{
					Rectangle rowBounds = new Rectangle(clientRectangle.Left, rowTop, clientRectangle.Width, _rowHeight);
					string monthText = DatePickerHelper.FormatDate(month, _monthFormat);
					if (_hoveredMonth.HasValue && month.Month == _hoveredMonth.Value.Month && month.Year == _hoveredMonth.Value.Year)
					{
						eventArgs.Graphics.FillRectangle(_calendarFormatStyle.GetBackBrush(rowBounds), rowBounds);
						textRenderer.DrawText(eventArgs.Graphics, monthText, rowBounds, Font, hoveredTextColor);
					}
					else
					{
						eventArgs.Graphics.FillRectangle(backBrush, rowBounds);
						textRenderer.DrawText(eventArgs.Graphics, monthText, rowBounds, Font, ForeColor);
					}
					rowTop += _rowHeight;
					month = DatePickerHelper.AddMonthsClamped(month, 1);
				}
			}
			DatePickerHelper.DrawRectangleOutline(eventArgs.Graphics, GdiCache.Pen(_borderColor), base.ClientRectangle);
			_calendarFormatStyle.DisposeResources();
		}

		internal void SetHoveredMonth(DateTime? month)
		{
			if (_hoveredMonth != month)
			{
				_hoveredMonth = month;
				Invalidate();
				Update();
			}
		}

		internal void SetFirstVisibleMonth(DateTime firstVisibleMonth)
		{
			if (_firstVisibleMonth != firstVisibleMonth
				&& DatePickerHelper.TryAddMonths(firstVisibleMonth, VisibleMonthCount - 1, out DateTime lastVisibleMonth)
				&& lastVisibleMonth <= _maxMonth
				&& firstVisibleMonth.AddMonths(MonthsAboveHovered) >= _minMonth)
			{
				_firstVisibleMonth = firstVisibleMonth;
				Invalidate();
				Update();
			}
		}
	}
}
