using System.Drawing;

namespace Mordex.Windows.Common
{
	/// <summary>How a face (button, tab, header, bar) is filled.</summary>
	public enum ThemeFaceKind
	{
		/// <summary>One gradient from the light end to the dark end.</summary>
		Gradient,
		/// <summary>A light upper half and a darker lower half with a sharp line between them (Office 2007).</summary>
		Glass
	}

	/// <summary>The glass look of push buttons.</summary>
	public enum ThemeGlassKind
	{
		None,
		/// <summary>Sharp split, gold hot glow, orange pressed glow, inner white ring (Office 2007).</summary>
		Office2007,
		/// <summary>Soft split, light gold glow, no inner ring (Office 2010 and Visual Studio 2010).</summary>
		Office2010
	}

	/// <summary>How a focused input shows its focus.</summary>
	public enum ThemeFocusKind
	{
		/// <summary>The border takes the focus color.</summary>
		Border,
		/// <summary>The border stays and an accent line lies along the bottom edge (Windows 11, Office 365).</summary>
		Underline,
		/// <summary>The border takes the focus color and a second, lighter ring lies inside it (Firefox).</summary>
		Ring
	}

	/// <summary>How the box of a check box and the circle of a radio button are drawn.</summary>
	public enum ThemeBoxLook
	{
		Default,
		/// <summary>A white box with a blue edge, a glass tint at the bottom, a gold glow when hot and an orange glow when pressed.</summary>
		Glass
	}

	/// <summary>Where the accent line of a selected tab lies.</summary>
	public enum ThemeTabLine
	{
		/// <summary>On the outer edge of the tab, away from the page.</summary>
		Outer,
		/// <summary>On the edge next to the page (an underline for tabs above their page).</summary>
		Inner
	}

	/// <summary>Values for the looks of Office 2007, Office 2010, Visual Studio 2010 and the modern themes.</summary>
	public readonly struct ThemeModernStyle
	{
		public ThemeFaceKind FaceKind { get; init; }

		/// <summary>The background of a pop-up menu. Empty takes the raised surface color.</summary>
		public Color MenuBack { get; init; }

		/// <summary>The glass look of push buttons.</summary>
		public ThemeGlassKind Glass { get; init; }

		public ThemeFocusKind InputFocus { get; init; }

		/// <summary>The width of the focus line or ring of an input, in 96 DPI pixels. 0 gives 1.</summary>
		public int InputFocusWidth { get; init; }

		public ThemeBoxLook BoxLook { get; init; }

		/// <summary>The color of the mark in a glass box. Empty takes the text color.</summary>
		public Color BoxMark { get; init; }

		public ThemeTabLine TabLine { get; init; }

		/// <summary>True when the tabs above their page are trapezoids with curved feet (Chrome).</summary>
		public bool TrapezoidTabs { get; init; }

		/// <summary>The thickness of the tab line in 96 DPI pixels. 0 gives 2.</summary>
		public int TabLineWidth { get; init; }
	}
}
