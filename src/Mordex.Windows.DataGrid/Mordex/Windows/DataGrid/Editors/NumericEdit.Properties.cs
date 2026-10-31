using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class NumericEdit
	{
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool Multiline
		{
			get
			{
				return base.Multiline;
			}
			set
			{
			}
		}

		[DefaultValue(-1)]
		public int DecimalDigits
		{
			get
			{
				return _decimalDigits;
			}
			set
			{
				if (value < -1)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (_decimalDigits != value)
				{
					_decimalDigits = value;
					UpdateNumericEdit();
				}
			}
		}

		[DefaultValue(NumericEditMode.FormattedText)]
		public NumericEditMode EditMode
		{
			get
			{
				return _editMode;
			}
			set
			{
				_editMode = value;
			}
		}

		[DefaultValue(NumericEditNullBehavior.NoNulls)]
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
					if (_nullBehavior == NumericEditNullBehavior.NoNulls && (_currentValue == null || _currentValue == DBNull.Value))
					{
						Value = 0;
					}
				}
			}
		}

		[Browsable(false)]
		[Bindable(BindableSupport.Yes)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object Value
		{
			get
			{
				return _currentValue;
			}
			set
			{
				if (!_emptyTextAsString)
				{
					if (_nullBehavior == NumericEditNullBehavior.AllowDBNull)
					{
						if (value == null)
						{
							value = DBNull.Value;
						}
					}
					else if (_nullBehavior == NumericEditNullBehavior.AllowNull)
					{
						if (value == DBNull.Value)
						{
							value = null;
						}
					}
					else if (value == null || value == DBNull.Value)
					{
						value = 0;
					}
				}
				if (value == DBNull.Value)
				{
					value = DBNull.Value;
				}
				else if (value is string)
				{
					value = ParseText((string)value);
				}
				else if (value is IConvertible)
				{
					value = Convert.ChangeType(value, GetValueType());
				}
				bool condition = GridHelper.CompareValues(_currentValue, value, null, true) != 0;
				_currentValue = Value;
				if (condition || GridHelper.CompareValues(value, _currentValue, null, true) != 0)
				{
					_currentValue = value;
					UpdateNumericEdit();
					base.Modified = false;
					OnValueChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(NumericEditValueType.Decimal)]
		public NumericEditValueType ValueType
		{
			get
			{
				return _valueType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(NumericEditValueType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _valueType)
				{
					_valueType = value;
					try
					{
						Value = _currentValue;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						Value = 0;
					}
					UpdateNumericEdit();
				}
			}
		}

		[DefaultValue(NumericEditFormatMask.Number)]
		public NumericEditFormatMask FormatMask
		{
			get
			{
				return _formatMask;
			}
			set
			{
				if (!Enum.IsDefined(typeof(NumericEditFormatMask), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_formatMask != value)
				{
					_formatMask = value;
					UpdateNumericEdit();
					SelectAll();
				}
			}
		}

		[DefaultValue("")]
		public string FormatString
		{
			get
			{
				return _formatStringOverride;
			}
			set
			{
				if (_formatStringOverride != value)
				{
					_formatStringOverride = value;
					UpdateNumericEdit();
					SelectAll();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		public TriState AllowParentheses
		{
			get
			{
				return _allowParentheses;
			}
			set
			{
				_allowParentheses = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new string[] Lines
		{
			get
			{
				return null;
			}
			set
			{
				if (value == null)
				{
					Text = "";
				}
				else
				{
					Text = value[0];
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool AcceptsReturn
		{
			get
			{
				return false;
			}
			set
			{
				base.AcceptsReturn = false;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool AcceptsTab
		{
			get
			{
				return false;
			}
			set
			{
				base.AcceptsTab = false;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool WordWrap
		{
			get
			{
				return false;
			}
			set
			{
				base.WordWrap = false;
			}
		}

		[DefaultValue(HorizontalAlignment.Right)]
		public new HorizontalAlignment TextAlign
		{
			get
			{
				return base.TextAlign;
			}
			set
			{
				base.TextAlign = value;
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
				if (value != base.Text)
				{
					try
					{
						object parseText = ParseText(value);
						Value = parseText;
					}
					catch (Exception caught) when (GridExceptions.IsRecoverable(caught))
					{
						GridExceptions.Report(caught);
						Value = null;
					}
				}
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
	}
}
