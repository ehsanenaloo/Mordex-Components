using System;

namespace Mordex.Windows.DataGrid
{
	public class ErrorEventArgs : EventArgs
	{
		private readonly Exception _exception;

		private bool _displayErrorMessage = true;

		private string _errorMessage;

		public Exception Exception => _exception;

		public bool DisplayErrorMessage
		{
			get
			{
				return _displayErrorMessage;
			}
			set
			{
				_displayErrorMessage = value;
			}
		}

		public string ErrorMessage
		{
			get
			{
				return _errorMessage;
			}
			set
			{
				_errorMessage = value;
			}
		}

		internal ErrorEventArgs(Exception exception)
		{
			_exception = exception;
			_errorMessage = exception.Message;
		}
	}
}
