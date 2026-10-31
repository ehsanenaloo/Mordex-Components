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
using Mordex.Windows.UI.CommandBars;
using Mordex.Windows.UI.Design;
using Mordex.Windows.UI.Internal;

namespace Mordex.Windows.UI.Controls
{
	public partial class MxButton
	{
		public ButtonHitTest HitTest()
		{
			Point position = Cursor.Position;
			position = PointToClient(position);
			return HitTest(position.X, position.Y);
		}

		public ButtonHitTest HitTest(Point pt)
		{
			return HitTest(pt.X, pt.Y);
		}

		public ButtonHitTest HitTest(int x, int y)
		{
			if (base.ClientRectangle.Contains(x, y))
			{
				if (_dropDownBounds.Contains(x, y))
				{
					return ButtonHitTest.DropDownButton;
				}
				return ButtonHitTest.Button;
			}
			// The open menu is a top-level window, so its bounds are in screen coordinates.
			if (_dropDownContextMenu != null && _dropDownContextMenu.GetPopupControl() != null && _dropDownContextMenu.GetPopupControl().Bounds.Contains(PointToScreen(new Point(x, y))))
			{
				return ButtonHitTest.DropDownMenu;
			}
			return ButtonHitTest.None;
		}

		protected override bool ProcessMnemonic(char charCode)
		{
			if (base.Enabled && base.Visible && Control.IsMnemonic(charCode, Text))
			{
				SetPressed(true);
				PerformClick();
				SetPressed(false);
				return true;
			}
			return base.ProcessMnemonic(charCode);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new PushButtonAccessibleObject(this);
		}

		protected override void OnClick(EventArgs e)
		{
			if (!_isDropDownOpen && !_isRightClickPending)
			{
				Form form = RenderingHelper.FindForm(this);
				if (form != null && DialogResult != DialogResult.None)
				{
					form.DialogResult = DialogResult;
				}
				if ((ResolveButtonStyle() == ButtonStyle.DropDown || ResolveButtonStyle() == ButtonStyle.DropDownButton) && _dropDownContextMenu != null && (ResolveButtonStyle() == ButtonStyle.DropDown || _dropDownBounds.Contains(PointToClient(Cursor.Position))))
				{
					return;
				}
				if (GetAssociatedCommand() != null)
				{
					GetAssociatedCommand().InvokeOnClick();
				}
				base.OnClick(e);
			}
			_isRightClickPending = false;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			InvalidateLayoutAndPaint();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			Invalidate();
			SetHot(false);
			SetDropDownHot(false);
			SetPressed(false);
			base.OnLostFocus(e);
			RenderingHelper.SetMouseCapture(this, false);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (base.Enabled)
			{
				if (_dropDownBounds.Contains(e.X, e.Y))
				{
					SetDropDownHot(true);
				}
				else if (GetAssociatedCommand() == null || GetAssociatedCommand().IsEnabled)
				{
					SetDropDownHot(false);
					if (base.ClientRectangle.Contains(e.X, e.Y))
					{
						SetHot(true);
					}
					else
					{
						SetHot(false);
					}
				}
			}
			else
			{
				SetHot(false);
				SetDropDownHot(false);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (base.Enabled && e.Button == MouseButtons.Left)
			{
				RenderingHelper.TryFocusControl(this);
				if (!base.IsDisposed)
				{
					if (_dropDownBounds.Contains(e.X, e.Y))
					{
						SetDropDownPressed(true);
					}
					else if (GetAssociatedCommand() == null || GetAssociatedCommand().IsEnabled)
					{
						SetPressed(true);
					}
					if ((ResolveButtonStyle() == ButtonStyle.DropDown || ResolveButtonStyle() == ButtonStyle.DropDownButton) && _dropDownContextMenu != null && (ResolveButtonStyle() == ButtonStyle.DropDown || _dropDownBounds.Contains(e.X, e.Y)))
					{
						DropDown();
					}
				}
			}
			else if (e.Button == MouseButtons.Right)
			{
				_isRightClickPending = true;
			}
		}

		public void DropDown()
		{
			if (_dropDownContextMenu != null)
			{
				if (!_isDropDownOpen)
				{
					SetDropDownPressed(true);
					Update();
					_isDropDownOpen = true;
					_dropDownContextMenu._ownerHeight = base.Height;
					_dropDownContextMenu._ownerWidth = base.Width;
					_dropDownContextMenu.Closed += OnDropDownContextMenuClosed;
					try
					{
						_dropDownContextMenu.RightToLeft = RightToLeft;
						_dropDownContextMenu.Show(this, new Point(base.ClientRectangle.Left, base.ClientRectangle.Bottom));
					}
					finally
					{
						_dropDownContextMenu.Closed -= OnDropDownContextMenuClosed;
						_dropDownContextMenu._ownerHeight = -1;
						_dropDownContextMenu._ownerWidth = -1;
						_isDropDownOpen = false;
						SetDropDownPressed(false);
						SetDropDownHot(false);
						SetPressed(false);
						SetHot(false);
						Invalidate();
					}
				}
				else
				{
					_dropDownContextMenu.Close();
					_isDropDownOpen = false;
					SetDropDownPressed(false);
					SetPressed(false);
				}
			}
		}

		private void OnDropDownContextMenuClosed(object sender, EventArgs eventArgs)
		{
			_isDropDownOpen = false;
			SetDropDownPressed(false);
			SetDropDownHot(false);
			SetPressed(false);
			SetHot(false);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			_isRightClickPending = false;
			SetDropDownPressed(false);
			SetPressed(false);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == MouseLeaveMessage)
			{
				SetHot(false);
			}
			base.WndProc(ref m);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			SetDropDownHot(false);
			SetHot(false);
		}

		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
			InvalidateLayoutAndPaint();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled || !base.Enabled || !base.Visible)
			{
				return;
			}
			if (e.KeyData == Keys.Down || (e.KeyData == Keys.Up && ResolveButtonStyle() == ButtonStyle.DropDown))
			{
				SetHot(true);
				SetPressed(true);
				PerformClick();
			}
			else if (e.KeyData == Keys.Return || e.KeyData == Keys.Space)
			{
				SetHot(true);
				SetPressed(true);
				if (e.KeyData == Keys.Return)
				{
					PerformClick();
				}
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (IsPressed())
			{
				if (e.KeyData == Keys.Return || e.KeyData == Keys.Space)
				{
					SetHot(false);
					SetPressed(false);
				}
				if (!e.Handled && base.Enabled && base.Visible && e.KeyData == Keys.Space)
				{
					PerformClick();
				}
			}
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Up:
			case Keys.Down:
				if (ResolveButtonStyle() == ButtonStyle.DropDown)
				{
					return true;
				}
				return base.IsInputKey(keyData);
			default:
				return base.IsInputKey(keyData);
			}
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			SetHot(false);
			SetPressed(false);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			InvalidateLayoutAndPaint();
		}
	}
}
