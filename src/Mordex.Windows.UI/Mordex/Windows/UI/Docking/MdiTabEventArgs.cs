using System;

namespace Mordex.Windows.UI.Docking
{
	public class MdiTabEventArgs : EventArgs
	{
		private MdiChildTab _tab;

		public MdiChildTab Tab => _tab;

		internal MdiTabEventArgs(MdiChildTab tab)
		{
			_tab = tab;
		}
	}
}
