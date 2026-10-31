using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Navigation
{
	[Localizable(true)]
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class NavigationBarFormatStyle : IMordexXmlLayoutsSupport
	{
		public const int AlphaEmpty = -1;

		internal INavigationBarOwner _owner;

		private bool _canInvalidateOwner = true;

		private int _alpha = -1;

		private Color _backColor = Color.Empty;

		private AlphaMode _backColorAlphaMode;

		private AlphaMode _backColorGradientAlphaMode;

		private Color _foreColor = Color.Empty;

		private AlphaMode _foreColorAlphaMode;

		private Font _font;

		private Font _effectiveFont;

		private Font _effectiveFontSource;

		private TriState _fontBold;

		private TriState _fontItalic;

		private TriState _fontUnderline;

		private TriState _fontStrikeout;

		private string _fontName;

		private float _fontSize;

		private Brush _backBrush;

		private Brush _foreBrush;

		private float _blend = -1f;

		private Color _backColorGradient;

		private BackgroundGradientMode _backgroundGradientMode;

		private float _blendGradient = -1f;

		private Color _predefinedForeColor = Color.Empty;

		private Color _predefinedBackColor = Color.Empty;

		private Color _defaultBackColorGradient = Color.Empty;

		private BackgroundGradientMode _defaultBackgroundGradientMode;

		private float _blendFactor = -1f;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		public MxNavigationBar NavigationBar
		{
			get
			{
				if (_owner == null)
				{
					return null;
				}
				return _owner.NavigationBar;
			}
		}

		[DefaultValue(-1)]
		[Description("Gets or sets a value specifying the level of alpha (transparency).")]
		public int Alpha
		{
			get
			{
				return _alpha;
			}
			set
			{
				if (value < -1 || value > 255)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _alpha)
				{
					_alpha = value;
					RepaintOwner();
				}
			}
		}

		[Description("The background color for this NavigationBarFormatStyle.")]
		public Color BackColor
		{
			get
			{
				if (_backColor.IsEmpty)
				{
					return _predefinedBackColor;
				}
				return _backColor;
			}
			set
			{
				if (value != _backColor)
				{
					_backColor = value;
					RepaintOwner();
				}
			}
		}

		[Description("Specifies the level of alpha (transparency) to use in the background.")]
		[DefaultValue(AlphaMode.Empty)]
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
					RepaintOwner();
				}
			}
		}

		[Description("Specifies the level of alpha (transparency) to use in the background of the second color in a linear gradient pattern.")]
		[DefaultValue(AlphaMode.Empty)]
		public AlphaMode BackColorGradientAlphaMode
		{
			get
			{
				return _backColorGradientAlphaMode;
			}
			set
			{
				if (!Enum.IsDefined(typeof(AlphaMode), value))
				{
					throw new InvalidEnumArgumentException();
				}
				if (value != _backColorGradientAlphaMode)
				{
					_backColorGradientAlphaMode = value;
					RepaintOwner();
				}
			}
		}

		[Description("The foreground color for this NavigationBarFormatStyle.")]
		public Color ForeColor
		{
			get
			{
				if (_foreColor.IsEmpty)
				{
					return _predefinedForeColor;
				}
				return _foreColor;
			}
			set
			{
				if (value != _foreColor)
				{
					_foreColor = value;
					RepaintOwner();
				}
			}
		}

		[Description("Specifies the level of alpha (transparency) to use in the foreground.")]
		[DefaultValue(AlphaMode.Empty)]
		public AlphaMode ForeColorAlphaMode
		{
			get
			{
				return _foreColorAlphaMode;
			}
			set
			{
				if (value != _foreColorAlphaMode)
				{
					_foreColorAlphaMode = value;
					RepaintOwner();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The Font object used by this NavigationBarFormatStyle.")]
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
					ReleaseEffectiveFont();
					RepaintOwner();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object of this NavigationBarFormatStyle is bold.")]
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
					ReleaseEffectiveFont();
					RelayoutOwner();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object of this NavigationBarFormatStyle is italic.")]
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
					ReleaseEffectiveFont();
					RelayoutOwner();
				}
			}
		}

		[Description("Indicates whether the Font object of this NavigationBarFormatStyle is underlined.")]
		[DefaultValue(TriState.Empty)]
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
					ReleaseEffectiveFont();
					RelayoutOwner();
				}
			}
		}

		[Description("Indicates whether the Font object of this NavigationBarFormatStyle specifies a horizontal line through the font.")]
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
					ReleaseEffectiveFont();
					RelayoutOwner();
				}
			}
		}

		[DefaultValue("")]
		[Description("The face name of the font used by the NavigationBarFormatSrtyle class.")]
		public string FontName
		{
			get
			{
				return _fontName;
			}
			set
			{
				if (value != _fontName)
				{
					_fontName = value;
					ReleaseEffectiveFont();
					RelayoutOwner();
				}
			}
		}

		[DefaultValue(0f)]
		[Description("The size of the Font object used by this NavigationBarFormatStyle object.")]
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
					ReleaseEffectiveFont();
					RelayoutOwner();
				}
			}
		}

		[DefaultValue(-1f)]
		[Description("Indicates the percentage of the BlendColor to blend with the BackColor.")]
		public float Blend
		{
			get
			{
				if (_blend == -1f)
				{
					return _blendFactor;
				}
				return _blend;
			}
			set
			{
				if ((value < 0f || value > 1f) && value != -1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _blend)
				{
					_blend = value;
					RepaintOwner();
				}
			}
		}

		[Description("Specifies the background color used in a linear gradient pathern.")]
		public Color BackColorGradient
		{
			get
			{
				if (_backColorGradient.IsEmpty)
				{
					return _defaultBackColorGradient;
				}
				return _backColorGradient;
			}
			set
			{
				if (value != _backColorGradient)
				{
					_backColorGradient = value;
					RepaintOwner();
				}
			}
		}

		[Description("Specifies if a linear gradient will be applied and its direction.")]
		public BackgroundGradientMode BackgroundGradientMode
		{
			get
			{
				if (_backgroundGradientMode == BackgroundGradientMode.Empty)
				{
					return _defaultBackgroundGradientMode;
				}
				return _backgroundGradientMode;
			}
			set
			{
				if (_backgroundGradientMode != value)
				{
					_backgroundGradientMode = value;
					RepaintOwner();
				}
			}
		}

		[Description("Indicates the percentage of the BlendColor to blend with the BackColorGradient.")]
		[DefaultValue(-1f)]
		public float BlendGradient
		{
			get
			{
				return _blendGradient;
			}
			set
			{
				if ((value < 0f || value > 1f) && value != -1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _blendGradient)
				{
					_blendGradient = value;
					RepaintOwner();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), -1);
			_backColor = (Color)LayoutReader.GetPropertyValue(info, "BackColor", typeof(Color), Color.Empty);
			_backColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "BackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_backColorGradient = (Color)LayoutReader.GetPropertyValue(info, "BackColorGradient", typeof(Color), _backColorGradient);
			_backColorGradientAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "BackColorGradientAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_backgroundGradientMode = (BackgroundGradientMode)LayoutReader.GetPropertyValue(info, "BackgroundGradientMode", typeof(BackgroundGradientMode), BackgroundGradientMode.Empty);
			_blend = (float)LayoutReader.GetPropertyValue(info, "Blend", typeof(float), -1f);
			_blendGradient = (float)LayoutReader.GetPropertyValue(info, "BlendGradient", typeof(float), -1f);
			_font = (Font)LayoutReader.GetPropertyValue(info, "Font", typeof(Font), null);
			_fontBold = (TriState)LayoutReader.GetPropertyValue(info, "FontBold", typeof(TriState), TriState.Empty);
			_fontItalic = (TriState)LayoutReader.GetPropertyValue(info, "FontItalic", typeof(TriState), TriState.Empty);
			_fontName = (string)LayoutReader.GetPropertyValue(info, "FontName", typeof(string), "");
			_fontSize = (float)LayoutReader.GetPropertyValue(info, "FontSize", typeof(float), 0f);
			_fontStrikeout = (TriState)LayoutReader.GetPropertyValue(info, "FontStrikeout", typeof(TriState), TriState.Empty);
			_fontUnderline = (TriState)LayoutReader.GetPropertyValue(info, "FontUnderline", typeof(TriState), TriState.Empty);
			_foreColor = (Color)LayoutReader.GetPropertyValue(info, "ForeColor", typeof(Color), Color.Empty);
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "ForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			ReleaseEffectiveFont();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_alpha != -1)
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
			if (_backColorGradient != Color.Empty)
			{
				info.AddValue("BackColorGradient", _backColorGradient);
			}
			if (_backColorGradientAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("BackColorGradientAlphaMode", _backColorGradientAlphaMode);
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				info.AddValue("BackgroundGradientMode", _backgroundGradientMode);
			}
			if (_blend != -1f)
			{
				info.AddValue("Blend", _blend);
			}
			if (BlendGradient != -1f)
			{
				info.AddValue("BlendGradient", _blendGradient);
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
			if (_fontName != "")
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
		}

		internal NavigationBarFormatStyle(INavigationBarOwner owner)
		{
			_owner = owner;
			Reset();
		}

		internal NavigationBarFormatStyle()
		{
			Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_backColor = Color.Empty;
			_backgroundGradientMode = BackgroundGradientMode.Empty;
			_backColorGradient = Color.Empty;
			_blendGradient = -1f;
			_backColorGradientAlphaMode = AlphaMode.Empty;
			_backColorAlphaMode = AlphaMode.Empty;
			_font = null;
			ReleaseEffectiveFont();
			_fontBold = TriState.Empty;
			_fontItalic = TriState.Empty;
			_fontName = "";
			_fontSize = 0f;
			_fontStrikeout = TriState.Empty;
			_fontUnderline = TriState.Empty;
			_foreColor = Color.Empty;
			_foreColorAlphaMode = AlphaMode.Empty;
			_blend = -1f;
			_alpha = -1;
			_predefinedBackColor = Color.Empty;
			_defaultBackColorGradient = Color.Empty;
			_defaultBackgroundGradientMode = BackgroundGradientMode.Empty;
			_blendFactor = -1f;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			return !IsEmptyStyle();
		}

		internal bool IsEmptyStyle()
		{
			if (_alpha != -1)
			{
				return false;
			}
			if (BackColor != Color.Empty)
			{
				return false;
			}
			if (BackColorGradient != Color.Empty)
			{
				return false;
			}
			if (_backColorGradientAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (BackgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return false;
			}
			if (_blendGradient != -1f)
			{
				return false;
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_font != null)
			{
				return false;
			}
			if (_fontBold != TriState.Empty)
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
			if (_foreColor != Color.Empty)
			{
				return false;
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (Blend != -1f)
			{
				return false;
			}
			return true;
		}

		public NavigationBarFormatStyle(NavigationBarFormatStyle baseStyle)
		{
			_alpha = baseStyle._alpha;
			_backColor = baseStyle.BackColor;
			_backColorAlphaMode = baseStyle._backColorAlphaMode;
			_backColorGradient = baseStyle.BackColorGradient;
			_backColorGradientAlphaMode = baseStyle._backColorGradientAlphaMode;
			_backgroundGradientMode = baseStyle.BackgroundGradientMode;
			_blendGradient = baseStyle._blendGradient;
			CopyFontSettings(baseStyle);
			_foreColor = baseStyle.ForeColor;
			_foreColorAlphaMode = baseStyle._foreColorAlphaMode;
			_blend = baseStyle.Blend;
			_owner = baseStyle._owner;
			_canInvalidateOwner = false;
		}

		internal void SetAlphaModes(int alpha, AlphaMode backColorAlphaMode, AlphaMode foreColorAlphaMode)
		{
			_alpha = alpha;
			_backColorAlphaMode = backColorAlphaMode;
			_foreColorAlphaMode = foreColorAlphaMode;
		}

		internal NavigationBarFormatStyle MergeWith(NavigationBarFormatStyle overrideStyle)
		{
			// Always a new style: callers dispose the result, and this style may be a live style of the control.
			NavigationBarFormatStyle navigationBarFormatStyle = new NavigationBarFormatStyle(this);
			if (overrideStyle == null)
			{
				return navigationBarFormatStyle;
			}
			if (overrideStyle.IsEmptyStyle())
			{
				if (overrideStyle._owner != null)
				{
					navigationBarFormatStyle._owner = overrideStyle._owner;
				}
				return navigationBarFormatStyle;
			}
			if (overrideStyle._alpha != -1)
			{
				navigationBarFormatStyle._alpha = overrideStyle._alpha;
			}
			if (!overrideStyle.BackColor.IsEmpty)
			{
				navigationBarFormatStyle._backColor = overrideStyle.BackColor;
			}
			if (overrideStyle.BackColorGradient != Color.Empty)
			{
				navigationBarFormatStyle._backColorGradient = overrideStyle.BackColorGradient;
			}
			if (overrideStyle._blendGradient != -1f)
			{
				navigationBarFormatStyle._blendGradient = overrideStyle._blendGradient;
			}
			if (overrideStyle._backColorGradientAlphaMode != AlphaMode.Empty)
			{
				navigationBarFormatStyle._backColorGradientAlphaMode = overrideStyle._backColorGradientAlphaMode;
			}
			if (overrideStyle.BackgroundGradientMode != BackgroundGradientMode.Empty)
			{
				navigationBarFormatStyle._backgroundGradientMode = overrideStyle.BackgroundGradientMode;
			}
			if (overrideStyle._backColorAlphaMode != AlphaMode.Empty)
			{
				navigationBarFormatStyle._backColorAlphaMode = overrideStyle._backColorAlphaMode;
			}
			if (!overrideStyle.ForeColor.IsEmpty)
			{
				navigationBarFormatStyle._foreColor = overrideStyle.ForeColor;
			}
			if (overrideStyle._foreColorAlphaMode != AlphaMode.Empty)
			{
				navigationBarFormatStyle._foreColorAlphaMode = overrideStyle._foreColorAlphaMode;
			}
			if (overrideStyle.Blend != -1f)
			{
				navigationBarFormatStyle._blend = overrideStyle.Blend;
			}
			if (overrideStyle._owner != null)
			{
				navigationBarFormatStyle._owner = overrideStyle._owner;
			}
			if (overrideStyle._font != null)
			{
				navigationBarFormatStyle.CopyFontSettings(overrideStyle);
			}
			else
			{
				if (overrideStyle._fontBold != TriState.Empty)
				{
					navigationBarFormatStyle._fontBold = overrideStyle._fontBold;
				}
				if (overrideStyle._fontItalic != TriState.Empty)
				{
					navigationBarFormatStyle._fontItalic = overrideStyle._fontItalic;
				}
				if (overrideStyle._fontName != "")
				{
					navigationBarFormatStyle._fontName = overrideStyle._fontName;
				}
				if (overrideStyle._fontSize != 0f)
				{
					navigationBarFormatStyle._fontSize = overrideStyle._fontSize;
				}
				if (overrideStyle._fontStrikeout != TriState.Empty)
				{
					navigationBarFormatStyle._fontStrikeout = overrideStyle._fontStrikeout;
				}
				if (overrideStyle._fontUnderline != TriState.Empty)
				{
					navigationBarFormatStyle._fontUnderline = overrideStyle._fontUnderline;
				}
			}
			return navigationBarFormatStyle;
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
		public void ResetForeColor()
		{
			ForeColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeForeColor()
		{
			return !_foreColor.IsEmpty;
		}

		// Only _effectiveFont belongs to this style. The font set by the user and the base font are never disposed here.
		internal Font GetEffectiveFont(Font baseFont)
		{
			Font source = _font ?? baseFont;
			if (!HasFontOverrides())
			{
				return source;
			}
			if (_effectiveFont == null || _effectiveFontSource != source)
			{
				ReleaseEffectiveFont();
				_effectiveFont = CreateFontWithOverrides(source);
				_effectiveFontSource = source;
			}
			return _effectiveFont;
		}

		private bool HasFontOverrides()
		{
			return _fontBold != TriState.Empty || _fontItalic != TriState.Empty || !string.IsNullOrEmpty(_fontName) || _fontSize != 0f || _fontStrikeout != TriState.Empty || _fontUnderline != TriState.Empty;
		}

		private void CopyFontSettings(NavigationBarFormatStyle source)
		{
			_font = source._font;
			_fontBold = source._fontBold;
			_fontItalic = source._fontItalic;
			_fontName = source._fontName;
			_fontSize = source._fontSize;
			_fontStrikeout = source._fontStrikeout;
			_fontUnderline = source._fontUnderline;
			ReleaseEffectiveFont();
		}

		private void ReleaseEffectiveFont()
		{
			_effectiveFont?.Dispose();
			_effectiveFont = null;
			_effectiveFontSource = null;
		}

		private Font CreateFontWithOverrides(Font source)
		{
			string name = source.Name;
			float sizeInPoints = source.SizeInPoints;
			FontStyle fontStyle = FontStyle.Regular;
			if (!string.IsNullOrEmpty(_fontName))
			{
				name = _fontName;
			}
			if (_fontSize > 0f)
			{
				sizeInPoints = _fontSize;
			}
			if (_fontBold == TriState.Empty ? source.Bold : _fontBold != TriState.False)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (_fontItalic == TriState.Empty ? source.Italic : _fontItalic != TriState.False)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (_fontStrikeout == TriState.Empty ? source.Strikeout : _fontStrikeout != TriState.False)
			{
				fontStyle |= FontStyle.Strikeout;
			}
			if (_fontUnderline == TriState.Empty ? source.Underline : _fontUnderline != TriState.False)
			{
				fontStyle |= FontStyle.Underline;
			}
			return new Font(name, sizeInPoints, fontStyle);
		}

		private void RelayoutOwner()
		{
			if (NavigationBar != null && _canInvalidateOwner)
			{
				NavigationBar.InvalidateLayout(false);
			}
		}

		private void RepaintOwner()
		{
			if (NavigationBar != null && _canInvalidateOwner)
			{
				NavigationBar.Invalidate(invalidateChildren: true);
			}
		}

		internal void DisposeBrushesAndFont()
		{
			if (_backBrush != null)
			{
				_backBrush.Dispose();
				_backBrush = null;
			}
			if (_foreBrush != null)
			{
				_foreBrush.Dispose();
				_foreBrush = null;
			}
			ReleaseEffectiveFont();
		}

		private Brush CreateGradientBrush(Color startColor, Color endColor, Rectangle bounds, BackgroundGradientMode backgroundGradientMode)
		{
			switch (backgroundGradientMode)
			{
			case BackgroundGradientMode.Solid:
				return new SolidBrush(startColor);
			case BackgroundGradientMode.Diagonal:
				return new LinearGradientBrush(bounds, startColor, endColor, 45f);
			case BackgroundGradientMode.DiagonalBackwards:
				return new LinearGradientBrush(bounds, startColor, endColor, 225f);
			case BackgroundGradientMode.Horizontal:
				return new LinearGradientBrush(bounds, startColor, endColor, 0f);
			case BackgroundGradientMode.Vertical:
				return new LinearGradientBrush(bounds, startColor, endColor, 90f);
			default:
				return new SolidBrush(startColor);
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

		internal Brush CreateBackBrush(Rectangle bounds)
		{
			if (HasGradient() && bounds.Width > 0 && bounds.Height > 0)
			{
				Color startColor = BlendColors(GetBlendColor(), BackColor, GetEffectiveBlend(), _alpha, _backColorAlphaMode);
				Color endColor = BlendColors(GetBlendColor(), BackColorGradient, GetGradientBlend(), _alpha, _backColorGradientAlphaMode);
				return CreateGradientBrush(startColor, endColor, bounds, BackgroundGradientMode);
			}
			if (GetEffectiveBlend() != 0f && !BackColor.IsEmpty)
			{
				return new SolidBrush(BlendColors(BackColor, GetBlendColor(), GetEffectiveBlend(), _alpha, _backColorAlphaMode));
			}
			if (BackColor.IsEmpty)
			{
				return new SolidBrush(Color.Empty);
			}
			return new SolidBrush(Color.FromArgb(ResolveAlpha(_alpha, _backColorAlphaMode), BackColor));
		}

		internal Brush GetCachedBackBrush(Rectangle bounds)
		{
			if (_backBrush == null || HasGradient())
			{
				if (_backBrush != null)
				{
					_backBrush.Dispose();
					_backBrush = null;
				}
				if (HasGradient() && bounds.Width > 0 && bounds.Height > 0)
				{
					bounds.X--;
					bounds.Width++;
					Color startColor = BlendColors(GetBlendColor(), BackColor, GetEffectiveBlend(), _alpha, _backColorAlphaMode);
					Color endColor = BlendColors(GetBlendColor(), BackColorGradient, GetGradientBlend(), _alpha, _backColorGradientAlphaMode);
					_backBrush = CreateGradientBrush(startColor, endColor, bounds, BackgroundGradientMode);
				}
				else if (GetEffectiveBlend() != 0f && !BackColor.IsEmpty)
				{
					_backBrush = new SolidBrush(BlendColors(GetBlendColor(), BackColor, GetEffectiveBlend(), _alpha, _backColorAlphaMode));
				}
				else
				{
					if (BackColor.IsEmpty)
					{
						return null;
					}
					_backBrush = new SolidBrush(Color.FromArgb(ResolveAlpha(_alpha, _backColorAlphaMode), BackColor));
				}
			}
			return _backBrush;
		}

		private int ResolveAlpha(int alpha, AlphaMode alphaMode)
		{
			switch (alphaMode)
			{
			case AlphaMode.UseAlpha:
				if (alpha == -1)
				{
					return 255;
				}
				return alpha;
			case AlphaMode.Transparent:
				return 0;
			default:
				return 255;
			}
		}

		private Color BlendColors(Color primaryColor, Color secondaryColor, float floatValue, int alpha, AlphaMode alphaMode)
		{
			int red = (int)((float)(int)primaryColor.R * floatValue + (float)(int)secondaryColor.R * (1f - floatValue));
			int green = (int)((float)(int)primaryColor.G * floatValue + (float)(int)secondaryColor.G * (1f - floatValue));
			int blue = (int)((float)(int)primaryColor.B * floatValue + (float)(int)secondaryColor.B * (1f - floatValue));
			if (alphaMode == AlphaMode.Empty)
			{
				alphaMode = AlphaMode.Opaque;
			}
			switch (alphaMode)
			{
			case AlphaMode.Transparent:
				return Color.Transparent;
			case AlphaMode.UseAlpha:
				if (_alpha == -1)
				{
					_alpha = 255;
				}
				return Color.FromArgb(_alpha, red, green, blue);
			default:
				return Color.FromArgb(red, green, blue);
			}
		}

		internal Brush GetForeBrush(Color defaultColor)
		{
			if (!ForeColor.IsEmpty)
			{
				defaultColor = ForeColor;
			}
			if (_foreBrush == null)
			{
				_foreBrush = NavigationGraphicsHelper.CreateSolidBrush(defaultColor, _alpha, _foreColorAlphaMode);
			}
			return _foreBrush;
		}

		internal Color GetForeColorOrDefault(Color defaultColor)
		{
			if (ForeColor.IsEmpty)
			{
				return defaultColor;
			}
			return ForeColor;
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

		internal float GetGradientBlend()
		{
			if (_blendGradient == -1f)
			{
				if (NavigationBar != null)
				{
					return NavigationBar.DefaultBlend;
				}
				return 0f;
			}
			return _blendGradient;
		}

		internal float GetEffectiveBlend()
		{
			if (Blend == -1f)
			{
				if (NavigationBar != null)
				{
					return NavigationBar.DefaultBlend;
				}
				return 0f;
			}
			return Blend;
		}

		internal Color GetBlendColor()
		{
			if (NavigationBar != null)
			{
				return NavigationBar.BlendColor;
			}
			return SystemColors.Window;
		}

		internal void ApplyThemedColors(Color back, Color fore, Color gradient)
		{
			ApplyThemedColors(back, fore);
			if (!back.IsEmpty && !gradient.IsEmpty)
			{
				_defaultBackColorGradient = gradient;
				_defaultBackgroundGradientMode = BackgroundGradientMode.Vertical;
			}
		}

		internal void ApplyThemedColors(Color back, Color fore)
		{
			_predefinedForeColor = fore;
			_predefinedBackColor = back;
			_defaultBackColorGradient = Color.Empty;
			_defaultBackgroundGradientMode = back.IsEmpty ? BackgroundGradientMode.Empty : BackgroundGradientMode.Solid;
			_blendFactor = -1f;
		}
	}
}
