using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[Designer("Mordex.Windows.Ribbon.Design.ButtonGroupDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class ButtonGroup : CommandBase, ICommandContainer, IKeyTipProvider
	{
		private CommandCollection _commands;

		MxRibbon ICommandContainer.Ribbon
		{
			get
			{
				if (GetParentContainer() != null)
				{
					return GetParentContainer().Ribbon;
				}
				return null;
			}
		}

		MxRibbonContextMenu ICommandContainer.RibbonContextMenu
		{
			get
			{
				if (GetParentContainer() != null)
				{
					return GetParentContainer().RibbonContextMenu;
				}
				return null;
			}
		}

		CommandType ICommandContainer.CommandType => CommandType.ButtonGroup;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Small;

		bool ICommandContainer.IsEnabled => IsEnabled;

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Horizontal;

		AccessibleObject ICommandContainer.AccessibilityObject => AccessibilityObject;

		bool IKeyTipProvider.HasKeyTip => false;

		string IKeyTipProvider.KeyTip => "";

		internal override CommandType CommandType => CommandType.ButtonGroup;

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
				OnSiteChanged();
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Browsable(false)]
		public CommandCollection Commands => _commands;

		public ButtonGroup()
		{
		}

		public ButtonGroup(string key)
			: base(key)
		{
		}

		void ICommandContainer.InsertCommand(int index, CommandBase command)
		{
			Commands.Insert(index, command);
		}

		void ICommandContainer.DetachCommand(CommandBase command)
		{
			Commands.RemoveFromInnerListAndClearOwner(command);
		}

		void ICommandContainer.CommandAdded(CommandBase command)
		{
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandAdded(command);
			}
		}

		void ICommandContainer.CommandRemoved(CommandBase command)
		{
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandRemoved(command);
			}
		}

		void ICommandContainer.CommandClicked(CommandEventArgs eventArgs)
		{
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandClicked(eventArgs);
			}
		}

		bool ICommandContainer.CommandDropDownClosing(DropDownClosingEventArgs eventArgs)
		{
			if (GetParentContainer() != null)
			{
				return GetParentContainer().CommandDropDownClosing(eventArgs);
			}
			return false;
		}

		void ICommandContainer.CommandDropDownClosed(CommandEventArgs eventArgs)
		{
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandDropDownClosed(eventArgs);
			}
		}

		void ICommandContainer.CommandDropDownOpened(CommandEventArgs eventArgs)
		{
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandDropDownOpened(eventArgs);
			}
		}

		bool ICommandContainer.CommandDropDownOpening(CommandCancelEventArgs eventArgs)
		{
			if (GetParentContainer() != null)
			{
				return GetParentContainer().CommandDropDownOpening(eventArgs);
			}
			return false;
		}

		void ICommandContainer.CommandDisposed(CommandEventArgs eventArgs)
		{
			if (Commands.Contains(eventArgs.Command))
			{
				Commands.Remove(eventArgs.Command);
			}
		}

		bool ICommandContainer.CanBeParentTo(CommandType commandType)
		{
			switch (commandType)
			{
			case CommandType.LargeButton:
			case CommandType.SmallButton:
			case CommandType.DropDownButton:
			case CommandType.ContainerControl:
			case CommandType.Gallery:
				return true;
			default:
				return false;
			}
		}

		void ICommandContainer.CommandCheckedChanged(CommandEventArgs eventArgs)
		{
			if (!(eventArgs.Command is ButtonCommand buttonCommand) || !buttonCommand.Checked || !buttonCommand.ActAsOptionButton)
			{
				return;
			}
			foreach (CommandBase command in Commands)
			{
				if (command is ButtonCommand && command != eventArgs.Command && ((ButtonCommand)command).ActAsOptionButton)
				{
					((ButtonCommand)command).Checked = false;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _commands != null)
			{
				for (int i = 0; i < _commands.Count; i++)
				{
					CommandBase commandBase = _commands[i];
					commandBase.SetParentContainer(null);
					commandBase.CommandsOwner = null;
					commandBase.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_commands = new CommandCollection(this, isQuickAccessCollection: false);
			_defaultMergeType = CommandMergeType.MergeItems;
			base.MergeType = _defaultMergeType;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (_commands.Count > 0)
			{
				writer.AddValue("Commands", _commands);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			DeserializeButtonGroupState(reader);
		}

		internal override bool SearchShortcut(Shortcut shortcut, Control inputControl, Message msg)
		{
			if (!base.Visible || !Enabled)
			{
				return false;
			}
			foreach (CommandBase command in Commands)
			{
				if (command.SearchShortcut(shortcut, inputControl, msg))
				{
					return true;
				}
			}
			return false;
		}

		private void OnSiteChanged()
		{
		}

		public override object Clone()
		{
			ButtonGroup buttonGroup = new ButtonGroup();
			buttonGroup._commands = _commands.CloneWithParent(buttonGroup);
			buttonGroup.Enabled = Enabled;
			buttonGroup.Key = base.Key;
			buttonGroup.Tag = base.Tag;
			buttonGroup.Visible = base.Visible;
			buttonGroup.AllowAddToQuickAccessBar = base.AllowAddToQuickAccessBar;
			return buttonGroup;
		}

		private void DeserializeButtonGroupState(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["Commands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_commands);
				return;
			}
			_commands.DisposeAll();
		}
	}
}
