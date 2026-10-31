using System;

namespace Mordex.Windows.DataGrid
{
	public class NotInListEventArgs : EventArgs
	{
		private readonly GridColumn _column;

		private readonly string _enteredText;

		private bool _cancel;

		public GridColumn Column => _column;

		public string Text => _enteredText;

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

		internal NotInListEventArgs(GridColumn column, string text, bool cancel)
		{
			_column = column;
			_enteredText = text;
			_cancel = cancel;
		}
	}
}
