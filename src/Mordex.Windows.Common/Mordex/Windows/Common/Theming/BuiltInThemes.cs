using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>The themes that come with the suite.</summary>
	internal static class BuiltInThemes
	{
		// The newer looks come first, then the classic system look, then the older looks of the suite.
		internal static IEnumerable<MxTheme> All()
		{
			foreach (MxTheme theme in ModernThemes.All())
			{
				yield return theme;
			}
			yield return Standard();
			yield return WindowsClassic();
			foreach (MxTheme theme in ClassicThemes.All())
			{
				yield return theme;
			}
		}
		/// <summary>The colors of Windows with raised 3D edges on buttons and tabs, as the classic Windows controls draw them.</summary>
		internal static MxTheme WindowsClassic()
		{
			Color[] c = (Color[])Standard().ColorsNoCopy.Clone();
			// The Windows 2000 colors, fixed: the system colors of today are flat and light.
			Color face = Color.FromArgb(212, 208, 200);
			Color shadow = Color.FromArgb(128, 128, 128);
			Color darkShadow = Color.FromArgb(64, 64, 64);
			Color navy = Color.FromArgb(10, 36, 106);
			Set(c, ThemeRole.Surface, face);
			Set(c, ThemeRole.SurfaceAlt, face);
			Set(c, ThemeRole.SurfaceRaised, face);
			Set(c, ThemeRole.SurfaceHot, face);
			Set(c, ThemeRole.SurfacePressed, face);
			Set(c, ThemeRole.SurfaceDisabled, face);
			Set(c, ThemeRole.InputBackDisabled, face);
			Set(c, ThemeRole.TextDisabled, shadow);
			Set(c, ThemeRole.Border, shadow);
			Set(c, ThemeRole.BorderStrong, darkShadow);
			Set(c, ThemeRole.BorderHot, darkShadow);
			Set(c, ThemeRole.BorderPressed, darkShadow);
			Set(c, ThemeRole.BorderDisabled, shadow);
			Set(c, ThemeRole.BorderFocus, Color.Black);
			Set(c, ThemeRole.Accent, navy);
			Set(c, ThemeRole.AccentHot, navy);
			Set(c, ThemeRole.AccentPressed, navy);
			Set(c, ThemeRole.AccentSubtle, face);
			Set(c, ThemeRole.Selection, navy);
			Set(c, ThemeRole.SelectionText, Color.White);
			Set(c, ThemeRole.TextOnAccent, Color.White);
			Set(c, ThemeRole.SelectionInactive, shadow);
			Set(c, ThemeRole.SelectionInactiveText, Color.White);
			Set(c, ThemeRole.Header, face);
			Set(c, ThemeRole.HeaderHot, face);
			Set(c, ThemeRole.HeaderPressed, face);
			Set(c, ThemeRole.GridLine, face);
			Set(c, ThemeRole.RowAlt, Color.White);
			Set(c, ThemeRole.ScrollTrack, face);
			Set(c, ThemeRole.ScrollThumb, face);
			Set(c, ThemeRole.ScrollThumbHot, face);
			Set(c, ThemeRole.ScrollThumbPressed, face);
			Set(c, ThemeRole.TabActive, face);
			Set(c, ThemeRole.TabInactive, face);
			Set(c, ThemeRole.TabHot, face);
			Set(c, ThemeRole.ToolbarBack, face);
			Set(c, ThemeRole.StatusBarBack, face);
			Set(c, ThemeRole.Caption, navy);
			Set(c, ThemeRole.CaptionInactive, shadow);
			Set(c, ThemeRole.Info, navy);
			return new MxTheme("Windows Classic", "Windows Classic", ThemeKind.Light, c, new ThemeShape(0, 1, true, 1, true), null, ThemeStyles.WindowsClassicLook());
		}

		/// <summary>The colors of Windows, as the standard controls draw them.</summary>
		internal static MxTheme Standard()
		{
			Color[] c = new Color[MxTheme.RoleCount];
			Set(c, ThemeRole.Surface, SystemColors.Control);
			Set(c, ThemeRole.SurfaceAlt, SystemColors.ControlLight);
			Set(c, ThemeRole.SurfaceRaised, SystemColors.ControlLightLight);
			Set(c, ThemeRole.SurfaceHot, SystemColors.ControlLight);
			Set(c, ThemeRole.SurfacePressed, SystemColors.ControlDark);
			Set(c, ThemeRole.SurfaceDisabled, SystemColors.Control);
			Set(c, ThemeRole.InputBack, SystemColors.Window);
			Set(c, ThemeRole.InputBackDisabled, SystemColors.Control);
			Set(c, ThemeRole.Text, SystemColors.ControlText);
			Set(c, ThemeRole.TextMuted, SystemColors.GrayText);
			Set(c, ThemeRole.TextDisabled, SystemColors.GrayText);
			Set(c, ThemeRole.TextOnAccent, SystemColors.HighlightText);
			Set(c, ThemeRole.TextLink, Color.FromArgb(0, 0, 255));
			Set(c, ThemeRole.Border, SystemColors.ControlDark);
			Set(c, ThemeRole.BorderStrong, SystemColors.ControlDarkDark);
			Set(c, ThemeRole.BorderHot, SystemColors.Highlight);
			Set(c, ThemeRole.BorderPressed, SystemColors.Highlight);
			Set(c, ThemeRole.BorderFocus, SystemColors.Highlight);
			Set(c, ThemeRole.BorderDisabled, SystemColors.ControlDark);
			Set(c, ThemeRole.Accent, SystemColors.Highlight);
			Set(c, ThemeRole.AccentHot, MxTheme.Darken(SystemColors.Highlight, 0.12f));
			Set(c, ThemeRole.AccentPressed, MxTheme.Darken(SystemColors.Highlight, 0.24f));
			Set(c, ThemeRole.AccentSubtle, MxTheme.Blend(SystemColors.Window, SystemColors.Highlight, 0.15f));
			Set(c, ThemeRole.Selection, SystemColors.Highlight);
			Set(c, ThemeRole.SelectionText, SystemColors.HighlightText);
			Set(c, ThemeRole.SelectionInactive, SystemColors.ControlDark);
			Set(c, ThemeRole.SelectionInactiveText, SystemColors.ControlText);
			Set(c, ThemeRole.Header, SystemColors.Control);
			Set(c, ThemeRole.HeaderHot, SystemColors.ControlLight);
			Set(c, ThemeRole.HeaderPressed, SystemColors.ControlDark);
			Set(c, ThemeRole.HeaderText, SystemColors.ControlText);
			Set(c, ThemeRole.GridLine, SystemColors.ControlLight);
			Set(c, ThemeRole.RowAlt, MxTheme.Blend(SystemColors.Window, SystemColors.Control, 0.4f));
			Set(c, ThemeRole.ScrollTrack, SystemColors.ScrollBar);
			Set(c, ThemeRole.ScrollThumb, SystemColors.ControlDark);
			Set(c, ThemeRole.ScrollThumbHot, SystemColors.ControlDarkDark);
			Set(c, ThemeRole.ScrollThumbPressed, SystemColors.ControlDarkDark);
			Set(c, ThemeRole.TabActive, SystemColors.Window);
			Set(c, ThemeRole.TabInactive, SystemColors.Control);
			Set(c, ThemeRole.TabHot, SystemColors.ControlLight);
			Set(c, ThemeRole.ToolbarBack, SystemColors.Control);
			Set(c, ThemeRole.StatusBarBack, SystemColors.Control);
			Set(c, ThemeRole.StatusBarText, SystemColors.ControlText);
			Set(c, ThemeRole.TooltipBack, SystemColors.Info);
			Set(c, ThemeRole.TooltipText, SystemColors.InfoText);
			Set(c, ThemeRole.Caption, SystemColors.ActiveCaption);
			Set(c, ThemeRole.CaptionInactive, SystemColors.InactiveCaption);
			Set(c, ThemeRole.Success, Color.FromArgb(0, 128, 0));
			Set(c, ThemeRole.Warning, Color.FromArgb(200, 120, 0));
			Set(c, ThemeRole.Error, Color.FromArgb(200, 0, 0));
			Set(c, ThemeRole.Info, SystemColors.Highlight);
			Set(c, ThemeRole.Shadow, Color.FromArgb(64, 0, 0, 0));
			Set(c, ThemeRole.Overlay, Color.FromArgb(96, 0, 0, 0));
			return new MxTheme("Standard", "Standard", ThemeKind.Light, c, ThemeShape.Classic);
		}

		private static void Set(Color[] colors, ThemeRole role, Color color)
		{
			colors[(int)role] = color;
		}
	}
}
