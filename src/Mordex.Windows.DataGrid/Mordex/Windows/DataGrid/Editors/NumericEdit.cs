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
	[ToolboxItem(false)]
	public partial class NumericEdit : TextBox
	{
		private enum NumberCategory
		{
			DigitGroup,
			GroupSeparator,
			DecimalSepator,
			Sign,
			CurrencySymbol,
			OpenParentheses,
			CloseParentheses,
			PercentSymbol,
			Space,
			InvalidData
		}

		private class NumberToken
		{
			internal NumberCategory _category;

			internal string _slot;

			internal bool _isNegative;

			internal NumberToken(NumberCategory nc, string slot)
			{
				_slot = slot;
				_category = nc;
				_isNegative = false;
			}
		}

		internal class NumericEditAccessibleObject : ControlAccessibleObject
		{
			public override AccessibleRole Role => AccessibleRole.Text;

			public override AccessibleObject Parent => base.Owner.Parent.AccessibilityObject;

			public override string Value
			{
				get
				{
					return ((NumericEdit)base.Owner).Text;
				}
				set
				{
					((NumericEdit)base.Owner).Text = value;
				}
			}

			internal NumericEditAccessibleObject(NumericEdit owner)
				: base(owner)
			{
			}
		}

		private int _decimalDigits = -1;

		private NumericEditMode _editMode;

		private bool _emptyTextAsString;

		private NumericEditNullBehavior _nullBehavior = NumericEditNullBehavior.NoNulls;

		private object _currentValue;

		private EventHandler _valueChangedHandler;

		private NumericEditValueType _valueType = NumericEditValueType.Decimal;

		private NumericEditFormatMask _formatMask = NumericEditFormatMask.Number;

		private string _formatStringOverride = "";

		private bool _isUpdatingDisplay;

		private bool _isDecimalKeyDown;

		private bool _selectAllOnMouseDown;

		private bool _selectAllOnFirstFocus = true;

		internal bool _validateOnLeave = true;

		private string _committedText = "";

		private TriState _allowParentheses;

		private bool _allowNegative = true;

		private bool _isProgrammaticChange;

		public NumericEdit()
		{
			TextAlign = HorizontalAlignment.Right;
			Value = 0m;
			UpdateNumericEdit();
		}

		internal NumericEdit(bool checkLicense)
		{
			TextAlign = HorizontalAlignment.Right;
			Value = 0m;
			UpdateNumericEdit();
		}
	}
}
