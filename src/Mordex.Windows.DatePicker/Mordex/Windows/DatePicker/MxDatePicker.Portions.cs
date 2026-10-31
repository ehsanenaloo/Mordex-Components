using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mordex.Windows.DatePicker
{
	public partial class MxDatePicker
	{
		// Rebuilds the cached format portions and edit text when the thread culture changed since they were built.
		private void EnsureCulture()
		{
			if (!DatePickerHelper.IsCultureStale(_cultureStamp))
			{
				return;
			}
			BuildPortions();
			_calendar?.EnsureCulture();
		}

		private void BuildPortions()
		{
			_cultureStamp = CultureInfo.CurrentCulture;
			if (_editStyle == EditStyle.Free)
			{
				SyncTextBoxFromValue();
			}
			int activeIndex = -1;
			if (_activePortion != null && _portions != null)
			{
				activeIndex = Array.IndexOf(_portions, _activePortion);
			}
			List<DateFieldPortion> newPortions = new List<DateFieldPortion>();
			string pattern = GetFormatPattern();
			string currentToken = "";
			char previousChar = '\0';
			bool inQuote = false;
			char quoteChar = '\0';
			bool escapeNext = false;
			bool isLiteral = false;
			foreach (char currentChar in pattern)
			{
				bool escaped = escapeNext;
				escapeNext = false;
				if (!escaped && !inQuote && currentChar == '\\')
				{
					escapeNext = true;
					continue;
				}
				if (!escaped && (inQuote ? currentChar == quoteChar : currentChar == '\'' || currentChar == '"'))
				{
					inQuote = !inQuote;
					quoteChar = currentChar;
					if (inQuote && currentToken.Length > 0)
					{
						newPortions.Add(CreatePortion(isLiteral, currentToken));
						currentToken = "";
					}
				}
				else if (inQuote)
				{
					currentToken += currentChar;
					isLiteral = true;
				}
				else if (escaped)
				{
					if (!isLiteral)
					{
						AddPendingToken(newPortions, currentToken, false);
						currentToken = "";
					}
					currentToken += currentChar;
					isLiteral = true;
					previousChar = '\0';
					continue;
				}
				else if (IsFormatSpecifier(currentChar))
				{
					if (currentChar == previousChar)
					{
						currentToken += currentChar;
					}
					else
					{
						AddPendingToken(newPortions, currentToken, isLiteral);
						currentToken = currentChar.ToString();
					}
					isLiteral = false;
				}
				else if (currentChar == '/' || currentChar == ':')
				{
					AddPendingToken(newPortions, currentToken, false);
					newPortions.Add(CreatePortion(false, currentChar.ToString()));
					currentToken = "";
				}
				else
				{
					if (isLiteral)
					{
						currentToken += currentChar;
					}
					else
					{
						AddPendingToken(newPortions, currentToken, false);
						currentToken = currentChar.ToString();
					}
					isLiteral = true;
				}
				previousChar = currentChar;
			}
			if (currentToken.Length > 0)
			{
				newPortions.Add(CreatePortion(isLiteral, currentToken));
			}
			_portions = newPortions.ToArray();
			if (activeIndex != -1)
			{
				SetActivePortion(activeIndex < _portions.Length ? _portions[activeIndex] : null);
			}
			RequestLayout();
		}

		private DateFieldPortion CreatePortion(bool isLiteral, string token)
		{
			return new DateFieldPortion(isLiteral, token, _maskedValidation, UseCompatibleTextRendering);
		}

		private void AddPendingToken(List<DateFieldPortion> portions, string token, bool isLiteral)
		{
			if (token.Length > 0)
			{
				portions.Add(CreatePortion(isLiteral, token));
			}
		}

		private static bool IsFormatSpecifier(char character)
		{
			return "MydhHmst".IndexOf(character) >= 0;
		}

		private DateFieldPortion FindPortion(DatePortion datePortion)
		{
			if (datePortion == DatePortion.None)
			{
				return null;
			}
			if (_portions != null)
			{
				PortionType portionType = DatePickerHelper.ToPortionType(datePortion);
				DateFieldPortion[] portions = _portions;
				foreach (DateFieldPortion portion in portions)
				{
					if (portionType == portion.GetPortionType())
					{
						return portion;
					}
					if (datePortion == DatePortion.Hour && portion.GetPortionType() == PortionType.Hour24)
					{
						return portion;
					}
				}
			}
			return null;
		}

		private DateFieldPortion GetActivePortion()
		{
			return _activePortion;
		}

		private void SetActivePortion(DateFieldPortion newPortion)
		{
			if (newPortion == _activePortion)
			{
				return;
			}
			if (_activePortion != null)
			{
				if (_maskedValidation == MaskedValidation.OnChange)
				{
					DateTime value = _activePortion.ApplyTypedValue(_value);
					_activePortion.SetIsActive(false);
					_activePortion = null;
					if (!_isNullDate)
					{
						Value = value;
					}
				}
				else
				{
					_activePortion.SetIsActive(false);
					_activePortion = null;
				}
			}
			if ((newPortion != null && !newPortion.IsEditable()) || (_showCheckBox && !_checkedValue))
			{
				_activePortion = null;
			}
			else
			{
				_activePortion = newPortion;
			}
			if (_activePortion != null)
			{
				_activePortion.LoadFromDate(_value);
				_activePortion.SetIsActive(true);
			}
			RequestLayout();
		}

		private DateFieldPortion GetPortionAt(int x, int y)
		{
			bool hitNonEditable = false;
			DateFieldPortion[] portions = _portions;
			foreach (DateFieldPortion portion in portions)
			{
				if (hitNonEditable && portion.IsEditable())
				{
					return portion;
				}
				if (portion._bounds.Contains(x, y))
				{
					if (portion.IsEditable())
					{
						return portion;
					}
					hitNonEditable = true;
				}
			}
			return GetLastEditablePortion();
		}

		private void ActivateFirstPortion()
		{
			if (_portions == null)
			{
				return;
			}
			DateFieldPortion[] portions = _portions;
			foreach (DateFieldPortion portion in portions)
			{
				if (portion.IsEditable())
				{
					SetActivePortion(portion);
					break;
				}
			}
		}

		private void ActivateNextPortion()
		{
			if (_portions != null)
			{
				bool passedActive = false;
				DateFieldPortion[] portions = _portions;
				foreach (DateFieldPortion portion in portions)
				{
					if (portion.IsEditable() && passedActive)
					{
						SetActivePortion(portion);
						return;
					}
					if (portion == _activePortion)
					{
						passedActive = true;
					}
				}
			}
			ActivateFirstPortion();
		}

		private void ActivatePreviousPortion()
		{
			if (_portions != null)
			{
				bool passedActive = false;
				for (int index = _portions.Length - 1; index >= 0; index--)
				{
					DateFieldPortion portion = _portions[index];
					if (portion.IsEditable() && passedActive)
					{
						SetActivePortion(portion);
						return;
					}
					if (portion == _activePortion)
					{
						passedActive = true;
					}
				}
			}
			ActivateLastPortion();
		}

		private void ActivateLastPortion()
		{
			if (_portions == null)
			{
				return;
			}
			for (int index = _portions.Length - 1; index >= 0; index--)
			{
				DateFieldPortion portion = _portions[index];
				if (portion.IsEditable())
				{
					SetActivePortion(portion);
					break;
				}
			}
		}

		private DateFieldPortion GetLastEditablePortion()
		{
			if (_portions != null)
			{
				for (int index = _portions.Length - 1; index >= 0; index--)
				{
					DateFieldPortion portion = _portions[index];
					if (portion.IsEditable())
					{
						return portion;
					}
				}
			}
			return null;
		}
	}
}
