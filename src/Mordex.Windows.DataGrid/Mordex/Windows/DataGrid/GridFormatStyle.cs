using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public partial class GridFormatStyle : IMordexXmlLayoutsSupport
	{
		private bool _isReadOnly;

		internal IFormatStyleOwner _owner;

		private float _floatValue = 1f;

		private Color _defaultBaseColor;

		private string _identifier;

		private int _alpha;

		private Color _backgroundColor;

		private Color _backColorGradient;

		private BackgroundGradientMode _backgroundGradientMode;

		private AlphaMode _backColorAlphaMode;

		private Color _textColor;

		private AlphaMode _foreColorAlphaMode;

		// _baseFont is the font the style was given; the Font* overrides and the scale are never baked into it. ResolveFont applies
		// them to _baseFont (or to the default font) in one cached _resolvedFont, so every scale uses the same overrides.
		// Fonts returned by Font and ResolveFont are borrowed: use them for the current paint or measure, and clone one to keep it.
		// The style owns a _baseFont it adopted (_isUserFont false) and disposes it when it replaces it, on Reset and in
		// DisposeResources. A font assigned through Font belongs to the caller. A font copied from another style and the
		// _resolvedFont come from GridFontCache: a reference count shares one Font between the styles that need it and the
		// last style to release it disposes it (the grid defers that to the end of a paint pass).
		private bool _isUserFont;

		private bool _isPooledBaseFont;

		private Font _baseFont;

		private Font _resolvedFont;

		private Font _resolvedFontSource;

		private Appearance _appearance;

		private Image _backgroundImage;

		private ImageHorizontalAlignment _imageHorizontalAlignment;

		private ImageVerticalAlignment _imageVerticalAlignment;

		private BackgroundImageDrawMode _backgroundImageDrawMode;

		private TriState _fontBold;

		private TriState _fontItalic;

		private TriState _fontUnderline;

		private TriState _fontStrikeout;

		private string _fontName;

		private float _fontSize;

		private Brush _backBrush;

		private bool _translateSystemColors;

		private Brush _gridFormatStyleBrush;

		private TextAlignment _textAlignment;

		private TextAlignment _lineAlignment;

		private PredefinedStyle _predefinedStyle;

		private Color _defaultBackColorGradient = Color.Empty;

		private Color _baseForeColor = Color.Empty;

		private bool _hasExplicitBackColor;

		// The gradient of a selected row in the theme, and the back color it stands for. The gradient is drawn only while
		// the style still has that back color, so a back color set by the developer wins.
		private System.Drawing.Drawing2D.ColorBlend _selectionBlend;

		private Color _selectionBlendColor = Color.Empty;

		private Color _baseBackColor = Color.Empty;

		private float _blend = -1f;

		private float _blendGradient = -1f;

		public GridFormatStyle()
			: this(null, PredefinedStyle.None)
		{
		}

		internal GridFormatStyle(IFormatStyleOwner owner, PredefinedStyle predefinedStyle)
		{
			_identifier = "";
			_owner = owner;
			_predefinedStyle = predefinedStyle;
			ApplyPredefinedColors();
			Reset();
		}

		public GridFormatStyle(GridFormatStyle formatStyle)
		{
			_defaultBaseColor = formatStyle._defaultBaseColor;
			_identifier = "";
			_predefinedStyle = PredefinedStyle.None;
			_alpha = formatStyle._alpha;
			_appearance = formatStyle._appearance;
			BackColor = formatStyle.BackColor;
			BackColorGradient = formatStyle.BackColorGradient;
			ForeColor = formatStyle.ForeColor;
			_backColorAlphaMode = formatStyle._backColorAlphaMode;
			_backgroundImage = formatStyle._backgroundImage;
			_backgroundImageDrawMode = formatStyle._backgroundImageDrawMode;
			_baseFont = (formatStyle._baseFont != null) ? GridFontCache.Acquire(formatStyle._baseFont) : null;
			_isPooledBaseFont = _baseFont != null;
			_fontBold = formatStyle._fontBold;
			_fontItalic = formatStyle._fontItalic;
			_fontName = formatStyle._fontName;
			_fontSize = formatStyle._fontSize;
			_fontStrikeout = formatStyle._fontStrikeout;
			_fontUnderline = formatStyle._fontUnderline;
			_foreColorAlphaMode = formatStyle._foreColorAlphaMode;
			_imageHorizontalAlignment = formatStyle._imageHorizontalAlignment;
			_imageVerticalAlignment = formatStyle._imageVerticalAlignment;
			_textAlignment = formatStyle._textAlignment;
			_lineAlignment = formatStyle._lineAlignment;
			_floatValue = formatStyle._floatValue;
			_blend = formatStyle._blend;
			_blendGradient = formatStyle._blendGradient;
			_backgroundGradientMode = formatStyle._backgroundGradientMode;
			_selectionBlend = formatStyle._selectionBlend;
			_selectionBlendColor = formatStyle._selectionBlendColor;
		}
	}
}
