using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Mordex.Windows.Common;

namespace Mordex.Windows.Scheduler
{
	internal class SchedulerFormatStyleScrollableBase : ScrollableBase
	{
		private Container _container = new Container();

		private Timer _dragTimer;

		internal SchedulerFormatStyle _schedulerFormatStyle;

		protected int _itemHeight;

		private bool _isEdgeScrolling;

		private bool _isScrollingUp = true;

		internal bool _isDropListShown;

		private IntPtr _intPtr = IntPtr.Zero;

		private IDropListOwner _dropListOwner;

		internal MxScheduler _owner;

		private bool _isRecapturePending;

		private Timer _captureTimer;

		internal int _firstVisibleIndex;

		private ArrayList _owners;

		private SchedulerAppointmentOwner _schedulerAppointmentOwner;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.ExStyle |= 128;
				return createParams;
			}
		}

		internal SchedulerFormatStyleScrollableBase()
		{
			UpdateContainer();
			SetHeaderHeight(0);
			base.BorderStyle = BorderStyle.Flat;
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.UserPaint, value: true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
			SetStyle(ControlStyles.Selectable, value: true);
			AllowDrop = true;
		}

		protected override void OnScrollBarScroll(int scrollCode)
		{
			_firstVisibleIndex = GetVerticalScrollBarControl().GetScrollValue();
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs eventArgs)
		{
			base.OnMouseUp(eventArgs);
			if (base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y) && eventArgs.Button == MouseButtons.Left)
			{
				HideDropList(false);
			}
			if (base.Visible && _isDropListShown)
			{
				_captureTimer = new Timer();
				_captureTimer.Interval = 20;
				_captureTimer.Tick += OnTimerTick;
				_captureTimer.Enabled = true;
			}
		}

		private void OnTimerTick(object sender, EventArgs eventArgs)
		{
			if (base.Visible)
			{
				SchedulerRenderer.SetCapture(this, true);
			}
			_captureTimer.Dispose();
			_captureTimer = null;
		}

		protected override void OnMouseMove(MouseEventArgs eventArgs)
		{
			base.OnMouseMove(eventArgs);
			UpdateSchedulerAppointmentOwner(GetOwnerAtPoint(eventArgs.X, eventArgs.Y));
		}

		private SchedulerAppointmentOwner GetOwnerAtPoint(int x, int integerArgument)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Contains(x, integerArgument))
			{
				int listTop = clientRectangle.Top - _firstVisibleIndex * _itemHeight;
				int offsetY = integerArgument - listTop;
				int index = offsetY / _itemHeight;
				return (SchedulerAppointmentOwner)_owners[index];
			}
			return null;
		}

		protected override void OnDragOver(DragEventArgs eventArgs)
		{
			base.OnDragOver(eventArgs);
			Point point = PointToClient(new Point(eventArgs.X, eventArgs.Y));
			SchedulerAppointmentOwner schedulerAppointmentOwner = GetOwnerAtPoint(point.X, point.Y);
			if (point.Y <= _itemHeight && _firstVisibleIndex > 0)
			{
				_isEdgeScrolling = true;
				_isScrollingUp = true;
				if (_dragTimer == null)
				{
					_isEdgeScrolling = true;
					_isScrollingUp = true;
					_dragTimer = new Timer();
					_dragTimer.Tick += OnDragTimerTick;
					_dragTimer.Interval = 300;
					_dragTimer.Start();
				}
				return;
			}
			if (point.Y >= base.Height - _itemHeight && schedulerAppointmentOwner._index < _owners.Count - 1)
			{
				if (_dragTimer == null)
				{
					_isEdgeScrolling = true;
					_isScrollingUp = false;
					_dragTimer = new Timer();
					_dragTimer.Tick += OnDragTimerTick;
					_dragTimer.Interval = 300;
					_dragTimer.Start();
				}
				return;
			}
			_isEdgeScrolling = false;
			if (schedulerAppointmentOwner != GetSchedulerAppointmentOwner())
			{
				DisposeResources();
				UpdateSchedulerAppointmentOwner(schedulerAppointmentOwner);
			}
			else if (_dragTimer == null)
			{
				_dragTimer = new Timer();
				_dragTimer.Tick += OnDragTimerTick;
				_dragTimer.Interval = 500;
				_dragTimer.Start();
			}
			eventArgs.Effect = DragDropEffects.Move;
		}

		protected override void OnDragDrop(DragEventArgs eventArgs)
		{
			base.OnDragDrop(eventArgs);
			DisposeResources();
			if (eventArgs.Data.GetDataPresent(typeof(SchedulerAppointment)) || eventArgs.Data.GetDataPresent(typeof(AppointmentDragHelper)))
			{
				HideDropList(false);
			}
		}

		protected override void OnMouseDown(MouseEventArgs eventArgs)
		{
			base.OnMouseDown(eventArgs);
			if (_isDropListShown && !base.ClientRectangle.Contains(eventArgs.X, eventArgs.Y))
			{
				if (GetVerticalScrollBarControl().Bounds.Contains(eventArgs.X, eventArgs.Y))
				{
					Point point = GetVerticalScrollBarControl().PointToClient(Cursor.Position);
					GetVerticalScrollBarControl().ForwardMouseDown(eventArgs, point.X, point.Y);
				}
				else
				{
					SchedulerRenderer.SetCapture(this, false);
					HideDropList(true);
				}
			}
		}

		private void OnControlMouseUp(object sender, MouseEventArgs eventArgs)
		{
			if (_isDropListShown)
			{
				SchedulerRenderer.SetCapture(this, true);
			}
		}

		internal void ShowDropList(Rectangle rectangle, object value, ArrayList arrayList, int schedulerFormatStyleScrollableBaseInteger)
		{
			_firstVisibleIndex = 0;
			GetVerticalScrollBarControl().UpdateScrollValue(0);
			_isDropListShown = true;
			_itemHeight = schedulerFormatStyleScrollableBaseInteger;
			SetOwners(arrayList);
			_dropListOwner = (IDropListOwner)value;
			_intPtr = NativeMethods.SetParent(base.Handle, IntPtr.Zero);
			NativeMethods.SetWindowPos(base.Handle, (IntPtr)(-1), 0, 0, 0, 0, 67);
			int left = rectangle.Left;
			int top = rectangle.Top;
			Rectangle virtualScreen = SystemInformation.VirtualScreen;
			base.Size = rectangle.Size;
			if (left + base.Width > virtualScreen.Right)
			{
				left = virtualScreen.Right - base.Width;
			}
			if (left < 0)
			{
				left = 0;
			}
			if (top + base.Height > virtualScreen.Bottom)
			{
				top = rectangle.Top - base.Height - _itemHeight;
			}
			base.Location = new Point(left, top);
			base.Visible = true;
		}

		private void OnDragTimerTick(object sender, EventArgs eventArgs)
		{
			if (!_isEdgeScrolling)
			{
				if (_owner != null && _schedulerAppointmentOwner != null)
				{
					_owner.CurrentOwner = _schedulerAppointmentOwner;
				}
			}
			else if (_isScrollingUp)
			{
				StyledScrollBar scrollBar = GetVerticalScrollBarControl();
				scrollBar.UpdateScrollValue(scrollBar.GetScrollValue() - 1);
			}
			else
			{
				StyledScrollBar scrollBarStyleControl2Argument = GetVerticalScrollBarControl();
				scrollBarStyleControl2Argument.UpdateScrollValue(scrollBarStyleControl2Argument.GetScrollValue() + 1);
			}
		}

		protected override void OnDragLeave(EventArgs eventArgs)
		{
			base.OnDragLeave(eventArgs);
			DisposeResources();
			HideDropList(true);
		}

		internal void DisposeResources()
		{
			_isEdgeScrolling = false;
			if (_dragTimer != null)
			{
				try
				{
					_dragTimer.Stop();
					_dragTimer.Tick -= OnDragTimerTick;
					_dragTimer.Dispose();
					_dragTimer = null;
				}
				catch
				{
				}
			}
		}

		internal void HideDropList(bool cancel)
		{
			if (_isDropListShown)
			{
				ReleaseDropList(cancel);
				_isDropListShown = false;
				if (_dropListOwner != null)
				{
					_dropListOwner.OnDropListHide(cancel);
					_dropListOwner = null;
				}
			}
		}

		internal void ReleaseDropList(bool cancel)
		{
			_isDropListShown = false;
			Hide();
			NativeMethods.SetParent(base.Handle, _intPtr);
			SchedulerRenderer.SetCapture(this, false);
			_intPtr = IntPtr.Zero;
		}

		internal bool HandleDialogKey(Keys keyData)
		{
			return ProcessDialogKey(keyData);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _container != null)
			{
				_container.Dispose();
			}
			base.Dispose(disposing);
		}

		public void SetOwners(ArrayList arrayList)
		{
			_owners = arrayList;
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			switch (keyData)
			{
			case Keys.Prior:
				if (GetVerticalScrollBarControl().Visible)
				{
					GetVerticalScrollBarControl().UpdateScrollValue(0);
				}
				UpdateSchedulerAppointmentOwner((SchedulerAppointmentOwner)_owners[0]);
				break;
			case Keys.Next:
				if (GetVerticalScrollBarControl().Visible)
				{
					GetVerticalScrollBarControl().UpdateScrollValue(GetVerticalScrollBarControl().GetMaximum());
				}
				UpdateSchedulerAppointmentOwner((SchedulerAppointmentOwner)_owners[_owners.Count - 1]);
				break;
			case Keys.Down:
				if (_schedulerAppointmentOwner != null)
				{
					if (_schedulerAppointmentOwner._index < _owners.Count - 1)
					{
						int visibleItemCount = base.Height / _itemHeight;
						UpdateSchedulerAppointmentOwner((SchedulerAppointmentOwner)_owners[_schedulerAppointmentOwner._index + 1]);
						if (GetVerticalScrollBarControl().Visible && _firstVisibleIndex + visibleItemCount <= _schedulerAppointmentOwner._index)
						{
							GetVerticalScrollBarControl().UpdateScrollValue(_schedulerAppointmentOwner._index - visibleItemCount + 1);
						}
					}
				}
				else
				{
					if (GetVerticalScrollBarControl().Visible)
					{
						GetVerticalScrollBarControl().UpdateScrollValue(0);
					}
					UpdateSchedulerAppointmentOwner((SchedulerAppointmentOwner)_owners[0]);
				}
				break;
			case Keys.Up:
				if (_schedulerAppointmentOwner != null)
				{
					if (_schedulerAppointmentOwner._index > 0)
					{
						UpdateSchedulerAppointmentOwner((SchedulerAppointmentOwner)_owners[_schedulerAppointmentOwner._index - 1]);
						if (GetVerticalScrollBarControl().Visible && _firstVisibleIndex > _schedulerAppointmentOwner.Index)
						{
							GetVerticalScrollBarControl().UpdateScrollValue(_schedulerAppointmentOwner._index);
						}
					}
				}
				else
				{
					if (GetVerticalScrollBarControl().Visible)
					{
						GetVerticalScrollBarControl().UpdateScrollValue(GetVerticalScrollBarControl().GetMaximum());
					}
					UpdateSchedulerAppointmentOwner((SchedulerAppointmentOwner)_owners[_owners.Count - 1]);
				}
				break;
			case Keys.Return:
			case Keys.F4:
			case Keys.Up | Keys.Alt:
			case Keys.Down | Keys.Alt:
				HideDropList(false);
				break;
			case Keys.Escape:
				HideDropList(true);
				break;
			default:
				HideDropList(true);
				return base.ProcessDialogKey(keyData);
			}
			return true;
		}

		internal SchedulerAppointmentOwner GetSchedulerAppointmentOwner()
		{
			return _schedulerAppointmentOwner;
		}

		internal void UpdateSchedulerAppointmentOwner(SchedulerAppointmentOwner schedulerAppointmentOwner)
		{
			if (_schedulerAppointmentOwner != schedulerAppointmentOwner)
			{
				_schedulerAppointmentOwner = schedulerAppointmentOwner;
				Invalidate();
			}
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == 533)
			{
				base.WndProc(ref msg);
				if (_isRecapturePending && msg.LParam == IntPtr.Zero && base.Visible)
				{
					SchedulerRenderer.SetCapture(this, true);
					_isRecapturePending = false;
				}
			}
			else if (msg.Msg == 31)
			{
				HideDropList(true);
				base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
			}
		}

		protected override void OnPaint(PaintEventArgs eventArgs)
		{
			TextLayoutRenderer textLayoutRenderer = new TextLayoutRenderer(!_owner.UseCompatibleTextRendering);
			textLayoutRenderer.UpdateStringAlignment(StringAlignment.Near);
			textLayoutRenderer.UpdateTextLayoutRendererStringAlignment(StringAlignment.Center);
			textLayoutRenderer.UpdateWrapsText(false);
			Brush brush = GdiCache.Brush(BackColor);
			Rectangle clientRectangle = base.ClientRectangle;
			int itemTop = clientRectangle.Top - _firstVisibleIndex * _itemHeight;
			for (int i = 0; i < _owners.Count; i++)
			{
				if (itemTop < base.ClientRectangle.Top)
				{
					itemTop += _itemHeight;
					continue;
				}
				if (itemTop >= base.ClientRectangle.Height)
				{
					break;
				}
				SchedulerAppointmentOwner schedulerAppointmentOwner = (SchedulerAppointmentOwner)_owners[i];
				Rectangle rectangle = new Rectangle(clientRectangle.Left, itemTop, clientRectangle.Width, _itemHeight);
				DrawSchedulerAreaEventArgs drawArgs = new DrawSchedulerAreaEventArgs(eventArgs.Graphics, _owner, OwnerDrawnArea.OwnerListItem, rectangle, schedulerAppointmentOwner);
				_owner.RaiseDrawSchedulerArea(drawArgs);
				if (!drawArgs.Handled)
				{
					if (_schedulerAppointmentOwner != null && schedulerAppointmentOwner == _schedulerAppointmentOwner)
					{
						DrawImage(eventArgs.Graphics, rectangle, schedulerAppointmentOwner, _schedulerFormatStyle.GetBackBrush(rectangle), _schedulerFormatStyle.ResolveForeColor(SystemColors.HighlightText), textLayoutRenderer);
					}
					else
					{
						DrawImage(eventArgs.Graphics, rectangle, schedulerAppointmentOwner, brush, ForeColor, textLayoutRenderer);
					}
				}
				itemTop += _itemHeight;
			}
			_owner.UpdateThemedRendering();
			DrawBorder(eventArgs.Graphics, _owner._themeRenderer);
			_owner.DisposeDrawingResources();
			_schedulerFormatStyle.DisposeResources();
			textLayoutRenderer.ResetFormat();
		}

		private void DrawImage(Graphics graphics, Rectangle rectArgument, SchedulerAppointmentOwner schedulerAppointmentOwner, Brush brush, Color color, TextLayoutRenderer textLayoutRenderer)
		{
			if (brush != null)
			{
				graphics.FillRectangle(brush, rectArgument);
			}
			Rectangle rectangle = rectArgument;
			rectangle.X += 5;
			Image image = null;
			if (schedulerAppointmentOwner.ImageIndex >= 0)
			{
				try
				{
					ImageList imageList = _owner.GetImageList();
					if (imageList != null && schedulerAppointmentOwner.ImageIndex < imageList.Images.Count)
					{
						image = _owner.GetImageList().Images[schedulerAppointmentOwner.ImageIndex];
					}
				}
				catch
				{
					image = null;
				}
			}
			if (image != null)
			{
				Rectangle rect = rectArgument;
				rect.X += 5;
				rect.Width = image.Width;
				int verticalOffset = (rectArgument.Height - image.Height) / 2;
				if (verticalOffset > 0)
				{
					rect.Y += verticalOffset;
				}
				graphics.DrawImageUnscaled(image, rect);
				rectangle.X = rect.Right + 5;
			}
			rectangle.Width -= rectangle.Left;
			textLayoutRenderer.DrawText(graphics, schedulerAppointmentOwner.Text, rectangle, Font, color);
		}

		private void UpdateContainer()
		{
			_container = new Container();
			GetVerticalScrollBarControl().MouseUp += OnControlMouseUp;
			SchedulerRenderer.SetAllowDrop(this, true);
		}
	}
}
