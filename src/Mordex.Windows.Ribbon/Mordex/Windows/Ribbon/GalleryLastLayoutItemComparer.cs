using System;
using System.Collections;

namespace Mordex.Windows.Ribbon
{
	internal class GalleryLastLayoutItemComparer : IComparer
	{
		internal GalleryLastLayoutItemComparer()
		{
		}

		public int Compare(object value, object otherValue)
		{
			if (!(value is CommandBaseRenderer) || !(otherValue is CommandBaseRenderer))
			{
				throw new InvalidOperationException();
			}
			CommandBaseRenderer commandBaseRenderer = (CommandBaseRenderer)value;
			CommandBaseRenderer otherRenderer = (CommandBaseRenderer)otherValue;
			if (commandBaseRenderer.CommandType == CommandType.Gallery && otherRenderer.CommandType != CommandType.Gallery)
			{
				return 1;
			}
			if (otherRenderer.CommandType == CommandType.Gallery && commandBaseRenderer.CommandType != CommandType.Gallery)
			{
				return -1;
			}
			return ((RibbonLayoutItem)commandBaseRenderer).GetItemIndex().CompareTo(((RibbonLayoutItem)otherRenderer).GetItemIndex());
		}
	}
}
