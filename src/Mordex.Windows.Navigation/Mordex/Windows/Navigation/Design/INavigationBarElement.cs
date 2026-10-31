using System;

namespace Mordex.Windows.Navigation.Design
{
	internal interface INavigationBarElement
	{
		MxNavigationBar NavigationBar { get; set; }

		event EventHandler SelectTab;

		bool CommitChanges();
	}
}
