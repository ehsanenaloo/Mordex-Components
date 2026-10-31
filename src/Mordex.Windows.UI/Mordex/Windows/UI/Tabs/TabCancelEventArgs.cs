using System;

namespace Mordex.Windows.UI.Tabs
{
	public class TabCancelEventArgs : EventArgs
	{
		private MxTabPage _previousPage;

		private MxTabPage _page;

		private bool _cancel;

		public MxTabPage PreviousPage => _previousPage;

		public MxTabPage Page => _page;

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

		internal TabCancelEventArgs(MxTabPage prevPage, MxTabPage page)
		{
			_previousPage = prevPage;
			_page = page;
		}
	}
}
