using System;
using System.ComponentModel;
using Microsoft.DotNet.DesignTools.Designers;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class CaptionContainerDesigner : ParentControlDesigner
	{
		public override SelectionRules SelectionRules => SelectionRules.Visible | SelectionRules.BottomSizeable;

		public CaptionContainerDesigner()
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
