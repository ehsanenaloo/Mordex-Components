using System;

namespace Mordex.Windows.Timeline.Design.Designer
{
	internal interface ITimelineDesignerPage
	{
		MxTimeline Timeline { get; set; }

		event EventHandler SelectTab;

		bool CommitChanges();

		void Unload();
	}
}
