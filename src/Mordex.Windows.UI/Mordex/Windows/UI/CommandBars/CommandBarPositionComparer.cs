using System;
using System.Collections;

namespace Mordex.Windows.UI.CommandBars
{
	internal class CommandBarPositionComparer : IComparer
	{
		public bool _isHorizontal = true;

		internal CommandBarPositionComparer(bool horizontal)
		{
			_isHorizontal = horizontal;
		}

		public int Compare(object value, object secondValue)
		{
			if (!(value is MxCommandBar) || !(secondValue is MxCommandBar))
			{
				throw new InvalidOperationException();
			}
			int comparison = 0;
			int leftValue = ((MxCommandBar)value)._rowIndex;
			int rightValue = ((MxCommandBar)secondValue)._rowIndex;
			comparison = leftValue.CompareTo(rightValue);
			if (comparison != 0)
			{
				return comparison;
			}
			if (_isHorizontal)
			{
				leftValue = ((MxCommandBar)value).GetPosition().X;
				rightValue = ((MxCommandBar)secondValue).GetPosition().X;
				comparison = leftValue.CompareTo(rightValue);
				if (comparison == 0)
				{
					leftValue = ((MxCommandBar)value).Location.X;
					rightValue = ((MxCommandBar)secondValue).Location.X;
					comparison = leftValue.CompareTo(rightValue);
				}
			}
			else
			{
				leftValue = ((MxCommandBar)value).GetPosition().Y;
				rightValue = ((MxCommandBar)secondValue).GetPosition().Y;
				comparison = leftValue.CompareTo(rightValue);
				if (comparison == 0)
				{
					leftValue = ((MxCommandBar)value).Location.Y;
					rightValue = ((MxCommandBar)secondValue).Location.Y;
					comparison = leftValue.CompareTo(rightValue);
				}
			}
			return comparison;
		}
	}
}
