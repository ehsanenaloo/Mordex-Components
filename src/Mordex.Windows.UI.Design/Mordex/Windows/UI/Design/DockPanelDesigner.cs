using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using Mordex.Windows.UI.Docking;

namespace Mordex.Windows.UI.Design
{
	[CLSCompliant(false)]
	public class DockPanelDesigner : DockPanelBaseDesigner
	{
		public DockPanelDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}
	}
}
