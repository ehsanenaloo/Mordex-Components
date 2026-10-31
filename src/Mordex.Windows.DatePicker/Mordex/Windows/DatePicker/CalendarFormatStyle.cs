using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public class CalendarFormatStyle
	{
		internal IDatePickerElement _owner;

		private float _fontScale = 1f;

		private Color _backColor;

		private Color _backColorGradient;

		private BackgroundGradientMode _gradientMode;

		private Color _foreColor;

		private Font _styledFont;

		internal TriState _fontBold;

		private TriState _fontItalic;

		private TriState _fontUnderline;

		private TriState _fontStrikeout;

		private Brush _backBrush;

		private Rectangle _backBrushBounds;

		private PredefinedStyle _predefinedStyle;

		private float _blend = -1f;

		private float _blendGradient = -1f;

		private Color _predefinedForeColor;

		private Color _predefinedBackColor;

		private Color _predefinedBackColorGradient;

		private BackgroundGradientMode _predefinedGradientMode;

		[Browsable(false)]
		public DatePickerCalendar Calendar
		{
			get
			{
				if (_owner == null)
				{
					return null;
				}
				return _owner.Calendar;
			}
		}

		[Description("The background color of this CalendarFormatStyle.")]
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
					InvalidateCalendar();
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
					return _predefinedBackColorGradient;
				}
				return _backColorGradient;
			}
			set
			{
				if (value != _backColorGradient)
				{
					_backColorGradient = value;
					InvalidateCalendar();
				}
			}
		}

		[Description("Specifies if a linear gradient will be applied and its direction.")]
		public BackgroundGradientMode BackgrodunGradientMode
		{
			get
			{
				if (_gradientMode == BackgroundGradientMode.Empty)
				{
					return _predefinedGradientMode;
				}
				return _gradientMode;
			}
			set
			{
				if (_gradientMode != value)
				{
					_gradientMode = value;
					InvalidateCalendar();
				}
			}
		}

		[Description("Specifies the foreground color of the CalendarFormatStyle.")]
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
					InvalidateCalendar();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object used is bold.")]
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
					if (_styledFont != null)
					{
						RebuildFont();
					}
					InvalidateCalendar();
				}
			}
		}

		[Description("Indicates whether the Font object used is italic.")]
		[DefaultValue(TriState.Empty)]
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
					if (_styledFont != null)
					{
						RebuildFont();
					}
					InvalidateCalendar();
				}
			}
		}

		[Description("Specifies whether the Font object used is underlined.")]
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
					if (_styledFont != null)
					{
						RebuildFont();
					}
					InvalidateCalendar();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Specifies whether the Font object used has a horizontal line through the font.")]
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
					if (_styledFont != null)
					{
						RebuildFont();
					}
					InvalidateCalendar();
				}
			}
		}

		[Description("Indicates the percentage of the BlendColor to blend with the BackColor.")]
		[DefaultValue(-1f)]
		public float Blend
		{
			get
			{
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
					InvalidateCalendar();
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
					InvalidateCalendar();
				}
			}
		}

		internal static CalendarFormatStyle CreateResolvedCopy(CalendarFormatStyle baseStyle)
		{
			CalendarFormatStyle resolvedStyle = new CalendarFormatStyle(baseStyle);
			resolvedStyle._blend = baseStyle.GetBlendAmount();
			resolvedStyle._blendGradient = baseStyle.GetGradientBlendAmount();
			return resolvedStyle;
		}

		public CalendarFormatStyle()
			: this(PredefinedStyle.None)
		{
		}

		internal CalendarFormatStyle(PredefinedStyle predefinedStyle)
		{
			_predefinedStyle = predefinedStyle;
			Reset();
		}

		internal CalendarFormatStyle(IDatePickerElement owner, PredefinedStyle predefinedStyle)
		{
			_owner = owner;
			_predefinedStyle = predefinedStyle;
			Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_backColor = Color.Empty;
			_styledFont = null;
			_fontBold = TriState.Empty;
			_fontItalic = TriState.Empty;
			_fontStrikeout = TriState.Empty;
			_fontUnderline = TriState.Empty;
			_foreColor = Color.Empty;
			_fontScale = 1f;
			_blend = -1f;
			_blendGradient = -1f;
			_backColorGradient = Color.Empty;
			_gradientMode = BackgroundGradientMode.Empty;
			ApplyTheme();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_backColor != Color.Empty)
			{
				return true;
			}
			if (_styledFont != null)
			{
				return true;
			}
			if (_fontBold != TriState.Empty)
			{
				return true;
			}
			if (_fontItalic != TriState.Empty)
			{
				return true;
			}
			if (_fontStrikeout != TriState.Empty)
			{
				return true;
			}
			if (_fontUnderline != TriState.Empty)
			{
				return true;
			}
			if (!_foreColor.IsEmpty)
			{
				return true;
			}
			if (_fontScale != 1f)
			{
				return true;
			}
			if (_blend != -1f)
			{
				return true;
			}
			if (_blendGradient != -1f)
			{
				return true;
			}
			if (_backColorGradient != Color.Empty)
			{
				return true;
			}
			if (_gradientMode != BackgroundGradientMode.Empty)
			{
				return true;
			}
			return false;
		}

		internal bool IsDefault()
		{
			if (!_backColor.IsEmpty)
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
			if (_fontStrikeout != TriState.Empty)
			{
				return false;
			}
			if (_fontUnderline != TriState.Empty)
			{
				return false;
			}
			if (!_foreColor.IsEmpty)
			{
				return false;
			}
			if (_blend != -1f)
			{
				return false;
			}
			if (_blendGradient != -1f)
			{
				return false;
			}
			if (!_backColorGradient.IsEmpty)
			{
				return false;
			}
			if (_gradientMode != BackgroundGradientMode.Empty)
			{
				return false;
			}
			return true;
		}

		public CalendarFormatStyle(CalendarFormatStyle baseStyle)
		{
			_predefinedStyle = PredefinedStyle.None;
			_backColor = baseStyle.BackColor;
			if (baseStyle._styledFont != null)
			{
				_styledFont = (Font)baseStyle._styledFont.Clone();
				_fontBold = TriState.Empty;
				_fontItalic = TriState.Empty;
				_fontStrikeout = TriState.Empty;
				_fontUnderline = TriState.Empty;
			}
			else
			{
				_styledFont = null;
				_fontBold = baseStyle._fontBold;
				_fontItalic = baseStyle._fontItalic;
				_fontStrikeout = baseStyle._fontStrikeout;
				_fontUnderline = baseStyle._fontUnderline;
			}
			_foreColor = baseStyle.ForeColor;
			_fontScale = baseStyle._fontScale;
			_blend = baseStyle._blend;
			_blendGradient = baseStyle._blendGradient;
			_backColorGradient = baseStyle.BackColorGradient;
			_gradientMode = baseStyle.BackgrodunGradientMode;
		}

		internal CalendarFormatStyle MergeWith(CalendarFormatStyle overrideStyle)
		{
			if (overrideStyle == null || (overrideStyle._predefinedStyle == PredefinedStyle.None && overrideStyle.IsDefault()))
			{
				return this;
			}
			CalendarFormatStyle mergedStyle = new CalendarFormatStyle(this);
			if (overrideStyle._fontScale != 1f)
			{
				mergedStyle._fontScale = overrideStyle._fontScale;
			}
			if (!overrideStyle.BackColor.IsEmpty)
			{
				mergedStyle._backColor = overrideStyle.BackColor;
			}
			if (!overrideStyle.ForeColor.IsEmpty)
			{
				mergedStyle._foreColor = overrideStyle.ForeColor;
			}
			if (overrideStyle._blend != -1f)
			{
				mergedStyle._blend = overrideStyle._blend;
			}
			if (overrideStyle._blendGradient != -1f)
			{
				mergedStyle._blendGradient = overrideStyle._blendGradient;
			}
			if (overrideStyle.BackColorGradient != Color.Empty)
			{
				mergedStyle._backColorGradient = overrideStyle.BackColorGradient;
			}
			if (overrideStyle.BackgrodunGradientMode != BackgroundGradientMode.Empty)
			{
				mergedStyle._gradientMode = overrideStyle.BackgrodunGradientMode;
			}
			if (overrideStyle._styledFont != null)
			{
				mergedStyle._styledFont?.Dispose();
				mergedStyle._styledFont = (Font)overrideStyle._styledFont.Clone();
			}
			else
			{
				if (overrideStyle._fontBold != TriState.Empty)
				{
					mergedStyle._fontBold = overrideStyle._fontBold;
				}
				if (overrideStyle._fontItalic != TriState.Empty)
				{
					mergedStyle._fontItalic = overrideStyle._fontItalic;
				}
				if (overrideStyle._fontStrikeout != TriState.Empty)
				{
					mergedStyle._fontStrikeout = overrideStyle._fontStrikeout;
				}
				if (overrideStyle._fontUnderline != TriState.Empty)
				{
					mergedStyle._fontUnderline = overrideStyle._fontUnderline;
				}
				if (mergedStyle._styledFont != null)
				{
					mergedStyle.RebuildFont();
				}
			}
			return mergedStyle;
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
		public bool ShouldSerializeBackgrodunGradientMode()
		{
			return _gradientMode != BackgroundGradientMode.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetBackgrodunGradientMode()
		{
			BackgrodunGradientMode = BackgroundGradientMode.Empty;
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

		internal Font GetEffectiveFont(Font font)
		{
			if (_styledFont != null)
			{
				return _styledFont;
			}
			bool hasOverrides = _fontBold != TriState.Empty || _fontItalic != TriState.Empty || _fontStrikeout != TriState.Empty || _fontUnderline != TriState.Empty || _fontScale != 1f;
			if (!hasOverrides)
			{
				return font;
			}
			_styledFont = CreateStyledFont(font);
			return _styledFont;
		}

		internal void RebuildFont()
		{
			if (_styledFont != null)
			{
				Font oldFont = _styledFont;
				_styledFont = CreateStyledFont(oldFont);
				oldFont.Dispose();
			}
		}

		// Builds a new font from the source font with the bold, italic, underline, strikeout and scale settings of this style.
		private Font CreateStyledFont(Font source)
		{
			FontStyle fontStyle = FontStyle.Regular;
			if (ResolveFontFlag(_fontBold, source.Bold))
			{
				fontStyle |= FontStyle.Bold;
			}
			if (ResolveFontFlag(_fontItalic, source.Italic))
			{
				fontStyle |= FontStyle.Italic;
			}
			if (ResolveFontFlag(_fontStrikeout, source.Strikeout))
			{
				fontStyle |= FontStyle.Strikeout;
			}
			if (ResolveFontFlag(_fontUnderline, source.Underline))
			{
				fontStyle |= FontStyle.Underline;
			}
			float fontSize = source.SizeInPoints;
			if (_fontScale != 1f)
			{
				fontSize *= _fontScale;
			}
			return new Font(source.Name, fontSize, fontStyle);
		}

		private static bool ResolveFontFlag(TriState setting, bool sourceValue)
		{
			return setting == TriState.Empty ? sourceValue : setting != TriState.False;
		}

		private Brush CreateGradientBrush(Color startColor, Color endColor, Rectangle rect, BackgroundGradientMode backgroundGradientMode)
		{
			switch (backgroundGradientMode)
			{
			case BackgroundGradientMode.Solid:
				return new SolidBrush(startColor);
			case BackgroundGradientMode.Diagonal:
				return new LinearGradientBrush(rect, startColor, endColor, 45f);
			case BackgroundGradientMode.DiagonalBackward:
				return new LinearGradientBrush(rect, startColor, endColor, 225f);
			case BackgroundGradientMode.Horizontal:
				return new LinearGradientBrush(rect, startColor, endColor, 0f);
			case BackgroundGradientMode.Vertical:
				return new LinearGradientBrush(rect, startColor, endColor, 90f);
			default:
				return new SolidBrush(startColor);
			}
		}

		private void InvalidateCalendar()
		{
			if (Calendar != null)
			{
				Calendar.Invalidate();
			}
		}

		internal void DisposeResources()
		{
			if (_backBrush != null)
			{
				_backBrush.Dispose();
				_backBrush = null;
			}
			if (_styledFont != null)
			{
				_styledFont.Dispose();
				_styledFont = null;
			}
		}

		private bool HasGradient()
		{
			if (BackgrodunGradientMode != BackgroundGradientMode.Empty)
			{
				return BackgrodunGradientMode != BackgroundGradientMode.Solid;
			}
			return false;
		}

		internal Brush GetBackBrush(Rectangle rectangle)
		{
			if (HasGradient() && rectangle.Width > 0 && rectangle.Height > 0)
			{
				if (_backBrush == null || _backBrushBounds != rectangle)
				{
					_backBrush?.Dispose();
					_backBrushBounds = rectangle;
					_backBrush = CreateGradientBrush(BlendColors(255, GetBlendColor(), BackColor, GetBlendAmount()), BlendColors(255, GetBlendColor(), BackColorGradient, GetGradientBlendAmount()), rectangle, BackgrodunGradientMode);
				}
				return _backBrush;
			}
			if (GetBlendAmount() != 0f && !BackColor.IsEmpty)
			{
				return GdiCache.Brush(BlendColors(255, GetBlendColor(), BackColor, GetBlendAmount()));
			}
			return GdiCache.Brush(BackColor);
		}

		private Color BlendColors(int alpha, Color blendColor, Color baseColor, float blendAmount)
		{
			if (blendColor == Color.Empty)
			{
				return baseColor;
			}
			if (baseColor == Color.Empty)
			{
				return blendColor;
			}
			int red = (int)((float)(int)blendColor.R * blendAmount + (float)(int)baseColor.R * (1f - blendAmount));
			int green = (int)((float)(int)blendColor.G * blendAmount + (float)(int)baseColor.G * (1f - blendAmount));
			int blue = (int)((float)(int)blendColor.B * blendAmount + (float)(int)baseColor.B * (1f - blendAmount));
			return Color.FromArgb(alpha, red, green, blue);
		}

		internal int GetOpaqueAlpha()
		{
			return 255;
		}

		internal Brush GetForeBrush(Color fallbackColor)
		{
			return GdiCache.Brush(ForeColor != Color.Empty ? ForeColor : fallbackColor);
		}

		internal float GetBlendAmount()
		{
			if (_blend == -1f)
			{
				if (Calendar != null)
				{
					return Calendar.DefaultBlend;
				}
				return 0f;
			}
			return _blend;
		}

		internal float GetGradientBlendAmount()
		{
			if (_blendGradient == -1f)
			{
				if (Calendar != null)
				{
					return Calendar.DefaultBlend;
				}
				return 0f;
			}
			return _blendGradient;
		}

		internal Color GetBlendColor()
		{
			if (Calendar != null)
			{
				return Calendar.BlendColor;
			}
			if (GetOwnerControlStyle() != null)
			{
				return GetOwnerControlStyle().HoverBlendColor;
			}
			return SystemColors.Window;
		}

		internal Color GetBlendedBackColor()
		{
			if (BackColor.IsEmpty && GetBlendAmount() == 0f)
			{
				return BackColor;
			}
			return BlendColors(GetOpaqueAlpha(), GetBlendColor(), BackColor, GetBlendAmount());
		}

		private ControlStyle GetOwnerControlStyle()
		{
			if (_owner != null && _owner is ControlStyle)
			{
				return _owner as ControlStyle;
			}
			return null;
		}

		// The theme sets the defaults of the predefined styles. A color that the developer sets still wins.
		private void ApplyThemedColors()
		{
			ResolvedTheme theme = MxThemeManager.ResolveDefault();
			switch (_predefinedStyle)
			{
			case PredefinedStyle.Header:
				_predefinedBackColor = theme.Color(ThemeRole.Header);
				_predefinedForeColor = theme.Color(ThemeRole.HeaderText);
				_predefinedGradientMode = BackgroundGradientMode.Solid;
				break;
			case PredefinedStyle.Button:
				_predefinedBackColor = theme.Color(ThemeRole.SurfaceRaised);
				_predefinedForeColor = theme.Color(ThemeRole.Text);
				_predefinedGradientMode = BackgroundGradientMode.Solid;
				break;
			case PredefinedStyle.Hot:
				_predefinedBackColor = theme.Color(ThemeRole.SurfaceHot);
				_predefinedGradientMode = BackgroundGradientMode.Solid;
				break;
			case PredefinedStyle.Pressed:
				_predefinedBackColor = theme.Color(ThemeRole.SurfacePressed);
				_predefinedGradientMode = BackgroundGradientMode.Solid;
				break;
			case PredefinedStyle.Selected:
				_predefinedBackColor = theme.Color(ThemeRole.Selection);
				_predefinedForeColor = theme.Color(ThemeRole.SelectionText);
				_predefinedGradientMode = BackgroundGradientMode.Solid;
				break;
			case PredefinedStyle.Trailing:
				_predefinedForeColor = theme.Color(ThemeRole.TextDisabled);
				break;
			}
			ThemeStyle look = theme.Style;
			if (look.FaceGradient != ThemeGradient.None && (_predefinedStyle == PredefinedStyle.Header || _predefinedStyle == PredefinedStyle.Button || _predefinedStyle == PredefinedStyle.Hot || _predefinedStyle == PredefinedStyle.Pressed))
			{
				Color face = _predefinedBackColor;
				_predefinedBackColor = look.LightEnd(face);
				_predefinedBackColorGradient = look.DarkEnd(face);
				_predefinedGradientMode = look.FaceGradient == ThemeGradient.Horizontal ? BackgroundGradientMode.Horizontal : BackgroundGradientMode.Vertical;
			}
		}

		internal void ApplyTheme()
		{
			_predefinedForeColor = Color.Empty;
			_predefinedBackColor = Color.Empty;
			_predefinedBackColorGradient = Color.Empty;
			_predefinedGradientMode = BackgroundGradientMode.Empty;
			ApplyThemedColors();
		}
	}
}
