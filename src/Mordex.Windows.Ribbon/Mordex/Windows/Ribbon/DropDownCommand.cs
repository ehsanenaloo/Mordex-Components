using System.ComponentModel;
using System.Windows.Forms;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Ribbon
{
	public class DropDownCommand : ButtonCommand, ICommandContainer
	{
		internal CommandCollection _dropDownItems;

		private DropDownButtonStyle _dropDownStyle;

		private int _maxDropDownItems;

		private bool _allowDropDownResize;

		private CommandSizeStyle _defaultSizeStyle;

		private AccessibleObject _dropDownAccessibleObject;

		private static readonly object _dropDownClosingEventKey = new object();

		private static readonly object _dropDownClosedEventKey = new object();

		private static readonly object _dropDownOpeningEventKey = new object();

		private static readonly object _dropDownOpenedEventKey = new object();

		internal override AccessibleRole Role
		{
			get
			{
				if (base.AccessibleRole != AccessibleRole.Default)
				{
					return base.AccessibleRole;
				}
				if (GetParentContainer() is DropDownCommand)
				{
					return AccessibleRole.MenuItem;
				}
				if (DropDownStyle == DropDownButtonStyle.SplitButton)
				{
					return AccessibleRole.SplitButton;
				}
				return AccessibleRole.ButtonDropDown;
			}
		}

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

		CommandType ICommandContainer.CommandType => CommandType;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle
		{
			get
			{
				if (_defaultSizeStyle == CommandSizeStyle.Default)
				{
					return CommandSizeStyle.Small;
				}
				return _defaultSizeStyle;
			}
		}

		bool ICommandContainer.IsEnabled => IsEnabled;

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Vertical;

		CommandCollection ICommandContainer.Commands => _dropDownItems;

		AccessibleObject ICommandContainer.AccessibilityObject => AccessibilityObject;

		internal override AccessibleObject AccessibilityObject
		{
			get
			{
				if (_dropDownAccessibleObject == null)
				{
					_dropDownAccessibleObject = new DropDownCommandState(this);
				}
				return _dropDownAccessibleObject;
			}
		}

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
		public CommandCollection Commands => _dropDownItems;

		[DefaultValue(DropDownButtonStyle.DropDown)]
		[Description("Represents the style and behavior of the DropDownButton.")]
		public DropDownButtonStyle DropDownStyle
		{
			get
			{
				return _dropDownStyle;
			}
			set
			{
				if (_dropDownStyle != value)
				{
					_dropDownStyle = value;
					NotifyTextChanged("DropDownStyle");
				}
			}
		}

		[DefaultValue(CommandSizeStyle.Default)]
		[Description("Specifies how the child commands are displayed in the menu window.")]
		public CommandSizeStyle CommandsSizeStyle
		{
			get
			{
				return _defaultSizeStyle;
			}
			set
			{
				if (_defaultSizeStyle != value)
				{
					_defaultSizeStyle = value;
					NotifyTextChanged("CommandsSizeStyle");
				}
			}
		}

		[Description("Indicates the maximum number of child commands to display in the dropdown list.")]
		[DefaultValue(25)]
		public int MaxDropDownItems
		{
			get
			{
				return _maxDropDownItems;
			}
			set
			{
				_maxDropDownItems = value;
			}
		}

		[Description("Specifies whether the user can change the height of the drop down window by dragging a bar at the bottom of menu.")]
		[DefaultValue(false)]
		public bool AllowDropDownResize
		{
			get
			{
				return _allowDropDownResize;
			}
			set
			{
				if (_allowDropDownResize != value)
				{
					_allowDropDownResize = value;
				}
			}
		}

		internal override CommandType CommandType => CommandType.DropDownButton;

		[Description("Occurs when the drop-down window of subitems is about to be closed.")]
		public event DropDownClosingEventHandler DropDownClosing
		{
			add
			{
				base.Events.AddHandler(_dropDownClosingEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dropDownClosingEventKey, value);
			}
		}

		[Description("Occurs when the drop-down window of subitems is closed.")]
		public event CommandEventHandler DropDownClosed
		{
			add
			{
				base.Events.AddHandler(_dropDownClosedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dropDownClosedEventKey, value);
			}
		}

		[Description("Occurs when the drop-down window of subitems is opening.")]
		public event CommandCancelEventHandler DropDownOpening
		{
			add
			{
				base.Events.AddHandler(_dropDownOpeningEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dropDownOpeningEventKey, value);
			}
		}

		[Description("Occurs when the drop-down window of subitems is opened.")]
		public event CommandEventHandler DropDownOpened
		{
			add
			{
				base.Events.AddHandler(_dropDownOpenedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(_dropDownOpenedEventKey, value);
			}
		}

		public DropDownCommand()
		{
		}

		public DropDownCommand(string key)
			: base(key)
		{
		}

		public DropDownCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public DropDownCommand(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			base.Text = text;
			base.ImageIndex = imageIndex;
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
			CommandDisposed(eventArgs.Command);
		}

		void ICommandContainer.DetachCommand(CommandBase command)
		{
			Commands.RemoveFromInnerListAndClearOwner(command);
		}

		void ICommandContainer.InsertCommand(int index, CommandBase command)
		{
			Commands.Insert(index, command);
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

		private void OnSiteChanged()
		{
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_dropDownItems = new CommandCollection(this, isQuickAccessCollection: false);
			_dropDownStyle = DropDownButtonStyle.DropDown;
			_defaultSizeStyle = CommandSizeStyle.Default;
			_maxDropDownItems = 25;
			_defaultMergeType = CommandMergeType.MergeItems;
			base.MergeType = _defaultMergeType;
		}

		internal virtual void CommandDisposed(CommandBase cmd)
		{
			if (Commands.Contains(cmd))
			{
				Commands.Remove(cmd);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _dropDownItems != null)
			{
				for (int i = 0; i < _dropDownItems.Count; i++)
				{
					CommandBase commandBase = _dropDownItems[i];
					commandBase.SetParentContainer(null);
					commandBase.CommandsOwner = null;
					commandBase.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public void DropDown()
		{
			if (base.Ribbon != null)
			{
				base.Ribbon.OpenDropDownCommand(this);
			}
			else if (GetOwningContextMenu() != null)
			{
				GetOwningContextMenu().ShowDropDownFor(this);
			}
		}

		public void CloseUp()
		{
			if (base.Ribbon != null)
			{
				base.Ribbon.CloseDropDownPopup(this);
			}
			else if (GetOwningContextMenu() != null)
			{
				GetOwningContextMenu().CloseOrForwardDropDown(this);
			}
		}

		internal override bool SearchShortcut(Shortcut shortcut, Control inputControl, Message msg)
		{
			if (!base.Visible || !Enabled)
			{
				return false;
			}
			if (base.Shortcut == shortcut)
			{
				if (inputControl == null || inputControl.PreProcessControlMessage(ref msg) == PreProcessControlState.MessageNotNeeded)
				{
					InvokeOnClick();
				}
				else if (inputControl != null)
				{
					NativeMethods.NativeMessage n = new NativeMethods.NativeMessage
					{
						_windowHandle = msg.HWnd,
						_lParam = msg.LParam,
						_wParam = msg.WParam,
						_message = msg.Msg
					};
					NativeMethods.TranslateMessage(ref n);
					NativeMethods.DispatchMessage(ref n);
				}
				return true;
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
			DropDownCommand dropDownCommand = new DropDownCommand();
			dropDownCommand._allowDropDownResize = _allowDropDownResize;
			dropDownCommand._dropDownStyle = _dropDownStyle;
			dropDownCommand._dropDownItems = _dropDownItems.CloneWithParent(dropDownCommand);
			dropDownCommand._defaultSizeStyle = _defaultSizeStyle;
			dropDownCommand.Description = base.Description;
			dropDownCommand.Enabled = Enabled;
			dropDownCommand.Icon = base.Icon;
			dropDownCommand.Image = base.Image;
			dropDownCommand.ImageIndex = base.ImageIndex;
			dropDownCommand.Key = base.Key;
			dropDownCommand.KeyTip = base.KeyTip;
			dropDownCommand.LargeIcon = base.LargeIcon;
			dropDownCommand.LargeImage = base.LargeImage;
			dropDownCommand.LargeImageIndex = base.LargeImageIndex;
			dropDownCommand._maxDropDownItems = _maxDropDownItems;
			dropDownCommand.Shortcut = base.Shortcut;
			dropDownCommand.SizeStyle = SizeStyle;
			dropDownCommand.Style = base.Style;
			dropDownCommand.CheckOnClick = base.CheckOnClick;
			dropDownCommand.ActAsOptionButton = base.ActAsOptionButton;
			dropDownCommand.ImageKey = base.ImageKey;
			dropDownCommand.LargeImageKey = base.LargeImageKey;
			dropDownCommand.Checked = base.Checked;
			dropDownCommand.AllowAddToQuickAccessBar = base.AllowAddToQuickAccessBar;
			if (_superTipSettings != null)
			{
				dropDownCommand._superTipSettings = _superTipSettings.Clone();
				dropDownCommand._superTipSettings.ImageListProvider = dropDownCommand;
			}
			dropDownCommand.Tag = base.Tag;
			dropDownCommand.Text = base.Text;
			dropDownCommand.Visible = base.Visible;
			return dropDownCommand;
		}

		internal bool CanCloseDropDown(CommandCloseReason closeReason)
		{
			DropDownClosingEventArgs e = new DropDownClosingEventArgs(this, closeReason);
			if (GetParentContainer() != null)
			{
				e.Cancel = GetParentContainer().CommandDropDownClosing(e);
			}
			OnDropDownClosing(e);
			return !e.Cancel;
		}

		internal void NotifyDropDownClosed()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandDropDownClosed(e);
			}
			OnDropDownClosed(e);
		}

		internal bool CanOpenDropDown()
		{
			CommandCancelEventArgs e = new CommandCancelEventArgs(this);
			if (GetParentContainer() != null)
			{
				e.Cancel = GetParentContainer().CommandDropDownOpening(e);
			}
			OnDropDownOpening(e);
			return !e.Cancel;
		}

		internal void NotifyDropDownOpened()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandDropDownOpened(e);
			}
			OnDropDownOpened(e);
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (_allowDropDownResize)
			{
				writer.AddValue("AllowDropDownResize", _allowDropDownResize);
			}
			if (_dropDownStyle != DropDownButtonStyle.DropDown)
			{
				writer.AddValue("DropDownStyle", _dropDownStyle);
			}
			if (_defaultSizeStyle != CommandSizeStyle.Default)
			{
				writer.AddValue("CommandsSizeStyle", _defaultSizeStyle);
			}
			if (_maxDropDownItems != 25)
			{
				writer.AddValue("MaxDropDownItems", _maxDropDownItems);
			}
			if (_dropDownItems != null && _dropDownItems.Count > 0)
			{
				writer.AddValue("Commands", _dropDownItems);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			_allowDropDownResize = (bool)LayoutReader.GetPropertyValue(reader, "AllowDropDownResize", typeof(bool), false);
			_dropDownStyle = (DropDownButtonStyle)LayoutReader.GetPropertyValue(reader, "DropDownStyle", typeof(DropDownButtonStyle), DropDownButtonStyle.DropDown);
			_defaultSizeStyle = (CommandSizeStyle)LayoutReader.GetPropertyValue(reader, "CommandsSizeStyle", typeof(CommandSizeStyle), CommandSizeStyle.Default);
			_maxDropDownItems = (int)LayoutReader.GetPropertyValue(reader, "MaxDropDownItems", typeof(int), 25);
			DeserializeDropDownCommandState(reader);
		}

		private void DeserializeDropDownCommandState(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["Commands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_dropDownItems);
				return;
			}
			_dropDownItems.DisposeAll();
		}

		protected virtual void OnDropDownClosing(DropDownClosingEventArgs e)
		{
			if (base.Events[_dropDownClosingEventKey] is DropDownClosingEventHandler dropDownClosingEventHandler)
			{
				dropDownClosingEventHandler(this, e);
			}
		}

		protected virtual void OnDropDownClosed(CommandEventArgs e)
		{
			if (base.Events[_dropDownClosedEventKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		protected virtual void OnDropDownOpening(CommandCancelEventArgs e)
		{
			if (base.Events[_dropDownOpeningEventKey] is CommandCancelEventHandler commandCancelEventHandler)
			{
				commandCancelEventHandler(this, e);
			}
		}

		protected virtual void OnDropDownOpened(CommandEventArgs e)
		{
			if (base.Events[_dropDownOpenedEventKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}
	}
}
