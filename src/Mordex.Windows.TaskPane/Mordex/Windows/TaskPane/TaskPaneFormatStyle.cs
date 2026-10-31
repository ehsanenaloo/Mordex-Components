using System;
using System.ComponentModel;
using System.Drawing;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.TaskPane
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	[Localizable(true)]
	public class TaskPaneFormatStyle : IMordexXmlLayoutsSupport
	{
		public const int AlphaEmpty = -1;

		internal ITaskPaneOwner _owner;

		private int _alpha = -1;

		private Color _backColor;

		private PredefinedStyle _predefinedStyle;

		private AlphaMode _backColorAlphaMode;

		internal Color _foreColor;

		private AlphaMode _foreColorAlphaMode;

		private Font _font;

		private Font _resolvedFont;

		private Font _resolvedFontSource;

		private TriState _fontBold;

		private TriState _fontItalic;

		internal TriState _fontUnderline;

		private TriState _fontStrikeout;

		private string _fontName;

		private float _fontSize;

		private bool _invalidatesOwner = true;

		private Brush _backBrush;

		private Pen _backPen;

		private Color _backColorGradient;

		private AlphaMode _backColorGradientAlphaMode;

		private float _blendGradient = -1f;

		private Brush _foreBrush;

		private float _blend = -1f;

		private BackgroundGradientMode _backgroundGradientMode;

		internal Color _defaultForeColor = Color.Empty;

		private Color _defaultBackColorGradient = Color.Empty;

		private Color _defaultBackColor = Color.Empty;

		private BackgroundGradientMode _defaultBackgroundGradientMode;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		public MxTaskPane TaskPane
		{
			get
			{
				if (_owner == null)
				{
					return null;
				}
				return _owner.TaskPane;
			}
		}

		[Description("Gets or sets a value specifying the level of alpha (transparency).")]
		[DefaultValue(-1)]
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
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("The background color for this TaskPaneFormatStyle.")]
		public Color BackColor
		{
			get
			{
				if (_backColor.IsEmpty)
				{
					return _defaultBackColor;
				}
				return _backColor;
			}
			set
			{
				if (value != _backColor)
				{
					_backColor = value;
					InvalidateOwnerLayout();
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
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("The foreground color for this TaskPaneFormatStyle.")]
		public Color ForeColor
		{
			get
			{
				if (_foreColor.IsEmpty)
				{
					return _defaultForeColor;
				}
				return _foreColor;
			}
			set
			{
				if (value != _foreColor)
				{
					_foreColor = value;
					InvalidateOwnerLayout();
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
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(null)]
		[Description("The Font object used by this TaskPaneFormatStyle.")]
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
					ReleaseResolvedFont();
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Indicates whether the Font object of this TaskPaneFormatStyle is bold.")]
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
					ReleaseResolvedFont();
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Indicates whether the Font object of this TaskPaneFormatStyle is italic.")]
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
					ReleaseResolvedFont();
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(TriState.Empty)]
		[Description("Indicates whether the Font object of this TaskPaneFormatStyle is underlined.")]
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
					ReleaseResolvedFont();
					InvalidateOwnerLayout();
				}
			}
		}

		[Description("Indicates whether the Font object of this TaskPaneFormatStyle specifies a horizontal line through the font.")]
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
					ReleaseResolvedFont();
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue("")]
		[Description("The face name of the font used by the TaskPaneFormatSrtyle class.")]
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
					ReleaseResolvedFont();
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(0f)]
		[Description("The size of the Font object used by this TaskPaneFormatStyle object.")]
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
					ReleaseResolvedFont();
					InvalidateOwnerLayout();
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
					return GetDefaultBackColorGradient();
				}
				return _backColorGradient;
			}
			set
			{
				if (value != _backColorGradient)
				{
					_backColorGradient = value;
					InvalidateOwnerLayout();
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
					InvalidateOwnerLayout();
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
					InvalidateOwnerLayout();
				}
			}
		}

		[DefaultValue(-1f)]
		[Description("Indicates the percentage of the BlendColor to blend with the BackColor.")]
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
					InvalidateOwnerLayout();
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
					InvalidateOwnerLayout();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_predefinedStyle = (PredefinedStyle)LayoutReader.GetPropertyValue(info, "PredefinedStyle", typeof(PredefinedStyle), PredefinedStyle.None);
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), -1);
			_backColor = (Color)LayoutReader.GetPropertyValue(info, "BackColor", typeof(Color), GetDefaultBackColor());
			_backColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "BackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_backColorGradient = (Color)LayoutReader.GetPropertyValue(info, "BackColorGradient", typeof(Color), _backColorGradient);
			_backColorGradientAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "BackColorGradientAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_backgroundGradientMode = (BackgroundGradientMode)LayoutReader.GetPropertyValue(info, "BackgroundGradientMode", typeof(BackgroundGradientMode), BackgroundGradientMode.Empty);
			_blend = (float)LayoutReader.GetPropertyValue(info, "Blend", typeof(float), -1f);
			_blendGradient = (float)LayoutReader.GetPropertyValue(info, "BlendGradient", typeof(float), -1f);
			_font = (Font)LayoutReader.GetPropertyValue(info, "Font", typeof(Font), null);
			_fontBold = (TriState)LayoutReader.GetPropertyValue(info, "FontBold", typeof(TriState), GetDefaultFontBold());
			_fontItalic = (TriState)LayoutReader.GetPropertyValue(info, "FontItalic", typeof(TriState), TriState.Empty);
			_fontName = (string)LayoutReader.GetPropertyValue(info, "FontName", typeof(string), "");
			_fontSize = (float)LayoutReader.GetPropertyValue(info, "FontSize", typeof(float), 0f);
			_fontStrikeout = (TriState)LayoutReader.GetPropertyValue(info, "FontStrikeout", typeof(TriState), TriState.Empty);
			_fontUnderline = (TriState)LayoutReader.GetPropertyValue(info, "FontUnderline", typeof(TriState), TriState.Empty);
			_foreColor = (Color)LayoutReader.GetPropertyValue(info, "ForeColor", typeof(Color), GetDefaultForeColor());
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "ForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			ReleaseResolvedFont();
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (_predefinedStyle != PredefinedStyle.None)
			{
				info.AddValue("PredefinedStyle", _predefinedStyle);
			}
			if (_alpha != -1)
			{
				info.AddValue("Alpha", _alpha);
			}
			if (!_backColor.IsEmpty)
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
			if (_fontBold != GetDefaultFontBold())
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
			if (!_foreColor.IsEmpty)
			{
				info.AddValue("ForeColor", _foreColor);
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("ForeColorAlphaMode", _foreColorAlphaMode);
			}
		}

		internal TaskPaneFormatStyle(ITaskPaneOwner owner)
		{
			_owner = owner;
			Reset();
		}

		internal TaskPaneFormatStyle(PredefinedStyle predefinedStyle)
		{
			Reset();
			_predefinedStyle = predefinedStyle;
		}

		internal TaskPaneFormatStyle(ITaskPaneOwner owner, PredefinedStyle predefinedStyle)
		{
			_owner = owner;
			_predefinedStyle = predefinedStyle;
			Reset();
		}

		internal TaskPaneFormatStyle()
		{
			Reset();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_backColor = Color.Empty;
			_backColorAlphaMode = AlphaMode.Empty;
			_backColorGradient = Color.Empty;
			_backgroundGradientMode = BackgroundGradientMode.Empty;
			_backColorGradientAlphaMode = AlphaMode.Empty;
			_blendGradient = -1f;
			_font = null;
			ReleaseResolvedFont();
			_fontBold = GetDefaultFontBold();
			_fontItalic = TriState.Empty;
			_fontName = "";
			_fontSize = 0f;
			_fontStrikeout = TriState.Empty;
			_fontUnderline = TriState.Empty;
			_foreColor = Color.Empty;
			_foreColorAlphaMode = AlphaMode.Empty;
			_blend = -1f;
			_alpha = -1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return true;
			}
			if (_backColorGradientAlphaMode != AlphaMode.Empty)
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
			if (!_backColor.IsEmpty)
			{
				return true;
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return true;
			}
			if (_font != null)
			{
				return true;
			}
			if (_fontBold != GetDefaultFontBold())
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
			if (FontUnderline != TriState.Empty)
			{
				return true;
			}
			if (!_foreColor.IsEmpty)
			{
				return true;
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return true;
			}
			if (_blend != -1f)
			{
				return true;
			}
			if (_alpha != -1)
			{
				return true;
			}
			return false;
		}

		internal bool IsDefault()
		{
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return false;
			}
			if (_backColorGradientAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_blendGradient != -1f)
			{
				return false;
			}
			if (_backColorGradient != Color.Empty)
			{
				return false;
			}
			if (!_backColor.IsEmpty)
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
			if (!_foreColor.IsEmpty)
			{
				return false;
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_blend != -1f)
			{
				return false;
			}
			return true;
		}

		public TaskPaneFormatStyle(TaskPaneFormatStyle baseStyle)
		{
			_alpha = baseStyle._alpha;
			_backColor = baseStyle.BackColor;
			_backgroundGradientMode = baseStyle.BackgroundGradientMode;
			_backColorGradient = baseStyle.BackColorGradient;
			_blendGradient = baseStyle._blendGradient;
			_backColorGradientAlphaMode = baseStyle._backColorGradientAlphaMode;
			_backColorAlphaMode = baseStyle._backColorAlphaMode;
			CopyFontSettings(baseStyle);
			_owner = baseStyle._owner;
			_invalidatesOwner = false;
			_foreColor = baseStyle.ForeColor;
			_foreColorAlphaMode = baseStyle._foreColorAlphaMode;
			_blend = baseStyle._blend;
		}

		internal void SetAlphaSettings(int alpha, AlphaMode backColorAlphaMode, AlphaMode foreColorAlphaMode)
		{
			_alpha = alpha;
			_backColorAlphaMode = backColorAlphaMode;
			_foreColorAlphaMode = foreColorAlphaMode;
		}

		internal TaskPaneFormatStyle MergeWith(TaskPaneFormatStyle overrideStyle)
		{
			if (overrideStyle == null || (overrideStyle._predefinedStyle == PredefinedStyle.None && overrideStyle.IsDefault()))
			{
				return this;
			}
			TaskPaneFormatStyle taskPaneFormatStyle = new TaskPaneFormatStyle(this);
			if (overrideStyle._alpha != -1)
			{
				taskPaneFormatStyle._alpha = overrideStyle._alpha;
			}
			if (overrideStyle.BackgroundGradientMode != BackgroundGradientMode.Empty)
			{
				taskPaneFormatStyle._backgroundGradientMode = overrideStyle.BackgroundGradientMode;
			}
			if (overrideStyle._backColorGradientAlphaMode != AlphaMode.Empty)
			{
				taskPaneFormatStyle._backColorGradientAlphaMode = overrideStyle._backColorGradientAlphaMode;
			}
			if (overrideStyle._blendGradient != -1f)
			{
				taskPaneFormatStyle._blendGradient = overrideStyle._blendGradient;
			}
			if (overrideStyle.BackColorGradient != Color.Empty)
			{
				taskPaneFormatStyle._backColorGradient = overrideStyle.BackColorGradient;
			}
			if (!overrideStyle.BackColor.IsEmpty)
			{
				taskPaneFormatStyle._backColor = overrideStyle.BackColor;
			}
			if (overrideStyle._backColorAlphaMode != AlphaMode.Empty)
			{
				taskPaneFormatStyle._backColorAlphaMode = overrideStyle._backColorAlphaMode;
			}
			if (!overrideStyle.ForeColor.IsEmpty)
			{
				taskPaneFormatStyle._foreColor = overrideStyle.ForeColor;
			}
			if (overrideStyle._foreColorAlphaMode != AlphaMode.Empty)
			{
				taskPaneFormatStyle._foreColorAlphaMode = overrideStyle._foreColorAlphaMode;
			}
			if (overrideStyle._owner != null)
			{
				taskPaneFormatStyle._owner = overrideStyle._owner;
			}
			if (overrideStyle._blend != -1f)
			{
				taskPaneFormatStyle._blend = overrideStyle._blend;
			}
			if (overrideStyle._font != null)
			{
				taskPaneFormatStyle.CopyFontSettings(overrideStyle);
			}
			else
			{
				if (overrideStyle._fontBold != TriState.Empty)
				{
					taskPaneFormatStyle._fontBold = overrideStyle._fontBold;
				}
				if (overrideStyle._fontItalic != TriState.Empty)
				{
					taskPaneFormatStyle._fontItalic = overrideStyle._fontItalic;
				}
				if (overrideStyle._fontName != "")
				{
					taskPaneFormatStyle._fontName = overrideStyle._fontName;
				}
				if (overrideStyle._fontSize != 0f)
				{
					taskPaneFormatStyle._fontSize = overrideStyle._fontSize;
				}
				if (overrideStyle._fontStrikeout != TriState.Empty)
				{
					taskPaneFormatStyle._fontStrikeout = overrideStyle._fontStrikeout;
				}
				if (overrideStyle._fontUnderline != TriState.Empty)
				{
					taskPaneFormatStyle._fontUnderline = overrideStyle._fontUnderline;
				}
			}
			return taskPaneFormatStyle;
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

		internal void SetPredefinedStyle(PredefinedStyle predefinedStyle)
		{
			_predefinedStyle = predefinedStyle;
		}

		private Color GetDefaultBackColorGradient()
		{
			return _defaultBackColorGradient;
		}

		private Color GetDefaultBackColor()
		{
			return _defaultBackColor;
		}

		private TriState GetDefaultFontBold()
		{
			if (_predefinedStyle == PredefinedStyle.GroupHeader || _predefinedStyle == PredefinedStyle.SpecialGroupHeader)
			{
				return TriState.True;
			}
			return TriState.Empty;
		}

		private Color GetDefaultForeColor()
		{
			return _defaultForeColor;
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

		// Only _resolvedFont belongs to this style. The font set by the user and the base font are never disposed here.
		internal Font ResolveFont(Font baseFont)
		{
			Font source = _font ?? baseFont;
			if (!HasFontOverrides())
			{
				return source;
			}
			if (_resolvedFont != null && _resolvedFontSource == source)
			{
				return _resolvedFont;
			}
			ReleaseResolvedFont();
			try
			{
				_resolvedFont = CreateFontWithOverrides(source);
				_resolvedFontSource = source;
				return _resolvedFont;
			}
			catch (Exception failure) when (ControlFailure.IsRecoverable(failure))
			{
				return source;
			}
		}

		internal void SetFontUnderline(TriState fontUnderline)
		{
			_fontUnderline = fontUnderline;
			ReleaseResolvedFont();
		}

		// A font set by the user is drawn as given unless FontBold differs from the default of the style.
		private TriState GetBoldOverride()
		{
			if (_font != null && _fontBold == GetDefaultFontBold())
			{
				return TriState.Empty;
			}
			return _fontBold;
		}

		private bool HasFontOverrides()
		{
			return GetBoldOverride() != TriState.Empty || _fontItalic != TriState.Empty || !string.IsNullOrEmpty(_fontName) || _fontSize != 0f || _fontStrikeout != TriState.Empty || _fontUnderline != TriState.Empty;
		}

		// A copy has no predefined style, so the bold value is copied as the override the source really applies.
		private void CopyFontSettings(TaskPaneFormatStyle source)
		{
			_font = source._font;
			_fontBold = source._font != null ? source.GetBoldOverride() : source._fontBold;
			_fontItalic = source._fontItalic;
			_fontName = source._fontName;
			_fontSize = source._fontSize;
			_fontStrikeout = source._fontStrikeout;
			_fontUnderline = source._fontUnderline;
			ReleaseResolvedFont();
		}

		private void ReleaseResolvedFont()
		{
			_resolvedFont?.Dispose();
			_resolvedFont = null;
			_resolvedFontSource = null;
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
			TriState fontBold = GetBoldOverride();
			if (fontBold == TriState.Empty ? source.Bold : fontBold != TriState.False)
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeFontBold()
		{
			return _fontBold != GetDefaultFontBold();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetFontBold()
		{
			FontBold = GetDefaultFontBold();
		}

		private void InvalidateOwnerLayout()
		{
			if (TaskPane != null && _invalidatesOwner)
			{
				TaskPane.InvalidateLayout();
			}
		}

		private void RepaintOwner()
		{
			if (TaskPane != null && _invalidatesOwner)
			{
				TaskPane.Invalidate();
			}
		}

		internal void DisposeDrawingObjects()
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
			if (_backPen != null)
			{
				_backPen.Dispose();
				_backPen = null;
			}
			ReleaseResolvedFont();
		}

		internal Brush CreateBackBrush(Rectangle bounds)
		{
			if (HasGradient() && bounds.Width > 0 && bounds.Height > 0)
			{
				bounds.X--;
				bounds.Width++;
				Color startColor = BlendColors(GetBlendColor(), BackColor, GetEffectiveBlend(), _alpha, _backColorAlphaMode);
				Color endColor = BlendColors(GetBlendColor(), BackColorGradient, GetGradientBlend(), _alpha, _backColorGradientAlphaMode);
				return TaskPanePaintHelper.CreateGradientBrush(startColor, endColor, bounds, BackgroundGradientMode);
			}
			if (GetEffectiveBlend() != 0f && !BackColor.IsEmpty)
			{
				return new SolidBrush(BlendColors(BackColor, GetBlendColor(), GetEffectiveBlend(), _alpha, _backColorAlphaMode));
			}
			if (BackColor.IsEmpty)
			{
				return null;
			}
			return new SolidBrush(Color.FromArgb(GetEffectiveAlpha(_alpha, _backColorAlphaMode), BackColor));
		}

		internal Brush GetBackBrush(Rectangle bounds)
		{
			if (_backBrush == null || HasGradient())
			{
				if (_backBrush != null)
				{
					_backBrush.Dispose();
					_backBrush = null;
				}
				return CreateBackBrush(bounds);
			}
			return _backBrush;
		}

		internal int GetEffectiveAlpha(int alpha, AlphaMode alphaMode)
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
			if (primaryColor == Color.Empty)
			{
				return secondaryColor;
			}
			if (secondaryColor == Color.Empty)
			{
				return primaryColor;
			}
			float redValue = (float)(int)primaryColor.R * floatValue + (float)(int)secondaryColor.R * (1f - floatValue);
			int red = (int)Math.Round(redValue, 0);
			float greenValue = (float)(int)primaryColor.G * floatValue + (float)(int)secondaryColor.G * (1f - floatValue);
			int green = (int)Math.Round(greenValue, 0);
			float blueValue = (float)(int)primaryColor.B * floatValue + (float)(int)secondaryColor.B * (1f - floatValue);
			int blue = (int)Math.Round(blueValue, 0);
			if (red > 255)
			{
				red = 255;
			}
			if (green > 255)
			{
				green = 255;
			}
			if (blue > 255)
			{
				blue = 255;
			}
			if (alphaMode == AlphaMode.Empty)
			{
				alphaMode = AlphaMode.Opaque;
			}
			switch (alphaMode)
			{
			case AlphaMode.Transparent:
				return Color.Transparent;
			case AlphaMode.UseAlpha:
				if (_alpha == -1 || _alpha > 255)
				{
					_alpha = 255;
				}
				return Color.FromArgb(_alpha, red, green, blue);
			default:
				return Color.FromArgb(red, green, blue);
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

		internal float GetGradientBlend()
		{
			if (_blendGradient == -1f)
			{
				if (TaskPane != null)
				{
					return TaskPane.DefaultBlend;
				}
				return 0f;
			}
			return _blendGradient;
		}

		internal Brush GetForeBrush()
		{
			if (_foreBrush == null)
			{
				_foreBrush = TaskPanePaintHelper.CreateSolidBrush(ForeColor, _alpha, _foreColorAlphaMode);
			}
			return _foreBrush;
		}

		internal Brush CreateForeBrush(Color fallbackColor)
		{
			if (ForeColor.IsEmpty)
			{
				return TaskPanePaintHelper.CreateSolidBrush(fallbackColor, _alpha, _foreColorAlphaMode);
			}
			return TaskPanePaintHelper.CreateSolidBrush(ForeColor, _alpha, _foreColorAlphaMode);
		}

		internal float GetEffectiveBlend()
		{
			if (_blend == -1f)
			{
				if (TaskPane != null)
				{
					return TaskPane.DefaultBlend;
				}
				return 0f;
			}
			return _blend;
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

		internal Color GetBlendColor()
		{
			if (TaskPane != null)
			{
				return TaskPane.BlendColor;
			}
			return SystemColors.Window;
		}

		internal void ApplyThemedColors(Color back, Color fore)
		{
			_defaultBackColor = back;
			_defaultBackColorGradient = Color.Empty;
			_defaultBackgroundGradientMode = back.IsEmpty ? BackgroundGradientMode.Empty : BackgroundGradientMode.Solid;
			_defaultForeColor = fore;
			_fontUnderline = TriState.Empty;
		}

	}
}
