using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Accessibility;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	[ToolboxItem(true)]
	[DefaultBindingProperty("Value")]
	public class MxIntegerUpDown : UpDownBase
	{
		internal class IntegerUpDownAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleRole Role => AccessibleRole.Text;

			public override string Value
			{
				get
				{
					return GetOwnerUpDown().Text;
				}
				set
				{
					GetOwnerUpDown().Text = value;
				}
			}

			internal IntegerUpDownAccessibleObject(MxIntegerUpDown ctl)
				: base(ctl)
			{
			}

			private MxIntegerUpDown GetOwnerUpDown()
			{
				return (MxIntegerUpDown)base.Owner;
			}

			public override int GetChildCount()
			{
				MxIntegerUpDown integerUpDown = GetOwnerUpDown();
				int childCount = 2;
				if (integerUpDown.EditControl != null)
				{
					childCount++;
				}
				return childCount;
			}

			public override AccessibleObject GetChild(int index)
			{
				MxIntegerUpDown integerUpDown = GetOwnerUpDown();
				if (integerUpDown.TextBox != null)
				{
					if (index == 0)
					{
						return integerUpDown.TextBox.AccessibilityObject;
					}
					index--;
				}
				switch (index)
				{
				case 0:
					return integerUpDown.GetFirstChildAccessibleObject();
				case 1:
					return integerUpDown.GetSecondChildAccessibleObject();
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}

		private int _integerUpDownValue;

		private bool _isTextSyncEnabled = true;

		private EventHandler _valueChangedHandler;

		private EventHandler _bindableValueChangedHandler;

		private int _minimum;

		private int _maximum = 100;

		private int _increment = 1;

		private bool _isNullValue;

		private NumericEditNullBehavior _nullBehavior = NumericEditNullBehavior.NoNulls;

		private NumericEdit _integerUpDownDefaultEditControl;

		private AccessibleObject _firstChildAccessible;

		private AccessibleObject _secondChildAccessible;

		private IntegerUpDownAccessibleObject _accessibleObject;

		[Bindable(BindableSupport.Yes)]
		[DefaultValue(0)]
		[Description("The value assigned to the control.")]
		public int Value
		{
			get
			{
				if (_integerUpDownDefaultEditControl != null)
				{
					int editControlValue = GetNumericEditValue();
					if (editControlValue < _minimum)
					{
						editControlValue = _minimum;
					}
					if (editControlValue > _maximum)
					{
						editControlValue = _maximum;
					}
					return editControlValue;
				}
				return _integerUpDownValue;
			}
			set
			{
				if (value < _minimum || value > _maximum)
				{
					throw new ArgumentOutOfRangeException("Value", value, "Value property must be a value between " + _minimum + " and " + _maximum);
				}
				if (_integerUpDownValue != value)
				{
					SetIsNullValue(false);
					ApplyClampedValue(value, true);
				}
				else if (GetIsNullValue())
				{
					SetIsNullValue(false);
					ApplyClampedValue(value, true);
					OnValueChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(0)]
		[Description("The minimum allowed value for the control.")]
		public int Minimum
		{
			get
			{
				return _minimum;
			}
			set
			{
				if (value > _maximum)
				{
					throw new InvalidOperationException("Minimum can not be greater than Maximum.");
				}
				if (_minimum == value)
				{
					return;
				}
				_minimum = value;
				if (_integerUpDownDefaultEditControl != null)
				{
					if (_minimum < 0)
					{
						_integerUpDownDefaultEditControl.SetAllowNegative(true);
					}
					else
					{
						_integerUpDownDefaultEditControl.SetAllowNegative(false);
					}
				}
				ApplyClampedValue(_integerUpDownValue, true);
			}
		}

		[Description("the maximum allowed value for the control.")]
		[DefaultValue(100)]
		public int Maximum
		{
			get
			{
				return _maximum;
			}
			set
			{
				if (value < _minimum)
				{
					throw new InvalidOperationException("Maximum can not be less than Minimum.");
				}
				if (_maximum != value)
				{
					_maximum = value;
					ApplyClampedValue(_integerUpDownValue, true);
				}
			}
		}

		[DefaultValue(1)]
		[Description("Indicates the amount to increment or decrement the Value when up or down buttons are clicked.")]
		public int Increment
		{
			get
			{
				return _increment;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException();
				}
				_increment = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Bindable(BindableSupport.Yes)]
		[Description("Represents an integer, or null (Nothing in Visual Basic), associated with the control.")]
		public object BindableValue
		{
			get
			{
				if (_isNullValue)
				{
					if (_nullBehavior == NumericEditNullBehavior.AllowDBNull)
					{
						return DBNull.Value;
					}
					if (_nullBehavior == NumericEditNullBehavior.AllowNull)
					{
						return null;
					}
				}
				return Value;
			}
			set
			{
				if (value == null || value == DBNull.Value)
				{
					if (_nullBehavior == NumericEditNullBehavior.NoNulls)
					{
						throw new ArgumentException("Value can not be null.");
					}
					SetIsNullValue(true);
					return;
				}
				if (!(value is int))
				{
					value = Convert.ToInt32(value);
				}
				SetIsNullValue(false);
				Value = (int)value;
			}
		}

		[DefaultValue(NumericEditNullBehavior.NoNulls)]
		[Description("Determines whether the control allows null values.")]
		public NumericEditNullBehavior NullBehavior
		{
			get
			{
				return _nullBehavior;
			}
			set
			{
				if (!Enum.IsDefined(typeof(NumericEditNullBehavior), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_nullBehavior != value)
				{
					_nullBehavior = value;
					if (_integerUpDownDefaultEditControl != null)
					{
						_integerUpDownDefaultEditControl.NullBehavior = value;
					}
					if (_nullBehavior == NumericEditNullBehavior.NoNulls && _isNullValue)
					{
						SetIsNullValue(false);
						Value = 0;
					}
				}
			}
		}

		protected override TextBox DefaultEditControl
		{
			get
			{
				SetNumericEdit(ComputeNumericEdit());
				return _integerUpDownDefaultEditControl;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override CharacterCasing CharacterCasing
		{
			get
			{
				return CharacterCasing.Normal;
			}
			set
			{
			}
		}

		protected override AccessibleObject AccessibleObjectCore
		{
			get
			{
				if (_accessibleObject == null)
				{
					_accessibleObject = new IntegerUpDownAccessibleObject(this);
				}
				return _accessibleObject;
			}
		}

		[Description("Event fired when the Value property has changed")]
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

		[Description("Event fired when the BindableValue property has changed")]
		public event EventHandler BindableValueChanged
		{
			add
			{
				EventHandler eventHandler = _bindableValueChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Combine(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _bindableValueChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
			remove
			{
				EventHandler eventHandler = _bindableValueChangedHandler;
				EventHandler otherEventHandler;
				do
				{
					otherEventHandler = eventHandler;
					EventHandler otherValue = (EventHandler)Delegate.Remove(otherEventHandler, value);
					eventHandler = Interlocked.CompareExchange(ref _bindableValueChangedHandler, otherValue, otherEventHandler);
				}
				while ((object)eventHandler != otherEventHandler);
			}
		}

		public MxIntegerUpDown()
		{
			base.Text = 0.ToString(GridHelper.GetCurrentCulture());
		}

		private void ApplyClampedValue(int integerUpDownValue, bool condition)
		{
			if (condition)
			{
				if (integerUpDownValue < _minimum)
				{
					integerUpDownValue = _minimum;
				}
				if (integerUpDownValue > _maximum)
				{
					integerUpDownValue = _maximum;
				}
			}
			_isTextSyncEnabled = false;
			if (GetIsNullValue())
			{
				if (_integerUpDownDefaultEditControl != null)
				{
					_integerUpDownDefaultEditControl.Value = BindableValue;
				}
				Text = "";
			}
			else if (integerUpDownValue != _integerUpDownValue)
			{
				if (_integerUpDownDefaultEditControl != null)
				{
					_integerUpDownDefaultEditControl.Value = integerUpDownValue;
				}
				_integerUpDownValue = integerUpDownValue;
				SetSuppressTextChange(true);
				OnValueChanged(EventArgs.Empty);
				OnBindableValueChanged(EventArgs.Empty);
				Text = _integerUpDownValue.ToString();
			}
			else if (_integerUpDownDefaultEditControl != null)
			{
				_integerUpDownDefaultEditControl.Value = integerUpDownValue;
			}
			else
			{
				Text = _integerUpDownValue.ToString();
			}
			_isTextSyncEnabled = true;
		}

		internal void LoadEditValue(object value)
		{
			BindableValue = value;
			if (_integerUpDownDefaultEditControl != null)
			{
				_integerUpDownDefaultEditControl.Value = value;
				_integerUpDownDefaultEditControl.SelectAll();
			}
			SetSuppressTextChange(false);
		}

		internal void ProcessCharacter(char character)
		{
			if (char.IsDigit(character))
			{
				try
				{
					Value = int.Parse(character.ToString(GridHelper.GetCurrentCulture()), GridHelper.GetCurrentCulture());
					if (_integerUpDownDefaultEditControl != null)
					{
						_integerUpDownDefaultEditControl.SelectionStart = _integerUpDownDefaultEditControl.Text.Length;
					}
					return;
				}
				catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
				{
					GridExceptions.Report(caught);
					return;
				}
			}
			if (character.ToString() == CultureInfo.CurrentCulture.NumberFormat.NegativeSign && _minimum < 0 && _integerUpDownDefaultEditControl != null)
			{
				_integerUpDownDefaultEditControl.ReplaceTextWithChar(character);
			}
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			if (_valueChangedHandler != null)
			{
				_valueChangedHandler(this, e);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled && e.KeyCode == Keys.Delete && _nullBehavior != NumericEditNullBehavior.NoNulls && !GetIsNullValue())
			{
				SetIsNullValue(true);
				OnValueChanged(EventArgs.Empty);
			}
		}

		protected virtual void OnBindableValueChanged(EventArgs e)
		{
			if (_bindableValueChangedHandler != null)
			{
				_bindableValueChangedHandler(this, e);
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			if (!_isTextSyncEnabled || _integerUpDownDefaultEditControl == null)
			{
				return;
			}
			if (!GridHelper.IsNullOrDBNull(_integerUpDownDefaultEditControl.Value))
			{
				SetSuppressTextChange(true);
				int integerUpDownValue = (int)_integerUpDownDefaultEditControl.Value;
				if (integerUpDownValue != _integerUpDownValue)
				{
					_isNullValue = false;
					ApplyClampedValue(integerUpDownValue, false);
				}
				else if (_isNullValue)
				{
					BindableValue = integerUpDownValue;
				}
			}
			else
			{
				if (_nullBehavior != NumericEditNullBehavior.NoNulls)
				{
					SetIsNullValue(true);
				}
				OnValueChanged(e);
			}
		}

		protected override void UpButton()
		{
			long next = ((!GetIsNullValue()) ? ((long)Value + _increment) : _minimum);
			if (next < _minimum)
			{
				next = _minimum;
			}
			if (next > _maximum)
			{
				next = _maximum;
			}
			Value = (int)next;
		}

		protected override void DownButton()
		{
			long next = ((!GetIsNullValue()) ? ((long)Value - _increment) : _maximum);
			if (next < _minimum)
			{
				next = _minimum;
			}
			if (next > _maximum)
			{
				next = _maximum;
			}
			Value = (int)next;
		}

		private int GetNumericEditValue()
		{
			if (_integerUpDownDefaultEditControl != null)
			{
				object value = _integerUpDownDefaultEditControl.Value;
				if (value is int)
				{
					return (int)value;
				}
				return _integerUpDownValue;
			}
			return _integerUpDownValue;
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
		}

		protected override void OnValidating(CancelEventArgs e)
		{
			ApplyClampedValue(GetNumericEditValue(), true);
			base.OnValidating(e);
		}

		private bool GetIsNullValue()
		{
			return _isNullValue;
		}

		private void SetIsNullValue(bool isNullValue)
		{
			if (_isNullValue != isNullValue)
			{
				_isNullValue = isNullValue;
				ApplyClampedValue(_integerUpDownValue, false);
				OnBindableValueChanged(EventArgs.Empty);
				OnValueChanged(EventArgs.Empty);
				SetSuppressTextChange(true);
			}
		}

		private NumericEdit ComputeNumericEdit()
		{
			NumericEdit numericEdit = new NumericEdit(checkLicense: false);
			if (_minimum < 0)
			{
				numericEdit.SetAllowNegative(true);
			}
			else
			{
				numericEdit.SetAllowNegative(false);
			}
			numericEdit.ValueType = NumericEditValueType.Int32;
			numericEdit.FormatMask = NumericEditFormatMask.General;
			numericEdit.NullBehavior = _nullBehavior;
			return numericEdit;
		}

		protected override void OnUpDownStyleChanged()
		{
			if (base.UpDownStyle == UpDownStyle.UpDownCombo)
			{
				if (GetNumericEdit() == null)
				{
					SetNumericEdit(ComputeNumericEdit());
				}
			}
			else if (GetNumericEdit() != null)
			{
				SetNumericEdit(null);
			}
			base.EditControl = GetNumericEdit();
		}

		private NumericEdit GetNumericEdit()
		{
			return _integerUpDownDefaultEditControl;
		}

		private void SetNumericEdit(NumericEdit integerUpDownDefaultEditControl)
		{
			if (_integerUpDownDefaultEditControl != integerUpDownDefaultEditControl)
			{
				_integerUpDownDefaultEditControl = integerUpDownDefaultEditControl;
			}
		}

		internal AccessibleObject GetFirstChildAccessibleObject()
		{
			if (_firstChildAccessible == null)
			{
				_firstChildAccessible = new EditBaseAccessibleObject(this, 0);
			}
			return _firstChildAccessible;
		}

		internal AccessibleObject GetSecondChildAccessibleObject()
		{
			if (_secondChildAccessible == null)
			{
				_secondChildAccessible = new EditBaseAccessibleObject(this, 1);
			}
			return _secondChildAccessible;
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return AccessibleObjectCore;
		}
	}
}
