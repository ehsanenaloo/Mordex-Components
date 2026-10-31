namespace Mordex.Windows.Common
{
	public enum ThemeKind
	{
		Light,
		Dark
	}

	/// <summary>The shape settings of a theme, in pixels at 96 DPI.</summary>
	public readonly struct ThemeShape
	{
		public ThemeShape(int cornerRadius, int borderWidth, bool isFlat, int focusRingWidth, bool hasShadow)
		{
			CornerRadius = cornerRadius;
			BorderWidth = borderWidth;
			IsFlat = isFlat;
			FocusRingWidth = focusRingWidth;
			HasShadow = hasShadow;
		}

		/// <summary>The radius of the corners of buttons, inputs and cards. 0 means square corners.</summary>
		public int CornerRadius { get; }

		public int BorderWidth { get; }

		/// <summary>True when the theme paints solid fills, with no gradient.</summary>
		public bool IsFlat { get; }

		public int FocusRingWidth { get; }

		/// <summary>True when popups and floating panels have a shadow.</summary>
		public bool HasShadow { get; }

		public static ThemeShape Classic => new ThemeShape(0, 1, false, 1, true);

		public static ThemeShape Flat => new ThemeShape(0, 1, true, 1, false);

		public static ThemeShape Rounded => new ThemeShape(4, 1, true, 2, true);
	}
}
