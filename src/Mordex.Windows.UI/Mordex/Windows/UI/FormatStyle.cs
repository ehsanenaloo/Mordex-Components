using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class FormatStyle : IMordexXmlLayoutsSupport
	{
		private const int NoAlpha = -1;

		private const float NoBlend = -1f;

		private const int ChannelMax = 255;

		private const float SelectedHotBlend = 0.5f;

		private const float HoverControlBlend = 0.8f;

		private const float HoverWindowBlend = 0.53f;

		private const float PressedControlBlend = 0.1f;

		private const float PressedWindowBlend = 0.33f;

		private const float BrightnessLimit = 0.85f;

		private const float BrightnessDrop = 0.45f;

		internal IFormatStyleOwner _owner;

		private bool _isDefault = true;

		private float _fontSizeScale = 1f;

		private int _alpha;

		private Color _backColor;

		private Color _backColorGradient;

		private BackgroundGradientMode _backgroundGradientMode;

		private AlphaMode _backColorAlphaMode;

		private Color _foreColor;

		private AlphaMode _foreColorAlphaMode;

		private bool _isFontExplicit;

		// The font assigned by the user or copied from another style. _resolvedFont is either this font or a
		// font built from it (or from a control font) by this style; a built font is always owned here.
		private Font _font;

		// True when _font is a clone made by this style, so the style must dispose it.
		private bool _ownsFont;

		private Font _resolvedFont;

		private Image _backgroundImage;

		private BackgroundImageDrawMode _backgroundImageDrawMode;

		private TriState _fontBold;

		private TriState _fontItalic;

		private TriState _fontUnderline;

		private TriState _fontStrikeout;

		private string _fontName = "";

		private float _fontSize;

		private Brush _backgroundBrush;

		private Brush _foreBrush;

		private float _blend = NoBlend;

		private float _blendGradient = NoBlend;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(NoAlpha)]
		[Description("Represents the alpha component that will be applied to the colors in the FormatStyle.")]
		public int Alpha
		{
			get
			{
				return _alpha;
			}
			set
			{
				if (value < NoAlpha || value > ChannelMax)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _alpha)
				{
					_alpha = value;
					UpdateDefaultFlag(value == NoAlpha);
				}
			}
		}

		[Description("Represents the background color of the UIFormatStyle.")]
		public Color BackColor
		{
			get
			{
				return _backColor;
			}
			set
			{
				if (value != _backColor)
				{
					_backColor = value;
					UpdateDefaultFlag(value.IsEmpty);
				}
			}
		}

		[Description("Represents the background color used in a linear gradient pattern.")]
		public Color BackColorGradient
		{
			get
			{
				return _backColorGradient;
			}
			set
			{
				if (value != _backColorGradient)
				{
					_backColorGradient = value;
					UpdateDefaultFlag(value.IsEmpty);
				}
			}
		}

		[Description("Specifies if a linear gradient will be applied and its direction.")]
		public BackgroundGradientMode BackgroundGradientMode
		{
			get
			{
				return _backgroundGradientMode;
			}
			set
			{
				if (_backgroundGradientMode != value)
				{
					_backgroundGradientMode = value;
					UpdateDefaultFlag(value == BackgroundGradientMode.Empty);
				}
			}
		}

		[DefaultValue(AlphaMode.Empty)]
		[Description("Determines how alpha should be combined with the background color.")]
		public AlphaMode BackColorAlphaMode
		{
			get
			{
				return _backColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backColorAlphaMode)
				{
					_backColorAlphaMode = value;
					UpdateDefaultFlag(value == AlphaMode.Empty);
				}
			}
		}

		[Description("Specifies the foreground color of the UIFormatStyle.")]
		public Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				if (value != _foreColor)
				{
					_foreColor = value;
					UpdateDefaultFlag(_foreColor.IsEmpty);
				}
			}
		}

		[DefaultValue(AlphaMode.Empty)]
		[Description("Determines how Alpha should be combined with the foreground color.")]
		public AlphaMode ForeColorAlphaMode
		{
			get
			{
				return _foreColorAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _foreColorAlphaMode)
				{
					_foreColorAlphaMode = value;
					UpdateDefaultFlag(_foreColorAlphaMode == AlphaMode.Empty);
				}
			}
		}

		[Description("Represent the font used to draw a text.")]
		[DefaultValue(null)]
		public Font Font
		{
			get
			{
				return _resolvedFont;
			}
			set
			{
				if (value != _resolvedFont)
				{
					ReleaseFonts();
					_font = value;
					_resolvedFont = value;
					_isFontExplicit = value != null;
					UpdateDefaultFlag(!_isFontExplicit);
				}
			}
		}

		[Editor(typeof(ImageEditor), typeof(UITypeEditor))]
		[Description("Represent the background image")]
		[DefaultValue(null)]
		public Image BackgroundImage
		{
			get
			{
				return _backgroundImage;
			}
			set
			{
				if (_backgroundImage != value)
				{
					_backgroundImage = value;
					UpdateDefaultFlag(value == null);
				}
			}
		}

		[Description("Determines how the background image is drawn.")]
		[DefaultValue(BackgroundImageDrawMode.Empty)]
		public BackgroundImageDrawMode BackgroundImageDrawMode
		{
			get
			{
				return _backgroundImageDrawMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(BackgroundImageDrawMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backgroundImageDrawMode)
				{
					_backgroundImageDrawMode = value;
					UpdateDefaultFlag(value == BackgroundImageDrawMode.Empty);
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicate whether the Font object used is bold.")]
		public TriState FontBold
		{
			get
			{
				return _fontBold;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontBold)
				{
					_fontBold = value;
					if (_resolvedFont != null)
					{
						RebuildResolvedFont();
					}
					UpdateDefaultFlag(value == TriState.Empty);
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object used is italic.")]
		public TriState FontItalic
		{
			get
			{
				return _fontItalic;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontItalic)
				{
					_fontItalic = value;
					if (_resolvedFont != null)
					{
						RebuildResolvedFont();
					}
					UpdateDefaultFlag(value == TriState.Empty);
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object used is underline")]
		public TriState FontUnderline
		{
			get
			{
				return _fontUnderline;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontUnderline)
				{
					_fontUnderline = value;
					if (_resolvedFont != null)
					{
						RebuildResolvedFont();
					}
					UpdateDefaultFlag(_fontUnderline == TriState.Empty);
				}
			}
		}

		[Description("Indicates whether the Font object used has a horizontal line through the font.")]
		[DefaultValue(TriState.Empty)]
		public TriState FontStrikeout
		{
			get
			{
				return _fontStrikeout;
			}
			set
			{
				if (!Enum.IsDefined(typeof(TriState), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _fontStrikeout)
				{
					_fontStrikeout = value;
					if (_resolvedFont != null)
					{
						RebuildResolvedFont();
					}
					UpdateDefaultFlag(value == TriState.Empty);
				}
			}
		}

		[DefaultValue("")]
		public string FontName
		{
			get
			{
				return _fontName;
			}
			set
			{
				value = value ?? "";
				if (value != _fontName)
				{
					_fontName = value;
					if (_resolvedFont != null)
					{
						RebuildResolvedFont();
					}
					UpdateDefaultFlag(_fontName.Length == 0);
				}
			}
		}

		[DefaultValue(0f)]
		public float FontSize
		{
			get
			{
				return _fontSize;
			}
			set
			{
				if (value < 0f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _fontSize)
				{
					_fontSize = value;
					if (_resolvedFont != null)
					{
						RebuildResolvedFont();
					}
					UpdateDefaultFlag(value == 0f);
				}
			}
		}

		[DefaultValue(NoBlend)]
		[Description("Indicates the percentage of the Window color to blend with the BackColor.")]
		public float Blend
		{
			get
			{
				return _blend;
			}
			set
			{
				if ((value < 0f || value > 1f) && value != NoBlend)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _blend)
				{
					_blend = value;
					UpdateDefaultFlag(_blend == NoBlend);
				}
			}
		}

		[DefaultValue(NoBlend)]
		[Description("Indicates the percentage of the Window color to blend with the BackColorGradient.")]
		public float BlendGradient
		{
			get
			{
				return _blendGradient;
			}
			set
			{
				if ((value < 0f || value > 1f) && value != NoBlend)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _blendGradient)
				{
					_blendGradient = value;
					UpdateDefaultFlag(value == NoBlend);
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), NoAlpha);
			_backColor = (Color)LayoutReader.GetPropertyValue(info, "BackColor", typeof(Color), Color.Empty);
			_backColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "BackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_backgroundImage = (Image)LayoutReader.GetPropertyValue(info, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (BackgroundImageDrawMode)LayoutReader.GetPropertyValue(info, "BackgroundImageDrawMode", typeof(BackgroundImageDrawMode), BackgroundImageDrawMode.Empty);
			ReleaseFonts();
			_font = (Font)LayoutReader.GetPropertyValue(info, "Font", typeof(Font), null);
			_resolvedFont = _font;
			_isFontExplicit = _font != null;
			_fontBold = (TriState)LayoutReader.GetPropertyValue(info, "FontBold", typeof(TriState), TriState.Empty);
			_fontItalic = (TriState)LayoutReader.GetPropertyValue(info, "FontItalic", typeof(TriState), TriState.Empty);
			_fontName = (string)LayoutReader.GetPropertyValue(info, "FontName", typeof(string), string.Empty);
			_fontSize = (float)LayoutReader.GetPropertyValue(info, "FontSize", typeof(float), 0f);
			_fontStrikeout = (TriState)LayoutReader.GetPropertyValue(info, "FontStrikeout", typeof(TriState), TriState.Empty);
			_fontUnderline = (TriState)LayoutReader.GetPropertyValue(info, "FontUnderline", typeof(TriState), TriState.Empty);
			_foreColor = (Color)LayoutReader.GetPropertyValue(info, "ForeColor", typeof(Color), Color.Empty);
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "ForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_blend = (float)LayoutReader.GetPropertyValue(info, "Blend", typeof(float), NoBlend);
			_blendGradient = (float)LayoutReader.GetPropertyValue(info, "BlendGradient", typeof(float), NoBlend);
			_backColorGradient = (Color)LayoutReader.GetPropertyValue(info, "BackColorGradient", typeof(Color), Color.Empty);
			_backgroundGradientMode = (BackgroundGradientMode)LayoutReader.GetPropertyValue(info, "BackgroundGradientMode", typeof(BackgroundGradientMode), BackgroundGradientMode.Empty);
			_isDefault = AreAllPropertiesDefault();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_alpha != NoAlpha)
			{
				info.AddValue("Alpha", _alpha);
			}
			if (_backColor != Color.Empty)
			{
				info.AddValue("BackColor", _backColor);
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("BackColorAlphaMode", _backColorAlphaMode);
			}
			if (_backgroundImage != null)
			{
				info.AddValue("BackgroundImage", _backgroundImage);
			}
			if (_backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
			{
				info.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (_font != null)
			{
				info.AddValue("Font", _font);
			}
			if (_fontBold != TriState.Empty)
			{
				info.AddValue("FontBold", _fontBold);
			}
			if (_fontItalic != TriState.Empty)
			{
				info.AddValue("FontItalic", _fontItalic);
			}
			if (_fontName != string.Empty)
			{
				info.AddValue("FontName", _fontName);
			}
			if (_fontSize != 0f)
			{
				info.AddValue("FontSize", _fontSize);
			}
			if (_fontStrikeout != TriState.Empty)
			{
				info.AddValue("FontStrikeout", _fontStrikeout);
			}
			if (_fontUnderline != TriState.Empty)
			{
				info.AddValue("FontUnderline", _fontUnderline);
			}
			if (_foreColor != Color.Empty)
			{
				info.AddValue("ForeColor", _foreColor);
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("ForeColorAlphaMode", _foreColorAlphaMode);
			}
			if (_blend != NoBlend)
			{
				info.AddValue("Blend", _blend);
			}
			if (_blendGradient != NoBlend)
			{
				info.AddValue("BlendGradient", _blendGradient);
			}
			if (_backColorGradient != Color.Empty)
			{
				info.AddValue("BackColorGradient", _backColorGradient);
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				info.AddValue("BackgroundGradientMode", _backgroundGradientMode);
			}
		}

		public FormatStyle()
		{
		}

		internal FormatStyle(IFormatStyleOwner owner)
		{
			_owner = owner;
			Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_alpha = NoAlpha;
			_backColor = Color.Empty;
			_backColorAlphaMode = AlphaMode.Empty;
			_backgroundImage = null;
			_backgroundImageDrawMode = BackgroundImageDrawMode.Empty;
			ReleaseFonts();
			_isFontExplicit = false;
			_fontBold = TriState.Empty;
			_fontItalic = TriState.Empty;
			_fontName = "";
			_fontSize = 0f;
			_fontStrikeout = TriState.Empty;
			_fontUnderline = TriState.Empty;
			_foreColor = Color.Empty;
			_foreColorAlphaMode = AlphaMode.Empty;
			_fontSizeScale = 1f;
			_blend = NoBlend;
			_blendGradient = NoBlend;
			_backColorGradient = Color.Empty;
			_backgroundGradientMode = BackgroundGradientMode.Empty;
			_isDefault = true;
		}

		internal bool IsDefaultStyle()
		{
			return _isDefault;
		}

		internal bool AreAllPropertiesDefault()
		{
			if (_alpha != NoAlpha)
			{
				return false;
			}
			if (!BackColor.IsEmpty)
			{
				return false;
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_backgroundImage != null)
			{
				return false;
			}
			if (_backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
			{
				return false;
			}
			if (_font != null)
			{
				return false;
			}
			if (FontBold != TriState.Empty)
			{
				return false;
			}
			if (_fontItalic != TriState.Empty)
			{
				return false;
			}
			if (_fontName != "")
			{
				return false;
			}
			if (_fontSize != 0f)
			{
				return false;
			}
			if (_fontStrikeout != TriState.Empty)
			{
				return false;
			}
			if (_fontUnderline != TriState.Empty)
			{
				return false;
			}
			if (!ForeColor.IsEmpty)
			{
				return false;
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (Blend != NoBlend)
			{
				return false;
			}
			if (BlendGradient != NoBlend)
			{
				return false;
			}
			if (!BackColorGradient.IsEmpty)
			{
				return false;
			}
			if (BackgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return false;
			}
			return true;
		}

		internal FormatStyle(FormatStyle formatStyle)
		{
			_alpha = formatStyle.Alpha;
			_backColor = formatStyle.BackColor;
			_backColorAlphaMode = formatStyle.BackColorAlphaMode;
			_backgroundImage = formatStyle._backgroundImage;
			_backgroundImageDrawMode = formatStyle._backgroundImageDrawMode;
			if (formatStyle._resolvedFont != null)
			{
				AdoptFont(formatStyle);
				_fontBold = TriState.Empty;
				_fontItalic = TriState.Empty;
				_fontSize = 0f;
				_fontName = "";
				_fontStrikeout = TriState.Empty;
				_fontUnderline = TriState.Empty;
			}
			else
			{
				_fontBold = formatStyle.FontBold;
				_fontItalic = formatStyle._fontItalic;
				_fontName = formatStyle._fontName;
				_fontSize = formatStyle._fontSize;
				_fontStrikeout = formatStyle._fontStrikeout;
				_fontUnderline = formatStyle._fontUnderline;
			}
			_foreColor = formatStyle.ForeColor;
			_foreColorAlphaMode = formatStyle._foreColorAlphaMode;
			_fontSizeScale = formatStyle._fontSizeScale;
			_blend = formatStyle.Blend;
			_blendGradient = formatStyle.BlendGradient;
			_backColorGradient = formatStyle.BackColorGradient;
			_backgroundGradientMode = formatStyle.BackgroundGradientMode;
			_isDefault = formatStyle._isDefault;
		}

		internal FormatStyle MergeStyle(FormatStyle overrideStyle, bool skipBackground)
		{
			if (overrideStyle == null || overrideStyle.IsDefaultStyle())
			{
				return this;
			}
			FormatStyle uiFormatStyle = new FormatStyle(this);
			if (overrideStyle._fontSizeScale != 1f)
			{
				uiFormatStyle._fontSizeScale = overrideStyle._fontSizeScale;
			}
			if (!skipBackground)
			{
				if (overrideStyle.Alpha != NoAlpha)
				{
					uiFormatStyle._alpha = overrideStyle.Alpha;
				}
				if (!overrideStyle.BackColor.IsEmpty)
				{
					uiFormatStyle._backColor = overrideStyle.BackColor;
				}
				if (overrideStyle.BackColorAlphaMode != AlphaMode.Empty)
				{
					uiFormatStyle._backColorAlphaMode = overrideStyle.BackColorAlphaMode;
				}
				if (overrideStyle._backgroundImage != null)
				{
					uiFormatStyle._backgroundImage = overrideStyle._backgroundImage;
				}
				if (overrideStyle._backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
				{
					uiFormatStyle._backgroundImageDrawMode = overrideStyle._backgroundImageDrawMode;
				}
				if (overrideStyle.Blend != NoBlend)
				{
					uiFormatStyle._blend = overrideStyle.Blend;
				}
				if (overrideStyle.BlendGradient != NoBlend)
				{
					uiFormatStyle._blendGradient = overrideStyle.BlendGradient;
				}
				if (overrideStyle.BackColorGradient != Color.Empty)
				{
					uiFormatStyle._backColorGradient = overrideStyle.BackColorGradient;
				}
				if (overrideStyle.BackgroundGradientMode != BackgroundGradientMode.Empty)
				{
					uiFormatStyle._backgroundGradientMode = overrideStyle.BackgroundGradientMode;
				}
			}
			if (!overrideStyle.ForeColor.IsEmpty)
			{
				uiFormatStyle._foreColor = overrideStyle.ForeColor;
			}
			if (overrideStyle._foreColorAlphaMode != AlphaMode.Empty)
			{
				uiFormatStyle._foreColorAlphaMode = overrideStyle._foreColorAlphaMode;
			}
			if (overrideStyle._resolvedFont != null)
			{
				uiFormatStyle.AdoptFont(overrideStyle);
			}
			else
			{
				if (overrideStyle.FontBold != TriState.Empty)
				{
					uiFormatStyle._fontBold = overrideStyle.FontBold;
				}
				if (overrideStyle._fontItalic != TriState.Empty)
				{
					uiFormatStyle._fontItalic = overrideStyle._fontItalic;
				}
				if (overrideStyle._fontName != "")
				{
					uiFormatStyle._fontName = overrideStyle._fontName;
				}
				if (overrideStyle._fontSize != 0f)
				{
					uiFormatStyle._fontSize = overrideStyle._fontSize;
				}
				if (overrideStyle._fontStrikeout != TriState.Empty)
				{
					uiFormatStyle._fontStrikeout = overrideStyle._fontStrikeout;
				}
				if (overrideStyle._fontUnderline != TriState.Empty)
				{
					uiFormatStyle._fontUnderline = overrideStyle._fontUnderline;
				}
				if (uiFormatStyle._resolvedFont != null)
				{
					uiFormatStyle.RebuildResolvedFont();
				}
			}
			if (!overrideStyle._isDefault)
			{
				uiFormatStyle._isDefault = false;
			}
			return uiFormatStyle;
		}

		internal FormatStyle MergeStyle(FormatStyle uiFormatStyle)
		{
			return MergeStyle(uiFormatStyle, false);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAlpha()
		{
			Alpha = NoAlpha;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAlpha()
		{
			return _alpha != NoAlpha;
		}

		internal Color GetBlendedBackColor()
		{
			if (GetEffectiveBlend() != 0f && !BackColor.IsEmpty)
			{
				return BlendColors(GetWindowColor(), BackColor, GetEffectiveBlend());
			}
			if (!BackColor.IsEmpty)
			{
				return BackColor;
			}
			return Color.Empty;
		}

		internal Color GetBlendedGradientColor()
		{
			if (GetEffectiveBlendGradient() != 0f && !BackColorGradient.IsEmpty)
			{
				return BlendColors(GetWindowColor(), BackColorGradient, GetEffectiveBlendGradient());
			}
			if (!BackColorGradient.IsEmpty)
			{
				return BackColorGradient;
			}
			return SystemColors.Window;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorGradient()
		{
			BackColorGradient = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorGradient()
		{
			return !_backColorGradient.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackgroundGradientMode()
		{
			return _backgroundGradientMode != BackgroundGradientMode.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackgroundGradientMode()
		{
			BackgroundGradientMode = BackgroundGradientMode.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColor()
		{
			BackColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !_backColor.IsEmpty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackColorAlphaMode()
		{
			BackColorAlphaMode = AlphaMode.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColorAlphaMode()
		{
			return _backColorAlphaMode != AlphaMode.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetForeColor()
		{
			ForeColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColor()
		{
			return !_foreColor.IsEmpty;
		}

		internal Font ResolveFont(Font font)
		{
			if (_resolvedFont != null)
			{
				if (_resolvedFont == _font && HasFontOverrides())
				{
					RebuildResolvedFont();
				}
				return _resolvedFont;
			}
			if (!HasFontOverrides())
			{
				return font;
			}
			BuildResolvedFont(font);
			return _resolvedFont;
		}

		private bool HasFontOverrides()
		{
			if (_fontBold == TriState.Empty && _fontItalic == TriState.Empty && _fontName.Length <= 0 && _fontSize == 0f && _fontStrikeout == TriState.Empty && _fontUnderline == TriState.Empty)
			{
				return _fontSizeScale != 1f;
			}
			return true;
		}

		private void RebuildResolvedFont()
		{
			BuildResolvedFont(_font ?? _resolvedFont);
		}

		private void BuildResolvedFont(Font baseFont)
		{
			if (baseFont != null)
			{
				string name = baseFont.Name;
				float fontSize = baseFont.SizeInPoints;
				FontStyle fontStyle = FontStyle.Regular;
				if (_fontName != null && _fontName.Length > 0)
				{
					name = _fontName;
				}
				if (_fontSize > 0f)
				{
					fontSize = _fontSize;
				}
				bool isBold = ((FontBold == TriState.Empty) ? baseFont.Bold : (FontBold != TriState.False));
				bool isItalic = ((_fontItalic == TriState.Empty) ? baseFont.Italic : (_fontItalic != TriState.False));
				bool isStrikeout = ((_fontStrikeout == TriState.Empty) ? baseFont.Strikeout : (_fontStrikeout != TriState.False));
				bool isUnderline = ((_fontUnderline == TriState.Empty) ? baseFont.Underline : (_fontUnderline != TriState.False));
				if (isBold)
				{
					fontStyle |= FontStyle.Bold;
				}
				if (isItalic)
				{
					fontStyle |= FontStyle.Italic;
				}
				if (isStrikeout)
				{
					fontStyle |= FontStyle.Strikeout;
				}
				if (isUnderline)
				{
					fontStyle |= FontStyle.Underline;
				}
				if (_fontSizeScale != 1f)
				{
					fontSize *= _fontSizeScale;
				}
				Font builtFont = new Font(name, fontSize, fontStyle);
				ReleaseBuiltFont();
				_resolvedFont = builtFont;
			}
		}

		private void ReleaseBuiltFont()
		{
			if (_resolvedFont != null && _resolvedFont != _font)
			{
				_resolvedFont.Dispose();
			}
			_resolvedFont = _font;
		}

		private void ReleaseFonts()
		{
			ReleaseBuiltFont();
			if (_ownsFont)
			{
				_font.Dispose();
			}
			_font = null;
			_resolvedFont = null;
			_ownsFont = false;
		}

		// A user font is shared; a font the source built or owns is cloned because the source may dispose it.
		private void AdoptFont(FormatStyle source)
		{
			ReleaseFonts();
			if (source._resolvedFont == source._font && !source._ownsFont)
			{
				_font = source._font;
			}
			else
			{
				_font = (Font)source._resolvedFont.Clone();
				_ownsFont = true;
			}
			_resolvedFont = _font;
			_isFontExplicit = true;
		}

		private void UpdateDefaultFlag(bool isDefaultValue)
		{
			if (!isDefaultValue)
			{
				_isDefault = false;
			}
			else if (!_isDefault)
			{
				_isDefault = AreAllPropertiesDefault();
			}
			if (_owner != null)
			{
				_owner.PropertyChange();
			}
		}

		internal void DisposeResources()
		{
			if (_backgroundBrush != null)
			{
				_backgroundBrush.Dispose();
				_backgroundBrush = null;
			}
			if (_foreBrush != null)
			{
				_foreBrush.Dispose();
				_foreBrush = null;
			}
			if (_ownsFont)
			{
				ReleaseFonts();
				_isFontExplicit = false;
			}
			else
			{
				ReleaseBuiltFont();
			}
		}

		internal bool HasGradient()
		{
			if (BackgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return BackgroundGradientMode != BackgroundGradientMode.Solid;
			}
			return false;
		}

		internal Brush GetBackgroundBrush(Rectangle rectangle)
		{
			if (_backgroundBrush == null || HasGradient())
			{
				if (_backgroundBrush != null)
				{
					_backgroundBrush.Dispose();
					_backgroundBrush = null;
				}
				if (HasGradient())
				{
					rectangle.X--;
					rectangle.Width++;
					rectangle.Y--;
					rectangle.Height++;
					if (rectangle.Width > 0 && rectangle.Height > 0)
					{
						_backgroundBrush = RenderingHelper.CreateGradientBrush(BlendColors(GetWindowColor(), BackColor, GetEffectiveBlend()), BlendColors(GetWindowColor(), BackColorGradient, GetEffectiveBlendGradient()), rectangle, BackgroundGradientMode, Alpha, BackColorAlphaMode);
					}
				}
				else if (GetEffectiveBlend() != 0f && !BackColor.IsEmpty)
				{
					_backgroundBrush = RenderingHelper.CreateAlphaBrush(BlendColors(GetWindowColor(), BackColor, GetEffectiveBlend()), Alpha, BackColorAlphaMode);
				}
				else
				{
					_backgroundBrush = RenderingHelper.CreateAlphaBrush(BackColor, Alpha, BackColorAlphaMode);
				}
			}
			return _backgroundBrush;
		}

		internal Color BlendColors(Color color, Color otherColor, float colorWeight)
		{
			if (color == Color.Empty)
			{
				return otherColor;
			}
			if (otherColor == Color.Empty)
			{
				return color;
			}
			float redMix = (float)(int)color.R * colorWeight + (float)(int)otherColor.R * (1f - colorWeight);
			int red = (int)Math.Round(redMix, 0);
			float greenMix = (float)(int)color.G * colorWeight + (float)(int)otherColor.G * (1f - colorWeight);
			int green = (int)Math.Round(greenMix, 0);
			float blueMix = (float)(int)color.B * colorWeight + (float)(int)otherColor.B * (1f - colorWeight);
			int blue = (int)Math.Round(blueMix, 0);
			return Color.FromArgb(Math.Min(red, ChannelMax), Math.Min(green, ChannelMax), Math.Min(blue, ChannelMax));
		}

		internal Color GetEffectiveForeColor()
		{
			if (ForeColorAlphaMode == AlphaMode.Transparent)
			{
				return Color.Empty;
			}
			if (ForeColorAlphaMode == AlphaMode.UseAlpha)
			{
				if (_alpha == NoAlpha)
				{
					_alpha = ChannelMax;
				}
				return Color.FromArgb(_alpha, ForeColor);
			}
			return ForeColor;
		}

		internal Brush GetForeBrush()
		{
			if (_foreBrush == null)
			{
				_foreBrush = RenderingHelper.CreateAlphaBrush(ForeColor, _alpha, _foreColorAlphaMode);
			}
			return _foreBrush;
		}

		internal Color GetStateBorderColor(bool boolean, bool booleanArgument)
		{
			Color controlColor = RenderingHelper.GetSchemeColor(ColorName.Control);
			Color windowColor = RenderingHelper.GetSchemeColor(ColorName.Window);
			Color highlightColor = RenderingHelper.GetSchemeColor(ColorName.Highlight);
			if (boolean && booleanArgument)
			{
				Color mixedColor = RenderingHelper.BlendColors(highlightColor, windowColor, SelectedHotBlend);
				if (RenderingHelper.AreColorsEqual(mixedColor, BackColor))
				{
					return highlightColor;
				}
				return BackColor;
			}
			if (RenderingHelper.AreColorsEqual(highlightColor, BackColor))
			{
				return BackColor;
			}
			if (booleanArgument)
			{
				Color mixedColor = RenderingHelper.BlendColors(controlColor, highlightColor, HoverControlBlend);
				mixedColor = RenderingHelper.BlendColors(mixedColor, windowColor, HoverWindowBlend);
				if (RenderingHelper.AreColorsEqual(mixedColor, BackColor))
				{
					return highlightColor;
				}
			}
			else if (boolean)
			{
				Color mixedColor = RenderingHelper.BlendColors(controlColor, highlightColor, PressedControlBlend);
				Color tintedColor = RenderingHelper.BlendColors(mixedColor, windowColor, PressedWindowBlend);
				float brightness = (float)(tintedColor.R + tintedColor.G + tintedColor.B) / 3f / ChannelMax;
				if (brightness >= BrightnessLimit)
				{
					float brightnessDelta = brightness - BrightnessDrop;
					tintedColor = RenderingHelper.BlendColors(RenderingHelper.BlendColors(mixedColor, Color.Black, 1f - brightnessDelta), windowColor, PressedWindowBlend);
				}
				if (RenderingHelper.AreColorsEqual(tintedColor, BackColor))
				{
					return highlightColor;
				}
			}
			return BackColor;
		}

		internal float GetEffectiveBlend()
		{
			if (Blend == NoBlend)
			{
				return 0f;
			}
			return Blend;
		}

		internal float GetEffectiveBlendGradient()
		{
			if (BlendGradient == NoBlend)
			{
				return 0f;
			}
			return BlendGradient;
		}

		internal Color GetWindowColor()
		{
			return SystemColors.Window;
		}
	}
}
