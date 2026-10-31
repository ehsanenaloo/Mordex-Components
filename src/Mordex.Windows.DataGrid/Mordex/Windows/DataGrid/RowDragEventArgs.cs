using System;
using System.Windows.Forms;

namespace Mordex.Windows.DataGrid
{
	public class RowDragEventArgs : EventArgs
	{
		private readonly MouseButtons _button;

		public MouseButtons Button => _button;

		internal RowDragEventArgs(MouseButtons button)
		{
			_button = button;
		}
	}
}
