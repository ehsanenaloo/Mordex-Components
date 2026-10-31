using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors.Calendar
{
	internal class CalendarEditControl : ControlStyleContainerControl
	{
		internal class CalendarAccessibleObject : ControlAccessibleObject
		{
			public override string Name => "Date Cell Editor";

			public override AccessibleRole Role
			{
				get
				{
					if (GetOwnerCalendar().GetEditStyle() == EditStyle.Combo)
					{
						return AccessibleRole.ComboBox;
					}
					return AccessibleRole.DropList;
				}
			}

			public override string Value
			{
				get
				{
					if (GridHelper.IsNullOrDBNull(GetOwnerCalendar().GetValueOrText()))
					{
						return "";
					}
					if (GetOwnerCalendar().GetValueOrText() is DateTime)
					{
						return ((DateTime)GetOwnerCalendar().GetValueOrText()).ToString("MM/dd/yyyy");
					}
					return string.Concat(GetOwnerCalendar().GetValueOrText());
				}
				set
				{
					DateTime? dateTime = null;
					if (!string.IsNullOrEmpty(value))
					{
						try
						{
							dateTime = DateTime.ParseExact(value, "MM/dd/yyyy", CultureInfo.InvariantCulture);
						}
						catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
						{
							GridExceptions.Report(caught);
						}
						if (dateTime.HasValue)
						{
							GetOwnerCalendar().AssignDateValue(dateTime.Value);
						}
						else
						{
							GetOwnerCalendar().AssignDateValue(null);
						}
					}
					else
					{
						GetOwnerCalendar().AssignDateValue(null);
					}
				}
			}

			internal CalendarAccessibleObject(CalendarEditControl owner)
				: base(owner)
			{
			}

			private CalendarEditControl GetOwnerCalendar()
			{
				return (CalendarEditControl)base.Owner;
			}
		}

		private CalendarAccessibleObject _accessibleObject;

		private ControlStyleControl _calendarDropControl;

		private object _dateValue;

		private bool _hasValueChanged;

		private IFormatProvider _iFormatProvider;

		private string _formatString = "d";

		private bool _isUpdatingText;

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				if (value != base.Text)
				{
					base.Text = value;
					DateTime result = DateTime.MinValue;
					if (DateTime.TryParse(base.Text, null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out result))
					{
						_dateValue = result;
					}
					else
					{
						_dateValue = null;
					}
				}
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (_accessibleObject == null)
			{
				_accessibleObject = new CalendarAccessibleObject(this);
			}
			return _accessibleObject;
		}

		public CalendarEditControl()
		{
			_iFormatProvider = GridHelper.GetCurrentCulture();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			UpdateDropControl();
			SetEditStyle(EditStyle.Combo);
		}

		private void UpdateDropControl()
		{
			_calendarDropControl = new ControlStyleControl();
			_calendarDropControl.SetTodayButtonText("");
			_calendarDropControl.SetNoneButtonText("");
			_calendarDropControl._calendarStyle = GetControlStyle();
			_calendarDropControl.Visible = false;
		}

		public void SetTodayButtonText(string text)
		{
			_calendarDropControl.SetTodayButtonText(text);
		}

		public void SetNoneButtonText(string text)
		{
			_calendarDropControl.SetNoneButtonText(text);
		}

		protected override void OnDropHide(bool cancel)
		{
			GridHelper.SelectAndFocus(this);
			if (!cancel)
			{
				if (_calendarDropControl.GetSelectedValue() == null)
				{
					AssignDateValue(null);
					Text = "";
				}
				else if (_dateValue == null || ((DateTime)_dateValue).Date != (DateTime)_calendarDropControl.GetSelectedValue())
				{
					DateTime dateTime = (DateTime)_calendarDropControl.GetSelectedValue();
					if (_dateValue != null)
					{
						dateTime = dateTime.Add(((DateTime)_dateValue).TimeOfDay);
					}
					AssignDateValue(dateTime);
					SelectRange(Text.Length, 0);
					OnDateSelected(new CalendarEventArgs((DateTime)_calendarDropControl.GetSelectedValue()));
				}
				AccessibilityNotifyClients(AccessibleEvents.Focus, -1);
				AccessibilityNotifyClients(AccessibleEvents.ValueChange, -1);
			}
			_calendarDropControl.CloseDropDown(cancel);
			if (_textBox is MaskEdit maskEdit)
			{
				maskEdit._isDropHideAllowed = true;
			}
			base.OnDropHide(cancel);
		}

		protected override void OnMouseDown(MouseEventArgs me)
		{
			base.OnMouseDown(me);
			if (me.Button == MouseButtons.Right)
			{
				AccessibilityNotifyClients(AccessibleEvents.ValueChange, -1);
			}
		}

		protected override void OnScrollMouseDown()
		{
			base.OnScrollMouseDown();
		}

		protected internal override Control GetDropControl()
		{
			return _calendarDropControl;
		}

		protected override void DropListControl(bool setCapture)
		{
			base.DropListControl(setCapture);
			Rectangle rectangle = GetVirtualClientRectangle();
			rectangle.Location = PointToScreen(rectangle.Location);
			if (_dateValue != null)
			{
				try
				{
					_calendarDropControl.SetSelectedValue(((DateTime)_dateValue).Date);
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					_calendarDropControl.SetSelectedValue(DateTime.Today);
				}
			}
			_calendarDropControl.SetSelectedStyle(_gridFormatStyle);
			_calendarDropControl.ShowDropDown(rectangle, this);
			GridHelper.SetCapture(_calendarDropControl, setCapture);
			if (_textBox is MaskEdit maskEdit)
			{
				maskEdit._isDropHideAllowed = false;
			}
		}

		protected override void OnMouseUp(MouseEventArgs me)
		{
			base.OnMouseUp(me);
			if (GetIsDroppedDown())
			{
				GridHelper.SetCapture(_calendarDropControl, true);
			}
		}

		protected internal virtual void OnDateSelected(CalendarEventArgs calendarEventArgs)
		{
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (GetIsDroppedDown() && keyData != Keys.Tab)
			{
				return _calendarDropControl.HandleKey(keyData);
			}
			return base.ProcessDialogKey(keyData);
		}

		public object GetValueOrText()
		{
			if (_dateValue == null && !string.IsNullOrEmpty(Text))
			{
				return Text;
			}
			return _dateValue;
		}

		public void AssignDateValue(object value)
		{
			if (value is string text)
			{
				Text = text;
				return;
			}
			if (value == DBNull.Value)
			{
				value = null;
			}
			if (value != _dateValue)
			{
				_dateValue = value;
				SetValueChanged(true);
				RefreshText();
				_dateValue = value;
				OnChange(EventArgs.Empty);
			}
		}

		internal void SetEditValue(object value)
		{
			if (value is string text)
			{
				Text = text;
				_hasValueChanged = false;
				return;
			}
			if (!GridHelper.IsNullOrDBNull(value) && !(value is DateTime))
			{
				value = null;
			}
			AssignDateValue(value);
			_hasValueChanged = false;
		}

		public void SetValueChanged(bool condition)
		{
			_hasValueChanged = condition;
		}

		private void RefreshText()
		{
			_isUpdatingText = true;
			if (GridHelper.IsNullOrDBNull(_dateValue))
			{
				base.Text = "";
			}
			else if (_formatString.Length > 0)
			{
				base.Text = ((DateTime)_dateValue).ToString(_formatString, _iFormatProvider);
			}
			else
			{
				base.Text = ((DateTime)_dateValue).ToString(_iFormatProvider);
			}
			_isUpdatingText = false;
		}

		public void SetFormatString(string text)
		{
			if (text != _formatString)
			{
				_formatString = text;
				RefreshText();
			}
		}

		protected override void OnTextChanged(EventArgs te)
		{
			base.OnTextChanged(te);
			try
			{
				if (string.IsNullOrEmpty(base.Text))
				{
					_dateValue = null;
				}
				else if (_formatString != null && _formatString.Length > 0)
				{
					DateTime result = DateTime.Today;
					if (DateTime.TryParseExact(base.Text, _formatString, null, DateTimeStyles.NoCurrentDateDefault, out result))
					{
						_dateValue = result;
					}
					else if (DateTime.TryParseExact(base.Text, _formatString, null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out result))
					{
						_dateValue = result;
					}
					else
					{
						_dateValue = DateTime.Parse(base.Text, null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
					}
				}
				else
				{
					_dateValue = DateTime.Parse(base.Text, null, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault);
				}
			}
			catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
			{
				GridExceptions.Report(caught);
				_dateValue = null;
			}
			SetValueChanged(true);
			if (!_isUpdatingText)
			{
				OnChange(EventArgs.Empty);
			}
		}

		protected override void OnControlStyleChanged()
		{
			if (_calendarDropControl != null)
			{
				_calendarDropControl._calendarStyle = GetControlStyle();
				if (GetControlStyle() != null && GetControlStyle()._printDocument is Control)
				{
					_calendarDropControl.Font = ((Control)GetControlStyle()._printDocument).Font;
				}
			}
		}
	}
}
