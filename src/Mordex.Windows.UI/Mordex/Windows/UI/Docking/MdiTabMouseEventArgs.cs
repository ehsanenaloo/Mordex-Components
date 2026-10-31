using System;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking
{
	public class MdiTabMouseEventArgs : EventArgs
	{
		private MdiChildTab _tab;

		private int _clicks;

		private int _delta;

		private MouseButtons _button;

		private int _mouseX;

		private int _mouseY;

		public int Clicks => _clicks;

		public int Delta => _delta;

		public MouseButtons Button => _button;

		public int X => _mouseX;

		public int Y => _mouseY;

		public MdiChildTab Tab => _tab;

		internal MdiTabMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta, MdiChildTab tab)
		{
			_tab = tab;
			_button = button;
			_clicks = clicks;
			_mouseX = x;
			_mouseY = y;
			_delta = delta;
		}
	}
}
