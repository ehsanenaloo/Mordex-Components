using System;
using System.Collections;
using System.Collections.Generic;

namespace Mordex.Windows.DataGrid.Export
{
	internal class ExportStyleTable
	{
		private ArrayList _styles;

		// HasSameFormatting compares only the number format when neither side has formatting, and every field
		// otherwise. These maps hold the first matching position for each case, so a lookup returns the same
		// style as a scan of the list in order.
		private readonly Dictionary<ValueTuple<string>, int> _firstUnformatted = new Dictionary<ValueTuple<string>, int>();

		private readonly Dictionary<ExportAppearanceState.FormattingKey, int> _firstFormatted = new Dictionary<ExportAppearanceState.FormattingKey, int>();

		private readonly Dictionary<ExportAppearanceState.FormattingKey, int> _firstAny = new Dictionary<ExportAppearanceState.FormattingKey, int>();

		public ExportStyleTable()
		{
			_styles = new ArrayList();
		}

		public string GetOrAddStyleName(ExportAppearanceState exportAppearanceState)
		{
			if (exportAppearanceState.IsEmpty())
			{
				return null;
			}
			ExportAppearanceState.FormattingKey key = exportAppearanceState.GetFormattingKey();
			int match = FindMatch(key, exportAppearanceState.HasFormatting());
			if (match >= 0)
			{
				return ((ExportAppearanceState)_styles[match]).GetName();
			}
			if (exportAppearanceState.GetName() == null)
			{
				exportAppearanceState.SetName("s" + (_styles.Count + 1));
			}
			int index = _styles.Add(exportAppearanceState);
			_firstAny.TryAdd(key, index);
			if (exportAppearanceState.HasFormatting())
			{
				_firstFormatted.TryAdd(key, index);
			}
			else
			{
				_firstUnformatted.TryAdd(ValueTuple.Create(key.NumberFormat), index);
			}
			return exportAppearanceState.GetName();
		}

		private int FindMatch(ExportAppearanceState.FormattingKey key, bool hasFormatting)
		{
			if (hasFormatting)
			{
				return _firstAny.TryGetValue(key, out int any) ? any : -1;
			}
			int unformatted = _firstUnformatted.TryGetValue(ValueTuple.Create(key.NumberFormat), out int u) ? u : -1;
			int formatted = _firstFormatted.TryGetValue(key, out int f) ? f : -1;
			if (unformatted < 0)
			{
				return formatted;
			}
			return formatted < 0 ? unformatted : Math.Min(unformatted, formatted);
		}

		public ArrayList GetArrayList()
		{
			return _styles;
		}
	}
}
