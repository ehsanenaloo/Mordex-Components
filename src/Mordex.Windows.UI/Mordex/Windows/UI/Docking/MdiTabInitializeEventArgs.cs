using System;

namespace Mordex.Windows.UI.Docking
{
	public class MdiTabInitializeEventArgs : EventArgs
	{
		private MdiChildTab _tab;

		public MdiChildTab Tab => _tab;

		internal MdiTabInitializeEventArgs(MdiChildTab tab)
		{
			_tab = tab;
		}
	}
}
