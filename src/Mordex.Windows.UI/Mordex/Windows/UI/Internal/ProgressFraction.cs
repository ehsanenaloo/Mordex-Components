using System;

namespace Mordex.Windows.UI.Internal
{
	internal static class ProgressFraction
	{
		/// <summary>
		/// How far the value is from the minimum to the maximum, from 0 to 1. The differences use long arithmetic, so the full int
		/// range does not overflow. An empty or reversed range gives 0; a value outside the range is clamped.
		/// </summary>
		internal static double Of(int value, int minimum, int maximum)
		{
			long range = (long)maximum - minimum;
			if (range <= 0)
			{
				return 0.0;
			}
			double fraction = ((long)value - minimum) / (double)range;
			return Math.Max(0.0, Math.Min(1.0, fraction));
		}
	}
}
