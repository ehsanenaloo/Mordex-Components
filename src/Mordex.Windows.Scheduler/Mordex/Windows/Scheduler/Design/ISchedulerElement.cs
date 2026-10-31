using System;
using System.ComponentModel;
using System.Drawing;

namespace Mordex.Windows.Scheduler.Design
{
	internal interface ISchedulerElement
	{
		object SelectedObject { get; set; }

		Image ItemImage { get; set; }

		event EventHandler SelectedObjectChanged;

		event EventHandler RefreshTree;

		bool CommitChanges();

		bool Support(ToolbarOptions option);

		void SetSite(ISite site);

		void Unload();
	}
}
