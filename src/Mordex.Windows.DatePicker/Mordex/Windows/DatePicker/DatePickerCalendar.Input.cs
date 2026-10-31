using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	public partial class DatePickerCalendar
	{
		// Horizontal space reserved for the previous and next arrows inside a month header.
		private const int HeaderArrowWidth = 12;

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetHotMonthHeader(null);
			SetHotButton(null);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			_scrollTimer.Enabled = false;
			_scrollTimer.Interval = ScrollRepeatInterval;
			if (_pressedButton == _todayButton)
			{
				if (_todayButton._bounds.Contains(e.X, e.Y))
				{
					OnTodayButtonClicked();
				}
				else
				{
					DatePickerHelper.SetMouseCapture(this, true);
				}
				SetPressedButton(null);
				return;
			}
			if (_pressedButton == _noneButton)
			{
				if (_noneButton._bounds.Contains(e.X, e.Y))
				{
					OnNoneButtonClicked();
				}
				SetPressedButton(null);
				return;
			}
			if (base.ClientRectangle.Contains(e.X, e.Y))
			{
				CalendarCell cell = GetCellAt(e.X, e.Y, false);
				if (cell != null && cell.GetIsSelectable())
				{
					SetSelectedDate(cell.GetDate());
					CloseDropList(false);
				}
			}
			if (Visible && _isDroppedDown)
			{
				DisposeCaptureTimer();
				_captureTimer = new Timer();
				_captureTimer.Interval = CaptureRetakeInterval;
				_captureTimer.Tick += OnCaptureTimerTick;
				_captureTimer.Enabled = true;
			}
		}

		private void OnCaptureTimerTick(object sender, EventArgs eventArgs)
		{
			if (Visible)
			{
				DatePickerHelper.SetMouseCapture(this, true);
			}
			DisposeCaptureTimer();
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == WmCaptureChanged)
			{
				base.WndProc(ref msg);
				if (_retakeCaptureOnLoss && msg.LParam == IntPtr.Zero && Visible)
				{
					DatePickerHelper.SetMouseCapture(this, true);
					_retakeCaptureOnLoss = false;
				}
			}
			else if (msg.Msg == WmCancelMode)
			{
				CloseDropList(true);
				base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (_pressedButton != null)
			{
				_pressedButton.SetIsPressed(GetButtonAt(e.X, e.Y) == _pressedButton);
				return;
			}
			if (e.Button == MouseButtons.None)
			{
				SetHotButton(GetButtonAt(e.X, e.Y));
			}
			MonthLayout hoverMonth = GetMonthHeaderAt(e.X, e.Y, false);
			if (hoverMonth != null)
			{
				if (hoverMonth != _hotMonthHeader)
				{
					SetHotMonthHeader(hoverMonth);
				}
				return;
			}
			SetHotMonthHeader(null);
			if (e.Button == MouseButtons.Left)
			{
				CalendarCell cell = GetCellAt(e.X, e.Y, false);
				if (cell != null && cell.GetIsSelectable())
				{
					SetSelectedDate(cell.GetDate());
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (_isDroppedDown && !base.ClientRectangle.Contains(e.X, e.Y))
			{
				DatePickerHelper.SetMouseCapture(this, false);
				CloseDropList(true);
				return;
			}
			for (int arrowIndex = 0; arrowIndex < _scrollArrowBounds.Length; arrowIndex++)
			{
				if (_scrollArrowBounds[arrowIndex].Contains(e.X, e.Y))
				{
					_scrollMonthsStep = arrowIndex == 0 ? -_scrollChange : _scrollChange;
					FirstMonth = DatePickerHelper.AddMonthsClamped(_firstMonth, _scrollMonthsStep);
					_scrollTimer.Interval = SystemInformation.DoubleClickTime;
					_scrollTimer.Enabled = true;
					return;
				}
			}
			MonthLayout headerMonth = GetMonthHeaderAt(e.X, e.Y, true);
			if (headerMonth != null)
			{
				ShowMonthPicker(headerMonth);
				return;
			}
			CalendarCell cell = GetCellAt(e.X, e.Y, true);
			if (cell != null && cell.GetIsSelectable())
			{
				_followSelection = false;
				SetSelectedDate(cell.GetDate());
				_followSelection = true;
			}
			SetPressedButton(GetButtonAt(e.X, e.Y));
		}

		private void ShowMonthPicker(MonthLayout headerMonth)
		{
			SetPressedMonthHeader(headerMonth);
			Update();
			_pickedMonthIndex = headerMonth.GetMonthIndex();
			Rectangle popupBounds = headerMonth.GetHeaderBounds();
			popupBounds.Width -= _borderSize.Width / 2;
			popupBounds.Inflate(-HeaderArrowWidth, -1);
			Point location = PointToScreen(popupBounds.Location);
			DateTime pickedMonth = new DateTime(headerMonth.GetYear(), headerMonth.GetMonth(), 1);
			_calendarControl.BackColor = GetEffectiveBackColor();
			_calendarControl.ForeColor = GetEffectiveForeColor();
			_calendarControl.Font = Font;
			_calendarControl.SetHoveredMonth(pickedMonth);
			_calendarControl._calendarFormatStyle = _selectedFormatStyle;
			_calendarControl._borderColor = GetMonthPickerBorderColor();
			_calendarControl.ShowAt(location, popupBounds.Size, pickedMonth);
		}

		private Color GetMonthPickerBorderColor()
		{
			return GetThemeRenderer()._borderColor;
		}

		private PickerButton GetButtonAt(int x, int y)
		{
			if (_todayButton.ContainsPoint(x, y))
			{
				return _todayButton;
			}
			if (_noneButton.ContainsPoint(x, y))
			{
				return _noneButton;
			}
			return null;
		}

		internal CalendarCell GetCellAt(int x, int y, bool snapToVisibleCell)
		{
			Size monthSize = _monthLayoutTemplate.GetMonthSize();
			if (x < 0 || y < 0 || monthSize.Width <= 0 || monthSize.Height <= 0)
			{
				return null;
			}
			int row = y / monthSize.Height;
			int column = x / monthSize.Width;
			if (row >= _rows || column >= _columns || row >= _monthLayouts.GetLength(0) || column >= _monthLayouts.GetLength(1))
			{
				return null;
			}
			return _monthLayouts[row, column].FindCellAt(x, y, snapToVisibleCell);
		}

		private MonthLayout GetMonthHeaderAt(int x, int y, bool excludeArrowAreas)
		{
			for (int row = 0; row < _rows; row++)
			{
				for (int column = 0; column < _columns; column++)
				{
					MonthLayout monthLayout = _monthLayouts[row, column];
					Rectangle headerBounds = monthLayout.GetHeaderBounds();
					if (excludeArrowAreas)
					{
						headerBounds.Inflate(-HeaderArrowWidth, -1);
					}
					if (headerBounds.Contains(x, y))
					{
						return monthLayout;
					}
				}
			}
			return null;
		}

		private void SetHotMonthHeader(MonthLayout monthLayout)
		{
			if (_hotMonthHeader != null)
			{
				_hotMonthHeader.SetIsHot(false);
				Invalidate(_hotMonthHeader.GetHeaderBounds());
			}
			_hotMonthHeader = monthLayout;
			if (_hotMonthHeader != null)
			{
				_hotMonthHeader.SetIsHot(true);
				Invalidate(_hotMonthHeader.GetHeaderBounds());
			}
		}

		private void SetPressedMonthHeader(MonthLayout monthLayout)
		{
			if (_pressedMonthHeader != null)
			{
				_pressedMonthHeader.SetIsPressed(false);
				Invalidate(_pressedMonthHeader.GetHeaderBounds());
			}
			_pressedMonthHeader = monthLayout;
			if (_pressedMonthHeader != null)
			{
				_pressedMonthHeader.SetIsPressed(true);
				Invalidate(_pressedMonthHeader.GetHeaderBounds());
			}
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Down || keyData == Keys.Up || keyData == Keys.Left || keyData == Keys.Right)
			{
				return true;
			}
			return base.IsInputKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			switch (e.KeyData)
			{
			case Keys.Left:
				SetSelectedDate(DatePickerHelper.AddDaysClamped(_selectedDate, -1));
				break;
			case Keys.Right:
				SetSelectedDate(DatePickerHelper.AddDaysClamped(_selectedDate, 1));
				break;
			case Keys.Up:
				SetSelectedDate(DatePickerHelper.AddDaysClamped(_selectedDate, -DatePickerHelper.DaysPerWeek));
				break;
			case Keys.Down:
				SetSelectedDate(DatePickerHelper.AddDaysClamped(_selectedDate, DatePickerHelper.DaysPerWeek));
				break;
			}
		}

		internal bool HandleDialogKey(Keys keyData)
		{
			return ProcessDialogKey(keyData);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Prior:
				SetSelectedDate(DatePickerHelper.AddMonthsClamped(_selectedDate, -1));
				break;
			case Keys.Next:
				SetSelectedDate(DatePickerHelper.AddMonthsClamped(_selectedDate, 1));
				break;
			case Keys.Down:
				SetSelectedDate(DatePickerHelper.AddDaysClamped(_selectedDate, DatePickerHelper.DaysPerWeek));
				break;
			case Keys.Up:
				SetSelectedDate(DatePickerHelper.AddDaysClamped(_selectedDate, -DatePickerHelper.DaysPerWeek));
				break;
			case Keys.Right:
				SetSelectedDate(DatePickerHelper.AddDaysClamped(_selectedDate, 1));
				break;
			case Keys.Left:
				SetSelectedDate(DatePickerHelper.AddDaysClamped(_selectedDate, -1));
				break;
			case Keys.Return:
			case Keys.F4:
			case Keys.Up | Keys.Alt:
			case Keys.Down | Keys.Alt:
				CloseDropList(false);
				break;
			case Keys.Escape:
				if (_calendarControl._isShown)
				{
					_calendarControl.Dismiss();
				}
				CloseDropList(true);
				break;
			default:
				return false;
			}
			return true;
		}

		private void SetHotButton(PickerButton button)
		{
			if (_hotButton != button)
			{
				if (_hotButton != null)
				{
					_hotButton.SetIsHot(false);
				}
				_hotButton = button;
				if (_hotButton != null)
				{
					_hotButton.SetIsHot(true);
				}
			}
		}

		private void SetPressedButton(PickerButton button)
		{
			if (_pressedButton != button)
			{
				if (_pressedButton != null)
				{
					_pressedButton.SetIsPressed(false);
				}
				_pressedButton = button;
				if (_pressedButton != null)
				{
					_pressedButton.SetIsPressed(true);
				}
			}
		}
	}
}
