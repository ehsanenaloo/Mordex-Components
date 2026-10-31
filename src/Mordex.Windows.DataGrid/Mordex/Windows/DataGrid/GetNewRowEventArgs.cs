using System;

namespace Mordex.Windows.DataGrid
{
	public class GetNewRowEventArgs : EventArgs
	{
		private object _newRow;

		public object NewRow
		{
			get
			{
				return _newRow;
			}
			set
			{
				_newRow = value;
			}
		}

		internal GetNewRowEventArgs()
		{
		}
	}
}
