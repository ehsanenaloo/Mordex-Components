using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.FilterBuilder
{
	[ToolboxItem(false)]
	public class TransparentTableLayoutPanel : TableLayoutPanel
	{
		public TransparentTableLayoutPanel()
		{
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			BackColor = Color.Transparent;
		}
	}
}
