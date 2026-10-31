using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mordex.Windows.Timeline.Internal
{
	[DefaultMember("Item")]
	internal class TimelineItemLaneCollection : CollectionBase
	{
		public TimelineItemLane AddLane()
		{
			TimelineItemLane lane = new TimelineItemLane();
			lane.SetLaneIndex(base.List.Count);
			base.List.Add(lane);
			return lane;
		}
	}
}
