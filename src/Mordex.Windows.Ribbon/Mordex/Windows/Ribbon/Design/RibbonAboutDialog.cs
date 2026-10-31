using Mordex.Windows.Common;

namespace Mordex.Windows.Ribbon.Design
{
	internal class RibbonAboutDialog : MxAboutDialog
	{
		internal RibbonAboutDialog()
			: base(typeof(RibbonAboutDialog).Assembly)
		{
		}

		internal void ShowRibbonAbout(MxRibbon ribbon)
		{
			ShowDialog();
		}

		internal void ShowStatusBarAbout(MxRibbonStatusBar ribbonStatusBar)
		{
			ShowDialog();
		}
	}
}
