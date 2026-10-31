using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;

namespace Mordex.Windows.Scheduler
{
	public partial class SchedulerFormatStyle
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Reset()
		{
			_backColor = Color.Empty;
			_alpha = -1;
			_backColorAlphaMode = AlphaMode.Empty;
			_foreColorAlphaMode = AlphaMode.Empty;
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
			ApplyTheme();
			InvalidateOwnerLayout();
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

		internal void ApplyTheme()
		{
			DisposeResources();
			_themeBackColor = Color.Empty;
			_themeBackColorGradient = Color.Empty;
			_themeGradientMode = BackgroundGradientMode.Empty;
			_themeForeColor = Color.Empty;
			ApplyThemedColors();
		}
	}
}
