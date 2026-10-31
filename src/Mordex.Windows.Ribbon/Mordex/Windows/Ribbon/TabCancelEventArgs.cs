using System;

namespace Mordex.Windows.Ribbon
{
	public class TabCancelEventArgs : EventArgs
	{
		private RibbonTab _prevTab;

		private RibbonTab _tab;

		private bool _cancel;

		public RibbonTab PreviousTab => _prevTab;

		public RibbonTab Tab => _tab;

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

		internal TabCancelEventArgs(RibbonTab prevTab, RibbonTab tab)
		{
			_prevTab = prevTab;
			_tab = tab;
		}
	}
}
