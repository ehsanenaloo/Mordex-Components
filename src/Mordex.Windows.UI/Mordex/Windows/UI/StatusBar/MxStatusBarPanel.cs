using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.StatusBar
{
	[TypeConverter(typeof(StatusBarPanelConverter))]
	public class MxStatusBarPanel : IFormatStyleOwner
	{
		private Color _borderColor = Color.Empty;

		private string _key = "";

		private string _text = "";

		private ToggleKeyValue _toggleKeyValue = ToggleKeyValue.CapsLock;

		private bool _isOn;

		private string _toggleKeyOnText = "";

		private string _toggleKeyOffText = "";

		private StatusBarPanelType _panelType;

		private int _imageIndex = -1;

		private string _imageKey = "";

		private Image _image;

		private string _toolTipText = "";

		private Icon _icon;

		private FormatStyle _formatStyle;

		private FormatStyle _hotFormatStyle;

		private Control _control;

		private int _progressBarValue;

		private int _progressBarMinValue;

		private int _progressBarMaxValue = 100;

		private bool _drawBorder = true;

		private int _minWidth = 10;

		internal int _width = 100;

		private HorizontalAlignment _alignment;

		private StatusBarPanelAutoSize _autoSize = StatusBarPanelAutoSize.None;

		private bool _visible = true;

		private bool _isHot;

		internal Rectangle _contentBounds = Rectangle.Empty;

		internal Rectangle _imageBounds = Rectangle.Empty;

		internal Rectangle _bounds = Rectangle.Empty;

		internal StatusBarPanelCollection _owner;

		Control IFormatStyleOwner.Owner => null;

		[Description("Represents the color of the border around a the panel. This property works only when the control is not painted using Windows XP Visual Stlyes.")]
		public Color BorderColor
		{
			get
			{
				return _borderColor;
			}
			set
			{
				if (_borderColor != value)
				{
					_borderColor = value;
					NotifyOwner();
				}
			}
		}

		[Description("Represents the key used to identify the panel in a collection")]
		public string Key
		{
			get
			{
				return _key;
			}
			set
			{
				_key = value;
			}
		}

		[DefaultValue("")]
		[Description("Represents the text displayed for this panel.")]
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
					value = "";
				}
				if (_text != value)
				{
					_text = value;
					NotifyOwner();
				}
			}
		}

		[DefaultValue(ToggleKeyValue.CapsLock)]
		[Description("Specifyng which toggle key the StatusBarPanel will monitor its state.")]
		public ToggleKeyValue ToggleKeyValue
		{
			get
			{
				return _toggleKeyValue;
			}
			set
			{
				if (_toggleKeyValue != value)
				{
					_toggleKeyValue = value;
					if (_owner != null && _owner._statusBar != null)
					{
						_owner._statusBar.RefreshToggleKeyStates();
					}
				}
			}
		}

		[DefaultValue("")]
		[Description("Specifies the Text of the panel when the assigned toggle key is on.")]
		public string ToggleKeyOnText
		{
			get
			{
				return _toggleKeyOnText;
			}
			set
			{
				if (_toggleKeyOnText != value)
				{
					_toggleKeyOnText = value;
					if (PanelType == StatusBarPanelType.ToggleKeyState)
					{
						NotifyOwner();
					}
				}
			}
		}

		[Description("Specifies the Text of the panel when the assigned toggle key is off.")]
		[DefaultValue("")]
		public string ToggleKeyOffText
		{
			get
			{
				return _toggleKeyOffText;
			}
			set
			{
				if (_toggleKeyOffText != value)
				{
					_toggleKeyOffText = value;
					if (_panelType == StatusBarPanelType.ToggleKeyState)
					{
						NotifyOwner();
					}
				}
			}
		}

		[Description("Indicates the type of the panel.")]
		[DefaultValue(StatusBarPanelType.Item)]
		public StatusBarPanelType PanelType
		{
			get
			{
				return _panelType;
			}
			set
			{
				if (!Enum.IsDefined(typeof(StatusBarPanelType), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_panelType != value)
				{
					if (_control != null && _owner != null)
					{
						_owner._statusBar.RemovePanelControl(this);
					}
					_panelType = value;
					NotifyOwner();
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the index of the image in the ImageList associated with this panel.")]
		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageIndexConverter))]
		[Editor(typeof(ImageIndexEditor), typeof(UITypeEditor))]
		[DefaultValue(-1)]
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
					_imageKey = "";
					NotifyOwner();
				}
			}
		}

		[TypeConverter(typeof(Mordex.Windows.UI.Design.ImageKeyConverter))]
		[Description("Represents the key accessor for the image in the ImageList associated with this panel.")]
		[Editor(typeof(ImageKeyEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		[Localizable(true)]
		public string ImageKey
		{
			get
			{
				return _imageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_imageKey != value)
				{
					_imageKey = value;
					_imageIndex = -1;
					NotifyOwner();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		[Description("The image that is displayed on the panel.")]
		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
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
					NotifyOwner();
				}
			}
		}

		[Description("The text that is shown when the mouse hovers over this panel.")]
		[DefaultValue("")]
		[Localizable(true)]
		public string ToolTipText
		{
			get
			{
				return _toolTipText;
			}
			set
			{
				_toolTipText = value;
			}
		}

		[DefaultValue(null)]
		[Localizable(true)]
		[Description("The icon that is displayed on the panel.")]
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
					NotifyOwner();
				}
			}
		}

		[Description("The format style properties used to draw the panel.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle FormatStyle
		{
			get
			{
				if (_formatStyle == null)
				{
					_formatStyle = new FormatStyle(this);
				}
				return _formatStyle;
			}
		}

		[Description("Represent the format style used to draw the panels when the mouse is over them.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle HotFormatStyle
		{
			get
			{
				if (_hotFormatStyle == null)
				{
					_hotFormatStyle = new FormatStyle(this);
				}
				return _hotFormatStyle;
			}
		}

		[DefaultValue(null)]
		[Description("Represents the System.Windows.Forms.Control associated with this panel.")]
		[Editor(typeof(StatusControlEditor), typeof(UITypeEditor))]
		public Control Control
		{
			get
			{
				return _control;
			}
			set
			{
				if (_control != value)
				{
					if (_control != null && _owner != null)
					{
						_owner._statusBar.RemovePanelControl(this);
					}
					_control = value;
					NotifyOwner();
				}
			}
		}

		[DefaultValue(null)]
		[Description("Specifies the current position of the progress bar")]
		[Localizable(true)]
		public int ProgressBarValue
		{
			get
			{
				return _progressBarValue;
			}
			set
			{
				if (_progressBarValue != value)
				{
					if (value > _progressBarMaxValue || value < _progressBarMinValue)
					{
						throw new Exception("'" + value + "' is is not a valid value for 'ProgressBarValue'. 'ProgressBarValue' should be between 'ProgressBarMinValue' and 'ProgressMaxValue'.");
					}
					_progressBarValue = value;
					if (_owner != null)
					{
						_owner._statusBar.Invalidate(_bounds);
						_owner._statusBar.Update();
					}
				}
			}
		}

		[Description("Specifies the minimum value of the progress bar")]
		[DefaultValue(0)]
		[Localizable(true)]
		public int ProgressBarMinValue
		{
			get
			{
				return _progressBarMinValue;
			}
			set
			{
				if (_progressBarMinValue != value)
				{
					if (value > _progressBarMaxValue)
					{
						_progressBarMaxValue = value;
					}
					_progressBarMinValue = value;
					if (_owner != null)
					{
						_owner._statusBar.Invalidate(_bounds);
						_owner._statusBar.Update();
					}
				}
			}
		}

		[DefaultValue(100)]
		[Description("Specifies the maximum value of the progress bar")]
		[Localizable(true)]
		public int ProgressBarMaxValue
		{
			get
			{
				return _progressBarMaxValue;
			}
			set
			{
				if (_progressBarMaxValue != value)
				{
					if (value < _progressBarMinValue)
					{
						_progressBarMinValue = value;
					}
					_progressBarMaxValue = value;
					if (ProgressBarValue > _progressBarMaxValue)
					{
						ProgressBarValue = _progressBarMaxValue;
					}
					InvalidateBounds();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether a border is drawn around the group.")]
		[Localizable(true)]
		public bool DrawBorder
		{
			get
			{
				return _drawBorder;
			}
			set
			{
				if (_drawBorder != value)
				{
					_drawBorder = value;
					InvalidateBounds();
				}
			}
		}

		[Description("Represents the minimum width of the status bar panel whithin the UIStatusBar control.")]
		[DefaultValue(10)]
		[Localizable(true)]
		public int MinWidth
		{
			get
			{
				return _minWidth;
			}
			set
			{
				if (_minWidth != value)
				{
					_minWidth = value;
					NotifyOwner();
				}
			}
		}

		[Description("Represents the width of the status bar panel whithin the UIStatusBar control.")]
		[DefaultValue(100)]
		[Localizable(true)]
		public int Width
		{
			get
			{
				return _width;
			}
			set
			{
				if (_width != value)
				{
					_width = value;
					NotifyOwner();
				}
			}
		}

		[DefaultValue(HorizontalAlignment.Left)]
		[Description("Indicates the alignment of the text and icons whithin the status bar panel.")]
		[Localizable(true)]
		public HorizontalAlignment Alignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				if (_alignment != value)
				{
					_alignment = value;
					NotifyOwner();
				}
			}
		}

		[Description("Indicates whether the status bar panel is autosized.")]
		[DefaultValue(StatusBarPanelAutoSize.None)]
		[Localizable(false)]
		public StatusBarPanelAutoSize AutoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					NotifyOwner();
				}
			}
		}

		[Description("Indicates whether the panel is visible or hidded.")]
		[Localizable(true)]
		[DefaultValue(true)]
		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				if (_visible != value)
				{
					_visible = value;
					if (_control != null)
					{
						_control.Visible = value;
					}
					NotifyOwner();
				}
			}
		}

		[Browsable(false)]
		public Rectangle Bounds => _bounds;

		void IFormatStyleOwner.PropertyChange()
		{
			NotifyOwner();
		}

		public MxStatusBarPanel()
		{
		}

		public MxStatusBarPanel(string text)
		{
			_text = text;
		}

		public MxStatusBarPanel(string text, int imageIndex)
		{
			_text = text;
			_imageIndex = imageIndex;
		}

		internal string GetDisplayText()
		{
			if (_panelType == StatusBarPanelType.Item)
			{
				return _text;
			}
			if (_panelType == StatusBarPanelType.ToggleKeyState)
			{
				if (!_isOn)
				{
					return _toggleKeyOffText;
				}
				if (_toggleKeyOnText.Length > 0)
				{
					return _toggleKeyOnText;
				}
				switch (ToggleKeyValue)
				{
				case ToggleKeyValue.CapsLock:
					return "CAPS";
				case ToggleKeyValue.NumLock:
					return "NUM";
				case ToggleKeyValue.Insert:
					return "OVR";
				case ToggleKeyValue.ScrollLock:
					return "SCRL";
				}
			}
			return "";
		}

		internal void SetIsOn(bool isOn)
		{
			if (_isOn != isOn)
			{
				_isOn = isOn;
				if (_owner != null)
				{
					NotifyOwner();
				}
			}
		}

		internal bool GetIsHot()
		{
			return _isHot;
		}

		internal void SetIsHot(bool isHot)
		{
			if (_isHot != isHot)
			{
				_isHot = isHot;
				InvalidateBounds();
			}
		}

		private void NotifyOwner()
		{
			if (_owner != null)
			{
				_owner.NotifyChanged();
			}
		}

		private void InvalidateBounds()
		{
			if (_owner != null)
			{
				_owner._statusBar.Invalidate(_bounds);
			}
		}
	}
}
