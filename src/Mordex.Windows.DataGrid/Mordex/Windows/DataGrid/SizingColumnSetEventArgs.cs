using System;

namespace Mordex.Windows.DataGrid
{
	public class SizingColumnSetEventArgs : EventArgs
	{
		private readonly GridColumnSet _columnSet;

		private int _proposedWidth;

		private bool _cancel;

		public GridColumnSet ColumnSet => _columnSet;

		public int ProposedWidth
		{
			get
			{
				return _proposedWidth;
			}
			set
			{
				_proposedWidth = value;
			}
		}

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

		internal SizingColumnSetEventArgs(GridColumnSet columnSet, int proposedWidth, bool cancel)
		{
			_columnSet = columnSet;
			_proposedWidth = proposedWidth;
			_cancel = cancel;
		}
	}
}
