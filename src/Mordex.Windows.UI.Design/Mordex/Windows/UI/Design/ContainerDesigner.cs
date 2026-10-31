using System;
using System.ComponentModel;
using Microsoft.DotNet.DesignTools.Designers;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class ContainerDesigner : ScrollableControlDesigner
	{
		public ContainerDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			DrawGrid = true;
		}
	}
}
