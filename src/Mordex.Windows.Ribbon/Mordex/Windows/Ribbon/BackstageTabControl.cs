using System.ComponentModel;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	[ToolboxItem(false)]
	[Designer("Mordex.Windows.Ribbon.Design.BackstageTabControlDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class BackstageTabControl : ContainerControl
	{
		internal BackstageTab _owner;

		public BackstageTabControl()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
		}

		internal BackstageTabControl(BackstageTab tab)
		{
			_owner = tab;
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			SetStyle(ControlStyles.ContainerControl, value: true);
		}
	}
}
