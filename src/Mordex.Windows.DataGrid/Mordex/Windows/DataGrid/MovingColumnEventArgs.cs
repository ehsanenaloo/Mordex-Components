using System;

namespace Mordex.Windows.DataGrid
{
	public class MovingColumnEventArgs : EventArgs
	{
		private bool _cancel;

		private readonly GridColumn _column;

		private int _proposedPosition;

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

		public GridColumn Column => _column;

		public int ProposedPosition
		{
			get
			{
				return _proposedPosition;
			}
			set
			{
				_proposedPosition = value;
			}
		}

		internal MovingColumnEventArgs(GridColumn column, int proposedPosition)
		{
			_column = column;
			_proposedPosition = proposedPosition;
			_cancel = false;
		}
	}
}
