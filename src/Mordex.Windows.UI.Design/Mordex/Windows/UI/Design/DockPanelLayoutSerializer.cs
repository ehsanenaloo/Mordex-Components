using System;
using Mordex.Windows.Common.Design;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class DockPanelLayoutSerializer : LayoutSerializer
	{
		protected override Type LayoutType => typeof(DockPanelLayout);

		public DockPanelLayoutSerializer()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
