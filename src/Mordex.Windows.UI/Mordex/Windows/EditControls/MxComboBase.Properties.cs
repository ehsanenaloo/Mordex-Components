using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxComboBase
	{
		TextBox ITextBoxOwner.TextBoxControl => _textBox;

		bool ITextBoxOwner.UseDefaultMenu
		{
			get
			{
				return _useDefaultMenu;
			}
			set
			{
				if (value == _useDefaultMenu)
				{
					return;
				}
				_useDefaultMenu = value;
				if (_textBox == null)
				{
					return;
				}
				if (_useDefaultMenu)
				{
					if (_textBox.ContextMenuStrip == GetContextMenu())
					{
						_textBox.ContextMenuStrip = null;
					}
				}
				else
				{
					_textBox.ContextMenuStrip = GetContextMenu();
				}
			}
		}

		Color IWindowsElement.FlatBorderColor => FlatBorderColor;

		Control IFormatStyleOwner.Owner => this;

		int IBaseLineControl.Offset => _baselineOffset;

		bool IRibbonEditControl.ActAsRibbonEditControl
		{
			get
			{
				return _actAsRibbonEditControl;
			}
			set
			{
				if (_actAsRibbonEditControl != value)
				{
					_actAsRibbonEditControl = value;
					if (_textBox != null)
					{
						_textBox.BackColor = GetEffectiveBackColor();
					}
				}
			}
		}

		[Localizable(true)]
		[Description("Represents the Font used by the drop down window")]
		public Font DropListFont
		{
			get
			{
				if (_dropListFont == null)
				{
					return Font;
				}
				return _dropListFont;
			}
			set
			{
				_dropListFont = value;
			}
		}

		[Description("Indicates whether the contents of the control can be changed.")]
		[DefaultValue(false)]
		public bool ReadOnly
		{
			get
			{
				return _readOnly;
			}
			set
			{
				_readOnly = value;
				if (_textBox != null)
				{
					_textBox.ReadOnly = _readOnly;
				}
				OnReadOnlyChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AutoScroll
		{
			get
			{
				return base.AutoScroll;
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override ImageLayout BackgroundImageLayout
		{
			get
			{
				return base.BackgroundImageLayout;
			}
			set
			{
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size AutoScrollMinSize
		{
			get
			{
				return base.AutoScrollMinSize;
			}
			set
			{
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Size AutoScrollMargin
		{
			get
			{
				return base.AutoScrollMargin;
			}
			set
			{
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Padding Padding
		{
			get
			{
				return base.Padding;
			}
			set
			{
				base.Padding = value;
			}
		}

		[Description("The color of the border of the control when it\u00b4s border is Flat.")]
		public Color FlatBorderColor
		{
			get
			{
				if (_flatBorderColor.IsEmpty)
				{
					return RenderingHelper.GetSchemeColor(ColorName.ControlDark);
				}
				return _flatBorderColor;
			}
			set
			{
				if (value != _flatBorderColor)
				{
					_flatBorderColor = value;
					Invalidate();
				}
			}
		}

		[Description("Represents the distance, in pixels, in an indent.")]
		[DefaultValue(DefaultIndentWidth)]
		public int IndentWidth
		{
			get
			{
				return _indentWidth;
			}
			set
			{
				if (_indentWidth != value)
				{
					_formatStyleRenderer.SetIndentWidth(value);
					_indentWidth = value;
				}
			}
		}

		[Description("Specifies whether the text rendering should be compatible with previous release of Windows Forms.")]
		public bool UseCompatibleTextRendering
		{
			get
			{
				if (!_useCompatibleTextRendering.HasValue)
				{
					return TextRenderingOptions.DefaultUseCompatibleTextRendering;
				}
				return _useCompatibleTextRendering.Value;
			}
			set
			{
				if (_useCompatibleTextRendering != value)
				{
					_useCompatibleTextRendering = value;
					InvalidateLayout();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[DefaultValue(null)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Obsolete("The value of this property has no effect")]
		public StringFormat StringFormat
		{
			get
			{
				if (_stringFormat == null)
				{
					_stringFormat = new StringFormat();
					switch (TextAlignment)
					{
					case TextAlignment.Center:
						_stringFormat.Alignment = StringAlignment.Center;
						break;
					case TextAlignment.Empty:
					case TextAlignment.Near:
						_stringFormat.Alignment = StringAlignment.Near;
						break;
					case TextAlignment.Far:
						_stringFormat.Alignment = StringAlignment.Far;
						break;
					}
					_stringFormat.HotkeyPrefix = HotkeyPrefix.None;
					_stringFormat.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
					_stringFormat.LineAlignment = StringAlignment.Center;
					if (RightToLeft == RightToLeft.Yes)
					{
						_stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
					}
				}
				return _stringFormat;
			}
			set
			{
				if (_stringFormat != value)
				{
					_stringFormat = value;
					InvalidateLayout();
				}
			}
		}

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Indicates whether the control's height is automatically resized to display the font.")]
		[DefaultValue(true)]
		[Localizable(true)]
		public new bool AutoSize
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
					InvalidateLayout();
				}
			}
		}

		public override Rectangle DisplayRectangle
		{
			get
			{
				if (UsesNativeTheme())
				{
					return base.ClientRectangle;
				}
				int inset = GetDisplayInset();
				return Rectangle.Inflate(base.ClientRectangle, -inset, -inset);
			}
		}

		public new Color BackColor
		{
			get
			{
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
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
			}
		}

		[DefaultValue(Mordex.Windows.UI.BorderStyle.SunkenLight3D)]
		[Description("The border style of the control.")]
		public Mordex.Windows.UI.BorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(Mordex.Windows.UI.BorderStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _borderStyle)
				{
					_borderStyle = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Specifies the effect of the control when the mouse hovers over it")]
		[DefaultValue(HoverMode.None)]
		public HoverMode HoverMode
		{
			get
			{
				return _hoverMode;
			}
			set
			{
				if (_hoverMode != value)
				{
					_hoverMode = value;
					InvalidateLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the format styles used to draw the buttons of the combo in their different states.")]
		public StateStyles StateStyles
		{
			get
			{
				if (_stateStyles == null)
				{
					_stateStyles = new StateStyles(this);
				}
				return _stateStyles;
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
				if (base.DesignMode)
				{
					DesignHelper.EnsureDesignAssemblyLoaded();
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(false)]
		public bool DroppedDown
		{
			get
			{
				return _droppedDown;
			}
			set
			{
				if ((!value || _editStyle != EditStyle.Text) && value != _droppedDown)
				{
					_droppedDown = value;
					if (value)
					{
						DropListControl(setCapture: true);
					}
					else
					{
						OnDropHide(cancel: true);
					}
				}
			}
		}

		IntPtr IDropListOwner.DropWindowHandle => _formatStyleRenderer.Handle;

		bool IDropListOwner.UseCompatibleTextRendering => UseCompatibleTextRendering;

		[DefaultValue(ComboStyle.Combo)]
		[Localizable(false)]
		[Description("Controls the appearance and functionality of the combo box")]
		public ComboStyle ComboStyle
		{
			get
			{
				return _comboStyle;
			}
			set
			{
				if (!Enum.IsDefined(typeof(ComboStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _comboStyle)
				{
					_comboStyle = value;
					if (_comboStyle == ComboStyle.Combo)
					{
						UpdateEditStyle(EditStyle.Combo);
					}
					else
					{
						UpdateEditStyle(EditStyle.DropDownList);
					}
				}
			}
		}

		public override bool Focused => base.ContainsFocus;

		[DefaultValue(ImageHorizontalAlignment.BeforeText)]
		[Description("Determines the horizontal alignment of the image.")]
		[Localizable(false)]
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
					InvalidateLayout();
				}
			}
		}

		[Localizable(false)]
		[Description("Determines the vertical alignment of the image.")]
		[DefaultValue(ImageVerticalAlignment.Center)]
		public ImageVerticalAlignment ImageVerticalAlignment
		{
			get
			{
				return _imageVerticalAlignment;
			}
			set
			{
				if (_imageVerticalAlignment != value)
				{
					_imageVerticalAlignment = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(false)]
		[Description("Determines how the the text is aligned")]
		[DefaultValue(TextAlignment.Near)]
		public TextAlignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TextAlignment), value))
				{
					throw new ArgumentException();
				}
				if (value != _textAlignment)
				{
					_textAlignment = value;
					ApplyTextAlignmentToTextBox();
					if (_stringFormat != null)
					{
						_stringFormat.Dispose();
						_stringFormat = null;
					}
					InvalidateLayout();
				}
			}
		}

		[Description("Specifies whether the control modifies the case of characters as they are typed.")]
		[DefaultValue(CharacterCasing.Normal)]
		[Localizable(true)]
		public CharacterCasing CharacterCasing
		{
			get
			{
				return _characterCasing;
			}
			set
			{
				if (_characterCasing != value)
				{
					_characterCasing = value;
					if (_textBox != null)
					{
						_textBox.CharacterCasing = value;
					}
				}
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
				if (value == _allowDrop)
				{
					return;
				}
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

		[Browsable(false)]
		public TextBox TextBox
		{
			get
			{
				if (_isLayoutDirty && base.IsHandleCreated && !base.IsDisposed && !base.Disposing)
				{
					using (Graphics graphics = CreateGraphics())
					{
						LayoutContent(graphics);
					}
				}
				return _textBox;
			}
		}

		[Localizable(true)]
		[Bindable(BindableSupport.Yes)]
		[DefaultValue("")]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				if (!(value != Text))
				{
					return;
				}
				base.Text = value;
				LayoutContent(null);
				if (_textBox != null && _textBox.Text != value)
				{
					_textBox.Text = value;
					try
					{
						if (value != null)
						{
							SelectRange(0, 0);
						}
						return;
					}
					catch (SecurityException)
					{
						_textBox.SelectAll();
						return;
					}
				}
				Invalidate();
			}
		}

		[Description("Indicates whether the control should display an image.")]
		[Localizable(true)]
		[DefaultValue(true)]
		public bool HasImage
		{
			get
			{
				return _hasImage;
			}
			set
			{
				if (value != _hasImage)
				{
					_hasImage = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[Description("Indicates whether the control should display text.")]
		[DefaultValue(true)]
		public bool HasText
		{
			get
			{
				return _hasText;
			}
			set
			{
				if (value != _hasText)
				{
					_hasText = value;
					UpdateTextBoxForEditStyle();
					InvalidateLayout();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The ImageList that contains the images used in the control.")]
		[Localizable(true)]
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
					if (_formatStyleRenderer != null)
					{
						_formatStyleRenderer.SetImageList(value);
					}
					InvalidateLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style used to draw the items")]
		public FormatStyle ItemsFormatStyle
		{
			get
			{
				if (_itemsFormatStyle == null)
				{
					_itemsFormatStyle = new FormatStyle(this);
				}
				return _itemsFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the format style used to draw the selected item")]
		public FormatStyle SelectedItemFormatStyle
		{
			get
			{
				if (_selectedItemFormatStyle == null)
				{
					_selectedItemFormatStyle = new FormatStyle(this);
				}
				return _selectedItemFormatStyle;
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		[Description("Indicates whether literals should be included when updating a masked value.")]
		public bool IncludeLiterals
		{
			get
			{
				return _includeLiterals;
			}
			set
			{
				if (_includeLiterals != value)
				{
					_includeLiterals = value;
				}
			}
		}

		[Description("Indicates the maximum number of entries to display in the dropdown list.")]
		[DefaultValue(DefaultMaxDropDownItems)]
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

		[Browsable(false)]
		public Size DropDownWindowSize
		{
			get
			{
				if (_formatStyleRenderer != null)
				{
					return _formatStyleRenderer.Size;
				}
				return Size.Empty;
			}
		}

		[Description("Indicates the maximum of the dropdown list.")]
		[DefaultValue(-1)]
		public int MaxDropDownWidth
		{
			get
			{
				return _maxDropDownWidth;
			}
			set
			{
				_maxDropDownWidth = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object used to draw buttons and scroll bars in the control.")]
		public ControlAppearance ControlAppearance
		{
			get
			{
				return _controlAppearance;
			}
			set
			{
				if (_controlAppearance != value)
				{
					_controlAppearance = value;
					OnControlStyleChanged();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeDropListFont()
		{
			return _dropListFont != null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetDropListFont()
		{
			_dropListFont = null;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFlatBorderColor()
		{
			FlatBorderColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFlatBorderColor()
		{
			return !_flatBorderColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseCompatibleTextRendering()
		{
			if (_useCompatibleTextRendering.HasValue)
			{
				_useCompatibleTextRendering = null;
				InvalidateLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseCompatibleTextRendering()
		{
			return _useCompatibleTextRendering.HasValue;
		}
	}
}
