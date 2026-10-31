using System;

namespace Mordex.Windows.DataGrid
{
	public class SizingColumnEventArgs : EventArgs
	{
		private readonly GridColumn _column;

		private int _proposedWidth;

		private bool _cancel;

		public GridColumn Column => _column;

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

		internal SizingColumnEventArgs(GridColumn column, int proposedWidth, bool cancel)
		{
			_column = column;
			_proposedWidth = proposedWidth;
			_cancel = cancel;
		}
	}
}
