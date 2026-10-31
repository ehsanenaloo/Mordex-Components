using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	[Localizable(true)]
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	public partial class SchedulerFormatStyle : IMordexXmlLayoutsSupport
	{
		internal ISchedulerFormatStyleOwner _owner;

		internal string _text = "";

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

		private Brush _cachedSolidBrush;

		private PredefinedStyle _predefinedStyle;

		private Color _themeBackColor = Color.Empty;

		private Color _themeBackColorGradient = Color.Empty;

		private Color _themeForeColor = Color.Empty;

		private BackgroundGradientMode _themeGradientMode;

		private float _blend = -1f;

		private float _blendGradient = -1f;

		private string _key;

		private int _alpha;

		private AlphaMode _backColorAlphaMode;

		private AlphaMode _foreColorAlphaMode;

		string IMordexXmlLayoutsSupport.Identifier => _key;

		bool IMordexXmlLayoutsSupport.IsCollection => false;

		string IMordexXmlLayoutsSupport.CollectionElementName => null;

		[Browsable(false)]
		public MxScheduler Scheduler
		{
			get
			{
				if (_owner == null)
				{
					return null;
				}
				return _owner.Scheduler;
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
			_key = (string)LayoutReader.GetPropertyValue(info, "Key", typeof(string), "");
			_backColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "BackColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
			_alpha = (int)LayoutReader.GetPropertyValue(info, "Alpha", typeof(int), -1);
			_foreColorAlphaMode = (AlphaMode)LayoutReader.GetPropertyValue(info, "ForeColorAlphaMode", typeof(AlphaMode), AlphaMode.Empty);
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
			if (!_backColorGradient.IsEmpty)
			{
				info.AddValue("BackColorGradient", _backColorGradient);
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				info.AddValue("BackgroundGradientMode", _backgroundGradientMode);
			}
			if (_key != "")
			{
				info.AddValue("Key", _key);
			}
			if (_alpha != -1)
			{
				info.AddValue("Alpha", _alpha);
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("ForeColorAlphaMode", _foreColorAlphaMode);
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				info.AddValue("BackColorAlphaMode", _backColorAlphaMode);
			}
		}

		public SchedulerFormatStyle()
			: this(PredefinedStyle.None)
		{
		}

		internal SchedulerFormatStyle(PredefinedStyle predefinedStyle)
		{
			_predefinedStyle = predefinedStyle;
			_isOwnerNotificationEnabled = false;
			Reset();
			_isOwnerNotificationEnabled = true;
		}

		internal SchedulerFormatStyle(ISchedulerFormatStyleOwner owner, PredefinedStyle predefinedStyle)
		{
			_owner = owner;
			_predefinedStyle = predefinedStyle;
			_isOwnerNotificationEnabled = false;
			Reset();
			_isOwnerNotificationEnabled = true;
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
			if (!_backColorGradient.IsEmpty)
			{
				return true;
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return true;
			}
			if (_alpha != -1)
			{
				return true;
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return true;
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return true;
			}
			return false;
		}

		public SchedulerFormatStyle(SchedulerFormatStyle baseStyle)
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
			_alpha = baseStyle._alpha;
			_backColorAlphaMode = baseStyle._backColorAlphaMode;
			_foreColorAlphaMode = baseStyle._foreColorAlphaMode;
			if (_alpha == -1 && baseStyle._owner != null)
			{
				_alpha = baseStyle._owner.DefaultAlpha;
			}
			if (_backColorAlphaMode == AlphaMode.Empty && baseStyle._owner != null)
			{
				_backColorAlphaMode = baseStyle._owner.BackColorAlphaMode;
			}
			if (_foreColorAlphaMode == AlphaMode.Empty && baseStyle._owner != null)
			{
				_foreColorAlphaMode = baseStyle._owner.ForeColorAlphaMode;
			}
			_isOwnerNotificationEnabled = false;
		}

		internal SchedulerFormatStyle CloneAsCustomStyle()
		{
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle();
			schedulerFormatStyle._predefinedStyle = PredefinedStyle.None;
			schedulerFormatStyle._backColor = BackColor;
			if (_customFont != null)
			{
				schedulerFormatStyle._customFont = (Font)_customFont.Clone();
				schedulerFormatStyle._fontBold = TriState.Empty;
				schedulerFormatStyle._fontItalic = TriState.Empty;
				schedulerFormatStyle._fontStrikeout = TriState.Empty;
				schedulerFormatStyle._fontUnderline = TriState.Empty;
			}
			else
			{
				schedulerFormatStyle._fontBold = _fontBold;
				schedulerFormatStyle._fontItalic = _fontItalic;
				schedulerFormatStyle._fontStrikeout = _fontStrikeout;
				schedulerFormatStyle._fontUnderline = _fontUnderline;
			}
			schedulerFormatStyle._alpha = _alpha;
			schedulerFormatStyle._backColorAlphaMode = _backColorAlphaMode;
			schedulerFormatStyle._foreColorAlphaMode = _foreColorAlphaMode;
			schedulerFormatStyle._foreColor = ForeColor;
			schedulerFormatStyle._floatValue = _floatValue;
			schedulerFormatStyle._blend = _blend;
			schedulerFormatStyle._blendGradient = _blendGradient;
			schedulerFormatStyle._backColorGradient = BackColorGradient;
			schedulerFormatStyle._backgroundGradientMode = BackgroundGradientMode;
			return schedulerFormatStyle;
		}

		internal SchedulerFormatStyle MergeWith(SchedulerFormatStyle schedulerFormatStyleArgument)
		{
			if (schedulerFormatStyleArgument == null || (schedulerFormatStyleArgument._predefinedStyle == PredefinedStyle.None && schedulerFormatStyleArgument.HasOnlyDefaultValues()))
			{
				return this;
			}
			SchedulerFormatStyle schedulerFormatStyle = new SchedulerFormatStyle(this);
			if (schedulerFormatStyleArgument._floatValue != 1f)
			{
				schedulerFormatStyle._floatValue = schedulerFormatStyleArgument._floatValue;
			}
			if (!schedulerFormatStyleArgument.BackColor.IsEmpty)
			{
				schedulerFormatStyle._backColor = schedulerFormatStyleArgument.BackColor;
			}
			if (schedulerFormatStyleArgument._alpha != -1)
			{
				schedulerFormatStyle._alpha = schedulerFormatStyleArgument._alpha;
			}
			if (schedulerFormatStyleArgument._backColorAlphaMode != AlphaMode.Empty)
			{
				schedulerFormatStyle._backColorAlphaMode = schedulerFormatStyleArgument._backColorAlphaMode;
			}
			if (schedulerFormatStyleArgument._foreColorAlphaMode != AlphaMode.Empty)
			{
				schedulerFormatStyle._foreColorAlphaMode = schedulerFormatStyleArgument._foreColorAlphaMode;
			}
			if (!schedulerFormatStyleArgument.ForeColor.IsEmpty)
			{
				schedulerFormatStyle._foreColor = schedulerFormatStyleArgument.ForeColor;
			}
			if (schedulerFormatStyleArgument._blend != -1f)
			{
				schedulerFormatStyle._blend = schedulerFormatStyleArgument._blend;
			}
			if (schedulerFormatStyleArgument._blendGradient != -1f)
			{
				schedulerFormatStyle._blendGradient = schedulerFormatStyleArgument._blendGradient;
			}
			if (!schedulerFormatStyleArgument.BackColorGradient.IsEmpty)
			{
				schedulerFormatStyle._backColorGradient = schedulerFormatStyleArgument.BackColorGradient;
			}
			if (schedulerFormatStyleArgument.BackgroundGradientMode != BackgroundGradientMode.Empty)
			{
				schedulerFormatStyle._backgroundGradientMode = schedulerFormatStyleArgument.BackgroundGradientMode;
			}
			if (schedulerFormatStyleArgument._customFont != null)
			{
				// The merged style disposes its font, so it gets its own copy.
				schedulerFormatStyle._customFont?.Dispose();
				schedulerFormatStyle._customFont = (Font)schedulerFormatStyleArgument._customFont.Clone();
			}
			else
			{
				if (schedulerFormatStyleArgument._fontBold != TriState.Empty)
				{
					schedulerFormatStyle._fontBold = schedulerFormatStyleArgument._fontBold;
				}
				if (schedulerFormatStyleArgument._fontItalic != TriState.Empty)
				{
					schedulerFormatStyle._fontItalic = schedulerFormatStyleArgument._fontItalic;
				}
				if (schedulerFormatStyleArgument._fontStrikeout != TriState.Empty)
				{
					schedulerFormatStyle._fontStrikeout = schedulerFormatStyleArgument._fontStrikeout;
				}
				if (schedulerFormatStyleArgument._fontUnderline != TriState.Empty)
				{
					schedulerFormatStyle._fontUnderline = schedulerFormatStyleArgument._fontUnderline;
				}
				if (schedulerFormatStyle._customFont != null)
				{
					schedulerFormatStyle.RebuildFont();
				}
			}
			return schedulerFormatStyle;
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

		internal void RebuildFont()
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

		private void InvalidateOwnerLayout()
		{
			if (_isOwnerNotificationEnabled && _owner != null)
			{
				_owner.InvalidateFormatStyleLayout();
			}
		}

		internal void DisposeResources()
		{
			if (_backBrush != null)
			{
				_backBrush.Dispose();
				_backBrush = null;
			}
			if (_cachedSolidBrush != null)
			{
				_cachedSolidBrush.Dispose();
				_cachedSolidBrush = null;
			}
			if (_customFont != null)
			{
				_customFont.Dispose();
				_customFont = null;
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

		private Color BlendColors(Color color, Color colorArgument, float floatValue)
		{
			if (color == Color.Empty)
			{
				return colorArgument;
			}
			if (colorArgument == Color.Empty)
			{
				return color;
			}
			float redBlend = (float)(int)color.R * floatValue + (float)(int)colorArgument.R * (1f - floatValue);
			int red = (int)Math.Round(redBlend, 0);
			float greenBlend = (float)(int)color.G * floatValue + (float)(int)colorArgument.G * (1f - floatValue);
			int green = (int)Math.Round(greenBlend, 0);
			float blueBlend = (float)(int)color.B * floatValue + (float)(int)colorArgument.B * (1f - floatValue);
			int blue = (int)Math.Round(blueBlend, 0);
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
			return Color.FromArgb(red, green, blue);
		}
	}
}
