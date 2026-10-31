using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.DotNet.DesignTools.Designers;
using Mordex.Windows.UI.CommandBars;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class RebarDesigner : ParentControlDesigner
	{
		public override SelectionRules SelectionRules => SelectionRules.Visible;

		public RebarDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
			DrawGrid = false;
		}
	}
}
