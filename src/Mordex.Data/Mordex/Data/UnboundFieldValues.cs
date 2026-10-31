using System.Collections;
using System.Reflection;

namespace Mordex.Data
{
	[DefaultMember("Item")]
	internal class UnboundFieldValues
	{
		private Hashtable _values;

		internal UnboundFieldValues()
		{
		}

		internal bool IsEmpty()
		{
			if (_values != null)
			{
				return _values.Count == 0;
			}
			return true;
		}

		internal void RemoveValue(MordexFieldBase key)
		{
			if (_values != null && _values.ContainsKey(key))
			{
				_values.Remove(key);
			}
		}

		internal object GetValue(MordexFieldBase mordexFieldBase)
		{
			if (_values == null)
			{
				return null;
			}
			return _values[mordexFieldBase];
		}

		internal void SetValue(MordexFieldBase key, object value)
		{
			if (_values == null)
			{
				if (value != null)
				{
					_values = new Hashtable();
					_values.Add(key, value);
				}
			}
			else if (_values.ContainsKey(key))
			{
				_values[key] = value;
			}
			else
			{
				_values.Add(key, value);
			}
		}
	}
}
