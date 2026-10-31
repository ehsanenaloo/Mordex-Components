using System.Windows.Forms;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Docking
{
	public partial class MxDockManager
	{
		private ThemeCallback _themeCallback;

		private void UpdateThemeListening()
		{
			bool themed = !_isDisposing;
			if (_themeCallback == null)
			{
				if (!themed)
				{
					return;
				}
				_themeCallback = new ThemeCallback(OnDefaultThemeChanged);
			}
			_themeCallback.Update(themed);
		}

		private void OnDefaultThemeChanged()
		{
			Control owner = ContainerControl;
			if (owner == null || owner.IsDisposed)
			{
				return;
			}
			if (owner.InvokeRequired)
			{
				owner.BeginInvoke(new MethodInvoker(ApplyDefaultTheme));
			}
			else
			{
				ApplyDefaultTheme();
			}
		}

		private void ApplyDefaultTheme()
		{
			if (_isDisposing || ContainerControl == null)
			{
				return;
			}
			CreateRenderers();
			InvalidatePanels(_panels);
			LayoutAutoHideStrips();
			RefreshMdiTabGroups();
			ContainerControl.Invalidate(true);
		}

		private void StopThemeListening()
		{
			_themeCallback?.Update(false);
		}
	}
}
