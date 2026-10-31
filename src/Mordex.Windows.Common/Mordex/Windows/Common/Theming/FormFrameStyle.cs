using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>The shape of the window frame that a theme asks for.</summary>
	public enum FormFrameKind
	{
		/// <summary>The frame of Windows, with no change. Used by the Standard theme.</summary>
		System,
		/// <summary>The frame of Windows with the caption color and the dark or light title bar of the theme. Flat themes.</summary>
		Native,
		/// <summary>Windows XP: a rounded blue caption with a thick colored frame.</summary>
		Luna,
		/// <summary>Windows 95 to 2000: a gradient caption in a 3D frame.</summary>
		Classic,
		/// <summary>Office 2007: a light glass caption with rounded top corners.</summary>
		Glass,
		/// <summary>Office 2010: a flat gradient caption.</summary>
		Gradient,
		/// <summary>Chrome and Firefox: a flat caption that can hold the tab strip of a tab control.</summary>
		Browser
	}

	/// <summary>The measures of a window frame, in pixels at 96 DPI, and the kind of frame a theme uses.</summary>
	public readonly struct FormFrameStyle
	{
		public FormFrameKind Kind { get; init; }

		/// <summary>The height of the caption, from the top edge of the window.</summary>
		public int CaptionHeight { get; init; }

		/// <summary>The width of the frame on the left, right and bottom.</summary>
		public int BorderWidth { get; init; }

		/// <summary>The radius of the top corners. 0 means square corners.</summary>
		public int TopRadius { get; init; }

		/// <summary>True when the frame is painted by the component and not by Windows.</summary>
		public bool IsCustom => Kind != FormFrameKind.System && Kind != FormFrameKind.Native;

		public static FormFrameStyle System => new FormFrameStyle { Kind = FormFrameKind.System };

		/// <summary>The frame style of a theme, chosen by its family.</summary>
		public static FormFrameStyle For(MxTheme theme)
		{
			if (theme == null)
			{
				return System;
			}
			switch (theme.Family)
			{
			case "Windows XP":
			case "Office XP":
			case "Office 2003":
			case "Visual Studio 2005":
				return new FormFrameStyle { Kind = FormFrameKind.Luna, CaptionHeight = 30, BorderWidth = 4, TopRadius = 8 };
			case "Windows Classic":
				return new FormFrameStyle { Kind = FormFrameKind.Classic, CaptionHeight = 26, BorderWidth = 4, TopRadius = 0 };
			case "Office 2007":
				return new FormFrameStyle { Kind = FormFrameKind.Glass, CaptionHeight = 30, BorderWidth = 4, TopRadius = 6 };
			case "Office 2010":
			case "Visual Studio 2010":
				return new FormFrameStyle { Kind = FormFrameKind.Gradient, CaptionHeight = 30, BorderWidth = 4, TopRadius = 0 };
			case "Chrome":
			case "Firefox":
				return new FormFrameStyle { Kind = FormFrameKind.Browser, CaptionHeight = 40, BorderWidth = 2, TopRadius = 0 };
			case "Standard":
				return System;
			default:
				return new FormFrameStyle { Kind = FormFrameKind.Native };
			}
		}
	}

	/// <summary>The colors a custom frame paints with, taken from the roles and the slot palette of a theme.</summary>
	internal readonly struct FormFrameColors
	{
		internal Color CaptionTop { get; init; }

		internal Color CaptionMiddle { get; init; }

		internal Color CaptionBottom { get; init; }

		internal Color Frame { get; init; }

		internal Color FrameOuter { get; init; }

		internal Color FrameInner { get; init; }

		internal Color Text { get; init; }

		internal Color TextShadow { get; init; }

		internal Color Glyph { get; init; }

		internal static FormFrameColors Create(MxTheme theme, FormFrameKind kind, bool active)
		{
			Color caption = theme.GetColor(active ? ThemeRole.Caption : ThemeRole.CaptionInactive);
			if (kind == FormFrameKind.Luna && theme.Family != "Windows XP")
			{
				// Office XP, Office 2003 and Visual Studio 2005 run in the Luna frame of Windows XP, in the same three schemes.
				bool olive = theme.Name.EndsWith("Olive Green", System.StringComparison.Ordinal);
				bool silver = theme.Name.EndsWith("Silver", System.StringComparison.Ordinal);
				caption = active
					? (olive ? Color.FromArgb(0x91, 0xA0, 0x75) : silver ? Color.FromArgb(0xA6, 0xA5, 0xBF) : Color.FromArgb(0x00, 0x54, 0xE3))
					: (olive ? Color.FromArgb(0xB6, 0xC3, 0x92) : silver ? Color.FromArgb(0xD3, 0xD4, 0xDD) : Color.FromArgb(0x7A, 0x96, 0xDF));
			}
			Color[] palette = theme.OfficePaletteNoCopy;
			switch (kind)
			{
			case FormFrameKind.Luna:
			{
				Color text = MxTheme.Luminance(caption) > 0.7 ? Color.Black : Color.White;
				return new FormFrameColors
				{
					CaptionTop = MxTheme.Lighten(caption, 0.28f),
					CaptionMiddle = caption,
					CaptionBottom = MxTheme.Darken(caption, 0.1f),
					Frame = MxTheme.Darken(caption, 0.04f),
					FrameOuter = MxTheme.Darken(caption, 0.32f),
					FrameInner = MxTheme.Lighten(caption, 0.12f),
					Text = text,
					TextShadow = text == Color.White ? MxTheme.Darken(caption, 0.62f) : Color.Empty,
					Glyph = Color.White
				};
			}
			case FormFrameKind.Classic:
			{
				// The colors of Windows 98: a navy to blue caption, gray when inactive. The system colors of today are pale.
				Color left = active ? Color.FromArgb(10, 36, 106) : Color.FromArgb(128, 128, 128);
				Color right = active ? Color.FromArgb(166, 202, 240) : Color.FromArgb(192, 192, 192);
				return new FormFrameColors
				{
					CaptionTop = left,
					CaptionMiddle = left,
					CaptionBottom = right,
					Frame = Color.FromArgb(212, 208, 200),
					FrameOuter = Color.FromArgb(64, 64, 64),
					FrameInner = Color.White,
					Text = active ? Color.White : Color.FromArgb(212, 208, 200),
					TextShadow = Color.Empty,
					Glyph = Color.Black
				};
			}
			case FormFrameKind.Glass:
			{
				Color top = palette != null ? palette[active ? OfficeSlot.ActiveFormCaptionColor1 : OfficeSlot.InactiveFormCaptionColor1] : caption;
				Color bottom = palette != null ? palette[active ? OfficeSlot.ActiveFormCaptionColor5 : OfficeSlot.InactiveFormCaptionColor3] : caption;
				Color border = palette != null ? palette[active ? OfficeSlot.ActiveFormBorder : OfficeSlot.InactiveFormBorder] : theme.GetColor(ThemeRole.Border);
				return new FormFrameColors
				{
					CaptionTop = top,
					CaptionMiddle = MxTheme.Blend(top, bottom, 0.5f),
					CaptionBottom = bottom,
					Frame = bottom,
					FrameOuter = border,
					FrameInner = MxTheme.Lighten(top, 0.5f),
					Text = theme.Kind == ThemeKind.Dark ? Color.White : Color.FromArgb(62, 62, 62),
					TextShadow = Color.Empty,
					Glyph = theme.Kind == ThemeKind.Dark ? Color.White : Color.FromArgb(62, 62, 62)
				};
			}
			case FormFrameKind.Browser:
			{
				// The default Chrome and Firefox title bars are one flat color: the color of the tab strip.
				Color left = caption;
				Color right = caption;
				Color text = theme.GetColor(ThemeRole.Text);
				return new FormFrameColors
				{
					CaptionTop = left,
					CaptionMiddle = MxTheme.Blend(left, right, 0.5f),
					CaptionBottom = right,
					Frame = caption,
					FrameOuter = theme.GetColor(ThemeRole.Border),
					FrameInner = caption,
					Text = text,
					TextShadow = Color.Empty,
					Glyph = text
				};
			}
			default:
			{
				Color top = palette != null ? palette[active ? OfficeSlot.ActiveFormCaptionColor1 : OfficeSlot.InactiveFormCaptionColor1] : caption;
				Color bottom = palette != null ? palette[active ? OfficeSlot.ActiveFormCaptionColor2 : OfficeSlot.InactiveFormCaptionColor2] : MxTheme.Darken(caption, 0.1f);
				Color border = palette != null ? palette[active ? OfficeSlot.ActiveFormBorder : OfficeSlot.InactiveFormBorder] : theme.GetColor(ThemeRole.Border);
				bool dark = MxTheme.Luminance(top) < 0.45;
				return new FormFrameColors
				{
					CaptionTop = top,
					CaptionMiddle = MxTheme.Blend(top, bottom, 0.5f),
					CaptionBottom = bottom,
					Frame = bottom,
					FrameOuter = border,
					FrameInner = MxTheme.Lighten(top, 0.3f),
					Text = dark ? Color.White : Color.FromArgb(40, 40, 40),
					TextShadow = Color.Empty,
					Glyph = dark ? Color.White : Color.FromArgb(40, 40, 40)
				};
			}
			}
		}
	}
}
