using System;

namespace Mordex.Windows.DatePicker
{
	public class ParseEventArgs
	{
		private bool _isNullDate;

		private DateTime _value;

		private string _text;

		public bool IsNullDate
		{
			get
			{
				return _isNullDate;
			}
			set
			{
				_isNullDate = value;
			}
		}

		public DateTime Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		public string Text => _text;

		internal ParseEventArgs(string text, DateTime value, bool isNullDate)
		{
			_text = text;
			_value = value;
			_isNullDate = isNullDate;
		}
	}
}
