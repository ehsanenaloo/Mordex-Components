namespace Mordex.Windows.Common
{
	/// <summary>How the older looks draw their faces and edges. Colors are in the theme data; this is the shape of the look.</summary>
	internal static partial class ThemeStyles
	{
		// Windows 95 to 2000: flat gray faces with 3D edges, sunken inputs, etched group frames, dithered scroll bar track.
		internal static ThemeStyle WindowsClassic()
		{
			return new ThemeStyle(ThemeGradient.None, 0f, false, ThemeBevel.Raised)
			{
				BevelLight = System.Drawing.Color.White,
				ButtonLook = ThemeButtonLook.Classic,
				ProgressLook = ThemeProgressLook.Blocks,
				ProgressBlock = 8,
				TabRaised = true,
				PointerThumb = true,
				CaptionEnd = System.Drawing.Color.FromArgb(166, 202, 240),
				InputBorder = ThemeBorder.Sunken,
				PanelBorder = ThemeBorder.Etched,
				HeaderBorder = ThemeBorder.Raised,
				Arrow = ThemeArrowGlyph.Triangle,
				FaceThumb = true,
				Check = ThemeCheckGlyph.BoldTick,
				ScrollBar = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rectangle, Grip = ThemeGrip.None, ButtonFaces = true, Edge = ThemeBorder.Raised, ThumbGradient = false }
			};
		}

		// Luna: rounded white-to-beige faces, bold ticks, rounded scroll bar thumbs with a grip.
		internal static ThemeStyle WindowsXp()
		{
			return new ThemeStyle(ThemeGradient.Vertical, 0.45f, false, ThemeBevel.None)
			{
				GradientShade = 0.1f,
				ButtonRadius = 3,
				TabRadius = 3,
				HeaderRadius = 0,
				InputRadius = 0,
				MenuRadius = 0,
				PanelRadius = 4,
				Arrow = ThemeArrowGlyph.Triangle,
				FaceThumb = true,
				ButtonLook = ThemeButtonLook.Luna,
				ButtonGlow = System.Drawing.Color.FromArgb(248, 179, 48),
				DefaultGlow = System.Drawing.Color.FromArgb(152, 184, 234),
				ProgressLook = ThemeProgressLook.Blocks,
				ProgressFill = System.Drawing.Color.FromArgb(55, 200, 55),
				ProgressBlock = 8,
				TabAccent = System.Drawing.Color.FromArgb(248, 179, 48),
				PointerThumb = true,
				ExplorerBar = true,
				Check = ThemeCheckGlyph.BoldTick,
				ScrollBar = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rounded, Grip = ThemeGrip.Lines, ButtonFaces = true, Edge = ThemeBorder.Flat, ThumbGradient = true }
			};
		}

		// Office XP: flat faces, a thin blue edge and a flat scroll bar with square buttons.
		internal static ThemeStyle OfficeXp()
		{
			return new ThemeStyle(ThemeGradient.None, 0f, false, ThemeBevel.None)
			{
				ButtonRadius = 0,
				TabRadius = 0,
				Arrow = ThemeArrowGlyph.Triangle,
				FaceThumb = true,
				ProgressLook = ThemeProgressLook.Blocks,
				ProgressBlock = 8,
				MenuGutter = true,
				Check = ThemeCheckGlyph.Tick,
				ScrollBar = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rectangle, Grip = ThemeGrip.None, ButtonFaces = true, Edge = ThemeBorder.Flat, ThumbGradient = false }
			};
		}

		// Office 2003: gradient bars, menus and toolbars with slightly rounded buttons.
		internal static ThemeStyle Office2003()
		{
			return new ThemeStyle(ThemeGradient.Vertical, 0.5f, false, ThemeBevel.None)
			{
				GradientShade = 0.12f,
				ButtonRadius = 3,
				TabRadius = 3,
				HeaderRadius = 0,
				MenuRadius = 0,
				Arrow = ThemeArrowGlyph.Triangle,
				FaceThumb = true,
				ProgressLook = ThemeProgressLook.Blocks,
				ProgressFill = System.Drawing.Color.FromArgb(255, 166, 76),
				ProgressBlock = 7,
				Grip = true,
				MenuGutter = true,
				Check = ThemeCheckGlyph.Tick,
				ScrollBar = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rounded, Grip = ThemeGrip.Lines, ButtonFaces = true, Edge = ThemeBorder.Flat, ThumbGradient = true }
			};
		}

		// Office 2007: glossy gradients with a light upper band, rounded buttons, chevron arrows.
		internal static ThemeStyle Office2007()
		{
			return new ThemeStyle(ThemeGradient.Vertical, 0.35f, true, ThemeBevel.None)
			{
				GradientShade = 0.1f,
				GlossBand = true,
				GlossAlpha = 85,
				AppointmentRadius = 3,
				ButtonRadius = 3,
				TabRadius = 3,
				HeaderRadius = 0,
				InputRadius = 0,
				MenuRadius = 2,
				PanelRadius = 3,
				Arrow = ThemeArrowGlyph.Chevron,
				FaceThumb = true,
				ButtonLook = ThemeButtonLook.Glow,
				ProgressLook = ThemeProgressLook.Glossy,
				ProgressFill = System.Drawing.Color.FromArgb(100, 190, 80),
				MenuGutter = true,
				Check = ThemeCheckGlyph.Tick,
				ScrollBar = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rounded, Grip = ThemeGrip.Lines, ButtonFaces = true, Edge = ThemeBorder.Flat, ThumbGradient = true }
			};
		}

		// Office 2010: flatter gradient, small radii, no gloss.
		internal static ThemeStyle Office2010()
		{
			return new ThemeStyle(ThemeGradient.Vertical, 0.2f, false, ThemeBevel.None)
			{
				GradientShade = 0.06f,
				ButtonRadius = 2,
				TabRadius = 2,
				HeaderRadius = 0,
				InputRadius = 0,
				MenuRadius = 0,
				PanelRadius = 2,
				Arrow = ThemeArrowGlyph.Triangle,
				FaceThumb = true,
				ButtonLook = ThemeButtonLook.Glow,
				ProgressLook = ThemeProgressLook.Glossy,
				ProgressFill = System.Drawing.Color.FromArgb(110, 190, 90),
				MenuGutter = true,
				Check = ThemeCheckGlyph.Tick,
				ScrollBar = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rectangle, Grip = ThemeGrip.Lines, ButtonFaces = false, Edge = ThemeBorder.Flat, ThumbGradient = true }
			};
		}

		// Visual Studio 2005: Luna colors with square panels and gradient tabs and captions.
		internal static ThemeStyle VisualStudio2005()
		{
			return new ThemeStyle(ThemeGradient.Vertical, 0.35f, false, ThemeBevel.None)
			{
				GradientShade = 0.08f,
				ButtonRadius = 2,
				TabRadius = 3,
				HeaderRadius = 0,
				InputRadius = 0,
				MenuRadius = 0,
				Arrow = ThemeArrowGlyph.Triangle,
				FaceThumb = true,
				ButtonLook = ThemeButtonLook.Luna,
				ButtonGlow = System.Drawing.Color.FromArgb(248, 179, 48),
				DefaultGlow = System.Drawing.Color.FromArgb(152, 184, 234),
				ProgressLook = ThemeProgressLook.Blocks,
				ProgressFill = System.Drawing.Color.FromArgb(55, 200, 55),
				ProgressBlock = 8,
				PointerThumb = true,
				MenuGutter = true,
				Check = ThemeCheckGlyph.Tick,
				ScrollBar = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rounded, Grip = ThemeGrip.Lines, ButtonFaces = true, Edge = ThemeBorder.Flat, ThumbGradient = true }
			};
		}

		// Visual Studio 2010: dark blue chrome, gradient captions and tabs, flat thin scroll bars.
		internal static ThemeStyle VisualStudio2010()
		{
			return new ThemeStyle(ThemeGradient.Vertical, 0.3f, false, ThemeBevel.None)
			{
				GradientShade = 0.08f,
				ButtonRadius = 2,
				TabRadius = 3,
				HeaderRadius = 0,
				InputRadius = 0,
				MenuRadius = 0,
				Arrow = ThemeArrowGlyph.Triangle,
				FaceThumb = true,
				ButtonLook = ThemeButtonLook.Glow,
				ProgressLook = ThemeProgressLook.Glossy,
				ProgressFill = System.Drawing.Color.FromArgb(90, 180, 70),
				MenuGutter = true,
				Check = ThemeCheckGlyph.Tick,
				ScrollBar = new ThemeScrollBarStyle { Custom = true, Thumb = ThemeThumbShape.Rectangle, Grip = ThemeGrip.None, ButtonFaces = false, Edge = ThemeBorder.Flat, ThumbGradient = true }
			};
		}
	}
}
