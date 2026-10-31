using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Mordex.Windows.Common
{
	/// <summary>
	/// Shared solid brushes, pens and two color gradients for paint code, made once for each color and size. A paint
	/// method that has a color but not a theme role uses this, so it creates no GDI+ object.
	/// </summary>
	/// <remarks>
	/// What it returns is borrowed: do not dispose it, change it, or keep it after the paint. Each thread has its own
	/// cache, because GDI+ objects are for one thread at a time. When a cache is full it is dropped without disposing, so
	/// an object that a paint method still holds stays valid and the garbage collector releases it later.
	/// </remarks>
	public static class GdiCache
	{
		private const int Largest = 512;

		[ThreadStatic]
		private static Dictionary<int, SolidBrush> _brushes;

		[ThreadStatic]
		private static Dictionary<(int, float), Pen> _pens;

		[ThreadStatic]
		private static Dictionary<(int, int, int, int, LinearGradientMode), LinearGradientBrush> _gradients;

		public static SolidBrush Brush(Color color)
		{
			Dictionary<int, SolidBrush> cache = _brushes ?? (_brushes = new Dictionary<int, SolidBrush>());
			int key = color.ToArgb();
			if (!cache.TryGetValue(key, out SolidBrush brush))
			{
				if (cache.Count >= Largest)
				{
					cache.Clear();
				}
				brush = new SolidBrush(color);
				cache[key] = brush;
			}
			return brush;
		}

		public static Pen Pen(Color color)
		{
			return Pen(color, 1f);
		}

		public static Pen Pen(Color color, float width)
		{
			Dictionary<(int, float), Pen> cache = _pens ?? (_pens = new Dictionary<(int, float), Pen>());
			(int, float) key = (color.ToArgb(), width);
			if (!cache.TryGetValue(key, out Pen pen))
			{
				if (cache.Count >= Largest)
				{
					cache.Clear();
				}
				pen = new Pen(color, width);
				cache[key] = pen;
			}
			return pen;
		}

		/// <summary>Fills a rectangle with a gradient of two colors. The rectangle may be anywhere; the brush is kept for its size.</summary>
		public static void FillGradient(Graphics graphics, Rectangle bounds, Color from, Color to, LinearGradientMode mode)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return;
			}
			Dictionary<(int, int, int, int, LinearGradientMode), LinearGradientBrush> cache = _gradients ?? (_gradients = new Dictionary<(int, int, int, int, LinearGradientMode), LinearGradientBrush>());
			(int, int, int, int, LinearGradientMode) key = (from.ToArgb(), to.ToArgb(), bounds.Width, bounds.Height, mode);
			if (!cache.TryGetValue(key, out LinearGradientBrush brush))
			{
				if (cache.Count >= Largest / 4)
				{
					cache.Clear();
				}
				brush = new LinearGradientBrush(new Rectangle(0, 0, bounds.Width, bounds.Height), from, to, mode);
				cache[key] = brush;
			}
			graphics.TranslateTransform(bounds.X, bounds.Y);
			graphics.FillRectangle(brush, 0, 0, bounds.Width, bounds.Height);
			graphics.TranslateTransform(-bounds.X, -bounds.Y);
		}
	}
}
