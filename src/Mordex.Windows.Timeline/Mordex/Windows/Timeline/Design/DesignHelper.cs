using System;
using System.Windows.Forms;
using Mordex.Windows.Timeline.Design.Designer;

namespace Mordex.Windows.Timeline.Design
{
	public static class DesignHelper
	{


		public static void ShowAboutBox(MxTimeline timeline)
		{
			TimelineAboutDialog timelineAboutDialog = new TimelineAboutDialog();
			timelineAboutDialog.ShowAboutDialog(timeline);
			timelineAboutDialog.Dispose();
		}

		public static DialogResult ShowTimelineDesigner(MxTimeline timeline)
		{
			TimelineDesignerDialog timelineDesignerDialog = new TimelineDesignerDialog();
			timelineDesignerDialog.UpdateTimeline(timeline);
			DialogResult dialogResult = timelineDesignerDialog.DialogResult;
			timelineDesignerDialog.Dispose();
			return dialogResult;
		}

	}
}
