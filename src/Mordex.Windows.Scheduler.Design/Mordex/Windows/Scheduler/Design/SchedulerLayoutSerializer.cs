using System;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Scheduler.Design
{
	[CLSCompliant(false)]
	public class SchedulerLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(SchedulerLayout);

		public SchedulerLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
