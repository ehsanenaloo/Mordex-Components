using Mordex.Data;

namespace Mordex.Windows.DataGrid.Data
{
	internal class VirtualDataRow : MordexRow
	{
		private GridDataSourceManager _manager;

		public override MordexTableBase Table => (MordexTableBase)_manager.RootTable;

		public override int RowManagerPosition => -2;

		internal VirtualDataRow(GridDataSourceManager manager)
		{
			_manager = manager;
		}

		public override bool IsVirtualRow()
		{
			return true;
		}
	}
}
