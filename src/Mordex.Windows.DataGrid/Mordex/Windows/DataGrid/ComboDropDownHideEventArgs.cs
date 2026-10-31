using System;

namespace Mordex.Windows.DataGrid
{
	public class ComboDropDownHideEventArgs : EventArgs
	{
		private readonly bool _valueSelected;

		public bool ValueSelected => _valueSelected;

		internal ComboDropDownHideEventArgs(bool valueSelected)
		{
			_valueSelected = valueSelected;
		}
	}
}
