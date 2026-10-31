using System;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	internal interface IManagerEditorControl
	{
		MxCommandManager CommandManager { get; set; }

		MxDockManager PanelManager { get; set; }

		event EventHandler DisableCancel;

		event EventHandler DataChanged;

		event EventHandler SelectTab;

		bool CommitChanges();
	}
}
