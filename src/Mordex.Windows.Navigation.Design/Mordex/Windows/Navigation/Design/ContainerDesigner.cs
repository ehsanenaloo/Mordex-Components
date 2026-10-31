using System;
using System.ComponentModel;
using Microsoft.DotNet.DesignTools.Designers;

namespace Mordex.Windows.Navigation.Design
{
	[CLSCompliant(false)]
	public class ContainerDesigner : ParentControlDesigner
	{
		public override SelectionRules SelectionRules => SelectionRules.Visible | SelectionRules.BottomSizeable;

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
