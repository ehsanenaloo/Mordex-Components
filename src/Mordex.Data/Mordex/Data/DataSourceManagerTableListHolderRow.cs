namespace Mordex.Data
{
	internal class DataSourceManagerTableListHolderRow : TableListHolderRow
	{
		private DataSourceManager _manager;

		public override DataSourceManager DataSourceManager => _manager;

		internal DataSourceManagerTableListHolderRow(DataSourceManager manager, IMordexTable table)
			: base(table)
		{
			_manager = manager;
			SetShouldRetrieveChildRows(false);
		}
	}
}
