using System;
using Mordex.Data;

namespace Mordex.Windows.FilterBuilder
{
	public class FetchFieldValuesEventArgs : EventArgs
	{
		private TriState _hasValueList;

		private IMordexCondition _condition;

		private IMordexValueItemCollection _valueList;

		public TriState HasValueList
		{
			get
			{
				return _hasValueList;
			}
			set
			{
				_hasValueList = value;
			}
		}

		public IMordexCondition FilterCondition => _condition;

		public IMordexValueItemCollection ValueList
		{
			get
			{
				return _valueList;
			}
			set
			{
				_valueList = value;
			}
		}

		internal FetchFieldValuesEventArgs(IMordexCondition condition)
		{
			_condition = condition;
		}
	}
}
