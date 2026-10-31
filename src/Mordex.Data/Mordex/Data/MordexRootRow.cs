namespace Mordex.Data
{
	public class MordexRootRow : MordexTableRow
	{
		private DataSourceManager _dataSourceManager;

		protected override TableListHolderRow BaseList
		{
			get
			{
				if (_dataSourceManager != null)
				{
					return _dataSourceManager.GetRootListHolderRow();
				}
				return null;
			}
		}

		public override DataSourceManager DataSourceManager => _dataSourceManager;

		public override MordexNewRowPosition NewRowPosition
		{
			get
			{
				MordexNewRowPosition newRowPosition = base.NewRowPosition;
				if (newRowPosition == MordexNewRowPosition.Bottom)
				{
					return newRowPosition;
				}
				return MordexNewRowPosition.None;
			}
		}

		internal MordexRootRow(DataSourceManager manager, bool preventGroups)
			: base((MordexTableBase)manager.RootTable, preventGroups)
		{
			_dataSourceManager = manager;
		}

		public override bool CanHaveChildren()
		{
			return true;
		}

		protected override void OnRowChange(MordexRow row, RowChangeType change)
		{
			base.OnRowChange(row, change);
			if (DataSourceManager != null)
			{
				DataSourceManager.HandleRowChange(row, change, this);
			}
		}
	}
}
