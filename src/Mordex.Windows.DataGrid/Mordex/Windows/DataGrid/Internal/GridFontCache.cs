using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.DataGrid.Internal
{
	// Shares one Font object between all styles that resolve to the same font, so drawing code that keys
	// its work on the Font object (GDI font handles, text width caches) sees one object per font.
	// A style takes a reference with Acquire and gives it back with Release; the last release disposes the font.
	// While a deferral is open (one paint pass) a font with no references stays usable and is disposed when the
	// deferral ends, so a text batch that holds a font until a row is done never sees a disposed font.
	internal static class GridFontCache
	{
		private struct Key
		{
			public string Name;

			public float Size;

			public FontStyle Style;

			public GraphicsUnit Unit;

			public byte CharSet;

			public bool Vertical;
		}

		private sealed class KeyComparer : IEqualityComparer<Key>
		{
			public bool Equals(Key x, Key y)
			{
				return x.Size == y.Size && x.Style == y.Style && x.Unit == y.Unit && x.CharSet == y.CharSet && x.Vertical == y.Vertical && x.Name == y.Name;
			}

			public int GetHashCode(Key key)
			{
				return System.HashCode.Combine(key.Name, key.Size, key.Style, key.Unit, key.CharSet, key.Vertical);
			}
		}

		private sealed class Entry
		{
			public Key Key;

			public Font Font;

			public int References;

			public bool Removed;
		}

		private static readonly object Sync = new object();

		private static readonly Dictionary<Key, Entry> Entries = new Dictionary<Key, Entry>(new KeyComparer());

		private static readonly Dictionary<Font, Entry> ByFont = new Dictionary<Font, Entry>(ReferenceEqualityComparer.Instance);

		private static readonly List<Entry> Unreferenced = new List<Entry>();

		private static int _deferrals;

		// Takes a reference to the shared font with the properties of the source.
		internal static Font Acquire(Font source)
		{
			Key key = new Key
			{
				Name = source.Name,
				Size = source.Size,
				Style = source.Style,
				Unit = source.Unit,
				CharSet = source.GdiCharSet,
				Vertical = source.GdiVerticalFont
			};
			lock (Sync)
			{
				if (Entries.TryGetValue(key, out Entry entry))
				{
					entry.References++;
					return entry.Font;
				}
				return Add(key, (Font)source.Clone());
			}
		}

		// Takes a reference to the shared font of the given name, size in points and style.
		internal static Font Acquire(string name, float sizeInPoints, FontStyle style)
		{
			Key key = new Key { Name = name, Size = sizeInPoints, Style = style, Unit = GraphicsUnit.Point, CharSet = 1, Vertical = false };
			lock (Sync)
			{
				if (Entries.TryGetValue(key, out Entry entry))
				{
					entry.References++;
					return entry.Font;
				}
				return Add(key, new Font(name, sizeInPoints, style));
			}
		}

		private static Font Add(Key key, Font font)
		{
			Entry entry = new Entry { Key = key, Font = font, References = 1 };
			Entries[key] = entry;
			ByFont[font] = entry;
			return font;
		}

		internal static void Release(Font font)
		{
			lock (Sync)
			{
				if (!ByFont.TryGetValue(font, out Entry entry))
				{
					return;
				}
				entry.References--;
				if (entry.References > 0)
				{
					return;
				}
				if (_deferrals > 0)
				{
					Unreferenced.Add(entry);
					return;
				}
				Remove(entry);
			}
		}

		private static void Remove(Entry entry)
		{
			if (entry.Removed)
			{
				return;
			}
			entry.Removed = true;
			Entries.Remove(entry.Key);
			ByFont.Remove(entry.Font);
			entry.Font.Dispose();
		}

		internal static void BeginDeferral()
		{
			lock (Sync)
			{
				_deferrals++;
			}
		}

		internal static void EndDeferral()
		{
			lock (Sync)
			{
				_deferrals--;
				if (_deferrals > 0)
				{
					return;
				}
				foreach (Entry entry in Unreferenced)
				{
					if (entry.References <= 0)
					{
						Remove(entry);
					}
				}
				Unreferenced.Clear();
			}
		}
	}
}
