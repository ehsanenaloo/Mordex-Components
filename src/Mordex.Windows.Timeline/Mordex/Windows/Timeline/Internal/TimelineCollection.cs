using System.Collections;
using System.Reflection;

namespace Mordex.Windows.Timeline.Internal
{
	[DefaultMember("Item")]
	internal class TimelineCollection : CollectionBase
	{
		public void AddRowLayout(MxRowLayoutState mordexRowLayoutState)
		{
			base.List.Add(mordexRowLayoutState);
		}

		public MxRowLayoutState GetRowLayoutAt(int index)
		{
			return (MxRowLayoutState)base.List[index];
		}
	}
}
