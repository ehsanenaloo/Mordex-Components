using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class NumericEdit
	{
		private TypeCode GetValueType()
		{
			return (TypeCode)_valueType;
		}

		protected virtual void OnValueChanged(EventArgs e)
		{
			if (_valueChangedHandler != null)
			{
				_valueChangedHandler(this, e);
			}
		}

		private bool IsInsideColumnContainer()
		{
			for (Control control = GridHelper.GetParentSafe(this); control != null; control = GridHelper.GetParentSafe(control))
			{
				if (control is GridColumnContainerControl)
				{
					return true;
				}
			}
			return false;
		}

		private ArrayList ComputeArrayList(string text)
		{
			bool areParenthesesAllowed = AreParenthesesAllowed();
			bool condition = false;
			bool allowNegative = _allowNegative;
			bool otherCondition = _formatMask == NumericEditFormatMask.Currency;
			bool nextCondition = true;
			bool previousCondition = false;
			bool firstCondition = GetEffectiveDecimalDigits() != 0;
			bool secondCondition = _formatMask == NumericEditFormatMask.Percent;
			bool thirdCondition = false;
			bool fourthCondition = false;
			ArrayList arrayList = new ArrayList();
			int startIndex = 0;
			while (startIndex < text.Length)
			{
				NumberToken a;
				if (areParenthesesAllowed)
				{
					a = ParseToken(text, startIndex, NumberCategory.OpenParentheses);
					if (a != null)
					{
						startIndex += a._slot.Length;
						areParenthesesAllowed = false;
						condition = true;
						allowNegative = false;
						arrayList.Add(a);
						continue;
					}
				}
				if (allowNegative)
				{
					a = ParseToken(text, startIndex, NumberCategory.Sign);
					if (a != null)
					{
						startIndex += a._slot.Length;
						allowNegative = false;
						arrayList.Add(a);
						nextCondition = !thirdCondition;
						areParenthesesAllowed = false;
						secondCondition = secondCondition && nextCondition;
						continue;
					}
				}
				if (otherCondition)
				{
					a = ParseToken(text, startIndex, NumberCategory.CurrencySymbol);
					if (a != null)
					{
						startIndex += a._slot.Length;
						otherCondition = false;
						arrayList.Add(a);
						nextCondition = !thirdCondition;
						continue;
					}
				}
				if (nextCondition)
				{
					a = ParseToken(text, startIndex, NumberCategory.DigitGroup);
					if (a != null)
					{
						startIndex += a._slot.Length;
						thirdCondition = true;
						arrayList.Add(a);
						previousCondition = !fourthCondition && _formatMask != NumericEditFormatMask.General;
						continue;
					}
				}
				if (firstCondition)
				{
					a = ParseToken(text, startIndex, NumberCategory.DecimalSepator);
					if (a != null)
					{
						startIndex += a._slot.Length;
						previousCondition = false;
						firstCondition = false;
						fourthCondition = true;
						arrayList.Add(a);
						continue;
					}
				}
				if (previousCondition)
				{
					a = ParseToken(text, startIndex, NumberCategory.GroupSeparator);
					if (a != null)
					{
						startIndex += a._slot.Length;
						arrayList.Add(a);
						continue;
					}
				}
				if (condition)
				{
					a = ParseToken(text, startIndex, NumberCategory.CloseParentheses);
					if (a != null)
					{
						startIndex += a._slot.Length;
						areParenthesesAllowed = false;
						condition = false;
						firstCondition = false;
						previousCondition = false;
						nextCondition = false;
						secondCondition = false;
						arrayList.Add(a);
						continue;
					}
				}
				if (secondCondition)
				{
					if (!thirdCondition)
					{
						if (GetNumberFormat().PercentNegativePattern != 2)
						{
							_ = GetNumberFormat().PercentPositivePattern;
						}
					}
					else if (GetNumberFormat().PercentNegativePattern != 2)
					{
						_ = GetNumberFormat().PercentPositivePattern;
					}
					a = ParseToken(text, startIndex, NumberCategory.PercentSymbol);
					if (a != null)
					{
						startIndex += a._slot.Length;
						nextCondition = !thirdCondition;
						secondCondition = false;
						firstCondition = !thirdCondition;
						previousCondition = !thirdCondition && _formatMask != NumericEditFormatMask.General;
						arrayList.Add(a);
						continue;
					}
				}
				a = ParseToken(text, startIndex, NumberCategory.Space);
				if (a != null)
				{
					startIndex += a._slot.Length;
					arrayList.Add(a);
					nextCondition = !thirdCondition;
					continue;
				}
				arrayList.Add(new NumberToken(NumberCategory.InvalidData, text.Substring(startIndex)));
				break;
			}
			return arrayList;
		}
	}
}
