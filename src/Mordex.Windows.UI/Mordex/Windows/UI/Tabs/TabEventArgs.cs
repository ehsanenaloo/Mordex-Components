using System;

namespace Mordex.Windows.UI.Tabs
{
	public class TabEventArgs : EventArgs
	{
		private MxTabPage _previousPage;

		private MxTabPage _page;

		public MxTabPage PreviousPage => _previousPage;

		public MxTabPage Page => _page;

		public TabEventArgs(MxTabPage page)
		{
			_page = page;
		}

		internal TabEventArgs(MxTabPage prevPage, MxTabPage page)
		{
			_previousPage = prevPage;
			_page = page;
		}
	}
}
