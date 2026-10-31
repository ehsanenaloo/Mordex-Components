using System;

namespace Mordex.Windows.UI.Docking
{
	public class MdiTabGroupCreatedEventArgs : EventArgs
	{
		private MxMdiTabGroup _group;

		private MdiChildTab _tab;

		public MxMdiTabGroup Group => _group;

		public MdiChildTab Tab => _tab;

		internal MdiTabGroupCreatedEventArgs(MxMdiTabGroup group, MdiChildTab tab)
		{
			_group = group;
			_tab = tab;
		}
	}
}
