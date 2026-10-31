using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	public partial class DatePickerCalendar
	{
		internal void ShowDropList(Rectangle anchorBounds, object owner, int columns, int rows)
		{
			_isDroppedDown = true;
			base.Parent = (MxDatePicker)owner;
			_dropListOwner = (IDropListOwner)owner;
			EnsureCulture();
			ComputeDroppedControlSize(columns, rows);
			MeasureCells();
			UpdateMonthLayouts(true);
			_previousParentHandle = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			NativeMethods.ShowTopMost(base.Handle);
			int x = anchorBounds.Right - base.Bounds.Width;
			int bottom = anchorBounds.Bottom;
			Point location = new Point(x, bottom);
			if (location.X + base.Width > SystemInformation.VirtualScreen.Right)
			{
				location.X = SystemInformation.VirtualScreen.Right - base.Width;
			}
			if (location.X < SystemInformation.VirtualScreen.Left)
			{
				location.X = SystemInformation.VirtualScreen.Left;
			}
			if (location.Y + base.Height > SystemInformation.VirtualScreen.Bottom)
			{
				if (anchorBounds.Y > SystemInformation.VirtualScreen.Bottom - anchorBounds.Bottom)
				{
					location.Y = anchorBounds.Y - base.Height;
					if (location.Y < SystemInformation.VirtualScreen.Top)
					{
						location.Y = SystemInformation.VirtualScreen.Top;
						base.Height = anchorBounds.Y;
					}
				}
				else
				{
					base.Height = SystemInformation.VirtualScreen.Bottom - anchorBounds.Bottom;
				}
			}
			Location = location;
			UpdateMonthLayouts(true);
			Visible = true;
		}

		internal void CloseDropList(bool cancel)
		{
			if (_isDroppedDown)
			{
				HideDropList();
				_isDroppedDown = false;
				if (_dropListOwner != null)
				{
					_dropListOwner.OnDropListHide(cancel);
					_dropListOwner = null;
				}
			}
		}

		internal void HideDropList()
		{
			_isDroppedDown = false;
			Hide();
			NativeMethods.SetParent(base.Handle, _previousParentHandle);
			DatePickerHelper.SetMouseCapture(this, false);
			_previousParentHandle = IntPtr.Zero;
		}

		internal string GetTodayButtonText()
		{
			return _todayButtonText;
		}

		internal void SetTodayButtonText(string text)
		{
			_todayButtonText = text;
		}

		internal string GetNoneButtonText()
		{
			return _noneButtonText;
		}

		internal void SetNoneButtonText(string text)
		{
			_noneButtonText = text;
		}

		internal void OnMonthPicked(DateTime? pickedMonth)
		{
			SetPressedMonthHeader(null);
			if (pickedMonth.HasValue)
			{
				FirstMonth = DatePickerHelper.AddMonthsClamped(pickedMonth.Value, -_pickedMonthIndex);
			}
			_scrollTimer.Interval = ScrollRepeatInterval;
			_scrollTimer.Enabled = false;
			if (_isDroppedDown)
			{
				DatePickerHelper.SetMouseCapture(this, true);
			}
		}

		private void OnTodayButtonClicked()
		{
			SetSelectedDate(DateTime.Today);
			MxDatePicker datePicker = _dropListOwner as MxDatePicker;
			CloseDropList(false);
			datePicker?.RaiseTodayButtonClick();
		}

		private void OnNoneButtonClicked()
		{
			_isNullDateSelected = true;
			MxDatePicker datePicker = _dropListOwner as MxDatePicker;
			CloseDropList(false);
			datePicker?.RaiseNoneButtonClick();
		}

		internal bool GetIsNullDateSelected()
		{
			return _isNullDateSelected;
		}

		internal void SetIsNullDateSelected(bool isNullDateSelected)
		{
			_isNullDateSelected = isNullDateSelected;
		}

		internal bool GetShowNoneButton()
		{
			return _showNoneButton;
		}

		internal void SetShowNoneButton(bool showNoneButton)
		{
			_showNoneButton = showNoneButton;
		}

		internal bool GetShowTodayButton()
		{
			return _showTodayButton;
		}

		internal void SetShowTodayButton(bool showTodayButton)
		{
			_showTodayButton = showTodayButton;
		}
	}
}
