using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	[Designer("Mordex.Windows.Ribbon.Design.RibbonGroupDesigner, Mordex.Windows.Ribbon.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
	public class RibbonGroup : CommandBase, ICommandContainer, IKeyTipProvider, IInvalidatable
	{
		private Image _ribbonGroupImage;

		private int _ribbonGroupImageIndex;

		private Icon _ribbonGroupIcon;

		private string _ribbonGroupText;

		private string _ribbonGroupKeyTip;

		private int _ribbonGroupItemsSpace;

		private CommandCollection _ribbonGroupCommands;

		private SuperTipSettings _superTipSettings;

		private bool _ribbonGroupAllowAutoSizeItems;

		private bool _dialogButtonVisible;

		private bool _ribbonGroupCenterItems;

		private string _ribbonGroupImageKey;

		private string _dialogButtonKeyTip;

		private RibbonForegroundStyle _ribbonGroupForegroundStyle;

		private SuperTipSettings _dialogButtonSuperTipSettings;

		private static readonly object s_dialogButtonClickEventKey = new object();

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

		CommandType ICommandContainer.CommandType => CommandType.RibbonGroup;

		CommandSizeStyle ICommandContainer.DefaultSizeStyle => CommandSizeStyle.Large;

		bool ICommandContainer.IsEnabled => IsEnabled;

		BoxStyle ICommandContainer.LayoutDirection => BoxStyle.Vertical;

		AccessibleObject ICommandContainer.AccessibilityObject => AccessibilityObject;

		bool IKeyTipProvider.HasKeyTip
		{
			get
			{
				if (_ribbonGroupKeyTip.Length > 0)
				{
					return true;
				}
				return false;
			}
		}

		string IKeyTipProvider.KeyTip => _ribbonGroupKeyTip;

		[Description("Represents the letter to press after the Alt key to activate the command.")]
		[DefaultValue("")]
		[Localizable(true)]
		public string KeyTip
		{
			get
			{
				return _ribbonGroupKeyTip;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_ribbonGroupKeyTip != value)
				{
					_ribbonGroupKeyTip = value;
				}
			}
		}

		[DefaultValue("")]
		[Description("Represents the character used as the key tip for the dialog button.")]
		public string DialogButtonKeyTip
		{
			get
			{
				return _dialogButtonKeyTip;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_dialogButtonKeyTip != value)
				{
					_dialogButtonKeyTip = value;
				}
			}
		}

		[Description("Represents the settings used to customize the supertip of the dialog button.")]
		[TypeConverter("Mordex.Windows.Common.Design.SuperTipSettingsConverter, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor("Mordex.Windows.Common.Design.SuperTipSettingsEditor, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", typeof(UITypeEditor))]
		public SuperTipSettings DialogButtonSuperTipSettings => _dialogButtonSuperTipSettings;

		[Localizable(true)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Reppresents the image displayed on the group when it is shrunk.")]
		public Image Image
		{
			get
			{
				return _ribbonGroupImage;
			}
			set
			{
				if (_ribbonGroupImage != value)
				{
					_ribbonGroupImage = value;
					NotifyTextChanged("Image");
				}
			}
		}

		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed on the group when it is shrunk.")]
		[Localizable(true)]
		public int ImageIndex
		{
			get
			{
				return _ribbonGroupImageIndex;
			}
			set
			{
				if (_ribbonGroupImageIndex != value)
				{
					_ribbonGroupImageIndex = value;
					NotifyTextChanged("ImageIndex");
				}
			}
		}

		[Description("Represents the key accessor for the image in the ImageList displayed on the group when it is shrunk.")]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageKeyConverter))]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[DefaultValue(null)]
		public string ImageKey
		{
			get
			{
				return _ribbonGroupImageKey;
			}
			set
			{
				if (_ribbonGroupImageKey != value)
				{
					_ribbonGroupImageKey = value;
					NotifyTextChanged("ImageKey");
				}
			}
		}

		[Description("Represents the Icon displayed on the displayed on the group when it is shrunk..")]
		[DefaultValue(null)]
		[Localizable(true)]
		public Icon Icon
		{
			get
			{
				return _ribbonGroupIcon;
			}
			set
			{
				if (_ribbonGroupIcon != value)
				{
					_ribbonGroupIcon = value;
					NotifyTextChanged("Icon");
				}
			}
		}

		[DefaultValue("")]
		[Description("Represents the text displayed on the group.")]
		[Localizable(true)]
		public string Text
		{
			get
			{
				return _ribbonGroupText;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_ribbonGroupText != value)
				{
					_ribbonGroupText = value;
					NotifyTextChanged("Text");
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public CommandCollection Commands => _ribbonGroupCommands;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Description("Represents the settings used to customize the supertip of the command.")]
		[Editor("Mordex.Windows.Common.Design.SuperTipSettingsEditor, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.Common.Design.SuperTipSettingsConverter, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
		public SuperTipSettings SuperTipSettings => _superTipSettings;

		[Browsable(false)]
		[Localizable(true)]
		public string SupertipSettingsData
		{
			get
			{
				PropertyValue root = new PropertyValue("SupertipSettings", _superTipSettings);
				LayoutWriter mordexLayoutWriter = new LayoutWriter(root, isRoot: true);
				return mordexLayoutWriter.ToString();
			}
			set
			{
				try
				{
					LayoutReader mordexLayoutReader = LayoutReader.FromString(value);
					mordexLayoutReader["SupertipSettings"]?.DeserializeObject(_superTipSettings);
				}
				catch (Exception ex) when (ex is System.Xml.XmlException || ex is FormatException || ex is InvalidCastException || ex is ArgumentException || ex is InvalidOperationException)
				{
					// Invalid saved data keeps the current supertip settings.
				}
			}
		}

		[Description("Specifies whether the items in the group can change its SizeStyle and display properties to fit in the area of the Ribbon.")]
		[DefaultValue(true)]
		public bool AllowAutoSizeItems
		{
			get
			{
				return _ribbonGroupAllowAutoSizeItems;
			}
			set
			{
				if (_ribbonGroupAllowAutoSizeItems != value)
				{
					_ribbonGroupAllowAutoSizeItems = value;
					NotifyTextChanged("AllowAutoSizeItems");
				}
			}
		}

		[DefaultValue(0)]
		[Description("Indicates the space between the items in the group")]
		public int ItemsSpace
		{
			get
			{
				return _ribbonGroupItemsSpace;
			}
			set
			{
				if (_ribbonGroupItemsSpace != value)
				{
					if (value < 0)
					{
						throw new ArgumentException("ItemSpace must be equal or bigger than zero");
					}
					_ribbonGroupItemsSpace = value;
					NotifyTextChanged("ItemsSpace");
				}
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the option button on the group's footer is visible.")]
		public bool DialogButtonVisible
		{
			get
			{
				return _dialogButtonVisible;
			}
			set
			{
				if (_dialogButtonVisible != value)
				{
					_dialogButtonVisible = value;
					NotifyTextChanged("DialogButtonVisible");
				}
			}
		}

		[DefaultValue(false)]
		[Description("Specifies whether the items will be vertically centered in the group.")]
		public bool CenterItems
		{
			get
			{
				return _ribbonGroupCenterItems;
			}
			set
			{
				if (_ribbonGroupCenterItems != value)
				{
					_ribbonGroupCenterItems = value;
					NotifyTextChanged("CenterItems");
				}
			}
		}

		[Description("Represents the object that holds the the color and FontStyle used to draw the group\u00b4s text.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RibbonForegroundStyle ForegroundStyle => _ribbonGroupForegroundStyle;

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

		internal override CommandType CommandType => CommandType.RibbonGroup;

		[Description("Occurs when the option button is clicked.")]
		public event EventHandler DialogButtonClick
		{
			add
			{
				base.Events.AddHandler(s_dialogButtonClickEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_dialogButtonClickEventKey, value);
			}
		}

		public RibbonGroup()
		{
		}

		public RibbonGroup(string key)
			: base(key)
		{
		}

		public RibbonGroup(string key, string text)
			: base(key)
		{
			base.Key = key;
			Text = text;
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

		public bool ShouldSerializeSupertipSettingsData()
		{
			if (_superTipSettings.FooterImage != null)
			{
				return true;
			}
			if (_superTipSettings.FooterImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.FooterImageKey != null && _superTipSettings.FooterImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.FooterText != null && _superTipSettings.FooterText.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.HeaderImage != null)
			{
				return true;
			}
			if (_superTipSettings.HeaderImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.HeaderImageKey != null && _superTipSettings.HeaderImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.HeaderText != null && _superTipSettings.HeaderText.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.Image != null)
			{
				return true;
			}
			if (_superTipSettings.ImageIndex != -1)
			{
				return true;
			}
			if (_superTipSettings.ImageKey != null && _superTipSettings.ImageKey.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.Text != null && _superTipSettings.Text.Length > 0)
			{
				return true;
			}
			if (_superTipSettings.ToolTipStyle != ToolTipStyle.Office2007)
			{
				return true;
			}
			if (_superTipSettings.WordWrap)
			{
				return true;
			}
			return false;
		}

		public void ResetSupertipSettingsData()
		{
			_superTipSettings.FooterImage = null;
			_superTipSettings.FooterImageIndex = -1;
			_superTipSettings.FooterImageKey = "";
			_superTipSettings.FooterText = "";
			_superTipSettings.HeaderImage = null;
			_superTipSettings.HeaderImageIndex = -1;
			_superTipSettings.HeaderImageKey = "";
			_superTipSettings.HeaderText = "";
			_superTipSettings.Image = null;
			_superTipSettings.ImageIndex = -1;
			_superTipSettings.ImageKey = "";
			_superTipSettings.Text = "";
			_superTipSettings.ToolTipStyle = ToolTipStyle.Office2007;
			_superTipSettings.WordWrap = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _ribbonGroupCommands != null)
			{
				for (int i = 0; i < _ribbonGroupCommands.Count; i++)
				{
					CommandBase commandBase = _ribbonGroupCommands[i];
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
			_ribbonGroupCommands = new CommandCollection(this, isQuickAccessCollection: false);
			_ribbonGroupText = "";
			_ribbonGroupKeyTip = "";
			_ribbonGroupItemsSpace = 0;
			_ribbonGroupImageIndex = -1;
			_ribbonGroupAllowAutoSizeItems = true;
			_superTipSettings = new SuperTipSettings();
			_superTipSettings.ImageListProvider = this;
			_dialogButtonSuperTipSettings = new SuperTipSettings();
			_dialogButtonSuperTipSettings.ImageListProvider = this;
			_dialogButtonKeyTip = "";
			_defaultMergeType = CommandMergeType.MergeItems;
			base.MergeType = _defaultMergeType;
			_ribbonGroupForegroundStyle = new RibbonForegroundStyle(this);
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
			RibbonGroup ribbonGroup = new RibbonGroup();
			ribbonGroup._ribbonGroupAllowAutoSizeItems = _ribbonGroupAllowAutoSizeItems;
			ribbonGroup._ribbonGroupCenterItems = _ribbonGroupCenterItems;
			ribbonGroup._ribbonGroupCommands = _ribbonGroupCommands.CloneWithParent(ribbonGroup);
			ribbonGroup.Enabled = Enabled;
			ribbonGroup._ribbonGroupIcon = _ribbonGroupIcon;
			ribbonGroup._ribbonGroupImage = _ribbonGroupImage;
			ribbonGroup._ribbonGroupImageIndex = _ribbonGroupImageIndex;
			ribbonGroup.ItemsSpace = ItemsSpace;
			ribbonGroup.Key = base.Key;
			ribbonGroup._ribbonGroupKeyTip = _ribbonGroupKeyTip;
			ribbonGroup._dialogButtonVisible = _dialogButtonVisible;
			ribbonGroup.AllowAddToQuickAccessBar = base.AllowAddToQuickAccessBar;
			if (_superTipSettings != null)
			{
				ribbonGroup._superTipSettings = _superTipSettings.Clone();
				ribbonGroup._superTipSettings.ImageListProvider = ribbonGroup;
			}
			if (_dialogButtonSuperTipSettings != null)
			{
				ribbonGroup._dialogButtonSuperTipSettings = _dialogButtonSuperTipSettings.Clone();
				ribbonGroup._dialogButtonSuperTipSettings.ImageListProvider = ribbonGroup;
			}
			ribbonGroup.Tag = base.Tag;
			ribbonGroup._ribbonGroupText = _ribbonGroupText;
			ribbonGroup.Visible = base.Visible;
			ribbonGroup._ribbonGroupForegroundStyle = _ribbonGroupForegroundStyle.CloneForOwner(ribbonGroup);
			return ribbonGroup;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (!_ribbonGroupAllowAutoSizeItems)
			{
				writer.AddValue("AllowAutoSizeItems", _ribbonGroupAllowAutoSizeItems);
			}
			if (_ribbonGroupCenterItems)
			{
				writer.AddValue("CenterItems", _ribbonGroupCenterItems);
			}
			if (_ribbonGroupCommands.Count > 0)
			{
				writer.AddValue("Commands", _ribbonGroupCommands);
			}
			if (_ribbonGroupForegroundStyle.HasCustomStyle())
			{
				writer.AddValue("ForegroundStyle", _ribbonGroupForegroundStyle);
			}
			if (_ribbonGroupIcon != null)
			{
				writer.AddValue("Icon", _ribbonGroupIcon);
			}
			if (_ribbonGroupImage != null)
			{
				writer.AddValue("Image", _ribbonGroupImage);
			}
			if (_ribbonGroupImageIndex != -1)
			{
				writer.AddValue("ImageIndex", _ribbonGroupImageIndex);
			}
			if (_ribbonGroupImageKey != null)
			{
				writer.AddValue("ImageKey", _ribbonGroupImageKey);
			}
			if (_ribbonGroupItemsSpace != 0)
			{
				writer.AddValue("ItemsSpace", _ribbonGroupItemsSpace);
			}
			if (_ribbonGroupKeyTip.Length > 0)
			{
				writer.AddValue("KeyTip", _ribbonGroupKeyTip);
			}
			if (_dialogButtonKeyTip.Length > 0)
			{
				writer.AddValue("DialogButtonKeyTip", _dialogButtonKeyTip);
			}
			if (_dialogButtonVisible)
			{
				writer.AddValue("DialogButtonVisible", _dialogButtonVisible);
			}
			if (_dialogButtonSuperTipSettings != null && !_dialogButtonSuperTipSettings.IsEmpty)
			{
				writer.AddValue("DialogButtonSuperTipSettings", _dialogButtonSuperTipSettings);
			}
			if (_superTipSettings != null && !_superTipSettings.IsEmpty)
			{
				writer.AddValue("SuperTipSettings", _superTipSettings);
			}
			if (_ribbonGroupText.Length > 0)
			{
				writer.AddValue("Text", _ribbonGroupText);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			_ribbonGroupAllowAutoSizeItems = (bool)LayoutReader.GetPropertyValue(reader, "AllowAutoSizeItems", typeof(bool), true);
			_ribbonGroupCenterItems = (bool)LayoutReader.GetPropertyValue(reader, "CenterItems", typeof(bool), false);
			PropertyValue propertyValue = reader["ForegroundStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_ribbonGroupForegroundStyle);
			}
			else
			{
				_ribbonGroupForegroundStyle.ResetAll();
			}
			propertyValue = reader["SuperTipSettings"] ?? reader["SupperTipSettings"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(SuperTipSettings);
			}
			else
			{
				_superTipSettings = new SuperTipSettings();
				_superTipSettings.ImageListProvider = this;
			}
			propertyValue = reader["DialogButtonSuperTipSettings"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(DialogButtonSuperTipSettings);
			}
			else
			{
				_dialogButtonSuperTipSettings = new SuperTipSettings();
				_dialogButtonSuperTipSettings.ImageListProvider = this;
			}
			_ribbonGroupIcon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_ribbonGroupImage = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			_ribbonGroupImageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_ribbonGroupImageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), "");
			_ribbonGroupItemsSpace = (int)LayoutReader.GetPropertyValue(reader, "ItemsSpace", typeof(int), 0);
			_ribbonGroupKeyTip = (string)LayoutReader.GetPropertyValue(reader, "KeyTip", typeof(string), "");
			_dialogButtonVisible = (bool)LayoutReader.GetPropertyValue(reader, "DialogButtonVisible", typeof(bool), false);
			_ribbonGroupText = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
			_dialogButtonKeyTip = (string)LayoutReader.GetPropertyValue(reader, "DialogButtonKeyTip", typeof(string), "");
			DeserializeRibbonGroupState(reader);
		}

		private void OnSiteChanged()
		{
		}

		private void DeserializeRibbonGroupState(LayoutReader mordexLayoutReader)
		{
			PropertyValue propertyValue = mordexLayoutReader["Commands"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_ribbonGroupCommands);
				return;
			}
			_ribbonGroupCommands.DisposeAll();
		}

		internal void RaiseDialogButtonClick()
		{
			OnDialogButtonClick(EventArgs.Empty);
		}

		protected virtual void OnDialogButtonClick(EventArgs e)
		{
			if (base.Events[s_dialogButtonClickEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		void IInvalidatable.Invalidate()
		{
			InvalidateLayout();
		}
	}
}
