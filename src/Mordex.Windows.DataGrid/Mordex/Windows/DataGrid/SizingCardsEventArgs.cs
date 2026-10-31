using System;

namespace Mordex.Windows.DataGrid
{
	public class SizingCardsEventArgs : EventArgs
	{
		private int _proposedWidth;

		private bool _cancel;

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

		internal SizingCardsEventArgs(int proposedWidth, bool cancel)
		{
			_proposedWidth = proposedWidth;
			_cancel = cancel;
		}
	}
}
