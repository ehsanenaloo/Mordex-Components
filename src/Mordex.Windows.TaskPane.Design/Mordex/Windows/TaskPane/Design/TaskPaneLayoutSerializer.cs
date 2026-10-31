using System;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.TaskPane.Design
{
	[CLSCompliant(false)]
	public class TaskPaneLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(TaskPaneLayout);

		public TaskPaneLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
