namespace Mordex.Data
{
	public interface IMordexGroupTotal
	{
		IMordexField Field { get; set; }

		IMordexTable Table { get; }

		IMordexCondition Condition { get; set; }

		AggregateFunction AggregateFunction { get; set; }

		string Key { get; set; }
	}
}
