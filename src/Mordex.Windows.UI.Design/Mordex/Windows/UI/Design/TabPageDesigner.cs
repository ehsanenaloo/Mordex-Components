using System;
using System.ComponentModel;
using Microsoft.DotNet.DesignTools.Designers;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class TabPageDesigner : ScrollableControlDesigner
	{
		public override SelectionRules SelectionRules => SelectionRules.Locked | SelectionRules.Visible;

		public TabPageDesigner()
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
