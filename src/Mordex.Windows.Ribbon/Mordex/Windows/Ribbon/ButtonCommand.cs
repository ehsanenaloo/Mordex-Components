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
	public class ButtonCommand : CommandBase, IKeyTipProvider, IMouseEventInvoker, IInvalidatable
	{
		private string _text;

		private string _keyTip;

		private Image _image;

		private Image _largeImage;

		private int _imageIndex;

		private int _largeImageIndex;

		private Icon _icon;

		private Icon _largeIcon;

		private string _description;

		private CommandStyle _style;

		internal SuperTipSettings _superTipSettings;

		private CommandSizeStyle _sizeStyle;

		private Shortcut _shortcut;

		private string _imageKey;

		private string _largeImageKey;

		private bool _isChecked;

		private bool _checkOnClick;

		private RibbonForegroundStyle _foregroundStyle;

		private RibbonForegroundStyle _descriptionForegroundStyle;

		private bool _actAsOptionButton;

		private char _mnemonicChar = '0';

		private static readonly object s_mouseEnterEventKey = new object();

		private static readonly object s_mouseLeaveEventKey = new object();

		private static readonly object s_mouseMoveEventKey = new object();

		private static readonly object s_mouseDownEventKey = new object();

		private static readonly object s_mouseUpEventKey = new object();

		private static readonly object s_checkedChangedEventKey = new object();

		public override ISite Site
		{
			get
			{
				return base.Site;
			}
			set
			{
				base.Site = value;
			}
		}

		[DefaultValue("")]
		[Description("Represents the text displayed on the command.")]
		[Localizable(true)]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (!(_text != value))
				{
					return;
				}
				_text = value;
				int mnemonicIndex = _text.IndexOf('&');
				if (mnemonicIndex != -1)
				{
					if (mnemonicIndex < _text.Length - 1)
					{
						HasMnemonic = true;
						_mnemonicChar = _text[mnemonicIndex + 1];
					}
				}
				else
				{
					HasMnemonic = false;
				}
				NotifyTextChanged("Text");
			}
		}

		[Browsable(false)]
		public char Mnemonic => _mnemonicChar;

		[Description("Represents the object that holds the the color and FontStyle used to draw the command\u00b4s text.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RibbonForegroundStyle ForegroundStyle => _foregroundStyle;

		[Description("Represents the object that holds the the color and FontStyle used to draw the command\u00b4s description.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RibbonForegroundStyle DescriptionForegroundStyle => _descriptionForegroundStyle;

		[Localizable(true)]
		[DefaultValue("")]
		[Description("Represents the letter to press after the Alt key to activate the command.")]
		public string KeyTip
		{
			get
			{
				return _keyTip;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_keyTip != value)
				{
					_keyTip = value;
				}
			}
		}

		[Description("Represents the shortcut key associated with the command.")]
		[DefaultValue(Shortcut.None)]
		[Localizable(true)]
		public Shortcut Shortcut
		{
			get
			{
				return _shortcut;
			}
			set
			{
				_shortcut = value;
			}
		}

		[DefaultValue(null)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[Description("Reppresents the image displayed on the command.")]
		public Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				if (_image != value)
				{
					_image = value;
					NotifyTextChanged("Image");
				}
			}
		}

		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Localizable(true)]
		[DefaultValue(null)]
		[Description("Reppresents the image displayed on the command when its SizeStyle is set to Large.")]
		public Image LargeImage
		{
			get
			{
				return _largeImage;
			}
			set
			{
				if (_largeImage != value)
				{
					_largeImage = value;
					NotifyTextChanged("LargeImage");
				}
			}
		}

		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageIndexConverter))]
		[Description("Represents the image list index value of the image displayed on the command.")]
		[Localizable(true)]
		public int ImageIndex
		{
			get
			{
				return _imageIndex;
			}
			set
			{
				if (_imageIndex != value)
				{
					_imageIndex = value;
					NotifyTextChanged("ImageIndex");
				}
			}
		}

		[DefaultValue(-1)]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageIndexConverter))]
		[Editor(typeof(LargeImageIndexEditor), typeof(UITypeEditor))]
		[Description("Represents the image list index value of the image displayed on the command when its SizeStyle is set to Large.")]
		[Localizable(true)]
		public int LargeImageIndex
		{
			get
			{
				return _largeImageIndex;
			}
			set
			{
				if (_largeImageIndex != value)
				{
					_largeImageIndex = value;
					NotifyTextChanged("LargeImageIndex");
				}
			}
		}

		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageKeyConverter))]
		[DefaultValue(null)]
		[Description("Represents the key accessor for the image in the ImageList displayed on the command.")]
		[Localizable(true)]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (_imageKey != value)
				{
					_imageKey = value;
					NotifyTextChanged("ImageKey");
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageKeyConverter))]
		[DefaultValue(null)]
		[Editor(typeof(LargeImageKeyEditor), typeof(UITypeEditor))]
		[Description("Represents the key accessor for the image in the ImageList displayed on the command when its SizeStyle is set to Large.")]
		[Localizable(true)]
		public string LargeImageKey
		{
			get
			{
				return _largeImageKey;
			}
			set
			{
				if (_largeImageKey != value)
				{
					_largeImageKey = value;
					NotifyTextChanged("LargeImageKey");
				}
			}
		}

		[Description("Represents the Icon displayed on the command.")]
		[DefaultValue(null)]
		[Localizable(true)]
		public Icon Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				if (_icon != value)
				{
					_icon = value;
					NotifyTextChanged("Icon");
				}
			}
		}

		[DefaultValue(null)]
		[Description("Represents the Icon displayed on the command when its SizeStyle is set to Large.")]
		[Localizable(true)]
		public Icon LargeIcon
		{
			get
			{
				return _largeIcon;
			}
			set
			{
				if (_largeIcon != value)
				{
					_largeIcon = value;
					NotifyTextChanged("LargeIcon");
				}
			}
		}

		[Description("Specifies how the command is displayed in the Ribbon control.")]
		[DefaultValue(CommandSizeStyle.Default)]
		public virtual CommandSizeStyle SizeStyle
		{
			get
			{
				return _sizeStyle;
			}
			set
			{
				if (_sizeStyle != value)
				{
					_sizeStyle = value;
					NotifyTextChanged("SizeStyle");
				}
			}
		}

		[DefaultValue(false)]
		[Description("Indicates whether the ButtonCommand is pressed or not pressed. ")]
		public bool Checked
		{
			get
			{
				return _isChecked;
			}
			set
			{
				if (_isChecked != value)
				{
					_isChecked = value;
					NotifyCheckedChanged();
				}
			}
		}

		[Description("Indicates whether the command should toggle its selected state when clicked")]
		[DefaultValue(false)]
		public bool CheckOnClick
		{
			get
			{
				return _checkOnClick;
			}
			set
			{
				_checkOnClick = value;
			}
		}

		[Description("Specifies whehter the other buttons in the same container will be unchecked when the user selects the button.")]
		[DefaultValue(false)]
		public bool ActAsOptionButton
		{
			get
			{
				return _actAsOptionButton;
			}
			set
			{
				if (_actAsOptionButton != value)
				{
					_actAsOptionButton = value;
					InvalidateLayout();
				}
			}
		}

		[DefaultValue("")]
		[Description("Specifies the description text that is displayed in menus when the SizeStyle property is set to Large.")]
		[Localizable(true)]
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Value cannot be null");
				}
				if (_description != value)
				{
					_description = value;
					NotifyTextChanged("Description");
				}
			}
		}

		[Description("Represents the settings used to customize the supertip of the command.")]
		[Editor("Mordex.Windows.Common.Design.SuperTipSettingsEditor, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.Common.Design.SuperTipSettingsConverter, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

		[Description("Specifies how the command will be drawn when it is on a Ribbon.")]
		[DefaultValue(CommandStyle.Default)]
		public CommandStyle Style
		{
			get
			{
				return _style;
			}
			set
			{
				if (_style != value)
				{
					_style = value;
					NotifyTextChanged("Style");
				}
			}
		}

		internal override CommandType CommandType
		{
			get
			{
				if (GetSizeStyle() == CommandSizeStyle.Large)
				{
					return CommandType.LargeButton;
				}
				return CommandType.SmallButton;
			}
		}

		bool IKeyTipProvider.HasKeyTip
		{
			get
			{
				if (KeyTip.Length > 0)
				{
					return true;
				}
				return false;
			}
		}

		string IKeyTipProvider.KeyTip => KeyTip;

		[Description("Occurs when the mouse pointer enters the command.")]
		public event EventHandler MouseEnter
		{
			add
			{
				base.Events.AddHandler(s_mouseEnterEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseEnterEventKey, value);
			}
		}

		[Description("Occurs when the mouse pointer leaves the command.")]
		public event EventHandler MouseLeave
		{
			add
			{
				base.Events.AddHandler(s_mouseLeaveEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseLeaveEventKey, value);
			}
		}

		[Description("Occurs when the mouse pointer is moved over the command.")]
		public event MouseEventHandler MouseMove
		{
			add
			{
				base.Events.AddHandler(s_mouseMoveEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseMoveEventKey, value);
			}
		}

		[DefaultValue("Occurs when the mouse pointer is over the command and the left button is pressed.")]
		public event MouseEventHandler MouseDown
		{
			add
			{
				base.Events.AddHandler(s_mouseDownEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseDownEventKey, value);
			}
		}

		[Description("Occurs when the mouse pointer is over the command and the left mouse button is released.")]
		public event MouseEventHandler MouseUp
		{
			add
			{
				base.Events.AddHandler(s_mouseUpEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_mouseUpEventKey, value);
			}
		}

		[Description("Occurs when the Checked property has changed.")]
		public event CommandEventHandler CheckedChanged
		{
			add
			{
				base.Events.AddHandler(s_checkedChangedEventKey, value);
			}
			remove
			{
				base.Events.RemoveHandler(s_checkedChangedEventKey, value);
			}
		}

		public ButtonCommand()
		{
		}

		public ButtonCommand(string key)
			: base(key)
		{
		}

		public ButtonCommand(string key, string text)
			: this(key, text, -1)
		{
		}

		public ButtonCommand(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			Text = text;
			_imageIndex = imageIndex;
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_text = "";
			_keyTip = "";
			_description = "";
			_style = CommandStyle.Default;
			_sizeStyle = CommandSizeStyle.Default;
			_shortcut = Shortcut.None;
			_imageIndex = -1;
			_largeImageIndex = -1;
			_foregroundStyle = new RibbonForegroundStyle(this);
			_descriptionForegroundStyle = new RibbonForegroundStyle(this);
			_superTipSettings = new SuperTipSettings();
			_superTipSettings.ImageListProvider = this;
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

		internal CommandStyle GetEffectiveCommandStyle()
		{
			if (_style == CommandStyle.Default)
			{
				return CommandStyle.ImageAndText;
			}
			return _style;
		}

		internal virtual CommandSizeStyle GetSizeStyle()
		{
			if (SizeStyle == CommandSizeStyle.Default)
			{
				if (GetParentContainer() != null)
				{
					return GetParentContainer().DefaultSizeStyle;
				}
				return CommandSizeStyle.Small;
			}
			return SizeStyle;
		}

		private protected override void ApplyClickState()
		{
			if ((_checkOnClick || _actAsOptionButton) && (!Checked || !_actAsOptionButton))
			{
				Checked = !Checked;
			}
		}

		internal void NotifyCheckedChanged()
		{
			CommandEventArgs e = new CommandEventArgs(this);
			if (GetParentContainer() != null)
			{
				GetParentContainer().CommandCheckedChanged(e);
			}
			InvalidateLayout();
			OnCheckedChanged(e);
		}

		internal override bool SearchShortcut(Shortcut shortcut, Control inputControl, Message msg)
		{
			if (!base.Visible || !Enabled)
			{
				return false;
			}
			if (Shortcut == shortcut)
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
			return false;
		}

		public override object Clone()
		{
			ButtonCommand buttonCommand = new ButtonCommand();
			buttonCommand._description = _description;
			buttonCommand.Enabled = Enabled;
			buttonCommand._icon = _icon;
			buttonCommand._image = _image;
			buttonCommand._imageIndex = _imageIndex;
			buttonCommand.Key = base.Key;
			buttonCommand._keyTip = _keyTip;
			buttonCommand._largeIcon = _largeIcon;
			buttonCommand._largeImage = _largeImage;
			buttonCommand._largeImageIndex = _largeImageIndex;
			buttonCommand._shortcut = _shortcut;
			buttonCommand._sizeStyle = _sizeStyle;
			buttonCommand._style = _style;
			buttonCommand._checkOnClick = _checkOnClick;
			buttonCommand._isChecked = _isChecked;
			buttonCommand._actAsOptionButton = _actAsOptionButton;
			buttonCommand._imageKey = _imageKey;
			buttonCommand._largeImageKey = _largeImageKey;
			buttonCommand.AllowAddToQuickAccessBar = base.AllowAddToQuickAccessBar;
			if (_superTipSettings != null)
			{
				buttonCommand._superTipSettings = SuperTipSettings.Clone();
				buttonCommand._superTipSettings.ImageListProvider = buttonCommand;
			}
			buttonCommand._text = _text;
			buttonCommand.Visible = base.Visible;
			buttonCommand._foregroundStyle = _foregroundStyle.CloneForOwner(buttonCommand);
			buttonCommand._descriptionForegroundStyle = _descriptionForegroundStyle.CloneForOwner(buttonCommand);
			return buttonCommand;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (_actAsOptionButton)
			{
				writer.AddValue("ActAsOptionButton", _actAsOptionButton);
			}
			if (_checkOnClick)
			{
				writer.AddValue("CheckOnClick", _checkOnClick);
			}
			if (_description.Length > 0)
			{
				writer.AddValue("Description", _description);
			}
			if (_descriptionForegroundStyle.HasCustomStyle())
			{
				writer.AddValue("DescriptionForegroundStyle", _descriptionForegroundStyle);
			}
			if (_foregroundStyle.HasCustomStyle())
			{
				writer.AddValue("ForegroundStyle", _foregroundStyle);
			}
			if (_icon != null)
			{
				writer.AddValue("Icon", _icon);
			}
			if (_image != null)
			{
				writer.AddValue("Image", _image);
			}
			if (_imageIndex != -1)
			{
				writer.AddValue("ImageIndex", _imageIndex);
			}
			if (_imageKey != null)
			{
				writer.AddValue("ImageKey", _imageKey);
			}
			if (_isChecked)
			{
				writer.AddValue("Checked", _isChecked);
			}
			if (_keyTip.Length > 0)
			{
				writer.AddValue("KeyTip", _keyTip);
			}
			if (_largeIcon != null)
			{
				writer.AddValue("LargeIcon", _largeIcon);
			}
			if (_largeImage != null)
			{
				writer.AddValue("LargeImage", _largeImage);
			}
			if (_largeImageIndex != -1)
			{
				writer.AddValue("LargeImageIndex", _largeImageIndex);
			}
			if (_largeImageKey != null)
			{
				writer.AddValue("LargeImageKey", _largeImageKey);
			}
			if (_shortcut != Shortcut.None)
			{
				writer.AddValue("Shortcut", _shortcut);
			}
			if (_sizeStyle != CommandSizeStyle.Default)
			{
				writer.AddValue("SizeStyle", _sizeStyle);
			}
			if (_style != CommandStyle.Default)
			{
				writer.AddValue("Style", _style);
			}
			if (_superTipSettings != null && !_superTipSettings.IsEmpty)
			{
				writer.AddValue("SuperTipSettings", _superTipSettings);
			}
			if (_text.Length > 0)
			{
				writer.AddValue("Text", _text);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			_actAsOptionButton = (bool)LayoutReader.GetPropertyValue(reader, "ActAsOptionButton", typeof(bool), false);
			_checkOnClick = (bool)LayoutReader.GetPropertyValue(reader, "CheckOnClick", typeof(bool), false);
			_description = (string)LayoutReader.GetPropertyValue(reader, "Description", typeof(string), "");
			PropertyValue propertyValue = reader["DescriptionForegroundStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_descriptionForegroundStyle);
			}
			else
			{
				_descriptionForegroundStyle.ResetAll();
			}
			propertyValue = reader["ForegroundStyle"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(_foregroundStyle);
			}
			else
			{
				_foregroundStyle.ResetAll();
			}
			propertyValue = reader["SuperTipSettings"];
			if (propertyValue != null)
			{
				propertyValue.DeserializeObject(SuperTipSettings);
			}
			else
			{
				_superTipSettings = new SuperTipSettings();
				_superTipSettings.ImageListProvider = this;
			}
			_icon = (Icon)LayoutReader.GetPropertyValue(reader, "Icon", typeof(Icon), null);
			_image = (Image)LayoutReader.GetPropertyValue(reader, "Image", typeof(Image), null);
			_imageIndex = (int)LayoutReader.GetPropertyValue(reader, "ImageIndex", typeof(int), -1);
			_imageKey = (string)LayoutReader.GetPropertyValue(reader, "ImageKey", typeof(string), null);
			_isChecked = (bool)LayoutReader.GetPropertyValue(reader, "Checked", typeof(bool), false);
			_keyTip = (string)LayoutReader.GetPropertyValue(reader, "KeyTip", typeof(string), "");
			_largeIcon = (Icon)LayoutReader.GetPropertyValue(reader, "LargeIcon", typeof(Icon), null);
			_largeImage = (Image)LayoutReader.GetPropertyValue(reader, "LargeImage", typeof(Image), null);
			_largeImageIndex = (int)LayoutReader.GetPropertyValue(reader, "LargeImageIndex", typeof(int), -1);
			_largeImageKey = (string)LayoutReader.GetPropertyValue(reader, "LargeImageKey", typeof(string), null);
			_shortcut = (Shortcut)LayoutReader.GetPropertyValue(reader, "Shortcut", typeof(Shortcut), Shortcut.None);
			_sizeStyle = (CommandSizeStyle)LayoutReader.GetPropertyValue(reader, "SizeStyle", typeof(CommandSizeStyle), CommandSizeStyle.Default);
			_style = (CommandStyle)LayoutReader.GetPropertyValue(reader, "Style", typeof(CommandStyle), CommandStyle.Default);
			_text = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
		}

		void IMouseEventInvoker.InvokeMouseEnter()
		{
			if (IsEnabled)
			{
				OnMouseEnter(EventArgs.Empty);
			}
		}

		void IMouseEventInvoker.InvokeMouseLeave()
		{
			if (IsEnabled)
			{
				OnMouseLeave(EventArgs.Empty);
			}
		}

		void IMouseEventInvoker.InvokeMouseMove(MouseEventArgs eventArgs)
		{
			if (IsEnabled)
			{
				OnMouseMove(eventArgs);
			}
		}

		void IMouseEventInvoker.InvokeMouseDown(MouseEventArgs eventArgs)
		{
			if (IsEnabled)
			{
				OnMouseDown(eventArgs);
			}
		}

		void IMouseEventInvoker.InvokeMouseUp(MouseEventArgs eventArgs)
		{
			if (IsEnabled)
			{
				OnMouseUp(eventArgs);
			}
		}

		protected virtual void OnMouseEnter(EventArgs e)
		{
			if (base.Events[s_mouseEnterEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnMouseLeave(EventArgs e)
		{
			if (base.Events[s_mouseLeaveEventKey] is EventHandler eventHandler)
			{
				eventHandler(this, e);
			}
		}

		protected virtual void OnMouseMove(MouseEventArgs e)
		{
			if (base.Events[s_mouseMoveEventKey] is MouseEventHandler mouseEventHandler)
			{
				mouseEventHandler(this, e);
			}
		}

		protected virtual void OnMouseDown(MouseEventArgs e)
		{
			if (base.Events[s_mouseDownEventKey] is MouseEventHandler mouseEventHandler)
			{
				mouseEventHandler(this, e);
			}
		}

		protected virtual void OnMouseUp(MouseEventArgs e)
		{
			if (base.Events[s_mouseUpEventKey] is MouseEventHandler mouseEventHandler)
			{
				mouseEventHandler(this, e);
			}
		}

		protected virtual void OnCheckedChanged(CommandEventArgs e)
		{
			if (base.Events[s_checkedChangedEventKey] is CommandEventHandler commandEventHandler)
			{
				commandEventHandler(this, e);
			}
		}

		void IInvalidatable.Invalidate()
		{
			InvalidateLayout();
		}
	}
}
