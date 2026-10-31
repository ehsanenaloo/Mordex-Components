namespace Mordex.Data
{
	public interface IMordexCustomGroupRow
	{
		string GroupCaption { get; set; }

		IMordexCondition Condition { get; set; }

		bool ShowWhenEmpty { get; set; }

		bool ShowRemainingRows { get; set; }
	}
}
