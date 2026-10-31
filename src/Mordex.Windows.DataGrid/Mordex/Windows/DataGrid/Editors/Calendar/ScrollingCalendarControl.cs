using System;
using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	[ToolboxItem(false)]
	internal class ScrollingCalendarControl : Control
	{
		private Timer _timer;

		internal ControlStyleControl _owner;

		private int _scrollDirection;

		protected int _rowHeight;

		internal Color _highlightColor;

		internal Color _selectedTextColor;

		private object _selectedMonth;

		private DateTime _dateTime;

		private IntPtr _popupHandle;

		private bool _isPopupShown;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 128;
				return createParams;
			}
		}

		public ScrollingCalendarControl()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			UpdateTimer();
		}

		private void UpdateTimer()
		{
			_timer = new Timer();
			_timer.Interval = Timing.AutoRepeatInterval;
			_timer.Tick += OnTimerTick;
		}

		protected void OnTimerTick(object sender, EventArgs eventArgs)
		{
			ScrollToMonth(_dateTime.AddMonths(_scrollDirection));
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			Rectangle clientRectangle = base.ClientRectangle;
			_scrollDirection = 0;
			if (clientRectangle.Contains(eventArgs.X, eventArgs.Y))
			{
				int value = eventArgs.Y - clientRectangle.Top;
				int months = value / _rowHeight;
				try
				{
					SetSelectedMonth(_dateTime.AddMonths(months));
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
				}
			}
			else
			{
				SetSelectedMonth(null);
				if (eventArgs.Y < clientRectangle.Top)
				{
					try
					{
						ScrollToMonth(_dateTime.AddMonths(-1));
						_scrollDirection = -1;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						_scrollDirection = 0;
					}
				}
				else if (eventArgs.Y > clientRectangle.Bottom)
				{
					try
					{
						ScrollToMonth(_dateTime.AddMonths(1));
						_scrollDirection = 1;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						_scrollDirection = 0;
					}
				}
			}
			_timer.Enabled = _scrollDirection != 0;
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			ClosePopup();
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			Brush brush = GdiCache.Brush(BackColor);
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(_isPopupShown);
			textLayoutRenderer.SetStringAlignment(StringAlignment.Center);
			Rectangle clientRectangle = base.ClientRectangle;
			int top = clientRectangle.Top;
			DateTime dateTime = _dateTime;
			for (int i = 0; i < 7; i++)
			{
				Rectangle rectangle = new Rectangle(clientRectangle.Left, top, clientRectangle.Width, _rowHeight);
				if (_selectedMonth != null && dateTime.Month == ((DateTime)_selectedMonth).Month && dateTime.Year == ((DateTime)_selectedMonth).Year)
				{
					Brush solidBrush = GdiCache.Brush(_highlightColor);
					eventArgs.Graphics.FillRectangle(solidBrush, rectangle);
					textLayoutRenderer.DrawTextWithColor(eventArgs.Graphics, dateTime.ToString("MMMM yyyy", GridHelper.GetCurrentCulture()), rectangle, Font, _selectedTextColor);
				}
				else
				{
					eventArgs.Graphics.FillRectangle(brush, rectangle);
					textLayoutRenderer.DrawTextWithColor(eventArgs.Graphics, dateTime.ToString("MMMM yyyy", GridHelper.GetCurrentCulture()), rectangle, Font, ForeColor);
				}
				top += _rowHeight;
				try
				{
					dateTime = dateTime.AddMonths(1);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					break;
				}
			}
			GridHelper.DrawFlatBorder(eventArgs.Graphics, SystemPens.WindowFrame, base.ClientRectangle);
			textLayoutRenderer.ReleaseStringFormat();
			brush = null;
		}

		internal object GetSelectedMonth()
		{
			return _selectedMonth;
		}

		internal void SetSelectedMonth(object value)
		{
			if (value != null && !(value is DateTime))
			{
				throw new ArgumentException();
			}
			if (_selectedMonth != value)
			{
				_selectedMonth = value;
				Invalidate();
				Update();
			}
		}

		internal void ScrollToMonth(DateTime dateTime)
		{
			if (_dateTime != dateTime)
			{
				_dateTime = dateTime;
				Invalidate();
				Update();
			}
		}

		protected override void OnHandleCreated(EventArgs eventArgs)
		{
			base.OnHandleCreated(eventArgs);
			NativeMethods.SetProp(base.Handle, "PROP_ISCELLDROPDOWN", new IntPtr(1));
		}

		protected override void OnHandleDestroyed(EventArgs eventArgs)
		{
			NativeMethods.RemoveProp(base.Handle, "PROP_ISCELLDROPDOWN");
			base.OnHandleDestroyed(eventArgs);
		}

		internal void ShowPopupAt(Point point, Size size, DateTime dateTime, bool condition)
		{
			_isPopupShown = condition;
			_rowHeight = size.Height;
			point.Y = point.Y - _rowHeight * 3 - 1;
			size.Height = size.Height * 7 + 2;
			base.Size = size;
			_ = DateTime.MinValue;
			try
			{
				ScrollToMonth(dateTime.AddMonths(-3));
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				ScrollToMonth(DateTime.MinValue);
			}
			_popupHandle = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			base.Location = point;
			NativeMethods.SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 67);
			GridHelper.SetCapture(this, true);
		}

		internal void ClosePopup()
		{
			NativeMethods.SetParent(base.Handle, _popupHandle);
			_popupHandle = IntPtr.Zero;
			_owner.SelectMonthFromPopup(GetSelectedMonth());
		}
	}
}
