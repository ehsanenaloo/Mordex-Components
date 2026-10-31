using System.Windows.Forms;

namespace Mordex.Windows.Common
{
	internal class SuperTipMessageFilter : IMessageFilter
	{
		private MxSuperTip _owner;

		internal SuperTipMessageFilter(MxSuperTip owner)
		{
			_owner = owner;
			Application.AddMessageFilter(this);
		}

		internal void Detach()
		{
			Application.RemoveMessageFilter(this);
			_owner = null;
		}

		public bool PreFilterMessage(ref Message message)
		{
			if (message.Msg == 512 && _owner != null)
			{
				_owner.ShowAreaSuperTip();
			}
			return false;
		}
	}
}
