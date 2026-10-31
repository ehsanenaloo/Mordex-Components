using System;
using System.ComponentModel;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		private static readonly object _checkedChangedEventKey = new object();

		private static readonly object _bindableValueChangedEventKey = new object();

		private static readonly object _parseEventKey = new object();

		private static readonly object _todayButtonClickEventKey = new object();

		private static readonly object _readOnlyChangedEventKey = new object();

		private static readonly object _noneButtonClickEventKey = new object();

		private static readonly object _valueChangedEventKey = new object();

		private static readonly object _dropDownEventKey = new object();

		private static readonly object _droppingDownEventKey = new object();

		private static readonly object _closeUpEventKey = new object();

		[Description("Event fired when the Checked property changes")]
		public event EventHandler CheckedChanged
		{
			add
			{
				base.Events.AddHandler(_checkedChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_checkedChangedEventKey, value);
			}
		}

		[Description("Event fired when the BindableValue property changes")]
		public event EventHandler BindableValueChanged
		{
			add
			{
				base.Events.AddHandler(_bindableValueChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_bindableValueChangedEventKey, value);
			}
		}

		[Description("Event fired when the Text in the control changes and before setting the value")]
		public event ParseEventHandler Parse
		{
			add
			{
				base.Events.AddHandler(_parseEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_parseEventKey, value);
			}
		}

		[Description("Event fired when the TodayButton is clicked.")]
		public event EventHandler TodayButtonClick
		{
			add
			{
				base.Events.AddHandler(_todayButtonClickEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_todayButtonClickEventKey, value);
			}
		}

		[Description("Event fired when the ReadOnly property has changed.")]
		public event EventHandler ReadOnlyChanged
		{
			add
			{
				base.Events.AddHandler(_readOnlyChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_readOnlyChangedEventKey, value);
			}
		}

		[Description("Event fired when the NoneButton is clicked.")]
		public event EventHandler NoneButtonClick
		{
			add
			{
				base.Events.AddHandler(_noneButtonClickEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_noneButtonClickEventKey, value);
			}
		}

		[Description("Event fired when the Value property changes")]
		public event EventHandler ValueChanged
		{
			add
			{
				base.Events.AddHandler(_valueChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_valueChangedEventKey, value);
			}
		}

		[Description("Event fired when the drop-down Calendar is shown.")]
		public event EventHandler DropDown
		{
			add
			{
				base.Events.AddHandler(_dropDownEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dropDownEventKey, value);
			}
		}

		[Description("Event fired when the DropDownCalendar is about to be displayed.")]
		public event CancelEventHandler DroppingDown
		{
			add
			{
				base.Events.AddHandler(_droppingDownEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_droppingDownEventKey, value);
			}
		}

		[Description("Event fired when the drop-down calendar is dismissed and disappears.")]
		public event EventHandler CloseUp
		{
			add
			{
				base.Events.AddHandler(_closeUpEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_closeUpEventKey, value);
			}
		}

		protected virtual void OnCheckedChanged(EventArgs e)
		{
			(base.Events[_checkedChangedEventKey] as EventHandler)?.Invoke(this, e);
		}

		protected virtual void OnBindableValueChanged(EventArgs e)
		{
			(base.Events[_bindableValueChangedEventKey] as EventHandler)?.Invoke(this, e);
		}

		protected virtual void OnParse(ParseEventArgs e)
		{
			(base.Events[_parseEventKey] as ParseEventHandler)?.Invoke(this, e);
		}

		protected virtual void OnTodayButtonClick(EventArgs e)
		{
			(base.Events[_todayButtonClickEventKey] as EventHandler)?.Invoke(this, e);
		}

		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			(base.Events[_readOnlyChangedEventKey] as EventHandler)?.Invoke(this, e);
		}

		protected virtual void OnNoneButtonClick(EventArgs e)
		{
			(base.Events[_noneButtonClickEventKey] as EventHandler)?.Invoke(this, e);
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			(base.Events[_valueChangedEventKey] as EventHandler)?.Invoke(this, e);
		}

		internal void RaiseTodayButtonClick()
		{
			OnTodayButtonClick(EventArgs.Empty);
		}

		internal void RaiseNoneButtonClick()
		{
			OnNoneButtonClick(EventArgs.Empty);
		}

		protected virtual void OnDropDown(EventArgs e)
		{
			(base.Events[_dropDownEventKey] as EventHandler)?.Invoke(this, e);
		}

		protected virtual void OnDroppingDown(CancelEventArgs e)
		{
			(base.Events[_droppingDownEventKey] as CancelEventHandler)?.Invoke(this, e);
		}

		protected virtual void OnCloseUp(EventArgs e)
		{
			(base.Events[_closeUpEventKey] as EventHandler)?.Invoke(this, e);
		}
	}
}
