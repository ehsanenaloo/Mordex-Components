using System.Drawing;

namespace Mordex.Windows.Common
{
	internal static partial class ThemeStyles
	{
		// Office 2007: glass faces with a sharp split, orange glow on hot and pressed buttons, blue glass check boxes.
		internal static ThemeStyle Office2007Glass()
		{
			return Office2007() with
			{
				Modern = new ThemeModernStyle
				{
					FaceKind = ThemeFaceKind.Glass,
					Glass = ThemeGlassKind.Office2007,
					BoxLook = ThemeBoxLook.Glass,
					BoxMark = Color.FromArgb(72, 105, 148)
				}
			};
		}

		// Office 2010: flat light gradients with a soft split, a light gold glow and the same boxes in a flatter form.
		internal static ThemeStyle Office2010Glass()
		{
			return Office2010() with
			{
				Modern = new ThemeModernStyle
				{
					Glass = ThemeGlassKind.Office2010,
					BoxLook = ThemeBoxLook.Glass,
					BoxMark = Color.FromArgb(57, 87, 130)
				}
			};
		}

		// Visual Studio 2010: flat gradients and a gold glow on hot and pressed buttons.
		internal static ThemeStyle VisualStudio2010Glass()
		{
			return VisualStudio2010() with
			{
				TabAccent = Color.FromArgb(229, 195, 101),
				Modern = new ThemeModernStyle
				{
					Glass = ThemeGlassKind.Office2010
				}
			};
		}

		// The color of dialogs and panels in the Office 2007 and 2010 looks (the control color of the old renderers).
		internal static Color OfficeSurface(string family, string scheme, Color menuBackground)
		{
			if (family == "Office 2007")
			{
				return scheme == "Blue" ? Color.FromArgb(227, 239, 255) : Color.FromArgb(240, 241, 242);
			}
			return scheme == "Blue" ? Color.FromArgb(207, 221, 238) : scheme == "Silver" ? Color.FromArgb(220, 226, 232) : menuBackground;
		}

		private static readonly ThemeScrollBarStyle Pill = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Pill, Grip = ThemeGrip.None, ButtonFaces = false, Edge = ThemeBorder.Flat, ThumbGradient = false };

		private static readonly ThemeScrollBarStyle Flat = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rectangle, Grip = ThemeGrip.None, ButtonFaces = false, Edge = ThemeBorder.Flat, ThumbGradient = false };

		internal static Color Nz(Color[] palette, int slot, int fallback)
		{
			return palette[slot].A < 200 ? palette[fallback] : palette[slot];
		}

		internal static ThemeStyle WithMenuBack(ThemeStyle style, Color menuBack)
		{
			return style with { Modern = style.Modern with { MenuBack = menuBack } };
		}

		// The shape of the modern looks. Colors come from the palette; this holds how the corners, focus and tabs look.
		internal static ThemeStyle Modern(string family, Color accent)
		{
			switch (family)
			{
			case "Windows 11":
				// Rounded 4 pixel controls. An input shows focus with a two pixel accent line along its bottom edge.
				return new ThemeStyle
				{
					TabRadius = 6,
					ScrollBar = Pill,
					Modern = new ThemeModernStyle { InputFocus = ThemeFocusKind.Underline, InputFocusWidth = 2 }
				};
			case "Windows 10":
				// Square and flat. Focus is a two pixel border.
				return new ThemeStyle
				{
					ButtonRadius = 0,
					InputRadius = 0,
					TabRadius = 0,
					ScrollBar = Flat,
					MenuRadius = 0,
					PanelRadius = 0,
					Modern = new ThemeModernStyle { InputFocus = ThemeFocusKind.Border, InputFocusWidth = 2 }
				};
			case "Visual Studio 2022":
				// Flat with a one pixel blue focus border and square tabs with a line of the accent color on top.
				return new ThemeStyle
				{
					ButtonRadius = 0,
					InputRadius = 0,
					TabRadius = 0,
					ScrollBar = Flat,
					MenuRadius = 0,
					PanelRadius = 0,
					TabAccent = accent,
					Modern = new ThemeModernStyle { InputFocus = ThemeFocusKind.Border }
				};
			case "Office 365":
				// Flat and square. Inputs and tabs show an accent line of two pixels.
				return new ThemeStyle
				{
					ButtonRadius = 0,
					InputRadius = 0,
					TabRadius = 0,
					ScrollBar = Flat,
					MenuRadius = 0,
					PanelRadius = 0,
					TabAccent = accent,
					Modern = new ThemeModernStyle { InputFocus = ThemeFocusKind.Underline, InputFocusWidth = 2, TabLine = ThemeTabLine.Inner, TabLineWidth = 2 }
				};
			case "Chrome":
				// Pill buttons, round inputs, tabs with round upper corners.
				return new ThemeStyle
				{
					ButtonRadius = 16,
					InputRadius = 12,
					TabRadius = 10,
					ScrollBar = Pill,
					MenuRadius = 8,
					PanelRadius = 8,
					Modern = new ThemeModernStyle { InputFocus = ThemeFocusKind.Border, InputFocusWidth = 2, TrapezoidTabs = true }
				};
			case "Firefox":
				// Four pixel corners with a focus ring.
				return new ThemeStyle
				{
					ButtonRadius = 4,
					InputRadius = 4,
					TabRadius = 4,
					ScrollBar = Pill,
					MenuRadius = 6,
					PanelRadius = 8,
					Modern = new ThemeModernStyle { InputFocus = ThemeFocusKind.Ring, InputFocusWidth = 2 }
				};
			default:
				return default;
			}
		}
	}
}
