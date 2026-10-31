using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;

namespace Mordex.Windows.DatePicker
{
	[TypeConverter("System.ComponentModel.ExpandableObjectConverter")]
	[Localizable(true)]
	public class CalendarBackgroundStyle
	{
		internal IDatePickerElement _owner;

		private float _scale = 1f;

		private Color _backColor;

		private Color _backColorGradient;

		private BackgroundGradientMode _gradientMode;

		private Brush _cachedBrush;

		private Rectangle _cachedBrushBounds;

		private float _blend = -1f;

		private float _blendGradient = -1f;

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

		[Description("The background color of this CalendarBackgroundStyle.")]
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
					InvalidateCalendar();
				}
			}
		}

		[Description("Specifies the background color used in a linear gradient pathern.")]
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
					InvalidateCalendar();
				}
			}
		}

		[DefaultValue(BackgroundGradientMode.Empty)]
		[Description("Specifies if a linear gradient will be applied and its direction.")]
		public BackgroundGradientMode BackgrodunGradientMode
		{
			get
			{
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
					InvalidateCalendar();
				}
			}
		}

		internal static CalendarBackgroundStyle CreateResolvedCopy(CalendarBackgroundStyle baseStyle)
		{
			CalendarBackgroundStyle calendarBackgroundStyle = new CalendarBackgroundStyle(baseStyle);
			calendarBackgroundStyle._blend = baseStyle.GetBlendAmount();
			calendarBackgroundStyle._blendGradient = baseStyle.GetGradientBlendAmount();
			return calendarBackgroundStyle;
		}

		public CalendarBackgroundStyle()
		{
		}

		internal CalendarBackgroundStyle(IDatePickerElement owner)
		{
			_owner = owner;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_backColor = Color.Empty;
			_scale = 1f;
			_blend = -1f;
			_blendGradient = -1f;
			_backColorGradient = Color.Empty;
			_gradientMode = BackgroundGradientMode.Empty;
			InvalidateCalendar();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
		{
			if (!_backColor.IsEmpty)
			{
				return true;
			}
			if (_scale != 1f)
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
			if (_blend != -1f)
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
			if (_blendGradient != -1f)
			{
				return false;
			}
			return true;
		}

		public CalendarBackgroundStyle(CalendarBackgroundStyle baseStyle)
		{
			_backColor = baseStyle.BackColor;
			_scale = baseStyle._scale;
			_blend = baseStyle._blend;
			_blendGradient = baseStyle._blendGradient;
			_backColorGradient = baseStyle._backColorGradient;
			_gradientMode = baseStyle._gradientMode;
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
		public void ResetBackColor()
		{
			BackColor = Color.Empty;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeBackColor()
		{
			return !_backColor.IsEmpty;
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

		internal void DisposeBrush()
		{
			if (_cachedBrush != null)
			{
				_cachedBrush.Dispose();
				_cachedBrush = null;
			}
		}

		private bool HasGradient()
		{
			if (_gradientMode != BackgroundGradientMode.Empty)
			{
				return _gradientMode != BackgroundGradientMode.Solid;
			}
			return false;
		}

		internal Brush GetBackgroundBrush(Rectangle rectangle, int alpha)
		{
			if (HasGradient() && rectangle.Width > 0 && rectangle.Height > 0)
			{
				if (_cachedBrush == null || _cachedBrushBounds != rectangle)
				{
					_cachedBrush?.Dispose();
					_cachedBrushBounds = rectangle;
					_cachedBrush = CreateGradientBrush(BlendColors(alpha, GetBlendColor(), BackColor, GetBlendAmount()), BlendColors(alpha, GetBlendColor(), _backColorGradient, GetGradientBlendAmount()), rectangle, _gradientMode);
				}
				return _cachedBrush;
			}
			if (GetBlendAmount() != 0f && !_backColor.IsEmpty)
			{
				return GdiCache.Brush(BlendColors(alpha, GetBlendColor(), BackColor, GetBlendAmount()));
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
			return SystemColors.Window;
		}

		internal Color GetBlendedBackColor()
		{
			if (_backColor.IsEmpty && GetBlendAmount() == 0f)
			{
				return _backColor;
			}
			return BlendColors(GetOpaqueAlpha(), GetBlendColor(), _backColor, GetBlendAmount());
		}
	}
}
