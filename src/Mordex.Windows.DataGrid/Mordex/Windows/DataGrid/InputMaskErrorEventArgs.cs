namespace Mordex.Windows.DataGrid
{
	public class InputMaskErrorEventArgs
	{
		private InputMaskErrorAction _action;

		private readonly GridColumn _column;

		private string _errorText;

		public GridColumn Column => _column;

		public string Text
		{
			get
			{
				return _errorText;
			}
			set
			{
				_errorText = value;
			}
		}

		public InputMaskErrorAction Action
		{
			get
			{
				return _action;
			}
			set
			{
				_action = value;
			}
		}

		internal InputMaskErrorEventArgs(GridColumn column, string text, InputMaskErrorAction action)
		{
			_action = action;
			_errorText = text;
			_column = column;
		}
	}
}
