using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerFormatStyle
	{
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
			if (!_backColorGradient.IsEmpty)
			{
				return false;
			}
			if (_backgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return false;
			}
			if (_alpha != -1)
			{
				return false;
			}
			if (_backColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			if (_foreColorAlphaMode != AlphaMode.Empty)
			{
				return false;
			}
			return true;
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
				RebuildFont();
				return _customFont;
			}
			_customFont = (Font)font.Clone();
			RebuildFont();
			return _customFont;
		}

		internal bool HasGradient()
		{
			if (BackgroundGradientMode != BackgroundGradientMode.Empty)
			{
				return BackgroundGradientMode != BackgroundGradientMode.Solid;
			}
			return false;
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
					if (rectangle.Width > 0 && rectangle.Height > 0)
					{
						_backBrush = CreateGradientBrush(BlendColors(GetBlendColor(), BackColor, GetEffectiveBlend()), BlendColors(GetBlendColor(), BackColorGradient, GetGradientBlend()), rectangle, BackgroundGradientMode, _alpha, _backColorAlphaMode);
					}
				}
				else if (GetEffectiveBlend() != 0f && !BackColor.IsEmpty)
				{
					_backBrush = CreateSolidBrush(BlendColors(GetBlendColor(), BackColor, GetEffectiveBlend()), _alpha, _backColorAlphaMode);
				}
				else
				{
					_backBrush = CreateSolidBrush(BackColor, _alpha, _backColorAlphaMode);
				}
			}
			return _backBrush;
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

		internal Brush GetForeColorBrush(Color colorArgument)
		{
			Color color = ForeColor;
			if (color == Color.Empty)
			{
				color = colorArgument;
			}
			if (_cachedSolidBrush == null)
			{
				_cachedSolidBrush = CreateSolidBrush(color, _alpha, _foreColorAlphaMode);
			}
			return _cachedSolidBrush;
		}

		internal float GetEffectiveBlend()
		{
			if (_blend == -1f)
			{
				if (_owner != null)
				{
					return _owner.DefaultBlend;
				}
				return 0f;
			}
			return _blend;
		}

		internal float GetGradientBlend()
		{
			if (_blendGradient == -1f)
			{
				if (_owner != null)
				{
					return _owner.DefaultBlend;
				}
				return 0f;
			}
			return _blendGradient;
		}

		internal Color GetBlendColor()
		{
			if (_owner != null)
			{
				return _owner.BlendColor;
			}
			return SystemColors.Window;
		}
	}
}
