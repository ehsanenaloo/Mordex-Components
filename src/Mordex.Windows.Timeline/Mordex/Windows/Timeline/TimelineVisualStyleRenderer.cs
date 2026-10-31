using System;
using System.Drawing;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Mordex.Windows.Timeline
{
	internal class TimelineVisualStyleRenderer
	{
		public static bool AreVisualStylesActive(bool requireKnownScheme)
		{
			try
			{
				bool areVisualStylesActive = VisualStyleInformation.IsEnabledByUser && VisualStyleInformation.IsSupportedByOS && (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled || Application.VisualStyleState == VisualStyleState.ClientAreaEnabled);
				if (areVisualStylesActive && requireKnownScheme)
				{
					if (VisualStyleInformation.ColorScheme == "NormalColor")
					{
						return true;
					}
					IntPtr intPtr = NativeMethods.OpenThemeData(IntPtr.Zero, "Button");
					int oleColor = NativeMethods.GetThemeSysColor(intPtr, 13);
					Color color = ColorTranslator.FromOle(oleColor);
					NativeMethods.CloseThemeData(ref intPtr);
					intPtr = IntPtr.Zero;
					if (color.R == 49 && color.G == 106 && color.B == 197)
					{
						return true;
					}
					if (color.R == 147 && color.G == 160 && color.B == 112)
					{
						return true;
					}
					if (color.R == 178 && color.G == 180 && color.B == 191)
					{
						return true;
					}
					return false;
				}
				return areVisualStylesActive;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsAlwaysSupported()
		{
			try
			{
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
			}
		}

		public static bool DrawText(Graphics dc, Rectangle bounds, Font font, string text, bool isHot, bool booleanArgument)
		{
			VisualStyleElement element = (booleanArgument ? VisualStyleElement.Button.PushButton.Pressed : ((!isHot) ? VisualStyleElement.Button.PushButton.Normal : VisualStyleElement.Button.PushButton.Hot));
			if (VisualStyleRenderer.IsElementDefined(element))
			{
				VisualStyleRenderer visualStyleRenderer = new VisualStyleRenderer(element);
				visualStyleRenderer.DrawBackground(dc, bounds);
				if (text != null && text.Length > 0)
				{
					TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
					TextRenderer.DrawText(dc, text, font, bounds, SystemColors.ControlText, flags);
				}
				return true;
			}
			return false;
		}
	}
}
