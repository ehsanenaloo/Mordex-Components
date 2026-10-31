
namespace Mordex.Windows.Timeline.Internal
{
	internal class TimelineItemLane
	{
		private double _doubleValue;

		private int _laneIndex = -1;

		public double GetDoubleValue()
		{
			return _doubleValue;
		}

		public void SetDoubleValue(double doubleValue)
		{
			_doubleValue = doubleValue;
		}

		public int GetLaneIndex()
		{
			return _laneIndex;
		}

		public void SetLaneIndex(int laneIndex)
		{
			_laneIndex = laneIndex;
		}
	}
}
