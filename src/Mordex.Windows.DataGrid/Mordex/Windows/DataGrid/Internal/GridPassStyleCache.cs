using System.Collections.Generic;

namespace Mordex.Windows.DataGrid.Internal
{
	// Styles that many rows of one paint pass would build again with the same result: the row style of a table and
	// row parity, and that style merged with a column style.
	internal sealed class GridPassStyleCache
	{
		private readonly Dictionary<(GridTable, int), GridFormatStyle> _rowStyles = new Dictionary<(GridTable, int), GridFormatStyle>();

		private readonly Dictionary<(GridFormatStyle, GridColumn), GridFormatStyle> _cellStyles = new Dictionary<(GridFormatStyle, GridColumn), GridFormatStyle>();

		private readonly List<GridFormatStyle> _kept = new List<GridFormatStyle>();

		internal GridFormatStyle GetRowStyle(MxDataGrid grid, GridTable table, int position)
		{
			(GridTable, int) key = (table, grid.UsesAlternatingColors ? position & 1 : 0);
			if (!_rowStyles.TryGetValue(key, out GridFormatStyle style))
			{
				style = grid.GetAlternatingRowStyle(table, position);
				Keep(style);
				_rowStyles[key] = style;
			}
			return style;
		}

		internal GridFormatStyle GetCellStyle(GridFormatStyle rowStyle, GridColumn column)
		{
			(GridFormatStyle, GridColumn) key = (rowStyle, column);
			if (!_cellStyles.TryGetValue(key, out GridFormatStyle style))
			{
				style = rowStyle.ApplyOverrides(column.CellStyle);
				if (style != rowStyle)
				{
					Keep(style);
				}
				_cellStyles[key] = style;
			}
			return style;
		}

		private void Keep(GridFormatStyle style)
		{
			style._isPassShared = true;
			_kept.Add(style);
		}

		internal void Release()
		{
			foreach (GridFormatStyle style in _kept)
			{
				style.ReleasePassShared();
			}
			_kept.Clear();
			_rowStyles.Clear();
			_cellStyles.Clear();
		}
	}
}
