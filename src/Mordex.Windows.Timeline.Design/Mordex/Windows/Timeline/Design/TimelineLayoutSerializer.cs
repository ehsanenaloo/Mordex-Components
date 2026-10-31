using System;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Timeline.Design
{
	[CLSCompliant(false)]
	public class TimelineLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(TimelineLayout);

		public TimelineLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
