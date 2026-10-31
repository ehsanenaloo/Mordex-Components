namespace Mordex.Data
{
	public interface IMordexStoredFilter
	{
		string Key { get; set; }

		object Tag { get; set; }

		string Description { get; set; }

		IMordexCondition Condition { get; set; }

		IMordexTable Table { get; }
	}
}
