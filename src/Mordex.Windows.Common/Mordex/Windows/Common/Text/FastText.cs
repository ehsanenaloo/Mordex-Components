using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// Draws short, single line texts with one GDI call on a device context that the caller keeps, and puts the same pixels on
	/// the bitmap as <see cref="TextRenderer.DrawText(IDeviceContext, string, Font, Rectangle, Color, TextFormatFlags)"/>. It is
	/// several times faster for many small texts (a calendar month, grid cells). It does not wrap, shorten with an ellipsis,
	/// read mnemonics or shape complex scripts: <see cref="TryDraw"/> returns false for those and the caller draws the text with
	/// <see cref="TextRenderer"/> instead.
	/// </summary>
	public static class FastText
	{
		private const int TransparentBackground = 1;

		private const uint Clipped = 0x0004;

		private const int WidthCacheLimit = 4096;

		private const int SizeCacheLimit = 131072;

		// What this class can place by itself. Everything else goes to TextRenderer.
		private const TextFormatFlags Supported = TextFormatFlags.Left | TextFormatFlags.HorizontalCenter | TextFormatFlags.Right
			| TextFormatFlags.Top | TextFormatFlags.VerticalCenter | TextFormatFlags.Bottom
			| TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping
			| TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform
			| TextFormatFlags.EndEllipsis | TextFormatFlags.ExpandTabs;

		[StructLayout(LayoutKind.Sequential)]
		private struct NativeSize
		{
			public int Width;
			public int Height;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct NativeRect
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
		private struct TextMetric
		{
			public int Height;
			public int Ascent;
			public int Descent;
			public int InternalLeading;
			public int ExternalLeading;
			public int AveCharWidth;
			public int MaxCharWidth;
			public int Weight;
			public int Overhang;
			public int DigitizedAspectX;
			public int DigitizedAspectY;
			public char FirstChar;
			public char LastChar;
			public char DefaultChar;
			public char BreakChar;
			public byte Italic;
			public byte Underlined;
			public byte StruckOut;
			public byte PitchAndFamily;
			public byte CharSet;
		}

		[DllImport("gdi32.dll")]
		private static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

		[DllImport("gdi32.dll")]
		private static extern int SetBkMode(IntPtr hdc, int mode);

		[DllImport("gdi32.dll")]
		private static extern uint SetTextColor(IntPtr hdc, int color);

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
		private static extern bool ExtTextOutW(IntPtr hdc, int x, int y, uint options, ref NativeRect rect, string text, uint count, IntPtr dx);

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
		private static extern bool GetTextExtentPoint32W(IntPtr hdc, string text, int length, out NativeSize size);

		[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
		private static extern bool GetTextMetricsW(IntPtr hdc, out TextMetric metric);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr obj);

		// One GDI font for each Font object, made when it is first used and deleted when the Font is collected.
		private sealed class FontHandle
		{
			internal readonly IntPtr Handle;

			internal int Height;

			// Widths belong to the font object, not to the handle value: GDI reuses a handle value after the font is deleted.
			internal readonly Dictionary<string, int> Widths = new Dictionary<string, int>();

			// Sizes from TryMeasure, packed as width and height, for texts with the default padding and with none.
			internal readonly Dictionary<string, long> PaddedSizes = new Dictionary<string, long>();

			internal readonly Dictionary<string, long> TightSizes = new Dictionary<string, long>();

			internal FontHandle(IntPtr handle)
			{
				Handle = handle;
			}

			~FontHandle()
			{
				DeleteObject(Handle);
			}
		}

		private static readonly ConditionalWeakTable<Font, FontHandle> Fonts = new ConditionalWeakTable<Font, FontHandle>();

		private const int SharedFontLimit = 128;

		// Fonts with the same face, size and style give the same GDI font: a control that makes a new Font object for each
		// pass (a style that resolves bold or italic) still shares the handle and the width caches. The table is cleared when
		// it grows; a handle that is dropped is deleted by its finalizer.
		private static readonly Dictionary<(string, float, FontStyle, GraphicsUnit, byte, bool), FontHandle> SharedFonts = new Dictionary<(string, float, FontStyle, GraphicsUnit, byte, bool), FontHandle>();

		// For tests and the benchmark: forgets every font handle and the sizes measured with it.
		internal static void ClearCaches()
		{
			lock (SharedFonts)
			{
				SharedFonts.Clear();
				Fonts.Clear();
			}
		}

		private static FontHandle HandleFor(Font font)
		{
			if (Fonts.TryGetValue(font, out FontHandle handle))
			{
				return handle;
			}
			(string, float, FontStyle, GraphicsUnit, byte, bool) key = (font.Name, font.Size, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
			lock (SharedFonts)
			{
				if (!SharedFonts.TryGetValue(key, out handle))
				{
					if (SharedFonts.Count >= SharedFontLimit)
					{
						SharedFonts.Clear();
					}
					handle = new FontHandle(CreateHandle(font));
					SharedFonts[key] = handle;
				}
			}
			return Fonts.GetValue(font, unused => handle);
		}

		[StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
		private sealed class LogFont
		{
			public int Height;
			public int Width;
			public int Escapement;
			public int Orientation;
			public int Weight;
			public byte Italic;
			public byte Underline;
			public byte StrikeOut;
			public byte CharSet;
			public byte OutPrecision;
			public byte ClipPrecision;
			public byte Quality;
			public byte PitchAndFamily;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string FaceName = "";
		}

		[DllImport("gdi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
		private static extern IntPtr CreateFontIndirectW([In, MarshalAs(UnmanagedType.LPStruct)] LogFont font);

		[DllImport("user32.dll")]
		private static extern IntPtr GetDC(IntPtr window);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr window, IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern int GetDeviceCaps(IntPtr hdc, int index);

		private const int LogPixelsY = 90;

		// TextRenderer rounds the height of the font up to a whole pixel, GDI+ rounds it to the nearest pixel.
		// They differ for sizes such as 7 pt (9.33 px), and the glyphs of the two heights are not the same.
		private static IntPtr CreateHandle(Font font)
		{
			LogFont log = new LogFont();
			font.ToLogFont(log);
			double pixels;
			if (font.Unit == GraphicsUnit.Pixel)
			{
				pixels = font.Size;
			}
			else
			{
				IntPtr screen = GetDC(IntPtr.Zero);
				try
				{
					pixels = font.SizeInPoints * GetDeviceCaps(screen, LogPixelsY) / 72.0;
				}
				finally
				{
					ReleaseDC(IntPtr.Zero, screen);
				}
			}
			log.Height = -(int)Math.Ceiling(pixels - 0.001);
			return CreateFontIndirectW(log);
		}

		/// <summary>True when the text has only characters that GDI places one after the other, from left to right.</summary>
		public static bool IsSimple(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			foreach (char c in text)
			{
				// Control characters, tabs and line breaks need a layout; everything from U+0590 up includes the scripts that are
				// written from right to left or joined (Hebrew, Arabic, Persian) and the combining or surrogate ranges.
				if (c < ' ' || c >= '֐')
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Draws the text and returns true, or returns false and draws nothing when this class cannot give the same result as
		/// <see cref="TextRenderer"/>: a flag it does not place, a text that is not simple, an empty rectangle, or a text wider
		/// or taller than the rectangle.
		/// </summary>
		/// <param name="hdc">A device context, for example from <see cref="Graphics.GetHdc"/>, kept by the caller for many texts.</param>
		public static bool TryDraw(IntPtr hdc, string text, Font font, Rectangle bounds, Color color, TextFormatFlags flags)
		{
			return TryDraw(hdc, text, font, bounds, color, flags, false, Rectangle.Empty);
		}

		/// <summary>
		/// Same as <see cref="TryDraw(IntPtr, string, Font, Rectangle, Color, TextFormatFlags)"/>, and the text is also clipped to
		/// <paramref name="clip"/>. The text is placed in <paramref name="bounds"/>, as with a rectangular clip region of the
		/// Graphics. Nothing is drawn (and true is returned) when the clip leaves nothing visible.
		/// </summary>
		public static bool TryDraw(IntPtr hdc, string text, Font font, Rectangle bounds, Rectangle clip, Color color, TextFormatFlags flags)
		{
			return TryDraw(hdc, text, font, bounds, color, flags, true, clip);
		}

		// Padding the text renderer adds without NoPadding: the height of the GDI font divided by 6, rounded up, on the left,
		// and one and a half times that on the right.
		private static void Margins(int fontHeight, TextFormatFlags flags, out int left, out int right)
		{
			if ((flags & TextFormatFlags.NoPadding) != 0)
			{
				left = 0;
				right = 0;
				return;
			}
			float unit = fontHeight / 6.0f;
			left = (int)Math.Ceiling(unit);
			right = (int)Math.Ceiling(unit * 1.5f);
		}

		private static bool TryDraw(IntPtr hdc, string text, Font font, Rectangle bounds, Color color, TextFormatFlags flags, bool hasClip, Rectangle clip)
		{
			if (!CanDrawFlags(text, bounds, flags))
			{
				return false;
			}
			// TextRenderer draws nothing for the named transparent color; any other color loses its alpha in GDI.
			if (color == Color.Transparent)
			{
				return true;
			}
			FontHandle handle = HandleFor(font);
			IntPtr old = SelectObject(hdc, handle.Handle);
			try
			{
				if (handle.Height == 0)
				{
					handle.Height = GetTextMetricsW(hdc, out TextMetric metric) ? metric.Height : 0;
				}
				int width = MeasureSelected(hdc, handle, text, (flags & TextFormatFlags.ExpandTabs) != 0, font);
				if (width < 0 || handle.Height == 0)
				{
					return false;
				}
				Margins(handle.Height, flags, out int marginLeft, out int marginRight);
				if (width + marginLeft + marginRight > bounds.Width || handle.Height > bounds.Height)
				{
					return false;
				}
				Rectangle visible = bounds;
				bool clipped = (flags & TextFormatFlags.NoClipping) == 0;
				if (hasClip)
				{
					visible = (flags & TextFormatFlags.NoClipping) == 0 ? Rectangle.Intersect(bounds, clip) : clip;
					clipped = true;
					if (visible.Width <= 0 || visible.Height <= 0)
					{
						return true;
					}
				}
				int x = (flags & TextFormatFlags.Right) != 0 ? bounds.Right - marginRight - width : (flags & TextFormatFlags.HorizontalCenter) != 0 ? bounds.Left + marginLeft + (bounds.Width - marginLeft - marginRight - width) / 2 : bounds.Left + marginLeft;
				int y = (flags & TextFormatFlags.Bottom) != 0 ? bounds.Bottom - handle.Height : (flags & TextFormatFlags.VerticalCenter) != 0 ? bounds.Top + (bounds.Height - handle.Height) / 2 : bounds.Top;
				int oldMode = SetBkMode(hdc, TransparentBackground);
				uint oldColor = SetTextColor(hdc, color.R | (color.G << 8) | (color.B << 16));
				NativeRect rect = new NativeRect { Left = visible.Left, Top = visible.Top, Right = visible.Right, Bottom = visible.Bottom };
				bool drawn;
				if ((flags & TextFormatFlags.ExpandTabs) != 0)
				{
					// The text renderer lays out with tab expansion, which places the glyphs a little differently.
					int saved = SaveDC(hdc);
					if (clipped)
					{
						IntersectClipRect(hdc, rect.Left, rect.Top, rect.Right, rect.Bottom);
					}
					drawn = TabbedTextOutW(hdc, x, y, text, text.Length, 0, IntPtr.Zero, 0) != 0;
					RestoreDC(hdc, saved);
				}
				else
				{
					drawn = ExtTextOutW(hdc, x, y, clipped ? Clipped : 0u, ref rect, text, (uint)text.Length, IntPtr.Zero);
				}
				// The device context belongs to the caller, who may draw other texts on it with TextRenderer.
				SetTextColor(hdc, (int)oldColor);
				SetBkMode(hdc, oldMode);
				return drawn;
			}
			finally
			{
				SelectObject(hdc, old);
				GC.KeepAlive(handle);
			}
		}

		private static bool CanDrawFlags(string text, Rectangle bounds, TextFormatFlags flags)
		{
			// Without NoPrefix the text renderer reads '&' as a mnemonic.
			return (flags & ~Supported) == 0 && IsSimple(text) && ((flags & TextFormatFlags.NoPrefix) != 0 || text.IndexOf('&') < 0) && bounds.Width > 0 && bounds.Height > 0 && (flags & TextFormatFlags.RightToLeft) == 0;
		}

		// A memory device context for each thread. The finalizer deletes it when the thread and its static data are gone.
		private sealed class MeasureDevice
		{
			internal readonly IntPtr Handle = CreateCompatibleDC(IntPtr.Zero);

			~MeasureDevice()
			{
				if (Handle != IntPtr.Zero)
				{
					DeleteDC(Handle);
				}
			}
		}

		[ThreadStatic]
		private static MeasureDevice _measureDevice;

		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern int SaveDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern bool RestoreDC(IntPtr hdc, int saved);

		[DllImport("gdi32.dll")]
		private static extern int IntersectClipRect(IntPtr hdc, int left, int top, int right, int bottom);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int TabbedTextOutW(IntPtr hdc, int x, int y, string text, int length, int tabs, IntPtr tabPositions, int origin);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern uint GetTabbedTextExtentW(IntPtr hdc, string text, int length, int tabs, IntPtr tabPositions);

		/// <summary>
		/// True when <see cref="TryDraw(IntPtr, string, Font, Rectangle, Color, TextFormatFlags)"/> would draw the text, decided
		/// without a device context of the caller (a Graphics cannot give one between two other drawing calls).
		/// </summary>
		public static bool CanDraw(string text, Font font, Rectangle bounds, TextFormatFlags flags)
		{
			if (!CanDrawFlags(text, bounds, flags))
			{
				return false;
			}
			IntPtr measureDc = (_measureDevice ??= new MeasureDevice()).Handle;
			if (measureDc == IntPtr.Zero)
			{
				return false;
			}
			FontHandle handle = HandleFor(font);
			IntPtr old = SelectObject(measureDc, handle.Handle);
			try
			{
				if (handle.Height == 0)
				{
					handle.Height = GetTextMetricsW(measureDc, out TextMetric metric) ? metric.Height : 0;
				}
				int width = MeasureSelected(measureDc, handle, text, (flags & TextFormatFlags.ExpandTabs) != 0, font);
				if (width < 0 || handle.Height == 0)
				{
					return false;
				}
				Margins(handle.Height, flags, out int marginLeft, out int marginRight);
				return width + marginLeft + marginRight <= bounds.Width && handle.Height <= bounds.Height;
			}
			finally
			{
				SelectObject(measureDc, old);
				GC.KeepAlive(handle);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DrawTextParams
		{
			public int Size;
			public int TabLength;
			public int LeftMargin;
			public int RightMargin;
			public uint LengthDrawn;
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int DrawTextExW(IntPtr hdc, string text, int length, ref NativeRect rect, uint format, ref DrawTextParams parameters);

		private const uint CalcRect = 0x0400;

		// What a single line measure can take: the flags that change the size are SingleLine, NoPrefix and NoPadding.
		private const TextFormatFlags Measurable = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix
			| TextFormatFlags.SingleLine | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform;

		/// <summary>
		/// Measures a single line text with the call <see cref="TextRenderer.MeasureText(string, Font, Size, TextFormatFlags)"/>
		/// makes (DrawTextEx with DT_CALCRECT, the same GDI font and the same padding), without the cost of its font lookup,
		/// so the size is the one the text renderer returns. Returns false when it cannot promise that: a flag that is not
		/// handled or a text that is not simple. The result is for the resolution of the screen.
		/// </summary>
		public static bool TryMeasure(string text, Font font, TextFormatFlags flags, out Size size)
		{
			size = Size.Empty;
			if ((flags & ~Measurable) != 0 || (flags & TextFormatFlags.SingleLine) == 0 || font == null || !IsSimple(text) || text.Length > 2048)
			{
				return false;
			}
			if ((flags & TextFormatFlags.NoPrefix) == 0 && text.IndexOf('&') >= 0)
			{
				return false;
			}
			IntPtr dc = (_measureDevice ??= new MeasureDevice()).Handle;
			if (dc == IntPtr.Zero)
			{
				return false;
			}
			FontHandle handle = HandleFor(font);
			Dictionary<string, long> sizes = (flags & TextFormatFlags.NoPadding) != 0 ? handle.TightSizes : handle.PaddedSizes;
			lock (sizes)
			{
				if (sizes.TryGetValue(text, out long packed))
				{
					size = new Size((int)(packed >> 32), (int)packed);
					return true;
				}
			}
			IntPtr old = SelectObject(dc, handle.Handle);
			try
			{
				if (handle.Height == 0)
				{
					handle.Height = GetTextMetricsW(dc, out TextMetric metric) ? metric.Height : 0;
				}
				if (handle.Height == 0)
				{
					return false;
				}
				Margins(handle.Height, flags, out int left, out int right);
				DrawTextParams parameters = new DrawTextParams { Size = Marshal.SizeOf<DrawTextParams>(), LeftMargin = left, RightMargin = right };
				NativeRect rect = new NativeRect { Right = int.MaxValue, Bottom = int.MaxValue };
				if (DrawTextExW(dc, text, text.Length, ref rect, CalcRect | (uint)(flags & (TextFormatFlags)0xFFFF), ref parameters) == 0)
				{
					return false;
				}
				size = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
				lock (sizes)
				{
					// Room for the cells of a large grid; the cache is dropped when its share of the budget is used.
					if (sizes.Count >= SizeCacheLimit)
					{
						sizes.Clear();
					}
					sizes[text] = ((long)size.Width << 32) | (uint)size.Height;
				}
				return true;
			}
			finally
			{
				SelectObject(dc, old);
				GC.KeepAlive(handle);
			}
		}

		/// <summary>
		/// True when a measure on this Graphics (a window, or a bitmap at the resolution of the screen) gives the same size as
		/// <see cref="TryMeasure"/>. A printer or a bitmap with another resolution does not.
		/// </summary>
		public static bool UsesScreenResolution(Graphics graphics)
		{
			if (graphics == null || graphics.PageUnit != GraphicsUnit.Pixel && graphics.PageUnit != GraphicsUnit.Display || graphics.PageScale != 1f)
			{
				return false;
			}
			IntPtr screen = GetDC(IntPtr.Zero);
			try
			{
				return Math.Abs(graphics.DpiY - GetDeviceCaps(screen, LogPixelsY)) < 0.5f;
			}
			finally
			{
				ReleaseDC(IntPtr.Zero, screen);
			}
		}

		/// <summary>The width of a simple text in pixels, as GDI places it, or -1 when the text is not simple.</summary>
		public static int Measure(IntPtr hdc, string text, Font font)
		{
			if (!IsSimple(text))
			{
				return -1;
			}
			FontHandle handle = HandleFor(font);
			IntPtr old = SelectObject(hdc, handle.Handle);
			try
			{
				return MeasureSelected(hdc, handle, text, false, null);
			}
			finally
			{
				SelectObject(hdc, old);
				GC.KeepAlive(handle);
			}
		}

		// With a Font the width is checked against the text renderer: it kerns some letter pairs (Tr, AV) that ExtTextOut places
		// one after the other, so such a text gets -1 and is left to the text renderer.
		private static int MeasureSelected(IntPtr hdc, FontHandle font, string text, bool tabbed, Font checkFont)
		{
			Dictionary<string, int> cache = font.Widths;
			string key = (tabbed ? "\u0001" : "") + (checkFont == null ? "\u0002" : "") + text;
			lock (cache)
			{
				if (cache.TryGetValue(key, out int cached))
				{
					return cached;
				}
			}
			NativeSize size;
			if (tabbed)
			{
				uint extent = GetTabbedTextExtentW(hdc, text, text.Length, 0, IntPtr.Zero);
				size = new NativeSize { Width = (int)(extent & 0xFFFF) };
			}
			else if (!GetTextExtentPoint32W(hdc, text, text.Length, out size))
			{
				return -1;
			}
			int result = size.Width;
			if (checkFont != null)
			{
				TextFormatFlags measureFlags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | (tabbed ? TextFormatFlags.ExpandTabs : 0);
				int rendererWidth = !tabbed && TryMeasure(text, checkFont, measureFlags, out Size measured)
					? measured.Width
					: TextRenderer.MeasureText(text, checkFont, new Size(int.MaxValue, 0), measureFlags).Width;
				if (rendererWidth != result)
				{
					result = -1;
				}
			}
			lock (cache)
			{
				if (cache.Count >= WidthCacheLimit)
				{
					cache.Clear();
				}
				cache[key] = result;
			}
			return result;
		}
	}
}
