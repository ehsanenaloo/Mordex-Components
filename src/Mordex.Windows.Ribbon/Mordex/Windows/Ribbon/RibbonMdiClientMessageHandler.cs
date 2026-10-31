using System;
using System.Collections;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal class RibbonMdiClientMessageHandler : NativeWindow
	{
		private static ArrayList s_handlers;

		private MdiClient _mdiClient;

		private bool _isHandleAssigned;

		internal static void Attach(MdiClient ctl)
		{
			if (s_handlers == null)
			{
				s_handlers = new ArrayList();
			}
			foreach (RibbonMdiClientMessageHandler item in s_handlers)
			{
				if (item._mdiClient == ctl)
				{
					return;
				}
			}
			RibbonMdiClientMessageHandler value = new RibbonMdiClientMessageHandler(ctl, menu: true, tabs: false);
			s_handlers.Add(value);
		}

		internal static void Detach(MdiClient mdiClient)
		{
			if (s_handlers == null || mdiClient == null)
			{
				return;
			}
			RibbonMdiClientMessageHandler existingHandler = null;
			foreach (RibbonMdiClientMessageHandler item in s_handlers)
			{
				if (item._mdiClient == mdiClient)
				{
					existingHandler = item;
					break;
				}
			}
			if (existingHandler != null)
			{
				existingHandler.Unhook();
				s_handlers.Remove(existingHandler);
			}
		}

		private RibbonMdiClientMessageHandler(MdiClient ctl, bool menu, bool tabs)
		{
			_mdiClient = ctl;
			if (ctl.Visible)
			{
				AssignHandle(ctl.Handle);
			}
			ctl.VisibleChanged += OnControlVisibleChanged;
			ctl.HandleDestroyed += OnControlHandleDestroyed;
			ctl.HandleCreated += OnControlHandleCreated;
		}

		private void OnControlVisibleChanged(object sender, EventArgs eventArgs)
		{
			if (_mdiClient != null && _mdiClient.Visible && !_isHandleAssigned)
			{
				AssignHandle(_mdiClient.Handle);
			}
		}

		private void OnControlHandleDestroyed(object sender, EventArgs eventArgs)
		{
			ReleaseHandle();
			_isHandleAssigned = false;
		}

		private void OnControlHandleCreated(object sender, EventArgs eventArgs)
		{
			AssignHandle(_mdiClient.Handle);
			_isHandleAssigned = true;
		}

		internal void Unhook()
		{
			ReleaseHandle();
			_mdiClient.VisibleChanged -= OnControlVisibleChanged;
			_mdiClient.HandleCreated -= OnControlHandleCreated;
			_mdiClient.HandleDestroyed -= OnControlHandleDestroyed;
			_mdiClient = null;
		}

		protected override void WndProc(ref Message message)
		{
			if (message.Msg == 560 || message.Msg == 564)
			{
				message.Result = IntPtr.Zero;
			}
			else
			{
				base.WndProc(ref message);
			}
		}
	}
}
