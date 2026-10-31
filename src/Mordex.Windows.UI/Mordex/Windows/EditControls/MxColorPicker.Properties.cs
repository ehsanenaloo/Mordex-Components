using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxColorPicker
	{
		Control IFormatStyleOwner.Owner => this;

		[Description("Represents the size of the items in pixels")]
		[Localizable(true)]
		public Size ColorItemSize
		{
			get
			{
				return _colorItemSize;
			}
			set
			{
				if (_colorItemSize != value)
				{
					_colorItemSize = value;
					InvalidateLayout();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue("Automatic")]
		[Description("Represents the Text displayed in the Automatic button.")]
		public string AutomaticButtonText
		{
			get
			{
				return _automaticButtonText;
			}
			set
			{
				if (_automaticButtonText != value)
				{
					_automaticButtonText = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Indicates whether the morecolors button is shown.")]
		[DefaultValue(true)]
		[Localizable(true)]
		public bool ShowMoreColorsButton
		{
			get
			{
				return _showMoreColorsButton;
			}
			set
			{
				if (_showMoreColorsButton != value)
				{
					_showMoreColorsButton = value;
					InvalidateLayout();
				}
			}
		}

		[Description("Indicates whether the automatic button is shown.")]
		[DefaultValue(true)]
		[Localizable(true)]
		public bool ShowAutomaticButton
		{
			get
			{
				return _showAutomaticButton;
			}
			set
			{
				if (_showAutomaticButton != value)
				{
					_showAutomaticButton = value;
					if (!value)
					{
						_isAutomaticSelected = false;
					}
					InvalidateLayout();
				}
			}
		}

		[DefaultValue("More Colors...")]
		[Description("Represents the Text displayed in the MoreColors button.")]
		[Localizable(true)]
		public string MoreColorsButtonText
		{
			get
			{
				return _moreColorsButtonText;
			}
			set
			{
				if (_moreColorsButtonText != value)
				{
					_moreColorsButtonText = value;
					InvalidateLayout();
				}
			}
		}

		[Browsable(false)]
		public ColorEntryCollection ColorEntries => _colorEntries;

		[DefaultValue(DefaultColumns)]
		[Localizable(true)]
		[Description("Represents the number of columns displayed in the UIColorPicker control.")]
		public int Columns
		{
			get
			{
				return _columns;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException();
				}
				if (_columns != value)
				{
					_columns = value;
					InvalidateLayout();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Description("Represents the object that holds the format styles used to draw the items in its different states.")]
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

		[Description("Represents the Color that will be used as the automatic color")]
		[Localizable(true)]
		public Color AutomaticColor
		{
			get
			{
				return _automaticColor;
			}
			set
			{
				_automaticColor = value;
				if (value != SelectedColor && _isAutomaticSelected)
				{
					_selectedEntry = null;
					_selectedColor = value;
					OnSelectedColorChanged(EventArgs.Empty);
				}
				Invalidate();
			}
		}

		[Description("Represents the selected color")]
		public Color SelectedColor
		{
			get
			{
				return _selectedColor;
			}
			set
			{
				if (!RenderingHelper.AreColorsEqual(_selectedColor, value))
				{
					_selectedColor = value;
					if (value == _automaticColor && _showAutomaticButton)
					{
						_isAutomaticSelected = true;
					}
					else
					{
						_isAutomaticSelected = false;
						SetSelectedEntry(_colorEntries[_selectedColor]);
					}
					OnSelectedColorChanged(EventArgs.Empty);
				}
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

		[Description("Indicates whether a ToolTip is shown when the mouse passes over a color.")]
		[DefaultValue(true)]
		[Localizable(true)]
		public bool ShowToolTips
		{
			get
			{
				return _showToolTips;
			}
			set
			{
				_showToolTips = value;
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
				if (!_flatBorderColor.Equals(value))
				{
					_flatBorderColor = value;
					Invalidate();
				}
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
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		[DefaultValue(Mordex.Windows.UI.BorderStyle.Flat)]
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
				if (_borderStyle != value)
				{
					_borderStyle = value;
					UpdateBorderWidth();
					InvalidateLayout();
				}
			}
		}

		[Description("Represents the format style used to draw the Automatic button")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle AutomaticButtonFormatStyle
		{
			get
			{
				if (_automaticButtonFormatStyle == null)
				{
					_automaticButtonFormatStyle = new FormatStyle(this);
				}
				return _automaticButtonFormatStyle;
			}
		}

		[Description("Represents the format style used to draw the MoreColors button")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public FormatStyle MoreColorsButtonFormatStyle
		{
			get
			{
				if (_moreColorsButtonFormatStyle == null)
				{
					_moreColorsButtonFormatStyle = new FormatStyle(this);
				}
				return _moreColorsButtonFormatStyle;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new Size Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				base.Size = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeColorItemSize()
		{
			return _colorItemSize != DefaultColorItemSize;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetColorItemSize()
		{
			ColorItemSize = DefaultColorItemSize;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAutomaticColor()
		{
			AutomaticColor = Color.Black;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAutomaticColor()
		{
			return !_automaticColor.Equals(Color.Black);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeSelectedColor()
		{
			return !RenderingHelper.AreColorsEqual(_selectedColor, _automaticColor);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetSelectedColor()
		{
			SelectedColor = _automaticColor;
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
		public bool ShouldSerializeBackColor()
		{
			Color color = RenderingHelper.GetSchemeColor(ColorName.Control);
			return !BackColor.Equals(RenderingHelper.BlendColors(color, RenderingHelper.GetSchemeColor(ColorName.Window), BackColorWindowMix));
		}

		public new void ResetBackColor()
		{
			Color color = RenderingHelper.GetSchemeColor(ColorName.Control);
			BackColor = RenderingHelper.BlendColors(color, RenderingHelper.GetSchemeColor(ColorName.Window), BackColorWindowMix);
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
