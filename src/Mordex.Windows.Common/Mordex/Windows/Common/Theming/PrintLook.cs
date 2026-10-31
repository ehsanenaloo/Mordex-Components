using System;
using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// The rule for printing a themed control. Paper is white, so a print document uses the colors of the default theme
	/// only when that theme is light. When the theme is dark, a print document uses the system colors (black text on white
	/// paper), as it does for a control that has no theme. A color that the developer set on the control is not changed.
	/// </summary>
	public static class PrintLook
	{
		/// <summary>True when a print document may use the colors of <paramref name="theme"/>: the theme is light.</summary>
		public static bool UsesTheme(MxTheme theme)
		{
			return theme != null && theme.Kind == ThemeKind.Light;
		}

		/// <summary>True when a print document may use the colors of the default theme.</summary>
		public static bool UsesDefaultTheme()
		{
			return UsesTheme(MxThemeManager.DefaultTheme);
		}

		/// <summary>
		/// Starts a print job. When the default theme is dark, the default theme is the Standard theme (system colors) until
		/// the returned scope is disposed, so every painter that reads the default theme prints on white paper. The change is announced like any theme change, so the
		/// controls rebuild their colors, and again when the scope is disposed. Returns null when the theme is light.
		/// </summary>
		public static IDisposable Begin()
		{
			MxTheme current = MxThemeManager.DefaultTheme;
			if (UsesTheme(current))
			{
				return null;
			}
			MxTheme standard = MxThemeManager.Find("Standard");
			if (standard == null)
			{
				return null;
			}
			MxThemeManager.DefaultTheme = standard;
			return new Scope(current);
		}

		private sealed class Scope : IDisposable
		{
			private MxTheme _previous;

			internal Scope(MxTheme previous)
			{
				_previous = previous;
			}

			public void Dispose()
			{
				if (_previous != null)
				{
					MxThemeManager.DefaultTheme = _previous;
					_previous = null;
				}
			}
		}

		/// <summary>The color of a role in the default theme for a print document, or <paramref name="system"/> when the theme is dark.</summary>
		public static Color Color(ThemeRole role, Color system)
		{
			MxTheme theme = MxThemeManager.DefaultTheme;
			return UsesTheme(theme) ? theme.GetColor(role) : system;
		}
	}
}
