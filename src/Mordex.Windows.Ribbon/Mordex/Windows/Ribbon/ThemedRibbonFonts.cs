using System;
using System.Collections.Generic;
using System.Drawing;

namespace Mordex.Windows.Ribbon
{
	/// <summary>Fonts that a paint needs again and again, kept so that a paint does not build a font.</summary>
	/// <remarks>
	/// A caller leases a font and disposes the lease when it no longer uses the font. The cache counts the leases of each font.
	/// A font with a lease is never disposed. A font without a lease stays cached for the next paint; when more than
	/// <see cref="IdleLimit"/> fonts have no lease, the one released longest ago is disposed. The cache belongs to one thread,
	/// and a lease is released on the thread that took it.
	/// </remarks>
	internal static class ThemedRibbonFonts
	{
		internal const int IdleLimit = 64;

		[ThreadStatic]
		private static FontCache _cache;

		internal static RibbonFontLease Bold(Font font)
		{
			return Lease(font.Name, font.Size, font.Style | FontStyle.Bold, font.Unit);
		}

		internal static RibbonFontLease Styled(string name, float size, FontStyle style, GraphicsUnit unit)
		{
			return Lease(name, size, style, unit);
		}

		/// <summary>The number of fonts the cache of this thread holds, leased or not.</summary>
		internal static int CachedCount => _cache?.Count ?? 0;

		private static RibbonFontLease Lease(string name, float size, FontStyle style, GraphicsUnit unit)
		{
			FontCache cache = _cache ?? (_cache = new FontCache());
			return new RibbonFontLease(cache.Acquire((name, size, style, unit)));
		}

		internal sealed class Entry
		{
			internal readonly (string, float, FontStyle, GraphicsUnit) Key;

			internal readonly Font Font;

			internal readonly FontCache Owner;

			internal int Leases;

			internal LinkedListNode<Entry> IdleNode;

			internal Entry((string, float, FontStyle, GraphicsUnit) key, FontCache owner)
			{
				Key = key;
				Owner = owner;
				Font = new Font(key.Item1, key.Item2, key.Item3, key.Item4);
			}
		}

		internal sealed class FontCache
		{
			private readonly Dictionary<(string, float, FontStyle, GraphicsUnit), Entry> _entries = new Dictionary<(string, float, FontStyle, GraphicsUnit), Entry>();

			// Fonts without a lease, the one released longest ago first.
			private readonly LinkedList<Entry> _idle = new LinkedList<Entry>();

			internal int Count => _entries.Count;

			internal Entry Acquire((string, float, FontStyle, GraphicsUnit) key)
			{
				if (!_entries.TryGetValue(key, out Entry entry))
				{
					entry = new Entry(key, this);
					_entries.Add(key, entry);
				}
				else if (entry.IdleNode != null)
				{
					_idle.Remove(entry.IdleNode);
					entry.IdleNode = null;
				}
				entry.Leases++;
				return entry;
			}

			internal void Release(Entry entry)
			{
				if (entry.Leases <= 0)
				{
					throw new InvalidOperationException("The font has no lease to release.");
				}
				entry.Leases--;
				if (entry.Leases > 0)
				{
					return;
				}
				entry.IdleNode = _idle.AddLast(entry);
				while (_idle.Count > IdleLimit)
				{
					Entry oldest = _idle.First.Value;
					_idle.RemoveFirst();
					oldest.IdleNode = null;
					_entries.Remove(oldest.Key);
					oldest.Font.Dispose();
				}
			}
		}
	}

	/// <summary>A font borrowed from <see cref="ThemedRibbonFonts"/>, or a font the caller keeps owning. Dispose it once the
	/// font is no longer used; do not dispose <see cref="Font"/> itself.</summary>
	internal sealed class RibbonFontLease : IDisposable
	{
		private ThemedRibbonFonts.Entry _entry;

		internal RibbonFontLease(ThemedRibbonFonts.Entry entry)
		{
			_entry = entry;
			Font = entry.Font;
		}

		// Wraps a font that the cache does not own; disposing the lease leaves it alone.
		internal RibbonFontLease(Font font)
		{
			Font = font;
		}

		internal Font Font { get; }

		public void Dispose()
		{
			ThemedRibbonFonts.Entry entry = _entry;
			if (entry != null)
			{
				_entry = null;
				entry.Owner.Release(entry);
			}
		}
	}
}
