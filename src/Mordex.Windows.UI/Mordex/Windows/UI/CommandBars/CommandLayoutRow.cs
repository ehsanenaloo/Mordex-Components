using System.Collections;

namespace Mordex.Windows.UI.CommandBars
{
	internal class CommandLayoutRow
	{
		private ArrayList _items;

		public bool _isSingleCommandLine;

		public int _length;

		public int _thickness;

		internal CommandLayoutRow()
		{
		}

		internal ArrayList GetItems()
		{
			if (_items == null)
			{
				_items = new ArrayList();
			}
			return _items;
		}
	}
}
