using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal partial class ControlStyleControl
	{
		protected void OnTimerTick(object sender, EventArgs eventArgs)
		{
			try
			{
				SetVisibleMonth(_dateTime.AddMonths(_scrollMonthDelta));
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private void InitializeLayoutState()
		{
			_calendarLayoutState = new CalendarLayoutState();
			BuildWeekdayNames();
			_dateTime = DateTime.Now;
			_calendarFont = (Font)base.Font.Clone();
			_adjacentMonthColor = SystemColors.GrayText;
			if (_controlStyleControlFont == null)
			{
				_controlStyleControlFont = new Font(base.Font, FontStyle.Bold);
			}
		}

		private CalendarDayCell FindDayCellAt(int value, int otherValue)
		{
			try
			{
				int nextValue = otherValue / _calendarLayoutState.GetBlockSize().Height;
				int previousValue = value / _calendarLayoutState.GetBlockSize().Width;
				if (nextValue > _monthRows - 1 || previousValue > _monthColumns - 1)
				{
					return null;
				}
				CalendarLayoutState calendarLayoutState = _monthBlocks[nextValue, previousValue];
				return calendarLayoutState.FindDayCellAt(value, otherValue);
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				return null;
			}
		}

		private CalendarLayoutState FindMonthTitleAt(int value, int otherValue)
		{
			for (int i = 0; i < _monthRows; i++)
			{
				for (int j = 0; j < _monthColumns; j++)
				{
					CalendarLayoutState calendarLayoutState = _monthBlocks[i, j];
					Rectangle rectangle = calendarLayoutState.GetTitleLayout()._bounds;
					rectangle.Inflate(-12, -1);
					if (rectangle.Contains(value, otherValue))
					{
						return calendarLayoutState;
					}
				}
			}
			return null;
		}

		protected internal void DisposeResources()
		{
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			BuildWeekdayNames();
			try
			{
				if (_calendarStyle != null)
				{
					_mordexColorSchemeAppearanceState = _calendarStyle.GetLayoutState().GetEditControlsStyle();
				}
				_calendarLayoutState = new CalendarLayoutState();
				Graphics graphics = CreateGraphics();
				graphics.PageUnit = GraphicsUnit.Pixel;
				_titleHeight = (int)graphics.MeasureString("88", _calendarFont).Height;
				if (_mordexColorSchemeAppearanceState != null && _mordexColorSchemeAppearanceState.IsThemedLook)
				{
					_titleHeight += 8;
				}
				else
				{
					_titleHeight += 6;
				}
				SizeF sizeF = graphics.MeasureString("88", _controlStyleControlFont);
				_dayCellWidth = (int)sizeF.Width + 2;
				_dayCellHeight = (int)sizeF.Height + 3;
				for (int i = 0; i < 7; i++)
				{
					sizeF = graphics.MeasureString(_weekdayNames[i], base.Font);
					if ((int)sizeF.Width + 4 > _dayCellWidth)
					{
						_dayCellWidth = (int)sizeF.Width + 2;
					}
				}
				_sideColumnWidth = _dayCellWidth;
				Rectangle rectangle = new Rectangle(1, 1, _sideColumnWidth * 2 + _dayCellWidth * 7, _titleHeight);
				if (_mordexColorSchemeAppearanceState != null && _mordexColorSchemeAppearanceState.IsThemedLook)
				{
					rectangle.Y += 4;
					rectangle.Height -= 4;
					rectangle.Inflate(-4, 0);
				}
				_calendarLayoutState.SetTitleRectangle(rectangle);
				_calendarLayoutState.LayoutCells(_dayCellWidth, _dayCellHeight, _sideColumnWidth, new Point(0, 0));
				graphics.Dispose();
				graphics = null;
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
			}
		}

		private bool IsShortDayNameCulture()
		{
			string name = Application.CurrentCulture.Name;
			if (string.Compare(name, "zh-CHS", ignoreCase: true) == 0 || string.Compare(name, "zh-MO", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "zh-CN", ignoreCase: true) == 0 || string.Compare(name, "zh-SG", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "zh-TW", ignoreCase: true) == 0)
			{
				return true;
			}
			if (string.Compare(name, "he-IL", ignoreCase: true) == 0)
			{
				return true;
			}
			return false;
		}

		private bool IsArabicCulture()
		{
			string name = CultureInfo.CurrentCulture.Name;
			if (name.StartsWith("ar", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}

		private void BuildWeekdayNames()
		{
			bool isShortDayNameCulture = IsShortDayNameCulture();
			bool startsWithCore = IsArabicCulture();
			DateTimeFormatInfo dateTimeFormat = GridHelper.GetCurrentCulture().DateTimeFormat;
			int daysOfWeekEnum = ((_daysOfWeekEnum != DaysOfWeekEnum.Default) ? ((int)_daysOfWeekEnum) : ((int)dateTimeFormat.FirstDayOfWeek));
			for (int i = 0; i < 7; i++)
			{
				if (daysOfWeekEnum == 7)
				{
					daysOfWeekEnum = 0;
				}
				string text = dateTimeFormat.GetAbbreviatedDayName((DayOfWeek)daysOfWeekEnum);
				if (_dayOfWeekAbbreviationEnum == DayOfWeekAbbreviationEnum.UseFirstLetter)
				{
					text = (isShortDayNameCulture ? DateTimeFormatInfo.CurrentInfo.GetShortestDayName((DayOfWeek)daysOfWeekEnum) : (startsWithCore ? text.Substring(text.Length - 1, 1) : text.Substring(0, 1)));
				}
				_weekdayNames[i] = text;
				daysOfWeekEnum++;
			}
		}

		protected internal void ResizeToFitMonths()
		{
			Rectangle clientRectangle = base.ClientRectangle;
			Size size = _calendarLayoutState.GetBlockSize();
			int monthRows = _monthRows;
			int monthColumns = _monthColumns;
			_monthColumns = clientRectangle.Width / size.Width;
			if (_monthColumns == 0)
			{
				_monthColumns = 1;
			}
			_monthRows = clientRectangle.Height / size.Height;
			if (_monthRows == 0)
			{
				_monthRows = 1;
			}
			if (monthColumns != _monthColumns || monthRows != _monthRows)
			{
				Invalidate();
			}
			base.Size = new Size(size.Width * _monthColumns, size.Height * _monthRows);
		}

		private void SetNoneButtonPressed(bool condition)
		{
			if (condition != _controlStyleControlGridLayoutState._isPressed)
			{
				_controlStyleControlGridLayoutState._isPressed = condition;
				Invalidate(_controlStyleControlGridLayoutState._bounds);
			}
		}

		private void SetTodayButtonHot(bool gridLayoutStateBoolean)
		{
			if (gridLayoutStateBoolean != _gridLayoutState._isHot)
			{
				_gridLayoutState._isHot = gridLayoutStateBoolean;
				Invalidate(_gridLayoutState._bounds);
			}
		}

		private void SetNoneButtonHot(bool gridLayoutStateBoolean)
		{
			if (gridLayoutStateBoolean != _controlStyleControlGridLayoutState._isHot)
			{
				_controlStyleControlGridLayoutState._isHot = gridLayoutStateBoolean;
				Invalidate(_controlStyleControlGridLayoutState._bounds);
			}
		}

		protected internal void LayoutFooterButtons()
		{
			if (base.IsDisposed || base.Disposing)
			{
				return;
			}
			_monthRows = 1;
			_monthColumns = 1;
			Size size = _calendarLayoutState.GetBlockSize();
			int height = size.Height;
			_controlStyleControlGridLayoutState._isVisible = GetNoneButtonText().Length > 0;
			_controlStyleControlGridLayoutState._isEnabled = true;
			_gridLayoutState._isVisible = GetTodayButtonText().Length > 0;
			_gridLayoutState._isEnabled = true;
			if (_todayButtonText.Length > 0 || _noneButtonText.Length > 0)
			{
				Graphics graphics = CreateGraphics();
				int otherHeight = (int)_calendarFont.GetHeight(graphics);
				height += otherHeight + 21;
				_footerBounds = new Rectangle(0, size.Height, size.Width, otherHeight + 21);
				int value = size.Width / 3;
				int width = 0;
				int otherWidth = 0;
				if (_todayButtonText.Length > 0)
				{
					width = (int)graphics.MeasureString(_todayButtonText, _calendarFont, -1).Width + 8;
					if (width < value)
					{
						width = value;
					}
				}
				if (_noneButtonText.Length > 0)
				{
					otherWidth = (int)graphics.MeasureString(_noneButtonText, _calendarFont, -1).Width + 8;
					if (otherWidth < value)
					{
						otherWidth = value;
					}
				}
				if (width > otherWidth)
				{
					otherWidth = width;
				}
				else
				{
					width = otherWidth;
				}
				value = size.Width - 3;
				if (width + otherWidth > value)
				{
					if (_noneButtonText.Length > 0 && _todayButtonText.Length > 0)
					{
						width = value / 2;
						otherWidth = width;
					}
					else if (_noneButtonText.Length > 0)
					{
						otherWidth = value;
					}
					else
					{
						width = value;
					}
				}
				if (_noneButtonText.Length > 0)
				{
					if (GetTodayButtonText().Length > 0)
					{
						int otherValue = (size.Width - (width + otherWidth)) / 3;
						_gridLayoutState._bounds = new Rectangle(otherValue, size.Height + 7, width, otherHeight + 8);
						_controlStyleControlGridLayoutState._bounds = _gridLayoutState._bounds;
						_controlStyleControlGridLayoutState._bounds.Offset(otherValue + width, 0);
					}
					else
					{
						int x = (size.Width - otherWidth) / 2;
						_controlStyleControlGridLayoutState._bounds = new Rectangle(x, size.Height + 7, otherWidth, otherHeight + 8);
						_gridLayoutState._bounds = Rectangle.Empty;
					}
				}
				else if (_todayButtonText.Length > 0)
				{
					int x2 = (size.Width - width) / 2;
					_gridLayoutState._bounds = new Rectangle(x2, size.Height + 7, width, otherHeight + 8);
					_controlStyleControlGridLayoutState._bounds = Rectangle.Empty;
				}
				graphics.Dispose();
				graphics = null;
			}
			else
			{
				_footerBounds = Rectangle.Empty;
				_gridLayoutState._bounds = Rectangle.Empty;
				_controlStyleControlGridLayoutState._bounds = Rectangle.Empty;
			}
			base.Size = new Size(size.Width + 2, height + 2);
		}

		public void SetVisibleMonth(DateTime dateTime)
		{
			_dateTime = new DateTime(dateTime.Year, dateTime.Month, 1);
			RebuildMonthBlocks(false);
			FillMonthBlocks();
			Invalidate();
		}

		private void RebuildMonthBlocks(bool condition)
		{
			Point point = new Point(0, 0);
			int value = 0;
			if (_calendarLayoutState.GetBlockSize().Width == 0 || _calendarLayoutState.GetBlockSize().Height == 0)
			{
				DisposeResources();
			}
			if (!condition && _monthBlocks.GetUpperBound(0) + 1 == _monthRows && _monthBlocks.GetUpperBound(1) + 1 == _monthColumns)
			{
				return;
			}
			_monthBlocks = new CalendarLayoutState[_monthRows, _monthColumns];
			for (int i = 0; i < _monthRows; i++)
			{
				for (int j = 0; j < _monthColumns; j++)
				{
					CalendarLayoutState calendarLayoutState = new CalendarLayoutState();
					calendarLayoutState.SetIndex(value);
					calendarLayoutState.SetTitleRectangle(_calendarLayoutState.GetTitleLayout()._bounds);
					calendarLayoutState.LayoutCells(_dayCellWidth, _dayCellHeight, _sideColumnWidth, point);
					_monthBlocks[i, j] = calendarLayoutState;
					point.X += _calendarLayoutState.GetBlockSize().Width;
					value++;
				}
				point.X = 0;
				point.Y += _calendarLayoutState.GetBlockSize().Height;
			}
			FillMonthBlocks();
			ref Rectangle reference = ref _arrowRectangles[0];
			reference = new Rectangle(1, 1, _titleHeight - 2, _titleHeight - 2);
			ref Rectangle rectangle = ref _arrowRectangles[1];
			rectangle = new Rectangle(base.ClientRectangle.Right - _titleHeight - 1, 1, _titleHeight - 2, _titleHeight - 2);
			Invalidate();
		}

		private CalendarLayoutState FindMonthBlockByDate(object value)
		{
			if (value == null || !(value is DateTime))
			{
				return null;
			}
			if (GridHelper.CompareDates(value, _calendarValueState.GetStart()) < 0 || GridHelper.CompareDates(value, _calendarValueState.GetEnd()) > 0)
			{
				return null;
			}
			for (int i = 0; i < _monthRows; i++)
			{
				for (int j = 0; j < _monthColumns; j++)
				{
					CalendarLayoutState calendarLayoutState = _monthBlocks[i, j];
					if (GridHelper.CompareDates(value, calendarLayoutState.GetVisibleRange().GetStart()) >= 0 && GridHelper.CompareDates(value, calendarLayoutState.GetVisibleRange().GetEnd()) <= 0)
					{
						return calendarLayoutState;
					}
				}
			}
			return null;
		}

		private CalendarDayCell FindDayCellByDateInBlocks(object value)
		{
			if (value == null || !(value is DateTime))
			{
				return null;
			}
			if (GridHelper.CompareDates(value, _calendarValueState.GetStart()) < 0 || GridHelper.CompareDates(value, _calendarValueState.GetEnd()) > 0)
			{
				return null;
			}
			return FindMonthBlockByDate(value)?.FindDayCellByDate(value);
		}

		private static int CountDaysInclusive(DateTime dateTime, DateTime otherDateTime)
		{
			DateTime date = dateTime.Date;
			DateTime otherDate = otherDateTime.Date;
			int days = date.Subtract(otherDate).Days;
			if (days < 0)
			{
				days = -days;
			}
			return days + 1;
		}

		internal bool HandleKey(Keys keys)
		{
			return ApplyKey(keys);
		}

		private bool ApplyKey(Keys keys)
		{
			switch (keys)
			{
			case Keys.Prior:
				SetSelectedValue(_controlStyleControlDateTime.AddMonths(-1));
				break;
			case Keys.Next:
				SetSelectedValue(_controlStyleControlDateTime.AddMonths(1));
				break;
			case Keys.Down:
				SetSelectedValue(_controlStyleControlDateTime.AddDays(7.0));
				break;
			case Keys.Up:
				SetSelectedValue(_controlStyleControlDateTime.AddDays(-7.0));
				break;
			case Keys.Right:
				SetSelectedValue(_controlStyleControlDateTime.AddDays(1.0));
				break;
			case Keys.Left:
				SetSelectedValue(_controlStyleControlDateTime.AddDays(-1.0));
				break;
			case Keys.Return:
				_isValueEmpty = false;
				CloseDropDown(false);
				break;
			case Keys.F4:
			case Keys.Up | Keys.Alt:
				CloseDropDown(false);
				break;
			case Keys.Escape:
				CloseDropDown(true);
				break;
			case Keys.Tab:
				return false;
			default:
				return false;
			}
			return true;
		}

		public string GetTodayButtonText()
		{
			return _todayButtonText;
		}

		public void SetTodayButtonText(string text)
		{
			if (text != _todayButtonText)
			{
				_todayButtonText = text;
			}
		}

		public string GetNoneButtonText()
		{
			return _noneButtonText;
		}

		public void SetNoneButtonText(string controlStyleControlText)
		{
			if (controlStyleControlText != _noneButtonText)
			{
				_noneButtonText = controlStyleControlText;
			}
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == WindowMessages.CancelMode)
			{
				CloseDropDown(true);
				base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
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

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new CalendarDropDownAccessibleObject(this);
		}
	}
}
