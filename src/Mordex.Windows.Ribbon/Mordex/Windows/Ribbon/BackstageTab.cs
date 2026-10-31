using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Xml;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Ribbon.Design;

namespace Mordex.Windows.Ribbon
{
	public class BackstageTab : CommandBase, IInvalidatable, IKeyTipProvider, IMouseEventInvoker
	{
		private string _text;

		private string _keyTip;

		private Image _image;

		private int _imageIndex;

		private Icon _icon;

		internal SuperTipSettings _superTipSettings;

		private string _imageKey;

		private RibbonForegroundStyle _foregroundStyle;

		private BackstageTabControl _tabPage;

		private bool _isSelected;

		private Font _font;

		private static readonly object s_mouseEnterEventKey = new object();

		private static readonly object s_mouseLeaveEventKey = new object();

		private static readonly object s_mouseMoveEventKey = new object();

		private static readonly object s_mouseDownEventKey = new object();

		private static readonly object s_mouseUpEventKey = new object();

		[Localizable(true)]
		[Description("Represents the text displayed on the tab.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
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
				if (_text != value)
				{
					_text = value;
					NotifyTextChanged("Text");
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public Font Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (value != _font)
				{
					_font = value;
					NotifyTextChanged("Font");
				}
			}
		}

		[Localizable(true)]
		[Editor("Mordex.Windows.Common.Design.SuperTipSettingsEditor, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null", typeof(UITypeEditor))]
		[TypeConverter("Mordex.Windows.Common.Design.SuperTipSettingsConverter, Mordex.Windows.Common.Design, Version=4.0.52.0, Culture=neutral, PublicKeyToken=null")]
		[Description("Represents the settings used to customize the supertip of the command.")]
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
				catch (Exception ex) when (ex is XmlException || ex is FormatException || ex is InvalidCastException || ex is ArgumentException || ex is InvalidOperationException)
				{
					// Invalid saved data keeps the current supertip settings.
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the the color and FontStyle used to draw the command\u00b4s text.")]
		public RibbonForegroundStyle ForegroundStyle => _foregroundStyle;

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

		[Localizable(true)]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Reppresents the image displayed on the tab.")]
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

		[DefaultValue(null)]
		[Localizable(true)]
		[Description("Reppresents the icon displayed on the tab.")]
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

		[Description("Represents the image list index value of the image displayed on the tab.")]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[Localizable(true)]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageIndexConverter))]
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

		[Localizable(true)]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue(null)]
		[Description("Represents the key accessor for the image in the ImageList displayed on the tab.")]
		[TypeConverter(typeof(Mordex.Windows.Ribbon.Design.ImageKeyConverter))]
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public bool Selected
		{
			get
			{
				if (!GetSelectedState() && IsMerged() && base.Ribbon != null && base.Ribbon._mergeHost != null && Equals(base.Ribbon._mergeHost.SelectedTab))
				{
					return true;
				}
				return GetSelectedState();
			}
			set
			{
				if (base.Ribbon != null)
				{
					base.Ribbon.BackstageMenu.SelectedTab = this;
				}
				if (ResolveRootRibbon() != null)
				{
					ResolveRootRibbon().BackstageMenu.SelectedTab = this;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public BackstageTabControl TabPage
		{
			get
			{
				if (_tabPage == null)
				{
					_tabPage = new BackstageTabControl(this);
				}
				return _tabPage;
			}
			set
			{
				if (_tabPage != value)
				{
					if (_tabPage != null)
					{
						_tabPage._owner = null;
					}
					_tabPage = value;
					if (_tabPage != null)
					{
						_tabPage._owner = this;
					}
				}
			}
		}

		internal override CommandType CommandType => CommandType.BackstageTab;

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

		public BackstageTab()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _tabPage != null)
			{
				_tabPage._owner = null;
				_tabPage.Dispose();
			}
			base.Dispose(disposing);
		}

		public BackstageTab(string key)
			: base(key)
		{
		}

		public BackstageTab(string key, string text)
			: this(key, text, -1)
		{
		}

		public BackstageTab(string key, string text, int imageIndex)
			: base(key)
		{
			base.Key = key;
			Text = text;
			_imageIndex = imageIndex;
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

		internal bool GetSelectedState()
		{
			return _isSelected;
		}

		internal void SetSelectedState(bool selected)
		{
			_isSelected = selected;
		}

		internal override void InitializeFields()
		{
			base.InitializeFields();
			_text = "";
			_keyTip = "";
			_imageIndex = -1;
			_foregroundStyle = new RibbonForegroundStyle(this);
			_superTipSettings = new SuperTipSettings();
			_superTipSettings.ImageListProvider = this;
		}

		internal override void Serialize(LayoutWriter writer)
		{
			base.Serialize(writer);
			if (_font != null)
			{
				writer.AddValue("Font", _font);
			}
			if (_foregroundStyle.HasCustomStyle())
			{
				writer.AddValue("ForegroundStyle", _foregroundStyle);
			}
			if (_superTipSettings != null && !_superTipSettings.IsEmpty)
			{
				writer.AddValue("SuperTipSettings", _superTipSettings);
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
			if (_keyTip.Length > 0)
			{
				writer.AddValue("KeyTip", _keyTip);
			}
			if (_text.Length > 0)
			{
				writer.AddValue("Text", _text);
			}
		}

		internal override void Deserialize(LayoutReader reader)
		{
			base.Deserialize(reader);
			_font = (Font)LayoutReader.GetPropertyValue(reader, "Font", typeof(Font), null);
			PropertyValue propertyValue = reader["ForegroundStyle"];
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
			_keyTip = (string)LayoutReader.GetPropertyValue(reader, "KeyTip", typeof(string), "");
			_text = (string)LayoutReader.GetPropertyValue(reader, "Text", typeof(string), "");
		}

		void IInvalidatable.Invalidate()
		{
			InvalidateLayout();
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
	}
}
