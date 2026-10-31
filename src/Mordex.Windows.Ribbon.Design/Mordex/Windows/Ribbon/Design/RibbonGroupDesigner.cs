using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using Mordex.Windows.Common.Design;

namespace Mordex.Windows.Ribbon.Design
{
	internal class RibbonGroupDesigner : CommandDesigner
	{
		public override DesignerVerbCollection Verbs
		{
			get
			{
				DesignerVerbCollection verbs = base.Verbs;
				verbs.Add(new DesignerVerb("New Button", OnNewButton));
				verbs.Add(new DesignerVerb("New Button Group", OnNewButtonGroup));
				verbs.Add(new DesignerVerb("New CheckBox", OnNewCheckBox));
				verbs.Add(new DesignerVerb("New ComboBox", OnNewComboBox));
				verbs.Add(new DesignerVerb("New TextBox", OnNewEditBox));
				verbs.Add(new DesignerVerb("New Container Control", OnNewContainerControl));
				verbs.Add(new DesignerVerb("New DropDown", OnNewDropDown));
				verbs.Add(new DesignerVerb("New Gallery", OnNewGallery));
				verbs.Add(new DesignerVerb("New Separator", OnNewSeparator));
				verbs.Add(new DesignerVerb("New Box", OnNewBox));
				return verbs;
			}
		}

		public RibbonGroupDesigner()
		{
			AssemblyLoaderHelper.AttachCurrentDomainEvents();
		}

		public override void Initialize(IComponent component)
		{
			base.Initialize(component);
		}

		private void OnNewSeparator(object sender, EventArgs eventArgs)
		{
			SeparatorCommand command = new SeparatorCommand();
			AddCommand(command);
		}

		private void OnNewEditBox(object sender, EventArgs eventArgs)
		{
			TextBoxCommand command = new TextBoxCommand();
			AddCommand(command);
		}

		private void OnNewButton(object sender, EventArgs eventArgs)
		{
			AddCommand(new ButtonCommand(), nameFromSite: true);
		}

		private void OnNewButtonGroup(object sender, EventArgs eventArgs)
		{
			ButtonGroup command = new ButtonGroup();
			AddCommand(command);
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

		private void OnNewBox(object sender, EventArgs eventArgs)
		{
			CommandBox command = new CommandBox();
			AddCommand(command);
		}

		private void AddCommand(CommandBase command, bool nameFromSite = false)
		{
			CommandCollection commands = (base.Component as RibbonGroup)?.Commands ?? (base.Component as CommandBox)?.Commands;
			if (commands == null)
			{
				return;
			}
			DesignerChange.Run(base.Component, "Commands", "New " + command.GetType().Name, () =>
			{
				commands.Add(command);
				DesignerDragHelper.NameFromSite(command, nameFromSite);
			});
		}
	}
}
