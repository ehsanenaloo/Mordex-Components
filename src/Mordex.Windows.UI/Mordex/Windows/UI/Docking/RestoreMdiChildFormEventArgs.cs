using System;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	public class RestoreMdiChildFormEventArgs : EventArgs
	{
		private MdiChildTab _tab;

		private Form _form;

		public MdiChildTab Tab => _tab;

		public Form Form
		{
			get
			{
				return _form;
			}
			set
			{
				_form = value;
			}
		}

		internal RestoreMdiChildFormEventArgs(MdiChildTab tab)
		{
			_tab = tab;
		}
	}
}
