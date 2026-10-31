using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Forms;
using Mordex.Data;
using Mordex.Windows.Common.Layouts;
using Mordex.Windows.DataGrid.Editors;
using Mordex.Windows.DataGrid.Internal;

namespace Mordex.Windows.DataGrid
{
	public partial class GridDropDown
	{
		private void InitializeDropDown()
		{
			SetDropDownActive(false);
			GetHorizontalScrollBar().MouseUp += OnScrollBarMouseUp;
			GetVScroll().MouseUp += OnScrollBarMouseUp;
			base.HideSelection = HideSelection.Highlight;
			base.AllowEdit = InheritableBoolean.False;
			base.GroupByBoxVisible = false;
			base.ExpandableGroups = InheritableBoolean.False;
			base.RootTable = new GridTable();
			base.Visible = false;
			_gridDropDownTimer = new Timer(_container);
			_gridDropDownTimer.Enabled = false;
			_gridDropDownTimer.Interval = Timing.AutoRepeatInterval;
			_gridDropDownTimer.Tick += OnAutoScrollTimerTick;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (!IsNotInEditorDialog())
			{
				base.OnMouseDown(e);
				return;
			}
			Cursor = GetCursorAtPointer();
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			_isMouseDownInside = true;
			if (base.ClientRectangle.Contains(e.X, e.Y))
			{
				base.OnMouseDown(e);
				_gridDropDownTimer.Enabled = true;
				base.CurrentColumn = null;
				return;
			}
			Point point = new Point(e.X, e.Y);
			Control control = null;
			try
			{
				control = GetChildAtPoint(point);
			}
			finally
			{
			}
			if (control == GetVScroll())
			{
				point = PointToScreen(point);
				point = control.PointToClient(point);
				((ScrollBarControl)control).ProcessMouseInput(e, point.X, point.Y);
			}
			else if (control == _scrollableBasePanel)
			{
				point = PointToScreen(point);
				point = control.PointToClient(point);
				control = control.GetChildAtPoint(point);
				if (control == GetHorizontalScrollBar())
				{
					point = _scrollableBasePanel.PointToScreen(point);
					point = control.PointToClient(point);
					((ScrollBarControl)control).ProcessMouseInput(e, point.X, point.Y);
				}
			}
			else
			{
				HideDropDown(true);
			}
		}

		protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
		{
			base.OnQueryContinueDrag(e);
			if (IsNotInEditorDialog() && e.Action != DragAction.Continue)
			{
				_gridDropDownTimer.Enabled = false;
				GridHelper.SetCapture(this, true);
				StartDeferredShowTimer();
			}
		}

		private void OnScrollBarMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (IsNotInEditorDialog())
			{
				_gridDropDownTimer.Enabled = false;
				if (IsDropDownOpenState())
				{
					GridHelper.SetCapture(this, true);
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (TryHandleMouseMove(e) || !IsNotInEditorDialog())
			{
				return;
			}
			if (e.Button == MouseButtons.Left)
			{
				switch (HitTest(e.X, e.Y))
				{
				case GridArea.RowHeader:
				case GridArea.Cell:
				{
					int rowPositionFromPoint = RowPositionFromPoint(e.X, e.Y);
					if (rowPositionFromPoint != -1)
					{
						base.Row = rowPositionFromPoint;
						Update();
					}
					break;
				}
				default:
					if (e.Button != MouseButtons.Left)
					{
						break;
					}
					if (e.Y < 0)
					{
						if (base.Row > 0)
						{
							base.Row--;
							Update();
						}
					}
					else if (e.Y > base.ClientRectangle.Height && base.FirstRow + _visibleRows < base.RowCount)
					{
						base.Row = base.FirstRow + _visibleRows;
						Update();
					}
					break;
				case GridArea.ColumnHeader:
					break;
				}
			}
			else
			{
				if (GetHorizontalScrollBar() != null)
				{
					GetHorizontalScrollBar().SetMouseOver(GetHorizontalScrollBar().Bounds.Contains(e.X, e.Y));
				}
				if (GetVScroll() != null)
				{
					GetVScroll().SetMouseOver(GetVScroll().Bounds.Contains(e.X, e.Y));
				}
			}
			base.CurrentColumn = null;
			if (e.Button == MouseButtons.None)
			{
				Cursor = GetCursorAtPointer();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (IsNotInEditorDialog())
			{
				_gridDropDownTimer.Enabled = false;
				GridArea gridArea = HitTest(e.X, e.Y);
				if (!_isMouseDownInside || gridArea == GridArea.ExpandBox)
				{
					StartDeferredShowTimer();
				}
				else if (gridArea == GridArea.Cell || gridArea == GridArea.PreviewRow || !base.ClientRectangle.Contains(e.X, e.Y))
				{
					HideDropDown(base.Row < 0);
				}
				else
				{
					StartDeferredShowTimer();
				}
			}
		}

		internal void SetDropDownWidth(int gridDropDownInteger)
		{
			_dropDownWidth = gridDropDownInteger;
		}

		internal void ShowDropDown(Rectangle otherBounds, object value)
		{
			if (!base.IsHandleCreated)
			{
				CreateControl();
			}
			Cursor = Cursors.Arrow;
			Visible = false;
			if (value is MxMultiColumnComboBox)
			{
				base.Parent = value as Control;
			}
			((ScrollableBase)this).SetHorizontalScrollRequired(false);
			_dropListOwner = (IGridDropListOwner)value;
			_dropDownWidth = otherBounds.Width;
			ResizeToFitRows(otherBounds.Width);
			_originalParentHandle = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			Point location = ((RightToLeft != RightToLeft.No) ? new Point(otherBounds.Right - base.Width, otherBounds.Bottom + 1) : new Point(otherBounds.X, otherBounds.Bottom + 1));
			Screen screen = Screen.FromRectangle(otherBounds);
			if (screen == null)
			{
				screen = Screen.PrimaryScreen;
			}
			Rectangle bounds = screen.Bounds;
			if (location.X + base.Width > bounds.Right)
			{
				location.X = bounds.Right - base.Width;
			}
			if (location.X < bounds.Left)
			{
				location.X = bounds.Left;
			}
			if (location.Y + base.Height > bounds.Bottom)
			{
				if (otherBounds.Y > bounds.Bottom - otherBounds.Bottom)
				{
					location.Y = otherBounds.Y - base.Height;
					if (location.Y < bounds.Top)
					{
						location.Y = bounds.Top;
						base.Height = otherBounds.Y;
					}
				}
				else
				{
					base.Height = bounds.Bottom - otherBounds.Bottom;
				}
			}
			base.Location = location;
			SetDropDownOpenState(true);
			base.CurrentColumn = null;
			RefreshRowCountAndInvalidate();
			NativeMethods.SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 3);
			Visible = true;
		}

		internal void HideDropDown(bool cancel)
		{
			if (!IsDropDownOpenState())
			{
				return;
			}
			SetDropDownOpenState(false);
			GridHelper.SetCapture(this, false);
			NativeMethods.ShowWindow(base.Handle, 0);
			if (_originalParentHandle != IntPtr.Zero)
			{
				NativeMethods.SetParent(base.Handle, _originalParentHandle);
				_originalParentHandle = IntPtr.Zero;
			}
			if (_dropListOwner != null)
			{
				_dropListOwner.OnDropListHide(cancel);
				if (IsDropDownOpenState())
				{
					return;
				}
				_dropListOwner = null;
			}
			_gridDropDownTimer.Enabled = false;
			_isMouseDownInside = false;
		}
	}
}
