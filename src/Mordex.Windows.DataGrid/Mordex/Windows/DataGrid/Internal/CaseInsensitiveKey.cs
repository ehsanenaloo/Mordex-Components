using System;

namespace Mordex.Windows.DataGrid.Internal
{
	internal class CaseInsensitiveKey
	{
		private object _keyValue;

		internal CaseInsensitiveKey(object value)
		{
			if (value is string)
			{
				_keyValue = ((string)value).ToUpperInvariant();
			}
			else
			{
				_keyValue = value;
			}
		}

		public object GetKeyValue()
		{
			return _keyValue;
		}

		public override bool Equals(object obj)
		{
			CaseInsensitiveKey state = (CaseInsensitiveKey)obj;
			if (_keyValue is string && state._keyValue is string)
			{
				return string.Compare((string)_keyValue, (string)state._keyValue, ignoreCase: true) == 0;
			}
			if (_keyValue != null)
			{
				return _keyValue.Equals(state._keyValue);
			}
			if (state._keyValue == null)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (_keyValue == null || _keyValue is DBNull)
			{
				return 0;
			}
			return _keyValue.GetHashCode();
		}
	}
}
