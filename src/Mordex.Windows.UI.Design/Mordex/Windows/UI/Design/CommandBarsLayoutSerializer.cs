using System;
using Mordex.Windows.Common.Design;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class CommandBarsLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(CommandBarsLayout);

		public CommandBarsLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
