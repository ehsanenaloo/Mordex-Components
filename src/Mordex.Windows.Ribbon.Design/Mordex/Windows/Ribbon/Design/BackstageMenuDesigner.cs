using System;
using Microsoft.DotNet.DesignTools.Designers;
using System.ComponentModel.Design;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Ribbon.Design
{
	[CLSCompliant(false)]
	public class BackstageMenuDesigner : ComponentDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerb[] value = new DesignerVerb[2]
				{
					new DesignerVerb("New Button", OnNewButton),
					new DesignerVerb("New Tab", OnNewTab)
				};
				return new DesignerVerbCollection(value);
			}
		}

		private BackstageMenu BackstageMenu => base.Component as BackstageMenu;

		public BackstageMenuDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnNewButton(object sender, EventArgs eventArgs)
		{
			AddCommand(new ButtonCommand());
		}

		private void OnNewTab(object sender, EventArgs eventArgs)
		{
			AddCommand(new BackstageTab());
		}

		private void AddCommand(CommandBase command)
		{
			if (BackstageMenu == null)
			{
				return;
			}
			DesignerChange.Run(BackstageMenu, "Commands", "New " + command.GetType().Name, () =>
			{
				BackstageMenu.Commands.Add(command);
				DesignerDragHelper.NameFromSite(command, nameFromSite: true);
			});
		}
	}
}
