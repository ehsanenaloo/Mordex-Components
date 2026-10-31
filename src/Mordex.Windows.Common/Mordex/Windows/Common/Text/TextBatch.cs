using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	// Collects text draws and paints them with one device context. Drawing each text on its own makes
	// TextRenderer get and release the device context and read the clip region every time.
	public sealed class TextBatch
	{
		private struct Item
		{
			internal string Text;
			internal Rectangle Bounds;
			internal Font Font;
			internal Color Color;
			internal TextFormatFlags Flags;
			internal bool HasClip;
			internal Rectangle Clip;
		}

		private sealed class HdcContext : IDeviceContext
		{
			private readonly IntPtr _hdc;

			internal HdcContext(IntPtr hdc)
			{
				_hdc = hdc;
			}

			public IntPtr GetHdc()
			{
				return _hdc;
			}

			public void ReleaseHdc()
			{
			}

			public void Dispose()
			{
			}
		}

		[DllImport("gdi32.dll")]
		private static extern int SaveDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern bool RestoreDC(IntPtr hdc, int saved);

		[DllImport("gdi32.dll")]
		private static extern int IntersectClipRect(IntPtr hdc, int left, int top, int right, int bottom);

		private readonly List<Item> _items = new List<Item>();

		public int Count => _items.Count;

		public void Add(string text, Rectangle bounds, Font font, Color color, TextFormatFlags flags)
		{
			_items.Add(new Item { Text = text, Bounds = bounds, Font = font, Color = color, Flags = flags });
		}

		// For a text that was drawn inside a rectangular clip region of the Graphics: the clip is given and the text is cut to
		// it when the batch is flushed, whatever the clip of the Graphics is at that time. A text that FastText cannot draw goes
		// to TextRenderer on the same device context with the clip set on it.
		public void Add(string text, Rectangle bounds, Font font, Color color, TextFormatFlags flags, Rectangle clip)
		{
			_items.Add(new Item { Text = text, Bounds = bounds, Font = font, Color = color, Flags = flags, HasClip = true, Clip = clip });
		}

		// True when a text of the batch would show pixels inside the area.
		public bool IntersectsPending(Rectangle area)
		{
			foreach (Item item in _items)
			{
				Rectangle visible = item.HasClip ? Rectangle.Intersect(item.Bounds, item.Clip) : item.Bounds;
				if (visible.IntersectsWith(area))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>True when a queued text still needs the font.</summary>
		public bool IsPending(Font font)
		{
			foreach (Item item in _items)
			{
				if (ReferenceEquals(item.Font, font))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Disposes a font that was passed to Add. Texts still queued with it are drawn first.</summary>
		public void ReleaseFont(Graphics graphics, Font font)
		{
			if (IsPending(font))
			{
				Flush(graphics);
			}
			font.Dispose();
		}

		// The fonts passed to Add must stay alive until Flush returns. One batch is for one Graphics.
		public void Flush(Graphics graphics)
		{
			if (_items.Count == 0)
			{
				return;
			}
			// A device context from GetHdc does not carry the clip of the Graphics. A rectangular clip is given to each text
			// that has none of its own; any other clip makes the texts go through the Graphics one by one.
			// Nor does it carry the transform. A translation is put on the coordinates here, as TextRenderer does for a text with
			// PreserveGraphicsTranslateTransform; a scale or a rotation makes the texts go through the Graphics one by one.
			Matrix3x2 transform = graphics.TransformElements;
			bool translateOnly = transform.M11 == 1f && transform.M12 == 0f && transform.M21 == 0f && transform.M22 == 1f;
			Rectangle? baseClip = null;
			bool complex = !translateOnly;
			if (translateOnly)
			{
				using (Region clip = graphics.Clip)
				{
					if (!clip.IsInfinite(graphics))
					{
						// The clip of the Graphics is in world coordinates; the device context needs device coordinates.
						clip.Translate(transform.M31, transform.M32);
						using (System.Drawing.Drawing2D.Matrix identity = new System.Drawing.Drawing2D.Matrix())
						{
							RectangleF[] scans = clip.GetRegionScans(identity);
							if (scans.Length == 1)
							{
								baseClip = Rectangle.Round(scans[0]);
							}
							else
							{
								complex = true;
							}
						}
					}
				}
			}
			// TextRenderer moves a text by the whole pixels of the translation, and only when the text asks for it.
			Point translation = new Point((int)transform.M31, (int)transform.M32);
			if (!translateOnly || complex && !_items.TrueForAll(item => item.HasClip))
			{
				foreach (Item item in _items)
				{
					if (item.HasClip)
					{
						using Region saved = graphics.Clip;
						graphics.SetClip(item.Clip, CombineMode.Intersect);
						TextRenderer.DrawText(graphics, item.Text, item.Font, item.Bounds, item.Color, item.Flags);
						graphics.Clip = saved;
					}
					else
					{
						TextRenderer.DrawText(graphics, item.Text, item.Font, item.Bounds, item.Color, item.Flags);
					}
				}
			}
			else
			{
				IntPtr hdc = graphics.GetHdc();
				try
				{
					HdcContext context = new HdcContext(hdc);
					foreach (Item item in _items)
					{
						// The clip given with a text is in the coordinates of its bounds, so it moves with them.
						Point offset = (item.Flags & TextFormatFlags.PreserveGraphicsTranslateTransform) != 0 ? translation : Point.Empty;
						Rectangle bounds = item.Bounds;
						bounds.Offset(offset);
						bool hasClip = item.HasClip || baseClip.HasValue;
						Rectangle itemClip = baseClip.GetValueOrDefault();
						if (item.HasClip)
						{
							itemClip = item.Clip;
							itemClip.Offset(offset);
						}
						// Short plain texts take the faster path, which gives the same pixels; the rest go to TextRenderer.
						bool drawn = hasClip
							? FastText.TryDraw(hdc, item.Text, item.Font, bounds, itemClip, item.Color, item.Flags)
							: FastText.TryDraw(hdc, item.Text, item.Font, bounds, item.Color, item.Flags);
						if (!drawn)
						{
							int saved = hasClip ? SaveDC(hdc) : 0;
							if (hasClip)
							{
								IntersectClipRect(hdc, itemClip.Left, itemClip.Top, itemClip.Right, itemClip.Bottom);
							}
							TextRenderer.DrawText(context, item.Text, item.Font, bounds, item.Color, item.Flags);
							if (hasClip)
							{
								RestoreDC(hdc, saved);
							}
						}
					}
				}
				finally
				{
					graphics.ReleaseHdc(hdc);
				}
			}
			_items.Clear();
		}
	}
}
