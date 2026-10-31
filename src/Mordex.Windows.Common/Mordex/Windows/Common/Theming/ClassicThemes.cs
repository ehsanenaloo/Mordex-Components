using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// The older looks of the suite as themes: Office 2007 and 2010 (from their slot palettes), Windows XP, Office XP,
	/// Office 2003, Visual Studio 2005 and Visual Studio 2010. Each look is plain data: a few colors and a style. The other
	/// roles come from them by blending, as in <see cref="ModernThemes"/>.
	/// </summary>
	/// <remarks>
	/// The colors of the Windows XP, Office 2003 and Visual Studio looks come from the color tables that the controls
	/// used for the Luna blue, olive green and silver schemes. Office XP follows the system color blends it used on Luna.
	/// </remarks>
	internal static class ClassicThemes
	{
		private sealed class Spec
		{
			public string Name;
			public string Family;
			public ThemeKind Kind = ThemeKind.Light;
			public ThemeShape Shape;
			public ThemeStyle Style;
			public int Surface;
			public int Panel;
			public int Raised;
			public int Input = 0xFFFFFF;
			public int Text = 0x000000;
			public int Muted;
			public int Border;
			public int BorderStrong;
			public int Hover;
			public int Pressed;
			public int Selected;
			public int Accent;
			public int AccentHot;
			public int AccentPressed;
			public int TextOnAccent = 0x000000;
			public int Caption;
			public int CaptionInactive;
			public int Toolbar;
			public int StatusBar;
			public int StatusBarText = 0x000000;
			public int Tooltip = 0xFFFFE1;
		}

		internal static IEnumerable<MxTheme> All()
		{
			yield return FromOffice("Office 2007", OfficePalettes.Office2007Blue, "Blue", false);
			yield return FromOffice("Office 2007", OfficePalettes.Office2007Silver, "Silver", false);
			yield return FromOffice("Office 2007", OfficePalettes.Office2007Black, "Black", true);
			yield return FromOffice("Office 2010", OfficePalettes.Office2010Blue, "Blue", false);
			yield return FromOffice("Office 2010", OfficePalettes.Office2010Silver, "Silver", false);
			yield return FromOffice("Office 2010", OfficePalettes.Office2010Black, "Black", true);
			foreach (MxTheme theme in WindowsXp())
			{
				yield return theme;
			}
			yield return OfficeXp();
			foreach (MxTheme theme in Office2003())
			{
				yield return theme;
			}
			foreach (MxTheme theme in VisualStudio2005())
			{
				yield return theme;
			}
			yield return VisualStudio2010();
		}

		// The role colors of the Office 2007 and 2010 looks come from their slot palette. The palette stays with the theme.
		private static MxTheme FromOffice(string family, uint[] data, string scheme, bool dark)
		{
			Color[] p = new Color[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				p[i] = Color.FromArgb(unchecked((int)data[i]));
			}
			Color back = ThemeStyles.OfficeSurface(family, scheme, p[OfficeSlot.MenuBackground]);
			Color hotFace = MxTheme.Blend(ThemeStyles.Nz(p, OfficeSlot.HotLightColor, OfficeSlot.HotDarkColor), ThemeStyles.Nz(p, OfficeSlot.HotDarkColor, OfficeSlot.HotLightColor), 0.5f);
			Color pressedFace = MxTheme.Blend(ThemeStyles.Nz(p, OfficeSlot.PressedLightColor, OfficeSlot.PressedDarkColor), ThemeStyles.Nz(p, OfficeSlot.PressedDarkColor, OfficeSlot.PressedLightColor), 0.5f);
			bool lightBack = MxTheme.Luminance(back) > 0.5;
			Color text = lightBack ? Color.Black : Color.White;
			Color accent = family == "Office 2007" ? Color.FromArgb(255, 189, 105) : Color.FromArgb(255, 213, 141);
			Color[] c = new Color[MxTheme.RoleCount];
			Set(c, ThemeRole.Surface, back);
			Set(c, ThemeRole.SurfaceAlt, p[OfficeSlot.ImageAreaBackground]);
			Set(c, ThemeRole.SurfaceRaised, p[OfficeSlot.RibbonLight]);
			Set(c, ThemeRole.SurfaceHot, hotFace);
			Set(c, ThemeRole.SurfacePressed, pressedFace);
			Set(c, ThemeRole.SurfaceDisabled, back);
			Set(c, ThemeRole.InputBack, Color.White);
			Set(c, ThemeRole.InputBackDisabled, back);
			Set(c, ThemeRole.Text, text);
			Set(c, ThemeRole.TextMuted, lightBack ? Color.FromArgb(96, 96, 96) : Color.FromArgb(170, 170, 170));
			Set(c, ThemeRole.TextDisabled, Color.FromArgb(128, 128, 128));
			Set(c, ThemeRole.TextOnAccent, Color.Black);
			Set(c, ThemeRole.TextLink, Color.FromArgb(0, 102, 204));
			Set(c, ThemeRole.Border, p[OfficeSlot.Border]);
			Set(c, ThemeRole.BorderStrong, p[OfficeSlot.BorderDark]);
			Set(c, ThemeRole.BorderHot, accent);
			Set(c, ThemeRole.BorderPressed, accent);
			Set(c, ThemeRole.BorderFocus, accent);
			Set(c, ThemeRole.BorderDisabled, MxTheme.Blend(p[OfficeSlot.Border], back, 0.5f));
			Set(c, ThemeRole.Accent, accent);
			Set(c, ThemeRole.AccentHot, MxTheme.Lighten(accent, 0.2f));
			Set(c, ThemeRole.AccentPressed, MxTheme.Darken(accent, 0.12f));
			Set(c, ThemeRole.AccentSubtle, MxTheme.Blend(back, accent, 0.3f));
			Set(c, ThemeRole.Selection, accent);
			Set(c, ThemeRole.SelectionText, Color.Black);
			Set(c, ThemeRole.SelectionInactive, p[OfficeSlot.RibbonDark]);
			Set(c, ThemeRole.SelectionInactiveText, text);
			Set(c, ThemeRole.Header, p[OfficeSlot.RibbonLight]);
			Set(c, ThemeRole.HeaderHot, hotFace);
			Set(c, ThemeRole.HeaderPressed, pressedFace);
			Set(c, ThemeRole.HeaderText, text);
			Set(c, ThemeRole.GridLine, MxTheme.Blend(p[OfficeSlot.Border], lightBack ? Color.White : back, 0.6f));
			Set(c, ThemeRole.RowAlt, MxTheme.Blend(Color.White, back, 0.35f));
			Set(c, ThemeRole.ScrollTrack, back);
			Set(c, ThemeRole.ScrollThumb, p[OfficeSlot.RibbonDark]);
			Set(c, ThemeRole.ScrollThumbHot, hotFace);
			Set(c, ThemeRole.ScrollThumbPressed, p[OfficeSlot.RibbonDark]);
			Set(c, ThemeRole.TabActive, Color.White);
			Set(c, ThemeRole.TabInactive, back);
			Set(c, ThemeRole.TabHot, hotFace);
			Set(c, ThemeRole.ToolbarBack, p[OfficeSlot.RibbonLight]);
			Set(c, ThemeRole.StatusBarBack, p[OfficeSlot.RibbonDark]);
			Set(c, ThemeRole.StatusBarText, text);
			Set(c, ThemeRole.TooltipBack, Color.FromArgb(255, 255, 225));
			Set(c, ThemeRole.TooltipText, Color.Black);
			Set(c, ThemeRole.Caption, p[OfficeSlot.RibbonDark]);
			Set(c, ThemeRole.CaptionInactive, back);
			Set(c, ThemeRole.Success, Color.FromArgb(0, 128, 0));
			Set(c, ThemeRole.Warning, Color.FromArgb(200, 120, 0));
			Set(c, ThemeRole.Error, Color.FromArgb(200, 0, 0));
			Set(c, ThemeRole.Info, Color.FromArgb(0, 102, 204));
			Set(c, ThemeRole.Shadow, Color.FromArgb(64, 0, 0, 0));
			Set(c, ThemeRole.Overlay, Color.FromArgb(96, 0, 0, 0));
			return new MxTheme(family + " " + scheme, family, dark ? ThemeKind.Dark : ThemeKind.Light, c, ThemeShape.Classic, p, ThemeStyles.WithMenuBack(family == "Office 2007" ? ThemeStyles.Office2007Glass() : ThemeStyles.Office2010Glass(), p[OfficeSlot.MenuBackground]));
		}

		// Luna: rounded faces with a light-to-face gradient, a blue, olive or silver selection and caption.
		private static IEnumerable<MxTheme> WindowsXp()
		{
			ThemeShape shape = new ThemeShape(3, 1, false, 1, true);
			ThemeStyle style = ThemeStyles.WindowsXpLook();
			yield return Make(new Spec
			{
				Name = "Windows XP Blue", Family = "Windows XP", Shape = shape, Style = style,
				Surface = 0xECE9D8, Panel = 0xF5F4EA, Raised = 0xFFFFFF, Muted = 0x716F64, Border = 0x7F9DB9, BorderStrong = 0x003C74,
				Hover = 0xE6EFFC, Pressed = 0xC1D2EE, Selected = 0xC1D2EE,
				Accent = 0x316AC5, AccentHot = 0x3B80ED, AccentPressed = 0x1E4B96, TextOnAccent = 0xFFFFFF,
				Caption = 0x0054E3, CaptionInactive = 0x7A96DF, Toolbar = 0xECE9D8, StatusBar = 0xECE9D8
			});
			yield return Make(new Spec
			{
				Name = "Windows XP Olive Green", Family = "Windows XP", Shape = shape, Style = style,
				Surface = 0xECE9D8, Panel = 0xF2F1E4, Raised = 0xFFFFFF, Muted = 0x716F64, Border = 0x60805A, BorderStrong = 0x4D6A48,
				Hover = 0xEDF2D4, Pressed = 0xB6C68D, Selected = 0xC2CF9E,
				Accent = 0x93A070, AccentHot = 0xB6C68D, AccentPressed = 0x74865E, TextOnAccent = 0x000000,
				Caption = 0x91A075, CaptionInactive = 0xB6C392, Toolbar = 0xECE9D8, StatusBar = 0xECE9D8
			});
			yield return Make(new Spec
			{
				Name = "Windows XP Silver", Family = "Windows XP", Shape = shape, Style = style,
				Surface = 0xE0DFE3, Panel = 0xF3F3F7, Raised = 0xFFFFFF, Muted = 0x6A6D92, Border = 0x7C7C94, BorderStrong = 0x5E5E7A,
				Hover = 0xE1E2EC, Pressed = 0xC9CAD9, Selected = 0xD3D4DD,
				Accent = 0x706F91, AccentHot = 0x9391B0, AccentPressed = 0x6A6D92, TextOnAccent = 0xFFFFFF,
				Caption = 0xA6A5BF, CaptionInactive = 0xD3D4DD, Toolbar = 0xE0DFE3, StatusBar = 0xE0DFE3
			});
		}

		// Office XP: flat faces with a light blue hot color and a blue edge. The blends are the ones the look made from
		// the Luna system colors.
		private static MxTheme OfficeXp()
		{
			Color control = Color.FromArgb(236, 233, 216);
			Color window = Color.White;
			Color highlight = Color.FromArgb(49, 106, 197);
			Color tinted = Mix(control, highlight, 0.1f);
			Color tintedWindow = Mix(tinted, window, 0.33f);
			Color hot = tintedWindow;
			Color selected = Mix(Mix(control, highlight, 0.8f), window, 0.53f);
			Color pressed = Mix(highlight, window, 0.5f);
			return Make(new Spec
			{
				Name = "Office XP", Family = "Office XP", Shape = new ThemeShape(0, 1, true, 1, true), Style = ThemeStyles.OfficeXpLook(),
				Surface = Pack(control), Panel = Pack(Mix(control, window, 0.85f)), Raised = Pack(Mix(control, window, 0.15f)),
				Muted = 0x716F64, Border = Pack(Mix(control, Color.Black, 0.65f)), BorderStrong = 0x716F64,
				Hover = Pack(hot), Pressed = Pack(pressed), Selected = Pack(selected),
				Accent = 0x316AC5, AccentHot = 0x3B80ED, AccentPressed = 0x1E4B96, TextOnAccent = 0xFFFFFF,
				Caption = 0x316AC5, CaptionInactive = 0xB6C0D8, StatusBar = Pack(control),
				// Measured on Office XP on Windows XP: the bars are lighter than the dock behind them.
				Toolbar = 0xEFEDDE
			});
		}

		// Office 2003: gradient bars in the blue, olive and silver schemes with an orange selection.
		private static IEnumerable<MxTheme> Office2003()
		{
			ThemeShape shape = new ThemeShape(0, 1, false, 1, true);
			ThemeStyle style = ThemeStyles.Office2003Look();
			yield return Make(new Spec
			{
				Name = "Office 2003 Blue", Family = "Office 2003", Shape = shape, Style = style,
				Surface = 0xC4DAFA, Panel = 0x9EBEF5, Raised = 0xDAEAFD, Muted = 0x4E6EA8, Border = 0x002D96, BorderStrong = 0x002D96,
				Hover = 0xFFFFDC, Pressed = 0xE87F08, Selected = 0xFBE694,
				Accent = 0xFFA64C, AccentHot = 0xFFD58C, AccentPressed = 0xE87F08,
				Caption = 0x5987D6, CaptionInactive = 0xA0BFF5, Toolbar = 0xC4DAFA, StatusBar = 0x9EBEF5
			});
			yield return Make(new Spec
			{
				Name = "Office 2003 Olive Green", Family = "Office 2003", Shape = shape, Style = style,
				Surface = 0xF2F1E4, Panel = 0xD9D9A7, Raised = 0xEDF2D4, Muted = 0x5F7A58, Border = 0x608058, BorderStrong = 0x608058,
				Hover = 0xFFFFDC, Pressed = 0xE87F08, Selected = 0xFBE694,
				Accent = 0xFFA64C, AccentHot = 0xFFD58C, AccentPressed = 0xE87F08,
				Caption = 0xAFC082, CaptionInactive = 0xDADAAA, Toolbar = 0xF2F1E4, StatusBar = 0xD9D9A7
			});
			yield return Make(new Spec
			{
				Name = "Office 2003 Silver", Family = "Office 2003", Shape = shape, Style = style,
				Surface = 0xF3F3F7, Panel = 0xD7D7E5, Raised = 0xF0F0F8, Muted = 0x6A6D92, Border = 0x7C7C94, BorderStrong = 0x7C7C94,
				Hover = 0xFFFFDC, Pressed = 0xE87F08, Selected = 0xFBE694,
				Accent = 0xFFA64C, AccentHot = 0xFFD58C, AccentPressed = 0xE87F08,
				Caption = 0xA8A7BF, CaptionInactive = 0xD8D8E6, Toolbar = 0xF3F3F7, StatusBar = 0xD7D7E5
			});
		}

		// Visual Studio 2005: the Luna color schemes with panel captions and a light hot color.
		private static IEnumerable<MxTheme> VisualStudio2005()
		{
			ThemeShape shape = new ThemeShape(0, 1, false, 1, true);
			ThemeStyle style = ThemeStyles.VisualStudio2005Look();
			yield return Make(new Spec
			{
				Name = "Visual Studio 2005 Blue", Family = "Visual Studio 2005", Shape = shape, Style = style,
				Surface = 0xECE9D8, Panel = 0xE5E5D7, Raised = 0xFCFCFE, Muted = 0x716F64, Border = 0xACA899, BorderStrong = 0x716F64,
				Hover = 0xC1D2ED, Pressed = 0x98B4E2, Selected = 0xE1E6E8,
				Accent = 0x316AC5, AccentHot = 0x3B80ED, AccentPressed = 0x1E4B96, TextOnAccent = 0xFFFFFF,
				Caption = 0x3B80ED, CaptionInactive = 0xCCC7BA, Toolbar = 0xECE9D8, StatusBar = 0xE5E5D7
			});
			yield return Make(new Spec
			{
				Name = "Visual Studio 2005 Olive Green", Family = "Visual Studio 2005", Shape = shape, Style = style,
				Surface = 0xECE9D8, Panel = 0xE5E5D7, Raised = 0xFCFCFE, Muted = 0x716F64, Border = 0xACA899, BorderStrong = 0x716F64,
				Hover = 0xB6C68D, Pressed = 0x93A070, Selected = 0xC2CF9E,
				Accent = 0x93A070, AccentHot = 0xB6C68D, AccentPressed = 0x74865E, TextOnAccent = 0x000000,
				Caption = 0xB6C392, CaptionInactive = 0xCCC7BA, Toolbar = 0xECE9D8, StatusBar = 0xE5E5D7
			});
			yield return Make(new Spec
			{
				Name = "Visual Studio 2005 Silver", Family = "Visual Studio 2005", Shape = shape, Style = style,
				Surface = 0xECE9D8, Panel = 0xD7D7E5, Raised = 0xFEFEFE, Muted = 0x716F64, Border = 0x7C7C94, BorderStrong = 0x6A6D92,
				Hover = 0xE1E2EC, Pressed = 0xC4C5D6, Selected = 0xD3D4DD,
				Accent = 0x706F91, AccentHot = 0x9391B0, AccentPressed = 0x6A6D92, TextOnAccent = 0xFFFFFF,
				Caption = 0xD3D4DD, CaptionInactive = 0xEEEEEE, Toolbar = 0xECE9D8, StatusBar = 0xD7D7E5
			});
		}

		// Visual Studio 2010: dark blue panel chrome with gold hot and selected colors.
		private static MxTheme VisualStudio2010()
		{
			return Make(new Spec
			{
				Name = "Visual Studio 2010", Family = "Visual Studio 2010", Shape = new ThemeShape(0, 1, false, 1, true),
				Style = ThemeStyles.VisualStudio2010Glass(),
				Surface = 0xBCC7D8, Panel = 0xCAD3E2, Raised = 0xFFFFFF, Muted = 0x3D5277, Border = 0x293955, BorderStrong = 0x293955,
				Hover = 0xF9E189, Pressed = 0xFFE8A6, Selected = 0xFFEFBB,
				Accent = 0xE5C365, AccentHot = 0xF9E189, AccentPressed = 0xCDAA4A,
				// Measured on Visual Studio 2010 pictures: D5DCE8 command bar, 293955 window back and status bar, 4D6082
				// inactive tool window caption.
				Caption = 0x2D3E5C, CaptionInactive = 0x4D6082, Toolbar = 0xD5DCE8, StatusBar = 0x293955, StatusBarText = 0xFFFFFF
			});
		}

		private static int Pack(Color color)
		{
			return (color.R << 16) | (color.G << 8) | color.B;
		}

		// Weight of the first color, as the old blends did: a * weight + b * (1 - weight).
		private static Color Mix(Color a, Color b, float weight)
		{
			return Color.FromArgb(255, (int)(a.R * weight + b.R * (1f - weight)), (int)(a.G * weight + b.G * (1f - weight)), (int)(a.B * weight + b.B * (1f - weight)));
		}

		private static Color C(int rgb)
		{
			return Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
		}

		private static MxTheme Make(Spec s)
		{
			bool dark = s.Kind == ThemeKind.Dark;
			Color surface = C(s.Surface);
			Color panel = C(s.Panel);
			Color text = C(s.Text);
			Color accent = C(s.Accent);
			Color border = C(s.Border);
			Color input = C(s.Input);
			Color[] c = new Color[MxTheme.RoleCount];
			Set(c, ThemeRole.Surface, surface);
			Set(c, ThemeRole.SurfaceAlt, panel);
			Set(c, ThemeRole.SurfaceRaised, C(s.Raised));
			Set(c, ThemeRole.SurfaceHot, C(s.Hover));
			Set(c, ThemeRole.SurfacePressed, C(s.Pressed));
			Set(c, ThemeRole.SurfaceDisabled, MxTheme.Blend(surface, panel, 0.5f));
			Set(c, ThemeRole.InputBack, input);
			Set(c, ThemeRole.InputBackDisabled, surface);
			Set(c, ThemeRole.Text, text);
			Set(c, ThemeRole.TextMuted, C(s.Muted));
			Set(c, ThemeRole.TextDisabled, Color.FromArgb(172, 168, 153));
			Set(c, ThemeRole.TextOnAccent, C(s.TextOnAccent));
			Set(c, ThemeRole.TextLink, Color.FromArgb(0, 0, 255));
			Set(c, ThemeRole.Border, border);
			Set(c, ThemeRole.BorderStrong, C(s.BorderStrong));
			Set(c, ThemeRole.BorderHot, accent);
			Set(c, ThemeRole.BorderPressed, C(s.AccentPressed));
			Set(c, ThemeRole.BorderFocus, accent);
			Set(c, ThemeRole.BorderDisabled, MxTheme.Blend(border, surface, 0.5f));
			Set(c, ThemeRole.Accent, accent);
			Set(c, ThemeRole.AccentHot, C(s.AccentHot));
			Set(c, ThemeRole.AccentPressed, C(s.AccentPressed));
			Set(c, ThemeRole.AccentSubtle, C(s.Selected));
			Set(c, ThemeRole.Selection, accent);
			Set(c, ThemeRole.SelectionText, C(s.TextOnAccent));
			Set(c, ThemeRole.SelectionInactive, border);
			Set(c, ThemeRole.SelectionInactiveText, text);
			Set(c, ThemeRole.Header, surface);
			Set(c, ThemeRole.HeaderHot, C(s.Hover));
			Set(c, ThemeRole.HeaderPressed, C(s.Pressed));
			Set(c, ThemeRole.HeaderText, text);
			Set(c, ThemeRole.GridLine, MxTheme.Blend(border, input, 0.5f));
			Set(c, ThemeRole.RowAlt, MxTheme.Blend(input, panel, 0.5f));
			Set(c, ThemeRole.ScrollTrack, panel);
			Set(c, ThemeRole.ScrollThumb, MxTheme.Blend(surface, border, 0.35f));
			Set(c, ThemeRole.ScrollThumbHot, MxTheme.Blend(surface, border, 0.55f));
			Set(c, ThemeRole.ScrollThumbPressed, MxTheme.Blend(surface, border, 0.75f));
			Set(c, ThemeRole.TabActive, C(s.Raised));
			Set(c, ThemeRole.TabInactive, surface);
			Set(c, ThemeRole.TabHot, C(s.Hover));
			Set(c, ThemeRole.ToolbarBack, C(s.Toolbar));
			Set(c, ThemeRole.StatusBarBack, C(s.StatusBar));
			Set(c, ThemeRole.StatusBarText, C(s.StatusBarText));
			Set(c, ThemeRole.TooltipBack, C(s.Tooltip));
			Set(c, ThemeRole.TooltipText, Color.Black);
			Set(c, ThemeRole.Caption, C(s.Caption));
			Set(c, ThemeRole.CaptionInactive, C(s.CaptionInactive));
			Set(c, ThemeRole.Success, Color.FromArgb(0, 128, 0));
			Set(c, ThemeRole.Warning, Color.FromArgb(200, 120, 0));
			Set(c, ThemeRole.Error, Color.FromArgb(200, 0, 0));
			Set(c, ThemeRole.Info, accent);
			Set(c, ThemeRole.Shadow, Color.FromArgb(dark ? 128 : 64, 0, 0, 0));
			Set(c, ThemeRole.Overlay, Color.FromArgb(dark ? 140 : 96, 0, 0, 0));
			return new MxTheme(s.Name, s.Family, s.Kind, c, s.Shape, null, s.Style);
		}

		private static void Set(Color[] colors, ThemeRole role, Color color)
		{
			colors[(int)role] = color;
		}
	}
}
