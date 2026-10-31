using Mordex.Data;

namespace Mordex.Windows.DataGrid.Data
{
	internal class DataMordexRow : MordexRow
	{
		private GridDataSourceManager _manager;

		public override MordexTableBase Table => (MordexTableBase)_manager.RootTable;

		public override int RowManagerPosition => -3;

		internal DataMordexRow(GridDataSourceManager manager)
		{
			_manager = manager;
		}

		public override bool IsVirtualRow()
		{
			return true;
		}
	}
}
