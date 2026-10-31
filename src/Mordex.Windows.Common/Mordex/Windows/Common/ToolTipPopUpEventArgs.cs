using System;
using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	public class ToolTipPopUpEventArgs : EventArgs
	{
		private Control _control;

		private SuperTipSettings _settings;

		private bool _cancel;

		public Control AssociatedControl => _control;

		public SuperTipSettings ToolTipSettings => _settings;

		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		internal ToolTipPopUpEventArgs(Control control, SuperTipSettings settings)
		{
			_control = control;
			_settings = settings;
		}
	}
}
