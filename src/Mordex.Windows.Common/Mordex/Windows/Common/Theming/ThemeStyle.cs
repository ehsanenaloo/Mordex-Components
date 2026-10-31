namespace Mordex.Windows.Common
{
	public enum ThemeGradient
	{
		None,
		Vertical,
		Horizontal
	}

	public enum ThemeBevel
	{
		None,
		/// <summary>A 3D edge: light on the top and left, dark on the bottom and right. A pressed face shows it reversed.</summary>
		Raised
	}

	/// <summary>The kinds of face that can have their own corner radius.</summary>
	public enum ThemeElement
	{
		Button,
		Tab,
		Header,
		Input,
		Menu,
		Panel
	}

	/// <summary>How the edge of a part is drawn.</summary>
	public enum ThemeBorder
	{
		/// <summary>One pixel line in the border role.</summary>
		Flat,
		/// <summary>3D edge, light on the top and left.</summary>
		Raised,
		/// <summary>3D edge, dark on the top and left, as the inside of an input or a list.</summary>
		Sunken,
		/// <summary>A dark line with a light line beside it, as the edge of a group.</summary>
		Etched
	}

	public enum ThemeArrowGlyph
	{
		/// <summary>Each control draws the arrow it always drew.</summary>
		Default,
		/// <summary>Two lines that meet in a point.</summary>
		Chevron,
		/// <summary>A filled triangle.</summary>
		Triangle
	}

	public enum ThemeCheckGlyph
	{
		/// <summary>Each control draws the mark it always drew.</summary>
		Default,
		/// <summary>A thin tick.</summary>
		Tick,
		/// <summary>A tick two pixels wide.</summary>
		BoldTick,
		/// <summary>A cross.</summary>
		Cross
	}

	/// <summary>How buttons are drawn, beyond the face gradient.</summary>
	public enum ThemeButtonLook
	{
		/// <summary>A face and an edge from the roles.</summary>
		Default,
		/// <summary>Rounded face, a colored ring inside when hot and a blue ring on the default button (Windows XP).</summary>
		Luna,
		/// <summary>A light ring inside the edge, so the gradient face looks like glass (Office 2007 and 2010).</summary>
		Glow,
		/// <summary>A plain face with a 3D edge; the default button has a black frame (Windows Classic).</summary>
		Classic
	}

	/// <summary>How a progress bar fills.</summary>
	public enum ThemeProgressLook
	{
		/// <summary>One solid bar.</summary>
		Default,
		/// <summary>Separate blocks with gaps.</summary>
		Blocks,
		/// <summary>A gradient bar with a light band on its upper half.</summary>
		Glossy
	}

	public enum ThemeThumbShape
	{
		Rectangle,
		Rounded,
		Pill
	}

	public enum ThemeGrip
	{
		None,
		/// <summary>Short parallel lines across the middle of the thumb.</summary>
		Lines,
		/// <summary>Small dots in the middle of the thumb.</summary>
		Dots
	}

	/// <summary>How a scroll bar looks. The default leaves each scroll bar to draw what it always drew.</summary>
	public readonly struct ThemeScrollBarStyle
	{
		/// <summary>True when the scroll bar is drawn by the shared painter and not by the control.</summary>
		public bool Custom { get; init; }

		public ThemeThumbShape Thumb { get; init; }

		public ThemeGrip Grip { get; init; }

		/// <summary>True when the arrow buttons have a face of their own (a 3D box or a gradient); otherwise only the arrow shows.</summary>
		public bool ButtonFaces { get; init; }

		/// <summary>The 3D edge of the thumb and of the button faces.</summary>
		public ThemeBorder Edge { get; init; }

		/// <summary>True when the thumb takes the face gradient of the theme.</summary>
		public bool ThumbGradient { get; init; }
	}

	/// <summary>
	/// How a theme draws faces and edges, beyond its colors. The default draws what the theme did before this type existed:
	/// solid faces and one pixel edges.
	/// </summary>
	public readonly struct ThemeStyle
	{
		public ThemeStyle(ThemeGradient faceGradient, float gradientAmount, bool gloss, ThemeBevel bevel)
		{
			FaceGradient = faceGradient;
			GradientAmount = gradientAmount;
			Gloss = gloss;
			Bevel = bevel;
		}

		/// <summary>The direction of the gradient on button, tab, header and toolbar faces.</summary>
		public ThemeGradient FaceGradient { get; init; }

		/// <summary>How far the light end of a face gradient lies from the face color toward white, from 0 to 1.</summary>
		public float GradientAmount { get; init; }

		/// <summary>How far the dark end of a face gradient lies from the face color toward black. 0 takes a third of the gradient amount.</summary>
		public float GradientShade { get; init; }

		/// <summary>True when faces get a light band over their upper half.</summary>
		public bool Gloss { get; init; }

		/// <summary>The edge of buttons, tabs and toolbars.</summary>
		public ThemeBevel Bevel { get; init; }

		/// <summary>True when the gloss is an even light band with a hard lower edge (Office 2007); false fades it out.</summary>
		public bool GlossBand { get; init; }

		/// <summary>The opacity of the top of the gloss, from 1 to 255. 0 gives the usual 120.</summary>
		public int GlossAlpha { get; init; }

		/// <summary>Corner radii of one kind of face, in 96 DPI pixels. Null takes the radius of the shape.</summary>
		public int? ButtonRadius { get; init; }

		public int? TabRadius { get; init; }

		public int? HeaderRadius { get; init; }

		public int? InputRadius { get; init; }

		public int? MenuRadius { get; init; }

		public int? PanelRadius { get; init; }

		public ThemeBorder InputBorder { get; init; }

		public ThemeBorder PanelBorder { get; init; }

		public ThemeBorder HeaderBorder { get; init; }

		public ThemeScrollBarStyle ScrollBar { get; init; }

		public ThemeArrowGlyph Arrow { get; init; }

		public ThemeCheckGlyph Check { get; init; }

		/// <summary>The color of the light side of a 3D edge. Empty takes the color of the raised surface role.</summary>
		public System.Drawing.Color BevelLight { get; init; }

		/// <summary>True when the thumb of a track bar is a button face, as in the system look, and not an accent colored shape.</summary>
		public bool FaceThumb { get; init; }

		/// <summary>True when a menu shows a band behind the column of images and check marks.</summary>
		public bool MenuGutter { get; init; }

		public ThemeButtonLook ButtonLook { get; init; }

		/// <summary>The ring color of a hot button in the Luna look (orange).</summary>
		public System.Drawing.Color ButtonGlow { get; init; }

		/// <summary>The ring color of the default button in the Luna look (blue).</summary>
		public System.Drawing.Color DefaultGlow { get; init; }

		/// <summary>The corner radius of an appointment frame in the scheduler, in 96 DPI pixels. 0 draws square frames.</summary>
		public int AppointmentRadius { get; init; }

		public ThemeProgressLook ProgressLook { get; init; }

		/// <summary>The color of the progress bar fill. Empty takes the accent color.</summary>
		public System.Drawing.Color ProgressFill { get; init; }

		/// <summary>The width of a progress block in 96 DPI pixels. 0 gives 8.</summary>
		public int ProgressBlock { get; init; }

		/// <summary>The color of the line on top of the selected and the hot tab. Empty draws no line.</summary>
		public System.Drawing.Color TabAccent { get; init; }

		/// <summary>True when a tab is a 3D raised box and not a flat face (Windows Classic).</summary>
		public bool TabRaised { get; init; }

		/// <summary>True when the thumb of a track bar has a pointed end toward its ticks.</summary>
		public bool PointerThumb { get; init; }

		/// <summary>True when a toolbar and a menu bar show a grip of dots at their start (Office 2003).</summary>
		public bool Grip { get; init; }

		/// <summary>True when the caption bar of a group in a task pane has the gradient and the round corners of the Windows XP explorer bar.</summary>
		public bool ExplorerBar { get; init; }

		/// <summary>The color at the right end of a caption gradient (Windows Classic title bars). Empty draws a plain caption.</summary>
		public System.Drawing.Color CaptionEnd { get; init; }

		/// <summary>The font and the details of the older Windows looks (check boxes, group boxes, drop-down buttons).</summary>
		public ThemeDetailStyle Detail { get; init; }
		/// <summary>The looks of the newer themes: glass faces, focus lines, glass boxes and tab lines.</summary>
		public ThemeModernStyle Modern { get; init; }

		public static ThemeStyle Default => default;

		/// <summary>The light end of a face gradient of this style, for a face of the given color.</summary>
		public System.Drawing.Color LightEnd(System.Drawing.Color face)
		{
			return MxTheme.Lighten(face, GradientAmount);
		}

		/// <summary>The dark end of a face gradient of this style, for a face of the given color.</summary>
		public System.Drawing.Color DarkEnd(System.Drawing.Color face)
		{
			return MxTheme.Darken(face, GradientShade > 0f ? GradientShade : GradientAmount * 0.35f);
		}

		/// <summary>The corner radius of a kind of face, or <paramref name="fallback"/> when the style leaves it to the shape.</summary>
		public int RadiusOf(ThemeElement element, int fallback)
		{
			int? value;
			switch (element)
			{
			case ThemeElement.Button:
				value = ButtonRadius;
				break;
			case ThemeElement.Tab:
				value = TabRadius;
				break;
			case ThemeElement.Header:
				value = HeaderRadius;
				break;
			case ThemeElement.Input:
				value = InputRadius;
				break;
			case ThemeElement.Menu:
				value = MenuRadius;
				break;
			default:
				value = PanelRadius;
				break;
			}
			return value ?? fallback;
		}
	}
}
