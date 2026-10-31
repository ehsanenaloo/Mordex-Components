using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Mordex.Windows.Common;
using Mordex.Windows.UI;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class CheckBase
	{
		protected override void OnRightToLeftChanged(EventArgs e)
		{
			InvalidateLayout();
			base.OnRightToLeftChanged(e);
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			InvalidateLayout();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			RenderingHelper.SetMouseCapture(this, false);
			Invalidate();
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			Form form = RenderingHelper.FindForm(this);
			if (form != null && form.ContainsFocus)
			{
				SetIsHot(true);
			}
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetIsHot(false);
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			_windowsAppearanceState.ApplyVisualStyle();
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs me)
		{
			base.OnMouseMove(me);
			if (!base.Enabled)
			{
				return;
			}
			Form form = RenderingHelper.FindForm(this);
			if (form != null && form.ContainsFocus)
			{
				if (!base.ClientRectangle.Contains(me.X, me.Y))
				{
					SetIsHot(false);
				}
				else
				{
					SetIsHot(true);
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			RenderingHelper.TryFocusControl(this);
			if (e.Button == MouseButtons.Left)
			{
				SetIsPressed(true);
			}
		}

		protected override void OnPaddingChanged(EventArgs e)
		{
			base.OnPaddingChanged(e);
			InvalidateLayout();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			InvalidateLayout();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			InvalidateLayout();
		}
	}
}
