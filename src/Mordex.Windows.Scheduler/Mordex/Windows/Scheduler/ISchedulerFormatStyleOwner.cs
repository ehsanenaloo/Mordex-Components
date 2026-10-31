using System.Drawing;

namespace Mordex.Windows.Scheduler
{
	internal interface ISchedulerFormatStyleOwner
	{
		MxScheduler Scheduler { get; }

		int DefaultAlpha { get; }

		AlphaMode BackColorAlphaMode { get; }

		AlphaMode ForeColorAlphaMode { get; }

		float DefaultBlend { get; }

		Color BlendColor { get; }

		void InvalidateFormatStyleLayout();
	}
}
