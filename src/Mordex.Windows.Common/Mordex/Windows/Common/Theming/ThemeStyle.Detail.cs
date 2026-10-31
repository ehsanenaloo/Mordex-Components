using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>A font that a theme asks the controls to use when the developer did not choose one.</summary>
	public readonly struct ThemeFont
	{
		public ThemeFont(string family, float size, FontStyle style = FontStyle.Regular)
		{
			Family = family;
			Size = size;
			Style = style;
		}

		public string Family { get; }

		/// <summary>The size in points.</summary>
		public float Size { get; }

		public FontStyle Style { get; }
	}

	/// <summary>Fonts made for the themes. A font is made once and kept for the life of the process; controls hold it.</summary>
	public static class ThemeFonts
	{
		private static readonly System.Collections.Generic.Dictionary<(string, float, FontStyle), Font> Fonts = new System.Collections.Generic.Dictionary<(string, float, FontStyle), Font>();

		/// <summary>The font of the default theme, or the default font of Windows Forms when the theme has none.</summary>
		public static Font Current
		{
			get
			{
				ThemeFont? font = MxThemeManager.DefaultTheme?.Style.Detail.Font;
				return font == null ? System.Windows.Forms.Control.DefaultFont : Get(font.Value);
			}
		}

		public static Font Get(ThemeFont font)
		{
			lock (Fonts)
			{
				(string, float, FontStyle) key = (font.Family, font.Size, font.Style);
				if (!Fonts.TryGetValue(key, out Font made))
				{
					made = new Font(font.Family, font.Size, font.Style, GraphicsUnit.Point);
					Fonts[key] = made;
				}
				return made;
			}
		}
	}

	/// <summary>How check boxes and radio buttons are drawn.</summary>
	public enum ThemeChoiceLook
	{
		/// <summary>The rounded modern box with an accent fill.</summary>
		Default,
		/// <summary>White box, blue frame, green mark, orange ring when hot (Windows XP).</summary>
		Luna,
		/// <summary>Sunken white box with a black mark (Windows Classic).</summary>
		Classic,
		/// <summary>White box with a flat frame and a dark blue mark (Office XP and 2003).</summary>
		Flat
	}

	/// <summary>How the drop-down button of a combo box and the buttons of an up-down box are drawn.</summary>
	public enum ThemeDropButtonLook
	{
		Default,
		/// <summary>A rounded light blue button with a dark blue arrow (Windows XP).</summary>
		Luna,
		/// <summary>A raised 3D button with a black arrow (Windows Classic).</summary>
		Classic,
		/// <summary>A flat button that shows a frame when hot (Office XP).</summary>
		Flat,
		/// <summary>A gradient button in the toolbar colors (Office 2003).</summary>
		Gradient
	}

	/// <summary>How pop-up menus are drawn in the older looks.</summary>
	public enum ThemeMenuLook
	{
		Default,
		/// <summary>White menu, light gutter, solid blue selection with white text, thin gray border.</summary>
		Xp,
		/// <summary>Darker icon column, light blue selection with a blue frame.</summary>
		OfficeXp,
		/// <summary>Gradient icon column, rounded orange gradient selection with a dark blue frame.</summary>
		Office2003,
		/// <summary>3D raised menu, full row navy selection with white text.</summary>
		Classic
	}

	public readonly struct ThemeDetailStyle
	{
		/// <summary>The font of the theme. Null leaves the font of the application.</summary>
		public ThemeFont? Font { get; init; }

		public ThemeChoiceLook ChoiceLook { get; init; }

		/// <summary>The color of the check mark and the radio dot in the older looks. Empty takes the text color.</summary>
		public Color ChoiceMark { get; init; }

		/// <summary>The frame of a check box or radio button in the older looks. Empty takes the strong border role.</summary>
		public Color ChoiceFrame { get; init; }

		/// <summary>The ring inside the frame of a hot box (Windows XP orange). Empty draws none.</summary>
		public Color ChoiceHot { get; init; }

		public ThemeDropButtonLook DropButton { get; init; }

		public ThemeMenuLook MenuLook { get; init; }

		/// <summary>True when the group headers of a navigation bar take a tint of the caption color.</summary>
		public bool NavHeaderTint { get; init; }

		/// <summary>True when appointment frames in the day view have a gradient face.</summary>
		public bool AppointmentGradient { get; init; }

		/// <summary>The fill of the selected menu row in the Windows XP and Classic looks.</summary>
		public Color MenuSelection { get; init; }

		/// <summary>The text color of the selected menu row. Empty keeps the text color of the row.</summary>
		public Color MenuSelectionText { get; init; }

		/// <summary>The back color of the controls inside a tab page. Empty keeps the color of the window.</summary>
		public Color TabPageBack { get; init; }

		/// <summary>The role of the caption text of a group box. Null takes the text role.</summary>
		public ThemeRole? GroupTitleRole { get; init; }

		/// <summary>The frame of a group box. Empty takes the border role.</summary>
		public Color GroupFrame { get; init; }
	}
}
