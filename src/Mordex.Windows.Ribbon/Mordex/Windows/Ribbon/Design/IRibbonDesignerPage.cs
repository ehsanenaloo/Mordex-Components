using System;

namespace Mordex.Windows.Ribbon.Design
{
	internal interface IRibbonDesignerPage
	{
		MxRibbonStatusBar StatusBar { get; set; }

		MxRibbon Ribbon { get; set; }

		event EventHandler DisableCancel;

		event EventHandler DataChanged;

		event EventHandler SelectTab;

		bool CommitChanges();

		void Unload();
	}
}
