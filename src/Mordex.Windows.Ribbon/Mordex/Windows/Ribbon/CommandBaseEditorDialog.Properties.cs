using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mordex.Windows.Ribbon
{
	internal partial class CommandBaseEditorDialog
	{
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				if (!_isDesignMode)
				{
					createParams.Parent = IntPtr.Zero;
					createParams.Style = int.MinValue;
					createParams.Style |= WsClipSiblings;
					createParams.Style |= WsClipChildren;
					createParams.ExStyle += WsExTopMost;
					createParams.ExStyle += WsExToolWindow;
					createParams.ExStyle += WsExNoActivate;
				}
				return createParams;
			}
		}

		public override Cursor Cursor
		{
			get
			{
				if (!base.IsDisposed && !base.Disposing)
				{
					Point pt = PointToClient(Cursor.Position);
					if (_resizeGripBounds.Contains(pt))
					{
						if (_dropDownResizeMode == DropDownResizeMode.Height)
						{
							return Cursors.SizeNS;
						}
						return Cursors.SizeNWSE;
					}
				}
				return base.Cursor;
			}
			set
			{
				base.Cursor = value;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		internal virtual CommandBaseRenderer FocusItem
		{
			get
			{
				return _focusedItem;
			}
			set
			{
				if (_focusedItem == value)
				{
					return;
				}
				if (_focusedItem != null)
				{
					if (value == null && GetChildMenu() != null && _focusedItem == _childMenu._sourceItem)
					{
						return;
					}
					_focusedItem.Hot = false;
					if (value != null)
					{
						StopHoverTimer();
					}
					if (_focusedItem.GetCommand() is IMouseEventInvoker)
					{
						((IMouseEventInvoker)_focusedItem.GetCommand()).InvokeMouseLeave();
					}
				}
				if (value != null)
				{
					if (value.GetCommand() is IMouseEventInvoker)
					{
						((IMouseEventInvoker)value.GetCommand()).InvokeMouseEnter();
					}
					if (!value.Focusable)
					{
						value = null;
					}
				}
				_focusedItem = value;
				if (_focusedItem != null)
				{
					if (_owner is CommandBaseEditorDialog)
					{
						((CommandBaseEditorDialog)_owner).FocusItem = null;
					}
					if (GetChildMenu() != null && value != null)
					{
						if (GetChildMenu()._sourceItem != value)
						{
							StartChildCloseTimer(400);
						}
						else
						{
							StopChildCloseTimer();
						}
					}
					if (_focusedItem != GetSelectedItem())
					{
						if (GetChildMenu() != null && _childMenu._sourceItem == _focusedItem)
						{
							SetSelectedItem(_focusedItem);
						}
						else
						{
							SetSelectedItem(null);
						}
					}
					_focusedItem.Hot = true;
				}
				UpdateSuperTip(_focusedItem);
				Invalidate();
			}
		}
	}
}
