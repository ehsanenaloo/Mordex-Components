namespace Mordex.Windows.Ribbon
{
	internal interface ITabOwner
	{
		bool IsEnabled { get; }

		MxRibbon Ribbon { get; }

		void TabAdded(RibbonTab tab);

		void TabRemoved(RibbonTab tab);

		void TabDisposed(RibbonTab tab);

		void RemoveTab(RibbonTab tab);
	}
}
