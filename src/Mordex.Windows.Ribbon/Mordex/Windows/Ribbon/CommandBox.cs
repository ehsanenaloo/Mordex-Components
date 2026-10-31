using System;
using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	[Designer("Mordex.Windows.Ribbon.Design.RibbonGroupDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class CommandBox : CommandBase, ICommandContainer, IKeyTipProvider
	{
		private CommandCollection _commandBoxCommands;

		private bool _centerItems;

		private int _itemsSpace;

		private BoxStyle _boxStyle;

		private bool _allowAutoSizeItems;

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

		CommandType ICommandContainer.CommandType => CommandType.Box;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Large;

		bool ICommandContainer.IsEnabled => IsEnabled;

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle;

		AccessibleObject ICommandContainer.AccessibilityObject => AccessibilityObject;

		bool IKeyTipProvider.HasKeyTip => false;

		string IKeyTipProvider.KeyTip => "";

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

		[Description("Specifies the orientation of the commands in a CommandBox.")]
		[DefaultValue(BoxStyle.Vertical)]
		public BoxStyle BoxStyle
		{
			get
			{
				return _boxStyle;
			}
			set
			{
				if (_boxStyle != value)
				{
					_boxStyle = value;
					NotifyTextChanged("BoxStyle");
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CommandCollection Commands => _commandBoxCommands;

		[Description("Specifies whether the items will be centered in the bounds of the CommandBox.")]
		[DefaultValue(false)]
		public bool CenterItems
		{
			get
			{
				return _centerItems;
			}
			set
			{
				if (_centerItems != value)
				{
					_centerItems = value;
					NotifyTextChanged("CenterItems");
				}
			}
		}

		[Description("Indicates the space between the items in the box")]
		[DefaultValue(0)]
		public int ItemsSpace
		{
			get
			{
				return _itemsSpace;
			}
			set
			{
				if (_itemsSpace != value)
				{
					if (value < 0)
					{
						throw new ArgumentException("ItemSpace must be equal or bigger than zero");
					}
					_itemsSpace = value;
					NotifyTextChanged("ItemsSpace");
				}
			}
		}

		[Description("Specifies whether the items in the box can change its SizeStyle and display properties to fit in the area of the Ribbon.")]
		[DefaultValue(true)]
		public bool AllowAutoSizeItems
		{
			get
			{
				return _allowAutoSizeItems;
			}
			set
			{
				if (_allowAutoSizeItems != value)
				{
					_allowAutoSizeItems = value;
					NotifyTextChanged("AllowAutoSizeItems");
				}
			}
		}

		internal override CommandType CommandType => CommandType.Box;

		public CommandBox()
		{
		}

		public CommandBox(string key, BoxStyle boxStyle)
			: base(key)
		{
			_boxStyle = boxStyle;
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
			case CommandType.ButtonGroup:
			case CommandType.ContainerControl:
			case CommandType.Box:
			case CommandType.Gallery:
			case CommandType.Separator:
			case CommandType.Label:
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
			if (disposing && _commandBoxCommands != null)
			{
				for (int i = 0; i < _commandBoxCommands.Count; i++)
				{
					CommandBase commandBase = _commandBoxCommands[i];
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
			_commandBoxCommands = new CommandCollection(this, isQuickAccessCollection: false);
			_boxStyle = BoxStyle.Vertical;
			_itemsSpace = 0;
			_allowAutoSizeItems = true;
			_defaultMergeType = CommandMergeType.MergeItems;
			base.MergeType = _defaultMergeType;
		}

		private void OnSiteChanged()
		{
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

		public override object Clone()
		{
			CommandBox commandBox = new CommandBox();
			commandBox._allowAutoSizeItems = _allowAutoSizeItems;
			commandBox._boxStyle = _boxStyle;
			commandBox._centerItems = _centerItems;
			commandBox._commandBoxCommands = _commandBoxCommands.CloneWithParent(commandBox);
			commandBox.Enabled = Enabled;
			commandBox._itemsSpace = _itemsSpace;
			commandBox.Key = base.Key;
			commandBox.Tag = base.Tag;
			commandBox.Visible = base.Visible;
			commandBox.AllowAddToQuickAccessBar = base.AllowAddToQuickAccessBar;
			return commandBox;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (!_allowAutoSizeItems)
			{
				writer.AddValue("AllowAutoSizeItems", _allowAutoSizeItems);
			}
			if (_boxStyle != BoxStyle.Vertical)
			{
				writer.AddValue("BoxStyle", _boxStyle);
			}
			if (_centerItems)
			{
				writer.AddValue("CenterItems", _centerItems);
			}
			if (_commandBoxCommands != null && _commandBoxCommands.Count > 0)
			{
				writer.AddValue("Commands", _commandBoxCommands);
			}
			if (_itemsSpace != 0)
			{
				writer.AddValue("ItemsSpace", _itemsSpace);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			_allowAutoSizeItems = (bool)LayoutReader.GetPropertyValue(reader, "AllowAutoSizeItems", typeof(bool), true);
			_boxStyle = (BoxStyle)LayoutReader.GetPropertyValue(reader, "BoxStyle", typeof(BoxStyle), BoxStyle.Vertical);
			_centerItems = (bool)LayoutReader.GetPropertyValue(reader, "CenterItems", typeof(bool), false);
			_itemsSpace = (int)LayoutReader.GetPropertyValue(reader, "ItemsSpace", typeof(int), 0);
			DeserializeCommandBoxState(reader);
		}

		private void DeserializeCommandBoxState(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["Commands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_commandBoxCommands);
				return;
			}
			_commandBoxCommands.DisposeAll();
		}
	}
}
