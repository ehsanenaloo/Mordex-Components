namespace Mordex.Data
{
	public interface IMordexSortField
	{
		IMordexField Field { get; set; }

		IMordexTable Table { get; }

		SortOrder SortOrder { get; set; }
	}
}
