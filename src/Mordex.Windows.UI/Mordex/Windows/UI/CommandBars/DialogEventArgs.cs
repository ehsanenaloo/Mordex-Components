using System;
using System.Windows.Forms;

namespace Mordex.Windows.UI.CommandBars
{
	public class DialogEventArgs : EventArgs
	{
		private Form _dialog;

		public Form Dialog => _dialog;

		internal DialogEventArgs(Form dialog)
		{
			_dialog = dialog;
		}
	}
}
