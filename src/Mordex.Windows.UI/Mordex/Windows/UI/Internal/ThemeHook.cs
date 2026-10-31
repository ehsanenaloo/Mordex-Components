using System;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.UI.Internal
{
	// Lets a control listen to the default theme only while it paints with it. The control keeps the hook; the manager holds it weakly.
	internal sealed class ThemeHook : IThemeListener
	{
		private readonly Control _owner;

		private readonly Action _changed;

		private bool _listening;

		private bool _stale;

		internal ThemeHook(Control owner, Action changed = null)
		{
			_owner = owner;
			_changed = changed;
		}

		void IThemeListener.OnThemeChanged()
		{
			if (!_owner.IsHandleCreated || _owner.IsDisposed)
			{
				return;
			}
			// A hidden control (for example on another tab) catches up when it is shown.
			if (!_owner.Visible)
			{
				_stale = true;
				return;
			}
			Apply();
		}

		private void Apply()
		{
			_stale = false;
			_changed?.Invoke();
			_owner.Invalidate(true);
		}

		private void OnOwnerVisibleChanged(object sender, EventArgs e)
		{
			if (_stale && _owner.Visible && _owner.IsHandleCreated && !_owner.IsDisposed)
			{
				Apply();
			}
		}

		internal void Update(bool themed)
		{
			bool wanted = themed && _owner.IsHandleCreated;
			if (wanted && !_listening)
			{
				MxThemeManager.AddListener(this);
				_owner.VisibleChanged += OnOwnerVisibleChanged;
				_listening = true;
			}
			else if (!wanted && _listening)
			{
				Stop();
			}
		}

		internal void Stop()
		{
			if (_listening)
			{
				MxThemeManager.RemoveListener(this);
				_owner.VisibleChanged -= OnOwnerVisibleChanged;
				_listening = false;
				_stale = false;
			}
		}
	}
}
