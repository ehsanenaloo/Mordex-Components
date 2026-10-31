using System;
using System.Windows.Forms;

namespace Mordex.Windows.UI.Docking.Internal
{
	internal class DockMessageFilter : IMessageFilter
	{
		private IDockElement _owner;

		private Form _form;

		internal DockMessageFilter(IDockElement owner)
		{
			_owner = owner;
			if (owner is Control)
			{
				_form = RenderingHelper.FindForm((Control)owner);
				if (_form != null)
				{
					_form.Deactivate += OnFormDeactivate;
				}
			}
		}

		private void OnFormDeactivate(object sender, EventArgs eventArgs)
		{
			if (_owner.AllowCancelDrag)
			{
				_owner.CancelSizing();
			}
		}

		internal void DetachFromForm()
		{
			if (_form != null)
			{
				_form.Deactivate -= OnFormDeactivate;
				_form = null;
			}
		}

		bool IMessageFilter.PreFilterMessage(ref Message message)
		{
			switch (message.Msg)
			{
			case 256:
				if ((int)message.WParam == 27)
				{
					_owner.CancelSizing();
				}
				break;
			case 15:
				_owner.EraseSizingBar();
				break;
			case 162:
			case 165:
			case 168:
			case 172:
			case 517:
			case 520:
			case 524:
				return true;
			}
			return false;
		}
	}
}
