using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		private void OnCalendarEnabledChanged(object sender, EventArgs eventArgs)
		{
			SetDropDownEnabled(_calendar.Enabled);
		}

		internal bool IsDropDownEnabled()
		{
			if (base.Enabled)
			{
				return _dropDownEnabled;
			}
			return false;
		}

		internal void SetDropDownEnabled(bool isEnabled)
		{
			if (_dropDownEnabled != isEnabled)
			{
				_dropDownEnabled = isEnabled;
				Invalidate();
			}
		}

		private void OpenDropDown(bool captureMouse)
		{
			if (!base.Visible)
			{
				return;
			}
			EnsureCulture();
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Location = PointToScreen(clientRectangle.Location);
			if (BindableValue != DBNull.Value && DatePickerHelper.CompareValues(BindableValue, NullValue, null, Application.CurrentCulture) != 0)
			{
				_calendar.SetIsNullDateSelected(false);
				_calendar.SetSelectedDate(Value);
			}
			else
			{
				_calendar.SetSelectedDate(DateTime.Today);
				_calendar.SetIsNullDateSelected(true);
			}
			if (!IsNullDate)
			{
				_calendar.FirstMonth = Value;
			}
			else
			{
				_calendar.FirstMonth = DateTime.Today;
			}
			CancelEventArgs e = new CancelEventArgs();
			OnDroppingDown(e);
			if (!e.Cancel)
			{
				_droppedDown = true;
				OnDropDown(EventArgs.Empty);
				_calendar.ShowDropList(clientRectangle, this, _dropDownColumns, _dropDownRows);
				DatePickerHelper.SetMouseCapture(_calendar, captureMouse);
			}
		}

		void IDropListOwner.OnDropListHide(bool cancel)
		{
			CloseDropDown(cancel);
		}

		private void CloseDropDown(bool cancel)
		{
			if (_calendar != null)
			{
				_calendar.CloseDropList(cancel);
			}
			if (DroppedDown)
			{
				_droppedDown = false;
				if (_dropDownButton != null)
				{
					_dropDownButton.SetIsPressed(false);
				}
			}
			SetPressedButton(null);
			if (!cancel)
			{
				int hour = 0;
				int minute = 0;
				int second = 0;
				bool wasNullDate = IsNullDate;
				if (_calendar.GetIsNullDateSelected())
				{
					IsNullDate = true;
				}
				else
				{
					if (!wasNullDate)
					{
						hour = _value.Hour;
						minute = _value.Minute;
						second = _value.Second;
					}
					Value = new DateTime(_calendar.GetSelectedDate().Year, _calendar.GetSelectedDate().Month, _calendar.GetSelectedDate().Day, hour, minute, second);
				}
				if (_textBox != null)
				{
					_textBox.SelectAll();
				}
			}
			OnCloseUp(EventArgs.Empty);
			Invalidate();
		}
	}
}
