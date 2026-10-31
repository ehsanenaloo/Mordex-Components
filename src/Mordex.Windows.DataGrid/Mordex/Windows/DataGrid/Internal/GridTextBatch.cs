using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.DataGrid.Internal
{
	// Holds the texts of the cells of a paint pass and draws them with one device context when a row is done. A text is held
	// back only when nothing that is drawn after it can touch its pixels, so the picture stays the same as when every text is
	// drawn at once: the part of the item that its gridlines and adornments leave free is set for the item (SetFreeArea),
	// and an item that paints over a held text flushes the batch before it paints.
	internal sealed class GridTextBatch
	{
		private readonly TextBatch _batch = new TextBatch();

		private Graphics _graphics;

		private bool _active;

		private Rectangle _freeArea;

		private bool _freeIsHeader;

		// For tests: how many texts were held back, and how many of them belong to headers.
		internal static long TextsHeld;

		internal static long HeaderTextsHeld;

		// The batch of the paint pass that runs on this thread. A style that releases its font while a text with it is held
		// back draws that text first.
		[System.ThreadStatic]
		private static GridTextBatch _current;

		// For tests: draws every text at once.
		internal static bool Disabled { get; set; }

		// For the benchmark: only the texts of plain cells wait, as before headers and list cells were added.
		internal static bool CellsOnly { get; set; }

		internal bool IsActive => _active;

		// Starts a paint pass. The batch works only when the clip of the Graphics is a rectangle (or none) and there is no
		// transform, because then the clip of each item is a rectangle that can be kept with its text.
		internal void Begin(Graphics graphics)
		{
			_batch.Flush(graphics);
			_graphics = graphics;
			_current = this;
			_freeArea = Rectangle.Empty;
			_active = false;
			if (Disabled)
			{
				return;
			}
			using (Matrix transform = graphics.Transform)
			{
				float[] m = transform.Elements;
				if (m[0] != 1f || m[1] != 0f || m[2] != 0f || m[3] != 1f || m[4] != 0f || m[5] != 0f || (graphics.PageUnit != GraphicsUnit.Pixel && graphics.PageUnit != GraphicsUnit.Display) || graphics.PageScale != 1f)
				{
					return;
				}
			}
			using (Region clip = graphics.Clip)
			{
				if (clip.IsInfinite(graphics))
				{
					_active = true;
					return;
				}
				using (Matrix identity = new Matrix())
				{
					_active = clip.GetRegionScans(identity).Length == 1;
				}
			}
		}

		internal void End()
		{
			Flush();
			_active = false;
			_graphics = null;
			if (ReferenceEquals(_current, this))
			{
				_current = null;
			}
		}

		internal void Flush()
		{
			if (_graphics != null && _batch.Count > 0)
			{
				_batch.Flush(_graphics);
			}
		}

		// Draws the held texts when one of them uses the font, before the font is disposed.
		internal static void FlushIfUses(Font font)
		{
			GridTextBatch current = _current;
			if (font != null && current != null && current._batch.IsPending(font))
			{
				current.Flush();
			}
		}

		// Called when an item is about to paint in the area: texts held back that show pixels there are drawn first.
		internal void FlushIfOverlaps(Rectangle area)
		{
			if (_active && _batch.Count > 0 && _batch.IntersectsPending(area))
			{
				Flush();
			}
		}

		// The part of the current item that nothing painted after the text of the item touches. Empty keeps the texts of the
		// item from being held back.
		internal void SetFreeArea(Rectangle freeArea, bool header = false)
		{
			_freeArea = _active ? freeArea : Rectangle.Empty;
			_freeIsHeader = header;
		}

		// A text that FastText cannot draw (one that needs an ellipsis, wraps, or is right to left or complex) is held too: the
		// batch draws it with TextRenderer on its device context, cut to the clip of the cell, which gives the same pixels.
		internal bool TryDraw(Graphics graphics, string text, Rectangle bounds, Font font, Color color, TextFormatFlags flags)
		{
			if (!_active || _freeArea.IsEmpty || graphics != _graphics || (flags & TextFormatFlags.NoClipping) != 0 || text == null)
			{
				return false;
			}
			// The clip is a rectangle with whole numbers: every clip of the grid is made from a rectangle.
			Rectangle clip = Rectangle.Round(graphics.ClipBounds);
			Rectangle visible = Rectangle.Intersect(bounds, clip);
			if (visible.Width <= 0 || visible.Height <= 0 || !_freeArea.Contains(visible))
			{
				return false;
			}
			_batch.Add(text, bounds, font, color, flags, clip);
			TextsHeld++;
			if (_freeIsHeader)
			{
				HeaderTextsHeld++;
			}
			return true;
		}
	}
}
