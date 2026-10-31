using System;
using Mordex.Windows.Common.Design;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class TabLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(TabLayout);

		public TabLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
