namespace Mordex.Data.Internal
{
	public class MordexUnboundDataRow
	{
		internal UnboundFieldValues _fieldValues;

		private object _bookmark;

		public object Bookmark => _bookmark;

		public MordexUnboundDataRow(object bookmark)
		{
			_bookmark = bookmark;
		}
	}
}
