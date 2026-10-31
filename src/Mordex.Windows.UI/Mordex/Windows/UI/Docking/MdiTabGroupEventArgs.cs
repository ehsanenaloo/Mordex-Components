using System;

namespace Mordex.Windows.UI.Docking
{
	public class MdiTabGroupEventArgs : EventArgs
	{
		private MxMdiTabGroup _group;

		public MxMdiTabGroup Group => _group;

		internal MdiTabGroupEventArgs(MxMdiTabGroup group)
		{
			_group = group;
		}
	}
}
