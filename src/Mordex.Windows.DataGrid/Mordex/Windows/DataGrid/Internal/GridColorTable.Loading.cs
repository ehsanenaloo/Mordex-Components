using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	internal partial class GridColorTable
	{internal void LoadColorsForStyle()
		{
			if (_themedLook)
			{
				LoadThemedColors();
				return;
			}
			if (_useThemes && LoadXpThemeColors())
			{
				return;
			}
			LoadClassicColors();
			if (_colors[43].IsEmpty)
			{
				ref Color reference = ref _colors[43];
				reference = _colors[30];
			}
		}

				private bool LoadXpThemeColors()
		{
			try
			{
				int themeSysColor = 0;
				if (VisualStyleInformation.ColorScheme == "HomeStead")
				{
					_themeName = "OLIVE";
				}
				else if (VisualStyleInformation.ColorScheme == "Metallic")
				{
					_themeName = "SILVER";
				}
				else
				{
					_themeName = "DEFAULT";
				}
				IntPtr intPtr = NativeMethods.OpenThemeData(IntPtr.Zero, "Button");
				themeSysColor = NativeMethods.GetThemeSysColor(intPtr, 15);
				Color color = ColorTranslator.FromOle(themeSysColor);
				VisualStyleDrawer.TrySetElement(VisualStyleElement.Button.PushButton.Normal);
				Color otherColor = VisualStyleDrawer._visualStyleRenderer.GetColor(ColorProperty.BorderColor);
				NativeMethods.GetThemeColor(intPtr, 1, 1, 3823, ref themeSysColor);
				Color gridColor = ColorTranslator.FromOle(themeSysColor);
				_colors[20] = color;
				ref Color reference = ref _colors[28];
				reference = SystemColors.ControlLightLight;
				_colors[29] = color;
				ref Color controlTextColor = ref _colors[30];
				controlTextColor = SystemColors.ControlText;
				ref Color otherReference = ref _colors[0];
				otherReference = GridHelper.BlendColors(color, SystemColors.ControlDark, 0.5f);
				ref Color nextReference = ref _colors[1];
				nextReference = SystemColors.ControlDark;
				ref Color previousReference = ref _colors[2];
				previousReference = GridHelper.BlendColors(SystemColors.ControlLightLight, color, 0.75f);
				ref Color firstReference = ref _colors[3];
				firstReference = GridHelper.BlendColors(color, otherColor, 0.95f);
				ref Color secondReference = ref _colors[4];
				secondReference = SystemColors.ControlDark;
				ref Color thirdReference = ref _colors[5];
				thirdReference = SystemColors.ControlLightLight;
				_colors[6] = color;
				ref Color otherControlTextColor = ref _colors[7];
				otherControlTextColor = SystemColors.ControlText;
				ref Color fourthReference = ref _colors[20];
				fourthReference = GridHelper.BlendColors(SystemColors.ControlLightLight, color, 0.5f);
				ref Color windowColor = ref _colors[18];
				windowColor = SystemColors.Window;
				ref Color windowTextColor = ref _colors[19];
				windowTextColor = SystemColors.WindowText;
				ref Color fifthReference = ref _colors[10];
				fifthReference = GridHelper.BlendColors(SystemColors.ControlLightLight, color, 0.75f);
				ref Color sixthReference = ref _colors[11];
				sixthReference = GridHelper.BlendColors(color, otherColor, 0.95f);
				ref Color nextControlTextColor = ref _colors[12];
				nextControlTextColor = SystemColors.ControlText;
				ref Color seventhReference = ref _colors[9];
				seventhReference = GridHelper.BlendColors(color, otherColor, 0.75f);
				ref Color otherGridColor = ref _colors[8];
				otherGridColor = _colors[9];
				ref Color nextGridColor = ref _colors[16];
				nextGridColor = _colors[6];
				ref Color previousGridColor = ref _colors[17];
				previousGridColor = _colors[5];
				ref Color eighthReference = ref _colors[13];
				eighthReference = GridHelper.BlendColors(SystemColors.ControlLightLight, gridColor, 0.75f);
				_colors[14] = gridColor;
				ref Color ninthReference = ref _colors[31];
				ninthReference = GridHelper.BlendColors(otherColor, color, 0.75f);
				_colors[32] = color;
				_colors[33] = color;
				ref Color otherWindowColor = ref _colors[35];
				otherWindowColor = SystemColors.Window;
				ref Color otherWindowTextColor = ref _colors[36];
				otherWindowTextColor = SystemColors.WindowText;
				ref Color highlightTextColor = ref _colors[38];
				highlightTextColor = SystemColors.HighlightText;
				ref Color highlightColor = ref _colors[37];
				highlightColor = SystemColors.Highlight;
				NativeMethods.CloseThemeData(ref intPtr);
				intPtr = NativeMethods.OpenThemeData(IntPtr.Zero, "EXPLORERBAR");
				NativeMethods.GetThemeColor(intPtr, 12, 1, 3803, ref themeSysColor);
				Color firstGridColor = ColorTranslator.FromOle(themeSysColor);
				NativeMethods.GetThemeColor(intPtr, 8, 1, 3810, ref themeSysColor);
				Color nextColor = ColorTranslator.FromOle(themeSysColor);
				ref Color tenthReference = ref _colors[22];
				tenthReference = GridHelper.BlendColors(SystemColors.ControlLightLight, nextColor, 0.95f);
				ref Color innerReference = ref _colors[23];
				innerReference = GridHelper.BlendColors(SystemColors.ControlLightLight, nextColor, 0.5f);
				NativeMethods.GetThemeColor(intPtr, 12, 1, 3802, ref themeSysColor);
				Color secondGridColor = ColorTranslator.FromOle(themeSysColor);
				_colors[24] = secondGridColor;
				_colors[25] = secondGridColor;
				ref Color outerReference = ref _colors[26];
				outerReference = GridHelper.BlendColors(SystemColors.ControlLightLight, secondGridColor, 0.25f);
				_colors[27] = firstGridColor;
				ref Color extraReference = ref _colors[39];
				extraReference = SystemColors.ControlDark;
				NativeMethods.CloseThemeData(ref intPtr);
				intPtr = NativeMethods.OpenThemeData(IntPtr.Zero, "COMBOBOX");
				NativeMethods.GetThemeColor(intPtr, 1, 1, 3801, ref themeSysColor);
				ref Color referenceMore = ref _colors[31];
				referenceMore = ColorTranslator.FromOle(themeSysColor);
				NativeMethods.GetThemeColor(intPtr, 1, 4, 3801, ref themeSysColor);
				ref Color referenceMorex = ref _colors[34];
				referenceMorex = ColorTranslator.FromOle(themeSysColor);
				NativeMethods.CloseThemeData(ref intPtr);
				ref Color thirdGridColor = ref _colors[41];
				thirdGridColor = _colors[9];
				return true;
			}
			finally
			{
			}
		}
	}
}
