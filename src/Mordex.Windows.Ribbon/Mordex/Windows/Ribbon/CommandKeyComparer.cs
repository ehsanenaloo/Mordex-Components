using System.Collections;

namespace Mordex.Windows.Ribbon
{
	internal class CommandKeyComparer : IComparer
	{
		internal CommandKeyComparer()
		{
		}

		public int Compare(object value, object otherValue)
		{
			if (value == null || otherValue == null || value == otherValue)
			{
				return 0;
			}
			string text = GetSortKey(value as CommandBase);
			string strB = GetSortKey(otherValue as CommandBase);
			return string.CompareOrdinal(text, strB);
		}

		private string GetSortKey(CommandBase commandBase)
		{
			string text = "";
			if (commandBase != null)
			{
				text = commandBase.Key;
				if (string.IsNullOrEmpty(text))
				{
					text = commandBase.Name;
				}
			}
			if (text == null)
			{
				text = "";
			}
			return text;
		}
	}
}
