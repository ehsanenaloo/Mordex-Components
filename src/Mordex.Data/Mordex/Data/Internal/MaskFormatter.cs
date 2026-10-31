using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Mordex.Data.Internal
{
	internal class MaskFormatter
	{
		private struct MaskSlot
		{
			internal bool _required;

			internal string _character;

			internal bool _allowLetter;

			internal bool _allowDigit;

			internal bool _allowSpace;

			internal bool _allowSign;

			internal bool _allowAny;

			internal bool _isLiteral;
		}

		private string _mask;

		private DataSourceManager _manager;

		private bool _fillLeftToRight = true;

		private MaskSlot[] _slots = new MaskSlot[0];

		private bool _hasMask;

		internal MaskFormatter(DataSourceManager manager)
		{
			_manager = manager;
		}

		internal void SetMask(string text)
		{
			if (text == null)
			{
				text = "";
			}
			if (_mask != text)
			{
				_mask = text;
				ParseMask();
			}
		}

		internal string ApplyMask(object value)
		{
			string text = string.Concat(value);
			if (!_hasMask || text.Length == 0)
			{
				return text;
			}
			if (MatchesMask(text))
			{
				return BuildMaskedText();
			}
			return text;
		}

		private const char EscapeCharacter = (char)92;

		private const char RightToLeftMarker = '!';

		private void ParseMask()
		{
			if (_mask.Length == 0)
			{
				_hasMask = false;
				_slots = new MaskSlot[0];
				return;
			}
			_fillLeftToRight = true;
			_hasMask = true;
			List<MaskSlot> slots = new List<MaskSlot>(_mask.Length);
			bool escaped = false;
			foreach (char symbol in _mask)
			{
				if (escaped)
				{
					slots.Add(CreateLiteralSlot(symbol.ToString()));
					escaped = false;
				}
				else if (symbol == EscapeCharacter)
				{
					escaped = true;
				}
				else if (symbol == RightToLeftMarker)
				{
					_fillLeftToRight = false;
				}
				else
				{
					slots.Add(CreateSlot(symbol));
				}
			}
			_slots = slots.ToArray();
		}

		private static MaskSlot CreateLiteralSlot(string text)
		{
			return new MaskSlot { _isLiteral = true, _character = text };
		}

		private MaskSlot CreateSlot(char symbol)
		{
			CultureInfo culture = _manager.CurrentCulture;
			switch (symbol)
			{
			case '0':
				return new MaskSlot { _required = true, _allowDigit = true };
			case '9':
			case '#':
				return new MaskSlot { _allowDigit = true, _allowSign = true, _allowSpace = true };
			case 'L':
				return new MaskSlot { _required = true, _allowLetter = true };
			case '?':
				return new MaskSlot { _allowLetter = true };
			case 'A':
				return new MaskSlot { _required = true, _allowDigit = true, _allowLetter = true };
			case 'a':
				return new MaskSlot { _allowDigit = true, _allowLetter = true };
			case '&':
				return new MaskSlot { _required = true, _allowAny = true };
			case 'C':
				return new MaskSlot { _allowAny = true };
			case '.':
				return CreateLiteralSlot(culture.NumberFormat.NumberDecimalSeparator);
			case ',':
				return CreateLiteralSlot(culture.NumberFormat.NumberGroupSeparator);
			case ':':
				return CreateLiteralSlot(culture.DateTimeFormat.TimeSeparator);
			case '/':
				return CreateLiteralSlot(culture.DateTimeFormat.DateSeparator);
			default:
				return CreateLiteralSlot(symbol.ToString());
			}
		}

		private void ClearNonLiteralSlots()
		{
			if (!_hasMask)
			{
				return;
			}
			for (int i = 0; i < _slots.Length; i++)
			{
				if (!_slots[i]._isLiteral)
				{
					_slots[i]._character = "";
				}
			}
		}

		private bool MatchesMask(string text)
		{
			if (!MatchesFormattedText(text) && !MatchesRawText(text))
			{
				return false;
			}
			return true;
		}

		private bool MatchesFormattedText(string text)
		{
			if (text.Length > _slots.Length)
			{
				return false;
			}
			ClearNonLiteralSlots();
			int textPosition = 0;
			if (_fillLeftToRight)
			{
				for (int i = 0; i < _slots.Length; i++)
				{
					if (text.Length <= textPosition)
					{
						continue;
					}
					if (_slots[i]._isLiteral)
					{
						if (text.Substring(textPosition, 1) != _slots[i]._character)
						{
							return false;
						}
						textPosition++;
					}
					else if (!SlotAcceptsCharacter(i, text[textPosition]))
					{
						if (_slots[i]._required)
						{
							return false;
						}
					}
					else
					{
						_slots[i]._character = text.Substring(textPosition, 1);
						textPosition++;
					}
				}
				if (textPosition < text.Length)
				{
					return false;
				}
			}
			else
			{
				textPosition = text.Length - 1;
				for (int slot = _slots.Length - 1; slot >= 0; slot--)
				{
					if (textPosition >= 0)
					{
						if (_slots[slot]._isLiteral)
						{
							if (text.Substring(textPosition, 1) != _slots[slot]._character)
							{
								return false;
							}
							textPosition--;
						}
						else if (!SlotAcceptsCharacter(slot, text[textPosition]))
						{
							if (_slots[slot]._required)
							{
								return false;
							}
						}
						else
						{
							_slots[slot]._character = text.Substring(textPosition, 1);
							textPosition--;
						}
					}
				}
				if (textPosition >= 0)
				{
					return false;
				}
			}
			return AreSlotsValid();
		}

		private bool MatchesRawText(string text)
		{
			ClearNonLiteralSlots();
			if (_fillLeftToRight)
			{
				int textPosition = 0;
				for (int i = 0; i < _slots.Length; i++)
				{
					if (text.Length > textPosition && !_slots[i]._isLiteral)
					{
						if (!SlotAcceptsCharacter(i, text[textPosition]))
						{
							return false;
						}
						_slots[i]._character = text.Substring(textPosition, 1);
						textPosition++;
					}
				}
				if (textPosition < text.Length)
				{
					return false;
				}
			}
			else
			{
				int textPosition = text.Length - 1;
				for (int slot = _slots.Length - 1; slot >= 0; slot--)
				{
					if (textPosition >= 0 && !_slots[slot]._isLiteral)
					{
						if (!SlotAcceptsCharacter(slot, text[textPosition]))
						{
							return false;
						}
						_slots[slot]._character = text.Substring(textPosition, 1);
						textPosition--;
					}
				}
				if (textPosition != -1)
				{
					return false;
				}
			}
			return AreSlotsValid();
		}

		private string BuildMaskedText()
		{
			char value = ' ';
			StringBuilder stringBuilder = new StringBuilder(_slots.Length);
			for (int i = 0; i < _slots.Length; i++)
			{
				if (_slots[i]._character == null || _slots[i]._character.Length == 0)
				{
					stringBuilder.Append(value);
				}
				else
				{
					stringBuilder.Append(_slots[i]._character);
				}
			}
			return stringBuilder.ToString();
		}

		private bool SlotAcceptsCharacter(int integer, char character)
		{
			if (_slots[integer]._allowAny)
			{
				return true;
			}
			if (_slots[integer]._allowDigit && char.GetUnicodeCategory(character) == UnicodeCategory.DecimalDigitNumber)
			{
				return true;
			}
			if (_slots[integer]._allowLetter && (char.GetUnicodeCategory(character) == UnicodeCategory.LowercaseLetter || char.GetUnicodeCategory(character) == UnicodeCategory.UppercaseLetter))
			{
				return true;
			}
			if (_slots[integer]._allowSpace && char.GetUnicodeCategory(character) == UnicodeCategory.SpaceSeparator)
			{
				return true;
			}
			if (_slots[integer]._allowSign && (character.ToString(_manager.CurrentCulture) == "+" || character.ToString(_manager.CurrentCulture) == "-"))
			{
				return true;
			}
			return false;
		}

		private bool AreSlotsValid()
		{
			if (_hasMask)
			{
				for (int i = 0; i < _slots.Length; i++)
				{
					if (!_slots[i]._isLiteral)
					{
						if (_slots[i]._required && _slots[i]._character != null && _slots[i]._character.Length == 0)
						{
							return false;
						}
						if (_slots[i]._character != null && _slots[i]._character.Length > 0 && !SlotAcceptsCharacter(i, _slots[i]._character[0]))
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}
}
