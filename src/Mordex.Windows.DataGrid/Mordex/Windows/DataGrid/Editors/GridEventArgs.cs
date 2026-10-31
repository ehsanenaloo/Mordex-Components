using System;

namespace Mordex.Windows.DataGrid.Editors
{
	internal class GridEventArgs : EventArgs
	{
		private readonly bool _valueSelected;

		internal GridEventArgs(bool valueSelected)
		{
			_valueSelected = valueSelected;
		}

		public bool GetValueSelected()
		{
			return _valueSelected;
		}
	}
}
