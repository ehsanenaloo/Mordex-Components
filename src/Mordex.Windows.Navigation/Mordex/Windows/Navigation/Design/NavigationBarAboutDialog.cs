using Mordex.Windows.Common;

namespace Mordex.Windows.Navigation.Design
{
	internal class NavigationBarAboutDialog : MxAboutDialog
	{
		internal NavigationBarAboutDialog()
			: base(typeof(NavigationBarAboutDialog).Assembly)
		{
		}

		internal void ShowAbout(MxNavigationBar navigationBar)
		{
			ShowDialog();
		}
	}
}
