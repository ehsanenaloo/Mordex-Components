using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.DataGrid.Design;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid.Editors
{
	public partial class EditBase
	{
		protected override Size DefaultSize => new Size(100, 20);

		protected bool TreatControlAreaAsButton
		{
			get
			{
				return _treatControlAreaAsButton;
			}
			set
			{
				_treatControlAreaAsButton = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override Image BackgroundImage
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public new virtual Color BackColor
		{
			get
			{
				if (!ShouldSerializeBackColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(_readOnly ? ThemeRole.InputBackDisabled : ThemeRole.InputBack);
				}
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
			}
		}

		public new Color ForeColor
		{
			get
			{
				if (!ShouldSerializeForeColor())
				{
					return MxThemeManager.DefaultTheme.GetColor(ThemeRole.Text);
				}
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		protected virtual bool IsDropDownPushed
		{
			get
			{
				if (_buttonStates[0]._isPressed)
				{
					return base.Enabled;
				}
				return false;
			}
		}

		public override bool AllowDrop
		{
			get
			{
				return _allowDrop;
			}
			set
			{
				if (value != _allowDrop)
				{
					_allowDrop = value;
					if (_textBox != null)
					{
						base.AllowDrop = false;
						_textBox.AllowDrop = _allowDrop;
					}
					else
					{
						base.AllowDrop = _allowDrop;
					}
				}
			}
		}

		[DefaultValue(HoverMode.None)]
		[Description("Determines the effect used in the control when the mouse hovers over it.")]
		public HoverMode HoverMode
		{
			get
			{
				return _hoverMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(HoverMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_hoverMode != value)
				{
					_hoverMode = value;
					RecalculateLayout();
				}
			}
		}

		[Description("Specifies the border style for the control.")]
		[DefaultValue(BorderStyle.Sunken)]
		public BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _borderStyle)
				{
					_borderStyle = value;
					Invalidate();
					RecalculateLayout();
				}
			}
		}

		public override Rectangle DisplayRectangle
		{
			get
			{
				int borderWidth = GetBorderWidth();
				if (borderWidth == 1)
				{
					Rectangle result = Rectangle.Inflate(base.ClientRectangle, -borderWidth, -borderWidth);
					result.Width--;
					if (RightToLeft != RightToLeft.Yes)
					{
						result.X++;
					}
					return result;
				}
				return Rectangle.Inflate(base.ClientRectangle, -borderWidth, -borderWidth);
			}
		}

		protected virtual TextBox DefaultEditControl => null;

		protected TextBox EditControl
		{
			get
			{
				return _textBox;
			}
			set
			{
				if (_textBox == value)
				{
					return;
				}
				if (_textBox != null)
				{
					_textBox.MouseClick -= OnEditControlMouseClick;
					_textBox.MouseDoubleClick -= OnEditControlMouseDoubleClick;
					_textBox.Click -= OnEditControlClick;
					_textBox.DoubleClick -= OnEditControlDoubleClick;
					_textBox.DragEnter -= OnEditControlDragEnter;
					_textBox.DragLeave -= OnEditControlDragLeave;
					_textBox.DragDrop -= OnEditControlDragDrop;
					_textBox.DragOver -= OnEditControlDragOver;
					_textBox.GotFocus -= OnEditControlGotFocus;
					_textBox.KeyDown -= OnEditControlKeyDown;
					_textBox.KeyPress -= OnEditControlKeyPress;
					_textBox.KeyUp -= OnEditControlKeyUp;
					_textBox.LostFocus -= OnEditControlLostFocus;
					_textBox.MouseDown -= OnEditControlMouseDown;
					_textBox.MouseEnter -= OnEditControlMouseEnter;
					_textBox.MouseHover -= OnEditControlMouseHover;
					_textBox.MouseLeave -= OnEditControlMouseLeave;
					_textBox.MouseMove -= OnEditControlMouseMove;
					_textBox.MouseUp -= OnEditControlMouseUp;
					_textBox.MouseWheel -= OnEditControlMouseWheel;
					_textBox.TextChanged -= OnEditControlTextChanged;
					base.Controls.Remove(_textBox);
					base.ContextMenuStrip = _contextMenuStrip;
					base.AllowDrop = _allowDrop;
				}
				_textBox = value;
				if (_textBox != null)
				{
					base.AllowDrop = false;
					_textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
					_textBox.Text = Text;
					_textBox.BackColor = BackColor;
					_textBox.ForeColor = ForeColor;
					_textBox.ReadOnly = _readOnly;
					_textBox.AllowDrop = _allowDrop;
					ApplyTextAlignment();
					_textBox.CharacterCasing = _characterCasing;
					_textBox.HideSelection = _hideSelection;
					_textBox.MaxLength = _maxLength;
					_textBox.RightToLeft = RightToLeft.Inherit;
					_textBox.Font = null;
					if (!(_textBox is EditBaseTextBox))
					{
						_textBox.Click += OnEditControlClick;
					}
					_textBox.DoubleClick += OnEditControlDoubleClick;
					_textBox.DragEnter += OnEditControlDragEnter;
					_textBox.DragLeave += OnEditControlDragLeave;
					_textBox.DragOver += OnEditControlDragOver;
					_textBox.GotFocus += OnEditControlGotFocus;
					_textBox.KeyDown += OnEditControlKeyDown;
					_textBox.KeyPress += OnEditControlKeyPress;
					_textBox.KeyUp += OnEditControlKeyUp;
					_textBox.LostFocus += OnEditControlLostFocus;
					_textBox.MouseDown += OnEditControlMouseDown;
					_textBox.Resize += OnEditControlResize;
					_textBox.MouseEnter += OnEditControlMouseEnter;
					_textBox.MouseHover += OnEditControlMouseHover;
					_textBox.MouseLeave += OnEditControlMouseLeave;
					_textBox.MouseMove += OnEditControlMouseMove;
					_textBox.MouseUp += OnEditControlMouseUp;
					_textBox.MouseWheel += OnEditControlMouseWheel;
					_textBox.TextChanged += OnEditControlTextChanged;
					_textBox.DragDrop += OnEditControlDragDrop;
					_textBox.MouseClick += OnEditControlMouseClick;
					_textBox.MouseDoubleClick += OnEditControlMouseDoubleClick;
					_textBox.TabStop = base.TabStop;
					if (_useDefaultMenu)
					{
						_textBox.ContextMenuStrip = _contextMenuStrip;
					}
					else
					{
						_textBox.ContextMenuStrip = new ContextMenuStrip();
					}
					_textBox.ContextMenuStrip = _contextMenuStrip;
					base.ContextMenuStrip = null;
					_textBox.Visible = base.Enabled;
					_textBox.ShortcutsEnabled = _shortcutsEnabled;
					base.Controls.Add(_textBox);
				}
				RecalculateLayout();
			}
		}

		[Browsable(false)]
		public TextBox TextBox => _textBox;

		protected EditButtonType ButtonType
		{
			get
			{
				return _buttonType;
			}
			set
			{
				if (_buttonType != value)
				{
					_buttonType = value;
					RecalculateLayout();
				}
			}
		}

		[Description("Indicates the color of the flat border of the control and button.")]
		public Color FlatBorderColor
		{
			get
			{
				return _officeStyle.GetFlatBorderColor();
			}
			set
			{
				if (value != FlatBorderColor)
				{
					_officeStyle.SetFlatBorderColor(value);
					Invalidate();
				}
			}
		}

		[Description("Indicates whether the control's height is automatically resized to display the font.")]
		[DefaultValue(true)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public override bool AutoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				if (value != _autoSize)
				{
					_autoSize = value;
					RecalculateLayout();
				}
			}
		}

		Font IGridStyleSource.ControlStyleFont => Font;

		bool IGridStyleSource.IsRightToLeft => RightToLeft == RightToLeft.Yes;

		[Description("The Image to display in the control.")]
		[Localizable(true)]
		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(null)]
		public Image Image
		{
			get
			{
				return _displayImage;
			}
			set
			{
				if (_displayImage != value)
				{
					_displayImage = value;
					_editBaseImage = value;
					RecalculateLayout();
				}
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.EditBoxImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[Localizable(true)]
		[Editor("Mordex.Windows.DataGrid.Design.EditBoxImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[DefaultValue(-1)]
		[Description("The image list index value of the image to display in the control.")]
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
					RecalculateLayout();
				}
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.EditBoxImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue("")]
		[Editor("Mordex.Windows.DataGrid.Design.EditBoxImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
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
					if (value.Length > 0)
					{
						_imageIndex = -1;
					}
					_imageKey = value;
					RecalculateLayout();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The ImageList that contains the images used in the control.")]
		public ImageList ImageList
		{
			get
			{
				return _imageList;
			}
			set
			{
				if (_imageList != value)
				{
					_imageList = value;
					OnImageListChanged(EventArgs.Empty);
					RecalculateLayout();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the control should display an image.")]
		public virtual bool HasImage
		{
			get
			{
				return _hasImage;
			}
			set
			{
				if (_hasImage != value)
				{
					_hasImage = value;
					RecalculateLayout();
				}
			}
		}

		[DefaultValue(true)]
		[Description("Indicates whether the control should display text.")]
		public virtual bool HasText
		{
			get
			{
				return _isTextPresent;
			}
			set
			{
				if (_isTextPresent != value)
				{
					_isTextPresent = value;
					RecalculateLayout();
				}
			}
		}

		[DefaultValue(ImageHorizontalAlignment.BeforeText)]
		[Description("Determines the horizontal alignment of the image.")]
		public ImageHorizontalAlignment ImageHorizontalAlignment
		{
			get
			{
				return _imageHorizontalAlignment;
			}
			set
			{
				if (_imageHorizontalAlignment != value)
				{
					_imageHorizontalAlignment = value;
					RecalculateLayout();
				}
			}
		}

		[DefaultValue(ImageVerticalAlignment.Center)]
		[Description("Determines the vertical alignment of the image.")]
		public ImageVerticalAlignment ImageVerticalAlignment
		{
			get
			{
				return _imageVerticalAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ImageVerticalAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value == ImageVerticalAlignment.Empty)
				{
					value = ImageVerticalAlignment.Center;
				}
				if (_imageVerticalAlignment != value)
				{
					_imageVerticalAlignment = value;
					RecalculateLayout();
				}
			}
		}

		[Description("Determines how the text is aligned in the control.")]
		public TextAlignment TextAlignment
		{
			get
			{
				if (_textAlignment == TextAlignment.Empty)
				{
					return DefaultTextAlignment;
				}
				return _textAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_textAlignment != value)
				{
					_textAlignment = value;
					_isTextAlignmentDirty = true;
					Invalidate();
				}
			}
		}

		protected virtual TextAlignment DefaultTextAlignment => TextAlignment.Near;

		[Description("The ControlStyle object used to draw buttons and scroll bars in the control.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ControlStyle ControlStyle => _officeStyle;

		[Description("Indicates whether text in the edit box is read-only.")]
		[DefaultValue(false)]
		public bool ReadOnly
		{
			get
			{
				return _readOnly;
			}
			set
			{
				if (_readOnly != value)
				{
					_readOnly = value;
					if (_textBox != null)
					{
						_textBox.ReadOnly = value;
						ApplyThemedTextBoxColors();
						Invalidate();
					}
					else
					{
						Invalidate();
					}
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(EditButtonStyle.NoButton)]
		[Description("Determines which button style will be used in the control.")]
		public EditButtonStyle ButtonStyle
		{
			get
			{
				return _buttonStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(EditButtonStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (_buttonStyle != value)
				{
					_buttonStyle = value;
					RecalculateLayout();
				}
			}
		}

		[DefaultValue(null)]
		[Editor("Mordex.Windows.DataGrid.Design.ImageEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		[Description("The image drawn in the button when the ButtonStyle property is set to Image.")]
		public Image ButtonImage
		{
			get
			{
				return _buttonImage;
			}
			set
			{
				if (_buttonImage != value)
				{
					_buttonImage = value;
					if (_buttonStyle == EditButtonStyle.Image)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[Localizable(true)]
		[Description("The text drawn in the button when ButtonStyle.TextButton is used.")]
		[DefaultValue("")]
		public string ButtonText
		{
			get
			{
				return _buttonCaption;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (_buttonCaption != value)
				{
					_buttonCaption = value;
					if (_buttonStyle == EditButtonStyle.TextButton)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[Description("The font of the text displayed in the button area of the control.")]
		public Font ButtonFont
		{
			get
			{
				if (_buttonFont == null)
				{
					return Font;
				}
				return _buttonFont;
			}
			set
			{
				if (_buttonFont != value)
				{
					_buttonFont = value;
					if (_buttonStyle == EditButtonStyle.TextButton)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[Description("Indicates the size in which button images should be drawn.")]
		public Size ButtonImageSize
		{
			get
			{
				return _buttonImageSize;
			}
			set
			{
				if (_buttonImageSize != value)
				{
					_buttonImageSize = value;
					if (_buttonStyle == EditButtonStyle.Image)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[TypeConverter("Mordex.Windows.DataGrid.Design.EditBoxImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[DefaultValue(-1)]
		[Description("The image list index value of the image drawn in the button when ButtonStyle.Image is used.")]
		[Editor("Mordex.Windows.DataGrid.Design.EditBoxImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public virtual int ButtonImageIndex
		{
			get
			{
				return _buttonImageIndex;
			}
			set
			{
				if (_buttonImageIndex != value)
				{
					_buttonImageIndex = value;
					if (_buttonImageIndex != -1)
					{
						_buttonImageKey = "";
					}
					if (_buttonStyle == EditButtonStyle.Image)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[DefaultValue("")]
		[TypeConverter("Mordex.Windows.DataGrid.Design.EditBoxImageIndexConverter, Mordex.Windows.DataGrid.Design")]
		[Editor("Mordex.Windows.DataGrid.Design.EditBoxImageIndexEditor, Mordex.Windows.DataGrid.Design", typeof(UITypeEditor))]
		public virtual string ButtonImageKey
		{
			get
			{
				return _buttonImageKey;
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				if (_buttonImageKey != value)
				{
					_buttonImageKey = value;
					if (_buttonImageKey.Length > 0)
					{
						_buttonImageIndex = -1;
					}
					if (_buttonStyle == EditButtonStyle.Image)
					{
						RecalculateLayout();
					}
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual bool UseHighlightColorCore
		{
			get
			{
				if (_isHighlightColorUsed)
				{
					return base.Focused;
				}
				return false;
			}
			set
			{
				if (_isHighlightColorUsed != value)
				{
					_isHighlightColorUsed = value;
					Invalidate();
				}
			}
		}

		public override bool Focused => base.ContainsFocus;

		public override ContextMenuStrip ContextMenuStrip
		{
			get
			{
				return _contextMenuStrip;
			}
			set
			{
				_contextMenuStrip = value;
				if (_textBox != null)
				{
					_textBox.ContextMenuStrip = _contextMenuStrip;
				}
				else
				{
					base.ContextMenuStrip = _contextMenuStrip;
				}
			}
		}

		TextBox ITextBoxOwner.TextBoxControl => TextBox;

		bool ITextBoxOwner.UseDefaultMenu
		{
			get
			{
				return _useDefaultMenu;
			}
			set
			{
				if (_useDefaultMenu == value)
				{
					return;
				}
				_useDefaultMenu = value;
				if (TextBox != null)
				{
					if (_useDefaultMenu)
					{
						TextBox.ContextMenuStrip = null;
					}
					else
					{
						TextBox.ContextMenuStrip = new ContextMenuStrip();
					}
				}
			}
		}

		[Description("Indicates whether the edit control modifies the case of characters as they are typed.")]
		[DefaultValue(CharacterCasing.Normal)]
		public virtual CharacterCasing CharacterCasing
		{
			get
			{
				return _characterCasing;
			}
			set
			{
				_characterCasing = value;
				if (TextBox != null)
				{
					TextBox.CharacterCasing = value;
				}
			}
		}

		[Browsable(false)]
		public bool CanUndo
		{
			get
			{
				if (TextBox != null)
				{
					return TextBox.CanUndo;
				}
				return false;
			}
		}

		[Description("Indicates whether the selected text in the edit control remains highlighted when the control loses focus.")]
		[DefaultValue(true)]
		public bool HideSelection
		{
			get
			{
				return _hideSelection;
			}
			set
			{
				_hideSelection = value;
				if (TextBox != null)
				{
					TextBox.HideSelection = value;
				}
			}
		}

		[DefaultValue(32767)]
		[Description("Indicates the maximum number of characters the user can type into the edit control.")]
		public int MaxLength
		{
			get
			{
				return _maxLength;
			}
			set
			{
				_maxLength = value;
				if (TextBox != null)
				{
					TextBox.MaxLength = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Modified
		{
			get
			{
				if (TextBox != null)
				{
					return TextBox.Modified;
				}
				return _modified;
			}
			set
			{
				if (TextBox != null)
				{
					TextBox.Modified = value;
				}
				else
				{
					_modified = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedText
		{
			get
			{
				if (TextBox is MaskEdit)
				{
					return ((MaskEdit)TextBox).SelectedText;
				}
				if (TextBox != null)
				{
					return TextBox.SelectedText;
				}
				return "";
			}
			set
			{
				if (TextBox != null)
				{
					TextBox.SelectedText = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionLength
		{
			get
			{
				if (TextBox != null)
				{
					return TextBox.SelectionLength;
				}
				return 0;
			}
			set
			{
				if (TextBox != null)
				{
					TextBox.SelectionLength = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionStart
		{
			get
			{
				if (TextBox != null)
				{
					return TextBox.SelectionStart;
				}
				return 0;
			}
			set
			{
				if (TextBox != null)
				{
					TextBox.SelectionStart = value;
				}
			}
		}

		[Browsable(false)]
		public int TextLength
		{
			get
			{
				if (TextBox != null)
				{
					return TextBox.TextLength;
				}
				return Text.Length;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override bool AutoScroll => false;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Size AutoScrollMargin
		{
			get
			{
				return new Size(0, 0);
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Size AutoScrollMinSize
		{
			get
			{
				return new Size(0, 0);
			}
			set
			{
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Point AutoScrollPosition
		{
			get
			{
				return default(Point);
			}
			set
			{
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new DockPaddingEdges DockPadding => null;

		[Description("Indicates whether the button in the control can respond to user interaction.")]
		[DefaultValue(true)]
		public bool ButtonEnabled
		{
			get
			{
				return _buttonEnabled;
			}
			set
			{
				if (_buttonEnabled != value)
				{
					_buttonEnabled = value;
					Invalidate();
				}
			}
		}

		[Browsable(false)]
		public int PreferredHeight
		{
			get
			{
				MxColorSchemeAppearanceState mordexColorSchemeAppearanceState = _officeStyle.ResolveAppearanceState(GridStyleArea.EditControls);
				int textHeight = GetTextHeight();
				Image image = GetCurrentImage();
				int value = textHeight;
				int height = 0;
				if (image != null)
				{
					height = image.Height + 2;
					if (_imageVerticalAlignment == ImageVerticalAlignment.BottomOfText || _imageVerticalAlignment == ImageVerticalAlignment.TopOfText)
					{
						height += textHeight + 1;
					}
					if (height > value)
					{
						value = height;
					}
				}
				if (_buttonStyle == EditButtonStyle.Image && _buttonType == EditButtonType.SingleButton)
				{
					height = _buttonImageSize.Height;
					if (height == 0)
					{
						Image buttonImage = GetButtonImage();
						if (buttonImage != null)
						{
							height = buttonImage.Height;
						}
					}
					if (mordexColorSchemeAppearanceState.IsThemedLook)
					{
						height += 2;
					}
					else if (mordexColorSchemeAppearanceState._isThemed)
					{
						height += 4;
					}
					else
					{
						ButtonAppearance buttonAppearance = _officeStyle.ButtonAppearance;
						height = ((buttonAppearance != ButtonAppearance.Regular) ? (height + 2) : (height + 4));
					}
					if (height > value)
					{
						value = height;
					}
				}
				if (BorderStyle != BorderStyle.None || _hoverMode == HoverMode.Highlight)
				{
					return value + 4;
				}
				return value + 2;
			}
		}
	}
}
