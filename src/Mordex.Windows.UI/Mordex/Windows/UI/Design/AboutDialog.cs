using Mordex.Windows.Common;
using Mordex.Windows.UI.Controls;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.StatusBar;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	internal class AboutDialog : MxAboutDialog
	{
		internal AboutDialog()
			: base(typeof(AboutDialog).Assembly)
		{
		}

		internal void ShowForCommandManager(MxCommandManager uiCommandManager)
		{
			ShowDialog();
		}

		internal void ShowForDockManager(MxDockManager uiPanelManager)
		{
			ShowDialog();
		}

		internal void ShowForStatusBar(MxStatusBar uiStatusBar)
		{
			ShowDialog();
		}

		internal void ShowForComboBase(MxComboBase uiComboBase)
		{
			ShowDialog();
		}

		internal void ShowForColorPicker(MxColorPicker uiColorPicker)
		{
			ShowDialog();
		}

		internal void ShowForTabControl(MxTabControl uiTab)
		{
			ShowDialog();
		}
	}
}
