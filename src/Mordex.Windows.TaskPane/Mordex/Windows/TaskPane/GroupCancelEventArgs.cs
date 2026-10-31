using System;

namespace Mordex.Windows.TaskPane
{
	public class GroupCancelEventArgs : EventArgs
	{
		private TaskPaneGroup _group;

		private bool _cancel;

		public TaskPaneGroup Group => _group;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		internal GroupCancelEventArgs(TaskPaneGroup group)
		{
			_group = group;
		}
	}
}
