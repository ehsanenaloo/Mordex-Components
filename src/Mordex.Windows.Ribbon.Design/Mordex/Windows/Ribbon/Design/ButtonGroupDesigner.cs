using System;
using System.ComponentModel.Design;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Ribbon.Design
{
	[CLSCompliant(false)]
	public class ButtonGroupDesigner : CommandDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerbCollection verbs = base.Verbs;
				verbs.Add(new DesignerVerb("New Button", OnNewButton));
				verbs.Add(new DesignerVerb("New CheckBox", OnNewCheckBox));
				verbs.Add(new DesignerVerb("New ComboBox", OnNewComboBox));
				verbs.Add(new DesignerVerb("New Container Control", OnNewContainerControl));
				verbs.Add(new DesignerVerb("New DropDown", OnNewDropDown));
				verbs.Add(new DesignerVerb("New Gallery", OnNewGallery));
				return verbs;
			}
		}

		public ButtonGroupDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		private void OnNewButton(object sender, EventArgs eventArgs)
		{
			AddCommand(new ButtonCommand(), nameFromSite: true);
		}

		private void OnNewCheckBox(object sender, EventArgs eventArgs)
		{
			CheckBoxCommand command = new CheckBoxCommand();
			AddCommand(command);
		}

		private void OnNewComboBox(object sender, EventArgs eventArgs)
		{
			ComboBoxCommand command = new ComboBoxCommand();
			AddCommand(command);
		}

		private void OnNewContainerControl(object sender, EventArgs eventArgs)
		{
			ContainerControlCommand command = new ContainerControlCommand();
			AddCommand(command);
		}

		private void OnNewDropDown(object sender, EventArgs eventArgs)
		{
			AddCommand(new DropDownCommand(), nameFromSite: true);
		}

		private void OnNewGallery(object sender, EventArgs eventArgs)
		{
			Gallery command = new Gallery();
			AddCommand(command);
		}

		private void AddCommand(CommandBase command, bool nameFromSite = false)
		{
			if (!(base.Component is ButtonGroup buttonGroup))
			{
				return;
			}
			DesignerChange.Run(buttonGroup, "Commands", "New " + command.GetType().Name, () =>
			{
				buttonGroup.Commands.Add(command);
				DesignerDragHelper.NameFromSite(command, nameFromSite);
			});
		}
	}
}
