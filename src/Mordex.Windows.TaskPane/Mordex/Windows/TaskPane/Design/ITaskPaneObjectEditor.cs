using System;
using System.Drawing;

namespace Mordex.Windows.TaskPane.Design
{
	internal interface ITaskPaneObjectEditor
	{
		object SelectedObject { get; set; }

		Image ItemImage { get; set; }

		event EventHandler SelectedObjectChanged;

		bool CommitChanges();

		bool Support(ToolbarOptions option);
	}
}
