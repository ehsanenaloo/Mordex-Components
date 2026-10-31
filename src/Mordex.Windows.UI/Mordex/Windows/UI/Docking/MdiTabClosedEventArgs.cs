using System;

namespace Mordex.Windows.UI.Docking
{
	public class MdiTabClosedEventArgs : EventArgs
	{
		private MdiChildTab _tab;

		public MdiChildTab Tab => _tab;

		internal MdiTabClosedEventArgs(MdiChildTab tab)
		{
			_tab = tab;
		}
	}
}
