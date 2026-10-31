namespace Mordex.Windows.Common
{
	/// <summary>How the application button of a ribbon looks.</summary>
	public enum RibbonAppButtonKind
	{
		/// <summary>A tab-like button in the accent color, with a gradient (Office 2010).</summary>
		FileTab,
		/// <summary>A round glass button with the icon of the application (Office 2007).</summary>
		Orb,
		/// <summary>A flat button in the accent color (Windows 11 and 10, Office 365, Visual Studio, browsers).</summary>
		FlatFile,
		/// <summary>A button in the face of the theme, with its gradient or 3D edge (Windows XP, Classic, Office 2003).</summary>
		ThemedButton
	}

	public enum RibbonTabShape
	{
		/// <summary>The selected tab is flat and has an accent line under its text.</summary>
		Underline,
		/// <summary>The selected tab is a plain box joined to the content area.</summary>
		Flat,
		/// <summary>Rounded top corners with a gradient (Office 2007).</summary>
		Rounded,
		/// <summary>A raised tab with a small corner radius (Windows XP, Office 2003) or a 3D edge (Classic).</summary>
		Beveled
	}

	public enum RibbonGroupFrameKind
	{
		/// <summary>Only a line between the groups.</summary>
		Separator,
		/// <summary>A box with a rounded outline, a gradient and a darker caption band (Office 2007).</summary>
		RoundedBox,
		/// <summary>An etched frame (Windows Classic).</summary>
		Etched
	}

	/// <summary>
	/// How a theme shapes the parts of a ribbon: the application button, the tabs, the group frames, the content area, the quick
	/// access toolbar and the galleries. Colors come from the roles of the theme; this type holds the shapes.
	/// </summary>
	public sealed class RibbonThemeStyle
	{
		public RibbonAppButtonKind AppButton { get; init; }

		public RibbonTabShape TabShape { get; init; }

		public RibbonGroupFrameKind GroupFrame { get; init; }

		/// <summary>The radius of the tab corners, button groups and galleries, in 96 DPI pixels.</summary>
		public int Radius { get; init; }

		/// <summary>True when the content area under the tabs has a vertical gradient.</summary>
		public bool ContentGradient { get; init; }

		/// <summary>True when the quick access toolbar is a rounded capsule with a gradient.</summary>
		public bool QuickAccessCapsule { get; init; }

		/// <summary>True when the caption of a group is drawn on a darker band.</summary>
		public bool GroupCaptionBand { get; init; }

		/// <summary>The colors of the application button: the palette of the file button for the Office looks, the accent for the others.</summary>
		public void GetAppButtonColors(MxTheme theme, out System.Drawing.Color light, out System.Drawing.Color dark, out System.Drawing.Color border)
		{
			System.Drawing.Color[] palette = theme.OfficePaletteNoCopy;
			if (palette != null && theme.Family == "Office 2010")
			{
				// The File tab of Office 2010 is a strong color: blue, steel gray or charcoal.
				bool silver = theme.Name.EndsWith("Silver", System.StringComparison.Ordinal);
				bool black = theme.Name.EndsWith("Black", System.StringComparison.Ordinal);
				light = black ? System.Drawing.Color.FromArgb(84, 84, 84) : silver ? System.Drawing.Color.FromArgb(124, 133, 145) : System.Drawing.Color.FromArgb(66, 114, 178);
				dark = black ? System.Drawing.Color.FromArgb(36, 36, 36) : silver ? System.Drawing.Color.FromArgb(78, 87, 99) : System.Drawing.Color.FromArgb(31, 74, 135);
				border = black ? System.Drawing.Color.FromArgb(20, 20, 20) : silver ? System.Drawing.Color.FromArgb(58, 66, 77) : System.Drawing.Color.FromArgb(22, 55, 104);
				return;
			}
			if (palette != null && theme.Family == "Office 2007")
			{
				// The orb of Office 2007 is blue glass in the Blue scheme and gray in Silver and Black.
				bool silver = theme.Name.EndsWith("Silver", System.StringComparison.Ordinal);
				bool black = theme.Name.EndsWith("Black", System.StringComparison.Ordinal);
				// The orb of Office 2007 is white glass with a rim that follows the scheme: blue, gray or dark.
				light = black ? System.Drawing.Color.FromArgb(238, 238, 240) : System.Drawing.Color.FromArgb(255, 255, 255);
				dark = black ? System.Drawing.Color.FromArgb(178, 178, 184) : silver ? System.Drawing.Color.FromArgb(214, 216, 224) : System.Drawing.Color.FromArgb(200, 212, 232);
				border = black ? System.Drawing.Color.FromArgb(72, 72, 78) : silver ? System.Drawing.Color.FromArgb(138, 142, 158) : System.Drawing.Color.FromArgb(104, 134, 182);
				return;
			}
			if (palette != null)
			{
				// The palette holds pale colors for the file menu; the button takes its hue from the border color of that menu.
				System.Drawing.Color mid = palette[OfficeSlot.FileMenuButtonBorder];
				light = MxTheme.Lighten(mid, 0.35f);
				dark = MxTheme.Darken(mid, 0.25f);
				border = MxTheme.Darken(mid, 0.5f);
				return;
			}
			dark = theme.GetColor(ThemeRole.Accent);
			light = MxTheme.Lighten(dark, 0.25f);
			border = MxTheme.Darken(dark, 0.3f);
		}

		/// <summary>The color of the text on the application button.</summary>
		public System.Drawing.Color GetAppButtonTextColor(MxTheme theme)
		{
			switch (AppButton)
			{
			case RibbonAppButtonKind.ThemedButton:
				return theme.GetColor(ThemeRole.Text);
			case RibbonAppButtonKind.FlatFile:
				return theme.GetColor(ThemeRole.TextOnAccent);
			default:
				GetAppButtonColors(theme, out _, out System.Drawing.Color dark, out _);
				return MxTheme.Luminance(dark) < 0.6 ? System.Drawing.Color.White : System.Drawing.Color.Black;
			}
		}

		private static readonly RibbonThemeStyle Modern = new RibbonThemeStyle { AppButton = RibbonAppButtonKind.FlatFile, TabShape = RibbonTabShape.Underline, GroupFrame = RibbonGroupFrameKind.Separator };

		private static readonly RibbonThemeStyle Office2007 = new RibbonThemeStyle
		{
			AppButton = RibbonAppButtonKind.Orb, TabShape = RibbonTabShape.Rounded, GroupFrame = RibbonGroupFrameKind.RoundedBox,
			Radius = 3, ContentGradient = true, QuickAccessCapsule = true, GroupCaptionBand = true
		};

		private static readonly RibbonThemeStyle Office2010 = new RibbonThemeStyle
		{
			AppButton = RibbonAppButtonKind.FileTab, TabShape = RibbonTabShape.Flat, GroupFrame = RibbonGroupFrameKind.Separator,
			Radius = 2, ContentGradient = true
		};

		private static readonly RibbonThemeStyle Luna = new RibbonThemeStyle
		{
			AppButton = RibbonAppButtonKind.ThemedButton, TabShape = RibbonTabShape.Beveled, GroupFrame = RibbonGroupFrameKind.Separator,
			Radius = 3, ContentGradient = true
		};

		private static readonly RibbonThemeStyle Classic = new RibbonThemeStyle
		{
			AppButton = RibbonAppButtonKind.ThemedButton, TabShape = RibbonTabShape.Beveled, GroupFrame = RibbonGroupFrameKind.Etched
		};

		/// <summary>The ribbon style of a theme, chosen by its family. Themes that the suite does not know get the flat modern style.</summary>
		public static RibbonThemeStyle For(MxTheme theme)
		{
			switch (theme == null ? null : theme.Family)
			{
			case "Office 2007":
				return Office2007;
			case "Office 2010":
			case "Visual Studio 2010":
				return Office2010;
			case "Windows XP":
			case "Office 2003":
			case "Visual Studio 2005":
				return Luna;
			case "Windows Classic":
			case "Office XP":
				return Classic;
			default:
				return Modern;
			}
		}
	}
}
