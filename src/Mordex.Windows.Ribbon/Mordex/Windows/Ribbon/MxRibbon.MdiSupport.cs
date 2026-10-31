using System;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	public partial class MxRibbon
	{
		private void OnFormMdiChildActivate(object sender, EventArgs eventArgs)
		{
			if (!IsParentedToForm())
			{
				return;
			}
			TrackActiveMdiChild();
			if (!GetHostForm().IsMdiContainer)
			{
				return;
			}
			if (_activeMdiChild != null && AllowMerge)
			{
				MxRibbon ribbon = FindRibbonInForm(_activeMdiChild);
				if (ribbon != null && ribbon.AllowMerge)
				{
					SetMergedRibbon(ribbon);
					_isAutoMerged = true;
					InvalidateFullLayout();
					return;
				}
			}
			if (AllowMerge && _isAutoMerged)
			{
				SetMergedRibbon(null);
			}
		}

		internal bool IsMdiChildMaximized()
		{
			if (_activeMdiChild != null)
			{
				return _activeMdiChild.WindowState == FormWindowState.Maximized;
			}
			return _isMdiChildMaximized;
		}

		internal void SetMdiChildMaximized(bool isMaximized)
		{
			if (_isMdiChildMaximized != isMaximized)
			{
				_isMdiChildMaximized = isMaximized;
				RequestTabStripLayout(false);
			}
		}

		internal Form GetMdiChildForm()
		{
			return _activeMdiChild;
		}

		private void TrackActiveMdiChild()
		{
			if (_activeMdiChild != null)
			{
				_activeMdiChild.SizeChanged -= OnFormSizeChanged;
			}
			if (_form != null)
			{
				_activeMdiChild = _form.ActiveMdiChild;
				if (_activeMdiChild != null)
				{
					if (_activeMdiChild.WindowState == FormWindowState.Maximized && _form.WindowState == FormWindowState.Maximized && _backBuffer == null)
					{
						NativeMethods.SendMessage(_activeMdiChild.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
						NativeMethods.SendMessage(_activeMdiChild.Handle, WmSysCommand, (IntPtr)ScRestore, IntPtr.Zero);
						NativeMethods.SendMessage(_activeMdiChild.Handle, WmSysCommand, (IntPtr)ScMaximize, IntPtr.Zero);
						NativeMethods.SendMessage(_activeMdiChild.Handle, WmSetRedraw, (IntPtr)1, IntPtr.Zero);
					}
					SetMdiChildMaximized(_activeMdiChild.WindowState == FormWindowState.Maximized);
					_activeMdiChild.SizeChanged += OnFormSizeChanged;
				}
				else
				{
					SetMdiChildMaximized(false);
				}
				RequestTabStripLayout(false);
			}
			else
			{
				_activeMdiChild = null;
				SetMdiChildMaximized(false);
			}
		}

		internal void CloseMdiChild()
		{
			if (_activeMdiChild != null)
			{
				_activeMdiChild.Close();
			}
		}

		internal void RestoreAllMdiChildren()
		{
			if (_form != null && _form.IsMdiContainer)
			{
				Form[] mdiChildren = _form.MdiChildren;
				foreach (Form form in mdiChildren)
				{
					form.SuspendLayout();
					form.WindowState = FormWindowState.Normal;
					form.Invalidate(invalidateChildren: true);
					form.ResumeLayout();
				}
			}
		}

		internal void MinimizeMdiChild()
		{
			if (_activeMdiChild != null)
			{
				_activeMdiChild.WindowState = FormWindowState.Minimized;
			}
		}
	}
}
