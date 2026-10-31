
namespace Mordex.Windows.Ribbon
{
	internal class RibbonKeyTipEntry
	{
		private string _keyTip;

		private object _command;

		private SuperTipSettingsWindowMessageHandler _toolTip;

		private bool _enabled;

		private CommandBaseRenderer _renderer;

		public RibbonKeyTipEntry(string keyTip, object command, CommandBaseRenderer layoutItem, SuperTipSettingsWindowMessageHandler toolTip, bool enabled)
		{
			_keyTip = keyTip;
			_command = command;
			_toolTip = toolTip;
			_enabled = enabled;
			_renderer = layoutItem;
		}

		public void DisposeToolTip()
		{
			if (_toolTip != null)
			{
				_toolTip.DisposeTimersAndBitmaps();
			}
		}

		public bool IsEnabled()
		{
			return _enabled;
		}

		public string GetKeyTip()
		{
			return _keyTip;
		}

		public object GetCommand()
		{
			return _command;
		}

		public CommandBaseRenderer GetRenderer()
		{
			return _renderer;
		}
	}
}
