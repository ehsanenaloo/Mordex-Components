using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class ControlStyle
	{
		private void DisposeResources()
		{
			if (_flatBorderPen != null)
			{
				_flatBorderPen.Dispose();
				_flatBorderPen = null;
			}
			if (_controlBrush != null)
			{
				_controlBrush.Dispose();
				_controlBrush = null;
			}
			if (_controlDarkBrush != null)
			{
				_controlDarkBrush.Dispose();
				_controlDarkBrush = null;
			}
			if (_controlDarkDarkPen != null)
			{
				_controlDarkDarkPen.Dispose();
				_controlDarkDarkPen = null;
			}
			if (_controlDarkPen != null)
			{
				_controlDarkPen.Dispose();
				_controlDarkPen = null;
			}
			if (_controlLightLightBrush != null)
			{
				_controlLightLightBrush.Dispose();
				_controlLightLightBrush = null;
			}
			if (_controlLightLightPen != null)
			{
				_controlLightLightPen.Dispose();
				_controlLightLightPen = null;
			}
			if (_controlLightPen != null)
			{
				_controlLightPen.Dispose();
				_controlLightPen = null;
			}
			if (_controlStyleBrush != null)
			{
				_controlStyleBrush.Dispose();
				_controlStyleBrush = null;
			}
			if (_controlStylePen != null)
			{
				_controlStylePen.Dispose();
				_controlStylePen = null;
			}
			if (_hoverBrush != null)
			{
				_hoverBrush.Dispose();
				_hoverBrush = null;
			}
			if (_hoverPen != null)
			{
				_hoverPen.Dispose();
				_hoverPen = null;
			}
			if (_hoverSelectedBrush != null)
			{
				_hoverSelectedBrush.Dispose();
				_hoverSelectedBrush = null;
			}
			if (_scrollBarBrush != null)
			{
				_scrollBarBrush.Dispose();
				_scrollBarBrush = null;
			}
			if (_windowBrush != null)
			{
				_windowBrush.Dispose();
				_windowBrush = null;
			}
			if (_windowTextBrush != null)
			{
				_windowTextBrush.Dispose();
				_windowTextBrush = null;
			}
			if (_windowTextPen != null)
			{
				_windowTextPen.Dispose();
				_windowTextPen = null;
			}
		}

		private void ApplyThemedPalette()
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			SetControlDarkColor(theme.GetColor(ThemeRole.BorderStrong));
			SetControlDarkDarkColor(theme.GetColor(ThemeRole.BorderStrong));
			SetControlLightColor(theme.GetColor(ThemeRole.Surface));
			SetControlLightLightColor(theme.GetColor(ThemeRole.SurfaceRaised));
		}

		private void RefreshPaletteColors()
		{
			if (_controlBrush != null)
			{
				_controlBrush.Dispose();
				_controlBrush = null;
			}
			if (!_classicLook && !ShouldSerializeControlColor())
			{
				ApplyThemedPalette();
			}
			else if (_controlColor == SystemColors.Control || _controlColor == SystemColors.Window)
			{
				SetControlDarkColor(SystemColors.ControlDark);
				SetControlDarkDarkColor(SystemColors.ControlDarkDark);
				SetControlLightColor(SystemColors.ControlLight);
				SetControlLightLightColor(SystemColors.ControlLightLight);
			}
			else
			{
				SetControlDarkColor(GridHelper.BlendColors(Color.Black, _controlColor, 0.5f));
				SetControlDarkDarkColor(GridHelper.BlendColors(Color.Black, _controlColor, 0.75f));
				SetControlLightColor(_controlColor);
				SetControlLightLightColor(GridHelper.BlendColors(Color.White, _controlColor, 0.75f));
			}
			ResetScrollBarBrush();
			NotifyChanged();
		}

		// The cached brushes and pens hold colors of the old theme, so they are dropped and made again on the next use.
		internal void ApplyThemeChange()
		{
			DisposeTextResources();
			DisposeWindowTextResources();
			DisposeWindowBrush();
			RefreshPaletteColors();
		}

		private void DisposeTextResources()
		{
			if (_controlStyleBrush != null)
			{
				_controlStyleBrush.Dispose();
				_controlStyleBrush = null;
			}
			if (_controlStylePen != null)
			{
				_controlStylePen.Dispose();
				_controlStylePen = null;
			}
			NotifyChanged();
		}

		private void DisposeControlLightLightResources()
		{
			if (_controlLightLightPen != null)
			{
				_controlLightLightPen.Dispose();
				_controlLightLightPen = null;
			}
			if (_controlLightLightBrush != null)
			{
				_controlLightLightBrush.Dispose();
				_controlLightLightBrush = null;
			}
			ResetScrollBarBrush();
			NotifyChanged();
		}

		private void DisposeWindowTextResources()
		{
			if (_windowTextBrush != null)
			{
				_windowTextBrush.Dispose();
				_windowTextBrush = null;
			}
			if (_windowTextPen != null)
			{
				_windowTextPen.Dispose();
				_windowTextPen = null;
			}
			NotifyChanged();
		}

		ImageList IGridEnvironment.GetImageList()
		{
			if (_printDocument == null)
			{
				return null;
			}
			return _printDocument.GetImageList();
		}
	}
}
