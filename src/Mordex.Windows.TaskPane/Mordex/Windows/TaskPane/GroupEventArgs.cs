using System;

namespace Mordex.Windows.TaskPane
{
	public class GroupEventArgs : EventArgs
	{
		private TaskPaneGroup _group;

		public TaskPaneGroup Group => _group;

		public GroupEventArgs(TaskPaneGroup group)
		{
			_group = group;
		}
	}
}
