namespace Mordex.Windows.UI.StatusBar
{
	public class StatusBarEventArgs
	{
		private MxStatusBarPanel _panel;

		public MxStatusBarPanel Panel => _panel;

		internal StatusBarEventArgs(MxStatusBarPanel panel)
		{
			_panel = panel;
		}
	}
}
