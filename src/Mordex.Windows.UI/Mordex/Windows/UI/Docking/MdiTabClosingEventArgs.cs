using System;

namespace Mordex.Windows.UI.Docking
{
	public class MdiTabClosingEventArgs : EventArgs
	{
		private MdiChildTab _tab;

		private bool _cancel;

		public MdiChildTab Tab => _tab;

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

		internal MdiTabClosingEventArgs(MdiChildTab tab)
		{
			_tab = tab;
		}
	}
}
