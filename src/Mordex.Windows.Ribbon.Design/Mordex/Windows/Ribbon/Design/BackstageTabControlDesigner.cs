using System;
using System.ComponentModel;
using Microsoft.DotNet.DesignTools.Designers;

namespace Mordex.Windows.Ribbon.Design
{
	[CLSCompliant(false)]
	public class BackstageTabControlDesigner : ScrollableControlDesigner
	{
		public override SelectionRules SelectionRules => SelectionRules.Locked | SelectionRules.Visible;

		public BackstageTabControlDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			DrawGrid = true;
		}

		protected override void OnCreateHandle()
		{
			base.OnCreateHandle();
			DrawGrid = true;
		}
	}
}
