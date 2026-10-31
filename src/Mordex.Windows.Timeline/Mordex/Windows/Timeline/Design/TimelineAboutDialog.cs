using Mordex.Windows.Common;

namespace Mordex.Windows.Timeline.Design
{
	internal class TimelineAboutDialog : MxAboutDialog
	{
		internal TimelineAboutDialog()
			: base(typeof(TimelineAboutDialog).Assembly)
		{
		}

		internal void ShowAboutDialog(MxTimeline timeline)
		{
			ShowDialog();
		}
	}
}
