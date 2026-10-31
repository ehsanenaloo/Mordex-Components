using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.Timeline.Internal;

namespace Mordex.Windows.Timeline
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public partial class TimelineFormatStyle : IMordexXmlLayoutsSupport
	{
		internal string _text = "";

		private Image _backgroundImage;

		private int _alpha;

		private AlphaMode _backColorAlphaMode;

		private AlphaMode _foreColorAlphaMode;

		private BackgroundImageDrawMode _backgroundImageDrawMode;

		internal ITimelineOwner _owner;

		private float _floatValue = 1f;

		private Color _backColor;

		private Color _backColorGradient;

		private BackgroundGradientMode _backgroundGradientMode;

		private Color _foreColor;

		private Font _customFont;

		internal TriState _fontBold;

		private TriState _fontItalic;

		private TriState _fontUnderline;

		private TriState _fontStrikeout;

		private bool _isOwnerNotificationEnabled = true;

		private Brush _backBrush;

		private Brush _foreColorBrush;

		private PredefinedStyle _predefinedStyle;

		private float _blend = -1f;

		private float _blendGradient = -1f;

		private Color _defaultForeColor = Color.Empty;

		private Color _defaultBackColor = Color.Empty;

		private Color _defaultBackColorGradient = Color.Empty;

		private BackgroundGradientMode _defaultBackgroundGradientMode;

		string IMordexXmlLayoutsSupport.Identifier => null;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[DefaultValue(null)]
		public Image BackgroundImage
		{
			get
			{
				return _backgroundImage;
			}
			set
			{
				_backgroundImage = value;
				UpdateLayout();
			}
		}

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
					UpdateLayout();
				}
			}
		}

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
					UpdateLayout();
				}
			}
		}

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
					UpdateLayout();
				}
			}
		}

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
					UpdateLayout();
				}
			}
		}

		[Description("The background color of this TimelineFormatStyle.")]
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
					UpdateLayout();
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
					UpdateLayout();
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
					UpdateLayout();
				}
			}
		}

		[Description("Specifies the foreground color of the TimelineFormatStyle.")]
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
					UpdateLayout();
				}
			}
		}

		[Description("Indicates whether the Font object used is bold.")]
		[DefaultValue(TriState.Empty)]
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
					if (_customFont != null)
					{
						ApplyFontStyle();
					}
					UpdateLayout();
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
					if (_customFont != null)
					{
						ApplyFontStyle();
					}
					UpdateLayout();
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
					if (_customFont != null)
					{
						ApplyFontStyle();
					}
					UpdateLayout();
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
					if (_customFont != null)
					{
						ApplyFontStyle();
					}
					UpdateLayout();
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
					UpdateLayout();
				}
			}
		}

		[DefaultValue(-1f)]
		[Description("Indicates the percentage of the BlendColor to blend with the BackColorGradient.")]
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
					UpdateLayout();
				}
			}
		}

		void IMordexXmlLayoutsSupport.Deserialize(LayoutReader info)
		{
			_predefinedStyle = (PredefinedStyle)LayoutReader.GetPropertyValue(info, "PredefinedStyle", typeof(PredefinedStyle), PredefinedStyle.None);
			_backColor = (Color)LayoutReader.GetPropertyValue(info, "BackColor", typeof(Color), Color.Empty);
			_fontBold = (TriState)LayoutReader.GetPropertyValue(info, "FontBold", typeof(TriState), TriState.Empty);
			_fontItalic = (TriState)LayoutReader.GetPropertyValue(info, "FontItalic", typeof(TriState), TriState.Empty);
			_fontStrikeout = (TriState)LayoutReader.GetPropertyValue(info, "FontStrikeout", typeof(TriState), TriState.Empty);
			_fontUnderline = (TriState)LayoutReader.GetPropertyValue(info, "FontUnderline", typeof(TriState), TriState.Empty);
			_foreColor = (Color)LayoutReader.GetPropertyValue(info, "ForeColor", typeof(Color), Color.Empty);
			_blend = (float)LayoutReader.GetPropertyValue(info, "Blend", typeof(float), -1f);
			_blendGradient = (float)LayoutReader.GetPropertyValue(info, "BlendGradient", typeof(float), -1f);
			_backColorGradient = (Color)LayoutReader.GetPropertyValue(info, "BackColorGradient", typeof(Color), Color.Empty);
			_backgroundGradientMode = (BackgroundGradientMode)LayoutReader.GetPropertyValue(info, "BackgroundGradientMode", typeof(BackgroundGradientMode), BackgroundGradientMode.Empty);
			_backColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "BackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), -1);
			_backgroundImage = (Image)LayoutReader.GetPropertyValue(info, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (BackgroundImageDrawMode)LayoutReader.GetPropertyValue(info, "BackgroundImageDrawMode", typeof(BackgroundImageDrawMode), BackgroundImageDrawMode.Empty);
			_customFont = (Font)LayoutReader.GetPropertyValue(info, "Font", typeof(Font), null);
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "ForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_backgroundImage = (Image)LayoutReader.GetPropertyValue(info, "BackgroundImage", typeof(Image), null);
			_backgroundImageDrawMode = (BackgroundImageDrawMode)LayoutReader.GetPropertyValue(info, "BackgroundImageDrawMode", typeof(BackgroundImageDrawMode), BackgroundImageDrawMode.Empty);
		}

		void IMordexXmlLayoutsSupport.Serialize(LayoutWriter info)
		{
			if (!_backColor.IsEmpty)
			{
				info.AddValue("BackColor", _backColor);
			}
			if (_fontBold != TriState.Empty)
			{
				info.AddValue("FontBold", _fontBold);
			}
			if (_fontItalic != TriState.Empty)
			{
				info.AddValue("FontItalic", _fontItalic);
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
			if (_predefinedStyle != PredefinedStyle.None)
			{
				info.AddValue("PredefinedStyle", _predefinedStyle);
			}
			if (_blend != -1f)
			{
				info.AddValue("Blend", _blend);
			}
			if (_blendGradient != -1f)
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
			if (_alpha != -1)
			{
				info.AddValue("Alpha", _alpha);
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("ForeColorAlphaMode", _foreColorAlphaMode);
			}
			if (_backgroundImage != null)
			{
				info.AddValue("BackgroundImage", _backgroundImage);
			}
			if (_backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
			{
				info.AddValue("BackgroundImageDrawMode", _backgroundImageDrawMode);
			}
			if (_customFont != null)
			{
				info.AddValue("Font", _customFont);
			}
		}

		public TimelineFormatStyle()
			: this(PredefinedStyle.None)
		{
		}

		internal TimelineFormatStyle(PredefinedStyle predefinedStyle)
		{
			_predefinedStyle = predefinedStyle;
			Reset();
		}

		internal TimelineFormatStyle(ITimelineOwner owner, PredefinedStyle predefinedStyle)
		{
			_owner = owner;
			_predefinedStyle = predefinedStyle;
			Reset();
		}

		internal MxTimeline GetTimeline()
		{
			if (_owner == null)
			{
				return null;
			}
			return _owner.Timeline;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_backColor = Color.Empty;
			_customFont = null;
			_fontBold = TriState.Empty;
			_fontItalic = TriState.Empty;
			_fontStrikeout = TriState.Empty;
			_fontUnderline = TriState.Empty;
			_foreColor = Color.Empty;
			_floatValue = 1f;
			_blend = -1f;
			_blendGradient = -1f;
			_backColorGradient = Color.Empty;
			_backgroundGradientMode = BackgroundGradientMode.Empty;
			_backColorAlphaMode = AlphaMode.Empty;
			_alpha = -1;
			_foreColorAlphaMode = AlphaMode.Empty;
			_backgroundImage = null;
			_backgroundImageDrawMode = BackgroundImageDrawMode.Empty;
			ApplyThemedStyle();
			UpdateLayout();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (!_backColor.IsEmpty)
			{
				return true;
			}
			if (_customFont != null)
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
			if (_floatValue != 1f)
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
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return true;
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return true;
			}
			if (_alpha != -1)
			{
				return true;
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return true;
			}
			if (_backgroundImage != null)
			{
				return true;
			}
			if (_backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
			{
				return true;
			}
			return false;
		}

		internal bool HasNoFontOverrides()
		{
			if (_fontBold != TriState.Empty)
			{
				return false;
			}
			if (_fontItalic != TriState.Empty)
			{
				return false;
			}
			if (_fontUnderline != TriState.Empty)
			{
				return false;
			}
			if (_fontStrikeout != TriState.Empty)
			{
				return false;
			}
			return true;
		}

		internal bool HasOnlyDefaultValues()
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
			if (_backColorGradient != Color.Empty)
			{
				return false;
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
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
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_alpha != -1)
			{
				return false;
			}
			return true;
		}

		public TimelineFormatStyle(TimelineFormatStyle baseStyle)
		{
			_predefinedStyle = PredefinedStyle.None;
			_backColor = baseStyle.BackColor;
			if (baseStyle._customFont != null)
			{
				_customFont = (Font)baseStyle._customFont.Clone();
				_fontBold = TriState.Empty;
				_fontItalic = TriState.Empty;
				_fontStrikeout = TriState.Empty;
				_fontUnderline = TriState.Empty;
			}
			else
			{
				_customFont = null;
				_fontBold = baseStyle._fontBold;
				_fontItalic = baseStyle._fontItalic;
				_fontStrikeout = baseStyle._fontStrikeout;
				_fontUnderline = baseStyle._fontUnderline;
			}
			_foreColor = baseStyle.ForeColor;
			_floatValue = baseStyle._floatValue;
			_blend = baseStyle._blend;
			_blendGradient = baseStyle._blendGradient;
			_backColorGradient = baseStyle.BackColorGradient;
			_backgroundGradientMode = baseStyle.BackgroundGradientMode;
			_owner = baseStyle._owner;
			_isOwnerNotificationEnabled = false;
			_backColorAlphaMode = baseStyle._backColorAlphaMode;
			_alpha = baseStyle._alpha;
			_foreColorAlphaMode = baseStyle._foreColorAlphaMode;
			_backgroundImage = baseStyle._backgroundImage;
			_backgroundImageDrawMode = baseStyle._backgroundImageDrawMode;
			_alpha = baseStyle._alpha;
			_backColorAlphaMode = baseStyle._backColorAlphaMode;
			_foreColorAlphaMode = baseStyle._foreColorAlphaMode;
		}

		internal TimelineFormatStyle(TimelineFormatStyle baseStyle, int defaultAlphaLevel, AlphaMode defaultBackColorAlphaMode, AlphaMode defaultForeColorAlphaMode)
		{
			_predefinedStyle = PredefinedStyle.None;
			_backColor = baseStyle.BackColor;
			if (baseStyle._customFont != null)
			{
				_customFont = (Font)baseStyle._customFont.Clone();
				_fontBold = TriState.Empty;
				_fontItalic = TriState.Empty;
				_fontStrikeout = TriState.Empty;
				_fontUnderline = TriState.Empty;
			}
			else
			{
				_customFont = null;
				_fontBold = baseStyle._fontBold;
				_fontItalic = baseStyle._fontItalic;
				_fontStrikeout = baseStyle._fontStrikeout;
				_fontUnderline = baseStyle._fontUnderline;
			}
			_foreColor = baseStyle.ForeColor;
			_floatValue = baseStyle._floatValue;
			_blend = baseStyle._blend;
			_blendGradient = baseStyle._blendGradient;
			_backColorGradient = baseStyle.BackColorGradient;
			_backgroundGradientMode = baseStyle.BackgroundGradientMode;
			_owner = baseStyle._owner;
			_isOwnerNotificationEnabled = false;
			_backColorAlphaMode = baseStyle._backColorAlphaMode;
			_alpha = baseStyle._alpha;
			_foreColorAlphaMode = baseStyle._foreColorAlphaMode;
			_backgroundImage = baseStyle._backgroundImage;
			_backgroundImageDrawMode = baseStyle._backgroundImageDrawMode;
			if (_alpha == -1)
			{
				_alpha = defaultAlphaLevel;
			}
			if (_backColorAlphaMode == AlphaMode.Empty)
			{
				_backColorAlphaMode = defaultBackColorAlphaMode;
			}
			if (_foreColorAlphaMode == AlphaMode.Empty)
			{
				_foreColorAlphaMode = defaultForeColorAlphaMode;
			}
		}

		internal TimelineFormatStyle Clone()
		{
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle();
			timelineFormatStyle._predefinedStyle = PredefinedStyle.None;
			timelineFormatStyle._alpha = Alpha;
			timelineFormatStyle._backColorAlphaMode = BackColorAlphaMode;
			timelineFormatStyle._backColor = BackColor;
			if (_customFont != null)
			{
				timelineFormatStyle._customFont = (Font)_customFont.Clone();
				timelineFormatStyle._fontBold = TriState.Empty;
				timelineFormatStyle._fontItalic = TriState.Empty;
				timelineFormatStyle._fontStrikeout = TriState.Empty;
				timelineFormatStyle._fontUnderline = TriState.Empty;
			}
			else
			{
				timelineFormatStyle._fontBold = _fontBold;
				timelineFormatStyle._fontItalic = _fontItalic;
				timelineFormatStyle._fontStrikeout = _fontStrikeout;
				timelineFormatStyle._fontUnderline = _fontUnderline;
			}
			timelineFormatStyle._foreColor = ForeColor;
			timelineFormatStyle._floatValue = _floatValue;
			timelineFormatStyle._blend = _blend;
			timelineFormatStyle._blendGradient = _blendGradient;
			timelineFormatStyle._backColorGradient = BackColorGradient;
			timelineFormatStyle._backgroundGradientMode = BackgroundGradientMode;
			timelineFormatStyle._backgroundImage = BackgroundImage;
			timelineFormatStyle._backgroundImageDrawMode = BackgroundImageDrawMode;
			return timelineFormatStyle;
		}

		internal static TimelineFormatStyle CloneForPainting(TimelineFormatStyle baseStyle)
		{
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(baseStyle, -1, AlphaMode.Empty, AlphaMode.Empty);
			timelineFormatStyle._blend = baseStyle.GetBlend();
			timelineFormatStyle._blendGradient = baseStyle.GetGradientBlend();
			return timelineFormatStyle;
		}

		internal TimelineFormatStyle MergeWith(TimelineFormatStyle timelineFormatStyleArgument)
		{
			if (timelineFormatStyleArgument == null || (timelineFormatStyleArgument._predefinedStyle == PredefinedStyle.None && timelineFormatStyleArgument.HasOnlyDefaultValues()))
			{
				return this;
			}
			TimelineFormatStyle timelineFormatStyle = new TimelineFormatStyle(this, -1, AlphaMode.Empty, AlphaMode.Empty);
			if (timelineFormatStyleArgument._floatValue != 1f)
			{
				timelineFormatStyle._floatValue = timelineFormatStyleArgument._floatValue;
			}
			if (!timelineFormatStyleArgument.BackColor.IsEmpty)
			{
				timelineFormatStyle._backColor = timelineFormatStyleArgument.BackColor;
			}
			if (!timelineFormatStyleArgument.ForeColor.IsEmpty)
			{
				timelineFormatStyle._foreColor = timelineFormatStyleArgument.ForeColor;
			}
			if (timelineFormatStyleArgument._blend != -1f)
			{
				timelineFormatStyle._blend = timelineFormatStyleArgument._blend;
			}
			if (timelineFormatStyleArgument._blendGradient != -1f)
			{
				timelineFormatStyle._blendGradient = timelineFormatStyleArgument._blendGradient;
			}
			if (timelineFormatStyleArgument.BackColorGradient != Color.Empty)
			{
				timelineFormatStyle._backColorGradient = timelineFormatStyleArgument.BackColorGradient;
			}
			if (timelineFormatStyleArgument.BackgroundGradientMode != BackgroundGradientMode.Empty)
			{
				timelineFormatStyle._backgroundGradientMode = timelineFormatStyleArgument.BackgroundGradientMode;
			}
			if (timelineFormatStyleArgument._alpha != -1)
			{
				timelineFormatStyle._alpha = timelineFormatStyleArgument._alpha;
			}
			if (timelineFormatStyleArgument._backColorAlphaMode != AlphaMode.Empty)
			{
				timelineFormatStyle._backColorAlphaMode = timelineFormatStyleArgument._backColorAlphaMode;
			}
			if (timelineFormatStyleArgument._foreColorAlphaMode != AlphaMode.Empty)
			{
				timelineFormatStyle._foreColorAlphaMode = timelineFormatStyleArgument._foreColorAlphaMode;
			}
			if (timelineFormatStyleArgument._backgroundImage != null)
			{
				timelineFormatStyle._backgroundImage = timelineFormatStyleArgument._backgroundImage;
			}
			if (timelineFormatStyleArgument._backgroundImageDrawMode != BackgroundImageDrawMode.Empty)
			{
				timelineFormatStyle._backgroundImageDrawMode = timelineFormatStyleArgument._backgroundImageDrawMode;
			}
			if (timelineFormatStyleArgument._customFont != null)
			{
				timelineFormatStyle._customFont = timelineFormatStyleArgument._customFont;
			}
			else
			{
				if (timelineFormatStyleArgument._fontBold != TriState.Empty)
				{
					timelineFormatStyle._fontBold = timelineFormatStyleArgument._fontBold;
				}
				if (timelineFormatStyleArgument._fontItalic != TriState.Empty)
				{
					timelineFormatStyle._fontItalic = timelineFormatStyleArgument._fontItalic;
				}
				if (timelineFormatStyleArgument._fontStrikeout != TriState.Empty)
				{
					timelineFormatStyle._fontStrikeout = timelineFormatStyleArgument._fontStrikeout;
				}
				if (timelineFormatStyleArgument._fontUnderline != TriState.Empty)
				{
					timelineFormatStyle._fontUnderline = timelineFormatStyleArgument._fontUnderline;
				}
				if (timelineFormatStyle._customFont != null)
				{
					timelineFormatStyle.ApplyFontStyle();
				}
			}
			return timelineFormatStyle;
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
			return !_backgroundGradientMode.Equals(BackgroundGradientMode.Empty);
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
			if (_customFont != null)
			{
				return _customFont;
			}
			if (_fontBold == TriState.Empty && _fontItalic == TriState.Empty && _fontStrikeout == TriState.Empty && _fontUnderline == TriState.Empty)
			{
				if (_floatValue == 1f)
				{
					return font;
				}
				_customFont = (Font)font.Clone();
				ApplyFontStyle();
				return _customFont;
			}
			_customFont = (Font)font.Clone();
			ApplyFontStyle();
			return _customFont;
		}

		internal void ApplyFontStyle()
		{
			if (_customFont != null)
			{
				string name = _customFont.Name;
				float fontSize = _customFont.SizeInPoints;
				FontStyle fontStyle = FontStyle.Regular;
				bool isBold = ((_fontBold == TriState.Empty) ? _customFont.Bold : ((_fontBold != TriState.False) ? true : false));
				bool isItalic = ((_fontItalic == TriState.Empty) ? _customFont.Italic : ((_fontItalic != TriState.False) ? true : false));
				bool isStrikeout = ((_fontStrikeout == TriState.Empty) ? _customFont.Strikeout : ((_fontStrikeout != TriState.False) ? true : false));
				bool isUnderline = ((_fontUnderline == TriState.Empty) ? _customFont.Underline : ((_fontUnderline != TriState.False) ? true : false));
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
				if (_floatValue != 1f)
				{
					fontSize *= _floatValue;
				}
				_customFont.Dispose();
				_customFont = null;
				_customFont = new Font(name, fontSize, fontStyle);
			}
		}

		private Brush CreateGradientBrush(Color baseColor, Color baseColorArgument, Rectangle rect, BackgroundGradientMode backgroundGradientMode, int alpha, AlphaMode alphaMode)
		{
			if (alphaMode == AlphaMode.Empty)
			{
				alphaMode = AlphaMode.Opaque;
			}
			switch (alphaMode)
			{
			case AlphaMode.Transparent:
				return null;
			case AlphaMode.UseAlpha:
				if (alpha == -1)
				{
					alpha = 255;
				}
				baseColor = Color.FromArgb(alpha, baseColor);
				baseColorArgument = Color.FromArgb(alpha, baseColorArgument);
				break;
			}
			switch (backgroundGradientMode)
			{
			case BackgroundGradientMode.Horizontal:
				return new LinearGradientBrush(rect, baseColor, baseColorArgument, 0f);
			case BackgroundGradientMode.Vertical:
				return new LinearGradientBrush(rect, baseColor, baseColorArgument, 90f);
			case BackgroundGradientMode.Diagonal:
				return new LinearGradientBrush(rect, baseColor, baseColorArgument, 45f);
			case BackgroundGradientMode.DiagonalBackwards:
				return new LinearGradientBrush(rect, baseColor, baseColorArgument, 225f);
			default:
				return new SolidBrush(baseColor);
			}
		}

		private Brush CreateSolidBrush(Color baseColor, int alpha, AlphaMode alphaMode)
		{
			if (baseColor == Color.Empty)
			{
				return null;
			}
			if (alphaMode == AlphaMode.Empty)
			{
				alphaMode = AlphaMode.Opaque;
			}
			switch (alphaMode)
			{
			case AlphaMode.Transparent:
				return null;
			case AlphaMode.UseAlpha:
				if (alpha == -1)
				{
					alpha = 255;
				}
				return new SolidBrush(Color.FromArgb(alpha, baseColor));
			default:
				return new SolidBrush(baseColor);
			}
		}

		internal void UpdateLayout()
		{
			if (_isOwnerNotificationEnabled && GetTimeline() != null)
			{
				GetTimeline().Invalidate();
			}
		}

		internal void DisposeResources()
		{
			if (_backBrush != null)
			{
				_backBrush.Dispose();
				_backBrush = null;
			}
			if (_foreColorBrush != null)
			{
				_foreColorBrush.Dispose();
				_foreColorBrush = null;
			}
			if (_customFont != null)
			{
				_customFont.Dispose();
				_customFont = null;
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

		internal Color ResolveForeColor(Color colorArgument)
		{
			Color color = ForeColor;
			if (color == Color.Empty)
			{
				color = colorArgument;
			}
			if (ForeColorAlphaMode == AlphaMode.Transparent)
			{
				return Color.Empty;
			}
			if (ForeColorAlphaMode == AlphaMode.UseAlpha)
			{
				if (_alpha == -1)
				{
					_alpha = 255;
				}
				return Color.FromArgb(_alpha, color);
			}
			return color;
		}

		internal Brush GetBackBrush(Rectangle rectangle)
		{
			if (_backBrush == null || HasGradient())
			{
				if (_backBrush != null)
				{
					_backBrush.Dispose();
					_backBrush = null;
				}
				if (HasGradient())
				{
					rectangle.X--;
					rectangle.Width++;
					rectangle.Y--;
					rectangle.Height++;
					if (rectangle.Width > 0 && rectangle.Height > 0)
					{
						_backBrush = CreateGradientBrush(MixColors(GetBlendColor(), BackColor, GetBlend()), MixColors(GetBlendColor(), BackColorGradient, GetGradientBlend()), rectangle, BackgroundGradientMode, _alpha, _backColorAlphaMode);
					}
				}
				else if (GetBlend() != 0f && !BackColor.IsEmpty)
				{
					_backBrush = CreateSolidBrush(MixColors(GetBlendColor(), BackColor, GetBlend()), _alpha, _backColorAlphaMode);
				}
				else
				{
					_backBrush = CreateSolidBrush(BackColor, _alpha, _backColorAlphaMode);
				}
			}
			return _backBrush;
		}

		private Color MixColors(Color color, Color colorArgument, float floatValue)
		{
			if (color == Color.Empty)
			{
				return colorArgument;
			}
			if (colorArgument == Color.Empty)
			{
				return color;
			}
			int red = (int)((float)(int)color.R * floatValue + (float)(int)colorArgument.R * (1f - floatValue));
			int green = (int)((float)(int)color.G * floatValue + (float)(int)colorArgument.G * (1f - floatValue));
			int blue = (int)((float)(int)color.B * floatValue + (float)(int)colorArgument.B * (1f - floatValue));
			return Color.FromArgb(red, green, blue);
		}

		internal Brush GetForeColorBrush(Color colorArgument)
		{
			Color color = ForeColor;
			if (color == Color.Empty)
			{
				color = colorArgument;
			}
			if (_foreColorBrush == null)
			{
				_foreColorBrush = CreateSolidBrush(color, _alpha, _foreColorAlphaMode);
			}
			return _foreColorBrush;
		}

		internal float GetBlend()
		{
			if (_blend == -1f)
			{
				if (GetTimeline() != null)
				{
					return GetTimeline().DefaultBlend;
				}
				return 0f;
			}
			return _blend;
		}

		internal float GetGradientBlend()
		{
			if (_blendGradient == -1f)
			{
				if (GetTimeline() != null)
				{
					return GetTimeline().DefaultBlend;
				}
				return 0f;
			}
			return _blendGradient;
		}

		internal Color GetBlendColor()
		{
			if (GetTimeline() != null)
			{
				return GetTimeline().BlendColor;
			}
			return SystemColors.Window;
		}

		internal void ApplyThemedStyle()
		{
			_defaultForeColor = Color.Empty;
			_defaultBackColorGradient = Color.Empty;
			_defaultBackgroundGradientMode = BackgroundGradientMode.Empty;
			ApplyThemedColors();
		}

		// Printed pages keep the system colors, so they do not depend on the theme.
		internal void ApplyPrintStyle()
		{
			_defaultForeColor = Color.Empty;
			_defaultBackColorGradient = Color.Empty;
			_defaultBackgroundGradientMode = BackgroundGradientMode.Empty;
			switch (_predefinedStyle)
			{
			case PredefinedStyle.Trailing:
				_defaultForeColor = SystemColors.GrayText;
				_defaultBackColor = Color.Empty;
				break;
			case PredefinedStyle.LowerScale:
				_defaultBackColor = SystemColors.Window;
				break;
			case PredefinedStyle.Selected:
				_defaultBackColor = TimelineUtilities.BlendColors(SystemColors.Highlight, SystemColors.Window, 0.5f);
				_defaultBackgroundGradientMode = BackgroundGradientMode.Solid;
				_defaultForeColor = SystemColors.HighlightText;
				break;
			case PredefinedStyle.SelectedInactive:
				_defaultBackColor = SystemColors.Control;
				_defaultBackgroundGradientMode = BackgroundGradientMode.Solid;
				_defaultForeColor = SystemColors.ControlText;
				break;
			case PredefinedStyle.Item:
				_defaultBackColor = SystemColors.Window;
				break;
			case PredefinedStyle.ItemBar:
				_defaultBackColor = Color.Blue;
				break;
			case PredefinedStyle.EstimatedItemBar:
				_defaultBackColor = Color.Yellow;
				break;
			case PredefinedStyle.GroupRow:
			case PredefinedStyle.RowHeader:
			case PredefinedStyle.Header:
			case PredefinedStyle.UpperScale:
				_defaultBackColor = SystemColors.Control;
				break;
			default:
				_defaultBackColor = Color.Empty;
				break;
			}
		}
	}
}
