using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Internal
{
	internal class VisualStyleSupport
	{
		private const int ThemeColorHighlight = 13;

		private const int DeviceCapsBitsPixel = 12;

		private const int MinColorBitsPerPixel = 16;

		private static readonly Color LunaBlueHighlight = Color.FromArgb(49, 106, 197);

		private static readonly Color LunaOliveHighlight = Color.FromArgb(147, 160, 112);

		private static readonly Color LunaSilverHighlight = Color.FromArgb(178, 180, 191);

		public static bool AreVisualStylesEnabled()
		{
			return VisualStyleInformation.IsEnabledByUser && VisualStyleInformation.IsSupportedByOS && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled);
		}

		public static bool IsThemingUsable(bool requireKnownScheme)
		{
			try
			{
				bool isEnabled = AreVisualStylesEnabled();
				if (isEnabled && requireKnownScheme)
				{
					if (VisualStyleInformation.ColorScheme == "NormalColor")
					{
						return true;
					}
					Color color = GetButtonThemeHighlightColor();
					return IsLunaBlueHighlight(color) || IsLunaOliveHighlight(color) || IsLunaSilverHighlight(color);
				}
				return isEnabled;
			}
			catch (Exception ex) when (IsThemeQueryFailure(ex))
			{
				return false;
			}
		}

		public static bool SupportsLayeredWindows()
		{
			try
			{
				if (!OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
				{
					return false;
				}
				IntPtr screenDc = NativeMethods.GetDC(IntPtr.Zero);
				int bitsPerPixel = NativeMethods.GetDeviceCaps(screenDc, DeviceCapsBitsPixel).ToInt32();
				NativeMethods.ReleaseDC(IntPtr.Zero, screenDc);
				return bitsPerPixel >= MinColorBitsPerPixel;
			}
			catch (Exception ex) when (IsThemeQueryFailure(ex))
			{
				return false;
			}
		}

		// Theme and display queries fail when the theme service or a native entry point is missing; the caller then uses plain drawing.
		private static bool IsThemeQueryFailure(Exception exception)
		{
			return exception is InvalidOperationException || exception is ExternalException || exception is DllNotFoundException || exception is EntryPointNotFoundException;
		}

		internal static Color GetButtonThemeHighlightColor()
		{
			IntPtr themeData = NativeMethods.OpenThemeData(IntPtr.Zero, "Button");
			int oleColor = NativeMethods.GetThemeSysColor(themeData, ThemeColorHighlight);
			NativeMethods.CloseThemeData(themeData);
			return ColorTranslator.FromOle(oleColor);
		}

		internal static bool IsLunaBlueHighlight(Color color)
		{
			return IsSameRgb(color, LunaBlueHighlight);
		}

		internal static bool IsLunaOliveHighlight(Color color)
		{
			return IsSameRgb(color, LunaOliveHighlight);
		}

		internal static bool IsLunaSilverHighlight(Color color)
		{
			return IsSameRgb(color, LunaSilverHighlight);
		}

		private static bool IsSameRgb(Color first, Color second)
		{
			return first.R == second.R && first.G == second.G && first.B == second.B;
		}

		public static bool IsToolTipHookSupported()
		{
			return true;
		}

		public static bool DrawComboBoxBorder(Graphics graphics, Rectangle rectangle, bool isEnabled, bool hideInnerBorder)
		{
			VisualStyleElement element = VisualStyleElement.ComboBox.DropDownButton.Normal;
			if (!isEnabled)
			{
				element = VisualStyleElement.ComboBox.DropDownButton.Disabled;
			}
			if (!VisualStyleRenderer.IsElementDefined(element))
			{
				return false;
			}
			VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(element);
			Color borderColor = visualStyleRenderer.GetColor(ColorProperty.BorderColor);
			Pen pen = GdiCache.Pen(borderColor);
			rectangle = RenderingHelper.DrawRectangleOutline(graphics, pen, rectangle);
			if (!hideInnerBorder)
			{
				pen = GdiCache.Pen(SystemColors.ControlLightLight);
				RenderingHelper.DrawRectangleOutline(graphics, pen, rectangle);
				pen = null;
			}
			return true;
		}

		public static bool DrawComboBoxButtonBorder(Graphics graphics, Rectangle rectangle, bool isEnabled, bool isHot)
		{
			VisualStyleElement element = VisualStyleElement.ComboBox.DropDownButton.Normal;
			if (!isEnabled)
			{
				element = VisualStyleElement.ComboBox.DropDownButton.Disabled;
			}
			else if (isHot)
			{
				element = VisualStyleElement.ComboBox.DropDownButton.Hot;
			}
			if (!VisualStyleRenderer.IsElementDefined(element))
			{
				return false;
			}
			VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(element);
			Color borderColor = visualStyleRenderer.GetColor(ColorProperty.BorderColor);
			Pen pen = GdiCache.Pen(borderColor);
			rectangle = RenderingHelper.DrawRectangleOutline(graphics, pen, rectangle);
			if (VisualStyleInformation.DisplayName == "Windows XP style")
			{
				pen = GdiCache.Pen(SystemColors.ControlLightLight);
				RenderingHelper.DrawRectangleOutline(graphics, pen, rectangle);
				pen = null;
			}
			return true;
		}
	}
}
