using System;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Ribbon.Design
{
	[CLSCompliant(false)]
	public class RibbonLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(RibbonLayout);

		public RibbonLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
