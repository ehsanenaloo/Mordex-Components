namespace Mordex.Windows.Timeline
{
	public interface ITimelineObject
	{
		ItemType ItemType { get; }

		TimelineItem Item { get; }

		TimelineGroupRow GroupRow { get; }

		bool Selected { get; set; }
	}
}
