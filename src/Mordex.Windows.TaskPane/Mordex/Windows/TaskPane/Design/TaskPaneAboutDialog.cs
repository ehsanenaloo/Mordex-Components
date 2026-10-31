using Mordex.Windows.Common;

namespace Mordex.Windows.TaskPane.Design
{
	internal class TaskPaneAboutDialog : MxAboutDialog
	{
		internal TaskPaneAboutDialog()
			: base(typeof(TaskPaneAboutDialog).Assembly)
		{
		}

		internal void ShowAbout(MxTaskPane taskPane)
		{
			ShowDialog();
		}
	}
}
