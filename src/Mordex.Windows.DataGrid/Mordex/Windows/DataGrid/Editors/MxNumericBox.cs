using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid.Editors
{
	[DefaultBindingProperty("Value")]
	[ToolboxItem(true)]
	public class MxNumericBox : EditBase
	{
		private EventHandler _valueChangedHandler;

		protected override TextBox DefaultEditControl
		{
			get
			{
				NumericEdit numericEdit = new NumericEdit(checkLicense: false);
				numericEdit.ValueChanged += OnControlValueChanged;
				return numericEdit;
			}
		}

		protected override TextAlignment DefaultTextAlignment => TextAlignment.Far;

		[DefaultValue(TriState.Empty)]
		[Description("Indicates wheter user can enter parentheses for negative nuumbers.")]
		public TriState AllowParentheses
		{
			get
			{
				return GetNumericEdit().AllowParentheses;
			}
			set
			{
				GetNumericEdit().AllowParentheses = value;
			}
		}

		[DefaultValue(-1)]
		[Description("Indicates the number of digits the user can enter.")]
		public int DecimalDigits
		{
			get
			{
				return GetNumericEdit().DecimalDigits;
			}
			set
			{
				GetNumericEdit().DecimalDigits = value;
			}
		}

		[DefaultValue(NumericEditMode.FormattedText)]
		[Description("Determines whether the control should strip the format from the value when the control is being edited.")]
		public NumericEditMode EditMode
		{
			get
			{
				return GetNumericEdit().EditMode;
			}
			set
			{
				GetNumericEdit().EditMode = value;
			}
		}

		[Description("Determines which mask for numeric data is used in the control.")]
		[DefaultValue(NumericEditFormatMask.Number)]
		public NumericEditFormatMask FormatMask
		{
			get
			{
				return GetNumericEdit().FormatMask;
			}
			set
			{
				GetNumericEdit().FormatMask = value;
			}
		}

		[Description("Represents the format to use in the value when the editing operation has finished.")]
		[DefaultValue("")]
		public string FormatString
		{
			get
			{
				return GetNumericEdit().FormatString;
			}
			set
			{
				GetNumericEdit().FormatString = value;
			}
		}

		[DefaultValue(NumericEditNullBehavior.NoNulls)]
		[Description("Determines whether the control allows null values.")]
		public NumericEditNullBehavior NullBehavior
		{
			get
			{
				return GetNumericEdit().NullBehavior;
			}
			set
			{
				GetNumericEdit().NullBehavior = value;
			}
		}

		[Browsable(false)]
		[Bindable(BindableSupport.Yes)]
		[Description("Indicates the value for the control.")]
		public object Value
		{
			get
			{
				return GetNumericEdit().Value;
			}
			set
			{
				GetNumericEdit().Value = value;
			}
		}

		[Description("Determines the Type of the Value property.")]
		[DefaultValue(NumericEditValueType.Decimal)]
		public NumericEditValueType ValueType
		{
			get
			{
				return GetNumericEdit().ValueType;
			}
			set
			{
				GetNumericEdit().ValueType = value;
			}
		}

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.TextBox.Text = value;
			}
		}

		[Description("Event fired when the Value property changes")]
		public event EventHandler ValueChanged
		{
			add
			{
				EventHandler eventHandler = _valueChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _valueChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _valueChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _valueChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		public MxNumericBox()
		{
			base.Text = base.TextBox.Text;
		}

		private NumericEdit GetNumericEdit()
		{
			return (NumericEdit)base.TextBox;
		}

		private void OnControlValueChanged(object sender, EventArgs eventArgs)
		{
			OnValueChanged(eventArgs);
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			if (_valueChangedHandler != null)
			{
				_valueChangedHandler(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeValue()
		{
			if (Value != DBNull.Value)
			{
				return Value != null;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetValue()
		{
			Value = 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeText()
		{
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new void ResetText()
		{
			Value = 0;
		}
	}
}
