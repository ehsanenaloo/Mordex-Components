namespace Mordex.Windows.DataGrid.Internal
{
	internal class ColumnCellPlacement
	{
		internal int _col;

		internal int _row;

		internal int _colSpan;

		internal int _rowSpan;

		internal GridColumn _column;

		internal ColumnCellPlacement(GridColumn column, int row, int col)
			: this(column, row, col, 1, 1)
		{
		}

		internal ColumnCellPlacement(GridColumn column, int row, int col, int rowSpan, int colSpan)
		{
			_column = column;
			_col = col;
			_row = row;
			_rowSpan = rowSpan;
			_colSpan = colSpan;
		}
	}
}
