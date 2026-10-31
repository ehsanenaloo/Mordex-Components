using System;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Navigation.Design
{
	[CLSCompliant(false)]
	public class NavigationBarLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(NavigationBarLayout);

		public NavigationBarLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
