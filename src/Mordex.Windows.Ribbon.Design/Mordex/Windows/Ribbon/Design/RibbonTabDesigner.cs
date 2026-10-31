using System;
using Microsoft.DotNet.DesignTools.Designers;
using System.ComponentModel.Design;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Ribbon.Design
{
	internal class RibbonTabDesigner : ComponentDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[1]
				{
					new DesignerVerb("New Group", OnNewGroup)
				};
				return new DesignerVerbCollection(value);
			}
		}

		private RibbonTab Tab => base.Component as RibbonTab;

		public RibbonTabDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnNewGroup(object sender, EventArgs eventArgs)
		{
			if (Tab == null)
			{
				return;
			}
			DesignerChange.Run(Tab, "Groups", "New Group", () =>
			{
				IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
				RibbonGroup ribbonGroup = designerHost != null ? (RibbonGroup)designerHost.CreateComponent(typeof(RibbonGroup)) : new RibbonGroup();
				ribbonGroup.Text = "Group " + Tab.Groups.Count;
				Tab.Groups.Add(ribbonGroup);
			});
		}
	}
}
