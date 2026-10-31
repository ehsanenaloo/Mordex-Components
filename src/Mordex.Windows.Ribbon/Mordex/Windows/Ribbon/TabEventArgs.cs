using System;

namespace Mordex.Windows.Ribbon
{
	public class TabEventArgs : EventArgs
	{
		private RibbonTab _prevTab;

		private RibbonTab _tab;

		public RibbonTab PreviousTab => _prevTab;

		public RibbonTab Tab => _tab;

		public TabEventArgs(RibbonTab tab)
		{
			_tab = tab;
		}

		internal TabEventArgs(RibbonTab prevTab, RibbonTab tab)
		{
			_prevTab = prevTab;
			_tab = tab;
		}
	}
}
