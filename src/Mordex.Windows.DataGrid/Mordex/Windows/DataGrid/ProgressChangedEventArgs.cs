using System;

namespace Mordex.Windows.DataGrid
{
	public class ProgressChangedEventArgs : EventArgs
	{
		private readonly int _progressPercentage;

		public int ProgressPercentage => _progressPercentage;

		public ProgressChangedEventArgs(int progressPercentage)
		{
			_progressPercentage = progressPercentage;
		}
	}
}
