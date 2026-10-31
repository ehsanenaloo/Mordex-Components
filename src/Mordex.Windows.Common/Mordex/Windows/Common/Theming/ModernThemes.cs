using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// The newer looks: Windows 11, Windows 10, Visual Studio 2022, Office 365, Chrome and Firefox, in light and dark where
	/// the product has both. Each look is a small palette; the other roles come from it by blending, so a new look is one
	/// line of data.
	/// </summary>
	/// <remarks>
	/// The colors are taken from the published design resources and the look of the products (Fluent theme resources for
	/// Windows 11, the Win32 flat controls for Windows 10, the Visual Studio 2022 themes, the Office 365 neutral palette,
	/// Chrome's Material look and Firefox's Proton look). They are close, not copies; a product may change its colors with a release.
	/// </remarks>
	internal static class ModernThemes
	{
		private sealed class Palette
		{
			public string Name;
			public string Family;
			public ThemeKind Kind;
			public ThemeShape Shape;
			public int Surface;
			public int Panel;
			public int Raised;
			public int Input;
			public int Text;
			public int Muted;
			public int Border;
			public int BorderStrong;
			public int Hover;
			public int Pressed;
			public int Accent;
			public int AccentHot;
			public int AccentPressed;
			public int TextOnAccent;
			public int Error;
			public int Warning;
			public int Success;
			// 0 gives the panel color. The Visual Studio status bar uses the accent color.
			public int StatusBar;
			// 0 gives the raised color for the selected tab and the usual caption color.
			public int TabActive;
			public int Caption;
			// 0 gives the panel color, or the surface color for a browser.
			public int Toolbar;
			// Null gives the text color.
			public int? ToolbarText;
		}

		internal static IEnumerable<MxTheme> All()
		{
			ThemeShape rounded = new ThemeShape(4, 1, true, 2, true);
			ThemeShape squareFlat = new ThemeShape(0, 1, true, 1, false);
			ThemeShape vs = new ThemeShape(0, 1, true, 1, true);
			ThemeShape browser = new ThemeShape(8, 1, true, 2, true);

			yield return Make(new Palette
			{
				Name = "Windows 11 Light", Family = "Windows 11", Kind = ThemeKind.Light, Shape = rounded,
				Surface = 0xF3F3F3, Panel = 0xEEEEEE, Raised = 0xFFFFFF, Input = 0xFFFFFF, Text = 0x1A1A1A, Muted = 0x606060,
				Border = 0xE5E5E5, BorderStrong = 0x8A8A8A, Hover = 0xF5F5F5, Pressed = 0xE6E6E6,
				Accent = 0x0067C0, AccentHot = 0x1975C5, AccentPressed = 0x3183CA, TextOnAccent = 0xFFFFFF,
				Error = 0xC42B1C, Warning = 0x9D5D00, Success = 0x0F7B0F
			});
			yield return Make(new Palette
			{
				Name = "Windows 11 Dark", Family = "Windows 11", Kind = ThemeKind.Dark, Shape = rounded,
				Surface = 0x202020, Panel = 0x1C1C1C, Raised = 0x2B2B2B, Input = 0x2D2D2D, Text = 0xFFFFFF, Muted = 0xC8C8C8,
				Border = 0x3A3A3A, BorderStrong = 0x9A9A9A, Hover = 0x323232, Pressed = 0x272727,
				Accent = 0x60CDFF, AccentHot = 0x57B9E5, AccentPressed = 0x4DA5CC, TextOnAccent = 0x000000,
				Error = 0xFF99A4, Warning = 0xFCE100, Success = 0x6CCB5F
			});
			yield return Make(new Palette
			{
				Name = "Windows 10 Light", Family = "Windows 10", Kind = ThemeKind.Light, Shape = squareFlat,
				Surface = 0xF0F0F0, Panel = 0xE6E6E6, Raised = 0xFFFFFF, Input = 0xFFFFFF, Text = 0x000000, Muted = 0x666666,
				Border = 0xADADAD, BorderStrong = 0x7A7A7A, Hover = 0xE5F1FB, Pressed = 0xCCE4F7,
				Accent = 0x0070C8, AccentHot = 0x1A7FD0, AccentPressed = 0x005499, TextOnAccent = 0xFFFFFF,
				Error = 0xE81123, Warning = 0xF7630C, Success = 0x107C10
			});
			yield return Make(new Palette
			{
				Name = "Windows 10 Dark", Family = "Windows 10", Kind = ThemeKind.Dark, Shape = squareFlat,
				// Measured on File Explorer in dark mode (Windows 10 1809): 202020 ribbon and content, 191919 navigation pane
				// and address box, 505050 box lines, 333333 hot and selected rows.
				Surface = 0x202020, Panel = 0x191919, Raised = 0x333333, Input = 0x191919, Text = 0xFFFFFF, Muted = 0xA0A0A0,
				Border = 0x505050, BorderStrong = 0x9A9A9A, Hover = 0x333333, Pressed = 0x4D4D4D,
				Accent = 0x0070C8, AccentHot = 0x1A7FD0, AccentPressed = 0x005499, TextOnAccent = 0xFFFFFF,
				Error = 0xFF4343, Warning = 0xF7630C, Success = 0x6CCB5F
			});
			yield return Make(new Palette
			{
				Name = "Visual Studio 2022 Light", Family = "Visual Studio 2022", Kind = ThemeKind.Light, Shape = vs,
				Surface = 0xF5F5F5, Panel = 0xEEEEF2, Raised = 0xFFFFFF, Input = 0xFFFFFF, Text = 0x1E1E1E, Muted = 0x6B6B6B,
				Border = 0xCCCEDB, BorderStrong = 0x9B9CA6, Hover = 0xC9DEF5, Pressed = 0xA9C9EE,
				Accent = 0x0078D4, AccentHot = 0x1C86DB, AccentPressed = 0x005A9E, TextOnAccent = 0xFFFFFF,
				Error = 0xE51400, Warning = 0xC27D1A, Success = 0x388A34, StatusBar = 0x0078D4
			});
			yield return Make(new Palette
			{
				Name = "Visual Studio 2022 Dark", Family = "Visual Studio 2022", Kind = ThemeKind.Dark, Shape = vs,
				Surface = 0x1F1F1F, Panel = 0x252526, Raised = 0x2D2D30, Input = 0x333337, Text = 0xF1F1F1, Muted = 0x9B9B9B,
				Border = 0x3F3F46, BorderStrong = 0x686868, Hover = 0x3E3E40, Pressed = 0x094771,
				Accent = 0x007ACC, AccentHot = 0x1C97EA, AccentPressed = 0x005A9E, TextOnAccent = 0xFFFFFF,
				Error = 0xF14C4C, Warning = 0xCCA700, Success = 0x4EC9B0, StatusBar = 0x007ACC
			});
			yield return Make(new Palette
			{
				Name = "Visual Studio 2022 Blue", Family = "Visual Studio 2022", Kind = ThemeKind.Light, Shape = vs,
				// Measured on a Visual Studio 2022 Blue window: blue gray chrome, pale tool windows, a yellow selected tab and
				// the dark blue of its glyphs as the accent.
				Surface = 0xF7F9FE, Panel = 0xCCD5F0, Raised = 0xFFFFFF, Input = 0xFCFCFC, Text = 0x1E1E1E, Muted = 0x6B6F7A,
				Border = 0x8591A2, BorderStrong = 0x5D6B99, Hover = 0xFDF4BF, Pressed = 0xFFE8A6,
				Accent = 0x0063B1, AccentHot = 0x1A72BB, AccentPressed = 0x004E8C, TextOnAccent = 0xFFFFFF,
				Error = 0xE51400, Warning = 0xC27D1A, Success = 0x388A34, StatusBar = 0x40508D, TabActive = 0xF5CC84, Caption = 0xCCD5F0
			});
			yield return Make(new Palette
			{
				Name = "Office 365 Colorful", Family = "Office 365", Kind = ThemeKind.Light, Shape = squareFlat,
				// Measured on the Office apps of today: neutral gray window F0F0F0 and the ribbon on a white card.
				Surface = 0xF0F0F0, Panel = 0xEBEBEB, Raised = 0xFFFFFF, Input = 0xFFFFFF, Text = 0x323130, Muted = 0x605E5C,
				Border = 0xC8C8C8, BorderStrong = 0x8A8A8A, Hover = 0xE0E0E0, Pressed = 0xC8C8C8, Toolbar = 0xFFFFFF,
				Accent = 0x2B579A, AccentHot = 0x3A6AB0, AccentPressed = 0x1F3F75, TextOnAccent = 0xFFFFFF,
				Error = 0xA4262C, Warning = 0xC19C00, Success = 0x107C10
			});
			yield return Make(new Palette
			{
				Name = "Office 365 White", Family = "Office 365", Kind = ThemeKind.Light, Shape = squareFlat,
				// Measured on the White theme picture of the Office help pages: blue gray window, the ribbon on a white card.
				Surface = 0xE9EEF2, Panel = 0xE9EEF2, Raised = 0xFFFFFF, Input = 0xFFFFFF, Text = 0x323130, Muted = 0x605E5C,
				Border = 0xD0D5D8, BorderStrong = 0x8A8886, Hover = 0xDDE2E6, Pressed = 0xC8CDD1, Toolbar = 0xFFFFFF,
				Accent = 0x0F6CBD, AccentHot = 0x115EA3, AccentPressed = 0x0C3B5E, TextOnAccent = 0xFFFFFF,
				Error = 0xA4262C, Warning = 0xC19C00, Success = 0x107C10
			});
			yield return Make(new Palette
			{
				Name = "Office 365 Dark Gray", Family = "Office 365", Kind = ThemeKind.Dark, Shape = squareFlat,
				// Measured on the Dark Gray theme picture of the Office help pages: window and document area.
				Surface = 0x54585B, Panel = 0x53585C, Raised = 0x505050, Input = 0x2B2B2B, Text = 0xFFFFFF, Muted = 0xD2D0CE,
				Border = 0x6B6B6B, BorderStrong = 0xA0A0A0, Hover = 0x5C5C5C, Pressed = 0x6B6B6B, Toolbar = 0xBDBDBD, ToolbarText = 0x000000,
				Accent = 0x6CB8F6, AccentHot = 0x8CC8F8, AccentPressed = 0x4F9FDE, TextOnAccent = 0x000000,
				Error = 0xFF6B6B, Warning = 0xFCE100, Success = 0x6CCB5F
			});
			yield return Make(new Palette
			{
				Name = "Office 365 Black", Family = "Office 365", Kind = ThemeKind.Dark, Shape = squareFlat,
				// Measured on the Black theme picture of the Office help pages: dark blue black window, the ribbon and the
				// boxes on a 292929 card with 666666 edges.
				Surface = 0x1C2227, Panel = 0x1A2127, Raised = 0x292929, Input = 0x292929, Text = 0xFFFFFF, Muted = 0xD2D0CE,
				Border = 0x666666, BorderStrong = 0x8A8A8A, Hover = 0x404040, Pressed = 0x4D4D4D, Toolbar = 0x292929,
				Accent = 0x6CB8F6, AccentHot = 0x8CC8F8, AccentPressed = 0x4F9FDE, TextOnAccent = 0x000000,
				Error = 0xFF6B6B, Warning = 0xFCE100, Success = 0x6CCB5F
			});
			yield return Make(new Palette
			{
				Name = "Chrome Light", Family = "Chrome", Kind = ThemeKind.Light, Shape = browser,
				// Measured on a Chrome window of Windows 11: tab strip E7EAED, white tab and toolbar, DBDCDD line below.
				Surface = 0xFFFFFF, Panel = 0xE7EAED, Raised = 0xFFFFFF, Input = 0xFFFFFF, Text = 0x202124, Muted = 0x5F6368,
				Border = 0xDBDCDD, BorderStrong = 0x9AA0A6, Hover = 0xF1F3F4, Pressed = 0xE8EAED,
				Accent = 0x1A73E8, AccentHot = 0x1B66C9, AccentPressed = 0x185ABC, TextOnAccent = 0xFFFFFF,
				Error = 0xD93025, Warning = 0xF9AB00, Success = 0x188038
			});
			yield return Make(new Palette
			{
				Name = "Chrome Dark", Family = "Chrome", Kind = ThemeKind.Dark, Shape = browser,
				// Measured on Chrome windows of Windows 10 in dark mode: tab strip 202124, tab and toolbar 323639, address box
				// 282C2F, 4F5356 line below the toolbar.
				Surface = 0x323639, Panel = 0x202124, Raised = 0x3C4043, Input = 0x282C2F, Text = 0xE8EAED, Muted = 0x9AA0A6,
				Border = 0x4F5356, BorderStrong = 0x80868B, Hover = 0x3C4043, Pressed = 0x4A4D51,
				Accent = 0x8AB4F8, AccentHot = 0xAECBFA, AccentPressed = 0x669DF6, TextOnAccent = 0x202124,
				Error = 0xF28B82, Warning = 0xFDD663, Success = 0x81C995
			});
			yield return Make(new Palette
			{
				Name = "Firefox Light", Family = "Firefox", Kind = ThemeKind.Light, Shape = browser,
				Surface = 0xF9F9FB, Panel = 0xF0F0F4, Raised = 0xFFFFFF, Input = 0xFFFFFF, Text = 0x15141A, Muted = 0x5B5B66,
				Border = 0xCFCFD8, BorderStrong = 0x8F8F9D, Hover = 0xE0E0E6, Pressed = 0xCFCFD8,
				Accent = 0x0061E0, AccentHot = 0x0054C1, AccentPressed = 0x0047A3, TextOnAccent = 0xFFFFFF,
				Error = 0xC50042, Warning = 0xA47F00, Success = 0x058B00
			});
			yield return Make(new Palette
			{
				Name = "Firefox Dark", Family = "Firefox", Kind = ThemeKind.Dark, Shape = browser,
				Surface = 0x2B2A33, Panel = 0x1C1B22, Raised = 0x42414D, Input = 0x1C1B22, Text = 0xFBFBFE, Muted = 0xBFBFC9,
				Border = 0x52525E, BorderStrong = 0x8F8F9D, Hover = 0x52525E, Pressed = 0x5B5B66,
				Accent = 0x00DDFF, AccentHot = 0x80EBFF, AccentPressed = 0xB1F2FF, TextOnAccent = 0x15141A,
				Error = 0xFF6A75, Warning = 0xFFD567, Success = 0x54FFBD
			});
		}

		private static Color C(int rgb)
		{
			return Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
		}

		private static MxTheme Make(Palette p)
		{
			bool dark = p.Kind == ThemeKind.Dark;
			Color surface = C(p.Surface);
			Color panel = C(p.Panel);
			Color text = C(p.Text);
			Color accent = C(p.Accent);
			Color border = C(p.Border);
			Color[] c = new Color[MxTheme.RoleCount];
			c[(int)ThemeRole.Surface] = surface;
			c[(int)ThemeRole.SurfaceAlt] = panel;
			c[(int)ThemeRole.SurfaceRaised] = C(p.Raised);
			c[(int)ThemeRole.SurfaceHot] = C(p.Hover);
			c[(int)ThemeRole.SurfacePressed] = C(p.Pressed);
			c[(int)ThemeRole.SurfaceDisabled] = MxTheme.Blend(surface, panel, 0.5f);
			c[(int)ThemeRole.InputBack] = C(p.Input);
			c[(int)ThemeRole.InputBackDisabled] = panel;
			c[(int)ThemeRole.Text] = text;
			c[(int)ThemeRole.TextMuted] = C(p.Muted);
			c[(int)ThemeRole.TextDisabled] = MxTheme.Blend(text, surface, 0.6f);
			c[(int)ThemeRole.TextOnAccent] = C(p.TextOnAccent);
			c[(int)ThemeRole.TextLink] = dark ? MxTheme.Lighten(accent, 0.25f) : accent;
			c[(int)ThemeRole.Border] = border;
			c[(int)ThemeRole.BorderStrong] = C(p.BorderStrong);
			c[(int)ThemeRole.BorderHot] = accent;
			c[(int)ThemeRole.BorderPressed] = C(p.AccentPressed);
			c[(int)ThemeRole.BorderFocus] = accent;
			c[(int)ThemeRole.BorderDisabled] = MxTheme.Blend(border, surface, 0.5f);
			c[(int)ThemeRole.Accent] = accent;
			c[(int)ThemeRole.AccentHot] = C(p.AccentHot);
			c[(int)ThemeRole.AccentPressed] = C(p.AccentPressed);
			c[(int)ThemeRole.AccentSubtle] = MxTheme.Blend(surface, accent, dark ? 0.25f : 0.15f);
			c[(int)ThemeRole.Selection] = accent;
			c[(int)ThemeRole.SelectionText] = C(p.TextOnAccent);
			c[(int)ThemeRole.SelectionInactive] = border;
			c[(int)ThemeRole.SelectionInactiveText] = text;
			c[(int)ThemeRole.Header] = panel;
			c[(int)ThemeRole.HeaderHot] = C(p.Hover);
			c[(int)ThemeRole.HeaderPressed] = C(p.Pressed);
			c[(int)ThemeRole.HeaderText] = text;
			c[(int)ThemeRole.GridLine] = MxTheme.Blend(border, C(p.Input), 0.4f);
			c[(int)ThemeRole.RowAlt] = MxTheme.Blend(C(p.Input), panel, 0.5f);
			c[(int)ThemeRole.ScrollTrack] = surface;
			c[(int)ThemeRole.ScrollThumb] = MxTheme.Blend(surface, text, 0.35f);
			c[(int)ThemeRole.ScrollThumbHot] = MxTheme.Blend(surface, text, 0.5f);
			c[(int)ThemeRole.ScrollThumbPressed] = MxTheme.Blend(surface, text, 0.65f);
			c[(int)ThemeRole.TabActive] = C(p.TabActive != 0 ? p.TabActive : p.Raised);
			c[(int)ThemeRole.TabInactive] = panel;
			c[(int)ThemeRole.TabHot] = C(p.Hover);
			bool accentStatus = p.StatusBar != 0;
			c[(int)ThemeRole.StatusBarBack] = accentStatus ? C(p.StatusBar) : panel;
			c[(int)ThemeRole.StatusBarText] = accentStatus ? Color.White : text;
			c[(int)ThemeRole.TooltipBack] = C(p.Raised);
			c[(int)ThemeRole.TooltipText] = text;
			bool browserFrame = p.Family == "Chrome" || p.Family == "Firefox";
			c[(int)ThemeRole.Caption] = p.Caption != 0 ? C(p.Caption) : browserFrame ? panel : surface;
			c[(int)ThemeRole.CaptionInactive] = browserFrame ? MxTheme.Blend(panel, surface, 0.5f) : panel;
			// A browser has its tabs on the darker strip and its toolbar in the lighter surface color under them.
			c[(int)ThemeRole.ToolbarBack] = p.Toolbar != 0 ? C(p.Toolbar) : browserFrame ? surface : panel;
			c[(int)ThemeRole.ToolbarText] = p.ToolbarText.HasValue ? C(p.ToolbarText.Value) : text;
			c[(int)ThemeRole.Success] = C(p.Success);
			c[(int)ThemeRole.Warning] = C(p.Warning);
			c[(int)ThemeRole.Error] = C(p.Error);
			c[(int)ThemeRole.Info] = accent;
			c[(int)ThemeRole.Shadow] = Color.FromArgb(dark ? 128 : 64, 0, 0, 0);
			c[(int)ThemeRole.Overlay] = Color.FromArgb(dark ? 140 : 96, 0, 0, 0);
			return new MxTheme(p.Name, p.Family, p.Kind, c, p.Shape, null, ThemeStyles.Modern(p.Family, accent));
		}
	}
}
