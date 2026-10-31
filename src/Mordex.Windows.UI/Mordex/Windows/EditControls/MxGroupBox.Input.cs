using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Docking;
using Mordex.Windows.UI.Internal;
using Mordex.Windows.UI.Tabs;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxGroupBox
	{
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			base.OnResize(EventArgs.Empty);
			InvalidateLayout();
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
			InvalidateLayout();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			InvalidateLayout();
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			Invalidate();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WmHScroll || m.Msg == WmVScroll)
			{
				Invalidate(invalidateChildren: true);
			}
			base.WndProc(ref m);
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			InvalidateLayout();
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			if (base.Enabled && base.Visible && Control.IsMnemonic(charCode, Text))
			{
				SelectNextControl(this, forward: true, tabStopOnly: true, nested: false, wrap: true);
				return true;
			}
			return base.ProcessMnemonic(charCode);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			base.OnResize(EventArgs.Empty);
			InvalidateLayout();
		}
	}
}
