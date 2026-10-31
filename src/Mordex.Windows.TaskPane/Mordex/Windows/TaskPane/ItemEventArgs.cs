using System;

namespace Mordex.Windows.TaskPane
{
	public class ItemEventArgs : EventArgs
	{
		private TaskPaneItem _item;

		public TaskPaneItem Item => _item;

		public ItemEventArgs(TaskPaneItem item)
		{
			_item = item;
		}
	}
}
