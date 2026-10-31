using System;

namespace Mordex.Windows.TaskPane.Design
{
	internal interface ITaskPaneElement
	{
		MxTaskPane TaskPane { get; set; }

		event EventHandler SelectTab;

		bool CommitChanges();
	}
}
