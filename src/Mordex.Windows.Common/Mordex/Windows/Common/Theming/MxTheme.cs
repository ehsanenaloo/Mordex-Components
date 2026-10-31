using System;
using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// A look for the controls: a color for each <see cref="ThemeRole"/> and the shape settings. A theme never changes
	/// after it is made; <see cref="With"/> gives a changed copy. Paint code does not read a theme directly. It reads the
	/// <see cref="ResolvedTheme"/> of the theme, which holds ready brushes and pens.
	/// </summary>
	public sealed class MxTheme
	{
		/// <summary>The number of roles. A color array for a theme has this length.</summary>
		public static readonly int RoleCount = Enum.GetValues(typeof(ThemeRole)).Length;

		private readonly Color[] _colors;

		private readonly Color[] _officePalette;

		public MxTheme(string name, string family, ThemeKind kind, Color[] colors, ThemeShape shape, Color[] officePalette = null, ThemeStyle style = default)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("A theme needs a name.", nameof(name));
			}
			if (colors == null)
			{
				throw new ArgumentNullException(nameof(colors));
			}
			if (colors.Length != RoleCount)
			{
				throw new ArgumentException("A theme needs one color for each role (" + RoleCount + ").", nameof(colors));
			}
			Name = name;
			Family = string.IsNullOrWhiteSpace(family) ? name : family;
			Kind = kind;
			Shape = shape;
			Style = style;
			_colors = (Color[])colors.Clone();
			if (_colors[(int)ThemeRole.ToolbarText].IsEmpty)
			{
				_colors[(int)ThemeRole.ToolbarText] = _colors[(int)ThemeRole.Text];
			}
			_officePalette = officePalette == null ? null : (Color[])officePalette.Clone();
		}

		public string Name { get; }

		/// <summary>The group that the light and the dark theme of one design share, for example "Windows 11".</summary>
		public string Family { get; }

		public ThemeKind Kind { get; }

		public ThemeShape Shape { get; }

		/// <summary>How the theme draws faces and edges.</summary>
		public ThemeStyle Style { get; }

		/// <summary>True when the theme carries the slot palette of a classic Office look.</summary>
		public bool HasOfficePalette => _officePalette != null;

		public Color GetColor(ThemeRole role)
		{
			return _colors[(int)role];
		}

		/// <summary>A copy of the slot palette of a classic Office look, or null.</summary>
		public Color[] GetOfficePalette()
		{
			return _officePalette == null ? null : (Color[])_officePalette.Clone();
		}

		internal Color[] ColorsNoCopy => _colors;

		internal Color[] OfficePaletteNoCopy => _officePalette;

		/// <summary>A copy of this theme with one color changed.</summary>
		public MxTheme With(ThemeRole role, Color color)
		{
			Color[] colors = (Color[])_colors.Clone();
			colors[(int)role] = color;
			return new MxTheme(Name, Family, Kind, colors, Shape, _officePalette, Style);
		}

		/// <summary>A copy of this theme with another accent color. The hot, pressed and subtle accent follow it.</summary>
		public MxTheme WithAccent(Color accent)
		{
			Color[] colors = (Color[])_colors.Clone();
			bool dark = Kind == ThemeKind.Dark;
			colors[(int)ThemeRole.Accent] = accent;
			colors[(int)ThemeRole.AccentHot] = dark ? Lighten(accent, 0.15f) : Darken(accent, 0.12f);
			colors[(int)ThemeRole.AccentPressed] = dark ? Lighten(accent, 0.3f) : Darken(accent, 0.24f);
			colors[(int)ThemeRole.AccentSubtle] = Blend(colors[(int)ThemeRole.Surface], accent, dark ? 0.25f : 0.15f);
			colors[(int)ThemeRole.Selection] = accent;
			colors[(int)ThemeRole.BorderFocus] = accent;
			// One text color is drawn on the accent and on its hot and pressed shades, so it must read on all three.
			Color textOnAccent = ColorContrast.BetterText(Color.White, Color.Black, accent, colors[(int)ThemeRole.AccentHot], colors[(int)ThemeRole.AccentPressed]);
			colors[(int)ThemeRole.TextOnAccent] = textOnAccent;
			colors[(int)ThemeRole.SelectionText] = textOnAccent;
			return new MxTheme(Name, Family, Kind, colors, Shape, _officePalette, Style);
		}

		public override string ToString()
		{
			return Name;
		}

		public static Color Blend(Color from, Color to, float amount)
		{
			return Color.FromArgb(
				255,
				(int)(from.R + (to.R - from.R) * amount),
				(int)(from.G + (to.G - from.G) * amount),
				(int)(from.B + (to.B - from.B) * amount));
		}

		public static Color Lighten(Color color, float amount)
		{
			return Blend(color, Color.White, amount);
		}

		public static Color Darken(Color color, float amount)
		{
			return Blend(color, Color.Black, amount);
		}

		internal static double Luminance(Color color)
		{
			return (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;
		}
	}
}
