namespace Mordex.Windows.DataGrid
{
	public class RowCheckStateChangingEventArgs : RowCheckStateChangeEventArgs
	{
		private bool _cancel;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		internal RowCheckStateChangingEventArgs(GridRow row, CheckStateChangeType changeType, RowCheckState oldCheckState, RowCheckState checkState)
			: base(row, changeType, oldCheckState, checkState)
		{
		}
	}
}
