namespace Mordex.Data
{
	public class MordexTotalRow : MordexRow
	{
		private MordexRow _parentRow;

		public override MordexTableBase Table => _parentRow.Table;

		public override MordexRow ParentRow => _parentRow;

		public override MordexGroupBase Group
		{
			get
			{
				if (_parentRow is MordexGroupRow)
				{
					return _parentRow.Group;
				}
				return null;
			}
		}

		internal MordexTotalRow(MordexRow parentRow)
		{
			_parentRow = parentRow;
		}

		public override bool IsRowVisible()
		{
			if (_parentRow is MordexGroupRow mordexGroupRow)
			{
				MordexGroupBase mordexGroupBase = mordexGroupRow.Group;
				if (mordexGroupBase != null && mordexGroupBase.GetEffectiveTotalVisibility() == GroupTotalVisibility.Visible)
				{
					return mordexGroupRow.IsRowVisible();
				}
			}
			return base.IsRowVisible();
		}
	}
}
